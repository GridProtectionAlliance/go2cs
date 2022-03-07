// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package rsa -- go2cs converted at 2022 March 06 22:17:13 UTC
// import "crypto/rsa" ==> using rsa = go.crypto.rsa_package
// Original source: C:\Program Files\Go\src\crypto\rsa\pkcs1v15.go
using crypto = go.crypto_package;
using subtle = go.crypto.subtle_package;
using errors = go.errors_package;
using io = go.io_package;
using big = go.math.big_package;

using randutil = go.crypto.@internal.randutil_package;

namespace go.crypto;

public static partial class rsa_package {

    // This file implements encryption and decryption using PKCS #1 v1.5 padding.

    // PKCS1v15DecrypterOpts is for passing options to PKCS #1 v1.5 decryption using
    // the crypto.Decrypter interface.
public partial struct PKCS1v15DecryptOptions {
    public nint SessionKeyLen;
}

// EncryptPKCS1v15 encrypts the given message with RSA and the padding
// scheme from PKCS #1 v1.5.  The message must be no longer than the
// length of the public modulus minus 11 bytes.
//
// The rand parameter is used as a source of entropy to ensure that
// encrypting the same message twice doesn't result in the same
// ciphertext.
//
// WARNING: use of this function to encrypt plaintexts other than
// session keys is dangerous. Use RSA OAEP in new protocols.
public static (slice<byte>, error) EncryptPKCS1v15(io.Reader rand, ptr<PublicKey> _addr_pub, slice<byte> msg) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref PublicKey pub = ref _addr_pub.val;

    randutil.MaybeReadByte(rand);

    {
        var err__prev1 = err;

        var err = checkPub(pub);

        if (err != null) {
            return (null, error.As(err)!);
        }
        err = err__prev1;

    }

    var k = pub.Size();
    if (len(msg) > k - 11) {
        return (null, error.As(ErrMessageTooLong)!);
    }
    var em = make_slice<byte>(k);
    em[1] = 2;
    var ps = em[(int)2..(int)len(em) - len(msg) - 1];
    var mm = em[(int)len(em) - len(msg)..];
    err = nonZeroRandomBytes(ps, rand);
    if (err != null) {
        return (null, error.As(err)!);
    }
    em[len(em) - len(msg) - 1] = 0;
    copy(mm, msg);

    ptr<object> m = @new<big.Int>().SetBytes(em);
    var c = encrypt(@new<big.Int>(), pub, m);

    return (c.FillBytes(em), error.As(null!)!);

}

// DecryptPKCS1v15 decrypts a plaintext using RSA and the padding scheme from PKCS #1 v1.5.
// If rand != nil, it uses RSA blinding to avoid timing side-channel attacks.
//
// Note that whether this function returns an error or not discloses secret
// information. If an attacker can cause this function to run repeatedly and
// learn whether each instance returned an error then they can decrypt and
// forge signatures as if they had the private key. See
// DecryptPKCS1v15SessionKey for a way of solving this problem.
public static (slice<byte>, error) DecryptPKCS1v15(io.Reader rand, ptr<PrivateKey> _addr_priv, slice<byte> ciphertext) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref PrivateKey priv = ref _addr_priv.val;

    {
        var err = checkPub(_addr_priv.PublicKey);

        if (err != null) {
            return (null, error.As(err)!);
        }
    }

    var (valid, out, index, err) = decryptPKCS1v15(rand, _addr_priv, ciphertext);
    if (err != null) {
        return (null, error.As(err)!);
    }
    if (valid == 0) {
        return (null, error.As(ErrDecryption)!);
    }
    return (out[(int)index..], error.As(null!)!);

}

// DecryptPKCS1v15SessionKey decrypts a session key using RSA and the padding scheme from PKCS #1 v1.5.
// If rand != nil, it uses RSA blinding to avoid timing side-channel attacks.
// It returns an error if the ciphertext is the wrong length or if the
// ciphertext is greater than the public modulus. Otherwise, no error is
// returned. If the padding is valid, the resulting plaintext message is copied
// into key. Otherwise, key is unchanged. These alternatives occur in constant
// time. It is intended that the user of this function generate a random
// session key beforehand and continue the protocol with the resulting value.
// This will remove any possibility that an attacker can learn any information
// about the plaintext.
// See ``Chosen Ciphertext Attacks Against Protocols Based on the RSA
// Encryption Standard PKCS #1'', Daniel Bleichenbacher, Advances in Cryptology
// (Crypto '98).
//
// Note that if the session key is too small then it may be possible for an
// attacker to brute-force it. If they can do that then they can learn whether
// a random value was used (because it'll be different for the same ciphertext)
// and thus whether the padding was correct. This defeats the point of this
// function. Using at least a 16-byte key will protect against this attack.
public static error DecryptPKCS1v15SessionKey(io.Reader rand, ptr<PrivateKey> _addr_priv, slice<byte> ciphertext, slice<byte> key) {
    ref PrivateKey priv = ref _addr_priv.val;

    {
        var err = checkPub(_addr_priv.PublicKey);

        if (err != null) {
            return error.As(err)!;
        }
    }

    var k = priv.Size();
    if (k - (len(key) + 3 + 8) < 0) {
        return error.As(ErrDecryption)!;
    }
    var (valid, em, index, err) = decryptPKCS1v15(rand, _addr_priv, ciphertext);
    if (err != null) {
        return error.As(err)!;
    }
    if (len(em) != k) { 
        // This should be impossible because decryptPKCS1v15 always
        // returns the full slice.
        return error.As(ErrDecryption)!;

    }
    valid &= subtle.ConstantTimeEq(int32(len(em) - index), int32(len(key)));
    subtle.ConstantTimeCopy(valid, key, em[(int)len(em) - len(key)..]);
    return error.As(null!)!;

}

// decryptPKCS1v15 decrypts ciphertext using priv and blinds the operation if
// rand is not nil. It returns one or zero in valid that indicates whether the
// plaintext was correctly structured. In either case, the plaintext is
// returned in em so that it may be read independently of whether it was valid
// in order to maintain constant memory access patterns. If the plaintext was
// valid then index contains the index of the original message in em.
private static (nint, slice<byte>, nint, error) decryptPKCS1v15(io.Reader rand, ptr<PrivateKey> _addr_priv, slice<byte> ciphertext) {
    nint valid = default;
    slice<byte> em = default;
    nint index = default;
    error err = default!;
    ref PrivateKey priv = ref _addr_priv.val;

    var k = priv.Size();
    if (k < 11) {
        err = ErrDecryption;
        return ;
    }
    ptr<object> c = @new<big.Int>().SetBytes(ciphertext);
    var (m, err) = decrypt(rand, priv, c);
    if (err != null) {
        return ;
    }
    em = m.FillBytes(make_slice<byte>(k));
    var firstByteIsZero = subtle.ConstantTimeByteEq(em[0], 0);
    var secondByteIsTwo = subtle.ConstantTimeByteEq(em[1], 2); 

    // The remainder of the plaintext must be a string of non-zero random
    // octets, followed by a 0, followed by the message.
    //   lookingForIndex: 1 iff we are still looking for the zero.
    //   index: the offset of the first zero byte.
    nint lookingForIndex = 1;

    for (nint i = 2; i < len(em); i++) {
        var equals0 = subtle.ConstantTimeByteEq(em[i], 0);
        index = subtle.ConstantTimeSelect(lookingForIndex & equals0, i, index);
        lookingForIndex = subtle.ConstantTimeSelect(equals0, 0, lookingForIndex);
    } 

    // The PS padding must be at least 8 bytes long, and it starts two
    // bytes into em.
    var validPS = subtle.ConstantTimeLessOrEq(2 + 8, index);

    valid = firstByteIsZero & secondByteIsTwo & (~lookingForIndex & 1) & validPS;
    index = subtle.ConstantTimeSelect(valid, index + 1, 0);
    return (valid, em, index, error.As(null!)!);

}

// nonZeroRandomBytes fills the given slice with non-zero random octets.
private static error nonZeroRandomBytes(slice<byte> s, io.Reader rand) {
    error err = default!;

    _, err = io.ReadFull(rand, s);
    if (err != null) {
        return ;
    }
    for (nint i = 0; i < len(s); i++) {
        while (s[i] == 0) {
            _, err = io.ReadFull(rand, s[(int)i..(int)i + 1]);
            if (err != null) {
                return ;
            } 
            // In tests, the PRNG may return all zeros so we do
            // this to break the loop.
            s[i] ^= 0x42;

        }

    }

    return ;

}

// These are ASN1 DER structures:
//   DigestInfo ::= SEQUENCE {
//     digestAlgorithm AlgorithmIdentifier,
//     digest OCTET STRING
//   }
// For performance, we don't use the generic ASN1 encoder. Rather, we
// precompute a prefix of the digest value that makes a valid ASN1 DER string
// with the correct contents.
private static map hashPrefixes = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<crypto.Hash, slice<byte>>{crypto.MD5:{0x30,0x20,0x30,0x0c,0x06,0x08,0x2a,0x86,0x48,0x86,0xf7,0x0d,0x02,0x05,0x05,0x00,0x04,0x10},crypto.SHA1:{0x30,0x21,0x30,0x09,0x06,0x05,0x2b,0x0e,0x03,0x02,0x1a,0x05,0x00,0x04,0x14},crypto.SHA224:{0x30,0x2d,0x30,0x0d,0x06,0x09,0x60,0x86,0x48,0x01,0x65,0x03,0x04,0x02,0x04,0x05,0x00,0x04,0x1c},crypto.SHA256:{0x30,0x31,0x30,0x0d,0x06,0x09,0x60,0x86,0x48,0x01,0x65,0x03,0x04,0x02,0x01,0x05,0x00,0x04,0x20},crypto.SHA384:{0x30,0x41,0x30,0x0d,0x06,0x09,0x60,0x86,0x48,0x01,0x65,0x03,0x04,0x02,0x02,0x05,0x00,0x04,0x30},crypto.SHA512:{0x30,0x51,0x30,0x0d,0x06,0x09,0x60,0x86,0x48,0x01,0x65,0x03,0x04,0x02,0x03,0x05,0x00,0x04,0x40},crypto.MD5SHA1:{},crypto.RIPEMD160:{0x30,0x20,0x30,0x08,0x06,0x06,0x28,0xcf,0x06,0x03,0x00,0x31,0x04,0x14},};

// SignPKCS1v15 calculates the signature of hashed using
// RSASSA-PKCS1-V1_5-SIGN from RSA PKCS #1 v1.5.  Note that hashed must
// be the result of hashing the input message using the given hash
// function. If hash is zero, hashed is signed directly. This isn't
// advisable except for interoperability.
//
// If rand is not nil then RSA blinding will be used to avoid timing
// side-channel attacks.
//
// This function is deterministic. Thus, if the set of possible
// messages is small, an attacker may be able to build a map from
// messages to signatures and identify the signed messages. As ever,
// signatures provide authenticity, not confidentiality.
public static (slice<byte>, error) SignPKCS1v15(io.Reader rand, ptr<PrivateKey> _addr_priv, crypto.Hash hash, slice<byte> hashed) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref PrivateKey priv = ref _addr_priv.val;

    var (hashLen, prefix, err) = pkcs1v15HashInfo(hash, len(hashed));
    if (err != null) {
        return (null, error.As(err)!);
    }
    var tLen = len(prefix) + hashLen;
    var k = priv.Size();
    if (k < tLen + 11) {
        return (null, error.As(ErrMessageTooLong)!);
    }
    var em = make_slice<byte>(k);
    em[1] = 1;
    for (nint i = 2; i < k - tLen - 1; i++) {
        em[i] = 0xff;
    }
    copy(em[(int)k - tLen..(int)k - hashLen], prefix);
    copy(em[(int)k - hashLen..(int)k], hashed);

    ptr<object> m = @new<big.Int>().SetBytes(em);
    var (c, err) = decryptAndCheck(rand, priv, m);
    if (err != null) {
        return (null, error.As(err)!);
    }
    return (c.FillBytes(em), error.As(null!)!);

}

// VerifyPKCS1v15 verifies an RSA PKCS #1 v1.5 signature.
// hashed is the result of hashing the input message using the given hash
// function and sig is the signature. A valid signature is indicated by
// returning a nil error. If hash is zero then hashed is used directly. This
// isn't advisable except for interoperability.
public static error VerifyPKCS1v15(ptr<PublicKey> _addr_pub, crypto.Hash hash, slice<byte> hashed, slice<byte> sig) {
    ref PublicKey pub = ref _addr_pub.val;

    var (hashLen, prefix, err) = pkcs1v15HashInfo(hash, len(hashed));
    if (err != null) {
        return error.As(err)!;
    }
    var tLen = len(prefix) + hashLen;
    var k = pub.Size();
    if (k < tLen + 11) {
        return error.As(ErrVerification)!;
    }
    if (k != len(sig)) {
        return error.As(ErrVerification)!;
    }
    ptr<object> c = @new<big.Int>().SetBytes(sig);
    var m = encrypt(@new<big.Int>(), pub, c);
    var em = m.FillBytes(make_slice<byte>(k)); 
    // EM = 0x00 || 0x01 || PS || 0x00 || T

    var ok = subtle.ConstantTimeByteEq(em[0], 0);
    ok &= subtle.ConstantTimeByteEq(em[1], 1);
    ok &= subtle.ConstantTimeCompare(em[(int)k - hashLen..(int)k], hashed);
    ok &= subtle.ConstantTimeCompare(em[(int)k - tLen..(int)k - hashLen], prefix);
    ok &= subtle.ConstantTimeByteEq(em[k - tLen - 1], 0);

    for (nint i = 2; i < k - tLen - 1; i++) {
        ok &= subtle.ConstantTimeByteEq(em[i], 0xff);
    }

    if (ok != 1) {
        return error.As(ErrVerification)!;
    }
    return error.As(null!)!;

}

private static (nint, slice<byte>, error) pkcs1v15HashInfo(crypto.Hash hash, nint inLen) {
    nint hashLen = default;
    slice<byte> prefix = default;
    error err = default!;
 
    // Special case: crypto.Hash(0) is used to indicate that the data is
    // signed directly.
    if (hash == 0) {
        return (inLen, null, error.As(null!)!);
    }
    hashLen = hash.Size();
    if (inLen != hashLen) {
        return (0, null, error.As(errors.New("crypto/rsa: input must be hashed message"))!);
    }
    var (prefix, ok) = hashPrefixes[hash];
    if (!ok) {
        return (0, null, error.As(errors.New("crypto/rsa: unsupported hash function"))!);
    }
    return ;

}

} // end rsa_package
