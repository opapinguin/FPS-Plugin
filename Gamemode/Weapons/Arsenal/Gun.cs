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

using FPSMO.Configuration;
using MCGalaxy;
using MCGalaxy.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPSMO.Weapons
{
    internal class GunWeapon : ProjectileWeapon
    {
        internal GunWeapon(Player pl)
        {
            name = "gun";
            damage = Constants.GUN_DAMAGE;
            reloadTimeTicks = Constants.MS_GUN_RELOAD / Constants.MS_UPDATE_WEAPON_ANIMATIONS;
            player = pl;
            block = Constants.GUN_BLOCK;
            lastFireTick = WeaponHandler.Tick;
            frameLength = Constants.GUN_FRAME_LENGTH;
        }

        /// <summary>
        /// Location at a given time relative
        /// </summary>
        internal override Vec3F32 LocAt(float tick, Position orig, Orientation rot, uint fireTime, uint speed)
        {
            float timeSpanTicks = tick - fireTime;
            float time = timeSpanTicks * Constants.MS_UPDATE_WEAPON_ANIMATIONS / 1000;

            float velocity = (float)speed / 10 * (Constants.MAX_GUN_VELOCITY - Constants.MIN_GUN_VELOCITY) + Constants.MIN_GUN_VELOCITY;

            float distance = velocity * time;

            Vec3F32 dir = DirUtils.GetDirVector(rot.RotY, rot.HeadX);

            // Note these are precise coordinates, and so are actually small by a factor of 32
            return new Vec3F32(dir.X * distance * 32 + orig.X,
                dir.Y * distance * 32 - 0.5f * Constants.GRAVITY * time * time * 32 + orig.Y,
                dir.Z * distance * 32 + orig.Z);
        }

        internal override void Use(Orientation rot, Vec3F32 loc)
        {
            lastFireTick = WeaponHandler.Tick;
            // Instantiate the weapon animation
            WeaponEntity fireAnimation = new Projectile(player, lastFireTick, block, player.Pos, player.Rot, frameLength, weaponSpeed, damage, LocAt);
        }
    }
}
