// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !plan9,!windows

// package main -- go2cs converted at 2022 March 13 05:29:28 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprogcgo\callback.go
namespace go;
/*
#include <pthread.h>

void go_callback();

static void *thr(void *arg) {
    go_callback();
    return 0;
}

static void foo() {
    pthread_t th;
    pthread_attr_t attr;
    pthread_attr_init(&attr);
    pthread_attr_setstacksize(&attr, 256 << 10);
    pthread_create(&th, &attr, thr, 0);
    pthread_join(th, 0);
}
*/



} // end main_package
