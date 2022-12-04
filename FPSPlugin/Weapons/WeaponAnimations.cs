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
/// 
/// TODO: Make it work with ping compensation
/// </summary>
internal static class WeaponAnimsHandler
{
    static BufferedBlockSender sender;
    static Level level;

    static Dictionary<int, BlockID> blockSenderCache;   // Using a dictionary cache has a few benefits, including preventing duplicate writes

    internal static void Activate()
    {
        sender = new BufferedBlockSender(FPSMOGame.Instance.map);
        level = FPSMOGame.Instance.map;
        blockSenderCache = new Dictionary<int, BlockID>();
    }

    internal static void Deactivate()
    {
        sender = null;
        level = null;
        blockSenderCache = null;
    }

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

    internal static void Undraw(List<WeaponEntity> weList,bool currentTick)
    {
        foreach (WeaponEntity we in weList)
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

    internal static void Flush()
    {
        foreach (Player p in FPSMOGame.Instance.players.Values)
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
