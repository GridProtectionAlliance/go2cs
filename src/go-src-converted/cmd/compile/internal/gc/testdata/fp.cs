// run

// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Tests floating point arithmetic expressions

// package main -- go2cs converted at 2020 August 29 09:58:21 UTC
// Original source: C:\Go\src\cmd\compile\internal\gc\testdata\fp.go
using fmt = go.fmt_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        // manysub_ssa is designed to tickle bugs that depend on register
        // pressure or unfriendly operand ordering in registers (and at
        // least once it succeeded in this).
        //go:noinline
        private static (double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double) manysub_ssa(double a, double b, double c, double d)
        {
            aa = a + 11.0F - a;
            ab = a - b;
            ac = a - c;
            ad = a - d;
            ba = b - a;
            bb = b + 22.0F - b;
            bc = b - c;
            bd = b - d;
            ca = c - a;
            cb = c - b;
            cc = c + 33.0F - c;
            cd = c - d;
            da = d - a;
            db = d - b;
            dc = d - c;
            dd = d + 44.0F - d;
            return;
        }

        // fpspill_ssa attempts to trigger a bug where phis with floating point values
        // were stored in non-fp registers causing an error in doasm.
        //go:noinline
        private static double fpspill_ssa(long a)
        {
            float ret = -1.0F;
            switch (a)
            {
                case 0L: 
                    ret = 1.0F;
                    break;
                case 1L: 
                    ret = 1.1F;
                    break;
                case 2L: 
                    ret = 1.2F;
                    break;
                case 3L: 
                    ret = 1.3F;
                    break;
                case 4L: 
                    ret = 1.4F;
                    break;
                case 5L: 
                    ret = 1.5F;
                    break;
                case 6L: 
                    ret = 1.6F;
                    break;
                case 7L: 
                    ret = 1.7F;
                    break;
                case 8L: 
                    ret = 1.8F;
                    break;
                case 9L: 
                    ret = 1.9F;
                    break;
                case 10L: 
                    ret = 1.10F;
                    break;
                case 11L: 
                    ret = 1.11F;
                    break;
                case 12L: 
                    ret = 1.12F;
                    break;
                case 13L: 
                    ret = 1.13F;
                    break;
                case 14L: 
                    ret = 1.14F;
                    break;
                case 15L: 
                    ret = 1.15F;
                    break;
                case 16L: 
                    ret = 1.16F;
                    break;
            }
            return ret;
        }

        //go:noinline
        private static double add64_ssa(double a, double b)
        {
            return a + b;
        }

        //go:noinline
        private static double mul64_ssa(double a, double b)
        {
            return a * b;
        }

        //go:noinline
        private static double sub64_ssa(double a, double b)
        {
            return a - b;
        }

        //go:noinline
        private static double div64_ssa(double a, double b)
        {
            return a / b;
        }

        //go:noinline
        private static double neg64_ssa(double a, double b)
        {
            return -a + -1L * b;
        }

        //go:noinline
        private static float add32_ssa(float a, float b)
        {
            return a + b;
        }

        //go:noinline
        private static float mul32_ssa(float a, float b)
        {
            return a * b;
        }

        //go:noinline
        private static float sub32_ssa(float a, float b)
        {
            return a - b;
        }

        //go:noinline
        private static float div32_ssa(float a, float b)
        {
            return a / b;
        }

        //go:noinline
        private static float neg32_ssa(float a, float b)
        {
            return -a + -1L * b;
        }

        //go:noinline
        private static (double, double, double, double, double, double, double, double, double) conv2Float64_ssa(sbyte a, byte b, short c, ushort d, int e, uint f, long g, ulong h, float i)
        {
            aa = float64(a);
            bb = float64(b);
            cc = float64(c);
            hh = float64(h);
            dd = float64(d);
            ee = float64(e);
            ff = float64(f);
            gg = float64(g);
            ii = float64(i);
            return;
        }

        //go:noinline
        private static (float, float, float, float, float, float, float, float, float) conv2Float32_ssa(sbyte a, byte b, short c, ushort d, int e, uint f, long g, ulong h, double i)
        {
            aa = float32(a);
            bb = float32(b);
            cc = float32(c);
            dd = float32(d);
            ee = float32(e);
            ff = float32(f);
            gg = float32(g);
            hh = float32(h);
            ii = float32(i);
            return;
        }

        private static long integer2floatConversions()
        {
            long fails = 0L;
            {
                var (a, b, c, d, e, f, g, h, i) = conv2Float64_ssa(0L, 0L, 0L, 0L, 0L, 0L, 0L, 0L, 0L);
                fails += expectAll64("zero64", 0L, a, b, c, d, e, f, g, h, i);
            }
            {
                (a, b, c, d, e, f, g, h, i) = conv2Float64_ssa(1L, 1L, 1L, 1L, 1L, 1L, 1L, 1L, 1L);
                fails += expectAll64("one64", 1L, a, b, c, d, e, f, g, h, i);
            }
            {
                (a, b, c, d, e, f, g, h, i) = conv2Float32_ssa(0L, 0L, 0L, 0L, 0L, 0L, 0L, 0L, 0L);
                fails += expectAll32("zero32", 0L, a, b, c, d, e, f, g, h, i);
            }
            {
                (a, b, c, d, e, f, g, h, i) = conv2Float32_ssa(1L, 1L, 1L, 1L, 1L, 1L, 1L, 1L, 1L);
                fails += expectAll32("one32", 1L, a, b, c, d, e, f, g, h, i);
            }
            { 
                // Check maximum values
                (a, b, c, d, e, f, g, h, i) = conv2Float64_ssa(127L, 255L, 32767L, 65535L, 0x7fffffffUL, 0xffffffffUL, 0x7fffFFFFffffFFFFUL, 0xffffFFFFffffFFFFUL, 3.402823E38F);
                fails += expect64("a", a, 127L);
                fails += expect64("b", b, 255L);
                fails += expect64("c", c, 32767L);
                fails += expect64("d", d, 65535L);
                fails += expect64("e", e, float64(int32(0x7fffffffUL)));
                fails += expect64("f", f, float64(uint32(0xffffffffUL)));
                fails += expect64("g", g, float64(int64(0x7fffffffffffffffUL)));
                fails += expect64("h", h, float64(uint64(0xffffffffffffffffUL)));
                fails += expect64("i", i, float64(float32(3.402823E38F)));
            }
            { 
                // Check minimum values (and tweaks for unsigned)
                (a, b, c, d, e, f, g, h, i) = conv2Float64_ssa(-128L, 254L, -32768L, 65534L, ~0x7fffffffUL, 0xfffffffeUL, ~0x7fffFFFFffffFFFFUL, 0xffffFFFFffffF401UL, 1.5E-45F);
                fails += expect64("a", a, -128L);
                fails += expect64("b", b, 254L);
                fails += expect64("c", c, -32768L);
                fails += expect64("d", d, 65534L);
                fails += expect64("e", e, float64(~int32(0x7fffffffUL)));
                fails += expect64("f", f, float64(uint32(0xfffffffeUL)));
                fails += expect64("g", g, float64(~int64(0x7fffffffffffffffUL)));
                fails += expect64("h", h, float64(uint64(0xfffffffffffff401UL)));
                fails += expect64("i", i, float64(float32(1.5E-45F)));
            }
            { 
                // Check maximum values
                (a, b, c, d, e, f, g, h, i) = conv2Float32_ssa(127L, 255L, 32767L, 65535L, 0x7fffffffUL, 0xffffffffUL, 0x7fffFFFFffffFFFFUL, 0xffffFFFFffffFFFFUL, 3.402823E38F);
                fails += expect32("a", a, 127L);
                fails += expect32("b", b, 255L);
                fails += expect32("c", c, 32767L);
                fails += expect32("d", d, 65535L);
                fails += expect32("e", e, float32(int32(0x7fffffffUL)));
                fails += expect32("f", f, float32(uint32(0xffffffffUL)));
                fails += expect32("g", g, float32(int64(0x7fffffffffffffffUL)));
                fails += expect32("h", h, float32(uint64(0xffffffffffffffffUL)));
                fails += expect32("i", i, float32(float64(3.402823E38F)));
            }
            { 
                // Check minimum values (and tweaks for unsigned)
                (a, b, c, d, e, f, g, h, i) = conv2Float32_ssa(-128L, 254L, -32768L, 65534L, ~0x7fffffffUL, 0xfffffffeUL, ~0x7fffFFFFffffFFFFUL, 0xffffFFFFffffF401UL, 1.5E-45F);
                fails += expect32("a", a, -128L);
                fails += expect32("b", b, 254L);
                fails += expect32("c", c, -32768L);
                fails += expect32("d", d, 65534L);
                fails += expect32("e", e, float32(~int32(0x7fffffffUL)));
                fails += expect32("f", f, float32(uint32(0xfffffffeUL)));
                fails += expect32("g", g, float32(~int64(0x7fffffffffffffffUL)));
                fails += expect32("h", h, float32(uint64(0xfffffffffffff401UL)));
                fails += expect32("i", i, float32(float64(1.5E-45F)));
            }
            return fails;
        }

        private static long multiplyAdd()
        {
            long fails = 0L;
            { 
                // Test that a multiply-accumulate operation with intermediate
                // rounding forced by a float32() cast produces the expected
                // result.
                // Test cases generated experimentally on a system (s390x) that
                // supports fused multiply-add instructions.

                Func<@string, float, float, long> check = (s, got, expected) =>
                {
                    if (got != expected)
                    {
                        fmt.Printf("multiplyAdd: %s, expected %g, got %g\n", s, expected, got);
                        return 1L;
                    }
                    return 0L;
                }
;
                {
                    var t__prev1 = t;

                    foreach (var (_, __t) in tests)
                    {
                        t = __t;
                        fails += check(fmt.Sprintf("float32(%v * %v) + %v", t.x, t.y, t.z), (x, y, z) =>
                        {
                            return float32(x * y) + z;
                        }(t.x, t.y, t.z), t.res);

                        fails += check(fmt.Sprintf("%v += float32(%v * %v)", t.z, t.x, t.y), (x, y, z) =>
                        {
                            z += float32(x * y);
                            return z;
                        }(t.x, t.y, t.z), t.res);
                    }

                    t = t__prev1;
                }

            }
            { 
                // Test that a multiply-accumulate operation with intermediate
                // rounding forced by a float64() cast produces the expected
                // result.
                // Test cases generated experimentally on a system (s390x) that
                // supports fused multiply-add instructions.

                check = (s, got, expected) =>
                {
                    if (got != expected)
                    {
                        fmt.Printf("multiplyAdd: %s, expected %g, got %g\n", s, expected, got);
                        return 1L;
                    }
                    return 0L;
                }
;
                {
                    var t__prev1 = t;

                    foreach (var (_, __t) in tests)
                    {
                        t = __t;
                        fails += check(fmt.Sprintf("float64(%v * %v) + %v", t.x, t.y, t.z), (x, y, z) =>
                        {
                            return float64(x * y) + z;
                        }(t.x, t.y, t.z), t.res);

                        fails += check(fmt.Sprintf("%v += float64(%v * %v)", t.z, t.x, t.y), (x, y, z) =>
                        {
                            z += float64(x * y);
                            return z;
                        }(t.x, t.y, t.z), t.res);
                    }

                    t = t__prev1;
                }

            }
            { 
                // Test that a multiply-accumulate operation with intermediate
                // rounding forced by a complex128() cast produces the expected
                // result.
                // Test cases generated experimentally on a system (s390x) that
                // supports fused multiply-add instructions.

                check = (s, got, expected) =>
                {
                    if (got != expected)
                    {
                        fmt.Printf("multiplyAdd: %s, expected %v, got %v\n", s, expected, got);
                        return 1L;
                    }
                    return 0L;
                }
;
                {
                    var t__prev1 = t;

                    foreach (var (_, __t) in tests)
                    {
                        t = __t;
                        fails += check(fmt.Sprintf("complex128(complex(%v, 1)*3) + complex(%v, 0)", t.x, t.y), (x, y) =>
                        {
                            return complex128(complex(x, 1L) * 3L) + complex(y, 0L);
                        }(t.x, t.y), t.res);

                        fails += check(fmt.Sprintf("z := complex(%v, 1); z += complex128(complex(%v, 1) * 3)", t.y, t.x), (x, y) =>
                        {
                            var z = complex(y, 0L);
                            z += complex128(complex(x, 1L) * 3L);
                            return z;
                        }(t.x, t.y), t.res);
                    }

                    t = t__prev1;
                }

            }
            return fails;
        }

        private static readonly ulong aa = 0x1000000000000000UL;
        private static readonly ulong ab = 0x100000000000000UL;
        private static readonly ulong ac = 0x10000000000000UL;
        private static readonly ulong ad = 0x1000000000000UL;
        private static readonly ulong ba = 0x100000000000UL;
        private static readonly ulong bb = 0x10000000000UL;
        private static readonly ulong bc = 0x1000000000UL;
        private static readonly ulong bd = 0x100000000UL;
        private static readonly ulong ca = 0x10000000UL;
        private static readonly ulong cb = 0x1000000UL;
        private static readonly ulong cc = 0x100000UL;
        private static readonly ulong cd = 0x10000UL;
        private static readonly ulong da = 0x1000UL;
        private static readonly ulong db = 0x100UL;
        private static readonly ulong dc = 0x10UL;
        private static readonly ulong dd = 0x1UL;

        //go:noinline
        private static (ulong, ulong, ulong, ulong, ulong, ulong) compares64_ssa(double a, double b, double c, double d)
        {
            if (a < a)
            {
                lt += aa;
            }
            if (a < b)
            {
                lt += ab;
            }
            if (a < c)
            {
                lt += ac;
            }
            if (a < d)
            {
                lt += ad;
            }
            if (b < a)
            {
                lt += ba;
            }
            if (b < b)
            {
                lt += bb;
            }
            if (b < c)
            {
                lt += bc;
            }
            if (b < d)
            {
                lt += bd;
            }
            if (c < a)
            {
                lt += ca;
            }
            if (c < b)
            {
                lt += cb;
            }
            if (c < c)
            {
                lt += cc;
            }
            if (c < d)
            {
                lt += cd;
            }
            if (d < a)
            {
                lt += da;
            }
            if (d < b)
            {
                lt += db;
            }
            if (d < c)
            {
                lt += dc;
            }
            if (d < d)
            {
                lt += dd;
            }
            if (a <= a)
            {
                le += aa;
            }
            if (a <= b)
            {
                le += ab;
            }
            if (a <= c)
            {
                le += ac;
            }
            if (a <= d)
            {
                le += ad;
            }
            if (b <= a)
            {
                le += ba;
            }
            if (b <= b)
            {
                le += bb;
            }
            if (b <= c)
            {
                le += bc;
            }
            if (b <= d)
            {
                le += bd;
            }
            if (c <= a)
            {
                le += ca;
            }
            if (c <= b)
            {
                le += cb;
            }
            if (c <= c)
            {
                le += cc;
            }
            if (c <= d)
            {
                le += cd;
            }
            if (d <= a)
            {
                le += da;
            }
            if (d <= b)
            {
                le += db;
            }
            if (d <= c)
            {
                le += dc;
            }
            if (d <= d)
            {
                le += dd;
            }
            if (a == a)
            {
                eq += aa;
            }
            if (a == b)
            {
                eq += ab;
            }
            if (a == c)
            {
                eq += ac;
            }
            if (a == d)
            {
                eq += ad;
            }
            if (b == a)
            {
                eq += ba;
            }
            if (b == b)
            {
                eq += bb;
            }
            if (b == c)
            {
                eq += bc;
            }
            if (b == d)
            {
                eq += bd;
            }
            if (c == a)
            {
                eq += ca;
            }
            if (c == b)
            {
                eq += cb;
            }
            if (c == c)
            {
                eq += cc;
            }
            if (c == d)
            {
                eq += cd;
            }
            if (d == a)
            {
                eq += da;
            }
            if (d == b)
            {
                eq += db;
            }
            if (d == c)
            {
                eq += dc;
            }
            if (d == d)
            {
                eq += dd;
            }
            if (a != a)
            {
                ne += aa;
            }
            if (a != b)
            {
                ne += ab;
            }
            if (a != c)
            {
                ne += ac;
            }
            if (a != d)
            {
                ne += ad;
            }
            if (b != a)
            {
                ne += ba;
            }
            if (b != b)
            {
                ne += bb;
            }
            if (b != c)
            {
                ne += bc;
            }
            if (b != d)
            {
                ne += bd;
            }
            if (c != a)
            {
                ne += ca;
            }
            if (c != b)
            {
                ne += cb;
            }
            if (c != c)
            {
                ne += cc;
            }
            if (c != d)
            {
                ne += cd;
            }
            if (d != a)
            {
                ne += da;
            }
            if (d != b)
            {
                ne += db;
            }
            if (d != c)
            {
                ne += dc;
            }
            if (d != d)
            {
                ne += dd;
            }
            if (a >= a)
            {
                ge += aa;
            }
            if (a >= b)
            {
                ge += ab;
            }
            if (a >= c)
            {
                ge += ac;
            }
            if (a >= d)
            {
                ge += ad;
            }
            if (b >= a)
            {
                ge += ba;
            }
            if (b >= b)
            {
                ge += bb;
            }
            if (b >= c)
            {
                ge += bc;
            }
            if (b >= d)
            {
                ge += bd;
            }
            if (c >= a)
            {
                ge += ca;
            }
            if (c >= b)
            {
                ge += cb;
            }
            if (c >= c)
            {
                ge += cc;
            }
            if (c >= d)
            {
                ge += cd;
            }
            if (d >= a)
            {
                ge += da;
            }
            if (d >= b)
            {
                ge += db;
            }
            if (d >= c)
            {
                ge += dc;
            }
            if (d >= d)
            {
                ge += dd;
            }
            if (a > a)
            {
                gt += aa;
            }
            if (a > b)
            {
                gt += ab;
            }
            if (a > c)
            {
                gt += ac;
            }
            if (a > d)
            {
                gt += ad;
            }
            if (b > a)
            {
                gt += ba;
            }
            if (b > b)
            {
                gt += bb;
            }
            if (b > c)
            {
                gt += bc;
            }
            if (b > d)
            {
                gt += bd;
            }
            if (c > a)
            {
                gt += ca;
            }
            if (c > b)
            {
                gt += cb;
            }
            if (c > c)
            {
                gt += cc;
            }
            if (c > d)
            {
                gt += cd;
            }
            if (d > a)
            {
                gt += da;
            }
            if (d > b)
            {
                gt += db;
            }
            if (d > c)
            {
                gt += dc;
            }
            if (d > d)
            {
                gt += dd;
            }
            return;
        }

        //go:noinline
        private static (ulong, ulong, ulong, ulong, ulong, ulong) compares32_ssa(float a, float b, float c, float d)
        {
            if (a < a)
            {
                lt += aa;
            }
            if (a < b)
            {
                lt += ab;
            }
            if (a < c)
            {
                lt += ac;
            }
            if (a < d)
            {
                lt += ad;
            }
            if (b < a)
            {
                lt += ba;
            }
            if (b < b)
            {
                lt += bb;
            }
            if (b < c)
            {
                lt += bc;
            }
            if (b < d)
            {
                lt += bd;
            }
            if (c < a)
            {
                lt += ca;
            }
            if (c < b)
            {
                lt += cb;
            }
            if (c < c)
            {
                lt += cc;
            }
            if (c < d)
            {
                lt += cd;
            }
            if (d < a)
            {
                lt += da;
            }
            if (d < b)
            {
                lt += db;
            }
            if (d < c)
            {
                lt += dc;
            }
            if (d < d)
            {
                lt += dd;
            }
            if (a <= a)
            {
                le += aa;
            }
            if (a <= b)
            {
                le += ab;
            }
            if (a <= c)
            {
                le += ac;
            }
            if (a <= d)
            {
                le += ad;
            }
            if (b <= a)
            {
                le += ba;
            }
            if (b <= b)
            {
                le += bb;
            }
            if (b <= c)
            {
                le += bc;
            }
            if (b <= d)
            {
                le += bd;
            }
            if (c <= a)
            {
                le += ca;
            }
            if (c <= b)
            {
                le += cb;
            }
            if (c <= c)
            {
                le += cc;
            }
            if (c <= d)
            {
                le += cd;
            }
            if (d <= a)
            {
                le += da;
            }
            if (d <= b)
            {
                le += db;
            }
            if (d <= c)
            {
                le += dc;
            }
            if (d <= d)
            {
                le += dd;
            }
            if (a == a)
            {
                eq += aa;
            }
            if (a == b)
            {
                eq += ab;
            }
            if (a == c)
            {
                eq += ac;
            }
            if (a == d)
            {
                eq += ad;
            }
            if (b == a)
            {
                eq += ba;
            }
            if (b == b)
            {
                eq += bb;
            }
            if (b == c)
            {
                eq += bc;
            }
            if (b == d)
            {
                eq += bd;
            }
            if (c == a)
            {
                eq += ca;
            }
            if (c == b)
            {
                eq += cb;
            }
            if (c == c)
            {
                eq += cc;
            }
            if (c == d)
            {
                eq += cd;
            }
            if (d == a)
            {
                eq += da;
            }
            if (d == b)
            {
                eq += db;
            }
            if (d == c)
            {
                eq += dc;
            }
            if (d == d)
            {
                eq += dd;
            }
            if (a != a)
            {
                ne += aa;
            }
            if (a != b)
            {
                ne += ab;
            }
            if (a != c)
            {
                ne += ac;
            }
            if (a != d)
            {
                ne += ad;
            }
            if (b != a)
            {
                ne += ba;
            }
            if (b != b)
            {
                ne += bb;
            }
            if (b != c)
            {
                ne += bc;
            }
            if (b != d)
            {
                ne += bd;
            }
            if (c != a)
            {
                ne += ca;
            }
            if (c != b)
            {
                ne += cb;
            }
            if (c != c)
            {
                ne += cc;
            }
            if (c != d)
            {
                ne += cd;
            }
            if (d != a)
            {
                ne += da;
            }
            if (d != b)
            {
                ne += db;
            }
            if (d != c)
            {
                ne += dc;
            }
            if (d != d)
            {
                ne += dd;
            }
            if (a >= a)
            {
                ge += aa;
            }
            if (a >= b)
            {
                ge += ab;
            }
            if (a >= c)
            {
                ge += ac;
            }
            if (a >= d)
            {
                ge += ad;
            }
            if (b >= a)
            {
                ge += ba;
            }
            if (b >= b)
            {
                ge += bb;
            }
            if (b >= c)
            {
                ge += bc;
            }
            if (b >= d)
            {
                ge += bd;
            }
            if (c >= a)
            {
                ge += ca;
            }
            if (c >= b)
            {
                ge += cb;
            }
            if (c >= c)
            {
                ge += cc;
            }
            if (c >= d)
            {
                ge += cd;
            }
            if (d >= a)
            {
                ge += da;
            }
            if (d >= b)
            {
                ge += db;
            }
            if (d >= c)
            {
                ge += dc;
            }
            if (d >= d)
            {
                ge += dd;
            }
            if (a > a)
            {
                gt += aa;
            }
            if (a > b)
            {
                gt += ab;
            }
            if (a > c)
            {
                gt += ac;
            }
            if (a > d)
            {
                gt += ad;
            }
            if (b > a)
            {
                gt += ba;
            }
            if (b > b)
            {
                gt += bb;
            }
            if (b > c)
            {
                gt += bc;
            }
            if (b > d)
            {
                gt += bd;
            }
            if (c > a)
            {
                gt += ca;
            }
            if (c > b)
            {
                gt += cb;
            }
            if (c > c)
            {
                gt += cc;
            }
            if (c > d)
            {
                gt += cd;
            }
            if (d > a)
            {
                gt += da;
            }
            if (d > b)
            {
                gt += db;
            }
            if (d > c)
            {
                gt += dc;
            }
            if (d > d)
            {
                gt += dd;
            }
            return;
        }

        //go:noinline
        private static bool le64_ssa(double x, double y)
        {
            return x <= y;
        }

        //go:noinline
        private static bool ge64_ssa(double x, double y)
        {
            return x >= y;
        }

        //go:noinline
        private static bool lt64_ssa(double x, double y)
        {
            return x < y;
        }

        //go:noinline
        private static bool gt64_ssa(double x, double y)
        {
            return x > y;
        }

        //go:noinline
        private static bool eq64_ssa(double x, double y)
        {
            return x == y;
        }

        //go:noinline
        private static bool ne64_ssa(double x, double y)
        {
            return x != y;
        }

        //go:noinline
        private static double eqbr64_ssa(double x, double y)
        {
            if (x == y)
            {
                return 17L;
            }
            return 42L;
        }

        //go:noinline
        private static double nebr64_ssa(double x, double y)
        {
            if (x != y)
            {
                return 17L;
            }
            return 42L;
        }

        //go:noinline
        private static double gebr64_ssa(double x, double y)
        {
            if (x >= y)
            {
                return 17L;
            }
            return 42L;
        }

        //go:noinline
        private static double lebr64_ssa(double x, double y)
        {
            if (x <= y)
            {
                return 17L;
            }
            return 42L;
        }

        //go:noinline
        private static double ltbr64_ssa(double x, double y)
        {
            if (x < y)
            {
                return 17L;
            }
            return 42L;
        }

        //go:noinline
        private static double gtbr64_ssa(double x, double y)
        {
            if (x > y)
            {
                return 17L;
            }
            return 42L;
        }

        //go:noinline
        private static bool le32_ssa(float x, float y)
        {
            return x <= y;
        }

        //go:noinline
        private static bool ge32_ssa(float x, float y)
        {
            return x >= y;
        }

        //go:noinline
        private static bool lt32_ssa(float x, float y)
        {
            return x < y;
        }

        //go:noinline
        private static bool gt32_ssa(float x, float y)
        {
            return x > y;
        }

        //go:noinline
        private static bool eq32_ssa(float x, float y)
        {
            return x == y;
        }

        //go:noinline
        private static bool ne32_ssa(float x, float y)
        {
            return x != y;
        }

        //go:noinline
        private static float eqbr32_ssa(float x, float y)
        {
            if (x == y)
            {
                return 17L;
            }
            return 42L;
        }

        //go:noinline
        private static float nebr32_ssa(float x, float y)
        {
            if (x != y)
            {
                return 17L;
            }
            return 42L;
        }

        //go:noinline
        private static float gebr32_ssa(float x, float y)
        {
            if (x >= y)
            {
                return 17L;
            }
            return 42L;
        }

        //go:noinline
        private static float lebr32_ssa(float x, float y)
        {
            if (x <= y)
            {
                return 17L;
            }
            return 42L;
        }

        //go:noinline
        private static float ltbr32_ssa(float x, float y)
        {
            if (x < y)
            {
                return 17L;
            }
            return 42L;
        }

        //go:noinline
        private static float gtbr32_ssa(float x, float y)
        {
            if (x > y)
            {
                return 17L;
            }
            return 42L;
        }

        //go:noinline
        public static byte F32toU8_ssa(float x)
        {
            return uint8(x);
        }

        //go:noinline
        public static sbyte F32toI8_ssa(float x)
        {
            return int8(x);
        }

        //go:noinline
        public static ushort F32toU16_ssa(float x)
        {
            return uint16(x);
        }

        //go:noinline
        public static short F32toI16_ssa(float x)
        {
            return int16(x);
        }

        //go:noinline
        public static uint F32toU32_ssa(float x)
        {
            return uint32(x);
        }

        //go:noinline
        public static int F32toI32_ssa(float x)
        {
            return int32(x);
        }

        //go:noinline
        public static ulong F32toU64_ssa(float x)
        {
            return uint64(x);
        }

        //go:noinline
        public static long F32toI64_ssa(float x)
        {
            return int64(x);
        }

        //go:noinline
        public static byte F64toU8_ssa(double x)
        {
            return uint8(x);
        }

        //go:noinline
        public static sbyte F64toI8_ssa(double x)
        {
            return int8(x);
        }

        //go:noinline
        public static ushort F64toU16_ssa(double x)
        {
            return uint16(x);
        }

        //go:noinline
        public static short F64toI16_ssa(double x)
        {
            return int16(x);
        }

        //go:noinline
        public static uint F64toU32_ssa(double x)
        {
            return uint32(x);
        }

        //go:noinline
        public static int F64toI32_ssa(double x)
        {
            return int32(x);
        }

        //go:noinline
        public static ulong F64toU64_ssa(double x)
        {
            return uint64(x);
        }

        //go:noinline
        public static long F64toI64_ssa(double x)
        {
            return int64(x);
        }

        private static long floatsToInts(double x, long expected)
        {
            var y = float32(x);
            long fails = 0L;
            fails += expectInt64("F64toI8", int64(F64toI8_ssa(x)), expected);
            fails += expectInt64("F64toI16", int64(F64toI16_ssa(x)), expected);
            fails += expectInt64("F64toI32", int64(F64toI32_ssa(x)), expected);
            fails += expectInt64("F64toI64", int64(F64toI64_ssa(x)), expected);
            fails += expectInt64("F32toI8", int64(F32toI8_ssa(y)), expected);
            fails += expectInt64("F32toI16", int64(F32toI16_ssa(y)), expected);
            fails += expectInt64("F32toI32", int64(F32toI32_ssa(y)), expected);
            fails += expectInt64("F32toI64", int64(F32toI64_ssa(y)), expected);
            return fails;
        }

        private static long floatsToUints(double x, ulong expected)
        {
            var y = float32(x);
            long fails = 0L;
            fails += expectUint64("F64toU8", uint64(F64toU8_ssa(x)), expected);
            fails += expectUint64("F64toU16", uint64(F64toU16_ssa(x)), expected);
            fails += expectUint64("F64toU32", uint64(F64toU32_ssa(x)), expected);
            fails += expectUint64("F64toU64", uint64(F64toU64_ssa(x)), expected);
            fails += expectUint64("F32toU8", uint64(F32toU8_ssa(y)), expected);
            fails += expectUint64("F32toU16", uint64(F32toU16_ssa(y)), expected);
            fails += expectUint64("F32toU32", uint64(F32toU32_ssa(y)), expected);
            fails += expectUint64("F32toU64", uint64(F32toU64_ssa(y)), expected);
            return fails;
        }

        private static long floatingToIntegerConversionsTest()
        {
            long fails = 0L;
            fails += floatsToInts(0.0F, 0L);
            fails += floatsToInts(0.5F, 0L);
            fails += floatsToInts(0.9F, 0L);
            fails += floatsToInts(1.0F, 1L);
            fails += floatsToInts(1.5F, 1L);
            fails += floatsToInts(127.0F, 127L);
            fails += floatsToInts(-1.0F, -1L);
            fails += floatsToInts(-128.0F, -128L);

            fails += floatsToUints(0.0F, 0L);
            fails += floatsToUints(1.0F, 1L);
            fails += floatsToUints(255.0F, 255L);

            {
                var j__prev1 = j;

                for (var j = uint(0L); j < 24L; j++)
                { 
                    // Avoid hard cases in the construction
                    // of the test inputs.
                    var v = int64(1L << (int)(62L)) | int64(1L << (int)((62L - j)));
                    var w = uint64(v);
                    var f = float32(v);
                    var d = float64(v);
                    fails += expectUint64("2**62...", F32toU64_ssa(f), w);
                    fails += expectUint64("2**62...", F64toU64_ssa(d), w);
                    fails += expectInt64("2**62...", F32toI64_ssa(f), v);
                    fails += expectInt64("2**62...", F64toI64_ssa(d), v);
                    fails += expectInt64("2**62...", F32toI64_ssa(-f), -v);
                    fails += expectInt64("2**62...", F64toI64_ssa(-d), -v);
                    w += w;
                    f += f;
                    d += d;
                    fails += expectUint64("2**63...", F32toU64_ssa(f), w);
                    fails += expectUint64("2**63...", F64toU64_ssa(d), w);
                }


                j = j__prev1;
            }

            {
                var j__prev1 = j;

                for (j = uint(0L); j < 16L; j++)
                { 
                    // Avoid hard cases in the construction
                    // of the test inputs.
                    v = int32(1L << (int)(30L)) | int32(1L << (int)((30L - j)));
                    w = uint32(v);
                    f = float32(v);
                    d = float64(v);
                    fails += expectUint32("2**30...", F32toU32_ssa(f), w);
                    fails += expectUint32("2**30...", F64toU32_ssa(d), w);
                    fails += expectInt32("2**30...", F32toI32_ssa(f), v);
                    fails += expectInt32("2**30...", F64toI32_ssa(d), v);
                    fails += expectInt32("2**30...", F32toI32_ssa(-f), -v);
                    fails += expectInt32("2**30...", F64toI32_ssa(-d), -v);
                    w += w;
                    f += f;
                    d += d;
                    fails += expectUint32("2**31...", F32toU32_ssa(f), w);
                    fails += expectUint32("2**31...", F64toU32_ssa(d), w);
                }


                j = j__prev1;
            }

            {
                var j__prev1 = j;

                for (j = uint(0L); j < 15L; j++)
                { 
                    // Avoid hard cases in the construction
                    // of the test inputs.
                    v = int16(1L << (int)(14L)) | int16(1L << (int)((14L - j)));
                    w = uint16(v);
                    f = float32(v);
                    d = float64(v);
                    fails += expectUint16("2**14...", F32toU16_ssa(f), w);
                    fails += expectUint16("2**14...", F64toU16_ssa(d), w);
                    fails += expectInt16("2**14...", F32toI16_ssa(f), v);
                    fails += expectInt16("2**14...", F64toI16_ssa(d), v);
                    fails += expectInt16("2**14...", F32toI16_ssa(-f), -v);
                    fails += expectInt16("2**14...", F64toI16_ssa(-d), -v);
                    w += w;
                    f += f;
                    d += d;
                    fails += expectUint16("2**15...", F32toU16_ssa(f), w);
                    fails += expectUint16("2**15...", F64toU16_ssa(d), w);
                }


                j = j__prev1;
            }

            fails += expectInt32("-2147483648", F32toI32_ssa(-2147483648L), -2147483648L);

            fails += expectInt32("-2147483648", F64toI32_ssa(-2147483648L), -2147483648L);
            fails += expectInt32("-2147483647", F64toI32_ssa(-2147483647L), -2147483647L);
            fails += expectUint32("4294967295", F64toU32_ssa(4294967295L), 4294967295L);

            fails += expectInt16("-32768", F64toI16_ssa(-32768L), -32768L);
            fails += expectInt16("-32768", F32toI16_ssa(-32768L), -32768L); 

            // NB more of a pain to do these for 32-bit because of lost bits in Float32 mantissa
            fails += expectInt16("32767", F64toI16_ssa(32767L), 32767L);
            fails += expectInt16("32767", F32toI16_ssa(32767L), 32767L);
            fails += expectUint16("32767", F64toU16_ssa(32767L), 32767L);
            fails += expectUint16("32767", F32toU16_ssa(32767L), 32767L);
            fails += expectUint16("65535", F64toU16_ssa(65535L), 65535L);
            fails += expectUint16("65535", F32toU16_ssa(65535L), 65535L);

            return fails;
        }

        private static long fail64(@string s, Func<double, double, double> f, double a, double b, double e)
        {
            var d = f(a, b);
            if (d != e)
            {
                fmt.Printf("For (float64) %v %v %v, expected %v, got %v\n", a, s, b, e, d);
                return 1L;
            }
            return 0L;
        }

        private static long fail64bool(@string s, Func<double, double, bool> f, double a, double b, bool e)
        {
            var d = f(a, b);
            if (d != e)
            {
                fmt.Printf("For (float64) %v %v %v, expected %v, got %v\n", a, s, b, e, d);
                return 1L;
            }
            return 0L;
        }

        private static long fail32(@string s, Func<float, float, float> f, float a, float b, float e)
        {
            var d = f(a, b);
            if (d != e)
            {
                fmt.Printf("For (float32) %v %v %v, expected %v, got %v\n", a, s, b, e, d);
                return 1L;
            }
            return 0L;
        }

        private static long fail32bool(@string s, Func<float, float, bool> f, float a, float b, bool e)
        {
            var d = f(a, b);
            if (d != e)
            {
                fmt.Printf("For (float32) %v %v %v, expected %v, got %v\n", a, s, b, e, d);
                return 1L;
            }
            return 0L;
        }

        private static long expect64(@string s, double x, double expected)
        {
            if (x != expected)
            {
                println("F64 Expected", expected, "for", s, ", got", x);
                return 1L;
            }
            return 0L;
        }

        private static long expect32(@string s, float x, float expected)
        {
            if (x != expected)
            {
                println("F32 Expected", expected, "for", s, ", got", x);
                return 1L;
            }
            return 0L;
        }

        private static long expectUint64(@string s, ulong x, ulong expected)
        {
            if (x != expected)
            {
                fmt.Printf("U64 Expected 0x%016x for %s, got 0x%016x\n", expected, s, x);
                return 1L;
            }
            return 0L;
        }

        private static long expectInt64(@string s, long x, long expected)
        {
            if (x != expected)
            {
                fmt.Printf("%s: Expected 0x%016x, got 0x%016x\n", s, expected, x);
                return 1L;
            }
            return 0L;
        }

        private static long expectUint32(@string s, uint x, uint expected)
        {
            if (x != expected)
            {
                fmt.Printf("U32 %s: Expected 0x%08x, got 0x%08x\n", s, expected, x);
                return 1L;
            }
            return 0L;
        }

        private static long expectInt32(@string s, int x, int expected)
        {
            if (x != expected)
            {
                fmt.Printf("I32 %s: Expected 0x%08x, got 0x%08x\n", s, expected, x);
                return 1L;
            }
            return 0L;
        }

        private static long expectUint16(@string s, ushort x, ushort expected)
        {
            if (x != expected)
            {
                fmt.Printf("U16 %s: Expected 0x%04x, got 0x%04x\n", s, expected, x);
                return 1L;
            }
            return 0L;
        }

        private static long expectInt16(@string s, short x, short expected)
        {
            if (x != expected)
            {
                fmt.Printf("I16 %s: Expected 0x%04x, got 0x%04x\n", s, expected, x);
                return 1L;
            }
            return 0L;
        }

        private static long expectAll64(@string s, double expected, double a, double b, double c, double d, double e, double f, double g, double h, double i)
        {
            long fails = 0L;
            fails += expect64(s + ":a", a, expected);
            fails += expect64(s + ":b", b, expected);
            fails += expect64(s + ":c", c, expected);
            fails += expect64(s + ":d", d, expected);
            fails += expect64(s + ":e", e, expected);
            fails += expect64(s + ":f", f, expected);
            fails += expect64(s + ":g", g, expected);
            return fails;
        }

        private static long expectAll32(@string s, float expected, float a, float b, float c, float d, float e, float f, float g, float h, float i)
        {
            long fails = 0L;
            fails += expect32(s + ":a", a, expected);
            fails += expect32(s + ":b", b, expected);
            fails += expect32(s + ":c", c, expected);
            fails += expect32(s + ":d", d, expected);
            fails += expect32(s + ":e", e, expected);
            fails += expect32(s + ":f", f, expected);
            fails += expect32(s + ":g", g, expected);
            return fails;
        }

        private static array<double> ev64 = new array<double>(new double[] { 42.0, 17.0 });
        private static array<float> ev32 = new array<float>(new float[] { 42.0, 17.0 });

        private static long cmpOpTest(@string s, Func<double, double, bool> f, Func<double, double, double> g, Func<float, float, bool> ff, Func<float, float, float> gg, double zero, double one, double inf, double nan, ulong result)
        {
            long fails = 0L;
            fails += fail64bool(s, f, zero, zero, result >> (int)(16L) & 1L == 1L);
            fails += fail64bool(s, f, zero, one, result >> (int)(12L) & 1L == 1L);
            fails += fail64bool(s, f, zero, inf, result >> (int)(8L) & 1L == 1L);
            fails += fail64bool(s, f, zero, nan, result >> (int)(4L) & 1L == 1L);
            fails += fail64bool(s, f, nan, nan, result & 1L == 1L);

            fails += fail64(s, g, zero, zero, ev64[result >> (int)(16L) & 1L]);
            fails += fail64(s, g, zero, one, ev64[result >> (int)(12L) & 1L]);
            fails += fail64(s, g, zero, inf, ev64[result >> (int)(8L) & 1L]);
            fails += fail64(s, g, zero, nan, ev64[result >> (int)(4L) & 1L]);
            fails += fail64(s, g, nan, nan, ev64[result >> (int)(0L) & 1L]);

            {
                var zero = float32(zero);
                var one = float32(one);
                var inf = float32(inf);
                var nan = float32(nan);
                fails += fail32bool(s, ff, zero, zero, (result >> (int)(16L)) & 1L == 1L);
                fails += fail32bool(s, ff, zero, one, (result >> (int)(12L)) & 1L == 1L);
                fails += fail32bool(s, ff, zero, inf, (result >> (int)(8L)) & 1L == 1L);
                fails += fail32bool(s, ff, zero, nan, (result >> (int)(4L)) & 1L == 1L);
                fails += fail32bool(s, ff, nan, nan, result & 1L == 1L);

                fails += fail32(s, gg, zero, zero, ev32[(result >> (int)(16L)) & 1L]);
                fails += fail32(s, gg, zero, one, ev32[(result >> (int)(12L)) & 1L]);
                fails += fail32(s, gg, zero, inf, ev32[(result >> (int)(8L)) & 1L]);
                fails += fail32(s, gg, zero, nan, ev32[(result >> (int)(4L)) & 1L]);
                fails += fail32(s, gg, nan, nan, ev32[(result >> (int)(0L)) & 1L]);
            }
            return fails;
        }

        private static long expectCx128(@string s, System.Numerics.Complex128 x, System.Numerics.Complex128 expected)
        {
            if (x != expected)
            {
                println("Cx 128 Expected", expected, "for", s, ", got", x);
                return 1L;
            }
            return 0L;
        }

        private static long expectCx64(@string s, complex64 x, complex64 expected)
        {
            if (x != expected)
            {
                println("Cx 64 Expected", expected, "for", s, ", got", x);
                return 1L;
            }
            return 0L;
        }

        //go:noinline
        private static System.Numerics.Complex128 cx128sum_ssa(System.Numerics.Complex128 a, System.Numerics.Complex128 b)
        {
            return a + b;
        }

        //go:noinline
        private static System.Numerics.Complex128 cx128diff_ssa(System.Numerics.Complex128 a, System.Numerics.Complex128 b)
        {
            return a - b;
        }

        //go:noinline
        private static System.Numerics.Complex128 cx128prod_ssa(System.Numerics.Complex128 a, System.Numerics.Complex128 b)
        {
            return a * b;
        }

        //go:noinline
        private static System.Numerics.Complex128 cx128quot_ssa(System.Numerics.Complex128 a, System.Numerics.Complex128 b)
        {
            return a / b;
        }

        //go:noinline
        private static System.Numerics.Complex128 cx128neg_ssa(System.Numerics.Complex128 a)
        {
            return -a;
        }

        //go:noinline
        private static double cx128real_ssa(System.Numerics.Complex128 a)
        {
            return real(a);
        }

        //go:noinline
        private static double cx128imag_ssa(System.Numerics.Complex128 a)
        {
            return imag(a);
        }

        //go:noinline
        private static System.Numerics.Complex128 cx128cnst_ssa(System.Numerics.Complex128 a)
        {
            long b = 2L + 3iUL;
            return a * b;
        }

        //go:noinline
        private static complex64 cx64sum_ssa(complex64 a, complex64 b)
        {
            return a + b;
        }

        //go:noinline
        private static complex64 cx64diff_ssa(complex64 a, complex64 b)
        {
            return a - b;
        }

        //go:noinline
        private static complex64 cx64prod_ssa(complex64 a, complex64 b)
        {
            return a * b;
        }

        //go:noinline
        private static complex64 cx64quot_ssa(complex64 a, complex64 b)
        {
            return a / b;
        }

        //go:noinline
        private static complex64 cx64neg_ssa(complex64 a)
        {
            return -a;
        }

        //go:noinline
        private static float cx64real_ssa(complex64 a)
        {
            return real(a);
        }

        //go:noinline
        private static float cx64imag_ssa(complex64 a)
        {
            return imag(a);
        }

        //go:noinline
        private static bool cx128eq_ssa(System.Numerics.Complex128 a, System.Numerics.Complex128 b)
        {
            return a == b;
        }

        //go:noinline
        private static bool cx128ne_ssa(System.Numerics.Complex128 a, System.Numerics.Complex128 b)
        {
            return a != b;
        }

        //go:noinline
        private static bool cx64eq_ssa(complex64 a, complex64 b)
        {
            return a == b;
        }

        //go:noinline
        private static bool cx64ne_ssa(complex64 a, complex64 b)
        {
            return a != b;
        }

        private static long expectTrue(@string s, bool b)
        {
            if (!b)
            {
                println("expected true for", s, ", got false");
                return 1L;
            }
            return 0L;
        }
        private static long expectFalse(@string s, bool b)
        {
            if (b)
            {
                println("expected false for", s, ", got true");
                return 1L;
            }
            return 0L;
        }

        private static long complexTest128()
        {
            long fails = 0L;
            System.Numerics.Complex128 a = 1L + 2iUL;
            System.Numerics.Complex128 b = 3L + 6iUL;
            var sum = cx128sum_ssa(b, a);
            var diff = cx128diff_ssa(b, a);
            var prod = cx128prod_ssa(b, a);
            var quot = cx128quot_ssa(b, a);
            var neg = cx128neg_ssa(a);
            var r = cx128real_ssa(a);
            var i = cx128imag_ssa(a);
            var cnst = cx128cnst_ssa(a);
            var c1 = cx128eq_ssa(a, a);
            var c2 = cx128eq_ssa(a, b);
            var c3 = cx128ne_ssa(a, a);
            var c4 = cx128ne_ssa(a, b);

            fails += expectCx128("sum", sum, 4L + 8iUL);
            fails += expectCx128("diff", diff, 2L + 4iUL);
            fails += expectCx128("prod", prod, -9L + 12iUL);
            fails += expectCx128("quot", quot, 3L + 0iUL);
            fails += expectCx128("neg", neg, -1L - 2iUL);
            fails += expect64("real", r, 1L);
            fails += expect64("imag", i, 2L);
            fails += expectCx128("cnst", cnst, -4L + 7iUL);
            fails += expectTrue(fmt.Sprintf("%v==%v", a, a), c1);
            fails += expectFalse(fmt.Sprintf("%v==%v", a, b), c2);
            fails += expectFalse(fmt.Sprintf("%v!=%v", a, a), c3);
            fails += expectTrue(fmt.Sprintf("%v!=%v", a, b), c4);

            return fails;
        }

        private static long complexTest64()
        {
            long fails = 0L;
            complex64 a = 1L + 2iUL;
            complex64 b = 3L + 6iUL;
            var sum = cx64sum_ssa(b, a);
            var diff = cx64diff_ssa(b, a);
            var prod = cx64prod_ssa(b, a);
            var quot = cx64quot_ssa(b, a);
            var neg = cx64neg_ssa(a);
            var r = cx64real_ssa(a);
            var i = cx64imag_ssa(a);
            var c1 = cx64eq_ssa(a, a);
            var c2 = cx64eq_ssa(a, b);
            var c3 = cx64ne_ssa(a, a);
            var c4 = cx64ne_ssa(a, b);

            fails += expectCx64("sum", sum, 4L + 8iUL);
            fails += expectCx64("diff", diff, 2L + 4iUL);
            fails += expectCx64("prod", prod, -9L + 12iUL);
            fails += expectCx64("quot", quot, 3L + 0iUL);
            fails += expectCx64("neg", neg, -1L - 2iUL);
            fails += expect32("real", r, 1L);
            fails += expect32("imag", i, 2L);
            fails += expectTrue(fmt.Sprintf("%v==%v", a, a), c1);
            fails += expectFalse(fmt.Sprintf("%v==%v", a, b), c2);
            fails += expectFalse(fmt.Sprintf("%v!=%v", a, a), c3);
            fails += expectTrue(fmt.Sprintf("%v!=%v", a, b), c4);

            return fails;
        }

        private static void Main() => func((_, panic, __) =>
        {
            float a = 3.0F;
            float b = 4.0F;

            var c = float32(3.0F);
            var d = float32(4.0F);

            var tiny = float32(1.5E-45F); // smallest f32 denorm = 2**(-149)
            var dtiny = float64(tiny); // well within range of f64

            long fails = 0L;
            fails += fail64("+", add64_ssa, a, b, 7.0F);
            fails += fail64("*", mul64_ssa, a, b, 12.0F);
            fails += fail64("-", sub64_ssa, a, b, -1.0F);
            fails += fail64("/", div64_ssa, a, b, 0.75F);
            fails += fail64("neg", neg64_ssa, a, b, -7L);

            fails += fail32("+", add32_ssa, c, d, 7.0F);
            fails += fail32("*", mul32_ssa, c, d, 12.0F);
            fails += fail32("-", sub32_ssa, c, d, -1.0F);
            fails += fail32("/", div32_ssa, c, d, 0.75F);
            fails += fail32("neg", neg32_ssa, c, d, -7L); 

            // denorm-squared should underflow to zero.
            fails += fail32("*", mul32_ssa, tiny, tiny, 0L); 

            // but should not underflow in float and in fact is exactly representable.
            fails += fail64("*", mul64_ssa, dtiny, dtiny, 1.9636373861190906e-90F); 

            // Intended to create register pressure which forces
            // asymmetric op into different code paths.
            var (aa, ab, ac, ad, ba, bb, bc, bd, ca, cb, cc, cd, da, db, dc, dd) = manysub_ssa(1000.0F, 100.0F, 10.0F, 1.0F);

            fails += expect64("aa", aa, 11.0F);
            fails += expect64("ab", ab, 900.0F);
            fails += expect64("ac", ac, 990.0F);
            fails += expect64("ad", ad, 999.0F);

            fails += expect64("ba", ba, -900.0F);
            fails += expect64("bb", bb, 22.0F);
            fails += expect64("bc", bc, 90.0F);
            fails += expect64("bd", bd, 99.0F);

            fails += expect64("ca", ca, -990.0F);
            fails += expect64("cb", cb, -90.0F);
            fails += expect64("cc", cc, 33.0F);
            fails += expect64("cd", cd, 9.0F);

            fails += expect64("da", da, -999.0F);
            fails += expect64("db", db, -99.0F);
            fails += expect64("dc", dc, -9.0F);
            fails += expect64("dd", dd, 44.0F);

            fails += integer2floatConversions();

            fails += multiplyAdd();

            double zero64 = 0.0F;
            double one64 = 1.0F;
            double inf64 = 1.0F / zero64;
            double nan64 = sub64_ssa(inf64, inf64);

            fails += cmpOpTest("!=", ne64_ssa, nebr64_ssa, ne32_ssa, nebr32_ssa, zero64, one64, inf64, nan64, 0x01111UL);
            fails += cmpOpTest("==", eq64_ssa, eqbr64_ssa, eq32_ssa, eqbr32_ssa, zero64, one64, inf64, nan64, 0x10000UL);
            fails += cmpOpTest("<=", le64_ssa, lebr64_ssa, le32_ssa, lebr32_ssa, zero64, one64, inf64, nan64, 0x11100UL);
            fails += cmpOpTest("<", lt64_ssa, ltbr64_ssa, lt32_ssa, ltbr32_ssa, zero64, one64, inf64, nan64, 0x01100UL);
            fails += cmpOpTest(">", gt64_ssa, gtbr64_ssa, gt32_ssa, gtbr32_ssa, zero64, one64, inf64, nan64, 0x00000UL);
            fails += cmpOpTest(">=", ge64_ssa, gebr64_ssa, ge32_ssa, gebr32_ssa, zero64, one64, inf64, nan64, 0x10000UL);

            {
                var (lt, le, eq, ne, ge, gt) = compares64_ssa(0.0F, 1.0F, inf64, nan64);
                fails += expectUint64("lt", lt, 0x0110001000000000UL);
                fails += expectUint64("le", le, 0x1110011000100000UL);
                fails += expectUint64("eq", eq, 0x1000010000100000UL);
                fails += expectUint64("ne", ne, 0x0111101111011111UL);
                fails += expectUint64("ge", ge, 0x1000110011100000UL);
                fails += expectUint64("gt", gt, 0x0000100011000000UL); 
                // fmt.Printf("lt=0x%016x, le=0x%016x, eq=0x%016x, ne=0x%016x, ge=0x%016x, gt=0x%016x\n",
                //     lt, le, eq, ne, ge, gt)
            }
            {
                (lt, le, eq, ne, ge, gt) = compares32_ssa(0.0F, 1.0F, float32(inf64), float32(nan64));
                fails += expectUint64("lt", lt, 0x0110001000000000UL);
                fails += expectUint64("le", le, 0x1110011000100000UL);
                fails += expectUint64("eq", eq, 0x1000010000100000UL);
                fails += expectUint64("ne", ne, 0x0111101111011111UL);
                fails += expectUint64("ge", ge, 0x1000110011100000UL);
                fails += expectUint64("gt", gt, 0x0000100011000000UL);
            }
            fails += floatingToIntegerConversionsTest();
            fails += complexTest128();
            fails += complexTest64();

            if (fails > 0L)
            {
                fmt.Printf("Saw %v failures\n", fails);
                panic("Failed.");
            }
        });
    }
}
