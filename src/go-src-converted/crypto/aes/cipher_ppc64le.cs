// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package aes -- go2cs converted at 2020 August 29 08:28:46 UTC
// import "crypto/aes" ==> using aes = go.crypto.aes_package
// Original source: C:\Go\src\crypto\aes\cipher_ppc64le.go
using cipher = go.crypto.cipher_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class aes_package
    {
        // defined in asm_ppc64le.s

        //go:noescape
        private static long setEncryptKeyAsm(ref byte key, long keylen, ref uint enc)
;

        //go:noescape

        private static long setDecryptKeyAsm(ref byte key, long keylen, ref uint dec)
;

        //go:noescape

        private static long doEncryptKeyAsm(ref byte key, long keylen, ref uint dec)
;

        //go:noescape

        private static void encryptBlockAsm(ref byte dst, ref byte src, ref uint enc)
;

        //go:noescape

        private static void decryptBlockAsm(ref byte dst, ref byte src, ref uint dec)
;

        private partial struct aesCipherAsm
        {
            public ref aesCipher aesCipher => ref aesCipher_val;
        }

        private static (cipher.Block, error) newCipher(slice<byte> key)
        {
            long n = 64L; // size is fixed for all and round value is stored inside it too
            aesCipherAsm c = new aesCipherAsm(aesCipher{make([]uint32,n),make([]uint32,n)});
            var k = len(key);

            long ret = 0L;
            ret += setEncryptKeyAsm(ref key[0L], k * 8L, ref c.enc[0L]);
            ret += setDecryptKeyAsm(ref key[0L], k * 8L, ref c.dec[0L]);

            if (ret > 0L)
            {>>MARKER:FUNCTION_decryptBlockAsm_BLOCK_PREFIX<<
                return (null, KeySizeError(k));
            }
            return (ref c, null);
        }

        private static long BlockSize(this ref aesCipherAsm c)
        {>>MARKER:FUNCTION_encryptBlockAsm_BLOCK_PREFIX<<
            return BlockSize;
        }

        private static void Encrypt(this ref aesCipherAsm _c, slice<byte> dst, slice<byte> src) => func(_c, (ref aesCipherAsm c, Defer _, Panic panic, Recover __) =>
        {>>MARKER:FUNCTION_doEncryptKeyAsm_BLOCK_PREFIX<<
            if (len(src) < BlockSize)
            {>>MARKER:FUNCTION_setDecryptKeyAsm_BLOCK_PREFIX<<
                panic("crypto/aes: input not full block");
            }
            if (len(dst) < BlockSize)
            {>>MARKER:FUNCTION_setEncryptKeyAsm_BLOCK_PREFIX<<
                panic("crypto/aes: output not full block");
            }
            encryptBlockAsm(ref dst[0L], ref src[0L], ref c.enc[0L]);
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
            decryptBlockAsm(ref dst[0L], ref src[0L], ref c.dec[0L]);
        });

        // expandKey is used by BenchmarkExpand to ensure that the asm implementation
        // of key expansion is used for the benchmark when it is available.
        private static void expandKey(slice<byte> key, slice<uint> enc, slice<uint> dec)
        {
            setEncryptKeyAsm(ref key[0L], len(key) * 8L, ref enc[0L]);
            setDecryptKeyAsm(ref key[0L], len(key) * 8L, ref dec[0L]);
        }
    }
}}
