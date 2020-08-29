// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Action graph execution.

// package work -- go2cs converted at 2020 August 29 10:01:27 UTC
// import "cmd/go/internal/work" ==> using work = go.cmd.go.@internal.work_package
// Original source: C:\Go\src\cmd\go\internal\work\exec.go
using bytes = go.bytes_package;
using json = go.encoding.json_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using log = go.log_package;
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
        private static slice<ref Action> actionList(ref Action root)
        {
            map seen = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ref Action, bool>{};
            ref Action all = new slice<ref Action>(new ref Action[] {  });
            Action<ref Action> walk = default;
            walk = a =>
            {
                if (seen[a])
                {
                    return;
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
        private static void Do(this ref Builder _b, ref Action _root) => func(_b, _root, (ref Builder b, ref Action root, Defer defer, Panic _, Recover __) =>
        {
            {
                var c = cache.Default();

                if (c != null && !b.ComputeStaleOnly)
                { 
                    // If we're doing real work, take time at the end to trim the cache.
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
            var all = actionList(root);
            {
                var i__prev1 = i;
                var a__prev1 = a;

                foreach (var (__i, __a) in all)
                {
                    i = __i;
                    a = __a;
                    a.priority = i;
                }

                i = i__prev1;
                a = a__prev1;
            }

            if (cfg.DebugActiongraph != "")
            {
                var js = actionGraphJSON(root);
                {
                    var err__prev2 = err;

                    var err = ioutil.WriteFile(cfg.DebugActiongraph, (slice<byte>)js, 0666L);

                    if (err != null)
                    {
                        fmt.Fprintf(os.Stderr, "go: writing action graph: %v\n", err);
                        @base.SetExitStatus(1L);
                    }

                    err = err__prev2;

                }
            }
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

            Action<ref Action> handle = a =>
            {
                err = default;

                if (a.Func != null && (!a.Failed || a.IgnoreFail))
                {
                    if (err == null)
                    {
                        err = a.Func(b, a);
                    }
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
                                return;
                            } 
                            // Receiving a value from b.readySema entitles
                            // us to take from the ready queue.
                            b.exec.Lock();
                            var a = b.ready.pop();
                            b.exec.Unlock();
                            handle(a);
                            @base.SetExitStatus(1L);
                            return;
                        }

                    }());
                }


                i = i__prev1;
            }

            wg.Wait();
        });

        // buildActionID computes the action ID for a build action.
        private static cache.ActionID buildActionID(this ref Builder b, ref Action a)
        {
            var p = a.Package;
            var h = cache.NewHash("build " + p.ImportPath); 

            // Configuration independent of compiler toolchain.
            // Note: buildmode has already been accounted for in buildGcflags
            // and should not be inserted explicitly. Most buildmodes use the
            // same compiler settings and can reuse each other's results.
            // If not, the reason is already recorded in buildGcflags.
            fmt.Fprintf(h, "compile\n"); 
            // The compiler hides the exact value of $GOROOT
            // when building things in GOROOT,
            // but it does not hide the exact value of $GOPATH.
            // Include the full dir in that case.
            // Assume b.WorkDir is being trimmed properly.
            if (!p.Goroot && !strings.HasPrefix(p.Dir, b.WorkDir))
            {
                fmt.Fprintf(h, "dir %s\n", p.Dir);
            }
            fmt.Fprintf(h, "goos %s goarch %s\n", cfg.Goos, cfg.Goarch);
            fmt.Fprintf(h, "import %q\n", p.ImportPath);
            fmt.Fprintf(h, "omitdebug %v standard %v local %v prefix %q\n", p.Internal.OmitDebug, p.Standard, p.Internal.Local, p.Internal.LocalPrefix);
            if (len(p.CgoFiles) + len(p.SwigFiles) > 0L)
            {
                fmt.Fprintf(h, "cgo %q\n", b.toolID("cgo"));
                var (cppflags, cflags, cxxflags, fflags, _, _) = b.CFlags(p);
                fmt.Fprintf(h, "CC=%q %q %q\n", b.ccExe(), cppflags, cflags);
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

            // Configuration specific to compiler toolchain.
            switch (cfg.BuildToolchainName)
            {
                case "gc": 
                    fmt.Fprintf(h, "compile %s %q %q\n", b.toolID("compile"), forcedGcflags, p.Internal.Gcflags);
                    if (len(p.SFiles) > 0L)
                    {
                        fmt.Fprintf(h, "asm %q %q %q\n", b.toolID("asm"), forcedAsmflags, p.Internal.Asmflags);
                    }
                    fmt.Fprintf(h, "GO$GOARCH=%s\n", os.Getenv("GO" + strings.ToUpper(cfg.BuildContext.GOARCH))); // GO386, GOARM, etc

                    // TODO(rsc): Convince compiler team not to add more magic environment variables,
                    // or perhaps restrict the environment variables passed to subprocesses.
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
                    if (len(p.SFiles) > 0L)
                    {
                        id, err = b.gccgoToolID(BuildToolchain.compiler(), "assembler-with-cpp"); 
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

        // build is the action for building a single package.
        // Note that any new influence on this logic must be reported in b.buildActionID above as well.
        private static error build(this ref Builder _b, ref Action _a) => func(_b, _a, (ref Builder b, ref Action a, Defer defer, Panic _, Recover __) =>
        {
            var p = a.Package;
            var cached = false;
            if (!p.BinaryOnly)
            {
                if (b.useCache(a, p, b.buildActionID(a), p.Target))
                { 
                    // If this build triggers a header install, run cgo to get the header.
                    // TODO(rsc): Once we can cache multiple file outputs from an action,
                    // the header should be cached, and then this awful test can be deleted.
                    // Need to look for install header actions depending on this action,
                    // or depending on a link that depends on this action.
                    var needHeader = false;
                    if ((a.Package.UsesCgo() || a.Package.UsesSwig()) && (cfg.BuildBuildmode == "c-archive" || cfg.BuildBuildmode == "c-shared"))
                    {
                        {
                            var t1__prev1 = t1;

                            foreach (var (_, __t1) in a.triggers)
                            {
                                t1 = __t1;
                                if (t1.Mode == "install header")
                                {
                                    needHeader = true;
                                    goto CheckedHeader;
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
                                        needHeader = true;
                                        goto CheckedHeader;
                                    }
                                }
                            }

                            t1 = t1__prev1;
                        }

                    }
CheckedHeader:
                    if (b.ComputeStaleOnly || !a.needVet && !needHeader)
                    {
                        return error.As(null);
                    }
                    cached = true;
                }
                defer(b.flushOutput(a));
            }
            defer(() =>
            {
                if (err != null && err != errPrintedOutput)
                {
                    err = fmt.Errorf("go build %s: %v", a.Package.ImportPath, err);
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
                var (_, err) = os.Stat(a.Package.Target);
                if (err == null)
                {
                    a.built = a.Package.Target;
                    a.Target = a.Package.Target;
                    a.buildID = b.fileHash(a.Package.Target);
                    a.Package.Stale = false;
                    a.Package.StaleReason = "binary-only package";
                    return error.As(null);
                }
                if (b.ComputeStaleOnly)
                {
                    a.Package.Stale = true;
                    a.Package.StaleReason = "missing or invalid binary-only package";
                    return error.As(null);
                }
                return error.As(fmt.Errorf("missing or invalid binary-only package"));
            }
            {
                var err__prev1 = err;

                var err = b.Mkdir(a.Objdir);

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }
            var objdir = a.Objdir; 

            // make target directory
            var (dir, _) = filepath.Split(a.Target);
            if (dir != "")
            {
                {
                    var err__prev2 = err;

                    err = b.Mkdir(dir);

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev2;

                }
            }
            slice<@string> gofiles = default;            slice<@string> cgofiles = default;            slice<@string> cfiles = default;            slice<@string> sfiles = default;            slice<@string> cxxfiles = default;            slice<@string> objects = default;            slice<@string> cgoObjects = default;            slice<@string> pcCFLAGS = default;            slice<@string> pcLDFLAGS = default;



            gofiles = append(gofiles, a.Package.GoFiles);
            cgofiles = append(cgofiles, a.Package.CgoFiles);
            cfiles = append(cfiles, a.Package.CFiles);
            sfiles = append(sfiles, a.Package.SFiles);
            cxxfiles = append(cxxfiles, a.Package.CXXFiles);

            if (a.Package.UsesCgo() || a.Package.UsesSwig())
            {
                pcCFLAGS, pcLDFLAGS, err = b.getPkgConfigFlags(a.Package);

                if (err != null)
                {
                    return;
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
                    return error.As(err);
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

                            err = b.cover(a, coverFile, sourceFile, 0666L, cover.Var);

                            if (err != null)
                            {
                                return error.As(err);
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
                        return (nongcc, gcc);
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
                                return error.As(fmt.Errorf("package using cgo has Go assembly file %s", sfile));
                            }
                        }
                    }
                    gccfiles = append(gccfiles, sfiles);
                    sfiles = null;
                }
                var (outGo, outObj, err) = b.cgo(a, @base.Tool("cgo"), objdir, pcCFLAGS, pcLDFLAGS, mkAbsFiles(a.Package.Dir, cgofiles), gccfiles, cxxfiles, a.Package.MFiles, a.Package.FFiles);
                if (err != null)
                {
                    return error.As(err);
                }
                if (cfg.BuildToolchainName == "gccgo")
                {
                    cgoObjects = append(cgoObjects, a.Objdir + "_cgo_flags");
                }
                cgoObjects = append(cgoObjects, outObj);
                gofiles = append(gofiles, outGo);
            }
            if (cached && !a.needVet)
            {
                return error.As(null);
            } 

            // Sanity check only, since Package.load already checked as well.
            if (len(gofiles) == 0L)
            {
                return error.As(ref new load.NoGoError(Package:a.Package));
            } 

            // Prepare Go vet config if needed.
            ref vetConfig vcfg = default;
            if (a.needVet)
            { 
                // Pass list of absolute paths to vet,
                // so that vet's error messages will use absolute paths,
                // so that we can reformat them relative to the directory
                // in which the go command is invoked.
                vcfg = ref new vetConfig(Compiler:cfg.BuildToolchainName,Dir:a.Package.Dir,GoFiles:mkAbsFiles(a.Package.Dir,gofiles),ImportPath:a.Package.ImportPath,ImportMap:make(map[string]string),PackageFile:make(map[string]string),);
                a.vetCfg = vcfg;
                {
                    var i__prev1 = i;
                    var raw__prev1 = raw;

                    foreach (var (__i, __raw) in a.Package.Internal.RawImports)
                    {
                        i = __i;
                        raw = __raw;
                        var final = a.Package.Imports[i];
                        vcfg.ImportMap[raw] = final;
                    }

                    i = i__prev1;
                    raw = raw__prev1;
                }

            } 

            // Prepare Go import config.
            // We start it off with a comment so it can't be empty, so icfg.Bytes() below is never nil.
            // It should never be empty anyway, but there have been bugs in the past that resulted
            // in empty configs, which then unfortunately turn into "no config passed to compiler",
            // and the compiler falls back to looking in pkg itself, which mostly works,
            // except when it doesn't.
            bytes.Buffer icfg = default;
            fmt.Fprintf(ref icfg, "# import config\n");

            {
                var i__prev1 = i;
                var raw__prev1 = raw;

                foreach (var (__i, __raw) in a.Package.Internal.RawImports)
                {
                    i = __i;
                    raw = __raw;
                    final = a.Package.Imports[i];
                    if (final != raw)
                    {
                        fmt.Fprintf(ref icfg, "importmap %s=%s\n", raw, final);
                    }
                } 

                // Compute the list of mapped imports in the vet config
                // so that we can add any missing mappings below.

                i = i__prev1;
                raw = raw__prev1;
            }

            map<@string, bool> vcfgMapped = default;
            if (vcfg != null)
            {
                vcfgMapped = make_map<@string, bool>();
                {
                    var p__prev1 = p;

                    foreach (var (_, __p) in vcfg.ImportMap)
                    {
                        p = __p;
                        vcfgMapped[p] = true;
                    }

                    p = p__prev1;
                }

            }
            foreach (var (_, a1) in a.Deps)
            {
                var p1 = a1.Package;
                if (p1 == null || p1.ImportPath == "" || a1.built == "")
                {
                    continue;
                }
                fmt.Fprintf(ref icfg, "packagefile %s=%s\n", p1.ImportPath, a1.built);
                if (vcfg != null)
                { 
                    // Add import mapping if needed
                    // (for imports like "runtime/cgo" that appear only in generated code).
                    if (!vcfgMapped[p1.ImportPath])
                    {
                        vcfg.ImportMap[p1.ImportPath] = p1.ImportPath;
                    }
                    vcfg.PackageFile[p1.ImportPath] = a1.built;
                }
            }
            if (cached)
            { 
                // The cached package file is OK, so we don't need to run the compile.
                // We've only going through the motions to prepare the vet configuration,
                // which is now complete.
                return error.As(null);
            } 

            // Compile Go.
            var objpkg = objdir + "_pkg_.a";
            var (ofile, out, err) = BuildToolchain.gc(b, a, objpkg, icfg.Bytes(), len(sfiles) > 0L, gofiles);
            if (len(out) > 0L)
            {
                b.showOutput(a, a.Package.Dir, a.Package.ImportPath, b.processOutput(out));
                if (err != null)
                {
                    return error.As(errPrintedOutput);
                }
            }
            if (err != null)
            {
                return error.As(err);
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

                            err = b.copyFile(a, objdir + targ, filepath.Join(a.Package.Dir, file), 0666L, true);

                            if (err != null)
                            {
                                return error.As(err);
                            }

                            err = err__prev1;

                        }
                    else if (strings.HasSuffix(name, _goarch)) 
                        targ = file[..len(name) - len(_goarch)] + "_GOARCH." + ext;
                        {
                            var err__prev1 = err;

                            err = b.copyFile(a, objdir + targ, filepath.Join(a.Package.Dir, file), 0666L, true);

                            if (err != null)
                            {
                                return error.As(err);
                            }

                            err = err__prev1;

                        }
                    else if (strings.HasSuffix(name, _goos)) 
                        targ = file[..len(name) - len(_goos)] + "_GOOS." + ext;
                        {
                            var err__prev1 = err;

                            err = b.copyFile(a, objdir + targ, filepath.Join(a.Package.Dir, file), 0666L, true);

                            if (err != null)
                            {
                                return error.As(err);
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
                            return error.As(err);
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
                    return error.As(err);
                }
                objects = append(objects, ofiles);
            } 

            // For gccgo on ELF systems, we write the build ID as an assembler file.
            // This lets us set the the SHF_EXCLUDE flag.
            // This is read by readGccgoArchive in cmd/internal/buildid/buildid.go.
            if (a.buildID != "" && cfg.BuildToolchainName == "gccgo")
            {
                switch (cfg.Goos)
                {
                    case "android": 

                    case "dragonfly": 

                    case "freebsd": 

                    case "linux": 

                    case "netbsd": 

                    case "openbsd": 

                    case "solaris": 
                        var (asmfile, err) = b.gccgoBuildIDELFFile(a);
                        if (err != null)
                        {
                            return error.As(err);
                        }
                        (ofiles, err) = BuildToolchain.asm(b, a, new slice<@string>(new @string[] { asmfile }));
                        if (err != null)
                        {
                            return error.As(err);
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
                        return error.As(err);
                    }

                    err = err__prev2;

                }
            }
            {
                var err__prev1 = err;

                err = b.updateBuildID(a, objpkg, true);

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }

            a.built = objpkg;
            return error.As(null);
        });

        private partial struct vetConfig
        {
            public @string Compiler;
            public @string Dir;
            public slice<@string> GoFiles;
            public map<@string, @string> ImportMap;
            public map<@string, @string> PackageFile;
            public @string ImportPath;
            public bool SucceedOnTypecheckFailure;
        }

        // VetTool is the path to an alternate vet tool binary.
        // The caller is expected to set it (if needed) before executing any vet actions.
        public static @string VetTool = default;

        // VetFlags are the flags to pass to vet.
        // The caller is expected to set them before executing any vet actions.
        public static slice<@string> VetFlags = default;

        private static error vet(this ref Builder b, ref Action a)
        { 
            // a.Deps[0] is the build of the package being vetted.
            // a.Deps[1] is the build of the "fmt" package.

            var vcfg = a.Deps[0L].vetCfg;
            if (vcfg == null)
            { 
                // Vet config should only be missing if the build failed.
                if (!a.Deps[0L].Failed)
                {
                    return error.As(fmt.Errorf("vet config not found"));
                }
                return error.As(null);
            }
            if (vcfg.ImportMap["fmt"] == "")
            {
                var a1 = a.Deps[1L];
                vcfg.ImportMap["fmt"] = "fmt";
                vcfg.PackageFile["fmt"] = a1.built;
            } 

            // During go test, ignore type-checking failures during vet.
            // We only run vet if the compilation has succeeded,
            // so at least for now assume the bug is in vet.
            // We know of at least #18395.
            // TODO(rsc,gri): Try to remove this for Go 1.11.
            vcfg.SucceedOnTypecheckFailure = cfg.CmdName == "test";

            var (js, err) = json.MarshalIndent(vcfg, "", "\t");
            if (err != null)
            {
                return error.As(fmt.Errorf("internal error marshaling vet config: %v", err));
            }
            js = append(js, '\n');
            {
                var err = b.writeFile(a.Objdir + "vet.cfg", js);

                if (err != null)
                {
                    return error.As(err);
                }

            }

            slice<@string> env = default;
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
            return error.As(b.run(a, p.Dir, p.ImportPath, env, cfg.BuildToolexec, tool, VetFlags, a.Objdir + "vet.cfg"));
        }

        // linkActionID computes the action ID for a link action.
        private static cache.ActionID linkActionID(this ref Builder b, ref Action a)
        {
            var p = a.Package;
            var h = cache.NewHash("link " + p.ImportPath); 

            // Toolchain-independent configuration.
            fmt.Fprintf(h, "link\n");
            fmt.Fprintf(h, "buildmode %s goos %s goarch %s\n", cfg.BuildBuildmode, cfg.Goos, cfg.Goarch);
            fmt.Fprintf(h, "import %q\n", p.ImportPath);
            fmt.Fprintf(h, "omitdebug %v standard %v local %v prefix %q\n", p.Internal.OmitDebug, p.Standard, p.Internal.Local, p.Internal.LocalPrefix); 

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
        private static void printLinkerConfig(this ref Builder b, io.Writer h, ref load.Package p)
        {
            switch (cfg.BuildToolchainName)
            {
                case "gc": 
                    fmt.Fprintf(h, "link %s %q %s\n", b.toolID("link"), forcedLdflags, ldBuildmode);
                    if (p != null)
                    {
                        fmt.Fprintf(h, "linkflags %q\n", p.Internal.Ldflags);
                    }
                    fmt.Fprintf(h, "GO$GOARCH=%s\n", os.Getenv("GO" + strings.ToUpper(cfg.BuildContext.GOARCH))); // GO386, GOARM, etc

                    // The linker writes source file paths that say GOROOT_FINAL.
                    fmt.Fprintf(h, "GOROOT=%s\n", cfg.GOROOT_FINAL); 

                    // TODO(rsc): Convince linker team not to add more magic environment variables,
                    // or perhaps restrict the environment variables passed to subprocesses.
                    @string magic = new slice<@string>(new @string[] { "GO_EXTLINK_ENABLED" });
                    foreach (var (_, env) in magic)
                    {
                        {
                            var x = os.Getenv(env);

                            if (x != "")
                            {
                                fmt.Fprintf(h, "magic %s=%s\n", env, x);
                            }

                        }
                    } 

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
        private static error link(this ref Builder _b, ref Action _a) => func(_b, _a, (ref Builder b, ref Action a, Defer defer, Panic _, Recover __) =>
        {
            if (b.useCache(a, a.Package, b.linkActionID(a), a.Package.Target))
            {
                return error.As(null);
            }
            defer(b.flushOutput(a));

            {
                var err__prev1 = err;

                var err = b.Mkdir(a.Objdir);

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }

            var importcfg = a.Objdir + "importcfg.link";
            {
                var err__prev1 = err;

                err = b.writeLinkImportcfg(a, importcfg);

                if (err != null)
                {
                    return error.As(err);
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
                        return error.As(err);
                    }

                    err = err__prev2;

                }
            }
            {
                var err__prev1 = err;

                err = BuildToolchain.ld(b, a, a.Target, importcfg, a.Deps[0L].built);

                if (err != null)
                {
                    return error.As(err);
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
                // result into the cache.
                // Not calling updateBuildID means we also don't insert these
                // binaries into the build object cache. That's probably a net win:
                // less cache space wasted on large binaries we are not likely to
                // need again. (On the other hand it does make repeated go test slower.)

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
            // result into the cache.
            // Not calling updateBuildID means we also don't insert these
            // binaries into the build object cache. That's probably a net win:
            // less cache space wasted on large binaries we are not likely to
            // need again. (On the other hand it does make repeated go test slower.)
            {
                var err__prev1 = err;

                err = b.updateBuildID(a, a.Target, !a.Package.Internal.OmitDebug);

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }

            a.built = a.Target;
            return error.As(null);
        });

        private static error writeLinkImportcfg(this ref Builder b, ref Action a, @string file)
        { 
            // Prepare Go import cfg.
            bytes.Buffer icfg = default;
            foreach (var (_, a1) in a.Deps)
            {
                var p1 = a1.Package;
                if (p1 == null)
                {
                    continue;
                }
                fmt.Fprintf(ref icfg, "packagefile %s=%s\n", p1.ImportPath, a1.built);
                if (p1.Shlib != "")
                {
                    fmt.Fprintf(ref icfg, "packageshlib %s=%s\n", p1.ImportPath, p1.Shlib);
                }
            }
            return error.As(b.writeFile(file, icfg.Bytes()));
        }

        // PkgconfigCmd returns a pkg-config binary name
        // defaultPkgConfig is defined in zdefaultcc.go, written by cmd/dist.
        private static @string PkgconfigCmd(this ref Builder b)
        {
            return envList("PKG_CONFIG", cfg.DefaultPkgConfig)[0L];
        }

        // splitPkgConfigOutput parses the pkg-config output into a slice of
        // flags. pkg-config always uses \ to escape special characters.
        private static slice<@string> splitPkgConfigOutput(slice<byte> @out)
        {
            if (len(out) == 0L)
            {
                return null;
            }
            slice<@string> flags = default;
            var flag = make_slice<byte>(len(out));
            long r = 0L;
            long w = 0L;
            while (r < len(out))
            {

                if (out[r] == ' ' || out[r] == '\t' || out[r] == '\r' || out[r] == '\n')
                {
                    if (w > 0L)
                    {
                        flags = append(flags, string(flag[..w]));
                    }
                    w = 0L;
                    goto __switch_break0;
                }
                if (out[r] == '\\')
                {
                    r++;
                }
                // default: 
                    if (r < len(out))
                    {
                        flag[w] = out[r];
                        w++;
                    }

                __switch_break0:;
                r++;
            }

            if (w > 0L)
            {
                flags = append(flags, string(flag[..w]));
            }
            return flags;
        }

        // Calls pkg-config if needed and returns the cflags/ldflags needed to build the package.
        private static (slice<@string>, slice<@string>, error) getPkgConfigFlags(this ref Builder b, ref load.Package p)
        {
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
                            return (null, null, fmt.Errorf("invalid pkg-config package name: %s", pkg));
                        }
                    }
                    slice<byte> @out = default;
                    out, err = b.runOut(p.Dir, p.ImportPath, null, b.PkgconfigCmd(), "--cflags", pcflags, "--", pkgs);
                    if (err != null)
                    {
                        b.showOutput(null, p.Dir, b.PkgconfigCmd() + " --cflags " + strings.Join(pcflags, " ") + strings.Join(pkgs, " "), string(out));
                        b.Print(err.Error() + "\n");
                        return (null, null, errPrintedOutput);
                    }
                    if (len(out) > 0L)
                    {
                        cflags = splitPkgConfigOutput(out);
                        {
                            var err__prev3 = err;

                            var err = checkCompilerFlags("CFLAGS", "pkg-config --cflags", cflags);

                            if (err != null)
                            {
                                return (null, null, err);
                            }

                            err = err__prev3;

                        }
                    }
                    out, err = b.runOut(p.Dir, p.ImportPath, null, b.PkgconfigCmd(), "--libs", pcflags, "--", pkgs);
                    if (err != null)
                    {
                        b.showOutput(null, p.Dir, b.PkgconfigCmd() + " --libs " + strings.Join(pcflags, " ") + strings.Join(pkgs, " "), string(out));
                        b.Print(err.Error() + "\n");
                        return (null, null, errPrintedOutput);
                    }
                    if (len(out) > 0L)
                    {
                        ldflags = strings.Fields(string(out));
                        {
                            var err__prev3 = err;

                            err = checkLinkerFlags("LDFLAGS", "pkg-config --libs", ldflags);

                            if (err != null)
                            {
                                return (null, null, err);
                            }

                            err = err__prev3;

                        }
                    }
                }

            }

            return;
        }

        private static error installShlibname(this ref Builder b, ref Action a)
        { 
            // TODO: BuildN
            var a1 = a.Deps[0L];
            var err = ioutil.WriteFile(a.Target, (slice<byte>)filepath.Base(a1.Target) + "\n", 0666L);
            if (err != null)
            {
                return error.As(err);
            }
            if (cfg.BuildX)
            {
                b.Showcmd("", "echo '%s' > %s # internal", filepath.Base(a1.Target), a.Target);
            }
            return error.As(null);
        }

        private static cache.ActionID linkSharedActionID(this ref Builder b, ref Action a)
        {
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

        private static error linkShared(this ref Builder _b, ref Action _a) => func(_b, _a, (ref Builder b, ref Action a, Defer defer, Panic _, Recover __) =>
        {
            if (b.useCache(a, null, b.linkSharedActionID(a), a.Target))
            {
                return error.As(null);
            }
            defer(b.flushOutput(a));

            {
                var err__prev1 = err;

                var err = b.Mkdir(a.Objdir);

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }

            var importcfg = a.Objdir + "importcfg.link";
            {
                var err__prev1 = err;

                err = b.writeLinkImportcfg(a, importcfg);

                if (err != null)
                {
                    return error.As(err);
                } 

                // TODO(rsc): There is a missing updateBuildID here,
                // but we have to decide where to store the build ID in these files.

                err = err__prev1;

            } 

            // TODO(rsc): There is a missing updateBuildID here,
            // but we have to decide where to store the build ID in these files.
            a.built = a.Target;
            return error.As(BuildToolchain.ldShared(b, a, a.Deps[0L].Deps, a.Target, importcfg, a.Deps));
        });

        // BuildInstallFunc is the action for installing a single package or executable.
        public static error BuildInstallFunc(ref Builder _b, ref Action _a) => func(_b, _a, (ref Builder b, ref Action a, Defer defer, Panic _, Recover __) =>
        {
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

            // If we are using the eventual install target as an up-to-date
            // cached copy of the thing we built, then there's no need to
            // copy it into itself (and that would probably fail anyway).
            // In this case a1.built == a.Target because a1.built == p.Target,
            // so the built target is not in the a1.Objdir tree that b.cleanup(a1) removes.
            if (a1.built == a.Target)
            {
                a.built = a.Target;
                b.cleanup(a1); 
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
                if (!a.buggyInstall)
                {
                    var now = time.Now();
                    os.Chtimes(a.Target, now, now);
                }
                return error.As(null);
            }
            if (b.ComputeStaleOnly)
            {
                return error.As(null);
            }
            {
                var err__prev1 = err;

                var err = b.Mkdir(a.Objdir);

                if (err != null)
                {
                    return error.As(err);
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
                        return error.As(err);
                    }

                    err = err__prev2;

                }
            }
            defer(b.cleanup(a1));

            return error.As(b.moveOrCopyFile(a, a.Target, a1.built, perm, false));
        });

        // cleanup removes a's object dir to keep the amount of
        // on-disk garbage down in a large build. On an operating system
        // with aggressive buffering, cleaning incrementally like
        // this keeps the intermediate objects from hitting the disk.
        private static void cleanup(this ref Builder b, ref Action a)
        {
            if (!cfg.BuildWork)
            {
                if (cfg.BuildX)
                {
                    b.Showcmd("", "rm -r %s", a.Objdir);
                }
                os.RemoveAll(a.Objdir);
            }
        }

        // moveOrCopyFile is like 'mv src dst' or 'cp src dst'.
        private static error moveOrCopyFile(this ref Builder b, ref Action a, @string dst, @string src, os.FileMode perm, bool force)
        {
            if (cfg.BuildN)
            {
                b.Showcmd("", "mv %s %s", src, dst);
                return error.As(null);
            } 

            // If we can update the mode and rename to the dst, do it.
            // Otherwise fall back to standard copy.

            // If the source is in the build cache, we need to copy it.
            if (strings.HasPrefix(src, cache.DefaultDir()))
            {
                return error.As(b.copyFile(a, dst, src, perm, force));
            } 

            // On Windows, always copy the file, so that we respect the NTFS
            // permissions of the parent folder. https://golang.org/issue/22343.
            // What matters here is not cfg.Goos (the system we are building
            // for) but runtime.GOOS (the system we are building on).
            if (runtime.GOOS == "windows")
            {
                return error.As(b.copyFile(a, dst, src, perm, force));
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
                        return error.As(b.copyFile(a, dst, src, perm, force));
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
                            return error.As(null);
                        }

                        err = err__prev2;

                    }
                }

                err = err__prev1;

            }

            return error.As(b.copyFile(a, dst, src, perm, force));
        }

        // copyFile is like 'cp src dst'.
        private static error copyFile(this ref Builder _b, ref Action _a, @string dst, @string src, os.FileMode perm, bool force) => func(_b, _a, (ref Builder b, ref Action a, Defer defer, Panic _, Recover __) =>
        {
            if (cfg.BuildN || cfg.BuildX)
            {
                b.Showcmd("", "cp %s %s", src, dst);
                if (cfg.BuildN)
                {
                    return error.As(null);
                }
            }
            var (sf, err) = os.Open(src);
            if (err != null)
            {
                return error.As(err);
            }
            defer(sf.Close()); 

            // Be careful about removing/overwriting dst.
            // Do not remove/overwrite if dst exists and is a directory
            // or a non-object file.
            {
                var (fi, err) = os.Stat(dst);

                if (err == null)
                {
                    if (fi.IsDir())
                    {
                        return error.As(fmt.Errorf("build output %q already exists and is a directory", dst));
                    }
                    if (!force && fi.Mode().IsRegular() && !isObject(dst))
                    {
                        return error.As(fmt.Errorf("build output %q already exists and is not an object file", dst));
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
                return error.As(err);
            }
            _, err = io.Copy(df, sf);
            df.Close();
            if (err != null)
            {
                mayberemovefile(dst);
                return error.As(fmt.Errorf("copying %s to %s: %v", src, dst, err));
            }
            return error.As(null);
        });

        // writeFile writes the text to file.
        private static error writeFile(this ref Builder b, @string file, slice<byte> text)
        {
            if (cfg.BuildN || cfg.BuildX)
            {
                b.Showcmd("", "cat >%s << 'EOF' # internal\n%sEOF", file, text);
            }
            if (cfg.BuildN)
            {
                return error.As(null);
            }
            return error.As(ioutil.WriteFile(file, text, 0666L));
        }

        // Install the cgo export header file, if there is one.
        private static error installHeader(this ref Builder b, ref Action a)
        {
            var src = a.Objdir + "_cgo_install.h";
            {
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
                    return error.As(null);
                }

            }

            var (dir, _) = filepath.Split(a.Target);
            if (dir != "")
            {
                {
                    var err = b.Mkdir(dir);

                    if (err != null)
                    {
                        return error.As(err);
                    }

                }
            }
            return error.As(b.moveOrCopyFile(a, a.Target, src, 0666L, true));
        }

        // cover runs, in effect,
        //    go tool cover -mode=b.coverMode -var="varName" -o dst.go src.go
        private static error cover(this ref Builder b, ref Action a, @string dst, @string src, os.FileMode perm, @string varName)
        {
            return error.As(b.run(a, a.Objdir, "cover " + a.Package.ImportPath, null, cfg.BuildToolexec, @base.Tool("cover"), "-mode", a.Package.Internal.CoverMode, "-var", varName, "-o", dst, src));
        }

        private static slice<byte> objectMagic = new slice<slice<byte>>(new slice<byte>[] { {'!','<','a','r','c','h','>','\n'}, {'\x7F','E','L','F'}, {0xFE,0xED,0xFA,0xCE}, {0xFE,0xED,0xFA,0xCF}, {0xCE,0xFA,0xED,0xFE}, {0xCF,0xFA,0xED,0xFE}, {0x4d,0x5a,0x90,0x00,0x03,0x00}, {0x00,0x00,0x01,0xEB}, {0x00,0x00,0x8a,0x97}, {0x00,0x00,0x06,0x47} });

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
                    return;
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
        private static @string fmtcmd(this ref Builder b, @string dir, @string format, params object[] args)
        {
            var cmd = fmt.Sprintf(format, args);
            if (dir != "" && dir != "/")
            {
                cmd = strings.Replace(" " + cmd, " " + dir, " .", -1L)[1L..];
                if (b.scriptDir != dir)
                {
                    b.scriptDir = dir;
                    cmd = "cd " + dir + "\n" + cmd;
                }
            }
            if (b.WorkDir != "")
            {
                cmd = strings.Replace(cmd, b.WorkDir, "$WORK", -1L);
            }
            return cmd;
        }

        // showcmd prints the given command to standard output
        // for the implementation of -n or -x.
        private static void Showcmd(this ref Builder _b, @string dir, @string format, params object[] args) => func(_b, (ref Builder b, Defer defer, Panic _, Recover __) =>
        {
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
        private static void showOutput(this ref Builder _b, ref Action _a, @string dir, @string desc, @string @out) => func(_b, _a, (ref Builder b, ref Action a, Defer defer, Panic _, Recover __) =>
        {
            @string prefix = "# " + desc;
            @string suffix = "\n" + out;
            {
                var reldir = @base.ShortPath(dir);

                if (reldir != dir)
                {
                    suffix = strings.Replace(suffix, " " + dir, " " + reldir, -1L);
                    suffix = strings.Replace(suffix, "\n" + dir, "\n" + reldir, -1L);
                }

            }
            suffix = strings.Replace(suffix, " " + b.WorkDir, " $WORK", -1L);

            if (a != null && a.output != null)
            {
                a.output = append(a.output, prefix);
                a.output = append(a.output, suffix);
                return;
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

        private static var cgoLine = regexp.MustCompile("\\[[^\\[\\]]+\\.(cgo1|cover)\\.go:[0-9]+(:[0-9]+)?\\]");
        private static var cgoTypeSigRe = regexp.MustCompile("\\b_C2?(type|func|var|macro)_\\B");

        // run runs the command given by cmdline in the directory dir.
        // If the command fails, run prints information about the failure
        // and returns a non-nil error.
        private static error run(this ref Builder b, ref Action a, @string dir, @string desc, slice<@string> env, params object[] cmdargs)
        {
            var (out, err) = b.runOut(dir, desc, env, cmdargs);
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
            return error.As(err);
        }

        // processOutput prepares the output of runOut to be output to the console.
        private static @string processOutput(this ref Builder b, slice<byte> @out)
        {
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
        private static (slice<byte>, error) runOut(this ref Builder b, @string dir, @string desc, slice<@string> env, params object[] cmdargs)
        {
            var cmdline = str.StringList(cmdargs);

            foreach (var (_, arg) in cmdline)
            { 
                // GNU binutils commands, including gcc and gccgo, interpret an argument
                // @foo anywhere in the command line (even following --) as meaning
                // "read and insert arguments from the file named foo."
                // Don't say anything that might be misinterpreted that way.
                if (strings.HasPrefix(arg, "@"))
                {
                    return (null, fmt.Errorf("invalid command-line argument %s in command: %s", arg, joinUnambiguously(cmdline)));
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
                    return (null, null);
                }
            }
            bytes.Buffer buf = default;
            var cmd = exec.Command(cmdline[0L], cmdline[1L..]);
            cmd.Stdout = ref buf;
            cmd.Stderr = ref buf;
            cmd.Dir = dir;
            cmd.Env = @base.MergeEnvLists(env, @base.EnvForDir(cmd.Dir, os.Environ()));
            var err = cmd.Run(); 

            // err can be something like 'exit status 1'.
            // Add information about what program was running.
            // Note that if buf.Bytes() is non-empty, the caller usually
            // shows buf.Bytes() and does not print err at all, so the
            // prefix here does not make most output any more verbose.
            if (err != null)
            {
                err = errors.New(cmdline[0L] + ": " + err.Error());
            }
            return (buf.Bytes(), err);
        }

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
                if (s == "" || strings.Contains(s, " ") || len(q) > len(s) + 2L)
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

        // mkdir makes the named directory.
        private static error Mkdir(this ref Builder _b, @string dir) => func(_b, (ref Builder b, Defer defer, Panic _, Recover __) =>
        { 
            // Make Mkdir(a.Objdir) a no-op instead of an error when a.Objdir == "".
            if (dir == "")
            {
                return error.As(null);
            }
            b.exec.Lock();
            defer(b.exec.Unlock()); 
            // We can be a little aggressive about being
            // sure directories exist. Skip repeated calls.
            if (b.mkdirCache[dir])
            {
                return error.As(null);
            }
            b.mkdirCache[dir] = true;

            if (cfg.BuildN || cfg.BuildX)
            {
                b.Showcmd("", "mkdir -p %s", dir);
                if (cfg.BuildN)
                {
                    return error.As(null);
                }
            }
            {
                var err = os.MkdirAll(dir, 0777L);

                if (err != null)
                {
                    return error.As(err);
                }

            }
            return error.As(null);
        });

        // symlink creates a symlink newname -> oldname.
        private static error Symlink(this ref Builder b, @string oldname, @string newname)
        {
            if (cfg.BuildN || cfg.BuildX)
            {
                b.Showcmd("", "ln -s %s %s", oldname, newname);
                if (cfg.BuildN)
                {
                    return error.As(null);
                }
            }
            return error.As(os.Symlink(oldname, newname));
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
            @string gc(ref Builder b, ref Action a, @string archive, slice<byte> importcfg, bool asmhdr, slice<@string> gofiles); // cc runs the toolchain's C compiler in a directory on a C file
// to produce an output file.
            @string cc(ref Builder b, ref Action a, @string ofile, @string cfile); // asm runs the assembler in a specific directory on specific files
// and returns a list of named output files.
            @string asm(ref Builder b, ref Action a, slice<@string> sfiles); // pack runs the archive packer in a specific directory to create
// an archive from a set of object files.
// typically it is run in the object directory.
            @string pack(ref Builder b, ref Action a, @string afile, slice<@string> ofiles); // ld runs the linker to create an executable starting at mainpkg.
            @string ld(ref Builder b, ref Action root, @string @out, @string importcfg, @string mainpkg); // ldShared runs the linker to create a shared library containing the pkgs built by toplevelactions
            @string ldShared(ref Builder b, ref Action root, slice<ref Action> toplevelactions, @string @out, @string importcfg, slice<ref Action> allactions);
            @string compiler();
            @string linker();
        }

        private partial struct noToolchain
        {
        }

        private static error noCompiler()
        {
            log.Fatalf("unknown compiler %q", cfg.BuildContext.Compiler);
            return error.As(null);
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

        private static (@string, slice<byte>, error) gc(this noToolchain _p0, ref Builder b, ref Action a, @string archive, slice<byte> importcfg, bool asmhdr, slice<@string> gofiles)
        {
            return ("", null, noCompiler());
        }

        private static (slice<@string>, error) asm(this noToolchain _p0, ref Builder b, ref Action a, slice<@string> sfiles)
        {
            return (null, noCompiler());
        }

        private static error pack(this noToolchain _p0, ref Builder b, ref Action a, @string afile, slice<@string> ofiles)
        {
            return error.As(noCompiler());
        }

        private static error ld(this noToolchain _p0, ref Builder b, ref Action root, @string @out, @string importcfg, @string mainpkg)
        {
            return error.As(noCompiler());
        }

        private static error ldShared(this noToolchain _p0, ref Builder b, ref Action root, slice<ref Action> toplevelactions, @string @out, @string importcfg, slice<ref Action> allactions)
        {
            return error.As(noCompiler());
        }

        private static error cc(this noToolchain _p0, ref Builder b, ref Action a, @string ofile, @string cfile)
        {
            return error.As(noCompiler());
        }

        // gcc runs the gcc C compiler to create an object from a single C file.
        private static error gcc(this ref Builder b, ref Action a, ref load.Package p, @string workdir, @string @out, slice<@string> flags, @string cfile)
        {
            return error.As(b.ccompile(a, p, out, flags, cfile, b.GccCmd(p.Dir, workdir)));
        }

        // gxx runs the g++ C++ compiler to create an object from a single C++ file.
        private static error gxx(this ref Builder b, ref Action a, ref load.Package p, @string workdir, @string @out, slice<@string> flags, @string cxxfile)
        {
            return error.As(b.ccompile(a, p, out, flags, cxxfile, b.GxxCmd(p.Dir, workdir)));
        }

        // gfortran runs the gfortran Fortran compiler to create an object from a single Fortran file.
        private static error gfortran(this ref Builder b, ref Action a, ref load.Package p, @string workdir, @string @out, slice<@string> flags, @string ffile)
        {
            return error.As(b.ccompile(a, p, out, flags, ffile, b.gfortranCmd(p.Dir, workdir)));
        }

        // ccompile runs the given C or C++ compiler and creates an object from a single source file.
        private static error ccompile(this ref Builder b, ref Action a, ref load.Package p, @string outfile, slice<@string> flags, @string file, slice<@string> compiler)
        {
            file = mkAbs(p.Dir, file);
            var desc = p.ImportPath;
            if (!filepath.IsAbs(outfile))
            {
                outfile = filepath.Join(p.Dir, outfile);
            }
            var (output, err) = b.runOut(filepath.Dir(file), desc, null, compiler, flags, "-o", outfile, "-c", filepath.Base(file));
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
                        return error.As(b.ccompile(a, p, outfile, newFlags, file, compiler));
                    }
                }
                b.showOutput(a, p.Dir, desc, b.processOutput(output));
                if (err != null)
                {
                    err = errPrintedOutput;
                }
                else if (os.Getenv("GO_BUILDER_NAME") != "")
                {
                    return error.As(errors.New("C compiler warning promoted to error on Go builders"));
                }
            }
            return error.As(err);
        }

        // gccld runs the gcc linker to create an executable from a set of object files.
        private static error gccld(this ref Builder b, ref load.Package p, @string objdir, @string @out, slice<@string> flags, slice<@string> objs)
        {
            slice<@string> cmd = default;
            if (len(p.CXXFiles) > 0L || len(p.SwigCXXFiles) > 0L)
            {
                cmd = b.GxxCmd(p.Dir, objdir);
            }
            else
            {
                cmd = b.GccCmd(p.Dir, objdir);
            }
            return error.As(b.run(null, p.Dir, p.ImportPath, null, cmd, "-o", out, objs, flags));
        }

        // Grab these before main helpfully overwrites them.
        private static var origCC = os.Getenv("CC");        private static var origCXX = os.Getenv("CXX");

        // gccCmd returns a gcc command line prefix
        // defaultCC is defined in zdefaultcc.go, written by cmd/dist.
        private static slice<@string> GccCmd(this ref Builder b, @string incdir, @string workdir)
        {
            return b.compilerCmd(b.ccExe(), incdir, workdir);
        }

        // gxxCmd returns a g++ command line prefix
        // defaultCXX is defined in zdefaultcc.go, written by cmd/dist.
        private static slice<@string> GxxCmd(this ref Builder b, @string incdir, @string workdir)
        {
            return b.compilerCmd(b.cxxExe(), incdir, workdir);
        }

        // gfortranCmd returns a gfortran command line prefix.
        private static slice<@string> gfortranCmd(this ref Builder b, @string incdir, @string workdir)
        {
            return b.compilerCmd(b.fcExe(), incdir, workdir);
        }

        // ccExe returns the CC compiler setting without all the extra flags we add implicitly.
        private static slice<@string> ccExe(this ref Builder b)
        {
            return b.compilerExe(origCC, cfg.DefaultCC(cfg.Goos, cfg.Goarch));
        }

        // cxxExe returns the CXX compiler setting without all the extra flags we add implicitly.
        private static slice<@string> cxxExe(this ref Builder b)
        {
            return b.compilerExe(origCXX, cfg.DefaultCXX(cfg.Goos, cfg.Goarch));
        }

        // fcExe returns the FC compiler setting without all the extra flags we add implicitly.
        private static slice<@string> fcExe(this ref Builder b)
        {
            return b.compilerExe(os.Getenv("FC"), "gfortran");
        }

        // compilerExe returns the compiler to use given an
        // environment variable setting (the value not the name)
        // and a default. The resulting slice is usually just the name
        // of the compiler but can have additional arguments if they
        // were present in the environment value.
        // For example if CC="gcc -DGOPHER" then the result is ["gcc", "-DGOPHER"].
        private static slice<@string> compilerExe(this ref Builder b, @string envValue, @string def)
        {
            var compiler = strings.Fields(envValue);
            if (len(compiler) == 0L)
            {
                compiler = new slice<@string>(new @string[] { def });
            }
            return compiler;
        }

        // compilerCmd returns a command line prefix for the given environment
        // variable and using the default command when the variable is empty.
        private static slice<@string> compilerCmd(this ref Builder b, slice<@string> compiler, @string incdir, @string workdir)
        { 
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
        private static @string gccNoPie(this ref Builder b, slice<@string> linker)
        {
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
        private static bool gccSupportsFlag(this ref Builder _b, slice<@string> compiler, @string flag) => func(_b, (ref Builder b, Defer defer, Panic _, Recover __) =>
        {
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
            // We used to write an empty C file, but that gets complicated with
            // go build -n. We tried using a file that does not exist, but that
            // fails on systems with GCC version 4.2.1; that is the last GPLv2
            // version of GCC, so some systems have frozen on it.
            // Now we pass an empty file on stdin, which should work at least for
            // GCC and clang.
            var cmdArgs = str.StringList(compiler, flag, "-c", "-x", "c", "-");
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
            cmd.Env = @base.MergeEnvLists(new slice<@string>(new @string[] { "LC_ALL=C" }), @base.EnvForDir(cmd.Dir, os.Environ()));
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
        private static slice<@string> gccArchArgs(this ref Builder b)
        {
            switch (cfg.Goarch)
            {
                case "386": 
                    return new slice<@string>(new @string[] { "-m32" });
                    break;
                case "amd64": 

                case "amd64p32": 
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
            }
            return null;
        }

        // envList returns the value of the given environment variable broken
        // into fields, using the default value when the variable is empty.
        private static slice<@string> envList(@string key, @string def)
        {
            var v = os.Getenv(key);
            if (v == "")
            {
                v = def;
            }
            return strings.Fields(v);
        }

        // CFlags returns the flags to use when invoking the C, C++ or Fortran compilers, or cgo.
        private static (slice<@string>, slice<@string>, slice<@string>, slice<@string>, slice<@string>, error) CFlags(this ref Builder b, ref load.Package p)
        {
            @string defaults = "-g -O2";

            cppflags, err = buildFlags("CPPFLAGS", "", p.CgoCPPFLAGS, checkCompilerFlags);

            if (err != null)
            {
                return;
            }
            cflags, err = buildFlags("CFLAGS", defaults, p.CgoCFLAGS, checkCompilerFlags);

            if (err != null)
            {
                return;
            }
            cxxflags, err = buildFlags("CXXFLAGS", defaults, p.CgoCXXFLAGS, checkCompilerFlags);

            if (err != null)
            {
                return;
            }
            fflags, err = buildFlags("FFLAGS", defaults, p.CgoFFLAGS, checkCompilerFlags);

            if (err != null)
            {
                return;
            }
            ldflags, err = buildFlags("LDFLAGS", defaults, p.CgoLDFLAGS, checkLinkerFlags);

            if (err != null)
            {
                return;
            }
            return;
        }

        private static (slice<@string>, error) buildFlags(@string name, @string defaults, slice<@string> fromPackage, Func<@string, @string, slice<@string>, error> check)
        {
            {
                var err = check(name, "#cgo " + name, fromPackage);

                if (err != null)
                {
                    return (null, err);
                }

            }
            return (str.StringList(envList("CGO_" + name, defaults), fromPackage), null);
        }

        private static var cgoRe = regexp.MustCompile("[/\\\\:]");

        private static (slice<@string>, slice<@string>, error) cgo(this ref Builder b, ref Action a, @string cgoExe, @string objdir, slice<@string> pcCFLAGS, slice<@string> pcLDFLAGS, slice<@string> cgofiles, slice<@string> gccfiles, slice<@string> gxxfiles, slice<@string> mfiles, slice<@string> ffiles)
        {
            var p = a.Package;
            var (cgoCPPFLAGS, cgoCFLAGS, cgoCXXFLAGS, cgoFFLAGS, cgoLDFLAGS, err) = b.CFlags(p);
            if (err != null)
            {
                return (null, null, err);
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
                var fc = os.Getenv("FC");
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
            slice<@string> cgoenv = default;
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
                switch (cfg.Goarch)
                {
                    case "386": 

                    case "amd64": 
                        cgoCFLAGS = append(cgoCFLAGS, "-fsplit-stack");
                        break;
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
                    return (null, null, err);
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
                        return (null, null, err);
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
                            return (null, null, err);
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
                            return (null, null, err);
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
                            return (null, null, err);
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
                            return (null, null, err);
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
                            return (null, null, err);
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
                            return (null, null, err);
                        }

                        err = err__prev1;

                    }
                    outObj = append(outObj, defunObj);
                    break;
                default: 
                    noCompiler();
                    break;
            }

            return (outGo, outObj, null);
        }

        // dynimport creates a Go source file named importGo containing
        // //go:cgo_import_dynamic directives for each symbol or library
        // dynamically imported by the object files outObj.
        private static error dynimport(this ref Builder b, ref Action a, ref load.Package p, @string objdir, @string importGo, @string cgoExe, slice<@string> cflags, slice<@string> cgoLDFLAGS, slice<@string> outObj)
        {
            var cfile = objdir + "_cgo_main.c";
            var ofile = objdir + "_cgo_main.o";
            {
                var err__prev1 = err;

                var err = b.gcc(a, p, objdir, ofile, cflags, cfile);

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }

            var linkobj = str.StringList(ofile, outObj, p.SysoFiles);
            var dynobj = objdir + "_cgo_.o"; 

            // we need to use -pie for Linux/ARM to get accurate imported sym
            var ldflags = cgoLDFLAGS;
            if ((cfg.Goarch == "arm" && cfg.Goos == "linux") || cfg.Goos == "android")
            {
                ldflags = append(ldflags, "-pie");
            }
            {
                var err__prev1 = err;

                err = b.gccld(p, objdir, dynobj, ldflags, linkobj);

                if (err != null)
                {
                    return error.As(err);
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
            return error.As(b.run(a, p.Dir, p.ImportPath, null, cfg.BuildToolexec, cgoExe, "-dynpackage", p.Name, "-dynimport", dynobj, "-dynout", importGo, cgoflags));
        }

        // Run SWIG on all SWIG input files.
        // TODO: Don't build a shared library, once SWIG emits the necessary
        // pragmas for external linking.
        private static (slice<@string>, slice<@string>, slice<@string>, error) swig(this ref Builder b, ref Action a, ref load.Package p, @string objdir, slice<@string> pcCFLAGS)
        {
            {
                var err = b.swigVersionCheck();

                if (err != null)
                {
                    return (null, null, null, err);
                }

            }

            var (intgosize, err) = b.swigIntSize(objdir);
            if (err != null)
            {
                return (null, null, null, err);
            }
            {
                var f__prev1 = f;

                foreach (var (_, __f) in p.SwigFiles)
                {
                    f = __f;
                    var (goFile, cFile, err) = b.swigOne(a, p, f, objdir, pcCFLAGS, false, intgosize);
                    if (err != null)
                    {
                        return (null, null, null, err);
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
                        return (null, null, null, err);
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

            return (outGo, outC, outCXX, null);
        }

        // Make sure SWIG is new enough.
        private static sync.Once swigCheckOnce = default;        private static error swigCheck = default;

        private static error swigDoVersionCheck(this ref Builder b)
        {
            var (out, err) = b.runOut("", "", null, "swig", "-version");
            if (err != null)
            {
                return error.As(err);
            }
            var re = regexp.MustCompile("[vV]ersion +([\\d]+)([.][\\d]+)?([.][\\d]+)?");
            var matches = re.FindSubmatch(out);
            if (matches == null)
            { 
                // Can't find version number; hope for the best.
                return error.As(null);
            }
            var (major, err) = strconv.Atoi(string(matches[1L]));
            if (err != null)
            { 
                // Can't find version number; hope for the best.
                return error.As(null);
            }
            const @string errmsg = "must have SWIG version >= 3.0.6";

            if (major < 3L)
            {
                return error.As(errors.New(errmsg));
            }
            if (major > 3L)
            { 
                // 4.0 or later
                return error.As(null);
            } 

            // We have SWIG version 3.x.
            if (len(matches[2L]) > 0L)
            {
                var (minor, err) = strconv.Atoi(string(matches[2L][1L..]));
                if (err != null)
                {
                    return error.As(null);
                }
                if (minor > 0L)
                { 
                    // 3.1 or later
                    return error.As(null);
                }
            } 

            // We have SWIG version 3.0.x.
            if (len(matches[3L]) > 0L)
            {
                var (patch, err) = strconv.Atoi(string(matches[3L][1L..]));
                if (err != null)
                {
                    return error.As(null);
                }
                if (patch < 6L)
                { 
                    // Before 3.0.6.
                    return error.As(errors.New(errmsg));
                }
            }
            return error.As(null);
        }

        private static error swigVersionCheck(this ref Builder b)
        {
            swigCheckOnce.Do(() =>
            {
                swigCheck = b.swigDoVersionCheck();
            });
            return error.As(swigCheck);
        }

        // Find the value to pass for the -intgosize option to swig.
        private static sync.Once swigIntSizeOnce = default;        private static @string swigIntSize = default;        private static error swigIntSizeError = default;

        // This code fails to build if sizeof(int) <= 32
        private static readonly @string swigIntSizeCode = "\npackage main\nconst i int = 1 << 32\n";

        // Determine the size of int on the target system for the -intgosize option
        // of swig >= 2.0.9. Run only once.


        // Determine the size of int on the target system for the -intgosize option
        // of swig >= 2.0.9. Run only once.
        private static (@string, error) swigDoIntSize(this ref Builder b, @string objdir)
        {
            if (cfg.BuildN)
            {
                return ("$INTBITS", null);
            }
            var src = filepath.Join(b.WorkDir, "swig_intsize.go");
            err = ioutil.WriteFile(src, (slice<byte>)swigIntSizeCode, 0666L);

            if (err != null)
            {
                return;
            }
            @string srcs = new slice<@string>(new @string[] { src });

            var p = load.GoFilesPackage(srcs);

            {
                var (_, _, e) = BuildToolchain.gc(b, ref new Action(Mode:"swigDoIntSize",Package:p,Objdir:objdir), "", null, false, srcs);

                if (e != null)
                {
                    return ("32", null);
                }

            }
            return ("64", null);
        }

        // Determine the size of int on the target system for the -intgosize option
        // of swig >= 2.0.9.
        private static (@string, error) swigIntSize(this ref Builder b, @string objdir)
        {
            swigIntSizeOnce.Do(() =>
            {
                swigIntSize, swigIntSizeError = b.swigDoIntSize(objdir);
            });
            return (swigIntSize, swigIntSizeError);
        }

        // Run SWIG on one SWIG input file.
        private static (@string, @string, error) swigOne(this ref Builder b, ref Action a, ref load.Package p, @string file, @string objdir, slice<@string> pcCFLAGS, bool cxx, @string intgosize)
        {
            var (cgoCPPFLAGS, cgoCFLAGS, cgoCXXFLAGS, _, _, err) = b.CFlags(p);
            if (err != null)
            {
                return ("", "", err);
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
            var (out, err) = b.runOut(p.Dir, p.ImportPath, null, "swig", args, file);
            if (err != null)
            {
                if (len(out) > 0L)
                {
                    if (bytes.Contains(out, (slice<byte>)"-intgosize") || bytes.Contains(out, (slice<byte>)"-cgo"))
                    {
                        return ("", "", errors.New("must have SWIG version >= 3.0.6"));
                    }
                    b.showOutput(a, p.Dir, p.ImportPath, b.processOutput(out)); // swig error
                    return ("", "", errPrintedOutput);
                }
                return ("", "", err);
            }
            if (len(out) > 0L)
            {
                b.showOutput(a, p.Dir, p.ImportPath, b.processOutput(out)); // swig warning
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
                    return ("", "", err);
                }

            }
            return (newGoFile, objdir + gccBase + gccExt, null);
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
        private static slice<@string> disableBuildID(this ref Builder b, slice<@string> ldflags)
        {
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
    }
}}}}
