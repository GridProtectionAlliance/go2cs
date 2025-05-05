// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime;

using @unsafe = unsafe_package;

partial class atomic_package {

// Export some functions via linkname to assembly in sync/atomic.
//
//go:linkname Load
//go:linkname Loadp
//go:linkname Load64

//go:nosplit
//go:noinline
public static uint32 Load(ж<uint32> Ꮡptr) {
    ref var ptr = ref Ꮡptr.val;

    return ptr;
}

//go:nosplit
//go:noinline
public static @unsafe.Pointer Loadp(@unsafe.Pointer ptr) {
    return ~(ж<@unsafe.Pointer>)(uintptr)(ptr);
}

//go:nosplit
//go:noinline
public static uint64 Load64(ж<uint64> Ꮡptr) {
    ref var ptr = ref Ꮡptr.val;

    return ptr;
}

//go:nosplit
//go:noinline
public static uint32 LoadAcq(ж<uint32> Ꮡptr) {
    ref var ptr = ref Ꮡptr.val;

    return ptr;
}

//go:nosplit
//go:noinline
public static uint64 LoadAcq64(ж<uint64> Ꮡptr) {
    ref var ptr = ref Ꮡptr.val;

    return ptr;
}

//go:nosplit
//go:noinline
public static uintptr LoadAcquintptr(ж<uintptr> Ꮡptr) {
    ref var ptr = ref Ꮡptr.val;

    return ptr;
}

//go:noescape
public static partial uint32 Xadd(ж<uint32> ptr, int32 delta);

//go:noescape
public static partial uint64 Xadd64(ж<uint64> ptr, int64 delta);

//go:noescape
public static partial uintptr Xadduintptr(ж<uintptr> ptr, uintptr delta);

//go:noescape
public static partial uint32 Xchg(ж<uint32> ptr, uint32 @new);

//go:noescape
public static partial uint64 Xchg64(ж<uint64> ptr, uint64 @new);

//go:noescape
public static partial uintptr Xchguintptr(ж<uintptr> ptr, uintptr @new);

//go:nosplit
//go:noinline
public static uint8 Load8(ж<uint8> Ꮡptr) {
    ref var ptr = ref Ꮡptr.val;

    return ptr;
}

//go:noescape
public static partial void And8(ж<uint8> ptr, uint8 val);

//go:noescape
public static partial void Or8(ж<uint8> ptr, uint8 val);

//go:noescape
public static partial void And(ж<uint32> ptr, uint32 val);

//go:noescape
public static partial void Or(ж<uint32> ptr, uint32 val);

//go:noescape
public static partial uint32 And32(ж<uint32> ptr, uint32 val);

//go:noescape
public static partial uint32 Or32(ж<uint32> ptr, uint32 val);

//go:noescape
public static partial uint64 And64(ж<uint64> ptr, uint64 val);

//go:noescape
public static partial uint64 Or64(ж<uint64> ptr, uint64 val);

//go:noescape
public static partial uintptr Anduintptr(ж<uintptr> ptr, uintptr val);

//go:noescape
public static partial uintptr Oruintptr(ж<uintptr> ptr, uintptr val);

// NOTE: Do not add atomicxor8 (XOR is not idempotent).

//go:noescape
public static partial bool Cas64(ж<uint64> ptr, uint64 old, uint64 @new);

//go:noescape
public static partial bool CasRel(ж<uint32> ptr, uint32 old, uint32 @new);

//go:noescape
public static partial void Store(ж<uint32> ptr, uint32 val);

//go:noescape
public static partial void Store8(ж<uint8> ptr, uint8 val);

//go:noescape
public static partial void Store64(ж<uint64> ptr, uint64 val);

//go:noescape
public static partial void StoreRel(ж<uint32> ptr, uint32 val);

//go:noescape
public static partial void StoreRel64(ж<uint64> ptr, uint64 val);

//go:noescape
public static partial void StoreReluintptr(ж<uintptr> ptr, uintptr val);

// StorepNoWB performs *ptr = val atomically and without a write
// barrier.
//
// NO go:noescape annotation; see atomic_pointer.go.
public static partial void StorepNoWB(@unsafe.Pointer ptr, @unsafe.Pointer val);

} // end atomic_package
