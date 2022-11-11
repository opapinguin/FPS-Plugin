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

using MCGalaxy;
using System.Collections.Generic;
using BlockID = System.UInt16;

namespace FPSMO.Configuration
{
    public struct FPSMOGameConfig {
        public FPSMOGameConfig(bool bAutoStart)
        {
            // DEFAULT VALUES
            maps = new List<string>
            {
                Server.Config.MainLevel
            };

            this.bAutoStart = bAutoStart;

            MS_ROUND_TICK = 100;
            S_COUNTDOWNTIME = 10;
            S_VOTETIME = 10;
            MAX_MOVE_DISTANCE = 1.5625f;
            bSetMainLevel = true;
            DEFAULT_ROUNDTIME_S = 60;
            
            MS_UPDATE_ROUND_STATUS = 50;
            MS_UPDATE_WEAPON_ANIMATIONS = 50;

            // Guns
            GUN_BLOCK = 41;
            MS_GUN_VELOCITY = 1;
            MS_GUN_RELOAD_MS = 10;
            GUN_DAMAGE = 1;
        }

        public bool bAutoStart;
        public bool bSetMainLevel;
        public List<string> maps;
        public uint MS_ROUND_TICK;
        public uint S_COUNTDOWNTIME;
        public uint S_VOTETIME;
        public float MAX_MOVE_DISTANCE;
        public uint DEFAULT_ROUNDTIME_S;

        public uint MS_UPDATE_WEAPON_ANIMATIONS;
        public uint MS_UPDATE_ROUND_STATUS;

        // Guns
        public BlockID GUN_BLOCK;
        public float MS_GUN_VELOCITY;   // in meters per second
        public uint MS_GUN_RELOAD_MS;
        public uint GUN_DAMAGE;
    }
}
