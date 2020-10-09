// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Go checksum database lookup

// +build !cmd_go_bootstrap

// package modfetch -- go2cs converted at 2020 October 09 05:47:24 UTC
// import "cmd/go/internal/modfetch" ==> using modfetch = go.cmd.go.@internal.modfetch_package
// Original source: C:\Go\src\cmd\go\internal\modfetch\sumdb.go
using bytes = go.bytes_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using ioutil = go.io.ioutil_package;
using url = go.net.url_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;

using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using get = go.cmd.go.@internal.get_package;
using lockedfile = go.cmd.go.@internal.lockedfile_package;
using str = go.cmd.go.@internal.str_package;
using web = go.cmd.go.@internal.web_package;

using module = go.golang.org.x.mod.module_package;
using sumdb = go.golang.org.x.mod.sumdb_package;
using note = go.golang.org.x.mod.sumdb.note_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class modfetch_package
    {
        // useSumDB reports whether to use the Go checksum database for the given module.
        private static bool useSumDB(module.Version mod)
        {
            return cfg.GOSUMDB != "off" && !get.Insecure && !str.GlobsMatchPath(cfg.GONOSUMDB, mod.Path);
        }

        // lookupSumDB returns the Go checksum database's go.sum lines for the given module,
        // along with the name of the database.
        private static (@string, slice<@string>, error) lookupSumDB(module.Version mod)
        {
            @string dbname = default;
            slice<@string> lines = default;
            error err = default!;

            dbOnce.Do(() =>
            {
                dbName, db, dbErr = dbDial();
            });
            if (dbErr != null)
            {
                return ("", null, error.As(dbErr)!);
            }

            lines, err = db.Lookup(mod.Path, mod.Version);
            return (dbName, lines, error.As(err)!);

        }

        private static sync.Once dbOnce = default;        private static @string dbName = default;        private static ptr<sumdb.Client> db;        private static error dbErr = default!;

        private static (@string, ptr<sumdb.Client>, error) dbDial()
        {
            @string dbName = default;
            ptr<sumdb.Client> db = default!;
            error err = default!;
 
            // $GOSUMDB can be "key" or "key url",
            // and the key can be a full verifier key
            // or a host on our list of known keys.

            // Special case: sum.golang.google.cn
            // is an alias, reachable inside mainland China,
            // for sum.golang.org. If there are more
            // of these we should add a map like knownGOSUMDB.
            var gosumdb = cfg.GOSUMDB;
            if (gosumdb == "sum.golang.google.cn")
            {
                gosumdb = "sum.golang.org https://sum.golang.google.cn";
            }

            var key = strings.Fields(gosumdb);
            if (len(key) >= 1L)
            {
                {
                    var k = knownGOSUMDB[key[0L]];

                    if (k != "")
                    {
                        key[0L] = k;
                    }

                }

            }

            if (len(key) == 0L)
            {
                return ("", _addr_null!, error.As(fmt.Errorf("missing GOSUMDB"))!);
            }

            if (len(key) > 2L)
            {
                return ("", _addr_null!, error.As(fmt.Errorf("invalid GOSUMDB: too many fields"))!);
            }

            var (vkey, err) = note.NewVerifier(key[0L]);
            if (err != null)
            {
                return ("", _addr_null!, error.As(fmt.Errorf("invalid GOSUMDB: %v", err))!);
            }

            var name = vkey.Name(); 

            // No funny business in the database name.
            var (direct, err) = url.Parse("https://" + name);
            if (err != null || strings.HasSuffix(name, "/") || direct != (new url.URL(Scheme:"https",Host:direct.Host,Path:direct.Path,RawPath:direct.RawPath)) || direct.RawPath != "" || direct.Host == "".val)
            {
                return ("", _addr_null!, error.As(fmt.Errorf("invalid sumdb name (must be host[/path]): %s %+v", name, direct.val))!);
            } 

            // Determine how to get to database.
            ptr<url.URL> @base;
            if (len(key) >= 2L)
            { 
                // Use explicit alternate URL listed in $GOSUMDB,
                // bypassing both the default URL derivation and any proxies.
                var (u, err) = url.Parse(key[1L]);
                if (err != null)
                {
                    return ("", _addr_null!, error.As(fmt.Errorf("invalid GOSUMDB URL: %v", err))!);
                }

                base = u;

            }

            return (name, _addr_sumdb.NewClient(addr(new dbClient(key:key[0],name:name,direct:direct,base:base)))!, error.As(null!)!);

        }

        private partial struct dbClient
        {
            public @string key;
            public @string name;
            public ptr<url.URL> direct;
            public sync.Once once;
            public ptr<url.URL> @base;
            public error baseErr;
        }

        private static (slice<byte>, error) ReadRemote(this ptr<dbClient> _addr_c, @string path)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref dbClient c = ref _addr_c.val;

            c.once.Do(c.initBase);
            if (c.baseErr != null)
            {
                return (null, error.As(c.baseErr)!);
            }

            slice<byte> data = default;
            var start = time.Now();
            var targ = web.Join(c.@base, path);
            var (data, err) = web.GetBytes(targ);
            if (false)
            {
                fmt.Fprintf(os.Stderr, "%.3fs %s\n", time.Since(start).Seconds(), targ.Redacted());
            }

            return (data, error.As(err)!);

        }

        // initBase determines the base URL for connecting to the database.
        // Determining the URL requires sending network traffic to proxies,
        // so this work is delayed until we need to download something from
        // the database. If everything we need is in the local cache and
        // c.ReadRemote is never called, we will never do this work.
        private static void initBase(this ptr<dbClient> _addr_c)
        {
            ref dbClient c = ref _addr_c.val;

            if (c.@base != null)
            {
                return ;
            } 

            // Try proxies in turn until we find out how to connect to this database.
            //
            // Before accessing any checksum database URL using a proxy, the proxy
            // client should first fetch <proxyURL>/sumdb/<sumdb-name>/supported.
            //
            // If that request returns a successful (HTTP 200) response, then the proxy
            // supports proxying checksum database requests. In that case, the client
            // should use the proxied access method only, never falling back to a direct
            // connection to the database.
            //
            // If the /sumdb/<sumdb-name>/supported check fails with a “not found” (HTTP
            // 404) or “gone” (HTTP 410) response, or if the proxy is configured to fall
            // back on errors, the client will try the next proxy. If there are no
            // proxies left or if the proxy is "direct" or "off", the client should
            // connect directly to that database.
            //
            // Any other response is treated as the database being unavailable.
            //
            // See https://golang.org/design/25530-sumdb#proxying-a-checksum-database.
            var err = TryProxies(proxy =>
            {
                switch (proxy)
                {
                    case "noproxy": 
                        return errUseProxy;
                        break;
                    case "direct": 

                    case "off": 
                        return errProxyOff;
                        break;
                    default: 
                        var (proxyURL, err) = url.Parse(proxy);
                        if (err != null)
                        {
                            return err;
                        }

                        {
                            var (_, err) = web.GetBytes(web.Join(proxyURL, "sumdb/" + c.name + "/supported"));

                            if (err != null)
                            {
                                return err;
                            } 
                            // Success! This proxy will help us.

                        } 
                        // Success! This proxy will help us.
                        c.@base = web.Join(proxyURL, "sumdb/" + c.name);
                        return null;
                        break;
                }

            });
            if (errors.Is(err, os.ErrNotExist))
            { 
                // No proxies, or all proxies failed (with 404, 410, or were were allowed
                // to fall back), or we reached an explicit "direct" or "off".
                c.@base = c.direct;

            }
            else if (err != null)
            {
                c.baseErr = err;
            }

        }

        // ReadConfig reads the key from c.key
        // and otherwise reads the config (a latest tree head) from GOPATH/pkg/sumdb/<file>.
        private static (slice<byte>, error) ReadConfig(this ptr<dbClient> _addr_c, @string file)
        {
            slice<byte> data = default;
            error err = default!;
            ref dbClient c = ref _addr_c.val;

            if (file == "key")
            {
                return ((slice<byte>)c.key, error.As(null!)!);
            }

            if (cfg.SumdbDir == "")
            {
                return (null, error.As(errors.New("could not locate sumdb file: missing $GOPATH"))!);
            }

            var targ = filepath.Join(cfg.SumdbDir, file);
            data, err = lockedfile.Read(targ);
            if (errors.Is(err, os.ErrNotExist))
            { 
                // Treat non-existent as empty, to bootstrap the "latest" file
                // the first time we connect to a given database.
                return (new slice<byte>(new byte[] {  }), error.As(null!)!);

            }

            return (data, error.As(err)!);

        }

        // WriteConfig rewrites the latest tree head.
        private static error WriteConfig(this ptr<dbClient> _addr__p0, @string file, slice<byte> old, slice<byte> @new) => func((defer, _, __) =>
        {
            ref dbClient _p0 = ref _addr__p0.val;

            if (file == "key")
            { 
                // Should not happen.
                return error.As(fmt.Errorf("cannot write key"))!;

            }

            if (cfg.SumdbDir == "")
            {
                return error.As(errors.New("could not locate sumdb file: missing $GOPATH"))!;
            }

            var targ = filepath.Join(cfg.SumdbDir, file);
            os.MkdirAll(filepath.Dir(targ), 0777L);
            var (f, err) = lockedfile.Edit(targ);
            if (err != null)
            {
                return error.As(err)!;
            }

            defer(f.Close());
            var (data, err) = ioutil.ReadAll(f);
            if (err != null)
            {
                return error.As(err)!;
            }

            if (len(data) > 0L && !bytes.Equal(data, old))
            {
                return error.As(sumdb.ErrWriteConflict)!;
            }

            {
                var (_, err) = f.Seek(0L, 0L);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            {
                var err = f.Truncate(0L);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            {
                (_, err) = f.Write(new);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            return error.As(f.Close())!;

        });

        // ReadCache reads cached lookups or tiles from
        // GOPATH/pkg/mod/cache/download/sumdb,
        // which will be deleted by "go clean -modcache".
        private static (slice<byte>, error) ReadCache(this ptr<dbClient> _addr__p0, @string file)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref dbClient _p0 = ref _addr__p0.val;

            var targ = filepath.Join(cfg.GOMODCACHE, "cache/download/sumdb", file);
            var (data, err) = lockedfile.Read(targ); 
            // lockedfile.Write does not atomically create the file with contents.
            // There is a moment between file creation and locking the file for writing,
            // during which the empty file can be locked for reading.
            // Treat observing an empty file as file not found.
            if (err == null && len(data) == 0L)
            {
                err = addr(new os.PathError(Op:"read",Path:targ,Err:os.ErrNotExist));
            }

            return (data, error.As(err)!);

        }

        // WriteCache updates cached lookups or tiles.
        private static void WriteCache(this ptr<dbClient> _addr__p0, @string file, slice<byte> data)
        {
            ref dbClient _p0 = ref _addr__p0.val;

            var targ = filepath.Join(cfg.GOMODCACHE, "cache/download/sumdb", file);
            os.MkdirAll(filepath.Dir(targ), 0777L);
            lockedfile.Write(targ, bytes.NewReader(data), 0666L);
        }

        private static void Log(this ptr<dbClient> _addr__p0, @string msg)
        {
            ref dbClient _p0 = ref _addr__p0.val;
 
            // nothing for now
        }

        private static void SecurityError(this ptr<dbClient> _addr__p0, @string msg)
        {
            ref dbClient _p0 = ref _addr__p0.val;

            @base.Fatalf("%s", msg);
        }
    }
}}}}
