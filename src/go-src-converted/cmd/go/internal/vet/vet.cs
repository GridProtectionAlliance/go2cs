// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package vet implements the ``go vet'' command.

// package vet -- go2cs converted at 2022 March 13 06:29:54 UTC
// import "cmd/go/internal/vet" ==> using vet = go.cmd.go.@internal.vet_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\vet\vet.go
namespace go.cmd.go.@internal;

using context = context_package;
using fmt = fmt_package;
using filepath = path.filepath_package;

using @base = cmd.go.@internal.@base_package;
using cfg = cmd.go.@internal.cfg_package;
using load = cmd.go.@internal.load_package;
using trace = cmd.go.@internal.trace_package;
using work = cmd.go.@internal.work_package;


// Break init loop.

using System;
public static partial class vet_package {

private static void init() {
    CmdVet.Run = runVet;
}

public static ptr<base.Command> CmdVet = addr(new base.Command(CustomFlags:true,UsageLine:"go vet [-n] [-x] [-vettool prog] [build flags] [vet flags] [packages]",Short:"report likely mistakes in packages",Long:`
Vet runs the Go vet command on the packages named by the import paths.

For more about vet and its flags, see 'go doc cmd/vet'.
For more about specifying packages, see 'go help packages'.
For a list of checkers and their flags, see 'go tool vet help'.
For details of a specific checker such as 'printf', see 'go tool vet help printf'.

The -n flag prints commands that would be executed.
The -x flag prints commands as they are executed.

The -vettool=prog flag selects a different analysis tool with alternative
or additional checks.
For example, the 'shadow' analyzer can be built and run using these commands:

  go install golang.org/x/tools/go/analysis/passes/shadow/cmd/shadow
  go vet -vettool=$(which shadow)

The build flags supported by go vet are those that control package resolution
and execution, such as -n, -x, -v, -tags, and -toolexec.
For more about these flags, see 'go help build'.

See also: go fmt, go fix.
	`,));

private static void runVet(context.Context ctx, ptr<base.Command> _addr_cmd, slice<@string> args) => func((defer, _, _) => {
    ref base.Command cmd = ref _addr_cmd.val;

    var (vetFlags, pkgArgs) = vetFlags(args);

    if (cfg.DebugTrace != "") {
        Func<error> close = default;
        error err = default!;
        ctx, close, err = trace.Start(ctx, cfg.DebugTrace);
        if (err != null) {
            @base.Fatalf("failed to start trace: %v", err);
        }
        defer(() => {
            {
                error err__prev2 = err;

                err = close();

                if (err != null) {
                    @base.Fatalf("failed to stop trace: %v", err);
                }

                err = err__prev2;

            }
        }());
    }
    var (ctx, span) = trace.StartSpan(ctx, fmt.Sprint("Running ", cmd.Name(), " command"));
    defer(span.Done());

    work.BuildInit();
    work.VetFlags = vetFlags;
    if (len(vetFlags) > 0) {
        work.VetExplicit = true;
    }
    if (vetTool != "") {
        err = default!;
        work.VetTool, err = filepath.Abs(vetTool);
        if (err != null) {
            @base.Fatalf("%v", err);
        }
    }
    load.PackageOpts pkgOpts = new load.PackageOpts(ModResolveTests:true);
    var pkgs = load.PackagesAndErrors(ctx, pkgOpts, pkgArgs);
    load.CheckPackageErrors(pkgs);
    if (len(pkgs) == 0) {
        @base.Fatalf("no packages to vet");
    }
    work.Builder b = default;
    b.Init();

    ptr<work.Action> root = addr(new work.Action(Mode:"go vet"));
    foreach (var (_, p) in pkgs) {
        var (_, ptest, pxtest, err) = load.TestPackagesFor(ctx, pkgOpts, p, null);
        if (err != null) {
            @base.Errorf("%v", err);
            continue;
        }
        if (len(ptest.GoFiles) == 0 && len(ptest.CgoFiles) == 0 && pxtest == null) {
            @base.Errorf("go vet %s: no Go files in %s", p.ImportPath, p.Dir);
            continue;
        }
        if (len(ptest.GoFiles) > 0 || len(ptest.CgoFiles) > 0) {
            root.Deps = append(root.Deps, b.VetAction(work.ModeBuild, work.ModeBuild, ptest));
        }
        if (pxtest != null) {
            root.Deps = append(root.Deps, b.VetAction(work.ModeBuild, work.ModeBuild, pxtest));
        }
    }    b.Do(ctx, root);
});

} // end vet_package
