// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !amd64 && !ppc64 && !ppc64le && !arm64
// +build !amd64,!ppc64,!ppc64le,!arm64

// package cipher -- go2cs converted at 2022 March 13 05:32:23 UTC
// import "crypto/cipher" ==> using cipher = go.crypto.cipher_package
// Original source: C:\Program Files\Go\src\crypto\cipher\xor_generic.go
namespace go.crypto;

using runtime = runtime_package;
using @unsafe = @unsafe_package;


// xorBytes xors the bytes in a and b. The destination should have enough
// space, otherwise xorBytes will panic. Returns the number of bytes xor'd.

public static partial class cipher_package {

private static nint xorBytes(slice<byte> dst, slice<byte> a, slice<byte> b) {
    var n = len(a);
    if (len(b) < n) {
        n = len(b);
    }
    if (n == 0) {
        return 0;
    }

    if (supportsUnaligned) 
        fastXORBytes(dst, a, b, n);
    else 
        // TODO(hanwen): if (dst, a, b) have common alignment
        // we could still try fastXORBytes. It is not clear
        // how often this happens, and it's only worth it if
        // the block encryption itself is hardware
        // accelerated.
        safeXORBytes(dst, a, b, n);
        return n;
}

private static readonly var wordSize = int(@unsafe.Sizeof(uintptr(0)));

private static readonly var supportsUnaligned = runtime.GOARCH == "386" || runtime.GOARCH == "ppc64" || runtime.GOARCH == "ppc64le" || runtime.GOARCH == "s390x";

// fastXORBytes xors in bulk. It only works on architectures that
// support unaligned read/writes.
// n needs to be smaller or equal than the length of a and b.


// fastXORBytes xors in bulk. It only works on architectures that
// support unaligned read/writes.
// n needs to be smaller or equal than the length of a and b.
private static void fastXORBytes(slice<byte> dst, slice<byte> a, slice<byte> b, nint n) { 
    // Assert dst has enough space
    _ = dst[n - 1];

    var w = n / wordSize;
    if (w > 0) {
        ptr<ptr<slice<System.UIntPtr>>> dw = new ptr<ptr<ptr<slice<System.UIntPtr>>>>(@unsafe.Pointer(_addr_dst));
        ptr<ptr<slice<System.UIntPtr>>> aw = new ptr<ptr<ptr<slice<System.UIntPtr>>>>(@unsafe.Pointer(_addr_a));
        ptr<ptr<slice<System.UIntPtr>>> bw = new ptr<ptr<ptr<slice<System.UIntPtr>>>>(@unsafe.Pointer(_addr_b));
        {
            nint i__prev1 = i;

            for (nint i = 0; i < w; i++) {
                dw[i] = aw[i] ^ bw[i];
            }


            i = i__prev1;
        }
    }
    {
        nint i__prev1 = i;

        for (i = (n - n % wordSize); i < n; i++) {
            dst[i] = a[i] ^ b[i];
        }

        i = i__prev1;
    }
}

// n needs to be smaller or equal than the length of a and b.
private static void safeXORBytes(slice<byte> dst, slice<byte> a, slice<byte> b, nint n) {
    for (nint i = 0; i < n; i++) {
        dst[i] = a[i] ^ b[i];
    }
}

// fastXORWords XORs multiples of 4 or 8 bytes (depending on architecture.)
// The arguments are assumed to be of equal length.
private static void fastXORWords(slice<byte> dst, slice<byte> a, slice<byte> b) {
    ptr<ptr<slice<System.UIntPtr>>> dw = new ptr<ptr<ptr<slice<System.UIntPtr>>>>(@unsafe.Pointer(_addr_dst));
    ptr<ptr<slice<System.UIntPtr>>> aw = new ptr<ptr<ptr<slice<System.UIntPtr>>>>(@unsafe.Pointer(_addr_a));
    ptr<ptr<slice<System.UIntPtr>>> bw = new ptr<ptr<ptr<slice<System.UIntPtr>>>>(@unsafe.Pointer(_addr_b));
    var n = len(b) / wordSize;
    for (nint i = 0; i < n; i++) {
        dw[i] = aw[i] ^ bw[i];
    }
}

// fastXORWords XORs multiples of 4 or 8 bytes (depending on architecture.)
// The slice arguments a and b are assumed to be of equal length.
private static void xorWords(slice<byte> dst, slice<byte> a, slice<byte> b) {
    if (supportsUnaligned) {
        fastXORWords(dst, a, b);
    }
    else
 {
        safeXORBytes(dst, a, b, len(b));
    }
}

} // end cipher_package
