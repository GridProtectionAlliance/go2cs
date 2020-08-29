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
// package atomic -- go2cs converted at 2020 August 29 08:16:21 UTC
// import "sync/atomic" ==> using atomic = go.sync.atomic_package
// Original source: C:\Go\src\sync\atomic\doc.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace sync
{
    public static partial class atomic_package
    {
        // BUG(rsc): On x86-32, the 64-bit functions use instructions unavailable before the Pentium MMX.
        //
        // On non-Linux ARM, the 64-bit functions use instructions unavailable before the ARMv6k core.
        //
        // On both ARM and x86-32, it is the caller's responsibility to arrange for 64-bit
        // alignment of 64-bit words accessed atomically. The first word in a
        // variable or in an allocated struct, array, or slice can be relied upon to be
        // 64-bit aligned.

        // SwapInt32 atomically stores new into *addr and returns the previous *addr value.
        public static int SwapInt32(ref int addr, int @new)
;

        // SwapInt64 atomically stores new into *addr and returns the previous *addr value.
        public static long SwapInt64(ref long addr, long @new)
;

        // SwapUint32 atomically stores new into *addr and returns the previous *addr value.
        public static uint SwapUint32(ref uint addr, uint @new)
;

        // SwapUint64 atomically stores new into *addr and returns the previous *addr value.
        public static ulong SwapUint64(ref ulong addr, ulong @new)
;

        // SwapUintptr atomically stores new into *addr and returns the previous *addr value.
        public static System.UIntPtr SwapUintptr(ref System.UIntPtr addr, System.UIntPtr @new)
;

        // SwapPointer atomically stores new into *addr and returns the previous *addr value.
        public static unsafe.Pointer SwapPointer(ref unsafe.Pointer addr, unsafe.Pointer @new)
;

        // CompareAndSwapInt32 executes the compare-and-swap operation for an int32 value.
        public static bool CompareAndSwapInt32(ref int addr, int old, int @new)
;

        // CompareAndSwapInt64 executes the compare-and-swap operation for an int64 value.
        public static bool CompareAndSwapInt64(ref long addr, long old, long @new)
;

        // CompareAndSwapUint32 executes the compare-and-swap operation for a uint32 value.
        public static bool CompareAndSwapUint32(ref uint addr, uint old, uint @new)
;

        // CompareAndSwapUint64 executes the compare-and-swap operation for a uint64 value.
        public static bool CompareAndSwapUint64(ref ulong addr, ulong old, ulong @new)
;

        // CompareAndSwapUintptr executes the compare-and-swap operation for a uintptr value.
        public static bool CompareAndSwapUintptr(ref System.UIntPtr addr, System.UIntPtr old, System.UIntPtr @new)
;

        // CompareAndSwapPointer executes the compare-and-swap operation for a unsafe.Pointer value.
        public static bool CompareAndSwapPointer(ref unsafe.Pointer addr, unsafe.Pointer old, unsafe.Pointer @new)
;

        // AddInt32 atomically adds delta to *addr and returns the new value.
        public static int AddInt32(ref int addr, int delta)
;

        // AddUint32 atomically adds delta to *addr and returns the new value.
        // To subtract a signed positive constant value c from x, do AddUint32(&x, ^uint32(c-1)).
        // In particular, to decrement x, do AddUint32(&x, ^uint32(0)).
        public static uint AddUint32(ref uint addr, uint delta)
;

        // AddInt64 atomically adds delta to *addr and returns the new value.
        public static long AddInt64(ref long addr, long delta)
;

        // AddUint64 atomically adds delta to *addr and returns the new value.
        // To subtract a signed positive constant value c from x, do AddUint64(&x, ^uint64(c-1)).
        // In particular, to decrement x, do AddUint64(&x, ^uint64(0)).
        public static ulong AddUint64(ref ulong addr, ulong delta)
;

        // AddUintptr atomically adds delta to *addr and returns the new value.
        public static System.UIntPtr AddUintptr(ref System.UIntPtr addr, System.UIntPtr delta)
;

        // LoadInt32 atomically loads *addr.
        public static int LoadInt32(ref int addr)
;

        // LoadInt64 atomically loads *addr.
        public static long LoadInt64(ref long addr)
;

        // LoadUint32 atomically loads *addr.
        public static uint LoadUint32(ref uint addr)
;

        // LoadUint64 atomically loads *addr.
        public static ulong LoadUint64(ref ulong addr)
;

        // LoadUintptr atomically loads *addr.
        public static System.UIntPtr LoadUintptr(ref System.UIntPtr addr)
;

        // LoadPointer atomically loads *addr.
        public static unsafe.Pointer LoadPointer(ref unsafe.Pointer addr)
;

        // StoreInt32 atomically stores val into *addr.
        public static void StoreInt32(ref int addr, int val)
;

        // StoreInt64 atomically stores val into *addr.
        public static void StoreInt64(ref long addr, long val)
;

        // StoreUint32 atomically stores val into *addr.
        public static void StoreUint32(ref uint addr, uint val)
;

        // StoreUint64 atomically stores val into *addr.
        public static void StoreUint64(ref ulong addr, ulong val)
;

        // StoreUintptr atomically stores val into *addr.
        public static void StoreUintptr(ref System.UIntPtr addr, System.UIntPtr val)
;

        // StorePointer atomically stores val into *addr.
        public static void StorePointer(ref unsafe.Pointer addr, unsafe.Pointer val)
;

        // Helper for ARM.  Linker will discard on other systems
        private static void panic64() => func((_, panic, __) =>
        {
            panic("sync/atomic: broken 64-bit atomic operations (buggy QEMU)");
        });
    }
}}
