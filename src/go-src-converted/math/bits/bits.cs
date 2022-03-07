// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:generate go run make_tables.go

// Package bits implements bit counting and manipulation
// functions for the predeclared unsigned integer types.
// package bits -- go2cs converted at 2022 March 06 22:14:59 UTC
// import "math/bits" ==> using bits = go.math.bits_package
// Original source: C:\Program Files\Go\src\math\bits\bits.go


namespace go.math;

public static partial class bits_package {

private static readonly nint uintSize = 32 << (int)((~uint(0) >> 63)); // 32 or 64

// UintSize is the size of a uint in bits.
 // 32 or 64

// UintSize is the size of a uint in bits.
public static readonly var UintSize = uintSize;

// --- LeadingZeros ---

// LeadingZeros returns the number of leading zero bits in x; the result is UintSize for x == 0.


// --- LeadingZeros ---

// LeadingZeros returns the number of leading zero bits in x; the result is UintSize for x == 0.
public static nint LeadingZeros(nuint x) {
    return UintSize - Len(x);
}

// LeadingZeros8 returns the number of leading zero bits in x; the result is 8 for x == 0.
public static nint LeadingZeros8(byte x) {
    return 8 - Len8(x);
}

// LeadingZeros16 returns the number of leading zero bits in x; the result is 16 for x == 0.
public static nint LeadingZeros16(ushort x) {
    return 16 - Len16(x);
}

// LeadingZeros32 returns the number of leading zero bits in x; the result is 32 for x == 0.
public static nint LeadingZeros32(uint x) {
    return 32 - Len32(x);
}

// LeadingZeros64 returns the number of leading zero bits in x; the result is 64 for x == 0.
public static nint LeadingZeros64(ulong x) {
    return 64 - Len64(x);
}

// --- TrailingZeros ---

// See http://supertech.csail.mit.edu/papers/debruijn.pdf
private static readonly nuint deBruijn32 = 0x077CB531;



private static array<byte> deBruijn32tab = new array<byte>(new byte[] { 0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8, 31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9 });

private static readonly nuint deBruijn64 = 0x03f79d71b4ca8b09;



private static array<byte> deBruijn64tab = new array<byte>(new byte[] { 0, 1, 56, 2, 57, 49, 28, 3, 61, 58, 42, 50, 38, 29, 17, 4, 62, 47, 59, 36, 45, 43, 51, 22, 53, 39, 33, 30, 24, 18, 12, 5, 63, 55, 48, 27, 60, 41, 37, 16, 46, 35, 44, 21, 52, 32, 23, 11, 54, 26, 40, 15, 34, 20, 31, 10, 25, 14, 19, 9, 13, 8, 7, 6 });

// TrailingZeros returns the number of trailing zero bits in x; the result is UintSize for x == 0.
public static nint TrailingZeros(nuint x) {
    if (UintSize == 32) {
        return TrailingZeros32(uint32(x));
    }
    return TrailingZeros64(uint64(x));

}

// TrailingZeros8 returns the number of trailing zero bits in x; the result is 8 for x == 0.
public static nint TrailingZeros8(byte x) {
    return int(ntz8tab[x]);
}

// TrailingZeros16 returns the number of trailing zero bits in x; the result is 16 for x == 0.
public static nint TrailingZeros16(ushort x) {
    if (x == 0) {
        return 16;
    }
    return int(deBruijn32tab[uint32(x & -x) * deBruijn32 >> (int)((32 - 5))]);

}

// TrailingZeros32 returns the number of trailing zero bits in x; the result is 32 for x == 0.
public static nint TrailingZeros32(uint x) {
    if (x == 0) {
        return 32;
    }
    return int(deBruijn32tab[(x & -x) * deBruijn32 >> (int)((32 - 5))]);

}

// TrailingZeros64 returns the number of trailing zero bits in x; the result is 64 for x == 0.
public static nint TrailingZeros64(ulong x) {
    if (x == 0) {
        return 64;
    }
    return int(deBruijn64tab[(x & -x) * deBruijn64 >> (int)((64 - 6))]);

}

// --- OnesCount ---

private static readonly nuint m0 = 0x5555555555555555; // 01010101 ...
 // 01010101 ...
private static readonly nuint m1 = 0x3333333333333333; // 00110011 ...
 // 00110011 ...
private static readonly nuint m2 = 0x0f0f0f0f0f0f0f0f; // 00001111 ...
 // 00001111 ...
private static readonly nuint m3 = 0x00ff00ff00ff00ff; // etc.
 // etc.
private static readonly nuint m4 = 0x0000ffff0000ffff;

// OnesCount returns the number of one bits ("population count") in x.


// OnesCount returns the number of one bits ("population count") in x.
public static nint OnesCount(nuint x) {
    if (UintSize == 32) {
        return OnesCount32(uint32(x));
    }
    return OnesCount64(uint64(x));

}

// OnesCount8 returns the number of one bits ("population count") in x.
public static nint OnesCount8(byte x) {
    return int(pop8tab[x]);
}

// OnesCount16 returns the number of one bits ("population count") in x.
public static nint OnesCount16(ushort x) {
    return int(pop8tab[x >> 8] + pop8tab[x & 0xff]);
}

// OnesCount32 returns the number of one bits ("population count") in x.
public static nint OnesCount32(uint x) {
    return int(pop8tab[x >> 24] + pop8tab[x >> 16 & 0xff] + pop8tab[x >> 8 & 0xff] + pop8tab[x & 0xff]);
}

// OnesCount64 returns the number of one bits ("population count") in x.
public static nint OnesCount64(ulong x) { 
    // Implementation: Parallel summing of adjacent bits.
    // See "Hacker's Delight", Chap. 5: Counting Bits.
    // The following pattern shows the general approach:
    //
    //   x = x>>1&(m0&m) + x&(m0&m)
    //   x = x>>2&(m1&m) + x&(m1&m)
    //   x = x>>4&(m2&m) + x&(m2&m)
    //   x = x>>8&(m3&m) + x&(m3&m)
    //   x = x>>16&(m4&m) + x&(m4&m)
    //   x = x>>32&(m5&m) + x&(m5&m)
    //   return int(x)
    //
    // Masking (& operations) can be left away when there's no
    // danger that a field's sum will carry over into the next
    // field: Since the result cannot be > 64, 8 bits is enough
    // and we can ignore the masks for the shifts by 8 and up.
    // Per "Hacker's Delight", the first line can be simplified
    // more, but it saves at best one instruction, so we leave
    // it alone for clarity.
    const nint m = 1 << 64 - 1;

    x = x >> 1 & (m0 & m) + x & (m0 & m);
    x = x >> 2 & (m1 & m) + x & (m1 & m);
    x = (x >> 4 + x) & (m2 & m);
    x += x >> 8;
    x += x >> 16;
    x += x >> 32;
    return int(x) & (1 << 7 - 1);

}

// --- RotateLeft ---

// RotateLeft returns the value of x rotated left by (k mod UintSize) bits.
// To rotate x right by k bits, call RotateLeft(x, -k).
//
// This function's execution time does not depend on the inputs.
public static nuint RotateLeft(nuint x, nint k) {
    if (UintSize == 32) {
        return uint(RotateLeft32(uint32(x), k));
    }
    return uint(RotateLeft64(uint64(x), k));

}

// RotateLeft8 returns the value of x rotated left by (k mod 8) bits.
// To rotate x right by k bits, call RotateLeft8(x, -k).
//
// This function's execution time does not depend on the inputs.
public static byte RotateLeft8(byte x, nint k) {
    const nint n = 8;

    var s = uint(k) & (n - 1);
    return x << (int)(s) | x >> (int)((n - s));
}

// RotateLeft16 returns the value of x rotated left by (k mod 16) bits.
// To rotate x right by k bits, call RotateLeft16(x, -k).
//
// This function's execution time does not depend on the inputs.
public static ushort RotateLeft16(ushort x, nint k) {
    const nint n = 16;

    var s = uint(k) & (n - 1);
    return x << (int)(s) | x >> (int)((n - s));
}

// RotateLeft32 returns the value of x rotated left by (k mod 32) bits.
// To rotate x right by k bits, call RotateLeft32(x, -k).
//
// This function's execution time does not depend on the inputs.
public static uint RotateLeft32(uint x, nint k) {
    const nint n = 32;

    var s = uint(k) & (n - 1);
    return x << (int)(s) | x >> (int)((n - s));
}

// RotateLeft64 returns the value of x rotated left by (k mod 64) bits.
// To rotate x right by k bits, call RotateLeft64(x, -k).
//
// This function's execution time does not depend on the inputs.
public static ulong RotateLeft64(ulong x, nint k) {
    const nint n = 64;

    var s = uint(k) & (n - 1);
    return x << (int)(s) | x >> (int)((n - s));
}

// --- Reverse ---

// Reverse returns the value of x with its bits in reversed order.
public static nuint Reverse(nuint x) {
    if (UintSize == 32) {
        return uint(Reverse32(uint32(x)));
    }
    return uint(Reverse64(uint64(x)));

}

// Reverse8 returns the value of x with its bits in reversed order.
public static byte Reverse8(byte x) {
    return rev8tab[x];
}

// Reverse16 returns the value of x with its bits in reversed order.
public static ushort Reverse16(ushort x) {
    return uint16(rev8tab[x >> 8]) | uint16(rev8tab[x & 0xff]) << 8;
}

// Reverse32 returns the value of x with its bits in reversed order.
public static uint Reverse32(uint x) {
    const nint m = 1 << 32 - 1;

    x = x >> 1 & (m0 & m) | x & (m0 & m) << 1;
    x = x >> 2 & (m1 & m) | x & (m1 & m) << 2;
    x = x >> 4 & (m2 & m) | x & (m2 & m) << 4;
    return ReverseBytes32(x);
}

// Reverse64 returns the value of x with its bits in reversed order.
public static ulong Reverse64(ulong x) {
    const nint m = 1 << 64 - 1;

    x = x >> 1 & (m0 & m) | x & (m0 & m) << 1;
    x = x >> 2 & (m1 & m) | x & (m1 & m) << 2;
    x = x >> 4 & (m2 & m) | x & (m2 & m) << 4;
    return ReverseBytes64(x);
}

// --- ReverseBytes ---

// ReverseBytes returns the value of x with its bytes in reversed order.
//
// This function's execution time does not depend on the inputs.
public static nuint ReverseBytes(nuint x) {
    if (UintSize == 32) {
        return uint(ReverseBytes32(uint32(x)));
    }
    return uint(ReverseBytes64(uint64(x)));

}

// ReverseBytes16 returns the value of x with its bytes in reversed order.
//
// This function's execution time does not depend on the inputs.
public static ushort ReverseBytes16(ushort x) {
    return x >> 8 | x << 8;
}

// ReverseBytes32 returns the value of x with its bytes in reversed order.
//
// This function's execution time does not depend on the inputs.
public static uint ReverseBytes32(uint x) {
    const nint m = 1 << 32 - 1;

    x = x >> 8 & (m3 & m) | x & (m3 & m) << 8;
    return x >> 16 | x << 16;
}

// ReverseBytes64 returns the value of x with its bytes in reversed order.
//
// This function's execution time does not depend on the inputs.
public static ulong ReverseBytes64(ulong x) {
    const nint m = 1 << 64 - 1;

    x = x >> 8 & (m3 & m) | x & (m3 & m) << 8;
    x = x >> 16 & (m4 & m) | x & (m4 & m) << 16;
    return x >> 32 | x << 32;
}

// --- Len ---

// Len returns the minimum number of bits required to represent x; the result is 0 for x == 0.
public static nint Len(nuint x) {
    if (UintSize == 32) {
        return Len32(uint32(x));
    }
    return Len64(uint64(x));

}

// Len8 returns the minimum number of bits required to represent x; the result is 0 for x == 0.
public static nint Len8(byte x) {
    return int(len8tab[x]);
}

// Len16 returns the minimum number of bits required to represent x; the result is 0 for x == 0.
public static nint Len16(ushort x) {
    nint n = default;

    if (x >= 1 << 8) {
        x>>=8;
        n = 8;
    }
    return n + int(len8tab[x]);

}

// Len32 returns the minimum number of bits required to represent x; the result is 0 for x == 0.
public static nint Len32(uint x) {
    nint n = default;

    if (x >= 1 << 16) {
        x>>=16;
        n = 16;
    }
    if (x >= 1 << 8) {
        x>>=8;
        n += 8;
    }
    return n + int(len8tab[x]);

}

// Len64 returns the minimum number of bits required to represent x; the result is 0 for x == 0.
public static nint Len64(ulong x) {
    nint n = default;

    if (x >= 1 << 32) {
        x>>=32;
        n = 32;
    }
    if (x >= 1 << 16) {
        x>>=16;
        n += 16;
    }
    if (x >= 1 << 8) {
        x>>=8;
        n += 8;
    }
    return n + int(len8tab[x]);

}

// --- Add with carry ---

// Add returns the sum with carry of x, y and carry: sum = x + y + carry.
// The carry input must be 0 or 1; otherwise the behavior is undefined.
// The carryOut output is guaranteed to be 0 or 1.
//
// This function's execution time does not depend on the inputs.
public static (nuint, nuint) Add(nuint x, nuint y, nuint carry) {
    nuint sum = default;
    nuint carryOut = default;

    if (UintSize == 32) {
        var (s32, c32) = Add32(uint32(x), uint32(y), uint32(carry));
        return (uint(s32), uint(c32));
    }
    var (s64, c64) = Add64(uint64(x), uint64(y), uint64(carry));
    return (uint(s64), uint(c64));

}

// Add32 returns the sum with carry of x, y and carry: sum = x + y + carry.
// The carry input must be 0 or 1; otherwise the behavior is undefined.
// The carryOut output is guaranteed to be 0 or 1.
//
// This function's execution time does not depend on the inputs.
public static (uint, uint) Add32(uint x, uint y, uint carry) {
    uint sum = default;
    uint carryOut = default;

    var sum64 = uint64(x) + uint64(y) + uint64(carry);
    sum = uint32(sum64);
    carryOut = uint32(sum64 >> 32);
    return ;
}

// Add64 returns the sum with carry of x, y and carry: sum = x + y + carry.
// The carry input must be 0 or 1; otherwise the behavior is undefined.
// The carryOut output is guaranteed to be 0 or 1.
//
// This function's execution time does not depend on the inputs.
public static (ulong, ulong) Add64(ulong x, ulong y, ulong carry) {
    ulong sum = default;
    ulong carryOut = default;

    sum = x + y + carry; 
    // The sum will overflow if both top bits are set (x & y) or if one of them
    // is (x | y), and a carry from the lower place happened. If such a carry
    // happens, the top bit will be 1 + 0 + 1 = 0 (&^ sum).
    carryOut = ((x & y) | ((x | y) & ~sum)) >> 63;
    return ;

}

// --- Subtract with borrow ---

// Sub returns the difference of x, y and borrow: diff = x - y - borrow.
// The borrow input must be 0 or 1; otherwise the behavior is undefined.
// The borrowOut output is guaranteed to be 0 or 1.
//
// This function's execution time does not depend on the inputs.
public static (nuint, nuint) Sub(nuint x, nuint y, nuint borrow) {
    nuint diff = default;
    nuint borrowOut = default;

    if (UintSize == 32) {
        var (d32, b32) = Sub32(uint32(x), uint32(y), uint32(borrow));
        return (uint(d32), uint(b32));
    }
    var (d64, b64) = Sub64(uint64(x), uint64(y), uint64(borrow));
    return (uint(d64), uint(b64));

}

// Sub32 returns the difference of x, y and borrow, diff = x - y - borrow.
// The borrow input must be 0 or 1; otherwise the behavior is undefined.
// The borrowOut output is guaranteed to be 0 or 1.
//
// This function's execution time does not depend on the inputs.
public static (uint, uint) Sub32(uint x, uint y, uint borrow) {
    uint diff = default;
    uint borrowOut = default;

    diff = x - y - borrow; 
    // The difference will underflow if the top bit of x is not set and the top
    // bit of y is set (^x & y) or if they are the same (^(x ^ y)) and a borrow
    // from the lower place happens. If that borrow happens, the result will be
    // 1 - 1 - 1 = 0 - 0 - 1 = 1 (& diff).
    borrowOut = ((~x & y) | (~(x ^ y) & diff)) >> 31;
    return ;

}

// Sub64 returns the difference of x, y and borrow: diff = x - y - borrow.
// The borrow input must be 0 or 1; otherwise the behavior is undefined.
// The borrowOut output is guaranteed to be 0 or 1.
//
// This function's execution time does not depend on the inputs.
public static (ulong, ulong) Sub64(ulong x, ulong y, ulong borrow) {
    ulong diff = default;
    ulong borrowOut = default;

    diff = x - y - borrow; 
    // See Sub32 for the bit logic.
    borrowOut = ((~x & y) | (~(x ^ y) & diff)) >> 63;
    return ;

}

// --- Full-width multiply ---

// Mul returns the full-width product of x and y: (hi, lo) = x * y
// with the product bits' upper half returned in hi and the lower
// half returned in lo.
//
// This function's execution time does not depend on the inputs.
public static (nuint, nuint) Mul(nuint x, nuint y) {
    nuint hi = default;
    nuint lo = default;

    if (UintSize == 32) {
        var (h, l) = Mul32(uint32(x), uint32(y));
        return (uint(h), uint(l));
    }
    (h, l) = Mul64(uint64(x), uint64(y));
    return (uint(h), uint(l));

}

// Mul32 returns the 64-bit product of x and y: (hi, lo) = x * y
// with the product bits' upper half returned in hi and the lower
// half returned in lo.
//
// This function's execution time does not depend on the inputs.
public static (uint, uint) Mul32(uint x, uint y) {
    uint hi = default;
    uint lo = default;

    var tmp = uint64(x) * uint64(y);
    (hi, lo) = (uint32(tmp >> 32), uint32(tmp));    return ;
}

// Mul64 returns the 128-bit product of x and y: (hi, lo) = x * y
// with the product bits' upper half returned in hi and the lower
// half returned in lo.
//
// This function's execution time does not depend on the inputs.
public static (ulong, ulong) Mul64(ulong x, ulong y) {
    ulong hi = default;
    ulong lo = default;

    const nint mask32 = 1 << 32 - 1;

    var x0 = x & mask32;
    var x1 = x >> 32;
    var y0 = y & mask32;
    var y1 = y >> 32;
    var w0 = x0 * y0;
    var t = x1 * y0 + w0 >> 32;
    var w1 = t & mask32;
    var w2 = t >> 32;
    w1 += x0 * y1;
    hi = x1 * y1 + w2 + w1 >> 32;
    lo = x * y;
    return ;
}

// --- Full-width divide ---

// Div returns the quotient and remainder of (hi, lo) divided by y:
// quo = (hi, lo)/y, rem = (hi, lo)%y with the dividend bits' upper
// half in parameter hi and the lower half in parameter lo.
// Div panics for y == 0 (division by zero) or y <= hi (quotient overflow).
public static (nuint, nuint) Div(nuint hi, nuint lo, nuint y) {
    nuint quo = default;
    nuint rem = default;

    if (UintSize == 32) {
        var (q, r) = Div32(uint32(hi), uint32(lo), uint32(y));
        return (uint(q), uint(r));
    }
    (q, r) = Div64(uint64(hi), uint64(lo), uint64(y));
    return (uint(q), uint(r));

}

// Div32 returns the quotient and remainder of (hi, lo) divided by y:
// quo = (hi, lo)/y, rem = (hi, lo)%y with the dividend bits' upper
// half in parameter hi and the lower half in parameter lo.
// Div32 panics for y == 0 (division by zero) or y <= hi (quotient overflow).
public static (uint, uint) Div32(uint hi, uint lo, uint y) => func((_, panic, _) => {
    uint quo = default;
    uint rem = default;

    if (y != 0 && y <= hi) {
        panic(overflowError);
    }
    var z = uint64(hi) << 32 | uint64(lo);
    (quo, rem) = (uint32(z / uint64(y)), uint32(z % uint64(y)));    return ;

});

// Div64 returns the quotient and remainder of (hi, lo) divided by y:
// quo = (hi, lo)/y, rem = (hi, lo)%y with the dividend bits' upper
// half in parameter hi and the lower half in parameter lo.
// Div64 panics for y == 0 (division by zero) or y <= hi (quotient overflow).
public static (ulong, ulong) Div64(ulong hi, ulong lo, ulong y) => func((_, panic, _) => {
    ulong quo = default;
    ulong rem = default;

    const nint two32 = 1 << 32;
    const var mask32 = two32 - 1;
    if (y == 0) {
        panic(divideError);
    }
    if (y <= hi) {
        panic(overflowError);
    }
    var s = uint(LeadingZeros64(y));
    y<<=s;

    var yn1 = y >> 32;
    var yn0 = y & mask32;
    var un32 = hi << (int)(s) | lo >> (int)((64 - s));
    var un10 = lo << (int)(s);
    var un1 = un10 >> 32;
    var un0 = un10 & mask32;
    var q1 = un32 / yn1;
    var rhat = un32 - q1 * yn1;

    while (q1 >= two32 || q1 * yn0 > two32 * rhat + un1) {
        q1--;
        rhat += yn1;
        if (rhat >= two32) {
            break;
        }
    }

    var un21 = un32 * two32 + un1 - q1 * y;
    var q0 = un21 / yn1;
    rhat = un21 - q0 * yn1;

    while (q0 >= two32 || q0 * yn0 > two32 * rhat + un0) {
        q0--;
        rhat += yn1;
        if (rhat >= two32) {
            break;
        }
    }

    return (q1 * two32 + q0, (un21 * two32 + un0 - q0 * y) >> (int)(s));

});

// Rem returns the remainder of (hi, lo) divided by y. Rem panics for
// y == 0 (division by zero) but, unlike Div, it doesn't panic on a
// quotient overflow.
public static nuint Rem(nuint hi, nuint lo, nuint y) {
    if (UintSize == 32) {
        return uint(Rem32(uint32(hi), uint32(lo), uint32(y)));
    }
    return uint(Rem64(uint64(hi), uint64(lo), uint64(y)));

}

// Rem32 returns the remainder of (hi, lo) divided by y. Rem32 panics
// for y == 0 (division by zero) but, unlike Div32, it doesn't panic
// on a quotient overflow.
public static uint Rem32(uint hi, uint lo, uint y) {
    return uint32((uint64(hi) << 32 | uint64(lo)) % uint64(y));
}

// Rem64 returns the remainder of (hi, lo) divided by y. Rem64 panics
// for y == 0 (division by zero) but, unlike Div64, it doesn't panic
// on a quotient overflow.
public static ulong Rem64(ulong hi, ulong lo, ulong y) { 
    // We scale down hi so that hi < y, then use Div64 to compute the
    // rem with the guarantee that it won't panic on quotient overflow.
    // Given that
    //   hi ≡ hi%y    (mod y)
    // we have
    //   hi<<64 + lo ≡ (hi%y)<<64 + lo    (mod y)
    var (_, rem) = Div64(hi % y, lo, y);
    return rem;

}

} // end bits_package
