using System.Collections.Generic;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using static AvoidBlock.AvoidBlockCore;

namespace AvoidBlock.Util
{
    public class Patches
    {
        /// <summary>
        /// Patch to modify the AI behavior of AiTaskGotoEntity.
        /// Prevents entities from continuing movement if they encounter a "npccollider" block.
        /// </summary>
        [HarmonyPatch(typeof(AiTaskGotoEntity), "ContinueExecute")]
        public class AvoidBlock_Patch_1
        {
            [HarmonyPostfix]
            private static void ContinueExecute_Patch(AiTaskGotoEntity __instance, ref bool __result)
            {
                // Ensure the AI task instance is valid before proceeding
                if (__instance == null) return;

                // Access the world instance from the AI task
                IWorldAccessor world = __instance.world;

                if (world == null) return;

                Dictionary<string, string[]> entitiesAndBlocks = AvoidBlockMod.Config.EntitiesAndBlocks;

                // Get the position 1 block ahead of the entity
                BlockPos forwardPos = __instance.entity.Pos.HorizontalAheadCopy(1).AsBlockPos;

                // Retrieve the block at the forward position
                Block avoidBlock = world.BlockAccessor.GetBlock(forwardPos);

                if (Helper.MatchFound(entitiesAndBlocks, __instance.entity, avoidBlock) || Helper.MatchFound(entitiesAndBlocks, __instance.entity, world.BlockAccessor.GetBlockAbove(forwardPos)))
                {
                    __result = false;
                }
                else 
                {
                    __result = true;
                }
            }   
        }

        /// <summary>
        /// Patch to modify the AI behavior of EntityTradingHumanoid.
        /// Stops wandering behavior when the entity is interacting with a player.
        /// </summary>
        [HarmonyPatch(typeof(EntityTradingHumanoid), "OnGameTick")]
        public class AvoidBlock_Patch_2
        {
            [HarmonyPostfix]
            private static void OnGameTick_Patch(EntityTradingHumanoid __instance, float dt)
            {
                // Ensure the entity instance is valid before proceeding
                if (__instance == null) return;

                // Get the API instance from the entity
                ICoreAPI api = __instance.Api;

                // Variable to store the entity's AI behavior
                EntityBehaviorTaskAI behavior = null;

                // Check if we are on the server side before modifying AI behavior
                if (api.Side == EnumAppSide.Server)
                {
                    // Retrieve the AI behavior component from the entity
                    behavior = __instance.GetBehavior<EntityBehaviorTaskAI>();
                }

                // If the entity is currently trading with a player, stop wandering AI task
                if (__instance.tradingWithPlayer != null && behavior != null)
                {
                    behavior.TaskManager.StopTask(typeof(AiTaskWander));
                }
            }
        }
    }
}