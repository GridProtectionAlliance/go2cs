// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !plan9,!windows

// package main -- go2cs converted at 2022 March 06 22:26:10 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprogcgo\callback.go
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
using C = go.C_package;/*
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


using fmt = go.fmt_package;
using os = go.os_package;
using runtime = go.runtime_package;
using System;
using System.Threading;


namespace go;

public static partial class main_package {

private static void init() {
    register("CgoCallbackGC", CgoCallbackGC);
}

//export go_callback
private static void go_callback() {
    runtime.GC();
    grow();
    runtime.GC();
}

private static nint cnt = default;

private static void grow() => func((_, panic, _) => {
    ref nint x = ref heap(10000, out ptr<nint> _addr_x);
    ref nint sum = ref heap(0, out ptr<nint> _addr_sum);
    if (grow1(_addr_x, _addr_sum) == 0) {
        panic("bad");
    }
});

private static nint grow1(ptr<nint> _addr_x, ptr<nint> _addr_sum) {
    ref nint x = ref _addr_x.val;
    ref nint sum = ref _addr_sum.val;

    if (x == 0.val) {
        return sum + 1.val;
    }
    x--;
    ref var sum1 = ref heap(sum + x.val, out ptr<var> _addr_sum1);
    return grow1(_addr_x, _addr_sum1);

}

public static void CgoCallbackGC() {
    nint P = 100;
    if (os.Getenv("RUNTIME_TESTING_SHORT") != "") {
        P = 10;
    }
    var done = make_channel<bool>(); 
    // allocate a bunch of stack frames and spray them with pointers
    {
        nint i__prev1 = i;

        for (nint i = 0; i < P; i++) {
            go_(() => () => {
                grow();
                done.Send(true);
            }());
        }

        i = i__prev1;
    }
    {
        nint i__prev1 = i;

        for (i = 0; i < P; i++) {
            done.Receive();
        }

        i = i__prev1;
    } 
    // now give these stack frames to cgo callbacks
    {
        nint i__prev1 = i;

        for (i = 0; i < P; i++) {
            go_(() => () => {
                C.foo();
                done.Send(true);
            }());
        }

        i = i__prev1;
    }
    {
        nint i__prev1 = i;

        for (i = 0; i < P; i++) {
            done.Receive();
        }

        i = i__prev1;
    }
    fmt.Printf("OK\n");

}

} // end main_package
