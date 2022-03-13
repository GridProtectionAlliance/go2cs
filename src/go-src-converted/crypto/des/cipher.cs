// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package des -- go2cs converted at 2022 March 13 05:34:15 UTC
// import "crypto/des" ==> using des = go.crypto.des_package
// Original source: C:\Program Files\Go\src\crypto\des\cipher.go
namespace go.crypto;

using cipher = crypto.cipher_package;
using subtle = crypto.@internal.subtle_package;
using binary = encoding.binary_package;
using strconv = strconv_package;


// The DES block size in bytes.

public static partial class des_package {

public static readonly nint BlockSize = 8;



public partial struct KeySizeError { // : nint
}

public static @string Error(this KeySizeError k) {
    return "crypto/des: invalid key size " + strconv.Itoa(int(k));
}

// desCipher is an instance of DES encryption.
private partial struct desCipher {
    public array<ulong> subkeys;
}

// NewCipher creates and returns a new cipher.Block.
public static (cipher.Block, error) NewCipher(slice<byte> key) {
    cipher.Block _p0 = default;
    error _p0 = default!;

    if (len(key) != 8) {
        return (null, error.As(KeySizeError(len(key)))!);
    }
    ptr<desCipher> c = @new<desCipher>();
    c.generateSubkeys(key);
    return (c, error.As(null!)!);
}

private static nint BlockSize(this ptr<desCipher> _addr_c) {
    ref desCipher c = ref _addr_c.val;

    return BlockSize;
}

private static void Encrypt(this ptr<desCipher> _addr_c, slice<byte> dst, slice<byte> src) => func((_, panic, _) => {
    ref desCipher c = ref _addr_c.val;

    if (len(src) < BlockSize) {
        panic("crypto/des: input not full block");
    }
    if (len(dst) < BlockSize) {
        panic("crypto/des: output not full block");
    }
    if (subtle.InexactOverlap(dst[..(int)BlockSize], src[..(int)BlockSize])) {
        panic("crypto/des: invalid buffer overlap");
    }
    encryptBlock(c.subkeys[..], dst, src);
});

private static void Decrypt(this ptr<desCipher> _addr_c, slice<byte> dst, slice<byte> src) => func((_, panic, _) => {
    ref desCipher c = ref _addr_c.val;

    if (len(src) < BlockSize) {
        panic("crypto/des: input not full block");
    }
    if (len(dst) < BlockSize) {
        panic("crypto/des: output not full block");
    }
    if (subtle.InexactOverlap(dst[..(int)BlockSize], src[..(int)BlockSize])) {
        panic("crypto/des: invalid buffer overlap");
    }
    decryptBlock(c.subkeys[..], dst, src);
});

// A tripleDESCipher is an instance of TripleDES encryption.
private partial struct tripleDESCipher {
    public desCipher cipher1;
    public desCipher cipher2;
    public desCipher cipher3;
}

// NewTripleDESCipher creates and returns a new cipher.Block.
public static (cipher.Block, error) NewTripleDESCipher(slice<byte> key) {
    cipher.Block _p0 = default;
    error _p0 = default!;

    if (len(key) != 24) {
        return (null, error.As(KeySizeError(len(key)))!);
    }
    ptr<tripleDESCipher> c = @new<tripleDESCipher>();
    c.cipher1.generateSubkeys(key[..(int)8]);
    c.cipher2.generateSubkeys(key[(int)8..(int)16]);
    c.cipher3.generateSubkeys(key[(int)16..]);
    return (c, error.As(null!)!);
}

private static nint BlockSize(this ptr<tripleDESCipher> _addr_c) {
    ref tripleDESCipher c = ref _addr_c.val;

    return BlockSize;
}

private static void Encrypt(this ptr<tripleDESCipher> _addr_c, slice<byte> dst, slice<byte> src) => func((_, panic, _) => {
    ref tripleDESCipher c = ref _addr_c.val;

    if (len(src) < BlockSize) {
        panic("crypto/des: input not full block");
    }
    if (len(dst) < BlockSize) {
        panic("crypto/des: output not full block");
    }
    if (subtle.InexactOverlap(dst[..(int)BlockSize], src[..(int)BlockSize])) {
        panic("crypto/des: invalid buffer overlap");
    }
    var b = binary.BigEndian.Uint64(src);
    b = permuteInitialBlock(b);
    var left = uint32(b >> 32);
    var right = uint32(b);

    left = (left << 1) | (left >> 31);
    right = (right << 1) | (right >> 31);

    {
        nint i__prev1 = i;

        for (nint i = 0; i < 8; i++) {
            left, right = feistel(left, right, c.cipher1.subkeys[2 * i], c.cipher1.subkeys[2 * i + 1]);
        }

        i = i__prev1;
    }
    {
        nint i__prev1 = i;

        for (i = 0; i < 8; i++) {
            right, left = feistel(right, left, c.cipher2.subkeys[15 - 2 * i], c.cipher2.subkeys[15 - (2 * i + 1)]);
        }

        i = i__prev1;
    }
    {
        nint i__prev1 = i;

        for (i = 0; i < 8; i++) {
            left, right = feistel(left, right, c.cipher3.subkeys[2 * i], c.cipher3.subkeys[2 * i + 1]);
        }

        i = i__prev1;
    }

    left = (left << 31) | (left >> 1);
    right = (right << 31) | (right >> 1);

    var preOutput = (uint64(right) << 32) | uint64(left);
    binary.BigEndian.PutUint64(dst, permuteFinalBlock(preOutput));
});

private static void Decrypt(this ptr<tripleDESCipher> _addr_c, slice<byte> dst, slice<byte> src) => func((_, panic, _) => {
    ref tripleDESCipher c = ref _addr_c.val;

    if (len(src) < BlockSize) {
        panic("crypto/des: input not full block");
    }
    if (len(dst) < BlockSize) {
        panic("crypto/des: output not full block");
    }
    if (subtle.InexactOverlap(dst[..(int)BlockSize], src[..(int)BlockSize])) {
        panic("crypto/des: invalid buffer overlap");
    }
    var b = binary.BigEndian.Uint64(src);
    b = permuteInitialBlock(b);
    var left = uint32(b >> 32);
    var right = uint32(b);

    left = (left << 1) | (left >> 31);
    right = (right << 1) | (right >> 31);

    {
        nint i__prev1 = i;

        for (nint i = 0; i < 8; i++) {
            left, right = feistel(left, right, c.cipher3.subkeys[15 - 2 * i], c.cipher3.subkeys[15 - (2 * i + 1)]);
        }

        i = i__prev1;
    }
    {
        nint i__prev1 = i;

        for (i = 0; i < 8; i++) {
            right, left = feistel(right, left, c.cipher2.subkeys[2 * i], c.cipher2.subkeys[2 * i + 1]);
        }

        i = i__prev1;
    }
    {
        nint i__prev1 = i;

        for (i = 0; i < 8; i++) {
            left, right = feistel(left, right, c.cipher1.subkeys[15 - 2 * i], c.cipher1.subkeys[15 - (2 * i + 1)]);
        }

        i = i__prev1;
    }

    left = (left << 31) | (left >> 1);
    right = (right << 31) | (right >> 1);

    var preOutput = (uint64(right) << 32) | uint64(left);
    binary.BigEndian.PutUint64(dst, permuteFinalBlock(preOutput));
});

} // end des_package
