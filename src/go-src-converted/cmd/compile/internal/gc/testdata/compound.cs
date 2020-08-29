// run

// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Test compound objects

// package main -- go2cs converted at 2020 August 29 09:57:47 UTC
// Original source: C:\Go\src\cmd\compile\internal\gc\testdata\compound.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static @string string_ssa(@string a, @string b, bool x)
        {
            @string s = "";
            if (x)
            {
                s = a;
            }
            else
            {
                s = b;
            }
            return s;
        }

        private static void testString()
        {
            @string a = "foo";
            @string b = "barz";
            {
                var want__prev1 = want;
                var got__prev1 = got;

                var want = a;
                var got = string_ssa(a, b, true);

                if (got != want)
                {
                    fmt.Printf("string_ssa(%v, %v, true) = %v, want %v\n", a, b, got, want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = b;
                got = string_ssa(a, b, false);

                if (got != want)
                {
                    fmt.Printf("string_ssa(%v, %v, false) = %v, want %v\n", a, b, got, want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
        }

        //go:noinline
        private static complex64 complex64_ssa(complex64 a, complex64 b, bool x)
        {
            complex64 c = default;
            if (x)
            {
                c = a;
            }
            else
            {
                c = b;
            }
            return c;
        }

        //go:noinline
        private static System.Numerics.Complex128 complex128_ssa(System.Numerics.Complex128 a, System.Numerics.Complex128 b, bool x)
        {
            System.Numerics.Complex128 c = default;
            if (x)
            {
                c = a;
            }
            else
            {
                c = b;
            }
            return c;
        }

        private static void testComplex64()
        {
            complex64 a = 1L + 2iUL;
            complex64 b = 3L + 4iUL;

            {
                var want__prev1 = want;
                var got__prev1 = got;

                var want = a;
                var got = complex64_ssa(a, b, true);

                if (got != want)
                {
                    fmt.Printf("complex64_ssa(%v, %v, true) = %v, want %v\n", a, b, got, want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = b;
                got = complex64_ssa(a, b, false);

                if (got != want)
                {
                    fmt.Printf("complex64_ssa(%v, %v, true) = %v, want %v\n", a, b, got, want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
        }

        private static void testComplex128()
        {
            System.Numerics.Complex128 a = 1L + 2iUL;
            System.Numerics.Complex128 b = 3L + 4iUL;

            {
                var want__prev1 = want;
                var got__prev1 = got;

                var want = a;
                var got = complex128_ssa(a, b, true);

                if (got != want)
                {
                    fmt.Printf("complex128_ssa(%v, %v, true) = %v, want %v\n", a, b, got, want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = b;
                got = complex128_ssa(a, b, false);

                if (got != want)
                {
                    fmt.Printf("complex128_ssa(%v, %v, true) = %v, want %v\n", a, b, got, want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
        }

        private static slice<byte> slice_ssa(slice<byte> a, slice<byte> b, bool x)
        {
            slice<byte> s = default;
            if (x)
            {
                s = a;
            }
            else
            {
                s = b;
            }
            return s;
        }

        private static void testSlice()
        {
            byte a = new slice<byte>(new byte[] { 3, 4, 5 });
            byte b = new slice<byte>(new byte[] { 7, 8, 9 });
            {
                var want__prev1 = want;
                var got__prev1 = got;

                var want = byte(3L);
                var got = slice_ssa(a, b, true)[0L];

                if (got != want)
                {
                    fmt.Printf("slice_ssa(%v, %v, true) = %v, want %v\n", a, b, got, want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = byte(7L);
                got = slice_ssa(a, b, false)[0L];

                if (got != want)
                {
                    fmt.Printf("slice_ssa(%v, %v, false) = %v, want %v\n", a, b, got, want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
        }

        private static void interface_ssa(object a, object b, bool x)
        {
            var s = default;
            if (x)
            {
                s = a;
            }
            else
            {
                s = b;
            }
            return s;
        }

        private static void testInterface()
        {
            {
                long want__prev1 = want;
                long got__prev1 = got;

                long want = 3L;
                long got = interface_ssa(a, b, true)._<long>();

                if (got != want)
                {
                    fmt.Printf("interface_ssa(%v, %v, true) = %v, want %v\n", a, b, got, want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                long want__prev1 = want;
                long got__prev1 = got;

                want = 4L;
                got = interface_ssa(a, b, false)._<long>();

                if (got != want)
                {
                    fmt.Printf("interface_ssa(%v, %v, false) = %v, want %v\n", a, b, got, want);
                    failed = true;
                }

                want = want__prev1;
                got = got__prev1;

            }
        }

        private static var failed = false;

        private static void Main() => func((_, panic, __) =>
        {
            testString();
            testSlice();
            testInterface();
            testComplex64();
            testComplex128();
            if (failed)
            {
                panic("failed");
            }
        });
    }
}
