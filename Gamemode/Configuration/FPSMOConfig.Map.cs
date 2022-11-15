﻿/*
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
using MCGalaxy.Maths;

namespace FPSMO.Configuration
{
    public struct SpawnPoint
    {
        public string team;
        public Vec3U16 loc;
    }

    public struct FPSMOMapConfig
    {
        public FPSMOMapConfig(uint RoundTime)
        {
            // DEFAULT VALUES
            this.ROUND_TIME_S = RoundTime;
            this.TOTAL_RATINGS = 0;
            this.SUM_RATINGS = 0;
            this.SPAWNPOINTS = new List<SpawnPoint>();
            this.COUNTDOWN_TIME = TimeSpan.FromSeconds(10);
            this.TEAM_VS_TEAM = true;
        }
        public float TOTAL_RATINGS;
        public float SUM_RATINGS;
        public uint ROUND_TIME_S;
        public List<SpawnPoint> SPAWNPOINTS;
        public TimeSpan COUNTDOWN_TIME;
        public bool TEAM_VS_TEAM;

        public float Rating {
            get {
                return (SUM_RATINGS / TOTAL_RATINGS) == float.NaN ? 0 : (SUM_RATINGS / TOTAL_RATINGS);
            }
        }
    }
}
