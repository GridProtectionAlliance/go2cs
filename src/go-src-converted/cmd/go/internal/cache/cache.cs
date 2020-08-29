// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package cache implements a build artifact cache.
// package cache -- go2cs converted at 2020 August 29 10:00:51 UTC
// import "cmd/go/internal/cache" ==> using cache = go.cmd.go.@internal.cache_package
// Original source: C:\Go\src\cmd\go\internal\cache\cache.go
using bytes = go.bytes_package;
using sha256 = go.crypto.sha256_package;
using hex = go.encoding.hex_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using time = go.time_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class cache_package
    {
        // An ActionID is a cache action key, the hash of a complete description of a
        // repeatable computation (command line, environment variables,
        // input file contents, executable contents).
        public partial struct ActionID // : array<byte>
        {
        }

        // An OutputID is a cache output key, the hash of an output of a computation.
        public partial struct OutputID // : array<byte>
        {
        }

        // A Cache is a package cache, backed by a file system directory tree.
        public partial struct Cache
        {
            public @string dir;
            public ptr<os.File> log;
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
        public static (ref Cache, error) Open(@string dir)
        {
            var (info, err) = os.Stat(dir);
            if (err != null)
            {
                return (null, err);
            }
            if (!info.IsDir())
            {
                return (null, ref new os.PathError(Op:"open",Path:dir,Err:fmt.Errorf("not a directory")));
            }
            for (long i = 0L; i < 256L; i++)
            {
                var name = filepath.Join(dir, fmt.Sprintf("%02x", i));
                {
                    var err = os.MkdirAll(name, 0777L);

                    if (err != null)
                    {
                        return (null, err);
                    }

                }
            }

            var (f, err) = os.OpenFile(filepath.Join(dir, "log.txt"), os.O_WRONLY | os.O_APPEND | os.O_CREATE, 0666L);
            if (err != null)
            {
                return (null, err);
            }
            Cache c = ref new Cache(dir:dir,log:f,now:time.Now,);
            return (c, null);
        }

        // fileName returns the name of the file corresponding to the given id.
        private static @string fileName(this ref Cache c, array<byte> id, @string key)
        {
            return filepath.Join(c.dir, fmt.Sprintf("%02x", id[0L]), fmt.Sprintf("%x", id) + "-" + key);
        }

        private static var errMissing = errors.New("cache entry not found");

 
        // action entry file is "v1 <hex id> <hex out> <decimal size space-padded to 20 bytes> <unixnano space-padded to 20 bytes>\n"
        private static readonly var hexSize = HashSize * 2L;
        private static readonly long entrySize = 2L + 1L + hexSize + 1L + hexSize + 1L + 20L + 1L + 20L + 1L;

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

        // DebugTest is set when GODEBUG=gocachetest=1 is in the environment.
        public static var DebugTest = false;

        private static void init()
        {
            initEnv();

        }

        private static void initEnv()
        {
            verify = false;
            debugHash = false;
            var debug = strings.Split(os.Getenv("GODEBUG"), ",");
            foreach (var (_, f) in debug)
            {
                if (f == "gocacheverify=1")
                {
                    verify = true;
                }
                if (f == "gocachehash=1")
                {
                    debugHash = true;
                }
                if (f == "gocachetest=1")
                {
                    DebugTest = true;
                }
            }
        }

        // Get looks up the action ID in the cache,
        // returning the corresponding output ID and file size, if any.
        // Note that finding an output ID does not guarantee that the
        // saved file for that output ID is still available.
        private static (Entry, error) Get(this ref Cache c, ActionID id)
        {
            if (verify)
            {
                return (new Entry(), errMissing);
            }
            return c.get(id);
        }

        public partial struct Entry
        {
            public OutputID OutputID;
            public long Size;
            public time.Time Time;
        }

        // get is Get but does not respect verify mode, so that Put can use it.
        private static (Entry, error) get(this ref Cache _c, ActionID id) => func(_c, (ref Cache c, Defer defer, Panic _, Recover __) =>
        {
            Func<(Entry, error)> missing = () =>
            {
                fmt.Fprintf(c.log, "%d miss %x\n", c.now().Unix(), id);
                return (new Entry(), errMissing);
            }
;
            var (f, err) = os.Open(c.fileName(id, "a"));
            if (err != null)
            {
                return missing();
            }
            defer(f.Close());
            var entry = make_slice<byte>(entrySize + 1L); // +1 to detect whether f is too long
            {
                var (n, err) = io.ReadFull(f, entry);

                if (n != entrySize || err != io.ErrUnexpectedEOF)
                {
                    return missing();
                }

            }
            if (entry[0L] != 'v' || entry[1L] != '1' || entry[2L] != ' ' || entry[3L + hexSize] != ' ' || entry[3L + hexSize + 1L + hexSize] != ' ' || entry[3L + hexSize + 1L + hexSize + 1L + 20L] != ' ' || entry[entrySize - 1L] != '\n')
            {
                return missing();
            }
            var eid = entry[3L..3L + hexSize];
            entry = entry[3L + hexSize..];
            var eout = entry[1L..1L + hexSize];
            entry = entry[1L + hexSize..];
            var esize = entry[1L..1L + 20L];
            entry = entry[1L + 20L..];
            var etime = entry[1L..1L + 20L];
            entry = entry[1L + 20L..];
            array<byte> buf = new array<byte>(HashSize);
            {
                var (_, err) = hex.Decode(buf[..], eid);

                if (err != null || buf != id)
                {
                    return missing();
                }

            }
            {
                (_, err) = hex.Decode(buf[..], eout);

                if (err != null)
                {
                    return missing();
                }

            }
            long i = 0L;
            while (i < len(esize) && esize[i] == ' ')
            {
                i++;
            }

            var (size, err) = strconv.ParseInt(string(esize[i..]), 10L, 64L);
            if (err != null || size < 0L)
            {
                return missing();
            }
            i = 0L;
            while (i < len(etime) && etime[i] == ' ')
            {
                i++;
            }

            var (tm, err) = strconv.ParseInt(string(etime[i..]), 10L, 64L);
            if (err != null || size < 0L)
            {
                return missing();
            }
            fmt.Fprintf(c.log, "%d get %x\n", c.now().Unix(), id);

            c.used(c.fileName(id, "a"));

            return (new Entry(buf,size,time.Unix(0,tm)), null);
        });

        // GetBytes looks up the action ID in the cache and returns
        // the corresponding output bytes.
        // GetBytes should only be used for data that can be expected to fit in memory.
        private static (slice<byte>, Entry, error) GetBytes(this ref Cache c, ActionID id)
        {
            var (entry, err) = c.Get(id);
            if (err != null)
            {
                return (null, entry, err);
            }
            var (data, _) = ioutil.ReadFile(c.OutputFile(entry.OutputID));
            if (sha256.Sum256(data) != entry.OutputID)
            {
                return (null, entry, errMissing);
            }
            return (data, entry, null);
        }

        // OutputFile returns the name of the cache file storing output with the given OutputID.
        private static @string OutputFile(this ref Cache c, OutputID @out)
        {
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
        private static readonly long mtimeInterval = 1L * time.Hour;
        private static readonly long trimInterval = 24L * time.Hour;
        private static readonly long trimLimit = 5L * 24L * time.Hour;

        // used makes a best-effort attempt to update mtime on file,
        // so that mtime reflects cache access time.
        //
        // Because the reflection only needs to be approximate,
        // and to reduce the amount of disk activity caused by using
        // cache entries, used only updates the mtime if the current
        // mtime is more than an hour old. This heuristic eliminates
        // nearly all of the mtime updates that would otherwise happen,
        // while still keeping the mtimes useful for cache trimming.
        private static void used(this ref Cache c, @string file)
        {
            var (info, err) = os.Stat(file);
            if (err == null && c.now().Sub(info.ModTime()) < mtimeInterval)
            {
                return;
            }
            os.Chtimes(file, c.now(), c.now());
        }

        // Trim removes old cache entries that are likely not to be reused.
        private static void Trim(this ref Cache c)
        {
            var now = c.now(); 

            // We maintain in dir/trim.txt the time of the last completed cache trim.
            // If the cache has been trimmed recently enough, do nothing.
            // This is the common case.
            var (data, _) = ioutil.ReadFile(filepath.Join(c.dir, "trim.txt"));
            var (t, err) = strconv.ParseInt(strings.TrimSpace(string(data)), 10L, 64L);
            if (err == null && now.Sub(time.Unix(t, 0L)) < trimInterval)
            {
                return;
            } 

            // Trim each of the 256 subdirectories.
            // We subtract an additional mtimeInterval
            // to account for the imprecision of our "last used" mtimes.
            var cutoff = now.Add(-trimLimit - mtimeInterval);
            for (long i = 0L; i < 256L; i++)
            {
                var subdir = filepath.Join(c.dir, fmt.Sprintf("%02x", i));
                c.trimSubdir(subdir, cutoff);
            }


            ioutil.WriteFile(filepath.Join(c.dir, "trim.txt"), (slice<byte>)fmt.Sprintf("%d", now.Unix()), 0666L);
        }

        // trimSubdir trims a single cache subdirectory.
        private static void trimSubdir(this ref Cache c, @string subdir, time.Time cutoff)
        { 
            // Read all directory entries from subdir before removing
            // any files, in case removing files invalidates the file offset
            // in the directory scan. Also, ignore error from f.Readdirnames,
            // because we don't care about reporting the error and we still
            // want to process any entries found before the error.
            var (f, err) = os.Open(subdir);
            if (err != null)
            {
                return;
            }
            var (names, _) = f.Readdirnames(-1L);
            f.Close();

            foreach (var (_, name) in names)
            { 
                // Remove only cache entries (xxxx-a and xxxx-d).
                if (!strings.HasSuffix(name, "-a") && !strings.HasSuffix(name, "-d"))
                {
                    continue;
                }
                var entry = filepath.Join(subdir, name);
                var (info, err) = os.Stat(entry);
                if (err == null && info.ModTime().Before(cutoff))
                {
                    os.Remove(entry);
                }
            }
        }

        // putIndexEntry adds an entry to the cache recording that executing the action
        // with the given id produces an output with the given output id (hash) and size.
        private static error putIndexEntry(this ref Cache _c, ActionID id, OutputID @out, long size, bool allowVerify) => func(_c, (ref Cache c, Defer _, Panic panic, Recover __) =>
        { 
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
            slice<byte> entry = (slice<byte>)fmt.Sprintf("v1 %x %x %20d %20d\n", id, out, size, time.Now().UnixNano());
            if (verify && allowVerify)
            {
                var (old, err) = c.get(id);
                if (err == null && (old.OutputID != out || old.Size != size))
                { 
                    // panic to show stack trace, so we can see what code is generating this cache entry.
                    var msg = fmt.Sprintf("go: internal cache error: cache verify failed: id=%x changed:<<<\n%s\n>>>\nold: %x %d\nnew: %x %d", id, reverseHash(id), out, size, old.OutputID, old.Size);
                    panic(msg);
                }
            }
            var file = c.fileName(id, "a");
            {
                var err = ioutil.WriteFile(file, entry, 0666L);

                if (err != null)
                {
                    os.Remove(file);
                    return error.As(err);
                }

            }
            os.Chtimes(file, c.now(), c.now()); // mainly for tests

            fmt.Fprintf(c.log, "%d put %x %x %d\n", c.now().Unix(), id, out, size);
            return error.As(null);
        });

        // Put stores the given output in the cache as the output for the action ID.
        // It may read file twice. The content of file must not change between the two passes.
        private static (OutputID, long, error) Put(this ref Cache c, ActionID id, io.ReadSeeker file)
        {
            return c.put(id, file, true);
        }

        // PutNoVerify is like Put but disables the verify check
        // when GODEBUG=goverifycache=1 is set.
        // It is meant for data that is OK to cache but that we expect to vary slightly from run to run,
        // like test output containing times and the like.
        private static (OutputID, long, error) PutNoVerify(this ref Cache c, ActionID id, io.ReadSeeker file)
        {
            return c.put(id, file, false);
        }

        private static (OutputID, long, error) put(this ref Cache c, ActionID id, io.ReadSeeker file, bool allowVerify)
        { 
            // Compute output ID.
            var h = sha256.New();
            {
                var (_, err) = file.Seek(0L, 0L);

                if (err != null)
                {
                    return (new OutputID(), 0L, err);
                }

            }
            var (size, err) = io.Copy(h, file);
            if (err != null)
            {
                return (new OutputID(), 0L, err);
            }
            OutputID @out = default;
            h.Sum(out[..0L]); 

            // Copy to cached output file (if not already present).
            {
                var err = c.copyFile(file, out, size);

                if (err != null)
                {
                    return (out, size, err);
                } 

                // Add to cache index.

            } 

            // Add to cache index.
            return (out, size, c.putIndexEntry(id, out, size, allowVerify));
        }

        // PutBytes stores the given bytes in the cache as the output for the action ID.
        private static error PutBytes(this ref Cache c, ActionID id, slice<byte> data)
        {
            var (_, _, err) = c.Put(id, bytes.NewReader(data));
            return error.As(err);
        }

        // copyFile copies file into the cache, expecting it to have the given
        // output ID and size, if that file is not present already.
        private static error copyFile(this ref Cache _c, io.ReadSeeker file, OutputID @out, long size) => func(_c, (ref Cache c, Defer defer, Panic _, Recover __) =>
        {
            var name = c.fileName(out, "d");
            var (info, err) = os.Stat(name);
            if (err == null && info.Size() == size)
            { 
                // Check hash.
                {
                    var f__prev2 = f;

                    var (f, err) = os.Open(name);

                    if (err == null)
                    {
                        var h = sha256.New();
                        io.Copy(h, f);
                        f.Close();
                        OutputID out2 = default;
                        h.Sum(out2[..0L]);
                        if (out == out2)
                        {
                            return error.As(null);
                        }
                    } 
                    // Hash did not match. Fall through and rewrite file.

                    f = f__prev2;

                } 
                // Hash did not match. Fall through and rewrite file.
            } 

            // Copy file to cache directory.
            var mode = os.O_RDWR | os.O_CREATE;
            if (err == null && info.Size() > size)
            { // shouldn't happen but fix in case
                mode |= os.O_TRUNC;
            }
            (f, err) = os.OpenFile(name, mode, 0666L);
            if (err != null)
            {
                return error.As(err);
            }
            defer(f.Close());
            if (size == 0L)
            { 
                // File now exists with correct size.
                // Only one possible zero-length file, so contents are OK too.
                // Early return here makes sure there's a "last byte" for code below.
                return error.As(null);
            } 

            // From here on, if any of the I/O writing the file fails,
            // we make a best-effort attempt to truncate the file f
            // before returning, to avoid leaving bad bytes in the file.

            // Copy file to f, but also into h to double-check hash.
            {
                var (_, err) = file.Seek(0L, 0L);

                if (err != null)
                {
                    f.Truncate(0L);
                    return error.As(err);
                }

            }
            h = sha256.New();
            var w = io.MultiWriter(f, h);
            {
                (_, err) = io.CopyN(w, file, size - 1L);

                if (err != null)
                {
                    f.Truncate(0L);
                    return error.As(err);
                } 
                // Check last byte before writing it; writing it will make the size match
                // what other processes expect to find and might cause them to start
                // using the file.

            } 
            // Check last byte before writing it; writing it will make the size match
            // what other processes expect to find and might cause them to start
            // using the file.
            var buf = make_slice<byte>(1L);
            {
                (_, err) = file.Read(buf);

                if (err != null)
                {
                    f.Truncate(0L);
                    return error.As(err);
                }

            }
            h.Write(buf);
            var sum = h.Sum(null);
            if (!bytes.Equal(sum, out[..]))
            {
                f.Truncate(0L);
                return error.As(fmt.Errorf("file content changed underfoot"));
            } 

            // Commit cache file entry.
            {
                (_, err) = f.Write(buf);

                if (err != null)
                {
                    f.Truncate(0L);
                    return error.As(err);
                }

            }
            {
                var err = f.Close();

                if (err != null)
                { 
                    // Data might not have been written,
                    // but file may look like it is the right size.
                    // To be extra careful, remove cached file.
                    os.Remove(name);
                    return error.As(err);
                }

            }
            os.Chtimes(name, c.now(), c.now()); // mainly for tests

            return error.As(null);
        });
    }
}}}}
