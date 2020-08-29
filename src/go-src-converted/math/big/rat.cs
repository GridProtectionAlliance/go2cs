// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements multi-precision rational numbers.

// package big -- go2cs converted at 2020 August 29 08:29:31 UTC
// import "math/big" ==> using big = go.math.big_package
// Original source: C:\Go\src\math\big\rat.go
using fmt = go.fmt_package;
using math = go.math_package;
using static go.builtin;

namespace go {
namespace math
{
    public static partial class big_package
    {
        // A Rat represents a quotient a/b of arbitrary precision.
        // The zero value for a Rat represents the value 0.
        public partial struct Rat
        {
            public Int a;
            public Int b;
        }

        // NewRat creates a new Rat with numerator a and denominator b.
        public static ref Rat NewRat(long a, long b)
        {
            return @new<Rat>().SetFrac64(a, b);
        }

        // SetFloat64 sets z to exactly f and returns z.
        // If f is not finite, SetFloat returns nil.
        private static ref Rat SetFloat64(this ref Rat z, double f)
        {
            const long expMask = 1L << (int)(11L) - 1L;

            var bits = math.Float64bits(f);
            var mantissa = bits & (1L << (int)(52L) - 1L);
            var exp = int((bits >> (int)(52L)) & expMask);

            if (exp == expMask) // non-finite
                return null;
            else if (exp == 0L) // denormal
                exp -= 1022L;
            else // normal
                mantissa |= 1L << (int)(52L);
                exp -= 1023L;
                        long shift = 52L - exp; 

            // Optimization (?): partially pre-normalise.
            while (mantissa & 1L == 0L && shift > 0L)
            {
                mantissa >>= 1L;
                shift--;
            }


            z.a.SetUint64(mantissa);
            z.a.neg = f < 0L;
            z.b.Set(intOne);
            if (shift > 0L)
            {
                z.b.Lsh(ref z.b, uint(shift));
            }
            else
            {
                z.a.Lsh(ref z.a, uint(-shift));
            }
            return z.norm();
        }

        // quotToFloat32 returns the non-negative float32 value
        // nearest to the quotient a/b, using round-to-even in
        // halfway cases. It does not mutate its arguments.
        // Preconditions: b is non-zero; a and b have no common factors.
        private static (float, bool) quotToFloat32(nat a, nat b) => func((_, panic, __) =>
        {
 
            // float size in bits
            const long Fsize = 32L; 

            // mantissa
            const long Msize = 23L;
            const var Msize1 = Msize + 1L; // incl. implicit 1
            const var Msize2 = Msize1 + 1L; 

            // exponent
            const var Esize = Fsize - Msize1;
            const long Ebias = 1L << (int)((Esize - 1L)) - 1L;
            const long Emin = 1L - Ebias;
            const var Emax = Ebias; 

            // TODO(adonovan): specialize common degenerate cases: 1.0, integers.
            var alen = a.bitLen();
            if (alen == 0L)
            {
                return (0L, true);
            }
            var blen = b.bitLen();
            if (blen == 0L)
            {
                panic("division by zero");
            } 

            // 1. Left-shift A or B such that quotient A/B is in [1<<Msize1, 1<<(Msize2+1)
            // (Msize2 bits if A < B when they are left-aligned, Msize2+1 bits if A >= B).
            // This is 2 or 3 more than the float32 mantissa field width of Msize:
            // - the optional extra bit is shifted away in step 3 below.
            // - the high-order 1 is omitted in "normal" representation;
            // - the low-order 1 will be used during rounding then discarded.
            var exp = alen - blen;
            nat a2 = default;            nat b2 = default;

            a2 = a2.set(a);
            b2 = b2.set(b);
            {
                var shift__prev1 = shift;

                var shift = Msize2 - exp;

                if (shift > 0L)
                {
                    a2 = a2.shl(a2, uint(shift));
                }
                else if (shift < 0L)
                {
                    b2 = b2.shl(b2, uint(-shift));
                } 

                // 2. Compute quotient and remainder (q, r).  NB: due to the
                // extra shift, the low-order bit of q is logically the
                // high-order bit of r.

                shift = shift__prev1;

            } 

            // 2. Compute quotient and remainder (q, r).  NB: due to the
            // extra shift, the low-order bit of q is logically the
            // high-order bit of r.
            nat q = default;
            var (q, r) = q.div(a2, a2, b2); // (recycle a2)
            var mantissa = low32(q);
            var haveRem = len(r) > 0L; // mantissa&1 && !haveRem => remainder is exactly half

            // 3. If quotient didn't fit in Msize2 bits, redo division by b2<<1
            // (in effect---we accomplish this incrementally).
            if (mantissa >> (int)(Msize2) == 1L)
            {
                if (mantissa & 1L == 1L)
                {
                    haveRem = true;
                }
                mantissa >>= 1L;
                exp++;
            }
            if (mantissa >> (int)(Msize1) != 1L)
            {
                panic(fmt.Sprintf("expected exactly %d bits of result", Msize2));
            } 

            // 4. Rounding.
            if (Emin - Msize <= exp && exp <= Emin)
            { 
                // Denormal case; lose 'shift' bits of precision.
                shift = uint(Emin - (exp - 1L)); // [1..Esize1)
                var lostbits = mantissa & (1L << (int)(shift) - 1L);
                haveRem = haveRem || lostbits != 0L;
                mantissa >>= shift;
                exp = 2L - Ebias; // == exp + shift
            } 
            // Round q using round-half-to-even.
            exact = !haveRem;
            if (mantissa & 1L != 0L)
            {
                exact = false;
                if (haveRem || mantissa & 2L != 0L)
                {
                    mantissa++;

                    if (mantissa >= 1L << (int)(Msize2))
                    { 
                        // Complete rollover 11...1 => 100...0, so shift is safe
                        mantissa >>= 1L;
                        exp++;
                    }
                }
            }
            mantissa >>= 1L; // discard rounding bit.  Mantissa now scaled by 1<<Msize1.

            f = float32(math.Ldexp(float64(mantissa), exp - Msize1));
            if (math.IsInf(float64(f), 0L))
            {
                exact = false;
            }
            return;
        });

        // quotToFloat64 returns the non-negative float64 value
        // nearest to the quotient a/b, using round-to-even in
        // halfway cases. It does not mutate its arguments.
        // Preconditions: b is non-zero; a and b have no common factors.
        private static (double, bool) quotToFloat64(nat a, nat b) => func((_, panic, __) =>
        {
 
            // float size in bits
            const long Fsize = 64L; 

            // mantissa
            const long Msize = 52L;
            const var Msize1 = Msize + 1L; // incl. implicit 1
            const var Msize2 = Msize1 + 1L; 

            // exponent
            const var Esize = Fsize - Msize1;
            const long Ebias = 1L << (int)((Esize - 1L)) - 1L;
            const long Emin = 1L - Ebias;
            const var Emax = Ebias; 

            // TODO(adonovan): specialize common degenerate cases: 1.0, integers.
            var alen = a.bitLen();
            if (alen == 0L)
            {
                return (0L, true);
            }
            var blen = b.bitLen();
            if (blen == 0L)
            {
                panic("division by zero");
            } 

            // 1. Left-shift A or B such that quotient A/B is in [1<<Msize1, 1<<(Msize2+1)
            // (Msize2 bits if A < B when they are left-aligned, Msize2+1 bits if A >= B).
            // This is 2 or 3 more than the float64 mantissa field width of Msize:
            // - the optional extra bit is shifted away in step 3 below.
            // - the high-order 1 is omitted in "normal" representation;
            // - the low-order 1 will be used during rounding then discarded.
            var exp = alen - blen;
            nat a2 = default;            nat b2 = default;

            a2 = a2.set(a);
            b2 = b2.set(b);
            {
                var shift__prev1 = shift;

                var shift = Msize2 - exp;

                if (shift > 0L)
                {
                    a2 = a2.shl(a2, uint(shift));
                }
                else if (shift < 0L)
                {
                    b2 = b2.shl(b2, uint(-shift));
                } 

                // 2. Compute quotient and remainder (q, r).  NB: due to the
                // extra shift, the low-order bit of q is logically the
                // high-order bit of r.

                shift = shift__prev1;

            } 

            // 2. Compute quotient and remainder (q, r).  NB: due to the
            // extra shift, the low-order bit of q is logically the
            // high-order bit of r.
            nat q = default;
            var (q, r) = q.div(a2, a2, b2); // (recycle a2)
            var mantissa = low64(q);
            var haveRem = len(r) > 0L; // mantissa&1 && !haveRem => remainder is exactly half

            // 3. If quotient didn't fit in Msize2 bits, redo division by b2<<1
            // (in effect---we accomplish this incrementally).
            if (mantissa >> (int)(Msize2) == 1L)
            {
                if (mantissa & 1L == 1L)
                {
                    haveRem = true;
                }
                mantissa >>= 1L;
                exp++;
            }
            if (mantissa >> (int)(Msize1) != 1L)
            {
                panic(fmt.Sprintf("expected exactly %d bits of result", Msize2));
            } 

            // 4. Rounding.
            if (Emin - Msize <= exp && exp <= Emin)
            { 
                // Denormal case; lose 'shift' bits of precision.
                shift = uint(Emin - (exp - 1L)); // [1..Esize1)
                var lostbits = mantissa & (1L << (int)(shift) - 1L);
                haveRem = haveRem || lostbits != 0L;
                mantissa >>= shift;
                exp = 2L - Ebias; // == exp + shift
            } 
            // Round q using round-half-to-even.
            exact = !haveRem;
            if (mantissa & 1L != 0L)
            {
                exact = false;
                if (haveRem || mantissa & 2L != 0L)
                {
                    mantissa++;

                    if (mantissa >= 1L << (int)(Msize2))
                    { 
                        // Complete rollover 11...1 => 100...0, so shift is safe
                        mantissa >>= 1L;
                        exp++;
                    }
                }
            }
            mantissa >>= 1L; // discard rounding bit.  Mantissa now scaled by 1<<Msize1.

            f = math.Ldexp(float64(mantissa), exp - Msize1);
            if (math.IsInf(f, 0L))
            {
                exact = false;
            }
            return;
        });

        // Float32 returns the nearest float32 value for x and a bool indicating
        // whether f represents x exactly. If the magnitude of x is too large to
        // be represented by a float32, f is an infinity and exact is false.
        // The sign of f always matches the sign of x, even if f == 0.
        private static (float, bool) Float32(this ref Rat x)
        {
            var b = x.b.abs;
            if (len(b) == 0L)
            {
                b = b.set(natOne); // materialize denominator
            }
            f, exact = quotToFloat32(x.a.abs, b);
            if (x.a.neg)
            {
                f = -f;
            }
            return;
        }

        // Float64 returns the nearest float64 value for x and a bool indicating
        // whether f represents x exactly. If the magnitude of x is too large to
        // be represented by a float64, f is an infinity and exact is false.
        // The sign of f always matches the sign of x, even if f == 0.
        private static (double, bool) Float64(this ref Rat x)
        {
            var b = x.b.abs;
            if (len(b) == 0L)
            {
                b = b.set(natOne); // materialize denominator
            }
            f, exact = quotToFloat64(x.a.abs, b);
            if (x.a.neg)
            {
                f = -f;
            }
            return;
        }

        // SetFrac sets z to a/b and returns z.
        private static ref Rat SetFrac(this ref Rat _z, ref Int _a, ref Int _b) => func(_z, _a, _b, (ref Rat z, ref Int a, ref Int b, Defer _, Panic panic, Recover __) =>
        {
            z.a.neg = a.neg != b.neg;
            var babs = b.abs;
            if (len(babs) == 0L)
            {
                panic("division by zero");
            }
            if (ref z.a == b || alias(z.a.abs, babs))
            {
                babs = nat(null).set(babs); // make a copy
            }
            z.a.abs = z.a.abs.set(a.abs);
            z.b.abs = z.b.abs.set(babs);
            return z.norm();
        });

        // SetFrac64 sets z to a/b and returns z.
        private static ref Rat SetFrac64(this ref Rat _z, long a, long b) => func(_z, (ref Rat z, Defer _, Panic panic, Recover __) =>
        {
            z.a.SetInt64(a);
            if (b == 0L)
            {
                panic("division by zero");
            }
            if (b < 0L)
            {
                b = -b;
                z.a.neg = !z.a.neg;
            }
            z.b.abs = z.b.abs.setUint64(uint64(b));
            return z.norm();
        });

        // SetInt sets z to x (by making a copy of x) and returns z.
        private static ref Rat SetInt(this ref Rat z, ref Int x)
        {
            z.a.Set(x);
            z.b.abs = z.b.abs[..0L];
            return z;
        }

        // SetInt64 sets z to x and returns z.
        private static ref Rat SetInt64(this ref Rat z, long x)
        {
            z.a.SetInt64(x);
            z.b.abs = z.b.abs[..0L];
            return z;
        }

        // Set sets z to x (by making a copy of x) and returns z.
        private static ref Rat Set(this ref Rat z, ref Rat x)
        {
            if (z != x)
            {
                z.a.Set(ref x.a);
                z.b.Set(ref x.b);
            }
            return z;
        }

        // Abs sets z to |x| (the absolute value of x) and returns z.
        private static ref Rat Abs(this ref Rat z, ref Rat x)
        {
            z.Set(x);
            z.a.neg = false;
            return z;
        }

        // Neg sets z to -x and returns z.
        private static ref Rat Neg(this ref Rat z, ref Rat x)
        {
            z.Set(x);
            z.a.neg = len(z.a.abs) > 0L && !z.a.neg; // 0 has no sign
            return z;
        }

        // Inv sets z to 1/x and returns z.
        private static ref Rat Inv(this ref Rat _z, ref Rat _x) => func(_z, _x, (ref Rat z, ref Rat x, Defer _, Panic panic, Recover __) =>
        {
            if (len(x.a.abs) == 0L)
            {
                panic("division by zero");
            }
            z.Set(x);
            var a = z.b.abs;
            if (len(a) == 0L)
            {
                a = a.set(natOne); // materialize numerator
            }
            var b = z.a.abs;
            if (b.cmp(natOne) == 0L)
            {
                b = b[..0L]; // normalize denominator
            }
            z.a.abs = a;
            z.b.abs = b; // sign doesn't change
            return z;
        });

        // Sign returns:
        //
        //    -1 if x <  0
        //     0 if x == 0
        //    +1 if x >  0
        //
        private static long Sign(this ref Rat x)
        {
            return x.a.Sign();
        }

        // IsInt reports whether the denominator of x is 1.
        private static bool IsInt(this ref Rat x)
        {
            return len(x.b.abs) == 0L || x.b.abs.cmp(natOne) == 0L;
        }

        // Num returns the numerator of x; it may be <= 0.
        // The result is a reference to x's numerator; it
        // may change if a new value is assigned to x, and vice versa.
        // The sign of the numerator corresponds to the sign of x.
        private static ref Int Num(this ref Rat x)
        {
            return ref x.a;
        }

        // Denom returns the denominator of x; it is always > 0.
        // The result is a reference to x's denominator; it
        // may change if a new value is assigned to x, and vice versa.
        private static ref Int Denom(this ref Rat x)
        {
            x.b.neg = false; // the result is always >= 0
            if (len(x.b.abs) == 0L)
            {
                x.b.abs = x.b.abs.set(natOne); // materialize denominator
            }
            return ref x.b;
        }

        private static ref Rat norm(this ref Rat z)
        {

            if (len(z.a.abs) == 0L) 
                // z == 0 - normalize sign and denominator
                z.a.neg = false;
                z.b.abs = z.b.abs[..0L];
            else if (len(z.b.abs) == 0L)             else if (z.b.abs.cmp(natOne) == 0L) 
                // z is int - normalize denominator
                z.b.abs = z.b.abs[..0L];
            else 
                var neg = z.a.neg;
                z.a.neg = false;
                z.b.neg = false;
                {
                    var f = NewInt(0L).lehmerGCD(ref z.a, ref z.b);

                    if (f.Cmp(intOne) != 0L)
                    {
                        z.a.abs, _ = z.a.abs.div(null, z.a.abs, f.abs);
                        z.b.abs, _ = z.b.abs.div(null, z.b.abs, f.abs);
                        if (z.b.abs.cmp(natOne) == 0L)
                        { 
                            // z is int - normalize denominator
                            z.b.abs = z.b.abs[..0L];
                        }
                    }

                }
                z.a.neg = neg;
                        return z;
        }

        // mulDenom sets z to the denominator product x*y (by taking into
        // account that 0 values for x or y must be interpreted as 1) and
        // returns z.
        private static nat mulDenom(nat z, nat x, nat y)
        {

            if (len(x) == 0L) 
                return z.set(y);
            else if (len(y) == 0L) 
                return z.set(x);
                        return z.mul(x, y);
        }

        // scaleDenom computes x*f.
        // If f == 0 (zero value of denominator), the result is (a copy of) x.
        private static ref Int scaleDenom(ref Int x, nat f)
        {
            Int z = default;
            if (len(f) == 0L)
            {
                return z.Set(x);
            }
            z.abs = z.abs.mul(x.abs, f);
            z.neg = x.neg;
            return ref z;
        }

        // Cmp compares x and y and returns:
        //
        //   -1 if x <  y
        //    0 if x == y
        //   +1 if x >  y
        //
        private static long Cmp(this ref Rat x, ref Rat y)
        {
            return scaleDenom(ref x.a, y.b.abs).Cmp(scaleDenom(ref y.a, x.b.abs));
        }

        // Add sets z to the sum x+y and returns z.
        private static ref Rat Add(this ref Rat z, ref Rat x, ref Rat y)
        {
            var a1 = scaleDenom(ref x.a, y.b.abs);
            var a2 = scaleDenom(ref y.a, x.b.abs);
            z.a.Add(a1, a2);
            z.b.abs = mulDenom(z.b.abs, x.b.abs, y.b.abs);
            return z.norm();
        }

        // Sub sets z to the difference x-y and returns z.
        private static ref Rat Sub(this ref Rat z, ref Rat x, ref Rat y)
        {
            var a1 = scaleDenom(ref x.a, y.b.abs);
            var a2 = scaleDenom(ref y.a, x.b.abs);
            z.a.Sub(a1, a2);
            z.b.abs = mulDenom(z.b.abs, x.b.abs, y.b.abs);
            return z.norm();
        }

        // Mul sets z to the product x*y and returns z.
        private static ref Rat Mul(this ref Rat z, ref Rat x, ref Rat y)
        {
            if (x == y)
            { 
                // a squared Rat is positive and can't be reduced
                z.a.neg = false;
                z.a.abs = z.a.abs.sqr(x.a.abs);
                z.b.abs = z.b.abs.sqr(x.b.abs);
                return z;
            }
            z.a.Mul(ref x.a, ref y.a);
            z.b.abs = mulDenom(z.b.abs, x.b.abs, y.b.abs);
            return z.norm();
        }

        // Quo sets z to the quotient x/y and returns z.
        // If y == 0, a division-by-zero run-time panic occurs.
        private static ref Rat Quo(this ref Rat _z, ref Rat _x, ref Rat _y) => func(_z, _x, _y, (ref Rat z, ref Rat x, ref Rat y, Defer _, Panic panic, Recover __) =>
        {
            if (len(y.a.abs) == 0L)
            {
                panic("division by zero");
            }
            var a = scaleDenom(ref x.a, y.b.abs);
            var b = scaleDenom(ref y.a, x.b.abs);
            z.a.abs = a.abs;
            z.b.abs = b.abs;
            z.a.neg = a.neg != b.neg;
            return z.norm();
        });
    }
}}
