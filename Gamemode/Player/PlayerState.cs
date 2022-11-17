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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPSMO.Entities
{
    /// <summary>
    /// The state interface. When you invoke a transition the default is not to transition at all, unless overriden
    /// </summary>
    internal abstract class PlayerState
    {
        protected PlayerData context;    // Would have preferred the context to be the Player class but we can't access that.

        public void SetContext(PlayerData pd)
        {
            this.context = pd;
        }

        public virtual void HandleLandEvent() { }
        public virtual void HandleJumpEvent() { }
        public virtual void HandleNormalEvent() { }
        public virtual void HandleRunEvent() { }
        public virtual void HandleCrawlEvent() { }
        public virtual void HandleLedgeGrabEvent() { }
    }

    class NormalState : PlayerState
    {
        public override void HandleJumpEvent()
        {
            this.context.TransitionTo(new JumpingState());
        }

        public override void HandleRunEvent()
        {
            // TODO: Check stamina here
            this.context.TransitionTo(new RunningState());
        }

        public override void HandleCrawlEvent()
        {
            this.context.TransitionTo(new CrawlingState());
        }
    }

    class JumpingState : PlayerState
    {
        public override void HandleLandEvent()
        {
            this.context.TransitionTo(new NormalState());
        }

        public override void HandleLedgeGrabEvent()
        {
            // TODO: Check stamina here
            this.context.TransitionTo(new LedgeGrabState());
        }
    }

    class LedgeGrabState : PlayerState
    {
        public override void HandleLedgeGrabEvent()
        {
            this.context.TransitionTo(new NormalState());
        }
    }

    class RunningState : PlayerState
    {
        public override void HandleRunEvent()
        {
            this.context.TransitionTo(new NormalState());
        }

        public override void HandleJumpEvent()
        {
            this.context.TransitionTo(new JumpingState());
        }
    }

    class CrawlingState : PlayerState
    {
    }

}
