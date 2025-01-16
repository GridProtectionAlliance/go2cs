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
// ReSharper disable StaticMemberInGenericType
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedParameter.Local

using System;
using System.Reflection;
using System.Reflection.Emit;

namespace go;

/// <summary>
/// Delegate that returns a <c>ref</c> to a field value within a struct <see cref="ptr{T}"/> reference.
/// </summary>
/// <typeparam name="TElem">Field type.</typeparam>
/// <param name="structPtr"><see cref="ptr{T}"/> heap reference of struct.</param>
/// <returns>A <c>ref</c> to the field value within the struct <see cref="ptr{T}"/> reference.</returns>
public delegate ref TElem FieldRefFunc<TElem>(object structPtr);

/// <summary>
/// Delegate that returns a <c>ref</c> to a field value within a struct <see cref="ptr{T}"/> reference.
/// </summary>
/// <typeparam name="T">Struct type.</typeparam>
/// <typeparam name="TElem">Field type.</typeparam>
/// <param name="structRef">Reference to struct.</param>
/// <returns>A <c>ref</c> to the field value within the struct <see cref="ptr{T}"/> reference.</returns>
public delegate ref TElem FieldRefFunc<T, TElem>(ref T structRef);

/// <summary>
/// Helper class for creating a <see cref="FieldRefFunc{TElem}"/> delegate for a struct field.
/// </summary>
/// <typeparam name="T">Type of struct.</typeparam>
public static class FieldRef<T> where T : struct
{
    /// <summary>
    /// Creates a <see cref="FieldRefFunc{TElem}"/> delegate for a struct field.
    /// </summary>
    /// <param name="fieldName">Field name.</param>
    /// <returns> A <see cref="FieldRefFunc{TElem}"/> delegate for a struct field. </returns>
    /// <typeparam name="TElem">Type of field.</typeparam>
    /// <exception cref="InvalidOperationException">
    /// Field <paramref name="fieldName"/> not found in type <typeparamref name="T"/>.
    /// </exception>
    public static FieldRefFunc<TElem> Create<TElem>(string fieldName)
    {
        // Get the FieldInfo for fieldName in struct T referenced by ptr<T>
        FieldInfo structField = typeof(T).GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                             ?? throw new InvalidOperationException($"Field '{fieldName}' not found in type {typeof(T).FullName}");

        // Create a dynamic method that matches the delegate signature
        DynamicMethod method = new(
            name: $"ref_{fieldName}",
            returnType: typeof(TElem).MakeByRefType(),
            parameterTypes: [typeof(object)],
            m: typeof(FieldRef<>).Module, // Use the module where this code is running
            skipVisibility: true);

        ILGenerator il = method.GetILGenerator();

        // Emit IL code to load the field address: ((ptr<T>)obj).m_val.fieldName
        il.Emit(OpCodes.Ldarg_0);               // Load the object argument
        il.Emit(OpCodes.Castclass, s_ptrType);  // Cast to ptr<T>
        il.Emit(OpCodes.Ldflda, s_ptrValField); // Load address of ptr<T>.m_val struct, type &T
        il.Emit(OpCodes.Ldflda, structField);   // Load address of m_val struct field, type &TElem
        il.Emit(OpCodes.Ret);                   // Return

        // Create the delegate
        return (FieldRefFunc<TElem>)method.CreateDelegate(typeof(FieldRefFunc<TElem>));
    }

    // Type of ptr<T>
    private static readonly Type s_ptrType = typeof(ptr<T>);

    // FieldInfo for m_val in ptr<T>
    private static readonly FieldInfo s_ptrValField = s_ptrType.GetField("m_val", BindingFlags.Instance | BindingFlags.NonPublic)!;
}

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
public partial class ptr<T>
{
    private readonly (object, FieldRefFunc<T>)? m_structFieldRef;
    private readonly (IArray, int)? m_arrayIndexRef;
    private T m_val;

    /// <summary>
    /// Creates a new heap allocated reference to an instance of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="value">Source value for heap allocated reference.</param>
    public ptr(in T value)
    {
        m_val = value;
    }

    // Create a new reference to a field in a heap allocated struct
    internal ptr(object source, FieldRefFunc<T> fieldRefFunc)
    {
        m_structFieldRef = (source, fieldRefFunc);
        m_val = default!;
    }

    // Create a new indexed reference into an existing heap allocated array
    internal ptr(IArray array, int index)
    {
        m_arrayIndexRef = (array, index);
        m_val = default!;
    }

    /// <summary>
    /// Creates a new heap allocated reference from a nil value.
    /// </summary>
    /// <param name="_"></param>
    public ptr(NilType _) : this(default(T)!) { }

    /// <summary>
    /// Creates a new heap allocated reference.
    /// </summary>
    public ptr() : this(default(T)!) { }

    /// <summary>
    /// Gets a reference to the value of type <typeparamref name="T"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Cannot get reference to value, source is not a valid array or slice pointer.</exception>
    public ref T val
    {
        get
        {
            // Get reference to pointer value
            if (m_structFieldRef is null && m_arrayIndexRef is null)
                return ref m_val;

            // Get reference to struct field
            if (m_structFieldRef is not null)
            {
                (object source, FieldRefFunc<T> fieldRefFunc) = m_structFieldRef!.Value;
                return ref fieldRefFunc(source);
            }

            // Get reference to array or slice element
            (IArray array, int index) = m_arrayIndexRef!.Value;

            if (array is IArray<T> typedArray)
                return ref typedArray[index];

            throw new InvalidOperationException("Cannot get reference to value, source is not a valid struct field, array or slice reference.");
        }
    }

    /// <summary>
    /// Gets a pointer to the field of a struct.
    /// </summary>
    /// <typeparam name="TElem">Type of field.</typeparam>
    /// <param name="fieldRefFunc">Struct field reference delegate.</param>
    /// <returns>Pointer to field of struct.</returns>
    public ptr<TElem> of<TElem>(FieldRefFunc<TElem> fieldRefFunc)
    {
        return new ptr<TElem>(this, fieldRefFunc);
    }

    /// <summary>
    /// Gets a pointer to the field of a struct.
    /// </summary>
    /// <typeparam name="TElem">Type of field.</typeparam>
    /// <param name="fieldRefFunc">Struct field reference delegate.</param>
    /// <returns>Pointer to field of struct.</returns>
    public ptr<TElem> of<TElem>(FieldRefFunc<T, TElem> fieldRefFunc)
    {
        return new ptr<TElem>(this, getFieldRef);

        ref TElem getFieldRef(object structPtr)
        {
            ptr<T> ptr = (ptr<T>)structPtr;
            return ref fieldRefFunc(ref ptr.m_val);
        }
    }

    /// <summary>
    /// Gets a pointer to element at the specified index for <see cref="array{T}"/> or <see cref="slice{T}"/> types.
    /// </summary>
    /// <typeparam name="Telem">Element type of array or slice.</typeparam>
    /// <param name="index">Index of element to get pointer for.</param>
    /// <returns>Pointer to element at specified index.</returns>
    /// <exception cref="InvalidOperationException">Cannot get pointer element at index, type is not an array or slice.</exception>
    /// <exception cref="IndexOutOfRangeException">Index is out of range for array or slice.</exception>
    public ptr<Telem> at<Telem>(int index)
    {
        if (m_val is not IArray<Telem> array)
            throw new InvalidOperationException("Cannot get pointer to element at index, type is not an array or slice.");
        
        if (!array.IndexIsValid(index))
            throw new IndexOutOfRangeException("Index is out of range for array or slice.");

        return new ptr<Telem>(array, index);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"&{m_val?.ToString() ?? "nil"}";
    }

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

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is ptr<T> other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return m_val?.GetHashCode() ?? 0;
    }

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
    public static T operator ~(ptr<T> value)
    {
        return value.m_val;
    }

    // I posted a suggestion for at least the "ref" operator:
    // https://github.com/dotnet/roslyn/issues/45881

    // Also, the following unsafe return option is possible when T is unmanaged:
    //     public static T* operator ~(ptr<T> value) => value.m_value;
    // However, going down the fully unmanaged path creates a cascading set of
    // issues, see header comments for the ptr<T> "experimental" implementation

    // Enable comparisons between nil and ptr<T> instance
    public static bool operator ==(ptr<T>? value, NilType _)
    {
        return value is null;
    }

    public static bool operator !=(ptr<T>? value, NilType nil)
    {
        return !(value == nil);
    }

    public static bool operator ==(NilType nil, ptr<T>? value)
    {
        return value == nil;
    }

    public static bool operator !=(NilType nil, ptr<T>? value)
    {
        return value != nil;
    }

    private static readonly bool IsReferenceType = default(T) is null;
}
