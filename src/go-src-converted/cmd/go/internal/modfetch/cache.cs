// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modfetch -- go2cs converted at 2022 March 06 23:18:37 UTC
// import "cmd/go/internal/modfetch" ==> using modfetch = go.cmd.go.@internal.modfetch_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modfetch\cache.go
using bytes = go.bytes_package;
using json = go.encoding.json_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using fs = go.io.fs_package;
using rand = go.math.rand_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;

using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using lockedfile = go.cmd.go.@internal.lockedfile_package;
using codehost = go.cmd.go.@internal.modfetch.codehost_package;
using par = go.cmd.go.@internal.par_package;
using robustio = go.cmd.go.@internal.robustio_package;

using module = go.golang.org.x.mod.module_package;
using semver = go.golang.org.x.mod.semver_package;
using System;


namespace go.cmd.go.@internal;

public static partial class modfetch_package {

private static (@string, error) cacheDir(@string path) {
    @string _p0 = default;
    error _p0 = default!;

    {
        var err = checkCacheDir();

        if (err != null) {
            return ("", error.As(err)!);
        }
    }

    var (enc, err) = module.EscapePath(path);
    if (err != null) {
        return ("", error.As(err)!);
    }
    return (filepath.Join(cfg.GOMODCACHE, "cache/download", enc, "/@v"), error.As(null!)!);

}

public static (@string, error) CachePath(module.Version m, @string suffix) {
    @string _p0 = default;
    error _p0 = default!;

    var (dir, err) = cacheDir(m.Path);
    if (err != null) {
        return ("", error.As(err)!);
    }
    if (!semver.IsValid(m.Version)) {
        return ("", error.As(fmt.Errorf("non-semver module version %q", m.Version))!);
    }
    if (module.CanonicalVersion(m.Version) != m.Version) {
        return ("", error.As(fmt.Errorf("non-canonical module version %q", m.Version))!);
    }
    var (encVer, err) = module.EscapeVersion(m.Version);
    if (err != null) {
        return ("", error.As(err)!);
    }
    return (filepath.Join(dir, encVer + "." + suffix), error.As(null!)!);

}

// DownloadDir returns the directory to which m should have been downloaded.
// An error will be returned if the module path or version cannot be escaped.
// An error satisfying errors.Is(err, fs.ErrNotExist) will be returned
// along with the directory if the directory does not exist or if the directory
// is not completely populated.
public static (@string, error) DownloadDir(module.Version m) {
    @string _p0 = default;
    error _p0 = default!;

    {
        var err = checkCacheDir();

        if (err != null) {
            return ("", error.As(err)!);
        }
    }

    var (enc, err) = module.EscapePath(m.Path);
    if (err != null) {
        return ("", error.As(err)!);
    }
    if (!semver.IsValid(m.Version)) {
        return ("", error.As(fmt.Errorf("non-semver module version %q", m.Version))!);
    }
    if (module.CanonicalVersion(m.Version) != m.Version) {
        return ("", error.As(fmt.Errorf("non-canonical module version %q", m.Version))!);
    }
    var (encVer, err) = module.EscapeVersion(m.Version);
    if (err != null) {
        return ("", error.As(err)!);
    }
    var dir = filepath.Join(cfg.GOMODCACHE, enc + "@" + encVer);
    {
        var (fi, err) = os.Stat(dir);

        if (os.IsNotExist(err)) {
            return (dir, error.As(err)!);
        }
        else if (err != null) {
            return (dir, error.As(addr(new DownloadDirPartialError(dir,err))!)!);
        }
        else if (!fi.IsDir()) {
            return (dir, error.As(addr(new DownloadDirPartialError(dir,errors.New("not a directory")))!)!);
        }

    } 

    // Check if a .partial file exists. This is created at the beginning of
    // a download and removed after the zip is extracted.
    var (partialPath, err) = CachePath(m, "partial");
    if (err != null) {
        return (dir, error.As(err)!);
    }
    {
        var (_, err) = os.Stat(partialPath);

        if (err == null) {
            return (dir, error.As(addr(new DownloadDirPartialError(dir,errors.New("not completely extracted")))!)!);
        }
        else if (!os.IsNotExist(err)) {
            return (dir, error.As(err)!);
        }

    } 

    // Check if a .ziphash file exists. It should be created before the
    // zip is extracted, but if it was deleted (by another program?), we need
    // to re-calculate it. Note that checkMod will repopulate the ziphash
    // file if it doesn't exist, but if the module is excluded by checks
    // through GONOSUMDB or GOPRIVATE, that check and repopulation won't happen.
    var (ziphashPath, err) = CachePath(m, "ziphash");
    if (err != null) {
        return (dir, error.As(err)!);
    }
    {
        (_, err) = os.Stat(ziphashPath);

        if (os.IsNotExist(err)) {
            return (dir, error.As(addr(new DownloadDirPartialError(dir,errors.New("ziphash file is missing")))!)!);
        }
        else if (err != null) {
            return (dir, error.As(err)!);
        }

    }

    return (dir, error.As(null!)!);

}

// DownloadDirPartialError is returned by DownloadDir if a module directory
// exists but was not completely populated.
//
// DownloadDirPartialError is equivalent to fs.ErrNotExist.
public partial struct DownloadDirPartialError {
    public @string Dir;
    public error Err;
}

private static @string Error(this ptr<DownloadDirPartialError> _addr_e) {
    ref DownloadDirPartialError e = ref _addr_e.val;

    return fmt.Sprintf("%s: %v", e.Dir, e.Err);
}
private static bool Is(this ptr<DownloadDirPartialError> _addr_e, error err) {
    ref DownloadDirPartialError e = ref _addr_e.val;

    return err == fs.ErrNotExist;
}

// lockVersion locks a file within the module cache that guards the downloading
// and extraction of the zipfile for the given module version.
private static (Action, error) lockVersion(module.Version mod) {
    Action unlock = default;
    error err = default!;

    var (path, err) = CachePath(mod, "lock");
    if (err != null) {
        return (null, error.As(err)!);
    }
    {
        var err = os.MkdirAll(filepath.Dir(path), 0777);

        if (err != null) {
            return (null, error.As(err)!);
        }
    }

    return lockedfile.MutexAt(path).Lock();

}

// SideLock locks a file within the module cache that previously guarded
// edits to files outside the cache, such as go.sum and go.mod files in the
// user's working directory.
// If err is nil, the caller MUST eventually call the unlock function.
public static (Action, error) SideLock() {
    Action unlock = default;
    error err = default!;

    {
        var err__prev1 = err;

        var err = checkCacheDir();

        if (err != null) {
            return (null, error.As(err)!);
        }
        err = err__prev1;

    }


    var path = filepath.Join(cfg.GOMODCACHE, "cache", "lock");
    {
        var err__prev1 = err;

        err = os.MkdirAll(filepath.Dir(path), 0777);

        if (err != null) {
            return (null, error.As(fmt.Errorf("failed to create cache directory: %w", err))!);
        }
        err = err__prev1;

    }


    return lockedfile.MutexAt(path).Lock();

}

// A cachingRepo is a cache around an underlying Repo,
// avoiding redundant calls to ModulePath, Versions, Stat, Latest, and GoMod (but not Zip).
// It is also safe for simultaneous use by multiple goroutines
// (so that it can be returned from Lookup multiple times).
// It serializes calls to the underlying Repo.
private partial struct cachingRepo {
    public @string path;
    public par.Cache cache; // cache for all operations

    public sync.Once once;
    public Func<(Repo, error)> initRepo;
    public Repo r;
}

private static ptr<cachingRepo> newCachingRepo(@string path, Func<(Repo, error)> initRepo) {
    return addr(new cachingRepo(path:path,initRepo:initRepo,));
}

private static Repo repo(this ptr<cachingRepo> _addr_r) {
    ref cachingRepo r = ref _addr_r.val;

    r.once.Do(() => {
        error err = default!;
        r.r, err = r.initRepo();
        if (err != null) {
            r.r = new errRepo(r.path,err);
        }
    });
    return r.r;

}

private static @string ModulePath(this ptr<cachingRepo> _addr_r) {
    ref cachingRepo r = ref _addr_r.val;

    return r.path;
}

private static (slice<@string>, error) Versions(this ptr<cachingRepo> _addr_r, @string prefix) {
    slice<@string> _p0 = default;
    error _p0 = default!;
    ref cachingRepo r = ref _addr_r.val;

    private partial struct cached {
        public slice<@string> list;
        public error err;
    }
    cached c = r.cache.Do("versions:" + prefix, () => {
        var (list, err) = r.repo().Versions(prefix);
        return new cached(list,err);
    })._<cached>();

    if (c.err != null) {
        return (null, error.As(c.err)!);
    }
    return (append((slice<@string>)null, c.list), error.As(null!)!);

}

private partial struct cachedInfo {
    public ptr<RevInfo> info;
    public error err;
}

private static (ptr<RevInfo>, error) Stat(this ptr<cachingRepo> _addr_r, @string rev) {
    ptr<RevInfo> _p0 = default!;
    error _p0 = default!;
    ref cachingRepo r = ref _addr_r.val;

    cachedInfo c = r.cache.Do("stat:" + rev, () => {
        var (file, info, err) = readDiskStat(r.path, rev);
        if (err == null) {
            return _addr_new cachedInfo(info,nil)!;
        }
        info, err = r.repo().Stat(rev);
        if (err == null) { 
            // If we resolved, say, 1234abcde to v0.0.0-20180604122334-1234abcdef78,
            // then save the information under the proper version, for future use.
            if (info.Version != rev) {
                file, _ = CachePath(new module.Version(Path:r.path,Version:info.Version), "info");
                r.cache.Do("stat:" + info.Version, () => {
                    return _addr_new cachedInfo(info,err)!;
                });
            }

            {
                var err = writeDiskStat(file, _addr_info);

                if (err != null) {
                    fmt.Fprintf(os.Stderr, "go: writing stat cache: %v\n", err);
                }

            }

        }
        return _addr_new cachedInfo(info,err)!;

    })._<cachedInfo>();

    if (c.err != null) {
        return (_addr_null!, error.As(c.err)!);
    }
    ref var info = ref heap(c.info.val, out ptr<var> _addr_info);
    return (_addr__addr_info!, error.As(null!)!);

}

private static (ptr<RevInfo>, error) Latest(this ptr<cachingRepo> _addr_r) {
    ptr<RevInfo> _p0 = default!;
    error _p0 = default!;
    ref cachingRepo r = ref _addr_r.val;

    cachedInfo c = r.cache.Do("latest:", () => {
        var (info, err) = r.repo().Latest(); 

        // Save info for likely future Stat call.
        if (err == null) {
            r.cache.Do("stat:" + info.Version, () => {
                return _addr_new cachedInfo(info,err)!;
            });
            {
                var (file, _, err) = readDiskStat(r.path, info.Version);

                if (err != null) {
                    writeDiskStat(file, _addr_info);
                }

            }

        }
        return _addr_new cachedInfo(info,err)!;

    })._<cachedInfo>();

    if (c.err != null) {
        return (_addr_null!, error.As(c.err)!);
    }
    ref var info = ref heap(c.info.val, out ptr<var> _addr_info);
    return (_addr__addr_info!, error.As(null!)!);

}

private static (slice<byte>, error) GoMod(this ptr<cachingRepo> _addr_r, @string version) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref cachingRepo r = ref _addr_r.val;

    private partial struct cached {
        public slice<@string> list;
        public error err;
    }
    cached c = r.cache.Do("gomod:" + version, () => {
        var (file, text, err) = readDiskGoMod(r.path, version);
        if (err == null) { 
            // Note: readDiskGoMod already called checkGoMod.
            return new cached(text,nil);

        }
        text, err = r.repo().GoMod(version);
        if (err == null) {
            {
                var err__prev2 = err;

                var err = checkGoMod(r.path, version, text);

                if (err != null) {
                    return new cached(text,err);
                }

                err = err__prev2;

            }

            {
                var err__prev2 = err;

                err = writeDiskGoMod(file, text);

                if (err != null) {
                    fmt.Fprintf(os.Stderr, "go: writing go.mod cache: %v\n", err);
                }

                err = err__prev2;

            }

        }
        return new cached(text,err);

    })._<cached>();

    if (c.err != null) {
        return (null, error.As(c.err)!);
    }
    return (append((slice<byte>)null, c.text), error.As(null!)!);

}

private static error Zip(this ptr<cachingRepo> _addr_r, io.Writer dst, @string version) {
    ref cachingRepo r = ref _addr_r.val;

    return error.As(r.repo().Zip(dst, version))!;
}

// InfoFile is like Lookup(path).Stat(version) but returns the name of the file
// containing the cached information.
public static (@string, error) InfoFile(@string path, @string version) {
    @string _p0 = default;
    error _p0 = default!;

    if (!semver.IsValid(version)) {
        return ("", error.As(fmt.Errorf("invalid version %q", version))!);
    }
    {
        var file__prev1 = file;

        var (file, _, err) = readDiskStat(path, version);

        if (err == null) {
            return (file, error.As(null!)!);
        }
        file = file__prev1;

    }


    var err = TryProxies(proxy => {
        var (_, err) = Lookup(proxy, path).Stat(version);
        return err;
    });
    if (err != null) {
        return ("", error.As(err)!);
    }
    var (file, err) = CachePath(new module.Version(Path:path,Version:version), "info");
    if (err != null) {
        return ("", error.As(err)!);
    }
    return (file, error.As(null!)!);

}

// GoMod is like Lookup(path).GoMod(rev) but avoids the
// repository path resolution in Lookup if the result is
// already cached on local disk.
public static (slice<byte>, error) GoMod(@string path, @string rev) {
    slice<byte> _p0 = default;
    error _p0 = default!;
 
    // Convert commit hash to pseudo-version
    // to increase cache hit rate.
    if (!semver.IsValid(rev)) {
        {
            var (_, info, err) = readDiskStat(path, rev);

            if (err == null) {
                rev = info.Version;
            }
            else
 {
                if (errors.Is(err, statCacheErr)) {
                    return (null, error.As(err)!);
                }
                var err = TryProxies(proxy => {
                    var (info, err) = Lookup(proxy, path).Stat(rev);
                    if (err == null) {
                        rev = info.Version;
                    }
                    return err;
                });
                if (err != null) {
                    return (null, error.As(err)!);
                }
            }

        }

    }
    var (_, data, err) = readDiskGoMod(path, rev);
    if (err == null) {
        return (data, error.As(null!)!);
    }
    err = TryProxies(proxy => {
        data, err = Lookup(proxy, path).GoMod(rev);
        return err;
    });
    return (data, error.As(err)!);

}

// GoModFile is like GoMod but returns the name of the file containing
// the cached information.
public static (@string, error) GoModFile(@string path, @string version) {
    @string _p0 = default;
    error _p0 = default!;

    if (!semver.IsValid(version)) {
        return ("", error.As(fmt.Errorf("invalid version %q", version))!);
    }
    {
        var (_, err) = GoMod(path, version);

        if (err != null) {
            return ("", error.As(err)!);
        }
    } 
    // GoMod should have populated the disk cache for us.
    var (file, err) = CachePath(new module.Version(Path:path,Version:version), "mod");
    if (err != null) {
        return ("", error.As(err)!);
    }
    return (file, error.As(null!)!);

}

// GoModSum returns the go.sum entry for the module version's go.mod file.
// (That is, it returns the entry listed in go.sum as "path version/go.mod".)
public static (@string, error) GoModSum(@string path, @string version) {
    @string _p0 = default;
    error _p0 = default!;

    if (!semver.IsValid(version)) {
        return ("", error.As(fmt.Errorf("invalid version %q", version))!);
    }
    var (data, err) = GoMod(path, version);
    if (err != null) {
        return ("", error.As(err)!);
    }
    var (sum, err) = goModSum(data);
    if (err != null) {
        return ("", error.As(err)!);
    }
    return (sum, error.As(null!)!);

}

private static var errNotCached = fmt.Errorf("not in cache");

// readDiskStat reads a cached stat result from disk,
// returning the name of the cache file and the result.
// If the read fails, the caller can use
// writeDiskStat(file, info) to write a new cache entry.
private static (@string, ptr<RevInfo>, error) readDiskStat(@string path, @string rev) {
    @string file = default;
    ptr<RevInfo> info = default!;
    error err = default!;

    var (file, data, err) = readDiskCache(path, rev, "info");
    if (err != null) { 
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
        if (cfg.GOPROXY == "off") {
            {
                var file__prev3 = file;

                var (file, info, err) = readDiskStatByHash(path, rev);

                if (err == null) {
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

        if (err != null) {
            return (file, _addr_null!, error.As(errNotCached)!);
        }
    } 
    // The disk might have stale .info files that have Name and Short fields set.
    // We want to canonicalize to .info files with those fields omitted.
    // Remarshal and update the cache file if needed.
    var (data2, err) = json.Marshal(info);
    if (err == null && !bytes.Equal(data2, data)) {
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
private static (@string, ptr<RevInfo>, error) readDiskStatByHash(@string path, @string rev) {
    @string file = default;
    ptr<RevInfo> info = default!;
    error err = default!;

    if (cfg.GOMODCACHE == "") { 
        // Do not download to current directory.
        return ("", _addr_null!, error.As(errNotCached)!);

    }
    if (!codehost.AllHex(rev) || len(rev) < 12) {
        return ("", _addr_null!, error.As(errNotCached)!);
    }
    rev = rev[..(int)12];
    var (cdir, err) = cacheDir(path);
    if (err != null) {
        return ("", _addr_null!, error.As(errNotCached)!);
    }
    var (dir, err) = os.Open(cdir);
    if (err != null) {
        return ("", _addr_null!, error.As(errNotCached)!);
    }
    var (names, err) = dir.Readdirnames(-1);
    dir.Close();
    if (err != null) {
        return ("", _addr_null!, error.As(errNotCached)!);
    }
    @string maxVersion = default;
    @string suffix = "-" + rev + ".info";
    err = errNotCached;
    foreach (var (_, name) in names) {
        if (strings.HasSuffix(name, suffix)) {
            var v = strings.TrimSuffix(name, ".info");
            if (module.IsPseudoVersion(v) && semver.Compare(v, maxVersion) > 0) {
                maxVersion = v;
                file, info, err = readDiskStat(path, strings.TrimSuffix(name, ".info"));
            }
        }
    }    return (file, _addr_info!, error.As(err)!);

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
private static (@string, slice<byte>, error) readDiskGoMod(@string path, @string rev) {
    @string file = default;
    slice<byte> data = default;
    error err = default!;

    file, data, err = readDiskCache(path, rev, "mod"); 

    // If the file has an old auto-conversion prefix, pretend it's not there.
    if (bytes.HasPrefix(data, oldVgoPrefix)) {
        err = errNotCached;
        data = null;
    }
    if (err == null) {
        {
            var err = checkGoMod(path, rev, data);

            if (err != null) {
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
private static (@string, slice<byte>, error) readDiskCache(@string path, @string rev, @string suffix) {
    @string file = default;
    slice<byte> data = default;
    error err = default!;

    file, err = CachePath(new module.Version(Path:path,Version:rev), suffix);
    if (err != null) {
        return ("", null, error.As(errNotCached)!);
    }
    data, err = robustio.ReadFile(file);
    if (err != null) {
        return (file, null, error.As(errNotCached)!);
    }
    return (file, data, error.As(null!)!);

}

// writeDiskStat writes a stat result cache entry.
// The file name must have been returned by a previous call to readDiskStat.
private static error writeDiskStat(@string file, ptr<RevInfo> _addr_info) {
    ref RevInfo info = ref _addr_info.val;

    if (file == "") {
        return error.As(null!)!;
    }
    var (js, err) = json.Marshal(info);
    if (err != null) {
        return error.As(err)!;
    }
    return error.As(writeDiskCache(file, js))!;

}

// writeDiskGoMod writes a go.mod cache entry.
// The file name must have been returned by a previous call to readDiskGoMod.
private static error writeDiskGoMod(@string file, slice<byte> text) {
    return error.As(writeDiskCache(file, text))!;
}

// writeDiskCache is the generic "write to a cache file" implementation.
// The file must have been returned by a previous call to readDiskCache.
private static error writeDiskCache(@string file, slice<byte> data) => func((defer, _, _) => {
    if (file == "") {
        return error.As(null!)!;
    }
    {
        var err__prev1 = err;

        var err = os.MkdirAll(filepath.Dir(file), 0777);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    } 

    // Write the file to a temporary location, and then rename it to its final
    // path to reduce the likelihood of a corrupt file existing at that final path.
    var (f, err) = tempFile(filepath.Dir(file), filepath.Base(file), 0666);
    if (err != null) {
        return error.As(err)!;
    }
    defer(() => { 
        // Only call os.Remove on f.Name() if we failed to rename it: otherwise,
        // some other process may have created a new file with the same name after
        // the rename completed.
        if (err != null) {
            f.Close();
            os.Remove(f.Name());
        }
    }());

    {
        var err__prev1 = err;

        var (_, err) = f.Write(data);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    {
        var err__prev1 = err;

        err = f.Close();

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    {
        var err__prev1 = err;

        err = robustio.Rename(f.Name(), file);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }


    if (strings.HasSuffix(file, ".mod")) {
        rewriteVersionList(filepath.Dir(file));
    }
    return error.As(null!)!;

});

// tempFile creates a new temporary file with given permission bits.
private static (ptr<os.File>, error) tempFile(@string dir, @string prefix, fs.FileMode perm) {
    ptr<os.File> f = default!;
    error err = default!;

    for (nint i = 0; i < 10000; i++) {
        var name = filepath.Join(dir, prefix + strconv.Itoa(rand.Intn(1000000000)) + ".tmp");
        f, err = os.OpenFile(name, os.O_RDWR | os.O_CREATE | os.O_EXCL, perm);
        if (os.IsExist(err)) {
            continue;
        }
        break;

    }
    return ;

}

// rewriteVersionList rewrites the version list in dir
// after a new *.mod file has been written.
private static error rewriteVersionList(@string dir) => func((defer, _, _) => {
    error err = default!;

    if (filepath.Base(dir) != "@v") {
        @base.Fatalf("go: internal error: misuse of rewriteVersionList");
    }
    var listFile = filepath.Join(dir, "list"); 

    // Lock listfile when writing to it to try to avoid corruption to the file.
    // Under rare circumstances, for instance, if the system loses power in the
    // middle of a write it is possible for corrupt data to be written. This is
    // not a problem for the go command itself, but may be an issue if the the
    // cache is being served by a GOPROXY HTTP server. This will be corrected
    // the next time a new version of the module is fetched and the file is rewritten.
    // TODO(matloob): golang.org/issue/43313 covers adding a go mod verify
    // command that removes module versions that fail checksums. It should also
    // remove list files that are detected to be corrupt.
    var (f, err) = lockedfile.Edit(listFile);
    if (err != null) {
        return error.As(err)!;
    }
    defer(() => {
        {
            var cerr = f.Close();

            if (cerr != null && err == null) {
                err = cerr;
            }

        }

    }());
    var (infos, err) = os.ReadDir(dir);
    if (err != null) {
        return error.As(err)!;
    }
    slice<@string> list = default;
    foreach (var (_, info) in infos) { 
        // We look for *.mod files on the theory that if we can't supply
        // the .mod file then there's no point in listing that version,
        // since it's unusable. (We can have *.info without *.mod.)
        // We don't require *.zip files on the theory that for code only
        // involved in module graph construction, many *.zip files
        // will never be requested.
        var name = info.Name();
        if (strings.HasSuffix(name, ".mod")) {
            var v = strings.TrimSuffix(name, ".mod");
            if (v != "" && module.CanonicalVersion(v) == v) {
                list = append(list, v);
            }
        }
    }    semver.Sort(list);

    bytes.Buffer buf = default;
    {
        var v__prev1 = v;

        foreach (var (_, __v) in list) {
            v = __v;
            buf.WriteString(v);
            buf.WriteString("\n");
        }
        v = v__prev1;
    }

    {
        var err__prev1 = err;

        var (fi, err) = f.Stat();

        if (err == null && int(fi.Size()) == buf.Len()) {
            var old = make_slice<byte>(buf.Len() + 1);
            {
                var err__prev2 = err;

                var (n, err) = f.ReadAt(old, 0);

                if (err == io.EOF && n == buf.Len() && bytes.Equal(buf.Bytes(), old)) {
                    return error.As(null!)!; // No edit needed.
                }

                err = err__prev2;

            }

        }
        err = err__prev1;

    } 
    // Remove existing contents, so that when we truncate to the actual size it will zero-fill,
    // and we will be able to detect (some) incomplete writes as files containing trailing NUL bytes.
    {
        var err__prev1 = err;

        var err = f.Truncate(0);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    } 
    // Reserve the final size and zero-fill.
    {
        var err__prev1 = err;

        err = f.Truncate(int64(buf.Len()));

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    } 
    // Write the actual contents. If this fails partway through,
    // the remainder of the file should remain as zeroes.
    {
        var err__prev1 = err;

        var (_, err) = f.Write(buf.Bytes());

        if (err != null) {
            f.Truncate(0);
            return error.As(err)!;
        }
        err = err__prev1;

    }


    return error.As(null!)!;

});

private static sync.Once statCacheOnce = default;private static error statCacheErr = default!;

// checkCacheDir checks if the directory specified by GOMODCACHE exists. An
// error is returned if it does not.
private static error checkCacheDir() {
    if (cfg.GOMODCACHE == "") { 
        // modload.Init exits if GOPATH[0] is empty, and cfg.GOMODCACHE
        // is set to GOPATH[0]/pkg/mod if GOMODCACHE is empty, so this should never happen.
        return error.As(fmt.Errorf("internal error: cfg.GOMODCACHE not set"))!;

    }
    if (!filepath.IsAbs(cfg.GOMODCACHE)) {
        return error.As(fmt.Errorf("GOMODCACHE entry is relative; must be absolute path: %q.\n", cfg.GOMODCACHE))!;
    }
    statCacheOnce.Do(() => {
        var (fi, err) = os.Stat(cfg.GOMODCACHE);
        if (err != null) {
            if (!os.IsNotExist(err)) {
                statCacheErr = fmt.Errorf("could not create module cache: %w", err);
                return ;
            }
            {
                var err = os.MkdirAll(cfg.GOMODCACHE, 0777);

                if (err != null) {
                    statCacheErr = fmt.Errorf("could not create module cache: %w", err);
                    return ;
                }

            }

            return ;

        }
        if (!fi.IsDir()) {
            statCacheErr = fmt.Errorf("could not create module cache: %q is not a directory", cfg.GOMODCACHE);
            return ;
        }
    });
    return error.As(statCacheErr)!;

}

} // end modfetch_package
