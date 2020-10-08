// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modload -- go2cs converted at 2020 October 08 04:35:35 UTC
// import "cmd/go/internal/modload" ==> using modload = go.cmd.go.@internal.modload_package
// Original source: C:\Go\src\cmd\go\internal\modload\load.go
using bytes = go.bytes_package;
using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using imports = go.cmd.go.@internal.imports_package;
using modfetch = go.cmd.go.@internal.modfetch_package;
using mvs = go.cmd.go.@internal.mvs_package;
using par = go.cmd.go.@internal.par_package;
using search = go.cmd.go.@internal.search_package;
using str = go.cmd.go.@internal.str_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using build = go.go.build_package;
using os = go.os_package;
using path = go.path_package;
using path = go.path_package;
using filepath = go.path.filepath_package;
using sort = go.sort_package;
using strings = go.strings_package;

using module = go.golang.org.x.mod.module_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class modload_package
    {
        // buildList is the list of modules to use for building packages.
        // It is initialized by calling ImportPaths, ImportFromFiles,
        // LoadALL, or LoadBuildList, each of which uses loaded.load.
        //
        // Ideally, exactly ONE of those functions would be called,
        // and exactly once. Most of the time, that's true.
        // During "go get" it may not be. TODO(rsc): Figure out if
        // that restriction can be established, or else document why not.
        //
        private static slice<module.Version> buildList = default;

        // loaded is the most recently-used package loader.
        // It holds details about individual packages.
        //
        // Note that loaded.buildList is only valid during a load operation;
        // afterward, it is copied back into the global buildList,
        // which should be used instead.
        private static ptr<loader> loaded;

        // ImportPaths returns the set of packages matching the args (patterns),
        // on the target platform. Modules may be added to the build list
        // to satisfy new imports.
        public static slice<ptr<search.Match>> ImportPaths(slice<@string> patterns)
        {
            var matches = ImportPathsQuiet(patterns, imports.Tags());
            search.WarnUnmatched(matches);
            return matches;
        }

        // ImportPathsQuiet is like ImportPaths but does not warn about patterns with
        // no matches. It also lets the caller specify a set of build tags to match
        // packages. The build tags should typically be imports.Tags() or
        // imports.AnyTags(); a nil map has no special meaning.
        public static slice<ptr<search.Match>> ImportPathsQuiet(slice<@string> patterns, map<@string, bool> tags) => func((_, panic, __) =>
        {
            Action<slice<ptr<search.Match>>, bool> updateMatches = (matches, iterating) =>
            {
                {
                    var m__prev1 = m;

                    foreach (var (_, __m) in matches)
                    {
                        m = __m;

                        if (m.IsLocal()) 
                            // Evaluate list of file system directories on first iteration.
                            if (m.Dirs == null)
                            {
                                matchLocalDirs(_addr_m);
                            } 

                            // Make a copy of the directory list and translate to import paths.
                            // Note that whether a directory corresponds to an import path
                            // changes as the build list is updated, and a directory can change
                            // from not being in the build list to being in it and back as
                            // the exact version of a particular module increases during
                            // the loader iterations.
                            m.Pkgs = m.Pkgs[..0L];
                            foreach (var (_, dir) in m.Dirs)
                            {
                                var (pkg, err) = resolveLocalPackage(dir);
                                if (err != null)
                                {
                                    if (!m.IsLiteral() && (err == errPkgIsBuiltin || err == errPkgIsGorootSrc))
                                    {
                                        continue; // Don't include "builtin" or GOROOT/src in wildcard patterns.
                                    } 

                                    // If we're outside of a module, ensure that the failure mode
                                    // indicates that.
                                    ModRoot();

                                    if (!iterating)
                                    {
                                        m.AddError(err);
                                    }

                                    continue;

                                }

                                m.Pkgs = append(m.Pkgs, pkg);

                            }
                        else if (m.IsLiteral()) 
                            m.Pkgs = new slice<@string>(new @string[] { m.Pattern() });
                        else if (strings.Contains(m.Pattern(), "...")) 
                            m.Errs = m.Errs[..0L];
                            matchPackages(m, loaded.tags, includeStd, buildList);
                        else if (m.Pattern() == "all") 
                            loaded.testAll = true;
                            if (iterating)
                            { 
                                // Enumerate the packages in the main module.
                                // We'll load the dependencies as we find them.
                                m.Errs = m.Errs[..0L];
                                matchPackages(m, loaded.tags, omitStd, new slice<module.Version>(new module.Version[] { Target }));

                            }
                            else
                            { 
                                // Starting with the packages in the main module,
                                // enumerate the full list of "all".
                                m.Pkgs = loaded.computePatternAll(m.Pkgs);

                            }

                        else if (m.Pattern() == "std" || m.Pattern() == "cmd") 
                            if (m.Pkgs == null)
                            {
                                m.MatchPackages(); // Locate the packages within GOROOT/src.
                            }

                        else 
                            panic(fmt.Sprintf("internal error: modload missing case for pattern %s", m.Pattern()));
                        
                    }

                    m = m__prev1;
                }
            }
;

            InitMod();

            slice<ptr<search.Match>> matches = default;
            foreach (var (_, pattern) in search.CleanPatterns(patterns))
            {
                matches = append(matches, search.NewMatch(pattern));
            }
            loaded = newLoader(tags);
            loaded.load(() =>
            {
                slice<@string> roots = default;
                updateMatches(matches, true);
                {
                    var m__prev1 = m;

                    foreach (var (_, __m) in matches)
                    {
                        m = __m;
                        roots = append(roots, m.Pkgs);
                    }

                    m = m__prev1;
                }

                return roots;

            }); 

            // One last pass to finalize wildcards.
            updateMatches(matches, false);
            checkMultiplePaths();
            WriteGoMod();

            return matches;

        });

        // checkMultiplePaths verifies that a given module path is used as itself
        // or as a replacement for another module, but not both at the same time.
        //
        // (See https://golang.org/issue/26607 and https://golang.org/issue/34650.)
        private static void checkMultiplePaths()
        {
            var firstPath = make_map<module.Version, @string>(len(buildList));
            foreach (var (_, mod) in buildList)
            {
                var src = mod;
                {
                    var rep = Replacement(mod);

                    if (rep.Path != "")
                    {
                        src = rep;
                    }

                }

                {
                    var (prev, ok) = firstPath[src];

                    if (!ok)
                    {
                        firstPath[src] = mod.Path;
                    }
                    else if (prev != mod.Path)
                    {
                        @base.Errorf("go: %s@%s used for two different module paths (%s and %s)", src.Path, src.Version, prev, mod.Path);
                    }


                }

            }
            @base.ExitIfErrors();

        }

        // matchLocalDirs is like m.MatchDirs, but tries to avoid scanning directories
        // outside of the standard library and active modules.
        private static void matchLocalDirs(ptr<search.Match> _addr_m) => func((_, panic, __) =>
        {
            ref search.Match m = ref _addr_m.val;

            if (!m.IsLocal())
            {
                panic(fmt.Sprintf("internal error: resolveLocalDirs on non-local pattern %s", m.Pattern()));
            }

            {
                var i = strings.Index(m.Pattern(), "...");

                if (i >= 0L)
                { 
                    // The pattern is local, but it is a wildcard. Its packages will
                    // only resolve to paths if they are inside of the standard
                    // library, the main module, or some dependency of the main
                    // module. Verify that before we walk the filesystem: a filesystem
                    // walk in a directory like /var or /etc can be very expensive!
                    var dir = filepath.Dir(filepath.Clean(m.Pattern()[..i + 3L]));
                    var absDir = dir;
                    if (!filepath.IsAbs(dir))
                    {
                        absDir = filepath.Join(@base.Cwd, dir);
                    }

                    if (search.InDir(absDir, cfg.GOROOTsrc) == "" && search.InDir(absDir, ModRoot()) == "" && pathInModuleCache(absDir) == "")
                    {
                        m.Dirs = new slice<@string>(new @string[] {  });
                        m.AddError(fmt.Errorf("directory prefix %s outside available modules", @base.ShortPath(absDir)));
                        return ;
                    }

                }

            }


            m.MatchDirs();

        });

        // resolveLocalPackage resolves a filesystem path to a package path.
        private static (@string, error) resolveLocalPackage(@string dir)
        {
            @string _p0 = default;
            error _p0 = default!;

            @string absDir = default;
            if (filepath.IsAbs(dir))
            {
                absDir = filepath.Clean(dir);
            }
            else
            {
                absDir = filepath.Join(@base.Cwd, dir);
            }

            var (bp, err) = cfg.BuildContext.ImportDir(absDir, 0L);
            if (err != null && (bp == null || len(bp.IgnoredGoFiles) == 0L))
            { 
                // golang.org/issue/32917: We should resolve a relative path to a
                // package path only if the relative path actually contains the code
                // for that package.
                //
                // If the named directory does not exist or contains no Go files,
                // the package does not exist.
                // Other errors may affect package loading, but not resolution.
                {
                    var (_, err) = os.Stat(absDir);

                    if (err != null)
                    {
                        if (os.IsNotExist(err))
                        { 
                            // Canonicalize OS-specific errors to errDirectoryNotFound so that error
                            // messages will be easier for users to search for.
                            return ("", error.As(addr(new os.PathError(Op:"stat",Path:absDir,Err:errDirectoryNotFound))!)!);

                        }

                        return ("", error.As(err)!);

                    }

                }

                {
                    ptr<build.NoGoError> (_, noGo) = err._<ptr<build.NoGoError>>();

                    if (noGo)
                    { 
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

            if (modRoot != "" && absDir == modRoot)
            {
                if (absDir == cfg.GOROOTsrc)
                {
                    return ("", error.As(errPkgIsGorootSrc)!);
                }

                return (targetPrefix, error.As(null!)!);

            } 

            // Note: The checks for @ here are just to avoid misinterpreting
            // the module cache directories (formerly GOPATH/src/mod/foo@v1.5.2/bar).
            // It's not strictly necessary but helpful to keep the checks.
            if (modRoot != "" && strings.HasPrefix(absDir, modRoot + string(filepath.Separator)) && !strings.Contains(absDir[len(modRoot)..], "@"))
            {
                var suffix = filepath.ToSlash(absDir[len(modRoot)..]);
                if (strings.HasPrefix(suffix, "/vendor/"))
                {
                    if (cfg.BuildMod != "vendor")
                    {
                        return ("", error.As(fmt.Errorf("without -mod=vendor, directory %s has no package path", absDir))!);
                    }

                    readVendorList();
                    var pkg = strings.TrimPrefix(suffix, "/vendor/");
                    {
                        var (_, ok) = vendorPkgModule[pkg];

                        if (!ok)
                        {
                            return ("", error.As(fmt.Errorf("directory %s is not a package listed in vendor/modules.txt", absDir))!);
                        }

                    }

                    return (pkg, error.As(null!)!);

                }

                if (targetPrefix == "")
                {
                    pkg = strings.TrimPrefix(suffix, "/");
                    if (pkg == "builtin")
                    { 
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

                    if (err != null)
                    {
                        return ("", error.As(err)!);
                    }
                    else if (!ok)
                    {
                        return ("", error.As(addr(new PackageNotInModuleError(Mod:Target,Pattern:pkg))!)!);
                    }


                }

                return (pkg, error.As(null!)!);

            }

            {
                var sub = search.InDir(absDir, cfg.GOROOTsrc);

                if (sub != "" && sub != "." && !strings.Contains(sub, "@"))
                {
                    pkg = filepath.ToSlash(sub);
                    if (pkg == "builtin")
                    {
                        return ("", error.As(errPkgIsBuiltin)!);
                    }

                    return (pkg, error.As(null!)!);

                }

            }


            pkg = pathInModuleCache(absDir);
            if (pkg == "")
            {
                return ("", error.As(fmt.Errorf("directory %s outside available modules", @base.ShortPath(absDir)))!);
            }

            return (pkg, error.As(null!)!);

        }

        private static var errDirectoryNotFound = errors.New("directory not found");        private static var errPkgIsGorootSrc = errors.New("GOROOT/src is not an importable package");        private static var errPkgIsBuiltin = errors.New("\"builtin\" is a pseudo-package, not an importable package");

        // pathInModuleCache returns the import path of the directory dir,
        // if dir is in the module cache copy of a module in our build list.
        private static @string pathInModuleCache(@string dir)
        {
            foreach (var (_, m) in buildList[1L..])
            {
                @string root = default;
                error err = default!;
                {
                    var repl = Replacement(m);

                    if (repl.Path != "" && repl.Version == "")
                    {
                        root = repl.Path;
                        if (!filepath.IsAbs(root))
                        {
                            root = filepath.Join(ModRoot(), root);
                        }

                    }
                    else if (repl.Path != "")
                    {
                        root, err = modfetch.DownloadDir(repl);
                    }
                    else
                    {
                        root, err = modfetch.DownloadDir(m);
                    }


                }

                if (err != null)
                {
                    continue;
                }

                {
                    var sub = search.InDir(dir, root);

                    if (sub != "")
                    {
                        sub = filepath.ToSlash(sub);
                        if (!strings.Contains(sub, "/vendor/") && !strings.HasPrefix(sub, "vendor/") && !strings.Contains(sub, "@"))
                        {
                            return path.Join(m.Path, filepath.ToSlash(sub));
                        }

                    }

                }

            }
            return "";

        }

        // ImportFromFiles adds modules to the build list as needed
        // to satisfy the imports in the named Go source files.
        public static void ImportFromFiles(slice<@string> gofiles)
        {
            InitMod();

            var tags = imports.Tags();
            var (imports, testImports, err) = imports.ScanFiles(gofiles, tags);
            if (err != null)
            {
                @base.Fatalf("go: %v", err);
            }

            loaded = newLoader(tags);
            loaded.load(() =>
            {
                slice<@string> roots = default;
                roots = append(roots, imports);
                roots = append(roots, testImports);
                return roots;
            });
            WriteGoMod();

        }

        // DirImportPath returns the effective import path for dir,
        // provided it is within the main module, or else returns ".".
        public static @string DirImportPath(@string dir)
        {
            if (modRoot == "")
            {
                return ".";
            }

            if (!filepath.IsAbs(dir))
            {
                dir = filepath.Join(@base.Cwd, dir);
            }
            else
            {
                dir = filepath.Clean(dir);
            }

            if (dir == modRoot)
            {
                return targetPrefix;
            }

            if (strings.HasPrefix(dir, modRoot + string(filepath.Separator)))
            {
                var suffix = filepath.ToSlash(dir[len(modRoot)..]);
                if (strings.HasPrefix(suffix, "/vendor/"))
                {
                    return strings.TrimPrefix(suffix, "/vendor/");
                }

                return targetPrefix + suffix;

            }

            return ".";

        }

        // LoadBuildList loads and returns the build list from go.mod.
        // The loading of the build list happens automatically in ImportPaths:
        // LoadBuildList need only be called if ImportPaths is not
        // (typically in commands that care about the module but
        // no particular package).
        public static slice<module.Version> LoadBuildList()
        {
            InitMod();
            ReloadBuildList();
            WriteGoMod();
            return buildList;
        }

        public static slice<module.Version> ReloadBuildList()
        {
            loaded = newLoader(imports.Tags());
            loaded.load(() => null);
            return buildList;
        }

        // LoadALL returns the set of all packages in the current module
        // and their dependencies in any other modules, without filtering
        // due to build tags, except "+build ignore".
        // It adds modules to the build list as needed to satisfy new imports.
        // This set is useful for deciding whether a particular import is needed
        // anywhere in a module.
        public static slice<@string> LoadALL()
        {
            return loadAll(true);
        }

        // LoadVendor is like LoadALL but only follows test dependencies
        // for tests in the main module. Tests in dependency modules are
        // ignored completely.
        // This set is useful for identifying the which packages to include in a vendor directory.
        public static slice<@string> LoadVendor()
        {
            return loadAll(false);
        }

        private static slice<@string> loadAll(bool testAll)
        {
            InitMod();

            loaded = newLoader(imports.AnyTags());
            loaded.isALL = true;
            loaded.testAll = testAll;
            if (!testAll)
            {
                loaded.testRoots = true;
            }

            var all = TargetPackages("...");
            loaded.load(() => all.Pkgs);
            checkMultiplePaths();
            WriteGoMod();

            slice<@string> paths = default;
            foreach (var (_, pkg) in loaded.pkgs)
            {
                if (pkg.err != null)
                {
                    @base.Errorf("%s: %v", pkg.stackText(), pkg.err);
                    continue;
                }

                paths = append(paths, pkg.path);

            }
            foreach (var (_, err) in all.Errs)
            {
                @base.Errorf("%v", err);
            }
            @base.ExitIfErrors();
            return paths;

        }

        // TargetPackages returns the list of packages in the target (top-level) module
        // matching pattern, which may be relative to the working directory, under all
        // build tag settings.
        public static ptr<search.Match> TargetPackages(@string pattern)
        { 
            // TargetPackages is relative to the main module, so ensure that the main
            // module is a thing that can contain packages.
            ModRoot();

            var m = search.NewMatch(pattern);
            matchPackages(m, imports.AnyTags(), omitStd, new slice<module.Version>(new module.Version[] { Target }));
            return _addr_m!;

        }

        // BuildList returns the module build list,
        // typically constructed by a previous call to
        // LoadBuildList or ImportPaths.
        // The caller must not modify the returned list.
        public static slice<module.Version> BuildList()
        {
            return buildList;
        }

        // SetBuildList sets the module build list.
        // The caller is responsible for ensuring that the list is valid.
        // SetBuildList does not retain a reference to the original list.
        public static void SetBuildList(slice<module.Version> list)
        {
            buildList = append(new slice<module.Version>(new module.Version[] {  }), list);
        }

        // TidyBuildList trims the build list to the minimal requirements needed to
        // retain the same versions of all packages from the preceding Load* or
        // ImportPaths* call.
        public static void TidyBuildList()
        {
            map used = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<module.Version, bool>{Target:true};
            foreach (var (_, pkg) in loaded.pkgs)
            {
                used[pkg.mod] = true;
            }
            module.Version keep = new slice<module.Version>(new module.Version[] { Target });
            slice<@string> direct = default;
            foreach (var (_, m) in buildList[1L..])
            {
                if (used[m])
                {
                    keep = append(keep, m);
                    if (loaded.direct[m.Path])
                    {
                        direct = append(direct, m.Path);
                    }

                }
                else if (cfg.BuildV)
                {
                    {
                        var (_, ok) = index.require[m];

                        if (ok)
                        {
                            fmt.Fprintf(os.Stderr, "unused %s\n", m.Path);
                        }

                    }

                }

            }
            var (min, err) = mvs.Req(Target, direct, addr(new mvsReqs(buildList:keep)));
            if (err != null)
            {
                @base.Fatalf("go: %v", err);
            }

            buildList = append(new slice<module.Version>(new module.Version[] { Target }), min);

        }

        // ImportMap returns the actual package import path
        // for an import path found in source code.
        // If the given import path does not appear in the source code
        // for the packages that have been loaded, ImportMap returns the empty string.
        public static @string ImportMap(@string path)
        {
            ptr<loadPkg> (pkg, ok) = loaded.pkgCache.Get(path)._<ptr<loadPkg>>();
            if (!ok)
            {
                return "";
            }

            return pkg.path;

        }

        // PackageDir returns the directory containing the source code
        // for the package named by the import path.
        public static @string PackageDir(@string path)
        {
            ptr<loadPkg> (pkg, ok) = loaded.pkgCache.Get(path)._<ptr<loadPkg>>();
            if (!ok)
            {
                return "";
            }

            return pkg.dir;

        }

        // PackageModule returns the module providing the package named by the import path.
        public static module.Version PackageModule(@string path)
        {
            ptr<loadPkg> (pkg, ok) = loaded.pkgCache.Get(path)._<ptr<loadPkg>>();
            if (!ok)
            {
                return new module.Version();
            }

            return pkg.mod;

        }

        // PackageImports returns the imports for the package named by the import path.
        // Test imports will be returned as well if tests were loaded for the package
        // (i.e., if "all" was loaded or if LoadTests was set and the path was matched
        // by a command line argument). PackageImports will return nil for
        // unknown package paths.
        public static (slice<@string>, slice<@string>) PackageImports(@string path)
        {
            slice<@string> imports = default;
            slice<@string> testImports = default;

            ptr<loadPkg> (pkg, ok) = loaded.pkgCache.Get(path)._<ptr<loadPkg>>();
            if (!ok)
            {
                return (null, null);
            }

            imports = make_slice<@string>(len(pkg.imports));
            {
                var i__prev1 = i;
                var p__prev1 = p;

                foreach (var (__i, __p) in pkg.imports)
                {
                    i = __i;
                    p = __p;
                    imports[i] = p.path;
                }

                i = i__prev1;
                p = p__prev1;
            }

            if (pkg.test != null)
            {
                testImports = make_slice<@string>(len(pkg.test.imports));
                {
                    var i__prev1 = i;
                    var p__prev1 = p;

                    foreach (var (__i, __p) in pkg.test.imports)
                    {
                        i = __i;
                        p = __p;
                        testImports[i] = p.path;
                    }

                    i = i__prev1;
                    p = p__prev1;
                }
            }

            return (imports, testImports);

        }

        // ModuleUsedDirectly reports whether the main module directly imports
        // some package in the module with the given path.
        public static bool ModuleUsedDirectly(@string path)
        {
            return loaded.direct[path];
        }

        // Lookup returns the source directory, import path, and any loading error for
        // the package at path as imported from the package in parentDir.
        // Lookup requires that one of the Load functions in this package has already
        // been called.
        public static (@string, @string, error) Lookup(@string parentPath, bool parentIsStd, @string path) => func((_, panic, __) =>
        {
            @string dir = default;
            @string realPath = default;
            error err = default!;

            if (path == "")
            {
                panic("Lookup called with empty package path");
            }

            if (parentIsStd)
            {
                path = loaded.stdVendor(parentPath, path);
            }

            ptr<loadPkg> (pkg, ok) = loaded.pkgCache.Get(path)._<ptr<loadPkg>>();
            if (!ok)
            { 
                // The loader should have found all the relevant paths.
                // There are a few exceptions, though:
                //    - during go list without -test, the p.Resolve calls to process p.TestImports and p.XTestImports
                //      end up here to canonicalize the import paths.
                //    - during any load, non-loaded packages like "unsafe" end up here.
                //    - during any load, build-injected dependencies like "runtime/cgo" end up here.
                //    - because we ignore appengine/* in the module loader,
                //      the dependencies of any actual appengine/* library end up here.
                var dir = findStandardImportPath(path);
                if (dir != "")
                {
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
        // Loading is an iterative process: try to load all the needed packages,
        // but if imports are missing, try to resolve those imports, and repeat.
        //
        // Although most of the loading state is maintained in the loader struct,
        // one key piece - the build list - is a global, so that it can be modified
        // separate from the loading operation, such as during "go get"
        // upgrades/downgrades or in "go mod" operations.
        // TODO(rsc): It might be nice to make the loader take and return
        // a buildList rather than hard-coding use of the global.
        private partial struct loader
        {
            public map<@string, bool> tags; // tags for scanDir
            public bool testRoots; // include tests for roots
            public bool isALL; // created with LoadALL
            public bool testAll; // include tests for all packages
            public bool forceStdVendor; // if true, load standard-library dependencies from the vendor subtree

// reset on each iteration
            public slice<ptr<loadPkg>> roots;
            public slice<ptr<loadPkg>> pkgs;
            public ptr<par.Work> work; // current work queue
            public ptr<par.Cache> pkgCache; // map from string to *loadPkg

// computed at end of iterations
            public map<@string, bool> direct; // imported directly by main module
            public map<@string, @string> goVersion; // go version recorded in each module
        }

        // LoadTests controls whether the loaders load tests of the root packages.
        public static bool LoadTests = default;

        private static ptr<loader> newLoader(map<@string, bool> tags)
        {
            ptr<loader> ld = @new<loader>();
            ld.tags = tags;
            ld.testRoots = LoadTests; 

            // Inside the "std" and "cmd" modules, we prefer to use the vendor directory
            // unless the command explicitly changes the module graph.
            if (!targetInGorootSrc || (cfg.CmdName != "get" && !strings.HasPrefix(cfg.CmdName, "mod ")))
            {
                ld.forceStdVendor = true;
            }

            return _addr_ld!;

        }

        private static void reset(this ptr<loader> _addr_ld)
        {
            ref loader ld = ref _addr_ld.val;

            ld.roots = null;
            ld.pkgs = null;
            ld.work = @new<par.Work>();
            ld.pkgCache = @new<par.Cache>();
        }

        // A loadPkg records information about a single loaded package.
        private partial struct loadPkg
        {
            public @string path; // import path
            public module.Version mod; // module providing package
            public @string dir; // directory containing source code
            public slice<ptr<loadPkg>> imports; // packages imported by this one
            public error err; // error loading package
            public ptr<loadPkg> stack; // package importing this one in minimal import stack for this pkg
            public ptr<loadPkg> test; // package with test imports, if we need test
            public ptr<loadPkg> testOf;
            public slice<@string> testImports; // test-only imports, saved for use by pkg.test.
        }

        private static var errMissing = errors.New("cannot find package");

        // load attempts to load the build graph needed to process a set of root packages.
        // The set of root packages is defined by the addRoots function,
        // which must call add(path) with the import path of each root package.
        private static slice<@string> load(this ptr<loader> _addr_ld, Func<slice<@string>> roots)
        {
            ref loader ld = ref _addr_ld.val;

            error err = default!;
            var reqs = Reqs();
            buildList, err = mvs.BuildList(Target, reqs);
            if (err != null)
            {
                @base.Fatalf("go: %v", err);
            }

            var added = make_map<@string, bool>();
            while (true)
            {
                ld.reset();
                if (roots != null)
                { 
                    // Note: the returned roots can change on each iteration,
                    // since the expansion of package patterns depends on the
                    // build list we're using.
                    foreach (var (_, path) in roots())
                    {
                        ld.work.Add(ld.pkg(path, true));
                    }

                }

                ld.work.Do(10L, ld.doPkg);
                ld.buildStacks();
                long numAdded = 0L;
                var haveMod = make_map<module.Version, bool>();
                {
                    var m__prev2 = m;

                    foreach (var (_, __m) in buildList)
                    {
                        m = __m;
                        haveMod[m] = true;
                    }

                    m = m__prev2;
                }

                var modAddedBy = make_map<module.Version, ptr<loadPkg>>();
                {
                    var pkg__prev2 = pkg;

                    foreach (var (_, __pkg) in ld.pkgs)
                    {
                        pkg = __pkg;
                        {
                            error err__prev1 = err;

                            ptr<ImportMissingError> (err, ok) = pkg.err._<ptr<ImportMissingError>>();

                            if (ok && err.Module.Path != "")
                            {
                                if (err.newMissingVersion != "")
                                {
                                    @base.Fatalf("go: %s: package provided by %s at latest version %s but not at required version %s", pkg.stackText(), err.Module.Path, err.Module.Version, err.newMissingVersion);
                                }

                                fmt.Fprintf(os.Stderr, "go: found %s in %s %s\n", pkg.path, err.Module.Path, err.Module.Version);
                                if (added[pkg.path])
                                {
                                    @base.Fatalf("go: %s: looping trying to add package", pkg.stackText());
                                }

                                added[pkg.path] = true;
                                numAdded++;
                                if (!haveMod[err.Module])
                                {
                                    haveMod[err.Module] = true;
                                    modAddedBy[err.Module] = pkg;
                                    buildList = append(buildList, err.Module);
                                }

                                continue;

                            } 
                            // Leave other errors for Import or load.Packages to report.

                            err = err__prev1;

                        } 
                        // Leave other errors for Import or load.Packages to report.
                    }

                    pkg = pkg__prev2;
                }

                @base.ExitIfErrors();
                if (numAdded == 0L)
                {
                    break;
                } 

                // Recompute buildList with all our additions.
                reqs = Reqs();
                buildList, err = mvs.BuildList(Target, reqs);
                if (err != null)
                { 
                    // If an error was found in a newly added module, report the package
                    // import stack instead of the module requirement stack. Packages
                    // are more descriptive.
                    {
                        error err__prev2 = err;

                        (err, ok) = err._<ptr<mvs.BuildListError>>();

                        if (ok)
                        {
                            {
                                var pkg__prev3 = pkg;

                                var pkg = modAddedBy[err.Module()];

                                if (pkg != null)
                                {
                                    @base.Fatalf("go: %s: %v", pkg.stackText(), err.Err);
                                }

                                pkg = pkg__prev3;

                            }

                        }

                        err = err__prev2;

                    }

                    @base.Fatalf("go: %v", err);

                }

            }

            @base.ExitIfErrors(); 

            // Compute directly referenced dependency modules.
            ld.direct = make_map<@string, bool>();
            {
                var pkg__prev1 = pkg;

                foreach (var (_, __pkg) in ld.pkgs)
                {
                    pkg = __pkg;
                    if (pkg.mod == Target)
                    {
                        foreach (var (_, dep) in pkg.imports)
                        {
                            if (dep.mod.Path != "")
                            {
                                ld.direct[dep.mod.Path] = true;
                            }

                        }

                    }

                } 

                // Add Go versions, computed during walk.

                pkg = pkg__prev1;
            }

            ld.goVersion = make_map<@string, @string>();
            {
                var m__prev1 = m;

                foreach (var (_, __m) in buildList)
                {
                    m = __m;
                    ptr<mvsReqs> (v, _) = reqs._<ptr<mvsReqs>>().versions.Load(m);
                    ld.goVersion[m.Path], _ = v._<@string>();
                } 

                // Mix in direct markings (really, lack of indirect markings)
                // from go.mod, unless we scanned the whole module
                // and can therefore be sure we know better than go.mod.

                m = m__prev1;
            }

            if (!ld.isALL && modFile != null)
            {
                foreach (var (_, r) in modFile.Require)
                {
                    if (!r.Indirect)
                    {
                        ld.direct[r.Mod.Path] = true;
                    }

                }

            }

        }

        // pkg returns the *loadPkg for path, creating and queuing it if needed.
        // If the package should be tested, its test is created but not queued
        // (the test is queued after processing pkg).
        // If isRoot is true, the pkg is being queued as one of the roots of the work graph.
        private static ptr<loadPkg> pkg(this ptr<loader> _addr_ld, @string path, bool isRoot)
        {
            ref loader ld = ref _addr_ld.val;

            return ld.pkgCache.Do(path, () =>
            {
                ptr<loadPkg> pkg = addr(new loadPkg(path:path,));
                if (ld.testRoots && isRoot || ld.testAll)
                {
                    ptr<loadPkg> test = addr(new loadPkg(path:path,testOf:pkg,));
                    pkg.test = test;
                }

                if (isRoot)
                {
                    ld.roots = append(ld.roots, pkg);
                }

                ld.work.Add(pkg);
                return _addr_pkg!;

            })._<ptr<loadPkg>>();

        }

        // doPkg processes a package on the work queue.
        private static void doPkg(this ptr<loader> _addr_ld, object item)
        {
            ref loader ld = ref _addr_ld.val;
 
            // TODO: what about replacements?
            ptr<loadPkg> pkg = item._<ptr<loadPkg>>();
            slice<@string> imports = default;
            if (pkg.testOf != null)
            {
                pkg.dir = pkg.testOf.dir;
                pkg.mod = pkg.testOf.mod;
                imports = pkg.testOf.testImports;
            }
            else
            {
                if (strings.Contains(pkg.path, "@"))
                { 
                    // Leave for error during load.
                    return ;

                }

                if (build.IsLocalImport(pkg.path) || filepath.IsAbs(pkg.path))
                { 
                    // Leave for error during load.
                    // (Module mode does not allow local imports.)
                    return ;

                }

                pkg.mod, pkg.dir, pkg.err = Import(pkg.path);
                if (pkg.dir == "")
                {
                    return ;
                }

                slice<@string> testImports = default;
                error err = default!;
                imports, testImports, err = scanDir(pkg.dir, ld.tags);
                if (err != null)
                {
                    pkg.err = err;
                    return ;
                }

                if (pkg.test != null)
                {
                    pkg.testImports = testImports;
                }

            }

            var inStd = (search.IsStandardImportPath(pkg.path) && search.InDir(pkg.dir, cfg.GOROOTsrc) != "");
            foreach (var (_, path) in imports)
            {
                if (inStd)
                {
                    path = ld.stdVendor(pkg.path, path);
                }

                pkg.imports = append(pkg.imports, ld.pkg(path, false));

            } 

            // Now that pkg.dir, pkg.mod, pkg.testImports are set, we can queue pkg.test.
            // TODO: All that's left is creating new imports. Why not just do it now?
            if (pkg.test != null)
            {
                ld.work.Add(pkg.test);
            }

        }

        // stdVendor returns the canonical import path for the package with the given
        // path when imported from the standard-library package at parentPath.
        private static @string stdVendor(this ptr<loader> _addr_ld, @string parentPath, @string path)
        {
            ref loader ld = ref _addr_ld.val;

            if (search.IsStandardImportPath(path))
            {
                return path;
            }

            if (str.HasPathPrefix(parentPath, "cmd"))
            {
                if (ld.forceStdVendor || Target.Path != "cmd")
                {
                    var vendorPath = pathpkg.Join("cmd", "vendor", path);
                    {
                        var (_, err) = os.Stat(filepath.Join(cfg.GOROOTsrc, filepath.FromSlash(vendorPath)));

                        if (err == null)
                        {
                            return vendorPath;
                        }

                    }

                }

            }
            else if (ld.forceStdVendor || Target.Path != "std")
            {
                vendorPath = pathpkg.Join("vendor", path);
                {
                    (_, err) = os.Stat(filepath.Join(cfg.GOROOTsrc, filepath.FromSlash(vendorPath)));

                    if (err == null)
                    {
                        return vendorPath;
                    }

                }

            } 

            // Not vendored: resolve from modules.
            return path;

        }

        // computePatternAll returns the list of packages matching pattern "all",
        // starting with a list of the import paths for the packages in the main module.
        private static slice<@string> computePatternAll(this ptr<loader> _addr_ld, slice<@string> paths)
        {
            ref loader ld = ref _addr_ld.val;

            var seen = make_map<ptr<loadPkg>, bool>();
            slice<@string> all = default;
            Action<ptr<loadPkg>> walk = default;
            walk = pkg =>
            {
                if (seen[pkg])
                {
                    return ;
                }

                seen[pkg] = true;
                if (pkg.testOf == null)
                {
                    all = append(all, pkg.path);
                }

                {
                    var p__prev1 = p;

                    foreach (var (_, __p) in pkg.imports)
                    {
                        p = __p;
                        walk(p);
                    }

                    p = p__prev1;
                }

                {
                    var p__prev1 = p;

                    var p = pkg.test;

                    if (p != null)
                    {
                        walk(p);
                    }

                    p = p__prev1;

                }

            }
;
            foreach (var (_, path) in paths)
            {
                walk(ld.pkg(path, false));
            }
            sort.Strings(all);

            return all;

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
        private static (slice<@string>, slice<@string>, error) scanDir(@string dir, map<@string, bool> tags)
        {
            slice<@string> imports_ = default;
            slice<@string> testImports = default;
            error err = default!;

            imports_, testImports, err = imports.ScanDir(dir, tags);

            Func<slice<@string>, slice<@string>> filter = x =>
            {
                long w = 0L;
                foreach (var (_, pkg) in x)
                {
                    if (pkg != "C" && pkg != "appengine" && !strings.HasPrefix(pkg, "appengine/") && pkg != "appengine_internal" && !strings.HasPrefix(pkg, "appengine_internal/"))
                    {
                        x[w] = pkg;
                        w++;
                    }

                }
                return x[..w];

            }
;

            return (filter(imports_), filter(testImports), error.As(err)!);

        }

        // buildStacks computes minimal import stacks for each package,
        // for use in error messages. When it completes, packages that
        // are part of the original root set have pkg.stack == nil,
        // and other packages have pkg.stack pointing at the next
        // package up the import stack in their minimal chain.
        // As a side effect, buildStacks also constructs ld.pkgs,
        // the list of all packages loaded.
        private static void buildStacks(this ptr<loader> _addr_ld) => func((_, panic, __) =>
        {
            ref loader ld = ref _addr_ld.val;

            if (len(ld.pkgs) > 0L)
            {
                panic("buildStacks");
            }

            {
                var pkg__prev1 = pkg;

                foreach (var (_, __pkg) in ld.roots)
                {
                    pkg = __pkg;
                    pkg.stack = pkg; // sentinel to avoid processing in next loop
                    ld.pkgs = append(ld.pkgs, pkg);

                }

                pkg = pkg__prev1;
            }

            for (long i = 0L; i < len(ld.pkgs); i++)
            { // not range: appending to ld.pkgs in loop
                var pkg = ld.pkgs[i];
                {
                    var next__prev2 = next;

                    foreach (var (_, __next) in pkg.imports)
                    {
                        next = __next;
                        if (next.stack == null)
                        {
                            next.stack = pkg;
                            ld.pkgs = append(ld.pkgs, next);
                        }

                    }

                    next = next__prev2;
                }

                {
                    var next__prev1 = next;

                    var next = pkg.test;

                    if (next != null && next.stack == null)
                    {
                        next.stack = pkg;
                        ld.pkgs = append(ld.pkgs, next);
                    }

                    next = next__prev1;

                }

            }

            {
                var pkg__prev1 = pkg;

                foreach (var (_, __pkg) in ld.roots)
                {
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
        private static @string stackText(this ptr<loadPkg> _addr_pkg)
        {
            ref loadPkg pkg = ref _addr_pkg.val;

            slice<ptr<loadPkg>> stack = default;
            {
                var p__prev1 = p;

                var p = pkg;

                while (p != null)
                {
                    stack = append(stack, p);
                    p = p.stack;
                }


                p = p__prev1;
            }

            ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
            for (var i = len(stack) - 1L; i >= 0L; i--)
            {
                p = stack[i];
                fmt.Fprint(_addr_buf, p.path);
                if (p.testOf != null)
                {
                    fmt.Fprint(_addr_buf, ".test");
                }

                if (i > 0L)
                {
                    if (stack[i - 1L].testOf == p)
                    {
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
        private static @string why(this ptr<loadPkg> _addr_pkg)
        {
            ref loadPkg pkg = ref _addr_pkg.val;

            ref strings.Builder buf = ref heap(out ptr<strings.Builder> _addr_buf);
            slice<ptr<loadPkg>> stack = default;
            {
                var p__prev1 = p;

                var p = pkg;

                while (p != null)
                {
                    stack = append(stack, p);
                    p = p.stack;
                }


                p = p__prev1;
            }

            for (var i = len(stack) - 1L; i >= 0L; i--)
            {
                p = stack[i];
                if (p.testOf != null)
                {
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
        // The package graph must have been loaded already, usually by LoadALL.
        // If there is no reason for the package to be in the current build,
        // Why returns an empty string.
        public static @string Why(@string path)
        {
            ptr<loadPkg> (pkg, ok) = loaded.pkgCache.Get(path)._<ptr<loadPkg>>();
            if (!ok)
            {
                return "";
            }

            return pkg.why();

        }

        // WhyDepth returns the number of steps in the Why listing.
        // If there is no reason for the package to be in the current build,
        // WhyDepth returns 0.
        public static long WhyDepth(@string path)
        {
            long n = 0L;
            ptr<loadPkg> (pkg, _) = loaded.pkgCache.Get(path)._<ptr<loadPkg>>();
            {
                var p = pkg;

                while (p != null)
                {
                    n++;
                    p = p.stack;
                }

            }
            return n;

        }
    }
}}}}
