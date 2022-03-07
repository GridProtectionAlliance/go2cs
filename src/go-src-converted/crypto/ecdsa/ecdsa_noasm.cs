// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !s390x
// +build !s390x

// package ecdsa -- go2cs converted at 2022 March 06 22:19:17 UTC
// import "crypto/ecdsa" ==> using ecdsa = go.crypto.ecdsa_package
// Original source: C:\Program Files\Go\src\crypto\ecdsa\ecdsa_noasm.go
using cipher = go.crypto.cipher_package;
using elliptic = go.crypto.elliptic_package;
using big = go.math.big_package;

namespace go.crypto;

public static partial class ecdsa_package {

private static (ptr<big.Int>, ptr<big.Int>, error) sign(ptr<PrivateKey> _addr_priv, ptr<cipher.StreamReader> _addr_csprng, elliptic.Curve c, slice<byte> hash) {
    ptr<big.Int> r = default!;
    ptr<big.Int> s = default!;
    error err = default!;
    ref PrivateKey priv = ref _addr_priv.val;
    ref cipher.StreamReader csprng = ref _addr_csprng.val;

    return _addr_signGeneric(priv, csprng, c, hash)!;
}

private static bool verify(ptr<PublicKey> _addr_pub, elliptic.Curve c, slice<byte> hash, ptr<big.Int> _addr_r, ptr<big.Int> _addr_s) {
    ref PublicKey pub = ref _addr_pub.val;
    ref big.Int r = ref _addr_r.val;
    ref big.Int s = ref _addr_s.val;

    return verifyGeneric(pub, c, hash, r, s);
}

} // end ecdsa_package
