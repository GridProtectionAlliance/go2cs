// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package pprof -- go2cs converted at 2020 August 29 08:23:38 UTC
// import "runtime/pprof" ==> using pprof = go.runtime.pprof_package
// Original source: C:\Go\src\runtime\pprof\protobuf.go

using static go.builtin;

namespace go {
namespace runtime
{
    public static partial class pprof_package
    {
        // A protobuf is a simple protocol buffer encoder.
        private partial struct protobuf
        {
            public slice<byte> data;
            public array<byte> tmp;
            public long nest;
        }

        private static void varint(this ref protobuf b, ulong x)
        {
            while (x >= 128L)
            {
                b.data = append(b.data, byte(x) | 0x80UL);
                x >>= 7L;
            }

            b.data = append(b.data, byte(x));
        }

        private static void length(this ref protobuf b, long tag, long len)
        {
            b.varint(uint64(tag) << (int)(3L) | 2L);
            b.varint(uint64(len));
        }

        private static void uint64(this ref protobuf b, long tag, ulong x)
        { 
            // append varint to b.data
            b.varint(uint64(tag) << (int)(3L) | 0L);
            b.varint(x);
        }

        private static void uint64s(this ref protobuf b, long tag, slice<ulong> x)
        {
            if (len(x) > 2L)
            { 
                // Use packed encoding
                var n1 = len(b.data);
                {
                    var u__prev1 = u;

                    foreach (var (_, __u) in x)
                    {
                        u = __u;
                        b.varint(u);
                    }

                    u = u__prev1;
                }

                var n2 = len(b.data);
                b.length(tag, n2 - n1);
                var n3 = len(b.data);
                copy(b.tmp[..], b.data[n2..n3]);
                copy(b.data[n1 + (n3 - n2)..], b.data[n1..n2]);
                copy(b.data[n1..], b.tmp[..n3 - n2]);
                return;
            }
            {
                var u__prev1 = u;

                foreach (var (_, __u) in x)
                {
                    u = __u;
                    b.uint64(tag, u);
                }

                u = u__prev1;
            }

        }

        private static void uint64Opt(this ref protobuf b, long tag, ulong x)
        {
            if (x == 0L)
            {
                return;
            }
            b.uint64(tag, x);
        }

        private static void int64(this ref protobuf b, long tag, long x)
        {
            var u = uint64(x);
            b.uint64(tag, u);
        }

        private static void int64Opt(this ref protobuf b, long tag, long x)
        {
            if (x == 0L)
            {
                return;
            }
            b.int64(tag, x);
        }

        private static void int64s(this ref protobuf b, long tag, slice<long> x)
        {
            if (len(x) > 2L)
            { 
                // Use packed encoding
                var n1 = len(b.data);
                {
                    var u__prev1 = u;

                    foreach (var (_, __u) in x)
                    {
                        u = __u;
                        b.varint(uint64(u));
                    }

                    u = u__prev1;
                }

                var n2 = len(b.data);
                b.length(tag, n2 - n1);
                var n3 = len(b.data);
                copy(b.tmp[..], b.data[n2..n3]);
                copy(b.data[n1 + (n3 - n2)..], b.data[n1..n2]);
                copy(b.data[n1..], b.tmp[..n3 - n2]);
                return;
            }
            {
                var u__prev1 = u;

                foreach (var (_, __u) in x)
                {
                    u = __u;
                    b.int64(tag, u);
                }

                u = u__prev1;
            }

        }

        private static void @string(this ref protobuf b, long tag, @string x)
        {
            b.length(tag, len(x));
            b.data = append(b.data, x);
        }

        private static void strings(this ref protobuf b, long tag, slice<@string> x)
        {
            foreach (var (_, s) in x)
            {
                b.@string(tag, s);
            }
        }

        private static void stringOpt(this ref protobuf b, long tag, @string x)
        {
            if (x == "")
            {
                return;
            }
            b.@string(tag, x);
        }

        private static void @bool(this ref protobuf b, long tag, bool x)
        {
            if (x)
            {
                b.uint64(tag, 1L);
            }
            else
            {
                b.uint64(tag, 0L);
            }
        }

        private static void boolOpt(this ref protobuf b, long tag, bool x)
        {
            if (x == false)
            {
                return;
            }
            b.@bool(tag, x);
        }

        private partial struct msgOffset // : long
        {
        }

        private static msgOffset startMessage(this ref protobuf b)
        {
            b.nest++;
            return msgOffset(len(b.data));
        }

        private static void endMessage(this ref protobuf b, long tag, msgOffset start)
        {
            var n1 = int(start);
            var n2 = len(b.data);
            b.length(tag, n2 - n1);
            var n3 = len(b.data);
            copy(b.tmp[..], b.data[n2..n3]);
            copy(b.data[n1 + (n3 - n2)..], b.data[n1..n2]);
            copy(b.data[n1..], b.tmp[..n3 - n2]);
            b.nest--;
        }
    }
}}
