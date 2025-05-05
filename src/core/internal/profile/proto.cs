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
namespace go.@internal;

using errors = errors_package;
using fmt = fmt_package;

partial class profile_package {

[GoType] partial struct buffer {
    internal nint field;
    internal nint typ;
    internal uint64 u64;
    internal slice<byte> data;
    internal array<byte> tmp = new(16);
}

internal delegate error decoder(ж<buffer> _, message _);

[GoType] partial interface message {
    slice<Δdecoder> decoder();
    void encode(ж<buffer> _);
}

internal static slice<byte> marshal(message m) {
    ref var b = ref heap(new buffer(), out var Ꮡb);
    m.encode(Ꮡb);
    return b.data;
}

internal static void encodeVarint(ж<buffer> Ꮡb, uint64 x) {
    ref var b = ref Ꮡb.val;

    while (x >= 128) {
        b.data = append(b.data, (byte)(((byte)x) | 128));
        x >>= (UntypedInt)(7);
    }
    b.data = append(b.data, ((byte)x));
}

internal static void encodeLength(ж<buffer> Ꮡb, nint tag, nint len) {
    ref var b = ref Ꮡb.val;

    encodeVarint(Ꮡb, (uint64)(((uint64)tag) << (int)(3) | 2));
    encodeVarint(Ꮡb, ((uint64)len));
}

internal static void encodeUint64(ж<buffer> Ꮡb, nint tag, uint64 x) {
    ref var b = ref Ꮡb.val;

    // append varint to b.data
    encodeVarint(Ꮡb, (uint64)(((uint64)tag) << (int)(3) | 0));
    encodeVarint(Ꮡb, x);
}

internal static void encodeUint64s(ж<buffer> Ꮡb, nint tag, slice<uint64> x) {
    ref var b = ref Ꮡb.val;

    if (len(x) > 2) {
        // Use packed encoding
        nint n1 = len(b.data);
        foreach (var (_, u) in x) {
            encodeVarint(Ꮡb, u);
        }
        nint n2 = len(b.data);
        encodeLength(Ꮡb, tag, n2 - n1);
        nint n3 = len(b.data);
        copy(b.tmp[..], b.data[(int)(n2)..(int)(n3)]);
        copy(b.data[(int)(n1 + (n3 - n2))..], b.data[(int)(n1)..(int)(n2)]);
        copy(b.data[(int)(n1)..], b.tmp[..(int)(n3 - n2)]);
        return;
    }
    foreach (var (_, u) in x) {
        encodeUint64(Ꮡb, tag, u);
    }
}

internal static void encodeUint64Opt(ж<buffer> Ꮡb, nint tag, uint64 x) {
    ref var b = ref Ꮡb.val;

    if (x == 0) {
        return;
    }
    encodeUint64(Ꮡb, tag, x);
}

internal static void encodeInt64(ж<buffer> Ꮡb, nint tag, int64 x) {
    ref var b = ref Ꮡb.val;

    var u = ((uint64)x);
    encodeUint64(Ꮡb, tag, u);
}

internal static void encodeInt64Opt(ж<buffer> Ꮡb, nint tag, int64 x) {
    ref var b = ref Ꮡb.val;

    if (x == 0) {
        return;
    }
    encodeInt64(Ꮡb, tag, x);
}

internal static void encodeInt64s(ж<buffer> Ꮡb, nint tag, slice<int64> x) {
    ref var b = ref Ꮡb.val;

    if (len(x) > 2) {
        // Use packed encoding
        nint n1 = len(b.data);
        foreach (var (_, u) in x) {
            encodeVarint(Ꮡb, ((uint64)u));
        }
        nint n2 = len(b.data);
        encodeLength(Ꮡb, tag, n2 - n1);
        nint n3 = len(b.data);
        copy(b.tmp[..], b.data[(int)(n2)..(int)(n3)]);
        copy(b.data[(int)(n1 + (n3 - n2))..], b.data[(int)(n1)..(int)(n2)]);
        copy(b.data[(int)(n1)..], b.tmp[..(int)(n3 - n2)]);
        return;
    }
    foreach (var (_, u) in x) {
        encodeInt64(Ꮡb, tag, u);
    }
}

internal static void encodeString(ж<buffer> Ꮡb, nint tag, @string x) {
    ref var b = ref Ꮡb.val;

    encodeLength(Ꮡb, tag, len(x));
    b.data = append(b.data, x.ꓸꓸꓸ);
}

internal static void encodeStrings(ж<buffer> Ꮡb, nint tag, slice<@string> x) {
    ref var b = ref Ꮡb.val;

    foreach (var (_, s) in x) {
        encodeString(Ꮡb, tag, s);
    }
}

internal static void encodeBool(ж<buffer> Ꮡb, nint tag, bool x) {
    ref var b = ref Ꮡb.val;

    if (x){
        encodeUint64(Ꮡb, tag, 1);
    } else {
        encodeUint64(Ꮡb, tag, 0);
    }
}

internal static void encodeBoolOpt(ж<buffer> Ꮡb, nint tag, bool x) {
    ref var b = ref Ꮡb.val;

    if (!x) {
        return;
    }
    encodeBool(Ꮡb, tag, x);
}

internal static void encodeMessage(ж<buffer> Ꮡb, nint tag, message m) {
    ref var b = ref Ꮡb.val;

    nint n1 = len(b.data);
    m.encode(Ꮡb);
    nint n2 = len(b.data);
    encodeLength(Ꮡb, tag, n2 - n1);
    nint n3 = len(b.data);
    copy(b.tmp[..], b.data[(int)(n2)..(int)(n3)]);
    copy(b.data[(int)(n1 + (n3 - n2))..], b.data[(int)(n1)..(int)(n2)]);
    copy(b.data[(int)(n1)..], b.tmp[..(int)(n3 - n2)]);
}

internal static error /*err*/ unmarshal(slice<byte> data, message m) {
    error err = default!;

    ref var b = ref heap<buffer>(out var Ꮡb);
    b = new buffer(data: data, typ: 2);
    return decodeMessage(Ꮡb, m);
}

internal static uint64 le64(slice<byte> p) {
    return (uint64)((uint64)((uint64)((uint64)((uint64)((uint64)((uint64)(((uint64)p[0]) | ((uint64)p[1]) << (int)(8)) | ((uint64)p[2]) << (int)(16)) | ((uint64)p[3]) << (int)(24)) | ((uint64)p[4]) << (int)(32)) | ((uint64)p[5]) << (int)(40)) | ((uint64)p[6]) << (int)(48)) | ((uint64)p[7]) << (int)(56));
}

internal static uint32 le32(slice<byte> p) {
    return (uint32)((uint32)((uint32)(((uint32)p[0]) | ((uint32)p[1]) << (int)(8)) | ((uint32)p[2]) << (int)(16)) | ((uint32)p[3]) << (int)(24));
}

internal static (uint64, slice<byte>, error) decodeVarint(slice<byte> data) {
    nint i = default!;
    uint64 u = default!;
    for (i = 0; ᐧ ; i++) {
        if (i >= 10 || i >= len(data)) {
            return (0, default!, errors.New("bad varint"u8));
        }
        u |= (uint64)(((uint64)((byte)(data[i] & 127))) << (int)(((nuint)(7 * i))));
        if ((byte)(data[i] & 128) == 0) {
            return (u, data[(int)(i + 1)..], default!);
        }
    }
}

internal static (slice<byte>, error) decodeField(ж<buffer> Ꮡb, slice<byte> data) {
    ref var b = ref Ꮡb.val;

    var (x, data, err) = decodeVarint(data);
    if (err != default!) {
        return (default!, err);
    }
    b.field = ((nint)(x >> (int)(3)));
    b.typ = ((nint)((uint64)(x & 7)));
    b.data = default!;
    b.u64 = 0;
    switch (b.typ) {
    case 0: {
        (b.u64, data, err) = decodeVarint(data);
        if (err != default!) {
            return (default!, err);
        }
        break;
    }
    case 1: {
        if (len(data) < 8) {
            return (default!, errors.New("not enough data"u8));
        }
        b.u64 = le64(data[..8]);
        data = data[8..];
        break;
    }
    case 2: {
        uint64 n = default!;
        (n, data, err) = decodeVarint(data);
        if (err != default!) {
            return (default!, err);
        }
        if (n > ((uint64)len(data))) {
            return (default!, errors.New("too much data"u8));
        }
        b.data = data[..(int)(n)];
        data = data[(int)(n)..];
        break;
    }
    case 5: {
        if (len(data) < 4) {
            return (default!, errors.New("not enough data"u8));
        }
        b.u64 = ((uint64)le32(data[..4]));
        data = data[4..];
        break;
    }
    default: {
        return (default!, fmt.Errorf("unknown wire type: %d"u8, b.typ));
    }}

    return (data, default!);
}

internal static error checkType(ж<buffer> Ꮡb, nint typ) {
    ref var b = ref Ꮡb.val;

    if (b.typ != typ) {
        return errors.New("type mismatch"u8);
    }
    return default!;
}

internal static error decodeMessage(ж<buffer> Ꮡb, message m) {
    ref var b = ref Ꮡb.val;

    {
        var errΔ1 = checkType(Ꮡb, 2); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    var dec = m.decoder();
    var data = b.data;
    while (len(data) > 0) {
        // pull varint field# + type
        error err = default!;
        (data, err) = decodeField(Ꮡb, data);
        if (err != default!) {
            return err;
        }
        if (b.field >= len(dec) || dec[b.field] == default!) {
            continue;
        }
        {
            var errΔ2 = dec[b.field](Ꮡb, m); if (errΔ2 != default!) {
                return errΔ2;
            }
        }
    }
    return default!;
}

internal static error decodeInt64(ж<buffer> Ꮡb, ж<int64> Ꮡx) {
    ref var b = ref Ꮡb.val;
    ref var x = ref Ꮡx.val;

    {
        var err = checkType(Ꮡb, 0); if (err != default!) {
            return err;
        }
    }
    x = ((int64)b.u64);
    return default!;
}

internal static error decodeInt64s(ж<buffer> Ꮡb, ж<slice<int64>> Ꮡx) {
    ref var b = ref Ꮡb.val;
    ref var x = ref Ꮡx.val;

    if (b.typ == 2) {
        // Packed encoding
        var data = b.data;
        while (len(data) > 0) {
            uint64 u = default!;
            error err = default!;
            {
                (u, data, err) = decodeVarint(data); if (err != default!) {
                    return err;
                }
            }
            x = append(x, ((int64)u));
        }
        return default!;
    }
    ref var i = ref heap(new int64(), out var Ꮡi);
    {
        var err = decodeInt64(Ꮡb, Ꮡi); if (err != default!) {
            return err;
        }
    }
    x = append(x, i);
    return default!;
}

internal static error decodeUint64(ж<buffer> Ꮡb, ж<uint64> Ꮡx) {
    ref var b = ref Ꮡb.val;
    ref var x = ref Ꮡx.val;

    {
        var err = checkType(Ꮡb, 0); if (err != default!) {
            return err;
        }
    }
    x = b.u64;
    return default!;
}

internal static error decodeUint64s(ж<buffer> Ꮡb, ж<slice<uint64>> Ꮡx) {
    ref var b = ref Ꮡb.val;
    ref var x = ref Ꮡx.val;

    if (b.typ == 2) {
        var data = b.data;
        // Packed encoding
        while (len(data) > 0) {
            uint64 uΔ1 = default!;
            error err = default!;
            {
                (uΔ1, data, err) = decodeVarint(data); if (err != default!) {
                    return err;
                }
            }
            x = append(x, uΔ1);
        }
        return default!;
    }
    ref var u = ref heap(new uint64(), out var Ꮡu);
    {
        var err = decodeUint64(Ꮡb, Ꮡu); if (err != default!) {
            return err;
        }
    }
    x = append(x, u);
    return default!;
}

internal static error decodeString(ж<buffer> Ꮡb, ж<@string> Ꮡx) {
    ref var b = ref Ꮡb.val;
    ref var x = ref Ꮡx.val;

    {
        var err = checkType(Ꮡb, 2); if (err != default!) {
            return err;
        }
    }
    x = ((@string)b.data);
    return default!;
}

internal static error decodeStrings(ж<buffer> Ꮡb, ж<slice<@string>> Ꮡx) {
    ref var b = ref Ꮡb.val;
    ref var x = ref Ꮡx.val;

    ref var s = ref heap(new @string(), out var Ꮡs);
    {
        var err = decodeString(Ꮡb, Ꮡs); if (err != default!) {
            return err;
        }
    }
    x = append(x, s);
    return default!;
}

internal static error decodeBool(ж<buffer> Ꮡb, ж<bool> Ꮡx) {
    ref var b = ref Ꮡb.val;
    ref var x = ref Ꮡx.val;

    {
        var err = checkType(Ꮡb, 0); if (err != default!) {
            return err;
        }
    }
    if (((int64)b.u64) == 0){
        x = false;
    } else {
        x = true;
    }
    return default!;
}

} // end profile_package
