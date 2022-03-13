// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package curve25519 provides an implementation of the X25519 function, which
// performs scalar multiplication on the elliptic curve known as Curve25519.
// See RFC 7748.

// package curve25519 -- go2cs converted at 2022 March 13 06:44:42 UTC
// import "vendor/golang.org/x/crypto/curve25519" ==> using curve25519 = go.vendor.golang.org.x.crypto.curve25519_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\crypto\curve25519\curve25519.go
namespace go.vendor.golang.org.x.crypto;
// import "golang.org/x/crypto/curve25519"


using subtle = crypto.subtle_package;
using fmt = fmt_package;


// ScalarMult sets dst to the product scalar * point.
//
// Deprecated: when provided a low-order point, ScalarMult will set dst to all
// zeroes, irrespective of the scalar. Instead, use the X25519 function, which
// will return an error.

public static partial class curve25519_package {

public static void ScalarMult(ptr<array<byte>> _addr_dst, ptr<array<byte>> _addr_scalar, ptr<array<byte>> _addr_point) {
    ref array<byte> dst = ref _addr_dst.val;
    ref array<byte> scalar = ref _addr_scalar.val;
    ref array<byte> point = ref _addr_point.val;

    scalarMult(dst, scalar, point);
}

// ScalarBaseMult sets dst to the product scalar * base where base is the
// standard generator.
//
// It is recommended to use the X25519 function with Basepoint instead, as
// copying into fixed size arrays can lead to unexpected bugs.
public static void ScalarBaseMult(ptr<array<byte>> _addr_dst, ptr<array<byte>> _addr_scalar) {
    ref array<byte> dst = ref _addr_dst.val;
    ref array<byte> scalar = ref _addr_scalar.val;

    ScalarMult(_addr_dst, _addr_scalar, _addr_basePoint);
}

 
// ScalarSize is the size of the scalar input to X25519.
public static readonly nint ScalarSize = 32; 
// PointSize is the size of the point input to X25519.
public static readonly nint PointSize = 32;

// Basepoint is the canonical Curve25519 generator.
public static slice<byte> Basepoint = default;

private static array<byte> basePoint = new array<byte>(new byte[] { 9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

private static void init() {
    Basepoint = basePoint[..];
}

private static void checkBasepoint() => func((_, panic, _) => {
    if (subtle.ConstantTimeCompare(Basepoint, new slice<byte>(new byte[] { 0x09, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 })) != 1) {
        panic("curve25519: global Basepoint value was modified");
    }
});

// X25519 returns the result of the scalar multiplication (scalar * point),
// according to RFC 7748, Section 5. scalar, point and the return value are
// slices of 32 bytes.
//
// scalar can be generated at random, for example with crypto/rand. point should
// be either Basepoint or the output of another X25519 call.
//
// If point is Basepoint (but not if it's a different slice with the same
// contents) a precomputed implementation might be used for performance.
public static (slice<byte>, error) X25519(slice<byte> scalar, slice<byte> point) {
    slice<byte> _p0 = default;
    error _p0 = default!;
 
    // Outline the body of function, to let the allocation be inlined in the
    // caller, and possibly avoid escaping to the heap.
    ref array<byte> dst = ref heap(new array<byte>(32), out ptr<array<byte>> _addr_dst);
    return x25519(_addr_dst, scalar, point);
}

private static (slice<byte>, error) x25519(ptr<array<byte>> _addr_dst, slice<byte> scalar, slice<byte> point) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref array<byte> dst = ref _addr_dst.val;

    ref array<byte> @in = ref heap(new array<byte>(32), out ptr<array<byte>> _addr_@in);
    {
        var l__prev1 = l;

        var l = len(scalar);

        if (l != 32) {
            return (null, error.As(fmt.Errorf("bad scalar length: %d, expected %d", l, 32))!);
        }
        l = l__prev1;

    }
    {
        var l__prev1 = l;

        l = len(point);

        if (l != 32) {
            return (null, error.As(fmt.Errorf("bad point length: %d, expected %d", l, 32))!);
        }
        l = l__prev1;

    }
    copy(in[..], scalar);
    if (_addr_point[0] == _addr_Basepoint[0]) {
        checkBasepoint();
        ScalarBaseMult(_addr_dst, _addr_in);
    }
    else
 {
        ref array<byte> @base = ref heap(new array<byte>(32), out ptr<array<byte>> _addr_@base);        array<byte> zero = new array<byte>(32);

        copy(base[..], point);
        ScalarMult(_addr_dst, _addr_in, _addr_base);
        if (subtle.ConstantTimeCompare(dst[..], zero[..]) == 1) {
            return (null, error.As(fmt.Errorf("bad input point: low order point"))!);
        }
    }
    return (dst[..], error.As(null!)!);
}

} // end curve25519_package
