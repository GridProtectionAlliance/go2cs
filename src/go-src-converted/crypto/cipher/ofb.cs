// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// OFB (Output Feedback) Mode.

// package cipher -- go2cs converted at 2020 October 08 03:35:45 UTC
// import "crypto/cipher" ==> using cipher = go.crypto.cipher_package
// Original source: C:\Go\src\crypto\cipher\ofb.go
using subtle = go.crypto.@internal.subtle_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class cipher_package
    {
        private partial struct ofb
        {
            public Block b;
            public slice<byte> cipher;
            public slice<byte> @out;
            public long outUsed;
        }

        // NewOFB returns a Stream that encrypts or decrypts using the block cipher b
        // in output feedback mode. The initialization vector iv's length must be equal
        // to b's block size.
        public static Stream NewOFB(Block b, slice<byte> iv) => func((_, panic, __) =>
        {
            var blockSize = b.BlockSize();
            if (len(iv) != blockSize)
            {
                panic("cipher.NewOFB: IV length must equal block size");
            }

            var bufSize = streamBufferSize;
            if (bufSize < blockSize)
            {
                bufSize = blockSize;
            }

            ptr<ofb> x = addr(new ofb(b:b,cipher:make([]byte,blockSize),out:make([]byte,0,bufSize),outUsed:0,));

            copy(x.cipher, iv);
            return x;

        });

        private static void refill(this ptr<ofb> _addr_x)
        {
            ref ofb x = ref _addr_x.val;

            var bs = x.b.BlockSize();
            var remain = len(x.@out) - x.outUsed;
            if (remain > x.outUsed)
            {
                return ;
            }

            copy(x.@out, x.@out[x.outUsed..]);
            x.@out = x.@out[..cap(x.@out)];
            while (remain < len(x.@out) - bs)
            {
                x.b.Encrypt(x.cipher, x.cipher);
                copy(x.@out[remain..], x.cipher);
                remain += bs;
            }

            x.@out = x.@out[..remain];
            x.outUsed = 0L;

        }

        private static void XORKeyStream(this ptr<ofb> _addr_x, slice<byte> dst, slice<byte> src) => func((_, panic, __) =>
        {
            ref ofb x = ref _addr_x.val;

            if (len(dst) < len(src))
            {
                panic("crypto/cipher: output smaller than input");
            }

            if (subtle.InexactOverlap(dst[..len(src)], src))
            {
                panic("crypto/cipher: invalid buffer overlap");
            }

            while (len(src) > 0L)
            {
                if (x.outUsed >= len(x.@out) - x.b.BlockSize())
                {
                    x.refill();
                }

                var n = xorBytes(dst, src, x.@out[x.outUsed..]);
                dst = dst[n..];
                src = src[n..];
                x.outUsed += n;

            }


        });
    }
}}
