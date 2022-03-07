// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !plan9,!windows

// package main -- go2cs converted at 2022 March 06 22:26:18 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprogcgo\threadpprof.go
// Run a slow C function saving a CPU profile.

/*
#include <stdint.h>
#include <time.h>
#include <pthread.h>

int threadSalt1;
int threadSalt2;

void cpuHogThread() {
    int foo = threadSalt1;
    int i;

    for (i = 0; i < 100000; i++) {
        if (foo > 0) {
            foo *= foo;
        } else {
            foo *= foo + 1;
        }
    }
    threadSalt2 = foo;
}

void cpuHogThread2() {
}

static int cpuHogThreadCount;

struct cgoTracebackArg {
    uintptr_t  context;
    uintptr_t  sigContext;
    uintptr_t* buf;
    uintptr_t  max;
};

// pprofCgoThreadTraceback is passed to runtime.SetCgoTraceback.
// For testing purposes it pretends that all CPU hits in C code are in cpuHog.
void pprofCgoThreadTraceback(void* parg) {
    struct cgoTracebackArg* arg = (struct cgoTracebackArg*)(parg);
    arg->buf[0] = (uintptr_t)(cpuHogThread) + 0x10;
    arg->buf[1] = (uintptr_t)(cpuHogThread2) + 0x4;
    arg->buf[2] = 0;
    __sync_add_and_fetch(&cpuHogThreadCount, 1);
}

// getCPUHogThreadCount fetches the number of times we've seen cpuHogThread
// in the traceback.
int getCPUHogThreadCount() {
    return __sync_add_and_fetch(&cpuHogThreadCount, 0);
}

static void* cpuHogDriver(void* arg __attribute__ ((unused))) {
    while (1) {
        cpuHogThread();
    }
    return 0;
}

void runCPUHogThread(void) {
    pthread_t tid;
    pthread_create(&tid, 0, cpuHogDriver, 0);
}
*/
using C = go.C_package;// Run a slow C function saving a CPU profile.

/*
#include <stdint.h>
#include <time.h>
#include <pthread.h>

int threadSalt1;
int threadSalt2;

void cpuHogThread() {
    int foo = threadSalt1;
    int i;

    for (i = 0; i < 100000; i++) {
        if (foo > 0) {
            foo *= foo;
        } else {
            foo *= foo + 1;
        }
    }
    threadSalt2 = foo;
}

void cpuHogThread2() {
}

static int cpuHogThreadCount;

struct cgoTracebackArg {
    uintptr_t  context;
    uintptr_t  sigContext;
    uintptr_t* buf;
    uintptr_t  max;
};

// pprofCgoThreadTraceback is passed to runtime.SetCgoTraceback.
// For testing purposes it pretends that all CPU hits in C code are in cpuHog.
void pprofCgoThreadTraceback(void* parg) {
    struct cgoTracebackArg* arg = (struct cgoTracebackArg*)(parg);
    arg->buf[0] = (uintptr_t)(cpuHogThread) + 0x10;
    arg->buf[1] = (uintptr_t)(cpuHogThread2) + 0x4;
    arg->buf[2] = 0;
    __sync_add_and_fetch(&cpuHogThreadCount, 1);
}

// getCPUHogThreadCount fetches the number of times we've seen cpuHogThread
// in the traceback.
int getCPUHogThreadCount() {
    return __sync_add_and_fetch(&cpuHogThreadCount, 0);
}

static void* cpuHogDriver(void* arg __attribute__ ((unused))) {
    while (1) {
        cpuHogThread();
    }
    return 0;
}

void runCPUHogThread(void) {
    pthread_t tid;
    pthread_create(&tid, 0, cpuHogDriver, 0);
}
*/


using fmt = go.fmt_package;
using os = go.os_package;
using runtime = go.runtime_package;
using pprof = go.runtime.pprof_package;
using time = go.time_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class main_package {

private static void init() {
    register("CgoPprofThread", CgoPprofThread);
    register("CgoPprofThreadNoTraceback", CgoPprofThreadNoTraceback);
}

public static void CgoPprofThread() {
    runtime.SetCgoTraceback(0, @unsafe.Pointer(C.pprofCgoThreadTraceback), null, null);
    pprofThread();
}

public static void CgoPprofThreadNoTraceback() {
    pprofThread();
}

private static void pprofThread() {
    var (f, err) = os.CreateTemp("", "prof");
    if (err != null) {
        fmt.Fprintln(os.Stderr, err);
        os.Exit(2);
    }
    {
        var err__prev1 = err;

        var err = pprof.StartCPUProfile(f);

        if (err != null) {
            fmt.Fprintln(os.Stderr, err);
            os.Exit(2);
        }
        err = err__prev1;

    }


    C.runCPUHogThread();

    var t0 = time.Now();
    while (C.getCPUHogThreadCount() < 2 && time.Since(t0) < time.Second) {
        time.Sleep(100 * time.Millisecond);
    }

    pprof.StopCPUProfile();

    var name = f.Name();
    {
        var err__prev1 = err;

        err = f.Close();

        if (err != null) {
            fmt.Fprintln(os.Stderr, err);
            os.Exit(2);
        }
        err = err__prev1;

    }


    fmt.Println(name);

}

} // end main_package
