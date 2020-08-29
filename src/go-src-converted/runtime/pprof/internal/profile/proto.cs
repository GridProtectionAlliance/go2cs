// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file is a simple protocol buffer encoder and decoder.
//
// A protocol message must implement the message interface:
//   decoder() []decoder
//   encode(*buffer)
//
// The decode method returns a slice indexed by field number that gives the
// function to decode that field.
// The encode method encodes its receiver into the given buffer.
//
// The two methods are simple enough to be implemented by hand rather than
// by using a protocol compiler.
//
// See profile.go for examples of messages implementing this interface.
//
// There is no support for groups, message sets, or "has" bits.

// package profile -- go2cs converted at 2020 August 29 08:24:20 UTC
// import "runtime/pprof/internal/profile" ==> using profile = go.runtime.pprof.@internal.profile_package
// Original source: C:\Go\src\runtime\pprof\internal\profile\proto.go
using errors = go.errors_package;
using static go.builtin;

namespace go {
namespace runtime {
namespace pprof {
namespace @internal
{
    public static partial class profile_package
    {
        private partial struct buffer
        {
            public long field;
            public long typ;
            public ulong u64;
            public slice<byte> data;
            public array<byte> tmp;
        }

        public delegate  error decoder(ref buffer,  message);

        private partial interface message
        {
            slice<decoder> decoder();
            slice<decoder> encode(ref buffer _p0);
        }

        private static slice<byte> marshal(message m)
        {
            buffer b = default;
            m.encode(ref b);
            return b.data;
        }

        private static void encodeVarint(ref buffer b, ulong x)
        {
            while (x >= 128L)
            {
                b.data = append(b.data, byte(x) | 0x80UL);
                x >>= 7L;
            }

            b.data = append(b.data, byte(x));
        }

        private static void encodeLength(ref buffer b, long tag, long len)
        {
            encodeVarint(b, uint64(tag) << (int)(3L) | 2L);
            encodeVarint(b, uint64(len));
        }

        private static void encodeUint64(ref buffer b, long tag, ulong x)
        { 
            // append varint to b.data
            encodeVarint(b, uint64(tag) << (int)(3L) | 0L);
            encodeVarint(b, x);
        }

        private static void encodeUint64s(ref buffer b, long tag, slice<ulong> x)
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
                        encodeVarint(b, u);
                    }

                    u = u__prev1;
                }

                var n2 = len(b.data);
                encodeLength(b, tag, n2 - n1);
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
                    encodeUint64(b, tag, u);
                }

                u = u__prev1;
            }

        }

        private static void encodeUint64Opt(ref buffer b, long tag, ulong x)
        {
            if (x == 0L)
            {
                return;
            }
            encodeUint64(b, tag, x);
        }

        private static void encodeInt64(ref buffer b, long tag, long x)
        {
            var u = uint64(x);
            encodeUint64(b, tag, u);
        }

        private static void encodeInt64Opt(ref buffer b, long tag, long x)
        {
            if (x == 0L)
            {
                return;
            }
            encodeInt64(b, tag, x);
        }

        private static void encodeInt64s(ref buffer b, long tag, slice<long> x)
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
                        encodeVarint(b, uint64(u));
                    }

                    u = u__prev1;
                }

                var n2 = len(b.data);
                encodeLength(b, tag, n2 - n1);
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
                    encodeInt64(b, tag, u);
                }

                u = u__prev1;
            }

        }

        private static void encodeString(ref buffer b, long tag, @string x)
        {
            encodeLength(b, tag, len(x));
            b.data = append(b.data, x);
        }

        private static void encodeStrings(ref buffer b, long tag, slice<@string> x)
        {
            foreach (var (_, s) in x)
            {
                encodeString(b, tag, s);
            }
        }

        private static void encodeStringOpt(ref buffer b, long tag, @string x)
        {
            if (x == "")
            {
                return;
            }
            encodeString(b, tag, x);
        }

        private static void encodeBool(ref buffer b, long tag, bool x)
        {
            if (x)
            {
                encodeUint64(b, tag, 1L);
            }
            else
            {
                encodeUint64(b, tag, 0L);
            }
        }

        private static void encodeBoolOpt(ref buffer b, long tag, bool x)
        {
            if (x == false)
            {
                return;
            }
            encodeBool(b, tag, x);
        }

        private static void encodeMessage(ref buffer b, long tag, message m)
        {
            var n1 = len(b.data);
            m.encode(b);
            var n2 = len(b.data);
            encodeLength(b, tag, n2 - n1);
            var n3 = len(b.data);
            copy(b.tmp[..], b.data[n2..n3]);
            copy(b.data[n1 + (n3 - n2)..], b.data[n1..n2]);
            copy(b.data[n1..], b.tmp[..n3 - n2]);
        }

        private static error unmarshal(slice<byte> data, message m)
        {
            buffer b = new buffer(data:data,typ:2);
            return error.As(decodeMessage(ref b, m));
        }

        private static ulong le64(slice<byte> p)
        {
            return uint64(p[0L]) | uint64(p[1L]) << (int)(8L) | uint64(p[2L]) << (int)(16L) | uint64(p[3L]) << (int)(24L) | uint64(p[4L]) << (int)(32L) | uint64(p[5L]) << (int)(40L) | uint64(p[6L]) << (int)(48L) | uint64(p[7L]) << (int)(56L);
        }

        private static uint le32(slice<byte> p)
        {
            return uint32(p[0L]) | uint32(p[1L]) << (int)(8L) | uint32(p[2L]) << (int)(16L) | uint32(p[3L]) << (int)(24L);
        }

        private static (ulong, slice<byte>, error) decodeVarint(slice<byte> data)
        {
            long i = default;
            ulong u = default;
            for (i = 0L; >>MARKER:FOREXPRESSION_LEVEL_1<<; i++)
            {
                if (i >= 10L || i >= len(data))
                {
                    return (0L, null, errors.New("bad varint"));
                }
                u |= uint64(data[i] & 0x7FUL) << (int)(uint(7L * i));
                if (data[i] & 0x80UL == 0L)
                {
                    return (u, data[i + 1L..], null);
                }
            }

        }

        private static (slice<byte>, error) decodeField(ref buffer b, slice<byte> data)
        {
            var (x, data, err) = decodeVarint(data);
            if (err != null)
            {
                return (null, err);
            }
            b.field = int(x >> (int)(3L));
            b.typ = int(x & 7L);
            b.data = null;
            b.u64 = 0L;
            switch (b.typ)
            {
                case 0L: 
                    b.u64, data, err = decodeVarint(data);
                    if (err != null)
                    {
                        return (null, err);
                    }
                    break;
                case 1L: 
                    if (len(data) < 8L)
                    {
                        return (null, errors.New("not enough data"));
                    }
                    b.u64 = le64(data[..8L]);
                    data = data[8L..];
                    break;
                case 2L: 
                    ulong n = default;
                    n, data, err = decodeVarint(data);
                    if (err != null)
                    {
                        return (null, err);
                    }
                    if (n > uint64(len(data)))
                    {
                        return (null, errors.New("too much data"));
                    }
                    b.data = data[..n];
                    data = data[n..];
                    break;
                case 5L: 
                    if (len(data) < 4L)
                    {
                        return (null, errors.New("not enough data"));
                    }
                    b.u64 = uint64(le32(data[..4L]));
                    data = data[4L..];
                    break;
                default: 
                    return (null, errors.New("unknown type: " + string(b.typ)));
                    break;
            }

            return (data, null);
        }

        private static error checkType(ref buffer b, long typ)
        {
            if (b.typ != typ)
            {
                return error.As(errors.New("type mismatch"));
            }
            return error.As(null);
        }

        private static error decodeMessage(ref buffer b, message m)
        {
            {
                var err__prev1 = err;

                var err = checkType(b, 2L);

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }
            var dec = m.decoder();
            var data = b.data;
            while (len(data) > 0L)
            { 
                // pull varint field# + type
                err = default;
                data, err = decodeField(b, data);
                if (err != null)
                {
                    return error.As(err);
                }
                if (b.field >= len(dec) || dec[b.field] == null)
                {
                    continue;
                }
                {
                    var err__prev1 = err;

                    err = dec[b.field](b, m);

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev1;

                }
            }

            return error.As(null);
        }

        private static error decodeInt64(ref buffer b, ref long x)
        {
            {
                var err = checkType(b, 0L);

                if (err != null)
                {
                    return error.As(err);
                }

            }
            x.Value = int64(b.u64);
            return error.As(null);
        }

        private static error decodeInt64s(ref buffer b, ref slice<long> x)
        {
            if (b.typ == 2L)
            { 
                // Packed encoding
                var data = b.data;
                while (len(data) > 0L)
                {
                    ulong u = default;
                    error err = default;

                    u, data, err = decodeVarint(data);

                    if (err != null)
                    {
                        return error.As(err);
                    }
                    x.Value = append(x.Value, int64(u));
                }

                return error.As(null);
            }
            long i = default;
            {
                error err__prev1 = err;

                err = decodeInt64(b, ref i);

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }
            x.Value = append(x.Value, i);
            return error.As(null);
        }

        private static error decodeUint64(ref buffer b, ref ulong x)
        {
            {
                var err = checkType(b, 0L);

                if (err != null)
                {
                    return error.As(err);
                }

            }
            x.Value = b.u64;
            return error.As(null);
        }

        private static error decodeUint64s(ref buffer b, ref slice<ulong> x)
        {
            if (b.typ == 2L)
            {
                var data = b.data; 
                // Packed encoding
                while (len(data) > 0L)
                {
                    ulong u = default;
                    error err = default;

                    u, data, err = decodeVarint(data);

                    if (err != null)
                    {
                        return error.As(err);
                    }
                    x.Value = append(x.Value, u);
                }

                return error.As(null);
            }
            u = default;
            {
                error err__prev1 = err;

                err = decodeUint64(b, ref u);

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }
            x.Value = append(x.Value, u);
            return error.As(null);
        }

        private static error decodeString(ref buffer b, ref @string x)
        {
            {
                var err = checkType(b, 2L);

                if (err != null)
                {
                    return error.As(err);
                }

            }
            x.Value = string(b.data);
            return error.As(null);
        }

        private static error decodeStrings(ref buffer b, ref slice<@string> x)
        {
            @string s = default;
            {
                var err = decodeString(b, ref s);

                if (err != null)
                {
                    return error.As(err);
                }

            }
            x.Value = append(x.Value, s);
            return error.As(null);
        }

        private static error decodeBool(ref buffer b, ref bool x)
        {
            {
                var err = checkType(b, 0L);

                if (err != null)
                {
                    return error.As(err);
                }

            }
            if (int64(b.u64) == 0L)
            {
                x.Value = false;
            }
            else
            {
                x.Value = true;
            }
            return error.As(null);
        }
    }
}}}}
