// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements multi-precision decimal numbers.
// The implementation is for float to decimal conversion only;
// not general purpose use.
// The only operations are precise conversion from binary to
// decimal and rounding.
//
// The key observation and some code (shr) is borrowed from
// strconv/decimal.go: conversion of binary fractional values can be done
// precisely in multi-precision decimal because 2 divides 10 (required for
// >> of mantissa); but conversion of decimal floating-point values cannot
// be done precisely in binary representation.
//
// In contrast to strconv/decimal.go, only right shift is implemented in
// decimal format - left shift can be done precisely in binary format.

// package big -- go2cs converted at 2020 August 29 08:29:00 UTC
// import "math/big" ==> using big = go.math.big_package
// Original source: C:\Go\src\math\big\decimal.go

using static go.builtin;

namespace go {
namespace math
{
    public static partial class big_package
    {
        // A decimal represents an unsigned floating-point number in decimal representation.
        // The value of a non-zero decimal d is d.mant * 10**d.exp with 0.1 <= d.mant < 1,
        // with the most-significant mantissa digit at index 0. For the zero decimal, the
        // mantissa length and exponent are 0.
        // The zero value for decimal represents a ready-to-use 0.0.
        private partial struct @decimal
        {
            public slice<byte> mant; // mantissa ASCII digits, big-endian
            public long exp; // exponent
        }

        // at returns the i'th mantissa digit, starting with the most significant digit at 0.
        private static byte at(this ref decimal d, long i)
        {
            if (0L <= i && i < len(d.mant))
            {
                return d.mant[i];
            }
            return '0';
        }

        // Maximum shift amount that can be done in one pass without overflow.
        // A Word has _W bits and (1<<maxShift - 1)*10 + 9 must fit into Word.
        private static readonly var maxShift = _W - 4L;

        // TODO(gri) Since we know the desired decimal precision when converting
        // a floating-point number, we may be able to limit the number of decimal
        // digits that need to be computed by init by providing an additional
        // precision argument and keeping track of when a number was truncated early
        // (equivalent of "sticky bit" in binary rounding).

        // TODO(gri) Along the same lines, enforce some limit to shift magnitudes
        // to avoid "infinitely" long running conversions (until we run out of space).

        // Init initializes x to the decimal representation of m << shift (for
        // shift >= 0), or m >> -shift (for shift < 0).


        // TODO(gri) Since we know the desired decimal precision when converting
        // a floating-point number, we may be able to limit the number of decimal
        // digits that need to be computed by init by providing an additional
        // precision argument and keeping track of when a number was truncated early
        // (equivalent of "sticky bit" in binary rounding).

        // TODO(gri) Along the same lines, enforce some limit to shift magnitudes
        // to avoid "infinitely" long running conversions (until we run out of space).

        // Init initializes x to the decimal representation of m << shift (for
        // shift >= 0), or m >> -shift (for shift < 0).
        private static void init(this ref decimal x, nat m, long shift)
        { 
            // special case 0
            if (len(m) == 0L)
            {
                x.mant = x.mant[..0L];
                x.exp = 0L;
                return;
            } 

            // Optimization: If we need to shift right, first remove any trailing
            // zero bits from m to reduce shift amount that needs to be done in
            // decimal format (since that is likely slower).
            if (shift < 0L)
            {
                var ntz = m.trailingZeroBits();
                var s = uint(-shift);
                if (s >= ntz)
                {
                    s = ntz; // shift at most ntz bits
                }
                m = nat(null).shr(m, s);
                shift += int(s);
            } 

            // Do any shift left in binary representation.
            if (shift > 0L)
            {
                m = nat(null).shl(m, uint(shift));
                shift = 0L;
            } 

            // Convert mantissa into decimal representation.
            s = m.utoa(10L);
            var n = len(s);
            x.exp = n; 
            // Trim trailing zeros; instead the exponent is tracking
            // the decimal point independent of the number of digits.
            while (n > 0L && s[n - 1L] == '0')
            {
                n--;
            }

            x.mant = append(x.mant[..0L], s[..n]); 

            // Do any (remaining) shift right in decimal representation.
            if (shift < 0L)
            {
                while (shift < -maxShift)
                {
                    shr(x, maxShift);
                    shift += maxShift;
                }

                shr(x, uint(-shift));
            }
        }

        // shr implements x >> s, for s <= maxShift.
        private static void shr(ref decimal x, ulong s)
        { 
            // Division by 1<<s using shift-and-subtract algorithm.

            // pick up enough leading digits to cover first shift
            long r = 0L; // read index
            Word n = default;
            while (n >> (int)(s) == 0L && r < len(x.mant))
            {
                var ch = Word(x.mant[r]);
                r++;
                n = n * 10L + ch - '0';
            }

            if (n == 0L)
            { 
                // x == 0; shouldn't get here, but handle anyway
                x.mant = x.mant[..0L];
                return;
            }
            while (n >> (int)(s) == 0L)
            {
                r++;
                n *= 10L;
            }

            x.exp += 1L - r; 

            // read a digit, write a digit
            long w = 0L; // write index
            var mask = Word(1L) << (int)(s) - 1L;
            while (r < len(x.mant))
            {
                ch = Word(x.mant[r]);
                r++;
                var d = n >> (int)(s);
                n &= mask; // n -= d << s
                x.mant[w] = byte(d + '0');
                w++;
                n = n * 10L + ch - '0';
            } 

            // write extra digits that still fit
 

            // write extra digits that still fit
            while (n > 0L && w < len(x.mant))
            {
                d = n >> (int)(s);
                n &= mask;
                x.mant[w] = byte(d + '0');
                w++;
                n = n * 10L;
            }

            x.mant = x.mant[..w]; // the number may be shorter (e.g. 1024 >> 10)

            // append additional digits that didn't fit
            while (n > 0L)
            {
                d = n >> (int)(s);
                n &= mask;
                x.mant = append(x.mant, byte(d + '0'));
                n = n * 10L;
            }


            trim(x);
        }

        private static @string String(this ref decimal x)
        {
            if (len(x.mant) == 0L)
            {
                return "0";
            }
            slice<byte> buf = default;

            if (x.exp <= 0L) 
                // 0.00ddd
                buf = append(buf, "0.");
                buf = appendZeros(buf, -x.exp);
                buf = append(buf, x.mant);
            else if (x.exp < len(x.mant)) 
                // dd.ddd
                buf = append(buf, x.mant[..x.exp]);
                buf = append(buf, '.');
                buf = append(buf, x.mant[x.exp..]);
            else // len(x.mant) <= x.exp
                // ddd00
                buf = append(buf, x.mant);
                buf = appendZeros(buf, x.exp - len(x.mant));
                        return string(buf);
        }

        // appendZeros appends n 0 digits to buf and returns buf.
        private static slice<byte> appendZeros(slice<byte> buf, long n)
        {
            while (n > 0L)
            {
                buf = append(buf, '0');
                n--;
            }

            return buf;
        }

        // shouldRoundUp reports if x should be rounded up
        // if shortened to n digits. n must be a valid index
        // for x.mant.
        private static bool shouldRoundUp(ref decimal x, long n)
        {
            if (x.mant[n] == '5' && n + 1L == len(x.mant))
            { 
                // exactly halfway - round to even
                return n > 0L && (x.mant[n - 1L] - '0') & 1L != 0L;
            } 
            // not halfway - digit tells all (x.mant has no trailing zeros)
            return x.mant[n] >= '5';
        }

        // round sets x to (at most) n mantissa digits by rounding it
        // to the nearest even value with n (or fever) mantissa digits.
        // If n < 0, x remains unchanged.
        private static void round(this ref decimal x, long n)
        {
            if (n < 0L || n >= len(x.mant))
            {
                return; // nothing to do
            }
            if (shouldRoundUp(x, n))
            {
                x.roundUp(n);
            }
            else
            {
                x.roundDown(n);
            }
        }

        private static void roundUp(this ref decimal x, long n)
        {
            if (n < 0L || n >= len(x.mant))
            {
                return; // nothing to do
            } 
            // 0 <= n < len(x.mant)

            // find first digit < '9'
            while (n > 0L && x.mant[n - 1L] >= '9')
            {
                n--;
            }


            if (n == 0L)
            { 
                // all digits are '9's => round up to '1' and update exponent
                x.mant[0L] = '1'; // ok since len(x.mant) > n
                x.mant = x.mant[..1L];
                x.exp++;
                return;
            } 

            // n > 0 && x.mant[n-1] < '9'
            x.mant[n - 1L]++;
            x.mant = x.mant[..n]; 
            // x already trimmed
        }

        private static void roundDown(this ref decimal x, long n)
        {
            if (n < 0L || n >= len(x.mant))
            {
                return; // nothing to do
            }
            x.mant = x.mant[..n];
            trim(x);
        }

        // trim cuts off any trailing zeros from x's mantissa;
        // they are meaningless for the value of x.
        private static void trim(ref decimal x)
        {
            var i = len(x.mant);
            while (i > 0L && x.mant[i - 1L] == '0')
            {
                i--;
            }

            x.mant = x.mant[..i];
            if (i == 0L)
            {
                x.exp = 0L;
            }
        }
    }
}}
