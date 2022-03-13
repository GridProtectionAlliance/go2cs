// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// We only build this file with the tag "threadprof", since it starts
// a thread running a busy loop at constructor time.

// +build !plan9,!windows
// +build threadprof

// package main -- go2cs converted at 2022 March 13 05:29:34 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprogcgo\threadprof.go
namespace go;
/*
#include <stdint.h>
#include <signal.h>
#include <pthread.h>

volatile int32_t spinlock;

static void *thread1(void *p) {
    (void)p;
    while (spinlock == 0)
        ;
    pthread_kill(pthread_self(), SIGPROF);
    spinlock = 0;
    return NULL;
}

__attribute__((constructor)) void issue9456() {
    pthread_t tid;
    pthread_create(&tid, 0, thread1, NULL);
}

void **nullptr;

void *crash(void *p) {
    *nullptr = p;
    return 0;
}

int start_crashing_thread(void) {
    pthread_t tid;
    return pthread_create(&tid, 0, crash, 0);
}
*/



} // end main_package
