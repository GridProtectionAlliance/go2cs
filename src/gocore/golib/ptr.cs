//******************************************************************************************************
//  ptr.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
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
//  06/13/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable UnusedParameter.Local

using System.Runtime.CompilerServices;

namespace go
{
    /// <summary>
    /// Represents a heap allocated reference to an instance of type <typeparamref name="T"/>
    /// where <typeparamref name="T"/> is a managed type, e.g., a class.
    /// </summary>
    /// <typeparam name="T">Reference type for heap based reference.</typeparam>
    /// <remarks>
    /// <para>
    /// If <typeparamref name="T"/> is a <see cref="System.ValueType"/>, e.g., a struct, note that
    /// value will be "boxed" for heap allocation. Since boxed value will be a new copy of original
    /// value, make sure to use point <see cref="Value"/> for value updates instead of local stack
    /// copy of value. See the <see cref="builtin.heap{T}"/> helper function and
    /// https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/types/boxing-and-unboxing
    /// </para>
    /// <para>
    /// So long as a reference to this class exists, so will the value <typeparamref name="T"/>.
    /// </para>
    /// </remarks>
    public sealed class ptr<T>
    {
        private T m_value;
        
        public ref T Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref m_value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ptr(in T value) => m_value = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ptr(NilType _) : this(default(T)!) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ptr() : this(default(T)!) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"&{m_value?.ToString() ?? "nil"}";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Equals(in ptr<T> other)
        {
            if (m_value is null && other is null)
                return true;

            if (other is null)
                return false;

            return !(m_value is null) && m_value.Equals(other.m_value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) => obj is ptr<T> other && Equals(other);

        // ReSharper disable once NonReadonlyMemberInGetHashCode
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => m_value?.GetHashCode() ?? 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T operator ~(ptr<T> value) => value.m_value;

        // Enable comparisons between nil and @ref<T> interface instance
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ptr<T> value, NilType _) => value.Equals(null!);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ptr<T> value, NilType nil) => !(value == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, ptr<T> value) => value == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, in ptr<T> value) => value != nil;
    }
}