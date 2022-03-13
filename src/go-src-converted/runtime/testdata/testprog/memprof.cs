// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 13 05:29:24 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprog\memprof.go
namespace go;

using bytes = bytes_package;
using fmt = fmt_package;
using os = os_package;
using runtime = runtime_package;
using pprof = runtime.pprof_package;

public static partial class main_package {

private static void init() {
    register("MemProf", MemProf);
}

private static bytes.Buffer memProfBuf = default;
private static @string memProfStr = default;

public static void MemProf() { 
    // Force heap sampling for determinism.
    runtime.MemProfileRate = 1;

    for (nint i = 0; i < 10; i++) {
        fmt.Fprintf(_addr_memProfBuf, "%*d\n", i, i);
    }
    memProfStr = memProfBuf.String();

    runtime.GC();

    var (f, err) = os.CreateTemp("", "memprof");
    if (err != null) {
        fmt.Fprintln(os.Stderr, err);
        os.Exit(2);
    }
    {
        var err__prev1 = err;

        var err = pprof.WriteHeapProfile(f);

        if (err != null) {
            fmt.Fprintln(os.Stderr, err);
            os.Exit(2);
        }
        err = err__prev1;

    }

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
