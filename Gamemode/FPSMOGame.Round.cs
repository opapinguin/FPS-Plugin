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
using System.Threading;
using FPSMO.Weapons;
using MCGalaxy;

namespace FPSMO
{
    /// <summary>
    /// Round-specific functions. The round is the second stage of the game, between the countdown and the voting
    /// </summary>
    public sealed partial class FPSMOGame
    {
        /*************
         * BEGINNING *
         *************/
        #region begin
        private void BeginRound()
        {
            WeaponAnimsHandler.Activate();

            // Move on to the next sub-stage
            subStage = SubStage.Middle;
            OnRoundStarted();
        }

        #endregion
        /**********
         * MIDDLE *
         **********/
        #region middle
        private void MiddleRound()
        {
            if (DateTime.UtcNow >= roundStart + roundTime) {
                // Move on to the next sub-stage
                subStage = SubStage.End;
                return;
            }

            // The below line is generally bad practice, and indeed we therefore require that updateRound() does the minimum work possible
            // The animation loops and other events are handled by scheduler tasks on other threads and don't just sleep like this
            Thread.Sleep(MS_ROUND_TICK);

            DateTime roundEnd = roundStart + roundTime;
            TimeSpan timeLeft = roundEnd - DateTime.UtcNow;
            OnRoundTicked((int) timeLeft.TotalSeconds);
        }

        #endregion
        /*******
         * END *
         *******/
        #region end
        private void EndRound()
        {
            WeaponAnimsHandler.Deactivate();

            // Move on to the next sub-stage and stage
            stage = Stage.Voting;
            subStage = SubStage.Begin;

            OnRoundEnded();
        }

        #endregion
    }
}
