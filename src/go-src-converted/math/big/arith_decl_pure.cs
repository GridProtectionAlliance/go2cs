// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build math_big_pure_go

// package big -- go2cs converted at 2020 August 29 08:28:58 UTC
// import "math/big" ==> using big = go.math.big_package
// Original source: C:\Go\src\math\big\arith_decl_pure.go

using static go.builtin;

namespace go {
namespace math
{
    public static partial class big_package
    {
        private static (Word, Word) mulWW(Word x, Word y)
        {
            return mulWW_g(x, y);
        }

        private static (Word, Word) divWW(Word x1, Word x0, Word y)
        {
            return divWW_g(x1, x0, y);
        }

        private static Word addVV(slice<Word> z, slice<Word> x, slice<Word> y)
        {
            return addVV_g(z, x, y);
        }

        private static Word subVV(slice<Word> z, slice<Word> x, slice<Word> y)
        {
            return subVV_g(z, x, y);
        }

        private static Word addVW(slice<Word> z, slice<Word> x, Word y)
        {
            return addVW_g(z, x, y);
        }

        private static Word subVW(slice<Word> z, slice<Word> x, Word y)
        {
            return subVW_g(z, x, y);
        }

        private static Word shlVU(slice<Word> z, slice<Word> x, ulong s)
        {
            return shlVU_g(z, x, s);
        }

        private static Word shrVU(slice<Word> z, slice<Word> x, ulong s)
        {
            return shrVU_g(z, x, s);
        }

        private static Word mulAddVWW(slice<Word> z, slice<Word> x, Word y, Word r)
        {
            return mulAddVWW_g(z, x, y, r);
        }

        private static Word addMulVVW(slice<Word> z, slice<Word> x, Word y)
        {
            return addMulVVW_g(z, x, y);
        }

        private static Word divWVW(slice<Word> z, Word xn, slice<Word> x, Word y)
        {
            return divWVW_g(z, xn, x, y);
        }
    }
}}
