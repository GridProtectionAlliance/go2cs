// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !plan9,!windows

// package main -- go2cs converted at 2022 March 13 05:29:31 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprogcgo\eintr.go
namespace go;
/*
#include <errno.h>
#include <signal.h>
#include <string.h>

static int clearRestart(int sig) {
    struct sigaction sa;

    memset(&sa, 0, sizeof sa);
    if (sigaction(sig, NULL, &sa) < 0) {
        return errno;
    }
    sa.sa_flags &=~ SA_RESTART;
    if (sigaction(sig, &sa, NULL) < 0) {
        return errno;
    }
    return 0;
}
*/



} // end main_package
