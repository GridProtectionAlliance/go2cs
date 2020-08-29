// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package chacha20poly1305 -- go2cs converted at 2020 August 29 10:11:13 UTC
// import "vendor/golang_org/x/crypto/chacha20poly1305" ==> using chacha20poly1305 = go.vendor.golang_org.x.crypto.chacha20poly1305_package
// Original source: C:\Go\src\vendor\golang_org\x\crypto\chacha20poly1305\chacha20poly1305_generic.go
using binary = go.encoding.binary_package;

using chacha20 = go.golang_org.x.crypto.chacha20poly1305.@internal.chacha20_package;
using poly1305 = go.golang_org.x.crypto.poly1305_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace crypto
{
    public static partial class chacha20poly1305_package
    {
        private static long roundTo16(long n)
        {
            return 16L * ((n + 15L) / 16L);
        }

        private static slice<byte> sealGeneric(this ref chacha20poly1305 c, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData)
        {
            array<byte> counter = new array<byte>(16L);
            copy(counter[4L..], nonce);

            array<byte> polyKey = new array<byte>(32L);
            chacha20.XORKeyStream(polyKey[..], polyKey[..], ref counter, ref c.key);

            var (ret, out) = sliceForAppend(dst, len(plaintext) + poly1305.TagSize);
            counter[0L] = 1L;
            chacha20.XORKeyStream(out, plaintext, ref counter, ref c.key);

            var polyInput = make_slice<byte>(roundTo16(len(additionalData)) + roundTo16(len(plaintext)) + 8L + 8L);
            copy(polyInput, additionalData);
            copy(polyInput[roundTo16(len(additionalData))..], out[..len(plaintext)]);
            binary.LittleEndian.PutUint64(polyInput[len(polyInput) - 16L..], uint64(len(additionalData)));
            binary.LittleEndian.PutUint64(polyInput[len(polyInput) - 8L..], uint64(len(plaintext)));

            array<byte> tag = new array<byte>(poly1305.TagSize);
            poly1305.Sum(ref tag, polyInput, ref polyKey);
            copy(out[len(plaintext)..], tag[..]);

            return ret;
        }

        private static (slice<byte>, error) openGeneric(this ref chacha20poly1305 c, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> additionalData)
        {
            array<byte> tag = new array<byte>(poly1305.TagSize);
            copy(tag[..], ciphertext[len(ciphertext) - 16L..]);
            ciphertext = ciphertext[..len(ciphertext) - 16L];

            array<byte> counter = new array<byte>(16L);
            copy(counter[4L..], nonce);

            array<byte> polyKey = new array<byte>(32L);
            chacha20.XORKeyStream(polyKey[..], polyKey[..], ref counter, ref c.key);

            var polyInput = make_slice<byte>(roundTo16(len(additionalData)) + roundTo16(len(ciphertext)) + 8L + 8L);
            copy(polyInput, additionalData);
            copy(polyInput[roundTo16(len(additionalData))..], ciphertext);
            binary.LittleEndian.PutUint64(polyInput[len(polyInput) - 16L..], uint64(len(additionalData)));
            binary.LittleEndian.PutUint64(polyInput[len(polyInput) - 8L..], uint64(len(ciphertext)));

            var (ret, out) = sliceForAppend(dst, len(ciphertext));
            if (!poly1305.Verify(ref tag, polyInput, ref polyKey))
            {
                foreach (var (i) in out)
                {
                    out[i] = 0L;
                }
                return (null, errOpen);
            }
            counter[0L] = 1L;
            chacha20.XORKeyStream(out, ciphertext, ref counter, ref c.key);
            return (ret, null);
        }
    }
}}}}}
