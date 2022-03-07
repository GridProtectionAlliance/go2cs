// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package cache implements a build artifact cache.
// package cache -- go2cs converted at 2022 March 06 23:16:35 UTC
// import "cmd/go/internal/cache" ==> using cache = go.cmd.go.@internal.cache_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\cache\cache.go
using bytes = go.bytes_package;
using sha256 = go.crypto.sha256_package;
using hex = go.encoding.hex_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using fs = go.io.fs_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using time = go.time_package;

using lockedfile = go.cmd.go.@internal.lockedfile_package;
using System;


namespace go.cmd.go.@internal;

public static partial class cache_package {

    // An ActionID is a cache action key, the hash of a complete description of a
    // repeatable computation (command line, environment variables,
    // input file contents, executable contents).
public partial struct ActionID { // : array<byte>
}

// An OutputID is a cache output key, the hash of an output of a computation.
public partial struct OutputID { // : array<byte>
}

// A Cache is a package cache, backed by a file system directory tree.
public partial struct Cache {
    public @string dir;
    public Func<time.Time> now;
}

// Open opens and returns the cache in the given directory.
//
// It is safe for multiple processes on a single machine to use the
// same cache directory in a local file system simultaneously.
// They will coordinate using operating system file locks and may
// duplicate effort but will not corrupt the cache.
//
// However, it is NOT safe for multiple processes on different machines
// to share a cache directory (for example, if the directory were stored
// in a network file system). File locking is notoriously unreliable in
// network file systems and may not suffice to protect the cache.
//
public static (ptr<Cache>, error) Open(@string dir) {
    ptr<Cache> _p0 = default!;
    error _p0 = default!;

    var (info, err) = os.Stat(dir);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    if (!info.IsDir()) {
        return (_addr_null!, error.As(addr(new fs.PathError(Op:"open",Path:dir,Err:fmt.Errorf("not a directory")))!)!);
    }
    for (nint i = 0; i < 256; i++) {
        var name = filepath.Join(dir, fmt.Sprintf("%02x", i));
        {
            var err = os.MkdirAll(name, 0777);

            if (err != null) {
                return (_addr_null!, error.As(err)!);
            }

        }

    }
    ptr<Cache> c = addr(new Cache(dir:dir,now:time.Now,));
    return (_addr_c!, error.As(null!)!);

}

// fileName returns the name of the file corresponding to the given id.
private static @string fileName(this ptr<Cache> _addr_c, array<byte> id, @string key) {
    id = id.Clone();
    ref Cache c = ref _addr_c.val;

    return filepath.Join(c.dir, fmt.Sprintf("%02x", id[0]), fmt.Sprintf("%x", id) + "-" + key);
}

// An entryNotFoundError indicates that a cache entry was not found, with an
// optional underlying reason.
private partial struct entryNotFoundError {
    public error Err;
}

private static @string Error(this ptr<entryNotFoundError> _addr_e) {
    ref entryNotFoundError e = ref _addr_e.val;

    if (e.Err == null) {
        return "cache entry not found";
    }
    return fmt.Sprintf("cache entry not found: %v", e.Err);

}

private static error Unwrap(this ptr<entryNotFoundError> _addr_e) {
    ref entryNotFoundError e = ref _addr_e.val;

    return error.As(e.Err)!;
}

 
// action entry file is "v1 <hex id> <hex out> <decimal size space-padded to 20 bytes> <unixnano space-padded to 20 bytes>\n"
private static readonly var hexSize = HashSize * 2;
private static readonly nint entrySize = 2 + 1 + hexSize + 1 + hexSize + 1 + 20 + 1 + 20 + 1;


// verify controls whether to run the cache in verify mode.
// In verify mode, the cache always returns errMissing from Get
// but then double-checks in Put that the data being written
// exactly matches any existing entry. This provides an easy
// way to detect program behavior that would have been different
// had the cache entry been returned from Get.
//
// verify is enabled by setting the environment variable
// GODEBUG=gocacheverify=1.
private static var verify = false;

private static var errVerifyMode = errors.New("gocacheverify=1");

// DebugTest is set when GODEBUG=gocachetest=1 is in the environment.
public static var DebugTest = false;

private static void init() {
    initEnv();
}

private static void initEnv() {
    verify = false;
    debugHash = false;
    var debug = strings.Split(os.Getenv("GODEBUG"), ",");
    foreach (var (_, f) in debug) {
        if (f == "gocacheverify=1") {
            verify = true;
        }
        if (f == "gocachehash=1") {
            debugHash = true;
        }
        if (f == "gocachetest=1") {
            DebugTest = true;
        }
    }
}

// Get looks up the action ID in the cache,
// returning the corresponding output ID and file size, if any.
// Note that finding an output ID does not guarantee that the
// saved file for that output ID is still available.
private static (Entry, error) Get(this ptr<Cache> _addr_c, ActionID id) {
    Entry _p0 = default;
    error _p0 = default!;
    ref Cache c = ref _addr_c.val;

    if (verify) {
        return (new Entry(), error.As(addr(new entryNotFoundError(Err:errVerifyMode))!)!);
    }
    return c.get(id);

}

public partial struct Entry {
    public OutputID OutputID;
    public long Size;
    public time.Time Time;
}

// get is Get but does not respect verify mode, so that Put can use it.
private static (Entry, error) get(this ptr<Cache> _addr_c, ActionID id) => func((defer, _, _) => {
    Entry _p0 = default;
    error _p0 = default!;
    ref Cache c = ref _addr_c.val;

    Func<error, (Entry, error)> missing = reason => {
        return (new Entry(), error.As(addr(new entryNotFoundError(Err:reason))!)!);
    };
    var (f, err) = os.Open(c.fileName(id, "a"));
    if (err != null) {
        return missing(err);
    }
    defer(f.Close());
    var entry = make_slice<byte>(entrySize + 1); // +1 to detect whether f is too long
    {
        var (n, err) = io.ReadFull(f, entry);

        if (n > entrySize) {
            return missing(errors.New("too long"));
        }
        else if (err != io.ErrUnexpectedEOF) {
            if (err == io.EOF) {
                return missing(errors.New("file is empty"));
            }
            return missing(err);
        }
        else if (n < entrySize) {
            return missing(errors.New("entry file incomplete"));
        }

    }

    if (entry[0] != 'v' || entry[1] != '1' || entry[2] != ' ' || entry[3 + hexSize] != ' ' || entry[3 + hexSize + 1 + hexSize] != ' ' || entry[3 + hexSize + 1 + hexSize + 1 + 20] != ' ' || entry[entrySize - 1] != '\n') {
        return missing(errors.New("invalid header"));
    }
    var eid = entry[(int)3..(int)3 + hexSize];
    entry = entry[(int)3 + hexSize..];
    var eout = entry[(int)1..(int)1 + hexSize];
    entry = entry[(int)1 + hexSize..];
    var esize = entry[(int)1..(int)1 + 20];
    entry = entry[(int)1 + 20..];
    var etime = entry[(int)1..(int)1 + 20];
    entry = entry[(int)1 + 20..];
    array<byte> buf = new array<byte>(HashSize);
    {
        var (_, err) = hex.Decode(buf[..], eid);

        if (err != null) {
            return missing(fmt.Errorf("decoding ID: %v", err));
        }
        else if (buf != id) {
            return missing(errors.New("mismatched ID"));
        }

    }

    {
        (_, err) = hex.Decode(buf[..], eout);

        if (err != null) {
            return missing(fmt.Errorf("decoding output ID: %v", err));
        }
    }

    nint i = 0;
    while (i < len(esize) && esize[i] == ' ') {
        i++;
    }
    var (size, err) = strconv.ParseInt(string(esize[(int)i..]), 10, 64);
    if (err != null) {
        return missing(fmt.Errorf("parsing size: %v", err));
    }
    else if (size < 0) {
        return missing(errors.New("negative size"));
    }
    i = 0;
    while (i < len(etime) && etime[i] == ' ') {
        i++;
    }
    var (tm, err) = strconv.ParseInt(string(etime[(int)i..]), 10, 64);
    if (err != null) {
        return missing(fmt.Errorf("parsing timestamp: %v", err));
    }
    else if (tm < 0) {
        return missing(errors.New("negative timestamp"));
    }
    c.used(c.fileName(id, "a"));

    return (new Entry(buf,size,time.Unix(0,tm)), error.As(null!)!);

});

// GetFile looks up the action ID in the cache and returns
// the name of the corresponding data file.
private static (@string, Entry, error) GetFile(this ptr<Cache> _addr_c, ActionID id) {
    @string file = default;
    Entry entry = default;
    error err = default!;
    ref Cache c = ref _addr_c.val;

    entry, err = c.Get(id);
    if (err != null) {
        return ("", new Entry(), error.As(err)!);
    }
    file = c.OutputFile(entry.OutputID);
    var (info, err) = os.Stat(file);
    if (err != null) {
        return ("", new Entry(), error.As(addr(new entryNotFoundError(Err:err))!)!);
    }
    if (info.Size() != entry.Size) {
        return ("", new Entry(), error.As(addr(new entryNotFoundError(Err:errors.New("file incomplete")))!)!);
    }
    return (file, entry, error.As(null!)!);

}

// GetBytes looks up the action ID in the cache and returns
// the corresponding output bytes.
// GetBytes should only be used for data that can be expected to fit in memory.
private static (slice<byte>, Entry, error) GetBytes(this ptr<Cache> _addr_c, ActionID id) {
    slice<byte> _p0 = default;
    Entry _p0 = default;
    error _p0 = default!;
    ref Cache c = ref _addr_c.val;

    var (entry, err) = c.Get(id);
    if (err != null) {
        return (null, entry, error.As(err)!);
    }
    var (data, _) = os.ReadFile(c.OutputFile(entry.OutputID));
    if (sha256.Sum256(data) != entry.OutputID) {
        return (null, entry, error.As(addr(new entryNotFoundError(Err:errors.New("bad checksum")))!)!);
    }
    return (data, entry, error.As(null!)!);

}

// OutputFile returns the name of the cache file storing output with the given OutputID.
private static @string OutputFile(this ptr<Cache> _addr_c, OutputID @out) {
    ref Cache c = ref _addr_c.val;

    var file = c.fileName(out, "d");
    c.used(file);
    return file;
}

// Time constants for cache expiration.
//
// We set the mtime on a cache file on each use, but at most one per mtimeInterval (1 hour),
// to avoid causing many unnecessary inode updates. The mtimes therefore
// roughly reflect "time of last use" but may in fact be older by at most an hour.
//
// We scan the cache for entries to delete at most once per trimInterval (1 day).
//
// When we do scan the cache, we delete entries that have not been used for
// at least trimLimit (5 days). Statistics gathered from a month of usage by
// Go developers found that essentially all reuse of cached entries happened
// within 5 days of the previous reuse. See golang.org/issue/22990.
private static readonly nint mtimeInterval = 1 * time.Hour;
private static readonly nint trimInterval = 24 * time.Hour;
private static readonly nint trimLimit = 5 * 24 * time.Hour;


// used makes a best-effort attempt to update mtime on file,
// so that mtime reflects cache access time.
//
// Because the reflection only needs to be approximate,
// and to reduce the amount of disk activity caused by using
// cache entries, used only updates the mtime if the current
// mtime is more than an hour old. This heuristic eliminates
// nearly all of the mtime updates that would otherwise happen,
// while still keeping the mtimes useful for cache trimming.
private static void used(this ptr<Cache> _addr_c, @string file) {
    ref Cache c = ref _addr_c.val;

    var (info, err) = os.Stat(file);
    if (err == null && c.now().Sub(info.ModTime()) < mtimeInterval) {
        return ;
    }
    os.Chtimes(file, c.now(), c.now());

}

// Trim removes old cache entries that are likely not to be reused.
private static void Trim(this ptr<Cache> _addr_c) {
    ref Cache c = ref _addr_c.val;

    var now = c.now(); 

    // We maintain in dir/trim.txt the time of the last completed cache trim.
    // If the cache has been trimmed recently enough, do nothing.
    // This is the common case.
    // If the trim file is corrupt, detected if the file can't be parsed, or the
    // trim time is too far in the future, attempt the trim anyway. It's possible that
    // the cache was full when the corruption happened. Attempting a trim on
    // an empty cache is cheap, so there wouldn't be a big performance hit in that case.
    {
        var (data, err) = lockedfile.Read(filepath.Join(c.dir, "trim.txt"));

        if (err == null) {
            {
                var (t, err) = strconv.ParseInt(strings.TrimSpace(string(data)), 10, 64);

                if (err == null) {
                    var lastTrim = time.Unix(t, 0);
                    {
                        var d = now.Sub(lastTrim);

                        if (d < trimInterval && d > -mtimeInterval) {
                            return ;
                        }

                    }

                }

            }

        }
    } 

    // Trim each of the 256 subdirectories.
    // We subtract an additional mtimeInterval
    // to account for the imprecision of our "last used" mtimes.
    var cutoff = now.Add(-trimLimit - mtimeInterval);
    for (nint i = 0; i < 256; i++) {
        var subdir = filepath.Join(c.dir, fmt.Sprintf("%02x", i));
        c.trimSubdir(subdir, cutoff);
    } 

    // Ignore errors from here: if we don't write the complete timestamp, the
    // cache will appear older than it is, and we'll trim it again next time.
    ref bytes.Buffer b = ref heap(out ptr<bytes.Buffer> _addr_b);
    fmt.Fprintf(_addr_b, "%d", now.Unix());
    {
        var err = lockedfile.Write(filepath.Join(c.dir, "trim.txt"), _addr_b, 0666);

        if (err != null) {
            return ;
        }
    }

}

// trimSubdir trims a single cache subdirectory.
private static void trimSubdir(this ptr<Cache> _addr_c, @string subdir, time.Time cutoff) {
    ref Cache c = ref _addr_c.val;
 
    // Read all directory entries from subdir before removing
    // any files, in case removing files invalidates the file offset
    // in the directory scan. Also, ignore error from f.Readdirnames,
    // because we don't care about reporting the error and we still
    // want to process any entries found before the error.
    var (f, err) = os.Open(subdir);
    if (err != null) {
        return ;
    }
    var (names, _) = f.Readdirnames(-1);
    f.Close();

    foreach (var (_, name) in names) { 
        // Remove only cache entries (xxxx-a and xxxx-d).
        if (!strings.HasSuffix(name, "-a") && !strings.HasSuffix(name, "-d")) {
            continue;
        }
        var entry = filepath.Join(subdir, name);
        var (info, err) = os.Stat(entry);
        if (err == null && info.ModTime().Before(cutoff)) {
            os.Remove(entry);
        }
    }
}

// putIndexEntry adds an entry to the cache recording that executing the action
// with the given id produces an output with the given output id (hash) and size.
private static error putIndexEntry(this ptr<Cache> _addr_c, ActionID id, OutputID @out, long size, bool allowVerify) => func((_, panic, _) => {
    ref Cache c = ref _addr_c.val;
 
    // Note: We expect that for one reason or another it may happen
    // that repeating an action produces a different output hash
    // (for example, if the output contains a time stamp or temp dir name).
    // While not ideal, this is also not a correctness problem, so we
    // don't make a big deal about it. In particular, we leave the action
    // cache entries writable specifically so that they can be overwritten.
    //
    // Setting GODEBUG=gocacheverify=1 does make a big deal:
    // in verify mode we are double-checking that the cache entries
    // are entirely reproducible. As just noted, this may be unrealistic
    // in some cases but the check is also useful for shaking out real bugs.
    var entry = fmt.Sprintf("v1 %x %x %20d %20d\n", id, out, size, time.Now().UnixNano());
    if (verify && allowVerify) {
        var (old, err) = c.get(id);
        if (err == null && (old.OutputID != out || old.Size != size)) { 
            // panic to show stack trace, so we can see what code is generating this cache entry.
            var msg = fmt.Sprintf("go: internal cache error: cache verify failed: id=%x changed:<<<\n%s\n>>>\nold: %x %d\nnew: %x %d", id, reverseHash(id), out, size, old.OutputID, old.Size);
            panic(msg);

        }
    }
    var file = c.fileName(id, "a"); 

    // Copy file to cache directory.
    var mode = os.O_WRONLY | os.O_CREATE;
    var (f, err) = os.OpenFile(file, mode, 0666);
    if (err != null) {
        return error.As(err)!;
    }
    _, err = f.WriteString(entry);
    if (err == null) { 
        // Truncate the file only *after* writing it.
        // (This should be a no-op, but truncate just in case of previous corruption.)
        //
        // This differs from os.WriteFile, which truncates to 0 *before* writing
        // via os.O_TRUNC. Truncating only after writing ensures that a second write
        // of the same content to the same file is idempotent, and does not — even
        // temporarily! — undo the effect of the first write.
        err = f.Truncate(int64(len(entry)));

    }
    {
        var closeErr = f.Close();

        if (err == null) {
            err = closeErr;
        }
    }

    if (err != null) { 
        // TODO(bcmills): This Remove potentially races with another go command writing to file.
        // Can we eliminate it?
        os.Remove(file);
        return error.As(err)!;

    }
    os.Chtimes(file, c.now(), c.now()); // mainly for tests

    return error.As(null!)!;

});

// Put stores the given output in the cache as the output for the action ID.
// It may read file twice. The content of file must not change between the two passes.
private static (OutputID, long, error) Put(this ptr<Cache> _addr_c, ActionID id, io.ReadSeeker file) {
    OutputID _p0 = default;
    long _p0 = default;
    error _p0 = default!;
    ref Cache c = ref _addr_c.val;

    return c.put(id, file, true);
}

// PutNoVerify is like Put but disables the verify check
// when GODEBUG=goverifycache=1 is set.
// It is meant for data that is OK to cache but that we expect to vary slightly from run to run,
// like test output containing times and the like.
private static (OutputID, long, error) PutNoVerify(this ptr<Cache> _addr_c, ActionID id, io.ReadSeeker file) {
    OutputID _p0 = default;
    long _p0 = default;
    error _p0 = default!;
    ref Cache c = ref _addr_c.val;

    return c.put(id, file, false);
}

private static (OutputID, long, error) put(this ptr<Cache> _addr_c, ActionID id, io.ReadSeeker file, bool allowVerify) {
    OutputID _p0 = default;
    long _p0 = default;
    error _p0 = default!;
    ref Cache c = ref _addr_c.val;
 
    // Compute output ID.
    var h = sha256.New();
    {
        var (_, err) = file.Seek(0, 0);

        if (err != null) {
            return (new OutputID(), 0, error.As(err)!);
        }
    }

    var (size, err) = io.Copy(h, file);
    if (err != null) {
        return (new OutputID(), 0, error.As(err)!);
    }
    OutputID @out = default;
    h.Sum(out[..(int)0]); 

    // Copy to cached output file (if not already present).
    {
        var err = c.copyFile(file, out, size);

        if (err != null) {
            return (out, size, error.As(err)!);
        }
    } 

    // Add to cache index.
    return (out, size, error.As(c.putIndexEntry(id, out, size, allowVerify))!);

}

// PutBytes stores the given bytes in the cache as the output for the action ID.
private static error PutBytes(this ptr<Cache> _addr_c, ActionID id, slice<byte> data) {
    ref Cache c = ref _addr_c.val;

    var (_, _, err) = c.Put(id, bytes.NewReader(data));
    return error.As(err)!;
}

// copyFile copies file into the cache, expecting it to have the given
// output ID and size, if that file is not present already.
private static error copyFile(this ptr<Cache> _addr_c, io.ReadSeeker file, OutputID @out, long size) => func((defer, _, _) => {
    ref Cache c = ref _addr_c.val;

    var name = c.fileName(out, "d");
    var (info, err) = os.Stat(name);
    if (err == null && info.Size() == size) { 
        // Check hash.
        {
            var f__prev2 = f;

            var (f, err) = os.Open(name);

            if (err == null) {
                var h = sha256.New();
                io.Copy(h, f);
                f.Close();
                OutputID out2 = default;
                h.Sum(out2[..(int)0]);
                if (out == out2) {
                    return error.As(null!)!;
                }
            } 
            // Hash did not match. Fall through and rewrite file.

            f = f__prev2;

        } 
        // Hash did not match. Fall through and rewrite file.
    }
    var mode = os.O_RDWR | os.O_CREATE;
    if (err == null && info.Size() > size) { // shouldn't happen but fix in case
        mode |= os.O_TRUNC;

    }
    (f, err) = os.OpenFile(name, mode, 0666);
    if (err != null) {
        return error.As(err)!;
    }
    defer(f.Close());
    if (size == 0) { 
        // File now exists with correct size.
        // Only one possible zero-length file, so contents are OK too.
        // Early return here makes sure there's a "last byte" for code below.
        return error.As(null!)!;

    }
    {
        var (_, err) = file.Seek(0, 0);

        if (err != null) {
            f.Truncate(0);
            return error.As(err)!;
        }
    }

    h = sha256.New();
    var w = io.MultiWriter(f, h);
    {
        (_, err) = io.CopyN(w, file, size - 1);

        if (err != null) {
            f.Truncate(0);
            return error.As(err)!;
        }
    } 
    // Check last byte before writing it; writing it will make the size match
    // what other processes expect to find and might cause them to start
    // using the file.
    var buf = make_slice<byte>(1);
    {
        (_, err) = file.Read(buf);

        if (err != null) {
            f.Truncate(0);
            return error.As(err)!;
        }
    }

    h.Write(buf);
    var sum = h.Sum(null);
    if (!bytes.Equal(sum, out[..])) {
        f.Truncate(0);
        return error.As(fmt.Errorf("file content changed underfoot"))!;
    }
    {
        (_, err) = f.Write(buf);

        if (err != null) {
            f.Truncate(0);
            return error.As(err)!;
        }
    }

    {
        var err = f.Close();

        if (err != null) { 
            // Data might not have been written,
            // but file may look like it is the right size.
            // To be extra careful, remove cached file.
            os.Remove(name);
            return error.As(err)!;

        }
    }

    os.Chtimes(name, c.now(), c.now()); // mainly for tests

    return error.As(null!)!;

});

} // end cache_package
