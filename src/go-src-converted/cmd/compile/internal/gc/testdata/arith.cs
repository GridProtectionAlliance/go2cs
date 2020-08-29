// run

// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Tests arithmetic expressions

// package main -- go2cs converted at 2020 August 29 09:30:19 UTC
// Original source: C:\Go\src\cmd\compile\internal\gc\testdata\arith.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static readonly ulong y = 0x0fffFFFFUL;

        //go:noinline
        private static ulong lshNop1(ulong x)
        { 
            // two outer shifts should be removed
            return (((x << (int)(5L)) >> (int)(2L)) << (int)(2L));
        }

        //go:noinline
        private static ulong lshNop2(ulong x)
        {
            return (((x << (int)(5L)) >> (int)(2L)) << (int)(3L));
        }

        //go:noinline
        private static ulong lshNop3(ulong x)
        {
            return (((x << (int)(5L)) >> (int)(2L)) << (int)(6L));
        }

        //go:noinline
        private static ulong lshNotNop(ulong x)
        { 
            // outer shift can't be removed
            return (((x << (int)(5L)) >> (int)(2L)) << (int)(1L));
        }

        //go:noinline
        private static ulong rshNop1(ulong x)
        {
            return (((x >> (int)(5L)) << (int)(2L)) >> (int)(2L));
        }

        //go:noinline
        private static ulong rshNop2(ulong x)
        {
            return (((x >> (int)(5L)) << (int)(2L)) >> (int)(3L));
        }

        //go:noinline
        private static ulong rshNop3(ulong x)
        {
            return (((x >> (int)(5L)) << (int)(2L)) >> (int)(6L));
        }

        //go:noinline
        private static ulong rshNotNop(ulong x)
        {
            return (((x >> (int)(5L)) << (int)(2L)) >> (int)(1L));
        }

        private static void testShiftRemoval()
        {
            var allSet = ~uint64(0L);
            {
                var want__prev1 = want;
                var got__prev1 = got;

                var want = uint64(0x7ffffffffffffffUL);
                var got = rshNop1(allSet);

                if (want != got)
                {
                    println("testShiftRemoval rshNop1 failed, wanted", want, "got", got);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = uint64(0x3ffffffffffffffUL);
                got = rshNop2(allSet);

                if (want != got)
                {
                    println("testShiftRemoval rshNop2 failed, wanted", want, "got", got);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = uint64(0x7fffffffffffffUL);
                got = rshNop3(allSet);

                if (want != got)
                {
                    println("testShiftRemoval rshNop3 failed, wanted", want, "got", got);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = uint64(0xffffffffffffffeUL);
                got = rshNotNop(allSet);

                if (want != got)
                {
                    println("testShiftRemoval rshNotNop failed, wanted", want, "got", got);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = uint64(0xffffffffffffffe0UL);
                got = lshNop1(allSet);

                if (want != got)
                {
                    println("testShiftRemoval lshNop1 failed, wanted", want, "got", got);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = uint64(0xffffffffffffffc0UL);
                got = lshNop2(allSet);

                if (want != got)
                {
                    println("testShiftRemoval lshNop2 failed, wanted", want, "got", got);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = uint64(0xfffffffffffffe00UL);
                got = lshNop3(allSet);

                if (want != got)
                {
                    println("testShiftRemoval lshNop3 failed, wanted", want, "got", got);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = uint64(0x7ffffffffffffff0UL);
                got = lshNotNop(allSet);

                if (want != got)
                {
                    println("testShiftRemoval lshNotNop failed, wanted", want, "got", got);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
        }

        //go:noinline
        private static ulong parseLE64(slice<byte> b)
        { 
            // skip the first two bytes, and parse the remaining 8 as a uint64
            return uint64(b[2L]) | uint64(b[3L]) << (int)(8L) | uint64(b[4L]) << (int)(16L) | uint64(b[5L]) << (int)(24L) | uint64(b[6L]) << (int)(32L) | uint64(b[7L]) << (int)(40L) | uint64(b[8L]) << (int)(48L) | uint64(b[9L]) << (int)(56L);
        }

        //go:noinline
        private static uint parseLE32(slice<byte> b)
        {
            return uint32(b[2L]) | uint32(b[3L]) << (int)(8L) | uint32(b[4L]) << (int)(16L) | uint32(b[5L]) << (int)(24L);
        }

        //go:noinline
        private static ushort parseLE16(slice<byte> b)
        {
            return uint16(b[2L]) | uint16(b[3L]) << (int)(8L);
        }

        // testLoadCombine tests for issue #14694 where load combining didn't respect the pointer offset.
        private static void testLoadCombine()
        {
            byte testData = new slice<byte>(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 });
            {
                var want__prev1 = want;
                var got__prev1 = got;

                var want = uint64(0x0908070605040302UL);
                var got = parseLE64(testData);

                if (want != got)
                {
                    println("testLoadCombine failed, wanted", want, "got", got);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = uint32(0x05040302UL);
                got = parseLE32(testData);

                if (want != got)
                {
                    println("testLoadCombine failed, wanted", want, "got", got);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = uint16(0x0302UL);
                got = parseLE16(testData);

                if (want != got)
                {
                    println("testLoadCombine failed, wanted", want, "got", got);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
        }

        private static array<byte> loadSymData = new array<byte>(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 });

        private static void testLoadSymCombine()
        {
            var w2 = uint16(0x0201UL);
            var g2 = uint16(loadSymData[0L]) | uint16(loadSymData[1L]) << (int)(8L);
            if (g2 != w2)
            {
                println("testLoadSymCombine failed, wanted", w2, "got", g2);
                failed = true;
            }
            var w4 = uint32(0x04030201UL);
            var g4 = uint32(loadSymData[0L]) | uint32(loadSymData[1L]) << (int)(8L) | uint32(loadSymData[2L]) << (int)(16L) | uint32(loadSymData[3L]) << (int)(24L);
            if (g4 != w4)
            {
                println("testLoadSymCombine failed, wanted", w4, "got", g4);
                failed = true;
            }
            var w8 = uint64(0x0807060504030201UL);
            var g8 = uint64(loadSymData[0L]) | uint64(loadSymData[1L]) << (int)(8L) | uint64(loadSymData[2L]) << (int)(16L) | uint64(loadSymData[3L]) << (int)(24L) | uint64(loadSymData[4L]) << (int)(32L) | uint64(loadSymData[5L]) << (int)(40L) | uint64(loadSymData[6L]) << (int)(48L) | uint64(loadSymData[7L]) << (int)(56L);
            if (g8 != w8)
            {
                println("testLoadSymCombine failed, wanted", w8, "got", g8);
                failed = true;
            }
        }

        //go:noinline
        private static uint invalidAdd_ssa(uint x)
        {
            return x + y + y + y + y + y + y + y + y + y + y + y + y + y + y + y + y + y;
        }

        //go:noinline
        private static uint invalidSub_ssa(uint x)
        {
            return x - y - y - y - y - y - y - y - y - y - y - y - y - y - y - y - y - y;
        }

        //go:noinline
        private static uint invalidMul_ssa(uint x)
        {
            return x * y * y * y * y * y * y * y * y * y * y * y * y * y * y * y * y * y;
        }

        // testLargeConst tests a situation where larger than 32 bit consts were passed to ADDL
        // causing an invalid instruction error.
        private static void testLargeConst()
        {
            {
                var want__prev1 = want;
                var got__prev1 = got;

                var want = uint32(268435440L);
                var got = invalidAdd_ssa(1L);

                if (want != got)
                {
                    println("testLargeConst add failed, wanted", want, "got", got);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = uint32(4026531858L);
                got = invalidSub_ssa(1L);

                if (want != got)
                {
                    println("testLargeConst sub failed, wanted", want, "got", got);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = uint32(268435455L);
                got = invalidMul_ssa(1L);

                if (want != got)
                {
                    println("testLargeConst mul failed, wanted", want, "got", got);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
        }

        // testArithRshConst ensures that "const >> const" right shifts correctly perform
        // sign extension on the lhs constant
        private static void testArithRshConst()
        {
            var wantu = uint64(0x4000000000000000UL);
            {
                var got__prev1 = got;

                var got = arithRshuConst_ssa();

                if (got != wantu)
                {
                    println("arithRshuConst failed, wanted", wantu, "got", got);
                    failed = true;
                }

                got = got__prev1;

            }

            var wants = int64(-0x4000000000000000UL);
            {
                var got__prev1 = got;

                got = arithRshConst_ssa();

                if (got != wants)
                {
                    println("arithRshuConst failed, wanted", wants, "got", got);
                    failed = true;
                }

                got = got__prev1;

            }
        }

        //go:noinline
        private static ulong arithRshuConst_ssa()
        {
            var y = uint64(0x8000000000000001UL);
            var z = uint64(1L);
            return uint64(y >> (int)(z));
        }

        //go:noinline
        private static long arithRshConst_ssa()
        {
            var y = int64(-0x8000000000000000UL);
            var z = uint64(1L);
            return int64(y >> (int)(z));
        }

        //go:noinline
        private static long arithConstShift_ssa(long x)
        {
            return x >> (int)(100L);
        }

        // testArithConstShift tests that right shift by large constants preserve
        // the sign of the input.
        private static void testArithConstShift()
        {
            var want = int64(-1L);
            {
                var got__prev1 = got;

                var got = arithConstShift_ssa(-1L);

                if (want != got)
                {
                    println("arithConstShift_ssa(-1) failed, wanted", want, "got", got);
                    failed = true;
                }

                got = got__prev1;

            }
            want = 0L;
            {
                var got__prev1 = got;

                got = arithConstShift_ssa(1L);

                if (want != got)
                {
                    println("arithConstShift_ssa(1) failed, wanted", want, "got", got);
                    failed = true;
                }

                got = got__prev1;

            }
        }

        // overflowConstShift_ssa verifes that constant folding for shift
        // doesn't wrap (i.e. x << MAX_INT << 1 doesn't get folded to x << 0).
        //go:noinline
        private static long overflowConstShift64_ssa(long x)
        {
            return x << (int)(uint64(0xffffffffffffffffUL)) << (int)(uint64(1L));
        }

        //go:noinline
        private static int overflowConstShift32_ssa(long x)
        {
            return int32(x) << (int)(uint32(0xffffffffUL)) << (int)(uint32(1L));
        }

        //go:noinline
        private static short overflowConstShift16_ssa(long x)
        {
            return int16(x) << (int)(uint16(0xffffUL)) << (int)(uint16(1L));
        }

        //go:noinline
        private static sbyte overflowConstShift8_ssa(long x)
        {
            return int8(x) << (int)(uint8(0xffUL)) << (int)(uint8(1L));
        }

        private static void testOverflowConstShift()
        {
            var want = int64(0L);
            for (var x = int64(-127L); x < int64(127L); x++)
            {
                var got = overflowConstShift64_ssa(x);
                if (want != got)
                {
                    fmt.Printf("overflowShift64 failed, wanted %d got %d\n", want, got);
                }
                got = int64(overflowConstShift32_ssa(x));
                if (want != got)
                {
                    fmt.Printf("overflowShift32 failed, wanted %d got %d\n", want, got);
                }
                got = int64(overflowConstShift16_ssa(x));
                if (want != got)
                {
                    fmt.Printf("overflowShift16 failed, wanted %d got %d\n", want, got);
                }
                got = int64(overflowConstShift8_ssa(x));
                if (want != got)
                {
                    fmt.Printf("overflowShift8 failed, wanted %d got %d\n", want, got);
                }
            }

        }

        // test64BitConstMult tests that rewrite rules don't fold 64 bit constants
        // into multiply instructions.
        private static void test64BitConstMult()
        {
            var want = int64(103079215109L);
            {
                var got = test64BitConstMult_ssa(1L, 2L);

                if (want != got)
                {
                    println("test64BitConstMult failed, wanted", want, "got", got);
                    failed = true;
                }

            }
        }

        //go:noinline
        private static long test64BitConstMult_ssa(long a, long b)
        {
            return 34359738369L * a + b * 34359738370L;
        }

        // test64BitConstAdd tests that rewrite rules don't fold 64 bit constants
        // into add instructions.
        private static void test64BitConstAdd()
        {
            var want = int64(3567671782835376650L);
            {
                var got = test64BitConstAdd_ssa(1L, 2L);

                if (want != got)
                {
                    println("test64BitConstAdd failed, wanted", want, "got", got);
                    failed = true;
                }

            }
        }

        //go:noinline
        private static long test64BitConstAdd_ssa(long a, long b)
        {
            return a + 575815584948629622L + b + 2991856197886747025L;
        }

        // testRegallocCVSpill tests that regalloc spills a value whose last use is the
        // current value.
        private static void testRegallocCVSpill()
        {
            var want = int8(-9L);
            {
                var got = testRegallocCVSpill_ssa(1L, 2L, 3L, 4L);

                if (want != got)
                {
                    println("testRegallocCVSpill failed, wanted", want, "got", got);
                    failed = true;
                }

            }
        }

        //go:noinline
        private static sbyte testRegallocCVSpill_ssa(sbyte a, sbyte b, sbyte c, sbyte d)
        {
            return a + -32L + b + 63L * c * -87L * d;
        }

        private static void testBitwiseLogic()
        {
            var a = uint32(57623283L);
            var b = uint32(1314713839L);
            {
                var want__prev1 = want;
                var got__prev1 = got;

                var want = uint32(38551779L);
                var got = testBitwiseAnd_ssa(a, b);

                if (want != got)
                {
                    println("testBitwiseAnd failed, wanted", want, "got", got);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = uint32(1333785343L);
                got = testBitwiseOr_ssa(a, b);

                if (want != got)
                {
                    println("testBitwiseOr failed, wanted", want, "got", got);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = uint32(1295233564L);
                got = testBitwiseXor_ssa(a, b);

                if (want != got)
                {
                    println("testBitwiseXor failed, wanted", want, "got", got);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = int32(832L);
                got = testBitwiseLsh_ssa(13L, 4L, 2L);

                if (want != got)
                {
                    println("testBitwiseLsh failed, wanted", want, "got", got);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = int32(0L);
                got = testBitwiseLsh_ssa(13L, 25L, 15L);

                if (want != got)
                {
                    println("testBitwiseLsh failed, wanted", want, "got", got);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = int32(0L);
                got = testBitwiseLsh_ssa(-13L, 25L, 15L);

                if (want != got)
                {
                    println("testBitwiseLsh failed, wanted", want, "got", got);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = int32(-13L);
                got = testBitwiseRsh_ssa(-832L, 4L, 2L);

                if (want != got)
                {
                    println("testBitwiseRsh failed, wanted", want, "got", got);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = int32(0L);
                got = testBitwiseRsh_ssa(13L, 25L, 15L);

                if (want != got)
                {
                    println("testBitwiseRsh failed, wanted", want, "got", got);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = int32(-1L);
                got = testBitwiseRsh_ssa(-13L, 25L, 15L);

                if (want != got)
                {
                    println("testBitwiseRsh failed, wanted", want, "got", got);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = uint32(0x3ffffffUL);
                got = testBitwiseRshU_ssa(0xffffffffUL, 4L, 2L);

                if (want != got)
                {
                    println("testBitwiseRshU failed, wanted", want, "got", got);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = uint32(0L);
                got = testBitwiseRshU_ssa(13L, 25L, 15L);

                if (want != got)
                {
                    println("testBitwiseRshU failed, wanted", want, "got", got);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = uint32(0L);
                got = testBitwiseRshU_ssa(0x8aaaaaaaUL, 25L, 15L);

                if (want != got)
                {
                    println("testBitwiseRshU failed, wanted", want, "got", got);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
        }

        //go:noinline
        private static uint testBitwiseAnd_ssa(uint a, uint b)
        {
            return a & b;
        }

        //go:noinline
        private static uint testBitwiseOr_ssa(uint a, uint b)
        {
            return a | b;
        }

        //go:noinline
        private static uint testBitwiseXor_ssa(uint a, uint b)
        {
            return a ^ b;
        }

        //go:noinline
        private static int testBitwiseLsh_ssa(int a, uint b, uint c)
        {
            return a << (int)(b) << (int)(c);
        }

        //go:noinline
        private static int testBitwiseRsh_ssa(int a, uint b, uint c)
        {
            return a >> (int)(b) >> (int)(c);
        }

        //go:noinline
        private static uint testBitwiseRshU_ssa(uint a, uint b, uint c)
        {
            return a >> (int)(b) >> (int)(c);
        }

        //go:noinline
        private static long testShiftCX_ssa()
        {
            var v1 = uint8(3L);
            var v4 = (v1 * v1) ^ v1 | v1 - v1 - v1 & v1 ^ uint8(3L + 2L) + v1 * 1L >> (int)(0L) - v1 | 1L | v1 << (int)((2L * 3L | 0L - 0L * 0L ^ 1L));
            var v5 = v4 >> (int)((3L - 0L - uint(3L))) | v1 | v1 + v1 ^ v4 << (int)((0L + 1L | 3L & 1L)) << (int)((uint64(1L) << (int)(0L) * 2L * 0L << (int)(0L))) ^ v1;
            var v6 = v5 ^ (v1 + v1) * v1 | v1 | v1 * v1 >> (int)((v1 & v1)) >> (int)((uint(1L) << (int)(0L) * uint(3L) >> (int)(1L))) * v1 << (int)(2L) * v1 << (int)(v1) - v1 >> (int)(2L) | (v4 - v1) ^ v1 + v1 ^ v1 >> (int)(1L) | v1 + v1 - v1 ^ v1;
            var v7 = v6 & v5 << (int)(0L);
            v1++;
            long v11 = 2L & 1L ^ 0L + 3L | int(0L ^ 0L) << (int)(1L) >> (int)((1L * 0L * 3L)) ^ 0L * 0L ^ 3L & 0L * 3L & 3L ^ 3L * 3L ^ 1L ^ int(2L) << (int)((2L * 3L)) + 2L | 2L | 2L ^ 2L + 1L | 3L | 0L ^ int(1L) >> (int)(1L) ^ 2L; // int
            v7--;
            return int(uint64(2L * 1L) << (int)((3L - 2L)) << (int)(uint(3L >> (int)(v7))) - 2L) & v11 | v11 - int(2L) << (int)(0L) >> (int)((2L - 1L)) * (v11 * 0L & v11 << (int)(1L) << (int)((uint8(2L) + v4)));
        }

        private static void testShiftCX()
        {
            long want = 141L;
            {
                var got = testShiftCX_ssa();

                if (want != got)
                {
                    println("testShiftCX failed, wanted", want, "got", got);
                    failed = true;
                }

            }
        }

        // testSubqToNegq ensures that the SUBQ -> NEGQ translation works correctly.
        private static void testSubqToNegq()
        {
            var want = int64(-318294940372190156L);
            {
                var got = testSubqToNegq_ssa(1L, 2L, 3L, 4L, 5L, 6L, 7L, 8L, 9L, 1L, 2L);

                if (want != got)
                {
                    println("testSubqToNegq failed, wanted", want, "got", got);
                    failed = true;
                }

            }
        }

        //go:noinline
        private static long testSubqToNegq_ssa(long a, long b, long c, long d, long e, long f, long g, long h, long i, long j, long k)
        {
            return a + 8207351403619448057L - b - 1779494519303207690L + c * 8810076340510052032L * d - 4465874067674546219L - e * 4361839741470334295L - f + 8688847565426072650L * g * 8065564729145417479L;
        }

        private static void testOcom()
        {
            var want1 = int32(0x55555555UL);
            var want2 = int32(-0x55555556UL);
            {
                var (got1, got2) = testOcom_ssa(0x55555555UL, 0x55555555UL);

                if (want1 != got1 || want2 != got2)
                {
                    println("testSubqToNegq failed, wanted", want1, "and", want2, "got", got1, "and", got2);
                    failed = true;
                }

            }
        }

        //go:noinline
        private static (int, int) testOcom_ssa(int a, int b)
        {
            return (~~~~a, ~~~~~b);
        }

        private static (byte, ushort, uint, ulong) lrot1_ssa(byte w, ushort x, uint y, ulong z)
        {
            a = (w << (int)(5L)) | (w >> (int)(3L));
            b = (x << (int)(13L)) | (x >> (int)(3L));
            c = (y << (int)(29L)) | (y >> (int)(3L));
            d = (z << (int)(61L)) | (z >> (int)(3L));
            return;
        }

        //go:noinline
        private static uint lrot2_ssa(uint w, uint n)
        { 
            // Want to be sure that a "rotate by 32" which
            // is really 0 | (w >> 0) == w
            // is correctly compiled.
            return (w << (int)(n)) | (w >> (int)((32L - n)));
        }

        //go:noinline
        private static uint lrot3_ssa(uint w)
        { 
            // Want to be sure that a "rotate by 32" which
            // is really 0 | (w >> 0) == w
            // is correctly compiled.
            return (w << (int)(32L)) | (w >> (int)((32L - 32L)));
        }

        private static void testLrot()
        {
            var wantA = uint8(0xe1UL);
            var wantB = uint16(0xe001UL);
            var wantC = uint32(0xe0000001UL);
            var wantD = uint64(0xe000000000000001UL);
            var (a, b, c, d) = lrot1_ssa(0xfUL, 0xfUL, 0xfUL, 0xfUL);
            if (a != wantA || b != wantB || c != wantC || d != wantD)
            {
                println("lrot1_ssa(0xf, 0xf, 0xf, 0xf)=", wantA, wantB, wantC, wantD, ", got", a, b, c, d);
                failed = true;
            }
            var x = lrot2_ssa(0xb0000001UL, 32L);
            var wantX = uint32(0xb0000001UL);
            if (x != wantX)
            {
                println("lrot2_ssa(0xb0000001, 32)=", wantX, ", got", x);
                failed = true;
            }
            x = lrot3_ssa(0xb0000001UL);
            if (x != wantX)
            {
                println("lrot3_ssa(0xb0000001)=", wantX, ", got", x);
                failed = true;
            }
        }

        //go:noinline
        private static ulong sub1_ssa()
        {
            var v1 = uint64(3L); // uint64
            return v1 * v1 - (v1 & v1) & v1;
        }

        //go:noinline
        private static byte sub2_ssa()
        {
            var v1 = uint8(0L);
            var v3 = v1 + v1 + v1 ^ v1 | 3L + v1 ^ v1 | v1 ^ v1;
            v1--; // dev.ssa doesn't see this one
            return v1 ^ v1 * v1 - v3;
        }

        private static void testSubConst()
        {
            var x1 = sub1_ssa();
            var want1 = uint64(6L);
            if (x1 != want1)
            {
                println("sub1_ssa()=", want1, ", got", x1);
                failed = true;
            }
            var x2 = sub2_ssa();
            var want2 = uint8(251L);
            if (x2 != want2)
            {
                println("sub2_ssa()=", want2, ", got", x2);
                failed = true;
            }
        }

        //go:noinline
        private static long orPhi_ssa(bool a, long x)
        {
            long v = 0L;
            if (a)
            {
                v = -1L;
            }
            else
            {
                v = -1L;
            }
            return x | v;
        }

        private static void testOrPhi()
        {
            {
                long want__prev1 = want;
                var got__prev1 = got;

                long want = -1L;
                var got = orPhi_ssa(true, 4L);

                if (got != want)
                {
                    println("orPhi_ssa(true, 4)=", got, " want ", want);
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                long want__prev1 = want;
                var got__prev1 = got;

                want = -1L;
                got = orPhi_ssa(false, 0L);

                if (got != want)
                {
                    println("orPhi_ssa(false, 0)=", got, " want ", want);
                }

                want = want__prev1;
                got = got__prev1;

            }
        }

        //go:noinline
        private static uint addshiftLL_ssa(uint a, uint b)
        {
            return a + b << (int)(3L);
        }

        //go:noinline
        private static uint subshiftLL_ssa(uint a, uint b)
        {
            return a - b << (int)(3L);
        }

        //go:noinline
        private static uint rsbshiftLL_ssa(uint a, uint b)
        {
            return a << (int)(3L) - b;
        }

        //go:noinline
        private static uint andshiftLL_ssa(uint a, uint b)
        {
            return a & (b << (int)(3L));
        }

        //go:noinline
        private static uint orshiftLL_ssa(uint a, uint b)
        {
            return a | b << (int)(3L);
        }

        //go:noinline
        private static uint xorshiftLL_ssa(uint a, uint b)
        {
            return a ^ b << (int)(3L);
        }

        //go:noinline
        private static uint bicshiftLL_ssa(uint a, uint b)
        {
            return a & ~(b << (int)(3L));
        }

        //go:noinline
        private static uint notshiftLL_ssa(uint a)
        {
            return ~(a << (int)(3L));
        }

        //go:noinline
        private static uint addshiftRL_ssa(uint a, uint b)
        {
            return a + b >> (int)(3L);
        }

        //go:noinline
        private static uint subshiftRL_ssa(uint a, uint b)
        {
            return a - b >> (int)(3L);
        }

        //go:noinline
        private static uint rsbshiftRL_ssa(uint a, uint b)
        {
            return a >> (int)(3L) - b;
        }

        //go:noinline
        private static uint andshiftRL_ssa(uint a, uint b)
        {
            return a & (b >> (int)(3L));
        }

        //go:noinline
        private static uint orshiftRL_ssa(uint a, uint b)
        {
            return a | b >> (int)(3L);
        }

        //go:noinline
        private static uint xorshiftRL_ssa(uint a, uint b)
        {
            return a ^ b >> (int)(3L);
        }

        //go:noinline
        private static uint bicshiftRL_ssa(uint a, uint b)
        {
            return a & ~(b >> (int)(3L));
        }

        //go:noinline
        private static uint notshiftRL_ssa(uint a)
        {
            return ~(a >> (int)(3L));
        }

        //go:noinline
        private static int addshiftRA_ssa(int a, int b)
        {
            return a + b >> (int)(3L);
        }

        //go:noinline
        private static int subshiftRA_ssa(int a, int b)
        {
            return a - b >> (int)(3L);
        }

        //go:noinline
        private static int rsbshiftRA_ssa(int a, int b)
        {
            return a >> (int)(3L) - b;
        }

        //go:noinline
        private static int andshiftRA_ssa(int a, int b)
        {
            return a & (b >> (int)(3L));
        }

        //go:noinline
        private static int orshiftRA_ssa(int a, int b)
        {
            return a | b >> (int)(3L);
        }

        //go:noinline
        private static int xorshiftRA_ssa(int a, int b)
        {
            return a ^ b >> (int)(3L);
        }

        //go:noinline
        private static int bicshiftRA_ssa(int a, int b)
        {
            return a & ~(b >> (int)(3L));
        }

        //go:noinline
        private static int notshiftRA_ssa(int a)
        {
            return ~(a >> (int)(3L));
        }

        //go:noinline
        private static uint addshiftLLreg_ssa(uint a, uint b, byte s)
        {
            return a + b << (int)(s);
        }

        //go:noinline
        private static uint subshiftLLreg_ssa(uint a, uint b, byte s)
        {
            return a - b << (int)(s);
        }

        //go:noinline
        private static uint rsbshiftLLreg_ssa(uint a, uint b, byte s)
        {
            return a << (int)(s) - b;
        }

        //go:noinline
        private static uint andshiftLLreg_ssa(uint a, uint b, byte s)
        {
            return a & (b << (int)(s));
        }

        //go:noinline
        private static uint orshiftLLreg_ssa(uint a, uint b, byte s)
        {
            return a | b << (int)(s);
        }

        //go:noinline
        private static uint xorshiftLLreg_ssa(uint a, uint b, byte s)
        {
            return a ^ b << (int)(s);
        }

        //go:noinline
        private static uint bicshiftLLreg_ssa(uint a, uint b, byte s)
        {
            return a & ~(b << (int)(s));
        }

        //go:noinline
        private static uint notshiftLLreg_ssa(uint a, byte s)
        {
            return ~(a << (int)(s));
        }

        //go:noinline
        private static uint addshiftRLreg_ssa(uint a, uint b, byte s)
        {
            return a + b >> (int)(s);
        }

        //go:noinline
        private static uint subshiftRLreg_ssa(uint a, uint b, byte s)
        {
            return a - b >> (int)(s);
        }

        //go:noinline
        private static uint rsbshiftRLreg_ssa(uint a, uint b, byte s)
        {
            return a >> (int)(s) - b;
        }

        //go:noinline
        private static uint andshiftRLreg_ssa(uint a, uint b, byte s)
        {
            return a & (b >> (int)(s));
        }

        //go:noinline
        private static uint orshiftRLreg_ssa(uint a, uint b, byte s)
        {
            return a | b >> (int)(s);
        }

        //go:noinline
        private static uint xorshiftRLreg_ssa(uint a, uint b, byte s)
        {
            return a ^ b >> (int)(s);
        }

        //go:noinline
        private static uint bicshiftRLreg_ssa(uint a, uint b, byte s)
        {
            return a & ~(b >> (int)(s));
        }

        //go:noinline
        private static uint notshiftRLreg_ssa(uint a, byte s)
        {
            return ~(a >> (int)(s));
        }

        //go:noinline
        private static int addshiftRAreg_ssa(int a, int b, byte s)
        {
            return a + b >> (int)(s);
        }

        //go:noinline
        private static int subshiftRAreg_ssa(int a, int b, byte s)
        {
            return a - b >> (int)(s);
        }

        //go:noinline
        private static int rsbshiftRAreg_ssa(int a, int b, byte s)
        {
            return a >> (int)(s) - b;
        }

        //go:noinline
        private static int andshiftRAreg_ssa(int a, int b, byte s)
        {
            return a & (b >> (int)(s));
        }

        //go:noinline
        private static int orshiftRAreg_ssa(int a, int b, byte s)
        {
            return a | b >> (int)(s);
        }

        //go:noinline
        private static int xorshiftRAreg_ssa(int a, int b, byte s)
        {
            return a ^ b >> (int)(s);
        }

        //go:noinline
        private static int bicshiftRAreg_ssa(int a, int b, byte s)
        {
            return a & ~(b >> (int)(s));
        }

        //go:noinline
        private static int notshiftRAreg_ssa(int a, byte s)
        {
            return ~(a >> (int)(s));
        }

        // test ARM shifted ops
        private static void testShiftedOps()
        {
            var a = uint32(10L);
            var b = uint32(42L);
            {
                var want__prev1 = want;
                var got__prev1 = got;

                var want = a + b << (int)(3L);
                var got = addshiftLL_ssa(a, b);

                if (got != want)
                {
                    println("addshiftLL_ssa(10, 42) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = a - b << (int)(3L);
                got = subshiftLL_ssa(a, b);

                if (got != want)
                {
                    println("subshiftLL_ssa(10, 42) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = a << (int)(3L) - b;
                got = rsbshiftLL_ssa(a, b);

                if (got != want)
                {
                    println("rsbshiftLL_ssa(10, 42) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = a & (b << (int)(3L));
                got = andshiftLL_ssa(a, b);

                if (got != want)
                {
                    println("andshiftLL_ssa(10, 42) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = a | b << (int)(3L);
                got = orshiftLL_ssa(a, b);

                if (got != want)
                {
                    println("orshiftLL_ssa(10, 42) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = a ^ b << (int)(3L);
                got = xorshiftLL_ssa(a, b);

                if (got != want)
                {
                    println("xorshiftLL_ssa(10, 42) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = a & ~(b << (int)(3L));
                got = bicshiftLL_ssa(a, b);

                if (got != want)
                {
                    println("bicshiftLL_ssa(10, 42) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = ~(a << (int)(3L));
                got = notshiftLL_ssa(a);

                if (got != want)
                {
                    println("notshiftLL_ssa(10) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = a + b >> (int)(3L);
                got = addshiftRL_ssa(a, b);

                if (got != want)
                {
                    println("addshiftRL_ssa(10, 42) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = a - b >> (int)(3L);
                got = subshiftRL_ssa(a, b);

                if (got != want)
                {
                    println("subshiftRL_ssa(10, 42) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = a >> (int)(3L) - b;
                got = rsbshiftRL_ssa(a, b);

                if (got != want)
                {
                    println("rsbshiftRL_ssa(10, 42) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = a & (b >> (int)(3L));
                got = andshiftRL_ssa(a, b);

                if (got != want)
                {
                    println("andshiftRL_ssa(10, 42) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = a | b >> (int)(3L);
                got = orshiftRL_ssa(a, b);

                if (got != want)
                {
                    println("orshiftRL_ssa(10, 42) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = a ^ b >> (int)(3L);
                got = xorshiftRL_ssa(a, b);

                if (got != want)
                {
                    println("xorshiftRL_ssa(10, 42) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = a & ~(b >> (int)(3L));
                got = bicshiftRL_ssa(a, b);

                if (got != want)
                {
                    println("bicshiftRL_ssa(10, 42) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = ~(a >> (int)(3L));
                got = notshiftRL_ssa(a);

                if (got != want)
                {
                    println("notshiftRL_ssa(10) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            var c = int32(10L);
            var d = int32(-42L);
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = c + d >> (int)(3L);
                got = addshiftRA_ssa(c, d);

                if (got != want)
                {
                    println("addshiftRA_ssa(10, -42) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = c - d >> (int)(3L);
                got = subshiftRA_ssa(c, d);

                if (got != want)
                {
                    println("subshiftRA_ssa(10, -42) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = c >> (int)(3L) - d;
                got = rsbshiftRA_ssa(c, d);

                if (got != want)
                {
                    println("rsbshiftRA_ssa(10, -42) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = c & (d >> (int)(3L));
                got = andshiftRA_ssa(c, d);

                if (got != want)
                {
                    println("andshiftRA_ssa(10, -42) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = c | d >> (int)(3L);
                got = orshiftRA_ssa(c, d);

                if (got != want)
                {
                    println("orshiftRA_ssa(10, -42) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = c ^ d >> (int)(3L);
                got = xorshiftRA_ssa(c, d);

                if (got != want)
                {
                    println("xorshiftRA_ssa(10, -42) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = c & ~(d >> (int)(3L));
                got = bicshiftRA_ssa(c, d);

                if (got != want)
                {
                    println("bicshiftRA_ssa(10, -42) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = ~(d >> (int)(3L));
                got = notshiftRA_ssa(d);

                if (got != want)
                {
                    println("notshiftRA_ssa(-42) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            var s = uint8(3L);
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = a + b << (int)(s);
                got = addshiftLLreg_ssa(a, b, s);

                if (got != want)
                {
                    println("addshiftLLreg_ssa(10, 42, 3) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = a - b << (int)(s);
                got = subshiftLLreg_ssa(a, b, s);

                if (got != want)
                {
                    println("subshiftLLreg_ssa(10, 42, 3) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = a << (int)(s) - b;
                got = rsbshiftLLreg_ssa(a, b, s);

                if (got != want)
                {
                    println("rsbshiftLLreg_ssa(10, 42, 3) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = a & (b << (int)(s));
                got = andshiftLLreg_ssa(a, b, s);

                if (got != want)
                {
                    println("andshiftLLreg_ssa(10, 42, 3) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = a | b << (int)(s);
                got = orshiftLLreg_ssa(a, b, s);

                if (got != want)
                {
                    println("orshiftLLreg_ssa(10, 42, 3) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = a ^ b << (int)(s);
                got = xorshiftLLreg_ssa(a, b, s);

                if (got != want)
                {
                    println("xorshiftLLreg_ssa(10, 42, 3) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = a & ~(b << (int)(s));
                got = bicshiftLLreg_ssa(a, b, s);

                if (got != want)
                {
                    println("bicshiftLLreg_ssa(10, 42, 3) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = ~(a << (int)(s));
                got = notshiftLLreg_ssa(a, s);

                if (got != want)
                {
                    println("notshiftLLreg_ssa(10) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = a + b >> (int)(s);
                got = addshiftRLreg_ssa(a, b, s);

                if (got != want)
                {
                    println("addshiftRLreg_ssa(10, 42, 3) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = a - b >> (int)(s);
                got = subshiftRLreg_ssa(a, b, s);

                if (got != want)
                {
                    println("subshiftRLreg_ssa(10, 42, 3) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = a >> (int)(s) - b;
                got = rsbshiftRLreg_ssa(a, b, s);

                if (got != want)
                {
                    println("rsbshiftRLreg_ssa(10, 42, 3) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = a & (b >> (int)(s));
                got = andshiftRLreg_ssa(a, b, s);

                if (got != want)
                {
                    println("andshiftRLreg_ssa(10, 42, 3) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = a | b >> (int)(s);
                got = orshiftRLreg_ssa(a, b, s);

                if (got != want)
                {
                    println("orshiftRLreg_ssa(10, 42, 3) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = a ^ b >> (int)(s);
                got = xorshiftRLreg_ssa(a, b, s);

                if (got != want)
                {
                    println("xorshiftRLreg_ssa(10, 42, 3) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = a & ~(b >> (int)(s));
                got = bicshiftRLreg_ssa(a, b, s);

                if (got != want)
                {
                    println("bicshiftRLreg_ssa(10, 42, 3) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = ~(a >> (int)(s));
                got = notshiftRLreg_ssa(a, s);

                if (got != want)
                {
                    println("notshiftRLreg_ssa(10) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = c + d >> (int)(s);
                got = addshiftRAreg_ssa(c, d, s);

                if (got != want)
                {
                    println("addshiftRAreg_ssa(10, -42, 3) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = c - d >> (int)(s);
                got = subshiftRAreg_ssa(c, d, s);

                if (got != want)
                {
                    println("subshiftRAreg_ssa(10, -42, 3) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = c >> (int)(s) - d;
                got = rsbshiftRAreg_ssa(c, d, s);

                if (got != want)
                {
                    println("rsbshiftRAreg_ssa(10, -42, 3) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = c & (d >> (int)(s));
                got = andshiftRAreg_ssa(c, d, s);

                if (got != want)
                {
                    println("andshiftRAreg_ssa(10, -42, 3) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = c | d >> (int)(s);
                got = orshiftRAreg_ssa(c, d, s);

                if (got != want)
                {
                    println("orshiftRAreg_ssa(10, -42, 3) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = c ^ d >> (int)(s);
                got = xorshiftRAreg_ssa(c, d, s);

                if (got != want)
                {
                    println("xorshiftRAreg_ssa(10, -42, 3) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = c & ~(d >> (int)(s));
                got = bicshiftRAreg_ssa(c, d, s);

                if (got != want)
                {
                    println("bicshiftRAreg_ssa(10, -42, 3) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = ~(d >> (int)(s));
                got = notshiftRAreg_ssa(d, s);

                if (got != want)
                {
                    println("notshiftRAreg_ssa(-42, 3) =", got, " want ", want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
        }

        private static var failed = false;

        private static void Main() => func((_, panic, __) =>
        {
            test64BitConstMult();
            test64BitConstAdd();
            testRegallocCVSpill();
            testSubqToNegq();
            testBitwiseLogic();
            testOcom();
            testLrot();
            testShiftCX();
            testSubConst();
            testOverflowConstShift();
            testArithConstShift();
            testArithRshConst();
            testLargeConst();
            testLoadCombine();
            testLoadSymCombine();
            testShiftRemoval();
            testShiftedOps();

            if (failed)
            {
                panic("failed");
            }
        });
    }
}
