// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 13 06:01:38 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\magic.go
namespace go.cmd.compile.@internal;

using big = math.big_package;
using bits = math.bits_package;


// So you want to compute x / c for some constant c?
// Machine division instructions are slow, so we try to
// compute this division with a multiplication + a few
// other cheap instructions instead.
// (We assume here that c != 0, +/- 1, or +/- 2^i.  Those
// cases are easy to handle in different ways).

// Technique from https://gmplib.org/~tege/divcnst-pldi94.pdf

// First consider unsigned division.
// Our strategy is to precompute 1/c then do
//   ⎣x / c⎦ = ⎣x * (1/c)⎦.
// 1/c is less than 1, so we can't compute it directly in
// integer arithmetic.  Let's instead compute 2^e/c
// for a value of e TBD (^ = exponentiation).  Then
//   ⎣x / c⎦ = ⎣x * (2^e/c) / 2^e⎦.
// Dividing by 2^e is easy.  2^e/c isn't an integer, unfortunately.
// So we must approximate it.  Let's call its approximation m.
// We'll then compute
//   ⎣x * m / 2^e⎦
// Which we want to be equal to ⎣x / c⎦ for 0 <= x < 2^n-1
// where n is the word size.
// Setting x = c gives us c * m >= 2^e.
// We'll chose m = ⎡2^e/c⎤ to satisfy that equation.
// What remains is to choose e.
// Let m = 2^e/c + delta, 0 <= delta < 1
//   ⎣x * (2^e/c + delta) / 2^e⎦
//   ⎣x / c + x * delta / 2^e⎦
// We must have x * delta / 2^e < 1/c so that this
// additional term never rounds differently than ⎣x / c⎦ does.
// Rearranging,
//   2^e > x * delta * c
// x can be at most 2^n-1 and delta can be at most 1.
// So it is sufficient to have 2^e >= 2^n*c.
// So we'll choose e = n + s, with s = ⎡log2(c)⎤.
//
// An additional complication arises because m has n+1 bits in it.
// Hardware restricts us to n bit by n bit multiplies.
// We divide into 3 cases:
//
// Case 1: m is even.
//   ⎣x / c⎦ = ⎣x * m / 2^(n+s)⎦
//   ⎣x / c⎦ = ⎣x * (m/2) / 2^(n+s-1)⎦
//   ⎣x / c⎦ = ⎣x * (m/2) / 2^n / 2^(s-1)⎦
//   ⎣x / c⎦ = ⎣⎣x * (m/2) / 2^n⎦ / 2^(s-1)⎦
//   multiply + shift
//
// Case 2: c is even.
//   ⎣x / c⎦ = ⎣(x/2) / (c/2)⎦
//   ⎣x / c⎦ = ⎣⎣x/2⎦ / (c/2)⎦
//     This is just the original problem, with x' = ⎣x/2⎦, c' = c/2, n' = n-1.
//       s' = s-1
//       m' = ⎡2^(n'+s')/c'⎤
//          = ⎡2^(n+s-1)/c⎤
//          = ⎡m/2⎤
//   ⎣x / c⎦ = ⎣x' * m' / 2^(n'+s')⎦
//   ⎣x / c⎦ = ⎣⎣x/2⎦ * ⎡m/2⎤ / 2^(n+s-2)⎦
//   ⎣x / c⎦ = ⎣⎣⎣x/2⎦ * ⎡m/2⎤ / 2^n⎦ / 2^(s-2)⎦
//   shift + multiply + shift
//
// Case 3: everything else
//   let k = m - 2^n. k fits in n bits.
//   ⎣x / c⎦ = ⎣x * m / 2^(n+s)⎦
//   ⎣x / c⎦ = ⎣x * (2^n + k) / 2^(n+s)⎦
//   ⎣x / c⎦ = ⎣(x + x * k / 2^n) / 2^s⎦
//   ⎣x / c⎦ = ⎣(x + ⎣x * k / 2^n⎦) / 2^s⎦
//   ⎣x / c⎦ = ⎣(x + ⎣x * k / 2^n⎦) / 2^s⎦
//   ⎣x / c⎦ = ⎣⎣(x + ⎣x * k / 2^n⎦) / 2⎦ / 2^(s-1)⎦
//   multiply + avg + shift
//
// These can be implemented in hardware using:
//  ⎣a * b / 2^n⎦ - aka high n bits of an n-bit by n-bit multiply.
//  ⎣(a+b) / 2⎦   - aka "average" of two n-bit numbers.
//                  (Not just a regular add & shift because the intermediate result
//                   a+b has n+1 bits in it.  Nevertheless, can be done
//                   in 2 instructions on x86.)

// umagicOK reports whether we should strength reduce a n-bit divide by c.

public static partial class ssa_package {

private static bool umagicOK(nuint n, long c) { 
    // Convert from ConstX auxint values to the real uint64 constant they represent.
    var d = uint64(c) << (int)((64 - n)) >> (int)((64 - n)); 

    // Doesn't work for 0.
    // Don't use for powers of 2.
    return d & (d - 1) != 0;
}

// umagicOKn reports whether we should strength reduce an unsigned n-bit divide by c.
// We can strength reduce when c != 0 and c is not a power of two.
private static bool umagicOK8(sbyte c) {
    return c & (c - 1) != 0;
}
private static bool umagicOK16(short c) {
    return c & (c - 1) != 0;
}
private static bool umagicOK32(int c) {
    return c & (c - 1) != 0;
}
private static bool umagicOK64(long c) {
    return c & (c - 1) != 0;
}

private partial struct umagicData {
    public long s; // ⎡log2(c)⎤
    public ulong m; // ⎡2^(n+s)/c⎤ - 2^n
}

// umagic computes the constants needed to strength reduce unsigned n-bit divides by the constant uint64(c).
// The return values satisfy for all 0 <= x < 2^n
//  floor(x / uint64(c)) = x * (m + 2^n) >> (n+s)
private static umagicData umagic(nuint n, long c) => func((_, panic, _) => { 
    // Convert from ConstX auxint values to the real uint64 constant they represent.
    var d = uint64(c) << (int)((64 - n)) >> (int)((64 - n));

    ptr<object> C = @new<big.Int>().SetUint64(d);
    var s = C.BitLen();
    var M = big.NewInt(1);
    M.Lsh(M, n + uint(s)); // 2^(n+s)
    M.Add(M, C); // 2^(n+s)+c
    M.Sub(M, big.NewInt(1)); // 2^(n+s)+c-1
    M.Div(M, C); // ⎡2^(n+s)/c⎤
    if (M.Bit(int(n)) != 1) {
        panic("n+1st bit isn't set");
    }
    M.SetBit(M, int(n), 0);
    var m = M.Uint64();
    return new umagicData(s:int64(s),m:m);
});

private static umagicData umagic8(sbyte c) {
    return umagic(8, int64(c));
}
private static umagicData umagic16(short c) {
    return umagic(16, int64(c));
}
private static umagicData umagic32(int c) {
    return umagic(32, int64(c));
}
private static umagicData umagic64(long c) {
    return umagic(64, c);
}

// For signed division, we use a similar strategy.
// First, we enforce a positive c.
//   x / c = -(x / (-c))
// This will require an additional Neg op for c<0.
//
// If x is positive we're in a very similar state
// to the unsigned case above.  We define:
//   s = ⎡log2(c)⎤-1
//   m = ⎡2^(n+s)/c⎤
// Then
//   ⎣x / c⎦ = ⎣x * m / 2^(n+s)⎦
// If x is negative we have
//   ⎡x / c⎤ = ⎣x * m / 2^(n+s)⎦ + 1
// (TODO: derivation?)
//
// The multiply is a bit odd, as it is a signed n-bit value
// times an unsigned n-bit value.  For n smaller than the
// word size, we can extend x and m appropriately and use the
// signed multiply instruction.  For n == word size,
// we must use the signed multiply high and correct
// the result by adding x*2^n.
//
// Adding 1 if x<0 is done by subtracting x>>(n-1).

private static bool smagicOK(nuint n, long c) {
    if (c < 0) { 
        // Doesn't work for negative c.
        return false;
    }
    return c & (c - 1) != 0;
}

// smagicOKn reports whether we should strength reduce an signed n-bit divide by c.
private static bool smagicOK8(sbyte c) {
    return smagicOK(8, int64(c));
}
private static bool smagicOK16(short c) {
    return smagicOK(16, int64(c));
}
private static bool smagicOK32(int c) {
    return smagicOK(32, int64(c));
}
private static bool smagicOK64(long c) {
    return smagicOK(64, c);
}

private partial struct smagicData {
    public long s; // ⎡log2(c)⎤-1
    public ulong m; // ⎡2^(n+s)/c⎤
}

// magic computes the constants needed to strength reduce signed n-bit divides by the constant c.
// Must have c>0.
// The return values satisfy for all -2^(n-1) <= x < 2^(n-1)
//  trunc(x / c) = x * m >> (n+s) + (x < 0 ? 1 : 0)
private static smagicData smagic(nuint n, long c) => func((_, panic, _) => {
    ptr<object> C = @new<big.Int>().SetInt64(c);
    var s = C.BitLen() - 1;
    var M = big.NewInt(1);
    M.Lsh(M, n + uint(s)); // 2^(n+s)
    M.Add(M, C); // 2^(n+s)+c
    M.Sub(M, big.NewInt(1)); // 2^(n+s)+c-1
    M.Div(M, C); // ⎡2^(n+s)/c⎤
    if (M.Bit(int(n)) != 0) {
        panic("n+1st bit is set");
    }
    if (M.Bit(int(n - 1)) == 0) {
        panic("nth bit is not set");
    }
    var m = M.Uint64();
    return new smagicData(s:int64(s),m:m);
});

private static smagicData smagic8(sbyte c) {
    return smagic(8, int64(c));
}
private static smagicData smagic16(short c) {
    return smagic(16, int64(c));
}
private static smagicData smagic32(int c) {
    return smagic(32, int64(c));
}
private static smagicData smagic64(long c) {
    return smagic(64, c);
}

// Divisibility x%c == 0 can be checked more efficiently than directly computing
// the modulus x%c and comparing against 0.
//
// The same "Division by invariant integers using multiplication" paper
// by Granlund and Montgomery referenced above briefly mentions this method
// and it is further elaborated in "Hacker's Delight" by Warren Section 10-17
//
// The first thing to note is that for odd integers, exact division can be computed
// by using the modular inverse with respect to the word size 2^n.
//
// Given c, compute m such that (c * m) mod 2^n == 1
// Then if c divides x (x%c ==0), the quotient is given by q = x/c == x*m mod 2^n
//
// x can range from 0, c, 2c, 3c, ... ⎣(2^n - 1)/c⎦ * c the maximum multiple
// Thus, x*m mod 2^n is 0, 1, 2, 3, ... ⎣(2^n - 1)/c⎦
// i.e. the quotient takes all values from zero up to max = ⎣(2^n - 1)/c⎦
//
// If x is not divisible by c, then x*m mod 2^n must take some larger value than max.
//
// This gives x*m mod 2^n <= ⎣(2^n - 1)/c⎦ as a test for divisibility
// involving one multiplication and compare.
//
// To extend this to even integers, consider c = d0 * 2^k where d0 is odd.
// We can test whether x is divisible by both d0 and 2^k.
// For d0, the test is the same as above.  Let m be such that m*d0 mod 2^n == 1
// Then x*m mod 2^n <= ⎣(2^n - 1)/d0⎦ is the first test.
// The test for divisibility by 2^k is a check for k trailing zeroes.
// Note that since d0 is odd, m is odd and thus x*m will have the same number of
// trailing zeroes as x.  So the two tests are,
//
// x*m mod 2^n <= ⎣(2^n - 1)/d0⎦
// and x*m ends in k zero bits
//
// These can be combined into a single comparison by the following
// (theorem ZRU in Hacker's Delight) for unsigned integers.
//
// x <= a and x ends in k zero bits if and only if RotRight(x ,k) <= ⎣a/(2^k)⎦
// Where RotRight(x ,k) is right rotation of x by k bits.
//
// To prove the first direction, x <= a -> ⎣x/(2^k)⎦ <= ⎣a/(2^k)⎦
// But since x ends in k zeroes all the rotated bits would be zero too.
// So RotRight(x, k) == ⎣x/(2^k)⎦ <= ⎣a/(2^k)⎦
//
// If x does not end in k zero bits, then RotRight(x, k)
// has some non-zero bits in the k highest bits.
// ⎣x/(2^k)⎦ has all zeroes in the k highest bits,
// so RotRight(x, k) > ⎣x/(2^k)⎦
//
// Finally, if x > a and has k trailing zero bits, then RotRight(x, k) == ⎣x/(2^k)⎦
// and ⎣x/(2^k)⎦ must be greater than ⎣a/(2^k)⎦, that is the top n-k bits of x must
// be greater than the top n-k bits of a because the rest of x bits are zero.
//
// So the two conditions about can be replaced with the single test
//
// RotRight(x*m mod 2^n, k) <= ⎣(2^n - 1)/c⎦
//
// Where d0*2^k was replaced by c on the right hand side.

// udivisibleOK reports whether we should strength reduce an unsigned n-bit divisibilty check by c.
private static bool udivisibleOK(nuint n, long c) { 
    // Convert from ConstX auxint values to the real uint64 constant they represent.
    var d = uint64(c) << (int)((64 - n)) >> (int)((64 - n)); 

    // Doesn't work for 0.
    // Don't use for powers of 2.
    return d & (d - 1) != 0;
}

private static bool udivisibleOK8(sbyte c) {
    return udivisibleOK(8, int64(c));
}
private static bool udivisibleOK16(short c) {
    return udivisibleOK(16, int64(c));
}
private static bool udivisibleOK32(int c) {
    return udivisibleOK(32, int64(c));
}
private static bool udivisibleOK64(long c) {
    return udivisibleOK(64, c);
}

private partial struct udivisibleData {
    public long k; // trailingZeros(c)
    public ulong m; // m * (c>>k) mod 2^n == 1 multiplicative inverse of odd portion modulo 2^n
    public ulong max; // ⎣(2^n - 1)/ c⎦ max value to for divisibility
}

private static udivisibleData udivisible(nuint n, long c) { 
    // Convert from ConstX auxint values to the real uint64 constant they represent.
    var d = uint64(c) << (int)((64 - n)) >> (int)((64 - n));

    var k = bits.TrailingZeros64(d);
    var d0 = d >> (int)(uint(k)); // the odd portion of the divisor

    var mask = ~uint64(0) >> (int)((64 - n)); 

    // Calculate the multiplicative inverse via Newton's method.
    // Quadratic convergence doubles the number of correct bits per iteration.
    var m = d0; // initial guess correct to 3-bits d0*d0 mod 8 == 1
    m = m * (2 - m * d0); // 6-bits
    m = m * (2 - m * d0); // 12-bits
    m = m * (2 - m * d0); // 24-bits
    m = m * (2 - m * d0); // 48-bits
    m = m * (2 - m * d0); // 96-bits >= 64-bits
    m = m & mask;

    var max = mask / d;

    return new udivisibleData(k:int64(k),m:m,max:max,);
}

private static udivisibleData udivisible8(sbyte c) {
    return udivisible(8, int64(c));
}
private static udivisibleData udivisible16(short c) {
    return udivisible(16, int64(c));
}
private static udivisibleData udivisible32(int c) {
    return udivisible(32, int64(c));
}
private static udivisibleData udivisible64(long c) {
    return udivisible(64, c);
}

// For signed integers, a similar method follows.
//
// Given c > 1 and odd, compute m such that (c * m) mod 2^n == 1
// Then if c divides x (x%c ==0), the quotient is given by q = x/c == x*m mod 2^n
//
// x can range from ⎡-2^(n-1)/c⎤ * c, ... -c, 0, c, ...  ⎣(2^(n-1) - 1)/c⎦ * c
// Thus, x*m mod 2^n is ⎡-2^(n-1)/c⎤, ... -2, -1, 0, 1, 2, ... ⎣(2^(n-1) - 1)/c⎦
//
// So, x is a multiple of c if and only if:
// ⎡-2^(n-1)/c⎤ <= x*m mod 2^n <= ⎣(2^(n-1) - 1)/c⎦
//
// Since c > 1 and odd, this can be simplified by
// ⎡-2^(n-1)/c⎤ == ⎡(-2^(n-1) + 1)/c⎤ == -⎣(2^(n-1) - 1)/c⎦
//
// -⎣(2^(n-1) - 1)/c⎦ <= x*m mod 2^n <= ⎣(2^(n-1) - 1)/c⎦
//
// To extend this to even integers, consider c = d0 * 2^k where d0 is odd.
// We can test whether x is divisible by both d0 and 2^k.
//
// Let m be such that (d0 * m) mod 2^n == 1.
// Let q = x*m mod 2^n. Then c divides x if:
//
// -⎣(2^(n-1) - 1)/d0⎦ <= q <= ⎣(2^(n-1) - 1)/d0⎦ and q ends in at least k 0-bits
//
// To transform this to a single comparison, we use the following theorem (ZRS in Hacker's Delight).
//
// For a >= 0 the following conditions are equivalent:
// 1) -a <= x <= a and x ends in at least k 0-bits
// 2) RotRight(x+a', k) <= ⎣2a'/2^k⎦
//
// Where a' = a & -2^k (a with its right k bits set to zero)
//
// To see that 1 & 2 are equivalent, note that -a <= x <= a is equivalent to
// -a' <= x <= a' if and only if x ends in at least k 0-bits.  Adding -a' to each side gives,
// 0 <= x + a' <= 2a' and x + a' ends in at least k 0-bits if and only if x does since a' has
// k 0-bits by definition.  We can use theorem ZRU above with x -> x + a' and a -> 2a' giving 1) == 2).
//
// Let m be such that (d0 * m) mod 2^n == 1.
// Let q = x*m mod 2^n.
// Let a' = ⎣(2^(n-1) - 1)/d0⎦ & -2^k
//
// Then the divisibility test is:
//
// RotRight(q+a', k) <= ⎣2a'/2^k⎦
//
// Note that the calculation is performed using unsigned integers.
// Since a' can have n-1 bits, 2a' may have n bits and there is no risk of overflow.

// sdivisibleOK reports whether we should strength reduce a signed n-bit divisibilty check by c.
private static bool sdivisibleOK(nuint n, long c) {
    if (c < 0) { 
        // Doesn't work for negative c.
        return false;
    }
    return c & (c - 1) != 0;
}

private static bool sdivisibleOK8(sbyte c) {
    return sdivisibleOK(8, int64(c));
}
private static bool sdivisibleOK16(short c) {
    return sdivisibleOK(16, int64(c));
}
private static bool sdivisibleOK32(int c) {
    return sdivisibleOK(32, int64(c));
}
private static bool sdivisibleOK64(long c) {
    return sdivisibleOK(64, c);
}

private partial struct sdivisibleData {
    public long k; // trailingZeros(c)
    public ulong m; // m * (c>>k) mod 2^n == 1 multiplicative inverse of odd portion modulo 2^n
    public ulong a; // ⎣(2^(n-1) - 1)/ (c>>k)⎦ & -(1<<k) additive constant
    public ulong max; // ⎣(2 a) / (1<<k)⎦ max value to for divisibility
}

private static sdivisibleData sdivisible(nuint n, long c) {
    var d = uint64(c);
    var k = bits.TrailingZeros64(d);
    var d0 = d >> (int)(uint(k)); // the odd portion of the divisor

    var mask = ~uint64(0) >> (int)((64 - n)); 

    // Calculate the multiplicative inverse via Newton's method.
    // Quadratic convergence doubles the number of correct bits per iteration.
    var m = d0; // initial guess correct to 3-bits d0*d0 mod 8 == 1
    m = m * (2 - m * d0); // 6-bits
    m = m * (2 - m * d0); // 12-bits
    m = m * (2 - m * d0); // 24-bits
    m = m * (2 - m * d0); // 48-bits
    m = m * (2 - m * d0); // 96-bits >= 64-bits
    m = m & mask;

    var a = ((mask >> 1) / d0) & -(1 << (int)(uint(k)));
    nint max = (2 * a) >> (int)(uint(k));

    return new sdivisibleData(k:int64(k),m:m,a:a,max:max,);
}

private static sdivisibleData sdivisible8(sbyte c) {
    return sdivisible(8, int64(c));
}
private static sdivisibleData sdivisible16(short c) {
    return sdivisible(16, int64(c));
}
private static sdivisibleData sdivisible32(int c) {
    return sdivisible(32, int64(c));
}
private static sdivisibleData sdivisible64(long c) {
    return sdivisible(64, c);
}

} // end ssa_package
