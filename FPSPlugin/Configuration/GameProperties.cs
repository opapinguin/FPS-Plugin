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

using System.IO;
using System.Text;

namespace FPS.Configuration;

internal class GameProperties
{
    internal bool AutoStart { get; set; } = true;
    internal uint CountdownDurationSeconds { get; set; } = 10u;
    internal uint DefaultRoundDurationSeconds { get; set; } = 60u;
    internal uint VoteDurationSeconds { get; set; } = 10u;
    internal uint AFKNoticeSeconds { get; set; } = 110u;
    internal uint AFKModeSeconds { get; set; } = 120u;
    internal uint MapHistory { get; set; } = 1u;
    internal bool TeamVersusTeamMode { get; set; } = false;

    public override string ToString()
    {
        StringBuilder stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"autostart = {AutoStart}");
        stringBuilder.AppendLine($"countdown_duration_seconds = {CountdownDurationSeconds}");
        stringBuilder.AppendLine($"vote_duration_seconds = {VoteDurationSeconds}");
        stringBuilder.AppendLine($"default_round_duration = {DefaultRoundDurationSeconds}");
        stringBuilder.AppendLine($"afk_notice_seconds = {AFKNoticeSeconds}");
        stringBuilder.AppendLine($"afk_mode_seconds = {AFKModeSeconds}");
        stringBuilder.AppendLine($"map_history = {MapHistory}");
        stringBuilder.AppendLine($"team_vs_team_mode = {TeamVersusTeamMode}");

        return stringBuilder.ToString();
    }

    internal static GameProperties Default()
    {
        return new GameProperties();
    }

    internal static GameProperties Load(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException();
        }

        string[] lines = System.IO.File.ReadAllLines(path);
        GamePropertiesParser parser = new GamePropertiesParser();
        return parser.Parse(lines);
    }

    internal static void Save(GameProperties properties,
                              string directory, string fileName = "game.properties")
    {
        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException();
        }

        string path = Path.Combine(directory, fileName);
        System.IO.File.WriteAllText(path, properties.ToString());
    }
}
