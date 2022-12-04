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
using MCGalaxy.Commands;
using System.IO;

namespace FPS.Commands;

internal class CmdRate : Command2
{
    public override string name { get { return "Rate"; } }
    public override string type { get { return CommandTypes.Games; } }
    public override bool SuperUseable { get { return false; } }

    private DatabaseManager _databaseManager;

    internal CmdRate(DatabaseManager databaseManager)
    {
        _databaseManager = databaseManager;
    }

    public override void Use(Player player, string message, CommandData data)
    {
        if (message == "")
        {
            Help(player);
            return;
        }
        else if (message.ToLower() == "remove")
        {
            RemoveRating(player.level, player);
            return;
        }

        int rating = 0;

        if (!CommandParser.GetInt(player, message, "Rating", ref rating, min: 0, max: 5))
            return;

        Level level = player.level;

        if (IsLevelAuthor(level, player))
        {
            player.Message($"Cannot rate {level.name} as you are an author of it"); return;
        }

        int? previousRating = _databaseManager.GetRating(level.name, player);
        _databaseManager.SetRating(level.name, player, rating);

        if (previousRating is null)
        {
            player.Message("&SThank you for rating this map.");
            player.Message($"&SYour rating: &T{rating}/5&S.");
        }
        else if (previousRating != rating)
        {
            player.Message($"&SYou have updated your rating to &T{rating}/5&S.");
        }
        else
        {
            player.Message($"&WYour rating for this map is already &T{rating}/5&S.");
        }
    }

    private void RemoveRating(Level level, Player player)
    {
        int? previousRating = _databaseManager.GetRating(level.name, player);

        if (previousRating is null)
        {
            player.Message("&WYour haven't rated this map yet.");
        }
        else
        {
            _databaseManager.RemoveRating(level.name, player);
            player.Message($"&SYour rating on &T{level.name} &Swas removed.");
        }

    }

    private bool IsLevelAuthor(Level level, Player player)
    {
        string[] authors = level.Config.Authors.SplitComma();
        return authors.CaselessContains(player.truename);
    }

    public override void Help(Player p)
    {
        p.Message("&T/rate <0-5> &H- rates current map.");
        p.Message("&T/rate remove &H- removes your rating.");
    }
}
