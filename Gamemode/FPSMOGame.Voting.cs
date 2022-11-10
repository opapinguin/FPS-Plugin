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

using MCGalaxy.Events.PlayerEvents;
using MCGalaxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FPSMO.Entities;
using System.Threading;

namespace FPSMO
{
    /// <summary>
    /// Voting-specific functions. The voting is the third stage of the game, after the round
    /// </summary>
    public sealed partial class FPSMOGame
    {
        /*************
         * BEGINNING * 
         *************/
        #region begin
        string map1, map2, map3;
        uint votes1, votes2, votes3;
        private void BeginVoting()
        {
            map1 = LevelPicker.PopAndPush();
            map2 = LevelPicker.PopAndPush();
            map3 = LevelPicker.PopAndPush();

            OnPlayerChatEvent.Register(HandleVoting, Priority.High);

            // Move on to the next sub-stage
            subStage = SubStage.Middle;
        }

        #endregion
        /**********
         * MIDDLE *
         **********/
        #region middle
        private void MiddleVoting()
        {
            ShowToAll(ShowVote);
            for (int i = gameConfig.S_VOTETIME; i >= 0; i--)
            {
                if (!bRunning) return;
                ShowToAll(ShowVoteTime);
                Thread.Sleep(1000); // Sleep 1 second
            }

            // Move on to the next sub-stage
            subStage = SubStage.End;
        }

        #endregion
        /*******
         * END *
         *******/
        #region end
        private void EndVoting()
        {
            MessageMap(CpeMessageType.Normal, String.Format("Votes are in! map 1: {0} map 2: {1} map 3: {2}", votes1, votes2, votes3));
            ShowToAll(ClearBottomRight);

            OnPlayerChatEvent.Unregister(HandleVoting); // TODO: Probably just want all this in the plugins sections for elegance

            string nextMap;

            nextMap = GetNextMap();

            // TODO: Save Stats

            PlayerDataHandler.Instance.ResetPlayerData();
            ResetVotes();

            MoveToNextMap(nextMap);

            // Move on to the next sub-stage and stage
            stage = Stage.Countdown;
            subStage = SubStage.Begin;
        }

        #endregion

        /******************
         * HELPER METHODS *
         ******************/
        #region Helper Methods

        private void HandleVoting(Player p, string msg)
        {
            msg = msg.ToLower().TrimEnd();
            UpdateVote(msg, p, "1", "one", ref votes1);
            UpdateVote(msg, p, "2", "two", ref votes2);
            UpdateVote(msg, p, "3", "three", ref votes3);
        }

        /// <summary>
        /// Handles a message like "1" or "2" in one function. Updates the vote based on the message
        /// </summary>
        private void UpdateVote(string msg, Player p, string numInt, string numStr, ref uint votes)
        {
            if (msg == numInt || msg == numStr)
            {
                if (PlayerDataHandler.Instance[p.name].bVoted)
                {
                    votes++;
                    ushort prevVote = PlayerDataHandler.Instance[p.name].vote;
                    switch (prevVote)
                    {
                        case 1:
                            votes1--;
                            break;
                        case 2:
                            votes2--;
                            break;
                        case 3:
                            votes3--;
                            break;
                    }

                    PlayerDataHandler.Instance[p.name].vote = ushort.Parse(numInt);
                } else
                {
                    votes += 1;
                    PlayerDataHandler.Instance[p.name].bVoted = true;
                    PlayerDataHandler.Instance[p.name].vote = ushort.Parse(numInt);
                }
                p.Message("Thank you for voting");
                p.cancelchat = true;
            }
        }

        private string GetNextMap()
        {
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            int index = rand.Next(3);

            if (votes3 > votes2 && votes2 >= votes1)
            {
                return map3;
            }
            else if (votes2 > votes1 && votes1 >= votes3)
            {
                return map2;
            }
            else if (votes1 > votes3 && votes3 >= votes2)
            {
                return map1;
            } else
            {
                // votes1 == votes2 == votes3, only way to get here
                return index == 0 ? map1 : (index == 1 ? map2 : map3);
            }
        }

        private void ResetVotes()
        {
            votes1 = votes2 = votes3 = 0;
            map1 = map2 = map3 = "";
        }

        private void MoveToNextMap(string map)
        {
            foreach (Player p in players)
            {
                PlayerActions.ChangeMap(p, map);
            }
        }

        #endregion
    }
}
