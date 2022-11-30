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
using FPSMO.DB;
using MCGalaxy;
using System;
using System.IO;

namespace FPSMO.Commands
{
    internal class CmdVoteQueue : Command2
    {
        public override string name { get { return "VoteQueue"; } }
        public override string type { get { return CommandTypes.Games; } }
        public override bool SuperUseable { get { return false; } }

        private DateTime? _lastVoteQueue = null;
        private TimeSpan _spanBetweenUses = new TimeSpan(hours: 0, minutes: 15, seconds: 0);

        internal bool CanUse => (_lastVoteQueue is null || (DateTime.Now - _lastVoteQueue) > _spanBetweenUses);

        private DatabaseManager _databaseManager;

        internal CmdVoteQueue(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        public override void Use(Player p, string message)
        {
            if (message is null || message == "")
            {
                p.Message("&Husage: &T/votequeue <map>");
                return;
            }

            if (!CanUse)
            {
                p.Message("&SCannot run &T/VoteQueue &Sbecause it was already run recently.");
                return;
            }

            if (!_databaseManager.IsInMapPool(message))
            {
                p.Message($"&SThere is no map &T\"{message}\" in the current map cycle.");
                return;
            }

            _lastVoteQueue = DateTime.Now;
            LevelPicker.VoteQueue(message);
        }

        public override void Help(Player p)
        {
            p.Message("&T/votequeue <map>");
            p.Message("&H<map> will be included to the votes at the end of this round.");
        }
    }
}
