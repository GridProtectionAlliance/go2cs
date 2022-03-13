// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package fmtcmd implements the ``go fmt'' command.

// package fmtcmd -- go2cs converted at 2022 March 13 06:29:31 UTC
// import "cmd/go/internal/fmtcmd" ==> using fmtcmd = go.cmd.go.@internal.fmtcmd_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\fmtcmd\fmt.go
namespace go.cmd.go.@internal;

using context = context_package;
using errors = errors_package;
using fmt = fmt_package;
using os = os_package;
using filepath = path.filepath_package;
using runtime = runtime_package;
using sync = sync_package;

using @base = cmd.go.@internal.@base_package;
using cfg = cmd.go.@internal.cfg_package;
using load = cmd.go.@internal.load_package;
using modload = cmd.go.@internal.modload_package;
using str = cmd.go.@internal.str_package;
using System;
using System.Threading;

public static partial class fmtcmd_package {

private static void init() {
    @base.AddBuildFlagsNX(_addr_CmdFmt.Flag);
    @base.AddModFlag(_addr_CmdFmt.Flag);
    @base.AddModCommonFlags(_addr_CmdFmt.Flag);
}

public static ptr<base.Command> CmdFmt = addr(new base.Command(Run:runFmt,UsageLine:"go fmt [-n] [-x] [packages]",Short:"gofmt (reformat) package sources",Long:`
Fmt runs the command 'gofmt -l -w' on the packages named
by the import paths. It prints the names of the files that are modified.

For more about gofmt, see 'go doc cmd/gofmt'.
For more about specifying packages, see 'go help packages'.

The -n flag prints commands that would be executed.
The -x flag prints commands as they are executed.

The -mod flag's value sets which module download mode
to use: readonly or vendor. See 'go help modules' for more.

To run gofmt with specific options, run gofmt itself.

See also: go fix, go vet.
	`,));

private static void runFmt(context.Context ctx, ptr<base.Command> _addr_cmd, slice<@string> args) => func((defer, _, _) => {
    ref base.Command cmd = ref _addr_cmd.val;

    var printed = false;
    var gofmt = gofmtPath();
    var procs = runtime.GOMAXPROCS(0);
    sync.WaitGroup wg = default;
    wg.Add(procs);
    var fileC = make_channel<@string>(2 * procs);
    for (nint i = 0; i < procs; i++) {
        go_(() => () => {
            defer(wg.Done());
            {
                var file__prev2 = file;

                foreach (var (__file) in fileC) {
                    file = __file;
                    @base.Run(str.StringList(gofmt, "-l", "-w", file));
                }

                file = file__prev2;
            }
        }());
    }
    foreach (var (_, pkg) in load.PackagesAndErrors(ctx, new load.PackageOpts(), args)) {
        if (modload.Enabled() && pkg.Module != null && !pkg.Module.Main) {
            if (!printed) {
                fmt.Fprintf(os.Stderr, "go: not formatting packages in dependency modules\n");
                printed = true;
            }
            continue;
        }
        if (pkg.Error != null) {
            ptr<load.NoGoError> nogo;
            ptr<load.EmbedError> embed;
            if ((errors.As(pkg.Error, _addr_nogo) || errors.As(pkg.Error, _addr_embed)) && len(pkg.InternalAllGoFiles()) > 0) { 
                // Skip this error, as we will format
                // all files regardless.
            }
            else
 {
                @base.Errorf("%v", pkg.Error);
                continue;
            }
        }
        var files = @base.RelPaths(pkg.InternalAllGoFiles());
        {
            var file__prev2 = file;

            foreach (var (_, __file) in files) {
                file = __file;
                fileC.Send(file);
            }

            file = file__prev2;
        }
    }    close(fileC);
    wg.Wait();
});

private static @string gofmtPath() {
    @string gofmt = "gofmt";
    if (@base.ToolIsWindows) {
        gofmt += @base.ToolWindowsExtension;
    }
    var gofmtPath = filepath.Join(cfg.GOBIN, gofmt);
    {
        var (_, err) = os.Stat(gofmtPath);

        if (err == null) {
            return gofmtPath;
        }
    }

    gofmtPath = filepath.Join(cfg.GOROOT, "bin", gofmt);
    {
        (_, err) = os.Stat(gofmtPath);

        if (err == null) {
            return gofmtPath;
        }
    } 

    // fallback to looking for gofmt in $PATH
    return "gofmt";
}

} // end fmtcmd_package
