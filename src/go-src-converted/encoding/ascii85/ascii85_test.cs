// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using bytes = bytes_package;
using io = io_package;
using strings = strings_package;
using testing = testing_package;
using ꓸꓸꓸany = Span<any>;

partial class ascii85_package {

[GoType] partial struct testpair {
    internal @string decoded, encoded;
}

internal static testpair bigtest = new testpair(
    "Man is distinguished, not only by his reason, but by this singular passion from " + "other animals, which is a lust of the mind, that by a perseverance of delight in " + "the continued and indefatigable generation of knowledge, exceeds the short " + "vehemence of any carnal pleasure.",
    "9jqo^BlbD-BleB1DJ+*+F(f,q/0JhKF<GL>Cj@.4Gp$d7F!,L7@<6@)/0JDEF<G%<+EV:2F!,\n" + "O<DJ+*.@<*K0@<6L(Df-\\0Ec5e;DffZ(EZee.Bl.9pF\"AGXBPCsi+DGm>@3BB/F*&OCAfu2/AKY\n" + "i(DIb:@FD,*)+C]U=@3BN#EcYf8ATD3s@q?d$AftVqCh[NqF<G:8+EV:.+Cf>-FD5W8ARlolDIa\n" + "l(DId<j@<?3r@:F%a+D58'ATD4$Bl@l3De:,-DJs`8ARoFb/0JMK@qB4^F!,R<AKZ&-DfTqBG%G\n" + ">uD.RTpAKYo'+CT/5+Cei#DII?(E,9)oF*2M7/c\n"
);

// Encode returns 0 when len(src) is 0
// Wikipedia example
// Special case when shortening !!!!! to z.
internal static slice<testpair> pairs = new testpair[]{
    new(
        ""u8,
        ""u8
    ),
    bigtest,
    new(
        "\u0000\u0000\u0000\u0000"u8,
        "z"u8
    )
}.slice();

internal static bool testEqual(ж<testing.T> Ꮡt, @string msg, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    Ꮡt.Helper();
    if (!AreEqual(args[len(args) - 2], args[len(args) - 1])) {
        Ꮡt.Errorf(msg, args.ꓸꓸꓸ);
        return false;
    }
    return true;
}

internal static @string strip85(@string s) {
    var t = new slice<byte>(len(s));
    nint w = 0;
    for (nint r = 0; r < len(s); r++) {
        var c = s[r];
        if (c > (rune)' ') {
            t[w] = c;
            w++;
        }
    }
    return ((@string)(t[0..(int)(w)]));
}

public static void TestEncode(ж<testing.T> Ꮡt) {
    foreach (var (_, p) in pairs) {
        var buf = new slice<byte>(MaxEncodedLen(len(p.decoded)));
        nint n = Encode(buf, slice<byte>(p.decoded));
        buf = buf[0..(int)(n)];
        testEqual(Ꮡt, "Encode(%q) = %q, want %q"u8, p.decoded, strip85(((@string)buf)), strip85(p.encoded));
    }
}

public static void TestEncoder(ж<testing.T> Ꮡt) {
    foreach (var (_, p) in pairs) {
        var bb = Ꮡ(new strings.Builder(nil));
        var encoder = NewEncoder(new strings_BuilderжWriter(bb));
        encoder.Write(slice<byte>(p.decoded));
        encoder.Close();
        testEqual(Ꮡt, "Encode(%q) = %q, want %q"u8, p.decoded, strip85(bb.String()), strip85(p.encoded));
    }
}

public static void TestEncoderBuffering(ж<testing.T> Ꮡt) {
    var input = slice<byte>(bigtest.decoded);
    for (nint bs = 1; bs <= 12; bs++) {
        var bb = Ꮡ(new strings.Builder(nil));
        var encoder = NewEncoder(new strings_BuilderжWriter(bb));
        for (nint pos = 0; pos < len(input); pos += bs) {
            nint end = pos + bs;
            if (end > len(input)) {
                end = len(input);
            }
            var (n, errΔ1) = encoder.Write(input[(int)(pos)..(int)(end)]);
            testEqual(Ꮡt, "Write(%q) gave error %v, want %v"u8, input[(int)(pos)..(int)(end)], errΔ1, ((error)default!));
            testEqual(Ꮡt, "Write(%q) gave length %v, want %v"u8, input[(int)(pos)..(int)(end)], n, end - pos);
        }
        var err = encoder.Close();
        testEqual(Ꮡt, "Close gave error %v, want %v"u8, err, ((error)default!));
        testEqual(Ꮡt, "Encoding/%d of %q = %q, want %q"u8, bs, bigtest.decoded, strip85(bb.String()), strip85(bigtest.encoded));
    }
}

public static void TestDecode(ж<testing.T> Ꮡt) {
    foreach (var (_, p) in pairs) {
        var dbuf = new slice<byte>(4 * len(p.encoded));
        var (ndst, nsrc, err) = Decode(dbuf, slice<byte>(p.encoded), true);
        testEqual(Ꮡt, "Decode(%q) = error %v, want %v"u8, p.encoded, err, ((error)default!));
        testEqual(Ꮡt, "Decode(%q) = nsrc %v, want %v"u8, p.encoded, nsrc, len(p.encoded));
        testEqual(Ꮡt, "Decode(%q) = ndst %v, want %v"u8, p.encoded, ndst, len(p.decoded));
        testEqual(Ꮡt, "Decode(%q) = %q, want %q"u8, p.encoded, ((@string)(dbuf[0..(int)(ndst)])), p.decoded);
    }
}

public static void TestDecoder(ж<testing.T> Ꮡt) {
    foreach (var (_, p) in pairs) {
        var decoder = NewDecoder(new strings_ReaderжReader(strings.NewReader(p.encoded)));
        var (dbuf, err) = io.ReadAll(decoder);
        if (err != default!) {
            Ꮡt.Fatal("Read failed", err);
        }
        testEqual(Ꮡt, "Read from %q = length %v, want %v"u8, p.encoded, len(dbuf), len(p.decoded));
        testEqual(Ꮡt, "Decoding of %q = %q, want %q"u8, p.encoded, ((@string)dbuf), p.decoded);
        if (err != default!) {
            testEqual(Ꮡt, "Read from %q = %v, want %v"u8, p.encoded, err, io.EOF);
        }
    }
}

public static void TestDecoderBuffering(ж<testing.T> Ꮡt) {
    for (nint bs = 1; bs <= 12; bs++) {
        var decoder = NewDecoder(new strings_ReaderжReader(strings.NewReader(bigtest.encoded)));
        var buf = new slice<byte>(len(bigtest.decoded) + 12);
        nint total = default!;
        nint n = default!;
        error err = default!;
        for (total = 0; total < len(bigtest.decoded) && err == default!; ) {
            (n, err) = decoder.Read(buf[(int)(total)..(int)(total + bs)]);
            total += n;
        }
        if (err != default! && !AreEqual(err, io.EOF)) {
            Ꮡt.Errorf("Read from %q at pos %d = %d, unexpected error %v"u8, bigtest.encoded, total, n, err);
        }
        testEqual(Ꮡt, "Decoding/%d of %q = %q, want %q"u8, bs, bigtest.encoded, ((@string)(buf[0..(int)(total)])), bigtest.decoded);
    }
}

[GoType("dyn")] partial struct TestDecodeCorrupt_corrupt {
    internal @string e;
    internal nint p;
}

public static void TestDecodeCorrupt(ж<testing.T> Ꮡt) {
    var examples = new TestDecodeCorrupt_corrupt[]{
        new("v"u8, 0),
        new("!z!!!!!!!!!"u8, 1)
    }.slice();
    foreach (var (_, e) in examples) {
        var dbuf = new slice<byte>(4 * len(e.e));
        var (_, _, err) = Decode(dbuf, slice<byte>(e.e), true);
        switch (err.type()) {
        case CorruptInputError errΔ1: {
            testEqual(Ꮡt, "Corruption in %q at offset %v, want %v"u8, e.e, (nint)(int64)errΔ1, e.p);
            break;
        }
        default: {
            var errΔ1 = err;
            Ꮡt.Error("Decoder failed to detect corruption in", e);
            break;
        }}
    }
}

public static void TestBig(ж<testing.T> Ꮡt) {
    nint n = 3 * 1000 + 1;
    var raw = new slice<byte>(n);
    @string alpha = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"u8;
    for (nint i = 0; i < n; i++) {
        raw[i] = alpha[i % len(alpha)];
    }
    var encoded = @new<bytes.Buffer>();
    var w = NewEncoder(new bytes_BufferжWriter(encoded));
    var (nn, err) = w.Write(raw);
    if (nn != n || err != default!) {
        Ꮡt.Fatalf("Encoder.Write(raw) = %d, %v want %d, nil"u8, nn, err, n);
    }
    err = w.Close();
    if (err != default!) {
        Ꮡt.Fatalf("Encoder.Close() = %v want nil"u8, err);
    }
    (var decoded, err) = io.ReadAll(NewDecoder(new bytes_BufferжReader(encoded)));
    if (err != default!) {
        Ꮡt.Fatalf("io.ReadAll(NewDecoder(...)): %v"u8, err);
    }
    if (!bytes.Equal(raw, decoded)) {
        nint i = default!;
        for (i = 0; i < len(decoded) && i < len(raw); i++) {
            if (decoded[i] != raw[i]) {
                break;
            }
        }
        Ꮡt.Errorf("Decode(Encode(%d-byte string)) failed at offset %d"u8, n, i);
    }
}

public static void TestDecoderInternalWhitespace(ж<testing.T> Ꮡt) {
    @string s = strings.Repeat(" "u8, 2048) + "z"u8;
    var (decoded, err) = io.ReadAll(NewDecoder(new strings_ReaderжReader(strings.NewReader(s))));
    if (err != default!) {
        Ꮡt.Errorf("Decode gave error %v"u8, err);
    }
    {
        var want = slice<byte>("\u0000\u0000\u0000\u0000"u8); if (!bytes.Equal(want, decoded)) {
            Ꮡt.Errorf("Decode failed: got %v, want %v"u8, decoded, want);
        }
    }
}

} // end ascii85_package
