// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !plan9,!windows

// package main -- go2cs converted at 2020 August 29 08:24:44 UTC
// Original source: C:\Go\src\runtime\testdata\testprogcgo\callback.go
/*
#include <pthread.h>

void go_callback();

static void *thr(void *arg) {
    go_callback();
    return 0;
}

static void foo() {
    pthread_t th;
    pthread_attr_t attr;
    pthread_attr_init(&attr);
    pthread_attr_setstacksize(&attr, 256 << 10);
    pthread_create(&th, &attr, thr, 0);
    pthread_join(th, 0);
}
*/
using C = go.C_package;/*
#include <pthread.h>

void go_callback();

static void *thr(void *arg) {
    go_callback();
    return 0;
}

static void foo() {
    pthread_t th;
    pthread_attr_t attr;
    pthread_attr_init(&attr);
    pthread_attr_setstacksize(&attr, 256 << 10);
    pthread_create(&th, &attr, thr, 0);
    pthread_join(th, 0);
}
*/


using fmt = go.fmt_package;
using os = go.os_package;
using runtime = go.runtime_package;
using static go.builtin;
using System;
using System.Threading;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("CgoCallbackGC", CgoCallbackGC);
        }

        //export go_callback
        private static void go_callback()
        {
            runtime.GC();
            grow();
            runtime.GC();
        }

        private static long cnt = default;

        private static void grow() => func((_, panic, __) =>
        {
            long x = 10000L;
            long sum = 0L;
            if (grow1(ref x, ref sum) == 0L)
            {
                panic("bad");
            }
        });

        private static long grow1(ref long x, ref long sum)
        {
            if (x == 0L.Value)
            {
                return sum + 1L.Value;
            }
            x.Value--;
            var sum1 = sum + x.Value.Value;
            return grow1(x, ref sum1);
        }

        public static void CgoCallbackGC()
        {
            long P = 100L;
            if (os.Getenv("RUNTIME_TESTING_SHORT") != "")
            {
                P = 10L;
            }
            var done = make_channel<bool>(); 
            // allocate a bunch of stack frames and spray them with pointers
            {
                long i__prev1 = i;

                for (long i = 0L; i < P; i++)
                {
                    go_(() => () =>
                    {
                        grow();
                        done.Send(true);
                    }());
                }


                i = i__prev1;
            }
            {
                long i__prev1 = i;

                for (i = 0L; i < P; i++)
                {
                    done.Receive();
                } 
                // now give these stack frames to cgo callbacks


                i = i__prev1;
            } 
            // now give these stack frames to cgo callbacks
            {
                long i__prev1 = i;

                for (i = 0L; i < P; i++)
                {
                    go_(() => () =>
                    {
                        C.foo();
                        done.Send(true);
                    }());
                }


                i = i__prev1;
            }
            {
                long i__prev1 = i;

                for (i = 0L; i < P; i++)
                {
                    done.Receive();
                }


                i = i__prev1;
            }
            fmt.Printf("OK\n");
        }
    }
}
