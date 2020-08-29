// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package x509 parses X.509-encoded keys and certificates.
//
// On UNIX systems the environment variables SSL_CERT_FILE and SSL_CERT_DIR
// can be used to override the system default locations for the SSL certificate
// file and SSL certificate files directory, respectively.
// package x509 -- go2cs converted at 2020 August 29 08:32:13 UTC
// import "crypto/x509" ==> using x509 = go.crypto.x509_package
// Original source: C:\Go\src\crypto\x509\x509.go
using bytes = go.bytes_package;
using crypto = go.crypto_package;
using dsa = go.crypto.dsa_package;
using ecdsa = go.crypto.ecdsa_package;
using elliptic = go.crypto.elliptic_package;
using rsa = go.crypto.rsa_package;
using _sha1_ = go.crypto.sha1_package;
using _sha256_ = go.crypto.sha256_package;
using _sha512_ = go.crypto.sha512_package;
using pkix = go.crypto.x509.pkix_package;
using asn1 = go.encoding.asn1_package;
using pem = go.encoding.pem_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using big = go.math.big_package;
using net = go.net_package;
using url = go.net.url_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using time = go.time_package;
using utf8 = go.unicode.utf8_package;

using cryptobyte = go.golang_org.x.crypto.cryptobyte_package;
using cryptobyte_asn1 = go.golang_org.x.crypto.cryptobyte.asn1_package;
using static go.builtin;
using System.ComponentModel;
using System;

namespace go {
namespace crypto
{
    public static partial class x509_package
    {
        // pkixPublicKey reflects a PKIX public key structure. See SubjectPublicKeyInfo
        // in RFC 3280.
        private partial struct pkixPublicKey
        {
            public pkix.AlgorithmIdentifier Algo;
            public asn1.BitString BitString;
        }

        // ParsePKIXPublicKey parses a DER encoded public key. These values are
        // typically found in PEM blocks with "BEGIN PUBLIC KEY".
        //
        // Supported key types include RSA, DSA, and ECDSA. Unknown key
        // types result in an error.
        //
        // On success, pub will be of type *rsa.PublicKey, *dsa.PublicKey,
        // or *ecdsa.PublicKey.
        public static (object, error) ParsePKIXPublicKey(slice<byte> derBytes)
        {
            publicKeyInfo pki = default;
            {
                var (rest, err) = asn1.Unmarshal(derBytes, ref pki);

                if (err != null)
                {
                    return (null, err);
                }
                else if (len(rest) != 0L)
                {
                    return (null, errors.New("x509: trailing data after ASN.1 of public-key"));
                }

            }
            var algo = getPublicKeyAlgorithmFromOID(pki.Algorithm.Algorithm);
            if (algo == UnknownPublicKeyAlgorithm)
            {
                return (null, errors.New("x509: unknown public key algorithm"));
            }
            return parsePublicKey(algo, ref pki);
        }

        private static (slice<byte>, pkix.AlgorithmIdentifier, error) marshalPublicKey(object pub)
        {
            switch (pub.type())
            {
                case ref rsa.PublicKey pub:
                    publicKeyBytes, err = asn1.Marshal(new pkcs1PublicKey(N:pub.N,E:pub.E,));
                    if (err != null)
                    {
                        return (null, new pkix.AlgorithmIdentifier(), err);
                    }
                    publicKeyAlgorithm.Algorithm = oidPublicKeyRSA; 
                    // This is a NULL parameters value which is required by
                    // https://tools.ietf.org/html/rfc3279#section-2.3.1.
                    publicKeyAlgorithm.Parameters = asn1.NullRawValue;
                    break;
                case ref ecdsa.PublicKey pub:
                    publicKeyBytes = elliptic.Marshal(pub.Curve, pub.X, pub.Y);
                    var (oid, ok) = oidFromNamedCurve(pub.Curve);
                    if (!ok)
                    {
                        return (null, new pkix.AlgorithmIdentifier(), errors.New("x509: unsupported elliptic curve"));
                    }
                    publicKeyAlgorithm.Algorithm = oidPublicKeyECDSA;
                    slice<byte> paramBytes = default;
                    paramBytes, err = asn1.Marshal(oid);
                    if (err != null)
                    {
                        return;
                    }
                    publicKeyAlgorithm.Parameters.FullBytes = paramBytes;
                    break;
                default:
                {
                    var pub = pub.type();
                    return (null, new pkix.AlgorithmIdentifier(), errors.New("x509: only RSA and ECDSA public keys supported"));
                    break;
                }

            }

            return (publicKeyBytes, publicKeyAlgorithm, null);
        }

        // MarshalPKIXPublicKey serialises a public key to DER-encoded PKIX format.
        public static (slice<byte>, error) MarshalPKIXPublicKey(object pub)
        {
            slice<byte> publicKeyBytes = default;
            pkix.AlgorithmIdentifier publicKeyAlgorithm = default;
            error err = default;

            publicKeyBytes, publicKeyAlgorithm, err = marshalPublicKey(pub);

            if (err != null)
            {
                return (null, err);
            }
            pkixPublicKey pkix = new pkixPublicKey(Algo:publicKeyAlgorithm,BitString:asn1.BitString{Bytes:publicKeyBytes,BitLength:8*len(publicKeyBytes),},);

            var (ret, _) = asn1.Marshal(pkix);
            return (ret, null);
        }

        // These structures reflect the ASN.1 structure of X.509 certificates.:

        private partial struct certificate
        {
            public asn1.RawContent Raw;
            public tbsCertificate TBSCertificate;
            public pkix.AlgorithmIdentifier SignatureAlgorithm;
            public asn1.BitString SignatureValue;
        }

        private partial struct tbsCertificate
        {
            public asn1.RawContent Raw;
            [Description("asn1:\"optional,explicit,default:0,tag:0\"")]
            public long Version;
            public ptr<big.Int> SerialNumber;
            public pkix.AlgorithmIdentifier SignatureAlgorithm;
            public asn1.RawValue Issuer;
            public validity Validity;
            public asn1.RawValue Subject;
            public publicKeyInfo PublicKey;
            [Description("asn1:\"optional,tag:1\"")]
            public asn1.BitString UniqueId;
            [Description("asn1:\"optional,tag:2\"")]
            public asn1.BitString SubjectUniqueId;
            [Description("asn1:\"optional,explicit,tag:3\"")]
            public slice<pkix.Extension> Extensions;
        }

        private partial struct dsaAlgorithmParameters
        {
            public ptr<big.Int> P;
            public ptr<big.Int> Q;
            public ptr<big.Int> G;
        }

        private partial struct dsaSignature
        {
            public ptr<big.Int> R;
            public ptr<big.Int> S;
        }

        private partial struct ecdsaSignature // : dsaSignature
        {
        }

        private partial struct validity
        {
            public time.Time NotBefore;
            public time.Time NotAfter;
        }

        private partial struct publicKeyInfo
        {
            public asn1.RawContent Raw;
            public pkix.AlgorithmIdentifier Algorithm;
            public asn1.BitString PublicKey;
        }

        // RFC 5280,  4.2.1.1
        private partial struct authKeyId
        {
            [Description("asn1:\"optional,tag:0\"")]
            public slice<byte> Id;
        }

        public partial struct SignatureAlgorithm // : long
        {
        }

        public static readonly SignatureAlgorithm UnknownSignatureAlgorithm = iota;
        public static readonly var MD2WithRSA = 0;
        public static readonly var MD5WithRSA = 1;
        public static readonly var SHA1WithRSA = 2;
        public static readonly var SHA256WithRSA = 3;
        public static readonly var SHA384WithRSA = 4;
        public static readonly var SHA512WithRSA = 5;
        public static readonly var DSAWithSHA1 = 6;
        public static readonly var DSAWithSHA256 = 7;
        public static readonly var ECDSAWithSHA1 = 8;
        public static readonly var ECDSAWithSHA256 = 9;
        public static readonly var ECDSAWithSHA384 = 10;
        public static readonly var ECDSAWithSHA512 = 11;
        public static readonly var SHA256WithRSAPSS = 12;
        public static readonly var SHA384WithRSAPSS = 13;
        public static readonly var SHA512WithRSAPSS = 14;

        public static bool isRSAPSS(this SignatureAlgorithm algo)
        {

            if (algo == SHA256WithRSAPSS || algo == SHA384WithRSAPSS || algo == SHA512WithRSAPSS) 
                return true;
            else 
                return false;
                    }

        public static @string String(this SignatureAlgorithm algo)
        {
            foreach (var (_, details) in signatureAlgorithmDetails)
            {
                if (details.algo == algo)
                {
                    return details.name;
                }
            }
            return strconv.Itoa(int(algo));
        }

        public partial struct PublicKeyAlgorithm // : long
        {
        }

        public static readonly PublicKeyAlgorithm UnknownPublicKeyAlgorithm = iota;
        public static readonly var RSA = 0;
        public static readonly var DSA = 1;
        public static readonly var ECDSA = 2;

        private static array<@string> publicKeyAlgoName = new array<@string>(InitKeyedValues<@string>((RSA, "RSA"), (DSA, "DSA"), (ECDSA, "ECDSA")));

        public static @string String(this PublicKeyAlgorithm algo)
        {
            if (0L < algo && int(algo) < len(publicKeyAlgoName))
            {
                return publicKeyAlgoName[algo];
            }
            return strconv.Itoa(int(algo));
        }

        // OIDs for signature algorithms
        //
        // pkcs-1 OBJECT IDENTIFIER ::= {
        //    iso(1) member-body(2) us(840) rsadsi(113549) pkcs(1) 1 }
        //
        //
        // RFC 3279 2.2.1 RSA Signature Algorithms
        //
        // md2WithRSAEncryption OBJECT IDENTIFIER ::= { pkcs-1 2 }
        //
        // md5WithRSAEncryption OBJECT IDENTIFIER ::= { pkcs-1 4 }
        //
        // sha-1WithRSAEncryption OBJECT IDENTIFIER ::= { pkcs-1 5 }
        //
        // dsaWithSha1 OBJECT IDENTIFIER ::= {
        //    iso(1) member-body(2) us(840) x9-57(10040) x9cm(4) 3 }
        //
        // RFC 3279 2.2.3 ECDSA Signature Algorithm
        //
        // ecdsa-with-SHA1 OBJECT IDENTIFIER ::= {
        //       iso(1) member-body(2) us(840) ansi-x962(10045)
        //    signatures(4) ecdsa-with-SHA1(1)}
        //
        //
        // RFC 4055 5 PKCS #1 Version 1.5
        //
        // sha256WithRSAEncryption OBJECT IDENTIFIER ::= { pkcs-1 11 }
        //
        // sha384WithRSAEncryption OBJECT IDENTIFIER ::= { pkcs-1 12 }
        //
        // sha512WithRSAEncryption OBJECT IDENTIFIER ::= { pkcs-1 13 }
        //
        //
        // RFC 5758 3.1 DSA Signature Algorithms
        //
        // dsaWithSha256 OBJECT IDENTIFIER ::= {
        //    joint-iso-ccitt(2) country(16) us(840) organization(1) gov(101)
        //    csor(3) algorithms(4) id-dsa-with-sha2(3) 2}
        //
        // RFC 5758 3.2 ECDSA Signature Algorithm
        //
        // ecdsa-with-SHA256 OBJECT IDENTIFIER ::= { iso(1) member-body(2)
        //    us(840) ansi-X9-62(10045) signatures(4) ecdsa-with-SHA2(3) 2 }
        //
        // ecdsa-with-SHA384 OBJECT IDENTIFIER ::= { iso(1) member-body(2)
        //    us(840) ansi-X9-62(10045) signatures(4) ecdsa-with-SHA2(3) 3 }
        //
        // ecdsa-with-SHA512 OBJECT IDENTIFIER ::= { iso(1) member-body(2)
        //    us(840) ansi-X9-62(10045) signatures(4) ecdsa-with-SHA2(3) 4 }

        private static asn1.ObjectIdentifier oidSignatureMD2WithRSA = new asn1.ObjectIdentifier(1,2,840,113549,1,1,2);        private static asn1.ObjectIdentifier oidSignatureMD5WithRSA = new asn1.ObjectIdentifier(1,2,840,113549,1,1,4);        private static asn1.ObjectIdentifier oidSignatureSHA1WithRSA = new asn1.ObjectIdentifier(1,2,840,113549,1,1,5);        private static asn1.ObjectIdentifier oidSignatureSHA256WithRSA = new asn1.ObjectIdentifier(1,2,840,113549,1,1,11);        private static asn1.ObjectIdentifier oidSignatureSHA384WithRSA = new asn1.ObjectIdentifier(1,2,840,113549,1,1,12);        private static asn1.ObjectIdentifier oidSignatureSHA512WithRSA = new asn1.ObjectIdentifier(1,2,840,113549,1,1,13);        private static asn1.ObjectIdentifier oidSignatureRSAPSS = new asn1.ObjectIdentifier(1,2,840,113549,1,1,10);        private static asn1.ObjectIdentifier oidSignatureDSAWithSHA1 = new asn1.ObjectIdentifier(1,2,840,10040,4,3);        private static asn1.ObjectIdentifier oidSignatureDSAWithSHA256 = new asn1.ObjectIdentifier(2,16,840,1,101,3,4,3,2);        private static asn1.ObjectIdentifier oidSignatureECDSAWithSHA1 = new asn1.ObjectIdentifier(1,2,840,10045,4,1);        private static asn1.ObjectIdentifier oidSignatureECDSAWithSHA256 = new asn1.ObjectIdentifier(1,2,840,10045,4,3,2);        private static asn1.ObjectIdentifier oidSignatureECDSAWithSHA384 = new asn1.ObjectIdentifier(1,2,840,10045,4,3,3);        private static asn1.ObjectIdentifier oidSignatureECDSAWithSHA512 = new asn1.ObjectIdentifier(1,2,840,10045,4,3,4);        private static asn1.ObjectIdentifier oidSHA256 = new asn1.ObjectIdentifier(2,16,840,1,101,3,4,2,1);        private static asn1.ObjectIdentifier oidSHA384 = new asn1.ObjectIdentifier(2,16,840,1,101,3,4,2,2);        private static asn1.ObjectIdentifier oidSHA512 = new asn1.ObjectIdentifier(2,16,840,1,101,3,4,2,3);        private static asn1.ObjectIdentifier oidMGF1 = new asn1.ObjectIdentifier(1,2,840,113549,1,1,8);        private static asn1.ObjectIdentifier oidISOSignatureSHA1WithRSA = new asn1.ObjectIdentifier(1,3,14,3,2,29);



        // pssParameters reflects the parameters in an AlgorithmIdentifier that
        // specifies RSA PSS. See https://tools.ietf.org/html/rfc3447#appendix-A.2.3
        private partial struct pssParameters
        {
            [Description("asn1:\"explicit,tag:0\"")]
            public pkix.AlgorithmIdentifier Hash;
            [Description("asn1:\"explicit,tag:1\"")]
            public pkix.AlgorithmIdentifier MGF;
            [Description("asn1:\"explicit,tag:2\"")]
            public long SaltLength;
            [Description("asn1:\"optional,explicit,tag:3,default:1\"")]
            public long TrailerField;
        }

        // rsaPSSParameters returns an asn1.RawValue suitable for use as the Parameters
        // in an AlgorithmIdentifier that specifies RSA PSS.
        private static asn1.RawValue rsaPSSParameters(crypto.Hash hashFunc) => func((_, panic, __) =>
        {
            asn1.ObjectIdentifier hashOID = default;


            if (hashFunc == crypto.SHA256) 
                hashOID = oidSHA256;
            else if (hashFunc == crypto.SHA384) 
                hashOID = oidSHA384;
            else if (hashFunc == crypto.SHA512) 
                hashOID = oidSHA512;
                        pssParameters @params = new pssParameters(Hash:pkix.AlgorithmIdentifier{Algorithm:hashOID,Parameters:asn1.NullRawValue,},MGF:pkix.AlgorithmIdentifier{Algorithm:oidMGF1,},SaltLength:hashFunc.Size(),TrailerField:1,);

            pkix.AlgorithmIdentifier mgf1Params = new pkix.AlgorithmIdentifier(Algorithm:hashOID,Parameters:asn1.NullRawValue,);

            error err = default;
            @params.MGF.Parameters.FullBytes, err = asn1.Marshal(mgf1Params);
            if (err != null)
            {
                panic(err);
            }
            var (serialized, err) = asn1.Marshal(params);
            if (err != null)
            {
                panic(err);
            }
            return new asn1.RawValue(FullBytes:serialized);
        });

        private static SignatureAlgorithm getSignatureAlgorithmFromAI(pkix.AlgorithmIdentifier ai)
        {
            if (!ai.Algorithm.Equal(oidSignatureRSAPSS))
            {
                foreach (var (_, details) in signatureAlgorithmDetails)
                {
                    if (ai.Algorithm.Equal(details.oid))
                    {
                        return details.algo;
                    }
                }
                return UnknownSignatureAlgorithm;
            } 

            // RSA PSS is special because it encodes important parameters
            // in the Parameters.
            pssParameters @params = default;
            {
                var (_, err) = asn1.Unmarshal(ai.Parameters.FullBytes, ref params);

                if (err != null)
                {
                    return UnknownSignatureAlgorithm;
                }

            }

            pkix.AlgorithmIdentifier mgf1HashFunc = default;
            {
                (_, err) = asn1.Unmarshal(@params.MGF.Parameters.FullBytes, ref mgf1HashFunc);

                if (err != null)
                {
                    return UnknownSignatureAlgorithm;
                } 

                // PSS is greatly overburdened with options. This code forces
                // them into three buckets by requiring that the MGF1 hash
                // function always match the message hash function (as
                // recommended in
                // https://tools.ietf.org/html/rfc3447#section-8.1), that the
                // salt length matches the hash length, and that the trailer
                // field has the default value.

            } 

            // PSS is greatly overburdened with options. This code forces
            // them into three buckets by requiring that the MGF1 hash
            // function always match the message hash function (as
            // recommended in
            // https://tools.ietf.org/html/rfc3447#section-8.1), that the
            // salt length matches the hash length, and that the trailer
            // field has the default value.
            if (!bytes.Equal(@params.Hash.Parameters.FullBytes, asn1.NullBytes) || !@params.MGF.Algorithm.Equal(oidMGF1) || !mgf1HashFunc.Algorithm.Equal(@params.Hash.Algorithm) || !bytes.Equal(mgf1HashFunc.Parameters.FullBytes, asn1.NullBytes) || @params.TrailerField != 1L)
            {
                return UnknownSignatureAlgorithm;
            }

            if (@params.Hash.Algorithm.Equal(oidSHA256) && @params.SaltLength == 32L) 
                return SHA256WithRSAPSS;
            else if (@params.Hash.Algorithm.Equal(oidSHA384) && @params.SaltLength == 48L) 
                return SHA384WithRSAPSS;
            else if (@params.Hash.Algorithm.Equal(oidSHA512) && @params.SaltLength == 64L) 
                return SHA512WithRSAPSS;
                        return UnknownSignatureAlgorithm;
        }

        // RFC 3279, 2.3 Public Key Algorithms
        //
        // pkcs-1 OBJECT IDENTIFIER ::== { iso(1) member-body(2) us(840)
        //    rsadsi(113549) pkcs(1) 1 }
        //
        // rsaEncryption OBJECT IDENTIFIER ::== { pkcs1-1 1 }
        //
        // id-dsa OBJECT IDENTIFIER ::== { iso(1) member-body(2) us(840)
        //    x9-57(10040) x9cm(4) 1 }
        //
        // RFC 5480, 2.1.1 Unrestricted Algorithm Identifier and Parameters
        //
        // id-ecPublicKey OBJECT IDENTIFIER ::= {
        //       iso(1) member-body(2) us(840) ansi-X9-62(10045) keyType(2) 1 }
        private static asn1.ObjectIdentifier oidPublicKeyRSA = new asn1.ObjectIdentifier(1,2,840,113549,1,1,1);        private static asn1.ObjectIdentifier oidPublicKeyDSA = new asn1.ObjectIdentifier(1,2,840,10040,4,1);        private static asn1.ObjectIdentifier oidPublicKeyECDSA = new asn1.ObjectIdentifier(1,2,840,10045,2,1);

        private static PublicKeyAlgorithm getPublicKeyAlgorithmFromOID(asn1.ObjectIdentifier oid)
        {

            if (oid.Equal(oidPublicKeyRSA)) 
                return RSA;
            else if (oid.Equal(oidPublicKeyDSA)) 
                return DSA;
            else if (oid.Equal(oidPublicKeyECDSA)) 
                return ECDSA;
                        return UnknownPublicKeyAlgorithm;
        }

        // RFC 5480, 2.1.1.1. Named Curve
        //
        // secp224r1 OBJECT IDENTIFIER ::= {
        //   iso(1) identified-organization(3) certicom(132) curve(0) 33 }
        //
        // secp256r1 OBJECT IDENTIFIER ::= {
        //   iso(1) member-body(2) us(840) ansi-X9-62(10045) curves(3)
        //   prime(1) 7 }
        //
        // secp384r1 OBJECT IDENTIFIER ::= {
        //   iso(1) identified-organization(3) certicom(132) curve(0) 34 }
        //
        // secp521r1 OBJECT IDENTIFIER ::= {
        //   iso(1) identified-organization(3) certicom(132) curve(0) 35 }
        //
        // NB: secp256r1 is equivalent to prime256v1
        private static asn1.ObjectIdentifier oidNamedCurveP224 = new asn1.ObjectIdentifier(1,3,132,0,33);        private static asn1.ObjectIdentifier oidNamedCurveP256 = new asn1.ObjectIdentifier(1,2,840,10045,3,1,7);        private static asn1.ObjectIdentifier oidNamedCurveP384 = new asn1.ObjectIdentifier(1,3,132,0,34);        private static asn1.ObjectIdentifier oidNamedCurveP521 = new asn1.ObjectIdentifier(1,3,132,0,35);

        private static elliptic.Curve namedCurveFromOID(asn1.ObjectIdentifier oid)
        {

            if (oid.Equal(oidNamedCurveP224)) 
                return elliptic.P224();
            else if (oid.Equal(oidNamedCurveP256)) 
                return elliptic.P256();
            else if (oid.Equal(oidNamedCurveP384)) 
                return elliptic.P384();
            else if (oid.Equal(oidNamedCurveP521)) 
                return elliptic.P521();
                        return null;
        }

        private static (asn1.ObjectIdentifier, bool) oidFromNamedCurve(elliptic.Curve curve)
        {

            if (curve == elliptic.P224()) 
                return (oidNamedCurveP224, true);
            else if (curve == elliptic.P256()) 
                return (oidNamedCurveP256, true);
            else if (curve == elliptic.P384()) 
                return (oidNamedCurveP384, true);
            else if (curve == elliptic.P521()) 
                return (oidNamedCurveP521, true);
                        return (null, false);
        }

        // KeyUsage represents the set of actions that are valid for a given key. It's
        // a bitmap of the KeyUsage* constants.
        public partial struct KeyUsage // : long
        {
        }

        public static readonly KeyUsage KeyUsageDigitalSignature = 1L << (int)(iota);
        public static readonly var KeyUsageContentCommitment = 0;
        public static readonly var KeyUsageKeyEncipherment = 1;
        public static readonly var KeyUsageDataEncipherment = 2;
        public static readonly var KeyUsageKeyAgreement = 3;
        public static readonly var KeyUsageCertSign = 4;
        public static readonly var KeyUsageCRLSign = 5;
        public static readonly var KeyUsageEncipherOnly = 6;
        public static readonly var KeyUsageDecipherOnly = 7;

        // RFC 5280, 4.2.1.12  Extended Key Usage
        //
        // anyExtendedKeyUsage OBJECT IDENTIFIER ::= { id-ce-extKeyUsage 0 }
        //
        // id-kp OBJECT IDENTIFIER ::= { id-pkix 3 }
        //
        // id-kp-serverAuth             OBJECT IDENTIFIER ::= { id-kp 1 }
        // id-kp-clientAuth             OBJECT IDENTIFIER ::= { id-kp 2 }
        // id-kp-codeSigning            OBJECT IDENTIFIER ::= { id-kp 3 }
        // id-kp-emailProtection        OBJECT IDENTIFIER ::= { id-kp 4 }
        // id-kp-timeStamping           OBJECT IDENTIFIER ::= { id-kp 8 }
        // id-kp-OCSPSigning            OBJECT IDENTIFIER ::= { id-kp 9 }
        private static asn1.ObjectIdentifier oidExtKeyUsageAny = new asn1.ObjectIdentifier(2,5,29,37,0);        private static asn1.ObjectIdentifier oidExtKeyUsageServerAuth = new asn1.ObjectIdentifier(1,3,6,1,5,5,7,3,1);        private static asn1.ObjectIdentifier oidExtKeyUsageClientAuth = new asn1.ObjectIdentifier(1,3,6,1,5,5,7,3,2);        private static asn1.ObjectIdentifier oidExtKeyUsageCodeSigning = new asn1.ObjectIdentifier(1,3,6,1,5,5,7,3,3);        private static asn1.ObjectIdentifier oidExtKeyUsageEmailProtection = new asn1.ObjectIdentifier(1,3,6,1,5,5,7,3,4);        private static asn1.ObjectIdentifier oidExtKeyUsageIPSECEndSystem = new asn1.ObjectIdentifier(1,3,6,1,5,5,7,3,5);        private static asn1.ObjectIdentifier oidExtKeyUsageIPSECTunnel = new asn1.ObjectIdentifier(1,3,6,1,5,5,7,3,6);        private static asn1.ObjectIdentifier oidExtKeyUsageIPSECUser = new asn1.ObjectIdentifier(1,3,6,1,5,5,7,3,7);        private static asn1.ObjectIdentifier oidExtKeyUsageTimeStamping = new asn1.ObjectIdentifier(1,3,6,1,5,5,7,3,8);        private static asn1.ObjectIdentifier oidExtKeyUsageOCSPSigning = new asn1.ObjectIdentifier(1,3,6,1,5,5,7,3,9);        private static asn1.ObjectIdentifier oidExtKeyUsageMicrosoftServerGatedCrypto = new asn1.ObjectIdentifier(1,3,6,1,4,1,311,10,3,3);        private static asn1.ObjectIdentifier oidExtKeyUsageNetscapeServerGatedCrypto = new asn1.ObjectIdentifier(2,16,840,1,113730,4,1);        private static asn1.ObjectIdentifier oidExtKeyUsageMicrosoftCommercialCodeSigning = new asn1.ObjectIdentifier(1,3,6,1,4,1,311,2,1,22);        private static asn1.ObjectIdentifier oidExtKeyUsageMicrosoftKernelCodeSigning = new asn1.ObjectIdentifier(1,3,6,1,4,1,311,61,1,1);

        // ExtKeyUsage represents an extended set of actions that are valid for a given key.
        // Each of the ExtKeyUsage* constants define a unique action.
        public partial struct ExtKeyUsage // : long
        {
        }

        public static readonly ExtKeyUsage ExtKeyUsageAny = iota;
        public static readonly var ExtKeyUsageServerAuth = 0;
        public static readonly var ExtKeyUsageClientAuth = 1;
        public static readonly var ExtKeyUsageCodeSigning = 2;
        public static readonly var ExtKeyUsageEmailProtection = 3;
        public static readonly var ExtKeyUsageIPSECEndSystem = 4;
        public static readonly var ExtKeyUsageIPSECTunnel = 5;
        public static readonly var ExtKeyUsageIPSECUser = 6;
        public static readonly var ExtKeyUsageTimeStamping = 7;
        public static readonly var ExtKeyUsageOCSPSigning = 8;
        public static readonly var ExtKeyUsageMicrosoftServerGatedCrypto = 9;
        public static readonly var ExtKeyUsageNetscapeServerGatedCrypto = 10;
        public static readonly var ExtKeyUsageMicrosoftCommercialCodeSigning = 11;
        public static readonly var ExtKeyUsageMicrosoftKernelCodeSigning = 12;

        // extKeyUsageOIDs contains the mapping between an ExtKeyUsage and its OID.


        private static (ExtKeyUsage, bool) extKeyUsageFromOID(asn1.ObjectIdentifier oid)
        {
            foreach (var (_, pair) in extKeyUsageOIDs)
            {
                if (oid.Equal(pair.oid))
                {
                    return (pair.extKeyUsage, true);
                }
            }
            return;
        }

        private static (asn1.ObjectIdentifier, bool) oidFromExtKeyUsage(ExtKeyUsage eku)
        {
            foreach (var (_, pair) in extKeyUsageOIDs)
            {
                if (eku == pair.extKeyUsage)
                {
                    return (pair.oid, true);
                }
            }
            return;
        }

        // A Certificate represents an X.509 certificate.
        public partial struct Certificate
        {
            public slice<byte> Raw; // Complete ASN.1 DER content (certificate, signature algorithm and signature).
            public slice<byte> RawTBSCertificate; // Certificate part of raw ASN.1 DER content.
            public slice<byte> RawSubjectPublicKeyInfo; // DER encoded SubjectPublicKeyInfo.
            public slice<byte> RawSubject; // DER encoded Subject
            public slice<byte> RawIssuer; // DER encoded Issuer

            public slice<byte> Signature;
            public SignatureAlgorithm SignatureAlgorithm;
            public PublicKeyAlgorithm PublicKeyAlgorithm;
            public long Version;
            public ptr<big.Int> SerialNumber;
            public pkix.Name Issuer;
            public pkix.Name Subject;
            public time.Time NotBefore; // Validity bounds.
            public time.Time NotAfter; // Validity bounds.
            public KeyUsage KeyUsage; // Extensions contains raw X.509 extensions. When parsing certificates,
// this can be used to extract non-critical extensions that are not
// parsed by this package. When marshaling certificates, the Extensions
// field is ignored, see ExtraExtensions.
            public slice<pkix.Extension> Extensions; // ExtraExtensions contains extensions to be copied, raw, into any
// marshaled certificates. Values override any extensions that would
// otherwise be produced based on the other fields. The ExtraExtensions
// field is not populated when parsing certificates, see Extensions.
            public slice<pkix.Extension> ExtraExtensions; // UnhandledCriticalExtensions contains a list of extension IDs that
// were not (fully) processed when parsing. Verify will fail if this
// slice is non-empty, unless verification is delegated to an OS
// library which understands all the critical extensions.
//
// Users can access these extensions using Extensions and can remove
// elements from this slice if they believe that they have been
// handled.
            public slice<asn1.ObjectIdentifier> UnhandledCriticalExtensions;
            public slice<ExtKeyUsage> ExtKeyUsage; // Sequence of extended key usages.
            public slice<asn1.ObjectIdentifier> UnknownExtKeyUsage; // Encountered extended key usages unknown to this package.

// BasicConstraintsValid indicates whether IsCA, MaxPathLen,
// and MaxPathLenZero are valid.
            public bool BasicConstraintsValid;
            public bool IsCA; // MaxPathLen and MaxPathLenZero indicate the presence and
// value of the BasicConstraints' "pathLenConstraint".
//
// When parsing a certificate, a positive non-zero MaxPathLen
// means that the field was specified, -1 means it was unset,
// and MaxPathLenZero being true mean that the field was
// explicitly set to zero. The case of MaxPathLen==0 with MaxPathLenZero==false
// should be treated equivalent to -1 (unset).
//
// When generating a certificate, an unset pathLenConstraint
// can be requested with either MaxPathLen == -1 or using the
// zero value for both MaxPathLen and MaxPathLenZero.
            public long MaxPathLen; // MaxPathLenZero indicates that BasicConstraintsValid==true
// and MaxPathLen==0 should be interpreted as an actual
// maximum path length of zero. Otherwise, that combination is
// interpreted as MaxPathLen not being set.
            public bool MaxPathLenZero;
            public slice<byte> SubjectKeyId;
            public slice<byte> AuthorityKeyId; // RFC 5280, 4.2.2.1 (Authority Information Access)
            public slice<@string> OCSPServer;
            public slice<@string> IssuingCertificateURL; // Subject Alternate Name values. (Note that these values may not be valid
// if invalid values were contained within a parsed certificate. For
// example, an element of DNSNames may not be a valid DNS domain name.)
            public slice<@string> DNSNames;
            public slice<@string> EmailAddresses;
            public slice<net.IP> IPAddresses;
            public slice<ref url.URL> URIs; // Name constraints
            public bool PermittedDNSDomainsCritical; // if true then the name constraints are marked critical.
            public slice<@string> PermittedDNSDomains;
            public slice<@string> ExcludedDNSDomains;
            public slice<ref net.IPNet> PermittedIPRanges;
            public slice<ref net.IPNet> ExcludedIPRanges;
            public slice<@string> PermittedEmailAddresses;
            public slice<@string> ExcludedEmailAddresses;
            public slice<@string> PermittedURIDomains;
            public slice<@string> ExcludedURIDomains; // CRL Distribution Points
            public slice<@string> CRLDistributionPoints;
            public slice<asn1.ObjectIdentifier> PolicyIdentifiers;
        }

        // ErrUnsupportedAlgorithm results from attempting to perform an operation that
        // involves algorithms that are not currently implemented.
        public static var ErrUnsupportedAlgorithm = errors.New("x509: cannot verify signature: algorithm unimplemented");

        // An InsecureAlgorithmError
        public partial struct InsecureAlgorithmError // : SignatureAlgorithm
        {
        }

        public static @string Error(this InsecureAlgorithmError e)
        {
            return fmt.Sprintf("x509: cannot verify signature: insecure algorithm %v", SignatureAlgorithm(e));
        }

        // ConstraintViolationError results when a requested usage is not permitted by
        // a certificate. For example: checking a signature when the public key isn't a
        // certificate signing key.
        public partial struct ConstraintViolationError
        {
        }

        public static @string Error(this ConstraintViolationError _p0)
        {
            return "x509: invalid signature: parent certificate cannot sign this kind of certificate";
        }

        private static bool Equal(this ref Certificate c, ref Certificate other)
        {
            return bytes.Equal(c.Raw, other.Raw);
        }

        private static bool hasSANExtension(this ref Certificate c)
        {
            return oidInExtensions(oidExtensionSubjectAltName, c.Extensions);
        }

        // Entrust have a broken root certificate (CN=Entrust.net Certification
        // Authority (2048)) which isn't marked as a CA certificate and is thus invalid
        // according to PKIX.
        // We recognise this certificate by its SubjectPublicKeyInfo and exempt it
        // from the Basic Constraints requirement.
        // See http://www.entrust.net/knowledge-base/technote.cfm?tn=7869
        //
        // TODO(agl): remove this hack once their reissued root is sufficiently
        // widespread.
        private static byte entrustBrokenSPKI = new slice<byte>(new byte[] { 0x30, 0x82, 0x01, 0x22, 0x30, 0x0d, 0x06, 0x09, 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x01, 0x01, 0x05, 0x00, 0x03, 0x82, 0x01, 0x0f, 0x00, 0x30, 0x82, 0x01, 0x0a, 0x02, 0x82, 0x01, 0x01, 0x00, 0x97, 0xa3, 0x2d, 0x3c, 0x9e, 0xde, 0x05, 0xda, 0x13, 0xc2, 0x11, 0x8d, 0x9d, 0x8e, 0xe3, 0x7f, 0xc7, 0x4b, 0x7e, 0x5a, 0x9f, 0xb3, 0xff, 0x62, 0xab, 0x73, 0xc8, 0x28, 0x6b, 0xba, 0x10, 0x64, 0x82, 0x87, 0x13, 0xcd, 0x57, 0x18, 0xff, 0x28, 0xce, 0xc0, 0xe6, 0x0e, 0x06, 0x91, 0x50, 0x29, 0x83, 0xd1, 0xf2, 0xc3, 0x2a, 0xdb, 0xd8, 0xdb, 0x4e, 0x04, 0xcc, 0x00, 0xeb, 0x8b, 0xb6, 0x96, 0xdc, 0xbc, 0xaa, 0xfa, 0x52, 0x77, 0x04, 0xc1, 0xdb, 0x19, 0xe4, 0xae, 0x9c, 0xfd, 0x3c, 0x8b, 0x03, 0xef, 0x4d, 0xbc, 0x1a, 0x03, 0x65, 0xf9, 0xc1, 0xb1, 0x3f, 0x72, 0x86, 0xf2, 0x38, 0xaa, 0x19, 0xae, 0x10, 0x88, 0x78, 0x28, 0xda, 0x75, 0xc3, 0x3d, 0x02, 0x82, 0x02, 0x9c, 0xb9, 0xc1, 0x65, 0x77, 0x76, 0x24, 0x4c, 0x98, 0xf7, 0x6d, 0x31, 0x38, 0xfb, 0xdb, 0xfe, 0xdb, 0x37, 0x02, 0x76, 0xa1, 0x18, 0x97, 0xa6, 0xcc, 0xde, 0x20, 0x09, 0x49, 0x36, 0x24, 0x69, 0x42, 0xf6, 0xe4, 0x37, 0x62, 0xf1, 0x59, 0x6d, 0xa9, 0x3c, 0xed, 0x34, 0x9c, 0xa3, 0x8e, 0xdb, 0xdc, 0x3a, 0xd7, 0xf7, 0x0a, 0x6f, 0xef, 0x2e, 0xd8, 0xd5, 0x93, 0x5a, 0x7a, 0xed, 0x08, 0x49, 0x68, 0xe2, 0x41, 0xe3, 0x5a, 0x90, 0xc1, 0x86, 0x55, 0xfc, 0x51, 0x43, 0x9d, 0xe0, 0xb2, 0xc4, 0x67, 0xb4, 0xcb, 0x32, 0x31, 0x25, 0xf0, 0x54, 0x9f, 0x4b, 0xd1, 0x6f, 0xdb, 0xd4, 0xdd, 0xfc, 0xaf, 0x5e, 0x6c, 0x78, 0x90, 0x95, 0xde, 0xca, 0x3a, 0x48, 0xb9, 0x79, 0x3c, 0x9b, 0x19, 0xd6, 0x75, 0x05, 0xa0, 0xf9, 0x88, 0xd7, 0xc1, 0xe8, 0xa5, 0x09, 0xe4, 0x1a, 0x15, 0xdc, 0x87, 0x23, 0xaa, 0xb2, 0x75, 0x8c, 0x63, 0x25, 0x87, 0xd8, 0xf8, 0x3d, 0xa6, 0xc2, 0xcc, 0x66, 0xff, 0xa5, 0x66, 0x68, 0x55, 0x02, 0x03, 0x01, 0x00, 0x01 });

        // CheckSignatureFrom verifies that the signature on c is a valid signature
        // from parent.
        private static error CheckSignatureFrom(this ref Certificate c, ref Certificate parent)
        { 
            // RFC 5280, 4.2.1.9:
            // "If the basic constraints extension is not present in a version 3
            // certificate, or the extension is present but the cA boolean is not
            // asserted, then the certified public key MUST NOT be used to verify
            // certificate signatures."
            // (except for Entrust, see comment above entrustBrokenSPKI)
            if ((parent.Version == 3L && !parent.BasicConstraintsValid || parent.BasicConstraintsValid && !parent.IsCA) && !bytes.Equal(c.RawSubjectPublicKeyInfo, entrustBrokenSPKI))
            {
                return error.As(new ConstraintViolationError());
            }
            if (parent.KeyUsage != 0L && parent.KeyUsage & KeyUsageCertSign == 0L)
            {
                return error.As(new ConstraintViolationError());
            }
            if (parent.PublicKeyAlgorithm == UnknownPublicKeyAlgorithm)
            {
                return error.As(ErrUnsupportedAlgorithm);
            } 

            // TODO(agl): don't ignore the path length constraint.
            return error.As(parent.CheckSignature(c.SignatureAlgorithm, c.RawTBSCertificate, c.Signature));
        }

        // CheckSignature verifies that signature is a valid signature over signed from
        // c's public key.
        private static error CheckSignature(this ref Certificate c, SignatureAlgorithm algo, slice<byte> signed, slice<byte> signature)
        {
            return error.As(checkSignature(algo, signed, signature, c.PublicKey));
        }

        private static bool hasNameConstraints(this ref Certificate c)
        {
            foreach (var (_, e) in c.Extensions)
            {
                if (len(e.Id) == 4L && e.Id[0L] == 2L && e.Id[1L] == 5L && e.Id[2L] == 29L && e.Id[3L] == 30L)
                {
                    return true;
                }
            }
            return false;
        }

        private static (slice<byte>, bool) getSANExtension(this ref Certificate c)
        {
            foreach (var (_, e) in c.Extensions)
            {
                if (len(e.Id) == 4L && e.Id[0L] == 2L && e.Id[1L] == 5L && e.Id[2L] == 29L && e.Id[3L] == 17L)
                {
                    return (e.Value, true);
                }
            }
            return (null, false);
        }

        private static error signaturePublicKeyAlgoMismatchError(PublicKeyAlgorithm expectedPubKeyAlgo, object pubKey)
        {
            return error.As(fmt.Errorf("x509: signature algorithm specifies an %s public key, but have public key of type %T", expectedPubKeyAlgo.String(), pubKey));
        }

        // CheckSignature verifies that signature is a valid signature over signed from
        // a crypto.PublicKey.
        private static error checkSignature(SignatureAlgorithm algo, slice<byte> signed, slice<byte> signature, crypto.PublicKey publicKey)
        {
            crypto.Hash hashType = default;
            PublicKeyAlgorithm pubKeyAlgo = default;

            foreach (var (_, details) in signatureAlgorithmDetails)
            {
                if (details.algo == algo)
                {
                    hashType = details.hash;
                    pubKeyAlgo = details.pubKeyAlgo;
                }
            }

            if (hashType == crypto.Hash(0L)) 
                return error.As(ErrUnsupportedAlgorithm);
            else if (hashType == crypto.MD5) 
                return error.As(InsecureAlgorithmError(algo));
                        if (!hashType.Available())
            {
                return error.As(ErrUnsupportedAlgorithm);
            }
            var h = hashType.New();

            h.Write(signed);
            var digest = h.Sum(null);

            switch (publicKey.type())
            {
                case ref rsa.PublicKey pub:
                    if (pubKeyAlgo != RSA)
                    {
                        return error.As(signaturePublicKeyAlgoMismatchError(pubKeyAlgo, pub));
                    }
                    if (algo.isRSAPSS())
                    {
                        return error.As(rsa.VerifyPSS(pub, hashType, digest, signature, ref new rsa.PSSOptions(SaltLength:rsa.PSSSaltLengthEqualsHash)));
                    }
                    else
                    {
                        return error.As(rsa.VerifyPKCS1v15(pub, hashType, digest, signature));
                    }
                    break;
                case ref dsa.PublicKey pub:
                    if (pubKeyAlgo != DSA)
                    {
                        return error.As(signaturePublicKeyAlgoMismatchError(pubKeyAlgo, pub));
                    }
                    ptr<dsaSignature> dsaSig = @new<dsaSignature>();
                    {
                        var rest__prev1 = rest;

                        var (rest, err) = asn1.Unmarshal(signature, dsaSig);

                        if (err != null)
                        {
                            return error.As(err);
                        }
                        else if (len(rest) != 0L)
                        {
                            return error.As(errors.New("x509: trailing data after DSA signature"));
                        }

                        rest = rest__prev1;

                    }
                    if (dsaSig.R.Sign() <= 0L || dsaSig.S.Sign() <= 0L)
                    {
                        return error.As(errors.New("x509: DSA signature contained zero or negative values"));
                    }
                    if (!dsa.Verify(pub, digest, dsaSig.R, dsaSig.S))
                    {
                        return error.As(errors.New("x509: DSA verification failure"));
                    }
                    return;
                    break;
                case ref ecdsa.PublicKey pub:
                    if (pubKeyAlgo != ECDSA)
                    {
                        return error.As(signaturePublicKeyAlgoMismatchError(pubKeyAlgo, pub));
                    }
                    ptr<object> ecdsaSig = @new<ecdsaSignature>();
                    {
                        var rest__prev1 = rest;

                        (rest, err) = asn1.Unmarshal(signature, ecdsaSig);

                        if (err != null)
                        {
                            return error.As(err);
                        }
                        else if (len(rest) != 0L)
                        {
                            return error.As(errors.New("x509: trailing data after ECDSA signature"));
                        }

                        rest = rest__prev1;

                    }
                    if (ecdsaSig.R.Sign() <= 0L || ecdsaSig.S.Sign() <= 0L)
                    {
                        return error.As(errors.New("x509: ECDSA signature contained zero or negative values"));
                    }
                    if (!ecdsa.Verify(pub, digest, ecdsaSig.R, ecdsaSig.S))
                    {
                        return error.As(errors.New("x509: ECDSA verification failure"));
                    }
                    return;
                    break;
            }
            return error.As(ErrUnsupportedAlgorithm);
        }

        // CheckCRLSignature checks that the signature in crl is from c.
        private static error CheckCRLSignature(this ref Certificate c, ref pkix.CertificateList crl)
        {
            var algo = getSignatureAlgorithmFromAI(crl.SignatureAlgorithm);
            return error.As(c.CheckSignature(algo, crl.TBSCertList.Raw, crl.SignatureValue.RightAlign()));
        }

        public partial struct UnhandledCriticalExtension
        {
        }

        public static @string Error(this UnhandledCriticalExtension h)
        {
            return "x509: unhandled critical extension";
        }

        private partial struct basicConstraints
        {
            [Description("asn1:\"optional\"")]
            public bool IsCA;
            [Description("asn1:\"optional,default:-1\"")]
            public long MaxPathLen;
        }

        // RFC 5280 4.2.1.4
        private partial struct policyInformation
        {
            public asn1.ObjectIdentifier Policy; // policyQualifiers omitted
        }

        private static readonly long nameTypeEmail = 1L;
        private static readonly long nameTypeDNS = 2L;
        private static readonly long nameTypeURI = 6L;
        private static readonly long nameTypeIP = 7L;

        // RFC 5280, 4.2.2.1
        private partial struct authorityInfoAccess
        {
            public asn1.ObjectIdentifier Method;
            public asn1.RawValue Location;
        }

        // RFC 5280, 4.2.1.14
        private partial struct distributionPoint
        {
            [Description("asn1:\"optional,tag:0\"")]
            public distributionPointName DistributionPoint;
            [Description("asn1:\"optional,tag:1\"")]
            public asn1.BitString Reason;
            [Description("asn1:\"optional,tag:2\"")]
            public asn1.RawValue CRLIssuer;
        }

        private partial struct distributionPointName
        {
            [Description("asn1:\"optional,tag:0\"")]
            public slice<asn1.RawValue> FullName;
            [Description("asn1:\"optional,tag:1\"")]
            public pkix.RDNSequence RelativeName;
        }

        private static (object, error) parsePublicKey(PublicKeyAlgorithm algo, ref publicKeyInfo keyData)
        {
            var asn1Data = keyData.PublicKey.RightAlign();

            if (algo == RSA) 
                // RSA public keys must have a NULL in the parameters
                // (https://tools.ietf.org/html/rfc3279#section-2.3.1).
                if (!bytes.Equal(keyData.Algorithm.Parameters.FullBytes, asn1.NullBytes))
                {
                    return (null, errors.New("x509: RSA key missing NULL parameters"));
                }
                ptr<pkcs1PublicKey> p = @new<pkcs1PublicKey>();
                var (rest, err) = asn1.Unmarshal(asn1Data, p);
                if (err != null)
                {
                    return (null, err);
                }
                if (len(rest) != 0L)
                {
                    return (null, errors.New("x509: trailing data after RSA public key"));
                }
                if (p.N.Sign() <= 0L)
                {
                    return (null, errors.New("x509: RSA modulus is not a positive number"));
                }
                if (p.E <= 0L)
                {
                    return (null, errors.New("x509: RSA public exponent is not a positive number"));
                }
                rsa.PublicKey pub = ref new rsa.PublicKey(E:p.E,N:p.N,);
                return (pub, null);
            else if (algo == DSA) 
                p = default;
                (rest, err) = asn1.Unmarshal(asn1Data, ref p);
                if (err != null)
                {
                    return (null, err);
                }
                if (len(rest) != 0L)
                {
                    return (null, errors.New("x509: trailing data after DSA public key"));
                }
                var paramsData = keyData.Algorithm.Parameters.FullBytes;
                ptr<object> @params = @new<dsaAlgorithmParameters>();
                rest, err = asn1.Unmarshal(paramsData, params);
                if (err != null)
                {
                    return (null, err);
                }
                if (len(rest) != 0L)
                {
                    return (null, errors.New("x509: trailing data after DSA parameters"));
                }
                if (p.Sign() <= 0L || @params.P.Sign() <= 0L || @params.Q.Sign() <= 0L || @params.G.Sign() <= 0L)
                {
                    return (null, errors.New("x509: zero or negative DSA parameter"));
                }
                pub = ref new dsa.PublicKey(Parameters:dsa.Parameters{P:params.P,Q:params.Q,G:params.G,},Y:p,);
                return (pub, null);
            else if (algo == ECDSA) 
                paramsData = keyData.Algorithm.Parameters.FullBytes;
                ptr<asn1.ObjectIdentifier> namedCurveOID = @new<asn1.ObjectIdentifier>();
                (rest, err) = asn1.Unmarshal(paramsData, namedCurveOID);
                if (err != null)
                {
                    return (null, err);
                }
                if (len(rest) != 0L)
                {
                    return (null, errors.New("x509: trailing data after ECDSA parameters"));
                }
                var namedCurve = namedCurveFromOID(namedCurveOID.Value);
                if (namedCurve == null)
                {
                    return (null, errors.New("x509: unsupported elliptic curve"));
                }
                var (x, y) = elliptic.Unmarshal(namedCurve, asn1Data);
                if (x == null)
                {
                    return (null, errors.New("x509: failed to unmarshal elliptic curve point"));
                }
                pub = ref new ecdsa.PublicKey(Curve:namedCurve,X:x,Y:y,);
                return (pub, null);
            else 
                return (null, null);
                    }

        private static error forEachSAN(slice<byte> extension, Func<long, slice<byte>, error> callback)
        { 
            // RFC 5280, 4.2.1.6

            // SubjectAltName ::= GeneralNames
            //
            // GeneralNames ::= SEQUENCE SIZE (1..MAX) OF GeneralName
            //
            // GeneralName ::= CHOICE {
            //      otherName                       [0]     OtherName,
            //      rfc822Name                      [1]     IA5String,
            //      dNSName                         [2]     IA5String,
            //      x400Address                     [3]     ORAddress,
            //      directoryName                   [4]     Name,
            //      ediPartyName                    [5]     EDIPartyName,
            //      uniformResourceIdentifier       [6]     IA5String,
            //      iPAddress                       [7]     OCTET STRING,
            //      registeredID                    [8]     OBJECT IDENTIFIER }
            asn1.RawValue seq = default;
            var (rest, err) = asn1.Unmarshal(extension, ref seq);
            if (err != null)
            {
                return error.As(err);
            }
            else if (len(rest) != 0L)
            {
                return error.As(errors.New("x509: trailing data after X.509 extension"));
            }
            if (!seq.IsCompound || seq.Tag != 16L || seq.Class != 0L)
            {
                return error.As(new asn1.StructuralError(Msg:"bad SAN sequence"));
            }
            rest = seq.Bytes;
            while (len(rest) > 0L)
            {
                asn1.RawValue v = default;
                rest, err = asn1.Unmarshal(rest, ref v);
                if (err != null)
                {
                    return error.As(err);
                }
                {
                    var err = callback(v.Tag, v.Bytes);

                    if (err != null)
                    {
                        return error.As(err);
                    }

                }
            }


            return error.As(null);
        }

        private static (slice<@string>, slice<@string>, slice<net.IP>, slice<ref url.URL>, error) parseSANExtension(slice<byte> value)
        {
            err = forEachSAN(value, (tag, data) =>
            {

                if (tag == nameTypeEmail) 
                    emailAddresses = append(emailAddresses, string(data));
                else if (tag == nameTypeDNS) 
                    dnsNames = append(dnsNames, string(data));
                else if (tag == nameTypeURI) 
                    var (uri, err) = url.Parse(string(data));
                    if (err != null)
                    {
                        return fmt.Errorf("x509: cannot parse URI %q: %s", string(data), err);
                    }
                    if (len(uri.Host) > 0L)
                    {
                        {
                            var (_, ok) = domainToReverseLabels(uri.Host);

                            if (!ok)
                            {
                                return fmt.Errorf("x509: cannot parse URI %q: invalid domain", string(data));
                            }

                        }
                    }
                    uris = append(uris, uri);
                else if (tag == nameTypeIP) 

                    if (len(data) == net.IPv4len || len(data) == net.IPv6len) 
                        ipAddresses = append(ipAddresses, data);
                    else 
                        return errors.New("x509: cannot parse IP address of length " + strconv.Itoa(len(data)));
                                                    return null;
            });

            return;
        }

        // isValidIPMask returns true iff mask consists of zero or more 1 bits, followed by zero bits.
        private static bool isValidIPMask(slice<byte> mask)
        {
            var seenZero = false;

            foreach (var (_, b) in mask)
            {
                if (seenZero)
                {
                    if (b != 0L)
                    {
                        return false;
                    }
                    continue;
                }
                switch (b)
                {
                    case 0x00UL: 

                    case 0x80UL: 

                    case 0xc0UL: 

                    case 0xe0UL: 

                    case 0xf0UL: 

                    case 0xf8UL: 

                    case 0xfcUL: 

                    case 0xfeUL: 
                        seenZero = true;
                        break;
                    case 0xffUL: 
                        break;
                    default: 
                        return false;
                        break;
                }
            }
            return true;
        }

        private static (bool, error) parseNameConstraintsExtension(ref Certificate @out, pkix.Extension e)
        { 
            // RFC 5280, 4.2.1.10

            // NameConstraints ::= SEQUENCE {
            //      permittedSubtrees       [0]     GeneralSubtrees OPTIONAL,
            //      excludedSubtrees        [1]     GeneralSubtrees OPTIONAL }
            //
            // GeneralSubtrees ::= SEQUENCE SIZE (1..MAX) OF GeneralSubtree
            //
            // GeneralSubtree ::= SEQUENCE {
            //      base                    GeneralName,
            //      minimum         [0]     BaseDistance DEFAULT 0,
            //      maximum         [1]     BaseDistance OPTIONAL }
            //
            // BaseDistance ::= INTEGER (0..MAX)

            var outer = cryptobyte.String(e.Value);
            cryptobyte.String toplevel = default;            cryptobyte.String permitted = default;            cryptobyte.String excluded = default;

            bool havePermitted = default;            bool haveExcluded = default;

            if (!outer.ReadASN1(ref toplevel, cryptobyte_asn1.SEQUENCE) || !outer.Empty() || !toplevel.ReadOptionalASN1(ref permitted, ref havePermitted, cryptobyte_asn1.Tag(0L).ContextSpecific().Constructed()) || !toplevel.ReadOptionalASN1(ref excluded, ref haveExcluded, cryptobyte_asn1.Tag(1L).ContextSpecific().Constructed()) || !toplevel.Empty())
            {
                return (false, errors.New("x509: invalid NameConstraints extension"));
            }
            if (!havePermitted && !haveExcluded || len(permitted) == 0L && len(excluded) == 0L)
            { 
                // https://tools.ietf.org/html/rfc5280#section-4.2.1.10:
                //   “either the permittedSubtrees field
                //   or the excludedSubtrees MUST be
                //   present”
                return (false, errors.New("x509: empty name constraints extension"));
            }
            Func<cryptobyte.String, (slice<@string>, slice<ref net.IPNet>, slice<@string>, slice<@string>, error)> getValues = subtrees =>
            {
                while (!subtrees.Empty())
                {
                    cryptobyte.String seq = default;                    cryptobyte.String value = default;

                    cryptobyte_asn1.Tag tag = default;
                    if (!subtrees.ReadASN1(ref seq, cryptobyte_asn1.SEQUENCE) || !seq.ReadAnyASN1(ref value, ref tag))
                    {
                        return (null, null, null, null, fmt.Errorf("x509: invalid NameConstraints extension"));
                    }
                    var dnsTag = cryptobyte_asn1.Tag(2L).ContextSpecific();                    var emailTag = cryptobyte_asn1.Tag(1L).ContextSpecific();                    var ipTag = cryptobyte_asn1.Tag(7L).ContextSpecific();                    var uriTag = cryptobyte_asn1.Tag(6L).ContextSpecific();


                    if (tag == dnsTag) 
                        var domain = string(value);
                        {
                            var err__prev1 = err;

                            var err = isIA5String(domain);

                            if (err != null)
                            {
                                return (null, null, null, null, errors.New("x509: invalid constraint value: " + err.Error()));
                            }

                            err = err__prev1;

                        }

                        var trimmedDomain = domain;
                        if (len(trimmedDomain) > 0L && trimmedDomain[0L] == '.')
                        { 
                            // constraints can have a leading
                            // period to exclude the domain
                            // itself, but that's not valid in a
                            // normal domain name.
                            trimmedDomain = trimmedDomain[1L..];
                        }
                        {
                            var (_, ok) = domainToReverseLabels(trimmedDomain);

                            if (!ok)
                            {
                                return (null, null, null, null, fmt.Errorf("x509: failed to parse dnsName constraint %q", domain));
                            }

                        }
                        dnsNames = append(dnsNames, domain);
                    else if (tag == ipTag) 
                        var l = len(value);
                        slice<byte> ip = default;                        slice<byte> mask = default;



                        switch (l)
                        {
                            case 8L: 
                                ip = value[..4L];
                                mask = value[4L..];
                                break;
                            case 32L: 
                                ip = value[..16L];
                                mask = value[16L..];
                                break;
                            default: 
                                return (null, null, null, null, fmt.Errorf("x509: IP constraint contained value of length %d", l));
                                break;
                        }

                        if (!isValidIPMask(mask))
                        {
                            return (null, null, null, null, fmt.Errorf("x509: IP constraint contained invalid mask %x", mask));
                        }
                        ips = append(ips, ref new net.IPNet(IP:net.IP(ip),Mask:net.IPMask(mask)));
                    else if (tag == emailTag) 
                        var constraint = string(value);
                        {
                            var err__prev1 = err;

                            err = isIA5String(constraint);

                            if (err != null)
                            {
                                return (null, null, null, null, errors.New("x509: invalid constraint value: " + err.Error()));
                            } 

                            // If the constraint contains an @ then
                            // it specifies an exact mailbox name.

                            err = err__prev1;

                        } 

                        // If the constraint contains an @ then
                        // it specifies an exact mailbox name.
                        if (strings.Contains(constraint, "@"))
                        {
                            {
                                (_, ok) = parseRFC2821Mailbox(constraint);

                                if (!ok)
                                {
                                    return (null, null, null, null, fmt.Errorf("x509: failed to parse rfc822Name constraint %q", constraint));
                                }

                            }
                        }
                        else
                        { 
                            // Otherwise it's a domain name.
                            domain = constraint;
                            if (len(domain) > 0L && domain[0L] == '.')
                            {
                                domain = domain[1L..];
                            }
                            {
                                (_, ok) = domainToReverseLabels(domain);

                                if (!ok)
                                {
                                    return (null, null, null, null, fmt.Errorf("x509: failed to parse rfc822Name constraint %q", constraint));
                                }

                            }
                        }
                        emails = append(emails, constraint);
                    else if (tag == uriTag) 
                        domain = string(value);
                        {
                            var err__prev1 = err;

                            err = isIA5String(domain);

                            if (err != null)
                            {
                                return (null, null, null, null, errors.New("x509: invalid constraint value: " + err.Error()));
                            }

                            err = err__prev1;

                        }

                        if (net.ParseIP(domain) != null)
                        {
                            return (null, null, null, null, fmt.Errorf("x509: failed to parse URI constraint %q: cannot be IP address", domain));
                        }
                        trimmedDomain = domain;
                        if (len(trimmedDomain) > 0L && trimmedDomain[0L] == '.')
                        { 
                            // constraints can have a leading
                            // period to exclude the domain itself,
                            // but that's not valid in a normal
                            // domain name.
                            trimmedDomain = trimmedDomain[1L..];
                        }
                        {
                            (_, ok) = domainToReverseLabels(trimmedDomain);

                            if (!ok)
                            {
                                return (null, null, null, null, fmt.Errorf("x509: failed to parse URI constraint %q", domain));
                            }

                        }
                        uriDomains = append(uriDomains, domain);
                    else 
                        unhandled = true;
                                    }


                return (dnsNames, ips, emails, uriDomains, null);
            }
;

            @out.PermittedDNSDomains, @out.PermittedIPRanges, @out.PermittedEmailAddresses, @out.PermittedURIDomains, err = getValues(permitted);

            if (err != null)
            {
                return (false, err);
            }
            @out.ExcludedDNSDomains, @out.ExcludedIPRanges, @out.ExcludedEmailAddresses, @out.ExcludedURIDomains, err = getValues(excluded);

            if (err != null)
            {
                return (false, err);
            }
            @out.PermittedDNSDomainsCritical = e.Critical;

            return (unhandled, null);
        }

        private static (ref Certificate, error) parseCertificate(ref certificate @in)
        {
            ptr<Certificate> @out = @new<Certificate>();
            @out.Raw = @in.Raw;
            @out.RawTBSCertificate = @in.TBSCertificate.Raw;
            @out.RawSubjectPublicKeyInfo = @in.TBSCertificate.PublicKey.Raw;
            @out.RawSubject = @in.TBSCertificate.Subject.FullBytes;
            @out.RawIssuer = @in.TBSCertificate.Issuer.FullBytes;

            @out.Signature = @in.SignatureValue.RightAlign();
            @out.SignatureAlgorithm = getSignatureAlgorithmFromAI(@in.TBSCertificate.SignatureAlgorithm);

            @out.PublicKeyAlgorithm = getPublicKeyAlgorithmFromOID(@in.TBSCertificate.PublicKey.Algorithm.Algorithm);
            error err = default;
            @out.PublicKey, err = parsePublicKey(@out.PublicKeyAlgorithm, ref @in.TBSCertificate.PublicKey);
            if (err != null)
            {
                return (null, err);
            }
            @out.Version = @in.TBSCertificate.Version + 1L;
            @out.SerialNumber = @in.TBSCertificate.SerialNumber;

            pkix.RDNSequence issuer = default;            pkix.RDNSequence subject = default;

            {
                var rest__prev1 = rest;

                var (rest, err) = asn1.Unmarshal(@in.TBSCertificate.Subject.FullBytes, ref subject);

                if (err != null)
                {
                    return (null, err);
                }
                else if (len(rest) != 0L)
                {
                    return (null, errors.New("x509: trailing data after X.509 subject"));
                }

                rest = rest__prev1;

            }
            {
                var rest__prev1 = rest;

                (rest, err) = asn1.Unmarshal(@in.TBSCertificate.Issuer.FullBytes, ref issuer);

                if (err != null)
                {
                    return (null, err);
                }
                else if (len(rest) != 0L)
                {
                    return (null, errors.New("x509: trailing data after X.509 subject"));
                }

                rest = rest__prev1;

            }

            @out.Issuer.FillFromRDNSequence(ref issuer);
            @out.Subject.FillFromRDNSequence(ref subject);

            @out.NotBefore = @in.TBSCertificate.Validity.NotBefore;
            @out.NotAfter = @in.TBSCertificate.Validity.NotAfter;

            foreach (var (_, e) in @in.TBSCertificate.Extensions)
            {
                @out.Extensions = append(@out.Extensions, e);
                var unhandled = false;

                if (len(e.Id) == 4L && e.Id[0L] == 2L && e.Id[1L] == 5L && e.Id[2L] == 29L)
                {
                    switch (e.Id[3L])
                    {
                        case 15L: 
                            // RFC 5280, 4.2.1.3
                            asn1.BitString usageBits = default;
                            {
                                var rest__prev2 = rest;

                                (rest, err) = asn1.Unmarshal(e.Value, ref usageBits);

                                if (err != null)
                                {
                                    return (null, err);
                                }
                                else if (len(rest) != 0L)
                                {
                                    return (null, errors.New("x509: trailing data after X.509 KeyUsage"));
                                }

                                rest = rest__prev2;

                            }

                            long usage = default;
                            {
                                long i__prev2 = i;

                                for (long i = 0L; i < 9L; i++)
                                {
                                    if (usageBits.At(i) != 0L)
                                    {
                                        usage |= 1L << (int)(uint(i));
                                    }
                                }


                                i = i__prev2;
                            }
                            @out.KeyUsage = KeyUsage(usage);
                            break;
                        case 19L: 
                            // RFC 5280, 4.2.1.9
                            basicConstraints constraints = default;
                            {
                                var rest__prev2 = rest;

                                (rest, err) = asn1.Unmarshal(e.Value, ref constraints);

                                if (err != null)
                                {
                                    return (null, err);
                                }
                                else if (len(rest) != 0L)
                                {
                                    return (null, errors.New("x509: trailing data after X.509 BasicConstraints"));
                                }

                                rest = rest__prev2;

                            }

                            @out.BasicConstraintsValid = true;
                            @out.IsCA = constraints.IsCA;
                            @out.MaxPathLen = constraints.MaxPathLen;
                            @out.MaxPathLenZero = @out.MaxPathLen == 0L; 
                            // TODO: map out.MaxPathLen to 0 if it has the -1 default value? (Issue 19285)
                            break;
                        case 17L: 
                            @out.DNSNames, @out.EmailAddresses, @out.IPAddresses, @out.URIs, err = parseSANExtension(e.Value);
                            if (err != null)
                            {
                                return (null, err);
                            }
                            if (len(@out.DNSNames) == 0L && len(@out.EmailAddresses) == 0L && len(@out.IPAddresses) == 0L && len(@out.URIs) == 0L)
                            { 
                                // If we didn't parse anything then we do the critical check, below.
                                unhandled = true;
                            }
                            break;
                        case 30L: 
                            unhandled, err = parseNameConstraintsExtension(out, e);
                            if (err != null)
                            {
                                return (null, err);
                            }
                            break;
                        case 31L: 
                            // RFC 5280, 4.2.1.13

                            // CRLDistributionPoints ::= SEQUENCE SIZE (1..MAX) OF DistributionPoint
                            //
                            // DistributionPoint ::= SEQUENCE {
                            //     distributionPoint       [0]     DistributionPointName OPTIONAL,
                            //     reasons                 [1]     ReasonFlags OPTIONAL,
                            //     cRLIssuer               [2]     GeneralNames OPTIONAL }
                            //
                            // DistributionPointName ::= CHOICE {
                            //     fullName                [0]     GeneralNames,
                            //     nameRelativeToCRLIssuer [1]     RelativeDistinguishedName }

                            slice<distributionPoint> cdp = default;
                            {
                                var rest__prev2 = rest;

                                (rest, err) = asn1.Unmarshal(e.Value, ref cdp);

                                if (err != null)
                                {
                                    return (null, err);
                                }
                                else if (len(rest) != 0L)
                                {
                                    return (null, errors.New("x509: trailing data after X.509 CRL distribution point"));
                                }

                                rest = rest__prev2;

                            }

                            foreach (var (_, dp) in cdp)
                            { 
                                // Per RFC 5280, 4.2.1.13, one of distributionPoint or cRLIssuer may be empty.
                                if (len(dp.DistributionPoint.FullName) == 0L)
                                {
                                    continue;
                                }
                                foreach (var (_, fullName) in dp.DistributionPoint.FullName)
                                {
                                    if (fullName.Tag == 6L)
                                    {
                                        @out.CRLDistributionPoints = append(@out.CRLDistributionPoints, string(fullName.Bytes));
                                    }
                                }
                            }
                            break;
                        case 35L: 
                            // RFC 5280, 4.2.1.1
                            authKeyId a = default;
                            {
                                var rest__prev2 = rest;

                                (rest, err) = asn1.Unmarshal(e.Value, ref a);

                                if (err != null)
                                {
                                    return (null, err);
                                }
                                else if (len(rest) != 0L)
                                {
                                    return (null, errors.New("x509: trailing data after X.509 authority key-id"));
                                }

                                rest = rest__prev2;

                            }
                            @out.AuthorityKeyId = a.Id;
                            break;
                        case 37L: 
                            // RFC 5280, 4.2.1.12.  Extended Key Usage

                            // id-ce-extKeyUsage OBJECT IDENTIFIER ::= { id-ce 37 }
                            //
                            // ExtKeyUsageSyntax ::= SEQUENCE SIZE (1..MAX) OF KeyPurposeId
                            //
                            // KeyPurposeId ::= OBJECT IDENTIFIER

                            slice<asn1.ObjectIdentifier> keyUsage = default;
                            {
                                var rest__prev2 = rest;

                                (rest, err) = asn1.Unmarshal(e.Value, ref keyUsage);

                                if (err != null)
                                {
                                    return (null, err);
                                }
                                else if (len(rest) != 0L)
                                {
                                    return (null, errors.New("x509: trailing data after X.509 ExtendedKeyUsage"));
                                }

                                rest = rest__prev2;

                            }

                            foreach (var (_, u) in keyUsage)
                            {
                                {
                                    var (extKeyUsage, ok) = extKeyUsageFromOID(u);

                                    if (ok)
                                    {
                                        @out.ExtKeyUsage = append(@out.ExtKeyUsage, extKeyUsage);
                                    }
                                    else
                                    {
                                        @out.UnknownExtKeyUsage = append(@out.UnknownExtKeyUsage, u);
                                    }

                                }
                            }
                            break;
                        case 14L: 
                            // RFC 5280, 4.2.1.2
                            slice<byte> keyid = default;
                            {
                                var rest__prev2 = rest;

                                (rest, err) = asn1.Unmarshal(e.Value, ref keyid);

                                if (err != null)
                                {
                                    return (null, err);
                                }
                                else if (len(rest) != 0L)
                                {
                                    return (null, errors.New("x509: trailing data after X.509 key-id"));
                                }

                                rest = rest__prev2;

                            }
                            @out.SubjectKeyId = keyid;
                            break;
                        case 32L: 
                            // RFC 5280 4.2.1.4: Certificate Policies
                            slice<policyInformation> policies = default;
                            {
                                var rest__prev2 = rest;

                                (rest, err) = asn1.Unmarshal(e.Value, ref policies);

                                if (err != null)
                                {
                                    return (null, err);
                                }
                                else if (len(rest) != 0L)
                                {
                                    return (null, errors.New("x509: trailing data after X.509 certificate policies"));
                                }

                                rest = rest__prev2;

                            }
                            @out.PolicyIdentifiers = make_slice<asn1.ObjectIdentifier>(len(policies));
                            {
                                long i__prev2 = i;

                                foreach (var (__i, __policy) in policies)
                                {
                                    i = __i;
                                    policy = __policy;
                                    @out.PolicyIdentifiers[i] = policy.Policy;
                                }

                                i = i__prev2;
                            }
                            break;
                        default: 
                            // Unknown extensions are recorded if critical.
                            unhandled = true;
                            break;
                    }
                }
                else if (e.Id.Equal(oidExtensionAuthorityInfoAccess))
                { 
                    // RFC 5280 4.2.2.1: Authority Information Access
                    slice<authorityInfoAccess> aia = default;
                    {
                        var rest__prev3 = rest;

                        (rest, err) = asn1.Unmarshal(e.Value, ref aia);

                        if (err != null)
                        {
                            return (null, err);
                        }
                        else if (len(rest) != 0L)
                        {
                            return (null, errors.New("x509: trailing data after X.509 authority information"));
                        }

                        rest = rest__prev3;

                    }

                    foreach (var (_, v) in aia)
                    { 
                        // GeneralName: uniformResourceIdentifier [6] IA5String
                        if (v.Location.Tag != 6L)
                        {
                            continue;
                        }
                        if (v.Method.Equal(oidAuthorityInfoAccessOcsp))
                        {
                            @out.OCSPServer = append(@out.OCSPServer, string(v.Location.Bytes));
                        }
                        else if (v.Method.Equal(oidAuthorityInfoAccessIssuers))
                        {
                            @out.IssuingCertificateURL = append(@out.IssuingCertificateURL, string(v.Location.Bytes));
                        }
                    }
                else
                }                { 
                    // Unknown extensions are recorded if critical.
                    unhandled = true;
                }
                if (e.Critical && unhandled)
                {
                    @out.UnhandledCriticalExtensions = append(@out.UnhandledCriticalExtensions, e.Id);
                }
            }
            return (out, null);
        }

        // ParseCertificate parses a single certificate from the given ASN.1 DER data.
        public static (ref Certificate, error) ParseCertificate(slice<byte> asn1Data)
        {
            certificate cert = default;
            var (rest, err) = asn1.Unmarshal(asn1Data, ref cert);
            if (err != null)
            {
                return (null, err);
            }
            if (len(rest) > 0L)
            {
                return (null, new asn1.SyntaxError(Msg:"trailing data"));
            }
            return parseCertificate(ref cert);
        }

        // ParseCertificates parses one or more certificates from the given ASN.1 DER
        // data. The certificates must be concatenated with no intermediate padding.
        public static (slice<ref Certificate>, error) ParseCertificates(slice<byte> asn1Data)
        {
            slice<ref certificate> v = default;

            while (len(asn1Data) > 0L)
            {
                ptr<certificate> cert = @new<certificate>();
                error err = default;
                asn1Data, err = asn1.Unmarshal(asn1Data, cert);
                if (err != null)
                {
                    return (null, err);
                }
                v = append(v, cert);
            }


            var ret = make_slice<ref Certificate>(len(v));
            foreach (var (i, ci) in v)
            {
                var (cert, err) = parseCertificate(ci);
                if (err != null)
                {
                    return (null, err);
                }
                ret[i] = cert;
            }
            return (ret, null);
        }

        private static byte reverseBitsInAByte(byte @in)
        {
            var b1 = in >> (int)(4L) | in << (int)(4L);
            var b2 = b1 >> (int)(2L) & 0x33UL | b1 << (int)(2L) & 0xccUL;
            var b3 = b2 >> (int)(1L) & 0x55UL | b2 << (int)(1L) & 0xaaUL;
            return b3;
        }

        // asn1BitLength returns the bit-length of bitString by considering the
        // most-significant bit in a byte to be the "first" bit. This convention
        // matches ASN.1, but differs from almost everything else.
        private static long asn1BitLength(slice<byte> bitString)
        {
            var bitLen = len(bitString) * 8L;

            foreach (var (i) in bitString)
            {
                var b = bitString[len(bitString) - i - 1L];

                for (var bit = uint(0L); bit < 8L; bit++)
                {
                    if ((b >> (int)(bit)) & 1L == 1L)
                    {
                        return bitLen;
                    }
                    bitLen--;
                }

            }
            return 0L;
        }

        private static long oidExtensionSubjectKeyId = new slice<long>(new long[] { 2, 5, 29, 14 });        private static long oidExtensionKeyUsage = new slice<long>(new long[] { 2, 5, 29, 15 });        private static long oidExtensionExtendedKeyUsage = new slice<long>(new long[] { 2, 5, 29, 37 });        private static long oidExtensionAuthorityKeyId = new slice<long>(new long[] { 2, 5, 29, 35 });        private static long oidExtensionBasicConstraints = new slice<long>(new long[] { 2, 5, 29, 19 });        private static long oidExtensionSubjectAltName = new slice<long>(new long[] { 2, 5, 29, 17 });        private static long oidExtensionCertificatePolicies = new slice<long>(new long[] { 2, 5, 29, 32 });        private static long oidExtensionNameConstraints = new slice<long>(new long[] { 2, 5, 29, 30 });        private static long oidExtensionCRLDistributionPoints = new slice<long>(new long[] { 2, 5, 29, 31 });        private static long oidExtensionAuthorityInfoAccess = new slice<long>(new long[] { 1, 3, 6, 1, 5, 5, 7, 1, 1 });

        private static asn1.ObjectIdentifier oidAuthorityInfoAccessOcsp = new asn1.ObjectIdentifier(1,3,6,1,5,5,7,48,1);        private static asn1.ObjectIdentifier oidAuthorityInfoAccessIssuers = new asn1.ObjectIdentifier(1,3,6,1,5,5,7,48,2);

        // oidNotInExtensions returns whether an extension with the given oid exists in
        // extensions.
        private static bool oidInExtensions(asn1.ObjectIdentifier oid, slice<pkix.Extension> extensions)
        {
            foreach (var (_, e) in extensions)
            {
                if (e.Id.Equal(oid))
                {
                    return true;
                }
            }
            return false;
        }

        // marshalSANs marshals a list of addresses into a the contents of an X.509
        // SubjectAlternativeName extension.
        private static (slice<byte>, error) marshalSANs(slice<@string> dnsNames, slice<@string> emailAddresses, slice<net.IP> ipAddresses, slice<ref url.URL> uris)
        {
            slice<asn1.RawValue> rawValues = default;
            foreach (var (_, name) in dnsNames)
            {
                rawValues = append(rawValues, new asn1.RawValue(Tag:nameTypeDNS,Class:2,Bytes:[]byte(name)));
            }
            foreach (var (_, email) in emailAddresses)
            {
                rawValues = append(rawValues, new asn1.RawValue(Tag:nameTypeEmail,Class:2,Bytes:[]byte(email)));
            }
            foreach (var (_, rawIP) in ipAddresses)
            { 
                // If possible, we always want to encode IPv4 addresses in 4 bytes.
                var ip = rawIP.To4();
                if (ip == null)
                {
                    ip = rawIP;
                }
                rawValues = append(rawValues, new asn1.RawValue(Tag:nameTypeIP,Class:2,Bytes:ip));
            }
            foreach (var (_, uri) in uris)
            {
                rawValues = append(rawValues, new asn1.RawValue(Tag:nameTypeURI,Class:2,Bytes:[]byte(uri.String())));
            }
            return asn1.Marshal(rawValues);
        }

        private static error isIA5String(@string s)
        {
            foreach (var (_, r) in s)
            {
                if (r >= utf8.RuneSelf)
                {
                    return error.As(fmt.Errorf("x509: %q cannot be encoded as an IA5String", s));
                }
            }
            return error.As(null);
        }

        private static (slice<pkix.Extension>, error) buildExtensions(ref Certificate _template, bool subjectIsEmpty, slice<byte> authorityKeyId) => func(_template, (ref Certificate template, Defer _, Panic panic, Recover __) =>
        {
            ret = make_slice<pkix.Extension>(10L);
            long n = 0L;

            if (template.KeyUsage != 0L && !oidInExtensions(oidExtensionKeyUsage, template.ExtraExtensions))
            {
                ret[n].Id = oidExtensionKeyUsage;
                ret[n].Critical = true;

                array<byte> a = new array<byte>(2L);
                a[0L] = reverseBitsInAByte(byte(template.KeyUsage));
                a[1L] = reverseBitsInAByte(byte(template.KeyUsage >> (int)(8L)));

                long l = 1L;
                if (a[1L] != 0L)
                {
                    l = 2L;
                }
                var bitString = a[..l];
                ret[n].Value, err = asn1.Marshal(new asn1.BitString(Bytes:bitString,BitLength:asn1BitLength(bitString)));
                if (err != null)
                {
                    return;
                }
                n++;
            }
            if ((len(template.ExtKeyUsage) > 0L || len(template.UnknownExtKeyUsage) > 0L) && !oidInExtensions(oidExtensionExtendedKeyUsage, template.ExtraExtensions))
            {
                ret[n].Id = oidExtensionExtendedKeyUsage;

                slice<asn1.ObjectIdentifier> oids = default;
                foreach (var (_, u) in template.ExtKeyUsage)
                {
                    {
                        var (oid, ok) = oidFromExtKeyUsage(u);

                        if (ok)
                        {
                            oids = append(oids, oid);
                        }
                        else
                        {
                            panic("internal error");
                        }

                    }
                }
                oids = append(oids, template.UnknownExtKeyUsage);

                ret[n].Value, err = asn1.Marshal(oids);
                if (err != null)
                {
                    return;
                }
                n++;
            }
            if (template.BasicConstraintsValid && !oidInExtensions(oidExtensionBasicConstraints, template.ExtraExtensions))
            { 
                // Leaving MaxPathLen as zero indicates that no maximum path
                // length is desired, unless MaxPathLenZero is set. A value of
                // -1 causes encoding/asn1 to omit the value as desired.
                var maxPathLen = template.MaxPathLen;
                if (maxPathLen == 0L && !template.MaxPathLenZero)
                {
                    maxPathLen = -1L;
                }
                ret[n].Id = oidExtensionBasicConstraints;
                ret[n].Value, err = asn1.Marshal(new basicConstraints(template.IsCA,maxPathLen));
                ret[n].Critical = true;
                if (err != null)
                {
                    return;
                }
                n++;
            }
            if (len(template.SubjectKeyId) > 0L && !oidInExtensions(oidExtensionSubjectKeyId, template.ExtraExtensions))
            {
                ret[n].Id = oidExtensionSubjectKeyId;
                ret[n].Value, err = asn1.Marshal(template.SubjectKeyId);
                if (err != null)
                {
                    return;
                }
                n++;
            }
            if (len(authorityKeyId) > 0L && !oidInExtensions(oidExtensionAuthorityKeyId, template.ExtraExtensions))
            {
                ret[n].Id = oidExtensionAuthorityKeyId;
                ret[n].Value, err = asn1.Marshal(new authKeyId(authorityKeyId));
                if (err != null)
                {
                    return;
                }
                n++;
            }
            if ((len(template.OCSPServer) > 0L || len(template.IssuingCertificateURL) > 0L) && !oidInExtensions(oidExtensionAuthorityInfoAccess, template.ExtraExtensions))
            {
                ret[n].Id = oidExtensionAuthorityInfoAccess;
                slice<authorityInfoAccess> aiaValues = default;
                {
                    var name__prev1 = name;

                    foreach (var (_, __name) in template.OCSPServer)
                    {
                        name = __name;
                        aiaValues = append(aiaValues, new authorityInfoAccess(Method:oidAuthorityInfoAccessOcsp,Location:asn1.RawValue{Tag:6,Class:2,Bytes:[]byte(name)},));
                    }

                    name = name__prev1;
                }

                {
                    var name__prev1 = name;

                    foreach (var (_, __name) in template.IssuingCertificateURL)
                    {
                        name = __name;
                        aiaValues = append(aiaValues, new authorityInfoAccess(Method:oidAuthorityInfoAccessIssuers,Location:asn1.RawValue{Tag:6,Class:2,Bytes:[]byte(name)},));
                    }

                    name = name__prev1;
                }

                ret[n].Value, err = asn1.Marshal(aiaValues);
                if (err != null)
                {
                    return;
                }
                n++;
            }
            if ((len(template.DNSNames) > 0L || len(template.EmailAddresses) > 0L || len(template.IPAddresses) > 0L || len(template.URIs) > 0L) && !oidInExtensions(oidExtensionSubjectAltName, template.ExtraExtensions))
            {
                ret[n].Id = oidExtensionSubjectAltName; 
                // https://tools.ietf.org/html/rfc5280#section-4.2.1.6
                // “If the subject field contains an empty sequence ... then
                // subjectAltName extension ... is marked as critical”
                ret[n].Critical = subjectIsEmpty;
                ret[n].Value, err = marshalSANs(template.DNSNames, template.EmailAddresses, template.IPAddresses, template.URIs);
                if (err != null)
                {
                    return;
                }
                n++;
            }
            if (len(template.PolicyIdentifiers) > 0L && !oidInExtensions(oidExtensionCertificatePolicies, template.ExtraExtensions))
            {
                ret[n].Id = oidExtensionCertificatePolicies;
                var policies = make_slice<policyInformation>(len(template.PolicyIdentifiers));
                foreach (var (i, policy) in template.PolicyIdentifiers)
                {
                    policies[i].Policy = policy;
                }
                ret[n].Value, err = asn1.Marshal(policies);
                if (err != null)
                {
                    return;
                }
                n++;
            }
            if ((len(template.PermittedDNSDomains) > 0L || len(template.ExcludedDNSDomains) > 0L || len(template.PermittedIPRanges) > 0L || len(template.ExcludedIPRanges) > 0L || len(template.PermittedEmailAddresses) > 0L || len(template.ExcludedEmailAddresses) > 0L || len(template.PermittedURIDomains) > 0L || len(template.ExcludedURIDomains) > 0L) && !oidInExtensions(oidExtensionNameConstraints, template.ExtraExtensions))
            {
                ret[n].Id = oidExtensionNameConstraints;
                ret[n].Critical = template.PermittedDNSDomainsCritical;

                ipAndMask = ipNet =>
                {
                    var maskedIP = ipNet.IP.Mask(ipNet.Mask);
                    var ipAndMask = make_slice<byte>(0L, len(maskedIP) + len(ipNet.Mask));
                    ipAndMask = append(ipAndMask, maskedIP);
                    ipAndMask = append(ipAndMask, ipNet.Mask);
                    return ipAndMask;
                }
;

                Func<slice<@string>, slice<ref net.IPNet>, slice<@string>, slice<@string>, (slice<byte>, error)> serialiseConstraints = (dns, ips, emails, uriDomains) =>
                {
                    cryptobyte.Builder b = default;

                    {
                        var name__prev1 = name;

                        foreach (var (_, __name) in dns)
                        {
                            name = __name;
                            err = isIA5String(name);

                            if (err != null)
                            {
                                return (null, err);
                            }
                            b.AddASN1(cryptobyte_asn1.SEQUENCE, b =>
                            {
                                b.AddASN1(cryptobyte_asn1.Tag(2L).ContextSpecific(), b =>
                                {
                                    b.AddBytes((slice<byte>)name);
                                });
                            });
                        }

                        name = name__prev1;
                    }

                    foreach (var (_, ipNet) in ips)
                    {
                        b.AddASN1(cryptobyte_asn1.SEQUENCE, b =>
                        {
                            b.AddASN1(cryptobyte_asn1.Tag(7L).ContextSpecific(), b =>
                            {
                                b.AddBytes(ipAndMask(ipNet));
                            });
                        });
                    }
                    foreach (var (_, email) in emails)
                    {
                        err = isIA5String(email);

                        if (err != null)
                        {
                            return (null, err);
                        }
                        b.AddASN1(cryptobyte_asn1.SEQUENCE, b =>
                        {
                            b.AddASN1(cryptobyte_asn1.Tag(1L).ContextSpecific(), b =>
                            {
                                b.AddBytes((slice<byte>)email);
                            });
                        });
                    }
                    foreach (var (_, uriDomain) in uriDomains)
                    {
                        err = isIA5String(uriDomain);

                        if (err != null)
                        {
                            return (null, err);
                        }
                        b.AddASN1(cryptobyte_asn1.SEQUENCE, b =>
                        {
                            b.AddASN1(cryptobyte_asn1.Tag(6L).ContextSpecific(), b =>
                            {
                                b.AddBytes((slice<byte>)uriDomain);
                            });
                        });
                    }
                    return b.Bytes();
                }
;

                var (permitted, err) = serialiseConstraints(template.PermittedDNSDomains, template.PermittedIPRanges, template.PermittedEmailAddresses, template.PermittedURIDomains);
                if (err != null)
                {
                    return (null, err);
                }
                var (excluded, err) = serialiseConstraints(template.ExcludedDNSDomains, template.ExcludedIPRanges, template.ExcludedEmailAddresses, template.ExcludedURIDomains);
                if (err != null)
                {
                    return (null, err);
                }
                b = default;
                b.AddASN1(cryptobyte_asn1.SEQUENCE, b =>
                {
                    if (len(permitted) > 0L)
                    {
                        b.AddASN1(cryptobyte_asn1.Tag(0L).ContextSpecific().Constructed(), b =>
                        {
                            b.AddBytes(permitted);
                        });
                    }
                    if (len(excluded) > 0L)
                    {
                        b.AddASN1(cryptobyte_asn1.Tag(1L).ContextSpecific().Constructed(), b =>
                        {
                            b.AddBytes(excluded);
                        });
                    }
                });

                ret[n].Value, err = b.Bytes();
                if (err != null)
                {
                    return (null, err);
                }
                n++;
            }
            if (len(template.CRLDistributionPoints) > 0L && !oidInExtensions(oidExtensionCRLDistributionPoints, template.ExtraExtensions))
            {
                ret[n].Id = oidExtensionCRLDistributionPoints;

                slice<distributionPoint> crlDp = default;
                {
                    var name__prev1 = name;

                    foreach (var (_, __name) in template.CRLDistributionPoints)
                    {
                        name = __name;
                        distributionPoint dp = new distributionPoint(DistributionPoint:distributionPointName{FullName:[]asn1.RawValue{asn1.RawValue{Tag:6,Class:2,Bytes:[]byte(name)},},},);
                        crlDp = append(crlDp, dp);
                    }

                    name = name__prev1;
                }

                ret[n].Value, err = asn1.Marshal(crlDp);
                if (err != null)
                {
                    return;
                }
                n++;
            } 

            // Adding another extension here? Remember to update the maximum number
            // of elements in the make() at the top of the function.
            return (append(ret[..n], template.ExtraExtensions), null);
        });

        private static (slice<byte>, error) subjectBytes(ref Certificate cert)
        {
            if (len(cert.RawSubject) > 0L)
            {
                return (cert.RawSubject, null);
            }
            return asn1.Marshal(cert.Subject.ToRDNSequence());
        }

        // signingParamsForPublicKey returns the parameters to use for signing with
        // priv. If requestedSigAlgo is not zero then it overrides the default
        // signature algorithm.
        private static (crypto.Hash, pkix.AlgorithmIdentifier, error) signingParamsForPublicKey(object pub, SignatureAlgorithm requestedSigAlgo)
        {
            PublicKeyAlgorithm pubType = default;

            switch (pub.type())
            {
                case ref rsa.PublicKey pub:
                    pubType = RSA;
                    hashFunc = crypto.SHA256;
                    sigAlgo.Algorithm = oidSignatureSHA256WithRSA;
                    sigAlgo.Parameters = asn1.NullRawValue;
                    break;
                case ref ecdsa.PublicKey pub:
                    pubType = ECDSA;


                    if (pub.Curve == elliptic.P224() || pub.Curve == elliptic.P256()) 
                        hashFunc = crypto.SHA256;
                        sigAlgo.Algorithm = oidSignatureECDSAWithSHA256;
                    else if (pub.Curve == elliptic.P384()) 
                        hashFunc = crypto.SHA384;
                        sigAlgo.Algorithm = oidSignatureECDSAWithSHA384;
                    else if (pub.Curve == elliptic.P521()) 
                        hashFunc = crypto.SHA512;
                        sigAlgo.Algorithm = oidSignatureECDSAWithSHA512;
                    else 
                        err = errors.New("x509: unknown elliptic curve");
                                        break;
                default:
                {
                    var pub = pub.type();
                    err = errors.New("x509: only RSA and ECDSA keys supported");
                    break;
                }

            }

            if (err != null)
            {
                return;
            }
            if (requestedSigAlgo == 0L)
            {
                return;
            }
            var found = false;
            foreach (var (_, details) in signatureAlgorithmDetails)
            {
                if (details.algo == requestedSigAlgo)
                {
                    if (details.pubKeyAlgo != pubType)
                    {
                        err = errors.New("x509: requested SignatureAlgorithm does not match private key type");
                        return;
                    }
                    sigAlgo.Algorithm = details.oid;
                    hashFunc = details.hash;
                    if (hashFunc == 0L)
                    {
                        err = errors.New("x509: cannot sign with hash function requested");
                        return;
                    }
                    if (requestedSigAlgo.isRSAPSS())
                    {
                        sigAlgo.Parameters = rsaPSSParameters(hashFunc);
                    }
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                err = errors.New("x509: unknown SignatureAlgorithm");
            }
            return;
        }

        // emptyASN1Subject is the ASN.1 DER encoding of an empty Subject, which is
        // just an empty SEQUENCE.
        private static byte emptyASN1Subject = new slice<byte>(new byte[] { 0x30, 0 });

        // CreateCertificate creates a new X.509v3 certificate based on a template.
        // The following members of template are used: AuthorityKeyId,
        // BasicConstraintsValid, DNSNames, ExcludedDNSDomains, ExtKeyUsage,
        // IsCA, KeyUsage, MaxPathLen, MaxPathLenZero, NotAfter, NotBefore,
        // PermittedDNSDomains, PermittedDNSDomainsCritical, SerialNumber,
        // SignatureAlgorithm, Subject, SubjectKeyId, and UnknownExtKeyUsage.
        //
        // The certificate is signed by parent. If parent is equal to template then the
        // certificate is self-signed. The parameter pub is the public key of the
        // signee and priv is the private key of the signer.
        //
        // The returned slice is the certificate in DER encoding.
        //
        // All keys types that are implemented via crypto.Signer are supported (This
        // includes *rsa.PublicKey and *ecdsa.PublicKey.)
        //
        // The AuthorityKeyId will be taken from the SubjectKeyId of parent, if any,
        // unless the resulting certificate is self-signed. Otherwise the value from
        // template will be used.
        public static (slice<byte>, error) CreateCertificate(io.Reader rand, ref Certificate template, ref Certificate parent, object pub, object priv)
        {
            crypto.Signer (key, ok) = priv._<crypto.Signer>();
            if (!ok)
            {
                return (null, errors.New("x509: certificate private key does not implement crypto.Signer"));
            }
            if (template.SerialNumber == null)
            {
                return (null, errors.New("x509: no SerialNumber given"));
            }
            var (hashFunc, signatureAlgorithm, err) = signingParamsForPublicKey(key.Public(), template.SignatureAlgorithm);
            if (err != null)
            {
                return (null, err);
            }
            var (publicKeyBytes, publicKeyAlgorithm, err) = marshalPublicKey(pub);
            if (err != null)
            {
                return (null, err);
            }
            var (asn1Issuer, err) = subjectBytes(parent);
            if (err != null)
            {
                return;
            }
            var (asn1Subject, err) = subjectBytes(template);
            if (err != null)
            {
                return;
            }
            var authorityKeyId = template.AuthorityKeyId;
            if (!bytes.Equal(asn1Issuer, asn1Subject) && len(parent.SubjectKeyId) > 0L)
            {
                authorityKeyId = parent.SubjectKeyId;
            }
            var (extensions, err) = buildExtensions(template, bytes.Equal(asn1Subject, emptyASN1Subject), authorityKeyId);
            if (err != null)
            {
                return;
            }
            asn1.BitString encodedPublicKey = new asn1.BitString(BitLength:len(publicKeyBytes)*8,Bytes:publicKeyBytes);
            tbsCertificate c = new tbsCertificate(Version:2,SerialNumber:template.SerialNumber,SignatureAlgorithm:signatureAlgorithm,Issuer:asn1.RawValue{FullBytes:asn1Issuer},Validity:validity{template.NotBefore.UTC(),template.NotAfter.UTC()},Subject:asn1.RawValue{FullBytes:asn1Subject},PublicKey:publicKeyInfo{nil,publicKeyAlgorithm,encodedPublicKey},Extensions:extensions,);

            var (tbsCertContents, err) = asn1.Marshal(c);
            if (err != null)
            {
                return;
            }
            c.Raw = tbsCertContents;

            var h = hashFunc.New();
            h.Write(tbsCertContents);
            var digest = h.Sum(null);

            crypto.SignerOpts signerOpts = default;
            signerOpts = hashFunc;
            if (template.SignatureAlgorithm != 0L && template.SignatureAlgorithm.isRSAPSS())
            {
                signerOpts = ref new rsa.PSSOptions(SaltLength:rsa.PSSSaltLengthEqualsHash,Hash:hashFunc,);
            }
            slice<byte> signature = default;
            signature, err = key.Sign(rand, digest, signerOpts);
            if (err != null)
            {
                return;
            }
            return asn1.Marshal(new certificate(nil,c,signatureAlgorithm,asn1.BitString{Bytes:signature,BitLength:len(signature)*8},));
        }

        // pemCRLPrefix is the magic string that indicates that we have a PEM encoded
        // CRL.
        private static slice<byte> pemCRLPrefix = (slice<byte>)"-----BEGIN X509 CRL";

        // pemType is the type of a PEM encoded CRL.
        private static @string pemType = "X509 CRL";

        // ParseCRL parses a CRL from the given bytes. It's often the case that PEM
        // encoded CRLs will appear where they should be DER encoded, so this function
        // will transparently handle PEM encoding as long as there isn't any leading
        // garbage.
        public static (ref pkix.CertificateList, error) ParseCRL(slice<byte> crlBytes)
        {
            if (bytes.HasPrefix(crlBytes, pemCRLPrefix))
            {
                var (block, _) = pem.Decode(crlBytes);
                if (block != null && block.Type == pemType)
                {
                    crlBytes = block.Bytes;
                }
            }
            return ParseDERCRL(crlBytes);
        }

        // ParseDERCRL parses a DER encoded CRL from the given bytes.
        public static (ref pkix.CertificateList, error) ParseDERCRL(slice<byte> derBytes)
        {
            ptr<pkix.CertificateList> certList = @new<pkix.CertificateList>();
            {
                var (rest, err) = asn1.Unmarshal(derBytes, certList);

                if (err != null)
                {
                    return (null, err);
                }
                else if (len(rest) != 0L)
                {
                    return (null, errors.New("x509: trailing data after CRL"));
                }

            }
            return (certList, null);
        }

        // CreateCRL returns a DER encoded CRL, signed by this Certificate, that
        // contains the given list of revoked certificates.
        private static (slice<byte>, error) CreateCRL(this ref Certificate c, io.Reader rand, object priv, slice<pkix.RevokedCertificate> revokedCerts, time.Time now, time.Time expiry)
        {
            crypto.Signer (key, ok) = priv._<crypto.Signer>();
            if (!ok)
            {
                return (null, errors.New("x509: certificate private key does not implement crypto.Signer"));
            }
            var (hashFunc, signatureAlgorithm, err) = signingParamsForPublicKey(key.Public(), 0L);
            if (err != null)
            {
                return (null, err);
            } 

            // Force revocation times to UTC per RFC 5280.
            var revokedCertsUTC = make_slice<pkix.RevokedCertificate>(len(revokedCerts));
            foreach (var (i, rc) in revokedCerts)
            {
                rc.RevocationTime = rc.RevocationTime.UTC();
                revokedCertsUTC[i] = rc;
            }
            pkix.TBSCertificateList tbsCertList = new pkix.TBSCertificateList(Version:1,Signature:signatureAlgorithm,Issuer:c.Subject.ToRDNSequence(),ThisUpdate:now.UTC(),NextUpdate:expiry.UTC(),RevokedCertificates:revokedCertsUTC,); 

            // Authority Key Id
            if (len(c.SubjectKeyId) > 0L)
            {
                pkix.Extension aki = default;
                aki.Id = oidExtensionAuthorityKeyId;
                aki.Value, err = asn1.Marshal(new authKeyId(Id:c.SubjectKeyId));
                if (err != null)
                {
                    return;
                }
                tbsCertList.Extensions = append(tbsCertList.Extensions, aki);
            }
            var (tbsCertListContents, err) = asn1.Marshal(tbsCertList);
            if (err != null)
            {
                return;
            }
            var h = hashFunc.New();
            h.Write(tbsCertListContents);
            var digest = h.Sum(null);

            slice<byte> signature = default;
            signature, err = key.Sign(rand, digest, hashFunc);
            if (err != null)
            {
                return;
            }
            return asn1.Marshal(new pkix.CertificateList(TBSCertList:tbsCertList,SignatureAlgorithm:signatureAlgorithm,SignatureValue:asn1.BitString{Bytes:signature,BitLength:len(signature)*8},));
        }

        // CertificateRequest represents a PKCS #10, certificate signature request.
        public partial struct CertificateRequest
        {
            public slice<byte> Raw; // Complete ASN.1 DER content (CSR, signature algorithm and signature).
            public slice<byte> RawTBSCertificateRequest; // Certificate request info part of raw ASN.1 DER content.
            public slice<byte> RawSubjectPublicKeyInfo; // DER encoded SubjectPublicKeyInfo.
            public slice<byte> RawSubject; // DER encoded Subject.

            public long Version;
            public slice<byte> Signature;
            public SignatureAlgorithm SignatureAlgorithm;
            public PublicKeyAlgorithm PublicKeyAlgorithm;
            public pkix.Name Subject; // Attributes is the dried husk of a bug and shouldn't be used.
            public slice<pkix.AttributeTypeAndValueSET> Attributes; // Extensions contains raw X.509 extensions. When parsing CSRs, this
// can be used to extract extensions that are not parsed by this
// package.
            public slice<pkix.Extension> Extensions; // ExtraExtensions contains extensions to be copied, raw, into any
// marshaled CSR. Values override any extensions that would otherwise
// be produced based on the other fields but are overridden by any
// extensions specified in Attributes.
//
// The ExtraExtensions field is not populated when parsing CSRs, see
// Extensions.
            public slice<pkix.Extension> ExtraExtensions; // Subject Alternate Name values.
            public slice<@string> DNSNames;
            public slice<@string> EmailAddresses;
            public slice<net.IP> IPAddresses;
            public slice<ref url.URL> URIs;
        }

        // These structures reflect the ASN.1 structure of X.509 certificate
        // signature requests (see RFC 2986):

        private partial struct tbsCertificateRequest
        {
            public asn1.RawContent Raw;
            public long Version;
            public asn1.RawValue Subject;
            public publicKeyInfo PublicKey;
            [Description("asn1:\"tag:0\"")]
            public slice<asn1.RawValue> RawAttributes;
        }

        private partial struct certificateRequest
        {
            public asn1.RawContent Raw;
            public tbsCertificateRequest TBSCSR;
            public pkix.AlgorithmIdentifier SignatureAlgorithm;
            public asn1.BitString SignatureValue;
        }

        // oidExtensionRequest is a PKCS#9 OBJECT IDENTIFIER that indicates requested
        // extensions in a CSR.
        private static asn1.ObjectIdentifier oidExtensionRequest = new asn1.ObjectIdentifier(1,2,840,113549,1,9,14);

        // newRawAttributes converts AttributeTypeAndValueSETs from a template
        // CertificateRequest's Attributes into tbsCertificateRequest RawAttributes.
        private static (slice<asn1.RawValue>, error) newRawAttributes(slice<pkix.AttributeTypeAndValueSET> attributes)
        {
            slice<asn1.RawValue> rawAttributes = default;
            var (b, err) = asn1.Marshal(attributes);
            if (err != null)
            {
                return (null, err);
            }
            var (rest, err) = asn1.Unmarshal(b, ref rawAttributes);
            if (err != null)
            {
                return (null, err);
            }
            if (len(rest) != 0L)
            {
                return (null, errors.New("x509: failed to unmarshal raw CSR Attributes"));
            }
            return (rawAttributes, null);
        }

        // parseRawAttributes Unmarshals RawAttributes intos AttributeTypeAndValueSETs.
        private static slice<pkix.AttributeTypeAndValueSET> parseRawAttributes(slice<asn1.RawValue> rawAttributes)
        {
            slice<pkix.AttributeTypeAndValueSET> attributes = default;
            foreach (var (_, rawAttr) in rawAttributes)
            {
                pkix.AttributeTypeAndValueSET attr = default;
                var (rest, err) = asn1.Unmarshal(rawAttr.FullBytes, ref attr); 
                // Ignore attributes that don't parse into pkix.AttributeTypeAndValueSET
                // (i.e.: challengePassword or unstructuredName).
                if (err == null && len(rest) == 0L)
                {
                    attributes = append(attributes, attr);
                }
            }
            return attributes;
        }

        // parseCSRExtensions parses the attributes from a CSR and extracts any
        // requested extensions.
        private static (slice<pkix.Extension>, error) parseCSRExtensions(slice<asn1.RawValue> rawAttributes)
        { 
            // pkcs10Attribute reflects the Attribute structure from section 4.1 of
            // https://tools.ietf.org/html/rfc2986.
            private partial struct pkcs10Attribute
            {
                public asn1.ObjectIdentifier Id;
                [Description("asn1:\"set\"")]
                public slice<asn1.RawValue> Values;
            }

            slice<pkix.Extension> ret = default;
            foreach (var (_, rawAttr) in rawAttributes)
            {
                pkcs10Attribute attr = default;
                {
                    var (rest, err) = asn1.Unmarshal(rawAttr.FullBytes, ref attr);

                    if (err != null || len(rest) != 0L || len(attr.Values) == 0L)
                    { 
                        // Ignore attributes that don't parse.
                        continue;
                    }

                }

                if (!attr.Id.Equal(oidExtensionRequest))
                {
                    continue;
                }
                slice<pkix.Extension> extensions = default;
                {
                    var (_, err) = asn1.Unmarshal(attr.Values[0L].FullBytes, ref extensions);

                    if (err != null)
                    {
                        return (null, err);
                    }

                }
                ret = append(ret, extensions);
            }
            return (ret, null);
        }

        // CreateCertificateRequest creates a new certificate request based on a
        // template. The following members of template are used: Attributes, DNSNames,
        // EmailAddresses, ExtraExtensions, IPAddresses, URIs, SignatureAlgorithm, and
        // Subject. The private key is the private key of the signer.
        //
        // The returned slice is the certificate request in DER encoding.
        //
        // All keys types that are implemented via crypto.Signer are supported (This
        // includes *rsa.PublicKey and *ecdsa.PublicKey.)
        public static (slice<byte>, error) CreateCertificateRequest(io.Reader rand, ref CertificateRequest template, object priv)
        {
            crypto.Signer (key, ok) = priv._<crypto.Signer>();
            if (!ok)
            {
                return (null, errors.New("x509: certificate private key does not implement crypto.Signer"));
            }
            crypto.Hash hashFunc = default;
            pkix.AlgorithmIdentifier sigAlgo = default;
            hashFunc, sigAlgo, err = signingParamsForPublicKey(key.Public(), template.SignatureAlgorithm);
            if (err != null)
            {
                return (null, err);
            }
            slice<byte> publicKeyBytes = default;
            pkix.AlgorithmIdentifier publicKeyAlgorithm = default;
            publicKeyBytes, publicKeyAlgorithm, err = marshalPublicKey(key.Public());
            if (err != null)
            {
                return (null, err);
            }
            slice<pkix.Extension> extensions = default;

            if ((len(template.DNSNames) > 0L || len(template.EmailAddresses) > 0L || len(template.IPAddresses) > 0L || len(template.URIs) > 0L) && !oidInExtensions(oidExtensionSubjectAltName, template.ExtraExtensions))
            {
                var (sanBytes, err) = marshalSANs(template.DNSNames, template.EmailAddresses, template.IPAddresses, template.URIs);
                if (err != null)
                {
                    return (null, err);
                }
                extensions = append(extensions, new pkix.Extension(Id:oidExtensionSubjectAltName,Value:sanBytes,));
            }
            extensions = append(extensions, template.ExtraExtensions);

            slice<pkix.AttributeTypeAndValueSET> attributes = default;
            attributes = append(attributes, template.Attributes);

            if (len(extensions) > 0L)
            { 
                // specifiedExtensions contains all the extensions that we
                // found specified via template.Attributes.
                var specifiedExtensions = make_map<@string, bool>();

                {
                    var atvSet__prev1 = atvSet;

                    foreach (var (_, __atvSet) in template.Attributes)
                    {
                        atvSet = __atvSet;
                        if (!atvSet.Type.Equal(oidExtensionRequest))
                        {
                            continue;
                        }
                        {
                            var atvs__prev2 = atvs;

                            foreach (var (_, __atvs) in atvSet.Value)
                            {
                                atvs = __atvs;
                                foreach (var (_, atv) in atvs)
                                {
                                    specifiedExtensions[atv.Type.String()] = true;
                                }
                            }

                            atvs = atvs__prev2;
                        }

                    }

                    atvSet = atvSet__prev1;
                }

                var atvs = make_slice<pkix.AttributeTypeAndValue>(0L, len(extensions));
                foreach (var (_, e) in extensions)
                {
                    if (specifiedExtensions[e.Id.String()])
                    { 
                        // Attributes already contained a value for
                        // this extension and it takes priority.
                        continue;
                    }
                    atvs = append(atvs, new pkix.AttributeTypeAndValue(Type:e.Id,Value:e.Value,));
                } 

                // Append the extensions to an existing attribute if possible.
                var appended = false;
                {
                    var atvSet__prev1 = atvSet;

                    foreach (var (_, __atvSet) in attributes)
                    {
                        atvSet = __atvSet;
                        if (!atvSet.Type.Equal(oidExtensionRequest) || len(atvSet.Value) == 0L)
                        {
                            continue;
                        }
                        atvSet.Value[0L] = append(atvSet.Value[0L], atvs);
                        appended = true;
                        break;
                    } 

                    // Otherwise, add a new attribute for the extensions.

                    atvSet = atvSet__prev1;
                }

                if (!appended)
                {
                    attributes = append(attributes, new pkix.AttributeTypeAndValueSET(Type:oidExtensionRequest,Value:[][]pkix.AttributeTypeAndValue{atvs,},));
                }
            }
            var asn1Subject = template.RawSubject;
            if (len(asn1Subject) == 0L)
            {
                asn1Subject, err = asn1.Marshal(template.Subject.ToRDNSequence());
                if (err != null)
                {
                    return;
                }
            }
            var (rawAttributes, err) = newRawAttributes(attributes);
            if (err != null)
            {
                return;
            }
            tbsCertificateRequest tbsCSR = new tbsCertificateRequest(Version:0,Subject:asn1.RawValue{FullBytes:asn1Subject},PublicKey:publicKeyInfo{Algorithm:publicKeyAlgorithm,PublicKey:asn1.BitString{Bytes:publicKeyBytes,BitLength:len(publicKeyBytes)*8,},},RawAttributes:rawAttributes,);

            var (tbsCSRContents, err) = asn1.Marshal(tbsCSR);
            if (err != null)
            {
                return;
            }
            tbsCSR.Raw = tbsCSRContents;

            var h = hashFunc.New();
            h.Write(tbsCSRContents);
            var digest = h.Sum(null);

            slice<byte> signature = default;
            signature, err = key.Sign(rand, digest, hashFunc);
            if (err != null)
            {
                return;
            }
            return asn1.Marshal(new certificateRequest(TBSCSR:tbsCSR,SignatureAlgorithm:sigAlgo,SignatureValue:asn1.BitString{Bytes:signature,BitLength:len(signature)*8,},));
        }

        // ParseCertificateRequest parses a single certificate request from the
        // given ASN.1 DER data.
        public static (ref CertificateRequest, error) ParseCertificateRequest(slice<byte> asn1Data)
        {
            certificateRequest csr = default;

            var (rest, err) = asn1.Unmarshal(asn1Data, ref csr);
            if (err != null)
            {
                return (null, err);
            }
            else if (len(rest) != 0L)
            {
                return (null, new asn1.SyntaxError(Msg:"trailing data"));
            }
            return parseCertificateRequest(ref csr);
        }

        private static (ref CertificateRequest, error) parseCertificateRequest(ref certificateRequest @in)
        {
            CertificateRequest @out = ref new CertificateRequest(Raw:in.Raw,RawTBSCertificateRequest:in.TBSCSR.Raw,RawSubjectPublicKeyInfo:in.TBSCSR.PublicKey.Raw,RawSubject:in.TBSCSR.Subject.FullBytes,Signature:in.SignatureValue.RightAlign(),SignatureAlgorithm:getSignatureAlgorithmFromAI(in.SignatureAlgorithm),PublicKeyAlgorithm:getPublicKeyAlgorithmFromOID(in.TBSCSR.PublicKey.Algorithm.Algorithm),Version:in.TBSCSR.Version,Attributes:parseRawAttributes(in.TBSCSR.RawAttributes),);

            error err = default;
            @out.PublicKey, err = parsePublicKey(@out.PublicKeyAlgorithm, ref @in.TBSCSR.PublicKey);
            if (err != null)
            {
                return (null, err);
            }
            pkix.RDNSequence subject = default;
            {
                var (rest, err) = asn1.Unmarshal(@in.TBSCSR.Subject.FullBytes, ref subject);

                if (err != null)
                {
                    return (null, err);
                }
                else if (len(rest) != 0L)
                {
                    return (null, errors.New("x509: trailing data after X.509 Subject"));
                }

            }

            @out.Subject.FillFromRDNSequence(ref subject);

            @out.Extensions, err = parseCSRExtensions(@in.TBSCSR.RawAttributes);

            if (err != null)
            {
                return (null, err);
            }
            foreach (var (_, extension) in @out.Extensions)
            {
                if (extension.Id.Equal(oidExtensionSubjectAltName))
                {
                    @out.DNSNames, @out.EmailAddresses, @out.IPAddresses, @out.URIs, err = parseSANExtension(extension.Value);
                    if (err != null)
                    {
                        return (null, err);
                    }
                }
            }
            return (out, null);
        }

        // CheckSignature reports whether the signature on c is valid.
        private static error CheckSignature(this ref CertificateRequest c)
        {
            return error.As(checkSignature(c.SignatureAlgorithm, c.RawTBSCertificateRequest, c.Signature, c.PublicKey));
        }
    }
}}
