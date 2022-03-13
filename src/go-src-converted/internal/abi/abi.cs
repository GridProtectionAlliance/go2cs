// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package abi -- go2cs converted at 2022 March 13 05:23:55 UTC
// import "internal/abi" ==> using abi = go.@internal.abi_package
// Original source: C:\Program Files\Go\src\internal\abi\abi.go
namespace go.@internal;

using @unsafe = @unsafe_package;

public static partial class abi_package {

// RegArgs is a struct that has space for each argument
// and return value register on the current architecture.
//
// Assembly code knows the layout of the first two fields
// of RegArgs.
//
// RegArgs also contains additional space to hold pointers
// when it may not be safe to keep them only in the integer
// register space otherwise.
public partial struct RegArgs {
    public array<System.UIntPtr> Ints; // untyped integer registers
    public array<ulong> Floats; // untyped float registers

// Fields above this point are known to assembly.

// Ptrs is a space that duplicates Ints but with pointer type,
// used to make pointers passed or returned  in registers
// visible to the GC by making the type unsafe.Pointer.
    public array<unsafe.Pointer> Ptrs; // ReturnIsPtr is a bitmap that indicates which registers
// contain or will contain pointers on the return path from
// a reflectcall. The i'th bit indicates whether the i'th
// register contains or will contain a valid Go pointer.
    public IntArgRegBitmap ReturnIsPtr;
}

// IntArgRegBitmap is a bitmap large enough to hold one bit per
// integer argument/return register.
public partial struct IntArgRegBitmap { // : array<byte>
}

// Set sets the i'th bit of the bitmap to 1.
private static void Set(this ptr<IntArgRegBitmap> _addr_b, nint i) {
    ref IntArgRegBitmap b = ref _addr_b.val;

    b[i / 8] |= uint8(1) << (int)((i % 8));
}

// Get returns whether the i'th bit of the bitmap is set.
//
// nosplit because it's called in extremely sensitive contexts, like
// on the reflectcall return path.
//
//go:nosplit
private static bool Get(this ptr<IntArgRegBitmap> _addr_b, nint i) {
    ref IntArgRegBitmap b = ref _addr_b.val;

    return b[i / 8] & (uint8(1) << (int)((i % 8))) != 0;
}

// FuncPC* intrinsics.
//
// CAREFUL: In programs with plugins, FuncPC* can return different values
// for the same function (because there are actually multiple copies of
// the same function in the address space). To be safe, don't use the
// results of this function in any == expression. It is only safe to
// use the result as an address at which to start executing code.

// FuncPCABI0 returns the entry PC of the function f, which must be a
// direct reference of a function defined as ABI0. Otherwise it is a
// compile-time error.
//
// Implemented as a compile intrinsic.
public static System.UIntPtr FuncPCABI0(object f);

// FuncPCABIInternal returns the entry PC of the function f. If f is a
// direct reference of a function, it must be defined as ABIInternal.
// Otherwise it is a compile-time error. If f is not a direct reference
// of a defined function, it assumes that f is a func value. Otherwise
// the behavior is undefined.
//
// Implemented as a compile intrinsic.
public static System.UIntPtr FuncPCABIInternal(object f);

} // end abi_package
