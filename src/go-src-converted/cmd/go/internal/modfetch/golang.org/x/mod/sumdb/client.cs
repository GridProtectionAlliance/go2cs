// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sumdb -- go2cs converted at 2020 October 08 04:36:18 UTC
// import "golang.org/x/mod/sumdb" ==> using sumdb = go.golang.org.x.mod.sumdb_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\mod\sumdb\client.go
using bytes = go.bytes_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using path = go.path_package;
using strings = go.strings_package;
using sync = go.sync_package;
using atomic = go.sync.atomic_package;

using module = go.golang.org.x.mod.module_package;
using note = go.golang.org.x.mod.sumdb.note_package;
using tlog = go.golang.org.x.mod.sumdb.tlog_package;
using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace golang.org {
namespace x {
namespace mod
{
    public static partial class sumdb_package
    {
        // A ClientOps provides the external operations
        // (file caching, HTTP fetches, and so on) needed by the Client.
        // The methods must be safe for concurrent use by multiple goroutines.
        public partial interface ClientOps
        {
            (slice<byte>, error) ReadRemote(@string path); // ReadConfig reads and returns the content of the named configuration file.
// There are only a fixed set of configuration files.
//
// "key" returns a file containing the verifier key for the server.
//
// serverName + "/latest" returns a file containing the latest known
// signed tree from the server.
// To signal that the client wishes to start with an "empty" signed tree,
// ReadConfig can return a successful empty result (0 bytes of data).
            (slice<byte>, error) ReadConfig(@string file); // WriteConfig updates the content of the named configuration file,
// changing it from the old []byte to the new []byte.
// If the old []byte does not match the stored configuration,
// WriteConfig must return ErrWriteConflict.
// Otherwise, WriteConfig should atomically replace old with new.
// The "key" configuration file is never written using WriteConfig.
            (slice<byte>, error) WriteConfig(@string file, slice<byte> old, slice<byte> @new); // ReadCache reads and returns the content of the named cache file.
// Any returned error will be treated as equivalent to the file not existing.
// There can be arbitrarily many cache files, such as:
//    serverName/lookup/pkg@version
//    serverName/tile/8/1/x123/456
            (slice<byte>, error) ReadCache(@string file); // WriteCache writes the named cache file.
            (slice<byte>, error) WriteCache(@string file, slice<byte> data); // Log prints the given log message (such as with log.Print)
            (slice<byte>, error) Log(@string msg); // SecurityError prints the given security error log message.
// The Client returns ErrSecurity from any operation that invokes SecurityError,
// but the return value is mainly for testing. In a real program,
// SecurityError should typically print the message and call log.Fatal or os.Exit.
            (slice<byte>, error) SecurityError(@string msg);
        }

        // ErrWriteConflict signals a write conflict during Client.WriteConfig.
        public static var ErrWriteConflict = errors.New("write conflict");

        // ErrSecurity is returned by Client operations that invoke Client.SecurityError.
        public static var ErrSecurity = errors.New("security error: misbehaving server");

        // A Client is a client connection to a checksum database.
        // All the methods are safe for simultaneous use by multiple goroutines.
        public partial struct Client
        {
            public ClientOps ops; // access to operations in the external world

            public uint didLookup; // one-time initialized data
            public sync.Once initOnce;
            public error initErr; // init error, if any
            public @string name; // name of accepted verifier
            public note.Verifiers verifiers; // accepted verifiers (just one, but Verifiers for note.Open)
            public tileReader tileReader;
            public long tileHeight;
            public @string nosumdb;
            public parCache record; // cache of record lookup, keyed by path@vers
            public parCache tileCache; // cache of c.readTile, keyed by tile

            public sync.Mutex latestMu;
            public tlog.Tree latest; // latest known tree head
            public slice<byte> latestMsg; // encoded signed note for latest

            public sync.Mutex tileSavedMu;
            public map<tlog.Tile, bool> tileSaved; // which tiles have been saved using c.ops.WriteCache already
        }

        // NewClient returns a new Client using the given Client.
        public static ptr<Client> NewClient(ClientOps ops)
        {
            return addr(new Client(ops:ops,));
        }

        // init initiailzes the client (if not already initialized)
        // and returns any initialization error.
        private static error init(this ptr<Client> _addr_c)
        {
            ref Client c = ref _addr_c.val;

            c.initOnce.Do(c.initWork);
            return error.As(c.initErr)!;
        }

        // initWork does the actual initialization work.
        private static void initWork(this ptr<Client> _addr_c) => func((defer, _, __) =>
        {
            ref Client c = ref _addr_c.val;

            defer(() =>
            {
                if (c.initErr != null)
                {
                    c.initErr = fmt.Errorf("initializing sumdb.Client: %v", c.initErr);
                }
            }());

            c.tileReader.c = c;
            if (c.tileHeight == 0L)
            {
                c.tileHeight = 8L;
            }
            c.tileSaved = make_map<tlog.Tile, bool>();

            var (vkey, err) = c.ops.ReadConfig("key");
            if (err != null)
            {
                c.initErr = err;
                return ;
            }
            var (verifier, err) = note.NewVerifier(strings.TrimSpace(string(vkey)));
            if (err != null)
            {
                c.initErr = err;
                return ;
            }
            c.verifiers = note.VerifierList(verifier);
            c.name = verifier.Name();

            var (data, err) = c.ops.ReadConfig(c.name + "/latest");
            if (err != null)
            {
                c.initErr = err;
                return ;
            }
            {
                var err = c.mergeLatest(data);

                if (err != null)
                {
                    c.initErr = err;
                    return ;
                }

            }
        });

        // SetTileHeight sets the tile height for the Client.
        // Any call to SetTileHeight must happen before the first call to Lookup.
        // If SetTileHeight is not called, the Client defaults to tile height 8.
        // SetTileHeight can be called at most once,
        // and if so it must be called before the first call to Lookup.
        private static void SetTileHeight(this ptr<Client> _addr_c, long height) => func((_, panic, __) =>
        {
            ref Client c = ref _addr_c.val;

            if (atomic.LoadUint32(_addr_c.didLookup) != 0L)
            {
                panic("SetTileHeight used after Lookup");
            }
            if (height <= 0L)
            {
                panic("invalid call to SetTileHeight");
            }
            if (c.tileHeight != 0L)
            {
                panic("multiple calls to SetTileHeight");
            }
            c.tileHeight = height;
        });

        // SetGONOSUMDB sets the list of comma-separated GONOSUMDB patterns for the Client.
        // For any module path matching one of the patterns,
        // Lookup will return ErrGONOSUMDB.
        // SetGONOSUMDB can be called at most once,
        // and if so it must be called before the first call to Lookup.
        private static void SetGONOSUMDB(this ptr<Client> _addr_c, @string list) => func((_, panic, __) =>
        {
            ref Client c = ref _addr_c.val;

            if (atomic.LoadUint32(_addr_c.didLookup) != 0L)
            {
                panic("SetGONOSUMDB used after Lookup");
            }
            if (c.nosumdb != "")
            {
                panic("multiple calls to SetGONOSUMDB");
            }
            c.nosumdb = list;
        });

        // ErrGONOSUMDB is returned by Lookup for paths that match
        // a pattern listed in the GONOSUMDB list (set by SetGONOSUMDB,
        // usually from the environment variable).
        public static var ErrGONOSUMDB = errors.New("skipped (listed in GONOSUMDB)");

        private static bool skip(this ptr<Client> _addr_c, @string target)
        {
            ref Client c = ref _addr_c.val;

            return globsMatchPath(c.nosumdb, target);
        }

        // globsMatchPath reports whether any path prefix of target
        // matches one of the glob patterns (as defined by path.Match)
        // in the comma-separated globs list.
        // It ignores any empty or malformed patterns in the list.
        private static bool globsMatchPath(@string globs, @string target)
        {
            while (globs != "")
            { 
                // Extract next non-empty glob in comma-separated list.
                @string glob = default;
                {
                    var i__prev1 = i;

                    var i = strings.Index(globs, ",");

                    if (i >= 0L)
                    {
                        glob = globs[..i];
                        globs = globs[i + 1L..];
                    }
                    else
                    {
                        glob = globs;
                        globs = "";
                    }

                    i = i__prev1;

                }
                if (glob == "")
                {
                    continue;
                } 

                // A glob with N+1 path elements (N slashes) needs to be matched
                // against the first N+1 path elements of target,
                // which end just before the N+1'th slash.
                var n = strings.Count(glob, "/");
                var prefix = target; 
                // Walk target, counting slashes, truncating at the N+1'th slash.
                {
                    var i__prev2 = i;

                    for (i = 0L; i < len(target); i++)
                    {
                        if (target[i] == '/')
                        {
                            if (n == 0L)
                            {
                                prefix = target[..i];
                                break;
                            }
                            n--;
                        }
                    }


                    i = i__prev2;
                }
                if (n > 0L)
                { 
                    // Not enough prefix elements.
                    continue;
                }
                var (matched, _) = path.Match(glob, prefix);
                if (matched)
                {
                    return true;
                }
            }

            return false;
        }

        // Lookup returns the go.sum lines for the given module path and version.
        // The version may end in a /go.mod suffix, in which case Lookup returns
        // the go.sum lines for the module's go.mod-only hash.
        private static (slice<@string>, error) Lookup(this ptr<Client> _addr_c, @string path, @string vers) => func((defer, _, __) =>
        {
            slice<@string> lines = default;
            error err = default!;
            ref Client c = ref _addr_c.val;

            atomic.StoreUint32(_addr_c.didLookup, 1L);

            if (c.skip(path))
            {
                return (null, error.As(ErrGONOSUMDB)!);
            }
            defer(() =>
            {
                if (err != null)
                {
                    err = fmt.Errorf("%s@%s: %v", path, vers, err);
                }
            }());

            {
                var err__prev1 = err;

                var err = c.init();

                if (err != null)
                {
                    return (null, error.As(err)!);
                } 

                // Prepare encoded cache filename / URL.

                err = err__prev1;

            } 

            // Prepare encoded cache filename / URL.
            var (epath, err) = module.EscapePath(path);
            if (err != null)
            {
                return (null, error.As(err)!);
            }
            var (evers, err) = module.EscapeVersion(strings.TrimSuffix(vers, "/go.mod"));
            if (err != null)
            {
                return (null, error.As(err)!);
            }
            @string remotePath = "/lookup/" + epath + "@" + evers;
            var file = c.name + remotePath; 

            // Fetch the data.
            // The lookupCache avoids redundant ReadCache/GetURL operations
            // (especially since go.sum lines tend to come in pairs for a given
            // path and version) and also avoids having multiple of the same
            // request in flight at once.
            private partial struct cached
            {
                public slice<byte> data;
                public error err;
            }
            cached result = c.record.Do(file, () =>
            { 
                // Try the on-disk cache, or else get from web.
                var writeCache = false;
                var (data, err) = c.ops.ReadCache(file);
                if (err != null)
                {
                    data, err = c.ops.ReadRemote(remotePath);
                    if (err != null)
                    {
                        return new cached(nil,err);
                    }
                    writeCache = true;
                } 

                // Validate the record before using it for anything.
                var (id, text, treeMsg, err) = tlog.ParseRecord(data);
                if (err != null)
                {
                    return new cached(nil,err);
                }
                {
                    var err__prev1 = err;

                    err = c.mergeLatest(treeMsg);

                    if (err != null)
                    {
                        return new cached(nil,err);
                    }

                    err = err__prev1;

                }
                {
                    var err__prev1 = err;

                    err = c.checkRecord(id, text);

                    if (err != null)
                    {
                        return new cached(nil,err);
                    } 

                    // Now that we've validated the record,
                    // save it to the on-disk cache (unless that's where it came from).

                    err = err__prev1;

                } 

                // Now that we've validated the record,
                // save it to the on-disk cache (unless that's where it came from).
                if (writeCache)
                {
                    c.ops.WriteCache(file, data);
                }
                return new cached(data,nil);
            })._<cached>();
            if (result.err != null)
            {
                return (null, error.As(result.err)!);
            } 

            // Extract the lines for the specific version we want
            // (with or without /go.mod).
            var prefix = path + " " + vers + " ";
            slice<@string> hashes = default;
            foreach (var (_, line) in strings.Split(string(result.data), "\n"))
            {
                if (strings.HasPrefix(line, prefix))
                {
                    hashes = append(hashes, line);
                }
            }
            return (hashes, error.As(null!)!);
        });

        // mergeLatest merges the tree head in msg
        // with the Client's current latest tree head,
        // ensuring the result is a consistent timeline.
        // If the result is inconsistent, mergeLatest calls c.ops.SecurityError
        // with a detailed security error message and then
        // (only if c.ops.SecurityError does not exit the program) returns ErrSecurity.
        // If the Client's current latest tree head moves forward,
        // mergeLatest updates the underlying configuration file as well,
        // taking care to merge any independent updates to that configuration.
        private static error mergeLatest(this ptr<Client> _addr_c, slice<byte> msg)
        {
            ref Client c = ref _addr_c.val;
 
            // Merge msg into our in-memory copy of the latest tree head.
            var (when, err) = c.mergeLatestMem(msg);
            if (err != null)
            {
                return error.As(err)!;
            }
            if (when != msgFuture)
            { 
                // msg matched our present or was in the past.
                // No change to our present, so no update of config file.
                return error.As(null!)!;
            } 

            // Flush our extended timeline back out to the configuration file.
            // If the configuration file has been updated in the interim,
            // we need to merge any updates made there as well.
            // Note that writeConfig is an atomic compare-and-swap.
            while (true)
            {
                var (msg, err) = c.ops.ReadConfig(c.name + "/latest");
                if (err != null)
                {
                    return error.As(err)!;
                }
                (when, err) = c.mergeLatestMem(msg);
                if (err != null)
                {
                    return error.As(err)!;
                }
                if (when != msgPast)
                { 
                    // msg matched our present or was from the future,
                    // and now our in-memory copy matches.
                    return error.As(null!)!;
                } 

                // msg (== config) is in the past, so we need to update it.
                c.latestMu.Lock();
                var latestMsg = c.latestMsg;
                c.latestMu.Unlock();
                {
                    var err = c.ops.WriteConfig(c.name + "/latest", msg, latestMsg);

                    if (err != ErrWriteConflict)
                    { 
                        // Success or a non-write-conflict error.
                        return error.As(err)!;
                    }

                }
            }
        }

        private static readonly long msgPast = (long)1L + iota;
        private static readonly var msgNow = (var)0;
        private static readonly var msgFuture = (var)1;

        // mergeLatestMem is like mergeLatest but is only concerned with
        // updating the in-memory copy of the latest tree head (c.latest)
        // not the configuration file.
        // The when result explains when msg happened relative to our
        // previous idea of c.latest:
        // msgPast means msg was from before c.latest,
        // msgNow means msg was exactly c.latest, and
        // msgFuture means msg was from after c.latest, which has now been updated.
        private static (long, error) mergeLatestMem(this ptr<Client> _addr_c, slice<byte> msg)
        {
            long when = default;
            error err = default!;
            ref Client c = ref _addr_c.val;

            if (len(msg) == 0L)
            { 
                // Accept empty msg as the unsigned, empty timeline.
                c.latestMu.Lock();
                var latest = c.latest;
                c.latestMu.Unlock();
                if (latest.N == 0L)
                {
                    return (msgNow, error.As(null!)!);
                }
                return (msgPast, error.As(null!)!);
            }
            var (note, err) = note.Open(msg, c.verifiers);
            if (err != null)
            {
                return (0L, error.As(fmt.Errorf("reading tree note: %v\nnote:\n%s", err, msg))!);
            }
            var (tree, err) = tlog.ParseTree((slice<byte>)note.Text);
            if (err != null)
            {
                return (0L, error.As(fmt.Errorf("reading tree: %v\ntree:\n%s", err, note.Text))!);
            } 

            // Other lookups may be calling mergeLatest with other heads,
            // so c.latest is changing underfoot. We don't want to hold the
            // c.mu lock during tile fetches, so loop trying to update c.latest.
            c.latestMu.Lock();
            latest = c.latest;
            var latestMsg = c.latestMsg;
            c.latestMu.Unlock();

            while (true)
            { 
                // If the tree head looks old, check that it is on our timeline.
                if (tree.N <= latest.N)
                {
                    {
                        var err__prev2 = err;

                        var err = c.checkTrees(tree, msg, latest, latestMsg);

                        if (err != null)
                        {
                            return (0L, error.As(err)!);
                        }

                        err = err__prev2;

                    }
                    if (tree.N < latest.N)
                    {
                        return (msgPast, error.As(null!)!);
                    }
                    return (msgNow, error.As(null!)!);
                } 

                // The tree head looks new. Check that we are on its timeline and try to move our timeline forward.
                {
                    var err__prev1 = err;

                    err = c.checkTrees(latest, latestMsg, tree, msg);

                    if (err != null)
                    {
                        return (0L, error.As(err)!);
                    } 

                    // Install our msg if possible.
                    // Otherwise we will go around again.

                    err = err__prev1;

                } 

                // Install our msg if possible.
                // Otherwise we will go around again.
                c.latestMu.Lock();
                var installed = false;
                if (c.latest == latest)
                {
                    installed = true;
                    c.latest = tree;
                    c.latestMsg = msg;
                }
                else
                {
                    latest = c.latest;
                    latestMsg = c.latestMsg;
                }
                c.latestMu.Unlock();

                if (installed)
                {
                    return (msgFuture, error.As(null!)!);
                }
            }
        }

        // checkTrees checks that older (from olderNote) is contained in newer (from newerNote).
        // If an error occurs, such as malformed data or a network problem, checkTrees returns that error.
        // If on the other hand checkTrees finds evidence of misbehavior, it prepares a detailed
        // message and calls log.Fatal.
        private static error checkTrees(this ptr<Client> _addr_c, tlog.Tree older, slice<byte> olderNote, tlog.Tree newer, slice<byte> newerNote)
        {
            ref Client c = ref _addr_c.val;

            var thr = tlog.TileHashReader(newer, _addr_c.tileReader);
            var (h, err) = tlog.TreeHash(older.N, thr);
            if (err != null)
            {
                if (older.N == newer.N)
                {
                    return error.As(fmt.Errorf("checking tree#%d: %v", older.N, err))!;
                }
                return error.As(fmt.Errorf("checking tree#%d against tree#%d: %v", older.N, newer.N, err))!;
            }
            if (h == older.Hash)
            {
                return error.As(null!)!;
            } 

            // Detected a fork in the tree timeline.
            // Start by reporting the inconsistent signed tree notes.
            ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
            fmt.Fprintf(_addr_buf, "SECURITY ERROR\n");
            fmt.Fprintf(_addr_buf, "go.sum database server misbehavior detected!\n\n");
            Func<slice<byte>, slice<byte>> indent = b => error.As(bytes.Replace(b, (slice<byte>)"\n", (slice<byte>)"\n\t", -1L))!;
            fmt.Fprintf(_addr_buf, "old database:\n\t%s\n", indent(olderNote));
            fmt.Fprintf(_addr_buf, "new database:\n\t%s\n", indent(newerNote)); 

            // The notes alone are not enough to prove the inconsistency.
            // We also need to show that the newer note's tree hash for older.N
            // does not match older.Hash. The consumer of this report could
            // of course consult the server to try to verify the inconsistency,
            // but we are holding all the bits we need to prove it right now,
            // so we might as well print them and make the report not depend
            // on the continued availability of the misbehaving server.
            // Preparing this data only reuses the tiled hashes needed for
            // tlog.TreeHash(older.N, thr) above, so assuming thr is caching tiles,
            // there are no new access to the server here, and these operations cannot fail.
            fmt.Fprintf(_addr_buf, "proof of misbehavior:\n\t%v", h);
            {
                var (p, err) = tlog.ProveTree(newer.N, older.N, thr);

                if (err != null)
                {
                    fmt.Fprintf(_addr_buf, "\tinternal error: %v\n", err);
                }                {
                    var err = tlog.CheckTree(p, newer.N, newer.Hash, older.N, h);


                    else if (err != null)
                    {
                        fmt.Fprintf(_addr_buf, "\tinternal error: generated inconsistent proof\n");
                    }
                    else
                    {
                        {
                            var h__prev1 = h;

                            foreach (var (_, __h) in p)
                            {
                                h = __h;
                                fmt.Fprintf(_addr_buf, "\n\t%v", h);
                            }

                            h = h__prev1;
                        }
                    }

                }

            }
            c.ops.SecurityError(buf.String());
            return error.As(ErrSecurity)!;
        }

        // checkRecord checks that record #id's hash matches data.
        private static error checkRecord(this ptr<Client> _addr_c, long id, slice<byte> data)
        {
            ref Client c = ref _addr_c.val;

            c.latestMu.Lock();
            var latest = c.latest;
            c.latestMu.Unlock();

            if (id >= latest.N)
            {
                return error.As(fmt.Errorf("cannot validate record %d in tree of size %d", id, latest.N))!;
            }
            var (hashes, err) = tlog.TileHashReader(latest, _addr_c.tileReader).ReadHashes(new slice<long>(new long[] { tlog.StoredHashIndex(0,id) }));
            if (err != null)
            {
                return error.As(err)!;
            }
            if (hashes[0L] == tlog.RecordHash(data))
            {
                return error.As(null!)!;
            }
            return error.As(fmt.Errorf("cannot authenticate record data in server response"))!;
        }

        // tileReader is a *Client wrapper that implements tlog.TileReader.
        // The separate type avoids exposing the ReadTiles and SaveTiles
        // methods on Client itself.
        private partial struct tileReader
        {
            public ptr<Client> c;
        }

        private static long Height(this ptr<tileReader> _addr_r)
        {
            ref tileReader r = ref _addr_r.val;

            return r.c.tileHeight;
        }

        // ReadTiles reads and returns the requested tiles,
        // either from the on-disk cache or the server.
        private static (slice<slice<byte>>, error) ReadTiles(this ptr<tileReader> _addr_r, slice<tlog.Tile> tiles) => func((defer, _, __) =>
        {
            slice<slice<byte>> _p0 = default;
            error _p0 = default!;
            ref tileReader r = ref _addr_r.val;
 
            // Read all the tiles in parallel.
            var data = make_slice<slice<byte>>(len(tiles));
            var errs = make_slice<error>(len(tiles));
            sync.WaitGroup wg = default;
            foreach (var (i, tile) in tiles)
            {
                wg.Add(1L);
                go_(() => (i, tile) =>
                {
                    defer(wg.Done());
                    data[i], errs[i] = r.c.readTile(tile);
                }(i, tile));
            }
            wg.Wait();

            foreach (var (_, err) in errs)
            {
                if (err != null)
                {
                    return (null, error.As(err)!);
                }
            }
            return (data, error.As(null!)!);
        });

        // tileCacheKey returns the cache key for the tile.
        private static @string tileCacheKey(this ptr<Client> _addr_c, tlog.Tile tile)
        {
            ref Client c = ref _addr_c.val;

            return c.name + "/" + tile.Path();
        }

        // tileRemotePath returns the remote path for the tile.
        private static @string tileRemotePath(this ptr<Client> _addr_c, tlog.Tile tile)
        {
            ref Client c = ref _addr_c.val;

            return "/" + tile.Path();
        }

        // readTile reads a single tile, either from the on-disk cache or the server.
        private static (slice<byte>, error) readTile(this ptr<Client> _addr_c, tlog.Tile tile)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref Client c = ref _addr_c.val;

            private partial struct cached
            {
                public slice<byte> data;
                public error err;
            }

            cached result = c.tileCache.Do(tile, () =>
            { 
                // Try the requested tile in on-disk cache.
                var (data, err) = c.ops.ReadCache(c.tileCacheKey(tile));
                if (err == null)
                {
                    c.markTileSaved(tile);
                    return new cached(data,nil);
                } 

                // Try the full tile in on-disk cache (if requested tile not already full).
                // We only save authenticated tiles to the on-disk cache,
                // so the recreated prefix is equally authenticated.
                var full = tile;
                full.W = 1L << (int)(uint(tile.H));
                if (tile != full)
                {
                    (data, err) = c.ops.ReadCache(c.tileCacheKey(full));
                    if (err == null)
                    {
                        c.markTileSaved(tile); // don't save tile later; we already have full
                        return new cached(data[:len(data)/full.W*tile.W],nil);
                    }
                } 

                // Try requested tile from server.
                data, err = c.ops.ReadRemote(c.tileRemotePath(tile));
                if (err == null)
                {
                    return new cached(data,nil);
                } 

                // Try full tile on server.
                // If the partial tile does not exist, it should be because
                // the tile has been completed and only the complete one
                // is available.
                if (tile != full)
                {
                    (data, err) = c.ops.ReadRemote(c.tileRemotePath(full));
                    if (err == null)
                    { 
                        // Note: We could save the full tile in the on-disk cache here,
                        // but we don't know if it is valid yet, and we will only find out
                        // about the partial data, not the full data. So let SaveTiles
                        // save the partial tile, and we'll just refetch the full tile later
                        // once we can validate more (or all) of it.
                        return new cached(data[:len(data)/full.W*tile.W],nil);
                    }
                } 

                // Nothing worked.
                // Return the error from the server fetch for the requested (not full) tile.
                return new cached(nil,err);
            })._<cached>();

            return (result.data, error.As(result.err)!);
        }

        // markTileSaved records that tile is already present in the on-disk cache,
        // so that a future SaveTiles for that tile can be ignored.
        private static void markTileSaved(this ptr<Client> _addr_c, tlog.Tile tile)
        {
            ref Client c = ref _addr_c.val;

            c.tileSavedMu.Lock();
            c.tileSaved[tile] = true;
            c.tileSavedMu.Unlock();
        }

        // SaveTiles saves the now validated tiles.
        private static void SaveTiles(this ptr<tileReader> _addr_r, slice<tlog.Tile> tiles, slice<slice<byte>> data)
        {
            ref tileReader r = ref _addr_r.val;

            var c = r.c; 

            // Determine which tiles need saving.
            // (Tiles that came from the cache need not be saved back.)
            var save = make_slice<bool>(len(tiles));
            c.tileSavedMu.Lock();
            {
                var i__prev1 = i;
                var tile__prev1 = tile;

                foreach (var (__i, __tile) in tiles)
                {
                    i = __i;
                    tile = __tile;
                    if (!c.tileSaved[tile])
                    {
                        save[i] = true;
                        c.tileSaved[tile] = true;
                    }
                }

                i = i__prev1;
                tile = tile__prev1;
            }

            c.tileSavedMu.Unlock();

            {
                var i__prev1 = i;
                var tile__prev1 = tile;

                foreach (var (__i, __tile) in tiles)
                {
                    i = __i;
                    tile = __tile;
                    if (save[i])
                    { 
                        // If WriteCache fails here (out of disk space? i/o error?),
                        // c.tileSaved[tile] is still true and we will not try to write it again.
                        // Next time we run maybe we'll redownload it again and be
                        // more successful.
                        c.ops.WriteCache(c.name + "/" + tile.Path(), data[i]);
                    }
                }

                i = i__prev1;
                tile = tile__prev1;
            }
        }
    }
}}}}
