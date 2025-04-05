//******************************************************************************************************
//  UntypedComplex.cs - Gbtc
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
using System.Globalization;

namespace go;

/// <summary>
/// Represents an untyped complex value.
/// </summary>
public readonly struct UntypedComplex(complex128 value) : IEquatable<UntypedComplex>
{
    // Value of the struct 'UntypedComplex'
    private readonly complex128 m_value = value;

    public bool Equals(UntypedComplex other) => m_value == other.m_value;

    public override bool Equals(object? obj)
    {
        return obj switch
        {
            UntypedComplex other => Equals(other),
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

    public static UntypedComplex operator +(UntypedComplex left, UntypedComplex right) => left.m_value + right.m_value;

    public static UntypedComplex operator -(UntypedComplex left, UntypedComplex right) => left.m_value - right.m_value;

    public static UntypedComplex operator -(UntypedComplex value) => -value.m_value;

    public static UntypedComplex operator *(UntypedComplex left, UntypedComplex right) => left.m_value * right.m_value;

    public static UntypedComplex operator /(UntypedComplex left, UntypedComplex right) => left.m_value / right.m_value;

    public override string ToString() => m_value.ToString(CultureInfo.InvariantCulture);

    public static bool operator ==(UntypedComplex left, UntypedComplex right) => left.Equals(right);

    public static bool operator !=(UntypedComplex left, UntypedComplex right) => !(left == right);

    // Handle implicit conversions between 'nint' and struct 'UntypedComplex'
    public static implicit operator UntypedComplex(nint value) => new(value);

    public static implicit operator nint(UntypedComplex value) => (nint)value.m_value.Real;

    // Handle implicit conversions between 'int8' and struct 'UntypedComplex'
    public static implicit operator UntypedComplex(int8 value) => new(value);

    public static implicit operator int8(UntypedComplex value) => (int8)value.m_value.Real;

    // Handle implicit conversions between 'int16' and struct 'UntypedComplex'
    public static implicit operator UntypedComplex(int16 value) => new(value);

    public static implicit operator int16(UntypedComplex value) => (int16)value.m_value.Real;

    // Handle implicit conversions between 'int32' and struct 'UntypedComplex'
    public static implicit operator UntypedComplex(int32 value) => new(value);

    public static implicit operator int32(UntypedComplex value) => (int32)value.m_value.Real;

    // Handle implicit conversions between 'int64' and struct 'UntypedComplex'
    public static implicit operator UntypedComplex(int64 value) => new(value);

    public static implicit operator int64(UntypedComplex value) => (int64)value.m_value.Real;

    // Handle implicit conversions between 'nuint' and struct 'UntypedComplex'
    public static implicit operator UntypedComplex(nuint value) => new(value);

    public static implicit operator nuint(UntypedComplex value) => (nuint)value.m_value.Real;

    // Handle implicit conversions between 'uint8' and struct 'UntypedComplex'
    public static implicit operator UntypedComplex(uint8 value) => new(value);

    public static implicit operator uint8(UntypedComplex value) => (uint8)value.m_value.Real;

    // Handle implicit conversions between 'uint16' and struct 'UntypedComplex'
    public static implicit operator UntypedComplex(uint16 value) => new(value);

    public static implicit operator uint16(UntypedComplex value) => (uint16)value.m_value.Real;

    // Handle implicit conversions between 'uint32' and struct 'UntypedComplex'
    public static implicit operator UntypedComplex(uint32 value) => new(value);

    public static implicit operator uint32(UntypedComplex value) => (uint32)value.m_value.Real;

    // Handle implicit conversions between 'uint64' and struct 'UntypedComplex'
    public static implicit operator UntypedComplex(uint64 value) => new(value);

    public static implicit operator uint64(UntypedComplex value) => (uint64)value.m_value.Real;

    // Handle implicit conversions between 'float32' and struct 'UntypedComplex'
    public static implicit operator UntypedComplex(float32 value) => new(value);

    public static implicit operator float32(UntypedComplex value) => (float32)value.m_value.Real;

    // Handle implicit conversions between 'float64' and struct 'UntypedComplex'
    public static implicit operator UntypedComplex(float64 value) => new(value);

    public static implicit operator float64(UntypedComplex value) => value.m_value.Real;

    // Handle implicit conversions between 'complex64' and struct 'UntypedComplex'
    public static implicit operator UntypedComplex(complex64 value) => new(value);

    public static implicit operator complex64(UntypedComplex value) => (complex64)value.m_value;

    // Handle implicit conversions between 'complex128' and struct 'UntypedComplex'
    public static implicit operator UntypedComplex(complex128 value) => new(value);

    public static implicit operator complex128(UntypedComplex value) => value.m_value;

    // Handle comparisons between 'nil' and struct 'UntypedComplex'
    public static bool operator ==(UntypedComplex value, NilType nil) => value.Equals(default);

    public static bool operator !=(UntypedComplex value, NilType nil) => !(value == nil);

    public static bool operator ==(NilType nil, UntypedComplex value) => value == nil;

    public static bool operator !=(NilType nil, UntypedComplex value) => value != nil;

    public static implicit operator UntypedComplex(NilType nil) => default!;
}
