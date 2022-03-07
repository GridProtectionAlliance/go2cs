// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modfetch -- go2cs converted at 2022 March 06 23:18:55 UTC
// import "cmd/go/internal/modfetch" ==> using modfetch = go.cmd.go.@internal.modfetch_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modfetch\fetch.go
using zip = go.archive.zip_package;
using bytes = go.bytes_package;
using context = go.context_package;
using sha256 = go.crypto.sha256_package;
using base64 = go.encoding.base64_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using fs = go.io.fs_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using sort = go.sort_package;
using strings = go.strings_package;
using sync = go.sync_package;

using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using fsys = go.cmd.go.@internal.fsys_package;
using lockedfile = go.cmd.go.@internal.lockedfile_package;
using par = go.cmd.go.@internal.par_package;
using robustio = go.cmd.go.@internal.robustio_package;
using trace = go.cmd.go.@internal.trace_package;

using module = go.golang.org.x.mod.module_package;
using dirhash = go.golang.org.x.mod.sumdb.dirhash_package;
using modzip = go.golang.org.x.mod.zip_package;
using System;


namespace go.cmd.go.@internal;

public static partial class modfetch_package {

private static par.Cache downloadCache = default;

// Download downloads the specific module version to the
// local download cache and returns the name of the directory
// corresponding to the root of the module's file tree.
public static (@string, error) Download(context.Context ctx, module.Version mod) {
    @string dir = default;
    error err = default!;

    {
        var err = checkCacheDir();

        if (err != null) {
            @base.Fatalf("go: %v", err);
        }
    } 

    // The par.Cache here avoids duplicate work.
    private partial struct cached {
        public @string dir;
        public error err;
    }
    cached c = downloadCache.Do(mod, () => {
        var (dir, err) = download(ctx, mod);
        if (err != null) {
            return new cached("",err);
        }
        checkMod(mod);
        return new cached(dir,nil);

    })._<cached>();
    return (c.dir, error.As(c.err)!);

}

private static (@string, error) download(context.Context ctx, module.Version mod) => func((defer, _, _) => {
    @string dir = default;
    error err = default!;

    var (ctx, span) = trace.StartSpan(ctx, "modfetch.download " + mod.String());
    defer(span.Done());

    dir, err = DownloadDir(mod);
    if (err == null) { 
        // The directory has already been completely extracted (no .partial file exists).
        return (dir, error.As(null!)!);

    }
    else if (dir == "" || !errors.Is(err, fs.ErrNotExist)) {
        return ("", error.As(err)!);
    }
    var (zipfile, err) = DownloadZip(ctx, mod);
    if (err != null) {
        return ("", error.As(err)!);
    }
    var (unlock, err) = lockVersion(mod);
    if (err != null) {
        return ("", error.As(err)!);
    }
    defer(unlock());

    ctx, span = trace.StartSpan(ctx, "unzip " + zipfile);
    defer(span.Done()); 

    // Check whether the directory was populated while we were waiting on the lock.
    var (_, dirErr) = DownloadDir(mod);
    if (dirErr == null) {
        return (dir, error.As(null!)!);
    }
    ptr<DownloadDirPartialError> (_, dirExists) = dirErr._<ptr<DownloadDirPartialError>>(); 

    // Clean up any remaining temporary directories created by old versions
    // (before 1.16), as well as partially extracted directories (indicated by
    // DownloadDirPartialError, usually because of a .partial file). This is only
    // safe to do because the lock file ensures that their writers are no longer
    // active.
    var parentDir = filepath.Dir(dir);
    var tmpPrefix = filepath.Base(dir) + ".tmp-";
    {
        var err__prev1 = err;

        var (old, err) = filepath.Glob(filepath.Join(parentDir, tmpPrefix + "*"));

        if (err == null) {
            foreach (var (_, path) in old) {
                RemoveAll(path); // best effort
            }

        }
        err = err__prev1;

    }

    if (dirExists) {
        {
            var err__prev2 = err;

            var err = RemoveAll(dir);

            if (err != null) {
                return ("", error.As(err)!);
            }

            err = err__prev2;

        }

    }
    var (partialPath, err) = CachePath(mod, "partial");
    if (err != null) {
        return ("", error.As(err)!);
    }
    {
        var err__prev1 = err;

        err = os.MkdirAll(parentDir, 0777);

        if (err != null) {
            return ("", error.As(err)!);
        }
        err = err__prev1;

    }

    {
        var err__prev1 = err;

        err = os.WriteFile(partialPath, null, 0666);

        if (err != null) {
            return ("", error.As(err)!);
        }
        err = err__prev1;

    }

    {
        var err__prev1 = err;

        err = modzip.Unzip(dir, mod, zipfile);

        if (err != null) {
            fmt.Fprintf(os.Stderr, "-> %s\n", err);
            {
                var rmErr = RemoveAll(dir);

                if (rmErr == null) {
                    os.Remove(partialPath);
                }

            }

            return ("", error.As(err)!);

        }
        err = err__prev1;

    }

    {
        var err__prev1 = err;

        err = os.Remove(partialPath);

        if (err != null) {
            return ("", error.As(err)!);
        }
        err = err__prev1;

    }


    if (!cfg.ModCacheRW) {
        makeDirsReadOnly(dir);
    }
    return (dir, error.As(null!)!);

});

private static par.Cache downloadZipCache = default;

// DownloadZip downloads the specific module version to the
// local zip cache and returns the name of the zip file.
public static (@string, error) DownloadZip(context.Context ctx, module.Version mod) => func((defer, _, _) => {
    @string zipfile = default;
    error err = default!;
 
    // The par.Cache here avoids duplicate work.
    private partial struct cached {
        public @string dir;
        public error err;
    }
    cached c = downloadZipCache.Do(mod, () => {
        var (zipfile, err) = CachePath(mod, "zip");
        if (err != null) {
            return new cached("",err);
        }
        var ziphashfile = zipfile + "hash"; 

        // Return without locking if the zip and ziphash files exist.
        {
            var (_, err) = os.Stat(zipfile);

            if (err == null) {
                {
                    (_, err) = os.Stat(ziphashfile);

                    if (err == null) {
                        return new cached(zipfile,nil);
                    }

                }

            } 

            // The zip or ziphash file does not exist. Acquire the lock and create them.

        } 

        // The zip or ziphash file does not exist. Acquire the lock and create them.
        if (cfg.CmdName != "mod download") {
            fmt.Fprintf(os.Stderr, "go: downloading %s %s\n", mod.Path, mod.Version);
        }
        var (unlock, err) = lockVersion(mod);
        if (err != null) {
            return new cached("",err);
        }
        defer(unlock());

        {
            var err = downloadZip(ctx, mod, zipfile);

            if (err != null) {
                return new cached("",err);
            }

        }

        return new cached(zipfile,nil);

    })._<cached>();
    return (c.zipfile, error.As(c.err)!);

});

private static error downloadZip(context.Context ctx, module.Version mod, @string zipfile) => func((defer, _, _) => {
    error err = default!;

    var (ctx, span) = trace.StartSpan(ctx, "modfetch.downloadZip " + zipfile);
    defer(span.Done()); 

    // Double-check that the zipfile was not created while we were waiting for
    // the lock in DownloadZip.
    var ziphashfile = zipfile + "hash";
    bool zipExists = default;    bool ziphashExists = default;

    {
        var err__prev1 = err;

        var (_, err) = os.Stat(zipfile);

        if (err == null) {
            zipExists = true;
        }
        err = err__prev1;

    }

    {
        var err__prev1 = err;

        (_, err) = os.Stat(ziphashfile);

        if (err == null) {
            ziphashExists = true;
        }
        err = err__prev1;

    }

    if (zipExists && ziphashExists) {
        return error.As(null!)!;
    }
    {
        var err__prev1 = err;

        var err = os.MkdirAll(filepath.Dir(zipfile), 0777);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    } 

    // Clean up any remaining tempfiles from previous runs.
    // This is only safe to do because the lock file ensures that their
    // writers are no longer active.
    var tmpPattern = filepath.Base(zipfile) + "*.tmp";
    {
        var err__prev1 = err;

        var (old, err) = filepath.Glob(filepath.Join(filepath.Dir(zipfile), tmpPattern));

        if (err == null) {
            foreach (var (_, path) in old) {
                os.Remove(path); // best effort
            }

        }
        err = err__prev1;

    } 

    // If the zip file exists, the ziphash file must have been deleted
    // or lost after a file system crash. Re-hash the zip without downloading.
    if (zipExists) {
        return error.As(hashZip(mod, zipfile, ziphashfile))!;
    }
    var (f, err) = os.CreateTemp(filepath.Dir(zipfile), tmpPattern);
    if (err != null) {
        return error.As(err)!;
    }
    defer(() => {
        if (err != null) {
            f.Close();
            os.Remove(f.Name());
        }
    }());

    error unrecoverableErr = default!;
    err = TryProxies(proxy => {
        if (unrecoverableErr != null) {
            return error.As(unrecoverableErr)!;
        }
        var repo = Lookup(proxy, mod.Path);
        err = repo.Zip(f, mod.Version);
        if (err != null) { 
            // Zip may have partially written to f before failing.
            // (Perhaps the server crashed while sending the file?)
            // Since we allow fallback on error in some cases, we need to fix up the
            // file to be empty again for the next attempt.
            {
                var err__prev2 = err;

                (_, err) = f.Seek(0, io.SeekStart);

                if (err != null) {
                    unrecoverableErr = error.As(err)!;
                    return error.As(err)!;
                }

                err = err__prev2;

            }

            {
                var err__prev2 = err;

                err = f.Truncate(0);

                if (err != null) {
                    unrecoverableErr = error.As(err)!;
                    return error.As(err)!;
                }

                err = err__prev2;

            }

        }
        return error.As(err)!;

    });
    if (err != null) {
        return error.As(err)!;
    }
    var (fi, err) = f.Stat();
    if (err != null) {
        return error.As(err)!;
    }
    var (z, err) = zip.NewReader(f, fi.Size());
    if (err != null) {
        return error.As(err)!;
    }
    var prefix = mod.Path + "@" + mod.Version + "/";
    {
        var f__prev1 = f;

        foreach (var (_, __f) in z.File) {
            f = __f;
            if (!strings.HasPrefix(f.Name, prefix)) {
                return error.As(fmt.Errorf("zip for %s has unexpected file %s", prefix[..(int)len(prefix) - 1], f.Name))!;
            }
        }
        f = f__prev1;
    }

    {
        var err__prev1 = err;

        err = f.Close();

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    } 

    // Hash the zip file and check the sum before renaming to the final location.
    {
        var err__prev1 = err;

        err = hashZip(mod, f.Name(), ziphashfile);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    {
        var err__prev1 = err;

        err = os.Rename(f.Name(), zipfile);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    } 

    // TODO(bcmills): Should we make the .zip and .ziphash files read-only to discourage tampering?

    return error.As(null!)!;

});

// hashZip reads the zip file opened in f, then writes the hash to ziphashfile,
// overwriting that file if it exists.
//
// If the hash does not match go.sum (or the sumdb if enabled), hashZip returns
// an error and does not write ziphashfile.
private static error hashZip(module.Version mod, @string zipfile, @string ziphashfile) {
    var (hash, err) = dirhash.HashZip(zipfile, dirhash.DefaultHash);
    if (err != null) {
        return error.As(err)!;
    }
    {
        var err__prev1 = err;

        var err = checkModSum(mod, hash);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    var (hf, err) = lockedfile.Create(ziphashfile);
    if (err != null) {
        return error.As(err)!;
    }
    {
        var err__prev1 = err;

        err = hf.Truncate(int64(len(hash)));

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    {
        var err__prev1 = err;

        var (_, err) = hf.WriteAt((slice<byte>)hash, 0);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    {
        var err__prev1 = err;

        err = hf.Close();

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }


    return error.As(null!)!;

}

// makeDirsReadOnly makes a best-effort attempt to remove write permissions for dir
// and its transitive contents.
private static void makeDirsReadOnly(@string dir) {
    private partial struct pathMode {
        public @string path;
        public fs.FileMode mode;
    }
    slice<pathMode> dirs = default; // in lexical order
    filepath.WalkDir(dir, (path, d, err) => {
        if (err == null && d.IsDir()) {
            var (info, err) = d.Info();
            if (err == null && info.Mode() & 0222 != 0) {
                dirs = append(dirs, new pathMode(path,info.Mode()));
            }
        }
        return null;

    }); 

    // Run over list backward to chmod children before parents.
    for (var i = len(dirs) - 1; i >= 0; i--) {
        os.Chmod(dirs[i].path, dirs[i].mode & ~0222);
    }

}

// RemoveAll removes a directory written by Download or Unzip, first applying
// any permission changes needed to do so.
public static error RemoveAll(@string dir) { 
    // Module cache has 0555 directories; make them writable in order to remove content.
    filepath.WalkDir(dir, (path, info, err) => {
        if (err != null) {
            return error.As(null!)!; // ignore errors walking in file system
        }
        if (info.IsDir()) {
            os.Chmod(path, 0777);
        }
        return error.As(null!)!;

    });
    return error.As(robustio.RemoveAll(dir))!;

}

public static @string GoSumFile = default; // path to go.sum; set by package modload

private partial struct modSum {
    public module.Version mod;
    public @string sum;
}

private static var goSum = default;

private partial struct modSumStatus {
    public bool used;
    public bool dirty;
}

// initGoSum initializes the go.sum data.
// The boolean it returns reports whether the
// use of go.sum is now enabled.
// The goSum lock must be held.
private static (bool, error) initGoSum() {
    bool _p0 = default;
    error _p0 = default!;

    if (GoSumFile == "") {
        return (false, error.As(null!)!);
    }
    if (goSum.m != null) {
        return (true, error.As(null!)!);
    }
    goSum.m = make_map<module.Version, slice<@string>>();
    goSum.status = make_map<modSum, modSumStatus>();
    slice<byte> data = default;    error err = default!;
    {
        var (actualSumFile, ok) = fsys.OverlayPath(GoSumFile);

        if (ok) { 
            // Don't lock go.sum if it's part of the overlay.
            // On Plan 9, locking requires chmod, and we don't want to modify any file
            // in the overlay. See #44700.
            data, err = os.ReadFile(actualSumFile);

        }
        else
 {
            data, err = lockedfile.Read(GoSumFile);
        }
    }

    if (err != null && !os.IsNotExist(err)) {
        return (false, error.As(err)!);
    }
    goSum.enabled = true;
    readGoSum(goSum.m, GoSumFile, data);

    return (true, error.As(null!)!);

}

// emptyGoModHash is the hash of a 1-file tree containing a 0-length go.mod.
// A bug caused us to write these into go.sum files for non-modules.
// We detect and remove them.
private static readonly @string emptyGoModHash = "h1:G7mAYYxgmS0lVkHyy2hEOLQCFB0DlQFTMLWggykrydY=";

// readGoSum parses data, which is the content of file,
// and adds it to goSum.m. The goSum lock must be held.


// readGoSum parses data, which is the content of file,
// and adds it to goSum.m. The goSum lock must be held.
private static error readGoSum(map<module.Version, slice<@string>> dst, @string file, slice<byte> data) {
    nint lineno = 0;
    while (len(data) > 0) {
        slice<byte> line = default;
        lineno++;
        var i = bytes.IndexByte(data, '\n');
        if (i < 0) {
            (line, data) = (data, null);
        }
        else
 {
            (line, data) = (data[..(int)i], data[(int)i + 1..]);
        }
        var f = strings.Fields(string(line));
        if (len(f) == 0) { 
            // blank line; skip it
            continue;

        }
        if (len(f) != 3) {
            return error.As(fmt.Errorf("malformed go.sum:\n%s:%d: wrong number of fields %v", file, lineno, len(f)))!;
        }
        if (f[2] == emptyGoModHash) { 
            // Old bug; drop it.
            continue;

        }
        module.Version mod = new module.Version(Path:f[0],Version:f[1]);
        dst[mod] = append(dst[mod], f[2]);

    }
    return error.As(null!)!;

}

// HaveSum returns true if the go.sum file contains an entry for mod.
// The entry's hash must be generated with a known hash algorithm.
// mod.Version may have a "/go.mod" suffix to distinguish sums for
// .mod and .zip files.
public static bool HaveSum(module.Version mod) => func((defer, _, _) => {
    goSum.mu.Lock();
    defer(goSum.mu.Unlock());
    var (inited, err) = initGoSum();
    if (err != null || !inited) {
        return false;
    }
    foreach (var (_, h) in goSum.m[mod]) {
        if (!strings.HasPrefix(h, "h1:")) {
            continue;
        }
        if (!goSum.status[new modSum(mod,h)].dirty) {
            return true;
        }
    }    return false;

});

// checkMod checks the given module's checksum.
private static void checkMod(module.Version mod) { 
    // Do the file I/O before acquiring the go.sum lock.
    var (ziphash, err) = CachePath(mod, "ziphash");
    if (err != null) {
        @base.Fatalf("verifying %v", module.VersionError(mod, err));
    }
    var (data, err) = lockedfile.Read(ziphash);
    if (err != null) {
        @base.Fatalf("verifying %v", module.VersionError(mod, err));
    }
    data = bytes.TrimSpace(data);
    if (!isValidSum(data)) { 
        // Recreate ziphash file from zip file and use that to check the mod sum.
        var (zip, err) = CachePath(mod, "zip");
        if (err != null) {
            @base.Fatalf("verifying %v", module.VersionError(mod, err));
        }
        err = hashZip(mod, zip, ziphash);
        if (err != null) {
            @base.Fatalf("verifying %v", module.VersionError(mod, err));
        }
        return ;

    }
    var h = string(data);
    if (!strings.HasPrefix(h, "h1:")) {
        @base.Fatalf("verifying %v", module.VersionError(mod, fmt.Errorf("unexpected ziphash: %q", h)));
    }
    {
        var err = checkModSum(mod, h);

        if (err != null) {
            @base.Fatalf("%s", err);
        }
    }

}

// goModSum returns the checksum for the go.mod contents.
private static (@string, error) goModSum(slice<byte> data) {
    @string _p0 = default;
    error _p0 = default!;

    return dirhash.Hash1(new slice<@string>(new @string[] { "go.mod" }), _p0 => {
        return (io.NopCloser(bytes.NewReader(data)), error.As(null!)!);
    });
}

// checkGoMod checks the given module's go.mod checksum;
// data is the go.mod content.
private static error checkGoMod(@string path, @string version, slice<byte> data) {
    var (h, err) = goModSum(data);
    if (err != null) {
        return error.As(addr(new module.ModuleError(Path:path,Version:version,Err:fmt.Errorf("verifying go.mod: %v",err)))!)!;
    }
    return error.As(checkModSum(new module.Version(Path:path,Version:version+"/go.mod"), h))!;

}

// checkModSum checks that the recorded checksum for mod is h.
//
// mod.Version may have the additional suffix "/go.mod" to request the checksum
// for the module's go.mod file only.
private static error checkModSum(module.Version mod, @string h) { 
    // We lock goSum when manipulating it,
    // but we arrange to release the lock when calling checkSumDB,
    // so that parallel calls to checkModHash can execute parallel calls
    // to checkSumDB.

    // Check whether mod+h is listed in go.sum already. If so, we're done.
    goSum.mu.Lock();
    var (inited, err) = initGoSum();
    if (err != null) {
        goSum.mu.Unlock();
        return error.As(err)!;
    }
    var done = inited && haveModSumLocked(mod, h);
    if (inited) {
        var st = goSum.status[new modSum(mod,h)];
        st.used = true;
        goSum.status[new modSum(mod,h)] = st;
    }
    goSum.mu.Unlock();

    if (done) {
        return error.As(null!)!;
    }
    if (useSumDB(mod)) { 
        // Calls base.Fatalf if mismatch detected.
        {
            var err = checkSumDB(mod, h);

            if (err != null) {
                return error.As(err)!;
            }

        }

    }
    if (inited) {
        goSum.mu.Lock();
        addModSumLocked(mod, h);
        st = goSum.status[new modSum(mod,h)];
        st.dirty = true;
        goSum.status[new modSum(mod,h)] = st;
        goSum.mu.Unlock();
    }
    return error.As(null!)!;

}

// haveModSumLocked reports whether the pair mod,h is already listed in go.sum.
// If it finds a conflicting pair instead, it calls base.Fatalf.
// goSum.mu must be locked.
private static bool haveModSumLocked(module.Version mod, @string h) {
    foreach (var (_, vh) in goSum.m[mod]) {
        if (h == vh) {
            return true;
        }
        if (strings.HasPrefix(vh, "h1:")) {
            @base.Fatalf("verifying %s@%s: checksum mismatch\n\tdownloaded: %v\n\tgo.sum:     %v" + goSumMismatch, mod.Path, mod.Version, h, vh);
        }
    }    return false;

}

// addModSumLocked adds the pair mod,h to go.sum.
// goSum.mu must be locked.
private static void addModSumLocked(module.Version mod, @string h) {
    if (haveModSumLocked(mod, h)) {
        return ;
    }
    if (len(goSum.m[mod]) > 0) {
        fmt.Fprintf(os.Stderr, "warning: verifying %s@%s: unknown hashes in go.sum: %v; adding %v" + hashVersionMismatch, mod.Path, mod.Version, strings.Join(goSum.m[mod], ", "), h);
    }
    goSum.m[mod] = append(goSum.m[mod], h);

}

// checkSumDB checks the mod, h pair against the Go checksum database.
// It calls base.Fatalf if the hash is to be rejected.
private static error checkSumDB(module.Version mod, @string h) {
    var modWithoutSuffix = mod;
    @string noun = "module";
    if (strings.HasSuffix(mod.Version, "/go.mod")) {
        noun = "go.mod";
        modWithoutSuffix.Version = strings.TrimSuffix(mod.Version, "/go.mod");
    }
    var (db, lines, err) = lookupSumDB(mod);
    if (err != null) {
        return error.As(module.VersionError(modWithoutSuffix, fmt.Errorf("verifying %s: %v", noun, err)))!;
    }
    var have = mod.Path + " " + mod.Version + " " + h;
    var prefix = mod.Path + " " + mod.Version + " h1:";
    foreach (var (_, line) in lines) {
        if (line == have) {
            return error.As(null!)!;
        }
        if (strings.HasPrefix(line, prefix)) {
            return error.As(module.VersionError(modWithoutSuffix, fmt.Errorf("verifying %s: checksum mismatch\n\tdownloaded: %v\n\t%s: %v" + sumdbMismatch, noun, h, db, line[(int)len(prefix) - len("h1:")..])))!;
        }
    }    return error.As(null!)!;

}

// Sum returns the checksum for the downloaded copy of the given module,
// if present in the download cache.
public static @string Sum(module.Version mod) {
    if (cfg.GOMODCACHE == "") { 
        // Do not use current directory.
        return "";

    }
    var (ziphash, err) = CachePath(mod, "ziphash");
    if (err != null) {
        return "";
    }
    var (data, err) = lockedfile.Read(ziphash);
    if (err != null) {
        return "";
    }
    data = bytes.TrimSpace(data);
    if (!isValidSum(data)) {
        return "";
    }
    return string(data);

}

// isValidSum returns true if data is the valid contents of a zip hash file.
// Certain critical files are written to disk by first truncating
// then writing the actual bytes, so that if the write fails
// the corrupt file should contain at least one of the null
// bytes written by the truncate operation.
private static bool isValidSum(slice<byte> data) {
    if (bytes.IndexByte(data, ' ') >= 0) {
        return false;
    }
    if (len(data) != len("h1:") + base64.StdEncoding.EncodedLen(sha256.Size)) {
        return false;
    }
    return true;

}

// WriteGoSum writes the go.sum file if it needs to be updated.
//
// keep is used to check whether a newly added sum should be saved in go.sum.
// It should have entries for both module content sums and go.mod sums
// (version ends with "/go.mod"). Existing sums will be preserved unless they
// have been marked for deletion with TrimGoSum.
public static void WriteGoSum(map<module.Version, bool> keep) => func((defer, _, _) => {
    goSum.mu.Lock();
    defer(goSum.mu.Unlock()); 

    // If we haven't read the go.sum file yet, don't bother writing it.
    if (!goSum.enabled) {
        return ;
    }
    var dirty = false;
Outer:
    {
        var m__prev1 = m;

        foreach (var (__m, __hs) in goSum.m) {
            m = __m;
            hs = __hs;
            {
                var h__prev2 = h;

                foreach (var (_, __h) in hs) {
                    h = __h;
                    var st = goSum.status[new modSum(m,h)];
                    if (st.dirty && (!st.used || keep[m])) {
                        dirty = true;
                        _breakOuter = true;
                        break;
                    }

                }

                h = h__prev2;
            }
        }
        m = m__prev1;
    }
    if (!dirty) {
        return ;
    }
    if (cfg.BuildMod == "readonly") {
        @base.Fatalf("go: updates to go.sum needed, disabled by -mod=readonly");
    }
    {
        var (_, ok) = fsys.OverlayPath(GoSumFile);

        if (ok) {
            @base.Fatalf("go: updates to go.sum needed, but go.sum is part of the overlay specified with -overlay");
        }
    } 

    // Make a best-effort attempt to acquire the side lock, only to exclude
    // previous versions of the 'go' command from making simultaneous edits.
    {
        var (unlock, err) = SideLock();

        if (err == null) {
            defer(unlock());
        }
    }


    var err = lockedfile.Transform(GoSumFile, data => {
        if (!goSum.overwrite) { 
            // Incorporate any sums added by other processes in the meantime.
            // Add only the sums that we actually checked: the user may have edited or
            // truncated the file to remove erroneous hashes, and we shouldn't restore
            // them without good reason.
            goSum.m = make_map<module.Version, slice<@string>>(len(goSum.m));
            readGoSum(goSum.m, GoSumFile, data);
            {
                var st__prev1 = st;

                foreach (var (__ms, __st) in goSum.status) {
                    ms = __ms;
                    st = __st;
                    if (st.used) {
                        addModSumLocked(ms.mod, ms.sum);
                    }
                }

                st = st__prev1;
            }
        }
        slice<module.Version> mods = default;
        {
            var m__prev1 = m;

            foreach (var (__m) in goSum.m) {
                m = __m;
                mods = append(mods, m);
            }

            m = m__prev1;
        }

        module.Sort(mods);

        ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
        {
            var m__prev1 = m;

            foreach (var (_, __m) in mods) {
                m = __m;
                var list = goSum.m[m];
                sort.Strings(list);
                {
                    var h__prev2 = h;

                    foreach (var (_, __h) in list) {
                        h = __h;
                        st = goSum.status[new modSum(m,h)];
                        if (!st.dirty || (st.used && keep[m])) {
                            fmt.Fprintf(_addr_buf, "%s %s %s\n", m.Path, m.Version, h);
                        }
                    }

                    h = h__prev2;
                }
            }

            m = m__prev1;
        }

        return (buf.Bytes(), null);

    });

    if (err != null) {
        @base.Fatalf("go: updating go.sum: %v", err);
    }
    goSum.status = make_map<modSum, modSumStatus>();
    goSum.overwrite = false;

});

// TrimGoSum trims go.sum to contain only the modules needed for reproducible
// builds.
//
// keep is used to check whether a sum should be retained in go.mod. It should
// have entries for both module content sums and go.mod sums (version ends
// with "/go.mod").
public static void TrimGoSum(map<module.Version, bool> keep) => func((defer, _, _) => {
    goSum.mu.Lock();
    defer(goSum.mu.Unlock());
    var (inited, err) = initGoSum();
    if (err != null) {
        @base.Fatalf("%s", err);
    }
    if (!inited) {
        return ;
    }
    foreach (var (m, hs) in goSum.m) {
        if (!keep[m]) {
            foreach (var (_, h) in hs) {
                goSum.status[new modSum(m,h)] = new modSumStatus(used:false,dirty:true);
            }
            goSum.overwrite = true;
        }
    }
});

private static readonly @string goSumMismatch = "\n\nSECURITY ERROR\nThis download does NOT match an earlier download recorded in go." +
    "sum.\nThe bits may have been replaced on the origin server, or an attacker may\nha" +
    "ve intercepted the download attempt.\n\nFor more information, see \'go help module-" +
    "auth\'.\n";



private static readonly @string sumdbMismatch = "\n\nSECURITY ERROR\nThis download does NOT match the one reported by the checksum se" +
    "rver.\nThe bits may have been replaced on the origin server, or an attacker may\nh" +
    "ave intercepted the download attempt.\n\nFor more information, see \'go help module" +
    "-auth\'.\n";



private static readonly @string hashVersionMismatch = "\n\nSECURITY WARNING\nThis download is listed in go.sum, but using an unknown hash a" +
    "lgorithm.\nThe download cannot be verified.\n\nFor more information, see \'go help m" +
    "odule-auth\'.\n\n";



public static ptr<base.Command> HelpModuleAuth = addr(new base.Command(UsageLine:"module-auth",Short:"module authentication using go.sum",Long:`
When the go command downloads a module zip file or go.mod file into the
module cache, it computes a cryptographic hash and compares it with a known
value to verify the file hasn't changed since it was first downloaded. Known
hashes are stored in a file in the module root directory named go.sum. Hashes
may also be downloaded from the checksum database depending on the values of
GOSUMDB, GOPRIVATE, and GONOSUMDB.

For details, see https://golang.org/ref/mod#authenticating.
`,));

public static ptr<base.Command> HelpPrivate = addr(new base.Command(UsageLine:"private",Short:"configuration for downloading non-public code",Long:`
The go command defaults to downloading modules from the public Go module
mirror at proxy.golang.org. It also defaults to validating downloaded modules,
regardless of source, against the public Go checksum database at sum.golang.org.
These defaults work well for publicly available source code.

The GOPRIVATE environment variable controls which modules the go command
considers to be private (not available publicly) and should therefore not use
the proxy or checksum database. The variable is a comma-separated list of
glob patterns (in the syntax of Go's path.Match) of module path prefixes.
For example,

	GOPRIVATE=*.corp.example.com,rsc.io/private

causes the go command to treat as private any module with a path prefix
matching either pattern, including git.corp.example.com/xyzzy, rsc.io/private,
and rsc.io/private/quux.

For fine-grained control over module download and validation, the GONOPROXY
and GONOSUMDB environment variables accept the same kind of glob list
and override GOPRIVATE for the specific decision of whether to use the proxy
and checksum database, respectively.

For example, if a company ran a module proxy serving private modules,
users would configure go using:

	GOPRIVATE=*.corp.example.com
	GOPROXY=proxy.example.com
	GONOPROXY=none

The GOPRIVATE variable is also used to define the "public" and "private"
patterns for the GOVCS variable; see 'go help vcs'. For that usage,
GOPRIVATE applies even in GOPATH mode. In that case, it matches import paths
instead of module paths.

The 'go env -w' command (see 'go help env') can be used to set these variables
for future go command invocations.

For more details, see https://golang.org/ref/mod#private-modules.
`,));

} // end modfetch_package
