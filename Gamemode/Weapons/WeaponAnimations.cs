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

namespace FPSMO.Weapons
{
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

        public static void Activate()
        {
            sender = new BufferedBlockSender(FPSMOGame.Instance.map);
        }

        public static void Deactivate()
        {
            sender = null;
        }

        public static void Update(List<WeaponEntity> entities, List<WeaponEntity> collidingEntities)
        {
            foreach (Player p in FPSMOGame.Instance.players.Values)
            {
                sender = new BufferedBlockSender(p);    // TODO: Could this be per level instead of per player?
                SendCurrentFrame(p, entities);
                RemoveCollidingBlocks(p, collidingEntities);
            }
        }

        private static void SendCurrentFrame(Player p, List<WeaponEntity> entities)
        {
            foreach (WeaponEntity we in entities)
            {
                // Out with the old frame
                foreach (WeaponBlock wb in we.lastBlocks)
                {
                    sender.Add(p.level.PosToInt(wb.x, wb.y, wb.z), p.level.GetBlock(wb.x, wb.y, wb.z));
                }

                // In with the new frame
                foreach (WeaponBlock wb in we.currentBlocks)
                {
                    sender.Add(p.level.PosToInt(wb.x, wb.y, wb.z), wb.block);
                }
            }

            // TODO: Down here work with static animations later

            //if (sender.count < 16)  // TODO: Benchmark this
            //{
            //    // TODO: Send current blocks the simple way
            //    //return;
            //}

            if (sender.count > 0)
            {
                sender.Flush();
            }
        }

        private static void RemoveCollidingBlocks(Player p, List<WeaponEntity> collidingEntities)
        {
            foreach (WeaponEntity we in collidingEntities)
            {
                foreach (WeaponBlock wb in we.currentBlocks)
                {
                    sender.Add(p.level.PosToInt(wb.x, wb.y, wb.z), wb.block);
                }
            }
        }
    }
}
