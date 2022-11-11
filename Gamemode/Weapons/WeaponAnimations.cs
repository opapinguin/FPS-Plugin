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
    public struct AnimBlock : IEqualityComparer<AnimBlock> {
        public AnimBlock(Vec3U16 loc, BlockID block)
        {
            x = loc.X;
            y = loc.Y;
            z = loc.Z;
            this.block = block;
        }
        public System.UInt16 x;
        public System.UInt16 y;
        public System.UInt16 z;
        public BlockID block;

        public bool Equals(AnimBlock a, AnimBlock b)
        {
            return (a.x == b.x && a.y == b.y && a.z == b.z && a.block == b.block);
        }

        public int GetHashCode(AnimBlock obj)
        {
            return obj.GetHashCode();
        }
    }

    /// <summary>
    /// An interface for different types of weapon animations
    /// </summary>
    public abstract class Animation
    {
        protected Player shooter;
        protected DateTime start;
        /// <summary>
        /// Gets the current blocks at a specific animation tick
        /// </summary>
        public abstract List<AnimBlock> GetCurrentBlocks(float tick);

        /// <summary>
        /// Gets the current blocks in a range of ticks. Interpolates your blocks
        /// </summary>
        public virtual List<AnimBlock> GetCurrentBlocksInterpolate(float tickStart, float tickEnd, bool interpolate=true, uint maxDepth=0)
        {
            List<AnimBlock> result = new List<AnimBlock>();
            if (Enumerable.SequenceEqual(GetCurrentBlocks(tickStart), GetCurrentBlocks(tickEnd)) || maxDepth >= 5)  // maxDepth just in case
            {
                return GetCurrentBlocks(tickStart);
            }

            // TODO: If interpolate consider also the length of the gun itself. Or maybe remove the bool and just consider it

            // Since we do not have easy access to the inverse of our locat functions, we employ a sort of binary search
            result.AddRange(GetCurrentBlocksInterpolate(tickStart, (tickStart + tickEnd) / 2, interpolate, maxDepth + 1));
            result.AddRange(GetCurrentBlocksInterpolate((tickStart + tickEnd) / 2, tickEnd, interpolate, maxDepth + 1));
            return result;
        }
    }

    public class ProjectileAnimation : Animation
    {
        public delegate Vec3F32 LocAt(float tick, Position orig, Orientation rot, uint fireTime);  // Location at delegates the parametric function of our choice. Assumes a 1D path

        private LocAt locAt;
        private BlockID block;
        private uint fireTimeTick;
        private Position origin;
        private Orientation rotation;
        public ProjectileAnimation(Player p, uint start, BlockID b, Position orig, Orientation rot, LocAt ft)
        {
            shooter = p;
            fireTimeTick = start;
            block = b;
            locAt = ft;
            origin = orig;
            rotation = rot;

            WeaponAnimsHandler.AddAnimation(this);
        }

        public override List<AnimBlock> GetCurrentBlocks(float tick)  // TODO: Probably want to return a list of blocks, like a short line
        {
            Vec3F32 loc = locAt(tick, origin, rotation, fireTimeTick);     // To add a short line get the location at a number of different times
            Vec3U16 locU16 = new Vec3U16((ushort)(loc.X / 32), (ushort)(loc.Y / 32), (ushort)(loc.Z / 32));

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
        static SchedulerTask task;
        static Scheduler instance;
        static readonly object activateLock = new object();
        static readonly object deactivateLock = new object();
        static BufferedBlockSender sender;
        static List<Animation> weaponAnimations = new List<Animation>();
        static bool activated;
        static uint currentTick;       // Why not 0? Fixes issue with all weapon startTicks being 0, given a long reloadTime
        static uint MSRoundTick;

        internal static void Activate()
        {
            MSRoundTick = FPSMOGame.Instance.gameConfig.MS_ROUND_TICK;
            lock (activateLock)                 // Thread safety
            {
                if (instance != null) return;   // Singleton boilerplate
                instance = new Scheduler("WeaponAnimationsScheduler");
                task = instance.QueueRepeat(Update, FPSMOGame.Instance, TimeSpan.FromMilliseconds(MSRoundTick));
                activated = true;
            }
            currentTick = 10;
            sender = new BufferedBlockSender(FPSMOGame.Instance.map);
        }

        internal static uint Tick { get { return currentTick; } }

        internal static void Deactivate()
        {
            lock (deactivateLock)
            {
                if (instance != null)
                {
                    instance.Cancel(task);
                    instance = null;
                }
                sender = null;
                activated = false;
                weaponAnimations = new List<Animation>();
            }
            currentTick = 10;
        }

        internal static void AddAnimation(Animation anim)
        {
            if (activated) { weaponAnimations.Add(anim); }
        }

        internal static void RemoveAnimation(Animation anim)
        {
            weaponAnimations.Remove(anim);
        }

        private static void Update(SchedulerTask task)
        {
            foreach (Player p in FPSMOGame.Instance.players.Values)
            {
                SendCurrentFrame(p);
            }
            currentTick++;
        }

        internal static void SendCurrentFrame(Player p)
        {
            if (!activated) return;
            sender = new BufferedBlockSender(p);
            
            for (int i = 0; i < weaponAnimations.Count; i++)    // Foreach wouldn't be thread-safe with the constant deletions
            {
                Animation anim = weaponAnimations[i];

                foreach (AnimBlock ab in anim.GetCurrentBlocksInterpolate(currentTick, currentTick + 1))
                {
                    sender.Add(p.level.PosToInt(ab.x, ab.y, ab.z), ab.block); // TODO: could have overlapping block changes, but I think that's fine
                }
            }

            if (sender.count < 16)  // TODO: Benchmark this
            {
                // TODO: Send current blocks the simple way
                //return;
            }

            if (sender.count > 0)
            {
                sender.Flush();
            }
        }

    }
    #endregion
}
