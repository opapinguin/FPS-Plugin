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

using MCGalaxy;
using MCGalaxy.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPS.Weapons;

/// <summary>
/// Handles weapon animations and collisions
/// </summary>
internal static class WeaponHandler
{
    /**********
     * FIELDS *
     **********/
    static readonly object activateLock = new();
    static readonly object deactivateLock = new();

    static SchedulerTask task;
    static Scheduler instance;

    static uint currentTick;

    static List<WeaponEntity> weaponEntities = new();
    static List<WeaponEntity> collidingEntities = new();

    static Level level;

    internal static uint Tick { get { return currentTick; } }
    
    /// <summary>
    /// Prepares the weapon handler. In particular, initializes the current tick and the weapon schedulertask
    /// </summary>
    internal static void Activate()
    {
        currentTick = 10;   // Why not 0? Fixes issue with all weapon startTicks being 0, giving a long reload time

        level = FPSGame.Instance.Map;

        lock (activateLock)
        {
            if (instance != null) return;   // Singleton boilerplate
            instance = new Scheduler("WeaponAnimationsScheduler");
            task = instance.QueueRepeat(Update, null, TimeSpan.FromMilliseconds(Constants.UpdateWeaponAnimationsMilliseconds));
        }

        WeaponAnimsHandler.Activate();
        WeaponCollisionsHandler.Activate();
    }

    /// <summary>
    /// Deactivates the weapon handler. Unloads the scheduler task. Releases resources
    /// </summary>
    internal static void Deactivate()
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

    /// <summary>
    /// Adds a weapon entity to the list of entities
    /// </summary>
    /// <param name="we">Weapon entity</param>
    internal static void AddEntity(WeaponEntity we)
    {
        weaponEntities.Add(we);
    }

    /// <summary>
    /// Removes a weapon entity from the list of entities
    /// </summary>
    /// <param name="we">Weapon entity</param>
    internal static void RemoveEntity(WeaponEntity we)
    {
        weaponEntities.Remove(we);
    }

    /// <summary>
    /// The main update call in the scheduler task
    /// Handles animations and collisions in a frame and prepares the next repeatedly
    /// See function body for the full algorithm
    /// </summary>
    /// <param name="task"></param>
    internal static void Update(SchedulerTask task)
    {
        // 1. Find blocks for tick T
        // 2. Undraw everything from tick T-1 (this caches)
        // 3. Remove animations that were found to collide at T-1
        // 4. Set collidingEntities to tick T's colliding entities
        // 5. For entities that did collide, add to the collided entities list
        // 6. Check collisions against players and handle hits
        // 7. Draw all animations for tick T (this caches)
        // 8. Actually flush the animations (draw them)
        // 8. Increase tick to T+1
        // Rinse and repeat

        UpdateEntityBlocks();
        WeaponAnimsHandler.Undraw(weaponEntities, currentTick : false);
        RemoveEntities(collidingEntities);
        RemoveDiedEntities();
        collidingEntities = WeaponCollisionsHandler.GetCollisions(weaponEntities);  // Time-wise the heaviest line of code here
        WeaponAnimsHandler.Draw(weaponEntities, currentTick : true);
        WeaponCollisionsHandler.Update(weaponEntities);
        WeaponAnimsHandler.Flush();
        currentTick++;
    }

    /// <summary>
    /// Updates all entity block information in the given frame.
    /// In particular, fetches the current blocks available inside that weapon entity
    /// </summary>
    private static void UpdateEntityBlocks()
    {
        for (int i = 0; i < weaponEntities.Count; i++)
        {
            WeaponEntity we = weaponEntities[i];
            we.lastBlocks = we.currentBlocks;
            we.currentBlocks = we.GetCurrentBlocksInterpolate(Tick, Tick + we.frameLength);
            weaponEntities[i] = we;
        }
    }

    /// <summary>
    /// Removes a list of entities from the weapon entities
    /// </summary>
    /// <param name="weList">Weapon entity list</param>
    private static void RemoveEntities(List<WeaponEntity> weList)
    {
        foreach (WeaponEntity entity in weList)
        {
            WeaponHandler.RemoveEntity(entity);
        }
    }

    /// <summary>
    /// Removes entities that reached their lifetimes
    /// </summary>
    private static void RemoveDiedEntities()
    {
        List<WeaponEntity> diedEntities = new();
        foreach (WeaponEntity we in weaponEntities)
        {
            if (we.deathTick - currentTick <= 0) diedEntities.Add(we);
        }

        RemoveEntities(diedEntities);
    }
}
