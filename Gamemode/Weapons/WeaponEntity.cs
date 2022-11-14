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
    /// <summary>
    /// An abstract block that only exists to send block changes. Not a part of the map
    /// </summary>
    internal struct WeaponBlock : IEqualityComparer<WeaponBlock> {
        public WeaponBlock(Vec3U16 loc, BlockID block)
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

        public bool Equals(WeaponBlock a, WeaponBlock b)
        {
            return (a.x == b.x && a.y == b.y && a.z == b.z && a.block == b.block);
        }

        public int GetHashCode(WeaponBlock obj)
        {
            return obj.GetHashCode();
        }
    }

    /// <summary>
    /// An interface for different types of weapon entities
    /// </summary>
    internal abstract class WeaponEntity
    {
        public Player shooter;
        protected uint fireTimeTick;
        public float frameLength;   // Number of frames shown at once
        public bool collided;

        public List<WeaponBlock> currentBlocks = new List<WeaponBlock>(); // current tick blocks
        public List<WeaponBlock> lastBlocks = new List<WeaponBlock>();    // last tick blocks

        /// <summary>
        /// Gets the current blocks at a specific animation tick
        /// </summary>
        public abstract List<WeaponBlock> GetCurrentBlocks(float tick);

        /// <summary>
        /// Gets the current blocks in a range of ticks. Interpolates your blocks
        /// Depth first in-order
        /// </summary>
        public virtual List<WeaponBlock> GetCurrentBlocksInterpolate(float tickStart, float tickEnd, uint depth=0)
        {
            List<WeaponBlock> result = new List<WeaponBlock>();
            List<WeaponBlock> currentBlocks = GetCurrentBlocks(tickStart);

            if (WeaponCollisionsHandler.CheckCollision(currentBlocks, shooter.level))   // If a collision is found return (and don't bother with what's comes after this tick)
            {
                collided = true;
                return new List<WeaponBlock>();
            }

            if (Enumerable.SequenceEqual(currentBlocks, GetCurrentBlocks(tickEnd)) || depth >= 5)
            {
                return currentBlocks;
            }

            // Since we do not have easy access to the inverse of our LocAt functions, we employ an in-order depth-first traversal
            result.AddRange(GetCurrentBlocksInterpolate(tickStart, (tickStart + tickEnd) / 2, depth + 1));
            result.AddRange(GetCurrentBlocksInterpolate((tickStart + tickEnd) / 2, tickEnd, depth + 1));

            return result;
        }
    }

    internal class Projectile : WeaponEntity
    {
        public delegate Vec3F32 LocAt(float tick, Position orig, Orientation rot, uint fireTime, uint speed);  // Location at delegates the parametric function of our choice. Assumes a 1D path

        private readonly LocAt locAt;
        private readonly BlockID block;
        private Position origin;
        private Orientation rotation;
        private readonly uint weaponSpeed;

        public Projectile(Player p, uint start, BlockID b, Position orig, Orientation rot, float fl, uint ws, LocAt ft)
        {
            shooter = p;
            fireTimeTick = start;
            block = b;
            locAt = ft;
            origin = orig;
            rotation = rot;
            frameLength = fl;
            weaponSpeed = ws;

            WeaponHandler.AddEntity(this);
        }

        public override List<WeaponBlock> GetCurrentBlocks(float tick)  // TODO: Probably want to return a list of blocks, like a short line
        {
            Vec3F32 loc = locAt(tick, origin, rotation, fireTimeTick, weaponSpeed);
            Vec3U16 locU16 = new Vec3U16((ushort)(loc.X / 32), (ushort)(loc.Y / 32), (ushort)(loc.Z / 32));

            WeaponBlock ab = new WeaponBlock(locU16, block);

            List<WeaponBlock> animBlocks = new List<WeaponBlock>
            {
                ab
            };

            return animBlocks;
        }
    }
}
