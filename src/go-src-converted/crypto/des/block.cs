// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package des -- go2cs converted at 2020 October 08 03:36:35 UTC
// import "crypto/des" ==> using des = go.crypto.des_package
// Original source: C:\Go\src\crypto\des\block.go
using binary = go.encoding.binary_package;
using sync = go.sync_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class des_package
    {
        private static void cryptBlock(slice<ulong> subkeys, slice<byte> dst, slice<byte> src, bool decrypt)
        {
            var b = binary.BigEndian.Uint64(src);
            b = permuteInitialBlock(b);
            var left = uint32(b >> (int)(32L));
            var right = uint32(b);

            left = (left << (int)(1L)) | (left >> (int)(31L));
            right = (right << (int)(1L)) | (right >> (int)(31L));

            if (decrypt)
            {
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < 8L; i++)
                    {
                        left, right = feistel(left, right, subkeys[15L - 2L * i], subkeys[15L - (2L * i + 1L)]);
                    }
            else


                    i = i__prev1;
                }

            }            {
                {
                    long i__prev1 = i;

                    for (i = 0L; i < 8L; i++)
                    {
                        left, right = feistel(left, right, subkeys[2L * i], subkeys[2L * i + 1L]);
                    }

                    i = i__prev1;
                }

            }
            left = (left << (int)(31L)) | (left >> (int)(1L));
            right = (right << (int)(31L)) | (right >> (int)(1L)); 

            // switch left & right and perform final permutation
            var preOutput = (uint64(right) << (int)(32L)) | uint64(left);
            binary.BigEndian.PutUint64(dst, permuteFinalBlock(preOutput));

        }

        // Encrypt one block from src into dst, using the subkeys.
        private static void encryptBlock(slice<ulong> subkeys, slice<byte> dst, slice<byte> src)
        {
            cryptBlock(subkeys, dst, src, false);
        }

        // Decrypt one block from src into dst, using the subkeys.
        private static void decryptBlock(slice<ulong> subkeys, slice<byte> dst, slice<byte> src)
        {
            cryptBlock(subkeys, dst, src, true);
        }

        // DES Feistel function. feistelBox must be initialized via
        // feistelBoxOnce.Do(initFeistelBox) first.
        private static (uint, uint) feistel(uint l, uint r, ulong k0, ulong k1)
        {
            uint lout = default;
            uint rout = default;

            uint t = default;

            t = r ^ uint32(k0 >> (int)(32L));
            l ^= feistelBox[7L][t & 0x3fUL] ^ feistelBox[5L][(t >> (int)(8L)) & 0x3fUL] ^ feistelBox[3L][(t >> (int)(16L)) & 0x3fUL] ^ feistelBox[1L][(t >> (int)(24L)) & 0x3fUL];

            t = ((r << (int)(28L)) | (r >> (int)(4L))) ^ uint32(k0);
            l ^= feistelBox[6L][(t) & 0x3fUL] ^ feistelBox[4L][(t >> (int)(8L)) & 0x3fUL] ^ feistelBox[2L][(t >> (int)(16L)) & 0x3fUL] ^ feistelBox[0L][(t >> (int)(24L)) & 0x3fUL];

            t = l ^ uint32(k1 >> (int)(32L));
            r ^= feistelBox[7L][t & 0x3fUL] ^ feistelBox[5L][(t >> (int)(8L)) & 0x3fUL] ^ feistelBox[3L][(t >> (int)(16L)) & 0x3fUL] ^ feistelBox[1L][(t >> (int)(24L)) & 0x3fUL];

            t = ((l << (int)(28L)) | (l >> (int)(4L))) ^ uint32(k1);
            r ^= feistelBox[6L][(t) & 0x3fUL] ^ feistelBox[4L][(t >> (int)(8L)) & 0x3fUL] ^ feistelBox[2L][(t >> (int)(16L)) & 0x3fUL] ^ feistelBox[0L][(t >> (int)(24L)) & 0x3fUL];

            return (l, r);
        }

        // feistelBox[s][16*i+j] contains the output of permutationFunction
        // for sBoxes[s][i][j] << 4*(7-s)
        private static array<array<uint>> feistelBox = new array<array<uint>>(8L);

        private static sync.Once feistelBoxOnce = default;

        // general purpose function to perform DES block permutations
        private static ulong permuteBlock(ulong src, slice<byte> permutation)
        {
            ulong block = default;

            foreach (var (position, n) in permutation)
            {
                var bit = (src >> (int)(n)) & 1L;
                block |= bit << (int)(uint((len(permutation) - 1L) - position));
            }
            return ;

        }

        private static void initFeistelBox()
        {
            foreach (var (s) in sBoxes)
            {
                for (long i = 0L; i < 4L; i++)
                {
                    for (long j = 0L; j < 16L; j++)
                    {
                        var f = uint64(sBoxes[s][i][j]) << (int)((4L * (7L - uint(s))));
                        f = permuteBlock(f, permutationFunction[..]); 

                        // Row is determined by the 1st and 6th bit.
                        // Column is the middle four bits.
                        var row = uint8(((i & 2L) << (int)(4L)) | i & 1L);
                        var col = uint8(j << (int)(1L));
                        var t = row | col; 

                        // The rotation was performed in the feistel rounds, being factored out and now mixed into the feistelBox.
                        f = (f << (int)(1L)) | (f >> (int)(31L));

                        feistelBox[s][t] = uint32(f);

                    }


                }


            }

        }

        // permuteInitialBlock is equivalent to the permutation defined
        // by initialPermutation.
        private static ulong permuteInitialBlock(ulong block)
        { 
            // block = b7 b6 b5 b4 b3 b2 b1 b0 (8 bytes)
            var b1 = block >> (int)(48L);
            var b2 = block << (int)(48L);
            block ^= b1 ^ b2 ^ b1 << (int)(48L) ^ b2 >> (int)(48L); 

            // block = b1 b0 b5 b4 b3 b2 b7 b6
            b1 = block >> (int)(32L) & 0xff00ffUL;
            b2 = (block & 0xff00ff00UL);
            block ^= b1 << (int)(32L) ^ b2 ^ b1 << (int)(8L) ^ b2 << (int)(24L); // exchange b0 b4 with b3 b7

            // block is now b1 b3 b5 b7 b0 b2 b4 b7, the permutation:
            //                  ...  8
            //                  ... 24
            //                  ... 40
            //                  ... 56
            //  7  6  5  4  3  2  1  0
            // 23 22 21 20 19 18 17 16
            //                  ... 32
            //                  ... 48

            // exchange 4,5,6,7 with 32,33,34,35 etc.
            b1 = block & 0x0f0f00000f0f0000UL;
            b2 = block & 0x0000f0f00000f0f0UL;
            block ^= b1 ^ b2 ^ b1 >> (int)(12L) ^ b2 << (int)(12L); 

            // block is the permutation:
            //
            //   [+8]         [+40]
            //
            //  7  6  5  4
            // 23 22 21 20
            //  3  2  1  0
            // 19 18 17 16    [+32]

            // exchange 0,1,4,5 with 18,19,22,23
            b1 = block & 0x3300330033003300UL;
            b2 = block & 0x00cc00cc00cc00ccUL;
            block ^= b1 ^ b2 ^ b1 >> (int)(6L) ^ b2 << (int)(6L); 

            // block is the permutation:
            // 15 14
            // 13 12
            // 11 10
            //  9  8
            //  7  6
            //  5  4
            //  3  2
            //  1  0 [+16] [+32] [+64]

            // exchange 0,2,4,6 with 9,11,13,15:
            b1 = block & 0xaaaaaaaa55555555UL;
            block ^= b1 ^ b1 >> (int)(33L) ^ b1 << (int)(33L); 

            // block is the permutation:
            // 6 14 22 30 38 46 54 62
            // 4 12 20 28 36 44 52 60
            // 2 10 18 26 34 42 50 58
            // 0  8 16 24 32 40 48 56
            // 7 15 23 31 39 47 55 63
            // 5 13 21 29 37 45 53 61
            // 3 11 19 27 35 43 51 59
            // 1  9 17 25 33 41 49 57
            return block;

        }

        // permuteInitialBlock is equivalent to the permutation defined
        // by finalPermutation.
        private static ulong permuteFinalBlock(ulong block)
        { 
            // Perform the same bit exchanges as permuteInitialBlock
            // but in reverse order.
            var b1 = block & 0xaaaaaaaa55555555UL;
            block ^= b1 ^ b1 >> (int)(33L) ^ b1 << (int)(33L);

            b1 = block & 0x3300330033003300UL;
            var b2 = block & 0x00cc00cc00cc00ccUL;
            block ^= b1 ^ b2 ^ b1 >> (int)(6L) ^ b2 << (int)(6L);

            b1 = block & 0x0f0f00000f0f0000UL;
            b2 = block & 0x0000f0f00000f0f0UL;
            block ^= b1 ^ b2 ^ b1 >> (int)(12L) ^ b2 << (int)(12L);

            b1 = block >> (int)(32L) & 0xff00ffUL;
            b2 = (block & 0xff00ff00UL);
            block ^= b1 << (int)(32L) ^ b2 ^ b1 << (int)(8L) ^ b2 << (int)(24L);

            b1 = block >> (int)(48L);
            b2 = block << (int)(48L);
            block ^= b1 ^ b2 ^ b1 << (int)(48L) ^ b2 >> (int)(48L);
            return block;

        }

        // creates 16 28-bit blocks rotated according
        // to the rotation schedule
        private static slice<uint> ksRotate(uint @in)
        {
            slice<uint> @out = default;

            out = make_slice<uint>(16L);
            var last = in;
            for (long i = 0L; i < 16L; i++)
            { 
                // 28-bit circular left shift
                var left = (last << (int)((4L + ksRotations[i]))) >> (int)(4L);
                var right = (last << (int)(4L)) >> (int)((32L - ksRotations[i]));
                out[i] = left | right;
                last = out[i];

            }

            return ;

        }

        // creates 16 56-bit subkeys from the original key
        private static void generateSubkeys(this ptr<desCipher> _addr_c, slice<byte> keyBytes)
        {
            ref desCipher c = ref _addr_c.val;

            feistelBoxOnce.Do(initFeistelBox); 

            // apply PC1 permutation to key
            var key = binary.BigEndian.Uint64(keyBytes);
            var permutedKey = permuteBlock(key, permutedChoice1[..]); 

            // rotate halves of permuted key according to the rotation schedule
            var leftRotations = ksRotate(uint32(permutedKey >> (int)(28L)));
            var rightRotations = ksRotate(uint32(permutedKey << (int)(4L)) >> (int)(4L)); 

            // generate subkeys
            for (long i = 0L; i < 16L; i++)
            { 
                // combine halves to form 56-bit input to PC2
                var pc2Input = uint64(leftRotations[i]) << (int)(28L) | uint64(rightRotations[i]); 
                // apply PC2 permutation to 7 byte input
                c.subkeys[i] = unpack(permuteBlock(pc2Input, permutedChoice2[..]));

            }


        }

        // Expand 48-bit input to 64-bit, with each 6-bit block padded by extra two bits at the top.
        // By doing so, we can have the input blocks (four bits each), and the key blocks (six bits each) well-aligned without
        // extra shifts/rotations for alignments.
        private static ulong unpack(ulong x)
        {
            ulong result = default;

            result = ((x >> (int)((6L * 1L))) & 0xffUL) << (int)((8L * 0L)) | ((x >> (int)((6L * 3L))) & 0xffUL) << (int)((8L * 1L)) | ((x >> (int)((6L * 5L))) & 0xffUL) << (int)((8L * 2L)) | ((x >> (int)((6L * 7L))) & 0xffUL) << (int)((8L * 3L)) | ((x >> (int)((6L * 0L))) & 0xffUL) << (int)((8L * 4L)) | ((x >> (int)((6L * 2L))) & 0xffUL) << (int)((8L * 5L)) | ((x >> (int)((6L * 4L))) & 0xffUL) << (int)((8L * 6L)) | ((x >> (int)((6L * 6L))) & 0xffUL) << (int)((8L * 7L));

            return result;
        }
    }
}}
