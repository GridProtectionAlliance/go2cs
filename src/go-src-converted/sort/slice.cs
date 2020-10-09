// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sort -- go2cs converted at 2020 October 09 04:49:14 UTC
// import "sort" ==> using sort = go.sort_package
// Original source: C:\Go\src\sort\slice.go

using static go.builtin;
using System;

namespace go
{
    public static partial class sort_package
    {
        // Slice sorts the provided slice given the provided less function.
        //
        // The sort is not guaranteed to be stable. For a stable sort, use
        // SliceStable.
        //
        // The function panics if the provided interface is not a slice.
        public static bool Slice(object slice, Func<long, long, bool> less)
        {
            var rv = reflectValueOf(slice);
            var swap = reflectSwapper(slice);
            var length = rv.Len();
            quickSort_func(new lessSwap(less,swap), 0L, length, maxDepth(length));
        }

        // SliceStable sorts the provided slice given the provided less
        // function while keeping the original order of equal elements.
        //
        // The function panics if the provided interface is not a slice.
        public static bool SliceStable(object slice, Func<long, long, bool> less)
        {
            var rv = reflectValueOf(slice);
            var swap = reflectSwapper(slice);
            stable_func(new lessSwap(less,swap), rv.Len());
        }

        // SliceIsSorted tests whether a slice is sorted.
        //
        // The function panics if the provided interface is not a slice.
        public static bool SliceIsSorted(object slice, Func<long, long, bool> less)
        {
            var rv = reflectValueOf(slice);
            var n = rv.Len();
            for (var i = n - 1L; i > 0L; i--)
            {
                if (less(i, i - 1L))
                {
                    return false;
                }

            }

            return true;

        }
    }
}
