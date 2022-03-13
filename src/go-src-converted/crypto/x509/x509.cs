// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package x509 parses X.509-encoded keys and certificates.

// package x509 -- go2cs converted at 2022 March 13 05:34:59 UTC
// import "crypto/x509" ==> using x509 = go.crypto.x509_package
// Original source: C:\Program Files\Go\src\crypto\x509\x509.go
namespace go.crypto;

using bytes = bytes_package;
using crypto = crypto_package;
using ecdsa = crypto.ecdsa_package;
using ed25519 = crypto.ed25519_package;
using elliptic = crypto.elliptic_package;
using rsa = crypto.rsa_package;
using sha1 = crypto.sha1_package;
using pkix = crypto.x509.pkix_package;
using asn1 = encoding.asn1_package;
using pem = encoding.pem_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using big = math.big_package;
using net = net_package;
using url = net.url_package;
using strconv = strconv_package;
using time = time_package;
using unicode = unicode_package; 

// Explicitly import these for their crypto.RegisterHash init side-effects.
// Keep these as blank imports, even if they're imported above.
using sha1 = crypto.sha1_package;
using _sha256_ = crypto.sha256_package;
using _sha512_ = crypto.sha512_package;

using cryptobyte = golang.org.x.crypto.cryptobyte_package;
using cryptobyte_asn1 = golang.org.x.crypto.cryptobyte.asn1_package;


// pkixPublicKey reflects a PKIX public key structure. See SubjectPublicKeyInfo
// in RFC 3280.

using System.ComponentModel;
using System;
public static partial class x509_package {

private partial struct pkixPublicKey {
    public pkix.AlgorithmIdentifier Algo;
    public asn1.BitString BitString;
}

// ParsePKIXPublicKey parses a public key in PKIX, ASN.1 DER form.
// The encoded public key is a SubjectPublicKeyInfo structure
// (see RFC 5280, Section 4.1).
//
// It returns a *rsa.PublicKey, *dsa.PublicKey, *ecdsa.PublicKey, or
// ed25519.PublicKey. More types might be supported in the future.
//
// This kind of key is commonly encoded in PEM blocks of type "PUBLIC KEY".
public static (object, error) ParsePKIXPublicKey(slice<byte> derBytes) {
    object pub = default;
    error err = default!;

    ref publicKeyInfo pki = ref heap(out ptr<publicKeyInfo> _addr_pki);
    {
        var (rest, err) = asn1.Unmarshal(derBytes, _addr_pki);

        if (err != null) {
            {
                var (_, err) = asn1.Unmarshal(derBytes, addr(new pkcs1PublicKey()));

                if (err == null) {
                    return (null, error.As(errors.New("x509: failed to parse public key (use ParsePKCS1PublicKey instead for this key format)"))!);
                }

            }
            return (null, error.As(err)!);
        }
        else if (len(rest) != 0) {
            return (null, error.As(errors.New("x509: trailing data after ASN.1 of public-key"))!);
        }

    }
    var algo = getPublicKeyAlgorithmFromOID(pki.Algorithm.Algorithm);
    if (algo == UnknownPublicKeyAlgorithm) {
        return (null, error.As(errors.New("x509: unknown public key algorithm"))!);
    }
    return parsePublicKey(algo, _addr_pki);
}

private static (slice<byte>, pkix.AlgorithmIdentifier, error) marshalPublicKey(object pub) {
    slice<byte> publicKeyBytes = default;
    pkix.AlgorithmIdentifier publicKeyAlgorithm = default;
    error err = default!;

    switch (pub.type()) {
        case ptr<rsa.PublicKey> pub:
            publicKeyBytes, err = asn1.Marshal(new pkcs1PublicKey(N:pub.N,E:pub.E,));
            if (err != null) {
                return (null, new pkix.AlgorithmIdentifier(), error.As(err)!);
            }
            publicKeyAlgorithm.Algorithm = oidPublicKeyRSA; 
            // This is a NULL parameters value which is required by
            // RFC 3279, Section 2.3.1.
            publicKeyAlgorithm.Parameters = asn1.NullRawValue;
            break;
        case ptr<ecdsa.PublicKey> pub:
            publicKeyBytes = elliptic.Marshal(pub.Curve, pub.X, pub.Y);
            var (oid, ok) = oidFromNamedCurve(pub.Curve);
            if (!ok) {
                return (null, new pkix.AlgorithmIdentifier(), error.As(errors.New("x509: unsupported elliptic curve"))!);
            }
            publicKeyAlgorithm.Algorithm = oidPublicKeyECDSA;
            slice<byte> paramBytes = default;
            paramBytes, err = asn1.Marshal(oid);
            if (err != null) {
                return ;
            }
            publicKeyAlgorithm.Parameters.FullBytes = paramBytes;
            break;
        case ed25519.PublicKey pub:
            publicKeyBytes = pub;
            publicKeyAlgorithm.Algorithm = oidPublicKeyEd25519;
            break;
        default:
        {
            var pub = pub.type();
            return (null, new pkix.AlgorithmIdentifier(), error.As(fmt.Errorf("x509: unsupported public key type: %T", pub))!);
            break;
        }

    }

    return (publicKeyBytes, publicKeyAlgorithm, error.As(null!)!);
}

// MarshalPKIXPublicKey converts a public key to PKIX, ASN.1 DER form.
// The encoded public key is a SubjectPublicKeyInfo structure
// (see RFC 5280, Section 4.1).
//
// The following key types are currently supported: *rsa.PublicKey, *ecdsa.PublicKey
// and ed25519.PublicKey. Unsupported key types result in an error.
//
// This kind of key is commonly encoded in PEM blocks of type "PUBLIC KEY".
public static (slice<byte>, error) MarshalPKIXPublicKey(object pub) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    slice<byte> publicKeyBytes = default;
    pkix.AlgorithmIdentifier publicKeyAlgorithm = default;
    error err = default!;

    publicKeyBytes, publicKeyAlgorithm, err = marshalPublicKey(pub);

    if (err != null) {
        return (null, error.As(err)!);
    }
    pkixPublicKey pkix = new pkixPublicKey(Algo:publicKeyAlgorithm,BitString:asn1.BitString{Bytes:publicKeyBytes,BitLength:8*len(publicKeyBytes),},);

    var (ret, _) = asn1.Marshal(pkix);
    return (ret, error.As(null!)!);
}

// These structures reflect the ASN.1 structure of X.509 certificates.:

private partial struct certificate {
    public asn1.RawContent Raw;
    public tbsCertificate TBSCertificate;
    public pkix.AlgorithmIdentifier SignatureAlgorithm;
    public asn1.BitString SignatureValue;
}

private partial struct tbsCertificate {
    public asn1.RawContent Raw;
    [Description("asn1:\"optional,explicit,default:0,tag:0\"")]
    public nint Version;
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

private partial struct dsaAlgorithmParameters {
    public ptr<big.Int> P;
    public ptr<big.Int> Q;
    public ptr<big.Int> G;
}

private partial struct validity {
    public time.Time NotBefore;
    public time.Time NotAfter;
}

private partial struct publicKeyInfo {
    public asn1.RawContent Raw;
    public pkix.AlgorithmIdentifier Algorithm;
    public asn1.BitString PublicKey;
}

// RFC 5280,  4.2.1.1
private partial struct authKeyId {
    [Description("asn1:\"optional,tag:0\"")]
    public slice<byte> Id;
}

public partial struct SignatureAlgorithm { // : nint
}

public static readonly SignatureAlgorithm UnknownSignatureAlgorithm = iota;

public static readonly var MD2WithRSA = 0; // Unsupported.
public static readonly var MD5WithRSA = 1; // Only supported for signing, not verification.
public static readonly var SHA1WithRSA = 2;
public static readonly var SHA256WithRSA = 3;
public static readonly var SHA384WithRSA = 4;
public static readonly var SHA512WithRSA = 5;
public static readonly var DSAWithSHA1 = 6; // Unsupported.
public static readonly var DSAWithSHA256 = 7; // Unsupported.
public static readonly var ECDSAWithSHA1 = 8;
public static readonly var ECDSAWithSHA256 = 9;
public static readonly var ECDSAWithSHA384 = 10;
public static readonly var ECDSAWithSHA512 = 11;
public static readonly var SHA256WithRSAPSS = 12;
public static readonly var SHA384WithRSAPSS = 13;
public static readonly var SHA512WithRSAPSS = 14;
public static readonly var PureEd25519 = 15;

public static bool isRSAPSS(this SignatureAlgorithm algo) {

    if (algo == SHA256WithRSAPSS || algo == SHA384WithRSAPSS || algo == SHA512WithRSAPSS) 
        return true;
    else 
        return false;
    }

public static @string String(this SignatureAlgorithm algo) {
    foreach (var (_, details) in signatureAlgorithmDetails) {
        if (details.algo == algo) {
            return details.name;
        }
    }    return strconv.Itoa(int(algo));
}

public partial struct PublicKeyAlgorithm { // : nint
}

public static readonly PublicKeyAlgorithm UnknownPublicKeyAlgorithm = iota;
public static readonly var RSA = 0;
public static readonly var DSA = 1; // Unsupported.
public static readonly var ECDSA = 2;
public static readonly var Ed25519 = 3;

private static array<@string> publicKeyAlgoName = new array<@string>(InitKeyedValues<@string>((RSA, "RSA"), (DSA, "DSA"), (ECDSA, "ECDSA"), (Ed25519, "Ed25519")));

public static @string String(this PublicKeyAlgorithm algo) {
    if (0 < algo && int(algo) < len(publicKeyAlgoName)) {
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
//
//
// RFC 8410 3 Curve25519 and Curve448 Algorithm Identifiers
//
// id-Ed25519   OBJECT IDENTIFIER ::= { 1 3 101 112 }

private static asn1.ObjectIdentifier oidSignatureMD2WithRSA = new asn1.ObjectIdentifier(1,2,840,113549,1,1,2);private static asn1.ObjectIdentifier oidSignatureMD5WithRSA = new asn1.ObjectIdentifier(1,2,840,113549,1,1,4);private static asn1.ObjectIdentifier oidSignatureSHA1WithRSA = new asn1.ObjectIdentifier(1,2,840,113549,1,1,5);private static asn1.ObjectIdentifier oidSignatureSHA256WithRSA = new asn1.ObjectIdentifier(1,2,840,113549,1,1,11);private static asn1.ObjectIdentifier oidSignatureSHA384WithRSA = new asn1.ObjectIdentifier(1,2,840,113549,1,1,12);private static asn1.ObjectIdentifier oidSignatureSHA512WithRSA = new asn1.ObjectIdentifier(1,2,840,113549,1,1,13);private static asn1.ObjectIdentifier oidSignatureRSAPSS = new asn1.ObjectIdentifier(1,2,840,113549,1,1,10);private static asn1.ObjectIdentifier oidSignatureDSAWithSHA1 = new asn1.ObjectIdentifier(1,2,840,10040,4,3);private static asn1.ObjectIdentifier oidSignatureDSAWithSHA256 = new asn1.ObjectIdentifier(2,16,840,1,101,3,4,3,2);private static asn1.ObjectIdentifier oidSignatureECDSAWithSHA1 = new asn1.ObjectIdentifier(1,2,840,10045,4,1);private static asn1.ObjectIdentifier oidSignatureECDSAWithSHA256 = new asn1.ObjectIdentifier(1,2,840,10045,4,3,2);private static asn1.ObjectIdentifier oidSignatureECDSAWithSHA384 = new asn1.ObjectIdentifier(1,2,840,10045,4,3,3);private static asn1.ObjectIdentifier oidSignatureECDSAWithSHA512 = new asn1.ObjectIdentifier(1,2,840,10045,4,3,4);private static asn1.ObjectIdentifier oidSignatureEd25519 = new asn1.ObjectIdentifier(1,3,101,112);private static asn1.ObjectIdentifier oidSHA256 = new asn1.ObjectIdentifier(2,16,840,1,101,3,4,2,1);private static asn1.ObjectIdentifier oidSHA384 = new asn1.ObjectIdentifier(2,16,840,1,101,3,4,2,2);private static asn1.ObjectIdentifier oidSHA512 = new asn1.ObjectIdentifier(2,16,840,1,101,3,4,2,3);private static asn1.ObjectIdentifier oidMGF1 = new asn1.ObjectIdentifier(1,2,840,113549,1,1,8);private static asn1.ObjectIdentifier oidISOSignatureSHA1WithRSA = new asn1.ObjectIdentifier(1,3,14,3,2,29);



// hashToPSSParameters contains the DER encoded RSA PSS parameters for the
// SHA256, SHA384, and SHA512 hashes as defined in RFC 3447, Appendix A.2.3.
// The parameters contain the following values:
//   * hashAlgorithm contains the associated hash identifier with NULL parameters
//   * maskGenAlgorithm always contains the default mgf1SHA1 identifier
//   * saltLength contains the length of the associated hash
//   * trailerField always contains the default trailerFieldBC value
private static map hashToPSSParameters = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<crypto.Hash, asn1.RawValue>{crypto.SHA256:asn1.RawValue{FullBytes:[]byte{48,52,160,15,48,13,6,9,96,134,72,1,101,3,4,2,1,5,0,161,28,48,26,6,9,42,134,72,134,247,13,1,1,8,48,13,6,9,96,134,72,1,101,3,4,2,1,5,0,162,3,2,1,32}},crypto.SHA384:asn1.RawValue{FullBytes:[]byte{48,52,160,15,48,13,6,9,96,134,72,1,101,3,4,2,2,5,0,161,28,48,26,6,9,42,134,72,134,247,13,1,1,8,48,13,6,9,96,134,72,1,101,3,4,2,2,5,0,162,3,2,1,48}},crypto.SHA512:asn1.RawValue{FullBytes:[]byte{48,52,160,15,48,13,6,9,96,134,72,1,101,3,4,2,3,5,0,161,28,48,26,6,9,42,134,72,134,247,13,1,1,8,48,13,6,9,96,134,72,1,101,3,4,2,3,5,0,162,3,2,1,64}},};

// pssParameters reflects the parameters in an AlgorithmIdentifier that
// specifies RSA PSS. See RFC 3447, Appendix A.2.3.
private partial struct pssParameters {
    [Description("asn1:\"explicit,tag:0\"")]
    public pkix.AlgorithmIdentifier Hash;
    [Description("asn1:\"explicit,tag:1\"")]
    public pkix.AlgorithmIdentifier MGF;
    [Description("asn1:\"explicit,tag:2\"")]
    public nint SaltLength;
    [Description("asn1:\"optional,explicit,tag:3,default:1\"")]
    public nint TrailerField;
}

private static SignatureAlgorithm getSignatureAlgorithmFromAI(pkix.AlgorithmIdentifier ai) {
    if (ai.Algorithm.Equal(oidSignatureEd25519)) { 
        // RFC 8410, Section 3
        // > For all of the OIDs, the parameters MUST be absent.
        if (len(ai.Parameters.FullBytes) != 0) {
            return UnknownSignatureAlgorithm;
        }
    }
    if (!ai.Algorithm.Equal(oidSignatureRSAPSS)) {
        foreach (var (_, details) in signatureAlgorithmDetails) {
            if (ai.Algorithm.Equal(details.oid)) {
                return details.algo;
            }
        }        return UnknownSignatureAlgorithm;
    }
    ref pssParameters @params = ref heap(out ptr<pssParameters> _addr_@params);
    {
        var (_, err) = asn1.Unmarshal(ai.Parameters.FullBytes, _addr_params);

        if (err != null) {
            return UnknownSignatureAlgorithm;
        }
    }

    ref pkix.AlgorithmIdentifier mgf1HashFunc = ref heap(out ptr<pkix.AlgorithmIdentifier> _addr_mgf1HashFunc);
    {
        (_, err) = asn1.Unmarshal(@params.MGF.Parameters.FullBytes, _addr_mgf1HashFunc);

        if (err != null) {
            return UnknownSignatureAlgorithm;
        }
    } 

    // PSS is greatly overburdened with options. This code forces them into
    // three buckets by requiring that the MGF1 hash function always match the
    // message hash function (as recommended in RFC 3447, Section 8.1), that the
    // salt length matches the hash length, and that the trailer field has the
    // default value.
    if ((len(@params.Hash.Parameters.FullBytes) != 0 && !bytes.Equal(@params.Hash.Parameters.FullBytes, asn1.NullBytes)) || !@params.MGF.Algorithm.Equal(oidMGF1) || !mgf1HashFunc.Algorithm.Equal(@params.Hash.Algorithm) || (len(mgf1HashFunc.Parameters.FullBytes) != 0 && !bytes.Equal(mgf1HashFunc.Parameters.FullBytes, asn1.NullBytes)) || @params.TrailerField != 1) {
        return UnknownSignatureAlgorithm;
    }

    if (@params.Hash.Algorithm.Equal(oidSHA256) && @params.SaltLength == 32) 
        return SHA256WithRSAPSS;
    else if (@params.Hash.Algorithm.Equal(oidSHA384) && @params.SaltLength == 48) 
        return SHA384WithRSAPSS;
    else if (@params.Hash.Algorithm.Equal(oidSHA512) && @params.SaltLength == 64) 
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
private static asn1.ObjectIdentifier oidPublicKeyRSA = new asn1.ObjectIdentifier(1,2,840,113549,1,1,1);private static asn1.ObjectIdentifier oidPublicKeyDSA = new asn1.ObjectIdentifier(1,2,840,10040,4,1);private static asn1.ObjectIdentifier oidPublicKeyECDSA = new asn1.ObjectIdentifier(1,2,840,10045,2,1);private static var oidPublicKeyEd25519 = oidSignatureEd25519;

private static PublicKeyAlgorithm getPublicKeyAlgorithmFromOID(asn1.ObjectIdentifier oid) {

    if (oid.Equal(oidPublicKeyRSA)) 
        return RSA;
    else if (oid.Equal(oidPublicKeyDSA)) 
        return DSA;
    else if (oid.Equal(oidPublicKeyECDSA)) 
        return ECDSA;
    else if (oid.Equal(oidPublicKeyEd25519)) 
        return Ed25519;
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
private static asn1.ObjectIdentifier oidNamedCurveP224 = new asn1.ObjectIdentifier(1,3,132,0,33);private static asn1.ObjectIdentifier oidNamedCurveP256 = new asn1.ObjectIdentifier(1,2,840,10045,3,1,7);private static asn1.ObjectIdentifier oidNamedCurveP384 = new asn1.ObjectIdentifier(1,3,132,0,34);private static asn1.ObjectIdentifier oidNamedCurveP521 = new asn1.ObjectIdentifier(1,3,132,0,35);

private static elliptic.Curve namedCurveFromOID(asn1.ObjectIdentifier oid) {

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

private static (asn1.ObjectIdentifier, bool) oidFromNamedCurve(elliptic.Curve curve) {
    asn1.ObjectIdentifier _p0 = default;
    bool _p0 = default;


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
public partial struct KeyUsage { // : nint
}

public static readonly KeyUsage KeyUsageDigitalSignature = 1 << (int)(iota);
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
private static asn1.ObjectIdentifier oidExtKeyUsageAny = new asn1.ObjectIdentifier(2,5,29,37,0);private static asn1.ObjectIdentifier oidExtKeyUsageServerAuth = new asn1.ObjectIdentifier(1,3,6,1,5,5,7,3,1);private static asn1.ObjectIdentifier oidExtKeyUsageClientAuth = new asn1.ObjectIdentifier(1,3,6,1,5,5,7,3,2);private static asn1.ObjectIdentifier oidExtKeyUsageCodeSigning = new asn1.ObjectIdentifier(1,3,6,1,5,5,7,3,3);private static asn1.ObjectIdentifier oidExtKeyUsageEmailProtection = new asn1.ObjectIdentifier(1,3,6,1,5,5,7,3,4);private static asn1.ObjectIdentifier oidExtKeyUsageIPSECEndSystem = new asn1.ObjectIdentifier(1,3,6,1,5,5,7,3,5);private static asn1.ObjectIdentifier oidExtKeyUsageIPSECTunnel = new asn1.ObjectIdentifier(1,3,6,1,5,5,7,3,6);private static asn1.ObjectIdentifier oidExtKeyUsageIPSECUser = new asn1.ObjectIdentifier(1,3,6,1,5,5,7,3,7);private static asn1.ObjectIdentifier oidExtKeyUsageTimeStamping = new asn1.ObjectIdentifier(1,3,6,1,5,5,7,3,8);private static asn1.ObjectIdentifier oidExtKeyUsageOCSPSigning = new asn1.ObjectIdentifier(1,3,6,1,5,5,7,3,9);private static asn1.ObjectIdentifier oidExtKeyUsageMicrosoftServerGatedCrypto = new asn1.ObjectIdentifier(1,3,6,1,4,1,311,10,3,3);private static asn1.ObjectIdentifier oidExtKeyUsageNetscapeServerGatedCrypto = new asn1.ObjectIdentifier(2,16,840,1,113730,4,1);private static asn1.ObjectIdentifier oidExtKeyUsageMicrosoftCommercialCodeSigning = new asn1.ObjectIdentifier(1,3,6,1,4,1,311,2,1,22);private static asn1.ObjectIdentifier oidExtKeyUsageMicrosoftKernelCodeSigning = new asn1.ObjectIdentifier(1,3,6,1,4,1,311,61,1,1);

// ExtKeyUsage represents an extended set of actions that are valid for a given key.
// Each of the ExtKeyUsage* constants define a unique action.
public partial struct ExtKeyUsage { // : nint
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


private static (ExtKeyUsage, bool) extKeyUsageFromOID(asn1.ObjectIdentifier oid) {
    ExtKeyUsage eku = default;
    bool ok = default;

    foreach (var (_, pair) in extKeyUsageOIDs) {
        if (oid.Equal(pair.oid)) {
            return (pair.extKeyUsage, true);
        }
    }    return ;
}

private static (asn1.ObjectIdentifier, bool) oidFromExtKeyUsage(ExtKeyUsage eku) {
    asn1.ObjectIdentifier oid = default;
    bool ok = default;

    foreach (var (_, pair) in extKeyUsageOIDs) {
        if (eku == pair.extKeyUsage) {
            return (pair.oid, true);
        }
    }    return ;
}

// A Certificate represents an X.509 certificate.
public partial struct Certificate {
    public slice<byte> Raw; // Complete ASN.1 DER content (certificate, signature algorithm and signature).
    public slice<byte> RawTBSCertificate; // Certificate part of raw ASN.1 DER content.
    public slice<byte> RawSubjectPublicKeyInfo; // DER encoded SubjectPublicKeyInfo.
    public slice<byte> RawSubject; // DER encoded Subject
    public slice<byte> RawIssuer; // DER encoded Issuer

    public slice<byte> Signature;
    public SignatureAlgorithm SignatureAlgorithm;
    public PublicKeyAlgorithm PublicKeyAlgorithm;
    public nint Version;
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
    public nint MaxPathLen; // MaxPathLenZero indicates that BasicConstraintsValid==true
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
    public slice<ptr<url.URL>> URIs; // Name constraints
    public bool PermittedDNSDomainsCritical; // if true then the name constraints are marked critical.
    public slice<@string> PermittedDNSDomains;
    public slice<@string> ExcludedDNSDomains;
    public slice<ptr<net.IPNet>> PermittedIPRanges;
    public slice<ptr<net.IPNet>> ExcludedIPRanges;
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
public partial struct InsecureAlgorithmError { // : SignatureAlgorithm
}

public static @string Error(this InsecureAlgorithmError e) {
    return fmt.Sprintf("x509: cannot verify signature: insecure algorithm %v", SignatureAlgorithm(e));
}

// ConstraintViolationError results when a requested usage is not permitted by
// a certificate. For example: checking a signature when the public key isn't a
// certificate signing key.
public partial struct ConstraintViolationError {
}

public static @string Error(this ConstraintViolationError _p0) {
    return "x509: invalid signature: parent certificate cannot sign this kind of certificate";
}

private static bool Equal(this ptr<Certificate> _addr_c, ptr<Certificate> _addr_other) {
    ref Certificate c = ref _addr_c.val;
    ref Certificate other = ref _addr_other.val;

    if (c == null || other == null) {
        return c == other;
    }
    return bytes.Equal(c.Raw, other.Raw);
}

private static bool hasSANExtension(this ptr<Certificate> _addr_c) {
    ref Certificate c = ref _addr_c.val;

    return oidInExtensions(oidExtensionSubjectAltName, c.Extensions);
}

// CheckSignatureFrom verifies that the signature on c is a valid signature
// from parent.
private static error CheckSignatureFrom(this ptr<Certificate> _addr_c, ptr<Certificate> _addr_parent) {
    ref Certificate c = ref _addr_c.val;
    ref Certificate parent = ref _addr_parent.val;
 
    // RFC 5280, 4.2.1.9:
    // "If the basic constraints extension is not present in a version 3
    // certificate, or the extension is present but the cA boolean is not
    // asserted, then the certified public key MUST NOT be used to verify
    // certificate signatures."
    if (parent.Version == 3 && !parent.BasicConstraintsValid || parent.BasicConstraintsValid && !parent.IsCA) {
        return error.As(new ConstraintViolationError())!;
    }
    if (parent.KeyUsage != 0 && parent.KeyUsage & KeyUsageCertSign == 0) {
        return error.As(new ConstraintViolationError())!;
    }
    if (parent.PublicKeyAlgorithm == UnknownPublicKeyAlgorithm) {
        return error.As(ErrUnsupportedAlgorithm)!;
    }
    return error.As(parent.CheckSignature(c.SignatureAlgorithm, c.RawTBSCertificate, c.Signature))!;
}

// CheckSignature verifies that signature is a valid signature over signed from
// c's public key.
private static error CheckSignature(this ptr<Certificate> _addr_c, SignatureAlgorithm algo, slice<byte> signed, slice<byte> signature) {
    ref Certificate c = ref _addr_c.val;

    return error.As(checkSignature(algo, signed, signature, c.PublicKey))!;
}

private static bool hasNameConstraints(this ptr<Certificate> _addr_c) {
    ref Certificate c = ref _addr_c.val;

    return oidInExtensions(oidExtensionNameConstraints, c.Extensions);
}

private static slice<byte> getSANExtension(this ptr<Certificate> _addr_c) {
    ref Certificate c = ref _addr_c.val;

    foreach (var (_, e) in c.Extensions) {
        if (e.Id.Equal(oidExtensionSubjectAltName)) {
            return e.Value;
        }
    }    return null;
}

private static error signaturePublicKeyAlgoMismatchError(PublicKeyAlgorithm expectedPubKeyAlgo, object pubKey) {
    return error.As(fmt.Errorf("x509: signature algorithm specifies an %s public key, but have public key of type %T", expectedPubKeyAlgo.String(), pubKey))!;
}

// CheckSignature verifies that signature is a valid signature over signed from
// a crypto.PublicKey.
private static error checkSignature(SignatureAlgorithm algo, slice<byte> signed, slice<byte> signature, crypto.PublicKey publicKey) {
    error err = default!;

    crypto.Hash hashType = default;
    PublicKeyAlgorithm pubKeyAlgo = default;

    foreach (var (_, details) in signatureAlgorithmDetails) {
        if (details.algo == algo) {
            hashType = details.hash;
            pubKeyAlgo = details.pubKeyAlgo;
        }
    }
    if (hashType == crypto.Hash(0)) 
        if (pubKeyAlgo != Ed25519) {
            return error.As(ErrUnsupportedAlgorithm)!;
        }
    else if (hashType == crypto.MD5) 
        return error.As(InsecureAlgorithmError(algo))!;
    else 
        if (!hashType.Available()) {
            return error.As(ErrUnsupportedAlgorithm)!;
        }
        var h = hashType.New();
        h.Write(signed);
        signed = h.Sum(null);
        switch (publicKey.type()) {
        case ptr<rsa.PublicKey> pub:
            if (pubKeyAlgo != RSA) {
                return error.As(signaturePublicKeyAlgoMismatchError(pubKeyAlgo, pub))!;
            }
            if (algo.isRSAPSS()) {
                return error.As(rsa.VerifyPSS(pub, hashType, signed, signature, addr(new rsa.PSSOptions(SaltLength:rsa.PSSSaltLengthEqualsHash))))!;
            }
            else
 {
                return error.As(rsa.VerifyPKCS1v15(pub, hashType, signed, signature))!;
            }
            break;
        case ptr<ecdsa.PublicKey> pub:
            if (pubKeyAlgo != ECDSA) {
                return error.As(signaturePublicKeyAlgoMismatchError(pubKeyAlgo, pub))!;
            }
            if (!ecdsa.VerifyASN1(pub, signed, signature)) {
                return error.As(errors.New("x509: ECDSA verification failure"))!;
            }
            return ;
            break;
        case ed25519.PublicKey pub:
            if (pubKeyAlgo != Ed25519) {
                return error.As(signaturePublicKeyAlgoMismatchError(pubKeyAlgo, pub))!;
            }
            if (!ed25519.Verify(pub, signed, signature)) {
                return error.As(errors.New("x509: Ed25519 verification failure"))!;
            }
            return ;
            break;
    }
    return error.As(ErrUnsupportedAlgorithm)!;
}

// CheckCRLSignature checks that the signature in crl is from c.
private static error CheckCRLSignature(this ptr<Certificate> _addr_c, ptr<pkix.CertificateList> _addr_crl) {
    ref Certificate c = ref _addr_c.val;
    ref pkix.CertificateList crl = ref _addr_crl.val;

    var algo = getSignatureAlgorithmFromAI(crl.SignatureAlgorithm);
    return error.As(c.CheckSignature(algo, crl.TBSCertList.Raw, crl.SignatureValue.RightAlign()))!;
}

public partial struct UnhandledCriticalExtension {
}

public static @string Error(this UnhandledCriticalExtension h) {
    return "x509: unhandled critical extension";
}

private partial struct basicConstraints {
    [Description("asn1:\"optional\"")]
    public bool IsCA;
    [Description("asn1:\"optional,default:-1\"")]
    public nint MaxPathLen;
}

// RFC 5280 4.2.1.4
private partial struct policyInformation {
    public asn1.ObjectIdentifier Policy; // policyQualifiers omitted
}

private static readonly nint nameTypeEmail = 1;
private static readonly nint nameTypeDNS = 2;
private static readonly nint nameTypeURI = 6;
private static readonly nint nameTypeIP = 7;

// RFC 5280, 4.2.2.1
private partial struct authorityInfoAccess {
    public asn1.ObjectIdentifier Method;
    public asn1.RawValue Location;
}

// RFC 5280, 4.2.1.14
private partial struct distributionPoint {
    [Description("asn1:\"optional,tag:0\"")]
    public distributionPointName DistributionPoint;
    [Description("asn1:\"optional,tag:1\"")]
    public asn1.BitString Reason;
    [Description("asn1:\"optional,tag:2\"")]
    public asn1.RawValue CRLIssuer;
}

private partial struct distributionPointName {
    [Description("asn1:\"optional,tag:0\"")]
    public slice<asn1.RawValue> FullName;
    [Description("asn1:\"optional,tag:1\"")]
    public pkix.RDNSequence RelativeName;
}

private static byte reverseBitsInAByte(byte @in) {
    var b1 = in >> 4 | in << 4;
    var b2 = b1 >> 2 & 0x33 | b1 << 2 & 0xcc;
    var b3 = b2 >> 1 & 0x55 | b2 << 1 & 0xaa;
    return b3;
}

// asn1BitLength returns the bit-length of bitString by considering the
// most-significant bit in a byte to be the "first" bit. This convention
// matches ASN.1, but differs from almost everything else.
private static nint asn1BitLength(slice<byte> bitString) {
    var bitLen = len(bitString) * 8;

    foreach (var (i) in bitString) {
        var b = bitString[len(bitString) - i - 1];

        for (var bit = uint(0); bit < 8; bit++) {
            if ((b >> (int)(bit)) & 1 == 1) {
                return bitLen;
            }
            bitLen--;
        }
    }    return 0;
}

private static nint oidExtensionSubjectKeyId = new slice<nint>(new nint[] { 2, 5, 29, 14 });private static nint oidExtensionKeyUsage = new slice<nint>(new nint[] { 2, 5, 29, 15 });private static nint oidExtensionExtendedKeyUsage = new slice<nint>(new nint[] { 2, 5, 29, 37 });private static nint oidExtensionAuthorityKeyId = new slice<nint>(new nint[] { 2, 5, 29, 35 });private static nint oidExtensionBasicConstraints = new slice<nint>(new nint[] { 2, 5, 29, 19 });private static nint oidExtensionSubjectAltName = new slice<nint>(new nint[] { 2, 5, 29, 17 });private static nint oidExtensionCertificatePolicies = new slice<nint>(new nint[] { 2, 5, 29, 32 });private static nint oidExtensionNameConstraints = new slice<nint>(new nint[] { 2, 5, 29, 30 });private static nint oidExtensionCRLDistributionPoints = new slice<nint>(new nint[] { 2, 5, 29, 31 });private static nint oidExtensionAuthorityInfoAccess = new slice<nint>(new nint[] { 1, 3, 6, 1, 5, 5, 7, 1, 1 });private static nint oidExtensionCRLNumber = new slice<nint>(new nint[] { 2, 5, 29, 20 });

private static asn1.ObjectIdentifier oidAuthorityInfoAccessOcsp = new asn1.ObjectIdentifier(1,3,6,1,5,5,7,48,1);private static asn1.ObjectIdentifier oidAuthorityInfoAccessIssuers = new asn1.ObjectIdentifier(1,3,6,1,5,5,7,48,2);

// oidNotInExtensions reports whether an extension with the given oid exists in
// extensions.
private static bool oidInExtensions(asn1.ObjectIdentifier oid, slice<pkix.Extension> extensions) {
    foreach (var (_, e) in extensions) {
        if (e.Id.Equal(oid)) {
            return true;
        }
    }    return false;
}

// marshalSANs marshals a list of addresses into a the contents of an X.509
// SubjectAlternativeName extension.
private static (slice<byte>, error) marshalSANs(slice<@string> dnsNames, slice<@string> emailAddresses, slice<net.IP> ipAddresses, slice<ptr<url.URL>> uris) {
    slice<byte> derBytes = default;
    error err = default!;

    slice<asn1.RawValue> rawValues = default;
    foreach (var (_, name) in dnsNames) {
        {
            var err__prev1 = err;

            var err = isIA5String(name);

            if (err != null) {
                return (null, error.As(err)!);
            }

            err = err__prev1;

        }
        rawValues = append(rawValues, new asn1.RawValue(Tag:nameTypeDNS,Class:2,Bytes:[]byte(name)));
    }    foreach (var (_, email) in emailAddresses) {
        {
            var err__prev1 = err;

            err = isIA5String(email);

            if (err != null) {
                return (null, error.As(err)!);
            }

            err = err__prev1;

        }
        rawValues = append(rawValues, new asn1.RawValue(Tag:nameTypeEmail,Class:2,Bytes:[]byte(email)));
    }    foreach (var (_, rawIP) in ipAddresses) { 
        // If possible, we always want to encode IPv4 addresses in 4 bytes.
        var ip = rawIP.To4();
        if (ip == null) {
            ip = rawIP;
        }
        rawValues = append(rawValues, new asn1.RawValue(Tag:nameTypeIP,Class:2,Bytes:ip));
    }    foreach (var (_, uri) in uris) {
        var uriStr = uri.String();
        {
            var err__prev1 = err;

            err = isIA5String(uriStr);

            if (err != null) {
                return (null, error.As(err)!);
            }

            err = err__prev1;

        }
        rawValues = append(rawValues, new asn1.RawValue(Tag:nameTypeURI,Class:2,Bytes:[]byte(uriStr)));
    }    return asn1.Marshal(rawValues);
}

private static error isIA5String(@string s) {
    foreach (var (_, r) in s) { 
        // Per RFC5280 "IA5String is limited to the set of ASCII characters"
        if (r > unicode.MaxASCII) {
            return error.As(fmt.Errorf("x509: %q cannot be encoded as an IA5String", s))!;
        }
    }    return error.As(null!)!;
}

private static (slice<pkix.Extension>, error) buildCertExtensions(ptr<Certificate> _addr_template, bool subjectIsEmpty, slice<byte> authorityKeyId, slice<byte> subjectKeyId) {
    slice<pkix.Extension> ret = default;
    error err = default!;
    ref Certificate template = ref _addr_template.val;

    ret = make_slice<pkix.Extension>(10);
    nint n = 0;

    if (template.KeyUsage != 0 && !oidInExtensions(oidExtensionKeyUsage, template.ExtraExtensions)) {
        ret[n], err = marshalKeyUsage(template.KeyUsage);
        if (err != null) {
            return (null, error.As(err)!);
        }
        n++;
    }
    if ((len(template.ExtKeyUsage) > 0 || len(template.UnknownExtKeyUsage) > 0) && !oidInExtensions(oidExtensionExtendedKeyUsage, template.ExtraExtensions)) {
        ret[n], err = marshalExtKeyUsage(template.ExtKeyUsage, template.UnknownExtKeyUsage);
        if (err != null) {
            return (null, error.As(err)!);
        }
        n++;
    }
    if (template.BasicConstraintsValid && !oidInExtensions(oidExtensionBasicConstraints, template.ExtraExtensions)) {
        ret[n], err = marshalBasicConstraints(template.IsCA, template.MaxPathLen, template.MaxPathLenZero);
        if (err != null) {
            return (null, error.As(err)!);
        }
        n++;
    }
    if (len(subjectKeyId) > 0 && !oidInExtensions(oidExtensionSubjectKeyId, template.ExtraExtensions)) {
        ret[n].Id = oidExtensionSubjectKeyId;
        ret[n].Value, err = asn1.Marshal(subjectKeyId);
        if (err != null) {
            return ;
        }
        n++;
    }
    if (len(authorityKeyId) > 0 && !oidInExtensions(oidExtensionAuthorityKeyId, template.ExtraExtensions)) {
        ret[n].Id = oidExtensionAuthorityKeyId;
        ret[n].Value, err = asn1.Marshal(new authKeyId(authorityKeyId));
        if (err != null) {
            return ;
        }
        n++;
    }
    if ((len(template.OCSPServer) > 0 || len(template.IssuingCertificateURL) > 0) && !oidInExtensions(oidExtensionAuthorityInfoAccess, template.ExtraExtensions)) {
        ret[n].Id = oidExtensionAuthorityInfoAccess;
        slice<authorityInfoAccess> aiaValues = default;
        {
            var name__prev1 = name;

            foreach (var (_, __name) in template.OCSPServer) {
                name = __name;
                aiaValues = append(aiaValues, new authorityInfoAccess(Method:oidAuthorityInfoAccessOcsp,Location:asn1.RawValue{Tag:6,Class:2,Bytes:[]byte(name)},));
            }

            name = name__prev1;
        }

        {
            var name__prev1 = name;

            foreach (var (_, __name) in template.IssuingCertificateURL) {
                name = __name;
                aiaValues = append(aiaValues, new authorityInfoAccess(Method:oidAuthorityInfoAccessIssuers,Location:asn1.RawValue{Tag:6,Class:2,Bytes:[]byte(name)},));
            }

            name = name__prev1;
        }

        ret[n].Value, err = asn1.Marshal(aiaValues);
        if (err != null) {
            return ;
        }
        n++;
    }
    if ((len(template.DNSNames) > 0 || len(template.EmailAddresses) > 0 || len(template.IPAddresses) > 0 || len(template.URIs) > 0) && !oidInExtensions(oidExtensionSubjectAltName, template.ExtraExtensions)) {
        ret[n].Id = oidExtensionSubjectAltName; 
        // From RFC 5280, Section 4.2.1.6:
        // “If the subject field contains an empty sequence ... then
        // subjectAltName extension ... is marked as critical”
        ret[n].Critical = subjectIsEmpty;
        ret[n].Value, err = marshalSANs(template.DNSNames, template.EmailAddresses, template.IPAddresses, template.URIs);
        if (err != null) {
            return ;
        }
        n++;
    }
    if (len(template.PolicyIdentifiers) > 0 && !oidInExtensions(oidExtensionCertificatePolicies, template.ExtraExtensions)) {
        ret[n], err = marshalCertificatePolicies(template.PolicyIdentifiers);
        if (err != null) {
            return (null, error.As(err)!);
        }
        n++;
    }
    if ((len(template.PermittedDNSDomains) > 0 || len(template.ExcludedDNSDomains) > 0 || len(template.PermittedIPRanges) > 0 || len(template.ExcludedIPRanges) > 0 || len(template.PermittedEmailAddresses) > 0 || len(template.ExcludedEmailAddresses) > 0 || len(template.PermittedURIDomains) > 0 || len(template.ExcludedURIDomains) > 0) && !oidInExtensions(oidExtensionNameConstraints, template.ExtraExtensions)) {
        ret[n].Id = oidExtensionNameConstraints;
        ret[n].Critical = template.PermittedDNSDomainsCritical;

        ipAndMask = ipNet => {
            var maskedIP = ipNet.IP.Mask(ipNet.Mask);
            var ipAndMask = make_slice<byte>(0, len(maskedIP) + len(ipNet.Mask));
            ipAndMask = append(ipAndMask, maskedIP);
            ipAndMask = append(ipAndMask, ipNet.Mask);
            return ipAndMask;
        };

        Func<slice<@string>, slice<ptr<net.IPNet>>, slice<@string>, slice<@string>, (slice<byte>, error)> serialiseConstraints = (dns, ips, emails, uriDomains) => {
            cryptobyte.Builder b = default;

            {
                var name__prev1 = name;

                foreach (var (_, __name) in dns) {
                    name = __name;
                    err = isIA5String(name);

                    if (err != null) {
                        return (null, error.As(err)!);
                    }
                    b.AddASN1(cryptobyte_asn1.SEQUENCE, b => {
                        b.AddASN1(cryptobyte_asn1.Tag(2).ContextSpecific(), b => {
                            b.AddBytes((slice<byte>)name);
                        });
                    });
                }

                name = name__prev1;
            }

            foreach (var (_, ipNet) in ips) {
                b.AddASN1(cryptobyte_asn1.SEQUENCE, b => {
                    b.AddASN1(cryptobyte_asn1.Tag(7).ContextSpecific(), b => {
                        b.AddBytes(ipAndMask(ipNet));
                    });
                });
            }
            foreach (var (_, email) in emails) {
                err = isIA5String(email);

                if (err != null) {
                    return (null, error.As(err)!);
                }
                b.AddASN1(cryptobyte_asn1.SEQUENCE, b => {
                    b.AddASN1(cryptobyte_asn1.Tag(1).ContextSpecific(), b => {
                        b.AddBytes((slice<byte>)email);
                    });
                });
            }
            foreach (var (_, uriDomain) in uriDomains) {
                err = isIA5String(uriDomain);

                if (err != null) {
                    return (null, error.As(err)!);
                }
                b.AddASN1(cryptobyte_asn1.SEQUENCE, b => {
                    b.AddASN1(cryptobyte_asn1.Tag(6).ContextSpecific(), b => {
                        b.AddBytes((slice<byte>)uriDomain);
                    });
                });
            }
            return b.Bytes();
        };

        var (permitted, err) = serialiseConstraints(template.PermittedDNSDomains, template.PermittedIPRanges, template.PermittedEmailAddresses, template.PermittedURIDomains);
        if (err != null) {
            return (null, error.As(err)!);
        }
        var (excluded, err) = serialiseConstraints(template.ExcludedDNSDomains, template.ExcludedIPRanges, template.ExcludedEmailAddresses, template.ExcludedURIDomains);
        if (err != null) {
            return (null, error.As(err)!);
        }
        b = default;
        b.AddASN1(cryptobyte_asn1.SEQUENCE, b => {
            if (len(permitted) > 0) {
                b.AddASN1(cryptobyte_asn1.Tag(0).ContextSpecific().Constructed(), b => {
                    b.AddBytes(permitted);
                });
            }
            if (len(excluded) > 0) {
                b.AddASN1(cryptobyte_asn1.Tag(1).ContextSpecific().Constructed(), b => {
                    b.AddBytes(excluded);
                });
            }
        });

        ret[n].Value, err = b.Bytes();
        if (err != null) {
            return (null, error.As(err)!);
        }
        n++;
    }
    if (len(template.CRLDistributionPoints) > 0 && !oidInExtensions(oidExtensionCRLDistributionPoints, template.ExtraExtensions)) {
        ret[n].Id = oidExtensionCRLDistributionPoints;

        slice<distributionPoint> crlDp = default;
        {
            var name__prev1 = name;

            foreach (var (_, __name) in template.CRLDistributionPoints) {
                name = __name;
                distributionPoint dp = new distributionPoint(DistributionPoint:distributionPointName{FullName:[]asn1.RawValue{{Tag:6,Class:2,Bytes:[]byte(name)},},},);
                crlDp = append(crlDp, dp);
            }

            name = name__prev1;
        }

        ret[n].Value, err = asn1.Marshal(crlDp);
        if (err != null) {
            return ;
        }
        n++;
    }
    return (append(ret[..(int)n], template.ExtraExtensions), error.As(null!)!);
}

private static (pkix.Extension, error) marshalKeyUsage(KeyUsage ku) {
    pkix.Extension _p0 = default;
    error _p0 = default!;

    pkix.Extension ext = new pkix.Extension(Id:oidExtensionKeyUsage,Critical:true);

    array<byte> a = new array<byte>(2);
    a[0] = reverseBitsInAByte(byte(ku));
    a[1] = reverseBitsInAByte(byte(ku >> 8));

    nint l = 1;
    if (a[1] != 0) {
        l = 2;
    }
    var bitString = a[..(int)l];
    error err = default!;
    ext.Value, err = asn1.Marshal(new asn1.BitString(Bytes:bitString,BitLength:asn1BitLength(bitString)));
    if (err != null) {
        return (ext, error.As(err)!);
    }
    return (ext, error.As(null!)!);
}

private static (pkix.Extension, error) marshalExtKeyUsage(slice<ExtKeyUsage> extUsages, slice<asn1.ObjectIdentifier> unknownUsages) {
    pkix.Extension _p0 = default;
    error _p0 = default!;

    pkix.Extension ext = new pkix.Extension(Id:oidExtensionExtendedKeyUsage);

    var oids = make_slice<asn1.ObjectIdentifier>(len(extUsages) + len(unknownUsages));
    foreach (var (i, u) in extUsages) {
        {
            var (oid, ok) = oidFromExtKeyUsage(u);

            if (ok) {
                oids[i] = oid;
            }
            else
 {
                return (ext, error.As(errors.New("x509: unknown extended key usage"))!);
            }

        }
    }    copy(oids[(int)len(extUsages)..], unknownUsages);

    error err = default!;
    ext.Value, err = asn1.Marshal(oids);
    if (err != null) {
        return (ext, error.As(err)!);
    }
    return (ext, error.As(null!)!);
}

private static (pkix.Extension, error) marshalBasicConstraints(bool isCA, nint maxPathLen, bool maxPathLenZero) {
    pkix.Extension _p0 = default;
    error _p0 = default!;

    pkix.Extension ext = new pkix.Extension(Id:oidExtensionBasicConstraints,Critical:true); 
    // Leaving MaxPathLen as zero indicates that no maximum path
    // length is desired, unless MaxPathLenZero is set. A value of
    // -1 causes encoding/asn1 to omit the value as desired.
    if (maxPathLen == 0 && !maxPathLenZero) {
        maxPathLen = -1;
    }
    error err = default!;
    ext.Value, err = asn1.Marshal(new basicConstraints(isCA,maxPathLen));
    if (err != null) {
        return (ext, error.As(null!)!);
    }
    return (ext, error.As(null!)!);
}

private static (pkix.Extension, error) marshalCertificatePolicies(slice<asn1.ObjectIdentifier> policyIdentifiers) {
    pkix.Extension _p0 = default;
    error _p0 = default!;

    pkix.Extension ext = new pkix.Extension(Id:oidExtensionCertificatePolicies);
    var policies = make_slice<policyInformation>(len(policyIdentifiers));
    foreach (var (i, policy) in policyIdentifiers) {
        policies[i].Policy = policy;
    }    error err = default!;
    ext.Value, err = asn1.Marshal(policies);
    if (err != null) {
        return (ext, error.As(err)!);
    }
    return (ext, error.As(null!)!);
}

private static (slice<pkix.Extension>, error) buildCSRExtensions(ptr<CertificateRequest> _addr_template) {
    slice<pkix.Extension> _p0 = default;
    error _p0 = default!;
    ref CertificateRequest template = ref _addr_template.val;

    slice<pkix.Extension> ret = default;

    if ((len(template.DNSNames) > 0 || len(template.EmailAddresses) > 0 || len(template.IPAddresses) > 0 || len(template.URIs) > 0) && !oidInExtensions(oidExtensionSubjectAltName, template.ExtraExtensions)) {
        var (sanBytes, err) = marshalSANs(template.DNSNames, template.EmailAddresses, template.IPAddresses, template.URIs);
        if (err != null) {
            return (null, error.As(err)!);
        }
        ret = append(ret, new pkix.Extension(Id:oidExtensionSubjectAltName,Value:sanBytes,));
    }
    return (append(ret, template.ExtraExtensions), error.As(null!)!);
}

private static (slice<byte>, error) subjectBytes(ptr<Certificate> _addr_cert) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref Certificate cert = ref _addr_cert.val;

    if (len(cert.RawSubject) > 0) {
        return (cert.RawSubject, error.As(null!)!);
    }
    return asn1.Marshal(cert.Subject.ToRDNSequence());
}

// signingParamsForPublicKey returns the parameters to use for signing with
// priv. If requestedSigAlgo is not zero then it overrides the default
// signature algorithm.
private static (crypto.Hash, pkix.AlgorithmIdentifier, error) signingParamsForPublicKey(object pub, SignatureAlgorithm requestedSigAlgo) {
    crypto.Hash hashFunc = default;
    pkix.AlgorithmIdentifier sigAlgo = default;
    error err = default!;

    PublicKeyAlgorithm pubType = default;

    switch (pub.type()) {
        case ptr<rsa.PublicKey> pub:
            pubType = RSA;
            hashFunc = crypto.SHA256;
            sigAlgo.Algorithm = oidSignatureSHA256WithRSA;
            sigAlgo.Parameters = asn1.NullRawValue;
            break;
        case ptr<ecdsa.PublicKey> pub:
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
        case ed25519.PublicKey pub:
            pubType = Ed25519;
            sigAlgo.Algorithm = oidSignatureEd25519;
            break;
        default:
        {
            var pub = pub.type();
            err = errors.New("x509: only RSA, ECDSA and Ed25519 keys supported");
            break;
        }

    }

    if (err != null) {
        return ;
    }
    if (requestedSigAlgo == 0) {
        return ;
    }
    var found = false;
    foreach (var (_, details) in signatureAlgorithmDetails) {
        if (details.algo == requestedSigAlgo) {
            if (details.pubKeyAlgo != pubType) {
                err = errors.New("x509: requested SignatureAlgorithm does not match private key type");
                return ;
            }
            (sigAlgo.Algorithm, hashFunc) = (details.oid, details.hash);            if (hashFunc == 0 && pubType != Ed25519) {
                err = errors.New("x509: cannot sign with hash function requested");
                return ;
            }
            if (requestedSigAlgo.isRSAPSS()) {
                sigAlgo.Parameters = hashToPSSParameters[hashFunc];
            }
            found = true;
            break;
        }
    }    if (!found) {
        err = errors.New("x509: unknown SignatureAlgorithm");
    }
    return ;
}

// emptyASN1Subject is the ASN.1 DER encoding of an empty Subject, which is
// just an empty SEQUENCE.
private static byte emptyASN1Subject = new slice<byte>(new byte[] { 0x30, 0 });

// CreateCertificate creates a new X.509 v3 certificate based on a template.
// The following members of template are currently used:
//
//  - AuthorityKeyId
//  - BasicConstraintsValid
//  - CRLDistributionPoints
//  - DNSNames
//  - EmailAddresses
//  - ExcludedDNSDomains
//  - ExcludedEmailAddresses
//  - ExcludedIPRanges
//  - ExcludedURIDomains
//  - ExtKeyUsage
//  - ExtraExtensions
//  - IPAddresses
//  - IsCA
//  - IssuingCertificateURL
//  - KeyUsage
//  - MaxPathLen
//  - MaxPathLenZero
//  - NotAfter
//  - NotBefore
//  - OCSPServer
//  - PermittedDNSDomains
//  - PermittedDNSDomainsCritical
//  - PermittedEmailAddresses
//  - PermittedIPRanges
//  - PermittedURIDomains
//  - PolicyIdentifiers
//  - SerialNumber
//  - SignatureAlgorithm
//  - Subject
//  - SubjectKeyId
//  - URIs
//  - UnknownExtKeyUsage
//
// The certificate is signed by parent. If parent is equal to template then the
// certificate is self-signed. The parameter pub is the public key of the
// certificate to be generated and priv is the private key of the signer.
//
// The returned slice is the certificate in DER encoding.
//
// The currently supported key types are *rsa.PublicKey, *ecdsa.PublicKey and
// ed25519.PublicKey. pub must be a supported key type, and priv must be a
// crypto.Signer with a supported public key.
//
// The AuthorityKeyId will be taken from the SubjectKeyId of parent, if any,
// unless the resulting certificate is self-signed. Otherwise the value from
// template will be used.
//
// If SubjectKeyId from template is empty and the template is a CA, SubjectKeyId
// will be generated from the hash of the public key.
public static (slice<byte>, error) CreateCertificate(io.Reader rand, ptr<Certificate> _addr_template, ptr<Certificate> _addr_parent, object pub, object priv) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref Certificate template = ref _addr_template.val;
    ref Certificate parent = ref _addr_parent.val;

    crypto.Signer (key, ok) = priv._<crypto.Signer>();
    if (!ok) {
        return (null, error.As(errors.New("x509: certificate private key does not implement crypto.Signer"))!);
    }
    if (template.SerialNumber == null) {
        return (null, error.As(errors.New("x509: no SerialNumber given"))!);
    }
    if (template.BasicConstraintsValid && !template.IsCA && template.MaxPathLen != -1 && (template.MaxPathLen != 0 || template.MaxPathLenZero)) {
        return (null, error.As(errors.New("x509: only CAs are allowed to specify MaxPathLen"))!);
    }
    var (hashFunc, signatureAlgorithm, err) = signingParamsForPublicKey(key.Public(), template.SignatureAlgorithm);
    if (err != null) {
        return (null, error.As(err)!);
    }
    var (publicKeyBytes, publicKeyAlgorithm, err) = marshalPublicKey(pub);
    if (err != null) {
        return (null, error.As(err)!);
    }
    var (asn1Issuer, err) = subjectBytes(_addr_parent);
    if (err != null) {
        return (null, error.As(err)!);
    }
    var (asn1Subject, err) = subjectBytes(_addr_template);
    if (err != null) {
        return (null, error.As(err)!);
    }
    var authorityKeyId = template.AuthorityKeyId;
    if (!bytes.Equal(asn1Issuer, asn1Subject) && len(parent.SubjectKeyId) > 0) {
        authorityKeyId = parent.SubjectKeyId;
    }
    var subjectKeyId = template.SubjectKeyId;
    if (len(subjectKeyId) == 0 && template.IsCA) { 
        // SubjectKeyId generated using method 1 in RFC 5280, Section 4.2.1.2:
        //   (1) The keyIdentifier is composed of the 160-bit SHA-1 hash of the
        //   value of the BIT STRING subjectPublicKey (excluding the tag,
        //   length, and number of unused bits).
        var h = sha1.Sum(publicKeyBytes);
        subjectKeyId = h[..];
    }
    private partial interface privateKey {
        bool Equal(crypto.PublicKey _p0);
    }
    {
        privateKey (privPub, ok) = privateKey.As(key.Public()._<privateKey>())!;

        if (!ok) {
            return (null, error.As(errors.New("x509: internal error: supported public key does not implement Equal"))!);
        }
        else if (parent.PublicKey != null && !privPub.Equal(parent.PublicKey)) {
            return (null, error.As(errors.New("x509: provided PrivateKey doesn't match parent's PublicKey"))!);
        }

    }

    var (extensions, err) = buildCertExtensions(_addr_template, bytes.Equal(asn1Subject, emptyASN1Subject), authorityKeyId, subjectKeyId);
    if (err != null) {
        return (null, error.As(err)!);
    }
    asn1.BitString encodedPublicKey = new asn1.BitString(BitLength:len(publicKeyBytes)*8,Bytes:publicKeyBytes);
    tbsCertificate c = new tbsCertificate(Version:2,SerialNumber:template.SerialNumber,SignatureAlgorithm:signatureAlgorithm,Issuer:asn1.RawValue{FullBytes:asn1Issuer},Validity:validity{template.NotBefore.UTC(),template.NotAfter.UTC()},Subject:asn1.RawValue{FullBytes:asn1Subject},PublicKey:publicKeyInfo{nil,publicKeyAlgorithm,encodedPublicKey},Extensions:extensions,);

    var (tbsCertContents, err) = asn1.Marshal(c);
    if (err != null) {
        return (null, error.As(err)!);
    }
    c.Raw = tbsCertContents;

    var signed = tbsCertContents;
    if (hashFunc != 0) {
        h = hashFunc.New();
        h.Write(signed);
        signed = h.Sum(null);
    }
    crypto.SignerOpts signerOpts = hashFunc;
    if (template.SignatureAlgorithm != 0 && template.SignatureAlgorithm.isRSAPSS()) {
        signerOpts = addr(new rsa.PSSOptions(SaltLength:rsa.PSSSaltLengthEqualsHash,Hash:hashFunc,));
    }
    slice<byte> signature = default;
    signature, err = key.Sign(rand, signed, signerOpts);
    if (err != null) {
        return (null, error.As(err)!);
    }
    var (signedCert, err) = asn1.Marshal(new certificate(nil,c,signatureAlgorithm,asn1.BitString{Bytes:signature,BitLength:len(signature)*8},));
    if (err != null) {
        return (null, error.As(err)!);
    }
    {
        var sigAlg = getSignatureAlgorithmFromAI(signatureAlgorithm);

        if (sigAlg != MD5WithRSA) {
            {
                var err = checkSignature(sigAlg, c.Raw, signature, key.Public());

                if (err != null) {
                    return (null, error.As(fmt.Errorf("x509: signature over certificate returned by signer is invalid: %w", err))!);
                }

            }
        }
    }

    return (signedCert, error.As(null!)!);
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
public static (ptr<pkix.CertificateList>, error) ParseCRL(slice<byte> crlBytes) {
    ptr<pkix.CertificateList> _p0 = default!;
    error _p0 = default!;

    if (bytes.HasPrefix(crlBytes, pemCRLPrefix)) {
        var (block, _) = pem.Decode(crlBytes);
        if (block != null && block.Type == pemType) {
            crlBytes = block.Bytes;
        }
    }
    return _addr_ParseDERCRL(crlBytes)!;
}

// ParseDERCRL parses a DER encoded CRL from the given bytes.
public static (ptr<pkix.CertificateList>, error) ParseDERCRL(slice<byte> derBytes) {
    ptr<pkix.CertificateList> _p0 = default!;
    error _p0 = default!;

    ptr<pkix.CertificateList> certList = @new<pkix.CertificateList>();
    {
        var (rest, err) = asn1.Unmarshal(derBytes, certList);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        else if (len(rest) != 0) {
            return (_addr_null!, error.As(errors.New("x509: trailing data after CRL"))!);
        }

    }
    return (_addr_certList!, error.As(null!)!);
}

// CreateCRL returns a DER encoded CRL, signed by this Certificate, that
// contains the given list of revoked certificates.
//
// Note: this method does not generate an RFC 5280 conformant X.509 v2 CRL.
// To generate a standards compliant CRL, use CreateRevocationList instead.
private static (slice<byte>, error) CreateCRL(this ptr<Certificate> _addr_c, io.Reader rand, object priv, slice<pkix.RevokedCertificate> revokedCerts, time.Time now, time.Time expiry) {
    slice<byte> crlBytes = default;
    error err = default!;
    ref Certificate c = ref _addr_c.val;

    crypto.Signer (key, ok) = priv._<crypto.Signer>();
    if (!ok) {
        return (null, error.As(errors.New("x509: certificate private key does not implement crypto.Signer"))!);
    }
    var (hashFunc, signatureAlgorithm, err) = signingParamsForPublicKey(key.Public(), 0);
    if (err != null) {
        return (null, error.As(err)!);
    }
    var revokedCertsUTC = make_slice<pkix.RevokedCertificate>(len(revokedCerts));
    foreach (var (i, rc) in revokedCerts) {
        rc.RevocationTime = rc.RevocationTime.UTC();
        revokedCertsUTC[i] = rc;
    }    pkix.TBSCertificateList tbsCertList = new pkix.TBSCertificateList(Version:1,Signature:signatureAlgorithm,Issuer:c.Subject.ToRDNSequence(),ThisUpdate:now.UTC(),NextUpdate:expiry.UTC(),RevokedCertificates:revokedCertsUTC,); 

    // Authority Key Id
    if (len(c.SubjectKeyId) > 0) {
        pkix.Extension aki = default;
        aki.Id = oidExtensionAuthorityKeyId;
        aki.Value, err = asn1.Marshal(new authKeyId(Id:c.SubjectKeyId));
        if (err != null) {
            return ;
        }
        tbsCertList.Extensions = append(tbsCertList.Extensions, aki);
    }
    var (tbsCertListContents, err) = asn1.Marshal(tbsCertList);
    if (err != null) {
        return ;
    }
    var signed = tbsCertListContents;
    if (hashFunc != 0) {
        var h = hashFunc.New();
        h.Write(signed);
        signed = h.Sum(null);
    }
    slice<byte> signature = default;
    signature, err = key.Sign(rand, signed, hashFunc);
    if (err != null) {
        return ;
    }
    return asn1.Marshal(new pkix.CertificateList(TBSCertList:tbsCertList,SignatureAlgorithm:signatureAlgorithm,SignatureValue:asn1.BitString{Bytes:signature,BitLength:len(signature)*8},));
}

// CertificateRequest represents a PKCS #10, certificate signature request.
public partial struct CertificateRequest {
    public slice<byte> Raw; // Complete ASN.1 DER content (CSR, signature algorithm and signature).
    public slice<byte> RawTBSCertificateRequest; // Certificate request info part of raw ASN.1 DER content.
    public slice<byte> RawSubjectPublicKeyInfo; // DER encoded SubjectPublicKeyInfo.
    public slice<byte> RawSubject; // DER encoded Subject.

    public nint Version;
    public slice<byte> Signature;
    public SignatureAlgorithm SignatureAlgorithm;
    public PublicKeyAlgorithm PublicKeyAlgorithm;
    public pkix.Name Subject; // Attributes contains the CSR attributes that can parse as
// pkix.AttributeTypeAndValueSET.
//
// Deprecated: Use Extensions and ExtraExtensions instead for parsing and
// generating the requestedExtensions attribute.
    public slice<pkix.AttributeTypeAndValueSET> Attributes; // Extensions contains all requested extensions, in raw form. When parsing
// CSRs, this can be used to extract extensions that are not parsed by this
// package.
    public slice<pkix.Extension> Extensions; // ExtraExtensions contains extensions to be copied, raw, into any CSR
// marshaled by CreateCertificateRequest. Values override any extensions
// that would otherwise be produced based on the other fields but are
// overridden by any extensions specified in Attributes.
//
// The ExtraExtensions field is not populated by ParseCertificateRequest,
// see Extensions instead.
    public slice<pkix.Extension> ExtraExtensions; // Subject Alternate Name values.
    public slice<@string> DNSNames;
    public slice<@string> EmailAddresses;
    public slice<net.IP> IPAddresses;
    public slice<ptr<url.URL>> URIs;
}

// These structures reflect the ASN.1 structure of X.509 certificate
// signature requests (see RFC 2986):

private partial struct tbsCertificateRequest {
    public asn1.RawContent Raw;
    public nint Version;
    public asn1.RawValue Subject;
    public publicKeyInfo PublicKey;
    [Description("asn1:\"tag:0\"")]
    public slice<asn1.RawValue> RawAttributes;
}

private partial struct certificateRequest {
    public asn1.RawContent Raw;
    public tbsCertificateRequest TBSCSR;
    public pkix.AlgorithmIdentifier SignatureAlgorithm;
    public asn1.BitString SignatureValue;
}

// oidExtensionRequest is a PKCS #9 OBJECT IDENTIFIER that indicates requested
// extensions in a CSR.
private static asn1.ObjectIdentifier oidExtensionRequest = new asn1.ObjectIdentifier(1,2,840,113549,1,9,14);

// newRawAttributes converts AttributeTypeAndValueSETs from a template
// CertificateRequest's Attributes into tbsCertificateRequest RawAttributes.
private static (slice<asn1.RawValue>, error) newRawAttributes(slice<pkix.AttributeTypeAndValueSET> attributes) {
    slice<asn1.RawValue> _p0 = default;
    error _p0 = default!;

    ref slice<asn1.RawValue> rawAttributes = ref heap(out ptr<slice<asn1.RawValue>> _addr_rawAttributes);
    var (b, err) = asn1.Marshal(attributes);
    if (err != null) {
        return (null, error.As(err)!);
    }
    var (rest, err) = asn1.Unmarshal(b, _addr_rawAttributes);
    if (err != null) {
        return (null, error.As(err)!);
    }
    if (len(rest) != 0) {
        return (null, error.As(errors.New("x509: failed to unmarshal raw CSR Attributes"))!);
    }
    return (rawAttributes, error.As(null!)!);
}

// parseRawAttributes Unmarshals RawAttributes into AttributeTypeAndValueSETs.
private static slice<pkix.AttributeTypeAndValueSET> parseRawAttributes(slice<asn1.RawValue> rawAttributes) {
    slice<pkix.AttributeTypeAndValueSET> attributes = default;
    foreach (var (_, rawAttr) in rawAttributes) {
        ref pkix.AttributeTypeAndValueSET attr = ref heap(out ptr<pkix.AttributeTypeAndValueSET> _addr_attr);
        var (rest, err) = asn1.Unmarshal(rawAttr.FullBytes, _addr_attr); 
        // Ignore attributes that don't parse into pkix.AttributeTypeAndValueSET
        // (i.e.: challengePassword or unstructuredName).
        if (err == null && len(rest) == 0) {
            attributes = append(attributes, attr);
        }
    }    return attributes;
}

// parseCSRExtensions parses the attributes from a CSR and extracts any
// requested extensions.
private static (slice<pkix.Extension>, error) parseCSRExtensions(slice<asn1.RawValue> rawAttributes) {
    slice<pkix.Extension> _p0 = default;
    error _p0 = default!;
 
    // pkcs10Attribute reflects the Attribute structure from RFC 2986, Section 4.1.
    private partial struct pkcs10Attribute {
        public asn1.ObjectIdentifier Id;
        [Description("asn1:\"set\"")]
        public slice<asn1.RawValue> Values;
    }

    slice<pkix.Extension> ret = default;
    foreach (var (_, rawAttr) in rawAttributes) {
        ref pkcs10Attribute attr = ref heap(out ptr<pkcs10Attribute> _addr_attr);
        {
            var (rest, err) = asn1.Unmarshal(rawAttr.FullBytes, _addr_attr);

            if (err != null || len(rest) != 0 || len(attr.Values) == 0) { 
                // Ignore attributes that don't parse.
                continue;
            }

        }

        if (!attr.Id.Equal(oidExtensionRequest)) {
            continue;
        }
        ref slice<pkix.Extension> extensions = ref heap(out ptr<slice<pkix.Extension>> _addr_extensions);
        {
            var (_, err) = asn1.Unmarshal(attr.Values[0].FullBytes, _addr_extensions);

            if (err != null) {
                return (null, error.As(err)!);
            }

        }
        ret = append(ret, extensions);
    }    return (ret, error.As(null!)!);
}

// CreateCertificateRequest creates a new certificate request based on a
// template. The following members of template are used:
//
//  - SignatureAlgorithm
//  - Subject
//  - DNSNames
//  - EmailAddresses
//  - IPAddresses
//  - URIs
//  - ExtraExtensions
//  - Attributes (deprecated)
//
// priv is the private key to sign the CSR with, and the corresponding public
// key will be included in the CSR. It must implement crypto.Signer and its
// Public() method must return a *rsa.PublicKey or a *ecdsa.PublicKey or a
// ed25519.PublicKey. (A *rsa.PrivateKey, *ecdsa.PrivateKey or
// ed25519.PrivateKey satisfies this.)
//
// The returned slice is the certificate request in DER encoding.
public static (slice<byte>, error) CreateCertificateRequest(io.Reader rand, ptr<CertificateRequest> _addr_template, object priv) {
    slice<byte> csr = default;
    error err = default!;
    ref CertificateRequest template = ref _addr_template.val;

    crypto.Signer (key, ok) = priv._<crypto.Signer>();
    if (!ok) {
        return (null, error.As(errors.New("x509: certificate private key does not implement crypto.Signer"))!);
    }
    crypto.Hash hashFunc = default;
    pkix.AlgorithmIdentifier sigAlgo = default;
    hashFunc, sigAlgo, err = signingParamsForPublicKey(key.Public(), template.SignatureAlgorithm);
    if (err != null) {
        return (null, error.As(err)!);
    }
    slice<byte> publicKeyBytes = default;
    pkix.AlgorithmIdentifier publicKeyAlgorithm = default;
    publicKeyBytes, publicKeyAlgorithm, err = marshalPublicKey(key.Public());
    if (err != null) {
        return (null, error.As(err)!);
    }
    var (extensions, err) = buildCSRExtensions(_addr_template);
    if (err != null) {
        return (null, error.As(err)!);
    }
    var attributes = make_slice<pkix.AttributeTypeAndValueSET>(0, len(template.Attributes));
    {
        var attr__prev1 = attr;

        foreach (var (_, __attr) in template.Attributes) {
            attr = __attr;
            var values = make_slice<slice<pkix.AttributeTypeAndValue>>(len(attr.Value));
            copy(values, attr.Value);
            attributes = append(attributes, new pkix.AttributeTypeAndValueSET(Type:attr.Type,Value:values,));
        }
        attr = attr__prev1;
    }

    var extensionsAppended = false;
    if (len(extensions) > 0) { 
        // Append the extensions to an existing attribute if possible.
        foreach (var (_, atvSet) in attributes) {
            if (!atvSet.Type.Equal(oidExtensionRequest) || len(atvSet.Value) == 0) {
                continue;
            } 

            // specifiedExtensions contains all the extensions that we
            // found specified via template.Attributes.
            var specifiedExtensions = make_map<@string, bool>();

            foreach (var (_, atvs) in atvSet.Value) {
                foreach (var (_, atv) in atvs) {
                    specifiedExtensions[atv.Type.String()] = true;
                }
            }
            var newValue = make_slice<pkix.AttributeTypeAndValue>(0, len(atvSet.Value[0]) + len(extensions));
            newValue = append(newValue, atvSet.Value[0]);

            foreach (var (_, e) in extensions) {
                if (specifiedExtensions[e.Id.String()]) { 
                    // Attributes already contained a value for
                    // this extension and it takes priority.
                    continue;
                }
                newValue = append(newValue, new pkix.AttributeTypeAndValue(Type:e.Id,Value:e.Value,));
            }
            atvSet.Value[0] = newValue;
            extensionsAppended = true;
            break;
        }
    }
    var (rawAttributes, err) = newRawAttributes(attributes);
    if (err != null) {
        return ;
    }
    if (len(extensions) > 0 && !extensionsAppended) {
        struct{Typeasn1.ObjectIdentifierValue[][]pkix.Extension`asn1:"set"`} attr = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{Typeasn1.ObjectIdentifierValue[][]pkix.Extension`asn1:"set"`}{Type:oidExtensionRequest,Value:[][]pkix.Extension{extensions},};

        var (b, err) = asn1.Marshal(attr);
        if (err != null) {
            return (null, error.As(errors.New("x509: failed to serialise extensions attribute: " + err.Error()))!);
        }
        ref asn1.RawValue rawValue = ref heap(out ptr<asn1.RawValue> _addr_rawValue);
        {
            var (_, err) = asn1.Unmarshal(b, _addr_rawValue);

            if (err != null) {
                return (null, error.As(err)!);
            }

        }

        rawAttributes = append(rawAttributes, rawValue);
    }
    var asn1Subject = template.RawSubject;
    if (len(asn1Subject) == 0) {
        asn1Subject, err = asn1.Marshal(template.Subject.ToRDNSequence());
        if (err != null) {
            return (null, error.As(err)!);
        }
    }
    tbsCertificateRequest tbsCSR = new tbsCertificateRequest(Version:0,Subject:asn1.RawValue{FullBytes:asn1Subject},PublicKey:publicKeyInfo{Algorithm:publicKeyAlgorithm,PublicKey:asn1.BitString{Bytes:publicKeyBytes,BitLength:len(publicKeyBytes)*8,},},RawAttributes:rawAttributes,);

    var (tbsCSRContents, err) = asn1.Marshal(tbsCSR);
    if (err != null) {
        return ;
    }
    tbsCSR.Raw = tbsCSRContents;

    var signed = tbsCSRContents;
    if (hashFunc != 0) {
        var h = hashFunc.New();
        h.Write(signed);
        signed = h.Sum(null);
    }
    slice<byte> signature = default;
    signature, err = key.Sign(rand, signed, hashFunc);
    if (err != null) {
        return ;
    }
    return asn1.Marshal(new certificateRequest(TBSCSR:tbsCSR,SignatureAlgorithm:sigAlgo,SignatureValue:asn1.BitString{Bytes:signature,BitLength:len(signature)*8,},));
}

// ParseCertificateRequest parses a single certificate request from the
// given ASN.1 DER data.
public static (ptr<CertificateRequest>, error) ParseCertificateRequest(slice<byte> asn1Data) {
    ptr<CertificateRequest> _p0 = default!;
    error _p0 = default!;

    ref certificateRequest csr = ref heap(out ptr<certificateRequest> _addr_csr);

    var (rest, err) = asn1.Unmarshal(asn1Data, _addr_csr);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    else if (len(rest) != 0) {
        return (_addr_null!, error.As(new asn1.SyntaxError(Msg:"trailing data"))!);
    }
    return _addr_parseCertificateRequest(_addr_csr)!;
}

private static (ptr<CertificateRequest>, error) parseCertificateRequest(ptr<certificateRequest> _addr_@in) {
    ptr<CertificateRequest> _p0 = default!;
    error _p0 = default!;
    ref certificateRequest @in = ref _addr_@in.val;

    ptr<CertificateRequest> @out = addr(new CertificateRequest(Raw:in.Raw,RawTBSCertificateRequest:in.TBSCSR.Raw,RawSubjectPublicKeyInfo:in.TBSCSR.PublicKey.Raw,RawSubject:in.TBSCSR.Subject.FullBytes,Signature:in.SignatureValue.RightAlign(),SignatureAlgorithm:getSignatureAlgorithmFromAI(in.SignatureAlgorithm),PublicKeyAlgorithm:getPublicKeyAlgorithmFromOID(in.TBSCSR.PublicKey.Algorithm.Algorithm),Version:in.TBSCSR.Version,Attributes:parseRawAttributes(in.TBSCSR.RawAttributes),));

    error err = default!;
    @out.PublicKey, err = parsePublicKey(@out.PublicKeyAlgorithm, _addr_@in.TBSCSR.PublicKey);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    ref pkix.RDNSequence subject = ref heap(out ptr<pkix.RDNSequence> _addr_subject);
    {
        var (rest, err) = asn1.Unmarshal(@in.TBSCSR.Subject.FullBytes, _addr_subject);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        else if (len(rest) != 0) {
            return (_addr_null!, error.As(errors.New("x509: trailing data after X.509 Subject"))!);
        }

    }

    @out.Subject.FillFromRDNSequence(_addr_subject);

    @out.Extensions, err = parseCSRExtensions(@in.TBSCSR.RawAttributes);

    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    foreach (var (_, extension) in @out.Extensions) {

        if (extension.Id.Equal(oidExtensionSubjectAltName)) 
            @out.DNSNames, @out.EmailAddresses, @out.IPAddresses, @out.URIs, err = parseSANExtension(extension.Value);
            if (err != null) {
                return (_addr_null!, error.As(err)!);
            }
            }    return (_addr_out!, error.As(null!)!);
}

// CheckSignature reports whether the signature on c is valid.
private static error CheckSignature(this ptr<CertificateRequest> _addr_c) {
    ref CertificateRequest c = ref _addr_c.val;

    return error.As(checkSignature(c.SignatureAlgorithm, c.RawTBSCertificateRequest, c.Signature, c.PublicKey))!;
}

// RevocationList contains the fields used to create an X.509 v2 Certificate
// Revocation list with CreateRevocationList.
public partial struct RevocationList {
    public SignatureAlgorithm SignatureAlgorithm; // RevokedCertificates is used to populate the revokedCertificates
// sequence in the CRL, it may be empty. RevokedCertificates may be nil,
// in which case an empty CRL will be created.
    public slice<pkix.RevokedCertificate> RevokedCertificates; // Number is used to populate the X.509 v2 cRLNumber extension in the CRL,
// which should be a monotonically increasing sequence number for a given
// CRL scope and CRL issuer.
    public ptr<big.Int> Number; // ThisUpdate is used to populate the thisUpdate field in the CRL, which
// indicates the issuance date of the CRL.
    public time.Time ThisUpdate; // NextUpdate is used to populate the nextUpdate field in the CRL, which
// indicates the date by which the next CRL will be issued. NextUpdate
// must be greater than ThisUpdate.
    public time.Time NextUpdate; // ExtraExtensions contains any additional extensions to add directly to
// the CRL.
    public slice<pkix.Extension> ExtraExtensions;
}

// CreateRevocationList creates a new X.509 v2 Certificate Revocation List,
// according to RFC 5280, based on template.
//
// The CRL is signed by priv which should be the private key associated with
// the public key in the issuer certificate.
//
// The issuer may not be nil, and the crlSign bit must be set in KeyUsage in
// order to use it as a CRL issuer.
//
// The issuer distinguished name CRL field and authority key identifier
// extension are populated using the issuer certificate. issuer must have
// SubjectKeyId set.
public static (slice<byte>, error) CreateRevocationList(io.Reader rand, ptr<RevocationList> _addr_template, ptr<Certificate> _addr_issuer, crypto.Signer priv) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref RevocationList template = ref _addr_template.val;
    ref Certificate issuer = ref _addr_issuer.val;

    if (template == null) {
        return (null, error.As(errors.New("x509: template can not be nil"))!);
    }
    if (issuer == null) {
        return (null, error.As(errors.New("x509: issuer can not be nil"))!);
    }
    if ((issuer.KeyUsage & KeyUsageCRLSign) == 0) {
        return (null, error.As(errors.New("x509: issuer must have the crlSign key usage bit set"))!);
    }
    if (len(issuer.SubjectKeyId) == 0) {
        return (null, error.As(errors.New("x509: issuer certificate doesn't contain a subject key identifier"))!);
    }
    if (template.NextUpdate.Before(template.ThisUpdate)) {
        return (null, error.As(errors.New("x509: template.ThisUpdate is after template.NextUpdate"))!);
    }
    if (template.Number == null) {
        return (null, error.As(errors.New("x509: template contains nil Number field"))!);
    }
    var (hashFunc, signatureAlgorithm, err) = signingParamsForPublicKey(priv.Public(), template.SignatureAlgorithm);
    if (err != null) {
        return (null, error.As(err)!);
    }
    var revokedCertsUTC = make_slice<pkix.RevokedCertificate>(len(template.RevokedCertificates));
    foreach (var (i, rc) in template.RevokedCertificates) {
        rc.RevocationTime = rc.RevocationTime.UTC();
        revokedCertsUTC[i] = rc;
    }    var (aki, err) = asn1.Marshal(new authKeyId(Id:issuer.SubjectKeyId));
    if (err != null) {
        return (null, error.As(err)!);
    }
    var (crlNum, err) = asn1.Marshal(template.Number);
    if (err != null) {
        return (null, error.As(err)!);
    }
    pkix.TBSCertificateList tbsCertList = new pkix.TBSCertificateList(Version:1,Signature:signatureAlgorithm,Issuer:issuer.Subject.ToRDNSequence(),ThisUpdate:template.ThisUpdate.UTC(),NextUpdate:template.NextUpdate.UTC(),Extensions:[]pkix.Extension{{Id:oidExtensionAuthorityKeyId,Value:aki,},{Id:oidExtensionCRLNumber,Value:crlNum,},},);
    if (len(revokedCertsUTC) > 0) {
        tbsCertList.RevokedCertificates = revokedCertsUTC;
    }
    if (len(template.ExtraExtensions) > 0) {
        tbsCertList.Extensions = append(tbsCertList.Extensions, template.ExtraExtensions);
    }
    var (tbsCertListContents, err) = asn1.Marshal(tbsCertList);
    if (err != null) {
        return (null, error.As(err)!);
    }
    var input = tbsCertListContents;
    if (hashFunc != 0) {
        var h = hashFunc.New();
        h.Write(tbsCertListContents);
        input = h.Sum(null);
    }
    crypto.SignerOpts signerOpts = hashFunc;
    if (template.SignatureAlgorithm.isRSAPSS()) {
        signerOpts = addr(new rsa.PSSOptions(SaltLength:rsa.PSSSaltLengthEqualsHash,Hash:hashFunc,));
    }
    var (signature, err) = priv.Sign(rand, input, signerOpts);
    if (err != null) {
        return (null, error.As(err)!);
    }
    return asn1.Marshal(new pkix.CertificateList(TBSCertList:tbsCertList,SignatureAlgorithm:signatureAlgorithm,SignatureValue:asn1.BitString{Bytes:signature,BitLength:len(signature)*8},));
}

} // end x509_package
