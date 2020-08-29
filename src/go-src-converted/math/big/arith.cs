// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file provides Go implementations of elementary multi-precision
// arithmetic operations on word vectors. Needed for platforms without
// assembly implementations of these routines.

// package big -- go2cs converted at 2020 August 29 08:28:58 UTC
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
        private static readonly long _B = 1L << (int)(_W); // digit base
        private static readonly var _M = _B - 1L; // digit mask

        private static readonly var _W2 = _W / 2L; // half word size in bits
        private static readonly long _B2 = 1L << (int)(_W2); // half digit base
        private static readonly var _M2 = _B2 - 1L; // half digit mask

        // ----------------------------------------------------------------------------
        // Elementary operations on words
        //
        // These operations are used by the vector operations below.

        // z1<<_W + z0 = x+y+c, with c == 0 or 1
        private static (Word, Word) addWW_g(Word x, Word y, Word c)
        {
            var yc = y + c;
            z0 = x + yc;
            if (z0 < x || yc < y)
            {
                z1 = 1L;
            }
            return;
        }

        // z1<<_W + z0 = x-y-c, with c == 0 or 1
        private static (Word, Word) subWW_g(Word x, Word y, Word c)
        {
            var yc = y + c;
            z0 = x - yc;
            if (z0 > x || yc < y)
            {
                z1 = 1L;
            }
            return;
        }

        // z1<<_W + z0 = x*y
        // Adapted from Warren, Hacker's Delight, p. 132.
        private static (Word, Word) mulWW_g(Word x, Word y)
        {
            var x0 = x & _M2;
            var x1 = x >> (int)(_W2);
            var y0 = y & _M2;
            var y1 = y >> (int)(_W2);
            var w0 = x0 * y0;
            var t = x1 * y0 + w0 >> (int)(_W2);
            var w1 = t & _M2;
            var w2 = t >> (int)(_W2);
            w1 += x0 * y1;
            z1 = x1 * y1 + w2 + w1 >> (int)(_W2);
            z0 = x * y;
            return;
        }

        // z1<<_W + z0 = x*y + c
        private static (Word, Word) mulAddWWW_g(Word x, Word y, Word c)
        {
            var (z1, zz0) = mulWW_g(x, y);
            z0 = zz0 + c;

            if (z0 < zz0)
            {
                z1++;
            }
            return;
        }

        // nlz returns the number of leading zeros in x.
        // Wraps bits.LeadingZeros call for convenience.
        private static ulong nlz(Word x)
        {
            return uint(bits.LeadingZeros(uint(x)));
        }

        // q = (u1<<_W + u0 - r)/y
        // Adapted from Warren, Hacker's Delight, p. 152.
        private static (Word, Word) divWW_g(Word u1, Word u0, Word v)
        {
            if (u1 >= v)
            {
                return (1L << (int)(_W) - 1L, 1L << (int)(_W) - 1L);
            }
            var s = nlz(v);
            v <<= s;

            var vn1 = v >> (int)(_W2);
            var vn0 = v & _M2;
            var un32 = u1 << (int)(s) | u0 >> (int)((_W - s));
            var un10 = u0 << (int)(s);
            var un1 = un10 >> (int)(_W2);
            var un0 = un10 & _M2;
            var q1 = un32 / vn1;
            var rhat = un32 - q1 * vn1;

            while (q1 >= _B2 || q1 * vn0 > _B2 * rhat + un1)
            {
                q1--;
                rhat += vn1;
                if (rhat >= _B2)
                {
                    break;
                }
            }


            var un21 = un32 * _B2 + un1 - q1 * v;
            var q0 = un21 / vn1;
            rhat = un21 - q0 * vn1;

            while (q0 >= _B2 || q0 * vn0 > _B2 * rhat + un0)
            {
                q0--;
                rhat += vn1;
                if (rhat >= _B2)
                {
                    break;
                }
            }


            return (q1 * _B2 + q0, (un21 * _B2 + un0 - q0 * v) >> (int)(s));
        }

        // Keep for performance debugging.
        // Using addWW_g is likely slower.
        private static readonly var use_addWW_g = false;

        // The resulting carry c is either 0 or 1.


        // The resulting carry c is either 0 or 1.
        private static Word addVV_g(slice<Word> z, slice<Word> x, slice<Word> y)
        {
            if (use_addWW_g)
            {
                {
                    var i__prev1 = i;

                    foreach (var (__i) in z)
                    {
                        i = __i;
                        c, z[i] = addWW_g(x[i], y[i], c);
                    }

                    i = i__prev1;
                }

                return;
            }
            {
                var i__prev1 = i;

                foreach (var (__i, __xi) in x[..len(z)])
                {
                    i = __i;
                    xi = __xi;
                    var yi = y[i];
                    var zi = xi + yi + c;
                    z[i] = zi; 
                    // see "Hacker's Delight", section 2-12 (overflow detection)
                    c = (xi & yi | (xi | yi) & ~zi) >> (int)((_W - 1L));
                }

                i = i__prev1;
            }

            return;
        }

        // The resulting carry c is either 0 or 1.
        private static Word subVV_g(slice<Word> z, slice<Word> x, slice<Word> y)
        {
            if (use_addWW_g)
            {
                {
                    var i__prev1 = i;

                    foreach (var (__i) in z)
                    {
                        i = __i;
                        c, z[i] = subWW_g(x[i], y[i], c);
                    }

                    i = i__prev1;
                }

                return;
            }
            {
                var i__prev1 = i;

                foreach (var (__i, __xi) in x[..len(z)])
                {
                    i = __i;
                    xi = __xi;
                    var yi = y[i];
                    var zi = xi - yi - c;
                    z[i] = zi; 
                    // see "Hacker's Delight", section 2-12 (overflow detection)
                    c = (yi & ~xi | (yi | ~xi) & zi) >> (int)((_W - 1L));
                }

                i = i__prev1;
            }

            return;
        }

        // The resulting carry c is either 0 or 1.
        private static Word addVW_g(slice<Word> z, slice<Word> x, Word y)
        {
            if (use_addWW_g)
            {
                c = y;
                {
                    var i__prev1 = i;

                    foreach (var (__i) in z)
                    {
                        i = __i;
                        c, z[i] = addWW_g(x[i], c, 0L);
                    }

                    i = i__prev1;
                }

                return;
            }
            c = y;
            {
                var i__prev1 = i;

                foreach (var (__i, __xi) in x[..len(z)])
                {
                    i = __i;
                    xi = __xi;
                    var zi = xi + c;
                    z[i] = zi;
                    c = xi & ~zi >> (int)((_W - 1L));
                }

                i = i__prev1;
            }

            return;
        }

        private static Word subVW_g(slice<Word> z, slice<Word> x, Word y)
        {
            if (use_addWW_g)
            {
                c = y;
                {
                    var i__prev1 = i;

                    foreach (var (__i) in z)
                    {
                        i = __i;
                        c, z[i] = subWW_g(x[i], c, 0L);
                    }

                    i = i__prev1;
                }

                return;
            }
            c = y;
            {
                var i__prev1 = i;

                foreach (var (__i, __xi) in x[..len(z)])
                {
                    i = __i;
                    xi = __xi;
                    var zi = xi - c;
                    z[i] = zi;
                    c = (zi & ~xi) >> (int)((_W - 1L));
                }

                i = i__prev1;
            }

            return;
        }

        private static Word shlVU_g(slice<Word> z, slice<Word> x, ulong s)
        {
            {
                var n = len(z);

                if (n > 0L)
                {
                    var ŝ = _W - s;
                    var w1 = x[n - 1L];
                    c = w1 >> (int)(ŝ);
                    for (var i = n - 1L; i > 0L; i--)
                    {
                        var w = w1;
                        w1 = x[i - 1L];
                        z[i] = w << (int)(s) | w1 >> (int)(ŝ);
                    }

                    z[0L] = w1 << (int)(s);
                }

            }
            return;
        }

        private static Word shrVU_g(slice<Word> z, slice<Word> x, ulong s)
        {
            {
                var n = len(z);

                if (n > 0L)
                {
                    var ŝ = _W - s;
                    var w1 = x[0L];
                    c = w1 << (int)(ŝ);
                    for (long i = 0L; i < n - 1L; i++)
                    {
                        var w = w1;
                        w1 = x[i + 1L];
                        z[i] = w >> (int)(s) | w1 << (int)(ŝ);
                    }

                    z[n - 1L] = w1 >> (int)(s);
                }

            }
            return;
        }

        private static Word mulAddVWW_g(slice<Word> z, slice<Word> x, Word y, Word r)
        {
            c = r;
            foreach (var (i) in z)
            {
                c, z[i] = mulAddWWW_g(x[i], y, c);
            }
            return;
        }

        // TODO(gri) Remove use of addWW_g here and then we can remove addWW_g and subWW_g.
        private static Word addMulVVW_g(slice<Word> z, slice<Word> x, Word y)
        {
            foreach (var (i) in z)
            {
                var (z1, z0) = mulAddWWW_g(x[i], y, z[i]);
                c, z[i] = addWW_g(z0, c, 0L);
                c += z1;
            }
            return;
        }

        private static Word divWVW_g(slice<Word> z, Word xn, slice<Word> x, Word y)
        {
            r = xn;
            for (var i = len(z) - 1L; i >= 0L; i--)
            {
                z[i], r = divWW_g(r, x[i], y);
            }

            return;
        }
    }
}}
