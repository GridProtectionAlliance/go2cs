// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.vendor.golang.org.x.crypto;

using encoding_asn1 = encoding.asn1_package;
using fmt = fmt_package;
using big = math.big_package;
using reflect = reflect_package;
using time = time_package;
using asn1 = golang.org.x.crypto.cryptobyte.asn1_package;
using golang.org.x.crypto.cryptobyte;
using math;

partial class cryptobyte_package {

// This file contains ASN.1-related methods for String and Builder.
// Builder

// AddASN1Int64 appends a DER-encoded ASN.1 INTEGER.
[GoRecv] public static void AddASN1Int64(this ref Builder b, int64 v) {
    b.addASN1Signed(asn1.INTEGER, v);
}

// AddASN1Int64WithTag appends a DER-encoded ASN.1 INTEGER with the
// given tag.
[GoRecv] public static void AddASN1Int64WithTag(this ref Builder b, int64 v, asn1.Tag tag) {
    b.addASN1Signed(tag, v);
}

// AddASN1Enum appends a DER-encoded ASN.1 ENUMERATION.
[GoRecv] public static void AddASN1Enum(this ref Builder b, int64 v) {
    b.addASN1Signed(asn1.ENUM, v);
}

[GoRecv] internal static void addASN1Signed(this ref Builder b, asn1.Tag tag, int64 v) {
    b.AddASN1(tag, (ж<Builder> c) => {
        nint length = 1;
        for (var i = v; i >= 128 || i < -128; i >>= (UntypedInt)(8)) {
            length++;
        }
        for (; length > 0; length--) {
            var i = (int64)(v >> (int)(((nuint)((length - 1) * 8))) & 255);
            c.AddUint8(((uint8)i));
        }
    });
}

// AddASN1Uint64 appends a DER-encoded ASN.1 INTEGER.
[GoRecv] public static void AddASN1Uint64(this ref Builder b, uint64 v) {
    b.AddASN1(asn1.INTEGER, (ж<Builder> c) => {
        nint length = 1;
        for (var i = v; i >= 128; i >>= (UntypedInt)(8)) {
            length++;
        }
        for (; length > 0; length--) {
            var i = (uint64)(v >> (int)(((nuint)((length - 1) * 8))) & 255);
            c.AddUint8(((uint8)i));
        }
    });
}

// AddASN1BigInt appends a DER-encoded ASN.1 INTEGER.
[GoRecv] public static void AddASN1BigInt(this ref Builder b, ж<bigꓸInt> Ꮡn) {
    ref var n = ref Ꮡn.val;

    if (b.err != default!) {
        return;
    }
    b.AddASN1(asn1.INTEGER, (ж<Builder> c) => {
        if (n.Sign() < 0){
            // A negative number has to be converted to two's-complement form. So we
            // invert and subtract 1. If the most-significant-bit isn't set then
            // we'll need to pad the beginning with 0xff in order to keep the number
            // negative.
            var nMinus1 = @new<bigꓸInt>().Neg(Ꮡn);
            nMinus1.Sub(nMinus1, bigOne);
            var bytes = nMinus1.Bytes();
            foreach (var (i, _) in bytes) {
                bytes[i] ^= (byte)(255);
            }
            if (len(bytes) == 0 || (byte)(bytes[0] & 128) == 0) {
                c.add(255);
            }
            c.add(bytes.ꓸꓸꓸ);
        } else 
        if (n.Sign() == 0){
            c.add(0);
        } else {
            var bytes = n.Bytes();
            if ((byte)(bytes[0] & 128) != 0) {
                c.add(0);
            }
            c.add(bytes.ꓸꓸꓸ);
        }
    });
}

// AddASN1OctetString appends a DER-encoded ASN.1 OCTET STRING.
[GoRecv] public static void AddASN1OctetString(this ref Builder b, slice<byte> bytes) {
    b.AddASN1(asn1.OCTET_STRING, 
    var bytesʗ1 = bytes;
    (ж<Builder> c) => {
        c.AddBytes(bytesʗ1);
    });
}

internal static readonly @string generalizedTimeFormatStr = "20060102150405Z0700"u8;

// AddASN1GeneralizedTime appends a DER-encoded ASN.1 GENERALIZEDTIME.
[GoRecv] public static void AddASN1GeneralizedTime(this ref Builder b, time.Time t) {
    if (t.Year() < 0 || t.Year() > 9999) {
        b.err = fmt.Errorf("cryptobyte: cannot represent %v as a GeneralizedTime"u8, t);
        return;
    }
    b.AddASN1(asn1.GeneralizedTime, 
    var tʗ1 = t;
    (ж<Builder> c) => {
        c.AddBytes(slice<byte>(tʗ1.Format(generalizedTimeFormatStr)));
    });
}

// AddASN1UTCTime appends a DER-encoded ASN.1 UTCTime.
[GoRecv] public static void AddASN1UTCTime(this ref Builder b, time.Time t) {
    b.AddASN1(asn1.UTCTime, 
    var tʗ1 = t;
    (ж<Builder> c) => {
        // As utilized by the X.509 profile, UTCTime can only
        // represent the years 1950 through 2049.
        if (tʗ1.Year() < 1950 || tʗ1.Year() >= 2050) {
            b.err = fmt.Errorf("cryptobyte: cannot represent %v as a UTCTime"u8, tʗ1);
            return;
        }
        c.AddBytes(slice<byte>(tʗ1.Format(defaultUTCTimeFormatStr)));
    });
}

// AddASN1BitString appends a DER-encoded ASN.1 BIT STRING. This does not
// support BIT STRINGs that are not a whole number of bytes.
[GoRecv] public static void AddASN1BitString(this ref Builder b, slice<byte> data) {
    b.AddASN1(asn1.BIT_STRING, 
    var dataʗ1 = data;
    (ж<Builder> b) => {
        bΔ1.AddUint8(0);
        bΔ1.AddBytes(dataʗ1);
    });
}

[GoRecv] internal static void addBase128Int(this ref Builder b, int64 n) {
    nint length = default!;
    if (n == 0){
        length = 1;
    } else {
        for (var iΔ1 = n; iΔ1 > 0;  >>= (UntypedInt)(7)) {
            length++;
        }
    }
    for (nint i = length - 1; i >= 0; i--) {
        var o = ((byte)(n >> (int)(((nuint)(i * 7)))));
        o &= (byte)(127);
        if (i != 0) {
            o |= (byte)(128);
        }
        b.add(o);
    }
}

internal static bool isValidOID(asn1.ObjectIdentifier oid) {
    if (len(oid) < 2) {
        return false;
    }
    if (oid[0] > 2 || (oid[0] <= 1 && oid[1] >= 40)) {
        return false;
    }
    foreach (var (_, v) in oid) {
        if (v < 0) {
            return false;
        }
    }
    return true;
}

[GoRecv] public static void AddASN1ObjectIdentifier(this ref Builder b, asn1.ObjectIdentifier oid) {
    b.AddASN1(asn1.OBJECT_IDENTIFIER, 
    var oidʗ1 = oid;
    (ж<Builder> b) => {
        if (!isValidOID(oidʗ1)) {
            bΔ1.val.err = fmt.Errorf("cryptobyte: invalid OID: %v"u8, oidʗ1);
            return;
        }
        bΔ1.addBase128Int(((int64)oidʗ1[0]) * 40 + ((int64)oidʗ1[1]));
        foreach (var (_, v) in oidʗ1[2..]) {
            bΔ1.addBase128Int(((int64)v));
        }
    });
}

[GoRecv] public static void AddASN1Boolean(this ref Builder b, bool v) {
    b.AddASN1(asn1.BOOLEAN, (ж<Builder> b) => {
        if (v){
            bΔ1.AddUint8(255);
        } else {
            bΔ1.AddUint8(0);
        }
    });
}

[GoRecv] public static void AddASN1NULL(this ref Builder b) {
    b.add(((uint8)asn1.NULL), 0);
}

// MarshalASN1 calls encoding_asn1.Marshal on its input and appends the result if
// successful or records an error if one occurred.
[GoRecv] public static void MarshalASN1(this ref Builder b, any v) {
    // NOTE(martinkr): This is somewhat of a hack to allow propagation of
    // encoding_asn1.Marshal errors into Builder.err. N.B. if you call MarshalASN1 with a
    // value embedded into a struct, its tag information is lost.
    if (b.err != default!) {
        return;
    }
    (bytes, err) = encoding_asn1.Marshal(v);
    if (err != default!) {
        b.err = err;
        return;
    }
    b.AddBytes(bytes);
}

// AddASN1 appends an ASN.1 object. The object is prefixed with the given tag.
// Tags greater than 30 are not supported and result in an error (i.e.
// low-tag-number form only). The child builder passed to the
// BuilderContinuation can be used to build the content of the ASN.1 object.
[GoRecv] public static void AddASN1(this ref Builder b, asn1.Tag tag, BuilderContinuation f) {
    if (b.err != default!) {
        return;
    }
    // Identifiers with the low five bits set indicate high-tag-number format
    // (two or more octets), which we don't support.
    if ((asn1.Tag)(tag & 31) == 31) {
        b.err = fmt.Errorf("cryptobyte: high-tag number identifier octects not supported: 0x%x"u8, tag);
        return;
    }
    b.AddUint8(((uint8)tag));
    b.addLengthPrefixed(1, true, f);
}

// String

// ReadASN1Boolean decodes an ASN.1 BOOLEAN and converts it to a boolean
// representation into out and advances. It reports whether the read
// was successful.
[GoRecv] public static bool ReadASN1Boolean(this ref String s, ж<bool> Ꮡout) {
    ref var @out = ref Ꮡout.val;

    String bytes = default!;
    if (!s.ReadASN1(Ꮡ(bytes), asn1.BOOLEAN) || len(bytes) != 1) {
        return false;
    }
    switch (bytes[0]) {
    case 0: {
        @out = false;
        break;
    }
    case 255: {
        @out = true;
        break;
    }
    default: {
        return false;
    }}

    return true;
}

// ReadASN1Integer decodes an ASN.1 INTEGER into out and advances. If out does
// not point to an integer, to a big.Int, or to a []byte it panics. Only
// positive and zero values can be decoded into []byte, and they are returned as
// big-endian binary values that share memory with s. Positive values will have
// no leading zeroes, and zero will be returned as a single zero byte.
// ReadASN1Integer reports whether the read was successful.
[GoRecv] public static bool ReadASN1Integer(this ref String s, any @out) {
    switch (@out.type()) {
    case @int.val @out: {
        ref var i = ref heap(new int64(), out var Ꮡi);
        if (!s.readASN1Int64(Ꮡi) || reflect.ValueOf(@out).Elem().OverflowInt(i)) {
            return false;
        }
        reflect.ValueOf(@out).Elem().SetInt(i);
        return true;
    }
    case int8.val @out: {
        ref var i = ref heap(new int64(), out var Ꮡi);
        if (!s.readASN1Int64(Ꮡi) || reflect.ValueOf(@out).Elem().OverflowInt(i)) {
            return false;
        }
        reflect.ValueOf(@out).Elem().SetInt(i);
        return true;
    }
    case int16.val @out: {
        ref var i = ref heap(new int64(), out var Ꮡi);
        if (!s.readASN1Int64(Ꮡi) || reflect.ValueOf(@out).Elem().OverflowInt(i)) {
            return false;
        }
        reflect.ValueOf(@out).Elem().SetInt(i);
        return true;
    }
    case int32.val @out: {
        ref var i = ref heap(new int64(), out var Ꮡi);
        if (!s.readASN1Int64(Ꮡi) || reflect.ValueOf(@out).Elem().OverflowInt(i)) {
            return false;
        }
        reflect.ValueOf(@out).Elem().SetInt(i);
        return true;
    }
    case int64.val @out: {
        ref var i = ref heap(new int64(), out var Ꮡi);
        if (!s.readASN1Int64(Ꮡi) || reflect.ValueOf(@out).Elem().OverflowInt(i)) {
            return false;
        }
        reflect.ValueOf(@out).Elem().SetInt(i);
        return true;
    }
    case @uint.val @out: {
        ref var u = ref heap(new uint64(), out var Ꮡu);
        if (!s.readASN1Uint64(Ꮡu) || reflect.ValueOf(@out).Elem().OverflowUint(u)) {
            return false;
        }
        reflect.ValueOf(@out).Elem().SetUint(u);
        return true;
    }
    case uint8.val @out: {
        ref var u = ref heap(new uint64(), out var Ꮡu);
        if (!s.readASN1Uint64(Ꮡu) || reflect.ValueOf(@out).Elem().OverflowUint(u)) {
            return false;
        }
        reflect.ValueOf(@out).Elem().SetUint(u);
        return true;
    }
    case uint16.val @out: {
        ref var u = ref heap(new uint64(), out var Ꮡu);
        if (!s.readASN1Uint64(Ꮡu) || reflect.ValueOf(@out).Elem().OverflowUint(u)) {
            return false;
        }
        reflect.ValueOf(@out).Elem().SetUint(u);
        return true;
    }
    case uint32.val @out: {
        ref var u = ref heap(new uint64(), out var Ꮡu);
        if (!s.readASN1Uint64(Ꮡu) || reflect.ValueOf(@out).Elem().OverflowUint(u)) {
            return false;
        }
        reflect.ValueOf(@out).Elem().SetUint(u);
        return true;
    }
    case uint64.val @out: {
        ref var u = ref heap(new uint64(), out var Ꮡu);
        if (!s.readASN1Uint64(Ꮡu) || reflect.ValueOf(@out).Elem().OverflowUint(u)) {
            return false;
        }
        reflect.ValueOf(@out).Elem().SetUint(u);
        return true;
    }
    case ж<bigꓸInt> @out: {
        return s.readASN1BigInt(Ꮡout);
    }
    case slice<byte>.val @out: {
        return s.readASN1Bytes(Ꮡout);
    }
    default: {
        var @out = @out.type();
        throw panic("out does not point to an integer type");
        break;
    }}
}

internal static bool checkASN1Integer(slice<byte> bytes) {
    if (len(bytes) == 0) {
        // An INTEGER is encoded with at least one octet.
        return false;
    }
    if (len(bytes) == 1) {
        return true;
    }
    if (bytes[0] == 0 && (byte)(bytes[1] & 128) == 0 || bytes[0] == 255 && (byte)(bytes[1] & 128) == 128) {
        // Value is not minimally encoded.
        return false;
    }
    return true;
}

internal static ж<bigꓸInt> bigOne = big.NewInt(1);

[GoRecv] public static bool readASN1BigInt(this ref String s, ж<bigꓸInt> Ꮡout) {
    ref var @out = ref Ꮡout.val;

    String bytes = default!;
    if (!s.ReadASN1(Ꮡ(bytes), asn1.INTEGER) || !checkASN1Integer(bytes)) {
        return false;
    }
    if ((byte)(bytes[0] & 128) == 128){
        // Negative number.
        var neg = new slice<byte>(len(bytes));
        foreach (var (i, b) in bytes) {
            neg[i] = ~b;
        }
        @out.SetBytes(neg);
        @out.Add(Ꮡout, bigOne);
        @out.Neg(Ꮡout);
    } else {
        @out.SetBytes(bytes);
    }
    return true;
}

[GoRecv] public static bool readASN1Bytes(this ref String s, ж<slice<byte>> Ꮡout) {
    ref var @out = ref Ꮡout.val;

    String bytes = default!;
    if (!s.ReadASN1(Ꮡ(bytes), asn1.INTEGER) || !checkASN1Integer(bytes)) {
        return false;
    }
    if ((byte)(bytes[0] & 128) == 128) {
        return false;
    }
    while (len(bytes) > 1 && bytes[0] == 0) {
        bytes = bytes[1..];
    }
    @out = bytes;
    return true;
}

[GoRecv] public static bool readASN1Int64(this ref String s, ж<int64> Ꮡout) {
    ref var @out = ref Ꮡout.val;

    String bytes = default!;
    if (!s.ReadASN1(Ꮡ(bytes), asn1.INTEGER) || !checkASN1Integer(bytes) || !asn1Signed(Ꮡout, bytes)) {
        return false;
    }
    return true;
}

internal static bool asn1Signed(ж<int64> Ꮡout, slice<byte> n) {
    ref var @out = ref Ꮡout.val;

    nint length = len(n);
    if (length > 8) {
        return false;
    }
    for (nint i = 0; i < length; i++) {
        @out <<= (UntypedInt)(8);
        @out |= (int64)(((int64)n[i]));
    }
    // Shift up and down in order to sign extend the result.
    @out <<= (uint8)(64 - ((uint8)length) * 8);
    @out >>= (uint8)(64 - ((uint8)length) * 8);
    return true;
}

[GoRecv] public static bool readASN1Uint64(this ref String s, ж<uint64> Ꮡout) {
    ref var @out = ref Ꮡout.val;

    String bytes = default!;
    if (!s.ReadASN1(Ꮡ(bytes), asn1.INTEGER) || !checkASN1Integer(bytes) || !asn1Unsigned(Ꮡout, bytes)) {
        return false;
    }
    return true;
}

internal static bool asn1Unsigned(ж<uint64> Ꮡout, slice<byte> n) {
    ref var @out = ref Ꮡout.val;

    nint length = len(n);
    if (length > 9 || length == 9 && n[0] != 0) {
        // Too large for uint64.
        return false;
    }
    if ((byte)(n[0] & 128) != 0) {
        // Negative number.
        return false;
    }
    for (nint i = 0; i < length; i++) {
        @out <<= (UntypedInt)(8);
        @out |= (uint64)(((uint64)n[i]));
    }
    return true;
}

// ReadASN1Int64WithTag decodes an ASN.1 INTEGER with the given tag into out
// and advances. It reports whether the read was successful and resulted in a
// value that can be represented in an int64.
[GoRecv] public static bool ReadASN1Int64WithTag(this ref String s, ж<int64> Ꮡout, asn1.Tag tag) {
    ref var @out = ref Ꮡout.val;

    String bytes = default!;
    return s.ReadASN1(Ꮡ(bytes), tag) && checkASN1Integer(bytes) && asn1Signed(Ꮡout, bytes);
}

// ReadASN1Enum decodes an ASN.1 ENUMERATION into out and advances. It reports
// whether the read was successful.
[GoRecv] public static bool ReadASN1Enum(this ref String s, ж<nint> Ꮡout) {
    ref var @out = ref Ꮡout.val;

    String bytes = default!;
    ref var i = ref heap(new int64(), out var Ꮡi);
    if (!s.ReadASN1(Ꮡ(bytes), asn1.ENUM) || !checkASN1Integer(bytes) || !asn1Signed(Ꮡi, bytes)) {
        return false;
    }
    if (((int64)((nint)i)) != i) {
        return false;
    }
    @out = ((nint)i);
    return true;
}

[GoRecv] public static bool readBase128Int(this ref String s, ж<nint> Ꮡout) {
    ref var @out = ref Ꮡout.val;

    nint ret = 0;
    for (nint i = 0; len(s) > 0; i++) {
        if (i == 5) {
            return false;
        }
        // Avoid overflowing int on a 32-bit platform.
        // We don't want different behavior based on the architecture.
        if (ret >= 1 << (int)((31 - 7))) {
            return false;
        }
        ret <<= (UntypedInt)(7);
        var b = s.read(1)[0];
        // ITU-T X.690, section 8.19.2:
        // The subidentifier shall be encoded in the fewest possible octets,
        // that is, the leading octet of the subidentifier shall not have the value 0x80.
        if (i == 0 && b == 128) {
            return false;
        }
        ret |= (nint)(((nint)((byte)(b & 127))));
        if ((byte)(b & 128) == 0) {
            @out = ret;
            return true;
        }
    }
    return false;
}

// truncated

// ReadASN1ObjectIdentifier decodes an ASN.1 OBJECT IDENTIFIER into out and
// advances. It reports whether the read was successful.
[GoRecv] public static bool ReadASN1ObjectIdentifier(this ref String s, ж<asn1.ObjectIdentifier> Ꮡout) {
    ref var @out = ref Ꮡout.val;

    String bytes = default!;
    if (!s.ReadASN1(Ꮡ(bytes), asn1.OBJECT_IDENTIFIER) || len(bytes) == 0) {
        return false;
    }
    // In the worst case, we get two elements from the first byte (which is
    // encoded differently) and then every varint is a single byte long.
    var components = new slice<nint>(len(bytes) + 1);
    // The first varint is 40*value1 + value2:
    // According to this packing, value1 can take the values 0, 1 and 2 only.
    // When value1 = 0 or value1 = 1, then value2 is <= 39. When value1 = 2,
    // then there are no restrictions on value2.
    ref var v = ref heap(new nint(), out var Ꮡv);
    if (!bytes.readBase128Int(Ꮡv)) {
        return false;
    }
    if (v < 80){
        components[0] = v / 40;
        components[1] = v % 40;
    } else {
        components[0] = 2;
        components[1] = v - 80;
    }
    nint i = 2;
    for (; len(bytes) > 0; i++) {
        if (!bytes.readBase128Int(Ꮡv)) {
            return false;
        }
        components[i] = v;
    }
    @out = components[..(int)(i)];
    return true;
}

// ReadASN1GeneralizedTime decodes an ASN.1 GENERALIZEDTIME into out and
// advances. It reports whether the read was successful.
[GoRecv] public static bool ReadASN1GeneralizedTime(this ref String s, ж<time.Time> Ꮡout) {
    ref var @out = ref Ꮡout.val;

    String bytes = default!;
    if (!s.ReadASN1(Ꮡ(bytes), asn1.GeneralizedTime)) {
        return false;
    }
    @string t = ((@string)bytes);
    var (res, err) = time.Parse(generalizedTimeFormatStr, t);
    if (err != default!) {
        return false;
    }
    {
        @string serialized = res.Format(generalizedTimeFormatStr); if (serialized != t) {
            return false;
        }
    }
    @out = res;
    return true;
}

internal static readonly @string defaultUTCTimeFormatStr = "060102150405Z0700"u8;

// ReadASN1UTCTime decodes an ASN.1 UTCTime into out and advances.
// It reports whether the read was successful.
[GoRecv] public static bool ReadASN1UTCTime(this ref String s, ж<time.Time> Ꮡout) {
    ref var @out = ref Ꮡout.val;

    String bytes = default!;
    if (!s.ReadASN1(Ꮡ(bytes), asn1.UTCTime)) {
        return false;
    }
    @string t = ((@string)bytes);
    @string formatStr = defaultUTCTimeFormatStr;
    error err = default!;
    var (res, err) = time.Parse(formatStr, t);
    if (err != default!) {
        // Fallback to minute precision if we can't parse second
        // precision. If we are following X.509 or X.690 we shouldn't
        // support this, but we do.
        formatStr = "0601021504Z0700"u8;
        (res, err) = time.Parse(formatStr, t);
    }
    if (err != default!) {
        return false;
    }
    {
        @string serialized = res.Format(formatStr); if (serialized != t) {
            return false;
        }
    }
    if (res.Year() >= 2050) {
        // UTCTime interprets the low order digits 50-99 as 1950-99.
        // This only applies to its use in the X.509 profile.
        // See https://tools.ietf.org/html/rfc5280#section-4.1.2.5.1
        res = res.AddDate(-100, 0, 0);
    }
    @out = res;
    return true;
}

// ReadASN1BitString decodes an ASN.1 BIT STRING into out and advances.
// It reports whether the read was successful.
[GoRecv] public static bool ReadASN1BitString(this ref String s, ж<asn1.BitString> Ꮡout) {
    ref var @out = ref Ꮡout.val;

    String bytes = default!;
    if (!s.ReadASN1(Ꮡ(bytes), asn1.BIT_STRING) || len(bytes) == 0 || len(bytes) * 8 / 8 != len(bytes)) {
        return false;
    }
    var paddingBits = bytes[0];
    bytes = bytes[1..];
    if (paddingBits > 7 || len(bytes) == 0 && paddingBits != 0 || len(bytes) > 0 && (byte)(bytes[len(bytes) - 1] & (1 << (int)(paddingBits) - 1)) != 0) {
        return false;
    }
    @out.BitLength = len(bytes) * 8 - ((nint)paddingBits);
    @out.Bytes = bytes;
    return true;
}

// ReadASN1BitStringAsBytes decodes an ASN.1 BIT STRING into out and advances. It is
// an error if the BIT STRING is not a whole number of bytes. It reports
// whether the read was successful.
[GoRecv] public static bool ReadASN1BitStringAsBytes(this ref String s, ж<slice<byte>> Ꮡout) {
    ref var @out = ref Ꮡout.val;

    String bytes = default!;
    if (!s.ReadASN1(Ꮡ(bytes), asn1.BIT_STRING) || len(bytes) == 0) {
        return false;
    }
    var paddingBits = bytes[0];
    if (paddingBits != 0) {
        return false;
    }
    @out = bytes[1..];
    return true;
}

// ReadASN1Bytes reads the contents of a DER-encoded ASN.1 element (not including
// tag and length bytes) into out, and advances. The element must match the
// given tag. It reports whether the read was successful.
[GoRecv] public static bool ReadASN1Bytes(this ref String s, ж<slice<byte>> Ꮡout, asn1.Tag tag) {
    ref var @out = ref Ꮡout.val;

    return s.ReadASN1(((ж<String>)@out), tag);
}

// ReadASN1 reads the contents of a DER-encoded ASN.1 element (not including
// tag and length bytes) into out, and advances. The element must match the
// given tag. It reports whether the read was successful.
//
// Tags greater than 30 are not supported (i.e. low-tag-number format only).
[GoRecv] public static bool ReadASN1(this ref String s, ж<String> Ꮡout, asn1.Tag tag) {
    ref var @out = ref Ꮡout.val;

    ref var t = ref heap(new golang.org.x.crypto.cryptobyte.asn1_package.Tag(), out var Ꮡt);
    if (!s.ReadAnyASN1(Ꮡout, Ꮡt) || t != tag) {
        return false;
    }
    return true;
}

// ReadASN1Element reads the contents of a DER-encoded ASN.1 element (including
// tag and length bytes) into out, and advances. The element must match the
// given tag. It reports whether the read was successful.
//
// Tags greater than 30 are not supported (i.e. low-tag-number format only).
[GoRecv] public static bool ReadASN1Element(this ref String s, ж<String> Ꮡout, asn1.Tag tag) {
    ref var @out = ref Ꮡout.val;

    ref var t = ref heap(new golang.org.x.crypto.cryptobyte.asn1_package.Tag(), out var Ꮡt);
    if (!s.ReadAnyASN1Element(Ꮡout, Ꮡt) || t != tag) {
        return false;
    }
    return true;
}

// ReadAnyASN1 reads the contents of a DER-encoded ASN.1 element (not including
// tag and length bytes) into out, sets outTag to its tag, and advances.
// It reports whether the read was successful.
//
// Tags greater than 30 are not supported (i.e. low-tag-number format only).
[GoRecv] public static bool ReadAnyASN1(this ref String s, ж<String> Ꮡout, ж<asn1.Tag> ᏑoutTag) {
    ref var @out = ref Ꮡout.val;
    ref var outTag = ref ᏑoutTag.val;

    return s.readASN1(Ꮡout, ᏑoutTag, true);
}

/* skip header */

// ReadAnyASN1Element reads the contents of a DER-encoded ASN.1 element
// (including tag and length bytes) into out, sets outTag to is tag, and
// advances. It reports whether the read was successful.
//
// Tags greater than 30 are not supported (i.e. low-tag-number format only).
[GoRecv] public static bool ReadAnyASN1Element(this ref String s, ж<String> Ꮡout, ж<asn1.Tag> ᏑoutTag) {
    ref var @out = ref Ꮡout.val;
    ref var outTag = ref ᏑoutTag.val;

    return s.readASN1(Ꮡout, ᏑoutTag, false);
}

/* include header */

// PeekASN1Tag reports whether the next ASN.1 value on the string starts with
// the given tag.
public static bool PeekASN1Tag(this String s, asn1.Tag tag) {
    if (len(s) == 0) {
        return false;
    }
    return ((asn1.Tag)s[0]) == tag;
}

// SkipASN1 reads and discards an ASN.1 element with the given tag. It
// reports whether the operation was successful.
[GoRecv] public static bool SkipASN1(this ref String s, asn1.Tag tag) {
    String unused = default!;
    return s.ReadASN1(Ꮡ(unused), tag);
}

// ReadOptionalASN1 attempts to read the contents of a DER-encoded ASN.1
// element (not including tag and length bytes) tagged with the given tag into
// out. It stores whether an element with the tag was found in outPresent,
// unless outPresent is nil. It reports whether the read was successful.
[GoRecv] public static bool ReadOptionalASN1(this ref String s, ж<String> Ꮡout, ж<bool> ᏑoutPresent, asn1.Tag tag) {
    ref var @out = ref Ꮡout.val;
    ref var outPresent = ref ᏑoutPresent.val;

    var present = s.PeekASN1Tag(tag);
    if (outPresent != nil) {
        outPresent = present;
    }
    if (present && !s.ReadASN1(Ꮡout, tag)) {
        return false;
    }
    return true;
}

// SkipOptionalASN1 advances s over an ASN.1 element with the given tag, or
// else leaves s unchanged. It reports whether the operation was successful.
[GoRecv] public static bool SkipOptionalASN1(this ref String s, asn1.Tag tag) {
    if (!s.PeekASN1Tag(tag)) {
        return true;
    }
    String unused = default!;
    return s.ReadASN1(Ꮡ(unused), tag);
}

// ReadOptionalASN1Integer attempts to read an optional ASN.1 INTEGER explicitly
// tagged with tag into out and advances. If no element with a matching tag is
// present, it writes defaultValue into out instead. Otherwise, it behaves like
// ReadASN1Integer.
[GoRecv] public static bool ReadOptionalASN1Integer(this ref String s, any @out, asn1.Tag tag, any defaultValue) {
    ref var present = ref heap(new bool(), out var Ꮡpresent);
    String i = default!;
    if (!s.ReadOptionalASN1(Ꮡ(i), Ꮡpresent, tag)) {
        return false;
    }
    if (!present) {
        switch (@out.type()) {
        case @int.val : {
            reflect.ValueOf(@out).Elem().Set(reflect.ValueOf(defaultValue));
            break;
        }
        case int8.val : {
            reflect.ValueOf(@out).Elem().Set(reflect.ValueOf(defaultValue));
            break;
        }
        case int16.val : {
            reflect.ValueOf(@out).Elem().Set(reflect.ValueOf(defaultValue));
            break;
        }
        case int32.val : {
            reflect.ValueOf(@out).Elem().Set(reflect.ValueOf(defaultValue));
            break;
        }
        case int64.val : {
            reflect.ValueOf(@out).Elem().Set(reflect.ValueOf(defaultValue));
            break;
        }
        case @uint.val : {
            reflect.ValueOf(@out).Elem().Set(reflect.ValueOf(defaultValue));
            break;
        }
        case uint8.val : {
            reflect.ValueOf(@out).Elem().Set(reflect.ValueOf(defaultValue));
            break;
        }
        case uint16.val : {
            reflect.ValueOf(@out).Elem().Set(reflect.ValueOf(defaultValue));
            break;
        }
        case uint32.val : {
            reflect.ValueOf(@out).Elem().Set(reflect.ValueOf(defaultValue));
            break;
        }
        case uint64.val : {
            reflect.ValueOf(@out).Elem().Set(reflect.ValueOf(defaultValue));
            break;
        }
        case slice<byte>.val : {
            reflect.ValueOf(@out).Elem().Set(reflect.ValueOf(defaultValue));
            break;
        }
        case ж<bigꓸInt> : {
            {
                var (defaultValueΔ1, ok) = defaultValue._<ж<bigꓸInt>>(ᐧ); if (ok){
                    @out._<ж<bigꓸInt>>().Set(ᏑdefaultValueΔ1);
                } else {
                    throw panic("out points to big.Int, but defaultValue does not");
                }
            }
            break;
        }
        default: {

            throw panic("invalid integer type");
            break;
        }}

        return true;
    }
    if (!i.ReadASN1Integer(@out) || !i.Empty()) {
        return false;
    }
    return true;
}

// ReadOptionalASN1OctetString attempts to read an optional ASN.1 OCTET STRING
// explicitly tagged with tag into out and advances. If no element with a
// matching tag is present, it sets "out" to nil instead. It reports
// whether the read was successful.
[GoRecv] public static bool ReadOptionalASN1OctetString(this ref String s, ж<slice<byte>> Ꮡout, ж<bool> ᏑoutPresent, asn1.Tag tag) {
    ref var @out = ref Ꮡout.val;
    ref var outPresent = ref ᏑoutPresent.val;

    ref var present = ref heap(new bool(), out var Ꮡpresent);
    String child = default!;
    if (!s.ReadOptionalASN1(Ꮡ(child), Ꮡpresent, tag)) {
        return false;
    }
    if (outPresent != nil) {
        outPresent = present;
    }
    if (present){
        String oct = default!;
        if (!child.ReadASN1(Ꮡ(oct), asn1.OCTET_STRING) || !child.Empty()) {
            return false;
        }
        @out = oct;
    } else {
        @out = default!;
    }
    return true;
}

// ReadOptionalASN1Boolean attempts to read an optional ASN.1 BOOLEAN
// explicitly tagged with tag into out and advances. If no element with a
// matching tag is present, it sets "out" to defaultValue instead. It reports
// whether the read was successful.
[GoRecv] public static bool ReadOptionalASN1Boolean(this ref String s, ж<bool> Ꮡout, asn1.Tag tag, bool defaultValue) {
    ref var @out = ref Ꮡout.val;

    ref var present = ref heap(new bool(), out var Ꮡpresent);
    String child = default!;
    if (!s.ReadOptionalASN1(Ꮡ(child), Ꮡpresent, tag)) {
        return false;
    }
    if (!present) {
        @out = defaultValue;
        return true;
    }
    return child.ReadASN1Boolean(Ꮡout);
}

[GoRecv] public static unsafe bool readASN1(this ref String s, ж<String> Ꮡout, ж<asn1.Tag> ᏑoutTag, bool skipHeader) {
    ref var @out = ref Ꮡout.val;
    ref var outTag = ref ᏑoutTag.val;

    if (len(s) < 2) {
        return false;
    }
    var (tag, lenByte) = ((ж<ж<String>>)[0], (ж<ж<String>>)[1]);
    if ((byte)(tag & 31) == 31) {
        // ITU-T X.690 section 8.1.2
        //
        // An identifier octet with a tag part of 0x1f indicates a high-tag-number
        // form identifier with two or more octets. We only support tags less than
        // 31 (i.e. low-tag-number form, single octet identifier).
        return false;
    }
    if (outTag != nil) {
        outTag = ((asn1.Tag)tag);
    }
    // ITU-T X.690 section 8.1.3
    //
    // Bit 8 of the first length byte indicates whether the length is short- or
    // long-form.
    uint32 length = default!;                   // length includes headerLen
    uint32 headerLen = default!;
    if ((byte)(lenByte & 128) == 0){
        // Short-form length (section 8.1.3.4), encoded in bits 1-7.
        length = ((uint32)lenByte) + 2;
        headerLen = 2;
    } else {
        // Long-form length (section 8.1.3.5). Bits 1-7 encode the number of octets
        // used to encode the length.
        var lenLen = (byte)(lenByte & 127);
        ref var len32 = ref heap(new uint32(), out var Ꮡlen32);
        if (lenLen == 0 || lenLen > 4 || len(s) < ((nint)(2 + lenLen))) {
            return false;
        }
        var lenBytes = ((String)(new Span<ж<String>>((String**), 2 + lenLen)));
        if (!lenBytes.readUnsigned(Ꮡlen32, ((nint)lenLen))) {
            return false;
        }
        // ITU-T X.690 section 10.1 (DER length forms) requires encoding the length
        // with the minimum number of octets.
        if (len32 < 128) {
            // Length should have used short-form encoding.
            return false;
        }
        if (len32 >> (int)(((lenLen - 1) * 8)) == 0) {
            // Leading octet is 0. Length should have been at least one byte shorter.
            return false;
        }
        headerLen = 2 + ((uint32)lenLen);
        if (headerLen + len32 < len32) {
            // Overflow.
            return false;
        }
        length = headerLen + len32;
    }
    if (((nint)length) < 0 || !s.ReadBytes((ж<slice<byte>>)(@out), ((nint)length))) {
        return false;
    }
    if (skipHeader && !@out.Skip(((nint)headerLen))) {
        throw panic("cryptobyte: internal error");
    }
    return true;
}

} // end cryptobyte_package
