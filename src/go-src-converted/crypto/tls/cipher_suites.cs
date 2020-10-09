// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package tls -- go2cs converted at 2020 October 09 04:54:41 UTC
// import "crypto/tls" ==> using tls = go.crypto.tls_package
// Original source: C:\Go\src\crypto\tls\cipher_suites.go
using crypto = go.crypto_package;
using aes = go.crypto.aes_package;
using cipher = go.crypto.cipher_package;
using des = go.crypto.des_package;
using hmac = go.crypto.hmac_package;
using rc4 = go.crypto.rc4_package;
using sha1 = go.crypto.sha1_package;
using sha256 = go.crypto.sha256_package;
using x509 = go.crypto.x509_package;
using fmt = go.fmt_package;
using hash = go.hash_package;

using chacha20poly1305 = go.golang.org.x.crypto.chacha20poly1305_package;
using static go.builtin;
using System;

namespace go {
namespace crypto
{
    public static partial class tls_package
    {
        // CipherSuite is a TLS cipher suite. Note that most functions in this package
        // accept and expose cipher suite IDs instead of this type.
        public partial struct CipherSuite
        {
            public ushort ID;
            public @string Name; // Supported versions is the list of TLS protocol versions that can
// negotiate this cipher suite.
            public slice<ushort> SupportedVersions; // Insecure is true if the cipher suite has known security issues
// due to its primitives, design, or implementation.
            public bool Insecure;
        }

        private static ushort supportedUpToTLS12 = new slice<ushort>(new ushort[] { VersionTLS10, VersionTLS11, VersionTLS12 });        private static ushort supportedOnlyTLS12 = new slice<ushort>(new ushort[] { VersionTLS12 });        private static ushort supportedOnlyTLS13 = new slice<ushort>(new ushort[] { VersionTLS13 });

        // CipherSuites returns a list of cipher suites currently implemented by this
        // package, excluding those with security issues, which are returned by
        // InsecureCipherSuites.
        //
        // The list is sorted by ID. Note that the default cipher suites selected by
        // this package might depend on logic that can't be captured by a static list.
        public static slice<ptr<CipherSuite>> CipherSuites()
        {
            return new slice<ptr<CipherSuite>>(new ptr<CipherSuite>[] { {TLS_RSA_WITH_3DES_EDE_CBC_SHA,"TLS_RSA_WITH_3DES_EDE_CBC_SHA",supportedUpToTLS12,false}, {TLS_RSA_WITH_AES_128_CBC_SHA,"TLS_RSA_WITH_AES_128_CBC_SHA",supportedUpToTLS12,false}, {TLS_RSA_WITH_AES_256_CBC_SHA,"TLS_RSA_WITH_AES_256_CBC_SHA",supportedUpToTLS12,false}, {TLS_RSA_WITH_AES_128_GCM_SHA256,"TLS_RSA_WITH_AES_128_GCM_SHA256",supportedOnlyTLS12,false}, {TLS_RSA_WITH_AES_256_GCM_SHA384,"TLS_RSA_WITH_AES_256_GCM_SHA384",supportedOnlyTLS12,false}, {TLS_AES_128_GCM_SHA256,"TLS_AES_128_GCM_SHA256",supportedOnlyTLS13,false}, {TLS_AES_256_GCM_SHA384,"TLS_AES_256_GCM_SHA384",supportedOnlyTLS13,false}, {TLS_CHACHA20_POLY1305_SHA256,"TLS_CHACHA20_POLY1305_SHA256",supportedOnlyTLS13,false}, {TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA,"TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA",supportedUpToTLS12,false}, {TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA,"TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA",supportedUpToTLS12,false}, {TLS_ECDHE_RSA_WITH_3DES_EDE_CBC_SHA,"TLS_ECDHE_RSA_WITH_3DES_EDE_CBC_SHA",supportedUpToTLS12,false}, {TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA,"TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA",supportedUpToTLS12,false}, {TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA,"TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA",supportedUpToTLS12,false}, {TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256,"TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256",supportedOnlyTLS12,false}, {TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384,"TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384",supportedOnlyTLS12,false}, {TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,"TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256",supportedOnlyTLS12,false}, {TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384,"TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384",supportedOnlyTLS12,false}, {TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256,"TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256",supportedOnlyTLS12,false}, {TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305_SHA256,"TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305_SHA256",supportedOnlyTLS12,false} });
        }

        // InsecureCipherSuites returns a list of cipher suites currently implemented by
        // this package and which have security issues.
        //
        // Most applications should not use the cipher suites in this list, and should
        // only use those returned by CipherSuites.
        public static slice<ptr<CipherSuite>> InsecureCipherSuites()
        { 
            // RC4 suites are broken because RC4 is.
            // CBC-SHA256 suites have no Lucky13 countermeasures.
            return new slice<ptr<CipherSuite>>(new ptr<CipherSuite>[] { {TLS_RSA_WITH_RC4_128_SHA,"TLS_RSA_WITH_RC4_128_SHA",supportedUpToTLS12,true}, {TLS_RSA_WITH_AES_128_CBC_SHA256,"TLS_RSA_WITH_AES_128_CBC_SHA256",supportedOnlyTLS12,true}, {TLS_ECDHE_ECDSA_WITH_RC4_128_SHA,"TLS_ECDHE_ECDSA_WITH_RC4_128_SHA",supportedUpToTLS12,true}, {TLS_ECDHE_RSA_WITH_RC4_128_SHA,"TLS_ECDHE_RSA_WITH_RC4_128_SHA",supportedUpToTLS12,true}, {TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256,"TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256",supportedOnlyTLS12,true}, {TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256,"TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256",supportedOnlyTLS12,true} });

        }

        // CipherSuiteName returns the standard name for the passed cipher suite ID
        // (e.g. "TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256"), or a fallback representation
        // of the ID value if the cipher suite is not implemented by this package.
        public static @string CipherSuiteName(ushort id)
        {
            {
                var c__prev1 = c;

                foreach (var (_, __c) in CipherSuites())
                {
                    c = __c;
                    if (c.ID == id)
                    {
                        return c.Name;
                    }

                }

                c = c__prev1;
            }

            {
                var c__prev1 = c;

                foreach (var (_, __c) in InsecureCipherSuites())
                {
                    c = __c;
                    if (c.ID == id)
                    {
                        return c.Name;
                    }

                }

                c = c__prev1;
            }

            return fmt.Sprintf("0x%04X", id);

        }

        // a keyAgreement implements the client and server side of a TLS key agreement
        // protocol by generating and processing key exchange messages.
        private partial interface keyAgreement
        {
            (slice<byte>, ptr<clientKeyExchangeMsg>, error) generateServerKeyExchange(ptr<Config> _p0, ptr<Certificate> _p0, ptr<clientHelloMsg> _p0, ptr<serverHelloMsg> _p0);
            (slice<byte>, ptr<clientKeyExchangeMsg>, error) processClientKeyExchange(ptr<Config> _p0, ptr<Certificate> _p0, ptr<clientKeyExchangeMsg> _p0, ushort _p0); // On the client side, the next two methods are called in order.

// This method may not be called if the server doesn't send a
// ServerKeyExchange message.
            (slice<byte>, ptr<clientKeyExchangeMsg>, error) processServerKeyExchange(ptr<Config> _p0, ptr<clientHelloMsg> _p0, ptr<serverHelloMsg> _p0, ptr<x509.Certificate> _p0, ptr<serverKeyExchangeMsg> _p0);
            (slice<byte>, ptr<clientKeyExchangeMsg>, error) generateClientKeyExchange(ptr<Config> _p0, ptr<clientHelloMsg> _p0, ptr<x509.Certificate> _p0);
        }

 
        // suiteECDHE indicates that the cipher suite involves elliptic curve
        // Diffie-Hellman. This means that it should only be selected when the
        // client indicates that it supports ECC with a curve and point format
        // that we're happy with.
        private static readonly long suiteECDHE = (long)1L << (int)(iota); 
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
        // suiteDefaultOff indicates that this cipher suite is not included by
        // default.
        private static readonly var suiteDefaultOff = 3;


        // A cipherSuite is a specific combination of key agreement, cipher and MAC function.
        private partial struct cipherSuite
        {
            public ushort id; // the lengths, in bytes, of the key material needed for each component.
            public long keyLen;
            public long macLen;
            public long ivLen;
            public Func<ushort, keyAgreement> ka; // flags is a bitmask of the suite* values, above.
            public long flags;
            public Action<slice<byte>, slice<byte>, bool> cipher;
            public Func<ushort, slice<byte>, macFunction> mac;
            public Func<slice<byte>, slice<byte>, aead> aead;
        }

        private static ptr<cipherSuite> cipherSuites = new slice<ptr<cipherSuite>>(new ptr<cipherSuite>[] { {TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305,32,0,12,ecdheRSAKA,suiteECDHE|suiteTLS12,nil,nil,aeadChaCha20Poly1305}, {TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305,32,0,12,ecdheECDSAKA,suiteECDHE|suiteECSign|suiteTLS12,nil,nil,aeadChaCha20Poly1305}, {TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,16,0,4,ecdheRSAKA,suiteECDHE|suiteTLS12,nil,nil,aeadAESGCM}, {TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256,16,0,4,ecdheECDSAKA,suiteECDHE|suiteECSign|suiteTLS12,nil,nil,aeadAESGCM}, {TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384,32,0,4,ecdheRSAKA,suiteECDHE|suiteTLS12|suiteSHA384,nil,nil,aeadAESGCM}, {TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384,32,0,4,ecdheECDSAKA,suiteECDHE|suiteECSign|suiteTLS12|suiteSHA384,nil,nil,aeadAESGCM}, {TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256,16,32,16,ecdheRSAKA,suiteECDHE|suiteTLS12|suiteDefaultOff,cipherAES,macSHA256,nil}, {TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA,16,20,16,ecdheRSAKA,suiteECDHE,cipherAES,macSHA1,nil}, {TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256,16,32,16,ecdheECDSAKA,suiteECDHE|suiteECSign|suiteTLS12|suiteDefaultOff,cipherAES,macSHA256,nil}, {TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA,16,20,16,ecdheECDSAKA,suiteECDHE|suiteECSign,cipherAES,macSHA1,nil}, {TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA,32,20,16,ecdheRSAKA,suiteECDHE,cipherAES,macSHA1,nil}, {TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA,32,20,16,ecdheECDSAKA,suiteECDHE|suiteECSign,cipherAES,macSHA1,nil}, {TLS_RSA_WITH_AES_128_GCM_SHA256,16,0,4,rsaKA,suiteTLS12,nil,nil,aeadAESGCM}, {TLS_RSA_WITH_AES_256_GCM_SHA384,32,0,4,rsaKA,suiteTLS12|suiteSHA384,nil,nil,aeadAESGCM}, {TLS_RSA_WITH_AES_128_CBC_SHA256,16,32,16,rsaKA,suiteTLS12|suiteDefaultOff,cipherAES,macSHA256,nil}, {TLS_RSA_WITH_AES_128_CBC_SHA,16,20,16,rsaKA,0,cipherAES,macSHA1,nil}, {TLS_RSA_WITH_AES_256_CBC_SHA,32,20,16,rsaKA,0,cipherAES,macSHA1,nil}, {TLS_ECDHE_RSA_WITH_3DES_EDE_CBC_SHA,24,20,8,ecdheRSAKA,suiteECDHE,cipher3DES,macSHA1,nil}, {TLS_RSA_WITH_3DES_EDE_CBC_SHA,24,20,8,rsaKA,0,cipher3DES,macSHA1,nil}, {TLS_RSA_WITH_RC4_128_SHA,16,20,0,rsaKA,suiteDefaultOff,cipherRC4,macSHA1,nil}, {TLS_ECDHE_RSA_WITH_RC4_128_SHA,16,20,0,ecdheRSAKA,suiteECDHE|suiteDefaultOff,cipherRC4,macSHA1,nil}, {TLS_ECDHE_ECDSA_WITH_RC4_128_SHA,16,20,0,ecdheECDSAKA,suiteECDHE|suiteECSign|suiteDefaultOff,cipherRC4,macSHA1,nil} });

        // selectCipherSuite returns the first cipher suite from ids which is also in
        // supportedIDs and passes the ok filter.
        private static ptr<cipherSuite> selectCipherSuite(slice<ushort> ids, slice<ushort> supportedIDs, Func<ptr<cipherSuite>, bool> ok)
        {
            foreach (var (_, id) in ids)
            {
                var candidate = cipherSuiteByID(id);
                if (candidate == null || !ok(candidate))
                {
                    continue;
                }

                foreach (var (_, suppID) in supportedIDs)
                {
                    if (id == suppID)
                    {
                        return _addr_candidate!;
                    }

                }

            }
            return _addr_null!;

        }

        // A cipherSuiteTLS13 defines only the pair of the AEAD algorithm and hash
        // algorithm to be used with HKDF. See RFC 8446, Appendix B.4.
        private partial struct cipherSuiteTLS13
        {
            public ushort id;
            public long keyLen;
            public Func<slice<byte>, slice<byte>, aead> aead;
            public crypto.Hash hash;
        }

        private static ptr<cipherSuiteTLS13> cipherSuitesTLS13 = new slice<ptr<cipherSuiteTLS13>>(new ptr<cipherSuiteTLS13>[] { {TLS_AES_128_GCM_SHA256,16,aeadAESGCMTLS13,crypto.SHA256}, {TLS_CHACHA20_POLY1305_SHA256,32,aeadChaCha20Poly1305,crypto.SHA256}, {TLS_AES_256_GCM_SHA384,32,aeadAESGCMTLS13,crypto.SHA384} });

        private static void cipherRC4(slice<byte> key, slice<byte> iv, bool isRead)
        {
            var (cipher, _) = rc4.NewCipher(key);
            return cipher;
        }

        private static void cipher3DES(slice<byte> key, slice<byte> iv, bool isRead)
        {
            var (block, _) = des.NewTripleDESCipher(key);
            if (isRead)
            {
                return cipher.NewCBCDecrypter(block, iv);
            }

            return cipher.NewCBCEncrypter(block, iv);

        }

        private static void cipherAES(slice<byte> key, slice<byte> iv, bool isRead)
        {
            var (block, _) = aes.NewCipher(key);
            if (isRead)
            {
                return cipher.NewCBCDecrypter(block, iv);
            }

            return cipher.NewCBCEncrypter(block, iv);

        }

        // macSHA1 returns a macFunction for the given protocol version.
        private static macFunction macSHA1(ushort version, slice<byte> key)
        {
            return new tls10MAC(h:hmac.New(newConstantTimeHash(sha1.New),key));
        }

        // macSHA256 returns a SHA-256 based MAC. These are only supported in TLS 1.2
        // so the given version is ignored.
        private static macFunction macSHA256(ushort version, slice<byte> key)
        {
            return new tls10MAC(h:hmac.New(sha256.New,key));
        }

        private partial interface macFunction
        {
            slice<byte> Size(); // MAC appends the MAC of (seq, header, data) to out. The extra data is fed
// into the MAC after obtaining the result to normalize timing. The result
// is only valid until the next invocation of MAC as the buffer is reused.
            slice<byte> MAC(slice<byte> seq, slice<byte> header, slice<byte> data, slice<byte> extra);
        }

        private partial interface aead : cipher.AEAD
        {
            long explicitNonceLen();
        }

        private static readonly long aeadNonceLength = (long)12L;
        private static readonly long noncePrefixLength = (long)4L;


        // prefixNonceAEAD wraps an AEAD and prefixes a fixed portion of the nonce to
        // each call.
        private partial struct prefixNonceAEAD
        {
            public array<byte> nonce;
            public cipher.AEAD aead;
        }

        private static long NonceSize(this ptr<prefixNonceAEAD> _addr_f)
        {
            ref prefixNonceAEAD f = ref _addr_f.val;

            return aeadNonceLength - noncePrefixLength;
        }
        private static long Overhead(this ptr<prefixNonceAEAD> _addr_f)
        {
            ref prefixNonceAEAD f = ref _addr_f.val;

            return f.aead.Overhead();
        }
        private static long explicitNonceLen(this ptr<prefixNonceAEAD> _addr_f)
        {
            ref prefixNonceAEAD f = ref _addr_f.val;

            return f.NonceSize();
        }

        private static slice<byte> Seal(this ptr<prefixNonceAEAD> _addr_f, slice<byte> @out, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData)
        {
            ref prefixNonceAEAD f = ref _addr_f.val;

            copy(f.nonce[4L..], nonce);
            return f.aead.Seal(out, f.nonce[..], plaintext, additionalData);
        }

        private static (slice<byte>, error) Open(this ptr<prefixNonceAEAD> _addr_f, slice<byte> @out, slice<byte> nonce, slice<byte> ciphertext, slice<byte> additionalData)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref prefixNonceAEAD f = ref _addr_f.val;

            copy(f.nonce[4L..], nonce);
            return f.aead.Open(out, f.nonce[..], ciphertext, additionalData);
        }

        // xoredNonceAEAD wraps an AEAD by XORing in a fixed pattern to the nonce
        // before each call.
        private partial struct xorNonceAEAD
        {
            public array<byte> nonceMask;
            public cipher.AEAD aead;
        }

        private static long NonceSize(this ptr<xorNonceAEAD> _addr_f)
        {
            ref xorNonceAEAD f = ref _addr_f.val;

            return 8L;
        } // 64-bit sequence number
        private static long Overhead(this ptr<xorNonceAEAD> _addr_f)
        {
            ref xorNonceAEAD f = ref _addr_f.val;

            return f.aead.Overhead();
        }
        private static long explicitNonceLen(this ptr<xorNonceAEAD> _addr_f)
        {
            ref xorNonceAEAD f = ref _addr_f.val;

            return 0L;
        }

        private static slice<byte> Seal(this ptr<xorNonceAEAD> _addr_f, slice<byte> @out, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData)
        {
            ref xorNonceAEAD f = ref _addr_f.val;

            {
                var i__prev1 = i;
                var b__prev1 = b;

                foreach (var (__i, __b) in nonce)
                {
                    i = __i;
                    b = __b;
                    f.nonceMask[4L + i] ^= b;
                }

                i = i__prev1;
                b = b__prev1;
            }

            var result = f.aead.Seal(out, f.nonceMask[..], plaintext, additionalData);
            {
                var i__prev1 = i;
                var b__prev1 = b;

                foreach (var (__i, __b) in nonce)
                {
                    i = __i;
                    b = __b;
                    f.nonceMask[4L + i] ^= b;
                }

                i = i__prev1;
                b = b__prev1;
            }

            return result;

        }

        private static (slice<byte>, error) Open(this ptr<xorNonceAEAD> _addr_f, slice<byte> @out, slice<byte> nonce, slice<byte> ciphertext, slice<byte> additionalData)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref xorNonceAEAD f = ref _addr_f.val;

            {
                var i__prev1 = i;
                var b__prev1 = b;

                foreach (var (__i, __b) in nonce)
                {
                    i = __i;
                    b = __b;
                    f.nonceMask[4L + i] ^= b;
                }

                i = i__prev1;
                b = b__prev1;
            }

            var (result, err) = f.aead.Open(out, f.nonceMask[..], ciphertext, additionalData);
            {
                var i__prev1 = i;
                var b__prev1 = b;

                foreach (var (__i, __b) in nonce)
                {
                    i = __i;
                    b = __b;
                    f.nonceMask[4L + i] ^= b;
                }

                i = i__prev1;
                b = b__prev1;
            }

            return (result, error.As(err)!);

        }

        private static aead aeadAESGCM(slice<byte> key, slice<byte> noncePrefix) => func((_, panic, __) =>
        {
            if (len(noncePrefix) != noncePrefixLength)
            {
                panic("tls: internal error: wrong nonce length");
            }

            var (aes, err) = aes.NewCipher(key);
            if (err != null)
            {
                panic(err);
            }

            var (aead, err) = cipher.NewGCM(aes);
            if (err != null)
            {
                panic(err);
            }

            ptr<prefixNonceAEAD> ret = addr(new prefixNonceAEAD(aead:aead));
            copy(ret.nonce[..], noncePrefix);
            return ret;

        });

        private static aead aeadAESGCMTLS13(slice<byte> key, slice<byte> nonceMask) => func((_, panic, __) =>
        {
            if (len(nonceMask) != aeadNonceLength)
            {
                panic("tls: internal error: wrong nonce length");
            }

            var (aes, err) = aes.NewCipher(key);
            if (err != null)
            {
                panic(err);
            }

            var (aead, err) = cipher.NewGCM(aes);
            if (err != null)
            {
                panic(err);
            }

            ptr<xorNonceAEAD> ret = addr(new xorNonceAEAD(aead:aead));
            copy(ret.nonceMask[..], nonceMask);
            return ret;

        });

        private static aead aeadChaCha20Poly1305(slice<byte> key, slice<byte> nonceMask) => func((_, panic, __) =>
        {
            if (len(nonceMask) != aeadNonceLength)
            {
                panic("tls: internal error: wrong nonce length");
            }

            var (aead, err) = chacha20poly1305.New(key);
            if (err != null)
            {
                panic(err);
            }

            ptr<xorNonceAEAD> ret = addr(new xorNonceAEAD(aead:aead));
            copy(ret.nonceMask[..], nonceMask);
            return ret;

        });

        private partial interface constantTimeHash : hash.Hash
        {
            slice<byte> ConstantTimeSum(slice<byte> b);
        }

        // cthWrapper wraps any hash.Hash that implements ConstantTimeSum, and replaces
        // with that all calls to Sum. It's used to obtain a ConstantTimeSum-based HMAC.
        private partial struct cthWrapper
        {
            public constantTimeHash h;
        }

        private static long Size(this ptr<cthWrapper> _addr_c)
        {
            ref cthWrapper c = ref _addr_c.val;

            return c.h.Size();
        }
        private static long BlockSize(this ptr<cthWrapper> _addr_c)
        {
            ref cthWrapper c = ref _addr_c.val;

            return c.h.BlockSize();
        }
        private static void Reset(this ptr<cthWrapper> _addr_c)
        {
            ref cthWrapper c = ref _addr_c.val;

            c.h.Reset();
        }
        private static (long, error) Write(this ptr<cthWrapper> _addr_c, slice<byte> p)
        {
            long _p0 = default;
            error _p0 = default!;
            ref cthWrapper c = ref _addr_c.val;

            return c.h.Write(p);
        }
        private static slice<byte> Sum(this ptr<cthWrapper> _addr_c, slice<byte> b)
        {
            ref cthWrapper c = ref _addr_c.val;

            return c.h.ConstantTimeSum(b);
        }

        private static Func<hash.Hash> newConstantTimeHash(Func<hash.Hash> h)
        {
            return () =>
            {
                return addr(new cthWrapper(h().(constantTimeHash)));
            };

        }

        // tls10MAC implements the TLS 1.0 MAC function. RFC 2246, Section 6.2.3.
        private partial struct tls10MAC
        {
            public hash.Hash h;
            public slice<byte> buf;
        }

        private static long Size(this tls10MAC s)
        {
            return s.h.Size();
        }

        // MAC is guaranteed to take constant time, as long as
        // len(seq)+len(header)+len(data)+len(extra) is constant. extra is not fed into
        // the MAC, but is only provided to make the timing profile constant.
        private static slice<byte> MAC(this tls10MAC s, slice<byte> seq, slice<byte> header, slice<byte> data, slice<byte> extra)
        {
            s.h.Reset();
            s.h.Write(seq);
            s.h.Write(header);
            s.h.Write(data);
            var res = s.h.Sum(s.buf[..0L]);
            if (extra != null)
            {
                s.h.Write(extra);
            }

            return res;

        }

        private static keyAgreement rsaKA(ushort version)
        {
            return new rsaKeyAgreement();
        }

        private static keyAgreement ecdheECDSAKA(ushort version)
        {
            return addr(new ecdheKeyAgreement(isRSA:false,version:version,));
        }

        private static keyAgreement ecdheRSAKA(ushort version)
        {
            return addr(new ecdheKeyAgreement(isRSA:true,version:version,));
        }

        // mutualCipherSuite returns a cipherSuite given a list of supported
        // ciphersuites and the id requested by the peer.
        private static ptr<cipherSuite> mutualCipherSuite(slice<ushort> have, ushort want)
        {
            foreach (var (_, id) in have)
            {
                if (id == want)
                {
                    return _addr_cipherSuiteByID(id)!;
                }

            }
            return _addr_null!;

        }

        private static ptr<cipherSuite> cipherSuiteByID(ushort id)
        {
            foreach (var (_, cipherSuite) in cipherSuites)
            {
                if (cipherSuite.id == id)
                {
                    return _addr_cipherSuite!;
                }

            }
            return _addr_null!;

        }

        private static ptr<cipherSuiteTLS13> mutualCipherSuiteTLS13(slice<ushort> have, ushort want)
        {
            foreach (var (_, id) in have)
            {
                if (id == want)
                {
                    return _addr_cipherSuiteTLS13ByID(id)!;
                }

            }
            return _addr_null!;

        }

        private static ptr<cipherSuiteTLS13> cipherSuiteTLS13ByID(ushort id)
        {
            foreach (var (_, cipherSuite) in cipherSuitesTLS13)
            {
                if (cipherSuite.id == id)
                {
                    return _addr_cipherSuite!;
                }

            }
            return _addr_null!;

        }

        // A list of cipher suite IDs that are, or have been, implemented by this
        // package.
        //
        // See https://www.iana.org/assignments/tls-parameters/tls-parameters.xml
 
        // TLS 1.0 - 1.2 cipher suites.
        public static readonly ushort TLS_RSA_WITH_RC4_128_SHA = (ushort)0x0005UL;
        public static readonly ushort TLS_RSA_WITH_3DES_EDE_CBC_SHA = (ushort)0x000aUL;
        public static readonly ushort TLS_RSA_WITH_AES_128_CBC_SHA = (ushort)0x002fUL;
        public static readonly ushort TLS_RSA_WITH_AES_256_CBC_SHA = (ushort)0x0035UL;
        public static readonly ushort TLS_RSA_WITH_AES_128_CBC_SHA256 = (ushort)0x003cUL;
        public static readonly ushort TLS_RSA_WITH_AES_128_GCM_SHA256 = (ushort)0x009cUL;
        public static readonly ushort TLS_RSA_WITH_AES_256_GCM_SHA384 = (ushort)0x009dUL;
        public static readonly ushort TLS_ECDHE_ECDSA_WITH_RC4_128_SHA = (ushort)0xc007UL;
        public static readonly ushort TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA = (ushort)0xc009UL;
        public static readonly ushort TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA = (ushort)0xc00aUL;
        public static readonly ushort TLS_ECDHE_RSA_WITH_RC4_128_SHA = (ushort)0xc011UL;
        public static readonly ushort TLS_ECDHE_RSA_WITH_3DES_EDE_CBC_SHA = (ushort)0xc012UL;
        public static readonly ushort TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA = (ushort)0xc013UL;
        public static readonly ushort TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA = (ushort)0xc014UL;
        public static readonly ushort TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256 = (ushort)0xc023UL;
        public static readonly ushort TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256 = (ushort)0xc027UL;
        public static readonly ushort TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256 = (ushort)0xc02fUL;
        public static readonly ushort TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256 = (ushort)0xc02bUL;
        public static readonly ushort TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384 = (ushort)0xc030UL;
        public static readonly ushort TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384 = (ushort)0xc02cUL;
        public static readonly ushort TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256 = (ushort)0xcca8UL;
        public static readonly ushort TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305_SHA256 = (ushort)0xcca9UL; 

        // TLS 1.3 cipher suites.
        public static readonly ushort TLS_AES_128_GCM_SHA256 = (ushort)0x1301UL;
        public static readonly ushort TLS_AES_256_GCM_SHA384 = (ushort)0x1302UL;
        public static readonly ushort TLS_CHACHA20_POLY1305_SHA256 = (ushort)0x1303UL; 

        // TLS_FALLBACK_SCSV isn't a standard cipher suite but an indicator
        // that the client is doing version fallback. See RFC 7507.
        public static readonly ushort TLS_FALLBACK_SCSV = (ushort)0x5600UL; 

        // Legacy names for the corresponding cipher suites with the correct _SHA256
        // suffix, retained for backward compatibility.
        public static readonly var TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305 = TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256;
        public static readonly var TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305 = TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305_SHA256;

    }
}}
