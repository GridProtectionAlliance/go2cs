// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package des -- go2cs converted at 2020 October 08 03:36:35 UTC
// import "crypto/des" ==> using des = go.crypto.des_package
// Original source: C:\Go\src\crypto\des\cipher.go
using cipher = go.crypto.cipher_package;
using subtle = go.crypto.@internal.subtle_package;
using binary = go.encoding.binary_package;
using strconv = go.strconv_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class des_package
    {
        // The DES block size in bytes.
        public static readonly long BlockSize = (long)8L;



        public partial struct KeySizeError // : long
        {
        }

        public static @string Error(this KeySizeError k)
        {
            return "crypto/des: invalid key size " + strconv.Itoa(int(k));
        }

        // desCipher is an instance of DES encryption.
        private partial struct desCipher
        {
            public array<ulong> subkeys;
        }

        // NewCipher creates and returns a new cipher.Block.
        public static (cipher.Block, error) NewCipher(slice<byte> key)
        {
            cipher.Block _p0 = default;
            error _p0 = default!;

            if (len(key) != 8L)
            {
                return (null, error.As(KeySizeError(len(key)))!);
            }

            ptr<desCipher> c = @new<desCipher>();
            c.generateSubkeys(key);
            return (c, error.As(null!)!);

        }

        private static long BlockSize(this ptr<desCipher> _addr_c)
        {
            ref desCipher c = ref _addr_c.val;

            return BlockSize;
        }

        private static void Encrypt(this ptr<desCipher> _addr_c, slice<byte> dst, slice<byte> src) => func((_, panic, __) =>
        {
            ref desCipher c = ref _addr_c.val;

            if (len(src) < BlockSize)
            {
                panic("crypto/des: input not full block");
            }

            if (len(dst) < BlockSize)
            {
                panic("crypto/des: output not full block");
            }

            if (subtle.InexactOverlap(dst[..BlockSize], src[..BlockSize]))
            {
                panic("crypto/des: invalid buffer overlap");
            }

            encryptBlock(c.subkeys[..], dst, src);

        });

        private static void Decrypt(this ptr<desCipher> _addr_c, slice<byte> dst, slice<byte> src) => func((_, panic, __) =>
        {
            ref desCipher c = ref _addr_c.val;

            if (len(src) < BlockSize)
            {
                panic("crypto/des: input not full block");
            }

            if (len(dst) < BlockSize)
            {
                panic("crypto/des: output not full block");
            }

            if (subtle.InexactOverlap(dst[..BlockSize], src[..BlockSize]))
            {
                panic("crypto/des: invalid buffer overlap");
            }

            decryptBlock(c.subkeys[..], dst, src);

        });

        // A tripleDESCipher is an instance of TripleDES encryption.
        private partial struct tripleDESCipher
        {
            public desCipher cipher1;
            public desCipher cipher2;
            public desCipher cipher3;
        }

        // NewTripleDESCipher creates and returns a new cipher.Block.
        public static (cipher.Block, error) NewTripleDESCipher(slice<byte> key)
        {
            cipher.Block _p0 = default;
            error _p0 = default!;

            if (len(key) != 24L)
            {
                return (null, error.As(KeySizeError(len(key)))!);
            }

            ptr<tripleDESCipher> c = @new<tripleDESCipher>();
            c.cipher1.generateSubkeys(key[..8L]);
            c.cipher2.generateSubkeys(key[8L..16L]);
            c.cipher3.generateSubkeys(key[16L..]);
            return (c, error.As(null!)!);

        }

        private static long BlockSize(this ptr<tripleDESCipher> _addr_c)
        {
            ref tripleDESCipher c = ref _addr_c.val;

            return BlockSize;
        }

        private static void Encrypt(this ptr<tripleDESCipher> _addr_c, slice<byte> dst, slice<byte> src) => func((_, panic, __) =>
        {
            ref tripleDESCipher c = ref _addr_c.val;

            if (len(src) < BlockSize)
            {
                panic("crypto/des: input not full block");
            }

            if (len(dst) < BlockSize)
            {
                panic("crypto/des: output not full block");
            }

            if (subtle.InexactOverlap(dst[..BlockSize], src[..BlockSize]))
            {
                panic("crypto/des: invalid buffer overlap");
            }

            var b = binary.BigEndian.Uint64(src);
            b = permuteInitialBlock(b);
            var left = uint32(b >> (int)(32L));
            var right = uint32(b);

            left = (left << (int)(1L)) | (left >> (int)(31L));
            right = (right << (int)(1L)) | (right >> (int)(31L));

            {
                long i__prev1 = i;

                for (long i = 0L; i < 8L; i++)
                {
                    left, right = feistel(left, right, c.cipher1.subkeys[2L * i], c.cipher1.subkeys[2L * i + 1L]);
                }


                i = i__prev1;
            }
            {
                long i__prev1 = i;

                for (i = 0L; i < 8L; i++)
                {
                    right, left = feistel(right, left, c.cipher2.subkeys[15L - 2L * i], c.cipher2.subkeys[15L - (2L * i + 1L)]);
                }


                i = i__prev1;
            }
            {
                long i__prev1 = i;

                for (i = 0L; i < 8L; i++)
                {
                    left, right = feistel(left, right, c.cipher3.subkeys[2L * i], c.cipher3.subkeys[2L * i + 1L]);
                }


                i = i__prev1;
            }

            left = (left << (int)(31L)) | (left >> (int)(1L));
            right = (right << (int)(31L)) | (right >> (int)(1L));

            var preOutput = (uint64(right) << (int)(32L)) | uint64(left);
            binary.BigEndian.PutUint64(dst, permuteFinalBlock(preOutput));

        });

        private static void Decrypt(this ptr<tripleDESCipher> _addr_c, slice<byte> dst, slice<byte> src) => func((_, panic, __) =>
        {
            ref tripleDESCipher c = ref _addr_c.val;

            if (len(src) < BlockSize)
            {
                panic("crypto/des: input not full block");
            }

            if (len(dst) < BlockSize)
            {
                panic("crypto/des: output not full block");
            }

            if (subtle.InexactOverlap(dst[..BlockSize], src[..BlockSize]))
            {
                panic("crypto/des: invalid buffer overlap");
            }

            var b = binary.BigEndian.Uint64(src);
            b = permuteInitialBlock(b);
            var left = uint32(b >> (int)(32L));
            var right = uint32(b);

            left = (left << (int)(1L)) | (left >> (int)(31L));
            right = (right << (int)(1L)) | (right >> (int)(31L));

            {
                long i__prev1 = i;

                for (long i = 0L; i < 8L; i++)
                {
                    left, right = feistel(left, right, c.cipher3.subkeys[15L - 2L * i], c.cipher3.subkeys[15L - (2L * i + 1L)]);
                }


                i = i__prev1;
            }
            {
                long i__prev1 = i;

                for (i = 0L; i < 8L; i++)
                {
                    right, left = feistel(right, left, c.cipher2.subkeys[2L * i], c.cipher2.subkeys[2L * i + 1L]);
                }


                i = i__prev1;
            }
            {
                long i__prev1 = i;

                for (i = 0L; i < 8L; i++)
                {
                    left, right = feistel(left, right, c.cipher1.subkeys[15L - 2L * i], c.cipher1.subkeys[15L - (2L * i + 1L)]);
                }


                i = i__prev1;
            }

            left = (left << (int)(31L)) | (left >> (int)(1L));
            right = (right << (int)(31L)) | (right >> (int)(1L));

            var preOutput = (uint64(right) << (int)(32L)) | uint64(left);
            binary.BigEndian.PutUint64(dst, permuteFinalBlock(preOutput));

        });
    }
}}
