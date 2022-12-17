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

using System.Collections.Generic;
using MCGalaxy;
using MCGalaxy.Blocks;
using MCGalaxy.Maths;
using BlockID = System.UInt16;

namespace FPS.Weapons;

/// <summary>
/// Handles collisions of weapon entities against players
/// </summary>
internal static class WeaponCollisionsHandler
{
    static Level level;

    /// <summary>
    /// Sets up the weapon collisions handler
    /// </summary>
    internal static void Activate()
    {
        level = FPSGame.Instance.Map;
    }

    /// <summary>
    /// Checks collisions against players on the FPS map and emits the corresponding death events
    /// </summary>
    /// <param name="weaponEntities">Weapon entities</param>
    internal static void Update(List<WeaponEntity> weaponEntities)
    {
        foreach (Player p in FPSGame.Instance.Players.Values)
        {
            Walkthrough(p, p.ModelBB.OffsetPosition(p.Pos), weaponEntities);    // Handle walkthrough
        }
    }

    /// <summary>
    /// Checks for a collision against the map in a list of blocks
    /// A collision occurs when this list of weapon blocks overlaps with any non-air block
    /// </summary>
    /// <param name="blocks">List of weapon blocks</param>
    /// <returns>True if there is any collision, False otherwise</returns>
    internal static bool CheckCollision(List<WeaponBlock> blocks)
    {
        foreach (WeaponBlock wb in blocks)
        {
            if (Block.Air != level.GetBlock(wb.x, wb.y, wb.z)) {
                return true;
            }
        }
        return false;
    }

    internal static WeaponBlock GetCollision(List<WeaponBlock> blocks)
    {
        foreach (WeaponBlock wb in blocks)
        {
            if (Block.Air != level.GetBlock(wb.x, wb.y, wb.z))
            {
                return wb;
            }
        }
        return null;
    }

    /// <summary>
    /// Gets the collided blocks in a list of weapon entities
    /// </summary>
    /// <param name="weaponEntities">Weapon entities</param>
    /// <returns>A sublist for which all weapon entities collided</returns>
    internal static List<WeaponEntity> GetCollisions(List<WeaponEntity> weaponEntities)
    {
        List<WeaponEntity> result = new();

        for (int i = 0; i < weaponEntities.Count; i++)
        {
            if (weaponEntities[i].collided) { result.Add(weaponEntities[i]); }
        }

        return result;
    }

    /// <summary>
    /// Handles walkthrough for an individual player against all animated blocks
    /// </summary>
    /// <param name="p">The player against which we check</param>
    /// <param name="bb">The player's axis aligned bounding box</param>
    /// <param name="weaponEntities">The list of weapon entities against which we check</param>
    private static void Walkthrough(Player p, AABB bb, List<WeaponEntity> weaponEntities)
    {
        Vec3S32 min = bb.BlockMin, max = bb.BlockMax;
        bool hitWalkthrough = false;

        // Copied from MCGalaxy source... I think there's a better way to do this?
        //
        // Looks like a huge loop but the number of animations isn't that large. Not sure why Unk handled it like this though,
        // Seems like you really only need to check against 8 points max

        // TODO: Make this OBB and optimize the inner 3 loops
        for (int i = 0; i < weaponEntities.Count; i++)  // Small                
        {
            for (int y = min.Y; y <= max.Y; y++)
                for (int z = min.Z; z <= max.Z; z++)
                    for (int x = min.X; x <= max.X; x++)
                    {
                        ushort xP = (ushort)x, yP = (ushort)y, zP = (ushort)z;

                        BlockID block = GetCurrentBlock(xP, yP, zP, weaponEntities[i]);
                        if (block == System.UInt16.MaxValue) continue;

                        AABB blockBB = Block.BlockAABB(block, level).Offset(x * 32, y * 32, z * 32);
                        if (!AABB.Intersects(ref bb, ref blockBB)) continue;

                        // We can activate only one walkthrough block per movement
                        if (!hitWalkthrough)
                        {
                            HandleWalkthrough handler = level.WalkthroughHandlers[block];
                            if (handler != null && handler(p, block, xP, yP, zP))
                            {
                                hitWalkthrough = true;
                            }
                        }

                        if (weaponEntities[i].shooter == p)
                        {
                            continue;
                        }

                        // Some blocks will cause death of players
                        if (!level.Props[block].KillerBlock) continue;
                        if (level.Config.KillerBlocks) FPSGame.Instance.OnPlayerHitPlayer(weaponEntities[i].shooter, p, weaponEntities[i]);
                    }
        }
    }

    /// <summary>
    /// Returns the current block in a weapon entity given a location
    /// </summary>
    /// <param name="x">x coordinate</param>
    /// <param name="y">y coordinate</param>
    /// <param name="z">z coordinate</param>
    /// <param name="we">Weapon entity</param>
    /// <returns>The blockID of the weapon block that is found at (x, y, z).
    /// If not found, returns System.UInt16.MaxValue</returns>
    internal static BlockID GetCurrentBlock(ushort x, ushort y, ushort z, WeaponEntity we)
    {
        foreach (WeaponBlock wb in we.currentBlocks)
            {
                if (wb.x == x && wb.y == y && wb.z == z)
                {
                    return wb.block;
                }
            }
        return System.UInt16.MaxValue;
    }
}
