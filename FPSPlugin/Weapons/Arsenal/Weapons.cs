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
using FPS.Configuration;
using MCGalaxy;
using MCGalaxy.Maths;
using MCGalaxy.Network;
using MCGalaxy.Tasks;
using BlockID = System.UInt16;

namespace FPS.Weapons;

internal abstract class Weapon
{
    internal abstract void Use(Orientation rot, Vec3F32 loc);
    internal virtual ushort GetStatus(uint tick)       // 10 if fully reloaded, 0 if not, and everything inbetween
    {
        ushort status = (ushort)((float)(tick - lastFireTick) / (float)reloadTimeTicks * 10);
        return (ushort)(status > 10 ? 10 : status);
    }
    internal virtual void Reset() { lastFireTick = 0; }

    internal string name;
    protected uint damage;
    protected uint lastFireTick;    // Much more efficient than using timespans
    protected uint reloadTimeTicks; // Ditto
    protected Player player;
    internal ushort weaponSpeed;
}

internal abstract class ProjectileWeapon : Weapon
{
    internal abstract Vec3F32 LocAt(float tick, Position orig, Orientation rot, uint fireTimeTick, uint weaponSpeed);

    protected BlockID block;
    protected float frameLength;
}
