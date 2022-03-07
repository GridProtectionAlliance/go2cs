// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package atomic -- go2cs converted at 2022 March 06 22:08:18 UTC
// import "runtime/internal/atomic" ==> using atomic = go.runtime.@internal.atomic_package
// Original source: C:\Program Files\Go\src\runtime\internal\atomic\atomic_amd64.go
using @unsafe = go.@unsafe_package;

namespace go.runtime.@internal;

public static partial class atomic_package {

    // Export some functions via linkname to assembly in sync/atomic.
    //go:linkname Load
    //go:linkname Loadp
    //go:linkname Load64

    //go:nosplit
    //go:noinline
public static uint Load(ptr<uint> _addr_ptr) {
    ref uint ptr = ref _addr_ptr.val;

    return ptr;
}

//go:nosplit
//go:noinline
public static unsafe.Pointer Loadp(unsafe.Pointer ptr) {
    return new ptr<ptr<ptr<unsafe.Pointer>>>(ptr);
}

//go:nosplit
//go:noinline
public static ulong Load64(ptr<ulong> _addr_ptr) {
    ref ulong ptr = ref _addr_ptr.val;

    return ptr;
}

//go:nosplit
//go:noinline
public static uint LoadAcq(ptr<uint> _addr_ptr) {
    ref uint ptr = ref _addr_ptr.val;

    return ptr;
}

//go:nosplit
//go:noinline
public static ulong LoadAcq64(ptr<ulong> _addr_ptr) {
    ref ulong ptr = ref _addr_ptr.val;

    return ptr;
}

//go:nosplit
//go:noinline
public static System.UIntPtr LoadAcquintptr(ptr<System.UIntPtr> _addr_ptr) {
    ref System.UIntPtr ptr = ref _addr_ptr.val;

    return ptr;
}

//go:noescape
public static uint Xadd(ptr<uint> ptr, int delta);

//go:noescape
public static ulong Xadd64(ptr<ulong> ptr, long delta);

//go:noescape
public static System.UIntPtr Xadduintptr(ptr<System.UIntPtr> ptr, System.UIntPtr delta);

//go:noescape
public static uint Xchg(ptr<uint> ptr, uint @new);

//go:noescape
public static ulong Xchg64(ptr<ulong> ptr, ulong @new);

//go:noescape
public static System.UIntPtr Xchguintptr(ptr<System.UIntPtr> ptr, System.UIntPtr @new);

//go:nosplit
//go:noinline
public static byte Load8(ptr<byte> _addr_ptr) {
    ref byte ptr = ref _addr_ptr.val;

    return ptr;
}

//go:noescape
public static void And8(ptr<byte> ptr, byte val);

//go:noescape
public static void Or8(ptr<byte> ptr, byte val);

//go:noescape
public static void And(ptr<uint> ptr, uint val);

//go:noescape
public static void Or(ptr<uint> ptr, uint val);

// NOTE: Do not add atomicxor8 (XOR is not idempotent).

//go:noescape
public static bool Cas64(ptr<ulong> ptr, ulong old, ulong @new);

//go:noescape
public static bool CasRel(ptr<uint> ptr, uint old, uint @new);

//go:noescape
public static void Store(ptr<uint> ptr, uint val);

//go:noescape
public static void Store8(ptr<byte> ptr, byte val);

//go:noescape
public static void Store64(ptr<ulong> ptr, ulong val);

//go:noescape
public static void StoreRel(ptr<uint> ptr, uint val);

//go:noescape
public static void StoreRel64(ptr<ulong> ptr, ulong val);

//go:noescape
public static void StoreReluintptr(ptr<System.UIntPtr> ptr, System.UIntPtr val);

// StorepNoWB performs *ptr = val atomically and without a write
// barrier.
//
// NO go:noescape annotation; see atomic_pointer.go.
public static void StorepNoWB(unsafe.Pointer ptr, unsafe.Pointer val);

} // end atomic_package
