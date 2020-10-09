// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build math_big_pure_go

// package big -- go2cs converted at 2020 October 09 04:53:16 UTC
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
            Word z1 = default;
            Word z0 = default;

            return mulWW_g(x, y);
        }

        private static (Word, Word) divWW(Word x1, Word x0, Word y)
        {
            Word q = default;
            Word r = default;

            return divWW_g(x1, x0, y);
        }

        private static Word addVV(slice<Word> z, slice<Word> x, slice<Word> y)
        {
            Word c = default;

            return addVV_g(z, x, y);
        }

        private static Word subVV(slice<Word> z, slice<Word> x, slice<Word> y)
        {
            Word c = default;

            return subVV_g(z, x, y);
        }

        private static Word addVW(slice<Word> z, slice<Word> x, Word y)
        {
            Word c = default;
 
            // TODO: remove indirect function call when golang.org/issue/30548 is fixed
            var fn = addVW_g;
            if (len(z) > 32L)
            {
                fn = addVWlarge;
            }

            return fn(z, x, y);

        }

        private static Word subVW(slice<Word> z, slice<Word> x, Word y)
        {
            Word c = default;
 
            // TODO: remove indirect function call when golang.org/issue/30548 is fixed
            var fn = subVW_g;
            if (len(z) > 32L)
            {
                fn = subVWlarge;
            }

            return fn(z, x, y);

        }

        private static Word shlVU(slice<Word> z, slice<Word> x, ulong s)
        {
            Word c = default;

            return shlVU_g(z, x, s);
        }

        private static Word shrVU(slice<Word> z, slice<Word> x, ulong s)
        {
            Word c = default;

            return shrVU_g(z, x, s);
        }

        private static Word mulAddVWW(slice<Word> z, slice<Word> x, Word y, Word r)
        {
            Word c = default;

            return mulAddVWW_g(z, x, y, r);
        }

        private static Word addMulVVW(slice<Word> z, slice<Word> x, Word y)
        {
            Word c = default;

            return addMulVVW_g(z, x, y);
        }

        private static Word divWVW(slice<Word> z, Word xn, slice<Word> x, Word y)
        {
            Word r = default;

            return divWVW_g(z, xn, x, y);
        }
    }
}}
