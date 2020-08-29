// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package chacha20poly1305 implements the ChaCha20-Poly1305 AEAD as specified in RFC 7539.
// package chacha20poly1305 -- go2cs converted at 2020 August 29 10:11:12 UTC
// import "vendor/golang_org/x/crypto/chacha20poly1305" ==> using chacha20poly1305 = go.vendor.golang_org.x.crypto.chacha20poly1305_package
// Original source: C:\Go\src\vendor\golang_org\x\crypto\chacha20poly1305\chacha20poly1305.go
// import "golang.org/x/crypto/chacha20poly1305"

using cipher = go.crypto.cipher_package;
using errors = go.errors_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace crypto
{
    public static partial class chacha20poly1305_package
    {
 
        // KeySize is the size of the key used by this AEAD, in bytes.
        public static readonly long KeySize = 32L; 
        // NonceSize is the size of the nonce used with this AEAD, in bytes.
        public static readonly long NonceSize = 12L;

        private partial struct chacha20poly1305
        {
            public array<byte> key;
        }

        // New returns a ChaCha20-Poly1305 AEAD that uses the given, 256-bit key.
        public static (cipher.AEAD, error) New(slice<byte> key)
        {
            if (len(key) != KeySize)
            {
                return (null, errors.New("chacha20poly1305: bad key length"));
            }
            ptr<object> ret = @new<chacha20poly1305>();
            copy(ret.key[..], key);
            return (ret, null);
        }

        private static long NonceSize(this ref chacha20poly1305 c)
        {
            return NonceSize;
        }

        private static long Overhead(this ref chacha20poly1305 c)
        {
            return 16L;
        }

        private static slice<byte> Seal(this ref chacha20poly1305 _c, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData) => func(_c, (ref chacha20poly1305 c, Defer _, Panic panic, Recover __) =>
        {
            if (len(nonce) != NonceSize)
            {
                panic("chacha20poly1305: bad nonce length passed to Seal");
            }
            if (uint64(len(plaintext)) > (1L << (int)(38L)) - 64L)
            {
                panic("chacha20poly1305: plaintext too large");
            }
            return c.seal(dst, nonce, plaintext, additionalData);
        });

        private static var errOpen = errors.New("chacha20poly1305: message authentication failed");

        private static (slice<byte>, error) Open(this ref chacha20poly1305 _c, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> additionalData) => func(_c, (ref chacha20poly1305 c, Defer _, Panic panic, Recover __) =>
        {
            if (len(nonce) != NonceSize)
            {
                panic("chacha20poly1305: bad nonce length passed to Open");
            }
            if (len(ciphertext) < 16L)
            {
                return (null, errOpen);
            }
            if (uint64(len(ciphertext)) > (1L << (int)(38L)) - 48L)
            {
                panic("chacha20poly1305: ciphertext too large");
            }
            return c.open(dst, nonce, ciphertext, additionalData);
        });

        // sliceForAppend takes a slice and a requested number of bytes. It returns a
        // slice with the contents of the given slice followed by that many bytes and a
        // second slice that aliases into it and contains only the extra bytes. If the
        // original slice has sufficient capacity then no allocation is performed.
        private static (slice<byte>, slice<byte>) sliceForAppend(slice<byte> @in, long n)
        {
            {
                var total = len(in) + n;

                if (cap(in) >= total)
                {
                    head = in[..total];
                }
                else
                {
                    head = make_slice<byte>(total);
                    copy(head, in);
                }

            }
            tail = head[len(in)..];
            return;
        }
    }
}}}}}
