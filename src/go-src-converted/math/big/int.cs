// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements signed multi-precision integers.

// package big -- go2cs converted at 2022 March 13 05:32:02 UTC
// import "math/big" ==> using big = go.math.big_package
// Original source: C:\Program Files\Go\src\math\big\int.go
namespace go.math;

using fmt = fmt_package;
using io = io_package;
using rand = math.rand_package;
using strings = strings_package;


// An Int represents a signed multi-precision integer.
// The zero value for an Int represents the value 0.
//
// Operations always take pointer arguments (*Int) rather
// than Int values, and each unique Int value requires
// its own unique *Int pointer. To "copy" an Int value,
// an existing (or newly allocated) Int must be set to
// a new value using the Int.Set method; shallow copies
// of Ints are not supported and may lead to errors.

public static partial class big_package {

public partial struct Int {
    public bool neg; // sign
    public nat abs; // absolute value of the integer
}

private static ptr<Int> intOne = addr(new Int(false,natOne));

// Sign returns:
//
//    -1 if x <  0
//     0 if x == 0
//    +1 if x >  0
//
private static nint Sign(this ptr<Int> _addr_x) {
    ref Int x = ref _addr_x.val;

    if (len(x.abs) == 0) {
        return 0;
    }
    if (x.neg) {
        return -1;
    }
    return 1;
}

// SetInt64 sets z to x and returns z.
private static ptr<Int> SetInt64(this ptr<Int> _addr_z, long x) {
    ref Int z = ref _addr_z.val;

    var neg = false;
    if (x < 0) {
        neg = true;
        x = -x;
    }
    z.abs = z.abs.setUint64(uint64(x));
    z.neg = neg;
    return _addr_z!;
}

// SetUint64 sets z to x and returns z.
private static ptr<Int> SetUint64(this ptr<Int> _addr_z, ulong x) {
    ref Int z = ref _addr_z.val;

    z.abs = z.abs.setUint64(x);
    z.neg = false;
    return _addr_z!;
}

// NewInt allocates and returns a new Int set to x.
public static ptr<Int> NewInt(long x) {
    return @new<Int>().SetInt64(x);
}

// Set sets z to x and returns z.
private static ptr<Int> Set(this ptr<Int> _addr_z, ptr<Int> _addr_x) {
    ref Int z = ref _addr_z.val;
    ref Int x = ref _addr_x.val;

    if (z != x) {
        z.abs = z.abs.set(x.abs);
        z.neg = x.neg;
    }
    return _addr_z!;
}

// Bits provides raw (unchecked but fast) access to x by returning its
// absolute value as a little-endian Word slice. The result and x share
// the same underlying array.
// Bits is intended to support implementation of missing low-level Int
// functionality outside this package; it should be avoided otherwise.
private static slice<Word> Bits(this ptr<Int> _addr_x) {
    ref Int x = ref _addr_x.val;

    return x.abs;
}

// SetBits provides raw (unchecked but fast) access to z by setting its
// value to abs, interpreted as a little-endian Word slice, and returning
// z. The result and abs share the same underlying array.
// SetBits is intended to support implementation of missing low-level Int
// functionality outside this package; it should be avoided otherwise.
private static ptr<Int> SetBits(this ptr<Int> _addr_z, slice<Word> abs) {
    ref Int z = ref _addr_z.val;

    z.abs = nat(abs).norm();
    z.neg = false;
    return _addr_z!;
}

// Abs sets z to |x| (the absolute value of x) and returns z.
private static ptr<Int> Abs(this ptr<Int> _addr_z, ptr<Int> _addr_x) {
    ref Int z = ref _addr_z.val;
    ref Int x = ref _addr_x.val;

    z.Set(x);
    z.neg = false;
    return _addr_z!;
}

// Neg sets z to -x and returns z.
private static ptr<Int> Neg(this ptr<Int> _addr_z, ptr<Int> _addr_x) {
    ref Int z = ref _addr_z.val;
    ref Int x = ref _addr_x.val;

    z.Set(x);
    z.neg = len(z.abs) > 0 && !z.neg; // 0 has no sign
    return _addr_z!;
}

// Add sets z to the sum x+y and returns z.
private static ptr<Int> Add(this ptr<Int> _addr_z, ptr<Int> _addr_x, ptr<Int> _addr_y) {
    ref Int z = ref _addr_z.val;
    ref Int x = ref _addr_x.val;
    ref Int y = ref _addr_y.val;

    var neg = x.neg;
    if (x.neg == y.neg) { 
        // x + y == x + y
        // (-x) + (-y) == -(x + y)
        z.abs = z.abs.add(x.abs, y.abs);
    }
    else
 { 
        // x + (-y) == x - y == -(y - x)
        // (-x) + y == y - x == -(x - y)
        if (x.abs.cmp(y.abs) >= 0) {
            z.abs = z.abs.sub(x.abs, y.abs);
        }
        else
 {
            neg = !neg;
            z.abs = z.abs.sub(y.abs, x.abs);
        }
    }
    z.neg = len(z.abs) > 0 && neg; // 0 has no sign
    return _addr_z!;
}

// Sub sets z to the difference x-y and returns z.
private static ptr<Int> Sub(this ptr<Int> _addr_z, ptr<Int> _addr_x, ptr<Int> _addr_y) {
    ref Int z = ref _addr_z.val;
    ref Int x = ref _addr_x.val;
    ref Int y = ref _addr_y.val;

    var neg = x.neg;
    if (x.neg != y.neg) { 
        // x - (-y) == x + y
        // (-x) - y == -(x + y)
        z.abs = z.abs.add(x.abs, y.abs);
    }
    else
 { 
        // x - y == x - y == -(y - x)
        // (-x) - (-y) == y - x == -(x - y)
        if (x.abs.cmp(y.abs) >= 0) {
            z.abs = z.abs.sub(x.abs, y.abs);
        }
        else
 {
            neg = !neg;
            z.abs = z.abs.sub(y.abs, x.abs);
        }
    }
    z.neg = len(z.abs) > 0 && neg; // 0 has no sign
    return _addr_z!;
}

// Mul sets z to the product x*y and returns z.
private static ptr<Int> Mul(this ptr<Int> _addr_z, ptr<Int> _addr_x, ptr<Int> _addr_y) {
    ref Int z = ref _addr_z.val;
    ref Int x = ref _addr_x.val;
    ref Int y = ref _addr_y.val;
 
    // x * y == x * y
    // x * (-y) == -(x * y)
    // (-x) * y == -(x * y)
    // (-x) * (-y) == x * y
    if (x == y) {
        z.abs = z.abs.sqr(x.abs);
        z.neg = false;
        return _addr_z!;
    }
    z.abs = z.abs.mul(x.abs, y.abs);
    z.neg = len(z.abs) > 0 && x.neg != y.neg; // 0 has no sign
    return _addr_z!;
}

// MulRange sets z to the product of all integers
// in the range [a, b] inclusively and returns z.
// If a > b (empty range), the result is 1.
private static ptr<Int> MulRange(this ptr<Int> _addr_z, long a, long b) {
    ref Int z = ref _addr_z.val;


    if (a > b) 
        return _addr_z.SetInt64(1)!; // empty range
    else if (a <= 0 && b >= 0) 
        return _addr_z.SetInt64(0)!; // range includes 0
    // a <= b && (b < 0 || a > 0)

    var neg = false;
    if (a < 0) {
        neg = (b - a) & 1 == 0;
        (a, b) = (-b, -a);
    }
    z.abs = z.abs.mulRange(uint64(a), uint64(b));
    z.neg = neg;
    return _addr_z!;
}

// Binomial sets z to the binomial coefficient of (n, k) and returns z.
private static ptr<Int> Binomial(this ptr<Int> _addr_z, long n, long k) {
    ref Int z = ref _addr_z.val;
 
    // reduce the number of multiplications by reducing k
    if (n / 2 < k && k <= n) {
        k = n - k; // Binomial(n, k) == Binomial(n, n-k)
    }
    ref Int a = ref heap(out ptr<Int> _addr_a);    ref Int b = ref heap(out ptr<Int> _addr_b);

    a.MulRange(n - k + 1, n);
    b.MulRange(1, k);
    return _addr_z.Quo(_addr_a, _addr_b)!;
}

// Quo sets z to the quotient x/y for y != 0 and returns z.
// If y == 0, a division-by-zero run-time panic occurs.
// Quo implements truncated division (like Go); see QuoRem for more details.
private static ptr<Int> Quo(this ptr<Int> _addr_z, ptr<Int> _addr_x, ptr<Int> _addr_y) {
    ref Int z = ref _addr_z.val;
    ref Int x = ref _addr_x.val;
    ref Int y = ref _addr_y.val;

    z.abs, _ = z.abs.div(null, x.abs, y.abs);
    z.neg = len(z.abs) > 0 && x.neg != y.neg; // 0 has no sign
    return _addr_z!;
}

// Rem sets z to the remainder x%y for y != 0 and returns z.
// If y == 0, a division-by-zero run-time panic occurs.
// Rem implements truncated modulus (like Go); see QuoRem for more details.
private static ptr<Int> Rem(this ptr<Int> _addr_z, ptr<Int> _addr_x, ptr<Int> _addr_y) {
    ref Int z = ref _addr_z.val;
    ref Int x = ref _addr_x.val;
    ref Int y = ref _addr_y.val;

    _, z.abs = nat(null).div(z.abs, x.abs, y.abs);
    z.neg = len(z.abs) > 0 && x.neg; // 0 has no sign
    return _addr_z!;
}

// QuoRem sets z to the quotient x/y and r to the remainder x%y
// and returns the pair (z, r) for y != 0.
// If y == 0, a division-by-zero run-time panic occurs.
//
// QuoRem implements T-division and modulus (like Go):
//
//    q = x/y      with the result truncated to zero
//    r = x - y*q
//
// (See Daan Leijen, ``Division and Modulus for Computer Scientists''.)
// See DivMod for Euclidean division and modulus (unlike Go).
//
private static (ptr<Int>, ptr<Int>) QuoRem(this ptr<Int> _addr_z, ptr<Int> _addr_x, ptr<Int> _addr_y, ptr<Int> _addr_r) {
    ptr<Int> _p0 = default!;
    ptr<Int> _p0 = default!;
    ref Int z = ref _addr_z.val;
    ref Int x = ref _addr_x.val;
    ref Int y = ref _addr_y.val;
    ref Int r = ref _addr_r.val;

    z.abs, r.abs = z.abs.div(r.abs, x.abs, y.abs);
    (z.neg, r.neg) = (len(z.abs) > 0 && x.neg != y.neg, len(r.abs) > 0 && x.neg);    return (_addr_z!, _addr_r!);
}

// Div sets z to the quotient x/y for y != 0 and returns z.
// If y == 0, a division-by-zero run-time panic occurs.
// Div implements Euclidean division (unlike Go); see DivMod for more details.
private static ptr<Int> Div(this ptr<Int> _addr_z, ptr<Int> _addr_x, ptr<Int> _addr_y) {
    ref Int z = ref _addr_z.val;
    ref Int x = ref _addr_x.val;
    ref Int y = ref _addr_y.val;

    var y_neg = y.neg; // z may be an alias for y
    ref Int r = ref heap(out ptr<Int> _addr_r);
    z.QuoRem(x, y, _addr_r);
    if (r.neg) {
        if (y_neg) {
            z.Add(z, intOne);
        }
        else
 {
            z.Sub(z, intOne);
        }
    }
    return _addr_z!;
}

// Mod sets z to the modulus x%y for y != 0 and returns z.
// If y == 0, a division-by-zero run-time panic occurs.
// Mod implements Euclidean modulus (unlike Go); see DivMod for more details.
private static ptr<Int> Mod(this ptr<Int> _addr_z, ptr<Int> _addr_x, ptr<Int> _addr_y) {
    ref Int z = ref _addr_z.val;
    ref Int x = ref _addr_x.val;
    ref Int y = ref _addr_y.val;

    var y0 = y; // save y
    if (z == y || alias(z.abs, y.abs)) {
        y0 = @new<Int>().Set(y);
    }
    Int q = default;
    q.QuoRem(x, y, z);
    if (z.neg) {
        if (y0.neg) {
            z.Sub(z, y0);
        }
        else
 {
            z.Add(z, y0);
        }
    }
    return _addr_z!;
}

// DivMod sets z to the quotient x div y and m to the modulus x mod y
// and returns the pair (z, m) for y != 0.
// If y == 0, a division-by-zero run-time panic occurs.
//
// DivMod implements Euclidean division and modulus (unlike Go):
//
//    q = x div y  such that
//    m = x - y*q  with 0 <= m < |y|
//
// (See Raymond T. Boute, ``The Euclidean definition of the functions
// div and mod''. ACM Transactions on Programming Languages and
// Systems (TOPLAS), 14(2):127-144, New York, NY, USA, 4/1992.
// ACM press.)
// See QuoRem for T-division and modulus (like Go).
//
private static (ptr<Int>, ptr<Int>) DivMod(this ptr<Int> _addr_z, ptr<Int> _addr_x, ptr<Int> _addr_y, ptr<Int> _addr_m) {
    ptr<Int> _p0 = default!;
    ptr<Int> _p0 = default!;
    ref Int z = ref _addr_z.val;
    ref Int x = ref _addr_x.val;
    ref Int y = ref _addr_y.val;
    ref Int m = ref _addr_m.val;

    var y0 = y; // save y
    if (z == y || alias(z.abs, y.abs)) {
        y0 = @new<Int>().Set(y);
    }
    z.QuoRem(x, y, m);
    if (m.neg) {
        if (y0.neg) {
            z.Add(z, intOne);
            m.Sub(m, y0);
        }
        else
 {
            z.Sub(z, intOne);
            m.Add(m, y0);
        }
    }
    return (_addr_z!, _addr_m!);
}

// Cmp compares x and y and returns:
//
//   -1 if x <  y
//    0 if x == y
//   +1 if x >  y
//
private static nint Cmp(this ptr<Int> _addr_x, ptr<Int> _addr_y) {
    nint r = default;
    ref Int x = ref _addr_x.val;
    ref Int y = ref _addr_y.val;
 
    // x cmp y == x cmp y
    // x cmp (-y) == x
    // (-x) cmp y == y
    // (-x) cmp (-y) == -(x cmp y)

    if (x == y)     else if (x.neg == y.neg) 
        r = x.abs.cmp(y.abs);
        if (x.neg) {
            r = -r;
        }
    else if (x.neg) 
        r = -1;
    else 
        r = 1;
        return ;
}

// CmpAbs compares the absolute values of x and y and returns:
//
//   -1 if |x| <  |y|
//    0 if |x| == |y|
//   +1 if |x| >  |y|
//
private static nint CmpAbs(this ptr<Int> _addr_x, ptr<Int> _addr_y) {
    ref Int x = ref _addr_x.val;
    ref Int y = ref _addr_y.val;

    return x.abs.cmp(y.abs);
}

// low32 returns the least significant 32 bits of x.
private static uint low32(nat x) {
    if (len(x) == 0) {
        return 0;
    }
    return uint32(x[0]);
}

// low64 returns the least significant 64 bits of x.
private static ulong low64(nat x) {
    if (len(x) == 0) {
        return 0;
    }
    var v = uint64(x[0]);
    if (_W == 32 && len(x) > 1) {
        return uint64(x[1]) << 32 | v;
    }
    return v;
}

// Int64 returns the int64 representation of x.
// If x cannot be represented in an int64, the result is undefined.
private static long Int64(this ptr<Int> _addr_x) {
    ref Int x = ref _addr_x.val;

    var v = int64(low64(x.abs));
    if (x.neg) {
        v = -v;
    }
    return v;
}

// Uint64 returns the uint64 representation of x.
// If x cannot be represented in a uint64, the result is undefined.
private static ulong Uint64(this ptr<Int> _addr_x) {
    ref Int x = ref _addr_x.val;

    return low64(x.abs);
}

// IsInt64 reports whether x can be represented as an int64.
private static bool IsInt64(this ptr<Int> _addr_x) {
    ref Int x = ref _addr_x.val;

    if (len(x.abs) <= 64 / _W) {
        var w = int64(low64(x.abs));
        return w >= 0 || x.neg && w == -w;
    }
    return false;
}

// IsUint64 reports whether x can be represented as a uint64.
private static bool IsUint64(this ptr<Int> _addr_x) {
    ref Int x = ref _addr_x.val;

    return !x.neg && len(x.abs) <= 64 / _W;
}

// SetString sets z to the value of s, interpreted in the given base,
// and returns z and a boolean indicating success. The entire string
// (not just a prefix) must be valid for success. If SetString fails,
// the value of z is undefined but the returned value is nil.
//
// The base argument must be 0 or a value between 2 and MaxBase.
// For base 0, the number prefix determines the actual base: A prefix of
// ``0b'' or ``0B'' selects base 2, ``0'', ``0o'' or ``0O'' selects base 8,
// and ``0x'' or ``0X'' selects base 16. Otherwise, the selected base is 10
// and no prefix is accepted.
//
// For bases <= 36, lower and upper case letters are considered the same:
// The letters 'a' to 'z' and 'A' to 'Z' represent digit values 10 to 35.
// For bases > 36, the upper case letters 'A' to 'Z' represent the digit
// values 36 to 61.
//
// For base 0, an underscore character ``_'' may appear between a base
// prefix and an adjacent digit, and between successive digits; such
// underscores do not change the value of the number.
// Incorrect placement of underscores is reported as an error if there
// are no other errors. If base != 0, underscores are not recognized
// and act like any other character that is not a valid digit.
//
private static (ptr<Int>, bool) SetString(this ptr<Int> _addr_z, @string s, nint @base) {
    ptr<Int> _p0 = default!;
    bool _p0 = default;
    ref Int z = ref _addr_z.val;

    return _addr_z.setFromScanner(strings.NewReader(s), base)!;
}

// setFromScanner implements SetString given an io.ByteScanner.
// For documentation see comments of SetString.
private static (ptr<Int>, bool) setFromScanner(this ptr<Int> _addr_z, io.ByteScanner r, nint @base) {
    ptr<Int> _p0 = default!;
    bool _p0 = default;
    ref Int z = ref _addr_z.val;

    {
        var (_, _, err) = z.scan(r, base);

        if (err != null) {
            return (_addr_null!, false);
        }
    } 
    // entire content must have been consumed
    {
        var (_, err) = r.ReadByte();

        if (err != io.EOF) {
            return (_addr_null!, false);
        }
    }
    return (_addr_z!, true); // err == io.EOF => scan consumed all content of r
}

// SetBytes interprets buf as the bytes of a big-endian unsigned
// integer, sets z to that value, and returns z.
private static ptr<Int> SetBytes(this ptr<Int> _addr_z, slice<byte> buf) {
    ref Int z = ref _addr_z.val;

    z.abs = z.abs.setBytes(buf);
    z.neg = false;
    return _addr_z!;
}

// Bytes returns the absolute value of x as a big-endian byte slice.
//
// To use a fixed length slice, or a preallocated one, use FillBytes.
private static slice<byte> Bytes(this ptr<Int> _addr_x) {
    ref Int x = ref _addr_x.val;

    var buf = make_slice<byte>(len(x.abs) * _S);
    return buf[(int)x.abs.bytes(buf)..];
}

// FillBytes sets buf to the absolute value of x, storing it as a zero-extended
// big-endian byte slice, and returns buf.
//
// If the absolute value of x doesn't fit in buf, FillBytes will panic.
private static slice<byte> FillBytes(this ptr<Int> _addr_x, slice<byte> buf) {
    ref Int x = ref _addr_x.val;
 
    // Clear whole buffer. (This gets optimized into a memclr.)
    foreach (var (i) in buf) {
        buf[i] = 0;
    }    x.abs.bytes(buf);
    return buf;
}

// BitLen returns the length of the absolute value of x in bits.
// The bit length of 0 is 0.
private static nint BitLen(this ptr<Int> _addr_x) {
    ref Int x = ref _addr_x.val;

    return x.abs.bitLen();
}

// TrailingZeroBits returns the number of consecutive least significant zero
// bits of |x|.
private static nuint TrailingZeroBits(this ptr<Int> _addr_x) {
    ref Int x = ref _addr_x.val;

    return x.abs.trailingZeroBits();
}

// Exp sets z = x**y mod |m| (i.e. the sign of m is ignored), and returns z.
// If m == nil or m == 0, z = x**y unless y <= 0 then z = 1. If m != 0, y < 0,
// and x and m are not relatively prime, z is unchanged and nil is returned.
//
// Modular exponentiation of inputs of a particular size is not a
// cryptographically constant-time operation.
private static ptr<Int> Exp(this ptr<Int> _addr_z, ptr<Int> _addr_x, ptr<Int> _addr_y, ptr<Int> _addr_m) {
    ref Int z = ref _addr_z.val;
    ref Int x = ref _addr_x.val;
    ref Int y = ref _addr_y.val;
    ref Int m = ref _addr_m.val;
 
    // See Knuth, volume 2, section 4.6.3.
    var xWords = x.abs;
    if (y.neg) {
        if (m == null || len(m.abs) == 0) {
            return _addr_z.SetInt64(1)!;
        }
        ptr<Int> inverse = @new<Int>().ModInverse(x, m);
        if (inverse == null) {
            return _addr_null!;
        }
        xWords = inverse.abs;
    }
    var yWords = y.abs;

    nat mWords = default;
    if (m != null) {
        mWords = m.abs; // m.abs may be nil for m == 0
    }
    z.abs = z.abs.expNN(xWords, yWords, mWords);
    z.neg = len(z.abs) > 0 && x.neg && len(yWords) > 0 && yWords[0] & 1 == 1; // 0 has no sign
    if (z.neg && len(mWords) > 0) { 
        // make modulus result positive
        z.abs = z.abs.sub(mWords, z.abs); // z == x**y mod |m| && 0 <= z < |m|
        z.neg = false;
    }
    return _addr_z!;
}

// GCD sets z to the greatest common divisor of a and b and returns z.
// If x or y are not nil, GCD sets their value such that z = a*x + b*y.
//
// a and b may be positive, zero or negative. (Before Go 1.14 both had
// to be > 0.) Regardless of the signs of a and b, z is always >= 0.
//
// If a == b == 0, GCD sets z = x = y = 0.
//
// If a == 0 and b != 0, GCD sets z = |b|, x = 0, y = sign(b) * 1.
//
// If a != 0 and b == 0, GCD sets z = |a|, x = sign(a) * 1, y = 0.
private static ptr<Int> GCD(this ptr<Int> _addr_z, ptr<Int> _addr_x, ptr<Int> _addr_y, ptr<Int> _addr_a, ptr<Int> _addr_b) {
    ref Int z = ref _addr_z.val;
    ref Int x = ref _addr_x.val;
    ref Int y = ref _addr_y.val;
    ref Int a = ref _addr_a.val;
    ref Int b = ref _addr_b.val;

    if (len(a.abs) == 0 || len(b.abs) == 0) {
        var lenA = len(a.abs);
        var lenB = len(b.abs);
        var negA = a.neg;
        var negB = b.neg;
        if (lenA == 0) {
            z.Set(b);
        }
        else
 {
            z.Set(a);
        }
        z.neg = false;
        if (x != null) {
            if (lenA == 0) {
                x.SetUint64(0);
            }
            else
 {
                x.SetUint64(1);
                x.neg = negA;
            }
        }
        if (y != null) {
            if (lenB == 0) {
                y.SetUint64(0);
            }
            else
 {
                y.SetUint64(1);
                y.neg = negB;
            }
        }
        return _addr_z!;
    }
    return _addr_z.lehmerGCD(x, y, a, b)!;
}

// lehmerSimulate attempts to simulate several Euclidean update steps
// using the leading digits of A and B.  It returns u0, u1, v0, v1
// such that A and B can be updated as:
//        A = u0*A + v0*B
//        B = u1*A + v1*B
// Requirements: A >= B and len(B.abs) >= 2
// Since we are calculating with full words to avoid overflow,
// we use 'even' to track the sign of the cosequences.
// For even iterations: u0, v1 >= 0 && u1, v0 <= 0
// For odd  iterations: u0, v1 <= 0 && u1, v0 >= 0
private static (Word, Word, Word, Word, bool) lehmerSimulate(ptr<Int> _addr_A, ptr<Int> _addr_B) {
    Word u0 = default;
    Word u1 = default;
    Word v0 = default;
    Word v1 = default;
    bool even = default;
    ref Int A = ref _addr_A.val;
    ref Int B = ref _addr_B.val;
 
    // initialize the digits
    Word a1 = default;    Word a2 = default;    Word u2 = default;    Word v2 = default;



    var m = len(B.abs); // m >= 2
    var n = len(A.abs); // n >= m >= 2

    // extract the top Word of bits from A and B
    var h = nlz(A.abs[n - 1]);
    a1 = A.abs[n - 1] << (int)(h) | A.abs[n - 2] >> (int)((_W - h)); 
    // B may have implicit zero words in the high bits if the lengths differ

    if (n == m) 
        a2 = B.abs[n - 1] << (int)(h) | B.abs[n - 2] >> (int)((_W - h));
    else if (n == m + 1) 
        a2 = B.abs[n - 2] >> (int)((_W - h));
    else 
        a2 = 0;
    // Since we are calculating with full words to avoid overflow,
    // we use 'even' to track the sign of the cosequences.
    // For even iterations: u0, v1 >= 0 && u1, v0 <= 0
    // For odd  iterations: u0, v1 <= 0 && u1, v0 >= 0
    // The first iteration starts with k=1 (odd).
    even = false; 
    // variables to track the cosequences
    (u0, u1, u2) = (0, 1, 0);    (v0, v1, v2) = (0, 0, 1);    while (a2 >= v2 && a1 - a2 >= v1 + v2) {
        var q = a1 / a2;
        var r = a1 % a2;
        (a1, a2) = (a2, r);        (u0, u1, u2) = (u1, u2, u1 + q * u2);        (v0, v1, v2) = (v1, v2, v1 + q * v2);        even = !even;
    }
    return ;
}

// lehmerUpdate updates the inputs A and B such that:
//        A = u0*A + v0*B
//        B = u1*A + v1*B
// where the signs of u0, u1, v0, v1 are given by even
// For even == true: u0, v1 >= 0 && u1, v0 <= 0
// For even == false: u0, v1 <= 0 && u1, v0 >= 0
// q, r, s, t are temporary variables to avoid allocations in the multiplication
private static void lehmerUpdate(ptr<Int> _addr_A, ptr<Int> _addr_B, ptr<Int> _addr_q, ptr<Int> _addr_r, ptr<Int> _addr_s, ptr<Int> _addr_t, Word u0, Word u1, Word v0, Word v1, bool even) {
    ref Int A = ref _addr_A.val;
    ref Int B = ref _addr_B.val;
    ref Int q = ref _addr_q.val;
    ref Int r = ref _addr_r.val;
    ref Int s = ref _addr_s.val;
    ref Int t = ref _addr_t.val;

    t.abs = t.abs.setWord(u0);
    s.abs = s.abs.setWord(v0);
    t.neg = !even;
    s.neg = even;

    t.Mul(A, t);
    s.Mul(B, s);

    r.abs = r.abs.setWord(u1);
    q.abs = q.abs.setWord(v1);
    r.neg = even;
    q.neg = !even;

    r.Mul(A, r);
    q.Mul(B, q);

    A.Add(t, s);
    B.Add(r, q);
}

// euclidUpdate performs a single step of the Euclidean GCD algorithm
// if extended is true, it also updates the cosequence Ua, Ub
private static void euclidUpdate(ptr<Int> _addr_A, ptr<Int> _addr_B, ptr<Int> _addr_Ua, ptr<Int> _addr_Ub, ptr<Int> _addr_q, ptr<Int> _addr_r, ptr<Int> _addr_s, ptr<Int> _addr_t, bool extended) {
    ref Int A = ref _addr_A.val;
    ref Int B = ref _addr_B.val;
    ref Int Ua = ref _addr_Ua.val;
    ref Int Ub = ref _addr_Ub.val;
    ref Int q = ref _addr_q.val;
    ref Int r = ref _addr_r.val;
    ref Int s = ref _addr_s.val;
    ref Int t = ref _addr_t.val;

    q, r = q.QuoRem(A, B, r);

    (A, B, r) = (B, r, A);    if (extended) { 
        // Ua, Ub = Ub, Ua - q*Ub
        t.Set(Ub);
        s.Mul(Ub, q);
        Ub.Sub(Ua, s);
        Ua.Set(t);
    }
}

// lehmerGCD sets z to the greatest common divisor of a and b,
// which both must be != 0, and returns z.
// If x or y are not nil, their values are set such that z = a*x + b*y.
// See Knuth, The Art of Computer Programming, Vol. 2, Section 4.5.2, Algorithm L.
// This implementation uses the improved condition by Collins requiring only one
// quotient and avoiding the possibility of single Word overflow.
// See Jebelean, "Improving the multiprecision Euclidean algorithm",
// Design and Implementation of Symbolic Computation Systems, pp 45-58.
// The cosequences are updated according to Algorithm 10.45 from
// Cohen et al. "Handbook of Elliptic and Hyperelliptic Curve Cryptography" pp 192.
private static ptr<Int> lehmerGCD(this ptr<Int> _addr_z, ptr<Int> _addr_x, ptr<Int> _addr_y, ptr<Int> _addr_a, ptr<Int> _addr_b) {
    ref Int z = ref _addr_z.val;
    ref Int x = ref _addr_x.val;
    ref Int y = ref _addr_y.val;
    ref Int a = ref _addr_a.val;
    ref Int b = ref _addr_b.val;

    ptr<Int> A;    ptr<Int> B;    ptr<Int> Ua;    ptr<Int> Ub;



    A = @new<Int>().Abs(a);
    B = @new<Int>().Abs(b);

    var extended = x != null || y != null;

    if (extended) { 
        // Ua (Ub) tracks how many times input a has been accumulated into A (B).
        Ua = @new<Int>().SetInt64(1);
        Ub = @new<Int>();
    }
    ptr<Int> q = @new<Int>();
    ptr<Int> r = @new<Int>();
    ptr<Int> s = @new<Int>();
    ptr<Int> t = @new<Int>(); 

    // ensure A >= B
    if (A.abs.cmp(B.abs) < 0) {
        (A, B) = (B, A);        (Ub, Ua) = (Ua, Ub);
    }
    while (len(B.abs) > 1) { 
        // Attempt to calculate in single-precision using leading words of A and B.
        var (u0, u1, v0, v1, even) = lehmerSimulate(A, B); 

        // multiprecision Step
        if (v0 != 0) { 
            // Simulate the effect of the single-precision steps using the cosequences.
            // A = u0*A + v0*B
            // B = u1*A + v1*B
            lehmerUpdate(A, B, q, r, s, t, u0, u1, v0, v1, even);

            if (extended) { 
                // Ua = u0*Ua + v0*Ub
                // Ub = u1*Ua + v1*Ub
                lehmerUpdate(Ua, Ub, q, r, s, t, u0, u1, v0, v1, even);
            }
        }
        else
 { 
            // Single-digit calculations failed to simulate any quotients.
            // Do a standard Euclidean step.
            euclidUpdate(A, B, Ua, Ub, q, r, s, t, extended);
        }
    }

    if (len(B.abs) > 0) { 
        // extended Euclidean algorithm base case if B is a single Word
        if (len(A.abs) > 1) { 
            // A is longer than a single Word, so one update is needed.
            euclidUpdate(A, B, Ua, Ub, q, r, s, t, extended);
        }
        if (len(B.abs) > 0) { 
            // A and B are both a single Word.
            var aWord = A.abs[0];
            var bWord = B.abs[0];
            if (extended) {
                Word ua = default;                Word ub = default;                Word va = default;                Word vb = default;

                (ua, ub) = (1, 0);                (va, vb) = (0, 1);                var even = true;
                while (bWord != 0) {
                    q = aWord / bWord;
                    r = aWord % bWord;
                    (aWord, bWord) = (bWord, r);                    (ua, ub) = (ub, ua + q * ub);                    (va, vb) = (vb, va + q * vb);                    even = !even;
                }
            else



                t.abs = t.abs.setWord(ua);
                s.abs = s.abs.setWord(va);
                t.neg = !even;
                s.neg = even;

                t.Mul(Ua, t);
                s.Mul(Ub, s);

                Ua.Add(t, s);
            } {
                while (bWord != 0) {
                    (aWord, bWord) = (bWord, aWord % bWord);
                }
            }
            A.abs[0] = aWord;
        }
    }
    var negA = a.neg;
    if (y != null) { 
        // avoid aliasing b needed in the division below
        if (y == b) {
            B.Set(b);
        }
        else
 {
            B = b;
        }
        y.Mul(a, Ua); // y can safely alias a
        if (negA) {
            y.neg = !y.neg;
        }
        y.Sub(A, y);
        y.Div(y, B);
    }
    if (x != null) {
        x = Ua.val;
        if (negA) {
            x.neg = !x.neg;
        }
    }
    z.val = A.val;

    return _addr_z!;
}

// Rand sets z to a pseudo-random number in [0, n) and returns z.
//
// As this uses the math/rand package, it must not be used for
// security-sensitive work. Use crypto/rand.Int instead.
private static ptr<Int> Rand(this ptr<Int> _addr_z, ptr<rand.Rand> _addr_rnd, ptr<Int> _addr_n) {
    ref Int z = ref _addr_z.val;
    ref rand.Rand rnd = ref _addr_rnd.val;
    ref Int n = ref _addr_n.val;

    z.neg = false;
    if (n.neg || len(n.abs) == 0) {
        z.abs = null;
        return _addr_z!;
    }
    z.abs = z.abs.random(rnd, n.abs, n.abs.bitLen());
    return _addr_z!;
}

// ModInverse sets z to the multiplicative inverse of g in the ring ℤ/nℤ
// and returns z. If g and n are not relatively prime, g has no multiplicative
// inverse in the ring ℤ/nℤ.  In this case, z is unchanged and the return value
// is nil.
private static ptr<Int> ModInverse(this ptr<Int> _addr_z, ptr<Int> _addr_g, ptr<Int> _addr_n) {
    ref Int z = ref _addr_z.val;
    ref Int g = ref _addr_g.val;
    ref Int n = ref _addr_n.val;
 
    // GCD expects parameters a and b to be > 0.
    if (n.neg) {
        Int n2 = default;
        n = n2.Neg(n);
    }
    if (g.neg) {
        Int g2 = default;
        g = g2.Mod(g, n);
    }
    Int d = default;    ref Int x = ref heap(out ptr<Int> _addr_x);

    d.GCD(_addr_x, null, g, n); 

    // if and only if d==1, g and n are relatively prime
    if (d.Cmp(intOne) != 0) {
        return _addr_null!;
    }
    if (x.neg) {
        z.Add(_addr_x, n);
    }
    else
 {
        z.Set(_addr_x);
    }
    return _addr_z!;
}

// Jacobi returns the Jacobi symbol (x/y), either +1, -1, or 0.
// The y argument must be an odd integer.
public static nint Jacobi(ptr<Int> _addr_x, ptr<Int> _addr_y) => func((_, panic, _) => {
    ref Int x = ref _addr_x.val;
    ref Int y = ref _addr_y.val;

    if (len(y.abs) == 0 || y.abs[0] & 1 == 0) {
        panic(fmt.Sprintf("big: invalid 2nd argument to Int.Jacobi: need odd integer but got %s", y));
    }
    ref Int a = ref heap(out ptr<Int> _addr_a);    ref Int b = ref heap(out ptr<Int> _addr_b);    ref Int c = ref heap(out ptr<Int> _addr_c);

    a.Set(x);
    b.Set(y);
    nint j = 1;

    if (b.neg) {
        if (a.neg) {
            j = -1;
        }
        b.neg = false;
    }
    while (true) {
        if (b.Cmp(intOne) == 0) {
            return j;
        }
        if (len(a.abs) == 0) {
            return 0;
        }
        a.Mod(_addr_a, _addr_b);
        if (len(a.abs) == 0) {
            return 0;
        }
        var s = a.abs.trailingZeroBits();
        if (s & 1 != 0) {
            var bmod8 = b.abs[0] & 7;
            if (bmod8 == 3 || bmod8 == 5) {
                j = -j;
            }
        }
        c.Rsh(_addr_a, s); // a = 2^s*c

        // swap numerator and denominator
        if (b.abs[0] & 3 == 3 && c.abs[0] & 3 == 3) {
            j = -j;
        }
        a.Set(_addr_b);
        b.Set(_addr_c);
    }
});

// modSqrt3Mod4 uses the identity
//      (a^((p+1)/4))^2  mod p
//   == u^(p+1)          mod p
//   == u^2              mod p
// to calculate the square root of any quadratic residue mod p quickly for 3
// mod 4 primes.
private static ptr<Int> modSqrt3Mod4Prime(this ptr<Int> _addr_z, ptr<Int> _addr_x, ptr<Int> _addr_p) {
    ref Int z = ref _addr_z.val;
    ref Int x = ref _addr_x.val;
    ref Int p = ref _addr_p.val;

    ptr<Int> e = @new<Int>().Add(p, intOne); // e = p + 1
    e.Rsh(e, 2); // e = (p + 1) / 4
    z.Exp(x, e, p); // z = x^e mod p
    return _addr_z!;
}

// modSqrt5Mod8 uses Atkin's observation that 2 is not a square mod p
//   alpha ==  (2*a)^((p-5)/8)    mod p
//   beta  ==  2*a*alpha^2        mod p  is a square root of -1
//   b     ==  a*alpha*(beta-1)   mod p  is a square root of a
// to calculate the square root of any quadratic residue mod p quickly for 5
// mod 8 primes.
private static ptr<Int> modSqrt5Mod8Prime(this ptr<Int> _addr_z, ptr<Int> _addr_x, ptr<Int> _addr_p) {
    ref Int z = ref _addr_z.val;
    ref Int x = ref _addr_x.val;
    ref Int p = ref _addr_p.val;
 
    // p == 5 mod 8 implies p = e*8 + 5
    // e is the quotient and 5 the remainder on division by 8
    ptr<Int> e = @new<Int>().Rsh(p, 3); // e = (p - 5) / 8
    ptr<Int> tx = @new<Int>().Lsh(x, 1); // tx = 2*x
    ptr<Int> alpha = @new<Int>().Exp(tx, e, p);
    ptr<Int> beta = @new<Int>().Mul(alpha, alpha);
    beta.Mod(beta, p);
    beta.Mul(beta, tx);
    beta.Mod(beta, p);
    beta.Sub(beta, intOne);
    beta.Mul(beta, x);
    beta.Mod(beta, p);
    beta.Mul(beta, alpha);
    z.Mod(beta, p);
    return _addr_z!;
}

// modSqrtTonelliShanks uses the Tonelli-Shanks algorithm to find the square
// root of a quadratic residue modulo any prime.
private static ptr<Int> modSqrtTonelliShanks(this ptr<Int> _addr_z, ptr<Int> _addr_x, ptr<Int> _addr_p) {
    ref Int z = ref _addr_z.val;
    ref Int x = ref _addr_x.val;
    ref Int p = ref _addr_p.val;
 
    // Break p-1 into s*2^e such that s is odd.
    ref Int s = ref heap(out ptr<Int> _addr_s);
    s.Sub(p, intOne);
    var e = s.abs.trailingZeroBits();
    s.Rsh(_addr_s, e); 

    // find some non-square n
    ref Int n = ref heap(out ptr<Int> _addr_n);
    n.SetInt64(2);
    while (Jacobi(_addr_n, _addr_p) != -1) {
        n.Add(_addr_n, intOne);
    } 

    // Core of the Tonelli-Shanks algorithm. Follows the description in
    // section 6 of "Square roots from 1; 24, 51, 10 to Dan Shanks" by Ezra
    // Brown:
    // https://www.maa.org/sites/default/files/pdf/upload_library/22/Polya/07468342.di020786.02p0470a.pdf
    ref Int y = ref heap(out ptr<Int> _addr_y);    ref Int b = ref heap(out ptr<Int> _addr_b);    ref Int g = ref heap(out ptr<Int> _addr_g);    ref Int t = ref heap(out ptr<Int> _addr_t);

    y.Add(_addr_s, intOne);
    y.Rsh(_addr_y, 1);
    y.Exp(x, _addr_y, p); // y = x^((s+1)/2)
    b.Exp(x, _addr_s, p); // b = x^s
    g.Exp(_addr_n, _addr_s, p); // g = n^s
    var r = e;
    while (true) { 
        // find the least m such that ord_p(b) = 2^m
        nuint m = default;
        t.Set(_addr_b);
        while (t.Cmp(intOne) != 0) {
            t.Mul(_addr_t, _addr_t).Mod(_addr_t, p);
            m++;
        }

        if (m == 0) {
            return _addr_z.Set(_addr_y)!;
        }
        t.SetInt64(0).SetBit(_addr_t, int(r - m - 1), 1).Exp(_addr_g, _addr_t, p); 
        // t = g^(2^(r-m-1)) mod p
        g.Mul(_addr_t, _addr_t).Mod(_addr_g, p); // g = g^(2^(r-m)) mod p
        y.Mul(_addr_y, _addr_t).Mod(_addr_y, p);
        b.Mul(_addr_b, _addr_g).Mod(_addr_b, p);
        r = m;
    }
}

// ModSqrt sets z to a square root of x mod p if such a square root exists, and
// returns z. The modulus p must be an odd prime. If x is not a square mod p,
// ModSqrt leaves z unchanged and returns nil. This function panics if p is
// not an odd integer.
private static ptr<Int> ModSqrt(this ptr<Int> _addr_z, ptr<Int> _addr_x, ptr<Int> _addr_p) {
    ref Int z = ref _addr_z.val;
    ref Int x = ref _addr_x.val;
    ref Int p = ref _addr_p.val;

    switch (Jacobi(_addr_x, _addr_p)) {
        case -1: 
            return _addr_null!; // x is not a square mod p
            break;
        case 0: 
            return _addr_z.SetInt64(0)!; // sqrt(0) mod p = 0
            break;
        case 1: 
            break;
            break;
    }
    if (x.neg || x.Cmp(p) >= 0) { // ensure 0 <= x < p
        x = @new<Int>().Mod(x, p);
    }

    if (p.abs[0] % 4 == 3) 
        // Check whether p is 3 mod 4, and if so, use the faster algorithm.
        return _addr_z.modSqrt3Mod4Prime(x, p)!;
    else if (p.abs[0] % 8 == 5) 
        // Check whether p is 5 mod 8, use Atkin's algorithm.
        return _addr_z.modSqrt5Mod8Prime(x, p)!;
    else 
        // Otherwise, use Tonelli-Shanks.
        return _addr_z.modSqrtTonelliShanks(x, p)!;
    }

// Lsh sets z = x << n and returns z.
private static ptr<Int> Lsh(this ptr<Int> _addr_z, ptr<Int> _addr_x, nuint n) {
    ref Int z = ref _addr_z.val;
    ref Int x = ref _addr_x.val;

    z.abs = z.abs.shl(x.abs, n);
    z.neg = x.neg;
    return _addr_z!;
}

// Rsh sets z = x >> n and returns z.
private static ptr<Int> Rsh(this ptr<Int> _addr_z, ptr<Int> _addr_x, nuint n) {
    ref Int z = ref _addr_z.val;
    ref Int x = ref _addr_x.val;

    if (x.neg) { 
        // (-x) >> s == ^(x-1) >> s == ^((x-1) >> s) == -(((x-1) >> s) + 1)
        var t = z.abs.sub(x.abs, natOne); // no underflow because |x| > 0
        t = t.shr(t, n);
        z.abs = t.add(t, natOne);
        z.neg = true; // z cannot be zero if x is negative
        return _addr_z!;
    }
    z.abs = z.abs.shr(x.abs, n);
    z.neg = false;
    return _addr_z!;
}

// Bit returns the value of the i'th bit of x. That is, it
// returns (x>>i)&1. The bit index i must be >= 0.
private static nuint Bit(this ptr<Int> _addr_x, nint i) => func((_, panic, _) => {
    ref Int x = ref _addr_x.val;

    if (i == 0) { 
        // optimization for common case: odd/even test of x
        if (len(x.abs) > 0) {
            return uint(x.abs[0] & 1); // bit 0 is same for -x
        }
        return 0;
    }
    if (i < 0) {
        panic("negative bit index");
    }
    if (x.neg) {
        var t = nat(null).sub(x.abs, natOne);
        return t.bit(uint(i)) ^ 1;
    }
    return x.abs.bit(uint(i));
});

// SetBit sets z to x, with x's i'th bit set to b (0 or 1).
// That is, if b is 1 SetBit sets z = x | (1 << i);
// if b is 0 SetBit sets z = x &^ (1 << i). If b is not 0 or 1,
// SetBit will panic.
private static ptr<Int> SetBit(this ptr<Int> _addr_z, ptr<Int> _addr_x, nint i, nuint b) => func((_, panic, _) => {
    ref Int z = ref _addr_z.val;
    ref Int x = ref _addr_x.val;

    if (i < 0) {
        panic("negative bit index");
    }
    if (x.neg) {
        var t = z.abs.sub(x.abs, natOne);
        t = t.setBit(t, uint(i), b ^ 1);
        z.abs = t.add(t, natOne);
        z.neg = len(z.abs) > 0;
        return _addr_z!;
    }
    z.abs = z.abs.setBit(x.abs, uint(i), b);
    z.neg = false;
    return _addr_z!;
});

// And sets z = x & y and returns z.
private static ptr<Int> And(this ptr<Int> _addr_z, ptr<Int> _addr_x, ptr<Int> _addr_y) {
    ref Int z = ref _addr_z.val;
    ref Int x = ref _addr_x.val;
    ref Int y = ref _addr_y.val;

    if (x.neg == y.neg) {
        if (x.neg) { 
            // (-x) & (-y) == ^(x-1) & ^(y-1) == ^((x-1) | (y-1)) == -(((x-1) | (y-1)) + 1)
            var x1 = nat(null).sub(x.abs, natOne);
            var y1 = nat(null).sub(y.abs, natOne);
            z.abs = z.abs.add(z.abs.or(x1, y1), natOne);
            z.neg = true; // z cannot be zero if x and y are negative
            return _addr_z!;
        }
        z.abs = z.abs.and(x.abs, y.abs);
        z.neg = false;
        return _addr_z!;
    }
    if (x.neg) {
        (x, y) = (y, x);
    }
    y1 = nat(null).sub(y.abs, natOne);
    z.abs = z.abs.andNot(x.abs, y1);
    z.neg = false;
    return _addr_z!;
}

// AndNot sets z = x &^ y and returns z.
private static ptr<Int> AndNot(this ptr<Int> _addr_z, ptr<Int> _addr_x, ptr<Int> _addr_y) {
    ref Int z = ref _addr_z.val;
    ref Int x = ref _addr_x.val;
    ref Int y = ref _addr_y.val;

    if (x.neg == y.neg) {
        if (x.neg) { 
            // (-x) &^ (-y) == ^(x-1) &^ ^(y-1) == ^(x-1) & (y-1) == (y-1) &^ (x-1)
            var x1 = nat(null).sub(x.abs, natOne);
            var y1 = nat(null).sub(y.abs, natOne);
            z.abs = z.abs.andNot(y1, x1);
            z.neg = false;
            return _addr_z!;
        }
        z.abs = z.abs.andNot(x.abs, y.abs);
        z.neg = false;
        return _addr_z!;
    }
    if (x.neg) { 
        // (-x) &^ y == ^(x-1) &^ y == ^(x-1) & ^y == ^((x-1) | y) == -(((x-1) | y) + 1)
        x1 = nat(null).sub(x.abs, natOne);
        z.abs = z.abs.add(z.abs.or(x1, y.abs), natOne);
        z.neg = true; // z cannot be zero if x is negative and y is positive
        return _addr_z!;
    }
    y1 = nat(null).sub(y.abs, natOne);
    z.abs = z.abs.and(x.abs, y1);
    z.neg = false;
    return _addr_z!;
}

// Or sets z = x | y and returns z.
private static ptr<Int> Or(this ptr<Int> _addr_z, ptr<Int> _addr_x, ptr<Int> _addr_y) {
    ref Int z = ref _addr_z.val;
    ref Int x = ref _addr_x.val;
    ref Int y = ref _addr_y.val;

    if (x.neg == y.neg) {
        if (x.neg) { 
            // (-x) | (-y) == ^(x-1) | ^(y-1) == ^((x-1) & (y-1)) == -(((x-1) & (y-1)) + 1)
            var x1 = nat(null).sub(x.abs, natOne);
            var y1 = nat(null).sub(y.abs, natOne);
            z.abs = z.abs.add(z.abs.and(x1, y1), natOne);
            z.neg = true; // z cannot be zero if x and y are negative
            return _addr_z!;
        }
        z.abs = z.abs.or(x.abs, y.abs);
        z.neg = false;
        return _addr_z!;
    }
    if (x.neg) {
        (x, y) = (y, x);
    }
    y1 = nat(null).sub(y.abs, natOne);
    z.abs = z.abs.add(z.abs.andNot(y1, x.abs), natOne);
    z.neg = true; // z cannot be zero if one of x or y is negative
    return _addr_z!;
}

// Xor sets z = x ^ y and returns z.
private static ptr<Int> Xor(this ptr<Int> _addr_z, ptr<Int> _addr_x, ptr<Int> _addr_y) {
    ref Int z = ref _addr_z.val;
    ref Int x = ref _addr_x.val;
    ref Int y = ref _addr_y.val;

    if (x.neg == y.neg) {
        if (x.neg) { 
            // (-x) ^ (-y) == ^(x-1) ^ ^(y-1) == (x-1) ^ (y-1)
            var x1 = nat(null).sub(x.abs, natOne);
            var y1 = nat(null).sub(y.abs, natOne);
            z.abs = z.abs.xor(x1, y1);
            z.neg = false;
            return _addr_z!;
        }
        z.abs = z.abs.xor(x.abs, y.abs);
        z.neg = false;
        return _addr_z!;
    }
    if (x.neg) {
        (x, y) = (y, x);
    }
    y1 = nat(null).sub(y.abs, natOne);
    z.abs = z.abs.add(z.abs.xor(x.abs, y1), natOne);
    z.neg = true; // z cannot be zero if only one of x or y is negative
    return _addr_z!;
}

// Not sets z = ^x and returns z.
private static ptr<Int> Not(this ptr<Int> _addr_z, ptr<Int> _addr_x) {
    ref Int z = ref _addr_z.val;
    ref Int x = ref _addr_x.val;

    if (x.neg) { 
        // ^(-x) == ^(^(x-1)) == x-1
        z.abs = z.abs.sub(x.abs, natOne);
        z.neg = false;
        return _addr_z!;
    }
    z.abs = z.abs.add(x.abs, natOne);
    z.neg = true; // z cannot be zero if x is positive
    return _addr_z!;
}

// Sqrt sets z to ⌊√x⌋, the largest integer such that z² ≤ x, and returns z.
// It panics if x is negative.
private static ptr<Int> Sqrt(this ptr<Int> _addr_z, ptr<Int> _addr_x) => func((_, panic, _) => {
    ref Int z = ref _addr_z.val;
    ref Int x = ref _addr_x.val;

    if (x.neg) {
        panic("square root of negative number");
    }
    z.neg = false;
    z.abs = z.abs.sqrt(x.abs);
    return _addr_z!;
});

} // end big_package
