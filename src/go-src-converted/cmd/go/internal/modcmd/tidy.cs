// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// go mod tidy

// package modcmd -- go2cs converted at 2020 October 08 04:36:45 UTC
// import "cmd/go/internal/modcmd" ==> using modcmd = go.cmd.go.@internal.modcmd_package
// Original source: C:\Go\src\cmd\go\internal\modcmd\tidy.go
using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using modfetch = go.cmd.go.@internal.modfetch_package;
using modload = go.cmd.go.@internal.modload_package;
using work = go.cmd.go.@internal.work_package;

using module = go.golang.org.x.mod.module_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class modcmd_package
    {
        private static ptr<base.Command> cmdTidy = addr(new base.Command(UsageLine:"go mod tidy [-v]",Short:"add missing and remove unused modules",Long:`
Tidy makes sure go.mod matches the source code in the module.
It adds any missing modules necessary to build the current module's
packages and dependencies, and it removes unused modules that
don't provide any relevant packages. It also adds any missing entries
to go.sum and removes any unnecessary ones.

The -v flag causes tidy to print information about removed modules
to standard error.
	`,));

        private static void init()
        {
            cmdTidy.Run = runTidy; // break init cycle
            cmdTidy.Flag.BoolVar(_addr_cfg.BuildV, "v", false, "");
            work.AddModCommonFlags(cmdTidy);

        }

        private static void runTidy(ptr<base.Command> _addr_cmd, slice<@string> args)
        {
            ref base.Command cmd = ref _addr_cmd.val;

            if (len(args) > 0L)
            {
                @base.Fatalf("go mod tidy: no arguments allowed");
            }

            modload.LoadALL();
            modload.TidyBuildList();
            modTidyGoSum(); // updates memory copy; WriteGoMod on next line flushes it out
            modload.WriteGoMod();

        }

        // modTidyGoSum resets the go.sum file content
        // to be exactly what's needed for the current go.mod.
        private static void modTidyGoSum()
        { 
            // Assuming go.sum already has at least enough from the successful load,
            // we only have to tell modfetch what needs keeping.
            var reqs = modload.Reqs();
            var keep = make_map<module.Version, bool>();
            var replaced = make_map<module.Version, bool>();
            Action<module.Version> walk = default;
            walk = m =>
            { 
                // If we build using a replacement module, keep the sum for the replacement,
                // since that's the code we'll actually use during a build.
                //
                // TODO(golang.org/issue/29182): Perhaps we should keep both sums, and the
                // sums for both sets of transitive requirements.
                var r = modload.Replacement(m);
                if (r.Path == "")
                {
                    keep[m] = true;
                }
                else
                {
                    keep[r] = true;
                    replaced[m] = true;
                }

                var (list, _) = reqs.Required(m);
                {
                    var r__prev1 = r;

                    foreach (var (_, __r) in list)
                    {
                        r = __r;
                        if (!keep[r] && !replaced[r])
                        {
                            walk(r);
                        }

                    }

                    r = r__prev1;
                }
            }
;
            walk(modload.Target);
            modfetch.TrimGoSum(keep);

        }
    }
}}}}
