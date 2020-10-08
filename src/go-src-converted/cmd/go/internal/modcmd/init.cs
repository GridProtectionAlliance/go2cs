// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// go mod init

// package modcmd -- go2cs converted at 2020 October 08 04:36:45 UTC
// import "cmd/go/internal/modcmd" ==> using modcmd = go.cmd.go.@internal.modcmd_package
// Original source: C:\Go\src\cmd\go\internal\modcmd\init.go
using @base = go.cmd.go.@internal.@base_package;
using modload = go.cmd.go.@internal.modload_package;
using work = go.cmd.go.@internal.work_package;
using os = go.os_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class modcmd_package
    {
        private static ptr<base.Command> cmdInit = addr(new base.Command(UsageLine:"go mod init [module]",Short:"initialize new module in current directory",Long:`
Init initializes and writes a new go.mod to the current directory,
in effect creating a new module rooted at the current directory.
The file go.mod must not already exist.
If possible, init will guess the module path from import comments
(see 'go help importpath') or from version control configuration.
To override this guess, supply the module path as an argument.
	`,Run:runInit,));

        private static void init()
        {
            work.AddModCommonFlags(cmdInit);
        }

        private static void runInit(ptr<base.Command> _addr_cmd, slice<@string> args)
        {
            ref base.Command cmd = ref _addr_cmd.val;

            modload.CmdModInit = true;
            if (len(args) > 1L)
            {
                @base.Fatalf("go mod init: too many arguments");
            }

            if (len(args) == 1L)
            {
                modload.CmdModModule = args[0L];
            }

            if (os.Getenv("GO111MODULE") == "off")
            {
                @base.Fatalf("go mod init: modules disabled by GO111MODULE=off; see 'go help modules'");
            }

            var modFilePath = modload.ModFilePath();
            {
                var (_, err) = os.Stat(modFilePath);

                if (err == null)
                {
                    @base.Fatalf("go mod init: go.mod already exists");
                }

            }

            if (strings.Contains(modload.CmdModModule, "@"))
            {
                @base.Fatalf("go mod init: module path must not contain '@'");
            }

            modload.InitMod(); // does all the hard work
        }
    }
}}}}
