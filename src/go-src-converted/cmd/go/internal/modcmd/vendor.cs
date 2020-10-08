// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modcmd -- go2cs converted at 2020 October 08 04:36:46 UTC
// import "cmd/go/internal/modcmd" ==> using modcmd = go.cmd.go.@internal.modcmd_package
// Original source: C:\Go\src\cmd\go\internal\modcmd\vendor.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using sort = go.sort_package;
using strings = go.strings_package;

using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using imports = go.cmd.go.@internal.imports_package;
using modload = go.cmd.go.@internal.modload_package;
using work = go.cmd.go.@internal.work_package;

using module = go.golang.org.x.mod.module_package;
using semver = go.golang.org.x.mod.semver_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class modcmd_package
    {
        private static ptr<base.Command> cmdVendor = addr(new base.Command(UsageLine:"go mod vendor [-v]",Short:"make vendored copy of dependencies",Long:`
Vendor resets the main module's vendor directory to include all packages
needed to build and test all the main module's packages.
It does not include test code for vendored packages.

The -v flag causes vendor to print the names of vendored
modules and packages to standard error.
	`,Run:runVendor,));

        private static void init()
        {
            cmdVendor.Flag.BoolVar(_addr_cfg.BuildV, "v", false, "");
            work.AddModCommonFlags(cmdVendor);
        }

        private static void runVendor(ptr<base.Command> _addr_cmd, slice<@string> args)
        {
            ref base.Command cmd = ref _addr_cmd.val;

            if (len(args) != 0L)
            {
                @base.Fatalf("go mod vendor: vendor takes no arguments");
            }

            var pkgs = modload.LoadVendor();

            var vdir = filepath.Join(modload.ModRoot(), "vendor");
            {
                var err__prev1 = err;

                var err = os.RemoveAll(vdir);

                if (err != null)
                {
                    @base.Fatalf("go mod vendor: %v", err);
                }

                err = err__prev1;

            }


            var modpkgs = make_map<module.Version, slice<@string>>();
            {
                var pkg__prev1 = pkg;

                foreach (var (_, __pkg) in pkgs)
                {
                    pkg = __pkg;
                    var m = modload.PackageModule(pkg);
                    if (m == modload.Target)
                    {
                        continue;
                    }

                    modpkgs[m] = append(modpkgs[m], pkg);

                }

                pkg = pkg__prev1;
            }

            var includeAllReplacements = false;
            map isExplicit = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<module.Version, bool>{};
            {
                var gv = modload.ModFile().Go;

                if (gv != null && semver.Compare("v" + gv.Version, "v1.14") >= 0L)
                { 
                    // If the Go version is at least 1.14, annotate all explicit 'require' and
                    // 'replace' targets found in the go.mod file so that we can perform a
                    // stronger consistency check when -mod=vendor is set.
                    {
                        var r__prev1 = r;

                        foreach (var (_, __r) in modload.ModFile().Require)
                        {
                            r = __r;
                            isExplicit[r.Mod] = true;
                        }

                        r = r__prev1;
                    }

                    includeAllReplacements = true;

                }

            }


            ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
            {
                var m__prev1 = m;

                foreach (var (_, __m) in modload.BuildList()[1L..])
                {
                    m = __m;
                    {
                        var pkgs__prev1 = pkgs;

                        pkgs = modpkgs[m];

                        if (len(pkgs) > 0L || isExplicit[m])
                        {
                            var line = moduleLine(m, modload.Replacement(m));
                            buf.WriteString(line);
                            if (cfg.BuildV)
                            {
                                os.Stderr.WriteString(line);
                            }

                            if (isExplicit[m])
                            {
                                buf.WriteString("## explicit\n");
                                if (cfg.BuildV)
                                {
                                    os.Stderr.WriteString("## explicit\n");
                                }

                            }

                            sort.Strings(pkgs);
                            {
                                var pkg__prev2 = pkg;

                                foreach (var (_, __pkg) in pkgs)
                                {
                                    pkg = __pkg;
                                    fmt.Fprintf(_addr_buf, "%s\n", pkg);
                                    if (cfg.BuildV)
                                    {
                                        fmt.Fprintf(os.Stderr, "%s\n", pkg);
                                    }

                                    vendorPkg(vdir, pkg);

                                }

                                pkg = pkg__prev2;
                            }
                        }

                        pkgs = pkgs__prev1;

                    }

                }

                m = m__prev1;
            }

            if (includeAllReplacements)
            { 
                // Record unused and wildcard replacements at the end of the modules.txt file:
                // without access to the complete build list, the consumer of the vendor
                // directory can't otherwise determine that those replacements had no effect.
                {
                    var r__prev1 = r;

                    foreach (var (_, __r) in modload.ModFile().Replace)
                    {
                        r = __r;
                        if (len(modpkgs[r.Old]) > 0L)
                        { 
                            // We we already recorded this replacement in the entry for the replaced
                            // module with the packages it provides.
                            continue;

                        }

                        line = moduleLine(r.Old, r.New);
                        buf.WriteString(line);
                        if (cfg.BuildV)
                        {
                            os.Stderr.WriteString(line);
                        }

                    }

                    r = r__prev1;
                }
            }

            if (buf.Len() == 0L)
            {
                fmt.Fprintf(os.Stderr, "go: no dependencies to vendor\n");
                return ;
            }

            {
                var err__prev1 = err;

                err = os.MkdirAll(vdir, 0777L);

                if (err != null)
                {
                    @base.Fatalf("go mod vendor: %v", err);
                }

                err = err__prev1;

            }


            {
                var err__prev1 = err;

                err = ioutil.WriteFile(filepath.Join(vdir, "modules.txt"), buf.Bytes(), 0666L);

                if (err != null)
                {
                    @base.Fatalf("go mod vendor: %v", err);
                }

                err = err__prev1;

            }

        }

        private static @string moduleLine(module.Version m, module.Version r)
        {
            ptr<object> b = @new<strings.Builder>();
            b.WriteString("# ");
            b.WriteString(m.Path);
            if (m.Version != "")
            {
                b.WriteString(" ");
                b.WriteString(m.Version);
            }

            if (r.Path != "")
            {
                b.WriteString(" => ");
                b.WriteString(r.Path);
                if (r.Version != "")
                {
                    b.WriteString(" ");
                    b.WriteString(r.Version);
                }

            }

            b.WriteString("\n");
            return b.String();

        }

        private static void vendorPkg(@string vdir, @string pkg)
        {
            var realPath = modload.ImportMap(pkg);
            if (realPath != pkg && modload.ImportMap(realPath) != "")
            {
                fmt.Fprintf(os.Stderr, "warning: %s imported as both %s and %s; making two copies.\n", realPath, realPath, pkg);
            }

            var dst = filepath.Join(vdir, pkg);
            var src = modload.PackageDir(realPath);
            if (src == "")
            {
                fmt.Fprintf(os.Stderr, "internal error: no pkg for %s -> %s\n", pkg, realPath);
            }

            copyDir(dst, src, matchPotentialSourceFile);
            {
                var m = modload.PackageModule(realPath);

                if (m.Path != "")
                {
                    copyMetadata(m.Path, realPath, dst, src);
                }

            }

        }

        private partial struct metakey
        {
            public @string modPath;
            public @string dst;
        }

        private static var copiedMetadata = make_map<metakey, bool>();

        // copyMetadata copies metadata files from parents of src to parents of dst,
        // stopping after processing the src parent for modPath.
        private static void copyMetadata(@string modPath, @string pkg, @string dst, @string src)
        {
            for (long parent = 0L; >>MARKER:FOREXPRESSION_LEVEL_1<<; parent++)
            {
                if (copiedMetadata[new metakey(modPath,dst)])
                {
                    break;
                }

                copiedMetadata[new metakey(modPath,dst)] = true;
                if (parent > 0L)
                {
                    copyDir(dst, src, matchMetadata);
                }

                if (modPath == pkg)
                {
                    break;
                }

                pkg = filepath.Dir(pkg);
                dst = filepath.Dir(dst);
                src = filepath.Dir(src);

            }


        }

        // metaPrefixes is the list of metadata file prefixes.
        // Vendoring copies metadata files from parents of copied directories.
        // Note that this list could be arbitrarily extended, and it is longer
        // in other tools (such as godep or dep). By using this limited set of
        // prefixes and also insisting on capitalized file names, we are trying
        // to nudge people toward more agreement on the naming
        // and also trying to avoid false positives.
        private static @string metaPrefixes = new slice<@string>(new @string[] { "AUTHORS", "CONTRIBUTORS", "COPYLEFT", "COPYING", "COPYRIGHT", "LEGAL", "LICENSE", "NOTICE", "PATENTS" });

        // matchMetadata reports whether info is a metadata file.
        private static bool matchMetadata(@string dir, os.FileInfo info)
        {
            var name = info.Name();
            foreach (var (_, p) in metaPrefixes)
            {
                if (strings.HasPrefix(name, p))
                {
                    return true;
                }

            }
            return false;

        }

        // matchPotentialSourceFile reports whether info may be relevant to a build operation.
        private static bool matchPotentialSourceFile(@string dir, os.FileInfo info) => func((defer, _, __) =>
        {
            if (strings.HasSuffix(info.Name(), "_test.go"))
            {
                return false;
            }

            if (strings.HasSuffix(info.Name(), ".go"))
            {
                var (f, err) = os.Open(filepath.Join(dir, info.Name()));
                if (err != null)
                {
                    @base.Fatalf("go mod vendor: %v", err);
                }

                defer(f.Close());

                var (content, err) = imports.ReadImports(f, false, null);
                if (err == null && !imports.ShouldBuild(content, imports.AnyTags()))
                { 
                    // The file is explicitly tagged "ignore", so it can't affect the build.
                    // Leave it out.
                    return false;

                }

                return true;

            } 

            // We don't know anything about this file, so optimistically assume that it is
            // needed.
            return true;

        });

        // copyDir copies all regular files satisfying match(info) from src to dst.
        private static bool copyDir(@string dst, @string src, Func<@string, os.FileInfo, bool> match)
        {
            var (files, err) = ioutil.ReadDir(src);
            if (err != null)
            {
                @base.Fatalf("go mod vendor: %v", err);
            }

            {
                var err__prev1 = err;

                var err = os.MkdirAll(dst, 0777L);

                if (err != null)
                {
                    @base.Fatalf("go mod vendor: %v", err);
                }

                err = err__prev1;

            }

            foreach (var (_, file) in files)
            {
                if (file.IsDir() || !file.Mode().IsRegular() || !match(src, file))
                {
                    continue;
                }

                var (r, err) = os.Open(filepath.Join(src, file.Name()));
                if (err != null)
                {
                    @base.Fatalf("go mod vendor: %v", err);
                }

                var (w, err) = os.Create(filepath.Join(dst, file.Name()));
                if (err != null)
                {
                    @base.Fatalf("go mod vendor: %v", err);
                }

                {
                    var err__prev1 = err;

                    var (_, err) = io.Copy(w, r);

                    if (err != null)
                    {
                        @base.Fatalf("go mod vendor: %v", err);
                    }

                    err = err__prev1;

                }

                r.Close();
                {
                    var err__prev1 = err;

                    err = w.Close();

                    if (err != null)
                    {
                        @base.Fatalf("go mod vendor: %v", err);
                    }

                    err = err__prev1;

                }

            }

        }
    }
}}}}
