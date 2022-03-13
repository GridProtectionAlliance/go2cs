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

// package profile -- go2cs converted at 2022 March 13 05:38:48 UTC
// import "internal/profile" ==> using profile = go.@internal.profile_package
// Original source: C:\Program Files\Go\src\internal\profile\proto.go
namespace go.@internal;

using errors = errors_package;
using fmt = fmt_package;

public static partial class profile_package {

private partial struct buffer {
    public nint field;
    public nint typ;
    public ulong u64;
    public slice<byte> data;
    public array<byte> tmp;
}

public delegate  error decoder(ptr<buffer>,  message);

private partial interface message {
    slice<decoder> decoder();
    slice<decoder> encode(ptr<buffer> _p0);
}

private static slice<byte> marshal(message m) {
    ref buffer b = ref heap(out ptr<buffer> _addr_b);
    m.encode(_addr_b);
    return b.data;
}

private static void encodeVarint(ptr<buffer> _addr_b, ulong x) {
    ref buffer b = ref _addr_b.val;

    while (x >= 128) {
        b.data = append(b.data, byte(x) | 0x80);
        x>>=7;
    }
    b.data = append(b.data, byte(x));
}

private static void encodeLength(ptr<buffer> _addr_b, nint tag, nint len) {
    ref buffer b = ref _addr_b.val;

    encodeVarint(_addr_b, uint64(tag) << 3 | 2);
    encodeVarint(_addr_b, uint64(len));
}

private static void encodeUint64(ptr<buffer> _addr_b, nint tag, ulong x) {
    ref buffer b = ref _addr_b.val;
 
    // append varint to b.data
    encodeVarint(_addr_b, uint64(tag) << 3 | 0);
    encodeVarint(_addr_b, x);
}

private static void encodeUint64s(ptr<buffer> _addr_b, nint tag, slice<ulong> x) {
    ref buffer b = ref _addr_b.val;

    if (len(x) > 2) { 
        // Use packed encoding
        var n1 = len(b.data);
        {
            var u__prev1 = u;

            foreach (var (_, __u) in x) {
                u = __u;
                encodeVarint(_addr_b, u);
            }

            u = u__prev1;
        }

        var n2 = len(b.data);
        encodeLength(_addr_b, tag, n2 - n1);
        var n3 = len(b.data);
        copy(b.tmp[..], b.data[(int)n2..(int)n3]);
        copy(b.data[(int)n1 + (n3 - n2)..], b.data[(int)n1..(int)n2]);
        copy(b.data[(int)n1..], b.tmp[..(int)n3 - n2]);
        return ;
    }
    {
        var u__prev1 = u;

        foreach (var (_, __u) in x) {
            u = __u;
            encodeUint64(_addr_b, tag, u);
        }
        u = u__prev1;
    }
}

private static void encodeUint64Opt(ptr<buffer> _addr_b, nint tag, ulong x) {
    ref buffer b = ref _addr_b.val;

    if (x == 0) {
        return ;
    }
    encodeUint64(_addr_b, tag, x);
}

private static void encodeInt64(ptr<buffer> _addr_b, nint tag, long x) {
    ref buffer b = ref _addr_b.val;

    var u = uint64(x);
    encodeUint64(_addr_b, tag, u);
}

private static void encodeInt64Opt(ptr<buffer> _addr_b, nint tag, long x) {
    ref buffer b = ref _addr_b.val;

    if (x == 0) {
        return ;
    }
    encodeInt64(_addr_b, tag, x);
}

private static void encodeInt64s(ptr<buffer> _addr_b, nint tag, slice<long> x) {
    ref buffer b = ref _addr_b.val;

    if (len(x) > 2) { 
        // Use packed encoding
        var n1 = len(b.data);
        {
            var u__prev1 = u;

            foreach (var (_, __u) in x) {
                u = __u;
                encodeVarint(_addr_b, uint64(u));
            }

            u = u__prev1;
        }

        var n2 = len(b.data);
        encodeLength(_addr_b, tag, n2 - n1);
        var n3 = len(b.data);
        copy(b.tmp[..], b.data[(int)n2..(int)n3]);
        copy(b.data[(int)n1 + (n3 - n2)..], b.data[(int)n1..(int)n2]);
        copy(b.data[(int)n1..], b.tmp[..(int)n3 - n2]);
        return ;
    }
    {
        var u__prev1 = u;

        foreach (var (_, __u) in x) {
            u = __u;
            encodeInt64(_addr_b, tag, u);
        }
        u = u__prev1;
    }
}

private static void encodeString(ptr<buffer> _addr_b, nint tag, @string x) {
    ref buffer b = ref _addr_b.val;

    encodeLength(_addr_b, tag, len(x));
    b.data = append(b.data, x);
}

private static void encodeStrings(ptr<buffer> _addr_b, nint tag, slice<@string> x) {
    ref buffer b = ref _addr_b.val;

    foreach (var (_, s) in x) {
        encodeString(_addr_b, tag, s);
    }
}

private static void encodeStringOpt(ptr<buffer> _addr_b, nint tag, @string x) {
    ref buffer b = ref _addr_b.val;

    if (x == "") {
        return ;
    }
    encodeString(_addr_b, tag, x);
}

private static void encodeBool(ptr<buffer> _addr_b, nint tag, bool x) {
    ref buffer b = ref _addr_b.val;

    if (x) {
        encodeUint64(_addr_b, tag, 1);
    }
    else
 {
        encodeUint64(_addr_b, tag, 0);
    }
}

private static void encodeBoolOpt(ptr<buffer> _addr_b, nint tag, bool x) {
    ref buffer b = ref _addr_b.val;

    if (x == false) {
        return ;
    }
    encodeBool(_addr_b, tag, x);
}

private static void encodeMessage(ptr<buffer> _addr_b, nint tag, message m) {
    ref buffer b = ref _addr_b.val;

    var n1 = len(b.data);
    m.encode(b);
    var n2 = len(b.data);
    encodeLength(_addr_b, tag, n2 - n1);
    var n3 = len(b.data);
    copy(b.tmp[..], b.data[(int)n2..(int)n3]);
    copy(b.data[(int)n1 + (n3 - n2)..], b.data[(int)n1..(int)n2]);
    copy(b.data[(int)n1..], b.tmp[..(int)n3 - n2]);
}

private static error unmarshal(slice<byte> data, message m) {
    error err = default!;

    ref buffer b = ref heap(new buffer(data:data,typ:2), out ptr<buffer> _addr_b);
    return error.As(decodeMessage(_addr_b, m))!;
}

private static ulong le64(slice<byte> p) {
    return uint64(p[0]) | uint64(p[1]) << 8 | uint64(p[2]) << 16 | uint64(p[3]) << 24 | uint64(p[4]) << 32 | uint64(p[5]) << 40 | uint64(p[6]) << 48 | uint64(p[7]) << 56;
}

private static uint le32(slice<byte> p) {
    return uint32(p[0]) | uint32(p[1]) << 8 | uint32(p[2]) << 16 | uint32(p[3]) << 24;
}

private static (ulong, slice<byte>, error) decodeVarint(slice<byte> data) {
    ulong _p0 = default;
    slice<byte> _p0 = default;
    error _p0 = default!;

    nint i = default;
    ulong u = default;
    for (i = 0; ; i++) {
        if (i >= 10 || i >= len(data)) {
            return (0, null, error.As(errors.New("bad varint"))!);
        }
        u |= uint64(data[i] & 0x7F) << (int)(uint(7 * i));
        if (data[i] & 0x80 == 0) {
            return (u, data[(int)i + 1..], error.As(null!)!);
        }
    }
}

private static (slice<byte>, error) decodeField(ptr<buffer> _addr_b, slice<byte> data) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref buffer b = ref _addr_b.val;

    var (x, data, err) = decodeVarint(data);
    if (err != null) {
        return (null, error.As(err)!);
    }
    b.field = int(x >> 3);
    b.typ = int(x & 7);
    b.data = null;
    b.u64 = 0;
    switch (b.typ) {
        case 0: 
            b.u64, data, err = decodeVarint(data);
            if (err != null) {
                return (null, error.As(err)!);
            }
            break;
        case 1: 
            if (len(data) < 8) {
                return (null, error.As(errors.New("not enough data"))!);
            }
            b.u64 = le64(data[..(int)8]);
            data = data[(int)8..];
            break;
        case 2: 
            ulong n = default;
            n, data, err = decodeVarint(data);
            if (err != null) {
                return (null, error.As(err)!);
            }
            if (n > uint64(len(data))) {
                return (null, error.As(errors.New("too much data"))!);
            }
            b.data = data[..(int)n];
            data = data[(int)n..];
            break;
        case 5: 
            if (len(data) < 4) {
                return (null, error.As(errors.New("not enough data"))!);
            }
            b.u64 = uint64(le32(data[..(int)4]));
            data = data[(int)4..];
            break;
        default: 
            return (null, error.As(fmt.Errorf("unknown wire type: %d", b.typ))!);
            break;
    }

    return (data, error.As(null!)!);
}

private static error checkType(ptr<buffer> _addr_b, nint typ) {
    ref buffer b = ref _addr_b.val;

    if (b.typ != typ) {
        return error.As(errors.New("type mismatch"))!;
    }
    return error.As(null!)!;
}

private static error decodeMessage(ptr<buffer> _addr_b, message m) {
    ref buffer b = ref _addr_b.val;

    {
        var err__prev1 = err;

        var err = checkType(_addr_b, 2);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }
    var dec = m.decoder();
    var data = b.data;
    while (len(data) > 0) { 
        // pull varint field# + type
        err = default!;
        data, err = decodeField(_addr_b, data);
        if (err != null) {
            return error.As(err)!;
        }
        if (b.field >= len(dec) || dec[b.field] == null) {
            continue;
        }
        {
            var err__prev1 = err;

            err = dec[b.field](b, m);

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev1;

        }
    }
    return error.As(null!)!;
}

private static error decodeInt64(ptr<buffer> _addr_b, ptr<long> _addr_x) {
    ref buffer b = ref _addr_b.val;
    ref long x = ref _addr_x.val;

    {
        var err = checkType(_addr_b, 0);

        if (err != null) {
            return error.As(err)!;
        }
    }
    x = int64(b.u64);
    return error.As(null!)!;
}

private static error decodeInt64s(ptr<buffer> _addr_b, ptr<slice<long>> _addr_x) {
    ref buffer b = ref _addr_b.val;
    ref slice<long> x = ref _addr_x.val;

    if (b.typ == 2) { 
        // Packed encoding
        var data = b.data;
        while (len(data) > 0) {
            ulong u = default;
            error err = default!;

            u, data, err = decodeVarint(data);

            if (err != null) {
                return error.As(err)!;
            }
            x = append(x, int64(u));
        }
        return error.As(null!)!;
    }
    ref long i = ref heap(out ptr<long> _addr_i);
    {
        error err__prev1 = err;

        err = decodeInt64(_addr_b, _addr_i);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }
    x = append(x, i);
    return error.As(null!)!;
}

private static error decodeUint64(ptr<buffer> _addr_b, ptr<ulong> _addr_x) {
    ref buffer b = ref _addr_b.val;
    ref ulong x = ref _addr_x.val;

    {
        var err = checkType(_addr_b, 0);

        if (err != null) {
            return error.As(err)!;
        }
    }
    x = b.u64;
    return error.As(null!)!;
}

private static error decodeUint64s(ptr<buffer> _addr_b, ptr<slice<ulong>> _addr_x) {
    ref buffer b = ref _addr_b.val;
    ref slice<ulong> x = ref _addr_x.val;

    if (b.typ == 2) {
        var data = b.data; 
        // Packed encoding
        while (len(data) > 0) {
            ref ulong u = ref heap(out ptr<ulong> _addr_u);
            error err = default!;

            u, data, err = decodeVarint(data);

            if (err != null) {
                return error.As(err)!;
            }
            x = append(x, u);
        }
        return error.As(null!)!;
    }
    u = default;
    {
        error err__prev1 = err;

        err = decodeUint64(_addr_b, _addr_u);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }
    x = append(x, u);
    return error.As(null!)!;
}

private static error decodeString(ptr<buffer> _addr_b, ptr<@string> _addr_x) {
    ref buffer b = ref _addr_b.val;
    ref @string x = ref _addr_x.val;

    {
        var err = checkType(_addr_b, 2);

        if (err != null) {
            return error.As(err)!;
        }
    }
    x = string(b.data);
    return error.As(null!)!;
}

private static error decodeStrings(ptr<buffer> _addr_b, ptr<slice<@string>> _addr_x) {
    ref buffer b = ref _addr_b.val;
    ref slice<@string> x = ref _addr_x.val;

    ref @string s = ref heap(out ptr<@string> _addr_s);
    {
        var err = decodeString(_addr_b, _addr_s);

        if (err != null) {
            return error.As(err)!;
        }
    }
    x = append(x, s);
    return error.As(null!)!;
}

private static error decodeBool(ptr<buffer> _addr_b, ptr<bool> _addr_x) {
    ref buffer b = ref _addr_b.val;
    ref bool x = ref _addr_x.val;

    {
        var err = checkType(_addr_b, 0);

        if (err != null) {
            return error.As(err)!;
        }
    }
    if (int64(b.u64) == 0) {
        x = false;
    }
    else
 {
        x = true;
    }
    return error.As(null!)!;
}

} // end profile_package
