// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Cipher block chaining (CBC) mode.

// CBC provides confidentiality by xoring (chaining) each plaintext block
// with the previous ciphertext block before applying the block cipher.

// See NIST SP 800-38A, pp 10-11

// package cipher -- go2cs converted at 2020 August 29 08:28:32 UTC
// import "crypto/cipher" ==> using cipher = go.crypto.cipher_package
// Original source: C:\Go\src\crypto\cipher\cbc.go

using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class cipher_package
    {
        private partial struct cbc
        {
            public Block b;
            public long blockSize;
            public slice<byte> iv;
            public slice<byte> tmp;
        }

        private static ref cbc newCBC(Block b, slice<byte> iv)
        {
            return ref new cbc(b:b,blockSize:b.BlockSize(),iv:dup(iv),tmp:make([]byte,b.BlockSize()),);
        }

        private partial struct cbcEncrypter // : cbc
        {
        }

        // cbcEncAble is an interface implemented by ciphers that have a specific
        // optimized implementation of CBC encryption, like crypto/aes.
        // NewCBCEncrypter will check for this interface and return the specific
        // BlockMode if found.
        private partial interface cbcEncAble
        {
            BlockMode NewCBCEncrypter(slice<byte> iv);
        }

        // NewCBCEncrypter returns a BlockMode which encrypts in cipher block chaining
        // mode, using the given Block. The length of iv must be the same as the
        // Block's block size.
        public static BlockMode NewCBCEncrypter(Block b, slice<byte> iv) => func((_, panic, __) =>
        {
            if (len(iv) != b.BlockSize())
            {
                panic("cipher.NewCBCEncrypter: IV length must equal block size");
            }
            {
                cbcEncAble (cbc, ok) = b._<cbcEncAble>();

                if (ok)
                {
                    return cbc.NewCBCEncrypter(iv);
                }

            }
            return (cbcEncrypter.Value)(newCBC(b, iv));
        });

        private static long BlockSize(this ref cbcEncrypter x)
        {
            return x.blockSize;
        }

        private static void CryptBlocks(this ref cbcEncrypter _x, slice<byte> dst, slice<byte> src) => func(_x, (ref cbcEncrypter x, Defer _, Panic panic, Recover __) =>
        {
            if (len(src) % x.blockSize != 0L)
            {
                panic("crypto/cipher: input not full blocks");
            }
            if (len(dst) < len(src))
            {
                panic("crypto/cipher: output smaller than input");
            }
            var iv = x.iv;

            while (len(src) > 0L)
            { 
                // Write the xor to dst, then encrypt in place.
                xorBytes(dst[..x.blockSize], src[..x.blockSize], iv);
                x.b.Encrypt(dst[..x.blockSize], dst[..x.blockSize]); 

                // Move to the next block with this block as the next iv.
                iv = dst[..x.blockSize];
                src = src[x.blockSize..];
                dst = dst[x.blockSize..];
            } 

            // Save the iv for the next CryptBlocks call.
 

            // Save the iv for the next CryptBlocks call.
            copy(x.iv, iv);
        });

        private static void SetIV(this ref cbcEncrypter _x, slice<byte> iv) => func(_x, (ref cbcEncrypter x, Defer _, Panic panic, Recover __) =>
        {
            if (len(iv) != len(x.iv))
            {
                panic("cipher: incorrect length IV");
            }
            copy(x.iv, iv);
        });

        private partial struct cbcDecrypter // : cbc
        {
        }

        // cbcDecAble is an interface implemented by ciphers that have a specific
        // optimized implementation of CBC decryption, like crypto/aes.
        // NewCBCDecrypter will check for this interface and return the specific
        // BlockMode if found.
        private partial interface cbcDecAble
        {
            BlockMode NewCBCDecrypter(slice<byte> iv);
        }

        // NewCBCDecrypter returns a BlockMode which decrypts in cipher block chaining
        // mode, using the given Block. The length of iv must be the same as the
        // Block's block size and must match the iv used to encrypt the data.
        public static BlockMode NewCBCDecrypter(Block b, slice<byte> iv) => func((_, panic, __) =>
        {
            if (len(iv) != b.BlockSize())
            {
                panic("cipher.NewCBCDecrypter: IV length must equal block size");
            }
            {
                cbcDecAble (cbc, ok) = b._<cbcDecAble>();

                if (ok)
                {
                    return cbc.NewCBCDecrypter(iv);
                }

            }
            return (cbcDecrypter.Value)(newCBC(b, iv));
        });

        private static long BlockSize(this ref cbcDecrypter x)
        {
            return x.blockSize;
        }

        private static void CryptBlocks(this ref cbcDecrypter _x, slice<byte> dst, slice<byte> src) => func(_x, (ref cbcDecrypter x, Defer _, Panic panic, Recover __) =>
        {
            if (len(src) % x.blockSize != 0L)
            {
                panic("crypto/cipher: input not full blocks");
            }
            if (len(dst) < len(src))
            {
                panic("crypto/cipher: output smaller than input");
            }
            if (len(src) == 0L)
            {
                return;
            } 

            // For each block, we need to xor the decrypted data with the previous block's ciphertext (the iv).
            // To avoid making a copy each time, we loop over the blocks BACKWARDS.
            var end = len(src);
            var start = end - x.blockSize;
            var prev = start - x.blockSize; 

            // Copy the last block of ciphertext in preparation as the new iv.
            copy(x.tmp, src[start..end]); 

            // Loop over all but the first block.
            while (start > 0L)
            {
                x.b.Decrypt(dst[start..end], src[start..end]);
                xorBytes(dst[start..end], dst[start..end], src[prev..start]);

                end = start;
                start = prev;
                prev -= x.blockSize;
            } 

            // The first block is special because it uses the saved iv.
 

            // The first block is special because it uses the saved iv.
            x.b.Decrypt(dst[start..end], src[start..end]);
            xorBytes(dst[start..end], dst[start..end], x.iv); 

            // Set the new iv to the first block we copied earlier.
            x.iv = x.tmp;
            x.tmp = x.iv;
        });

        private static void SetIV(this ref cbcDecrypter _x, slice<byte> iv) => func(_x, (ref cbcDecrypter x, Defer _, Panic panic, Recover __) =>
        {
            if (len(iv) != len(x.iv))
            {
                panic("cipher: incorrect length IV");
            }
            copy(x.iv, iv);
        });
    }
}}
