// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package aes -- go2cs converted at 2020 August 29 08:28:45 UTC
// import "crypto/aes" ==> using aes = go.crypto.aes_package
// Original source: C:\Go\src\crypto\aes\cipher_amd64.go
using cipher = go.crypto.cipher_package;
using cipherhw = go.crypto.@internal.cipherhw_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class aes_package
    {
        // defined in asm_amd64.s
        private static void encryptBlockAsm(long nr, ref uint xk, ref byte dst, ref byte src)
;
        private static void decryptBlockAsm(long nr, ref uint xk, ref byte dst, ref byte src)
;
        private static void expandKeyAsm(long nr, ref byte key, ref uint enc, ref uint dec)
;

        private partial struct aesCipherAsm
        {
            public ref aesCipher aesCipher => ref aesCipher_val;
        }

        private static var useAsm = cipherhw.AESGCMSupport();

        private static (cipher.Block, error) newCipher(slice<byte> key)
        {
            if (!useAsm)
            {>>MARKER:FUNCTION_expandKeyAsm_BLOCK_PREFIX<<
                return newCipherGeneric(key);
            }
            var n = len(key) + 28L;
            aesCipherAsm c = new aesCipherAsm(aesCipher{make([]uint32,n),make([]uint32,n)});
            long rounds = 10L;
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
            expandKeyAsm(rounds, ref key[0L], ref c.enc[0L], ref c.dec[0L]);
            if (hasGCMAsm())
            {>>MARKER:FUNCTION_decryptBlockAsm_BLOCK_PREFIX<<
                return (ref new aesCipherGCM(c), null);
            }
            return (ref c, null);
        }

        private static long BlockSize(this ref aesCipherAsm c)
        {>>MARKER:FUNCTION_encryptBlockAsm_BLOCK_PREFIX<<
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
            encryptBlockAsm(len(c.enc) / 4L - 1L, ref c.enc[0L], ref dst[0L], ref src[0L]);
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
            decryptBlockAsm(len(c.dec) / 4L - 1L, ref c.dec[0L], ref dst[0L], ref src[0L]);
        });

        // expandKey is used by BenchmarkExpand to ensure that the asm implementation
        // of key expansion is used for the benchmark when it is available.
        private static void expandKey(slice<byte> key, slice<uint> enc, slice<uint> dec)
        {
            if (useAsm)
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
                expandKeyAsm(rounds, ref key[0L], ref enc[0L], ref dec[0L]);
            }
            else
            {
                expandKeyGo(key, enc, dec);
            }
        }
    }
}}
