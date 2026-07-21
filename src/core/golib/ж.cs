// ж.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable StaticMemberInGenericType
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedParameter.Local

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using go.golib;

[assembly:InternalsVisibleTo("unsafe")]
[assembly:InternalsVisibleTo("GolibTests")]

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
    ref T Value { get; }

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
/// sure to use ref-based <see cref="Value"/> for updates instead of a local stack copy of value.
/// See the <see cref="builtin.heap{T}(out ж{T})"/> and notes on boxing:
/// https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/types/boxing-and-unboxing
/// </para>
/// <para>
/// So long as a reference to this class exists, so will the value of type <typeparamref name="T"/>.
/// </para>
/// </remarks>
public class ж<T> : IPointer<T>, IEquatable<ж<T>>
{
    // Item3 is the field's IDENTITY token for pointer equality: the ORIGINAL (typically static,
    // compiler-cached) field accessor delegate when the field ref was created through the typed
    // `of(FieldRefFunc<T, TElem>)` overload — that overload WRAPS the accessor in a per-call
    // closure (getFieldRef), so comparing the wrapper delegates made every distinct `&x.field`
    // box unequal (`&x.f == &x.f` was FALSE, violating Go pointer identity; it also broke the
    // address-keyed runtime semaphores in the hand-owned sync/internal-poll implementations).
    // Delegate.Equals compares method+target, so two conversions of the same accessor method
    // group compare equal across call sites.
    private readonly (object, FieldRefFunc<T>, Delegate)? m_structFieldRef;
    private readonly (IArray, int)? m_arrayIndexRef;
    private readonly bool m_isNull;
    private T m_val;

    // A NATIVE address this box aliases, rather than managed storage it owns (0 when this is an
    // ordinary managed box). This is the fourth reference kind, alongside the standard value, the
    // struct-field ref and the array-element ref above.
    //
    // It exists because a Win32 API can RETURN a pointer into native memory that the caller must
    // walk and then hand BACK to the OS — GetEnvironmentStringsW/FreeEnvironmentStringsW being the
    // canonical pair. Reinterpreting such an address by copying the pointed-at value into a managed
    // box (what the uintptr operator used to do) loses the address: the walk then scans the GC heap,
    // and returning the box's address to the OS asks it to free GC memory — observed as an outright
    // STATUS_HEAP_CORRUPTION (0xC0000374) process kill. A native-backed box instead aliases the real
    // address, so `.Value` reads the real memory and the uintptr/void* round-trip is EXACT.
    private readonly nuint m_nativeAddr;

    // Lazily-created pin of this box's fixed-array backing store, kept alive for the box's lifetime so a
    // native syscall can write into the array data and the managed reads afterward observe the result.
    // See the uintptr/void* operators and pinnedArrayData below. Freed when the box is collected (the
    // PinnedBuffer finalizer releases the GCHandle).
    private PinnedBuffer? m_pinnedArrayData;

    /// <summary>
    /// Creates a new pointer to heap allocated instance of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="value">Source value for heap allocated reference.</param>
    public ж(in T value)
    {
        m_val = value;
    }

    // Create a new reference to a field in a heap allocated struct. fieldIdentity carries the
    // original accessor delegate when fieldRefFunc is a per-call closure wrapper (see the typed
    // `of(...)` overload) so pointer equality compares the FIELD, not the wrapper instance.
    internal ж(object source, FieldRefFunc<T> fieldRefFunc, Delegate? fieldIdentity = null)
    {
        m_structFieldRef = (source, fieldRefFunc, fieldIdentity ?? fieldRefFunc);
        m_val = default!;
    }

    // Create a new indexed reference into an existing heap allocated array
    internal ж(IArray array, int index)
    {
        m_arrayIndexRef = (array, index);
        m_val = default!;
    }

    // Create a pointer that ALIASES a native address (see m_nativeAddr). A zero address is the
    // nil pointer, matching Go's `(*T)(unsafe.Pointer(uintptr(0))) == nil`.
    internal ж(nuint nativeAddress)
    {
        m_nativeAddr = nativeAddress;
        m_val = default!;
        m_isNull = nativeAddress == 0;
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
    public unsafe ref T Value
    {
        get
        {
            // Alias native memory at the address this box holds (never managed storage).
            if (m_nativeAddr != 0)
                return ref Unsafe.AsRef<T>((void*)m_nativeAddr);

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
                (object source, FieldRefFunc<T> fieldRefFunc, Delegate _) = m_structFieldRef!.Value;
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
    /// Gets a reference to the value slot WITHOUT the nil-pointer-dereference check that <see cref="Value"/>
    /// performs — identical to <see cref="Value"/> except it never throws.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used only where this box is a real heap allocation (created via <c>Ꮡ</c> / <c>heap</c>, so it is
    /// structurally a non-nil pointer) AND its value is a <em>reference</em> type that may legitimately
    /// be null — a heap-boxed pointer, slice, map, interface, or func <em>local</em> captured by a
    /// closure (a <c>ж&lt;ж&lt;T&gt;&gt;</c>, etc.). There <c>.Value</c> is a <em>read of the held value</em>,
    /// not a dereference of this box, so it must not panic when the held value is null: in Go,
    /// <c>*(&amp;p)</c> where <c>p</c> is a nil <c>*T</c>/slice/map yields the nil value, no dereference
    /// happens. Unlike <see cref="PointerExtensions.DerefOrNil{T}"/> (which returns a throwaway slot for a
    /// nil box, so writes are lost), this returns the <em>real</em> slot — reads and writes both persist,
    /// which the captured local requires. A genuine nil-pointer dereference (<c>~Ꮡp</c> / <c>Ꮡp.Value</c>)
    /// still routes through the strict <see cref="Value"/> and panics, preserving Go semantics.
    /// </para>
    /// </remarks>
    public unsafe ref T ValueSlot
    {
        get
        {
            // Alias native memory at the address this box holds (see Value).
            if (m_nativeAddr != 0)
                return ref Unsafe.AsRef<T>((void*)m_nativeAddr);

            if (m_structFieldRef is null && m_arrayIndexRef is null)
                return ref m_val!;

            if (m_structFieldRef is not null)
            {
                (object source, FieldRefFunc<T> fieldRefFunc, Delegate _) = m_structFieldRef!.Value;
                return ref fieldRefFunc(source);
            }

            (IArray array, int index) = m_arrayIndexRef!.Value;

            if (array is IArray<T> typedArray)
                return ref typedArray[index];

            throw new InvalidOperationException("Cannot get reference to value, source is not a valid struct field, array or slice reference.");
        }
    }

    /// <inheritdoc/>
    // A native-backed box is non-nil exactly when its address is non-zero — its m_val slot is unused
    // and would read as null for a reference-typed T, which must not be mistaken for a nil pointer.
    public bool IsNull => m_isNull || (m_nativeAddr == 0 && m_val is null);

    /// <summary>
    /// Gets a flag indicating whether this pointer ALIASES a native address rather than managed
    /// storage — see the <c>m_nativeAddr</c> field.
    /// </summary>
    internal bool IsNative => m_nativeAddr != 0;

    /// <summary>
    /// Gets the native address this pointer aliases, or zero when it is an ordinary managed box.
    /// </summary>
    internal nuint NativeAddress => m_nativeAddr;

    /// <summary>
    /// Gets a flag indicating whether this is a nil <em>standard</em> pointer — a plain heap pointer
    /// whose value is unset — as opposed to a struct-field or array-element reference (which resolve
    /// through <see cref="Value"/> without a null check). This is exactly the case for which the
    /// <see cref="Value"/> getter throws <see cref="RuntimeErrorPanic.NilPointerDereference"/>; the
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
                
                return new PinnedBuffer(Value, Marshal.SizeOf<T>());
            }

            // Get reference to struct field
            if (m_structFieldRef is not null)
            {
                (object source, FieldRefFunc<T> _, Delegate _) = m_structFieldRef!.Value;
                return new PinnedBuffer(Value, Marshal.SizeOf(source));
            }

            // Get reference to array or slice element
            (IArray array, int _) = m_arrayIndexRef!.Value;

            if (array is IArray<T>)
                return new PinnedBuffer(Value, array.Length);

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
        // fieldRefFunc doubles as the equality-identity token: getFieldRef is a NEW closure per
        // call, but the accessor delegate compares equal across call sites (same method group).
        return new ж<TElem>(this, getFieldRef, fieldRefFunc);

        ref TElem getFieldRef(object structPtr)
        {
            ж<T> typedPtr = (ж<T>)structPtr;

            // Resolve the parent value through `Value`, not `m_val` — when this pointer is itself a
            // field reference (or array element) its real storage lives behind `Value` and `m_val` is
            // an empty default. This is the case for a nested `of()` chain, e.g.
            // `Ꮡb.of(Bool.Ꮡu).of(Uint8.Ꮡvalue)` where the intermediate `ж<Uint8>` is a field ref —
            // reading `m_val` would alias a throwaway copy and lose writes.
            return ref fieldRefFunc(ref typedPtr.Value);
        }
    }

    // Materializes a value-type IArray implementer's lazy backing ON THE REAL STORAGE (see the
    // comment in at<Telem> below). Built once per T when T is a struct implementing IArray; the
    // constrained generic call (`value.Source` on `ref TVal`) does not box, so the wrapper's
    // `m_value ??= …` runs against the caller's ref. Null for every other T (zero overhead).
    private delegate void EnsureBackingFunc(ref T value);

    private static readonly EnsureBackingFunc? s_ensureArrayBacking = BuildEnsureArrayBacking();

    private static EnsureBackingFunc? BuildEnsureArrayBacking()
    {
        if (!typeof(T).IsValueType || !typeof(IArray).IsAssignableFrom(typeof(T)))
            return null;

        MethodInfo method = typeof(ж<T>).GetMethod(nameof(EnsureArrayBackingImpl), BindingFlags.Static | BindingFlags.NonPublic)!;
        return (EnsureBackingFunc)method.MakeGenericMethod(typeof(T)).CreateDelegate(typeof(EnsureBackingFunc));
    }

    private static void EnsureArrayBackingImpl<TVal>(ref TVal value) where TVal : IArray
    {
        _ = value.Source; // constrained call — lazy backings materialize on the real storage
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Cannot get pointer element at index, type is not an array or slice.</exception>
    /// <exception cref="IndexOutOfRangeException">Index is out of range for array or slice.</exception>
    public ж<Telem> at<Telem>(nint index)
    {
        // An Array-class [GoType] wrapper's ZERO VALUE has a null lazy backing that its members
        // allocate on first touch. The `Value is IArray<Telem>` pattern below COPIES the wrapper —
        // touched on the copy, the backing allocates on the copy and the REAL storage stays
        // virgin, silently dropping every write through the returned element pointer (the
        // pallocBits lesson, resurfacing at the box-element seam). Materialize the backing on
        // the real storage FIRST via a non-boxing constrained interface call; the copy then
        // SHARES the materialized backing (array<T> is a readonly struct over a shared T[]).
        s_ensureArrayBacking?.Invoke(ref Value);

        // Read through `Value`, not `m_val` — when this pointer is itself a field reference (from
        // `of(...)`) or an array-element reference, the real array storage lives behind `Value` and
        // `m_val` is an empty default. Reading `m_val` would miss the array entirely (spurious "not
        // an array" / null deref). This is the `Ꮡg.of(T.ᏑarrayField).at<E>(i)` form — the address of
        // an element of an array FIELD of a boxed struct (a boxed global, a pointer param/local).
        // `array<T>` is a readonly struct over a shared backing `T[]`, so the copy `Value` yields still
        // aliases the real elements (writes through the returned element pointer land).
        if (Value is not IArray<Telem> array)
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
        // The comparison uses the field IDENTITY token (Item3) — the original accessor delegate —
        // not the stored ref function, which the typed `of(...)` overload wraps in a PER-CALL
        // closure: comparing wrappers made every distinct `&x.field` box unequal, so `&x.f == &x.f`
        // was false (Go pointer identity violated) and the address-keyed runtime semaphores in the
        // hand-owned sync/internal-poll implementations never paired release with acquire.
        if (m_structFieldRef is not null || other.m_structFieldRef is not null)
        {
            if (m_structFieldRef is null || other.m_structFieldRef is null)
                return false;

            (object source1, FieldRefFunc<T> _, Delegate fieldId1) = m_structFieldRef.Value;
            (object source2, FieldRefFunc<T> _, Delegate fieldId2) = other.m_structFieldRef.Value;

            return ReferenceEquals(source1, source2) && fieldId1.Equals(fieldId2);
        }

        // Pointer into an array/slice element: same backing storage and same index. A PinnedBuffer
        // is a per-access VIEW over its pinned storage (e.g. @string.buffer creates one per
        // unsafe.StringData call), so it canonicalizes to the pinned object — two pointers to the
        // same string data compare equal (Go compares addresses, never view instances).
        if (m_arrayIndexRef is not null || other.m_arrayIndexRef is not null)
        {
            if (m_arrayIndexRef is null || other.m_arrayIndexRef is null)
                return false;

            (IArray array1, int index1) = m_arrayIndexRef.Value;
            (IArray array2, int index2) = other.m_arrayIndexRef.Value;

            return ReferenceEquals(CanonicalStorage(array1), CanonicalStorage(array2)) && index1 == index2;
        }

        // Two standard heap pointers, each a distinct allocation: equal only if they wrap the same
        // reference-type object (the ReferenceEquals(this, other) check above already handled the
        // same-box case, so distinct value-type allocations are distinct addresses → not equal).
        return IsReferenceType && ReferenceEquals(m_val, other.m_val);
    }

    // Reduces an array-index referent to the identity of its actual storage: a PinnedBuffer view
    // yields the object it pins (falling back to the view itself when nothing is pinned); any
    // other IArray is its own storage. Used by Equals/GetHashCode above so equal pointers into
    // per-access views still hash and compare as the same address.
    private static object CanonicalStorage(IArray array)
    {
        return array is PinnedBuffer pinned ? pinned.PinnedTarget ?? array : array;
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
            return System.HashCode.Combine(RuntimeHelpers.GetHashCode(CanonicalStorage(m_arrayIndexRef.Value.Item1)), m_arrayIndexRef.Value.Item2);

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
    //     vp.Value = 999;
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

        // Resolve through `Value`, not the raw `m_val` field: for a struct-field reference (`Ꮡx.of(T.Ꮡf)`)
        // or an array-element reference, the real storage lives behind `Value` and `m_val` is an empty
        // default — returning `m_val` would yield a zero-valued copy (so `(~(&x.field)).sub` read 0, e.g.
        // a `c := &b.w; c.a` field-chain read). For a standard pointer `Value` returns `m_val`, so this is
        // identical there. Matches the IPointer<T>.operator ~ above, which already resolves via `Value`.
        return value.Value;
    }

    static T IPointer<T>.operator ~(IPointer<T> value)
    {
        if (value.IsNull)
            throw RuntimeErrorPanic.NilPointerDereference();

        return value.Value;
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

    // EXPLICIT by design: reinterpreting a raw address as a pointer is the runtime-unsafe reinterpret
    // seam — never something to happen silently. Converter-emitted reinterprets always use explicit
    // cast syntax ((ж<T>)(uintptr)(p)). As an implicit conversion it also made every uintptr argument
    // ambiguous between an unsafe.Pointer overload and a ж<T> overload (CS0121 — runtime's free
    // `add(p, x)` vs the `(*notInHeap).add` static companion).
    //
    // The result ALIASES the address (see m_nativeAddr). It formerly boxed a COPY of the pointed-at
    // value, which silently discarded the address: pointer arithmetic then walked the GC heap and
    // handing the pointer back to a native API freed GC memory (STATUS_HEAP_CORRUPTION). Aliasing
    // keeps `uintptr(unsafe.Pointer(p))` an exact round-trip, as Go requires.
    public static unsafe explicit operator ж<T>(uintptr value)
    {
        return new ж<T>((nuint)value.Value);
    }

    public static unsafe implicit operator uintptr(ж<T> value)
    {
        // A native-backed pointer round-trips to the EXACT address it aliases — it is not managed
        // storage, so there is nothing to pin and no copy to take.
        if (value is not null && value.m_nativeAddr != 0)
            return (uintptr)value.m_nativeAddr;

        // A NIL pointer's address is 0, matching Go's `uintptr(unsafe.Pointer(nil)) == 0`. A nil box
        // has no storage to pin, so taking `&value.Value` would dereference it and throw — but the
        // syscall wrappers legitimately pass nil pointers whose numeric address is simply 0
        // (syscall.Write hands writeFile a nil `*Overlapped` for a synchronous write, then passes
        // `uintptr(unsafe.Pointer(overlapped))` to the trampoline). Return 0 instead of throwing.
        if (value is null || value.IsNull)
            return default;

        // A pointer to a Go fixed array (`unsafe.Pointer(&arr)`): the native address must reference the
        // array's DATA (element 0), pinned so a syscall can fill it in and the managed reads afterward
        // observe the result — not the transient address of the `array<T>` struct wrapper. Slices keep
        // header semantics (`&s` is the slice header in Go), so they fall through to the value-slot path.
        if (value.Value is IArray arr && arr is not ISlice)
            return (uintptr)value.pinnedArrayData(arr);

        fixed (void* ptr = &value.Value)
            return (uintptr)ptr;
    }

    // Aliases the address rather than copying the pointed-at value — see the uintptr operator above.
    public static unsafe implicit operator ж<T>(void* value)
    {
        return new ж<T>((nuint)value);
    }

    public static unsafe implicit operator void*(ж<T> value)
    {
        // A native-backed pointer converts to the exact address it aliases (see the uintptr operator).
        if (value is not null && value.m_nativeAddr != 0)
            return (void*)value.m_nativeAddr;

        // A nil pointer converts to the null address (see the uintptr operator above); pinning a
        // nil box's absent storage would throw.
        if (value is null || value.IsNull)
            return null;

        // A pointer to a Go fixed array resolves to the pinned address of the array data — see the
        // uintptr operator above for the full rationale.
        if (value.Value is IArray arr && arr is not ISlice)
            return value.pinnedArrayData(arr);

        fixed (T* ptr = &value.Value)
            return ptr;
    }

    // Returns a stable native pointer to the first element of this box's Go fixed-array data, pinning the
    // backing store for the box's lifetime (freed when the box is collected). Used when a fixed array is
    // handed to a native syscall as a read/write buffer via `unsafe.Pointer(&arr)`: the address must
    // reference the array DATA — not the `array<T>` struct wrapper — and the data must not move while
    // native code fills it in and while the value is read back afterward. Valid only for Go fixed arrays
    // (`array<T>`, an IArray that is not an ISlice); a slice's `&s` addresses its header, not its data.
    private unsafe void* pinnedArrayData(IArray arr)
    {
        // One stable pin per box: the syscall write and the subsequent managed reads must all see the
        // same address. (Not locked — a syscall buffer is a function-local array, never shared; a rare
        // concurrent first-touch would at worst leak one pinned handle to the finalizer.)
        m_pinnedArrayData ??= new PinnedBuffer(arr.Source, arr.Length);
        return m_pinnedArrayData.Pointer;
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
    /// <see cref="ж{T}.Value"/> getter would throw a nil-pointer dereference before the loop guard is
    /// re-checked. This accessor instead yields a <c>ref</c> to a throwaway default slot — never read
    /// while the box is nil (the <c>Ꮡp != nil</c> guard excludes it), so the value is harmless.
    /// </para>
    /// <para>
    /// This is <em>not</em> a substitute for a genuine dereference: reading or writing <c>*p</c> on a
    /// nil pointer (emitted as <c>~Ꮡp</c> / <c>Ꮡp.Value</c>) still panics, preserving Go semantics. Only
    /// the re-alias — which captures a reference without reading it — uses the nil-safe form. As an
    /// extension method it tolerates a <c>null</c> receiver (a nil pointer field is a <c>null</c>
    /// reference, not a nil box).
    /// </para>
    /// </remarks>
    public static ref T DerefOrNil<T>(this ж<T>? box)
    {
        if (box is null || box.IsNilStandardPointer)
            return ref NilSlot<T>.Slot;

        return ref box.Value;
    }

    // Per-T shared default slot returned by ref for a nil pointer. Never read while the pointer is
    // nil (the converted loop guard excludes it), so the shared mutable storage is benign.
    private static class NilSlot<T>
    {
        public static T Slot = default!;
    }
}
