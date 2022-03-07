// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !plan9,!windows

// package main -- go2cs converted at 2022 March 06 22:26:14 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprogcgo\exec.go
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
using fs = go.io.fs_package;
using os = go.os_package;
using exec = go.os.exec_package;
using signal = go.os.signal_package;
using sync = go.sync_package;
using syscall = go.syscall_package;
using System;
using System.Threading;


namespace go;

public static partial class main_package {

private static void init() {
    register("CgoExecSignalMask", CgoExecSignalMask);
}

public static void CgoExecSignalMask() => func((defer, _, _) => {
    if (len(os.Args) > 2 && os.Args[2] == "testsigint") {
        if (C.SIGINTBlocked() != 0) {
            os.Exit(1);
        }
        os.Exit(0);

    }
    var c = make_channel<os.Signal>(1);
    signal.Notify(c, syscall.SIGTERM);
    go_(() => () => {
        foreach (>>MARKER:FORRANGEEXPRESSIONS_LEVEL_1<< in c) {>>MARKER:FORRANGEMUTABLEEXPRESSIONS_LEVEL_1<<
        }
    }());

    const nint goCount = 10;

    const nint execCount = 10;

    sync.WaitGroup wg = default;
    wg.Add(goCount * execCount + goCount);
    for (nint i = 0; i < goCount; i++) {
        go_(() => () => {
            defer(wg.Done());
            for (nint j = 0; j < execCount; j++) {
                var c2 = make_channel<os.Signal>(1);
                signal.Notify(c2, syscall.SIGUSR1);
                syscall.Kill(os.Getpid(), syscall.SIGTERM);
                go_(() => j => {
                    defer(wg.Done());
                    var cmd = exec.Command(os.Args[0], "CgoExecSignalMask", "testsigint");
                    cmd.Stdin = os.Stdin;
                    cmd.Stdout = os.Stdout;
                    cmd.Stderr = os.Stderr;
                    {
                        var err = cmd.Run();

                        if (err != null) { 
                            // An overloaded system
                            // may fail with EAGAIN.
                            // This doesn't tell us
                            // anything useful; ignore it.
                            // Issue #27731.
                            if (isEAGAIN(err)) {
                                return ;
                            }

                            fmt.Printf("iteration %d: %v\n", j, err);
                            os.Exit(1);

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

// isEAGAIN reports whether err is an EAGAIN error from a process execution.
private static bool isEAGAIN(error err) {
    {
        ptr<fs.PathError> (p, ok) = err._<ptr<fs.PathError>>();

        if (ok) {
            err = p.Err;
        }
    }

    return err == syscall.EAGAIN;

}

} // end main_package
