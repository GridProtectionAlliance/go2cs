// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package sumdb implements the HTTP protocols for serving or accessing a module checksum database.
// package sumdb -- go2cs converted at 2020 October 08 04:36:22 UTC
// import "golang.org/x/mod/sumdb" ==> using sumdb = go.golang.org.x.mod.sumdb_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\mod\sumdb\server.go
using context = go.context_package;
using http = go.net.http_package;
using os = go.os_package;
using strings = go.strings_package;

using lazyregexp = go.golang.org.x.mod.@internal.lazyregexp_package;
using module = go.golang.org.x.mod.module_package;
using tlog = go.golang.org.x.mod.sumdb.tlog_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace mod
{
    public static partial class sumdb_package
    {
        // A ServerOps provides the external operations
        // (underlying database access and so on) needed by the Server.
        public partial interface ServerOps
        {
            (slice<byte>, error) Signed(context.Context ctx); // ReadRecords returns the content for the n records id through id+n-1.
            (slice<byte>, error) ReadRecords(context.Context ctx, long id, long n); // Lookup looks up a record for the given module,
// returning the record ID.
            (slice<byte>, error) Lookup(context.Context ctx, module.Version m); // ReadTileData reads the content of tile t.
// It is only invoked for hash tiles (t.L ≥ 0).
            (slice<byte>, error) ReadTileData(context.Context ctx, tlog.Tile t);
        }

        // A Server is the checksum database HTTP server,
        // which implements http.Handler and should be invoked
        // to serve the paths listed in ServerPaths.
        public partial struct Server
        {
            public ServerOps ops;
        }

        // NewServer returns a new Server using the given operations.
        public static ptr<Server> NewServer(ServerOps ops)
        {
            return addr(new Server(ops:ops));
        }

        // ServerPaths are the URL paths the Server can (and should) serve.
        //
        // Typically a server will do:
        //
        //    srv := sumdb.NewServer(ops)
        //    for _, path := range sumdb.ServerPaths {
        //        http.Handle(path, srv)
        //    }
        //
        public static @string ServerPaths = new slice<@string>(new @string[] { "/lookup/", "/latest", "/tile/" });

        private static var modVerRE = lazyregexp.New("^[^@]+@v[0-9]+\\.[0-9]+\\.[0-9]+(-[^@]*)?(\\+incompatible)?$");

        private static void ServeHTTP(this ptr<Server> _addr_s, http.ResponseWriter w, ptr<http.Request> _addr_r)
        {
            ref Server s = ref _addr_s.val;
            ref http.Request r = ref _addr_r.val;

            var ctx = r.Context();


            if (strings.HasPrefix(r.URL.Path, "/lookup/")) 
                var mod = strings.TrimPrefix(r.URL.Path, "/lookup/");
                if (!modVerRE.MatchString(mod))
                {
                    http.Error(w, "invalid module@version syntax", http.StatusBadRequest);
                    return ;
                }
                var i = strings.Index(mod, "@");
                var escPath = mod[..i];
                var escVers = mod[i + 1L..];
                var (path, err) = module.UnescapePath(escPath);
                if (err != null)
                {
                    reportError(w, err);
                    return ;
                }
                var (vers, err) = module.UnescapeVersion(escVers);
                if (err != null)
                {
                    reportError(w, err);
                    return ;
                }
                var (id, err) = s.ops.Lookup(ctx, new module.Version(Path:path,Version:vers));
                if (err != null)
                {
                    reportError(w, err);
                    return ;
                }
                var (records, err) = s.ops.ReadRecords(ctx, id, 1L);
                if (err != null)
                { 
                    // This should never happen - the lookup says the record exists.
                    http.Error(w, err.Error(), http.StatusInternalServerError);
                    return ;
                }
                if (len(records) != 1L)
                {
                    http.Error(w, "invalid record count returned by ReadRecords", http.StatusInternalServerError);
                    return ;
                }
                var (msg, err) = tlog.FormatRecord(id, records[0L]);
                if (err != null)
                {
                    http.Error(w, err.Error(), http.StatusInternalServerError);
                    return ;
                }
                var (signed, err) = s.ops.Signed(ctx);
                if (err != null)
                {
                    http.Error(w, err.Error(), http.StatusInternalServerError);
                    return ;
                }
                w.Header().Set("Content-Type", "text/plain; charset=UTF-8");
                w.Write(msg);
                w.Write(signed);
            else if (r.URL.Path == "/latest") 
                var (data, err) = s.ops.Signed(ctx);
                if (err != null)
                {
                    http.Error(w, err.Error(), http.StatusInternalServerError);
                    return ;
                }
                w.Header().Set("Content-Type", "text/plain; charset=UTF-8");
                w.Write(data);
            else if (strings.HasPrefix(r.URL.Path, "/tile/")) 
                var (t, err) = tlog.ParseTilePath(r.URL.Path[1L..]);
                if (err != null)
                {
                    http.Error(w, "invalid tile syntax", http.StatusBadRequest);
                    return ;
                }
                if (t.L == -1L)
                { 
                    // Record data.
                    var start = t.N << (int)(uint(t.H));
                    (records, err) = s.ops.ReadRecords(ctx, start, int64(t.W));
                    if (err != null)
                    {
                        reportError(w, err);
                        return ;
                    }
                    if (len(records) != t.W)
                    {
                        http.Error(w, "invalid record count returned by ReadRecords", http.StatusInternalServerError);
                        return ;
                    }
                    slice<byte> data = default;
                    {
                        var i__prev1 = i;

                        foreach (var (__i, __text) in records)
                        {
                            i = __i;
                            text = __text;
                            (msg, err) = tlog.FormatRecord(start + int64(i), text);
                            if (err != null)
                            {
                                http.Error(w, err.Error(), http.StatusInternalServerError);
                            }
                            data = append(data, msg);
                        }

                        i = i__prev1;
                    }

                    w.Header().Set("Content-Type", "text/plain; charset=UTF-8");
                    w.Write(data);
                    return ;
                }
                (data, err) = s.ops.ReadTileData(ctx, t);
                if (err != null)
                {
                    reportError(w, err);
                    return ;
                }
                w.Header().Set("Content-Type", "application/octet-stream");
                w.Write(data);
            else 
                http.NotFound(w, r);
                    }

        // reportError reports err to w.
        // If it's a not-found, the reported error is 404.
        // Otherwise it is an internal server error.
        // The caller must only call reportError in contexts where
        // a not-found err should be reported as 404.
        private static void reportError(http.ResponseWriter w, error err)
        {
            if (os.IsNotExist(err))
            {
                http.Error(w, err.Error(), http.StatusNotFound);
                return ;
            }
            http.Error(w, err.Error(), http.StatusInternalServerError);
        }
    }
}}}}
