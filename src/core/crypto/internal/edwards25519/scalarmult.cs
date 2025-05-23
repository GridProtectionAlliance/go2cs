// Copyright (c) 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal;

using sync = sync_package;

partial class edwards25519_package {

// basepointTable is a set of 32 affineLookupTables, where table i is generated
// from 256i * basepoint. It is precomputed the first time it's used.
internal static ж<array<affineLookupTable>> basepointTable() {
    basepointTablePrecomp.initOnce.Do(
    var basepointTablePrecompʗ2 = basepointTablePrecomp;
    () => {
        var p = NewGeneratorPoint();
        for (nint i = 0; i < 32; i++) {
            basepointTablePrecompʗ2.table[i].FromP3(p);
            for (nint j = 0; j < 8; j++) {
                p.Add(p, p);
            }
        }
    });
    return ᏑbasepointTablePrecomp.of(struct{table [32]affineLookupTable; initOnce sync.Once}.Ꮡtable);
}


[GoType("dyn")] partial struct basepointTablePrecompᴛ1 {
    internal array<affineLookupTable> table = new(32);
    internal sync_package.Once initOnce;
}
internal static basepointTablePrecompᴛ1 basepointTablePrecomp;

// ScalarBaseMult sets v = x * B, where B is the canonical generator, and
// returns v.
//
// The scalar multiplication is done in constant time.
[GoRecv("capture")] public static ж<Point> ScalarBaseMult(this ref Point v, ж<Scalar> Ꮡx) {
    ref var x = ref Ꮡx.val;

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
    var multiple = Ꮡ(new affineCached(nil));
    var tmp1 = Ꮡ(new projP1xP1(nil));
    var tmp2 = Ꮡ(new projP2(nil));
    // Accumulate the odd components first
    v.Set(NewIdentityPoint());
    for (nint i = 1; i < 64; i += 2) {
        basepointTable.val[i / 2].SelectInto(multiple, digits[i]);
        tmp1.AddAffine(v, multiple);
        v.fromP1xP1(tmp1);
    }
    // Multiply by 16
    tmp2.FromP3(v);
    // tmp2 =    v in P2 coords
    tmp1.Double(tmp2);
    // tmp1 =  2*v in P1xP1 coords
    tmp2.FromP1xP1(tmp1);
    // tmp2 =  2*v in P2 coords
    tmp1.Double(tmp2);
    // tmp1 =  4*v in P1xP1 coords
    tmp2.FromP1xP1(tmp1);
    // tmp2 =  4*v in P2 coords
    tmp1.Double(tmp2);
    // tmp1 =  8*v in P1xP1 coords
    tmp2.FromP1xP1(tmp1);
    // tmp2 =  8*v in P2 coords
    tmp1.Double(tmp2);
    // tmp1 = 16*v in P1xP1 coords
    v.fromP1xP1(tmp1);
    // now v = 16*(odd components)
    // Accumulate the even components
    for (nint i = 0; i < 64; i += 2) {
        basepointTable.val[i / 2].SelectInto(multiple, digits[i]);
        tmp1.AddAffine(v, multiple);
        v.fromP1xP1(tmp1);
    }
    return ScalarBaseMultꓸᏑv;
}

// ScalarMult sets v = x * q, and returns v.
//
// The scalar multiplication is done in constant time.
[GoRecv("capture")] public static ж<Point> ScalarMult(this ref Point v, ж<Scalar> Ꮡx, ж<Point> Ꮡq) {
    ref var x = ref Ꮡx.val;
    ref var q = ref Ꮡq.val;

    checkInitialized(Ꮡq);
    projLookupTable table = default!;
    table.FromP3(Ꮡq);
    // Write x = sum(x_i * 16^i)
    // so  x*Q = sum( Q*x_i*16^i )
    //         = Q*x_0 + 16*(Q*x_1 + 16*( ... + Q*x_63) ... )
    //           <------compute inside out---------
    //
    // We use the lookup table to get the x_i*Q values
    // and do four doublings to compute 16*Q
    var digits = x.signedRadix16();
    // Unwrap first loop iteration to save computing 16*identity
    var multiple = Ꮡ(new projCached(nil));
    var tmp1 = Ꮡ(new projP1xP1(nil));
    var tmp2 = Ꮡ(new projP2(nil));
    table.SelectInto(multiple, digits[63]);
    v.Set(NewIdentityPoint());
    tmp1.Add(v, multiple);
    // tmp1 = x_63*Q in P1xP1 coords
    for (nint i = 62; i >= 0; i--) {
        tmp2.FromP1xP1(tmp1);
        // tmp2 =    (prev) in P2 coords
        tmp1.Double(tmp2);
        // tmp1 =  2*(prev) in P1xP1 coords
        tmp2.FromP1xP1(tmp1);
        // tmp2 =  2*(prev) in P2 coords
        tmp1.Double(tmp2);
        // tmp1 =  4*(prev) in P1xP1 coords
        tmp2.FromP1xP1(tmp1);
        // tmp2 =  4*(prev) in P2 coords
        tmp1.Double(tmp2);
        // tmp1 =  8*(prev) in P1xP1 coords
        tmp2.FromP1xP1(tmp1);
        // tmp2 =  8*(prev) in P2 coords
        tmp1.Double(tmp2);
        // tmp1 = 16*(prev) in P1xP1 coords
        v.fromP1xP1(tmp1);
        //    v = 16*(prev) in P3 coords
        table.SelectInto(multiple, digits[i]);
        tmp1.Add(v, multiple);
    }
    // tmp1 = x_i*Q + 16*(prev) in P1xP1 coords
    v.fromP1xP1(tmp1);
    return ScalarMultꓸᏑv;
}

// basepointNafTable is the nafLookupTable8 for the basepoint.
// It is precomputed the first time it's used.
internal static ж<nafLookupTable8> basepointNafTable() {
    basepointNafTablePrecomp.initOnce.Do(
    var basepointNafTablePrecompʗ2 = basepointNafTablePrecomp;
    () => {
        basepointNafTablePrecompʗ2.table.FromP3(NewGeneratorPoint());
    });
    return ᏑbasepointNafTablePrecomp.of(struct{table nafLookupTable8; initOnce sync.Once}.Ꮡtable);
}


[GoType("dyn")] partial struct basepointNafTablePrecompᴛ1 {
    internal nafLookupTable8 table;
    internal sync_package.Once initOnce;
}
internal static basepointNafTablePrecompᴛ1 basepointNafTablePrecomp;

// VarTimeDoubleScalarBaseMult sets v = a * A + b * B, where B is the canonical
// generator, and returns v.
//
// Execution time depends on the inputs.
[GoRecv("capture")] public static ж<Point> VarTimeDoubleScalarBaseMult(this ref Point v, ж<Scalar> Ꮡa, ж<Point> ᏑA, ж<Scalar> Ꮡb) {
    ref var a = ref Ꮡa.val;
    ref var A = ref ᏑA.val;
    ref var b = ref Ꮡb.val;

    checkInitialized(ᏑA);
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
    nafLookupTable5 aTable = default!;
    aTable.FromP3(ᏑA);
    // Because the basepoint is fixed, we can use a wider NAF
    // corresponding to a bigger table.
    var aNaf = a.nonAdjacentForm(5);
    var bNaf = b.nonAdjacentForm(8);
    // Find the first nonzero coefficient.
    nint i = 255;
    for (nint j = i; j >= 0; j--) {
        if (aNaf[j] != 0 || bNaf[j] != 0) {
            break;
        }
    }
    var multA = Ꮡ(new projCached(nil));
    var multB = Ꮡ(new affineCached(nil));
    var tmp1 = Ꮡ(new projP1xP1(nil));
    var tmp2 = Ꮡ(new projP2(nil));
    tmp2.Zero();
    // Move from high to low bits, doubling the accumulator
    // at each iteration and checking whether there is a nonzero
    // coefficient to look up a multiple of.
    for (; i >= 0; i--) {
        tmp1.Double(tmp2);
        // Only update v if we have a nonzero coeff to add in.
        if (aNaf[i] > 0){
            v.fromP1xP1(tmp1);
            aTable.SelectInto(multA, aNaf[i]);
            tmp1.Add(v, multA);
        } else 
        if (aNaf[i] < 0) {
            v.fromP1xP1(tmp1);
            aTable.SelectInto(multA, -aNaf[i]);
            tmp1.Sub(v, multA);
        }
        if (bNaf[i] > 0){
            v.fromP1xP1(tmp1);
            basepointNafTable.SelectInto(multB, bNaf[i]);
            tmp1.AddAffine(v, multB);
        } else 
        if (bNaf[i] < 0) {
            v.fromP1xP1(tmp1);
            basepointNafTable.SelectInto(multB, -bNaf[i]);
            tmp1.SubAffine(v, multB);
        }
        tmp2.FromP1xP1(tmp1);
    }
    v.fromP2(tmp2);
    return VarTimeDoubleScalarBaseMultꓸᏑv;
}

} // end edwards25519_package
