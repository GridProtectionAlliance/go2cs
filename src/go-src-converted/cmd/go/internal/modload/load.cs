// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modload -- go2cs converted at 2022 March 13 06:31:42 UTC
// import "cmd/go/internal/modload" ==> using modload = go.cmd.go.@internal.modload_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modload\load.go
namespace go.cmd.go.@internal;
// This file contains the module-mode package loader, as well as some accessory
// functions pertaining to the package import graph.
//
// There are two exported entry points into package loading — LoadPackages and
// ImportFromFiles — both implemented in terms of loadFromRoots, which itself
// manipulates an instance of the loader struct.
//
// Although most of the loading state is maintained in the loader struct,
// one key piece - the build list - is a global, so that it can be modified
// separate from the loading operation, such as during "go get"
// upgrades/downgrades or in "go mod" operations.
// TODO(#40775): It might be nice to make the loader take and return
// a buildList rather than hard-coding use of the global.
//
// Loading is an iterative process. On each iteration, we try to load the
// requested packages and their transitive imports, then try to resolve modules
// for any imported packages that are still missing.
//
// The first step of each iteration identifies a set of “root” packages.
// Normally the root packages are exactly those matching the named pattern
// arguments. However, for the "all" meta-pattern, the final set of packages is
// computed from the package import graph, and therefore cannot be an initial
// input to loading that graph. Instead, the root packages for the "all" pattern
// are those contained in the main module, and allPatternIsRoot parameter to the
// loader instructs it to dynamically expand those roots to the full "all"
// pattern as loading progresses.
//
// The pkgInAll flag on each loadPkg instance tracks whether that
// package is known to match the "all" meta-pattern.
// A package matches the "all" pattern if:
//     - it is in the main module, or
//     - it is imported by any test in the main module, or
//     - it is imported by another package in "all", or
//     - the main module specifies a go version ≤ 1.15, and the package is imported
//       by a *test of* another package in "all".
//
// When we implement lazy loading, we will record the modules providing packages
// in "all" even when we are only loading individual packages, so we set the
// pkgInAll flag regardless of the whether the "all" pattern is a root.
// (This is necessary to maintain the “import invariant” described in
// https://golang.org/design/36460-lazy-module-loading.)
//
// Because "go mod vendor" prunes out the tests of vendored packages, the
// behavior of the "all" pattern with -mod=vendor in Go 1.11–1.15 is the same
// as the "all" pattern (regardless of the -mod flag) in 1.16+.
// The loader uses the GoVersion parameter to determine whether the "all"
// pattern should close over tests (as in Go 1.11–1.15) or stop at only those
// packages transitively imported by the packages and tests in the main module
// ("all" in Go 1.16+ and "go mod vendor" in Go 1.11+).
//
// Note that it is possible for a loaded package NOT to be in "all" even when we
// are loading the "all" pattern. For example, packages that are transitive
// dependencies of other roots named on the command line must be loaded, but are
// not in "all". (The mod_notall test illustrates this behavior.)
// Similarly, if the LoadTests flag is set but the "all" pattern does not close
// over test dependencies, then when we load the test of a package that is in
// "all" but outside the main module, the dependencies of that test will not
// necessarily themselves be in "all". (That configuration does not arise in Go
// 1.11–1.15, but it will be possible in Go 1.16+.)
//
// Loading proceeds from the roots, using a parallel work-queue with a limit on
// the amount of active work (to avoid saturating disks, CPU cores, and/or
// network connections). Each package is added to the queue the first time it is
// imported by another package. When we have finished identifying the imports of
// a package, we add the test for that package if it is needed. A test may be
// needed if:
//     - the package matches a root pattern and tests of the roots were requested, or
//     - the package is in the main module and the "all" pattern is requested
//       (because the "all" pattern includes the dependencies of tests in the main
//       module), or
//     - the package is in "all" and the definition of "all" we are using includes
//       dependencies of tests (as is the case in Go ≤1.15).
//
// After all available packages have been loaded, we examine the results to
// identify any requested or imported packages that are still missing, and if
// so, which modules we could add to the module graph in order to make the
// missing packages available. We add those to the module graph and iterate,
// until either all packages resolve successfully or we cannot identify any
// module that would resolve any remaining missing package.
//
// If the main module is “tidy” (that is, if "go mod tidy" is a no-op for it)
// and all requested packages are in "all", then loading completes in a single
// iteration.
// TODO(bcmills): We should also be able to load in a single iteration if the
// requested packages all come from modules that are themselves tidy, regardless
// of whether those packages are in "all". Today, that requires two iterations
// if those packages are not found in existing dependencies of the main module.


using bytes = bytes_package;
using context = context_package;
using errors = errors_package;
using fmt = fmt_package;
using build = go.build_package;
using fs = io.fs_package;
using os = os_package;
using path = path_package;
using path = path_package;
using filepath = path.filepath_package;
using reflect = reflect_package;
using runtime = runtime_package;
using sort = sort_package;
using strings = strings_package;
using sync = sync_package;
using atomic = sync.atomic_package;

using @base = cmd.go.@internal.@base_package;
using cfg = cmd.go.@internal.cfg_package;
using fsys = cmd.go.@internal.fsys_package;
using imports = cmd.go.@internal.imports_package;
using modfetch = cmd.go.@internal.modfetch_package;
using mvs = cmd.go.@internal.mvs_package;
using par = cmd.go.@internal.par_package;
using search = cmd.go.@internal.search_package;
using str = cmd.go.@internal.str_package;

using module = golang.org.x.mod.module_package;
using semver = golang.org.x.mod.semver_package;


// loaded is the most recently-used package loader.
// It holds details about individual packages.
//
// This variable should only be accessed directly in top-level exported
// functions. All other functions that require or produce a *loader should pass
// or return it as an explicit parameter.

using System;
public static partial class modload_package {

private static ptr<loader> loaded;

// PackageOpts control the behavior of the LoadPackages function.
public partial struct PackageOpts {
    public @string GoVersion; // Tags are the build tags in effect (as interpreted by the
// cmd/go/internal/imports package).
// If nil, treated as equivalent to imports.Tags().
    public map<@string, bool> Tags; // Tidy, if true, requests that the build list and go.sum file be reduced to
// the minimial dependencies needed to reproducibly reload the requested
// packages.
    public bool Tidy; // TidyCompatibleVersion is the oldest Go version that must be able to
// reproducibly reload the requested packages.
//
// If empty, the compatible version is the Go version immediately prior to the
// 'go' version listed in the go.mod file.
    public @string TidyCompatibleVersion; // VendorModulesInGOROOTSrc indicates that if we are within a module in
// GOROOT/src, packages in the module's vendor directory should be resolved as
// actual module dependencies (instead of standard-library packages).
    public bool VendorModulesInGOROOTSrc; // ResolveMissingImports indicates that we should attempt to add module
// dependencies as needed to resolve imports of packages that are not found.
//
// For commands that support the -mod flag, resolving imports may still fail
// if the flag is set to "readonly" (the default) or "vendor".
    public bool ResolveMissingImports; // AssumeRootsImported indicates that the transitive dependencies of the root
// packages should be treated as if those roots will be imported by the main
// module.
    public bool AssumeRootsImported; // AllowPackage, if non-nil, is called after identifying the module providing
// each package. If AllowPackage returns a non-nil error, that error is set
// for the package, and the imports and test of that package will not be
// loaded.
//
// AllowPackage may be invoked concurrently by multiple goroutines,
// and may be invoked multiple times for a given package path.
    public Func<context.Context, @string, module.Version, error> AllowPackage; // LoadTests loads the test dependencies of each package matching a requested
// pattern. If ResolveMissingImports is also true, test dependencies will be
// resolved if missing.
    public bool LoadTests; // UseVendorAll causes the "all" package pattern to be interpreted as if
// running "go mod vendor" (or building with "-mod=vendor").
//
// This is a no-op for modules that declare 'go 1.16' or higher, for which this
// is the default (and only) interpretation of the "all" pattern in module mode.
    public bool UseVendorAll; // AllowErrors indicates that LoadPackages should not terminate the process if
// an error occurs.
    public bool AllowErrors; // SilencePackageErrors indicates that LoadPackages should not print errors
// that occur while matching or loading packages, and should not terminate the
// process if such an error occurs.
//
// Errors encountered in the module graph will still be reported.
//
// The caller may retrieve the silenced package errors using the Lookup
// function, and matching errors are still populated in the Errs field of the
// associated search.Match.)
    public bool SilencePackageErrors; // SilenceMissingStdImports indicates that LoadPackages should not print
// errors or terminate the process if an imported package is missing, and the
// import path looks like it might be in the standard library (perhaps in a
// future version).
    public bool SilenceMissingStdImports; // SilenceNoGoErrors indicates that LoadPackages should not print
// imports.ErrNoGo errors.
// This allows the caller to invoke LoadPackages (and report other errors)
// without knowing whether the requested packages exist for the given tags.
//
// Note that if a requested package does not exist *at all*, it will fail
// during module resolution and the error will not be suppressed.
    public bool SilenceNoGoErrors; // SilenceUnmatchedWarnings suppresses the warnings normally emitted for
// patterns that did not match any packages.
    public bool SilenceUnmatchedWarnings;
}

// LoadPackages identifies the set of packages matching the given patterns and
// loads the packages in the import graph rooted at that set.
public static (slice<ptr<search.Match>>, slice<@string>) LoadPackages(context.Context ctx, PackageOpts opts, params @string[] patterns) => func((_, panic, _) => {
    slice<ptr<search.Match>> matches = default;
    slice<@string> loadedPackages = default;
    patterns = patterns.Clone();

    if (opts.Tags == null) {
        opts.Tags = imports.Tags();
    }
    patterns = search.CleanPatterns(patterns);
    matches = make_slice<ptr<search.Match>>(0, len(patterns));
    var allPatternIsRoot = false;
    foreach (var (_, pattern) in patterns) {
        matches = append(matches, search.NewMatch(pattern));
        if (pattern == "all") {
            allPatternIsRoot = true;
        }
    }    Action<ptr<Requirements>, ptr<loader>> updateMatches = (rs, ld) => {
        {
            var m__prev1 = m;

            foreach (var (_, __m) in matches) {
                m = __m;

                if (m.IsLocal()) 
                    // Evaluate list of file system directories on first iteration.
                    if (m.Dirs == null) {
                        matchLocalDirs(ctx, _addr_m, _addr_rs);
                    } 

                    // Make a copy of the directory list and translate to import paths.
                    // Note that whether a directory corresponds to an import path
                    // changes as the build list is updated, and a directory can change
                    // from not being in the build list to being in it and back as
                    // the exact version of a particular module increases during
                    // the loader iterations.
                    m.Pkgs = m.Pkgs[..(int)0];
                    foreach (var (_, dir) in m.Dirs) {
                        var (pkg, err) = resolveLocalPackage(ctx, dir, _addr_rs);
                        if (err != null) {
                            if (!m.IsLiteral() && (err == errPkgIsBuiltin || err == errPkgIsGorootSrc)) {
                                continue; // Don't include "builtin" or GOROOT/src in wildcard patterns.
                            } 

                            // If we're outside of a module, ensure that the failure mode
                            // indicates that.
                            ModRoot();

                            if (ld != null) {
                                m.AddError(err);
                            }
                            continue;
                        }
                        m.Pkgs = append(m.Pkgs, pkg);
                    }
                else if (m.IsLiteral()) 
                    m.Pkgs = new slice<@string>(new @string[] { m.Pattern() });
                else if (strings.Contains(m.Pattern(), "...")) 
                    m.Errs = m.Errs[..(int)0];
                    var (mg, err) = rs.Graph(ctx);
                    if (err != null) { 
                        // The module graph is (or may be) incomplete — perhaps we failed to
                        // load the requirements of some module. This is an error in matching
                        // the patterns to packages, because we may be missing some packages
                        // or we may erroneously match packages in the wrong versions of
                        // modules. However, for cases like 'go list -e', the error should not
                        // necessarily prevent us from loading the packages we could find.
                        m.Errs = append(m.Errs, err);
                    }
                    matchPackages(ctx, m, opts.Tags, includeStd, mg.BuildList());
                else if (m.Pattern() == "all") 
                    if (ld == null) { 
                        // The initial roots are the packages in the main module.
                        // loadFromRoots will expand that to "all".
                        m.Errs = m.Errs[..(int)0];
                        matchPackages(ctx, m, opts.Tags, omitStd, new slice<module.Version>(new module.Version[] { Target }));
                    }
                    else
 { 
                        // Starting with the packages in the main module,
                        // enumerate the full list of "all".
                        m.Pkgs = ld.computePatternAll();
                    }
                else if (m.Pattern() == "std" || m.Pattern() == "cmd") 
                    if (m.Pkgs == null) {
                        m.MatchPackages(); // Locate the packages within GOROOT/src.
                    }
                else 
                    panic(fmt.Sprintf("internal error: modload missing case for pattern %s", m.Pattern()));
                            }

            m = m__prev1;
        }
    };

    var (initialRS, _) = loadModFile(ctx); // Ignore needCommit — we're going to commit at the end regardless.

    var ld = loadFromRoots(ctx, new loaderParams(PackageOpts:opts,requirements:initialRS,allPatternIsRoot:allPatternIsRoot,listRoots:func(rs*Requirements)(roots[]string){updateMatches(rs,nil)for_,m:=rangematches{roots=append(roots,m.Pkgs...)}returnroots},)); 

    // One last pass to finalize wildcards.
    updateMatches(ld.requirements, ld); 

    // List errors in matching patterns (such as directory permission
    // errors for wildcard patterns).
    if (!ld.SilencePackageErrors) {
        foreach (var (_, match) in matches) {
            foreach (var (_, err) in match.Errs) {
                ld.errorf("%v\n", err);
            }
        }
    }
    @base.ExitIfErrors();

    if (!opts.SilenceUnmatchedWarnings) {
        search.WarnUnmatched(matches);
    }
    if (opts.Tidy) {
        if (cfg.BuildV) {
            var (mg, _) = ld.requirements.Graph(ctx);

            {
                var m__prev1 = m;

                foreach (var (_, __m) in initialRS.rootModules) {
                    m = __m;
                    bool unused = default;
                    if (ld.requirements.depth == eager) { 
                        // m is unused if it was dropped from the module graph entirely. If it
                        // was only demoted from direct to indirect, it may still be in use via
                        // a transitive import.
                        unused = mg.Selected(m.Path) == "none";
                    }
                    else
 { 
                        // m is unused if it was dropped from the roots. If it is still present
                        // as a transitive dependency, that transitive dependency is not needed
                        // by any package or test in the main module.
                        var (_, ok) = ld.requirements.rootSelected(m.Path);
                        unused = !ok;
                    }
                    if (unused) {
                        fmt.Fprintf(os.Stderr, "unused %s\n", m.Path);
                    }
                }

                m = m__prev1;
            }
        }
        var keep = keepSums(ctx, ld, ld.requirements, loadedZipSumsOnly);
        {
            var compatDepth = modDepthFromGoVersion(ld.TidyCompatibleVersion);

            if (compatDepth != ld.requirements.depth) {
                var compatRS = newRequirements(compatDepth, ld.requirements.rootModules, ld.requirements.direct);
                ld.checkTidyCompatibility(ctx, compatRS);

                {
                    var m__prev1 = m;

                    foreach (var (__m) in keepSums(ctx, ld, compatRS, loadedZipSumsOnly)) {
                        m = __m;
                        keep[m] = true;
                    }

                    m = m__prev1;
                }
            }

        }

        if (allowWriteGoMod) {
            modfetch.TrimGoSum(keep); 

            // commitRequirements below will also call WriteGoSum, but the "keep" map
            // we have here could be strictly larger: commitRequirements only commits
            // loaded.requirements, but here we may have also loaded (and want to
            // preserve checksums for) additional entities from compatRS, which are
            // only needed for compatibility with ld.TidyCompatibleVersion.
            modfetch.WriteGoSum(keep);
        }
    }
    loaded = ld;
    commitRequirements(ctx, loaded.GoVersion, loaded.requirements);

    {
        var pkg__prev1 = pkg;

        foreach (var (_, __pkg) in ld.pkgs) {
            pkg = __pkg;
            if (!pkg.isTest()) {
                loadedPackages = append(loadedPackages, pkg.path);
            }
        }
        pkg = pkg__prev1;
    }

    sort.Strings(loadedPackages);
    return (matches, loadedPackages);
});

// matchLocalDirs is like m.MatchDirs, but tries to avoid scanning directories
// outside of the standard library and active modules.
private static void matchLocalDirs(context.Context ctx, ptr<search.Match> _addr_m, ptr<Requirements> _addr_rs) => func((_, panic, _) => {
    ref search.Match m = ref _addr_m.val;
    ref Requirements rs = ref _addr_rs.val;

    if (!m.IsLocal()) {
        panic(fmt.Sprintf("internal error: resolveLocalDirs on non-local pattern %s", m.Pattern()));
    }
    {
        var i = strings.Index(m.Pattern(), "...");

        if (i >= 0) { 
            // The pattern is local, but it is a wildcard. Its packages will
            // only resolve to paths if they are inside of the standard
            // library, the main module, or some dependency of the main
            // module. Verify that before we walk the filesystem: a filesystem
            // walk in a directory like /var or /etc can be very expensive!
            var dir = filepath.Dir(filepath.Clean(m.Pattern()[..(int)i + 3]));
            var absDir = dir;
            if (!filepath.IsAbs(dir)) {
                absDir = filepath.Join(@base.Cwd(), dir);
            }
            if (search.InDir(absDir, cfg.GOROOTsrc) == "" && search.InDir(absDir, ModRoot()) == "" && pathInModuleCache(ctx, absDir, _addr_rs) == "") {
                m.Dirs = new slice<@string>(new @string[] {  });
                m.AddError(fmt.Errorf("directory prefix %s outside available modules", @base.ShortPath(absDir)));
                return ;
            }
        }
    }

    m.MatchDirs();
});

// resolveLocalPackage resolves a filesystem path to a package path.
private static (@string, error) resolveLocalPackage(context.Context ctx, @string dir, ptr<Requirements> _addr_rs) {
    @string _p0 = default;
    error _p0 = default!;
    ref Requirements rs = ref _addr_rs.val;

    @string absDir = default;
    if (filepath.IsAbs(dir)) {
        absDir = filepath.Clean(dir);
    }
    else
 {
        absDir = filepath.Join(@base.Cwd(), dir);
    }
    var (bp, err) = cfg.BuildContext.ImportDir(absDir, 0);
    if (err != null && (bp == null || len(bp.IgnoredGoFiles) == 0)) { 
        // golang.org/issue/32917: We should resolve a relative path to a
        // package path only if the relative path actually contains the code
        // for that package.
        //
        // If the named directory does not exist or contains no Go files,
        // the package does not exist.
        // Other errors may affect package loading, but not resolution.
        {
            var (_, err) = fsys.Stat(absDir);

            if (err != null) {
                if (os.IsNotExist(err)) { 
                    // Canonicalize OS-specific errors to errDirectoryNotFound so that error
                    // messages will be easier for users to search for.
                    return ("", error.As(addr(new fs.PathError(Op:"stat",Path:absDir,Err:errDirectoryNotFound))!)!);
                }
                return ("", error.As(err)!);
            }

        }
        {
            ptr<build.NoGoError> (_, noGo) = err._<ptr<build.NoGoError>>();

            if (noGo) { 
                // A directory that does not contain any Go source files — even ignored
                // ones! — is not a Go package, and we can't resolve it to a package
                // path because that path could plausibly be provided by some other
                // module.
                //
                // Any other error indicates that the package “exists” (at least in the
                // sense that it cannot exist in any other module), but has some other
                // problem (such as a syntax error).
                return ("", error.As(err)!);
            }

        }
    }
    if (modRoot != "" && absDir == modRoot) {
        if (absDir == cfg.GOROOTsrc) {
            return ("", error.As(errPkgIsGorootSrc)!);
        }
        return (targetPrefix, error.As(null!)!);
    }
    if (modRoot != "" && strings.HasPrefix(absDir, modRoot + string(filepath.Separator)) && !strings.Contains(absDir[(int)len(modRoot)..], "@")) {
        var suffix = filepath.ToSlash(absDir[(int)len(modRoot)..]);
        if (strings.HasPrefix(suffix, "/vendor/")) {
            if (cfg.BuildMod != "vendor") {
                return ("", error.As(fmt.Errorf("without -mod=vendor, directory %s has no package path", absDir))!);
            }
            readVendorList();
            var pkg = strings.TrimPrefix(suffix, "/vendor/");
            {
                var (_, ok) = vendorPkgModule[pkg];

                if (!ok) {
                    return ("", error.As(fmt.Errorf("directory %s is not a package listed in vendor/modules.txt", absDir))!);
                }

            }
            return (pkg, error.As(null!)!);
        }
        if (targetPrefix == "") {
            pkg = strings.TrimPrefix(suffix, "/");
            if (pkg == "builtin") { 
                // "builtin" is a pseudo-package with a real source file.
                // It's not included in "std", so it shouldn't resolve from "."
                // within module "std" either.
                return ("", error.As(errPkgIsBuiltin)!);
            }
            return (pkg, error.As(null!)!);
        }
        pkg = targetPrefix + suffix;
        {
            var (_, ok, err) = dirInModule(pkg, targetPrefix, modRoot, true);

            if (err != null) {
                return ("", error.As(err)!);
            }
            else if (!ok) {
                return ("", error.As(addr(new PackageNotInModuleError(Mod:Target,Pattern:pkg))!)!);
            }

        }
        return (pkg, error.As(null!)!);
    }
    {
        var sub = search.InDir(absDir, cfg.GOROOTsrc);

        if (sub != "" && sub != "." && !strings.Contains(sub, "@")) {
            pkg = filepath.ToSlash(sub);
            if (pkg == "builtin") {
                return ("", error.As(errPkgIsBuiltin)!);
            }
            return (pkg, error.As(null!)!);
        }
    }

    pkg = pathInModuleCache(ctx, absDir, _addr_rs);
    if (pkg == "") {
        return ("", error.As(fmt.Errorf("directory %s outside available modules", @base.ShortPath(absDir)))!);
    }
    return (pkg, error.As(null!)!);
}

private static var errDirectoryNotFound = errors.New("directory not found");private static var errPkgIsGorootSrc = errors.New("GOROOT/src is not an importable package");private static var errPkgIsBuiltin = errors.New("\"builtin\" is a pseudo-package, not an importable package");

// pathInModuleCache returns the import path of the directory dir,
// if dir is in the module cache copy of a module in our build list.
private static @string pathInModuleCache(context.Context ctx, @string dir, ptr<Requirements> _addr_rs) {
    ref Requirements rs = ref _addr_rs.val;

    Func<module.Version, (@string, bool)> tryMod = m => {
        @string root = default;
        error err = default!;
        {
            var repl = Replacement(m);

            if (repl.Path != "" && repl.Version == "") {
                root = repl.Path;
                if (!filepath.IsAbs(root)) {
                    root = filepath.Join(ModRoot(), root);
                }
            }
            else if (repl.Path != "") {
                root, err = modfetch.DownloadDir(repl);
            }
            else
 {
                root, err = modfetch.DownloadDir(m);
            }

        }
        if (err != null) {
            return ("", false);
        }
        var sub = search.InDir(dir, root);
        if (sub == "") {
            return ("", false);
        }
        sub = filepath.ToSlash(sub);
        if (strings.Contains(sub, "/vendor/") || strings.HasPrefix(sub, "vendor/") || strings.Contains(sub, "@")) {
            return ("", false);
        }
        return (path.Join(m.Path, filepath.ToSlash(sub)), true);
    };

    if (rs.depth == lazy) {
        {
            var m__prev1 = m;

            foreach (var (_, __m) in rs.rootModules) {
                m = __m;
                {
                    var (v, _) = rs.rootSelected(m.Path);

                    if (v != m.Version) {
                        continue; // m is a root, but we have a higher root for the same path.
                    }

                }
                {
                    var importPath__prev2 = importPath;

                    var (importPath, ok) = tryMod(m);

                    if (ok) { 
                        // checkMultiplePaths ensures that a module can be used for at most one
                        // requirement, so this must be it.
                        return importPath;
                    }

                    importPath = importPath__prev2;

                }
            }

            m = m__prev1;
        }
    }
    var (mg, _) = rs.Graph(ctx);
    @string importPath = default;
    {
        var m__prev1 = m;

        foreach (var (_, __m) in mg.BuildList()) {
            m = __m;
            bool found = default;
            importPath, found = tryMod(m);
            if (found) {
                break;
            }
        }
        m = m__prev1;
    }

    return importPath;
}

// ImportFromFiles adds modules to the build list as needed
// to satisfy the imports in the named Go source files.
//
// Errors in missing dependencies are silenced.
//
// TODO(bcmills): Silencing errors seems off. Take a closer look at this and
// figure out what the error-reporting actually ought to be.
public static void ImportFromFiles(context.Context ctx, slice<@string> gofiles) {
    var rs = LoadModFile(ctx);

    var tags = imports.Tags();
    var (imports, testImports, err) = imports.ScanFiles(gofiles, tags);
    if (err != null) {
        @base.Fatalf("go: %v", err);
    }
    loaded = loadFromRoots(ctx, new loaderParams(PackageOpts:PackageOpts{Tags:tags,ResolveMissingImports:true,SilencePackageErrors:true,},requirements:rs,listRoots:func(*Requirements)(roots[]string){roots=append(roots,imports...)roots=append(roots,testImports...)returnroots},));
    commitRequirements(ctx, loaded.GoVersion, loaded.requirements);
}

// DirImportPath returns the effective import path for dir,
// provided it is within the main module, or else returns ".".
public static @string DirImportPath(context.Context ctx, @string dir) {
    if (!HasModRoot()) {
        return ".";
    }
    LoadModFile(ctx); // Sets targetPrefix.

    if (!filepath.IsAbs(dir)) {
        dir = filepath.Join(@base.Cwd(), dir);
    }
    else
 {
        dir = filepath.Clean(dir);
    }
    if (dir == modRoot) {
        return targetPrefix;
    }
    if (strings.HasPrefix(dir, modRoot + string(filepath.Separator))) {
        var suffix = filepath.ToSlash(dir[(int)len(modRoot)..]);
        if (strings.HasPrefix(suffix, "/vendor/")) {
            return strings.TrimPrefix(suffix, "/vendor/");
        }
        return targetPrefix + suffix;
    }
    return ".";
}

// ImportMap returns the actual package import path
// for an import path found in source code.
// If the given import path does not appear in the source code
// for the packages that have been loaded, ImportMap returns the empty string.
public static @string ImportMap(@string path) {
    ptr<loadPkg> (pkg, ok) = loaded.pkgCache.Get(path)._<ptr<loadPkg>>();
    if (!ok) {
        return "";
    }
    return pkg.path;
}

// PackageDir returns the directory containing the source code
// for the package named by the import path.
public static @string PackageDir(@string path) {
    ptr<loadPkg> (pkg, ok) = loaded.pkgCache.Get(path)._<ptr<loadPkg>>();
    if (!ok) {
        return "";
    }
    return pkg.dir;
}

// PackageModule returns the module providing the package named by the import path.
public static module.Version PackageModule(@string path) {
    ptr<loadPkg> (pkg, ok) = loaded.pkgCache.Get(path)._<ptr<loadPkg>>();
    if (!ok) {
        return new module.Version();
    }
    return pkg.mod;
}

// Lookup returns the source directory, import path, and any loading error for
// the package at path as imported from the package in parentDir.
// Lookup requires that one of the Load functions in this package has already
// been called.
public static (@string, @string, error) Lookup(@string parentPath, bool parentIsStd, @string path) => func((_, panic, _) => {
    @string dir = default;
    @string realPath = default;
    error err = default!;

    if (path == "") {
        panic("Lookup called with empty package path");
    }
    if (parentIsStd) {
        path = loaded.stdVendor(parentPath, path);
    }
    ptr<loadPkg> (pkg, ok) = loaded.pkgCache.Get(path)._<ptr<loadPkg>>();
    if (!ok) { 
        // The loader should have found all the relevant paths.
        // There are a few exceptions, though:
        //    - during go list without -test, the p.Resolve calls to process p.TestImports and p.XTestImports
        //      end up here to canonicalize the import paths.
        //    - during any load, non-loaded packages like "unsafe" end up here.
        //    - during any load, build-injected dependencies like "runtime/cgo" end up here.
        //    - because we ignore appengine/* in the module loader,
        //      the dependencies of any actual appengine/* library end up here.
        var dir = findStandardImportPath(path);
        if (dir != "") {
            return (dir, path, error.As(null!)!);
        }
        return ("", "", error.As(errMissing)!);
    }
    return (pkg.dir, pkg.path, error.As(pkg.err)!);
});

// A loader manages the process of loading information about
// the required packages for a particular build,
// checking that the packages are available in the module set,
// and updating the module set if needed.
private partial struct loader {
    public ref loaderParams loaderParams => ref loaderParams_val; // allClosesOverTests indicates whether the "all" pattern includes
// dependencies of tests outside the main module (as in Go 1.11–1.15).
// (Otherwise — as in Go 1.16+ — the "all" pattern includes only the packages
// transitively *imported by* the packages and tests in the main module.)
    public bool allClosesOverTests;
    public ptr<par.Queue> work; // reset on each iteration
    public slice<ptr<loadPkg>> roots;
    public ptr<par.Cache> pkgCache; // package path (string) → *loadPkg
    public slice<ptr<loadPkg>> pkgs; // transitive closure of loaded packages and tests; populated in buildStacks
}

// loaderParams configure the packages loaded by, and the properties reported
// by, a loader instance.
private partial struct loaderParams {
    public ref PackageOpts PackageOpts => ref PackageOpts_val;
    public ptr<Requirements> requirements;
    public bool allPatternIsRoot; // Is the "all" pattern an additional root?

    public Func<ptr<Requirements>, slice<@string>> listRoots;
}

private static void reset(this ptr<loader> _addr_ld) => func((_, panic, _) => {
    ref loader ld = ref _addr_ld.val;

    panic("loader.reset when not idle");
    ld.roots = null;
    ld.pkgCache = @new<par.Cache>();
    ld.pkgs = null;
});

// errorf reports an error via either os.Stderr or base.Errorf,
// according to whether ld.AllowErrors is set.
private static void errorf(this ptr<loader> _addr_ld, @string format, params object[] args) {
    args = args.Clone();
    ref loader ld = ref _addr_ld.val;

    if (ld.AllowErrors) {
        fmt.Fprintf(os.Stderr, format, args);
    }
    else
 {
        @base.Errorf(format, args);
    }
}

// A loadPkg records information about a single loaded package.
private partial struct loadPkg {
    public @string path; // import path
    public ptr<loadPkg> testOf; // Populated at construction time and updated by (*loader).applyPkgFlags:
    public atomicLoadPkgFlags flags; // Populated by (*loader).load:
    public module.Version mod; // module providing package
    public @string dir; // directory containing source code
    public error err; // error loading package
    public slice<ptr<loadPkg>> imports; // packages imported by this one
    public slice<@string> testImports; // test-only imports, saved for use by pkg.test.
    public bool inStd; // Populated by (*loader).pkgTest:
    public sync.Once testOnce;
    public ptr<loadPkg> test; // Populated by postprocessing in (*loader).buildStacks:
    public ptr<loadPkg> stack; // package importing this one in minimal import stack for this pkg
}

// loadPkgFlags is a set of flags tracking metadata about a package.
private partial struct loadPkgFlags { // : sbyte
}

 
// pkgInAll indicates that the package is in the "all" package pattern,
// regardless of whether we are loading the "all" package pattern.
//
// When the pkgInAll flag and pkgImportsLoaded flags are both set, the caller
// who set the last of those flags must propagate the pkgInAll marking to all
// of the imports of the marked package.
//
// A test is marked with pkgInAll if that test would promote the packages it
// imports to be in "all" (such as when the test is itself within the main
// module, or when ld.allClosesOverTests is true).
private static readonly loadPkgFlags pkgInAll = 1 << (int)(iota); 

// pkgIsRoot indicates that the package matches one of the root package
// patterns requested by the caller.
//
// If LoadTests is set, then when pkgIsRoot and pkgImportsLoaded are both set,
// the caller who set the last of those flags must populate a test for the
// package (in the pkg.test field).
//
// If the "all" pattern is included as a root, then non-test packages in "all"
// are also roots (and must be marked pkgIsRoot).
private static readonly var pkgIsRoot = 0; 

// pkgFromRoot indicates that the package is in the transitive closure of
// imports starting at the roots. (Note that every package marked as pkgIsRoot
// is also trivially marked pkgFromRoot.)
private static readonly var pkgFromRoot = 1; 

// pkgImportsLoaded indicates that the imports and testImports fields of a
// loadPkg have been populated.
private static readonly var pkgImportsLoaded = 2;

// has reports whether all of the flags in cond are set in f.
private static bool has(this loadPkgFlags f, loadPkgFlags cond) {
    return f & cond == cond;
}

// An atomicLoadPkgFlags stores a loadPkgFlags for which individual flags can be
// added atomically.
private partial struct atomicLoadPkgFlags {
    public int bits;
}

// update sets the given flags in af (in addition to any flags already set).
//
// update returns the previous flag state so that the caller may determine which
// flags were newly-set.
private static loadPkgFlags update(this ptr<atomicLoadPkgFlags> _addr_af, loadPkgFlags flags) {
    loadPkgFlags old = default;
    ref atomicLoadPkgFlags af = ref _addr_af.val;

    while (true) {
        var old = atomic.LoadInt32(_addr_af.bits);
        var @new = old | int32(flags);
        if (new == old || atomic.CompareAndSwapInt32(_addr_af.bits, old, new)) {
            return loadPkgFlags(old);
        }
    }
}

// has reports whether all of the flags in cond are set in af.
private static bool has(this ptr<atomicLoadPkgFlags> _addr_af, loadPkgFlags cond) {
    ref atomicLoadPkgFlags af = ref _addr_af.val;

    return loadPkgFlags(atomic.LoadInt32(_addr_af.bits)) & cond == cond;
}

// isTest reports whether pkg is a test of another package.
private static bool isTest(this ptr<loadPkg> _addr_pkg) {
    ref loadPkg pkg = ref _addr_pkg.val;

    return pkg.testOf != null;
}

// fromExternalModule reports whether pkg was loaded from a module other than
// the main module.
private static bool fromExternalModule(this ptr<loadPkg> _addr_pkg) {
    ref loadPkg pkg = ref _addr_pkg.val;

    if (pkg.mod.Path == "") {
        return false; // loaded from the standard library, not a module
    }
    if (pkg.mod.Path == Target.Path) {
        return false; // loaded from the main module.
    }
    return true;
}

private static var errMissing = errors.New("cannot find package");

// loadFromRoots attempts to load the build graph needed to process a set of
// root packages and their dependencies.
//
// The set of root packages is returned by the params.listRoots function, and
// expanded to the full set of packages by tracing imports (and possibly tests)
// as needed.
private static ptr<loader> loadFromRoots(context.Context ctx, loaderParams @params) => func((_, panic, _) => {
    ptr<loader> ld = addr(new loader(loaderParams:params,work:par.NewQueue(runtime.GOMAXPROCS(0)),));

    if (ld.GoVersion == "") {
        ld.GoVersion = modFileGoVersion();

        if (ld.Tidy && semver.Compare("v" + ld.GoVersion, "v" + LatestGoVersion()) > 0) {
            ld.errorf("go mod tidy: go.mod file indicates go %s, but maximum supported version is %s\n", ld.GoVersion, LatestGoVersion());
            @base.ExitIfErrors();
        }
    }
    if (ld.Tidy) {
        if (ld.TidyCompatibleVersion == "") {
            ld.TidyCompatibleVersion = priorGoVersion(ld.GoVersion);
        }
        else if (semver.Compare("v" + ld.TidyCompatibleVersion, "v" + ld.GoVersion) > 0) { 
            // Each version of the Go toolchain knows how to interpret go.mod and
            // go.sum files produced by all previous versions, so a compatibility
            // version higher than the go.mod version adds nothing.
            ld.TidyCompatibleVersion = ld.GoVersion;
        }
    }
    if (semver.Compare("v" + ld.GoVersion, narrowAllVersionV) < 0 && !ld.UseVendorAll) { 
        // The module's go version explicitly predates the change in "all" for lazy
        // loading, so continue to use the older interpretation.
        ld.allClosesOverTests = true;
    }
    error err = default!;
    ld.requirements, err = convertDepth(ctx, ld.requirements, modDepthFromGoVersion(ld.GoVersion));
    if (err != null) {
        ld.errorf("go: %v\n", err);
    }
    if (ld.requirements.depth == eager) {
        err = default!;
        ld.requirements, _, err = expandGraph(ctx, ld.requirements);
        if (err != null) {
            ld.errorf("go: %v\n", err);
        }
    }
    while (true) {
        ld.reset(); 

        // Load the root packages and their imports.
        // Note: the returned roots can change on each iteration,
        // since the expansion of package patterns depends on the
        // build list we're using.
        var rootPkgs = ld.listRoots(ld.requirements);

        if (ld.requirements.depth == lazy && cfg.BuildMod == "mod") { 
            // Before we start loading transitive imports of packages, locate all of
            // the root packages and promote their containing modules to root modules
            // dependencies. If their go.mod files are tidy (the common case) and the
            // set of root packages does not change then we can select the correct
            // versions of all transitive imports on the first try and complete
            // loading in a single iteration.
            var changedBuildList = ld.preloadRootModules(ctx, rootPkgs);
            if (changedBuildList) { 
                // The build list has changed, so the set of root packages may have also
                // changed. Start over to pick up the changes. (Preloading roots is much
                // cheaper than loading the full import graph, so we would rather pay
                // for an extra iteration of preloading than potentially end up
                // discarding the result of a full iteration of loading.)
                continue;
            }
        }
        map inRoots = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<loadPkg>, bool>{};
        foreach (var (_, path) in rootPkgs) {
            var root = ld.pkg(ctx, path, pkgIsRoot);
            if (!inRoots[root]) {
                ld.roots = append(ld.roots, root);
                inRoots[root] = true;
            }
        }        ld.work.Idle().Receive();

        ld.buildStacks();

        var (changed, err) = ld.updateRequirements(ctx);
        if (err != null) {
            ld.errorf("go: %v\n", err);
            break;
        }
        if (changed) { 
            // Don't resolve missing imports until the module graph have stabilized.
            // If the roots are still changing, they may turn out to specify a
            // requirement on the missing package(s), and we would rather use a
            // version specified by a new root than add a new dependency on an
            // unrelated version.
            continue;
        }
        if (!ld.ResolveMissingImports || (!HasModRoot() && !allowMissingModuleImports)) { 
            // We've loaded as much as we can without resolving missing imports.
            break;
        }
        var modAddedBy = ld.resolveMissingImports(ctx);
        if (len(modAddedBy) == 0) { 
            // The roots are stable, and we've resolved all of the missing packages
            // that we can.
            break;
        }
        var toAdd = make_slice<module.Version>(0, len(modAddedBy));
        {
            var m__prev2 = m;

            foreach (var (__m, _) in modAddedBy) {
                m = __m;
                toAdd = append(toAdd, m);
            }

            m = m__prev2;
        }

        module.Sort(toAdd); // to make errors deterministic

        // We ran updateRequirements before resolving missing imports and it didn't
        // make any changes, so we know that the requirement graph is already
        // consistent with ld.pkgs: we don't need to pass ld.pkgs to updateRoots
        // again. (That would waste time looking for changes that we have already
        // applied.)
        slice<ptr<loadPkg>> noPkgs = default; 
        // We also know that we're going to call updateRequirements again next
        // iteration so we don't need to also update it here. (That would waste time
        // computing a "direct" map that we'll have to recompute later anyway.)
        var direct = ld.requirements.direct;
        var (rs, err) = updateRoots(ctx, direct, ld.requirements, noPkgs, toAdd, ld.AssumeRootsImported);
        if (err != null) { 
            // If an error was found in a newly added module, report the package
            // import stack instead of the module requirement stack. Packages
            // are more descriptive.
            {
                error err__prev2 = err;

                ptr<mvs.BuildListError> (err, ok) = err._<ptr<mvs.BuildListError>>();

                if (ok) {
                    {
                        var pkg__prev3 = pkg;

                        var pkg = modAddedBy[err.Module()];

                        if (pkg != null) {
                            ld.errorf("go: %s: %v\n", pkg.stackText(), err.Err);
                            break;
                        }

                        pkg = pkg__prev3;

                    }
                }

                err = err__prev2;

            }
            ld.errorf("go: %v\n", err);
            break;
        }
        if (reflect.DeepEqual(rs.rootModules, ld.requirements.rootModules)) { 
            // Something is deeply wrong. resolveMissingImports gave us a non-empty
            // set of modules to add to the graph, but adding those modules had no
            // effect — either they were already in the graph, or updateRoots did not
            // add them as requested.
            panic(fmt.Sprintf("internal error: adding %v to module graph had no effect on root requirements (%v)", toAdd, rs.rootModules));
        }
        ld.requirements = rs;
    }
    @base.ExitIfErrors(); // TODO(bcmills): Is this actually needed?

    // Tidy the build list, if applicable, before we report errors.
    // (The process of tidying may remove errors from irrelevant dependencies.)
    if (ld.Tidy) {
        (rs, err) = tidyRoots(ctx, ld.requirements, ld.pkgs);
        if (err != null) {
            ld.errorf("go: %v\n", err);
        }
        if (ld.requirements.depth == lazy) { 
            // We continuously add tidy roots to ld.requirements during loading, so at
            // this point the tidy roots should be a subset of the roots of
            // ld.requirements, ensuring that no new dependencies are brought inside
            // the lazy-loading horizon.
            // If that is not the case, there is a bug in the loading loop above.
            {
                var m__prev1 = m;

                foreach (var (_, __m) in rs.rootModules) {
                    m = __m;
                    {
                        var (v, ok) = ld.requirements.rootSelected(m.Path);

                        if (!ok || v != m.Version) {
                            ld.errorf("go mod tidy: internal error: a requirement on %v is needed but was not added during package loading\n", m);
                            @base.ExitIfErrors();
                        }

                    }
                }

                m = m__prev1;
            }
        }
        ld.requirements = rs;
    }
    {
        var pkg__prev1 = pkg;

        foreach (var (_, __pkg) in ld.pkgs) {
            pkg = __pkg;
            if (pkg.err == null) {
                continue;
            } 

            // Add importer information to checksum errors.
            {
                ref var sumErr = ref heap((ImportMissingSumError.val)(null), out ptr<var> _addr_sumErr);

                if (errors.As(pkg.err, _addr_sumErr)) {
                    {
                        var importer = pkg.stack;

                        if (importer != null) {
                            sumErr.importer = importer.path;
                            sumErr.importerVersion = importer.mod.Version;
                            sumErr.importerIsTest = importer.testOf != null;
                        }

                    }
                }

            }

            if (ld.SilencePackageErrors) {
                continue;
            }
            {
                ref var stdErr = ref heap((ImportMissingError.val)(null), out ptr<var> _addr_stdErr);

                if (errors.As(pkg.err, _addr_stdErr) && stdErr.isStd && ld.SilenceMissingStdImports) {
                    continue;
                }

            }
            if (ld.SilenceNoGoErrors && errors.Is(pkg.err, imports.ErrNoGo)) {
                continue;
            }
            ld.errorf("%s: %v\n", pkg.stackText(), pkg.err);
        }
        pkg = pkg__prev1;
    }

    ld.checkMultiplePaths();
    return _addr_ld!;
});

// updateRequirements ensures that ld.requirements is consistent with the
// information gained from ld.pkgs and includes the modules in add as roots at
// at least the given versions.
//
// In particular:
//
//     - Modules that provide packages directly imported from the main module are
//       marked as direct, and are promoted to explicit roots. If a needed root
//       cannot be promoted due to -mod=readonly or -mod=vendor, the importing
//       package is marked with an error.
//
//     - If ld scanned the "all" pattern independent of build constraints, it is
//       guaranteed to have seen every direct import. Module dependencies that did
//       not provide any directly-imported package are then marked as indirect.
//
//     - Root dependencies are updated to their selected versions.
//
// The "changed" return value reports whether the update changed the selected
// version of any module that either provided a loaded package or may now
// provide a package that was previously unresolved.
private static (bool, error) updateRequirements(this ptr<loader> _addr_ld, context.Context ctx) {
    bool changed = default;
    error err = default!;
    ref loader ld = ref _addr_ld.val;

    var rs = ld.requirements; 

    // direct contains the set of modules believed to provide packages directly
    // imported by the main module.
    map<@string, bool> direct = default; 

    // If we didn't scan all of the imports from the main module, or didn't use
    // imports.AnyTags, then we didn't necessarily load every package that
    // contributes “direct” imports — so we can't safely mark existing direct
    // dependencies in ld.requirements as indirect-only. Propagate them as direct.
    var loadedDirect = ld.allPatternIsRoot && reflect.DeepEqual(ld.Tags, imports.AnyTags());
    if (loadedDirect) {
        direct = make_map<@string, bool>();
    }
    else
 { 
        // TODO(bcmills): It seems like a shame to allocate and copy a map here when
        // it will only rarely actually vary from rs.direct. Measure this cost and
        // maybe avoid the copy.
        direct = make_map<@string, bool>(len(rs.direct));
        foreach (var (mPath) in rs.direct) {
            direct[mPath] = true;
        }
    }
    {
        var pkg__prev1 = pkg;

        foreach (var (_, __pkg) in ld.pkgs) {
            pkg = __pkg;
            if (pkg.mod != Target) {
                continue;
            }
            foreach (var (_, dep) in pkg.imports) {
                if (!dep.fromExternalModule()) {
                    continue;
                }
                if (pkg.err == null && cfg.BuildMod != "mod") {
                    {
                        var (v, ok) = rs.rootSelected(dep.mod.Path);

                        if (!ok || v != dep.mod.Version) { 
                            // dep.mod is not an explicit dependency, but needs to be.
                            // Because we are not in "mod" mode, we will not be able to update it.
                            // Instead, mark the importing package with an error.
                            //
                            // TODO(#41688): The resulting error message fails to include the file
                            // position of the import statement (because that information is not
                            // tracked by the module loader). Figure out how to plumb the import
                            // position through.
                            pkg.err = addr(new DirectImportFromImplicitDependencyError(ImporterPath:pkg.path,ImportedPath:dep.path,Module:dep.mod,)); 
                            // cfg.BuildMod does not allow us to change dep.mod to be a direct
                            // dependency, so don't mark it as such.
                            continue;
                        }

                    }
                } 

                // dep is a package directly imported by a package or test in the main
                // module and loaded from some other module (not the standard library).
                // Mark its module as a direct dependency.
                direct[dep.mod.Path] = true;
            }
        }
        pkg = pkg__prev1;
    }

    slice<module.Version> addRoots = default;
    if (ld.Tidy) { 
        // When we are tidying a lazy module, we may need to add roots to preserve
        // the versions of indirect, test-only dependencies that are upgraded
        // above or otherwise missing from the go.mod files of direct
        // dependencies. (For example, the direct dependency might be a very
        // stable codebase that predates modules and thus lacks a go.mod file, or
        // the author of the direct dependency may have forgotten to commit a
        // change to the go.mod file, or may have made an erroneous hand-edit that
        // causes it to be untidy.)
        //
        // Promoting an indirect dependency to a root adds the next layer of its
        // dependencies to the module graph, which may increase the selected
        // versions of other modules from which we have already loaded packages.
        // So after we promote an indirect dependency to a root, we need to reload
        // packages, which means another iteration of loading.
        //
        // As an extra wrinkle, the upgrades due to promoting a root can cause
        // previously-resolved packages to become unresolved. For example, the
        // module providing an unstable package might be upgraded to a version
        // that no longer contains that package. If we then resolve the missing
        // package, we might add yet another root that upgrades away some other
        // dependency. (The tests in mod_tidy_convergence*.txt illustrate some
        // particularly worrisome cases.)
        //
        // To ensure that this process of promoting, adding, and upgrading roots
        // eventually terminates, during iteration we only ever add modules to the
        // root set — we only remove irrelevant roots at the very end of
        // iteration, after we have already added every root that we plan to need
        // in the (eventual) tidy root set.
        //
        // Since we do not remove any roots during iteration, even if they no
        // longer provide any imported packages, the selected versions of the
        // roots can only increase and the set of roots can only expand. The set
        // of extant root paths is finite and the set of versions of each path is
        // finite, so the iteration *must* reach a stable fixed-point.
        var (tidy, err) = tidyRoots(ctx, rs, ld.pkgs);
        if (err != null) {
            return (false, error.As(err)!);
        }
        addRoots = tidy.rootModules;
    }
    rs, err = updateRoots(ctx, direct, rs, ld.pkgs, addRoots, ld.AssumeRootsImported);
    if (err != null) { 
        // We don't actually know what even the root requirements are supposed to be,
        // so we can't proceed with loading. Return the error to the caller
        return (false, error.As(err)!);
    }
    if (rs != ld.requirements && !reflect.DeepEqual(rs.rootModules, ld.requirements.rootModules)) { 
        // The roots of the module graph have changed in some way (not just the
        // "direct" markings). Check whether the changes affected any of the loaded
        // packages.
        var (mg, err) = rs.Graph(ctx);
        if (err != null) {
            return (false, error.As(err)!);
        }
        {
            var pkg__prev1 = pkg;

            foreach (var (_, __pkg) in ld.pkgs) {
                pkg = __pkg;
                if (pkg.fromExternalModule() && mg.Selected(pkg.mod.Path) != pkg.mod.Version) {
                    changed = true;
                    break;
                }
                if (pkg.err != null) { 
                    // Promoting a module to a root may resolve an import that was
                    // previously missing (by pulling in a previously-prune dependency that
                    // provides it) or ambiguous (by promoting exactly one of the
                    // alternatives to a root and ignoring the second-level alternatives) or
                    // otherwise errored out (by upgrading from a version that cannot be
                    // fetched to one that can be).
                    //
                    // Instead of enumerating all of the possible errors, we'll just check
                    // whether importFromModules returns nil for the package.
                    // False-positives are ok: if we have a false-positive here, we'll do an
                    // extra iteration of package loading this time, but we'll still
                    // converge when the root set stops changing.
                    //
                    // In some sense, we can think of this as ‘upgraded the module providing
                    // pkg.path from "none" to a version higher than "none"’.
                    _, _, err = importFromModules(ctx, pkg.path, rs, null);

                    if (err == null) {
                        changed = true;
                        break;
                    }
                }
            }

            pkg = pkg__prev1;
        }
    }
    ld.requirements = rs;
    return (changed, error.As(null!)!);
}

// resolveMissingImports returns a set of modules that could be added as
// dependencies in order to resolve missing packages from pkgs.
//
// The newly-resolved packages are added to the addedModuleFor map, and
// resolveMissingImports returns a map from each new module version to
// the first missing package that module would resolve.
private static map<module.Version, ptr<loadPkg>> resolveMissingImports(this ptr<loader> _addr_ld, context.Context ctx) {
    map<module.Version, ptr<loadPkg>> modAddedBy = default;
    ref loader ld = ref _addr_ld.val;

    private partial struct pkgMod {
        public ptr<loadPkg> pkg;
        public ptr<module.Version> mod;
    }
    slice<pkgMod> pkgMods = default;
    {
        var pkg__prev1 = pkg;

        foreach (var (_, __pkg) in ld.pkgs) {
            pkg = __pkg;
            if (pkg.err == null) {
                continue;
            }
            if (pkg.isTest()) { 
                // If we are missing a test, we are also missing its non-test version, and
                // we should only add the missing import once.
                continue;
            }
            if (!errors.As(pkg.err, @new<ImportMissingError.val>())) { 
                // Leave other errors for Import or load.Packages to report.
                continue;
            }
            var pkg = pkg;
            ref module.Version mod = ref heap(out ptr<module.Version> _addr_mod);
            ld.work.Add(() => {
                error err = default!;
                mod, err = queryImport(ctx, pkg.path, ld.requirements);
                if (err != null) { 
                    // pkg.err was already non-nil, so we can reasonably attribute the error
                    // for pkg to either the original error or the one returned by
                    // queryImport. The existing error indicates only that we couldn't find
                    // the package, whereas the query error also explains why we didn't fix
                    // the problem — so we prefer the latter.
                    pkg.err = err;
                } 

                // err is nil, but we intentionally leave pkg.err non-nil and pkg.mod
                // unset: we still haven't satisfied other invariants of a
                // successfully-loaded package, such as scanning and loading the imports
                // of that package. If we succeed in resolving the new dependency graph,
                // the caller can reload pkg and update the error at that point.
                //
                // Even then, the package might not be loaded from the version we've
                // identified here. The module may be upgraded by some other dependency,
                // or by a transitive dependency of mod itself, or — less likely — the
                // package may be rejected by an AllowPackage hook or rendered ambiguous
                // by some other newly-added or newly-upgraded dependency.
            });

            pkgMods = append(pkgMods, new pkgMod(pkg:pkg,mod:&mod));
        }
        pkg = pkg__prev1;
    }

    ld.work.Idle().Receive();

    modAddedBy = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<module.Version, ptr<loadPkg>>{};
    foreach (var (_, pm) in pkgMods) {
        pkg = pm.pkg;
        mod = pm.mod.val;
        if (mod.Path == "") {
            continue;
        }
        fmt.Fprintf(os.Stderr, "go: found %s in %s %s\n", pkg.path, mod.Path, mod.Version);
        if (modAddedBy[mod] == null) {
            modAddedBy[mod] = pkg;
        }
    }    return modAddedBy;
}

// pkg locates the *loadPkg for path, creating and queuing it for loading if
// needed, and updates its state to reflect the given flags.
//
// The imports of the returned *loadPkg will be loaded asynchronously in the
// ld.work queue, and its test (if requested) will also be populated once
// imports have been resolved. When ld.work goes idle, all transitive imports of
// the requested package (and its test, if requested) will have been loaded.
private static ptr<loadPkg> pkg(this ptr<loader> _addr_ld, context.Context ctx, @string path, loadPkgFlags flags) => func((_, panic, _) => {
    ref loader ld = ref _addr_ld.val;

    if (flags.has(pkgImportsLoaded)) {
        panic("internal error: (*loader).pkg called with pkgImportsLoaded flag set");
    }
    pkg = ld.pkgCache.Do(path, () => {
        ptr<loadPkg> pkg = addr(new loadPkg(path:path,));
        ld.applyPkgFlags(ctx, pkg, flags);

        ld.work.Add(() => {
            ld.load(ctx, pkg);
        });
        return _addr_pkg!;
    })._<ptr<loadPkg>>();

    ld.applyPkgFlags(ctx, pkg, flags);
    return _addr_pkg!;
});

// applyPkgFlags updates pkg.flags to set the given flags and propagate the
// (transitive) effects of those flags, possibly loading or enqueueing further
// packages as a result.
private static void applyPkgFlags(this ptr<loader> _addr_ld, context.Context ctx, ptr<loadPkg> _addr_pkg, loadPkgFlags flags) {
    ref loader ld = ref _addr_ld.val;
    ref loadPkg pkg = ref _addr_pkg.val;

    if (flags == 0) {
        return ;
    }
    if (flags.has(pkgInAll) && ld.allPatternIsRoot && !pkg.isTest()) { 
        // This package matches a root pattern by virtue of being in "all".
        flags |= pkgIsRoot;
    }
    if (flags.has(pkgIsRoot)) {
        flags |= pkgFromRoot;
    }
    var old = pkg.flags.update(flags);
    var @new = old | flags;
    if (new == old || !@new.has(pkgImportsLoaded)) { 
        // We either didn't change the state of pkg, or we don't know anything about
        // its dependencies yet. Either way, we can't usefully load its test or
        // update its dependencies.
        return ;
    }
    if (!pkg.isTest()) { 
        // Check whether we should add (or update the flags for) a test for pkg.
        // ld.pkgTest is idempotent and extra invocations are inexpensive,
        // so it's ok if we call it more than is strictly necessary.
        var wantTest = false;

        if (ld.allPatternIsRoot && pkg.mod == Target) 
            // We are loading the "all" pattern, which includes packages imported by
            // tests in the main module. This package is in the main module, so we
            // need to identify the imports of its test even if LoadTests is not set.
            //
            // (We will filter out the extra tests explicitly in computePatternAll.)
            wantTest = true;
        else if (ld.allPatternIsRoot && ld.allClosesOverTests && @new.has(pkgInAll)) 
            // This variant of the "all" pattern includes imports of tests of every
            // package that is itself in "all", and pkg is in "all", so its test is
            // also in "all" (as above).
            wantTest = true;
        else if (ld.LoadTests && @new.has(pkgIsRoot)) 
            // LoadTest explicitly requests tests of “the root packages”.
            wantTest = true;
                if (wantTest) {
            loadPkgFlags testFlags = default;
            if (pkg.mod == Target || (ld.allClosesOverTests && @new.has(pkgInAll))) { 
                // Tests of packages in the main module are in "all", in the sense that
                // they cause the packages they import to also be in "all". So are tests
                // of packages in "all" if "all" closes over test dependencies.
                testFlags |= pkgInAll;
            }
            ld.pkgTest(ctx, pkg, testFlags);
        }
    }
    if (@new.has(pkgInAll) && !old.has(pkgInAll | pkgImportsLoaded)) { 
        // We have just marked pkg with pkgInAll, or we have just loaded its
        // imports, or both. Now is the time to propagate pkgInAll to the imports.
        {
            var dep__prev1 = dep;

            foreach (var (_, __dep) in pkg.imports) {
                dep = __dep;
                ld.applyPkgFlags(ctx, dep, pkgInAll);
            }

            dep = dep__prev1;
        }
    }
    if (@new.has(pkgFromRoot) && !old.has(pkgFromRoot | pkgImportsLoaded)) {
        {
            var dep__prev1 = dep;

            foreach (var (_, __dep) in pkg.imports) {
                dep = __dep;
                ld.applyPkgFlags(ctx, dep, pkgFromRoot);
            }

            dep = dep__prev1;
        }
    }
}

// preloadRootModules loads the module requirements needed to identify the
// selected version of each module providing a package in rootPkgs,
// adding new root modules to the module graph if needed.
private static bool preloadRootModules(this ptr<loader> _addr_ld, context.Context ctx, slice<@string> rootPkgs) => func((_, panic, _) => {
    bool changedBuildList = default;
    ref loader ld = ref _addr_ld.val;

    var needc = make_channel<map<module.Version, bool>>(1);
    needc.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<module.Version, bool>{});
    {
        var path__prev1 = path;

        foreach (var (_, __path) in rootPkgs) {
            path = __path;
            var path = path;
            ld.work.Add(() => { 
                // First, try to identify the module containing the package using only roots.
                //
                // If the main module is tidy and the package is in "all" — or if we're
                // lucky — we can identify all of its imports without actually loading the
                // full module graph.
                var (m, _, err) = importFromModules(ctx, path, ld.requirements, null);
                if (err != null) {
                    ptr<ImportMissingError> missing;
                    if (errors.As(err, _addr_missing) && ld.ResolveMissingImports) { 
                        // This package isn't provided by any selected module.
                        // If we can find it, it will be a new root dependency.
                        m, err = queryImport(ctx, path, ld.requirements);
                    }
                    if (err != null) { 
                        // We couldn't identify the root module containing this package.
                        // Leave it unresolved; we will report it during loading.
                        return ;
                    }
                }
                if (m.Path == "") { 
                    // The package is in std or cmd. We don't need to change the root set.
                    return ;
                }
                var (v, ok) = ld.requirements.rootSelected(m.Path);
                if (!ok || v != m.Version) { 
                    // We found the requested package in m, but m is not a root, so
                    // loadModGraph will not load its requirements. We need to promote the
                    // module to a root to ensure that any other packages this package
                    // imports are resolved from correct dependency versions.
                    //
                    // (This is the “argument invariant” from the lazy loading design.)
                    var need = needc.Receive();
                    need[m] = true;
                    needc.Send(need);
                }
            });
        }
        path = path__prev1;
    }

    ld.work.Idle().Receive();

    need = needc.Receive();
    if (len(need) == 0) {
        return false; // No roots to add.
    }
    var toAdd = make_slice<module.Version>(0, len(need));
    {
        var m__prev1 = m;

        foreach (var (__m) in need) {
            m = __m;
            toAdd = append(toAdd, m);
        }
        m = m__prev1;
    }

    module.Sort(toAdd);

    var (rs, err) = updateRoots(ctx, ld.requirements.direct, ld.requirements, null, toAdd, ld.AssumeRootsImported);
    if (err != null) { 
        // We are missing some root dependency, and for some reason we can't load
        // enough of the module dependency graph to add the missing root. Package
        // loading is doomed to fail, so fail quickly.
        ld.errorf("go: %v\n", err);
        @base.ExitIfErrors();
        return false;
    }
    if (reflect.DeepEqual(rs.rootModules, ld.requirements.rootModules)) { 
        // Something is deeply wrong. resolveMissingImports gave us a non-empty
        // set of modules to add to the graph, but adding those modules had no
        // effect — either they were already in the graph, or updateRoots did not
        // add them as requested.
        panic(fmt.Sprintf("internal error: adding %v to module graph had no effect on root requirements (%v)", toAdd, rs.rootModules));
    }
    ld.requirements = rs;
    return true;
});

// load loads an individual package.
private static void load(this ptr<loader> _addr_ld, context.Context ctx, ptr<loadPkg> _addr_pkg) {
    ref loader ld = ref _addr_ld.val;
    ref loadPkg pkg = ref _addr_pkg.val;

    if (strings.Contains(pkg.path, "@")) { 
        // Leave for error during load.
        return ;
    }
    if (build.IsLocalImport(pkg.path) || filepath.IsAbs(pkg.path)) { 
        // Leave for error during load.
        // (Module mode does not allow local imports.)
        return ;
    }
    if (search.IsMetaPackage(pkg.path)) {
        pkg.err = addr(new invalidImportError(importPath:pkg.path,err:fmt.Errorf("%q is not an importable package; see 'go help packages'",pkg.path),));
        return ;
    }
    ptr<ModuleGraph> mg;
    if (ld.requirements.depth == eager) {
        error err = default!;
        mg, err = ld.requirements.Graph(ctx);
        if (err != null) { 
            // We already checked the error from Graph in loadFromRoots and/or
            // updateRequirements, so we ignored the error on purpose and we should
            // keep trying to push past it.
            //
            // However, because mg may be incomplete (and thus may select inaccurate
            // versions), we shouldn't use it to load packages. Instead, we pass a nil
            // *ModuleGraph, which will cause mg to first try loading from only the
            // main module and root dependencies.
            mg = null;
        }
    }
    pkg.mod, pkg.dir, pkg.err = importFromModules(ctx, pkg.path, ld.requirements, mg);
    if (pkg.dir == "") {
        return ;
    }
    if (pkg.mod == Target) { 
        // Go ahead and mark pkg as in "all". This provides the invariant that a
        // package that is *only* imported by other packages in "all" is always
        // marked as such before loading its imports.
        //
        // We don't actually rely on that invariant at the moment, but it may
        // improve efficiency somewhat and makes the behavior a bit easier to reason
        // about (by reducing churn on the flag bits of dependencies), and costs
        // essentially nothing (these atomic flag ops are essentially free compared
        // to scanning source code for imports).
        ld.applyPkgFlags(ctx, pkg, pkgInAll);
    }
    if (ld.AllowPackage != null) {
        {
            error err__prev2 = err;

            err = ld.AllowPackage(ctx, pkg.path, pkg.mod);

            if (err != null) {
                pkg.err = err;
            }

            err = err__prev2;

        }
    }
    pkg.inStd = (search.IsStandardImportPath(pkg.path) && search.InDir(pkg.dir, cfg.GOROOTsrc) != "");

    slice<@string> imports = default;    slice<@string> testImports = default;



    if (cfg.BuildContext.Compiler == "gccgo" && pkg.inStd) { 
        // We can't scan standard packages for gccgo.
    }
    else
 {
        err = default!;
        imports, testImports, err = scanDir(pkg.dir, ld.Tags);
        if (err != null) {
            pkg.err = err;
            return ;
        }
    }
    pkg.imports = make_slice<ptr<loadPkg>>(0, len(imports));
    loadPkgFlags importFlags = default;
    if (pkg.flags.has(pkgInAll)) {
        importFlags = pkgInAll;
    }
    foreach (var (_, path) in imports) {
        if (pkg.inStd) { 
            // Imports from packages in "std" and "cmd" should resolve using
            // GOROOT/src/vendor even when "std" is not the main module.
            path = ld.stdVendor(pkg.path, path);
        }
        pkg.imports = append(pkg.imports, ld.pkg(ctx, path, importFlags));
    }    pkg.testImports = testImports;

    ld.applyPkgFlags(ctx, pkg, pkgImportsLoaded);
}

// pkgTest locates the test of pkg, creating it if needed, and updates its state
// to reflect the given flags.
//
// pkgTest requires that the imports of pkg have already been loaded (flagged
// with pkgImportsLoaded).
private static ptr<loadPkg> pkgTest(this ptr<loader> _addr_ld, context.Context ctx, ptr<loadPkg> _addr_pkg, loadPkgFlags testFlags) => func((_, panic, _) => {
    ref loader ld = ref _addr_ld.val;
    ref loadPkg pkg = ref _addr_pkg.val;

    if (pkg.isTest()) {
        panic("pkgTest called on a test package");
    }
    var createdTest = false;
    pkg.testOnce.Do(() => {
        pkg.test = addr(new loadPkg(path:pkg.path,testOf:pkg,mod:pkg.mod,dir:pkg.dir,err:pkg.err,inStd:pkg.inStd,));
        ld.applyPkgFlags(ctx, pkg.test, testFlags);
        createdTest = true;
    });

    var test = pkg.test;
    if (createdTest) {
        test.imports = make_slice<ptr<loadPkg>>(0, len(pkg.testImports));
        loadPkgFlags importFlags = default;
        if (test.flags.has(pkgInAll)) {
            importFlags = pkgInAll;
        }
        foreach (var (_, path) in pkg.testImports) {
            if (pkg.inStd) {
                path = ld.stdVendor(test.path, path);
            }
            test.imports = append(test.imports, ld.pkg(ctx, path, importFlags));
        }
    else
        pkg.testImports = null;
        ld.applyPkgFlags(ctx, test, pkgImportsLoaded);
    } {
        ld.applyPkgFlags(ctx, test, testFlags);
    }
    return _addr_test!;
});

// stdVendor returns the canonical import path for the package with the given
// path when imported from the standard-library package at parentPath.
private static @string stdVendor(this ptr<loader> _addr_ld, @string parentPath, @string path) {
    ref loader ld = ref _addr_ld.val;

    if (search.IsStandardImportPath(path)) {
        return path;
    }
    if (str.HasPathPrefix(parentPath, "cmd")) {
        if (!ld.VendorModulesInGOROOTSrc || Target.Path != "cmd") {
            var vendorPath = pathpkg.Join("cmd", "vendor", path);
            {
                var (_, err) = os.Stat(filepath.Join(cfg.GOROOTsrc, filepath.FromSlash(vendorPath)));

                if (err == null) {
                    return vendorPath;
                }

            }
        }
    }
    else if (!ld.VendorModulesInGOROOTSrc || Target.Path != "std" || str.HasPathPrefix(parentPath, "vendor")) { 
        // If we are outside of the 'std' module, resolve imports from within 'std'
        // to the vendor directory.
        //
        // Do the same for importers beginning with the prefix 'vendor/' even if we
        // are *inside* of the 'std' module: the 'vendor/' packages that resolve
        // globally from GOROOT/src/vendor (and are listed as part of 'go list std')
        // are distinct from the real module dependencies, and cannot import
        // internal packages from the real module.
        //
        // (Note that although the 'vendor/' packages match the 'std' *package*
        // pattern, they are not part of the std *module*, and do not affect
        // 'go mod tidy' and similar module commands when working within std.)
        vendorPath = pathpkg.Join("vendor", path);
        {
            (_, err) = os.Stat(filepath.Join(cfg.GOROOTsrc, filepath.FromSlash(vendorPath)));

            if (err == null) {
                return vendorPath;
            }

        }
    }
    return path;
}

// computePatternAll returns the list of packages matching pattern "all",
// starting with a list of the import paths for the packages in the main module.
private static slice<@string> computePatternAll(this ptr<loader> _addr_ld) {
    slice<@string> all = default;
    ref loader ld = ref _addr_ld.val;

    foreach (var (_, pkg) in ld.pkgs) {
        if (pkg.flags.has(pkgInAll) && !pkg.isTest()) {
            all = append(all, pkg.path);
        }
    }    sort.Strings(all);
    return all;
}

// checkMultiplePaths verifies that a given module path is used as itself
// or as a replacement for another module, but not both at the same time.
//
// (See https://golang.org/issue/26607 and https://golang.org/issue/34650.)
private static void checkMultiplePaths(this ptr<loader> _addr_ld) {
    ref loader ld = ref _addr_ld.val;

    var mods = ld.requirements.rootModules;
    {
        var cached = ld.requirements.graph.Load();

        if (cached != null) {
            {
                cachedGraph mg = cached._<cachedGraph>().mg;

                if (mg != null) {
                    mods = mg.BuildList();
                }

            }
        }
    }

    map firstPath = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<module.Version, @string>{};
    foreach (var (_, mod) in mods) {
        var src = resolveReplacement(mod);
        {
            var (prev, ok) = firstPath[src];

            if (!ok) {
                firstPath[src] = mod.Path;
            }
            else if (prev != mod.Path) {
                ld.errorf("go: %s@%s used for two different module paths (%s and %s)\n", src.Path, src.Version, prev, mod.Path);
            }

        }
    }
}

// checkTidyCompatibility emits an error if any package would be loaded from a
// different module under rs than under ld.requirements.
private static void checkTidyCompatibility(this ptr<loader> _addr_ld, context.Context ctx, ptr<Requirements> _addr_rs) {
    ref loader ld = ref _addr_ld.val;
    ref Requirements rs = ref _addr_rs.val;

    var suggestUpgrade = false;
    var suggestEFlag = false;
    Action suggestFixes = () => {
        if (ld.AllowErrors) { 
            // The user is explicitly ignoring these errors, so don't bother them with
            // other options.
            return ;
        }
        fmt.Fprintln(os.Stderr);

        @string goFlag = "";
        if (ld.GoVersion != modFileGoVersion()) {
            goFlag = " -go=" + ld.GoVersion;
        }
        @string compatFlag = "";
        if (ld.TidyCompatibleVersion != priorGoVersion(ld.GoVersion)) {
            compatFlag = " -compat=" + ld.TidyCompatibleVersion;
        }
        if (suggestUpgrade) {
            @string eDesc = "";
            @string eFlag = "";
            if (suggestEFlag) {
                eDesc = ", leaving some packages unresolved";
                eFlag = " -e";
            }
            fmt.Fprintf(os.Stderr, "To upgrade to the versions selected by go %s%s:\n\tgo mod tidy%s -go=%s && go mod tidy%s -go=%s%s\n", ld.TidyCompatibleVersion, eDesc, eFlag, ld.TidyCompatibleVersion, eFlag, ld.GoVersion, compatFlag);
        }
        else if (suggestEFlag) { 
            // If some packages are missing but no package is upgraded, then we
            // shouldn't suggest upgrading to the Go 1.16 versions explicitly — that
            // wouldn't actually fix anything for Go 1.16 users, and *would* break
            // something for Go 1.17 users.
            fmt.Fprintf(os.Stderr, "To proceed despite packages unresolved in go %s:\n\tgo mod tidy -e%s%s\n", ld.TidyCompatibleVersion, goFlag, compatFlag);
        }
        fmt.Fprintf(os.Stderr, "If reproducibility with go %s is not needed:\n\tgo mod tidy%s -compat=%s\n", ld.TidyCompatibleVersion, goFlag, ld.GoVersion); 

        // TODO(#46141): Populate the linked wiki page.
        fmt.Fprintf(os.Stderr, "For other options, see:\n\thttps://golang.org/doc/modules/pruning\n");
    };

    var (mg, err) = rs.Graph(ctx);
    if (err != null) {
        ld.errorf("go mod tidy: error loading go %s module graph: %v\n", ld.TidyCompatibleVersion, err);
        suggestFixes();
        return ;
    }
    private partial struct mismatch {
        public module.Version mod;
        public error err;
    }
    var mismatchMu = make_channel<map<ptr<loadPkg>, mismatch>>(1);
    mismatchMu.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<loadPkg>, mismatch>{});
    {
        var pkg__prev1 = pkg;

        foreach (var (_, __pkg) in ld.pkgs) {
            pkg = __pkg;
            if (pkg.mod.Path == "" && pkg.err == null) { 
                // This package is from the standard library (which does not vary based on
                // the module graph).
                continue;
            }
            var pkg = pkg;
            ld.work.Add(() => {
                var (mod, _, err) = importFromModules(ctx, pkg.path, rs, mg);
                if (mod != pkg.mod) {
                    var mismatches = mismatchMu.Receive();
                    mismatches[pkg] = new mismatch(mod:mod,err:err);
                    mismatchMu.Send(mismatches);
                }
            });
        }
        pkg = pkg__prev1;
    }

    ld.work.Idle().Receive();

    mismatches = mismatchMu.Receive();
    if (len(mismatches) == 0) { 
        // Since we're running as part of 'go mod tidy', the roots of the module
        // graph should contain only modules that are relevant to some package in
        // the package graph. We checked every package in the package graph and
        // didn't find any mismatches, so that must mean that all of the roots of
        // the module graph are also consistent.
        //
        // If we're wrong, Go 1.16 in -mod=readonly mode will error out with
        // "updates to go.mod needed", which would be very confusing. So instead,
        // we'll double-check that our reasoning above actually holds — if it
        // doesn't, we'll emit an internal error and hopefully the user will report
        // it as a bug.
        {
            var m__prev1 = m;

            foreach (var (_, __m) in ld.requirements.rootModules) {
                m = __m;
                {
                    var v__prev2 = v;

                    var v = mg.Selected(m.Path);

                    if (v != m.Version) {
                        fmt.Fprintln(os.Stderr);
                        @base.Fatalf("go: internal error: failed to diagnose selected-version mismatch for module %s: go %s selects %s, but go %s selects %s\n\tPlease report this at https://golang.org/issue.", m.Path, ld.GoVersion, m.Version, ld.TidyCompatibleVersion, v);
                    }

                    v = v__prev2;

                }
            }

            m = m__prev1;
        }

        return ;
    }
    {
        var pkg__prev1 = pkg;

        foreach (var (_, __pkg) in ld.pkgs) {
            pkg = __pkg;
            var (mismatch, ok) = mismatches[pkg];
            if (!ok) {
                continue;
            }
            if (pkg.isTest()) { 
                // We already did (or will) report an error for the package itself,
                // so don't report a duplicate (and more vebose) error for its test.
                {
                    var (_, ok) = mismatches[pkg.testOf];

                    if (!ok) {
                        @base.Fatalf("go: internal error: mismatch recorded for test %s, but not its non-test package", pkg.path);
                    }

                }
                continue;
            }

            if (mismatch.err != null) 
                // pkg resolved successfully, but errors out using the requirements in rs.
                //
                // This could occur because the import is provided by a single lazy root
                // (and is thus unambiguous in lazy mode) and also one or more
                // transitive dependencies (and is ambiguous in eager mode).
                //
                // It could also occur because some transitive dependency upgrades the
                // module that previously provided the package to a version that no
                // longer does, or to a version for which the module source code (but
                // not the go.mod file in isolation) has a checksum error.
                {
                    ref var missing = ref heap((ImportMissingError.val)(null), out ptr<var> _addr_missing);

                    if (errors.As(mismatch.err, _addr_missing)) {
                        module.Version selected = new module.Version(Path:pkg.mod.Path,Version:mg.Selected(pkg.mod.Path),);
                        ld.errorf("%s loaded from %v,\n\tbut go %s would fail to locate it in %s\n", pkg.stackText(), pkg.mod, ld.TidyCompatibleVersion, selected);
                    }
                    else
 {
                        {
                            ref var ambiguous = ref heap((AmbiguousImportError.val)(null), out ptr<var> _addr_ambiguous);

                            if (errors.As(mismatch.err, _addr_ambiguous)) { 
                                // TODO: Is this check needed?
                            }

                        }
                        ld.errorf("%s loaded from %v,\n\tbut go %s would fail to locate it:\n\t%v\n", pkg.stackText(), pkg.mod, ld.TidyCompatibleVersion, mismatch.err);
                    }

                }

                suggestEFlag = true; 

                // Even if we press ahead with the '-e' flag, the older version will
                // error out in readonly mode if it thinks the go.mod file contains
                // any *explicit* dependency that is not at its selected version,
                // even if that dependency is not relevant to any package being loaded.
                //
                // We check for that condition here. If all of the roots are consistent
                // the '-e' flag suffices, but otherwise we need to suggest an upgrade.
                if (!suggestUpgrade) {
                    {
                        var m__prev2 = m;

                        foreach (var (_, __m) in ld.requirements.rootModules) {
                            m = __m;
                            {
                                var v__prev2 = v;

                                v = mg.Selected(m.Path);

                                if (v != m.Version) {
                                    suggestUpgrade = true;
                                    break;
                                }

                                v = v__prev2;

                            }
                        }

                        m = m__prev2;
                    }
                }
            else if (pkg.err != null) 
                // pkg had an error in lazy mode (presumably suppressed with the -e flag),
                // but not in eager mode.
                //
                // This is possible, if, say, the import is unresolved in lazy mode
                // (because the "latest" version of each candidate module either is
                // unavailable or does not contain the package), but is resolved in
                // eager mode due to a newer-than-latest dependency that is normally
                // runed out of the module graph.
                //
                // This could also occur if the source code for the module providing the
                // package in lazy mode has a checksum error, but eager mode upgrades
                // that module to a version with a correct checksum.
                //
                // pkg.err should have already been logged elsewhere — along with a
                // stack trace — so log only the import path and non-error info here.
                suggestUpgrade = true;
                ld.errorf("%s failed to load from any module,\n\tbut go %s would load it from %v\n", pkg.path, ld.TidyCompatibleVersion, mismatch.mod);
            else if (pkg.mod != mismatch.mod) 
                // The package is loaded successfully by both Go versions, but from a
                // different module in each. This could lead to subtle (and perhaps even
                // unnoticed!) variations in behavior between builds with different
                // toolchains.
                suggestUpgrade = true;
                ld.errorf("%s loaded from %v,\n\tbut go %s would select %v\n", pkg.stackText(), pkg.mod, ld.TidyCompatibleVersion, mismatch.mod.Version);
            else 
                @base.Fatalf("go: internal error: mismatch recorded for package %s, but no differences found", pkg.path);
                    }
        pkg = pkg__prev1;
    }

    suggestFixes();
    @base.ExitIfErrors();
}

// scanDir is like imports.ScanDir but elides known magic imports from the list,
// so that we do not go looking for packages that don't really exist.
//
// The standard magic import is "C", for cgo.
//
// The only other known magic imports are appengine and appengine/*.
// These are so old that they predate "go get" and did not use URL-like paths.
// Most code today now uses google.golang.org/appengine instead,
// but not all code has been so updated. When we mostly ignore build tags
// during "go vendor", we look into "// +build appengine" files and
// may see these legacy imports. We drop them so that the module
// search does not look for modules to try to satisfy them.
private static (slice<@string>, slice<@string>, error) scanDir(@string dir, map<@string, bool> tags) {
    slice<@string> imports_ = default;
    slice<@string> testImports = default;
    error err = default!;

    imports_, testImports, err = imports.ScanDir(dir, tags);

    Func<slice<@string>, slice<@string>> filter = x => {
        nint w = 0;
        foreach (var (_, pkg) in x) {
            if (pkg != "C" && pkg != "appengine" && !strings.HasPrefix(pkg, "appengine/") && pkg != "appengine_internal" && !strings.HasPrefix(pkg, "appengine_internal/")) {
                x[w] = pkg;
                w++;
            }
        }        return x[..(int)w];
    };

    return (filter(imports_), filter(testImports), error.As(err)!);
}

// buildStacks computes minimal import stacks for each package,
// for use in error messages. When it completes, packages that
// are part of the original root set have pkg.stack == nil,
// and other packages have pkg.stack pointing at the next
// package up the import stack in their minimal chain.
// As a side effect, buildStacks also constructs ld.pkgs,
// the list of all packages loaded.
private static void buildStacks(this ptr<loader> _addr_ld) => func((_, panic, _) => {
    ref loader ld = ref _addr_ld.val;

    if (len(ld.pkgs) > 0) {
        panic("buildStacks");
    }
    {
        var pkg__prev1 = pkg;

        foreach (var (_, __pkg) in ld.roots) {
            pkg = __pkg;
            pkg.stack = pkg; // sentinel to avoid processing in next loop
            ld.pkgs = append(ld.pkgs, pkg);
        }
        pkg = pkg__prev1;
    }

    for (nint i = 0; i < len(ld.pkgs); i++) { // not range: appending to ld.pkgs in loop
        var pkg = ld.pkgs[i];
        {
            var next__prev2 = next;

            foreach (var (_, __next) in pkg.imports) {
                next = __next;
                if (next.stack == null) {
                    next.stack = pkg;
                    ld.pkgs = append(ld.pkgs, next);
                }
            }

            next = next__prev2;
        }

        {
            var next__prev1 = next;

            var next = pkg.test;

            if (next != null && next.stack == null) {
                next.stack = pkg;
                ld.pkgs = append(ld.pkgs, next);
            }

            next = next__prev1;

        }
    }
    {
        var pkg__prev1 = pkg;

        foreach (var (_, __pkg) in ld.roots) {
            pkg = __pkg;
            pkg.stack = null;
        }
        pkg = pkg__prev1;
    }
});

// stackText builds the import stack text to use when
// reporting an error in pkg. It has the general form
//
//    root imports
//        other imports
//        other2 tested by
//        other2.test imports
//        pkg
//
private static @string stackText(this ptr<loadPkg> _addr_pkg) {
    ref loadPkg pkg = ref _addr_pkg.val;

    slice<ptr<loadPkg>> stack = default;
    {
        var p__prev1 = p;

        var p = pkg;

        while (p != null) {
            stack = append(stack, p);
            p = p.stack;
        }

        p = p__prev1;
    }

    ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
    for (var i = len(stack) - 1; i >= 0; i--) {
        p = stack[i];
        fmt.Fprint(_addr_buf, p.path);
        if (p.testOf != null) {
            fmt.Fprint(_addr_buf, ".test");
        }
        if (i > 0) {
            if (stack[i - 1].testOf == p) {
                fmt.Fprint(_addr_buf, " tested by\n\t");
            }
            else
 {
                fmt.Fprint(_addr_buf, " imports\n\t");
            }
        }
    }
    return buf.String();
}

// why returns the text to use in "go mod why" output about the given package.
// It is less ornate than the stackText but contains the same information.
private static @string why(this ptr<loadPkg> _addr_pkg) {
    ref loadPkg pkg = ref _addr_pkg.val;

    ref strings.Builder buf = ref heap(out ptr<strings.Builder> _addr_buf);
    slice<ptr<loadPkg>> stack = default;
    {
        var p__prev1 = p;

        var p = pkg;

        while (p != null) {
            stack = append(stack, p);
            p = p.stack;
        }

        p = p__prev1;
    }

    for (var i = len(stack) - 1; i >= 0; i--) {
        p = stack[i];
        if (p.testOf != null) {
            fmt.Fprintf(_addr_buf, "%s.test\n", p.testOf.path);
        }
        else
 {
            fmt.Fprintf(_addr_buf, "%s\n", p.path);
        }
    }
    return buf.String();
}

// Why returns the "go mod why" output stanza for the given package,
// without the leading # comment.
// The package graph must have been loaded already, usually by LoadPackages.
// If there is no reason for the package to be in the current build,
// Why returns an empty string.
public static @string Why(@string path) {
    ptr<loadPkg> (pkg, ok) = loaded.pkgCache.Get(path)._<ptr<loadPkg>>();
    if (!ok) {
        return "";
    }
    return pkg.why();
}

// WhyDepth returns the number of steps in the Why listing.
// If there is no reason for the package to be in the current build,
// WhyDepth returns 0.
public static nint WhyDepth(@string path) {
    nint n = 0;
    ptr<loadPkg> (pkg, _) = loaded.pkgCache.Get(path)._<ptr<loadPkg>>();
    {
        var p = pkg;

        while (p != null) {
            n++;
            p = p.stack;
        }
    }
    return n;
}

} // end modload_package
