// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file provides Go implementations of elementary multi-precision
// arithmetic operations on word vectors. These have the suffix _g.
// These are needed for platforms without assembly implementations of these routines.
// This file also contains elementary operations that can be implemented
// sufficiently efficiently in Go.

// package big -- go2cs converted at 2020 October 09 04:53:15 UTC
// import "math/big" ==> using big = go.math.big_package
// Original source: C:\Go\src\math\big\arith.go
using bits = go.math.bits_package;
using static go.builtin;

namespace go {
namespace math
{
    public static partial class big_package
    {
        // A Word represents a single digit of a multi-precision unsigned integer.
        public partial struct Word // : ulong
        {
        }

        private static readonly var _S = _W / 8L; // word size in bytes

        private static readonly var _W = bits.UintSize; // word size in bits
        private static readonly long _B = (long)1L << (int)(_W); // digit base
        private static readonly var _M = _B - 1L; // digit mask

        // Many of the loops in this file are of the form
        //   for i := 0; i < len(z) && i < len(x) && i < len(y); i++
        // i < len(z) is the real condition.
        // However, checking i < len(x) && i < len(y) as well is faster than
        // having the compiler do a bounds check in the body of the loop;
        // remarkably it is even faster than hoisting the bounds check
        // out of the loop, by doing something like
        //   _, _ = x[len(z)-1], y[len(z)-1]
        // There are other ways to hoist the bounds check out of the loop,
        // but the compiler's BCE isn't powerful enough for them (yet?).
        // See the discussion in CL 164966.

        // ----------------------------------------------------------------------------
        // Elementary operations on words
        //
        // These operations are used by the vector operations below.

        // z1<<_W + z0 = x*y
        private static (Word, Word) mulWW_g(Word x, Word y)
        {
            Word z1 = default;
            Word z0 = default;

            var (hi, lo) = bits.Mul(uint(x), uint(y));
            return (Word(hi), Word(lo));
        }

        // z1<<_W + z0 = x*y + c
        private static (Word, Word) mulAddWWW_g(Word x, Word y, Word c)
        {
            Word z1 = default;
            Word z0 = default;

            var (hi, lo) = bits.Mul(uint(x), uint(y));
            ulong cc = default;
            lo, cc = bits.Add(lo, uint(c), 0L);
            return (Word(hi + cc), Word(lo));
        }

        // nlz returns the number of leading zeros in x.
        // Wraps bits.LeadingZeros call for convenience.
        private static ulong nlz(Word x)
        {
            return uint(bits.LeadingZeros(uint(x)));
        }

        // q = (u1<<_W + u0 - r)/v
        private static (Word, Word) divWW_g(Word u1, Word u0, Word v)
        {
            Word q = default;
            Word r = default;

            var (qq, rr) = bits.Div(uint(u1), uint(u0), uint(v));
            return (Word(qq), Word(rr));
        }

        // The resulting carry c is either 0 or 1.
        private static Word addVV_g(slice<Word> z, slice<Word> x, slice<Word> y)
        {
            Word c = default;
 
            // The comment near the top of this file discusses this for loop condition.
            for (long i = 0L; i < len(z) && i < len(x) && i < len(y); i++)
            {
                var (zi, cc) = bits.Add(uint(x[i]), uint(y[i]), uint(c));
                z[i] = Word(zi);
                c = Word(cc);
            }

            return ;

        }

        // The resulting carry c is either 0 or 1.
        private static Word subVV_g(slice<Word> z, slice<Word> x, slice<Word> y)
        {
            Word c = default;
 
            // The comment near the top of this file discusses this for loop condition.
            for (long i = 0L; i < len(z) && i < len(x) && i < len(y); i++)
            {
                var (zi, cc) = bits.Sub(uint(x[i]), uint(y[i]), uint(c));
                z[i] = Word(zi);
                c = Word(cc);
            }

            return ;

        }

        // The resulting carry c is either 0 or 1.
        private static Word addVW_g(slice<Word> z, slice<Word> x, Word y)
        {
            Word c = default;

            c = y; 
            // The comment near the top of this file discusses this for loop condition.
            for (long i = 0L; i < len(z) && i < len(x); i++)
            {
                var (zi, cc) = bits.Add(uint(x[i]), uint(c), 0L);
                z[i] = Word(zi);
                c = Word(cc);
            }

            return ;

        }

        // addVWlarge is addVW, but intended for large z.
        // The only difference is that we check on every iteration
        // whether we are done with carries,
        // and if so, switch to a much faster copy instead.
        // This is only a good idea for large z,
        // because the overhead of the check and the function call
        // outweigh the benefits when z is small.
        private static Word addVWlarge(slice<Word> z, slice<Word> x, Word y)
        {
            Word c = default;

            c = y; 
            // The comment near the top of this file discusses this for loop condition.
            for (long i = 0L; i < len(z) && i < len(x); i++)
            {
                if (c == 0L)
                {
                    copy(z[i..], x[i..]);
                    return ;
                }

                var (zi, cc) = bits.Add(uint(x[i]), uint(c), 0L);
                z[i] = Word(zi);
                c = Word(cc);

            }

            return ;

        }

        private static Word subVW_g(slice<Word> z, slice<Word> x, Word y)
        {
            Word c = default;

            c = y; 
            // The comment near the top of this file discusses this for loop condition.
            for (long i = 0L; i < len(z) && i < len(x); i++)
            {
                var (zi, cc) = bits.Sub(uint(x[i]), uint(c), 0L);
                z[i] = Word(zi);
                c = Word(cc);
            }

            return ;

        }

        // subVWlarge is to subVW as addVWlarge is to addVW.
        private static Word subVWlarge(slice<Word> z, slice<Word> x, Word y)
        {
            Word c = default;

            c = y; 
            // The comment near the top of this file discusses this for loop condition.
            for (long i = 0L; i < len(z) && i < len(x); i++)
            {
                if (c == 0L)
                {
                    copy(z[i..], x[i..]);
                    return ;
                }

                var (zi, cc) = bits.Sub(uint(x[i]), uint(c), 0L);
                z[i] = Word(zi);
                c = Word(cc);

            }

            return ;

        }

        private static Word shlVU_g(slice<Word> z, slice<Word> x, ulong s)
        {
            Word c = default;

            if (s == 0L)
            {
                copy(z, x);
                return ;
            }

            if (len(z) == 0L)
            {
                return ;
            }

            s &= _W - 1L; // hint to the compiler that shifts by s don't need guard code
            var ŝ = _W - s;
            ŝ &= _W - 1L; // ditto
            c = x[len(z) - 1L] >> (int)(ŝ);
            for (var i = len(z) - 1L; i > 0L; i--)
            {
                z[i] = x[i] << (int)(s) | x[i - 1L] >> (int)(ŝ);
            }

            z[0L] = x[0L] << (int)(s);
            return ;

        }

        private static Word shrVU_g(slice<Word> z, slice<Word> x, ulong s)
        {
            Word c = default;

            if (s == 0L)
            {
                copy(z, x);
                return ;
            }

            if (len(z) == 0L)
            {
                return ;
            }

            s &= _W - 1L; // hint to the compiler that shifts by s don't need guard code
            var ŝ = _W - s;
            ŝ &= _W - 1L; // ditto
            c = x[0L] << (int)(ŝ);
            for (long i = 0L; i < len(z) - 1L; i++)
            {
                z[i] = x[i] >> (int)(s) | x[i + 1L] << (int)(ŝ);
            }

            z[len(z) - 1L] = x[len(z) - 1L] >> (int)(s);
            return ;

        }

        private static Word mulAddVWW_g(slice<Word> z, slice<Word> x, Word y, Word r)
        {
            Word c = default;

            c = r; 
            // The comment near the top of this file discusses this for loop condition.
            for (long i = 0L; i < len(z) && i < len(x); i++)
            {
                c, z[i] = mulAddWWW_g(x[i], y, c);
            }

            return ;

        }

        private static Word addMulVVW_g(slice<Word> z, slice<Word> x, Word y)
        {
            Word c = default;
 
            // The comment near the top of this file discusses this for loop condition.
            for (long i = 0L; i < len(z) && i < len(x); i++)
            {
                var (z1, z0) = mulAddWWW_g(x[i], y, z[i]);
                var (lo, cc) = bits.Add(uint(z0), uint(c), 0L);
                c = Word(cc);
                z[i] = Word(lo);
                c += z1;

            }

            return ;

        }

        private static Word divWVW_g(slice<Word> z, Word xn, slice<Word> x, Word y)
        {
            Word r = default;

            r = xn;
            for (var i = len(z) - 1L; i >= 0L; i--)
            {
                z[i], r = divWW_g(r, x[i], y);
            }

            return ;

        }
    }
}}
