// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package crypto collects common cryptographic constants.
// package crypto -- go2cs converted at 2020 August 29 08:28:39 UTC
// import "crypto" ==> using crypto = go.crypto_package
// Original source: C:\Go\src\crypto\crypto.go
using hash = go.hash_package;
using io = go.io_package;
using strconv = go.strconv_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class crypto_package
    {
        // Hash identifies a cryptographic hash function that is implemented in another
        // package.
        public partial struct Hash // : ulong
        {
        }

        // HashFunc simply returns the value of h so that Hash implements SignerOpts.
        public static Hash HashFunc(this Hash h)
        {
            return h;
        }

        public static readonly Hash MD4 = 1L + iota; // import golang.org/x/crypto/md4
        public static readonly var MD5 = 0; // import crypto/md5
        public static readonly var SHA1 = 1; // import crypto/sha1
        public static readonly var SHA224 = 2; // import crypto/sha256
        public static readonly var SHA256 = 3; // import crypto/sha256
        public static readonly var SHA384 = 4; // import crypto/sha512
        public static readonly var SHA512 = 5; // import crypto/sha512
        public static readonly var MD5SHA1 = 6; // no implementation; MD5+SHA1 used for TLS RSA
        public static readonly var RIPEMD160 = 7; // import golang.org/x/crypto/ripemd160
        public static readonly var SHA3_224 = 8; // import golang.org/x/crypto/sha3
        public static readonly var SHA3_256 = 9; // import golang.org/x/crypto/sha3
        public static readonly var SHA3_384 = 10; // import golang.org/x/crypto/sha3
        public static readonly var SHA3_512 = 11; // import golang.org/x/crypto/sha3
        public static readonly var SHA512_224 = 12; // import crypto/sha512
        public static readonly var SHA512_256 = 13; // import crypto/sha512
        public static readonly var BLAKE2s_256 = 14; // import golang.org/x/crypto/blake2s
        public static readonly var BLAKE2b_256 = 15; // import golang.org/x/crypto/blake2b
        public static readonly var BLAKE2b_384 = 16; // import golang.org/x/crypto/blake2b
        public static readonly var BLAKE2b_512 = 17; // import golang.org/x/crypto/blake2b
        private static readonly var maxHash = 18;

        private static byte digestSizes = new slice<byte>(InitKeyedValues<byte>((MD4, 16), (MD5, 16), (SHA1, 20), (SHA224, 28), (SHA256, 32), (SHA384, 48), (SHA512, 64), (SHA512_224, 28), (SHA512_256, 32), (SHA3_224, 28), (SHA3_256, 32), (SHA3_384, 48), (SHA3_512, 64), (MD5SHA1, 36), (RIPEMD160, 20), (BLAKE2s_256, 32), (BLAKE2b_256, 32), (BLAKE2b_384, 48), (BLAKE2b_512, 64)));

        // Size returns the length, in bytes, of a digest resulting from the given hash
        // function. It doesn't require that the hash function in question be linked
        // into the program.
        public static long Size(this Hash h) => func((_, panic, __) =>
        {
            if (h > 0L && h < maxHash)
            {
                return int(digestSizes[h]);
            }
            panic("crypto: Size of unknown hash function");
        });

        private static var hashes = make_slice<Func<hash.Hash>>(maxHash);

        // New returns a new hash.Hash calculating the given hash function. New panics
        // if the hash function is not linked into the binary.
        public static hash.Hash New(this Hash h) => func((_, panic, __) =>
        {
            if (h > 0L && h < maxHash)
            {
                var f = hashes[h];
                if (f != null)
                {
                    return f();
                }
            }
            panic("crypto: requested hash function #" + strconv.Itoa(int(h)) + " is unavailable");
        });

        // Available reports whether the given hash function is linked into the binary.
        public static bool Available(this Hash h)
        {
            return h < maxHash && hashes[h] != null;
        }

        // RegisterHash registers a function that returns a new instance of the given
        // hash function. This is intended to be called from the init function in
        // packages that implement hash functions.
        public static hash.Hash RegisterHash(Hash h, Func<hash.Hash> f) => func((_, panic, __) =>
        {
            if (h >= maxHash)
            {
                panic("crypto: RegisterHash of unknown hash function");
            }
            hashes[h] = f;
        });

        // PublicKey represents a public key using an unspecified algorithm.
        public partial interface PublicKey
        {
        }

        // PrivateKey represents a private key using an unspecified algorithm.
        public partial interface PrivateKey
        {
        }

        // Signer is an interface for an opaque private key that can be used for
        // signing operations. For example, an RSA key kept in a hardware module.
        public partial interface Signer
        {
            (slice<byte>, error) Public(); // Sign signs digest with the private key, possibly using entropy from
// rand. For an RSA key, the resulting signature should be either a
// PKCS#1 v1.5 or PSS signature (as indicated by opts). For an (EC)DSA
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
            (slice<byte>, error) Sign(io.Reader rand, slice<byte> digest, SignerOpts opts);
        }

        // SignerOpts contains options for signing with a Signer.
        public partial interface SignerOpts
        {
            Hash HashFunc();
        }

        // Decrypter is an interface for an opaque private key that can be used for
        // asymmetric decryption operations. An example would be an RSA key
        // kept in a hardware module.
        public partial interface Decrypter
        {
            (slice<byte>, error) Public(); // Decrypt decrypts msg. The opts argument should be appropriate for
// the primitive used. See the documentation in each implementation for
// details.
            (slice<byte>, error) Decrypt(io.Reader rand, slice<byte> msg, DecrypterOpts opts);
        }

        public partial interface DecrypterOpts
        {
        }
    }
}
