// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package fiat implements prime order fields using formally verified algorithms
// from the Fiat Cryptography project.
// package fiat -- go2cs converted at 2022 March 06 22:19:11 UTC
// import "crypto/elliptic/internal/fiat" ==> using fiat = go.crypto.elliptic.@internal.fiat_package
// Original source: C:\Program Files\Go\src\crypto\elliptic\internal\fiat\p521.go
using subtle = go.crypto.subtle_package;
using errors = go.errors_package;

namespace go.crypto.elliptic.@internal;

public static partial class fiat_package {

    // P521Element is an integer modulo 2^521 - 1.
    //
    // The zero value is a valid zero element.
public partial struct P521Element {
    public array<ulong> x;
}

// One sets e = 1, and returns e.
private static ptr<P521Element> One(this ptr<P521Element> _addr_e) {
    ref P521Element e = ref _addr_e.val;

    e.val = new P521Element();
    e.x[0] = 1;
    return _addr_e!;
}

// Equal returns 1 if e == t, and zero otherwise.
private static nint Equal(this ptr<P521Element> _addr_e, ptr<P521Element> _addr_t) {
    ref P521Element e = ref _addr_e.val;
    ref P521Element t = ref _addr_t.val;

    var eBytes = e.Bytes();
    var tBytes = t.Bytes();
    return subtle.ConstantTimeCompare(eBytes, tBytes);
}

private static ptr<P521Element> p521ZeroEncoding = @new<P521Element>().Bytes();

// IsZero returns 1 if e == 0, and zero otherwise.
private static nint IsZero(this ptr<P521Element> _addr_e) {
    ref P521Element e = ref _addr_e.val;

    var eBytes = e.Bytes();
    return subtle.ConstantTimeCompare(eBytes, p521ZeroEncoding);
}

// Set sets e = t, and returns e.
private static ptr<P521Element> Set(this ptr<P521Element> _addr_e, ptr<P521Element> _addr_t) {
    ref P521Element e = ref _addr_e.val;
    ref P521Element t = ref _addr_t.val;

    e.x = t.x;
    return _addr_e!;
}

// Bytes returns the 66-byte little-endian encoding of e.
private static slice<byte> Bytes(this ptr<P521Element> _addr_e) {
    ref P521Element e = ref _addr_e.val;
 
    // This function must be inlined to move the allocation to the parent and
    // save it from escaping to the heap.
    ref array<byte> @out = ref heap(new array<byte>(66), out ptr<array<byte>> _addr_@out);
    p521ToBytes(_addr_out, _addr_e.x);
    return out[..];

}

// SetBytes sets e = v, where v is a little-endian 66-byte encoding, and returns
// e. If v is not 66 bytes or it encodes a value higher than 2^521 - 1, SetBytes
// returns nil and an error, and e is unchanged.
private static (ptr<P521Element>, error) SetBytes(this ptr<P521Element> _addr_e, slice<byte> v) {
    ptr<P521Element> _p0 = default!;
    error _p0 = default!;
    ref P521Element e = ref _addr_e.val;

    if (len(v) != 66 || v[65] > 1) {
        return (_addr_null!, error.As(errors.New("invalid P-521 field encoding"))!);
    }
    ref array<byte> @in = ref heap(new array<byte>(66), out ptr<array<byte>> _addr_@in);
    copy(in[..], v);
    p521FromBytes(_addr_e.x, _addr_in);
    return (_addr_e!, error.As(null!)!);

}

// Add sets e = t1 + t2, and returns e.
private static ptr<P521Element> Add(this ptr<P521Element> _addr_e, ptr<P521Element> _addr_t1, ptr<P521Element> _addr_t2) {
    ref P521Element e = ref _addr_e.val;
    ref P521Element t1 = ref _addr_t1.val;
    ref P521Element t2 = ref _addr_t2.val;

    p521Add(_addr_e.x, _addr_t1.x, _addr_t2.x);
    p521Carry(_addr_e.x, _addr_e.x);
    return _addr_e!;
}

// Sub sets e = t1 - t2, and returns e.
private static ptr<P521Element> Sub(this ptr<P521Element> _addr_e, ptr<P521Element> _addr_t1, ptr<P521Element> _addr_t2) {
    ref P521Element e = ref _addr_e.val;
    ref P521Element t1 = ref _addr_t1.val;
    ref P521Element t2 = ref _addr_t2.val;

    p521Sub(_addr_e.x, _addr_t1.x, _addr_t2.x);
    p521Carry(_addr_e.x, _addr_e.x);
    return _addr_e!;
}

// Mul sets e = t1 * t2, and returns e.
private static ptr<P521Element> Mul(this ptr<P521Element> _addr_e, ptr<P521Element> _addr_t1, ptr<P521Element> _addr_t2) {
    ref P521Element e = ref _addr_e.val;
    ref P521Element t1 = ref _addr_t1.val;
    ref P521Element t2 = ref _addr_t2.val;

    p521CarryMul(_addr_e.x, _addr_t1.x, _addr_t2.x);
    return _addr_e!;
}

// Square sets e = t * t, and returns e.
private static ptr<P521Element> Square(this ptr<P521Element> _addr_e, ptr<P521Element> _addr_t) {
    ref P521Element e = ref _addr_e.val;
    ref P521Element t = ref _addr_t.val;

    p521CarrySquare(_addr_e.x, _addr_t.x);
    return _addr_e!;
}

// Select sets e to a if cond == 1, and to b if cond == 0.
private static ptr<P521Element> Select(this ptr<P521Element> _addr_v, ptr<P521Element> _addr_a, ptr<P521Element> _addr_b, nint cond) {
    ref P521Element v = ref _addr_v.val;
    ref P521Element a = ref _addr_a.val;
    ref P521Element b = ref _addr_b.val;

    p521Selectznz(_addr_v.x, p521Uint1(cond), _addr_b.x, _addr_a.x);
    return _addr_v!;
}

// Invert sets e = 1/t, and returns e.
//
// If t == 0, Invert returns e = 0.
private static ptr<P521Element> Invert(this ptr<P521Element> _addr_e, ptr<P521Element> _addr_t) {
    ref P521Element e = ref _addr_e.val;
    ref P521Element t = ref _addr_t.val;
 
    // Inversion is implemented as exponentiation with exponent p − 2.
    // The sequence of multiplications and squarings was generated with
    // github.com/mmcloughlin/addchain v0.2.0.

    ptr<P521Element> t1 = @new<P521Element>();    ptr<P521Element> t2 = @new<P521Element>(); 

    // _10 = 2 * 1
 

    // _10 = 2 * 1
    t1.Square(t); 

    // _11 = 1 + _10
    t1.Mul(t, t1); 

    // _1100 = _11 << 2
    t2.Square(t1);
    t2.Square(t2); 

    // _1111 = _11 + _1100
    t1.Mul(t1, t2); 

    // _11110000 = _1111 << 4
    t2.Square(t1);
    {
        nint i__prev1 = i;

        for (nint i = 0; i < 3; i++) {
            t2.Square(t2);
        }

        i = i__prev1;
    } 

    // _11111111 = _1111 + _11110000
    t1.Mul(t1, t2); 

    // x16 = _11111111<<8 + _11111111
    t2.Square(t1);
    {
        nint i__prev1 = i;

        for (i = 0; i < 7; i++) {
            t2.Square(t2);
        }

        i = i__prev1;
    }
    t1.Mul(t1, t2); 

    // x32 = x16<<16 + x16
    t2.Square(t1);
    {
        nint i__prev1 = i;

        for (i = 0; i < 15; i++) {
            t2.Square(t2);
        }

        i = i__prev1;
    }
    t1.Mul(t1, t2); 

    // x64 = x32<<32 + x32
    t2.Square(t1);
    {
        nint i__prev1 = i;

        for (i = 0; i < 31; i++) {
            t2.Square(t2);
        }

        i = i__prev1;
    }
    t1.Mul(t1, t2); 

    // x65 = 2*x64 + 1
    t2.Square(t1);
    t2.Mul(t2, t); 

    // x129 = x65<<64 + x64
    {
        nint i__prev1 = i;

        for (i = 0; i < 64; i++) {
            t2.Square(t2);
        }

        i = i__prev1;
    }
    t1.Mul(t1, t2); 

    // x130 = 2*x129 + 1
    t2.Square(t1);
    t2.Mul(t2, t); 

    // x259 = x130<<129 + x129
    {
        nint i__prev1 = i;

        for (i = 0; i < 129; i++) {
            t2.Square(t2);
        }

        i = i__prev1;
    }
    t1.Mul(t1, t2); 

    // x260 = 2*x259 + 1
    t2.Square(t1);
    t2.Mul(t2, t); 

    // x519 = x260<<259 + x259
    {
        nint i__prev1 = i;

        for (i = 0; i < 259; i++) {
            t2.Square(t2);
        }

        i = i__prev1;
    }
    t1.Mul(t1, t2); 

    // return x519<<2 + 1
    t1.Square(t1);
    t1.Square(t1);
    return _addr_e.Mul(t1, t)!;

}

} // end fiat_package
