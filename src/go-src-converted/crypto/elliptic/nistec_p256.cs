// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build amd64 || arm64
namespace go.crypto;

using nistec = go.crypto.@internal.nistec_package;
using big = math.big_package;
using go.crypto.@internal;
using math;

partial class elliptic_package {

internal static ж<bigꓸInt> Inverse(this p256Curve c, ж<bigꓸInt> Ꮡk) {
    ref var k = ref Ꮡk.Value;

    if (k.Sign() < 0) {
        // This should never happen.
        Ꮡk = @new<bigꓸInt>().Neg(Ꮡk); k = ref Ꮡk.Value;
    }
    if (Ꮡk.Cmp((~c.@params).N) >= 0) {
        // This should never happen.
        Ꮡk = @new<bigꓸInt>().Mod(Ꮡk, (~c.@params).N); k = ref Ꮡk.Value;
    }
    var scalar = k.FillBytes(new slice<byte>(32));
    var (inverse, err) = nistec.P256OrdInverse(scalar);
    if (err != default!) {
        throw panic("crypto/elliptic: nistec rejected normalized scalar");
    }
    return @new<bigꓸInt>().SetBytes(inverse);
}

} // end elliptic_package
