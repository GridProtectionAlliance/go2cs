// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package atomic provides low-level atomic memory primitives
// useful for implementing synchronization algorithms.
//
// These functions require great care to be used correctly.
// Except for special, low-level applications, synchronization is better
// done with channels or the facilities of the [sync] package.
// Share memory by communicating;
// don't communicate by sharing memory.
//
// The swap operation, implemented by the SwapT functions, is the atomic
// equivalent of:
//
//	old = *addr
//	*addr = new
//	return old
//
// The compare-and-swap operation, implemented by the CompareAndSwapT
// functions, is the atomic equivalent of:
//
//	if *addr == old {
//		*addr = new
//		return true
//	}
//	return false
//
// The add operation, implemented by the AddT functions, is the atomic
// equivalent of:
//
//	*addr += delta
//	return *addr
//
// The load and store operations, implemented by the LoadT and StoreT
// functions, are the atomic equivalents of "return *addr" and
// "*addr = val".
//
// In the terminology of [the Go memory model], if the effect of
// an atomic operation A is observed by atomic operation B,
// then A “synchronizes before” B.
// Additionally, all the atomic operations executed in a program
// behave as though executed in some sequentially consistent order.
// This definition provides the same semantics as
// C++'s sequentially consistent atomics and Java's volatile variables.
//
// [the Go memory model]: https://go.dev/ref/mem
namespace go.sync;

using @unsafe = unsafe_package;

partial class atomic_package {

// BUG(rsc): On 386, the 64-bit functions use instructions unavailable before the Pentium MMX.
//
// On non-Linux ARM, the 64-bit functions use instructions unavailable before the ARMv6k core.
//
// On ARM, 386, and 32-bit MIPS, it is the caller's responsibility to arrange
// for 64-bit alignment of 64-bit words accessed atomically via the primitive
// atomic functions (types [Int64] and [Uint64] are automatically aligned).
// The first word in an allocated struct, array, or slice; in a global
// variable; or in a local variable (because the subject of all atomic operations
// will escape to the heap) can be relied upon to be 64-bit aligned.

// SwapInt32 atomically stores new into *addr and returns the previous *addr value.
// Consider using the more ergonomic and less error-prone [Int32.Swap] instead.
public static partial int32 /*old*/ SwapInt32(ж<int32> addr, int32 @new);

// SwapInt64 atomically stores new into *addr and returns the previous *addr value.
// Consider using the more ergonomic and less error-prone [Int64.Swap] instead
// (particularly if you target 32-bit platforms; see the bugs section).
public static partial int64 /*old*/ SwapInt64(ж<int64> addr, int64 @new);

// SwapUint32 atomically stores new into *addr and returns the previous *addr value.
// Consider using the more ergonomic and less error-prone [Uint32.Swap] instead.
public static partial uint32 /*old*/ SwapUint32(ж<uint32> addr, uint32 @new);

// SwapUint64 atomically stores new into *addr and returns the previous *addr value.
// Consider using the more ergonomic and less error-prone [Uint64.Swap] instead
// (particularly if you target 32-bit platforms; see the bugs section).
public static partial uint64 /*old*/ SwapUint64(ж<uint64> addr, uint64 @new);

// SwapUintptr atomically stores new into *addr and returns the previous *addr value.
// Consider using the more ergonomic and less error-prone [Uintptr.Swap] instead.
public static partial uintptr /*old*/ SwapUintptr(ж<uintptr> addr, uintptr @new);

// SwapPointer atomically stores new into *addr and returns the previous *addr value.
// Consider using the more ergonomic and less error-prone [Pointer.Swap] instead.
public static partial @unsafe.Pointer /*old*/ SwapPointer(ж<@unsafe.Pointer> addr, @unsafe.Pointer @new);

// CompareAndSwapInt32 executes the compare-and-swap operation for an int32 value.
// Consider using the more ergonomic and less error-prone [Int32.CompareAndSwap] instead.
public static partial bool /*swapped*/ CompareAndSwapInt32(ж<int32> addr, int32 old, int32 @new);

// CompareAndSwapInt64 executes the compare-and-swap operation for an int64 value.
// Consider using the more ergonomic and less error-prone [Int64.CompareAndSwap] instead
// (particularly if you target 32-bit platforms; see the bugs section).
public static partial bool /*swapped*/ CompareAndSwapInt64(ж<int64> addr, int64 old, int64 @new);

// CompareAndSwapUint32 executes the compare-and-swap operation for a uint32 value.
// Consider using the more ergonomic and less error-prone [Uint32.CompareAndSwap] instead.
public static partial bool /*swapped*/ CompareAndSwapUint32(ж<uint32> addr, uint32 old, uint32 @new);

// CompareAndSwapUint64 executes the compare-and-swap operation for a uint64 value.
// Consider using the more ergonomic and less error-prone [Uint64.CompareAndSwap] instead
// (particularly if you target 32-bit platforms; see the bugs section).
public static partial bool /*swapped*/ CompareAndSwapUint64(ж<uint64> addr, uint64 old, uint64 @new);

// CompareAndSwapUintptr executes the compare-and-swap operation for a uintptr value.
// Consider using the more ergonomic and less error-prone [Uintptr.CompareAndSwap] instead.
public static partial bool /*swapped*/ CompareAndSwapUintptr(ж<uintptr> addr, uintptr old, uintptr @new);

// CompareAndSwapPointer executes the compare-and-swap operation for a unsafe.Pointer value.
// Consider using the more ergonomic and less error-prone [Pointer.CompareAndSwap] instead.
public static partial bool /*swapped*/ CompareAndSwapPointer(ж<@unsafe.Pointer> addr, @unsafe.Pointer old, @unsafe.Pointer @new);

// AddInt32 atomically adds delta to *addr and returns the new value.
// Consider using the more ergonomic and less error-prone [Int32.Add] instead.
public static partial int32 /*new*/ AddInt32(ж<int32> addr, int32 delta);

// AddUint32 atomically adds delta to *addr and returns the new value.
// To subtract a signed positive constant value c from x, do AddUint32(&x, ^uint32(c-1)).
// In particular, to decrement x, do AddUint32(&x, ^uint32(0)).
// Consider using the more ergonomic and less error-prone [Uint32.Add] instead.
public static partial uint32 /*new*/ AddUint32(ж<uint32> addr, uint32 delta);

// AddInt64 atomically adds delta to *addr and returns the new value.
// Consider using the more ergonomic and less error-prone [Int64.Add] instead
// (particularly if you target 32-bit platforms; see the bugs section).
public static partial int64 /*new*/ AddInt64(ж<int64> addr, int64 delta);

// AddUint64 atomically adds delta to *addr and returns the new value.
// To subtract a signed positive constant value c from x, do AddUint64(&x, ^uint64(c-1)).
// In particular, to decrement x, do AddUint64(&x, ^uint64(0)).
// Consider using the more ergonomic and less error-prone [Uint64.Add] instead
// (particularly if you target 32-bit platforms; see the bugs section).
public static partial uint64 /*new*/ AddUint64(ж<uint64> addr, uint64 delta);

// AddUintptr atomically adds delta to *addr and returns the new value.
// Consider using the more ergonomic and less error-prone [Uintptr.Add] instead.
public static partial uintptr /*new*/ AddUintptr(ж<uintptr> addr, uintptr delta);

// AndInt32 atomically performs a bitwise AND operation on *addr using the bitmask provided as mask
// and returns the old value.
// Consider using the more ergonomic and less error-prone [Int32.And] instead.
public static partial int32 /*old*/ AndInt32(ж<int32> addr, int32 mask);

// AndUint32 atomically performs a bitwise AND operation on *addr using the bitmask provided as mask
// and returns the old value.
// Consider using the more ergonomic and less error-prone [Uint32.And] instead.
public static partial uint32 /*old*/ AndUint32(ж<uint32> addr, uint32 mask);

// AndInt64 atomically performs a bitwise AND operation on *addr using the bitmask provided as mask
// and returns the old value.
// Consider using the more ergonomic and less error-prone [Int64.And] instead.
public static partial int64 /*old*/ AndInt64(ж<int64> addr, int64 mask);

// AndUint64 atomically performs a bitwise AND operation on *addr using the bitmask provided as mask
// and returns the old.
// Consider using the more ergonomic and less error-prone [Uint64.And] instead.
public static partial uint64 /*old*/ AndUint64(ж<uint64> addr, uint64 mask);

// AndUintptr atomically performs a bitwise AND operation on *addr using the bitmask provided as mask
// and returns the old value.
// Consider using the more ergonomic and less error-prone [Uintptr.And] instead.
public static partial uintptr /*old*/ AndUintptr(ж<uintptr> addr, uintptr mask);

// OrInt32 atomically performs a bitwise OR operation on *addr using the bitmask provided as mask
// and returns the old value.
// Consider using the more ergonomic and less error-prone [Int32.Or] instead.
public static partial int32 /*old*/ OrInt32(ж<int32> addr, int32 mask);

// OrUint32 atomically performs a bitwise OR operation on *addr using the bitmask provided as mask
// and returns the old value.
// Consider using the more ergonomic and less error-prone [Uint32.Or] instead.
public static partial uint32 /*old*/ OrUint32(ж<uint32> addr, uint32 mask);

// OrInt64 atomically performs a bitwise OR operation on *addr using the bitmask provided as mask
// and returns the old value.
// Consider using the more ergonomic and less error-prone [Int64.Or] instead.
public static partial int64 /*old*/ OrInt64(ж<int64> addr, int64 mask);

// OrUint64 atomically performs a bitwise OR operation on *addr using the bitmask provided as mask
// and returns the old value.
// Consider using the more ergonomic and less error-prone [Uint64.Or] instead.
public static partial uint64 /*old*/ OrUint64(ж<uint64> addr, uint64 mask);

// OrUintptr atomically performs a bitwise OR operation on *addr using the bitmask provided as mask
// and returns the old value.
// Consider using the more ergonomic and less error-prone [Uintptr.Or] instead.
public static partial uintptr /*old*/ OrUintptr(ж<uintptr> addr, uintptr mask);

// LoadInt32 atomically loads *addr.
// Consider using the more ergonomic and less error-prone [Int32.Load] instead.
public static partial int32 /*val*/ LoadInt32(ж<int32> addr);

// LoadInt64 atomically loads *addr.
// Consider using the more ergonomic and less error-prone [Int64.Load] instead
// (particularly if you target 32-bit platforms; see the bugs section).
public static partial int64 /*val*/ LoadInt64(ж<int64> addr);

// LoadUint32 atomically loads *addr.
// Consider using the more ergonomic and less error-prone [Uint32.Load] instead.
public static partial uint32 /*val*/ LoadUint32(ж<uint32> addr);

// LoadUint64 atomically loads *addr.
// Consider using the more ergonomic and less error-prone [Uint64.Load] instead
// (particularly if you target 32-bit platforms; see the bugs section).
public static partial uint64 /*val*/ LoadUint64(ж<uint64> addr);

// LoadUintptr atomically loads *addr.
// Consider using the more ergonomic and less error-prone [Uintptr.Load] instead.
public static partial uintptr /*val*/ LoadUintptr(ж<uintptr> addr);

// LoadPointer atomically loads *addr.
// Consider using the more ergonomic and less error-prone [Pointer.Load] instead.
public static partial @unsafe.Pointer /*val*/ LoadPointer(ж<@unsafe.Pointer> addr);

// StoreInt32 atomically stores val into *addr.
// Consider using the more ergonomic and less error-prone [Int32.Store] instead.
public static partial void StoreInt32(ж<int32> addr, int32 val);

// StoreInt64 atomically stores val into *addr.
// Consider using the more ergonomic and less error-prone [Int64.Store] instead
// (particularly if you target 32-bit platforms; see the bugs section).
public static partial void StoreInt64(ж<int64> addr, int64 val);

// StoreUint32 atomically stores val into *addr.
// Consider using the more ergonomic and less error-prone [Uint32.Store] instead.
public static partial void StoreUint32(ж<uint32> addr, uint32 val);

// StoreUint64 atomically stores val into *addr.
// Consider using the more ergonomic and less error-prone [Uint64.Store] instead
// (particularly if you target 32-bit platforms; see the bugs section).
public static partial void StoreUint64(ж<uint64> addr, uint64 val);

// StoreUintptr atomically stores val into *addr.
// Consider using the more ergonomic and less error-prone [Uintptr.Store] instead.
public static partial void StoreUintptr(ж<uintptr> addr, uintptr val);

// StorePointer atomically stores val into *addr.
// Consider using the more ergonomic and less error-prone [Pointer.Store] instead.
public static partial void StorePointer(ж<@unsafe.Pointer> addr, @unsafe.Pointer val);

} // end atomic_package
