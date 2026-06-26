// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.vendor.golang.org.x.crypto;

// This file provides functions for creating instances of the SHA-3
// and SHAKE hash functions, as well as utility functions for hashing
// bytes.
using hash = hash_package;

partial class sha3_package {

// New224 creates a new SHA3-224 hash.
// Its generic security strength is 224 bits against preimage attacks,
// and 112 bits against collision attacks.
public static hash.Hash New224() {
    return ~new224();
}

// New256 creates a new SHA3-256 hash.
// Its generic security strength is 256 bits against preimage attacks,
// and 128 bits against collision attacks.
public static hash.Hash New256() {
    return ~new256();
}

// New384 creates a new SHA3-384 hash.
// Its generic security strength is 384 bits against preimage attacks,
// and 192 bits against collision attacks.
public static hash.Hash New384() {
    return ~new384();
}

// New512 creates a new SHA3-512 hash.
// Its generic security strength is 512 bits against preimage attacks,
// and 256 bits against collision attacks.
public static hash.Hash New512() {
    return ~new512();
}

internal static ж<state> new224Generic() {
    return Ꮡ(new state(rate: 144, outputLen: 28, dsbyte: 6));
}

internal static ж<state> new256Generic() {
    return Ꮡ(new state(rate: 136, outputLen: 32, dsbyte: 6));
}

internal static ж<state> new384Generic() {
    return Ꮡ(new state(rate: 104, outputLen: 48, dsbyte: 6));
}

internal static ж<state> new512Generic() {
    return Ꮡ(new state(rate: 72, outputLen: 64, dsbyte: 6));
}

// NewLegacyKeccak256 creates a new Keccak-256 hash.
//
// Only use this function if you require compatibility with an existing cryptosystem
// that uses non-standard padding. All other users should use New256 instead.
public static hash.Hash NewLegacyKeccak256() {
    return new state(rate: 136, outputLen: 32, dsbyte: 1);
}

// NewLegacyKeccak512 creates a new Keccak-512 hash.
//
// Only use this function if you require compatibility with an existing cryptosystem
// that uses non-standard padding. All other users should use New512 instead.
public static hash.Hash NewLegacyKeccak512() {
    return new state(rate: 72, outputLen: 64, dsbyte: 1);
}

// Sum224 returns the SHA3-224 digest of the data.
public static array<byte> /*digest*/ Sum224(slice<byte> data) {
    array<byte> digest = default!;

    var h = New224();
    h.Write(data);
    h.Sum(digest[..0]);
    return digest;
}

// Sum256 returns the SHA3-256 digest of the data.
public static array<byte> /*digest*/ Sum256(slice<byte> data) {
    array<byte> digest = default!;

    var h = New256();
    h.Write(data);
    h.Sum(digest[..0]);
    return digest;
}

// Sum384 returns the SHA3-384 digest of the data.
public static array<byte> /*digest*/ Sum384(slice<byte> data) {
    array<byte> digest = default!;

    var h = New384();
    h.Write(data);
    h.Sum(digest[..0]);
    return digest;
}

// Sum512 returns the SHA3-512 digest of the data.
public static array<byte> /*digest*/ Sum512(slice<byte> data) {
    array<byte> digest = default!;

    var h = New512();
    h.Write(data);
    h.Sum(digest[..0]);
    return digest;
}

} // end sha3_package
