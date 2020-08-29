// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains tests for the rangeloop checker.

// package testdata -- go2cs converted at 2020 August 29 10:10:37 UTC
// import "cmd/vet/testdata" ==> using testdata = go.cmd.vet.testdata_package
// Original source: C:\Go\src\cmd\vet\testdata\rangeloop.go

using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace cmd {
namespace vet
{
    public static partial class testdata_package
    {
        public static void RangeLoopTests() => func((defer, _, __) =>
        {
            slice<long> s = default;
            {
                var i__prev1 = i;
                var v__prev1 = v;

                foreach (var (__i, __v) in s)
                {
                    i = __i;
                    v = __v;
                    go_(() => () =>
                    {
                        println(i); // ERROR "loop variable i captured by func literal"
                        println(v); // ERROR "loop variable v captured by func literal"
                    }());
                }
                i = i__prev1;
                v = v__prev1;
            }

            {
                var i__prev1 = i;
                var v__prev1 = v;

                foreach (var (__i, __v) in s)
                {
                    i = __i;
                    v = __v;
                    defer(() =>
                    {
                        println(i); // ERROR "loop variable i captured by func literal"
                        println(v); // ERROR "loop variable v captured by func literal"
                    }());
                }
                i = i__prev1;
                v = v__prev1;
            }

            {
                var i__prev1 = i;

                foreach (var (__i) in s)
                {
                    i = __i;
                    go_(() => () =>
                    {
                        println(i); // ERROR "loop variable i captured by func literal"
                    }());
                }
                i = i__prev1;
            }

            {
                var v__prev1 = v;

                foreach (var (_, __v) in s)
                {
                    v = __v;
                    go_(() => () =>
                    {
                        println(v); // ERROR "loop variable v captured by func literal"
                    }());
                }
                v = v__prev1;
            }

            {
                var i__prev1 = i;
                var v__prev1 = v;

                foreach (var (__i, __v) in s)
                {
                    i = __i;
                    v = __v;
                    go_(() => () =>
                    {
                        println(i, v);
                    }());
                    println("unfortunately, we don't catch the error above because of this statement");
                }
                i = i__prev1;
                v = v__prev1;
            }

            {
                var i__prev1 = i;
                var v__prev1 = v;

                foreach (var (__i, __v) in s)
                {
                    i = __i;
                    v = __v;
                    go_(() => (i, v) =>
                    {
                        println(i, v);
                    }(i, v));
                }
                i = i__prev1;
                v = v__prev1;
            }

            {
                var i__prev1 = i;
                var v__prev1 = v;

                foreach (var (__i, __v) in s)
                {
                    i = __i;
                    v = __v;
                    var i = i;
                    var v = v;
                    go_(() => () =>
                    {
                        println(i, v);
                    }());
                }
                i = i__prev1;
                v = v__prev1;
            }

            array<long> x = new array<long>(2L);
            long f = default;
            foreach (var (__x[0L], __f) in s)
            {
                x[0L] = __x[0L];
                f = __f;
                go_(() => () =>
                {
                    _ = f; // ERROR "loop variable f captured by func literal"
                }());
            }
            public partial struct T
            {
                public long v;
            }
            {
                var v__prev1 = v;

                foreach (var (_, __v) in s)
                {
                    v = __v;
                    go_(() => () =>
                    {
                        _ = new T(v:1);
                        _ = new slice<long>(InitKeyedValues<long>((v, 1))); // ERROR "loop variable v captured by func literal"
                    }());
                }
                v = v__prev1;
            }

            {
                var i__prev1 = i;

                for (i = 0L; i < 10L; i++)
                {
                    go_(() => () =>
                    {
                        print(i); // ERROR "loop variable i captured by func literal"
                    }());
                }

                i = i__prev1;
            }
            {
                var i__prev1 = i;

                i = 0L;
                long j = 1L;

                while (i < 100L)
                {
                    go_(() => () =>
                    {
                        print(j); // ERROR "loop variable j captured by func literal"
                    i = j;
                j = i + j;
                    }());
                }

                i = i__prev1;
            }
            private partial struct cons
            {
                public long car;
                public ptr<cons> cdr;
            }
            ref cons head = default;
            {
                var p = head;

                while (p != null)
                {
                    go_(() => () =>
                    {
                        print(p.car); // ERROR "loop variable p captured by func literal"
                    p = p.next;
                    }());
                }
            }
        });
    }
}}}
