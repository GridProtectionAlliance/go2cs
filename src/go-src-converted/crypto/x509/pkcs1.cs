// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package x509 -- go2cs converted at 2020 August 29 08:31:42 UTC
// import "crypto/x509" ==> using x509 = go.crypto.x509_package
// Original source: C:\Go\src\crypto\x509\pkcs1.go
using rsa = go.crypto.rsa_package;
using asn1 = go.encoding.asn1_package;
using errors = go.errors_package;
using big = go.math.big_package;
using static go.builtin;
using System.ComponentModel;

namespace go {
namespace crypto
{
    public static partial class x509_package
    {
        // pkcs1PrivateKey is a structure which mirrors the PKCS#1 ASN.1 for an RSA private key.
        private partial struct pkcs1PrivateKey
        {
            public long Version;
            public ptr<big.Int> N;
            public long E;
            public ptr<big.Int> D;
            public ptr<big.Int> P;
            public ptr<big.Int> Q; // We ignore these values, if present, because rsa will calculate them.
            [Description("asn1:\"optional\"")]
            public ptr<big.Int> Dp;
            [Description("asn1:\"optional\"")]
            public ptr<big.Int> Dq;
            [Description("asn1:\"optional\"")]
            public ptr<big.Int> Qinv;
            [Description("asn1:\"optional,omitempty\"")]
            public slice<pkcs1AdditionalRSAPrime> AdditionalPrimes;
        }

        private partial struct pkcs1AdditionalRSAPrime
        {
            public ptr<big.Int> Prime; // We ignore these values because rsa will calculate them.
            public ptr<big.Int> Exp;
            public ptr<big.Int> Coeff;
        }

        // pkcs1PublicKey reflects the ASN.1 structure of a PKCS#1 public key.
        private partial struct pkcs1PublicKey
        {
            public ptr<big.Int> N;
            public long E;
        }

        // ParsePKCS1PrivateKey returns an RSA private key from its ASN.1 PKCS#1 DER encoded form.
        public static (ref rsa.PrivateKey, error) ParsePKCS1PrivateKey(slice<byte> der)
        {
            pkcs1PrivateKey priv = default;
            var (rest, err) = asn1.Unmarshal(der, ref priv);
            if (len(rest) > 0L)
            {
                return (null, new asn1.SyntaxError(Msg:"trailing data"));
            }
            if (err != null)
            {
                return (null, err);
            }
            if (priv.Version > 1L)
            {
                return (null, errors.New("x509: unsupported private key version"));
            }
            if (priv.N.Sign() <= 0L || priv.D.Sign() <= 0L || priv.P.Sign() <= 0L || priv.Q.Sign() <= 0L)
            {
                return (null, errors.New("x509: private key contains zero or negative value"));
            }
            ptr<rsa.PrivateKey> key = @new<rsa.PrivateKey>();
            key.PublicKey = new rsa.PublicKey(E:priv.E,N:priv.N,);

            key.D = priv.D;
            key.Primes = make_slice<ref big.Int>(2L + len(priv.AdditionalPrimes));
            key.Primes[0L] = priv.P;
            key.Primes[1L] = priv.Q;
            foreach (var (i, a) in priv.AdditionalPrimes)
            {
                if (a.Prime.Sign() <= 0L)
                {
                    return (null, errors.New("x509: private key contains zero or negative prime"));
                }
                key.Primes[i + 2L] = a.Prime; 
                // We ignore the other two values because rsa will calculate
                // them as needed.
            }
            err = key.Validate();
            if (err != null)
            {
                return (null, err);
            }
            key.Precompute();

            return (key, null);
        }

        // MarshalPKCS1PrivateKey converts a private key to ASN.1 DER encoded form.
        public static slice<byte> MarshalPKCS1PrivateKey(ref rsa.PrivateKey key)
        {
            key.Precompute();

            long version = 0L;
            if (len(key.Primes) > 2L)
            {
                version = 1L;
            }
            pkcs1PrivateKey priv = new pkcs1PrivateKey(Version:version,N:key.N,E:key.PublicKey.E,D:key.D,P:key.Primes[0],Q:key.Primes[1],Dp:key.Precomputed.Dp,Dq:key.Precomputed.Dq,Qinv:key.Precomputed.Qinv,);

            priv.AdditionalPrimes = make_slice<pkcs1AdditionalRSAPrime>(len(key.Precomputed.CRTValues));
            foreach (var (i, values) in key.Precomputed.CRTValues)
            {
                priv.AdditionalPrimes[i].Prime = key.Primes[2L + i];
                priv.AdditionalPrimes[i].Exp = values.Exp;
                priv.AdditionalPrimes[i].Coeff = values.Coeff;
            }
            var (b, _) = asn1.Marshal(priv);
            return b;
        }

        // ParsePKCS1PublicKey parses a PKCS#1 public key in ASN.1 DER form.
        public static (ref rsa.PublicKey, error) ParsePKCS1PublicKey(slice<byte> der)
        {
            pkcs1PublicKey pub = default;
            var (rest, err) = asn1.Unmarshal(der, ref pub);
            if (err != null)
            {
                return (null, err);
            }
            if (len(rest) > 0L)
            {
                return (null, new asn1.SyntaxError(Msg:"trailing data"));
            }
            if (pub.N.Sign() <= 0L || pub.E <= 0L)
            {
                return (null, errors.New("x509: public key contains zero or negative value"));
            }
            if (pub.E > 1L << (int)(31L) - 1L)
            {
                return (null, errors.New("x509: public key contains large public exponent"));
            }
            return (ref new rsa.PublicKey(E:pub.E,N:pub.N,), null);
        }

        // MarshalPKCS1PublicKey converts an RSA public key to PKCS#1, ASN.1 DER form.
        public static slice<byte> MarshalPKCS1PublicKey(ref rsa.PublicKey key)
        {
            var (derBytes, _) = asn1.Marshal(new pkcs1PublicKey(N:key.N,E:key.E,));
            return derBytes;
        }
    }
}}
