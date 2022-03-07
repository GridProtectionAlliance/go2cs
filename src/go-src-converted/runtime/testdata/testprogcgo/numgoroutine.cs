// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !plan9,!windows

// package main -- go2cs converted at 2022 March 06 22:26:16 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprogcgo\numgoroutine.go
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
using C = go.C_package;/*
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


using fmt = go.fmt_package;
using runtime = go.runtime_package;
using strings = go.strings_package;

namespace go;

public static partial class main_package {

private static nint baseGoroutines = default;

private static void init() {
    register("NumGoroutine", NumGoroutine);
}

public static void NumGoroutine() { 
    // Test that there are just the expected number of goroutines
    // running. Specifically, test that the spare M's goroutine
    // doesn't show up.
    {
        var (_, ok) = checkNumGoroutine("first", 1 + baseGoroutines);

        if (!ok) {
            return ;
        }
    } 

    // Test that the goroutine for a callback from C appears.
    C.CheckNumGoroutine();

    if (!callbackok) {
        return ;
    }
    {
        (_, ok) = checkNumGoroutine("third", 1 + baseGoroutines);

        if (!ok) {
            return ;
        }
    }


    fmt.Println("OK");

}

private static (@string, bool) checkNumGoroutine(@string label, nint want) {
    @string _p0 = default;
    bool _p0 = default;

    var n = runtime.NumGoroutine();
    if (n != want) {
        fmt.Printf("%s NumGoroutine: want %d; got %d\n", label, want, n);
        return ("", false);
    }
    var sbuf = make_slice<byte>(32 << 10);
    sbuf = sbuf[..(int)runtime.Stack(sbuf, true)];
    n = strings.Count(string(sbuf), "goroutine ");
    if (n != want) {
        fmt.Printf("%s Stack: want %d; got %d:\n%s\n", label, want, n, string(sbuf));
        return ("", false);
    }
    return (string(sbuf), true);

}

private static bool callbackok = default;

//export CallbackNumGoroutine
public static void CallbackNumGoroutine() {
    var (stk, ok) = checkNumGoroutine("second", 2 + baseGoroutines);
    if (!ok) {
        return ;
    }
    if (!strings.Contains(stk, "CallbackNumGoroutine")) {
        fmt.Printf("missing CallbackNumGoroutine from stack:\n%s\n", stk);
        return ;
    }
    callbackok = true;

}

} // end main_package
