// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !math_big_pure_go

// package big -- go2cs converted at 2020 October 08 03:25:23 UTC
// import "math/big" ==> using big = go.math.big_package
// Original source: C:\Go\src\math\big\arith_amd64.go
using cpu = go.@internal.cpu_package;
using static go.builtin;

namespace go {
namespace math
{
    public static partial class big_package
    {
        private static var support_adx = cpu.X86.HasADX && cpu.X86.HasBMI2;
    }
}}
