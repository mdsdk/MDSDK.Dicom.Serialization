// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace MDSDK.Dicom.Serialization
{
    public static class DicomConverter
    {
        #region array <--> array

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConvertArray<TIn, TOut>(TIn[] arrayIn, out TOut[] arrayOut, Func<TIn, TOut> convert)
        {
            var n = arrayIn.Length;
            var tmp = new TOut[n];
            for (var i = 0; i < n; i++)
            {
                tmp[i] = convert(arrayIn[i]);
            }
            arrayOut = tmp;
        }

        public delegate void ConvertDelegate<TIn, TOut>(TIn i, out TOut o);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConvertArray<TIn, TOut>(TIn[] arrayIn, out TOut[] arrayOut, ConvertDelegate<TIn, TOut> convert)
        {
            if (arrayIn == null)
            {
                arrayOut = null;
            }
            else
            {
                var n = arrayIn.Length;
                var tmp = new TOut[n];
                for (var i = 0; i < n; i++)
                {
                    convert(arrayIn[i], out tmp[i]);
                }
                arrayOut = tmp;
            }
        }

        #endregion

        #region byte/ushort/int <--> byte/ushort/int 

        public static void Convert(byte[] i, out ushort[] o)
        {
            ConvertArray(i, out o, x => x);
        }

        public static void Convert(short[]i , out int[] o)
        {
            ConvertArray(i, out o, x => x);
        }

        public static void Convert(ushort[] i, out int[] o)
        {
            ConvertArray(i, out o, x => x);
        }

        #endregion

        #region string <--> short

        public static void Convert(string i, out short o)
        {
            o = short.Parse(i, NumberFormatInfo.InvariantInfo);
        }

        public static void Convert(string[] i, out short[] o)
        {
            ConvertArray(i, out o, Convert);
        }

        public static void Convert(short i, out string o)
        {
            o = i.ToString(NumberFormatInfo.InvariantInfo);
        }

        public static void Convert(short[] i, out string[] o)
        {
            ConvertArray(i, out o, Convert);
        }

        #endregion

        #region string <--> ushort

        public static void Convert(string i, out ushort o)
        {
            o = ushort.Parse(i, NumberFormatInfo.InvariantInfo);
        }

        public static void Convert(string[] i, out ushort[] o)
        {
            ConvertArray(i, out o, Convert);
        }

        public static void Convert(ushort i, out string o)
        {
            o = i.ToString(NumberFormatInfo.InvariantInfo);
        }

        public static void Convert(ushort[] i, out string[] o)
        {
            ConvertArray(i, out o, Convert);
        }

        #endregion

        #region string <--> int

        public static void Convert(string i, out int o)
        {
            o = int.Parse(i, NumberFormatInfo.InvariantInfo);
        }

        public static void Convert(string[] i, out int[] o)
        {
            ConvertArray(i, out o, Convert);
        }

        public static void Convert(int i, out string o)
        {
            o = i.ToString(NumberFormatInfo.InvariantInfo);
        }

        public static void Convert(int[] i, out string[] o)
        {
            ConvertArray(i, out o, Convert);
        }

        #endregion

        #region string <--> uint

        public static void Convert(string i, out uint o)
        {
            o = uint.Parse(i, NumberFormatInfo.InvariantInfo);
        }

        public static void Convert(string[] i, out uint[] o)
        {
            ConvertArray(i, out o, Convert);
        }

        public static void Convert(uint i, out string o)
        {
            o = i.ToString(NumberFormatInfo.InvariantInfo);
        }

        public static void Convert(uint[] i, out string[] o)
        {
            ConvertArray(i, out o, Convert);
        }

        #endregion

        #region string <--> long

        public static void Convert(string i, out long o)
        {
            o = long.Parse(i, NumberFormatInfo.InvariantInfo);
        }

        public static void Convert(string[] i, out long[] o)
        {
            ConvertArray(i, out o, Convert);
        }

        public static void Convert(long i, out string o)
        {
            o = i.ToString(NumberFormatInfo.InvariantInfo);
        }

        public static void Convert(long[] i, out string[] o)
        {
            ConvertArray(i, out o, Convert);
        }

        #endregion

        #region string <--> ulong

        public static void Convert(string i, out ulong o)
        {
            o = ulong.Parse(i, NumberFormatInfo.InvariantInfo);
        }

        public static void Convert(string[] i, out ulong[] o)
        {
            ConvertArray(i, out o, Convert);
        }

        public static void Convert(ulong i, out string o)
        {
            o = i.ToString(NumberFormatInfo.InvariantInfo);
        }

        public static void Convert(ulong[] i, out string[] o)
        {
            ConvertArray(i, out o, Convert);
        }

        #endregion

        #region string <--> float

        public static void Convert(string i, out float o)
        {
            o = float.Parse(i, NumberStyles.Float, NumberFormatInfo.InvariantInfo);
        }

        public static void Convert(string[] i, out float[] o)
        {
            ConvertArray(i, out o, Convert);
        }

        public static void Convert(float i, out string o)
        {
            // Must use "G9" instead of "R" (see description of "R" format string)
            o = i.ToString("G9", NumberFormatInfo.InvariantInfo);
        }

        public static void Convert(float[] i, out string[] o)
        {
            ConvertArray(i, out o, Convert);
        }

        #endregion

        #region string <--> double

        public static void Convert(string i, out double o)
        {
            o = double.Parse(i, NumberStyles.Float, NumberFormatInfo.InvariantInfo);
        }

        public static void Convert(string[] i, out double[] o)
        {
            ConvertArray(i, out o, Convert);
        }

        public static void Convert(double i, out string o)
        {
            // We should use "G17" to produce a roundtrippable string (see description of "R" format string).
            // However, a Decimal String may be at most 16 bytes long. Taking the longest exponential format 
            // "-d.ddddE+ddd" into account leaves spaces for only 13 significant digits.
            o = i.ToString("G13", NumberFormatInfo.InvariantInfo);
        }

        public static void Convert(double[] i, out string[] o)
        {
            ConvertArray(i, out o, Convert);
        }

        #endregion
    }
}
