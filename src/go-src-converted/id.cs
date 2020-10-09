// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package export -- go2cs converted at 2020 October 09 06:01:44 UTC
// import "golang.org/x/tools/internal/event/export" ==> using export = go.golang.org.x.tools.@internal.@event.export_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\event\export\id.go
using crand = go.crypto.rand_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using rand = go.math.rand_package;
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace @internal {
namespace @event
{
    public static partial class export_package
    {
        public partial struct TraceID // : array<byte>
        {
        }
        public partial struct SpanID // : array<byte>
        {
        }

        public static @string String(this TraceID t)
        {
            return fmt.Sprintf("%02x", t[..]);
        }

        public static @string String(this SpanID s)
        {
            return fmt.Sprintf("%02x", s[..]);
        }

        public static bool IsValid(this SpanID s)
        {
            return s != new SpanID();
        }

        private static sync.Mutex generationMu = default;        private static ulong nextSpanID = default;        private static ulong spanIDInc = default;        private static array<ulong> traceIDAdd = new array<ulong>(2L);        private static ptr<rand.Rand> traceIDRand;

        private static void initGenerator()
        {
            ref long rngSeed = ref heap(out ptr<long> _addr_rngSeed);
            foreach (var (_, p) in true)
            {
                binary.Read(crand.Reader, binary.LittleEndian, p);
            }
            traceIDRand = rand.New(rand.NewSource(rngSeed));
            spanIDInc |= 1L;

        }

        private static TraceID newTraceID() => func((defer, _, __) =>
        {
            generationMu.Lock();
            defer(generationMu.Unlock());
            if (traceIDRand == null)
            {
                initGenerator();
            }

            array<byte> tid = new array<byte>(16L);
            binary.LittleEndian.PutUint64(tid[0L..8L], traceIDRand.Uint64() + traceIDAdd[0L]);
            binary.LittleEndian.PutUint64(tid[8L..16L], traceIDRand.Uint64() + traceIDAdd[1L]);
            return tid;

        });

        private static SpanID newSpanID()
        {
            ulong id = default;
            while (id == 0L)
            {
                id = atomic.AddUint64(_addr_nextSpanID, spanIDInc);
            }

            array<byte> sid = new array<byte>(8L);
            binary.LittleEndian.PutUint64(sid[..], id);
            return sid;

        }
    }
}}}}}}
