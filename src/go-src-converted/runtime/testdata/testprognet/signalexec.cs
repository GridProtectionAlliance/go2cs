// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux netbsd openbsd

// This is in testprognet instead of testprog because testprog
// must not import anything (like net, but also like os/signal)
// that kicks off background goroutines during init.

// package main -- go2cs converted at 2022 March 13 05:40:28 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprognet\signalexec.go
namespace go;

using fmt = fmt_package;
using os = os_package;
using exec = os.exec_package;
using signal = os.signal_package;
using sync = sync_package;
using syscall = syscall_package;
using time = time_package;
using System;
using System.Threading;

public static partial class main_package {

private static void init() {
    register("SignalDuringExec", SignalDuringExec);
    register("Nop", Nop);
}

public static void SignalDuringExec() => func((defer, _, _) => {
    var pgrp = syscall.Getpgrp();

    const nint tries = 10;



    sync.WaitGroup wg = default;
    var c = make_channel<os.Signal>(tries);
    signal.Notify(c, syscall.SIGWINCH);
    wg.Add(1);
    go_(() => () => {
        defer(wg.Done());
        foreach (>>MARKER:FORRANGEEXPRESSIONS_LEVEL_1<< in c) {>>MARKER:FORRANGEMUTABLEEXPRESSIONS_LEVEL_1<<
        }
    }());

    for (nint i = 0; i < tries; i++) {
        time.Sleep(time.Microsecond);
        wg.Add(2);
        go_(() => () => {
            defer(wg.Done());
            var cmd = exec.Command(os.Args[0], "Nop");
            cmd.Stdout = os.Stdout;
            cmd.Stderr = os.Stderr;
            {
                var err = cmd.Run();

                if (err != null) {
                    fmt.Printf("Start failed: %v", err);
                }

            }
        }());
        go_(() => () => {
            defer(wg.Done());
            syscall.Kill(-pgrp, syscall.SIGWINCH);
        }());
    }

    signal.Stop(c);
    close(c);
    wg.Wait();

    fmt.Println("OK");
});

public static void Nop() { 
    // This is just for SignalDuringExec.
}

} // end main_package
