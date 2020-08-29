// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package work -- go2cs converted at 2020 August 29 10:01:36 UTC
// import "cmd/go/internal/work" ==> using work = go.cmd.go.@internal.work_package
// Original source: C:\Go\src\cmd\go\internal\work\gccgo.go
using fmt = go.fmt_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;

using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using load = go.cmd.go.@internal.load_package;
using str = go.cmd.go.@internal.str_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class work_package
    {
        // The Gccgo toolchain.
        private partial struct gccgoToolchain
        {
        }

        public static @string GccgoName = default;        public static @string GccgoBin = default;

        private static error gccgoErr = default;

        private static void init()
        {
            GccgoName = os.Getenv("GCCGO");
            if (GccgoName == "")
            {
                GccgoName = "gccgo";
            }
            GccgoBin, gccgoErr = exec.LookPath(GccgoName);
        }

        private static @string compiler(this gccgoToolchain _p0)
        {
            checkGccgoBin();
            return GccgoBin;
        }

        private static @string linker(this gccgoToolchain _p0)
        {
            checkGccgoBin();
            return GccgoBin;
        }

        private static void checkGccgoBin()
        {
            if (gccgoErr == null)
            {
                return;
            }
            fmt.Fprintf(os.Stderr, "cmd/go: gccgo: %s\n", gccgoErr);
            os.Exit(2L);
        }

        private static (@string, slice<byte>, error) gc(this gccgoToolchain tools, ref Builder b, ref Action a, @string archive, slice<byte> importcfg, bool asmhdr, slice<@string> gofiles)
        {
            var p = a.Package;
            var objdir = a.Objdir;
            @string @out = "_go_.o";
            ofile = objdir + out;
            @string gcargs = new slice<@string>(new @string[] { "-g" });
            gcargs = append(gcargs, b.gccArchArgs());
            {
                var pkgpath = gccgoPkgpath(p);

                if (pkgpath != "")
                {
                    gcargs = append(gcargs, "-fgo-pkgpath=" + pkgpath);
                }

            }
            if (p.Internal.LocalPrefix != "")
            {
                gcargs = append(gcargs, "-fgo-relative-import-path=" + p.Internal.LocalPrefix);
            }
            var args = str.StringList(tools.compiler(), "-c", gcargs, "-o", ofile, forcedGccgoflags);
            if (importcfg != null)
            {
                if (b.gccSupportsFlag(args[..1L], "-fgo-importcfg=/dev/null"))
                {
                    {
                        var err__prev3 = err;

                        var err = b.writeFile(objdir + "importcfg", importcfg);

                        if (err != null)
                        {
                            return ("", null, err);
                        }

                        err = err__prev3;

                    }
                    args = append(args, "-fgo-importcfg=" + objdir + "importcfg");
                }
                else
                {
                    var root = objdir + "_importcfgroot_";
                    {
                        var err__prev3 = err;

                        err = buildImportcfgSymlinks(b, root, importcfg);

                        if (err != null)
                        {
                            return ("", null, err);
                        }

                        err = err__prev3;

                    }
                    args = append(args, "-I", root);
                }
            }
            args = append(args, a.Package.Internal.Gccgoflags);
            foreach (var (_, f) in gofiles)
            {
                args = append(args, mkAbs(p.Dir, f));
            }
            output, err = b.runOut(p.Dir, p.ImportPath, null, args);
            return (ofile, output, err);
        }

        // buildImportcfgSymlinks builds in root a tree of symlinks
        // implementing the directives from importcfg.
        // This serves as a temporary transition mechanism until
        // we can depend on gccgo reading an importcfg directly.
        // (The Go 1.9 and later gc compilers already do.)
        private static error buildImportcfgSymlinks(ref Builder b, @string root, slice<byte> importcfg)
        {
            foreach (var (lineNum, line) in strings.Split(string(importcfg), "\n"))
            {
                lineNum++; // 1-based
                line = strings.TrimSpace(line);
                if (line == "")
                {
                    continue;
                }
                if (line == "" || strings.HasPrefix(line, "#"))
                {
                    continue;
                }
                @string verb = default;                @string args = default;

                {
                    var i__prev1 = i;

                    var i = strings.Index(line, " ");

                    if (i < 0L)
                    {
                        verb = line;
                    }
                    else
                    {
                        verb = line[..i];
                        args = strings.TrimSpace(line[i + 1L..]);
                    }

                    i = i__prev1;

                }
                @string before = default;                @string after = default;

                {
                    var i__prev1 = i;

                    i = strings.Index(args, "=");

                    if (i >= 0L)
                    {
                        before = args[..i];
                        after = args[i + 1L..];
                    }

                    i = i__prev1;

                }
                switch (verb)
                {
                    case "packagefile": 
                        if (before == "" || after == "")
                        {
                            return error.As(fmt.Errorf("importcfg:%d: invalid packagefile: syntax is \"packagefile path=filename\": %s", lineNum, line));
                        }
                        var archive = gccgoArchive(root, before);
                        {
                            var err__prev1 = err;

                            var err = b.Mkdir(filepath.Dir(archive));

                            if (err != null)
                            {
                                return error.As(err);
                            }

                            err = err__prev1;

                        }
                        {
                            var err__prev1 = err;

                            err = b.Symlink(after, archive);

                            if (err != null)
                            {
                                return error.As(err);
                            }

                            err = err__prev1;

                        }
                        break;
                    case "importmap": 
                        if (before == "" || after == "")
                        {
                            return error.As(fmt.Errorf("importcfg:%d: invalid importmap: syntax is \"importmap old=new\": %s", lineNum, line));
                        }
                        var beforeA = gccgoArchive(root, before);
                        var afterA = gccgoArchive(root, after);
                        {
                            var err__prev1 = err;

                            err = b.Mkdir(filepath.Dir(beforeA));

                            if (err != null)
                            {
                                return error.As(err);
                            }

                            err = err__prev1;

                        }
                        {
                            var err__prev1 = err;

                            err = b.Mkdir(filepath.Dir(afterA));

                            if (err != null)
                            {
                                return error.As(err);
                            }

                            err = err__prev1;

                        }
                        {
                            var err__prev1 = err;

                            err = b.Symlink(afterA, beforeA);

                            if (err != null)
                            {
                                return error.As(err);
                            }

                            err = err__prev1;

                        }
                        break;
                    case "packageshlib": 
                        return error.As(fmt.Errorf("gccgo -importcfg does not support shared libraries"));
                        break;
                    default: 
                        @base.Fatalf("importcfg:%d: unknown directive %q", lineNum, verb);
                        break;
                }
            }
            return error.As(null);
        }

        private static (slice<@string>, error) asm(this gccgoToolchain tools, ref Builder b, ref Action a, slice<@string> sfiles)
        {
            var p = a.Package;
            slice<@string> ofiles = default;
            foreach (var (_, sfile) in sfiles)
            {
                var @base = filepath.Base(sfile);
                var ofile = a.Objdir + base[..len(base) - len(".s")] + ".o";
                ofiles = append(ofiles, ofile);
                sfile = mkAbs(p.Dir, sfile);
                @string defs = new slice<@string>(new @string[] { "-D", "GOOS_"+cfg.Goos, "-D", "GOARCH_"+cfg.Goarch });
                {
                    var pkgpath = gccgoCleanPkgpath(p);

                    if (pkgpath != "")
                    {
                        defs = append(defs, "-D", "GOPKGPATH=" + pkgpath);
                    }

                }
                defs = tools.maybePIC(defs);
                defs = append(defs, b.gccArchArgs());
                var err = b.run(a, p.Dir, p.ImportPath, null, tools.compiler(), "-xassembler-with-cpp", "-I", a.Objdir, "-c", "-o", ofile, defs, sfile);
                if (err != null)
                {
                    return (null, err);
                }
            }
            return (ofiles, null);
        }

        private static @string gccgoArchive(@string basedir, @string imp)
        {
            var end = filepath.FromSlash(imp + ".a");
            var afile = filepath.Join(basedir, end); 
            // add "lib" to the final element
            return filepath.Join(filepath.Dir(afile), "lib" + filepath.Base(afile));
        }

        private static error pack(this gccgoToolchain _p0, ref Builder b, ref Action a, @string afile, slice<@string> ofiles)
        {
            var p = a.Package;
            var objdir = a.Objdir;
            slice<@string> absOfiles = default;
            foreach (var (_, f) in ofiles)
            {
                absOfiles = append(absOfiles, mkAbs(objdir, f));
            }
            return error.As(b.run(a, p.Dir, p.ImportPath, null, "ar", "rc", mkAbs(objdir, afile), absOfiles));
        }

        private static error link(this gccgoToolchain tools, ref Builder b, ref Action root, @string @out, @string importcfg, slice<ref Action> allactions, @string buildmode, @string desc)
        { 
            // gccgo needs explicit linking with all package dependencies,
            // and all LDFLAGS from cgo dependencies.
            @string afiles = new slice<@string>(new @string[] {  });
            @string shlibs = new slice<@string>(new @string[] {  });
            var ldflags = b.gccArchArgs();
            @string cgoldflags = new slice<@string>(new @string[] {  });
            var usesCgo = false;
            var cxx = false;
            var objc = false;
            var fortran = false;
            if (root.Package != null)
            {
                cxx = len(root.Package.CXXFiles) > 0L || len(root.Package.SwigCXXFiles) > 0L;
                objc = len(root.Package.MFiles) > 0L;
                fortran = len(root.Package.FFiles) > 0L;
            }
            Func<@string, error> readCgoFlags = flagsFile =>
            {
                var (flags, err) = ioutil.ReadFile(flagsFile);
                if (err != null)
                {
                    return error.As(err);
                }
                const @string ldflagsPrefix = "_CGO_LDFLAGS=";

                foreach (var (_, line) in strings.Split(string(flags), "\n"))
                {
                    if (strings.HasPrefix(line, ldflagsPrefix))
                    {
                        var newFlags = strings.Fields(line[len(ldflagsPrefix)..]);
                        foreach (var (_, flag) in newFlags)
                        { 
                            // Every _cgo_flags file has -g and -O2 in _CGO_LDFLAGS
                            // but they don't mean anything to the linker so filter
                            // them out.
                            if (flag != "-g" && !strings.HasPrefix(flag, "-O"))
                            {
                                cgoldflags = append(cgoldflags, flag);
                            }
                        }
                    }
                }
                return error.As(null);
            }
;

            long newID = 0L;
            Func<@string, (@string, error)> readAndRemoveCgoFlags = archive =>
            {
                newID++;
                var newArchive = root.Objdir + fmt.Sprintf("_pkg%d_.a", newID);
                {
                    var err__prev1 = err;

                    var err = b.copyFile(root, newArchive, archive, 0666L, false);

                    if (err != null)
                    {
                        return ("", err);
                    }

                    err = err__prev1;

                }
                if (cfg.BuildN || cfg.BuildX)
                {
                    b.Showcmd("", "ar d %s _cgo_flags", newArchive);
                    if (cfg.BuildN)
                    { 
                        // TODO(rsc): We could do better about showing the right _cgo_flags even in -n mode.
                        // Either the archive is already built and we can read them out,
                        // or we're printing commands to build the archive and can
                        // forward the _cgo_flags directly to this step.
                        return ("", null);
                    }
                }
                err = b.run(root, root.Objdir, desc, null, "ar", "x", newArchive, "_cgo_flags");
                if (err != null)
                {
                    return ("", err);
                }
                err = b.run(root, ".", desc, null, "ar", "d", newArchive, "_cgo_flags");
                if (err != null)
                {
                    return ("", err);
                }
                err = readCgoFlags(filepath.Join(root.Objdir, "_cgo_flags"));
                if (err != null)
                {
                    return ("", err);
                }
                return (newArchive, null);
            } 

            // If using -linkshared, find the shared library deps.
; 

            // If using -linkshared, find the shared library deps.
            var haveShlib = make_map<@string, bool>();
            var targetBase = filepath.Base(root.Target);
            if (cfg.BuildLinkshared)
            {
                {
                    var a__prev1 = a;

                    foreach (var (_, __a) in root.Deps)
                    {
                        a = __a;
                        var p = a.Package;
                        if (p == null || p.Shlib == "")
                        {
                            continue;
                        } 

                        // The .a we are linking into this .so
                        // will have its Shlib set to this .so.
                        // Don't start thinking we want to link
                        // this .so into itself.
                        var @base = filepath.Base(p.Shlib);
                        if (base != targetBase)
                        {
                            haveShlib[base] = true;
                        }
                    }

                    a = a__prev1;
                }

            } 

            // Arrange the deps into afiles and shlibs.
            var addedShlib = make_map<@string, bool>();
            {
                var a__prev1 = a;

                foreach (var (_, __a) in root.Deps)
                {
                    a = __a;
                    p = a.Package;
                    if (p != null && p.Shlib != "" && haveShlib[filepath.Base(p.Shlib)])
                    { 
                        // This is a package linked into a shared
                        // library that we will put into shlibs.
                        continue;
                    }
                    if (haveShlib[filepath.Base(a.Target)])
                    { 
                        // This is a shared library we want to link againt.
                        if (!addedShlib[a.Target])
                        {
                            shlibs = append(shlibs, a.Target);
                            addedShlib[a.Target] = true;
                        }
                        continue;
                    }
                    if (p != null)
                    {
                        var target = a.built;
                        if (p.UsesCgo() || p.UsesSwig())
                        {
                            err = default;
                            target, err = readAndRemoveCgoFlags(target);
                            if (err != null)
                            {
                                continue;
                            }
                        }
                        afiles = append(afiles, target);
                    }
                }

                a = a__prev1;
            }

            {
                var a__prev1 = a;

                foreach (var (_, __a) in allactions)
                {
                    a = __a; 
                    // Gather CgoLDFLAGS, but not from standard packages.
                    // The go tool can dig up runtime/cgo from GOROOT and
                    // think that it should use its CgoLDFLAGS, but gccgo
                    // doesn't use runtime/cgo.
                    if (a.Package == null)
                    {
                        continue;
                    }
                    if (!a.Package.Standard)
                    {
                        cgoldflags = append(cgoldflags, a.Package.CgoLDFLAGS);
                    }
                    if (len(a.Package.CgoFiles) > 0L)
                    {
                        usesCgo = true;
                    }
                    if (a.Package.UsesSwig())
                    {
                        usesCgo = true;
                    }
                    if (len(a.Package.CXXFiles) > 0L || len(a.Package.SwigCXXFiles) > 0L)
                    {
                        cxx = true;
                    }
                    if (len(a.Package.MFiles) > 0L)
                    {
                        objc = true;
                    }
                    if (len(a.Package.FFiles) > 0L)
                    {
                        fortran = true;
                    }
                }

                a = a__prev1;
            }

            ldflags = append(ldflags, "-Wl,--whole-archive");
            ldflags = append(ldflags, afiles);
            ldflags = append(ldflags, "-Wl,--no-whole-archive");

            ldflags = append(ldflags, cgoldflags);
            ldflags = append(ldflags, envList("CGO_LDFLAGS", ""));
            if (root.Package != null)
            {
                ldflags = append(ldflags, root.Package.CgoLDFLAGS);
            }
            ldflags = str.StringList("-Wl,-(", ldflags, "-Wl,-)");

            if (root.buildID != "")
            { 
                // On systems that normally use gold or the GNU linker,
                // use the --build-id option to write a GNU build ID note.
                switch (cfg.Goos)
                {
                    case "android": 

                    case "dragonfly": 

                    case "linux": 

                    case "netbsd": 
                        ldflags = append(ldflags, fmt.Sprintf("-Wl,--build-id=0x%x", root.buildID));
                        break;
                }
            }
            foreach (var (_, shlib) in shlibs)
            {
                ldflags = append(ldflags, "-L" + filepath.Dir(shlib), "-Wl,-rpath=" + filepath.Dir(shlib), "-l" + strings.TrimSuffix(strings.TrimPrefix(filepath.Base(shlib), "lib"), ".so"));
            }
            @string realOut = default;
            switch (buildmode)
            {
                case "exe": 
                    if (usesCgo && cfg.Goos == "linux")
                    {
                        ldflags = append(ldflags, "-Wl,-E");
                    }
                    break;
                case "c-archive": 
                    // Link the Go files into a single .o, and also link
                    // in -lgolibbegin.
                    //
                    // We need to use --whole-archive with -lgolibbegin
                    // because it doesn't define any symbols that will
                    // cause the contents to be pulled in; it's just
                    // initialization code.
                    //
                    // The user remains responsible for linking against
                    // -lgo -lpthread -lm in the final link. We can't use
                    // -r to pick them up because we can't combine
                    // split-stack and non-split-stack code in a single -r
                    // link, and libgo picks up non-split-stack code from
                    // libffi.
                    ldflags = append(ldflags, "-Wl,-r", "-nostdlib", "-Wl,--whole-archive", "-lgolibbegin", "-Wl,--no-whole-archive");

                    {
                        var nopie = b.gccNoPie(new slice<@string>(new @string[] { tools.linker() }));

                        if (nopie != "")
                        {
                            ldflags = append(ldflags, nopie);
                        } 

                        // We are creating an object file, so we don't want a build ID.

                    } 

                    // We are creating an object file, so we don't want a build ID.
                    if (root.buildID == "")
                    {
                        ldflags = b.disableBuildID(ldflags);
                    }
                    realOut = out;
                    out = out + ".o";
                    break;
                case "c-shared": 
                    ldflags = append(ldflags, "-shared", "-nostdlib", "-Wl,--whole-archive", "-lgolibbegin", "-Wl,--no-whole-archive", "-lgo", "-lgcc_s", "-lgcc", "-lc", "-lgcc");
                    break;
                case "shared": 
                    ldflags = append(ldflags, "-zdefs", "-shared", "-nostdlib", "-lgo", "-lgcc_s", "-lgcc", "-lc");
                    break;
                default: 
                    @base.Fatalf("-buildmode=%s not supported for gccgo", buildmode);
                    break;
            }

            switch (buildmode)
            {
                case "exe": 

                case "c-shared": 
                    if (cxx)
                    {
                        ldflags = append(ldflags, "-lstdc++");
                    }
                    if (objc)
                    {
                        ldflags = append(ldflags, "-lobjc");
                    }
                    if (fortran)
                    {
                        var fc = os.Getenv("FC");
                        if (fc == "")
                        {
                            fc = "gfortran";
                        } 
                        // support gfortran out of the box and let others pass the correct link options
                        // via CGO_LDFLAGS
                        if (strings.Contains(fc, "gfortran"))
                        {
                            ldflags = append(ldflags, "-lgfortran");
                        }
                    }
                    break;
            }

            {
                var err__prev1 = err;

                err = b.run(root, ".", desc, null, tools.linker(), "-o", out, ldflags, forcedGccgoflags, root.Package.Internal.Gccgoflags);

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }

            switch (buildmode)
            {
                case "c-archive": 
                    {
                        var err__prev1 = err;

                        err = b.run(root, ".", desc, null, "ar", "rc", realOut, out);

                        if (err != null)
                        {
                            return error.As(err);
                        }

                        err = err__prev1;

                    }
                    break;
            }
            return error.As(null);
        }

        private static error ld(this gccgoToolchain tools, ref Builder b, ref Action root, @string @out, @string importcfg, @string mainpkg)
        {
            return error.As(tools.link(b, root, out, importcfg, root.Deps, ldBuildmode, root.Package.ImportPath));
        }

        private static error ldShared(this gccgoToolchain tools, ref Builder b, ref Action root, slice<ref Action> toplevelactions, @string @out, @string importcfg, slice<ref Action> allactions)
        {
            return error.As(tools.link(b, root, out, importcfg, allactions, "shared", out));
        }

        private static error cc(this gccgoToolchain tools, ref Builder b, ref Action a, @string ofile, @string cfile)
        {
            var p = a.Package;
            var inc = filepath.Join(cfg.GOROOT, "pkg", "include");
            cfile = mkAbs(p.Dir, cfile);
            @string defs = new slice<@string>(new @string[] { "-D", "GOOS_"+cfg.Goos, "-D", "GOARCH_"+cfg.Goarch });
            defs = append(defs, b.gccArchArgs());
            {
                var pkgpath = gccgoCleanPkgpath(p);

                if (pkgpath != "")
                {
                    defs = append(defs, "-D", "GOPKGPATH=\"" + pkgpath + "\"");
                }

            }
            switch (cfg.Goarch)
            {
                case "386": 

                case "amd64": 
                    defs = append(defs, "-fsplit-stack");
                    break;
            }
            defs = tools.maybePIC(defs);
            return error.As(b.run(a, p.Dir, p.ImportPath, null, envList("CC", cfg.DefaultCC(cfg.Goos, cfg.Goarch)), "-Wall", "-g", "-I", a.Objdir, "-I", inc, "-o", ofile, defs, "-c", cfile));
        }

        // maybePIC adds -fPIC to the list of arguments if needed.
        private static slice<@string> maybePIC(this gccgoToolchain tools, slice<@string> args)
        {
            switch (cfg.BuildBuildmode)
            {
                case "c-shared": 

                case "shared": 

                case "plugin": 
                    args = append(args, "-fPIC");
                    break;
            }
            return args;
        }

        private static @string gccgoPkgpath(ref load.Package p)
        {
            if (p.Internal.Build.IsCommand() && !p.Internal.ForceLibrary)
            {
                return "";
            }
            return p.ImportPath;
        }

        private static @string gccgoCleanPkgpath(ref load.Package p)
        {
            Func<int, int> clean = r =>
            {

                if ('A' <= r && r <= 'Z' || 'a' <= r && r <= 'z' || '0' <= r && r <= '9') 
                    return r;
                                return '_';
            }
;
            return strings.Map(clean, gccgoPkgpath(p));
        }
    }
}}}}
