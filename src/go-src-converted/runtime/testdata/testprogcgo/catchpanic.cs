// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !plan9,!windows

// package main -- go2cs converted at 2022 March 13 05:29:28 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprogcgo\catchpanic.go
namespace go;
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



} // end main_package
