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
using FPSMO.Configuration;

namespace FPSMO
{
    /// <summary>
    /// Voting-specific functions. The voting is the third stage of the game, after the round
    /// </summary>
    internal sealed partial class FPSMOGame
    {
        /*************
         * BEGINNING * 
         *************/
        #region begin
        internal string map1, map2, map3;
        internal uint votes1, votes2, votes3;
        private void BeginVoting()
        {
            List<string> pickedMaps = LevelPicker.PickVotingMaps();

            if (pickedMaps.Count == 0)
            {
                Stop();
            }
            else if (pickedMaps.Count == 1)
            {
                string mapName = pickedMaps[0];
                StartRound(mapName);
                return;
            }
            else if (pickedMaps.Count == 2)
            {
                map1 = pickedMaps[0];
                map2 = pickedMaps[1];
                map3 = null;

                subStage = SubStage.Middle;
                OnVoteStarted(map1, map2, null, count: 2);
                return;
            }

            map1 = pickedMaps[0];
            map2 = pickedMaps[1];
            map3 = pickedMaps[2];

            subStage = SubStage.Middle;
            OnVoteStarted(map1, map2, map3, count: 3);
        }

        #endregion
        /**********
         * MIDDLE *
         **********/
        #region middle
        private void MiddleVoting()
        {
            for (uint i = _gameProperties.VoteDurationSeconds; i > 0; i--)
            {
                if (!bRunning) return;
                OnVoteTicked((int)i);

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
            string nextMap = GetNextMap();
            OnVoteEnded();
            StartRound(nextMap);
        }

        #endregion

        /******************
         * HELPER METHODS *
         ******************/
        #region Helper Methods

        private void Vote(Player player, int mapNumber)
        {
            PlayerData playerData = PlayerDataHandler.Instance[player.truename];

            if (playerData.bVoted)
            {
                int previousVote = (int)playerData.vote;

                switch (previousVote)
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
            }

            playerData.vote = (ushort)mapNumber;

            switch (mapNumber)
            {
                case 1:
                    votes1++;
                    break;
                case 2:
                    votes2++;
                    break;
                case 3:
                    votes3++;
                    break;
            }

            playerData.bVoted = true;
            player.Message($"&SYour vote: &T{mapNumber}&S.");
        }

        private string GetNextMap()
        {
            int count = (map3 is null) ? 2 : 3;
            int[] votes;
            List<int> indexes;

            if (count == 2)
                votes = new int[] { (int)votes1, (int)votes2 };
            else
                votes = new int[] { (int)votes1, (int)votes2, (int)votes3 };

            indexes = Utils.ArgMaxAllIndexes(votes);
            var maps = new List<string>();

            if (indexes.Contains(0)) maps.Add(map1);
            if (indexes.Contains(1)) maps.Add(map2);
            if (indexes.Contains(2)) maps.Add(map3);

            Random rand = new Random();
            int index = rand.Next(indexes.Count);
            return maps[index];
        }

        private void ResetVotes()
        {
            votes1 = votes2 = votes3 = 0;
            map1 = map2 = map3 = "";
        }

        private void MovePlayersToNextMap(string map)
        {
            List<Player> playersList = players.Values.ToList();

            foreach (Player p in playersList)
            {
                PlayerActions.ChangeMap(p, map);
            }
        }

        #endregion
    }
}
