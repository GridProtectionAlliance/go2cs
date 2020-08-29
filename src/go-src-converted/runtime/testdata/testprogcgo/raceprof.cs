// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build linux,amd64

// package main -- go2cs converted at 2020 August 29 08:24:57 UTC
// Original source: C:\Go\src\runtime\testdata\testprogcgo\raceprof.go
// Test that we can collect a lot of colliding profiling signals from
// an external C thread. This used to fail when built with the race
// detector, because a call of the predeclared function copy was
// turned into a call to runtime.slicecopy, which is not marked nosplit.

/*
#include <signal.h>
#include <stdint.h>
#include <pthread.h>
#include <sched.h>

struct cgoTracebackArg {
    uintptr_t  context;
    uintptr_t  sigContext;
    uintptr_t* buf;
    uintptr_t  max;
};

static int raceprofCount;

// We want a bunch of different profile stacks that collide in the
// hash table maintained in runtime/cpuprof.go. This code knows the
// size of the hash table (1 << 10) and knows that the hash function
// is simply multiplicative.
void raceprofTraceback(void* parg) {
    struct cgoTracebackArg* arg = (struct cgoTracebackArg*)(parg);
    raceprofCount++;
    arg->buf[0] = raceprofCount * (1 << 10);
    arg->buf[1] = 0;
}

static void* raceprofThread(void* p) {
    int i;

    for (i = 0; i < 100; i++) {
        pthread_kill(pthread_self(), SIGPROF);
        sched_yield();
    }
    return 0;
}

void runRaceprofThread() {
    pthread_t tid;
    pthread_create(&tid, 0, raceprofThread, 0);
    pthread_join(tid, 0);
}
*/
using C = go.C_package;// Test that we can collect a lot of colliding profiling signals from
// an external C thread. This used to fail when built with the race
// detector, because a call of the predeclared function copy was
// turned into a call to runtime.slicecopy, which is not marked nosplit.

/*
#include <signal.h>
#include <stdint.h>
#include <pthread.h>
#include <sched.h>

struct cgoTracebackArg {
    uintptr_t  context;
    uintptr_t  sigContext;
    uintptr_t* buf;
    uintptr_t  max;
};

static int raceprofCount;

// We want a bunch of different profile stacks that collide in the
// hash table maintained in runtime/cpuprof.go. This code knows the
// size of the hash table (1 << 10) and knows that the hash function
// is simply multiplicative.
void raceprofTraceback(void* parg) {
    struct cgoTracebackArg* arg = (struct cgoTracebackArg*)(parg);
    raceprofCount++;
    arg->buf[0] = raceprofCount * (1 << 10);
    arg->buf[1] = 0;
}

static void* raceprofThread(void* p) {
    int i;

    for (i = 0; i < 100; i++) {
        pthread_kill(pthread_self(), SIGPROF);
        sched_yield();
    }
    return 0;
}

void runRaceprofThread() {
    pthread_t tid;
    pthread_create(&tid, 0, raceprofThread, 0);
    pthread_join(tid, 0);
}
*/


using bytes = go.bytes_package;
using fmt = go.fmt_package;
using runtime = go.runtime_package;
using pprof = go.runtime.pprof_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("CgoRaceprof", CgoRaceprof);
        }

        public static void CgoRaceprof()
        {
            runtime.SetCgoTraceback(0L, @unsafe.Pointer(C.raceprofTraceback), null, null);

            bytes.Buffer buf = default;
            pprof.StartCPUProfile(ref buf);

            C.runRaceprofThread();
            fmt.Println("OK");
        }
    }
}
