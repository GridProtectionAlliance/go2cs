// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// go mod graph

// package modcmd -- go2cs converted at 2022 March 13 06:32:24 UTC
// import "cmd/go/internal/modcmd" ==> using modcmd = go.cmd.go.@internal.modcmd_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modcmd\graph.go
namespace go.cmd.go.@internal;

using bufio = bufio_package;
using context = context_package;
using os = os_package;

using @base = cmd.go.@internal.@base_package;
using modload = cmd.go.@internal.modload_package;

using module = golang.org.x.mod.module_package;
using System;

public static partial class modcmd_package {

private static ptr<base.Command> cmdGraph = addr(new base.Command(UsageLine:"go mod graph [-go=version]",Short:"print module requirement graph",Long:`
Graph prints the module requirement graph (with replacements applied)
in text form. Each line in the output has two space-separated fields: a module
and one of its requirements. Each module is identified as a string of the form
path@version, except for the main module, which has no @version suffix.

The -go flag causes graph to report the module graph as loaded by the
given Go version, instead of the version indicated by the 'go' directive
in the go.mod file.

See https://golang.org/ref/mod#go-mod-graph for more about 'go mod graph'.
	`,Run:runGraph,));

private static goVersionFlag graphGo = default;

private static void init() {
    cmdGraph.Flag.Var(_addr_graphGo, "go", "");
    @base.AddModCommonFlags(_addr_cmdGraph.Flag);
}

private static void runGraph(context.Context ctx, ptr<base.Command> _addr_cmd, slice<@string> args) => func((defer, _, _) => {
    ref base.Command cmd = ref _addr_cmd.val;

    if (len(args) > 0) {
        @base.Fatalf("go mod graph: graph takes no arguments");
    }
    modload.ForceUseModules = true;
    modload.RootMode = modload.NeedRoot;
    var mg = modload.LoadModGraph(ctx, graphGo.String());

    var w = bufio.NewWriter(os.Stdout);
    defer(w.Flush());

    Action<module.Version> format = m => {
        w.WriteString(m.Path);
        if (m.Version != "") {
            w.WriteString("@");
            w.WriteString(m.Version);
        }
    };

    mg.WalkBreadthFirst(m => {
        var (reqs, _) = mg.RequiredBy(m);
        foreach (var (_, r) in reqs) {
            format(m);
            w.WriteByte(' ');
            format(r);
            w.WriteByte('\n');
        }
    });
});

} // end modcmd_package
