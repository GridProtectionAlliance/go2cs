//******************************************************************************************************
//  ж.cs - Gbtc
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
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using go.runtime;

[assembly:InternalsVisibleTo("unsafe")]

namespace go;

/// <summary>
/// Delegate that returns a <c>ref</c> to a field value within a struct <see cref="ж{T}"/> reference.
/// </summary>
/// <typeparam name="TElem">Field type.</typeparam>
/// <param name="structPtr"><see cref="ж{T}"/> heap reference of struct.</param>
/// <returns>A <c>ref</c> to the field value within the struct <see cref="ж{T}"/> reference.</returns>
public delegate ref TElem FieldRefFunc<TElem>(object structPtr);

/// <summary>
/// Delegate that returns a <c>ref</c> to a field value within a struct <see cref="ж{T}"/> reference.
/// </summary>
/// <typeparam name="T">Struct type.</typeparam>
/// <typeparam name="TElem">Field type.</typeparam>
/// <param name="structRef">Reference to struct.</param>
/// <returns>A <c>ref</c> to the field value within the struct <see cref="ж{T}"/> reference.</returns>
public delegate ref TElem FieldRefFunc<T, TElem>(ref T structRef);

/// <summary>
/// Helper class for creating a <see cref="FieldRefFunc{TElem}"/> delegate for a struct field.
/// </summary>
/// <typeparam name="T">Type of struct.</typeparam>
public static class FieldRef<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields)] T> where T : struct
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
        // Get the FieldInfo for fieldName in struct T referenced by ж<T>
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

        // Emit IL code to load the field address: ((ж<T>)obj).m_val.fieldName
        il.Emit(OpCodes.Ldarg_0);               // Load the object argument
        il.Emit(OpCodes.Castclass, s_ptrType);  // Cast to ж<T>
        il.Emit(OpCodes.Ldflda, s_ptrValField); // Load address of ж<T>.m_val struct, type &T
        il.Emit(OpCodes.Ldflda, structField);   // Load address of m_val struct field, type &TElem
        il.Emit(OpCodes.Ret);                   // Return

        // Create the delegate
        return (FieldRefFunc<TElem>)method.CreateDelegate(typeof(FieldRefFunc<TElem>));
    }

    // Type of ж<T>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicFields)]
    private static readonly Type s_ptrType = typeof(ж<T>);

    // FieldInfo for m_val in ж<T>
    private static readonly FieldInfo s_ptrValField = s_ptrType.GetField("m_val", BindingFlags.Instance | BindingFlags.NonPublic)!;
}

/// <summary>
/// Defines an interface that represents a pointer <see cref="ж{T}"/> type.
/// </summary>
/// <typeparam name="T">Type for heap based reference.</typeparam>
public interface IPointer<T>
{
    /// <summary>
    /// Gets a reference to the value of type <typeparamref name="T"/>.
    /// </summary>
    /// <exception cref="PanicException">runtime error: invalid memory address or nil pointer dereference</exception>
    ref T val { get; }

    /// <summary>
    /// Gets flag indicating if the pointer is null.
    /// </summary>
    bool IsNull { get; }

    /// <summary>
    /// Gets a pointer to the field of a struct.
    /// </summary>
    /// <typeparam name="TElem">Type of field.</typeparam>
    /// <param name="fieldRefFunc">Struct field reference delegate.</param>
    /// <returns>Pointer to field of struct.</returns>
    ж<TElem> of<TElem>(FieldRefFunc<TElem> fieldRefFunc);

    /// <summary>
    /// Gets a pointer to the field of a struct.
    /// </summary>
    /// <typeparam name="TElem">Type of field.</typeparam>
    /// <param name="fieldRefFunc">Struct field reference delegate.</param>
    /// <returns>Pointer to field of struct.</returns>
    ж<TElem> of<TElem>(FieldRefFunc<T, TElem> fieldRefFunc);

    /// <summary>
    /// Gets a pointer to element at the specified index for <see cref="array{T}"/> or <see cref="slice{T}"/> types.
    /// </summary>
    /// <typeparam name="TElem">Element type of array or slice.</typeparam>
    /// <param name="index">Index of element to get pointer for.</param>
    /// <returns>Pointer to element at specified index.</returns>
    ж<TElem> at<TElem>(nint index);

    /// <summary>
    /// Dereferences the heap allocated reference to the value of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="value">Pointer value to dereference.</param>
    /// <returns>Dereferenced pointer value.</returns>
    static abstract T operator ~(IPointer<T> value);
}

/// <summary>
/// Represents a pointer to a heap allocated instance of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">Type for heap based reference.</typeparam>
/// <remarks>
/// <para>
/// A new .NET class instance is always allocated on the heap and registered for garbage collection.
/// The <see cref="ж{T}"/> class is used to create a reference to a heap allocated instance of type
/// <typeparamref name="T"/> so that the type can (1) have scope beyond the current stack, and (2)
/// have the ability to create a safe pointer to the type, i.e., a reference.
/// </para>
/// <para>
/// If <typeparamref name="T"/> is a <see cref="System.ValueType"/>, e.g., a struct, note that value
/// will be "boxed" for heap allocation. Since boxed value will be a new copy of original value, make
/// sure to use ref-based <see cref="val"/> for updates instead of a local stack copy of value.
/// See the <see cref="builtin.heap{T}(out ж{T})"/> and notes on boxing:
/// https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/types/boxing-and-unboxing
/// </para>
/// <para>
/// So long as a reference to this class exists, so will the value of type <typeparamref name="T"/>.
/// </para>
/// </remarks>
public class ж<T> : IPointer<T>, IEquatable<ж<T>>
{
    private readonly (object, FieldRefFunc<T>)? m_structFieldRef;
    private readonly (IArray, int)? m_arrayIndexRef;
    private readonly bool m_isNull;
    private T m_val;

    /// <summary>
    /// Creates a new pointer to heap allocated instance of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="value">Source value for heap allocated reference.</param>
    public ж(in T value)
    {
        m_val = value;
    }

    // Create a new reference to a field in a heap allocated struct
    internal ж(object source, FieldRefFunc<T> fieldRefFunc)
    {
        m_structFieldRef = (source, fieldRefFunc);
        m_val = default!;
    }

    // Create a new indexed reference into an existing heap allocated array
    internal ж(IArray array, int index)
    {
        m_arrayIndexRef = (array, index);
        m_val = default!;
    }

    /// <summary>
    /// Creates a new pointer from a nil value.
    /// </summary>
    /// <param name="_"></param>
    public ж(NilType _)
    {
        m_val = default!;
        m_isNull = true;
    }

    /// <summary>
    /// Creates a new nil pointer.
    /// </summary>
    public ж() : this(nil) { }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Cannot get reference to value, source is not a valid array or slice pointer.</exception>
    public ref T val
    {
        get
        {
            // Get reference to standard pointer value
            if (m_structFieldRef is null && m_arrayIndexRef is null)
            {
                if (IsNull)
                    throw RuntimeErrorPanic.NilPointerDereference();

                return ref m_val!;
            }

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
    /// Gets a reference to the value slot WITHOUT the nil-pointer-dereference check that <see cref="val"/>
    /// performs — identical to <see cref="val"/> except it never throws.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used only where this box is a real heap allocation (created via <c>Ꮡ</c> / <c>heap</c>, so it is
    /// structurally a non-nil pointer) AND its value is a <em>reference</em> type that may legitimately
    /// be null — a heap-boxed pointer, slice, map, interface, or func <em>local</em> captured by a
    /// closure (a <c>ж&lt;ж&lt;T&gt;&gt;</c>, etc.). There <c>.val</c> is a <em>read of the held value</em>,
    /// not a dereference of this box, so it must not panic when the held value is null: in Go,
    /// <c>*(&amp;p)</c> where <c>p</c> is a nil <c>*T</c>/slice/map yields the nil value, no dereference
    /// happens. Unlike <see cref="PointerExtensions.DerefOrNil{T}"/> (which returns a throwaway slot for a
    /// nil box, so writes are lost), this returns the <em>real</em> slot — reads and writes both persist,
    /// which the captured local requires. A genuine nil-pointer dereference (<c>~Ꮡp</c> / <c>Ꮡp.val</c>)
    /// still routes through the strict <see cref="val"/> and panics, preserving Go semantics.
    /// </para>
    /// </remarks>
    public ref T ValueSlot
    {
        get
        {
            if (m_structFieldRef is null && m_arrayIndexRef is null)
                return ref m_val!;

            if (m_structFieldRef is not null)
            {
                (object source, FieldRefFunc<T> fieldRefFunc) = m_structFieldRef!.Value;
                return ref fieldRefFunc(source);
            }

            (IArray array, int index) = m_arrayIndexRef!.Value;

            if (array is IArray<T> typedArray)
                return ref typedArray[index];

            throw new InvalidOperationException("Cannot get reference to value, source is not a valid struct field, array or slice reference.");
        }
    }

    /// <inheritdoc/>
    public bool IsNull => m_isNull || m_val is null;

    /// <summary>
    /// Gets a flag indicating whether this is a nil <em>standard</em> pointer — a plain heap pointer
    /// whose value is unset — as opposed to a struct-field or array-element reference (which resolve
    /// through <see cref="val"/> without a null check). This is exactly the case for which the
    /// <see cref="val"/> getter throws <see cref="RuntimeErrorPanic.NilPointerDereference"/>; the
    /// nil-safe <see cref="PointerExtensions.DerefOrNil{T}"/> re-alias accessor uses it to avoid that
    /// throw when re-aliasing a pointer parameter walked to a nil terminator.
    /// </summary>
    internal bool IsNilStandardPointer => m_structFieldRef is null && m_arrayIndexRef is null && IsNull;

    /// <summary>
    /// Gets a pinned pointer to the value of type <typeparamref name="T"/>.
    /// </summary>
    internal PinnedBuffer PinnedBuffer
    {
        get
        {
            // Get reference to standard pointer value
            if (m_structFieldRef is null && m_arrayIndexRef is null)
            {
                if (IsNull)
                    throw RuntimeErrorPanic.NilPointerDereference();
                
                return new PinnedBuffer(val, Marshal.SizeOf<T>());
            }

            // Get reference to struct field
            if (m_structFieldRef is not null)
            {
                (object source, FieldRefFunc<T> _) = m_structFieldRef!.Value;
                return new PinnedBuffer(val, Marshal.SizeOf(source));
            }

            // Get reference to array or slice element
            (IArray array, int _) = m_arrayIndexRef!.Value;

            if (array is IArray<T>)
                return new PinnedBuffer(val, array.Length);

            throw new InvalidOperationException("Cannot get pinned buffer to value, source is not a valid struct field, array or slice reference.");
        }
    }

    internal (IArray, int)? ArrayRef => m_arrayIndexRef;

    /// <inheritdoc/>
    public ж<TElem> of<TElem>(FieldRefFunc<TElem> fieldRefFunc)
    {
        return new ж<TElem>(this, fieldRefFunc);
    }

    /// <inheritdoc/>
    public ж<TElem> of<TElem>(FieldRefFunc<T, TElem> fieldRefFunc)
    {
        return new ж<TElem>(this, getFieldRef);

        ref TElem getFieldRef(object structPtr)
        {
            ж<T> typedPtr = (ж<T>)structPtr;

            // Resolve the parent value through `val`, not `m_val` — when this pointer is itself a
            // field reference (or array element) its real storage lives behind `val` and `m_val` is
            // an empty default. This is the case for a nested `of()` chain, e.g.
            // `Ꮡb.of(Bool.Ꮡu).of(Uint8.Ꮡvalue)` where the intermediate `ж<Uint8>` is a field ref —
            // reading `m_val` would alias a throwaway copy and lose writes.
            return ref fieldRefFunc(ref typedPtr.val);
        }
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Cannot get pointer element at index, type is not an array or slice.</exception>
    /// <exception cref="IndexOutOfRangeException">Index is out of range for array or slice.</exception>
    public ж<Telem> at<Telem>(nint index)
    {
        // Read through `val`, not `m_val` — when this pointer is itself a field reference (from
        // `of(...)`) or an array-element reference, the real array storage lives behind `val` and
        // `m_val` is an empty default. Reading `m_val` would miss the array entirely (spurious "not
        // an array" / null deref). This is the `Ꮡg.of(T.ᏑarrayField).at<E>(i)` form — the address of
        // an element of an array FIELD of a boxed struct (a boxed global, a pointer param/local).
        // `array<T>` is a readonly struct over a shared backing `T[]`, so the copy `val` yields still
        // aliases the real elements (writes through the returned element pointer land).
        if (val is not IArray<Telem> array)
            throw new InvalidOperationException("Cannot get pointer to element at index, type is not an array or slice.");
        
        if (!array.IndexIsValid(index))
            throw new IndexOutOfRangeException("Index is out of range for array or slice.");

        return new ж<Telem>(array, (int)index);
    }

    /// <summary>
    /// Gets a pointer to the element at <paramref name="index"/> of an <see cref="array{T}"/> or
    /// <see cref="slice{T}"/> FIELD of the pointed-to struct — the address of `x.field[index]`.
    /// </summary>
    /// <remarks>
    /// Combines <see cref="of{TElem}(FieldRefFunc{T,TElem})"/> and <see cref="at{Telem}(nint)"/> into a
    /// single call so the element type is INFERRED from the field accessor's return type, letting the
    /// converter emit `Ꮡx.at(T.Ꮡfield, i)` instead of the noisier `Ꮡx.of(T.Ꮡfield).at&lt;E&gt;(i)`.
    /// One overload per field-accessor shape (`ref T`/`object`) and collection kind (array/slice); each
    /// just forwards to the existing `of(...).at&lt;TElem&gt;(index)`.
    /// </remarks>
    public ж<TElem> at<TElem>(FieldRefFunc<T, array<TElem>> fieldRefFunc, nint index) => of(fieldRefFunc).at<TElem>(index);

    /// <inheritdoc cref="at{TElem}(FieldRefFunc{T,array{TElem}},nint)"/>
    public ж<TElem> at<TElem>(FieldRefFunc<array<TElem>> fieldRefFunc, nint index) => of(fieldRefFunc).at<TElem>(index);

    /// <inheritdoc cref="at{TElem}(FieldRefFunc{T,array{TElem}},nint)"/>
    public ж<TElem> at<TElem>(FieldRefFunc<T, slice<TElem>> fieldRefFunc, nint index) => of(fieldRefFunc).at<TElem>(index);

    /// <inheritdoc cref="at{TElem}(FieldRefFunc{T,array{TElem}},nint)"/>
    public ж<TElem> at<TElem>(FieldRefFunc<slice<TElem>> fieldRefFunc, nint index) => of(fieldRefFunc).at<TElem>(index);

    /// <inheritdoc />
    public override string ToString()
    {
        // $"&{m_val?.ToString() ?? "nil"}";
        return this.PrintPointer();
    }

    /// <inheritdoc/>
    public bool Equals(ж<T>? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        // A nil pointer compares equal only to another nil pointer.
        if (IsNull || other.IsNull)
            return IsNull && other.IsNull;

        // Go pointer comparison is by identity — the same storage location — NOT by the pointed-to
        // value. A value-comparison fallback is both wrong (two distinct addresses that happen to
        // hold equal values are not equal pointers in Go) and unsound: it infinitely recurses for a
        // self-referential struct, one whose fields include a ж<T> back to its own type (e.g.
        // container/ring's `Ring.next *Ring`), since the struct's own Equals compares those fields.

        // Pointer into a struct field (`Ꮡx.of(T.ᏑField)`): same source object and field accessor.
        if (m_structFieldRef is not null || other.m_structFieldRef is not null)
        {
            if (m_structFieldRef is null || other.m_structFieldRef is null)
                return false;

            (object source1, FieldRefFunc<T> fieldRef1) = m_structFieldRef.Value;
            (object source2, FieldRefFunc<T> fieldRef2) = other.m_structFieldRef.Value;

            return ReferenceEquals(source1, source2) && fieldRef1.Equals(fieldRef2);
        }

        // Pointer into an array/slice element: same backing array and same index.
        if (m_arrayIndexRef is not null || other.m_arrayIndexRef is not null)
        {
            if (m_arrayIndexRef is null || other.m_arrayIndexRef is null)
                return false;

            (IArray array1, int index1) = m_arrayIndexRef.Value;
            (IArray array2, int index2) = other.m_arrayIndexRef.Value;

            return ReferenceEquals(array1, array2) && index1 == index2;
        }

        // Two standard heap pointers, each a distinct allocation: equal only if they wrap the same
        // reference-type object (the ReferenceEquals(this, other) check above already handled the
        // same-box case, so distinct value-type allocations are distinct addresses → not equal).
        return IsReferenceType && ReferenceEquals(m_val, other.m_val);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is ж<T> other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // Identity-based hash, consistent with the identity Equals above. Hashing m_val directly
        // would recurse for a self-referential struct (its hash includes its ж<T> fields), so hash
        // by the referenced storage instead.
        if (IsNull)
            return 0;

        if (m_structFieldRef is not null)
            return RuntimeHelpers.GetHashCode(m_structFieldRef.Value.Item1);

        if (m_arrayIndexRef is not null)
            return System.HashCode.Combine(RuntimeHelpers.GetHashCode(m_arrayIndexRef.Value.Item1), m_arrayIndexRef.Value.Item2);

        // Standard heap pointer: a reference-type value uses the wrapped object's identity (equal
        // pointers share it); a value-type box uses this box's own identity.
        return IsReferenceType && m_val is not null
            ? RuntimeHelpers.GetHashCode(m_val)
            : RuntimeHelpers.GetHashCode(this);
    }

    // WISH: Would be super cool if this operator supported "ref" return, like:
    //     public static ref T operator ~(ж<T> value) => ref value.m_value;
    // or ideally C# supported an overloaded ref-based pointer operator, like:
    //     public static ref T operator *(ж<T> value) => ref value.m_value;
    // or maybe even an overall "ref" operator for a class, like:
    //     public static ref T operator ref(ж<T> value) => ref value.m_value;
    // Converted code like this:
    //     var v = 2; var vp = ptr(v);
    //     vp.val = 999;
    // Could then become:
    //     var v = 2; var vp = ptr(v);
    //     ~vp = 999; // or
    //     *vp = 999; // or
    //     ref vp = 999;
    // As it stands, this operator just returns a copy of the structure value:

    /// <summary>
    /// Dereferences the heap allocated reference to the value of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="value">Pointer value to dereference.</param>
    /// <returns>Dereferenced pointer value.</returns>
    public static T operator ~(ж<T> value)
    {
        if (value.IsNull)
            throw RuntimeErrorPanic.NilPointerDereference();

        // Resolve through `val`, not the raw `m_val` field: for a struct-field reference (`Ꮡx.of(T.Ꮡf)`)
        // or an array-element reference, the real storage lives behind `val` and `m_val` is an empty
        // default — returning `m_val` would yield a zero-valued copy (so `(~(&x.field)).sub` read 0, e.g.
        // a `c := &b.w; c.a` field-chain read). For a standard pointer `val` returns `m_val`, so this is
        // identical there. Matches the IPointer<T>.operator ~ above, which already resolves via `val`.
        return value.val;
    }

    static T IPointer<T>.operator ~(IPointer<T> value)
    {
        if (value.IsNull)
            throw RuntimeErrorPanic.NilPointerDereference();

        return value.val;
    }

    // I posted a suggestion for at least the "ref" operator:
    // https://github.com/dotnet/roslyn/issues/45881

    // Also, the following unsafe return option is possible when T is unmanaged:
    //     public static T* operator ~(ж<T> value) => value.m_value;
    // However, going down the fully unmanaged path creates a cascading set of
    // issues, see header comments for the ж<T> "experimental" implementation

    public static bool operator ==(ж<T>? value1, ж<T>? value2)
    {
        return value1?.Equals(value2) ?? value2 is null;
    }

    public static bool operator !=(ж<T>? value1, ж<T>? value2)
    {
        return !(value1 == value2);
    }

    // Enable comparisons between nil and ж<T> instance
    public static bool operator ==(ж<T>? value, NilType _)
    {
        return value?.IsNull ?? true;
    }

    public static bool operator !=(ж<T>? value, NilType nil)
    {
        return !(value == nil);
    }

    public static bool operator ==(NilType nil, ж<T>? value)
    {
        return value == nil;
    }

    public static bool operator !=(NilType nil, ж<T>? value)
    {
        return value != nil;
    }

    public static implicit operator ж<T>(NilType _)
    {
        return new ж<T>(nil);
    }

    // EXPLICIT by design: reinterpreting a raw address as a pointer BOXES A COPY of the pointed-at
    // value (the runtime-unsafe reinterpret seam) and dereferences an arbitrary address — never
    // something to happen silently. Converter-emitted reinterprets always use explicit cast syntax
    // ((ж<T>)(uintptr)(p)). As an implicit conversion it also made every uintptr argument ambiguous
    // between an unsafe.Pointer overload and a ж<T> overload (CS0121 — runtime's free `add(p, x)`
    // vs the `(*notInHeap).add` static companion).
    public static unsafe explicit operator ж<T>(uintptr value)
    {
        return new ж<T>(*(T*)value);
    }

    public static unsafe implicit operator uintptr(ж<T> value)
    {
        fixed (void* ptr = &value.val)
            return (uintptr)ptr;
    }

    public static unsafe implicit operator ж<T>(void* value)
    {
        return new ж<T>(*(T*)value);
    }

    public static unsafe implicit operator void*(ж<T> value)
    {
        fixed (T* ptr = &value.val)
            return ptr;
    }

    private static readonly bool IsReferenceType = default(T) is null;
}

/// <summary>
/// Extension methods for <see cref="ж{T}"/> pointer references.
/// </summary>
public static class PointerExtensions
{
    /// <summary>
    /// Nil-safe re-alias accessor: returns a reference to the pointed-to value, or to a shared
    /// default(<typeparamref name="T"/>) slot when <paramref name="box"/> is a nil pointer
    /// (a <c>null</c> reference or a nil standard pointer).
    /// </summary>
    /// <typeparam name="T">Pointed-to type.</typeparam>
    /// <param name="box">Pointer reference, which may be <c>null</c>.</param>
    /// <returns>
    /// A <c>ref</c> to the value of <paramref name="box"/>, or to a shared default slot when it is nil.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Converted code re-aliases a deref'd pointer parameter after repointing its box, e.g.
    /// <c>Ꮡp = p.next; p = ref Ꮡp.DerefOrNil();</c>, when the parameter is walked to a nil terminator
    /// (<c>for p != nil { …; p = p.next }</c>). On the final step the box becomes nil, and the plain
    /// <see cref="ж{T}.val"/> getter would throw a nil-pointer dereference before the loop guard is
    /// re-checked. This accessor instead yields a <c>ref</c> to a throwaway default slot — never read
    /// while the box is nil (the <c>Ꮡp != nil</c> guard excludes it), so the value is harmless.
    /// </para>
    /// <para>
    /// This is <em>not</em> a substitute for a genuine dereference: reading or writing <c>*p</c> on a
    /// nil pointer (emitted as <c>~Ꮡp</c> / <c>Ꮡp.val</c>) still panics, preserving Go semantics. Only
    /// the re-alias — which captures a reference without reading it — uses the nil-safe form. As an
    /// extension method it tolerates a <c>null</c> receiver (a nil pointer field is a <c>null</c>
    /// reference, not a nil box).
    /// </para>
    /// </remarks>
    public static ref T DerefOrNil<T>(this ж<T>? box)
    {
        if (box is null || box.IsNilStandardPointer)
            return ref NilSlot<T>.Slot;

        return ref box.val;
    }

    // Per-T shared default slot returned by ref for a nil pointer. Never read while the pointer is
    // nil (the converted loop guard excludes it), so the shared mutable storage is benign.
    private static class NilSlot<T>
    {
        public static T Slot = default!;
    }
}
