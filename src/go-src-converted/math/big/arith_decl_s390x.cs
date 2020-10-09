// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !math_big_pure_go

// package big -- go2cs converted at 2020 October 09 04:53:16 UTC
// import "math/big" ==> using big = go.math.big_package
// Original source: C:\Go\src\math\big\arith_decl_s390x.go
using cpu = go.@internal.cpu_package;
using static go.builtin;

namespace go {
namespace math
{
    public static partial class big_package
    {
        private static Word addVV_check(slice<Word> z, slice<Word> x, slice<Word> y)
;
        private static Word addVV_vec(slice<Word> z, slice<Word> x, slice<Word> y)
;
        private static Word addVV_novec(slice<Word> z, slice<Word> x, slice<Word> y)
;
        private static Word subVV_check(slice<Word> z, slice<Word> x, slice<Word> y)
;
        private static Word subVV_vec(slice<Word> z, slice<Word> x, slice<Word> y)
;
        private static Word subVV_novec(slice<Word> z, slice<Word> x, slice<Word> y)
;

        private static var hasVX = cpu.S390X.HasVX;
    }
}}
