// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This program outputs a CPU profile that includes
// both Go and Cgo stacks. This is used by the mapping info
// tests in runtime/pprof.
//
// If SETCGOTRACEBACK=1 is set, the CPU profile will includes
// PCs from C side but they will not be symbolized.
// package main -- go2cs converted at 2022 March 06 22:15:10 UTC
// Original source: C:\Program Files\Go\src\runtime\pprof\testdata\mappingtest\main.go
/*
#include <stdint.h>
#include <stdlib.h>

int cpuHogCSalt1 = 0;
int cpuHogCSalt2 = 0;

void CPUHogCFunction0(int foo) {
    int i;
    for (i = 0; i < 100000; i++) {
        if (foo > 0) {
            foo *= foo;
        } else {
            foo *= foo + 1;
        }
        cpuHogCSalt2 = foo;
    }
}

void CPUHogCFunction() {
    CPUHogCFunction0(cpuHogCSalt1);
}

struct CgoTracebackArg {
    uintptr_t context;
        uintptr_t sigContext;
    uintptr_t *buf;
        uintptr_t max;
};

void CollectCgoTraceback(void* parg) {
        struct CgoTracebackArg* arg = (struct CgoTracebackArg*)(parg);
    arg->buf[0] = (uintptr_t)(CPUHogCFunction0);
    arg->buf[1] = (uintptr_t)(CPUHogCFunction);
    arg->buf[2] = 0;
};
*/
using C = go.C_package;/*
#include <stdint.h>
#include <stdlib.h>

int cpuHogCSalt1 = 0;
int cpuHogCSalt2 = 0;

void CPUHogCFunction0(int foo) {
    int i;
    for (i = 0; i < 100000; i++) {
        if (foo > 0) {
            foo *= foo;
        } else {
            foo *= foo + 1;
        }
        cpuHogCSalt2 = foo;
    }
}

void CPUHogCFunction() {
    CPUHogCFunction0(cpuHogCSalt1);
}

struct CgoTracebackArg {
    uintptr_t context;
        uintptr_t sigContext;
    uintptr_t *buf;
        uintptr_t max;
};

void CollectCgoTraceback(void* parg) {
        struct CgoTracebackArg* arg = (struct CgoTracebackArg*)(parg);
    arg->buf[0] = (uintptr_t)(CPUHogCFunction0);
    arg->buf[1] = (uintptr_t)(CPUHogCFunction);
    arg->buf[2] = 0;
};
*/


using log = go.log_package;
using os = go.os_package;
using runtime = go.runtime_package;
using pprof = go.runtime.pprof_package;
using time = go.time_package;
using @unsafe = go.@unsafe_package;
using System.Threading;


namespace go;

public static partial class main_package {

private static void init() {
    {
        var v = os.Getenv("SETCGOTRACEBACK");

        if (v == "1") { 
            // Collect some PCs from C-side, but don't symbolize.
            runtime.SetCgoTraceback(0, @unsafe.Pointer(C.CollectCgoTraceback), null, null);

        }
    }

}

private static void Main() {
    go_(() => cpuHogGoFunction());
    go_(() => cpuHogCFunction());
    runtime.Gosched();

    {
        var err__prev1 = err;

        var err = pprof.StartCPUProfile(os.Stdout);

        if (err != null) {
            log.Fatal("can't start CPU profile: ", err);
        }
        err = err__prev1;

    }

    time.Sleep(200 * time.Millisecond);
    pprof.StopCPUProfile();

    {
        var err__prev1 = err;

        err = os.Stdout.Close();

        if (err != null) {
            log.Fatal("can't write CPU profile: ", err);
        }
        err = err__prev1;

    }

}

private static nint salt1 = default;
private static nint salt2 = default;

private static void cpuHogGoFunction() {
    while (true) {
        var foo = salt1;
        for (nint i = 0; i < 1e5F; i++) {
            if (foo > 0) {
                foo *= foo;
            }
            else
 {
                foo *= foo + 1;
            }

            salt2 = foo;

        }
        runtime.Gosched();

    }

}

private static void cpuHogCFunction() { 
    // Generates CPU profile samples including a Cgo call path.
    while (true) {
        C.CPUHogCFunction();
        runtime.Gosched();
    }

}

} // end main_package
