// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modcmd -- go2cs converted at 2022 March 06 23:19:39 UTC
// import "cmd/go/internal/modcmd" ==> using modcmd = go.cmd.go.@internal.modcmd_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modcmd\why.go
using context = go.context_package;
using fmt = go.fmt_package;
using strings = go.strings_package;

using @base = go.cmd.go.@internal.@base_package;
using imports = go.cmd.go.@internal.imports_package;
using modload = go.cmd.go.@internal.modload_package;

using module = go.golang.org.x.mod.module_package;

namespace go.cmd.go.@internal;

public static partial class modcmd_package {

private static ptr<base.Command> cmdWhy = addr(new base.Command(UsageLine:"go mod why [-m] [-vendor] packages...",Short:"explain why packages or modules are needed",Long:`
Why shows a shortest path in the import graph from the main module to
each of the listed packages. If the -m flag is given, why treats the
arguments as a list of modules and finds a path to any package in each
of the modules.

By default, why queries the graph of packages matched by "go list all",
which includes tests for reachable packages. The -vendor flag causes why
to exclude tests of dependencies.

The output is a sequence of stanzas, one for each package or module
name on the command line, separated by blank lines. Each stanza begins
with a comment line "# package" or "# module" giving the target
package or module. Subsequent lines give a path through the import
graph, one package per line. If the package or module is not
referenced from the main module, the stanza will display a single
parenthesized note indicating that fact.

For example:

	$ go mod why golang.org/x/text/language golang.org/x/text/encoding
	# golang.org/x/text/language
	rsc.io/quote
	rsc.io/sampler
	golang.org/x/text/language

	# golang.org/x/text/encoding
	(main module does not need package golang.org/x/text/encoding)
	$

See https://golang.org/ref/mod#go-mod-why for more about 'go mod why'.
	`,));

private static var whyM = cmdWhy.Flag.Bool("m", false, "");private static var whyVendor = cmdWhy.Flag.Bool("vendor", false, "");

private static void init() {
    cmdWhy.Run = runWhy; // break init cycle
    @base.AddModCommonFlags(_addr_cmdWhy.Flag);

}

private static void runWhy(context.Context ctx, ptr<base.Command> _addr_cmd, slice<@string> args) {
    ref base.Command cmd = ref _addr_cmd.val;

    modload.ForceUseModules = true;
    modload.RootMode = modload.NeedRoot;

    modload.PackageOpts loadOpts = new modload.PackageOpts(Tags:imports.AnyTags(),VendorModulesInGOROOTSrc:true,LoadTests:!*whyVendor,SilencePackageErrors:true,UseVendorAll:*whyVendor,);

    if (whyM.val) {
        foreach (var (_, arg) in args) {
            if (strings.Contains(arg, "@")) {
                @base.Fatalf("go mod why: module query not allowed");
            }
        }
    else
        var (mods, err) = modload.ListModules(ctx, args, 0);
        if (err != null) {
            @base.Fatalf("go mod why: %v", err);
        }
        var byModule = make_map<module.Version, slice<@string>>();
        var (_, pkgs) = modload.LoadPackages(ctx, loadOpts, "all");
        {
            var path__prev1 = path;

            foreach (var (_, __path) in pkgs) {
                path = __path;
                var m = modload.PackageModule(path);
                if (m.Path != "") {
                    byModule[m] = append(byModule[m], path);
                }
            }

            path = path__prev1;
        }

        @string sep = "";
        {
            var m__prev1 = m;

            foreach (var (_, __m) in mods) {
                m = __m;
                @string best = "";
                nint bestDepth = 1000000000;
                {
                    var path__prev2 = path;

                    foreach (var (_, __path) in byModule[new module.Version(Path:m.Path,Version:m.Version)]) {
                        path = __path;
                        var d = modload.WhyDepth(path);
                        if (d > 0 && d < bestDepth) {
                            best = path;
                            bestDepth = d;
                        }
                    }

                    path = path__prev2;
                }

                var why = modload.Why(best);
                if (why == "") {
                    @string vendoring = "";
                    if (whyVendor.val) {
                        vendoring = " to vendor";
                    }
                    why = "(main module does not need" + vendoring + " module " + m.Path + ")\n";
                }

                fmt.Printf("%s# %s\n%s", sep, m.Path, why);
                sep = "\n";

            }

            m = m__prev1;
        }
    } { 
        // Resolve to packages.
        var (matches, _) = modload.LoadPackages(ctx, loadOpts, args);

        modload.LoadPackages(ctx, loadOpts, "all"); // rebuild graph, from main module (not from named packages)

        sep = "";
        {
            var m__prev1 = m;

            foreach (var (_, __m) in matches) {
                m = __m;
                {
                    var path__prev2 = path;

                    foreach (var (_, __path) in m.Pkgs) {
                        path = __path;
                        why = modload.Why(path);
                        if (why == "") {
                            vendoring = "";
                            if (whyVendor.val) {
                                vendoring = " to vendor";
                            }
                            why = "(main module does not need" + vendoring + " package " + path + ")\n";
                        }
                        fmt.Printf("%s# %s\n%s", sep, path, why);
                        sep = "\n";
                    }

                    path = path__prev2;
                }
            }

            m = m__prev1;
        }
    }
}

} // end modcmd_package
