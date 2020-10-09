// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package aes -- go2cs converted at 2020 October 09 04:53:47 UTC
// import "crypto/aes" ==> using aes = go.crypto.aes_package
// Original source: C:\Go\src\crypto\aes\cipher_s390x.go
using cipher = go.crypto.cipher_package;
using subtle = go.crypto.@internal.subtle_package;
using cpu = go.@internal.cpu_package;
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
        private static readonly code aes128 = (code)18L;
        private static readonly long aes192 = (long)19L;
        private static readonly long aes256 = (long)20L;


        private partial struct aesCipherAsm
        {
            public code function; // code for cipher message instruction
            public slice<byte> key; // key (128, 192 or 256 bits)
            public array<byte> storage; // array backing key slice
        }

        // cryptBlocks invokes the cipher message (KM) instruction with
        // the given function code. This is equivalent to AES in ECB
        // mode. The length must be a multiple of BlockSize (16).
        //go:noescape
        private static void cryptBlocks(code c, ptr<byte> key, ptr<byte> dst, ptr<byte> src, long length)
;

        private static (cipher.Block, error) newCipher(slice<byte> key)
        {
            cipher.Block _p0 = default;
            error _p0 = default!;
 
            // The aesCipherAsm type implements the cbcEncAble, cbcDecAble,
            // ctrAble and gcmAble interfaces. We therefore need to check
            // for all the features required to implement these modes.
            // Keep in sync with crypto/tls/common.go.
            if (!(cpu.S390X.HasAES && cpu.S390X.HasAESCBC && cpu.S390X.HasAESCTR && (cpu.S390X.HasGHASH || cpu.S390X.HasAESGCM)))
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
                    return (null, error.As(KeySizeError(len(key)))!);
                    break;
            }

            ref aesCipherAsm c = ref heap(out ptr<aesCipherAsm> _addr_c);
            c.function = function;
            c.key = c.storage[..len(key)];
            copy(c.key, key);
            return (_addr_c, error.As(null!)!);

        }

        private static long BlockSize(this ptr<aesCipherAsm> _addr_c)
        {
            ref aesCipherAsm c = ref _addr_c.val;

            return BlockSize;
        }

        private static void Encrypt(this ptr<aesCipherAsm> _addr_c, slice<byte> dst, slice<byte> src) => func((_, panic, __) =>
        {
            ref aesCipherAsm c = ref _addr_c.val;

            if (len(src) < BlockSize)
            {
                panic("crypto/aes: input not full block");
            }

            if (len(dst) < BlockSize)
            {
                panic("crypto/aes: output not full block");
            }

            if (subtle.InexactOverlap(dst[..BlockSize], src[..BlockSize]))
            {
                panic("crypto/aes: invalid buffer overlap");
            }

            cryptBlocks(c.function, _addr_c.key[0L], _addr_dst[0L], _addr_src[0L], BlockSize);

        });

        private static void Decrypt(this ptr<aesCipherAsm> _addr_c, slice<byte> dst, slice<byte> src) => func((_, panic, __) =>
        {
            ref aesCipherAsm c = ref _addr_c.val;

            if (len(src) < BlockSize)
            {
                panic("crypto/aes: input not full block");
            }

            if (len(dst) < BlockSize)
            {
                panic("crypto/aes: output not full block");
            }

            if (subtle.InexactOverlap(dst[..BlockSize], src[..BlockSize]))
            {
                panic("crypto/aes: invalid buffer overlap");
            } 
            // The decrypt function code is equal to the function code + 128.
            cryptBlocks(c.function + 128L, _addr_c.key[0L], _addr_dst[0L], _addr_src[0L], BlockSize);

        });

        // expandKey is used by BenchmarkExpand. cipher message (KM) does not need key
        // expansion so there is no assembly equivalent.
        private static void expandKey(slice<byte> key, slice<uint> enc, slice<uint> dec)
        {
            expandKeyGo(key, enc, dec);
        }
    }
}}
