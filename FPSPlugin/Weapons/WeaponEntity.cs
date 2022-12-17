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
using FPSMO.Weapons;
using MCGalaxy;
using MCGalaxy.Maths;
using static FPS.Weapons.Projectile;
using BlockID = System.UInt16;

namespace FPS.Weapons;

/// <summary>
/// An abstract block that only exists to send block changes. Not a part of the map
/// </summary>
internal class WeaponBlock : IEqualityComparer<WeaponBlock> {
    internal WeaponBlock(Vec3U16 loc, BlockID block)
    {
        x = loc.X;
        y = loc.Y;
        z = loc.Z;
        this.block = block;
    }
    internal System.UInt16 x;
    internal System.UInt16 y;
    internal System.UInt16 z;
    internal BlockID block;

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
    internal Player shooter;
    protected uint fireTimeTick;
    internal float frameLength;   // Number of frames shown at once
    internal bool collided;
    internal protected uint deathTick = uint.MaxValue; // Last tick before we remove automatically
    protected OnHit onHit;

    internal List<WeaponBlock> currentBlocks = new(); // current tick blocks
    internal List<WeaponBlock> lastBlocks = new();    // last tick blocks

    internal uint damage;

    /// <summary>
    /// Gets the current blocks at a specific animation tick
    /// </summary>
    internal abstract List<WeaponBlock> GetCurrentBlocks(float tick);

    /// <summary>
    /// Gets the current blocks in a range of ticks. Interpolates your blocks
    /// Depth first in-order
    /// </summary>
    internal virtual List<WeaponBlock> GetCurrentBlocksInterpolate(float tickStart, float tickEnd, uint depth=0)
    {
        List<WeaponBlock> result = new();
        List<WeaponBlock> currentBlocks = GetCurrentBlocks(tickStart);

        // TODO: could be more efficient if not constantly jumping to find the map
        if (WeaponCollisionsHandler.CheckCollision(currentBlocks))   // If a collision is found return (and don't bother with what's comes after this tick)
        {
            // Set collision
            collided = true;

            WeaponBlock collidedBlock = WeaponCollisionsHandler.GetCollision(currentBlocks);
            
            // Call onhit (e.g., might spawn an explosion)
            onHit(new Vec3U16(collidedBlock.x, collidedBlock.y, collidedBlock.z));
            return new List<WeaponBlock>();
        }

        if (Enumerable.SequenceEqual(currentBlocks, GetCurrentBlocks(tickEnd)) || depth >= 5)
        {
            return currentBlocks;
        }

        // Since we do not have easy access to the inverse of our LocAt functions, we employ an in-order depth-first traversal
        result.AddRange(GetCurrentBlocksInterpolate(tickStart, (tickStart + tickEnd) / 2 - 1f / (float)Math.Pow(2, depth + 1), depth + 1));
        result.AddRange(GetCurrentBlocksInterpolate((tickStart + tickEnd) / 2 - 1f / (float)Math.Pow(2, depth), tickEnd, depth + 1));

        return result;
    }
}

internal class Projectile : WeaponEntity
{
    internal delegate Vec3F32 LocAt(float tick, Position orig, Orientation rot, uint fireTime, uint speed);  // Location at delegates the parametric function of our choice. Assumes a 1D path
    internal delegate void OnHit(Vec3U16 orig);

    private readonly LocAt locAt;
    private readonly BlockID block;
    private Position origin;
    private Orientation rotation;
    private readonly uint weaponSpeed;

    internal Projectile(Player p, uint start, BlockID b, Position orig, Orientation rot, float fl, uint ws, uint dmg, LocAt locat, OnHit onhit)
    {
        shooter = p;
        fireTimeTick = start;
        block = b;
        locAt = locat;
        onHit = onhit;
        origin = orig;
        rotation = rot;
        frameLength = fl;
        weaponSpeed = ws;
        damage = dmg;

        WeaponHandler.AddEntity(this);
    }

    internal override List<WeaponBlock> GetCurrentBlocks(float tick)  // TODO: Probably want to return a list of blocks, like a short line
    {
        Vec3F32 loc = locAt(tick, origin, rotation, fireTimeTick, weaponSpeed);
        Vec3U16 locU16 = new((ushort)(loc.X / 32), (ushort)(loc.Y / 32), (ushort)(loc.Z / 32));

        WeaponBlock ab = new(locU16, block);

        List<WeaponBlock> animBlocks = new()
        {
            ab
        };

        return animBlocks;
    }
}

internal class StaticAnimation : WeaponEntity
{
    List<List<WeaponBlock>> animation;
    uint firetime;

    internal StaticAnimation(float tick, AnimationType type, Vec3U16 origin)
    {
        animation = AnimationsLibrary.GetAnimationWithCollisions(type, origin);
        deathTick = (uint)(tick + animation.Count);
        firetime = WeaponHandler.Tick;

        WeaponHandler.AddEntity(this);
    }

    internal override List<WeaponBlock> GetCurrentBlocks(float tick)
    {
        return animation[Utils.Clamp((int)(tick - firetime), 0, animation.Count - 1)];
    }

    // Forbid interpolation in this case (it's a bit silly to interpolate)
    internal override List<WeaponBlock> GetCurrentBlocksInterpolate(float tickStart, float tickEnd, uint depth = 0)
    {
        return GetCurrentBlocks(tickStart);
    }
}
