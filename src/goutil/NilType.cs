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
using System.Runtime.CompilerServices;

namespace go
{
    /// <summary>
    /// Represents the "nil" type.
    /// </summary>
    public partial class NilType : EmptyInterface
    {
        public static NilType Default = null;

        public override int GetHashCode() => 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case NilType _:
                    return true;
                case ISlice slice:
                    return slice == Default;
                case IArray array:
                    return array == Default;
                case IChannel channel:
                    return channel == Default;
                case @string gostr:
                    return gostr == Default;
                case string str:
                    return str == Default;
                // TODO: Add map, etc...
            }

            return obj == null;
        }

        // IArray to nil comparisons
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(IArray array, NilType nil) => array == null || array.Length == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(IArray array, NilType nil) => !(array == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, IArray array) => array == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, IArray array) => array != nil;

        // ISlice to nil comparisons
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ISlice slice, NilType nil) => slice == null || slice.Length == 0 && slice.Capacity == 0 && slice.Array == null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ISlice slice, NilType nil) => !(slice == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, ISlice slice) => slice == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, ISlice slice) => slice != nil;

        // IChannel to nil comparisons
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(IChannel channel, NilType nil) => channel == null || channel.Length == 0 && channel.Capacity == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(IChannel channel, NilType nil) => !(channel == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, IChannel channel) => channel == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, IChannel channel) => channel != nil;

        // string to nil comparisons
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(string obj, NilType nil) => string.IsNullOrEmpty(obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(string obj, NilType nil) => !string.IsNullOrEmpty(obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, string obj) => string.IsNullOrEmpty(obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, string obj) => !string.IsNullOrEmpty(obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator string(NilType nil) => ""; // In Go, string defaults to empty string, not null

        // object to nil comparisons
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(object obj, NilType nil) => obj == null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(object obj, NilType nil) => obj != null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, object obj) => obj == null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, object obj) => obj != null;
    }

    /// <summary>
    /// Represents the "nil" type.
    /// </summary>
    public class NilType<T> : NilType where T : class
    {
        public override int GetHashCode() => 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) => base.Equals(obj);

        // Enable comparisons between nil and Abser interface
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(T value, NilType<T> nil) => (object)value == null || Activator.CreateInstance(value.GetType()).Equals(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(T value, NilType<T> nil) => !(value == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType<T> nil, T value) => value == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType<T> nil, T value) => value != nil;
    }
}