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

using MCGalaxy.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPSMO.Weapons
{
    /// <summary>
    /// Handles weapon animations and collisions
    /// </summary>
    internal static class WeaponHandler
    {
        /**********
         * FIELDS *
         **********/
        static readonly object activateLock = new object();
        static readonly object deactivateLock = new object();

        static SchedulerTask task;
        static Scheduler instance;

        static uint MSTick;
        static uint currentTick;

        static List<WeaponEntity> weaponEntities = new List<WeaponEntity>();
        static List<WeaponEntity> collidingEntities = new List<WeaponEntity>();

        public static uint Tick { get { return currentTick; } }

        public static void Activate()
        {
            currentTick = 10;   // Why not 0? Fixes issue with all weapon startTicks being 0, giving a long reload time
            MSTick = FPSMOGame.Instance.gameConfig.MS_UPDATE_WEAPON_ANIMATIONS;

            lock (activateLock)
            {
                if (instance != null) return;   // Singleton boilerplate
                instance = new Scheduler("WeaponAnimationsScheduler");
                task = instance.QueueRepeat(Update, null, TimeSpan.FromMilliseconds(MSTick));
            }

            WeaponAnimsHandler.Activate();
        }

        public static void Deactivate()
        {
            lock (deactivateLock)
            {
                if (instance != null)
                {
                    instance.Cancel(task);
                    instance = null;
                }
            }

            WeaponAnimsHandler.Undraw(weaponEntities, currentTick : true);
            WeaponAnimsHandler.Deactivate();
            currentTick = 10;

            weaponEntities = new List<WeaponEntity>();
        }

        public static void AddEntity(WeaponEntity anim)
        {
            weaponEntities.Add(anim);
        }

        public static void RemoveEntity(WeaponEntity anim)
        {
            weaponEntities.Remove(anim);
        }

        public static void Update(SchedulerTask task)
        {
            // 1. Find blocks for tick T
            // 2. Undraw everything from tick T-1
            // 3. Remove animations that were found to collide at T-1
            // 4. Set collidingEntities to tick T's colliding entities
            // 5. For entities that did collide, add to the collided entities list
            // 6. Check collisions against players and handle hits
            // 7. Draw all animations for tick T
            // 8. Increase tick to T+1
            // Rinse and repeat

            UpdateEntityBlocks();
            WeaponAnimsHandler.Undraw(weaponEntities, currentTick : false);
            RemoveEntities(collidingEntities);
            collidingEntities = WeaponCollisionsHandler.GetCollisions(weaponEntities);
            WeaponAnimsHandler.Draw(weaponEntities, currentTick : true);
            WeaponCollisionsHandler.Update(weaponEntities);
            currentTick++;
        }

        private static void UpdateEntityBlocks()
        {
            foreach(WeaponEntity we in weaponEntities)
            {
                we.lastBlocks = we.currentBlocks;
                we.currentBlocks = we.GetCurrentBlocksInterpolate(Tick, Tick + we.frameLength);
            }
        }

        private static void RemoveEntities(List<WeaponEntity> weList)
        {
            foreach (WeaponEntity entity in weList)
            {
                WeaponHandler.RemoveEntity(entity);
            }
        }
    }
}
