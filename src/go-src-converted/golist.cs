// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package packages -- go2cs converted at 2020 October 08 04:54:47 UTC
// import "golang.org/x/tools/go/packages" ==> using packages = go.golang.org.x.tools.go.packages_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\packages\golist.go
using bytes = go.bytes_package;
using context = go.context_package;
using json = go.encoding.json_package;
using fmt = go.fmt_package;
using types = go.go.types_package;
using log = go.log_package;
using os = go.os_package;
using exec = go.os.exec_package;
using path = go.path_package;
using filepath = go.path.filepath_package;
using reflect = go.reflect_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;
using unicode = go.unicode_package;

using packagesdriver = go.golang.org.x.tools.go.@internal.packagesdriver_package;
using gocommand = go.golang.org.x.tools.@internal.gocommand_package;
using xerrors = go.golang.org.x.xerrors_package;
using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go
{
    public static partial class packages_package
    {
        // debug controls verbose logging.


        // A goTooOldError reports that the go command
        // found by exec.LookPath is too old to use the new go list behavior.
        private partial struct goTooOldError : error
        {
            public error error;
        }

        // responseDeduper wraps a driverResponse, deduplicating its contents.
        private partial struct responseDeduper
        {
            public map<@string, bool> seenRoots;
            public map<@string, ptr<Package>> seenPackages;
            public ptr<driverResponse> dr;
        }

        private static ptr<responseDeduper> newDeduper()
        {
            return addr(new responseDeduper(dr:&driverResponse{},seenRoots:map[string]bool{},seenPackages:map[string]*Package{},));
        }

        // addAll fills in r with a driverResponse.
        private static void addAll(this ptr<responseDeduper> _addr_r, ptr<driverResponse> _addr_dr)
        {
            ref responseDeduper r = ref _addr_r.val;
            ref driverResponse dr = ref _addr_dr.val;

            foreach (var (_, pkg) in dr.Packages)
            {
                r.addPackage(pkg);
            }
            foreach (var (_, root) in dr.Roots)
            {
                r.addRoot(root);
            }

        }

        private static void addPackage(this ptr<responseDeduper> _addr_r, ptr<Package> _addr_p)
        {
            ref responseDeduper r = ref _addr_r.val;
            ref Package p = ref _addr_p.val;

            if (r.seenPackages[p.ID] != null)
            {
                return ;
            }

            r.seenPackages[p.ID] = p;
            r.dr.Packages = append(r.dr.Packages, p);

        }

        private static void addRoot(this ptr<responseDeduper> _addr_r, @string id)
        {
            ref responseDeduper r = ref _addr_r.val;

            if (r.seenRoots[id])
            {
                return ;
            }

            r.seenRoots[id] = true;
            r.dr.Roots = append(r.dr.Roots, id);

        }

        private partial struct golistState
        {
            public ptr<Config> cfg;
            public context.Context ctx;
            public sync.Once envOnce;
            public error goEnvError;
            public map<@string, @string> goEnv;
            public sync.Once rootsOnce;
            public error rootDirsError;
            public map<@string, @string> rootDirs; // vendorDirs caches the (non)existence of vendor directories.
            public map<@string, bool> vendorDirs;
        }

        // getEnv returns Go environment variables. Only specific variables are
        // populated -- computing all of them is slow.
        private static (map<@string, @string>, error) getEnv(this ptr<golistState> _addr_state)
        {
            map<@string, @string> _p0 = default;
            error _p0 = default!;
            ref golistState state = ref _addr_state.val;

            state.envOnce.Do(() =>
            {
                ptr<bytes.Buffer> b;
                b, state.goEnvError = state.invokeGo("env", "-json", "GOMOD", "GOPATH");
                if (state.goEnvError != null)
                {
                    return ;
                }

                state.goEnv = make_map<@string, @string>();
                var decoder = json.NewDecoder(b);
                state.goEnvError = decoder.Decode(_addr_state.goEnv);

                if (state.goEnvError != null)
                {
                    return ;
                }

            });
            return (state.goEnv, error.As(state.goEnvError)!);

        }

        // mustGetEnv is a convenience function that can be used if getEnv has already succeeded.
        private static map<@string, @string> mustGetEnv(this ptr<golistState> _addr_state) => func((_, panic, __) =>
        {
            ref golistState state = ref _addr_state.val;

            var (env, err) = state.getEnv();
            if (err != null)
            {
                panic(fmt.Sprintf("mustGetEnv: %v", err));
            }

            return env;

        });

        // goListDriver uses the go list command to interpret the patterns and produce
        // the build system package structure.
        // See driver for more details.
        private static (ptr<driverResponse>, error) goListDriver(ptr<Config> _addr_cfg, params @string[] patterns) => func((defer, _, __) =>
        {
            ptr<driverResponse> _p0 = default!;
            error _p0 = default!;
            patterns = patterns.Clone();
            ref Config cfg = ref _addr_cfg.val;
 
            // Make sure that any asynchronous go commands are killed when we return.
            var parentCtx = cfg.Context;
            if (parentCtx == null)
            {
                parentCtx = context.Background();
            }

            var (ctx, cancel) = context.WithCancel(parentCtx);
            defer(cancel());

            var response = newDeduper(); 

            // Fill in response.Sizes asynchronously if necessary.
            error sizeserr = default!;
            sync.WaitGroup sizeswg = default;
            if (cfg.Mode & NeedTypesSizes != 0L || cfg.Mode & NeedTypes != 0L)
            {
                sizeswg.Add(1L);
                go_(() => () =>
                {
                    types.Sizes sizes = default;
                    sizes, sizeserr = packagesdriver.GetSizesGolist(ctx, cfg.BuildFlags, cfg.Env, cfg.gocmdRunner, cfg.Dir); 
                    // types.SizesFor always returns nil or a *types.StdSizes.
                    response.dr.Sizes, _ = sizes._<ptr<types.StdSizes>>();
                    sizeswg.Done();

                }());

            }

            ptr<golistState> state = addr(new golistState(cfg:cfg,ctx:ctx,vendorDirs:map[string]bool{},)); 

            // Determine files requested in contains patterns
            slice<@string> containFiles = default;
            var restPatterns = make_slice<@string>(0L, len(patterns)); 
            // Extract file= and other [querytype]= patterns. Report an error if querytype
            // doesn't exist.
extractQueries: 

            // See if we have any patterns to pass through to go list. Zero initial
            // patterns also requires a go list call, since it's the equivalent of
            // ".".
            foreach (var (_, pattern) in patterns)
            {
                var eqidx = strings.Index(pattern, "=");
                if (eqidx < 0L)
                {
                    restPatterns = append(restPatterns, pattern);
                }
                else
                {
                    var query = pattern[..eqidx];
                    var value = pattern[eqidx + len("=")..];
                    switch (query)
                    {
                        case "file": 
                            containFiles = append(containFiles, value);
                            break;
                        case "pattern": 
                            restPatterns = append(restPatterns, value);
                            break;
                        case "": // not a reserved query
                            restPatterns = append(restPatterns, pattern);
                            break;
                        default: 
                            foreach (var (_, rune) in query)
                            {
                                if (rune < 'a' || rune > 'z')
                                { // not a reserved query
                                    restPatterns = append(restPatterns, pattern);
                                    _continueextractQueries = true;
                                    break;
                                }

                            } 
                            // Reject all other patterns containing "="
                            return (_addr_null!, error.As(fmt.Errorf("invalid query type %q in query pattern %q", query, pattern))!);
                            break;
                    }

                }

            } 

            // See if we have any patterns to pass through to go list. Zero initial
            // patterns also requires a go list call, since it's the equivalent of
            // ".".
            if (len(restPatterns) > 0L || len(patterns) == 0L)
            {
                var (dr, err) = state.createDriverResponse(restPatterns);
                if (err != null)
                {
                    return (_addr_null!, error.As(err)!);
                }

                response.addAll(dr);

            }

            if (len(containFiles) != 0L)
            {
                {
                    var err__prev2 = err;

                    var err = state.runContainsQueries(response, containFiles);

                    if (err != null)
                    {
                        return (_addr_null!, error.As(err)!);
                    }

                    err = err__prev2;

                }

            }

            var (modifiedPkgs, needPkgs, err) = state.processGolistOverlay(response);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            slice<@string> containsCandidates = default;
            if (len(containFiles) > 0L)
            {
                containsCandidates = append(containsCandidates, modifiedPkgs);
                containsCandidates = append(containsCandidates, needPkgs);
            }

            {
                var err__prev1 = err;

                err = state.addNeededOverlayPackages(response, needPkgs);

                if (err != null)
                {
                    return (_addr_null!, error.As(err)!);
                } 
                // Check candidate packages for containFiles.

                err = err__prev1;

            } 
            // Check candidate packages for containFiles.
            if (len(containFiles) > 0L)
            {
                foreach (var (_, id) in containsCandidates)
                {
                    var (pkg, ok) = response.seenPackages[id];
                    if (!ok)
                    {
                        response.addPackage(addr(new Package(ID:id,Errors:[]Error{{Kind:ListError,Msg:fmt.Sprintf("package %s expected but not seen",id),},},)));
                        continue;
                    }

                    foreach (var (_, f) in containFiles)
                    {
                        foreach (var (_, g) in pkg.GoFiles)
                        {
                            if (sameFile(f, g))
                            {
                                response.addRoot(id);
                            }

                        }

                    }

                }

            }

            sizeswg.Wait();
            if (sizeserr != null)
            {
                return (_addr_null!, error.As(sizeserr)!);
            }

            return (_addr_response.dr!, error.As(null!)!);

        });

        private static error addNeededOverlayPackages(this ptr<golistState> _addr_state, ptr<responseDeduper> _addr_response, slice<@string> pkgs)
        {
            ref golistState state = ref _addr_state.val;
            ref responseDeduper response = ref _addr_response.val;

            if (len(pkgs) == 0L)
            {
                return error.As(null!)!;
            }

            var (dr, err) = state.createDriverResponse(pkgs);
            if (err != null)
            {
                return error.As(err)!;
            }

            foreach (var (_, pkg) in dr.Packages)
            {
                response.addPackage(pkg);
            }
            var (_, needPkgs, err) = state.processGolistOverlay(response);
            if (err != null)
            {
                return error.As(err)!;
            }

            return error.As(state.addNeededOverlayPackages(response, needPkgs))!;

        }

        private static error runContainsQueries(this ptr<golistState> _addr_state, ptr<responseDeduper> _addr_response, slice<@string> queries)
        {
            ref golistState state = ref _addr_state.val;
            ref responseDeduper response = ref _addr_response.val;

            foreach (var (_, query) in queries)
            { 
                // TODO(matloob): Do only one query per directory.
                var fdir = filepath.Dir(query); 
                // Pass absolute path of directory to go list so that it knows to treat it as a directory,
                // not a package path.
                var (pattern, err) = filepath.Abs(fdir);
                if (err != null)
                {
                    return error.As(fmt.Errorf("could not determine absolute path of file= query path %q: %v", query, err))!;
                }

                var (dirResponse, err) = state.createDriverResponse(pattern); 

                // If there was an error loading the package, or the package is returned
                // with errors, try to load the file as an ad-hoc package.
                // Usually the error will appear in a returned package, but may not if we're
                // in module mode and the ad-hoc is located outside a module.
                if (err != null || len(dirResponse.Packages) == 1L && len(dirResponse.Packages[0L].GoFiles) == 0L && len(dirResponse.Packages[0L].Errors) == 1L)
                {
                    error queryErr = default!;
                    dirResponse, queryErr = state.adhocPackage(pattern, query);

                    if (queryErr != null)
                    {
                        return error.As(err)!; // return the original error
                    }

                }

                var isRoot = make_map<@string, bool>(len(dirResponse.Roots));
                foreach (var (_, root) in dirResponse.Roots)
                {
                    isRoot[root] = true;
                }
                foreach (var (_, pkg) in dirResponse.Packages)
                { 
                    // Add any new packages to the main set
                    // We don't bother to filter packages that will be dropped by the changes of roots,
                    // that will happen anyway during graph construction outside this function.
                    // Over-reporting packages is not a problem.
                    response.addPackage(pkg); 
                    // if the package was not a root one, it cannot have the file
                    if (!isRoot[pkg.ID])
                    {
                        continue;
                    }

                    foreach (var (_, pkgFile) in pkg.GoFiles)
                    {
                        if (filepath.Base(query) == filepath.Base(pkgFile))
                        {
                            response.addRoot(pkg.ID);
                            break;
                        }

                    }

                }

            }
            return error.As(null!)!;

        }

        // adhocPackage attempts to load or construct an ad-hoc package for a given
        // query, if the original call to the driver produced inadequate results.
        private static (ptr<driverResponse>, error) adhocPackage(this ptr<golistState> _addr_state, @string pattern, @string query)
        {
            ptr<driverResponse> _p0 = default!;
            error _p0 = default!;
            ref golistState state = ref _addr_state.val;

            var (response, err) = state.createDriverResponse(query);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            } 
            // If we get nothing back from `go list`,
            // try to make this file into its own ad-hoc package.
            // TODO(rstambler): Should this check against the original response?
            if (len(response.Packages) == 0L)
            {
                response.Packages = append(response.Packages, addr(new Package(ID:"command-line-arguments",PkgPath:query,GoFiles:[]string{query},CompiledGoFiles:[]string{query},Imports:make(map[string]*Package),)));
                response.Roots = append(response.Roots, "command-line-arguments");
            } 
            // Handle special cases.
            if (len(response.Packages) == 1L)
            { 
                // golang/go#33482: If this is a file= query for ad-hoc packages where
                // the file only exists on an overlay, and exists outside of a module,
                // add the file to the package and remove the errors.
                if (response.Packages[0L].ID == "command-line-arguments" || filepath.ToSlash(response.Packages[0L].PkgPath) == filepath.ToSlash(query))
                {
                    if (len(response.Packages[0L].GoFiles) == 0L)
                    {
                        var filename = filepath.Join(pattern, filepath.Base(query)); // avoid recomputing abspath
                        // TODO(matloob): check if the file is outside of a root dir?
                        foreach (var (path) in state.cfg.Overlay)
                        {
                            if (path == filename)
                            {
                                response.Packages[0L].Errors = null;
                                response.Packages[0L].GoFiles = new slice<@string>(new @string[] { path });
                                response.Packages[0L].CompiledGoFiles = new slice<@string>(new @string[] { path });
                            }

                        }

                    }

                }

            }

            return (_addr_response!, error.As(null!)!);

        }

        // Fields must match go list;
        // see $GOROOT/src/cmd/go/internal/load/pkg.go.
        private partial struct jsonPackage
        {
            public @string ImportPath;
            public @string Dir;
            public @string Name;
            public @string Export;
            public slice<@string> GoFiles;
            public slice<@string> CompiledGoFiles;
            public slice<@string> CFiles;
            public slice<@string> CgoFiles;
            public slice<@string> CXXFiles;
            public slice<@string> MFiles;
            public slice<@string> HFiles;
            public slice<@string> FFiles;
            public slice<@string> SFiles;
            public slice<@string> SwigFiles;
            public slice<@string> SwigCXXFiles;
            public slice<@string> SysoFiles;
            public slice<@string> Imports;
            public map<@string, @string> ImportMap;
            public slice<@string> Deps;
            public ptr<Module> Module;
            public slice<@string> TestGoFiles;
            public slice<@string> TestImports;
            public slice<@string> XTestGoFiles;
            public slice<@string> XTestImports;
            public @string ForTest; // q in a "p [q.test]" package, else ""
            public bool DepOnly;
            public ptr<jsonPackageError> Error;
        }

        private partial struct jsonPackageError
        {
            public slice<@string> ImportStack;
            public @string Pos;
            public @string Err;
        }

        private static slice<slice<@string>> otherFiles(ptr<jsonPackage> _addr_p)
        {
            ref jsonPackage p = ref _addr_p.val;

            return new slice<slice<@string>>(new slice<@string>[] { p.CFiles, p.CXXFiles, p.MFiles, p.HFiles, p.FFiles, p.SFiles, p.SwigFiles, p.SwigCXXFiles, p.SysoFiles });
        }

        // createDriverResponse uses the "go list" command to expand the pattern
        // words and return a response for the specified packages.
        private static (ptr<driverResponse>, error) createDriverResponse(this ptr<golistState> _addr_state, params @string[] words)
        {
            ptr<driverResponse> _p0 = default!;
            error _p0 = default!;
            words = words.Clone();
            ref golistState state = ref _addr_state.val;
 
            // go list uses the following identifiers in ImportPath and Imports:
            //
            //     "p"            -- importable package or main (command)
            //     "q.test"        -- q's test executable
            //     "p [q.test]"        -- variant of p as built for q's test executable
            //     "q_test [q.test]"    -- q's external test package
            //
            // The packages p that are built differently for a test q.test
            // are q itself, plus any helpers used by the external test q_test,
            // typically including "testing" and all its dependencies.

            // Run "go list" for complete
            // information on the specified packages.
            var (buf, err) = state.invokeGo("list", golistargs(_addr_state.cfg, words));
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            var seen = make_map<@string, ptr<jsonPackage>>();
            var pkgs = make_map<@string, ptr<Package>>();
            var additionalErrors = make_map<@string, slice<Error>>(); 
            // Decode the JSON and convert it to Package form.
            ref driverResponse response = ref heap(out ptr<driverResponse> _addr_response);
            {
                var dec = json.NewDecoder(buf);

                while (dec.More())
                {
                    ptr<jsonPackage> p = @new<jsonPackage>();
                    {
                        var err = dec.Decode(p);

                        if (err != null)
                        {
                            return (_addr_null!, error.As(fmt.Errorf("JSON decoding failed: %v", err))!);
                        }

                    }


                    if (p.ImportPath == "")
                    { 
                        // The documentation for go list says that “[e]rroneous packages will have
                        // a non-empty ImportPath”. If for some reason it comes back empty, we
                        // prefer to error out rather than silently discarding data or handing
                        // back a package without any way to refer to it.
                        if (p.Error != null)
                        {
                            return (_addr_null!, error.As(new Error(Pos:p.Error.Pos,Msg:p.Error.Err,))!);
                        }

                        return (_addr_null!, error.As(fmt.Errorf("package missing import path: %+v", p))!);

                    } 

                    // Work around https://golang.org/issue/33157:
                    // go list -e, when given an absolute path, will find the package contained at
                    // that directory. But when no package exists there, it will return a fake package
                    // with an error and the ImportPath set to the absolute path provided to go list.
                    // Try to convert that absolute path to what its package path would be if it's
                    // contained in a known module or GOPATH entry. This will allow the package to be
                    // properly "reclaimed" when overlays are processed.
                    if (filepath.IsAbs(p.ImportPath) && p.Error != null)
                    {
                        var (pkgPath, ok, err) = state.getPkgPath(p.ImportPath);
                        if (err != null)
                        {
                            return (_addr_null!, error.As(err)!);
                        }

                        if (ok)
                        {
                            p.ImportPath = pkgPath;
                        }

                    }

                    {
                        var (old, found) = seen[p.ImportPath];

                        if (found)
                        { 
                            // If one version of the package has an error, and the other doesn't, assume
                            // that this is a case where go list is reporting a fake dependency variant
                            // of the imported package: When a package tries to invalidly import another
                            // package, go list emits a variant of the imported package (with the same
                            // import path, but with an error on it, and the package will have a
                            // DepError set on it). An example of when this can happen is for imports of
                            // main packages: main packages can not be imported, but they may be
                            // separately matched and listed by another pattern.
                            // See golang.org/issue/36188 for more details.

                            // The plan is that eventually, hopefully in Go 1.15, the error will be
                            // reported on the importing package rather than the duplicate "fake"
                            // version of the imported package. Once all supported versions of Go
                            // have the new behavior this logic can be deleted.
                            // TODO(matloob): delete the workaround logic once all supported versions of
                            // Go return the errors on the proper package.

                            // There should be exactly one version of a package that doesn't have an
                            // error.
                            if (old.Error == null && p.Error == null)
                            {
                                if (!reflect.DeepEqual(p, old))
                                {
                                    return (_addr_null!, error.As(fmt.Errorf("internal error: go list gives conflicting information for package %v", p.ImportPath))!);
                                }

                                continue;

                            } 

                            // Determine if this package's error needs to be bubbled up.
                            // This is a hack, and we expect for go list to eventually set the error
                            // on the package.
                            if (old.Error != null)
                            {
                                @string errkind = default;
                                if (strings.Contains(old.Error.Err, "not an importable package"))
                                {
                                    errkind = "not an importable package";
                                }
                                else if (strings.Contains(old.Error.Err, "use of internal package") && strings.Contains(old.Error.Err, "not allowed"))
                                {
                                    errkind = "use of internal package not allowed";
                                }

                                if (errkind != "")
                                {
                                    if (len(old.Error.ImportStack) < 1L)
                                    {
                                        return (_addr_null!, error.As(fmt.Errorf("internal error: go list gave a %q error with empty import stack", errkind))!);
                                    }

                                    var importingPkg = old.Error.ImportStack[len(old.Error.ImportStack) - 1L];
                                    if (importingPkg == old.ImportPath)
                                    { 
                                        // Using an older version of Go which put this package itself on top of import
                                        // stack, instead of the importer. Look for importer in second from top
                                        // position.
                                        if (len(old.Error.ImportStack) < 2L)
                                        {
                                            return (_addr_null!, error.As(fmt.Errorf("internal error: go list gave a %q error with an import stack without importing pa" +
    "ckage", errkind))!);

                                        }

                                        importingPkg = old.Error.ImportStack[len(old.Error.ImportStack) - 2L];

                                    }

                                    additionalErrors[importingPkg] = append(additionalErrors[importingPkg], new Error(Pos:old.Error.Pos,Msg:old.Error.Err,Kind:ListError,));

                                }

                            } 

                            // Make sure that if there's a version of the package without an error,
                            // that's the one reported to the user.
                            if (old.Error == null)
                            {
                                continue;
                            } 

                            // This package will replace the old one at the end of the loop.
                        }

                    }

                    seen[p.ImportPath] = p;

                    ptr<Package> pkg = addr(new Package(Name:p.Name,ID:p.ImportPath,GoFiles:absJoin(p.Dir,p.GoFiles,p.CgoFiles),CompiledGoFiles:absJoin(p.Dir,p.CompiledGoFiles),OtherFiles:absJoin(p.Dir,otherFiles(p)...),forTest:p.ForTest,Module:p.Module,));

                    if ((state.cfg.Mode & typecheckCgo) != 0L && len(p.CgoFiles) != 0L)
                    {
                        if (len(p.CompiledGoFiles) > len(p.GoFiles))
                        { 
                            // We need the cgo definitions, which are in the first
                            // CompiledGoFile after the non-cgo ones. This is a hack but there
                            // isn't currently a better way to find it. We also need the pure
                            // Go files and unprocessed cgo files, all of which are already
                            // in pkg.GoFiles.
                            var cgoTypes = p.CompiledGoFiles[len(p.GoFiles)];
                            pkg.CompiledGoFiles = append(new slice<@string>(new @string[] { cgoTypes }), pkg.GoFiles);

                        }
                        else
                        { 
                            // golang/go#38990: go list silently fails to do cgo processing
                            pkg.CompiledGoFiles = null;
                            pkg.Errors = append(pkg.Errors, new Error(Msg:"go list failed to return CompiledGoFiles; https://golang.org/issue/38990?",Kind:ListError,));

                        }

                    } 

                    // Work around https://golang.org/issue/28749:
                    // cmd/go puts assembly, C, and C++ files in CompiledGoFiles.
                    // Filter out any elements of CompiledGoFiles that are also in OtherFiles.
                    // We have to keep this workaround in place until go1.12 is a distant memory.
                    if (len(pkg.OtherFiles) > 0L)
                    {
                        var other = make_map<@string, bool>(len(pkg.OtherFiles));
                        {
                            var f__prev2 = f;

                            foreach (var (_, __f) in pkg.OtherFiles)
                            {
                                f = __f;
                                other[f] = true;
                            }

                            f = f__prev2;
                        }

                        var @out = pkg.CompiledGoFiles[..0L];
                        {
                            var f__prev2 = f;

                            foreach (var (_, __f) in pkg.CompiledGoFiles)
                            {
                                f = __f;
                                if (other[f])
                                {
                                    continue;
                                }

                                out = append(out, f);

                            }

                            f = f__prev2;
                        }

                        pkg.CompiledGoFiles = out;

                    } 

                    // Extract the PkgPath from the package's ID.
                    {
                        var i = strings.IndexByte(pkg.ID, ' ');

                        if (i >= 0L)
                        {
                            pkg.PkgPath = pkg.ID[..i];
                        }
                        else
                        {
                            pkg.PkgPath = pkg.ID;
                        }

                    }


                    if (pkg.PkgPath == "unsafe")
                    {
                        pkg.GoFiles = null; // ignore fake unsafe.go file
                    } 

                    // Assume go list emits only absolute paths for Dir.
                    if (p.Dir != "" && !filepath.IsAbs(p.Dir))
                    {
                        log.Fatalf("internal error: go list returned non-absolute Package.Dir: %s", p.Dir);
                    }

                    if (p.Export != "" && !filepath.IsAbs(p.Export))
                    {
                        pkg.ExportFile = filepath.Join(p.Dir, p.Export);
                    }
                    else
                    {
                        pkg.ExportFile = p.Export;
                    } 

                    // imports
                    //
                    // Imports contains the IDs of all imported packages.
                    // ImportsMap records (path, ID) only where they differ.
                    var ids = make_map<@string, bool>();
                    {
                        var id__prev2 = id;

                        foreach (var (_, __id) in p.Imports)
                        {
                            id = __id;
                            ids[id] = true;
                        }

                        id = id__prev2;
                    }

                    pkg.Imports = make_map<@string, ptr<Package>>();
                    {
                        var id__prev2 = id;

                        foreach (var (__path, __id) in p.ImportMap)
                        {
                            path = __path;
                            id = __id;
                            pkg.Imports[path] = addr(new Package(ID:id)); // non-identity import
                            delete(ids, id);

                        }

                        id = id__prev2;
                    }

                    {
                        var id__prev2 = id;

                        foreach (var (__id) in ids)
                        {
                            id = __id;
                            if (id == "C")
                            {
                                continue;
                            }

                            pkg.Imports[id] = addr(new Package(ID:id)); // identity import
                        }

                        id = id__prev2;
                    }

                    if (!p.DepOnly)
                    {
                        response.Roots = append(response.Roots, pkg.ID);
                    } 

                    // Work around for pre-go.1.11 versions of go list.
                    // TODO(matloob): they should be handled by the fallback.
                    // Can we delete this?
                    if (len(pkg.CompiledGoFiles) == 0L)
                    {
                        pkg.CompiledGoFiles = pkg.GoFiles;
                    }

                    if (p.Error != null)
                    {
                        var msg = strings.TrimSpace(p.Error.Err); // Trim to work around golang.org/issue/32363.
                        // Address golang.org/issue/35964 by appending import stack to error message.
                        if (msg == "import cycle not allowed" && len(p.Error.ImportStack) != 0L)
                        {
                            msg += fmt.Sprintf(": import stack: %v", p.Error.ImportStack);
                        }

                        pkg.Errors = append(pkg.Errors, new Error(Pos:p.Error.Pos,Msg:msg,Kind:ListError,));

                    }

                    pkgs[pkg.ID] = pkg;

                }

            }

            {
                var id__prev1 = id;

                foreach (var (__id, __errs) in additionalErrors)
                {
                    id = __id;
                    errs = __errs;
                    {
                        ptr<jsonPackage> p__prev1 = p;

                        var (p, ok) = pkgs[id];

                        if (ok)
                        {
                            p.Errors = append(p.Errors, errs);
                        }

                        p = p__prev1;

                    }

                }

                id = id__prev1;
            }

            {
                Package pkg__prev1 = pkg;

                foreach (var (_, __pkg) in pkgs)
                {
                    pkg = __pkg;
                    response.Packages = append(response.Packages, pkg);
                }

                pkg = pkg__prev1;
            }

            sort.Slice(response.Packages, (i, j) => _addr_response.Packages[i].ID < response.Packages[j].ID!);

            return (_addr__addr_response!, error.As(null!)!);

        }

        // getPkgPath finds the package path of a directory if it's relative to a root directory.
        private static (@string, bool, error) getPkgPath(this ptr<golistState> _addr_state, @string dir)
        {
            @string _p0 = default;
            bool _p0 = default;
            error _p0 = default!;
            ref golistState state = ref _addr_state.val;

            var (absDir, err) = filepath.Abs(dir);
            if (err != null)
            {
                return ("", false, error.As(err)!);
            }

            var (roots, err) = state.determineRootDirs();
            if (err != null)
            {
                return ("", false, error.As(err)!);
            }

            foreach (var (rdir, rpath) in roots)
            { 
                // Make sure that the directory is in the module,
                // to avoid creating a path relative to another module.
                if (!strings.HasPrefix(absDir, rdir))
                {
                    continue;
                } 
                // TODO(matloob): This doesn't properly handle symlinks.
                var (r, err) = filepath.Rel(rdir, dir);
                if (err != null)
                {
                    continue;
                }

                if (rpath != "")
                { 
                    // We choose only one root even though the directory even it can belong in multiple modules
                    // or GOPATH entries. This is okay because we only need to work with absolute dirs when a
                    // file is missing from disk, for instance when gopls calls go/packages in an overlay.
                    // Once the file is saved, gopls, or the next invocation of the tool will get the correct
                    // result straight from golist.
                    // TODO(matloob): Implement module tiebreaking?
                    return (path.Join(rpath, filepath.ToSlash(r)), true, error.As(null!)!);

                }

                return (filepath.ToSlash(r), true, error.As(null!)!);

            }
            return ("", false, error.As(null!)!);

        }

        // absJoin absolutizes and flattens the lists of files.
        private static slice<@string> absJoin(@string dir, params slice<@string>[] fileses)
        {
            slice<@string> res = default;
            fileses = fileses.Clone();

            foreach (var (_, files) in fileses)
            {
                foreach (var (_, file) in files)
                {
                    if (!filepath.IsAbs(file))
                    {
                        file = filepath.Join(dir, file);
                    }

                    res = append(res, file);

                }

            }
            return res;

        }

        private static slice<@string> golistargs(ptr<Config> _addr_cfg, slice<@string> words)
        {
            ref Config cfg = ref _addr_cfg.val;

            const var findFlags = (var)NeedImports | NeedTypes | NeedSyntax | NeedTypesInfo;

            @string fullargs = new slice<@string>(new @string[] { "-e", "-json", fmt.Sprintf("-compiled=%t",cfg.Mode&(NeedCompiledGoFiles|NeedSyntax|NeedTypes|NeedTypesInfo|NeedTypesSizes)!=0), fmt.Sprintf("-test=%t",cfg.Tests), fmt.Sprintf("-export=%t",usesExportData(cfg)), fmt.Sprintf("-deps=%t",cfg.Mode&NeedImports!=0), fmt.Sprintf("-find=%t",!cfg.Tests&&cfg.Mode&findFlags==0) });
            fullargs = append(fullargs, cfg.BuildFlags);
            fullargs = append(fullargs, "--");
            fullargs = append(fullargs, words);
            return fullargs;
        }

        // invokeGo returns the stdout of a go command invocation.
        private static (ptr<bytes.Buffer>, error) invokeGo(this ptr<golistState> _addr_state, @string verb, params @string[] args)
        {
            ptr<bytes.Buffer> _p0 = default!;
            error _p0 = default!;
            args = args.Clone();
            ref golistState state = ref _addr_state.val;

            var cfg = state.cfg;

            gocommand.Invocation inv = new gocommand.Invocation(Verb:verb,Args:args,BuildFlags:cfg.BuildFlags,Env:cfg.Env,Logf:cfg.Logf,WorkingDir:cfg.Dir,);
            var gocmdRunner = cfg.gocmdRunner;
            if (gocmdRunner == null)
            {
                gocmdRunner = addr(new gocommand.Runner());
            }

            var (stdout, stderr, _, err) = gocmdRunner.RunRaw(cfg.Context, inv);
            if (err != null)
            { 
                // Check for 'go' executable not being found.
                {
                    ptr<exec.Error> (ee, ok) = err._<ptr<exec.Error>>();

                    if (ok && ee.Err == exec.ErrNotFound)
                    {
                        return (_addr_null!, error.As(fmt.Errorf("'go list' driver requires 'go', but %s", exec.ErrNotFound))!);
                    }

                }


                ptr<exec.ExitError> (exitErr, ok) = err._<ptr<exec.ExitError>>();
                if (!ok)
                { 
                    // Catastrophic error:
                    // - context cancellation
                    return (_addr_null!, error.As(xerrors.Errorf("couldn't run 'go': %w", err))!);

                } 

                // Old go version?
                if (strings.Contains(stderr.String(), "flag provided but not defined"))
                {
                    return (_addr_null!, error.As(new goTooOldError(fmt.Errorf("unsupported version of go: %s: %s",exitErr,stderr)))!);
                } 

                // Related to #24854
                if (len(stderr.String()) > 0L && strings.Contains(stderr.String(), "unexpected directory layout"))
                {
                    return (_addr_null!, error.As(fmt.Errorf("%s", stderr.String()))!);
                } 

                // Is there an error running the C compiler in cgo? This will be reported in the "Error" field
                // and should be suppressed by go list -e.
                //
                // This condition is not perfect yet because the error message can include other error messages than runtime/cgo.
                Func<int, bool> isPkgPathRune = r =>
                { 
                    // From https://golang.org/ref/spec#Import_declarations:
                    //    Implementation restriction: A compiler may restrict ImportPaths to non-empty strings
                    //    using only characters belonging to Unicode's L, M, N, P, and S general categories
                    //    (the Graphic characters without spaces) and may also exclude the
                    //    characters !"#$%&'()*,:;<=>?[\]^`{|} and the Unicode replacement character U+FFFD.
                    return _addr_unicode.IsOneOf(new slice<ptr<unicode.RangeTable>>(new ptr<unicode.RangeTable>[] { unicode.L, unicode.M, unicode.N, unicode.P, unicode.S }), r) && !strings.ContainsRune("!\"#$%&'()*,:;<=>?[\\]^`{|}\uFFFD", r)!;

                }
;
                if (len(stderr.String()) > 0L && strings.HasPrefix(stderr.String(), "# "))
                {
                    var msg = stderr.String()[len("# ")..];
                    if (strings.HasPrefix(strings.TrimLeftFunc(msg, isPkgPathRune), "\n"))
                    {
                        return (_addr_stdout!, error.As(null!)!);
                    } 
                    // Treat pkg-config errors as a special case (golang.org/issue/36770).
                    if (strings.HasPrefix(msg, "pkg-config"))
                    {
                        return (_addr_stdout!, error.As(null!)!);
                    }

                } 

                // This error only appears in stderr. See golang.org/cl/166398 for a fix in go list to show
                // the error in the Err section of stdout in case -e option is provided.
                // This fix is provided for backwards compatibility.
                if (len(stderr.String()) > 0L && strings.Contains(stderr.String(), "named files must be .go files"))
                {
                    var output = fmt.Sprintf("{\"ImportPath\": \"command-line-arguments\",\"Incomplete\": true,\"Error\": {\"Pos\": \"\",\"E" +
    "rr\": %q}}", strings.Trim(stderr.String(), "\n"));
                    return (_addr_bytes.NewBufferString(output)!, error.As(null!)!);

                } 

                // Similar to the previous error, but currently lacks a fix in Go.
                if (len(stderr.String()) > 0L && strings.Contains(stderr.String(), "named files must all be in one directory"))
                {
                    output = fmt.Sprintf("{\"ImportPath\": \"command-line-arguments\",\"Incomplete\": true,\"Error\": {\"Pos\": \"\",\"E" +
    "rr\": %q}}", strings.Trim(stderr.String(), "\n"));
                    return (_addr_bytes.NewBufferString(output)!, error.As(null!)!);

                } 

                // Backwards compatibility for Go 1.11 because 1.12 and 1.13 put the directory in the ImportPath.
                // If the package doesn't exist, put the absolute path of the directory into the error message,
                // as Go 1.13 list does.
                const @string noSuchDirectory = (@string)"no such directory";

                if (len(stderr.String()) > 0L && strings.Contains(stderr.String(), noSuchDirectory))
                {
                    var errstr = stderr.String();
                    var abspath = strings.TrimSpace(errstr[strings.Index(errstr, noSuchDirectory) + len(noSuchDirectory)..]);
                    output = fmt.Sprintf("{\"ImportPath\": %q,\"Incomplete\": true,\"Error\": {\"Pos\": \"\",\"Err\": %q}}", abspath, strings.Trim(stderr.String(), "\n"));
                    return (_addr_bytes.NewBufferString(output)!, error.As(null!)!);
                } 

                // Workaround for #29280: go list -e has incorrect behavior when an ad-hoc package doesn't exist.
                // Note that the error message we look for in this case is different that the one looked for above.
                if (len(stderr.String()) > 0L && strings.Contains(stderr.String(), "no such file or directory"))
                {
                    output = fmt.Sprintf("{\"ImportPath\": \"command-line-arguments\",\"Incomplete\": true,\"Error\": {\"Pos\": \"\",\"E" +
    "rr\": %q}}", strings.Trim(stderr.String(), "\n"));
                    return (_addr_bytes.NewBufferString(output)!, error.As(null!)!);

                } 

                // Workaround for #34273. go list -e with GO111MODULE=on has incorrect behavior when listing a
                // directory outside any module.
                if (len(stderr.String()) > 0L && strings.Contains(stderr.String(), "outside available modules"))
                {
                    output = fmt.Sprintf("{\"ImportPath\": %q,\"Incomplete\": true,\"Error\": {\"Pos\": \"\",\"Err\": %q}}", "command-line-arguments", strings.Trim(stderr.String(), "\n"));
                    return (_addr_bytes.NewBufferString(output)!, error.As(null!)!);
                } 

                // Another variation of the previous error
                if (len(stderr.String()) > 0L && strings.Contains(stderr.String(), "outside module root"))
                {
                    output = fmt.Sprintf("{\"ImportPath\": %q,\"Incomplete\": true,\"Error\": {\"Pos\": \"\",\"Err\": %q}}", "command-line-arguments", strings.Trim(stderr.String(), "\n"));
                    return (_addr_bytes.NewBufferString(output)!, error.As(null!)!);
                } 

                // Workaround for an instance of golang.org/issue/26755: go list -e  will return a non-zero exit
                // status if there's a dependency on a package that doesn't exist. But it should return
                // a zero exit status and set an error on that package.
                if (len(stderr.String()) > 0L && strings.Contains(stderr.String(), "no Go files in"))
                { 
                    // Don't clobber stdout if `go list` actually returned something.
                    if (len(stdout.String()) > 0L)
                    {
                        return (_addr_stdout!, error.As(null!)!);
                    } 
                    // try to extract package name from string
                    var stderrStr = stderr.String();
                    @string importPath = default;
                    var colon = strings.Index(stderrStr, ":");
                    if (colon > 0L && strings.HasPrefix(stderrStr, "go build "))
                    {
                        importPath = stderrStr[len("go build ")..colon];
                    }

                    output = fmt.Sprintf("{\"ImportPath\": %q,\"Incomplete\": true,\"Error\": {\"Pos\": \"\",\"Err\": %q}}", importPath, strings.Trim(stderrStr, "\n"));
                    return (_addr_bytes.NewBufferString(output)!, error.As(null!)!);

                } 

                // Export mode entails a build.
                // If that build fails, errors appear on stderr
                // (despite the -e flag) and the Export field is blank.
                // Do not fail in that case.
                // The same is true if an ad-hoc package given to go list doesn't exist.
                // TODO(matloob): Remove these once we can depend on go list to exit with a zero status with -e even when
                // packages don't exist or a build fails.
                if (!usesExportData(cfg) && !containsGoFile(args))
                {
                    return (_addr_null!, error.As(fmt.Errorf("go %v: %s: %s", args, exitErr, stderr))!);
                }

            }

            return (_addr_stdout!, error.As(null!)!);

        }

        private static bool containsGoFile(slice<@string> s)
        {
            foreach (var (_, f) in s)
            {
                if (strings.HasSuffix(f, ".go"))
                {
                    return true;
                }

            }
            return false;

        }

        private static @string cmdDebugStr(ptr<exec.Cmd> _addr_cmd, params @string[] args)
        {
            args = args.Clone();
            ref exec.Cmd cmd = ref _addr_cmd.val;

            var env = make_map<@string, @string>();
            foreach (var (_, kv) in cmd.Env)
            {
                var split = strings.Split(kv, "=");
                var k = split[0L];
                var v = split[1L];
                env[k] = v;

            }
            slice<@string> quotedArgs = default;
            foreach (var (_, arg) in args)
            {
                quotedArgs = append(quotedArgs, strconv.Quote(arg));
            }
            return fmt.Sprintf("GOROOT=%v GOPATH=%v GO111MODULE=%v PWD=%v go %s", env["GOROOT"], env["GOPATH"], env["GO111MODULE"], env["PWD"], strings.Join(quotedArgs, " "));

        }
    }
}}}}}
