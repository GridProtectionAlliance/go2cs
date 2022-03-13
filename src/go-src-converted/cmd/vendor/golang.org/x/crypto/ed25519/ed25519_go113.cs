// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build go1.13
// +build go1.13

// Package ed25519 implements the Ed25519 signature algorithm. See
// https://ed25519.cr.yp.to/.
//
// These functions are also compatible with the “Ed25519” function defined in
// RFC 8032. However, unlike RFC 8032's formulation, this package's private key
// representation includes a public key suffix to make multiple signing
// operations with the same key more efficient. This package refers to the RFC
// 8032 private key as the “seed”.
//
// Beginning with Go 1.13, the functionality of this package was moved to the
// standard library as crypto/ed25519. This package only acts as a compatibility
// wrapper.

// package ed25519 -- go2cs converted at 2022 March 13 06:38:57 UTC
// import "cmd/vendor/golang.org/x/crypto/ed25519" ==> using ed25519 = go.cmd.vendor.golang.org.x.crypto.ed25519_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\crypto\ed25519\ed25519_go113.go
namespace go.cmd.vendor.golang.org.x.crypto;

using ed25519 = crypto.ed25519_package;
using io = io_package;

public static partial class ed25519_package {

 
// PublicKeySize is the size, in bytes, of public keys as used in this package.
public static readonly nint PublicKeySize = 32; 
// PrivateKeySize is the size, in bytes, of private keys as used in this package.
public static readonly nint PrivateKeySize = 64; 
// SignatureSize is the size, in bytes, of signatures generated and verified by this package.
public static readonly nint SignatureSize = 64; 
// SeedSize is the size, in bytes, of private key seeds. These are the private key representations used by RFC 8032.
public static readonly nint SeedSize = 32;

// PublicKey is the type of Ed25519 public keys.
//
// This type is an alias for crypto/ed25519's PublicKey type.
// See the crypto/ed25519 package for the methods on this type.
public partial struct PublicKey { // : ed25519.PublicKey
}

// PrivateKey is the type of Ed25519 private keys. It implements crypto.Signer.
//
// This type is an alias for crypto/ed25519's PrivateKey type.
// See the crypto/ed25519 package for the methods on this type.
public partial struct PrivateKey { // : ed25519.PrivateKey
}

// GenerateKey generates a public/private key pair using entropy from rand.
// If rand is nil, crypto/rand.Reader will be used.
public static (PublicKey, PrivateKey, error) GenerateKey(io.Reader rand) {
    PublicKey _p0 = default;
    PrivateKey _p0 = default;
    error _p0 = default!;

    return ed25519.GenerateKey(rand);
}

// NewKeyFromSeed calculates a private key from a seed. It will panic if
// len(seed) is not SeedSize. This function is provided for interoperability
// with RFC 8032. RFC 8032's private keys correspond to seeds in this
// package.
public static PrivateKey NewKeyFromSeed(slice<byte> seed) {
    return ed25519.NewKeyFromSeed(seed);
}

// Sign signs the message with privateKey and returns a signature. It will
// panic if len(privateKey) is not PrivateKeySize.
public static slice<byte> Sign(PrivateKey privateKey, slice<byte> message) {
    return ed25519.Sign(privateKey, message);
}

// Verify reports whether sig is a valid signature of message by publicKey. It
// will panic if len(publicKey) is not PublicKeySize.
public static bool Verify(PublicKey publicKey, slice<byte> message, slice<byte> sig) {
    return ed25519.Verify(publicKey, message, sig);
}

} // end ed25519_package
