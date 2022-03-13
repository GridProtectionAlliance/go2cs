// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// go mod tidy

// package modcmd -- go2cs converted at 2022 March 13 06:32:25 UTC
// import "cmd/go/internal/modcmd" ==> using modcmd = go.cmd.go.@internal.modcmd_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modcmd\tidy.go
namespace go.cmd.go.@internal;

using @base = cmd.go.@internal.@base_package;
using cfg = cmd.go.@internal.cfg_package;
using imports = cmd.go.@internal.imports_package;
using modload = cmd.go.@internal.modload_package;
using context = context_package;
using fmt = fmt_package;

using modfile = golang.org.x.mod.modfile_package;
using semver = golang.org.x.mod.semver_package;

public static partial class modcmd_package {

private static ptr<base.Command> cmdTidy = addr(new base.Command(UsageLine:"go mod tidy [-e] [-v] [-go=version] [-compat=version]",Short:"add missing and remove unused modules",Long:`
Tidy makes sure go.mod matches the source code in the module.
It adds any missing modules necessary to build the current module's
packages and dependencies, and it removes unused modules that
don't provide any relevant packages. It also adds any missing entries
to go.sum and removes any unnecessary ones.

The -v flag causes tidy to print information about removed modules
to standard error.

The -e flag causes tidy to attempt to proceed despite errors
encountered while loading packages.

The -go flag causes tidy to update the 'go' directive in the go.mod
file to the given version, which may change which module dependencies
are retained as explicit requirements in the go.mod file.
(Go versions 1.17 and higher retain more requirements in order to
support lazy module loading.)

The -compat flag preserves any additional checksums needed for the
'go' command from the indicated major Go release to successfully load
the module graph, and causes tidy to error out if that version of the
'go' command would load any imported package from a different module
version. By default, tidy acts as if the -compat flag were set to the
version prior to the one indicated by the 'go' directive in the go.mod
file.

See https://golang.org/ref/mod#go-mod-tidy for more about 'go mod tidy'.
	`,Run:runTidy,));

private static bool tidyE = default;private static goVersionFlag tidyGo = default;private static goVersionFlag tidyCompat = default;

private static void init() {
    cmdTidy.Flag.BoolVar(_addr_cfg.BuildV, "v", false, "");
    cmdTidy.Flag.BoolVar(_addr_tidyE, "e", false, "");
    cmdTidy.Flag.Var(_addr_tidyGo, "go", "");
    cmdTidy.Flag.Var(_addr_tidyCompat, "compat", "");
    @base.AddModCommonFlags(_addr_cmdTidy.Flag);
}

// A goVersionFlag is a flag.Value representing a supported Go version.
//
// (Note that the -go argument to 'go mod edit' is *not* a goVersionFlag.
// It intentionally allows newer-than-supported versions as arguments.)
private partial struct goVersionFlag {
    public @string v;
}

private static @string String(this ptr<goVersionFlag> _addr_f) {
    ref goVersionFlag f = ref _addr_f.val;

    return f.v;
}
private static void Get(this ptr<goVersionFlag> _addr_f) {
    ref goVersionFlag f = ref _addr_f.val;

    return f.v;
}

private static error Set(this ptr<goVersionFlag> _addr_f, @string s) {
    ref goVersionFlag f = ref _addr_f.val;

    if (s != "") {
        var latest = modload.LatestGoVersion();
        if (!modfile.GoVersionRE.MatchString(s)) {
            return error.As(fmt.Errorf("expecting a Go version like %q", latest))!;
        }
        if (semver.Compare("v" + s, "v" + latest) > 0) {
            return error.As(fmt.Errorf("maximum supported Go version is %s", latest))!;
        }
    }
    f.v = s;
    return error.As(null!)!;
}

private static void runTidy(context.Context ctx, ptr<base.Command> _addr_cmd, slice<@string> args) {
    ref base.Command cmd = ref _addr_cmd.val;

    if (len(args) > 0) {
        @base.Fatalf("go mod tidy: no arguments allowed");
    }
    modload.ForceUseModules = true;
    modload.RootMode = modload.NeedRoot;

    modload.LoadPackages(ctx, new modload.PackageOpts(GoVersion:tidyGo.String(),Tags:imports.AnyTags(),Tidy:true,TidyCompatibleVersion:tidyCompat.String(),VendorModulesInGOROOTSrc:true,ResolveMissingImports:true,LoadTests:true,AllowErrors:tidyE,SilenceMissingStdImports:true,), "all");
}

} // end modcmd_package
