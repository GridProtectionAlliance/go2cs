// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sumdb -- go2cs converted at 2022 March 06 23:19:09 UTC
// import "golang.org/x/mod/sumdb" ==> using sumdb = go.golang.org.x.mod.sumdb_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\mod\sumdb\test.go
using context = go.context_package;
using fmt = go.fmt_package;
using sync = go.sync_package;

using module = go.golang.org.x.mod.module_package;
using note = go.golang.org.x.mod.sumdb.note_package;
using tlog = go.golang.org.x.mod.sumdb.tlog_package;
using System;


namespace go.golang.org.x.mod;

public static partial class sumdb_package {

    // NewTestServer constructs a new TestServer
    // that will sign its tree with the given signer key
    // (see golang.org/x/mod/sumdb/note)
    // and fetch new records as needed by calling gosum.
public static ptr<TestServer> NewTestServer(@string signer, Func<@string, @string, (slice<byte>, error)> gosum) {
    return addr(new TestServer(signer:signer,gosum:gosum));
}

// A TestServer is an in-memory implementation of Server for testing.
public partial struct TestServer {
    public @string signer;
    public Func<@string, @string, (slice<byte>, error)> gosum;
    public sync.Mutex mu;
    public testHashes hashes;
    public slice<slice<byte>> records;
    public map<@string, long> lookup;
}

// testHashes implements tlog.HashReader, reading from a slice.
private partial struct testHashes { // : slice<tlog.Hash>
}

private static (slice<tlog.Hash>, error) ReadHashes(this testHashes h, slice<long> indexes) {
    slice<tlog.Hash> _p0 = default;
    error _p0 = default!;

    slice<tlog.Hash> list = default;
    foreach (var (_, id) in indexes) {
        list = append(list, h[id]);
    }    return (list, error.As(null!)!);
}

private static (slice<byte>, error) Signed(this ptr<TestServer> _addr_s, context.Context ctx) => func((defer, _, _) => {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref TestServer s = ref _addr_s.val;

    s.mu.Lock();
    defer(s.mu.Unlock());

    var size = int64(len(s.records));
    var (h, err) = tlog.TreeHash(size, s.hashes);
    if (err != null) {
        return (null, error.As(err)!);
    }
    var text = tlog.FormatTree(new tlog.Tree(N:size,Hash:h));
    var (signer, err) = note.NewSigner(s.signer);
    if (err != null) {
        return (null, error.As(err)!);
    }
    return note.Sign(addr(new note.Note(Text:string(text))), signer);
});

private static (slice<slice<byte>>, error) ReadRecords(this ptr<TestServer> _addr_s, context.Context ctx, long id, long n) => func((defer, _, _) => {
    slice<slice<byte>> _p0 = default;
    error _p0 = default!;
    ref TestServer s = ref _addr_s.val;

    s.mu.Lock();
    defer(s.mu.Unlock());

    slice<slice<byte>> list = default;
    for (var i = int64(0); i < n; i++) {
        if (id + i >= int64(len(s.records))) {
            return (null, error.As(fmt.Errorf("missing records"))!);
        }
        list = append(list, s.records[id + i]);
    }
    return (list, error.As(null!)!);
});

private static (long, error) Lookup(this ptr<TestServer> _addr_s, context.Context ctx, module.Version m) => func((defer, panic, _) => {
    long _p0 = default;
    error _p0 = default!;
    ref TestServer s = ref _addr_s.val;

    var key = m.String();
    s.mu.Lock();
    var (id, ok) = s.lookup[key];
    s.mu.Unlock();
    if (ok) {
        return (id, error.As(null!)!);
    }
    var (data, err) = s.gosum(m.Path, m.Version);
    if (err != null) {
        return (0, error.As(err)!);
    }
    s.mu.Lock();
    defer(s.mu.Unlock()); 

    // We ran the fetch without the lock.
    // If another fetch happened and committed, use it instead.
    id, ok = s.lookup[key];
    if (ok) {
        return (id, error.As(null!)!);
    }
    id = int64(len(s.records));
    s.records = append(s.records, data);
    if (s.lookup == null) {
        s.lookup = make_map<@string, long>();
    }
    s.lookup[key] = id;
    var (hashes, err) = tlog.StoredHashesForRecordHash(id, tlog.RecordHash((slice<byte>)data), s.hashes);
    if (err != null) {
        panic(err);
    }
    s.hashes = append(s.hashes, hashes);

    return (id, error.As(null!)!);
});

private static (slice<byte>, error) ReadTileData(this ptr<TestServer> _addr_s, context.Context ctx, tlog.Tile t) => func((defer, _, _) => {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref TestServer s = ref _addr_s.val;

    s.mu.Lock();
    defer(s.mu.Unlock());

    return tlog.ReadTileData(t, s.hashes);
});

} // end sumdb_package
