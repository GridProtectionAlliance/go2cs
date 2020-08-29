// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package aes -- go2cs converted at 2020 August 29 08:28:44 UTC
// import "crypto/aes" ==> using aes = go.crypto.aes_package
// Original source: C:\Go\src\crypto\aes\cbc_s390x.go
using cipher = go.crypto.cipher_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class aes_package
    {
        // Assert that aesCipherAsm implements the cbcEncAble and cbcDecAble interfaces.
        private static cbcEncAble _ = (aesCipherAsm.Value)(null);
        private static cbcDecAble _ = (aesCipherAsm.Value)(null);

        private partial struct cbc
        {
            public ptr<aesCipherAsm> b;
            public code c;
            public array<byte> iv;
        }

        private static cipher.BlockMode NewCBCEncrypter(this ref aesCipherAsm b, slice<byte> iv)
        {
            cbc c = default;
            c.b = b;
            c.c = b.function;
            copy(c.iv[..], iv);
            return ref c;
        }

        private static cipher.BlockMode NewCBCDecrypter(this ref aesCipherAsm b, slice<byte> iv)
        {
            cbc c = default;
            c.b = b;
            c.c = b.function + 128L; // decrypt function code is encrypt + 128
            copy(c.iv[..], iv);
            return ref c;
        }

        private static long BlockSize(this ref cbc x)
        {
            return BlockSize;
        }

        // cryptBlocksChain invokes the cipher message with chaining (KMC) instruction
        // with the given function code. The length must be a multiple of BlockSize (16).
        //go:noescape
        private static void cryptBlocksChain(code c, ref byte iv, ref byte key, ref byte dst, ref byte src, long length)
;

        private static void CryptBlocks(this ref cbc _x, slice<byte> dst, slice<byte> src) => func(_x, (ref cbc x, Defer _, Panic panic, Recover __) =>
        {>>MARKER:FUNCTION_cryptBlocksChain_BLOCK_PREFIX<<
            if (len(src) % BlockSize != 0L)
            {
                panic("crypto/cipher: input not full blocks");
            }
            if (len(dst) < len(src))
            {
                panic("crypto/cipher: output smaller than input");
            }
            if (len(src) > 0L)
            {
                cryptBlocksChain(x.c, ref x.iv[0L], ref x.b.key[0L], ref dst[0L], ref src[0L], len(src));
            }
        });

        private static void SetIV(this ref cbc _x, slice<byte> iv) => func(_x, (ref cbc x, Defer _, Panic panic, Recover __) =>
        {
            if (len(iv) != BlockSize)
            {
                panic("cipher: incorrect length IV");
            }
            copy(x.iv[..], iv);
        });
    }
}}
