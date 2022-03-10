// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 06 23:22:40 UTC
// Original source: C:\Program Files\Go\src\cmd\pprof\testdata\cpu.go
using flag = go.flag_package;
using fmt = go.fmt_package;
using os = go.os_package;
using pprof = go.runtime.pprof_package;
using time = go.time_package;

namespace go;

public static partial class main_package {

private static var output = flag.String("output", "", "pprof profile output file");

private static void Main() => func((defer, _, _) => {
    flag.Parse();
    if (output == "".val) {
        fmt.Fprintf(os.Stderr, "usage: %s -output file.pprof\n", os.Args[0]);
        os.Exit(2);
    }
    var (f, err) = os.Create(output.val);
    if (err != null) {
        fmt.Fprintln(os.Stderr, err);
        os.Exit(2);
    }
    defer(f.Close());

    {
        var err = pprof.StartCPUProfile(f);

        if (err != null) {
            fmt.Fprintln(os.Stderr, err);
            os.Exit(2);
        }
    }

    defer(pprof.StopCPUProfile()); 

    // Spin for long enough to collect some samples.
    var start = time.Now();
    while (time.Since(start) < time.Second)     }

});

} // end main_package