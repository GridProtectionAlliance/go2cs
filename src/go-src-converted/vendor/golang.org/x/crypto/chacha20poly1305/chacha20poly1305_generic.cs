// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package chacha20poly1305 -- go2cs converted at 2020 October 08 04:59:55 UTC
// import "vendor/golang.org/x/crypto/chacha20poly1305" ==> using chacha20poly1305 = go.vendor.golang.org.x.crypto.chacha20poly1305_package
// Original source: C:\Go\src\vendor\golang.org\x\crypto\chacha20poly1305\chacha20poly1305_generic.go
using binary = go.encoding.binary_package;

using chacha20 = go.golang.org.x.crypto.chacha20_package;
using subtle = go.golang.org.x.crypto.@internal.subtle_package;
using poly1305 = go.golang.org.x.crypto.poly1305_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace crypto
{
    public static partial class chacha20poly1305_package
    {
        private static void writeWithPadding(ptr<poly1305.MAC> _addr_p, slice<byte> b)
        {
            ref poly1305.MAC p = ref _addr_p.val;

            p.Write(b);
            {
                var rem = len(b) % 16L;

                if (rem != 0L)
                {
                    array<byte> buf = new array<byte>(16L);
                    long padLen = 16L - rem;
                    p.Write(buf[..padLen]);
                }
            }

        }

        private static void writeUint64(ptr<poly1305.MAC> _addr_p, long n)
        {
            ref poly1305.MAC p = ref _addr_p.val;

            array<byte> buf = new array<byte>(8L);
            binary.LittleEndian.PutUint64(buf[..], uint64(n));
            p.Write(buf[..]);
        }

        private static slice<byte> sealGeneric(this ptr<chacha20poly1305> _addr_c, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData) => func((_, panic, __) =>
        {
            ref chacha20poly1305 c = ref _addr_c.val;

            var (ret, out) = sliceForAppend(dst, len(plaintext) + poly1305.TagSize);
            var ciphertext = out[..len(plaintext)];
            var tag = out[len(plaintext)..];
            if (subtle.InexactOverlap(out, plaintext))
            {
                panic("chacha20poly1305: invalid buffer overlap");
            }

            ref array<byte> polyKey = ref heap(new array<byte>(32L), out ptr<array<byte>> _addr_polyKey);
            var (s, _) = chacha20.NewUnauthenticatedCipher(c.key[..], nonce);
            s.XORKeyStream(polyKey[..], polyKey[..]);
            s.SetCounter(1L); // set the counter to 1, skipping 32 bytes
            s.XORKeyStream(ciphertext, plaintext);

            var p = poly1305.New(_addr_polyKey);
            writeWithPadding(_addr_p, additionalData);
            writeWithPadding(_addr_p, ciphertext);
            writeUint64(_addr_p, len(additionalData));
            writeUint64(_addr_p, len(plaintext));
            p.Sum(tag[..0L]);

            return ret;

        });

        private static (slice<byte>, error) openGeneric(this ptr<chacha20poly1305> _addr_c, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> additionalData) => func((_, panic, __) =>
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref chacha20poly1305 c = ref _addr_c.val;

            var tag = ciphertext[len(ciphertext) - 16L..];
            ciphertext = ciphertext[..len(ciphertext) - 16L];

            ref array<byte> polyKey = ref heap(new array<byte>(32L), out ptr<array<byte>> _addr_polyKey);
            var (s, _) = chacha20.NewUnauthenticatedCipher(c.key[..], nonce);
            s.XORKeyStream(polyKey[..], polyKey[..]);
            s.SetCounter(1L); // set the counter to 1, skipping 32 bytes

            var p = poly1305.New(_addr_polyKey);
            writeWithPadding(_addr_p, additionalData);
            writeWithPadding(_addr_p, ciphertext);
            writeUint64(_addr_p, len(additionalData));
            writeUint64(_addr_p, len(ciphertext));

            var (ret, out) = sliceForAppend(dst, len(ciphertext));
            if (subtle.InexactOverlap(out, ciphertext))
            {
                panic("chacha20poly1305: invalid buffer overlap");
            }

            if (!p.Verify(tag))
            {
                foreach (var (i) in out)
                {
                    out[i] = 0L;
                }
                return (null, error.As(errOpen)!);

            }

            s.XORKeyStream(out, ciphertext);
            return (ret, error.As(null!)!);

        });
    }
}}}}}
