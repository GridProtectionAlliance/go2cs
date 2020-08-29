// run

// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This test makes sure that naming named
// return variables in a return statement works.
// See issue #14904.

// package main -- go2cs converted at 2020 August 29 09:58:23 UTC
// Original source: C:\Go\src\cmd\compile\internal\gc\testdata\namedReturn.go
using fmt = go.fmt_package;
using runtime = go.runtime_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        // Our heap-allocated object that will be GC'd incorrectly.
        // Note that we always check the second word because that's
        // where 0xdeaddeaddeaddead is written.
        public partial struct B // : array<long>
        {
        }

        // small (SSAable) array
        public partial struct T1 // : array<ref B>
        {
        }

        //go:noinline
        private static T1 f1()
        {
            t[0L] = ref new B(91,92,93,94);
            runtime.GC();
            return t;
        }

        // large (non-SSAable) array
        public partial struct T2 // : array<ref B>
        {
        }

        //go:noinline
        private static T2 f2()
        {
            t[0L] = ref new B(91,92,93,94);
            runtime.GC();
            return t;
        }

        // small (SSAable) struct
        public partial struct T3
        {
            public ptr<B> a;
            public ptr<B> b;
            public ptr<B> c;
        }

        //go:noinline
        private static T3 f3()
        {
            t.a = ref new B(91,92,93,94);
            runtime.GC();
            return t;
        }

        // large (non-SSAable) struct
        public partial struct T4
        {
            public ptr<B> a;
            public ptr<B> b;
            public ptr<B> c;
            public ptr<B> d;
            public ptr<B> e;
            public ptr<B> f;
        }

        //go:noinline
        private static T4 f4()
        {
            t.a = ref new B(91,92,93,94);
            runtime.GC();
            return t;
        }

        private static ref B sink = default;

        private static long f5()
        {
            B b = ref new B(91,92,93,94);
            T4 t = new T4(b,nil,nil,nil,nil,nil);
            sink = b; // make sure b is heap allocated ...
            sink = null; // ... but not live
            runtime.GC();
            t = t;
            return t.a[1L];
        }

        private static void Main() => func((_, panic, __) =>
        {
            var failed = false;

            {
                var v__prev1 = v;

                var v = f1()[0L][1L];

                if (v != 92L)
                {
                    fmt.Printf("f1()[0][1]=%d, want 92\n", v);
                    failed = true;
                }

                v = v__prev1;

            }
            {
                var v__prev1 = v;

                v = f2()[0L][1L];

                if (v != 92L)
                {
                    fmt.Printf("f2()[0][1]=%d, want 92\n", v);
                    failed = true;
                }

                v = v__prev1;

            }
            {
                var v__prev1 = v;

                v = f3().a[1L];

                if (v != 92L)
                {
                    fmt.Printf("f3().a[1]=%d, want 92\n", v);
                    failed = true;
                }

                v = v__prev1;

            }
            {
                var v__prev1 = v;

                v = f4().a[1L];

                if (v != 92L)
                {
                    fmt.Printf("f4().a[1]=%d, want 92\n", v);
                    failed = true;
                }

                v = v__prev1;

            }
            {
                var v__prev1 = v;

                v = f5();

                if (v != 92L)
                {
                    fmt.Printf("f5()=%d, want 92\n", v);
                    failed = true;
                }

                v = v__prev1;

            }
            if (failed)
            {
                panic("bad");
            }
        });
    }
}
