//******************************************************************************************************
//  NilType.cs - Gbtc
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
//  05/07/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable CheckNamespace

using System;

namespace go;

/// <summary>
/// Represents the "nil" type.
/// </summary>
public class NilType : IConvertible
{
    public static NilType Default = null!;

    public override int GetHashCode() => 0;

    public override bool Equals(object? obj)
    {
        return obj switch
        {
            NilType          => true,
            ISlice slice     => slice == Default,
            IArray array     => array == Default,
            IChannel channel => channel == Default,
            @string gostr    => gostr == Default,
            string str       => str == Default,
            _                => obj is null
        };
    }

    public override string ToString() => "nil";

    // IArray to nil comparisons
    public static bool operator ==(IArray? array, NilType _) => array is null || array.Length == 0;

    public static bool operator !=(IArray? array, NilType nil) => !(array == nil);

    public static bool operator ==(NilType nil, IArray? array) => array == nil;

    public static bool operator !=(NilType nil, IArray? array) => array != nil;

    // ISlice to nil comparisons
    public static bool operator ==(ISlice? slice, NilType _) => slice is null or { Length: 0, Capacity: 0, Source: null };

    public static bool operator !=(ISlice? slice, NilType nil) => !(slice == nil);

    public static bool operator ==(NilType nil, ISlice? slice) => slice == nil;

    public static bool operator !=(NilType nil, ISlice? slice) => slice != nil;

    // IChannel to nil comparisons
    public static bool operator ==(IChannel? channel, NilType _) => channel is null or { Length: 0, Capacity: 0 };

    public static bool operator !=(IChannel? channel, NilType nil) => !(channel == nil);

    public static bool operator ==(NilType nil, IChannel? channel) => channel == nil;

    public static bool operator !=(NilType nil, IChannel? channel) => channel != nil;

    // string to nil comparisons
    public static bool operator ==(string? obj, NilType _) => string.IsNullOrEmpty(obj);

    public static bool operator !=(string? obj, NilType _) => !string.IsNullOrEmpty(obj);

    public static bool operator ==(NilType _, string? obj) => string.IsNullOrEmpty(obj);

    public static bool operator !=(NilType _, string? obj) => !string.IsNullOrEmpty(obj);

    public static implicit operator string(NilType _) => ""; // In Go, string defaults to empty string, not null

    // object to nil comparisons
    public static bool operator ==(object? obj, NilType _) => obj is null;

    public static bool operator !=(object? obj, NilType _) => obj is not null;

    public static bool operator ==(NilType _, object? obj) => obj is null;

    public static bool operator !=(NilType _, object? obj) => obj is not null;

    TypeCode IConvertible.GetTypeCode() => TypeCode.DBNull;

    bool IConvertible.ToBoolean(IFormatProvider? provider) => default;

    byte IConvertible.ToByte(IFormatProvider? provider) => default;

    char IConvertible.ToChar(IFormatProvider? provider) => default;

    DateTime IConvertible.ToDateTime(IFormatProvider? provider) => default;

    decimal IConvertible.ToDecimal(IFormatProvider? provider) => default;

    double IConvertible.ToDouble(IFormatProvider? provider) => default;

    short IConvertible.ToInt16(IFormatProvider? provider) => default;

    int IConvertible.ToInt32(IFormatProvider? provider) => default;

    long IConvertible.ToInt64(IFormatProvider? provider) => default;

    sbyte IConvertible.ToSByte(IFormatProvider? provider) => default;

    float IConvertible.ToSingle(IFormatProvider? provider) => default;

    string IConvertible.ToString(IFormatProvider? provider) => "";

    object IConvertible.ToType(Type conversionType, IFormatProvider? provider) => Default;

    ushort IConvertible.ToUInt16(IFormatProvider? provider) => default;

    uint IConvertible.ToUInt32(IFormatProvider? provider) => default;

    ulong IConvertible.ToUInt64(IFormatProvider? provider) => default;
}

/// <summary>
/// Represents the "nil" type.
/// </summary>
public class NilType<T> : NilType where T : class?
{
    public override int GetHashCode() => 0;

    public override bool Equals(object? obj) => base.Equals(obj);

    // Enable comparisons between nil and class types (should be rare)
    public static bool operator ==(in T? value, NilType<T> _) => value is null || (Activator.CreateInstance(value.GetType())?.Equals(value) ?? false);

    public static bool operator !=(in T? value, NilType<T> nil) => !(value == nil);

    public static bool operator ==(NilType<T> nil, in T? value) => value == nil;

    public static bool operator !=(NilType<T> nil, in T? value) => value != nil;
}
