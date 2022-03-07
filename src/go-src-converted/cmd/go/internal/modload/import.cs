// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modload -- go2cs converted at 2022 March 06 23:18:06 UTC
// import "cmd/go/internal/modload" ==> using modload = go.cmd.go.@internal.modload_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modload\import.go
using context = go.context_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using build = go.go.build_package;
using goroot = go.@internal.goroot_package;
using fs = go.io.fs_package;
using os = go.os_package;
using pathpkg = go.path_package;
using filepath = go.path.filepath_package;
using sort = go.sort_package;
using strings = go.strings_package;

using cfg = go.cmd.go.@internal.cfg_package;
using fsys = go.cmd.go.@internal.fsys_package;
using modfetch = go.cmd.go.@internal.modfetch_package;
using par = go.cmd.go.@internal.par_package;
using search = go.cmd.go.@internal.search_package;

using module = go.golang.org.x.mod.module_package;
using semver = go.golang.org.x.mod.semver_package;
using System;


namespace go.cmd.go.@internal;

public static partial class modload_package {

public partial struct ImportMissingError {
    public @string Path;
    public module.Version Module;
    public error QueryErr; // isStd indicates whether we would expect to find the package in the standard
// library. This is normally true for all dotless import paths, but replace
// directives can cause us to treat the replaced paths as also being in
// modules.
    public bool isStd; // replaced the highest replaced version of the module where the replacement
// contains the package. replaced is only set if the replacement is unused.
    public module.Version replaced; // newMissingVersion is set to a newer version of Module if one is present
// in the build list. When set, we can't automatically upgrade.
    public @string newMissingVersion;
}

private static @string Error(this ptr<ImportMissingError> _addr_e) {
    ref ImportMissingError e = ref _addr_e.val;

    if (e.Module.Path == "") {
        if (e.isStd) {
            return fmt.Sprintf("package %s is not in GOROOT (%s)", e.Path, filepath.Join(cfg.GOROOT, "src", e.Path));
        }
        if (e.QueryErr != null && e.QueryErr != ErrNoModRoot) {
            return fmt.Sprintf("cannot find module providing package %s: %v", e.Path, e.QueryErr);
        }
        if (cfg.BuildMod == "mod" || (cfg.BuildMod == "readonly" && allowMissingModuleImports)) {
            return "cannot find module providing package " + e.Path;
        }
        if (e.replaced.Path != "") {
            var suggestArg = e.replaced.Path;
            if (!module.IsZeroPseudoVersion(e.replaced.Version)) {
                suggestArg = e.replaced.String();
            }
            return fmt.Sprintf("module %s provides package %s and is replaced but not required; to add it:\n\tgo get %s", e.replaced.Path, e.Path, suggestArg);
        }
        var message = fmt.Sprintf("no required module provides package %s", e.Path);
        if (e.QueryErr != null) {
            return fmt.Sprintf("%s: %v", message, e.QueryErr);
        }
        return fmt.Sprintf("%s; to add it:\n\tgo get %s", message, e.Path);

    }
    if (e.newMissingVersion != "") {
        return fmt.Sprintf("package %s provided by %s at latest version %s but not at required version %s", e.Path, e.Module.Path, e.Module.Version, e.newMissingVersion);
    }
    return fmt.Sprintf("missing module for import: %s@%s provides %s", e.Module.Path, e.Module.Version, e.Path);

}

private static error Unwrap(this ptr<ImportMissingError> _addr_e) {
    ref ImportMissingError e = ref _addr_e.val;

    return error.As(e.QueryErr)!;
}

private static @string ImportPath(this ptr<ImportMissingError> _addr_e) {
    ref ImportMissingError e = ref _addr_e.val;

    return e.Path;
}

// An AmbiguousImportError indicates an import of a package found in multiple
// modules in the build list, or found in both the main module and its vendor
// directory.
public partial struct AmbiguousImportError {
    public @string importPath;
    public slice<@string> Dirs;
    public slice<module.Version> Modules; // Either empty or 1:1 with Dirs.
}

private static @string ImportPath(this ptr<AmbiguousImportError> _addr_e) {
    ref AmbiguousImportError e = ref _addr_e.val;

    return e.importPath;
}

private static @string Error(this ptr<AmbiguousImportError> _addr_e) {
    ref AmbiguousImportError e = ref _addr_e.val;

    @string locType = "modules";
    if (len(e.Modules) == 0) {
        locType = "directories";
    }
    ref strings.Builder buf = ref heap(out ptr<strings.Builder> _addr_buf);
    fmt.Fprintf(_addr_buf, "ambiguous import: found package %s in multiple %s:", e.importPath, locType);

    foreach (var (i, dir) in e.Dirs) {
        buf.WriteString("\n\t");
        if (i < len(e.Modules)) {
            var m = e.Modules[i];
            buf.WriteString(m.Path);
            if (m.Version != "") {
                fmt.Fprintf(_addr_buf, " %s", m.Version);
            }
            fmt.Fprintf(_addr_buf, " (%s)", dir);
        }
        else
 {
            buf.WriteString(dir);
        }
    }    return buf.String();

}

// A DirectImportFromImplicitDependencyError indicates a package directly
// imported by a package or test in the main module that is satisfied by a
// dependency that is not explicit in the main module's go.mod file.
public partial struct DirectImportFromImplicitDependencyError {
    public @string ImporterPath;
    public @string ImportedPath;
    public module.Version Module;
}

private static @string Error(this ptr<DirectImportFromImplicitDependencyError> _addr_e) {
    ref DirectImportFromImplicitDependencyError e = ref _addr_e.val;

    return fmt.Sprintf("package %s imports %s from implicitly required module; to add missing requirements, run:\n\tgo get %s@%s", e.ImporterPath, e.ImportedPath, e.Module.Path, e.Module.Version);
}

private static @string ImportPath(this ptr<DirectImportFromImplicitDependencyError> _addr_e) {
    ref DirectImportFromImplicitDependencyError e = ref _addr_e.val;

    return e.ImporterPath;
}

// ImportMissingSumError is reported in readonly mode when we need to check
// if a module contains a package, but we don't have a sum for its .zip file.
// We might need sums for multiple modules to verify the package is unique.
//
// TODO(#43653): consolidate multiple errors of this type into a single error
// that suggests a 'go get' command for root packages that transtively import
// packages from modules with missing sums. load.CheckPackageErrors would be
// a good place to consolidate errors, but we'll need to attach the import
// stack here.
public partial struct ImportMissingSumError {
    public @string importPath;
    public bool found;
    public slice<module.Version> mods;
    public @string importer; // optional, but used for additional context
    public @string importerVersion; // optional, but used for additional context
    public bool importerIsTest;
}

private static @string Error(this ptr<ImportMissingSumError> _addr_e) {
    ref ImportMissingSumError e = ref _addr_e.val;

    @string importParen = default;
    if (e.importer != "") {
        importParen = fmt.Sprintf(" (imported by %s)", e.importer);
    }
    @string message = default;
    if (e.found) {
        message = fmt.Sprintf("missing go.sum entry needed to verify package %s%s is provided by exactly one module", e.importPath, importParen);
    }
    else
 {
        message = fmt.Sprintf("missing go.sum entry for module providing package %s%s", e.importPath, importParen);
    }
    @string hint = default;
    if (e.importer == "") { 
        // Importing package is unknown, or the missing package was named on the
        // command line. Recommend 'go mod download' for the modules that could
        // provide the package, since that shouldn't change go.mod.
        if (len(e.mods) > 0) {
            var args = make_slice<@string>(len(e.mods));
            foreach (var (i, mod) in e.mods) {
                args[i] = mod.Path;
            }
            hint = fmt.Sprintf("; to add:\n\tgo mod download %s", strings.Join(args, " "));
        }
    else
    } { 
        // Importing package is known (common case). Recommend 'go get' on the
        // current version of the importing package.
        @string tFlag = "";
        if (e.importerIsTest) {
            tFlag = " -t";
        }
        @string version = "";
        if (e.importerVersion != "") {
            version = "@" + e.importerVersion;
        }
        hint = fmt.Sprintf("; to add:\n\tgo get%s %s%s", tFlag, e.importer, version);

    }
    return message + hint;

}

private static @string ImportPath(this ptr<ImportMissingSumError> _addr_e) {
    ref ImportMissingSumError e = ref _addr_e.val;

    return e.importPath;
}

private partial struct invalidImportError {
    public @string importPath;
    public error err;
}

private static @string ImportPath(this ptr<invalidImportError> _addr_e) {
    ref invalidImportError e = ref _addr_e.val;

    return e.importPath;
}

private static @string Error(this ptr<invalidImportError> _addr_e) {
    ref invalidImportError e = ref _addr_e.val;

    return e.err.Error();
}

private static error Unwrap(this ptr<invalidImportError> _addr_e) {
    ref invalidImportError e = ref _addr_e.val;

    return error.As(e.err)!;
}

// importFromModules finds the module and directory in the dependency graph of
// rs containing the package with the given import path. If mg is nil,
// importFromModules attempts to locate the module using only the main module
// and the roots of rs before it loads the full graph.
//
// The answer must be unique: importFromModules returns an error if multiple
// modules are observed to provide the same package.
//
// importFromModules can return a module with an empty m.Path, for packages in
// the standard library.
//
// importFromModules can return an empty directory string, for fake packages
// like "C" and "unsafe".
//
// If the package is not present in any module selected from the requirement
// graph, importFromModules returns an *ImportMissingError.
private static (module.Version, @string, error) importFromModules(context.Context ctx, @string path, ptr<Requirements> _addr_rs, ptr<ModuleGraph> _addr_mg) {
    module.Version m = default;
    @string dir = default;
    error err = default!;
    ref Requirements rs = ref _addr_rs.val;
    ref ModuleGraph mg = ref _addr_mg.val;

    if (strings.Contains(path, "@")) {
        return (new module.Version(), "", error.As(fmt.Errorf("import path should not have @version"))!);
    }
    if (build.IsLocalImport(path)) {
        return (new module.Version(), "", error.As(fmt.Errorf("relative import not supported"))!);
    }
    if (path == "C") { 
        // There's no directory for import "C".
        return (new module.Version(), "", error.As(null!)!);

    }
    {
        var err = module.CheckImportPath(path);

        if (err != null) {
            return (new module.Version(), "", error.As(addr(new invalidImportError(importPath:path,err:err))!)!);
        }
    } 

    // Is the package in the standard library?
    var pathIsStd = search.IsStandardImportPath(path);
    if (pathIsStd && goroot.IsStandardPackage(cfg.GOROOT, cfg.BuildContext.Compiler, path)) {
        if (targetInGorootSrc) {
            {
                var dir__prev3 = dir;

                var (dir, ok, err) = dirInModule(path, targetPrefix, ModRoot(), true);

                if (err != null) {
                    return (new module.Version(), dir, error.As(err)!);
                }
                else if (ok) {
                    return (Target, dir, error.As(null!)!);
                }


                dir = dir__prev3;

            }

        }
        var dir = filepath.Join(cfg.GOROOT, "src", path);
        return (new module.Version(), dir, error.As(null!)!);

    }
    if (cfg.BuildMod == "vendor") {
        var (mainDir, mainOK, mainErr) = dirInModule(path, targetPrefix, ModRoot(), true);
        var (vendorDir, vendorOK, _) = dirInModule(path, "", filepath.Join(ModRoot(), "vendor"), false);
        if (mainOK && vendorOK) {
            return (new module.Version(), "", error.As(addr(new AmbiguousImportError(importPath:path,Dirs:[]string{mainDir,vendorDir}))!)!);
        }
        if (!vendorOK && mainDir != "") {
            return (Target, mainDir, error.As(null!)!);
        }
        if (mainErr != null) {
            return (new module.Version(), "", error.As(mainErr)!);
        }
        readVendorList();
        return (vendorPkgModule[path], vendorDir, error.As(null!)!);

    }
    slice<@string> dirs = default;
    slice<module.Version> mods = default; 

    // Iterate over possible modules for the path, not all selected modules.
    // Iterating over selected modules would make the overall loading time
    // O(M × P) for M modules providing P imported packages, whereas iterating
    // over path prefixes is only O(P × k) with maximum path depth k. For
    // large projects both M and P may be very large (note that M ≤ P), but k
    // will tend to remain smallish (if for no other reason than filesystem
    // path limitations).
    //
    // We perform this iteration either one or two times. If mg is initially nil,
    // then we first attempt to load the package using only the main module and
    // its root requirements. If that does not identify the package, or if mg is
    // already non-nil, then we attempt to load the package using the full
    // requirements in mg.
    while (true) {
        slice<module.Version> sumErrMods = default;
        {
            var prefix = path;

            while (prefix != ".") {
                @string v = default;                bool ok = default;
                if (mg == null) {
                    v, ok = rs.rootSelected(prefix);
                prefix = pathpkg.Dir(prefix);
                }
                else
 {
                    (v, ok) = (mg.Selected(prefix), true);
                }

                if (!ok || v == "none") {
                    continue;
                }

                module.Version m = new module.Version(Path:prefix,Version:v);

                var needSum = true;
                var (root, isLocal, err) = fetch(ctx, m, needSum);
                if (err != null) {
                    {
                        ref var sumErr = ref heap((sumMissingError.val)(null), out ptr<var> _addr_sumErr);

                        if (errors.As(err, _addr_sumErr)) { 
                            // We are missing a sum needed to fetch a module in the build list.
                            // We can't verify that the package is unique, and we may not find
                            // the package at all. Keep checking other modules to decide which
                            // error to report. Multiple sums may be missing if we need to look in
                            // multiple nested modules to resolve the import; we'll report them all.
                            sumErrMods = append(sumErrMods, m);
                            continue;

                        } 
                        // Report fetch error.
                        // Note that we don't know for sure this module is necessary,
                        // but it certainly _could_ provide the package, and even if we
                        // continue the loop and find the package in some other module,
                        // we need to look at this module to make sure the import is
                        // not ambiguous.

                    } 
                    // Report fetch error.
                    // Note that we don't know for sure this module is necessary,
                    // but it certainly _could_ provide the package, and even if we
                    // continue the loop and find the package in some other module,
                    // we need to look at this module to make sure the import is
                    // not ambiguous.
                    return (new module.Version(), "", error.As(err)!);

                }

                {
                    var dir__prev1 = dir;

                    (dir, ok, err) = dirInModule(path, m.Path, root, isLocal);

                    if (err != null) {
                        return (new module.Version(), "", error.As(err)!);
                    }
                    else if (ok) {
                        mods = append(mods, m);
                        dirs = append(dirs, dir);
                    }


                    dir = dir__prev1;

                }

            }

        }

        if (len(mods) > 1) { 
            // We produce the list of directories from longest to shortest candidate
            // module path, but the AmbiguousImportError should report them from
            // shortest to longest. Reverse them now.
            {
                nint i__prev2 = i;

                for (nint i = 0; i < len(mods) / 2; i++) {
                    var j = len(mods) - 1 - i;
                    (mods[i], mods[j]) = (mods[j], mods[i]);                    (dirs[i], dirs[j]) = (dirs[j], dirs[i]);
                }


                i = i__prev2;
            }
            return (new module.Version(), "", error.As(addr(new AmbiguousImportError(importPath:path,Dirs:dirs,Modules:mods))!)!);

        }
        if (len(sumErrMods) > 0) {
            {
                nint i__prev2 = i;

                for (i = 0; i < len(sumErrMods) / 2; i++) {
                    j = len(sumErrMods) - 1 - i;
                    (sumErrMods[i], sumErrMods[j]) = (sumErrMods[j], sumErrMods[i]);
                }


                i = i__prev2;
            }
            return (new module.Version(), "", error.As(addr(new ImportMissingSumError(importPath:path,mods:sumErrMods,found:len(mods)>0,))!)!);

        }
        if (len(mods) == 1) {
            return (mods[0], dirs[0], error.As(null!)!);
        }
        if (mg != null) { 
            // We checked the full module graph and still didn't find the
            // requested package.
            error queryErr = default!;
            if (!HasModRoot()) {
                queryErr = error.As(ErrNoModRoot)!;
            }

            return (new module.Version(), "", error.As(addr(new ImportMissingError(Path:path,QueryErr:queryErr,isStd:pathIsStd))!)!);

        }
        mg, err = rs.Graph(ctx);
        if (err != null) { 
            // We might be missing one or more transitive (implicit) dependencies from
            // the module graph, so we can't return an ImportMissingError here — one
            // of the missing modules might actually contain the package in question,
            // in which case we shouldn't go looking for it in some new dependency.
            return (new module.Version(), "", error.As(err)!);

        }
    }

}

// queryImport attempts to locate a module that can be added to the current
// build list to provide the package with the given import path.
//
// Unlike QueryPattern, queryImport prefers to add a replaced version of a
// module *before* checking the proxies for a version to add.
private static (module.Version, error) queryImport(context.Context ctx, @string path, ptr<Requirements> _addr_rs) {
    module.Version _p0 = default;
    error _p0 = default!;
    ref Requirements rs = ref _addr_rs.val;
 
    // To avoid spurious remote fetches, try the latest replacement for each
    // module (golang.org/issue/26241).
    if (index != null) {
        slice<module.Version> mods = default;
        foreach (var (mp, mv) in index.highestReplaced) {
            if (!maybeInModule(path, mp)) {
                continue;
            }
            if (mv == "") { 
                // The only replacement is a wildcard that doesn't specify a version, so
                // synthesize a pseudo-version with an appropriate major version and a
                // timestamp below any real timestamp. That way, if the main module is
                // used from within some other module, the user will be able to upgrade
                // the requirement to any real version they choose.
                {
                    var (_, pathMajor, ok) = module.SplitPathVersion(mp);

                    if (ok && len(pathMajor) > 0) {
                        mv = module.ZeroPseudoVersion(pathMajor[(int)1..]);
                    }
                    else
 {
                        mv = module.ZeroPseudoVersion("v0");
                    }

                }

            }

            var (mg, err) = rs.Graph(ctx);
            if (err != null) {
                return (new module.Version(), error.As(err)!);
            }

            if (cmpVersion(mg.Selected(mp), mv) >= 0) { 
                // We can't resolve the import by adding mp@mv to the module graph,
                // because the selected version of mp is already at least mv.
                continue;

            }

            mods = append(mods, new module.Version(Path:mp,Version:mv));

        }        sort.Slice(mods, (i, j) => {
            return len(mods[i].Path) > len(mods[j].Path);
        });
        foreach (var (_, m) in mods) {
            var needSum = true;
            var (root, isLocal, err) = fetch(ctx, m, needSum);
            if (err != null) {
                {
                    ref var sumErr = ref heap((sumMissingError.val)(null), out ptr<var> _addr_sumErr);

                    if (errors.As(err, _addr_sumErr)) {
                        return (new module.Version(), error.As(addr(new ImportMissingSumError(importPath:path))!)!);
                    }

                }

                return (new module.Version(), error.As(err)!);

            }

            {
                var (_, ok, err) = dirInModule(path, m.Path, root, isLocal);

                if (err != null) {
                    return (m, error.As(err)!);
                }
                else if (ok) {
                    if (cfg.BuildMod == "readonly") {
                        return (new module.Version(), error.As(addr(new ImportMissingError(Path:path,replaced:m))!)!);
                    }
                    return (m, error.As(null!)!);
                }


            }

        }        if (len(mods) > 0 && module.CheckPath(path) != null) { 
            // The package path is not valid to fetch remotely,
            // so it can only exist in a replaced module,
            // and we know from the above loop that it is not.
            return (new module.Version(), error.As(addr(new PackageNotInModuleError(Mod:mods[0],Query:"latest",Pattern:path,Replacement:Replacement(mods[0]),))!)!);

        }
    }
    if (search.IsStandardImportPath(path)) { 
        // This package isn't in the standard library, isn't in any module already
        // in the build list, and isn't in any other module that the user has
        // shimmed in via a "replace" directive.
        // Moreover, the import path is reserved for the standard library, so
        // QueryPattern cannot possibly find a module containing this package.
        //
        // Instead of trying QueryPattern, report an ImportMissingError immediately.
        return (new module.Version(), error.As(addr(new ImportMissingError(Path:path,isStd:true))!)!);

    }
    if (cfg.BuildMod == "readonly" && !allowMissingModuleImports) { 
        // In readonly mode, we can't write go.mod, so we shouldn't try to look up
        // the module. If readonly mode was enabled explicitly, include that in
        // the error message.
        error queryErr = default!;
        if (cfg.BuildModExplicit) {
            queryErr = error.As(fmt.Errorf("import lookup disabled by -mod=%s", cfg.BuildMod))!;
        }
        else if (cfg.BuildModReason != "") {
            queryErr = error.As(fmt.Errorf("import lookup disabled by -mod=%s\n\t(%s)", cfg.BuildMod, cfg.BuildModReason))!;
        }
        return (new module.Version(), error.As(addr(new ImportMissingError(Path:path,QueryErr:queryErr))!)!);

    }
    fmt.Fprintf(os.Stderr, "go: finding module for package %s\n", path);

    (mg, err) = rs.Graph(ctx);
    if (err != null) {
        return (new module.Version(), error.As(err)!);
    }
    var (candidates, err) = QueryPackages(ctx, path, "latest", mg.Selected, CheckAllowed);
    if (err != null) {
        if (errors.Is(err, fs.ErrNotExist)) { 
            // Return "cannot find module providing package […]" instead of whatever
            // low-level error QueryPattern produced.
            return (new module.Version(), error.As(addr(new ImportMissingError(Path:path,QueryErr:err))!)!);

        }
        else
 {
            return (new module.Version(), error.As(err)!);
        }
    }
    @string candidate0MissingVersion = "";
    foreach (var (i, c) in candidates) {
        {
            var v = mg.Selected(c.Mod.Path);

            if (semver.Compare(v, c.Mod.Version) > 0) { 
                // QueryPattern proposed that we add module c.Mod to provide the package,
                // but we already depend on a newer version of that module (and that
                // version doesn't have the package).
                //
                // This typically happens when a package is present at the "@latest"
                // version (e.g., v1.0.0) of a module, but we have a newer version
                // of the same module in the build list (e.g., v1.0.1-beta), and
                // the package is not present there.
                if (i == 0) {
                    candidate0MissingVersion = v;
                }

                continue;

            }

        }

        return (c.Mod, error.As(null!)!);

    }    return (new module.Version(), error.As(addr(new ImportMissingError(Path:path,Module:candidates[0].Mod,newMissingVersion:candidate0MissingVersion,))!)!);

}

// maybeInModule reports whether, syntactically,
// a package with the given import path could be supplied
// by a module with the given module path (mpath).
private static bool maybeInModule(@string path, @string mpath) {
    return mpath == path || len(path) > len(mpath) && path[len(mpath)] == '/' && path[..(int)len(mpath)] == mpath;
}

private static par.Cache haveGoModCache = default;private static par.Cache haveGoFilesCache = default;

private partial struct goFilesEntry {
    public bool haveGoFiles;
    public error err;
}

// dirInModule locates the directory that would hold the package named by the given path,
// if it were in the module with module path mpath and root mdir.
// If path is syntactically not within mpath,
// or if mdir is a local file tree (isLocal == true) and the directory
// that would hold path is in a sub-module (covered by a go.mod below mdir),
// dirInModule returns "", false, nil.
//
// Otherwise, dirInModule returns the name of the directory where
// Go source files would be expected, along with a boolean indicating
// whether there are in fact Go source files in that directory.
// A non-nil error indicates that the existence of the directory and/or
// source files could not be determined, for example due to a permission error.
private static (@string, bool, error) dirInModule(@string path, @string mpath, @string mdir, bool isLocal) {
    @string dir = default;
    bool haveGoFiles = default;
    error err = default!;
 
    // Determine where to expect the package.
    if (path == mpath) {
        dir = mdir;
    }
    else if (mpath == "") { // vendor directory
        dir = filepath.Join(mdir, path);

    }
    else if (len(path) > len(mpath) && path[len(mpath)] == '/' && path[..(int)len(mpath)] == mpath) {
        dir = filepath.Join(mdir, path[(int)len(mpath) + 1..]);
    }
    else
 {
        return ("", false, error.As(null!)!);
    }
    if (isLocal) {
        {
            var d = dir;

            while (d != mdir && len(d) > len(mdir)) {
                bool haveGoMod = haveGoModCache.Do(d, () => {
                    var (fi, err) = fsys.Stat(filepath.Join(d, "go.mod"));
                    return err == null && !fi.IsDir();
                })._<bool>();

                if (haveGoMod) {
                    return ("", false, error.As(null!)!);
                }
                var parent = filepath.Dir(d);
                if (parent == d) { 
                    // Break the loop, as otherwise we'd loop
                    // forever if d=="." and mdir=="".
                    break;

                }

                d = parent;

            }

        }

    }
    goFilesEntry res = haveGoFilesCache.Do(dir, () => {
        var (ok, err) = fsys.IsDirWithGoFiles(dir);
        return new goFilesEntry(haveGoFiles:ok,err:err);
    })._<goFilesEntry>();

    return (dir, res.haveGoFiles, error.As(res.err)!);

}

// fetch downloads the given module (or its replacement)
// and returns its location.
//
// needSum indicates whether the module may be downloaded in readonly mode
// without a go.sum entry. It should only be false for modules fetched
// speculatively (for example, for incompatible version filtering). The sum
// will still be verified normally.
//
// The isLocal return value reports whether the replacement,
// if any, is local to the filesystem.
private static (@string, bool, error) fetch(context.Context ctx, module.Version mod, bool needSum) {
    @string dir = default;
    bool isLocal = default;
    error err = default!;

    if (mod == Target) {
        return (ModRoot(), true, error.As(null!)!);
    }
    {
        var r = Replacement(mod);

        if (r.Path != "") {
            if (r.Version == "") {
                dir = r.Path;
                if (!filepath.IsAbs(dir)) {
                    dir = filepath.Join(ModRoot(), dir);
                } 
                // Ensure that the replacement directory actually exists:
                // dirInModule does not report errors for missing modules,
                // so if we don't report the error now, later failures will be
                // very mysterious.
                {
                    var (_, err) = fsys.Stat(dir);

                    if (err != null) {
                        if (os.IsNotExist(err)) { 
                            // Semantically the module version itself “exists” — we just don't
                            // have its source code. Remove the equivalence to os.ErrNotExist,
                            // and make the message more concise while we're at it.
                            err = fmt.Errorf("replacement directory %s does not exist", r.Path);

                        }
                        else
 {
                            err = fmt.Errorf("replacement directory %s: %w", r.Path, err);
                        }

                        return (dir, true, error.As(module.VersionError(mod, err))!);

                    }

                }

                return (dir, true, error.As(null!)!);

            }

            mod = r;

        }
    }


    if (HasModRoot() && cfg.BuildMod == "readonly" && needSum && !modfetch.HaveSum(mod)) {
        return ("", false, error.As(module.VersionError(mod, addr(new sumMissingError())))!);
    }
    dir, err = modfetch.Download(ctx, mod);
    return (dir, false, error.As(err)!);

}

private partial struct sumMissingError {
    public @string suggestion;
}

private static @string Error(this ptr<sumMissingError> _addr_e) {
    ref sumMissingError e = ref _addr_e.val;

    return "missing go.sum entry" + e.suggestion;
}

} // end modload_package
