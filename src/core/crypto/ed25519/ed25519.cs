// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package ed25519 implements the Ed25519 signature algorithm. See
// https://ed25519.cr.yp.to/.
//
// These functions are also compatible with the “Ed25519” function defined in
// RFC 8032. However, unlike RFC 8032's formulation, this package's private key
// representation includes a public key suffix to make multiple signing
// operations with the same key more efficient. This package refers to the RFC
// 8032 private key as the “seed”.
//
// Operations involving private keys are implemented using constant-time
// algorithms.
namespace go.crypto;

using bytes = bytes_package;
using crypto = crypto_package;
using edwards25519 = crypto.@internal.edwards25519_package;
using cryptorand = crypto.rand_package;
using sha512 = crypto.sha512_package;
using subtle = crypto.subtle_package;
using errors = errors_package;
using io = io_package;
using strconv = strconv_package;
using crypto.@internal;

partial class ed25519_package {

public static readonly UntypedInt PublicKeySize = 32;
public static readonly UntypedInt PrivateKeySize = 64;
public static readonly UntypedInt SignatureSize = 64;
public static readonly UntypedInt SeedSize = 32;

[GoType("[]byte")] partial struct PublicKey;

// Any methods implemented on PublicKey might need to also be implemented on
// PrivateKey, as the latter embeds the former and will expose its methods.

// Equal reports whether pub and x have the same value.
public static bool Equal(this PublicKey pub, crypto.PublicKey x) {
    var (xx, ok) = x._<PublicKey>(ᐧ);
    if (!ok) {
        return false;
    }
    return subtle.ConstantTimeCompare(pub, xx) == 1;
}

[GoType("[]byte")] partial struct PrivateKey;

// Public returns the [PublicKey] corresponding to priv.
public static crypto.PublicKey Public(this PrivateKey priv) {
    var publicKey = new slice<byte>(PublicKeySize);
    copy(publicKey, priv[32..]);
    return ((PublicKey)publicKey);
}

// Equal reports whether priv and x have the same value.
public static bool Equal(this PrivateKey priv, crypto.PrivateKey x) {
    var (xx, ok) = x._<PrivateKey>(ᐧ);
    if (!ok) {
        return false;
    }
    return subtle.ConstantTimeCompare(priv, xx) == 1;
}

// Seed returns the private key seed corresponding to priv. It is provided for
// interoperability with RFC 8032. RFC 8032's private keys correspond to seeds
// in this package.
public static slice<byte> Seed(this PrivateKey priv) {
    return bytes.Clone(priv[..(int)(SeedSize)]);
}

// Sign signs the given message with priv. rand is ignored and can be nil.
//
// If opts.HashFunc() is [crypto.SHA512], the pre-hashed variant Ed25519ph is used
// and message is expected to be a SHA-512 hash, otherwise opts.HashFunc() must
// be [crypto.Hash](0) and the message must not be hashed, as Ed25519 performs two
// passes over messages to be signed.
//
// A value of type [Options] can be used as opts, or crypto.Hash(0) or
// crypto.SHA512 directly to select plain Ed25519 or Ed25519ph, respectively.
public static (slice<byte> signature, error err) Sign(this PrivateKey priv, io.Reader rand, slice<byte> message, crypto.SignerOpts opts) {
    slice<byte> signature = default!;
    error err = default!;

    crypto.Hash hash = opts.HashFunc();
    @string context = ""u8;
    {
        var (optsΔ1, ok) = opts._<Options.val>(ᐧ); if (ok) {
            context = optsΔ1.val.Context;
        }
    }
    switch (ᐧ) {
    case {} when hash is crypto.SHA512: {
        {
            nint l = len(message); if (l != sha512.ΔSize) {
                // Ed25519ph
                return (default!, errors.New("ed25519: bad Ed25519ph message hash length: "u8 + strconv.Itoa(l)));
            }
        }
        {
            nint l = len(context); if (l > 255) {
                return (default!, errors.New("ed25519: bad Ed25519ph context length: "u8 + strconv.Itoa(l)));
            }
        }
        var signatureΔ2 = new slice<byte>(SignatureSize);
        sign(signatureΔ2, priv, message, domPrefixPh, context);
        return (signatureΔ2, default!);
    }
    case {} when hash == ((crypto.Hash)0) && context != ""u8: {
        {
            nint l = len(context); if (l > 255) {
                // Ed25519ctx
                return (default!, errors.New("ed25519: bad Ed25519ctx context length: "u8 + strconv.Itoa(l)));
            }
        }
        var signatureΔ3 = new slice<byte>(SignatureSize);
        sign(signatureΔ3, priv, message, domPrefixCtx, context);
        return (signatureΔ3, default!);
    }
    case {} when hash == ((crypto.Hash)0): {
        return (Sign(priv, // Ed25519
 message), default!);
    }
    default: {
        return (default!, errors.New("ed25519: expected opts.HashFunc() zero (unhashed message, for standard Ed25519) or SHA-512 (for Ed25519ph)"u8));
    }}

}

// Options can be used with [PrivateKey.Sign] or [VerifyWithOptions]
// to select Ed25519 variants.
[GoType] partial struct Options {
    // Hash can be zero for regular Ed25519, or crypto.SHA512 for Ed25519ph.
    public crypto_package.Hash Hash;
    // Context, if not empty, selects Ed25519ctx or provides the context string
    // for Ed25519ph. It can be at most 255 bytes in length.
    public @string Context;
}

// HashFunc returns o.Hash.
[GoRecv] public static crypto.Hash HashFunc(this ref Options o) {
    return o.Hash;
}

// GenerateKey generates a public/private key pair using entropy from rand.
// If rand is nil, [crypto/rand.Reader] will be used.
//
// The output of this function is deterministic, and equivalent to reading
// [SeedSize] bytes from rand, and passing them to [NewKeyFromSeed].
public static (PublicKey, PrivateKey, error) GenerateKey(io.Reader rand) {
    if (rand == default!) {
        rand = cryptorand.Reader;
    }
    var seed = new slice<byte>(SeedSize);
    {
        var (_, err) = io.ReadFull(rand, seed); if (err != default!) {
            return (default!, default!, err);
        }
    }
    var privateKey = NewKeyFromSeed(seed);
    var publicKey = new slice<byte>(PublicKeySize);
    copy(publicKey, privateKey[32..]);
    return (publicKey, privateKey, default!);
}

// NewKeyFromSeed calculates a private key from a seed. It will panic if
// len(seed) is not [SeedSize]. This function is provided for interoperability
// with RFC 8032. RFC 8032's private keys correspond to seeds in this
// package.
public static PrivateKey NewKeyFromSeed(slice<byte> seed) {
    // Outline the function body so that the returned key can be stack-allocated.
    var privateKey = new slice<byte>(PrivateKeySize);
    newKeyFromSeed(privateKey, seed);
    return privateKey;
}

internal static void newKeyFromSeed(slice<byte> privateKey, slice<byte> seed) {
    {
        nint l = len(seed); if (l != SeedSize) {
            throw panic("ed25519: bad seed length: "u8 + strconv.Itoa(l));
        }
    }
    var h = sha512.Sum512(seed);
    (s, err) = edwards25519.NewScalar().SetBytesWithClamping(h[..32]);
    if (err != default!) {
        throw panic("ed25519: internal error: setting scalar failed");
    }
    var A = (Ꮡ(new edwards25519.Point(nil))).ScalarBaseMult(s);
    var publicKey = A.Bytes();
    copy(privateKey, seed);
    copy(privateKey[32..], publicKey);
}

// Sign signs the message with privateKey and returns a signature. It will
// panic if len(privateKey) is not [PrivateKeySize].
public static slice<byte> Sign(PrivateKey privateKey, slice<byte> message) {
    // Outline the function body so that the returned signature can be
    // stack-allocated.
    var signature = new slice<byte>(SignatureSize);
    sign(signature, privateKey, message, domPrefixPure, ""u8);
    return signature;
}

// Domain separation prefixes used to disambiguate Ed25519/Ed25519ph/Ed25519ctx.
// See RFC 8032, Section 2 and Section 5.1.
internal static readonly @string domPrefixPure = ""u8;

internal static readonly @string domPrefixPh = "SigEd25519 no Ed25519 collisions\x01"u8;

internal static readonly @string domPrefixCtx = "SigEd25519 no Ed25519 collisions\x00"u8;

internal static void sign(slice<byte> signature, slice<byte> privateKey, slice<byte> message, @string domPrefix, @string context) {
    {
        nint l = len(privateKey); if (l != PrivateKeySize) {
            throw panic("ed25519: bad private key length: "u8 + strconv.Itoa(l));
        }
    }
    var seed = privateKey[..(int)(SeedSize)];
    var publicKey = privateKey[(int)(SeedSize)..];
    var h = sha512.Sum512(seed);
    (s, err) = edwards25519.NewScalar().SetBytesWithClamping(h[..32]);
    if (err != default!) {
        throw panic("ed25519: internal error: setting scalar failed");
    }
    var prefix = h[32..];
    var mh = sha512.New();
    if (domPrefix != domPrefixPure) {
        mh.Write(slice<byte>(domPrefix));
        mh.Write(new byte[]{((byte)len(context))}.slice());
        mh.Write(slice<byte>(context));
    }
    mh.Write(prefix);
    mh.Write(message);
    var messageDigest = new slice<byte>(0, sha512.ΔSize);
    messageDigest = mh.Sum(messageDigest);
    (r, err) = edwards25519.NewScalar().SetUniformBytes(messageDigest);
    if (err != default!) {
        throw panic("ed25519: internal error: setting scalar failed");
    }
    var R = (Ꮡ(new edwards25519.Point(nil))).ScalarBaseMult(r);
    var kh = sha512.New();
    if (domPrefix != domPrefixPure) {
        kh.Write(slice<byte>(domPrefix));
        kh.Write(new byte[]{((byte)len(context))}.slice());
        kh.Write(slice<byte>(context));
    }
    kh.Write(R.Bytes());
    kh.Write(publicKey);
    kh.Write(message);
    var hramDigest = new slice<byte>(0, sha512.ΔSize);
    hramDigest = kh.Sum(hramDigest);
    (k, err) = edwards25519.NewScalar().SetUniformBytes(hramDigest);
    if (err != default!) {
        throw panic("ed25519: internal error: setting scalar failed");
    }
    var S = edwards25519.NewScalar().MultiplyAdd(k, s, r);
    copy(signature[..32], R.Bytes());
    copy(signature[32..], S.Bytes());
}

// Verify reports whether sig is a valid signature of message by publicKey. It
// will panic if len(publicKey) is not [PublicKeySize].
//
// The inputs are not considered confidential, and may leak through timing side
// channels, or if an attacker has control of part of the inputs.
public static bool Verify(PublicKey publicKey, slice<byte> message, slice<byte> sig) {
    return verify(publicKey, message, sig, domPrefixPure, ""u8);
}

// VerifyWithOptions reports whether sig is a valid signature of message by
// publicKey. A valid signature is indicated by returning a nil error. It will
// panic if len(publicKey) is not [PublicKeySize].
//
// If opts.Hash is [crypto.SHA512], the pre-hashed variant Ed25519ph is used and
// message is expected to be a SHA-512 hash, otherwise opts.Hash must be
// [crypto.Hash](0) and the message must not be hashed, as Ed25519 performs two
// passes over messages to be signed.
//
// The inputs are not considered confidential, and may leak through timing side
// channels, or if an attacker has control of part of the inputs.
public static error VerifyWithOptions(PublicKey publicKey, slice<byte> message, slice<byte> sig, ж<Options> Ꮡopts) {
    ref var opts = ref Ꮡopts.val;

    switch (ᐧ) {
    case {} when opts.Hash is crypto.SHA512: {
        {
            nint l = len(message); if (l != sha512.ΔSize) {
                // Ed25519ph
                return errors.New("ed25519: bad Ed25519ph message hash length: "u8 + strconv.Itoa(l));
            }
        }
        {
            nint l = len(opts.Context); if (l > 255) {
                return errors.New("ed25519: bad Ed25519ph context length: "u8 + strconv.Itoa(l));
            }
        }
        if (!verify(publicKey, message, sig, domPrefixPh, opts.Context)) {
            return errors.New("ed25519: invalid signature"u8);
        }
        return default!;
    }
    case {} when opts.Hash == ((crypto.Hash)0) && opts.Context != ""u8: {
        {
            nint l = len(opts.Context); if (l > 255) {
                // Ed25519ctx
                return errors.New("ed25519: bad Ed25519ctx context length: "u8 + strconv.Itoa(l));
            }
        }
        if (!verify(publicKey, message, sig, domPrefixCtx, opts.Context)) {
            return errors.New("ed25519: invalid signature"u8);
        }
        return default!;
    }
    case {} when opts.Hash == ((crypto.Hash)0): {
        if (!verify(publicKey, // Ed25519
 message, sig, domPrefixPure, ""u8)) {
            return errors.New("ed25519: invalid signature"u8);
        }
        return default!;
    }
    default: {
        return errors.New("ed25519: expected opts.Hash zero (unhashed message, for standard Ed25519) or SHA-512 (for Ed25519ph)"u8);
    }}

}

internal static bool verify(PublicKey publicKey, slice<byte> message, slice<byte> sig, @string domPrefix, @string context) {
    {
        nint l = len(publicKey); if (l != PublicKeySize) {
            throw panic("ed25519: bad public key length: "u8 + strconv.Itoa(l));
        }
    }
    if (len(sig) != SignatureSize || (byte)(sig[63] & 224) != 0) {
        return false;
    }
    (A, err) = (Ꮡ(new edwards25519.Point(nil))).SetBytes(publicKey);
    if (err != default!) {
        return false;
    }
    var kh = sha512.New();
    if (domPrefix != domPrefixPure) {
        kh.Write(slice<byte>(domPrefix));
        kh.Write(new byte[]{((byte)len(context))}.slice());
        kh.Write(slice<byte>(context));
    }
    kh.Write(sig[..32]);
    kh.Write(publicKey);
    kh.Write(message);
    var hramDigest = new slice<byte>(0, sha512.ΔSize);
    hramDigest = kh.Sum(hramDigest);
    (k, err) = edwards25519.NewScalar().SetUniformBytes(hramDigest);
    if (err != default!) {
        throw panic("ed25519: internal error: setting scalar failed");
    }
    (S, err) = edwards25519.NewScalar().SetCanonicalBytes(sig[32..]);
    if (err != default!) {
        return false;
    }
    // [S]B = R + [k]A --> [k](-A) + [S]B = R
    var minusA = (Ꮡ(new edwards25519.Point(nil))).Negate(A);
    var R = (Ꮡ(new edwards25519.Point(nil))).VarTimeDoubleScalarBaseMult(k, minusA, S);
    return bytes.Equal(sig[..32], R.Bytes());
}

} // end ed25519_package
