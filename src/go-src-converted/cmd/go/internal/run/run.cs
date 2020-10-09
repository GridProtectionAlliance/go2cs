// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package run implements the ``go run'' command.
// package run -- go2cs converted at 2020 October 09 05:45:21 UTC
// import "cmd/go/internal/run" ==> using run = go.cmd.go.@internal.run_package
// Original source: C:\Go\src\cmd\go\internal\run\run.go
using fmt = go.fmt_package;
using os = go.os_package;
using path = go.path_package;
using strings = go.strings_package;

using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using load = go.cmd.go.@internal.load_package;
using str = go.cmd.go.@internal.str_package;
using work = go.cmd.go.@internal.work_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class run_package
    {
        public static ptr<base.Command> CmdRun = addr(new base.Command(UsageLine:"go run [build flags] [-exec xprog] package [arguments...]",Short:"compile and run Go program",Long:`
Run compiles and runs the named main Go package.
Typically the package is specified as a list of .go source files from a single directory,
but it may also be an import path, file system path, or pattern
matching a single known package, as in 'go run .' or 'go run my/cmd'.

By default, 'go run' runs the compiled binary directly: 'a.out arguments...'.
If the -exec flag is given, 'go run' invokes the binary using xprog:
	'xprog a.out arguments...'.
If the -exec flag is not given, GOOS or GOARCH is different from the system
default, and a program named go_$GOOS_$GOARCH_exec can be found
on the current search path, 'go run' invokes the binary using that program,
for example 'go_js_wasm_exec a.out arguments...'. This allows execution of
cross-compiled programs when a simulator or other execution method is
available.

The exit status of Run is not the exit status of the compiled binary.

For more about build flags, see 'go help build'.
For more about specifying packages, see 'go help packages'.

See also: go build.
	`,));

        private static void init()
        {
            CmdRun.Run = runRun; // break init loop

            work.AddBuildFlags(CmdRun, work.DefaultBuildFlags);
            CmdRun.Flag.Var((@base.StringsFlag.val)(_addr_work.ExecCmd), "exec", "");

        }

        private static (long, error) printStderr(params object[] args)
        {
            long _p0 = default;
            error _p0 = default!;
            args = args.Clone();

            return fmt.Fprint(os.Stderr, args);
        }

        private static void runRun(ptr<base.Command> _addr_cmd, slice<@string> args)
        {
            ref base.Command cmd = ref _addr_cmd.val;

            work.BuildInit();
            work.Builder b = default;
            b.Init();
            b.Print = printStderr;
            long i = 0L;
            while (i < len(args) && strings.HasSuffix(args[i], ".go"))
            {
                i++;
            }

            ptr<load.Package> p;
            if (i > 0L)
            {
                var files = args[..i];
                foreach (var (_, file) in files)
                {
                    if (strings.HasSuffix(file, "_test.go"))
                    { 
                        // GoFilesPackage is going to assign this to TestGoFiles.
                        // Reject since it won't be part of the build.
                        @base.Fatalf("go run: cannot run *_test.go files (%s)", file);

                    }

                }
                p = load.GoFilesPackage(files);

            }
            else if (len(args) > 0L && !strings.HasPrefix(args[0L], "-"))
            {
                var pkgs = load.PackagesAndErrors(args[..1L]);
                if (len(pkgs) == 0L)
                {
                    @base.Fatalf("go run: no packages loaded from %s", args[0L]);
                }

                if (len(pkgs) > 1L)
                {
                    slice<@string> names = default;
                    {
                        ptr<load.Package> p__prev1 = p;

                        foreach (var (_, __p) in pkgs)
                        {
                            p = __p;
                            names = append(names, p.ImportPath);
                        }

                        p = p__prev1;
                    }

                    @base.Fatalf("go run: pattern %s matches multiple packages:\n\t%s", args[0L], strings.Join(names, "\n\t"));

                }
            else
                p = pkgs[0L];
                i++;

            }            {
                @base.Fatalf("go run: no go files listed");
            }

            var cmdArgs = args[i..];
            if (p.Error != null)
            {
                @base.Fatalf("%s", p.Error);
            }

            p.Internal.OmitDebug = true;
            if (len(p.DepsErrors) > 0L)
            { 
                // Since these are errors in dependencies,
                // the same error might show up multiple times,
                // once in each package that depends on it.
                // Only print each once.
                map printed = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<load.PackageError>, bool>{};
                foreach (var (_, err) in p.DepsErrors)
                {
                    if (!printed[err])
                    {
                        printed[err] = true;
                        @base.Errorf("%s", err);
                    }

                }

            }

            @base.ExitIfErrors();
            if (p.Name != "main")
            {
                @base.Fatalf("go run: cannot run non-main package");
            }

            p.Target = ""; // must build - not up to date
            if (p.Internal.CmdlineFiles)
            { 
                //set executable name if go file is given as cmd-argument
                @string src = default;
                if (len(p.GoFiles) > 0L)
                {
                    src = p.GoFiles[0L];
                }
                else if (len(p.CgoFiles) > 0L)
                {
                    src = p.CgoFiles[0L];
                }
                else
                { 
                    // this case could only happen if the provided source uses cgo
                    // while cgo is disabled.
                    @string hint = "";
                    if (!cfg.BuildContext.CgoEnabled)
                    {
                        hint = " (cgo is disabled)";
                    }

                    @base.Fatalf("go run: no suitable source files%s", hint);

                }

                p.Internal.ExeName = src[..len(src) - len(".go")];

            }
            else
            {
                p.Internal.ExeName = path.Base(p.ImportPath);
            }

            var a1 = b.LinkAction(work.ModeBuild, work.ModeBuild, p);
            ptr<work.Action> a = addr(new work.Action(Mode:"go run",Func:buildRunProgram,Args:cmdArgs,Deps:[]*work.Action{a1}));
            b.Do(a);

        }

        // buildRunProgram is the action for running a binary that has already
        // been compiled. We ignore exit status.
        private static error buildRunProgram(ptr<work.Builder> _addr_b, ptr<work.Action> _addr_a)
        {
            ref work.Builder b = ref _addr_b.val;
            ref work.Action a = ref _addr_a.val;

            var cmdline = str.StringList(work.FindExecCmd(), a.Deps[0L].Target, a.Args);
            if (cfg.BuildN || cfg.BuildX)
            {
                b.Showcmd("", "%s", strings.Join(cmdline, " "));
                if (cfg.BuildN)
                {
                    return error.As(null!)!;
                }

            }

            @base.RunStdin(cmdline);
            return error.As(null!)!;

        }
    }
}}}}
