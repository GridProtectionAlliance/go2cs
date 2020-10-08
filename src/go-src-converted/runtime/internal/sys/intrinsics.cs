// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !386

// TODO finish intrinsifying 386, deadcode the assembly, remove build tags, merge w/ intrinsics_common
// TODO replace all uses of CtzXX with TrailingZerosXX; they are the same.

// package sys -- go2cs converted at 2020 October 08 03:19:07 UTC
// import "runtime/internal/sys" ==> using sys = go.runtime.@internal.sys_package
// Original source: C:\Go\src\runtime\internal\sys\intrinsics.go

using static go.builtin;

namespace go {
namespace runtime {
namespace @internal
{
    public static partial class sys_package
    {
        // Using techniques from http://supertech.csail.mit.edu/papers/debruijn.pdf
        private static readonly ulong deBruijn64ctz = (ulong)0x0218a392cd3d5dbfUL;



        private static array<byte> deBruijnIdx64ctz = new array<byte>(new byte[] { 0, 1, 2, 7, 3, 13, 8, 19, 4, 25, 14, 28, 9, 34, 20, 40, 5, 17, 26, 38, 15, 46, 29, 48, 10, 31, 35, 54, 21, 50, 41, 57, 63, 6, 12, 18, 24, 27, 33, 39, 16, 37, 45, 47, 30, 53, 49, 56, 62, 11, 23, 32, 36, 44, 52, 55, 61, 22, 43, 51, 60, 42, 59, 58 });

        private static readonly ulong deBruijn32ctz = (ulong)0x04653adfUL;



        private static array<byte> deBruijnIdx32ctz = new array<byte>(new byte[] { 0, 1, 2, 6, 3, 11, 7, 16, 4, 14, 12, 21, 8, 23, 17, 26, 31, 5, 10, 15, 13, 20, 22, 25, 30, 9, 19, 24, 29, 18, 28, 27 });

        // Ctz64 counts trailing (low-order) zeroes,
        // and if all are zero, then 64.
        public static long Ctz64(ulong x)
        {
            x &= -x; // isolate low-order bit
            var y = x * deBruijn64ctz >> (int)(58L); // extract part of deBruijn sequence
            var i = int(deBruijnIdx64ctz[y]); // convert to bit index
            var z = int((x - 1L) >> (int)(57L) & 64L); // adjustment if zero
            return i + z;

        }

        // Ctz32 counts trailing (low-order) zeroes,
        // and if all are zero, then 32.
        public static long Ctz32(uint x)
        {
            x &= -x; // isolate low-order bit
            var y = x * deBruijn32ctz >> (int)(27L); // extract part of deBruijn sequence
            var i = int(deBruijnIdx32ctz[y]); // convert to bit index
            var z = int((x - 1L) >> (int)(26L) & 32L); // adjustment if zero
            return i + z;

        }

        // Ctz8 returns the number of trailing zero bits in x; the result is 8 for x == 0.
        public static long Ctz8(byte x)
        {
            return int(ntz8tab[x]);
        }

        // Bswap64 returns its input with byte order reversed
        // 0x0102030405060708 -> 0x0807060504030201
        public static ulong Bswap64(ulong x)
        {
            var c8 = uint64(0x00ff00ff00ff00ffUL);
            var a = x >> (int)(8L) & c8;
            var b = (x & c8) << (int)(8L);
            x = a | b;
            var c16 = uint64(0x0000ffff0000ffffUL);
            a = x >> (int)(16L) & c16;
            b = (x & c16) << (int)(16L);
            x = a | b;
            var c32 = uint64(0x00000000ffffffffUL);
            a = x >> (int)(32L) & c32;
            b = (x & c32) << (int)(32L);
            x = a | b;
            return x;
        }

        // Bswap32 returns its input with byte order reversed
        // 0x01020304 -> 0x04030201
        public static uint Bswap32(uint x)
        {
            var c8 = uint32(0x00ff00ffUL);
            var a = x >> (int)(8L) & c8;
            var b = (x & c8) << (int)(8L);
            x = a | b;
            var c16 = uint32(0x0000ffffUL);
            a = x >> (int)(16L) & c16;
            b = (x & c16) << (int)(16L);
            x = a | b;
            return x;
        }
    }
}}}
