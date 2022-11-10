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
using MCGalaxy.Events;
using System.Collections.Generic;

namespace FPSMO
{
    public enum Achievement : ushort
    {
        ACHIEVEMENT_TEST = 0,
    }

    public enum AwardEvent : ushort
    {
        EVENT_TEST = 0,
    }

    public interface IObserver {
        void OnNotify(ref Player player, AwardEvent award);
    }

    class AwardSubject
    {
        public void Attach(ref IObserver observer)
        {
            _observers.Add(observer);
            _numAchievements += 1;
        }
        public void Detach(ref IObserver observer)
        {
            _observers.Remove(observer);
            _numAchievements -= 1;
        }
        public void Notify()
        {

        }

        private List<IObserver> _observers = new List<IObserver>();
        private int _numAchievements = 0;
    }

    public class Achievements : IObserver
    {
        public void OnNotify(ref Player player, AwardEvent award)
        {
            switch (award)
            {
                case AwardEvent.EVENT_TEST:
                    unlock(Achievement.ACHIEVEMENT_TEST);
                    return;
                default:
                    return;
            }
        }

        private void unlock(Achievement achievement)
        {
            // TODO: Work on this
        }
    }
}