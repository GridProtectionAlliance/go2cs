// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package ecdsa implements the Elliptic Curve Digital Signature Algorithm, as
// defined in FIPS 186-3.
//
// This implementation  derives the nonce from an AES-CTR CSPRNG keyed by
// ChopMD(256, SHA2-512(priv.D || entropy || hash)). The CSPRNG key is IRO by
// a result of Coron; the AES-CTR stream is IRO under standard assumptions.
// package ecdsa -- go2cs converted at 2020 August 29 08:29:35 UTC
// import "crypto/ecdsa" ==> using ecdsa = go.crypto.ecdsa_package
// Original source: C:\Go\src\crypto\ecdsa\ecdsa.go
// References:
//   [NSA]: Suite B implementer's guide to FIPS 186-3,
//     http://www.nsa.gov/ia/_files/ecdsa.pdf
//   [SECG]: SECG, SEC1
//     http://www.secg.org/sec1-v2.pdf

using crypto = go.crypto_package;
using aes = go.crypto.aes_package;
using cipher = go.crypto.cipher_package;
using elliptic = go.crypto.elliptic_package;
using sha512 = go.crypto.sha512_package;
using asn1 = go.encoding.asn1_package;
using errors = go.errors_package;
using io = go.io_package;
using big = go.math.big_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class ecdsa_package
    {
        // A invertible implements fast inverse mod Curve.Params().N
        private partial interface invertible
        {
            ref big.Int Inverse(ref big.Int k);
        }

        // combinedMult implements fast multiplication S1*g + S2*p (g - generator, p - arbitrary point)
        private partial interface combinedMult
        {
            (ref big.Int, ref big.Int) CombinedMult(ref big.Int bigX, ref big.Int bigY, slice<byte> baseScalar, slice<byte> scalar);
        }

        private static readonly @string aesIV = "IV for ECDSA CTR";

        // PublicKey represents an ECDSA public key.
        public partial struct PublicKey : elliptic.Curve
        {
            public ref elliptic.Curve Curve => ref Curve_val;
            public ptr<big.Int> X;
            public ptr<big.Int> Y;
        }

        // PrivateKey represents an ECDSA private key.
        public partial struct PrivateKey : PublicKey
        {
            public PublicKey PublicKey;
            public ptr<big.Int> D;
        }

        private partial struct ecdsaSignature
        {
            public ptr<big.Int> R;
            public ptr<big.Int> S;
        }

        // Public returns the public key corresponding to priv.
        private static crypto.PublicKey Public(this ref PrivateKey priv)
        {
            return ref priv.PublicKey;
        }

        // Sign signs digest with priv, reading randomness from rand. The opts argument
        // is not currently used but, in keeping with the crypto.Signer interface,
        // should be the hash function used to digest the message.
        //
        // This method implements crypto.Signer, which is an interface to support keys
        // where the private part is kept in, for example, a hardware module. Common
        // uses should use the Sign function in this package directly.
        private static (slice<byte>, error) Sign(this ref PrivateKey priv, io.Reader rand, slice<byte> digest, crypto.SignerOpts opts)
        {
            var (r, s, err) = Sign(rand, priv, digest);
            if (err != null)
            {
                return (null, err);
            }
            return asn1.Marshal(new ecdsaSignature(r,s));
        }

        private static ptr<big.Int> one = @new<big.Int>().SetInt64(1L);

        // randFieldElement returns a random element of the field underlying the given
        // curve using the procedure given in [NSA] A.2.1.
        private static (ref big.Int, error) randFieldElement(elliptic.Curve c, io.Reader rand)
        {
            var @params = c.Params();
            var b = make_slice<byte>(@params.BitSize / 8L + 8L);
            _, err = io.ReadFull(rand, b);
            if (err != null)
            {
                return;
            }
            k = @new<big.Int>().SetBytes(b);
            ptr<big.Int> n = @new<big.Int>().Sub(@params.N, one);
            k.Mod(k, n);
            k.Add(k, one);
            return;
        }

        // GenerateKey generates a public and private key pair.
        public static (ref PrivateKey, error) GenerateKey(elliptic.Curve c, io.Reader rand)
        {
            var (k, err) = randFieldElement(c, rand);
            if (err != null)
            {
                return (null, err);
            }
            ptr<PrivateKey> priv = @new<PrivateKey>();
            priv.PublicKey.Curve = c;
            priv.D = k;
            priv.PublicKey.X, priv.PublicKey.Y = c.ScalarBaseMult(k.Bytes());
            return (priv, null);
        }

        // hashToInt converts a hash value to an integer. There is some disagreement
        // about how this is done. [NSA] suggests that this is done in the obvious
        // manner, but [SECG] truncates the hash to the bit-length of the curve order
        // first. We follow [SECG] because that's what OpenSSL does. Additionally,
        // OpenSSL right shifts excess bits from the number if the hash is too large
        // and we mirror that too.
        private static ref big.Int hashToInt(slice<byte> hash, elliptic.Curve c)
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
            return ret;
        }

        // fermatInverse calculates the inverse of k in GF(P) using Fermat's method.
        // This has better constant-time properties than Euclid's method (implemented
        // in math/big.Int.ModInverse) although math/big itself isn't strictly
        // constant-time so it's not perfect.
        private static ref big.Int fermatInverse(ref big.Int k, ref big.Int N)
        {
            var two = big.NewInt(2L);
            ptr<big.Int> nMinus2 = @new<big.Int>().Sub(N, two);
            return @new<big.Int>().Exp(k, nMinus2, N);
        }

        private static var errZeroParam = errors.New("zero parameter");

        // Sign signs a hash (which should be the result of hashing a larger message)
        // using the private key, priv. If the hash is longer than the bit-length of the
        // private key's curve order, the hash will be truncated to that length.  It
        // returns the signature as a pair of integers. The security of the private key
        // depends on the entropy of rand.
        public static (ref big.Int, ref big.Int, error) Sign(io.Reader rand, ref PrivateKey priv, slice<byte> hash)
        { 
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
                return;
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
                return (null, null, err);
            } 

            // Create a CSPRNG that xors a stream of zeros with
            // the output of the AES-CTR instance.
            cipher.StreamReader csprng = new cipher.StreamReader(R:zeroReader,S:cipher.NewCTR(block,[]byte(aesIV)),); 

            // See [NSA] 3.4.1
            var c = priv.PublicKey.Curve;
            var N = c.Params().N;
            if (N.Sign() == 0L)
            {
                return (null, null, errZeroParam);
            }
            ref big.Int k = default;            ref big.Int kInv = default;

            while (true)
            {
                while (true)
                {
                    k, err = randFieldElement(c, csprng);
                    if (err != null)
                    {
                        r = null;
                        return;
                    }
                    {
                        invertible (in, ok) = priv.Curve._<invertible>();

                        if (ok)
                        {
                            kInv = @in.Inverse(k);
                        }
                        else
                        {
                            kInv = fermatInverse(k, N); // N != 0
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


            return;
        }

        // Verify verifies the signature in r, s of hash using the public key, pub. Its
        // return value records whether the signature is valid.
        public static bool Verify(ref PublicKey pub, slice<byte> hash, ref big.Int r, ref big.Int s)
        { 
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
            var e = hashToInt(hash, c);

            ref big.Int w = default;
            {
                invertible (in, ok) = c._<invertible>();

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
            ref big.Int x = default;            ref big.Int y = default;

            {
                combinedMult (opt, ok) = c._<combinedMult>();

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

        private partial struct zr : io.Reader
        {
            public ref io.Reader Reader => ref Reader_val;
        }

        // Read replaces the contents of dst with zeros.
        private static (long, error) Read(this ref zr z, slice<byte> dst)
        {
            foreach (var (i) in dst)
            {
                dst[i] = 0L;
            }
            return (len(dst), null);
        }

        private static zr zeroReader = ref new zr();
    }
}}
