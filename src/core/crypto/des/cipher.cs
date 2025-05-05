// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using cipher = crypto.cipher_package;
using alias = crypto.@internal.alias_package;
using byteorder = @internal.byteorder_package;
using strconv = strconv_package;
using @internal;
using crypto.@internal;

partial class des_package {

// The DES block size in bytes.
public static readonly UntypedInt ΔBlockSize = 8;

[GoType("num:nint")] partial struct KeySizeError;

public static @string Error(this KeySizeError k) {
    return "crypto/des: invalid key size "u8 + strconv.Itoa(((nint)k));
}

// desCipher is an instance of DES encryption.
[GoType] partial struct desCipher {
    internal array<uint64> subkeys = new(16);
}

// NewCipher creates and returns a new [cipher.Block].
public static (cipher.Block, error) NewCipher(slice<byte> key) {
    if (len(key) != 8) {
        return (default!, ((KeySizeError)len(key)));
    }
    var c = @new<desCipher>();
    c.generateSubkeys(key);
    return (~c, default!);
}

[GoRecv] internal static nint BlockSize(this ref desCipher c) {
    return ΔBlockSize;
}

[GoRecv] internal static void Encrypt(this ref desCipher c, slice<byte> dst, slice<byte> src) {
    if (len(src) < ΔBlockSize) {
        throw panic("crypto/des: input not full block");
    }
    if (len(dst) < ΔBlockSize) {
        throw panic("crypto/des: output not full block");
    }
    if (alias.InexactOverlap(dst[..(int)(ΔBlockSize)], src[..(int)(ΔBlockSize)])) {
        throw panic("crypto/des: invalid buffer overlap");
    }
    cryptBlock(c.subkeys[..], dst, src, false);
}

[GoRecv] internal static void Decrypt(this ref desCipher c, slice<byte> dst, slice<byte> src) {
    if (len(src) < ΔBlockSize) {
        throw panic("crypto/des: input not full block");
    }
    if (len(dst) < ΔBlockSize) {
        throw panic("crypto/des: output not full block");
    }
    if (alias.InexactOverlap(dst[..(int)(ΔBlockSize)], src[..(int)(ΔBlockSize)])) {
        throw panic("crypto/des: invalid buffer overlap");
    }
    cryptBlock(c.subkeys[..], dst, src, true);
}

// A tripleDESCipher is an instance of TripleDES encryption.
[GoType] partial struct tripleDESCipher {
    internal desCipher cipher1;
    internal desCipher cipher2;
    internal desCipher cipher3;
}

// NewTripleDESCipher creates and returns a new [cipher.Block].
public static (cipher.Block, error) NewTripleDESCipher(slice<byte> key) {
    if (len(key) != 24) {
        return (default!, ((KeySizeError)len(key)));
    }
    var c = @new<tripleDESCipher>();
    (~c).cipher1.generateSubkeys(key[..8]);
    (~c).cipher2.generateSubkeys(key[8..16]);
    (~c).cipher3.generateSubkeys(key[16..]);
    return (~c, default!);
}

[GoRecv] internal static nint BlockSize(this ref tripleDESCipher c) {
    return ΔBlockSize;
}

[GoRecv] internal static void Encrypt(this ref tripleDESCipher c, slice<byte> dst, slice<byte> src) {
    if (len(src) < ΔBlockSize) {
        throw panic("crypto/des: input not full block");
    }
    if (len(dst) < ΔBlockSize) {
        throw panic("crypto/des: output not full block");
    }
    if (alias.InexactOverlap(dst[..(int)(ΔBlockSize)], src[..(int)(ΔBlockSize)])) {
        throw panic("crypto/des: invalid buffer overlap");
    }
    var b = byteorder.BeUint64(src);
    b = permuteInitialBlock(b);
    var (left, right) = (((uint32)(b >> (int)(32))), ((uint32)b));
    left = (uint32)((left << (int)(1)) | (left >> (int)(31)));
    right = (uint32)((right << (int)(1)) | (right >> (int)(31)));
    for (nint i = 0; i < 8; i++) {
        (left, right) = feistel(left, right, c.cipher1.subkeys[2 * i], c.cipher1.subkeys[2 * i + 1]);
    }
    for (nint i = 0; i < 8; i++) {
        (right, left) = feistel(right, left, c.cipher2.subkeys[15 - 2 * i], c.cipher2.subkeys[15 - (2 * i + 1)]);
    }
    for (nint i = 0; i < 8; i++) {
        (left, right) = feistel(left, right, c.cipher3.subkeys[2 * i], c.cipher3.subkeys[2 * i + 1]);
    }
    left = (uint32)((left << (int)(31)) | (left >> (int)(1)));
    right = (uint32)((right << (int)(31)) | (right >> (int)(1)));
    var preOutput = (uint64)((((uint64)right) << (int)(32)) | ((uint64)left));
    byteorder.BePutUint64(dst, permuteFinalBlock(preOutput));
}

[GoRecv] internal static void Decrypt(this ref tripleDESCipher c, slice<byte> dst, slice<byte> src) {
    if (len(src) < ΔBlockSize) {
        throw panic("crypto/des: input not full block");
    }
    if (len(dst) < ΔBlockSize) {
        throw panic("crypto/des: output not full block");
    }
    if (alias.InexactOverlap(dst[..(int)(ΔBlockSize)], src[..(int)(ΔBlockSize)])) {
        throw panic("crypto/des: invalid buffer overlap");
    }
    var b = byteorder.BeUint64(src);
    b = permuteInitialBlock(b);
    var (left, right) = (((uint32)(b >> (int)(32))), ((uint32)b));
    left = (uint32)((left << (int)(1)) | (left >> (int)(31)));
    right = (uint32)((right << (int)(1)) | (right >> (int)(31)));
    for (nint i = 0; i < 8; i++) {
        (left, right) = feistel(left, right, c.cipher3.subkeys[15 - 2 * i], c.cipher3.subkeys[15 - (2 * i + 1)]);
    }
    for (nint i = 0; i < 8; i++) {
        (right, left) = feistel(right, left, c.cipher2.subkeys[2 * i], c.cipher2.subkeys[2 * i + 1]);
    }
    for (nint i = 0; i < 8; i++) {
        (left, right) = feistel(left, right, c.cipher1.subkeys[15 - 2 * i], c.cipher1.subkeys[15 - (2 * i + 1)]);
    }
    left = (uint32)((left << (int)(31)) | (left >> (int)(1)));
    right = (uint32)((right << (int)(31)) | (right >> (int)(1)));
    var preOutput = (uint64)((((uint64)right) << (int)(32)) | ((uint64)left));
    byteorder.BePutUint64(dst, permuteFinalBlock(preOutput));
}

} // end des_package
