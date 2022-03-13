// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package rc4 implements RC4 encryption, as defined in Bruce Schneier's
// Applied Cryptography.
//
// RC4 is cryptographically broken and should not be used for secure
// applications.

// package rc4 -- go2cs converted at 2022 March 13 05:34:20 UTC
// import "crypto/rc4" ==> using rc4 = go.crypto.rc4_package
// Original source: C:\Program Files\Go\src\crypto\rc4\rc4.go
namespace go.crypto;

using subtle = crypto.@internal.subtle_package;
using strconv = strconv_package;


// A Cipher is an instance of RC4 using a particular key.

public static partial class rc4_package {

public partial struct Cipher {
    public array<uint> s;
    public byte i;
    public byte j;
}

public partial struct KeySizeError { // : nint
}

public static @string Error(this KeySizeError k) {
    return "crypto/rc4: invalid key size " + strconv.Itoa(int(k));
}

// NewCipher creates and returns a new Cipher. The key argument should be the
// RC4 key, at least 1 byte and at most 256 bytes.
public static (ptr<Cipher>, error) NewCipher(slice<byte> key) {
    ptr<Cipher> _p0 = default!;
    error _p0 = default!;

    var k = len(key);
    if (k < 1 || k > 256) {
        return (_addr_null!, error.As(KeySizeError(k))!);
    }
    ref Cipher c = ref heap(out ptr<Cipher> _addr_c);
    {
        nint i__prev1 = i;

        for (nint i = 0; i < 256; i++) {
            c.s[i] = uint32(i);
        }

        i = i__prev1;
    }
    byte j = 0;
    {
        nint i__prev1 = i;

        for (i = 0; i < 256; i++) {
            j += uint8(c.s[i]) + key[i % k];
            (c.s[i], c.s[j]) = (c.s[j], c.s[i]);
        }

        i = i__prev1;
    }
    return (_addr__addr_c!, error.As(null!)!);
}

// Reset zeros the key data and makes the Cipher unusable.
//
// Deprecated: Reset can't guarantee that the key will be entirely removed from
// the process's memory.
private static void Reset(this ptr<Cipher> _addr_c) {
    ref Cipher c = ref _addr_c.val;

    foreach (var (i) in c.s) {
        c.s[i] = 0;
    }    (c.i, c.j) = (0, 0);
}

// XORKeyStream sets dst to the result of XORing src with the key stream.
// Dst and src must overlap entirely or not at all.
private static void XORKeyStream(this ptr<Cipher> _addr_c, slice<byte> dst, slice<byte> src) => func((_, panic, _) => {
    ref Cipher c = ref _addr_c.val;

    if (len(src) == 0) {
        return ;
    }
    if (subtle.InexactOverlap(dst[..(int)len(src)], src)) {
        panic("crypto/rc4: invalid buffer overlap");
    }
    var i = c.i;
    var j = c.j;
    _ = dst[len(src) - 1];
    dst = dst[..(int)len(src)]; // eliminate bounds check from loop
    foreach (var (k, v) in src) {
        i += 1;
        var x = c.s[i];
        j += uint8(x);
        var y = c.s[j];
        (c.s[i], c.s[j]) = (y, x);        dst[k] = v ^ uint8(c.s[uint8(x + y)]);
    }    (c.i, c.j) = (i, j);
});

} // end rc4_package
