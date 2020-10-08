// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !plan9,!windows

// package main -- go2cs converted at 2020 October 08 03:44:01 UTC
// Original source: C:\Go\src\runtime\testdata\testprogcgo\numgoroutine.go
/*
#include <stddef.h>
#include <pthread.h>

extern void CallbackNumGoroutine();

static void* thread2(void* arg __attribute__ ((unused))) {
    CallbackNumGoroutine();
    return NULL;
}

static void CheckNumGoroutine() {
    pthread_t tid;
    pthread_create(&tid, NULL, thread2, NULL);
    pthread_join(tid, NULL);
}
*/
using C = go.C_package;/*
#include <stddef.h>
#include <pthread.h>

extern void CallbackNumGoroutine();

static void* thread2(void* arg __attribute__ ((unused))) {
    CallbackNumGoroutine();
    return NULL;
}

static void CheckNumGoroutine() {
    pthread_t tid;
    pthread_create(&tid, NULL, thread2, NULL);
    pthread_join(tid, NULL);
}
*/


using fmt = go.fmt_package;
using runtime = go.runtime_package;
using strings = go.strings_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static long baseGoroutines = default;

        private static void init()
        {
            register("NumGoroutine", NumGoroutine);
        }

        public static void NumGoroutine()
        { 
            // Test that there are just the expected number of goroutines
            // running. Specifically, test that the spare M's goroutine
            // doesn't show up.
            {
                var (_, ok) = checkNumGoroutine("first", 1L + baseGoroutines);

                if (!ok)
                {
                    return ;
                } 

                // Test that the goroutine for a callback from C appears.

            } 

            // Test that the goroutine for a callback from C appears.
            C.CheckNumGoroutine();

            if (!callbackok)
            {
                return ;
            } 

            // Make sure we're back to the initial goroutines.
            {
                (_, ok) = checkNumGoroutine("third", 1L + baseGoroutines);

                if (!ok)
                {
                    return ;
                }

            }


            fmt.Println("OK");

        }

        private static (@string, bool) checkNumGoroutine(@string label, long want)
        {
            @string _p0 = default;
            bool _p0 = default;

            var n = runtime.NumGoroutine();
            if (n != want)
            {
                fmt.Printf("%s NumGoroutine: want %d; got %d\n", label, want, n);
                return ("", false);
            }

            var sbuf = make_slice<byte>(32L << (int)(10L));
            sbuf = sbuf[..runtime.Stack(sbuf, true)];
            n = strings.Count(string(sbuf), "goroutine ");
            if (n != want)
            {
                fmt.Printf("%s Stack: want %d; got %d:\n%s\n", label, want, n, string(sbuf));
                return ("", false);
            }

            return (string(sbuf), true);

        }

        private static bool callbackok = default;

        //export CallbackNumGoroutine
        public static void CallbackNumGoroutine()
        {
            var (stk, ok) = checkNumGoroutine("second", 2L + baseGoroutines);
            if (!ok)
            {
                return ;
            }

            if (!strings.Contains(stk, "CallbackNumGoroutine"))
            {
                fmt.Printf("missing CallbackNumGoroutine from stack:\n%s\n", stk);
                return ;
            }

            callbackok = true;

        }
    }
}
