// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements unsigned multi-precision integers (natural
// numbers). They are the building blocks for the implementation
// of signed integers, rationals, and floating-point numbers.

// package big -- go2cs converted at 2020 August 29 08:29:24 UTC
// import "math/big" ==> using big = go.math.big_package
// Original source: C:\Go\src\math\big\nat.go
using bits = go.math.bits_package;
using rand = go.math.rand_package;
using sync = go.sync_package;
using static go.builtin;

namespace go {
namespace math
{
    public static partial class big_package
    {
        // An unsigned integer x of the form
        //
        //   x = x[n-1]*_B^(n-1) + x[n-2]*_B^(n-2) + ... + x[1]*_B + x[0]
        //
        // with 0 <= x[i] < _B and 0 <= i < n is stored in a slice of length n,
        // with the digits x[i] as the slice elements.
        //
        // A number is normalized if the slice contains no leading 0 digits.
        // During arithmetic operations, denormalized values may occur but are
        // always normalized before returning the final result. The normalized
        // representation of 0 is the empty or nil slice (length = 0).
        //
        private partial struct nat // : slice<Word>
        {
        }

        private static nat natOne = new nat(1);        private static nat natTwo = new nat(2);        private static nat natTen = new nat(10);

        private static void clear(this nat z)
        {
            foreach (var (i) in z)
            {
                z[i] = 0L;
            }
        }

        private static nat norm(this nat z)
        {
            var i = len(z);
            while (i > 0L && z[i - 1L] == 0L)
            {
                i--;
            }

            return z[0L..i];
        }

        private static nat make(this nat z, long n)
        {
            if (n <= cap(z))
            {
                return z[..n]; // reuse z
            } 
            // Choosing a good value for e has significant performance impact
            // because it increases the chance that a value can be reused.
            const long e = 4L; // extra capacity
 // extra capacity
            return make(nat, n, n + e);
        }

        private static nat setWord(this nat z, Word x)
        {
            if (x == 0L)
            {
                return z[..0L];
            }
            z = z.make(1L);
            z[0L] = x;
            return z;
        }

        private static nat setUint64(this nat z, ulong x)
        { 
            // single-word value
            {
                var w = Word(x);

                if (uint64(w) == x)
                {
                    return z.setWord(w);
                } 
                // 2-word value

            } 
            // 2-word value
            z = z.make(2L);
            z[1L] = Word(x >> (int)(32L));
            z[0L] = Word(x);
            return z;
        }

        private static nat set(this nat z, nat x)
        {
            z = z.make(len(x));
            copy(z, x);
            return z;
        }

        private static nat add(this nat z, nat x, nat y)
        {
            var m = len(x);
            var n = len(y);


            if (m < n) 
                return z.add(y, x);
            else if (m == 0L) 
                // n == 0 because m >= n; result is 0
                return z[..0L];
            else if (n == 0L) 
                // result is x
                return z.set(x);
            // m > 0

            z = z.make(m + 1L);
            var c = addVV(z[0L..n], x, y);
            if (m > n)
            {
                c = addVW(z[n..m], x[n..], c);
            }
            z[m] = c;

            return z.norm();
        }

        private static nat sub(this nat z, nat x, nat y) => func((_, panic, __) =>
        {
            var m = len(x);
            var n = len(y);


            if (m < n) 
                panic("underflow");
            else if (m == 0L) 
                // n == 0 because m >= n; result is 0
                return z[..0L];
            else if (n == 0L) 
                // result is x
                return z.set(x);
            // m > 0

            z = z.make(m);
            var c = subVV(z[0L..n], x, y);
            if (m > n)
            {
                c = subVW(z[n..], x[n..], c);
            }
            if (c != 0L)
            {
                panic("underflow");
            }
            return z.norm();
        });

        private static long cmp(this nat x, nat y)
        {
            var m = len(x);
            var n = len(y);
            if (m != n || m == 0L)
            {

                if (m < n) 
                    r = -1L;
                else if (m > n) 
                    r = 1L;
                                return;
            }
            var i = m - 1L;
            while (i > 0L && x[i] == y[i])
            {
                i--;
            }



            if (x[i] < y[i]) 
                r = -1L;
            else if (x[i] > y[i]) 
                r = 1L;
                        return;
        }

        private static nat mulAddWW(this nat z, nat x, Word y, Word r)
        {
            var m = len(x);
            if (m == 0L || y == 0L)
            {
                return z.setWord(r); // result is r
            } 
            // m > 0
            z = z.make(m + 1L);
            z[m] = mulAddVWW(z[0L..m], x, y, r);

            return z.norm();
        }

        // basicMul multiplies x and y and leaves the result in z.
        // The (non-normalized) result is placed in z[0 : len(x) + len(y)].
        private static void basicMul(nat z, nat x, nat y)
        {
            z[0L..len(x) + len(y)].clear(); // initialize z
            foreach (var (i, d) in y)
            {
                if (d != 0L)
                {
                    z[len(x) + i] = addMulVVW(z[i..i + len(x)], x, d);
                }
            }
        }

        // montgomery computes z mod m = x*y*2**(-n*_W) mod m,
        // assuming k = -1/m mod 2**_W.
        // z is used for storing the result which is returned;
        // z must not alias x, y or m.
        // See Gueron, "Efficient Software Implementations of Modular Exponentiation".
        // https://eprint.iacr.org/2011/239.pdf
        // In the terminology of that paper, this is an "Almost Montgomery Multiplication":
        // x and y are required to satisfy 0 <= z < 2**(n*_W) and then the result
        // z is guaranteed to satisfy 0 <= z < 2**(n*_W), but it may not be < m.
        private static nat montgomery(this nat z, nat x, nat y, nat m, Word k, long n) => func((_, panic, __) =>
        { 
            // This code assumes x, y, m are all the same length, n.
            // (required by addMulVVW and the for loop).
            // It also assumes that x, y are already reduced mod m,
            // or else the result will not be properly reduced.
            if (len(x) != n || len(y) != n || len(m) != n)
            {
                panic("math/big: mismatched montgomery number lengths");
            }
            z = z.make(n);
            z.clear();
            Word c = default;
            for (long i = 0L; i < n; i++)
            {
                var d = y[i];
                var c2 = addMulVVW(z, x, d);
                var t = z[0L] * k;
                var c3 = addMulVVW(z, m, t);
                copy(z, z[1L..]);
                var cx = c + c2;
                var cy = cx + c3;
                z[n - 1L] = cy;
                if (cx < c2 || cy < c3)
                {
                    c = 1L;
                }
                else
                {
                    c = 0L;
                }
            }

            if (c != 0L)
            {
                subVV(z, z, m);
            }
            return z;
        });

        // Fast version of z[0:n+n>>1].add(z[0:n+n>>1], x[0:n]) w/o bounds checks.
        // Factored out for readability - do not use outside karatsuba.
        private static void karatsubaAdd(nat z, nat x, long n)
        {
            {
                var c = addVV(z[0L..n], z, x);

                if (c != 0L)
                {
                    addVW(z[n..n + n >> (int)(1L)], z[n..], c);
                }

            }
        }

        // Like karatsubaAdd, but does subtract.
        private static void karatsubaSub(nat z, nat x, long n)
        {
            {
                var c = subVV(z[0L..n], z, x);

                if (c != 0L)
                {
                    subVW(z[n..n + n >> (int)(1L)], z[n..], c);
                }

            }
        }

        // Operands that are shorter than karatsubaThreshold are multiplied using
        // "grade school" multiplication; for longer operands the Karatsuba algorithm
        // is used.
        private static long karatsubaThreshold = 40L; // computed by calibrate_test.go

        // karatsuba multiplies x and y and leaves the result in z.
        // Both x and y must have the same length n and n must be a
        // power of 2. The result vector z must have len(z) >= 6*n.
        // The (non-normalized) result is placed in z[0 : 2*n].
        private static void karatsuba(nat z, nat x, nat y)
        {
            var n = len(y); 

            // Switch to basic multiplication if numbers are odd or small.
            // (n is always even if karatsubaThreshold is even, but be
            // conservative)
            if (n & 1L != 0L || n < karatsubaThreshold || n < 2L)
            {
                basicMul(z, x, y);
                return;
            } 
            // n&1 == 0 && n >= karatsubaThreshold && n >= 2

            // Karatsuba multiplication is based on the observation that
            // for two numbers x and y with:
            //
            //   x = x1*b + x0
            //   y = y1*b + y0
            //
            // the product x*y can be obtained with 3 products z2, z1, z0
            // instead of 4:
            //
            //   x*y = x1*y1*b*b + (x1*y0 + x0*y1)*b + x0*y0
            //       =    z2*b*b +              z1*b +    z0
            //
            // with:
            //
            //   xd = x1 - x0
            //   yd = y0 - y1
            //
            //   z1 =      xd*yd                    + z2 + z0
            //      = (x1-x0)*(y0 - y1)             + z2 + z0
            //      = x1*y0 - x1*y1 - x0*y0 + x0*y1 + z2 + z0
            //      = x1*y0 -    z2 -    z0 + x0*y1 + z2 + z0
            //      = x1*y0                 + x0*y1

            // split x, y into "digits"
            var n2 = n >> (int)(1L); // n2 >= 1
            var x1 = x[n2..];
            var x0 = x[0L..n2]; // x = x1*b + y0
            var y1 = y[n2..];
            var y0 = y[0L..n2]; // y = y1*b + y0

            // z is used for the result and temporary storage:
            //
            //   6*n     5*n     4*n     3*n     2*n     1*n     0*n
            // z = [z2 copy|z0 copy| xd*yd | yd:xd | x1*y1 | x0*y0 ]
            //
            // For each recursive call of karatsuba, an unused slice of
            // z is passed in that has (at least) half the length of the
            // caller's z.

            // compute z0 and z2 with the result "in place" in z
            karatsuba(z, x0, y0); // z0 = x0*y0
            karatsuba(z[n..], x1, y1); // z2 = x1*y1

            // compute xd (or the negative value if underflow occurs)
            long s = 1L; // sign of product xd*yd
            var xd = z[2L * n..2L * n + n2];
            if (subVV(xd, x1, x0) != 0L)
            { // x1-x0
                s = -s;
                subVV(xd, x0, x1); // x0-x1
            } 

            // compute yd (or the negative value if underflow occurs)
            var yd = z[2L * n + n2..3L * n];
            if (subVV(yd, y0, y1) != 0L)
            { // y0-y1
                s = -s;
                subVV(yd, y1, y0); // y1-y0
            } 

            // p = (x1-x0)*(y0-y1) == x1*y0 - x1*y1 - x0*y0 + x0*y1 for s > 0
            // p = (x0-x1)*(y0-y1) == x0*y0 - x0*y1 - x1*y0 + x1*y1 for s < 0
            var p = z[n * 3L..];
            karatsuba(p, xd, yd); 

            // save original z2:z0
            // (ok to use upper half of z since we're done recursing)
            var r = z[n * 4L..];
            copy(r, z[..n * 2L]); 

            // add up all partial products
            //
            //   2*n     n     0
            // z = [ z2  | z0  ]
            //   +    [ z0  ]
            //   +    [ z2  ]
            //   +    [  p  ]
            //
            karatsubaAdd(z[n2..], r, n);
            karatsubaAdd(z[n2..], r[n..], n);
            if (s > 0L)
            {
                karatsubaAdd(z[n2..], p, n);
            }
            else
            {
                karatsubaSub(z[n2..], p, n);
            }
        }

        // alias reports whether x and y share the same base array.
        private static bool alias(nat x, nat y)
        {
            return cap(x) > 0L && cap(y) > 0L && ref x[0L..cap(x)][cap(x) - 1L] == ref y[0L..cap(y)][cap(y) - 1L];
        }

        // addAt implements z += x<<(_W*i); z must be long enough.
        // (we don't use nat.add because we need z to stay the same
        // slice, and we don't need to normalize z after each addition)
        private static void addAt(nat z, nat x, long i)
        {
            {
                var n = len(x);

                if (n > 0L)
                {
                    {
                        var c = addVV(z[i..i + n], z[i..], x);

                        if (c != 0L)
                        {
                            var j = i + n;
                            if (j < len(z))
                            {
                                addVW(z[j..], z[j..], c);
                            }
                        }

                    }
                }

            }
        }

        private static long max(long x, long y)
        {
            if (x > y)
            {
                return x;
            }
            return y;
        }

        // karatsubaLen computes an approximation to the maximum k <= n such that
        // k = p<<i for a number p <= karatsubaThreshold and an i >= 0. Thus, the
        // result is the largest number that can be divided repeatedly by 2 before
        // becoming about the value of karatsubaThreshold.
        private static long karatsubaLen(long n)
        {
            var i = uint(0L);
            while (n > karatsubaThreshold)
            {
                n >>= 1L;
                i++;
            }

            return n << (int)(i);
        }

        private static nat mul(this nat z, nat x, nat y)
        {
            var m = len(x);
            var n = len(y);


            if (m < n) 
                return z.mul(y, x);
            else if (m == 0L || n == 0L) 
                return z[..0L];
            else if (n == 1L) 
                return z.mulAddWW(x, y[0L], 0L);
            // m >= n > 1

            // determine if z can be reused
            if (alias(z, x) || alias(z, y))
            {
                z = null; // z is an alias for x or y - cannot reuse
            } 

            // use basic multiplication if the numbers are small
            if (n < karatsubaThreshold)
            {
                z = z.make(m + n);
                basicMul(z, x, y);
                return z.norm();
            } 
            // m >= n && n >= karatsubaThreshold && n >= 2

            // determine Karatsuba length k such that
            //
            //   x = xh*b + x0  (0 <= x0 < b)
            //   y = yh*b + y0  (0 <= y0 < b)
            //   b = 1<<(_W*k)  ("base" of digits xi, yi)
            //
            var k = karatsubaLen(n); 
            // k <= n

            // multiply x0 and y0 via Karatsuba
            var x0 = x[0L..k]; // x0 is not normalized
            var y0 = y[0L..k]; // y0 is not normalized
            z = z.make(max(6L * k, m + n)); // enough space for karatsuba of x0*y0 and full result of x*y
            karatsuba(z, x0, y0);
            z = z[0L..m + n]; // z has final length but may be incomplete
            z[2L * k..].clear(); // upper portion of z is garbage (and 2*k <= m+n since k <= n <= m)

            // If xh != 0 or yh != 0, add the missing terms to z. For
            //
            //   xh = xi*b^i + ... + x2*b^2 + x1*b (0 <= xi < b)
            //   yh =                         y1*b (0 <= y1 < b)
            //
            // the missing terms are
            //
            //   x0*y1*b and xi*y0*b^i, xi*y1*b^(i+1) for i > 0
            //
            // since all the yi for i > 1 are 0 by choice of k: If any of them
            // were > 0, then yh >= b^2 and thus y >= b^2. Then k' = k*2 would
            // be a larger valid threshold contradicting the assumption about k.
            //
            if (k < n || m != n)
            {
                nat t = default; 

                // add x0*y1*b
                x0 = x0.norm();
                var y1 = y[k..]; // y1 is normalized because y is
                t = t.mul(x0, y1); // update t so we don't lose t's underlying array
                addAt(z, t, k); 

                // add xi*y0<<i, xi*y1*b<<(i+k)
                y0 = y0.norm();
                {
                    var i = k;

                    while (i < len(x))
                    {
                        var xi = x[i..];
                        if (len(xi) > k)
                        {
                            xi = xi[..k];
                        i += k;
                        }
                        xi = xi.norm();
                        t = t.mul(xi, y0);
                        addAt(z, t, i);
                        t = t.mul(xi, y1);
                        addAt(z, t, i + k);
                    }

                }
            }
            return z.norm();
        }

        // basicSqr sets z = x*x and is asymptotically faster than basicMul
        // by about a factor of 2, but slower for small arguments due to overhead.
        // Requirements: len(x) > 0, len(z) >= 2*len(x)
        // The (non-normalized) result is placed in z[0 : 2 * len(x)].
        private static void basicSqr(nat z, nat x)
        {
            var n = len(x);
            var t = make(nat, 2L * n); // temporary variable to hold the products
            z[1L], z[0L] = mulWW(x[0L], x[0L]); // the initial square
            for (long i = 1L; i < n; i++)
            {
                var d = x[i]; 
                // z collects the squares x[i] * x[i]
                z[2L * i + 1L], z[2L * i] = mulWW(d, d); 
                // t collects the products x[i] * x[j] where j < i
                t[2L * i] = addMulVVW(t[i..2L * i], x[0L..i], d);
            }

            t[2L * n - 1L] = shlVU(t[1L..2L * n - 1L], t[1L..2L * n - 1L], 1L); // double the j < i products
            addVV(z, z, t); // combine the result
        }

        // Operands that are shorter than basicSqrThreshold are squared using
        // "grade school" multiplication; for operands longer than karatsubaSqrThreshold
        // the Karatsuba algorithm is used.
        private static long basicSqrThreshold = 20L; // computed by calibrate_test.go
        private static long karatsubaSqrThreshold = 400L; // computed by calibrate_test.go

        // z = x*x
        private static nat sqr(this nat z, nat x)
        {
            var n = len(x);

            if (n == 0L) 
                return z[..0L];
            else if (n == 1L) 
                var d = x[0L];
                z = z.make(2L);
                z[1L], z[0L] = mulWW(d, d);
                return z.norm();
                        if (alias(z, x))
            {
                z = null; // z is an alias for x - cannot reuse
            }
            z = z.make(2L * n);

            if (n < basicSqrThreshold)
            {
                basicMul(z, x, x);
                return z.norm();
            }
            if (n < karatsubaSqrThreshold)
            {
                basicSqr(z, x);
                return z.norm();
            }
            return z.mul(x, x);
        }

        // mulRange computes the product of all the unsigned integers in the
        // range [a, b] inclusively. If a > b (empty range), the result is 1.
        private static nat mulRange(this nat z, ulong a, ulong b)
        {

            if (a == 0L) 
                // cut long ranges short (optimization)
                return z.setUint64(0L);
            else if (a > b) 
                return z.setUint64(1L);
            else if (a == b) 
                return z.setUint64(a);
            else if (a + 1L == b) 
                return z.mul(nat(null).setUint64(a), nat(null).setUint64(b));
                        var m = (a + b) / 2L;
            return z.mul(nat(null).mulRange(a, m), nat(null).mulRange(m + 1L, b));
        }

        // q = (x-r)/y, with 0 <= r < y
        private static (nat, Word) divW(this nat z, nat x, Word y) => func((_, panic, __) =>
        {
            var m = len(x);

            if (y == 0L) 
                panic("division by zero");
            else if (y == 1L) 
                q = z.set(x); // result is x
                return;
            else if (m == 0L) 
                q = z[..0L]; // result is 0
                return;
            // m > 0
            z = z.make(m);
            r = divWVW(z, 0L, x, y);
            q = z.norm();
            return;
        });

        private static (nat, nat) div(this nat z, nat z2, nat u, nat v) => func((_, panic, __) =>
        {
            if (len(v) == 0L)
            {
                panic("division by zero");
            }
            if (u.cmp(v) < 0L)
            {
                q = z[..0L];
                r = z2.set(u);
                return;
            }
            if (len(v) == 1L)
            {
                Word r2 = default;
                q, r2 = z.divW(u, v[0L]);
                r = z2.setWord(r2);
                return;
            }
            q, r = z.divLarge(z2, u, v);
            return;
        });

        // getNat returns a *nat of len n. The contents may not be zero.
        // The pool holds *nat to avoid allocation when converting to interface{}.
        private static ref nat getNat(long n)
        {
            ref nat z = default;
            {
                var v = natPool.Get();

                if (v != null)
                {
                    z = v._<ref nat>();
                }

            }
            if (z == null)
            {
                z = @new<nat>();
            }
            z.Value = z.make(n);
            return z;
        }

        private static void putNat(ref nat x)
        {
            natPool.Put(x);
        }

        private static sync.Pool natPool = default;

        // q = (uIn-r)/v, with 0 <= r < y
        // Uses z as storage for q, and u as storage for r if possible.
        // See Knuth, Volume 2, section 4.3.1, Algorithm D.
        // Preconditions:
        //    len(v) >= 2
        //    len(uIn) >= len(v)
        private static (nat, nat) divLarge(this nat z, nat u, nat uIn, nat v)
        {
            var n = len(v);
            var m = len(uIn) - n; 

            // determine if z can be reused
            // TODO(gri) should find a better solution - this if statement
            //           is very costly (see e.g. time pidigits -s -n 10000)
            if (alias(z, u) || alias(z, uIn) || alias(z, v))
            {
                z = null; // z is an alias for u or uIn or v - cannot reuse
            }
            q = z.make(m + 1L);

            var qhatvp = getNat(n + 1L);
            var qhatv = qhatvp.Value;
            if (alias(u, uIn) || alias(u, v))
            {
                u = null; // u is an alias for uIn or v - cannot reuse
            }
            u = u.make(len(uIn) + 1L);
            u.clear(); // TODO(gri) no need to clear if we allocated a new u

            // D1.
            ref nat v1p = default;
            var shift = nlz(v[n - 1L]);
            if (shift > 0L)
            { 
                // do not modify v, it may be used by another goroutine simultaneously
                v1p = getNat(n);
                var v1 = v1p.Value;
                shlVU(v1, v, shift);
                v = v1;
            }
            u[len(uIn)] = shlVU(u[0L..len(uIn)], uIn, shift); 

            // D2.
            var vn1 = v[n - 1L];
            for (var j = m; j >= 0L; j--)
            { 
                // D3.
                var qhat = Word(_M);
                {
                    var ujn = u[j + n];

                    if (ujn != vn1)
                    {
                        Word rhat = default;
                        qhat, rhat = divWW(ujn, u[j + n - 1L], vn1); 

                        // x1 | x2 = q̂v_{n-2}
                        var vn2 = v[n - 2L];
                        var (x1, x2) = mulWW(qhat, vn2); 
                        // test if q̂v_{n-2} > br̂ + u_{j+n-2}
                        var ujn2 = u[j + n - 2L];
                        while (greaterThan(x1, x2, rhat, ujn2))
                        {
                            qhat--;
                            var prevRhat = rhat;
                            rhat += vn1; 
                            // v[n-1] >= 0, so this tests for overflow.
                            if (rhat < prevRhat)
                            {
                                break;
                            }
                            x1, x2 = mulWW(qhat, vn2);
                        }

                    } 

                    // D4.

                } 

                // D4.
                qhatv[n] = mulAddVWW(qhatv[0L..n], v, qhat, 0L);

                var c = subVV(u[j..j + len(qhatv)], u[j..], qhatv);
                if (c != 0L)
                {
                    c = addVV(u[j..j + n], u[j..], v);
                    u[j + n] += c;
                    qhat--;
                }
                q[j] = qhat;
            }

            if (v1p != null)
            {
                putNat(v1p);
            }
            putNat(qhatvp);

            q = q.norm();
            shrVU(u, u, shift);
            r = u.norm();

            return (q, r);
        }

        // Length of x in bits. x must be normalized.
        private static long bitLen(this nat x)
        {
            {
                var i = len(x) - 1L;

                if (i >= 0L)
                {
                    return i * _W + bits.Len(uint(x[i]));
                }

            }
            return 0L;
        }

        // trailingZeroBits returns the number of consecutive least significant zero
        // bits of x.
        private static ulong trailingZeroBits(this nat x)
        {
            if (len(x) == 0L)
            {
                return 0L;
            }
            ulong i = default;
            while (x[i] == 0L)
            {
                i++;
            } 
            // x[i] != 0
 
            // x[i] != 0
            return i * _W + uint(bits.TrailingZeros(uint(x[i])));
        }

        // z = x << s
        private static nat shl(this nat z, nat x, ulong s)
        {
            var m = len(x);
            if (m == 0L)
            {
                return z[..0L];
            } 
            // m > 0
            var n = m + int(s / _W);
            z = z.make(n + 1L);
            z[n] = shlVU(z[n - m..n], x, s % _W);
            z[0L..n - m].clear();

            return z.norm();
        }

        // z = x >> s
        private static nat shr(this nat z, nat x, ulong s)
        {
            var m = len(x);
            var n = m - int(s / _W);
            if (n <= 0L)
            {
                return z[..0L];
            } 
            // n > 0
            z = z.make(n);
            shrVU(z, x[m - n..], s % _W);

            return z.norm();
        }

        private static nat setBit(this nat z, nat x, ulong i, ulong b) => func((_, panic, __) =>
        {
            var j = int(i / _W);
            var m = Word(1L) << (int)((i % _W));
            var n = len(x);
            switch (b)
            {
                case 0L: 
                    z = z.make(n);
                    copy(z, x);
                    if (j >= n)
                    { 
                        // no need to grow
                        return z;
                    }
                    z[j] &= m;
                    return z.norm();
                    break;
                case 1L: 
                    if (j >= n)
                    {
                        z = z.make(j + 1L);
                        z[n..].clear();
                    }
                    else
                    {
                        z = z.make(n);
                    }
                    copy(z, x);
                    z[j] |= m; 
                    // no need to normalize
                    return z;
                    break;
            }
            panic("set bit is not 0 or 1");
        });

        // bit returns the value of the i'th bit, with lsb == bit 0.
        private static ulong bit(this nat x, ulong i)
        {
            var j = i / _W;
            if (j >= uint(len(x)))
            {
                return 0L;
            } 
            // 0 <= j < len(x)
            return uint(x[j] >> (int)((i % _W)) & 1L);
        }

        // sticky returns 1 if there's a 1 bit within the
        // i least significant bits, otherwise it returns 0.
        private static ulong sticky(this nat x, ulong i)
        {
            var j = i / _W;
            if (j >= uint(len(x)))
            {
                if (len(x) == 0L)
                {
                    return 0L;
                }
                return 1L;
            } 
            // 0 <= j < len(x)
            foreach (var (_, x) in x[..j])
            {
                if (x != 0L)
                {
                    return 1L;
                }
            }
            if (x[j] << (int)((_W - i % _W)) != 0L)
            {
                return 1L;
            }
            return 0L;
        }

        private static nat and(this nat z, nat x, nat y)
        {
            var m = len(x);
            var n = len(y);
            if (m > n)
            {
                m = n;
            } 
            // m <= n
            z = z.make(m);
            for (long i = 0L; i < m; i++)
            {
                z[i] = x[i] & y[i];
            }


            return z.norm();
        }

        private static nat andNot(this nat z, nat x, nat y)
        {
            var m = len(x);
            var n = len(y);
            if (n > m)
            {
                n = m;
            } 
            // m >= n
            z = z.make(m);
            for (long i = 0L; i < n; i++)
            {
                z[i] = x[i] & ~y[i];
            }

            copy(z[n..m], x[n..m]);

            return z.norm();
        }

        private static nat or(this nat z, nat x, nat y)
        {
            var m = len(x);
            var n = len(y);
            var s = x;
            if (m < n)
            {
                n = m;
                m = n;
                s = y;
            } 
            // m >= n
            z = z.make(m);
            for (long i = 0L; i < n; i++)
            {
                z[i] = x[i] | y[i];
            }

            copy(z[n..m], s[n..m]);

            return z.norm();
        }

        private static nat xor(this nat z, nat x, nat y)
        {
            var m = len(x);
            var n = len(y);
            var s = x;
            if (m < n)
            {
                n = m;
                m = n;
                s = y;
            } 
            // m >= n
            z = z.make(m);
            for (long i = 0L; i < n; i++)
            {
                z[i] = x[i] ^ y[i];
            }

            copy(z[n..m], s[n..m]);

            return z.norm();
        }

        // greaterThan reports whether (x1<<_W + x2) > (y1<<_W + y2)
        private static bool greaterThan(Word x1, Word x2, Word y1, Word y2)
        {
            return x1 > y1 || x1 == y1 && x2 > y2;
        }

        // modW returns x % d.
        private static Word modW(this nat x, Word d)
        { 
            // TODO(agl): we don't actually need to store the q value.
            nat q = default;
            q = q.make(len(x));
            return divWVW(q, 0L, x, d);
        }

        // random creates a random integer in [0..limit), using the space in z if
        // possible. n is the bit length of limit.
        private static nat random(this nat z, ref rand.Rand _rand, nat limit, long n) => func(_rand, (ref rand.Rand rand, Defer _, Panic panic, Recover __) =>
        {
            if (alias(z, limit))
            {
                z = null; // z is an alias for limit - cannot reuse
            }
            z = z.make(len(limit));

            var bitLengthOfMSW = uint(n % _W);
            if (bitLengthOfMSW == 0L)
            {
                bitLengthOfMSW = _W;
            }
            var mask = Word((1L << (int)(bitLengthOfMSW)) - 1L);

            while (true)
            {
                switch (_W)
                {
                    case 32L: 
                        {
                            var i__prev2 = i;

                            foreach (var (__i) in z)
                            {
                                i = __i;
                                z[i] = Word(rand.Uint32());
                            }

                            i = i__prev2;
                        }
                        break;
                    case 64L: 
                        {
                            var i__prev2 = i;

                            foreach (var (__i) in z)
                            {
                                i = __i;
                                z[i] = Word(rand.Uint32()) | Word(rand.Uint32()) << (int)(32L);
                            }

                            i = i__prev2;
                        }
                        break;
                    default: 
                        panic("unknown word size");
                        break;
                }
                z[len(limit) - 1L] &= mask;
                if (z.cmp(limit) < 0L)
                {
                    break;
                }
            }


            return z.norm();
        });

        // If m != 0 (i.e., len(m) != 0), expNN sets z to x**y mod m;
        // otherwise it sets z to x**y. The result is the value of z.
        private static nat expNN(this nat z, nat x, nat y, nat m)
        {
            if (alias(z, x) || alias(z, y))
            { 
                // We cannot allow in-place modification of x or y.
                z = null;
            } 

            // x**y mod 1 == 0
            if (len(m) == 1L && m[0L] == 1L)
            {
                return z.setWord(0L);
            } 
            // m == 0 || m > 1

            // x**0 == 1
            if (len(y) == 0L)
            {
                return z.setWord(1L);
            } 
            // y > 0

            // x**1 mod m == x mod m
            if (len(y) == 1L && y[0L] == 1L && len(m) != 0L)
            {
                _, z = z.div(z, x, m);
                return z;
            } 
            // y > 1
            if (len(m) != 0L)
            { 
                // We likely end up being as long as the modulus.
                z = z.make(len(m));
            }
            z = z.set(x); 

            // If the base is non-trivial and the exponent is large, we use
            // 4-bit, windowed exponentiation. This involves precomputing 14 values
            // (x^2...x^15) but then reduces the number of multiply-reduces by a
            // third. Even for a 32-bit exponent, this reduces the number of
            // operations. Uses Montgomery method for odd moduli.
            if (x.cmp(natOne) > 0L && len(y) > 1L && len(m) > 0L)
            {
                if (m[0L] & 1L == 1L)
                {
                    return z.expNNMontgomery(x, y, m);
                }
                return z.expNNWindowed(x, y, m);
            }
            var v = y[len(y) - 1L]; // v > 0 because y is normalized and y > 0
            var shift = nlz(v) + 1L;
            v <<= shift;
            nat q = default;

            const long mask = 1L << (int)((_W - 1L)); 

            // We walk through the bits of the exponent one by one. Each time we
            // see a bit, we square, thus doubling the power. If the bit is a one,
            // we also multiply by x, thus adding one to the power.

 

            // We walk through the bits of the exponent one by one. Each time we
            // see a bit, we square, thus doubling the power. If the bit is a one,
            // we also multiply by x, thus adding one to the power.

            var w = _W - int(shift); 
            // zz and r are used to avoid allocating in mul and div as
            // otherwise the arguments would alias.
            nat zz = default;            nat r = default;

            {
                long j__prev1 = j;

                for (long j = 0L; j < w; j++)
                {
                    zz = zz.sqr(z);
                    zz = z;
                    z = zz;

                    if (v & mask != 0L)
                    {
                        zz = zz.mul(z, x);
                        zz = z;
                        z = zz;
                    }
                    if (len(m) != 0L)
                    {
                        zz, r = zz.div(r, z, m);
                        zz = q;
                        r = z;
                        q = zz;
                        z = r;
                    }
                    v <<= 1L;
                }


                j = j__prev1;
            }

            for (var i = len(y) - 2L; i >= 0L; i--)
            {
                v = y[i];

                {
                    long j__prev2 = j;

                    for (j = 0L; j < _W; j++)
                    {
                        zz = zz.sqr(z);
                        zz = z;
                        z = zz;

                        if (v & mask != 0L)
                        {
                            zz = zz.mul(z, x);
                            zz = z;
                            z = zz;
                        }
                        if (len(m) != 0L)
                        {
                            zz, r = zz.div(r, z, m);
                            zz = q;
                            r = z;
                            q = zz;
                            z = r;
                        }
                        v <<= 1L;
                    }


                    j = j__prev2;
                }
            }


            return z.norm();
        }

        // expNNWindowed calculates x**y mod m using a fixed, 4-bit window.
        private static nat expNNWindowed(this nat z, nat x, nat y, nat m)
        { 
            // zz and r are used to avoid allocating in mul and div as otherwise
            // the arguments would alias.
            nat zz = default;            nat r = default;



            const long n = 4L; 
            // powers[i] contains x^i.
 
            // powers[i] contains x^i.
            array<nat> powers = new array<nat>(1L << (int)(n));
            powers[0L] = natOne;
            powers[1L] = x;
            {
                long i__prev1 = i;

                long i = 2L;

                while (i < 1L << (int)(n))
                {
                    var p2 = ref powers[i / 2L];
                    var p = ref powers[i];
                    var p1 = ref powers[i + 1L];
                    p.Value = p.sqr(p2.Value);
                    zz, r = zz.div(r, p.Value, m);
                    p.Value = r;
                    r = p.Value;
                    p1.Value = p1.mul(p.Value, x);
                    zz, r = zz.div(r, p1.Value, m);
                    p1.Value = r;
                    r = p1.Value;
                    i += 2L;
                }


                i = i__prev1;
            }

            z = z.setWord(1L);

            {
                long i__prev1 = i;

                for (i = len(y) - 1L; i >= 0L; i--)
                {
                    var yi = y[i];
                    {
                        long j = 0L;

                        while (j < _W)
                        {
                            if (i != len(y) - 1L || j != 0L)
                            { 
                                // Unrolled loop for significant performance
                                // gain. Use go test -bench=".*" in crypto/rsa
                                // to check performance before making changes.
                                zz = zz.sqr(z);
                                zz = z;
                                z = zz;
                                zz, r = zz.div(r, z, m);
                                z = r;
                                r = z;

                                zz = zz.sqr(z);
                                zz = z;
                                z = zz;
                                zz, r = zz.div(r, z, m);
                                z = r;
                                r = z;

                                zz = zz.sqr(z);
                                zz = z;
                                z = zz;
                                zz, r = zz.div(r, z, m);
                                z = r;
                                r = z;

                                zz = zz.sqr(z);
                                zz = z;
                                z = zz;
                                zz, r = zz.div(r, z, m);
                                z = r;
                                r = z;
                            j += n;
                            }
                            zz = zz.mul(z, powers[yi >> (int)((_W - n))]);
                            zz = z;
                            z = zz;
                            zz, r = zz.div(r, z, m);
                            z = r;
                            r = z;

                            yi <<= n;
                        }

                    }
                }


                i = i__prev1;
            }

            return z.norm();
        }

        // expNNMontgomery calculates x**y mod m using a fixed, 4-bit window.
        // Uses Montgomery representation.
        private static nat expNNMontgomery(this nat z, nat x, nat y, nat m)
        {
            var numWords = len(m); 

            // We want the lengths of x and m to be equal.
            // It is OK if x >= m as long as len(x) == len(m).
            if (len(x) > numWords)
            {
                _, x = nat(null).div(null, x, m); 
                // Note: now len(x) <= numWords, not guaranteed ==.
            }
            if (len(x) < numWords)
            {
                var rr = make(nat, numWords);
                copy(rr, x);
                x = rr;
            } 

            // Ideally the precomputations would be performed outside, and reused
            // k0 = -m**-1 mod 2**_W. Algorithm from: Dumas, J.G. "On Newton–Raphson
            // Iteration for Multiplicative Inverses Modulo Prime Powers".
            long k0 = 2L - m[0L];
            var t = m[0L] - 1L;
            {
                long i__prev1 = i;

                long i = 1L;

                while (i < _W)
                {
                    t *= t;
                    k0 *= (t + 1L);
                    i <<= 1L;
                }


                i = i__prev1;
            }
            k0 = -k0; 

            // RR = 2**(2*_W*len(m)) mod m
            var RR = nat(null).setWord(1L);
            var zz = nat(null).shl(RR, uint(2L * numWords * _W));
            _, RR = RR.div(RR, zz, m);
            if (len(RR) < numWords)
            {
                zz = zz.make(numWords);
                copy(zz, RR);
                RR = zz;
            } 
            // one = 1, with equal length to that of m
            var one = make(nat, numWords);
            one[0L] = 1L;

            const long n = 4L; 
            // powers[i] contains x^i
 
            // powers[i] contains x^i
            array<nat> powers = new array<nat>(1L << (int)(n));
            powers[0L] = powers[0L].montgomery(one, RR, m, k0, numWords);
            powers[1L] = powers[1L].montgomery(x, RR, m, k0, numWords);
            {
                long i__prev1 = i;

                for (i = 2L; i < 1L << (int)(n); i++)
                {
                    powers[i] = powers[i].montgomery(powers[i - 1L], powers[1L], m, k0, numWords);
                } 

                // initialize z = 1 (Montgomery 1)


                i = i__prev1;
            } 

            // initialize z = 1 (Montgomery 1)
            z = z.make(numWords);
            copy(z, powers[0L]);

            zz = zz.make(numWords); 

            // same windowed exponent, but with Montgomery multiplications
            {
                long i__prev1 = i;

                for (i = len(y) - 1L; i >= 0L; i--)
                {
                    var yi = y[i];
                    {
                        long j = 0L;

                        while (j < _W)
                        {
                            if (i != len(y) - 1L || j != 0L)
                            {
                                zz = zz.montgomery(z, z, m, k0, numWords);
                                z = z.montgomery(zz, zz, m, k0, numWords);
                                zz = zz.montgomery(z, z, m, k0, numWords);
                                z = z.montgomery(zz, zz, m, k0, numWords);
                            j += n;
                            }
                            zz = zz.montgomery(z, powers[yi >> (int)((_W - n))], m, k0, numWords);
                            z = zz;
                            zz = z;
                            yi <<= n;
                        }

                    }
                } 
                // convert to regular number


                i = i__prev1;
            } 
            // convert to regular number
            zz = zz.montgomery(z, one, m, k0, numWords); 

            // One last reduction, just in case.
            // See golang.org/issue/13907.
            if (zz.cmp(m) >= 0L)
            { 
                // Common case is m has high bit set; in that case,
                // since zz is the same length as m, there can be just
                // one multiple of m to remove. Just subtract.
                // We think that the subtract should be sufficient in general,
                // so do that unconditionally, but double-check,
                // in case our beliefs are wrong.
                // The div is not expected to be reached.
                zz = zz.sub(zz, m);
                if (zz.cmp(m) >= 0L)
                {
                    _, zz = nat(null).div(null, zz, m);
                }
            }
            return zz.norm();
        }

        // bytes writes the value of z into buf using big-endian encoding.
        // len(buf) must be >= len(z)*_S. The value of z is encoded in the
        // slice buf[i:]. The number i of unused bytes at the beginning of
        // buf is returned as result.
        private static long bytes(this nat z, slice<byte> buf)
        {
            i = len(buf);
            foreach (var (_, d) in z)
            {
                for (long j = 0L; j < _S; j++)
                {
                    i--;
                    buf[i] = byte(d);
                    d >>= 8L;
                }

            }
            while (i < len(buf) && buf[i] == 0L)
            {
                i++;
            }


            return;
        }

        // setBytes interprets buf as the bytes of a big-endian unsigned
        // integer, sets z to that value, and returns z.
        private static nat setBytes(this nat z, slice<byte> buf)
        {
            z = z.make((len(buf) + _S - 1L) / _S);

            long k = 0L;
            var s = uint(0L);
            Word d = default;
            for (var i = len(buf); i > 0L; i--)
            {
                d |= Word(buf[i - 1L]) << (int)(s);
                s += 8L;

                if (s == _S * 8L)
                {
                    z[k] = d;
                    k++;
                    s = 0L;
                    d = 0L;
                }
            }

            if (k < len(z))
            {
                z[k] = d;
            }
            return z.norm();
        }

        // sqrt sets z = ⌊√x⌋
        private static nat sqrt(this nat z, nat x)
        {
            if (x.cmp(natOne) <= 0L)
            {
                return z.set(x);
            }
            if (alias(z, x))
            {
                z = null;
            } 

            // Start with value known to be too large and repeat "z = ⌊(z + ⌊x/z⌋)/2⌋" until it stops getting smaller.
            // See Brent and Zimmermann, Modern Computer Arithmetic, Algorithm 1.13 (SqrtInt).
            // https://members.loria.fr/PZimmermann/mca/pub226.html
            // If x is one less than a perfect square, the sequence oscillates between the correct z and z+1;
            // otherwise it converges to the correct z and stays there.
            nat z1 = default;            nat z2 = default;

            z1 = z;
            z1 = z1.setUint64(1L);
            z1 = z1.shl(z1, uint(x.bitLen() / 2L + 1L)); // must be ≥ √x
            for (long n = 0L; >>MARKER:FOREXPRESSION_LEVEL_1<<; n++)
            {
                z2, _ = z2.div(null, x, z1);
                z2 = z2.add(z2, z1);
                z2 = z2.shr(z2, 1L);
                if (z2.cmp(z1) >= 0L)
                { 
                    // z1 is answer.
                    // Figure out whether z1 or z2 is currently aliased to z by looking at loop count.
                    if (n & 1L == 0L)
                    {
                        return z1;
                    }
                    return z.set(z1);
                }
                z1 = z2;
                z2 = z1;
            }

        }
    }
}}
