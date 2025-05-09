// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package dsa implements the Digital Signature Algorithm, as defined in FIPS 186-3.
//
// The DSA operations in this package are not implemented using constant-time algorithms.
//
// Deprecated: DSA is a legacy algorithm, and modern alternatives such as
// Ed25519 (implemented by package crypto/ed25519) should be used instead. Keys
// with 1024-bit moduli (L1024N160 parameters) are cryptographically weak, while
// bigger keys are not widely supported. Note that FIPS 186-5 no longer approves
// DSA for signature generation.
namespace go.crypto;

using errors = errors_package;
using io = io_package;
using big = math.big_package;
using randutil = crypto.@internal.randutil_package;
using crypto.@internal;
using math;

partial class dsa_package {

// Parameters represents the domain parameters for a key. These parameters can
// be shared across many keys. The bit length of Q must be a multiple of 8.
[GoType] partial struct Parameters {
    public ж<math.big_package.ΔInt> P;
    public ж<math.big_package.ΔInt> Q;
    public ж<math.big_package.ΔInt> G;
}

// PublicKey represents a DSA public key.
[GoType] partial struct PublicKey {
    public partial ref Parameters Parameters { get; }
    public ж<math.big_package.ΔInt> Y;
}

// PrivateKey represents a DSA private key.
[GoType] partial struct PrivateKey {
    public partial ref PublicKey PublicKey { get; }
    public ж<math.big_package.ΔInt> X;
}

// ErrInvalidPublicKey results when a public key is not usable by this code.
// FIPS is quite strict about the format of DSA keys, but other code may be
// less so. Thus, when using keys which may have been generated by other code,
// this error must be handled.
public static error ErrInvalidPublicKey = errors.New("crypto/dsa: invalid public key"u8);

[GoType("num:nint")] partial struct ParameterSizes;

public static readonly ParameterSizes L1024N160 = /* iota */ 0;
public static readonly ParameterSizes L2048N224 = 1;
public static readonly ParameterSizes L2048N256 = 2;
public static readonly ParameterSizes L3072N256 = 3;

// numMRTests is the number of Miller-Rabin primality tests that we perform. We
// pick the largest recommended number from table C.1 of FIPS 186-3.
internal static readonly UntypedInt numMRTests = 64;

// GenerateParameters puts a random, valid set of DSA parameters into params.
// This function can take many seconds, even on fast machines.
public static error GenerateParameters(ж<Parameters> Ꮡparams, io.Reader rand, ParameterSizes sizes) {
    ref var @params = ref Ꮡparams.val;

    // This function doesn't follow FIPS 186-3 exactly in that it doesn't
    // use a verification seed to generate the primes. The verification
    // seed doesn't appear to be exported or used by other code and
    // omitting it makes the code cleaner.
    nint L = default!;
    nint N = default!;
    var exprᴛ1 = sizes;
    if (exprᴛ1 == L1024N160) {
        L = 1024;
        N = 160;
    }
    else if (exprᴛ1 == L2048N224) {
        L = 2048;
        N = 224;
    }
    else if (exprᴛ1 == L2048N256) {
        L = 2048;
        N = 256;
    }
    else if (exprᴛ1 == L3072N256) {
        L = 3072;
        N = 256;
    }
    else { /* default: */
        return errors.New("crypto/dsa: invalid ParameterSizes"u8);
    }

    var qBytes = new slice<byte>(N / 8);
    var pBytes = new slice<byte>(L / 8);
    var q = @new<bigꓸInt>();
    var p = @new<bigꓸInt>();
    var rem = @new<bigꓸInt>();
    var one = @new<bigꓸInt>();
    one.SetInt64(1);
GeneratePrimes:
    while (ᐧ) {
        {
            var (_, err) = io.ReadFull(rand, qBytes); if (err != default!) {
                return err;
            }
        }
        qBytes[len(qBytes) - 1] |= (byte)(1);
        qBytes[0] |= (byte)(128);
        q.SetBytes(qBytes);
        if (!q.ProbablyPrime(numMRTests)) {
            continue;
        }
        for (nint i = 0; i < 4 * L; i++) {
            {
                var (_, err) = io.ReadFull(rand, pBytes); if (err != default!) {
                    return err;
                }
            }
            pBytes[len(pBytes) - 1] |= (byte)(1);
            pBytes[0] |= (byte)(128);
            p.SetBytes(pBytes);
            rem.Mod(p, q);
            rem.Sub(rem, one);
            p.Sub(p, rem);
            if (p.BitLen() < L) {
                continue;
            }
            if (!p.ProbablyPrime(numMRTests)) {
                continue;
            }
            @params.P = p;
            @params.Q = q;
            goto break_GeneratePrimes;
        }
continue_GeneratePrimes:;
    }
break_GeneratePrimes:;
    var h = @new<bigꓸInt>();
    h.SetInt64(2);
    var g = @new<bigꓸInt>();
    var pm1 = @new<bigꓸInt>().Sub(p, one);
    var e = @new<bigꓸInt>().Div(pm1, q);
    while (ᐧ) {
        g.Exp(h, e, p);
        if (g.Cmp(one) == 0) {
            h.Add(h, one);
            continue;
        }
        @params.G = g;
        return default!;
    }
}

// GenerateKey generates a public&private key pair. The Parameters of the
// [PrivateKey] must already be valid (see [GenerateParameters]).
public static error GenerateKey(ж<PrivateKey> Ꮡpriv, io.Reader rand) {
    ref var priv = ref Ꮡpriv.val;

    if (priv.P == nil || priv.Q == nil || priv.G == nil) {
        return errors.New("crypto/dsa: parameters not set up before generating key"u8);
    }
    var x = @new<bigꓸInt>();
    var xBytes = new slice<byte>(priv.Q.BitLen() / 8);
    while (ᐧ) {
        var (_, err) = io.ReadFull(rand, xBytes);
        if (err != default!) {
            return err;
        }
        x.SetBytes(xBytes);
        if (x.Sign() != 0 && x.Cmp(priv.Q) < 0) {
            break;
        }
    }
    priv.X = x;
    priv.Y = @new<bigꓸInt>();
    priv.Y.Exp(priv.G, x, priv.P);
    return default!;
}

// fermatInverse calculates the inverse of k in GF(P) using Fermat's method.
// This has better constant-time properties than Euclid's method (implemented
// in math/big.Int.ModInverse) although math/big itself isn't strictly
// constant-time so it's not perfect.
internal static ж<bigꓸInt> fermatInverse(ж<bigꓸInt> Ꮡk, ж<bigꓸInt> ᏑP) {
    ref var k = ref Ꮡk.val;
    ref var P = ref ᏑP.val;

    var two = big.NewInt(2);
    var pMinus2 = @new<bigꓸInt>().Sub(ᏑP, two);
    return @new<bigꓸInt>().Exp(Ꮡk, pMinus2, ᏑP);
}

// Sign signs an arbitrary length hash (which should be the result of hashing a
// larger message) using the private key, priv. It returns the signature as a
// pair of integers. The security of the private key depends on the entropy of
// rand.
//
// Note that FIPS 186-3 section 4.6 specifies that the hash should be truncated
// to the byte-length of the subgroup. This function does not perform that
// truncation itself.
//
// Be aware that calling Sign with an attacker-controlled [PrivateKey] may
// require an arbitrary amount of CPU.
public static (ж<bigꓸInt> r, ж<bigꓸInt> s, error err) Sign(io.Reader rand, ж<PrivateKey> Ꮡpriv, slice<byte> hash) {
    ж<bigꓸInt> r = default!;
    ж<bigꓸInt> s = default!;
    error err = default!;

    ref var priv = ref Ꮡpriv.val;
    randutil.MaybeReadByte(rand);
    // FIPS 186-3, section 4.6
    nint n = priv.Q.BitLen();
    if (priv.Q.Sign() <= 0 || priv.P.Sign() <= 0 || priv.G.Sign() <= 0 || priv.X.Sign() <= 0 || n % 8 != 0) {
        err = ErrInvalidPublicKey;
        return (r, s, err);
    }
    n >>= (UntypedInt)(3);
    nint attempts = default!;
    for (attempts = 10; attempts > 0; attempts--) {
        var k = @new<bigꓸInt>();
        var buf = new slice<byte>(n);
        while (ᐧ) {
            (_, err) = io.ReadFull(rand, buf);
            if (err != default!) {
                return (r, s, err);
            }
            k.SetBytes(buf);
            // priv.Q must be >= 128 because the test above
            // requires it to be > 0 and that
            //    ceil(log_2(Q)) mod 8 = 0
            // Thus this loop will quickly terminate.
            if (k.Sign() > 0 && k.Cmp(priv.Q) < 0) {
                break;
            }
        }
        var kInv = fermatInverse(k, priv.Q);
        r = @new<bigꓸInt>().Exp(priv.G, k, priv.P);
        r.Mod(r, priv.Q);
        if (r.Sign() == 0) {
            continue;
        }
        var z = k.SetBytes(hash);
        s = @new<bigꓸInt>().Mul(priv.X, r);
        s.Add(s, z);
        s.Mod(s, priv.Q);
        s.Mul(s, kInv);
        s.Mod(s, priv.Q);
        if (s.Sign() != 0) {
            break;
        }
    }
    // Only degenerate private keys will require more than a handful of
    // attempts.
    if (attempts == 0) {
        return (default!, default!, ErrInvalidPublicKey);
    }
    return (r, s, err);
}

// Verify verifies the signature in r, s of hash using the public key, pub. It
// reports whether the signature is valid.
//
// Note that FIPS 186-3 section 4.6 specifies that the hash should be truncated
// to the byte-length of the subgroup. This function does not perform that
// truncation itself.
public static bool Verify(ж<PublicKey> Ꮡpub, slice<byte> hash, ж<bigꓸInt> Ꮡr, ж<bigꓸInt> Ꮡs) {
    ref var pub = ref Ꮡpub.val;
    ref var r = ref Ꮡr.val;
    ref var s = ref Ꮡs.val;

    // FIPS 186-3, section 4.7
    if (pub.P.Sign() == 0) {
        return false;
    }
    if (r.Sign() < 1 || r.Cmp(pub.Q) >= 0) {
        return false;
    }
    if (s.Sign() < 1 || s.Cmp(pub.Q) >= 0) {
        return false;
    }
    var w = @new<bigꓸInt>().ModInverse(Ꮡs, pub.Q);
    if (w == nil) {
        return false;
    }
    nint n = pub.Q.BitLen();
    if (n % 8 != 0) {
        return false;
    }
    var z = @new<bigꓸInt>().SetBytes(hash);
    var u1 = @new<bigꓸInt>().Mul(z, w);
    u1.Mod(u1, pub.Q);
    var u2 = w.Mul(Ꮡr, w);
    u2.Mod(u2, pub.Q);
    var v = u1.Exp(pub.G, u1, pub.P);
    u2.Exp(pub.Y, u2, pub.P);
    v.Mul(v, u2);
    v.Mod(v, pub.P);
    v.Mod(v, pub.Q);
    return v.Cmp(Ꮡr) == 0;
}

} // end dsa_package
