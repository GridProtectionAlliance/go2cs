// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements signed multi-precision integers.
namespace go.math;

using fmt = fmt_package;
using io = io_package;
using rand = go.math.rand_package;
using strings = strings_package;
using go.math;

partial class big_package {

// An Int represents a signed multi-precision integer.
// The zero value for an Int represents the value 0.
//
// Operations always take pointer arguments (*Int) rather
// than Int values, and each unique Int value requires
// its own unique *Int pointer. To "copy" an Int value,
// an existing (or newly allocated) Int must be set to
// a new value using the [Int.Set] method; shallow copies
// of Ints are not supported and may lead to errors.
//
// Note that methods may leak the Int's value through timing side-channels.
// Because of this and because of the scope and complexity of the
// implementation, Int is not well-suited to implement cryptographic operations.
// The standard library avoids exposing non-trivial Int methods to
// attacker-controlled inputs and the determination of whether a bug in math/big
// is considered a security vulnerability might depend on the impact on the
// standard library.
[GoType] partial struct ΔInt {
    internal bool neg; // sign
    internal nat abs;  // absolute value of the integer
}

internal static ж<ΔInt> intOne;
internal static void initᴛintOne() { intOne = Ꮡ(new ΔInt(false, natOne)); }

// Sign returns:
//   - -1 if x < 0;
//   - 0 if x == 0;
//   - +1 if x > 0.
[GoRecv] public static nint Sign(this ref ΔInt x) {
    // This function is used in cryptographic operations. It must not leak
    // anything but the Int's sign and bit size through side-channels. Any
    // changes must be reviewed by a security expert.
    if (len(x.abs) == 0) {
        return 0;
    }
    if (x.neg) {
        return -1;
    }
    return 1;
}

// SetInt64 sets z to x and returns z.
public static ж<ΔInt> SetInt64(this ж<ΔInt> Ꮡz, int64 x) {
    ref var z = ref Ꮡz.Value;

    var neg = false;
    if (x < 0) {
        neg = true;
        x = -x;
    }
    z.abs = z.abs.setUint64((uint64)x);
    z.neg = neg;
    return Ꮡz;
}

// SetUint64 sets z to x and returns z.
public static ж<ΔInt> SetUint64(this ж<ΔInt> Ꮡz, uint64 x) {
    ref var z = ref Ꮡz.Value;

    z.abs = z.abs.setUint64(x);
    z.neg = false;
    return Ꮡz;
}

// NewInt allocates and returns a new [Int] set to x.
public static ж<ΔInt> NewInt(int64 x) {
    // This code is arranged to be inlineable and produce
    // zero allocations when inlined. See issue 29951.
    var u = (uint64)x;
    if (x < 0) {
        u = ((uint64)0 - u);
    }
    slice<Word> abs = default!;
    if (x == 0){
    } else 
    if (_W == 32 && (u >> (int)(32)) != 0){
        abs = new Word[]{((Word)(nuint)u), ((Word)(nuint)((u >> (int)(32))))}.slice();
    } else {
        abs = new Word[]{((Word)(nuint)u)}.slice();
    }
    return Ꮡ(new ΔInt(neg: x < 0, abs: abs));
}

// Set sets z to x and returns z.
public static ж<ΔInt> Set(this ж<ΔInt> Ꮡz, ж<ΔInt> Ꮡx) {
    ref var z = ref Ꮡz.Value;
    ref var x = ref Ꮡx.DerefOrNil();

    if (Ꮡz != Ꮡx) {
        z.abs = z.abs.set(x.abs);
        z.neg = x.neg;
    }
    return Ꮡz;
}

// Bits provides raw (unchecked but fast) access to x by returning its
// absolute value as a little-endian [Word] slice. The result and x share
// the same underlying array.
// Bits is intended to support implementation of missing low-level [Int]
// functionality outside this package; it should be avoided otherwise.
[GoRecv] public static slice<Word> Bits(this ref ΔInt x) {
    // This function is used in cryptographic operations. It must not leak
    // anything but the Int's sign and bit size through side-channels. Any
    // changes must be reviewed by a security expert.
    return x.abs;
}

// SetBits provides raw (unchecked but fast) access to z by setting its
// value to abs, interpreted as a little-endian [Word] slice, and returning
// z. The result and abs share the same underlying array.
// SetBits is intended to support implementation of missing low-level [Int]
// functionality outside this package; it should be avoided otherwise.
public static ж<ΔInt> SetBits(this ж<ΔInt> Ꮡz, slice<Word> abs) {
    ref var z = ref Ꮡz.Value;

    z.abs = ((nat)abs).norm();
    z.neg = false;
    return Ꮡz;
}

// Abs sets z to |x| (the absolute value of x) and returns z.
public static ж<ΔInt> Abs(this ж<ΔInt> Ꮡz, ж<ΔInt> Ꮡx) {
    ref var z = ref Ꮡz.Value;

    Ꮡz.Set(Ꮡx);
    z.neg = false;
    return Ꮡz;
}

// Neg sets z to -x and returns z.
public static ж<ΔInt> Neg(this ж<ΔInt> Ꮡz, ж<ΔInt> Ꮡx) {
    ref var z = ref Ꮡz.Value;

    Ꮡz.Set(Ꮡx);
    z.neg = len(z.abs) > 0 && !z.neg;
    // 0 has no sign
    return Ꮡz;
}

// Add sets z to the sum x+y and returns z.
public static ж<ΔInt> Add(this ж<ΔInt> Ꮡz, ж<ΔInt> Ꮡx, ж<ΔInt> Ꮡy) {
    ref var z = ref Ꮡz.Value;
    ref var x = ref Ꮡx.Value;
    ref var y = ref Ꮡy.Value;

    var neg = x.neg;
    if (x.neg == y.neg){
        // x + y == x + y
        // (-x) + (-y) == -(x + y)
        z.abs = z.abs.add(x.abs, y.abs);
    } else {
        // x + (-y) == x - y == -(y - x)
        // (-x) + y == y - x == -(x - y)
        if (x.abs.cmp(y.abs) >= 0){
            z.abs = z.abs.sub(x.abs, y.abs);
        } else {
            neg = !neg;
            z.abs = z.abs.sub(y.abs, x.abs);
        }
    }
    z.neg = len(z.abs) > 0 && neg;
    // 0 has no sign
    return Ꮡz;
}

// Sub sets z to the difference x-y and returns z.
public static ж<ΔInt> Sub(this ж<ΔInt> Ꮡz, ж<ΔInt> Ꮡx, ж<ΔInt> Ꮡy) {
    ref var z = ref Ꮡz.Value;
    ref var x = ref Ꮡx.Value;
    ref var y = ref Ꮡy.Value;

    var neg = x.neg;
    if (x.neg != y.neg){
        // x - (-y) == x + y
        // (-x) - y == -(x + y)
        z.abs = z.abs.add(x.abs, y.abs);
    } else {
        // x - y == x - y == -(y - x)
        // (-x) - (-y) == y - x == -(x - y)
        if (x.abs.cmp(y.abs) >= 0){
            z.abs = z.abs.sub(x.abs, y.abs);
        } else {
            neg = !neg;
            z.abs = z.abs.sub(y.abs, x.abs);
        }
    }
    z.neg = len(z.abs) > 0 && neg;
    // 0 has no sign
    return Ꮡz;
}

// Mul sets z to the product x*y and returns z.
public static ж<ΔInt> Mul(this ж<ΔInt> Ꮡz, ж<ΔInt> Ꮡx, ж<ΔInt> Ꮡy) {
    ref var z = ref Ꮡz.Value;
    ref var x = ref Ꮡx.DerefOrNil();
    ref var y = ref Ꮡy.DerefOrNil();

    // x * y == x * y
    // x * (-y) == -(x * y)
    // (-x) * y == -(x * y)
    // (-x) * (-y) == x * y
    if (Ꮡx == Ꮡy) {
        z.abs = z.abs.sqr(x.abs);
        z.neg = false;
        return Ꮡz;
    }
    z.abs = z.abs.mul(x.abs, y.abs);
    z.neg = len(z.abs) > 0 && x.neg != y.neg;
    // 0 has no sign
    return Ꮡz;
}

// MulRange sets z to the product of all integers
// in the range [a, b] inclusively and returns z.
// If a > b (empty range), the result is 1.
public static ж<ΔInt> MulRange(this ж<ΔInt> Ꮡz, int64 a, int64 b) {
    ref var z = ref Ꮡz.Value;

    switch (ᐧ) {
    case {} when a > b: {
        return Ꮡz.SetInt64(1);
    }
    case {} when a <= 0 && b >= 0: {
        return Ꮡz.SetInt64(0);
    }}

    // empty range
    // range includes 0
    // a <= b && (b < 0 || a > 0)
    var neg = false;
    if (a < 0) {
        neg = (int64)((b - a) & 1) == 0;
        (a, b) = (-b, -a);
    }
    z.abs = z.abs.mulRange((uint64)a, (uint64)b);
    z.neg = neg;
    return Ꮡz;
}

// Binomial sets z to the binomial coefficient C(n, k) and returns z.
public static ж<ΔInt> Binomial(this ж<ΔInt> Ꮡz, int64 n, int64 k) {
    ref var z = ref Ꮡz.Value;

    if (k > n) {
        return Ꮡz.SetInt64(0);
    }
    // reduce the number of multiplications by reducing k
    if (k > n - k) {
        k = n - k;
    }
    // C(n, k) == C(n, n-k)
    // C(n, k) == n * (n-1) * ... * (n-k+1) / k * (k-1) * ... * 1
    //         == n * (n-1) * ... * (n-k+1) / 1 * (1+1) * ... * k
    //
    // Using the multiplicative formula produces smaller values
    // at each step, requiring fewer allocations and computations:
    //
    // z = 1
    // for i := 0; i < k; i = i+1 {
    //     z *= n-i
    //     z /= i+1
    // }
    //
    // finally to avoid computing i+1 twice per loop:
    //
    // z = 1
    // i := 0
    // for i < k {
    //     z *= n-i
    //     i++
    //     z /= i
    // }
    ref var N = ref heap(new ΔInt(), out var ᏑN);
    ref var K = ref heap(new ΔInt(), out var ᏑK);
    ref var i = ref heap(new ΔInt(), out var Ꮡi);
    ref var t = ref heap(new ΔInt(), out var Ꮡt);
    ᏑN.SetInt64(n);
    ᏑK.SetInt64(k);
    Ꮡz.Set(intOne);
    while (Ꮡi.Cmp(ᏑK) < 0) {
        Ꮡz.Mul(Ꮡz, Ꮡt.Sub(ᏑN, Ꮡi));
        Ꮡi.Add(Ꮡi, intOne);
        Ꮡz.Quo(Ꮡz, Ꮡi);
    }
    return Ꮡz;
}

// Quo sets z to the quotient x/y for y != 0 and returns z.
// If y == 0, a division-by-zero run-time panic occurs.
// Quo implements truncated division (like Go); see [Int.QuoRem] for more details.
public static ж<ΔInt> Quo(this ж<ΔInt> Ꮡz, ж<ΔInt> Ꮡx, ж<ΔInt> Ꮡy) {
    ref var z = ref Ꮡz.Value;
    ref var x = ref Ꮡx.Value;
    ref var y = ref Ꮡy.Value;

    (z.abs, _) = z.abs.div(default!, x.abs, y.abs);
    z.neg = len(z.abs) > 0 && x.neg != y.neg;
    // 0 has no sign
    return Ꮡz;
}

// Rem sets z to the remainder x%y for y != 0 and returns z.
// If y == 0, a division-by-zero run-time panic occurs.
// Rem implements truncated modulus (like Go); see [Int.QuoRem] for more details.
public static ж<ΔInt> Rem(this ж<ΔInt> Ꮡz, ж<ΔInt> Ꮡx, ж<ΔInt> Ꮡy) {
    ref var z = ref Ꮡz.Value;
    ref var x = ref Ꮡx.Value;
    ref var y = ref Ꮡy.Value;

    (_, z.abs) = ((nat)default!).div(z.abs, x.abs, y.abs);
    z.neg = len(z.abs) > 0 && x.neg;
    // 0 has no sign
    return Ꮡz;
}

// QuoRem sets z to the quotient x/y and r to the remainder x%y
// and returns the pair (z, r) for y != 0.
// If y == 0, a division-by-zero run-time panic occurs.
//
// QuoRem implements T-division and modulus (like Go):
//
//	q = x/y      with the result truncated to zero
//	r = x - y*q
//
// (See Daan Leijen, “Division and Modulus for Computer Scientists”.)
// See [DivMod] for Euclidean division and modulus (unlike Go).
public static (ж<ΔInt>, ж<ΔInt>) QuoRem(this ж<ΔInt> Ꮡz, ж<ΔInt> Ꮡx, ж<ΔInt> Ꮡy, ж<ΔInt> Ꮡr) {
    ref var z = ref Ꮡz.Value;
    ref var x = ref Ꮡx.Value;
    ref var y = ref Ꮡy.Value;
    ref var r = ref Ꮡr.Value;

    (z.abs, r.abs) = z.abs.div(r.abs, x.abs, y.abs);
    z.neg = len(z.abs) > 0 && x.neg != y.neg;
    r.neg = len(r.abs) > 0 && x.neg;
    // 0 has no sign
    return (Ꮡz, Ꮡr);
}

// Div sets z to the quotient x/y for y != 0 and returns z.
// If y == 0, a division-by-zero run-time panic occurs.
// Div implements Euclidean division (unlike Go); see [Int.DivMod] for more details.
public static ж<ΔInt> Div(this ж<ΔInt> Ꮡz, ж<ΔInt> Ꮡx, ж<ΔInt> Ꮡy) {
    ref var z = ref Ꮡz.Value;
    ref var y = ref Ꮡy.Value;

    var y_neg = y.neg;
    // z may be an alias for y
    ref var r = ref heap(new ΔInt(), out var Ꮡr);
    Ꮡz.QuoRem(Ꮡx, Ꮡy, Ꮡr);
    if (r.neg) {
        if (y_neg){
            Ꮡz.Add(Ꮡz, intOne);
        } else {
            Ꮡz.Sub(Ꮡz, intOne);
        }
    }
    return Ꮡz;
}

// Mod sets z to the modulus x%y for y != 0 and returns z.
// If y == 0, a division-by-zero run-time panic occurs.
// Mod implements Euclidean modulus (unlike Go); see [Int.DivMod] for more details.
public static ж<ΔInt> Mod(this ж<ΔInt> Ꮡz, ж<ΔInt> Ꮡx, ж<ΔInt> Ꮡy) {
    ref var z = ref Ꮡz.Value;
    ref var y = ref Ꮡy.DerefOrNil();

    var y0 = Ꮡy;
    // save y
    if (Ꮡz == Ꮡy || alias(z.abs, y.abs)) {
        y0 = @new<ΔInt>().Set(Ꮡy);
    }
    ref var q = ref heap(new ΔInt(), out var Ꮡq);
    Ꮡq.QuoRem(Ꮡx, Ꮡy, Ꮡz);
    if (z.neg) {
        if ((~y0).neg){
            Ꮡz.Sub(Ꮡz, y0);
        } else {
            Ꮡz.Add(Ꮡz, y0);
        }
    }
    return Ꮡz;
}

// DivMod sets z to the quotient x div y and m to the modulus x mod y
// and returns the pair (z, m) for y != 0.
// If y == 0, a division-by-zero run-time panic occurs.
//
// DivMod implements Euclidean division and modulus (unlike Go):
//
//	q = x div y  such that
//	m = x - y*q  with 0 <= m < |y|
//
// (See Raymond T. Boute, “The Euclidean definition of the functions
// div and mod”. ACM Transactions on Programming Languages and
// Systems (TOPLAS), 14(2):127-144, New York, NY, USA, 4/1992.
// ACM press.)
// See [Int.QuoRem] for T-division and modulus (like Go).
public static (ж<ΔInt>, ж<ΔInt>) DivMod(this ж<ΔInt> Ꮡz, ж<ΔInt> Ꮡx, ж<ΔInt> Ꮡy, ж<ΔInt> Ꮡm) {
    ref var z = ref Ꮡz.Value;
    ref var y = ref Ꮡy.DerefOrNil();
    ref var m = ref Ꮡm.Value;

    var y0 = Ꮡy;
    // save y
    if (Ꮡz == Ꮡy || alias(z.abs, y.abs)) {
        y0 = @new<ΔInt>().Set(Ꮡy);
    }
    Ꮡz.QuoRem(Ꮡx, Ꮡy, Ꮡm);
    if (m.neg) {
        if ((~y0).neg){
            Ꮡz.Add(Ꮡz, intOne);
            Ꮡm.Sub(Ꮡm, y0);
        } else {
            Ꮡz.Sub(Ꮡz, intOne);
            Ꮡm.Add(Ꮡm, y0);
        }
    }
    return (Ꮡz, Ꮡm);
}

// Cmp compares x and y and returns:
//   - -1 if x < y;
//   - 0 if x == y;
//   - +1 if x > y.
public static nint /*r*/ Cmp(this ж<ΔInt> Ꮡx, ж<ΔInt> Ꮡy) {
    nint r = default!;

    ref var x = ref Ꮡx.Value;
    ref var y = ref Ꮡy.DerefOrNil();
    // x cmp y == x cmp y
    // x cmp (-y) == x
    // (-x) cmp y == y
    // (-x) cmp (-y) == -(x cmp y)
    switch (ᐧ) {
    case {} when Ꮡx == Ꮡy: {
        break;
    }
    case {} when x.neg == y.neg: {
        r = x.abs.cmp(y.abs);
        if (x.neg) {
            // nothing to do
            r = -r;
        }
        break;
    }
    case {} when x.neg: {
        r = -1;
        break;
    }
    default: {
        r = 1;
        break;
    }}

    return r;
}

// CmpAbs compares the absolute values of x and y and returns:
//   - -1 if |x| < |y|;
//   - 0 if |x| == |y|;
//   - +1 if |x| > |y|.
[GoRecv] public static nint CmpAbs(this ref ΔInt x, ж<ΔInt> Ꮡy) {
    ref var y = ref Ꮡy.Value;

    return x.abs.cmp(y.abs);
}

// low32 returns the least significant 32 bits of x.
internal static uint32 low32(nat x) {
    if (len(x) == 0) {
        return 0;
    }
    return (uint32)(nuint)x[0];
}

// low64 returns the least significant 64 bits of x.
internal static uint64 low64(nat x) {
    if (len(x) == 0) {
        return 0;
    }
    var v = (uint64)(nuint)x[0];
    if (_W == 32 && len(x) > 1) {
        return (uint64)(((uint64)(nuint)x[1] << (int)(32)) | v);
    }
    return v;
}

// Int64 returns the int64 representation of x.
// If x cannot be represented in an int64, the result is undefined.
[GoRecv] public static int64 Int64(this ref ΔInt x) {
    var v = (int64)low64(x.abs);
    if (x.neg) {
        v = -v;
    }
    return v;
}

// Uint64 returns the uint64 representation of x.
// If x cannot be represented in a uint64, the result is undefined.
[GoRecv] public static uint64 Uint64(this ref ΔInt x) {
    return low64(x.abs);
}

// IsInt64 reports whether x can be represented as an int64.
[GoRecv] public static bool IsInt64(this ref ΔInt x) {
    if (len(x.abs) <= 64 / _W) {
        var w = (int64)low64(x.abs);
        return w >= 0 || x.neg && w == -w;
    }
    return false;
}

// IsUint64 reports whether x can be represented as a uint64.
[GoRecv] public static bool IsUint64(this ref ΔInt x) {
    return !x.neg && len(x.abs) <= 64 / _W;
}

// Float64 returns the float64 value nearest x,
// and an indication of any rounding that occurred.
public static (float64, Accuracy) Float64(this ж<ΔInt> Ꮡx) {
    ref var x = ref Ꮡx.Value;

    nint n = x.abs.bitLen();
    // NB: still uses slow crypto impl!
    if (n == 0) {
        return (0.0D, Exact);
    }
    // Fast path: no more than 53 significant bits.
    if (n <= 53 || n < 64 && n - (nint)x.abs.trailingZeroBits() <= 53) {
        var f = (float64)low64(x.abs);
        if (x.neg) {
            f = -f;
        }
        return (f, Exact);
    }
    return @new<Float>().SetInt(Ꮡx).Float64();
}

// SetString sets z to the value of s, interpreted in the given base,
// and returns z and a boolean indicating success. The entire string
// (not just a prefix) must be valid for success. If SetString fails,
// the value of z is undefined but the returned value is nil.
//
// The base argument must be 0 or a value between 2 and [MaxBase].
// For base 0, the number prefix determines the actual base: A prefix of
// “0b” or “0B” selects base 2, “0”, “0o” or “0O” selects base 8,
// and “0x” or “0X” selects base 16. Otherwise, the selected base is 10
// and no prefix is accepted.
//
// For bases <= 36, lower and upper case letters are considered the same:
// The letters 'a' to 'z' and 'A' to 'Z' represent digit values 10 to 35.
// For bases > 36, the upper case letters 'A' to 'Z' represent the digit
// values 36 to 61.
//
// For base 0, an underscore character “_” may appear between a base
// prefix and an adjacent digit, and between successive digits; such
// underscores do not change the value of the number.
// Incorrect placement of underscores is reported as an error if there
// are no other errors. If base != 0, underscores are not recognized
// and act like any other character that is not a valid digit.
public static (ж<ΔInt>, bool) SetString(this ж<ΔInt> Ꮡz, @string s, nint @base) {
    return Ꮡz.setFromScanner(new strings_ReaderжByteScanner(strings.NewReader(s)), @base);
}

// setFromScanner implements SetString given an io.ByteScanner.
// For documentation see comments of SetString.
internal static (ж<ΔInt>, bool) setFromScanner(this ж<ΔInt> Ꮡz, io.ByteScanner r, nint @base) {
    {
        var (_, _, err) = Ꮡz.scan(r, @base); if (err != default!) {
            return (default!, false);
        }
    }
    // entire content must have been consumed
    {
        var (_, err) = r.ReadByte(); if (!AreEqual(err, io.EOF)) {
            return (default!, false);
        }
    }
    return (Ꮡz, true);
}

// err == io.EOF => scan consumed all content of r

// SetBytes interprets buf as the bytes of a big-endian unsigned
// integer, sets z to that value, and returns z.
public static ж<ΔInt> SetBytes(this ж<ΔInt> Ꮡz, slice<byte> buf) {
    ref var z = ref Ꮡz.Value;

    z.abs = z.abs.setBytes(buf);
    z.neg = false;
    return Ꮡz;
}

// Bytes returns the absolute value of x as a big-endian byte slice.
//
// To use a fixed length slice, or a preallocated one, use [Int.FillBytes].
[GoRecv] public static slice<byte> Bytes(this ref ΔInt x) {
    // This function is used in cryptographic operations. It must not leak
    // anything but the Int's sign and bit size through side-channels. Any
    // changes must be reviewed by a security expert.
    var buf = new slice<byte>(len(x.abs) * (nint)_S);
    return buf[(int)(x.abs.bytes(buf))..];
}

// FillBytes sets buf to the absolute value of x, storing it as a zero-extended
// big-endian byte slice, and returns buf.
//
// If the absolute value of x doesn't fit in buf, FillBytes will panic.
[GoRecv] public static slice<byte> FillBytes(this ref ΔInt x, slice<byte> buf) {
    // Clear whole buffer.
    clear(buf);
    x.abs.bytes(buf);
    return buf;
}

// BitLen returns the length of the absolute value of x in bits.
// The bit length of 0 is 0.
[GoRecv] public static nint BitLen(this ref ΔInt x) {
    // This function is used in cryptographic operations. It must not leak
    // anything but the Int's sign and bit size through side-channels. Any
    // changes must be reviewed by a security expert.
    return x.abs.bitLen();
}

// TrailingZeroBits returns the number of consecutive least significant zero
// bits of |x|.
[GoRecv] public static nuint TrailingZeroBits(this ref ΔInt x) {
    return x.abs.trailingZeroBits();
}

// Exp sets z = x**y mod |m| (i.e. the sign of m is ignored), and returns z.
// If m == nil or m == 0, z = x**y unless y <= 0 then z = 1. If m != 0, y < 0,
// and x and m are not relatively prime, z is unchanged and nil is returned.
//
// Modular exponentiation of inputs of a particular size is not a
// cryptographically constant-time operation.
public static ж<ΔInt> Exp(this ж<ΔInt> Ꮡz, ж<ΔInt> Ꮡx, ж<ΔInt> Ꮡy, ж<ΔInt> Ꮡm) {
    return Ꮡz.exp(Ꮡx, Ꮡy, Ꮡm, false);
}

internal static ж<ΔInt> expSlow(this ж<ΔInt> Ꮡz, ж<ΔInt> Ꮡx, ж<ΔInt> Ꮡy, ж<ΔInt> Ꮡm) {
    return Ꮡz.exp(Ꮡx, Ꮡy, Ꮡm, true);
}

internal static ж<ΔInt> exp(this ж<ΔInt> Ꮡz, ж<ΔInt> Ꮡx, ж<ΔInt> Ꮡy, ж<ΔInt> Ꮡm, bool slow) {
    ref var z = ref Ꮡz.Value;
    ref var x = ref Ꮡx.Value;
    ref var y = ref Ꮡy.Value;
    ref var m = ref Ꮡm.DerefOrNil();

    // See Knuth, volume 2, section 4.6.3.
    var xWords = x.abs;
    if (y.neg) {
        if (Ꮡm == nil || len(m.abs) == 0) {
            return Ꮡz.SetInt64(1);
        }
        // for y < 0: x**y mod m == (x**(-1))**|y| mod m
        var inverse = @new<ΔInt>().ModInverse(Ꮡx, Ꮡm);
        if (inverse == nil) {
            return default!;
        }
        xWords = inverse.Value.abs;
    }
    var yWords = y.abs;
    nat mWords = default!;
    if (Ꮡm != nil) {
        if (Ꮡz == Ꮡm || alias(z.abs, m.abs)) {
            Ꮡm = @new<ΔInt>().Set(Ꮡm); m = ref Ꮡm.DerefOrNil();
        }
        mWords = m.abs;
    }
    // m.abs may be nil for m == 0
    z.abs = z.abs.expNN(xWords, yWords, mWords, slow);
    z.neg = len(z.abs) > 0 && x.neg && len(yWords) > 0 && (Word)(yWords[0] & 1) == 1;
    // 0 has no sign
    if (z.neg && len(mWords) > 0) {
        // make modulus result positive
        z.abs = z.abs.sub(mWords, z.abs);
        // z == x**y mod |m| && 0 <= z < |m|
        z.neg = false;
    }
    return Ꮡz;
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
public static ж<ΔInt> GCD(this ж<ΔInt> Ꮡz, ж<ΔInt> Ꮡx, ж<ΔInt> Ꮡy, ж<ΔInt> Ꮡa, ж<ΔInt> Ꮡb) {
    ref var z = ref Ꮡz.Value;
    ref var x = ref Ꮡx.DerefOrNil();
    ref var y = ref Ꮡy.DerefOrNil();
    ref var a = ref Ꮡa.Value;
    ref var b = ref Ꮡb.Value;

    if (len(a.abs) == 0 || len(b.abs) == 0) {
        nint lenA = len(a.abs);
        nint lenB = len(b.abs);
        var negA = a.neg;
        var negB = b.neg;
        if (lenA == 0){
            Ꮡz.Set(Ꮡb);
        } else {
            Ꮡz.Set(Ꮡa);
        }
        z.neg = false;
        if (Ꮡx != nil) {
            if (lenA == 0){
                Ꮡx.SetUint64(0);
            } else {
                Ꮡx.SetUint64(1);
                x.neg = negA;
            }
        }
        if (Ꮡy != nil) {
            if (lenB == 0){
                Ꮡy.SetUint64(0);
            } else {
                Ꮡy.SetUint64(1);
                y.neg = negB;
            }
        }
        return Ꮡz;
    }
    return Ꮡz.lehmerGCD(Ꮡx, Ꮡy, Ꮡa, Ꮡb);
}

// lehmerSimulate attempts to simulate several Euclidean update steps
// using the leading digits of A and B.  It returns u0, u1, v0, v1
// such that A and B can be updated as:
//
//	A = u0*A + v0*B
//	B = u1*A + v1*B
//
// Requirements: A >= B and len(B.abs) >= 2
// Since we are calculating with full words to avoid overflow,
// we use 'even' to track the sign of the cosequences.
// For even iterations: u0, v1 >= 0 && u1, v0 <= 0
// For odd  iterations: u0, v1 <= 0 && u1, v0 >= 0
internal static (Word u0, Word u1, Word v0, Word v1, bool even) lehmerSimulate(ж<ΔInt> ᏑA, ж<ΔInt> ᏑB) {
    Word u0 = default!;
    Word u1 = default!;
    Word v0 = default!;
    Word v1 = default!;
    bool even = default!;

    ref var A = ref ᏑA.Value;
    ref var B = ref ᏑB.Value;
    // initialize the digits
    Word a1 = default!;
    Word a2 = default!;
    Word u2 = default!;
    Word v2 = default!;
    nint m = len(B.abs);
    // m >= 2
    nint n = len(A.abs);
    // n >= m >= 2
    // extract the top Word of bits from A and B
    nuint h = nlz(A.abs[n - 1]);
    a1 = (Word)((A.abs[n - 1] << (int)(h)) | (A.abs[n - 2] >> (int)(((nuint)_W - h))));
    // B may have implicit zero words in the high bits if the lengths differ
    switch (ᐧ) {
    case {} when n == m: {
        a2 = (Word)((B.abs[n - 1] << (int)(h)) | (B.abs[n - 2] >> (int)(((nuint)_W - h))));
        break;
    }
    case {} when n == m + 1: {
        a2 = (B.abs[n - 2] >> (int)(((nuint)_W - h)));
        break;
    }
    default: {
        a2 = 0;
        break;
    }}

    // Since we are calculating with full words to avoid overflow,
    // we use 'even' to track the sign of the cosequences.
    // For even iterations: u0, v1 >= 0 && u1, v0 <= 0
    // For odd  iterations: u0, v1 <= 0 && u1, v0 >= 0
    // The first iteration starts with k=1 (odd).
    even = false;
    // variables to track the cosequences
    (u0, u1, u2) = (0, 1, 0);
    (v0, v1, v2) = (0, 0, 1);
    // Calculate the quotient and cosequences using Collins' stopping condition.
    // Note that overflow of a Word is not possible when computing the remainder
    // sequence and cosequences since the cosequence size is bounded by the input size.
    // See section 4.2 of Jebelean for details.
    while (a2 >= v2 && a1 - a2 >= v1 + v2) {
        Word q = a1 / a2;
        Word r = a1 % a2;
        (a1, a2) = (a2, r);
        (u0, u1, u2) = (u1, u2, u1 + q * u2);
        (v0, v1, v2) = (v1, v2, v1 + q * v2);
        even = !even;
    }
    return (u0, u1, v0, v1, even);
}

// lehmerUpdate updates the inputs A and B such that:
//
//	A = u0*A + v0*B
//	B = u1*A + v1*B
//
// where the signs of u0, u1, v0, v1 are given by even
// For even == true: u0, v1 >= 0 && u1, v0 <= 0
// For even == false: u0, v1 <= 0 && u1, v0 >= 0
// q, r, s, t are temporary variables to avoid allocations in the multiplication.
internal static void lehmerUpdate(ж<ΔInt> ᏑA, ж<ΔInt> ᏑB, ж<ΔInt> Ꮡq, ж<ΔInt> Ꮡr, ж<ΔInt> Ꮡs, ж<ΔInt> Ꮡt, Word u0, Word u1, Word v0, Word v1, bool even) {
    ref var q = ref Ꮡq.Value;
    ref var r = ref Ꮡr.Value;
    ref var s = ref Ꮡs.Value;
    ref var t = ref Ꮡt.Value;

    t.abs = t.abs.setWord(u0);
    s.abs = s.abs.setWord(v0);
    t.neg = !even;
    s.neg = even;
    Ꮡt.Mul(ᏑA, Ꮡt);
    Ꮡs.Mul(ᏑB, Ꮡs);
    r.abs = r.abs.setWord(u1);
    q.abs = q.abs.setWord(v1);
    r.neg = even;
    q.neg = !even;
    Ꮡr.Mul(ᏑA, Ꮡr);
    Ꮡq.Mul(ᏑB, Ꮡq);
    ᏑA.Add(Ꮡt, Ꮡs);
    ᏑB.Add(Ꮡr, Ꮡq);
}

// euclidUpdate performs a single step of the Euclidean GCD algorithm
// if extended is true, it also updates the cosequence Ua, Ub.
internal static void euclidUpdate(ж<ΔInt> ᏑA, ж<ΔInt> ᏑB, ж<ΔInt> ᏑUa, ж<ΔInt> ᏑUb, ж<ΔInt> Ꮡq, ж<ΔInt> Ꮡr, ж<ΔInt> Ꮡs, ж<ΔInt> Ꮡt, bool extended) {
    ref var A = ref ᏑA.Value;
    ref var B = ref ᏑB.Value;
    ref var Ua = ref ᏑUa.Value;
    ref var Ub = ref ᏑUb.Value;
    ref var q = ref Ꮡq.Value;
    ref var r = ref Ꮡr.Value;

    (Ꮡq, Ꮡr) = Ꮡq.QuoRem(ᏑA, ᏑB, Ꮡr); q = ref Ꮡq.Value; r = ref Ꮡr.Value;
    (A, B, r) = (B, r, A);
    if (extended) {
        // Ua, Ub = Ub, Ua - q*Ub
        Ꮡt.Set(ᏑUb);
        Ꮡs.Mul(ᏑUb, Ꮡq);
        ᏑUb.Sub(ᏑUa, Ꮡs);
        ᏑUa.Set(Ꮡt);
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
internal static ж<ΔInt> lehmerGCD(this ж<ΔInt> Ꮡz, ж<ΔInt> Ꮡx, ж<ΔInt> Ꮡy, ж<ΔInt> Ꮡa, ж<ΔInt> Ꮡb) {
    ref var z = ref Ꮡz.Value;
    ref var x = ref Ꮡx.DerefOrNil();
    ref var y = ref Ꮡy.DerefOrNil();
    ref var a = ref Ꮡa.Value;
    ref var b = ref Ꮡb.DerefOrNil();

    ж<ΔInt> A = default!;
    ж<ΔInt> B = default!;
    ж<ΔInt> Ua = default!;
    ж<ΔInt> Ub = default!;
    A = @new<ΔInt>().Abs(Ꮡa);
    B = @new<ΔInt>().Abs(Ꮡb);
    var extended = Ꮡx != nil || Ꮡy != nil;
    if (extended) {
        // Ua (Ub) tracks how many times input a has been accumulated into A (B).
        Ua = @new<ΔInt>().SetInt64(1);
        Ub = @new<ΔInt>();
    }
    // temp variables for multiprecision update
    var q = @new<ΔInt>();
    var r = @new<ΔInt>();
    var s = @new<ΔInt>();
    var t = @new<ΔInt>();
    // ensure A >= B
    if ((~A).abs.cmp((~B).abs) < 0) {
        (A, B) = (B, A);
        (Ub, Ua) = (Ua, Ub);
    }
    // loop invariant A >= B
    while (len((~B).abs) > 1) {
        // Attempt to calculate in single-precision using leading words of A and B.
        var (u0, u1, v0, v1, even) = lehmerSimulate(A, B);
        // multiprecision Step
        if (v0 != 0){
            // Simulate the effect of the single-precision steps using the cosequences.
            // A = u0*A + v0*B
            // B = u1*A + v1*B
            lehmerUpdate(A, B, q, r, s, t, u0, u1, v0, v1, even);
            if (extended) {
                // Ua = u0*Ua + v0*Ub
                // Ub = u1*Ua + v1*Ub
                lehmerUpdate(Ua, Ub, q, r, s, t, u0, u1, v0, v1, even);
            }
        } else {
            // Single-digit calculations failed to simulate any quotients.
            // Do a standard Euclidean step.
            euclidUpdate(A, B, Ua, Ub, q, r, s, t, extended);
        }
    }
    if (len((~B).abs) > 0) {
        // extended Euclidean algorithm base case if B is a single Word
        if (len((~A).abs) > 1) {
            // A is longer than a single Word, so one update is needed.
            euclidUpdate(A, B, Ua, Ub, q, r, s, t, extended);
        }
        if (len((~B).abs) > 0) {
            // A and B are both a single Word.
            Word aWord = (~A).abs[0];
            Word bWord = (~B).abs[0];
            if (extended){
                Word ua = default!;
                Word ub = default!;
                Word va = default!;
                Word vb = default!;
                (ua, ub) = (1, 0);
                (va, vb) = (0, 1);
                var even = true;
                while (bWord != 0) {
                    Word qΔ1 = aWord / bWord;
                    Word rΔ1 = aWord % bWord;
                    (aWord, bWord) = (bWord, rΔ1);
                    (ua, ub) = (ub, ua + qΔ1 * ub);
                    (va, vb) = (vb, va + qΔ1 * vb);
                    even = !even;
                }
                t.Value.abs = (~t).abs.setWord(ua);
                s.Value.abs = (~s).abs.setWord(va);
                t.Value.neg = !even;
                s.Value.neg = even;
                t.Mul(Ua, t);
                s.Mul(Ub, s);
                Ua.Add(t, s);
            } else {
                while (bWord != 0) {
                    (aWord, bWord) = (bWord, aWord % bWord);
                }
            }
            A.Value.abs[0] = aWord;
        }
    }
    var negA = a.neg;
    if (Ꮡy != nil) {
        // avoid aliasing b needed in the division below
        if (Ꮡy == Ꮡb){
            B.Set(Ꮡb);
        } else {
            B = Ꮡb;
        }
        // y = (z - a*x)/b
        Ꮡy.Mul(Ꮡa, Ua);
        // y can safely alias a
        if (negA) {
            y.neg = !y.neg;
        }
        Ꮡy.Sub(A, Ꮡy);
        Ꮡy.Div(Ꮡy, B);
    }
    if (Ꮡx != nil) {
        x = Ua.Value;
        if (negA) {
            x.neg = !x.neg;
        }
    }
    z = A.Value;
    return Ꮡz;
}

// Rand sets z to a pseudo-random number in [0, n) and returns z.
//
// As this uses the [math/rand] package, it must not be used for
// security-sensitive work. Use [crypto/rand.Int] instead.
public static ж<ΔInt> Rand(this ж<ΔInt> Ꮡz, ж<rand.Rand> Ꮡrnd, ж<ΔInt> Ꮡn) {
    ref var z = ref Ꮡz.Value;
    ref var n = ref Ꮡn.Value;

    // z.neg is not modified before the if check, because z and n might alias.
    if (n.neg || len(n.abs) == 0) {
        z.neg = false;
        z.abs = default!;
        return Ꮡz;
    }
    z.neg = false;
    z.abs = z.abs.random(Ꮡrnd, n.abs, n.abs.bitLen());
    return Ꮡz;
}

// ModInverse sets z to the multiplicative inverse of g in the ring ℤ/nℤ
// and returns z. If g and n are not relatively prime, g has no multiplicative
// inverse in the ring ℤ/nℤ.  In this case, z is unchanged and the return value
// is nil. If n == 0, a division-by-zero run-time panic occurs.
public static ж<ΔInt> ModInverse(this ж<ΔInt> Ꮡz, ж<ΔInt> Ꮡg, ж<ΔInt> Ꮡn) {
    ref var z = ref Ꮡz.Value;
    ref var g = ref Ꮡg.Value;
    ref var n = ref Ꮡn.Value;

    // GCD expects parameters a and b to be > 0.
    if (n.neg) {
        ref var n2 = ref heap(new ΔInt(), out var Ꮡn2);
        Ꮡn = Ꮡn2.Neg(Ꮡn); n = ref Ꮡn.Value;
    }
    if (g.neg) {
        ref var g2 = ref heap(new ΔInt(), out var Ꮡg2);
        Ꮡg = Ꮡg2.Mod(Ꮡg, Ꮡn); g = ref Ꮡg.Value;
    }
    ref var d = ref heap(new ΔInt(), out var Ꮡd);
    ref var x = ref heap(new ΔInt(), out var Ꮡx);
    Ꮡd.GCD(Ꮡx, nil, Ꮡg, Ꮡn);
    // if and only if d==1, g and n are relatively prime
    if (Ꮡd.Cmp(intOne) != 0) {
        return default!;
    }
    // x and y are such that g*x + n*y = 1, therefore x is the inverse element,
    // but it may be negative, so convert to the range 0 <= z < |n|
    if (x.neg){
        Ꮡz.Add(Ꮡx, Ꮡn);
    } else {
        Ꮡz.Set(Ꮡx);
    }
    return Ꮡz;
}

internal static nat modInverse(this nat z, nat g, nat n) {
    // TODO(rsc): ModInverse should be implemented in terms of this function.
    return (~(Ꮡ(new ΔInt(abs: z))).ModInverse(Ꮡ(new ΔInt(abs: g)), Ꮡ(new ΔInt(abs: n)))).abs;
}

// Jacobi returns the Jacobi symbol (x/y), either +1, -1, or 0.
// The y argument must be an odd integer.
public static nint Jacobi(ж<ΔInt> Ꮡx, ж<ΔInt> Ꮡy) {
    ref var y = ref Ꮡy.Value;

    if (len(y.abs) == 0 || (Word)(y.abs[0] & 1) == 0) {
        throw panic(fmt.Sprintf("big: invalid 2nd argument to Int.Jacobi: need odd integer but got %s"u8, Ꮡy.String()));
    }
    // We use the formulation described in chapter 2, section 2.4,
    // "The Yacas Book of Algorithms":
    // http://yacas.sourceforge.net/Algo.book.pdf
    ref var a = ref heap(new ΔInt(), out var Ꮡa);
    ref var b = ref heap(new ΔInt(), out var Ꮡb);
    ref var c = ref heap(new ΔInt(), out var Ꮡc);
    Ꮡa.Set(Ꮡx);
    Ꮡb.Set(Ꮡy);
    nint j = 1;
    if (b.neg) {
        if (a.neg) {
            j = -1;
        }
        b.neg = false;
    }
    while (ᐧ) {
        if (Ꮡb.Cmp(intOne) == 0) {
            return j;
        }
        if (len(a.abs) == 0) {
            return 0;
        }
        Ꮡa.Mod(Ꮡa, Ꮡb);
        if (len(a.abs) == 0) {
            return 0;
        }
        // a > 0
        // handle factors of 2 in 'a'
        nuint s = a.abs.trailingZeroBits();
        if ((nuint)(s & 1) != 0) {
            Word bmod8 = (Word)(b.abs[0] & 7);
            if (bmod8 == 3 || bmod8 == 5) {
                j = -j;
            }
        }
        Ꮡc.Rsh(Ꮡa, s);
        // a = 2^s*c
        // swap numerator and denominator
        if ((Word)(b.abs[0] & 3) == 3 && (Word)(c.abs[0] & 3) == 3) {
            j = -j;
        }
        Ꮡa.Set(Ꮡb);
        Ꮡb.Set(Ꮡc);
    }
}

// modSqrt3Mod4 uses the identity
//
//	   (a^((p+1)/4))^2  mod p
//	== u^(p+1)          mod p
//	== u^2              mod p
//
// to calculate the square root of any quadratic residue mod p quickly for 3
// mod 4 primes.
internal static ж<ΔInt> modSqrt3Mod4Prime(this ж<ΔInt> Ꮡz, ж<ΔInt> Ꮡx, ж<ΔInt> Ꮡp) {
    ref var z = ref Ꮡz.Value;
    ref var x = ref Ꮡx.Value;
    ref var p = ref Ꮡp.Value;

    var e = @new<ΔInt>().Add(Ꮡp, intOne);
    // e = p + 1
    e.Rsh(e, 2);
    // e = (p + 1) / 4
    Ꮡz.Exp(Ꮡx, e, Ꮡp);
    // z = x^e mod p
    return Ꮡz;
}

// modSqrt5Mod8Prime uses Atkin's observation that 2 is not a square mod p
//
//	alpha ==  (2*a)^((p-5)/8)    mod p
//	beta  ==  2*a*alpha^2        mod p  is a square root of -1
//	b     ==  a*alpha*(beta-1)   mod p  is a square root of a
//
// to calculate the square root of any quadratic residue mod p quickly for 5
// mod 8 primes.
internal static ж<ΔInt> modSqrt5Mod8Prime(this ж<ΔInt> Ꮡz, ж<ΔInt> Ꮡx, ж<ΔInt> Ꮡp) {
    ref var x = ref Ꮡx.Value;
    ref var p = ref Ꮡp.Value;

    // p == 5 mod 8 implies p = e*8 + 5
    // e is the quotient and 5 the remainder on division by 8
    var e = @new<ΔInt>().Rsh(Ꮡp, 3);
    // e = (p - 5) / 8
    var tx = @new<ΔInt>().Lsh(Ꮡx, 1);
    // tx = 2*x
    var alpha = @new<ΔInt>().Exp(tx, e, Ꮡp);
    var beta = @new<ΔInt>().Mul(alpha, alpha);
    beta.Mod(beta, Ꮡp);
    beta.Mul(beta, tx);
    beta.Mod(beta, Ꮡp);
    beta.Sub(beta, intOne);
    beta.Mul(beta, Ꮡx);
    beta.Mod(beta, Ꮡp);
    beta.Mul(beta, alpha);
    Ꮡz.Mod(beta, Ꮡp);
    return Ꮡz;
}

// modSqrtTonelliShanks uses the Tonelli-Shanks algorithm to find the square
// root of a quadratic residue modulo any prime.
internal static ж<ΔInt> modSqrtTonelliShanks(this ж<ΔInt> Ꮡz, ж<ΔInt> Ꮡx, ж<ΔInt> Ꮡp) {
    ref var x = ref Ꮡx.Value;
    ref var p = ref Ꮡp.Value;

    // Break p-1 into s*2^e such that s is odd.
    ref var s = ref heap(new ΔInt(), out var Ꮡs);
    Ꮡs.Sub(Ꮡp, intOne);
    nuint e = s.abs.trailingZeroBits();
    Ꮡs.Rsh(Ꮡs, e);
    // find some non-square n
    ref var n = ref heap(new ΔInt(), out var Ꮡn);
    Ꮡn.SetInt64(2);
    while (Jacobi(Ꮡn, Ꮡp) != -1) {
        Ꮡn.Add(Ꮡn, intOne);
    }
    // Core of the Tonelli-Shanks algorithm. Follows the description in
    // section 6 of "Square roots from 1; 24, 51, 10 to Dan Shanks" by Ezra
    // Brown:
    // https://www.maa.org/sites/default/files/pdf/upload_library/22/Polya/07468342.di020786.02p0470a.pdf
    ref var y = ref heap(new ΔInt(), out var Ꮡy);
    ref var b = ref heap(new ΔInt(), out var Ꮡb);
    ref var g = ref heap(new ΔInt(), out var Ꮡg);
    ref var t = ref heap(new ΔInt(), out var Ꮡt);
    Ꮡy.Add(Ꮡs, intOne);
    Ꮡy.Rsh(Ꮡy, 1);
    Ꮡy.Exp(Ꮡx, Ꮡy, Ꮡp);
    // y = x^((s+1)/2)
    Ꮡb.Exp(Ꮡx, Ꮡs, Ꮡp);
    // b = x^s
    Ꮡg.Exp(Ꮡn, Ꮡs, Ꮡp);
    // g = n^s
    nuint r = e;
    while (ᐧ) {
        // find the least m such that ord_p(b) = 2^m
        nuint m = default!;
        Ꮡt.Set(Ꮡb);
        while (Ꮡt.Cmp(intOne) != 0) {
            Ꮡt.Mul(Ꮡt, Ꮡt).Mod(Ꮡt, Ꮡp);
            m++;
        }
        if (m == 0) {
            return Ꮡz.Set(Ꮡy);
        }
        Ꮡt.SetInt64(0).SetBit(Ꮡt, (nint)(r - m - 1), 1).Exp(Ꮡg, Ꮡt, Ꮡp);
        // t = g^(2^(r-m-1)) mod p
        Ꮡg.Mul(Ꮡt, Ꮡt).Mod(Ꮡg, Ꮡp);
        // g = g^(2^(r-m)) mod p
        Ꮡy.Mul(Ꮡy, Ꮡt).Mod(Ꮡy, Ꮡp);
        Ꮡb.Mul(Ꮡb, Ꮡg).Mod(Ꮡb, Ꮡp);
        r = m;
    }
}

// ModSqrt sets z to a square root of x mod p if such a square root exists, and
// returns z. The modulus p must be an odd prime. If x is not a square mod p,
// ModSqrt leaves z unchanged and returns nil. This function panics if p is
// not an odd integer, its behavior is undefined if p is odd but not prime.
public static ж<ΔInt> ModSqrt(this ж<ΔInt> Ꮡz, ж<ΔInt> Ꮡx, ж<ΔInt> Ꮡp) {
    ref var x = ref Ꮡx.Value;
    ref var p = ref Ꮡp.Value;

    var exprᴛ1 = Jacobi(Ꮡx, Ꮡp);
    if (exprᴛ1 == -1) {
        return default!;
    }
    if (exprᴛ1 is 0) {
        return Ꮡz.SetInt64(0);
    }
    if (exprᴛ1 is 1) {
        do {
            break;
        } while (false);
    }

    // x is not a square mod p
    // sqrt(0) mod p = 0
    if (x.neg || Ꮡx.Cmp(Ꮡp) >= 0) {
        // ensure 0 <= x < p
        Ꮡx = @new<ΔInt>().Mod(Ꮡx, Ꮡp); x = ref Ꮡx.Value;
    }
    switch (ᐧ) {
    case {} when p.abs[0] % 4 == 3: {
        return Ꮡz.modSqrt3Mod4Prime(Ꮡx, // Check whether p is 3 mod 4, and if so, use the faster algorithm.
 Ꮡp);
    }
    case {} when p.abs[0] % 8 == 5: {
        return Ꮡz.modSqrt5Mod8Prime(Ꮡx, // Check whether p is 5 mod 8, use Atkin's algorithm.
 Ꮡp);
    }
    default: {
        return Ꮡz.modSqrtTonelliShanks(Ꮡx, // Otherwise, use Tonelli-Shanks.
 Ꮡp);
    }}

}

// Lsh sets z = x << n and returns z.
public static ж<ΔInt> Lsh(this ж<ΔInt> Ꮡz, ж<ΔInt> Ꮡx, nuint n) {
    ref var z = ref Ꮡz.Value;
    ref var x = ref Ꮡx.Value;

    z.abs = z.abs.shl(x.abs, n);
    z.neg = x.neg;
    return Ꮡz;
}

// Rsh sets z = x >> n and returns z.
public static ж<ΔInt> Rsh(this ж<ΔInt> Ꮡz, ж<ΔInt> Ꮡx, nuint n) {
    ref var z = ref Ꮡz.Value;
    ref var x = ref Ꮡx.Value;

    if (x.neg) {
        // (-x) >> s == ^(x-1) >> s == ^((x-1) >> s) == -(((x-1) >> s) + 1)
        var t = z.abs.sub(x.abs, natOne);
        // no underflow because |x| > 0
        t = t.shr(t, n);
        z.abs = t.add(t, natOne);
        z.neg = true;
        // z cannot be zero if x is negative
        return Ꮡz;
    }
    z.abs = z.abs.shr(x.abs, n);
    z.neg = false;
    return Ꮡz;
}

// Bit returns the value of the i'th bit of x. That is, it
// returns (x>>i)&1. The bit index i must be >= 0.
[GoRecv] public static nuint Bit(this ref ΔInt x, nint i) {
    if (i == 0) {
        // optimization for common case: odd/even test of x
        if (len(x.abs) > 0) {
            return (nuint)((Word)(x.abs[0] & 1));
        }
        // bit 0 is same for -x
        return 0;
    }
    if (i < 0) {
        throw panic("negative bit index");
    }
    if (x.neg) {
        var t = ((nat)default!).sub(x.abs, natOne);
        return (nuint)(t.bit((nuint)i) ^ 1);
    }
    return x.abs.bit((nuint)i);
}

// SetBit sets z to x, with x's i'th bit set to b (0 or 1).
// That is,
//   - if b is 1, SetBit sets z = x | (1 << i);
//   - if b is 0, SetBit sets z = x &^ (1 << i);
//   - if b is not 0 or 1, SetBit will panic.
public static ж<ΔInt> SetBit(this ж<ΔInt> Ꮡz, ж<ΔInt> Ꮡx, nint i, nuint b) {
    ref var z = ref Ꮡz.Value;
    ref var x = ref Ꮡx.Value;

    if (i < 0) {
        throw panic("negative bit index");
    }
    if (x.neg) {
        var t = z.abs.sub(x.abs, natOne);
        t = t.setBit(t, (nuint)i, (nuint)(b ^ 1));
        z.abs = t.add(t, natOne);
        z.neg = len(z.abs) > 0;
        return Ꮡz;
    }
    z.abs = z.abs.setBit(x.abs, (nuint)i, b);
    z.neg = false;
    return Ꮡz;
}

// And sets z = x & y and returns z.
public static ж<ΔInt> And(this ж<ΔInt> Ꮡz, ж<ΔInt> Ꮡx, ж<ΔInt> Ꮡy) {
    ref var z = ref Ꮡz.Value;
    ref var x = ref Ꮡx.Value;
    ref var y = ref Ꮡy.Value;

    if (x.neg == y.neg) {
        if (x.neg) {
            // (-x) & (-y) == ^(x-1) & ^(y-1) == ^((x-1) | (y-1)) == -(((x-1) | (y-1)) + 1)
            var x1 = ((nat)default!).sub(x.abs, natOne);
            var y1Δ1 = ((nat)default!).sub(y.abs, natOne);
            z.abs = z.abs.add(z.abs.or(x1, y1Δ1), natOne);
            z.neg = true;
            // z cannot be zero if x and y are negative
            return Ꮡz;
        }
        // x & y == x & y
        z.abs = z.abs.and(x.abs, y.abs);
        z.neg = false;
        return Ꮡz;
    }
    // x.neg != y.neg
    if (x.neg) {
        (Ꮡx, Ꮡy) = (Ꮡy, Ꮡx); x = ref Ꮡx.Value; y = ref Ꮡy.Value;
    }
    // & is symmetric
    // x & (-y) == x & ^(y-1) == x &^ (y-1)
    var y1 = ((nat)default!).sub(y.abs, natOne);
    z.abs = z.abs.andNot(x.abs, y1);
    z.neg = false;
    return Ꮡz;
}

// AndNot sets z = x &^ y and returns z.
public static ж<ΔInt> AndNot(this ж<ΔInt> Ꮡz, ж<ΔInt> Ꮡx, ж<ΔInt> Ꮡy) {
    ref var z = ref Ꮡz.Value;
    ref var x = ref Ꮡx.Value;
    ref var y = ref Ꮡy.Value;

    if (x.neg == y.neg) {
        if (x.neg) {
            // (-x) &^ (-y) == ^(x-1) &^ ^(y-1) == ^(x-1) & (y-1) == (y-1) &^ (x-1)
            var x1 = ((nat)default!).sub(x.abs, natOne);
            var y1Δ1 = ((nat)default!).sub(y.abs, natOne);
            z.abs = z.abs.andNot(y1Δ1, x1);
            z.neg = false;
            return Ꮡz;
        }
        // x &^ y == x &^ y
        z.abs = z.abs.andNot(x.abs, y.abs);
        z.neg = false;
        return Ꮡz;
    }
    if (x.neg) {
        // (-x) &^ y == ^(x-1) &^ y == ^(x-1) & ^y == ^((x-1) | y) == -(((x-1) | y) + 1)
        var x1 = ((nat)default!).sub(x.abs, natOne);
        z.abs = z.abs.add(z.abs.or(x1, y.abs), natOne);
        z.neg = true;
        // z cannot be zero if x is negative and y is positive
        return Ꮡz;
    }
    // x &^ (-y) == x &^ ^(y-1) == x & (y-1)
    var y1 = ((nat)default!).sub(y.abs, natOne);
    z.abs = z.abs.and(x.abs, y1);
    z.neg = false;
    return Ꮡz;
}

// Or sets z = x | y and returns z.
public static ж<ΔInt> Or(this ж<ΔInt> Ꮡz, ж<ΔInt> Ꮡx, ж<ΔInt> Ꮡy) {
    ref var z = ref Ꮡz.Value;
    ref var x = ref Ꮡx.Value;
    ref var y = ref Ꮡy.Value;

    if (x.neg == y.neg) {
        if (x.neg) {
            // (-x) | (-y) == ^(x-1) | ^(y-1) == ^((x-1) & (y-1)) == -(((x-1) & (y-1)) + 1)
            var x1 = ((nat)default!).sub(x.abs, natOne);
            var y1Δ1 = ((nat)default!).sub(y.abs, natOne);
            z.abs = z.abs.add(z.abs.and(x1, y1Δ1), natOne);
            z.neg = true;
            // z cannot be zero if x and y are negative
            return Ꮡz;
        }
        // x | y == x | y
        z.abs = z.abs.or(x.abs, y.abs);
        z.neg = false;
        return Ꮡz;
    }
    // x.neg != y.neg
    if (x.neg) {
        (Ꮡx, Ꮡy) = (Ꮡy, Ꮡx); x = ref Ꮡx.Value; y = ref Ꮡy.Value;
    }
    // | is symmetric
    // x | (-y) == x | ^(y-1) == ^((y-1) &^ x) == -(^((y-1) &^ x) + 1)
    var y1 = ((nat)default!).sub(y.abs, natOne);
    z.abs = z.abs.add(z.abs.andNot(y1, x.abs), natOne);
    z.neg = true;
    // z cannot be zero if one of x or y is negative
    return Ꮡz;
}

// Xor sets z = x ^ y and returns z.
public static ж<ΔInt> Xor(this ж<ΔInt> Ꮡz, ж<ΔInt> Ꮡx, ж<ΔInt> Ꮡy) {
    ref var z = ref Ꮡz.Value;
    ref var x = ref Ꮡx.Value;
    ref var y = ref Ꮡy.Value;

    if (x.neg == y.neg) {
        if (x.neg) {
            // (-x) ^ (-y) == ^(x-1) ^ ^(y-1) == (x-1) ^ (y-1)
            var x1 = ((nat)default!).sub(x.abs, natOne);
            var y1Δ1 = ((nat)default!).sub(y.abs, natOne);
            z.abs = z.abs.xor(x1, y1Δ1);
            z.neg = false;
            return Ꮡz;
        }
        // x ^ y == x ^ y
        z.abs = z.abs.xor(x.abs, y.abs);
        z.neg = false;
        return Ꮡz;
    }
    // x.neg != y.neg
    if (x.neg) {
        (Ꮡx, Ꮡy) = (Ꮡy, Ꮡx); x = ref Ꮡx.Value; y = ref Ꮡy.Value;
    }
    // ^ is symmetric
    // x ^ (-y) == x ^ ^(y-1) == ^(x ^ (y-1)) == -((x ^ (y-1)) + 1)
    var y1 = ((nat)default!).sub(y.abs, natOne);
    z.abs = z.abs.add(z.abs.xor(x.abs, y1), natOne);
    z.neg = true;
    // z cannot be zero if only one of x or y is negative
    return Ꮡz;
}

// Not sets z = ^x and returns z.
public static ж<ΔInt> Not(this ж<ΔInt> Ꮡz, ж<ΔInt> Ꮡx) {
    ref var z = ref Ꮡz.Value;
    ref var x = ref Ꮡx.Value;

    if (x.neg) {
        // ^(-x) == ^(^(x-1)) == x-1
        z.abs = z.abs.sub(x.abs, natOne);
        z.neg = false;
        return Ꮡz;
    }
    // ^x == -x-1 == -(x+1)
    z.abs = z.abs.add(x.abs, natOne);
    z.neg = true;
    // z cannot be zero if x is positive
    return Ꮡz;
}

// Sqrt sets z to ⌊√x⌋, the largest integer such that z² ≤ x, and returns z.
// It panics if x is negative.
public static ж<ΔInt> Sqrt(this ж<ΔInt> Ꮡz, ж<ΔInt> Ꮡx) {
    ref var z = ref Ꮡz.Value;
    ref var x = ref Ꮡx.Value;

    if (x.neg) {
        throw panic("square root of negative number");
    }
    z.neg = false;
    z.abs = z.abs.sqrt(x.abs);
    return Ꮡz;
}

} // end big_package
