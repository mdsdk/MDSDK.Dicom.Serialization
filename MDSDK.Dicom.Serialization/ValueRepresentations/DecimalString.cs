// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Globalization;
using System.IO;
using System.Numerics;

namespace MDSDK.Dicom.Serialization.ValueRepresentations
{
    internal sealed class DecimalString : AsciiEncodedMultiValue, IMultiValue<decimal>, IMultiValue<double>, IMultiValue<float>, IMultiValue<Vector3>
    {
        public DecimalString() : base("DS") { }

        decimal[] IMultiValue<decimal>.ReadValues(DicomStreamReader reader) => ReadAndConvertValues(reader, decimal.Parse);

        decimal IMultiValue<decimal>.ReadSingleValue(DicomStreamReader reader) => ReadAndConvertSingleValue(reader, decimal.Parse);

        void IMultiValue<decimal>.WriteValues(DicomStreamWriter writer, decimal[] values) => ConvertAndWriteValues(writer, Convert.ToString, values);

        void IMultiValue<decimal>.WriteSingleValue(DicomStreamWriter writer, decimal value) => ConvertAndWriteSingleValue(writer, Convert.ToString, value);

        double[] IMultiValue<double>.ReadValues(DicomStreamReader reader) => ReadAndConvertValues(reader, double.Parse);

        double IMultiValue<double>.ReadSingleValue(DicomStreamReader reader) => ReadAndConvertSingleValue(reader, double.Parse);

        void IMultiValue<double>.WriteValues(DicomStreamWriter writer, double[] values) => ConvertAndWriteValues(writer, Convert.ToString, values);

        void IMultiValue<double>.WriteSingleValue(DicomStreamWriter writer, double value) => ConvertAndWriteSingleValue(writer, Convert.ToString, value);

        float[] IMultiValue<float>.ReadValues(DicomStreamReader reader) => ReadAndConvertValues(reader, float.Parse);

        float IMultiValue<float>.ReadSingleValue(DicomStreamReader reader) => ReadAndConvertSingleValue(reader, float.Parse);

        void IMultiValue<float>.WriteValues(DicomStreamWriter writer, float[] values) => ConvertAndWriteValues(writer, Convert.ToString, values);

        void IMultiValue<float>.WriteSingleValue(DicomStreamWriter writer, float value) => ConvertAndWriteSingleValue(writer, Convert.ToString, value);

        private static Vector3 ToVector3(string[] s, ref int i)
        {
            static float Parse(string s) => float.Parse(s, NumberFormatInfo.InvariantInfo);

            return new Vector3
            {
                X = Parse(s[i++]),
                Y = Parse(s[i++]),
                Z = Parse(s[i++])
            };
        }

        Vector3[] IMultiValue<Vector3>.ReadValues(DicomStreamReader reader)
        {
            var s = ReadValues(reader);
            if (s.Length % 3 != 0)
            {
                throw new IOException("Number of values is not a multiple of 3");
            }
            var count = s.Length / 3;
            var values = new Vector3[count];
            var i = 0;
            for (var j = 0; j < count; j++)
            {
                values[j] = ToVector3(s, ref i);
            }
            return values;
        }

        Vector3 IMultiValue<Vector3>.ReadSingleValue(DicomStreamReader reader)
        {
            var s = ReadValues(reader);
            if (s.Length != 3)
            {
                throw new IOException("Number of values is not 3");
            }
            var i = 0;
            return ToVector3(s, ref i);
        }

        private static void ToString(Vector3 value, string[] s, ref int i)
        {
            static string ToString(float f) => Convert.ToString(f, NumberFormatInfo.InvariantInfo);

            s[i++] = ToString(value.X);
            s[i++] = ToString(value.Y);
            s[i++] = ToString(value.Z);
        }

        void IMultiValue<Vector3>.WriteValues(DicomStreamWriter writer, Vector3[] values)
        {
            var s = new string[values.Length * 3];
            var i = 0;
            foreach (var value in values)
            {
                ToString(value, s, ref i);
            }
            WriteValues(writer, s);
        }

        void IMultiValue<Vector3>.WriteSingleValue(DicomStreamWriter writer, Vector3 value)
        {
            var s = new string[3];
            var i = 0;
            ToString(value, s, ref i);
            WriteValues(writer, s);
        }
    }
}
