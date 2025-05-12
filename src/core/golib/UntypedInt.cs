//******************************************************************************************************
//  UntypedInt.cs - Gbtc
//
//  Copyright © 2025, Grid Protection Alliance.  All Rights Reserved.
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
//  04/04/2025 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Diagnostics;

namespace go;

/// <summary>
/// Represents an untyped integer value.
/// </summary>
public readonly struct UntypedInt : IEquatable<UntypedInt>
{
    // 64-bit value of the struct 'UntypedInt'
    private readonly int64 m_value;

    /// <summary>
    /// Creates a new <see cref="UntypedInt"/> from <c>int64</c> value.
    /// </summary>
    public UntypedInt(int64 value)
    {
        m_value = value;
    }

    /// <summary>
    /// Creates a new <see cref="UntypedInt"/> from <c>uint64</c> value.
    /// </summary>
    public UntypedInt(uint64 value)
    {
        m_value = CastFrom(value)
    }

    // Perform bitwise cast from T to int64
    private static unsafe int64 CastFrom<T>(T source) where T : unmanaged
    {
        Debug.Assert(sizeof(T) <= sizeof(int64), "Type is too large for bitwise conversion to int64");
        void* sourceBits = &source;
        return *(int64*)sourceBits;
    }

    // Perform bitwise cast to T from int64
    private static unsafe T CastTo<T>(int64 source) where T : unmanaged
    {
        Debug.Assert(sizeof(T) <= sizeof(int64), "Type is too large for bitwise conversion from int64");
        void* sourceBits = &source;
        return *(T*)sourceBits;
    }

    public bool Equals(UntypedInt other) => m_value == other.m_value;

    public override bool Equals(object? obj)
    {
        return obj switch
        {
            UntypedInt other => Equals(other),
            nint value => Equals(value),
            int8 value => Equals(value),
            int16 value => Equals(value),
            int32 value => Equals(value),
            int64 value => Equals(value),
            nuint value => Equals(value),
            uint8 value => Equals(value),
            uint16 value => Equals(value),
            uint32 value => Equals(value),
            uint64 value => Equals(value),
            float32 value => Equals(value),
            float64 value => Equals(value),
            complex64 value => Equals(value),
            complex128 value => Equals(value),
            _ => false
        };
    }

    public override int GetHashCode() => m_value.GetHashCode();

    public static bool operator <(UntypedInt left, UntypedInt right) => left.m_value < right.m_value;

    public static bool operator <=(UntypedInt left, UntypedInt right) => left.m_value <= right.m_value;

    public static bool operator >(UntypedInt left, UntypedInt right) => left.m_value > right.m_value;

    public static bool operator >=(UntypedInt left, UntypedInt right) => left.m_value >= right.m_value;

    public static UntypedInt operator +(UntypedInt left, UntypedInt right) => left.m_value + right.m_value;

    public static UntypedInt operator -(UntypedInt left, UntypedInt right) => left.m_value - right.m_value;

    public static UntypedInt operator -(UntypedInt value) => -(int64)value.m_value;

    public static UntypedInt operator *(UntypedInt left, UntypedInt right) => left.m_value * right.m_value;

    public static UntypedInt operator /(UntypedInt left, UntypedInt right) => left.m_value / right.m_value;

    public static UntypedInt operator %(UntypedInt left, UntypedInt right) => left.m_value % right.m_value;

    public override string ToString() => m_value.ToString();

    public static bool operator ==(UntypedInt left, UntypedInt right) => left.Equals(right);

    public static bool operator !=(UntypedInt left, UntypedInt right) => !(left == right);

    // Handle implicit conversions between 'nint' and struct 'UntypedInt'
    public static implicit operator UntypedInt(nint value) => new(value);

    public static implicit operator nint(UntypedInt value) => (nint)value.m_value;

    // Handle implicit conversions between 'int8' and struct 'UntypedInt'
    public static implicit operator UntypedInt(int8 value) => new(value);

    public static implicit operator int8(UntypedInt value) => (int8)value.m_value;

    // Handle implicit conversions between 'int16' and struct 'UntypedInt'
    public static implicit operator UntypedInt(int16 value) => new(value);

    public static implicit operator int16(UntypedInt value) => (int16)value.m_value;

    // Handle implicit conversions between 'int32' and struct 'UntypedInt'
    public static implicit operator UntypedInt(int32 value) => new(value);

    public static implicit operator int32(UntypedInt value) => (int32)value.m_value;

    // Handle implicit conversions between 'int64' and struct 'UntypedInt'
    public static implicit operator UntypedInt(int64 value) => new(value);

    public static implicit operator int64(UntypedInt value) => value.m_value;

    // Handle implicit conversions between 'nuint' and struct 'UntypedInt'
    public static implicit operator UntypedInt(nuint value) => new(CastFrom(value));

    public static implicit operator nuint(UntypedInt value) => CastTo<nuint>(value.m_value);

    // Handle implicit conversions between 'uint8' and struct 'UntypedInt'
    public static implicit operator UntypedInt(uint8 value) => new(value);

    public static implicit operator uint8(UntypedInt value) => (uint8)value.m_value;

    // Handle implicit conversions between 'uint16' and struct 'UntypedInt'
    public static implicit operator UntypedInt(uint16 value) => new(value);

    public static implicit operator uint16(UntypedInt value) => (uint16)value.m_value;

    // Handle implicit conversions between 'uint32' and struct 'UntypedInt'
    public static implicit operator UntypedInt(uint32 value) => new(value);

    public static implicit operator uint32(UntypedInt value) => (uint32)value.m_value;

    // Handle implicit conversions between 'uint64' and struct 'UntypedInt'
    public static implicit operator UntypedInt(uint64 value) => new(CastFrom(value));

    public static implicit operator uint64(UntypedInt value) => CastTo<uint64>(value.m_value);

    // Handle implicit conversions between 'float32' and struct 'UntypedInt'
    public static implicit operator UntypedInt(float32 value) => new(CastFrom(value));

    public static implicit operator float32(UntypedInt value) => CastTo<float32>(value.m_value);

    // Handle implicit conversions between 'float64' and struct 'UntypedInt'
    public static implicit operator UntypedInt(float64 value) => new(CastFrom(value));

    public static implicit operator float64(UntypedInt value) => CastTo<float64>(value.m_value);

    // Handle implicit conversions between 'complex64' and struct 'UntypedInt'
    public static implicit operator UntypedInt(complex64 value) => new(CastFrom(value.Real));

    public static implicit operator complex64(UntypedInt value) => CastTo<float32>(value.m_value);

    // Handle implicit conversions between 'complex128' and struct 'UntypedInt'
    public static implicit operator UntypedInt(complex128 value) => new(CastFrom(value.Real));

    public static implicit operator complex128(UntypedInt value) => CastTo<float64>(value.m_value);

    // Handle comparisons between 'nil' and struct 'UntypedInt'
    public static bool operator ==(UntypedInt value, NilType nil) => value.Equals(default);

    public static bool operator !=(UntypedInt value, NilType nil) => !(value == nil);

    public static bool operator ==(NilType nil, UntypedInt value) => value == nil;

    public static bool operator !=(NilType nil, UntypedInt value) => value != nil;

    public static implicit operator UntypedInt(NilType nil) => default!;
}
