// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !boringcrypto
namespace go.crypto;

using boring = crypto.@internal.boring_package;
using crypto.@internal;

partial class ecdsa_package {

internal static (ж<boring.PublicKeyECDSA>, error) boringPublicKey(ж<PublicKey> Ꮡ) {
    ref var  = ref Ꮡ.val;

    throw panic("boringcrypto: not available");
}

internal static (ж<boring.PrivateKeyECDSA>, error) boringPrivateKey(ж<PrivateKey> Ꮡ) {
    ref var  = ref Ꮡ.val;

    throw panic("boringcrypto: not available");
}

} // end ecdsa_package
