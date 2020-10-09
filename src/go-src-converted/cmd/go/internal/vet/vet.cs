// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package vet implements the ``go vet'' command.
// package vet -- go2cs converted at 2020 October 09 05:45:23 UTC
// import "cmd/go/internal/vet" ==> using vet = go.cmd.go.@internal.vet_package
// Original source: C:\Go\src\cmd\go\internal\vet\vet.go
using @base = go.cmd.go.@internal.@base_package;
using load = go.cmd.go.@internal.load_package;
using modload = go.cmd.go.@internal.modload_package;
using work = go.cmd.go.@internal.work_package;
using filepath = go.path.filepath_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class vet_package
    {
        // Break init loop.
        private static void init()
        {
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

        private static void runVet(ptr<base.Command> _addr_cmd, slice<@string> args)
        {
            ref base.Command cmd = ref _addr_cmd.val;

            modload.LoadTests = true;

            var (vetFlags, pkgArgs) = vetFlags(args);

            work.BuildInit();
            work.VetFlags = vetFlags;
            if (len(vetFlags) > 0L)
            {
                work.VetExplicit = true;
            }

            if (vetTool != "")
            {
                error err = default!;
                work.VetTool, err = filepath.Abs(vetTool);
                if (err != null)
                {
                    @base.Fatalf("%v", err);
                }

            }

            var pkgs = load.PackagesForBuild(pkgArgs);
            if (len(pkgs) == 0L)
            {
                @base.Fatalf("no packages to vet");
            }

            work.Builder b = default;
            b.Init();

            ptr<work.Action> root = addr(new work.Action(Mode:"go vet"));
            foreach (var (_, p) in pkgs)
            {
                var (_, ptest, pxtest, err) = load.TestPackagesFor(p, null);
                if (err != null)
                {
                    @base.Errorf("%v", err);
                    continue;
                }

                if (len(ptest.GoFiles) == 0L && len(ptest.CgoFiles) == 0L && pxtest == null)
                {
                    @base.Errorf("go vet %s: no Go files in %s", p.ImportPath, p.Dir);
                    continue;
                }

                if (len(ptest.GoFiles) > 0L || len(ptest.CgoFiles) > 0L)
                {
                    root.Deps = append(root.Deps, b.VetAction(work.ModeBuild, work.ModeBuild, ptest));
                }

                if (pxtest != null)
                {
                    root.Deps = append(root.Deps, b.VetAction(work.ModeBuild, work.ModeBuild, pxtest));
                }

            }
            b.Do(root);

        }
    }
}}}}
