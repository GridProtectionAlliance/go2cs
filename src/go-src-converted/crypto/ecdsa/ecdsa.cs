// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package ecdsa implements the Elliptic Curve Digital Signature Algorithm, as
// defined in FIPS 186-3.
//
// This implementation derives the nonce from an AES-CTR CSPRNG keyed by:
//
// SHA2-512(priv.D || entropy || hash)[:32]
//
// The CSPRNG key is indifferentiable from a random oracle as shown in
// [Coron], the AES-CTR stream is indifferentiable from a random oracle
// under standard cryptographic assumptions (see [Larsson] for examples).
//
// References:
//   [Coron]
//     https://cs.nyu.edu/~dodis/ps/merkle.pdf
//   [Larsson]
//     https://www.nada.kth.se/kurser/kth/2D1441/semteo03/lecturenotes/assump.pdf
// package ecdsa -- go2cs converted at 2020 October 09 04:52:47 UTC
// import "crypto/ecdsa" ==> using ecdsa = go.crypto.ecdsa_package
// Original source: C:\Go\src\crypto\ecdsa\ecdsa.go
// Further references:
//   [NSA]: Suite B implementer's guide to FIPS 186-3
//     https://apps.nsa.gov/iaarchive/library/ia-guidance/ia-solutions-for-classified/algorithm-guidance/suite-b-implementers-guide-to-fips-186-3-ecdsa.cfm
//   [SECG]: SECG, SEC1
//     http://www.secg.org/sec1-v2.pdf

using crypto = go.crypto_package;
using aes = go.crypto.aes_package;
using cipher = go.crypto.cipher_package;
using elliptic = go.crypto.elliptic_package;
using randutil = go.crypto.@internal.randutil_package;
using sha512 = go.crypto.sha512_package;
using errors = go.errors_package;
using io = go.io_package;
using big = go.math.big_package;

using cryptobyte = go.golang.org.x.crypto.cryptobyte_package;
using asn1 = go.golang.org.x.crypto.cryptobyte.asn1_package;
using static go.builtin;
using System;

namespace go {
namespace crypto
{
    public static partial class ecdsa_package
    {
        // A invertible implements fast inverse mod Curve.Params().N
        private partial interface invertible
        {
            ptr<big.Int> Inverse(ptr<big.Int> k);
        }

        // combinedMult implements fast multiplication S1*g + S2*p (g - generator, p - arbitrary point)
        private partial interface combinedMult
        {
            (ptr<big.Int>, ptr<big.Int>) CombinedMult(ptr<big.Int> bigX, ptr<big.Int> bigY, slice<byte> baseScalar, slice<byte> scalar);
        }

        private static readonly @string aesIV = (@string)"IV for ECDSA CTR";


        // PublicKey represents an ECDSA public key.
        public partial struct PublicKey : elliptic.Curve
        {
            public ref elliptic.Curve Curve => ref Curve_val;
            public ptr<big.Int> X;
            public ptr<big.Int> Y;
        }

        // Any methods implemented on PublicKey might need to also be implemented on
        // PrivateKey, as the latter embeds the former and will expose its methods.

        // Equal reports whether pub and x have the same value.
        //
        // Two keys are only considered to have the same value if they have the same Curve value.
        // Note that for example elliptic.P256() and elliptic.P256().Params() are different
        // values, as the latter is a generic not constant time implementation.
        private static bool Equal(this ptr<PublicKey> _addr_pub, crypto.PublicKey x)
        {
            ref PublicKey pub = ref _addr_pub.val;

            ptr<PublicKey> (xx, ok) = x._<ptr<PublicKey>>();
            if (!ok)
            {
                return false;
            }

            return pub.X.Cmp(xx.X) == 0L && pub.Y.Cmp(xx.Y) == 0L && pub.Curve == xx.Curve;

        }

        // PrivateKey represents an ECDSA private key.
        public partial struct PrivateKey : PublicKey
        {
            public PublicKey PublicKey;
            public ptr<big.Int> D;
        }

        // Public returns the public key corresponding to priv.
        private static crypto.PublicKey Public(this ptr<PrivateKey> _addr_priv)
        {
            ref PrivateKey priv = ref _addr_priv.val;

            return _addr_priv.PublicKey;
        }

        // Equal reports whether priv and x have the same value.
        //
        // See PublicKey.Equal for details on how Curve is compared.
        private static bool Equal(this ptr<PrivateKey> _addr_priv, crypto.PrivateKey x)
        {
            ref PrivateKey priv = ref _addr_priv.val;

            ptr<PrivateKey> (xx, ok) = x._<ptr<PrivateKey>>();
            if (!ok)
            {
                return false;
            }

            return priv.PublicKey.Equal(_addr_xx.PublicKey) && priv.D.Cmp(xx.D) == 0L;

        }

        // Sign signs digest with priv, reading randomness from rand. The opts argument
        // is not currently used but, in keeping with the crypto.Signer interface,
        // should be the hash function used to digest the message.
        //
        // This method implements crypto.Signer, which is an interface to support keys
        // where the private part is kept in, for example, a hardware module. Common
        // uses should use the Sign function in this package directly.
        private static (slice<byte>, error) Sign(this ptr<PrivateKey> _addr_priv, io.Reader rand, slice<byte> digest, crypto.SignerOpts opts)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref PrivateKey priv = ref _addr_priv.val;

            var (r, s, err) = Sign(rand, _addr_priv, digest);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            cryptobyte.Builder b = default;
            b.AddASN1(asn1.SEQUENCE, b =>
            {
                b.AddASN1BigInt(r);
                b.AddASN1BigInt(s);
            });
            return b.Bytes();

        }

        private static ptr<big.Int> one = @new<big.Int>().SetInt64(1L);

        // randFieldElement returns a random element of the field underlying the given
        // curve using the procedure given in [NSA] A.2.1.
        private static (ptr<big.Int>, error) randFieldElement(elliptic.Curve c, io.Reader rand)
        {
            ptr<big.Int> k = default!;
            error err = default!;

            var @params = c.Params();
            var b = make_slice<byte>(@params.BitSize / 8L + 8L);
            _, err = io.ReadFull(rand, b);
            if (err != null)
            {
                return ;
            }

            k = @new<big.Int>().SetBytes(b);
            ptr<big.Int> n = @new<big.Int>().Sub(@params.N, one);
            k.Mod(k, n);
            k.Add(k, one);
            return ;

        }

        // GenerateKey generates a public and private key pair.
        public static (ptr<PrivateKey>, error) GenerateKey(elliptic.Curve c, io.Reader rand)
        {
            ptr<PrivateKey> _p0 = default!;
            error _p0 = default!;

            var (k, err) = randFieldElement(c, rand);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            ptr<PrivateKey> priv = @new<PrivateKey>();
            priv.PublicKey.Curve = c;
            priv.D = k;
            priv.PublicKey.X, priv.PublicKey.Y = c.ScalarBaseMult(k.Bytes());
            return (_addr_priv!, error.As(null!)!);

        }

        // hashToInt converts a hash value to an integer. There is some disagreement
        // about how this is done. [NSA] suggests that this is done in the obvious
        // manner, but [SECG] truncates the hash to the bit-length of the curve order
        // first. We follow [SECG] because that's what OpenSSL does. Additionally,
        // OpenSSL right shifts excess bits from the number if the hash is too large
        // and we mirror that too.
        private static ptr<big.Int> hashToInt(slice<byte> hash, elliptic.Curve c)
        {
            var orderBits = c.Params().N.BitLen();
            var orderBytes = (orderBits + 7L) / 8L;
            if (len(hash) > orderBytes)
            {
                hash = hash[..orderBytes];
            }

            ptr<big.Int> ret = @new<big.Int>().SetBytes(hash);
            var excess = len(hash) * 8L - orderBits;
            if (excess > 0L)
            {
                ret.Rsh(ret, uint(excess));
            }

            return _addr_ret!;

        }

        // fermatInverse calculates the inverse of k in GF(P) using Fermat's method.
        // This has better constant-time properties than Euclid's method (implemented
        // in math/big.Int.ModInverse) although math/big itself isn't strictly
        // constant-time so it's not perfect.
        private static ptr<big.Int> fermatInverse(ptr<big.Int> _addr_k, ptr<big.Int> _addr_N)
        {
            ref big.Int k = ref _addr_k.val;
            ref big.Int N = ref _addr_N.val;

            var two = big.NewInt(2L);
            ptr<big.Int> nMinus2 = @new<big.Int>().Sub(N, two);
            return @new<big.Int>().Exp(k, nMinus2, N);
        }

        private static var errZeroParam = errors.New("zero parameter");

        // Sign signs a hash (which should be the result of hashing a larger message)
        // using the private key, priv. If the hash is longer than the bit-length of the
        // private key's curve order, the hash will be truncated to that length. It
        // returns the signature as a pair of integers. The security of the private key
        // depends on the entropy of rand.
        public static (ptr<big.Int>, ptr<big.Int>, error) Sign(io.Reader rand, ptr<PrivateKey> _addr_priv, slice<byte> hash)
        {
            ptr<big.Int> r = default!;
            ptr<big.Int> s = default!;
            error err = default!;
            ref PrivateKey priv = ref _addr_priv.val;

            randutil.MaybeReadByte(rand); 

            // Get min(log2(q) / 2, 256) bits of entropy from rand.
            var entropylen = (priv.Curve.Params().BitSize + 7L) / 16L;
            if (entropylen > 32L)
            {
                entropylen = 32L;
            }

            var entropy = make_slice<byte>(entropylen);
            _, err = io.ReadFull(rand, entropy);
            if (err != null)
            {
                return ;
            } 

            // Initialize an SHA-512 hash context; digest ...
            var md = sha512.New();
            md.Write(priv.D.Bytes()); // the private key,
            md.Write(entropy); // the entropy,
            md.Write(hash); // and the input hash;
            var key = md.Sum(null)[..32L]; // and compute ChopMD-256(SHA-512),
            // which is an indifferentiable MAC.

            // Create an AES-CTR instance to use as a CSPRNG.
            var (block, err) = aes.NewCipher(key);
            if (err != null)
            {
                return (_addr_null!, _addr_null!, error.As(err)!);
            } 

            // Create a CSPRNG that xors a stream of zeros with
            // the output of the AES-CTR instance.
            ref cipher.StreamReader csprng = ref heap(new cipher.StreamReader(R:zeroReader,S:cipher.NewCTR(block,[]byte(aesIV)),), out ptr<cipher.StreamReader> _addr_csprng); 

            // See [NSA] 3.4.1
            var c = priv.PublicKey.Curve;
            return _addr_sign(priv, _addr_csprng, c, hash)!;

        }

        private static (ptr<big.Int>, ptr<big.Int>, error) signGeneric(ptr<PrivateKey> _addr_priv, ptr<cipher.StreamReader> _addr_csprng, elliptic.Curve c, slice<byte> hash)
        {
            ptr<big.Int> r = default!;
            ptr<big.Int> s = default!;
            error err = default!;
            ref PrivateKey priv = ref _addr_priv.val;
            ref cipher.StreamReader csprng = ref _addr_csprng.val;

            var N = c.Params().N;
            if (N.Sign() == 0L)
            {
                return (_addr_null!, _addr_null!, error.As(errZeroParam)!);
            }

            ptr<big.Int> k;            ptr<big.Int> kInv;

            while (true)
            {
                while (true)
                {
                    k, err = randFieldElement(c, csprng);
                    if (err != null)
                    {
                        r = null;
                        return ;
                    }

                    {
                        invertible (in, ok) = invertible.As(priv.Curve._<invertible>())!;

                        if (ok)
                        {
                            kInv = @in.Inverse(k);
                        }
                        else
                        {
                            kInv = fermatInverse(k, _addr_N); // N != 0
                        }

                    }


                    r, _ = priv.Curve.ScalarBaseMult(k.Bytes());
                    r.Mod(r, N);
                    if (r.Sign() != 0L)
                    {
                        break;
                    }

                }


                var e = hashToInt(hash, c);
                s = @new<big.Int>().Mul(priv.D, r);
                s.Add(s, e);
                s.Mul(s, kInv);
                s.Mod(s, N); // N != 0
                if (s.Sign() != 0L)
                {
                    break;
                }

            }


            return ;

        }

        // SignASN1 signs a hash (which should be the result of hashing a larger message)
        // using the private key, priv. If the hash is longer than the bit-length of the
        // private key's curve order, the hash will be truncated to that length. It
        // returns the ASN.1 encoded signature. The security of the private key
        // depends on the entropy of rand.
        public static (slice<byte>, error) SignASN1(io.Reader rand, ptr<PrivateKey> _addr_priv, slice<byte> hash)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref PrivateKey priv = ref _addr_priv.val;

            return priv.Sign(rand, hash, null);
        }

        // Verify verifies the signature in r, s of hash using the public key, pub. Its
        // return value records whether the signature is valid.
        public static bool Verify(ptr<PublicKey> _addr_pub, slice<byte> hash, ptr<big.Int> _addr_r, ptr<big.Int> _addr_s)
        {
            ref PublicKey pub = ref _addr_pub.val;
            ref big.Int r = ref _addr_r.val;
            ref big.Int s = ref _addr_s.val;
 
            // See [NSA] 3.4.2
            var c = pub.Curve;
            var N = c.Params().N;

            if (r.Sign() <= 0L || s.Sign() <= 0L)
            {
                return false;
            }

            if (r.Cmp(N) >= 0L || s.Cmp(N) >= 0L)
            {
                return false;
            }

            return verify(pub, c, hash, r, s);

        }

        private static bool verifyGeneric(ptr<PublicKey> _addr_pub, elliptic.Curve c, slice<byte> hash, ptr<big.Int> _addr_r, ptr<big.Int> _addr_s)
        {
            ref PublicKey pub = ref _addr_pub.val;
            ref big.Int r = ref _addr_r.val;
            ref big.Int s = ref _addr_s.val;

            var e = hashToInt(hash, c);
            ptr<big.Int> w;
            var N = c.Params().N;
            {
                invertible (in, ok) = invertible.As(c._<invertible>())!;

                if (ok)
                {
                    w = @in.Inverse(s);
                }
                else
                {
                    w = @new<big.Int>().ModInverse(s, N);
                }

            }


            var u1 = e.Mul(e, w);
            u1.Mod(u1, N);
            var u2 = w.Mul(r, w);
            u2.Mod(u2, N); 

            // Check if implements S1*g + S2*p
            ptr<big.Int> x;            ptr<big.Int> y;

            {
                combinedMult (opt, ok) = combinedMult.As(c._<combinedMult>())!;

                if (ok)
                {
                    x, y = opt.CombinedMult(pub.X, pub.Y, u1.Bytes(), u2.Bytes());
                }
                else
                {
                    var (x1, y1) = c.ScalarBaseMult(u1.Bytes());
                    var (x2, y2) = c.ScalarMult(pub.X, pub.Y, u2.Bytes());
                    x, y = c.Add(x1, y1, x2, y2);
                }

            }


            if (x.Sign() == 0L && y.Sign() == 0L)
            {
                return false;
            }

            x.Mod(x, N);
            return x.Cmp(r) == 0L;

        }

        // VerifyASN1 verifies the ASN.1 encoded signature, sig, of hash using the
        // public key, pub. Its return value records whether the signature is valid.
        public static bool VerifyASN1(ptr<PublicKey> _addr_pub, slice<byte> hash, slice<byte> sig)
        {
            ref PublicKey pub = ref _addr_pub.val;

            ptr<big.Int> r = addr(new big.Int());            ptr<big.Int> s = addr(new big.Int());
            ref cryptobyte.String inner = ref heap(out ptr<cryptobyte.String> _addr_inner);
            var input = cryptobyte.String(sig);
            if (!input.ReadASN1(_addr_inner, asn1.SEQUENCE) || !input.Empty() || !inner.ReadASN1Integer(r) || !inner.ReadASN1Integer(s) || !inner.Empty())
            {
                return false;
            }

            return Verify(_addr_pub, hash, _addr_r, _addr_s);

        }

        private partial struct zr : io.Reader
        {
            public ref io.Reader Reader => ref Reader_val;
        }

        // Read replaces the contents of dst with zeros.
        private static (long, error) Read(this ptr<zr> _addr_z, slice<byte> dst)
        {
            long n = default;
            error err = default!;
            ref zr z = ref _addr_z.val;

            foreach (var (i) in dst)
            {
                dst[i] = 0L;
            }
            return (len(dst), error.As(null!)!);

        }

        private static ptr<zr> zeroReader = addr(new zr());
    }
}}
