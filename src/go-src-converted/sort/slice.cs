// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sort -- go2cs converted at 2022 March 06 22:12:31 UTC
// import "sort" ==> using sort = go.sort_package
// Original source: C:\Program Files\Go\src\sort\slice.go

using System;


namespace go;

public static partial class sort_package {

    // Slice sorts the slice x given the provided less function.
    // It panics if x is not a slice.
    //
    // The sort is not guaranteed to be stable: equal elements
    // may be reversed from their original order.
    // For a stable sort, use SliceStable.
    //
    // The less function must satisfy the same requirements as
    // the Interface type's Less method.
public static bool Slice(object x, Func<nint, nint, bool> less) {
    var rv = reflectValueOf(x);
    var swap = reflectSwapper(x);
    var length = rv.Len();
    quickSort_func(new lessSwap(less,swap), 0, length, maxDepth(length));
}

// SliceStable sorts the slice x using the provided less
// function, keeping equal elements in their original order.
// It panics if x is not a slice.
//
// The less function must satisfy the same requirements as
// the Interface type's Less method.
public static bool SliceStable(object x, Func<nint, nint, bool> less) {
    var rv = reflectValueOf(x);
    var swap = reflectSwapper(x);
    stable_func(new lessSwap(less,swap), rv.Len());
}

// SliceIsSorted reports whether the slice x is sorted according to the provided less function.
// It panics if x is not a slice.
public static bool SliceIsSorted(object x, Func<nint, nint, bool> less) {
    var rv = reflectValueOf(x);
    var n = rv.Len();
    for (var i = n - 1; i > 0; i--) {
        if (less(i, i - 1)) {
            return false;
        }
    }
    return true;

}

} // end sort_package
