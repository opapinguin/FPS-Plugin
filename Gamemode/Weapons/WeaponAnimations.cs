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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCGalaxy;
using MCGalaxy.Maths;
using MCGalaxy.Network;
using MCGalaxy.Tasks;
using BlockID = System.UInt16;

namespace FPSMO.Weapons
{
    /*****************************
     * Animation Data Structures *
     *****************************/
    #region Data Structures
    public struct AnimBlock {
        public AnimBlock(Vec3U16 loc, BlockID type)
        {
            x = loc.X;
            y = loc.Y;
            z = loc.Z;
            block = type;
        }
        public ushort x;
        public ushort y;
        public ushort z;
        public BlockID block;
    }

    /// <summary>
    /// An interface for different types of weapon animations
    /// </summary>
    public abstract class Animation
    {
        protected DateTime start;
        public abstract List<AnimBlock> GetCurrentBlocks(Player p, DateTime t);
    }

    public class ProjectileAnimation : Animation
    {
        public delegate Vec3F32 LocAt(DateTime t);  // Location at delegates the parametric function of our choice. Assumes a 1D path
        LocAt locationCallback;
        BlockID block;
        DateTime startTime;
        Player shooter;
        public ProjectileAnimation(Player p, DateTime start, BlockID b, LocAt ft)
        {
            shooter = p;
            startTime = start;
            block = b;
            locationCallback = ft;
        }

        public override List<AnimBlock> GetCurrentBlocks(Player p, DateTime t)  // TODO: Probably want to return a few blocks
        {
            Vec3F32 loc = locationCallback(DateTime.Now);
            Vec3U16 locU16 = new Vec3U16((ushort)loc.X, (ushort)loc.Y, (ushort)loc.Z);

            AnimBlock ab = new AnimBlock(locU16, block);

            List<AnimBlock> animBlocks = new List<AnimBlock>
            {
                ab
            };

            return animBlocks;
        }
    }

    #endregion

    /*********************
     * ANIMATION HANDLER *
     *********************/
    #region Animation Handler

    /// <summary>
    /// This class handles all weapon animations in the map
    /// Instead of creating a schedulertask for each time a weapon gets fired (bad idea) we have one running continuously
    /// It can also send block changes for all weapons in bulk, which is considerably more efficient
    /// 
    /// TODO: Make it work with ping compensation
    /// </summary>
    public static class WeaponAnimsHandler
    {
        static int DEFAULT_TICK_SPEED = 50;  // 50 milliseconds
        static SchedulerTask task;
        static Scheduler instance;
        static readonly object activateLock = new object();
        static readonly object deactivateLock = new object();
        static BufferedBlockSender buffer;

        static List<BlockID> prevBlocks = new List<BlockID>();
        static List<BlockID> currentBlocks = new List<BlockID>();

        static bool writeLock = false;
        static List<Animation> weaponAnimations = new List<Animation>();

        internal static void Activate()
        {
            lock (activateLock)                 // Thread safety
            {
                if (instance != null) return;   // Singleton boilerplate
                instance = new Scheduler("WeaponAnimationsScheduler");
                task = instance.QueueRepeat(Update, FPSMOGame.Instance, TimeSpan.FromMilliseconds(DEFAULT_TICK_SPEED));
            }
            buffer = new BufferedBlockSender(FPSMOGame.Instance.map);
        }

        internal static void Deactivate()
        {
            lock (deactivateLock)
            {
                if (instance != null)
                {
                    instance.Cancel(task);
                }
                buffer = null;
            }
        }

        internal static void AddAnimation(Animation anim)
        {
            writeLock = true;
            weaponAnimations.Add(anim);
            writeLock = false;
        }

        private static void Update(SchedulerTask task)
        {
            prevBlocks = currentBlocks;

            DateTime now = DateTime.Now;
            foreach (Animation anim in weaponAnimations)
            {
                //currentBlocks.AddRange(anim.GetCurrentBlocks(now));
            }


        }

        internal static void SendCurrentFrame()
        {
            BufferedBlockSender sender = new BufferedBlockSender(FPSMOGame.Instance.map);
            


        }

    }
    #endregion
}
