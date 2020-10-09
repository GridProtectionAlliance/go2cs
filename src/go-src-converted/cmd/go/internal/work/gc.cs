// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package work -- go2cs converted at 2020 October 09 05:46:21 UTC
// import "cmd/go/internal/work" ==> using work = go.cmd.go.@internal.work_package
// Original source: C:\Go\src\cmd\go\internal\work\gc.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using log = go.log_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using strings = go.strings_package;

using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using load = go.cmd.go.@internal.load_package;
using str = go.cmd.go.@internal.str_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using sha1 = go.crypto.sha1_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class work_package
    {
        // The Go toolchain.
        private partial struct gcToolchain
        {
        }

        private static @string compiler(this gcToolchain _p0)
        {
            return @base.Tool("compile");
        }

        private static @string linker(this gcToolchain _p0)
        {
            return @base.Tool("link");
        }

        private static @string pkgPath(ptr<Action> _addr_a)
        {
            ref Action a = ref _addr_a.val;

            var p = a.Package;
            var ppath = p.ImportPath;
            if (cfg.BuildBuildmode == "plugin")
            {
                ppath = pluginPath(_addr_a);
            }
            else if (p.Name == "main" && !p.Internal.ForceLibrary)
            {
                ppath = "main";
            }

            return ppath;

        }

        private static (@string, slice<byte>, error) gc(this gcToolchain _p0, ptr<Builder> _addr_b, ptr<Action> _addr_a, @string archive, slice<byte> importcfg, @string symabis, bool asmhdr, slice<@string> gofiles)
        {
            @string ofile = default;
            slice<byte> output = default;
            error err = default!;
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;

            var p = a.Package;
            var objdir = a.Objdir;
            if (archive != "")
            {
                ofile = archive;
            }
            else
            {
                @string @out = "_go_.o";
                ofile = objdir + out;
            }

            var pkgpath = pkgPath(_addr_a);
            @string gcargs = new slice<@string>(new @string[] { "-p", pkgpath });
            if (p.Module != null && p.Module.GoVersion != "" && allowedVersion(p.Module.GoVersion))
            {
                gcargs = append(gcargs, "-lang=go" + p.Module.GoVersion);
            }

            if (p.Standard)
            {
                gcargs = append(gcargs, "-std");
            }

            var compilingRuntime = p.Standard && (p.ImportPath == "runtime" || strings.HasPrefix(p.ImportPath, "runtime/internal")); 
            // The runtime package imports a couple of general internal packages.
            if (p.Standard && (p.ImportPath == "internal/cpu" || p.ImportPath == "internal/bytealg"))
            {
                compilingRuntime = true;
            }

            if (compilingRuntime)
            { 
                // runtime compiles with a special gc flag to check for
                // memory allocations that are invalid in the runtime package,
                // and to implement some special compiler pragmas.
                gcargs = append(gcargs, "-+");

            } 

            // If we're giving the compiler the entire package (no C etc files), tell it that,
            // so that it can give good error messages about forward declarations.
            // Exceptions: a few standard packages have forward declarations for
            // pieces supplied behind-the-scenes by package runtime.
            var extFiles = len(p.CgoFiles) + len(p.CFiles) + len(p.CXXFiles) + len(p.MFiles) + len(p.FFiles) + len(p.SFiles) + len(p.SysoFiles) + len(p.SwigFiles) + len(p.SwigCXXFiles);
            if (p.Standard)
            {
                switch (p.ImportPath)
                {
                    case "bytes": 

                    case "internal/poll": 

                    case "net": 

                    case "os": 

                    case "runtime/pprof": 

                    case "runtime/trace": 

                    case "sync": 

                    case "syscall": 

                    case "time": 
                        extFiles++;
                        break;
                }

            }

            if (extFiles == 0L)
            {
                gcargs = append(gcargs, "-complete");
            }

            if (cfg.BuildContext.InstallSuffix != "")
            {
                gcargs = append(gcargs, "-installsuffix", cfg.BuildContext.InstallSuffix);
            }

            if (a.buildID != "")
            {
                gcargs = append(gcargs, "-buildid", a.buildID);
            }

            if (p.Internal.OmitDebug || cfg.Goos == "plan9" || cfg.Goarch == "wasm")
            {
                gcargs = append(gcargs, "-dwarf=false");
            }

            if (strings.HasPrefix(runtimeVersion, "go1") && !strings.Contains(os.Args[0L], "go_bootstrap"))
            {
                gcargs = append(gcargs, "-goversion", runtimeVersion);
            }

            if (symabis != "")
            {
                gcargs = append(gcargs, "-symabis", symabis);
            }

            var gcflags = str.StringList(forcedGcflags, p.Internal.Gcflags);
            if (compilingRuntime)
            { 
                // Remove -N, if present.
                // It is not possible to build the runtime with no optimizations,
                // because the compiler cannot eliminate enough write barriers.
                for (long i = 0L; i < len(gcflags); i++)
                {
                    if (gcflags[i] == "-N")
                    {
                        copy(gcflags[i..], gcflags[i + 1L..]);
                        gcflags = gcflags[..len(gcflags) - 1L];
                        i--;
                    }

                }


            }

            if (importcfg != null)
            {
                {
                    var err = b.writeFile(objdir + "importcfg", importcfg);

                    if (err != null)
                    {
                        return ("", null, error.As(err)!);
                    }

                }

                args = append(args, "-importcfg", objdir + "importcfg");

            }

            if (ofile == archive)
            {
                args = append(args, "-pack");
            }

            if (asmhdr)
            {
                args = append(args, "-asmhdr", objdir + "go_asm.h");
            } 

            // Add -c=N to use concurrent backend compilation, if possible.
            {
                var c = gcBackendConcurrency(gcflags);

                if (c > 1L)
                {
                    args = append(args, fmt.Sprintf("-c=%d", c));
                }

            }


            foreach (var (_, f) in gofiles)
            {
                args = append(args, mkAbs(p.Dir, f));
            }
            output, err = b.runOut(a, p.Dir, null, args);
            return (ofile, output, error.As(err)!);

        }

        // gcBackendConcurrency returns the backend compiler concurrency level for a package compilation.
        private static long gcBackendConcurrency(slice<@string> gcflags)
        { 
            // First, check whether we can use -c at all for this compilation.
            var canDashC = concurrentGCBackendCompilationEnabledByDefault;

            {
                var e = os.Getenv("GO19CONCURRENTCOMPILATION");

                switch (e)
                {
                    case "0": 
                        canDashC = false;
                        break;
                    case "1": 
                        canDashC = true;
                        break;
                    case "": 
                        break;
                    default: 
                        log.Fatalf("GO19CONCURRENTCOMPILATION must be 0, 1, or unset, got %q", e);
                        break;
                }
            }

CheckFlags: 

            // TODO: Test and delete these conditions.
            foreach (var (_, flag) in gcflags)
            { 
                // Concurrent compilation is presumed incompatible with any gcflags,
                // except for known commonly used flags.
                // If the user knows better, they can manually add their own -c to the gcflags.
                switch (flag)
                {
                    case "-N": 

                    case "-l": 

                    case "-S": 

                    case "-B": 

                    case "-C": 

                    case "-I": 
                        break;
                    default: 
                        canDashC = false;
                        _breakCheckFlags = true;
                        break;
                        break;
                }

            } 

            // TODO: Test and delete these conditions.
            if (objabi.Fieldtrack_enabled != 0L || objabi.Preemptibleloops_enabled != 0L)
            {
                canDashC = false;
            }

            if (!canDashC)
            {
                return 1L;
            } 

            // Decide how many concurrent backend compilations to allow.
            //
            // If we allow too many, in theory we might end up with p concurrent processes,
            // each with c concurrent backend compiles, all fighting over the same resources.
            // However, in practice, that seems not to happen too much.
            // Most build graphs are surprisingly serial, so p==1 for much of the build.
            // Furthermore, concurrent backend compilation is only enabled for a part
            // of the overall compiler execution, so c==1 for much of the build.
            // So don't worry too much about that interaction for now.
            //
            // However, in practice, setting c above 4 tends not to help very much.
            // See the analysis in CL 41192.
            //
            // TODO(josharian): attempt to detect whether this particular compilation
            // is likely to be a bottleneck, e.g. when:
            //   - it has no successor packages to compile (usually package main)
            //   - all paths through the build graph pass through it
            //   - critical path scheduling says it is high priority
            // and in such a case, set c to runtime.NumCPU.
            // We do this now when p==1.
            if (cfg.BuildP == 1L)
            { 
                // No process parallelism. Max out c.
                return runtime.NumCPU();

            } 
            // Some process parallelism. Set c to min(4, numcpu).
            long c = 4L;
            {
                var ncpu = runtime.NumCPU();

                if (ncpu < c)
                {
                    c = ncpu;
                }

            }

            return c;

        }

        // trimpath returns the -trimpath argument to use
        // when compiling the action.
        private static @string trimpath(this ptr<Action> _addr_a)
        {
            ref Action a = ref _addr_a.val;
 
            // Keep in sync with Builder.ccompile
            // The trimmed paths are a little different, but we need to trim in the
            // same situations.

            // Strip the object directory entirely.
            var objdir = a.Objdir;
            if (len(objdir) > 1L && objdir[len(objdir) - 1L] == filepath.Separator)
            {
                objdir = objdir[..len(objdir) - 1L];
            }

            var rewrite = objdir + "=>"; 

            // For "go build -trimpath", rewrite package source directory
            // to a file system-independent path (just the import path).
            if (cfg.BuildTrimpath)
            {
                {
                    var m = a.Package.Module;

                    if (m != null && m.Version != "")
                    {
                        rewrite += ";" + a.Package.Dir + "=>" + m.Path + "@" + m.Version + strings.TrimPrefix(a.Package.ImportPath, m.Path);
                    }
                    else
                    {
                        rewrite += ";" + a.Package.Dir + "=>" + a.Package.ImportPath;
                    }

                }

            }

            return rewrite;

        }

        private static slice<object> asmArgs(ptr<Action> _addr_a, ptr<load.Package> _addr_p)
        {
            ref Action a = ref _addr_a.val;
            ref load.Package p = ref _addr_p.val;
 
            // Add -I pkg/GOOS_GOARCH so #include "textflag.h" works in .s files.
            var inc = filepath.Join(cfg.GOROOT, "pkg", "include");
            var pkgpath = pkgPath(_addr_a);
            if (p.ImportPath == "runtime" && cfg.Goarch == "386")
            {
                foreach (var (_, arg) in forcedAsmflags)
                {
                    if (arg == "-dynlink")
                    {
                        args = append(args, "-D=GOBUILDMODE_shared=1");
                    }

                }

            }

            if (cfg.Goarch == "mips" || cfg.Goarch == "mipsle")
            { 
                // Define GOMIPS_value from cfg.GOMIPS.
                args = append(args, "-D", "GOMIPS_" + cfg.GOMIPS);

            }

            if (cfg.Goarch == "mips64" || cfg.Goarch == "mips64le")
            { 
                // Define GOMIPS64_value from cfg.GOMIPS64.
                args = append(args, "-D", "GOMIPS64_" + cfg.GOMIPS64);

            }

            return args;

        }

        private static (slice<@string>, error) asm(this gcToolchain _p0, ptr<Builder> _addr_b, ptr<Action> _addr_a, slice<@string> sfiles)
        {
            slice<@string> _p0 = default;
            error _p0 = default!;
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;

            var p = a.Package;
            var args = asmArgs(_addr_a, _addr_p);

            slice<@string> ofiles = default;
            foreach (var (_, sfile) in sfiles)
            {
                var ofile = a.Objdir + sfile[..len(sfile) - len(".s")] + ".o";
                ofiles = append(ofiles, ofile);
                var args1 = append(args, "-o", ofile, mkAbs(p.Dir, sfile));
                {
                    var err = b.run(a, p.Dir, p.ImportPath, null, args1);

                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    }

                }

            }
            return (ofiles, error.As(null!)!);

        }

        private static (@string, error) symabis(this gcToolchain _p0, ptr<Builder> _addr_b, ptr<Action> _addr_a, slice<@string> sfiles)
        {
            @string _p0 = default;
            error _p0 = default!;
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;

            Func<ptr<load.Package>, slice<@string>, @string, error> mkSymabis = (p, sfiles, path) =>
            {
                var args = asmArgs(_addr_a, _addr_p);
                args = append(args, "-gensymabis", "-o", path);
                foreach (var (_, sfile) in sfiles)
                {
                    if (p.ImportPath == "runtime/cgo" && strings.HasPrefix(sfile, "gcc_"))
                    {
                        continue;
                    }

                    args = append(args, mkAbs(p.Dir, sfile));

                } 

                // Supply an empty go_asm.h as if the compiler had been run.
                // -gensymabis parsing is lax enough that we don't need the
                // actual definitions that would appear in go_asm.h.
                {
                    var err__prev1 = err;

                    var err = b.writeFile(a.Objdir + "go_asm.h", null);

                    if (err != null)
                    {
                        return err;
                    }

                    err = err__prev1;

                }


                return b.run(a, p.Dir, p.ImportPath, null, args);

            }
;

            @string symabis = default; // Only set if we actually create the file
            var p = a.Package;
            if (len(sfiles) != 0L)
            {
                symabis = a.Objdir + "symabis";
                {
                    var err__prev2 = err;

                    err = mkSymabis(p, sfiles, symabis);

                    if (err != null)
                    {
                        return ("", error.As(err)!);
                    }

                    err = err__prev2;

                }

            }

            return (symabis, error.As(null!)!);

        }

        // toolVerify checks that the command line args writes the same output file
        // if run using newTool instead.
        // Unused now but kept around for future use.
        private static error toolVerify(ptr<Action> _addr_a, ptr<Builder> _addr_b, ptr<load.Package> _addr_p, @string newTool, @string ofile, slice<object> args)
        {
            ref Action a = ref _addr_a.val;
            ref Builder b = ref _addr_b.val;
            ref load.Package p = ref _addr_p.val;

            var newArgs = make_slice<object>(len(args));
            copy(newArgs, args);
            newArgs[1L] = @base.Tool(newTool);
            newArgs[3L] = ofile + ".new"; // x.6 becomes x.6.new
            {
                var err = b.run(a, p.Dir, p.ImportPath, null, newArgs);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            var (data1, err) = ioutil.ReadFile(ofile);
            if (err != null)
            {
                return error.As(err)!;
            }

            var (data2, err) = ioutil.ReadFile(ofile + ".new");
            if (err != null)
            {
                return error.As(err)!;
            }

            if (!bytes.Equal(data1, data2))
            {
                return error.As(fmt.Errorf("%s and %s produced different output files:\n%s\n%s", filepath.Base(args[1L]._<@string>()), newTool, strings.Join(str.StringList(args), " "), strings.Join(str.StringList(newArgs), " ")))!;
            }

            os.Remove(ofile + ".new");
            return error.As(null!)!;

        }

        private static error pack(this gcToolchain _p0, ptr<Builder> _addr_b, ptr<Action> _addr_a, @string afile, slice<@string> ofiles)
        {
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;

            slice<@string> absOfiles = default;
            foreach (var (_, f) in ofiles)
            {
                absOfiles = append(absOfiles, mkAbs(a.Objdir, f));
            }
            var absAfile = mkAbs(a.Objdir, afile); 

            // The archive file should have been created by the compiler.
            // Since it used to not work that way, verify.
            if (!cfg.BuildN)
            {
                {
                    var (_, err) = os.Stat(absAfile);

                    if (err != null)
                    {
                        @base.Fatalf("os.Stat of archive file failed: %v", err);
                    }

                }

            }

            var p = a.Package;
            if (cfg.BuildN || cfg.BuildX)
            {
                var cmdline = str.StringList(@base.Tool("pack"), "r", absAfile, absOfiles);
                b.Showcmd(p.Dir, "%s # internal", joinUnambiguously(cmdline));
            }

            if (cfg.BuildN)
            {
                return error.As(null!)!;
            }

            {
                var err = packInternal(absAfile, absOfiles);

                if (err != null)
                {
                    b.showOutput(a, p.Dir, p.Desc(), err.Error() + "\n");
                    return error.As(errPrintedOutput)!;
                }

            }

            return error.As(null!)!;

        }

        private static error packInternal(@string afile, slice<@string> ofiles) => func((defer, _, __) =>
        {
            var (dst, err) = os.OpenFile(afile, os.O_WRONLY | os.O_APPEND, 0L);
            if (err != null)
            {
                return error.As(err)!;
            }

            defer(dst.Close()); // only for error returns or panics
            var w = bufio.NewWriter(dst);

            foreach (var (_, ofile) in ofiles)
            {
                var (src, err) = os.Open(ofile);
                if (err != null)
                {
                    return error.As(err)!;
                }

                var (fi, err) = src.Stat();
                if (err != null)
                {
                    src.Close();
                    return error.As(err)!;
                } 
                // Note: Not using %-16.16s format because we care
                // about bytes, not runes.
                var name = fi.Name();
                if (len(name) > 16L)
                {
                    name = name[..16L];
                }
                else
                {
                    name += strings.Repeat(" ", 16L - len(name));
                }

                var size = fi.Size();
                fmt.Fprintf(w, "%s%-12d%-6d%-6d%-8o%-10d`\n", name, 0L, 0L, 0L, 0644L, size);
                var (n, err) = io.Copy(w, src);
                src.Close();
                if (err == null && n < size)
                {
                    err = io.ErrUnexpectedEOF;
                }
                else if (err == null && n > size)
                {
                    err = fmt.Errorf("file larger than size reported by stat");
                }

                if (err != null)
                {
                    return error.As(fmt.Errorf("copying %s to %s: %v", ofile, afile, err))!;
                }

                if (size & 1L != 0L)
                {
                    w.WriteByte(0L);
                }

            }
            {
                var err = w.Flush();

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            return error.As(dst.Close())!;

        });

        // setextld sets the appropriate linker flags for the specified compiler.
        private static slice<@string> setextld(slice<@string> ldflags, slice<@string> compiler)
        {
            {
                var f__prev1 = f;

                foreach (var (_, __f) in ldflags)
                {
                    f = __f;
                    if (f == "-extld" || strings.HasPrefix(f, "-extld="))
                    { 
                        // don't override -extld if supplied
                        return ldflags;

                    }

                }

                f = f__prev1;
            }

            ldflags = append(ldflags, "-extld=" + compiler[0L]);
            if (len(compiler) > 1L)
            {
                var extldflags = false;
                var add = strings.Join(compiler[1L..], " ");
                {
                    var f__prev1 = f;

                    foreach (var (__i, __f) in ldflags)
                    {
                        i = __i;
                        f = __f;
                        if (f == "-extldflags" && i + 1L < len(ldflags))
                        {
                            ldflags[i + 1L] = add + " " + ldflags[i + 1L];
                            extldflags = true;
                            break;
                        }
                        else if (strings.HasPrefix(f, "-extldflags="))
                        {
                            ldflags[i] = "-extldflags=" + add + " " + ldflags[i][len("-extldflags=")..];
                            extldflags = true;
                            break;
                        }

                    }

                    f = f__prev1;
                }

                if (!extldflags)
                {
                    ldflags = append(ldflags, "-extldflags=" + add);
                }

            }

            return ldflags;

        }

        // pluginPath computes the package path for a plugin main package.
        //
        // This is typically the import path of the main package p, unless the
        // plugin is being built directly from source files. In that case we
        // combine the package build ID with the contents of the main package
        // source files. This allows us to identify two different plugins
        // built from two source files with the same name.
        private static @string pluginPath(ptr<Action> _addr_a)
        {
            ref Action a = ref _addr_a.val;

            var p = a.Package;
            if (p.ImportPath != "command-line-arguments")
            {
                return p.ImportPath;
            }

            var h = sha1.New();
            var buildID = a.buildID;
            if (a.Mode == "link")
            { 
                // For linking, use the main package's build ID instead of
                // the binary's build ID, so it is the same hash used in
                // compiling and linking.
                // When compiling, we use actionID/actionID (instead of
                // actionID/contentID) as a temporary build ID to compute
                // the hash. Do the same here. (See buildid.go:useCache)
                // The build ID matters because it affects the overall hash
                // in the plugin's pseudo-import path returned below.
                // We need to use the same import path when compiling and linking.
                var id = strings.Split(buildID, buildIDSeparator);
                buildID = id[1L] + buildIDSeparator + id[1L];

            }

            fmt.Fprintf(h, "build ID: %s\n", buildID);
            foreach (var (_, file) in str.StringList(p.GoFiles, p.CgoFiles, p.SFiles))
            {
                var (data, err) = ioutil.ReadFile(filepath.Join(p.Dir, file));
                if (err != null)
                {
                    @base.Fatalf("go: %s", err);
                }

                h.Write(data);

            }
            return fmt.Sprintf("plugin/unnamed-%x", h.Sum(null));

        }

        private static error ld(this gcToolchain _p0, ptr<Builder> _addr_b, ptr<Action> _addr_root, @string @out, @string importcfg, @string mainpkg)
        {
            ref Builder b = ref _addr_b.val;
            ref Action root = ref _addr_root.val;

            var cxx = len(root.Package.CXXFiles) > 0L || len(root.Package.SwigCXXFiles) > 0L;
            foreach (var (_, a) in root.Deps)
            {
                if (a.Package != null && (len(a.Package.CXXFiles) > 0L || len(a.Package.SwigCXXFiles) > 0L))
                {
                    cxx = true;
                }

            }
            slice<@string> ldflags = default;
            if (cfg.BuildContext.InstallSuffix != "")
            {
                ldflags = append(ldflags, "-installsuffix", cfg.BuildContext.InstallSuffix);
            }

            if (root.Package.Internal.OmitDebug)
            {
                ldflags = append(ldflags, "-s", "-w");
            }

            if (cfg.BuildBuildmode == "plugin")
            {
                ldflags = append(ldflags, "-pluginpath", pluginPath(_addr_root));
            } 

            // Store BuildID inside toolchain binaries as a unique identifier of the
            // tool being run, for use by content-based staleness determination.
            if (root.Package.Goroot && strings.HasPrefix(root.Package.ImportPath, "cmd/"))
            { 
                // External linking will include our build id in the external
                // linker's build id, which will cause our build id to not
                // match the next time the tool is built.
                // Rely on the external build id instead.
                if (!sys.MustLinkExternal(cfg.Goos, cfg.Goarch))
                {
                    ldflags = append(ldflags, "-X=cmd/internal/objabi.buildID=" + root.buildID);
                }

            } 

            // If the user has not specified the -extld option, then specify the
            // appropriate linker. In case of C++ code, use the compiler named
            // by the CXX environment variable or defaultCXX if CXX is not set.
            // Else, use the CC environment variable and defaultCC as fallback.
            slice<@string> compiler = default;
            if (cxx)
            {
                compiler = envList("CXX", cfg.DefaultCXX(cfg.Goos, cfg.Goarch));
            }
            else
            {
                compiler = envList("CC", cfg.DefaultCC(cfg.Goos, cfg.Goarch));
            }

            ldflags = append(ldflags, "-buildmode=" + ldBuildmode);
            if (root.buildID != "")
            {
                ldflags = append(ldflags, "-buildid=" + root.buildID);
            }

            ldflags = append(ldflags, forcedLdflags);
            ldflags = append(ldflags, root.Package.Internal.Ldflags);
            ldflags = setextld(ldflags, compiler); 

            // On OS X when using external linking to build a shared library,
            // the argument passed here to -o ends up recorded in the final
            // shared library in the LC_ID_DYLIB load command.
            // To avoid putting the temporary output directory name there
            // (and making the resulting shared library useless),
            // run the link in the output directory so that -o can name
            // just the final path element.
            // On Windows, DLL file name is recorded in PE file
            // export section, so do like on OS X.
            @string dir = ".";
            if ((cfg.Goos == "darwin" || cfg.Goos == "windows") && cfg.BuildBuildmode == "c-shared")
            {
                dir, out = filepath.Split(out);
            }

            @string env = new slice<@string>(new @string[] {  });
            if (cfg.BuildTrimpath)
            {
                env = append(env, "GOROOT_FINAL=go");
            }

            return error.As(b.run(root, dir, root.Package.ImportPath, env, cfg.BuildToolexec, @base.Tool("link"), "-o", out, "-importcfg", importcfg, ldflags, mainpkg))!;

        }

        private static error ldShared(this gcToolchain _p0, ptr<Builder> _addr_b, ptr<Action> _addr_root, slice<ptr<Action>> toplevelactions, @string @out, @string importcfg, slice<ptr<Action>> allactions)
        {
            ref Builder b = ref _addr_b.val;
            ref Action root = ref _addr_root.val;

            @string ldflags = new slice<@string>(new @string[] { "-installsuffix", cfg.BuildContext.InstallSuffix });
            ldflags = append(ldflags, "-buildmode=shared");
            ldflags = append(ldflags, forcedLdflags);
            ldflags = append(ldflags, root.Package.Internal.Ldflags);
            var cxx = false;
            foreach (var (_, a) in allactions)
            {
                if (a.Package != null && (len(a.Package.CXXFiles) > 0L || len(a.Package.SwigCXXFiles) > 0L))
                {
                    cxx = true;
                }

            } 
            // If the user has not specified the -extld option, then specify the
            // appropriate linker. In case of C++ code, use the compiler named
            // by the CXX environment variable or defaultCXX if CXX is not set.
            // Else, use the CC environment variable and defaultCC as fallback.
            slice<@string> compiler = default;
            if (cxx)
            {
                compiler = envList("CXX", cfg.DefaultCXX(cfg.Goos, cfg.Goarch));
            }
            else
            {
                compiler = envList("CC", cfg.DefaultCC(cfg.Goos, cfg.Goarch));
            }

            ldflags = setextld(ldflags, compiler);
            foreach (var (_, d) in toplevelactions)
            {
                if (!strings.HasSuffix(d.Target, ".a"))
                { // omit unsafe etc and actions for other shared libraries
                    continue;

                }

                ldflags = append(ldflags, d.Package.ImportPath + "=" + d.Target);

            }
            return error.As(b.run(root, ".", out, null, cfg.BuildToolexec, @base.Tool("link"), "-o", out, "-importcfg", importcfg, ldflags))!;

        }

        private static error cc(this gcToolchain _p0, ptr<Builder> _addr_b, ptr<Action> _addr_a, @string ofile, @string cfile)
        {
            ref Builder b = ref _addr_b.val;
            ref Action a = ref _addr_a.val;

            return error.As(fmt.Errorf("%s: C source files not supported without cgo", mkAbs(a.Package.Dir, cfile)))!;
        }
    }
}}}}
