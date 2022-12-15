/*
Copyright 2022 WOCC Team

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files
(the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge,
publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using MCGalaxy;
using MCGalaxy.Blocks;
using MCGalaxy.Maths;
using MCGalaxy.Network;
using MCGalaxy.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlockID = System.UInt16;

namespace FPS.Weapons;

/// <summary>
/// This class handles all weapon animations in the map
/// Instead of creating a schedulertask for each time a weapon gets fired (bad idea) we have one running continuously
/// It can also send block changes for all weapon entities in bulk, which is considerably more efficient
/// </summary>
internal static class WeaponAnimsHandler
{
    static BufferedBlockSender sender;
    static Level level;

    static Dictionary<int, BlockID> blockSenderCache;   // Using a dictionary cache has a few benefits, including preventing duplicate writes

    /// <summary>
    /// Prepares the animation handler for sending blocks
    /// </summary>
    internal static void Activate()
    {
        sender = new BufferedBlockSender(FPSGame.Instance.Map);
        level = FPSGame.Instance.Map;   // Cached for efficiency
        blockSenderCache = new Dictionary<int, BlockID>();
    }

    /// <summary>
    /// Deactivate the animation handler
    /// </summary>
    internal static void Deactivate()
    {
        sender = null;
        level = null;
        blockSenderCache = null;
    }

    /// <summary>
    /// Draws a list of weapon entities at a given tick to the current FPS map
    /// Does not display them yet. Need to flush the blockSender cache to truly send the changes
    /// </summary>
    /// <param name="entities">Weapon entities</param>
    /// <param name="currentTick">The current animation tick</param>
    internal static void Draw(List<WeaponEntity> entities, bool currentTick)
    {
        foreach (WeaponEntity we in entities)
        {
            if (currentTick)
            {
                foreach (WeaponBlock wb in we.currentBlocks)
                {
                    blockSenderCache[level.PosToInt(wb.x, wb.y, wb.z)] = wb.block;
                }
            } else
            {
                foreach (WeaponBlock wb in we.lastBlocks)
                {
                    blockSenderCache[level.PosToInt(wb.x, wb.y, wb.z)] = wb.block;
                }
            }
        }
    }

    /// <summary>
    /// Undraws a list of weapon entities at a given tick to the current FPS map
    /// Does not undraw them in view. Need to flush the blockSender cache to see the fully effect
    /// Note: undrawing means sending the original block inside that map
    /// </summary>
    /// <param name="entities">Weapon entities</param>
    /// <param name="currentTick">The current animation tick</param>
    internal static void Undraw(List<WeaponEntity> entities,bool currentTick)
    {
        foreach (WeaponEntity we in entities)
        {
            if (currentTick)
            {
                foreach (WeaponBlock wb in we.currentBlocks)
                {
                    blockSenderCache[level.PosToInt(wb.x, wb.y, wb.z)] = level.GetBlock(wb.x, wb.y, wb.z);
                }
            } else
            {
                foreach (WeaponBlock wb in we.lastBlocks)
                {
                    blockSenderCache[level.PosToInt(wb.x, wb.y, wb.z)] = level.GetBlock(wb.x, wb.y, wb.z);
                }
            }
        }
    }

    /// <summary>
    /// Sends visual changes to the FPS map
    /// </summary>
    internal static void Flush()
    {
        foreach (Player p in FPSGame.Instance.Players.Values)
        {
            sender = new BufferedBlockSender(p);
            foreach (var kvp in blockSenderCache)
            {
                sender.Add(kvp.Key, kvp.Value);
            }
            if (sender.count > 0) sender.Flush();
        }
        blockSenderCache = new Dictionary<int, BlockID>();
    }
}
