// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !s390x || purego
namespace go.crypto;

using io = io_package;

partial class ecdsa_package {

internal static error verifyAsm(ж<PublicKey> Ꮡpub, slice<byte> hash, slice<byte> sig) {
    return errNoAsm;
}

internal static (slice<byte> sig, error err) signAsm(ж<PrivateKey> Ꮡpriv, io.Reader csprng, slice<byte> hash) {
    slice<byte> sig = default!;
    error err = default!;

    return (default!, errNoAsm);
}

} // end ecdsa_package
