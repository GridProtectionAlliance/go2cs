// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package aes -- go2cs converted at 2020 August 29 08:28:48 UTC
// import "crypto/aes" ==> using aes = go.crypto.aes_package
// Original source: C:\Go\src\crypto\aes\ctr_s390x.go
using cipher = go.crypto.cipher_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static unsafe partial class aes_package
    {
        // Assert that aesCipherAsm implements the ctrAble interface.
        private static ctrAble _ = (aesCipherAsm.Value)(null);

        // xorBytes xors the contents of a and b and places the resulting values into
        // dst. If a and b are not the same length then the number of bytes processed
        // will be equal to the length of shorter of the two. Returns the number
        // of bytes processed.
        //go:noescape
        private static long xorBytes(slice<byte> dst, slice<byte> a, slice<byte> b)
;

        // streamBufferSize is the number of bytes of encrypted counter values to cache.
        private static readonly long streamBufferSize = 32L * BlockSize;



        private partial struct aesctr
        {
            public ptr<aesCipherAsm> block; // block cipher
            public array<ulong> ctr; // next value of the counter (big endian)
            public slice<byte> buffer; // buffer for the encrypted counter values
            public array<byte> storage; // array backing buffer slice
        }

        // NewCTR returns a Stream which encrypts/decrypts using the AES block
        // cipher in counter mode. The length of iv must be the same as BlockSize.
        private static cipher.Stream NewCTR(this ref aesCipherAsm _c, slice<byte> iv) => func(_c, (ref aesCipherAsm c, Defer _, Panic panic, Recover __) =>
        {>>MARKER:FUNCTION_xorBytes_BLOCK_PREFIX<<
            if (len(iv) != BlockSize)
            {
                panic("cipher.NewCTR: IV length must equal block size");
            }
            aesctr ac = default;
            ac.block = c;
            ac.ctr[0L] = @unsafe.Pointer((ref iv[0L])).Value; // high bits
            ac.ctr[1L] = @unsafe.Pointer((ref iv[8L])).Value; // low bits
            ac.buffer = ac.storage[..0L];
            return ref ac;
        });

        private static void refill(this ref aesctr c)
        { 
            // Fill up the buffer with an incrementing count.
            c.buffer = c.storage[..streamBufferSize];
            var c0 = c.ctr[0L];
            var c1 = c.ctr[1L];
            {
                long i = 0L;

                while (i < streamBufferSize)
                {
                    var b0 = (uint64.Value)(@unsafe.Pointer(ref c.buffer[i]));
                    var b1 = (uint64.Value)(@unsafe.Pointer(ref c.buffer[i + BlockSize / 2L]));
                    b0.Value = c0;
                    b1.Value = c1; 
                    // Increment in big endian: c0 is high, c1 is low.
                    c1++;
                    if (c1 == 0L)
                    { 
                        // add carry
                        c0++;
                    i += BlockSize;
                    }
                }

            }
            c.ctr[0L] = c0;
            c.ctr[1L] = c1; 
            // Encrypt the buffer using AES in ECB mode.
            cryptBlocks(c.block.function, ref c.block.key[0L], ref c.buffer[0L], ref c.buffer[0L], streamBufferSize);
        }

        private static void XORKeyStream(this ref aesctr c, slice<byte> dst, slice<byte> src)
        {
            if (len(src) > 0L)
            { 
                // Assert len(dst) >= len(src)
                _ = dst[len(src) - 1L];
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

        }
    }
}}
