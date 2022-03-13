// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package des -- go2cs converted at 2022 March 13 05:34:14 UTC
// import "crypto/des" ==> using des = go.crypto.des_package
// Original source: C:\Program Files\Go\src\crypto\des\block.go
namespace go.crypto;

using binary = encoding.binary_package;
using sync = sync_package;

public static partial class des_package {

private static void cryptBlock(slice<ulong> subkeys, slice<byte> dst, slice<byte> src, bool decrypt) {
    var b = binary.BigEndian.Uint64(src);
    b = permuteInitialBlock(b);
    var left = uint32(b >> 32);
    var right = uint32(b);

    left = (left << 1) | (left >> 31);
    right = (right << 1) | (right >> 31);

    if (decrypt) {
        {
            nint i__prev1 = i;

            for (nint i = 0; i < 8; i++) {
                left, right = feistel(left, right, subkeys[15 - 2 * i], subkeys[15 - (2 * i + 1)]);
            }
    else


            i = i__prev1;
        }
    } {
        {
            nint i__prev1 = i;

            for (i = 0; i < 8; i++) {
                left, right = feistel(left, right, subkeys[2 * i], subkeys[2 * i + 1]);
            }

            i = i__prev1;
        }
    }
    left = (left << 31) | (left >> 1);
    right = (right << 31) | (right >> 1); 

    // switch left & right and perform final permutation
    var preOutput = (uint64(right) << 32) | uint64(left);
    binary.BigEndian.PutUint64(dst, permuteFinalBlock(preOutput));
}

// Encrypt one block from src into dst, using the subkeys.
private static void encryptBlock(slice<ulong> subkeys, slice<byte> dst, slice<byte> src) {
    cryptBlock(subkeys, dst, src, false);
}

// Decrypt one block from src into dst, using the subkeys.
private static void decryptBlock(slice<ulong> subkeys, slice<byte> dst, slice<byte> src) {
    cryptBlock(subkeys, dst, src, true);
}

// DES Feistel function. feistelBox must be initialized via
// feistelBoxOnce.Do(initFeistelBox) first.
private static (uint, uint) feistel(uint l, uint r, ulong k0, ulong k1) {
    uint lout = default;
    uint rout = default;

    uint t = default;

    t = r ^ uint32(k0 >> 32);
    l ^= feistelBox[7][t & 0x3f] ^ feistelBox[5][(t >> 8) & 0x3f] ^ feistelBox[3][(t >> 16) & 0x3f] ^ feistelBox[1][(t >> 24) & 0x3f];

    t = ((r << 28) | (r >> 4)) ^ uint32(k0);
    l ^= feistelBox[6][(t) & 0x3f] ^ feistelBox[4][(t >> 8) & 0x3f] ^ feistelBox[2][(t >> 16) & 0x3f] ^ feistelBox[0][(t >> 24) & 0x3f];

    t = l ^ uint32(k1 >> 32);
    r ^= feistelBox[7][t & 0x3f] ^ feistelBox[5][(t >> 8) & 0x3f] ^ feistelBox[3][(t >> 16) & 0x3f] ^ feistelBox[1][(t >> 24) & 0x3f];

    t = ((l << 28) | (l >> 4)) ^ uint32(k1);
    r ^= feistelBox[6][(t) & 0x3f] ^ feistelBox[4][(t >> 8) & 0x3f] ^ feistelBox[2][(t >> 16) & 0x3f] ^ feistelBox[0][(t >> 24) & 0x3f];

    return (l, r);
}

// feistelBox[s][16*i+j] contains the output of permutationFunction
// for sBoxes[s][i][j] << 4*(7-s)
private static array<array<uint>> feistelBox = new array<array<uint>>(8);

private static sync.Once feistelBoxOnce = default;

// general purpose function to perform DES block permutations
private static ulong permuteBlock(ulong src, slice<byte> permutation) {
    ulong block = default;

    foreach (var (position, n) in permutation) {
        var bit = (src >> (int)(n)) & 1;
        block |= bit << (int)(uint((len(permutation) - 1) - position));
    }    return ;
}

private static void initFeistelBox() {
    foreach (var (s) in sBoxes) {
        for (nint i = 0; i < 4; i++) {
            for (nint j = 0; j < 16; j++) {
                var f = uint64(sBoxes[s][i][j]) << (int)((4 * (7 - uint(s))));
                f = permuteBlock(f, permutationFunction[..]); 

                // Row is determined by the 1st and 6th bit.
                // Column is the middle four bits.
                var row = uint8(((i & 2) << 4) | i & 1);
                var col = uint8(j << 1);
                var t = row | col; 

                // The rotation was performed in the feistel rounds, being factored out and now mixed into the feistelBox.
                f = (f << 1) | (f >> 31);

                feistelBox[s][t] = uint32(f);
            }
        }
    }
}

// permuteInitialBlock is equivalent to the permutation defined
// by initialPermutation.
private static ulong permuteInitialBlock(ulong block) { 
    // block = b7 b6 b5 b4 b3 b2 b1 b0 (8 bytes)
    var b1 = block >> 48;
    var b2 = block << 48;
    block ^= b1 ^ b2 ^ b1 << 48 ^ b2 >> 48; 

    // block = b1 b0 b5 b4 b3 b2 b7 b6
    b1 = block >> 32 & 0xff00ff;
    b2 = (block & 0xff00ff00);
    block ^= b1 << 32 ^ b2 ^ b1 << 8 ^ b2 << 24; // exchange b0 b4 with b3 b7

    // block is now b1 b3 b5 b7 b0 b2 b4 b6, the permutation:
    //                  ...  8
    //                  ... 24
    //                  ... 40
    //                  ... 56
    //  7  6  5  4  3  2  1  0
    // 23 22 21 20 19 18 17 16
    //                  ... 32
    //                  ... 48

    // exchange 4,5,6,7 with 32,33,34,35 etc.
    b1 = block & 0x0f0f00000f0f0000;
    b2 = block & 0x0000f0f00000f0f0;
    block ^= b1 ^ b2 ^ b1 >> 12 ^ b2 << 12; 

    // block is the permutation:
    //
    //   [+8]         [+40]
    //
    //  7  6  5  4
    // 23 22 21 20
    //  3  2  1  0
    // 19 18 17 16    [+32]

    // exchange 0,1,4,5 with 18,19,22,23
    b1 = block & 0x3300330033003300;
    b2 = block & 0x00cc00cc00cc00cc;
    block ^= b1 ^ b2 ^ b1 >> 6 ^ b2 << 6; 

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
    b1 = block & 0xaaaaaaaa55555555;
    block ^= b1 ^ b1 >> 33 ^ b1 << 33; 

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
private static ulong permuteFinalBlock(ulong block) { 
    // Perform the same bit exchanges as permuteInitialBlock
    // but in reverse order.
    var b1 = block & 0xaaaaaaaa55555555;
    block ^= b1 ^ b1 >> 33 ^ b1 << 33;

    b1 = block & 0x3300330033003300;
    var b2 = block & 0x00cc00cc00cc00cc;
    block ^= b1 ^ b2 ^ b1 >> 6 ^ b2 << 6;

    b1 = block & 0x0f0f00000f0f0000;
    b2 = block & 0x0000f0f00000f0f0;
    block ^= b1 ^ b2 ^ b1 >> 12 ^ b2 << 12;

    b1 = block >> 32 & 0xff00ff;
    b2 = (block & 0xff00ff00);
    block ^= b1 << 32 ^ b2 ^ b1 << 8 ^ b2 << 24;

    b1 = block >> 48;
    b2 = block << 48;
    block ^= b1 ^ b2 ^ b1 << 48 ^ b2 >> 48;
    return block;
}

// creates 16 28-bit blocks rotated according
// to the rotation schedule
private static slice<uint> ksRotate(uint @in) {
    slice<uint> @out = default;

    out = make_slice<uint>(16);
    var last = in;
    for (nint i = 0; i < 16; i++) { 
        // 28-bit circular left shift
        var left = (last << (int)((4 + ksRotations[i]))) >> 4;
        var right = (last << 4) >> (int)((32 - ksRotations[i]));
        out[i] = left | right;
        last = out[i];
    }
    return ;
}

// creates 16 56-bit subkeys from the original key
private static void generateSubkeys(this ptr<desCipher> _addr_c, slice<byte> keyBytes) {
    ref desCipher c = ref _addr_c.val;

    feistelBoxOnce.Do(initFeistelBox); 

    // apply PC1 permutation to key
    var key = binary.BigEndian.Uint64(keyBytes);
    var permutedKey = permuteBlock(key, permutedChoice1[..]); 

    // rotate halves of permuted key according to the rotation schedule
    var leftRotations = ksRotate(uint32(permutedKey >> 28));
    var rightRotations = ksRotate(uint32(permutedKey << 4) >> 4); 

    // generate subkeys
    for (nint i = 0; i < 16; i++) { 
        // combine halves to form 56-bit input to PC2
        var pc2Input = uint64(leftRotations[i]) << 28 | uint64(rightRotations[i]); 
        // apply PC2 permutation to 7 byte input
        c.subkeys[i] = unpack(permuteBlock(pc2Input, permutedChoice2[..]));
    }
}

// Expand 48-bit input to 64-bit, with each 6-bit block padded by extra two bits at the top.
// By doing so, we can have the input blocks (four bits each), and the key blocks (six bits each) well-aligned without
// extra shifts/rotations for alignments.
private static ulong unpack(ulong x) {
    ulong result = default;

    result = ((x >> (int)((6 * 1))) & 0xff) << (int)((8 * 0)) | ((x >> (int)((6 * 3))) & 0xff) << (int)((8 * 1)) | ((x >> (int)((6 * 5))) & 0xff) << (int)((8 * 2)) | ((x >> (int)((6 * 7))) & 0xff) << (int)((8 * 3)) | ((x >> (int)((6 * 0))) & 0xff) << (int)((8 * 4)) | ((x >> (int)((6 * 2))) & 0xff) << (int)((8 * 5)) | ((x >> (int)((6 * 4))) & 0xff) << (int)((8 * 6)) | ((x >> (int)((6 * 6))) & 0xff) << (int)((8 * 7));

    return result;
}

} // end des_package
