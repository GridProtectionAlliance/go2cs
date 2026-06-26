// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
using go;

// Hand-finished conversion. Pointer<T> below is rewritten to store its value as a managed ж<T>
// (using Volatile/Interlocked) rather than an unsafe.Pointer: unsafe.Pointer is an alias for nuint
// and the CLR cannot hold a managed reference as a number across a GC move. This module marker is
// detected by containsManualConversionMarker (go2cs/directiveOperations.go); when set, go2cs skips
// re-converting this file so the manual edits are preserved on any future stdlib reconversion.
[module: GoManualConversion]

namespace go.sync;

using @unsafe = unsafe_package;
using System.Threading;

partial class atomic_package {

// A Bool is an atomic boolean value.
// The zero value is false.
[GoType] partial struct Bool {
    internal noCopy _;
    internal uint32 v;
}

// Load atomically loads and returns the value stored in x.
[GoRecv("capture")] public static bool Load(this ref Bool x) {
    return LoadUint32(Load_BoolꓸᏑx.of(Bool.Ꮡv)) != 0;
}

// Store atomically stores val into x.
[GoRecv("capture")] public static void Store(this ref Bool x, bool val) {
    StoreUint32(Store_BoolꓸᏑx.of(Bool.Ꮡv), b32(val));
}

// Swap atomically stores new into x and returns the previous value.
[GoRecv("capture")] public static bool /*old*/ Swap(this ref Bool x, bool @new) {
    bool old = default!;

    return SwapUint32(Swap_BoolꓸᏑx.of(Bool.Ꮡv), b32(@new)) != 0;
}

// CompareAndSwap executes the compare-and-swap operation for the boolean value x.
[GoRecv("capture")] public static bool /*swapped*/ CompareAndSwap(this ref Bool x, bool old, bool @new) {
    bool swapped = default!;

    return CompareAndSwapUint32(CompareAndSwap_BoolꓸᏑx.of(Bool.Ꮡv), b32(old), b32(@new));
}

// b32 returns a uint32 0 or 1 representing b.
internal static uint32 b32(bool b) {
    if (b) {
        return 1;
    }
    return 0;
}

// For testing *Pointer[T]'s methods can be inlined.
// Keep in sync with cmd/compile/internal/test/inl_test.go:TestIntendedInlining.
internal static ж<Pointer<nint>> _ᴛ1ʗ = Ꮡ(new Pointer<nint>(nil));

// A Pointer is an atomic pointer of type *T. The zero value is a nil *T.
[GoType] partial struct Pointer<T>
    where T : new()
{
    // Mention *T in a field to disallow conversion between Pointer types.
    // See go.dev/issue/56603 for more details.
    // Use *T, not T, to avoid spurious recursive type definition errors.
    internal array<ж<T>> _ = new(0);
    internal noCopy __;
    // go2cs: Go stores the value as an unsafe.Pointer (a raw machine address). In .NET a managed
    // pointer (ж<T>) cannot be held as a number and survive a GC move, so this slot holds the ж<T>
    // directly and the operations below use Volatile/Interlocked for atomicity. A null slot is a
    // nil *T; nilCanon collapses an explicit nil-ж to null so reference-based CompareAndSwap treats
    // every nil pointer as equal (matching Go, where a nil unsafe.Pointer compares equal to nil).
    internal ж<T> v;
}

// nilCanon canonicalizes a nil pointer to null so the reference comparison in CompareAndSwap
// treats all nil *T values as equal.
private static ж<T> nilCanon<T>(ж<T> p)
    where T : new()
{
    return p is null || p.IsNull ? default! : p;
}

// Load atomically loads and returns the value stored in x.
public static ж<T> Load<T>(this ж<Pointer<T>> Ꮡx)
    where T : new()
{
    ref var x = ref Ꮡx.val;

    return Volatile.Read(ref x.v);
}

// Store atomically stores val into x.
public static void Store<T>(this ж<Pointer<T>> Ꮡx, ж<T> Ꮡval)
    where T : new()
{
    ref var x = ref Ꮡx.val;

    Volatile.Write(ref x.v, nilCanon(Ꮡval));
}

// Swap atomically stores new into x and returns the previous value.
public static ж<T> /*old*/ Swap<T>(this ж<Pointer<T>> Ꮡx, ж<T> Ꮡnew)
    where T : new()
{
    ref var x = ref Ꮡx.val;

    return Interlocked.Exchange(ref x.v, nilCanon(Ꮡnew));
}

// CompareAndSwap executes the compare-and-swap operation for x.
public static bool /*swapped*/ CompareAndSwap<T>(this ж<Pointer<T>> Ꮡx, ж<T> Ꮡold, ж<T> Ꮡnew)
    where T : new()
{
    ref var x = ref Ꮡx.val;

    ж<T> old = nilCanon(Ꮡold);
    return ReferenceEquals(Interlocked.CompareExchange(ref x.v, nilCanon(Ꮡnew), old), old);
}

// An Int32 is an atomic int32. The zero value is zero.
[GoType] partial struct Int32 {
    internal noCopy _;
    internal int32 v;
}

// Load atomically loads and returns the value stored in x.
[GoRecv("capture")] public static int32 Load(this ref Int32 x) {
    return LoadInt32(Load_Int32ꓸᏑx.of(Int32.Ꮡv));
}

// Store atomically stores val into x.
[GoRecv("capture")] public static void Store(this ref Int32 x, int32 val) {
    StoreInt32(Store_Int32ꓸᏑx.of(Int32.Ꮡv), val);
}

// Swap atomically stores new into x and returns the previous value.
[GoRecv("capture")] public static int32 /*old*/ Swap(this ref Int32 x, int32 @new) {
    int32 old = default!;

    return SwapInt32(Swap_Int32ꓸᏑx.of(Int32.Ꮡv), @new);
}

// CompareAndSwap executes the compare-and-swap operation for x.
[GoRecv("capture")] public static bool /*swapped*/ CompareAndSwap(this ref Int32 x, int32 old, int32 @new) {
    bool swapped = default!;

    return CompareAndSwapInt32(CompareAndSwap_Int32ꓸᏑx.of(Int32.Ꮡv), old, @new);
}

// Add atomically adds delta to x and returns the new value.
[GoRecv("capture")] public static int32 /*new*/ Add(this ref Int32 x, int32 delta) {
    int32 @new = default!;

    return AddInt32(Add_Int32ꓸᏑx.of(Int32.Ꮡv), delta);
}

// And atomically performs a bitwise AND operation on x using the bitmask
// provided as mask and returns the old value.
[GoRecv("capture")] public static int32 /*old*/ And(this ref Int32 x, int32 mask) {
    int32 old = default!;

    return AndInt32(And_Int32ꓸᏑx.of(Int32.Ꮡv), mask);
}

// Or atomically performs a bitwise OR operation on x using the bitmask
// provided as mask and returns the old value.
[GoRecv("capture")] public static int32 /*old*/ Or(this ref Int32 x, int32 mask) {
    int32 old = default!;

    return OrInt32(Or_Int32ꓸᏑx.of(Int32.Ꮡv), mask);
}

// An Int64 is an atomic int64. The zero value is zero.
[GoType] partial struct Int64 {
    internal noCopy _;
    internal align64 __;
    internal int64 v;
}

// Load atomically loads and returns the value stored in x.
[GoRecv("capture")] public static int64 Load(this ref Int64 x) {
    return LoadInt64(Load_Int64ꓸᏑx.of(Int64.Ꮡv));
}

// Store atomically stores val into x.
[GoRecv("capture")] public static void Store(this ref Int64 x, int64 val) {
    StoreInt64(Store_Int64ꓸᏑx.of(Int64.Ꮡv), val);
}

// Swap atomically stores new into x and returns the previous value.
[GoRecv("capture")] public static int64 /*old*/ Swap(this ref Int64 x, int64 @new) {
    int64 old = default!;

    return SwapInt64(Swap_Int64ꓸᏑx.of(Int64.Ꮡv), @new);
}

// CompareAndSwap executes the compare-and-swap operation for x.
[GoRecv("capture")] public static bool /*swapped*/ CompareAndSwap(this ref Int64 x, int64 old, int64 @new) {
    bool swapped = default!;

    return CompareAndSwapInt64(CompareAndSwap_Int64ꓸᏑx.of(Int64.Ꮡv), old, @new);
}

// Add atomically adds delta to x and returns the new value.
[GoRecv("capture")] public static int64 /*new*/ Add(this ref Int64 x, int64 delta) {
    int64 @new = default!;

    return AddInt64(Add_Int64ꓸᏑx.of(Int64.Ꮡv), delta);
}

// And atomically performs a bitwise AND operation on x using the bitmask
// provided as mask and returns the old value.
[GoRecv("capture")] public static int64 /*old*/ And(this ref Int64 x, int64 mask) {
    int64 old = default!;

    return AndInt64(And_Int64ꓸᏑx.of(Int64.Ꮡv), mask);
}

// Or atomically performs a bitwise OR operation on x using the bitmask
// provided as mask and returns the old value.
[GoRecv("capture")] public static int64 /*old*/ Or(this ref Int64 x, int64 mask) {
    int64 old = default!;

    return OrInt64(Or_Int64ꓸᏑx.of(Int64.Ꮡv), mask);
}

// A Uint32 is an atomic uint32. The zero value is zero.
[GoType] partial struct Uint32 {
    internal noCopy _;
    internal uint32 v;
}

// Load atomically loads and returns the value stored in x.
[GoRecv("capture")] public static uint32 Load(this ref Uint32 x) {
    return LoadUint32(Load_Uint32ꓸᏑx.of(Uint32.Ꮡv));
}

// Store atomically stores val into x.
[GoRecv("capture")] public static void Store(this ref Uint32 x, uint32 val) {
    StoreUint32(Store_Uint32ꓸᏑx.of(Uint32.Ꮡv), val);
}

// Swap atomically stores new into x and returns the previous value.
[GoRecv("capture")] public static uint32 /*old*/ Swap(this ref Uint32 x, uint32 @new) {
    uint32 old = default!;

    return SwapUint32(Swap_Uint32ꓸᏑx.of(Uint32.Ꮡv), @new);
}

// CompareAndSwap executes the compare-and-swap operation for x.
[GoRecv("capture")] public static bool /*swapped*/ CompareAndSwap(this ref Uint32 x, uint32 old, uint32 @new) {
    bool swapped = default!;

    return CompareAndSwapUint32(CompareAndSwap_Uint32ꓸᏑx.of(Uint32.Ꮡv), old, @new);
}

// Add atomically adds delta to x and returns the new value.
[GoRecv("capture")] public static uint32 /*new*/ Add(this ref Uint32 x, uint32 delta) {
    uint32 @new = default!;

    return AddUint32(Add_Uint32ꓸᏑx.of(Uint32.Ꮡv), delta);
}

// And atomically performs a bitwise AND operation on x using the bitmask
// provided as mask and returns the old value.
[GoRecv("capture")] public static uint32 /*old*/ And(this ref Uint32 x, uint32 mask) {
    uint32 old = default!;

    return AndUint32(And_Uint32ꓸᏑx.of(Uint32.Ꮡv), mask);
}

// Or atomically performs a bitwise OR operation on x using the bitmask
// provided as mask and returns the old value.
[GoRecv("capture")] public static uint32 /*old*/ Or(this ref Uint32 x, uint32 mask) {
    uint32 old = default!;

    return OrUint32(Or_Uint32ꓸᏑx.of(Uint32.Ꮡv), mask);
}

// A Uint64 is an atomic uint64. The zero value is zero.
[GoType] partial struct Uint64 {
    internal noCopy _;
    internal align64 __;
    internal uint64 v;
}

// Load atomically loads and returns the value stored in x.
[GoRecv("capture")] public static uint64 Load(this ref Uint64 x) {
    return LoadUint64(Load_Uint64ꓸᏑx.of(Uint64.Ꮡv));
}

// Store atomically stores val into x.
[GoRecv("capture")] public static void Store(this ref Uint64 x, uint64 val) {
    StoreUint64(Store_Uint64ꓸᏑx.of(Uint64.Ꮡv), val);
}

// Swap atomically stores new into x and returns the previous value.
[GoRecv("capture")] public static uint64 /*old*/ Swap(this ref Uint64 x, uint64 @new) {
    uint64 old = default!;

    return SwapUint64(Swap_Uint64ꓸᏑx.of(Uint64.Ꮡv), @new);
}

// CompareAndSwap executes the compare-and-swap operation for x.
[GoRecv("capture")] public static bool /*swapped*/ CompareAndSwap(this ref Uint64 x, uint64 old, uint64 @new) {
    bool swapped = default!;

    return CompareAndSwapUint64(CompareAndSwap_Uint64ꓸᏑx.of(Uint64.Ꮡv), old, @new);
}

// Add atomically adds delta to x and returns the new value.
[GoRecv("capture")] public static uint64 /*new*/ Add(this ref Uint64 x, uint64 delta) {
    uint64 @new = default!;

    return AddUint64(Add_Uint64ꓸᏑx.of(Uint64.Ꮡv), delta);
}

// And atomically performs a bitwise AND operation on x using the bitmask
// provided as mask and returns the old value.
[GoRecv("capture")] public static uint64 /*old*/ And(this ref Uint64 x, uint64 mask) {
    uint64 old = default!;

    return AndUint64(And_Uint64ꓸᏑx.of(Uint64.Ꮡv), mask);
}

// Or atomically performs a bitwise OR operation on x using the bitmask
// provided as mask and returns the old value.
[GoRecv("capture")] public static uint64 /*old*/ Or(this ref Uint64 x, uint64 mask) {
    uint64 old = default!;

    return OrUint64(Or_Uint64ꓸᏑx.of(Uint64.Ꮡv), mask);
}

// A Uintptr is an atomic uintptr. The zero value is zero.
[GoType] partial struct Uintptr {
    internal noCopy _;
    internal uintptr v;
}

// Load atomically loads and returns the value stored in x.
[GoRecv("capture")] public static uintptr Load(this ref Uintptr x) {
    return LoadUintptr(Load_UintptrꓸᏑx.of(Uintptr.Ꮡv));
}

// Store atomically stores val into x.
[GoRecv("capture")] public static void Store(this ref Uintptr x, uintptr val) {
    StoreUintptr(Store_UintptrꓸᏑx.of(Uintptr.Ꮡv), val);
}

// Swap atomically stores new into x and returns the previous value.
[GoRecv("capture")] public static uintptr /*old*/ Swap(this ref Uintptr x, uintptr @new) {
    uintptr old = default!;

    return SwapUintptr(Swap_UintptrꓸᏑx.of(Uintptr.Ꮡv), @new);
}

// CompareAndSwap executes the compare-and-swap operation for x.
[GoRecv("capture")] public static bool /*swapped*/ CompareAndSwap(this ref Uintptr x, uintptr old, uintptr @new) {
    bool swapped = default!;

    return CompareAndSwapUintptr(CompareAndSwap_UintptrꓸᏑx.of(Uintptr.Ꮡv), old, @new);
}

// Add atomically adds delta to x and returns the new value.
[GoRecv("capture")] public static uintptr /*new*/ Add(this ref Uintptr x, uintptr delta) {
    uintptr @new = default!;

    return AddUintptr(Add_UintptrꓸᏑx.of(Uintptr.Ꮡv), delta);
}

// And atomically performs a bitwise AND operation on x using the bitmask
// provided as mask and returns the old value.
[GoRecv("capture")] public static uintptr /*old*/ And(this ref Uintptr x, uintptr mask) {
    uintptr old = default!;

    return AndUintptr(And_UintptrꓸᏑx.of(Uintptr.Ꮡv), mask);
}

// Or atomically performs a bitwise OR operation on x using the bitmask
// provided as mask and returns the updated value after the OR operation.
[GoRecv("capture")] public static uintptr /*old*/ Or(this ref Uintptr x, uintptr mask) {
    uintptr old = default!;

    return OrUintptr(Or_UintptrꓸᏑx.of(Uintptr.Ꮡv), mask);
}

// noCopy may be added to structs which must not be copied
// after the first use.
//
// See https://golang.org/issues/8005#issuecomment-190753527
// for details.
//
// Note that it must not be embedded, due to the Lock and Unlock methods.
[GoType] partial struct noCopy {
}

// Lock is a no-op used by -copylocks checker from `go vet`.
[GoRecv] internal static void Lock(this ref noCopy _) {
}

[GoRecv] internal static void Unlock(this ref noCopy _) {
}

// align64 may be added to structs that must be 64-bit aligned.
// This struct is recognized by a special case in the compiler
// and will not work if copied to any other package.
[GoType] partial struct align64 {
}

} // end atomic_package
