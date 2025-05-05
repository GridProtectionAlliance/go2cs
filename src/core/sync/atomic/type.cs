// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.sync;

using @unsafe = unsafe_package;

partial class atomic_package {

// A Bool is an atomic boolean value.
// The zero value is false.
[GoType] partial struct Bool {
    internal noCopy _;
    internal uint32 v;
}

// Load atomically loads and returns the value stored in x.
[GoRecv] public static bool Load(this ref Bool x) {
    return LoadUint32(Ꮡ(x.v)) != 0;
}

// Store atomically stores val into x.
[GoRecv] public static void Store(this ref Bool x, bool val) {
    StoreUint32(Ꮡ(x.v), b32(val));
}

// Swap atomically stores new into x and returns the previous value.
[GoRecv] public static bool /*old*/ Swap(this ref Bool x, bool @new) {
    bool old = default!;

    return SwapUint32(Ꮡ(x.v), b32(@new)) != 0;
}

// CompareAndSwap executes the compare-and-swap operation for the boolean value x.
[GoRecv] public static bool /*swapped*/ CompareAndSwap(this ref Bool x, bool old, bool @new) {
    bool swapped = default!;

    return CompareAndSwapUint32(Ꮡ(x.v), b32(old), b32(@new));
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
internal static ж<Pointer<nint>> _ = Ꮡ(new Pointer<nint>(nil));

// A Pointer is an atomic pointer of type *T. The zero value is a nil *T.
[GoType] partial struct Pointer<T>
    where T : new()
{
    // Mention *T in a field to disallow conversion between Pointer types.
    // See go.dev/issue/56603 for more details.
    // Use *T, not T, to avoid spurious recursive type definition errors.
    internal array<ж<T>> _ = new(0);
    internal noCopy __;
    internal @unsafe.Pointer v;
}

// Load atomically loads and returns the value stored in x.
[GoRecv] public static ж<T> Load<T>(this ref Pointer<T> x)
    where T : new()
{
    return (ж<T>)(uintptr)(LoadPointer(Ꮡ(x.v)));
}

// Store atomically stores val into x.
[GoRecv] public static void Store<T>(this ref Pointer<T> x, ж<T> Ꮡval)
    where T : new()
{
    ref var val = ref Ꮡval.val;

    StorePointer(Ꮡ(x.v), new @unsafe.Pointer(Ꮡval));
}

// Swap atomically stores new into x and returns the previous value.
[GoRecv] public static ж<T> /*old*/ Swap<T>(this ref Pointer<T> x, ж<T> Ꮡnew)
    where T : new()
{
    ж<T> old = default!;

    ref var @new = ref Ꮡnew.val;
    return (ж<T>)(uintptr)(SwapPointer(Ꮡ(x.v), new @unsafe.Pointer(Ꮡnew)));
}

// CompareAndSwap executes the compare-and-swap operation for x.
[GoRecv] public static bool /*swapped*/ CompareAndSwap<T>(this ref Pointer<T> x, ж<T> Ꮡold, ж<T> Ꮡnew)
    where T : new()
{
    bool swapped = default!;

    ref var old = ref Ꮡold.val;
    ref var @new = ref Ꮡnew.val;
    return CompareAndSwapPointer(Ꮡ(x.v), new @unsafe.Pointer(Ꮡold), new @unsafe.Pointer(Ꮡnew));
}

// An Int32 is an atomic int32. The zero value is zero.
[GoType] partial struct Int32 {
    internal noCopy _;
    internal int32 v;
}

// Load atomically loads and returns the value stored in x.
[GoRecv] public static int32 Load(this ref Int32 x) {
    return LoadInt32(Ꮡ(x.v));
}

// Store atomically stores val into x.
[GoRecv] public static void Store(this ref Int32 x, int32 val) {
    StoreInt32(Ꮡ(x.v), val);
}

// Swap atomically stores new into x and returns the previous value.
[GoRecv] public static int32 /*old*/ Swap(this ref Int32 x, int32 @new) {
    int32 old = default!;

    return SwapInt32(Ꮡ(x.v), @new);
}

// CompareAndSwap executes the compare-and-swap operation for x.
[GoRecv] public static bool /*swapped*/ CompareAndSwap(this ref Int32 x, int32 old, int32 @new) {
    bool swapped = default!;

    return CompareAndSwapInt32(Ꮡ(x.v), old, @new);
}

// Add atomically adds delta to x and returns the new value.
[GoRecv] public static int32 /*new*/ Add(this ref Int32 x, int32 delta) {
    int32 @new = default!;

    return AddInt32(Ꮡ(x.v), delta);
}

// And atomically performs a bitwise AND operation on x using the bitmask
// provided as mask and returns the old value.
[GoRecv] public static int32 /*old*/ And(this ref Int32 x, int32 mask) {
    int32 old = default!;

    return AndInt32(Ꮡ(x.v), mask);
}

// Or atomically performs a bitwise OR operation on x using the bitmask
// provided as mask and returns the old value.
[GoRecv] public static int32 /*old*/ Or(this ref Int32 x, int32 mask) {
    int32 old = default!;

    return OrInt32(Ꮡ(x.v), mask);
}

// An Int64 is an atomic int64. The zero value is zero.
[GoType] partial struct Int64 {
    internal noCopy _;
    internal align64 __;
    internal int64 v;
}

// Load atomically loads and returns the value stored in x.
[GoRecv] public static int64 Load(this ref Int64 x) {
    return LoadInt64(Ꮡ(x.v));
}

// Store atomically stores val into x.
[GoRecv] public static void Store(this ref Int64 x, int64 val) {
    StoreInt64(Ꮡ(x.v), val);
}

// Swap atomically stores new into x and returns the previous value.
[GoRecv] public static int64 /*old*/ Swap(this ref Int64 x, int64 @new) {
    int64 old = default!;

    return SwapInt64(Ꮡ(x.v), @new);
}

// CompareAndSwap executes the compare-and-swap operation for x.
[GoRecv] public static bool /*swapped*/ CompareAndSwap(this ref Int64 x, int64 old, int64 @new) {
    bool swapped = default!;

    return CompareAndSwapInt64(Ꮡ(x.v), old, @new);
}

// Add atomically adds delta to x and returns the new value.
[GoRecv] public static int64 /*new*/ Add(this ref Int64 x, int64 delta) {
    int64 @new = default!;

    return AddInt64(Ꮡ(x.v), delta);
}

// And atomically performs a bitwise AND operation on x using the bitmask
// provided as mask and returns the old value.
[GoRecv] public static int64 /*old*/ And(this ref Int64 x, int64 mask) {
    int64 old = default!;

    return AndInt64(Ꮡ(x.v), mask);
}

// Or atomically performs a bitwise OR operation on x using the bitmask
// provided as mask and returns the old value.
[GoRecv] public static int64 /*old*/ Or(this ref Int64 x, int64 mask) {
    int64 old = default!;

    return OrInt64(Ꮡ(x.v), mask);
}

// A Uint32 is an atomic uint32. The zero value is zero.
[GoType] partial struct Uint32 {
    internal noCopy _;
    internal uint32 v;
}

// Load atomically loads and returns the value stored in x.
[GoRecv] public static uint32 Load(this ref Uint32 x) {
    return LoadUint32(Ꮡ(x.v));
}

// Store atomically stores val into x.
[GoRecv] public static void Store(this ref Uint32 x, uint32 val) {
    StoreUint32(Ꮡ(x.v), val);
}

// Swap atomically stores new into x and returns the previous value.
[GoRecv] public static uint32 /*old*/ Swap(this ref Uint32 x, uint32 @new) {
    uint32 old = default!;

    return SwapUint32(Ꮡ(x.v), @new);
}

// CompareAndSwap executes the compare-and-swap operation for x.
[GoRecv] public static bool /*swapped*/ CompareAndSwap(this ref Uint32 x, uint32 old, uint32 @new) {
    bool swapped = default!;

    return CompareAndSwapUint32(Ꮡ(x.v), old, @new);
}

// Add atomically adds delta to x and returns the new value.
[GoRecv] public static uint32 /*new*/ Add(this ref Uint32 x, uint32 delta) {
    uint32 @new = default!;

    return AddUint32(Ꮡ(x.v), delta);
}

// And atomically performs a bitwise AND operation on x using the bitmask
// provided as mask and returns the old value.
[GoRecv] public static uint32 /*old*/ And(this ref Uint32 x, uint32 mask) {
    uint32 old = default!;

    return AndUint32(Ꮡ(x.v), mask);
}

// Or atomically performs a bitwise OR operation on x using the bitmask
// provided as mask and returns the old value.
[GoRecv] public static uint32 /*old*/ Or(this ref Uint32 x, uint32 mask) {
    uint32 old = default!;

    return OrUint32(Ꮡ(x.v), mask);
}

// A Uint64 is an atomic uint64. The zero value is zero.
[GoType] partial struct Uint64 {
    internal noCopy _;
    internal align64 __;
    internal uint64 v;
}

// Load atomically loads and returns the value stored in x.
[GoRecv] public static uint64 Load(this ref Uint64 x) {
    return LoadUint64(Ꮡ(x.v));
}

// Store atomically stores val into x.
[GoRecv] public static void Store(this ref Uint64 x, uint64 val) {
    StoreUint64(Ꮡ(x.v), val);
}

// Swap atomically stores new into x and returns the previous value.
[GoRecv] public static uint64 /*old*/ Swap(this ref Uint64 x, uint64 @new) {
    uint64 old = default!;

    return SwapUint64(Ꮡ(x.v), @new);
}

// CompareAndSwap executes the compare-and-swap operation for x.
[GoRecv] public static bool /*swapped*/ CompareAndSwap(this ref Uint64 x, uint64 old, uint64 @new) {
    bool swapped = default!;

    return CompareAndSwapUint64(Ꮡ(x.v), old, @new);
}

// Add atomically adds delta to x and returns the new value.
[GoRecv] public static uint64 /*new*/ Add(this ref Uint64 x, uint64 delta) {
    uint64 @new = default!;

    return AddUint64(Ꮡ(x.v), delta);
}

// And atomically performs a bitwise AND operation on x using the bitmask
// provided as mask and returns the old value.
[GoRecv] public static uint64 /*old*/ And(this ref Uint64 x, uint64 mask) {
    uint64 old = default!;

    return AndUint64(Ꮡ(x.v), mask);
}

// Or atomically performs a bitwise OR operation on x using the bitmask
// provided as mask and returns the old value.
[GoRecv] public static uint64 /*old*/ Or(this ref Uint64 x, uint64 mask) {
    uint64 old = default!;

    return OrUint64(Ꮡ(x.v), mask);
}

// A Uintptr is an atomic uintptr. The zero value is zero.
[GoType] partial struct Uintptr {
    internal noCopy _;
    internal uintptr v;
}

// Load atomically loads and returns the value stored in x.
[GoRecv] public static uintptr Load(this ref Uintptr x) {
    return LoadUintptr(Ꮡ(x.v));
}

// Store atomically stores val into x.
[GoRecv] public static void Store(this ref Uintptr x, uintptr val) {
    StoreUintptr(Ꮡ(x.v), val);
}

// Swap atomically stores new into x and returns the previous value.
[GoRecv] public static uintptr /*old*/ Swap(this ref Uintptr x, uintptr @new) {
    uintptr old = default!;

    return SwapUintptr(Ꮡ(x.v), @new);
}

// CompareAndSwap executes the compare-and-swap operation for x.
[GoRecv] public static bool /*swapped*/ CompareAndSwap(this ref Uintptr x, uintptr old, uintptr @new) {
    bool swapped = default!;

    return CompareAndSwapUintptr(Ꮡ(x.v), old, @new);
}

// Add atomically adds delta to x and returns the new value.
[GoRecv] public static uintptr /*new*/ Add(this ref Uintptr x, uintptr delta) {
    uintptr @new = default!;

    return AddUintptr(Ꮡ(x.v), delta);
}

// And atomically performs a bitwise AND operation on x using the bitmask
// provided as mask and returns the old value.
[GoRecv] public static uintptr /*old*/ And(this ref Uintptr x, uintptr mask) {
    uintptr old = default!;

    return AndUintptr(Ꮡ(x.v), mask);
}

// Or atomically performs a bitwise OR operation on x using the bitmask
// provided as mask and returns the updated value after the OR operation.
[GoRecv] public static uintptr /*old*/ Or(this ref Uintptr x, uintptr mask) {
    uintptr old = default!;

    return OrUintptr(Ꮡ(x.v), mask);
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
