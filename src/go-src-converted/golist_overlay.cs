// package packages -- go2cs converted at 2020 October 09 06:01:54 UTC
// import "golang.org/x/tools/go/packages" ==> using packages = go.golang.org.x.tools.go.packages_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\packages\golist_overlay.go
using json = go.encoding.json_package;
using fmt = go.fmt_package;
using parser = go.go.parser_package;
using token = go.go.token_package;
using log = go.log_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go
{
    public static partial class packages_package
    {
        // processGolistOverlay provides rudimentary support for adding
        // files that don't exist on disk to an overlay. The results can be
        // sometimes incorrect.
        // TODO(matloob): Handle unsupported cases, including the following:
        // - determining the correct package to add given a new import path
        private static (slice<@string>, slice<@string>, error) processGolistOverlay(this ptr<golistState> _addr_state, ptr<responseDeduper> _addr_response)
        {
            slice<@string> modifiedPkgs = default;
            slice<@string> needPkgs = default;
            error err = default!;
            ref golistState state = ref _addr_state.val;
            ref responseDeduper response = ref _addr_response.val;

            var havePkgs = make_map<@string, @string>(); // importPath -> non-test package ID
            var needPkgsSet = make_map<@string, bool>();
            var modifiedPkgsSet = make_map<@string, bool>();

            var pkgOfDir = make_map<@string, slice<ptr<Package>>>();
            {
                var pkg__prev1 = pkg;

                foreach (var (_, __pkg) in response.dr.Packages)
                {
                    pkg = __pkg; 
                    // This is an approximation of import path to id. This can be
                    // wrong for tests, vendored packages, and a number of other cases.
                    havePkgs[pkg.PkgPath] = pkg.ID;
                    var x = commonDir(pkg.GoFiles);
                    if (x != "")
                    {
                        pkgOfDir[x] = append(pkgOfDir[x], pkg);
                    }
                }
                pkg = pkg__prev1;
            }

            bool overlayAddsImports = default; 

            // If both a package and its test package are created by the overlay, we
            // need the real package first. Process all non-test files before test
            // files, and make the whole process deterministic while we're at it.
            slice<@string> overlayFiles = default;
            {
                var opath__prev1 = opath;

                foreach (var (__opath) in state.cfg.Overlay)
                {
                    opath = __opath;
                    overlayFiles = append(overlayFiles, opath);
                }
                opath = opath__prev1;
            }

            sort.Slice(overlayFiles, (i, j) =>
            {
                var iTest = strings.HasSuffix(overlayFiles[i], "_test.go");
                var jTest = strings.HasSuffix(overlayFiles[j], "_test.go");
                if (iTest != jTest)
                {
                    return !iTest; // non-tests are before tests.
                }
                return overlayFiles[i] < overlayFiles[j];

            });
            {
                var opath__prev1 = opath;

                foreach (var (_, __opath) in overlayFiles)
                {
                    opath = __opath;
                    var contents = state.cfg.Overlay[opath];
                    var @base = filepath.Base(opath);
                    var dir = filepath.Dir(opath);
                    ptr<Package> pkg; // if opath belongs to both a package and its test variant, this will be the test variant
                    ptr<Package> testVariantOf; // if opath is a test file, this is the package it is testing
                    bool fileExists = default;
                    var isTestFile = strings.HasSuffix(opath, "_test.go");
                    var (pkgName, ok) = extractPackageName(opath, contents);
                    if (!ok)
                    { 
                        // Don't bother adding a file that doesn't even have a parsable package statement
                        // to the overlay.
                        continue;

                    }
                    maybeFixPackageName(pkgName, isTestFile, pkgOfDir[dir]);
nextPackage: 
                    // The overlay could have included an entirely new package.
                    {
                        var p__prev2 = p;

                        foreach (var (_, __p) in response.dr.Packages)
                        {
                            p = __p;
                            if (pkgName != p.Name && p.ID != "command-line-arguments")
                            {
                                continue;
                            }
                            foreach (var (_, f) in p.GoFiles)
                            {
                                if (!sameFile(filepath.Dir(f), dir))
                                {
                                    continue;
                                }
                                if (isTestFile && !hasTestFiles(_addr_p))
                                { 
                                    // TODO(matloob): Are there packages other than the 'production' variant
                                    // of a package that this can match? This shouldn't match the test main package
                                    // because the file is generated in another directory.
                                    testVariantOf = p;
                                    _continuenextPackage = true;
                                    break;
                                }
                                if (pkg != null && p != pkg && pkg.PkgPath == p.PkgPath)
                                {
                                    if (hasTestFiles(_addr_p))
                                    {
                                        testVariantOf = addr(pkg);
                                    }
                                }
                                pkg = p;
                                if (filepath.Base(f) == base)
                                {
                                    fileExists = true;
                                }
                            }
                        }
                        p = p__prev2;
                    }
                    if (pkg == null)
                    { 
                        // Try to find the module or gopath dir the file is contained in.
                        // Then for modules, add the module opath to the beginning.
                        var (pkgPath, ok, err) = state.getPkgPath(dir);
                        if (err != null)
                        {
                            return (null, null, error.As(err)!);
                        }
                        if (!ok)
                        {
                            break;
                        }
                        var isXTest = strings.HasSuffix(pkgName, "_test");
                        if (isXTest)
                        {
                            pkgPath += "_test";
                        }
                        var id = pkgPath;
                        if (isTestFile && !isXTest)
                        {
                            id = fmt.Sprintf("%s [%s.test]", pkgPath, pkgPath);
                        }
                        {
                            var p__prev2 = p;

                            foreach (var (_, __p) in response.dr.Packages)
                            {
                                p = __p;
                                if (reclaimPackage(_addr_p, id, opath, contents))
                                {
                                    pkg = p;
                                    break;
                                }
                            }
                            p = p__prev2;
                        }

                        if (pkg == null)
                        {
                            pkg = addr(new Package(PkgPath:pkgPath,ID:id,Name:pkgName,Imports:make(map[string]*Package),));
                            response.addPackage(pkg);
                            havePkgs[pkg.PkgPath] = id; 
                            // Add the production package's sources for a test variant.
                            if (isTestFile && !isXTest && testVariantOf != null)
                            {
                                pkg.GoFiles = append(pkg.GoFiles, testVariantOf.GoFiles);
                                pkg.CompiledGoFiles = append(pkg.CompiledGoFiles, testVariantOf.CompiledGoFiles); 
                                // Add the package under test and its imports to the test variant.
                                pkg.forTest = testVariantOf.PkgPath;
                                foreach (var (k, v) in testVariantOf.Imports)
                                {
                                    pkg.Imports[k] = addr(new Package(ID:v.ID));
                                }
                            }
                        }
                    }
                    if (!fileExists)
                    {
                        pkg.GoFiles = append(pkg.GoFiles, opath); 
                        // TODO(matloob): Adding the file to CompiledGoFiles can exhibit the wrong behavior
                        // if the file will be ignored due to its build tags.
                        pkg.CompiledGoFiles = append(pkg.CompiledGoFiles, opath);
                        modifiedPkgsSet[pkg.ID] = true;

                    }
                    var (imports, err) = extractImports(opath, contents);
                    if (err != null)
                    { 
                        // Let the parser or type checker report errors later.
                        continue;

                    }
                    {
                        var imp__prev2 = imp;

                        foreach (var (_, __imp) in imports)
                        {
                            imp = __imp; 
                            // TODO(rstambler): If the package is an x test and the import has
                            // a test variant, make sure to replace it.
                            {
                                var (_, found) = pkg.Imports[imp];

                                if (found)
                                {
                                    continue;
                                }
                            }

                            overlayAddsImports = true;
                            var (id, ok) = havePkgs[imp];
                            if (!ok)
                            {
                                error err = default!;
                                id, err = state.resolveImport(dir, imp);
                                if (err != null)
                                {
                                    return (null, null, error.As(err)!);
                                }
                            }
                            pkg.Imports[imp] = addr(new Package(ID:id)); 
                            // Add dependencies to the non-test variant version of this package as well.
                            if (testVariantOf != null)
                            {
                                testVariantOf.Imports[imp] = addr(new Package(ID:id));
                            }
                        }
                        imp = imp__prev2;
                    }
                }
                opath = opath__prev1;
            }

            Func<@string, @string, (@string, error)> toPkgPath = (sourceDir, id) =>
            {
                {
                    var i = strings.IndexByte(id, ' ');

                    if (i >= 0L)
                    {
                        return state.resolveImport(sourceDir, id[..i]);
                    }
                }

                return state.resolveImport(sourceDir, id);

            }; 

            // Now that new packages have been created, do another pass to determine
            // the new set of missing packages.
            {
                var pkg__prev1 = pkg;

                foreach (var (_, __pkg) in response.dr.Packages)
                {
                    pkg = __pkg;
                    {
                        var imp__prev2 = imp;

                        foreach (var (_, __imp) in pkg.Imports)
                        {
                            imp = __imp;
                            if (len(pkg.GoFiles) == 0L)
                            {
                                return (null, null, error.As(fmt.Errorf("cannot resolve imports for package %q with no Go files", pkg.PkgPath))!);
                            }
                            var (pkgPath, err) = toPkgPath(filepath.Dir(pkg.GoFiles[0L]), imp.ID);
                            if (err != null)
                            {
                                return (null, null, error.As(err)!);
                            }
                            {
                                var (_, ok) = havePkgs[pkgPath];

                                if (!ok)
                                {
                                    needPkgsSet[pkgPath] = true;
                                }
                            }

                        }
                        imp = imp__prev2;
                    }
                }
                pkg = pkg__prev1;
            }

            if (overlayAddsImports)
            {
                needPkgs = make_slice<@string>(0L, len(needPkgsSet));
                {
                    var pkg__prev1 = pkg;

                    foreach (var (__pkg) in needPkgsSet)
                    {
                        pkg = __pkg;
                        needPkgs = append(needPkgs, pkg);
                    }
                    pkg = pkg__prev1;
                }
            }
            modifiedPkgs = make_slice<@string>(0L, len(modifiedPkgsSet));
            {
                var pkg__prev1 = pkg;

                foreach (var (__pkg) in modifiedPkgsSet)
                {
                    pkg = __pkg;
                    modifiedPkgs = append(modifiedPkgs, pkg);
                }
                pkg = pkg__prev1;
            }

            return (modifiedPkgs, needPkgs, error.As(err)!);

        }

        // resolveImport finds the the ID of a package given its import path.
        // In particular, it will find the right vendored copy when in GOPATH mode.
        private static (@string, error) resolveImport(this ptr<golistState> _addr_state, @string sourceDir, @string importPath)
        {
            @string _p0 = default;
            error _p0 = default!;
            ref golistState state = ref _addr_state.val;

            var (env, err) = state.getEnv();
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            if (env["GOMOD"] != "")
            {
                return (importPath, error.As(null!)!);
            }

            var searchDir = sourceDir;
            while (true)
            {
                var vendorDir = filepath.Join(searchDir, "vendor");
                var (exists, ok) = state.vendorDirs[vendorDir];
                if (!ok)
                {
                    var (info, err) = os.Stat(vendorDir);
                    exists = err == null && info.IsDir();
                    state.vendorDirs[vendorDir] = exists;
                }

                if (exists)
                {
                    var vendoredPath = filepath.Join(vendorDir, importPath);
                    {
                        var info__prev2 = info;

                        (info, err) = os.Stat(vendoredPath);

                        if (err == null && info.IsDir())
                        { 
                            // We should probably check for .go files here, but shame on anyone who fools us.
                            var (path, ok, err) = state.getPkgPath(vendoredPath);
                            if (err != null)
                            {
                                return ("", error.As(err)!);
                            }

                            if (ok)
                            {
                                return (path, error.As(null!)!);
                            }

                        }

                        info = info__prev2;

                    }

                } 

                // We know we've hit the top of the filesystem when we Dir / and get /,
                // or C:\ and get C:\, etc.
                var next = filepath.Dir(searchDir);
                if (next == searchDir)
                {
                    break;
                }

                searchDir = next;

            }

            return (importPath, error.As(null!)!);

        }

        private static bool hasTestFiles(ptr<Package> _addr_p)
        {
            ref Package p = ref _addr_p.val;

            foreach (var (_, f) in p.GoFiles)
            {
                if (strings.HasSuffix(f, "_test.go"))
                {
                    return true;
                }

            }
            return false;

        }

        // determineRootDirs returns a mapping from absolute directories that could
        // contain code to their corresponding import path prefixes.
        private static (map<@string, @string>, error) determineRootDirs(this ptr<golistState> _addr_state)
        {
            map<@string, @string> _p0 = default;
            error _p0 = default!;
            ref golistState state = ref _addr_state.val;

            var (env, err) = state.getEnv();
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            if (env["GOMOD"] != "")
            {
                state.rootsOnce.Do(() =>
                {
                    state.rootDirs, state.rootDirsError = state.determineRootDirsModules();
                }
            else
);

            }            {
                state.rootsOnce.Do(() =>
                {
                    state.rootDirs, state.rootDirsError = state.determineRootDirsGOPATH();
                });

            }

            return (state.rootDirs, error.As(state.rootDirsError)!);

        }

        private static (map<@string, @string>, error) determineRootDirsModules(this ptr<golistState> _addr_state)
        {
            map<@string, @string> _p0 = default;
            error _p0 = default!;
            ref golistState state = ref _addr_state.val;
 
            // This will only return the root directory for the main module.
            // For now we only support overlays in main modules.
            // Editing files in the module cache isn't a great idea, so we don't
            // plan to ever support that, but editing files in replaced modules
            // is something we may want to support. To do that, we'll want to
            // do a go list -m to determine the replaced module's module path and
            // directory, and then a go list -m {{with .Replace}}{{.Dir}}{{end}} <replaced module's path>
            // from the main module to determine if that module is actually a replacement.
            // See bcmills's comment here: https://github.com/golang/go/issues/37629#issuecomment-594179751
            // for more information.
            var (out, err) = state.invokeGo("list", "-m", "-json");
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            map m = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{};
            private partial struct jsonMod
            {
                public @string Path;
                public @string Dir;
            }
            {
                var dec = json.NewDecoder(out);

                while (dec.More())
                {
                    ptr<jsonMod> mod = @new<jsonMod>();
                    {
                        var err = dec.Decode(mod);

                        if (err != null)
                        {
                            return (null, error.As(err)!);
                        }

                    }

                    if (mod.Dir != "" && mod.Path != "")
                    { 
                        // This is a valid module; add it to the map.
                        var (absDir, err) = filepath.Abs(mod.Dir);
                        if (err != null)
                        {
                            return (null, error.As(err)!);
                        }

                        m[absDir] = mod.Path;

                    }

                }

            }
            return (m, error.As(null!)!);

        }

        private static (map<@string, @string>, error) determineRootDirsGOPATH(this ptr<golistState> _addr_state)
        {
            map<@string, @string> _p0 = default;
            error _p0 = default!;
            ref golistState state = ref _addr_state.val;

            map m = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{};
            foreach (var (_, dir) in filepath.SplitList(state.mustGetEnv()["GOPATH"]))
            {
                var (absDir, err) = filepath.Abs(dir);
                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                m[filepath.Join(absDir, "src")] = "";

            }
            return (m, error.As(null!)!);

        }

        private static (slice<@string>, error) extractImports(@string filename, slice<byte> contents)
        {
            slice<@string> _p0 = default;
            error _p0 = default!;

            var (f, err) = parser.ParseFile(token.NewFileSet(), filename, contents, parser.ImportsOnly); // TODO(matloob): reuse fileset?
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            slice<@string> res = default;
            foreach (var (_, imp) in f.Imports)
            {
                var quotedPath = imp.Path.Value;
                var (path, err) = strconv.Unquote(quotedPath);
                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                res = append(res, path);

            }
            return (res, error.As(null!)!);

        }

        // reclaimPackage attempts to reuse a package that failed to load in an overlay.
        //
        // If the package has errors and has no Name, GoFiles, or Imports,
        // then it's possible that it doesn't yet exist on disk.
        private static bool reclaimPackage(ptr<Package> _addr_pkg, @string id, @string filename, slice<byte> contents)
        {
            ref Package pkg = ref _addr_pkg.val;
 
            // TODO(rstambler): Check the message of the actual error?
            // It differs between $GOPATH and module mode.
            if (pkg.ID != id)
            {
                return false;
            }

            if (len(pkg.Errors) != 1L)
            {
                return false;
            }

            if (pkg.Name != "" || pkg.ExportFile != "")
            {
                return false;
            }

            if (len(pkg.GoFiles) > 0L || len(pkg.CompiledGoFiles) > 0L || len(pkg.OtherFiles) > 0L)
            {
                return false;
            }

            if (len(pkg.Imports) > 0L)
            {
                return false;
            }

            var (pkgName, ok) = extractPackageName(filename, contents);
            if (!ok)
            {
                return false;
            }

            pkg.Name = pkgName;
            pkg.Errors = null;
            return true;

        }

        private static (@string, bool) extractPackageName(@string filename, slice<byte> contents)
        {
            @string _p0 = default;
            bool _p0 = default;
 
            // TODO(rstambler): Check the message of the actual error?
            // It differs between $GOPATH and module mode.
            var (f, err) = parser.ParseFile(token.NewFileSet(), filename, contents, parser.PackageClauseOnly); // TODO(matloob): reuse fileset?
            if (err != null)
            {
                return ("", false);
            }

            return (f.Name.Name, true);

        }

        private static @string commonDir(slice<@string> a)
        {
            var seen = make_map<@string, bool>();
            var x = append(new slice<@string>(new @string[] {  }), a);
            foreach (var (_, f) in x)
            {
                seen[filepath.Dir(f)] = true;
            }
            if (len(seen) > 1L)
            {
                log.Fatalf("commonDir saw %v for %v", seen, x);
            }

            foreach (var (k) in seen)
            { 
                // len(seen) == 1
                return k;

            }
            return ""; // no files
        }

        // It is possible that the files in the disk directory dir have a different package
        // name from newName, which is deduced from the overlays. If they all have a different
        // package name, and they all have the same package name, then that name becomes
        // the package name.
        // It returns true if it changes the package name, false otherwise.
        private static void maybeFixPackageName(@string newName, bool isTestFile, slice<ptr<Package>> pkgsOfDir)
        {
            var names = make_map<@string, long>();
            {
                var p__prev1 = p;

                foreach (var (_, __p) in pkgsOfDir)
                {
                    p = __p;
                    names[p.Name]++;
                }

                p = p__prev1;
            }

            if (len(names) != 1L)
            { 
                // some files are in different packages
                return ;

            }

            @string oldName = default;
            foreach (var (k) in names)
            {
                oldName = k;
            }
            if (newName == oldName)
            {
                return ;
            } 
            // We might have a case where all of the package names in the directory are
            // the same, but the overlay file is for an x test, which belongs to its
            // own package. If the x test does not yet exist on disk, we may not yet
            // have its package name on disk, but we should not rename the packages.
            //
            // We use a heuristic to determine if this file belongs to an x test:
            // The test file should have a package name whose package name has a _test
            // suffix or looks like "newName_test".
            var maybeXTest = strings.HasPrefix(oldName + "_test", newName) || strings.HasSuffix(newName, "_test");
            if (isTestFile && maybeXTest)
            {
                return ;
            }

            {
                var p__prev1 = p;

                foreach (var (_, __p) in pkgsOfDir)
                {
                    p = __p;
                    p.Name = newName;
                }

                p = p__prev1;
            }
        }
    }
}}}}}
