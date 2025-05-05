// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using godebug = @internal.godebug_package;
using slices = slices_package;
using _ = unsafe_package; // for linkname
using @internal;

partial class tls_package {

// Defaults are collected in this file to allow distributions to more easily patch
// them to apply local policies.
internal static ж<godebug.Setting> tlskyber = godebug.New("tlskyber"u8);

internal static slice<CurveID> defaultCurvePreferences() {
    if (tlskyber.Value() == "0"u8) {
        return new CurveID[]{X25519, CurveP256, CurveP384, CurveP521}.slice();
    }
    // For now, x25519Kyber768Draft00 must always be followed by X25519.
    return new CurveID[]{x25519Kyber768Draft00, X25519, CurveP256, CurveP384, CurveP521}.slice();
}

// defaultSupportedSignatureAlgorithms contains the signature and hash algorithms that
// the code advertises as supported in a TLS 1.2+ ClientHello and in a TLS 1.2+
// CertificateRequest. The two fields are merged to match with TLS 1.3.
// Note that in TLS 1.2, the ECDSA algorithms are not constrained to P-256, etc.
internal static slice<SignatureScheme> defaultSupportedSignatureAlgorithms = new SignatureScheme[]{
    PSSWithSHA256,
    ECDSAWithP256AndSHA256,
    Ed25519,
    PSSWithSHA384,
    PSSWithSHA512,
    PKCS1WithSHA256,
    PKCS1WithSHA384,
    PKCS1WithSHA512,
    ECDSAWithP384AndSHA384,
    ECDSAWithP521AndSHA512,
    PKCS1WithSHA1,
    ECDSAWithSHA1
}.slice();

internal static ж<godebug.Setting> tlsrsakex = godebug.New("tlsrsakex"u8);

internal static ж<godebug.Setting> tls3des = godebug.New("tls3des"u8);

internal static slice<uint16> defaultCipherSuites() {
    var suites = slices.Clone(cipherSuitesPreferenceOrder);
    return slices.DeleteFunc(suites, (uint16 c) => disabledCipherSuites[c] || tlsrsakex.Value() != "1"u8 && rsaKexCiphers[c] || tls3des.Value() != "1"u8 && tdesCiphers[c]);
}

// defaultCipherSuitesTLS13 is also the preference order, since there are no
// disabled by default TLS 1.3 cipher suites. The same AES vs ChaCha20 logic as
// cipherSuitesPreferenceOrder applies.
//
// defaultCipherSuitesTLS13 should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/quic-go/quic-go
//   - github.com/sagernet/quic-go
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname defaultCipherSuitesTLS13
internal static slice<uint16> defaultCipherSuitesTLS13 = new uint16[]{
    TLS_AES_128_GCM_SHA256,
    TLS_AES_256_GCM_SHA384,
    TLS_CHACHA20_POLY1305_SHA256
}.slice();

// defaultCipherSuitesTLS13NoAES should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/quic-go/quic-go
//   - github.com/sagernet/quic-go
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname defaultCipherSuitesTLS13NoAES
internal static slice<uint16> defaultCipherSuitesTLS13NoAES = new uint16[]{
    TLS_CHACHA20_POLY1305_SHA256,
    TLS_AES_128_GCM_SHA256,
    TLS_AES_256_GCM_SHA384
}.slice();

internal static slice<uint16> defaultSupportedVersionsFIPS = new uint16[]{
    VersionTLS12
}.slice();

// defaultCurvePreferencesFIPS are the FIPS-allowed curves,
// in preference order (most preferable first).
internal static slice<CurveID> defaultCurvePreferencesFIPS = new CurveID[]{CurveP256, CurveP384, CurveP521}.slice();

// defaultSupportedSignatureAlgorithmsFIPS currently are a subset of
// defaultSupportedSignatureAlgorithms without Ed25519 and SHA-1.
internal static slice<SignatureScheme> defaultSupportedSignatureAlgorithmsFIPS = new SignatureScheme[]{
    PSSWithSHA256,
    PSSWithSHA384,
    PSSWithSHA512,
    PKCS1WithSHA256,
    ECDSAWithP256AndSHA256,
    PKCS1WithSHA384,
    ECDSAWithP384AndSHA384,
    PKCS1WithSHA512,
    ECDSAWithP521AndSHA512
}.slice();

// defaultCipherSuitesFIPS are the FIPS-allowed cipher suites.
internal static slice<uint16> defaultCipherSuitesFIPS = new uint16[]{
    TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,
    TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384,
    TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256,
    TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384,
    TLS_RSA_WITH_AES_128_GCM_SHA256,
    TLS_RSA_WITH_AES_256_GCM_SHA384
}.slice();

// defaultCipherSuitesTLS13FIPS are the FIPS-allowed cipher suites for TLS 1.3.
internal static slice<uint16> defaultCipherSuitesTLS13FIPS = new uint16[]{
    TLS_AES_128_GCM_SHA256,
    TLS_AES_256_GCM_SHA384
}.slice();

} // end tls_package
