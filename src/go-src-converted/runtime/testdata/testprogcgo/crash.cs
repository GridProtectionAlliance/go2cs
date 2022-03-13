// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 13 05:29:29 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprogcgo\crash.go
namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using System;
using System.Threading;

public static partial class main_package {

private static void init() {
    register("Crash", Crash);
}

private static void test(@string name) => func((defer, _, _) => {
    defer(() => {
        {
            var x = recover();

            if (x != null) {
                fmt.Printf(" recovered");
            }

        }
        fmt.Printf(" done\n");
    }());
    fmt.Printf("%s:", name);
    ptr<@string> s;
    _ = s.val;
    fmt.Print("SHOULD NOT BE HERE");
});

private static void testInNewThread(@string name) {
    var c = make_channel<bool>();
    go_(() => () => {
        runtime.LockOSThread();
        test(name);
        c.Send(true);
    }());
    c.Receive();
}

public static void Crash() {
    runtime.LockOSThread();
    test("main");
    testInNewThread("new-thread");
    testInNewThread("second-new-thread");
    test("main-again");
}

} // end main_package
