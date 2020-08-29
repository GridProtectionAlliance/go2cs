// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:generate go run make_tables.go

// Package bits implements bit counting and manipulation
// functions for the predeclared unsigned integer types.
// package bits -- go2cs converted at 2020 August 29 08:23:31 UTC
// import "math/bits" ==> using bits = go.math.bits_package
// Original source: C:\Go\src\math\bits\bits.go

using static go.builtin;

namespace go {
namespace math
{
    public static partial class bits_package
    {
        private static readonly long uintSize = 32L << (int)((~uint(0L) >> (int)(32L) & 1L)); // 32 or 64

        // UintSize is the size of a uint in bits.
 // 32 or 64

        // UintSize is the size of a uint in bits.
        public static readonly var UintSize = uintSize;

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
        private static readonly ulong deBruijn32 = 0x077CB531UL;



        private static array<byte> deBruijn32tab = new array<byte>(new byte[] { 0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8, 31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9 });

        private static readonly ulong deBruijn64 = 0x03f79d71b4ca8b09UL;



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

        private static readonly ulong m0 = 0x5555555555555555UL; // 01010101 ...
 // 01010101 ...
        private static readonly ulong m1 = 0x3333333333333333UL; // 00110011 ...
 // 00110011 ...
        private static readonly ulong m2 = 0x0f0f0f0f0f0f0f0fUL; // 00001111 ...
 // 00001111 ...
        private static readonly ulong m3 = 0x00ff00ff00ff00ffUL; // etc.
 // etc.
        private static readonly ulong m4 = 0x0000ffff0000ffffUL;

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
            const long m = 1L << (int)(64L) - 1L;

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
        public static byte RotateLeft8(byte x, long k)
        {
            const long n = 8L;

            var s = uint(k) & (n - 1L);
            return x << (int)(s) | x >> (int)((n - s));
        }

        // RotateLeft16 returns the value of x rotated left by (k mod 16) bits.
        // To rotate x right by k bits, call RotateLeft16(x, -k).
        public static ushort RotateLeft16(ushort x, long k)
        {
            const long n = 16L;

            var s = uint(k) & (n - 1L);
            return x << (int)(s) | x >> (int)((n - s));
        }

        // RotateLeft32 returns the value of x rotated left by (k mod 32) bits.
        // To rotate x right by k bits, call RotateLeft32(x, -k).
        public static uint RotateLeft32(uint x, long k)
        {
            const long n = 32L;

            var s = uint(k) & (n - 1L);
            return x << (int)(s) | x >> (int)((n - s));
        }

        // RotateLeft64 returns the value of x rotated left by (k mod 64) bits.
        // To rotate x right by k bits, call RotateLeft64(x, -k).
        public static ulong RotateLeft64(ulong x, long k)
        {
            const long n = 64L;

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
            const long m = 1L << (int)(32L) - 1L;

            x = x >> (int)(1L) & (m0 & m) | x & (m0 & m) << (int)(1L);
            x = x >> (int)(2L) & (m1 & m) | x & (m1 & m) << (int)(2L);
            x = x >> (int)(4L) & (m2 & m) | x & (m2 & m) << (int)(4L);
            x = x >> (int)(8L) & (m3 & m) | x & (m3 & m) << (int)(8L);
            return x >> (int)(16L) | x << (int)(16L);
        }

        // Reverse64 returns the value of x with its bits in reversed order.
        public static ulong Reverse64(ulong x)
        {
            const long m = 1L << (int)(64L) - 1L;

            x = x >> (int)(1L) & (m0 & m) | x & (m0 & m) << (int)(1L);
            x = x >> (int)(2L) & (m1 & m) | x & (m1 & m) << (int)(2L);
            x = x >> (int)(4L) & (m2 & m) | x & (m2 & m) << (int)(4L);
            x = x >> (int)(8L) & (m3 & m) | x & (m3 & m) << (int)(8L);
            x = x >> (int)(16L) & (m4 & m) | x & (m4 & m) << (int)(16L);
            return x >> (int)(32L) | x << (int)(32L);
        }

        // --- ReverseBytes ---

        // ReverseBytes returns the value of x with its bytes in reversed order.
        public static ulong ReverseBytes(ulong x)
        {
            if (UintSize == 32L)
            {
                return uint(ReverseBytes32(uint32(x)));
            }
            return uint(ReverseBytes64(uint64(x)));
        }

        // ReverseBytes16 returns the value of x with its bytes in reversed order.
        public static ushort ReverseBytes16(ushort x)
        {
            return x >> (int)(8L) | x << (int)(8L);
        }

        // ReverseBytes32 returns the value of x with its bytes in reversed order.
        public static uint ReverseBytes32(uint x)
        {
            const long m = 1L << (int)(32L) - 1L;

            x = x >> (int)(8L) & (m3 & m) | x & (m3 & m) << (int)(8L);
            return x >> (int)(16L) | x << (int)(16L);
        }

        // ReverseBytes64 returns the value of x with its bytes in reversed order.
        public static ulong ReverseBytes64(ulong x)
        {
            const long m = 1L << (int)(64L) - 1L;

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
    }
}}
