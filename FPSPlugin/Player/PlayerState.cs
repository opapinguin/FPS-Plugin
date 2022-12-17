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

namespace FPS.Entities;

/// <summary>
/// The state interface. When you invoke a transition the default is not to transition at all, unless overriden
/// </summary>
internal abstract class PlayerState
{
    protected PlayerData context;    // Would have preferred the context to be the Player class but we can't access that.

    internal void SetContext(PlayerData pd)
    {
        this.context = pd;
    }

    internal virtual void HandleLandEvent() { }
    internal virtual void HandleJumpEvent() { }
    internal virtual void HandleNormalEvent() { }
    internal virtual void HandleRunEvent() { }
    internal virtual void HandleCrawlEvent() { }
    internal virtual void HandleLedgeGrabEvent() { }
}

internal class NormalState : PlayerState
{
    internal override void HandleJumpEvent()
    {
        this.context.TransitionTo(new JumpingState());
    }

    internal override void HandleRunEvent()
    {
        // TODO: Check stamina here
        this.context.TransitionTo(new RunningState());
    }

    internal override void HandleCrawlEvent()
    {
        this.context.TransitionTo(new CrawlingState());
    }
}

internal class JumpingState : PlayerState
{
    internal override void HandleLandEvent()
    {
        this.context.TransitionTo(new NormalState());
    }

    internal override void HandleLedgeGrabEvent()
    {
        // TODO: Check stamina here
        this.context.TransitionTo(new LedgeGrabState());
    }
}

internal class LedgeGrabState : PlayerState
{
    internal override void HandleLedgeGrabEvent()
    {
        this.context.TransitionTo(new NormalState());
    }
}

internal class RunningState : PlayerState
{
    internal override void HandleRunEvent()
    {
        this.context.TransitionTo(new NormalState());
    }

    internal override void HandleJumpEvent()
    {
        this.context.TransitionTo(new JumpingState());
    }
}

internal class CrawlingState : PlayerState
{
}

