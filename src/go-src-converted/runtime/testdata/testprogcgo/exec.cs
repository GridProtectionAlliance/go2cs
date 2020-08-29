// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !plan9,!windows

// package main -- go2cs converted at 2020 August 29 08:24:48 UTC
// Original source: C:\Go\src\runtime\testdata\testprogcgo\exec.go
/*
#include <stddef.h>
#include <signal.h>
#include <pthread.h>

// Save the signal mask at startup so that we see what it is before
// the Go runtime starts setting up signals.

static sigset_t mask;

static void init(void) __attribute__ ((constructor));

static void init() {
    sigemptyset(&mask);
    pthread_sigmask(SIG_SETMASK, NULL, &mask);
}

int SIGINTBlocked() {
    return sigismember(&mask, SIGINT);
}
*/
using C = go.C_package;/*
#include <stddef.h>
#include <signal.h>
#include <pthread.h>

// Save the signal mask at startup so that we see what it is before
// the Go runtime starts setting up signals.

static sigset_t mask;

static void init(void) __attribute__ ((constructor));

static void init() {
    sigemptyset(&mask);
    pthread_sigmask(SIG_SETMASK, NULL, &mask);
}

int SIGINTBlocked() {
    return sigismember(&mask, SIGINT);
}
*/


using fmt = go.fmt_package;
using os = go.os_package;
using exec = go.os.exec_package;
using signal = go.os.signal_package;
using sync = go.sync_package;
using syscall = go.syscall_package;
using static go.builtin;
using System;
using System.Threading;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("CgoExecSignalMask", CgoExecSignalMask);
        }

        public static void CgoExecSignalMask() => func((defer, _, __) =>
        {
            if (len(os.Args) > 2L && os.Args[2L] == "testsigint")
            {
                if (C.SIGINTBlocked() != 0L)
                {
                    os.Exit(1L);
                }
                os.Exit(0L);
            }
            var c = make_channel<os.Signal>(1L);
            signal.Notify(c, syscall.SIGTERM);
            go_(() => () =>
            {
                foreach (>>MARKER:FORRANGEEXPRESSIONS_LEVEL_1<< in c)
                {>>MARKER:FORRANGEMUTABLEEXPRESSIONS_LEVEL_1<<
                }
            }());

            const long goCount = 10L;

            const long execCount = 10L;

            sync.WaitGroup wg = default;
            wg.Add(goCount * execCount + goCount);
            for (long i = 0L; i < goCount; i++)
            {
                go_(() => () =>
                {
                    defer(wg.Done());
                    for (long j = 0L; j < execCount; j++)
                    {
                        var c2 = make_channel<os.Signal>(1L);
                        signal.Notify(c2, syscall.SIGUSR1);
                        syscall.Kill(os.Getpid(), syscall.SIGTERM);
                        go_(() => j =>
                        {
                            defer(wg.Done());
                            var cmd = exec.Command(os.Args[0L], "CgoExecSignalMask", "testsigint");
                            cmd.Stdin = os.Stdin;
                            cmd.Stdout = os.Stdout;
                            cmd.Stderr = os.Stderr;
                            {
                                var err = cmd.Run();

                                if (err != null)
                                {
                                    fmt.Printf("iteration %d: %v\n", j, err);
                                    os.Exit(1L);
                                }

                            }
                        }(j));
                        signal.Stop(c2);
                    }

                }());
            }

            wg.Wait();

            fmt.Println("OK");
        });
    }
}
