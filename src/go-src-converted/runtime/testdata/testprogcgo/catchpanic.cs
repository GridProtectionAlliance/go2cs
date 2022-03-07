// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !plan9,!windows

// package main -- go2cs converted at 2022 March 06 22:26:10 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprogcgo\catchpanic.go
/*
#include <signal.h>
#include <stdlib.h>
#include <string.h>

static void abrthandler(int signum) {
    if (signum == SIGABRT) {
        exit(0);  // success
    }
}

void registerAbortHandler() {
    struct sigaction act;
    memset(&act, 0, sizeof act);
    act.sa_handler = abrthandler;
    sigaction(SIGABRT, &act, NULL);
}

static void __attribute__ ((constructor)) sigsetup(void) {
    if (getenv("CGOCATCHPANIC_EARLY_HANDLER") == NULL)
        return;
    registerAbortHandler();
}
*/
using C = go.C_package;/*
#include <signal.h>
#include <stdlib.h>
#include <string.h>

static void abrthandler(int signum) {
    if (signum == SIGABRT) {
        exit(0);  // success
    }
}

void registerAbortHandler() {
    struct sigaction act;
    memset(&act, 0, sizeof act);
    act.sa_handler = abrthandler;
    sigaction(SIGABRT, &act, NULL);
}

static void __attribute__ ((constructor)) sigsetup(void) {
    if (getenv("CGOCATCHPANIC_EARLY_HANDLER") == NULL)
        return;
    registerAbortHandler();
}
*/

using os = go.os_package;

namespace go;

public static partial class main_package {

private static void init() {
    register("CgoCatchPanic", CgoCatchPanic);
}

// Test that the SIGABRT raised by panic can be caught by an early signal handler.
public static void CgoCatchPanic() => func((_, panic, _) => {
    {
        var (_, ok) = os.LookupEnv("CGOCATCHPANIC_EARLY_HANDLER");

        if (!ok) {
            C.registerAbortHandler();
        }
    }

    panic("catch me");

});

} // end main_package
