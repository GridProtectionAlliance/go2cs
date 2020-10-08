// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 08 03:44:01 UTC
// Original source: C:\Go\src\runtime\testdata\testprogcgo\pprof.go
// Run a slow C function saving a CPU profile.

/*
#include <stdint.h>

int salt1;
int salt2;

void cpuHog() {
    int foo = salt1;
    int i;

    for (i = 0; i < 100000; i++) {
        if (foo > 0) {
            foo *= foo;
        } else {
            foo *= foo + 1;
        }
    }
    salt2 = foo;
}

void cpuHog2() {
}

static int cpuHogCount;

struct cgoTracebackArg {
    uintptr_t  context;
    uintptr_t  sigContext;
    uintptr_t* buf;
    uintptr_t  max;
};

// pprofCgoTraceback is passed to runtime.SetCgoTraceback.
// For testing purposes it pretends that all CPU hits in C code are in cpuHog.
// Issue #29034: At least 2 frames are required to verify all frames are captured
// since runtime/pprof ignores the runtime.goexit base frame if it exists.
void pprofCgoTraceback(void* parg) {
    struct cgoTracebackArg* arg = (struct cgoTracebackArg*)(parg);
    arg->buf[0] = (uintptr_t)(cpuHog) + 0x10;
    arg->buf[1] = (uintptr_t)(cpuHog2) + 0x4;
    arg->buf[2] = 0;
    ++cpuHogCount;
}

// getCpuHogCount fetches the number of times we've seen cpuHog in the
// traceback.
int getCpuHogCount() {
    return cpuHogCount;
}
*/
using C = go.C_package;// Run a slow C function saving a CPU profile.

/*
#include <stdint.h>

int salt1;
int salt2;

void cpuHog() {
    int foo = salt1;
    int i;

    for (i = 0; i < 100000; i++) {
        if (foo > 0) {
            foo *= foo;
        } else {
            foo *= foo + 1;
        }
    }
    salt2 = foo;
}

void cpuHog2() {
}

static int cpuHogCount;

struct cgoTracebackArg {
    uintptr_t  context;
    uintptr_t  sigContext;
    uintptr_t* buf;
    uintptr_t  max;
};

// pprofCgoTraceback is passed to runtime.SetCgoTraceback.
// For testing purposes it pretends that all CPU hits in C code are in cpuHog.
// Issue #29034: At least 2 frames are required to verify all frames are captured
// since runtime/pprof ignores the runtime.goexit base frame if it exists.
void pprofCgoTraceback(void* parg) {
    struct cgoTracebackArg* arg = (struct cgoTracebackArg*)(parg);
    arg->buf[0] = (uintptr_t)(cpuHog) + 0x10;
    arg->buf[1] = (uintptr_t)(cpuHog2) + 0x4;
    arg->buf[2] = 0;
    ++cpuHogCount;
}

// getCpuHogCount fetches the number of times we've seen cpuHog in the
// traceback.
int getCpuHogCount() {
    return cpuHogCount;
}
*/


using fmt = go.fmt_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using runtime = go.runtime_package;
using pprof = go.runtime.pprof_package;
using time = go.time_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("CgoPprof", CgoPprof);
        }

        public static void CgoPprof()
        {
            runtime.SetCgoTraceback(0L, @unsafe.Pointer(C.pprofCgoTraceback), null, null);

            var (f, err) = ioutil.TempFile("", "prof");
            if (err != null)
            {
                fmt.Fprintln(os.Stderr, err);
                os.Exit(2L);
            }

            {
                var err__prev1 = err;

                var err = pprof.StartCPUProfile(f);

                if (err != null)
                {
                    fmt.Fprintln(os.Stderr, err);
                    os.Exit(2L);
                }

                err = err__prev1;

            }


            var t0 = time.Now();
            while (C.getCpuHogCount() < 2L && time.Since(t0) < time.Second)
            {
                C.cpuHog();
            }


            pprof.StopCPUProfile();

            var name = f.Name();
            {
                var err__prev1 = err;

                err = f.Close();

                if (err != null)
                {
                    fmt.Fprintln(os.Stderr, err);
                    os.Exit(2L);
                }

                err = err__prev1;

            }


            fmt.Println(name);

        }
    }
}
