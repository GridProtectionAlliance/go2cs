// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package aes -- go2cs converted at 2020 August 29 08:28:46 UTC
// import "crypto/aes" ==> using aes = go.crypto.aes_package
// Original source: C:\Go\src\crypto\aes\cipher_s390x.go
using cipher = go.crypto.cipher_package;
using cipherhw = go.crypto.@internal.cipherhw_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class aes_package
    {
        private partial struct code // : long
        {
        }

        // Function codes for the cipher message family of instructions.
        private static readonly code aes128 = 18L;
        private static readonly long aes192 = 19L;
        private static readonly long aes256 = 20L;

        private partial struct aesCipherAsm
        {
            public code function; // code for cipher message instruction
            public slice<byte> key; // key (128, 192 or 256 bytes)
            public array<byte> storage; // array backing key slice
        }

        // cryptBlocks invokes the cipher message (KM) instruction with
        // the given function code. This is equivalent to AES in ECB
        // mode. The length must be a multiple of BlockSize (16).
        //go:noescape
        private static void cryptBlocks(code c, ref byte key, ref byte dst, ref byte src, long length)
;

        private static var useAsm = cipherhw.AESGCMSupport();

        private static (cipher.Block, error) newCipher(slice<byte> key)
        {
            if (!useAsm)
            {>>MARKER:FUNCTION_cryptBlocks_BLOCK_PREFIX<<
                return newCipherGeneric(key);
            }
            code function = default;
            switch (len(key))
            {
                case 128L / 8L: 
                    function = aes128;
                    break;
                case 192L / 8L: 
                    function = aes192;
                    break;
                case 256L / 8L: 
                    function = aes256;
                    break;
                default: 
                    return (null, KeySizeError(len(key)));
                    break;
            }

            aesCipherAsm c = default;
            c.function = function;
            c.key = c.storage[..len(key)];
            copy(c.key, key);
            return (ref c, null);
        }

        private static long BlockSize(this ref aesCipherAsm c)
        {
            return BlockSize;
        }

        private static void Encrypt(this ref aesCipherAsm _c, slice<byte> dst, slice<byte> src) => func(_c, (ref aesCipherAsm c, Defer _, Panic panic, Recover __) =>
        {
            if (len(src) < BlockSize)
            {
                panic("crypto/aes: input not full block");
            }
            if (len(dst) < BlockSize)
            {
                panic("crypto/aes: output not full block");
            }
            cryptBlocks(c.function, ref c.key[0L], ref dst[0L], ref src[0L], BlockSize);
        });

        private static void Decrypt(this ref aesCipherAsm _c, slice<byte> dst, slice<byte> src) => func(_c, (ref aesCipherAsm c, Defer _, Panic panic, Recover __) =>
        {
            if (len(src) < BlockSize)
            {
                panic("crypto/aes: input not full block");
            }
            if (len(dst) < BlockSize)
            {
                panic("crypto/aes: output not full block");
            } 
            // The decrypt function code is equal to the function code + 128.
            cryptBlocks(c.function + 128L, ref c.key[0L], ref dst[0L], ref src[0L], BlockSize);
        });

        // expandKey is used by BenchmarkExpand. cipher message (KM) does not need key
        // expansion so there is no assembly equivalent.
        private static void expandKey(slice<byte> key, slice<uint> enc, slice<uint> dec)
        {
            expandKeyGo(key, enc, dec);
        }
    }
}}
