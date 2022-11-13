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
    #region Weapon Interfaces
    internal abstract class Weapon
    {
        public abstract void Use(Orientation rot, Vec3F32 loc, ushort strength);
        public virtual ushort GetStatus(uint tick)       // 10 if fully reloaded, 0 if not, and everything inbetween
        {
            ushort status = (ushort)((float)(tick - lastFireTick) / (float)reloadTimeTicks * 10);
            return (ushort)(status > 10 ? 10 : status);
        }
        public virtual void Reset() { lastFireTick = 0; }

        public string name;
        protected uint damage;
        protected uint lastFireTick;    // Much more efficient than using timespans
        protected uint reloadTimeTicks; // Ditto
        protected Player player;
        public ushort WeaponSpeed { get; set; }
    }

    internal abstract class ProjectileWeapon : Weapon
    {
        public abstract Vec3F32 LocAt(float tick, Position orig, Orientation rot, uint fireTimeTick, uint weaponSpeed);

        protected BlockID block;
        protected float frameLength;
    }

    #endregion
    /***********
     * WEAPONS *
     ***********/
    #region Weapons

    internal class GunWeapon : ProjectileWeapon
    {
        public GunWeapon(Player pl)
        {
            FPSMOGameConfig config = FPSMOGame.Instance.gameConfig;

            name = "gun";
            damage = config.GUN_DAMAGE;
            reloadTimeTicks = config.MS_GUN_RELOAD / config.MS_UPDATE_WEAPON_ANIMATIONS;
            player = pl;
            block = config.GUN_BLOCK;
            lastFireTick = WeaponHandler.Tick;
            frameLength = config.GUN_FRAME_LENGTH;
        }

        /// <summary>
        /// Location at a given time relative
        /// </summary>
        public override Vec3F32 LocAt(float tick, Position orig, Orientation rot, uint fireTime, uint speed)
        {
            FPSMOGameConfig config = FPSMOGame.Instance.gameConfig;

            float timeSpanTicks = tick - fireTime;
            float time = timeSpanTicks * config.MS_UPDATE_WEAPON_ANIMATIONS / 1000;
            
            float velocity = (float)speed / 10 * (config.MAX_GUN_VELOCITY - config.MIN_GUN_VELOCITY) + config.MIN_GUN_VELOCITY;

            float distance = velocity * time;

            Vec3F32 dir = DirUtils.GetDirVector(rot.RotY, rot.HeadX);

            // Note these are precise coordinates, and so are actually small by a factor of 32
            return new Vec3F32(dir.X * distance * 32 + orig.X,
                dir.Y * distance * 32 - 0.5f * config.GRAVITY * time * time * 32 + orig.Y,
                dir.Z * distance * 32 + orig.Z);
        }

        public override void Use(Orientation rot, Vec3F32 loc, ushort strength) // TODO: Implement strength
        {
            lastFireTick = WeaponHandler.Tick;
            // Instantiate the weapon animation
            WeaponEntity fireAnimation = new Projectile(player, lastFireTick, block, player.Pos, player.Rot, frameLength, WeaponSpeed, LocAt);
        }
    }

    internal class RocketWeapon : ProjectileWeapon
    {
        public RocketWeapon(Player pl)
        {
            FPSMOGameConfig config = FPSMOGame.Instance.gameConfig;

            name = "rocket";
            damage = config.ROCKET_DAMAGE;
            reloadTimeTicks = config.MS_ROCKET_RELOAD / config.MS_UPDATE_WEAPON_ANIMATIONS;
            player = pl;
            block = config.ROCKET_BLOCK;
            lastFireTick = WeaponHandler.Tick;
            frameLength = config.ROCKET_FRAME_LENGTH;
        }

        /// <summary>
        /// Location at a given tick
        /// </summary>
        public override Vec3F32 LocAt(float tick, Position orig, Orientation rot, uint fireTime, uint speed)
        {
            FPSMOGameConfig config = FPSMOGame.Instance.gameConfig;

            float timeSpanTicks = tick - fireTime;
            float time = timeSpanTicks * config.MS_UPDATE_WEAPON_ANIMATIONS / 1000;
            
            float absVelocity = (float)speed / 10 * (config.MAX_ROCKET_VELOCITY - config.MIN_ROCKET_VELOCITY) + config.MIN_ROCKET_VELOCITY;

            float distance = absVelocity * time;

            Vec3F32 dir = DirUtils.GetDirVector(rot.RotY, rot.HeadX);
            Vec3F32 velBar = Vec3F32.Normalise(new Vec3F32(dir.X * absVelocity, dir.Y * absVelocity - config.GRAVITY * time, dir.Y * absVelocity));  // Velocity of the parabola

            // HELIX CALCULATION

            // Some basic calculus to get the perpendicular vector and construct a helix

            // The formula for the perpendicular pointing "backward" on the parabola
            Vec3F32 backwardVectorBar = new Vec3F32(-dir.X,
               (float)Math.Sqrt(1 - velBar.Y * velBar.Y),
                -dir.Z);                    // Helix displacement

            // Cross product with that to get the perpendicular vector that we want
            Vec3F32 helixBar = Vec3F32.Cross(backwardVectorBar, Vec3F32.Normalise(velBar));
            Vec3F32 helixDisplacement = (float)Math.Sin(time * 7f) * helixBar + (float)Math.Cos(time * 7f) * velBar;

            float helixR = (float)(10f / (1 + Math.Exp(-time * 3f)) - 5);   // Radius kind of peters out as time passes. Sigmoid

            // MOVING UP AND DOWN CALCULATION
            // This part is handled by a simple cubic
            float cY = 50;  // Add an extra 50 meters
            float cX = 2;  // For about 2 seconds
            float cubicDisplacementY = -27f * cY / (4f * cX * cX * cX) * time * time * time + 27f * cY / (4f * cX * cX) * time * time;

            // Note these are precise coordinates, and so are actually large by a factor of 32
            return new Vec3F32(dir.X * distance * 32 + helixR * helixDisplacement.X * 32 + orig.X,
                dir.Y * distance * 32 - 0.5f * config.GRAVITY * time * time * 32 + helixR * helixDisplacement.Y * 32 + cubicDisplacementY * 32 + orig.Y,
                dir.Z * distance * 32 + helixR * helixDisplacement.Z * 32 + orig.Z);
        }

        public override void Use(Orientation rot, Vec3F32 loc, ushort strength) // TODO: Implement strength
        {
            lastFireTick = WeaponHandler.Tick;
            // Instantiate the weapon animation
            WeaponEntity fireAnimation = new Projectile(player, lastFireTick, block, player.Pos, player.Rot, frameLength, WeaponSpeed, LocAt);
        }
    }



    #endregion
}
