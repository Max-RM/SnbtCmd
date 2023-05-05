using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryashtarUtils.Utility
{
    public static class MathUtils
    {
        // multiplies time duration by a percentage 0 to 1
        public static TimeSpan TimePercent(double fraction, TimeSpan total)
        {
            return TimeSpan.FromTicks((long)(total.Ticks * fraction));
        }

        // divides two time durations
        public static double PercentTime(TimeSpan current, TimeSpan total)
        {
            return (double)current.Ticks / total.Ticks;
        }

        // helper for inlining clamps
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0)
                return min;
            else if (val.CompareTo(max) > 0)
                return max;
            else
                return val;
        }

        // how I like timespans to look
        public static string StringTimeSpan(TimeSpan time, int decimals = 0)
        {
            // create decimal part if requested
            string ending = decimals > 0 ? time.ToString(new string('F', decimals)) : "";
            // only include the dot if there will be digits after it
            if (ending != "")
                ending = "." + ending;
            if (time.TotalMinutes < 10)
                return time.ToString(@"m\:ss") + ending;
            if (time.TotalHours < 1)
                return time.ToString(@"mm\:ss") + ending;
            return time.ToString(@"h\:mm\:ss") + ending;
        }

        // random number in a range, but exclude one value from potentially being generated
        public static int RandomExcept(Random rand, int lower, int upper, int disallowed)
        {
            int value = rand.Next(lower, upper - 1);
            if (value >= disallowed)
                value++;
            return value;
        }
    }
}
