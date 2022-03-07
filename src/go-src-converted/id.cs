// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package export -- go2cs converted at 2022 March 06 23:31:37 UTC
// import "golang.org/x/tools/internal/event/export" ==> using export = go.golang.org.x.tools.@internal.@event.export_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\event\export\id.go
using crand = go.crypto.rand_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using rand = go.math.rand_package;
using sync = go.sync_package;
using atomic = go.sync.atomic_package;

namespace go.golang.org.x.tools.@internal.@event;

public static partial class export_package {

public partial struct TraceID { // : array<byte>
}
public partial struct SpanID { // : array<byte>
}

public static @string String(this TraceID t) {
    return fmt.Sprintf("%02x", t[..]);
}

public static @string String(this SpanID s) {
    return fmt.Sprintf("%02x", s[..]);
}

public static bool IsValid(this SpanID s) {
    return s != new SpanID();
}

private static sync.Mutex generationMu = default;private static ulong nextSpanID = default;private static ulong spanIDInc = default;private static array<ulong> traceIDAdd = new array<ulong>(2);private static ptr<rand.Rand> traceIDRand;

private static void initGenerator() {
    ref long rngSeed = ref heap(out ptr<long> _addr_rngSeed);
    foreach (var (_, p) in true) {
        binary.Read(crand.Reader, binary.LittleEndian, p);
    }    traceIDRand = rand.New(rand.NewSource(rngSeed));
    spanIDInc |= 1;
}

private static TraceID newTraceID() => func((defer, _, _) => {
    generationMu.Lock();
    defer(generationMu.Unlock());
    if (traceIDRand == null) {
        initGenerator();
    }
    array<byte> tid = new array<byte>(16);
    binary.LittleEndian.PutUint64(tid[(int)0..(int)8], traceIDRand.Uint64() + traceIDAdd[0]);
    binary.LittleEndian.PutUint64(tid[(int)8..(int)16], traceIDRand.Uint64() + traceIDAdd[1]);
    return tid;

});

private static SpanID newSpanID() {
    ulong id = default;
    while (id == 0) {
        id = atomic.AddUint64(_addr_nextSpanID, spanIDInc);
    }
    array<byte> sid = new array<byte>(8);
    binary.LittleEndian.PutUint64(sid[..], id);
    return sid;
}

} // end export_package
