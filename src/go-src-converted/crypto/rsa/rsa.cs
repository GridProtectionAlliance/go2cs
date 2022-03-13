// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package rsa implements RSA encryption as specified in PKCS #1 and RFC 8017.
//
// RSA is a single, fundamental operation that is used in this package to
// implement either public-key encryption or public-key signatures.
//
// The original specification for encryption and signatures with RSA is PKCS #1
// and the terms "RSA encryption" and "RSA signatures" by default refer to
// PKCS #1 version 1.5. However, that specification has flaws and new designs
// should use version 2, usually called by just OAEP and PSS, where
// possible.
//
// Two sets of interfaces are included in this package. When a more abstract
// interface isn't necessary, there are functions for encrypting/decrypting
// with v1.5/OAEP and signing/verifying with v1.5/PSS. If one needs to abstract
// over the public key primitive, the PrivateKey type implements the
// Decrypter and Signer interfaces from the crypto package.
//
// The RSA operations in this package are not implemented using constant-time algorithms.

// package rsa -- go2cs converted at 2022 March 13 05:32:31 UTC
// import "crypto/rsa" ==> using rsa = go.crypto.rsa_package
// Original source: C:\Program Files\Go\src\crypto\rsa\rsa.go
namespace go.crypto;

using crypto = crypto_package;
using rand = crypto.rand_package;
using subtle = crypto.subtle_package;
using errors = errors_package;
using hash = hash_package;
using io = io_package;
using math = math_package;
using big = math.big_package;

using randutil = crypto.@internal.randutil_package;

public static partial class rsa_package {

private static var bigZero = big.NewInt(0);
private static var bigOne = big.NewInt(1);

// A PublicKey represents the public part of an RSA key.
public partial struct PublicKey {
    public ptr<big.Int> N; // modulus
    public nint E; // public exponent
}

// Any methods implemented on PublicKey might need to also be implemented on
// PrivateKey, as the latter embeds the former and will expose its methods.

// Size returns the modulus size in bytes. Raw signatures and ciphertexts
// for or by this public key will have the same size.
private static nint Size(this ptr<PublicKey> _addr_pub) {
    ref PublicKey pub = ref _addr_pub.val;

    return (pub.N.BitLen() + 7) / 8;
}

// Equal reports whether pub and x have the same value.
private static bool Equal(this ptr<PublicKey> _addr_pub, crypto.PublicKey x) {
    ref PublicKey pub = ref _addr_pub.val;

    ptr<PublicKey> (xx, ok) = x._<ptr<PublicKey>>();
    if (!ok) {
        return false;
    }
    return pub.N.Cmp(xx.N) == 0 && pub.E == xx.E;
}

// OAEPOptions is an interface for passing options to OAEP decryption using the
// crypto.Decrypter interface.
public partial struct OAEPOptions {
    public crypto.Hash Hash; // Label is an arbitrary byte string that must be equal to the value
// used when encrypting.
    public slice<byte> Label;
}

private static var errPublicModulus = errors.New("crypto/rsa: missing public modulus");private static var errPublicExponentSmall = errors.New("crypto/rsa: public exponent too small");private static var errPublicExponentLarge = errors.New("crypto/rsa: public exponent too large");

// checkPub sanity checks the public key before we use it.
// We require pub.E to fit into a 32-bit integer so that we
// do not have different behavior depending on whether
// int is 32 or 64 bits. See also
// https://www.imperialviolet.org/2012/03/16/rsae.html.
private static error checkPub(ptr<PublicKey> _addr_pub) {
    ref PublicKey pub = ref _addr_pub.val;

    if (pub.N == null) {
        return error.As(errPublicModulus)!;
    }
    if (pub.E < 2) {
        return error.As(errPublicExponentSmall)!;
    }
    if (pub.E > 1 << 31 - 1) {
        return error.As(errPublicExponentLarge)!;
    }
    return error.As(null!)!;
}

// A PrivateKey represents an RSA key
public partial struct PrivateKey : PublicKey {
    public PublicKey PublicKey; // public part.
    public ptr<big.Int> D; // private exponent
    public slice<ptr<big.Int>> Primes; // prime factors of N, has >= 2 elements.

// Precomputed contains precomputed values that speed up private
// operations, if available.
    public PrecomputedValues Precomputed;
}

// Public returns the public key corresponding to priv.
private static crypto.PublicKey Public(this ptr<PrivateKey> _addr_priv) {
    ref PrivateKey priv = ref _addr_priv.val;

    return _addr_priv.PublicKey;
}

// Equal reports whether priv and x have equivalent values. It ignores
// Precomputed values.
private static bool Equal(this ptr<PrivateKey> _addr_priv, crypto.PrivateKey x) {
    ref PrivateKey priv = ref _addr_priv.val;

    ptr<PrivateKey> (xx, ok) = x._<ptr<PrivateKey>>();
    if (!ok) {
        return false;
    }
    if (!priv.PublicKey.Equal(_addr_xx.PublicKey) || priv.D.Cmp(xx.D) != 0) {
        return false;
    }
    if (len(priv.Primes) != len(xx.Primes)) {
        return false;
    }
    foreach (var (i) in priv.Primes) {
        if (priv.Primes[i].Cmp(xx.Primes[i]) != 0) {
            return false;
        }
    }    return true;
}

// Sign signs digest with priv, reading randomness from rand. If opts is a
// *PSSOptions then the PSS algorithm will be used, otherwise PKCS #1 v1.5 will
// be used. digest must be the result of hashing the input message using
// opts.HashFunc().
//
// This method implements crypto.Signer, which is an interface to support keys
// where the private part is kept in, for example, a hardware module. Common
// uses should use the Sign* functions in this package directly.
private static (slice<byte>, error) Sign(this ptr<PrivateKey> _addr_priv, io.Reader rand, slice<byte> digest, crypto.SignerOpts opts) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref PrivateKey priv = ref _addr_priv.val;

    {
        ptr<PSSOptions> (pssOpts, ok) = opts._<ptr<PSSOptions>>();

        if (ok) {
            return SignPSS(rand, priv, pssOpts.Hash, digest, pssOpts);
        }
    }

    return SignPKCS1v15(rand, priv, opts.HashFunc(), digest);
}

// Decrypt decrypts ciphertext with priv. If opts is nil or of type
// *PKCS1v15DecryptOptions then PKCS #1 v1.5 decryption is performed. Otherwise
// opts must have type *OAEPOptions and OAEP decryption is done.
private static (slice<byte>, error) Decrypt(this ptr<PrivateKey> _addr_priv, io.Reader rand, slice<byte> ciphertext, crypto.DecrypterOpts opts) {
    slice<byte> plaintext = default;
    error err = default!;
    ref PrivateKey priv = ref _addr_priv.val;

    if (opts == null) {
        return DecryptPKCS1v15(rand, priv, ciphertext);
    }
    switch (opts.type()) {
        case ptr<OAEPOptions> opts:
            return DecryptOAEP(opts.Hash.New(), rand, _addr_priv, ciphertext, opts.Label);
            break;
        case ptr<PKCS1v15DecryptOptions> opts:
            {
                var l = opts.SessionKeyLen;

                if (l > 0) {
                    plaintext = make_slice<byte>(l);
                    {
                        var (_, err) = io.ReadFull(rand, plaintext);

                        if (err != null) {
                            return (null, error.As(err)!);
                        }

                    }
                    {
                        var err = DecryptPKCS1v15SessionKey(rand, priv, ciphertext, plaintext);

                        if (err != null) {
                            return (null, error.As(err)!);
                        }

                    }
                    return (plaintext, error.As(null!)!);
                }
                else
 {
                    return DecryptPKCS1v15(rand, priv, ciphertext);
                }

            }
            break;
        default:
        {
            var opts = opts.type();
            return (null, error.As(errors.New("crypto/rsa: invalid options for Decrypt"))!);
            break;
        }
    }
}

public partial struct PrecomputedValues {
    public ptr<big.Int> Dp; // D mod (P-1) (or mod Q-1)
    public ptr<big.Int> Dq; // D mod (P-1) (or mod Q-1)
    public ptr<big.Int> Qinv; // Q^-1 mod P

// CRTValues is used for the 3rd and subsequent primes. Due to a
// historical accident, the CRT for the first two primes is handled
// differently in PKCS #1 and interoperability is sufficiently
// important that we mirror this.
    public slice<CRTValue> CRTValues;
}

// CRTValue contains the precomputed Chinese remainder theorem values.
public partial struct CRTValue {
    public ptr<big.Int> Exp; // D mod (prime-1).
    public ptr<big.Int> Coeff; // R·Coeff ≡ 1 mod Prime.
    public ptr<big.Int> R; // product of primes prior to this (inc p and q).
}

// Validate performs basic sanity checks on the key.
// It returns nil if the key is valid, or else an error describing a problem.
private static error Validate(this ptr<PrivateKey> _addr_priv) {
    ref PrivateKey priv = ref _addr_priv.val;

    {
        var err = checkPub(_addr_priv.PublicKey);

        if (err != null) {
            return error.As(err)!;
        }
    } 

    // Check that Πprimes == n.
    ptr<big.Int> modulus = @new<big.Int>().Set(bigOne);
    {
        var prime__prev1 = prime;

        foreach (var (_, __prime) in priv.Primes) {
            prime = __prime; 
            // Any primes ≤ 1 will cause divide-by-zero panics later.
            if (prime.Cmp(bigOne) <= 0) {
                return error.As(errors.New("crypto/rsa: invalid prime value"))!;
            }
            modulus.Mul(modulus, prime);
        }
        prime = prime__prev1;
    }

    if (modulus.Cmp(priv.N) != 0) {
        return error.As(errors.New("crypto/rsa: invalid modulus"))!;
    }
    ptr<big.Int> congruence = @new<big.Int>();
    ptr<big.Int> de = @new<big.Int>().SetInt64(int64(priv.E));
    de.Mul(de, priv.D);
    {
        var prime__prev1 = prime;

        foreach (var (_, __prime) in priv.Primes) {
            prime = __prime;
            ptr<big.Int> pminus1 = @new<big.Int>().Sub(prime, bigOne);
            congruence.Mod(de, pminus1);
            if (congruence.Cmp(bigOne) != 0) {
                return error.As(errors.New("crypto/rsa: invalid exponents"))!;
            }
        }
        prime = prime__prev1;
    }

    return error.As(null!)!;
}

// GenerateKey generates an RSA keypair of the given bit size using the
// random source random (for example, crypto/rand.Reader).
public static (ptr<PrivateKey>, error) GenerateKey(io.Reader random, nint bits) {
    ptr<PrivateKey> _p0 = default!;
    error _p0 = default!;

    return _addr_GenerateMultiPrimeKey(random, 2, bits)!;
}

// GenerateMultiPrimeKey generates a multi-prime RSA keypair of the given bit
// size and the given random source, as suggested in [1]. Although the public
// keys are compatible (actually, indistinguishable) from the 2-prime case,
// the private keys are not. Thus it may not be possible to export multi-prime
// private keys in certain formats or to subsequently import them into other
// code.
//
// Table 1 in [2] suggests maximum numbers of primes for a given size.
//
// [1] US patent 4405829 (1972, expired)
// [2] http://www.cacr.math.uwaterloo.ca/techreports/2006/cacr2006-16.pdf
public static (ptr<PrivateKey>, error) GenerateMultiPrimeKey(io.Reader random, nint nprimes, nint bits) {
    ptr<PrivateKey> _p0 = default!;
    error _p0 = default!;

    randutil.MaybeReadByte(random);

    ptr<PrivateKey> priv = @new<PrivateKey>();
    priv.E = 65537;

    if (nprimes < 2) {
        return (_addr_null!, error.As(errors.New("crypto/rsa: GenerateMultiPrimeKey: nprimes must be >= 2"))!);
    }
    if (bits < 64) {
        var primeLimit = float64(uint64(1) << (int)(uint(bits / nprimes))); 
        // pi approximates the number of primes less than primeLimit
        var pi = primeLimit / (math.Log(primeLimit) - 1); 
        // Generated primes start with 11 (in binary) so we can only
        // use a quarter of them.
        pi /= 4; 
        // Use a factor of two to ensure that key generation terminates
        // in a reasonable amount of time.
        pi /= 2;
        if (pi <= float64(nprimes)) {
            return (_addr_null!, error.As(errors.New("crypto/rsa: too few primes of given length to generate an RSA key"))!);
        }
    }
    var primes = make_slice<ptr<big.Int>>(nprimes);

NextSetOfPrimes:

    while (true) {
        var todo = bits; 
        // crypto/rand should set the top two bits in each prime.
        // Thus each prime has the form
        //   p_i = 2^bitlen(p_i) × 0.11... (in base 2).
        // And the product is:
        //   P = 2^todo × α
        // where α is the product of nprimes numbers of the form 0.11...
        //
        // If α < 1/2 (which can happen for nprimes > 2), we need to
        // shift todo to compensate for lost bits: the mean value of 0.11...
        // is 7/8, so todo + shift - nprimes * log2(7/8) ~= bits - 1/2
        // will give good results.
        if (nprimes >= 7) {
            todo += (nprimes - 2) / 5;
        }
        {
            nint i__prev2 = i;

            for (nint i = 0; i < nprimes; i++) {
                error err = default!;
                primes[i], err = rand.Prime(random, todo / (nprimes - i));
                if (err != null) {
                    return (_addr_null!, error.As(err)!);
                }
                todo -= primes[i].BitLen();
            } 

            // Make sure that primes is pairwise unequal.


            i = i__prev2;
        } 

        // Make sure that primes is pairwise unequal.
        {
            nint i__prev2 = i;
            var prime__prev2 = prime;

            foreach (var (__i, __prime) in primes) {
                i = __i;
                prime = __prime;
                for (nint j = 0; j < i; j++) {
                    if (prime.Cmp(primes[j]) == 0) {
                        _continueNextSetOfPrimes = true;
                        break;
                    }
                }
            }

            i = i__prev2;
            prime = prime__prev2;
        }

        ptr<big.Int> n = @new<big.Int>().Set(bigOne);
        ptr<big.Int> totient = @new<big.Int>().Set(bigOne);
        ptr<big.Int> pminus1 = @new<big.Int>();
        {
            var prime__prev2 = prime;

            foreach (var (_, __prime) in primes) {
                prime = __prime;
                n.Mul(n, prime);
                pminus1.Sub(prime, bigOne);
                totient.Mul(totient, pminus1);
            }

            prime = prime__prev2;
        }

        if (n.BitLen() != bits) { 
            // This should never happen for nprimes == 2 because
            // crypto/rand should set the top two bits in each prime.
            // For nprimes > 2 we hope it does not happen often.
            _continueNextSetOfPrimes = true;
            break;
        }
        priv.D = @new<big.Int>();
        var e = big.NewInt(int64(priv.E));
        var ok = priv.D.ModInverse(e, totient);

        if (ok != null) {
            priv.Primes = primes;
            priv.N = n;
            break;
        }
    }
    priv.Precompute();
    return (_addr_priv!, error.As(null!)!);
}

// incCounter increments a four byte, big-endian counter.
private static void incCounter(ptr<array<byte>> _addr_c) {
    ref array<byte> c = ref _addr_c.val;

    c[3]++;

    if (c[3] != 0) {
        return ;
    }
    c[2]++;

    if (c[2] != 0) {
        return ;
    }
    c[1]++;

    if (c[1] != 0) {
        return ;
    }
    c[0]++;
}

// mgf1XOR XORs the bytes in out with a mask generated using the MGF1 function
// specified in PKCS #1 v2.1.
private static void mgf1XOR(slice<byte> @out, hash.Hash hash, slice<byte> seed) {
    ref array<byte> counter = ref heap(new array<byte>(4), out ptr<array<byte>> _addr_counter);
    slice<byte> digest = default;

    nint done = 0;
    while (done < len(out)) {
        hash.Write(seed);
        hash.Write(counter[(int)0..(int)4]);
        digest = hash.Sum(digest[..(int)0]);
        hash.Reset();

        for (nint i = 0; i < len(digest) && done < len(out); i++) {
            out[done] ^= digest[i];
            done++;
        }
        incCounter(_addr_counter);
    }
}

// ErrMessageTooLong is returned when attempting to encrypt a message which is
// too large for the size of the public key.
public static var ErrMessageTooLong = errors.New("crypto/rsa: message too long for RSA public key size");

private static ptr<big.Int> encrypt(ptr<big.Int> _addr_c, ptr<PublicKey> _addr_pub, ptr<big.Int> _addr_m) {
    ref big.Int c = ref _addr_c.val;
    ref PublicKey pub = ref _addr_pub.val;
    ref big.Int m = ref _addr_m.val;

    var e = big.NewInt(int64(pub.E));
    c.Exp(m, e, pub.N);
    return _addr_c!;
}

// EncryptOAEP encrypts the given message with RSA-OAEP.
//
// OAEP is parameterised by a hash function that is used as a random oracle.
// Encryption and decryption of a given message must use the same hash function
// and sha256.New() is a reasonable choice.
//
// The random parameter is used as a source of entropy to ensure that
// encrypting the same message twice doesn't result in the same ciphertext.
//
// The label parameter may contain arbitrary data that will not be encrypted,
// but which gives important context to the message. For example, if a given
// public key is used to encrypt two types of messages then distinct label
// values could be used to ensure that a ciphertext for one purpose cannot be
// used for another by an attacker. If not required it can be empty.
//
// The message must be no longer than the length of the public modulus minus
// twice the hash length, minus a further 2.
public static (slice<byte>, error) EncryptOAEP(hash.Hash hash, io.Reader random, ptr<PublicKey> _addr_pub, slice<byte> msg, slice<byte> label) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref PublicKey pub = ref _addr_pub.val;

    {
        var err = checkPub(_addr_pub);

        if (err != null) {
            return (null, error.As(err)!);
        }
    }
    hash.Reset();
    var k = pub.Size();
    if (len(msg) > k - 2 * hash.Size() - 2) {
        return (null, error.As(ErrMessageTooLong)!);
    }
    hash.Write(label);
    var lHash = hash.Sum(null);
    hash.Reset();

    var em = make_slice<byte>(k);
    var seed = em[(int)1..(int)1 + hash.Size()];
    var db = em[(int)1 + hash.Size()..];

    copy(db[(int)0..(int)hash.Size()], lHash);
    db[len(db) - len(msg) - 1] = 1;
    copy(db[(int)len(db) - len(msg)..], msg);

    var (_, err) = io.ReadFull(random, seed);
    if (err != null) {
        return (null, error.As(err)!);
    }
    mgf1XOR(db, hash, seed);
    mgf1XOR(seed, hash, db);

    ptr<big.Int> m = @new<big.Int>();
    m.SetBytes(em);
    var c = encrypt(@new<big.Int>(), _addr_pub, m);

    var @out = make_slice<byte>(k);
    return (c.FillBytes(out), error.As(null!)!);
}

// ErrDecryption represents a failure to decrypt a message.
// It is deliberately vague to avoid adaptive attacks.
public static var ErrDecryption = errors.New("crypto/rsa: decryption error");

// ErrVerification represents a failure to verify a signature.
// It is deliberately vague to avoid adaptive attacks.
public static var ErrVerification = errors.New("crypto/rsa: verification error");

// Precompute performs some calculations that speed up private key operations
// in the future.
private static void Precompute(this ptr<PrivateKey> _addr_priv) {
    ref PrivateKey priv = ref _addr_priv.val;

    if (priv.Precomputed.Dp != null) {
        return ;
    }
    priv.Precomputed.Dp = @new<big.Int>().Sub(priv.Primes[0], bigOne);
    priv.Precomputed.Dp.Mod(priv.D, priv.Precomputed.Dp);

    priv.Precomputed.Dq = @new<big.Int>().Sub(priv.Primes[1], bigOne);
    priv.Precomputed.Dq.Mod(priv.D, priv.Precomputed.Dq);

    priv.Precomputed.Qinv = @new<big.Int>().ModInverse(priv.Primes[1], priv.Primes[0]);

    ptr<big.Int> r = @new<big.Int>().Mul(priv.Primes[0], priv.Primes[1]);
    priv.Precomputed.CRTValues = make_slice<CRTValue>(len(priv.Primes) - 2);
    for (nint i = 2; i < len(priv.Primes); i++) {
        var prime = priv.Primes[i];
        var values = _addr_priv.Precomputed.CRTValues[i - 2];

        values.Exp = @new<big.Int>().Sub(prime, bigOne);
        values.Exp.Mod(priv.D, values.Exp);

        values.R = @new<big.Int>().Set(r);
        values.Coeff = @new<big.Int>().ModInverse(r, prime);

        r.Mul(r, prime);
    }
}

// decrypt performs an RSA decryption, resulting in a plaintext integer. If a
// random source is given, RSA blinding is used.
private static (ptr<big.Int>, error) decrypt(io.Reader random, ptr<PrivateKey> _addr_priv, ptr<big.Int> _addr_c) {
    ptr<big.Int> m = default!;
    error err = default!;
    ref PrivateKey priv = ref _addr_priv.val;
    ref big.Int c = ref _addr_c.val;
 
    // TODO(agl): can we get away with reusing blinds?
    if (c.Cmp(priv.N) > 0) {
        err = ErrDecryption;
        return ;
    }
    if (priv.N.Sign() == 0) {
        return (_addr_null!, error.As(ErrDecryption)!);
    }
    ptr<big.Int> ir;
    if (random != null) {
        randutil.MaybeReadByte(random); 

        // Blinding enabled. Blinding involves multiplying c by r^e.
        // Then the decryption operation performs (m^e * r^e)^d mod n
        // which equals mr mod n. The factor of r can then be removed
        // by multiplying by the multiplicative inverse of r.

        ptr<big.Int> r;
        ir = @new<big.Int>();
        while (true) {
            r, err = rand.Int(random, priv.N);
            if (err != null) {
                return ;
            }
            if (r.Cmp(bigZero) == 0) {
                r = bigOne;
            }
            var ok = ir.ModInverse(r, priv.N);
            if (ok != null) {
                break;
            }
        }
        var bigE = big.NewInt(int64(priv.E));
        ptr<big.Int> rpowe = @new<big.Int>().Exp(r, bigE, priv.N); // N != 0
        ptr<big.Int> cCopy = @new<big.Int>().Set(c);
        cCopy.Mul(cCopy, rpowe);
        cCopy.Mod(cCopy, priv.N);
        c = cCopy;
    }
    if (priv.Precomputed.Dp == null) {
        m = @new<big.Int>().Exp(c, priv.D, priv.N);
    }
    else
 { 
        // We have the precalculated values needed for the CRT.
        m = @new<big.Int>().Exp(c, priv.Precomputed.Dp, priv.Primes[0]);
        ptr<big.Int> m2 = @new<big.Int>().Exp(c, priv.Precomputed.Dq, priv.Primes[1]);
        m.Sub(m, m2);
        if (m.Sign() < 0) {
            m.Add(m, priv.Primes[0]);
        }
        m.Mul(m, priv.Precomputed.Qinv);
        m.Mod(m, priv.Primes[0]);
        m.Mul(m, priv.Primes[1]);
        m.Add(m, m2);

        foreach (var (i, values) in priv.Precomputed.CRTValues) {
            var prime = priv.Primes[2 + i];
            m2.Exp(c, values.Exp, prime);
            m2.Sub(m2, m);
            m2.Mul(m2, values.Coeff);
            m2.Mod(m2, prime);
            if (m2.Sign() < 0) {
                m2.Add(m2, prime);
            }
            m2.Mul(m2, values.R);
            m.Add(m, m2);
        }
    }
    if (ir != null) { 
        // Unblind.
        m.Mul(m, ir);
        m.Mod(m, priv.N);
    }
    return ;
}

private static (ptr<big.Int>, error) decryptAndCheck(io.Reader random, ptr<PrivateKey> _addr_priv, ptr<big.Int> _addr_c) {
    ptr<big.Int> m = default!;
    error err = default!;
    ref PrivateKey priv = ref _addr_priv.val;
    ref big.Int c = ref _addr_c.val;

    m, err = decrypt(random, _addr_priv, _addr_c);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    var check = encrypt(@new<big.Int>(), _addr_priv.PublicKey, _addr_m);
    if (c.Cmp(check) != 0) {
        return (_addr_null!, error.As(errors.New("rsa: internal error"))!);
    }
    return (_addr_m!, error.As(null!)!);
}

// DecryptOAEP decrypts ciphertext using RSA-OAEP.
//
// OAEP is parameterised by a hash function that is used as a random oracle.
// Encryption and decryption of a given message must use the same hash function
// and sha256.New() is a reasonable choice.
//
// The random parameter, if not nil, is used to blind the private-key operation
// and avoid timing side-channel attacks. Blinding is purely internal to this
// function – the random data need not match that used when encrypting.
//
// The label parameter must match the value given when encrypting. See
// EncryptOAEP for details.
public static (slice<byte>, error) DecryptOAEP(hash.Hash hash, io.Reader random, ptr<PrivateKey> _addr_priv, slice<byte> ciphertext, slice<byte> label) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref PrivateKey priv = ref _addr_priv.val;

    {
        var err = checkPub(_addr_priv.PublicKey);

        if (err != null) {
            return (null, error.As(err)!);
        }
    }
    var k = priv.Size();
    if (len(ciphertext) > k || k < hash.Size() * 2 + 2) {
        return (null, error.As(ErrDecryption)!);
    }
    ptr<big.Int> c = @new<big.Int>().SetBytes(ciphertext);

    var (m, err) = decrypt(random, _addr_priv, c);
    if (err != null) {
        return (null, error.As(err)!);
    }
    hash.Write(label);
    var lHash = hash.Sum(null);
    hash.Reset(); 

    // We probably leak the number of leading zeros.
    // It's not clear that we can do anything about this.
    var em = m.FillBytes(make_slice<byte>(k));

    var firstByteIsZero = subtle.ConstantTimeByteEq(em[0], 0);

    var seed = em[(int)1..(int)hash.Size() + 1];
    var db = em[(int)hash.Size() + 1..];

    mgf1XOR(seed, hash, db);
    mgf1XOR(db, hash, seed);

    var lHash2 = db[(int)0..(int)hash.Size()]; 

    // We have to validate the plaintext in constant time in order to avoid
    // attacks like: J. Manger. A Chosen Ciphertext Attack on RSA Optimal
    // Asymmetric Encryption Padding (OAEP) as Standardized in PKCS #1
    // v2.0. In J. Kilian, editor, Advances in Cryptology.
    var lHash2Good = subtle.ConstantTimeCompare(lHash, lHash2); 

    // The remainder of the plaintext must be zero or more 0x00, followed
    // by 0x01, followed by the message.
    //   lookingForIndex: 1 iff we are still looking for the 0x01
    //   index: the offset of the first 0x01 byte
    //   invalid: 1 iff we saw a non-zero byte before the 0x01.
    nint lookingForIndex = default;    nint index = default;    nint invalid = default;

    lookingForIndex = 1;
    var rest = db[(int)hash.Size()..];

    for (nint i = 0; i < len(rest); i++) {
        var equals0 = subtle.ConstantTimeByteEq(rest[i], 0);
        var equals1 = subtle.ConstantTimeByteEq(rest[i], 1);
        index = subtle.ConstantTimeSelect(lookingForIndex & equals1, i, index);
        lookingForIndex = subtle.ConstantTimeSelect(equals1, 0, lookingForIndex);
        invalid = subtle.ConstantTimeSelect(lookingForIndex & ~equals0, 1, invalid);
    }

    if (firstByteIsZero & lHash2Good & ~invalid & ~lookingForIndex != 1) {
        return (null, error.As(ErrDecryption)!);
    }
    return (rest[(int)index + 1..], error.As(null!)!);
}

} // end rsa_package
