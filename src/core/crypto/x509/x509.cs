// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package x509 implements a subset of the X.509 standard.
//
// It allows parsing and generating certificates, certificate signing
// requests, certificate revocation lists, and encoded public and private keys.
// It provides a certificate verifier, complete with a chain builder.
//
// The package targets the X.509 technical profile defined by the IETF (RFC
// 2459/3280/5280), and as further restricted by the CA/Browser Forum Baseline
// Requirements. There is minimal support for features outside of these
// profiles, as the primary goal of the package is to provide compatibility
// with the publicly trusted TLS certificate ecosystem and its policies and
// constraints.
//
// On macOS and Windows, certificate verification is handled by system APIs, but
// the package aims to apply consistent validation rules across operating
// systems.
namespace go.crypto;

using bytes = bytes_package;
using crypto = crypto_package;
using ecdh = crypto.ecdh_package;
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
using godebug = @internal.godebug_package;
using io = io_package;
using big = math.big_package;
using net = net_package;
using url = net.url_package;
using strconv = strconv_package;
using time = time_package;
using unicode = unicode_package;
using _ = crypto.sha1_package;
using _ = crypto.sha256_package;
using _ = crypto.sha512_package;
using cryptobyte = golang.org.x.crypto.cryptobyte_package;
using cryptobyte_asn1 = golang.org.x.crypto.cryptobyte.asn1_package;
using @internal;
using crypto.x509;
using encoding;
using golang.org.x.crypto;
using math;
using net;

partial class x509_package {

// pkixPublicKey reflects a PKIX public key structure. See SubjectPublicKeyInfo
// in RFC 3280.
[GoType] partial struct pkixPublicKey {
    public crypto.x509.pkix_package.AlgorithmIdentifier Algo;
    public encoding.asn1_package.BitString BitString;
}

// ParsePKIXPublicKey parses a public key in PKIX, ASN.1 DER form. The encoded
// public key is a SubjectPublicKeyInfo structure (see RFC 5280, Section 4.1).
//
// It returns a *[rsa.PublicKey], *[dsa.PublicKey], *[ecdsa.PublicKey],
// [ed25519.PublicKey] (not a pointer), or *[ecdh.PublicKey] (for X25519).
// More types might be supported in the future.
//
// This kind of key is commonly encoded in PEM blocks of type "PUBLIC KEY".
public static (any pub, error err) ParsePKIXPublicKey(slice<byte> derBytes) {
    any pub = default!;
    error err = default!;

    ref var pki = ref heap(new publicKeyInfo(), out var Ꮡpki);
    {
        (rest, errΔ1) = asn1.Unmarshal(derBytes, Ꮡpki); if (errΔ1 != default!){
            {
                (_, errΔ2) = asn1.Unmarshal(derBytes, Ꮡ(new pkcs1PublicKey(nil))); if (errΔ2 == default!) {
                    return (default!, errors.New("x509: failed to parse public key (use ParsePKCS1PublicKey instead for this key format)"u8));
                }
            }
            return (default!, errΔ1);
        } else 
        if (len(rest) != 0) {
            return (default!, errors.New("x509: trailing data after ASN.1 of public-key"u8));
        }
    }
    return parsePublicKey(Ꮡpki);
}

internal static (slice<byte> publicKeyBytes, pkix.AlgorithmIdentifier publicKeyAlgorithm, error err) marshalPublicKey(any pub) {
    slice<byte> publicKeyBytes = default!;
    pkix.AlgorithmIdentifier publicKeyAlgorithm = default!;
    error err = default!;

    switch (pub.type()) {
    case ж<rsa.PublicKey> pub: {
        (publicKeyBytes, err) = asn1.Marshal(new pkcs1PublicKey(
            N: (~pub).N,
            E: (~pub).E
        ));
        if (err != default!) {
            return (default!, new pkix.AlgorithmIdentifier(nil), err);
        }
        publicKeyAlgorithm.Algorithm = oidPublicKeyRSA;
        publicKeyAlgorithm.Parameters = asn1.NullRawValue;
        break;
    }
    case ж<ecdsa.PublicKey> pub: {
        var (oid, ok) = oidFromNamedCurve((~pub).Curve);
        if (!ok) {
            // This is a NULL parameters value which is required by
            // RFC 3279, Section 2.3.1.
            return (default!, new pkix.AlgorithmIdentifier(nil), errors.New("x509: unsupported elliptic curve"u8));
        }
        if (!(~pub).Curve.IsOnCurve((~pub).X, (~pub).Y)) {
            return (default!, new pkix.AlgorithmIdentifier(nil), errors.New("x509: invalid elliptic curve public key"u8));
        }
        publicKeyBytes = elliptic.Marshal((~pub).Curve, (~pub).X, (~pub).Y);
        publicKeyAlgorithm.Algorithm = oidPublicKeyECDSA;
        slice<byte> paramBytesΔ1 = default!;
        (, err) = asn1.Marshal(oid);
        if (err != default!) {
            return (publicKeyBytes, publicKeyAlgorithm, err);
        }
        publicKeyAlgorithm.Parameters.FullBytes = paramBytesΔ1;
        break;
    }
    case ed25519.PublicKey pub: {
        publicKeyBytes = pub;
        publicKeyAlgorithm.Algorithm = oidPublicKeyEd25519;
        break;
    }
    case ж<ecdhꓸPublicKey> pub: {
        publicKeyBytes = pub.Bytes();
        if (AreEqual(pub.Curve(), ecdh.X25519())){
            publicKeyAlgorithm.Algorithm = oidPublicKeyX25519;
        } else {
            var (oidΔ1, okΔ1) = oidFromECDHCurve(pub.Curve());
            if (!okΔ1) {
                return (default!, new pkix.AlgorithmIdentifier(nil), errors.New("x509: unsupported elliptic curve"u8));
            }
            publicKeyAlgorithm.Algorithm = oidPublicKeyECDSA;
            slice<byte> paramBytes = default!;
            (paramBytes, err) = asn1.Marshal(oidΔ1);
            if (err != default!) {
                return (publicKeyBytes, publicKeyAlgorithm, err);
            }
            publicKeyAlgorithm.Parameters.FullBytes = paramBytes;
        }
        break;
    }
    default: {
        var pub = pub.type();
        return (default!, new pkix.AlgorithmIdentifier(nil), fmt.Errorf("x509: unsupported public key type: %T"u8, pub));
    }}
    return (publicKeyBytes, publicKeyAlgorithm, default!);
}

// MarshalPKIXPublicKey converts a public key to PKIX, ASN.1 DER form.
// The encoded public key is a SubjectPublicKeyInfo structure
// (see RFC 5280, Section 4.1).
//
// The following key types are currently supported: *[rsa.PublicKey],
// *[ecdsa.PublicKey], [ed25519.PublicKey] (not a pointer), and *[ecdh.PublicKey].
// Unsupported key types result in an error.
//
// This kind of key is commonly encoded in PEM blocks of type "PUBLIC KEY".
public static (slice<byte>, error) MarshalPKIXPublicKey(any pub) {
    slice<byte> publicKeyBytes = default!;
    pkix.AlgorithmIdentifier publicKeyAlgorithm = default!;
    error err = default!;
    {
        (publicKeyBytes, publicKeyAlgorithm, err) = marshalPublicKey(pub); if (err != default!) {
            return (default!, err);
        }
    }
    var pkix = new pkixPublicKey(
        Algo: publicKeyAlgorithm,
        BitString: new asn1.BitString(
            Bytes: publicKeyBytes,
            BitLength: 8 * len(publicKeyBytes)
        )
    );
    (ret, _) = asn1.Marshal(pkix);
    return (ret, default!);
}

// These structures reflect the ASN.1 structure of X.509 certificates.:
[GoType] partial struct certificate {
    public tbsCertificate TBSCertificate;
    public crypto.x509.pkix_package.AlgorithmIdentifier SignatureAlgorithm;
    public encoding.asn1_package.BitString SignatureValue;
}

[GoType] partial struct tbsCertificate {
    public encoding.asn1_package.RawContent Raw;
    [GoTag(@"asn1:""optional,explicit,default:0,tag:0""")]
    public nint Version;
    public ж<math.big_package.ΔInt> SerialNumber;
    public crypto.x509.pkix_package.AlgorithmIdentifier SignatureAlgorithm;
    public encoding.asn1_package.RawValue Issuer;
    public validity Validity;
    public encoding.asn1_package.RawValue Subject;
    public publicKeyInfo PublicKey;
    [GoTag(@"asn1:""optional,tag:1""")]
    public encoding.asn1_package.BitString UniqueId;
    [GoTag(@"asn1:""optional,tag:2""")]
    public encoding.asn1_package.BitString SubjectUniqueId;
    [GoTag(@"asn1:""omitempty,optional,explicit,tag:3""")]
    public pkix.Extension Extensions;
}

[GoType] partial struct dsaAlgorithmParameters {
    public ж<math.big_package.ΔInt> P;
    public ж<math.big_package.ΔInt> Q;
    public ж<math.big_package.ΔInt> G;
}

[GoType] partial struct validity {
    public time_package.Time NotBefore;
    public time_package.Time NotAfter;
}

[GoType] partial struct publicKeyInfo {
    public encoding.asn1_package.RawContent Raw;
    public crypto.x509.pkix_package.AlgorithmIdentifier Algorithm;
    public encoding.asn1_package.BitString PublicKey;
}

// RFC 5280,  4.2.1.1
[GoType] partial struct authKeyId {
    [GoTag(@"asn1:""optional,tag:0""")]
    public slice<byte> Id;
}

[GoType("num:nint")] partial struct SignatureAlgorithm;

public static readonly SignatureAlgorithm UnknownSignatureAlgorithm = /* iota */ 0;
public static readonly SignatureAlgorithm MD2WithRSA = 1; // Unsupported.
public static readonly SignatureAlgorithm MD5WithRSA = 2; // Only supported for signing, not verification.
public static readonly SignatureAlgorithm SHA1WithRSA = 3; // Only supported for signing, and verification of CRLs, CSRs, and OCSP responses.
public static readonly SignatureAlgorithm SHA256WithRSA = 4;
public static readonly SignatureAlgorithm SHA384WithRSA = 5;
public static readonly SignatureAlgorithm SHA512WithRSA = 6;
public static readonly SignatureAlgorithm DSAWithSHA1 = 7; // Unsupported.
public static readonly SignatureAlgorithm DSAWithSHA256 = 8; // Unsupported.
public static readonly SignatureAlgorithm ECDSAWithSHA1 = 9; // Only supported for signing, and verification of CRLs, CSRs, and OCSP responses.
public static readonly SignatureAlgorithm ECDSAWithSHA256 = 10;
public static readonly SignatureAlgorithm ECDSAWithSHA384 = 11;
public static readonly SignatureAlgorithm ECDSAWithSHA512 = 12;
public static readonly SignatureAlgorithm SHA256WithRSAPSS = 13;
public static readonly SignatureAlgorithm SHA384WithRSAPSS = 14;
public static readonly SignatureAlgorithm SHA512WithRSAPSS = 15;
public static readonly SignatureAlgorithm PureEd25519 = 16;

internal static bool isRSAPSS(this SignatureAlgorithm algo) {
    foreach (var (_, details) in signatureAlgorithmDetails) {
        if (details.algo == algo) {
            return details.isRSAPSS;
        }
    }
    return false;
}

internal static crypto.Hash hashFunc(this SignatureAlgorithm algo) {
    foreach (var (_, details) in signatureAlgorithmDetails) {
        if (details.algo == algo) {
            return details.hash;
        }
    }
    return ((crypto.Hash)0);
}

public static @string String(this SignatureAlgorithm algo) {
    foreach (var (_, details) in signatureAlgorithmDetails) {
        if (details.algo == algo) {
            return details.name;
        }
    }
    return strconv.Itoa(((nint)algo));
}

[GoType("num:nint")] partial struct PublicKeyAlgorithm;

public static readonly PublicKeyAlgorithm UnknownPublicKeyAlgorithm = /* iota */ 0;
public static readonly PublicKeyAlgorithm RSA = 1;
public static readonly PublicKeyAlgorithm DSA = 2; // Only supported for parsing.
public static readonly PublicKeyAlgorithm ECDSA = 3;
public static readonly PublicKeyAlgorithm Ed25519 = 4;

internal static array<@string> publicKeyAlgoName = new runtime.SparseArray<@string>{
    [RSA] = "RSA"u8,
    [DSA] = "DSA"u8,
    [ECDSA] = "ECDSA"u8,
    [Ed25519] = "Ed25519"u8
}.array();

public static @string String(this PublicKeyAlgorithm algo) {
    if (0 < algo && ((nint)algo) < len(publicKeyAlgoName)) {
        return publicKeyAlgoName[algo];
    }
    return strconv.Itoa(((nint)algo));
}

// OIDs for signature algorithms
//
//	pkcs-1 OBJECT IDENTIFIER ::= {
//		iso(1) member-body(2) us(840) rsadsi(113549) pkcs(1) 1 }
//
// RFC 3279 2.2.1 RSA Signature Algorithms
//
//	md5WithRSAEncryption OBJECT IDENTIFIER ::= { pkcs-1 4 }
//
//	sha-1WithRSAEncryption OBJECT IDENTIFIER ::= { pkcs-1 5 }
//
//	dsaWithSha1 OBJECT IDENTIFIER ::= {
//		iso(1) member-body(2) us(840) x9-57(10040) x9cm(4) 3 }
//
// RFC 3279 2.2.3 ECDSA Signature Algorithm
//
//	ecdsa-with-SHA1 OBJECT IDENTIFIER ::= {
//		iso(1) member-body(2) us(840) ansi-x962(10045)
//		signatures(4) ecdsa-with-SHA1(1)}
//
// RFC 4055 5 PKCS #1 Version 1.5
//
//	sha256WithRSAEncryption OBJECT IDENTIFIER ::= { pkcs-1 11 }
//
//	sha384WithRSAEncryption OBJECT IDENTIFIER ::= { pkcs-1 12 }
//
//	sha512WithRSAEncryption OBJECT IDENTIFIER ::= { pkcs-1 13 }
//
// RFC 5758 3.1 DSA Signature Algorithms
//
//	dsaWithSha256 OBJECT IDENTIFIER ::= {
//		joint-iso-ccitt(2) country(16) us(840) organization(1) gov(101)
//		csor(3) algorithms(4) id-dsa-with-sha2(3) 2}
//
// RFC 5758 3.2 ECDSA Signature Algorithm
//
//	ecdsa-with-SHA256 OBJECT IDENTIFIER ::= { iso(1) member-body(2)
//		us(840) ansi-X9-62(10045) signatures(4) ecdsa-with-SHA2(3) 2 }
//
//	ecdsa-with-SHA384 OBJECT IDENTIFIER ::= { iso(1) member-body(2)
//		us(840) ansi-X9-62(10045) signatures(4) ecdsa-with-SHA2(3) 3 }
//
//	ecdsa-with-SHA512 OBJECT IDENTIFIER ::= { iso(1) member-body(2)
//		us(840) ansi-X9-62(10045) signatures(4) ecdsa-with-SHA2(3) 4 }
//
// RFC 8410 3 Curve25519 and Curve448 Algorithm Identifiers
//
//	id-Ed25519   OBJECT IDENTIFIER ::= { 1 3 101 112 }
internal static asn1.ObjectIdentifier oidSignatureMD5WithRSA = new asn1.ObjectIdentifier{1, 2, 840, 113549, 1, 1, 4};

internal static asn1.ObjectIdentifier oidSignatureSHA1WithRSA = new asn1.ObjectIdentifier{1, 2, 840, 113549, 1, 1, 5};

internal static asn1.ObjectIdentifier oidSignatureSHA256WithRSA = new asn1.ObjectIdentifier{1, 2, 840, 113549, 1, 1, 11};

internal static asn1.ObjectIdentifier oidSignatureSHA384WithRSA = new asn1.ObjectIdentifier{1, 2, 840, 113549, 1, 1, 12};

internal static asn1.ObjectIdentifier oidSignatureSHA512WithRSA = new asn1.ObjectIdentifier{1, 2, 840, 113549, 1, 1, 13};

internal static asn1.ObjectIdentifier oidSignatureRSAPSS = new asn1.ObjectIdentifier{1, 2, 840, 113549, 1, 1, 10};

internal static asn1.ObjectIdentifier oidSignatureDSAWithSHA1 = new asn1.ObjectIdentifier{1, 2, 840, 10040, 4, 3};

internal static asn1.ObjectIdentifier oidSignatureDSAWithSHA256 = new asn1.ObjectIdentifier{2, 16, 840, 1, 101, 3, 4, 3, 2};

internal static asn1.ObjectIdentifier oidSignatureECDSAWithSHA1 = new asn1.ObjectIdentifier{1, 2, 840, 10045, 4, 1};

internal static asn1.ObjectIdentifier oidSignatureECDSAWithSHA256 = new asn1.ObjectIdentifier{1, 2, 840, 10045, 4, 3, 2};

internal static asn1.ObjectIdentifier oidSignatureECDSAWithSHA384 = new asn1.ObjectIdentifier{1, 2, 840, 10045, 4, 3, 3};

internal static asn1.ObjectIdentifier oidSignatureECDSAWithSHA512 = new asn1.ObjectIdentifier{1, 2, 840, 10045, 4, 3, 4};

internal static asn1.ObjectIdentifier oidSignatureEd25519 = new asn1.ObjectIdentifier{1, 3, 101, 112};

internal static asn1.ObjectIdentifier oidSHA256 = new asn1.ObjectIdentifier{2, 16, 840, 1, 101, 3, 4, 2, 1};

internal static asn1.ObjectIdentifier oidSHA384 = new asn1.ObjectIdentifier{2, 16, 840, 1, 101, 3, 4, 2, 2};

internal static asn1.ObjectIdentifier oidSHA512 = new asn1.ObjectIdentifier{2, 16, 840, 1, 101, 3, 4, 2, 3};

internal static asn1.ObjectIdentifier oidMGF1 = new asn1.ObjectIdentifier{1, 2, 840, 113549, 1, 1, 8};

internal static asn1.ObjectIdentifier oidISOSignatureSHA1WithRSA = new asn1.ObjectIdentifier{1, 3, 14, 3, 2, 29};

/* no pre-hashing */

[GoType("dyn")] partial struct Δtype {
    internal SignatureAlgorithm algo;
    internal @string name;
    internal encoding.asn1_package.ObjectIdentifier oid;
    internal encoding.asn1_package.RawValue @params;
    internal PublicKeyAlgorithm pubKeyAlgo;
    internal crypto_package.Hash hash;
    internal bool isRSAPSS;
}
internal static slice<x509.PublicKeyAlgorithm; hash crypto.Hash; isRSAPSS bool}> signatureAlgorithmDetails = new x509.PublicKeyAlgorithm; hash crypto.Hash; isRSAPSS bool}[]{
    new(MD5WithRSA, "MD5-RSA"u8, oidSignatureMD5WithRSA, asn1.NullRawValue, RSA, crypto.MD5, false),
    new(SHA1WithRSA, "SHA1-RSA"u8, oidSignatureSHA1WithRSA, asn1.NullRawValue, RSA, crypto.SHA1, false),
    new(SHA1WithRSA, "SHA1-RSA"u8, oidISOSignatureSHA1WithRSA, asn1.NullRawValue, RSA, crypto.SHA1, false),
    new(SHA256WithRSA, "SHA256-RSA"u8, oidSignatureSHA256WithRSA, asn1.NullRawValue, RSA, crypto.SHA256, false),
    new(SHA384WithRSA, "SHA384-RSA"u8, oidSignatureSHA384WithRSA, asn1.NullRawValue, RSA, crypto.SHA384, false),
    new(SHA512WithRSA, "SHA512-RSA"u8, oidSignatureSHA512WithRSA, asn1.NullRawValue, RSA, crypto.SHA512, false),
    new(SHA256WithRSAPSS, "SHA256-RSAPSS"u8, oidSignatureRSAPSS, pssParametersSHA256, RSA, crypto.SHA256, true),
    new(SHA384WithRSAPSS, "SHA384-RSAPSS"u8, oidSignatureRSAPSS, pssParametersSHA384, RSA, crypto.SHA384, true),
    new(SHA512WithRSAPSS, "SHA512-RSAPSS"u8, oidSignatureRSAPSS, pssParametersSHA512, RSA, crypto.SHA512, true),
    new(DSAWithSHA1, "DSA-SHA1"u8, oidSignatureDSAWithSHA1, emptyRawValue, DSA, crypto.SHA1, false),
    new(DSAWithSHA256, "DSA-SHA256"u8, oidSignatureDSAWithSHA256, emptyRawValue, DSA, crypto.SHA256, false),
    new(ECDSAWithSHA1, "ECDSA-SHA1"u8, oidSignatureECDSAWithSHA1, emptyRawValue, ECDSA, crypto.SHA1, false),
    new(ECDSAWithSHA256, "ECDSA-SHA256"u8, oidSignatureECDSAWithSHA256, emptyRawValue, ECDSA, crypto.SHA256, false),
    new(ECDSAWithSHA384, "ECDSA-SHA384"u8, oidSignatureECDSAWithSHA384, emptyRawValue, ECDSA, crypto.SHA384, false),
    new(ECDSAWithSHA512, "ECDSA-SHA512"u8, oidSignatureECDSAWithSHA512, emptyRawValue, ECDSA, crypto.SHA512, false),
    new(PureEd25519, "Ed25519"u8, oidSignatureEd25519, emptyRawValue, Ed25519, ((crypto.Hash)0), false)
}.slice();

internal static asn1.RawValue emptyRawValue = new asn1.RawValue(nil);

// DER encoded RSA PSS parameters for the
// SHA256, SHA384, and SHA512 hashes as defined in RFC 3447, Appendix A.2.3.
// The parameters contain the following values:
//   - hashAlgorithm contains the associated hash identifier with NULL parameters
//   - maskGenAlgorithm always contains the default mgf1SHA1 identifier
//   - saltLength contains the length of the associated hash
//   - trailerField always contains the default trailerFieldBC value
internal static asn1.RawValue pssParametersSHA256 = new asn1.RawValue(FullBytes: new byte[]{48, 52, 160, 15, 48, 13, 6, 9, 96, 134, 72, 1, 101, 3, 4, 2, 1, 5, 0, 161, 28, 48, 26, 6, 9, 42, 134, 72, 134, 247, 13, 1, 1, 8, 48, 13, 6, 9, 96, 134, 72, 1, 101, 3, 4, 2, 1, 5, 0, 162, 3, 2, 1, 32}.slice());

internal static asn1.RawValue pssParametersSHA384 = new asn1.RawValue(FullBytes: new byte[]{48, 52, 160, 15, 48, 13, 6, 9, 96, 134, 72, 1, 101, 3, 4, 2, 2, 5, 0, 161, 28, 48, 26, 6, 9, 42, 134, 72, 134, 247, 13, 1, 1, 8, 48, 13, 6, 9, 96, 134, 72, 1, 101, 3, 4, 2, 2, 5, 0, 162, 3, 2, 1, 48}.slice());

internal static asn1.RawValue pssParametersSHA512 = new asn1.RawValue(FullBytes: new byte[]{48, 52, 160, 15, 48, 13, 6, 9, 96, 134, 72, 1, 101, 3, 4, 2, 3, 5, 0, 161, 28, 48, 26, 6, 9, 42, 134, 72, 134, 247, 13, 1, 1, 8, 48, 13, 6, 9, 96, 134, 72, 1, 101, 3, 4, 2, 3, 5, 0, 162, 3, 2, 1, 64}.slice());

// pssParameters reflects the parameters in an AlgorithmIdentifier that
// specifies RSA PSS. See RFC 3447, Appendix A.2.3.
[GoType] partial struct pssParameters {
    // The following three fields are not marked as
    // optional because the default values specify SHA-1,
    // which is no longer suitable for use in signatures.
    [GoTag(@"asn1:""explicit,tag:0""")]
    public crypto.x509.pkix_package.AlgorithmIdentifier Hash;
    [GoTag(@"asn1:""explicit,tag:1""")]
    public crypto.x509.pkix_package.AlgorithmIdentifier MGF;
    [GoTag(@"asn1:""explicit,tag:2""")]
    public nint SaltLength;
    [GoTag(@"asn1:""optional,explicit,tag:3,default:1""")]
    public nint TrailerField;
}

internal static SignatureAlgorithm getSignatureAlgorithmFromAI(pkix.AlgorithmIdentifier ai) {
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
        }
        return UnknownSignatureAlgorithm;
    }
    // RSA PSS is special because it encodes important parameters
    // in the Parameters.
    ref var params = ref heap(new pssParameters(), out var Ꮡparams);
    {
        (_, err) = asn1.Unmarshal(ai.Parameters.FullBytes, Ꮡ@params); if (err != default!) {
            return UnknownSignatureAlgorithm;
        }
    }
    ref var mgf1HashFunc = ref heap(new crypto.x509.pkix_package.AlgorithmIdentifier(), out var Ꮡmgf1HashFunc);
    {
        (_, err) = asn1.Unmarshal(@params.MGF.Parameters.FullBytes, Ꮡmgf1HashFunc); if (err != default!) {
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
    switch (ᐧ) {
    case {} when @params.Hash.Algorithm.Equal(oidSHA256) && @params.SaltLength == 32: {
        return SHA256WithRSAPSS;
    }
    case {} when @params.Hash.Algorithm.Equal(oidSHA384) && @params.SaltLength == 48: {
        return SHA384WithRSAPSS;
    }
    case {} when @params.Hash.Algorithm.Equal(oidSHA512) && @params.SaltLength == 64: {
        return SHA512WithRSAPSS;
    }}

    return UnknownSignatureAlgorithm;
}

internal static asn1.ObjectIdentifier oidPublicKeyRSA = new asn1.ObjectIdentifier{1, 2, 840, 113549, 1, 1, 1};
internal static asn1.ObjectIdentifier oidPublicKeyDSA = new asn1.ObjectIdentifier{1, 2, 840, 10040, 4, 1};
internal static asn1.ObjectIdentifier oidPublicKeyECDSA = new asn1.ObjectIdentifier{1, 2, 840, 10045, 2, 1};
internal static asn1.ObjectIdentifier oidPublicKeyX25519 = new asn1.ObjectIdentifier{1, 3, 101, 110};
internal static asn1.ObjectIdentifier oidPublicKeyEd25519 = new asn1.ObjectIdentifier{1, 3, 101, 112};

// getPublicKeyAlgorithmFromOID returns the exposed PublicKeyAlgorithm
// identifier for public key types supported in certificates and CSRs. Marshal
// and Parse functions may support a different set of public key types.
internal static PublicKeyAlgorithm getPublicKeyAlgorithmFromOID(asn1.ObjectIdentifier oid) {
    switch (ᐧ) {
    case {} when oid.Equal(oidPublicKeyRSA): {
        return RSA;
    }
    case {} when oid.Equal(oidPublicKeyDSA): {
        return DSA;
    }
    case {} when oid.Equal(oidPublicKeyECDSA): {
        return ECDSA;
    }
    case {} when oid.Equal(oidPublicKeyEd25519): {
        return Ed25519;
    }}

    return UnknownPublicKeyAlgorithm;
}

// RFC 5480, 2.1.1.1. Named Curve
//
//	secp224r1 OBJECT IDENTIFIER ::= {
//	  iso(1) identified-organization(3) certicom(132) curve(0) 33 }
//
//	secp256r1 OBJECT IDENTIFIER ::= {
//	  iso(1) member-body(2) us(840) ansi-X9-62(10045) curves(3)
//	  prime(1) 7 }
//
//	secp384r1 OBJECT IDENTIFIER ::= {
//	  iso(1) identified-organization(3) certicom(132) curve(0) 34 }
//
//	secp521r1 OBJECT IDENTIFIER ::= {
//	  iso(1) identified-organization(3) certicom(132) curve(0) 35 }
//
// NB: secp256r1 is equivalent to prime256v1
internal static asn1.ObjectIdentifier oidNamedCurveP224 = new asn1.ObjectIdentifier{1, 3, 132, 0, 33};

internal static asn1.ObjectIdentifier oidNamedCurveP256 = new asn1.ObjectIdentifier{1, 2, 840, 10045, 3, 1, 7};

internal static asn1.ObjectIdentifier oidNamedCurveP384 = new asn1.ObjectIdentifier{1, 3, 132, 0, 34};

internal static asn1.ObjectIdentifier oidNamedCurveP521 = new asn1.ObjectIdentifier{1, 3, 132, 0, 35};

internal static elliptic.Curve namedCurveFromOID(asn1.ObjectIdentifier oid) {
    switch (ᐧ) {
    case {} when oid.Equal(oidNamedCurveP224): {
        return elliptic.P224();
    }
    case {} when oid.Equal(oidNamedCurveP256): {
        return elliptic.P256();
    }
    case {} when oid.Equal(oidNamedCurveP384): {
        return elliptic.P384();
    }
    case {} when oid.Equal(oidNamedCurveP521): {
        return elliptic.P521();
    }}

    return default!;
}

internal static (asn1.ObjectIdentifier, bool) oidFromNamedCurve(elliptic.Curve curve) {
    var exprᴛ1 = curve;
    if (exprᴛ1 == elliptic.P224()) {
        return (oidNamedCurveP224, true);
    }
    if (exprᴛ1 == elliptic.P256()) {
        return (oidNamedCurveP256, true);
    }
    if (exprᴛ1 == elliptic.P384()) {
        return (oidNamedCurveP384, true);
    }
    if (exprᴛ1 == elliptic.P521()) {
        return (oidNamedCurveP521, true);
    }

    return (default!, false);
}

internal static (asn1.ObjectIdentifier, bool) oidFromECDHCurve(ecdhꓸCurve curve) {
    var exprᴛ1 = curve;
    if (exprᴛ1 == ecdh.X25519()) {
        return (oidPublicKeyX25519, true);
    }
    if (exprᴛ1 == ecdh.P256()) {
        return (oidNamedCurveP256, true);
    }
    if (exprᴛ1 == ecdh.P384()) {
        return (oidNamedCurveP384, true);
    }
    if (exprᴛ1 == ecdh.P521()) {
        return (oidNamedCurveP521, true);
    }

    return (default!, false);
}

[GoType("num:nint")] partial struct KeyUsage;

public static readonly KeyUsage KeyUsageDigitalSignature = /* 1 << iota */ 1;
public static readonly KeyUsage KeyUsageContentCommitment = 2;
public static readonly KeyUsage KeyUsageKeyEncipherment = 4;
public static readonly KeyUsage KeyUsageDataEncipherment = 8;
public static readonly KeyUsage KeyUsageKeyAgreement = 16;
public static readonly KeyUsage KeyUsageCertSign = 32;
public static readonly KeyUsage KeyUsageCRLSign = 64;
public static readonly KeyUsage KeyUsageEncipherOnly = 128;
public static readonly KeyUsage KeyUsageDecipherOnly = 256;

// RFC 5280, 4.2.1.12  Extended Key Usage
//
//	anyExtendedKeyUsage OBJECT IDENTIFIER ::= { id-ce-extKeyUsage 0 }
//
//	id-kp OBJECT IDENTIFIER ::= { id-pkix 3 }
//
//	id-kp-serverAuth             OBJECT IDENTIFIER ::= { id-kp 1 }
//	id-kp-clientAuth             OBJECT IDENTIFIER ::= { id-kp 2 }
//	id-kp-codeSigning            OBJECT IDENTIFIER ::= { id-kp 3 }
//	id-kp-emailProtection        OBJECT IDENTIFIER ::= { id-kp 4 }
//	id-kp-timeStamping           OBJECT IDENTIFIER ::= { id-kp 8 }
//	id-kp-OCSPSigning            OBJECT IDENTIFIER ::= { id-kp 9 }
internal static asn1.ObjectIdentifier oidExtKeyUsageAny = new asn1.ObjectIdentifier{2, 5, 29, 37, 0};

internal static asn1.ObjectIdentifier oidExtKeyUsageServerAuth = new asn1.ObjectIdentifier{1, 3, 6, 1, 5, 5, 7, 3, 1};

internal static asn1.ObjectIdentifier oidExtKeyUsageClientAuth = new asn1.ObjectIdentifier{1, 3, 6, 1, 5, 5, 7, 3, 2};

internal static asn1.ObjectIdentifier oidExtKeyUsageCodeSigning = new asn1.ObjectIdentifier{1, 3, 6, 1, 5, 5, 7, 3, 3};

internal static asn1.ObjectIdentifier oidExtKeyUsageEmailProtection = new asn1.ObjectIdentifier{1, 3, 6, 1, 5, 5, 7, 3, 4};

internal static asn1.ObjectIdentifier oidExtKeyUsageIPSECEndSystem = new asn1.ObjectIdentifier{1, 3, 6, 1, 5, 5, 7, 3, 5};

internal static asn1.ObjectIdentifier oidExtKeyUsageIPSECTunnel = new asn1.ObjectIdentifier{1, 3, 6, 1, 5, 5, 7, 3, 6};

internal static asn1.ObjectIdentifier oidExtKeyUsageIPSECUser = new asn1.ObjectIdentifier{1, 3, 6, 1, 5, 5, 7, 3, 7};

internal static asn1.ObjectIdentifier oidExtKeyUsageTimeStamping = new asn1.ObjectIdentifier{1, 3, 6, 1, 5, 5, 7, 3, 8};

internal static asn1.ObjectIdentifier oidExtKeyUsageOCSPSigning = new asn1.ObjectIdentifier{1, 3, 6, 1, 5, 5, 7, 3, 9};

internal static asn1.ObjectIdentifier oidExtKeyUsageMicrosoftServerGatedCrypto = new asn1.ObjectIdentifier{1, 3, 6, 1, 4, 1, 311, 10, 3, 3};

internal static asn1.ObjectIdentifier oidExtKeyUsageNetscapeServerGatedCrypto = new asn1.ObjectIdentifier{2, 16, 840, 1, 113730, 4, 1};

internal static asn1.ObjectIdentifier oidExtKeyUsageMicrosoftCommercialCodeSigning = new asn1.ObjectIdentifier{1, 3, 6, 1, 4, 1, 311, 2, 1, 22};

internal static asn1.ObjectIdentifier oidExtKeyUsageMicrosoftKernelCodeSigning = new asn1.ObjectIdentifier{1, 3, 6, 1, 4, 1, 311, 61, 1, 1};

[GoType("num:nint")] partial struct ExtKeyUsage;

public static readonly ExtKeyUsage ExtKeyUsageAny = /* iota */ 0;
public static readonly ExtKeyUsage ExtKeyUsageServerAuth = 1;
public static readonly ExtKeyUsage ExtKeyUsageClientAuth = 2;
public static readonly ExtKeyUsage ExtKeyUsageCodeSigning = 3;
public static readonly ExtKeyUsage ExtKeyUsageEmailProtection = 4;
public static readonly ExtKeyUsage ExtKeyUsageIPSECEndSystem = 5;
public static readonly ExtKeyUsage ExtKeyUsageIPSECTunnel = 6;
public static readonly ExtKeyUsage ExtKeyUsageIPSECUser = 7;
public static readonly ExtKeyUsage ExtKeyUsageTimeStamping = 8;
public static readonly ExtKeyUsage ExtKeyUsageOCSPSigning = 9;
public static readonly ExtKeyUsage ExtKeyUsageMicrosoftServerGatedCrypto = 10;
public static readonly ExtKeyUsage ExtKeyUsageNetscapeServerGatedCrypto = 11;
public static readonly ExtKeyUsage ExtKeyUsageMicrosoftCommercialCodeSigning = 12;
public static readonly ExtKeyUsage ExtKeyUsageMicrosoftKernelCodeSigning = 13;

// extKeyUsageOIDs contains the mapping between an ExtKeyUsage and its OID.

[GoType("dyn")] partial struct Δtypeᴛ1 {
    internal ExtKeyUsage extKeyUsage;
    internal encoding.asn1_package.ObjectIdentifier oid;
}
internal static slice<asn1.ObjectIdentifier}> extKeyUsageOIDs = new asn1.ObjectIdentifier}[]{
    new(ExtKeyUsageAny, oidExtKeyUsageAny),
    new(ExtKeyUsageServerAuth, oidExtKeyUsageServerAuth),
    new(ExtKeyUsageClientAuth, oidExtKeyUsageClientAuth),
    new(ExtKeyUsageCodeSigning, oidExtKeyUsageCodeSigning),
    new(ExtKeyUsageEmailProtection, oidExtKeyUsageEmailProtection),
    new(ExtKeyUsageIPSECEndSystem, oidExtKeyUsageIPSECEndSystem),
    new(ExtKeyUsageIPSECTunnel, oidExtKeyUsageIPSECTunnel),
    new(ExtKeyUsageIPSECUser, oidExtKeyUsageIPSECUser),
    new(ExtKeyUsageTimeStamping, oidExtKeyUsageTimeStamping),
    new(ExtKeyUsageOCSPSigning, oidExtKeyUsageOCSPSigning),
    new(ExtKeyUsageMicrosoftServerGatedCrypto, oidExtKeyUsageMicrosoftServerGatedCrypto),
    new(ExtKeyUsageNetscapeServerGatedCrypto, oidExtKeyUsageNetscapeServerGatedCrypto),
    new(ExtKeyUsageMicrosoftCommercialCodeSigning, oidExtKeyUsageMicrosoftCommercialCodeSigning),
    new(ExtKeyUsageMicrosoftKernelCodeSigning, oidExtKeyUsageMicrosoftKernelCodeSigning)
}.slice();

internal static (ExtKeyUsage eku, bool ok) extKeyUsageFromOID(asn1.ObjectIdentifier oid) {
    ExtKeyUsage eku = default!;
    bool ok = default!;

    foreach (var (_, pair) in extKeyUsageOIDs) {
        if (oid.Equal(pair.oid)) {
            return (pair.extKeyUsage, true);
        }
    }
    return (eku, ok);
}

internal static (asn1.ObjectIdentifier oid, bool ok) oidFromExtKeyUsage(ExtKeyUsage eku) {
    asn1.ObjectIdentifier oid = default!;
    bool ok = default!;

    foreach (var (_, pair) in extKeyUsageOIDs) {
        if (eku == pair.extKeyUsage) {
            return (pair.oid, true);
        }
    }
    return (oid, ok);
}

// A Certificate represents an X.509 certificate.
[GoType] partial struct Certificate {
    public slice<byte> Raw; // Complete ASN.1 DER content (certificate, signature algorithm and signature).
    public slice<byte> RawTBSCertificate; // Certificate part of raw ASN.1 DER content.
    public slice<byte> RawSubjectPublicKeyInfo; // DER encoded SubjectPublicKeyInfo.
    public slice<byte> RawSubject; // DER encoded Subject
    public slice<byte> RawIssuer; // DER encoded Issuer
    public slice<byte> Signature;
    public SignatureAlgorithm SignatureAlgorithm;
    public PublicKeyAlgorithm PublicKeyAlgorithm;
    public any PublicKey;
    public nint Version;
    public ж<math.big_package.ΔInt> SerialNumber;
    public crypto.x509.pkix_package.Name Issuer;
    public crypto.x509.pkix_package.Name Subject;
    public time_package.Time NotBefore; // Validity bounds.
    public time_package.Time NotAfter;
    public KeyUsage KeyUsage;
    // Extensions contains raw X.509 extensions. When parsing certificates,
    // this can be used to extract non-critical extensions that are not
    // parsed by this package. When marshaling certificates, the Extensions
    // field is ignored, see ExtraExtensions.
    public pkix.Extension Extensions;
    // ExtraExtensions contains extensions to be copied, raw, into any
    // marshaled certificates. Values override any extensions that would
    // otherwise be produced based on the other fields. The ExtraExtensions
    // field is not populated when parsing certificates, see Extensions.
    public pkix.Extension ExtraExtensions;
    // UnhandledCriticalExtensions contains a list of extension IDs that
    // were not (fully) processed when parsing. Verify will fail if this
    // slice is non-empty, unless verification is delegated to an OS
    // library which understands all the critical extensions.
    //
    // Users can access these extensions using Extensions and can remove
    // elements from this slice if they believe that they have been
    // handled.
    public asn1.ObjectIdentifier UnhandledCriticalExtensions;
    public slice<ExtKeyUsage> ExtKeyUsage;      // Sequence of extended key usages.
    public asn1.ObjectIdentifier UnknownExtKeyUsage; // Encountered extended key usages unknown to this package.
    // BasicConstraintsValid indicates whether IsCA, MaxPathLen,
    // and MaxPathLenZero are valid.
    public bool BasicConstraintsValid;
    public bool IsCA;
    // MaxPathLen and MaxPathLenZero indicate the presence and
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
    public nint MaxPathLen;
    // MaxPathLenZero indicates that BasicConstraintsValid==true
    // and MaxPathLen==0 should be interpreted as an actual
    // maximum path length of zero. Otherwise, that combination is
    // interpreted as MaxPathLen not being set.
    public bool MaxPathLenZero;
    public slice<byte> SubjectKeyId;
    public slice<byte> AuthorityKeyId;
    // RFC 5280, 4.2.2.1 (Authority Information Access)
    public slice<@string> OCSPServer;
    public slice<@string> IssuingCertificateURL;
    // Subject Alternate Name values. (Note that these values may not be valid
    // if invalid values were contained within a parsed certificate. For
    // example, an element of DNSNames may not be a valid DNS domain name.)
    public slice<@string> DNSNames;
    public slice<@string> EmailAddresses;
    public slice<net.IP> IPAddresses;
    public url.URL URIs;
    // Name constraints
    public bool PermittedDNSDomainsCritical; // if true then the name constraints are marked critical.
    public slice<@string> PermittedDNSDomains;
    public slice<@string> ExcludedDNSDomains;
    public slice<ж<net.IPNet>> PermittedIPRanges;
    public slice<ж<net.IPNet>> ExcludedIPRanges;
    public slice<@string> PermittedEmailAddresses;
    public slice<@string> ExcludedEmailAddresses;
    public slice<@string> PermittedURIDomains;
    public slice<@string> ExcludedURIDomains;
    // CRL Distribution Points
    public slice<@string> CRLDistributionPoints;
    // PolicyIdentifiers contains asn1.ObjectIdentifiers, the components
    // of which are limited to int32. If a certificate contains a policy which
    // cannot be represented by asn1.ObjectIdentifier, it will not be included in
    // PolicyIdentifiers, but will be present in Policies, which contains all parsed
    // policy OIDs.
    public asn1.ObjectIdentifier PolicyIdentifiers;
    // Policies contains all policy identifiers included in the certificate.
    // In Go 1.22, encoding/gob cannot handle and ignores this field.
    public slice<OID> Policies;
}

// ErrUnsupportedAlgorithm results from attempting to perform an operation that
// involves algorithms that are not currently implemented.
public static error ErrUnsupportedAlgorithm = errors.New("x509: cannot verify signature: algorithm unimplemented"u8);

[GoType("num:nint")] partial struct InsecureAlgorithmError;

public static @string Error(this InsecureAlgorithmError e) {
    @string @override = default!;
    if (((SignatureAlgorithm)e) == SHA1WithRSA || ((SignatureAlgorithm)e) == ECDSAWithSHA1) {
        @override = " (temporarily override with GODEBUG=x509sha1=1)"u8;
    }
    return fmt.Sprintf("x509: cannot verify signature: insecure algorithm %v"u8, ((SignatureAlgorithm)e)) + @override;
}

// ConstraintViolationError results when a requested usage is not permitted by
// a certificate. For example: checking a signature when the public key isn't a
// certificate signing key.
[GoType] partial struct ConstraintViolationError {
}

public static @string Error(this ConstraintViolationError _) {
    return "x509: invalid signature: parent certificate cannot sign this kind of certificate"u8;
}

[GoRecv] public static bool Equal(this ref Certificate c, ж<Certificate> Ꮡother) {
    ref var other = ref Ꮡother.val;

    if (c == nil || other == nil) {
        return c == Ꮡother;
    }
    return bytes.Equal(c.Raw, other.Raw);
}

[GoRecv] internal static bool hasSANExtension(this ref Certificate c) {
    return oidInExtensions(oidExtensionSubjectAltName, c.Extensions);
}

// CheckSignatureFrom verifies that the signature on c is a valid signature from parent.
//
// This is a low-level API that performs very limited checks, and not a full
// path verifier. Most users should use [Certificate.Verify] instead.
[GoRecv] public static error CheckSignatureFrom(this ref Certificate c, ж<Certificate> Ꮡparent) {
    ref var parent = ref Ꮡparent.val;

    // RFC 5280, 4.2.1.9:
    // "If the basic constraints extension is not present in a version 3
    // certificate, or the extension is present but the cA boolean is not
    // asserted, then the certified public key MUST NOT be used to verify
    // certificate signatures."
    if (parent.Version == 3 && !parent.BasicConstraintsValid || parent.BasicConstraintsValid && !parent.IsCA) {
        return new ConstraintViolationError(nil);
    }
    if (parent.KeyUsage != 0 && (KeyUsage)(parent.KeyUsage & KeyUsageCertSign) == 0) {
        return new ConstraintViolationError(nil);
    }
    if (parent.PublicKeyAlgorithm == UnknownPublicKeyAlgorithm) {
        return ErrUnsupportedAlgorithm;
    }
    return checkSignature(c.SignatureAlgorithm, c.RawTBSCertificate, c.Signature, parent.PublicKey, false);
}

// CheckSignature verifies that signature is a valid signature over signed from
// c's public key.
//
// This is a low-level API that performs no validity checks on the certificate.
//
// [MD5WithRSA] signatures are rejected, while [SHA1WithRSA] and [ECDSAWithSHA1]
// signatures are currently accepted.
[GoRecv] public static error CheckSignature(this ref Certificate c, SignatureAlgorithm algo, slice<byte> signed, slice<byte> signature) {
    return checkSignature(algo, signed, signature, c.PublicKey, true);
}

[GoRecv] internal static bool hasNameConstraints(this ref Certificate c) {
    return oidInExtensions(oidExtensionNameConstraints, c.Extensions);
}

[GoRecv] internal static slice<byte> getSANExtension(this ref Certificate c) {
    foreach (var (_, e) in c.Extensions) {
        if (e.Id.Equal(oidExtensionSubjectAltName)) {
            return e.Value;
        }
    }
    return default!;
}

internal static error signaturePublicKeyAlgoMismatchError(PublicKeyAlgorithm expectedPubKeyAlgo, any pubKey) {
    return fmt.Errorf("x509: signature algorithm specifies an %s public key, but have public key of type %T"u8, expectedPubKeyAlgo.String(), pubKey);
}

internal static ж<godebug.Setting> x509sha1 = godebug.New("x509sha1"u8);

// checkSignature verifies that signature is a valid signature over signed from
// a crypto.PublicKey.
internal static error /*err*/ checkSignature(SignatureAlgorithm algo, slice<byte> signed, slice<byte> signature, crypto.PublicKey publicKey, bool allowSHA1) {
    error err = default!;

    crypto.Hash hashType = default!;
    PublicKeyAlgorithm pubKeyAlgo = default!;
    foreach (var (_, details) in signatureAlgorithmDetails) {
        if (details.algo == algo) {
            hashType = details.hash;
            pubKeyAlgo = details.pubKeyAlgo;
            break;
        }
    }
    var exprᴛ1 = hashType;
    var matchᴛ1 = false;
    if (exprᴛ1 == ((crypto.Hash)0)) { matchᴛ1 = true;
        if (pubKeyAlgo != Ed25519) {
            return ErrUnsupportedAlgorithm;
        }
    }
    if (exprᴛ1 == crypto.MD5) { matchᴛ1 = true;
        return ((InsecureAlgorithmError)algo);
    }
    if (exprᴛ1 == crypto.SHA1) { matchᴛ1 = true;
        if (!allowSHA1) {
            // SHA-1 signatures are mostly disabled. See go.dev/issue/41682.
            if (x509sha1.Value() != "1"u8) {
                return ((InsecureAlgorithmError)algo);
            }
            x509sha1.IncNonDefault();
        }
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1) { /* default: */
        if (!hashType.Available()) {
            return ErrUnsupportedAlgorithm;
        }
        var h = hashType.New();
        h.Write(signed);
        signed = h.Sum(default!);
    }

    switch (publicKey.type()) {
    case ж<rsa.PublicKey> pub: {
        if (pubKeyAlgo != RSA) {
            return signaturePublicKeyAlgoMismatchError(pubKeyAlgo, pub);
        }
        if (algo.isRSAPSS()){
            return rsa.VerifyPSS(pub, hashType, signed, signature, Ꮡ(new rsa.PSSOptions(SaltLength: rsa.PSSSaltLengthEqualsHash)));
        } else {
            return rsa.VerifyPKCS1v15(pub, hashType, signed, signature);
        }
        break;
    }
    case ж<ecdsa.PublicKey> pub: {
        if (pubKeyAlgo != ECDSA) {
            return signaturePublicKeyAlgoMismatchError(pubKeyAlgo, pub);
        }
        if (!ecdsa.VerifyASN1(pub, signed, signature)) {
            return errors.New("x509: ECDSA verification failure"u8);
        }
        return err;
    }
    case ed25519.PublicKey pub: {
        if (pubKeyAlgo != Ed25519) {
            return signaturePublicKeyAlgoMismatchError(pubKeyAlgo, pub);
        }
        if (!ed25519.Verify(pub, signed, signature)) {
            return errors.New("x509: Ed25519 verification failure"u8);
        }
        return err;
    }}
    return ErrUnsupportedAlgorithm;
}

// CheckCRLSignature checks that the signature in crl is from c.
//
// Deprecated: Use [RevocationList.CheckSignatureFrom] instead.
[GoRecv] public static error CheckCRLSignature(this ref Certificate c, ж<pkix.CertificateList> Ꮡcrl) {
    ref var crl = ref Ꮡcrl.val;

    SignatureAlgorithm algo = getSignatureAlgorithmFromAI(crl.SignatureAlgorithm);
    return c.CheckSignature(algo, crl.TBSCertList.Raw, crl.SignatureValue.RightAlign());
}

[GoType] partial struct UnhandledCriticalExtension {
}

public static @string Error(this UnhandledCriticalExtension h) {
    return "x509: unhandled critical extension"u8;
}

[GoType] partial struct basicConstraints {
    [GoTag(@"asn1:""optional""")]
    public bool IsCA;
    [GoTag(@"asn1:""optional,default:-1""")]
    public nint MaxPathLen;
}

// RFC 5280 4.2.1.4
[GoType] partial struct policyInformation {
    public encoding.asn1_package.ObjectIdentifier Policy;
}

// policyQualifiers omitted
internal static readonly UntypedInt nameTypeEmail = 1;
internal static readonly UntypedInt nameTypeDNS = 2;
internal static readonly UntypedInt nameTypeURI = 6;
internal static readonly UntypedInt nameTypeIP = 7;

// RFC 5280, 4.2.2.1
[GoType] partial struct authorityInfoAccess {
    public encoding.asn1_package.ObjectIdentifier Method;
    public encoding.asn1_package.RawValue Location;
}

// RFC 5280, 4.2.1.14
[GoType] partial struct distributionPoint {
    [GoTag(@"asn1:""optional,tag:0""")]
    public distributionPointName DistributionPoint;
    [GoTag(@"asn1:""optional,tag:1""")]
    public encoding.asn1_package.BitString Reason;
    [GoTag(@"asn1:""optional,tag:2""")]
    public encoding.asn1_package.RawValue CRLIssuer;
}

[GoType] partial struct distributionPointName {
    [GoTag(@"asn1:""optional,tag:0""")]
    public asn1.RawValue FullName;
    [GoTag(@"asn1:""optional,tag:1""")]
    public crypto.x509.pkix_package.RDNSequence RelativeName;
}

internal static byte reverseBitsInAByte(byte @in) {
    var b1 = (byte)(@in >> (int)(4) | @in << (int)(4));
    var b2 = (byte)((byte)(b1 >> (int)(2) & 51) | (byte)(b1 << (int)(2) & 204));
    var b3 = (byte)((byte)(b2 >> (int)(1) & 85) | (byte)(b2 << (int)(1) & 170));
    return b3;
}

// asn1BitLength returns the bit-length of bitString by considering the
// most-significant bit in a byte to be the "first" bit. This convention
// matches ASN.1, but differs from almost everything else.
internal static nint asn1BitLength(slice<byte> bitString) {
    nint bitLen = len(bitString) * 8;
    foreach (var (i, _) in bitString) {
        var b = bitString[len(bitString) - i - 1];
        for (nuint bit = ((nuint)0); bit < 8; bit++) {
            if ((byte)((b >> (int)(bit)) & 1) == 1) {
                return bitLen;
            }
            bitLen--;
        }
    }
    return 0;
}

internal static slice<nint> oidExtensionSubjectKeyId = new nint[]{2, 5, 29, 14}.slice();
internal static slice<nint> oidExtensionKeyUsage = new nint[]{2, 5, 29, 15}.slice();
internal static slice<nint> oidExtensionExtendedKeyUsage = new nint[]{2, 5, 29, 37}.slice();
internal static slice<nint> oidExtensionAuthorityKeyId = new nint[]{2, 5, 29, 35}.slice();
internal static slice<nint> oidExtensionBasicConstraints = new nint[]{2, 5, 29, 19}.slice();
internal static slice<nint> oidExtensionSubjectAltName = new nint[]{2, 5, 29, 17}.slice();
internal static slice<nint> oidExtensionCertificatePolicies = new nint[]{2, 5, 29, 32}.slice();
internal static slice<nint> oidExtensionNameConstraints = new nint[]{2, 5, 29, 30}.slice();
internal static slice<nint> oidExtensionCRLDistributionPoints = new nint[]{2, 5, 29, 31}.slice();
internal static slice<nint> oidExtensionAuthorityInfoAccess = new nint[]{1, 3, 6, 1, 5, 5, 7, 1, 1}.slice();
internal static slice<nint> oidExtensionCRLNumber = new nint[]{2, 5, 29, 20}.slice();
internal static slice<nint> oidExtensionReasonCode = new nint[]{2, 5, 29, 21}.slice();

internal static asn1.ObjectIdentifier oidAuthorityInfoAccessOcsp = new asn1.ObjectIdentifier{1, 3, 6, 1, 5, 5, 7, 48, 1};
internal static asn1.ObjectIdentifier oidAuthorityInfoAccessIssuers = new asn1.ObjectIdentifier{1, 3, 6, 1, 5, 5, 7, 48, 2};

// oidInExtensions reports whether an extension with the given oid exists in
// extensions.
internal static bool oidInExtensions(asn1.ObjectIdentifier oid, slice<pkix.Extension> extensions) {
    foreach (var (_, e) in extensions) {
        if (e.Id.Equal(oid)) {
            return true;
        }
    }
    return false;
}

// marshalSANs marshals a list of addresses into a the contents of an X.509
// SubjectAlternativeName extension.
internal static (slice<byte> derBytes, error err) marshalSANs(slice<@string> dnsNames, slice<@string> emailAddresses, slice<net.IP> ipAddresses, slice<url.URL> uris) {
    slice<byte> derBytes = default!;
    error err = default!;

    slice<asn1.RawValue> rawValues = default!;
    foreach (var (_, name) in dnsNames) {
        {
            var errΔ1 = isIA5String(name); if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
        }
        rawValues = append(rawValues, new asn1.RawValue(Tag: nameTypeDNS, Class: 2, Bytes: slice<byte>(name)));
    }
    foreach (var (_, email) in emailAddresses) {
        {
            var errΔ2 = isIA5String(email); if (errΔ2 != default!) {
                return (default!, errΔ2);
            }
        }
        rawValues = append(rawValues, new asn1.RawValue(Tag: nameTypeEmail, Class: 2, Bytes: slice<byte>(email)));
    }
    foreach (var (_, rawIP) in ipAddresses) {
        // If possible, we always want to encode IPv4 addresses in 4 bytes.
        var ip = rawIP.To4();
        if (ip == default!) {
            ip = rawIP;
        }
        rawValues = append(rawValues, new asn1.RawValue(Tag: nameTypeIP, Class: 2, Bytes: ip));
    }
    foreach (var (_, uri) in uris) {
        @string uriStr = uri.String();
        {
            var errΔ3 = isIA5String(uriStr); if (errΔ3 != default!) {
                return (default!, errΔ3);
            }
        }
        rawValues = append(rawValues, new asn1.RawValue(Tag: nameTypeURI, Class: 2, Bytes: slice<byte>(uriStr)));
    }
    return asn1.Marshal(rawValues);
}

internal static error isIA5String(@string s) {
    foreach (var (_, r) in s) {
        // Per RFC5280 "IA5String is limited to the set of ASCII characters"
        if (r > unicode.MaxASCII) {
            return fmt.Errorf("x509: %q cannot be encoded as an IA5String"u8, s);
        }
    }
    return default!;
}

internal static ж<godebug.Setting> x509usepolicies = godebug.New("x509usepolicies"u8);

internal static (slice<pkix.Extension> ret, error err) buildCertExtensions(ж<Certificate> Ꮡtemplate, bool subjectIsEmpty, slice<byte> authorityKeyId, slice<byte> subjectKeyId) {
    slice<pkix.Extension> ret = default!;
    error err = default!;

    ref var template = ref Ꮡtemplate.val;
    ret = new slice<pkix.Extension>(10);
    /* maximum number of elements. */
    nint n = 0;
    if (template.KeyUsage != 0 && !oidInExtensions(oidExtensionKeyUsage, template.ExtraExtensions)) {
        (ret[n], err) = marshalKeyUsage(template.KeyUsage);
        if (err != default!) {
            return (default!, err);
        }
        n++;
    }
    if ((len(template.ExtKeyUsage) > 0 || len(template.UnknownExtKeyUsage) > 0) && !oidInExtensions(oidExtensionExtendedKeyUsage, template.ExtraExtensions)) {
        (ret[n], err) = marshalExtKeyUsage(template.ExtKeyUsage, template.UnknownExtKeyUsage);
        if (err != default!) {
            return (default!, err);
        }
        n++;
    }
    if (template.BasicConstraintsValid && !oidInExtensions(oidExtensionBasicConstraints, template.ExtraExtensions)) {
        (ret[n], err) = marshalBasicConstraints(template.IsCA, template.MaxPathLen, template.MaxPathLenZero);
        if (err != default!) {
            return (default!, err);
        }
        n++;
    }
    if (len(subjectKeyId) > 0 && !oidInExtensions(oidExtensionSubjectKeyId, template.ExtraExtensions)) {
        ret[n].Id = oidExtensionSubjectKeyId;
        (ret[n].Value, err) = asn1.Marshal(subjectKeyId);
        if (err != default!) {
            return (ret, err);
        }
        n++;
    }
    if (len(authorityKeyId) > 0 && !oidInExtensions(oidExtensionAuthorityKeyId, template.ExtraExtensions)) {
        ret[n].Id = oidExtensionAuthorityKeyId;
        (ret[n].Value, err) = asn1.Marshal(new authKeyId(authorityKeyId));
        if (err != default!) {
            return (ret, err);
        }
        n++;
    }
    if ((len(template.OCSPServer) > 0 || len(template.IssuingCertificateURL) > 0) && !oidInExtensions(oidExtensionAuthorityInfoAccess, template.ExtraExtensions)) {
        ret[n].Id = oidExtensionAuthorityInfoAccess;
        slice<authorityInfoAccess> aiaValues = default!;
        foreach (var (_, name) in template.OCSPServer) {
            aiaValues = append(aiaValues, new authorityInfoAccess(
                Method: oidAuthorityInfoAccessOcsp,
                Location: new asn1.RawValue(Tag: 6, Class: 2, Bytes: slice<byte>(name))
            ));
        }
        foreach (var (_, name) in template.IssuingCertificateURL) {
            aiaValues = append(aiaValues, new authorityInfoAccess(
                Method: oidAuthorityInfoAccessIssuers,
                Location: new asn1.RawValue(Tag: 6, Class: 2, Bytes: slice<byte>(name))
            ));
        }
        (ret[n].Value, err) = asn1.Marshal(aiaValues);
        if (err != default!) {
            return (ret, err);
        }
        n++;
    }
    if ((len(template.DNSNames) > 0 || len(template.EmailAddresses) > 0 || len(template.IPAddresses) > 0 || len(template.URIs) > 0) && !oidInExtensions(oidExtensionSubjectAltName, template.ExtraExtensions)) {
        ret[n].Id = oidExtensionSubjectAltName;
        // From RFC 5280, Section 4.2.1.6:
        // “If the subject field contains an empty sequence ... then
        // subjectAltName extension ... is marked as critical”
        ret[n].Critical = subjectIsEmpty;
        (ret[n].Value, err) = marshalSANs(template.DNSNames, template.EmailAddresses, template.IPAddresses, template.URIs);
        if (err != default!) {
            return (ret, err);
        }
        n++;
    }
    var usePolicies = x509usepolicies.Value() == "1"u8;
    if (((!usePolicies && len(template.PolicyIdentifiers) > 0) || (usePolicies && len(template.Policies) > 0)) && !oidInExtensions(oidExtensionCertificatePolicies, template.ExtraExtensions)) {
        (ret[n], err) = marshalCertificatePolicies(template.Policies, template.PolicyIdentifiers);
        if (err != default!) {
            return (default!, err);
        }
        n++;
    }
    if ((len(template.PermittedDNSDomains) > 0 || len(template.ExcludedDNSDomains) > 0 || len(template.PermittedIPRanges) > 0 || len(template.ExcludedIPRanges) > 0 || len(template.PermittedEmailAddresses) > 0 || len(template.ExcludedEmailAddresses) > 0 || len(template.PermittedURIDomains) > 0 || len(template.ExcludedURIDomains) > 0) && !oidInExtensions(oidExtensionNameConstraints, template.ExtraExtensions)) {
        ret[n].Id = oidExtensionNameConstraints;
        ret[n].Critical = template.PermittedDNSDomainsCritical;
        var ipAndMask = (ж<net.IPNet> ipNet) => {
            var maskedIP = (~ipNet).IP.Mask((~ipNet).Mask);
            var ipAndMaskΔ1 = new slice<byte>(0, len(maskedIP) + len((~ipNet).Mask));
            ipAndMaskΔ1 = append(ipAndMaskΔ1, maskedIP.ꓸꓸꓸ);
            ipAndMaskΔ1 = append(ipAndMaskΔ1, (~ipNet).Mask.ꓸꓸꓸ);
            return ipAndMaskΔ1;
        };
        var serialiseConstraints = 
        var ipAndMaskʗ1 = ipAndMask;
        (slice<@string> dns, slice<ж<net.IPNet>> ips, slice<@string> emails, slice<@string> uriDomains) => {
            ref var bΔ1 = ref heap(new vendor.golang.org.x.crypto.cryptobyte_package.Builder(), out var ᏑbΔ1);
            foreach (var (_, name) in dns) {
                {
                    errΔ1 = isIA5String(name); if (errΔ1 != default!) {
                        return (default!, errΔ1);
                    }
                }
                bΔ1.AddASN1(cryptobyte_asn1.SEQUENCE, 
                (ж<cryptobyte.Builder> b) => {
                    bΔ2.AddASN1(((asn1.Tag)2).ContextSpecific(), 
                    (ж<cryptobyte.Builder> b) => {
                        bΔ3.AddBytes(slice<byte>(name));
                    });
                });
            }
            foreach (var (_, ipNet) in ips) {
                bΔ1.AddASN1(cryptobyte_asn1.SEQUENCE, 
                var ipAndMaskʗ2 = ipAndMask;
                var ipNetʗ1 = ipNet;
                (ж<cryptobyte.Builder> b) => {
                    bΔ4.AddASN1(((asn1.Tag)7).ContextSpecific(), 
                    var ipAndMaskʗ3 = ipAndMask;
                    var ipNetʗ2 = ipNet;
                    (ж<cryptobyte.Builder> b) => {
                        bΔ5.AddBytes(ipAndMaskʗ3(ipNetʗ2));
                    });
                });
            }
            foreach (var (_, email) in emails) {
                {
                    errΔ1 = isIA5String(email); if (errΔ1 != default!) {
                        return (default!, errΔ1);
                    }
                }
                bΔ1.AddASN1(cryptobyte_asn1.SEQUENCE, 
                (ж<cryptobyte.Builder> b) => {
                    bΔ6.AddASN1(((asn1.Tag)1).ContextSpecific(), 
                    (ж<cryptobyte.Builder> b) => {
                        bΔ7.AddBytes(slice<byte>(email));
                    });
                });
            }
            foreach (var (_, uriDomain) in uriDomains) {
                {
                    errΔ1 = isIA5String(uriDomain); if (errΔ1 != default!) {
                        return (default!, errΔ1);
                    }
                }
                bΔ1.AddASN1(cryptobyte_asn1.SEQUENCE, 
                (ж<cryptobyte.Builder> b) => {
                    bΔ8.AddASN1(((asn1.Tag)6).ContextSpecific(), 
                    (ж<cryptobyte.Builder> b) => {
                        bΔ9.AddBytes(slice<byte>(uriDomain));
                    });
                });
            }
            return bΔ1.Bytes();
        };
        (permitted, err) = serialiseConstraints(template.PermittedDNSDomains, template.PermittedIPRanges, template.PermittedEmailAddresses, template.PermittedURIDomains);
        if (err != default!) {
            return (default!, err);
        }
        (excluded, errΔ1) = serialiseConstraints(template.ExcludedDNSDomains, template.ExcludedIPRanges, template.ExcludedEmailAddresses, template.ExcludedURIDomains);
        if (err != default!) {
            return (default!, err);
        }
        cryptobyte.Builder b = default!;
        b.AddASN1(cryptobyte_asn1.SEQUENCE, 
        var excludedʗ1 = excluded;
        var permittedʗ1 = permitted;
        (ж<cryptobyte.Builder> b) => {
            if (len(permittedʗ1) > 0) {
                bΔ10.AddASN1(((asn1.Tag)0).ContextSpecific().Constructed(), 
                var permittedʗ2 = permitted;
                (ж<cryptobyte.Builder> b) => {
                    bΔ11.AddBytes(permittedʗ2);
                });
            }
            if (len(excluded) > 0) {
                bΔ10.AddASN1(((asn1.Tag)1).ContextSpecific().Constructed(), 
                var excludedʗ2 = excluded;
                (ж<cryptobyte.Builder> b) => {
                    bΔ12.AddBytes(excludedʗ2);
                });
            }
        });
        (ret[n].Value, errΔ1) = b.Bytes();
        if (err != default!) {
            return (default!, err);
        }
        n++;
    }
    if (len(template.CRLDistributionPoints) > 0 && !oidInExtensions(oidExtensionCRLDistributionPoints, template.ExtraExtensions)) {
        ret[n].Id = oidExtensionCRLDistributionPoints;
        slice<distributionPoint> crlDp = default!;
        foreach (var (_, name) in template.CRLDistributionPoints) {
            var dp = new distributionPoint(
                DistributionPoint: new distributionPointName(
                    FullName: new asn1.RawValue[]{
                        new(Tag: 6, Class: 2, Bytes: slice<byte>(name))
                    }.slice()
                )
            );
            crlDp = append(crlDp, dp);
        }
        (ret[n].Value, errΔ1) = asn1.Marshal(crlDp);
        if (err != default!) {
            return (ret, err);
        }
        n++;
    }
    // Adding another extension here? Remember to update the maximum number
    // of elements in the make() at the top of the function and the list of
    // template fields used in CreateCertificate documentation.
    return (append(ret[..(int)(n)], template.ExtraExtensions.ꓸꓸꓸ), default!);
}

internal static (pkix.Extension, error) marshalKeyUsage(KeyUsage ku) {
    var ext = new pkix.Extension(Id: oidExtensionKeyUsage, Critical: true);
    array<byte> a = new(2);
    a[0] = reverseBitsInAByte(((byte)ku));
    a[1] = reverseBitsInAByte(((byte)(ku >> (int)(8))));
    nint l = 1;
    if (a[1] != 0) {
        l = 2;
    }
    var bitString = a[..(int)(l)];
    error err = default!;
    (ext.Value, err) = asn1.Marshal(new asn1.BitString(Bytes: bitString, BitLength: asn1BitLength(bitString)));
    return (ext, err);
}

internal static (pkix.Extension, error) marshalExtKeyUsage(slice<ExtKeyUsage> extUsages, slice<asn1.ObjectIdentifier> unknownUsages) {
    var ext = new pkix.Extension(Id: oidExtensionExtendedKeyUsage);
    var oids = new slice<asn1.ObjectIdentifier>(len(extUsages) + len(unknownUsages));
    foreach (var (i, u) in extUsages) {
        {
            var (oid, ok) = oidFromExtKeyUsage(u); if (ok){
                oids[i] = oid;
            } else {
                return (ext, errors.New("x509: unknown extended key usage"u8));
            }
        }
    }
    copy(oids[(int)(len(extUsages))..], unknownUsages);
    error err = default!;
    (ext.Value, err) = asn1.Marshal(oids);
    return (ext, err);
}

internal static (pkix.Extension, error) marshalBasicConstraints(bool isCA, nint maxPathLen, bool maxPathLenZero) {
    var ext = new pkix.Extension(Id: oidExtensionBasicConstraints, Critical: true);
    // Leaving MaxPathLen as zero indicates that no maximum path
    // length is desired, unless MaxPathLenZero is set. A value of
    // -1 causes encoding/asn1 to omit the value as desired.
    if (maxPathLen == 0 && !maxPathLenZero) {
        maxPathLen = -1;
    }
    error err = default!;
    (ext.Value, err) = asn1.Marshal(new basicConstraints(isCA, maxPathLen));
    return (ext, err);
}

internal static (pkix.Extension, error) marshalCertificatePolicies(slice<OID> policies, slice<asn1.ObjectIdentifier> policyIdentifiers) {
    var ext = new pkix.Extension(Id: oidExtensionCertificatePolicies);
    var b = cryptobyte.NewBuilder(new slice<byte>(0, 128));
    b.AddASN1(cryptobyte_asn1.SEQUENCE, 
    var policiesʗ1 = policies;
    var policyIdentifiersʗ1 = policyIdentifiers;
    (ж<cryptobyte.Builder> child) => {
        if (x509usepolicies.Value() == "1"u8){
            x509usepolicies.IncNonDefault();
            ref var v = ref heap(new OID(), out var Ꮡv);

            foreach (var (_, v) in policiesʗ1) {
                child.AddASN1(cryptobyte_asn1.SEQUENCE, 
                var vʗ1 = v;
                (ж<cryptobyte.Builder> child) => {
                    childΔ1.AddASN1(cryptobyte_asn1.OBJECT_IDENTIFIER, 
                    var vʗ2 = v;
                    (ж<cryptobyte.Builder> child) => {
                        if (len(vʗ2.der) == 0) {
                            childΔ2.SetError(errors.New("invalid policy object identifier"u8));
                            return;
                        }
                        childΔ2.AddBytes(vʗ2.der);
                    });
                });
            }
        } else {
            foreach (var (_, v) in policyIdentifiers) {
                child.AddASN1(cryptobyte_asn1.SEQUENCE, 
                var vʗ7 = v;
                (ж<cryptobyte.Builder> child) => {
                    childΔ3.AddASN1ObjectIdentifier(vʗ7);
                });
            }
        }
    });
    error err = default!;
    (ext.Value, err) = b.Bytes();
    return (ext, err);
}

internal static (slice<pkix.Extension>, error) buildCSRExtensions(ж<CertificateRequest> Ꮡtemplate) {
    ref var template = ref Ꮡtemplate.val;

    slice<pkix.Extension> ret = default!;
    if ((len(template.DNSNames) > 0 || len(template.EmailAddresses) > 0 || len(template.IPAddresses) > 0 || len(template.URIs) > 0) && !oidInExtensions(oidExtensionSubjectAltName, template.ExtraExtensions)) {
        (sanBytes, err) = marshalSANs(template.DNSNames, template.EmailAddresses, template.IPAddresses, template.URIs);
        if (err != default!) {
            return (default!, err);
        }
        ret = append(ret, new pkix.Extension(
            Id: oidExtensionSubjectAltName,
            Value: sanBytes
        ));
    }
    return (append(ret, template.ExtraExtensions.ꓸꓸꓸ), default!);
}

internal static (slice<byte>, error) subjectBytes(ж<Certificate> Ꮡcert) {
    ref var cert = ref Ꮡcert.val;

    if (len(cert.RawSubject) > 0) {
        return (cert.RawSubject, default!);
    }
    return asn1.Marshal(cert.Subject.ToRDNSequence());
}

// signingParamsForKey returns the signature algorithm and its Algorithm
// Identifier to use for signing, based on the key type. If sigAlgo is not zero
// then it overrides the default.
internal static (SignatureAlgorithm, pkix.AlgorithmIdentifier, error) signingParamsForKey(crypto.Signer key, SignatureAlgorithm sigAlgo) {
    pkix.AlgorithmIdentifier ai = default!;
    PublicKeyAlgorithm pubType = default!;
    SignatureAlgorithm defaultAlgo = default!;
    switch (key.Public().type()) {
    case ж<rsa.PublicKey> pub: {
        pubType = RSA;
        defaultAlgo = SHA256WithRSA;
        break;
    }
    case ж<ecdsa.PublicKey> pub: {
        pubType = ECDSA;
        var exprᴛ1 = (~pub).Curve;
        if (exprᴛ1 == elliptic.P224() || exprᴛ1 == elliptic.P256()) {
            defaultAlgo = ECDSAWithSHA256;
        }
        else if (exprᴛ1 == elliptic.P384()) {
            defaultAlgo = ECDSAWithSHA384;
        }
        else if (exprᴛ1 == elliptic.P521()) {
            defaultAlgo = ECDSAWithSHA512;
        }
        else { /* default: */
            return (0, ai, errors.New("x509: unsupported elliptic curve"u8));
        }

        break;
    }
    case ed25519.PublicKey pub: {
        pubType = Ed25519;
        defaultAlgo = PureEd25519;
        break;
    }
    default: {
        var pub = key.Public().type();
        return (0, ai, errors.New("x509: only RSA, ECDSA and Ed25519 keys supported"u8));
    }}
    if (sigAlgo == 0) {
        sigAlgo = defaultAlgo;
    }
    foreach (var (_, details) in signatureAlgorithmDetails) {
        if (details.algo == sigAlgo) {
            if (details.pubKeyAlgo != pubType) {
                return (0, ai, errors.New("x509: requested SignatureAlgorithm does not match private key type"u8));
            }
            if (details.hash == crypto.MD5) {
                return (0, ai, errors.New("x509: signing with MD5 is not supported"u8));
            }
            return (sigAlgo, new pkix.AlgorithmIdentifier(
                Algorithm: details.oid,
                Parameters: details.@params
            ), default!);
        }
    }
    return (0, ai, errors.New("x509: unknown SignatureAlgorithm"u8));
}

internal static (slice<byte>, error) signTBS(slice<byte> tbs, crypto.Signer key, SignatureAlgorithm sigAlg, io.Reader rand) {
    var signed = tbs;
    ref var hashFunc = ref heap<crypto_package.Hash>(out var ᏑhashFunc);
    hashFunc = sigAlg.hashFunc();
    if (hashFunc != 0) {
        var h = hashFunc.New();
        h.Write(signed);
        signed = h.Sum(default!);
    }
    crypto.SignerOpts signerOpts = hashFunc;
    if (sigAlg.isRSAPSS()) {
        ᏑsignerOpts = new rsa.PSSOptions(
            SaltLength: rsa.PSSSaltLengthEqualsHash,
            Hash: hashFunc
        ); signerOpts = ref ᏑsignerOpts.val;
    }
    (signature, err) = key.Sign(rand, signed, signerOpts);
    if (err != default!) {
        return (default!, err);
    }
    // Check the signature to ensure the crypto.Signer behaved correctly.
    {
        var errΔ1 = checkSignature(sigAlg, tbs, signature, key.Public(), true); if (errΔ1 != default!) {
            return (default!, fmt.Errorf("x509: signature returned by signer is invalid: %w"u8, errΔ1));
        }
    }
    return (signature, default!);
}

// emptyASN1Subject is the ASN.1 DER encoding of an empty Subject, which is
// just an empty SEQUENCE.
internal static slice<byte> emptyASN1Subject = new byte[]{48, 0}.slice();

// Check that the signer's public key matches the private key, if available.
[GoType("dyn")] partial interface CreateCertificate_privateKey {
    bool Equal(crypto.PublicKey _);
}

// CreateCertificate creates a new X.509 v3 certificate based on a template.
// The following members of template are currently used:
//
//   - AuthorityKeyId
//   - BasicConstraintsValid
//   - CRLDistributionPoints
//   - DNSNames
//   - EmailAddresses
//   - ExcludedDNSDomains
//   - ExcludedEmailAddresses
//   - ExcludedIPRanges
//   - ExcludedURIDomains
//   - ExtKeyUsage
//   - ExtraExtensions
//   - IPAddresses
//   - IsCA
//   - IssuingCertificateURL
//   - KeyUsage
//   - MaxPathLen
//   - MaxPathLenZero
//   - NotAfter
//   - NotBefore
//   - OCSPServer
//   - PermittedDNSDomains
//   - PermittedDNSDomainsCritical
//   - PermittedEmailAddresses
//   - PermittedIPRanges
//   - PermittedURIDomains
//   - PolicyIdentifiers (see note below)
//   - Policies (see note below)
//   - SerialNumber
//   - SignatureAlgorithm
//   - Subject
//   - SubjectKeyId
//   - URIs
//   - UnknownExtKeyUsage
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
//
// The PolicyIdentifier and Policies fields are both used to marshal certificate
// policy OIDs. By default, only the PolicyIdentifier is marshaled, but if the
// GODEBUG setting "x509usepolicies" has the value "1", the Policies field will
// be marshaled instead of the PolicyIdentifier field. The Policies field can
// be used to marshal policy OIDs which have components that are larger than 31
// bits.
public static (slice<byte>, error) CreateCertificate(io.Reader rand, ж<Certificate> Ꮡtemplate, ж<Certificate> Ꮡparent, any pub, any priv) {
    ref var template = ref Ꮡtemplate.val;
    ref var parent = ref Ꮡparent.val;

    var (key, ok) = priv._<crypto.Signer>(ᐧ);
    if (!ok) {
        return (default!, errors.New("x509: certificate private key does not implement crypto.Signer"u8));
    }
    if (template.SerialNumber == nil) {
        return (default!, errors.New("x509: no SerialNumber given"u8));
    }
    // RFC 5280 Section 4.1.2.2: serial number must positive
    //
    // We _should_ also restrict serials to <= 20 octets, but it turns out a lot of people
    // get this wrong, in part because the encoding can itself alter the length of the
    // serial. For now we accept these non-conformant serials.
    if (template.SerialNumber.Sign() == -1) {
        return (default!, errors.New("x509: serial number must be positive"u8));
    }
    if (template.BasicConstraintsValid && !template.IsCA && template.MaxPathLen != -1 && (template.MaxPathLen != 0 || template.MaxPathLenZero)) {
        return (default!, errors.New("x509: only CAs are allowed to specify MaxPathLen"u8));
    }
    var (signatureAlgorithm, algorithmIdentifier, err) = signingParamsForKey(key, template.SignatureAlgorithm);
    if (err != default!) {
        return (default!, err);
    }
    var (publicKeyBytes, publicKeyAlgorithm, err) = marshalPublicKey(pub);
    if (err != default!) {
        return (default!, err);
    }
    if (getPublicKeyAlgorithmFromOID(publicKeyAlgorithm.Algorithm) == UnknownPublicKeyAlgorithm) {
        return (default!, fmt.Errorf("x509: unsupported public key type: %T"u8, pub));
    }
    (asn1Issuer, err) = subjectBytes(Ꮡparent);
    if (err != default!) {
        return (default!, err);
    }
    (asn1Subject, err) = subjectBytes(Ꮡtemplate);
    if (err != default!) {
        return (default!, err);
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
    {
        var (privPub, okΔ1) = key.Public()._<privateKey>(ᐧ); if (!okΔ1){
            return (default!, errors.New("x509: internal error: supported public key does not implement Equal"u8));
        } else 
        if (parent.PublicKey != default! && !privPub.Equal(parent.PublicKey)) {
            return (default!, errors.New("x509: provided PrivateKey doesn't match parent's PublicKey"u8));
        }
    }
    (extensions, err) = buildCertExtensions(Ꮡtemplate, bytes.Equal(asn1Subject, emptyASN1Subject), authorityKeyId, subjectKeyId);
    if (err != default!) {
        return (default!, err);
    }
    var encodedPublicKey = new asn1.BitString(BitLength: len(publicKeyBytes) * 8, Bytes: publicKeyBytes);
    var c = new tbsCertificate(
        Version: 2,
        SerialNumber: template.SerialNumber,
        SignatureAlgorithm: algorithmIdentifier,
        Issuer: new asn1.RawValue(FullBytes: asn1Issuer),
        Validity: new validity(template.NotBefore.UTC(), template.NotAfter.UTC()),
        Subject: new asn1.RawValue(FullBytes: asn1Subject),
        PublicKey: new publicKeyInfo(default!, publicKeyAlgorithm, encodedPublicKey),
        Extensions: extensions
    );
    (tbsCertContents, err) = asn1.Marshal(c);
    if (err != default!) {
        return (default!, err);
    }
    c.Raw = tbsCertContents;
    (signature, err) = signTBS(tbsCertContents, key, signatureAlgorithm, rand);
    if (err != default!) {
        return (default!, err);
    }
    return asn1.Marshal(new certificate(
        TBSCertificate: c,
        SignatureAlgorithm: algorithmIdentifier,
        SignatureValue: new asn1.BitString(Bytes: signature, BitLength: len(signature) * 8)
    ));
}

// pemCRLPrefix is the magic string that indicates that we have a PEM encoded
// CRL.
internal static slice<byte> pemCRLPrefix = slice<byte>("-----BEGIN X509 CRL");

// pemType is the type of a PEM encoded CRL.
internal static @string pemType = "X509 CRL"u8;

// ParseCRL parses a CRL from the given bytes. It's often the case that PEM
// encoded CRLs will appear where they should be DER encoded, so this function
// will transparently handle PEM encoding as long as there isn't any leading
// garbage.
//
// Deprecated: Use [ParseRevocationList] instead.
public static (ж<pkix.CertificateList>, error) ParseCRL(slice<byte> crlBytes) {
    if (bytes.HasPrefix(crlBytes, pemCRLPrefix)) {
        (block, _) = pem.Decode(crlBytes);
        if (block != nil && (~block).Type == pemType) {
            crlBytes = block.val.Bytes;
        }
    }
    return ParseDERCRL(crlBytes);
}

// ParseDERCRL parses a DER encoded CRL from the given bytes.
//
// Deprecated: Use [ParseRevocationList] instead.
public static (ж<pkix.CertificateList>, error) ParseDERCRL(slice<byte> derBytes) {
    var certList = @new<pkix.CertificateList>();
    {
        (rest, err) = asn1.Unmarshal(derBytes, certList); if (err != default!){
            return (default!, err);
        } else 
        if (len(rest) != 0) {
            return (default!, errors.New("x509: trailing data after CRL"u8));
        }
    }
    return (certList, default!);
}

// CreateCRL returns a DER encoded CRL, signed by this Certificate, that
// contains the given list of revoked certificates.
//
// Deprecated: this method does not generate an RFC 5280 conformant X.509 v2 CRL.
// To generate a standards compliant CRL, use [CreateRevocationList] instead.
[GoRecv] public static (slice<byte> crlBytes, error err) CreateCRL(this ref Certificate c, io.Reader rand, any priv, slice<pkix.RevokedCertificate> revokedCerts, time.Time now, time.Time expiry) {
    slice<byte> crlBytes = default!;
    error err = default!;

    var (key, ok) = priv._<crypto.Signer>(ᐧ);
    if (!ok) {
        return (default!, errors.New("x509: certificate private key does not implement crypto.Signer"u8));
    }
    var (signatureAlgorithm, algorithmIdentifier, err) = signingParamsForKey(key, 0);
    if (err != default!) {
        return (default!, err);
    }
    // Force revocation times to UTC per RFC 5280.
    var revokedCertsUTC = new slice<pkix.RevokedCertificate>(len(revokedCerts));
    foreach (var (i, rc) in revokedCerts) {
        rc.RevocationTime = rc.RevocationTime.UTC();
        revokedCertsUTC[i] = rc;
    }
    var tbsCertList = new pkix.TBSCertificateList(
        Version: 1,
        Signature: algorithmIdentifier,
        Issuer: c.Subject.ToRDNSequence(),
        ThisUpdate: now.UTC(),
        NextUpdate: expiry.UTC(),
        RevokedCertificates: revokedCertsUTC
    );
    // Authority Key Id
    if (len(c.SubjectKeyId) > 0) {
        pkix.Extension aki = default!;
        aki.Id = oidExtensionAuthorityKeyId;
        (aki.Value, err) = asn1.Marshal(new authKeyId(Id: c.SubjectKeyId));
        if (err != default!) {
            return (default!, err);
        }
        tbsCertList.Extensions = append(tbsCertList.Extensions, aki);
    }
    (tbsCertListContents, err) = asn1.Marshal(tbsCertList);
    if (err != default!) {
        return (default!, err);
    }
    tbsCertList.Raw = tbsCertListContents;
    (signature, err) = signTBS(tbsCertListContents, key, signatureAlgorithm, rand);
    if (err != default!) {
        return (default!, err);
    }
    return asn1.Marshal(new pkix.CertificateList(
        TBSCertList: tbsCertList,
        SignatureAlgorithm: algorithmIdentifier,
        SignatureValue: new asn1.BitString(Bytes: signature, BitLength: len(signature) * 8)
    ));
}

// CertificateRequest represents a PKCS #10, certificate signature request.
[GoType] partial struct CertificateRequest {
    public slice<byte> Raw; // Complete ASN.1 DER content (CSR, signature algorithm and signature).
    public slice<byte> RawTBSCertificateRequest; // Certificate request info part of raw ASN.1 DER content.
    public slice<byte> RawSubjectPublicKeyInfo; // DER encoded SubjectPublicKeyInfo.
    public slice<byte> RawSubject; // DER encoded Subject.
    public nint Version;
    public slice<byte> Signature;
    public SignatureAlgorithm SignatureAlgorithm;
    public PublicKeyAlgorithm PublicKeyAlgorithm;
    public any PublicKey;
    public crypto.x509.pkix_package.Name Subject;
    // Attributes contains the CSR attributes that can parse as
    // pkix.AttributeTypeAndValueSET.
    //
    // Deprecated: Use Extensions and ExtraExtensions instead for parsing and
    // generating the requestedExtensions attribute.
    public pkix.AttributeTypeAndValueSET Attributes;
    // Extensions contains all requested extensions, in raw form. When parsing
    // CSRs, this can be used to extract extensions that are not parsed by this
    // package.
    public pkix.Extension Extensions;
    // ExtraExtensions contains extensions to be copied, raw, into any CSR
    // marshaled by CreateCertificateRequest. Values override any extensions
    // that would otherwise be produced based on the other fields but are
    // overridden by any extensions specified in Attributes.
    //
    // The ExtraExtensions field is not populated by ParseCertificateRequest,
    // see Extensions instead.
    public pkix.Extension ExtraExtensions;
    // Subject Alternate Name values.
    public slice<@string> DNSNames;
    public slice<@string> EmailAddresses;
    public slice<net.IP> IPAddresses;
    public url.URL URIs;
}

// These structures reflect the ASN.1 structure of X.509 certificate
// signature requests (see RFC 2986):
[GoType] partial struct tbsCertificateRequest {
    public encoding.asn1_package.RawContent Raw;
    public nint Version;
    public encoding.asn1_package.RawValue Subject;
    public publicKeyInfo PublicKey;
    [GoTag(@"asn1:""tag:0""")]
    public asn1.RawValue RawAttributes;
}

[GoType] partial struct certificateRequest {
    public encoding.asn1_package.RawContent Raw;
    public tbsCertificateRequest TBSCSR;
    public crypto.x509.pkix_package.AlgorithmIdentifier SignatureAlgorithm;
    public encoding.asn1_package.BitString SignatureValue;
}

// oidExtensionRequest is a PKCS #9 OBJECT IDENTIFIER that indicates requested
// extensions in a CSR.
internal static asn1.ObjectIdentifier oidExtensionRequest = new asn1.ObjectIdentifier{1, 2, 840, 113549, 1, 9, 14};

// newRawAttributes converts AttributeTypeAndValueSETs from a template
// CertificateRequest's Attributes into tbsCertificateRequest RawAttributes.
internal static (slice<asn1.RawValue>, error) newRawAttributes(slice<pkix.AttributeTypeAndValueSET> attributes) {
    slice<asn1.RawValue> rawAttributes = default!;
    (b, err) = asn1.Marshal(attributes);
    if (err != default!) {
        return (default!, err);
    }
    (rest, err) = asn1.Unmarshal(b, Ꮡ(rawAttributes));
    if (err != default!) {
        return (default!, err);
    }
    if (len(rest) != 0) {
        return (default!, errors.New("x509: failed to unmarshal raw CSR Attributes"u8));
    }
    return (rawAttributes, default!);
}

// parseRawAttributes Unmarshals RawAttributes into AttributeTypeAndValueSETs.
internal static slice<pkix.AttributeTypeAndValueSET> parseRawAttributes(slice<asn1.RawValue> rawAttributes) {
    slice<pkix.AttributeTypeAndValueSET> attributes = default!;
    foreach (var (_, rawAttr) in rawAttributes) {
        ref var attr = ref heap(new crypto.x509.pkix_package.AttributeTypeAndValueSET(), out var Ꮡattr);
        (rest, err) = asn1.Unmarshal(rawAttr.FullBytes, Ꮡattr);
        // Ignore attributes that don't parse into pkix.AttributeTypeAndValueSET
        // (i.e.: challengePassword or unstructuredName).
        if (err == default! && len(rest) == 0) {
            attributes = append(attributes, attr);
        }
    }
    return attributes;
}

// pkcs10Attribute reflects the Attribute structure from RFC 2986, Section 4.1.
[GoType("dyn")] partial struct parseCSRExtensions_pkcs10Attribute {
    public encoding.asn1_package.ObjectIdentifier Id;
    [GoTag(@"asn1:""set""")]
    public asn1.RawValue Values;
}

// parseCSRExtensions parses the attributes from a CSR and extracts any
// requested extensions.
internal static (slice<pkix.Extension>, error) parseCSRExtensions(slice<asn1.RawValue> rawAttributes) {
    slice<pkix.Extension> ret = default!;
    var requestedExts = new map<@string, bool>();
    foreach (var (_, rawAttr) in rawAttributes) {
        ref var attr = ref heap(new parseCSRExtensions_pkcs10Attribute(), out var Ꮡattr);
        {
            (rest, err) = asn1.Unmarshal(rawAttr.FullBytes, Ꮡattr); if (err != default! || len(rest) != 0 || len(attr.Values) == 0) {
                // Ignore attributes that don't parse.
                continue;
            }
        }
        if (!attr.Id.Equal(oidExtensionRequest)) {
            continue;
        }
        slice<pkix.Extension> extensions = default!;
        {
            (_, err) = asn1.Unmarshal(attr.Values[0].FullBytes, Ꮡ(extensions)); if (err != default!) {
                return (default!, err);
            }
        }
        foreach (var (_, ext) in extensions) {
            @string oidStr = ext.Id.String();
            if (requestedExts[oidStr]) {
                return (default!, errors.New("x509: certificate request contains duplicate requested extensions"u8));
            }
            requestedExts[oidStr] = true;
        }
        ret = append(ret, extensions.ꓸꓸꓸ);
    }
    return (ret, default!);
}

[GoType("dyn")] partial struct CreateCertificateRequest_attr {
    public encoding.asn1_package.ObjectIdentifier Type;
    [GoTag(@"asn1:""set""")]
    public pkix.Extension Value;
}

// CreateCertificateRequest creates a new certificate request based on a
// template. The following members of template are used:
//
//   - SignatureAlgorithm
//   - Subject
//   - DNSNames
//   - EmailAddresses
//   - IPAddresses
//   - URIs
//   - ExtraExtensions
//   - Attributes (deprecated)
//
// priv is the private key to sign the CSR with, and the corresponding public
// key will be included in the CSR. It must implement crypto.Signer and its
// Public() method must return a *rsa.PublicKey or a *ecdsa.PublicKey or a
// ed25519.PublicKey. (A *rsa.PrivateKey, *ecdsa.PrivateKey or
// ed25519.PrivateKey satisfies this.)
//
// The returned slice is the certificate request in DER encoding.
public static (slice<byte> csr, error err) CreateCertificateRequest(io.Reader rand, ж<CertificateRequest> Ꮡtemplate, any priv) {
    slice<byte> csr = default!;
    error err = default!;

    ref var template = ref Ꮡtemplate.val;
    var (key, ok) = priv._<crypto.Signer>(ᐧ);
    if (!ok) {
        return (default!, errors.New("x509: certificate private key does not implement crypto.Signer"u8));
    }
    var (signatureAlgorithm, algorithmIdentifier, err) = signingParamsForKey(key, template.SignatureAlgorithm);
    if (err != default!) {
        return (default!, err);
    }
    slice<byte> publicKeyBytes = default!;
    pkix.AlgorithmIdentifier publicKeyAlgorithm = default!;
    (publicKeyBytes, publicKeyAlgorithm, err) = marshalPublicKey(key.Public());
    if (err != default!) {
        return (default!, err);
    }
    (extensions, err) = buildCSRExtensions(Ꮡtemplate);
    if (err != default!) {
        return (default!, err);
    }
    // Make a copy of template.Attributes because we may alter it below.
    var attributes = new slice<pkix.AttributeTypeAndValueSET>(0, len(template.Attributes));
    foreach (var (_, attr) in template.Attributes) {
        var values = new slice<pkix.AttributeTypeAndValue>(len(attr.Value));
        copy(values, attr.Value);
        attributes = append(attributes, new pkix.AttributeTypeAndValueSET(
            Type: attr.Type,
            Value: values
        ));
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
            var specifiedExtensions = new map<@string, bool>();
            foreach (var (_, atvs) in atvSet.Value) {
                foreach (var (_, atv) in atvs) {
                    specifiedExtensions[atv.Type.String()] = true;
                }
            }
            var newValue = new slice<pkix.AttributeTypeAndValue>(0, len(atvSet.Value[0]) + len(extensions));
            newValue = append(newValue, atvSet.Value[0].ꓸꓸꓸ);
            foreach (var (_, e) in extensions) {
                if (specifiedExtensions[e.Id.String()]) {
                    // Attributes already contained a value for
                    // this extension and it takes priority.
                    continue;
                }
                newValue = append(newValue, new pkix.AttributeTypeAndValue( // There is no place for the critical
 // flag in an AttributeTypeAndValue.

                    Type: e.Id,
                    Value: e.Value
                ));
            }
            atvSet.Value[0] = newValue;
            extensionsAppended = true;
            break;
        }
    }
    (rawAttributes, err) = newRawAttributes(attributes);
    if (err != default!) {
        return (default!, err);
    }
    // If not included in attributes, add a new attribute for the
    // extensions.
    if (len(extensions) > 0 && !extensionsAppended) {
        var attr = new CreateCertificateRequest_attr(
            Type: oidExtensionRequest,
            Value: new pkix.Extension[]{extensions}.slice()
        );
        (b, errΔ1) = asn1.Marshal(attr);
        if (errΔ1 != default!) {
            return (default!, errors.New("x509: failed to serialise extensions attribute: "u8 + errΔ1.Error()));
        }
        ref var rawValue = ref heap(new encoding.asn1_package.RawValue(), out var ᏑrawValue);
        {
            (_, errΔ2) = asn1.Unmarshal(b, ᏑrawValue); if (errΔ2 != default!) {
                return (default!, errΔ2);
            }
        }
        rawAttributes = append(rawAttributes, rawValue);
    }
    var asn1Subject = template.RawSubject;
    if (len(asn1Subject) == 0) {
        (asn1Subject, err) = asn1.Marshal(template.Subject.ToRDNSequence());
        if (err != default!) {
            return (default!, err);
        }
    }
    var tbsCSR = new tbsCertificateRequest(
        Version: 0, // PKCS #10, RFC 2986

        Subject: new asn1.RawValue(FullBytes: asn1Subject),
        PublicKey: new publicKeyInfo(
            Algorithm: publicKeyAlgorithm,
            PublicKey: new asn1.BitString(
                Bytes: publicKeyBytes,
                BitLength: len(publicKeyBytes) * 8
            )
        ),
        RawAttributes: rawAttributes
    );
    (tbsCSRContents, err) = asn1.Marshal(tbsCSR);
    if (err != default!) {
        return (default!, err);
    }
    tbsCSR.Raw = tbsCSRContents;
    (signature, err) = signTBS(tbsCSRContents, key, signatureAlgorithm, rand);
    if (err != default!) {
        return (default!, err);
    }
    return asn1.Marshal(new certificateRequest(
        TBSCSR: tbsCSR,
        SignatureAlgorithm: algorithmIdentifier,
        SignatureValue: new asn1.BitString(Bytes: signature, BitLength: len(signature) * 8)
    ));
}

// ParseCertificateRequest parses a single certificate request from the
// given ASN.1 DER data.
public static (ж<CertificateRequest>, error) ParseCertificateRequest(slice<byte> asn1Data) {
    ref var csr = ref heap(new certificateRequest(), out var Ꮡcsr);
    (rest, err) = asn1.Unmarshal(asn1Data, Ꮡcsr);
    if (err != default!){
        return (default!, err);
    } else 
    if (len(rest) != 0) {
        return (default!, new asn1.SyntaxError(Msg: "trailing data"u8));
    }
    return parseCertificateRequest(Ꮡcsr);
}

internal static (ж<CertificateRequest>, error) parseCertificateRequest(ж<certificateRequest> Ꮡin) {
    ref var @in = ref Ꮡin.val;

    var @out = Ꮡ(new CertificateRequest(
        Raw: @in.Raw,
        RawTBSCertificateRequest: @in.TBSCSR.Raw,
        RawSubjectPublicKeyInfo: @in.TBSCSR.PublicKey.Raw,
        RawSubject: @in.TBSCSR.Subject.FullBytes,
        Signature: @in.SignatureValue.RightAlign(),
        SignatureAlgorithm: getSignatureAlgorithmFromAI(@in.SignatureAlgorithm),
        PublicKeyAlgorithm: getPublicKeyAlgorithmFromOID(@in.TBSCSR.PublicKey.Algorithm.Algorithm),
        Version: @in.TBSCSR.Version,
        Attributes: parseRawAttributes(@in.TBSCSR.RawAttributes)
    ));
    error err = default!;
    if ((~@out).PublicKeyAlgorithm != UnknownPublicKeyAlgorithm) {
        (@out.val.PublicKey, err) = parsePublicKey(Ꮡ@in.TBSCSR.of(tbsCertificateRequest.ᏑPublicKey));
        if (err != default!) {
            return (default!, err);
        }
    }
    pkix.RDNSequence subject = default!;
    {
        (rest, errΔ1) = asn1.Unmarshal(@in.TBSCSR.Subject.FullBytes, Ꮡ(subject)); if (errΔ1 != default!){
            return (default!, errΔ1);
        } else 
        if (len(rest) != 0) {
            return (default!, errors.New("x509: trailing data after X.509 Subject"u8));
        }
    }
    (~@out).Subject.FillFromRDNSequence(Ꮡ(subject));
    {
        (@out.val.Extensions, err) = parseCSRExtensions(@in.TBSCSR.RawAttributes); if (err != default!) {
            return (default!, err);
        }
    }
    foreach (var (_, extension) in (~@out).Extensions) {
        switch (ᐧ) {
        case {} when extension.Id.Equal(oidExtensionSubjectAltName): {
            (@out.val.DNSNames, @out.val.EmailAddresses, @out.val.IPAddresses, @out.val.URIs, err) = parseSANExtension(extension.Value);
            if (err != default!) {
                return (default!, err);
            }
            break;
        }}

    }
    return (@out, default!);
}

// CheckSignature reports whether the signature on c is valid.
[GoRecv] public static error CheckSignature(this ref CertificateRequest c) {
    return checkSignature(c.SignatureAlgorithm, c.RawTBSCertificateRequest, c.Signature, c.PublicKey, true);
}

// RevocationListEntry represents an entry in the revokedCertificates
// sequence of a CRL.
[GoType] partial struct RevocationListEntry {
    // Raw contains the raw bytes of the revokedCertificates entry. It is set when
    // parsing a CRL; it is ignored when generating a CRL.
    public slice<byte> Raw;
    // SerialNumber represents the serial number of a revoked certificate. It is
    // both used when creating a CRL and populated when parsing a CRL. It must not
    // be nil.
    public ж<math.big_package.ΔInt> SerialNumber;
    // RevocationTime represents the time at which the certificate was revoked. It
    // is both used when creating a CRL and populated when parsing a CRL. It must
    // not be the zero time.
    public time_package.Time RevocationTime;
    // ReasonCode represents the reason for revocation, using the integer enum
    // values specified in RFC 5280 Section 5.3.1. When creating a CRL, the zero
    // value will result in the reasonCode extension being omitted. When parsing a
    // CRL, the zero value may represent either the reasonCode extension being
    // absent (which implies the default revocation reason of 0/Unspecified), or
    // it may represent the reasonCode extension being present and explicitly
    // containing a value of 0/Unspecified (which should not happen according to
    // the DER encoding rules, but can and does happen anyway).
    public nint ReasonCode;
    // Extensions contains raw X.509 extensions. When parsing CRL entries,
    // this can be used to extract non-critical extensions that are not
    // parsed by this package. When marshaling CRL entries, the Extensions
    // field is ignored, see ExtraExtensions.
    public pkix.Extension Extensions;
    // ExtraExtensions contains extensions to be copied, raw, into any
    // marshaled CRL entries. Values override any extensions that would
    // otherwise be produced based on the other fields. The ExtraExtensions
    // field is not populated when parsing CRL entries, see Extensions.
    public pkix.Extension ExtraExtensions;
}

// RevocationList represents a [Certificate] Revocation List (CRL) as specified
// by RFC 5280.
[GoType] partial struct RevocationList {
    // Raw contains the complete ASN.1 DER content of the CRL (tbsCertList,
    // signatureAlgorithm, and signatureValue.)
    public slice<byte> Raw;
    // RawTBSRevocationList contains just the tbsCertList portion of the ASN.1
    // DER.
    public slice<byte> RawTBSRevocationList;
    // RawIssuer contains the DER encoded Issuer.
    public slice<byte> RawIssuer;
    // Issuer contains the DN of the issuing certificate.
    public crypto.x509.pkix_package.Name Issuer;
    // AuthorityKeyId is used to identify the public key associated with the
    // issuing certificate. It is populated from the authorityKeyIdentifier
    // extension when parsing a CRL. It is ignored when creating a CRL; the
    // extension is populated from the issuing certificate itself.
    public slice<byte> AuthorityKeyId;
    public slice<byte> Signature;
    // SignatureAlgorithm is used to determine the signature algorithm to be
    // used when signing the CRL. If 0 the default algorithm for the signing
    // key will be used.
    public SignatureAlgorithm SignatureAlgorithm;
    // RevokedCertificateEntries represents the revokedCertificates sequence in
    // the CRL. It is used when creating a CRL and also populated when parsing a
    // CRL. When creating a CRL, it may be empty or nil, in which case the
    // revokedCertificates ASN.1 sequence will be omitted from the CRL entirely.
    public slice<RevocationListEntry> RevokedCertificateEntries;
    // RevokedCertificates is used to populate the revokedCertificates
    // sequence in the CRL if RevokedCertificateEntries is empty. It may be empty
    // or nil, in which case an empty CRL will be created.
    //
    // Deprecated: Use RevokedCertificateEntries instead.
    public pkix.RevokedCertificate RevokedCertificates;
    // Number is used to populate the X.509 v2 cRLNumber extension in the CRL,
    // which should be a monotonically increasing sequence number for a given
    // CRL scope and CRL issuer. It is also populated from the cRLNumber
    // extension when parsing a CRL.
    public ж<math.big_package.ΔInt> Number;
    // ThisUpdate is used to populate the thisUpdate field in the CRL, which
    // indicates the issuance date of the CRL.
    public time_package.Time ThisUpdate;
    // NextUpdate is used to populate the nextUpdate field in the CRL, which
    // indicates the date by which the next CRL will be issued. NextUpdate
    // must be greater than ThisUpdate.
    public time_package.Time NextUpdate;
    // Extensions contains raw X.509 extensions. When creating a CRL,
    // the Extensions field is ignored, see ExtraExtensions.
    public pkix.Extension Extensions;
    // ExtraExtensions contains any additional extensions to add directly to
    // the CRL.
    public pkix.Extension ExtraExtensions;
}

// These structures reflect the ASN.1 structure of X.509 CRLs better than
// the existing crypto/x509/pkix variants do. These mirror the existing
// certificate structs in this file.
//
// Notably, we include issuer as an asn1.RawValue, mirroring the behavior of
// tbsCertificate and allowing raw (unparsed) subjects to be passed cleanly.
[GoType] partial struct certificateList {
    public tbsCertificateList TBSCertList;
    public crypto.x509.pkix_package.AlgorithmIdentifier SignatureAlgorithm;
    public encoding.asn1_package.BitString SignatureValue;
}

[GoType] partial struct tbsCertificateList {
    public encoding.asn1_package.RawContent Raw;
    [GoTag(@"asn1:""optional,default:0""")]
    public nint Version;
    public crypto.x509.pkix_package.AlgorithmIdentifier Signature;
    public encoding.asn1_package.RawValue Issuer;
    public time_package.Time ThisUpdate;
    [GoTag(@"asn1:""optional""")]
    public time_package.Time NextUpdate;
    [GoTag(@"asn1:""optional""")]
    public pkix.RevokedCertificate RevokedCertificates;
    [GoTag(@"asn1:""tag:0,optional,explicit""")]
    public pkix.Extension Extensions;
}

// CreateRevocationList creates a new X.509 v2 [Certificate] Revocation List,
// according to RFC 5280, based on template.
//
// The CRL is signed by priv which should be the private key associated with
// the public key in the issuer certificate.
//
// The issuer may not be nil, and the crlSign bit must be set in [KeyUsage] in
// order to use it as a CRL issuer.
//
// The issuer distinguished name CRL field and authority key identifier
// extension are populated using the issuer certificate. issuer must have
// SubjectKeyId set.
public static (slice<byte>, error) CreateRevocationList(io.Reader rand, ж<RevocationList> Ꮡtemplate, ж<Certificate> Ꮡissuer, crypto.Signer priv) {
    ref var template = ref Ꮡtemplate.val;
    ref var issuer = ref Ꮡissuer.val;

    if (template == nil) {
        return (default!, errors.New("x509: template can not be nil"u8));
    }
    if (issuer == nil) {
        return (default!, errors.New("x509: issuer can not be nil"u8));
    }
    if (((KeyUsage)(issuer.KeyUsage & KeyUsageCRLSign)) == 0) {
        return (default!, errors.New("x509: issuer must have the crlSign key usage bit set"u8));
    }
    if (len(issuer.SubjectKeyId) == 0) {
        return (default!, errors.New("x509: issuer certificate doesn't contain a subject key identifier"u8));
    }
    if (template.NextUpdate.Before(template.ThisUpdate)) {
        return (default!, errors.New("x509: template.ThisUpdate is after template.NextUpdate"u8));
    }
    if (template.Number == nil) {
        return (default!, errors.New("x509: template contains nil Number field"u8));
    }
    var (signatureAlgorithm, algorithmIdentifier, err) = signingParamsForKey(priv, template.SignatureAlgorithm);
    if (err != default!) {
        return (default!, err);
    }
    slice<pkix.RevokedCertificate> revokedCerts = default!;
    // Only process the deprecated RevokedCertificates field if it is populated
    // and the new RevokedCertificateEntries field is not populated.
    if (len(template.RevokedCertificates) > 0 && len(template.RevokedCertificateEntries) == 0){
        // Force revocation times to UTC per RFC 5280.
        revokedCerts = new slice<pkix.RevokedCertificate>(len(template.RevokedCertificates));
        foreach (var (i, rc) in template.RevokedCertificates) {
            rc.RevocationTime = rc.RevocationTime.UTC();
            revokedCerts[i] = rc;
        }
    } else {
        // Convert the ReasonCode field to a proper extension, and force revocation
        // times to UTC per RFC 5280.
        revokedCerts = new slice<pkix.RevokedCertificate>(len(template.RevokedCertificateEntries));
        foreach (var (i, rce) in template.RevokedCertificateEntries) {
            if (rce.SerialNumber == nil) {
                return (default!, errors.New("x509: template contains entry with nil SerialNumber field"u8));
            }
            if (rce.RevocationTime.IsZero()) {
                return (default!, errors.New("x509: template contains entry with zero RevocationTime field"u8));
            }
            var rc = new pkix.RevokedCertificate(
                SerialNumber: rce.SerialNumber,
                RevocationTime: rce.RevocationTime.UTC()
            );
            // Copy over any extra extensions, except for a Reason Code extension,
            // because we'll synthesize that ourselves to ensure it is correct.
            var exts = new slice<pkix.Extension>(0, len(rce.ExtraExtensions));
            foreach (var (_, ext) in rce.ExtraExtensions) {
                if (ext.Id.Equal(oidExtensionReasonCode)) {
                    return (default!, errors.New("x509: template contains entry with ReasonCode ExtraExtension; use ReasonCode field instead"u8));
                }
                exts = append(exts, ext);
            }
            // Only add a reasonCode extension if the reason is non-zero, as per
            // RFC 5280 Section 5.3.1.
            if (rce.ReasonCode != 0) {
                (reasonBytes, errΔ1) = asn1.Marshal(((asn1.Enumerated)rce.ReasonCode));
                if (errΔ1 != default!) {
                    return (default!, errΔ1);
                }
                exts = append(exts, new pkix.Extension(
                    Id: oidExtensionReasonCode,
                    Value: reasonBytes
                ));
            }
            if (len(exts) > 0) {
                rc.Extensions = exts;
            }
            revokedCerts[i] = rc;
        }
    }
    (aki, err) = asn1.Marshal(new authKeyId(Id: issuer.SubjectKeyId));
    if (err != default!) {
        return (default!, err);
    }
    {
        var numBytes = template.Number.Bytes(); if (len(numBytes) > 20 || (len(numBytes) == 20 && (byte)(numBytes[0] & 128) != 0)) {
            return (default!, errors.New("x509: CRL number exceeds 20 octets"u8));
        }
    }
    (crlNum, err) = asn1.Marshal(template.Number);
    if (err != default!) {
        return (default!, err);
    }
    // Correctly use the issuer's subject sequence if one is specified.
    (issuerSubject, err) = subjectBytes(Ꮡissuer);
    if (err != default!) {
        return (default!, err);
    }
    var tbsCertList = new tbsCertificateList(
        Version: 1, // v2

        Signature: algorithmIdentifier,
        Issuer: new asn1.RawValue(FullBytes: issuerSubject),
        ThisUpdate: template.ThisUpdate.UTC(),
        NextUpdate: template.NextUpdate.UTC(),
        Extensions: new pkix.Extension[]{
            new(
                Id: oidExtensionAuthorityKeyId,
                Value: aki
            ),
            new(
                Id: oidExtensionCRLNumber,
                Value: crlNum
            )
        }.slice()
    );
    if (len(revokedCerts) > 0) {
        tbsCertList.RevokedCertificates = revokedCerts;
    }
    if (len(template.ExtraExtensions) > 0) {
        tbsCertList.Extensions = append(tbsCertList.Extensions, template.ExtraExtensions.ꓸꓸꓸ);
    }
    (tbsCertListContents, err) = asn1.Marshal(tbsCertList);
    if (err != default!) {
        return (default!, err);
    }
    // Optimization to only marshal this struct once, when signing and
    // then embedding in certificateList below.
    tbsCertList.Raw = tbsCertListContents;
    (signature, err) = signTBS(tbsCertListContents, priv, signatureAlgorithm, rand);
    if (err != default!) {
        return (default!, err);
    }
    return asn1.Marshal(new certificateList(
        TBSCertList: tbsCertList,
        SignatureAlgorithm: algorithmIdentifier,
        SignatureValue: new asn1.BitString(Bytes: signature, BitLength: len(signature) * 8)
    ));
}

// CheckSignatureFrom verifies that the signature on rl is a valid signature
// from issuer.
[GoRecv] public static error CheckSignatureFrom(this ref RevocationList rl, ж<Certificate> Ꮡparent) {
    ref var parent = ref Ꮡparent.val;

    if (parent.Version == 3 && !parent.BasicConstraintsValid || parent.BasicConstraintsValid && !parent.IsCA) {
        return new ConstraintViolationError(nil);
    }
    if (parent.KeyUsage != 0 && (KeyUsage)(parent.KeyUsage & KeyUsageCRLSign) == 0) {
        return new ConstraintViolationError(nil);
    }
    if (parent.PublicKeyAlgorithm == UnknownPublicKeyAlgorithm) {
        return ErrUnsupportedAlgorithm;
    }
    return parent.CheckSignature(rl.SignatureAlgorithm, rl.RawTBSRevocationList, rl.Signature);
}

} // end x509_package
