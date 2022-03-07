// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package modget implements the module-aware ``go get'' command.
// package modget -- go2cs converted at 2022 March 06 23:16:17 UTC
// import "cmd/go/internal/modget" ==> using modget = go.cmd.go.@internal.modget_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modget\get.go
// The arguments to 'go get' are patterns with optional version queries, with
// the version queries defaulting to "upgrade".
//
// The patterns are normally interpreted as package patterns. However, if a
// pattern cannot match a package, it is instead interpreted as a *module*
// pattern. For version queries such as "upgrade" and "patch" that depend on the
// selected version of a module (or of the module containing a package),
// whether a pattern denotes a package or module may change as updates are
// applied (see the example in mod_get_patchmod.txt).
//
// There are a few other ambiguous cases to resolve, too. A package can exist in
// two different modules at the same version: for example, the package
// example.com/foo might be found in module example.com and also in module
// example.com/foo, and those modules may have independent v0.1.0 tags — so the
// input 'example.com/foo@v0.1.0' could syntactically refer to the variant of
// the package loaded from either module! (See mod_get_ambiguous_pkg.txt.)
// If the argument is ambiguous, the user can often disambiguate by specifying
// explicit versions for *all* of the potential module paths involved.

using context = go.context_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using sort = go.sort_package;
using strings = go.strings_package;
using sync = go.sync_package;

using @base = go.cmd.go.@internal.@base_package;
using imports = go.cmd.go.@internal.imports_package;
using load = go.cmd.go.@internal.load_package;
using modfetch = go.cmd.go.@internal.modfetch_package;
using modload = go.cmd.go.@internal.modload_package;
using par = go.cmd.go.@internal.par_package;
using search = go.cmd.go.@internal.search_package;
using work = go.cmd.go.@internal.work_package;

using modfile = go.golang.org.x.mod.modfile_package;
using module = go.golang.org.x.mod.module_package;
using semver = go.golang.org.x.mod.semver_package;
using System;


namespace go.cmd.go.@internal;

public static partial class modget_package {

public static ptr<base.Command> CmdGet = addr(new base.Command(UsageLine:"go get [-d] [-t] [-u] [-v] [build flags] [packages]",Short:"add dependencies to current module and install them",Long:`
Get resolves its command-line arguments to packages at specific module versions,
updates go.mod to require those versions, downloads source code into the
module cache, then builds and installs the named packages.

To add a dependency for a package or upgrade it to its latest version:

	go get example.com/pkg

To upgrade or downgrade a package to a specific version:

	go get example.com/pkg@v1.2.3

To remove a dependency on a module and downgrade modules that require it:

	go get example.com/mod@none

See https://golang.org/ref/mod#go-get for details.

The 'go install' command may be used to build and install packages. When a
version is specified, 'go install' runs in module-aware mode and ignores
the go.mod file in the current directory. For example:

	go install example.com/pkg@v1.2.3
	go install example.com/pkg@latest

See 'go help install' or https://golang.org/ref/mod#go-install for details.

In addition to build flags (listed in 'go help build') 'go get' accepts the
following flags.

The -t flag instructs get to consider modules needed to build tests of
packages specified on the command line.

The -u flag instructs get to update modules providing dependencies
of packages named on the command line to use newer minor or patch
releases when available.

The -u=patch flag (not -u patch) also instructs get to update dependencies,
but changes the default to select patch releases.

When the -t and -u flags are used together, get will update
test dependencies as well.

The -d flag instructs get not to build or install packages. get will only
update go.mod and download source code needed to build packages.

Building and installing packages with get is deprecated. In a future release,
the -d flag will be enabled by default, and 'go get' will be only be used to
adjust dependencies of the current module. To install a package using
dependencies from the current module, use 'go install'. To install a package
ignoring the current module, use 'go install' with an @version suffix like
"@latest" after each argument.

For more about modules, see https://golang.org/ref/mod.

For more about specifying packages, see 'go help packages'.

This text describes the behavior of get using modules to manage source
code and dependencies. If instead the go command is running in GOPATH
mode, the details of get's flags and effects change, as does 'go help get'.
See 'go help gopath-get'.

See also: go build, go install, go clean, go mod.
	`,));

// Note that this help text is a stopgap to make the module-aware get help text
// available even in non-module settings. It should be deleted when the old get
// is deleted. It should NOT be considered to set a precedent of having hierarchical
// help names with dashes.
public static ptr<base.Command> HelpModuleGet = addr(new base.Command(UsageLine:"module-get",Short:"module-aware go get",Long:`
The 'go get' command changes behavior depending on whether the
go command is running in module-aware mode or legacy GOPATH mode.
This help text, accessible as 'go help module-get' even in legacy GOPATH mode,
describes 'go get' as it operates in module-aware mode.

Usage: `+CmdGet.UsageLine+`
`+CmdGet.Long,));

public static ptr<base.Command> HelpVCS = addr(new base.Command(UsageLine:"vcs",Short:"controlling version control with GOVCS",Long:`
The 'go get' command can run version control commands like git
to download imported code. This functionality is critical to the decentralized
Go package ecosystem, in which code can be imported from any server,
but it is also a potential security problem, if a malicious server finds a
way to cause the invoked version control command to run unintended code.

To balance the functionality and security concerns, the 'go get' command
by default will only use git and hg to download code from public servers.
But it will use any known version control system (bzr, fossil, git, hg, svn)
to download code from private servers, defined as those hosting packages
matching the GOPRIVATE variable (see 'go help private'). The rationale behind
allowing only Git and Mercurial is that these two systems have had the most
attention to issues of being run as clients of untrusted servers. In contrast,
Bazaar, Fossil, and Subversion have primarily been used in trusted,
authenticated environments and are not as well scrutinized as attack surfaces.

The version control command restrictions only apply when using direct version
control access to download code. When downloading modules from a proxy,
'go get' uses the proxy protocol instead, which is always permitted.
By default, the 'go get' command uses the Go module mirror (proxy.golang.org)
for public packages and only falls back to version control for private
packages or when the mirror refuses to serve a public package (typically for
legal reasons). Therefore, clients can still access public code served from
Bazaar, Fossil, or Subversion repositories by default, because those downloads
use the Go module mirror, which takes on the security risk of running the
version control commands using a custom sandbox.

The GOVCS variable can be used to change the allowed version control systems
for specific packages (identified by a module or import path).
The GOVCS variable applies when building package in both module-aware mode
and GOPATH mode. When using modules, the patterns match against the module path.
When using GOPATH, the patterns match against the import path corresponding to
the root of the version control repository.

The general form of the GOVCS setting is a comma-separated list of
pattern:vcslist rules. The pattern is a glob pattern that must match
one or more leading elements of the module or import path. The vcslist
is a pipe-separated list of allowed version control commands, or "all"
to allow use of any known command, or "off" to disallow all commands.
Note that if a module matches a pattern with vcslist "off", it may still be
downloaded if the origin server uses the "mod" scheme, which instructs the
go command to download the module using the GOPROXY protocol.
The earliest matching pattern in the list applies, even if later patterns
might also match.

For example, consider:

	GOVCS=github.com:git,evil.com:off,*:git|hg

With this setting, code with a module or import path beginning with
github.com/ can only use git; paths on evil.com cannot use any version
control command, and all other paths (* matches everything) can use
only git or hg.

The special patterns "public" and "private" match public and private
module or import paths. A path is private if it matches the GOPRIVATE
variable; otherwise it is public.

If no rules in the GOVCS variable match a particular module or import path,
the 'go get' command applies its default rule, which can now be summarized
in GOVCS notation as 'public:git|hg,private:all'.

To allow unfettered use of any version control system for any package, use:

	GOVCS=*:all

To disable all use of version control, use:

	GOVCS=*:off

The 'go env -w' command (see 'go help env') can be used to set the GOVCS
variable for future go command invocations.
`,));

private static var getD = CmdGet.Flag.Bool("d", false, "");private static var getF = CmdGet.Flag.Bool("f", false, "");private static var getFix = CmdGet.Flag.Bool("fix", false, "");private static var getM = CmdGet.Flag.Bool("m", false, "");private static var getT = CmdGet.Flag.Bool("t", false, "");private static upgradeFlag getU = default;private static var getInsecure = CmdGet.Flag.Bool("insecure", false, "");

// upgradeFlag is a custom flag.Value for -u.
private partial struct upgradeFlag {
    public @string rawVersion;
    public @string version;
}

private static bool IsBoolFlag(this ptr<upgradeFlag> _addr__p0) {
    ref upgradeFlag _p0 = ref _addr__p0.val;

    return true;
} // allow -u

private static error Set(this ptr<upgradeFlag> _addr_v, @string s) {
    ref upgradeFlag v = ref _addr_v.val;

    if (s == "false") {
        v.version = "";
        v.rawVersion = "";
    }
    else if (s == "true") {
        v.version = "upgrade";
        v.rawVersion = "";
    }
    else
 {
        v.version = s;
        v.rawVersion = s;
    }
    return error.As(null!)!;

}

private static @string String(this ptr<upgradeFlag> _addr_v) {
    ref upgradeFlag v = ref _addr_v.val;

    return "";
}

private static void init() {
    work.AddBuildFlags(CmdGet, work.OmitModFlag);
    CmdGet.Run = runGet; // break init loop
    CmdGet.Flag.Var(_addr_getU, "u", "");

}

private static void runGet(context.Context ctx, ptr<base.Command> _addr_cmd, slice<@string> args) {
    ref base.Command cmd = ref _addr_cmd.val;

    switch (getU.version) {
        case "": 

        case "upgrade": 

        case "patch": 

            break;
        default: 
            @base.Fatalf("go get: unknown upgrade flag -u=%s", getU.rawVersion);
            break;
    }
    if (getF.val) {
        fmt.Fprintf(os.Stderr, "go get: -f flag is a no-op when using modules\n");
    }
    if (getFix.val) {
        fmt.Fprintf(os.Stderr, "go get: -fix flag is a no-op when using modules\n");
    }
    if (getM.val) {
        @base.Fatalf("go get: -m flag is no longer supported; consider -d to skip building packages");
    }
    if (getInsecure.val) {
        @base.Fatalf("go get: -insecure flag is no longer supported; use GOINSECURE instead");
    }
    modload.DisallowWriteGoMod(); 

    // Allow looking up modules for import paths when outside of a module.
    // 'go get' is expected to do this, unlike other commands.
    modload.AllowMissingModuleImports();

    var queries = parseArgs(ctx, args);

    var r = newResolver(ctx, queries);
    r.performLocalQueries(ctx);
    r.performPathQueries(ctx);

    while (true) {
        r.performWildcardQueries(ctx);
        r.performPatternAllQueries(ctx);

        {
            var changed__prev1 = changed;

            var changed = r.resolveQueries(ctx, queries);

            if (changed) { 
                // 'go get' arguments can be (and often are) package patterns rather than
                // (just) modules. A package can be provided by any module with a prefix
                // of its import path, and a wildcard can even match packages in modules
                // with totally different paths. Because of these effects, and because any
                // change to the selected version of a module can bring in entirely new
                // module paths as dependencies, we need to reissue queries whenever we
                // change the build list.
                //
                // The result of any version query for a given module — even "upgrade" or
                // "patch" — is always relative to the build list at the start of
                // the 'go get' command, not an intermediate state, and is therefore
                // dederministic and therefore cachable, and the constraints on the
                // selected version of each module can only narrow as we iterate.
                //
                // "all" is functionally very similar to a wildcard pattern. The set of
                // packages imported by the main module does not change, and the query
                // result for the module containing each such package also does not change
                // (it is always relative to the initial build list, before applying
                // queries). So the only way that the result of an "all" query can change
                // is if some matching package moves from one module in the build list
                // to another, which should not happen very often.
                continue;

            } 

            // When we load imports, we detect the following conditions:
            //
            // - missing transitive depencies that need to be resolved from outside the
            //   current build list (note that these may add new matches for existing
            //   pattern queries!)
            //
            // - transitive dependencies that didn't match any other query,
            //   but need to be upgraded due to the -u flag
            //
            // - ambiguous import errors.
            //   TODO(#27899): Try to resolve ambiguous import errors automatically.

            changed = changed__prev1;

        } 

        // When we load imports, we detect the following conditions:
        //
        // - missing transitive depencies that need to be resolved from outside the
        //   current build list (note that these may add new matches for existing
        //   pattern queries!)
        //
        // - transitive dependencies that didn't match any other query,
        //   but need to be upgraded due to the -u flag
        //
        // - ambiguous import errors.
        //   TODO(#27899): Try to resolve ambiguous import errors automatically.
        var upgrades = r.findAndUpgradeImports(ctx, queries);
        {
            var changed__prev1 = changed;

            changed = r.applyUpgrades(ctx, upgrades);

            if (changed) {
                continue;
            }

            changed = changed__prev1;

        }


        r.findMissingWildcards(ctx);
        {
            var changed__prev1 = changed;

            changed = r.resolveQueries(ctx, r.wildcardQueries);

            if (changed) {
                continue;
            }

            changed = changed__prev1;

        }


        break;

    }

    r.checkWildcardVersions(ctx);

    slice<@string> pkgPatterns = default;
    foreach (var (_, q) in queries) {
        if (q.matchesPackages) {
            pkgPatterns = append(pkgPatterns, q.pattern);
        }
    }    r.checkPackageProblems(ctx, pkgPatterns); 

    // We've already downloaded modules (and identified direct and indirect
    // dependencies) by loading packages in findAndUpgradeImports.
    // So if -d is set, we're done after the module work.
    //
    // Otherwise, we need to build and install the packages matched by
    // command line arguments.
    // Note that 'go get -u' without arguments is equivalent to
    // 'go get -u .', so we'll typically build the package in the current
    // directory.
    if (!getD && len(pkgPatterns) > 0.val) {
        work.BuildInit();

        load.PackageOpts pkgOpts = new load.PackageOpts(ModResolveTests:*getT);
        slice<ptr<load.Package>> pkgs = default;
        {
            var pkg__prev1 = pkg;

            foreach (var (_, __pkg) in load.PackagesAndErrors(ctx, pkgOpts, pkgPatterns)) {
                pkg = __pkg;
                if (pkg.Error != null) {
                    ptr<load.NoGoError> noGo;
                    if (errors.As(pkg.Error.Err, _addr_noGo)) {
                        {
                            var m = modload.PackageModule(pkg.ImportPath);

                            if (m.Path == pkg.ImportPath) { 
                                // pkg is at the root of a module, and doesn't exist with the current
                                // build tags. Probably the user just wanted to change the version of
                                // that module — not also build the package — so suppress the error.
                                // (See https://golang.org/issue/33526.)
                                continue;

                            }

                        }

                    }

                }

                pkgs = append(pkgs, pkg);

            }

            pkg = pkg__prev1;
        }

        load.CheckPackageErrors(pkgs);

        var haveExternalExe = false;
        {
            var pkg__prev1 = pkg;

            foreach (var (_, __pkg) in pkgs) {
                pkg = __pkg;
                if (pkg.Name == "main" && pkg.Module != null && pkg.Module.Path != modload.Target.Path) {
                    haveExternalExe = true;
                    break;
                }
            }

            pkg = pkg__prev1;
        }

        if (haveExternalExe) {
            fmt.Fprint(os.Stderr, "go get: installing executables with 'go get' in module mode is deprecated.");
            @string altMsg = default;
            if (modload.HasModRoot()) {
                altMsg = @"
	To adjust and download dependencies of the current module, use 'go get -d'.
	To install using requirements of the current module, use 'go install'.
	To install ignoring the current module, use 'go install' with a version,
	like 'go install example.com/cmd@latest'.
";
            }
            else
 {
                altMsg = "\n\tUse 'go install pkg@version' instead.\n";
            }

            fmt.Fprint(os.Stderr, altMsg);
            fmt.Fprintf(os.Stderr, "\tFor more information, see https://golang.org/doc/go-get-install-deprecation\n\tor run 'go help get' or 'go help install'.\n");

        }
        work.InstallPackages(ctx, pkgPatterns, pkgs);

    }
    if (!modload.HasModRoot()) {
        return ;
    }
    var oldReqs = reqsFromGoMod(_addr_modload.ModFile());

    modload.AllowWriteGoMod();
    modload.WriteGoMod(ctx);
    modload.DisallowWriteGoMod();

    var newReqs = reqsFromGoMod(_addr_modload.ModFile());
    r.reportChanges(oldReqs, newReqs);

}

// parseArgs parses command-line arguments and reports errors.
//
// The command-line arguments are of the form path@version or simply path, with
// implicit @upgrade. path@none is "downgrade away".
private static slice<ptr<query>> parseArgs(context.Context ctx, slice<@string> rawArgs) => func((defer, _, _) => {
    defer(@base.ExitIfErrors());

    slice<ptr<query>> queries = default;
    foreach (var (_, arg) in search.CleanPatterns(rawArgs)) {
        var (q, err) = newQuery(arg);
        if (err != null) {
            @base.Errorf("go get: %v", err);
            continue;
        }
        if (len(rawArgs) == 0) {
            q.raw = "";
        }
        if (strings.HasSuffix(q.raw, ".go") && q.rawVersion == "") {
            if (!strings.Contains(q.raw, "/")) {
                @base.Errorf("go get %s: arguments must be package or module paths", q.raw);
                continue;
            }
            {
                var (fi, err) = os.Stat(q.raw);

                if (err == null && !fi.IsDir()) {
                    @base.Errorf("go get: %s exists as a file, but 'go get' requires package arguments", q.raw);
                    continue;
                }

            }

        }
        queries = append(queries, q);

    }    return queries;

});

private partial struct resolver {
    public slice<ptr<query>> localQueries; // queries for absolute or relative paths
    public slice<ptr<query>> pathQueries; // package path literal queries in original order
    public slice<ptr<query>> wildcardQueries; // path wildcard queries in original order
    public slice<ptr<query>> patternAllQueries; // queries with the pattern "all"

// Indexed "none" queries. These are also included in the slices above;
// they are indexed here to speed up noneForPath.
    public map<@string, ptr<query>> nonesByPath; // path-literal "@none" queries indexed by path
    public slice<ptr<query>> wildcardNones; // wildcard "@none" queries

// resolvedVersion maps each module path to the version of that module that
// must be selected in the final build list, along with the first query
// that resolved the module to that version (the “reason”).
    public map<@string, versionReason> resolvedVersion;
    public slice<module.Version> buildList;
    public map<@string, @string> buildListVersion; // index of buildList (module path → version)

    public map<@string, @string> initialVersion; // index of the initial build list at the start of 'go get'

    public slice<pathSet> missing; // candidates for missing transitive dependencies

    public ptr<par.Queue> work;
    public par.Cache matchInModuleCache;
}

private partial struct versionReason {
    public @string version;
    public ptr<query> reason;
}

private static ptr<resolver> newResolver(context.Context ctx, slice<ptr<query>> queries) { 
    // LoadModGraph also sets modload.Target, which is needed by various resolver
    // methods.
    const @string defaultGoVersion = "";

    var mg = modload.LoadModGraph(ctx, defaultGoVersion);

    var buildList = mg.BuildList();
    var initialVersion = make_map<@string, @string>(len(buildList));
    foreach (var (_, m) in buildList) {
        initialVersion[m.Path] = m.Version;
    }    ptr<resolver> r = addr(new resolver(work:par.NewQueue(runtime.GOMAXPROCS(0)),resolvedVersion:map[string]versionReason{},buildList:buildList,buildListVersion:initialVersion,initialVersion:initialVersion,nonesByPath:map[string]*query{},));

    foreach (var (_, q) in queries) {
        if (q.pattern == "all") {
            r.patternAllQueries = append(r.patternAllQueries, q);
        }
        else if (q.patternIsLocal) {
            r.localQueries = append(r.localQueries, q);
        }
        else if (q.isWildcard()) {
            r.wildcardQueries = append(r.wildcardQueries, q);
        }
        else
 {
            r.pathQueries = append(r.pathQueries, q);
        }
        if (q.version == "none") { 
            // Index "none" queries to make noneForPath more efficient.
            if (q.isWildcard()) {
                r.wildcardNones = append(r.wildcardNones, q);
            }
            else
 { 
                // All "<path>@none" queries for the same path are identical; we only
                // need to index one copy.
                r.nonesByPath[q.pattern] = q;

            }

        }
    }    return _addr_r!;

}

// initialSelected returns the version of the module with the given path that
// was selected at the start of this 'go get' invocation.
private static @string initialSelected(this ptr<resolver> _addr_r, @string mPath) {
    @string version = default;
    ref resolver r = ref _addr_r.val;

    var (v, ok) = r.initialVersion[mPath];
    if (!ok) {
        return "none";
    }
    return v;

}

// selected returns the version of the module with the given path that is
// selected in the resolver's current build list.
private static @string selected(this ptr<resolver> _addr_r, @string mPath) {
    @string version = default;
    ref resolver r = ref _addr_r.val;

    var (v, ok) = r.buildListVersion[mPath];
    if (!ok) {
        return "none";
    }
    return v;

}

// noneForPath returns a "none" query matching the given module path,
// or found == false if no such query exists.
private static (ptr<query>, bool) noneForPath(this ptr<resolver> _addr_r, @string mPath) {
    ptr<query> nq = default!;
    bool found = default;
    ref resolver r = ref _addr_r.val;

    nq = r.nonesByPath[mPath];

    if (nq != null) {
        return (_addr_nq!, true);
    }
    foreach (var (_, nq) in r.wildcardNones) {
        if (nq.matchesPath(mPath)) {
            return (_addr_nq!, true);
        }
    }    return (_addr_null!, false);

}

// queryModule wraps modload.Query, substituting r.checkAllowedOr to decide
// allowed versions.
private static (module.Version, error) queryModule(this ptr<resolver> _addr_r, context.Context ctx, @string mPath, @string query, Func<@string, @string> selected) {
    module.Version _p0 = default;
    error _p0 = default!;
    ref resolver r = ref _addr_r.val;

    var current = r.initialSelected(mPath);
    var (rev, err) = modload.Query(ctx, mPath, query, current, r.checkAllowedOr(query, selected));
    if (err != null) {
        return (new module.Version(), error.As(err)!);
    }
    return (new module.Version(Path:mPath,Version:rev.Version), error.As(null!)!);

}

// queryPackage wraps modload.QueryPackage, substituting r.checkAllowedOr to
// decide allowed versions.
private static (slice<module.Version>, error) queryPackages(this ptr<resolver> _addr_r, context.Context ctx, @string pattern, @string query, Func<@string, @string> selected) {
    slice<module.Version> pkgMods = default;
    error err = default!;
    ref resolver r = ref _addr_r.val;

    var (results, err) = modload.QueryPackages(ctx, pattern, query, selected, r.checkAllowedOr(query, selected));
    if (len(results) > 0) {
        pkgMods = make_slice<module.Version>(0, len(results));
        foreach (var (_, qr) in results) {
            pkgMods = append(pkgMods, qr.Mod);
        }
    }
    return (pkgMods, error.As(err)!);

}

// queryPattern wraps modload.QueryPattern, substituting r.checkAllowedOr to
// decide allowed versions.
private static (slice<module.Version>, module.Version, error) queryPattern(this ptr<resolver> _addr_r, context.Context ctx, @string pattern, @string query, Func<@string, @string> selected) {
    slice<module.Version> pkgMods = default;
    module.Version mod = default;
    error err = default!;
    ref resolver r = ref _addr_r.val;

    var (results, modOnly, err) = modload.QueryPattern(ctx, pattern, query, selected, r.checkAllowedOr(query, selected));
    if (len(results) > 0) {
        pkgMods = make_slice<module.Version>(0, len(results));
        foreach (var (_, qr) in results) {
            pkgMods = append(pkgMods, qr.Mod);
        }
    }
    if (modOnly != null) {
        mod = modOnly.Mod;
    }
    return (pkgMods, mod, error.As(err)!);

}

// checkAllowedOr is like modload.CheckAllowed, but it always allows the requested
// and current versions (even if they are retracted or otherwise excluded).
private static modload.AllowedFunc checkAllowedOr(this ptr<resolver> _addr_r, @string requested, Func<@string, @string> selected) {
    ref resolver r = ref _addr_r.val;

    return (ctx, m) => {
        if (m.Version == requested) {
            return modload.CheckExclusions(ctx, m);
        }
        if ((requested == "upgrade" || requested == "patch") && m.Version == selected(m.Path)) {
            return null;
        }
        return modload.CheckAllowed(ctx, m);

    };

}

// matchInModule is a caching wrapper around modload.MatchInModule.
private static (slice<@string>, error) matchInModule(this ptr<resolver> _addr_r, context.Context ctx, @string pattern, module.Version m) {
    slice<@string> packages = default;
    error err = default!;
    ref resolver r = ref _addr_r.val;

    private partial struct key {
        public @string pattern;
        public module.Version m;
    }
    private partial struct entry {
        public slice<@string> packages;
        public error err;
    }

    entry e = r.matchInModuleCache.Do(new key(pattern,m), () => {
        var match = modload.MatchInModule(ctx, pattern, m, imports.AnyTags());
        if (len(match.Errs) > 0) {
            return new entry(match.Pkgs,match.Errs[0]);
        }
        return new entry(match.Pkgs,nil);

    })._<entry>();

    return (e.packages, error.As(e.err)!);

}

// queryNone adds a candidate set to q for each module matching q.pattern.
// Each candidate set has only one possible module version: the matched
// module at version "none".
//
// We interpret arguments to 'go get' as packages first, and fall back to
// modules second. However, no module exists at version "none", and therefore no
// package exists at that version either: we know that the argument cannot match
// any packages, and thus it must match modules instead.
private static void queryNone(this ptr<resolver> _addr_r, context.Context ctx, ptr<query> _addr_q) => func((_, panic, _) => {
    ref resolver r = ref _addr_r.val;
    ref query q = ref _addr_q.val;

    if (search.IsMetaPackage(q.pattern)) {
        panic(fmt.Sprintf("internal error: queryNone called with pattern %q", q.pattern));
    }
    if (!q.isWildcard()) {
        q.pathOnce(q.pattern, () => {
            if (modload.HasModRoot() && q.pattern == modload.Target.Path) { 
                // The user has explicitly requested to downgrade their own module to
                // version "none". This is not an entirely unreasonable request: it
                // could plausibly mean “downgrade away everything that depends on any
                // explicit version of the main module”, or “downgrade away the
                // package with the same path as the main module, found in a module
                // with a prefix of the main module's path”.
                //
                // However, neither of those behaviors would be consistent with the
                // plain meaning of the query. To try to reduce confusion, reject the
                // query explicitly.
                return errSet(addr(new modload.QueryMatchesMainModuleError(Pattern:q.pattern,Query:q.version)));

            }

            return new pathSet(mod:module.Version{Path:q.pattern,Version:"none"});

        });

    }
    foreach (var (_, curM) in r.buildList) {
        if (!q.matchesPath(curM.Path)) {
            continue;
        }
        q.pathOnce(curM.Path, () => {
            if (modload.HasModRoot() && curM == modload.Target) {
                return errSet(addr(new modload.QueryMatchesMainModuleError(Pattern:q.pattern,Query:q.version)));
            }
            return new pathSet(mod:module.Version{Path:curM.Path,Version:"none"});
        });

    }
});

private static void performLocalQueries(this ptr<resolver> _addr_r, context.Context ctx) {
    ref resolver r = ref _addr_r.val;

    foreach (var (_, q) in r.localQueries) {
        q.pathOnce(q.pattern, () => {
            @string absDetail = "";
            if (!filepath.IsAbs(q.pattern)) {
                {
                    var (absPath, err) = filepath.Abs(q.pattern);

                    if (err == null) {
                        absDetail = fmt.Sprintf(" (%s)", absPath);
                    }

                }

            } 

            // Absolute paths like C:\foo and relative paths like ../foo... are
            // restricted to matching packages in the main module.
            var pkgPattern = modload.DirImportPath(ctx, q.pattern);
            if (pkgPattern == ".") {
                return errSet(fmt.Errorf("%s%s is not within module rooted at %s", q.pattern, absDetail, modload.ModRoot()));
            }

            var match = modload.MatchInModule(ctx, pkgPattern, modload.Target, imports.AnyTags());
            if (len(match.Errs) > 0) {
                return new pathSet(err:match.Errs[0]);
            }

            if (len(match.Pkgs) == 0) {
                if (q.raw == "" || q.raw == ".") {
                    return errSet(fmt.Errorf("no package in current directory"));
                }
                if (!q.isWildcard()) {
                    return errSet(fmt.Errorf("%s%s is not a package in module rooted at %s", q.pattern, absDetail, modload.ModRoot()));
                }
                search.WarnUnmatched(new slice<ptr<search.Match>>(new ptr<search.Match>[] { match }));
                return new pathSet();
            }

            return new pathSet(pkgMods:[]module.Version{modload.Target});

        });

    }
}

// performWildcardQueries populates the candidates for each query whose pattern
// is a wildcard.
//
// The candidates for a given module path matching (or containing a package
// matching) a wildcard query depend only on the initial build list, but the set
// of modules may be expanded by other queries, so wildcard queries need to be
// re-evaluated whenever a potentially-matching module path is added to the
// build list.
private static void performWildcardQueries(this ptr<resolver> _addr_r, context.Context ctx) {
    ref resolver r = ref _addr_r.val;

    {
        var q__prev1 = q;

        foreach (var (_, __q) in r.wildcardQueries) {
            q = __q;
            var q = q;
            r.work.Add(() => {
                if (q.version == "none") {
                    r.queryNone(ctx, q);
                }
                else
 {
                    r.queryWildcard(ctx, q);
                }

            });

        }
        q = q__prev1;
    }

    r.work.Idle().Receive();

}

// queryWildcard adds a candidate set to q for each module for which:
//     - some version of the module is already in the build list, and
//     - that module exists at some version matching q.version, and
//     - either the module path itself matches q.pattern, or some package within
//       the module at q.version matches q.pattern.
private static void queryWildcard(this ptr<resolver> _addr_r, context.Context ctx, ptr<query> _addr_q) {
    ref resolver r = ref _addr_r.val;
    ref query q = ref _addr_q.val;
 
    // For wildcard patterns, modload.QueryPattern only identifies modules
    // matching the prefix of the path before the wildcard. However, the build
    // list may already contain other modules with matching packages, and we
    // should consider those modules to satisfy the query too.
    // We want to match any packages in existing dependencies, but we only want to
    // resolve new dependencies if nothing else turns up.
    foreach (var (_, curM) in r.buildList) {
        if (!q.canMatchInModule(curM.Path)) {
            continue;
        }
        q.pathOnce(curM.Path, () => {
            {
                var (_, hit) = r.noneForPath(curM.Path);

                if (hit) { 
                    // This module is being removed, so it will no longer be in the build list
                    // (and thus will no longer match the pattern).
                    return new pathSet();

                }

            }


            if (curM.Path == modload.Target.Path && !versionOkForMainModule(q.version)) {
                if (q.matchesPath(curM.Path)) {
                    return errSet(addr(new modload.QueryMatchesMainModuleError(Pattern:q.pattern,Query:q.version,)));
                }
                var (packages, err) = r.matchInModule(ctx, q.pattern, curM);
                if (err != null) {
                    return errSet(err);
                }
                if (len(packages) > 0) {
                    return errSet(addr(new modload.QueryMatchesPackagesInMainModuleError(Pattern:q.pattern,Query:q.version,Packages:packages,)));
                }
                return r.tryWildcard(ctx, q, curM);
            }

            var (m, err) = r.queryModule(ctx, curM.Path, q.version, r.initialSelected);
            if (err != null) {
                if (!isNoSuchModuleVersion(err)) { 
                    // We can't tell whether a matching version exists.
                    return errSet(err);

                } 
                // There is no version of curM.Path matching the query.

                // We haven't checked whether curM contains any matching packages at its
                // currently-selected version, or whether curM.Path itself matches q. If
                // either of those conditions holds, *and* no other query changes the
                // selected version of curM, then we will fail in checkWildcardVersions.
                // (This could be an error, but it's too soon to tell.)
                //
                // However, even then the transitive requirements of some other query
                // may downgrade this module out of the build list entirely, in which
                // case the pattern will no longer include it and it won't be an error.
                //
                // Either way, punt on the query rather than erroring out just yet.
                return new pathSet();

            }

            return r.tryWildcard(ctx, q, m);

        });

    }
}

// tryWildcard returns a pathSet for module m matching query q.
// If m does not actually match q, tryWildcard returns an empty pathSet.
private static pathSet tryWildcard(this ptr<resolver> _addr_r, context.Context ctx, ptr<query> _addr_q, module.Version m) {
    ref resolver r = ref _addr_r.val;
    ref query q = ref _addr_q.val;

    var mMatches = q.matchesPath(m.Path);
    var (packages, err) = r.matchInModule(ctx, q.pattern, m);
    if (err != null) {
        return errSet(err);
    }
    if (len(packages) > 0) {
        return new pathSet(pkgMods:[]module.Version{m});
    }
    if (mMatches) {
        return new pathSet(mod:m);
    }
    return new pathSet();

}

// findMissingWildcards adds a candidate set for each query in r.wildcardQueries
// that has not yet resolved to any version containing packages.
private static void findMissingWildcards(this ptr<resolver> _addr_r, context.Context ctx) {
    ref resolver r = ref _addr_r.val;

    foreach (var (_, q) in r.wildcardQueries) {
        if (q.version == "none" || q.matchesPackages) {
            continue; // q is not “missing”
        }
        r.work.Add(() => {
            q.pathOnce(q.pattern, () => {
                var (pkgMods, mod, err) = r.queryPattern(ctx, q.pattern, q.version, r.initialSelected);
                if (err != null) {
                    if (isNoSuchPackageVersion(err) && len(q.resolved) > 0) { 
                        // q already resolved one or more modules but matches no packages.
                        // That's ok: this pattern is just a module pattern, and we don't
                        // need to add any more modules to satisfy it.
                        return new pathSet();

                    }

                    return errSet(err);

                }

                return new pathSet(pkgMods:pkgMods,mod:mod);

            });

        });

    }    r.work.Idle().Receive();

}

// checkWildcardVersions reports an error if any module in the build list has a
// path (or contains a package) matching a query with a wildcard pattern, but
// has a selected version that does *not* match the query.
private static void checkWildcardVersions(this ptr<resolver> _addr_r, context.Context ctx) => func((defer, _, _) => {
    ref resolver r = ref _addr_r.val;

    defer(@base.ExitIfErrors());

    foreach (var (_, q) in r.wildcardQueries) {
        foreach (var (_, curM) in r.buildList) {
            if (!q.canMatchInModule(curM.Path)) {
                continue;
            }
            if (!q.matchesPath(curM.Path)) {
                var (packages, err) = r.matchInModule(ctx, q.pattern, curM);
                if (len(packages) == 0) {
                    if (err != null) {
                        reportError(q, err);
                    }
                    continue; // curM is not relevant to q.
                }

            }

            var (rev, err) = r.queryModule(ctx, curM.Path, q.version, r.initialSelected);
            if (err != null) {
                reportError(q, err);
                continue;
            }

            if (rev.Version == curM.Version) {
                continue; // curM already matches q.
            }

            if (!q.matchesPath(curM.Path)) {
                module.Version m = new module.Version(Path:curM.Path,Version:rev.Version);
                (packages, err) = r.matchInModule(ctx, q.pattern, m);
                if (err != null) {
                    reportError(q, err);
                    continue;
                }
                if (len(packages) == 0) { 
                    // curM at its original version contains a path matching q.pattern,
                    // but at rev.Version it does not, so (somewhat paradoxically) if
                    // we changed the version of curM it would no longer match the query.
                    var version = m;
                    if (rev.Version != q.version) {
                        version = fmt.Sprintf("%s@%s (%s)", m.Path, q.version, m.Version);
                    }

                    reportError(q, fmt.Errorf("%v matches packages in %v but not %v: specify a different version for module %s", q, curM, version, m.Path));
                    continue;

                }

            } 

            // Since queryModule succeeded and either curM or one of the packages it
            // contains matches q.pattern, we should have either selected the version
            // of curM matching q, or reported a conflict error (and exited).
            // If we're still here and the version doesn't match,
            // something has gone very wrong.
            reportError(q, fmt.Errorf("internal error: selected %v instead of %v", curM, rev.Version));

        }
    }
});

// performPathQueries populates the candidates for each query whose pattern is
// a path literal.
//
// The candidate packages and modules for path literals depend only on the
// initial build list, not the current build list, so we only need to query path
// literals once.
private static void performPathQueries(this ptr<resolver> _addr_r, context.Context ctx) {
    ref resolver r = ref _addr_r.val;

    {
        var q__prev1 = q;

        foreach (var (_, __q) in r.pathQueries) {
            q = __q;
            var q = q;
            r.work.Add(() => {
                if (q.version == "none") {
                    r.queryNone(ctx, q);
                }
                else
 {
                    r.queryPath(ctx, q);
                }

            });

        }
        q = q__prev1;
    }

    r.work.Idle().Receive();

}

// queryPath adds a candidate set to q for the package with path q.pattern.
// The candidate set consists of all modules that could provide q.pattern
// and have a version matching q, plus (if it exists) the module whose path
// is itself q.pattern (at a matching version).
private static void queryPath(this ptr<resolver> _addr_r, context.Context ctx, ptr<query> _addr_q) => func((_, panic, _) => {
    ref resolver r = ref _addr_r.val;
    ref query q = ref _addr_q.val;

    q.pathOnce(q.pattern, () => {
        if (search.IsMetaPackage(q.pattern) || q.isWildcard()) {
            panic(fmt.Sprintf("internal error: queryPath called with pattern %q", q.pattern));
        }
        if (q.version == "none") {
            panic("internal error: queryPath called with version \"none\"");
        }
        if (search.IsStandardImportPath(q.pattern)) {
            module.Version stdOnly = new module.Version();
            var (packages, _) = r.matchInModule(ctx, q.pattern, stdOnly);
            if (len(packages) > 0) {
                if (q.rawVersion != "") {
                    return errSet(fmt.Errorf("can't request explicit version %q of standard library package %s", q.version, q.pattern));
                }
                q.matchesPackages = true;
                return new pathSet(); // No module needed for standard library.
            }

        }
        var (pkgMods, mod, err) = r.queryPattern(ctx, q.pattern, q.version, r.initialSelected);
        if (err != null) {
            return errSet(err);
        }
        return new pathSet(pkgMods:pkgMods,mod:mod);

    });

});

// performPatternAllQueries populates the candidates for each query whose
// pattern is "all".
//
// The candidate modules for a given package in "all" depend only on the initial
// build list, but we cannot follow the dependencies of a given package until we
// know which candidate is selected — and that selection may depend on the
// results of other queries. We need to re-evaluate the "all" queries whenever
// the module for one or more packages in "all" are resolved.
private static void performPatternAllQueries(this ptr<resolver> _addr_r, context.Context ctx) {
    ref resolver r = ref _addr_r.val;

    if (len(r.patternAllQueries) == 0) {
        return ;
    }
    Func<context.Context, @string, module.Version, bool> findPackage = (ctx, path, m) => {
        versionOk = true;
        {
            var q__prev1 = q;

            foreach (var (_, __q) in r.patternAllQueries) {
                q = __q;
                q.pathOnce(path, () => {
                    var (pkgMods, err) = r.queryPackages(ctx, path, q.version, r.initialSelected);
                    if (len(pkgMods) != 1 || pkgMods[0] != m) { 
                        // There are candidates other than m for the given path, so we can't
                        // be certain that m will actually be the module selected to provide
                        // the package. Don't load its dependencies just yet, because they
                        // might no longer be dependencies after we resolve the correct
                        // version.
                        versionOk = false;

                    }

                    return new pathSet(pkgMods:pkgMods,err:err);

                });

            }

            q = q__prev1;
        }

        return versionOk;

    };

    r.loadPackages(ctx, new slice<@string>(new @string[] { "all" }), findPackage); 

    // Since we built up the candidate lists concurrently, they may be in a
    // nondeterministic order. We want 'go get' to be fully deterministic,
    // including in which errors it chooses to report, so sort the candidates
    // into a deterministic-but-arbitrary order.
    {
        var q__prev1 = q;

        foreach (var (_, __q) in r.patternAllQueries) {
            q = __q;
            sort.Slice(q.candidates, (i, j) => {
                return q.candidates[i].path < q.candidates[j].path;
            });
        }
        q = q__prev1;
    }
}

// findAndUpgradeImports returns a pathSet for each package that is not yet
// in the build list but is transitively imported by the packages matching the
// given queries (which must already have been resolved).
//
// If the getU flag ("-u") is set, findAndUpgradeImports also returns a
// pathSet for each module that is not constrained by any other
// command-line argument and has an available matching upgrade.
private static slice<pathSet> findAndUpgradeImports(this ptr<resolver> _addr_r, context.Context ctx, slice<ptr<query>> queries) {
    slice<pathSet> upgrades = default;
    ref resolver r = ref _addr_r.val;

    var patterns = make_slice<@string>(0, len(queries));
    foreach (var (_, q) in queries) {
        if (q.matchesPackages) {
            patterns = append(patterns, q.pattern);
        }
    }    if (len(patterns) == 0) {
        return null;
    }
    sync.Mutex mu = default;

    Func<context.Context, @string, module.Version, bool> findPackage = (ctx, path, m) => {
        @string version = "latest";
        if (m.Path != "") {
            if (getU.version == "") { 
                // The user did not request that we upgrade transitive dependencies.
                return true;

            }

            {
                var (_, ok) = r.resolvedVersion[m.Path];

                if (ok) { 
                    // We cannot upgrade m implicitly because its version is determined by
                    // an explicit pattern argument.
                    return true;

                }

            }

            version = getU.version;

        }
        var (pkgMods, err) = r.queryPackages(ctx, path, version, r.selected);
        foreach (var (_, u) in pkgMods) {
            if (u == m) { 
                // The selected package version is already upgraded appropriately; there
                // is no need to change it.
                return true;

            }

        }        if (err != null) {
            if (isNoSuchPackageVersion(err) || (m.Path == "" && module.CheckPath(path) != null)) { 
                // We can't find the package because it doesn't — or can't — even exist
                // in any module at the latest version. (Note that invalid module paths
                // could in general exist due to replacements, so we at least need to
                // run the query to check those.)
                //
                // There is no version change we can make to fix the package, so leave
                // it unresolved. Either some other query (perhaps a wildcard matching a
                // newly-added dependency for some other missing package) will fill in
                // the gaps, or we will report an error (with a better import stack) in
                // the final LoadPackages call.
                return true;

            }

        }
        mu.Lock();
        upgrades = append(upgrades, new pathSet(path:path,pkgMods:pkgMods,err:err));
        mu.Unlock();
        return false;

    };

    r.loadPackages(ctx, patterns, findPackage); 

    // Since we built up the candidate lists concurrently, they may be in a
    // nondeterministic order. We want 'go get' to be fully deterministic,
    // including in which errors it chooses to report, so sort the candidates
    // into a deterministic-but-arbitrary order.
    sort.Slice(upgrades, (i, j) => {
        return upgrades[i].path < upgrades[j].path;
    });
    return upgrades;

}

// loadPackages loads the packages matching the given patterns, invoking the
// findPackage function for each package that may require a change to the
// build list.
//
// loadPackages invokes the findPackage function for each package loaded from a
// module outside the main module. If the module or version that supplies that
// package needs to be changed due to a query, findPackage may return false
// and the imports of that package will not be loaded.
//
// loadPackages also invokes the findPackage function for each imported package
// that is neither present in the standard library nor in any module in the
// build list.
private static bool loadPackages(this ptr<resolver> _addr_r, context.Context ctx, slice<@string> patterns, Func<context.Context, @string, module.Version, bool> findPackage) {
    bool versionOk = default;
    ref resolver r = ref _addr_r.val;

    modload.PackageOpts opts = new modload.PackageOpts(Tags:imports.AnyTags(),VendorModulesInGOROOTSrc:true,LoadTests:*getT,AssumeRootsImported:true,SilencePackageErrors:true,);

    opts.AllowPackage = (ctx, path, m) => {
        if (m.Path == "" || m == modload.Target) { 
            // Packages in the standard library and main module are already at their
            // latest (and only) available versions.
            return null;

        }
        {
            var ok = findPackage(ctx, path, m);

            if (!ok) {
                return errVersionChange;
            }

        }

        return null;

    };

    var (_, pkgs) = modload.LoadPackages(ctx, opts, patterns);
    {
        var path__prev1 = path;

        foreach (var (_, __path) in pkgs) {
            path = __path;
            const @string parentPath = "";
            const var parentIsStd = false;
            var (_, _, err) = modload.Lookup(parentPath, parentIsStd, path);
            if (err == null) {
                continue;
            }
            if (errors.Is(err, errVersionChange)) { 
                // We already added candidates during loading.
                continue;

            }

            ptr<modload.ImportMissingError> importMissing;            ptr<modload.AmbiguousImportError> ambiguous;
            if (!errors.As(err, _addr_importMissing) && !errors.As(err, _addr_ambiguous)) { 
                // The package, which is a dependency of something we care about, has some
                // problem that we can't resolve with a version change.
                // Leave the error for the final LoadPackages call.
                continue;

            }

            var path = path;
            r.work.Add(() => {
                findPackage(ctx, path, new module.Version());
            });

        }
        path = path__prev1;
    }

    r.work.Idle().Receive();

}

// errVersionChange is a sentinel error indicating that a module's version needs
// to be updated before its dependencies can be loaded.
private static var errVersionChange = errors.New("version change needed");

// resolveQueries resolves candidate sets that are attached to the given
// queries and/or needed to provide the given missing-package dependencies.
//
// resolveQueries starts by resolving one module version from each
// unambiguous pathSet attached to the given queries.
//
// If no unambiguous query results in a change to the build list,
// resolveQueries revisits the ambiguous query candidates and resolves them
// arbitrarily in order to guarantee forward progress.
//
// If all pathSets are resolved without any changes to the build list,
// resolveQueries returns with changed=false.
private static bool resolveQueries(this ptr<resolver> _addr_r, context.Context ctx, slice<ptr<query>> queries) => func((defer, _, _) => {
    bool changed = default;
    ref resolver r = ref _addr_r.val;

    defer(@base.ExitIfErrors()); 

    // Note: this is O(N²) with the number of pathSets in the worst case.
    //
    // We could perhaps get it down to O(N) if we were to index the pathSets
    // by module path, so that we only revisit a given pathSet when the
    // version of some module in its containingPackage list has been determined.
    //
    // However, N tends to be small, and most candidate sets will include only one
    // candidate module (so they will be resolved in the first iteration), so for
    // now we'll stick to the simple O(N²) approach.

    nint resolved = 0;
    while (true) {
        var prevResolved = resolved;

        {
            var q__prev2 = q;

            foreach (var (_, __q) in queries) {
                q = __q;
                var unresolved = q.candidates[..(int)0];

                {
                    var cs__prev3 = cs;

                    foreach (var (_, __cs) in q.candidates) {
                        cs = __cs;
                        if (cs.err != null) {
                            reportError(q, cs.err);
                            resolved++;
                            continue;
                        }
                        var (filtered, isPackage, m, unique) = r.disambiguate(cs);
                        if (!unique) {
                            unresolved = append(unresolved, filtered);
                            continue;
                        }
                        if (m.Path == "") { 
                            // The query is not viable. Choose an arbitrary candidate from
                            // before filtering and “resolve” it to report a conflict.
                            isPackage, m = r.chooseArbitrarily(cs);

                        }

                        if (isPackage) {
                            q.matchesPackages = true;
                        }

                        r.resolve(q, m);
                        resolved++;

                    }

                    cs = cs__prev3;
                }

                q.candidates = unresolved;

            }

            q = q__prev2;
        }

        @base.ExitIfErrors();
        if (resolved == prevResolved) {
            break; // No unambiguous candidate remains.
        }
    }

    if (resolved > 0) {
        changed = r.updateBuildList(ctx, null);

        if (changed) { 
            // The build list has changed, so disregard any remaining ambiguous queries:
            // they might now be determined by requirements in the build list, which we
            // would prefer to use instead of arbitrary versions.
            return true;

        }
    }
    nint resolvedArbitrarily = 0;
    {
        var q__prev1 = q;

        foreach (var (_, __q) in queries) {
            q = __q;
            {
                var cs__prev2 = cs;

                foreach (var (_, __cs) in q.candidates) {
                    cs = __cs;
                    var (isPackage, m) = r.chooseArbitrarily(cs);
                    if (isPackage) {
                        q.matchesPackages = true;
                    }
                    r.resolve(q, m);
                    resolvedArbitrarily++;
                }

                cs = cs__prev2;
            }
        }
        q = q__prev1;
    }

    if (resolvedArbitrarily > 0) {
        changed = r.updateBuildList(ctx, null);
    }
    return changed;

});

// applyUpgrades disambiguates candidate sets that are needed to upgrade (or
// provide) transitive dependencies imported by previously-resolved packages.
//
// applyUpgrades modifies the build list by adding one module version from each
// pathSet in upgrades, then downgrading (or further upgrading) those modules as
// needed to maintain any already-resolved versions of other modules.
// applyUpgrades does not mark the new versions as resolved, so they can still
// be further modified by other queries (such as wildcards).
//
// If all pathSets are resolved without any changes to the build list,
// applyUpgrades returns with changed=false.
private static bool applyUpgrades(this ptr<resolver> _addr_r, context.Context ctx, slice<pathSet> upgrades) => func((defer, _, _) => {
    bool changed = default;
    ref resolver r = ref _addr_r.val;

    defer(@base.ExitIfErrors()); 

    // Arbitrarily add a "latest" version that provides each missing package, but
    // do not mark the version as resolved: we still want to allow the explicit
    // queries to modify the resulting versions.
    slice<module.Version> tentative = default;
    foreach (var (_, cs) in upgrades) {
        if (cs.err != null) {
            @base.Errorf("go get: %v", cs.err);
            continue;
        }
        var (filtered, _, m, unique) = r.disambiguate(cs);
        if (!unique) {
            _, m = r.chooseArbitrarily(filtered);
        }
        if (m.Path == "") { 
            // There is no viable candidate for the missing package.
            // Leave it unresolved.
            continue;

        }
        tentative = append(tentative, m);

    }    @base.ExitIfErrors();

    changed = r.updateBuildList(ctx, tentative);
    return changed;

});

// disambiguate eliminates candidates from cs that conflict with other module
// versions that have already been resolved. If there is only one (unique)
// remaining candidate, disambiguate returns that candidate, along with
// an indication of whether that result interprets cs.path as a package
//
// Note: we're only doing very simple disambiguation here. The goal is to
// reproduce the user's intent, not to find a solution that a human couldn't.
// In the vast majority of cases, we expect only one module per pathSet,
// but we want to give some minimal additional tools so that users can add an
// extra argument or two on the command line to resolve simple ambiguities.
private static (pathSet, bool, module.Version, bool) disambiguate(this ptr<resolver> _addr_r, pathSet cs) => func((_, panic, _) => {
    pathSet filtered = default;
    bool isPackage = default;
    module.Version m = default;
    bool unique = default;
    ref resolver r = ref _addr_r.val;

    if (len(cs.pkgMods) == 0 && cs.mod.Path == "") {
        panic("internal error: resolveIfUnambiguous called with empty pathSet");
    }
    foreach (var (_, m) in cs.pkgMods) {
        {
            var (_, ok) = r.noneForPath(m.Path);

            if (ok) { 
                // A query with version "none" forces the candidate module to version
                // "none", so we cannot use any other version for that module.
                continue;

            }

        }


        if (m.Path == modload.Target.Path) {
            if (m.Version == modload.Target.Version) {
                return (new pathSet(), true, m, true);
            } 
            // The main module can only be set to its own version.
            continue;

        }
        var (vr, ok) = r.resolvedVersion[m.Path];
        if (!ok) { 
            // m is a viable answer to the query, but other answers may also
            // still be viable.
            filtered.pkgMods = append(filtered.pkgMods, m);
            continue;

        }
        if (vr.version != m.Version) { 
            // Some query forces the candidate module to a version other than this
            // one.
            //
            // The command could be something like
            //
            //     go get example.com/foo/bar@none example.com/foo/bar/baz@latest
            //
            // in which case we *cannot* resolve the package from
            // example.com/foo/bar (because it is constrained to version
            // "none") and must fall through to module example.com/foo@latest.
            continue;

        }
        return (new pathSet(), true, m, true);

    }    if (cs.mod.Path != "") {
        (vr, ok) = r.resolvedVersion[cs.mod.Path];
        if (!ok || vr.version == cs.mod.Version) {
            filtered.mod = cs.mod;
        }
    }
    if (len(filtered.pkgMods) == 1 && (filtered.mod.Path == "" || filtered.mod == filtered.pkgMods[0])) { 
        // Exactly one viable module contains the package with the given path
        // (by far the common case), so we can resolve it unambiguously.
        return (new pathSet(), true, filtered.pkgMods[0], true);

    }
    if (len(filtered.pkgMods) == 0) { 
        // All modules that could provide the path as a package conflict with other
        // resolved arguments. If it can refer to a module instead, return that;
        // otherwise, this pathSet cannot be resolved (and we will return the
        // zero module.Version).
        return (new pathSet(), false, filtered.mod, true);

    }
    return (filtered, false, new module.Version(), false);

});

// chooseArbitrarily returns an arbitrary (but deterministic) module version
// from among those in the given set.
//
// chooseArbitrarily prefers module paths that were already in the build list at
// the start of 'go get', prefers modules that provide packages over those that
// do not, and chooses the first module meeting those criteria (so biases toward
// longer paths).
private static (bool, module.Version) chooseArbitrarily(this ptr<resolver> _addr_r, pathSet cs) {
    bool isPackage = default;
    module.Version m = default;
    ref resolver r = ref _addr_r.val;
 
    // Prefer to upgrade some module that was already in the build list.
    foreach (var (_, m) in cs.pkgMods) {
        if (r.initialSelected(m.Path) != "none") {
            return (true, m);
        }
    }    if (len(cs.pkgMods) > 0) {
        return (true, cs.pkgMods[0]);
    }
    return (false, cs.mod);

}

// checkPackageProblems reloads packages for the given patterns and reports
// missing and ambiguous package errors. It also reports retractions and
// deprecations for resolved modules and modules needed to build named packages.
// It also adds a sum for each updated module in the build list if we had one
// before and didn't get one while loading packages.
//
// We skip missing-package errors earlier in the process, since we want to
// resolve pathSets ourselves, but at that point, we don't have enough context
// to log the package-import chains leading to each error.
private static void checkPackageProblems(this ptr<resolver> _addr_r, context.Context ctx, slice<@string> pkgPatterns) => func((defer, _, _) => {
    ref resolver r = ref _addr_r.val;

    defer(@base.ExitIfErrors()); 

    // Gather information about modules we might want to load retractions and
    // deprecations for. Loading this metadata requires at least one version
    // lookup per module, and we don't want to load information that's neither
    // relevant nor actionable.
    private partial struct modFlags { // : nint
    }
    const modFlags resolved = 1 << (int)(iota); // version resolved by 'go get'
    const var named = 0; // explicitly named on command line or provides a named package
    const var hasPkg = 1; // needed to build named packages
    const var direct = 2; // provides a direct dependency of the main module
    var relevantMods = make_map<module.Version, modFlags>();
    foreach (var (path, reason) in r.resolvedVersion) {
        module.Version m = new module.Version(Path:path,Version:reason.version);
        relevantMods[m] |= resolved;
    }    if (len(pkgPatterns) > 0) { 
        // LoadPackages will print errors (since it has more context) but will not
        // exit, since we need to load retractions later.
        modload.PackageOpts pkgOpts = new modload.PackageOpts(VendorModulesInGOROOTSrc:true,LoadTests:*getT,ResolveMissingImports:false,AllowErrors:true,SilenceNoGoErrors:true,);
        var (matches, pkgs) = modload.LoadPackages(ctx, pkgOpts, pkgPatterns);
        {
            module.Version m__prev1 = m;

            foreach (var (_, __m) in matches) {
                m = __m;
                if (len(m.Errs) > 0) {
                    @base.SetExitStatus(1);
                    break;
                }
            }

            m = m__prev1;
        }

        {
            var pkg__prev1 = pkg;

            foreach (var (_, __pkg) in pkgs) {
                pkg = __pkg;
                {
                    var err__prev2 = err;

                    var (dir, _, err) = modload.Lookup("", false, pkg);

                    if (err != null) {
                        if (dir != "" && errors.Is(err, imports.ErrNoGo)) { 
                            // Since dir is non-empty, we must have located source files
                            // associated with either the package or its test — ErrNoGo must
                            // indicate that none of those source files happen to apply in this
                            // configuration. If we are actually building the package (no -d
                            // flag), we will report the problem then; otherwise, assume that the
                            // user is going to build or test this package in some other
                            // configuration and suppress the error.
                            continue;

                        }

                        @base.SetExitStatus(1);
                        {
                            ref var ambiguousErr = ref heap((modload.AmbiguousImportError.val)(null), out ptr<var> _addr_ambiguousErr);

                            if (errors.As(err, _addr_ambiguousErr)) {
                                {
                                    module.Version m__prev2 = m;

                                    foreach (var (_, __m) in ambiguousErr.Modules) {
                                        m = __m;
                                        relevantMods[m] |= hasPkg;
                                    }

                                    m = m__prev2;
                                }
                            }

                        }

                    }

                    err = err__prev2;

                }

                {
                    module.Version m__prev2 = m;

                    m = modload.PackageModule(pkg);

                    if (m.Path != "") {
                        relevantMods[m] |= hasPkg;
                    }

                    m = m__prev2;

                }

            }

            pkg = pkg__prev1;
        }

        foreach (var (_, match) in matches) {
            {
                var pkg__prev2 = pkg;

                foreach (var (_, __pkg) in match.Pkgs) {
                    pkg = __pkg;
                    m = modload.PackageModule(pkg);
                    relevantMods[m] |= named;
                }

                pkg = pkg__prev2;
            }
        }
    }
    var reqs = modload.LoadModFile(ctx);
    {
        module.Version m__prev1 = m;

        foreach (var (__m) in relevantMods) {
            m = __m;
            if (reqs.IsDirect(m.Path)) {
                relevantMods[m] |= direct;
            }
        }
        m = m__prev1;
    }

    private partial struct modMessage {
        public module.Version m;
        public @string message;
    }
    var retractions = make_slice<modMessage>(0, len(relevantMods));
    {
        module.Version m__prev1 = m;
        var flags__prev1 = flags;

        foreach (var (__m, __flags) in relevantMods) {
            m = __m;
            flags = __flags;
            if (flags & (resolved | named | hasPkg) != 0) {
                retractions = append(retractions, new modMessage(m:m));
            }
        }
        m = m__prev1;
        flags = flags__prev1;
    }

    sort.Slice(retractions, (i, j) => retractions[i].m.Path < retractions[j].m.Path);
    {
        var i__prev1 = i;

        foreach (var (__i) in retractions) {
            i = __i;
            var i = i;
            r.work.Add(() => {
                var err = modload.CheckRetractions(ctx, retractions[i].m);
                {
                    ref var retractErr = ref heap((modload.ModuleRetractedError.val)(null), out ptr<var> _addr_retractErr);

                    if (errors.As(err, _addr_retractErr)) {
                        retractions[i].message = err.Error();
                    }

                }

            });

        }
        i = i__prev1;
    }

    var deprecations = make_slice<modMessage>(0, len(relevantMods));
    {
        module.Version m__prev1 = m;
        var flags__prev1 = flags;

        foreach (var (__m, __flags) in relevantMods) {
            m = __m;
            flags = __flags;
            if (flags & (resolved | named) != 0 || flags & (hasPkg | direct) == hasPkg | direct) {
                deprecations = append(deprecations, new modMessage(m:m));
            }
        }
        m = m__prev1;
        flags = flags__prev1;
    }

    sort.Slice(deprecations, (i, j) => deprecations[i].m.Path < deprecations[j].m.Path);
    {
        var i__prev1 = i;

        foreach (var (__i) in deprecations) {
            i = __i;
            i = i;
            r.work.Add(() => {
                var (deprecation, err) = modload.CheckDeprecation(ctx, deprecations[i].m);
                if (err != null || deprecation == "") {
                    return ;
                }
                deprecations[i].message = modload.ShortMessage(deprecation, "");
            });
        }
        i = i__prev1;
    }

    var sumErrs = make_slice<error>(len(r.buildList));
    {
        var i__prev1 = i;

        foreach (var (__i) in r.buildList) {
            i = __i;
            i = i;
            m = r.buildList[i];
            var mActual = m;
            {
                var mRepl = modload.Replacement(m);

                if (mRepl.Path != "") {
                    mActual = mRepl;
                }

            }

            module.Version old = new module.Version(Path:m.Path,Version:r.initialVersion[m.Path]);
            if (old.Version == "") {
                continue;
            }

            var oldActual = old;
            {
                var oldRepl = modload.Replacement(old);

                if (oldRepl.Path != "") {
                    oldActual = oldRepl;
                }

            }

            if (mActual == oldActual || mActual.Version == "" || !modfetch.HaveSum(oldActual)) {
                continue;
            }

            r.work.Add(() => {
                {
                    var err__prev1 = err;

                    var (_, err) = modfetch.DownloadZip(ctx, mActual);

                    if (err != null) {
                        @string verb = "upgraded";
                        if (semver.Compare(m.Version, old.Version) < 0) {
                            verb = "downgraded";
                        }
                        @string replaced = "";
                        if (mActual != m) {
                            replaced = fmt.Sprintf(" (replaced by %s)", mActual);
                        }
                        err = fmt.Errorf("%s %s %s => %s%s: error finding sum for %s: %v", verb, m.Path, old.Version, m.Version, replaced, mActual, err);
                        sumErrs[i] = err;
                    }

                    err = err__prev1;

                }

            });

        }
        i = i__prev1;
    }

    r.work.Idle().Receive(); 

    // Report deprecations, then retractions, then errors fetching sums.
    // Only errors fetching sums are hard errors.
    {
        var mm__prev1 = mm;

        foreach (var (_, __mm) in deprecations) {
            mm = __mm;
            if (mm.message != "") {
                fmt.Fprintf(os.Stderr, "go: module %s is deprecated: %s\n", mm.m.Path, mm.message);
            }
        }
        mm = mm__prev1;
    }

    @string retractPath = default;
    {
        var mm__prev1 = mm;

        foreach (var (_, __mm) in retractions) {
            mm = __mm;
            if (mm.message != "") {
                fmt.Fprintf(os.Stderr, "go: warning: %v\n", mm.message);
                if (retractPath == "") {
                    retractPath = mm.m.Path;
                }
                else
 {
                    retractPath = "<module>";
                }

            }

        }
        mm = mm__prev1;
    }

    if (retractPath != "") {
        fmt.Fprintf(os.Stderr, "go: to switch to the latest unretracted version, run:\n\tgo get %s@latest\n", retractPath);
    }
    {
        var err__prev1 = err;

        foreach (var (_, __err) in sumErrs) {
            err = __err;
            if (err != null) {
                @base.Errorf("go: %v", err);
            }
        }
        err = err__prev1;
    }

    @base.ExitIfErrors();

});

// reportChanges logs version changes to os.Stderr.
//
// reportChanges only logs changes to modules named on the command line and to
// explicitly required modules in go.mod. Most changes to indirect requirements
// are not relevant to the user and are not logged.
//
// reportChanges should be called after WriteGoMod.
private static void reportChanges(this ptr<resolver> _addr_r, slice<module.Version> oldReqs, slice<module.Version> newReqs) {
    ref resolver r = ref _addr_r.val;

    private partial struct change {
        public @string path;
        public @string old;
        public @string @new;
    }
    var changes = make_map<@string, change>(); 

    // Collect changes in modules matched by command line arguments.
    {
        var path__prev1 = path;

        foreach (var (__path, __reason) in r.resolvedVersion) {
            path = __path;
            reason = __reason;
            var old = r.initialVersion[path];
            var @new = reason.version;
            if (old != new && (old != "" || new != "none")) {
                changes[path] = new change(path,old,new);
            }
        }
        path = path__prev1;
    }

    {
        var req__prev1 = req;

        foreach (var (_, __req) in oldReqs) {
            req = __req;
            var path = req.Path;
            old = req.Version;
            @new = r.buildListVersion[path];
            if (old != new) {
                changes[path] = new change(path,old,new);
            }
        }
        req = req__prev1;
    }

    {
        var req__prev1 = req;

        foreach (var (_, __req) in newReqs) {
            req = __req;
            path = req.Path;
            old = r.initialVersion[path];
            @new = req.Version;
            if (old != new) {
                changes[path] = new change(path,old,new);
            }
        }
        req = req__prev1;
    }

    var sortedChanges = make_slice<change>(0, len(changes));
    {
        var c__prev1 = c;

        foreach (var (_, __c) in changes) {
            c = __c;
            sortedChanges = append(sortedChanges, c);
        }
        c = c__prev1;
    }

    sort.Slice(sortedChanges, (i, j) => {
        return sortedChanges[i].path < sortedChanges[j].path;
    });
    {
        var c__prev1 = c;

        foreach (var (_, __c) in sortedChanges) {
            c = __c;
            if (c.old == "") {
                fmt.Fprintf(os.Stderr, "go get: added %s %s\n", c.path, c.@new);
            }
            else if (c.@new == "none" || c.@new == "") {
                fmt.Fprintf(os.Stderr, "go get: removed %s %s\n", c.path, c.old);
            }
            else if (semver.Compare(c.@new, c.old) > 0) {
                fmt.Fprintf(os.Stderr, "go get: upgraded %s %s => %s\n", c.path, c.old, c.@new);
            }
            else
 {
                fmt.Fprintf(os.Stderr, "go get: downgraded %s %s => %s\n", c.path, c.old, c.@new);
            }

        }
        c = c__prev1;
    }
}

// resolve records that module m must be at its indicated version (which may be
// "none") due to query q. If some other query forces module m to be at a
// different version, resolve reports a conflict error.
private static void resolve(this ptr<resolver> _addr_r, ptr<query> _addr_q, module.Version m) => func((_, panic, _) => {
    ref resolver r = ref _addr_r.val;
    ref query q = ref _addr_q.val;

    if (m.Path == "") {
        panic("internal error: resolving a module.Version with an empty path");
    }
    if (m.Path == modload.Target.Path && m.Version != modload.Target.Version) {
        reportError(q, addr(new modload.QueryMatchesMainModuleError(Pattern:q.pattern,Query:q.version,)));
        return ;
    }
    var (vr, ok) = r.resolvedVersion[m.Path];
    if (ok && vr.version != m.Version) {
        reportConflict(q, m, vr);
        return ;
    }
    r.resolvedVersion[m.Path] = new versionReason(m.Version,q);
    q.resolved = append(q.resolved, m);

});

// updateBuildList updates the module loader's global build list to be
// consistent with r.resolvedVersion, and to include additional modules
// provided that they do not conflict with the resolved versions.
//
// If the additional modules conflict with the resolved versions, they will be
// downgraded to a non-conflicting version (possibly "none").
//
// If the resulting build list is the same as the one resulting from the last
// call to updateBuildList, updateBuildList returns with changed=false.
private static bool updateBuildList(this ptr<resolver> _addr_r, context.Context ctx, slice<module.Version> additions) => func((defer, panic, _) => {
    bool changed = default;
    ref resolver r = ref _addr_r.val;

    defer(@base.ExitIfErrors());

    var resolved = make_slice<module.Version>(0, len(r.resolvedVersion));
    {
        var rv__prev1 = rv;

        foreach (var (__mPath, __rv) in r.resolvedVersion) {
            mPath = __mPath;
            rv = __rv;
            if (mPath != modload.Target.Path) {
                resolved = append(resolved, new module.Version(Path:mPath,Version:rv.version));
            }
        }
        rv = rv__prev1;
    }

    var (changed, err) = modload.EditBuildList(ctx, additions, resolved);
    if (err != null) {
        ptr<modload.ConstraintError> constraint;
        if (!errors.As(err, _addr_constraint)) {
            @base.Errorf("go get: %v", err);
            return false;
        }
        Func<module.Version, @string> reason = m => {
            var (rv, ok) = r.resolvedVersion[m.Path];
            if (!ok) {
                panic(fmt.Sprintf("internal error: can't find reason for requirement on %v", m));
            }
            return rv.reason.ResolvedString(new module.Version(Path:m.Path,Version:rv.version));
        };
        foreach (var (_, c) in constraint.Conflicts) {
            @base.Errorf("go get: %v requires %v, not %v", reason(c.Source), c.Dep, reason(c.Constraint));
        }        return false;

    }
    if (!changed) {
        return false;
    }
    const @string defaultGoVersion = "";

    r.buildList = modload.LoadModGraph(ctx, defaultGoVersion).BuildList();
    r.buildListVersion = make_map<@string, @string>(len(r.buildList));
    foreach (var (_, m) in r.buildList) {
        r.buildListVersion[m.Path] = m.Version;
    }    return true;

});

private static slice<module.Version> reqsFromGoMod(ptr<modfile.File> _addr_f) {
    ref modfile.File f = ref _addr_f.val;

    var reqs = make_slice<module.Version>(len(f.Require));
    foreach (var (i, r) in f.Require) {
        reqs[i] = r.Mod;
    }    return reqs;
}

// isNoSuchModuleVersion reports whether err indicates that the requested module
// does not exist at the requested version, either because the module does not
// exist at all or because it does not include that specific version.
private static bool isNoSuchModuleVersion(error err) {
    ptr<modload.NoMatchingVersionError> noMatch;
    return errors.Is(err, os.ErrNotExist) || errors.As(err, _addr_noMatch);
}

// isNoSuchPackageVersion reports whether err indicates that the requested
// package does not exist at the requested version, either because no module
// that could contain it exists at that version, or because every such module
// that does exist does not actually contain the package.
private static bool isNoSuchPackageVersion(error err) {
    ptr<modload.PackageNotInModuleError> noPackage;
    return isNoSuchModuleVersion(err) || errors.As(err, _addr_noPackage);
}

} // end modget_package
