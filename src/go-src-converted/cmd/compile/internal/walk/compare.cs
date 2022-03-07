// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package walk -- go2cs converted at 2022 March 06 23:11:37 UTC
// import "cmd/compile/internal/walk" ==> using walk = go.cmd.compile.@internal.walk_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\walk\compare.go
using binary = go.encoding.binary_package;
using constant = go.go.constant_package;

using @base = go.cmd.compile.@internal.@base_package;
using ir = go.cmd.compile.@internal.ir_package;
using reflectdata = go.cmd.compile.@internal.reflectdata_package;
using ssagen = go.cmd.compile.@internal.ssagen_package;
using typecheck = go.cmd.compile.@internal.typecheck_package;
using types = go.cmd.compile.@internal.types_package;
using sys = go.cmd.@internal.sys_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class walk_package {

    // The result of walkCompare MUST be assigned back to n, e.g.
    //     n.Left = walkCompare(n.Left, init)
private static ir.Node walkCompare(ptr<ir.BinaryExpr> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.BinaryExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    if (n.X.Type().IsInterface() && n.Y.Type().IsInterface() && n.X.Op() != ir.ONIL && n.Y.Op() != ir.ONIL) {
        return walkCompareInterface(_addr_n, _addr_init);
    }
    if (n.X.Type().IsString() && n.Y.Type().IsString()) {
        return walkCompareString(_addr_n, _addr_init);
    }
    n.X = walkExpr(n.X, init);
    n.Y = walkExpr(n.Y, init); 

    // Given mixed interface/concrete comparison,
    // rewrite into types-equal && data-equal.
    // This is efficient, avoids allocations, and avoids runtime calls.
    if (n.X.Type().IsInterface() != n.Y.Type().IsInterface()) { 
        // Preserve side-effects in case of short-circuiting; see #32187.
        var l = cheapExpr(n.X, init);
        var r = cheapExpr(n.Y, init); 
        // Swap so that l is the interface value and r is the concrete value.
        if (n.Y.Type().IsInterface()) {
            (l, r) = (r, l);
        }
        var eq = n.Op();
        var andor = ir.OOROR;
        if (eq == ir.OEQ) {
            andor = ir.OANDAND;
        }
        ir.Node eqtype = default;
        var tab = ir.NewUnaryExpr(@base.Pos, ir.OITAB, l);
        var rtyp = reflectdata.TypePtr(r.Type());
        if (l.Type().IsEmptyInterface()) {
            tab.SetType(types.NewPtr(types.Types[types.TUINT8]));
            tab.SetTypecheck(1);
            eqtype = ir.NewBinaryExpr(@base.Pos, eq, tab, rtyp);
        }
        else
 {
            var nonnil = ir.NewBinaryExpr(@base.Pos, brcom(eq), typecheck.NodNil(), tab);
            var match = ir.NewBinaryExpr(@base.Pos, eq, itabType(tab), rtyp);
            eqtype = ir.NewLogicalExpr(@base.Pos, andor, nonnil, match);
        }
        var eqdata = ir.NewBinaryExpr(@base.Pos, eq, ifaceData(n.Pos(), l, r.Type()), r); 
        // Put it all together.
        var expr = ir.NewLogicalExpr(@base.Pos, andor, eqtype, eqdata);
        return finishCompare(_addr_n, expr, _addr_init);

    }
    var t = n.X.Type();
    bool inline = default;

    var maxcmpsize = int64(4);
    var unalignedLoad = canMergeLoads();
    if (unalignedLoad) { 
        // Keep this low enough to generate less code than a function call.
        maxcmpsize = 2 * int64(ssagen.Arch.LinkArch.RegSize);

    }

    if (t.Kind() == types.TARRAY) 
        // We can compare several elements at once with 2/4/8 byte integer compares
        inline = t.NumElem() <= 1 || (types.IsSimple[t.Elem().Kind()] && (t.NumElem() <= 4 || t.Elem().Width * t.NumElem() <= maxcmpsize));
    else if (t.Kind() == types.TSTRUCT) 
        inline = t.NumComponents(types.IgnoreBlankFields) <= 4;
    else 
        if (@base.Debug.Libfuzzer != 0 && t.IsInteger()) {
            n.X = cheapExpr(n.X, init);
            n.Y = cheapExpr(n.Y, init); 

            // If exactly one comparison operand is
            // constant, invoke the constcmp functions
            // instead, and arrange for the constant
            // operand to be the first argument.
            l = n.X;
            r = n.Y;
            if (r.Op() == ir.OLITERAL) {
                (l, r) = (r, l);
            }
            var constcmp = l.Op() == ir.OLITERAL && r.Op() != ir.OLITERAL;

            @string fn = default;
            ptr<types.Type> paramType;
            switch (t.Size()) {
                case 1: 
                    fn = "libfuzzerTraceCmp1";
                    if (constcmp) {
                        fn = "libfuzzerTraceConstCmp1";
                    }
                    paramType = types.Types[types.TUINT8];

                    break;
                case 2: 
                    fn = "libfuzzerTraceCmp2";
                    if (constcmp) {
                        fn = "libfuzzerTraceConstCmp2";
                    }
                    paramType = types.Types[types.TUINT16];

                    break;
                case 4: 
                    fn = "libfuzzerTraceCmp4";
                    if (constcmp) {
                        fn = "libfuzzerTraceConstCmp4";
                    }
                    paramType = types.Types[types.TUINT32];

                    break;
                case 8: 
                    fn = "libfuzzerTraceCmp8";
                    if (constcmp) {
                        fn = "libfuzzerTraceConstCmp8";
                    }
                    paramType = types.Types[types.TUINT64];

                    break;
                default: 
                    @base.Fatalf("unexpected integer size %d for %v", t.Size(), t);
                    break;
            }
            init.Append(mkcall(fn, null, init, tracecmpArg(l, paramType, _addr_init), tracecmpArg(r, paramType, _addr_init)));

        }
        return n;
        var cmpl = n.X;
    while (cmpl != null && cmpl.Op() == ir.OCONVNOP) {
        cmpl = cmpl._<ptr<ir.ConvExpr>>().X;
    }
    var cmpr = n.Y;
    while (cmpr != null && cmpr.Op() == ir.OCONVNOP) {
        cmpr = cmpr._<ptr<ir.ConvExpr>>().X;
    } 

    // Chose not to inline. Call equality function directly.
    if (!inline) { 
        // eq algs take pointers; cmpl and cmpr must be addressable
        if (!ir.IsAddressable(cmpl) || !ir.IsAddressable(cmpr)) {
            @base.Fatalf("arguments of comparison must be lvalues - %v %v", cmpl, cmpr);
        }
        var (fn, needsize) = eqFor(_addr_t);
        var call = ir.NewCallExpr(@base.Pos, ir.OCALL, fn, null);
        call.Args.Append(typecheck.NodAddr(cmpl));
        call.Args.Append(typecheck.NodAddr(cmpr));
        if (needsize) {
            call.Args.Append(ir.NewInt(t.Width));
        }
        var res = ir.Node(call);
        if (n.Op() != ir.OEQ) {
            res = ir.NewUnaryExpr(@base.Pos, ir.ONOT, res);
        }
        return finishCompare(_addr_n, res, _addr_init);

    }
    andor = ir.OANDAND;
    if (n.Op() == ir.ONE) {
        andor = ir.OOROR;
    }
    expr = default;
    Action<ir.Node, ir.Node> compare = (el, er) => {
        var a = ir.NewBinaryExpr(@base.Pos, n.Op(), el, er);
        if (expr == null) {
            expr = a;
        }
        else
 {
            expr = ir.NewLogicalExpr(@base.Pos, andor, expr, a);
        }
    };
    cmpl = safeExpr(cmpl, init);
    cmpr = safeExpr(cmpr, init);
    if (t.IsStruct()) {
        foreach (var (_, f) in t.Fields().Slice()) {
            var sym = f.Sym;
            if (sym.IsBlank()) {
                continue;
            }
            compare(ir.NewSelectorExpr(@base.Pos, ir.OXDOT, cmpl, sym), ir.NewSelectorExpr(@base.Pos, ir.OXDOT, cmpr, sym));

        }
    else
    } {
        var step = int64(1);
        var remains = t.NumElem() * t.Elem().Width;
        var combine64bit = unalignedLoad && types.RegSize == 8 && t.Elem().Width <= 4 && t.Elem().IsInteger();
        var combine32bit = unalignedLoad && t.Elem().Width <= 2 && t.Elem().IsInteger();
        var combine16bit = unalignedLoad && t.Elem().Width == 1 && t.Elem().IsInteger();
        {
            var i = int64(0);

            while (remains > 0) {
                ptr<types.Type> convType;

                if (remains >= 8 && combine64bit) 
                    convType = types.Types[types.TINT64];
                    step = 8 / t.Elem().Width;
                else if (remains >= 4 && combine32bit) 
                    convType = types.Types[types.TUINT32];
                    step = 4 / t.Elem().Width;
                else if (remains >= 2 && combine16bit) 
                    convType = types.Types[types.TUINT16];
                    step = 2 / t.Elem().Width;
                else 
                    step = 1;
                                if (step == 1) {
                    compare(ir.NewIndexExpr(@base.Pos, cmpl, ir.NewInt(i)), ir.NewIndexExpr(@base.Pos, cmpr, ir.NewInt(i)));
                    i++;
                    remains -= t.Elem().Width;
                }
                else
 {
                    var elemType = t.Elem().ToUnsigned();
                    var cmplw = ir.Node(ir.NewIndexExpr(@base.Pos, cmpl, ir.NewInt(i)));
                    cmplw = typecheck.Conv(cmplw, elemType); // convert to unsigned
                    cmplw = typecheck.Conv(cmplw, convType); // widen
                    var cmprw = ir.Node(ir.NewIndexExpr(@base.Pos, cmpr, ir.NewInt(i)));
                    cmprw = typecheck.Conv(cmprw, elemType);
                    cmprw = typecheck.Conv(cmprw, convType); 
                    // For code like this:  uint32(s[0]) | uint32(s[1])<<8 | uint32(s[2])<<16 ...
                    // ssa will generate a single large load.
                    for (var offset = int64(1); offset < step; offset++) {
                        var lb = ir.Node(ir.NewIndexExpr(@base.Pos, cmpl, ir.NewInt(i + offset)));
                        lb = typecheck.Conv(lb, elemType);
                        lb = typecheck.Conv(lb, convType);
                        lb = ir.NewBinaryExpr(@base.Pos, ir.OLSH, lb, ir.NewInt(8 * t.Elem().Width * offset));
                        cmplw = ir.NewBinaryExpr(@base.Pos, ir.OOR, cmplw, lb);
                        var rb = ir.Node(ir.NewIndexExpr(@base.Pos, cmpr, ir.NewInt(i + offset)));
                        rb = typecheck.Conv(rb, elemType);
                        rb = typecheck.Conv(rb, convType);
                        rb = ir.NewBinaryExpr(@base.Pos, ir.OLSH, rb, ir.NewInt(8 * t.Elem().Width * offset));
                        cmprw = ir.NewBinaryExpr(@base.Pos, ir.OOR, cmprw, rb);
                    }
                    compare(cmplw, cmprw);
                    i += step;
                    remains -= step * t.Elem().Width;

                }
            }
        }

    }
    if (expr == null) {
        expr = ir.NewBool(n.Op() == ir.OEQ); 
        // We still need to use cmpl and cmpr, in case they contain
        // an expression which might panic. See issue 23837.
        t = typecheck.Temp(cmpl.Type());
        var a1 = typecheck.Stmt(ir.NewAssignStmt(@base.Pos, t, cmpl));
        var a2 = typecheck.Stmt(ir.NewAssignStmt(@base.Pos, t, cmpr));
        init.Append(a1, a2);

    }
    return finishCompare(_addr_n, expr, _addr_init);

}

private static ir.Node walkCompareInterface(ptr<ir.BinaryExpr> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.BinaryExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    n.Y = cheapExpr(n.Y, init);
    n.X = cheapExpr(n.X, init);
    var (eqtab, eqdata) = reflectdata.EqInterface(n.X, n.Y);
    ir.Node cmp = default;
    if (n.Op() == ir.OEQ) {
        cmp = ir.NewLogicalExpr(@base.Pos, ir.OANDAND, eqtab, eqdata);
    }
    else
 {
        eqtab.SetOp(ir.ONE);
        cmp = ir.NewLogicalExpr(@base.Pos, ir.OOROR, eqtab, ir.NewUnaryExpr(@base.Pos, ir.ONOT, eqdata));
    }
    return finishCompare(_addr_n, cmp, _addr_init);

}

private static ir.Node walkCompareString(ptr<ir.BinaryExpr> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.BinaryExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;
 
    // Rewrite comparisons to short constant strings as length+byte-wise comparisons.
    ir.Node cs = default;    ir.Node ncs = default; // const string, non-const string
 // const string, non-const string

    if (ir.IsConst(n.X, constant.String) && ir.IsConst(n.Y, constant.String))     else if (ir.IsConst(n.X, constant.String)) 
        cs = n.X;
        ncs = n.Y;
    else if (ir.IsConst(n.Y, constant.String)) 
        cs = n.Y;
        ncs = n.X;
        if (cs != null) {
        var cmp = n.Op(); 
        // Our comparison below assumes that the non-constant string
        // is on the left hand side, so rewrite "" cmp x to x cmp "".
        // See issue 24817.
        if (ir.IsConst(n.X, constant.String)) {
            cmp = brrev(cmp);
        }
        nint maxRewriteLen = 6; 
        // Some architectures can load unaligned byte sequence as 1 word.
        // So we can cover longer strings with the same amount of code.
        var canCombineLoads = canMergeLoads();
        var combine64bit = false;
        if (canCombineLoads) { 
            // Keep this low enough to generate less code than a function call.
            maxRewriteLen = 2 * ssagen.Arch.LinkArch.RegSize;
            combine64bit = ssagen.Arch.LinkArch.RegSize >= 8;

        }
        ir.Op and = default;

        if (cmp == ir.OEQ) 
            and = ir.OANDAND;
        else if (cmp == ir.ONE) 
            and = ir.OOROR;
        else 
            // Don't do byte-wise comparisons for <, <=, etc.
            // They're fairly complicated.
            // Length-only checks are ok, though.
            maxRewriteLen = 0;
                {
            var s = ir.StringVal(cs);

            if (len(s) <= maxRewriteLen) {
                if (len(s) > 0) {
                    ncs = safeExpr(ncs, init);
                }
                var r = ir.Node(ir.NewBinaryExpr(@base.Pos, cmp, ir.NewUnaryExpr(@base.Pos, ir.OLEN, ncs), ir.NewInt(int64(len(s)))));
                var remains = len(s);
                {
                    nint i = 0;

                    while (remains > 0) {
                        if (remains == 1 || !canCombineLoads) {
                            var cb = ir.NewInt(int64(s[i]));
                            var ncb = ir.NewIndexExpr(@base.Pos, ncs, ir.NewInt(int64(i)));
                            r = ir.NewLogicalExpr(@base.Pos, and, r, ir.NewBinaryExpr(@base.Pos, cmp, ncb, cb));
                            remains--;
                            i++;
                            continue;
                        }
                        nint step = default;
                        ptr<types.Type> convType;

                        if (remains >= 8 && combine64bit) 
                            convType = types.Types[types.TINT64];
                            step = 8;
                        else if (remains >= 4) 
                            convType = types.Types[types.TUINT32];
                            step = 4;
                        else if (remains >= 2) 
                            convType = types.Types[types.TUINT16];
                            step = 2;
                                                var ncsubstr = typecheck.Conv(ir.NewIndexExpr(@base.Pos, ncs, ir.NewInt(int64(i))), convType);
                        var csubstr = int64(s[i]); 
                        // Calculate large constant from bytes as sequence of shifts and ors.
                        // Like this:  uint32(s[0]) | uint32(s[1])<<8 | uint32(s[2])<<16 ...
                        // ssa will combine this into a single large load.
                        for (nint offset = 1; offset < step; offset++) {
                            var b = typecheck.Conv(ir.NewIndexExpr(@base.Pos, ncs, ir.NewInt(int64(i + offset))), convType);
                            b = ir.NewBinaryExpr(@base.Pos, ir.OLSH, b, ir.NewInt(int64(8 * offset)));
                            ncsubstr = ir.NewBinaryExpr(@base.Pos, ir.OOR, ncsubstr, b);
                            csubstr |= int64(s[i + offset]) << (int)(uint8(8 * offset));
                        }

                        var csubstrPart = ir.NewInt(csubstr); 
                        // Compare "step" bytes as once
                        r = ir.NewLogicalExpr(@base.Pos, and, r, ir.NewBinaryExpr(@base.Pos, cmp, csubstrPart, ncsubstr));
                        remains -= step;
                        i += step;

                    }

                }
                return finishCompare(_addr_n, r, _addr_init);

            }

        }

    }
    r = default;
    if (n.Op() == ir.OEQ || n.Op() == ir.ONE) { 
        // prepare for rewrite below
        n.X = cheapExpr(n.X, init);
        n.Y = cheapExpr(n.Y, init);
        var (eqlen, eqmem) = reflectdata.EqString(n.X, n.Y); 
        // quick check of len before full compare for == or !=.
        // memequal then tests equality up to length len.
        if (n.Op() == ir.OEQ) { 
            // len(left) == len(right) && memequal(left, right, len)
            r = ir.NewLogicalExpr(@base.Pos, ir.OANDAND, eqlen, eqmem);

        }
        else
 { 
            // len(left) != len(right) || !memequal(left, right, len)
            eqlen.SetOp(ir.ONE);
            r = ir.NewLogicalExpr(@base.Pos, ir.OOROR, eqlen, ir.NewUnaryExpr(@base.Pos, ir.ONOT, eqmem));

        }
    }
    else
 { 
        // sys_cmpstring(s1, s2) :: 0
        r = mkcall("cmpstring", types.Types[types.TINT], init, typecheck.Conv(n.X, types.Types[types.TSTRING]), typecheck.Conv(n.Y, types.Types[types.TSTRING]));
        r = ir.NewBinaryExpr(@base.Pos, n.Op(), r, ir.NewInt(0));

    }
    return finishCompare(_addr_n, r, _addr_init);

}

// The result of finishCompare MUST be assigned back to n, e.g.
//     n.Left = finishCompare(n.Left, x, r, init)
private static ir.Node finishCompare(ptr<ir.BinaryExpr> _addr_n, ir.Node r, ptr<ir.Nodes> _addr_init) {
    ref ir.BinaryExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    r = typecheck.Expr(r);
    r = typecheck.Conv(r, n.Type());
    r = walkExpr(r, init);
    return r;
}

private static (ir.Node, bool) eqFor(ptr<types.Type> _addr_t) {
    ir.Node n = default;
    bool needsize = default;
    ref types.Type t = ref _addr_t.val;
 
    // Should only arrive here with large memory or
    // a struct/array containing a non-memory field/element.
    // Small memory is handled inline, and single non-memory
    // is handled by walkCompare.
    {
        var (a, _) = types.AlgType(t);


        if (a == types.AMEM) 
            var n = typecheck.LookupRuntime("memequal");
            n = typecheck.SubstArgTypes(n, t, t);
            return (n, true);
        else if (a == types.ASPECIAL) 
            var sym = reflectdata.TypeSymPrefix(".eq", t); 
            // TODO(austin): This creates an ir.Name with a nil Func.
            n = typecheck.NewName(sym);
            ir.MarkFunc(n);
            n.SetType(types.NewSignature(types.NoPkg, null, null, new slice<ptr<types.Field>>(new ptr<types.Field>[] { types.NewField(base.Pos,nil,types.NewPtr(t)), types.NewField(base.Pos,nil,types.NewPtr(t)) }), new slice<ptr<types.Field>>(new ptr<types.Field>[] { types.NewField(base.Pos,nil,types.Types[types.TBOOL]) })));
            return (n, false);

    }
    @base.Fatalf("eqFor %v", t);
    return (null, false);

}

// brcom returns !(op).
// For example, brcom(==) is !=.
private static ir.Op brcom(ir.Op op) {

    if (op == ir.OEQ) 
        return ir.ONE;
    else if (op == ir.ONE) 
        return ir.OEQ;
    else if (op == ir.OLT) 
        return ir.OGE;
    else if (op == ir.OGT) 
        return ir.OLE;
    else if (op == ir.OLE) 
        return ir.OGT;
    else if (op == ir.OGE) 
        return ir.OLT;
        @base.Fatalf("brcom: no com for %v\n", op);
    return op;

}

// brrev returns reverse(op).
// For example, Brrev(<) is >.
private static ir.Op brrev(ir.Op op) {

    if (op == ir.OEQ) 
        return ir.OEQ;
    else if (op == ir.ONE) 
        return ir.ONE;
    else if (op == ir.OLT) 
        return ir.OGT;
    else if (op == ir.OGT) 
        return ir.OLT;
    else if (op == ir.OLE) 
        return ir.OGE;
    else if (op == ir.OGE) 
        return ir.OLE;
        @base.Fatalf("brrev: no rev for %v\n", op);
    return op;

}

private static ir.Node tracecmpArg(ir.Node n, ptr<types.Type> _addr_t, ptr<ir.Nodes> _addr_init) {
    ref types.Type t = ref _addr_t.val;
    ref ir.Nodes init = ref _addr_init.val;
 
    // Ugly hack to avoid "constant -1 overflows uintptr" errors, etc.
    if (n.Op() == ir.OLITERAL && n.Type().IsSigned() && ir.Int64Val(n) < 0) {
        n = copyExpr(n, n.Type(), init);
    }
    return typecheck.Conv(n, t);

}

// canMergeLoads reports whether the backend optimization passes for
// the current architecture can combine adjacent loads into a single
// larger, possibly unaligned, load. Note that currently the
// optimizations must be able to handle little endian byte order.
private static bool canMergeLoads() {

    if (ssagen.Arch.LinkArch.Family == sys.ARM64 || ssagen.Arch.LinkArch.Family == sys.AMD64 || ssagen.Arch.LinkArch.Family == sys.I386 || ssagen.Arch.LinkArch.Family == sys.S390X) 
        return true;
    else if (ssagen.Arch.LinkArch.Family == sys.PPC64) 
        // Load combining only supported on ppc64le.
        return ssagen.Arch.LinkArch.ByteOrder == binary.LittleEndian;
        return false;

}

} // end walk_package
