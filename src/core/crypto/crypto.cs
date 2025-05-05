// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package crypto collects common cryptographic constants.
namespace go;

using hash = hash_package;
using io = io_package;
using strconv = strconv_package;

partial class crypto_package {

[GoType("num:nuint")] partial struct Hash;

// HashFunc simply returns the value of h so that [Hash] implements [SignerOpts].
public static Hash HashFunc(this Hash h) {
    return h;
}

public static @string String(this Hash h) {
    var exprᴛ1 = h;
    if (exprᴛ1 == MD4) {
        return "MD4"u8;
    }
    if (exprᴛ1 == MD5) {
        return "MD5"u8;
    }
    if (exprᴛ1 == SHA1) {
        return "SHA-1"u8;
    }
    if (exprᴛ1 == SHA224) {
        return "SHA-224"u8;
    }
    if (exprᴛ1 == SHA256) {
        return "SHA-256"u8;
    }
    if (exprᴛ1 == SHA384) {
        return "SHA-384"u8;
    }
    if (exprᴛ1 == SHA512) {
        return "SHA-512"u8;
    }
    if (exprᴛ1 == MD5SHA1) {
        return "MD5+SHA1"u8;
    }
    if (exprᴛ1 == RIPEMD160) {
        return "RIPEMD-160"u8;
    }
    if (exprᴛ1 == SHA3_224) {
        return "SHA3-224"u8;
    }
    if (exprᴛ1 == SHA3_256) {
        return "SHA3-256"u8;
    }
    if (exprᴛ1 == SHA3_384) {
        return "SHA3-384"u8;
    }
    if (exprᴛ1 == SHA3_512) {
        return "SHA3-512"u8;
    }
    if (exprᴛ1 == SHA512_224) {
        return "SHA-512/224"u8;
    }
    if (exprᴛ1 == SHA512_256) {
        return "SHA-512/256"u8;
    }
    if (exprᴛ1 == BLAKE2s_256) {
        return "BLAKE2s-256"u8;
    }
    if (exprᴛ1 == BLAKE2b_256) {
        return "BLAKE2b-256"u8;
    }
    if (exprᴛ1 == BLAKE2b_384) {
        return "BLAKE2b-384"u8;
    }
    if (exprᴛ1 == BLAKE2b_512) {
        return "BLAKE2b-512"u8;
    }
    { /* default: */
        return "unknown hash value "u8 + strconv.Itoa(((nint)h));
    }

}

public static readonly Hash MD4 = /* 1 + iota */ 1;              // import golang.org/x/crypto/md4
public static readonly Hash MD5 = 2;              // import crypto/md5
public static readonly Hash SHA1 = 3;             // import crypto/sha1
public static readonly Hash SHA224 = 4;           // import crypto/sha256
public static readonly Hash SHA256 = 5;           // import crypto/sha256
public static readonly Hash SHA384 = 6;           // import crypto/sha512
public static readonly Hash SHA512 = 7;           // import crypto/sha512
public static readonly Hash MD5SHA1 = 8;          // no implementation; MD5+SHA1 used for TLS RSA
public static readonly Hash RIPEMD160 = 9;        // import golang.org/x/crypto/ripemd160
public static readonly Hash SHA3_224 = 10;         // import golang.org/x/crypto/sha3
public static readonly Hash SHA3_256 = 11;         // import golang.org/x/crypto/sha3
public static readonly Hash SHA3_384 = 12;         // import golang.org/x/crypto/sha3
public static readonly Hash SHA3_512 = 13;         // import golang.org/x/crypto/sha3
public static readonly Hash SHA512_224 = 14;       // import crypto/sha512
public static readonly Hash SHA512_256 = 15;       // import crypto/sha512
public static readonly Hash BLAKE2s_256 = 16;      // import golang.org/x/crypto/blake2s
public static readonly Hash BLAKE2b_256 = 17;      // import golang.org/x/crypto/blake2b
public static readonly Hash BLAKE2b_384 = 18;      // import golang.org/x/crypto/blake2b
public static readonly Hash BLAKE2b_512 = 19;      // import golang.org/x/crypto/blake2b
internal static readonly Hash maxHash = 20;

internal static slice<uint8> digestSizes = new runtime.SparseArray<uint8>{
    [MD4] = 16,
    [MD5] = 16,
    [SHA1] = 20,
    [SHA224] = 28,
    [SHA256] = 32,
    [SHA384] = 48,
    [SHA512] = 64,
    [SHA512_224] = 28,
    [SHA512_256] = 32,
    [SHA3_224] = 28,
    [SHA3_256] = 32,
    [SHA3_384] = 48,
    [SHA3_512] = 64,
    [MD5SHA1] = 36,
    [RIPEMD160] = 20,
    [BLAKE2s_256] = 32,
    [BLAKE2b_256] = 32,
    [BLAKE2b_384] = 48,
    [BLAKE2b_512] = 64
}.slice();

// Size returns the length, in bytes, of a digest resulting from the given hash
// function. It doesn't require that the hash function in question be linked
// into the program.
public static nint Size(this Hash h) {
    if (h > 0 && h < maxHash) {
        return ((nint)digestSizes[h]);
    }
    throw panic("crypto: Size of unknown hash function");
}

internal static slice<Func<hash.Hash>> hashes = new slice<Func<hash.Hash>>(maxHash);

// New returns a new hash.Hash calculating the given hash function. New panics
// if the hash function is not linked into the binary.
public static hash.Hash New(this Hash h) {
    if (h > 0 && h < maxHash) {
        var f = hashes[h];
        if (f != default!) {
            return f();
        }
    }
    throw panic("crypto: requested hash function #"u8 + strconv.Itoa(((nint)h)) + " is unavailable"u8);
}

// Available reports whether the given hash function is linked into the binary.
public static bool Available(this Hash h) {
    return h < maxHash && hashes[h] != default!;
}

// RegisterHash registers a function that returns a new instance of the given
// hash function. This is intended to be called from the init function in
// packages that implement hash functions.
public static void RegisterHash(Hash h, Func<hash.Hash> f) {
    if (h >= maxHash) {
        throw panic("crypto: RegisterHash of unknown hash function");
    }
    hashes[h] = f;
}

[GoType("any")] partial struct PublicKey;

[GoType("any")] partial struct PrivateKey;

// Signer is an interface for an opaque private key that can be used for
// signing operations. For example, an RSA key kept in a hardware module.
[GoType] partial interface Signer {
    // Public returns the public key corresponding to the opaque,
    // private key.
    PublicKey Public();
    // Sign signs digest with the private key, possibly using entropy from
    // rand. For an RSA key, the resulting signature should be either a
    // PKCS #1 v1.5 or PSS signature (as indicated by opts). For an (EC)DSA
    // key, it should be a DER-serialised, ASN.1 signature structure.
    //
    // Hash implements the SignerOpts interface and, in most cases, one can
    // simply pass in the hash function used as opts. Sign may also attempt
    // to type assert opts to other types in order to obtain algorithm
    // specific values. See the documentation in each package for details.
    //
    // Note that when a signature of a hash of a larger message is needed,
    // the caller is responsible for hashing the larger message and passing
    // the hash (as digest) and the hash function (as opts) to Sign.
    (slice<byte> signature, error err) Sign(io.Reader rand, slice<byte> digest, SignerOpts opts);
}

// SignerOpts contains options for signing with a [Signer].
[GoType] partial interface SignerOpts {
    // HashFunc returns an identifier for the hash function used to produce
    // the message passed to Signer.Sign, or else zero to indicate that no
    // hashing was done.
    Hash HashFunc();
}

// Decrypter is an interface for an opaque private key that can be used for
// asymmetric decryption operations. An example would be an RSA key
// kept in a hardware module.
[GoType] partial interface Decrypter {
    // Public returns the public key corresponding to the opaque,
    // private key.
    PublicKey Public();
    // Decrypt decrypts msg. The opts argument should be appropriate for
    // the primitive used. See the documentation in each implementation for
    // details.
    (slice<byte> plaintext, error err) Decrypt(io.Reader rand, slice<byte> msg, DecrypterOpts opts);
}

[GoType("any")] partial struct DecrypterOpts;

} // end crypto_package
