// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// We only build this file with the tag "threadprof", since it starts
// a thread running a busy loop at constructor time.

// +build !plan9,!windows
// +build threadprof

// package main -- go2cs converted at 2020 October 09 05:01:05 UTC
// Original source: C:\Go\src\runtime\testdata\testprogcgo\threadprof.go
/*
#include <stdint.h>
#include <signal.h>
#include <pthread.h>

volatile int32_t spinlock;

static void *thread1(void *p) {
    (void)p;
    while (spinlock == 0)
        ;
    pthread_kill(pthread_self(), SIGPROF);
    spinlock = 0;
    return NULL;
}

__attribute__((constructor)) void issue9456() {
    pthread_t tid;
    pthread_create(&tid, 0, thread1, NULL);
}

void **nullptr;

void *crash(void *p) {
    *nullptr = p;
    return 0;
}

int start_crashing_thread(void) {
    pthread_t tid;
    return pthread_create(&tid, 0, crash, 0);
}
*/
using C = go.C_package;/*
#include <stdint.h>
#include <signal.h>
#include <pthread.h>

volatile int32_t spinlock;

static void *thread1(void *p) {
    (void)p;
    while (spinlock == 0)
        ;
    pthread_kill(pthread_self(), SIGPROF);
    spinlock = 0;
    return NULL;
}

__attribute__((constructor)) void issue9456() {
    pthread_t tid;
    pthread_create(&tid, 0, thread1, NULL);
}

void **nullptr;

void *crash(void *p) {
    *nullptr = p;
    return 0;
}

int start_crashing_thread(void) {
    pthread_t tid;
    return pthread_create(&tid, 0, crash, 0);
}
*/


using fmt = go.fmt_package;
using os = go.os_package;
using exec = go.os.exec_package;
using runtime = go.runtime_package;
using atomic = go.sync.atomic_package;
using time = go.time_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("CgoExternalThreadSIGPROF", CgoExternalThreadSIGPROF);
            register("CgoExternalThreadSignal", CgoExternalThreadSignal);
        }

        public static void CgoExternalThreadSIGPROF()
        { 
            // This test intends to test that sending SIGPROF to foreign threads
            // before we make any cgo call will not abort the whole process, so
            // we cannot make any cgo call here. See https://golang.org/issue/9456.
            atomic.StoreInt32((int32.val)(@unsafe.Pointer(_addr_C.spinlock)), 1L);
            while (atomic.LoadInt32((int32.val)(@unsafe.Pointer(_addr_C.spinlock))) == 1L)
            {
                runtime.Gosched();
            }

            println("OK");

        }

        public static void CgoExternalThreadSignal()
        {
            if (len(os.Args) > 2L && os.Args[2L] == "crash")
            {
                var i = C.start_crashing_thread();
                if (i != 0L)
                {
                    fmt.Println("pthread_create failed:", i); 
                    // Exit with 0 because parent expects us to crash.
                    return ;

                } 

                // We should crash immediately, but give it plenty of
                // time before failing (by exiting 0) in case we are
                // running on a slow system.
                time.Sleep(5L * time.Second);
                return ;

            }

            var (out, err) = exec.Command(os.Args[0L], "CgoExternalThreadSignal", "crash").CombinedOutput();
            if (err == null)
            {
                fmt.Println("C signal did not crash as expected");
                fmt.Printf("\n%s\n", out);
                os.Exit(1L);
            }

            fmt.Println("OK");

        }
    }
}
