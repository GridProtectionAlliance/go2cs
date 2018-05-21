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

using System.Runtime.CompilerServices;

namespace goutil
{
    /// <summary>
    /// Represents the "nil" type.
    /// </summary>
    public class NilType
    {
        public static NilType Default = null;

        public override int GetHashCode() => 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            if (obj is NilType)
                return true;

            if (obj is ISlice slice)
                return slice == Default;

            // TODO: Add map, channel, etc...

            return false;
        }

        // Slice to nil comparisons
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ISlice slice, NilType nil) => slice == null || (slice.Length == 0 && slice.Capacity == 0 && slice.Array == null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ISlice slice, NilType nil) => !(slice == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, ISlice slice) => slice == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, ISlice slice) => slice != nil;

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