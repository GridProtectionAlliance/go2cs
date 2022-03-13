// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements unsigned multi-precision integers (natural
// numbers). They are the building blocks for the implementation
// of signed integers, rationals, and floating-point numbers.
//
// Caution: This implementation relies on the function "alias"
//          which assumes that (nat) slice capacities are never
//          changed (no 3-operand slice expressions). If that
//          changes, alias needs to be updated for correctness.

// package big -- go2cs converted at 2022 March 13 05:32:11 UTC
// import "math/big" ==> using big = go.math.big_package
// Original source: C:\Program Files\Go\src\math\big\nat.go
namespace go.math;

using binary = encoding.binary_package;
using bits = math.bits_package;
using rand = math.rand_package;
using sync = sync_package;


// An unsigned integer x of the form
//
//   x = x[n-1]*_B^(n-1) + x[n-2]*_B^(n-2) + ... + x[1]*_B + x[0]
//
// with 0 <= x[i] < _B and 0 <= i < n is stored in a slice of length n,
// with the digits x[i] as the slice elements.
//
// A number is normalized if the slice contains no leading 0 digits.
// During arithmetic operations, denormalized values may occur but are
// always normalized before returning the final result. The normalized
// representation of 0 is the empty or nil slice (length = 0).
//

public static partial class big_package {

private partial struct nat { // : slice<Word>
}

private static nat natOne = new nat(1);private static nat natTwo = new nat(2);private static nat natFive = new nat(5);private static nat natTen = new nat(10);

private static void clear(this nat z) {
    foreach (var (i) in z) {
        z[i] = 0;
    }
}

private static nat norm(this nat z) {
    var i = len(z);
    while (i > 0 && z[i - 1] == 0) {
        i--;
    }
    return z[(int)0..(int)i];
}

private static nat make(this nat z, nint n) {
    if (n <= cap(z)) {
        return z[..(int)n]; // reuse z
    }
    if (n == 1) { 
        // Most nats start small and stay that way; don't over-allocate.
        return make(nat, 1);
    }
    const nint e = 4; // extra capacity
 // extra capacity
    return make(nat, n, n + e);
}

private static nat setWord(this nat z, Word x) {
    if (x == 0) {
        return z[..(int)0];
    }
    z = z.make(1);
    z[0] = x;
    return z;
}

private static nat setUint64(this nat z, ulong x) { 
    // single-word value
    {
        var w = Word(x);

        if (uint64(w) == x) {
            return z.setWord(w);
        }
    } 
    // 2-word value
    z = z.make(2);
    z[1] = Word(x >> 32);
    z[0] = Word(x);
    return z;
}

private static nat set(this nat z, nat x) {
    z = z.make(len(x));
    copy(z, x);
    return z;
}

private static nat add(this nat z, nat x, nat y) {
    var m = len(x);
    var n = len(y);


    if (m < n) 
        return z.add(y, x);
    else if (m == 0) 
        // n == 0 because m >= n; result is 0
        return z[..(int)0];
    else if (n == 0) 
        // result is x
        return z.set(x);
    // m > 0

    z = z.make(m + 1);
    var c = addVV(z[(int)0..(int)n], x, y);
    if (m > n) {
        c = addVW(z[(int)n..(int)m], x[(int)n..], c);
    }
    z[m] = c;

    return z.norm();
}

private static nat sub(this nat z, nat x, nat y) => func((_, panic, _) => {
    var m = len(x);
    var n = len(y);


    if (m < n) 
        panic("underflow");
    else if (m == 0) 
        // n == 0 because m >= n; result is 0
        return z[..(int)0];
    else if (n == 0) 
        // result is x
        return z.set(x);
    // m > 0

    z = z.make(m);
    var c = subVV(z[(int)0..(int)n], x, y);
    if (m > n) {
        c = subVW(z[(int)n..], x[(int)n..], c);
    }
    if (c != 0) {
        panic("underflow");
    }
    return z.norm();
});

private static nint cmp(this nat x, nat y) {
    nint r = default;

    var m = len(x);
    var n = len(y);
    if (m != n || m == 0) {

        if (m < n) 
            r = -1;
        else if (m > n) 
            r = 1;
                return ;
    }
    var i = m - 1;
    while (i > 0 && x[i] == y[i]) {
        i--;
    }


    if (x[i] < y[i]) 
        r = -1;
    else if (x[i] > y[i]) 
        r = 1;
        return ;
}

private static nat mulAddWW(this nat z, nat x, Word y, Word r) {
    var m = len(x);
    if (m == 0 || y == 0) {
        return z.setWord(r); // result is r
    }
    z = z.make(m + 1);
    z[m] = mulAddVWW(z[(int)0..(int)m], x, y, r);

    return z.norm();
}

// basicMul multiplies x and y and leaves the result in z.
// The (non-normalized) result is placed in z[0 : len(x) + len(y)].
private static void basicMul(nat z, nat x, nat y) {
    z[(int)0..(int)len(x) + len(y)].clear(); // initialize z
    foreach (var (i, d) in y) {
        if (d != 0) {
            z[len(x) + i] = addMulVVW(z[(int)i..(int)i + len(x)], x, d);
        }
    }
}

// montgomery computes z mod m = x*y*2**(-n*_W) mod m,
// assuming k = -1/m mod 2**_W.
// z is used for storing the result which is returned;
// z must not alias x, y or m.
// See Gueron, "Efficient Software Implementations of Modular Exponentiation".
// https://eprint.iacr.org/2011/239.pdf
// In the terminology of that paper, this is an "Almost Montgomery Multiplication":
// x and y are required to satisfy 0 <= z < 2**(n*_W) and then the result
// z is guaranteed to satisfy 0 <= z < 2**(n*_W), but it may not be < m.
private static nat montgomery(this nat z, nat x, nat y, nat m, Word k, nint n) => func((_, panic, _) => { 
    // This code assumes x, y, m are all the same length, n.
    // (required by addMulVVW and the for loop).
    // It also assumes that x, y are already reduced mod m,
    // or else the result will not be properly reduced.
    if (len(x) != n || len(y) != n || len(m) != n) {
        panic("math/big: mismatched montgomery number lengths");
    }
    z = z.make(n * 2);
    z.clear();
    Word c = default;
    for (nint i = 0; i < n; i++) {
        var d = y[i];
        var c2 = addMulVVW(z[(int)i..(int)n + i], x, d);
        var t = z[i] * k;
        var c3 = addMulVVW(z[(int)i..(int)n + i], m, t);
        var cx = c + c2;
        var cy = cx + c3;
        z[n + i] = cy;
        if (cx < c2 || cy < c3) {
            c = 1;
        }
        else
 {
            c = 0;
        }
    }
    if (c != 0) {
        subVV(z[..(int)n], z[(int)n..], m);
    }
    else
 {
        copy(z[..(int)n], z[(int)n..]);
    }
    return z[..(int)n];
});

// Fast version of z[0:n+n>>1].add(z[0:n+n>>1], x[0:n]) w/o bounds checks.
// Factored out for readability - do not use outside karatsuba.
private static void karatsubaAdd(nat z, nat x, nint n) {
    {
        var c = addVV(z[(int)0..(int)n], z, x);

        if (c != 0) {
            addVW(z[(int)n..(int)n + n >> 1], z[(int)n..], c);
        }
    }
}

// Like karatsubaAdd, but does subtract.
private static void karatsubaSub(nat z, nat x, nint n) {
    {
        var c = subVV(z[(int)0..(int)n], z, x);

        if (c != 0) {
            subVW(z[(int)n..(int)n + n >> 1], z[(int)n..], c);
        }
    }
}

// Operands that are shorter than karatsubaThreshold are multiplied using
// "grade school" multiplication; for longer operands the Karatsuba algorithm
// is used.
private static nint karatsubaThreshold = 40; // computed by calibrate_test.go

// karatsuba multiplies x and y and leaves the result in z.
// Both x and y must have the same length n and n must be a
// power of 2. The result vector z must have len(z) >= 6*n.
// The (non-normalized) result is placed in z[0 : 2*n].
private static void karatsuba(nat z, nat x, nat y) {
    var n = len(y); 

    // Switch to basic multiplication if numbers are odd or small.
    // (n is always even if karatsubaThreshold is even, but be
    // conservative)
    if (n & 1 != 0 || n < karatsubaThreshold || n < 2) {
        basicMul(z, x, y);
        return ;
    }
    var n2 = n >> 1; // n2 >= 1
    var x1 = x[(int)n2..];
    var x0 = x[(int)0..(int)n2]; // x = x1*b + y0
    var y1 = y[(int)n2..];
    var y0 = y[(int)0..(int)n2]; // y = y1*b + y0

    // z is used for the result and temporary storage:
    //
    //   6*n     5*n     4*n     3*n     2*n     1*n     0*n
    // z = [z2 copy|z0 copy| xd*yd | yd:xd | x1*y1 | x0*y0 ]
    //
    // For each recursive call of karatsuba, an unused slice of
    // z is passed in that has (at least) half the length of the
    // caller's z.

    // compute z0 and z2 with the result "in place" in z
    karatsuba(z, x0, y0); // z0 = x0*y0
    karatsuba(z[(int)n..], x1, y1); // z2 = x1*y1

    // compute xd (or the negative value if underflow occurs)
    nint s = 1; // sign of product xd*yd
    var xd = z[(int)2 * n..(int)2 * n + n2];
    if (subVV(xd, x1, x0) != 0) { // x1-x0
        s = -s;
        subVV(xd, x0, x1); // x0-x1
    }
    var yd = z[(int)2 * n + n2..(int)3 * n];
    if (subVV(yd, y0, y1) != 0) { // y0-y1
        s = -s;
        subVV(yd, y1, y0); // y1-y0
    }
    var p = z[(int)n * 3..];
    karatsuba(p, xd, yd); 

    // save original z2:z0
    // (ok to use upper half of z since we're done recursing)
    var r = z[(int)n * 4..];
    copy(r, z[..(int)n * 2]); 

    // add up all partial products
    //
    //   2*n     n     0
    // z = [ z2  | z0  ]
    //   +    [ z0  ]
    //   +    [ z2  ]
    //   +    [  p  ]
    //
    karatsubaAdd(z[(int)n2..], r, n);
    karatsubaAdd(z[(int)n2..], r[(int)n..], n);
    if (s > 0) {
        karatsubaAdd(z[(int)n2..], p, n);
    }
    else
 {
        karatsubaSub(z[(int)n2..], p, n);
    }
}

// alias reports whether x and y share the same base array.
// Note: alias assumes that the capacity of underlying arrays
//       is never changed for nat values; i.e. that there are
//       no 3-operand slice expressions in this code (or worse,
//       reflect-based operations to the same effect).
private static bool alias(nat x, nat y) {
    return cap(x) > 0 && cap(y) > 0 && _addr_x[(int)0..(int)cap(x)][cap(x) - 1] == _addr_y[(int)0..(int)cap(y)][cap(y) - 1];
}

// addAt implements z += x<<(_W*i); z must be long enough.
// (we don't use nat.add because we need z to stay the same
// slice, and we don't need to normalize z after each addition)
private static void addAt(nat z, nat x, nint i) {
    {
        var n = len(x);

        if (n > 0) {
            {
                var c = addVV(z[(int)i..(int)i + n], z[(int)i..], x);

                if (c != 0) {
                    var j = i + n;
                    if (j < len(z)) {
                        addVW(z[(int)j..], z[(int)j..], c);
                    }
                }

            }
        }
    }
}

private static nint max(nint x, nint y) {
    if (x > y) {
        return x;
    }
    return y;
}

// karatsubaLen computes an approximation to the maximum k <= n such that
// k = p<<i for a number p <= threshold and an i >= 0. Thus, the
// result is the largest number that can be divided repeatedly by 2 before
// becoming about the value of threshold.
private static nint karatsubaLen(nint n, nint threshold) {
    var i = uint(0);
    while (n > threshold) {
        n>>=1;
        i++;
    }
    return n << (int)(i);
}

private static nat mul(this nat z, nat x, nat y) {
    var m = len(x);
    var n = len(y);


    if (m < n) 
        return z.mul(y, x);
    else if (m == 0 || n == 0) 
        return z[..(int)0];
    else if (n == 1) 
        return z.mulAddWW(x, y[0], 0);
    // m >= n > 1

    // determine if z can be reused
    if (alias(z, x) || alias(z, y)) {
        z = null; // z is an alias for x or y - cannot reuse
    }
    if (n < karatsubaThreshold) {
        z = z.make(m + n);
        basicMul(z, x, y);
        return z.norm();
    }
    var k = karatsubaLen(n, karatsubaThreshold); 
    // k <= n

    // multiply x0 and y0 via Karatsuba
    var x0 = x[(int)0..(int)k]; // x0 is not normalized
    var y0 = y[(int)0..(int)k]; // y0 is not normalized
    z = z.make(max(6 * k, m + n)); // enough space for karatsuba of x0*y0 and full result of x*y
    karatsuba(z, x0, y0);
    z = z[(int)0..(int)m + n]; // z has final length but may be incomplete
    z[(int)2 * k..].clear(); // upper portion of z is garbage (and 2*k <= m+n since k <= n <= m)

    // If xh != 0 or yh != 0, add the missing terms to z. For
    //
    //   xh = xi*b^i + ... + x2*b^2 + x1*b (0 <= xi < b)
    //   yh =                         y1*b (0 <= y1 < b)
    //
    // the missing terms are
    //
    //   x0*y1*b and xi*y0*b^i, xi*y1*b^(i+1) for i > 0
    //
    // since all the yi for i > 1 are 0 by choice of k: If any of them
    // were > 0, then yh >= b^2 and thus y >= b^2. Then k' = k*2 would
    // be a larger valid threshold contradicting the assumption about k.
    //
    if (k < n || m != n) {
        var tp = getNat(3 * k);
        var t = tp.val; 

        // add x0*y1*b
        x0 = x0.norm();
        var y1 = y[(int)k..]; // y1 is normalized because y is
        t = t.mul(x0, y1); // update t so we don't lose t's underlying array
        addAt(z, t, k); 

        // add xi*y0<<i, xi*y1*b<<(i+k)
        y0 = y0.norm();
        {
            var i = k;

            while (i < len(x)) {
                var xi = x[(int)i..];
                if (len(xi) > k) {
                    xi = xi[..(int)k];
                i += k;
                }
                xi = xi.norm();
                t = t.mul(xi, y0);
                addAt(z, t, i);
                t = t.mul(xi, y1);
                addAt(z, t, i + k);
            }

        }

        putNat(_addr_tp);
    }
    return z.norm();
}

// basicSqr sets z = x*x and is asymptotically faster than basicMul
// by about a factor of 2, but slower for small arguments due to overhead.
// Requirements: len(x) > 0, len(z) == 2*len(x)
// The (non-normalized) result is placed in z.
private static void basicSqr(nat z, nat x) {
    var n = len(x);
    var tp = getNat(2 * n);
    var t = tp.val; // temporary variable to hold the products
    t.clear();
    z[1], z[0] = mulWW(x[0], x[0]); // the initial square
    for (nint i = 1; i < n; i++) {
        var d = x[i]; 
        // z collects the squares x[i] * x[i]
        z[2 * i + 1], z[2 * i] = mulWW(d, d); 
        // t collects the products x[i] * x[j] where j < i
        t[2 * i] = addMulVVW(t[(int)i..(int)2 * i], x[(int)0..(int)i], d);
    }
    t[2 * n - 1] = shlVU(t[(int)1..(int)2 * n - 1], t[(int)1..(int)2 * n - 1], 1); // double the j < i products
    addVV(z, z, t); // combine the result
    putNat(_addr_tp);
}

// karatsubaSqr squares x and leaves the result in z.
// len(x) must be a power of 2 and len(z) >= 6*len(x).
// The (non-normalized) result is placed in z[0 : 2*len(x)].
//
// The algorithm and the layout of z are the same as for karatsuba.
private static void karatsubaSqr(nat z, nat x) {
    var n = len(x);

    if (n & 1 != 0 || n < karatsubaSqrThreshold || n < 2) {
        basicSqr(z[..(int)2 * n], x);
        return ;
    }
    var n2 = n >> 1;
    var x1 = x[(int)n2..];
    var x0 = x[(int)0..(int)n2];

    karatsubaSqr(z, x0);
    karatsubaSqr(z[(int)n..], x1); 

    // s = sign(xd*yd) == -1 for xd != 0; s == 1 for xd == 0
    var xd = z[(int)2 * n..(int)2 * n + n2];
    if (subVV(xd, x1, x0) != 0) {
        subVV(xd, x0, x1);
    }
    var p = z[(int)n * 3..];
    karatsubaSqr(p, xd);

    var r = z[(int)n * 4..];
    copy(r, z[..(int)n * 2]);

    karatsubaAdd(z[(int)n2..], r, n);
    karatsubaAdd(z[(int)n2..], r[(int)n..], n);
    karatsubaSub(z[(int)n2..], p, n); // s == -1 for p != 0; s == 1 for p == 0
}

// Operands that are shorter than basicSqrThreshold are squared using
// "grade school" multiplication; for operands longer than karatsubaSqrThreshold
// we use the Karatsuba algorithm optimized for x == y.
private static nint basicSqrThreshold = 20; // computed by calibrate_test.go
private static nint karatsubaSqrThreshold = 260; // computed by calibrate_test.go

// z = x*x
private static nat sqr(this nat z, nat x) {
    var n = len(x);

    if (n == 0) 
        return z[..(int)0];
    else if (n == 1) 
        var d = x[0];
        z = z.make(2);
        z[1], z[0] = mulWW(d, d);
        return z.norm();
        if (alias(z, x)) {
        z = null; // z is an alias for x - cannot reuse
    }
    if (n < basicSqrThreshold) {
        z = z.make(2 * n);
        basicMul(z, x, x);
        return z.norm();
    }
    if (n < karatsubaSqrThreshold) {
        z = z.make(2 * n);
        basicSqr(z, x);
        return z.norm();
    }
    var k = karatsubaLen(n, karatsubaSqrThreshold);

    var x0 = x[(int)0..(int)k];
    z = z.make(max(6 * k, 2 * n));
    karatsubaSqr(z, x0); // z = x0^2
    z = z[(int)0..(int)2 * n];
    z[(int)2 * k..].clear();

    if (k < n) {
        var tp = getNat(2 * k);
        var t = tp.val;
        x0 = x0.norm();
        var x1 = x[(int)k..];
        t = t.mul(x0, x1);
        addAt(z, t, k);
        addAt(z, t, k); // z = 2*x1*x0*b + x0^2
        t = t.sqr(x1);
        addAt(z, t, 2 * k); // z = x1^2*b^2 + 2*x1*x0*b + x0^2
        putNat(_addr_tp);
    }
    return z.norm();
}

// mulRange computes the product of all the unsigned integers in the
// range [a, b] inclusively. If a > b (empty range), the result is 1.
private static nat mulRange(this nat z, ulong a, ulong b) {

    if (a == 0) 
        // cut long ranges short (optimization)
        return z.setUint64(0);
    else if (a > b) 
        return z.setUint64(1);
    else if (a == b) 
        return z.setUint64(a);
    else if (a + 1 == b) 
        return z.mul(nat(null).setUint64(a), nat(null).setUint64(b));
        var m = (a + b) / 2;
    return z.mul(nat(null).mulRange(a, m), nat(null).mulRange(m + 1, b));
}

// getNat returns a *nat of len n. The contents may not be zero.
// The pool holds *nat to avoid allocation when converting to interface{}.
private static ptr<nat> getNat(nint n) {
    ptr<nat> z;
    {
        var v = natPool.Get();

        if (v != null) {
            z = v._<ptr<nat>>();
        }
    }
    if (z == null) {
        z = @new<nat>();
    }
    z.val = z.make(n);
    return _addr_z!;
}

private static void putNat(ptr<nat> _addr_x) {
    ref nat x = ref _addr_x.val;

    natPool.Put(x);
}

private static sync.Pool natPool = default;

// Length of x in bits. x must be normalized.
private static nint bitLen(this nat x) {
    {
        var i = len(x) - 1;

        if (i >= 0) {
            return i * _W + bits.Len(uint(x[i]));
        }
    }
    return 0;
}

// trailingZeroBits returns the number of consecutive least significant zero
// bits of x.
private static nuint trailingZeroBits(this nat x) {
    if (len(x) == 0) {
        return 0;
    }
    nuint i = default;
    while (x[i] == 0) {
        i++;
    } 
    // x[i] != 0
    return i * _W + uint(bits.TrailingZeros(uint(x[i])));
}

private static bool same(nat x, nat y) {
    return len(x) == len(y) && len(x) > 0 && _addr_x[0] == _addr_y[0];
}

// z = x << s
private static nat shl(this nat z, nat x, nuint s) {
    if (s == 0) {
        if (same(z, x)) {
            return z;
        }
        if (!alias(z, x)) {
            return z.set(x);
        }
    }
    var m = len(x);
    if (m == 0) {
        return z[..(int)0];
    }
    var n = m + int(s / _W);
    z = z.make(n + 1);
    z[n] = shlVU(z[(int)n - m..(int)n], x, s % _W);
    z[(int)0..(int)n - m].clear();

    return z.norm();
}

// z = x >> s
private static nat shr(this nat z, nat x, nuint s) {
    if (s == 0) {
        if (same(z, x)) {
            return z;
        }
        if (!alias(z, x)) {
            return z.set(x);
        }
    }
    var m = len(x);
    var n = m - int(s / _W);
    if (n <= 0) {
        return z[..(int)0];
    }
    z = z.make(n);
    shrVU(z, x[(int)m - n..], s % _W);

    return z.norm();
}

private static nat setBit(this nat z, nat x, nuint i, nuint b) => func((_, panic, _) => {
    var j = int(i / _W);
    var m = Word(1) << (int)((i % _W));
    var n = len(x);
    switch (b) {
        case 0: 
            z = z.make(n);
            copy(z, x);
            if (j >= n) { 
                // no need to grow
                return z;
            }
            z[j] &= m;
            return z.norm();
            break;
        case 1: 
                   if (j >= n) {
                       z = z.make(j + 1);
                       z[(int)n..].clear();
                   }
                   else
            {
                       z = z.make(n);
                   }
                   copy(z, x);
                   z[j] |= m; 
                   // no need to normalize
                   return z;
            break;
    }
    panic("set bit is not 0 or 1");
});

// bit returns the value of the i'th bit, with lsb == bit 0.
private static nuint bit(this nat x, nuint i) {
    var j = i / _W;
    if (j >= uint(len(x))) {
        return 0;
    }
    return uint(x[j] >> (int)((i % _W)) & 1);
}

// sticky returns 1 if there's a 1 bit within the
// i least significant bits, otherwise it returns 0.
private static nuint sticky(this nat x, nuint i) {
    var j = i / _W;
    if (j >= uint(len(x))) {
        if (len(x) == 0) {
            return 0;
        }
        return 1;
    }
    foreach (var (_, x) in x[..(int)j]) {
        if (x != 0) {
            return 1;
        }
    }    if (x[j] << (int)((_W - i % _W)) != 0) {
        return 1;
    }
    return 0;
}

private static nat and(this nat z, nat x, nat y) {
    var m = len(x);
    var n = len(y);
    if (m > n) {
        m = n;
    }
    z = z.make(m);
    for (nint i = 0; i < m; i++) {
        z[i] = x[i] & y[i];
    }

    return z.norm();
}

private static nat andNot(this nat z, nat x, nat y) {
    var m = len(x);
    var n = len(y);
    if (n > m) {
        n = m;
    }
    z = z.make(m);
    for (nint i = 0; i < n; i++) {
        z[i] = x[i] & ~y[i];
    }
    copy(z[(int)n..(int)m], x[(int)n..(int)m]);

    return z.norm();
}

private static nat or(this nat z, nat x, nat y) {
    var m = len(x);
    var n = len(y);
    var s = x;
    if (m < n) {
        (n, m) = (m, n);        s = y;
    }
    z = z.make(m);
    for (nint i = 0; i < n; i++) {
        z[i] = x[i] | y[i];
    }
    copy(z[(int)n..(int)m], s[(int)n..(int)m]);

    return z.norm();
}

private static nat xor(this nat z, nat x, nat y) {
    var m = len(x);
    var n = len(y);
    var s = x;
    if (m < n) {
        (n, m) = (m, n);        s = y;
    }
    z = z.make(m);
    for (nint i = 0; i < n; i++) {
        z[i] = x[i] ^ y[i];
    }
    copy(z[(int)n..(int)m], s[(int)n..(int)m]);

    return z.norm();
}

// random creates a random integer in [0..limit), using the space in z if
// possible. n is the bit length of limit.
private static nat random(this nat z, ptr<rand.Rand> _addr_rand, nat limit, nint n) => func((_, panic, _) => {
    ref rand.Rand rand = ref _addr_rand.val;

    if (alias(z, limit)) {
        z = null; // z is an alias for limit - cannot reuse
    }
    z = z.make(len(limit));

    var bitLengthOfMSW = uint(n % _W);
    if (bitLengthOfMSW == 0) {
        bitLengthOfMSW = _W;
    }
    var mask = Word((1 << (int)(bitLengthOfMSW)) - 1);

    while (true) {
        switch (_W) {
            case 32: 
                {
                    var i__prev2 = i;

                    foreach (var (__i) in z) {
                        i = __i;
                        z[i] = Word(rand.Uint32());
                    }

                    i = i__prev2;
                }
                break;
            case 64: 
                {
                    var i__prev2 = i;

                    foreach (var (__i) in z) {
                        i = __i;
                        z[i] = Word(rand.Uint32()) | Word(rand.Uint32()) << 32;
                    }

                    i = i__prev2;
                }
                break;
            default: 
                panic("unknown word size");
                break;
        }
        z[len(limit) - 1] &= mask;
        if (z.cmp(limit) < 0) {
            break;
        }
    }

    return z.norm();
});

// If m != 0 (i.e., len(m) != 0), expNN sets z to x**y mod m;
// otherwise it sets z to x**y. The result is the value of z.
private static nat expNN(this nat z, nat x, nat y, nat m) {
    if (alias(z, x) || alias(z, y)) { 
        // We cannot allow in-place modification of x or y.
        z = null;
    }
    if (len(m) == 1 && m[0] == 1) {
        return z.setWord(0);
    }
    if (len(y) == 0) {
        return z.setWord(1);
    }
    if (len(y) == 1 && y[0] == 1 && len(m) != 0) {
        _, z = nat(null).div(z, x, m);
        return z;
    }
    if (len(m) != 0) { 
        // We likely end up being as long as the modulus.
        z = z.make(len(m));
    }
    z = z.set(x); 

    // If the base is non-trivial and the exponent is large, we use
    // 4-bit, windowed exponentiation. This involves precomputing 14 values
    // (x^2...x^15) but then reduces the number of multiply-reduces by a
    // third. Even for a 32-bit exponent, this reduces the number of
    // operations. Uses Montgomery method for odd moduli.
    if (x.cmp(natOne) > 0 && len(y) > 1 && len(m) > 0) {
        if (m[0] & 1 == 1) {
            return z.expNNMontgomery(x, y, m);
        }
        return z.expNNWindowed(x, y, m);
    }
    var v = y[len(y) - 1]; // v > 0 because y is normalized and y > 0
    var shift = nlz(v) + 1;
    v<<=shift;
    nat q = default;

    const nint mask = 1 << (int)((_W - 1)); 

    // We walk through the bits of the exponent one by one. Each time we
    // see a bit, we square, thus doubling the power. If the bit is a one,
    // we also multiply by x, thus adding one to the power.

 

    // We walk through the bits of the exponent one by one. Each time we
    // see a bit, we square, thus doubling the power. If the bit is a one,
    // we also multiply by x, thus adding one to the power.

    var w = _W - int(shift); 
    // zz and r are used to avoid allocating in mul and div as
    // otherwise the arguments would alias.
    nat zz = default;    nat r = default;

    {
        nint j__prev1 = j;

        for (nint j = 0; j < w; j++) {
            zz = zz.sqr(z);
            (zz, z) = (z, zz);            if (v & mask != 0) {
                zz = zz.mul(z, x);
                (zz, z) = (z, zz);
            }
            if (len(m) != 0) {
                zz, r = zz.div(r, z, m);
                (zz, r, q, z) = (q, z, zz, r);
            }
            v<<=1;
        }

        j = j__prev1;
    }

    for (var i = len(y) - 2; i >= 0; i--) {
        v = y[i];

        {
            nint j__prev2 = j;

            for (j = 0; j < _W; j++) {
                zz = zz.sqr(z);
                (zz, z) = (z, zz);                if (v & mask != 0) {
                    zz = zz.mul(z, x);
                    (zz, z) = (z, zz);
                }
                if (len(m) != 0) {
                    zz, r = zz.div(r, z, m);
                    (zz, r, q, z) = (q, z, zz, r);
                }
                v<<=1;
            }


            j = j__prev2;
        }
    }

    return z.norm();
}

// expNNWindowed calculates x**y mod m using a fixed, 4-bit window.
private static nat expNNWindowed(this nat z, nat x, nat y, nat m) { 
    // zz and r are used to avoid allocating in mul and div as otherwise
    // the arguments would alias.
    nat zz = default;    nat r = default;



    const nint n = 4; 
    // powers[i] contains x^i.
 
    // powers[i] contains x^i.
    array<nat> powers = new array<nat>(1 << (int)(n));
    powers[0] = natOne;
    powers[1] = x;
    {
        nint i__prev1 = i;

        nint i = 2;

        while (i < 1 << (int)(n)) {
            var p2 = _addr_powers[i / 2];
            var p = _addr_powers[i];
            var p1 = _addr_powers[i + 1];
            p.val = p.sqr(p2.val);
            zz, r = zz.div(r, p.val, m);
            (p.val, r) = (r, p.val);            p1.val = p1.mul(p.val, x);
            zz, r = zz.div(r, p1.val, m);
            (p1.val, r) = (r, p1.val);            i += 2;
        }

        i = i__prev1;
    }

    z = z.setWord(1);

    {
        nint i__prev1 = i;

        for (i = len(y) - 1; i >= 0; i--) {
            var yi = y[i];
            {
                nint j = 0;

                while (j < _W) {
                    if (i != len(y) - 1 || j != 0) { 
                        // Unrolled loop for significant performance
                        // gain. Use go test -bench=".*" in crypto/rsa
                        // to check performance before making changes.
                        zz = zz.sqr(z);
                        (zz, z) = (z, zz);                        zz, r = zz.div(r, z, m);
                        (z, r) = (r, z);                        zz = zz.sqr(z);
                        (zz, z) = (z, zz);                        zz, r = zz.div(r, z, m);
                        (z, r) = (r, z);                        zz = zz.sqr(z);
                        (zz, z) = (z, zz);                        zz, r = zz.div(r, z, m);
                        (z, r) = (r, z);                        zz = zz.sqr(z);
                        (zz, z) = (z, zz);                        zz, r = zz.div(r, z, m);
                        (z, r) = (r, z);                    j += n;
                    }
                    zz = zz.mul(z, powers[yi >> (int)((_W - n))]);
                    (zz, z) = (z, zz);                    zz, r = zz.div(r, z, m);
                    (z, r) = (r, z);                    yi<<=n;
                }

            }
        }

        i = i__prev1;
    }

    return z.norm();
}

// expNNMontgomery calculates x**y mod m using a fixed, 4-bit window.
// Uses Montgomery representation.
private static nat expNNMontgomery(this nat z, nat x, nat y, nat m) {
    var numWords = len(m); 

    // We want the lengths of x and m to be equal.
    // It is OK if x >= m as long as len(x) == len(m).
    if (len(x) > numWords) {
        _, x = nat(null).div(null, x, m); 
        // Note: now len(x) <= numWords, not guaranteed ==.
    }
    if (len(x) < numWords) {
        var rr = make(nat, numWords);
        copy(rr, x);
        x = rr;
    }
    nint k0 = 2 - m[0];
    var t = m[0] - 1;
    {
        nint i__prev1 = i;

        nint i = 1;

        while (i < _W) {
            t *= t;
            k0 *= (t + 1);
            i<<=1;
        }

        i = i__prev1;
    }
    k0 = -k0; 

    // RR = 2**(2*_W*len(m)) mod m
    var RR = nat(null).setWord(1);
    var zz = nat(null).shl(RR, uint(2 * numWords * _W));
    _, RR = nat(null).div(RR, zz, m);
    if (len(RR) < numWords) {
        zz = zz.make(numWords);
        copy(zz, RR);
        RR = zz;
    }
    var one = make(nat, numWords);
    one[0] = 1;

    const nint n = 4; 
    // powers[i] contains x^i
 
    // powers[i] contains x^i
    array<nat> powers = new array<nat>(1 << (int)(n));
    powers[0] = powers[0].montgomery(one, RR, m, k0, numWords);
    powers[1] = powers[1].montgomery(x, RR, m, k0, numWords);
    {
        nint i__prev1 = i;

        for (i = 2; i < 1 << (int)(n); i++) {
            powers[i] = powers[i].montgomery(powers[i - 1], powers[1], m, k0, numWords);
        }

        i = i__prev1;
    } 

    // initialize z = 1 (Montgomery 1)
    z = z.make(numWords);
    copy(z, powers[0]);

    zz = zz.make(numWords); 

    // same windowed exponent, but with Montgomery multiplications
    {
        nint i__prev1 = i;

        for (i = len(y) - 1; i >= 0; i--) {
            var yi = y[i];
            {
                nint j = 0;

                while (j < _W) {
                    if (i != len(y) - 1 || j != 0) {
                        zz = zz.montgomery(z, z, m, k0, numWords);
                        z = z.montgomery(zz, zz, m, k0, numWords);
                        zz = zz.montgomery(z, z, m, k0, numWords);
                        z = z.montgomery(zz, zz, m, k0, numWords);
                    j += n;
                    }
                    zz = zz.montgomery(z, powers[yi >> (int)((_W - n))], m, k0, numWords);
                    (z, zz) = (zz, z);                    yi<<=n;
                }

            }
        }

        i = i__prev1;
    } 
    // convert to regular number
    zz = zz.montgomery(z, one, m, k0, numWords); 

    // One last reduction, just in case.
    // See golang.org/issue/13907.
    if (zz.cmp(m) >= 0) { 
        // Common case is m has high bit set; in that case,
        // since zz is the same length as m, there can be just
        // one multiple of m to remove. Just subtract.
        // We think that the subtract should be sufficient in general,
        // so do that unconditionally, but double-check,
        // in case our beliefs are wrong.
        // The div is not expected to be reached.
        zz = zz.sub(zz, m);
        if (zz.cmp(m) >= 0) {
            _, zz = nat(null).div(null, zz, m);
        }
    }
    return zz.norm();
}

// bytes writes the value of z into buf using big-endian encoding.
// The value of z is encoded in the slice buf[i:]. If the value of z
// cannot be represented in buf, bytes panics. The number i of unused
// bytes at the beginning of buf is returned as result.
private static nint bytes(this nat z, slice<byte> buf) => func((_, panic, _) => {
    nint i = default;

    i = len(buf);
    foreach (var (_, d) in z) {
        for (nint j = 0; j < _S; j++) {
            i--;
            if (i >= 0) {
                buf[i] = byte(d);
            }
            else if (byte(d) != 0) {
                panic("math/big: buffer too small to fit value");
            }
            d>>=8;
        }
    }    if (i < 0) {
        i = 0;
    }
    while (i < len(buf) && buf[i] == 0) {
        i++;
    }

    return ;
});

// bigEndianWord returns the contents of buf interpreted as a big-endian encoded Word value.
private static Word bigEndianWord(slice<byte> buf) {
    if (_W == 64) {
        return Word(binary.BigEndian.Uint64(buf));
    }
    return Word(binary.BigEndian.Uint32(buf));
}

// setBytes interprets buf as the bytes of a big-endian unsigned
// integer, sets z to that value, and returns z.
private static nat setBytes(this nat z, slice<byte> buf) {
    z = z.make((len(buf) + _S - 1) / _S);

    var i = len(buf);
    for (nint k = 0; i >= _S; k++) {
        z[k] = bigEndianWord(buf[(int)i - _S..(int)i]);
        i -= _S;
    }
    if (i > 0) {
        Word d = default;
        {
            var s = uint(0);

            while (i > 0) {
                d |= Word(buf[i - 1]) << (int)(s);
                i--;
                s += 8;
            }

        }
        z[len(z) - 1] = d;
    }
    return z.norm();
}

// sqrt sets z = ⌊√x⌋
private static nat sqrt(this nat z, nat x) {
    if (x.cmp(natOne) <= 0) {
        return z.set(x);
    }
    if (alias(z, x)) {
        z = null;
    }
    nat z1 = default;    nat z2 = default;

    z1 = z;
    z1 = z1.setUint64(1);
    z1 = z1.shl(z1, uint(x.bitLen() + 1) / 2); // must be ≥ √x
    for (nint n = 0; ; n++) {
        z2, _ = z2.div(null, x, z1);
        z2 = z2.add(z2, z1);
        z2 = z2.shr(z2, 1);
        if (z2.cmp(z1) >= 0) { 
            // z1 is answer.
            // Figure out whether z1 or z2 is currently aliased to z by looking at loop count.
            if (n & 1 == 0) {
                return z1;
            }
            return z.set(z1);
        }
        (z1, z2) = (z2, z1);
    }
}

} // end big_package
