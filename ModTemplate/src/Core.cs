using Vintagestory.API.Common;
using HarmonyLib;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using AvoidBlock.Entity.AiTask;
using System.Linq;
using System;
using Vintagestory.API.Util;

namespace AvoidBlock
{
    class AvoidBlockCore : ModSystem
    {
        public class ServerConfig
        {
            public string Entities { get; set; } = "@.*(trader).*";
        }

        public class AvoidBlockMod : ModSystem
        {
            private Harmony harmony;

            private ILogger logger;

            internal static readonly string ConfigName = "EntitiesList.json";

            public static ServerConfig Config;

            public override void Start(ICoreAPI api)
            {
                base.Start(api);

                //Server side only.
                if (api.Side == EnumAppSide.Server) 
                {
                    logger = api.Logger;

                    try
                    {
                        // Attempt to load the configuration file
                        Config = api.LoadModConfig<ServerConfig>(ConfigName);

                        if (Config == null)
                        {
                            // Create a new config if not found
                            Config = new ServerConfig();
                            logger.VerboseDebug($"Config file '{ConfigName}' not found. Creating a new one with default values...");
                            api.StoreModConfig(Config, ConfigName);
                        }
                        else
                        {
                            // Store the loaded config back to ensure all default values are present
                            api.StoreModConfig(Config, ConfigName);
                            logger.VerboseDebug($"Config file '{ConfigName}' loaded successfully.");
                        }
                    }
                    catch (Exception e)
                    {
                        // Log detailed error information and create a fallback config
                        logger.Error($"Failed to load config file '{ConfigName}'. Error: {e.Message}");
                        Config = new ServerConfig();
                        api.StoreModConfig(Config, ConfigName);
                        logger.Error("A new config file with default values has been created.");
                    }
                }
            }

            public override void StartServerSide(ICoreServerAPI api)
            {
                base.StartServerSide(api);

                logger = api.Logger;

                // Register AI task
                AiTaskRegistry.Register<AiTaskAvoidBlock>("avoidblock");

                api.Event.OnEntitySpawn += AppliedAiTask;
                api.Event.OnEntityLoaded += AppliedAiTask;

                // Initialize Harmony patching
                harmony = new Harmony("avoidblock-harmony");

                harmony.PatchAll();

                logger?.Notification("AvoidBlockMod: Harmony patches applied.");


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
                        bool value = WildcardUtil.Match(Config.Entities, entity.Code.ToString());

                        if (value)
                        {
                            // Check if the task already exists to avoid duplication
                            if (!taskAI.TaskManager.AllTasks.Any(t => t is AiTaskAvoidBlock))
                            {
                                // Add the custom AI task to the entity's task manager
                                AiTaskAvoidBlock avoidBlockTask = new AiTaskAvoidBlock(taskAI.entity as EntityAgent) { Priority = 4f };

                                taskAI.TaskManager.AddTask(avoidBlockTask);

                                entity.Api.Logger.Notification($"Added AvoidBlock AI task to {entity.Code}");
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
                    logger?.Notification("AvoidBlockMod: Harmony patches removed.");
                }

                base.Dispose();
            }
        }
    }
}
