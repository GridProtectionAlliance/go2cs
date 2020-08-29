// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package fmtcmd implements the ``go fmt'' command.
// package fmtcmd -- go2cs converted at 2020 August 29 10:00:35 UTC
// import "cmd/go/internal/fmtcmd" ==> using fmtcmd = go.cmd.go.@internal.fmtcmd_package
// Original source: C:\Go\src\cmd\go\internal\fmtcmd\fmt.go
using os = go.os_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using strings = go.strings_package;
using sync = go.sync_package;

using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using load = go.cmd.go.@internal.load_package;
using str = go.cmd.go.@internal.str_package;
using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class fmtcmd_package
    {
        private static void init()
        {
            @base.AddBuildFlagsNX(ref CmdFmt.Flag);
        }

        public static base.Command CmdFmt = ref new base.Command(Run:runFmt,UsageLine:"fmt [-n] [-x] [packages]",Short:"gofmt (reformat) package sources",Long:`
Fmt runs the command 'gofmt -l -w' on the packages named
by the import paths. It prints the names of the files that are modified.

For more about gofmt, see 'go doc cmd/gofmt'.
For more about specifying packages, see 'go help packages'.

The -n flag prints commands that would be executed.
The -x flag prints commands as they are executed.

To run gofmt with specific options, run gofmt itself.

See also: go fix, go vet.
	`,);

        private static void runFmt(ref base.Command _cmd, slice<@string> args) => func(_cmd, (ref base.Command cmd, Defer defer, Panic _, Recover __) =>
        {
            var gofmt = gofmtPath();
            var procs = runtime.GOMAXPROCS(0L);
            sync.WaitGroup wg = default;
            wg.Add(procs);
            var fileC = make_channel<@string>(2L * procs);
            for (long i = 0L; i < procs; i++)
            {
                go_(() => () =>
                {
                    defer(wg.Done());
                    {
                        var file__prev2 = file;

                        foreach (var (__file) in fileC)
                        {
                            file = __file;
                            @base.Run(str.StringList(gofmt, "-l", "-w", file));
                        }

                        file = file__prev2;
                    }

                }());
            }

            foreach (var (_, pkg) in load.PackagesAndErrors(args))
            {
                if (pkg.Error != null)
                {
                    if (strings.HasPrefix(pkg.Error.Err, "build constraints exclude all Go files"))
                    { 
                        // Skip this error, as we will format
                        // all files regardless.
                    }
                    else
                    {
                        @base.Errorf("can't load package: %s", pkg.Error);
                        continue;
                    }
                } 
                // Use pkg.gofiles instead of pkg.Dir so that
                // the command only applies to this package,
                // not to packages in subdirectories.
                var files = @base.RelPaths(pkg.InternalAllGoFiles());
                {
                    var file__prev2 = file;

                    foreach (var (_, __file) in files)
                    {
                        file = __file;
                        fileC.Send(file);
                    }

                    file = file__prev2;
                }

            }
            close(fileC);
            wg.Wait();
        });

        private static @string gofmtPath()
        {
            @string gofmt = "gofmt";
            if (@base.ToolIsWindows)
            {
                gofmt += @base.ToolWindowsExtension;
            }
            var gofmtPath = filepath.Join(cfg.GOBIN, gofmt);
            {
                var (_, err) = os.Stat(gofmtPath);

                if (err == null)
                {
                    return gofmtPath;
                }

            }

            gofmtPath = filepath.Join(cfg.GOROOT, "bin", gofmt);
            {
                (_, err) = os.Stat(gofmtPath);

                if (err == null)
                {
                    return gofmtPath;
                } 

                // fallback to looking for gofmt in $PATH

            } 

            // fallback to looking for gofmt in $PATH
            return "gofmt";
        }
    }
}}}}
