// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !plan9,!windows

// Test that a sequence of callbacks from C to Go get the same m.
// This failed to be true on arm and arm64, which was the root cause
// of issue 13881.

// package main -- go2cs converted at 2020 October 08 03:43:51 UTC
// Original source: C:\Go\src\runtime\testdata\testprogcgo\dropm.go
/*
#include <stddef.h>
#include <pthread.h>

extern void GoCheckM();

static void* thread(void* arg __attribute__ ((unused))) {
    GoCheckM();
    return NULL;
}

static void CheckM() {
    pthread_t tid;
    pthread_create(&tid, NULL, thread, NULL);
    pthread_join(tid, NULL);
    pthread_create(&tid, NULL, thread, NULL);
    pthread_join(tid, NULL);
}
*/
using C = go.C_package;/*
#include <stddef.h>
#include <pthread.h>

extern void GoCheckM();

static void* thread(void* arg __attribute__ ((unused))) {
    GoCheckM();
    return NULL;
}

static void CheckM() {
    pthread_t tid;
    pthread_create(&tid, NULL, thread, NULL);
    pthread_join(tid, NULL);
    pthread_create(&tid, NULL, thread, NULL);
    pthread_join(tid, NULL);
}
*/


using fmt = go.fmt_package;
using os = go.os_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("EnsureDropM", EnsureDropM);
        }

        private static System.UIntPtr savedM = default;

        //export GoCheckM
        public static void GoCheckM()
        {
            var m = runtime_getm_for_test();
            if (savedM == 0L)
            {
                savedM = m;
            }
            else if (savedM != m)
            {
                fmt.Printf("m == %x want %x\n", m, savedM);
                os.Exit(1L);
            }

        }

        public static void EnsureDropM()
        {
            C.CheckM();
            fmt.Println("OK");
        }
    }
}
