// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 06 22:25:57 UTC
// Original source: C:\Program Files\Go\src\runtime\race\testdata\cgo_test_main.go
/*
int sync;

void Notify(void)
{
    __sync_fetch_and_add(&sync, 1);
}

void Wait(void)
{
    while(__sync_fetch_and_add(&sync, 0) == 0) {}
}
*/
using C = go.C_package;
using System;
using System.Threading;


namespace go;

public static partial class main_package {

private static void Main() {
    nint data = 0;
    go_(() => () => {
        data = 1;
        C.Notify();
    }());
    C.Wait();
    _ = data;
}

} // end main_package
