// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package modget implements the module-aware ``go get'' command.
// package modget -- go2cs converted at 2020 October 08 04:33:45 UTC
// import "cmd/go/internal/modget" ==> using modget = go.cmd.go.@internal.modget_package
// Original source: C:\Go\src\cmd\go\internal\modget\get.go
using errors = go.errors_package;
using fmt = go.fmt_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using sort = go.sort_package;
using strings = go.strings_package;
using sync = go.sync_package;

using @base = go.cmd.go.@internal.@base_package;
using get = go.cmd.go.@internal.get_package;
using imports = go.cmd.go.@internal.imports_package;
using load = go.cmd.go.@internal.load_package;
using modload = go.cmd.go.@internal.modload_package;
using mvs = go.cmd.go.@internal.mvs_package;
using par = go.cmd.go.@internal.par_package;
using search = go.cmd.go.@internal.search_package;
using work = go.cmd.go.@internal.work_package;

using module = go.golang.org.x.mod.module_package;
using semver = go.golang.org.x.mod.semver_package;
using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class modget_package
    {
        public static ptr<base.Command> CmdGet = addr(new base.Command(UsageLine:"go get [-d] [-t] [-u] [-v] [-insecure] [build flags] [packages]",Short:"add dependencies to current module and install them",Long:`
Get resolves and adds dependencies to the current development module
and then builds and installs them.

The first step is to resolve which dependencies to add.

For each named package or package pattern, get must decide which version of
the corresponding module to use. By default, get looks up the latest tagged
release version, such as v0.4.5 or v1.2.3. If there are no tagged release
versions, get looks up the latest tagged pre-release version, such as
v0.0.1-pre1. If there are no tagged versions at all, get looks up the latest
known commit. If the module is not already required at a later version
(for example, a pre-release newer than the latest release), get will use
the version it looked up. Otherwise, get will use the currently
required version.

This default version selection can be overridden by adding an @version
suffix to the package argument, as in 'go get golang.org/x/text@v0.3.0'.
The version may be a prefix: @v1 denotes the latest available version starting
with v1. See 'go help modules' under the heading 'Module queries' for the
full query syntax.

For modules stored in source control repositories, the version suffix can
also be a commit hash, branch identifier, or other syntax known to the
source control system, as in 'go get golang.org/x/text@master'. Note that
branches with names that overlap with other module query syntax cannot be
selected explicitly. For example, the suffix @v2 means the latest version
starting with v2, not the branch named v2.

If a module under consideration is already a dependency of the current
development module, then get will update the required version.
Specifying a version earlier than the current required version is valid and
downgrades the dependency. The version suffix @none indicates that the
dependency should be removed entirely, downgrading or removing modules
depending on it as needed.

The version suffix @latest explicitly requests the latest minor release of the
module named by the given path. The suffix @upgrade is like @latest but
will not downgrade a module if it is already required at a revision or
pre-release version newer than the latest released version. The suffix
@patch requests the latest patch release: the latest released version
with the same major and minor version numbers as the currently required
version. Like @upgrade, @patch will not downgrade a module already required
at a newer version. If the path is not already required, @upgrade and @patch
are equivalent to @latest.

Although get defaults to using the latest version of the module containing
a named package, it does not use the latest version of that module's
dependencies. Instead it prefers to use the specific dependency versions
requested by that module. For example, if the latest A requires module
B v1.2.3, while B v1.2.4 and v1.3.1 are also available, then 'go get A'
will use the latest A but then use B v1.2.3, as requested by A. (If there
are competing requirements for a particular module, then 'go get' resolves
those requirements by taking the maximum requested version.)

The -t flag instructs get to consider modules needed to build tests of
packages specified on the command line.

The -u flag instructs get to update modules providing dependencies
of packages named on the command line to use newer minor or patch
releases when available. Continuing the previous example, 'go get -u A'
will use the latest A with B v1.3.1 (not B v1.2.3). If B requires module C,
but C does not provide any packages needed to build packages in A
(not including tests), then C will not be updated.

The -u=patch flag (not -u patch) also instructs get to update dependencies,
but changes the default to select patch releases.
Continuing the previous example,
'go get -u=patch A@latest' will use the latest A with B v1.2.4 (not B v1.2.3),
while 'go get -u=patch A' will use a patch release of A instead.

When the -t and -u flags are used together, get will update
test dependencies as well.

In general, adding a new dependency may require upgrading
existing dependencies to keep a working build, and 'go get' does
this automatically. Similarly, downgrading one dependency may
require downgrading other dependencies, and 'go get' does
this automatically as well.

The -insecure flag permits fetching from repositories and resolving
custom domains using insecure schemes such as HTTP. Use with caution. The
GOINSECURE environment variable is usually a better alternative, since it
provides control over which modules may be retrieved using an insecure scheme.
See 'go help environment' for details.

The second step is to download (if needed), build, and install
the named packages.

If an argument names a module but not a package (because there is no
Go source code in the module's root directory), then the install step
is skipped for that argument, instead of causing a build failure.
For example 'go get golang.org/x/perf' succeeds even though there
is no code corresponding to that import path.

Note that package patterns are allowed and are expanded after resolving
the module versions. For example, 'go get golang.org/x/perf/cmd/...'
adds the latest golang.org/x/perf and then installs the commands in that
latest version.

The -d flag instructs get to download the source code needed to build
the named packages, including downloading necessary dependencies,
but not to build and install them.

With no package arguments, 'go get' applies to Go package in the
current directory, if any. In particular, 'go get -u' and
'go get -u=patch' update all the dependencies of that package.
With no package arguments and also without -u, 'go get' is not much more
than 'go install', and 'go get -d' not much more than 'go list'.

For more about modules, see 'go help modules'.

For more about specifying packages, see 'go help packages'.

This text describes the behavior of get using modules to manage source
code and dependencies. If instead the go command is running in GOPATH
mode, the details of get's flags and effects change, as does 'go help get'.
See 'go help modules' and 'go help gopath-get'.

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

        private static var getD = CmdGet.Flag.Bool("d", false, "");        private static var getF = CmdGet.Flag.Bool("f", false, "");        private static var getFix = CmdGet.Flag.Bool("fix", false, "");        private static var getM = CmdGet.Flag.Bool("m", false, "");        private static var getT = CmdGet.Flag.Bool("t", false, "");        private static upgradeFlag getU = default;

        // upgradeFlag is a custom flag.Value for -u.
        private partial struct upgradeFlag // : @string
        {
        }

        private static bool IsBoolFlag(this ptr<upgradeFlag> _addr__p0)
        {
            ref upgradeFlag _p0 = ref _addr__p0.val;

            return true;
        } // allow -u

        private static error Set(this ptr<upgradeFlag> _addr_v, @string s)
        {
            ref upgradeFlag v = ref _addr_v.val;

            if (s == "false")
            {
                s = "";
            }

            if (s == "true")
            {
                s = "upgrade";
            }

            v.val = upgradeFlag(s);
            return error.As(null!)!;

        }

        private static @string String(this ptr<upgradeFlag> _addr_v)
        {
            ref upgradeFlag v = ref _addr_v.val;

            return "";
        }

        private static void init()
        {
            work.AddBuildFlags(CmdGet, work.OmitModFlag);
            CmdGet.Run = runGet; // break init loop
            CmdGet.Flag.BoolVar(_addr_get.Insecure, "insecure", get.Insecure, "");
            CmdGet.Flag.Var(_addr_getU, "u", "");

        }

        // A getArg holds a parsed positional argument for go get (path@vers).
        private partial struct getArg
        {
            public @string raw; // path is the part of the argument before "@" (or the whole argument
// if there is no "@"). path specifies the modules or packages to get.
            public @string path; // vers is the part of the argument after "@" or an implied
// "upgrade" or "patch" if there is no "@". vers specifies the
// module version to get.
            public @string vers;
        }

        // querySpec describes a query for a specific module. path may be a
        // module path, package path, or package pattern. vers is a version
        // query string from a command line argument.
        private partial struct querySpec
        {
            public @string path; // vers specifies what version of the module to get.
            public @string vers; // forceModulePath is true if path should be interpreted as a module path.
// If forceModulePath is true, prevM must be set.
            public bool forceModulePath; // prevM is the previous version of the module. prevM is needed
// to determine the minor version number if vers is "patch". It's also
// used to avoid downgrades from prerelease versions newer than
// "latest" and "patch". If prevM is set, forceModulePath must be true.
            public module.Version prevM;
        }

        // query holds the state for a query made for a specific module.
        // After a query is performed, we know the actual module path and
        // version and whether any packages were matched by the query path.
        private partial struct query
        {
            public ref querySpec querySpec => ref querySpec_val; // arg is the command line argument that matched the specified module.
            public @string arg; // m is the module path and version found by the query.
            public module.Version m;
        }

        private static void runGet(ptr<base.Command> _addr_cmd, slice<@string> args)
        {
            ref base.Command cmd = ref _addr_cmd.val;

            switch (getU)
            {
                case "": 

                case "upgrade": 

                case "patch": 
                    break;
                default: 
                    @base.Fatalf("go get: unknown upgrade flag -u=%s", getU);
                    break;
            }
            if (getF.val)
            {
                fmt.Fprintf(os.Stderr, "go get: -f flag is a no-op when using modules\n");
            }

            if (getFix.val)
            {
                fmt.Fprintf(os.Stderr, "go get: -fix flag is a no-op when using modules\n");
            }

            if (getM.val)
            {
                @base.Fatalf("go get: -m flag is no longer supported; consider -d to skip building packages");
            }

            modload.LoadTests = getT.val;

            var buildList = modload.LoadBuildList();
            buildList = buildList.slice(-1, len(buildList), len(buildList)); // copy on append
            var versionByPath = make_map<@string, @string>();
            {
                var m__prev1 = m;

                foreach (var (_, __m) in buildList)
                {
                    m = __m;
                    versionByPath[m.Path] = m.Version;
                } 

                // Do not allow any updating of go.mod until we've applied
                // all the requested changes and checked that the result matches
                // what was requested.

                m = m__prev1;
            }

            modload.DisallowWriteGoMod(); 

            // Allow looking up modules for import paths outside of a module.
            // 'go get' is expected to do this, unlike other commands.
            modload.AllowMissingModuleImports(); 

            // Parse command-line arguments and report errors. The command-line
            // arguments are of the form path@version or simply path, with implicit
            // @upgrade. path@none is "downgrade away".
            slice<getArg> gets = default;
            slice<ptr<query>> queries = default;
            {
                var arg__prev1 = arg;

                foreach (var (_, __arg) in search.CleanPatterns(args))
                {
                    arg = __arg; 
                    // Argument is path or path@vers.
                    var path = arg;
                    @string vers = "";
                    {
                        var i__prev1 = i;

                        var i = strings.Index(arg, "@");

                        if (i >= 0L)
                        {
                            path = arg[..i];
                            vers = arg[i + 1L..];

                        }

                        i = i__prev1;

                    }

                    if (strings.Contains(vers, "@") || arg != path && vers == "")
                    {
                        @base.Errorf("go get %s: invalid module version syntax", arg);
                        continue;
                    } 

                    // Guard against 'go get x.go', a common mistake.
                    // Note that package and module paths may end with '.go', so only print an error
                    // if the argument has no version and either has no slash or refers to an existing file.
                    if (strings.HasSuffix(arg, ".go") && vers == "")
                    {
                        if (!strings.Contains(arg, "/"))
                        {
                            @base.Errorf("go get %s: arguments must be package or module paths", arg);
                            continue;
                        }

                        {
                            var err__prev2 = err;

                            var (fi, err) = os.Stat(arg);

                            if (err == null && !fi.IsDir())
                            {
                                @base.Errorf("go get: %s exists as a file, but 'go get' requires package arguments", arg);
                                continue;
                            }

                            err = err__prev2;

                        }

                    } 

                    // If no version suffix is specified, assume @upgrade.
                    // If -u=patch was specified, assume @patch instead.
                    if (vers == "")
                    {
                        if (getU != "")
                        {
                            vers = string(getU);
                        }
                        else
                        {
                            vers = "upgrade";
                        }

                    }

                    gets = append(gets, new getArg(raw:arg,path:path,vers:vers)); 

                    // Determine the modules that path refers to, and create queries
                    // to lookup modules at target versions before loading packages.
                    // This is an imprecise process, but it helps reduce unnecessary
                    // queries and package loading. It's also necessary for handling
                    // patterns like golang.org/x/tools/..., which can't be expanded
                    // during package loading until they're in the build list.

                    if (filepath.IsAbs(path) || search.IsRelativePath(path)) 
                        // Absolute paths like C:\foo and relative paths like ../foo...
                        // are restricted to matching packages in the main module. If the path
                        // is explicit and contains no wildcards (...), check that it is a
                        // package in the main module. If the path contains wildcards but
                        // matches no packages, we'll warn after package loading.
                        if (!strings.Contains(path, "..."))
                        {
                            var m = search.NewMatch(path);
                            {
                                var pkgPath = modload.DirImportPath(path);

                                if (pkgPath != ".")
                                {
                                    m = modload.TargetPackages(pkgPath);
                                }

                            }

                            if (len(m.Pkgs) == 0L)
                            {
                                {
                                    var err__prev2 = err;

                                    foreach (var (_, __err) in m.Errs)
                                    {
                                        err = __err;
                                        @base.Errorf("go get %s: %v", arg, err);
                                    }

                                    err = err__prev2;
                                }

                                var (abs, err) = filepath.Abs(path);
                                if (err != null)
                                {
                                    abs = path;
                                }

                                @base.Errorf("go get %s: path %s is not a package in module rooted at %s", arg, abs, modload.ModRoot());
                                continue;

                            }

                        }

                        if (path != arg)
                        {
                            @base.Errorf("go get %s: can't request explicit version of path in main module", arg);
                            continue;
                        }

                    else if (strings.Contains(path, "..."))                     else if (path == "all") 
                        // If there is no main module, "all" is not meaningful.
                        if (!modload.HasModRoot())
                        {
                            @base.Errorf("go get %s: cannot match \"all\": working directory is not part of a module", arg);
                        } 
                        // Don't query modules until we load packages. We'll automatically
                        // look up any missing modules.
                    else if (search.IsMetaPackage(path)) 
                        @base.Errorf("go get %s: explicit requirement on standard-library module %s not allowed", path, path);
                        continue;
                    else 
                        // The argument is a package or module path.
                        if (modload.HasModRoot())
                        {
                            {
                                var m__prev2 = m;

                                m = modload.TargetPackages(path);

                                if (len(m.Pkgs) != 0L)
                                { 
                                    // The path is in the main module. Nothing to query.
                                    if (vers != "upgrade" && vers != "patch")
                                    {
                                        @base.Errorf("go get %s: can't request explicit version of path in main module", arg);
                                    }

                                    continue;

                                }

                                m = m__prev2;

                            }

                        }

                        var first = path;
                        {
                            var i__prev1 = i;

                            i = strings.IndexByte(first, '/');

                            if (i >= 0L)
                            {
                                first = path;
                            }

                            i = i__prev1;

                        }

                        if (!strings.Contains(first, "."))
                        { 
                            // The path doesn't have a dot in the first component and cannot be
                            // queried as a module. It may be a package in the standard library,
                            // which is fine, so don't report an error unless we encounter
                            // a problem loading packages below.
                            continue;

                        } 

                        // If we're querying "upgrade" or "patch", we need to know the current
                        // version of the module. For "upgrade", we want to avoid accidentally
                        // downgrading from a newer prerelease. For "patch", we need to query
                        // the correct minor version.
                        // Here, we check if "path" is the name of a module in the build list
                        // (other than the main module) and set prevM if so. If "path" isn't
                        // a module in the build list, the current version doesn't matter
                        // since it's either an unknown module or a package within a module
                        // that we'll discover later.
                        ptr<query> q = addr(new query(querySpec:querySpec{path:path,vers:vers},arg:arg));
                        {
                            var v__prev1 = v;

                            var (v, ok) = versionByPath[path];

                            if (ok && path != modload.Target.Path)
                            {
                                q.prevM = new module.Version(Path:path,Version:v);
                                q.forceModulePath = true;
                            }

                            v = v__prev1;

                        }

                        queries = append(queries, q);
                    
                }

                arg = arg__prev1;
            }

            @base.ExitIfErrors(); 

            // Query modules referenced by command line arguments at requested versions.
            // We need to do this before loading packages since patterns that refer to
            // packages in unknown modules can't be expanded. This also avoids looking
            // up new modules while loading packages, only to downgrade later.
            var queryCache = make_map<querySpec, ptr<query>>();
            var byPath = runQueries(queryCache, queries, null); 

            // Add missing modules to the build list.
            // We call SetBuildList here and elsewhere, since newUpgrader,
            // ImportPathsQuiet, and other functions read the global build list.
            {
                query q__prev1 = q;

                foreach (var (_, __q) in queries)
                {
                    q = __q;
                    {
                        var (_, ok) = versionByPath[q.m.Path];

                        if (!ok && q.m.Version != "none")
                        {
                            buildList = append(buildList, q.m);
                        }

                    }

                }

                q = q__prev1;
            }

            versionByPath = null; // out of date now; rebuilt later when needed
            modload.SetBuildList(buildList); 

            // Upgrade modules specifically named on the command line. This is our only
            // chance to upgrade modules without root packages (modOnly below).
            // This also skips loading packages at an old version, only to upgrade
            // and reload at a new version.
            var upgrade = make_map<@string, ptr<query>>();
            {
                var path__prev1 = path;
                query q__prev1 = q;

                foreach (var (__path, __q) in byPath)
                {
                    path = __path;
                    q = __q;
                    if (q.path == q.m.Path && q.m.Version != "none")
                    {
                        upgrade[path] = q;
                    }

                }

                path = path__prev1;
                q = q__prev1;
            }

            var (buildList, err) = mvs.UpgradeAll(modload.Target, newUpgrader(upgrade, null));
            if (err != null)
            {
                @base.Fatalf("go get: %v", err);
            }

            modload.SetBuildList(buildList);
            @base.ExitIfErrors();
            var prevBuildList = buildList; 

            // Build a set of module paths that we don't plan to load packages from.
            // This includes explicitly requested modules that don't have a root package
            // and modules with a target version of "none".
            sync.WaitGroup wg = default;
            sync.Mutex modOnlyMu = default;
            var modOnly = make_map<@string, ptr<query>>();
            {
                query q__prev1 = q;

                foreach (var (_, __q) in queries)
                {
                    q = __q;
                    if (q.m.Version == "none")
                    {
                        modOnlyMu.Lock();
                        modOnly[q.m.Path] = q;
                        modOnlyMu.Unlock();
                        continue;
                    }

                    if (q.path == q.m.Path)
                    {
                        wg.Add(1L);
                        go_(() => q =>
                        {
                            {
                                var err__prev2 = err;

                                var (hasPkg, err) = modload.ModuleHasRootPackage(q.m);

                                if (err != null)
                                {
                                    @base.Errorf("go get: %v", err);
                                }
                                else if (!hasPkg)
                                {
                                    modOnlyMu.Lock();
                                    modOnly[q.m.Path] = q;
                                    modOnlyMu.Unlock();
                                }


                                err = err__prev2;

                            }

                            wg.Done();

                        }(q));

                    }

                }

                q = q__prev1;
            }

            wg.Wait();
            @base.ExitIfErrors(); 

            // Build a list of arguments that may refer to packages.
            slice<@string> pkgPatterns = default;
            slice<getArg> pkgGets = default;
            {
                var arg__prev1 = arg;

                foreach (var (_, __arg) in gets)
                {
                    arg = __arg;
                    if (modOnly[arg.path] == null && arg.vers != "none")
                    {
                        pkgPatterns = append(pkgPatterns, arg.path);
                        pkgGets = append(pkgGets, arg);
                    }

                } 

                // Load packages and upgrade the modules that provide them. We do this until
                // we reach a fixed point, since modules providing packages may change as we
                // change versions. This must terminate because the module graph is finite,
                // and the load and upgrade operations may only add and upgrade modules
                // in the build list.

                arg = arg__prev1;
            }

            slice<ptr<search.Match>> matches = default;
            while (true)
            {
                map<@string, bool> seenPkgs = default;
                var seenQuery = make_map<querySpec, bool>();
                queries = default;
                Action<ptr<query>> addQuery = q =>
                {
                    if (!seenQuery[q.querySpec])
                    {
                        seenQuery[q.querySpec] = true;
                        queries = append(queries, q);
                    }

                }
;

                if (len(pkgPatterns) > 0L)
                { 
                    // Don't load packages if pkgPatterns is empty. Both
                    // modload.ImportPathsQuiet and ModulePackages convert an empty list
                    // of patterns to []string{"."}, which is not what we want.
                    matches = modload.ImportPathsQuiet(pkgPatterns, imports.AnyTags());
                    seenPkgs = make_map<@string, bool>();
                    {
                        var i__prev2 = i;

                        foreach (var (__i, __match) in matches)
                        {
                            i = __i;
                            match = __match;
                            var arg = pkgGets[i];

                            if (len(match.Pkgs) == 0L)
                            { 
                                // If the pattern did not match any packages, look up a new module.
                                // If the pattern doesn't match anything on the last iteration,
                                // we'll print a warning after the outer loop.
                                if (!match.IsLocal() && !match.IsLiteral() && arg.path != "all")
                                {
                                    addQuery(addr(new query(querySpec:querySpec{path:arg.path,vers:arg.vers},arg:arg.raw)));
                                }
                                else
                                {
                                    {
                                        var err__prev3 = err;

                                        foreach (var (_, __err) in match.Errs)
                                        {
                                            err = __err;
                                            @base.Errorf("go get: %v", err);
                                        }

                                        err = err__prev3;
                                    }
                                }

                                continue;

                            }

                            var allStd = true;
                            foreach (var (_, pkg) in match.Pkgs)
                            {
                                if (!seenPkgs[pkg])
                                {
                                    seenPkgs[pkg] = true;
                                    {
                                        var err__prev3 = err;

                                        var (_, _, err) = modload.Lookup("", false, pkg);

                                        if (err != null)
                                        {
                                            allStd = false;
                                            @base.Errorf("go get %s: %v", arg.raw, err);
                                            continue;
                                        }

                                        err = err__prev3;

                                    }

                                }

                                m = modload.PackageModule(pkg);
                                if (m.Path == "")
                                { 
                                    // pkg is in the standard library.
                                    continue;

                                }

                                allStd = false;
                                if (m.Path == modload.Target.Path)
                                { 
                                    // pkg is in the main module.
                                    continue;

                                }

                                addQuery(addr(new query(querySpec:querySpec{path:m.Path,vers:arg.vers,forceModulePath:true,prevM:m},arg:arg.raw)));

                            }
                            if (allStd && arg.path != arg.raw)
                            {
                                @base.Errorf("go get %s: cannot use pattern %q with explicit version", arg.raw, arg.raw);
                            }

                        }

                        i = i__prev2;
                    }
                }

                @base.ExitIfErrors(); 

                // Query target versions for modules providing packages matched by
                // command line arguments.
                byPath = runQueries(queryCache, queries, modOnly); 

                // Handle upgrades. This is needed for arguments that didn't match
                // modules or matched different modules from a previous iteration. It
                // also upgrades modules providing package dependencies if -u is set.
                (buildList, err) = mvs.UpgradeAll(modload.Target, newUpgrader(byPath, seenPkgs));
                if (err != null)
                {
                    @base.Fatalf("go get: %v", err);
                }

                modload.SetBuildList(buildList);
                @base.ExitIfErrors(); 

                // Stop if no changes have been made to the build list.
                buildList = modload.BuildList();
                var eq = len(buildList) == len(prevBuildList);
                {
                    var i__prev2 = i;

                    for (i = 0L; eq && i < len(buildList); i++)
                    {
                        eq = buildList[i] == prevBuildList[i];
                    }


                    i = i__prev2;
                }
                if (eq)
                {
                    break;
                }

                prevBuildList = buildList;

            }

            if (!getD.val)
            { 
                // Only print warnings after the last iteration,
                // and only if we aren't going to build.
                search.WarnUnmatched(matches);

            } 

            // Handle downgrades.
            slice<module.Version> down = default;
            {
                var m__prev1 = m;

                foreach (var (_, __m) in modload.BuildList())
                {
                    m = __m;
                    q = byPath[m.Path];
                    if (q != null && semver.Compare(m.Version, q.m.Version) > 0L)
                    {
                        down = append(down, new module.Version(Path:m.Path,Version:q.m.Version));
                    }

                }

                m = m__prev1;
            }

            if (len(down) > 0L)
            {
                (buildList, err) = mvs.Downgrade(modload.Target, modload.Reqs(), down);
                if (err != null)
                {
                    @base.Fatalf("go: %v", err);
                }

                modload.SetBuildList(buildList);
                modload.ReloadBuildList(); // note: does not update go.mod
                @base.ExitIfErrors();

            } 

            // Scan for any upgrades lost by the downgrades.
            slice<ptr<query>> lostUpgrades = default;
            if (len(down) > 0L)
            {
                versionByPath = make_map<@string, @string>();
                {
                    var m__prev1 = m;

                    foreach (var (_, __m) in modload.BuildList())
                    {
                        m = __m;
                        versionByPath[m.Path] = m.Version;
                    }

                    m = m__prev1;
                }

                {
                    query q__prev1 = q;

                    foreach (var (_, __q) in byPath)
                    {
                        q = __q;
                        {
                            var v__prev2 = v;

                            (v, ok) = versionByPath[q.m.Path];

                            if (q.m.Version != "none" && (!ok || semver.Compare(v, q.m.Version) != 0L))
                            {
                                lostUpgrades = append(lostUpgrades, q);
                            }

                            v = v__prev2;

                        }

                    }

                    q = q__prev1;
                }

                sort.Slice(lostUpgrades, (i, j) =>
                {
                    return lostUpgrades[i].m.Path < lostUpgrades[j].m.Path;
                });

            }

            if (len(lostUpgrades) > 0L)
            {
                Func<module.Version, @string> desc = m =>
                {
                    var s = m.Path + "@" + m.Version;
                    var t = byPath[m.Path];
                    if (t != null && t.arg != s)
                    {
                        s += " from " + t.arg;
                    }

                    return s;

                }
;
                var downByPath = make_map<@string, module.Version>();
                foreach (var (_, d) in down)
                {
                    downByPath[d.Path] = d;
                }
                ref strings.Builder buf = ref heap(out ptr<strings.Builder> _addr_buf);
                fmt.Fprintf(_addr_buf, "go get: inconsistent versions:");
                var reqs = modload.Reqs();
                {
                    query q__prev1 = q;

                    foreach (var (_, __q) in lostUpgrades)
                    {
                        q = __q; 
                        // We lost q because its build list requires a newer version of something in down.
                        // Figure out exactly what.
                        // Repeatedly constructing the build list is inefficient
                        // if there are MANY command-line arguments,
                        // but at least all the necessary requirement lists are cached at this point.
                        var (list, err) = buildListForLostUpgrade(q.m, reqs);
                        if (err != null)
                        {
                            @base.Fatalf("go: %v", err);
                        }

                        fmt.Fprintf(_addr_buf, "\n\t%s", desc(q.m));
                        @string sep = " requires";
                        {
                            var m__prev2 = m;

                            foreach (var (_, __m) in list)
                            {
                                m = __m;
                                {
                                    slice<module.Version> down__prev2 = down;

                                    var (down, ok) = downByPath[m.Path];

                                    if (ok && semver.Compare(down.Version, m.Version) < 0L)
                                    {
                                        fmt.Fprintf(_addr_buf, "%s %s@%s (not %s)", sep, m.Path, m.Version, desc(down));
                                        sep = ",";
                                    }

                                    down = down__prev2;

                                }

                            }

                            m = m__prev2;
                        }

                        if (sep != ",")
                        { 
                            // We have no idea why this happened.
                            // At least report the problem.
                            {
                                var v__prev3 = v;

                                var v = versionByPath[q.m.Path];

                                if (v == "")
                                {
                                    fmt.Fprintf(_addr_buf, " removed unexpectedly");
                                }
                                else
                                {
                                    fmt.Fprintf(_addr_buf, " ended up at %s unexpectedly", v);
                                }

                                v = v__prev3;

                            }

                            fmt.Fprintf(_addr_buf, " (please report at golang.org/issue/new)");

                        }

                    }

                    q = q__prev1;
                }

                @base.Fatalf("%v", buf.String());

            } 

            // Everything succeeded. Update go.mod.
            modload.AllowWriteGoMod();
            modload.WriteGoMod(); 

            // If -d was specified, we're done after the module work.
            // We've already downloaded modules by loading packages above.
            // Otherwise, we need to build and install the packages matched by
            // command line arguments. This may be a different set of packages,
            // since we only build packages for the target platform.
            // Note that 'go get -u' without arguments is equivalent to
            // 'go get -u .', so we'll typically build the package in the current
            // directory.
            if (getD || len(pkgPatterns) == 0L.val)
            {
                return ;
            }

            work.BuildInit();
            var pkgs = load.PackagesForBuild(pkgPatterns);
            work.InstallPackages(pkgPatterns, pkgs);

        }

        // runQueries looks up modules at target versions in parallel. Results will be
        // cached. If the same module is referenced by multiple queries at different
        // versions (including earlier queries in the modOnly map), an error will be
        // reported. A map from module paths to queries is returned, which includes
        // queries and modOnly.
        private static map<@string, ptr<query>> runQueries(map<querySpec, ptr<query>> cache, slice<ptr<query>> queries, map<@string, ptr<query>> modOnly)
        {
            par.Work lookup = default;
            {
                var q__prev1 = q;

                foreach (var (_, __q) in queries)
                {
                    q = __q;
                    {
                        var cached = cache[q.querySpec];

                        if (cached != null)
                        {
                            q.val = cached.val;
                        }
                        else
                        {
                            cache[q.querySpec] = q;
                            lookup.Add(q);
                        }

                    }

                }

                q = q__prev1;
            }

            lookup.Do(10L, item =>
            {
                ptr<query> q = item._<ptr<query>>();
                if (q.vers == "none")
                { 
                    // Wait for downgrade step.
                    q.m = new module.Version(Path:q.path,Version:"none");
                    return ;

                }

                var (m, err) = getQuery(q.path, q.vers, q.prevM, q.forceModulePath);
                if (err != null)
                {
                    @base.Errorf("go get %s: %v", q.arg, err);
                }

                q.m = m;

            });
            @base.ExitIfErrors();

            var byPath = make_map<@string, ptr<query>>();
            Action<ptr<query>> check = q =>
            {
                {
                    var (prev, ok) = byPath[q.m.Path];

                    if (prev != null && prev.m != q.m)
                    {
                        @base.Errorf("go get: conflicting versions for module %s: %s and %s", q.m.Path, prev.m.Version, q.m.Version);
                        byPath[q.m.Path] = null; // sentinel to stop errors
                        return ;

                    }
                    else if (!ok)
                    {
                        byPath[q.m.Path] = q;
                    }


                }

            }
;
            {
                var q__prev1 = q;

                foreach (var (_, __q) in queries)
                {
                    q = __q;
                    check(q);
                }

                q = q__prev1;
            }

            {
                var q__prev1 = q;

                foreach (var (_, __q) in modOnly)
                {
                    q = __q;
                    check(q);
                }

                q = q__prev1;
            }

            @base.ExitIfErrors();

            return byPath;

        }

        // getQuery evaluates the given (package or module) path and version
        // to determine the underlying module version being requested.
        // If forceModulePath is set, getQuery must interpret path
        // as a module path.
        private static (module.Version, error) getQuery(@string path, @string vers, module.Version prevM, bool forceModulePath)
        {
            module.Version _p0 = default;
            error _p0 = default!;

            if ((prevM.Version != "") != forceModulePath)
            { 
                // We resolve package patterns by calling QueryPattern, which does not
                // accept a previous version and therefore cannot take it into account for
                // the "latest" or "patch" queries.
                // If we are resolving a package path or pattern, the caller has already
                // resolved any existing packages to their containing module(s), and
                // will set both prevM.Version and forceModulePath for those modules.
                // The only remaining package patterns are those that are not already
                // provided by the build list, which are indicated by
                // an empty prevM.Version.
                @base.Fatalf("go get: internal error: prevM may be set if and only if forceModulePath is set");

            } 

            // If the query must be a module path, try only that module path.
            if (forceModulePath)
            {
                if (path == modload.Target.Path)
                {
                    if (vers != "latest")
                    {
                        return (new module.Version(), error.As(fmt.Errorf("can't get a specific version of the main module"))!);
                    }

                }

                var (info, err) = modload.Query(path, vers, prevM.Version, modload.Allowed);
                if (err == null)
                {
                    if (info.Version != vers && info.Version != prevM.Version)
                    {
                        logOncef("go: %s %s => %s", path, vers, info.Version);
                    }

                    return (new module.Version(Path:path,Version:info.Version), error.As(null!)!);

                } 

                // If the query was "upgrade" or "patch" and the current version has been
                // replaced, check to see whether the error was for that same version:
                // if so, the version was probably replaced because it is invalid,
                // and we should keep that replacement without complaining.
                if (vers == "upgrade" || vers == "patch")
                {
                    ptr<module.InvalidVersionError> vErr;
                    if (errors.As(err, _addr_vErr) && vErr.Version == prevM.Version && modload.Replacement(prevM).Path != "")
                    {
                        return (prevM, error.As(null!)!);
                    }

                }

                return (new module.Version(), error.As(err)!);

            } 

            // If the query may be either a package or a module, try it as a package path.
            // If it turns out to only exist as a module, we can detect the resulting
            // PackageNotInModuleError and avoid a second round-trip through (potentially)
            // all of the configured proxies.
            var (results, err) = modload.QueryPattern(path, vers, modload.Allowed);
            if (err != null)
            { 
                // If the path doesn't contain a wildcard, check whether it was actually a
                // module path instead. If so, return that.
                if (!strings.Contains(path, "..."))
                {
                    ptr<modload.PackageNotInModuleError> modErr;
                    if (errors.As(err, _addr_modErr) && modErr.Mod.Path == path)
                    {
                        if (modErr.Mod.Version != vers)
                        {
                            logOncef("go: %s %s => %s", path, vers, modErr.Mod.Version);
                        }

                        return (modErr.Mod, error.As(null!)!);

                    }

                }

                return (new module.Version(), error.As(err)!);

            }

            var m = results[0L].Mod;
            if (m.Path != path)
            {
                logOncef("go: found %s in %s %s", path, m.Path, m.Version);
            }
            else if (m.Version != vers)
            {
                logOncef("go: %s %s => %s", path, vers, m.Version);
            }

            return (m, error.As(null!)!);

        }

        // An upgrader adapts an underlying mvs.Reqs to apply an
        // upgrade policy to a list of targets and their dependencies.
        private partial struct upgrader : mvs.Reqs
        {
            public ref mvs.Reqs Reqs => ref Reqs_val; // cmdline maps a module path to a query made for that module at a
// specific target version. Each query corresponds to a module
// matched by a command line argument.
            public map<@string, ptr<query>> cmdline; // upgrade is a set of modules providing dependencies of packages
// matched by command line arguments. If -u or -u=patch is set,
// these modules are upgraded accordingly.
            public map<@string, bool> upgrade;
        }

        // newUpgrader creates an upgrader. cmdline contains queries made at
        // specific versions for modules matched by command line arguments. pkgs
        // is the set of packages matched by command line arguments. If -u or -u=patch
        // is set, modules providing dependencies of pkgs are upgraded accordingly.
        private static ptr<upgrader> newUpgrader(map<@string, ptr<query>> cmdline, map<@string, bool> pkgs)
        {
            ptr<upgrader> u = addr(new upgrader(Reqs:modload.Reqs(),cmdline:cmdline,));
            if (getU != "")
            {
                u.upgrade = make_map<@string, bool>(); 

                // Traverse package import graph.
                // Initialize work queue with root packages.
                var seen = make_map<@string, bool>();
                slice<@string> work = default;
                Action<@string> add = path =>
                {
                    if (!seen[path])
                    {
                        seen[path] = true;
                        work = append(work, path);
                    }

                }
;
                {
                    var pkg__prev1 = pkg;

                    foreach (var (__pkg) in pkgs)
                    {
                        pkg = __pkg;
                        add(pkg);
                    }

                    pkg = pkg__prev1;
                }

                while (len(work) > 0L)
                {
                    var pkg = work[0L];
                    work = work[1L..];
                    var m = modload.PackageModule(pkg);
                    u.upgrade[m.Path] = true; 

                    // testImports is empty unless test imports were actually loaded,
                    // i.e., -t was set or "all" was one of the arguments.
                    var (imports, testImports) = modload.PackageImports(pkg);
                    {
                        var imp__prev2 = imp;

                        foreach (var (_, __imp) in imports)
                        {
                            imp = __imp;
                            add(imp);
                        }

                        imp = imp__prev2;
                    }

                    {
                        var imp__prev2 = imp;

                        foreach (var (_, __imp) in testImports)
                        {
                            imp = __imp;
                            add(imp);
                        }

                        imp = imp__prev2;
                    }
                }


            }

            return _addr_u!;

        }

        // Required returns the requirement list for m.
        // For the main module, we override requirements with the modules named
        // one the command line, and we include new requirements. Otherwise,
        // we defer to u.Reqs.
        private static (slice<module.Version>, error) Required(this ptr<upgrader> _addr_u, module.Version m)
        {
            slice<module.Version> _p0 = default;
            error _p0 = default!;
            ref upgrader u = ref _addr_u.val;

            var (rs, err) = u.Reqs.Required(m);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            if (m != modload.Target)
            {
                return (rs, error.As(null!)!);
            }

            var overridden = make_map<@string, bool>();
            foreach (var (i, m) in rs)
            {
                {
                    var q__prev1 = q;

                    var q = u.cmdline[m.Path];

                    if (q != null && q.m.Version != "none")
                    {
                        rs[i] = q.m;
                        overridden[q.m.Path] = true;
                    }

                    q = q__prev1;

                }

            }
            {
                var q__prev1 = q;

                foreach (var (_, __q) in u.cmdline)
                {
                    q = __q;
                    if (!overridden[q.m.Path] && q.m.Path != modload.Target.Path && q.m.Version != "none")
                    {
                        rs = append(rs, q.m);
                    }

                }

                q = q__prev1;
            }

            return (rs, error.As(null!)!);

        }

        // Upgrade returns the desired upgrade for m.
        //
        // If m was requested at a specific version on the command line, then
        // Upgrade returns that version.
        //
        // If -u is set and m provides a dependency of a package matched by
        // command line arguments, then Upgrade may provider a newer tagged version.
        // If m is a tagged version, then Upgrade will return the latest tagged
        // version (with the same minor version number if -u=patch).
        // If m is a pseudo-version, then Upgrade returns the latest tagged version
        // only if that version has a time-stamp newer than m. This special case
        // prevents accidental downgrades when already using a pseudo-version
        // newer than the latest tagged version.
        //
        // If none of the above cases apply, then Upgrade returns m.
        private static (module.Version, error) Upgrade(this ptr<upgrader> _addr_u, module.Version m)
        {
            module.Version _p0 = default;
            error _p0 = default!;
            ref upgrader u = ref _addr_u.val;
 
            // Allow pkg@vers on the command line to override the upgrade choice v.
            // If q's version is < m.Version, then we're going to downgrade anyway,
            // and it's cleaner to avoid moving back and forth and picking up
            // extraneous other newer dependencies.
            // If q's version is > m.Version, then we're going to upgrade past
            // m.Version anyway, and again it's cleaner to avoid moving back and forth
            // picking up extraneous other newer dependencies.
            {
                var q = u.cmdline[m.Path];

                if (q != null)
                {
                    return (q.m, error.As(null!)!);
                }

            }


            if (!u.upgrade[m.Path])
            { 
                // Not involved in upgrade. Leave alone.
                return (m, error.As(null!)!);

            } 

            // Run query required by upgrade semantics.
            // Note that Query "latest" is not the same as using repo.Latest,
            // which may return a pseudoversion for the latest commit.
            // Query "latest" returns the newest tagged version or the newest
            // prerelease version if there are no non-prereleases, or repo.Latest
            // if there aren't any tagged versions.
            // If we're querying "upgrade" or "patch", Query will compare the current
            // version against the chosen version and will return the current version
            // if it is newer.
            var (info, err) = modload.Query(m.Path, string(getU), m.Version, modload.Allowed);
            if (err != null)
            { 
                // Report error but return m, to let version selection continue.
                // (Reporting the error will fail the command at the next base.ExitIfErrors.)

                // Special case: if the error is for m.Version itself and m.Version has a
                // replacement, then keep it and don't report the error: the fact that the
                // version is invalid is likely the reason it was replaced to begin with.
                ptr<module.InvalidVersionError> vErr;
                if (errors.As(err, _addr_vErr) && vErr.Version == m.Version && modload.Replacement(m).Path != "")
                {
                    return (m, error.As(null!)!);
                } 

                // Special case: if the error is "no matching versions" then don't
                // even report the error. Because Query does not consider pseudo-versions,
                // it may happen that we have a pseudo-version but during -u=patch
                // the query v0.0 matches no versions (not even the one we're using).
                ptr<modload.NoMatchingVersionError> noMatch;
                if (!errors.As(err, _addr_noMatch))
                {
                    @base.Errorf("go get: upgrading %s@%s: %v", m.Path, m.Version, err);
                }

                return (m, error.As(null!)!);

            }

            if (info.Version != m.Version)
            {
                logOncef("go: %s %s => %s", m.Path, getU, info.Version);
            }

            return (new module.Version(Path:m.Path,Version:info.Version), error.As(null!)!);

        }

        // buildListForLostUpgrade returns the build list for the module graph
        // rooted at lost. Unlike mvs.BuildList, the target module (lost) is not
        // treated specially. The returned build list may contain a newer version
        // of lost.
        //
        // buildListForLostUpgrade is used after a downgrade has removed a module
        // requested at a specific version. This helps us understand the requirements
        // implied by each downgrade.
        private static (slice<module.Version>, error) buildListForLostUpgrade(module.Version lost, mvs.Reqs reqs)
        {
            slice<module.Version> _p0 = default;
            error _p0 = default!;

            return mvs.BuildList(lostUpgradeRoot, addr(new lostUpgradeReqs(Reqs:reqs,lost:lost)));
        }

        private static module.Version lostUpgradeRoot = new module.Version(Path:"lost-upgrade-root",Version:"");

        private partial struct lostUpgradeReqs : mvs.Reqs
        {
            public ref mvs.Reqs Reqs => ref Reqs_val;
            public module.Version lost;
        }

        private static (slice<module.Version>, error) Required(this ptr<lostUpgradeReqs> _addr_r, module.Version mod)
        {
            slice<module.Version> _p0 = default;
            error _p0 = default!;
            ref lostUpgradeReqs r = ref _addr_r.val;

            if (mod == lostUpgradeRoot)
            {
                return (new slice<module.Version>(new module.Version[] { r.lost }), error.As(null!)!);
            }

            return r.Reqs.Required(mod);

        }

        private static sync.Map loggedLines = default;

        private static void logOncef(@string format, params object[] args)
        {
            args = args.Clone();

            var msg = fmt.Sprintf(format, args);
            {
                var (_, dup) = loggedLines.LoadOrStore(msg, true);

                if (!dup)
                {
                    fmt.Fprintln(os.Stderr, msg);
                }

            }

        }
    }
}}}}
