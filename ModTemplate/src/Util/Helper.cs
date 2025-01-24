using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace AvoidBlock.Util
{
    public class Helper
    {
        public static bool MatchFound(Dictionary<string, string[]> entitiesAndBlocks, EntityAgent entity, Block blockToCheck)
        {
            if (blockToCheck == null) return false;

            foreach (var types in entitiesAndBlocks)
            {
                // Match entity using regex pattern
                if (WildcardUtil.Match(types.Key, entity.Code.ToString()))
                {
                    foreach (string blockPattern in types.Value)
                    {
                        // Match block against the stored patterns
                        if (blockToCheck != null && WildcardUtil.Match(blockPattern, blockToCheck.Code.ToString()))
                        {
                            return true;  // Block should be avoided
                        }
                    }
                }
            }

            return false;
        }
    }
}
