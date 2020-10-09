// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package aes -- go2cs converted at 2020 October 09 04:53:46 UTC
// import "crypto/aes" ==> using aes = go.crypto.aes_package
// Original source: C:\Go\src\crypto\aes\cbc_s390x.go
using cipher = go.crypto.cipher_package;
using subtle = go.crypto.@internal.subtle_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class aes_package
    {
        // Assert that aesCipherAsm implements the cbcEncAble and cbcDecAble interfaces.
        private static cbcEncAble _ = (aesCipherAsm.val)(null);
        private static cbcDecAble _ = (aesCipherAsm.val)(null);

        private partial struct cbc
        {
            public ptr<aesCipherAsm> b;
            public code c;
            public array<byte> iv;
        }

        private static cipher.BlockMode NewCBCEncrypter(this ptr<aesCipherAsm> _addr_b, slice<byte> iv)
        {
            ref aesCipherAsm b = ref _addr_b.val;

            ref cbc c = ref heap(out ptr<cbc> _addr_c);
            c.b = b;
            c.c = b.function;
            copy(c.iv[..], iv);
            return _addr_c;
        }

        private static cipher.BlockMode NewCBCDecrypter(this ptr<aesCipherAsm> _addr_b, slice<byte> iv)
        {
            ref aesCipherAsm b = ref _addr_b.val;

            ref cbc c = ref heap(out ptr<cbc> _addr_c);
            c.b = b;
            c.c = b.function + 128L; // decrypt function code is encrypt + 128
            copy(c.iv[..], iv);
            return _addr_c;

        }

        private static long BlockSize(this ptr<cbc> _addr_x)
        {
            ref cbc x = ref _addr_x.val;

            return BlockSize;
        }

        // cryptBlocksChain invokes the cipher message with chaining (KMC) instruction
        // with the given function code. The length must be a multiple of BlockSize (16).
        //go:noescape
        private static void cryptBlocksChain(code c, ptr<byte> iv, ptr<byte> key, ptr<byte> dst, ptr<byte> src, long length)
;

        private static void CryptBlocks(this ptr<cbc> _addr_x, slice<byte> dst, slice<byte> src) => func((_, panic, __) =>
        {
            ref cbc x = ref _addr_x.val;

            if (len(src) % BlockSize != 0L)
            {>>MARKER:FUNCTION_cryptBlocksChain_BLOCK_PREFIX<<
                panic("crypto/cipher: input not full blocks");
            }

            if (len(dst) < len(src))
            {
                panic("crypto/cipher: output smaller than input");
            }

            if (subtle.InexactOverlap(dst[..len(src)], src))
            {
                panic("crypto/cipher: invalid buffer overlap");
            }

            if (len(src) > 0L)
            {
                cryptBlocksChain(x.c, _addr_x.iv[0L], _addr_x.b.key[0L], _addr_dst[0L], _addr_src[0L], len(src));
            }

        });

        private static void SetIV(this ptr<cbc> _addr_x, slice<byte> iv) => func((_, panic, __) =>
        {
            ref cbc x = ref _addr_x.val;

            if (len(iv) != BlockSize)
            {
                panic("cipher: incorrect length IV");
            }

            copy(x.iv[..], iv);

        });
    }
}}
