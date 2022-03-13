// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package chacha20poly1305 -- go2cs converted at 2022 March 13 06:44:36 UTC
// import "vendor/golang.org/x/crypto/chacha20poly1305" ==> using chacha20poly1305 = go.vendor.golang.org.x.crypto.chacha20poly1305_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\crypto\chacha20poly1305\chacha20poly1305_generic.go
namespace go.vendor.golang.org.x.crypto;

using binary = encoding.binary_package;

using chacha20 = golang.org.x.crypto.chacha20_package;
using subtle = golang.org.x.crypto.@internal.subtle_package;
using poly1305 = golang.org.x.crypto.poly1305_package;

public static partial class chacha20poly1305_package {

private static void writeWithPadding(ptr<poly1305.MAC> _addr_p, slice<byte> b) {
    ref poly1305.MAC p = ref _addr_p.val;

    p.Write(b);
    {
        var rem = len(b) % 16;

        if (rem != 0) {
            array<byte> buf = new array<byte>(16);
            nint padLen = 16 - rem;
            p.Write(buf[..(int)padLen]);
        }
    }
}

private static void writeUint64(ptr<poly1305.MAC> _addr_p, nint n) {
    ref poly1305.MAC p = ref _addr_p.val;

    array<byte> buf = new array<byte>(8);
    binary.LittleEndian.PutUint64(buf[..], uint64(n));
    p.Write(buf[..]);
}

private static slice<byte> sealGeneric(this ptr<chacha20poly1305> _addr_c, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData) => func((_, panic, _) => {
    ref chacha20poly1305 c = ref _addr_c.val;

    var (ret, out) = sliceForAppend(dst, len(plaintext) + poly1305.TagSize);
    var ciphertext = out[..(int)len(plaintext)];
    var tag = out[(int)len(plaintext)..];
    if (subtle.InexactOverlap(out, plaintext)) {
        panic("chacha20poly1305: invalid buffer overlap");
    }
    ref array<byte> polyKey = ref heap(new array<byte>(32), out ptr<array<byte>> _addr_polyKey);
    var (s, _) = chacha20.NewUnauthenticatedCipher(c.key[..], nonce);
    s.XORKeyStream(polyKey[..], polyKey[..]);
    s.SetCounter(1); // set the counter to 1, skipping 32 bytes
    s.XORKeyStream(ciphertext, plaintext);

    var p = poly1305.New(_addr_polyKey);
    writeWithPadding(_addr_p, additionalData);
    writeWithPadding(_addr_p, ciphertext);
    writeUint64(_addr_p, len(additionalData));
    writeUint64(_addr_p, len(plaintext));
    p.Sum(tag[..(int)0]);

    return ret;
});

private static (slice<byte>, error) openGeneric(this ptr<chacha20poly1305> _addr_c, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> additionalData) => func((_, panic, _) => {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref chacha20poly1305 c = ref _addr_c.val;

    var tag = ciphertext[(int)len(ciphertext) - 16..];
    ciphertext = ciphertext[..(int)len(ciphertext) - 16];

    ref array<byte> polyKey = ref heap(new array<byte>(32), out ptr<array<byte>> _addr_polyKey);
    var (s, _) = chacha20.NewUnauthenticatedCipher(c.key[..], nonce);
    s.XORKeyStream(polyKey[..], polyKey[..]);
    s.SetCounter(1); // set the counter to 1, skipping 32 bytes

    var p = poly1305.New(_addr_polyKey);
    writeWithPadding(_addr_p, additionalData);
    writeWithPadding(_addr_p, ciphertext);
    writeUint64(_addr_p, len(additionalData));
    writeUint64(_addr_p, len(ciphertext));

    var (ret, out) = sliceForAppend(dst, len(ciphertext));
    if (subtle.InexactOverlap(out, ciphertext)) {
        panic("chacha20poly1305: invalid buffer overlap");
    }
    if (!p.Verify(tag)) {
        foreach (var (i) in out) {
            out[i] = 0;
        }        return (null, error.As(errOpen)!);
    }
    s.XORKeyStream(out, ciphertext);
    return (ret, error.As(null!)!);
});

} // end chacha20poly1305_package
