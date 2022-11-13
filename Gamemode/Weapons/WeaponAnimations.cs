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
using MCGalaxy.Blocks;
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
    internal struct AnimBlock : IEqualityComparer<AnimBlock> {
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
    internal abstract class Animation
    {
        public Player shooter;
        protected uint fireTimeTick;
        public float frameLength;   // Number of frames shown at once

        // TODO: Add a doAnimationTick

        // Be clever and store the current and last blocks inside the animation

        // That way owner information cna be contained in there too




        /// <summary>
        /// Gets the current blocks at a specific animation tick
        /// </summary>
        public abstract List<AnimBlock> GetCurrentBlocks(float tick);

        /// <summary>
        /// Gets the current blocks in a range of ticks. Interpolates your blocks
        /// </summary>
        public virtual List<AnimBlock> GetCurrentBlocksInterpolate(float tickStart, float tickEnd, bool interpolate=true, uint depth=0)
        {
            List<AnimBlock> result = new List<AnimBlock>();
            if (Enumerable.SequenceEqual(GetCurrentBlocks(tickStart), GetCurrentBlocks(tickEnd)) || depth >= 5)  // maxDepth just in case
            {
                return GetCurrentBlocks(tickStart);
            }

            // Since we do not have easy access to the inverse of our locat functions, we employ a sort of binary search
            result.AddRange(GetCurrentBlocksInterpolate(tickStart, (tickStart + tickEnd) / 2, interpolate, depth + 1));
            result.AddRange(GetCurrentBlocksInterpolate((tickStart + tickEnd) / 2, tickEnd, interpolate, depth + 1));
            return result;
        }
    }

    internal class ProjectileAnimation : Animation
    {
        public delegate Vec3F32 LocAt(float tick, Position orig, Orientation rot, uint fireTime, uint speed);  // Location at delegates the parametric function of our choice. Assumes a 1D path

        private readonly LocAt locAt;
        private readonly BlockID block;
        private Position origin;
        private Orientation rotation;
        private readonly uint weaponSpeed;

        public ProjectileAnimation(Player p, uint start, BlockID b, Position orig, Orientation rot, float fl, uint ws, LocAt ft)
        {
            shooter = p;
            fireTimeTick = start;
            block = b;
            locAt = ft;
            origin = orig;
            rotation = rot;
            frameLength = fl;
            weaponSpeed = ws;

            WeaponAnimsHandler.AddAnimation(this);
        }

        public override List<AnimBlock> GetCurrentBlocks(float tick)  // TODO: Probably want to return a list of blocks, like a short line
        {
            Vec3F32 loc = locAt(tick, origin, rotation, fireTimeTick, weaponSpeed);
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
    internal static class WeaponAnimsHandler
    {
        static SchedulerTask task;
        static Scheduler instance;
        static readonly object activateLock = new object();
        static readonly object deactivateLock = new object();
        static BufferedBlockSender sender;
        static List<Animation> weaponAnimations = new List<Animation>();
        static bool activated;
        static uint currentTick;       // Why not 0? Fixes issue with all weapon startTicks being 0, given a long reloadTime
        static uint MSTick;

        static List<AnimBlock> currentFrame = new List<AnimBlock>();
        static List<AnimBlock> nextFrame = new List<AnimBlock>();
        static List<AnimBlock> collidingBlocks = new List<AnimBlock>();

        public static void Activate()
        {
            MSTick = FPSMOGame.Instance.gameConfig.MS_UPDATE_WEAPON_ANIMATIONS;
            lock (activateLock)                 // Thread safety
            {
                if (instance != null) return;   // Singleton boilerplate
                instance = new Scheduler("WeaponAnimationsScheduler");
                task = instance.QueueRepeat(Update, null, TimeSpan.FromMilliseconds(MSTick));
                activated = true;
            }
            currentTick = 10;
            sender = new BufferedBlockSender(FPSMOGame.Instance.map);
        }

        public static uint Tick { get { return currentTick; } }

        public static void Deactivate()
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

        public static void AddAnimation(Animation anim)
        {
            if (activated) { weaponAnimations.Add(anim); }
        }

        private static void Update(SchedulerTask task)
        {
            currentFrame = nextFrame;
            nextFrame = GetNextFrame();
            collidingBlocks = new List<AnimBlock>();
            List<Animation> collidingAnims = GetCollidingAnims(ref collidingBlocks);

            foreach (Player p in FPSMOGame.Instance.players.Values)
            {
                sender = new BufferedBlockSender(p);
                SendCurrentFrame(p);
                HandleWalkthrough(p);
                RemoveCollidingBlocks(p);
            }
            RemoveCollidingAnims(collidingAnims);

            currentTick++;
        }

        private static List<Animation> GetCollidingAnims(ref List<AnimBlock> collidingBlocks)
        {
            collidingBlocks = new List<AnimBlock>();
            List<Animation> result = new List<Animation>();

            bool collideFlag;
            for (int i = 0; i < weaponAnimations.Count; i++)
            {
                collideFlag = false;
                Animation anim = weaponAnimations[i];

                // Check if any block in the animation collides
                List<AnimBlock> blocks = anim.GetCurrentBlocksInterpolate(currentTick - anim.frameLength, currentTick);
                foreach (AnimBlock ab in blocks)
                {
                    if (Block.Air != anim.shooter.level.GetBlock(ab.x, ab.y, ab.z))
                    {
                        result.Add(anim);
                        collideFlag = true;
                        break;
                    }
                }

               if (collideFlag) collidingBlocks.AddRange(blocks);
            }

            return result;
        }

        private static List<AnimBlock> GetNextFrame()   // TODO: could cache this for next tick
        {
            List<AnimBlock> result = new List<AnimBlock>();
            for (int i = 0; i < weaponAnimations.Count; i++)
            {
                Animation anim = weaponAnimations[i];
                result.AddRange(anim.GetCurrentBlocksInterpolate(currentTick, currentTick + anim.frameLength));
            }
            return result;
        }

        private static void SendCurrentFrame(Player p)
        {
            if (!activated) return;
            foreach (AnimBlock ab in currentFrame)
            {
                sender.Add(p.level.PosToInt(ab.x, ab.y, ab.z), p.level.GetBlock(ab.x, ab.y, ab.z));
            }

            // Add all the new blocks for this frame
            foreach (AnimBlock ab in nextFrame)
            {
                sender.Add(p.level.PosToInt(ab.x, ab.y, ab.z), ab.block); // TODO: could have overlapping block changes, but I think that's fine
            }

                // TODO: Down here work with static animations

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

        private static void RemoveCollidingAnims(List<Animation> collidingAnims)
        {
            foreach (Animation anim in collidingAnims)
            {
                weaponAnimations.Remove(anim);
            }
        }

        private static void RemoveCollidingBlocks(Player p)
        {
            foreach (AnimBlock ab in collidingBlocks)
            {
                sender.Add(p.level.PosToInt(ab.x, ab.y, ab.z), ab.block); // TODO: could have overlapping block changes, but I think that's fine
            }
        }

        private static void HandleWalkthrough(Player p)
        {
            if (!activated) return;

            Walkthrough(p, p.ModelBB.OffsetPosition(p.Pos));
        }

        /// <summary>
        /// Handles walkthrough for an individual player against all animated blocks
        /// </summary>
        internal static void Walkthrough(Player p, AABB bb)
        {
            Vec3S32 min = bb.BlockMin, max = bb.BlockMax;
            bool hitWalkthrough = false;

            // Copied from MCGalaxy source... I think there's a better way to do this?
            //
            // Looks like a huge loop but the number of animations isn't that large. Not sure why Unk handled it like this though,
            // Seems like you really only need to check against 8 points max

            // TODO: Make this OBB and optimize the inner 3 loops
            for (int i = 0; i < weaponAnimations.Count; i++)  // Small                
            {
                for (int y = min.Y; y <= max.Y; y++)
                        for (int z = min.Z; z <= max.Z; z++)
                            for (int x = min.X; x <= max.X; x++)
                            {
                                ushort xP = (ushort)x, yP = (ushort)y, zP = (ushort)z;
                                BlockID block = GetCurrentBlock(xP, yP, zP, p, currentFrame);
                                if (block == System.UInt16.MaxValue) continue;

                                AABB blockBB = Block.BlockAABB(block, p.level).Offset(x * 32, y * 32, z * 32);
                                if (!AABB.Intersects(ref bb, ref blockBB)) continue;

                                // We can activate only one walkthrough block per movement
                                if (!hitWalkthrough)
                                {
                                    HandleWalkthrough handler = p.level.WalkthroughHandlers[block];
                                    if (handler != null && handler(p, block, xP, yP, zP))
                                    {
                                        hitWalkthrough = true;
                                    }
                                }

                                // Some blocks will cause death of players
                                if (!p.level.Props[block].KillerBlock) continue;
                                if (p.level.Config.KillerBlocks) p.HandleDeath(block);  // TODO: Replace this with a handleDeath for the game
                            }
                }
        }

        internal static BlockID GetCurrentBlock(ushort xP, ushort yP, ushort zP, Player p, List<AnimBlock> currentFrame)
        {
            foreach (AnimBlock ab in currentFrame)
            {
                if (ab.x == xP && ab.y == yP && ab.z == zP)
                {
                    return ab.block;
                }
            }
            return System.UInt16.MaxValue;
        }
    }
    #endregion
}
