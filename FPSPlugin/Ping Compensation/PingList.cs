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

namespace FPS.PingCompensation;

/// <summary>
/// Template for lists with timestamps e.g., location, rotation etc
/// Can linearly interpolate to get the value at a specific time
/// </summary>
internal class PingList<T>
{
    struct TimeStamp
    {
        internal TimeStamp(DateTime dt, T val)
        {
            time = dt;
            value = val;
        }
        internal TimeStamp(T val)
        {
            time = DateTime.Now;
            value = val;
        }
        internal DateTime time { get; }
        internal T value { get; }
    }

    List<TimeStamp> timeStamps;
    TimeSpan delay;

    internal PingList(int capacity, int delay)
    {
        this.delay = TimeSpan.FromMilliseconds(delay);

        timeStamps = new List<TimeStamp>(capacity);
    }

    internal void Add(DateTime t, T val)
    {
        timeStamps.Insert(0, new TimeStamp(t, val));
        timeStamps.RemoveAt(timeStamps.Count - 1);
    }

    internal T GetAt(DateTime t)
    {
        for (int i = 0; i < timeStamps.Count - 2; i++)
        {
            // If inbetween time stamps
            if ((timeStamps[i].time <= t) && (timeStamps[i].time + delay > t))
            {
                // Parametrize the "line" connecting the two times
                double x = ((timeStamps[i].time + delay - t).TotalMilliseconds
                    / delay.TotalMilliseconds);

                dynamic val1 = timeStamps[i].value; // C# doesn't have generic operators so can't just add
                dynamic val2 = timeStamps[i].value;
                //return (x * val1 + (1 - x) * val2);
            }
        }
        return default;
    }
}
