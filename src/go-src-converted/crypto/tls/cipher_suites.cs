// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package tls -- go2cs converted at 2022 March 06 22:19:29 UTC
// import "crypto/tls" ==> using tls = go.crypto.tls_package
// Original source: C:\Program Files\Go\src\crypto\tls\cipher_suites.go
using crypto = go.crypto_package;
using aes = go.crypto.aes_package;
using cipher = go.crypto.cipher_package;
using des = go.crypto.des_package;
using hmac = go.crypto.hmac_package;
using rc4 = go.crypto.rc4_package;
using sha1 = go.crypto.sha1_package;
using sha256 = go.crypto.sha256_package;
using fmt = go.fmt_package;
using hash = go.hash_package;
using cpu = go.@internal.cpu_package;
using runtime = go.runtime_package;

using chacha20poly1305 = go.golang.org.x.crypto.chacha20poly1305_package;
using System;


namespace go.crypto;

public static partial class tls_package {

    // CipherSuite is a TLS cipher suite. Note that most functions in this package
    // accept and expose cipher suite IDs instead of this type.
public partial struct CipherSuite {
    public ushort ID;
    public @string Name; // Supported versions is the list of TLS protocol versions that can
// negotiate this cipher suite.
    public slice<ushort> SupportedVersions; // Insecure is true if the cipher suite has known security issues
// due to its primitives, design, or implementation.
    public bool Insecure;
}

private static ushort supportedUpToTLS12 = new slice<ushort>(new ushort[] { VersionTLS10, VersionTLS11, VersionTLS12 });private static ushort supportedOnlyTLS12 = new slice<ushort>(new ushort[] { VersionTLS12 });private static ushort supportedOnlyTLS13 = new slice<ushort>(new ushort[] { VersionTLS13 });

// CipherSuites returns a list of cipher suites currently implemented by this
// package, excluding those with security issues, which are returned by
// InsecureCipherSuites.
//
// The list is sorted by ID. Note that the default cipher suites selected by
// this package might depend on logic that can't be captured by a static list,
// and might not match those returned by this function.
public static slice<ptr<CipherSuite>> CipherSuites() {
    return new slice<ptr<CipherSuite>>(new ptr<CipherSuite>[] { {TLS_RSA_WITH_AES_128_CBC_SHA,"TLS_RSA_WITH_AES_128_CBC_SHA",supportedUpToTLS12,false}, {TLS_RSA_WITH_AES_256_CBC_SHA,"TLS_RSA_WITH_AES_256_CBC_SHA",supportedUpToTLS12,false}, {TLS_RSA_WITH_AES_128_GCM_SHA256,"TLS_RSA_WITH_AES_128_GCM_SHA256",supportedOnlyTLS12,false}, {TLS_RSA_WITH_AES_256_GCM_SHA384,"TLS_RSA_WITH_AES_256_GCM_SHA384",supportedOnlyTLS12,false}, {TLS_AES_128_GCM_SHA256,"TLS_AES_128_GCM_SHA256",supportedOnlyTLS13,false}, {TLS_AES_256_GCM_SHA384,"TLS_AES_256_GCM_SHA384",supportedOnlyTLS13,false}, {TLS_CHACHA20_POLY1305_SHA256,"TLS_CHACHA20_POLY1305_SHA256",supportedOnlyTLS13,false}, {TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA,"TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA",supportedUpToTLS12,false}, {TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA,"TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA",supportedUpToTLS12,false}, {TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA,"TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA",supportedUpToTLS12,false}, {TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA,"TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA",supportedUpToTLS12,false}, {TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256,"TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256",supportedOnlyTLS12,false}, {TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384,"TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384",supportedOnlyTLS12,false}, {TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,"TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256",supportedOnlyTLS12,false}, {TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384,"TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384",supportedOnlyTLS12,false}, {TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256,"TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256",supportedOnlyTLS12,false}, {TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305_SHA256,"TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305_SHA256",supportedOnlyTLS12,false} });
}

// InsecureCipherSuites returns a list of cipher suites currently implemented by
// this package and which have security issues.
//
// Most applications should not use the cipher suites in this list, and should
// only use those returned by CipherSuites.
public static slice<ptr<CipherSuite>> InsecureCipherSuites() { 
    // This list includes RC4, CBC_SHA256, and 3DES cipher suites. See
    // cipherSuitesPreferenceOrder for details.
    return new slice<ptr<CipherSuite>>(new ptr<CipherSuite>[] { {TLS_RSA_WITH_RC4_128_SHA,"TLS_RSA_WITH_RC4_128_SHA",supportedUpToTLS12,true}, {TLS_RSA_WITH_3DES_EDE_CBC_SHA,"TLS_RSA_WITH_3DES_EDE_CBC_SHA",supportedUpToTLS12,true}, {TLS_RSA_WITH_AES_128_CBC_SHA256,"TLS_RSA_WITH_AES_128_CBC_SHA256",supportedOnlyTLS12,true}, {TLS_ECDHE_ECDSA_WITH_RC4_128_SHA,"TLS_ECDHE_ECDSA_WITH_RC4_128_SHA",supportedUpToTLS12,true}, {TLS_ECDHE_RSA_WITH_RC4_128_SHA,"TLS_ECDHE_RSA_WITH_RC4_128_SHA",supportedUpToTLS12,true}, {TLS_ECDHE_RSA_WITH_3DES_EDE_CBC_SHA,"TLS_ECDHE_RSA_WITH_3DES_EDE_CBC_SHA",supportedUpToTLS12,true}, {TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256,"TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256",supportedOnlyTLS12,true}, {TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256,"TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256",supportedOnlyTLS12,true} });

}

// CipherSuiteName returns the standard name for the passed cipher suite ID
// (e.g. "TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256"), or a fallback representation
// of the ID value if the cipher suite is not implemented by this package.
public static @string CipherSuiteName(ushort id) {
    {
        var c__prev1 = c;

        foreach (var (_, __c) in CipherSuites()) {
            c = __c;
            if (c.ID == id) {
                return c.Name;
            }
        }
        c = c__prev1;
    }

    {
        var c__prev1 = c;

        foreach (var (_, __c) in InsecureCipherSuites()) {
            c = __c;
            if (c.ID == id) {
                return c.Name;
            }
        }
        c = c__prev1;
    }

    return fmt.Sprintf("0x%04X", id);

}

 
// suiteECDHE indicates that the cipher suite involves elliptic curve
// Diffie-Hellman. This means that it should only be selected when the
// client indicates that it supports ECC with a curve and point format
// that we're happy with.
private static readonly nint suiteECDHE = 1 << (int)(iota); 
// suiteECSign indicates that the cipher suite involves an ECDSA or
// EdDSA signature and therefore may only be selected when the server's
// certificate is ECDSA or EdDSA. If this is not set then the cipher suite
// is RSA based.
private static readonly var suiteECSign = 0; 
// suiteTLS12 indicates that the cipher suite should only be advertised
// and accepted when using TLS 1.2.
private static readonly var suiteTLS12 = 1; 
// suiteSHA384 indicates that the cipher suite uses SHA384 as the
// handshake hash.
private static readonly var suiteSHA384 = 2;


// A cipherSuite is a TLS 1.0–1.2 cipher suite, and defines the key exchange
// mechanism, as well as the cipher+MAC pair or the AEAD.
private partial struct cipherSuite {
    public ushort id; // the lengths, in bytes, of the key material needed for each component.
    public nint keyLen;
    public nint macLen;
    public nint ivLen;
    public Func<ushort, keyAgreement> ka; // flags is a bitmask of the suite* values, above.
    public nint flags;
    public Action<slice<byte>, slice<byte>, bool> cipher;
    public Func<slice<byte>, hash.Hash> mac;
    public Func<slice<byte>, slice<byte>, aead> aead;
}

private static ptr<cipherSuite> cipherSuites = new slice<ptr<cipherSuite>>(new ptr<cipherSuite>[] { {TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305,32,0,12,ecdheRSAKA,suiteECDHE|suiteTLS12,nil,nil,aeadChaCha20Poly1305}, {TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305,32,0,12,ecdheECDSAKA,suiteECDHE|suiteECSign|suiteTLS12,nil,nil,aeadChaCha20Poly1305}, {TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,16,0,4,ecdheRSAKA,suiteECDHE|suiteTLS12,nil,nil,aeadAESGCM}, {TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256,16,0,4,ecdheECDSAKA,suiteECDHE|suiteECSign|suiteTLS12,nil,nil,aeadAESGCM}, {TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384,32,0,4,ecdheRSAKA,suiteECDHE|suiteTLS12|suiteSHA384,nil,nil,aeadAESGCM}, {TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384,32,0,4,ecdheECDSAKA,suiteECDHE|suiteECSign|suiteTLS12|suiteSHA384,nil,nil,aeadAESGCM}, {TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256,16,32,16,ecdheRSAKA,suiteECDHE|suiteTLS12,cipherAES,macSHA256,nil}, {TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA,16,20,16,ecdheRSAKA,suiteECDHE,cipherAES,macSHA1,nil}, {TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256,16,32,16,ecdheECDSAKA,suiteECDHE|suiteECSign|suiteTLS12,cipherAES,macSHA256,nil}, {TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA,16,20,16,ecdheECDSAKA,suiteECDHE|suiteECSign,cipherAES,macSHA1,nil}, {TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA,32,20,16,ecdheRSAKA,suiteECDHE,cipherAES,macSHA1,nil}, {TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA,32,20,16,ecdheECDSAKA,suiteECDHE|suiteECSign,cipherAES,macSHA1,nil}, {TLS_RSA_WITH_AES_128_GCM_SHA256,16,0,4,rsaKA,suiteTLS12,nil,nil,aeadAESGCM}, {TLS_RSA_WITH_AES_256_GCM_SHA384,32,0,4,rsaKA,suiteTLS12|suiteSHA384,nil,nil,aeadAESGCM}, {TLS_RSA_WITH_AES_128_CBC_SHA256,16,32,16,rsaKA,suiteTLS12,cipherAES,macSHA256,nil}, {TLS_RSA_WITH_AES_128_CBC_SHA,16,20,16,rsaKA,0,cipherAES,macSHA1,nil}, {TLS_RSA_WITH_AES_256_CBC_SHA,32,20,16,rsaKA,0,cipherAES,macSHA1,nil}, {TLS_ECDHE_RSA_WITH_3DES_EDE_CBC_SHA,24,20,8,ecdheRSAKA,suiteECDHE,cipher3DES,macSHA1,nil}, {TLS_RSA_WITH_3DES_EDE_CBC_SHA,24,20,8,rsaKA,0,cipher3DES,macSHA1,nil}, {TLS_RSA_WITH_RC4_128_SHA,16,20,0,rsaKA,0,cipherRC4,macSHA1,nil}, {TLS_ECDHE_RSA_WITH_RC4_128_SHA,16,20,0,ecdheRSAKA,suiteECDHE,cipherRC4,macSHA1,nil}, {TLS_ECDHE_ECDSA_WITH_RC4_128_SHA,16,20,0,ecdheECDSAKA,suiteECDHE|suiteECSign,cipherRC4,macSHA1,nil} });

// selectCipherSuite returns the first TLS 1.0–1.2 cipher suite from ids which
// is also in supportedIDs and passes the ok filter.
private static ptr<cipherSuite> selectCipherSuite(slice<ushort> ids, slice<ushort> supportedIDs, Func<ptr<cipherSuite>, bool> ok) {
    foreach (var (_, id) in ids) {
        var candidate = cipherSuiteByID(id);
        if (candidate == null || !ok(candidate)) {
            continue;
        }
        foreach (var (_, suppID) in supportedIDs) {
            if (id == suppID) {
                return _addr_candidate!;
            }
        }
    }    return _addr_null!;

}

// A cipherSuiteTLS13 defines only the pair of the AEAD algorithm and hash
// algorithm to be used with HKDF. See RFC 8446, Appendix B.4.
private partial struct cipherSuiteTLS13 {
    public ushort id;
    public nint keyLen;
    public Func<slice<byte>, slice<byte>, aead> aead;
    public crypto.Hash hash;
}

private static ptr<cipherSuiteTLS13> cipherSuitesTLS13 = new slice<ptr<cipherSuiteTLS13>>(new ptr<cipherSuiteTLS13>[] { {TLS_AES_128_GCM_SHA256,16,aeadAESGCMTLS13,crypto.SHA256}, {TLS_CHACHA20_POLY1305_SHA256,32,aeadChaCha20Poly1305,crypto.SHA256}, {TLS_AES_256_GCM_SHA384,32,aeadAESGCMTLS13,crypto.SHA384} });

// cipherSuitesPreferenceOrder is the order in which we'll select (on the
// server) or advertise (on the client) TLS 1.0–1.2 cipher suites.
//
// Cipher suites are filtered but not reordered based on the application and
// peer's preferences, meaning we'll never select a suite lower in this list if
// any higher one is available. This makes it more defensible to keep weaker
// cipher suites enabled, especially on the server side where we get the last
// word, since there are no known downgrade attacks on cipher suites selection.
//
// The list is sorted by applying the following priority rules, stopping at the
// first (most important) applicable one:
//
//   - Anything else comes before RC4
//
//       RC4 has practically exploitable biases. See https://www.rc4nomore.com.
//
//   - Anything else comes before CBC_SHA256
//
//       SHA-256 variants of the CBC ciphersuites don't implement any Lucky13
//       countermeasures. See http://www.isg.rhul.ac.uk/tls/Lucky13.html and
//       https://www.imperialviolet.org/2013/02/04/luckythirteen.html.
//
//   - Anything else comes before 3DES
//
//       3DES has 64-bit blocks, which makes it fundamentally susceptible to
//       birthday attacks. See https://sweet32.info.
//
//   - ECDHE comes before anything else
//
//       Once we got the broken stuff out of the way, the most important
//       property a cipher suite can have is forward secrecy. We don't
//       implement FFDHE, so that means ECDHE.
//
//   - AEADs come before CBC ciphers
//
//       Even with Lucky13 countermeasures, MAC-then-Encrypt CBC cipher suites
//       are fundamentally fragile, and suffered from an endless sequence of
//       padding oracle attacks. See https://eprint.iacr.org/2015/1129,
//       https://www.imperialviolet.org/2014/12/08/poodleagain.html, and
//       https://blog.cloudflare.com/yet-another-padding-oracle-in-openssl-cbc-ciphersuites/.
//
//   - AES comes before ChaCha20
//
//       When AES hardware is available, AES-128-GCM and AES-256-GCM are faster
//       than ChaCha20Poly1305.
//
//       When AES hardware is not available, AES-128-GCM is one or more of: much
//       slower, way more complex, and less safe (because not constant time)
//       than ChaCha20Poly1305.
//
//       We use this list if we think both peers have AES hardware, and
//       cipherSuitesPreferenceOrderNoAES otherwise.
//
//   - AES-128 comes before AES-256
//
//       The only potential advantages of AES-256 are better multi-target
//       margins, and hypothetical post-quantum properties. Neither apply to
//       TLS, and AES-256 is slower due to its four extra rounds (which don't
//       contribute to the advantages above).
//
//   - ECDSA comes before RSA
//
//       The relative order of ECDSA and RSA cipher suites doesn't matter,
//       as they depend on the certificate. Pick one to get a stable order.
//
private static ushort cipherSuitesPreferenceOrder = new slice<ushort>(new ushort[] { TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256, TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256, TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384, TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384, TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305, TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305, TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA, TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA, TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA, TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA, TLS_RSA_WITH_AES_128_GCM_SHA256, TLS_RSA_WITH_AES_256_GCM_SHA384, TLS_RSA_WITH_AES_128_CBC_SHA, TLS_RSA_WITH_AES_256_CBC_SHA, TLS_ECDHE_RSA_WITH_3DES_EDE_CBC_SHA, TLS_RSA_WITH_3DES_EDE_CBC_SHA, TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256, TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256, TLS_RSA_WITH_AES_128_CBC_SHA256, TLS_ECDHE_ECDSA_WITH_RC4_128_SHA, TLS_ECDHE_RSA_WITH_RC4_128_SHA, TLS_RSA_WITH_RC4_128_SHA });

private static ushort cipherSuitesPreferenceOrderNoAES = new slice<ushort>(new ushort[] { TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305, TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305, TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256, TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256, TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384, TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384, TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA, TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA, TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA, TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA, TLS_RSA_WITH_AES_128_GCM_SHA256, TLS_RSA_WITH_AES_256_GCM_SHA384, TLS_RSA_WITH_AES_128_CBC_SHA, TLS_RSA_WITH_AES_256_CBC_SHA, TLS_ECDHE_RSA_WITH_3DES_EDE_CBC_SHA, TLS_RSA_WITH_3DES_EDE_CBC_SHA, TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256, TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256, TLS_RSA_WITH_AES_128_CBC_SHA256, TLS_ECDHE_ECDSA_WITH_RC4_128_SHA, TLS_ECDHE_RSA_WITH_RC4_128_SHA, TLS_RSA_WITH_RC4_128_SHA });

// disabledCipherSuites are not used unless explicitly listed in
// Config.CipherSuites. They MUST be at the end of cipherSuitesPreferenceOrder.
private static ushort disabledCipherSuites = new slice<ushort>(new ushort[] { TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256, TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256, TLS_RSA_WITH_AES_128_CBC_SHA256, TLS_ECDHE_ECDSA_WITH_RC4_128_SHA, TLS_ECDHE_RSA_WITH_RC4_128_SHA, TLS_RSA_WITH_RC4_128_SHA });

private static var defaultCipherSuitesLen = len(cipherSuitesPreferenceOrder) - len(disabledCipherSuites);private static var defaultCipherSuites = cipherSuitesPreferenceOrder[..(int)defaultCipherSuitesLen];

// defaultCipherSuitesTLS13 is also the preference order, since there are no
// disabled by default TLS 1.3 cipher suites. The same AES vs ChaCha20 logic as
// cipherSuitesPreferenceOrder applies.
private static ushort defaultCipherSuitesTLS13 = new slice<ushort>(new ushort[] { TLS_AES_128_GCM_SHA256, TLS_AES_256_GCM_SHA384, TLS_CHACHA20_POLY1305_SHA256 });

private static ushort defaultCipherSuitesTLS13NoAES = new slice<ushort>(new ushort[] { TLS_CHACHA20_POLY1305_SHA256, TLS_AES_128_GCM_SHA256, TLS_AES_256_GCM_SHA384 });

private static var hasGCMAsmAMD64 = cpu.X86.HasAES && cpu.X86.HasPCLMULQDQ;private static var hasGCMAsmARM64 = cpu.ARM64.HasAES && cpu.ARM64.HasPMULL;private static var hasGCMAsmS390X = cpu.S390X.HasAES && cpu.S390X.HasAESCBC && cpu.S390X.HasAESCTR && (cpu.S390X.HasGHASH || cpu.S390X.HasAESGCM);private static var hasAESGCMHardwareSupport = runtime.GOARCH == "amd64" && hasGCMAsmAMD64 || runtime.GOARCH == "arm64" && hasGCMAsmARM64 || runtime.GOARCH == "s390x" && hasGCMAsmS390X;

private static map aesgcmCiphers = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ushort, bool>{TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256:true,TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384:true,TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256:true,TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384:true,TLS_AES_128_GCM_SHA256:true,TLS_AES_256_GCM_SHA384:true,};

private static map nonAESGCMAEADCiphers = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ushort, bool>{TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305:true,TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305:true,TLS_CHACHA20_POLY1305_SHA256:true,};

// aesgcmPreferred returns whether the first known cipher in the preference list
// is an AES-GCM cipher, implying the peer has hardware support for it.
private static bool aesgcmPreferred(slice<ushort> ciphers) {
    foreach (var (_, cID) in ciphers) {
        {
            var c__prev1 = c;

            var c = cipherSuiteByID(cID);

            if (c != null) {
                return aesgcmCiphers[cID];
            }

            c = c__prev1;

        }

        {
            var c__prev1 = c;

            c = cipherSuiteTLS13ByID(cID);

            if (c != null) {
                return aesgcmCiphers[cID];
            }

            c = c__prev1;

        }

    }    return false;

}

private static void cipherRC4(slice<byte> key, slice<byte> iv, bool isRead) {
    var (cipher, _) = rc4.NewCipher(key);
    return cipher;
}

private static void cipher3DES(slice<byte> key, slice<byte> iv, bool isRead) {
    var (block, _) = des.NewTripleDESCipher(key);
    if (isRead) {
        return cipher.NewCBCDecrypter(block, iv);
    }
    return cipher.NewCBCEncrypter(block, iv);

}

private static void cipherAES(slice<byte> key, slice<byte> iv, bool isRead) {
    var (block, _) = aes.NewCipher(key);
    if (isRead) {
        return cipher.NewCBCDecrypter(block, iv);
    }
    return cipher.NewCBCEncrypter(block, iv);

}

// macSHA1 returns a SHA-1 based constant time MAC.
private static hash.Hash macSHA1(slice<byte> key) {
    return hmac.New(newConstantTimeHash(sha1.New), key);
}

// macSHA256 returns a SHA-256 based MAC. This is only supported in TLS 1.2 and
// is currently only used in disabled-by-default cipher suites.
private static hash.Hash macSHA256(slice<byte> key) {
    return hmac.New(sha256.New, key);
}

private partial interface aead {
    nint explicitNonceLen();
}

private static readonly nint aeadNonceLength = 12;
private static readonly nint noncePrefixLength = 4;


// prefixNonceAEAD wraps an AEAD and prefixes a fixed portion of the nonce to
// each call.
private partial struct prefixNonceAEAD {
    public array<byte> nonce;
    public cipher.AEAD aead;
}

private static nint NonceSize(this ptr<prefixNonceAEAD> _addr_f) {
    ref prefixNonceAEAD f = ref _addr_f.val;

    return aeadNonceLength - noncePrefixLength;
}
private static nint Overhead(this ptr<prefixNonceAEAD> _addr_f) {
    ref prefixNonceAEAD f = ref _addr_f.val;

    return f.aead.Overhead();
}
private static nint explicitNonceLen(this ptr<prefixNonceAEAD> _addr_f) {
    ref prefixNonceAEAD f = ref _addr_f.val;

    return f.NonceSize();
}

private static slice<byte> Seal(this ptr<prefixNonceAEAD> _addr_f, slice<byte> @out, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData) {
    ref prefixNonceAEAD f = ref _addr_f.val;

    copy(f.nonce[(int)4..], nonce);
    return f.aead.Seal(out, f.nonce[..], plaintext, additionalData);
}

private static (slice<byte>, error) Open(this ptr<prefixNonceAEAD> _addr_f, slice<byte> @out, slice<byte> nonce, slice<byte> ciphertext, slice<byte> additionalData) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref prefixNonceAEAD f = ref _addr_f.val;

    copy(f.nonce[(int)4..], nonce);
    return f.aead.Open(out, f.nonce[..], ciphertext, additionalData);
}

// xoredNonceAEAD wraps an AEAD by XORing in a fixed pattern to the nonce
// before each call.
private partial struct xorNonceAEAD {
    public array<byte> nonceMask;
    public cipher.AEAD aead;
}

private static nint NonceSize(this ptr<xorNonceAEAD> _addr_f) {
    ref xorNonceAEAD f = ref _addr_f.val;

    return 8;
} // 64-bit sequence number
private static nint Overhead(this ptr<xorNonceAEAD> _addr_f) {
    ref xorNonceAEAD f = ref _addr_f.val;

    return f.aead.Overhead();
}
private static nint explicitNonceLen(this ptr<xorNonceAEAD> _addr_f) {
    ref xorNonceAEAD f = ref _addr_f.val;

    return 0;
}

private static slice<byte> Seal(this ptr<xorNonceAEAD> _addr_f, slice<byte> @out, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData) {
    ref xorNonceAEAD f = ref _addr_f.val;

    {
        var i__prev1 = i;
        var b__prev1 = b;

        foreach (var (__i, __b) in nonce) {
            i = __i;
            b = __b;
            f.nonceMask[4 + i] ^= b;
        }
        i = i__prev1;
        b = b__prev1;
    }

    var result = f.aead.Seal(out, f.nonceMask[..], plaintext, additionalData);
    {
        var i__prev1 = i;
        var b__prev1 = b;

        foreach (var (__i, __b) in nonce) {
            i = __i;
            b = __b;
            f.nonceMask[4 + i] ^= b;
        }
        i = i__prev1;
        b = b__prev1;
    }

    return result;

}

private static (slice<byte>, error) Open(this ptr<xorNonceAEAD> _addr_f, slice<byte> @out, slice<byte> nonce, slice<byte> ciphertext, slice<byte> additionalData) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref xorNonceAEAD f = ref _addr_f.val;

    {
        var i__prev1 = i;
        var b__prev1 = b;

        foreach (var (__i, __b) in nonce) {
            i = __i;
            b = __b;
            f.nonceMask[4 + i] ^= b;
        }
        i = i__prev1;
        b = b__prev1;
    }

    var (result, err) = f.aead.Open(out, f.nonceMask[..], ciphertext, additionalData);
    {
        var i__prev1 = i;
        var b__prev1 = b;

        foreach (var (__i, __b) in nonce) {
            i = __i;
            b = __b;
            f.nonceMask[4 + i] ^= b;
        }
        i = i__prev1;
        b = b__prev1;
    }

    return (result, error.As(err)!);

}

private static aead aeadAESGCM(slice<byte> key, slice<byte> noncePrefix) => func((_, panic, _) => {
    if (len(noncePrefix) != noncePrefixLength) {
        panic("tls: internal error: wrong nonce length");
    }
    var (aes, err) = aes.NewCipher(key);
    if (err != null) {
        panic(err);
    }
    var (aead, err) = cipher.NewGCM(aes);
    if (err != null) {
        panic(err);
    }
    ptr<prefixNonceAEAD> ret = addr(new prefixNonceAEAD(aead:aead));
    copy(ret.nonce[..], noncePrefix);
    return ret;

});

private static aead aeadAESGCMTLS13(slice<byte> key, slice<byte> nonceMask) => func((_, panic, _) => {
    if (len(nonceMask) != aeadNonceLength) {
        panic("tls: internal error: wrong nonce length");
    }
    var (aes, err) = aes.NewCipher(key);
    if (err != null) {
        panic(err);
    }
    var (aead, err) = cipher.NewGCM(aes);
    if (err != null) {
        panic(err);
    }
    ptr<xorNonceAEAD> ret = addr(new xorNonceAEAD(aead:aead));
    copy(ret.nonceMask[..], nonceMask);
    return ret;

});

private static aead aeadChaCha20Poly1305(slice<byte> key, slice<byte> nonceMask) => func((_, panic, _) => {
    if (len(nonceMask) != aeadNonceLength) {
        panic("tls: internal error: wrong nonce length");
    }
    var (aead, err) = chacha20poly1305.New(key);
    if (err != null) {
        panic(err);
    }
    ptr<xorNonceAEAD> ret = addr(new xorNonceAEAD(aead:aead));
    copy(ret.nonceMask[..], nonceMask);
    return ret;

});

private partial interface constantTimeHash {
    slice<byte> ConstantTimeSum(slice<byte> b);
}

// cthWrapper wraps any hash.Hash that implements ConstantTimeSum, and replaces
// with that all calls to Sum. It's used to obtain a ConstantTimeSum-based HMAC.
private partial struct cthWrapper {
    public constantTimeHash h;
}

private static nint Size(this ptr<cthWrapper> _addr_c) {
    ref cthWrapper c = ref _addr_c.val;

    return c.h.Size();
}
private static nint BlockSize(this ptr<cthWrapper> _addr_c) {
    ref cthWrapper c = ref _addr_c.val;

    return c.h.BlockSize();
}
private static void Reset(this ptr<cthWrapper> _addr_c) {
    ref cthWrapper c = ref _addr_c.val;

    c.h.Reset();
}
private static (nint, error) Write(this ptr<cthWrapper> _addr_c, slice<byte> p) {
    nint _p0 = default;
    error _p0 = default!;
    ref cthWrapper c = ref _addr_c.val;

    return c.h.Write(p);
}
private static slice<byte> Sum(this ptr<cthWrapper> _addr_c, slice<byte> b) {
    ref cthWrapper c = ref _addr_c.val;

    return c.h.ConstantTimeSum(b);
}

private static Func<hash.Hash> newConstantTimeHash(Func<hash.Hash> h) {
    return () => {
        return addr(new cthWrapper(h().(constantTimeHash)));
    };
}

// tls10MAC implements the TLS 1.0 MAC function. RFC 2246, Section 6.2.3.
private static slice<byte> tls10MAC(hash.Hash h, slice<byte> @out, slice<byte> seq, slice<byte> header, slice<byte> data, slice<byte> extra) {
    h.Reset();
    h.Write(seq);
    h.Write(header);
    h.Write(data);
    var res = h.Sum(out);
    if (extra != null) {
        h.Write(extra);
    }
    return res;

}

private static keyAgreement rsaKA(ushort version) {
    return new rsaKeyAgreement();
}

private static keyAgreement ecdheECDSAKA(ushort version) {
    return addr(new ecdheKeyAgreement(isRSA:false,version:version,));
}

private static keyAgreement ecdheRSAKA(ushort version) {
    return addr(new ecdheKeyAgreement(isRSA:true,version:version,));
}

// mutualCipherSuite returns a cipherSuite given a list of supported
// ciphersuites and the id requested by the peer.
private static ptr<cipherSuite> mutualCipherSuite(slice<ushort> have, ushort want) {
    foreach (var (_, id) in have) {
        if (id == want) {
            return _addr_cipherSuiteByID(id)!;
        }
    }    return _addr_null!;

}

private static ptr<cipherSuite> cipherSuiteByID(ushort id) {
    foreach (var (_, cipherSuite) in cipherSuites) {
        if (cipherSuite.id == id) {
            return _addr_cipherSuite!;
        }
    }    return _addr_null!;

}

private static ptr<cipherSuiteTLS13> mutualCipherSuiteTLS13(slice<ushort> have, ushort want) {
    foreach (var (_, id) in have) {
        if (id == want) {
            return _addr_cipherSuiteTLS13ByID(id)!;
        }
    }    return _addr_null!;

}

private static ptr<cipherSuiteTLS13> cipherSuiteTLS13ByID(ushort id) {
    foreach (var (_, cipherSuite) in cipherSuitesTLS13) {
        if (cipherSuite.id == id) {
            return _addr_cipherSuite!;
        }
    }    return _addr_null!;

}

// A list of cipher suite IDs that are, or have been, implemented by this
// package.
//
// See https://www.iana.org/assignments/tls-parameters/tls-parameters.xml
 
// TLS 1.0 - 1.2 cipher suites.
public static readonly ushort TLS_RSA_WITH_RC4_128_SHA = 0x0005;
public static readonly ushort TLS_RSA_WITH_3DES_EDE_CBC_SHA = 0x000a;
public static readonly ushort TLS_RSA_WITH_AES_128_CBC_SHA = 0x002f;
public static readonly ushort TLS_RSA_WITH_AES_256_CBC_SHA = 0x0035;
public static readonly ushort TLS_RSA_WITH_AES_128_CBC_SHA256 = 0x003c;
public static readonly ushort TLS_RSA_WITH_AES_128_GCM_SHA256 = 0x009c;
public static readonly ushort TLS_RSA_WITH_AES_256_GCM_SHA384 = 0x009d;
public static readonly ushort TLS_ECDHE_ECDSA_WITH_RC4_128_SHA = 0xc007;
public static readonly ushort TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA = 0xc009;
public static readonly ushort TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA = 0xc00a;
public static readonly ushort TLS_ECDHE_RSA_WITH_RC4_128_SHA = 0xc011;
public static readonly ushort TLS_ECDHE_RSA_WITH_3DES_EDE_CBC_SHA = 0xc012;
public static readonly ushort TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA = 0xc013;
public static readonly ushort TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA = 0xc014;
public static readonly ushort TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256 = 0xc023;
public static readonly ushort TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256 = 0xc027;
public static readonly ushort TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256 = 0xc02f;
public static readonly ushort TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256 = 0xc02b;
public static readonly ushort TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384 = 0xc030;
public static readonly ushort TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384 = 0xc02c;
public static readonly ushort TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256 = 0xcca8;
public static readonly ushort TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305_SHA256 = 0xcca9; 

// TLS 1.3 cipher suites.
public static readonly ushort TLS_AES_128_GCM_SHA256 = 0x1301;
public static readonly ushort TLS_AES_256_GCM_SHA384 = 0x1302;
public static readonly ushort TLS_CHACHA20_POLY1305_SHA256 = 0x1303; 

// TLS_FALLBACK_SCSV isn't a standard cipher suite but an indicator
// that the client is doing version fallback. See RFC 7507.
public static readonly ushort TLS_FALLBACK_SCSV = 0x5600; 

// Legacy names for the corresponding cipher suites with the correct _SHA256
// suffix, retained for backward compatibility.
public static readonly var TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305 = TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256;
public static readonly var TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305 = TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305_SHA256;


} // end tls_package
