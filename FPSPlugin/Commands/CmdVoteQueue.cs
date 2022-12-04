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

using FPS.Configuration;
using FPS.DB;
using MCGalaxy;
using System;
using System.IO;

namespace FPS.Commands;

internal class CmdVoteQueue : Command2
{
    public override string name { get { return "VoteQueue"; } }
    public override string type { get { return CommandTypes.Games; } }
    public override bool SuperUseable { get { return false; } }

    private DatabaseManager _databaseManager;
    private LevelPicker _levelPicker;

    internal CmdVoteQueue(DatabaseManager databaseManager, LevelPicker levelPicker)
    {
        _databaseManager = databaseManager;
        _levelPicker = levelPicker;
    }

    public override void Use(Player p, string message)
    {
        if (message is null || message == "")
        {
            p.Message("&HUsage: &T/votequeue <map>&H.");
            return;
        }

        if (!_databaseManager.IsInMapsPool(message))
        {
            p.Message($"&SThere is no map &T\"{message}\" in the maps pool.");
            return;
        }

        if (_levelPicker.HasMapQueued)
        {
            p.Message($"&WCould not vote-queue &T{message}&W: there is already a map queued.");
            return;
        }
        else if (_levelPicker.HasMapVoteQueued)
        {
            p.Message($"&WCould not vote-queue &T{message}&W: there is already a map vote-queued.");
            return;
        }

        _levelPicker.VoteQueue(message);
        Chat.MessageAll($"&T{message} &Swill be included in next vote.");
    }

    public override void Help(Player p)
    {
        p.Message("&T/votequeue <map>");
        p.Message("&H<map> will be included to the votes at the end of this round.");
    }
}
