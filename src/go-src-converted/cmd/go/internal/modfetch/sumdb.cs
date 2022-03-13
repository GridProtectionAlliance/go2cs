// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Go checksum database lookup

//go:build !cmd_go_bootstrap
// +build !cmd_go_bootstrap

// package modfetch -- go2cs converted at 2022 March 13 06:32:20 UTC
// import "cmd/go/internal/modfetch" ==> using modfetch = go.cmd.go.@internal.modfetch_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modfetch\sumdb.go
namespace go.cmd.go.@internal;

using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using fs = io.fs_package;
using url = net.url_package;
using os = os_package;
using filepath = path.filepath_package;
using strings = strings_package;
using sync = sync_package;
using time = time_package;

using @base = cmd.go.@internal.@base_package;
using cfg = cmd.go.@internal.cfg_package;
using lockedfile = cmd.go.@internal.lockedfile_package;
using web = cmd.go.@internal.web_package;

using module = golang.org.x.mod.module_package;
using sumdb = golang.org.x.mod.sumdb_package;
using note = golang.org.x.mod.sumdb.note_package;


// useSumDB reports whether to use the Go checksum database for the given module.

using System;
public static partial class modfetch_package {

private static bool useSumDB(module.Version mod) {
    return cfg.GOSUMDB != "off" && !module.MatchPrefixPatterns(cfg.GONOSUMDB, mod.Path);
}

// lookupSumDB returns the Go checksum database's go.sum lines for the given module,
// along with the name of the database.
private static (@string, slice<@string>, error) lookupSumDB(module.Version mod) {
    @string dbname = default;
    slice<@string> lines = default;
    error err = default!;

    dbOnce.Do(() => {
        dbName, db, dbErr = dbDial();
    });
    if (dbErr != null) {
        return ("", null, error.As(dbErr)!);
    }
    lines, err = db.Lookup(mod.Path, mod.Version);
    return (dbName, lines, error.As(err)!);
}

private static sync.Once dbOnce = default;private static @string dbName = default;private static ptr<sumdb.Client> db;private static error dbErr = default!;

private static (@string, ptr<sumdb.Client>, error) dbDial() {
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
    if (gosumdb == "sum.golang.google.cn") {
        gosumdb = "sum.golang.org https://sum.golang.google.cn";
    }
    var key = strings.Fields(gosumdb);
    if (len(key) >= 1) {
        {
            var k = knownGOSUMDB[key[0]];

            if (k != "") {
                key[0] = k;
            }

        }
    }
    if (len(key) == 0) {
        return ("", _addr_null!, error.As(fmt.Errorf("missing GOSUMDB"))!);
    }
    if (len(key) > 2) {
        return ("", _addr_null!, error.As(fmt.Errorf("invalid GOSUMDB: too many fields"))!);
    }
    var (vkey, err) = note.NewVerifier(key[0]);
    if (err != null) {
        return ("", _addr_null!, error.As(fmt.Errorf("invalid GOSUMDB: %v", err))!);
    }
    var name = vkey.Name(); 

    // No funny business in the database name.
    var (direct, err) = url.Parse("https://" + name);
    if (err != null || strings.HasSuffix(name, "/") || direct != (new url.URL(Scheme:"https",Host:direct.Host,Path:direct.Path,RawPath:direct.RawPath)) || direct.RawPath != "" || direct.Host == "".val) {
        return ("", _addr_null!, error.As(fmt.Errorf("invalid sumdb name (must be host[/path]): %s %+v", name, direct.val))!);
    }
    ptr<url.URL> @base;
    if (len(key) >= 2) { 
        // Use explicit alternate URL listed in $GOSUMDB,
        // bypassing both the default URL derivation and any proxies.
        var (u, err) = url.Parse(key[1]);
        if (err != null) {
            return ("", _addr_null!, error.As(fmt.Errorf("invalid GOSUMDB URL: %v", err))!);
        }
        base = u;
    }
    return (name, _addr_sumdb.NewClient(addr(new dbClient(key:key[0],name:name,direct:direct,base:base)))!, error.As(null!)!);
}

private partial struct dbClient {
    public @string key;
    public @string name;
    public ptr<url.URL> direct;
    public sync.Once once;
    public ptr<url.URL> @base;
    public error baseErr;
}

private static (slice<byte>, error) ReadRemote(this ptr<dbClient> _addr_c, @string path) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref dbClient c = ref _addr_c.val;

    c.once.Do(c.initBase);
    if (c.baseErr != null) {
        return (null, error.As(c.baseErr)!);
    }
    slice<byte> data = default;
    var start = time.Now();
    var targ = web.Join(c.@base, path);
    var (data, err) = web.GetBytes(targ);
    if (false) {
        fmt.Fprintf(os.Stderr, "%.3fs %s\n", time.Since(start).Seconds(), targ.Redacted());
    }
    return (data, error.As(err)!);
}

// initBase determines the base URL for connecting to the database.
// Determining the URL requires sending network traffic to proxies,
// so this work is delayed until we need to download something from
// the database. If everything we need is in the local cache and
// c.ReadRemote is never called, we will never do this work.
private static void initBase(this ptr<dbClient> _addr_c) {
    ref dbClient c = ref _addr_c.val;

    if (c.@base != null) {
        return ;
    }
    var err = TryProxies(proxy => {
        switch (proxy) {
            case "noproxy": 
                return errUseProxy;
                break;
            case "direct": 

            case "off": 
                return errProxyOff;
                break;
            default: 
                var (proxyURL, err) = url.Parse(proxy);
                if (err != null) {
                    return err;
                }
                {
                    var (_, err) = web.GetBytes(web.Join(proxyURL, "sumdb/" + c.name + "/supported"));

                    if (err != null) {
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
    if (errors.Is(err, fs.ErrNotExist)) { 
        // No proxies, or all proxies failed (with 404, 410, or were allowed
        // to fall back), or we reached an explicit "direct" or "off".
        c.@base = c.direct;
    }
    else if (err != null) {
        c.baseErr = err;
    }
}

// ReadConfig reads the key from c.key
// and otherwise reads the config (a latest tree head) from GOPATH/pkg/sumdb/<file>.
private static (slice<byte>, error) ReadConfig(this ptr<dbClient> _addr_c, @string file) {
    slice<byte> data = default;
    error err = default!;
    ref dbClient c = ref _addr_c.val;

    if (file == "key") {
        return ((slice<byte>)c.key, error.As(null!)!);
    }
    if (cfg.SumdbDir == "") {
        return (null, error.As(errors.New("could not locate sumdb file: missing $GOPATH"))!);
    }
    var targ = filepath.Join(cfg.SumdbDir, file);
    data, err = lockedfile.Read(targ);
    if (errors.Is(err, fs.ErrNotExist)) { 
        // Treat non-existent as empty, to bootstrap the "latest" file
        // the first time we connect to a given database.
        return (new slice<byte>(new byte[] {  }), error.As(null!)!);
    }
    return (data, error.As(err)!);
}

// WriteConfig rewrites the latest tree head.
private static error WriteConfig(this ptr<dbClient> _addr__p0, @string file, slice<byte> old, slice<byte> @new) => func((defer, _, _) => {
    ref dbClient _p0 = ref _addr__p0.val;

    if (file == "key") { 
        // Should not happen.
        return error.As(fmt.Errorf("cannot write key"))!;
    }
    if (cfg.SumdbDir == "") {
        return error.As(errors.New("could not locate sumdb file: missing $GOPATH"))!;
    }
    var targ = filepath.Join(cfg.SumdbDir, file);
    os.MkdirAll(filepath.Dir(targ), 0777);
    var (f, err) = lockedfile.Edit(targ);
    if (err != null) {
        return error.As(err)!;
    }
    defer(f.Close());
    var (data, err) = io.ReadAll(f);
    if (err != null) {
        return error.As(err)!;
    }
    if (len(data) > 0 && !bytes.Equal(data, old)) {
        return error.As(sumdb.ErrWriteConflict)!;
    }
    {
        var (_, err) = f.Seek(0, 0);

        if (err != null) {
            return error.As(err)!;
        }
    }
    {
        var err = f.Truncate(0);

        if (err != null) {
            return error.As(err)!;
        }
    }
    {
        (_, err) = f.Write(new);

        if (err != null) {
            return error.As(err)!;
        }
    }
    return error.As(f.Close())!;
});

// ReadCache reads cached lookups or tiles from
// GOPATH/pkg/mod/cache/download/sumdb,
// which will be deleted by "go clean -modcache".
private static (slice<byte>, error) ReadCache(this ptr<dbClient> _addr__p0, @string file) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref dbClient _p0 = ref _addr__p0.val;

    var targ = filepath.Join(cfg.GOMODCACHE, "cache/download/sumdb", file);
    var (data, err) = lockedfile.Read(targ); 
    // lockedfile.Write does not atomically create the file with contents.
    // There is a moment between file creation and locking the file for writing,
    // during which the empty file can be locked for reading.
    // Treat observing an empty file as file not found.
    if (err == null && len(data) == 0) {
        err = addr(new fs.PathError(Op:"read",Path:targ,Err:fs.ErrNotExist));
    }
    return (data, error.As(err)!);
}

// WriteCache updates cached lookups or tiles.
private static void WriteCache(this ptr<dbClient> _addr__p0, @string file, slice<byte> data) {
    ref dbClient _p0 = ref _addr__p0.val;

    var targ = filepath.Join(cfg.GOMODCACHE, "cache/download/sumdb", file);
    os.MkdirAll(filepath.Dir(targ), 0777);
    lockedfile.Write(targ, bytes.NewReader(data), 0666);
}

private static void Log(this ptr<dbClient> _addr__p0, @string msg) {
    ref dbClient _p0 = ref _addr__p0.val;
 
    // nothing for now
}

private static void SecurityError(this ptr<dbClient> _addr__p0, @string msg) {
    ref dbClient _p0 = ref _addr__p0.val;

    @base.Fatalf("%s", msg);
}

} // end modfetch_package
