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

using System.Runtime.CompilerServices;
using static go.builtin;

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
                case GoString gostr:
                    return gostr == Default;
                case string str:
                    return str == Default;
                // TODO: Add map, channel, etc...
            }

            return obj == null;
        }

        // ISlice to nil comparisons
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ISlice slice, NilType nil) => slice == null || (slice.Length == 0 && slice.Capacity == 0 && slice.Array == null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ISlice slice, NilType nil) => !(slice == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, ISlice slice) => slice == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, ISlice slice) => slice != nil;

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
}