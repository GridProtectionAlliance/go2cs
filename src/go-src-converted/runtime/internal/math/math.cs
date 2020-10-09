// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2020 October 09 04:45:39 UTC
// import "runtime/internal/math" ==> using math = go.runtime.@internal.math_package
// Original source: C:\Go\src\runtime\internal\math\math.go
using sys = go.runtime.@internal.sys_package;
using static go.builtin;

namespace go {
namespace runtime {
namespace @internal
{
    public static partial class math_package
    {
        public static readonly var MaxUintptr = ~uintptr(0L);

        // MulUintptr returns a * b and whether the multiplication overflowed.
        // On supported platforms this is an intrinsic lowered by the compiler.


        // MulUintptr returns a * b and whether the multiplication overflowed.
        // On supported platforms this is an intrinsic lowered by the compiler.
        public static (System.UIntPtr, bool) MulUintptr(System.UIntPtr a, System.UIntPtr b)
        {
            System.UIntPtr _p0 = default;
            bool _p0 = default;

            if (a | b < 1L << (int)((4L * sys.PtrSize)) || a == 0L)
            {
                return (a * b, false);
            }

            var overflow = b > MaxUintptr / a;
            return (a * b, overflow);

        }
    }
}}}
