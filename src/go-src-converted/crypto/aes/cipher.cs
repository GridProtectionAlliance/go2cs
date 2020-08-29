// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package aes -- go2cs converted at 2020 August 29 08:28:45 UTC
// import "crypto/aes" ==> using aes = go.crypto.aes_package
// Original source: C:\Go\src\crypto\aes\cipher.go
using cipher = go.crypto.cipher_package;
using strconv = go.strconv_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class aes_package
    {
        // The AES block size in bytes.
        public static readonly long BlockSize = 16L;

        // A cipher is an instance of AES encryption using a particular key.


        // A cipher is an instance of AES encryption using a particular key.
        private partial struct aesCipher
        {
            public slice<uint> enc;
            public slice<uint> dec;
        }

        public partial struct KeySizeError // : long
        {
        }

        public static @string Error(this KeySizeError k)
        {
            return "crypto/aes: invalid key size " + strconv.Itoa(int(k));
        }

        // NewCipher creates and returns a new cipher.Block.
        // The key argument should be the AES key,
        // either 16, 24, or 32 bytes to select
        // AES-128, AES-192, or AES-256.
        public static (cipher.Block, error) NewCipher(slice<byte> key)
        {
            var k = len(key);
            switch (k)
            {
                case 16L: 

                case 24L: 

                case 32L: 
                    break;
                    break;
                default: 
                    return (null, KeySizeError(k));
                    break;
            }
            return newCipher(key);
        }

        // newCipherGeneric creates and returns a new cipher.Block
        // implemented in pure Go.
        private static (cipher.Block, error) newCipherGeneric(slice<byte> key)
        {
            var n = len(key) + 28L;
            aesCipher c = new aesCipher(make([]uint32,n),make([]uint32,n));
            expandKeyGo(key, c.enc, c.dec);
            return (ref c, null);
        }

        private static long BlockSize(this ref aesCipher c)
        {
            return BlockSize;
        }

        private static void Encrypt(this ref aesCipher _c, slice<byte> dst, slice<byte> src) => func(_c, (ref aesCipher c, Defer _, Panic panic, Recover __) =>
        {
            if (len(src) < BlockSize)
            {
                panic("crypto/aes: input not full block");
            }
            if (len(dst) < BlockSize)
            {
                panic("crypto/aes: output not full block");
            }
            encryptBlockGo(c.enc, dst, src);
        });

        private static void Decrypt(this ref aesCipher _c, slice<byte> dst, slice<byte> src) => func(_c, (ref aesCipher c, Defer _, Panic panic, Recover __) =>
        {
            if (len(src) < BlockSize)
            {
                panic("crypto/aes: input not full block");
            }
            if (len(dst) < BlockSize)
            {
                panic("crypto/aes: output not full block");
            }
            decryptBlockGo(c.dec, dst, src);
        });
    }
}}
