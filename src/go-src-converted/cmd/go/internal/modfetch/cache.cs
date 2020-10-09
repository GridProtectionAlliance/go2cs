// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modfetch -- go2cs converted at 2020 October 09 05:47:03 UTC
// import "cmd/go/internal/modfetch" ==> using modfetch = go.cmd.go.@internal.modfetch_package
// Original source: C:\Go\src\cmd\go\internal\modfetch\cache.go
using bytes = go.bytes_package;
using json = go.encoding.json_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;

using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using lockedfile = go.cmd.go.@internal.lockedfile_package;
using codehost = go.cmd.go.@internal.modfetch.codehost_package;
using par = go.cmd.go.@internal.par_package;
using renameio = go.cmd.go.@internal.renameio_package;

using module = go.golang.org.x.mod.module_package;
using semver = go.golang.org.x.mod.semver_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class modfetch_package
    {
        private static (@string, error) cacheDir(@string path)
        {
            @string _p0 = default;
            error _p0 = default!;

            if (cfg.GOMODCACHE == "")
            { 
                // modload.Init exits if GOPATH[0] is empty, and cfg.GOMODCACHE
                // is set to GOPATH[0]/pkg/mod if GOMODCACHE is empty, so this should never happen.
                return ("", error.As(fmt.Errorf("internal error: cfg.GOMODCACHE not set"))!);

            }
            var (enc, err) = module.EscapePath(path);
            if (err != null)
            {
                return ("", error.As(err)!);
            }
            return (filepath.Join(cfg.GOMODCACHE, "cache/download", enc, "/@v"), error.As(null!)!);

        }

        public static (@string, error) CachePath(module.Version m, @string suffix)
        {
            @string _p0 = default;
            error _p0 = default!;

            var (dir, err) = cacheDir(m.Path);
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            if (!semver.IsValid(m.Version))
            {
                return ("", error.As(fmt.Errorf("non-semver module version %q", m.Version))!);
            }

            if (module.CanonicalVersion(m.Version) != m.Version)
            {
                return ("", error.As(fmt.Errorf("non-canonical module version %q", m.Version))!);
            }

            var (encVer, err) = module.EscapeVersion(m.Version);
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            return (filepath.Join(dir, encVer + "." + suffix), error.As(null!)!);

        }

        // DownloadDir returns the directory to which m should have been downloaded.
        // An error will be returned if the module path or version cannot be escaped.
        // An error satisfying errors.Is(err, os.ErrNotExist) will be returned
        // along with the directory if the directory does not exist or if the directory
        // is not completely populated.
        public static (@string, error) DownloadDir(module.Version m)
        {
            @string _p0 = default;
            error _p0 = default!;

            if (cfg.GOMODCACHE == "")
            { 
                // modload.Init exits if GOPATH[0] is empty, and cfg.GOMODCACHE
                // is set to GOPATH[0]/pkg/mod if GOMODCACHE is empty, so this should never happen.
                return ("", error.As(fmt.Errorf("internal error: cfg.GOMODCACHE not set"))!);

            }

            var (enc, err) = module.EscapePath(m.Path);
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            if (!semver.IsValid(m.Version))
            {
                return ("", error.As(fmt.Errorf("non-semver module version %q", m.Version))!);
            }

            if (module.CanonicalVersion(m.Version) != m.Version)
            {
                return ("", error.As(fmt.Errorf("non-canonical module version %q", m.Version))!);
            }

            var (encVer, err) = module.EscapeVersion(m.Version);
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            var dir = filepath.Join(cfg.GOMODCACHE, enc + "@" + encVer);
            {
                var (fi, err) = os.Stat(dir);

                if (os.IsNotExist(err))
                {
                    return (dir, error.As(err)!);
                }
                else if (err != null)
                {
                    return (dir, error.As(addr(new DownloadDirPartialError(dir,err))!)!);
                }
                else if (!fi.IsDir())
                {
                    return (dir, error.As(addr(new DownloadDirPartialError(dir,errors.New("not a directory")))!)!);
                }


            }

            var (partialPath, err) = CachePath(m, "partial");
            if (err != null)
            {
                return (dir, error.As(err)!);
            }

            {
                var (_, err) = os.Stat(partialPath);

                if (err == null)
                {
                    return (dir, error.As(addr(new DownloadDirPartialError(dir,errors.New("not completely extracted")))!)!);
                }
                else if (!os.IsNotExist(err))
                {
                    return (dir, error.As(err)!);
                }


            }

            return (dir, error.As(null!)!);

        }

        // DownloadDirPartialError is returned by DownloadDir if a module directory
        // exists but was not completely populated.
        //
        // DownloadDirPartialError is equivalent to os.ErrNotExist.
        public partial struct DownloadDirPartialError
        {
            public @string Dir;
            public error Err;
        }

        private static @string Error(this ptr<DownloadDirPartialError> _addr_e)
        {
            ref DownloadDirPartialError e = ref _addr_e.val;

            return fmt.Sprintf("%s: %v", e.Dir, e.Err);
        }
        private static bool Is(this ptr<DownloadDirPartialError> _addr_e, error err)
        {
            ref DownloadDirPartialError e = ref _addr_e.val;

            return err == os.ErrNotExist;
        }

        // lockVersion locks a file within the module cache that guards the downloading
        // and extraction of the zipfile for the given module version.
        private static (Action, error) lockVersion(module.Version mod)
        {
            Action unlock = default;
            error err = default!;

            var (path, err) = CachePath(mod, "lock");
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            {
                var err = os.MkdirAll(filepath.Dir(path), 0777L);

                if (err != null)
                {
                    return (null, error.As(err)!);
                }

            }

            return lockedfile.MutexAt(path).Lock();

        }

        // SideLock locks a file within the module cache that that previously guarded
        // edits to files outside the cache, such as go.sum and go.mod files in the
        // user's working directory.
        // If err is nil, the caller MUST eventually call the unlock function.
        public static (Action, error) SideLock()
        {
            Action unlock = default;
            error err = default!;

            if (cfg.GOMODCACHE == "")
            { 
                // modload.Init exits if GOPATH[0] is empty, and cfg.GOMODCACHE
                // is set to GOPATH[0]/pkg/mod if GOMODCACHE is empty, so this should never happen.
                @base.Fatalf("go: internal error: cfg.GOMODCACHE not set");

            }

            var path = filepath.Join(cfg.GOMODCACHE, "cache", "lock");
            {
                var err = os.MkdirAll(filepath.Dir(path), 0777L);

                if (err != null)
                {
                    return (null, error.As(fmt.Errorf("failed to create cache directory: %w", err))!);
                }

            }


            return lockedfile.MutexAt(path).Lock();

        }

        // A cachingRepo is a cache around an underlying Repo,
        // avoiding redundant calls to ModulePath, Versions, Stat, Latest, and GoMod (but not Zip).
        // It is also safe for simultaneous use by multiple goroutines
        // (so that it can be returned from Lookup multiple times).
        // It serializes calls to the underlying Repo.
        private partial struct cachingRepo
        {
            public @string path;
            public par.Cache cache; // cache for all operations
            public Repo r;
        }

        private static ptr<cachingRepo> newCachingRepo(Repo r)
        {
            return addr(new cachingRepo(r:r,path:r.ModulePath(),));
        }

        private static @string ModulePath(this ptr<cachingRepo> _addr_r)
        {
            ref cachingRepo r = ref _addr_r.val;

            return r.path;
        }

        private static (slice<@string>, error) Versions(this ptr<cachingRepo> _addr_r, @string prefix)
        {
            slice<@string> _p0 = default;
            error _p0 = default!;
            ref cachingRepo r = ref _addr_r.val;

            private partial struct cached
            {
                public slice<@string> list;
                public error err;
            }
            cached c = r.cache.Do("versions:" + prefix, () =>
            {
                var (list, err) = r.r.Versions(prefix);
                return new cached(list,err);
            })._<cached>();

            if (c.err != null)
            {
                return (null, error.As(c.err)!);
            }

            return (append((slice<@string>)null, c.list), error.As(null!)!);

        }

        private partial struct cachedInfo
        {
            public ptr<RevInfo> info;
            public error err;
        }

        private static (ptr<RevInfo>, error) Stat(this ptr<cachingRepo> _addr_r, @string rev)
        {
            ptr<RevInfo> _p0 = default!;
            error _p0 = default!;
            ref cachingRepo r = ref _addr_r.val;

            cachedInfo c = r.cache.Do("stat:" + rev, () =>
            {
                var (file, info, err) = readDiskStat(r.path, rev);
                if (err == null)
                {
                    return _addr_new cachedInfo(info,nil)!;
                }

                info, err = r.r.Stat(rev);
                if (err == null)
                { 
                    // If we resolved, say, 1234abcde to v0.0.0-20180604122334-1234abcdef78,
                    // then save the information under the proper version, for future use.
                    if (info.Version != rev)
                    {
                        file, _ = CachePath(new module.Version(Path:r.path,Version:info.Version), "info");
                        r.cache.Do("stat:" + info.Version, () =>
                        {
                            return _addr_new cachedInfo(info,err)!;
                        });

                    }

                    {
                        var err = writeDiskStat(file, _addr_info);

                        if (err != null)
                        {
                            fmt.Fprintf(os.Stderr, "go: writing stat cache: %v\n", err);
                        }

                    }

                }

                return _addr_new cachedInfo(info,err)!;

            })._<cachedInfo>();

            if (c.err != null)
            {
                return (_addr_null!, error.As(c.err)!);
            }

            ref var info = ref heap(c.info.val, out ptr<var> _addr_info);
            return (_addr__addr_info!, error.As(null!)!);

        }

        private static (ptr<RevInfo>, error) Latest(this ptr<cachingRepo> _addr_r)
        {
            ptr<RevInfo> _p0 = default!;
            error _p0 = default!;
            ref cachingRepo r = ref _addr_r.val;

            cachedInfo c = r.cache.Do("latest:", () =>
            {
                var (info, err) = r.r.Latest(); 

                // Save info for likely future Stat call.
                if (err == null)
                {
                    r.cache.Do("stat:" + info.Version, () =>
                    {
                        return _addr_new cachedInfo(info,err)!;
                    });
                    {
                        var (file, _, err) = readDiskStat(r.path, info.Version);

                        if (err != null)
                        {
                            writeDiskStat(file, _addr_info);
                        }

                    }

                }

                return _addr_new cachedInfo(info,err)!;

            })._<cachedInfo>();

            if (c.err != null)
            {
                return (_addr_null!, error.As(c.err)!);
            }

            ref var info = ref heap(c.info.val, out ptr<var> _addr_info);
            return (_addr__addr_info!, error.As(null!)!);

        }

        private static (slice<byte>, error) GoMod(this ptr<cachingRepo> _addr_r, @string version)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref cachingRepo r = ref _addr_r.val;

            private partial struct cached
            {
                public slice<@string> list;
                public error err;
            }
            cached c = r.cache.Do("gomod:" + version, () =>
            {
                var (file, text, err) = readDiskGoMod(r.path, version);
                if (err == null)
                { 
                    // Note: readDiskGoMod already called checkGoMod.
                    return new cached(text,nil);

                }

                text, err = r.r.GoMod(version);
                if (err == null)
                {
                    {
                        var err__prev2 = err;

                        var err = checkGoMod(r.path, version, text);

                        if (err != null)
                        {
                            return new cached(text,err);
                        }

                        err = err__prev2;

                    }

                    {
                        var err__prev2 = err;

                        err = writeDiskGoMod(file, text);

                        if (err != null)
                        {
                            fmt.Fprintf(os.Stderr, "go: writing go.mod cache: %v\n", err);
                        }

                        err = err__prev2;

                    }

                }

                return new cached(text,err);

            })._<cached>();

            if (c.err != null)
            {
                return (null, error.As(c.err)!);
            }

            return (append((slice<byte>)null, c.text), error.As(null!)!);

        }

        private static error Zip(this ptr<cachingRepo> _addr_r, io.Writer dst, @string version)
        {
            ref cachingRepo r = ref _addr_r.val;

            return error.As(r.r.Zip(dst, version))!;
        }

        // Stat is like Lookup(path).Stat(rev) but avoids the
        // repository path resolution in Lookup if the result is
        // already cached on local disk.
        public static (ptr<RevInfo>, error) Stat(@string proxy, @string path, @string rev)
        {
            ptr<RevInfo> _p0 = default!;
            error _p0 = default!;

            var (_, info, err) = readDiskStat(path, rev);
            if (err == null)
            {
                return (_addr_info!, error.As(null!)!);
            }

            var (repo, err) = Lookup(proxy, path);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            return _addr_repo.Stat(rev)!;

        }

        // InfoFile is like Stat but returns the name of the file containing
        // the cached information.
        public static (@string, error) InfoFile(@string path, @string version)
        {
            @string _p0 = default;
            error _p0 = default!;

            if (!semver.IsValid(version))
            {
                return ("", error.As(fmt.Errorf("invalid version %q", version))!);
            }

            {
                var file__prev1 = file;

                var (file, _, err) = readDiskStat(path, version);

                if (err == null)
                {
                    return (file, error.As(null!)!);
                }

                file = file__prev1;

            }


            var err = TryProxies(proxy =>
            {
                var (repo, err) = Lookup(proxy, path);
                if (err == null)
                {
                    _, err = repo.Stat(version);
                }

                return err;

            });
            if (err != null)
            {
                return ("", error.As(err)!);
            } 

            // Stat should have populated the disk cache for us.
            (file, _, err) = readDiskStat(path, version);
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            return (file, error.As(null!)!);

        }

        // GoMod is like Lookup(path).GoMod(rev) but avoids the
        // repository path resolution in Lookup if the result is
        // already cached on local disk.
        public static (slice<byte>, error) GoMod(@string path, @string rev)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
 
            // Convert commit hash to pseudo-version
            // to increase cache hit rate.
            if (!semver.IsValid(rev))
            {
                {
                    var (_, info, err) = readDiskStat(path, rev);

                    if (err == null)
                    {
                        rev = info.Version;
                    }
                    else
                    {
                        var err = TryProxies(proxy =>
                        {
                            var (repo, err) = Lookup(proxy, path);
                            if (err != null)
                            {
                                return err;
                            }

                            var (info, err) = repo.Stat(rev);
                            if (err == null)
                            {
                                rev = info.Version;
                            }

                            return err;

                        });
                        if (err != null)
                        {
                            return (null, error.As(err)!);
                        }

                    }

                }

            }

            var (_, data, err) = readDiskGoMod(path, rev);
            if (err == null)
            {
                return (data, error.As(null!)!);
            }

            err = TryProxies(proxy =>
            {
                (repo, err) = Lookup(proxy, path);
                if (err == null)
                {
                    data, err = repo.GoMod(rev);
                }

                return err;

            });
            return (data, error.As(err)!);

        }

        // GoModFile is like GoMod but returns the name of the file containing
        // the cached information.
        public static (@string, error) GoModFile(@string path, @string version)
        {
            @string _p0 = default;
            error _p0 = default!;

            if (!semver.IsValid(version))
            {
                return ("", error.As(fmt.Errorf("invalid version %q", version))!);
            }

            {
                var (_, err) = GoMod(path, version);

                if (err != null)
                {
                    return ("", error.As(err)!);
                } 
                // GoMod should have populated the disk cache for us.

            } 
            // GoMod should have populated the disk cache for us.
            var (file, _, err) = readDiskGoMod(path, version);
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            return (file, error.As(null!)!);

        }

        // GoModSum returns the go.sum entry for the module version's go.mod file.
        // (That is, it returns the entry listed in go.sum as "path version/go.mod".)
        public static (@string, error) GoModSum(@string path, @string version)
        {
            @string _p0 = default;
            error _p0 = default!;

            if (!semver.IsValid(version))
            {
                return ("", error.As(fmt.Errorf("invalid version %q", version))!);
            }

            var (data, err) = GoMod(path, version);
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            var (sum, err) = goModSum(data);
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            return (sum, error.As(null!)!);

        }

        private static var errNotCached = fmt.Errorf("not in cache");

        // readDiskStat reads a cached stat result from disk,
        // returning the name of the cache file and the result.
        // If the read fails, the caller can use
        // writeDiskStat(file, info) to write a new cache entry.
        private static (@string, ptr<RevInfo>, error) readDiskStat(@string path, @string rev)
        {
            @string file = default;
            ptr<RevInfo> info = default!;
            error err = default!;

            var (file, data, err) = readDiskCache(path, rev, "info");
            if (err != null)
            { 
                // If the cache already contains a pseudo-version with the given hash, we
                // would previously return that pseudo-version without checking upstream.
                // However, that produced an unfortunate side-effect: if the author added a
                // tag to the repository, 'go get' would not pick up the effect of that new
                // tag on the existing commits, and 'go' commands that referred to those
                // commits would use the previous name instead of the new one.
                //
                // That's especially problematic if the original pseudo-version starts with
                // v0.0.0-, as was the case for all pseudo-versions during vgo development,
                // since a v0.0.0- pseudo-version has lower precedence than pretty much any
                // tagged version.
                //
                // In practice, we're only looking up by hash during initial conversion of a
                // legacy config and during an explicit 'go get', and a little extra latency
                // for those operations seems worth the benefit of picking up more accurate
                // versions.
                //
                // Fall back to this resolution scheme only if the GOPROXY setting prohibits
                // us from resolving upstream tags.
                if (cfg.GOPROXY == "off")
                {
                    {
                        var file__prev3 = file;

                        var (file, info, err) = readDiskStatByHash(path, rev);

                        if (err == null)
                        {
                            return (file, _addr_info!, error.As(null!)!);
                        }

                        file = file__prev3;

                    }

                }

                return (file, _addr_null!, error.As(err)!);

            }

            info = @new<RevInfo>();
            {
                var err = json.Unmarshal(data, info);

                if (err != null)
                {
                    return (file, _addr_null!, error.As(errNotCached)!);
                } 
                // The disk might have stale .info files that have Name and Short fields set.
                // We want to canonicalize to .info files with those fields omitted.
                // Remarshal and update the cache file if needed.

            } 
            // The disk might have stale .info files that have Name and Short fields set.
            // We want to canonicalize to .info files with those fields omitted.
            // Remarshal and update the cache file if needed.
            var (data2, err) = json.Marshal(info);
            if (err == null && !bytes.Equal(data2, data))
            {
                writeDiskCache(file, data);
            }

            return (file, _addr_info!, error.As(null!)!);

        }

        // readDiskStatByHash is a fallback for readDiskStat for the case
        // where rev is a commit hash instead of a proper semantic version.
        // In that case, we look for a cached pseudo-version that matches
        // the commit hash. If we find one, we use it.
        // This matters most for converting legacy package management
        // configs, when we are often looking up commits by full hash.
        // Without this check we'd be doing network I/O to the remote repo
        // just to find out about a commit we already know about
        // (and have cached under its pseudo-version).
        private static (@string, ptr<RevInfo>, error) readDiskStatByHash(@string path, @string rev)
        {
            @string file = default;
            ptr<RevInfo> info = default!;
            error err = default!;

            if (cfg.GOMODCACHE == "")
            { 
                // Do not download to current directory.
                return ("", _addr_null!, error.As(errNotCached)!);

            }

            if (!codehost.AllHex(rev) || len(rev) < 12L)
            {
                return ("", _addr_null!, error.As(errNotCached)!);
            }

            rev = rev[..12L];
            var (cdir, err) = cacheDir(path);
            if (err != null)
            {
                return ("", _addr_null!, error.As(errNotCached)!);
            }

            var (dir, err) = os.Open(cdir);
            if (err != null)
            {
                return ("", _addr_null!, error.As(errNotCached)!);
            }

            var (names, err) = dir.Readdirnames(-1L);
            dir.Close();
            if (err != null)
            {
                return ("", _addr_null!, error.As(errNotCached)!);
            } 

            // A given commit hash may map to more than one pseudo-version,
            // depending on which tags are present on the repository.
            // Take the highest such version.
            @string maxVersion = default;
            @string suffix = "-" + rev + ".info";
            err = errNotCached;
            foreach (var (_, name) in names)
            {
                if (strings.HasSuffix(name, suffix))
                {
                    var v = strings.TrimSuffix(name, ".info");
                    if (IsPseudoVersion(v) && semver.Max(maxVersion, v) == v)
                    {
                        maxVersion = v;
                        file, info, err = readDiskStat(path, strings.TrimSuffix(name, ".info"));
                    }

                }

            }
            return (file, _addr_info!, error.As(err)!);

        }

        // oldVgoPrefix is the prefix in the old auto-generated cached go.mod files.
        // We stopped trying to auto-generate the go.mod files. Now we use a trivial
        // go.mod with only a module line, and we've dropped the version prefix
        // entirely. If we see a version prefix, that means we're looking at an old copy
        // and should ignore it.
        private static slice<byte> oldVgoPrefix = (slice<byte>)"//vgo 0.0.";

        // readDiskGoMod reads a cached go.mod file from disk,
        // returning the name of the cache file and the result.
        // If the read fails, the caller can use
        // writeDiskGoMod(file, data) to write a new cache entry.
        private static (@string, slice<byte>, error) readDiskGoMod(@string path, @string rev)
        {
            @string file = default;
            slice<byte> data = default;
            error err = default!;

            file, data, err = readDiskCache(path, rev, "mod"); 

            // If the file has an old auto-conversion prefix, pretend it's not there.
            if (bytes.HasPrefix(data, oldVgoPrefix))
            {
                err = errNotCached;
                data = null;
            }

            if (err == null)
            {
                {
                    var err = checkGoMod(path, rev, data);

                    if (err != null)
                    {
                        return ("", null, error.As(err)!);
                    }

                }

            }

            return (file, data, error.As(err)!);

        }

        // readDiskCache is the generic "read from a cache file" implementation.
        // It takes the revision and an identifying suffix for the kind of data being cached.
        // It returns the name of the cache file and the content of the file.
        // If the read fails, the caller can use
        // writeDiskCache(file, data) to write a new cache entry.
        private static (@string, slice<byte>, error) readDiskCache(@string path, @string rev, @string suffix)
        {
            @string file = default;
            slice<byte> data = default;
            error err = default!;

            file, err = CachePath(new module.Version(Path:path,Version:rev), suffix);
            if (err != null)
            {
                return ("", null, error.As(errNotCached)!);
            }

            data, err = renameio.ReadFile(file);
            if (err != null)
            {
                return (file, null, error.As(errNotCached)!);
            }

            return (file, data, error.As(null!)!);

        }

        // writeDiskStat writes a stat result cache entry.
        // The file name must have been returned by a previous call to readDiskStat.
        private static error writeDiskStat(@string file, ptr<RevInfo> _addr_info)
        {
            ref RevInfo info = ref _addr_info.val;

            if (file == "")
            {
                return error.As(null!)!;
            }

            var (js, err) = json.Marshal(info);
            if (err != null)
            {
                return error.As(err)!;
            }

            return error.As(writeDiskCache(file, js))!;

        }

        // writeDiskGoMod writes a go.mod cache entry.
        // The file name must have been returned by a previous call to readDiskGoMod.
        private static error writeDiskGoMod(@string file, slice<byte> text)
        {
            return error.As(writeDiskCache(file, text))!;
        }

        // writeDiskCache is the generic "write to a cache file" implementation.
        // The file must have been returned by a previous call to readDiskCache.
        private static error writeDiskCache(@string file, slice<byte> data)
        {
            if (file == "")
            {
                return error.As(null!)!;
            } 
            // Make sure directory for file exists.
            {
                var err__prev1 = err;

                var err = os.MkdirAll(filepath.Dir(file), 0777L);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }


            {
                var err__prev1 = err;

                err = renameio.WriteFile(file, data, 0666L);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }


            if (strings.HasSuffix(file, ".mod"))
            {
                rewriteVersionList(filepath.Dir(file));
            }

            return error.As(null!)!;

        }

        // rewriteVersionList rewrites the version list in dir
        // after a new *.mod file has been written.
        private static void rewriteVersionList(@string dir) => func((defer, _, __) =>
        {
            if (filepath.Base(dir) != "@v")
            {
                @base.Fatalf("go: internal error: misuse of rewriteVersionList");
            }

            var listFile = filepath.Join(dir, "list"); 

            // We use a separate lockfile here instead of locking listFile itself because
            // we want to use Rename to write the file atomically. The list may be read by
            // a GOPROXY HTTP server, and if we crash midway through a rewrite (or if the
            // HTTP server ignores our locking and serves the file midway through a
            // rewrite) it's better to serve a stale list than a truncated one.
            var (unlock, err) = lockedfile.MutexAt(listFile + ".lock").Lock();
            if (err != null)
            {
                @base.Fatalf("go: can't lock version list lockfile: %v", err);
            }

            defer(unlock());

            var (infos, err) = ioutil.ReadDir(dir);
            if (err != null)
            {
                return ;
            }

            slice<@string> list = default;
            foreach (var (_, info) in infos)
            { 
                // We look for *.mod files on the theory that if we can't supply
                // the .mod file then there's no point in listing that version,
                // since it's unusable. (We can have *.info without *.mod.)
                // We don't require *.zip files on the theory that for code only
                // involved in module graph construction, many *.zip files
                // will never be requested.
                var name = info.Name();
                if (strings.HasSuffix(name, ".mod"))
                {
                    var v = strings.TrimSuffix(name, ".mod");
                    if (v != "" && module.CanonicalVersion(v) == v)
                    {
                        list = append(list, v);
                    }

                }

            }
            SortVersions(list);

            bytes.Buffer buf = default;
            {
                var v__prev1 = v;

                foreach (var (_, __v) in list)
                {
                    v = __v;
                    buf.WriteString(v);
                    buf.WriteString("\n");
                }

                v = v__prev1;
            }

            var (old, _) = renameio.ReadFile(listFile);
            if (bytes.Equal(buf.Bytes(), old))
            {
                return ;
            }

            {
                var err = renameio.WriteFile(listFile, buf.Bytes(), 0666L);

                if (err != null)
                {
                    @base.Fatalf("go: failed to write version list: %v", err);
                }

            }

        });
    }
}}}}
