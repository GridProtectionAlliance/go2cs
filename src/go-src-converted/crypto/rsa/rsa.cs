// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package rsa implements RSA encryption as specified in PKCS#1.
//
// RSA is a single, fundamental operation that is used in this package to
// implement either public-key encryption or public-key signatures.
//
// The original specification for encryption and signatures with RSA is PKCS#1
// and the terms "RSA encryption" and "RSA signatures" by default refer to
// PKCS#1 version 1.5. However, that specification has flaws and new designs
// should use version two, usually called by just OAEP and PSS, where
// possible.
//
// Two sets of interfaces are included in this package. When a more abstract
// interface isn't necessary, there are functions for encrypting/decrypting
// with v1.5/OAEP and signing/verifying with v1.5/PSS. If one needs to abstract
// over the public-key primitive, the PrivateKey struct implements the
// Decrypter and Signer interfaces from the crypto package.
//
// The RSA operations in this package are not implemented using constant-time algorithms.
// package rsa -- go2cs converted at 2020 August 29 08:30:59 UTC
// import "crypto/rsa" ==> using rsa = go.crypto.rsa_package
// Original source: C:\Go\src\crypto\rsa\rsa.go
using crypto = go.crypto_package;
using rand = go.crypto.rand_package;
using subtle = go.crypto.subtle_package;
using errors = go.errors_package;
using hash = go.hash_package;
using io = go.io_package;
using math = go.math_package;
using big = go.math.big_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class rsa_package
    {
        private static var bigZero = big.NewInt(0L);
        private static var bigOne = big.NewInt(1L);

        // A PublicKey represents the public part of an RSA key.
        public partial struct PublicKey
        {
            public ptr<big.Int> N; // modulus
            public long E; // public exponent
        }

        // OAEPOptions is an interface for passing options to OAEP decryption using the
        // crypto.Decrypter interface.
        public partial struct OAEPOptions
        {
            public crypto.Hash Hash; // Label is an arbitrary byte string that must be equal to the value
// used when encrypting.
            public slice<byte> Label;
        }

        private static var errPublicModulus = errors.New("crypto/rsa: missing public modulus");        private static var errPublicExponentSmall = errors.New("crypto/rsa: public exponent too small");        private static var errPublicExponentLarge = errors.New("crypto/rsa: public exponent too large");

        // checkPub sanity checks the public key before we use it.
        // We require pub.E to fit into a 32-bit integer so that we
        // do not have different behavior depending on whether
        // int is 32 or 64 bits. See also
        // http://www.imperialviolet.org/2012/03/16/rsae.html.
        private static error checkPub(ref PublicKey pub)
        {
            if (pub.N == null)
            {
                return error.As(errPublicModulus);
            }
            if (pub.E < 2L)
            {
                return error.As(errPublicExponentSmall);
            }
            if (pub.E > 1L << (int)(31L) - 1L)
            {
                return error.As(errPublicExponentLarge);
            }
            return error.As(null);
        }

        // A PrivateKey represents an RSA key
        public partial struct PrivateKey : PublicKey
        {
            public PublicKey PublicKey; // public part.
            public ptr<big.Int> D; // private exponent
            public slice<ref big.Int> Primes; // prime factors of N, has >= 2 elements.

// Precomputed contains precomputed values that speed up private
// operations, if available.
            public PrecomputedValues Precomputed;
        }

        // Public returns the public key corresponding to priv.
        private static crypto.PublicKey Public(this ref PrivateKey priv)
        {
            return ref priv.PublicKey;
        }

        // Sign signs digest with priv, reading randomness from rand. If opts is a
        // *PSSOptions then the PSS algorithm will be used, otherwise PKCS#1 v1.5 will
        // be used.
        //
        // This method implements crypto.Signer, which is an interface to support keys
        // where the private part is kept in, for example, a hardware module. Common
        // uses should use the Sign* functions in this package directly.
        private static (slice<byte>, error) Sign(this ref PrivateKey priv, io.Reader rand, slice<byte> digest, crypto.SignerOpts opts)
        {
            {
                ref PSSOptions (pssOpts, ok) = opts._<ref PSSOptions>();

                if (ok)
                {
                    return SignPSS(rand, priv, pssOpts.Hash, digest, pssOpts);
                }

            }

            return SignPKCS1v15(rand, priv, opts.HashFunc(), digest);
        }

        // Decrypt decrypts ciphertext with priv. If opts is nil or of type
        // *PKCS1v15DecryptOptions then PKCS#1 v1.5 decryption is performed. Otherwise
        // opts must have type *OAEPOptions and OAEP decryption is done.
        private static (slice<byte>, error) Decrypt(this ref PrivateKey priv, io.Reader rand, slice<byte> ciphertext, crypto.DecrypterOpts opts)
        {
            if (opts == null)
            {
                return DecryptPKCS1v15(rand, priv, ciphertext);
            }
            switch (opts.type())
            {
                case ref OAEPOptions opts:
                    return DecryptOAEP(opts.Hash.New(), rand, priv, ciphertext, opts.Label);
                    break;
                case ref PKCS1v15DecryptOptions opts:
                    {
                        var l = opts.SessionKeyLen;

                        if (l > 0L)
                        {
                            plaintext = make_slice<byte>(l);
                            {
                                var (_, err) = io.ReadFull(rand, plaintext);

                                if (err != null)
                                {
                                    return (null, err);
                                }

                            }
                            {
                                var err = DecryptPKCS1v15SessionKey(rand, priv, ciphertext, plaintext);

                                if (err != null)
                                {
                                    return (null, err);
                                }

                            }
                            return (plaintext, null);
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
                    return (null, errors.New("crypto/rsa: invalid options for Decrypt"));
                    break;
                }
            }
        }

        public partial struct PrecomputedValues
        {
            public ptr<big.Int> Dp; // D mod (P-1) (or mod Q-1)
            public ptr<big.Int> Dq; // D mod (P-1) (or mod Q-1)
            public ptr<big.Int> Qinv; // Q^-1 mod P

// CRTValues is used for the 3rd and subsequent primes. Due to a
// historical accident, the CRT for the first two primes is handled
// differently in PKCS#1 and interoperability is sufficiently
// important that we mirror this.
            public slice<CRTValue> CRTValues;
        }

        // CRTValue contains the precomputed Chinese remainder theorem values.
        public partial struct CRTValue
        {
            public ptr<big.Int> Exp; // D mod (prime-1).
            public ptr<big.Int> Coeff; // R·Coeff ≡ 1 mod Prime.
            public ptr<big.Int> R; // product of primes prior to this (inc p and q).
        }

        // Validate performs basic sanity checks on the key.
        // It returns nil if the key is valid, or else an error describing a problem.
        private static error Validate(this ref PrivateKey priv)
        {
            {
                var err = checkPub(ref priv.PublicKey);

                if (err != null)
                {
                    return error.As(err);
                } 

                // Check that Πprimes == n.

            } 

            // Check that Πprimes == n.
            ptr<big.Int> modulus = @new<big.Int>().Set(bigOne);
            {
                var prime__prev1 = prime;

                foreach (var (_, __prime) in priv.Primes)
                {
                    prime = __prime; 
                    // Any primes ≤ 1 will cause divide-by-zero panics later.
                    if (prime.Cmp(bigOne) <= 0L)
                    {
                        return error.As(errors.New("crypto/rsa: invalid prime value"));
                    }
                    modulus.Mul(modulus, prime);
                }

                prime = prime__prev1;
            }

            if (modulus.Cmp(priv.N) != 0L)
            {
                return error.As(errors.New("crypto/rsa: invalid modulus"));
            } 

            // Check that de ≡ 1 mod p-1, for each prime.
            // This implies that e is coprime to each p-1 as e has a multiplicative
            // inverse. Therefore e is coprime to lcm(p-1,q-1,r-1,...) =
            // exponent(ℤ/nℤ). It also implies that a^de ≡ a mod p as a^(p-1) ≡ 1
            // mod p. Thus a^de ≡ a mod n for all a coprime to n, as required.
            ptr<big.Int> congruence = @new<big.Int>();
            ptr<big.Int> de = @new<big.Int>().SetInt64(int64(priv.E));
            de.Mul(de, priv.D);
            {
                var prime__prev1 = prime;

                foreach (var (_, __prime) in priv.Primes)
                {
                    prime = __prime;
                    ptr<big.Int> pminus1 = @new<big.Int>().Sub(prime, bigOne);
                    congruence.Mod(de, pminus1);
                    if (congruence.Cmp(bigOne) != 0L)
                    {
                        return error.As(errors.New("crypto/rsa: invalid exponents"));
                    }
                }

                prime = prime__prev1;
            }

            return error.As(null);
        }

        // GenerateKey generates an RSA keypair of the given bit size using the
        // random source random (for example, crypto/rand.Reader).
        public static (ref PrivateKey, error) GenerateKey(io.Reader random, long bits)
        {
            return GenerateMultiPrimeKey(random, 2L, bits);
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
        public static (ref PrivateKey, error) GenerateMultiPrimeKey(io.Reader random, long nprimes, long bits)
        {
            ptr<PrivateKey> priv = @new<PrivateKey>();
            priv.E = 65537L;

            if (nprimes < 2L)
            {
                return (null, errors.New("crypto/rsa: GenerateMultiPrimeKey: nprimes must be >= 2"));
            }
            if (bits < 64L)
            {
                var primeLimit = float64(uint64(1L) << (int)(uint(bits / nprimes))); 
                // pi approximates the number of primes less than primeLimit
                var pi = primeLimit / (math.Log(primeLimit) - 1L); 
                // Generated primes start with 11 (in binary) so we can only
                // use a quarter of them.
                pi /= 4L; 
                // Use a factor of two to ensure that key generation terminates
                // in a reasonable amount of time.
                pi /= 2L;
                if (pi <= float64(nprimes))
                {
                    return (null, errors.New("crypto/rsa: too few primes of given length to generate an RSA key"));
                }
            }
            var primes = make_slice<ref big.Int>(nprimes);

NextSetOfPrimes:

            while (true)
            {
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
                if (nprimes >= 7L)
                {
                    todo += (nprimes - 2L) / 5L;
                }
                {
                    long i__prev2 = i;

                    for (long i = 0L; i < nprimes; i++)
                    {
                        error err = default;
                        primes[i], err = rand.Prime(random, todo / (nprimes - i));
                        if (err != null)
                        {
                            return (null, err);
                        }
                        todo -= primes[i].BitLen();
                    } 

                    // Make sure that primes is pairwise unequal.


                    i = i__prev2;
                } 

                // Make sure that primes is pairwise unequal.
                {
                    long i__prev2 = i;
                    var prime__prev2 = prime;

                    foreach (var (__i, __prime) in primes)
                    {
                        i = __i;
                        prime = __prime;
                        for (long j = 0L; j < i; j++)
                        {
                            if (prime.Cmp(primes[j]) == 0L)
                            {
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

                    foreach (var (_, __prime) in primes)
                    {
                        prime = __prime;
                        n.Mul(n, prime);
                        pminus1.Sub(prime, bigOne);
                        totient.Mul(totient, pminus1);
                    }

                    prime = prime__prev2;
                }

                if (n.BitLen() != bits)
                { 
                    // This should never happen for nprimes == 2 because
                    // crypto/rand should set the top two bits in each prime.
                    // For nprimes > 2 we hope it does not happen often.
                    _continueNextSetOfPrimes = true;
                    break;
                }
                ptr<big.Int> g = @new<big.Int>();
                priv.D = @new<big.Int>();
                var e = big.NewInt(int64(priv.E));
                g.GCD(priv.D, null, e, totient);

                if (g.Cmp(bigOne) == 0L)
                {
                    if (priv.D.Sign() < 0L)
                    {
                        priv.D.Add(priv.D, totient);
                    }
                    priv.Primes = primes;
                    priv.N = n;

                    break;
                }
            }

            priv.Precompute();
            return (priv, null);
        }

        // incCounter increments a four byte, big-endian counter.
        private static void incCounter(ref array<byte> c)
        {
            c[3L]++;

            if (c[3L] != 0L)
            {
                return;
            }
            c[2L]++;

            if (c[2L] != 0L)
            {
                return;
            }
            c[1L]++;

            if (c[1L] != 0L)
            {
                return;
            }
            c[0L]++;
        }

        // mgf1XOR XORs the bytes in out with a mask generated using the MGF1 function
        // specified in PKCS#1 v2.1.
        private static void mgf1XOR(slice<byte> @out, hash.Hash hash, slice<byte> seed)
        {
            array<byte> counter = new array<byte>(4L);
            slice<byte> digest = default;

            long done = 0L;
            while (done < len(out))
            {
                hash.Write(seed);
                hash.Write(counter[0L..4L]);
                digest = hash.Sum(digest[..0L]);
                hash.Reset();

                for (long i = 0L; i < len(digest) && done < len(out); i++)
                {
                    out[done] ^= digest[i];
                    done++;
                }

                incCounter(ref counter);
            }

        }

        // ErrMessageTooLong is returned when attempting to encrypt a message which is
        // too large for the size of the public key.
        public static var ErrMessageTooLong = errors.New("crypto/rsa: message too long for RSA public key size");

        private static ref big.Int encrypt(ref big.Int c, ref PublicKey pub, ref big.Int m)
        {
            var e = big.NewInt(int64(pub.E));
            c.Exp(m, e, pub.N);
            return c;
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
        // public key is used to decrypt two types of messages then distinct label
        // values could be used to ensure that a ciphertext for one purpose cannot be
        // used for another by an attacker. If not required it can be empty.
        //
        // The message must be no longer than the length of the public modulus minus
        // twice the hash length, minus a further 2.
        public static (slice<byte>, error) EncryptOAEP(hash.Hash hash, io.Reader random, ref PublicKey pub, slice<byte> msg, slice<byte> label)
        {
            {
                var err = checkPub(pub);

                if (err != null)
                {
                    return (null, err);
                }

            }
            hash.Reset();
            var k = (pub.N.BitLen() + 7L) / 8L;
            if (len(msg) > k - 2L * hash.Size() - 2L)
            {
                return (null, ErrMessageTooLong);
            }
            hash.Write(label);
            var lHash = hash.Sum(null);
            hash.Reset();

            var em = make_slice<byte>(k);
            var seed = em[1L..1L + hash.Size()];
            var db = em[1L + hash.Size()..];

            copy(db[0L..hash.Size()], lHash);
            db[len(db) - len(msg) - 1L] = 1L;
            copy(db[len(db) - len(msg)..], msg);

            var (_, err) = io.ReadFull(random, seed);
            if (err != null)
            {
                return (null, err);
            }
            mgf1XOR(db, hash, seed);
            mgf1XOR(seed, hash, db);

            ptr<big.Int> m = @new<big.Int>();
            m.SetBytes(em);
            var c = encrypt(@new<big.Int>(), pub, m);
            var @out = c.Bytes();

            if (len(out) < k)
            { 
                // If the output is too small, we need to left-pad with zeros.
                var t = make_slice<byte>(k);
                copy(t[k - len(out)..], out);
                out = t;
            }
            return (out, null);
        }

        // ErrDecryption represents a failure to decrypt a message.
        // It is deliberately vague to avoid adaptive attacks.
        public static var ErrDecryption = errors.New("crypto/rsa: decryption error");

        // ErrVerification represents a failure to verify a signature.
        // It is deliberately vague to avoid adaptive attacks.
        public static var ErrVerification = errors.New("crypto/rsa: verification error");

        // modInverse returns ia, the inverse of a in the multiplicative group of prime
        // order n. It requires that a be a member of the group (i.e. less than n).
        private static (ref big.Int, bool) modInverse(ref big.Int a, ref big.Int n)
        {
            ptr<big.Int> g = @new<big.Int>();
            ptr<big.Int> x = @new<big.Int>();
            g.GCD(x, null, a, n);
            if (g.Cmp(bigOne) != 0L)
            { 
                // In this case, a and n aren't coprime and we cannot calculate
                // the inverse. This happens because the values of n are nearly
                // prime (being the product of two primes) rather than truly
                // prime.
                return;
            }
            if (x.Cmp(bigOne) < 0L)
            { 
                // 0 is not the multiplicative inverse of any element so, if x
                // < 1, then x is negative.
                x.Add(x, n);
            }
            return (x, true);
        }

        // Precompute performs some calculations that speed up private key operations
        // in the future.
        private static void Precompute(this ref PrivateKey priv)
        {
            if (priv.Precomputed.Dp != null)
            {
                return;
            }
            priv.Precomputed.Dp = @new<big.Int>().Sub(priv.Primes[0L], bigOne);
            priv.Precomputed.Dp.Mod(priv.D, priv.Precomputed.Dp);

            priv.Precomputed.Dq = @new<big.Int>().Sub(priv.Primes[1L], bigOne);
            priv.Precomputed.Dq.Mod(priv.D, priv.Precomputed.Dq);

            priv.Precomputed.Qinv = @new<big.Int>().ModInverse(priv.Primes[1L], priv.Primes[0L]);

            ptr<big.Int> r = @new<big.Int>().Mul(priv.Primes[0L], priv.Primes[1L]);
            priv.Precomputed.CRTValues = make_slice<CRTValue>(len(priv.Primes) - 2L);
            for (long i = 2L; i < len(priv.Primes); i++)
            {
                var prime = priv.Primes[i];
                var values = ref priv.Precomputed.CRTValues[i - 2L];

                values.Exp = @new<big.Int>().Sub(prime, bigOne);
                values.Exp.Mod(priv.D, values.Exp);

                values.R = @new<big.Int>().Set(r);
                values.Coeff = @new<big.Int>().ModInverse(r, prime);

                r.Mul(r, prime);
            }

        }

        // decrypt performs an RSA decryption, resulting in a plaintext integer. If a
        // random source is given, RSA blinding is used.
        private static (ref big.Int, error) decrypt(io.Reader random, ref PrivateKey priv, ref big.Int c)
        { 
            // TODO(agl): can we get away with reusing blinds?
            if (c.Cmp(priv.N) > 0L)
            {
                err = ErrDecryption;
                return;
            }
            if (priv.N.Sign() == 0L)
            {
                return (null, ErrDecryption);
            }
            ref big.Int ir = default;
            if (random != null)
            { 
                // Blinding enabled. Blinding involves multiplying c by r^e.
                // Then the decryption operation performs (m^e * r^e)^d mod n
                // which equals mr mod n. The factor of r can then be removed
                // by multiplying by the multiplicative inverse of r.

                ref big.Int r = default;

                while (true)
                {
                    r, err = rand.Int(random, priv.N);
                    if (err != null)
                    {
                        return;
                    }
                    if (r.Cmp(bigZero) == 0L)
                    {
                        r = bigOne;
                    }
                    bool ok = default;
                    ir, ok = modInverse(r, priv.N);
                    if (ok)
                    {
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
            if (priv.Precomputed.Dp == null)
            {
                m = @new<big.Int>().Exp(c, priv.D, priv.N);
            }
            else
            { 
                // We have the precalculated values needed for the CRT.
                m = @new<big.Int>().Exp(c, priv.Precomputed.Dp, priv.Primes[0L]);
                ptr<big.Int> m2 = @new<big.Int>().Exp(c, priv.Precomputed.Dq, priv.Primes[1L]);
                m.Sub(m, m2);
                if (m.Sign() < 0L)
                {
                    m.Add(m, priv.Primes[0L]);
                }
                m.Mul(m, priv.Precomputed.Qinv);
                m.Mod(m, priv.Primes[0L]);
                m.Mul(m, priv.Primes[1L]);
                m.Add(m, m2);

                foreach (var (i, values) in priv.Precomputed.CRTValues)
                {
                    var prime = priv.Primes[2L + i];
                    m2.Exp(c, values.Exp, prime);
                    m2.Sub(m2, m);
                    m2.Mul(m2, values.Coeff);
                    m2.Mod(m2, prime);
                    if (m2.Sign() < 0L)
                    {
                        m2.Add(m2, prime);
                    }
                    m2.Mul(m2, values.R);
                    m.Add(m, m2);
                }
            }
            if (ir != null)
            { 
                // Unblind.
                m.Mul(m, ir);
                m.Mod(m, priv.N);
            }
            return;
        }

        private static (ref big.Int, error) decryptAndCheck(io.Reader random, ref PrivateKey priv, ref big.Int c)
        {
            m, err = decrypt(random, priv, c);
            if (err != null)
            {
                return (null, err);
            } 

            // In order to defend against errors in the CRT computation, m^e is
            // calculated, which should match the original ciphertext.
            var check = encrypt(@new<big.Int>(), ref priv.PublicKey, m);
            if (c.Cmp(check) != 0L)
            {
                return (null, errors.New("rsa: internal error"));
            }
            return (m, null);
        }

        // DecryptOAEP decrypts ciphertext using RSA-OAEP.

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
        public static (slice<byte>, error) DecryptOAEP(hash.Hash hash, io.Reader random, ref PrivateKey priv, slice<byte> ciphertext, slice<byte> label)
        {
            {
                var err = checkPub(ref priv.PublicKey);

                if (err != null)
                {
                    return (null, err);
                }

            }
            var k = (priv.N.BitLen() + 7L) / 8L;
            if (len(ciphertext) > k || k < hash.Size() * 2L + 2L)
            {
                return (null, ErrDecryption);
            }
            ptr<big.Int> c = @new<big.Int>().SetBytes(ciphertext);

            var (m, err) = decrypt(random, priv, c);
            if (err != null)
            {
                return (null, err);
            }
            hash.Write(label);
            var lHash = hash.Sum(null);
            hash.Reset(); 

            // Converting the plaintext number to bytes will strip any
            // leading zeros so we may have to left pad. We do this unconditionally
            // to avoid leaking timing information. (Although we still probably
            // leak the number of leading zeros. It's not clear that we can do
            // anything about this.)
            var em = leftPad(m.Bytes(), k);

            var firstByteIsZero = subtle.ConstantTimeByteEq(em[0L], 0L);

            var seed = em[1L..hash.Size() + 1L];
            var db = em[hash.Size() + 1L..];

            mgf1XOR(seed, hash, db);
            mgf1XOR(db, hash, seed);

            var lHash2 = db[0L..hash.Size()]; 

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
            long lookingForIndex = default;            long index = default;            long invalid = default;

            lookingForIndex = 1L;
            var rest = db[hash.Size()..];

            for (long i = 0L; i < len(rest); i++)
            {
                var equals0 = subtle.ConstantTimeByteEq(rest[i], 0L);
                var equals1 = subtle.ConstantTimeByteEq(rest[i], 1L);
                index = subtle.ConstantTimeSelect(lookingForIndex & equals1, i, index);
                lookingForIndex = subtle.ConstantTimeSelect(equals1, 0L, lookingForIndex);
                invalid = subtle.ConstantTimeSelect(lookingForIndex & ~equals0, 1L, invalid);
            }


            if (firstByteIsZero & lHash2Good & ~invalid & ~lookingForIndex != 1L)
            {
                return (null, ErrDecryption);
            }
            return (rest[index + 1L..], null);
        }

        // leftPad returns a new slice of length size. The contents of input are right
        // aligned in the new slice.
        private static slice<byte> leftPad(slice<byte> input, long size)
        {
            var n = len(input);
            if (n > size)
            {
                n = size;
            }
            out = make_slice<byte>(size);
            copy(out[len(out) - n..], input);
            return;
        }
    }
}}
