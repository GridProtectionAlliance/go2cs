// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package aes -- go2cs converted at 2020 October 08 03:35:50 UTC
// import "crypto/aes" ==> using aes = go.crypto.aes_package
// Original source: C:\Go\src\crypto\aes\ctr_s390x.go
using cipher = go.crypto.cipher_package;
using subtle = go.crypto.@internal.subtle_package;
using binary = go.encoding.binary_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class aes_package
    {
        // Assert that aesCipherAsm implements the ctrAble interface.
        private static ctrAble _ = (aesCipherAsm.val)(null);

        // xorBytes xors the contents of a and b and places the resulting values into
        // dst. If a and b are not the same length then the number of bytes processed
        // will be equal to the length of shorter of the two. Returns the number
        // of bytes processed.
        //go:noescape
        private static long xorBytes(slice<byte> dst, slice<byte> a, slice<byte> b)
;

        // streamBufferSize is the number of bytes of encrypted counter values to cache.
        private static readonly long streamBufferSize = (long)32L * BlockSize;



        private partial struct aesctr
        {
            public ptr<aesCipherAsm> block; // block cipher
            public array<ulong> ctr; // next value of the counter (big endian)
            public slice<byte> buffer; // buffer for the encrypted counter values
            public array<byte> storage; // array backing buffer slice
        }

        // NewCTR returns a Stream which encrypts/decrypts using the AES block
        // cipher in counter mode. The length of iv must be the same as BlockSize.
        private static cipher.Stream NewCTR(this ptr<aesCipherAsm> _addr_c, slice<byte> iv) => func((_, panic, __) =>
        {
            ref aesCipherAsm c = ref _addr_c.val;

            if (len(iv) != BlockSize)
            {>>MARKER:FUNCTION_xorBytes_BLOCK_PREFIX<<
                panic("cipher.NewCTR: IV length must equal block size");
            }

            ref aesctr ac = ref heap(out ptr<aesctr> _addr_ac);
            ac.block = c;
            ac.ctr[0L] = binary.BigEndian.Uint64(iv[0L..]); // high bits
            ac.ctr[1L] = binary.BigEndian.Uint64(iv[8L..]); // low bits
            ac.buffer = ac.storage[..0L];
            return _addr_ac;

        });

        private static void refill(this ptr<aesctr> _addr_c)
        {
            ref aesctr c = ref _addr_c.val;
 
            // Fill up the buffer with an incrementing count.
            c.buffer = c.storage[..streamBufferSize];
            var c0 = c.ctr[0L];
            var c1 = c.ctr[1L];
            {
                long i = 0L;

                while (i < streamBufferSize)
                {
                    binary.BigEndian.PutUint64(c.buffer[i + 0L..], c0);
                    binary.BigEndian.PutUint64(c.buffer[i + 8L..], c1); 

                    // Increment in big endian: c0 is high, c1 is low.
                    c1++;
                    if (c1 == 0L)
                    { 
                        // add carry
                        c0++;
                    i += 16L;
                    }

                }

            }
            c.ctr[0L] = c0;
            c.ctr[1L] = c1; 
            // Encrypt the buffer using AES in ECB mode.
            cryptBlocks(c.block.function, _addr_c.block.key[0L], _addr_c.buffer[0L], _addr_c.buffer[0L], streamBufferSize);

        }

        private static void XORKeyStream(this ptr<aesctr> _addr_c, slice<byte> dst, slice<byte> src) => func((_, panic, __) =>
        {
            ref aesctr c = ref _addr_c.val;

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
                if (len(c.buffer) == 0L)
                {
                    c.refill();
                }

                var n = xorBytes(dst, src, c.buffer);
                c.buffer = c.buffer[n..];
                src = src[n..];
                dst = dst[n..];

            }


        });
    }
}}
