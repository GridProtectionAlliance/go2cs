// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !wasm && !windows
// +build !wasm,!windows

// package ld -- go2cs converted at 2022 March 13 06:34:22 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\execarchive.go
namespace go.cmd.link.@internal;

using exec = @internal.execabs_package;
using os = os_package;
using filepath = path.filepath_package;
using syscall = syscall_package;

public static partial class ld_package {

private static readonly var syscallExecSupported = true;

// execArchive invokes the archiver tool with syscall.Exec(), with
// the expectation that this is the last thing that takes place
// in the linking operation.


// execArchive invokes the archiver tool with syscall.Exec(), with
// the expectation that this is the last thing that takes place
// in the linking operation.
private static void execArchive(this ptr<Link> _addr_ctxt, slice<@string> argv) {
    ref Link ctxt = ref _addr_ctxt.val;

    error err = default!;
    var argv0 = argv[0];
    if (filepath.Base(argv0) == argv0) {
        argv0, err = exec.LookPath(argv0);
        if (err != null) {
            Exitf("cannot find %s: %v", argv[0], err);
        }
    }
    if (ctxt.Debugvlog != 0) {
        ctxt.Logf("invoking archiver with syscall.Exec()\n");
    }
    err = error.As(syscall.Exec(argv0, argv, os.Environ()))!;
    if (err != null) {
        Exitf("running %s failed: %v", argv[0], err);
    }
}

} // end ld_package
