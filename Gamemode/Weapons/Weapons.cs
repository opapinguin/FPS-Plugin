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
    /*********************
     * WEAPON INTERFACES *
     *********************/
    public abstract class Weapon
    {
        public abstract void Use(Vec3F32 dir, Vec3F32 loc, ushort strength);
        public abstract void OnHit(Player attacker, Player receiver);
        public abstract void CanHit(Vec3F32 locAttacker, Vec3F32 locReceiver);

        protected int damage, moneyCost;
        protected DateTime fireTime;
        protected TimeSpan reloadTime;
        protected Player player;
    }

    public abstract class Projectile : Weapon
    {
        public abstract Vec3F32 LocAt(DateTime t);

        protected float speed;
        protected Position origin;
        protected Orientation originOrientation;
        protected BlockID block;
    }

    public class GunWeapon : Projectile
    {
        public GunWeapon(Player pl)
        {
            damage = 1;
            moneyCost = 1;
            reloadTime = TimeSpan.FromMilliseconds(200);
            player = pl;
            origin = pl.Pos;
            originOrientation = pl.Rot;
            block = Block.Gold;
        }

        public override void CanHit(Vec3F32 locAttacker, Vec3F32 locReceiver)
        {
            throw new NotImplementedException();
        }

        public override Vec3F32 LocAt(DateTime t)
        {
            int timeSpanMS = (t - fireTime).Milliseconds;
            Vec3F32 loc = new Vec3F32(player.Rot.RotX * timeSpanMS + origin.X, player.Rot.RotY * timeSpanMS + origin.Y, player.Rot.RotZ * timeSpanMS + origin.Z);
            return loc;
        }

        public override void OnHit(Player attacker, Player receiver)
        {
            throw new NotImplementedException();
        }

        public override void Use(Vec3F32 dir, Vec3F32 loc, ushort strength)
        {
            fireTime = DateTime.Now;
            // Instantiate the weapon animation
            Animation fireAnimation = new ProjectileAnimation(player, DateTime.Now, block, LocAt);

            WeaponAnimsHandler.AddAnimation(fireAnimation);

            // TODO: Other logic for hit detection (keep this decoupled from the animationHandler)
        }
    }
}
