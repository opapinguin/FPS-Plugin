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
using MCGalaxy;
using FPS.Configuration;
using FPS.DB;

namespace FPS;

internal class LevelPicker
{
    private Random _random = new Random();
    internal bool HasMapVoteQueued { get; private set; } = false;
    private string _mapVoteQueued;
    internal bool HasMapQueued { get; private set; } = false;
    private string _mapQueued;
    private DatabaseManager _databaseManager;
    private readonly int _historySize;
    internal List<string> LastMapsPlayed { get; private set; }

    internal LevelPicker(DatabaseManager databaseManager, int historySize)
    {
        _databaseManager = databaseManager;

        if (historySize >= 0)
        {
            _historySize = historySize;
        }
        else
        {
            Logger.Log(LogType.Warning, "Can't use {historySize} for map_history. Using 0 instead...");
            _historySize = 0;
        }

        LastMapsPlayed = new List<string>();
    }

    internal void VoteQueue(string map)
    {
        HasMapVoteQueued = true;
        _mapVoteQueued = map;
    }

    internal void Queue(string map)
    {
        HasMapQueued = true;
        _mapQueued = map;
    }

    internal void Register(string map)
    {
        LastMapsPlayed.Add(map);

        if (LastMapsPlayed.Count > _historySize)
            LastMapsPlayed.RemoveAt(0);
    }

    private string Cycle(List<string> mapPool, string lastPlayed)
    {
        if (lastPlayed is null) return mapPool[0];
        int mapIndex = mapPool.IndexOf(lastPlayed);

        if (mapIndex == -1)
        {
            return mapPool[0];
        }
        else
        {
            string nextMap = mapPool[(mapIndex + 1) % mapPool.Count];
            return nextMap;
        }
    }

    private List<string> Deterministic(string item)
    {
        return new List<string>() { item };
    }

    internal List<string> PickVotingMaps()
    {
        List<string> mapsPool = _databaseManager.GetMapsPool().ToList(); ;
        bool isHistorySaturated = (_historySize >= mapsPool.Count - 1);

        if (mapsPool.Count == 0) return new List<string>();

        if (HasMapQueued)
        {
            HasMapQueued = false;
            return Deterministic(_mapQueued);
        }
        else if (isHistorySaturated)
        {
            return Deterministic(Cycle(mapsPool, LastMapsPlayed.LastOrDefault()));
        }
        else if (mapsPool.Count == 2)
        {
            return mapsPool.ToList();
        }

        List<int> indexes;
        var pickedMaps = new List<string>();

        if (HasMapVoteQueued)
        {
            var mapsPoolReduced = new List<string>(mapsPool);
            mapsPoolReduced.Remove(_mapVoteQueued);
            indexes = Utils.RandomSubset(mapsPoolReduced.Count, 2);

            foreach (int index in indexes)
                pickedMaps.Add(mapsPoolReduced[index]);

            pickedMaps.Add(_mapVoteQueued);
            HasMapVoteQueued = false;
        }
        else
        {
            indexes = Utils.RandomSubset(mapsPool.Count, 3);

            foreach (int index in indexes)
                pickedMaps.Add(mapsPool[index]);

        }

        return pickedMaps;
    }
}
