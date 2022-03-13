// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 13 05:29:27 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprog\timeprof.go
namespace go;

using fmt = fmt_package;
using os = os_package;
using pprof = runtime.pprof_package;
using time = time_package;

public static partial class main_package {

private static void init() {
    register("TimeProf", TimeProf);
}

public static void TimeProf() {
    var (f, err) = os.CreateTemp("", "timeprof");
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
    // We should get a profiling signal 100 times a second,
    // so running for 1/10 second should be sufficient.
    while (time.Since(t0) < time.Second / 10) {
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

    fmt.Println(name);
}

} // end main_package
