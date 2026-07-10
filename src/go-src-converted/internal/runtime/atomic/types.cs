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
public static int32 Load(this ж<Int32> Ꮡi) {
    ref var i = ref Ꮡi.Value;

    return Loadint32(Ꮡi.of(Int32.Ꮡvalue));
}

// Store updates the value atomically.
//
//go:nosplit
public static void Store(this ж<Int32> Ꮡi, int32 value) {
    ref var i = ref Ꮡi.Value;

    Storeint32(Ꮡi.of(Int32.Ꮡvalue), value);
}

// CompareAndSwap atomically compares i's value with old,
// and if they're equal, swaps i's value with new.
// It reports whether the swap ran.
//
//go:nosplit
public static bool CompareAndSwap(this ж<Int32> Ꮡi, int32 old, int32 @new) {
    ref var i = ref Ꮡi.Value;

    return Casint32(Ꮡi.of(Int32.Ꮡvalue), old, @new);
}

// Swap replaces i's value with new, returning
// i's value before the replacement.
//
//go:nosplit
public static int32 Swap(this ж<Int32> Ꮡi, int32 @new) {
    ref var i = ref Ꮡi.Value;

    return Xchgint32(Ꮡi.of(Int32.Ꮡvalue), @new);
}

// Add adds delta to i atomically, returning
// the new updated value.
//
// This operation wraps around in the usual
// two's-complement way.
//
//go:nosplit
public static int32 Add(this ж<Int32> Ꮡi, int32 delta) {
    ref var i = ref Ꮡi.Value;

    return Xaddint32(Ꮡi.of(Int32.Ꮡvalue), delta);
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
public static int64 Load(this ж<Int64> Ꮡi) {
    ref var i = ref Ꮡi.Value;

    return Loadint64(Ꮡi.of(Int64.Ꮡvalue));
}

// Store updates the value atomically.
//
//go:nosplit
public static void Store(this ж<Int64> Ꮡi, int64 value) {
    ref var i = ref Ꮡi.Value;

    Storeint64(Ꮡi.of(Int64.Ꮡvalue), value);
}

// CompareAndSwap atomically compares i's value with old,
// and if they're equal, swaps i's value with new.
// It reports whether the swap ran.
//
//go:nosplit
public static bool CompareAndSwap(this ж<Int64> Ꮡi, int64 old, int64 @new) {
    ref var i = ref Ꮡi.Value;

    return Casint64(Ꮡi.of(Int64.Ꮡvalue), old, @new);
}

// Swap replaces i's value with new, returning
// i's value before the replacement.
//
//go:nosplit
public static int64 Swap(this ж<Int64> Ꮡi, int64 @new) {
    ref var i = ref Ꮡi.Value;

    return Xchgint64(Ꮡi.of(Int64.Ꮡvalue), @new);
}

// Add adds delta to i atomically, returning
// the new updated value.
//
// This operation wraps around in the usual
// two's-complement way.
//
//go:nosplit
public static int64 Add(this ж<Int64> Ꮡi, int64 delta) {
    ref var i = ref Ꮡi.Value;

    return Xaddint64(Ꮡi.of(Int64.Ꮡvalue), delta);
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
public static uint8 Load(this ж<Uint8> Ꮡu) {
    ref var u = ref Ꮡu.Value;

    return Load8(Ꮡu.of(Uint8.Ꮡvalue));
}

// Store updates the value atomically.
//
//go:nosplit
public static void Store(this ж<Uint8> Ꮡu, uint8 value) {
    ref var u = ref Ꮡu.Value;

    Store8(Ꮡu.of(Uint8.Ꮡvalue), value);
}

// And takes value and performs a bit-wise
// "and" operation with the value of u, storing
// the result into u.
//
// The full process is performed atomically.
//
//go:nosplit
public static void And(this ж<Uint8> Ꮡu, uint8 value) {
    ref var u = ref Ꮡu.Value;

    And8(Ꮡu.of(Uint8.Ꮡvalue), value);
}

// Or takes value and performs a bit-wise
// "or" operation with the value of u, storing
// the result into u.
//
// The full process is performed atomically.
//
//go:nosplit
public static void Or(this ж<Uint8> Ꮡu, uint8 value) {
    ref var u = ref Ꮡu.Value;

    Or8(Ꮡu.of(Uint8.Ꮡvalue), value);
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
public static bool Load(this ж<Bool> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    return Ꮡb.of(Bool.Ꮡu).Load() != 0;
}

// Store updates the value atomically.
//
//go:nosplit
public static void Store(this ж<Bool> Ꮡb, bool value) {
    ref var b = ref Ꮡb.Value;

    var s = (uint8)0;
    if (value) {
        s = 1;
    }
    Ꮡb.of(Bool.Ꮡu).Store(s);
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
public static uint32 Load(this ж<Uint32> Ꮡu) {
    ref var u = ref Ꮡu.Value;

    return Load(Ꮡu.of(Uint32.Ꮡvalue));
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
public static uint32 LoadAcquire(this ж<Uint32> Ꮡu) {
    ref var u = ref Ꮡu.Value;

    return LoadAcq(Ꮡu.of(Uint32.Ꮡvalue));
}

// Store updates the value atomically.
//
//go:nosplit
public static void Store(this ж<Uint32> Ꮡu, uint32 value) {
    ref var u = ref Ꮡu.Value;

    Store(Ꮡu.of(Uint32.Ꮡvalue), value);
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
public static void StoreRelease(this ж<Uint32> Ꮡu, uint32 value) {
    ref var u = ref Ꮡu.Value;

    StoreRel(Ꮡu.of(Uint32.Ꮡvalue), value);
}

// CompareAndSwap atomically compares u's value with old,
// and if they're equal, swaps u's value with new.
// It reports whether the swap ran.
//
//go:nosplit
public static bool CompareAndSwap(this ж<Uint32> Ꮡu, uint32 old, uint32 @new) {
    ref var u = ref Ꮡu.Value;

    return Cas(Ꮡu.of(Uint32.Ꮡvalue), old, @new);
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
public static bool CompareAndSwapRelease(this ж<Uint32> Ꮡu, uint32 old, uint32 @new) {
    ref var u = ref Ꮡu.Value;

    return CasRel(Ꮡu.of(Uint32.Ꮡvalue), old, @new);
}

// Swap replaces u's value with new, returning
// u's value before the replacement.
//
//go:nosplit
public static uint32 Swap(this ж<Uint32> Ꮡu, uint32 value) {
    ref var u = ref Ꮡu.Value;

    return Xchg(Ꮡu.of(Uint32.Ꮡvalue), value);
}

// And takes value and performs a bit-wise
// "and" operation with the value of u, storing
// the result into u.
//
// The full process is performed atomically.
//
//go:nosplit
public static void And(this ж<Uint32> Ꮡu, uint32 value) {
    ref var u = ref Ꮡu.Value;

    And(Ꮡu.of(Uint32.Ꮡvalue), value);
}

// Or takes value and performs a bit-wise
// "or" operation with the value of u, storing
// the result into u.
//
// The full process is performed atomically.
//
//go:nosplit
public static void Or(this ж<Uint32> Ꮡu, uint32 value) {
    ref var u = ref Ꮡu.Value;

    Or(Ꮡu.of(Uint32.Ꮡvalue), value);
}

// Add adds delta to u atomically, returning
// the new updated value.
//
// This operation wraps around in the usual
// two's-complement way.
//
//go:nosplit
public static uint32 Add(this ж<Uint32> Ꮡu, int32 delta) {
    ref var u = ref Ꮡu.Value;

    return Xadd(Ꮡu.of(Uint32.Ꮡvalue), delta);
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
public static uint64 Load(this ж<Uint64> Ꮡu) {
    ref var u = ref Ꮡu.Value;

    return Load64(Ꮡu.of(Uint64.Ꮡvalue));
}

// Store updates the value atomically.
//
//go:nosplit
public static void Store(this ж<Uint64> Ꮡu, uint64 value) {
    ref var u = ref Ꮡu.Value;

    Store64(Ꮡu.of(Uint64.Ꮡvalue), value);
}

// CompareAndSwap atomically compares u's value with old,
// and if they're equal, swaps u's value with new.
// It reports whether the swap ran.
//
//go:nosplit
public static bool CompareAndSwap(this ж<Uint64> Ꮡu, uint64 old, uint64 @new) {
    ref var u = ref Ꮡu.Value;

    return Cas64(Ꮡu.of(Uint64.Ꮡvalue), old, @new);
}

// Swap replaces u's value with new, returning
// u's value before the replacement.
//
//go:nosplit
public static uint64 Swap(this ж<Uint64> Ꮡu, uint64 value) {
    ref var u = ref Ꮡu.Value;

    return Xchg64(Ꮡu.of(Uint64.Ꮡvalue), value);
}

// Add adds delta to u atomically, returning
// the new updated value.
//
// This operation wraps around in the usual
// two's-complement way.
//
//go:nosplit
public static uint64 Add(this ж<Uint64> Ꮡu, int64 delta) {
    ref var u = ref Ꮡu.Value;

    return Xadd64(Ꮡu.of(Uint64.Ꮡvalue), delta);
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
public static uintptr Load(this ж<Uintptr> Ꮡu) {
    ref var u = ref Ꮡu.Value;

    return Loaduintptr(Ꮡu.of(Uintptr.Ꮡvalue));
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
public static uintptr LoadAcquire(this ж<Uintptr> Ꮡu) {
    ref var u = ref Ꮡu.Value;

    return LoadAcquintptr(Ꮡu.of(Uintptr.Ꮡvalue));
}

// Store updates the value atomically.
//
//go:nosplit
public static void Store(this ж<Uintptr> Ꮡu, uintptr value) {
    ref var u = ref Ꮡu.Value;

    Storeuintptr(Ꮡu.of(Uintptr.Ꮡvalue), value);
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
public static void StoreRelease(this ж<Uintptr> Ꮡu, uintptr value) {
    ref var u = ref Ꮡu.Value;

    StoreReluintptr(Ꮡu.of(Uintptr.Ꮡvalue), value);
}

// CompareAndSwap atomically compares u's value with old,
// and if they're equal, swaps u's value with new.
// It reports whether the swap ran.
//
//go:nosplit
public static bool CompareAndSwap(this ж<Uintptr> Ꮡu, uintptr old, uintptr @new) {
    ref var u = ref Ꮡu.Value;

    return Casuintptr(Ꮡu.of(Uintptr.Ꮡvalue), old, @new);
}

// Swap replaces u's value with new, returning
// u's value before the replacement.
//
//go:nosplit
public static uintptr Swap(this ж<Uintptr> Ꮡu, uintptr value) {
    ref var u = ref Ꮡu.Value;

    return Xchguintptr(Ꮡu.of(Uintptr.Ꮡvalue), value);
}

// Add adds delta to u atomically, returning
// the new updated value.
//
// This operation wraps around in the usual
// two's-complement way.
//
//go:nosplit
public static uintptr Add(this ж<Uintptr> Ꮡu, uintptr delta) {
    ref var u = ref Ꮡu.Value;

    return Xadduintptr(Ꮡu.of(Uintptr.Ꮡvalue), delta);
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
public static float64 Load(this ж<Float64> Ꮡf) {
    ref var f = ref Ꮡf.Value;

    ref var r = ref heap<uint64>(out var Ꮡr);
    r = Ꮡf.of(Float64.Ꮡu).Load();
    return ~(ж<float64>)(uintptr)(new @unsafe.Pointer(Ꮡr));
}

// Store updates the value atomically.
//
//go:nosplit
public static void Store(this ж<Float64> Ꮡf, float64 value) {
    ref var f = ref Ꮡf.Value;

    Ꮡf.of(Float64.Ꮡu).Store(~(ж<uint64>)(uintptr)(new @unsafe.Pointer(Ꮡ(value))));
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
public static @unsafe.Pointer Load(this ж<UnsafePointer> Ꮡu) {
    ref var u = ref Ꮡu.Value;

    return (uintptr)Loadp(@unsafe.Pointer.FromRef(ref (Ꮡu.of(UnsafePointer.Ꮡvalue)).Value));
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
public static void StoreNoWB(this ж<UnsafePointer> Ꮡu, @unsafe.Pointer value) {
    ref var u = ref Ꮡu.Value;

    StorepNoWB(@unsafe.Pointer.FromRef(ref (Ꮡu.of(UnsafePointer.Ꮡvalue)).Value), value);
}

// Store updates the value atomically.
public static void Store(this ж<UnsafePointer> Ꮡu, @unsafe.Pointer value) {
    ref var u = ref Ꮡu.Value;

    storePointer(Ꮡu.of(UnsafePointer.Ꮡvalue), value);
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
public static bool CompareAndSwapNoWB(this ж<UnsafePointer> Ꮡu, @unsafe.Pointer old, @unsafe.Pointer @new) {
    ref var u = ref Ꮡu.Value;

    return Casp1(Ꮡu.of(UnsafePointer.Ꮡvalue), old, @new);
}

// CompareAndSwap atomically compares u's value with old,
// and if they're equal, swaps u's value with new.
// It reports whether the swap ran.
public static bool CompareAndSwap(this ж<UnsafePointer> Ꮡu, @unsafe.Pointer old, @unsafe.Pointer @new) {
    ref var u = ref Ꮡu.Value;

    return casPointer(Ꮡu.of(UnsafePointer.Ꮡvalue), old, @new);
}

internal static partial bool casPointer(ж<@unsafe.Pointer> ptr, @unsafe.Pointer old, @unsafe.Pointer @new);

// Pointer is an atomic pointer of type *T.
[GoType] partial struct Pointer<T> {
    internal UnsafePointer u;
}

// Load accesses and returns the value atomically.
//
//go:nosplit
public static ж<T> Load<T>(this ж<Pointer<T>> Ꮡp) {
    ref var p = ref Ꮡp.Value;

    return (ж<T>)(uintptr)(Ꮡp.of(Pointer<T>.Ꮡu).Load());
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
public static void StoreNoWB<T>(this ж<Pointer<T>> Ꮡp, ж<T> Ꮡvalue) {
    ref var p = ref Ꮡp.Value;
    ref var value = ref Ꮡvalue.Value;

    Ꮡp.of(Pointer<T>.Ꮡu).StoreNoWB(new @unsafe.Pointer(Ꮡvalue));
}

// Store updates the value atomically.
//
//go:nosplit
public static void Store<T>(this ж<Pointer<T>> Ꮡp, ж<T> Ꮡvalue) {
    ref var p = ref Ꮡp.Value;
    ref var value = ref Ꮡvalue.Value;

    Ꮡp.of(Pointer<T>.Ꮡu).Store(new @unsafe.Pointer(Ꮡvalue));
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
public static bool CompareAndSwapNoWB<T>(this ж<Pointer<T>> Ꮡp, ж<T> Ꮡold, ж<T> Ꮡnew) {
    ref var p = ref Ꮡp.Value;
    ref var old = ref Ꮡold.Value;
    ref var @new = ref Ꮡnew.Value;

    return Ꮡp.of(Pointer<T>.Ꮡu).CompareAndSwapNoWB(new @unsafe.Pointer(Ꮡold), new @unsafe.Pointer(Ꮡnew));
}

// CompareAndSwap atomically (with respect to other methods)
// compares u's value with old, and if they're equal,
// swaps u's value with new.
// It reports whether the swap ran.
public static bool CompareAndSwap<T>(this ж<Pointer<T>> Ꮡp, ж<T> Ꮡold, ж<T> Ꮡnew) {
    ref var p = ref Ꮡp.Value;
    ref var old = ref Ꮡold.Value;
    ref var @new = ref Ꮡnew.Value;

    return Ꮡp.of(Pointer<T>.Ꮡu).CompareAndSwap(new @unsafe.Pointer(Ꮡold), new @unsafe.Pointer(Ꮡnew));
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
