using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace TryashtarUtils.Utility
{
    public static class DataUtils
    {
        private static readonly Dictionary<string, (double d, float f)> SpecialFloatingPoints = new(StringComparer.OrdinalIgnoreCase)
        {
            { "∞", (double.PositiveInfinity, float.PositiveInfinity) },
            { "+∞", (double.PositiveInfinity, float.PositiveInfinity) },
            { "-∞", (double.NegativeInfinity, float.NegativeInfinity) },
            { "Infinity", (double.PositiveInfinity, float.PositiveInfinity) },
            { "+Infinity", (double.PositiveInfinity, float.PositiveInfinity) },
            { "-Infinity", (double.NegativeInfinity, float.NegativeInfinity) },
            { "NaN", (double.NaN, float.NaN) }
        };

        public static double? TryParseSpecialDouble(string value)
        {
            if (SpecialFloatingPoints.TryGetValue(value, out var result))
                return result.d;
            return null;
        }

        public static float? TryParseSpecialFloat(string value)
        {
            if (SpecialFloatingPoints.TryGetValue(value, out var result))
                return result.f;
            return null;
        }

        public static double ParseDouble(string value)
        {
            return TryParseSpecialDouble(value) ??
                double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        public static float ParseFloat(string value)
        {
            return TryParseSpecialFloat(value) ??
                float.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        public static string DoubleToString(double d)
        {
            return d.ToString("0." + new string('#', 339), CultureInfo.InvariantCulture);
        }

        public static string FloatToString(float f)
        {
            return f.ToString("0." + new string('#', 339), CultureInfo.InvariantCulture);
        }

        public static byte[] GetBytes(int value, bool little_endian = false)
        {
            byte[] result = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian != little_endian)
                Array.Reverse(result);
            return result;
        }

        public static int ToInt32(params byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                byte[] swap = bytes.Take(4).Reverse().ToArray();
                return BitConverter.ToInt32(swap, 0);
            }
            else
                return BitConverter.ToInt32(bytes, 0);
        }

        public static byte[] ToByteArray(params short[] shorts)
        {
            byte[] result = new byte[shorts.Length * sizeof(short)];
            Buffer.BlockCopy(shorts, 0, result, 0, result.Length);
            return result;
        }

        public static byte[] ToByteArray(params int[] ints)
        {
            byte[] result = new byte[ints.Length * sizeof(int)];
            Buffer.BlockCopy(ints, 0, result, 0, result.Length);
            return result;
        }

        public static byte[] ToByteArray(params long[] longs)
        {
            byte[] result = new byte[longs.Length * sizeof(long)];
            Buffer.BlockCopy(longs, 0, result, 0, result.Length);
            return result;
        }

        public static short[] ToShortArray(params byte[] bytes)
        {
            var size = bytes.Length / sizeof(short);
            var shorts = new short[size];
            for (int index = 0; index < size; index++)
            {
                shorts[index] = BitConverter.ToInt16(bytes, index * sizeof(short));
            }
            return shorts;
        }

        public static int[] ToIntArray(params byte[] bytes)
        {
            var size = bytes.Length / sizeof(int);
            var ints = new int[size];
            for (int index = 0; index < size; index++)
            {
                ints[index] = BitConverter.ToInt32(bytes, index * sizeof(int));
            }
            return ints;
        }

        public static long[] ToLongArray(params byte[] bytes)
        {
            var size = bytes.Length / sizeof(long);
            var longs = new long[size];
            for (int index = 0; index < size; index++)
            {
                longs[index] = BitConverter.ToInt64(bytes, index * sizeof(long));
            }
            return longs;
        }
    }
}
