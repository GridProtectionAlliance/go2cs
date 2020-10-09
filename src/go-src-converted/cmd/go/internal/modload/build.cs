// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modload -- go2cs converted at 2020 October 09 05:45:20 UTC
// import "cmd/go/internal/modload" ==> using modload = go.cmd.go.@internal.modload_package
// Original source: C:\Go\src\cmd\go\internal\modload\build.go
using bytes = go.bytes_package;
using hex = go.encoding.hex_package;
using fmt = go.fmt_package;
using goroot = go.@internal.goroot_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using debug = go.runtime.debug_package;
using strings = go.strings_package;

using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using modfetch = go.cmd.go.@internal.modfetch_package;
using modinfo = go.cmd.go.@internal.modinfo_package;
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


        private static bool isStandardImportPath(@string path)
        {
            return findStandardImportPath(path) != "";
        }

        private static @string findStandardImportPath(@string path) => func((_, panic, __) =>
        {
            if (path == "")
            {
                panic("findStandardImportPath called with empty path");
            }

            if (search.IsStandardImportPath(path))
            {
                if (goroot.IsStandardPackage(cfg.GOROOT, cfg.BuildContext.Compiler, path))
                {
                    return filepath.Join(cfg.GOROOT, "src", path);
                }

            }

            return "";

        });

        // PackageModuleInfo returns information about the module that provides
        // a given package. If modules are not enabled or if the package is in the
        // standard library or if the package was not successfully loaded with
        // ImportPaths or a similar loading function, nil is returned.
        public static ptr<modinfo.ModulePublic> PackageModuleInfo(@string pkgpath)
        {
            if (isStandardImportPath(pkgpath) || !Enabled())
            {
                return _addr_null!;
            }

            var (m, ok) = findModule(pkgpath);
            if (!ok)
            {
                return _addr_null!;
            }

            return _addr_moduleInfo(m, true)!;

        }

        public static ptr<modinfo.ModulePublic> ModuleInfo(@string path)
        {
            if (!Enabled())
            {
                return _addr_null!;
            }

            {
                var i = strings.Index(path, "@");

                if (i >= 0L)
                {
                    return _addr_moduleInfo(new module.Version(Path:path[:i],Version:path[i+1:]), false)!;
                }

            }


            foreach (var (_, m) in BuildList())
            {
                if (m.Path == path)
                {
                    return _addr_moduleInfo(m, true)!;
                }

            }
            return addr(new modinfo.ModulePublic(Path:path,Error:&modinfo.ModuleError{Err:"module not in current build",},));

        }

        // addUpdate fills in m.Update if an updated version is available.
        private static void addUpdate(ptr<modinfo.ModulePublic> _addr_m)
        {
            ref modinfo.ModulePublic m = ref _addr_m.val;

            if (m.Version == "")
            {
                return ;
            }

            {
                var (info, err) = Query(m.Path, "upgrade", m.Version, Allowed);

                if (err == null && semver.Compare(info.Version, m.Version) > 0L)
                {
                    m.Update = addr(new modinfo.ModulePublic(Path:m.Path,Version:info.Version,Time:&info.Time,));
                }

            }

        }

        // addVersions fills in m.Versions with the list of known versions.
        private static void addVersions(ptr<modinfo.ModulePublic> _addr_m)
        {
            ref modinfo.ModulePublic m = ref _addr_m.val;

            m.Versions, _ = versions(m.Path);
        }

        private static ptr<modinfo.ModulePublic> moduleInfo(module.Version m, bool fromBuildList)
        {
            if (m == Target)
            {
                ptr<modinfo.ModulePublic> info = addr(new modinfo.ModulePublic(Path:m.Path,Version:m.Version,Main:true,));
                if (HasModRoot())
                {
                    info.Dir = ModRoot();
                    info.GoMod = ModFilePath();
                    if (modFile.Go != null)
                    {
                        info.GoVersion = modFile.Go.Version;
                    }

                }

                return _addr_info!;

            }

            info = addr(new modinfo.ModulePublic(Path:m.Path,Version:m.Version,Indirect:fromBuildList&&loaded!=nil&&!loaded.direct[m.Path],));
            if (loaded != null)
            {
                info.GoVersion = loaded.goVersion[m.Path];
            } 

            // completeFromModCache fills in the extra fields in m using the module cache.
            Action<ptr<modinfo.ModulePublic>> completeFromModCache = m =>
            {
                if (m.Version != "")
                {
                    {
                        var (q, err) = Query(m.Path, m.Version, "", null);

                        if (err != null)
                        {
                            m.Error = addr(new modinfo.ModuleError(Err:err.Error()));
                        }
                        else
                        {
                            m.Version = q.Version;
                            m.Time = _addr_q.Time;
                        }

                    }


                    module.Version mod = new module.Version(Path:m.Path,Version:m.Version);
                    var (gomod, err) = modfetch.CachePath(mod, "mod");
                    if (err == null)
                    {
                        {
                            ptr<modinfo.ModulePublic> info__prev3 = info;

                            var (info, err) = os.Stat(gomod);

                            if (err == null && info.Mode().IsRegular())
                            {
                                m.GoMod = gomod;
                            }

                            info = info__prev3;

                        }

                    }

                    var (dir, err) = modfetch.DownloadDir(mod);
                    if (err == null)
                    {
                        m.Dir = dir;
                    }

                }

            }
;

            if (!fromBuildList)
            {
                completeFromModCache(info); // Will set m.Error in vendor mode.
                return _addr_info!;

            }

            var r = Replacement(m);
            if (r.Path == "")
            {
                if (cfg.BuildMod == "vendor")
                { 
                    // It's tempting to fill in the "Dir" field to point within the vendor
                    // directory, but that would be misleading: the vendor directory contains
                    // a flattened package tree, not complete modules, and it can even
                    // interleave packages from different modules if one module path is a
                    // prefix of the other.
                }
                else
                {
                    completeFromModCache(info);
                }

                return _addr_info!;

            } 

            // Don't hit the network to fill in extra data for replaced modules.
            // The original resolved Version and Time don't matter enough to be
            // worth the cost, and we're going to overwrite the GoMod and Dir from the
            // replacement anyway. See https://golang.org/issue/27859.
            info.Replace = addr(new modinfo.ModulePublic(Path:r.Path,Version:r.Version,GoVersion:info.GoVersion,));
            if (r.Version == "")
            {
                if (filepath.IsAbs(r.Path))
                {
                    info.Replace.Dir = r.Path;
                }
                else
                {
                    info.Replace.Dir = filepath.Join(ModRoot(), r.Path);
                }

                info.Replace.GoMod = filepath.Join(info.Replace.Dir, "go.mod");

            }

            if (cfg.BuildMod != "vendor")
            {
                completeFromModCache(info.Replace);
                info.Dir = info.Replace.Dir;
                info.GoMod = info.Replace.GoMod;
            }

            return _addr_info!;

        }

        // PackageBuildInfo returns a string containing module version information
        // for modules providing packages named by path and deps. path and deps must
        // name packages that were resolved successfully with ImportPaths or one of
        // the Load functions.
        public static @string PackageBuildInfo(@string path, slice<@string> deps)
        {
            if (isStandardImportPath(path) || !Enabled())
            {
                return "";
            }

            var target = mustFindModule(path, path);
            var mdeps = make_map<module.Version, bool>();
            foreach (var (_, dep) in deps)
            {
                if (!isStandardImportPath(dep))
                {
                    mdeps[mustFindModule(path, dep)] = true;
                }

            }
            slice<module.Version> mods = default;
            delete(mdeps, target);
            {
                var mod__prev1 = mod;

                foreach (var (__mod) in mdeps)
                {
                    mod = __mod;
                    mods = append(mods, mod);
                }

                mod = mod__prev1;
            }

            module.Sort(mods);

            ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
            fmt.Fprintf(_addr_buf, "path\t%s\n", path);

            Action<@string, module.Version> writeEntry = (token, m) =>
            {
                var mv = m.Version;
                if (mv == "")
                {
                    mv = "(devel)";
                }

                fmt.Fprintf(_addr_buf, "%s\t%s\t%s", token, m.Path, mv);
                {
                    var r = Replacement(m);

                    if (r.Path == "")
                    {
                        fmt.Fprintf(_addr_buf, "\t%s\n", modfetch.Sum(m));
                    }
                    else
                    {
                        fmt.Fprintf(_addr_buf, "\n=>\t%s\t%s\t%s\n", r.Path, r.Version, modfetch.Sum(r));
                    }

                }

            }
;

            writeEntry("mod", target);
            {
                var mod__prev1 = mod;

                foreach (var (_, __mod) in mods)
                {
                    mod = __mod;
                    writeEntry("dep", mod);
                }

                mod = mod__prev1;
            }

            return buf.String();

        }

        // mustFindModule is like findModule, but it calls base.Fatalf if the
        // module can't be found.
        //
        // TODO(jayconrod): remove this. Callers should use findModule and return
        // errors instead of relying on base.Fatalf.
        private static module.Version mustFindModule(@string target, @string path) => func((_, panic, __) =>
        {
            ptr<loadPkg> (pkg, ok) = loaded.pkgCache.Get(path)._<ptr<loadPkg>>();
            if (ok)
            {
                if (pkg.err != null)
                {
                    @base.Fatalf("build %v: cannot load %v: %v", target, path, pkg.err);
                }

                return pkg.mod;

            }

            if (path == "command-line-arguments")
            {
                return Target;
            }

            if (printStackInDie)
            {
                debug.PrintStack();
            }

            @base.Fatalf("build %v: cannot find module for path %v", target, path);
            panic("unreachable");

        });

        // findModule searches for the module that contains the package at path.
        // If the package was loaded with ImportPaths or one of the other loading
        // functions, its containing module and true are returned. Otherwise,
        // module.Version{} and false are returend.
        private static (module.Version, bool) findModule(@string path)
        {
            module.Version _p0 = default;
            bool _p0 = default;

            {
                ptr<loadPkg> (pkg, ok) = loaded.pkgCache.Get(path)._<ptr<loadPkg>>();

                if (ok)
                {
                    return (pkg.mod, pkg.mod != new module.Version());
                }

            }

            if (path == "command-line-arguments")
            {
                return (Target, true);
            }

            return (new module.Version(), false);

        }

        public static slice<byte> ModInfoProg(@string info, bool isgccgo)
        { 
            // Inject a variable with the debug information as runtime.modinfo,
            // but compile it in package main so that it is specific to the binary.
            // The variable must be a literal so that it will have the correct value
            // before the initializer for package main runs.
            //
            // The runtime startup code refers to the variable, which keeps it live
            // in all binaries.
            //
            // Note: we use an alternate recipe below for gccgo (based on an
            // init function) due to the fact that gccgo does not support
            // applying a "//go:linkname" directive to a variable. This has
            // drawbacks in that other packages may want to look at the module
            // info in their init functions (see issue 29628), which won't
            // work for gccgo. See also issue 30344.

            if (!isgccgo)
            {
                return (slice<byte>)fmt.Sprintf("package main\nimport _ \"unsafe\"\n//go:linkname __debug_modinfo__ runtime.modinfo\nva" +
    "r __debug_modinfo__ = %q\n\t", string(infoStart) + info + string(infoEnd));

            }
            else
            {
                return (slice<byte>)fmt.Sprintf("package main\nimport _ \"unsafe\"\n//go:linkname __set_debug_modinfo__ runtime.setmod" +
    "info\nfunc __set_debug_modinfo__(string)\nfunc init() { __set_debug_modinfo__(%q) " +
    "}\n\t", string(infoStart) + info + string(infoEnd));

            }

        }
    }
}}}}
