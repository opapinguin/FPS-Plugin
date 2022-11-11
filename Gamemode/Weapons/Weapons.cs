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
using FPSMO.Configuration;
using MCGalaxy;
using MCGalaxy.Maths;
using MCGalaxy.Network;
using MCGalaxy.Tasks;
using BlockID = System.UInt16;

namespace FPSMO.Weapons
{
    /*********************
     * WEAPON INTERFACES *
     *********************/
    public abstract class Weapon
    {
        public abstract void Use(Orientation rot, Vec3F32 loc, ushort strength);
        public virtual ushort GetStatus(uint tick)       // 10 if fully reloaded, 0 if not, and everything inbetween
        {
            ushort status = (ushort)((float)(tick - lastFireTick) / (float)reloadTimeTicks * 10);
            return (ushort)(status > 10 ? 10 : status);
        }
        public virtual void Reset() { lastFireTick = 0; }

        protected uint damage;
        protected uint lastFireTick;    // Much more efficient than using timespans
        protected uint reloadTimeTicks; // Ditto
        protected Player player;
    }

    public abstract class ProjectileWeapon : Weapon
    {
        public abstract Vec3F32 LocAt(float tick, Position orig, Orientation rot, uint fireTimeTick);

        protected float velocity;
        protected BlockID block;
    }

    public class GunWeapon : ProjectileWeapon
    {
        public GunWeapon(Player pl)
        {
            FPSMOGameConfig config = FPSMOGame.Instance.gameConfig;

            damage = config.GUN_DAMAGE;     // TODO: Might be nicer to put this in the game configuration
            reloadTimeTicks = config.MS_GUN_RELOAD_MS;
            player = pl;
            block = config.GUN_BLOCK;
            lastFireTick = WeaponAnimsHandler.Tick;
            velocity = config.MS_GUN_VELOCITY;
        }

        /// <summary>
        /// Location at a given time relative
        /// </summary>
        public override Vec3F32 LocAt(float tick, Position orig, Orientation rot, uint fireTime)
        {
            FPSMOGameConfig config = FPSMOGame.Instance.gameConfig;

            float timeSpanTicks = tick - fireTime;
            float distance = velocity * timeSpanTicks / config.MS_UPDATE_WEAPON_ANIMATIONS * 1000;

            Vec3F32 dir = DirUtils.GetDirVector(rot.RotY, rot.HeadX);

            // Note these are precise coordinates, and so are actually large by a factor of 32
            return new Vec3F32(dir.X * distance + orig.X,
                dir.Y * distance + orig.Y,
                dir.Z * distance + orig.Z);
        }

        public override void Use(Orientation rot, Vec3F32 loc, ushort strength) // TODO: Implement strength
        {
            lastFireTick = WeaponAnimsHandler.Tick;
            // Instantiate the weapon animation
            Animation fireAnimation = new ProjectileAnimation(player, lastFireTick, block, player.Pos, player.Rot, LocAt);
        }

        ~GunWeapon() {
            Logger.Log(LogType.ConsoleMessage, "Deleted");  // TODO: Need to implement garbage collection for these guns. Or rather the animations
        }
    }
}
