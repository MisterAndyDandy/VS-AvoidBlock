using Vintagestory.API.Common;
using HarmonyLib;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using AvoidBlock.Entity.AiTask;
using System.Linq;
using System;
using Vintagestory.API.Util;
using System.Collections.Generic;
using Vintagestory.API.Datastructures;

namespace AvoidBlock
{
    class AvoidBlockCore : ModSystem
    {
        public class ServerConfig
        {
            public bool DebugLogger { get; set; } = false;

            // Dictionary to store various string arrays with meaningful keys
            public Dictionary<string, string[]> EntitiesAndBlocks { get; set; } = new Dictionary<string, string[]>
            {
                { 
                    "@.*(trader).*", new string[] { "@.*(meta-barrier|fence).*" } 
                }
            };
        }

        public class AvoidBlockMod : ModSystem
        {
            private Harmony harmony;

            private ILogger Logger;

            internal static readonly string ConfigName = "EntitiesList.json";

            public static ServerConfig Config;

            public override void Start(ICoreAPI api)
            {
                base.Start(api);

                //Server side only.
                if (api.Side == EnumAppSide.Server) 
                {
                    Logger = api.Logger;

                    try
                    {
                        // Attempt to load the configuration file
                        Config = api.LoadModConfig<ServerConfig>(ConfigName);

                        if (Config == null)
                        {
                            // Create a new config if not found
                            Config = new ServerConfig();
                            Logger.VerboseDebug($"Config file '{ConfigName}' not found. Creating a new one with default values...");
                            api.StoreModConfig(Config, ConfigName);
                        }
                        else
                        {
                            // Store the loaded config back to ensure all default values are present
                            api.StoreModConfig(Config, ConfigName);
                            Logger.VerboseDebug($"Config file '{ConfigName}' loaded successfully.");
                        }
                    }
                    catch (Exception e)
                    {
                        // Log detailed error information and create a fallback config
                        Logger.Error($"Failed to load config file '{ConfigName}'. Error: {e.Message}");
                        Config = new ServerConfig();
                        api.StoreModConfig(Config, ConfigName);
                        Logger.Error("A new config file with default values has been created.");
                    }
                }
            }

            public override void StartServerSide(ICoreServerAPI api)
            {
                base.StartServerSide(api);

                Logger = api.Logger;

                // Register AI task
                AiTaskRegistry.Register<AiTaskAvoidBlock>("avoidblock");

                api.Event.OnEntitySpawn += AppliedAiTask;
                api.Event.OnEntityLoaded += AppliedAiTask;

                // Initialize Harmony patching
                harmony = new Harmony("avoidblock-harmony");

                harmony.PatchAll();

                DebugLogger("AvoidBlockMod: Harmony patches applied.");

            }

            /// <summary>
            /// Used to apply to entities that are listed in config.
            /// </summary>
            /// <param name="entity"></param>
            public void AppliedAiTask(Vintagestory.API.Common.Entities.Entity entity)
            {
                if (entity == null) return;

                // Find the AI behavior from the entity
                if (entity?.HasBehavior<EntityBehaviorTaskAI>() ?? false)
                {
                    EntityBehaviorTaskAI taskAI = entity.GetBehavior<EntityBehaviorTaskAI>();

                    if (taskAI != null)
                    {
                        string entitycode = string.Empty;

                        foreach (var code in Config.EntitiesAndBlocks.Keys) 
                        {
                            if (string.IsNullOrEmpty(code)) break;

                            entitycode = code;
                        }

                        bool value = WildcardUtil.Match(entitycode, entity.Code.ToString());

                        if (value)
                        {
                            // Check if the task already exists to avoid duplication
                            if (!taskAI.TaskManager.AllTasks.Any(t => t is AiTaskAvoidBlock))
                            {
                                // Add the custom AI task to the entity's task manager
                                AiTaskAvoidBlock avoidBlockTask = new AiTaskAvoidBlock(taskAI.entity as EntityAgent) { Priority = 4f };

                                taskAI.TaskManager.AddTask(avoidBlockTask);

                                DebugLogger($"Added AvoidBlock AI task to {entity.Code}");
                            }
                        }

                    }

                }
            }

            public override void Dispose()
            {
                if (harmony != null)
                {
                    harmony.UnpatchAll(harmony.Id);

                    DebugLogger("AvoidBlockMod: Harmony patches removed.");
                }

                base.Dispose();
            }

            public void DebugLogger(string text) 
            {
                if (Config.DebugLogger) return;

                Logger?.Notification(text);
            }
        }
    }
}
