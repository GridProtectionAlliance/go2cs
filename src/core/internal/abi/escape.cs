// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using @unsafe = unsafe_package;

partial class abi_package {

// NoEscape hides the pointer p from escape analysis, preventing it
// from escaping to the heap. It compiles down to nothing.
//
// WARNING: This is very subtle to use correctly. The caller must
// ensure that it's truly safe for p to not escape to the heap by
// maintaining runtime pointer invariants (for example, that globals
// and the heap may not generally point into a stack).
//
//go:nosplit
//go:nocheckptr
public static @unsafe.Pointer NoEscape(@unsafe.Pointer p) {
    var x = ((uintptr)p);
    return ((@unsafe.Pointer)(x ^ 0));
}

internal static bool alwaysFalse;

internal static any escapeSink;

// Escape forces any pointers in x to escape to the heap.
public static T Escape<T>(T x)
    where T : new()
{
    if (alwaysFalse) {
        escapeSink = x;
    }
    return x;
}

} // end abi_package
