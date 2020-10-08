// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package fix implements the ``go fix'' command.
// package fix -- go2cs converted at 2020 October 08 04:33:37 UTC
// import "cmd/go/internal/fix" ==> using fix = go.cmd.go.@internal.fix_package
// Original source: C:\Go\src\cmd\go\internal\fix\fix.go
using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using load = go.cmd.go.@internal.load_package;
using modload = go.cmd.go.@internal.modload_package;
using str = go.cmd.go.@internal.str_package;
using fmt = go.fmt_package;
using os = go.os_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class fix_package
    {
        public static ptr<base.Command> CmdFix = addr(new base.Command(Run:runFix,UsageLine:"go fix [packages]",Short:"update packages to use new APIs",Long:`
Fix runs the Go fix command on the packages named by the import paths.

For more about fix, see 'go doc cmd/fix'.
For more about specifying packages, see 'go help packages'.

To run fix with specific options, run 'go tool fix'.

See also: go fmt, go vet.
	`,));

        private static void runFix(ptr<base.Command> _addr_cmd, slice<@string> args)
        {
            ref base.Command cmd = ref _addr_cmd.val;

            var printed = false;
            foreach (var (_, pkg) in load.Packages(args))
            {
                if (modload.Enabled() && pkg.Module != null && !pkg.Module.Main)
                {
                    if (!printed)
                    {
                        fmt.Fprintf(os.Stderr, "go: not fixing packages in dependency modules\n");
                        printed = true;
                    }

                    continue;

                } 
                // Use pkg.gofiles instead of pkg.Dir so that
                // the command only applies to this package,
                // not to packages in subdirectories.
                var files = @base.RelPaths(pkg.InternalAllGoFiles());
                @base.Run(str.StringList(cfg.BuildToolexec, @base.Tool("fix"), files));

            }

        }
    }
}}}}
