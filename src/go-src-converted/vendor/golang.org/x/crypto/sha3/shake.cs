// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.vendor.golang.org.x.crypto;

// This file defines the ShakeHash interface, and provides
// functions for creating SHAKE and cSHAKE instances, as well as utility
// functions for hashing bytes to arbitrary-length output.
//
//
// SHAKE implementation is based on FIPS PUB 202 [1]
// cSHAKE implementations is based on NIST SP 800-185 [2]
//
// [1] https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.202.pdf
// [2] https://doi.org/10.6028/NIST.SP.800-185
using binary = encoding.binary_package;
using hash = hash_package;
using io = io_package;
using encoding;

partial class sha3_package {

// ShakeHash defines the interface to hash functions that support
// arbitrary-length output. When used as a plain [hash.Hash], it
// produces minimum-length outputs that provide full-strength generic
// security.
[GoType] partial interface ShakeHash :
    hash.Hash,
    io.Reader
{
    // Clone returns a copy of the ShakeHash in its current state.
    ShakeHash Clone();
}

// cSHAKE specific context
[GoType] partial struct cshakeState {
    public partial ref ж<state> state { get; } // SHA-3 state context and Read/Write operations
    // initBlock is the cSHAKE specific initialization set of bytes. It is initialized
    // by newCShake function and stores concatenation of N followed by S, encoded
    // by the method specified in 3.3 of [1].
    // It is stored here in order for Reset() to be able to put context into
    // initial state.
    internal slice<byte> initBlock;
}

// Consts for configuring initial SHA-3 state
internal static readonly UntypedInt dsbyteShake = /* 0x1f */ 31;

internal static readonly UntypedInt dsbyteCShake = /* 0x04 */ 4;

internal static readonly UntypedInt rate128 = 168;

internal static readonly UntypedInt rate256 = 136;

internal static slice<byte> bytepad(slice<byte> input, nint w) {
    // leftEncode always returns max 9 bytes
    var buf = new slice<byte>(0, 9 + len(input) + w);
    buf = append(buf, leftEncode(((uint64)w)).ꓸꓸꓸ);
    buf = append(buf, input.ꓸꓸꓸ);
    nint padlen = w - (len(buf) % w);
    return append(buf, new slice<byte>(padlen).ꓸꓸꓸ);
}

internal static slice<byte> leftEncode(uint64 value) {
    array<byte> b = new(9);
    binary.BigEndian.PutUint64(b[1..], value);
    // Trim all but last leading zero bytes
    var i = ((byte)1);
    while (i < 8 && b[i] == 0) {
        i++;
    }
    // Prepend number of encoded bytes
    b[i - 1] = 9 - i;
    return b[(int)(i - 1)..];
}

internal static ShakeHash newCShake(slice<byte> N, slice<byte> S, nint rate, nint outputLen, byte dsbyte) {
    ref var c = ref heap<cshakeState>(out var Ꮡc);
    c = new cshakeState(state: Ꮡ(new state(rate: rate, outputLen: outputLen, dsbyte: dsbyte)));
    // leftEncode returns max 9 bytes
    c.initBlock = new slice<byte>(0, 9 * 2 + len(N) + len(S));
    c.initBlock = append(c.initBlock, leftEncode(((uint64)(len(N) * 8))).ꓸꓸꓸ);
    c.initBlock = append(c.initBlock, N.ꓸꓸꓸ);
    c.initBlock = append(c.initBlock, leftEncode(((uint64)(len(S) * 8))).ꓸꓸꓸ);
    c.initBlock = append(c.initBlock, S.ꓸꓸꓸ);
    c.Write(bytepad(c.initBlock, c.rate));
    return ~Ꮡc;
}

// Reset resets the hash to initial state.
[GoRecv] internal static void Reset(this ref cshakeState c) {
    c.state.Reset();
    c.Write(bytepad(c.initBlock, c.rate));
}

// Clone returns copy of a cSHAKE context within its current state.
[GoRecv] internal static ShakeHash Clone(this ref cshakeState c) {
    var b = new slice<byte>(len(c.initBlock));
    copy(b, c.initBlock);
    return new cshakeState(state: c.clone(), initBlock: b);
}

// Clone returns copy of SHAKE context within its current state.
[GoRecv] internal static ShakeHash Clone(this ref state c) {
    return ~c.clone();
}

// NewShake128 creates a new SHAKE128 variable-output-length ShakeHash.
// Its generic security strength is 128 bits against all attacks if at
// least 32 bytes of its output are used.
public static ShakeHash NewShake128() {
    return ~newShake128();
}

// NewShake256 creates a new SHAKE256 variable-output-length ShakeHash.
// Its generic security strength is 256 bits against all attacks if
// at least 64 bytes of its output are used.
public static ShakeHash NewShake256() {
    return ~newShake256();
}

internal static ж<state> newShake128Generic() {
    return Ꮡ(new state(rate: rate128, outputLen: 32, dsbyte: dsbyteShake));
}

internal static ж<state> newShake256Generic() {
    return Ꮡ(new state(rate: rate256, outputLen: 64, dsbyte: dsbyteShake));
}

// NewCShake128 creates a new instance of cSHAKE128 variable-output-length ShakeHash,
// a customizable variant of SHAKE128.
// N is used to define functions based on cSHAKE, it can be empty when plain cSHAKE is
// desired. S is a customization byte string used for domain separation - two cSHAKE
// computations on same input with different S yield unrelated outputs.
// When N and S are both empty, this is equivalent to NewShake128.
public static ShakeHash NewCShake128(slice<byte> N, slice<byte> S) {
    if (len(N) == 0 && len(S) == 0) {
        return NewShake128();
    }
    return newCShake(N, S, rate128, 32, dsbyteCShake);
}

// NewCShake256 creates a new instance of cSHAKE256 variable-output-length ShakeHash,
// a customizable variant of SHAKE256.
// N is used to define functions based on cSHAKE, it can be empty when plain cSHAKE is
// desired. S is a customization byte string used for domain separation - two cSHAKE
// computations on same input with different S yield unrelated outputs.
// When N and S are both empty, this is equivalent to NewShake256.
public static ShakeHash NewCShake256(slice<byte> N, slice<byte> S) {
    if (len(N) == 0 && len(S) == 0) {
        return NewShake256();
    }
    return newCShake(N, S, rate256, 64, dsbyteCShake);
}

// ShakeSum128 writes an arbitrary-length digest of data into hash.
public static void ShakeSum128(slice<byte> hash, slice<byte> data) {
    var h = NewShake128();
    h.Write(data);
    h.Read(hash);
}

// ShakeSum256 writes an arbitrary-length digest of data into hash.
public static void ShakeSum256(slice<byte> hash, slice<byte> data) {
    var h = NewShake256();
    h.Write(data);
    h.Read(hash);
}

} // end sha3_package
