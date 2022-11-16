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
        static Random rand = new Random();
        static List<string> maps = new List<string>();
        static List<string> queue = new List<string>();

        static internal void Activate()
        {
            maps = FPSMOConfig<FPSMOGameConfig>.Read("Config").MAPS;    // Is this thread-safe?

            Logger.Log(LogType.ConsoleMessage, "test");
            Logger.Log(LogType.ConsoleMessage, maps.Count.ToString());

            if (maps.Count == 0)
            {
                maps = new List<string>
                {
                    Server.Config.MainLevel
                };
            }

            // Add 10 random levels to the queue
            for (int i = 0; i < 10; i++)
            {
                int index = rand.Next(maps.Count);
                queue.Add(maps[index]);
            }
        }

        static internal List<string> GetQueue()
        {
            return maps;
        }

        static internal void UpdateMaps()
        {
            maps = FPSMOConfig<FPSMOGameConfig>.Read("config").MAPS;
        }

        static internal void AddQueueLevel(string map)
        {
            for (int i = maps.Count - 1; i > 0; i--)
            {
                queue[i] = queue[i - 1];
            }
            queue[0] = map;
        }

        /// <summary>
        /// Retrieves the last level and pushes a new one onto the queue
        /// Basically gets the next level
        /// </summary>
        static internal string PopAndPush()
        {
            string newMap = queue[0];
            queue.RemoveAt(0);
            int index = rand.Next(maps.Count);
            queue.Add(maps[index]);
            return newMap;
        }
    }
}
