// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// go mod init

// package modcmd -- go2cs converted at 2022 March 13 06:32:24 UTC
// import "cmd/go/internal/modcmd" ==> using modcmd = go.cmd.go.@internal.modcmd_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modcmd\init.go
namespace go.cmd.go.@internal;

using @base = cmd.go.@internal.@base_package;
using modload = cmd.go.@internal.modload_package;
using context = context_package;

public static partial class modcmd_package {

private static ptr<base.Command> cmdInit = addr(new base.Command(UsageLine:"go mod init [module-path]",Short:"initialize new module in current directory",Long:`
Init initializes and writes a new go.mod file in the current directory, in
effect creating a new module rooted at the current directory. The go.mod file
must not already exist.

Init accepts one optional argument, the module path for the new module. If the
module path argument is omitted, init will attempt to infer the module path
using import comments in .go files, vendoring tool configuration files (like
Gopkg.lock), and the current directory (if in GOPATH).

If a configuration file for a vendoring tool is present, init will attempt to
import module requirements from it.

See https://golang.org/ref/mod#go-mod-init for more about 'go mod init'.
`,Run:runInit,));

private static void init() {
    @base.AddModCommonFlags(_addr_cmdInit.Flag);
}

private static void runInit(context.Context ctx, ptr<base.Command> _addr_cmd, slice<@string> args) {
    ref base.Command cmd = ref _addr_cmd.val;

    if (len(args) > 1) {
        @base.Fatalf("go mod init: too many arguments");
    }
    @string modPath = default;
    if (len(args) == 1) {
        modPath = args[0];
    }
    modload.ForceUseModules = true;
    modload.CreateModFile(ctx, modPath); // does all the hard work
}

} // end modcmd_package
