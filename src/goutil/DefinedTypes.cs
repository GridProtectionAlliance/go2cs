//******************************************************************************************************
//  DefinedTypes.cs - Gbtc
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
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable SpecifyACultureInStringConversionExplicitly

using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;

#pragma warning disable CS0660, CS0661

namespace go
{
    /// <summary>
    /// Represents the Go interface type known as the empty interface, i.e., "interface {}".
    /// </summary>
    /// <remarks>
    /// An empty interface may hold values of any Go type as every type implements at least
    /// zero methods. All Go types converted to C# inherit this interface.
    /// </remarks>
    public interface EmptyInterface
    {
    }

    /// <summary>
    /// Represents a boolean type for the set of binary truth values denoted by the predeclared constants <c>true</c> and <c>false</c>. 
    /// </summary>
    public struct @bool : EmptyInterface, IConvertible
    {
        // Value of the @bool struct
        private readonly bool m_value;

        public @bool(bool value) => m_value = value;

        // Enable implicit conversions between bool and @bool struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator @bool(bool value) => new @bool(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator bool(@bool value) => value.m_value;

        // Enable comparisons between nil and @bool struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(@bool value, NilType nil) => value.Equals(default(@bool));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(@bool value, NilType nil) => !(value == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, @bool value) => value == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, @bool value) => value != nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator @bool(NilType nil) => default;

        public override string ToString() => m_value.ToString().ToLowerInvariant();

        public string ToString(IFormatProvider provider) => m_value.ToString(provider).ToLowerInvariant();

        public TypeCode GetTypeCode() => m_value.GetTypeCode();

        bool IConvertible.ToBoolean(IFormatProvider provider) => ((IConvertible)m_value).ToBoolean(provider);

        char IConvertible.ToChar(IFormatProvider provider) => ((IConvertible)m_value).ToChar(provider);

        sbyte IConvertible.ToSByte(IFormatProvider provider) => ((IConvertible)m_value).ToSByte(provider);

        byte IConvertible.ToByte(IFormatProvider provider) => ((IConvertible)m_value).ToByte(provider);

        short IConvertible.ToInt16(IFormatProvider provider) => ((IConvertible)m_value).ToInt16(provider);

        ushort IConvertible.ToUInt16(IFormatProvider provider) => ((IConvertible)m_value).ToUInt16(provider);

        int IConvertible.ToInt32(IFormatProvider provider) => ((IConvertible)m_value).ToInt32(provider);

        uint IConvertible.ToUInt32(IFormatProvider provider) => ((IConvertible)m_value).ToUInt32(provider);

        long IConvertible.ToInt64(IFormatProvider provider) => ((IConvertible)m_value).ToInt64(provider);

        ulong IConvertible.ToUInt64(IFormatProvider provider) => ((IConvertible)m_value).ToUInt64(provider);

        float IConvertible.ToSingle(IFormatProvider provider) => ((IConvertible)m_value).ToSingle(provider);

        double IConvertible.ToDouble(IFormatProvider provider) => ((IConvertible)m_value).ToDouble(provider);

        decimal IConvertible.ToDecimal(IFormatProvider provider) => ((IConvertible)m_value).ToDecimal(provider);

        DateTime IConvertible.ToDateTime(IFormatProvider provider) => ((IConvertible)m_value).ToDateTime(provider);

        object IConvertible.ToType(Type conversionType, IFormatProvider provider) => ((IConvertible)m_value).ToType(conversionType, provider);
    }

    /// <summary>
    /// Represents a numeric type for the set of all unsigned 8-bit integers (0 to 255). 
    /// </summary>
    public struct uint8 : EmptyInterface, IConvertible
    {
        // Value of the uint8 struct
        private readonly byte m_value;

        public uint8(byte value) => m_value = value;

        // Enable implicit conversions between byte and uint8 struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator uint8(byte value) => new uint8(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator byte(uint8 value) => value.m_value;

        // Enable comparisons between nil and uint8 struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(uint8 value, NilType nil) => value.Equals(default(uint8));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(uint8 value, NilType nil) => !(value == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, uint8 value) => value == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, uint8 value) => value != nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator uint8(NilType nil) => default;

        public override string ToString() => m_value.ToString();

        public string ToString(IFormatProvider provider) => m_value.ToString(provider);

        public TypeCode GetTypeCode() => m_value.GetTypeCode();

        bool IConvertible.ToBoolean(IFormatProvider provider) => ((IConvertible)m_value).ToBoolean(provider);

        char IConvertible.ToChar(IFormatProvider provider) => ((IConvertible)m_value).ToChar(provider);

        sbyte IConvertible.ToSByte(IFormatProvider provider) => ((IConvertible)m_value).ToSByte(provider);

        byte IConvertible.ToByte(IFormatProvider provider) => ((IConvertible)m_value).ToByte(provider);

        short IConvertible.ToInt16(IFormatProvider provider) => ((IConvertible)m_value).ToInt16(provider);

        ushort IConvertible.ToUInt16(IFormatProvider provider) => ((IConvertible)m_value).ToUInt16(provider);

        int IConvertible.ToInt32(IFormatProvider provider) => ((IConvertible)m_value).ToInt32(provider);

        uint IConvertible.ToUInt32(IFormatProvider provider) => ((IConvertible)m_value).ToUInt32(provider);

        long IConvertible.ToInt64(IFormatProvider provider) => ((IConvertible)m_value).ToInt64(provider);

        ulong IConvertible.ToUInt64(IFormatProvider provider) => ((IConvertible)m_value).ToUInt64(provider);

        float IConvertible.ToSingle(IFormatProvider provider) => ((IConvertible)m_value).ToSingle(provider);

        double IConvertible.ToDouble(IFormatProvider provider) => ((IConvertible)m_value).ToDouble(provider);

        decimal IConvertible.ToDecimal(IFormatProvider provider) => ((IConvertible)m_value).ToDecimal(provider);

        DateTime IConvertible.ToDateTime(IFormatProvider provider) => ((IConvertible)m_value).ToDateTime(provider);

        object IConvertible.ToType(Type conversionType, IFormatProvider provider) => ((IConvertible)m_value).ToType(conversionType, provider);
    }

    /// <summary>
    /// Represents a numeric type for the set of all unsigned 16-bit integers (0 to 65535).
    /// </summary>
    public struct uint16 : EmptyInterface, IConvertible
    {
        // Value of the uint16 struct
        private readonly ushort m_value;

        public uint16(ushort value) => m_value = value;

        // Enable implicit conversions between ushort and uint16 struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator uint16(ushort value) => new uint16(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ushort(uint16 value) => value.m_value;

        // Enable comparisons between nil and uint16 struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(uint16 value, NilType nil) => value.Equals(default(uint16));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(uint16 value, NilType nil) => !(value == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, uint16 value) => value == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, uint16 value) => value != nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator uint16(NilType nil) => default;

        public override string ToString() => m_value.ToString();

        public string ToString(IFormatProvider provider) => m_value.ToString(provider);

        public TypeCode GetTypeCode() => m_value.GetTypeCode();

        bool IConvertible.ToBoolean(IFormatProvider provider) => ((IConvertible)m_value).ToBoolean(provider);

        char IConvertible.ToChar(IFormatProvider provider) => ((IConvertible)m_value).ToChar(provider);

        sbyte IConvertible.ToSByte(IFormatProvider provider) => ((IConvertible)m_value).ToSByte(provider);

        byte IConvertible.ToByte(IFormatProvider provider) => ((IConvertible)m_value).ToByte(provider);

        short IConvertible.ToInt16(IFormatProvider provider) => ((IConvertible)m_value).ToInt16(provider);

        ushort IConvertible.ToUInt16(IFormatProvider provider) => ((IConvertible)m_value).ToUInt16(provider);

        int IConvertible.ToInt32(IFormatProvider provider) => ((IConvertible)m_value).ToInt32(provider);

        uint IConvertible.ToUInt32(IFormatProvider provider) => ((IConvertible)m_value).ToUInt32(provider);

        long IConvertible.ToInt64(IFormatProvider provider) => ((IConvertible)m_value).ToInt64(provider);

        ulong IConvertible.ToUInt64(IFormatProvider provider) => ((IConvertible)m_value).ToUInt64(provider);

        float IConvertible.ToSingle(IFormatProvider provider) => ((IConvertible)m_value).ToSingle(provider);

        double IConvertible.ToDouble(IFormatProvider provider) => ((IConvertible)m_value).ToDouble(provider);

        decimal IConvertible.ToDecimal(IFormatProvider provider) => ((IConvertible)m_value).ToDecimal(provider);

        DateTime IConvertible.ToDateTime(IFormatProvider provider) => ((IConvertible)m_value).ToDateTime(provider);

        object IConvertible.ToType(Type conversionType, IFormatProvider provider) => ((IConvertible)m_value).ToType(conversionType, provider);
    }

    /// <summary>
    /// Represents a numeric type for the set of all unsigned 32-bit integers (0 to 4294967295).
    /// </summary>
    public struct uint32 : EmptyInterface, IConvertible
    {
        // Value of the uint32 struct
        private readonly uint m_value;

        public uint32(uint value) => m_value = value;

        // Enable implicit conversions between uint and uint32 struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator uint32(uint value) => new uint32(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator uint(uint32 value) => value.m_value;

        // Enable comparisons between nil and uint32 struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(uint32 value, NilType nil) => value.Equals(default(uint32));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(uint32 value, NilType nil) => !(value == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, uint32 value) => value == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, uint32 value) => value != nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator uint32(NilType nil) => default;

        public override string ToString() => m_value.ToString();

        public string ToString(IFormatProvider provider) => m_value.ToString(provider);

        public TypeCode GetTypeCode() => m_value.GetTypeCode();

        bool IConvertible.ToBoolean(IFormatProvider provider) => ((IConvertible)m_value).ToBoolean(provider);

        char IConvertible.ToChar(IFormatProvider provider) => ((IConvertible)m_value).ToChar(provider);

        sbyte IConvertible.ToSByte(IFormatProvider provider) => ((IConvertible)m_value).ToSByte(provider);

        byte IConvertible.ToByte(IFormatProvider provider) => ((IConvertible)m_value).ToByte(provider);

        short IConvertible.ToInt16(IFormatProvider provider) => ((IConvertible)m_value).ToInt16(provider);

        ushort IConvertible.ToUInt16(IFormatProvider provider) => ((IConvertible)m_value).ToUInt16(provider);

        int IConvertible.ToInt32(IFormatProvider provider) => ((IConvertible)m_value).ToInt32(provider);

        uint IConvertible.ToUInt32(IFormatProvider provider) => ((IConvertible)m_value).ToUInt32(provider);

        long IConvertible.ToInt64(IFormatProvider provider) => ((IConvertible)m_value).ToInt64(provider);

        ulong IConvertible.ToUInt64(IFormatProvider provider) => ((IConvertible)m_value).ToUInt64(provider);

        float IConvertible.ToSingle(IFormatProvider provider) => ((IConvertible)m_value).ToSingle(provider);

        double IConvertible.ToDouble(IFormatProvider provider) => ((IConvertible)m_value).ToDouble(provider);

        decimal IConvertible.ToDecimal(IFormatProvider provider) => ((IConvertible)m_value).ToDecimal(provider);

        DateTime IConvertible.ToDateTime(IFormatProvider provider) => ((IConvertible)m_value).ToDateTime(provider);

        object IConvertible.ToType(Type conversionType, IFormatProvider provider) => ((IConvertible)m_value).ToType(conversionType, provider);
    }

    /// <summary>
    /// Represents a numeric type for the set of all unsigned 64-bit integers (0 to 18446744073709551615).
    /// </summary>
    public struct uint64 : EmptyInterface, IConvertible
    {
        // Value of the uint64 struct
        private readonly ulong m_value;

        public uint64(ulong value) => m_value = value;

        // Enable implicit conversions between ulong and uint64 struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator uint64(ulong value) => new uint64(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ulong(uint64 value) => value.m_value;

        // Enable comparisons between nil and uint64 struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(uint64 value, NilType nil) => value.Equals(default(uint64));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(uint64 value, NilType nil) => !(value == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, uint64 value) => value == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, uint64 value) => value != nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator uint64(NilType nil) => default;

        public override string ToString() => m_value.ToString();

        public string ToString(IFormatProvider provider) => m_value.ToString(provider);

        public TypeCode GetTypeCode() => m_value.GetTypeCode();

        bool IConvertible.ToBoolean(IFormatProvider provider) => ((IConvertible)m_value).ToBoolean(provider);

        char IConvertible.ToChar(IFormatProvider provider) => ((IConvertible)m_value).ToChar(provider);

        sbyte IConvertible.ToSByte(IFormatProvider provider) => ((IConvertible)m_value).ToSByte(provider);

        byte IConvertible.ToByte(IFormatProvider provider) => ((IConvertible)m_value).ToByte(provider);

        short IConvertible.ToInt16(IFormatProvider provider) => ((IConvertible)m_value).ToInt16(provider);

        ushort IConvertible.ToUInt16(IFormatProvider provider) => ((IConvertible)m_value).ToUInt16(provider);

        int IConvertible.ToInt32(IFormatProvider provider) => ((IConvertible)m_value).ToInt32(provider);

        uint IConvertible.ToUInt32(IFormatProvider provider) => ((IConvertible)m_value).ToUInt32(provider);

        long IConvertible.ToInt64(IFormatProvider provider) => ((IConvertible)m_value).ToInt64(provider);

        ulong IConvertible.ToUInt64(IFormatProvider provider) => ((IConvertible)m_value).ToUInt64(provider);

        float IConvertible.ToSingle(IFormatProvider provider) => ((IConvertible)m_value).ToSingle(provider);

        double IConvertible.ToDouble(IFormatProvider provider) => ((IConvertible)m_value).ToDouble(provider);

        decimal IConvertible.ToDecimal(IFormatProvider provider) => ((IConvertible)m_value).ToDecimal(provider);

        DateTime IConvertible.ToDateTime(IFormatProvider provider) => ((IConvertible)m_value).ToDateTime(provider);

        object IConvertible.ToType(Type conversionType, IFormatProvider provider) => ((IConvertible)m_value).ToType(conversionType, provider);
    }

    /// <summary>
    /// Represents a numeric type for the set of all signed 8-bit integers (-128 to 127).
    /// </summary>
    public struct int8 : EmptyInterface, IConvertible
    {
        // Value of the int8 struct
        private readonly sbyte m_value;

        public int8(sbyte value) => m_value = value;

        // Enable implicit conversions between sbyte and int8 struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int8(sbyte value) => new int8(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator sbyte(int8 value) => value.m_value;

        // Enable comparisons between nil and int8 struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(int8 value, NilType nil) => value.Equals(default(int8));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(int8 value, NilType nil) => !(value == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, int8 value) => value == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, int8 value) => value != nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int8(NilType nil) => default;

        public override string ToString() => m_value.ToString();

        public string ToString(IFormatProvider provider) => m_value.ToString(provider);

        public TypeCode GetTypeCode() => m_value.GetTypeCode();

        bool IConvertible.ToBoolean(IFormatProvider provider) => ((IConvertible)m_value).ToBoolean(provider);

        char IConvertible.ToChar(IFormatProvider provider) => ((IConvertible)m_value).ToChar(provider);

        sbyte IConvertible.ToSByte(IFormatProvider provider) => ((IConvertible)m_value).ToSByte(provider);

        byte IConvertible.ToByte(IFormatProvider provider) => ((IConvertible)m_value).ToByte(provider);

        short IConvertible.ToInt16(IFormatProvider provider) => ((IConvertible)m_value).ToInt16(provider);

        ushort IConvertible.ToUInt16(IFormatProvider provider) => ((IConvertible)m_value).ToUInt16(provider);

        int IConvertible.ToInt32(IFormatProvider provider) => ((IConvertible)m_value).ToInt32(provider);

        uint IConvertible.ToUInt32(IFormatProvider provider) => ((IConvertible)m_value).ToUInt32(provider);

        long IConvertible.ToInt64(IFormatProvider provider) => ((IConvertible)m_value).ToInt64(provider);

        ulong IConvertible.ToUInt64(IFormatProvider provider) => ((IConvertible)m_value).ToUInt64(provider);

        float IConvertible.ToSingle(IFormatProvider provider) => ((IConvertible)m_value).ToSingle(provider);

        double IConvertible.ToDouble(IFormatProvider provider) => ((IConvertible)m_value).ToDouble(provider);

        decimal IConvertible.ToDecimal(IFormatProvider provider) => ((IConvertible)m_value).ToDecimal(provider);

        DateTime IConvertible.ToDateTime(IFormatProvider provider) => ((IConvertible)m_value).ToDateTime(provider);

        object IConvertible.ToType(Type conversionType, IFormatProvider provider) => ((IConvertible)m_value).ToType(conversionType, provider);
    }

    /// <summary>
    /// Represents a numeric type for the set of all signed 16-bit integers (-32768 to 32767).
    /// </summary>
    public struct int16 : EmptyInterface, IConvertible
    {
        // Value of the int16 struct
        private readonly short m_value;

        public int16(short value) => m_value = value;

        // Enable implicit conversions between short and int16 struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int16(short value) => new int16(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator short(int16 value) => value.m_value;

        // Enable comparisons between nil and int16 struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(int16 value, NilType nil) => value.Equals(default(int16));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(int16 value, NilType nil) => !(value == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, int16 value) => value == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, int16 value) => value != nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int16(NilType nil) => default;

        public override string ToString() => m_value.ToString();

        public string ToString(IFormatProvider provider) => m_value.ToString(provider);

        public TypeCode GetTypeCode() => m_value.GetTypeCode();

        bool IConvertible.ToBoolean(IFormatProvider provider) => ((IConvertible)m_value).ToBoolean(provider);

        char IConvertible.ToChar(IFormatProvider provider) => ((IConvertible)m_value).ToChar(provider);

        sbyte IConvertible.ToSByte(IFormatProvider provider) => ((IConvertible)m_value).ToSByte(provider);

        byte IConvertible.ToByte(IFormatProvider provider) => ((IConvertible)m_value).ToByte(provider);

        short IConvertible.ToInt16(IFormatProvider provider) => ((IConvertible)m_value).ToInt16(provider);

        ushort IConvertible.ToUInt16(IFormatProvider provider) => ((IConvertible)m_value).ToUInt16(provider);

        int IConvertible.ToInt32(IFormatProvider provider) => ((IConvertible)m_value).ToInt32(provider);

        uint IConvertible.ToUInt32(IFormatProvider provider) => ((IConvertible)m_value).ToUInt32(provider);

        long IConvertible.ToInt64(IFormatProvider provider) => ((IConvertible)m_value).ToInt64(provider);

        ulong IConvertible.ToUInt64(IFormatProvider provider) => ((IConvertible)m_value).ToUInt64(provider);

        float IConvertible.ToSingle(IFormatProvider provider) => ((IConvertible)m_value).ToSingle(provider);

        double IConvertible.ToDouble(IFormatProvider provider) => ((IConvertible)m_value).ToDouble(provider);

        decimal IConvertible.ToDecimal(IFormatProvider provider) => ((IConvertible)m_value).ToDecimal(provider);

        DateTime IConvertible.ToDateTime(IFormatProvider provider) => ((IConvertible)m_value).ToDateTime(provider);

        object IConvertible.ToType(Type conversionType, IFormatProvider provider) => ((IConvertible)m_value).ToType(conversionType, provider);
    }

    /// <summary>
    /// Represents a numeric type for the set of all signed 32-bit integers (-2147483648 to 2147483647).
    /// </summary>
    public struct int32 : EmptyInterface, IConvertible
    {
        // Value of the int32 struct
        private readonly int m_value;

        public int32(int value) => m_value = value;

        // Enable implicit conversions between int and int32 struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int32(int value) => new int32(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(int32 value) => value.m_value;

        // Enable comparisons between nil and int32 struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(int32 value, NilType nil) => value.Equals(default(int32));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(int32 value, NilType nil) => !(value == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, int32 value) => value == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, int32 value) => value != nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int32(NilType nil) => default;

        public override string ToString() => m_value.ToString();

        public string ToString(IFormatProvider provider) => m_value.ToString(provider);

        public TypeCode GetTypeCode() => m_value.GetTypeCode();

        bool IConvertible.ToBoolean(IFormatProvider provider) => ((IConvertible)m_value).ToBoolean(provider);

        char IConvertible.ToChar(IFormatProvider provider) => ((IConvertible)m_value).ToChar(provider);

        sbyte IConvertible.ToSByte(IFormatProvider provider) => ((IConvertible)m_value).ToSByte(provider);

        byte IConvertible.ToByte(IFormatProvider provider) => ((IConvertible)m_value).ToByte(provider);

        short IConvertible.ToInt16(IFormatProvider provider) => ((IConvertible)m_value).ToInt16(provider);

        ushort IConvertible.ToUInt16(IFormatProvider provider) => ((IConvertible)m_value).ToUInt16(provider);

        int IConvertible.ToInt32(IFormatProvider provider) => ((IConvertible)m_value).ToInt32(provider);

        uint IConvertible.ToUInt32(IFormatProvider provider) => ((IConvertible)m_value).ToUInt32(provider);

        long IConvertible.ToInt64(IFormatProvider provider) => ((IConvertible)m_value).ToInt64(provider);

        ulong IConvertible.ToUInt64(IFormatProvider provider) => ((IConvertible)m_value).ToUInt64(provider);

        float IConvertible.ToSingle(IFormatProvider provider) => ((IConvertible)m_value).ToSingle(provider);

        double IConvertible.ToDouble(IFormatProvider provider) => ((IConvertible)m_value).ToDouble(provider);

        decimal IConvertible.ToDecimal(IFormatProvider provider) => ((IConvertible)m_value).ToDecimal(provider);

        DateTime IConvertible.ToDateTime(IFormatProvider provider) => ((IConvertible)m_value).ToDateTime(provider);

        object IConvertible.ToType(Type conversionType, IFormatProvider provider) => ((IConvertible)m_value).ToType(conversionType, provider);
    }

    /// <summary>
    /// Represents a numeric type for the set of all signed 64-bit integers (-9223372036854775808 to 9223372036854775807).
    /// </summary>
    public struct int64 : EmptyInterface, IConvertible
    {
        // Value of the int64 struct
        private readonly long m_value;

        public int64(long value) => m_value = value;

        // Enable implicit conversions between long and int64 struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int64(long value) => new int64(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator long(int64 value) => value.m_value;

        // Enable comparisons between nil and int64 struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(int64 value, NilType nil) => value.Equals(default(int64));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(int64 value, NilType nil) => !(value == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, int64 value) => value == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, int64 value) => value != nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int64(NilType nil) => default;

        public override string ToString() => m_value.ToString();

        public string ToString(IFormatProvider provider) => m_value.ToString(provider);

        public TypeCode GetTypeCode() => m_value.GetTypeCode();

        bool IConvertible.ToBoolean(IFormatProvider provider) => ((IConvertible)m_value).ToBoolean(provider);

        char IConvertible.ToChar(IFormatProvider provider) => ((IConvertible)m_value).ToChar(provider);

        sbyte IConvertible.ToSByte(IFormatProvider provider) => ((IConvertible)m_value).ToSByte(provider);

        byte IConvertible.ToByte(IFormatProvider provider) => ((IConvertible)m_value).ToByte(provider);

        short IConvertible.ToInt16(IFormatProvider provider) => ((IConvertible)m_value).ToInt16(provider);

        ushort IConvertible.ToUInt16(IFormatProvider provider) => ((IConvertible)m_value).ToUInt16(provider);

        int IConvertible.ToInt32(IFormatProvider provider) => ((IConvertible)m_value).ToInt32(provider);

        uint IConvertible.ToUInt32(IFormatProvider provider) => ((IConvertible)m_value).ToUInt32(provider);

        long IConvertible.ToInt64(IFormatProvider provider) => ((IConvertible)m_value).ToInt64(provider);

        ulong IConvertible.ToUInt64(IFormatProvider provider) => ((IConvertible)m_value).ToUInt64(provider);

        float IConvertible.ToSingle(IFormatProvider provider) => ((IConvertible)m_value).ToSingle(provider);

        double IConvertible.ToDouble(IFormatProvider provider) => ((IConvertible)m_value).ToDouble(provider);

        decimal IConvertible.ToDecimal(IFormatProvider provider) => ((IConvertible)m_value).ToDecimal(provider);

        DateTime IConvertible.ToDateTime(IFormatProvider provider) => ((IConvertible)m_value).ToDateTime(provider);

        object IConvertible.ToType(Type conversionType, IFormatProvider provider) => ((IConvertible)m_value).ToType(conversionType, provider);
    }

    /// <summary>
    /// Represents a numeric type for the set of all IEEE-754 32-bit floating-point numbers.
    /// </summary>
    public struct float32 : EmptyInterface, IConvertible
    {
        // Value of the float32 struct
        private readonly float m_value;

        public float32(float value) => m_value = value;

        // Enable implicit conversions between float and float32 struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator float32(float value) => new float32(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator float(float32 value) => value.m_value;

        // Enable comparisons between nil and float32 struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(float32 value, NilType nil) => value.Equals(default(float32));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(float32 value, NilType nil) => !(value == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, float32 value) => value == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, float32 value) => value != nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator float32(NilType nil) => default;

        public override string ToString() => m_value.ToString();

        public string ToString(IFormatProvider provider) => m_value.ToString(provider);

        public TypeCode GetTypeCode() => m_value.GetTypeCode();

        bool IConvertible.ToBoolean(IFormatProvider provider) => ((IConvertible)m_value).ToBoolean(provider);

        char IConvertible.ToChar(IFormatProvider provider) => ((IConvertible)m_value).ToChar(provider);

        sbyte IConvertible.ToSByte(IFormatProvider provider) => ((IConvertible)m_value).ToSByte(provider);

        byte IConvertible.ToByte(IFormatProvider provider) => ((IConvertible)m_value).ToByte(provider);

        short IConvertible.ToInt16(IFormatProvider provider) => ((IConvertible)m_value).ToInt16(provider);

        ushort IConvertible.ToUInt16(IFormatProvider provider) => ((IConvertible)m_value).ToUInt16(provider);

        int IConvertible.ToInt32(IFormatProvider provider) => ((IConvertible)m_value).ToInt32(provider);

        uint IConvertible.ToUInt32(IFormatProvider provider) => ((IConvertible)m_value).ToUInt32(provider);

        long IConvertible.ToInt64(IFormatProvider provider) => ((IConvertible)m_value).ToInt64(provider);

        ulong IConvertible.ToUInt64(IFormatProvider provider) => ((IConvertible)m_value).ToUInt64(provider);

        float IConvertible.ToSingle(IFormatProvider provider) => ((IConvertible)m_value).ToSingle(provider);

        double IConvertible.ToDouble(IFormatProvider provider) => ((IConvertible)m_value).ToDouble(provider);

        decimal IConvertible.ToDecimal(IFormatProvider provider) => ((IConvertible)m_value).ToDecimal(provider);

        DateTime IConvertible.ToDateTime(IFormatProvider provider) => ((IConvertible)m_value).ToDateTime(provider);

        object IConvertible.ToType(Type conversionType, IFormatProvider provider) => ((IConvertible)m_value).ToType(conversionType, provider);
    }

    /// <summary>
    /// Represents a numeric type for the set of all IEEE-754 64-bit floating-point numbers.
    /// </summary>
    public struct float64 : EmptyInterface, IConvertible
    {
        // Value of the float64 struct
        private readonly double m_value;

        public float64(double value) => m_value = value;

        // Enable implicit conversions between double and float64 struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator float64(double value) => new float64(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator double(float64 value) => value.m_value;

        // Enable comparisons between nil and float64 struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(float64 value, NilType nil) => value.Equals(default(float64));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(float64 value, NilType nil) => !(value == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, float64 value) => value == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, float64 value) => value != nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator float64(NilType nil) => default;

        public override string ToString() => m_value.ToString();

        public string ToString(IFormatProvider provider) => m_value.ToString(provider);

        public TypeCode GetTypeCode() => m_value.GetTypeCode();

        bool IConvertible.ToBoolean(IFormatProvider provider) => ((IConvertible)m_value).ToBoolean(provider);

        char IConvertible.ToChar(IFormatProvider provider) => ((IConvertible)m_value).ToChar(provider);

        sbyte IConvertible.ToSByte(IFormatProvider provider) => ((IConvertible)m_value).ToSByte(provider);

        byte IConvertible.ToByte(IFormatProvider provider) => ((IConvertible)m_value).ToByte(provider);

        short IConvertible.ToInt16(IFormatProvider provider) => ((IConvertible)m_value).ToInt16(provider);

        ushort IConvertible.ToUInt16(IFormatProvider provider) => ((IConvertible)m_value).ToUInt16(provider);

        int IConvertible.ToInt32(IFormatProvider provider) => ((IConvertible)m_value).ToInt32(provider);

        uint IConvertible.ToUInt32(IFormatProvider provider) => ((IConvertible)m_value).ToUInt32(provider);

        long IConvertible.ToInt64(IFormatProvider provider) => ((IConvertible)m_value).ToInt64(provider);

        ulong IConvertible.ToUInt64(IFormatProvider provider) => ((IConvertible)m_value).ToUInt64(provider);

        float IConvertible.ToSingle(IFormatProvider provider) => ((IConvertible)m_value).ToSingle(provider);

        double IConvertible.ToDouble(IFormatProvider provider) => ((IConvertible)m_value).ToDouble(provider);

        decimal IConvertible.ToDecimal(IFormatProvider provider) => ((IConvertible)m_value).ToDecimal(provider);

        DateTime IConvertible.ToDateTime(IFormatProvider provider) => ((IConvertible)m_value).ToDateTime(provider);

        object IConvertible.ToType(Type conversionType, IFormatProvider provider) => ((IConvertible)m_value).ToType(conversionType, provider);
    }

    /// <summary>
    /// Represents a numeric type for the set of all complex numbers with float32 real and imaginary parts.
    /// </summary>
    public struct complex64 : EmptyInterface, IEquatable<complex64>, IFormattable
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
        public override bool Equals(object obj)
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
        public static explicit operator complex64(Decimal value) => new complex64((float)value, 0.0F);

        // Enable conversions between Complex and complex64 struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator complex64(Complex value) => new complex64((float)value.Real, (float)value.Imaginary);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator complex64(complex128 value) => new complex64((float)value.Real, (float)value.Imaginary);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Complex(complex64 value) => new Complex(value.m_real, value.m_imaginary);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator complex128(complex64 value) => new complex128(value.m_real, value.m_imaginary);

        // Enable comparisons between nil and complex64 struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(complex64 value, NilType nil) => value.Equals(default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(complex64 value, NilType nil) => !(value == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, complex64 value) => value == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, complex64 value) => value != nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator complex64(NilType nil) => default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => string.Format(CultureInfo.CurrentCulture, "({0}, {1})", m_real, m_imaginary);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(string format) => string.Format(CultureInfo.CurrentCulture, "({0}, {1})", m_real.ToString(format, CultureInfo.CurrentCulture), m_imaginary.ToString(format, CultureInfo.CurrentCulture));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(IFormatProvider provider) => string.Format(provider, "({0}, {1})", m_real, m_imaginary);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(String format, IFormatProvider provider) => string.Format(provider, "({0}, {1})", m_real.ToString(format, provider), m_imaginary.ToString(format, provider));

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
    }

    /// <summary>
    /// Represents a numeric type for the set of all complex numbers with float64 real and imaginary parts.
    /// </summary>
    public struct complex128 : EmptyInterface, IEquatable<complex128>, IFormattable
    {
        // Value of the complex128 struct
        private readonly Complex m_value;

        public complex128(Complex value) => m_value = value;

        public complex128(double real, double imaginary) => m_value = new Complex(real, imaginary);

        public double Real => m_value.Real;

        public double Imaginary => m_value.Imaginary;

        // Enable implicit conversions between Complex and complex128 struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator complex128(Complex value) => new complex128(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Complex(complex128 value) => value.m_value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator complex128(short value) => new complex128(value, 0.0D);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator complex128(int value) => new complex128(value, 0.0D);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator complex128(long value) => new complex128(value, 0.0D);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator complex128(ushort value) => new complex128(value, 0.0D);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator complex128(uint value) => new complex128(value, 0.0D);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator complex128(ulong value) => new complex128(value, 0.0D);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator complex128(sbyte value) => new complex128(value, 0.0D);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator complex128(byte value) => new complex128(value, 0.0D);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator complex128(float value) => new complex128(value, 0.0D);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator complex128(double value) => new complex128(value, 0.0D);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator complex128(BigInteger value) => new complex128((double)value, 0.0D);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator complex128(Decimal value) => new complex128((double)value, 0.0D);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static complex128 operator -(complex128 value) => new complex128(-value.Real, -value.Imaginary);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static complex128 operator +(complex128 left, complex128 right) => new complex128(left.Real + right.Real, left.Imaginary + right.Imaginary);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static complex128 operator -(complex128 left, complex128 right) => new complex128(left.Real - right.Real, left.Imaginary - right.Imaginary);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static complex128 operator *(complex128 left, complex128 right) => left.m_value * right.m_value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static complex128 operator /(complex128 left, complex128 right) => left.m_value / right.m_value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(complex128 left, complex128 right) => left.Real == right.Real && left.Imaginary == right.Imaginary;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(complex128 left, complex128 right) => left.Real != right.Real || left.Imaginary != right.Imaginary;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) => m_value.Equals(obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(complex128 value) => m_value.Equals(value.m_value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => m_value.GetHashCode();

        public override string ToString() => m_value.ToString();

        public string ToString(string format, IFormatProvider formatProvider) => m_value.ToString(format, formatProvider);

        // Enable comparisons between nil and complex128 struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(complex128 value, NilType nil) => value.Equals(default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(complex128 value, NilType nil) => !(value == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, complex128 value) => value == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, complex128 value) => value != nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator complex128(NilType nil) => default;
    }

    /// <summary>
    /// Represents an alias for <see cref="uint8"/>, i.e., a numeric type for the set of all unsigned 8-bit integers (0 to 255). 
    /// </summary>
    public struct @byte : EmptyInterface, IConvertible
    {
        // Value of the @byte struct
        private readonly uint8 m_value;

        public @byte(uint8 value) => m_value = value;

        // Enable implicit conversions between uint8 and @byte struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator @byte(uint8 value) => new @byte(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator uint8(@byte value) => value.m_value;

        // Enable implicit conversions between byte and @byte struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator @byte(byte value) => new @byte(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator byte(@byte value) => value.m_value;

        // Enable comparisons between nil and @byte struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(@byte value, NilType nil) => value.Equals(default(@byte));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(@byte value, NilType nil) => !(value == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, @byte value) => value == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, @byte value) => value != nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator @byte(NilType nil) => default;

        public override string ToString() => m_value.ToString();

        public string ToString(IFormatProvider provider) => m_value.ToString(provider);

        public TypeCode GetTypeCode() => m_value.GetTypeCode();

        bool IConvertible.ToBoolean(IFormatProvider provider) => ((IConvertible)m_value).ToBoolean(provider);

        char IConvertible.ToChar(IFormatProvider provider) => ((IConvertible)m_value).ToChar(provider);

        sbyte IConvertible.ToSByte(IFormatProvider provider) => ((IConvertible)m_value).ToSByte(provider);

        byte IConvertible.ToByte(IFormatProvider provider) => ((IConvertible)m_value).ToByte(provider);

        short IConvertible.ToInt16(IFormatProvider provider) => ((IConvertible)m_value).ToInt16(provider);

        ushort IConvertible.ToUInt16(IFormatProvider provider) => ((IConvertible)m_value).ToUInt16(provider);

        int IConvertible.ToInt32(IFormatProvider provider) => ((IConvertible)m_value).ToInt32(provider);

        uint IConvertible.ToUInt32(IFormatProvider provider) => ((IConvertible)m_value).ToUInt32(provider);

        long IConvertible.ToInt64(IFormatProvider provider) => ((IConvertible)m_value).ToInt64(provider);

        ulong IConvertible.ToUInt64(IFormatProvider provider) => ((IConvertible)m_value).ToUInt64(provider);

        float IConvertible.ToSingle(IFormatProvider provider) => ((IConvertible)m_value).ToSingle(provider);

        double IConvertible.ToDouble(IFormatProvider provider) => ((IConvertible)m_value).ToDouble(provider);

        decimal IConvertible.ToDecimal(IFormatProvider provider) => ((IConvertible)m_value).ToDecimal(provider);

        DateTime IConvertible.ToDateTime(IFormatProvider provider) => ((IConvertible)m_value).ToDateTime(provider);

        object IConvertible.ToType(Type conversionType, IFormatProvider provider) => ((IConvertible)m_value).ToType(conversionType, provider);
    }

    /// <summary>
    /// Represents an alias for <see cref="int32"/>, i.e., a numeric type for the set of all signed 32-bit integers (-2147483648 to 2147483647). 
    /// </summary>
    /// <remarks>
    /// The built-in rune type is used, by convention, to distinguish character values from integer values.
    /// It is an alias for <see cref="int32"/> and is equivalent to <see cref="int32"/> in all ways.
    /// </remarks>
    public struct rune : EmptyInterface, IConvertible
    {
        // Value of the rune struct
        private readonly int32 m_value;

        public rune(int32 value) => m_value = value;

        // Enable implicit conversions between int32 and rune struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator rune(int32 value) => new rune(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int32(rune value) => value.m_value;

        // Enable implicit conversions between char and rune struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator rune(char value) => new rune(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator char(rune value) => (char)value.m_value;

        // Enable comparisons between nil and rune struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(rune value, NilType nil) => value.Equals(default(rune));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(rune value, NilType nil) => !(value == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, rune value) => value == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, rune value) => value != nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator rune(NilType nil) => default;

        public override string ToString() => m_value.ToString();

        public string ToString(IFormatProvider provider) => m_value.ToString(provider);

        public TypeCode GetTypeCode() => m_value.GetTypeCode();

        bool IConvertible.ToBoolean(IFormatProvider provider) => ((IConvertible)m_value).ToBoolean(provider);

        char IConvertible.ToChar(IFormatProvider provider) => ((IConvertible)m_value).ToChar(provider);

        sbyte IConvertible.ToSByte(IFormatProvider provider) => ((IConvertible)m_value).ToSByte(provider);

        byte IConvertible.ToByte(IFormatProvider provider) => ((IConvertible)m_value).ToByte(provider);

        short IConvertible.ToInt16(IFormatProvider provider) => ((IConvertible)m_value).ToInt16(provider);

        ushort IConvertible.ToUInt16(IFormatProvider provider) => ((IConvertible)m_value).ToUInt16(provider);

        int IConvertible.ToInt32(IFormatProvider provider) => ((IConvertible)m_value).ToInt32(provider);

        uint IConvertible.ToUInt32(IFormatProvider provider) => ((IConvertible)m_value).ToUInt32(provider);

        long IConvertible.ToInt64(IFormatProvider provider) => ((IConvertible)m_value).ToInt64(provider);

        ulong IConvertible.ToUInt64(IFormatProvider provider) => ((IConvertible)m_value).ToUInt64(provider);

        float IConvertible.ToSingle(IFormatProvider provider) => ((IConvertible)m_value).ToSingle(provider);

        double IConvertible.ToDouble(IFormatProvider provider) => ((IConvertible)m_value).ToDouble(provider);

        decimal IConvertible.ToDecimal(IFormatProvider provider) => ((IConvertible)m_value).ToDecimal(provider);

        DateTime IConvertible.ToDateTime(IFormatProvider provider) => ((IConvertible)m_value).ToDateTime(provider);

        object IConvertible.ToType(Type conversionType, IFormatProvider provider) => ((IConvertible)m_value).ToType(conversionType, provider);
    }

#if Target32Bit
    /// <summary>
    /// Represents a numeric type for the set of all unsigned 32-bit integers (0 to 4294967295).
    /// </summary>
    public struct @uint : EmptyInterface, IConvertible
    {
        // Value of the @uint struct
        private readonly uint m_value;

        public @uint(uint value) => m_value = value;

        // Enable implicit conversions between uint and @uint struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator @uint(uint value) => new @uint(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator uint(@uint value) => value.m_value;

        // Enable comparisons between nil and @uint struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(@uint value, NilType nil) => value.Equals(default(@uint));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(@uint value, NilType nil) => !(value == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, @uint value) => value == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, @uint value) => value != nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator @uint(NilType nil) => default;

        public override string ToString() => m_value.ToString();

        public string ToString(IFormatProvider provider) => m_value.ToString(provider);

        public TypeCode GetTypeCode() => m_value.GetTypeCode();

        bool IConvertible.ToBoolean(IFormatProvider provider) => ((IConvertible)m_value).ToBoolean(provider);

        char IConvertible.ToChar(IFormatProvider provider) => ((IConvertible)m_value).ToChar(provider);

        sbyte IConvertible.ToSByte(IFormatProvider provider) => ((IConvertible)m_value).ToSByte(provider);

        byte IConvertible.ToByte(IFormatProvider provider) => ((IConvertible)m_value).ToByte(provider);

        short IConvertible.ToInt16(IFormatProvider provider) => ((IConvertible)m_value).ToInt16(provider);

        ushort IConvertible.ToUInt16(IFormatProvider provider) => ((IConvertible)m_value).ToUInt16(provider);

        int IConvertible.ToInt32(IFormatProvider provider) => ((IConvertible)m_value).ToInt32(provider);

        uint IConvertible.ToUInt32(IFormatProvider provider) => ((IConvertible)m_value).ToUInt32(provider);

        long IConvertible.ToInt64(IFormatProvider provider) => ((IConvertible)m_value).ToInt64(provider);

        ulong IConvertible.ToUInt64(IFormatProvider provider) => ((IConvertible)m_value).ToUInt64(provider);

        float IConvertible.ToSingle(IFormatProvider provider) => ((IConvertible)m_value).ToSingle(provider);

        double IConvertible.ToDouble(IFormatProvider provider) => ((IConvertible)m_value).ToDouble(provider);

        decimal IConvertible.ToDecimal(IFormatProvider provider) => ((IConvertible)m_value).ToDecimal(provider);

        DateTime IConvertible.ToDateTime(IFormatProvider provider) => ((IConvertible)m_value).ToDateTime(provider);

        object IConvertible.ToType(Type conversionType, IFormatProvider provider) => ((IConvertible)m_value).ToType(conversionType, provider);
    }

    /// <summary>
    /// Represents a numeric type for the set of all signed 32-bit integers (-2147483648 to 2147483647).
    /// </summary>
    public struct @int : EmptyInterface, IConvertible
    {
        // Value of the @int struct
        private readonly int m_value;

        public @int(int value) => m_value = value;

        // Enable implicit conversions between int and @int struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator @int(int value) => new @int(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(@int value) => value.m_value;

        // Enable comparisons between nil and int64 struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(@int value, NilType nil) => value.Equals(default(@int));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(@int value, NilType nil) => !(value == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, @int value) => value == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, @int value) => value != nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator @int(NilType nil) => default;

        public override string ToString() => m_value.ToString();

        public string ToString(IFormatProvider provider) => m_value.ToString(provider);

        public TypeCode GetTypeCode() => m_value.GetTypeCode();

        bool IConvertible.ToBoolean(IFormatProvider provider) => ((IConvertible)m_value).ToBoolean(provider);

        char IConvertible.ToChar(IFormatProvider provider) => ((IConvertible)m_value).ToChar(provider);

        sbyte IConvertible.ToSByte(IFormatProvider provider) => ((IConvertible)m_value).ToSByte(provider);

        byte IConvertible.ToByte(IFormatProvider provider) => ((IConvertible)m_value).ToByte(provider);

        short IConvertible.ToInt16(IFormatProvider provider) => ((IConvertible)m_value).ToInt16(provider);

        ushort IConvertible.ToUInt16(IFormatProvider provider) => ((IConvertible)m_value).ToUInt16(provider);

        int IConvertible.ToInt32(IFormatProvider provider) => ((IConvertible)m_value).ToInt32(provider);

        uint IConvertible.ToUInt32(IFormatProvider provider) => ((IConvertible)m_value).ToUInt32(provider);

        long IConvertible.ToInt64(IFormatProvider provider) => ((IConvertible)m_value).ToInt64(provider);

        ulong IConvertible.ToUInt64(IFormatProvider provider) => ((IConvertible)m_value).ToUInt64(provider);

        float IConvertible.ToSingle(IFormatProvider provider) => ((IConvertible)m_value).ToSingle(provider);

        double IConvertible.ToDouble(IFormatProvider provider) => ((IConvertible)m_value).ToDouble(provider);

        decimal IConvertible.ToDecimal(IFormatProvider provider) => ((IConvertible)m_value).ToDecimal(provider);

        DateTime IConvertible.ToDateTime(IFormatProvider provider) => ((IConvertible)m_value).ToDateTime(provider);

        object IConvertible.ToType(Type conversionType, IFormatProvider provider) => ((IConvertible)m_value).ToType(conversionType, provider);
    }
#else
    /// <summary>
    /// Represents a numeric type for the set of all unsigned 64-bit integers (0 to 18446744073709551615).
    /// </summary>
    public struct @uint : EmptyInterface, IConvertible
    {
        // Value of the @uint struct
        private readonly ulong m_value;

        public @uint(ulong value) => m_value = value;

        // Enable implicit conversions between ulong and @uint struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator @uint(ulong value) => new @uint(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ulong(@uint value) => value.m_value;

        // Enable comparisons between nil and @uint struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(@uint value, NilType nil) => value.Equals(default(@uint));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(@uint value, NilType nil) => !(value == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, @uint value) => value == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, @uint value) => value != nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator @uint(NilType nil) => default;

        public override string ToString() => m_value.ToString();

        public string ToString(IFormatProvider provider) => m_value.ToString(provider);

        public TypeCode GetTypeCode() => m_value.GetTypeCode();

        bool IConvertible.ToBoolean(IFormatProvider provider) => ((IConvertible)m_value).ToBoolean(provider);

        char IConvertible.ToChar(IFormatProvider provider) => ((IConvertible)m_value).ToChar(provider);

        sbyte IConvertible.ToSByte(IFormatProvider provider) => ((IConvertible)m_value).ToSByte(provider);

        byte IConvertible.ToByte(IFormatProvider provider) => ((IConvertible)m_value).ToByte(provider);

        short IConvertible.ToInt16(IFormatProvider provider) => ((IConvertible)m_value).ToInt16(provider);

        ushort IConvertible.ToUInt16(IFormatProvider provider) => ((IConvertible)m_value).ToUInt16(provider);

        int IConvertible.ToInt32(IFormatProvider provider) => ((IConvertible)m_value).ToInt32(provider);

        uint IConvertible.ToUInt32(IFormatProvider provider) => ((IConvertible)m_value).ToUInt32(provider);

        long IConvertible.ToInt64(IFormatProvider provider) => ((IConvertible)m_value).ToInt64(provider);

        ulong IConvertible.ToUInt64(IFormatProvider provider) => ((IConvertible)m_value).ToUInt64(provider);

        float IConvertible.ToSingle(IFormatProvider provider) => ((IConvertible)m_value).ToSingle(provider);

        double IConvertible.ToDouble(IFormatProvider provider) => ((IConvertible)m_value).ToDouble(provider);

        decimal IConvertible.ToDecimal(IFormatProvider provider) => ((IConvertible)m_value).ToDecimal(provider);

        DateTime IConvertible.ToDateTime(IFormatProvider provider) => ((IConvertible)m_value).ToDateTime(provider);

        object IConvertible.ToType(Type conversionType, IFormatProvider provider) => ((IConvertible)m_value).ToType(conversionType, provider);
    }

    /// <summary>
    /// Represents a numeric type for the set of all signed 64-bit integers (-9223372036854775808 to 9223372036854775807).
    /// </summary>
    public struct @int : EmptyInterface, IConvertible
    {
        // Value of the @int struct
        private readonly long m_value;

        public @int(long value) => m_value = value;

        // Enable implicit conversions between long and @int struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator @int(long value) => new @int(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator long(@int value) => value.m_value;

        // Enable comparisons between nil and int64 struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(@int value, NilType nil) => value.Equals(default(@int));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(@int value, NilType nil) => !(value == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, @int value) => value == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, @int value) => value != nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator @int(NilType nil) => default;

        public override string ToString() => m_value.ToString();

        public string ToString(IFormatProvider provider) => m_value.ToString(provider);

        public TypeCode GetTypeCode() => m_value.GetTypeCode();

        bool IConvertible.ToBoolean(IFormatProvider provider) => ((IConvertible)m_value).ToBoolean(provider);

        char IConvertible.ToChar(IFormatProvider provider) => ((IConvertible)m_value).ToChar(provider);

        sbyte IConvertible.ToSByte(IFormatProvider provider) => ((IConvertible)m_value).ToSByte(provider);

        byte IConvertible.ToByte(IFormatProvider provider) => ((IConvertible)m_value).ToByte(provider);

        short IConvertible.ToInt16(IFormatProvider provider) => ((IConvertible)m_value).ToInt16(provider);

        ushort IConvertible.ToUInt16(IFormatProvider provider) => ((IConvertible)m_value).ToUInt16(provider);

        int IConvertible.ToInt32(IFormatProvider provider) => ((IConvertible)m_value).ToInt32(provider);

        uint IConvertible.ToUInt32(IFormatProvider provider) => ((IConvertible)m_value).ToUInt32(provider);

        long IConvertible.ToInt64(IFormatProvider provider) => ((IConvertible)m_value).ToInt64(provider);

        ulong IConvertible.ToUInt64(IFormatProvider provider) => ((IConvertible)m_value).ToUInt64(provider);

        float IConvertible.ToSingle(IFormatProvider provider) => ((IConvertible)m_value).ToSingle(provider);

        double IConvertible.ToDouble(IFormatProvider provider) => ((IConvertible)m_value).ToDouble(provider);

        decimal IConvertible.ToDecimal(IFormatProvider provider) => ((IConvertible)m_value).ToDecimal(provider);

        DateTime IConvertible.ToDateTime(IFormatProvider provider) => ((IConvertible)m_value).ToDateTime(provider);

        object IConvertible.ToType(Type conversionType, IFormatProvider provider) => ((IConvertible)m_value).ToType(conversionType, provider);
    }
#endif

    /// <summary>
    /// Represents an unsigned integer large enough to store the uninterpreted bits of a pointer value.
    /// </summary>
    public struct uintptr : EmptyInterface
    {
        // Value of the uintptr struct
        private readonly UIntPtr m_value;

        public uintptr(UIntPtr value) => m_value = value;

        // Enable implicit conversions between UIntPtr and uintptr struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator uintptr(UIntPtr value) => new uintptr(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UIntPtr(uintptr value) => value.m_value;

        // Enable comparisons between nil and uintptr struct
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(uintptr value, NilType nil) => value.Equals(default(uintptr));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(uintptr value, NilType nil) => !(value == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, uintptr value) => value == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, uintptr value) => value != nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator uintptr(NilType nil) => default;
    }
}