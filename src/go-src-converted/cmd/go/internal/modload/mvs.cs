// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modload -- go2cs converted at 2020 October 09 05:46:54 UTC
// import "cmd/go/internal/modload" ==> using modload = go.cmd.go.@internal.modload_package
// Original source: C:\Go\src\cmd\go\internal\modload\mvs.go
using errors = go.errors_package;
using fmt = go.fmt_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using sort = go.sort_package;
using sync = go.sync_package;

using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using lockedfile = go.cmd.go.@internal.lockedfile_package;
using modfetch = go.cmd.go.@internal.modfetch_package;
using mvs = go.cmd.go.@internal.mvs_package;
using par = go.cmd.go.@internal.par_package;

using modfile = go.golang.org.x.mod.modfile_package;
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
        // mvsReqs implements mvs.Reqs for module semantic versions,
        // with any exclusions or replacements applied internally.
        private partial struct mvsReqs
        {
            public slice<module.Version> buildList;
            public par.Cache cache;
            public sync.Map versions;
        }

        // Reqs returns the current module requirement graph.
        // Future calls to SetBuildList do not affect the operation
        // of the returned Reqs.
        public static mvs.Reqs Reqs()
        {
            ptr<mvsReqs> r = addr(new mvsReqs(buildList:buildList,));
            return r;
        }

        private static (slice<module.Version>, error) Required(this ptr<mvsReqs> _addr_r, module.Version mod)
        {
            slice<module.Version> _p0 = default;
            error _p0 = default!;
            ref mvsReqs r = ref _addr_r.val;

            private partial struct cached
            {
                public slice<module.Version> list;
                public error err;
            }

            cached c = r.cache.Do(mod, () =>
            {
                var (list, err) = r.required(mod);
                if (err != null)
                {
                    return new cached(nil,err);
                }

                foreach (var (i, mv) in list)
                {
                    if (index != null)
                    {
                        while (index.exclude[mv])
                        {
                            var (mv1, err) = r.next(mv);
                            if (err != null)
                            {
                                return new cached(nil,err);
                            }

                            if (mv1.Version == "none")
                            {
                                return new cached(nil,fmt.Errorf("%s(%s) depends on excluded %s(%s) with no newer version available",mod.Path,mod.Version,mv.Path,mv.Version));
                            }

                            mv = mv1;

                        }


                    }

                    list[i] = mv;

                }
                return new cached(list,nil);

            })._<cached>();

            return (c.list, error.As(c.err)!);

        }

        private static slice<module.Version> modFileToList(this ptr<mvsReqs> _addr_r, ptr<modfile.File> _addr_f)
        {
            ref mvsReqs r = ref _addr_r.val;
            ref modfile.File f = ref _addr_f.val;

            var list = make_slice<module.Version>(0L, len(f.Require));
            foreach (var (_, r) in f.Require)
            {
                list = append(list, r.Mod);
            }
            return list;

        }

        // required returns a unique copy of the requirements of mod.
        private static (slice<module.Version>, error) required(this ptr<mvsReqs> _addr_r, module.Version mod)
        {
            slice<module.Version> _p0 = default;
            error _p0 = default!;
            ref mvsReqs r = ref _addr_r.val;

            if (mod == Target)
            {
                if (modFile != null && modFile.Go != null)
                {
                    r.versions.LoadOrStore(mod, modFile.Go.Version);
                }

                return (append((slice<module.Version>)null, r.buildList[1L..]), error.As(null!)!);

            }

            if (cfg.BuildMod == "vendor")
            { 
                // For every module other than the target,
                // return the full list of modules from modules.txt.
                readVendorList();
                return (append((slice<module.Version>)null, vendorList), error.As(null!)!);

            }

            var origPath = mod.Path;
            {
                var repl = Replacement(mod);

                if (repl.Path != "")
                {
                    if (repl.Version == "")
                    { 
                        // TODO: need to slip the new version into the tags list etc.
                        var dir = repl.Path;
                        if (!filepath.IsAbs(dir))
                        {
                            dir = filepath.Join(ModRoot(), dir);
                        }

                        var gomod = filepath.Join(dir, "go.mod");
                        var (data, err) = lockedfile.Read(gomod);
                        if (err != null)
                        {
                            return (null, error.As(fmt.Errorf("parsing %s: %v", @base.ShortPath(gomod), err))!);
                        }

                        var (f, err) = modfile.ParseLax(gomod, data, null);
                        if (err != null)
                        {
                            return (null, error.As(fmt.Errorf("parsing %s: %v", @base.ShortPath(gomod), err))!);
                        }

                        if (f.Go != null)
                        {
                            r.versions.LoadOrStore(mod, f.Go.Version);
                        }

                        return (r.modFileToList(f), error.As(null!)!);

                    }

                    mod = repl;

                }

            }


            if (mod.Version == "none")
            {
                return (null, error.As(null!)!);
            }

            if (!semver.IsValid(mod.Version))
            { 
                // Disallow the broader queries supported by fetch.Lookup.
                @base.Fatalf("go: internal error: %s@%s: unexpected invalid semantic version", mod.Path, mod.Version);

            }

            (data, err) = modfetch.GoMod(mod.Path, mod.Version);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            (f, err) = modfile.ParseLax("go.mod", data, null);
            if (err != null)
            {
                return (null, error.As(module.VersionError(mod, fmt.Errorf("parsing go.mod: %v", err)))!);
            }

            if (f.Module == null)
            {
                return (null, error.As(module.VersionError(mod, errors.New("parsing go.mod: missing module line")))!);
            }

            {
                var mpath = f.Module.Mod.Path;

                if (mpath != origPath && mpath != mod.Path)
                {
                    return (null, error.As(module.VersionError(mod, fmt.Errorf("parsing go.mod:\n\tmodule declares its path as: %s\n\t        but was required as: %s" +
    "", mpath, origPath)))!);

                }

            }

            if (f.Go != null)
            {
                r.versions.LoadOrStore(mod, f.Go.Version);
            }

            return (r.modFileToList(f), error.As(null!)!);

        }

        // Max returns the maximum of v1 and v2 according to semver.Compare.
        //
        // As a special case, the version "" is considered higher than all other
        // versions. The main module (also known as the target) has no version and must
        // be chosen over other versions of the same module in the module dependency
        // graph.
        private static @string Max(this ptr<mvsReqs> _addr__p0, @string v1, @string v2)
        {
            ref mvsReqs _p0 = ref _addr__p0.val;

            if (v1 != "" && semver.Compare(v1, v2) == -1L)
            {
                return v2;
            }

            return v1;

        }

        // Upgrade is a no-op, here to implement mvs.Reqs.
        // The upgrade logic for go get -u is in ../modget/get.go.
        private static (module.Version, error) Upgrade(this ptr<mvsReqs> _addr__p0, module.Version m)
        {
            module.Version _p0 = default;
            error _p0 = default!;
            ref mvsReqs _p0 = ref _addr__p0.val;

            return (m, error.As(null!)!);
        }

        private static (slice<@string>, error) versions(@string path)
        {
            slice<@string> _p0 = default;
            error _p0 = default!;
 
            // Note: modfetch.Lookup and repo.Versions are cached,
            // so there's no need for us to add extra caching here.
            slice<@string> versions = default;
            var err = modfetch.TryProxies(proxy =>
            {
                var (repo, err) = modfetch.Lookup(proxy, path);
                if (err == null)
                {
                    versions, err = repo.Versions("");
                }

                return err;

            });
            return (versions, error.As(err)!);

        }

        // Previous returns the tagged version of m.Path immediately prior to
        // m.Version, or version "none" if no prior version is tagged.
        private static (module.Version, error) Previous(this ptr<mvsReqs> _addr__p0, module.Version m)
        {
            module.Version _p0 = default;
            error _p0 = default!;
            ref mvsReqs _p0 = ref _addr__p0.val;

            var (list, err) = versions(m.Path);
            if (err != null)
            {
                return (new module.Version(), error.As(err)!);
            }

            var i = sort.Search(len(list), i => semver.Compare(list[i], m.Version) >= 0L);
            if (i > 0L)
            {
                return (new module.Version(Path:m.Path,Version:list[i-1]), error.As(null!)!);
            }

            return (new module.Version(Path:m.Path,Version:"none"), error.As(null!)!);

        }

        // next returns the next version of m.Path after m.Version.
        // It is only used by the exclusion processing in the Required method,
        // not called directly by MVS.
        private static (module.Version, error) next(this ptr<mvsReqs> _addr__p0, module.Version m)
        {
            module.Version _p0 = default;
            error _p0 = default!;
            ref mvsReqs _p0 = ref _addr__p0.val;

            var (list, err) = versions(m.Path);
            if (err != null)
            {
                return (new module.Version(), error.As(err)!);
            }

            var i = sort.Search(len(list), i => semver.Compare(list[i], m.Version) > 0L);
            if (i < len(list))
            {
                return (new module.Version(Path:m.Path,Version:list[i]), error.As(null!)!);
            }

            return (new module.Version(Path:m.Path,Version:"none"), error.As(null!)!);

        }

        // fetch downloads the given module (or its replacement)
        // and returns its location.
        //
        // The isLocal return value reports whether the replacement,
        // if any, is local to the filesystem.
        private static (@string, bool, error) fetch(module.Version mod)
        {
            @string dir = default;
            bool isLocal = default;
            error err = default!;

            if (mod == Target)
            {
                return (ModRoot(), true, error.As(null!)!);
            }

            {
                var r = Replacement(mod);

                if (r.Path != "")
                {
                    if (r.Version == "")
                    {
                        dir = r.Path;
                        if (!filepath.IsAbs(dir))
                        {
                            dir = filepath.Join(ModRoot(), dir);
                        } 
                        // Ensure that the replacement directory actually exists:
                        // dirInModule does not report errors for missing modules,
                        // so if we don't report the error now, later failures will be
                        // very mysterious.
                        {
                            var (_, err) = os.Stat(dir);

                            if (err != null)
                            {
                                if (os.IsNotExist(err))
                                { 
                                    // Semantically the module version itself “exists” — we just don't
                                    // have its source code. Remove the equivalence to os.ErrNotExist,
                                    // and make the message more concise while we're at it.
                                    err = fmt.Errorf("replacement directory %s does not exist", r.Path);

                                }
                                else
                                {
                                    err = fmt.Errorf("replacement directory %s: %w", r.Path, err);
                                }

                                return (dir, true, error.As(module.VersionError(mod, err))!);

                            }

                        }

                        return (dir, true, error.As(null!)!);

                    }

                    mod = r;

                }

            }


            dir, err = modfetch.Download(mod);
            return (dir, false, error.As(err)!);

        }
    }
}}}}
