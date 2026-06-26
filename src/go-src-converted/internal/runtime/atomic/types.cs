// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime;

using @unsafe = unsafe_package;

partial class atomic_package {

// Int32 is an atomically accessed int32 value.
//
// An Int32 must not be copied.
[GoType] partial struct Int32 {
    internal noCopy noCopy;
    internal int32 value;
}

// Load accesses and returns the value atomically.
//
//go:nosplit
[GoRecv] public static int32 Load(this ref Int32 i) {
    return Loadint32(Ꮡ(i.value));
}

// Store updates the value atomically.
//
//go:nosplit
[GoRecv] public static void Store(this ref Int32 i, int32 value) {
    Storeint32(Ꮡ(i.value), value);
}

// CompareAndSwap atomically compares i's value with old,
// and if they're equal, swaps i's value with new.
// It reports whether the swap ran.
//
//go:nosplit
[GoRecv] public static bool CompareAndSwap(this ref Int32 i, int32 old, int32 @new) {
    return Casint32(Ꮡ(i.value), old, @new);
}

// Swap replaces i's value with new, returning
// i's value before the replacement.
//
//go:nosplit
[GoRecv] public static int32 Swap(this ref Int32 i, int32 @new) {
    return Xchgint32(Ꮡ(i.value), @new);
}

// Add adds delta to i atomically, returning
// the new updated value.
//
// This operation wraps around in the usual
// two's-complement way.
//
//go:nosplit
[GoRecv] public static int32 Add(this ref Int32 i, int32 delta) {
    return Xaddint32(Ꮡ(i.value), delta);
}

// Int64 is an atomically accessed int64 value.
//
// 8-byte aligned on all platforms, unlike a regular int64.
//
// An Int64 must not be copied.
[GoType] partial struct Int64 {
    internal noCopy noCopy;
    internal align64 _;
    internal int64 value;
}

// Load accesses and returns the value atomically.
//
//go:nosplit
[GoRecv] public static int64 Load(this ref Int64 i) {
    return Loadint64(Ꮡ(i.value));
}

// Store updates the value atomically.
//
//go:nosplit
[GoRecv] public static void Store(this ref Int64 i, int64 value) {
    Storeint64(Ꮡ(i.value), value);
}

// CompareAndSwap atomically compares i's value with old,
// and if they're equal, swaps i's value with new.
// It reports whether the swap ran.
//
//go:nosplit
[GoRecv] public static bool CompareAndSwap(this ref Int64 i, int64 old, int64 @new) {
    return Casint64(Ꮡ(i.value), old, @new);
}

// Swap replaces i's value with new, returning
// i's value before the replacement.
//
//go:nosplit
[GoRecv] public static int64 Swap(this ref Int64 i, int64 @new) {
    return Xchgint64(Ꮡ(i.value), @new);
}

// Add adds delta to i atomically, returning
// the new updated value.
//
// This operation wraps around in the usual
// two's-complement way.
//
//go:nosplit
[GoRecv] public static int64 Add(this ref Int64 i, int64 delta) {
    return Xaddint64(Ꮡ(i.value), delta);
}

// Uint8 is an atomically accessed uint8 value.
//
// A Uint8 must not be copied.
[GoType] partial struct Uint8 {
    internal noCopy noCopy;
    internal uint8 value;
}

// Load accesses and returns the value atomically.
//
//go:nosplit
[GoRecv] public static uint8 Load(this ref Uint8 u) {
    return Load8(Ꮡ(u.value));
}

// Store updates the value atomically.
//
//go:nosplit
[GoRecv] public static void Store(this ref Uint8 u, uint8 value) {
    Store8(Ꮡ(u.value), value);
}

// And takes value and performs a bit-wise
// "and" operation with the value of u, storing
// the result into u.
//
// The full process is performed atomically.
//
//go:nosplit
[GoRecv] public static void And(this ref Uint8 u, uint8 value) {
    And8(Ꮡ(u.value), value);
}

// Or takes value and performs a bit-wise
// "or" operation with the value of u, storing
// the result into u.
//
// The full process is performed atomically.
//
//go:nosplit
[GoRecv] public static void Or(this ref Uint8 u, uint8 value) {
    Or8(Ꮡ(u.value), value);
}

// Bool is an atomically accessed bool value.
//
// A Bool must not be copied.
[GoType] partial struct Bool {
    // Inherits noCopy from Uint8.
    internal Uint8 u;
}

// Load accesses and returns the value atomically.
//
//go:nosplit
[GoRecv] public static bool Load(this ref Bool b) {
    return b.u.Load() != 0;
}

// Store updates the value atomically.
//
//go:nosplit
[GoRecv] public static void Store(this ref Bool b, bool value) {
    var s = ((uint8)0);
    if (value) {
        s = 1;
    }
    b.u.Store(s);
}

// Uint32 is an atomically accessed uint32 value.
//
// A Uint32 must not be copied.
[GoType] partial struct Uint32 {
    internal noCopy noCopy;
    internal uint32 value;
}

// Load accesses and returns the value atomically.
//
//go:nosplit
[GoRecv] public static uint32 Load(this ref Uint32 u) {
    return Load(Ꮡ(u.value));
}

// LoadAcquire is a partially unsynchronized version
// of Load that relaxes ordering constraints. Other threads
// may observe operations that precede this operation to
// occur after it, but no operation that occurs after it
// on this thread can be observed to occur before it.
//
// WARNING: Use sparingly and with great care.
//
//go:nosplit
[GoRecv] public static uint32 LoadAcquire(this ref Uint32 u) {
    return LoadAcq(Ꮡ(u.value));
}

// Store updates the value atomically.
//
//go:nosplit
[GoRecv] public static void Store(this ref Uint32 u, uint32 value) {
    Store(Ꮡ(u.value), value);
}

// StoreRelease is a partially unsynchronized version
// of Store that relaxes ordering constraints. Other threads
// may observe operations that occur after this operation to
// precede it, but no operation that precedes it
// on this thread can be observed to occur after it.
//
// WARNING: Use sparingly and with great care.
//
//go:nosplit
[GoRecv] public static void StoreRelease(this ref Uint32 u, uint32 value) {
    StoreRel(Ꮡ(u.value), value);
}

// CompareAndSwap atomically compares u's value with old,
// and if they're equal, swaps u's value with new.
// It reports whether the swap ran.
//
//go:nosplit
[GoRecv] public static bool CompareAndSwap(this ref Uint32 u, uint32 old, uint32 @new) {
    return Cas(Ꮡ(u.value), old, @new);
}

// CompareAndSwapRelease is a partially unsynchronized version
// of Cas that relaxes ordering constraints. Other threads
// may observe operations that occur after this operation to
// precede it, but no operation that precedes it
// on this thread can be observed to occur after it.
// It reports whether the swap ran.
//
// WARNING: Use sparingly and with great care.
//
//go:nosplit
[GoRecv] public static bool CompareAndSwapRelease(this ref Uint32 u, uint32 old, uint32 @new) {
    return CasRel(Ꮡ(u.value), old, @new);
}

// Swap replaces u's value with new, returning
// u's value before the replacement.
//
//go:nosplit
[GoRecv] public static uint32 Swap(this ref Uint32 u, uint32 value) {
    return Xchg(Ꮡ(u.value), value);
}

// And takes value and performs a bit-wise
// "and" operation with the value of u, storing
// the result into u.
//
// The full process is performed atomically.
//
//go:nosplit
[GoRecv] public static void And(this ref Uint32 u, uint32 value) {
    And(Ꮡ(u.value), value);
}

// Or takes value and performs a bit-wise
// "or" operation with the value of u, storing
// the result into u.
//
// The full process is performed atomically.
//
//go:nosplit
[GoRecv] public static void Or(this ref Uint32 u, uint32 value) {
    Or(Ꮡ(u.value), value);
}

// Add adds delta to u atomically, returning
// the new updated value.
//
// This operation wraps around in the usual
// two's-complement way.
//
//go:nosplit
[GoRecv] public static uint32 Add(this ref Uint32 u, int32 delta) {
    return Xadd(Ꮡ(u.value), delta);
}

// Uint64 is an atomically accessed uint64 value.
//
// 8-byte aligned on all platforms, unlike a regular uint64.
//
// A Uint64 must not be copied.
[GoType] partial struct Uint64 {
    internal noCopy noCopy;
    internal align64 _;
    internal uint64 value;
}

// Load accesses and returns the value atomically.
//
//go:nosplit
[GoRecv] public static uint64 Load(this ref Uint64 u) {
    return Load64(Ꮡ(u.value));
}

// Store updates the value atomically.
//
//go:nosplit
[GoRecv] public static void Store(this ref Uint64 u, uint64 value) {
    Store64(Ꮡ(u.value), value);
}

// CompareAndSwap atomically compares u's value with old,
// and if they're equal, swaps u's value with new.
// It reports whether the swap ran.
//
//go:nosplit
[GoRecv] public static bool CompareAndSwap(this ref Uint64 u, uint64 old, uint64 @new) {
    return Cas64(Ꮡ(u.value), old, @new);
}

// Swap replaces u's value with new, returning
// u's value before the replacement.
//
//go:nosplit
[GoRecv] public static uint64 Swap(this ref Uint64 u, uint64 value) {
    return Xchg64(Ꮡ(u.value), value);
}

// Add adds delta to u atomically, returning
// the new updated value.
//
// This operation wraps around in the usual
// two's-complement way.
//
//go:nosplit
[GoRecv] public static uint64 Add(this ref Uint64 u, int64 delta) {
    return Xadd64(Ꮡ(u.value), delta);
}

// Uintptr is an atomically accessed uintptr value.
//
// A Uintptr must not be copied.
[GoType] partial struct Uintptr {
    internal noCopy noCopy;
    internal uintptr value;
}

// Load accesses and returns the value atomically.
//
//go:nosplit
[GoRecv] public static uintptr Load(this ref Uintptr u) {
    return Loaduintptr(Ꮡ(u.value));
}

// LoadAcquire is a partially unsynchronized version
// of Load that relaxes ordering constraints. Other threads
// may observe operations that precede this operation to
// occur after it, but no operation that occurs after it
// on this thread can be observed to occur before it.
//
// WARNING: Use sparingly and with great care.
//
//go:nosplit
[GoRecv] public static uintptr LoadAcquire(this ref Uintptr u) {
    return LoadAcquintptr(Ꮡ(u.value));
}

// Store updates the value atomically.
//
//go:nosplit
[GoRecv] public static void Store(this ref Uintptr u, uintptr value) {
    Storeuintptr(Ꮡ(u.value), value);
}

// StoreRelease is a partially unsynchronized version
// of Store that relaxes ordering constraints. Other threads
// may observe operations that occur after this operation to
// precede it, but no operation that precedes it
// on this thread can be observed to occur after it.
//
// WARNING: Use sparingly and with great care.
//
//go:nosplit
[GoRecv] public static void StoreRelease(this ref Uintptr u, uintptr value) {
    StoreReluintptr(Ꮡ(u.value), value);
}

// CompareAndSwap atomically compares u's value with old,
// and if they're equal, swaps u's value with new.
// It reports whether the swap ran.
//
//go:nosplit
[GoRecv] public static bool CompareAndSwap(this ref Uintptr u, uintptr old, uintptr @new) {
    return Casuintptr(Ꮡ(u.value), old, @new);
}

// Swap replaces u's value with new, returning
// u's value before the replacement.
//
//go:nosplit
[GoRecv] public static uintptr Swap(this ref Uintptr u, uintptr value) {
    return Xchguintptr(Ꮡ(u.value), value);
}

// Add adds delta to u atomically, returning
// the new updated value.
//
// This operation wraps around in the usual
// two's-complement way.
//
//go:nosplit
[GoRecv] public static uintptr Add(this ref Uintptr u, uintptr delta) {
    return Xadduintptr(Ꮡ(u.value), delta);
}

// Float64 is an atomically accessed float64 value.
//
// 8-byte aligned on all platforms, unlike a regular float64.
//
// A Float64 must not be copied.
[GoType] partial struct Float64 {
    // Inherits noCopy and align64 from Uint64.
    internal Uint64 u;
}

// Load accesses and returns the value atomically.
//
//go:nosplit
[GoRecv] public static float64 Load(this ref Float64 f) {
    ref var r = ref heap<uint64>(out var Ꮡr);
    r = f.u.Load();
    return ~(ж<float64>)(uintptr)(new @unsafe.Pointer(Ꮡr));
}

// Store updates the value atomically.
//
//go:nosplit
[GoRecv] public static void Store(this ref Float64 f, float64 value) {
    f.u.Store(~(ж<uint64>)(uintptr)(new @unsafe.Pointer(Ꮡ(value))));
}

// UnsafePointer is an atomically accessed unsafe.Pointer value.
//
// Note that because of the atomicity guarantees, stores to values
// of this type never trigger a write barrier, and the relevant
// methods are suffixed with "NoWB" to indicate that explicitly.
// As a result, this type should be used carefully, and sparingly,
// mostly with values that do not live in the Go heap anyway.
//
// An UnsafePointer must not be copied.
[GoType] partial struct UnsafePointer {
    internal noCopy noCopy;
    internal @unsafe.Pointer value;
}

// Load accesses and returns the value atomically.
//
//go:nosplit
[GoRecv] public static @unsafe.Pointer Load(this ref UnsafePointer u) {
    return (uintptr)Loadp(((@unsafe.Pointer)(Ꮡ(u.value))));
}

// StoreNoWB updates the value atomically.
//
// WARNING: As the name implies this operation does *not*
// perform a write barrier on value, and so this operation may
// hide pointers from the GC. Use with care and sparingly.
// It is safe to use with values not found in the Go heap.
// Prefer Store instead.
//
//go:nosplit
[GoRecv] public static void StoreNoWB(this ref UnsafePointer u, @unsafe.Pointer value) {
    StorepNoWB(((@unsafe.Pointer)(Ꮡ(u.value))), value.val);
}

// Store updates the value atomically.
[GoRecv] public static void Store(this ref UnsafePointer u, @unsafe.Pointer value) {
    storePointer(Ꮡ(u.value), value.val);
}

// provided by runtime
//
//go:linkname storePointer
internal static partial void storePointer(ж<@unsafe.Pointer> ptr, @unsafe.Pointer @new);

// CompareAndSwapNoWB atomically (with respect to other methods)
// compares u's value with old, and if they're equal,
// swaps u's value with new.
// It reports whether the swap ran.
//
// WARNING: As the name implies this operation does *not*
// perform a write barrier on value, and so this operation may
// hide pointers from the GC. Use with care and sparingly.
// It is safe to use with values not found in the Go heap.
// Prefer CompareAndSwap instead.
//
//go:nosplit
[GoRecv] public static bool CompareAndSwapNoWB(this ref UnsafePointer u, @unsafe.Pointer old, @unsafe.Pointer @new) {
    return Casp1(Ꮡ(u.value), old.val, new.val);
}

// CompareAndSwap atomically compares u's value with old,
// and if they're equal, swaps u's value with new.
// It reports whether the swap ran.
[GoRecv] public static bool CompareAndSwap(this ref UnsafePointer u, @unsafe.Pointer old, @unsafe.Pointer @new) {
    return casPointer(Ꮡ(u.value), old.val, new.val);
}

internal static partial bool casPointer(ж<@unsafe.Pointer> ptr, @unsafe.Pointer old, @unsafe.Pointer @new);

// Pointer is an atomic pointer of type *T.
[GoType] partial struct Pointer<T>
    where T : new()
{
    internal UnsafePointer u;
}

// Load accesses and returns the value atomically.
//
//go:nosplit
[GoRecv] public static ж<T> Load<T>(this ref Pointer<T> p)
    where T : new()
{
    return (ж<T>)(uintptr)(p.u.Load());
}

// StoreNoWB updates the value atomically.
//
// WARNING: As the name implies this operation does *not*
// perform a write barrier on value, and so this operation may
// hide pointers from the GC. Use with care and sparingly.
// It is safe to use with values not found in the Go heap.
// Prefer Store instead.
//
//go:nosplit
[GoRecv] public static void StoreNoWB<T>(this ref Pointer<T> p, ж<T> Ꮡvalue)
    where T : new()
{
    ref var value = ref Ꮡvalue.val;

    p.u.StoreNoWB(new @unsafe.Pointer(Ꮡvalue));
}

// Store updates the value atomically.
//
//go:nosplit
[GoRecv] public static void Store<T>(this ref Pointer<T> p, ж<T> Ꮡvalue)
    where T : new()
{
    ref var value = ref Ꮡvalue.val;

    p.u.Store(new @unsafe.Pointer(Ꮡvalue));
}

// CompareAndSwapNoWB atomically (with respect to other methods)
// compares u's value with old, and if they're equal,
// swaps u's value with new.
// It reports whether the swap ran.
//
// WARNING: As the name implies this operation does *not*
// perform a write barrier on value, and so this operation may
// hide pointers from the GC. Use with care and sparingly.
// It is safe to use with values not found in the Go heap.
// Prefer CompareAndSwap instead.
//
//go:nosplit
[GoRecv] public static bool CompareAndSwapNoWB<T>(this ref Pointer<T> p, ж<T> Ꮡold, ж<T> Ꮡnew)
    where T : new()
{
    ref var old = ref Ꮡold.val;
    ref var @new = ref Ꮡnew.val;

    return p.u.CompareAndSwapNoWB(new @unsafe.Pointer(Ꮡold), new @unsafe.Pointer(Ꮡnew));
}

// CompareAndSwap atomically (with respect to other methods)
// compares u's value with old, and if they're equal,
// swaps u's value with new.
// It reports whether the swap ran.
[GoRecv] public static bool CompareAndSwap<T>(this ref Pointer<T> p, ж<T> Ꮡold, ж<T> Ꮡnew)
    where T : new()
{
    ref var old = ref Ꮡold.val;
    ref var @new = ref Ꮡnew.val;

    return p.u.CompareAndSwap(new @unsafe.Pointer(Ꮡold), new @unsafe.Pointer(Ꮡnew));
}

// noCopy may be embedded into structs which must not be copied
// after the first use.
//
// See https://golang.org/issues/8005#issuecomment-190753527
// for details.
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
