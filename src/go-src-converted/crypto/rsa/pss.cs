// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

// This file implements the RSASSA-PSS signature scheme according to RFC 8017.
using bytes = bytes_package;
using crypto = crypto_package;
using boring = go.crypto.@internal.boring_package;
using errors = errors_package;
using hash = hash_package;
using io = io_package;
using go.crypto.@internal;
using go.math;

partial class rsa_package {

// Per RFC 8017, Section 9.1
//
//     EM = MGF1 xor DB || H( 8*0x00 || mHash || salt ) || 0xbc
//
// where
//
//     DB = PS || 0x01 || salt
//
// and PS can be empty so
//
//     emLen = dbLen + hLen + 1 = psLen + sLen + hLen + 2
//
internal static (slice<byte>, error) emsaPSSEncode(slice<byte> mHash, nint emBits, slice<byte> salt, hash.Hash hashΔ1) {
    // See RFC 8017, Section 9.1.1.
    nint hLen = hashΔ1.Size();
    nint sLen = len(salt);
    nint emLen = (emBits + 7) / 8;
    // 1.  If the length of M is greater than the input limitation for the
    //     hash function (2^61 - 1 octets for SHA-1), output "message too
    //     long" and stop.
    //
    // 2.  Let mHash = Hash(M), an octet string of length hLen.
    if (len(mHash) != hLen) {
        return (default!, errors.New("crypto/rsa: input must be hashed with given hash"u8));
    }
    // 3.  If emLen < hLen + sLen + 2, output "encoding error" and stop.
    if (emLen < hLen + sLen + 2) {
        return (default!, ErrMessageTooLong);
    }
    var em = new slice<byte>(emLen);
    nint psLen = emLen - sLen - hLen - 2;
    var db = em[..(int)(psLen + 1 + sLen)];
    var h = em[(int)(psLen + 1 + sLen)..(int)(emLen - 1)];
    // 4.  Generate a random octet string salt of length sLen; if sLen = 0,
    //     then salt is the empty string.
    //
    // 5.  Let
    //       M' = (0x)00 00 00 00 00 00 00 00 || mHash || salt;
    //
    //     M' is an octet string of length 8 + hLen + sLen with eight
    //     initial zero octets.
    //
    // 6.  Let H = Hash(M'), an octet string of length hLen.
    array<byte> prefix = new(8);
    hashΔ1.Write(prefix[..]);
    hashΔ1.Write(mHash);
    hashΔ1.Write(salt);
    h = hashΔ1.Sum(h[..0]);
    hashΔ1.Reset();
    // 7.  Generate an octet string PS consisting of emLen - sLen - hLen - 2
    //     zero octets. The length of PS may be 0.
    //
    // 8.  Let DB = PS || 0x01 || salt; DB is an octet string of length
    //     emLen - hLen - 1.
    db[psLen] = 0x01;
    copy(db[(int)(psLen + 1)..], salt);
    // 9.  Let dbMask = MGF(H, emLen - hLen - 1).
    //
    // 10. Let maskedDB = DB \xor dbMask.
    mgf1XOR(db, hashΔ1, h);
    // 11. Set the leftmost 8 * emLen - emBits bits of the leftmost octet in
    //     maskedDB to zero.
    db[0] &= (byte)(((byte)0xff).Rsh((uint64)((8 * emLen - emBits))));
    // 12. Let EM = maskedDB || H || 0xbc.
    em[emLen - 1] = 0xbc;
    // 13. Output EM.
    return (em, default!);
}

internal static error emsaPSSVerify(slice<byte> mHash, slice<byte> em, nint emBits, nint sLen, hash.Hash hashΔ1) {
    // See RFC 8017, Section 9.1.2.
    nint hLen = hashΔ1.Size();
    if (sLen == PSSSaltLengthEqualsHash) {
        sLen = hLen;
    }
    nint emLen = (emBits + 7) / 8;
    if (emLen != len(em)) {
        return errors.New("rsa: internal error: inconsistent length"u8);
    }
    // 1.  If the length of M is greater than the input limitation for the
    //     hash function (2^61 - 1 octets for SHA-1), output "inconsistent"
    //     and stop.
    //
    // 2.  Let mHash = Hash(M), an octet string of length hLen.
    if (hLen != len(mHash)) {
        return ErrVerification;
    }
    // 3.  If emLen < hLen + sLen + 2, output "inconsistent" and stop.
    if (emLen < hLen + sLen + 2) {
        return ErrVerification;
    }
    // 4.  If the rightmost octet of EM does not have hexadecimal value
    //     0xbc, output "inconsistent" and stop.
    if (em[emLen - 1] != 0xbc) {
        return ErrVerification;
    }
    // 5.  Let maskedDB be the leftmost emLen - hLen - 1 octets of EM, and
    //     let H be the next hLen octets.
    var db = em[..(int)(emLen - hLen - 1)];
    var h = em[(int)(emLen - hLen - 1)..(int)(emLen - 1)];
    // 6.  If the leftmost 8 * emLen - emBits bits of the leftmost octet in
    //     maskedDB are not all equal to zero, output "inconsistent" and
    //     stop.
    byte bitMask = (byte)(((byte)0xff).Rsh((uint64)((8 * emLen - emBits))));
    if ((byte)(em[0] & ~bitMask) != 0) {
        return ErrVerification;
    }
    // 7.  Let dbMask = MGF(H, emLen - hLen - 1).
    //
    // 8.  Let DB = maskedDB \xor dbMask.
    mgf1XOR(db, hashΔ1, h);
    // 9.  Set the leftmost 8 * emLen - emBits bits of the leftmost octet in DB
    //     to zero.
    db[0] &= (byte)(bitMask);
    // If we don't know the salt length, look for the 0x01 delimiter.
    if (sLen == PSSSaltLengthAuto) {
        nint psLenΔ1 = bytes.IndexByte(db, 0x01);
        if (psLenΔ1 < 0) {
            return ErrVerification;
        }
        sLen = len(db) - psLenΔ1 - 1;
    }
    // 10. If the emLen - hLen - sLen - 2 leftmost octets of DB are not zero
    //     or if the octet at position emLen - hLen - sLen - 1 (the leftmost
    //     position is "position 1") does not have hexadecimal value 0x01,
    //     output "inconsistent" and stop.
    nint psLen = emLen - hLen - sLen - 2;
    foreach (var (_, e) in db[..(int)(psLen)]) {
        if (e != 0x00) {
            return ErrVerification;
        }
    }
    if (db[psLen] != 0x01) {
        return ErrVerification;
    }
    // 11.  Let salt be the last sLen octets of DB.
    var salt = db[(int)(len(db) - sLen)..];
    // 12.  Let
    //          M' = (0x)00 00 00 00 00 00 00 00 || mHash || salt ;
    //     M' is an octet string of length 8 + hLen + sLen with eight
    //     initial zero octets.
    //
    // 13. Let H' = Hash(M'), an octet string of length hLen.
    array<byte> prefix = new(8);
    hashΔ1.Write(prefix[..]);
    hashΔ1.Write(mHash);
    hashΔ1.Write(salt);
    var h0 = hashΔ1.Sum(default!);
    // 14. If H = H', output "consistent." Otherwise, output "inconsistent."
    if (!bytes.Equal(h0, h)) {
        // TODO: constant time?
        return ErrVerification;
    }
    return default!;
}

// signPSSWithSalt calculates the signature of hashed using PSS with specified salt.
// Note that hashed must be the result of hashing the input message using the
// given hash function. salt is a random sequence of bytes whose length will be
// later used to verify the signature.
internal static (slice<byte>, error) signPSSWithSalt(ж<PrivateKey> Ꮡpriv, crypto.Hash hash, slice<byte> hashed, slice<byte> salt) {
    ref var priv = ref Ꮡpriv.Value;

    nint emBits = priv.N.BitLen() - 1;
    var (em, err) = emsaPSSEncode(hashed, emBits, salt, hash.New());
    if (err != default!) {
        return (default!, err);
    }
    if (boring.Enabled) {
        var (bkey, errΔ1) = boringPrivateKey(Ꮡpriv);
        if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
        // Note: BoringCrypto always does decrypt "withCheck".
        // (It's not just decrypt.)
        (var s, errΔ1) = boring.DecryptRSANoPadding(bkey, em);
        if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
        return (s, default!);
    }
    // RFC 8017: "Note that the octet length of EM will be one less than k if
    // modBits - 1 is divisible by 8 and equal to k otherwise, where k is the
    // length in octets of the RSA modulus n." 🙄
    //
    // This is extremely annoying, as all other encrypt and decrypt inputs are
    // always the exact same size as the modulus. Since it only happens for
    // weird modulus sizes, fix it by padding inefficiently.
    {
        nint emLen = len(em);
        nint k = Ꮡpriv.of(PrivateKey.ᏑPublicKey).Size(); if (emLen < k) {
            var emNew = new slice<byte>(k);
            copy(emNew[(int)(k - emLen)..], em);
            em = emNew;
        }
    }
    return decrypt(Ꮡpriv, em, withCheck);
}

public static readonly UntypedInt PSSSaltLengthAuto = 0;
public static readonly UntypedInt PSSSaltLengthEqualsHash = -1;

// PSSOptions contains options for creating and verifying PSS signatures.
[GoType] partial struct PSSOptions {
    // SaltLength controls the length of the salt used in the PSS signature. It
    // can either be a positive number of bytes, or one of the special
    // PSSSaltLength constants.
    public nint SaltLength;
    // Hash is the hash function used to generate the message digest. If not
    // zero, it overrides the hash function passed to SignPSS. It's required
    // when using PrivateKey.Sign.
    public crypto.Hash Hash;
}

// HashFunc returns opts.Hash so that [PSSOptions] implements [crypto.SignerOpts].
[GoRecv] public static crypto.Hash HashFunc(this ref PSSOptions opts) {
    return opts.Hash;
}

internal static nint saltLength(this ж<PSSOptions> Ꮡopts) {
    ref var opts = ref Ꮡopts.DerefOrNil();

    if (Ꮡopts == nil) {
        return PSSSaltLengthAuto;
    }
    return opts.SaltLength;
}

internal static error invalidSaltLenErr = errors.New("crypto/rsa: PSSOptions.SaltLength cannot be negative"u8);

// SignPSS calculates the signature of digest using PSS.
//
// digest must be the result of hashing the input message using the given hash
// function. The opts argument may be nil, in which case sensible defaults are
// used. If opts.Hash is set, it overrides hash.
//
// The signature is randomized depending on the message, key, and salt size,
// using bytes from rand. Most applications should use [crypto/rand.Reader] as
// rand.
public static (slice<byte>, error) SignPSS(io.Reader rand, ж<PrivateKey> Ꮡpriv, crypto.Hash hash, slice<byte> digest, ж<PSSOptions> Ꮡopts) {
    ref var priv = ref Ꮡpriv.Value;
    ref var opts = ref Ꮡopts.DerefOrNil();

    // Note that while we don't commit to deterministic execution with respect
    // to the rand stream, we also don't apply MaybeReadByte, so per Hyrum's Law
    // it's probably relied upon by some. It's a tolerable promise because a
    // well-specified number of random bytes is included in the signature, in a
    // well-specified way.
    if (boring.Enabled && AreEqual(rand, boring.RandReader)) {
        var (bkey, err) = boringPrivateKey(Ꮡpriv);
        if (err != default!) {
            return (default!, err);
        }
        return boring.SignRSAPSS(bkey, hash, digest, Ꮡopts.saltLength());
    }
    boring.UnreachableExceptTests();
    if (Ꮡopts != nil && opts.Hash != 0) {
        hash = opts.Hash;
    }
    nint saltLength = Ꮡopts.saltLength();
    var exprᴛ1 = saltLength;
    if (exprᴛ1 == PSSSaltLengthAuto) {
        saltLength = (priv.N.BitLen() - 1 + 7) / 8 - 2 - hash.Size();
        if (saltLength < 0) {
            return (default!, ErrMessageTooLong);
        }
    }
    else if (exprᴛ1 == PSSSaltLengthEqualsHash) {
        saltLength = hash.Size();
    }
    else { /* default: */
        if (saltLength <= 0) {
            // If we get here saltLength is either > 0 or < -1, in the
            // latter case we fail out.
            return (default!, invalidSaltLenErr);
        }
    }

    var salt = new slice<byte>(saltLength);
    {
        var (_, err) = io.ReadFull(rand, salt); if (err != default!) {
            return (default!, err);
        }
    }
    return signPSSWithSalt(Ꮡpriv, hash, digest, salt);
}

// VerifyPSS verifies a PSS signature.
//
// A valid signature is indicated by returning a nil error. digest must be the
// result of hashing the input message using the given hash function. The opts
// argument may be nil, in which case sensible defaults are used. opts.Hash is
// ignored.
//
// The inputs are not considered confidential, and may leak through timing side
// channels, or if an attacker has control of part of the inputs.
public static error VerifyPSS(ж<PublicKey> Ꮡpub, crypto.Hash hash, slice<byte> digest, slice<byte> sig, ж<PSSOptions> Ꮡopts) {
    ref var pub = ref Ꮡpub.Value;

    if (boring.Enabled) {
        var (bkey, errΔ1) = boringPublicKey(Ꮡpub);
        if (errΔ1 != default!) {
            return errΔ1;
        }
        {
            var errΔ2 = boring.VerifyRSAPSS(bkey, hash, digest, sig, Ꮡopts.saltLength()); if (errΔ2 != default!) {
                return ErrVerification;
            }
        }
        return default!;
    }
    if (len(sig) != pub.Size()) {
        return ErrVerification;
    }
    // Salt length must be either one of the special constants (-1 or 0)
    // or otherwise positive. If it is < PSSSaltLengthEqualsHash (-1)
    // we return an error.
    if (Ꮡopts.saltLength() < PSSSaltLengthEqualsHash) {
        return invalidSaltLenErr;
    }
    nint emBits = pub.N.BitLen() - 1;
    nint emLen = (emBits + 7) / 8;
    var (em, err) = encrypt(Ꮡpub, sig);
    if (err != default!) {
        return ErrVerification;
    }
    // Like in signPSSWithSalt, deal with mismatches between emLen and the size
    // of the modulus. The spec would have us wire emLen into the encoding
    // function, but we'd rather always encode to the size of the modulus and
    // then strip leading zeroes if necessary. This only happens for weird
    // modulus sizes anyway.
    while (len(em) > emLen && len(em) > 0) {
        if (em[0] != 0) {
            return ErrVerification;
        }
        em = em[1..];
    }
    return emsaPSSVerify(digest, em, emBits, Ꮡopts.saltLength(), hash.New());
}

} // end rsa_package
