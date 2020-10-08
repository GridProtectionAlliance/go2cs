// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modload -- go2cs converted at 2020 October 08 04:35:17 UTC
// import "cmd/go/internal/modload" ==> using modload = go.cmd.go.@internal.modload_package
// Original source: C:\Go\src\cmd\go\internal\modload\import.go
using errors = go.errors_package;
using fmt = go.fmt_package;
using build = go.go.build_package;
using goroot = go.@internal.goroot_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using sort = go.sort_package;
using strings = go.strings_package;
using time = go.time_package;

using cfg = go.cmd.go.@internal.cfg_package;
using load = go.cmd.go.@internal.load_package;
using modfetch = go.cmd.go.@internal.modfetch_package;
using par = go.cmd.go.@internal.par_package;
using search = go.cmd.go.@internal.search_package;

using module = go.golang.org.x.mod.module_package;
using semver = go.golang.org.x.mod.semver_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class modload_package
    {
        public partial struct ImportMissingError
        {
            public @string Path;
            public module.Version Module;
            public error QueryErr; // newMissingVersion is set to a newer version of Module if one is present
// in the build list. When set, we can't automatically upgrade.
            public @string newMissingVersion;
        }

        private static load.ImportPathError _ = (ImportMissingError.val)(null);

        private static @string Error(this ptr<ImportMissingError> _addr_e)
        {
            ref ImportMissingError e = ref _addr_e.val;

            if (e.Module.Path == "")
            {
                if (search.IsStandardImportPath(e.Path))
                {
                    return fmt.Sprintf("package %s is not in GOROOT (%s)", e.Path, filepath.Join(cfg.GOROOT, "src", e.Path));
                }

                if (e.QueryErr != null)
                {
                    return fmt.Sprintf("cannot find module providing package %s: %v", e.Path, e.QueryErr);
                }

                return "cannot find module providing package " + e.Path;

            }

            return fmt.Sprintf("missing module for import: %s@%s provides %s", e.Module.Path, e.Module.Version, e.Path);

        }

        private static error Unwrap(this ptr<ImportMissingError> _addr_e)
        {
            ref ImportMissingError e = ref _addr_e.val;

            return error.As(e.QueryErr)!;
        }

        private static @string ImportPath(this ptr<ImportMissingError> _addr_e)
        {
            ref ImportMissingError e = ref _addr_e.val;

            return e.Path;
        }

        // An AmbiguousImportError indicates an import of a package found in multiple
        // modules in the build list, or found in both the main module and its vendor
        // directory.
        public partial struct AmbiguousImportError
        {
            public @string importPath;
            public slice<@string> Dirs;
            public slice<module.Version> Modules; // Either empty or 1:1 with Dirs.
        }

        private static @string ImportPath(this ptr<AmbiguousImportError> _addr_e)
        {
            ref AmbiguousImportError e = ref _addr_e.val;

            return e.importPath;
        }

        private static @string Error(this ptr<AmbiguousImportError> _addr_e)
        {
            ref AmbiguousImportError e = ref _addr_e.val;

            @string locType = "modules";
            if (len(e.Modules) == 0L)
            {
                locType = "directories";
            }

            ref strings.Builder buf = ref heap(out ptr<strings.Builder> _addr_buf);
            fmt.Fprintf(_addr_buf, "ambiguous import: found package %s in multiple %s:", e.importPath, locType);

            foreach (var (i, dir) in e.Dirs)
            {
                buf.WriteString("\n\t");
                if (i < len(e.Modules))
                {
                    var m = e.Modules[i];
                    buf.WriteString(m.Path);
                    if (m.Version != "")
                    {
                        fmt.Fprintf(_addr_buf, " %s", m.Version);
                    }

                    fmt.Fprintf(_addr_buf, " (%s)", dir);

                }
                else
                {
                    buf.WriteString(dir);
                }

            }
            return buf.String();

        }

        private static load.ImportPathError _ = addr(new AmbiguousImportError());

        // Import finds the module and directory in the build list
        // containing the package with the given import path.
        // The answer must be unique: Import returns an error
        // if multiple modules attempt to provide the same package.
        // Import can return a module with an empty m.Path, for packages in the standard library.
        // Import can return an empty directory string, for fake packages like "C" and "unsafe".
        //
        // If the package cannot be found in the current build list,
        // Import returns an ImportMissingError as the error.
        // If Import can identify a module that could be added to supply the package,
        // the ImportMissingError records that module.
        public static (module.Version, @string, error) Import(@string path)
        {
            module.Version m = default;
            @string dir = default;
            error err = default!;

            if (strings.Contains(path, "@"))
            {
                return (new module.Version(), "", error.As(fmt.Errorf("import path should not have @version"))!);
            }

            if (build.IsLocalImport(path))
            {
                return (new module.Version(), "", error.As(fmt.Errorf("relative import not supported"))!);
            }

            if (path == "C" || path == "unsafe")
            { 
                // There's no directory for import "C" or import "unsafe".
                return (new module.Version(), "", error.As(null!)!);

            } 

            // Is the package in the standard library?
            var pathIsStd = search.IsStandardImportPath(path);
            if (pathIsStd && goroot.IsStandardPackage(cfg.GOROOT, cfg.BuildContext.Compiler, path))
            {
                if (targetInGorootSrc)
                {
                    {
                        var dir__prev3 = dir;

                        var (dir, ok, err) = dirInModule(path, targetPrefix, ModRoot(), true);

                        if (err != null)
                        {
                            return (new module.Version(), dir, error.As(err)!);
                        }
                        else if (ok)
                        {
                            return (Target, dir, error.As(null!)!);
                        }


                        dir = dir__prev3;

                    }

                }

                var dir = filepath.Join(cfg.GOROOT, "src", path);
                return (new module.Version(), dir, error.As(null!)!);

            } 

            // -mod=vendor is special.
            // Everything must be in the main module or the main module's vendor directory.
            if (cfg.BuildMod == "vendor")
            {
                var (mainDir, mainOK, mainErr) = dirInModule(path, targetPrefix, ModRoot(), true);
                var (vendorDir, vendorOK, _) = dirInModule(path, "", filepath.Join(ModRoot(), "vendor"), false);
                if (mainOK && vendorOK)
                {
                    return (new module.Version(), "", error.As(addr(new AmbiguousImportError(importPath:path,Dirs:[]string{mainDir,vendorDir}))!)!);
                } 
                // Prefer to return main directory if there is one,
                // Note that we're not checking that the package exists.
                // We'll leave that for load.
                if (!vendorOK && mainDir != "")
                {
                    return (Target, mainDir, error.As(null!)!);
                }

                if (mainErr != null)
                {
                    return (new module.Version(), "", error.As(mainErr)!);
                }

                readVendorList();
                return (vendorPkgModule[path], vendorDir, error.As(null!)!);

            } 

            // Check each module on the build list.
            slice<@string> dirs = default;
            slice<module.Version> mods = default;
            {
                var m__prev1 = m;

                foreach (var (_, __m) in buildList)
                {
                    m = __m;
                    if (!maybeInModule(path, m.Path))
                    { 
                        // Avoid possibly downloading irrelevant modules.
                        continue;

                    }

                    var (root, isLocal, err) = fetch(m);
                    if (err != null)
                    { 
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

                        if (err != null)
                        {
                            return (new module.Version(), "", error.As(err)!);
                        }
                        else if (ok)
                        {
                            mods = append(mods, m);
                            dirs = append(dirs, dir);
                        }


                        dir = dir__prev1;

                    }

                }

                m = m__prev1;
            }

            if (len(mods) == 1L)
            {
                return (mods[0L], dirs[0L], error.As(null!)!);
            }

            if (len(mods) > 0L)
            {
                return (new module.Version(), "", error.As(addr(new AmbiguousImportError(importPath:path,Dirs:dirs,Modules:mods))!)!);
            } 

            // Look up module containing the package, for addition to the build list.
            // Goal is to determine the module, download it to dir, and return m, dir, ErrMissing.
            if (cfg.BuildMod == "readonly")
            {
                error queryErr = default!;
                if (!pathIsStd)
                {
                    if (cfg.BuildModReason == "")
                    {
                        queryErr = error.As(fmt.Errorf("import lookup disabled by -mod=%s", cfg.BuildMod))!;
                    }
                    else
                    {
                        queryErr = error.As(fmt.Errorf("import lookup disabled by -mod=%s\n\t(%s)", cfg.BuildMod, cfg.BuildModReason))!;
                    }

                }

                return (new module.Version(), "", error.As(addr(new ImportMissingError(Path:path,QueryErr:queryErr))!)!);

            }

            if (modRoot == "" && !allowMissingModuleImports)
            {
                return (new module.Version(), "", error.As(addr(new ImportMissingError(Path:path,QueryErr:errors.New("working directory is not part of a module"),))!)!);
            } 

            // Not on build list.
            // To avoid spurious remote fetches, next try the latest replacement for each module.
            // (golang.org/issue/26241)
            if (modFile != null)
            {
                map latest = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{}; // path -> version
                foreach (var (_, r) in modFile.Replace)
                {
                    if (maybeInModule(path, r.Old.Path))
                    { 
                        // Don't use semver.Max here; need to preserve +incompatible suffix.
                        var v = latest[r.Old.Path];
                        if (semver.Compare(r.Old.Version, v) > 0L)
                        {
                            v = r.Old.Version;
                        }

                        latest[r.Old.Path] = v;

                    }

                }
                mods = make_slice<module.Version>(0L, len(latest));
                {
                    var v__prev1 = v;

                    foreach (var (__p, __v) in latest)
                    {
                        p = __p;
                        v = __v; 
                        // If the replacement didn't specify a version, synthesize a
                        // pseudo-version with an appropriate major version and a timestamp below
                        // any real timestamp. That way, if the main module is used from within
                        // some other module, the user will be able to upgrade the requirement to
                        // any real version they choose.
                        if (v == "")
                        {
                            {
                                var (_, pathMajor, ok) = module.SplitPathVersion(p);

                                if (ok && len(pathMajor) > 0L)
                                {
                                    v = modfetch.PseudoVersion(pathMajor[1L..], "", new time.Time(), "000000000000");
                                }
                                else
                                {
                                    v = modfetch.PseudoVersion("v0", "", new time.Time(), "000000000000");
                                }

                            }

                        }

                        mods = append(mods, new module.Version(Path:p,Version:v));

                    } 

                    // Every module path in mods is a prefix of the import path.
                    // As in QueryPackage, prefer the longest prefix that satisfies the import.

                    v = v__prev1;
                }

                sort.Slice(mods, (i, j) =>
                {
                    return len(mods[i].Path) > len(mods[j].Path);
                });
                {
                    var m__prev1 = m;

                    foreach (var (_, __m) in mods)
                    {
                        m = __m;
                        (root, isLocal, err) = fetch(m);
                        if (err != null)
                        { 
                            // Report fetch error as above.
                            return (new module.Version(), "", error.As(err)!);

                        }

                        {
                            var (_, ok, err) = dirInModule(path, m.Path, root, isLocal);

                            if (err != null)
                            {
                                return (m, "", error.As(err)!);
                            }
                            else if (ok)
                            {
                                return (m, "", error.As(addr(new ImportMissingError(Path:path,Module:m))!)!);
                            }


                        }

                    }

                    m = m__prev1;
                }

                if (len(mods) > 0L && module.CheckPath(path) != null)
                { 
                    // The package path is not valid to fetch remotely,
                    // so it can only exist if in a replaced module,
                    // and we know from the above loop that it is not.
                    return (new module.Version(), "", error.As(addr(new PackageNotInModuleError(Mod:mods[0],Query:"latest",Pattern:path,Replacement:Replacement(mods[0]),))!)!);

                }

            }

            if (pathIsStd)
            { 
                // This package isn't in the standard library, isn't in any module already
                // in the build list, and isn't in any other module that the user has
                // shimmed in via a "replace" directive.
                // Moreover, the import path is reserved for the standard library, so
                // QueryPackage cannot possibly find a module containing this package.
                //
                // Instead of trying QueryPackage, report an ImportMissingError immediately.
                return (new module.Version(), "", error.As(addr(new ImportMissingError(Path:path))!)!);

            }

            fmt.Fprintf(os.Stderr, "go: finding module for package %s\n", path);

            var (candidates, err) = QueryPackage(path, "latest", Allowed);
            if (err != null)
            {
                if (errors.Is(err, os.ErrNotExist))
                { 
                    // Return "cannot find module providing package […]" instead of whatever
                    // low-level error QueryPackage produced.
                    return (new module.Version(), "", error.As(addr(new ImportMissingError(Path:path,QueryErr:err))!)!);

                }
                else
                {
                    return (new module.Version(), "", error.As(err)!);
                }

            }

            m = candidates[0L].Mod;
            @string newMissingVersion = "";
            foreach (var (_, c) in candidates)
            {
                var cm = c.Mod;
                foreach (var (_, bm) in buildList)
                {
                    if (bm.Path == cm.Path && semver.Compare(bm.Version, cm.Version) > 0L)
                    { 
                        // QueryPackage proposed that we add module cm to provide the package,
                        // but we already depend on a newer version of that module (and we don't
                        // have the package).
                        //
                        // This typically happens when a package is present at the "@latest"
                        // version (e.g., v1.0.0) of a module, but we have a newer version
                        // of the same module in the build list (e.g., v1.0.1-beta), and
                        // the package is not present there.
                        m = cm;
                        newMissingVersion = bm.Version;
                        break;

                    }

                }

            }
            return (m, "", error.As(addr(new ImportMissingError(Path:path,Module:m,newMissingVersion:newMissingVersion))!)!);

        }

        // maybeInModule reports whether, syntactically,
        // a package with the given import path could be supplied
        // by a module with the given module path (mpath).
        private static bool maybeInModule(@string path, @string mpath)
        {
            return mpath == path || len(path) > len(mpath) && path[len(mpath)] == '/' && path[..len(mpath)] == mpath;
        }

        private static par.Cache haveGoModCache = default;        private static par.Cache haveGoFilesCache = default;

        private partial struct goFilesEntry
        {
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
        private static (@string, bool, error) dirInModule(@string path, @string mpath, @string mdir, bool isLocal)
        {
            @string dir = default;
            bool haveGoFiles = default;
            error err = default!;
 
            // Determine where to expect the package.
            if (path == mpath)
            {
                dir = mdir;
            }
            else if (mpath == "")
            { // vendor directory
                dir = filepath.Join(mdir, path);

            }
            else if (len(path) > len(mpath) && path[len(mpath)] == '/' && path[..len(mpath)] == mpath)
            {
                dir = filepath.Join(mdir, path[len(mpath) + 1L..]);
            }
            else
            {
                return ("", false, error.As(null!)!);
            } 

            // Check that there aren't other modules in the way.
            // This check is unnecessary inside the module cache
            // and important to skip in the vendor directory,
            // where all the module trees have been overlaid.
            // So we only check local module trees
            // (the main module, and any directory trees pointed at by replace directives).
            if (isLocal)
            {
                {
                    var d = dir;

                    while (d != mdir && len(d) > len(mdir))
                    {
                        bool haveGoMod = haveGoModCache.Do(d, () =>
                        {
                            var (fi, err) = os.Stat(filepath.Join(d, "go.mod"));
                            return err == null && !fi.IsDir();
                        })._<bool>();

                        if (haveGoMod)
                        {
                            return ("", false, error.As(null!)!);
                        }

                        var parent = filepath.Dir(d);
                        if (parent == d)
                        { 
                            // Break the loop, as otherwise we'd loop
                            // forever if d=="." and mdir=="".
                            break;

                        }

                        d = parent;

                    }

                }

            } 

            // Now committed to returning dir (not "").

            // Are there Go source files in the directory?
            // We don't care about build tags, not even "+build ignore".
            // We're just looking for a plausible directory.
            goFilesEntry res = haveGoFilesCache.Do(dir, () =>
            {
                var (ok, err) = isDirWithGoFiles(dir);
                return new goFilesEntry(haveGoFiles:ok,err:err);
            })._<goFilesEntry>();

            return (dir, res.haveGoFiles, error.As(res.err)!);

        }

        private static (bool, error) isDirWithGoFiles(@string dir) => func((defer, _, __) =>
        {
            bool _p0 = default;
            error _p0 = default!;

            var (f, err) = os.Open(dir);
            if (err != null)
            {
                if (os.IsNotExist(err))
                {
                    return (false, error.As(null!)!);
                }

                return (false, error.As(err)!);

            }

            defer(f.Close());

            var (names, firstErr) = f.Readdirnames(-1L);
            if (firstErr != null)
            {
                {
                    var (fi, err) = f.Stat();

                    if (err == null && !fi.IsDir())
                    {
                        return (false, error.As(null!)!);
                    } 

                    // Rewrite the error from ReadDirNames to include the path if not present.
                    // See https://golang.org/issue/38923.

                } 

                // Rewrite the error from ReadDirNames to include the path if not present.
                // See https://golang.org/issue/38923.
                ptr<os.PathError> pe;
                if (!errors.As(firstErr, _addr_pe))
                {
                    firstErr = addr(new os.PathError(Op:"readdir",Path:dir,Err:firstErr));
                }

            }

            foreach (var (_, name) in names)
            {
                if (strings.HasSuffix(name, ".go"))
                {
                    var (info, err) = os.Stat(filepath.Join(dir, name));
                    if (err == null && info.Mode().IsRegular())
                    { 
                        // If any .go source file exists, the package exists regardless of
                        // errors for other source files. Leave further error reporting for
                        // later.
                        return (true, error.As(null!)!);

                    }

                    if (firstErr == null)
                    {
                        if (os.IsNotExist(err))
                        { 
                            // If the file was concurrently deleted, or was a broken symlink,
                            // convert the error to an opaque error instead of one matching
                            // os.IsNotExist.
                            err = errors.New(err.Error());

                        }

                        firstErr = err;

                    }

                }

            }
            return (false, error.As(firstErr)!);

        });
    }
}}}}
