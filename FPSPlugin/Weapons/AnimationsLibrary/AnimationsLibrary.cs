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

using MCGalaxy.Maths;
using MCGalaxy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BlockID = System.UInt16;
using System.CodeDom.Compiler;
using MCGalaxy.Blocks;
using FPSMO.Weapons;

namespace FPS.Weapons;

/// <summary>
/// This contains all the weapon animations needed for hit events (e.g., explosions)
/// </summary>
internal static class AnimationsLibrary
{
    static List<List<WeaponBlock>> smallExplosion;
    //List<List<WeaponBlock>> mediumExplosion;
    //List<List<WeaponBlock>> largeExplosion;

    /// <summary>
    /// Initializes the animations library. Retrieves and caches animations
    /// </summary>
    static internal void Initialize()
    {
        smallExplosion = ReadAnimations(AnimationType.SmallExplosion);
        //mediumExplosion = ReadAnimations(AnimationType.MediumExplosion);
        //largeExplosion = ReadAnimations(AnimationType.LargeExplosion);
    }
    
    /// <summary>
    /// Extracts an animation of a given type displaced by some amount, and accounting for collisions
    /// Collisions are accounted for relative to the origin
    /// OP blocks are seen as "collidable"
    /// </summary>
    /// <param name="type">The animation type we wish to retrieve (e.g., smallExplosion)</param>
    /// <param name="origin">The origin of this animation</param>
    /// <returns></returns>
    internal static List<List<WeaponBlock>> GetAnimationWithCollisions(AnimationType type, Vec3U16 origin)
    {
        // Candidates to check against
        List<List<WeaponBlock>> candidateFrames = GetAnimation(type, origin);
        List<WeaponBlock> blocksFrame;

        List<List<WeaponBlock>> result = new();

        foreach (List<WeaponBlock> candidateFrame in candidateFrames) {
            blocksFrame = new();
            foreach (WeaponBlock wb in candidateFrame)
            {
                // If this block did not collide add it
                // TODO: Not the most efficient method really, could reduce its running time by O(n) if we could efficiently cache the ray. But this routine is small
                if (!Collided(origin, new Vec3U16(wb.x, wb.y, wb.z)))
                {
                    candidateFrame.Add(wb);
                }
            }
            result.Add(candidateFrame);
        }

        return result;   // Default behavior
    }

    /// <summary>
    /// Check if there is an OP block between start and end location
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    private static bool Collided(Vec3U16 start, Vec3U16 end, int depth=0)
    {
        if (start == end || depth == 5)
        {
            // TODO: calling this each time is rather inefficient. Can somehow cache it somewhere?
            Level map = FPSGame.Instance.Map;
            return map.Props[map.GetBlock(start.X, start.Y, start.Z)].OPBlock == true;
        }

        bool startHalf = Collided(start,
            new Vec3U16(
            (ushort)((start.X + end.X) << 1),
            (ushort)((start.Y + end.Y) << 1),
            (ushort)((start.Z + end.Z) << 1)
            ), depth+1);
        bool endHalf = Collided(new Vec3U16(
            (ushort)((start.X + end.X) << 1),
            (ushort)((start.Y + end.Y) << 1),
            (ushort)((start.Z + end.Z) << 1)),
            end, depth+1);

        return (startHalf || endHalf);
    }

    /// <summary>
    /// Extracts an animation of a given type displaced by some amount, not accounting for collisions
    /// </summary>
    /// <param name="type">The animation type we wish to retrieve (e.g., smallExplosion)</param>
    /// <param name="origin">The origin of this animation</param>
    /// <returns>A list of weaponblocks</returns>
    /// <exception cref="NotImplementedException">Exception if animation type is not yet implemented</exception>
    private static List<List<WeaponBlock>> GetAnimation(AnimationType type, Vec3U16 origin)
    {
        List<List<WeaponBlock>> result = new List<List<WeaponBlock>>();
        switch (type)
        {
            case AnimationType.SmallExplosion:
                result = new List<List<WeaponBlock>>(smallExplosion);
                break;
            default:
                throw new NotImplementedException("No support for animation type" + type.ToString());
        }

        for (int i = 0; i < result.Count; i++)
        {
            for (int j = 0; j < result[i].Count; j++)
            {
                // Add the offset for this animation based on where it hits
                result[i][j].x += origin.X;
                result[i][j].y += origin.Y;
                result[i][j].z += origin.Z;

                // Remove inherent offset in cached animations
                result[i][j].x -= 32768;
                result[i][j].y -= 32768;
                result[i][j].z -= 32768;
            }
        }

        return result;   // Default behavior
    }

    /// <summary>
    /// Reads an animation file (.txt) and saves the animation in the handler
    /// Returns a list of weaponblocks, offset by (+32768, +32768, +32768) to allow "negative" offsets relative to the origin (origin being (32768, 32768 32768))
    /// </summary>
    private static List<List<WeaponBlock>> ReadAnimations(AnimationType type)
    {
        List<List<WeaponBlock>> result = new();
        List<WeaponBlock> currentFrameBlocks = new();
        int currentFrame = 0;
        UInt16 x, y, z;
        BlockID block;
        string filePath;

        // Break down by type
        switch (type)
        {
            case AnimationType.SmallExplosion:
                filePath = "Weapons/AnimationsLibrary/SmallExplosion.txt";
                break;
            default:
                throw new NotImplementedException("No support for animation type" + type.ToString());
        }

        // Now parse the file
        StreamReader file = null;
        try
        {
            string line;
            file = new StreamReader(filePath);
            while ((line = file.ReadLine()) != null)
            {
                Regex rFrame = new Regex(@"[0-9]+");
                Regex rBlock = new Regex(@"[+-]?[0-9]+ [+-]?[0-9]+ [+-]?[0-9]+ [+-]?[0-9]+");
                // Lines are formatted as either [frame number]
                // OR as [x offset] [y offset] [z offset] [BlockID]

                // Check if the line is a match for the regex
                if (!rFrame.IsMatch(line) && !rBlock.IsMatch(line))
                {
                    Logger.Log(LogType.Error, String.Format("Line '{0} in {1} cannot be parsed'", line, filePath));
                    continue;
                }

                // If it's just a line (animation frame) and move on
                if (rFrame.IsMatch(line))
                {
                    // Set current frame
                    int.TryParse(line, out currentFrame);
                    if (currentFrameBlocks.Count > 0)
                    {
                        result.Add(currentFrameBlocks);
                    }
                    currentFrameBlocks.Clear();
                    continue;
                }

                // If it's a block, add it to the current blocks and move on
                if (rBlock.IsMatch(line)) {
                    // Here the match is rBlock, no other way round it
                    string[] lines = line.SplitSpaces();
                    UInt16.TryParse(lines[0], out x);
                    UInt16.TryParse(lines[1], out y);
                    UInt16.TryParse(lines[2], out z);
                    UInt16.TryParse(lines[3], out block);

                    currentFrameBlocks.Add(new WeaponBlock(new Vec3U16((UInt16)(x + 32768), (UInt16)(y + 32768), (UInt16)(z + 32768)), block));
                }
            }
            return result;
        }
        catch (Exception e)
        {
            Logger.Log(LogType.Error, String.Format("Problem reading animations {0}: {1}", filePath, e.StackTrace));
        }
        finally
        {
            file?.Close();
        }

        return result;
    }
}
