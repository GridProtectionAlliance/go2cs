// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !plan9,!windows

// package main -- go2cs converted at 2022 March 13 05:29:32 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprogcgo\numgoroutine.go
namespace go;
/*
#include <stddef.h>
#include <pthread.h>

extern void CallbackNumGoroutine();

static void* thread2(void* arg __attribute__ ((unused))) {
    CallbackNumGoroutine();
    return NULL;
}

static void CheckNumGoroutine() {
    pthread_t tid;
    pthread_create(&tid, NULL, thread2, NULL);
    pthread_join(tid, NULL);
}
*/



} // end main_package
