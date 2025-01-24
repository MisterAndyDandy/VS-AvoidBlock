using System.IO;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using System.Collections.Generic;
using AvoidBlock.Util;
using static AvoidBlock.AvoidBlockCore;
using Vintagestory.GameContent;

namespace AvoidBlock.Entity.AiTask
{
    public class AiTaskAvoidBlock : AiTaskBase
    {
        //Entity has avoid block.
        public bool isAvoidBlock { get; set; }

        //Entity pos
        public EntityPos entityPos { get; set; }

        //Block pos for block
        public BlockPos avoidBlockPos { get; set; }

        //Block it self.
        public Block avoidBlock { get; set; }

        // Constructor for the AI task, passing the entity to the base class
        public AiTaskAvoidBlock(EntityAgent entity) : base(entity)
        {
           this.entityPos = entity.Pos as EntityPos;
           this.avoidBlockPos = entity.Pos.AsBlockPos as BlockPos;
        }

        // Determines whether the AI should start executing the task
        public override bool ShouldExecute()
        {
            Dictionary<string, string[]> entitiesAndBlocks = AvoidBlockMod.Config.EntitiesAndBlocks;

            // Get the block position 1 step ahead of the entity
            avoidBlockPos = entityPos.HorizontalAheadCopy(1).AsBlockPos;

            // Retrieve the block at the forward position
            avoidBlock = world.BlockAccessor.GetBlock(avoidBlockPos);

            bool value1 = Helper.MatchFound(entitiesAndBlocks, entity, avoidBlock);

            bool value2 = Helper.MatchFound(entitiesAndBlocks, entity, world.BlockAccessor.GetBlockAbove(avoidBlockPos));

            return value1 || value2;
            
        }

        // Called when the AI task starts executing
        public override void StartExecute()
        {
            Vec3d avoidDirection = FindSafeDirection();

            isAvoidBlock = false;

            pathTraverser.WalkTowards(avoidDirection, 0.5f, 1f, GetOnGoalReached, OnStuck);
        }

        private void OnStuck()
        {
            isAvoidBlock = true;
        }

        private void GetOnGoalReached()
        {
            isAvoidBlock = true;
        }

        private Vec3d FindSafeDirection()
        {
            // Directions to check for safe movement
            Vec3d[] possibleDirections = new Vec3d[]
            {
                entity.ServerPos.BehindCopy(1).XYZ
            };

            foreach (Vec3d direction in possibleDirections)
            {
                BlockPos targetPos = direction.AsBlockPos;

                Block targetBlock = world.BlockAccessor.GetBlock(targetPos);

                // Check if the block is passable (not solid)
                if (targetBlock.IsReplacableBy(entity.World.BlockAccessor.GetBlock(targetPos)) || targetBlock.Id == 0)
                {
                    return direction;  // Found a safe spot to move
                }
            }

            return entity.ServerPos.XYZ;  // No safe space found, stay in place
        }

        public override bool CanContinueExecute()
        {
            return pathTraverser.Ready;
        }

        public override bool ContinueExecute(float dt)
        {
            return isAvoidBlock;
        }

        // Called when the AI task finishes execution, either completed or canceled
        public override void FinishExecute(bool cancelled)
        {
            // Stop pathfinding when the task is done
            pathTraverser.Stop();
        }
    }
}