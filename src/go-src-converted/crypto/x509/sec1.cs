// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package x509 -- go2cs converted at 2020 October 09 04:54:57 UTC
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
        private static readonly long ecPrivKeyVersion = (long)1L;

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

        // ParseECPrivateKey parses an EC private key in SEC 1, ASN.1 DER form.
        //
        // This kind of key is commonly encoded in PEM blocks of type "EC PRIVATE KEY".
        public static (ptr<ecdsa.PrivateKey>, error) ParseECPrivateKey(slice<byte> der)
        {
            ptr<ecdsa.PrivateKey> _p0 = default!;
            error _p0 = default!;

            return _addr_parseECPrivateKey(_addr_null, der)!;
        }

        // MarshalECPrivateKey converts an EC private key to SEC 1, ASN.1 DER form.
        //
        // This kind of key is commonly encoded in PEM blocks of type "EC PRIVATE KEY".
        // For a more flexible key format which is not EC specific, use
        // MarshalPKCS8PrivateKey.
        public static (slice<byte>, error) MarshalECPrivateKey(ptr<ecdsa.PrivateKey> _addr_key)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref ecdsa.PrivateKey key = ref _addr_key.val;

            var (oid, ok) = oidFromNamedCurve(key.Curve);
            if (!ok)
            {
                return (null, error.As(errors.New("x509: unknown elliptic curve"))!);
            }

            return marshalECPrivateKeyWithOID(_addr_key, oid);

        }

        // marshalECPrivateKey marshals an EC private key into ASN.1, DER format and
        // sets the curve ID to the given OID, or omits it if OID is nil.
        private static (slice<byte>, error) marshalECPrivateKeyWithOID(ptr<ecdsa.PrivateKey> _addr_key, asn1.ObjectIdentifier oid)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref ecdsa.PrivateKey key = ref _addr_key.val;

            var privateKey = make_slice<byte>((key.Curve.Params().N.BitLen() + 7L) / 8L);
            return asn1.Marshal(new ecPrivateKey(Version:1,PrivateKey:key.D.FillBytes(privateKey),NamedCurveOID:oid,PublicKey:asn1.BitString{Bytes:elliptic.Marshal(key.Curve,key.X,key.Y)},));
        }

        // parseECPrivateKey parses an ASN.1 Elliptic Curve Private Key Structure.
        // The OID for the named curve may be provided from another source (such as
        // the PKCS8 container) - if it is provided then use this instead of the OID
        // that may exist in the EC private key structure.
        private static (ptr<ecdsa.PrivateKey>, error) parseECPrivateKey(ptr<asn1.ObjectIdentifier> _addr_namedCurveOID, slice<byte> der)
        {
            ptr<ecdsa.PrivateKey> key = default!;
            error err = default!;
            ref asn1.ObjectIdentifier namedCurveOID = ref _addr_namedCurveOID.val;

            ref ecPrivateKey privKey = ref heap(out ptr<ecPrivateKey> _addr_privKey);
            {
                var (_, err) = asn1.Unmarshal(der, _addr_privKey);

                if (err != null)
                {
                    {
                        (_, err) = asn1.Unmarshal(der, addr(new pkcs8()));

                        if (err == null)
                        {
                            return (_addr_null!, error.As(errors.New("x509: failed to parse private key (use ParsePKCS8PrivateKey instead for this key format)"))!);
                        }

                    }

                    {
                        (_, err) = asn1.Unmarshal(der, addr(new pkcs1PrivateKey()));

                        if (err == null)
                        {
                            return (_addr_null!, error.As(errors.New("x509: failed to parse private key (use ParsePKCS1PrivateKey instead for this key format)"))!);
                        }

                    }

                    return (_addr_null!, error.As(errors.New("x509: failed to parse EC private key: " + err.Error()))!);

                }

            }

            if (privKey.Version != ecPrivKeyVersion)
            {
                return (_addr_null!, error.As(fmt.Errorf("x509: unknown EC private key version %d", privKey.Version))!);
            }

            elliptic.Curve curve = default;
            if (namedCurveOID != null)
            {
                curve = namedCurveFromOID(namedCurveOID);
            }
            else
            {
                curve = namedCurveFromOID(privKey.NamedCurveOID);
            }

            if (curve == null)
            {
                return (_addr_null!, error.As(errors.New("x509: unknown elliptic curve"))!);
            }

            ptr<object> k = @new<big.Int>().SetBytes(privKey.PrivateKey);
            var curveOrder = curve.Params().N;
            if (k.Cmp(curveOrder) >= 0L)
            {
                return (_addr_null!, error.As(errors.New("x509: invalid elliptic curve private key value"))!);
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
                    return (_addr_null!, error.As(errors.New("x509: invalid private key length"))!);
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

            return (_addr_priv!, error.As(null!)!);

        }
    }
}}
