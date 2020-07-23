//******************************************************************************************************
//  complex64.cs - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  07/16/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable InconsistentNaming

using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;

using complex128 = System.Numerics.Complex;

namespace go
{
    /// <summary>
    /// Represents a numeric type for the set of all complex numbers with float32 real and imaginary parts.
    /// </summary>
    public readonly struct complex64 : IConvertible, IEquatable<complex64>, IFormattable
    {
        // complex64 implementation derived from .NET Complex source:
        //      https://github.com/Microsoft/referencesource/blob/master/System.Numerics/System/Numerics/Complex.cs
        //      Copyright (c) Microsoft Corporation.  All rights reserved.

        private readonly float m_real;
        private readonly float m_imaginary;

        private const float LOG_10_INV = 0.43429448190325F;

        public float Real => m_real;

        public float Imaginary => m_imaginary;

        public float Magnitude => Abs(this);

        public float Phase => (float)Math.Atan2(m_imaginary, m_real);

        public static readonly complex64 Zero = new complex64(0.0F, 0.0F);
        public static readonly complex64 One = new complex64(1.0F, 0.0F);
        public static readonly complex64 ImaginaryOne = new complex64(0.0F, 1.0F);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public complex64(float real, float imaginary)  /* Constructor to create a complex number with rectangular co-ordinates  */
        {
            m_real = real;
            m_imaginary = imaginary;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static complex64 FromPolarCoordinates(float magnitude, float phase) => new complex64(magnitude * (float)Math.Cos(phase), magnitude * (float)Math.Sin(phase));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static complex64 Negate(complex64 value) => -value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static complex64 Add(complex64 left, complex64 right) => left + right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static complex64 Subtract(complex64 left, complex64 right) => left - right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static complex64 Multiply(complex64 left, complex64 right) => left * right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static complex64 Divide(complex64 dividend, complex64 divisor) => dividend / divisor;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static complex64 operator -(complex64 value) => new complex64(-value.m_real, -value.m_imaginary);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static complex64 operator +(complex64 left, complex64 right) => new complex64(left.m_real + right.m_real, left.m_imaginary + right.m_imaginary);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static complex64 operator -(complex64 left, complex64 right) => new complex64(left.m_real - right.m_real, left.m_imaginary - right.m_imaginary);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static complex64 operator *(complex64 left, complex64 right)
        {
            // Multiplication:  (a + bi)(c + di) = (ac -bd) + (bc + ad)i
            float result_Realpart = left.m_real * right.m_real - left.m_imaginary * right.m_imaginary;
            float result_Imaginarypart = left.m_imaginary * right.m_real + left.m_real * right.m_imaginary;
            return new complex64(result_Realpart, result_Imaginarypart);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static complex64 operator /(complex64 left, complex64 right)
        {
            // Division : Smith's formula.
            float a = left.m_real;
            float b = left.m_imaginary;
            float c = right.m_real;
            float d = right.m_imaginary;

            if (Math.Abs(d) < Math.Abs(c))
            {
                float doc = d / c;
                return new complex64((a + b * doc) / (c + d * doc), (b - a * doc) / (c + d * doc));
            }

            float cod = c / d;
            return new complex64((b + a * cod) / (d + c * cod), (-a + b * cod) / (d + c * cod));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Abs(complex64 value)
        {
            if (float.IsInfinity(value.m_real) || float.IsInfinity(value.m_imaginary))
                return float.PositiveInfinity;

            // |value| == sqrt(a^2 + b^2)
            // sqrt(a^2 + b^2) == a/a * sqrt(a^2 + b^2) = a * sqrt(a^2/a^2 + b^2/a^2)
            // Using the above we can factor out the square of the larger component to dodge overflow.

            float c = Math.Abs(value.m_real);
            float d = Math.Abs(value.m_imaginary);
            float r;

            if (c > d)
            {
                r = d / c;
                return c * (float)Math.Sqrt(1.0 + r * r);
            }

            if (d == 0.0F)
                return c;  // c is either 0.0 or NaN

            r = c / d;
            return d * (float)Math.Sqrt(1.0 + r * r);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static complex64 Conjugate(complex64 value) => new complex64(value.m_real, -value.m_imaginary);

        // Reciprocal of a Complex number : the reciprocal of x+i*y is 1/(x+i*y)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static complex64 Reciprocal(complex64 value) => value.m_real == 0 && value.m_imaginary == 0 ? Zero : One / value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(complex64 left, complex64 right) => left.m_real == right.m_real && left.m_imaginary == right.m_imaginary;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(complex64 left, complex64 right) => left.m_real != right.m_real || left.m_imaginary != right.m_imaginary;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj)
        {
            if (!(obj is complex64))
                return false;

            return this == (complex64)obj;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(complex64 value) => m_real.Equals(value.m_real) && m_imaginary.Equals(value.m_imaginary);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator complex64(short value) => new complex64(value, 0.0F);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator complex64(int value) => new complex64(value, 0.0F);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator complex64(long value) => new complex64(value, 0.0F);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator complex64(ushort value) => new complex64(value, 0.0F);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator complex64(uint value) => new complex64(value, 0.0F);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator complex64(ulong value) => new complex64(value, 0.0F);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator complex64(sbyte value) => new complex64(value, 0.0F);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator complex64(byte value) => new complex64(value, 0.0F);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator complex64(float value) => new complex64(value, 0.0F);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator complex64(double value) => new complex64((float)value, 0.0F);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator complex64(BigInteger value) => new complex64((float)value, 0.0F);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator complex64(decimal value) => new complex64((float)value, 0.0F);

        // Enable conversions between Complex and complex64 struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator complex64(complex128 value) => new complex64((float)value.Real, (float)value.Imaginary);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator complex128(complex64 value) => new complex128(value.m_real, value.m_imaginary);

        // Enable comparisons between nil and complex64 struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(complex64 value, NilType _) => value.Equals(default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(complex64 value, NilType nil) => !(value == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, complex64 value) => value == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, complex64 value) => value != nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator complex64(NilType _) => default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => string.Format(CultureInfo.CurrentCulture, "({0}, {1})", m_real, m_imaginary);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(string format) => string.Format(CultureInfo.CurrentCulture, "({0}, {1})", m_real.ToString(format, CultureInfo.CurrentCulture), m_imaginary.ToString(format, CultureInfo.CurrentCulture));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(string? format, IFormatProvider? provider) => string.Format(provider, "({0}, {1})", m_real.ToString(format, provider), m_imaginary.ToString(format, provider));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(IFormatProvider? provider) => string.Format(provider, "({0}, {1})", m_real, m_imaginary);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            int n1 = 99999997;
            int hash_real = m_real.GetHashCode() % n1;
            int hash_imaginary = m_imaginary.GetHashCode();
            int final_hashcode = hash_real ^ hash_imaginary;

            return final_hashcode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static complex64 Sin(complex64 value)
        {
            float a = value.m_real;
            float b = value.m_imaginary;
            return new complex64((float)Math.Sin(a) * (float)Math.Cosh(b), (float)Math.Cos(a) * (float)Math.Sinh(b));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static complex64 Sinh(complex64 value)
        {
            float a = value.m_real;
            float b = value.m_imaginary;
            return new complex64((float)Math.Sinh(a) * (float)Math.Cos(b), (float)Math.Cosh(a) * (float)Math.Sin(b));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static complex64 Asin(complex64 value) => -ImaginaryOne * Log(ImaginaryOne * value + Sqrt(One - value * value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static complex64 Cos(complex64 value)
        {
            float a = value.m_real;
            float b = value.m_imaginary;
            return new complex64((float)Math.Cos(a) * (float)Math.Cosh(b), -((float)Math.Sin(a) * (float)Math.Sinh(b)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static complex64 Cosh(complex64 value)
        {
            float a = value.m_real;
            float b = value.m_imaginary;
            return new complex64((float)Math.Cosh(a) * (float)Math.Cos(b), (float)Math.Sinh(a) * (float)Math.Sin(b));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static complex64 Acos(complex64 value) => -ImaginaryOne * Log(value + ImaginaryOne * Sqrt(One - value * value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static complex64 Tan(complex64 value) => Sin(value) / Cos(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static complex64 Tanh(complex64 value) => Sinh(value) / Cosh(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static complex64 Atan(complex64 value)
        {
            complex64 Two = new complex64(2.0F, 0.0F);
            return ImaginaryOne / Two * (Log(One - ImaginaryOne * value) - Log(One + ImaginaryOne * value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static complex64 Log(complex64 value) => new complex64((float)Math.Log(Abs(value)), (float)Math.Atan2(value.m_imaginary, value.m_real));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static complex64 Log(complex64 value, float baseValue) => Log(value) / Log(baseValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static complex64 Log10(complex64 value) => Scale(Log(value), LOG_10_INV);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static complex64 Exp(complex64 value)
        {
            float temp_factor = (float)Math.Exp(value.m_real);
            float result_re = temp_factor * (float)Math.Cos(value.m_imaginary);
            float result_im = temp_factor * (float)Math.Sin(value.m_imaginary);
            return new complex64(result_re, result_im);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static complex64 Sqrt(complex64 value) => FromPolarCoordinates((float)Math.Sqrt(value.Magnitude), value.Phase / 2.0F);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static complex64 Pow(complex64 value, complex64 power)
        {
            if (power == Zero)
                return One;

            if (value == Zero)
                return Zero;

            float a = value.m_real;
            float b = value.m_imaginary;
            float c = power.m_real;
            float d = power.m_imaginary;

            float rho = Abs(value);
            float theta = (float)Math.Atan2(b, a);
            float newRho = c * theta + d * (float)Math.Log(rho);

            float t = (float)Math.Pow(rho, c) * (float)Math.Pow(Math.E, -d * theta);

            return new complex64(t * (float)Math.Cos(newRho), t * (float)Math.Sin(newRho));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static complex64 Pow(complex64 value, float power) => Pow(value, new complex64(power, 0));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static complex64 Scale(complex64 value, float factor)
        {
            float result_re = factor * value.m_real;
            float result_im = factor * value.m_imaginary;
            return new complex64(result_re, result_im);
        }

        TypeCode IConvertible.GetTypeCode() => m_real.GetTypeCode();

        bool IConvertible.ToBoolean(IFormatProvider? provider) => ((IConvertible)m_real).ToBoolean(provider);

        char IConvertible.ToChar(IFormatProvider? provider) => ((IConvertible)m_real).ToChar(provider);

        sbyte IConvertible.ToSByte(IFormatProvider? provider) => ((IConvertible)m_real).ToSByte(provider);

        byte IConvertible.ToByte(IFormatProvider? provider) => ((IConvertible)m_real).ToByte(provider);

        short IConvertible.ToInt16(IFormatProvider? provider) => ((IConvertible)m_real).ToInt16(provider);

        ushort IConvertible.ToUInt16(IFormatProvider? provider) => ((IConvertible)m_real).ToUInt16(provider);

        int IConvertible.ToInt32(IFormatProvider? provider) => ((IConvertible)m_real).ToInt32(provider);

        uint IConvertible.ToUInt32(IFormatProvider? provider) => ((IConvertible)m_real).ToUInt32(provider);

        long IConvertible.ToInt64(IFormatProvider? provider) => ((IConvertible)m_real).ToInt64(provider);

        ulong IConvertible.ToUInt64(IFormatProvider? provider) => ((IConvertible)m_real).ToUInt64(provider);

        float IConvertible.ToSingle(IFormatProvider? provider) => ((IConvertible)m_real).ToSingle(provider);

        double IConvertible.ToDouble(IFormatProvider? provider) => ((IConvertible)m_real).ToDouble(provider);

        decimal IConvertible.ToDecimal(IFormatProvider? provider) => ((IConvertible)m_real).ToDecimal(provider);

        DateTime IConvertible.ToDateTime(IFormatProvider? provider) => ((IConvertible)m_real).ToDateTime(provider);

        object IConvertible.ToType(Type conversionType, IFormatProvider? provider) => ((IConvertible)m_real).ToType(conversionType, provider);
    }
}
