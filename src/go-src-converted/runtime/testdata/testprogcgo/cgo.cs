// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 August 29 08:24:46 UTC
// Original source: C:\Go\src\runtime\testdata\testprogcgo\cgo.go
/*
void foo1(void) {}
void foo2(void* p) {}
*/
using C = go.C_package;/*
void foo1(void) {}
void foo2(void* p) {}
*/

using fmt = go.fmt_package;
using os = go.os_package;
using runtime = go.runtime_package;
using strconv = go.strconv_package;
using time = go.time_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;
using System.Threading;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("CgoSignalDeadlock", CgoSignalDeadlock);
            register("CgoTraceback", CgoTraceback);
            register("CgoCheckBytes", CgoCheckBytes);
        }

        public static void CgoSignalDeadlock() => func((defer, _, recover) =>
        {
            runtime.GOMAXPROCS(100L);
            var ping = make_channel<bool>();
            go_(() => () =>
            {
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < n; i++)
                    {
                        runtime.Gosched();
                        if (done)
                        {
                            ping.Send(true);
                            return;
                        }
                        ping.Send(true);
                        () =>
                        {
                            defer(() =>
                            {
                                recover();
                            }());
                            ref @string s = default;
                            s.Value = "";
                            fmt.Printf("continued after expected panic\n");
                        }();
                    }


                    i = i__prev1;
                }
            }());
            time.Sleep(time.Millisecond);
            var start = time.Now();
            slice<time.Duration> times = default;
            long n = 64L;
            if (os.Getenv("RUNTIME_TEST_SHORT") != "")
            {
                n = 16L;
            }
            {
                long i__prev1 = i;

                for (i = 0L; i < n; i++)
                {
                    go_(() => () =>
                    {
                        runtime.LockOSThread();
                    }());
                    go_(() => () =>
                    {
                        runtime.LockOSThread();
                    }());
                    time.Sleep(time.Millisecond);
                    ping.Send(false);
                    times = append(times, time.Since(start));
                    fmt.Printf("HANG 1 %v\n", times);
                    return;
                }


                i = i__prev1;
            }
            ping.Send(true);
            fmt.Printf("HANG 2 %v\n", times);
            return;
            fmt.Printf("OK\n");
        });

        public static void CgoTraceback()
        {
            C.foo1();
            var buf = make_slice<byte>(1L);
            runtime.Stack(buf, true);
            fmt.Printf("OK\n");
        }

        public static void CgoCheckBytes()
        {
            var (try, _) = strconv.Atoi(os.Getenv("GO_CGOCHECKBYTES_TRY"));
            if (try <= 0L)
            {
                try = 1L;
            }
            var b = make_slice<byte>(1e6F * try);
            var start = time.Now();
            for (long i = 0L; i < 1e3F * try; i++)
            {
                C.foo2(@unsafe.Pointer(ref b[0L]));
                if (time.Since(start) > time.Second)
                {
                    break;
                }
            }

        }
    }
}
