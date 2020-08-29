// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !math_big_pure_go

// package big -- go2cs converted at 2020 August 29 08:28:58 UTC
// import "math/big" ==> using big = go.math.big_package
// Original source: C:\Go\src\math\big\arith_decl_s390x.go

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
        private static Word addVW_check(slice<Word> z, slice<Word> x, Word y)
;
        private static Word addVW_vec(slice<Word> z, slice<Word> x, Word y)
;
        private static Word addVW_novec(slice<Word> z, slice<Word> x, Word y)
;
        private static Word subVW_check(slice<Word> z, slice<Word> x, Word y)
;
        private static Word subVW_vec(slice<Word> z, slice<Word> x, Word y)
;
        private static Word subVW_novec(slice<Word> z, slice<Word> x, Word y)
;
        private static bool hasVectorFacility()
;

        private static var hasVX = hasVectorFacility();
    }
}}
