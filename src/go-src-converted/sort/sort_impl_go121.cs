// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build go1.21
// Starting with Go 1.21, we can leverage the new generic functions from the
// slices package to implement some `sort` functions faster. However, until
// the bootstrap compiler uses Go 1.21 or later, we keep a fallback version
// in sort_impl_120.go that retains the old implementation.
namespace go;

using slices = slices_package;

partial class sort_package {

internal static void intsImpl(slice<nint> x) {
    slices.Sort<slice<nint>, nint>(x);
}

internal static void float64sImpl(slice<float64> x) {
    slices.Sort<slice<float64>, float64>(x);
}

internal static void stringsImpl(slice<@string> x) {
    slices.Sort<slice<@string>, @string>(x);
}

internal static bool intsAreSortedImpl(slice<nint> x) {
    return slices.IsSorted<slice<nint>, nint>(x);
}

internal static bool float64sAreSortedImpl(slice<float64> x) {
    return slices.IsSorted<slice<float64>, float64>(x);
}

internal static bool stringsAreSortedImpl(slice<@string> x) {
    return slices.IsSorted<slice<@string>, @string>(x);
}

} // end sort_package
