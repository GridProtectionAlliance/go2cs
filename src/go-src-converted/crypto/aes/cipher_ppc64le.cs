// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package aes -- go2cs converted at 2020 October 08 03:35:49 UTC
// import "crypto/aes" ==> using aes = go.crypto.aes_package
// Original source: C:\Go\src\crypto\aes\cipher_ppc64le.go
using cipher = go.crypto.cipher_package;
using subtle = go.crypto.@internal.subtle_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class aes_package
    {
        // defined in asm_ppc64le.s

        //go:noescape
        private static long setEncryptKeyAsm(ptr<byte> key, long keylen, ptr<uint> enc)
;

        //go:noescape
        private static long setDecryptKeyAsm(ptr<byte> key, long keylen, ptr<uint> dec)
;

        //go:noescape
        private static long doEncryptKeyAsm(ptr<byte> key, long keylen, ptr<uint> dec)
;

        //go:noescape
        private static void encryptBlockAsm(ptr<byte> dst, ptr<byte> src, ptr<uint> enc)
;

        //go:noescape
        private static void decryptBlockAsm(ptr<byte> dst, ptr<byte> src, ptr<uint> dec)
;

        private partial struct aesCipherAsm
        {
            public ref aesCipher aesCipher => ref aesCipher_val;
        }

        private static (cipher.Block, error) newCipher(slice<byte> key)
        {
            cipher.Block _p0 = default;
            error _p0 = default!;

            long n = 64L; // size is fixed for all and round value is stored inside it too
            ref aesCipherAsm c = ref heap(new aesCipherAsm(aesCipher{make([]uint32,n),make([]uint32,n)}), out ptr<aesCipherAsm> _addr_c);
            var k = len(key);

            long ret = 0L;
            ret += setEncryptKeyAsm(_addr_key[0L], k * 8L, _addr_c.enc[0L]);
            ret += setDecryptKeyAsm(_addr_key[0L], k * 8L, _addr_c.dec[0L]);

            if (ret > 0L)
            {>>MARKER:FUNCTION_decryptBlockAsm_BLOCK_PREFIX<<
                return (null, error.As(KeySizeError(k))!);
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
            {>>MARKER:FUNCTION_doEncryptKeyAsm_BLOCK_PREFIX<<
                panic("crypto/aes: output not full block");
            }

            if (subtle.InexactOverlap(dst[..BlockSize], src[..BlockSize]))
            {>>MARKER:FUNCTION_setDecryptKeyAsm_BLOCK_PREFIX<<
                panic("crypto/aes: invalid buffer overlap");
            }

            encryptBlockAsm(_addr_dst[0L], _addr_src[0L], _addr_c.enc[0L]);

        });

        private static void Decrypt(this ptr<aesCipherAsm> _addr_c, slice<byte> dst, slice<byte> src) => func((_, panic, __) =>
        {
            ref aesCipherAsm c = ref _addr_c.val;

            if (len(src) < BlockSize)
            {>>MARKER:FUNCTION_setEncryptKeyAsm_BLOCK_PREFIX<<
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

            decryptBlockAsm(_addr_dst[0L], _addr_src[0L], _addr_c.dec[0L]);

        });

        // expandKey is used by BenchmarkExpand to ensure that the asm implementation
        // of key expansion is used for the benchmark when it is available.
        private static void expandKey(slice<byte> key, slice<uint> enc, slice<uint> dec)
        {
            setEncryptKeyAsm(_addr_key[0L], len(key) * 8L, _addr_enc[0L]);
            setDecryptKeyAsm(_addr_key[0L], len(key) * 8L, _addr_dec[0L]);
        }
    }
}}
