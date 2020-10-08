// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !wasm,!windows

// package ld -- go2cs converted at 2020 October 08 04:38:43 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\execarchive.go
using os = go.os_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class ld_package
    {
        private static readonly var syscallExecSupported = (var)true;

        // execArchive invokes the archiver tool with syscall.Exec(), with
        // the expectation that this is the last thing that takes place
        // in the linking operation.


        // execArchive invokes the archiver tool with syscall.Exec(), with
        // the expectation that this is the last thing that takes place
        // in the linking operation.
        private static void execArchive(this ptr<Link> _addr_ctxt, slice<@string> argv)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            error err = default!;
            var argv0 = argv[0L];
            if (filepath.Base(argv0) == argv0)
            {
                argv0, err = exec.LookPath(argv0);
                if (err != null)
                {
                    Exitf("cannot find %s: %v", argv[0L], err);
                }

            }

            if (ctxt.Debugvlog != 0L)
            {
                ctxt.Logf("invoking archiver with syscall.Exec()\n");
            }

            err = error.As(syscall.Exec(argv0, argv, os.Environ()))!;
            if (err != null)
            {
                Exitf("running %s failed: %v", argv[0L], err);
            }

        }
    }
}}}}
