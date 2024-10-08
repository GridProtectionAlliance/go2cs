// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build mips64 || mips64le
// +build mips64 mips64le

// package atomic -- go2cs converted at 2022 March 13 05:24:08 UTC
// import "runtime/internal/atomic" ==> using atomic = go.runtime.@internal.atomic_package
// Original source: C:\Program Files\Go\src\runtime\internal\atomic\atomic_mips64x.go
namespace go.runtime.@internal;

using @unsafe = @unsafe_package;

public static partial class atomic_package {

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

//go:noescape
public static uint Load(ptr<uint> ptr);

//go:noescape
public static byte Load8(ptr<byte> ptr);

//go:noescape
public static ulong Load64(ptr<ulong> ptr);

// NO go:noescape annotation; *ptr escapes if result escapes (#31525)
public static unsafe.Pointer Loadp(unsafe.Pointer ptr);

//go:noescape
public static uint LoadAcq(ptr<uint> ptr);

//go:noescape
public static ulong LoadAcq64(ptr<ulong> ptr);

//go:noescape
public static System.UIntPtr LoadAcquintptr(ptr<System.UIntPtr> ptr);

//go:noescape
public static void And8(ptr<byte> ptr, byte val);

//go:noescape
public static void Or8(ptr<byte> ptr, byte val);

// NOTE: Do not add atomicxor8 (XOR is not idempotent).

//go:noescape
public static void And(ptr<uint> ptr, uint val);

//go:noescape
public static void Or(ptr<uint> ptr, uint val);

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

// NO go:noescape annotation; see atomic_pointer.go.
public static void StorepNoWB(unsafe.Pointer ptr, unsafe.Pointer val);

//go:noescape
public static void StoreRel(ptr<uint> ptr, uint val);

//go:noescape
public static void StoreRel64(ptr<ulong> ptr, ulong val);

//go:noescape
public static void StoreReluintptr(ptr<System.UIntPtr> ptr, System.UIntPtr val);

} // end atomic_package
