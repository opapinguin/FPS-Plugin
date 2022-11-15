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

using FPSMO.Entities;
using MCGalaxy;
using MCGalaxy.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPSMO
{
    /// <summary>
    /// 
    /// 
    /// TODO: Could squeeze a little extra performance out turning these off when they're not needed, but during
    /// the main round they should all run at the same time without noticeable lag. So might as well always run them
    /// </summary>
    internal sealed partial class FPSMOGame
    {
        private SchedulerTask RoundStatusTask;
        private Scheduler RoundStatusInstance;

        private readonly object activateLock = new object();

        // Putting this here even if it fits the gameloop. Sending CPE messages too regularly can spike ping, so once a second is enough
        public void ActivateTasks()
        {
            lock (activateLock)
            {
                if (RoundStatusInstance != null) { return; }
                
                RoundStatusInstance = new Scheduler("Round Update");
                RoundStatusTask = RoundStatusInstance.QueueRepeat(RoundStatusUpdate, null, TimeSpan.FromMilliseconds(50));
            }
        }

        public void DeactivateTasks()
        {
            if (RoundStatusInstance != null)
            {
                RoundStatusInstance.Cancel(RoundStatusTask);
            }
            RoundStatusInstance = null;
        }

        private void RoundStatusUpdate(SchedulerTask task)
        {
            if (stage == Stage.Round && subStage == SubStage.Middle)
            {
                ShowToAll(ShowRoundTime);
                ShowToAll(ShowStamina);
                ShowToAll(ShowHealth);
                ShowToAll(ShowWeaponStatus);
                ShowToAll(ShowTeamStatistics);
                ShowToAll(ShowLevel);
            }
            if (stage == Stage.Voting && subStage == SubStage.Middle)
            {
                ShowToAll(ShowVoteTime);
            }
            ClearWeaponSpeeds();
        }

        private void ClearWeaponSpeeds()
        {
            Dictionary<string, Player> playersCopy = new Dictionary<string, Player>(players);
            foreach (Player p in playersCopy.Values)
            {
                PlayerData pd = PlayerDataHandler.Instance[p.truename];
                if (pd == null) continue;
                if (DateTime.Now - pd.lastWeaponSpeedChange > TimeSpan.FromMilliseconds(200))
                {
                    ClearWeaponSpeed(p);
                }
            }
        }
    }
}
