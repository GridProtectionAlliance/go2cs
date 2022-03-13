// Copyright (c) 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package edwards25519 -- go2cs converted at 2022 March 13 05:31:43 UTC
// import "crypto/ed25519/internal/edwards25519" ==> using edwards25519 = go.crypto.ed25519.@internal.edwards25519_package
// Original source: C:\Program Files\Go\src\crypto\ed25519\internal\edwards25519\scalarmult.go
namespace go.crypto.ed25519.@internal;

using sync = sync_package;
using System;

public static partial class edwards25519_package {

// basepointTable is a set of 32 affineLookupTables, where table i is generated
// from 256i * basepoint. It is precomputed the first time it's used.
private static ptr<array<affineLookupTable>> basepointTable() {
    basepointTablePrecomp.initOnce.Do(() => {
        var p = NewGeneratorPoint();
        for (nint i = 0; i < 32; i++) {
            basepointTablePrecomp.table[i].FromP3(p);
            for (nint j = 0; j < 8; j++) {
                p.Add(p, p);
            }
        }
    });
    return _addr__addr_basepointTablePrecomp.table!;
}

private static var basepointTablePrecomp = default;

// ScalarBaseMult sets v = x * B, where B is the canonical generator, and
// returns v.
//
// The scalar multiplication is done in constant time.
private static ptr<Point> ScalarBaseMult(this ptr<Point> _addr_v, ptr<Scalar> _addr_x) {
    ref Point v = ref _addr_v.val;
    ref Scalar x = ref _addr_x.val;

    var basepointTable = basepointTable(); 

    // Write x = sum(x_i * 16^i) so  x*B = sum( B*x_i*16^i )
    // as described in the Ed25519 paper
    //
    // Group even and odd coefficients
    // x*B     = x_0*16^0*B + x_2*16^2*B + ... + x_62*16^62*B
    //         + x_1*16^1*B + x_3*16^3*B + ... + x_63*16^63*B
    // x*B     = x_0*16^0*B + x_2*16^2*B + ... + x_62*16^62*B
    //    + 16*( x_1*16^0*B + x_3*16^2*B + ... + x_63*16^62*B)
    //
    // We use a lookup table for each i to get x_i*16^(2*i)*B
    // and do four doublings to multiply by 16.
    var digits = x.signedRadix16();

    ptr<affineCached> multiple = addr(new affineCached());
    ptr<projP1xP1> tmp1 = addr(new projP1xP1());
    ptr<projP2> tmp2 = addr(new projP2()); 

    // Accumulate the odd components first
    v.Set(NewIdentityPoint());
    {
        nint i__prev1 = i;

        nint i = 1;

        while (i < 64) {
            basepointTable[i / 2].SelectInto(multiple, digits[i]);
            tmp1.AddAffine(v, multiple);
            v.fromP1xP1(tmp1);
            i += 2;
        }

        i = i__prev1;
    } 

    // Multiply by 16
    tmp2.FromP3(v); // tmp2 =    v in P2 coords
    tmp1.Double(tmp2); // tmp1 =  2*v in P1xP1 coords
    tmp2.FromP1xP1(tmp1); // tmp2 =  2*v in P2 coords
    tmp1.Double(tmp2); // tmp1 =  4*v in P1xP1 coords
    tmp2.FromP1xP1(tmp1); // tmp2 =  4*v in P2 coords
    tmp1.Double(tmp2); // tmp1 =  8*v in P1xP1 coords
    tmp2.FromP1xP1(tmp1); // tmp2 =  8*v in P2 coords
    tmp1.Double(tmp2); // tmp1 = 16*v in P1xP1 coords
    v.fromP1xP1(tmp1); // now v = 16*(odd components)

    // Accumulate the even components
    {
        nint i__prev1 = i;

        i = 0;

        while (i < 64) {
            basepointTable[i / 2].SelectInto(multiple, digits[i]);
            tmp1.AddAffine(v, multiple);
            v.fromP1xP1(tmp1);
            i += 2;
        }

        i = i__prev1;
    }

    return _addr_v!;
}

// ScalarMult sets v = x * q, and returns v.
//
// The scalar multiplication is done in constant time.
private static ptr<Point> ScalarMult(this ptr<Point> _addr_v, ptr<Scalar> _addr_x, ptr<Point> _addr_q) {
    ref Point v = ref _addr_v.val;
    ref Scalar x = ref _addr_x.val;
    ref Point q = ref _addr_q.val;

    checkInitialized(q);

    projLookupTable table = default;
    table.FromP3(q); 

    // Write x = sum(x_i * 16^i)
    // so  x*Q = sum( Q*x_i*16^i )
    //         = Q*x_0 + 16*(Q*x_1 + 16*( ... + Q*x_63) ... )
    //           <------compute inside out---------
    //
    // We use the lookup table to get the x_i*Q values
    // and do four doublings to compute 16*Q
    var digits = x.signedRadix16(); 

    // Unwrap first loop iteration to save computing 16*identity
    ptr<projCached> multiple = addr(new projCached());
    ptr<projP1xP1> tmp1 = addr(new projP1xP1());
    ptr<projP2> tmp2 = addr(new projP2());
    table.SelectInto(multiple, digits[63]);

    v.Set(NewIdentityPoint());
    tmp1.Add(v, multiple); // tmp1 = x_63*Q in P1xP1 coords
    for (nint i = 62; i >= 0; i--) {
        tmp2.FromP1xP1(tmp1); // tmp2 =    (prev) in P2 coords
        tmp1.Double(tmp2); // tmp1 =  2*(prev) in P1xP1 coords
        tmp2.FromP1xP1(tmp1); // tmp2 =  2*(prev) in P2 coords
        tmp1.Double(tmp2); // tmp1 =  4*(prev) in P1xP1 coords
        tmp2.FromP1xP1(tmp1); // tmp2 =  4*(prev) in P2 coords
        tmp1.Double(tmp2); // tmp1 =  8*(prev) in P1xP1 coords
        tmp2.FromP1xP1(tmp1); // tmp2 =  8*(prev) in P2 coords
        tmp1.Double(tmp2); // tmp1 = 16*(prev) in P1xP1 coords
        v.fromP1xP1(tmp1); //    v = 16*(prev) in P3 coords
        table.SelectInto(multiple, digits[i]);
        tmp1.Add(v, multiple); // tmp1 = x_i*Q + 16*(prev) in P1xP1 coords
    }
    v.fromP1xP1(tmp1);
    return _addr_v!;
}

// basepointNafTable is the nafLookupTable8 for the basepoint.
// It is precomputed the first time it's used.
private static ptr<nafLookupTable8> basepointNafTable() {
    basepointNafTablePrecomp.initOnce.Do(() => {
        basepointNafTablePrecomp.table.FromP3(NewGeneratorPoint());
    });
    return _addr__addr_basepointNafTablePrecomp.table!;
}

private static var basepointNafTablePrecomp = default;

// VarTimeDoubleScalarBaseMult sets v = a * A + b * B, where B is the canonical
// generator, and returns v.
//
// Execution time depends on the inputs.
private static ptr<Point> VarTimeDoubleScalarBaseMult(this ptr<Point> _addr_v, ptr<Scalar> _addr_a, ptr<Point> _addr_A, ptr<Scalar> _addr_b) {
    ref Point v = ref _addr_v.val;
    ref Scalar a = ref _addr_a.val;
    ref Point A = ref _addr_A.val;
    ref Scalar b = ref _addr_b.val;

    checkInitialized(A); 

    // Similarly to the single variable-base approach, we compute
    // digits and use them with a lookup table.  However, because
    // we are allowed to do variable-time operations, we don't
    // need constant-time lookups or constant-time digit
    // computations.
    //
    // So we use a non-adjacent form of some width w instead of
    // radix 16.  This is like a binary representation (one digit
    // for each binary place) but we allow the digits to grow in
    // magnitude up to 2^{w-1} so that the nonzero digits are as
    // sparse as possible.  Intuitively, this "condenses" the
    // "mass" of the scalar onto sparse coefficients (meaning
    // fewer additions).

    var basepointNafTable = basepointNafTable();
    nafLookupTable5 aTable = default;
    aTable.FromP3(A); 
    // Because the basepoint is fixed, we can use a wider NAF
    // corresponding to a bigger table.
    var aNaf = a.nonAdjacentForm(5);
    var bNaf = b.nonAdjacentForm(8); 

    // Find the first nonzero coefficient.
    nint i = 255;
    for (var j = i; j >= 0; j--) {
        if (aNaf[j] != 0 || bNaf[j] != 0) {
            break;
        }
    }

    ptr<projCached> multA = addr(new projCached());
    ptr<affineCached> multB = addr(new affineCached());
    ptr<projP1xP1> tmp1 = addr(new projP1xP1());
    ptr<projP2> tmp2 = addr(new projP2());
    tmp2.Zero(); 

    // Move from high to low bits, doubling the accumulator
    // at each iteration and checking whether there is a nonzero
    // coefficient to look up a multiple of.
    while (i >= 0) {
        tmp1.Double(tmp2); 

        // Only update v if we have a nonzero coeff to add in.
        if (aNaf[i] > 0) {
            v.fromP1xP1(tmp1);
            aTable.SelectInto(multA, aNaf[i]);
            tmp1.Add(v, multA);
        i--;
        }
        else if (aNaf[i] < 0) {
            v.fromP1xP1(tmp1);
            aTable.SelectInto(multA, -aNaf[i]);
            tmp1.Sub(v, multA);
        }
        if (bNaf[i] > 0) {
            v.fromP1xP1(tmp1);
            basepointNafTable.SelectInto(multB, bNaf[i]);
            tmp1.AddAffine(v, multB);
        }
        else if (bNaf[i] < 0) {
            v.fromP1xP1(tmp1);
            basepointNafTable.SelectInto(multB, -bNaf[i]);
            tmp1.SubAffine(v, multB);
        }
        tmp2.FromP1xP1(tmp1);
    }

    v.fromP2(tmp2);
    return _addr_v!;
}

} // end edwards25519_package
