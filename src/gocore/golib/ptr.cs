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
// ReSharper disable InconsistentNaming
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
    /// A .NET class is always allocated on the heap and registered for garbage collection.
    /// The <see cref="ptr{T}"/> class is used to create a reference to a heap allocated instance
    /// of type <typeparamref name="T"/> so that the type can (1) have scope beyond the current
    /// stack, and (2) have the ability to create a safe pointer to the type, i.e., a reference.
    /// </para>
    /// <para>
    /// If <typeparamref name="T"/> is a <see cref="System.ValueType"/>, e.g., a struct, note that
    /// value will be "boxed" for heap allocation. Since boxed value will be a new copy of original
    /// value, make sure to use ref-based <see cref="Value"/> for updates instead of a local stack
    /// copy of value. See the <see cref="builtin.heap{T}"/> helper function and notes on boxing:
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
        private bool Equals(ptr<T>? other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (IsReferenceType)
            {
                if (m_value is null && other.m_value is null)
                    return true;

                if (m_value is null || other.m_value is null)
                    return false;

                if (ReferenceEquals(m_value, other.m_value))
                    return true;
            }

            return m_value!.Equals(other.m_value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) => obj is ptr<T> other && Equals(other);

        // ReSharper disable once NonReadonlyMemberInGetHashCode
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => m_value?.GetHashCode() ?? 0;

        // WISH: Would be super cool if this operator supported "ref" return, like:
        //     public static ref T operator ~(ptr<T> value) => ref value.m_value;
        // or ideally C# supported an overloaded ref-based pointer operator, like:
        //     public static ref T operator *(ptr<T> value) => ref value.m_value;
        // or maybe even an overall "ref" operator for a class, like:
        //     public static ref T operator ref(ptr<T> value) => ref value.m_value;
        // Converted code like this:
        //     var v = 2; var vp = ptr(v);
        //     vp.Value = 999;
        // Could then become:
        //     var v = 2; var vp = ptr(v);
        //     ~vp = 999; // or
        //     *vp = 999; // or
        //     ref vp = 999;
        // As it stands, this operator just returns a copy of the structure value:
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T operator ~(ptr<T> value) => value.m_value;

        // I posted a suggestion for at least the "ref" operator:
        // https://github.com/dotnet/roslyn/issues/45881

        // Also, the following unsafe return option is possible when T is unmanaged:
        //     public static T* operator ~(ptr<T> value) => value.m_value;
        // However, going down the fully unmanaged path creates a cascading set of
        // issues, see header comments for the ptr<T> "experimental" implementation

        // Enable comparisons between nil and @ref<T> interface instance
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ptr<T> value, NilType _) => value.Equals(null!);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ptr<T> value, NilType nil) => !(value == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, ptr<T> value) => value == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, in ptr<T> value) => value != nil;

        private static readonly bool IsReferenceType = default(T) is null;
    }
}
