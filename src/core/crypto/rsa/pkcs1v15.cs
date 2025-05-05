// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using crypto = crypto_package;
using boring = crypto.@internal.boring_package;
using randutil = crypto.@internal.randutil_package;
using subtle = crypto.subtle_package;
using errors = errors_package;
using io = io_package;
using crypto.@internal;

partial class rsa_package {

// This file implements encryption and decryption using PKCS #1 v1.5 padding.

// PKCS1v15DecryptOptions is for passing options to PKCS #1 v1.5 decryption using
// the [crypto.Decrypter] interface.
[GoType] partial struct PKCS1v15DecryptOptions {
    // SessionKeyLen is the length of the session key that is being
    // decrypted. If not zero, then a padding error during decryption will
    // cause a random plaintext of this length to be returned rather than
    // an error. These alternatives happen in constant time.
    public nint SessionKeyLen;
}

// EncryptPKCS1v15 encrypts the given message with RSA and the padding
// scheme from PKCS #1 v1.5.  The message must be no longer than the
// length of the public modulus minus 11 bytes.
//
// The random parameter is used as a source of entropy to ensure that
// encrypting the same message twice doesn't result in the same
// ciphertext. Most applications should use [crypto/rand.Reader]
// as random. Note that the returned ciphertext does not depend
// deterministically on the bytes read from random, and may change
// between calls and/or between versions.
//
// WARNING: use of this function to encrypt plaintexts other than
// session keys is dangerous. Use RSA OAEP in new protocols.
public static (slice<byte>, error) EncryptPKCS1v15(io.Reader random, ж<PublicKey> Ꮡpub, slice<byte> msg) {
    ref var pub = ref Ꮡpub.val;

    randutil.MaybeReadByte(random);
    {
        var errΔ1 = checkPub(Ꮡpub); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    nint k = pub.Size();
    if (len(msg) > k - 11) {
        return (default!, ErrMessageTooLong);
    }
    if (boring.Enabled && random == boring.RandReader) {
        (bkeyΔ1, errΔ2) = boringPublicKey(Ꮡpub);
        if (errΔ2 != default!) {
            return (default!, errΔ2);
        }
        return boring.EncryptRSAPKCS1(bkeyΔ1, msg);
    }
    boring.UnreachableExceptTests();
    // EM = 0x00 || 0x02 || PS || 0x00 || M
    var em = new slice<byte>(k);
    em[1] = 2;
    var ps = em[2..(int)(len(em) - len(msg) - 1)];
    var mm = em[(int)(len(em) - len(msg))..];
    var err = nonZeroRandomBytes(ps, random);
    if (err != default!) {
        return (default!, err);
    }
    em[len(em) - len(msg) - 1] = 0;
    copy(mm, msg);
    if (boring.Enabled) {
        ж<boring.PublicKeyRSA> bkey = default!;
        (bkey, err) = boringPublicKey(Ꮡpub);
        if (err != default!) {
            return (default!, err);
        }
        return boring.EncryptRSANoPadding(bkey, em);
    }
    return encrypt(Ꮡpub, em);
}

// DecryptPKCS1v15 decrypts a plaintext using RSA and the padding scheme from PKCS #1 v1.5.
// The random parameter is legacy and ignored, and it can be nil.
//
// Note that whether this function returns an error or not discloses secret
// information. If an attacker can cause this function to run repeatedly and
// learn whether each instance returned an error then they can decrypt and
// forge signatures as if they had the private key. See
// DecryptPKCS1v15SessionKey for a way of solving this problem.
public static (slice<byte>, error) DecryptPKCS1v15(io.Reader random, ж<PrivateKey> Ꮡpriv, slice<byte> ciphertext) {
    ref var priv = ref Ꮡpriv.val;

    {
        var errΔ1 = checkPub(Ꮡ(priv.PublicKey)); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    if (boring.Enabled) {
        (bkey, errΔ2) = boringPrivateKey(Ꮡpriv);
        if (errΔ2 != default!) {
            return (default!, errΔ2);
        }
        (outΔ1, ) = boring.DecryptRSAPKCS1(bkey, ciphertext);
        if (errΔ2 != default!) {
            return (default!, ErrDecryption);
        }
        return (outΔ1, default!);
    }
    var (valid, @out, index, err) = decryptPKCS1v15(Ꮡpriv, ciphertext);
    if (err != default!) {
        return (default!, err);
    }
    if (valid == 0) {
        return (default!, ErrDecryption);
    }
    return (@out[(int)(index)..], default!);
}

// DecryptPKCS1v15SessionKey decrypts a session key using RSA and the padding
// scheme from PKCS #1 v1.5. The random parameter is legacy and ignored, and it
// can be nil.
//
// DecryptPKCS1v15SessionKey returns an error if the ciphertext is the wrong
// length or if the ciphertext is greater than the public modulus. Otherwise, no
// error is returned. If the padding is valid, the resulting plaintext message
// is copied into key. Otherwise, key is unchanged. These alternatives occur in
// constant time. It is intended that the user of this function generate a
// random session key beforehand and continue the protocol with the resulting
// value.
//
// Note that if the session key is too small then it may be possible for an
// attacker to brute-force it. If they can do that then they can learn whether a
// random value was used (because it'll be different for the same ciphertext)
// and thus whether the padding was correct. This also defeats the point of this
// function. Using at least a 16-byte key will protect against this attack.
//
// This method implements protections against Bleichenbacher chosen ciphertext
// attacks [0] described in RFC 3218 Section 2.3.2 [1]. While these protections
// make a Bleichenbacher attack significantly more difficult, the protections
// are only effective if the rest of the protocol which uses
// DecryptPKCS1v15SessionKey is designed with these considerations in mind. In
// particular, if any subsequent operations which use the decrypted session key
// leak any information about the key (e.g. whether it is a static or random
// key) then the mitigations are defeated. This method must be used extremely
// carefully, and typically should only be used when absolutely necessary for
// compatibility with an existing protocol (such as TLS) that is designed with
// these properties in mind.
//
//   - [0] “Chosen Ciphertext Attacks Against Protocols Based on the RSA Encryption
//     Standard PKCS #1”, Daniel Bleichenbacher, Advances in Cryptology (Crypto '98)
//   - [1] RFC 3218, Preventing the Million Message Attack on CMS,
//     https://www.rfc-editor.org/rfc/rfc3218.html
public static error DecryptPKCS1v15SessionKey(io.Reader random, ж<PrivateKey> Ꮡpriv, slice<byte> ciphertext, slice<byte> key) {
    ref var priv = ref Ꮡpriv.val;

    {
        var errΔ1 = checkPub(Ꮡ(priv.PublicKey)); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    nint k = priv.Size();
    if (k - (len(key) + 3 + 8) < 0) {
        return ErrDecryption;
    }
    var (valid, em, index, err) = decryptPKCS1v15(Ꮡpriv, ciphertext);
    if (err != default!) {
        return err;
    }
    if (len(em) != k) {
        // This should be impossible because decryptPKCS1v15 always
        // returns the full slice.
        return ErrDecryption;
    }
    valid &= (nint)(subtle.ConstantTimeEq(((int32)(len(em) - index)), ((int32)len(key))));
    subtle.ConstantTimeCopy(valid, key, em[(int)(len(em) - len(key))..]);
    return default!;
}

// decryptPKCS1v15 decrypts ciphertext using priv. It returns one or zero in
// valid that indicates whether the plaintext was correctly structured.
// In either case, the plaintext is returned in em so that it may be read
// independently of whether it was valid in order to maintain constant memory
// access patterns. If the plaintext was valid then index contains the index of
// the original message in em, to allow constant time padding removal.
internal static (nint valid, slice<byte> em, nint index, error err) decryptPKCS1v15(ж<PrivateKey> Ꮡpriv, slice<byte> ciphertext) {
    nint valid = default!;
    slice<byte> em = default!;
    nint index = default!;
    error err = default!;

    ref var priv = ref Ꮡpriv.val;
    nint k = priv.Size();
    if (k < 11) {
        err = ErrDecryption;
        return (valid, em, index, err);
    }
    if (boring.Enabled){
        ж<boring.PrivateKeyRSA> bkey = default!;
        (bkey, err) = boringPrivateKey(Ꮡpriv);
        if (err != default!) {
            return (valid, em, index, err);
        }
        (em, err) = boring.DecryptRSANoPadding(bkey, ciphertext);
        if (err != default!) {
            return (valid, em, index, err);
        }
    } else {
        (em, err) = decrypt(Ꮡpriv, ciphertext, noCheck);
        if (err != default!) {
            return (valid, em, index, err);
        }
    }
    nint firstByteIsZero = subtle.ConstantTimeByteEq(em[0], 0);
    nint secondByteIsTwo = subtle.ConstantTimeByteEq(em[1], 2);
    // The remainder of the plaintext must be a string of non-zero random
    // octets, followed by a 0, followed by the message.
    //   lookingForIndex: 1 iff we are still looking for the zero.
    //   index: the offset of the first zero byte.
    nint lookingForIndex = 1;
    for (nint i = 2; i < len(em); i++) {
        nint equals0 = subtle.ConstantTimeByteEq(em[i], 0);
        index = subtle.ConstantTimeSelect((nint)(lookingForIndex & equals0), i, index);
        lookingForIndex = subtle.ConstantTimeSelect(equals0, 0, lookingForIndex);
    }
    // The PS padding must be at least 8 bytes long, and it starts two
    // bytes into em.
    nint validPS = subtle.ConstantTimeLessOrEq(2 + 8, index);
    valid = (nint)((nint)((nint)(firstByteIsZero & secondByteIsTwo) & ((nint)(^lookingForIndex & 1))) & validPS);
    index = subtle.ConstantTimeSelect(valid, index + 1, 0);
    return (valid, em, index, default!);
}

// nonZeroRandomBytes fills the given slice with non-zero random octets.
internal static error /*err*/ nonZeroRandomBytes(slice<byte> s, io.Reader random) {
    error err = default!;

    (_, err) = io.ReadFull(random, s);
    if (err != default!) {
        return err;
    }
    for (nint i = 0; i < len(s); i++) {
        while (s[i] == 0) {
            (_, err) = io.ReadFull(random, s[(int)(i)..(int)(i + 1)]);
            if (err != default!) {
                return err;
            }
            // In tests, the PRNG may return all zeros so we do
            // this to break the loop.
            s[i] ^= (byte)(66);
        }
    }
    return err;
}

// A special TLS case which doesn't use an ASN1 prefix.
// These are ASN1 DER structures:
//
//	DigestInfo ::= SEQUENCE {
//	  digestAlgorithm AlgorithmIdentifier,
//	  digest OCTET STRING
//	}
//
// For performance, we don't use the generic ASN1 encoder. Rather, we
// precompute a prefix of the digest value that makes a valid ASN1 DER string
// with the correct contents.
internal static map<crypto.Hash, slice<byte>> hashPrefixes = new map<crypto.Hash, slice<byte>>{
    [crypto.MD5] = new(48, 32, 48, 12, 6, 8, 42, 134, 72, 134, 247, 13, 2, 5, 5, 0, 4, 16),
    [crypto.SHA1] = new(48, 33, 48, 9, 6, 5, 43, 14, 3, 2, 26, 5, 0, 4, 20),
    [crypto.SHA224] = new(48, 45, 48, 13, 6, 9, 96, 134, 72, 1, 101, 3, 4, 2, 4, 5, 0, 4, 28),
    [crypto.SHA256] = new(48, 49, 48, 13, 6, 9, 96, 134, 72, 1, 101, 3, 4, 2, 1, 5, 0, 4, 32),
    [crypto.SHA384] = new(48, 65, 48, 13, 6, 9, 96, 134, 72, 1, 101, 3, 4, 2, 2, 5, 0, 4, 48),
    [crypto.SHA512] = new(48, 81, 48, 13, 6, 9, 96, 134, 72, 1, 101, 3, 4, 2, 3, 5, 0, 4, 64),
    [crypto.MD5SHA1] = new(),
    [crypto.RIPEMD160] = new(48, 32, 48, 8, 6, 6, 40, 207, 6, 3, 0, 49, 4, 20)
};

// SignPKCS1v15 calculates the signature of hashed using
// RSASSA-PKCS1-V1_5-SIGN from RSA PKCS #1 v1.5.  Note that hashed must
// be the result of hashing the input message using the given hash
// function. If hash is zero, hashed is signed directly. This isn't
// advisable except for interoperability.
//
// The random parameter is legacy and ignored, and it can be nil.
//
// This function is deterministic. Thus, if the set of possible
// messages is small, an attacker may be able to build a map from
// messages to signatures and identify the signed messages. As ever,
// signatures provide authenticity, not confidentiality.
public static (slice<byte>, error) SignPKCS1v15(io.Reader random, ж<PrivateKey> Ꮡpriv, crypto.Hash hash, slice<byte> hashed) {
    ref var priv = ref Ꮡpriv.val;

    // pkcs1v15ConstructEM is called before boring.SignRSAPKCS1v15 to return
    // consistent errors, including ErrMessageTooLong.
    (em, err) = pkcs1v15ConstructEM(Ꮡ(priv.PublicKey), hash, hashed);
    if (err != default!) {
        return (default!, err);
    }
    if (boring.Enabled) {
        (bkey, errΔ1) = boringPrivateKey(Ꮡpriv);
        if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
        return boring.SignRSAPKCS1v15(bkey, hash, hashed);
    }
    return decrypt(Ꮡpriv, em, withCheck);
}

internal static (slice<byte>, error) pkcs1v15ConstructEM(ж<PublicKey> Ꮡpub, crypto.Hash hash, slice<byte> hashed) {
    ref var pub = ref Ꮡpub.val;

    // Special case: crypto.Hash(0) is used to indicate that the data is
    // signed directly.
    slice<byte> prefix = default!;
    if (hash != 0) {
        if (len(hashed) != hash.Size()) {
            return (default!, errors.New("crypto/rsa: input must be hashed message"u8));
        }
        bool ok = default!;
        (prefix, ok) = hashPrefixes[hash];
        if (!ok) {
            return (default!, errors.New("crypto/rsa: unsupported hash function"u8));
        }
    }
    // EM = 0x00 || 0x01 || PS || 0x00 || T
    nint k = pub.Size();
    if (k < len(prefix) + len(hashed) + 2 + 8 + 1) {
        return (default!, ErrMessageTooLong);
    }
    var em = new slice<byte>(k);
    em[1] = 1;
    for (nint i = 2; i < k - len(prefix) - len(hashed) - 1; i++) {
        em[i] = 255;
    }
    copy(em[(int)(k - len(prefix) - len(hashed))..], prefix);
    copy(em[(int)(k - len(hashed))..], hashed);
    return (em, default!);
}

// VerifyPKCS1v15 verifies an RSA PKCS #1 v1.5 signature.
// hashed is the result of hashing the input message using the given hash
// function and sig is the signature. A valid signature is indicated by
// returning a nil error. If hash is zero then hashed is used directly. This
// isn't advisable except for interoperability.
//
// The inputs are not considered confidential, and may leak through timing side
// channels, or if an attacker has control of part of the inputs.
public static error VerifyPKCS1v15(ж<PublicKey> Ꮡpub, crypto.Hash hash, slice<byte> hashed, slice<byte> sig) {
    ref var pub = ref Ꮡpub.val;

    if (boring.Enabled) {
        (bkey, errΔ1) = boringPublicKey(Ꮡpub);
        if (errΔ1 != default!) {
            return errΔ1;
        }
        {
            var errΔ2 = boring.VerifyRSAPKCS1v15(bkey, hash, hashed, sig); if (errΔ2 != default!) {
                return ErrVerification;
            }
        }
        return default!;
    }
    // RFC 8017 Section 8.2.2: If the length of the signature S is not k
    // octets (where k is the length in octets of the RSA modulus n), output
    // "invalid signature" and stop.
    if (pub.Size() != len(sig)) {
        return ErrVerification;
    }
    (em, err) = encrypt(Ꮡpub, sig);
    if (err != default!) {
        return ErrVerification;
    }
    (expected, err) = pkcs1v15ConstructEM(Ꮡpub, hash, hashed);
    if (err != default!) {
        return ErrVerification;
    }
    if (!bytes.Equal(em, expected)) {
        return ErrVerification;
    }
    return default!;
}

} // end rsa_package
