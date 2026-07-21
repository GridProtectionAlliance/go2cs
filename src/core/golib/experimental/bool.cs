// bool.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.CompilerServices;

namespace go.experimental
{
    /// <summary>
    /// Represents a boolean type for the set of binary truth values denoted by the predeclared constants <c>true</c> and <c>false</c>. 
    /// </summary>
    /// <remarks>
    /// The C# <see cref="bool"/> type is not blittable and cannot be used with unmanaged operations.
    /// This Go equivalent bool structure is based on <see cref="byte"/> which is blittable.
    /// </remarks>
    public readonly struct @bool : IConvertible
    {
        // Value of the @bool struct
        private readonly byte m_value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public @bool(byte value) => 
            m_value = value > byte.MinValue ? byte.MaxValue : byte.MinValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public @bool(bool value) => 
            m_value = value ? byte.MaxValue : byte.MinValue;

        // Enable implicit conversions between bool and @bool struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator @bool(bool value) => new @bool(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator bool(@bool value) => value.Value;

        // Enable comparisons between nil and @bool struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(@bool value, NilType _) => value.Equals(default(@bool));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(@bool value, NilType nil) => !(value == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, @bool value) => value == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, @bool value) => value != nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator @bool(NilType _) => default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => Value.ToString().ToLowerInvariant();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(IFormatProvider? provider) => Value.ToString(provider).ToLowerInvariant();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TypeCode GetTypeCode() => TypeCode.Boolean;

        public bool Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_value != byte.MinValue;
        }

        bool IConvertible.ToBoolean(IFormatProvider? provider) => ((IConvertible)Value).ToBoolean(provider);

        char IConvertible.ToChar(IFormatProvider? provider) => ((IConvertible)Value).ToChar(provider);

        sbyte IConvertible.ToSByte(IFormatProvider? provider) => ((IConvertible)Value).ToSByte(provider);

        byte IConvertible.ToByte(IFormatProvider? provider) => ((IConvertible)Value).ToByte(provider);

        short IConvertible.ToInt16(IFormatProvider? provider) => ((IConvertible)Value).ToInt16(provider);

        ushort IConvertible.ToUInt16(IFormatProvider? provider) => ((IConvertible)Value).ToUInt16(provider);

        int IConvertible.ToInt32(IFormatProvider? provider) => ((IConvertible)Value).ToInt32(provider);

        uint IConvertible.ToUInt32(IFormatProvider? provider) => ((IConvertible)Value).ToUInt32(provider);

        long IConvertible.ToInt64(IFormatProvider? provider) => ((IConvertible)Value).ToInt64(provider);

        ulong IConvertible.ToUInt64(IFormatProvider? provider) => ((IConvertible)Value).ToUInt64(provider);

        float IConvertible.ToSingle(IFormatProvider? provider) => ((IConvertible)Value).ToSingle(provider);

        double IConvertible.ToDouble(IFormatProvider? provider) => ((IConvertible)Value).ToDouble(provider);

        decimal IConvertible.ToDecimal(IFormatProvider? provider) => ((IConvertible)Value).ToDecimal(provider);

        DateTime IConvertible.ToDateTime(IFormatProvider? provider) => ((IConvertible)Value).ToDateTime(provider);

        object IConvertible.ToType(Type conversionType, IFormatProvider? provider) => ((IConvertible)Value).ToType(conversionType, provider);
    }
}
