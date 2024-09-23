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

namespace go;

/// <summary>
/// Represents a heap allocated reference to an instance of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">Type for heap based reference.</typeparam>
/// <remarks>
/// <para>
/// A new .NET class instance is always allocated on the heap and registered for garbage collection.
/// The <see cref="ptr{T}"/> class is used to create a reference to a heap allocated instance of type
/// <typeparamref name="T"/> so that the type can (1) have scope beyond the current stack, and (2)
/// have the ability to create a safe pointer to the type, i.e., a reference.
/// </para>
/// <para>
/// If <typeparamref name="T"/> is a <see cref="System.ValueType"/>, e.g., a struct, note that value
/// will be "boxed" for heap allocation. Since boxed value will be a new copy of original value, make
/// sure to use ref-based <see cref="val"/> for updates instead of a local stack copy of value.
/// See the <see cref="builtin.heap{T}(out ptr{T})"/> and notes on boxing:
/// https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/types/boxing-and-unboxing
/// </para>
/// <para>
/// So long as a reference to this class exists, so will the value of type <typeparamref name="T"/>.
/// </para>
/// </remarks>
public sealed class ptr<T>
{
    private T m_val;

    public ptr(in T value) => m_val = value;

    public ptr(NilType _) : this(default(T)!) { }

    public ptr() : this(default(T)!) { }

    public ref T val
    {
        get => ref m_val;
    }

    public override string ToString() => $"&{m_val?.ToString() ?? "nil"}";

    private bool Equals(ptr<T>? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (IsReferenceType)
        {
            if (m_val is null && other.m_val is null)
                return true;

            if (m_val is null || other.m_val is null)
                return false;

            if (ReferenceEquals(m_val, other.m_val))
                return true;
        }

        return m_val!.Equals(other.m_val);
    }

    public override bool Equals(object? obj) => obj is ptr<T> other && Equals(other);

    // ReSharper disable once NonReadonlyMemberInGetHashCode
    public override int GetHashCode() => m_val?.GetHashCode() ?? 0;

    // WISH: Would be super cool if this operator supported "ref" return, like:
    //     public static ref T operator ~(ptr<T> value) => ref value.m_value;
    // or ideally C# supported an overloaded ref-based pointer operator, like:
    //     public static ref T operator *(ptr<T> value) => ref value.m_value;
    // or maybe even an overall "ref" operator for a class, like:
    //     public static ref T operator ref(ptr<T> value) => ref value.m_value;
    // Converted code like this:
    //     var v = 2; var vp = ptr(v);
    //     vp.val = 999;
    // Could then become:
    //     var v = 2; var vp = ptr(v);
    //     ~vp = 999; // or
    //     *vp = 999; // or
    //     ref vp = 999;
    // As it stands, this operator just returns a copy of the structure value:
    public static T operator ~(ptr<T> value) => value.m_val;

    // I posted a suggestion for at least the "ref" operator:
    // https://github.com/dotnet/roslyn/issues/45881

    // Also, the following unsafe return option is possible when T is unmanaged:
    //     public static T* operator ~(ptr<T> value) => value.m_value;
    // However, going down the fully unmanaged path creates a cascading set of
    // issues, see header comments for the ptr<T> "experimental" implementation

    // Enable comparisons between nil and @ref<T> interface instance
    public static bool operator ==(ptr<T>? value, NilType _) => value is null;

    public static bool operator !=(ptr<T>? value, NilType nil) => !(value == nil);

    public static bool operator ==(NilType nil, ptr<T>? value) => value == nil;

    public static bool operator !=(NilType nil, in ptr<T>? value) => value != nil;

    private static readonly bool IsReferenceType = default(T) is null;
}
