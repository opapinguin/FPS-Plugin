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
using System.Threading;
using FPS.Entities;
using FPS.Weapons;
using MCGalaxy;

namespace FPS
{
    /// <summary>
    /// Round-specific functions. The round is the second stage of the game, between the countdown and the voting
    /// </summary>
    internal sealed partial class FPSMOGame
    {
        /*************
         * BEGINNING *
         *************/
        #region begin
        private void BeginRound()
        {
            WeaponHandler.Activate();

            // Move on to the next sub-stage
            subStage = SubStage.Middle;
            OnRoundStarted();
        }

        #endregion
        /**********
         * MIDDLE *
         **********/
        #region middle
        private void MiddleRound()
        {
            if (DateTime.Now >= roundStart + roundTime) {
                // Move on to the next sub-stage
                subStage = SubStage.End;
                return;
            }

            // Update weapon status for all players
            Dictionary<String, Player > playersCopy = new Dictionary<String, Player>(players);
            foreach (var kvp in playersCopy)
            {
                // Hacky way to get that extra message in when status is 10. Second term gets the number of weapon ticks passed after a round tick
                if (PlayerDataHandler.Instance[kvp.Key].currentWeapon.GetStatus(WeaponHandler.Tick
                    - (uint)((float)Constants.MS_ROUND_TICK / (float)Constants.MS_UPDATE_WEAPON_ANIMATIONS)) < 10)
                {
                    OnWeaponStatusChanged(kvp.Value);
                }
            }

            // The below line is generally bad practice, and indeed we therefore require that updateRound() does the minimum work possible
            // The animation loops and other events are handled by scheduler tasks on other threads and don't just sleep like this
            // Most of the stuff on this thread is small
            // TODO: If you want to do extra work while you wait for something to happen, the typical way is caching a datetime for the last time this thread woke up
            // And sleep only for the time necessary after tasks are performed
            Thread.Sleep((int)Constants.MS_ROUND_TICK);    // TODO: Add this to the configuration

            DateTime roundEnd = roundStart + roundTime;
            TimeSpan timeLeft = roundEnd - DateTime.Now;
            OnRoundTicked((int) timeLeft.TotalSeconds);
        }

        #endregion
        /*******
         * END *
         *******/
        #region end
        private void EndRound()
        {
            WeaponHandler.Deactivate();

            // Move on to the next sub-stage and stage
            stage = Stage.Voting;
            subStage = SubStage.Begin;

            OnRoundEnded();
        }

        #endregion
    }
}
