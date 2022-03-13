// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !plan9,!windows

// package main -- go2cs converted at 2022 March 13 05:29:31 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprogcgo\exec.go
namespace go;
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



} // end main_package
