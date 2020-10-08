// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build amd64 arm64

// package aes -- go2cs converted at 2020 October 08 03:35:48 UTC
// import "crypto/aes" ==> using aes = go.crypto.aes_package
// Original source: C:\Go\src\crypto\aes\cipher_asm.go
using cipher = go.crypto.cipher_package;
using subtle = go.crypto.@internal.subtle_package;
using cpu = go.@internal.cpu_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class aes_package
    {
        // defined in asm_*.s

        //go:noescape
        private static void encryptBlockAsm(long nr, ptr<uint> xk, ptr<byte> dst, ptr<byte> src)
;

        //go:noescape
        private static void decryptBlockAsm(long nr, ptr<uint> xk, ptr<byte> dst, ptr<byte> src)
;

        //go:noescape
        private static void expandKeyAsm(long nr, ptr<byte> key, ptr<uint> enc, ptr<uint> dec)
;

        private partial struct aesCipherAsm
        {
            public ref aesCipher aesCipher => ref aesCipher_val;
        }

        private static var supportsAES = cpu.X86.HasAES || cpu.ARM64.HasAES;
        private static var supportsGFMUL = cpu.X86.HasPCLMULQDQ || cpu.ARM64.HasPMULL;

        private static (cipher.Block, error) newCipher(slice<byte> key)
        {
            cipher.Block _p0 = default;
            error _p0 = default!;

            if (!supportsAES)
            {>>MARKER:FUNCTION_expandKeyAsm_BLOCK_PREFIX<<
                return newCipherGeneric(key);
            }

            var n = len(key) + 28L;
            ref aesCipherAsm c = ref heap(new aesCipherAsm(aesCipher{make([]uint32,n),make([]uint32,n)}), out ptr<aesCipherAsm> _addr_c);
            long rounds = default;
            switch (len(key))
            {
                case 128L / 8L: 
                    rounds = 10L;
                    break;
                case 192L / 8L: 
                    rounds = 12L;
                    break;
                case 256L / 8L: 
                    rounds = 14L;
                    break;
            }

            expandKeyAsm(rounds, _addr_key[0L], _addr_c.enc[0L], _addr_c.dec[0L]);
            if (supportsAES && supportsGFMUL)
            {>>MARKER:FUNCTION_decryptBlockAsm_BLOCK_PREFIX<<
                return (addr(new aesCipherGCM(c)), error.As(null!)!);
            }

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
            {>>MARKER:FUNCTION_encryptBlockAsm_BLOCK_PREFIX<<
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

            encryptBlockAsm(len(c.enc) / 4L - 1L, _addr_c.enc[0L], _addr_dst[0L], _addr_src[0L]);

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

            decryptBlockAsm(len(c.dec) / 4L - 1L, _addr_c.dec[0L], _addr_dst[0L], _addr_src[0L]);

        });

        // expandKey is used by BenchmarkExpand to ensure that the asm implementation
        // of key expansion is used for the benchmark when it is available.
        private static void expandKey(slice<byte> key, slice<uint> enc, slice<uint> dec)
        {
            if (supportsAES)
            {
                long rounds = 10L; // rounds needed for AES128
                switch (len(key))
                {
                    case 192L / 8L: 
                        rounds = 12L;
                        break;
                    case 256L / 8L: 
                        rounds = 14L;
                        break;
                }
                expandKeyAsm(rounds, _addr_key[0L], _addr_enc[0L], _addr_dec[0L]);

            }
            else
            {
                expandKeyGo(key, enc, dec);
            }

        }
    }
}}
