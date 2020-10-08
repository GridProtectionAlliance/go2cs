// Copyright 2014 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// This file is a simple protocol buffer encoder and decoder.
// The format is described at
// https://developers.google.com/protocol-buffers/docs/encoding
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

// package profile -- go2cs converted at 2020 October 08 04:43:41 UTC
// import "cmd/vendor/github.com/google/pprof/profile" ==> using profile = go.cmd.vendor.github.com.google.pprof.profile_package
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\profile\proto.go
using errors = go.errors_package;
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace github.com {
namespace google {
namespace pprof
{
    public static partial class profile_package
    {
        private partial struct buffer
        {
            public long field; // field tag
            public long typ; // proto wire type code for field
            public ulong u64;
            public slice<byte> data;
            public array<byte> tmp;
        }

        public delegate  error decoder(ptr<buffer>,  message);

        private partial interface message
        {
            slice<decoder> decoder();
            slice<decoder> encode(ptr<buffer> _p0);
        }

        private static slice<byte> marshal(message m)
        {
            ref buffer b = ref heap(out ptr<buffer> _addr_b);
            m.encode(_addr_b);
            return b.data;
        }

        private static void encodeVarint(ptr<buffer> _addr_b, ulong x)
        {
            ref buffer b = ref _addr_b.val;

            while (x >= 128L)
            {
                b.data = append(b.data, byte(x) | 0x80UL);
                x >>= 7L;
            }

            b.data = append(b.data, byte(x));

        }

        private static void encodeLength(ptr<buffer> _addr_b, long tag, long len)
        {
            ref buffer b = ref _addr_b.val;

            encodeVarint(_addr_b, uint64(tag) << (int)(3L) | 2L);
            encodeVarint(_addr_b, uint64(len));
        }

        private static void encodeUint64(ptr<buffer> _addr_b, long tag, ulong x)
        {
            ref buffer b = ref _addr_b.val;
 
            // append varint to b.data
            encodeVarint(_addr_b, uint64(tag) << (int)(3L));
            encodeVarint(_addr_b, x);

        }

        private static void encodeUint64s(ptr<buffer> _addr_b, long tag, slice<ulong> x)
        {
            ref buffer b = ref _addr_b.val;

            if (len(x) > 2L)
            { 
                // Use packed encoding
                var n1 = len(b.data);
                {
                    var u__prev1 = u;

                    foreach (var (_, __u) in x)
                    {
                        u = __u;
                        encodeVarint(_addr_b, u);
                    }

                    u = u__prev1;
                }

                var n2 = len(b.data);
                encodeLength(_addr_b, tag, n2 - n1);
                var n3 = len(b.data);
                copy(b.tmp[..], b.data[n2..n3]);
                copy(b.data[n1 + (n3 - n2)..], b.data[n1..n2]);
                copy(b.data[n1..], b.tmp[..n3 - n2]);
                return ;

            }

            {
                var u__prev1 = u;

                foreach (var (_, __u) in x)
                {
                    u = __u;
                    encodeUint64(_addr_b, tag, u);
                }

                u = u__prev1;
            }
        }

        private static void encodeUint64Opt(ptr<buffer> _addr_b, long tag, ulong x)
        {
            ref buffer b = ref _addr_b.val;

            if (x == 0L)
            {
                return ;
            }

            encodeUint64(_addr_b, tag, x);

        }

        private static void encodeInt64(ptr<buffer> _addr_b, long tag, long x)
        {
            ref buffer b = ref _addr_b.val;

            var u = uint64(x);
            encodeUint64(_addr_b, tag, u);
        }

        private static void encodeInt64s(ptr<buffer> _addr_b, long tag, slice<long> x)
        {
            ref buffer b = ref _addr_b.val;

            if (len(x) > 2L)
            { 
                // Use packed encoding
                var n1 = len(b.data);
                {
                    var u__prev1 = u;

                    foreach (var (_, __u) in x)
                    {
                        u = __u;
                        encodeVarint(_addr_b, uint64(u));
                    }

                    u = u__prev1;
                }

                var n2 = len(b.data);
                encodeLength(_addr_b, tag, n2 - n1);
                var n3 = len(b.data);
                copy(b.tmp[..], b.data[n2..n3]);
                copy(b.data[n1 + (n3 - n2)..], b.data[n1..n2]);
                copy(b.data[n1..], b.tmp[..n3 - n2]);
                return ;

            }

            {
                var u__prev1 = u;

                foreach (var (_, __u) in x)
                {
                    u = __u;
                    encodeInt64(_addr_b, tag, u);
                }

                u = u__prev1;
            }
        }

        private static void encodeInt64Opt(ptr<buffer> _addr_b, long tag, long x)
        {
            ref buffer b = ref _addr_b.val;

            if (x == 0L)
            {
                return ;
            }

            encodeInt64(_addr_b, tag, x);

        }

        private static void encodeString(ptr<buffer> _addr_b, long tag, @string x)
        {
            ref buffer b = ref _addr_b.val;

            encodeLength(_addr_b, tag, len(x));
            b.data = append(b.data, x);
        }

        private static void encodeStrings(ptr<buffer> _addr_b, long tag, slice<@string> x)
        {
            ref buffer b = ref _addr_b.val;

            foreach (var (_, s) in x)
            {
                encodeString(_addr_b, tag, s);
            }

        }

        private static void encodeBool(ptr<buffer> _addr_b, long tag, bool x)
        {
            ref buffer b = ref _addr_b.val;

            if (x)
            {
                encodeUint64(_addr_b, tag, 1L);
            }
            else
            {
                encodeUint64(_addr_b, tag, 0L);
            }

        }

        private static void encodeBoolOpt(ptr<buffer> _addr_b, long tag, bool x)
        {
            ref buffer b = ref _addr_b.val;

            if (x)
            {
                encodeBool(_addr_b, tag, x);
            }

        }

        private static void encodeMessage(ptr<buffer> _addr_b, long tag, message m)
        {
            ref buffer b = ref _addr_b.val;

            var n1 = len(b.data);
            m.encode(b);
            var n2 = len(b.data);
            encodeLength(_addr_b, tag, n2 - n1);
            var n3 = len(b.data);
            copy(b.tmp[..], b.data[n2..n3]);
            copy(b.data[n1 + (n3 - n2)..], b.data[n1..n2]);
            copy(b.data[n1..], b.tmp[..n3 - n2]);
        }

        private static error unmarshal(slice<byte> data, message m)
        {
            error err = default!;

            ref buffer b = ref heap(new buffer(data:data,typ:2), out ptr<buffer> _addr_b);
            return error.As(decodeMessage(_addr_b, m))!;
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
            ulong _p0 = default;
            slice<byte> _p0 = default;
            error _p0 = default!;

            ulong u = default;
            for (long i = 0L; >>MARKER:FOREXPRESSION_LEVEL_1<<; i++)
            {
                if (i >= 10L || i >= len(data))
                {
                    return (0L, null, error.As(errors.New("bad varint"))!);
                }

                u |= uint64(data[i] & 0x7FUL) << (int)(uint(7L * i));
                if (data[i] & 0x80UL == 0L)
                {
                    return (u, data[i + 1L..], error.As(null!)!);
                }

            }


        }

        private static (slice<byte>, error) decodeField(ptr<buffer> _addr_b, slice<byte> data)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref buffer b = ref _addr_b.val;

            var (x, data, err) = decodeVarint(data);
            if (err != null)
            {
                return (null, error.As(err)!);
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
                        return (null, error.As(err)!);
                    }

                    break;
                case 1L: 
                    if (len(data) < 8L)
                    {
                        return (null, error.As(errors.New("not enough data"))!);
                    }

                    b.u64 = le64(data[..8L]);
                    data = data[8L..];
                    break;
                case 2L: 
                    ulong n = default;
                    n, data, err = decodeVarint(data);
                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    }

                    if (n > uint64(len(data)))
                    {
                        return (null, error.As(errors.New("too much data"))!);
                    }

                    b.data = data[..n];
                    data = data[n..];
                    break;
                case 5L: 
                    if (len(data) < 4L)
                    {
                        return (null, error.As(errors.New("not enough data"))!);
                    }

                    b.u64 = uint64(le32(data[..4L]));
                    data = data[4L..];
                    break;
                default: 
                    return (null, error.As(fmt.Errorf("unknown wire type: %d", b.typ))!);
                    break;
            }

            return (data, error.As(null!)!);

        }

        private static error checkType(ptr<buffer> _addr_b, long typ)
        {
            ref buffer b = ref _addr_b.val;

            if (b.typ != typ)
            {
                return error.As(errors.New("type mismatch"))!;
            }

            return error.As(null!)!;

        }

        private static error decodeMessage(ptr<buffer> _addr_b, message m)
        {
            ref buffer b = ref _addr_b.val;

            {
                var err__prev1 = err;

                var err = checkType(_addr_b, 2L);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            var dec = m.decoder();
            var data = b.data;
            while (len(data) > 0L)
            { 
                // pull varint field# + type
                err = default!;
                data, err = decodeField(_addr_b, data);
                if (err != null)
                {
                    return error.As(err)!;
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
                        return error.As(err)!;
                    }

                    err = err__prev1;

                }

            }

            return error.As(null!)!;

        }

        private static error decodeInt64(ptr<buffer> _addr_b, ptr<long> _addr_x)
        {
            ref buffer b = ref _addr_b.val;
            ref long x = ref _addr_x.val;

            {
                var err = checkType(_addr_b, 0L);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            x = int64(b.u64);
            return error.As(null!)!;

        }

        private static error decodeInt64s(ptr<buffer> _addr_b, ptr<slice<long>> _addr_x)
        {
            ref buffer b = ref _addr_b.val;
            ref slice<long> x = ref _addr_x.val;

            if (b.typ == 2L)
            { 
                // Packed encoding
                var data = b.data;
                var tmp = make_slice<long>(0L, len(data)); // Maximally sized
                while (len(data) > 0L)
                {
                    ulong u = default;
                    error err = default!;

                    u, data, err = decodeVarint(data);

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    tmp = append(tmp, int64(u));

                }

                x = append(x, tmp);
                return error.As(null!)!;

            }

            ref long i = ref heap(out ptr<long> _addr_i);
            {
                error err__prev1 = err;

                err = decodeInt64(_addr_b, _addr_i);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            x = append(x, i);
            return error.As(null!)!;

        }

        private static error decodeUint64(ptr<buffer> _addr_b, ptr<ulong> _addr_x)
        {
            ref buffer b = ref _addr_b.val;
            ref ulong x = ref _addr_x.val;

            {
                var err = checkType(_addr_b, 0L);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            x = b.u64;
            return error.As(null!)!;

        }

        private static error decodeUint64s(ptr<buffer> _addr_b, ptr<slice<ulong>> _addr_x)
        {
            ref buffer b = ref _addr_b.val;
            ref slice<ulong> x = ref _addr_x.val;

            if (b.typ == 2L)
            {
                var data = b.data; 
                // Packed encoding
                var tmp = make_slice<ulong>(0L, len(data)); // Maximally sized
                while (len(data) > 0L)
                {
                    ref ulong u = ref heap(out ptr<ulong> _addr_u);
                    error err = default!;

                    u, data, err = decodeVarint(data);

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    tmp = append(tmp, u);

                }

                x = append(x, tmp);
                return error.As(null!)!;

            }

            u = default;
            {
                error err__prev1 = err;

                err = decodeUint64(_addr_b, _addr_u);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            x = append(x, u);
            return error.As(null!)!;

        }

        private static error decodeString(ptr<buffer> _addr_b, ptr<@string> _addr_x)
        {
            ref buffer b = ref _addr_b.val;
            ref @string x = ref _addr_x.val;

            {
                var err = checkType(_addr_b, 2L);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            x = string(b.data);
            return error.As(null!)!;

        }

        private static error decodeStrings(ptr<buffer> _addr_b, ptr<slice<@string>> _addr_x)
        {
            ref buffer b = ref _addr_b.val;
            ref slice<@string> x = ref _addr_x.val;

            ref @string s = ref heap(out ptr<@string> _addr_s);
            {
                var err = decodeString(_addr_b, _addr_s);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            x = append(x, s);
            return error.As(null!)!;

        }

        private static error decodeBool(ptr<buffer> _addr_b, ptr<bool> _addr_x)
        {
            ref buffer b = ref _addr_b.val;
            ref bool x = ref _addr_x.val;

            {
                var err = checkType(_addr_b, 0L);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            if (int64(b.u64) == 0L)
            {
                x = false;
            }
            else
            {
                x = true;
            }

            return error.As(null!)!;

        }
    }
}}}}}}
