// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package tls -- go2cs converted at 2022 March 13 05:30:27 UTC
// import "crypto/tls" ==> using tls = go.crypto.tls_package
// Original source: C:\Program Files\Go\src\crypto\tls\auth.go
namespace go.crypto;

using bytes = bytes_package;
using crypto = crypto_package;
using ecdsa = crypto.ecdsa_package;
using ed25519 = crypto.ed25519_package;
using elliptic = crypto.elliptic_package;
using rsa = crypto.rsa_package;
using errors = errors_package;
using fmt = fmt_package;
using hash = hash_package;
using io = io_package;


// verifyHandshakeSignature verifies a signature against pre-hashed
// (if required) handshake contents.

public static partial class tls_package {

private static error verifyHandshakeSignature(byte sigType, crypto.PublicKey pubkey, crypto.Hash hashFunc, slice<byte> signed, slice<byte> sig) {

    if (sigType == signatureECDSA) 
        ptr<ecdsa.PublicKey> (pubKey, ok) = pubkey._<ptr<ecdsa.PublicKey>>();
        if (!ok) {
            return error.As(fmt.Errorf("expected an ECDSA public key, got %T", pubkey))!;
        }
        if (!ecdsa.VerifyASN1(pubKey, signed, sig)) {
            return error.As(errors.New("ECDSA verification failure"))!;
        }
    else if (sigType == signatureEd25519) 
        (pubKey, ok) = pubkey._<ed25519.PublicKey>();
        if (!ok) {
            return error.As(fmt.Errorf("expected an Ed25519 public key, got %T", pubkey))!;
        }
        if (!ed25519.Verify(pubKey, signed, sig)) {
            return error.As(errors.New("Ed25519 verification failure"))!;
        }
    else if (sigType == signaturePKCS1v15) 
        (pubKey, ok) = pubkey._<ptr<rsa.PublicKey>>();
        if (!ok) {
            return error.As(fmt.Errorf("expected an RSA public key, got %T", pubkey))!;
        }
        {
            var err__prev1 = err;

            var err = rsa.VerifyPKCS1v15(pubKey, hashFunc, signed, sig);

            if (err != null) {
                return error.As(err)!;
            }
            err = err__prev1;

        }
    else if (sigType == signatureRSAPSS) 
        (pubKey, ok) = pubkey._<ptr<rsa.PublicKey>>();
        if (!ok) {
            return error.As(fmt.Errorf("expected an RSA public key, got %T", pubkey))!;
        }
        ptr<rsa.PSSOptions> signOpts = addr(new rsa.PSSOptions(SaltLength:rsa.PSSSaltLengthEqualsHash));
        {
            var err__prev1 = err;

            err = rsa.VerifyPSS(pubKey, hashFunc, signed, sig, signOpts);

            if (err != null) {
                return error.As(err)!;
            }
            err = err__prev1;

        }
    else 
        return error.As(errors.New("internal error: unknown signature type"))!;
        return error.As(null!)!;
}

private static readonly @string serverSignatureContext = "TLS 1.3, server CertificateVerify\x00";
private static readonly @string clientSignatureContext = "TLS 1.3, client CertificateVerify\x00";

private static byte signaturePadding = new slice<byte>(new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 });

// signedMessage returns the pre-hashed (if necessary) message to be signed by
// certificate keys in TLS 1.3. See RFC 8446, Section 4.4.3.
private static slice<byte> signedMessage(crypto.Hash sigHash, @string context, hash.Hash transcript) {
    if (sigHash == directSigning) {
        ptr<bytes.Buffer> b = addr(new bytes.Buffer());
        b.Write(signaturePadding);
        io.WriteString(b, context);
        b.Write(transcript.Sum(null));
        return b.Bytes();
    }
    var h = sigHash.New();
    h.Write(signaturePadding);
    io.WriteString(h, context);
    h.Write(transcript.Sum(null));
    return h.Sum(null);
}

// typeAndHashFromSignatureScheme returns the corresponding signature type and
// crypto.Hash for a given TLS SignatureScheme.
private static (byte, crypto.Hash, error) typeAndHashFromSignatureScheme(SignatureScheme signatureAlgorithm) {
    byte sigType = default;
    crypto.Hash hash = default;
    error err = default!;


    if (signatureAlgorithm == PKCS1WithSHA1 || signatureAlgorithm == PKCS1WithSHA256 || signatureAlgorithm == PKCS1WithSHA384 || signatureAlgorithm == PKCS1WithSHA512) 
        sigType = signaturePKCS1v15;
    else if (signatureAlgorithm == PSSWithSHA256 || signatureAlgorithm == PSSWithSHA384 || signatureAlgorithm == PSSWithSHA512) 
        sigType = signatureRSAPSS;
    else if (signatureAlgorithm == ECDSAWithSHA1 || signatureAlgorithm == ECDSAWithP256AndSHA256 || signatureAlgorithm == ECDSAWithP384AndSHA384 || signatureAlgorithm == ECDSAWithP521AndSHA512) 
        sigType = signatureECDSA;
    else if (signatureAlgorithm == Ed25519) 
        sigType = signatureEd25519;
    else 
        return (0, 0, error.As(fmt.Errorf("unsupported signature algorithm: %v", signatureAlgorithm))!);
    
    if (signatureAlgorithm == PKCS1WithSHA1 || signatureAlgorithm == ECDSAWithSHA1) 
        hash = crypto.SHA1;
    else if (signatureAlgorithm == PKCS1WithSHA256 || signatureAlgorithm == PSSWithSHA256 || signatureAlgorithm == ECDSAWithP256AndSHA256) 
        hash = crypto.SHA256;
    else if (signatureAlgorithm == PKCS1WithSHA384 || signatureAlgorithm == PSSWithSHA384 || signatureAlgorithm == ECDSAWithP384AndSHA384) 
        hash = crypto.SHA384;
    else if (signatureAlgorithm == PKCS1WithSHA512 || signatureAlgorithm == PSSWithSHA512 || signatureAlgorithm == ECDSAWithP521AndSHA512) 
        hash = crypto.SHA512;
    else if (signatureAlgorithm == Ed25519) 
        hash = directSigning;
    else 
        return (0, 0, error.As(fmt.Errorf("unsupported signature algorithm: %v", signatureAlgorithm))!);
        return (sigType, hash, error.As(null!)!);
}

// legacyTypeAndHashFromPublicKey returns the fixed signature type and crypto.Hash for
// a given public key used with TLS 1.0 and 1.1, before the introduction of
// signature algorithm negotiation.
private static (byte, crypto.Hash, error) legacyTypeAndHashFromPublicKey(crypto.PublicKey pub) {
    byte sigType = default;
    crypto.Hash hash = default;
    error err = default!;

    switch (pub.type()) {
        case ptr<rsa.PublicKey> _:
            return (signaturePKCS1v15, crypto.MD5SHA1, error.As(null!)!);
            break;
        case ptr<ecdsa.PublicKey> _:
            return (signatureECDSA, crypto.SHA1, error.As(null!)!);
            break;
        case ed25519.PublicKey _:
            return (0, 0, error.As(fmt.Errorf("tls: Ed25519 public keys are not supported before TLS 1.2"))!);
            break;
        default:
        {
            return (0, 0, error.As(fmt.Errorf("tls: unsupported public key: %T", pub))!);
            break;
        }
    }
}



// signatureSchemesForCertificate returns the list of supported SignatureSchemes
// for a given certificate, based on the public key and the protocol version,
// and optionally filtered by its explicit SupportedSignatureAlgorithms.
//
// This function must be kept in sync with supportedSignatureAlgorithms.
private static slice<SignatureScheme> signatureSchemesForCertificate(ushort version, ptr<Certificate> _addr_cert) {
    ref Certificate cert = ref _addr_cert.val;

    crypto.Signer (priv, ok) = cert.PrivateKey._<crypto.Signer>();
    if (!ok) {
        return null;
    }
    slice<SignatureScheme> sigAlgs = default;
    switch (priv.Public().type()) {
        case ptr<ecdsa.PublicKey> pub:
            if (version != VersionTLS13) { 
                // In TLS 1.2 and earlier, ECDSA algorithms are not
                // constrained to a single curve.
                sigAlgs = new slice<SignatureScheme>(new SignatureScheme[] { ECDSAWithP256AndSHA256, ECDSAWithP384AndSHA384, ECDSAWithP521AndSHA512, ECDSAWithSHA1 });
                break;
            }

            if (pub.Curve == elliptic.P256()) 
                sigAlgs = new slice<SignatureScheme>(new SignatureScheme[] { ECDSAWithP256AndSHA256 });
            else if (pub.Curve == elliptic.P384()) 
                sigAlgs = new slice<SignatureScheme>(new SignatureScheme[] { ECDSAWithP384AndSHA384 });
            else if (pub.Curve == elliptic.P521()) 
                sigAlgs = new slice<SignatureScheme>(new SignatureScheme[] { ECDSAWithP521AndSHA512 });
            else 
                return null;
                        break;
        case ptr<rsa.PublicKey> pub:
            var size = pub.Size();
            sigAlgs = make_slice<SignatureScheme>(0, len(rsaSignatureSchemes));
            foreach (var (_, candidate) in rsaSignatureSchemes) {
                if (size >= candidate.minModulusBytes && version <= candidate.maxVersion) {
                    sigAlgs = append(sigAlgs, candidate.scheme);
                }
            }
            break;
        case ed25519.PublicKey pub:
            sigAlgs = new slice<SignatureScheme>(new SignatureScheme[] { Ed25519 });
            break;
        default:
        {
            var pub = priv.Public().type();
            return null;
            break;
        }

    }

    if (cert.SupportedSignatureAlgorithms != null) {
        slice<SignatureScheme> filteredSigAlgs = default;
        foreach (var (_, sigAlg) in sigAlgs) {
            if (isSupportedSignatureAlgorithm(sigAlg, cert.SupportedSignatureAlgorithms)) {
                filteredSigAlgs = append(filteredSigAlgs, sigAlg);
            }
        }        return filteredSigAlgs;
    }
    return sigAlgs;
}

// selectSignatureScheme picks a SignatureScheme from the peer's preference list
// that works with the selected certificate. It's only called for protocol
// versions that support signature algorithms, so TLS 1.2 and 1.3.
private static (SignatureScheme, error) selectSignatureScheme(ushort vers, ptr<Certificate> _addr_c, slice<SignatureScheme> peerAlgs) {
    SignatureScheme _p0 = default;
    error _p0 = default!;
    ref Certificate c = ref _addr_c.val;

    var supportedAlgs = signatureSchemesForCertificate(vers, _addr_c);
    if (len(supportedAlgs) == 0) {
        return (0, error.As(unsupportedCertificateError(_addr_c))!);
    }
    if (len(peerAlgs) == 0 && vers == VersionTLS12) { 
        // For TLS 1.2, if the client didn't send signature_algorithms then we
        // can assume that it supports SHA1. See RFC 5246, Section 7.4.1.4.1.
        peerAlgs = new slice<SignatureScheme>(new SignatureScheme[] { PKCS1WithSHA1, ECDSAWithSHA1 });
    }
    foreach (var (_, preferredAlg) in peerAlgs) {
        if (isSupportedSignatureAlgorithm(preferredAlg, supportedAlgs)) {
            return (preferredAlg, error.As(null!)!);
        }
    }    return (0, error.As(errors.New("tls: peer doesn't support any of the certificate's signature algorithms"))!);
}

// unsupportedCertificateError returns a helpful error for certificates with
// an unsupported private key.
private static error unsupportedCertificateError(ptr<Certificate> _addr_cert) {
    ref Certificate cert = ref _addr_cert.val;

    switch (cert.PrivateKey.type()) {
        case rsa.PrivateKey _:
            return error.As(fmt.Errorf("tls: unsupported certificate: private key is %T, expected *%T", cert.PrivateKey, cert.PrivateKey))!;
            break;
        case ecdsa.PrivateKey _:
            return error.As(fmt.Errorf("tls: unsupported certificate: private key is %T, expected *%T", cert.PrivateKey, cert.PrivateKey))!;
            break;
        case ptr<ed25519.PrivateKey> _:
            return error.As(fmt.Errorf("tls: unsupported certificate: private key is *ed25519.PrivateKey, expected ed25519.PrivateKey"))!;
            break;

    }

    crypto.Signer (signer, ok) = cert.PrivateKey._<crypto.Signer>();
    if (!ok) {
        return error.As(fmt.Errorf("tls: certificate private key (%T) does not implement crypto.Signer", cert.PrivateKey))!;
    }
    switch (signer.Public().type()) {
        case ptr<ecdsa.PublicKey> pub:

            if (pub.Curve == elliptic.P256())             else if (pub.Curve == elliptic.P384())             else if (pub.Curve == elliptic.P521())             else 
                return error.As(fmt.Errorf("tls: unsupported certificate curve (%s)", pub.Curve.Params().Name))!;
                        break;
        case ptr<rsa.PublicKey> pub:
            return error.As(fmt.Errorf("tls: certificate RSA key size too small for supported signature algorithms"))!;
            break;
        case ed25519.PublicKey pub:
            break;
        default:
        {
            var pub = signer.Public().type();
            return error.As(fmt.Errorf("tls: unsupported certificate key (%T)", pub))!;
            break;
        }

    }

    if (cert.SupportedSignatureAlgorithms != null) {
        return error.As(fmt.Errorf("tls: peer doesn't support the certificate custom signature algorithms"))!;
    }
    return error.As(fmt.Errorf("tls: internal error: unsupported key (%T)", cert.PrivateKey))!;
}

} // end tls_package
