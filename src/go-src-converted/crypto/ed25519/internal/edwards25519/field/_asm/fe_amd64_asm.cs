// Copyright (c) 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 06 22:17:29 UTC
// Original source: C:\Program Files\Go\src\crypto\ed25519\internal\edwards25519\field\_asm\fe_amd64_asm.go
using fmt = go.fmt_package;

using static go.github.com.mmcloughlin.avo.build_package;
using static go.github.com.mmcloughlin.avo.gotypes_package;
using static go.github.com.mmcloughlin.avo.operand_package;
using static go.github.com.mmcloughlin.avo.reg_package;

namespace go;

public static partial class main_package {

    //go:generate go run . -out ../fe_amd64.s -stubs ../fe_amd64.go -pkg field
private static void Main() {
    Package("crypto/ed25519/internal/edwards25519/field");
    ConstraintExpr("amd64,gc,!purego");
    feMul();
    feSquare();
    Generate();
}

private partial struct namedComponent {
    public ref Component Component => ref Component_val;
    public @string name;
}

private static @string String(this namedComponent c) {
    return c.name;
}

private partial struct uint128 {
    public @string name;
    public GPVirtual hi;
    public GPVirtual lo;
}

private static @string String(this uint128 c) {
    return c.name;
}

private static void feSquare() {
    TEXT("feSquare", NOSPLIT, "func(out, a *Element)");
    Doc("feSquare sets out = a * a. It works like feSquareGeneric.");
    Pragma("noescape");

    var a = Dereference(Param("a"));
    namedComponent l0 = new namedComponent(a.Field("l0"),"l0");
    namedComponent l1 = new namedComponent(a.Field("l1"),"l1");
    namedComponent l2 = new namedComponent(a.Field("l2"),"l2");
    namedComponent l3 = new namedComponent(a.Field("l3"),"l3");
    namedComponent l4 = new namedComponent(a.Field("l4"),"l4"); 

    // r0 = l0×l0 + 19×2×(l1×l4 + l2×l3)
    ref uint128 r0 = ref heap(new uint128("r0",GP64(),GP64()), out ptr<uint128> _addr_r0);
    mul64(r0, 1, l0, l0);
    addMul64(r0, 38, l1, l4);
    addMul64(r0, 38, l2, l3); 

    // r1 = 2×l0×l1 + 19×2×l2×l4 + 19×l3×l3
    ref uint128 r1 = ref heap(new uint128("r1",GP64(),GP64()), out ptr<uint128> _addr_r1);
    mul64(r1, 2, l0, l1);
    addMul64(r1, 38, l2, l4);
    addMul64(r1, 19, l3, l3); 

    // r2 = = 2×l0×l2 + l1×l1 + 19×2×l3×l4
    ref uint128 r2 = ref heap(new uint128("r2",GP64(),GP64()), out ptr<uint128> _addr_r2);
    mul64(r2, 2, l0, l2);
    addMul64(r2, 1, l1, l1);
    addMul64(r2, 38, l3, l4); 

    // r3 = = 2×l0×l3 + 2×l1×l2 + 19×l4×l4
    ref uint128 r3 = ref heap(new uint128("r3",GP64(),GP64()), out ptr<uint128> _addr_r3);
    mul64(r3, 2, l0, l3);
    addMul64(r3, 2, l1, l2);
    addMul64(r3, 19, l4, l4); 

    // r4 = = 2×l0×l4 + 2×l1×l3 + l2×l2
    ref uint128 r4 = ref heap(new uint128("r4",GP64(),GP64()), out ptr<uint128> _addr_r4);
    mul64(r4, 2, l0, l4);
    addMul64(r4, 2, l1, l3);
    addMul64(r4, 1, l2, l2);

    Comment("First reduction chain");
    var maskLow51Bits = GP64();
    MOVQ(Imm((1 << 51) - 1), maskLow51Bits);
    var (c0, r0lo) = shiftRightBy51(_addr_r0);
    var (c1, r1lo) = shiftRightBy51(_addr_r1);
    var (c2, r2lo) = shiftRightBy51(_addr_r2);
    var (c3, r3lo) = shiftRightBy51(_addr_r3);
    var (c4, r4lo) = shiftRightBy51(_addr_r4);
    maskAndAdd(r0lo, maskLow51Bits, c4, 19);
    maskAndAdd(r1lo, maskLow51Bits, c0, 1);
    maskAndAdd(r2lo, maskLow51Bits, c1, 1);
    maskAndAdd(r3lo, maskLow51Bits, c2, 1);
    maskAndAdd(r4lo, maskLow51Bits, c3, 1);

    Comment("Second reduction chain (carryPropagate)"); 
    // c0 = r0 >> 51
    MOVQ(r0lo, c0);
    SHRQ(Imm(51), c0); 
    // c1 = r1 >> 51
    MOVQ(r1lo, c1);
    SHRQ(Imm(51), c1); 
    // c2 = r2 >> 51
    MOVQ(r2lo, c2);
    SHRQ(Imm(51), c2); 
    // c3 = r3 >> 51
    MOVQ(r3lo, c3);
    SHRQ(Imm(51), c3); 
    // c4 = r4 >> 51
    MOVQ(r4lo, c4);
    SHRQ(Imm(51), c4);
    maskAndAdd(r0lo, maskLow51Bits, c4, 19);
    maskAndAdd(r1lo, maskLow51Bits, c0, 1);
    maskAndAdd(r2lo, maskLow51Bits, c1, 1);
    maskAndAdd(r3lo, maskLow51Bits, c2, 1);
    maskAndAdd(r4lo, maskLow51Bits, c3, 1);

    Comment("Store output");
    var @out = Dereference(Param("out"));
    Store(r0lo, @out.Field("l0"));
    Store(r1lo, @out.Field("l1"));
    Store(r2lo, @out.Field("l2"));
    Store(r3lo, @out.Field("l3"));
    Store(r4lo, @out.Field("l4"));

    RET();

}

private static void feMul() {
    TEXT("feMul", NOSPLIT, "func(out, a, b *Element)");
    Doc("feMul sets out = a * b. It works like feMulGeneric.");
    Pragma("noescape");

    var a = Dereference(Param("a"));
    namedComponent a0 = new namedComponent(a.Field("l0"),"a0");
    namedComponent a1 = new namedComponent(a.Field("l1"),"a1");
    namedComponent a2 = new namedComponent(a.Field("l2"),"a2");
    namedComponent a3 = new namedComponent(a.Field("l3"),"a3");
    namedComponent a4 = new namedComponent(a.Field("l4"),"a4");

    var b = Dereference(Param("b"));
    namedComponent b0 = new namedComponent(b.Field("l0"),"b0");
    namedComponent b1 = new namedComponent(b.Field("l1"),"b1");
    namedComponent b2 = new namedComponent(b.Field("l2"),"b2");
    namedComponent b3 = new namedComponent(b.Field("l3"),"b3");
    namedComponent b4 = new namedComponent(b.Field("l4"),"b4"); 

    // r0 = a0×b0 + 19×(a1×b4 + a2×b3 + a3×b2 + a4×b1)
    ref uint128 r0 = ref heap(new uint128("r0",GP64(),GP64()), out ptr<uint128> _addr_r0);
    mul64(r0, 1, a0, b0);
    addMul64(r0, 19, a1, b4);
    addMul64(r0, 19, a2, b3);
    addMul64(r0, 19, a3, b2);
    addMul64(r0, 19, a4, b1); 

    // r1 = a0×b1 + a1×b0 + 19×(a2×b4 + a3×b3 + a4×b2)
    ref uint128 r1 = ref heap(new uint128("r1",GP64(),GP64()), out ptr<uint128> _addr_r1);
    mul64(r1, 1, a0, b1);
    addMul64(r1, 1, a1, b0);
    addMul64(r1, 19, a2, b4);
    addMul64(r1, 19, a3, b3);
    addMul64(r1, 19, a4, b2); 

    // r2 = a0×b2 + a1×b1 + a2×b0 + 19×(a3×b4 + a4×b3)
    ref uint128 r2 = ref heap(new uint128("r2",GP64(),GP64()), out ptr<uint128> _addr_r2);
    mul64(r2, 1, a0, b2);
    addMul64(r2, 1, a1, b1);
    addMul64(r2, 1, a2, b0);
    addMul64(r2, 19, a3, b4);
    addMul64(r2, 19, a4, b3); 

    // r3 = a0×b3 + a1×b2 + a2×b1 + a3×b0 + 19×a4×b4
    ref uint128 r3 = ref heap(new uint128("r3",GP64(),GP64()), out ptr<uint128> _addr_r3);
    mul64(r3, 1, a0, b3);
    addMul64(r3, 1, a1, b2);
    addMul64(r3, 1, a2, b1);
    addMul64(r3, 1, a3, b0);
    addMul64(r3, 19, a4, b4); 

    // r4 = a0×b4 + a1×b3 + a2×b2 + a3×b1 + a4×b0
    ref uint128 r4 = ref heap(new uint128("r4",GP64(),GP64()), out ptr<uint128> _addr_r4);
    mul64(r4, 1, a0, b4);
    addMul64(r4, 1, a1, b3);
    addMul64(r4, 1, a2, b2);
    addMul64(r4, 1, a3, b1);
    addMul64(r4, 1, a4, b0);

    Comment("First reduction chain");
    var maskLow51Bits = GP64();
    MOVQ(Imm((1 << 51) - 1), maskLow51Bits);
    var (c0, r0lo) = shiftRightBy51(_addr_r0);
    var (c1, r1lo) = shiftRightBy51(_addr_r1);
    var (c2, r2lo) = shiftRightBy51(_addr_r2);
    var (c3, r3lo) = shiftRightBy51(_addr_r3);
    var (c4, r4lo) = shiftRightBy51(_addr_r4);
    maskAndAdd(r0lo, maskLow51Bits, c4, 19);
    maskAndAdd(r1lo, maskLow51Bits, c0, 1);
    maskAndAdd(r2lo, maskLow51Bits, c1, 1);
    maskAndAdd(r3lo, maskLow51Bits, c2, 1);
    maskAndAdd(r4lo, maskLow51Bits, c3, 1);

    Comment("Second reduction chain (carryPropagate)"); 
    // c0 = r0 >> 51
    MOVQ(r0lo, c0);
    SHRQ(Imm(51), c0); 
    // c1 = r1 >> 51
    MOVQ(r1lo, c1);
    SHRQ(Imm(51), c1); 
    // c2 = r2 >> 51
    MOVQ(r2lo, c2);
    SHRQ(Imm(51), c2); 
    // c3 = r3 >> 51
    MOVQ(r3lo, c3);
    SHRQ(Imm(51), c3); 
    // c4 = r4 >> 51
    MOVQ(r4lo, c4);
    SHRQ(Imm(51), c4);
    maskAndAdd(r0lo, maskLow51Bits, c4, 19);
    maskAndAdd(r1lo, maskLow51Bits, c0, 1);
    maskAndAdd(r2lo, maskLow51Bits, c1, 1);
    maskAndAdd(r3lo, maskLow51Bits, c2, 1);
    maskAndAdd(r4lo, maskLow51Bits, c3, 1);

    Comment("Store output");
    var @out = Dereference(Param("out"));
    Store(r0lo, @out.Field("l0"));
    Store(r1lo, @out.Field("l1"));
    Store(r2lo, @out.Field("l2"));
    Store(r3lo, @out.Field("l3"));
    Store(r4lo, @out.Field("l4"));

    RET();

}

// mul64 sets r to i * aX * bX.
private static void mul64(uint128 r, nint i, namedComponent aX, namedComponent bX) => func((_, panic, _) => {
    switch (i) {
        case 1: 
            Comment(fmt.Sprintf("%s = %s×%s", r, aX, bX));
            Load(aX, RAX);
            break;
        case 2: 
            Comment(fmt.Sprintf("%s = 2×%s×%s", r, aX, bX));
            Load(aX, RAX);
            SHLQ(Imm(1), RAX);
            break;
        default: 
            panic("unsupported i value");
            break;
    }
    MULQ(mustAddr(bX)); // RDX, RAX = RAX * bX
    MOVQ(RAX, r.lo);
    MOVQ(RDX, r.hi);

});

// addMul64 sets r to r + i * aX * bX.
private static void addMul64(uint128 r, ulong i, namedComponent aX, namedComponent bX) {
    switch (i) {
        case 1: 
            Comment(fmt.Sprintf("%s += %s×%s", r, aX, bX));
            Load(aX, RAX);
            break;
        default: 
            Comment(fmt.Sprintf("%s += %d×%s×%s", r, i, aX, bX));
            IMUL3Q(Imm(i), Load(aX, GP64()), RAX);
            break;
    }
    MULQ(mustAddr(bX)); // RDX, RAX = RAX * bX
    ADDQ(RAX, r.lo);
    ADCQ(RDX, r.hi);

}

// shiftRightBy51 returns r >> 51 and r.lo.
//
// After this function is called, the uint128 may not be used anymore.
private static (GPVirtual, GPVirtual) shiftRightBy51(ptr<uint128> _addr_r) {
    GPVirtual @out = default;
    GPVirtual lo = default;
    ref uint128 r = ref _addr_r.val;

    out = r.hi;
    lo = r.lo;
    SHLQ(Imm(64 - 51), r.lo, r.hi);
    (r.lo, r.hi) = (null, null);    return ;
}

// maskAndAdd sets r = r&mask + c*i.
private static void maskAndAdd(GPVirtual r, GPVirtual mask, GPVirtual c, ulong i) {
    ANDQ(mask, r);
    if (i != 1) {
        IMUL3Q(Imm(i), c, c);
    }
    ADDQ(c, r);

}

private static Op mustAddr(Component c) => func((_, panic, _) => {
    var (b, err) = c.Resolve();
    if (err != null) {
        panic(err);
    }
    return b.Addr;

});

} // end main_package
