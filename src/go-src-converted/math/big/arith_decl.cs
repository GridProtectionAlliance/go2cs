// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !math_big_pure_go

// package big -- go2cs converted at 2020 October 08 03:25:23 UTC
// import "math/big" ==> using big = go.math.big_package
// Original source: C:\Go\src\math\big\arith_decl.go

using static go.builtin;

namespace go {
namespace math
{
    public static partial class big_package
    {
        // implemented in arith_$GOARCH.s
        private static (Word, Word) mulWW(Word x, Word y)
;
        private static (Word, Word) divWW(Word x1, Word x0, Word y)
;
        private static Word addVV(slice<Word> z, slice<Word> x, slice<Word> y)
;
        private static Word subVV(slice<Word> z, slice<Word> x, slice<Word> y)
;
        private static Word addVW(slice<Word> z, slice<Word> x, Word y)
;
        private static Word subVW(slice<Word> z, slice<Word> x, Word y)
;
        private static Word shlVU(slice<Word> z, slice<Word> x, ulong s)
;
        private static Word shrVU(slice<Word> z, slice<Word> x, ulong s)
;
        private static Word mulAddVWW(slice<Word> z, slice<Word> x, Word y, Word r)
;
        private static Word addMulVVW(slice<Word> z, slice<Word> x, Word y)
;
        private static Word divWVW(slice<Word> z, Word xn, slice<Word> x, Word y)
;
    }
}}
