// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:generate go run make_tables.go

// Package bits implements bit counting and manipulation
// functions for the predeclared unsigned integer types.
// package bits -- go2cs converted at 2020 October 08 03:25:15 UTC
// import "math/bits" ==> using bits = go.math.bits_package
// Original source: C:\Go\src\math\bits\bits.go

using static go.builtin;

namespace go {
namespace math
{
    public static partial class bits_package
    {
        private static readonly long uintSize = (long)32L << (int)((~uint(0L) >> (int)(32L) & 1L)); // 32 or 64

        // UintSize is the size of a uint in bits.
 // 32 or 64

        // UintSize is the size of a uint in bits.
        public static readonly var UintSize = (var)uintSize;

        // --- LeadingZeros ---

        // LeadingZeros returns the number of leading zero bits in x; the result is UintSize for x == 0.


        // --- LeadingZeros ---

        // LeadingZeros returns the number of leading zero bits in x; the result is UintSize for x == 0.
        public static long LeadingZeros(ulong x)
        {
            return UintSize - Len(x);
        }

        // LeadingZeros8 returns the number of leading zero bits in x; the result is 8 for x == 0.
        public static long LeadingZeros8(byte x)
        {
            return 8L - Len8(x);
        }

        // LeadingZeros16 returns the number of leading zero bits in x; the result is 16 for x == 0.
        public static long LeadingZeros16(ushort x)
        {
            return 16L - Len16(x);
        }

        // LeadingZeros32 returns the number of leading zero bits in x; the result is 32 for x == 0.
        public static long LeadingZeros32(uint x)
        {
            return 32L - Len32(x);
        }

        // LeadingZeros64 returns the number of leading zero bits in x; the result is 64 for x == 0.
        public static long LeadingZeros64(ulong x)
        {
            return 64L - Len64(x);
        }

        // --- TrailingZeros ---

        // See http://supertech.csail.mit.edu/papers/debruijn.pdf
        private static readonly ulong deBruijn32 = (ulong)0x077CB531UL;



        private static array<byte> deBruijn32tab = new array<byte>(new byte[] { 0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8, 31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9 });

        private static readonly ulong deBruijn64 = (ulong)0x03f79d71b4ca8b09UL;



        private static array<byte> deBruijn64tab = new array<byte>(new byte[] { 0, 1, 56, 2, 57, 49, 28, 3, 61, 58, 42, 50, 38, 29, 17, 4, 62, 47, 59, 36, 45, 43, 51, 22, 53, 39, 33, 30, 24, 18, 12, 5, 63, 55, 48, 27, 60, 41, 37, 16, 46, 35, 44, 21, 52, 32, 23, 11, 54, 26, 40, 15, 34, 20, 31, 10, 25, 14, 19, 9, 13, 8, 7, 6 });

        // TrailingZeros returns the number of trailing zero bits in x; the result is UintSize for x == 0.
        public static long TrailingZeros(ulong x)
        {
            if (UintSize == 32L)
            {
                return TrailingZeros32(uint32(x));
            }

            return TrailingZeros64(uint64(x));

        }

        // TrailingZeros8 returns the number of trailing zero bits in x; the result is 8 for x == 0.
        public static long TrailingZeros8(byte x)
        {
            return int(ntz8tab[x]);
        }

        // TrailingZeros16 returns the number of trailing zero bits in x; the result is 16 for x == 0.
        public static long TrailingZeros16(ushort x)
        {
            if (x == 0L)
            {
                return 16L;
            } 
            // see comment in TrailingZeros64
            return int(deBruijn32tab[uint32(x & -x) * deBruijn32 >> (int)((32L - 5L))]);

        }

        // TrailingZeros32 returns the number of trailing zero bits in x; the result is 32 for x == 0.
        public static long TrailingZeros32(uint x)
        {
            if (x == 0L)
            {
                return 32L;
            } 
            // see comment in TrailingZeros64
            return int(deBruijn32tab[(x & -x) * deBruijn32 >> (int)((32L - 5L))]);

        }

        // TrailingZeros64 returns the number of trailing zero bits in x; the result is 64 for x == 0.
        public static long TrailingZeros64(ulong x)
        {
            if (x == 0L)
            {
                return 64L;
            } 
            // If popcount is fast, replace code below with return popcount(^x & (x - 1)).
            //
            // x & -x leaves only the right-most bit set in the word. Let k be the
            // index of that bit. Since only a single bit is set, the value is two
            // to the power of k. Multiplying by a power of two is equivalent to
            // left shifting, in this case by k bits. The de Bruijn (64 bit) constant
            // is such that all six bit, consecutive substrings are distinct.
            // Therefore, if we have a left shifted version of this constant we can
            // find by how many bits it was shifted by looking at which six bit
            // substring ended up at the top of the word.
            // (Knuth, volume 4, section 7.3.1)
            return int(deBruijn64tab[(x & -x) * deBruijn64 >> (int)((64L - 6L))]);

        }

        // --- OnesCount ---

        private static readonly ulong m0 = (ulong)0x5555555555555555UL; // 01010101 ...
 // 01010101 ...
        private static readonly ulong m1 = (ulong)0x3333333333333333UL; // 00110011 ...
 // 00110011 ...
        private static readonly ulong m2 = (ulong)0x0f0f0f0f0f0f0f0fUL; // 00001111 ...
 // 00001111 ...
        private static readonly ulong m3 = (ulong)0x00ff00ff00ff00ffUL; // etc.
 // etc.
        private static readonly ulong m4 = (ulong)0x0000ffff0000ffffUL;

        // OnesCount returns the number of one bits ("population count") in x.


        // OnesCount returns the number of one bits ("population count") in x.
        public static long OnesCount(ulong x)
        {
            if (UintSize == 32L)
            {
                return OnesCount32(uint32(x));
            }

            return OnesCount64(uint64(x));

        }

        // OnesCount8 returns the number of one bits ("population count") in x.
        public static long OnesCount8(byte x)
        {
            return int(pop8tab[x]);
        }

        // OnesCount16 returns the number of one bits ("population count") in x.
        public static long OnesCount16(ushort x)
        {
            return int(pop8tab[x >> (int)(8L)] + pop8tab[x & 0xffUL]);
        }

        // OnesCount32 returns the number of one bits ("population count") in x.
        public static long OnesCount32(uint x)
        {
            return int(pop8tab[x >> (int)(24L)] + pop8tab[x >> (int)(16L) & 0xffUL] + pop8tab[x >> (int)(8L) & 0xffUL] + pop8tab[x & 0xffUL]);
        }

        // OnesCount64 returns the number of one bits ("population count") in x.
        public static long OnesCount64(ulong x)
        { 
            // Implementation: Parallel summing of adjacent bits.
            // See "Hacker's Delight", Chap. 5: Counting Bits.
            // The following pattern shows the general approach:
            //
            //   x = x>>1&(m0&m) + x&(m0&m)
            //   x = x>>2&(m1&m) + x&(m1&m)
            //   x = x>>4&(m2&m) + x&(m2&m)
            //   x = x>>8&(m3&m) + x&(m3&m)
            //   x = x>>16&(m4&m) + x&(m4&m)
            //   x = x>>32&(m5&m) + x&(m5&m)
            //   return int(x)
            //
            // Masking (& operations) can be left away when there's no
            // danger that a field's sum will carry over into the next
            // field: Since the result cannot be > 64, 8 bits is enough
            // and we can ignore the masks for the shifts by 8 and up.
            // Per "Hacker's Delight", the first line can be simplified
            // more, but it saves at best one instruction, so we leave
            // it alone for clarity.
            const long m = (long)1L << (int)(64L) - 1L;

            x = x >> (int)(1L) & (m0 & m) + x & (m0 & m);
            x = x >> (int)(2L) & (m1 & m) + x & (m1 & m);
            x = (x >> (int)(4L) + x) & (m2 & m);
            x += x >> (int)(8L);
            x += x >> (int)(16L);
            x += x >> (int)(32L);
            return int(x) & (1L << (int)(7L) - 1L);

        }

        // --- RotateLeft ---

        // RotateLeft returns the value of x rotated left by (k mod UintSize) bits.
        // To rotate x right by k bits, call RotateLeft(x, -k).
        //
        // This function's execution time does not depend on the inputs.
        public static ulong RotateLeft(ulong x, long k)
        {
            if (UintSize == 32L)
            {
                return uint(RotateLeft32(uint32(x), k));
            }

            return uint(RotateLeft64(uint64(x), k));

        }

        // RotateLeft8 returns the value of x rotated left by (k mod 8) bits.
        // To rotate x right by k bits, call RotateLeft8(x, -k).
        //
        // This function's execution time does not depend on the inputs.
        public static byte RotateLeft8(byte x, long k)
        {
            const long n = (long)8L;

            var s = uint(k) & (n - 1L);
            return x << (int)(s) | x >> (int)((n - s));
        }

        // RotateLeft16 returns the value of x rotated left by (k mod 16) bits.
        // To rotate x right by k bits, call RotateLeft16(x, -k).
        //
        // This function's execution time does not depend on the inputs.
        public static ushort RotateLeft16(ushort x, long k)
        {
            const long n = (long)16L;

            var s = uint(k) & (n - 1L);
            return x << (int)(s) | x >> (int)((n - s));
        }

        // RotateLeft32 returns the value of x rotated left by (k mod 32) bits.
        // To rotate x right by k bits, call RotateLeft32(x, -k).
        //
        // This function's execution time does not depend on the inputs.
        public static uint RotateLeft32(uint x, long k)
        {
            const long n = (long)32L;

            var s = uint(k) & (n - 1L);
            return x << (int)(s) | x >> (int)((n - s));
        }

        // RotateLeft64 returns the value of x rotated left by (k mod 64) bits.
        // To rotate x right by k bits, call RotateLeft64(x, -k).
        //
        // This function's execution time does not depend on the inputs.
        public static ulong RotateLeft64(ulong x, long k)
        {
            const long n = (long)64L;

            var s = uint(k) & (n - 1L);
            return x << (int)(s) | x >> (int)((n - s));
        }

        // --- Reverse ---

        // Reverse returns the value of x with its bits in reversed order.
        public static ulong Reverse(ulong x)
        {
            if (UintSize == 32L)
            {
                return uint(Reverse32(uint32(x)));
            }

            return uint(Reverse64(uint64(x)));

        }

        // Reverse8 returns the value of x with its bits in reversed order.
        public static byte Reverse8(byte x)
        {
            return rev8tab[x];
        }

        // Reverse16 returns the value of x with its bits in reversed order.
        public static ushort Reverse16(ushort x)
        {
            return uint16(rev8tab[x >> (int)(8L)]) | uint16(rev8tab[x & 0xffUL]) << (int)(8L);
        }

        // Reverse32 returns the value of x with its bits in reversed order.
        public static uint Reverse32(uint x)
        {
            const long m = (long)1L << (int)(32L) - 1L;

            x = x >> (int)(1L) & (m0 & m) | x & (m0 & m) << (int)(1L);
            x = x >> (int)(2L) & (m1 & m) | x & (m1 & m) << (int)(2L);
            x = x >> (int)(4L) & (m2 & m) | x & (m2 & m) << (int)(4L);
            return ReverseBytes32(x);
        }

        // Reverse64 returns the value of x with its bits in reversed order.
        public static ulong Reverse64(ulong x)
        {
            const long m = (long)1L << (int)(64L) - 1L;

            x = x >> (int)(1L) & (m0 & m) | x & (m0 & m) << (int)(1L);
            x = x >> (int)(2L) & (m1 & m) | x & (m1 & m) << (int)(2L);
            x = x >> (int)(4L) & (m2 & m) | x & (m2 & m) << (int)(4L);
            return ReverseBytes64(x);
        }

        // --- ReverseBytes ---

        // ReverseBytes returns the value of x with its bytes in reversed order.
        //
        // This function's execution time does not depend on the inputs.
        public static ulong ReverseBytes(ulong x)
        {
            if (UintSize == 32L)
            {
                return uint(ReverseBytes32(uint32(x)));
            }

            return uint(ReverseBytes64(uint64(x)));

        }

        // ReverseBytes16 returns the value of x with its bytes in reversed order.
        //
        // This function's execution time does not depend on the inputs.
        public static ushort ReverseBytes16(ushort x)
        {
            return x >> (int)(8L) | x << (int)(8L);
        }

        // ReverseBytes32 returns the value of x with its bytes in reversed order.
        //
        // This function's execution time does not depend on the inputs.
        public static uint ReverseBytes32(uint x)
        {
            const long m = (long)1L << (int)(32L) - 1L;

            x = x >> (int)(8L) & (m3 & m) | x & (m3 & m) << (int)(8L);
            return x >> (int)(16L) | x << (int)(16L);
        }

        // ReverseBytes64 returns the value of x with its bytes in reversed order.
        //
        // This function's execution time does not depend on the inputs.
        public static ulong ReverseBytes64(ulong x)
        {
            const long m = (long)1L << (int)(64L) - 1L;

            x = x >> (int)(8L) & (m3 & m) | x & (m3 & m) << (int)(8L);
            x = x >> (int)(16L) & (m4 & m) | x & (m4 & m) << (int)(16L);
            return x >> (int)(32L) | x << (int)(32L);
        }

        // --- Len ---

        // Len returns the minimum number of bits required to represent x; the result is 0 for x == 0.
        public static long Len(ulong x)
        {
            if (UintSize == 32L)
            {
                return Len32(uint32(x));
            }

            return Len64(uint64(x));

        }

        // Len8 returns the minimum number of bits required to represent x; the result is 0 for x == 0.
        public static long Len8(byte x)
        {
            return int(len8tab[x]);
        }

        // Len16 returns the minimum number of bits required to represent x; the result is 0 for x == 0.
        public static long Len16(ushort x)
        {
            long n = default;

            if (x >= 1L << (int)(8L))
            {
                x >>= 8L;
                n = 8L;
            }

            return n + int(len8tab[x]);

        }

        // Len32 returns the minimum number of bits required to represent x; the result is 0 for x == 0.
        public static long Len32(uint x)
        {
            long n = default;

            if (x >= 1L << (int)(16L))
            {
                x >>= 16L;
                n = 16L;
            }

            if (x >= 1L << (int)(8L))
            {
                x >>= 8L;
                n += 8L;
            }

            return n + int(len8tab[x]);

        }

        // Len64 returns the minimum number of bits required to represent x; the result is 0 for x == 0.
        public static long Len64(ulong x)
        {
            long n = default;

            if (x >= 1L << (int)(32L))
            {
                x >>= 32L;
                n = 32L;
            }

            if (x >= 1L << (int)(16L))
            {
                x >>= 16L;
                n += 16L;
            }

            if (x >= 1L << (int)(8L))
            {
                x >>= 8L;
                n += 8L;
            }

            return n + int(len8tab[x]);

        }

        // --- Add with carry ---

        // Add returns the sum with carry of x, y and carry: sum = x + y + carry.
        // The carry input must be 0 or 1; otherwise the behavior is undefined.
        // The carryOut output is guaranteed to be 0 or 1.
        //
        // This function's execution time does not depend on the inputs.
        public static (ulong, ulong) Add(ulong x, ulong y, ulong carry)
        {
            ulong sum = default;
            ulong carryOut = default;

            if (UintSize == 32L)
            {
                var (s32, c32) = Add32(uint32(x), uint32(y), uint32(carry));
                return (uint(s32), uint(c32));
            }

            var (s64, c64) = Add64(uint64(x), uint64(y), uint64(carry));
            return (uint(s64), uint(c64));

        }

        // Add32 returns the sum with carry of x, y and carry: sum = x + y + carry.
        // The carry input must be 0 or 1; otherwise the behavior is undefined.
        // The carryOut output is guaranteed to be 0 or 1.
        //
        // This function's execution time does not depend on the inputs.
        public static (uint, uint) Add32(uint x, uint y, uint carry)
        {
            uint sum = default;
            uint carryOut = default;

            var sum64 = uint64(x) + uint64(y) + uint64(carry);
            sum = uint32(sum64);
            carryOut = uint32(sum64 >> (int)(32L));
            return ;
        }

        // Add64 returns the sum with carry of x, y and carry: sum = x + y + carry.
        // The carry input must be 0 or 1; otherwise the behavior is undefined.
        // The carryOut output is guaranteed to be 0 or 1.
        //
        // This function's execution time does not depend on the inputs.
        public static (ulong, ulong) Add64(ulong x, ulong y, ulong carry)
        {
            ulong sum = default;
            ulong carryOut = default;

            sum = x + y + carry; 
            // The sum will overflow if both top bits are set (x & y) or if one of them
            // is (x | y), and a carry from the lower place happened. If such a carry
            // happens, the top bit will be 1 + 0 + 1 = 0 (&^ sum).
            carryOut = ((x & y) | ((x | y) & ~sum)) >> (int)(63L);
            return ;

        }

        // --- Subtract with borrow ---

        // Sub returns the difference of x, y and borrow: diff = x - y - borrow.
        // The borrow input must be 0 or 1; otherwise the behavior is undefined.
        // The borrowOut output is guaranteed to be 0 or 1.
        //
        // This function's execution time does not depend on the inputs.
        public static (ulong, ulong) Sub(ulong x, ulong y, ulong borrow)
        {
            ulong diff = default;
            ulong borrowOut = default;

            if (UintSize == 32L)
            {
                var (d32, b32) = Sub32(uint32(x), uint32(y), uint32(borrow));
                return (uint(d32), uint(b32));
            }

            var (d64, b64) = Sub64(uint64(x), uint64(y), uint64(borrow));
            return (uint(d64), uint(b64));

        }

        // Sub32 returns the difference of x, y and borrow, diff = x - y - borrow.
        // The borrow input must be 0 or 1; otherwise the behavior is undefined.
        // The borrowOut output is guaranteed to be 0 or 1.
        //
        // This function's execution time does not depend on the inputs.
        public static (uint, uint) Sub32(uint x, uint y, uint borrow)
        {
            uint diff = default;
            uint borrowOut = default;

            diff = x - y - borrow; 
            // The difference will underflow if the top bit of x is not set and the top
            // bit of y is set (^x & y) or if they are the same (^(x ^ y)) and a borrow
            // from the lower place happens. If that borrow happens, the result will be
            // 1 - 1 - 1 = 0 - 0 - 1 = 1 (& diff).
            borrowOut = ((~x & y) | (~(x ^ y) & diff)) >> (int)(31L);
            return ;

        }

        // Sub64 returns the difference of x, y and borrow: diff = x - y - borrow.
        // The borrow input must be 0 or 1; otherwise the behavior is undefined.
        // The borrowOut output is guaranteed to be 0 or 1.
        //
        // This function's execution time does not depend on the inputs.
        public static (ulong, ulong) Sub64(ulong x, ulong y, ulong borrow)
        {
            ulong diff = default;
            ulong borrowOut = default;

            diff = x - y - borrow; 
            // See Sub32 for the bit logic.
            borrowOut = ((~x & y) | (~(x ^ y) & diff)) >> (int)(63L);
            return ;

        }

        // --- Full-width multiply ---

        // Mul returns the full-width product of x and y: (hi, lo) = x * y
        // with the product bits' upper half returned in hi and the lower
        // half returned in lo.
        //
        // This function's execution time does not depend on the inputs.
        public static (ulong, ulong) Mul(ulong x, ulong y)
        {
            ulong hi = default;
            ulong lo = default;

            if (UintSize == 32L)
            {
                var (h, l) = Mul32(uint32(x), uint32(y));
                return (uint(h), uint(l));
            }

            (h, l) = Mul64(uint64(x), uint64(y));
            return (uint(h), uint(l));

        }

        // Mul32 returns the 64-bit product of x and y: (hi, lo) = x * y
        // with the product bits' upper half returned in hi and the lower
        // half returned in lo.
        //
        // This function's execution time does not depend on the inputs.
        public static (uint, uint) Mul32(uint x, uint y)
        {
            uint hi = default;
            uint lo = default;

            var tmp = uint64(x) * uint64(y);
            hi = uint32(tmp >> (int)(32L));
            lo = uint32(tmp);
            return ;

        }

        // Mul64 returns the 128-bit product of x and y: (hi, lo) = x * y
        // with the product bits' upper half returned in hi and the lower
        // half returned in lo.
        //
        // This function's execution time does not depend on the inputs.
        public static (ulong, ulong) Mul64(ulong x, ulong y)
        {
            ulong hi = default;
            ulong lo = default;

            const long mask32 = (long)1L << (int)(32L) - 1L;

            var x0 = x & mask32;
            var x1 = x >> (int)(32L);
            var y0 = y & mask32;
            var y1 = y >> (int)(32L);
            var w0 = x0 * y0;
            var t = x1 * y0 + w0 >> (int)(32L);
            var w1 = t & mask32;
            var w2 = t >> (int)(32L);
            w1 += x0 * y1;
            hi = x1 * y1 + w2 + w1 >> (int)(32L);
            lo = x * y;
            return ;
        }

        // --- Full-width divide ---

        // Div returns the quotient and remainder of (hi, lo) divided by y:
        // quo = (hi, lo)/y, rem = (hi, lo)%y with the dividend bits' upper
        // half in parameter hi and the lower half in parameter lo.
        // Div panics for y == 0 (division by zero) or y <= hi (quotient overflow).
        public static (ulong, ulong) Div(ulong hi, ulong lo, ulong y)
        {
            ulong quo = default;
            ulong rem = default;

            if (UintSize == 32L)
            {
                var (q, r) = Div32(uint32(hi), uint32(lo), uint32(y));
                return (uint(q), uint(r));
            }

            (q, r) = Div64(uint64(hi), uint64(lo), uint64(y));
            return (uint(q), uint(r));

        }

        // Div32 returns the quotient and remainder of (hi, lo) divided by y:
        // quo = (hi, lo)/y, rem = (hi, lo)%y with the dividend bits' upper
        // half in parameter hi and the lower half in parameter lo.
        // Div32 panics for y == 0 (division by zero) or y <= hi (quotient overflow).
        public static (uint, uint) Div32(uint hi, uint lo, uint y) => func((_, panic, __) =>
        {
            uint quo = default;
            uint rem = default;

            if (y != 0L && y <= hi)
            {
                panic(overflowError);
            }

            var z = uint64(hi) << (int)(32L) | uint64(lo);
            quo = uint32(z / uint64(y));
            rem = uint32(z % uint64(y));
            return ;

        });

        // Div64 returns the quotient and remainder of (hi, lo) divided by y:
        // quo = (hi, lo)/y, rem = (hi, lo)%y with the dividend bits' upper
        // half in parameter hi and the lower half in parameter lo.
        // Div64 panics for y == 0 (division by zero) or y <= hi (quotient overflow).
        public static (ulong, ulong) Div64(ulong hi, ulong lo, ulong y) => func((_, panic, __) =>
        {
            ulong quo = default;
            ulong rem = default;

            const long two32 = (long)1L << (int)(32L);
            const var mask32 = (var)two32 - 1L;
            if (y == 0L)
            {
                panic(divideError);
            }

            if (y <= hi)
            {
                panic(overflowError);
            }

            var s = uint(LeadingZeros64(y));
            y <<= s;

            var yn1 = y >> (int)(32L);
            var yn0 = y & mask32;
            var un32 = hi << (int)(s) | lo >> (int)((64L - s));
            var un10 = lo << (int)(s);
            var un1 = un10 >> (int)(32L);
            var un0 = un10 & mask32;
            var q1 = un32 / yn1;
            var rhat = un32 - q1 * yn1;

            while (q1 >= two32 || q1 * yn0 > two32 * rhat + un1)
            {
                q1--;
                rhat += yn1;
                if (rhat >= two32)
                {
                    break;
                }

            }


            var un21 = un32 * two32 + un1 - q1 * y;
            var q0 = un21 / yn1;
            rhat = un21 - q0 * yn1;

            while (q0 >= two32 || q0 * yn0 > two32 * rhat + un0)
            {
                q0--;
                rhat += yn1;
                if (rhat >= two32)
                {
                    break;
                }

            }


            return (q1 * two32 + q0, (un21 * two32 + un0 - q0 * y) >> (int)(s));

        });

        // Rem returns the remainder of (hi, lo) divided by y. Rem panics for
        // y == 0 (division by zero) but, unlike Div, it doesn't panic on a
        // quotient overflow.
        public static ulong Rem(ulong hi, ulong lo, ulong y)
        {
            if (UintSize == 32L)
            {
                return uint(Rem32(uint32(hi), uint32(lo), uint32(y)));
            }

            return uint(Rem64(uint64(hi), uint64(lo), uint64(y)));

        }

        // Rem32 returns the remainder of (hi, lo) divided by y. Rem32 panics
        // for y == 0 (division by zero) but, unlike Div32, it doesn't panic
        // on a quotient overflow.
        public static uint Rem32(uint hi, uint lo, uint y)
        {
            return uint32((uint64(hi) << (int)(32L) | uint64(lo)) % uint64(y));
        }

        // Rem64 returns the remainder of (hi, lo) divided by y. Rem64 panics
        // for y == 0 (division by zero) but, unlike Div64, it doesn't panic
        // on a quotient overflow.
        public static ulong Rem64(ulong hi, ulong lo, ulong y)
        { 
            // We scale down hi so that hi < y, then use Div64 to compute the
            // rem with the guarantee that it won't panic on quotient overflow.
            // Given that
            //   hi ≡ hi%y    (mod y)
            // we have
            //   hi<<64 + lo ≡ (hi%y)<<64 + lo    (mod y)
            var (_, rem) = Div64(hi % y, lo, y);
            return rem;

        }
    }
}}
