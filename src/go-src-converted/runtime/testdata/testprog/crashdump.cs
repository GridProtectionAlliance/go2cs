// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 06 22:25:58 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprog\crashdump.go
using fmt = go.fmt_package;
using os = go.os_package;
using runtime = go.runtime_package;
using System.Threading;


namespace go;

public static partial class main_package {

private static void init() {
    register("CrashDumpsAllThreads", CrashDumpsAllThreads);
}

public static void CrashDumpsAllThreads() {
    const nint count = 4;

    runtime.GOMAXPROCS(count + 1);

    var chans = make_slice<channel<bool>>(count);
    foreach (var (i) in chans) {
        chans[i] = make_channel<bool>();
        go_(() => crashDumpsAllThreadsLoop(i, chans[i]));
    }    foreach (var (_, c) in chans) {
        c.Receive();
    }    {
        var (_, err) = os.NewFile(3, "pipe").WriteString("x");

        if (err != null) {
            fmt.Fprintf(os.Stderr, "write to pipe failed: %v\n", err);
            os.Exit(2);
        }
    }


}

private static void crashDumpsAllThreadsLoop(nint i, channel<bool> c) {
    close(c);
    while (true) {
        for (nint j = 0; j < 0x7fffffff; j++)         }
    }
}

} // end main_package
