// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !compiler_bootstrap

// package bits -- go2cs converted at 2020 October 08 03:25:15 UTC
// import "math/bits" ==> using bits = go.math.bits_package
// Original source: C:\Go\src\math\bits\bits_errors.go
using _@unsafe_ = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace math
{
    public static partial class bits_package
    {
        //go:linkname overflowError runtime.overflowError
        private static error overflowError = default!;

        //go:linkname divideError runtime.divideError
        private static error divideError = default!;
    }
}}
