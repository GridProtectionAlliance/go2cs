// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using reflectlite = @internal.reflectlite_package;
using bits = math.bits_package;
using @internal;
using math;

partial class sort_package {

// Slice sorts the slice x given the provided less function.
// It panics if x is not a slice.
//
// The sort is not guaranteed to be stable: equal elements
// may be reversed from their original order.
// For a stable sort, use [SliceStable].
//
// The less function must satisfy the same requirements as
// the Interface type's Less method.
//
// Note: in many situations, the newer [slices.SortFunc] function is more
// ergonomic and runs faster.
public static void Slice(any x, Func<nint, nint, bool> less) {
    var rv = reflectlite.ValueOf(x);
    var swap = reflectlite.Swapper(x);
    nint length = rv.Len();
    nint limit = bits.Len(((nuint)length));
    pdqsort_func(new lessSwap(less, swap), 0, length, limit);
}

// SliceStable sorts the slice x using the provided less
// function, keeping equal elements in their original order.
// It panics if x is not a slice.
//
// The less function must satisfy the same requirements as
// the Interface type's Less method.
//
// Note: in many situations, the newer [slices.SortStableFunc] function is more
// ergonomic and runs faster.
public static void SliceStable(any x, Func<nint, nint, bool> less) {
    var rv = reflectlite.ValueOf(x);
    var swap = reflectlite.Swapper(x);
    stable_func(new lessSwap(less, swap), rv.Len());
}

// SliceIsSorted reports whether the slice x is sorted according to the provided less function.
// It panics if x is not a slice.
//
// Note: in many situations, the newer [slices.IsSortedFunc] function is more
// ergonomic and runs faster.
public static bool SliceIsSorted(any x, Func<nint, nint, bool> less) {
    var rv = reflectlite.ValueOf(x);
    nint n = rv.Len();
    for (nint i = n - 1; i > 0; i--) {
        if (less(i, i - 1)) {
            return false;
        }
    }
    return true;
}

} // end sort_package
