// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// go mod graph

// package modcmd -- go2cs converted at 2020 October 09 05:47:52 UTC
// import "cmd/go/internal/modcmd" ==> using modcmd = go.cmd.go.@internal.modcmd_package
// Original source: C:\Go\src\cmd\go\internal\modcmd\graph.go
using bufio = go.bufio_package;
using os = go.os_package;
using sort = go.sort_package;

using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using modload = go.cmd.go.@internal.modload_package;
using par = go.cmd.go.@internal.par_package;
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
        private static ptr<base.Command> cmdGraph = addr(new base.Command(UsageLine:"go mod graph",Short:"print module requirement graph",Long:`
Graph prints the module requirement graph (with replacements applied)
in text form. Each line in the output has two space-separated fields: a module
and one of its requirements. Each module is identified as a string of the form
path@version, except for the main module, which has no @version suffix.
	`,Run:runGraph,));

        private static void init()
        {
            work.AddModCommonFlags(cmdGraph);
        }

        private static void runGraph(ptr<base.Command> _addr_cmd, slice<@string> args)
        {
            ref base.Command cmd = ref _addr_cmd.val;

            if (len(args) > 0L)
            {
                @base.Fatalf("go mod graph: graph takes no arguments");
            } 
            // Checks go mod expected behavior
            if (!modload.Enabled())
            {
                if (cfg.Getenv("GO111MODULE") == "off")
                {
                    @base.Fatalf("go: modules disabled by GO111MODULE=off; see 'go help modules'");
                }
                else
                {
                    @base.Fatalf("go: cannot find main module; see 'go help modules'");
                }

            }

            modload.LoadBuildList();

            var reqs = modload.MinReqs();
            Func<module.Version, @string> format = m =>
            {
                if (m.Version == "")
                {
                    return m.Path;
                }

                return m.Path + "@" + m.Version;

            } 

            // Note: using par.Work only to manage work queue.
            // No parallelism here, so no locking.
; 

            // Note: using par.Work only to manage work queue.
            // No parallelism here, so no locking.
            slice<@string> @out = default;
            long deps = default; // index in out where deps start
            par.Work work = default;
            work.Add(modload.Target);
            work.Do(1L, item =>
            {
                module.Version m = item._<module.Version>();
                var (list, _) = reqs.Required(m);
                foreach (var (_, r) in list)
                {
                    work.Add(r);
                    out = append(out, format(m) + " " + format(r) + "\n");
                }
                if (m == modload.Target)
                {
                    deps = len(out);
                }

            });

            sort.Slice(out[deps..], (i, j) =>
            {
                return out[deps + i][0L] < out[deps + j][0L];
            });

            var w = bufio.NewWriter(os.Stdout);
            foreach (var (_, line) in out)
            {
                w.WriteString(line);
            }
            w.Flush();

        }
    }
}}}}
