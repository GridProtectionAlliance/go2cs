// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !plan9,!windows

// Test that a sequence of callbacks from C to Go get the same m.
// This failed to be true on arm and arm64, which was the root cause
// of issue 13881.

// package main -- go2cs converted at 2022 March 13 05:29:29 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprogcgo\dropm.go
namespace go;
/*
#include <stddef.h>
#include <pthread.h>

extern void GoCheckM();

static void* thread(void* arg __attribute__ ((unused))) {
    GoCheckM();
    return NULL;
}

static void CheckM() {
    pthread_t tid;
    pthread_create(&tid, NULL, thread, NULL);
    pthread_join(tid, NULL);
    pthread_create(&tid, NULL, thread, NULL);
    pthread_join(tid, NULL);
}
*/



} // end main_package
