// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package x509 -- go2cs converted at 2020 August 29 08:31:43 UTC
// import "crypto/x509" ==> using x509 = go.crypto.x509_package
// Original source: C:\Go\src\crypto\x509\pkcs8.go
using ecdsa = go.crypto.ecdsa_package;
using rsa = go.crypto.rsa_package;
using pkix = go.crypto.x509.pkix_package;
using asn1 = go.encoding.asn1_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class x509_package
    {
        // pkcs8 reflects an ASN.1, PKCS#8 PrivateKey. See
        // ftp://ftp.rsasecurity.com/pub/pkcs/pkcs-8/pkcs-8v1_2.asn
        // and RFC 5208.
        private partial struct pkcs8
        {
            public long Version;
            public pkix.AlgorithmIdentifier Algo;
            public slice<byte> PrivateKey; // optional attributes omitted.
        }

        // ParsePKCS8PrivateKey parses an unencrypted, PKCS#8 private key.
        // See RFC 5208.
        public static (object, error) ParsePKCS8PrivateKey(slice<byte> der)
        {
            pkcs8 privKey = default;
            {
                var (_, err) = asn1.Unmarshal(der, ref privKey);

                if (err != null)
                {
                    return (null, err);
                }

            }

            if (privKey.Algo.Algorithm.Equal(oidPublicKeyRSA)) 
                key, err = ParsePKCS1PrivateKey(privKey.PrivateKey);
                if (err != null)
                {
                    return (null, errors.New("x509: failed to parse RSA private key embedded in PKCS#8: " + err.Error()));
                }
                return (key, null);
            else if (privKey.Algo.Algorithm.Equal(oidPublicKeyECDSA)) 
                var bytes = privKey.Algo.Parameters.FullBytes;
                ptr<object> namedCurveOID = @new<asn1.ObjectIdentifier>();
                {
                    (_, err) = asn1.Unmarshal(bytes, namedCurveOID);

                    if (err != null)
                    {
                        namedCurveOID = null;
                    }

                }
                key, err = parseECPrivateKey(namedCurveOID, privKey.PrivateKey);
                if (err != null)
                {
                    return (null, errors.New("x509: failed to parse EC private key embedded in PKCS#8: " + err.Error()));
                }
                return (key, null);
            else 
                return (null, fmt.Errorf("x509: PKCS#8 wrapping contained private key with unknown algorithm: %v", privKey.Algo.Algorithm));
                    }

        // MarshalPKCS8PrivateKey converts a private key to PKCS#8 encoded form.
        // The following key types are supported: *rsa.PrivateKey, *ecdsa.PublicKey.
        // Unsupported key types result in an error.
        //
        // See RFC 5208.
        public static (slice<byte>, error) MarshalPKCS8PrivateKey(object key)
        {
            pkcs8 privKey = default;

            switch (key.type())
            {
                case ref rsa.PrivateKey k:
                    privKey.Algo = new pkix.AlgorithmIdentifier(Algorithm:oidPublicKeyRSA,Parameters:asn1.NullRawValue,);
                    privKey.PrivateKey = MarshalPKCS1PrivateKey(k);
                    break;
                case ref ecdsa.PrivateKey k:
                    var (oid, ok) = oidFromNamedCurve(k.Curve);
                    if (!ok)
                    {
                        return (null, errors.New("x509: unknown curve while marshalling to PKCS#8"));
                    }
                    var (oidBytes, err) = asn1.Marshal(oid);
                    if (err != null)
                    {
                        return (null, errors.New("x509: failed to marshal curve OID: " + err.Error()));
                    }
                    privKey.Algo = new pkix.AlgorithmIdentifier(Algorithm:oidPublicKeyECDSA,Parameters:asn1.RawValue{FullBytes:oidBytes,},);

                    privKey.PrivateKey, err = marshalECPrivateKeyWithOID(k, null);

                    if (err != null)
                    {
                        return (null, errors.New("x509: failed to marshal EC private key while building PKCS#8: " + err.Error()));
                    }
                    break;
                default:
                {
                    var k = key.type();
                    return (null, fmt.Errorf("x509: unknown key type while marshalling PKCS#8: %T", key));
                    break;
                }

            }

            return asn1.Marshal(privKey);
        }
    }
}}
