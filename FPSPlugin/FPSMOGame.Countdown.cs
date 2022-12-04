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

using FPS.Configuration;
using MCGalaxy;
using MCGalaxy.Games;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FPS;

/// <summary>
/// Countdown-specific functions. The countdown is the first stage of the game, before the round
/// </summary>
internal sealed partial class FPSMOGame
{
    /*************
     * BEGINNING *
     *************/
    #region begin
    private void BeginCountdown(uint delay)
    {
        roundStart = DateTime.Now.AddSeconds(delay);

        // Move on to the next sub-stage
        subStage = SubStage.Middle;

        OnCountdownStarted();
    }

    #endregion
    /**********
     * MIDDLE *
     **********/
    #region middle

    private void MiddleCountdown(uint delay)
    {
        // TODO: change this back to 2
        int minimumPlayersCount = 1;

        for (int i = (int)(roundStart - DateTime.Now).TotalSeconds; i > 0; i--)
        {
            if (!bRunning) return;
            OnCountdownTicked((int) i, players.Count >= minimumPlayersCount);
            Thread.Sleep(1000);
        }

        if (players.Count >= minimumPlayersCount)
        {
            subStage = SubStage.End;
        }
        else
        {
            roundStart = DateTime.Now.AddSeconds(delay);
        }
    }

    #endregion
    /*******
     * END *
     ******/
    #region end

    private void EndCountdown()
    {
        // Move on to the next sub-stage and stage
        stage = Stage.Round;
        subStage = SubStage.Begin;

        OnCountdownEnded();
    }

    #endregion
}