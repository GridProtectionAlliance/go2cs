// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// CFB (Cipher Feedback) Mode.

// package cipher -- go2cs converted at 2020 October 08 03:35:43 UTC
// import "crypto/cipher" ==> using cipher = go.crypto.cipher_package
// Original source: C:\Go\src\crypto\cipher\cfb.go
using subtle = go.crypto.@internal.subtle_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class cipher_package
    {
        private partial struct cfb
        {
            public Block b;
            public slice<byte> next;
            public slice<byte> @out;
            public long outUsed;
            public bool decrypt;
        }

        private static void XORKeyStream(this ptr<cfb> _addr_x, slice<byte> dst, slice<byte> src) => func((_, panic, __) =>
        {
            ref cfb x = ref _addr_x.val;

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
                if (x.outUsed == len(x.@out))
                {
                    x.b.Encrypt(x.@out, x.next);
                    x.outUsed = 0L;
                }

                if (x.decrypt)
                { 
                    // We can precompute a larger segment of the
                    // keystream on decryption. This will allow
                    // larger batches for xor, and we should be
                    // able to match CTR/OFB performance.
                    copy(x.next[x.outUsed..], src);

                }

                var n = xorBytes(dst, src, x.@out[x.outUsed..]);
                if (!x.decrypt)
                {
                    copy(x.next[x.outUsed..], dst);
                }

                dst = dst[n..];
                src = src[n..];
                x.outUsed += n;

            }


        });

        // NewCFBEncrypter returns a Stream which encrypts with cipher feedback mode,
        // using the given Block. The iv must be the same length as the Block's block
        // size.
        public static Stream NewCFBEncrypter(Block block, slice<byte> iv)
        {
            return newCFB(block, iv, false);
        }

        // NewCFBDecrypter returns a Stream which decrypts with cipher feedback mode,
        // using the given Block. The iv must be the same length as the Block's block
        // size.
        public static Stream NewCFBDecrypter(Block block, slice<byte> iv)
        {
            return newCFB(block, iv, true);
        }

        private static Stream newCFB(Block block, slice<byte> iv, bool decrypt) => func((_, panic, __) =>
        {
            var blockSize = block.BlockSize();
            if (len(iv) != blockSize)
            { 
                // stack trace will indicate whether it was de or encryption
                panic("cipher.newCFB: IV length must equal block size");

            }

            ptr<cfb> x = addr(new cfb(b:block,out:make([]byte,blockSize),next:make([]byte,blockSize),outUsed:blockSize,decrypt:decrypt,));
            copy(x.next, iv);

            return x;

        });
    }
}}
