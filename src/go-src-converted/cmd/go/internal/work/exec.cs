// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Action graph execution.

// package work -- go2cs converted at 2020 October 09 05:46:17 UTC
// import "cmd/go/internal/work" ==> using work = go.cmd.go.@internal.work_package
// Original source: C:\Go\src\cmd\go\internal\work\exec.go
using bytes = go.bytes_package;
using json = go.encoding.json_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using lazyregexp = go.@internal.lazyregexp_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using log = go.log_package;
using rand = go.math.rand_package;
using os = go.os_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
using regexp = go.regexp_package;
using runtime = go.runtime_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;

using @base = go.cmd.go.@internal.@base_package;
using cache = go.cmd.go.@internal.cache_package;
using cfg = go.cmd.go.@internal.cfg_package;
using load = go.cmd.go.@internal.load_package;
using str = go.cmd.go.@internal.str_package;
using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class work_package
    {
        // actionList returns the list of actions in the dag rooted at root
        // as visited in a depth-first post-order traversal.
        private static slice<ptr<Action>> actionList(ptr<Action> _addr_root)
        {
            ref Action root = ref _addr_root.val;

            map seen = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<Action>, bool>{};
            ptr<Action> all = new slice<ptr<Action>>(new ptr<Action>[] {  });
            Action<ptr<Action>> walk = default;
            walk = a =>
            {
                if (seen[a])
                {
                    return ;
                }
                seen[a] = true;
                foreach (var (_, a1) in a.Deps)
                {
                    walk(a1);
                }                all = append(all, a);

            };
            walk(root);
            return all;

        }

        // do runs the action graph rooted at root.
        private static void Do(this ptr<Builder> _addr_b, ptr<Action> _addr_root) => func((defer, _, __) =>
        {
            ref Builder b = ref _addr_b.val;
            ref Action root = ref _addr_root.val;

            if (!b.IsCmdList)
            { 
                // If we're doing real work, take time at the end to trim the cache.
                var c = cache.Default();
                defer(c.Trim());

            } 

            // Build list of all actions, assigning depth-first post-order priority.
            // The original implementation here was a true queue
            // (using a channel) but it had the effect of getting
            // distracted by low-level leaf actions to the detriment
            // of completing higher-level actions. The order of
            // work does not matter much to overall execution time,
            // but when running "go test std" it is nice to see each test
            // results as soon as possible. The priorities assigned
            // ensure that, all else being equal, the execution prefers
            // to do what it would have done first in a simple depth-first
            // dependency order traversal.
            var all = actionList(_addr_root);
            {
                var i__prev1 = i;
                var a__prev1 = a;

                foreach (var (__i, __a) in all)
                {
                    i = __i;
                    a = __a;
                    a.priority = i;
                } 

                // Write action graph, without timing information, in case we fail and exit early.

                i = i__prev1;
                a = a__prev1;
            }

            Action writeActionGraph = () =>
            {
                {
                    var file = cfg.DebugActiongraph;

                    if (file != "")
                    {
                        if (strings.HasSuffix(file, ".go"))
                        { 
                            // Do not overwrite Go source code in:
                            //    go build -debug-actiongraph x.go
                            @base.Fatalf("go: refusing to write action graph to %v\n", file);

                        }

                        var js = actionGraphJSON(root);
                        {
                            var err__prev2 = err;

                            var err = ioutil.WriteFile(file, (slice<byte>)js, 0666L);

                            if (err != null)
                            {
                                fmt.Fprintf(os.Stderr, "go: writing action graph: %v\n", err);
                                @base.SetExitStatus(1L);
                            }

                            err = err__prev2;

                        }

                    }

                }

            }
;
            writeActionGraph();

            b.readySema = make_channel<bool>(len(all)); 

            // Initialize per-action execution state.
            {
                var a__prev1 = a;

                foreach (var (_, __a) in all)
                {
                    a = __a;
                    foreach (var (_, a1) in a.Deps)
                    {
                        a1.triggers = append(a1.triggers, a);
                    }
                    a.pending = len(a.Deps);
                    if (a.pending == 0L)
                    {
                        b.ready.push(a);
                        b.readySema.Send(true);
                    }

                } 

                // Handle runs a single action and takes care of triggering
                // any actions that are runnable as a result.

                a = a__prev1;
            }

            Action<ptr<Action>> handle = a =>
            {
                if (a.json != null)
                {
                    a.json.TimeStart = time.Now();
                }

                err = default!;
                if (a.Func != null && (!a.Failed || a.IgnoreFail))
                {
                    err = a.Func(b, a);
                }

                if (a.json != null)
                {
                    a.json.TimeDone = time.Now();
                } 

                // The actions run in parallel but all the updates to the
                // shared work state are serialized through b.exec.
                b.exec.Lock();
                defer(b.exec.Unlock());

                if (err != null)
                {
                    if (err == errPrintedOutput)
                    {
                        @base.SetExitStatus(2L);
                    }
                    else
                    {
                        @base.Errorf("%s", err);
                    }

                    a.Failed = true;

                }

                foreach (var (_, a0) in a.triggers)
                {
                    if (a.Failed)
                    {
                        a0.Failed = true;
                    }

                    a0.pending--;

                    if (a0.pending == 0L)
                    {
                        b.ready.push(a0);
                        b.readySema.Send(true);
                    }

                }
                if (a == root)
                {
                    close(b.readySema);
                }

            }
;

            sync.WaitGroup wg = default; 

            // Kick off goroutines according to parallelism.
            // If we are using the -n flag (just printing commands)
            // drop the parallelism to 1, both to make the output
            // deterministic and because there is no real work anyway.
            var par = cfg.BuildP;
            if (cfg.BuildN)
            {
                par = 1L;
            }

            {
                var i__prev1 = i;

                for (long i = 0L; i < par; i++)
                {
                    wg.Add(1L);
                    go_(() => () =>
                    {
                        defer(wg.Done());
                        while (true)
                        {
                            if (!ok)
                            {
                                return ;
                            } 
                            // Receiving a value from b.readySema entitles
                            // us to take from the ready queue.
                            b.exec.Lock();
                            var a = b.ready.pop();
                            b.exec.Unlock();
                            handle(a);
                            @base.SetExitStatus(1L);
                            return ;

                        }


                    }());

                }


                i = i__prev1;
            }

            wg.Wait(); 

            // Write action graph again, this time with timing information.
            writeActionGraph();

        });

        // buildActionID computes the action ID for a build action.
        private static cache.ActionID buildActionID(this ptr<Builder> _addr_b, ptr<Action> _addr_a)
        {
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;

            var p = a.Package;
            var h = cache.NewHash("build " + p.ImportPath); 

            // Configuration independent of compiler toolchain.
            // Note: buildmode has already been accounted for in buildGcflags
            // and should not be inserted explicitly. Most buildmodes use the
            // same compiler settings and can reuse each other's results.
            // If not, the reason is already recorded in buildGcflags.
            fmt.Fprintf(h, "compile\n"); 
            // Only include the package directory if it may affect the output.
            // We trim workspace paths for all packages when -trimpath is set.
            // The compiler hides the exact value of $GOROOT
            // when building things in GOROOT.
            // Assume b.WorkDir is being trimmed properly.
            // When -trimpath is used with a package built from the module cache,
            // use the module path and version instead of the directory.
            if (!p.Goroot && !cfg.BuildTrimpath && !strings.HasPrefix(p.Dir, b.WorkDir))
            {
                fmt.Fprintf(h, "dir %s\n", p.Dir);
            }
            else if (cfg.BuildTrimpath && p.Module != null)
            {
                fmt.Fprintf(h, "module %s@%s\n", p.Module.Path, p.Module.Version);
            }

            if (p.Module != null)
            {
                fmt.Fprintf(h, "go %s\n", p.Module.GoVersion);
            }

            fmt.Fprintf(h, "goos %s goarch %s\n", cfg.Goos, cfg.Goarch);
            fmt.Fprintf(h, "import %q\n", p.ImportPath);
            fmt.Fprintf(h, "omitdebug %v standard %v local %v prefix %q\n", p.Internal.OmitDebug, p.Standard, p.Internal.Local, p.Internal.LocalPrefix);
            if (cfg.BuildTrimpath)
            {
                fmt.Fprintln(h, "trimpath");
            }

            if (p.Internal.ForceLibrary)
            {
                fmt.Fprintf(h, "forcelibrary\n");
            }

            if (len(p.CgoFiles) + len(p.SwigFiles) > 0L)
            {
                fmt.Fprintf(h, "cgo %q\n", b.toolID("cgo"));
                var (cppflags, cflags, cxxflags, fflags, ldflags, _) = b.CFlags(p);
                fmt.Fprintf(h, "CC=%q %q %q %q\n", b.ccExe(), cppflags, cflags, ldflags);
                if (len(p.CXXFiles) + len(p.SwigFiles) > 0L)
                {
                    fmt.Fprintf(h, "CXX=%q %q\n", b.cxxExe(), cxxflags);
                }

                if (len(p.FFiles) > 0L)
                {
                    fmt.Fprintf(h, "FC=%q %q\n", b.fcExe(), fflags);
                } 
                // TODO(rsc): Should we include the SWIG version or Fortran/GCC/G++/Objective-C compiler versions?
            }

            if (p.Internal.CoverMode != "")
            {
                fmt.Fprintf(h, "cover %q %q\n", p.Internal.CoverMode, b.toolID("cover"));
            }

            fmt.Fprintf(h, "modinfo %q\n", p.Internal.BuildInfo); 

            // Configuration specific to compiler toolchain.
            switch (cfg.BuildToolchainName)
            {
                case "gc": 
                    fmt.Fprintf(h, "compile %s %q %q\n", b.toolID("compile"), forcedGcflags, p.Internal.Gcflags);
                    if (len(p.SFiles) > 0L)
                    {
                        fmt.Fprintf(h, "asm %q %q %q\n", b.toolID("asm"), forcedAsmflags, p.Internal.Asmflags);
                    } 

                    // GO386, GOARM, GOMIPS, etc.
                    var (key, val) = cfg.GetArchEnv();
                    fmt.Fprintf(h, "%s=%s\n", key, val); 

                    // TODO(rsc): Convince compiler team not to add more magic environment variables,
                    // or perhaps restrict the environment variables passed to subprocesses.
                    // Because these are clumsy, undocumented special-case hacks
                    // for debugging the compiler, they are not settable using 'go env -w',
                    // and so here we use os.Getenv, not cfg.Getenv.
                    @string magic = new slice<@string>(new @string[] { "GOCLOBBERDEADHASH", "GOSSAFUNC", "GO_SSA_PHI_LOC_CUTOFF", "GOSSAHASH" });
                    {
                        var env__prev1 = env;

                        foreach (var (_, __env) in magic)
                        {
                            env = __env;
                            {
                                var x__prev1 = x;

                                var x = os.Getenv(env);

                                if (x != "")
                                {
                                    fmt.Fprintf(h, "magic %s=%s\n", env, x);
                                }

                                x = x__prev1;

                            }

                        }

                        env = env__prev1;
                    }

                    if (os.Getenv("GOSSAHASH") != "")
                    {
                        for (long i = 0L; inputFiles; i++)
                        {
                            var env = fmt.Sprintf("GOSSAHASH%d", i);
                            x = os.Getenv(env);
                            if (x == "")
                            {
                                break;
                            }

                            fmt.Fprintf(h, "magic %s=%s\n", env, x);

                        }


                    }

                    if (os.Getenv("GSHS_LOGFILE") != "")
                    { 
                        // Clumsy hack. Compiler writes to this log file,
                        // so do not allow use of cache at all.
                        // We will still write to the cache but it will be
                        // essentially unfindable.
                        fmt.Fprintf(h, "nocache %d\n", time.Now().UnixNano());

                    }

                    break;
                case "gccgo": 
                    var (id, err) = b.gccgoToolID(BuildToolchain.compiler(), "go");
                    if (err != null)
                    {
                        @base.Fatalf("%v", err);
                    }

                    fmt.Fprintf(h, "compile %s %q %q\n", id, forcedGccgoflags, p.Internal.Gccgoflags);
                    fmt.Fprintf(h, "pkgpath %s\n", gccgoPkgpath(p));
                    fmt.Fprintf(h, "ar %q\n", BuildToolchain._<gccgoToolchain>().ar());
                    if (len(p.SFiles) > 0L)
                    {
                        id, _ = b.gccgoToolID(BuildToolchain.compiler(), "assembler-with-cpp"); 
                        // Ignore error; different assembler versions
                        // are unlikely to make any difference anyhow.
                        fmt.Fprintf(h, "asm %q\n", id);

                    }

                    break;
                default: 
                    @base.Fatalf("buildActionID: unknown build toolchain %q", cfg.BuildToolchainName);
                    break;
            } 

            // Input files.
            var inputFiles = str.StringList(p.GoFiles, p.CgoFiles, p.CFiles, p.CXXFiles, p.FFiles, p.MFiles, p.HFiles, p.SFiles, p.SysoFiles, p.SwigFiles, p.SwigCXXFiles);
            foreach (var (_, file) in inputFiles)
            {
                fmt.Fprintf(h, "file %s %s\n", file, b.fileHash(filepath.Join(p.Dir, file)));
            }
            foreach (var (_, a1) in a.Deps)
            {
                var p1 = a1.Package;
                if (p1 != null)
                {
                    fmt.Fprintf(h, "import %s %s\n", p1.ImportPath, contentID(a1.buildID));
                }

            }
            return h.Sum();

        }

        // needCgoHdr reports whether the actions triggered by this one
        // expect to be able to access the cgo-generated header file.
        private static bool needCgoHdr(this ptr<Builder> _addr_b, ptr<Action> _addr_a)
        {
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;
 
            // If this build triggers a header install, run cgo to get the header.
            if (!b.IsCmdList && (a.Package.UsesCgo() || a.Package.UsesSwig()) && (cfg.BuildBuildmode == "c-archive" || cfg.BuildBuildmode == "c-shared"))
            {
                {
                    var t1__prev1 = t1;

                    foreach (var (_, __t1) in a.triggers)
                    {
                        t1 = __t1;
                        if (t1.Mode == "install header")
                        {
                            return true;
                        }

                    }

                    t1 = t1__prev1;
                }

                {
                    var t1__prev1 = t1;

                    foreach (var (_, __t1) in a.triggers)
                    {
                        t1 = __t1;
                        foreach (var (_, t2) in t1.triggers)
                        {
                            if (t2.Mode == "install header")
                            {
                                return true;
                            }

                        }

                    }

                    t1 = t1__prev1;
                }
            }

            return false;

        }

        // allowedVersion reports whether the version v is an allowed version of go
        // (one that we can compile).
        // v is known to be of the form "1.23".
        private static bool allowedVersion(@string v)
        { 
            // Special case: no requirement.
            if (v == "")
            {
                return true;
            } 
            // Special case "1.0" means "go1", which is OK.
            if (v == "1.0")
            {
                return true;
            } 
            // Otherwise look through release tags of form "go1.23" for one that matches.
            foreach (var (_, tag) in cfg.BuildContext.ReleaseTags)
            {
                if (strings.HasPrefix(tag, "go") && tag[2L..] == v)
                {
                    return true;
                }

            }
            return false;

        }

        private static readonly uint needBuild = (uint)1L << (int)(iota);
        private static readonly var needCgoHdr = 0;
        private static readonly var needVet = 1;
        private static readonly var needCompiledGoFiles = 2;
        private static readonly var needStale = 3;


        // build is the action for building a single package.
        // Note that any new influence on this logic must be reported in b.buildActionID above as well.
        private static error build(this ptr<Builder> _addr_b, ptr<Action> _addr_a) => func((defer, _, __) =>
        {
            error err = default!;
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;

            var p = a.Package;

            Func<uint, bool, uint> bit = (x, b) =>
            {
                if (b)
                {
                    return error.As(x)!;
                }

                return error.As(0L)!;

            }
;

            var cachedBuild = false;
            var need = bit(needBuild, !b.IsCmdList && a.needBuild || b.NeedExport) | bit(needCgoHdr, b.needCgoHdr(a)) | bit(needVet, a.needVet) | bit(needCompiledGoFiles, b.NeedCompiledGoFiles);

            if (!p.BinaryOnly)
            {
                if (b.useCache(a, b.buildActionID(a), p.Target))
                { 
                    // We found the main output in the cache.
                    // If we don't need any other outputs, we can stop.
                    // Otherwise, we need to write files to a.Objdir (needVet, needCgoHdr).
                    // Remember that we might have them in cache
                    // and check again after we create a.Objdir.
                    cachedBuild = true;
                    a.output = new slice<byte>(new byte[] {  }); // start saving output in case we miss any cache results
                    need &= needBuild;
                    if (b.NeedExport)
                    {
                        p.Export = a.built;
                    }

                    if (need & needCompiledGoFiles != 0L)
                    {
                        {
                            var err__prev4 = err;

                            var err = b.loadCachedSrcFiles(a);

                            if (err == null)
                            {
                                need &= needCompiledGoFiles;
                            }

                            err = err__prev4;

                        }

                    }

                } 

                // Source files might be cached, even if the full action is not
                // (e.g., go list -compiled -find).
                if (!cachedBuild && need & needCompiledGoFiles != 0L)
                {
                    {
                        var err__prev3 = err;

                        err = b.loadCachedSrcFiles(a);

                        if (err == null)
                        {
                            need &= needCompiledGoFiles;
                        }

                        err = err__prev3;

                    }

                }

                if (need == 0L)
                {
                    return error.As(null!)!;
                }

                defer(b.flushOutput(a));

            }

            defer(() =>
            {
                if (err != null && err != errPrintedOutput)
                {
                    err = fmt.Errorf("go build %s: %v", a.Package.ImportPath, err);
                }

                if (err != null && b.IsCmdList && b.NeedError && p.Error == null)
                {
                    p.Error = addr(new load.PackageError(Err:err));
                }

            }());
            if (cfg.BuildN)
            { 
                // In -n mode, print a banner between packages.
                // The banner is five lines so that when changes to
                // different sections of the bootstrap script have to
                // be merged, the banners give patch something
                // to use to find its context.
                b.Print("\n#\n# " + a.Package.ImportPath + "\n#\n\n");

            }

            if (cfg.BuildV)
            {
                b.Print(a.Package.ImportPath + "\n");
            }

            if (a.Package.BinaryOnly)
            {
                p.Stale = true;
                p.StaleReason = "binary-only packages are no longer supported";
                if (b.IsCmdList)
                {
                    return error.As(null!)!;
                }

                return error.As(errors.New("binary-only packages are no longer supported"))!;

            }

            {
                var err__prev1 = err;

                err = b.Mkdir(a.Objdir);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            var objdir = a.Objdir; 

            // Load cached cgo header, but only if we're skipping the main build (cachedBuild==true).
            if (cachedBuild && need & needCgoHdr != 0L)
            {
                {
                    var err__prev2 = err;

                    err = b.loadCachedCgoHdr(a);

                    if (err == null)
                    {
                        need &= needCgoHdr;
                    }

                    err = err__prev2;

                }

            } 

            // Load cached vet config, but only if that's all we have left
            // (need == needVet, not testing just the one bit).
            // If we are going to do a full build anyway,
            // we're going to regenerate the files below anyway.
            if (need == needVet)
            {
                {
                    var err__prev2 = err;

                    err = b.loadCachedVet(a);

                    if (err == null)
                    {
                        need &= needVet;
                    }

                    err = err__prev2;

                }

            }

            if (need == 0L)
            {
                return error.As(null!)!;
            }

            {
                var err__prev1 = err;

                err = allowInstall(a);

                if (err != null)
                {
                    return error.As(err)!;
                } 

                // make target directory

                err = err__prev1;

            } 

            // make target directory
            var (dir, _) = filepath.Split(a.Target);
            if (dir != "")
            {
                {
                    var err__prev2 = err;

                    err = b.Mkdir(dir);

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }

            }

            var gofiles = str.StringList(a.Package.GoFiles);
            var cgofiles = str.StringList(a.Package.CgoFiles);
            var cfiles = str.StringList(a.Package.CFiles);
            var sfiles = str.StringList(a.Package.SFiles);
            var cxxfiles = str.StringList(a.Package.CXXFiles);
            slice<@string> objects = default;            slice<@string> cgoObjects = default;            slice<@string> pcCFLAGS = default;            slice<@string> pcLDFLAGS = default;



            if (a.Package.UsesCgo() || a.Package.UsesSwig())
            {
                pcCFLAGS, pcLDFLAGS, err = b.getPkgConfigFlags(a.Package);

                if (err != null)
                {
                    return ;
                }

            } 

            // Run SWIG on each .swig and .swigcxx file.
            // Each run will generate two files, a .go file and a .c or .cxx file.
            // The .go file will use import "C" and is to be processed by cgo.
            if (a.Package.UsesSwig())
            {
                var (outGo, outC, outCXX, err) = b.swig(a, a.Package, objdir, pcCFLAGS);
                if (err != null)
                {
                    return error.As(err)!;
                }

                cgofiles = append(cgofiles, outGo);
                cfiles = append(cfiles, outC);
                cxxfiles = append(cxxfiles, outCXX);

            } 

            // If we're doing coverage, preprocess the .go files and put them in the work directory
            if (a.Package.Internal.CoverMode != "")
            {
                {
                    var i__prev1 = i;
                    var file__prev1 = file;

                    foreach (var (__i, __file) in str.StringList(gofiles, cgofiles))
                    {
                        i = __i;
                        file = __file;
                        @string sourceFile = default;
                        @string coverFile = default;
                        @string key = default;
                        if (strings.HasSuffix(file, ".cgo1.go"))
                        { 
                            // cgo files have absolute paths
                            var @base = filepath.Base(file);
                            sourceFile = file;
                            coverFile = objdir + base;
                            key = strings.TrimSuffix(base, ".cgo1.go") + ".go";

                        }
                        else
                        {
                            sourceFile = filepath.Join(a.Package.Dir, file);
                            coverFile = objdir + file;
                            key = file;
                        }

                        coverFile = strings.TrimSuffix(coverFile, ".go") + ".cover.go";
                        var cover = a.Package.Internal.CoverVars[key];
                        if (cover == null || @base.IsTestFile(file))
                        { 
                            // Not covering this file.
                            continue;

                        }

                        {
                            var err__prev2 = err;

                            err = b.cover(a, coverFile, sourceFile, cover.Var);

                            if (err != null)
                            {
                                return error.As(err)!;
                            }

                            err = err__prev2;

                        }

                        if (i < len(gofiles))
                        {
                            gofiles[i] = coverFile;
                        }
                        else
                        {
                            cgofiles[i - len(gofiles)] = coverFile;
                        }

                    }

                    i = i__prev1;
                    file = file__prev1;
                }
            } 

            // Run cgo.
            if (a.Package.UsesCgo() || a.Package.UsesSwig())
            { 
                // In a package using cgo, cgo compiles the C, C++ and assembly files with gcc.
                // There is one exception: runtime/cgo's job is to bridge the
                // cgo and non-cgo worlds, so it necessarily has files in both.
                // In that case gcc only gets the gcc_* files.
                slice<@string> gccfiles = default;
                gccfiles = append(gccfiles, cfiles);
                cfiles = null;
                if (a.Package.Standard && a.Package.ImportPath == "runtime/cgo")
                {
                    Func<slice<@string>, slice<@string>, slice<@string>, (slice<@string>, slice<@string>)> filter = (files, nongcc, gcc) =>
                    {
                        foreach (var (_, f) in files)
                        {
                            if (strings.HasPrefix(f, "gcc_"))
                            {
                                gcc = append(gcc, f);
                            }
                            else
                            {
                                nongcc = append(nongcc, f);
                            }

                        }
                else
                        return (error.As(nongcc)!, gcc);

                    }
;
                    sfiles, gccfiles = filter(sfiles, sfiles[..0L], gccfiles);

                }                {
                    foreach (var (_, sfile) in sfiles)
                    {
                        var (data, err) = ioutil.ReadFile(filepath.Join(a.Package.Dir, sfile));
                        if (err == null)
                        {
                            if (bytes.HasPrefix(data, (slice<byte>)"TEXT") || bytes.Contains(data, (slice<byte>)"\nTEXT") || bytes.HasPrefix(data, (slice<byte>)"DATA") || bytes.Contains(data, (slice<byte>)"\nDATA") || bytes.HasPrefix(data, (slice<byte>)"GLOBL") || bytes.Contains(data, (slice<byte>)"\nGLOBL"))
                            {
                                return error.As(fmt.Errorf("package using cgo has Go assembly file %s", sfile))!;
                            }

                        }

                    }
                    gccfiles = append(gccfiles, sfiles);
                    sfiles = null;

                }

                var (outGo, outObj, err) = b.cgo(a, @base.Tool("cgo"), objdir, pcCFLAGS, pcLDFLAGS, mkAbsFiles(a.Package.Dir, cgofiles), gccfiles, cxxfiles, a.Package.MFiles, a.Package.FFiles);
                if (err != null)
                {
                    return error.As(err)!;
                }

                if (cfg.BuildToolchainName == "gccgo")
                {
                    cgoObjects = append(cgoObjects, a.Objdir + "_cgo_flags");
                }

                cgoObjects = append(cgoObjects, outObj);
                gofiles = append(gofiles, outGo);

                switch (cfg.BuildBuildmode)
                {
                    case "c-archive": 

                    case "c-shared": 
                        b.cacheCgoHdr(a);
                        break;
                }

            }

            slice<@string> srcfiles = default; // .go and non-.go
            srcfiles = append(srcfiles, gofiles);
            srcfiles = append(srcfiles, sfiles);
            srcfiles = append(srcfiles, cfiles);
            srcfiles = append(srcfiles, cxxfiles);
            b.cacheSrcFiles(a, srcfiles); 

            // Running cgo generated the cgo header.
            need &= needCgoHdr; 

            // Sanity check only, since Package.load already checked as well.
            if (len(gofiles) == 0L)
            {
                return error.As(addr(new load.NoGoError(Package:a.Package))!)!;
            } 

            // Prepare Go vet config if needed.
            if (need & needVet != 0L)
            {
                buildVetConfig(_addr_a, srcfiles);
                need &= needVet;
            }

            if (need & needCompiledGoFiles != 0L)
            {
                {
                    var err__prev2 = err;

                    err = b.loadCachedSrcFiles(a);

                    if (err != null)
                    {
                        return error.As(fmt.Errorf("loading compiled Go files from cache: %w", err))!;
                    }

                    err = err__prev2;

                }

                need &= needCompiledGoFiles;

            }

            if (need == 0L)
            { 
                // Nothing left to do.
                return error.As(null!)!;

            } 

            // Collect symbol ABI requirements from assembly.
            var (symabis, err) = BuildToolchain.symabis(b, a, sfiles);
            if (err != null)
            {
                return error.As(err)!;
            } 

            // Prepare Go import config.
            // We start it off with a comment so it can't be empty, so icfg.Bytes() below is never nil.
            // It should never be empty anyway, but there have been bugs in the past that resulted
            // in empty configs, which then unfortunately turn into "no config passed to compiler",
            // and the compiler falls back to looking in pkg itself, which mostly works,
            // except when it doesn't.
            ref bytes.Buffer icfg = ref heap(out ptr<bytes.Buffer> _addr_icfg);
            fmt.Fprintf(_addr_icfg, "# import config\n");
            {
                var i__prev1 = i;

                foreach (var (__i, __raw) in a.Package.Internal.RawImports)
                {
                    i = __i;
                    raw = __raw;
                    var final = a.Package.Imports[i];
                    if (final != raw)
                    {
                        fmt.Fprintf(_addr_icfg, "importmap %s=%s\n", raw, final);
                    }

                }

                i = i__prev1;
            }

            foreach (var (_, a1) in a.Deps)
            {
                var p1 = a1.Package;
                if (p1 == null || p1.ImportPath == "" || a1.built == "")
                {
                    continue;
                }

                fmt.Fprintf(_addr_icfg, "packagefile %s=%s\n", p1.ImportPath, a1.built);

            }
            if (p.Internal.BuildInfo != "" && cfg.ModulesEnabled)
            {
                {
                    var err__prev2 = err;

                    err = b.writeFile(objdir + "_gomod_.go", load.ModInfoProg(p.Internal.BuildInfo, cfg.BuildToolchainName == "gccgo"));

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }

                gofiles = append(gofiles, objdir + "_gomod_.go");

            } 

            // Compile Go.
            var objpkg = objdir + "_pkg_.a";
            var (ofile, out, err) = BuildToolchain.gc(b, a, objpkg, icfg.Bytes(), symabis, len(sfiles) > 0L, gofiles);
            if (len(out) > 0L)
            {
                var output = b.processOutput(out);
                if (p.Module != null && !allowedVersion(p.Module.GoVersion))
                {
                    output += "note: module requires Go " + p.Module.GoVersion + "\n";
                }

                b.showOutput(a, a.Package.Dir, a.Package.Desc(), output);
                if (err != null)
                {
                    return error.As(errPrintedOutput)!;
                }

            }

            if (err != null)
            {
                if (p.Module != null && !allowedVersion(p.Module.GoVersion))
                {
                    b.showOutput(a, a.Package.Dir, a.Package.Desc(), "note: module requires Go " + p.Module.GoVersion);
                }

                return error.As(err)!;

            }

            if (ofile != objpkg)
            {
                objects = append(objects, ofile);
            } 

            // Copy .h files named for goos or goarch or goos_goarch
            // to names using GOOS and GOARCH.
            // For example, defs_linux_amd64.h becomes defs_GOOS_GOARCH.h.
            @string _goos_goarch = "_" + cfg.Goos + "_" + cfg.Goarch;
            @string _goos = "_" + cfg.Goos;
            @string _goarch = "_" + cfg.Goarch;
            {
                var file__prev1 = file;

                foreach (var (_, __file) in a.Package.HFiles)
                {
                    file = __file;
                    var (name, ext) = fileExtSplit(file);

                    if (strings.HasSuffix(name, _goos_goarch)) 
                        var targ = file[..len(name) - len(_goos_goarch)] + "_GOOS_GOARCH." + ext;
                        {
                            var err__prev1 = err;

                            err = b.copyFile(objdir + targ, filepath.Join(a.Package.Dir, file), 0666L, true);

                            if (err != null)
                            {
                                return error.As(err)!;
                            }

                            err = err__prev1;

                        }

                    else if (strings.HasSuffix(name, _goarch)) 
                        targ = file[..len(name) - len(_goarch)] + "_GOARCH." + ext;
                        {
                            var err__prev1 = err;

                            err = b.copyFile(objdir + targ, filepath.Join(a.Package.Dir, file), 0666L, true);

                            if (err != null)
                            {
                                return error.As(err)!;
                            }

                            err = err__prev1;

                        }

                    else if (strings.HasSuffix(name, _goos)) 
                        targ = file[..len(name) - len(_goos)] + "_GOOS." + ext;
                        {
                            var err__prev1 = err;

                            err = b.copyFile(objdir + targ, filepath.Join(a.Package.Dir, file), 0666L, true);

                            if (err != null)
                            {
                                return error.As(err)!;
                            }

                            err = err__prev1;

                        }

                                    }

                file = file__prev1;
            }

            {
                var file__prev1 = file;

                foreach (var (_, __file) in cfiles)
                {
                    file = __file;
                    var @out = file[..len(file) - len(".c")] + ".o";
                    {
                        var err__prev1 = err;

                        err = BuildToolchain.cc(b, a, objdir + out, file);

                        if (err != null)
                        {
                            return error.As(err)!;
                        }

                        err = err__prev1;

                    }

                    objects = append(objects, out);

                } 

                // Assemble .s files.

                file = file__prev1;
            }

            if (len(sfiles) > 0L)
            {
                var (ofiles, err) = BuildToolchain.asm(b, a, sfiles);
                if (err != null)
                {
                    return error.As(err)!;
                }

                objects = append(objects, ofiles);

            } 

            // For gccgo on ELF systems, we write the build ID as an assembler file.
            // This lets us set the SHF_EXCLUDE flag.
            // This is read by readGccgoArchive in cmd/internal/buildid/buildid.go.
            if (a.buildID != "" && cfg.BuildToolchainName == "gccgo")
            {
                switch (cfg.Goos)
                {
                    case "aix": 

                    case "android": 

                    case "dragonfly": 

                    case "freebsd": 

                    case "illumos": 

                    case "linux": 

                    case "netbsd": 

                    case "openbsd": 

                    case "solaris": 
                        var (asmfile, err) = b.gccgoBuildIDFile(a);
                        if (err != null)
                        {
                            return error.As(err)!;
                        }

                        (ofiles, err) = BuildToolchain.asm(b, a, new slice<@string>(new @string[] { asmfile }));
                        if (err != null)
                        {
                            return error.As(err)!;
                        }

                        objects = append(objects, ofiles);
                        break;
                }

            } 

            // NOTE(rsc): On Windows, it is critically important that the
            // gcc-compiled objects (cgoObjects) be listed after the ordinary
            // objects in the archive. I do not know why this is.
            // https://golang.org/issue/2601
            objects = append(objects, cgoObjects); 

            // Add system object files.
            foreach (var (_, syso) in a.Package.SysoFiles)
            {
                objects = append(objects, filepath.Join(a.Package.Dir, syso));
            } 

            // Pack into archive in objdir directory.
            // If the Go compiler wrote an archive, we only need to add the
            // object files for non-Go sources to the archive.
            // If the Go compiler wrote an archive and the package is entirely
            // Go sources, there is no pack to execute at all.
            if (len(objects) > 0L)
            {
                {
                    var err__prev2 = err;

                    err = BuildToolchain.pack(b, a, objpkg, objects);

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }

            }

            {
                var err__prev1 = err;

                err = b.updateBuildID(a, objpkg, true);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }


            a.built = objpkg;
            return error.As(null!)!;

        });

        private static error cacheObjdirFile(this ptr<Builder> _addr_b, ptr<Action> _addr_a, ptr<cache.Cache> _addr_c, @string name) => func((defer, _, __) =>
        {
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;
            ref cache.Cache c = ref _addr_c.val;

            var (f, err) = os.Open(a.Objdir + name);
            if (err != null)
            {
                return error.As(err)!;
            }

            defer(f.Close());
            _, _, err = c.Put(cache.Subkey(a.actionID, name), f);
            return error.As(err)!;

        });

        private static (@string, error) findCachedObjdirFile(this ptr<Builder> _addr_b, ptr<Action> _addr_a, ptr<cache.Cache> _addr_c, @string name)
        {
            @string _p0 = default;
            error _p0 = default!;
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;
            ref cache.Cache c = ref _addr_c.val;

            var (file, _, err) = c.GetFile(cache.Subkey(a.actionID, name));
            if (err != null)
            {
                return ("", error.As(fmt.Errorf("loading cached file %s: %w", name, err))!);
            }

            return (file, error.As(null!)!);

        }

        private static error loadCachedObjdirFile(this ptr<Builder> _addr_b, ptr<Action> _addr_a, ptr<cache.Cache> _addr_c, @string name)
        {
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;
            ref cache.Cache c = ref _addr_c.val;

            var (cached, err) = b.findCachedObjdirFile(a, c, name);
            if (err != null)
            {
                return error.As(err)!;
            }

            return error.As(b.copyFile(a.Objdir + name, cached, 0666L, true))!;

        }

        private static void cacheCgoHdr(this ptr<Builder> _addr_b, ptr<Action> _addr_a)
        {
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;

            var c = cache.Default();
            b.cacheObjdirFile(a, c, "_cgo_install.h");
        }

        private static error loadCachedCgoHdr(this ptr<Builder> _addr_b, ptr<Action> _addr_a)
        {
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;

            var c = cache.Default();
            return error.As(b.loadCachedObjdirFile(a, c, "_cgo_install.h"))!;
        }

        private static void cacheSrcFiles(this ptr<Builder> _addr_b, ptr<Action> _addr_a, slice<@string> srcfiles)
        {
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;

            var c = cache.Default();
            bytes.Buffer buf = default;
            foreach (var (_, file) in srcfiles)
            {
                if (!strings.HasPrefix(file, a.Objdir))
                { 
                    // not generated
                    buf.WriteString("./");
                    buf.WriteString(file);
                    buf.WriteString("\n");
                    continue;

                }

                var name = file[len(a.Objdir)..];
                buf.WriteString(name);
                buf.WriteString("\n");
                {
                    var err = b.cacheObjdirFile(a, c, name);

                    if (err != null)
                    {
                        return ;
                    }

                }

            }
            c.PutBytes(cache.Subkey(a.actionID, "srcfiles"), buf.Bytes());

        }

        private static error loadCachedVet(this ptr<Builder> _addr_b, ptr<Action> _addr_a)
        {
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;

            var c = cache.Default();
            var (list, _, err) = c.GetBytes(cache.Subkey(a.actionID, "srcfiles"));
            if (err != null)
            {
                return error.As(fmt.Errorf("reading srcfiles list: %w", err))!;
            }

            slice<@string> srcfiles = default;
            foreach (var (_, name) in strings.Split(string(list), "\n"))
            {
                if (name == "")
                { // end of list
                    continue;

                }

                if (strings.HasPrefix(name, "./"))
                {
                    srcfiles = append(srcfiles, name[2L..]);
                    continue;
                }

                {
                    var err = b.loadCachedObjdirFile(a, c, name);

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                }

                srcfiles = append(srcfiles, a.Objdir + name);

            }
            buildVetConfig(_addr_a, srcfiles);
            return error.As(null!)!;

        }

        private static error loadCachedSrcFiles(this ptr<Builder> _addr_b, ptr<Action> _addr_a)
        {
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;

            var c = cache.Default();
            var (list, _, err) = c.GetBytes(cache.Subkey(a.actionID, "srcfiles"));
            if (err != null)
            {
                return error.As(fmt.Errorf("reading srcfiles list: %w", err))!;
            }

            slice<@string> files = default;
            foreach (var (_, name) in strings.Split(string(list), "\n"))
            {
                if (name == "")
                { // end of list
                    continue;

                }

                if (strings.HasPrefix(name, "./"))
                {
                    files = append(files, name[len("./")..]);
                    continue;
                }

                var (file, err) = b.findCachedObjdirFile(a, c, name);
                if (err != null)
                {
                    return error.As(fmt.Errorf("finding %s: %w", name, err))!;
                }

                files = append(files, file);

            }
            a.Package.CompiledGoFiles = files;
            return error.As(null!)!;

        }

        // vetConfig is the configuration passed to vet describing a single package.
        private partial struct vetConfig
        {
            public @string ID; // package ID (example: "fmt [fmt.test]")
            public @string Compiler; // compiler name (gc, gccgo)
            public @string Dir; // directory containing package
            public @string ImportPath; // canonical import path ("package path")
            public slice<@string> GoFiles; // absolute paths to package source files
            public slice<@string> NonGoFiles; // absolute paths to package non-Go files

            public map<@string, @string> ImportMap; // map import path in source code to package path
            public map<@string, @string> PackageFile; // map package path to .a file with export data
            public map<@string, bool> Standard; // map package path to whether it's in the standard library
            public map<@string, @string> PackageVetx; // map package path to vetx data from earlier vet run
            public bool VetxOnly; // only compute vetx data; don't report detected problems
            public @string VetxOutput; // write vetx data to this output file

            public bool SucceedOnTypecheckFailure; // awful hack; see #18395 and below
        }

        private static void buildVetConfig(ptr<Action> _addr_a, slice<@string> srcfiles)
        {
            ref Action a = ref _addr_a.val;
 
            // Classify files based on .go extension.
            // srcfiles does not include raw cgo files.
            slice<@string> gofiles = default;            slice<@string> nongofiles = default;

            foreach (var (_, name) in srcfiles)
            {
                if (strings.HasSuffix(name, ".go"))
                {
                    gofiles = append(gofiles, name);
                }
                else
                {
                    nongofiles = append(nongofiles, name);
                }

            } 

            // Pass list of absolute paths to vet,
            // so that vet's error messages will use absolute paths,
            // so that we can reformat them relative to the directory
            // in which the go command is invoked.
            ptr<vetConfig> vcfg = addr(new vetConfig(ID:a.Package.ImportPath,Compiler:cfg.BuildToolchainName,Dir:a.Package.Dir,GoFiles:mkAbsFiles(a.Package.Dir,gofiles),NonGoFiles:mkAbsFiles(a.Package.Dir,nongofiles),ImportPath:a.Package.ImportPath,ImportMap:make(map[string]string),PackageFile:make(map[string]string),Standard:make(map[string]bool),));
            a.vetCfg = vcfg;
            foreach (var (i, raw) in a.Package.Internal.RawImports)
            {
                var final = a.Package.Imports[i];
                vcfg.ImportMap[raw] = final;
            } 

            // Compute the list of mapped imports in the vet config
            // so that we can add any missing mappings below.
            var vcfgMapped = make_map<@string, bool>();
            foreach (var (_, p) in vcfg.ImportMap)
            {
                vcfgMapped[p] = true;
            }
            foreach (var (_, a1) in a.Deps)
            {
                var p1 = a1.Package;
                if (p1 == null || p1.ImportPath == "")
                {
                    continue;
                } 
                // Add import mapping if needed
                // (for imports like "runtime/cgo" that appear only in generated code).
                if (!vcfgMapped[p1.ImportPath])
                {
                    vcfg.ImportMap[p1.ImportPath] = p1.ImportPath;
                }

                if (a1.built != "")
                {
                    vcfg.PackageFile[p1.ImportPath] = a1.built;
                }

                if (p1.Standard)
                {
                    vcfg.Standard[p1.ImportPath] = true;
                }

            }

        }

        // VetTool is the path to an alternate vet tool binary.
        // The caller is expected to set it (if needed) before executing any vet actions.
        public static @string VetTool = default;

        // VetFlags are the default flags to pass to vet.
        // The caller is expected to set them before executing any vet actions.
        public static slice<@string> VetFlags = default;

        // VetExplicit records whether the vet flags were set explicitly on the command line.
        public static bool VetExplicit = default;

        private static error vet(this ptr<Builder> _addr_b, ptr<Action> _addr_a)
        {
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;
 
            // a.Deps[0] is the build of the package being vetted.
            // a.Deps[1] is the build of the "fmt" package.

            a.Failed = false; // vet of dependency may have failed but we can still succeed

            if (a.Deps[0L].Failed)
            { 
                // The build of the package has failed. Skip vet check.
                // Vet could return export data for non-typecheck errors,
                // but we ignore it because the package cannot be compiled.
                return error.As(null!)!;

            }

            var vcfg = a.Deps[0L].vetCfg;
            if (vcfg == null)
            { 
                // Vet config should only be missing if the build failed.
                return error.As(fmt.Errorf("vet config not found"))!;

            }

            vcfg.VetxOnly = a.VetxOnly;
            vcfg.VetxOutput = a.Objdir + "vet.out";
            vcfg.PackageVetx = make_map<@string, @string>();

            var h = cache.NewHash("vet " + a.Package.ImportPath);
            fmt.Fprintf(h, "vet %q\n", b.toolID("vet"));

            var vetFlags = VetFlags; 

            // In GOROOT, we enable all the vet tests during 'go test',
            // not just the high-confidence subset. This gets us extra
            // checking for the standard library (at some compliance cost)
            // and helps us gain experience about how well the checks
            // work, to help decide which should be turned on by default.
            // The command-line still wins.
            //
            // Note that this flag change applies even when running vet as
            // a dependency of vetting a package outside std.
            // (Otherwise we'd have to introduce a whole separate
            // space of "vet fmt as a dependency of a std top-level vet"
            // versus "vet fmt as a dependency of a non-std top-level vet".)
            // This is OK as long as the packages that are farther down the
            // dependency tree turn on *more* analysis, as here.
            // (The unsafeptr check does not write any facts for use by
            // later vet runs.)
            if (a.Package.Goroot && !VetExplicit && VetTool == "")
            { 
                // Note that $GOROOT/src/buildall.bash
                // does the same for the misc-compile trybots
                // and should be updated if these flags are
                // changed here.
                //
                // There's too much unsafe.Pointer code
                // that vet doesn't like in low-level packages
                // like runtime, sync, and reflect.
                vetFlags = new slice<@string>(new @string[] { "-unsafeptr=false" });

            } 

            // Note: We could decide that vet should compute export data for
            // all analyses, in which case we don't need to include the flags here.
            // But that would mean that if an analysis causes problems like
            // unexpected crashes there would be no way to turn it off.
            // It seems better to let the flags disable export analysis too.
            fmt.Fprintf(h, "vetflags %q\n", vetFlags);

            fmt.Fprintf(h, "pkg %q\n", a.Deps[0L].actionID);
            foreach (var (_, a1) in a.Deps)
            {
                if (a1.Mode == "vet" && a1.built != "")
                {
                    fmt.Fprintf(h, "vetout %q %s\n", a1.Package.ImportPath, b.fileHash(a1.built));
                    vcfg.PackageVetx[a1.Package.ImportPath] = a1.built;
                }

            }
            var key = cache.ActionID(h.Sum());

            if (vcfg.VetxOnly && !cfg.BuildA)
            {
                var c = cache.Default();
                {
                    var (file, _, err) = c.GetFile(key);

                    if (err == null)
                    {
                        a.built = file;
                        return error.As(null!)!;
                    }

                }

            }

            var (js, err) = json.MarshalIndent(vcfg, "", "\t");
            if (err != null)
            {
                return error.As(fmt.Errorf("internal error marshaling vet config: %v", err))!;
            }

            js = append(js, '\n');
            {
                var err = b.writeFile(a.Objdir + "vet.cfg", js);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }


            var env = b.cCompilerEnv();
            if (cfg.BuildToolchainName == "gccgo")
            {
                env = append(env, "GCCGO=" + BuildToolchain.compiler());
            }

            var p = a.Package;
            var tool = VetTool;
            if (tool == "")
            {
                tool = @base.Tool("vet");
            }

            var runErr = b.run(a, p.Dir, p.ImportPath, env, cfg.BuildToolexec, tool, vetFlags, a.Objdir + "vet.cfg"); 

            // If vet wrote export data, save it for input to future vets.
            {
                var (f, err) = os.Open(vcfg.VetxOutput);

                if (err == null)
                {
                    a.built = vcfg.VetxOutput;
                    cache.Default().Put(key, f);
                    f.Close();
                }

            }


            return error.As(runErr)!;

        }

        // linkActionID computes the action ID for a link action.
        private static cache.ActionID linkActionID(this ptr<Builder> _addr_b, ptr<Action> _addr_a)
        {
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;

            var p = a.Package;
            var h = cache.NewHash("link " + p.ImportPath); 

            // Toolchain-independent configuration.
            fmt.Fprintf(h, "link\n");
            fmt.Fprintf(h, "buildmode %s goos %s goarch %s\n", cfg.BuildBuildmode, cfg.Goos, cfg.Goarch);
            fmt.Fprintf(h, "import %q\n", p.ImportPath);
            fmt.Fprintf(h, "omitdebug %v standard %v local %v prefix %q\n", p.Internal.OmitDebug, p.Standard, p.Internal.Local, p.Internal.LocalPrefix);
            if (cfg.BuildTrimpath)
            {
                fmt.Fprintln(h, "trimpath");
            } 

            // Toolchain-dependent configuration, shared with b.linkSharedActionID.
            b.printLinkerConfig(h, p); 

            // Input files.
            foreach (var (_, a1) in a.Deps)
            {
                var p1 = a1.Package;
                if (p1 != null)
                {
                    if (a1.built != "" || a1.buildID != "")
                    {
                        var buildID = a1.buildID;
                        if (buildID == "")
                        {
                            buildID = b.buildID(a1.built);
                        }

                        fmt.Fprintf(h, "packagefile %s=%s\n", p1.ImportPath, contentID(buildID));

                    } 
                    // Because we put package main's full action ID into the binary's build ID,
                    // we must also put the full action ID into the binary's action ID hash.
                    if (p1.Name == "main")
                    {
                        fmt.Fprintf(h, "packagemain %s\n", a1.buildID);
                    }

                    if (p1.Shlib != "")
                    {
                        fmt.Fprintf(h, "packageshlib %s=%s\n", p1.ImportPath, contentID(b.buildID(p1.Shlib)));
                    }

                }

            }
            return h.Sum();

        }

        // printLinkerConfig prints the linker config into the hash h,
        // as part of the computation of a linker-related action ID.
        private static void printLinkerConfig(this ptr<Builder> _addr_b, io.Writer h, ptr<load.Package> _addr_p)
        {
            ref Builder b = ref _addr_b.val;
            ref load.Package p = ref _addr_p.val;

            switch (cfg.BuildToolchainName)
            {
                case "gc": 
                    fmt.Fprintf(h, "link %s %q %s\n", b.toolID("link"), forcedLdflags, ldBuildmode);
                    if (p != null)
                    {
                        fmt.Fprintf(h, "linkflags %q\n", p.Internal.Ldflags);
                    } 

                    // GO386, GOARM, GOMIPS, etc.
                    var (key, val) = cfg.GetArchEnv();
                    fmt.Fprintf(h, "%s=%s\n", key, val); 

                    // The linker writes source file paths that say GOROOT_FINAL.
                    fmt.Fprintf(h, "GOROOT=%s\n", cfg.GOROOT_FINAL); 

                    // GO_EXTLINK_ENABLED controls whether the external linker is used.
                    fmt.Fprintf(h, "GO_EXTLINK_ENABLED=%s\n", cfg.Getenv("GO_EXTLINK_ENABLED")); 

                    // TODO(rsc): Do cgo settings and flags need to be included?
                    // Or external linker settings and flags?
                    break;
                case "gccgo": 
                    var (id, err) = b.gccgoToolID(BuildToolchain.linker(), "go");
                    if (err != null)
                    {
                        @base.Fatalf("%v", err);
                    }

                    fmt.Fprintf(h, "link %s %s\n", id, ldBuildmode); 
                    // TODO(iant): Should probably include cgo flags here.
                    break;
                default: 
                    @base.Fatalf("linkActionID: unknown toolchain %q", cfg.BuildToolchainName);
                    break;
            }

        }

        // link is the action for linking a single command.
        // Note that any new influence on this logic must be reported in b.linkActionID above as well.
        private static error link(this ptr<Builder> _addr_b, ptr<Action> _addr_a) => func((defer, _, __) =>
        {
            error err = default!;
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;

            if (b.useCache(a, b.linkActionID(a), a.Package.Target) || b.IsCmdList)
            {
                return error.As(null!)!;
            }

            defer(b.flushOutput(a));

            {
                var err__prev1 = err;

                var err = b.Mkdir(a.Objdir);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }


            var importcfg = a.Objdir + "importcfg.link";
            {
                var err__prev1 = err;

                err = b.writeLinkImportcfg(a, importcfg);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }


            {
                var err__prev1 = err;

                err = allowInstall(a);

                if (err != null)
                {
                    return error.As(err)!;
                } 

                // make target directory

                err = err__prev1;

            } 

            // make target directory
            var (dir, _) = filepath.Split(a.Target);
            if (dir != "")
            {
                {
                    var err__prev2 = err;

                    err = b.Mkdir(dir);

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }

            }

            {
                var err__prev1 = err;

                err = BuildToolchain.ld(b, a, a.Target, importcfg, a.Deps[0L].built);

                if (err != null)
                {
                    return error.As(err)!;
                } 

                // Update the binary with the final build ID.
                // But if OmitDebug is set, don't rewrite the binary, because we set OmitDebug
                // on binaries that we are going to run and then delete.
                // There's no point in doing work on such a binary.
                // Worse, opening the binary for write here makes it
                // essentially impossible to safely fork+exec due to a fundamental
                // incompatibility between ETXTBSY and threads on modern Unix systems.
                // See golang.org/issue/22220.
                // We still call updateBuildID to update a.buildID, which is important
                // for test result caching, but passing rewrite=false (final arg)
                // means we don't actually rewrite the binary, nor store the
                // result into the cache. That's probably a net win:
                // less cache space wasted on large binaries we are not likely to
                // need again. (On the other hand it does make repeated go test slower.)
                // It also makes repeated go run slower, which is a win in itself:
                // we don't want people to treat go run like a scripting environment.

                err = err__prev1;

            } 

            // Update the binary with the final build ID.
            // But if OmitDebug is set, don't rewrite the binary, because we set OmitDebug
            // on binaries that we are going to run and then delete.
            // There's no point in doing work on such a binary.
            // Worse, opening the binary for write here makes it
            // essentially impossible to safely fork+exec due to a fundamental
            // incompatibility between ETXTBSY and threads on modern Unix systems.
            // See golang.org/issue/22220.
            // We still call updateBuildID to update a.buildID, which is important
            // for test result caching, but passing rewrite=false (final arg)
            // means we don't actually rewrite the binary, nor store the
            // result into the cache. That's probably a net win:
            // less cache space wasted on large binaries we are not likely to
            // need again. (On the other hand it does make repeated go test slower.)
            // It also makes repeated go run slower, which is a win in itself:
            // we don't want people to treat go run like a scripting environment.
            {
                var err__prev1 = err;

                err = b.updateBuildID(a, a.Target, !a.Package.Internal.OmitDebug);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }


            a.built = a.Target;
            return error.As(null!)!;

        });

        private static error writeLinkImportcfg(this ptr<Builder> _addr_b, ptr<Action> _addr_a, @string file)
        {
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;
 
            // Prepare Go import cfg.
            ref bytes.Buffer icfg = ref heap(out ptr<bytes.Buffer> _addr_icfg);
            foreach (var (_, a1) in a.Deps)
            {
                var p1 = a1.Package;
                if (p1 == null)
                {
                    continue;
                }

                fmt.Fprintf(_addr_icfg, "packagefile %s=%s\n", p1.ImportPath, a1.built);
                if (p1.Shlib != "")
                {
                    fmt.Fprintf(_addr_icfg, "packageshlib %s=%s\n", p1.ImportPath, p1.Shlib);
                }

            }
            return error.As(b.writeFile(file, icfg.Bytes()))!;

        }

        // PkgconfigCmd returns a pkg-config binary name
        // defaultPkgConfig is defined in zdefaultcc.go, written by cmd/dist.
        private static @string PkgconfigCmd(this ptr<Builder> _addr_b)
        {
            ref Builder b = ref _addr_b.val;

            return envList("PKG_CONFIG", cfg.DefaultPkgConfig)[0L];
        }

        // splitPkgConfigOutput parses the pkg-config output into a slice of
        // flags. This implements the algorithm from pkgconf/libpkgconf/argvsplit.c.
        private static (slice<@string>, error) splitPkgConfigOutput(slice<byte> @out)
        {
            slice<@string> _p0 = default;
            error _p0 = default!;

            if (len(out) == 0L)
            {
                return (null, error.As(null!)!);
            }

            slice<@string> flags = default;
            var flag = make_slice<byte>(0L, len(out));
            var escaped = false;
            var quote = byte(0L);

            foreach (var (_, c) in out)
            {
                if (escaped)
                {
                    if (quote != 0L)
                    {
                        switch (c)
                        {
                            case '$': 

                            case '`': 

                            case '"': 

                            case '\\': 
                                break;
                            default: 
                                flag = append(flag, '\\');
                                break;
                        }
                        flag = append(flag, c);

                    }
                    else
                    {
                        flag = append(flag, c);
                    }

                    escaped = false;

                }
                else if (quote != 0L)
                {
                    if (c == quote)
                    {
                        quote = 0L;
                    }
                    else
                    {
                        switch (c)
                        {
                            case '\\': 
                                escaped = true;
                                break;
                            default: 
                                flag = append(flag, c);
                                break;
                        }

                    }

                }
                else if (strings.IndexByte(" \t\n\v\f\r", c) < 0L)
                {
                    switch (c)
                    {
                        case '\\': 
                            escaped = true;
                            break;
                        case '\'': 

                        case '"': 
                            quote = c;
                            break;
                        default: 
                            flag = append(flag, c);
                            break;
                    }

                }
                else if (len(flag) != 0L)
                {
                    flags = append(flags, string(flag));
                    flag = flag[..0L];
                }

            }
            if (escaped)
            {
                return (null, error.As(errors.New("broken character escaping in pkgconf output "))!);
            }

            if (quote != 0L)
            {
                return (null, error.As(errors.New("unterminated quoted string in pkgconf output "))!);
            }
            else if (len(flag) != 0L)
            {
                flags = append(flags, string(flag));
            }

            return (flags, error.As(null!)!);

        }

        // Calls pkg-config if needed and returns the cflags/ldflags needed to build the package.
        private static (slice<@string>, slice<@string>, error) getPkgConfigFlags(this ptr<Builder> _addr_b, ptr<load.Package> _addr_p)
        {
            slice<@string> cflags = default;
            slice<@string> ldflags = default;
            error err = default!;
            ref Builder b = ref _addr_b.val;
            ref load.Package p = ref _addr_p.val;

            {
                var pcargs = p.CgoPkgConfig;

                if (len(pcargs) > 0L)
                { 
                    // pkg-config permits arguments to appear anywhere in
                    // the command line. Move them all to the front, before --.
                    slice<@string> pcflags = default;
                    slice<@string> pkgs = default;
                    foreach (var (_, pcarg) in pcargs)
                    {
                        if (pcarg == "--")
                        { 
                            // We're going to add our own "--" argument.
                        }
                        else if (strings.HasPrefix(pcarg, "--"))
                        {
                            pcflags = append(pcflags, pcarg);
                        }
                        else
                        {
                            pkgs = append(pkgs, pcarg);
                        }

                    }
                    foreach (var (_, pkg) in pkgs)
                    {
                        if (!load.SafeArg(pkg))
                        {
                            return (null, null, error.As(fmt.Errorf("invalid pkg-config package name: %s", pkg))!);
                        }

                    }
                    slice<byte> @out = default;
                    out, err = b.runOut(null, p.Dir, null, b.PkgconfigCmd(), "--cflags", pcflags, "--", pkgs);
                    if (err != null)
                    {
                        b.showOutput(null, p.Dir, b.PkgconfigCmd() + " --cflags " + strings.Join(pcflags, " ") + " -- " + strings.Join(pkgs, " "), string(out));
                        b.Print(err.Error() + "\n");
                        return (null, null, error.As(errPrintedOutput)!);
                    }

                    if (len(out) > 0L)
                    {
                        cflags, err = splitPkgConfigOutput(out);
                        if (err != null)
                        {
                            return (null, null, error.As(err)!);
                        }

                        {
                            var err__prev3 = err;

                            var err = checkCompilerFlags("CFLAGS", "pkg-config --cflags", cflags);

                            if (err != null)
                            {
                                return (null, null, error.As(err)!);
                            }

                            err = err__prev3;

                        }

                    }

                    out, err = b.runOut(null, p.Dir, null, b.PkgconfigCmd(), "--libs", pcflags, "--", pkgs);
                    if (err != null)
                    {
                        b.showOutput(null, p.Dir, b.PkgconfigCmd() + " --libs " + strings.Join(pcflags, " ") + " -- " + strings.Join(pkgs, " "), string(out));
                        b.Print(err.Error() + "\n");
                        return (null, null, error.As(errPrintedOutput)!);
                    }

                    if (len(out) > 0L)
                    {
                        ldflags = strings.Fields(string(out));
                        {
                            var err__prev3 = err;

                            err = checkLinkerFlags("LDFLAGS", "pkg-config --libs", ldflags);

                            if (err != null)
                            {
                                return (null, null, error.As(err)!);
                            }

                            err = err__prev3;

                        }

                    }

                }

            }


            return ;

        }

        private static error installShlibname(this ptr<Builder> _addr_b, ptr<Action> _addr_a)
        {
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;

            {
                var err__prev1 = err;

                var err = allowInstall(a);

                if (err != null)
                {
                    return error.As(err)!;
                } 

                // TODO: BuildN

                err = err__prev1;

            } 

            // TODO: BuildN
            var a1 = a.Deps[0L];
            err = ioutil.WriteFile(a.Target, (slice<byte>)filepath.Base(a1.Target) + "\n", 0666L);
            if (err != null)
            {
                return error.As(err)!;
            }

            if (cfg.BuildX)
            {
                b.Showcmd("", "echo '%s' > %s # internal", filepath.Base(a1.Target), a.Target);
            }

            return error.As(null!)!;

        }

        private static cache.ActionID linkSharedActionID(this ptr<Builder> _addr_b, ptr<Action> _addr_a)
        {
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;

            var h = cache.NewHash("linkShared"); 

            // Toolchain-independent configuration.
            fmt.Fprintf(h, "linkShared\n");
            fmt.Fprintf(h, "goos %s goarch %s\n", cfg.Goos, cfg.Goarch); 

            // Toolchain-dependent configuration, shared with b.linkActionID.
            b.printLinkerConfig(h, null); 

            // Input files.
            {
                var a1__prev1 = a1;

                foreach (var (_, __a1) in a.Deps)
                {
                    a1 = __a1;
                    var p1 = a1.Package;
                    if (a1.built == "")
                    {
                        continue;
                    }

                    if (p1 != null)
                    {
                        fmt.Fprintf(h, "packagefile %s=%s\n", p1.ImportPath, contentID(b.buildID(a1.built)));
                        if (p1.Shlib != "")
                        {
                            fmt.Fprintf(h, "packageshlib %s=%s\n", p1.ImportPath, contentID(b.buildID(p1.Shlib)));
                        }

                    }

                } 
                // Files named on command line are special.

                a1 = a1__prev1;
            }

            {
                var a1__prev1 = a1;

                foreach (var (_, __a1) in a.Deps[0L].Deps)
                {
                    a1 = __a1;
                    p1 = a1.Package;
                    fmt.Fprintf(h, "top %s=%s\n", p1.ImportPath, contentID(b.buildID(a1.built)));
                }

                a1 = a1__prev1;
            }

            return h.Sum();

        }

        private static error linkShared(this ptr<Builder> _addr_b, ptr<Action> _addr_a) => func((defer, _, __) =>
        {
            error err = default!;
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;

            if (b.useCache(a, b.linkSharedActionID(a), a.Target) || b.IsCmdList)
            {
                return error.As(null!)!;
            }

            defer(b.flushOutput(a));

            {
                var err__prev1 = err;

                var err = allowInstall(a);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }


            {
                var err__prev1 = err;

                err = b.Mkdir(a.Objdir);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }


            var importcfg = a.Objdir + "importcfg.link";
            {
                var err__prev1 = err;

                err = b.writeLinkImportcfg(a, importcfg);

                if (err != null)
                {
                    return error.As(err)!;
                } 

                // TODO(rsc): There is a missing updateBuildID here,
                // but we have to decide where to store the build ID in these files.

                err = err__prev1;

            } 

            // TODO(rsc): There is a missing updateBuildID here,
            // but we have to decide where to store the build ID in these files.
            a.built = a.Target;
            return error.As(BuildToolchain.ldShared(b, a, a.Deps[0L].Deps, a.Target, importcfg, a.Deps))!;

        });

        // BuildInstallFunc is the action for installing a single package or executable.
        public static error BuildInstallFunc(ptr<Builder> _addr_b, ptr<Action> _addr_a) => func((defer, _, __) =>
        {
            error err = default!;
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;

            defer(() =>
            {
                if (err != null && err != errPrintedOutput)
                { 
                    // a.Package == nil is possible for the go install -buildmode=shared
                    // action that installs libmangledname.so, which corresponds to
                    // a list of packages, not just one.
                    @string sep = "";
                    @string path = "";
                    if (a.Package != null)
                    {
                        sep = " ";
                        path = a.Package.ImportPath;

                    }

                    err = fmt.Errorf("go %s%s%s: %v", cfg.CmdName, sep, path, err);

                }

            }());

            var a1 = a.Deps[0L];
            a.buildID = a1.buildID;
            if (a.json != null)
            {
                a.json.BuildID = a.buildID;
            } 

            // If we are using the eventual install target as an up-to-date
            // cached copy of the thing we built, then there's no need to
            // copy it into itself (and that would probably fail anyway).
            // In this case a1.built == a.Target because a1.built == p.Target,
            // so the built target is not in the a1.Objdir tree that b.cleanup(a1) removes.
            if (a1.built == a.Target)
            {
                a.built = a.Target;
                if (!a.buggyInstall)
                {
                    b.cleanup(a1);
                } 
                // Whether we're smart enough to avoid a complete rebuild
                // depends on exactly what the staleness and rebuild algorithms
                // are, as well as potentially the state of the Go build cache.
                // We don't really want users to be able to infer (or worse start depending on)
                // those details from whether the modification time changes during
                // "go install", so do a best-effort update of the file times to make it
                // look like we rewrote a.Target even if we did not. Updating the mtime
                // may also help other mtime-based systems that depend on our
                // previous mtime updates that happened more often.
                // This is still not perfect - we ignore the error result, and if the file was
                // unwritable for some reason then pretending to have written it is also
                // confusing - but it's probably better than not doing the mtime update.
                //
                // But don't do that for the special case where building an executable
                // with -linkshared implicitly installs all its dependent libraries.
                // We want to hide that awful detail as much as possible, so don't
                // advertise it by touching the mtimes (usually the libraries are up
                // to date).
                if (!a.buggyInstall && !b.IsCmdList)
                {
                    if (cfg.BuildN)
                    {
                        b.Showcmd("", "touch %s", a.Target);
                    }                    {
                        var err__prev4 = err;

                        var err = allowInstall(a);


                        else if (err == null)
                        {
                            var now = time.Now();
                            os.Chtimes(a.Target, now, now);
                        }

                        err = err__prev4;

                    }

                }

                return error.As(null!)!;

            } 

            // If we're building for go list -export,
            // never install anything; just keep the cache reference.
            if (b.IsCmdList)
            {
                a.built = a1.built;
                return error.As(null!)!;
            }

            {
                var err__prev1 = err;

                err = allowInstall(a);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }


            {
                var err__prev1 = err;

                err = b.Mkdir(a.Objdir);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }


            var perm = os.FileMode(0666L);
            if (a1.Mode == "link")
            {
                switch (cfg.BuildBuildmode)
                {
                    case "c-archive": 

                    case "c-shared": 

                    case "plugin": 
                        break;
                    default: 
                        perm = 0777L;
                        break;
                }

            } 

            // make target directory
            var (dir, _) = filepath.Split(a.Target);
            if (dir != "")
            {
                {
                    var err__prev2 = err;

                    err = b.Mkdir(dir);

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }

            }

            if (!a.buggyInstall)
            {
                defer(b.cleanup(a1));
            }

            return error.As(b.moveOrCopyFile(a.Target, a1.built, perm, false))!;

        });

        // allowInstall returns a non-nil error if this invocation of the go command is
        // allowed to install a.Target.
        //
        // (The build of cmd/go running under its own test is forbidden from installing
        // to its original GOROOT.)
        private static Func<ptr<Action>, error> allowInstall = _p0 => null;

        // cleanup removes a's object dir to keep the amount of
        // on-disk garbage down in a large build. On an operating system
        // with aggressive buffering, cleaning incrementally like
        // this keeps the intermediate objects from hitting the disk.
        private static void cleanup(this ptr<Builder> _addr_b, ptr<Action> _addr_a)
        {
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;

            if (!cfg.BuildWork)
            {
                if (cfg.BuildX)
                { 
                    // Don't say we are removing the directory if
                    // we never created it.
                    {
                        var (_, err) = os.Stat(a.Objdir);

                        if (err == null || cfg.BuildN)
                        {
                            b.Showcmd("", "rm -r %s", a.Objdir);
                        }

                    }

                }

                os.RemoveAll(a.Objdir);

            }

        }

        // moveOrCopyFile is like 'mv src dst' or 'cp src dst'.
        private static error moveOrCopyFile(this ptr<Builder> _addr_b, @string dst, @string src, os.FileMode perm, bool force)
        {
            ref Builder b = ref _addr_b.val;

            if (cfg.BuildN)
            {
                b.Showcmd("", "mv %s %s", src, dst);
                return error.As(null!)!;
            } 

            // If we can update the mode and rename to the dst, do it.
            // Otherwise fall back to standard copy.

            // If the source is in the build cache, we need to copy it.
            if (strings.HasPrefix(src, cache.DefaultDir()))
            {
                return error.As(b.copyFile(dst, src, perm, force))!;
            } 

            // On Windows, always copy the file, so that we respect the NTFS
            // permissions of the parent folder. https://golang.org/issue/22343.
            // What matters here is not cfg.Goos (the system we are building
            // for) but runtime.GOOS (the system we are building on).
            if (runtime.GOOS == "windows")
            {
                return error.As(b.copyFile(dst, src, perm, force))!;
            } 

            // If the destination directory has the group sticky bit set,
            // we have to copy the file to retain the correct permissions.
            // https://golang.org/issue/18878
            {
                var fi__prev1 = fi;
                var err__prev1 = err;

                var (fi, err) = os.Stat(filepath.Dir(dst));

                if (err == null)
                {
                    if (fi.IsDir() && (fi.Mode() & os.ModeSetgid) != 0L)
                    {
                        return error.As(b.copyFile(dst, src, perm, force))!;
                    }

                } 

                // The perm argument is meant to be adjusted according to umask,
                // but we don't know what the umask is.
                // Create a dummy file to find out.
                // This avoids build tags and works even on systems like Plan 9
                // where the file mask computation incorporates other information.

                fi = fi__prev1;
                err = err__prev1;

            } 

            // The perm argument is meant to be adjusted according to umask,
            // but we don't know what the umask is.
            // Create a dummy file to find out.
            // This avoids build tags and works even on systems like Plan 9
            // where the file mask computation incorporates other information.
            var mode = perm;
            var (f, err) = os.OpenFile(filepath.Clean(dst) + "-go-tmp-umask", os.O_WRONLY | os.O_CREATE | os.O_EXCL, perm);
            if (err == null)
            {
                (fi, err) = f.Stat();
                if (err == null)
                {
                    mode = fi.Mode() & 0777L;
                }

                var name = f.Name();
                f.Close();
                os.Remove(name);

            }

            {
                var err__prev1 = err;

                var err = os.Chmod(src, mode);

                if (err == null)
                {
                    {
                        var err__prev2 = err;

                        err = os.Rename(src, dst);

                        if (err == null)
                        {
                            if (cfg.BuildX)
                            {
                                b.Showcmd("", "mv %s %s", src, dst);
                            }

                            return error.As(null!)!;

                        }

                        err = err__prev2;

                    }

                }

                err = err__prev1;

            }


            return error.As(b.copyFile(dst, src, perm, force))!;

        }

        // copyFile is like 'cp src dst'.
        private static error copyFile(this ptr<Builder> _addr_b, @string dst, @string src, os.FileMode perm, bool force) => func((defer, _, __) =>
        {
            ref Builder b = ref _addr_b.val;

            if (cfg.BuildN || cfg.BuildX)
            {
                b.Showcmd("", "cp %s %s", src, dst);
                if (cfg.BuildN)
                {
                    return error.As(null!)!;
                }

            }

            var (sf, err) = os.Open(src);
            if (err != null)
            {
                return error.As(err)!;
            }

            defer(sf.Close()); 

            // Be careful about removing/overwriting dst.
            // Do not remove/overwrite if dst exists and is a directory
            // or a non-empty non-object file.
            {
                var (fi, err) = os.Stat(dst);

                if (err == null)
                {
                    if (fi.IsDir())
                    {
                        return error.As(fmt.Errorf("build output %q already exists and is a directory", dst))!;
                    }

                    if (!force && fi.Mode().IsRegular() && fi.Size() != 0L && !isObject(dst))
                    {
                        return error.As(fmt.Errorf("build output %q already exists and is not an object file", dst))!;
                    }

                } 

                // On Windows, remove lingering ~ file from last attempt.

            } 

            // On Windows, remove lingering ~ file from last attempt.
            if (@base.ToolIsWindows)
            {
                {
                    var (_, err) = os.Stat(dst + "~");

                    if (err == null)
                    {
                        os.Remove(dst + "~");
                    }

                }

            }

            mayberemovefile(dst);
            var (df, err) = os.OpenFile(dst, os.O_WRONLY | os.O_CREATE | os.O_TRUNC, perm);
            if (err != null && @base.ToolIsWindows)
            { 
                // Windows does not allow deletion of a binary file
                // while it is executing. Try to move it out of the way.
                // If the move fails, which is likely, we'll try again the
                // next time we do an install of this binary.
                {
                    var err = os.Rename(dst, dst + "~");

                    if (err == null)
                    {
                        os.Remove(dst + "~");
                    }

                }

                df, err = os.OpenFile(dst, os.O_WRONLY | os.O_CREATE | os.O_TRUNC, perm);

            }

            if (err != null)
            {
                return error.As(fmt.Errorf("copying %s: %w", src, err))!; // err should already refer to dst
            }

            _, err = io.Copy(df, sf);
            df.Close();
            if (err != null)
            {
                mayberemovefile(dst);
                return error.As(fmt.Errorf("copying %s to %s: %v", src, dst, err))!;
            }

            return error.As(null!)!;

        });

        // writeFile writes the text to file.
        private static error writeFile(this ptr<Builder> _addr_b, @string file, slice<byte> text)
        {
            ref Builder b = ref _addr_b.val;

            if (cfg.BuildN || cfg.BuildX)
            {
                b.Showcmd("", "cat >%s << 'EOF' # internal\n%sEOF", file, text);
            }

            if (cfg.BuildN)
            {
                return error.As(null!)!;
            }

            return error.As(ioutil.WriteFile(file, text, 0666L))!;

        }

        // Install the cgo export header file, if there is one.
        private static error installHeader(this ptr<Builder> _addr_b, ptr<Action> _addr_a)
        {
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;

            var src = a.Objdir + "_cgo_install.h";
            {
                var err__prev1 = err;

                var (_, err) = os.Stat(src);

                if (os.IsNotExist(err))
                { 
                    // If the file does not exist, there are no exported
                    // functions, and we do not install anything.
                    // TODO(rsc): Once we know that caching is rebuilding
                    // at the right times (not missing rebuilds), here we should
                    // probably delete the installed header, if any.
                    if (cfg.BuildX)
                    {
                        b.Showcmd("", "# %s not created", src);
                    }

                    return error.As(null!)!;

                }

                err = err__prev1;

            }


            {
                var err__prev1 = err;

                var err = allowInstall(a);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }


            var (dir, _) = filepath.Split(a.Target);
            if (dir != "")
            {
                {
                    var err__prev2 = err;

                    err = b.Mkdir(dir);

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }

            }

            return error.As(b.moveOrCopyFile(a.Target, src, 0666L, true))!;

        }

        // cover runs, in effect,
        //    go tool cover -mode=b.coverMode -var="varName" -o dst.go src.go
        private static error cover(this ptr<Builder> _addr_b, ptr<Action> _addr_a, @string dst, @string src, @string varName)
        {
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;

            return error.As(b.run(a, a.Objdir, "cover " + a.Package.ImportPath, null, cfg.BuildToolexec, @base.Tool("cover"), "-mode", a.Package.Internal.CoverMode, "-var", varName, "-o", dst, src))!;
        }

        private static slice<byte> objectMagic = new slice<slice<byte>>(new slice<byte>[] { {'!','<','a','r','c','h','>','\n'}, {'<','b','i','g','a','f','>','\n'}, {'\x7F','E','L','F'}, {0xFE,0xED,0xFA,0xCE}, {0xFE,0xED,0xFA,0xCF}, {0xCE,0xFA,0xED,0xFE}, {0xCF,0xFA,0xED,0xFE}, {0x4d,0x5a,0x90,0x00,0x03,0x00}, {0x00,0x00,0x01,0xEB}, {0x00,0x00,0x8a,0x97}, {0x00,0x00,0x06,0x47}, {0x00,0x61,0x73,0x6D}, {0x01,0xDF}, {0x01,0xF7} });

        private static bool isObject(@string s) => func((defer, _, __) =>
        {
            var (f, err) = os.Open(s);
            if (err != null)
            {
                return false;
            }

            defer(f.Close());
            var buf = make_slice<byte>(64L);
            io.ReadFull(f, buf);
            foreach (var (_, magic) in objectMagic)
            {
                if (bytes.HasPrefix(buf, magic))
                {
                    return true;
                }

            }
            return false;

        });

        // mayberemovefile removes a file only if it is a regular file
        // When running as a user with sufficient privileges, we may delete
        // even device files, for example, which is not intended.
        private static void mayberemovefile(@string s)
        {
            {
                var (fi, err) = os.Lstat(s);

                if (err == null && !fi.Mode().IsRegular())
                {
                    return ;
                }

            }

            os.Remove(s);

        }

        // fmtcmd formats a command in the manner of fmt.Sprintf but also:
        //
        //    If dir is non-empty and the script is not in dir right now,
        //    fmtcmd inserts "cd dir\n" before the command.
        //
        //    fmtcmd replaces the value of b.WorkDir with $WORK.
        //    fmtcmd replaces the value of goroot with $GOROOT.
        //    fmtcmd replaces the value of b.gobin with $GOBIN.
        //
        //    fmtcmd replaces the name of the current directory with dot (.)
        //    but only when it is at the beginning of a space-separated token.
        //
        private static @string fmtcmd(this ptr<Builder> _addr_b, @string dir, @string format, params object[] args)
        {
            args = args.Clone();
            ref Builder b = ref _addr_b.val;

            var cmd = fmt.Sprintf(format, args);
            if (dir != "" && dir != "/")
            {
                @string dot = " .";
                if (dir[len(dir) - 1L] == filepath.Separator)
                {
                    dot += string(filepath.Separator);
                }

                cmd = strings.ReplaceAll(" " + cmd, " " + dir, dot)[1L..];
                if (b.scriptDir != dir)
                {
                    b.scriptDir = dir;
                    cmd = "cd " + dir + "\n" + cmd;
                }

            }

            if (b.WorkDir != "")
            {
                cmd = strings.ReplaceAll(cmd, b.WorkDir, "$WORK");
                var escaped = strconv.Quote(b.WorkDir);
                escaped = escaped[1L..len(escaped) - 1L]; // strip quote characters
                if (escaped != b.WorkDir)
                {
                    cmd = strings.ReplaceAll(cmd, escaped, "$WORK");
                }

            }

            return cmd;

        }

        // showcmd prints the given command to standard output
        // for the implementation of -n or -x.
        private static void Showcmd(this ptr<Builder> _addr_b, @string dir, @string format, params object[] args) => func((defer, _, __) =>
        {
            args = args.Clone();
            ref Builder b = ref _addr_b.val;

            b.output.Lock();
            defer(b.output.Unlock());
            b.Print(b.fmtcmd(dir, format, args) + "\n");
        });

        // showOutput prints "# desc" followed by the given output.
        // The output is expected to contain references to 'dir', usually
        // the source directory for the package that has failed to build.
        // showOutput rewrites mentions of dir with a relative path to dir
        // when the relative path is shorter. This is usually more pleasant.
        // For example, if fmt doesn't compile and we are in src/html,
        // the output is
        //
        //    $ go build
        //    # fmt
        //    ../fmt/print.go:1090: undefined: asdf
        //    $
        //
        // instead of
        //
        //    $ go build
        //    # fmt
        //    /usr/gopher/go/src/fmt/print.go:1090: undefined: asdf
        //    $
        //
        // showOutput also replaces references to the work directory with $WORK.
        //
        // If a is not nil and a.output is not nil, showOutput appends to that slice instead of
        // printing to b.Print.
        //
        private static void showOutput(this ptr<Builder> _addr_b, ptr<Action> _addr_a, @string dir, @string desc, @string @out) => func((defer, _, __) =>
        {
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;

            @string prefix = "# " + desc;
            @string suffix = "\n" + out;
            {
                var reldir = @base.ShortPath(dir);

                if (reldir != dir)
                {
                    suffix = strings.ReplaceAll(suffix, " " + dir, " " + reldir);
                    suffix = strings.ReplaceAll(suffix, "\n" + dir, "\n" + reldir);
                }

            }

            suffix = strings.ReplaceAll(suffix, " " + b.WorkDir, " $WORK");

            if (a != null && a.output != null)
            {
                a.output = append(a.output, prefix);
                a.output = append(a.output, suffix);
                return ;
            }

            b.output.Lock();
            defer(b.output.Unlock());
            b.Print(prefix, suffix);

        });

        // errPrintedOutput is a special error indicating that a command failed
        // but that it generated output as well, and that output has already
        // been printed, so there's no point showing 'exit status 1' or whatever
        // the wait status was. The main executor, builder.do, knows not to
        // print this error.
        private static var errPrintedOutput = errors.New("already printed output - no need to show error");

        private static var cgoLine = lazyregexp.New("\\[[^\\[\\]]+\\.(cgo1|cover)\\.go:[0-9]+(:[0-9]+)?\\]");
        private static var cgoTypeSigRe = lazyregexp.New("\\b_C2?(type|func|var|macro)_\\B");

        // run runs the command given by cmdline in the directory dir.
        // If the command fails, run prints information about the failure
        // and returns a non-nil error.
        private static error run(this ptr<Builder> _addr_b, ptr<Action> _addr_a, @string dir, @string desc, slice<@string> env, params object[] cmdargs)
        {
            cmdargs = cmdargs.Clone();
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;

            var (out, err) = b.runOut(a, dir, env, cmdargs);
            if (len(out) > 0L)
            {
                if (desc == "")
                {
                    desc = b.fmtcmd(dir, "%s", strings.Join(str.StringList(cmdargs), " "));
                }

                b.showOutput(a, dir, desc, b.processOutput(out));
                if (err != null)
                {
                    err = errPrintedOutput;
                }

            }

            return error.As(err)!;

        }

        // processOutput prepares the output of runOut to be output to the console.
        private static @string processOutput(this ptr<Builder> _addr_b, slice<byte> @out)
        {
            ref Builder b = ref _addr_b.val;

            if (out[len(out) - 1L] != '\n')
            {
                out = append(out, '\n');
            }

            var messages = string(out); 
            // Fix up output referring to cgo-generated code to be more readable.
            // Replace x.go:19[/tmp/.../x.cgo1.go:18] with x.go:19.
            // Replace *[100]_Ctype_foo with *[100]C.foo.
            // If we're using -x, assume we're debugging and want the full dump, so disable the rewrite.
            if (!cfg.BuildX && cgoLine.MatchString(messages))
            {
                messages = cgoLine.ReplaceAllString(messages, "");
                messages = cgoTypeSigRe.ReplaceAllString(messages, "C.");
            }

            return messages;

        }

        // runOut runs the command given by cmdline in the directory dir.
        // It returns the command output and any errors that occurred.
        // It accumulates execution time in a.
        private static (slice<byte>, error) runOut(this ptr<Builder> _addr_b, ptr<Action> _addr_a, @string dir, slice<@string> env, params object[] cmdargs) => func((defer, _, __) =>
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            cmdargs = cmdargs.Clone();
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;

            var cmdline = str.StringList(cmdargs);

            foreach (var (_, arg) in cmdline)
            { 
                // GNU binutils commands, including gcc and gccgo, interpret an argument
                // @foo anywhere in the command line (even following --) as meaning
                // "read and insert arguments from the file named foo."
                // Don't say anything that might be misinterpreted that way.
                if (strings.HasPrefix(arg, "@"))
                {
                    return (null, error.As(fmt.Errorf("invalid command-line argument %s in command: %s", arg, joinUnambiguously(cmdline)))!);
                }

            }
            if (cfg.BuildN || cfg.BuildX)
            {
                @string envcmdline = default;
                foreach (var (_, e) in env)
                {
                    {
                        var j = strings.IndexByte(e, '=');

                        if (j != -1L)
                        {
                            if (strings.ContainsRune(e[j + 1L..], '\''))
                            {
                                envcmdline += fmt.Sprintf("%s=%q", e[..j], e[j + 1L..]);
                            }
                            else
                            {
                                envcmdline += fmt.Sprintf("%s='%s'", e[..j], e[j + 1L..]);
                            }

                            envcmdline += " ";

                        }

                    }

                }
                envcmdline += joinUnambiguously(cmdline);
                b.Showcmd(dir, "%s", envcmdline);
                if (cfg.BuildN)
                {
                    return (null, error.As(null!)!);
                }

            }

            ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
            var cmd = exec.Command(cmdline[0L], cmdline[1L..]);
            _addr_cmd.Stdout = _addr_buf;
            cmd.Stdout = ref _addr_cmd.Stdout.val;
            _addr_cmd.Stderr = _addr_buf;
            cmd.Stderr = ref _addr_cmd.Stderr.val;
            var cleanup = passLongArgsInResponseFiles(_addr_cmd);
            defer(cleanup());
            cmd.Dir = dir;
            cmd.Env = @base.AppendPWD(os.Environ(), cmd.Dir);
            cmd.Env = append(cmd.Env, env);
            var start = time.Now();
            var err = cmd.Run();
            if (a != null && a.json != null)
            {
                var aj = a.json;
                aj.Cmd = append(aj.Cmd, joinUnambiguously(cmdline));
                aj.CmdReal += time.Since(start);
                {
                    var ps = cmd.ProcessState;

                    if (ps != null)
                    {
                        aj.CmdUser += ps.UserTime();
                        aj.CmdSys += ps.SystemTime();
                    }

                }

            } 

            // err can be something like 'exit status 1'.
            // Add information about what program was running.
            // Note that if buf.Bytes() is non-empty, the caller usually
            // shows buf.Bytes() and does not print err at all, so the
            // prefix here does not make most output any more verbose.
            if (err != null)
            {
                err = errors.New(cmdline[0L] + ": " + err.Error());
            }

            return (buf.Bytes(), error.As(err)!);

        });

        // joinUnambiguously prints the slice, quoting where necessary to make the
        // output unambiguous.
        // TODO: See issue 5279. The printing of commands needs a complete redo.
        private static @string joinUnambiguously(slice<@string> a)
        {
            bytes.Buffer buf = default;
            foreach (var (i, s) in a)
            {
                if (i > 0L)
                {
                    buf.WriteByte(' ');
                }

                var q = strconv.Quote(s); 
                // A gccgo command line can contain -( and -).
                // Make sure we quote them since they are special to the shell.
                // The trimpath argument can also contain > (part of =>) and ;. Quote those too.
                if (s == "" || strings.ContainsAny(s, " ()>;") || len(q) > len(s) + 2L)
                {
                    buf.WriteString(q);
                }
                else
                {
                    buf.WriteString(s);
                }

            }
            return buf.String();

        }

        // cCompilerEnv returns environment variables to set when running the
        // C compiler. This is needed to disable escape codes in clang error
        // messages that confuse tools like cgo.
        private static slice<@string> cCompilerEnv(this ptr<Builder> _addr_b)
        {
            ref Builder b = ref _addr_b.val;

            return new slice<@string>(new @string[] { "TERM=dumb" });
        }

        // mkdir makes the named directory.
        private static error Mkdir(this ptr<Builder> _addr_b, @string dir) => func((defer, _, __) =>
        {
            ref Builder b = ref _addr_b.val;
 
            // Make Mkdir(a.Objdir) a no-op instead of an error when a.Objdir == "".
            if (dir == "")
            {
                return error.As(null!)!;
            }

            b.exec.Lock();
            defer(b.exec.Unlock()); 
            // We can be a little aggressive about being
            // sure directories exist. Skip repeated calls.
            if (b.mkdirCache[dir])
            {
                return error.As(null!)!;
            }

            b.mkdirCache[dir] = true;

            if (cfg.BuildN || cfg.BuildX)
            {
                b.Showcmd("", "mkdir -p %s", dir);
                if (cfg.BuildN)
                {
                    return error.As(null!)!;
                }

            }

            {
                var err = os.MkdirAll(dir, 0777L);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            return error.As(null!)!;

        });

        // symlink creates a symlink newname -> oldname.
        private static error Symlink(this ptr<Builder> _addr_b, @string oldname, @string newname)
        {
            ref Builder b = ref _addr_b.val;
 
            // It's not an error to try to recreate an existing symlink.
            {
                var (link, err) = os.Readlink(newname);

                if (err == null && link == oldname)
                {
                    return error.As(null!)!;
                }

            }


            if (cfg.BuildN || cfg.BuildX)
            {
                b.Showcmd("", "ln -s %s %s", oldname, newname);
                if (cfg.BuildN)
                {
                    return error.As(null!)!;
                }

            }

            return error.As(os.Symlink(oldname, newname))!;

        }

        // mkAbs returns an absolute path corresponding to
        // evaluating f in the directory dir.
        // We always pass absolute paths of source files so that
        // the error messages will include the full path to a file
        // in need of attention.
        private static @string mkAbs(@string dir, @string f)
        { 
            // Leave absolute paths alone.
            // Also, during -n mode we use the pseudo-directory $WORK
            // instead of creating an actual work directory that won't be used.
            // Leave paths beginning with $WORK alone too.
            if (filepath.IsAbs(f) || strings.HasPrefix(f, "$WORK"))
            {
                return f;
            }

            return filepath.Join(dir, f);

        }

        private partial interface toolchain
        {
            @string gc(ptr<Builder> b, ptr<Action> a, @string archive, slice<byte> importcfg, @string symabis, bool asmhdr, slice<@string> gofiles); // cc runs the toolchain's C compiler in a directory on a C file
// to produce an output file.
            @string cc(ptr<Builder> b, ptr<Action> a, @string ofile, @string cfile); // asm runs the assembler in a specific directory on specific files
// and returns a list of named output files.
            @string asm(ptr<Builder> b, ptr<Action> a, slice<@string> sfiles); // symabis scans the symbol ABIs from sfiles and returns the
// path to the output symbol ABIs file, or "" if none.
            @string symabis(ptr<Builder> b, ptr<Action> a, slice<@string> sfiles); // pack runs the archive packer in a specific directory to create
// an archive from a set of object files.
// typically it is run in the object directory.
            @string pack(ptr<Builder> b, ptr<Action> a, @string afile, slice<@string> ofiles); // ld runs the linker to create an executable starting at mainpkg.
            @string ld(ptr<Builder> b, ptr<Action> root, @string @out, @string importcfg, @string mainpkg); // ldShared runs the linker to create a shared library containing the pkgs built by toplevelactions
            @string ldShared(ptr<Builder> b, ptr<Action> root, slice<ptr<Action>> toplevelactions, @string @out, @string importcfg, slice<ptr<Action>> allactions);
            @string compiler();
            @string linker();
        }

        private partial struct noToolchain
        {
        }

        private static error noCompiler()
        {
            log.Fatalf("unknown compiler %q", cfg.BuildContext.Compiler);
            return error.As(null!)!;
        }

        private static @string compiler(this noToolchain _p0)
        {
            noCompiler();
            return "";
        }

        private static @string linker(this noToolchain _p0)
        {
            noCompiler();
            return "";
        }

        private static (@string, slice<byte>, error) gc(this noToolchain _p0, ptr<Builder> _addr_b, ptr<Action> _addr_a, @string archive, slice<byte> importcfg, @string symabis, bool asmhdr, slice<@string> gofiles)
        {
            @string ofile = default;
            slice<byte> @out = default;
            error err = default!;
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;

            return ("", null, error.As(noCompiler())!);
        }

        private static (slice<@string>, error) asm(this noToolchain _p0, ptr<Builder> _addr_b, ptr<Action> _addr_a, slice<@string> sfiles)
        {
            slice<@string> _p0 = default;
            error _p0 = default!;
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;

            return (null, error.As(noCompiler())!);
        }

        private static (@string, error) symabis(this noToolchain _p0, ptr<Builder> _addr_b, ptr<Action> _addr_a, slice<@string> sfiles)
        {
            @string _p0 = default;
            error _p0 = default!;
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;

            return ("", error.As(noCompiler())!);
        }

        private static error pack(this noToolchain _p0, ptr<Builder> _addr_b, ptr<Action> _addr_a, @string afile, slice<@string> ofiles)
        {
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;

            return error.As(noCompiler())!;
        }

        private static error ld(this noToolchain _p0, ptr<Builder> _addr_b, ptr<Action> _addr_root, @string @out, @string importcfg, @string mainpkg)
        {
            ref Builder b = ref _addr_b.val;
            ref Action root = ref _addr_root.val;

            return error.As(noCompiler())!;
        }

        private static error ldShared(this noToolchain _p0, ptr<Builder> _addr_b, ptr<Action> _addr_root, slice<ptr<Action>> toplevelactions, @string @out, @string importcfg, slice<ptr<Action>> allactions)
        {
            ref Builder b = ref _addr_b.val;
            ref Action root = ref _addr_root.val;

            return error.As(noCompiler())!;
        }

        private static error cc(this noToolchain _p0, ptr<Builder> _addr_b, ptr<Action> _addr_a, @string ofile, @string cfile)
        {
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;

            return error.As(noCompiler())!;
        }

        // gcc runs the gcc C compiler to create an object from a single C file.
        private static error gcc(this ptr<Builder> _addr_b, ptr<Action> _addr_a, ptr<load.Package> _addr_p, @string workdir, @string @out, slice<@string> flags, @string cfile)
        {
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;
            ref load.Package p = ref _addr_p.val;

            return error.As(b.ccompile(a, p, out, flags, cfile, b.GccCmd(p.Dir, workdir)))!;
        }

        // gxx runs the g++ C++ compiler to create an object from a single C++ file.
        private static error gxx(this ptr<Builder> _addr_b, ptr<Action> _addr_a, ptr<load.Package> _addr_p, @string workdir, @string @out, slice<@string> flags, @string cxxfile)
        {
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;
            ref load.Package p = ref _addr_p.val;

            return error.As(b.ccompile(a, p, out, flags, cxxfile, b.GxxCmd(p.Dir, workdir)))!;
        }

        // gfortran runs the gfortran Fortran compiler to create an object from a single Fortran file.
        private static error gfortran(this ptr<Builder> _addr_b, ptr<Action> _addr_a, ptr<load.Package> _addr_p, @string workdir, @string @out, slice<@string> flags, @string ffile)
        {
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;
            ref load.Package p = ref _addr_p.val;

            return error.As(b.ccompile(a, p, out, flags, ffile, b.gfortranCmd(p.Dir, workdir)))!;
        }

        // ccompile runs the given C or C++ compiler and creates an object from a single source file.
        private static error ccompile(this ptr<Builder> _addr_b, ptr<Action> _addr_a, ptr<load.Package> _addr_p, @string outfile, slice<@string> flags, @string file, slice<@string> compiler)
        {
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;
            ref load.Package p = ref _addr_p.val;

            file = mkAbs(p.Dir, file);
            var desc = p.ImportPath;
            outfile = mkAbs(p.Dir, outfile); 

            // Elide source directory paths if -trimpath or GOROOT_FINAL is set.
            // This is needed for source files (e.g., a .c file in a package directory).
            // TODO(golang.org/issue/36072): cgo also generates files with #line
            // directives pointing to the source directory. It should not generate those
            // when -trimpath is enabled.
            if (b.gccSupportsFlag(compiler, "-fdebug-prefix-map=a=b"))
            {
                if (cfg.BuildTrimpath)
                { 
                    // Keep in sync with Action.trimpath.
                    // The trimmed paths are a little different, but we need to trim in the
                    // same situations.
                    @string from = default;                    @string toPath = default;

                    {
                        var m = p.Module;

                        if (m != null)
                        {
                            from = m.Dir;
                            toPath = m.Path + "@" + m.Version;
                        }
                        else
                        {
                            from = p.Dir;
                            toPath = p.ImportPath;
                        } 
                        // -fdebug-prefix-map requires an absolute "to" path (or it joins the path
                        // with the working directory). Pick something that makes sense for the
                        // target platform.

                    } 
                    // -fdebug-prefix-map requires an absolute "to" path (or it joins the path
                    // with the working directory). Pick something that makes sense for the
                    // target platform.
                    @string to = default;
                    if (cfg.BuildContext.GOOS == "windows")
                    {
                        to = filepath.Join("\\\\_\\_", toPath);
                    }
                    else
                    {
                        to = filepath.Join("/_", toPath);
                    }

                    flags = append(flags.slice(-1, len(flags), len(flags)), "-fdebug-prefix-map=" + from + "=" + to);

                }
                else if (p.Goroot && cfg.GOROOT_FINAL != cfg.GOROOT)
                {
                    flags = append(flags.slice(-1, len(flags), len(flags)), "-fdebug-prefix-map=" + cfg.GOROOT + "=" + cfg.GOROOT_FINAL);
                }

            }

            var (output, err) = b.runOut(a, filepath.Dir(file), b.cCompilerEnv(), compiler, flags, "-o", outfile, "-c", filepath.Base(file));
            if (len(output) > 0L)
            { 
                // On FreeBSD 11, when we pass -g to clang 3.8 it
                // invokes its internal assembler with -dwarf-version=2.
                // When it sees .section .note.GNU-stack, it warns
                // "DWARF2 only supports one section per compilation unit".
                // This warning makes no sense, since the section is empty,
                // but it confuses people.
                // We work around the problem by detecting the warning
                // and dropping -g and trying again.
                if (bytes.Contains(output, (slice<byte>)"DWARF2 only supports one section per compilation unit"))
                {
                    var newFlags = make_slice<@string>(0L, len(flags));
                    foreach (var (_, f) in flags)
                    {
                        if (!strings.HasPrefix(f, "-g"))
                        {
                            newFlags = append(newFlags, f);
                        }

                    }
                    if (len(newFlags) < len(flags))
                    {
                        return error.As(b.ccompile(a, p, outfile, newFlags, file, compiler))!;
                    }

                }

                b.showOutput(a, p.Dir, desc, b.processOutput(output));
                if (err != null)
                {
                    err = errPrintedOutput;
                }
                else if (os.Getenv("GO_BUILDER_NAME") != "")
                {
                    return error.As(errors.New("C compiler warning promoted to error on Go builders"))!;
                }

            }

            return error.As(err)!;

        }

        // gccld runs the gcc linker to create an executable from a set of object files.
        private static error gccld(this ptr<Builder> _addr_b, ptr<Action> _addr_a, ptr<load.Package> _addr_p, @string objdir, @string outfile, slice<@string> flags, slice<@string> objs)
        {
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;
            ref load.Package p = ref _addr_p.val;

            slice<@string> cmd = default;
            if (len(p.CXXFiles) > 0L || len(p.SwigCXXFiles) > 0L)
            {
                cmd = b.GxxCmd(p.Dir, objdir);
            }
            else
            {
                cmd = b.GccCmd(p.Dir, objdir);
            }

            var dir = p.Dir;
            var (out, err) = b.runOut(a, dir, b.cCompilerEnv(), cmdargs);
            if (len(out) > 0L)
            { 
                // Filter out useless linker warnings caused by bugs outside Go.
                // See also cmd/link/internal/ld's hostlink method.
                slice<slice<byte>> save = default;
                long skipLines = default;
                foreach (var (_, line) in bytes.SplitAfter(out, (slice<byte>)"\n"))
                { 
                    // golang.org/issue/26073 - Apple Xcode bug
                    if (bytes.Contains(line, (slice<byte>)"ld: warning: text-based stub file"))
                    {
                        continue;
                    }

                    if (skipLines > 0L)
                    {
                        skipLines--;
                        continue;
                    } 

                    // Remove duplicate main symbol with runtime/cgo on AIX.
                    // With runtime/cgo, two main are available:
                    // One is generated by cgo tool with {return 0;}.
                    // The other one is the main calling runtime.rt0_go
                    // in runtime/cgo.
                    // The second can't be used by cgo programs because
                    // runtime.rt0_go is unknown to them.
                    // Therefore, we let ld remove this main version
                    // and used the cgo generated one.
                    if (p.ImportPath == "runtime/cgo" && bytes.Contains(line, (slice<byte>)"ld: 0711-224 WARNING: Duplicate symbol: .main"))
                    {
                        skipLines = 1L;
                        continue;
                    }

                    save = append(save, line);

                }
                out = bytes.Join(save, null);
                if (len(out) > 0L)
                {
                    b.showOutput(null, dir, p.ImportPath, b.processOutput(out));
                    if (err != null)
                    {
                        err = errPrintedOutput;
                    }

                }

            }

            return error.As(err)!;

        }

        // Grab these before main helpfully overwrites them.
        private static var origCC = cfg.Getenv("CC");        private static var origCXX = cfg.Getenv("CXX");

        // gccCmd returns a gcc command line prefix
        // defaultCC is defined in zdefaultcc.go, written by cmd/dist.
        private static slice<@string> GccCmd(this ptr<Builder> _addr_b, @string incdir, @string workdir)
        {
            ref Builder b = ref _addr_b.val;

            return b.compilerCmd(b.ccExe(), incdir, workdir);
        }

        // gxxCmd returns a g++ command line prefix
        // defaultCXX is defined in zdefaultcc.go, written by cmd/dist.
        private static slice<@string> GxxCmd(this ptr<Builder> _addr_b, @string incdir, @string workdir)
        {
            ref Builder b = ref _addr_b.val;

            return b.compilerCmd(b.cxxExe(), incdir, workdir);
        }

        // gfortranCmd returns a gfortran command line prefix.
        private static slice<@string> gfortranCmd(this ptr<Builder> _addr_b, @string incdir, @string workdir)
        {
            ref Builder b = ref _addr_b.val;

            return b.compilerCmd(b.fcExe(), incdir, workdir);
        }

        // ccExe returns the CC compiler setting without all the extra flags we add implicitly.
        private static slice<@string> ccExe(this ptr<Builder> _addr_b)
        {
            ref Builder b = ref _addr_b.val;

            return b.compilerExe(origCC, cfg.DefaultCC(cfg.Goos, cfg.Goarch));
        }

        // cxxExe returns the CXX compiler setting without all the extra flags we add implicitly.
        private static slice<@string> cxxExe(this ptr<Builder> _addr_b)
        {
            ref Builder b = ref _addr_b.val;

            return b.compilerExe(origCXX, cfg.DefaultCXX(cfg.Goos, cfg.Goarch));
        }

        // fcExe returns the FC compiler setting without all the extra flags we add implicitly.
        private static slice<@string> fcExe(this ptr<Builder> _addr_b)
        {
            ref Builder b = ref _addr_b.val;

            return b.compilerExe(cfg.Getenv("FC"), "gfortran");
        }

        // compilerExe returns the compiler to use given an
        // environment variable setting (the value not the name)
        // and a default. The resulting slice is usually just the name
        // of the compiler but can have additional arguments if they
        // were present in the environment value.
        // For example if CC="gcc -DGOPHER" then the result is ["gcc", "-DGOPHER"].
        private static slice<@string> compilerExe(this ptr<Builder> _addr_b, @string envValue, @string def)
        {
            ref Builder b = ref _addr_b.val;

            var compiler = strings.Fields(envValue);
            if (len(compiler) == 0L)
            {
                compiler = new slice<@string>(new @string[] { def });
            }

            return compiler;

        }

        // compilerCmd returns a command line prefix for the given environment
        // variable and using the default command when the variable is empty.
        private static slice<@string> compilerCmd(this ptr<Builder> _addr_b, slice<@string> compiler, @string incdir, @string workdir)
        {
            ref Builder b = ref _addr_b.val;
 
            // NOTE: env.go's mkEnv knows that the first three
            // strings returned are "gcc", "-I", incdir (and cuts them off).
            @string a = new slice<@string>(new @string[] { compiler[0], "-I", incdir });
            a = append(a, compiler[1L..]); 

            // Definitely want -fPIC but on Windows gcc complains
            // "-fPIC ignored for target (all code is position independent)"
            if (cfg.Goos != "windows")
            {
                a = append(a, "-fPIC");
            }

            a = append(a, b.gccArchArgs()); 
            // gcc-4.5 and beyond require explicit "-pthread" flag
            // for multithreading with pthread library.
            if (cfg.BuildContext.CgoEnabled)
            {
                switch (cfg.Goos)
                {
                    case "windows": 
                        a = append(a, "-mthreads");
                        break;
                    default: 
                        a = append(a, "-pthread");
                        break;
                }

            }

            if (cfg.Goos == "aix")
            { 
                // mcmodel=large must always be enabled to allow large TOC.
                a = append(a, "-mcmodel=large");

            } 

            // disable ASCII art in clang errors, if possible
            if (b.gccSupportsFlag(compiler, "-fno-caret-diagnostics"))
            {
                a = append(a, "-fno-caret-diagnostics");
            } 
            // clang is too smart about command-line arguments
            if (b.gccSupportsFlag(compiler, "-Qunused-arguments"))
            {
                a = append(a, "-Qunused-arguments");
            } 

            // disable word wrapping in error messages
            a = append(a, "-fmessage-length=0"); 

            // Tell gcc not to include the work directory in object files.
            if (b.gccSupportsFlag(compiler, "-fdebug-prefix-map=a=b"))
            {
                if (workdir == "")
                {
                    workdir = b.WorkDir;
                }

                workdir = strings.TrimSuffix(workdir, string(filepath.Separator));
                a = append(a, "-fdebug-prefix-map=" + workdir + "=/tmp/go-build");

            } 

            // Tell gcc not to include flags in object files, which defeats the
            // point of -fdebug-prefix-map above.
            if (b.gccSupportsFlag(compiler, "-gno-record-gcc-switches"))
            {
                a = append(a, "-gno-record-gcc-switches");
            } 

            // On OS X, some of the compilers behave as if -fno-common
            // is always set, and the Mach-O linker in 6l/8l assumes this.
            // See https://golang.org/issue/3253.
            if (cfg.Goos == "darwin")
            {
                a = append(a, "-fno-common");
            }

            return a;

        }

        // gccNoPie returns the flag to use to request non-PIE. On systems
        // with PIE (position independent executables) enabled by default,
        // -no-pie must be passed when doing a partial link with -Wl,-r.
        // But -no-pie is not supported by all compilers, and clang spells it -nopie.
        private static @string gccNoPie(this ptr<Builder> _addr_b, slice<@string> linker)
        {
            ref Builder b = ref _addr_b.val;

            if (b.gccSupportsFlag(linker, "-no-pie"))
            {
                return "-no-pie";
            }

            if (b.gccSupportsFlag(linker, "-nopie"))
            {
                return "-nopie";
            }

            return "";

        }

        // gccSupportsFlag checks to see if the compiler supports a flag.
        private static bool gccSupportsFlag(this ptr<Builder> _addr_b, slice<@string> compiler, @string flag) => func((defer, _, __) =>
        {
            ref Builder b = ref _addr_b.val;

            array<@string> key = new array<@string>(new @string[] { compiler[0], flag });

            b.exec.Lock();
            defer(b.exec.Unlock());
            {
                var (b, ok) = b.flagCache[key];

                if (ok)
                {
                    return b;
                }

            }

            if (b.flagCache == null)
            {
                b.flagCache = make_map<array<@string>, bool>();
            }

            var tmp = os.DevNull;
            if (runtime.GOOS == "windows")
            {
                var (f, err) = ioutil.TempFile(b.WorkDir, "");
                if (err != null)
                {
                    return false;
                }

                f.Close();
                tmp = f.Name();
                defer(os.Remove(tmp));

            } 

            // We used to write an empty C file, but that gets complicated with
            // go build -n. We tried using a file that does not exist, but that
            // fails on systems with GCC version 4.2.1; that is the last GPLv2
            // version of GCC, so some systems have frozen on it.
            // Now we pass an empty file on stdin, which should work at least for
            // GCC and clang.
            var cmdArgs = str.StringList(compiler, flag, "-c", "-x", "c", "-", "-o", tmp);
            if (cfg.BuildN || cfg.BuildX)
            {
                b.Showcmd(b.WorkDir, "%s || true", joinUnambiguously(cmdArgs));
                if (cfg.BuildN)
                {
                    return false;
                }

            }

            var cmd = exec.Command(cmdArgs[0L], cmdArgs[1L..]);
            cmd.Dir = b.WorkDir;
            cmd.Env = @base.AppendPWD(os.Environ(), cmd.Dir);
            cmd.Env = append(cmd.Env, "LC_ALL=C");
            var (out, _) = cmd.CombinedOutput(); 
            // GCC says "unrecognized command line option".
            // clang says "unknown argument".
            // Older versions of GCC say "unrecognised debug output level".
            // For -fsplit-stack GCC says "'-fsplit-stack' is not supported".
            var supported = !bytes.Contains(out, (slice<byte>)"unrecognized") && !bytes.Contains(out, (slice<byte>)"unknown") && !bytes.Contains(out, (slice<byte>)"unrecognised") && !bytes.Contains(out, (slice<byte>)"is not supported");
            b.flagCache[key] = supported;
            return supported;

        });

        // gccArchArgs returns arguments to pass to gcc based on the architecture.
        private static slice<@string> gccArchArgs(this ptr<Builder> _addr_b)
        {
            ref Builder b = ref _addr_b.val;

            switch (cfg.Goarch)
            {
                case "386": 
                    return new slice<@string>(new @string[] { "-m32" });
                    break;
                case "amd64": 
                    return new slice<@string>(new @string[] { "-m64" });
                    break;
                case "arm": 
                    return new slice<@string>(new @string[] { "-marm" }); // not thumb
                    break;
                case "s390x": 
                    return new slice<@string>(new @string[] { "-m64", "-march=z196" });
                    break;
                case "mips64": 

                case "mips64le": 
                    return new slice<@string>(new @string[] { "-mabi=64" });
                    break;
                case "mips": 

                case "mipsle": 
                    return new slice<@string>(new @string[] { "-mabi=32", "-march=mips32" });
                    break;
                case "ppc64": 
                    if (cfg.Goos == "aix")
                    {
                        return new slice<@string>(new @string[] { "-maix64" });
                    }

                    break;
            }
            return null;

        }

        // envList returns the value of the given environment variable broken
        // into fields, using the default value when the variable is empty.
        private static slice<@string> envList(@string key, @string def)
        {
            var v = cfg.Getenv(key);
            if (v == "")
            {
                v = def;
            }

            return strings.Fields(v);

        }

        // CFlags returns the flags to use when invoking the C, C++ or Fortran compilers, or cgo.
        private static (slice<@string>, slice<@string>, slice<@string>, slice<@string>, slice<@string>, error) CFlags(this ptr<Builder> _addr_b, ptr<load.Package> _addr_p)
        {
            slice<@string> cppflags = default;
            slice<@string> cflags = default;
            slice<@string> cxxflags = default;
            slice<@string> fflags = default;
            slice<@string> ldflags = default;
            error err = default!;
            ref Builder b = ref _addr_b.val;
            ref load.Package p = ref _addr_p.val;

            @string defaults = "-g -O2";

            cppflags, err = buildFlags("CPPFLAGS", "", p.CgoCPPFLAGS, checkCompilerFlags);

            if (err != null)
            {
                return ;
            }

            cflags, err = buildFlags("CFLAGS", defaults, p.CgoCFLAGS, checkCompilerFlags);

            if (err != null)
            {
                return ;
            }

            cxxflags, err = buildFlags("CXXFLAGS", defaults, p.CgoCXXFLAGS, checkCompilerFlags);

            if (err != null)
            {
                return ;
            }

            fflags, err = buildFlags("FFLAGS", defaults, p.CgoFFLAGS, checkCompilerFlags);

            if (err != null)
            {
                return ;
            }

            ldflags, err = buildFlags("LDFLAGS", defaults, p.CgoLDFLAGS, checkLinkerFlags);

            if (err != null)
            {
                return ;
            }

            return ;

        }

        private static (slice<@string>, error) buildFlags(@string name, @string defaults, slice<@string> fromPackage, Func<@string, @string, slice<@string>, error> check)
        {
            slice<@string> _p0 = default;
            error _p0 = default!;

            {
                var err = check(name, "#cgo " + name, fromPackage);

                if (err != null)
                {
                    return (null, error.As(err)!);
                }

            }

            return (str.StringList(envList("CGO_" + name, defaults), fromPackage), error.As(null!)!);

        }

        private static var cgoRe = lazyregexp.New("[/\\\\:]");

        private static (slice<@string>, slice<@string>, error) cgo(this ptr<Builder> _addr_b, ptr<Action> _addr_a, @string cgoExe, @string objdir, slice<@string> pcCFLAGS, slice<@string> pcLDFLAGS, slice<@string> cgofiles, slice<@string> gccfiles, slice<@string> gxxfiles, slice<@string> mfiles, slice<@string> ffiles)
        {
            slice<@string> outGo = default;
            slice<@string> outObj = default;
            error err = default!;
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;

            var p = a.Package;
            var (cgoCPPFLAGS, cgoCFLAGS, cgoCXXFLAGS, cgoFFLAGS, cgoLDFLAGS, err) = b.CFlags(p);
            if (err != null)
            {
                return (null, null, error.As(err)!);
            }

            cgoCPPFLAGS = append(cgoCPPFLAGS, pcCFLAGS);
            cgoLDFLAGS = append(cgoLDFLAGS, pcLDFLAGS); 
            // If we are compiling Objective-C code, then we need to link against libobjc
            if (len(mfiles) > 0L)
            {
                cgoLDFLAGS = append(cgoLDFLAGS, "-lobjc");
            } 

            // Likewise for Fortran, except there are many Fortran compilers.
            // Support gfortran out of the box and let others pass the correct link options
            // via CGO_LDFLAGS
            if (len(ffiles) > 0L)
            {
                var fc = cfg.Getenv("FC");
                if (fc == "")
                {
                    fc = "gfortran";
                }

                if (strings.Contains(fc, "gfortran"))
                {
                    cgoLDFLAGS = append(cgoLDFLAGS, "-lgfortran");
                }

            }

            if (cfg.BuildMSan)
            {
                cgoCFLAGS = append(new slice<@string>(new @string[] { "-fsanitize=memory" }), cgoCFLAGS);
                cgoLDFLAGS = append(new slice<@string>(new @string[] { "-fsanitize=memory" }), cgoLDFLAGS);
            } 

            // Allows including _cgo_export.h from .[ch] files in the package.
            cgoCPPFLAGS = append(cgoCPPFLAGS, "-I", objdir); 

            // cgo
            // TODO: CGO_FLAGS?
            @string gofiles = new slice<@string>(new @string[] { objdir+"_cgo_gotypes.go" });
            @string cfiles = new slice<@string>(new @string[] { "_cgo_export.c" });
            foreach (var (_, fn) in cgofiles)
            {
                var f = strings.TrimSuffix(filepath.Base(fn), ".go");
                gofiles = append(gofiles, objdir + f + ".cgo1.go");
                cfiles = append(cfiles, f + ".cgo2.c");
            } 

            // TODO: make cgo not depend on $GOARCH?
            @string cgoflags = new slice<@string>(new @string[] {  });
            if (p.Standard && p.ImportPath == "runtime/cgo")
            {
                cgoflags = append(cgoflags, "-import_runtime_cgo=false");
            }

            if (p.Standard && (p.ImportPath == "runtime/race" || p.ImportPath == "runtime/msan" || p.ImportPath == "runtime/cgo"))
            {
                cgoflags = append(cgoflags, "-import_syscall=false");
            } 

            // Update $CGO_LDFLAGS with p.CgoLDFLAGS.
            // These flags are recorded in the generated _cgo_gotypes.go file
            // using //go:cgo_ldflag directives, the compiler records them in the
            // object file for the package, and then the Go linker passes them
            // along to the host linker. At this point in the code, cgoLDFLAGS
            // consists of the original $CGO_LDFLAGS (unchecked) and all the
            // flags put together from source code (checked).
            var cgoenv = b.cCompilerEnv();
            if (len(cgoLDFLAGS) > 0L)
            {
                var flags = make_slice<@string>(len(cgoLDFLAGS));
                {
                    var f__prev1 = f;

                    foreach (var (__i, __f) in cgoLDFLAGS)
                    {
                        i = __i;
                        f = __f;
                        flags[i] = strconv.Quote(f);
                    }

                    f = f__prev1;
                }

                cgoenv = new slice<@string>(new @string[] { "CGO_LDFLAGS="+strings.Join(flags," ") });

            }

            if (cfg.BuildToolchainName == "gccgo")
            {
                if (b.gccSupportsFlag(new slice<@string>(new @string[] { BuildToolchain.compiler() }), "-fsplit-stack"))
                {
                    cgoCFLAGS = append(cgoCFLAGS, "-fsplit-stack");
                }

                cgoflags = append(cgoflags, "-gccgo");
                {
                    var pkgpath = gccgoPkgpath(p);

                    if (pkgpath != "")
                    {
                        cgoflags = append(cgoflags, "-gccgopkgpath=" + pkgpath);
                    }

                }

            }

            switch (cfg.BuildBuildmode)
            {
                case "c-archive": 
                    // Tell cgo that if there are any exported functions
                    // it should generate a header file that C code can
                    // #include.

                case "c-shared": 
                    // Tell cgo that if there are any exported functions
                    // it should generate a header file that C code can
                    // #include.
                    cgoflags = append(cgoflags, "-exportheader=" + objdir + "_cgo_install.h");
                    break;
            }

            {
                var err__prev1 = err;

                var err = b.run(a, p.Dir, p.ImportPath, cgoenv, cfg.BuildToolexec, cgoExe, "-objdir", objdir, "-importpath", p.ImportPath, cgoflags, "--", cgoCPPFLAGS, cgoCFLAGS, cgofiles);

                if (err != null)
                {
                    return (null, null, error.As(err)!);
                }

                err = err__prev1;

            }

            outGo = append(outGo, gofiles); 

            // Use sequential object file names to keep them distinct
            // and short enough to fit in the .a header file name slots.
            // We no longer collect them all into _all.o, and we'd like
            // tools to see both the .o suffix and unique names, so
            // we need to make them short enough not to be truncated
            // in the final archive.
            long oseq = 0L;
            Func<@string> nextOfile = () =>
            {
                oseq++;
                return objdir + fmt.Sprintf("_x%03d.o", oseq);
            } 

            // gcc
; 

            // gcc
            var cflags = str.StringList(cgoCPPFLAGS, cgoCFLAGS);
            foreach (var (_, cfile) in cfiles)
            {
                var ofile = nextOfile();
                {
                    var err__prev1 = err;

                    err = b.gcc(a, p, a.Objdir, ofile, cflags, objdir + cfile);

                    if (err != null)
                    {
                        return (null, null, error.As(err)!);
                    }

                    err = err__prev1;

                }

                outObj = append(outObj, ofile);

            }
            {
                var file__prev1 = file;

                foreach (var (_, __file) in gccfiles)
                {
                    file = __file;
                    ofile = nextOfile();
                    {
                        var err__prev1 = err;

                        err = b.gcc(a, p, a.Objdir, ofile, cflags, file);

                        if (err != null)
                        {
                            return (null, null, error.As(err)!);
                        }

                        err = err__prev1;

                    }

                    outObj = append(outObj, ofile);

                }

                file = file__prev1;
            }

            var cxxflags = str.StringList(cgoCPPFLAGS, cgoCXXFLAGS);
            {
                var file__prev1 = file;

                foreach (var (_, __file) in gxxfiles)
                {
                    file = __file;
                    ofile = nextOfile();
                    {
                        var err__prev1 = err;

                        err = b.gxx(a, p, a.Objdir, ofile, cxxflags, file);

                        if (err != null)
                        {
                            return (null, null, error.As(err)!);
                        }

                        err = err__prev1;

                    }

                    outObj = append(outObj, ofile);

                }

                file = file__prev1;
            }

            {
                var file__prev1 = file;

                foreach (var (_, __file) in mfiles)
                {
                    file = __file;
                    ofile = nextOfile();
                    {
                        var err__prev1 = err;

                        err = b.gcc(a, p, a.Objdir, ofile, cflags, file);

                        if (err != null)
                        {
                            return (null, null, error.As(err)!);
                        }

                        err = err__prev1;

                    }

                    outObj = append(outObj, ofile);

                }

                file = file__prev1;
            }

            var fflags = str.StringList(cgoCPPFLAGS, cgoFFLAGS);
            {
                var file__prev1 = file;

                foreach (var (_, __file) in ffiles)
                {
                    file = __file;
                    ofile = nextOfile();
                    {
                        var err__prev1 = err;

                        err = b.gfortran(a, p, a.Objdir, ofile, fflags, file);

                        if (err != null)
                        {
                            return (null, null, error.As(err)!);
                        }

                        err = err__prev1;

                    }

                    outObj = append(outObj, ofile);

                }

                file = file__prev1;
            }

            switch (cfg.BuildToolchainName)
            {
                case "gc": 
                    var importGo = objdir + "_cgo_import.go";
                    {
                        var err__prev1 = err;

                        err = b.dynimport(a, p, objdir, importGo, cgoExe, cflags, cgoLDFLAGS, outObj);

                        if (err != null)
                        {
                            return (null, null, error.As(err)!);
                        }

                        err = err__prev1;

                    }

                    outGo = append(outGo, importGo);
                    break;
                case "gccgo": 
                    var defunC = objdir + "_cgo_defun.c";
                    var defunObj = objdir + "_cgo_defun.o";
                    {
                        var err__prev1 = err;

                        err = BuildToolchain.cc(b, a, defunObj, defunC);

                        if (err != null)
                        {
                            return (null, null, error.As(err)!);
                        }

                        err = err__prev1;

                    }

                    outObj = append(outObj, defunObj);
                    break;
                default: 
                    noCompiler();
                    break;
            }

            return (outGo, outObj, error.As(null!)!);

        }

        // dynimport creates a Go source file named importGo containing
        // //go:cgo_import_dynamic directives for each symbol or library
        // dynamically imported by the object files outObj.
        private static error dynimport(this ptr<Builder> _addr_b, ptr<Action> _addr_a, ptr<load.Package> _addr_p, @string objdir, @string importGo, @string cgoExe, slice<@string> cflags, slice<@string> cgoLDFLAGS, slice<@string> outObj)
        {
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;
            ref load.Package p = ref _addr_p.val;

            var cfile = objdir + "_cgo_main.c";
            var ofile = objdir + "_cgo_main.o";
            {
                var err__prev1 = err;

                var err = b.gcc(a, p, objdir, ofile, cflags, cfile);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }


            var linkobj = str.StringList(ofile, outObj, p.SysoFiles);
            var dynobj = objdir + "_cgo_.o"; 

            // we need to use -pie for Linux/ARM to get accurate imported sym
            var ldflags = cgoLDFLAGS;
            if ((cfg.Goarch == "arm" && cfg.Goos == "linux") || cfg.Goos == "android")
            { 
                // -static -pie doesn't make sense, and causes link errors.
                // Issue 26197.
                var n = make_slice<@string>(0L, len(ldflags));
                foreach (var (_, flag) in ldflags)
                {
                    if (flag != "-static")
                    {
                        n = append(n, flag);
                    }

                }
                ldflags = append(n, "-pie");

            }

            {
                var err__prev1 = err;

                err = b.gccld(a, p, objdir, dynobj, ldflags, linkobj);

                if (err != null)
                {
                    return error.As(err)!;
                } 

                // cgo -dynimport

                err = err__prev1;

            } 

            // cgo -dynimport
            slice<@string> cgoflags = default;
            if (p.Standard && p.ImportPath == "runtime/cgo")
            {
                cgoflags = new slice<@string>(new @string[] { "-dynlinker" }); // record path to dynamic linker
            }

            return error.As(b.run(a, p.Dir, p.ImportPath, b.cCompilerEnv(), cfg.BuildToolexec, cgoExe, "-dynpackage", p.Name, "-dynimport", dynobj, "-dynout", importGo, cgoflags))!;

        }

        // Run SWIG on all SWIG input files.
        // TODO: Don't build a shared library, once SWIG emits the necessary
        // pragmas for external linking.
        private static (slice<@string>, slice<@string>, slice<@string>, error) swig(this ptr<Builder> _addr_b, ptr<Action> _addr_a, ptr<load.Package> _addr_p, @string objdir, slice<@string> pcCFLAGS)
        {
            slice<@string> outGo = default;
            slice<@string> outC = default;
            slice<@string> outCXX = default;
            error err = default!;
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;
            ref load.Package p = ref _addr_p.val;

            {
                var err = b.swigVersionCheck();

                if (err != null)
                {
                    return (null, null, null, error.As(err)!);
                }

            }


            var (intgosize, err) = b.swigIntSize(objdir);
            if (err != null)
            {
                return (null, null, null, error.As(err)!);
            }

            {
                var f__prev1 = f;

                foreach (var (_, __f) in p.SwigFiles)
                {
                    f = __f;
                    var (goFile, cFile, err) = b.swigOne(a, p, f, objdir, pcCFLAGS, false, intgosize);
                    if (err != null)
                    {
                        return (null, null, null, error.As(err)!);
                    }

                    if (goFile != "")
                    {
                        outGo = append(outGo, goFile);
                    }

                    if (cFile != "")
                    {
                        outC = append(outC, cFile);
                    }

                }

                f = f__prev1;
            }

            {
                var f__prev1 = f;

                foreach (var (_, __f) in p.SwigCXXFiles)
                {
                    f = __f;
                    var (goFile, cxxFile, err) = b.swigOne(a, p, f, objdir, pcCFLAGS, true, intgosize);
                    if (err != null)
                    {
                        return (null, null, null, error.As(err)!);
                    }

                    if (goFile != "")
                    {
                        outGo = append(outGo, goFile);
                    }

                    if (cxxFile != "")
                    {
                        outCXX = append(outCXX, cxxFile);
                    }

                }

                f = f__prev1;
            }

            return (outGo, outC, outCXX, error.As(null!)!);

        }

        // Make sure SWIG is new enough.
        private static sync.Once swigCheckOnce = default;        private static error swigCheck = default!;

        private static error swigDoVersionCheck(this ptr<Builder> _addr_b)
        {
            ref Builder b = ref _addr_b.val;

            var (out, err) = b.runOut(null, "", null, "swig", "-version");
            if (err != null)
            {
                return error.As(err)!;
            }

            var re = regexp.MustCompile("[vV]ersion +([\\d]+)([.][\\d]+)?([.][\\d]+)?");
            var matches = re.FindSubmatch(out);
            if (matches == null)
            { 
                // Can't find version number; hope for the best.
                return error.As(null!)!;

            }

            var (major, err) = strconv.Atoi(string(matches[1L]));
            if (err != null)
            { 
                // Can't find version number; hope for the best.
                return error.As(null!)!;

            }

            const @string errmsg = (@string)"must have SWIG version >= 3.0.6";

            if (major < 3L)
            {
                return error.As(errors.New(errmsg))!;
            }

            if (major > 3L)
            { 
                // 4.0 or later
                return error.As(null!)!;

            } 

            // We have SWIG version 3.x.
            if (len(matches[2L]) > 0L)
            {
                var (minor, err) = strconv.Atoi(string(matches[2L][1L..]));
                if (err != null)
                {
                    return error.As(null!)!;
                }

                if (minor > 0L)
                { 
                    // 3.1 or later
                    return error.As(null!)!;

                }

            } 

            // We have SWIG version 3.0.x.
            if (len(matches[3L]) > 0L)
            {
                var (patch, err) = strconv.Atoi(string(matches[3L][1L..]));
                if (err != null)
                {
                    return error.As(null!)!;
                }

                if (patch < 6L)
                { 
                    // Before 3.0.6.
                    return error.As(errors.New(errmsg))!;

                }

            }

            return error.As(null!)!;

        }

        private static error swigVersionCheck(this ptr<Builder> _addr_b)
        {
            ref Builder b = ref _addr_b.val;

            swigCheckOnce.Do(() =>
            {
                swigCheck = b.swigDoVersionCheck();
            });
            return error.As(swigCheck)!;

        }

        // Find the value to pass for the -intgosize option to swig.
        private static sync.Once swigIntSizeOnce = default;        private static @string swigIntSize = default;        private static error swigIntSizeError = default!;

        // This code fails to build if sizeof(int) <= 32
        private static readonly @string swigIntSizeCode = (@string)"\npackage main\nconst i int = 1 << 32\n";

        // Determine the size of int on the target system for the -intgosize option
        // of swig >= 2.0.9. Run only once.


        // Determine the size of int on the target system for the -intgosize option
        // of swig >= 2.0.9. Run only once.
        private static (@string, error) swigDoIntSize(this ptr<Builder> _addr_b, @string objdir)
        {
            @string intsize = default;
            error err = default!;
            ref Builder b = ref _addr_b.val;

            if (cfg.BuildN)
            {
                return ("$INTBITS", error.As(null!)!);
            }

            var src = filepath.Join(b.WorkDir, "swig_intsize.go");
            err = ioutil.WriteFile(src, (slice<byte>)swigIntSizeCode, 0666L);

            if (err != null)
            {
                return ;
            }

            @string srcs = new slice<@string>(new @string[] { src });

            var p = load.GoFilesPackage(srcs);

            {
                var (_, _, e) = BuildToolchain.gc(b, addr(new Action(Mode:"swigDoIntSize",Package:p,Objdir:objdir)), "", null, "", false, srcs);

                if (e != null)
                {
                    return ("32", error.As(null!)!);
                }

            }

            return ("64", error.As(null!)!);

        }

        // Determine the size of int on the target system for the -intgosize option
        // of swig >= 2.0.9.
        private static (@string, error) swigIntSize(this ptr<Builder> _addr_b, @string objdir)
        {
            @string intsize = default;
            error err = default!;
            ref Builder b = ref _addr_b.val;

            swigIntSizeOnce.Do(() =>
            {
                swigIntSize, swigIntSizeError = b.swigDoIntSize(objdir);
            });
            return (swigIntSize, error.As(swigIntSizeError)!);

        }

        // Run SWIG on one SWIG input file.
        private static (@string, @string, error) swigOne(this ptr<Builder> _addr_b, ptr<Action> _addr_a, ptr<load.Package> _addr_p, @string file, @string objdir, slice<@string> pcCFLAGS, bool cxx, @string intgosize)
        {
            @string outGo = default;
            @string outC = default;
            error err = default!;
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;
            ref load.Package p = ref _addr_p.val;

            var (cgoCPPFLAGS, cgoCFLAGS, cgoCXXFLAGS, _, _, err) = b.CFlags(p);
            if (err != null)
            {
                return ("", "", error.As(err)!);
            }

            slice<@string> cflags = default;
            if (cxx)
            {
                cflags = str.StringList(cgoCPPFLAGS, pcCFLAGS, cgoCXXFLAGS);
            }
            else
            {
                cflags = str.StringList(cgoCPPFLAGS, pcCFLAGS, cgoCFLAGS);
            }

            long n = 5L; // length of ".swig"
            if (cxx)
            {
                n = 8L; // length of ".swigcxx"
            }

            var @base = file[..len(file) - n];
            var goFile = base + ".go";
            var gccBase = base + "_wrap.";
            @string gccExt = "c";
            if (cxx)
            {
                gccExt = "cxx";
            }

            var gccgo = cfg.BuildToolchainName == "gccgo"; 

            // swig
            @string args = new slice<@string>(new @string[] { "-go", "-cgo", "-intgosize", intgosize, "-module", base, "-o", objdir+gccBase+gccExt, "-outdir", objdir });

            foreach (var (_, f) in cflags)
            {
                if (len(f) > 3L && f[..2L] == "-I")
                {
                    args = append(args, f);
                }

            }
            if (gccgo)
            {
                args = append(args, "-gccgo");
                {
                    var pkgpath = gccgoPkgpath(p);

                    if (pkgpath != "")
                    {
                        args = append(args, "-go-pkgpath", pkgpath);
                    }

                }

            }

            if (cxx)
            {
                args = append(args, "-c++");
            }

            var (out, err) = b.runOut(a, p.Dir, null, "swig", args, file);
            if (err != null)
            {
                if (len(out) > 0L)
                {
                    if (bytes.Contains(out, (slice<byte>)"-intgosize") || bytes.Contains(out, (slice<byte>)"-cgo"))
                    {
                        return ("", "", error.As(errors.New("must have SWIG version >= 3.0.6"))!);
                    }

                    b.showOutput(a, p.Dir, p.Desc(), b.processOutput(out)); // swig error
                    return ("", "", error.As(errPrintedOutput)!);

                }

                return ("", "", error.As(err)!);

            }

            if (len(out) > 0L)
            {
                b.showOutput(a, p.Dir, p.Desc(), b.processOutput(out)); // swig warning
            } 

            // If the input was x.swig, the output is x.go in the objdir.
            // But there might be an x.go in the original dir too, and if it
            // uses cgo as well, cgo will be processing both and will
            // translate both into x.cgo1.go in the objdir, overwriting one.
            // Rename x.go to _x_swig.go to avoid this problem.
            // We ignore files in the original dir that begin with underscore
            // so _x_swig.go cannot conflict with an original file we were
            // going to compile.
            goFile = objdir + goFile;
            var newGoFile = objdir + "_" + base + "_swig.go";
            {
                var err = os.Rename(goFile, newGoFile);

                if (err != null)
                {
                    return ("", "", error.As(err)!);
                }

            }

            return (newGoFile, objdir + gccBase + gccExt, error.As(null!)!);

        }

        // disableBuildID adjusts a linker command line to avoid creating a
        // build ID when creating an object file rather than an executable or
        // shared library. Some systems, such as Ubuntu, always add
        // --build-id to every link, but we don't want a build ID when we are
        // producing an object file. On some of those system a plain -r (not
        // -Wl,-r) will turn off --build-id, but clang 3.0 doesn't support a
        // plain -r. I don't know how to turn off --build-id when using clang
        // other than passing a trailing --build-id=none. So that is what we
        // do, but only on systems likely to support it, which is to say,
        // systems that normally use gold or the GNU linker.
        private static slice<@string> disableBuildID(this ptr<Builder> _addr_b, slice<@string> ldflags)
        {
            ref Builder b = ref _addr_b.val;

            switch (cfg.Goos)
            {
                case "android": 

                case "dragonfly": 

                case "linux": 

                case "netbsd": 
                    ldflags = append(ldflags, "-Wl,--build-id=none");
                    break;
            }
            return ldflags;

        }

        // mkAbsFiles converts files into a list of absolute files,
        // assuming they were originally relative to dir,
        // and returns that new list.
        private static slice<@string> mkAbsFiles(@string dir, slice<@string> files)
        {
            var abs = make_slice<@string>(len(files));
            foreach (var (i, f) in files)
            {
                if (!filepath.IsAbs(f))
                {
                    f = filepath.Join(dir, f);
                }

                abs[i] = f;

            }
            return abs;

        }

        // passLongArgsInResponseFiles modifies cmd such that, for
        // certain programs, long arguments are passed in "response files", a
        // file on disk with the arguments, with one arg per line. An actual
        // argument starting with '@' means that the rest of the argument is
        // a filename of arguments to expand.
        //
        // See issues 18468 (Windows) and 37768 (Darwin).
        private static Action passLongArgsInResponseFiles(ptr<exec.Cmd> _addr_cmd)
        {
            Action cleanup = default;
            ref exec.Cmd cmd = ref _addr_cmd.val;

            cleanup = () =>
            {
            } // no cleanup by default
; // no cleanup by default

            long argLen = default;
            {
                var arg__prev1 = arg;

                foreach (var (_, __arg) in cmd.Args)
                {
                    arg = __arg;
                    argLen += len(arg);
                } 

                // If we're not approaching 32KB of args, just pass args normally.
                // (use 30KB instead to be conservative; not sure how accounting is done)

                arg = arg__prev1;
            }

            if (!useResponseFile(cmd.Path, argLen))
            {
                return ;
            }

            var (tf, err) = ioutil.TempFile("", "args");
            if (err != null)
            {
                log.Fatalf("error writing long arguments to response file: %v", err);
            }

            cleanup = () =>
            {
                os.Remove(tf.Name());
            }
;
            ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
            {
                var arg__prev1 = arg;

                foreach (var (_, __arg) in cmd.Args[1L..])
                {
                    arg = __arg;
                    fmt.Fprintf(_addr_buf, "%s\n", arg);
                }

                arg = arg__prev1;
            }

            {
                var (_, err) = tf.Write(buf.Bytes());

                if (err != null)
                {
                    tf.Close();
                    cleanup();
                    log.Fatalf("error writing long arguments to response file: %v", err);
                }

            }

            {
                var err = tf.Close();

                if (err != null)
                {
                    cleanup();
                    log.Fatalf("error writing long arguments to response file: %v", err);
                }

            }

            cmd.Args = new slice<@string>(new @string[] { cmd.Args[0], "@"+tf.Name() });
            return cleanup;

        }

        private static bool useResponseFile(@string path, long argLen)
        { 
            // Unless the program uses objabi.Flagparse, which understands
            // response files, don't use response files.
            // TODO: do we need more commands? asm? cgo? For now, no.
            var prog = strings.TrimSuffix(filepath.Base(path), ".exe");
            switch (prog)
            {
                case "compile": 

                case "link": 
                    break;
                default: 
                    return false;
                    break;
            } 

            // Windows has a limit of 32 KB arguments. To be conservative and not
            // worry about whether that includes spaces or not, just use 30 KB.
            // Darwin's limit is less clear. The OS claims 256KB, but we've seen
            // failures with arglen as small as 50KB.
            if (argLen > (30L << (int)(10L)))
            {
                return true;
            } 

            // On the Go build system, use response files about 10% of the
            // time, just to exercise this codepath.
            var isBuilder = os.Getenv("GO_BUILDER_NAME") != "";
            if (isBuilder && rand.Intn(10L) == 0L)
            {
                return true;
            }

            return false;

        }
    }
}}}}
