// run

// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Tests type assertion expressions and statements

// package main -- go2cs converted at 2020 August 29 09:57:37 UTC
// Original source: C:\Go\src\cmd\compile\internal\gc\testdata\assert.go
using fmt = go.fmt_package;
using runtime = go.runtime_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        public partial struct S
        {
        }
        public partial struct T
        {
        }

        public partial interface I
        {
            void F();
        }        private static ref S s = default;        private static ref T t = default;

        private static void F(this ref S s)
        {
        }
        private static void F(this ref T t)
        {
        }

        private static ref T e2t_ssa(object e)
        {
            return e._<ref T>();
        }

        private static ref T i2t_ssa(I i)
        {
            return i._<ref T>();
        }

        private static void testAssertE2TOk()
        {
            {
                var got = e2t_ssa(t);

                if (got != t)
                {
                    fmt.Printf("e2t_ssa(t)=%v want %v", got, t);
                    failed = true;
                }

            }
        }

        private static void testAssertE2TPanic() => func((defer, _, __) =>
        {
            ref T got = default;
            defer(() =>
            {
                if (got != null)
                {
                    fmt.Printf("e2t_ssa(s)=%v want nil", got);
                    failed = true;
                }
                var e = recover();
                ref runtime.TypeAssertionError (err, ok) = e._<ref runtime.TypeAssertionError>();
                if (!ok)
                {
                    fmt.Printf("e2t_ssa(s) panic type %T", e);
                    failed = true;
                }
                @string want = "interface conversion: interface {} is *main.S, not *main.T";
                if (err.Error() != want)
                {
                    fmt.Printf("e2t_ssa(s) wrong error, want '%s', got '%s'\n", want, err.Error());
                    failed = true;
                }
            }());
            got = e2t_ssa(s);
            fmt.Printf("e2t_ssa(s) should panic");
            failed = true;
        });

        private static void testAssertI2TOk()
        {
            {
                var got = i2t_ssa(t);

                if (got != t)
                {
                    fmt.Printf("i2t_ssa(t)=%v want %v", got, t);
                    failed = true;
                }

            }
        }

        private static void testAssertI2TPanic() => func((defer, _, __) =>
        {
            ref T got = default;
            defer(() =>
            {
                if (got != null)
                {
                    fmt.Printf("i2t_ssa(s)=%v want nil", got);
                    failed = true;
                }
                var e = recover();
                ref runtime.TypeAssertionError (err, ok) = e._<ref runtime.TypeAssertionError>();
                if (!ok)
                {
                    fmt.Printf("i2t_ssa(s) panic type %T", e);
                    failed = true;
                }
                @string want = "interface conversion: main.I is *main.S, not *main.T";
                if (err.Error() != want)
                {
                    fmt.Printf("i2t_ssa(s) wrong error, want '%s', got '%s'\n", want, err.Error());
                    failed = true;
                }
            }());
            got = i2t_ssa(s);
            fmt.Printf("i2t_ssa(s) should panic");
            failed = true;
        });

        private static (ref T, bool) e2t2_ssa(object e)
        {
            ref T (t, ok) = e._<ref T>();
            return (t, ok);
        }

        private static (ref T, bool) i2t2_ssa(I i)
        {
            ref T (t, ok) = i._<ref T>();
            return (t, ok);
        }

        private static void testAssertE2T2()
        {
            {
                var got__prev1 = got;

                var (got, ok) = e2t2_ssa(t);

                if (!ok || got != t)
                {
                    fmt.Printf("e2t2_ssa(t)=(%v, %v) want (%v, %v)", got, ok, t, true);
                    failed = true;
                }

                got = got__prev1;

            }
            {
                var got__prev1 = got;

                (got, ok) = e2t2_ssa(s);

                if (ok || got != null)
                {
                    fmt.Printf("e2t2_ssa(s)=(%v, %v) want (%v, %v)", got, ok, null, false);
                    failed = true;
                }

                got = got__prev1;

            }
        }

        private static void testAssertI2T2()
        {
            {
                var got__prev1 = got;

                var (got, ok) = i2t2_ssa(t);

                if (!ok || got != t)
                {
                    fmt.Printf("i2t2_ssa(t)=(%v, %v) want (%v, %v)", got, ok, t, true);
                    failed = true;
                }

                got = got__prev1;

            }
            {
                var got__prev1 = got;

                (got, ok) = i2t2_ssa(s);

                if (ok || got != null)
                {
                    fmt.Printf("i2t2_ssa(s)=(%v, %v) want (%v, %v)", got, ok, null, false);
                    failed = true;
                }

                got = got__prev1;

            }
        }

        private static var failed = false;

        private static void Main() => func((_, panic, __) =>
        {
            testAssertE2TOk();
            testAssertE2TPanic();
            testAssertI2TOk();
            testAssertI2TPanic();
            testAssertE2T2();
            testAssertI2T2();
            if (failed)
            {
                panic("failed");
            }
        });
    }
}
