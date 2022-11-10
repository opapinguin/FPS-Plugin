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

using System.Collections.Generic;

namespace FPSMO.Configuration
{
    public struct FPSMOGameConfig {
        public FPSMOGameConfig(bool bAutoStart)
        {
            this.maps = new List<string>();
            // DEFAULT VALUES
            this.bAutoStart = bAutoStart;
            this.MS_ROUND_TICK = 100;
            this.S_COUNTDOWNTIME = 10;
            this.S_VOTETIME = 10;
            this.MAX_MOVE_DISTANCE = 1.5625f;
            this.bSetMainLevel = true;
            this.DEFAULT_ROUNDTIME_S = 60;
        }
        public bool bAutoStart;
        public bool bSetMainLevel;
        public List<string> maps;
        public int MS_ROUND_TICK;
        public int S_COUNTDOWNTIME;
        public int S_VOTETIME;
        public float MAX_MOVE_DISTANCE;
        public int DEFAULT_ROUNDTIME_S;
    }
}
