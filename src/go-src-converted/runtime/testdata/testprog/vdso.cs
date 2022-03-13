// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Invoke signal hander in the VDSO context (see issue 32912).

// package main -- go2cs converted at 2022 March 13 05:29:28 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprog\vdso.go
namespace go;

using fmt = fmt_package;
using os = os_package;
using pprof = runtime.pprof_package;
using time = time_package;

public static partial class main_package {

private static void init() {
    register("SignalInVDSO", signalInVDSO);
}

private static void signalInVDSO() {
    var (f, err) = os.CreateTemp("", "timeprofnow");
    if (err != null) {
        fmt.Fprintln(os.Stderr, err);
        os.Exit(2);
    }
    {
        var err__prev1 = err;

        var err = pprof.StartCPUProfile(f);

        if (err != null) {
            fmt.Fprintln(os.Stderr, err);
            os.Exit(2);
        }
        err = err__prev1;

    }

    var t0 = time.Now();
    var t1 = t0; 
    // We should get a profiling signal 100 times a second,
    // so running for 1 second should be sufficient.
    while (t1.Sub(t0) < time.Second) {
        t1 = time.Now();
    }

    pprof.StopCPUProfile();

    var name = f.Name();
    {
        var err__prev1 = err;

        err = f.Close();

        if (err != null) {
            fmt.Fprintln(os.Stderr, err);
            os.Exit(2);
        }
        err = err__prev1;

    }

    {
        var err__prev1 = err;

        err = os.Remove(name);

        if (err != null) {
            fmt.Fprintln(os.Stderr, err);
            os.Exit(2);
        }
        err = err__prev1;

    }

    fmt.Println("success");
}

} // end main_package
