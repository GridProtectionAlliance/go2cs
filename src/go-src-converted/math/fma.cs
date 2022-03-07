// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2022 March 06 22:31:05 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Program Files\Go\src\math\fma.go
using bits = go.math.bits_package;

namespace go;

public static partial class math_package {

private static ulong zero(ulong x) {
    if (x == 0) {
        return 1;
    }
    return 0; 
    // branchless:
    // return ((x>>1 | x&1) - 1) >> 63
}

private static ulong nonzero(ulong x) {
    if (x != 0) {
        return 1;
    }
    return 0; 
    // branchless:
    // return 1 - ((x>>1|x&1)-1)>>63
}

private static (ulong, ulong) shl(ulong u1, ulong u2, nuint n) {
    ulong r1 = default;
    ulong r2 = default;

    r1 = u1 << (int)(n) | u2 >> (int)((64 - n)) | u2 << (int)((n - 64));
    r2 = u2 << (int)(n);
    return ;
}

private static (ulong, ulong) shr(ulong u1, ulong u2, nuint n) {
    ulong r1 = default;
    ulong r2 = default;

    r2 = u2 >> (int)(n) | u1 << (int)((64 - n)) | u1 >> (int)((n - 64));
    r1 = u1 >> (int)(n);
    return ;
}

// shrcompress compresses the bottom n+1 bits of the two-word
// value into a single bit. the result is equal to the value
// shifted to the right by n, except the result's 0th bit is
// set to the bitwise OR of the bottom n+1 bits.
private static (ulong, ulong) shrcompress(ulong u1, ulong u2, nuint n) {
    ulong r1 = default;
    ulong r2 = default;
 
    // TODO: Performance here is really sensitive to the
    // order/placement of these branches. n == 0 is common
    // enough to be in the fast path. Perhaps more measurement
    // needs to be done to find the optimal order/placement?

    if (n == 0) 
        return (u1, u2);
    else if (n == 64) 
        return (0, u1 | nonzero(u2));
    else if (n >= 128) 
        return (0, nonzero(u1 | u2));
    else if (n < 64) 
        r1, r2 = shr(u1, u2, n);
        r2 |= nonzero(u2 & (1 << (int)(n) - 1));
    else if (n < 128) 
        r1, r2 = shr(u1, u2, n);
        r2 |= nonzero(u1 & (1 << (int)((n - 64)) - 1) | u2);
        return ;

}

private static int lz(ulong u1, ulong u2) {
    int l = default;

    l = int32(bits.LeadingZeros64(u1));
    if (l == 64) {
        l += int32(bits.LeadingZeros64(u2));
    }
    return l;

}

// split splits b into sign, biased exponent, and mantissa.
// It adds the implicit 1 bit to the mantissa for normal values,
// and normalizes subnormal values.
private static (uint, int, ulong) split(ulong b) {
    uint sign = default;
    int exp = default;
    ulong mantissa = default;

    sign = uint32(b >> 63);
    exp = int32(b >> 52) & mask;
    mantissa = b & fracMask;

    if (exp == 0) { 
        // Normalize value if subnormal.
        var shift = uint(bits.LeadingZeros64(mantissa) - 11);
        mantissa<<=shift;
        exp = 1 - int32(shift);

    }
    else
 { 
        // Add implicit 1 bit
        mantissa |= 1 << 52;

    }
    return ;

}

// FMA returns x * y + z, computed with only one rounding.
// (That is, FMA returns the fused multiply-add of x, y, and z.)
public static double FMA(double x, double y, double z) {
    var bx = Float64bits(x);
    var by = Float64bits(y);
    var bz = Float64bits(z); 

    // Inf or NaN or zero involved. At most one rounding will occur.
    if (x == 0.0F || y == 0.0F || z == 0.0F || bx & uvinf == uvinf || by & uvinf == uvinf) {
        return x * y + z;
    }
    if (bz & uvinf == uvinf) {
        return z;
    }
    var (xs, xe, xm) = split(bx);
    var (ys, ye, ym) = split(by);
    var (zs, ze, zm) = split(bz); 

    // Compute product p = x*y as sign, exponent, two-word mantissa.
    // Start with exponent. "is normal" bit isn't subtracted yet.
    var pe = xe + ye - bias + 1; 

    // pm1:pm2 is the double-word mantissa for the product p.
    // Shift left to leave top bit in product. Effectively
    // shifts the 106-bit product to the left by 21.
    var (pm1, pm2) = bits.Mul64(xm << 10, ym << 11);
    var zm1 = zm << 10;
    var zm2 = uint64(0);
    var ps = xs ^ ys; // product sign

    // normalize to 62nd bit
    var is62zero = uint((~pm1 >> 62) & 1);
    pm1, pm2 = shl(pm1, pm2, is62zero);
    pe -= int32(is62zero); 

    // Swap addition operands so |p| >= |z|
    if (pe < ze || pe == ze && pm1 < zm1) {
        (ps, pe, pm1, pm2, zs, ze, zm1, zm2) = (zs, ze, zm1, zm2, ps, pe, pm1, pm2);
    }
    zm1, zm2 = shrcompress(zm1, zm2, uint(pe - ze)); 

    // Compute resulting significands, normalizing if necessary.
    ulong m = default;    ulong c = default;

    if (ps == zs) { 
        // Adding (pm1:pm2) + (zm1:zm2)
        pm2, c = bits.Add64(pm2, zm2, 0);
        pm1, _ = bits.Add64(pm1, zm1, c);
        pe -= int32(~pm1 >> 63);
        pm1, m = shrcompress(pm1, pm2, uint(64 + pm1 >> 63));

    }
    else
 { 
        // Subtracting (pm1:pm2) - (zm1:zm2)
        // TODO: should we special-case cancellation?
        pm2, c = bits.Sub64(pm2, zm2, 0);
        pm1, _ = bits.Sub64(pm1, zm1, c);
        var nz = lz(pm1, pm2);
        pe -= nz;
        m, pm2 = shl(pm1, pm2, uint(nz - 1));
        m |= nonzero(pm2);

    }
    if (pe > 1022 + bias || pe == 1022 + bias && (m + 1 << 9) >> 63 == 1) { 
        // rounded value overflows exponent range
        return Float64frombits(uint64(ps) << 63 | uvinf);

    }
    if (pe < 0) {
        var n = uint(-pe);
        m = m >> (int)(n) | nonzero(m & (1 << (int)(n) - 1));
        pe = 0;
    }
    m = ((m + 1 << 9) >> 10) & ~zero((m & (1 << 10 - 1)) ^ 1 << 9);
    pe &= -int32(nonzero(m));
    return Float64frombits(uint64(ps) << 63 + uint64(pe) << 52 + m);

}

} // end math_package
