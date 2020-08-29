// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements signed multi-precision integers.

// package big -- go2cs converted at 2020 August 29 08:29:17 UTC
// import "math/big" ==> using big = go.math.big_package
// Original source: C:\Go\src\math\big\int.go
using fmt = go.fmt_package;
using io = go.io_package;
using rand = go.math.rand_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace math
{
    public static partial class big_package
    {
        // An Int represents a signed multi-precision integer.
        // The zero value for an Int represents the value 0.
        public partial struct Int
        {
            public bool neg; // sign
            public nat abs; // absolute value of the integer
        }

        private static Int intOne = ref new Int(false,natOne);

        // Sign returns:
        //
        //    -1 if x <  0
        //     0 if x == 0
        //    +1 if x >  0
        //
        private static long Sign(this ref Int x)
        {
            if (len(x.abs) == 0L)
            {
                return 0L;
            }
            if (x.neg)
            {
                return -1L;
            }
            return 1L;
        }

        // SetInt64 sets z to x and returns z.
        private static ref Int SetInt64(this ref Int z, long x)
        {
            var neg = false;
            if (x < 0L)
            {
                neg = true;
                x = -x;
            }
            z.abs = z.abs.setUint64(uint64(x));
            z.neg = neg;
            return z;
        }

        // SetUint64 sets z to x and returns z.
        private static ref Int SetUint64(this ref Int z, ulong x)
        {
            z.abs = z.abs.setUint64(x);
            z.neg = false;
            return z;
        }

        // NewInt allocates and returns a new Int set to x.
        public static ref Int NewInt(long x)
        {
            return @new<Int>().SetInt64(x);
        }

        // Set sets z to x and returns z.
        private static ref Int Set(this ref Int z, ref Int x)
        {
            if (z != x)
            {
                z.abs = z.abs.set(x.abs);
                z.neg = x.neg;
            }
            return z;
        }

        // Bits provides raw (unchecked but fast) access to x by returning its
        // absolute value as a little-endian Word slice. The result and x share
        // the same underlying array.
        // Bits is intended to support implementation of missing low-level Int
        // functionality outside this package; it should be avoided otherwise.
        private static slice<Word> Bits(this ref Int x)
        {
            return x.abs;
        }

        // SetBits provides raw (unchecked but fast) access to z by setting its
        // value to abs, interpreted as a little-endian Word slice, and returning
        // z. The result and abs share the same underlying array.
        // SetBits is intended to support implementation of missing low-level Int
        // functionality outside this package; it should be avoided otherwise.
        private static ref Int SetBits(this ref Int z, slice<Word> abs)
        {
            z.abs = nat(abs).norm();
            z.neg = false;
            return z;
        }

        // Abs sets z to |x| (the absolute value of x) and returns z.
        private static ref Int Abs(this ref Int z, ref Int x)
        {
            z.Set(x);
            z.neg = false;
            return z;
        }

        // Neg sets z to -x and returns z.
        private static ref Int Neg(this ref Int z, ref Int x)
        {
            z.Set(x);
            z.neg = len(z.abs) > 0L && !z.neg; // 0 has no sign
            return z;
        }

        // Add sets z to the sum x+y and returns z.
        private static ref Int Add(this ref Int z, ref Int x, ref Int y)
        {
            var neg = x.neg;
            if (x.neg == y.neg)
            { 
                // x + y == x + y
                // (-x) + (-y) == -(x + y)
                z.abs = z.abs.add(x.abs, y.abs);
            }
            else
            { 
                // x + (-y) == x - y == -(y - x)
                // (-x) + y == y - x == -(x - y)
                if (x.abs.cmp(y.abs) >= 0L)
                {
                    z.abs = z.abs.sub(x.abs, y.abs);
                }
                else
                {
                    neg = !neg;
                    z.abs = z.abs.sub(y.abs, x.abs);
                }
            }
            z.neg = len(z.abs) > 0L && neg; // 0 has no sign
            return z;
        }

        // Sub sets z to the difference x-y and returns z.
        private static ref Int Sub(this ref Int z, ref Int x, ref Int y)
        {
            var neg = x.neg;
            if (x.neg != y.neg)
            { 
                // x - (-y) == x + y
                // (-x) - y == -(x + y)
                z.abs = z.abs.add(x.abs, y.abs);
            }
            else
            { 
                // x - y == x - y == -(y - x)
                // (-x) - (-y) == y - x == -(x - y)
                if (x.abs.cmp(y.abs) >= 0L)
                {
                    z.abs = z.abs.sub(x.abs, y.abs);
                }
                else
                {
                    neg = !neg;
                    z.abs = z.abs.sub(y.abs, x.abs);
                }
            }
            z.neg = len(z.abs) > 0L && neg; // 0 has no sign
            return z;
        }

        // Mul sets z to the product x*y and returns z.
        private static ref Int Mul(this ref Int z, ref Int x, ref Int y)
        { 
            // x * y == x * y
            // x * (-y) == -(x * y)
            // (-x) * y == -(x * y)
            // (-x) * (-y) == x * y
            if (x == y)
            {
                z.abs = z.abs.sqr(x.abs);
                z.neg = false;
                return z;
            }
            z.abs = z.abs.mul(x.abs, y.abs);
            z.neg = len(z.abs) > 0L && x.neg != y.neg; // 0 has no sign
            return z;
        }

        // MulRange sets z to the product of all integers
        // in the range [a, b] inclusively and returns z.
        // If a > b (empty range), the result is 1.
        private static ref Int MulRange(this ref Int z, long a, long b)
        {

            if (a > b) 
                return z.SetInt64(1L); // empty range
            else if (a <= 0L && b >= 0L) 
                return z.SetInt64(0L); // range includes 0
            // a <= b && (b < 0 || a > 0)

            var neg = false;
            if (a < 0L)
            {
                neg = (b - a) & 1L == 0L;
                a = -b;
                b = -a;
            }
            z.abs = z.abs.mulRange(uint64(a), uint64(b));
            z.neg = neg;
            return z;
        }

        // Binomial sets z to the binomial coefficient of (n, k) and returns z.
        private static ref Int Binomial(this ref Int z, long n, long k)
        { 
            // reduce the number of multiplications by reducing k
            if (n / 2L < k && k <= n)
            {
                k = n - k; // Binomial(n, k) == Binomial(n, n-k)
            }
            Int a = default;            Int b = default;

            a.MulRange(n - k + 1L, n);
            b.MulRange(1L, k);
            return z.Quo(ref a, ref b);
        }

        // Quo sets z to the quotient x/y for y != 0 and returns z.
        // If y == 0, a division-by-zero run-time panic occurs.
        // Quo implements truncated division (like Go); see QuoRem for more details.
        private static ref Int Quo(this ref Int z, ref Int x, ref Int y)
        {
            z.abs, _ = z.abs.div(null, x.abs, y.abs);
            z.neg = len(z.abs) > 0L && x.neg != y.neg; // 0 has no sign
            return z;
        }

        // Rem sets z to the remainder x%y for y != 0 and returns z.
        // If y == 0, a division-by-zero run-time panic occurs.
        // Rem implements truncated modulus (like Go); see QuoRem for more details.
        private static ref Int Rem(this ref Int z, ref Int x, ref Int y)
        {
            _, z.abs = nat(null).div(z.abs, x.abs, y.abs);
            z.neg = len(z.abs) > 0L && x.neg; // 0 has no sign
            return z;
        }

        // QuoRem sets z to the quotient x/y and r to the remainder x%y
        // and returns the pair (z, r) for y != 0.
        // If y == 0, a division-by-zero run-time panic occurs.
        //
        // QuoRem implements T-division and modulus (like Go):
        //
        //    q = x/y      with the result truncated to zero
        //    r = x - y*q
        //
        // (See Daan Leijen, ``Division and Modulus for Computer Scientists''.)
        // See DivMod for Euclidean division and modulus (unlike Go).
        //
        private static (ref Int, ref Int) QuoRem(this ref Int z, ref Int x, ref Int y, ref Int r)
        {
            z.abs, r.abs = z.abs.div(r.abs, x.abs, y.abs);
            z.neg = len(z.abs) > 0L && x.neg != y.neg;
            r.neg = len(r.abs) > 0L && x.neg; // 0 has no sign
            return (z, r);
        }

        // Div sets z to the quotient x/y for y != 0 and returns z.
        // If y == 0, a division-by-zero run-time panic occurs.
        // Div implements Euclidean division (unlike Go); see DivMod for more details.
        private static ref Int Div(this ref Int z, ref Int x, ref Int y)
        {
            var y_neg = y.neg; // z may be an alias for y
            Int r = default;
            z.QuoRem(x, y, ref r);
            if (r.neg)
            {
                if (y_neg)
                {
                    z.Add(z, intOne);
                }
                else
                {
                    z.Sub(z, intOne);
                }
            }
            return z;
        }

        // Mod sets z to the modulus x%y for y != 0 and returns z.
        // If y == 0, a division-by-zero run-time panic occurs.
        // Mod implements Euclidean modulus (unlike Go); see DivMod for more details.
        private static ref Int Mod(this ref Int z, ref Int x, ref Int y)
        {
            var y0 = y; // save y
            if (z == y || alias(z.abs, y.abs))
            {
                y0 = @new<Int>().Set(y);
            }
            Int q = default;
            q.QuoRem(x, y, z);
            if (z.neg)
            {
                if (y0.neg)
                {
                    z.Sub(z, y0);
                }
                else
                {
                    z.Add(z, y0);
                }
            }
            return z;
        }

        // DivMod sets z to the quotient x div y and m to the modulus x mod y
        // and returns the pair (z, m) for y != 0.
        // If y == 0, a division-by-zero run-time panic occurs.
        //
        // DivMod implements Euclidean division and modulus (unlike Go):
        //
        //    q = x div y  such that
        //    m = x - y*q  with 0 <= m < |y|
        //
        // (See Raymond T. Boute, ``The Euclidean definition of the functions
        // div and mod''. ACM Transactions on Programming Languages and
        // Systems (TOPLAS), 14(2):127-144, New York, NY, USA, 4/1992.
        // ACM press.)
        // See QuoRem for T-division and modulus (like Go).
        //
        private static (ref Int, ref Int) DivMod(this ref Int z, ref Int x, ref Int y, ref Int m)
        {
            var y0 = y; // save y
            if (z == y || alias(z.abs, y.abs))
            {
                y0 = @new<Int>().Set(y);
            }
            z.QuoRem(x, y, m);
            if (m.neg)
            {
                if (y0.neg)
                {
                    z.Add(z, intOne);
                    m.Sub(m, y0);
                }
                else
                {
                    z.Sub(z, intOne);
                    m.Add(m, y0);
                }
            }
            return (z, m);
        }

        // Cmp compares x and y and returns:
        //
        //   -1 if x <  y
        //    0 if x == y
        //   +1 if x >  y
        //
        private static long Cmp(this ref Int x, ref Int y)
        { 
            // x cmp y == x cmp y
            // x cmp (-y) == x
            // (-x) cmp y == y
            // (-x) cmp (-y) == -(x cmp y)

            if (x.neg == y.neg) 
                r = x.abs.cmp(y.abs);
                if (x.neg)
                {
                    r = -r;
                }
            else if (x.neg) 
                r = -1L;
            else 
                r = 1L;
                        return;
        }

        // CmpAbs compares the absolute values of x and y and returns:
        //
        //   -1 if |x| <  |y|
        //    0 if |x| == |y|
        //   +1 if |x| >  |y|
        //
        private static long CmpAbs(this ref Int x, ref Int y)
        {
            return x.abs.cmp(y.abs);
        }

        // low32 returns the least significant 32 bits of x.
        private static uint low32(nat x)
        {
            if (len(x) == 0L)
            {
                return 0L;
            }
            return uint32(x[0L]);
        }

        // low64 returns the least significant 64 bits of x.
        private static ulong low64(nat x)
        {
            if (len(x) == 0L)
            {
                return 0L;
            }
            var v = uint64(x[0L]);
            if (_W == 32L && len(x) > 1L)
            {
                return uint64(x[1L]) << (int)(32L) | v;
            }
            return v;
        }

        // Int64 returns the int64 representation of x.
        // If x cannot be represented in an int64, the result is undefined.
        private static long Int64(this ref Int x)
        {
            var v = int64(low64(x.abs));
            if (x.neg)
            {
                v = -v;
            }
            return v;
        }

        // Uint64 returns the uint64 representation of x.
        // If x cannot be represented in a uint64, the result is undefined.
        private static ulong Uint64(this ref Int x)
        {
            return low64(x.abs);
        }

        // IsInt64 reports whether x can be represented as an int64.
        private static bool IsInt64(this ref Int x)
        {
            if (len(x.abs) <= 64L / _W)
            {
                var w = int64(low64(x.abs));
                return w >= 0L || x.neg && w == -w;
            }
            return false;
        }

        // IsUint64 reports whether x can be represented as a uint64.
        private static bool IsUint64(this ref Int x)
        {
            return !x.neg && len(x.abs) <= 64L / _W;
        }

        // SetString sets z to the value of s, interpreted in the given base,
        // and returns z and a boolean indicating success. The entire string
        // (not just a prefix) must be valid for success. If SetString fails,
        // the value of z is undefined but the returned value is nil.
        //
        // The base argument must be 0 or a value between 2 and MaxBase. If the base
        // is 0, the string prefix determines the actual conversion base. A prefix of
        // ``0x'' or ``0X'' selects base 16; the ``0'' prefix selects base 8, and a
        // ``0b'' or ``0B'' prefix selects base 2. Otherwise the selected base is 10.
        //
        // For bases <= 36, lower and upper case letters are considered the same:
        // The letters 'a' to 'z' and 'A' to 'Z' represent digit values 10 to 35.
        // For bases > 36, the upper case letters 'A' to 'Z' represent the digit
        // values 36 to 61.
        //
        private static (ref Int, bool) SetString(this ref Int z, @string s, long @base)
        {
            return z.setFromScanner(strings.NewReader(s), base);
        }

        // setFromScanner implements SetString given an io.BytesScanner.
        // For documentation see comments of SetString.
        private static (ref Int, bool) setFromScanner(this ref Int z, io.ByteScanner r, long @base)
        {
            {
                var (_, _, err) = z.scan(r, base);

                if (err != null)
                {
                    return (null, false);
                } 
                // entire content must have been consumed

            } 
            // entire content must have been consumed
            {
                var (_, err) = r.ReadByte();

                if (err != io.EOF)
                {
                    return (null, false);
                }

            }
            return (z, true); // err == io.EOF => scan consumed all content of r
        }

        // SetBytes interprets buf as the bytes of a big-endian unsigned
        // integer, sets z to that value, and returns z.
        private static ref Int SetBytes(this ref Int z, slice<byte> buf)
        {
            z.abs = z.abs.setBytes(buf);
            z.neg = false;
            return z;
        }

        // Bytes returns the absolute value of x as a big-endian byte slice.
        private static slice<byte> Bytes(this ref Int x)
        {
            var buf = make_slice<byte>(len(x.abs) * _S);
            return buf[x.abs.bytes(buf)..];
        }

        // BitLen returns the length of the absolute value of x in bits.
        // The bit length of 0 is 0.
        private static long BitLen(this ref Int x)
        {
            return x.abs.bitLen();
        }

        // Exp sets z = x**y mod |m| (i.e. the sign of m is ignored), and returns z.
        // If y <= 0, the result is 1 mod |m|; if m == nil or m == 0, z = x**y.
        //
        // Modular exponentation of inputs of a particular size is not a
        // cryptographically constant-time operation.
        private static ref Int Exp(this ref Int z, ref Int x, ref Int y, ref Int m)
        { 
            // See Knuth, volume 2, section 4.6.3.
            nat yWords = default;
            if (!y.neg)
            {
                yWords = y.abs;
            } 
            // y >= 0
            nat mWords = default;
            if (m != null)
            {
                mWords = m.abs; // m.abs may be nil for m == 0
            }
            z.abs = z.abs.expNN(x.abs, yWords, mWords);
            z.neg = len(z.abs) > 0L && x.neg && len(yWords) > 0L && yWords[0L] & 1L == 1L; // 0 has no sign
            if (z.neg && len(mWords) > 0L)
            { 
                // make modulus result positive
                z.abs = z.abs.sub(mWords, z.abs); // z == x**y mod |m| && 0 <= z < |m|
                z.neg = false;
            }
            return z;
        }

        // GCD sets z to the greatest common divisor of a and b, which both must
        // be > 0, and returns z.
        // If x or y are not nil, GCD sets their value such that z = a*x + b*y.
        // If either a or b is <= 0, GCD sets z = x = y = 0.
        private static ref Int GCD(this ref Int z, ref Int x, ref Int y, ref Int a, ref Int b)
        {
            if (a.Sign() <= 0L || b.Sign() <= 0L)
            {
                z.SetInt64(0L);
                if (x != null)
                {
                    x.SetInt64(0L);
                }
                if (y != null)
                {
                    y.SetInt64(0L);
                }
                return z;
            }
            if (x == null && y == null)
            {
                return z.lehmerGCD(a, b);
            }
            ptr<Int> A = @new<Int>().Set(a);
            ptr<Int> B = @new<Int>().Set(b);

            ptr<Int> X = @new<Int>();
            ptr<Int> lastX = @new<Int>().SetInt64(1L);

            ptr<Int> q = @new<Int>();
            ptr<Int> temp = @new<Int>();

            ptr<Int> r = @new<Int>();
            while (len(B.abs) > 0L)
            {
                q, r = q.QuoRem(A, B, r);

                A = B;
                B = r;
                r = A;

                temp.Set(X);
                X.Mul(X, q);
                X.Sub(lastX, X);
                lastX.Set(temp);
            }


            if (x != null)
            {
                x.Value = lastX.Value;
            }
            if (y != null)
            { 
                // y = (z - a*x)/b
                y.Mul(a, lastX);
                y.Sub(A, y);
                y.Div(y, b);
            }
            z.Value = A.Value;
            return z;
        }

        // lehmerGCD sets z to the greatest common divisor of a and b,
        // which both must be > 0, and returns z.
        // See Knuth, The Art of Computer Programming, Vol. 2, Section 4.5.2, Algorithm L.
        // This implementation uses the improved condition by Collins requiring only one
        // quotient and avoiding the possibility of single Word overflow.
        // See Jebelean, "Improving the multiprecision Euclidean algorithm",
        // Design and Implementation of Symbolic Computation Systems, pp 45-58.
        private static ref Int lehmerGCD(this ref Int z, ref Int a, ref Int b)
        { 
            // ensure a >= b
            if (a.abs.cmp(b.abs) < 0L)
            {
                a = b;
                b = a;
            } 

            // don't destroy incoming values of a and b
            ptr<Int> B = @new<Int>().Set(b); // must be set first in case b is an alias of z
            var A = z.Set(a); 

            // temp variables for multiprecision update
            ptr<Int> t = @new<Int>();
            ptr<Int> r = @new<Int>();
            ptr<Int> s = @new<Int>();
            ptr<Int> w = @new<Int>(); 

            // loop invariant A >= B
            while (len(B.abs) > 1L)
            { 
                // initialize the digits
                Word a1 = default;                Word a2 = default;                Word u0 = default;                Word u1 = default;                Word u2 = default;                Word v0 = default;                Word v1 = default;                Word v2 = default;



                var m = len(B.abs); // m >= 2
                var n = len(A.abs); // n >= m >= 2

                // extract the top Word of bits from A and B
                var h = nlz(A.abs[n - 1L]);
                a1 = (A.abs[n - 1L] << (int)(h)) | (A.abs[n - 2L] >> (int)((_W - h))); 
                // B may have implicit zero words in the high bits if the lengths differ

                if (n == m) 
                    a2 = (B.abs[n - 1L] << (int)(h)) | (B.abs[n - 2L] >> (int)((_W - h)));
                else if (n == m + 1L) 
                    a2 = (B.abs[n - 2L] >> (int)((_W - h)));
                else 
                    a2 = 0L;
                // Since we are calculating with full words to avoid overflow,
                // we use 'even' to track the sign of the cosequences.
                // For even iterations: u0, v1 >= 0 && u1, v0 <= 0
                // For odd  iterations: u0, v1 <= 0 && u1, v0 >= 0
                // The first iteration starts with k=1 (odd).
                var even = false; 
                // variables to track the cosequences
                u0 = 0L;
                u1 = 1L;
                u2 = 0L;
                v0 = 0L;
                v1 = 0L;
                v2 = 1L; 

                // Calculate the quotient and cosequences using Collins' stopping condition.
                // Note that overflow of a Word is not possible when computing the remainder
                // sequence and cosequences since the cosequence size is bounded by the input size.
                // See section 4.2 of Jebelean for details.
                while (a2 >= v2 && a1 - a2 >= v1 + v2)
                {
                    var q = a1 / a2;
                    a1 = a2;
                    a2 = a1 - q * a2;
                    u0 = u1;
                    u1 = u2;
                    u2 = u1 + q * u2;
                    v0 = v1;
                    v1 = v2;
                    v2 = v1 + q * v2;
                    even = !even;
                } 

                // multiprecision step
 

                // multiprecision step
                if (v0 != 0L)
                { 
                    // simulate the effect of the single precision steps using the cosequences
                    // A = u0*A + v0*B
                    // B = u1*A + v1*B

                    t.abs = t.abs.setWord(u0);
                    s.abs = s.abs.setWord(v0);
                    t.neg = !even;
                    s.neg = even;

                    t.Mul(A, t);
                    s.Mul(B, s);

                    r.abs = r.abs.setWord(u1);
                    w.abs = w.abs.setWord(v1);
                    r.neg = even;
                    w.neg = !even;

                    r.Mul(A, r);
                    w.Mul(B, w);

                    A.Add(t, s);
                    B.Add(r, w);

                }
                else
                { 
                    // single-digit calculations failed to simluate any quotients
                    // do a standard Euclidean step
                    t.Rem(A, B);
                    A = B;
                    B = t;
                    t = A;
                }
            }


            if (len(B.abs) > 0L)
            { 
                // standard Euclidean algorithm base case for B a single Word
                if (len(A.abs) > 1L)
                { 
                    // A is longer than a single Word
                    t.Rem(A, B);
                    A = B;
                    B = t;
                    t = A;
                }
                if (len(B.abs) > 0L)
                { 
                    // A and B are both a single Word
                    a1 = A.abs[0L];
                    a2 = B.abs[0L];
                    while (a2 != 0L)
                    {
                        a1 = a2;
                        a2 = a1 % a2;
                    }

                    A.abs[0L] = a1;
                }
            }
            z.Value = A.Value;
            return z;
        }

        // Rand sets z to a pseudo-random number in [0, n) and returns z.
        //
        // As this uses the math/rand package, it must not be used for
        // security-sensitive work. Use crypto/rand.Int instead.
        private static ref Int Rand(this ref Int z, ref rand.Rand rnd, ref Int n)
        {
            z.neg = false;
            if (n.neg || len(n.abs) == 0L)
            {
                z.abs = null;
                return z;
            }
            z.abs = z.abs.random(rnd, n.abs, n.abs.bitLen());
            return z;
        }

        // ModInverse sets z to the multiplicative inverse of g in the ring ℤ/nℤ
        // and returns z. If g and n are not relatively prime, the result is undefined.
        private static ref Int ModInverse(this ref Int z, ref Int g, ref Int n)
        {
            if (g.neg)
            { 
                // GCD expects parameters a and b to be > 0.
                Int g2 = default;
                g = g2.Mod(g, n);
            }
            Int d = default;
            d.GCD(z, null, g, n); 
            // x and y are such that g*x + n*y = d. Since g and n are
            // relatively prime, d = 1. Taking that modulo n results in
            // g*x = 1, therefore x is the inverse element.
            if (z.neg)
            {
                z.Add(z, n);
            }
            return z;
        }

        // Jacobi returns the Jacobi symbol (x/y), either +1, -1, or 0.
        // The y argument must be an odd integer.
        public static long Jacobi(ref Int _x, ref Int _y) => func(_x, _y, (ref Int x, ref Int y, Defer _, Panic panic, Recover __) =>
        {
            if (len(y.abs) == 0L || y.abs[0L] & 1L == 0L)
            {
                panic(fmt.Sprintf("big: invalid 2nd argument to Int.Jacobi: need odd integer but got %s", y));
            } 

            // We use the formulation described in chapter 2, section 2.4,
            // "The Yacas Book of Algorithms":
            // http://yacas.sourceforge.net/Algo.book.pdf
            Int a = default;            Int b = default;            Int c = default;

            a.Set(x);
            b.Set(y);
            long j = 1L;

            if (b.neg)
            {
                if (a.neg)
                {
                    j = -1L;
                }
                b.neg = false;
            }
            while (true)
            {
                if (b.Cmp(intOne) == 0L)
                {
                    return j;
                }
                if (len(a.abs) == 0L)
                {
                    return 0L;
                }
                a.Mod(ref a, ref b);
                if (len(a.abs) == 0L)
                {
                    return 0L;
                } 
                // a > 0

                // handle factors of 2 in 'a'
                var s = a.abs.trailingZeroBits();
                if (s & 1L != 0L)
                {
                    var bmod8 = b.abs[0L] & 7L;
                    if (bmod8 == 3L || bmod8 == 5L)
                    {
                        j = -j;
                    }
                }
                c.Rsh(ref a, s); // a = 2^s*c

                // swap numerator and denominator
                if (b.abs[0L] & 3L == 3L && c.abs[0L] & 3L == 3L)
                {
                    j = -j;
                }
                a.Set(ref b);
                b.Set(ref c);
            }

        });

        // modSqrt3Mod4 uses the identity
        //      (a^((p+1)/4))^2  mod p
        //   == u^(p+1)          mod p
        //   == u^2              mod p
        // to calculate the square root of any quadratic residue mod p quickly for 3
        // mod 4 primes.
        private static ref Int modSqrt3Mod4Prime(this ref Int z, ref Int x, ref Int p)
        {
            ptr<Int> e = @new<Int>().Add(p, intOne); // e = p + 1
            e.Rsh(e, 2L); // e = (p + 1) / 4
            z.Exp(x, e, p); // z = x^e mod p
            return z;
        }

        // modSqrtTonelliShanks uses the Tonelli-Shanks algorithm to find the square
        // root of a quadratic residue modulo any prime.
        private static ref Int modSqrtTonelliShanks(this ref Int z, ref Int x, ref Int p)
        { 
            // Break p-1 into s*2^e such that s is odd.
            Int s = default;
            s.Sub(p, intOne);
            var e = s.abs.trailingZeroBits();
            s.Rsh(ref s, e); 

            // find some non-square n
            Int n = default;
            n.SetInt64(2L);
            while (Jacobi(ref n, p) != -1L)
            {
                n.Add(ref n, intOne);
            } 

            // Core of the Tonelli-Shanks algorithm. Follows the description in
            // section 6 of "Square roots from 1; 24, 51, 10 to Dan Shanks" by Ezra
            // Brown:
            // https://www.maa.org/sites/default/files/pdf/upload_library/22/Polya/07468342.di020786.02p0470a.pdf
 

            // Core of the Tonelli-Shanks algorithm. Follows the description in
            // section 6 of "Square roots from 1; 24, 51, 10 to Dan Shanks" by Ezra
            // Brown:
            // https://www.maa.org/sites/default/files/pdf/upload_library/22/Polya/07468342.di020786.02p0470a.pdf
            Int y = default;            Int b = default;            Int g = default;            Int t = default;

            y.Add(ref s, intOne);
            y.Rsh(ref y, 1L);
            y.Exp(x, ref y, p); // y = x^((s+1)/2)
            b.Exp(x, ref s, p); // b = x^s
            g.Exp(ref n, ref s, p); // g = n^s
            var r = e;
            while (true)
            { 
                // find the least m such that ord_p(b) = 2^m
                ulong m = default;
                t.Set(ref b);
                while (t.Cmp(intOne) != 0L)
                {
                    t.Mul(ref t, ref t).Mod(ref t, p);
                    m++;
                }


                if (m == 0L)
                {
                    return z.Set(ref y);
                }
                t.SetInt64(0L).SetBit(ref t, int(r - m - 1L), 1L).Exp(ref g, ref t, p); 
                // t = g^(2^(r-m-1)) mod p
                g.Mul(ref t, ref t).Mod(ref g, p); // g = g^(2^(r-m)) mod p
                y.Mul(ref y, ref t).Mod(ref y, p);
                b.Mul(ref b, ref g).Mod(ref b, p);
                r = m;
            }

        }

        // ModSqrt sets z to a square root of x mod p if such a square root exists, and
        // returns z. The modulus p must be an odd prime. If x is not a square mod p,
        // ModSqrt leaves z unchanged and returns nil. This function panics if p is
        // not an odd integer.
        private static ref Int ModSqrt(this ref Int z, ref Int x, ref Int p)
        {
            switch (Jacobi(x, p))
            {
                case -1L: 
                    return null; // x is not a square mod p
                    break;
                case 0L: 
                    return z.SetInt64(0L); // sqrt(0) mod p = 0
                    break;
                case 1L: 
                    break;
                    break;
            }
            if (x.neg || x.Cmp(p) >= 0L)
            { // ensure 0 <= x < p
                x = @new<Int>().Mod(x, p);
            } 

            // Check whether p is 3 mod 4, and if so, use the faster algorithm.
            if (len(p.abs) > 0L && p.abs[0L] % 4L == 3L)
            {
                return z.modSqrt3Mod4Prime(x, p);
            } 
            // Otherwise, use Tonelli-Shanks.
            return z.modSqrtTonelliShanks(x, p);
        }

        // Lsh sets z = x << n and returns z.
        private static ref Int Lsh(this ref Int z, ref Int x, ulong n)
        {
            z.abs = z.abs.shl(x.abs, n);
            z.neg = x.neg;
            return z;
        }

        // Rsh sets z = x >> n and returns z.
        private static ref Int Rsh(this ref Int z, ref Int x, ulong n)
        {
            if (x.neg)
            { 
                // (-x) >> s == ^(x-1) >> s == ^((x-1) >> s) == -(((x-1) >> s) + 1)
                var t = z.abs.sub(x.abs, natOne); // no underflow because |x| > 0
                t = t.shr(t, n);
                z.abs = t.add(t, natOne);
                z.neg = true; // z cannot be zero if x is negative
                return z;
            }
            z.abs = z.abs.shr(x.abs, n);
            z.neg = false;
            return z;
        }

        // Bit returns the value of the i'th bit of x. That is, it
        // returns (x>>i)&1. The bit index i must be >= 0.
        private static ulong Bit(this ref Int _x, long i) => func(_x, (ref Int x, Defer _, Panic panic, Recover __) =>
        {
            if (i == 0L)
            { 
                // optimization for common case: odd/even test of x
                if (len(x.abs) > 0L)
                {
                    return uint(x.abs[0L] & 1L); // bit 0 is same for -x
                }
                return 0L;
            }
            if (i < 0L)
            {
                panic("negative bit index");
            }
            if (x.neg)
            {
                var t = nat(null).sub(x.abs, natOne);
                return t.bit(uint(i)) ^ 1L;
            }
            return x.abs.bit(uint(i));
        });

        // SetBit sets z to x, with x's i'th bit set to b (0 or 1).
        // That is, if b is 1 SetBit sets z = x | (1 << i);
        // if b is 0 SetBit sets z = x &^ (1 << i). If b is not 0 or 1,
        // SetBit will panic.
        private static ref Int SetBit(this ref Int _z, ref Int _x, long i, ulong b) => func(_z, _x, (ref Int z, ref Int x, Defer _, Panic panic, Recover __) =>
        {
            if (i < 0L)
            {
                panic("negative bit index");
            }
            if (x.neg)
            {
                var t = z.abs.sub(x.abs, natOne);
                t = t.setBit(t, uint(i), b ^ 1L);
                z.abs = t.add(t, natOne);
                z.neg = len(z.abs) > 0L;
                return z;
            }
            z.abs = z.abs.setBit(x.abs, uint(i), b);
            z.neg = false;
            return z;
        });

        // And sets z = x & y and returns z.
        private static ref Int And(this ref Int z, ref Int x, ref Int y)
        {
            if (x.neg == y.neg)
            {
                if (x.neg)
                { 
                    // (-x) & (-y) == ^(x-1) & ^(y-1) == ^((x-1) | (y-1)) == -(((x-1) | (y-1)) + 1)
                    var x1 = nat(null).sub(x.abs, natOne);
                    var y1 = nat(null).sub(y.abs, natOne);
                    z.abs = z.abs.add(z.abs.or(x1, y1), natOne);
                    z.neg = true; // z cannot be zero if x and y are negative
                    return z;
                } 

                // x & y == x & y
                z.abs = z.abs.and(x.abs, y.abs);
                z.neg = false;
                return z;
            } 

            // x.neg != y.neg
            if (x.neg)
            {
                x = y;
                y = x; // & is symmetric
            } 

            // x & (-y) == x & ^(y-1) == x &^ (y-1)
            y1 = nat(null).sub(y.abs, natOne);
            z.abs = z.abs.andNot(x.abs, y1);
            z.neg = false;
            return z;
        }

        // AndNot sets z = x &^ y and returns z.
        private static ref Int AndNot(this ref Int z, ref Int x, ref Int y)
        {
            if (x.neg == y.neg)
            {
                if (x.neg)
                { 
                    // (-x) &^ (-y) == ^(x-1) &^ ^(y-1) == ^(x-1) & (y-1) == (y-1) &^ (x-1)
                    var x1 = nat(null).sub(x.abs, natOne);
                    var y1 = nat(null).sub(y.abs, natOne);
                    z.abs = z.abs.andNot(y1, x1);
                    z.neg = false;
                    return z;
                } 

                // x &^ y == x &^ y
                z.abs = z.abs.andNot(x.abs, y.abs);
                z.neg = false;
                return z;
            }
            if (x.neg)
            { 
                // (-x) &^ y == ^(x-1) &^ y == ^(x-1) & ^y == ^((x-1) | y) == -(((x-1) | y) + 1)
                x1 = nat(null).sub(x.abs, natOne);
                z.abs = z.abs.add(z.abs.or(x1, y.abs), natOne);
                z.neg = true; // z cannot be zero if x is negative and y is positive
                return z;
            } 

            // x &^ (-y) == x &^ ^(y-1) == x & (y-1)
            y1 = nat(null).sub(y.abs, natOne);
            z.abs = z.abs.and(x.abs, y1);
            z.neg = false;
            return z;
        }

        // Or sets z = x | y and returns z.
        private static ref Int Or(this ref Int z, ref Int x, ref Int y)
        {
            if (x.neg == y.neg)
            {
                if (x.neg)
                { 
                    // (-x) | (-y) == ^(x-1) | ^(y-1) == ^((x-1) & (y-1)) == -(((x-1) & (y-1)) + 1)
                    var x1 = nat(null).sub(x.abs, natOne);
                    var y1 = nat(null).sub(y.abs, natOne);
                    z.abs = z.abs.add(z.abs.and(x1, y1), natOne);
                    z.neg = true; // z cannot be zero if x and y are negative
                    return z;
                } 

                // x | y == x | y
                z.abs = z.abs.or(x.abs, y.abs);
                z.neg = false;
                return z;
            } 

            // x.neg != y.neg
            if (x.neg)
            {
                x = y;
                y = x; // | is symmetric
            } 

            // x | (-y) == x | ^(y-1) == ^((y-1) &^ x) == -(^((y-1) &^ x) + 1)
            y1 = nat(null).sub(y.abs, natOne);
            z.abs = z.abs.add(z.abs.andNot(y1, x.abs), natOne);
            z.neg = true; // z cannot be zero if one of x or y is negative
            return z;
        }

        // Xor sets z = x ^ y and returns z.
        private static ref Int Xor(this ref Int z, ref Int x, ref Int y)
        {
            if (x.neg == y.neg)
            {
                if (x.neg)
                { 
                    // (-x) ^ (-y) == ^(x-1) ^ ^(y-1) == (x-1) ^ (y-1)
                    var x1 = nat(null).sub(x.abs, natOne);
                    var y1 = nat(null).sub(y.abs, natOne);
                    z.abs = z.abs.xor(x1, y1);
                    z.neg = false;
                    return z;
                } 

                // x ^ y == x ^ y
                z.abs = z.abs.xor(x.abs, y.abs);
                z.neg = false;
                return z;
            } 

            // x.neg != y.neg
            if (x.neg)
            {
                x = y;
                y = x; // ^ is symmetric
            } 

            // x ^ (-y) == x ^ ^(y-1) == ^(x ^ (y-1)) == -((x ^ (y-1)) + 1)
            y1 = nat(null).sub(y.abs, natOne);
            z.abs = z.abs.add(z.abs.xor(x.abs, y1), natOne);
            z.neg = true; // z cannot be zero if only one of x or y is negative
            return z;
        }

        // Not sets z = ^x and returns z.
        private static ref Int Not(this ref Int z, ref Int x)
        {
            if (x.neg)
            { 
                // ^(-x) == ^(^(x-1)) == x-1
                z.abs = z.abs.sub(x.abs, natOne);
                z.neg = false;
                return z;
            } 

            // ^x == -x-1 == -(x+1)
            z.abs = z.abs.add(x.abs, natOne);
            z.neg = true; // z cannot be zero if x is positive
            return z;
        }

        // Sqrt sets z to ⌊√x⌋, the largest integer such that z² ≤ x, and returns z.
        // It panics if x is negative.
        private static ref Int Sqrt(this ref Int _z, ref Int _x) => func(_z, _x, (ref Int z, ref Int x, Defer _, Panic panic, Recover __) =>
        {
            if (x.neg)
            {
                panic("square root of negative number");
            }
            z.neg = false;
            z.abs = z.abs.sqrt(x.abs);
            return z;
        });
    }
}}
