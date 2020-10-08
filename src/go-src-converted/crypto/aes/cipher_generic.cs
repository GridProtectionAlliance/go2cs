// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !amd64,!s390x,!ppc64le,!arm64

// package aes -- go2cs converted at 2020 October 08 03:35:48 UTC
// import "crypto/aes" ==> using aes = go.crypto.aes_package
// Original source: C:\Go\src\crypto\aes\cipher_generic.go
using cipher = go.crypto.cipher_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class aes_package
    {
        // newCipher calls the newCipherGeneric function
        // directly. Platforms with hardware accelerated
        // implementations of AES should implement their
        // own version of newCipher (which may then call
        // newCipherGeneric if needed).
        private static (cipher.Block, error) newCipher(slice<byte> key)
        {
            cipher.Block _p0 = default;
            error _p0 = default!;

            return newCipherGeneric(key);
        }

        // expandKey is used by BenchmarkExpand and should
        // call an assembly implementation if one is available.
        private static void expandKey(slice<byte> key, slice<uint> enc, slice<uint> dec)
        {
            expandKeyGo(key, enc, dec);
        }
    }
}}
