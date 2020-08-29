// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package tls -- go2cs converted at 2020 August 29 08:28:30 UTC
// import "crypto/tls" ==> using tls = go.crypto.tls_package
// Original source: C:\Go\src\crypto\tls\cipher_suites.go
using aes = go.crypto.aes_package;
using cipher = go.crypto.cipher_package;
using des = go.crypto.des_package;
using hmac = go.crypto.hmac_package;
using rc4 = go.crypto.rc4_package;
using sha1 = go.crypto.sha1_package;
using sha256 = go.crypto.sha256_package;
using x509 = go.crypto.x509_package;
using hash = go.hash_package;

using chacha20poly1305 = go.golang_org.x.crypto.chacha20poly1305_package;
using static go.builtin;
using System;

namespace go {
namespace crypto
{
    public static partial class tls_package
    {
        // a keyAgreement implements the client and server side of a TLS key agreement
        // protocol by generating and processing key exchange messages.
        private partial interface keyAgreement
        {
            (slice<byte>, ref clientKeyExchangeMsg, error) generateServerKeyExchange(ref Config _p0, ref Certificate _p0, ref clientHelloMsg _p0, ref serverHelloMsg _p0);
            (slice<byte>, ref clientKeyExchangeMsg, error) processClientKeyExchange(ref Config _p0, ref Certificate _p0, ref clientKeyExchangeMsg _p0, ushort _p0); // On the client side, the next two methods are called in order.

// This method may not be called if the server doesn't send a
// ServerKeyExchange message.
            (slice<byte>, ref clientKeyExchangeMsg, error) processServerKeyExchange(ref Config _p0, ref clientHelloMsg _p0, ref serverHelloMsg _p0, ref x509.Certificate _p0, ref serverKeyExchangeMsg _p0);
            (slice<byte>, ref clientKeyExchangeMsg, error) generateClientKeyExchange(ref Config _p0, ref clientHelloMsg _p0, ref x509.Certificate _p0);
        }

 
        // suiteECDH indicates that the cipher suite involves elliptic curve
        // Diffie-Hellman. This means that it should only be selected when the
        // client indicates that it supports ECC with a curve and point format
        // that we're happy with.
        private static readonly long suiteECDHE = 1L << (int)(iota); 
        // suiteECDSA indicates that the cipher suite involves an ECDSA
        // signature and therefore may only be selected when the server's
        // certificate is ECDSA. If this is not set then the cipher suite is
        // RSA based.
        private static readonly var suiteECDSA = 0; 
        // suiteTLS12 indicates that the cipher suite should only be advertised
        // and accepted when using TLS 1.2.
        private static readonly var suiteTLS12 = 1; 
        // suiteSHA384 indicates that the cipher suite uses SHA384 as the
        // handshake hash.
        private static readonly var suiteSHA384 = 2; 
        // suiteDefaultOff indicates that this cipher suite is not included by
        // default.
        private static readonly var suiteDefaultOff = 3;

        // A cipherSuite is a specific combination of key agreement, cipher and MAC
        // function. All cipher suites currently assume RSA key agreement.
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
            public Func<slice<byte>, slice<byte>, cipher.AEAD> aead;
        }

        private static ref cipherSuite cipherSuites = new slice<ref cipherSuite>(new ref cipherSuite[] { {TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305,32,0,12,ecdheRSAKA,suiteECDHE|suiteTLS12,nil,nil,aeadChaCha20Poly1305}, {TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305,32,0,12,ecdheECDSAKA,suiteECDHE|suiteECDSA|suiteTLS12,nil,nil,aeadChaCha20Poly1305}, {TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,16,0,4,ecdheRSAKA,suiteECDHE|suiteTLS12,nil,nil,aeadAESGCM}, {TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256,16,0,4,ecdheECDSAKA,suiteECDHE|suiteECDSA|suiteTLS12,nil,nil,aeadAESGCM}, {TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384,32,0,4,ecdheRSAKA,suiteECDHE|suiteTLS12|suiteSHA384,nil,nil,aeadAESGCM}, {TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384,32,0,4,ecdheECDSAKA,suiteECDHE|suiteECDSA|suiteTLS12|suiteSHA384,nil,nil,aeadAESGCM}, {TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256,16,32,16,ecdheRSAKA,suiteECDHE|suiteTLS12|suiteDefaultOff,cipherAES,macSHA256,nil}, {TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA,16,20,16,ecdheRSAKA,suiteECDHE,cipherAES,macSHA1,nil}, {TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256,16,32,16,ecdheECDSAKA,suiteECDHE|suiteECDSA|suiteTLS12|suiteDefaultOff,cipherAES,macSHA256,nil}, {TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA,16,20,16,ecdheECDSAKA,suiteECDHE|suiteECDSA,cipherAES,macSHA1,nil}, {TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA,32,20,16,ecdheRSAKA,suiteECDHE,cipherAES,macSHA1,nil}, {TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA,32,20,16,ecdheECDSAKA,suiteECDHE|suiteECDSA,cipherAES,macSHA1,nil}, {TLS_RSA_WITH_AES_128_GCM_SHA256,16,0,4,rsaKA,suiteTLS12,nil,nil,aeadAESGCM}, {TLS_RSA_WITH_AES_256_GCM_SHA384,32,0,4,rsaKA,suiteTLS12|suiteSHA384,nil,nil,aeadAESGCM}, {TLS_RSA_WITH_AES_128_CBC_SHA256,16,32,16,rsaKA,suiteTLS12|suiteDefaultOff,cipherAES,macSHA256,nil}, {TLS_RSA_WITH_AES_128_CBC_SHA,16,20,16,rsaKA,0,cipherAES,macSHA1,nil}, {TLS_RSA_WITH_AES_256_CBC_SHA,32,20,16,rsaKA,0,cipherAES,macSHA1,nil}, {TLS_ECDHE_RSA_WITH_3DES_EDE_CBC_SHA,24,20,8,ecdheRSAKA,suiteECDHE,cipher3DES,macSHA1,nil}, {TLS_RSA_WITH_3DES_EDE_CBC_SHA,24,20,8,rsaKA,0,cipher3DES,macSHA1,nil}, {TLS_RSA_WITH_RC4_128_SHA,16,20,0,rsaKA,suiteDefaultOff,cipherRC4,macSHA1,nil}, {TLS_ECDHE_RSA_WITH_RC4_128_SHA,16,20,0,ecdheRSAKA,suiteECDHE|suiteDefaultOff,cipherRC4,macSHA1,nil}, {TLS_ECDHE_ECDSA_WITH_RC4_128_SHA,16,20,0,ecdheECDSAKA,suiteECDHE|suiteECDSA|suiteDefaultOff,cipherRC4,macSHA1,nil} });

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
            if (version == VersionSSL30)
            {
                ssl30MAC mac = new ssl30MAC(h:sha1.New(),key:make([]byte,len(key)),);
                copy(mac.key, key);
                return mac;
            }
            return new tls10MAC(hmac.New(newConstantTimeHash(sha1.New),key));
        }

        // macSHA256 returns a SHA-256 based MAC. These are only supported in TLS 1.2
        // so the given version is ignored.
        private static macFunction macSHA256(ushort version, slice<byte> key)
        {
            return new tls10MAC(hmac.New(sha256.New,key));
        }

        private partial interface macFunction
        {
            slice<byte> Size();
            slice<byte> MAC(slice<byte> digestBuf, slice<byte> seq, slice<byte> header, slice<byte> data, slice<byte> extra);
        }

        private partial interface aead : cipher.AEAD
        {
            long explicitNonceLen();
        }

        // fixedNonceAEAD wraps an AEAD and prefixes a fixed portion of the nonce to
        // each call.
        private partial struct fixedNonceAEAD
        {
            public array<byte> nonce;
            public cipher.AEAD aead;
        }

        private static long NonceSize(this ref fixedNonceAEAD f)
        {
            return 8L;
        }
        private static long Overhead(this ref fixedNonceAEAD f)
        {
            return f.aead.Overhead();
        }
        private static long explicitNonceLen(this ref fixedNonceAEAD f)
        {
            return 8L;
        }

        private static slice<byte> Seal(this ref fixedNonceAEAD f, slice<byte> @out, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData)
        {
            copy(f.nonce[4L..], nonce);
            return f.aead.Seal(out, f.nonce[..], plaintext, additionalData);
        }

        private static (slice<byte>, error) Open(this ref fixedNonceAEAD f, slice<byte> @out, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData)
        {
            copy(f.nonce[4L..], nonce);
            return f.aead.Open(out, f.nonce[..], plaintext, additionalData);
        }

        // xoredNonceAEAD wraps an AEAD by XORing in a fixed pattern to the nonce
        // before each call.
        private partial struct xorNonceAEAD
        {
            public array<byte> nonceMask;
            public cipher.AEAD aead;
        }

        private static long NonceSize(this ref xorNonceAEAD f)
        {
            return 8L;
        }
        private static long Overhead(this ref xorNonceAEAD f)
        {
            return f.aead.Overhead();
        }
        private static long explicitNonceLen(this ref xorNonceAEAD f)
        {
            return 0L;
        }

        private static slice<byte> Seal(this ref xorNonceAEAD f, slice<byte> @out, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData)
        {
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

        private static (slice<byte>, error) Open(this ref xorNonceAEAD f, slice<byte> @out, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData)
        {
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

            var (result, err) = f.aead.Open(out, f.nonceMask[..], plaintext, additionalData);
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

            return (result, err);
        }

        private static cipher.AEAD aeadAESGCM(slice<byte> key, slice<byte> fixedNonce) => func((_, panic, __) =>
        {
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
            fixedNonceAEAD ret = ref new fixedNonceAEAD(aead:aead);
            copy(ret.nonce[..], fixedNonce);
            return ret;
        });

        private static cipher.AEAD aeadChaCha20Poly1305(slice<byte> key, slice<byte> fixedNonce) => func((_, panic, __) =>
        {
            var (aead, err) = chacha20poly1305.New(key);
            if (err != null)
            {
                panic(err);
            }
            xorNonceAEAD ret = ref new xorNonceAEAD(aead:aead);
            copy(ret.nonceMask[..], fixedNonce);
            return ret;
        });

        // ssl30MAC implements the SSLv3 MAC function, as defined in
        // www.mozilla.org/projects/security/pki/nss/ssl/draft302.txt section 5.2.3.1
        private partial struct ssl30MAC
        {
            public hash.Hash h;
            public slice<byte> key;
        }

        private static long Size(this ssl30MAC s)
        {
            return s.h.Size();
        }

        private static array<byte> ssl30Pad1 = new array<byte>(new byte[] { 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36, 0x36 });

        private static array<byte> ssl30Pad2 = new array<byte>(new byte[] { 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c, 0x5c });

        // MAC does not offer constant timing guarantees for SSL v3.0, since it's deemed
        // useless considering the similar, protocol-level POODLE vulnerability.
        private static slice<byte> MAC(this ssl30MAC s, slice<byte> digestBuf, slice<byte> seq, slice<byte> header, slice<byte> data, slice<byte> extra)
        {
            long padLength = 48L;
            if (s.h.Size() == 20L)
            {
                padLength = 40L;
            }
            s.h.Reset();
            s.h.Write(s.key);
            s.h.Write(ssl30Pad1[..padLength]);
            s.h.Write(seq);
            s.h.Write(header[..1L]);
            s.h.Write(header[3L..5L]);
            s.h.Write(data);
            digestBuf = s.h.Sum(digestBuf[..0L]);

            s.h.Reset();
            s.h.Write(s.key);
            s.h.Write(ssl30Pad2[..padLength]);
            s.h.Write(digestBuf);
            return s.h.Sum(digestBuf[..0L]);
        }

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

        private static long Size(this ref cthWrapper c)
        {
            return c.h.Size();
        }
        private static long BlockSize(this ref cthWrapper c)
        {
            return c.h.BlockSize();
        }
        private static void Reset(this ref cthWrapper c)
        {
            c.h.Reset();

        }
        private static (long, error) Write(this ref cthWrapper c, slice<byte> p)
        {
            return c.h.Write(p);
        }
        private static slice<byte> Sum(this ref cthWrapper c, slice<byte> b)
        {
            return c.h.ConstantTimeSum(b);
        }

        private static Func<hash.Hash> newConstantTimeHash(Func<hash.Hash> h)
        {
            return () =>
            {
                return ref new cthWrapper(h().(constantTimeHash));
            }
;
        }

        // tls10MAC implements the TLS 1.0 MAC function. RFC 2246, section 6.2.3.
        private partial struct tls10MAC
        {
            public hash.Hash h;
        }

        private static long Size(this tls10MAC s)
        {
            return s.h.Size();
        }

        // MAC is guaranteed to take constant time, as long as
        // len(seq)+len(header)+len(data)+len(extra) is constant. extra is not fed into
        // the MAC, but is only provided to make the timing profile constant.
        private static slice<byte> MAC(this tls10MAC s, slice<byte> digestBuf, slice<byte> seq, slice<byte> header, slice<byte> data, slice<byte> extra)
        {
            s.h.Reset();
            s.h.Write(seq);
            s.h.Write(header);
            s.h.Write(data);
            var res = s.h.Sum(digestBuf[..0L]);
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
            return ref new ecdheKeyAgreement(sigType:signatureECDSA,version:version,);
        }

        private static keyAgreement ecdheRSAKA(ushort version)
        {
            return ref new ecdheKeyAgreement(sigType:signatureRSA,version:version,);
        }

        // mutualCipherSuite returns a cipherSuite given a list of supported
        // ciphersuites and the id requested by the peer.
        private static ref cipherSuite mutualCipherSuite(slice<ushort> have, ushort want)
        {
            foreach (var (_, id) in have)
            {
                if (id == want)
                {
                    foreach (var (_, suite) in cipherSuites)
                    {
                        if (suite.id == want)
                        {
                            return suite;
                        }
                    }
                    return null;
                }
            }
            return null;
        }

        // A list of cipher suite IDs that are, or have been, implemented by this
        // package.
        //
        // Taken from http://www.iana.org/assignments/tls-parameters/tls-parameters.xml
        public static readonly ushort TLS_RSA_WITH_RC4_128_SHA = 0x0005UL;
        public static readonly ushort TLS_RSA_WITH_3DES_EDE_CBC_SHA = 0x000aUL;
        public static readonly ushort TLS_RSA_WITH_AES_128_CBC_SHA = 0x002fUL;
        public static readonly ushort TLS_RSA_WITH_AES_256_CBC_SHA = 0x0035UL;
        public static readonly ushort TLS_RSA_WITH_AES_128_CBC_SHA256 = 0x003cUL;
        public static readonly ushort TLS_RSA_WITH_AES_128_GCM_SHA256 = 0x009cUL;
        public static readonly ushort TLS_RSA_WITH_AES_256_GCM_SHA384 = 0x009dUL;
        public static readonly ushort TLS_ECDHE_ECDSA_WITH_RC4_128_SHA = 0xc007UL;
        public static readonly ushort TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA = 0xc009UL;
        public static readonly ushort TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA = 0xc00aUL;
        public static readonly ushort TLS_ECDHE_RSA_WITH_RC4_128_SHA = 0xc011UL;
        public static readonly ushort TLS_ECDHE_RSA_WITH_3DES_EDE_CBC_SHA = 0xc012UL;
        public static readonly ushort TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA = 0xc013UL;
        public static readonly ushort TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA = 0xc014UL;
        public static readonly ushort TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256 = 0xc023UL;
        public static readonly ushort TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256 = 0xc027UL;
        public static readonly ushort TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256 = 0xc02fUL;
        public static readonly ushort TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256 = 0xc02bUL;
        public static readonly ushort TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384 = 0xc030UL;
        public static readonly ushort TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384 = 0xc02cUL;
        public static readonly ushort TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305 = 0xcca8UL;
        public static readonly ushort TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305 = 0xcca9UL; 

        // TLS_FALLBACK_SCSV isn't a standard cipher suite but an indicator
        // that the client is doing version fallback. See
        // https://tools.ietf.org/html/rfc7507.
        public static readonly ushort TLS_FALLBACK_SCSV = 0x5600UL;
    }
}}
