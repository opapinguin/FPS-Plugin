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
using System.Linq;
using BlockID = System.UInt16;

namespace FPSMO.Configuration
{
    public class FPSMOGameConfig {
        public FPSMOGameConfig()
        {
            // DEFAULT VALUES
            MAPS = new List<string>
            {
                Server.Config.MainLevel
            };

            this.B_AUTO_START = true;

            MS_ROUND_TICK = 50;
            S_VOTETIME = 10;
            MAX_MOVE_DISTANCE = 1.5625f;
            B_SET_MAIN_LEVEL = true;
            DEFAULT_ROUNDTIME_S = 60;
            GRAVITY = 9.81f;
            
            MS_UPDATE_ROUND_STATUS = 50;
            MS_UPDATE_WEAPON_ANIMATIONS = 50;

            // Guns
            GUN_BLOCK = 41;
            MIN_GUN_VELOCITY = 50;
            MAX_GUN_VELOCITY = 300;
            MS_GUN_RELOAD = 200;
            GUN_DAMAGE = 1;
            GUN_FRAME_LENGTH = 1;

            // Rockets
            ROCKET_BLOCK = 42;
            MIN_ROCKET_VELOCITY = 10;
            MAX_ROCKET_VELOCITY = 60;
            MS_ROCKET_RELOAD = 2000;
            ROCKET_DAMAGE = 1;
            ROCKET_FRAME_LENGTH = 4f;
        }

        public List<string> MAPS { get; set; }

        public bool B_AUTO_START { get; set; }
        public bool B_SET_MAIN_LEVEL { get; set; }
        public uint MS_ROUND_TICK { get; set; }
        public uint S_VOTETIME { get; set; }
        public float MAX_MOVE_DISTANCE { get; set; }
        public uint DEFAULT_ROUNDTIME_S { get; set; }

        public float GRAVITY { get; set; }

        public uint MS_UPDATE_WEAPON_ANIMATIONS { get; set; }
        public uint MS_UPDATE_ROUND_STATUS { get; set; }

        // Guns
        public BlockID GUN_BLOCK { get; set; }
        public float MIN_GUN_VELOCITY { get; set; }   // in meters per second
        public float MAX_GUN_VELOCITY { get; set; }   // in meters per second
        public uint MS_GUN_RELOAD { get; set; }
        public uint GUN_DAMAGE { get; set; }
        public float GUN_FRAME_LENGTH { get; set; }  // Number of frames "long" a single shot is

        // Rockets
        public BlockID ROCKET_BLOCK { get; set; }
        public float MIN_ROCKET_VELOCITY { get; set; }
        public float MAX_ROCKET_VELOCITY { get; set; }
        public uint MS_ROCKET_RELOAD { get; set; }
        public uint ROCKET_DAMAGE { get; set; }
        public float ROCKET_FRAME_LENGTH { get; set; }
    }
}
