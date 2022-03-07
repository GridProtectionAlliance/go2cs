// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package run implements the ``go run'' command.
// package run -- go2cs converted at 2022 March 06 23:16:20 UTC
// import "cmd/go/internal/run" ==> using run = go.cmd.go.@internal.run_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\run\run.go
using context = go.context_package;
using fmt = go.fmt_package;
using build = go.go.build_package;
using os = go.os_package;
using path = go.path_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;

using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using load = go.cmd.go.@internal.load_package;
using modload = go.cmd.go.@internal.modload_package;
using str = go.cmd.go.@internal.str_package;
using work = go.cmd.go.@internal.work_package;

namespace go.cmd.go.@internal;

public static partial class run_package {

public static ptr<base.Command> CmdRun = addr(new base.Command(UsageLine:"go run [build flags] [-exec xprog] package [arguments...]",Short:"compile and run Go program",Long:`
Run compiles and runs the named main Go package.
Typically the package is specified as a list of .go source files from a single
directory, but it may also be an import path, file system path, or pattern
matching a single known package, as in 'go run .' or 'go run my/cmd'.

If the package argument has a version suffix (like @latest or @v1.0.0),
"go run" builds the program in module-aware mode, ignoring the go.mod file in
the current directory or any parent directory, if there is one. This is useful
for running programs without affecting the dependencies of the main module.

If the package argument doesn't have a version suffix, "go run" may run in
module-aware mode or GOPATH mode, depending on the GO111MODULE environment
variable and the presence of a go.mod file. See 'go help modules' for details.
If module-aware mode is enabled, "go run" runs in the context of the main
module.

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

private static void init() {
    CmdRun.Run = runRun; // break init loop

    work.AddBuildFlags(CmdRun, work.DefaultBuildFlags);
    CmdRun.Flag.Var((@base.StringsFlag.val)(_addr_work.ExecCmd), "exec", "");

}

private static (nint, error) printStderr(params object[] args) {
    nint _p0 = default;
    error _p0 = default!;
    args = args.Clone();

    return fmt.Fprint(os.Stderr, args);
}

private static void runRun(context.Context ctx, ptr<base.Command> _addr_cmd, slice<@string> args) {
    ref base.Command cmd = ref _addr_cmd.val;

    if (shouldUseOutsideModuleMode(args)) { 
        // Set global module flags for 'go run cmd@version'.
        // This must be done before modload.Init, but we need to call work.BuildInit
        // before loading packages, since it affects package locations, e.g.,
        // for -race and -msan.
        modload.ForceUseModules = true;
        modload.RootMode = modload.NoRoot;
        modload.AllowMissingModuleImports();
        modload.Init();

    }
    work.BuildInit();
    work.Builder b = default;
    b.Init();
    b.Print = printStderr;

    nint i = 0;
    while (i < len(args) && strings.HasSuffix(args[i], ".go")) {
        i++;
    }
    load.PackageOpts pkgOpts = new load.PackageOpts(MainOnly:true);
    ptr<load.Package> p;
    if (i > 0) {
        var files = args[..(int)i];
        foreach (var (_, file) in files) {
            if (strings.HasSuffix(file, "_test.go")) { 
                // GoFilesPackage is going to assign this to TestGoFiles.
                // Reject since it won't be part of the build.
                @base.Fatalf("go run: cannot run *_test.go files (%s)", file);

            }

        }        p = load.GoFilesPackage(ctx, pkgOpts, files);

    }
    else if (len(args) > 0 && !strings.HasPrefix(args[0], "-")) {
        var arg = args[0];
        slice<ptr<load.Package>> pkgs = default;
        if (strings.Contains(arg, "@") && !build.IsLocalImport(arg) && !filepath.IsAbs(arg)) {
            error err = default!;
            pkgs, err = load.PackagesAndErrorsOutsideModule(ctx, pkgOpts, args[..(int)1]);
            if (err != null) {
                @base.Fatalf("go run: %v", err);
            }
        }
        else
 {
            pkgs = load.PackagesAndErrors(ctx, pkgOpts, args[..(int)1]);
        }
        if (len(pkgs) == 0) {
            @base.Fatalf("go run: no packages loaded from %s", arg);
        }
        if (len(pkgs) > 1) {
            slice<@string> names = default;
            {
                ptr<load.Package> p__prev1 = p;

                foreach (var (_, __p) in pkgs) {
                    p = __p;
                    names = append(names, p.ImportPath);
                }

                p = p__prev1;
            }

            @base.Fatalf("go run: pattern %s matches multiple packages:\n\t%s", arg, strings.Join(names, "\n\t"));

        }
    else
        p = pkgs[0];
        i++;

    } {
        @base.Fatalf("go run: no go files listed");
    }
    var cmdArgs = args[(int)i..];
    load.CheckPackageErrors(new slice<ptr<load.Package>>(new ptr<load.Package>[] { p }));

    p.Internal.OmitDebug = true;
    p.Target = ""; // must build - not up to date
    if (p.Internal.CmdlineFiles) { 
        //set executable name if go file is given as cmd-argument
        @string src = default;
        if (len(p.GoFiles) > 0) {
            src = p.GoFiles[0];
        }
        else if (len(p.CgoFiles) > 0) {
            src = p.CgoFiles[0];
        }
        else
 { 
            // this case could only happen if the provided source uses cgo
            // while cgo is disabled.
            @string hint = "";
            if (!cfg.BuildContext.CgoEnabled) {
                hint = " (cgo is disabled)";
            }

            @base.Fatalf("go run: no suitable source files%s", hint);

        }
        p.Internal.ExeName = src[..(int)len(src) - len(".go")];

    }
    else
 {
        p.Internal.ExeName = path.Base(p.ImportPath);
    }
    var a1 = b.LinkAction(work.ModeBuild, work.ModeBuild, p);
    ptr<work.Action> a = addr(new work.Action(Mode:"go run",Func:buildRunProgram,Args:cmdArgs,Deps:[]*work.Action{a1}));
    b.Do(ctx, a);

}

// shouldUseOutsideModuleMode returns whether 'go run' will load packages in
// module-aware mode, ignoring the go.mod file in the current directory. It
// returns true if the first argument contains "@", does not begin with "-"
// (resembling a flag) or end with ".go" (a file). The argument must not be a
// local or absolute file path.
//
// These rules are slightly different than other commands. Whether or not
// 'go run' uses this mode, it interprets arguments ending with ".go" as files
// and uses arguments up to the last ".go" argument to comprise the package.
// If there are no ".go" arguments, only the first argument is interpreted
// as a package path, since there can be only one package.
private static bool shouldUseOutsideModuleMode(slice<@string> args) { 
    // NOTE: "@" not allowed in import paths, but it is allowed in non-canonical
    // versions.
    return len(args) > 0 && !strings.HasSuffix(args[0], ".go") && !strings.HasPrefix(args[0], "-") && strings.Contains(args[0], "@") && !build.IsLocalImport(args[0]) && !filepath.IsAbs(args[0]);

}

// buildRunProgram is the action for running a binary that has already
// been compiled. We ignore exit status.
private static error buildRunProgram(ptr<work.Builder> _addr_b, context.Context ctx, ptr<work.Action> _addr_a) {
    ref work.Builder b = ref _addr_b.val;
    ref work.Action a = ref _addr_a.val;

    var cmdline = str.StringList(work.FindExecCmd(), a.Deps[0].Target, a.Args);
    if (cfg.BuildN || cfg.BuildX) {
        b.Showcmd("", "%s", strings.Join(cmdline, " "));
        if (cfg.BuildN) {
            return error.As(null!)!;
        }
    }
    @base.RunStdin(cmdline);
    return error.As(null!)!;

}

} // end run_package
