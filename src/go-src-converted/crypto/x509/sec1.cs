// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package x509 -- go2cs converted at 2020 August 29 08:31:58 UTC
// import "crypto/x509" ==> using x509 = go.crypto.x509_package
// Original source: C:\Go\src\crypto\x509\sec1.go
using ecdsa = go.crypto.ecdsa_package;
using elliptic = go.crypto.elliptic_package;
using asn1 = go.encoding.asn1_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using big = go.math.big_package;
using static go.builtin;
using System.ComponentModel;

namespace go {
namespace crypto
{
    public static partial class x509_package
    {
        private static readonly long ecPrivKeyVersion = 1L;

        // ecPrivateKey reflects an ASN.1 Elliptic Curve Private Key Structure.
        // References:
        //   RFC 5915
        //   SEC1 - http://www.secg.org/sec1-v2.pdf
        // Per RFC 5915 the NamedCurveOID is marked as ASN.1 OPTIONAL, however in
        // most cases it is not.


        // ecPrivateKey reflects an ASN.1 Elliptic Curve Private Key Structure.
        // References:
        //   RFC 5915
        //   SEC1 - http://www.secg.org/sec1-v2.pdf
        // Per RFC 5915 the NamedCurveOID is marked as ASN.1 OPTIONAL, however in
        // most cases it is not.
        private partial struct ecPrivateKey
        {
            public long Version;
            public slice<byte> PrivateKey;
            [Description("asn1:\"optional,explicit,tag:0\"")]
            public asn1.ObjectIdentifier NamedCurveOID;
            [Description("asn1:\"optional,explicit,tag:1\"")]
            public asn1.BitString PublicKey;
        }

        // ParseECPrivateKey parses an ASN.1 Elliptic Curve Private Key Structure.
        public static (ref ecdsa.PrivateKey, error) ParseECPrivateKey(slice<byte> der)
        {
            return parseECPrivateKey(null, der);
        }

        // MarshalECPrivateKey marshals an EC private key into ASN.1, DER format.
        public static (slice<byte>, error) MarshalECPrivateKey(ref ecdsa.PrivateKey key)
        {
            var (oid, ok) = oidFromNamedCurve(key.Curve);
            if (!ok)
            {
                return (null, errors.New("x509: unknown elliptic curve"));
            }
            return marshalECPrivateKeyWithOID(key, oid);
        }

        // marshalECPrivateKey marshals an EC private key into ASN.1, DER format and
        // sets the curve ID to the given OID, or omits it if OID is nil.
        private static (slice<byte>, error) marshalECPrivateKeyWithOID(ref ecdsa.PrivateKey key, asn1.ObjectIdentifier oid)
        {
            var privateKeyBytes = key.D.Bytes();
            var paddedPrivateKey = make_slice<byte>((key.Curve.Params().N.BitLen() + 7L) / 8L);
            copy(paddedPrivateKey[len(paddedPrivateKey) - len(privateKeyBytes)..], privateKeyBytes);

            return asn1.Marshal(new ecPrivateKey(Version:1,PrivateKey:paddedPrivateKey,NamedCurveOID:oid,PublicKey:asn1.BitString{Bytes:elliptic.Marshal(key.Curve,key.X,key.Y)},));
        }

        // parseECPrivateKey parses an ASN.1 Elliptic Curve Private Key Structure.
        // The OID for the named curve may be provided from another source (such as
        // the PKCS8 container) - if it is provided then use this instead of the OID
        // that may exist in the EC private key structure.
        private static (ref ecdsa.PrivateKey, error) parseECPrivateKey(ref asn1.ObjectIdentifier namedCurveOID, slice<byte> der)
        {
            ecPrivateKey privKey = default;
            {
                var (_, err) = asn1.Unmarshal(der, ref privKey);

                if (err != null)
                {
                    return (null, errors.New("x509: failed to parse EC private key: " + err.Error()));
                }

            }
            if (privKey.Version != ecPrivKeyVersion)
            {
                return (null, fmt.Errorf("x509: unknown EC private key version %d", privKey.Version));
            }
            elliptic.Curve curve = default;
            if (namedCurveOID != null)
            {
                curve = namedCurveFromOID(namedCurveOID.Value);
            }
            else
            {
                curve = namedCurveFromOID(privKey.NamedCurveOID);
            }
            if (curve == null)
            {
                return (null, errors.New("x509: unknown elliptic curve"));
            }
            ptr<object> k = @new<big.Int>().SetBytes(privKey.PrivateKey);
            var curveOrder = curve.Params().N;
            if (k.Cmp(curveOrder) >= 0L)
            {
                return (null, errors.New("x509: invalid elliptic curve private key value"));
            }
            ptr<ecdsa.PrivateKey> priv = @new<ecdsa.PrivateKey>();
            priv.Curve = curve;
            priv.D = k;

            var privateKey = make_slice<byte>((curveOrder.BitLen() + 7L) / 8L); 

            // Some private keys have leading zero padding. This is invalid
            // according to [SEC1], but this code will ignore it.
            while (len(privKey.PrivateKey) > len(privateKey))
            {
                if (privKey.PrivateKey[0L] != 0L)
                {
                    return (null, errors.New("x509: invalid private key length"));
                }
                privKey.PrivateKey = privKey.PrivateKey[1L..];
            } 

            // Some private keys remove all leading zeros, this is also invalid
            // according to [SEC1] but since OpenSSL used to do this, we ignore
            // this too.
 

            // Some private keys remove all leading zeros, this is also invalid
            // according to [SEC1] but since OpenSSL used to do this, we ignore
            // this too.
            copy(privateKey[len(privateKey) - len(privKey.PrivateKey)..], privKey.PrivateKey);
            priv.X, priv.Y = curve.ScalarBaseMult(privateKey);

            return (priv, null);
        }
    }
}}
