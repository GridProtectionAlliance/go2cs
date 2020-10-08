// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modfetch -- go2cs converted at 2020 October 08 04:36:06 UTC
// import "cmd/go/internal/modfetch" ==> using modfetch = go.cmd.go.@internal.modfetch_package
// Original source: C:\Go\src\cmd\go\internal\modfetch\fetch.go
using zip = go.archive.zip_package;
using bytes = go.bytes_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using sort = go.sort_package;
using strings = go.strings_package;
using sync = go.sync_package;

using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using lockedfile = go.cmd.go.@internal.lockedfile_package;
using par = go.cmd.go.@internal.par_package;
using renameio = go.cmd.go.@internal.renameio_package;
using robustio = go.cmd.go.@internal.robustio_package;

using module = go.golang.org.x.mod.module_package;
using dirhash = go.golang.org.x.mod.sumdb.dirhash_package;
using modzip = go.golang.org.x.mod.zip_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class modfetch_package
    {
        private static par.Cache downloadCache = default;

        // Download downloads the specific module version to the
        // local download cache and returns the name of the directory
        // corresponding to the root of the module's file tree.
        public static (@string, error) Download(module.Version mod)
        {
            @string dir = default;
            error err = default!;

            if (cfg.GOMODCACHE == "")
            { 
                // modload.Init exits if GOPATH[0] is empty, and cfg.GOMODCACHE
                // is set to GOPATH[0]/pkg/mod if GOMODCACHE is empty, so this should never happen.
                @base.Fatalf("go: internal error: cfg.GOMODCACHE not set");

            } 

            // The par.Cache here avoids duplicate work.
            private partial struct cached
            {
                public @string dir;
                public error err;
            }
            cached c = downloadCache.Do(mod, () =>
            {
                var (dir, err) = download(mod);
                if (err != null)
                {
                    return new cached("",err);
                }

                checkMod(mod);
                return new cached(dir,nil);

            })._<cached>();
            return (c.dir, error.As(c.err)!);

        }

        private static (@string, error) download(module.Version mod) => func((defer, _, __) =>
        {
            @string dir = default;
            error err = default!;
 
            // If the directory exists, and no .partial file exists, the module has
            // already been completely extracted. .partial files may be created when a
            // module zip directory is extracted in place instead of being extracted to a
            // temporary directory and renamed.
            dir, err = DownloadDir(mod);
            if (err == null)
            {
                return (dir, error.As(null!)!);
            }
            else if (dir == "" || !errors.Is(err, os.ErrNotExist))
            {
                return ("", error.As(err)!);
            } 

            // To avoid cluttering the cache with extraneous files,
            // DownloadZip uses the same lockfile as Download.
            // Invoke DownloadZip before locking the file.
            var (zipfile, err) = DownloadZip(mod);
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            var (unlock, err) = lockVersion(mod);
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            defer(unlock()); 

            // Check whether the directory was populated while we were waiting on the lock.
            var (_, dirErr) = DownloadDir(mod);
            if (dirErr == null)
            {
                return (dir, error.As(null!)!);
            }

            ptr<DownloadDirPartialError> (_, dirExists) = dirErr._<ptr<DownloadDirPartialError>>(); 

            // Clean up any remaining temporary directories from previous runs, as well
            // as partially extracted diectories created by future versions of cmd/go.
            // This is only safe to do because the lock file ensures that their writers
            // are no longer active.
            var parentDir = filepath.Dir(dir);
            var tmpPrefix = filepath.Base(dir) + ".tmp-";
            {
                var err__prev1 = err;

                var (old, err) = filepath.Glob(filepath.Join(parentDir, tmpPrefix + "*"));

                if (err == null)
                {
                    foreach (var (_, path) in old)
                    {
                        RemoveAll(path); // best effort
                    }

                }

                err = err__prev1;

            }

            if (dirExists)
            {
                {
                    var err__prev2 = err;

                    var err = RemoveAll(dir);

                    if (err != null)
                    {
                        return ("", error.As(err)!);
                    }

                    err = err__prev2;

                }

            }

            var (partialPath, err) = CachePath(mod, "partial");
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            {
                var err__prev1 = err;

                err = os.Remove(partialPath);

                if (err != null && !os.IsNotExist(err))
                {
                    return ("", error.As(err)!);
                } 

                // Extract the module zip directory.
                //
                // By default, we extract to a temporary directory, then atomically rename to
                // its final location. We use the existence of the source directory to signal
                // that it has been extracted successfully (see DownloadDir).  If someone
                // deletes the entire directory (e.g., as an attempt to prune out file
                // corruption), the module cache will still be left in a recoverable
                // state.
                //
                // Unfortunately, os.Rename may fail with ERROR_ACCESS_DENIED on Windows if
                // another process opens files in the temporary directory. This is partially
                // mitigated by using robustio.Rename, which retries os.Rename for a short
                // time.
                //
                // To avoid this error completely, if unzipInPlace is set, we instead create a
                // .partial file (indicating the directory isn't fully extracted), then we
                // extract the directory at its final location, then we delete the .partial
                // file. This is not the default behavior because older versions of Go may
                // simply stat the directory to check whether it exists without looking for a
                // .partial file. If multiple versions run concurrently, the older version may
                // assume a partially extracted directory is complete.
                // TODO(golang.org/issue/36568): when these older versions are no longer
                // supported, remove the old default behavior and the unzipInPlace flag.

                err = err__prev1;

            } 

            // Extract the module zip directory.
            //
            // By default, we extract to a temporary directory, then atomically rename to
            // its final location. We use the existence of the source directory to signal
            // that it has been extracted successfully (see DownloadDir).  If someone
            // deletes the entire directory (e.g., as an attempt to prune out file
            // corruption), the module cache will still be left in a recoverable
            // state.
            //
            // Unfortunately, os.Rename may fail with ERROR_ACCESS_DENIED on Windows if
            // another process opens files in the temporary directory. This is partially
            // mitigated by using robustio.Rename, which retries os.Rename for a short
            // time.
            //
            // To avoid this error completely, if unzipInPlace is set, we instead create a
            // .partial file (indicating the directory isn't fully extracted), then we
            // extract the directory at its final location, then we delete the .partial
            // file. This is not the default behavior because older versions of Go may
            // simply stat the directory to check whether it exists without looking for a
            // .partial file. If multiple versions run concurrently, the older version may
            // assume a partially extracted directory is complete.
            // TODO(golang.org/issue/36568): when these older versions are no longer
            // supported, remove the old default behavior and the unzipInPlace flag.
            {
                var err__prev1 = err;

                err = os.MkdirAll(parentDir, 0777L);

                if (err != null)
                {
                    return ("", error.As(err)!);
                }

                err = err__prev1;

            }


            if (unzipInPlace)
            {
                {
                    var err__prev2 = err;

                    err = ioutil.WriteFile(partialPath, null, 0666L);

                    if (err != null)
                    {
                        return ("", error.As(err)!);
                    }

                    err = err__prev2;

                }

                {
                    var err__prev2 = err;

                    err = modzip.Unzip(dir, mod, zipfile);

                    if (err != null)
                    {
                        fmt.Fprintf(os.Stderr, "-> %s\n", err);
                        {
                            var rmErr = RemoveAll(dir);

                            if (rmErr == null)
                            {
                                os.Remove(partialPath);
                            }

                        }

                        return ("", error.As(err)!);

                    }

                    err = err__prev2;

                }

                {
                    var err__prev2 = err;

                    err = os.Remove(partialPath);

                    if (err != null)
                    {
                        return ("", error.As(err)!);
                    }

                    err = err__prev2;

                }

            }
            else
            {
                var (tmpDir, err) = ioutil.TempDir(parentDir, tmpPrefix);
                if (err != null)
                {
                    return ("", error.As(err)!);
                }

                {
                    var err__prev2 = err;

                    err = modzip.Unzip(tmpDir, mod, zipfile);

                    if (err != null)
                    {
                        fmt.Fprintf(os.Stderr, "-> %s\n", err);
                        RemoveAll(tmpDir);
                        return ("", error.As(err)!);
                    }

                    err = err__prev2;

                }

                {
                    var err__prev2 = err;

                    err = robustio.Rename(tmpDir, dir);

                    if (err != null)
                    {
                        RemoveAll(tmpDir);
                        return ("", error.As(err)!);
                    }

                    err = err__prev2;

                }

            }

            if (!cfg.ModCacheRW)
            { 
                // Make dir read-only only *after* renaming it.
                // os.Rename was observed to fail for read-only directories on macOS.
                makeDirsReadOnly(dir);

            }

            return (dir, error.As(null!)!);

        });

        private static bool unzipInPlace = default;

        private static void init()
        {
            foreach (var (_, f) in strings.Split(os.Getenv("GODEBUG"), ","))
            {
                if (f == "modcacheunzipinplace=1")
                {
                    unzipInPlace = true;
                    break;
                }

            }

        }

        private static par.Cache downloadZipCache = default;

        // DownloadZip downloads the specific module version to the
        // local zip cache and returns the name of the zip file.
        public static (@string, error) DownloadZip(module.Version mod) => func((defer, _, __) =>
        {
            @string zipfile = default;
            error err = default!;
 
            // The par.Cache here avoids duplicate work.
            private partial struct cached
            {
                public @string dir;
                public error err;
            }
            cached c = downloadZipCache.Do(mod, () =>
            {
                var (zipfile, err) = CachePath(mod, "zip");
                if (err != null)
                {
                    return new cached("",err);
                } 

                // Skip locking if the zipfile already exists.
                {
                    var err__prev1 = err;

                    var (_, err) = os.Stat(zipfile);

                    if (err == null)
                    {
                        return new cached(zipfile,nil);
                    } 

                    // The zip file does not exist. Acquire the lock and create it.

                    err = err__prev1;

                } 

                // The zip file does not exist. Acquire the lock and create it.
                if (cfg.CmdName != "mod download")
                {
                    fmt.Fprintf(os.Stderr, "go: downloading %s %s\n", mod.Path, mod.Version);
                }

                var (unlock, err) = lockVersion(mod);
                if (err != null)
                {
                    return new cached("",err);
                }

                defer(unlock()); 

                // Double-check that the zipfile was not created while we were waiting for
                // the lock.
                {
                    var err__prev1 = err;

                    (_, err) = os.Stat(zipfile);

                    if (err == null)
                    {
                        return new cached(zipfile,nil);
                    }

                    err = err__prev1;

                }

                {
                    var err__prev1 = err;

                    var err = os.MkdirAll(filepath.Dir(zipfile), 0777L);

                    if (err != null)
                    {
                        return new cached("",err);
                    }

                    err = err__prev1;

                }

                {
                    var err__prev1 = err;

                    err = downloadZip(mod, zipfile);

                    if (err != null)
                    {
                        return new cached("",err);
                    }

                    err = err__prev1;

                }

                return new cached(zipfile,nil);

            })._<cached>();
            return (c.zipfile, error.As(c.err)!);

        });

        private static error downloadZip(module.Version mod, @string zipfile) => func((defer, _, __) =>
        {
            error err = default!;
 
            // Clean up any remaining tempfiles from previous runs.
            // This is only safe to do because the lock file ensures that their
            // writers are no longer active.
            foreach (var (_, base) in new slice<@string>(new @string[] { zipfile, zipfile+"hash" }))
            {
                {
                    var err__prev1 = err;

                    var (old, err) = filepath.Glob(renameio.Pattern(base));

                    if (err == null)
                    {
                        foreach (var (_, path) in old)
                        {
                            os.Remove(path); // best effort
                        }

                    }

                    err = err__prev1;

                }

            } 

            // From here to the os.Rename call below is functionally almost equivalent to
            // renameio.WriteToFile, with one key difference: we want to validate the
            // contents of the file (by hashing it) before we commit it. Because the file
            // is zip-compressed, we need an actual file — or at least an io.ReaderAt — to
            // validate it: we can't just tee the stream as we write it.
            var (f, err) = ioutil.TempFile(filepath.Dir(zipfile), filepath.Base(renameio.Pattern(zipfile)));
            if (err != null)
            {
                return error.As(err)!;
            }

            defer(() =>
            {
                if (err != null)
                {
                    f.Close();
                    os.Remove(f.Name());
                }

            }());

            err = TryProxies(proxy =>
            {
                var (repo, err) = Lookup(proxy, mod.Path);
                if (err != null)
                {
                    return error.As(err)!;
                }

                return error.As(repo.Zip(f, mod.Version))!;

            });
            if (err != null)
            {
                return error.As(err)!;
            } 

            // Double-check that the paths within the zip file are well-formed.
            //
            // TODO(bcmills): There is a similar check within the Unzip function. Can we eliminate one?
            var (fi, err) = f.Stat();
            if (err != null)
            {
                return error.As(err)!;
            }

            var (z, err) = zip.NewReader(f, fi.Size());
            if (err != null)
            {
                return error.As(err)!;
            }

            var prefix = mod.Path + "@" + mod.Version + "/";
            {
                var f__prev1 = f;

                foreach (var (_, __f) in z.File)
                {
                    f = __f;
                    if (!strings.HasPrefix(f.Name, prefix))
                    {
                        return error.As(fmt.Errorf("zip for %s has unexpected file %s", prefix[..len(prefix) - 1L], f.Name))!;
                    }

                } 

                // Sync the file before renaming it: otherwise, after a crash the reader may
                // observe a 0-length file instead of the actual contents.
                // See https://golang.org/issue/22397#issuecomment-380831736.

                f = f__prev1;
            }

            {
                var err__prev1 = err;

                var err = f.Sync();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            {
                var err__prev1 = err;

                err = f.Close();

                if (err != null)
                {
                    return error.As(err)!;
                } 

                // Hash the zip file and check the sum before renaming to the final location.

                err = err__prev1;

            } 

            // Hash the zip file and check the sum before renaming to the final location.
            var (hash, err) = dirhash.HashZip(f.Name(), dirhash.DefaultHash);
            if (err != null)
            {
                return error.As(err)!;
            }

            {
                var err__prev1 = err;

                err = checkModSum(mod, hash);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }


            {
                var err__prev1 = err;

                err = renameio.WriteFile(zipfile + "hash", (slice<byte>)hash, 0666L);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            {
                var err__prev1 = err;

                err = os.Rename(f.Name(), zipfile);

                if (err != null)
                {
                    return error.As(err)!;
                } 

                // TODO(bcmills): Should we make the .zip and .ziphash files read-only to discourage tampering?

                err = err__prev1;

            } 

            // TODO(bcmills): Should we make the .zip and .ziphash files read-only to discourage tampering?

            return error.As(null!)!;

        });

        // makeDirsReadOnly makes a best-effort attempt to remove write permissions for dir
        // and its transitive contents.
        private static void makeDirsReadOnly(@string dir)
        {
            private partial struct pathMode
            {
                public @string path;
                public os.FileMode mode;
            }
            slice<pathMode> dirs = default; // in lexical order
            filepath.Walk(dir, (path, info, err) =>
            {
                if (err == null && info.Mode() & 0222L != 0L)
                {
                    if (info.IsDir())
                    {
                        dirs = append(dirs, new pathMode(path,info.Mode()));
                    }

                }

                return null;

            }); 

            // Run over list backward to chmod children before parents.
            for (var i = len(dirs) - 1L; i >= 0L; i--)
            {
                os.Chmod(dirs[i].path, dirs[i].mode & ~0222L);
            }


        }

        // RemoveAll removes a directory written by Download or Unzip, first applying
        // any permission changes needed to do so.
        public static error RemoveAll(@string dir)
        { 
            // Module cache has 0555 directories; make them writable in order to remove content.
            filepath.Walk(dir, (path, info, err) =>
            {
                if (err != null)
                {
                    return error.As(null!)!; // ignore errors walking in file system
                }

                if (info.IsDir())
                {
                    os.Chmod(path, 0777L);
                }

                return error.As(null!)!;

            });
            return error.As(robustio.RemoveAll(dir))!;

        }

        public static @string GoSumFile = default; // path to go.sum; set by package modload

        private partial struct modSum
        {
            public module.Version mod;
            public @string sum;
        }

        private static var goSum = default;

        // initGoSum initializes the go.sum data.
        // The boolean it returns reports whether the
        // use of go.sum is now enabled.
        // The goSum lock must be held.
        private static (bool, error) initGoSum()
        {
            bool _p0 = default;
            error _p0 = default!;

            if (GoSumFile == "")
            {
                return (false, error.As(null!)!);
            }

            if (goSum.m != null)
            {
                return (true, error.As(null!)!);
            }

            goSum.m = make_map<module.Version, slice<@string>>();
            goSum.@checked = make_map<modSum, bool>();
            var (data, err) = lockedfile.Read(GoSumFile);
            if (err != null && !os.IsNotExist(err))
            {
                return (false, error.As(err)!);
            }

            goSum.enabled = true;
            readGoSum(goSum.m, GoSumFile, data); 

            // Add old go.modverify file.
            // We'll delete go.modverify in WriteGoSum.
            var alt = strings.TrimSuffix(GoSumFile, ".sum") + ".modverify";
            {
                var data__prev1 = data;

                (data, err) = renameio.ReadFile(alt);

                if (err == null)
                {
                    var migrate = make_map<module.Version, slice<@string>>();
                    readGoSum(migrate, alt, data);
                    foreach (var (mod, sums) in migrate)
                    {
                        foreach (var (_, sum) in sums)
                        {
                            addModSumLocked(mod, sum);
                        }

                    }
                    goSum.modverify = alt;

                }

                data = data__prev1;

            }

            return (true, error.As(null!)!);

        }

        // emptyGoModHash is the hash of a 1-file tree containing a 0-length go.mod.
        // A bug caused us to write these into go.sum files for non-modules.
        // We detect and remove them.
        private static readonly @string emptyGoModHash = (@string)"h1:G7mAYYxgmS0lVkHyy2hEOLQCFB0DlQFTMLWggykrydY=";

        // readGoSum parses data, which is the content of file,
        // and adds it to goSum.m. The goSum lock must be held.


        // readGoSum parses data, which is the content of file,
        // and adds it to goSum.m. The goSum lock must be held.
        private static error readGoSum(map<module.Version, slice<@string>> dst, @string file, slice<byte> data)
        {
            long lineno = 0L;
            while (len(data) > 0L)
            {
                slice<byte> line = default;
                lineno++;
                var i = bytes.IndexByte(data, '\n');
                if (i < 0L)
                {
                    line = data;
                    data = null;

                }
                else
                {
                    line = data[..i];
                    data = data[i + 1L..];

                }

                var f = strings.Fields(string(line));
                if (len(f) == 0L)
                { 
                    // blank line; skip it
                    continue;

                }

                if (len(f) != 3L)
                {
                    return error.As(fmt.Errorf("malformed go.sum:\n%s:%d: wrong number of fields %v", file, lineno, len(f)))!;
                }

                if (f[2L] == emptyGoModHash)
                { 
                    // Old bug; drop it.
                    continue;

                }

                module.Version mod = new module.Version(Path:f[0],Version:f[1]);
                dst[mod] = append(dst[mod], f[2L]);

            }

            return error.As(null!)!;

        }

        // checkMod checks the given module's checksum.
        private static void checkMod(module.Version mod)
        {
            if (cfg.GOMODCACHE == "")
            { 
                // Do not use current directory.
                return ;

            } 

            // Do the file I/O before acquiring the go.sum lock.
            var (ziphash, err) = CachePath(mod, "ziphash");
            if (err != null)
            {
                @base.Fatalf("verifying %v", module.VersionError(mod, err));
            }

            var (data, err) = renameio.ReadFile(ziphash);
            if (err != null)
            {
                if (errors.Is(err, os.ErrNotExist))
                { 
                    // This can happen if someone does rm -rf GOPATH/src/cache/download. So it goes.
                    return ;

                }

                @base.Fatalf("verifying %v", module.VersionError(mod, err));

            }

            var h = strings.TrimSpace(string(data));
            if (!strings.HasPrefix(h, "h1:"))
            {
                @base.Fatalf("verifying %v", module.VersionError(mod, fmt.Errorf("unexpected ziphash: %q", h)));
            }

            {
                var err = checkModSum(mod, h);

                if (err != null)
                {
                    @base.Fatalf("%s", err);
                }

            }

        }

        // goModSum returns the checksum for the go.mod contents.
        private static (@string, error) goModSum(slice<byte> data)
        {
            @string _p0 = default;
            error _p0 = default!;

            return dirhash.Hash1(new slice<@string>(new @string[] { "go.mod" }), _p0 =>
            {
                return (ioutil.NopCloser(bytes.NewReader(data)), error.As(null!)!);
            });

        }

        // checkGoMod checks the given module's go.mod checksum;
        // data is the go.mod content.
        private static error checkGoMod(@string path, @string version, slice<byte> data)
        {
            var (h, err) = goModSum(data);
            if (err != null)
            {
                return error.As(addr(new module.ModuleError(Path:path,Version:version,Err:fmt.Errorf("verifying go.mod: %v",err)))!)!;
            }

            return error.As(checkModSum(new module.Version(Path:path,Version:version+"/go.mod"), h))!;

        }

        // checkModSum checks that the recorded checksum for mod is h.
        private static error checkModSum(module.Version mod, @string h)
        { 
            // We lock goSum when manipulating it,
            // but we arrange to release the lock when calling checkSumDB,
            // so that parallel calls to checkModHash can execute parallel calls
            // to checkSumDB.

            // Check whether mod+h is listed in go.sum already. If so, we're done.
            goSum.mu.Lock();
            var (inited, err) = initGoSum();
            if (err != null)
            {
                goSum.mu.Unlock();
                return error.As(err)!;
            }

            var done = inited && haveModSumLocked(mod, h);
            goSum.mu.Unlock();

            if (done)
            {
                return error.As(null!)!;
            } 

            // Not listed, so we want to add them.
            // Consult checksum database if appropriate.
            if (useSumDB(mod))
            { 
                // Calls base.Fatalf if mismatch detected.
                {
                    var err = checkSumDB(mod, h);

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                }

            } 

            // Add mod+h to go.sum, if it hasn't appeared already.
            if (inited)
            {
                goSum.mu.Lock();
                addModSumLocked(mod, h);
                goSum.mu.Unlock();
            }

            return error.As(null!)!;

        }

        // haveModSumLocked reports whether the pair mod,h is already listed in go.sum.
        // If it finds a conflicting pair instead, it calls base.Fatalf.
        // goSum.mu must be locked.
        private static bool haveModSumLocked(module.Version mod, @string h)
        {
            goSum.@checked[new modSum(mod,h)] = true;
            foreach (var (_, vh) in goSum.m[mod])
            {
                if (h == vh)
                {
                    return true;
                }

                if (strings.HasPrefix(vh, "h1:"))
                {
                    @base.Fatalf("verifying %s@%s: checksum mismatch\n\tdownloaded: %v\n\tgo.sum:     %v" + goSumMismatch, mod.Path, mod.Version, h, vh);
                }

            }
            return false;

        }

        // addModSumLocked adds the pair mod,h to go.sum.
        // goSum.mu must be locked.
        private static void addModSumLocked(module.Version mod, @string h)
        {
            if (haveModSumLocked(mod, h))
            {
                return ;
            }

            if (len(goSum.m[mod]) > 0L)
            {
                fmt.Fprintf(os.Stderr, "warning: verifying %s@%s: unknown hashes in go.sum: %v; adding %v" + hashVersionMismatch, mod.Path, mod.Version, strings.Join(goSum.m[mod], ", "), h);
            }

            goSum.m[mod] = append(goSum.m[mod], h);
            goSum.dirty = true;

        }

        // checkSumDB checks the mod, h pair against the Go checksum database.
        // It calls base.Fatalf if the hash is to be rejected.
        private static error checkSumDB(module.Version mod, @string h)
        {
            var (db, lines, err) = lookupSumDB(mod);
            if (err != null)
            {
                return error.As(module.VersionError(mod, fmt.Errorf("verifying module: %v", err)))!;
            }

            var have = mod.Path + " " + mod.Version + " " + h;
            var prefix = mod.Path + " " + mod.Version + " h1:";
            foreach (var (_, line) in lines)
            {
                if (line == have)
                {
                    return error.As(null!)!;
                }

                if (strings.HasPrefix(line, prefix))
                {
                    return error.As(module.VersionError(mod, fmt.Errorf("verifying module: checksum mismatch\n\tdownloaded: %v\n\t%s: %v" + sumdbMismatch, h, db, line[len(prefix) - len("h1:")..])))!;
                }

            }
            return error.As(null!)!;

        }

        // Sum returns the checksum for the downloaded copy of the given module,
        // if present in the download cache.
        public static @string Sum(module.Version mod)
        {
            if (cfg.GOMODCACHE == "")
            { 
                // Do not use current directory.
                return "";

            }

            var (ziphash, err) = CachePath(mod, "ziphash");
            if (err != null)
            {
                return "";
            }

            var (data, err) = renameio.ReadFile(ziphash);
            if (err != null)
            {
                return "";
            }

            return strings.TrimSpace(string(data));

        }

        // WriteGoSum writes the go.sum file if it needs to be updated.
        public static void WriteGoSum() => func((defer, _, __) =>
        {
            goSum.mu.Lock();
            defer(goSum.mu.Unlock());

            if (!goSum.enabled)
            { 
                // If we haven't read the go.sum file yet, don't bother writing it: at best,
                // we could rename the go.modverify file if it isn't empty, but we haven't
                // needed to touch it so far — how important could it be?
                return ;

            }

            if (!goSum.dirty)
            { 
                // Don't bother opening the go.sum file if we don't have anything to add.
                return ;

            }

            if (cfg.BuildMod == "readonly")
            {
                @base.Fatalf("go: updates to go.sum needed, disabled by -mod=readonly");
            } 

            // Make a best-effort attempt to acquire the side lock, only to exclude
            // previous versions of the 'go' command from making simultaneous edits.
            {
                var (unlock, err) = SideLock();

                if (err == null)
                {
                    defer(unlock());
                }

            }


            var err = lockedfile.Transform(GoSumFile, data =>
            {
                if (!goSum.overwrite)
                { 
                    // Incorporate any sums added by other processes in the meantime.
                    // Add only the sums that we actually checked: the user may have edited or
                    // truncated the file to remove erroneous hashes, and we shouldn't restore
                    // them without good reason.
                    goSum.m = make_map<module.Version, slice<@string>>(len(goSum.m));
                    readGoSum(goSum.m, GoSumFile, data);
                    foreach (var (ms) in goSum.@checked)
                    {
                        addModSumLocked(ms.mod, ms.sum);
                        goSum.dirty = true;
                    }

                }

                slice<module.Version> mods = default;
                {
                    var m__prev1 = m;

                    foreach (var (__m) in goSum.m)
                    {
                        m = __m;
                        mods = append(mods, m);
                    }

                    m = m__prev1;
                }

                module.Sort(mods);

                ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
                {
                    var m__prev1 = m;

                    foreach (var (_, __m) in mods)
                    {
                        m = __m;
                        var list = goSum.m[m];
                        sort.Strings(list);
                        foreach (var (_, h) in list)
                        {
                            fmt.Fprintf(_addr_buf, "%s %s %s\n", m.Path, m.Version, h);
                        }

                    }

                    m = m__prev1;
                }

                return (buf.Bytes(), null);

            });

            if (err != null)
            {
                @base.Fatalf("go: updating go.sum: %v", err);
            }

            goSum.@checked = make_map<modSum, bool>();
            goSum.dirty = false;
            goSum.overwrite = false;

            if (goSum.modverify != "")
            {
                os.Remove(goSum.modverify); // best effort
            }

        });

        // TrimGoSum trims go.sum to contain only the modules for which keep[m] is true.
        public static void TrimGoSum(map<module.Version, bool> keep) => func((defer, _, __) =>
        {
            goSum.mu.Lock();
            defer(goSum.mu.Unlock());
            var (inited, err) = initGoSum();
            if (err != null)
            {
                @base.Fatalf("%s", err);
            }

            if (!inited)
            {
                return ;
            }

            foreach (var (m) in goSum.m)
            { 
                // If we're keeping x@v we also keep x@v/go.mod.
                // Map x@v/go.mod back to x@v for the keep lookup.
                module.Version noGoMod = new module.Version(Path:m.Path,Version:strings.TrimSuffix(m.Version,"/go.mod"));
                if (!keep[m] && !keep[noGoMod])
                {
                    delete(goSum.m, m);
                    goSum.dirty = true;
                    goSum.overwrite = true;
                }

            }

        });

        private static readonly @string goSumMismatch = (@string)"\n\nSECURITY ERROR\nThis download does NOT match an earlier download recorded in go." +
    "sum.\nThe bits may have been replaced on the origin server, or an attacker may\nha" +
    "ve intercepted the download attempt.\n\nFor more information, see \'go help module-" +
    "auth\'.\n";



        private static readonly @string sumdbMismatch = (@string)"\n\nSECURITY ERROR\nThis download does NOT match the one reported by the checksum se" +
    "rver.\nThe bits may have been replaced on the origin server, or an attacker may\nh" +
    "ave intercepted the download attempt.\n\nFor more information, see \'go help module" +
    "-auth\'.\n";



        private static readonly @string hashVersionMismatch = (@string)"\n\nSECURITY WARNING\nThis download is listed in go.sum, but using an unknown hash a" +
    "lgorithm.\nThe download cannot be verified.\n\nFor more information, see \'go help m" +
    "odule-auth\'.\n\n";



        public static ptr<base.Command> HelpModuleAuth = addr(new base.Command(UsageLine:"module-auth",Short:"module authentication using go.sum",Long:`
The go command tries to authenticate every downloaded module,
checking that the bits downloaded for a specific module version today
match bits downloaded yesterday. This ensures repeatable builds
and detects introduction of unexpected changes, malicious or not.

In each module's root, alongside go.mod, the go command maintains
a file named go.sum containing the cryptographic checksums of the
module's dependencies.

The form of each line in go.sum is three fields:

	<module> <version>[/go.mod] <hash>

Each known module version results in two lines in the go.sum file.
The first line gives the hash of the module version's file tree.
The second line appends "/go.mod" to the version and gives the hash
of only the module version's (possibly synthesized) go.mod file.
The go.mod-only hash allows downloading and authenticating a
module version's go.mod file, which is needed to compute the
dependency graph, without also downloading all the module's source code.

The hash begins with an algorithm prefix of the form "h<N>:".
The only defined algorithm prefix is "h1:", which uses SHA-256.

Module authentication failures

The go command maintains a cache of downloaded packages and computes
and records the cryptographic checksum of each package at download time.
In normal operation, the go command checks the main module's go.sum file
against these precomputed checksums instead of recomputing them on
each command invocation. The 'go mod verify' command checks that
the cached copies of module downloads still match both their recorded
checksums and the entries in go.sum.

In day-to-day development, the checksum of a given module version
should never change. Each time a dependency is used by a given main
module, the go command checks its local cached copy, freshly
downloaded or not, against the main module's go.sum. If the checksums
don't match, the go command reports the mismatch as a security error
and refuses to run the build. When this happens, proceed with caution:
code changing unexpectedly means today's build will not match
yesterday's, and the unexpected change may not be beneficial.

If the go command reports a mismatch in go.sum, the downloaded code
for the reported module version does not match the one used in a
previous build of the main module. It is important at that point
to find out what the right checksum should be, to decide whether
go.sum is wrong or the downloaded code is wrong. Usually go.sum is right:
you want to use the same code you used yesterday.

If a downloaded module is not yet included in go.sum and it is a publicly
available module, the go command consults the Go checksum database to fetch
the expected go.sum lines. If the downloaded code does not match those
lines, the go command reports the mismatch and exits. Note that the
database is not consulted for module versions already listed in go.sum.

If a go.sum mismatch is reported, it is always worth investigating why
the code downloaded today differs from what was downloaded yesterday.

The GOSUMDB environment variable identifies the name of checksum database
to use and optionally its public key and URL, as in:

	GOSUMDB="sum.golang.org"
	GOSUMDB="sum.golang.org+<publickey>"
	GOSUMDB="sum.golang.org+<publickey> https://sum.golang.org"

The go command knows the public key of sum.golang.org, and also that the name
sum.golang.google.cn (available inside mainland China) connects to the
sum.golang.org checksum database; use of any other database requires giving
the public key explicitly.
The URL defaults to "https://" followed by the database name.

GOSUMDB defaults to "sum.golang.org", the Go checksum database run by Google.
See https://sum.golang.org/privacy for the service's privacy policy.

If GOSUMDB is set to "off", or if "go get" is invoked with the -insecure flag,
the checksum database is not consulted, and all unrecognized modules are
accepted, at the cost of giving up the security guarantee of verified repeatable
downloads for all modules. A better way to bypass the checksum database
for specific modules is to use the GOPRIVATE or GONOSUMDB environment
variables. See 'go help module-private' for details.

The 'go env -w' command (see 'go help env') can be used to set these variables
for future go command invocations.
`,));

        public static ptr<base.Command> HelpModulePrivate = addr(new base.Command(UsageLine:"module-private",Short:"module configuration for non-public modules",Long:`
The go command defaults to downloading modules from the public Go module
mirror at proxy.golang.org. It also defaults to validating downloaded modules,
regardless of source, against the public Go checksum database at sum.golang.org.
These defaults work well for publicly available source code.

The GOPRIVATE environment variable controls which modules the go command
considers to be private (not available publicly) and should therefore not use the
proxy or checksum database. The variable is a comma-separated list of
glob patterns (in the syntax of Go's path.Match) of module path prefixes.
For example,

	GOPRIVATE=*.corp.example.com,rsc.io/private

causes the go command to treat as private any module with a path prefix
matching either pattern, including git.corp.example.com/xyzzy, rsc.io/private,
and rsc.io/private/quux.

The GOPRIVATE environment variable may be used by other tools as well to
identify non-public modules. For example, an editor could use GOPRIVATE
to decide whether to hyperlink a package import to a godoc.org page.

For fine-grained control over module download and validation, the GONOPROXY
and GONOSUMDB environment variables accept the same kind of glob list
and override GOPRIVATE for the specific decision of whether to use the proxy
and checksum database, respectively.

For example, if a company ran a module proxy serving private modules,
users would configure go using:

	GOPRIVATE=*.corp.example.com
	GOPROXY=proxy.example.com
	GONOPROXY=none

This would tell the go command and other tools that modules beginning with
a corp.example.com subdomain are private but that the company proxy should
be used for downloading both public and private modules, because
GONOPROXY has been set to a pattern that won't match any modules,
overriding GOPRIVATE.

The 'go env -w' command (see 'go help env') can be used to set these variables
for future go command invocations.
`,));
    }
}}}}
