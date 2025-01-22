using Vintagestory.API.Common;
using HarmonyLib;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using AvoidBlock.Entity.AiTask;

namespace AvoidBlock
{
    class AvoidBlockCore : ModSystem
    {
        public class AvoidBlockMod : ModSystem
        {
            private Harmony harmony;

            private ILogger logger;

            public override void StartServerSide(ICoreServerAPI api)
            {
                base.StartServerSide(api);

                logger = api.Logger;

                // Register AI task
                AiTaskRegistry.Register<AiTaskAvoidBlock>("avoidblock");

                // Initialize Harmony patching
                harmony = new Harmony("avoidblock-harmony");

                harmony.PatchAll();

                logger?.Notification("AvoidBlockMod: Harmony patches applied.");
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
