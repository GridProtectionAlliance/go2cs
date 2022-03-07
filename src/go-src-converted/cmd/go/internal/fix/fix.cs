// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package fix implements the ``go fix'' command.
// package fix -- go2cs converted at 2022 March 06 23:16:01 UTC
// import "cmd/go/internal/fix" ==> using fix = go.cmd.go.@internal.fix_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\fix\fix.go
using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using load = go.cmd.go.@internal.load_package;
using modload = go.cmd.go.@internal.modload_package;
using str = go.cmd.go.@internal.str_package;
using context = go.context_package;
using fmt = go.fmt_package;
using os = go.os_package;

namespace go.cmd.go.@internal;

public static partial class fix_package {

public static ptr<base.Command> CmdFix = addr(new base.Command(Run:runFix,UsageLine:"go fix [packages]",Short:"update packages to use new APIs",Long:`
Fix runs the Go fix command on the packages named by the import paths.

For more about fix, see 'go doc cmd/fix'.
For more about specifying packages, see 'go help packages'.

To run fix with specific options, run 'go tool fix'.

See also: go fmt, go vet.
	`,));

private static void runFix(context.Context ctx, ptr<base.Command> _addr_cmd, slice<@string> args) {
    ref base.Command cmd = ref _addr_cmd.val;

    var pkgs = load.PackagesAndErrors(ctx, new load.PackageOpts(), args);
    nint w = 0;
    {
        var pkg__prev1 = pkg;

        foreach (var (_, __pkg) in pkgs) {
            pkg = __pkg;
            if (pkg.Error != null) {
                @base.Errorf("%v", pkg.Error);
                continue;
            }
            pkgs[w] = pkg;
            w++;
        }
        pkg = pkg__prev1;
    }

    pkgs = pkgs[..(int)w];

    var printed = false;
    {
        var pkg__prev1 = pkg;

        foreach (var (_, __pkg) in pkgs) {
            pkg = __pkg;
            if (modload.Enabled() && pkg.Module != null && !pkg.Module.Main) {
                if (!printed) {
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
        pkg = pkg__prev1;
    }
}

} // end fix_package
