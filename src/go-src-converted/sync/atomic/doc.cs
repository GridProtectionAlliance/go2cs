// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package atomic provides low-level atomic memory primitives
// useful for implementing synchronization algorithms.
//
// These functions require great care to be used correctly.
// Except for special, low-level applications, synchronization is better
// done with channels or the facilities of the sync package.
// Share memory by communicating;
// don't communicate by sharing memory.
//
// The swap operation, implemented by the SwapT functions, is the atomic
// equivalent of:
//
//    old = *addr
//    *addr = new
//    return old
//
// The compare-and-swap operation, implemented by the CompareAndSwapT
// functions, is the atomic equivalent of:
//
//    if *addr == old {
//        *addr = new
//        return true
//    }
//    return false
//
// The add operation, implemented by the AddT functions, is the atomic
// equivalent of:
//
//    *addr += delta
//    return *addr
//
// The load and store operations, implemented by the LoadT and StoreT
// functions, are the atomic equivalents of "return *addr" and
// "*addr = val".
//

// package atomic -- go2cs converted at 2022 March 13 05:24:02 UTC
// import "sync/atomic" ==> using atomic = go.sync.atomic_package
// Original source: C:\Program Files\Go\src\sync\atomic\doc.go
namespace go.sync;

using @unsafe = @unsafe_package;


// BUG(rsc): On 386, the 64-bit functions use instructions unavailable before the Pentium MMX.
//
// On non-Linux ARM, the 64-bit functions use instructions unavailable before the ARMv6k core.
//
// On ARM, 386, and 32-bit MIPS, it is the caller's responsibility
// to arrange for 64-bit alignment of 64-bit words accessed atomically.
// The first word in a variable or in an allocated struct, array, or slice can
// be relied upon to be 64-bit aligned.

// SwapInt32 atomically stores new into *addr and returns the previous *addr value.

public static partial class atomic_package {

public static int SwapInt32(ptr<int> addr, int @new);

// SwapInt64 atomically stores new into *addr and returns the previous *addr value.
public static long SwapInt64(ptr<long> addr, long @new);

// SwapUint32 atomically stores new into *addr and returns the previous *addr value.
public static uint SwapUint32(ptr<uint> addr, uint @new);

// SwapUint64 atomically stores new into *addr and returns the previous *addr value.
public static ulong SwapUint64(ptr<ulong> addr, ulong @new);

// SwapUintptr atomically stores new into *addr and returns the previous *addr value.
public static System.UIntPtr SwapUintptr(ptr<System.UIntPtr> addr, System.UIntPtr @new);

// SwapPointer atomically stores new into *addr and returns the previous *addr value.
public static unsafe.Pointer SwapPointer(ptr<unsafe.Pointer> addr, unsafe.Pointer @new);

// CompareAndSwapInt32 executes the compare-and-swap operation for an int32 value.
public static bool CompareAndSwapInt32(ptr<int> addr, int old, int @new);

// CompareAndSwapInt64 executes the compare-and-swap operation for an int64 value.
public static bool CompareAndSwapInt64(ptr<long> addr, long old, long @new);

// CompareAndSwapUint32 executes the compare-and-swap operation for a uint32 value.
public static bool CompareAndSwapUint32(ptr<uint> addr, uint old, uint @new);

// CompareAndSwapUint64 executes the compare-and-swap operation for a uint64 value.
public static bool CompareAndSwapUint64(ptr<ulong> addr, ulong old, ulong @new);

// CompareAndSwapUintptr executes the compare-and-swap operation for a uintptr value.
public static bool CompareAndSwapUintptr(ptr<System.UIntPtr> addr, System.UIntPtr old, System.UIntPtr @new);

// CompareAndSwapPointer executes the compare-and-swap operation for a unsafe.Pointer value.
public static bool CompareAndSwapPointer(ptr<unsafe.Pointer> addr, unsafe.Pointer old, unsafe.Pointer @new);

// AddInt32 atomically adds delta to *addr and returns the new value.
public static int AddInt32(ptr<int> addr, int delta);

// AddUint32 atomically adds delta to *addr and returns the new value.
// To subtract a signed positive constant value c from x, do AddUint32(&x, ^uint32(c-1)).
// In particular, to decrement x, do AddUint32(&x, ^uint32(0)).
public static uint AddUint32(ptr<uint> addr, uint delta);

// AddInt64 atomically adds delta to *addr and returns the new value.
public static long AddInt64(ptr<long> addr, long delta);

// AddUint64 atomically adds delta to *addr and returns the new value.
// To subtract a signed positive constant value c from x, do AddUint64(&x, ^uint64(c-1)).
// In particular, to decrement x, do AddUint64(&x, ^uint64(0)).
public static ulong AddUint64(ptr<ulong> addr, ulong delta);

// AddUintptr atomically adds delta to *addr and returns the new value.
public static System.UIntPtr AddUintptr(ptr<System.UIntPtr> addr, System.UIntPtr delta);

// LoadInt32 atomically loads *addr.
public static int LoadInt32(ptr<int> addr);

// LoadInt64 atomically loads *addr.
public static long LoadInt64(ptr<long> addr);

// LoadUint32 atomically loads *addr.
public static uint LoadUint32(ptr<uint> addr);

// LoadUint64 atomically loads *addr.
public static ulong LoadUint64(ptr<ulong> addr);

// LoadUintptr atomically loads *addr.
public static System.UIntPtr LoadUintptr(ptr<System.UIntPtr> addr);

// LoadPointer atomically loads *addr.
public static unsafe.Pointer LoadPointer(ptr<unsafe.Pointer> addr);

// StoreInt32 atomically stores val into *addr.
public static void StoreInt32(ptr<int> addr, int val);

// StoreInt64 atomically stores val into *addr.
public static void StoreInt64(ptr<long> addr, long val);

// StoreUint32 atomically stores val into *addr.
public static void StoreUint32(ptr<uint> addr, uint val);

// StoreUint64 atomically stores val into *addr.
public static void StoreUint64(ptr<ulong> addr, ulong val);

// StoreUintptr atomically stores val into *addr.
public static void StoreUintptr(ptr<System.UIntPtr> addr, System.UIntPtr val);

// StorePointer atomically stores val into *addr.
public static void StorePointer(ptr<unsafe.Pointer> addr, unsafe.Pointer val);

} // end atomic_package
