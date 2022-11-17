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
using FPSMO.Configuration;

namespace FPSMO
{
    internal static class LevelPicker
    {
        static private Random _random = new Random();
        static private List<string> _maps = new List<string>();
        static private bool _hasMapVoteQueued = false;
        static private string _mapVoteQueued;

        static internal string RandomMap => _maps[_random.Next(_maps.Count)];

        static internal void Activate()
        {
            _maps = FPSMOConfig<FPSMOGameConfig>.Read("Config").MAPS;    // Is this thread-safe?
            if (_maps.Count == 0) _maps = new List<string> { Server.Config.MainLevel };
        }

        static internal bool MapExists(string map)
        {
            Console.WriteLine(_maps.Contains(map));
            return _maps.Contains(map);
        }

        static internal void VoteQueue(string map)
        {
            if (!MapExists(map))
                    throw new ArgumentException($"There is no map called {map} in the current map cycle.");

            _hasMapVoteQueued = true;
            _mapVoteQueued = map;
        }

        static internal List<string> PickVotingMaps()
        {
            if (_maps.Count == 1)
                return new List<string>() { _maps[0], _maps[0], _maps[0] };
            else if (_maps.Count == 2)
                return new List<string>() { _maps[0], _maps[1], _maps[1] };

            List<string> mapsPool;
            List<int> indexes;

            if (_hasMapVoteQueued)
            {
                mapsPool = new List<string>(_maps);
                mapsPool.Remove(_mapVoteQueued);
                indexes = Utils.RandomSubset(mapsPool.Count, 2);
                indexes.Add(_maps.IndexOf(_mapVoteQueued));
            }
            else
            {
                mapsPool = _maps;
                indexes = Utils.RandomSubset(mapsPool.Count, 3);
            }

            var pickedMaps = new List<string>();

            foreach (int index in indexes)
                pickedMaps.Add(_maps[index]);

            _hasMapVoteQueued = false;
            return pickedMaps;
        }
    }
}
