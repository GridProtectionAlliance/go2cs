// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build go1.7
// +build go1.7

// package gc -- go2cs converted at 2022 March 13 06:27:55 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\gc\trace.go
namespace go.cmd.compile.@internal;

using os = os_package;
using tracepkg = runtime.trace_package;

using @base = cmd.compile.@internal.@base_package;

public static partial class gc_package {

private static void init() {
    traceHandler = traceHandlerGo17;
}

private static void traceHandlerGo17(@string traceprofile) {
    var (f, err) = os.Create(traceprofile);
    if (err != null) {
        @base.Fatalf("%v", err);
    }
    {
        var err = tracepkg.Start(f);

        if (err != null) {
            @base.Fatalf("%v", err);
        }
    }
    @base.AtExit(tracepkg.Stop);
}

} // end gc_package
