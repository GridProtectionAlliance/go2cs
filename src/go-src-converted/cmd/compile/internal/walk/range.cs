// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package walk -- go2cs converted at 2022 March 06 23:12:00 UTC
// import "cmd/compile/internal/walk" ==> using walk = go.cmd.compile.@internal.walk_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\walk\range.go
using utf8 = go.unicode.utf8_package;

using @base = go.cmd.compile.@internal.@base_package;
using ir = go.cmd.compile.@internal.ir_package;
using reflectdata = go.cmd.compile.@internal.reflectdata_package;
using ssagen = go.cmd.compile.@internal.ssagen_package;
using typecheck = go.cmd.compile.@internal.typecheck_package;
using types = go.cmd.compile.@internal.types_package;
using sys = go.cmd.@internal.sys_package;

namespace go.cmd.compile.@internal;

public static partial class walk_package {

private static bool cheapComputableIndex(long width) {

    // MIPS does not have R+R addressing
    // Arm64 may lack ability to generate this code in our assembler,
    // but the architecture supports it.
    if (ssagen.Arch.LinkArch.Family == sys.PPC64 || ssagen.Arch.LinkArch.Family == sys.S390X) 
        return width == 1;
    else if (ssagen.Arch.LinkArch.Family == sys.AMD64 || ssagen.Arch.LinkArch.Family == sys.I386 || ssagen.Arch.LinkArch.Family == sys.ARM64 || ssagen.Arch.LinkArch.Family == sys.ARM) 
        switch (width) {
            case 1: 

            case 2: 

            case 4: 

            case 8: 
                return true;
                break;
        }
        return false;

}

// walkRange transforms various forms of ORANGE into
// simpler forms.  The result must be assigned back to n.
// Node n may also be modified in place, and may also be
// the returned node.
private static ir.Node walkRange(ptr<ir.RangeStmt> _addr_nrange) {
    ref ir.RangeStmt nrange = ref _addr_nrange.val;

    if (isMapClear(_addr_nrange)) {
        var m = nrange.X;
        var lno = ir.SetPos(m);
        var n = mapClear(m);
        @base.Pos = lno;
        return n;
    }
    var nfor = ir.NewForStmt(nrange.Pos(), null, null, null, null);
    nfor.SetInit(nrange.Init());
    nfor.Label = nrange.Label; 

    // variable name conventions:
    //    ohv1, hv1, hv2: hidden (old) val 1, 2
    //    ha, hit: hidden aggregate, iterator
    //    hn, hp: hidden len, pointer
    //    hb: hidden bool
    //    a, v1, v2: not hidden aggregate, val 1, 2

    var a = nrange.X;
    var t = typecheck.RangeExprType(a.Type());
    lno = ir.SetPos(a);

    var v1 = nrange.Key;
    var v2 = nrange.Value;

    if (ir.IsBlank(v2)) {
        v2 = null;
    }
    if (ir.IsBlank(v1) && v2 == null) {
        v1 = null;
    }
    if (v1 == null && v2 != null) {
        @base.Fatalf("walkRange: v2 != nil while v1 == nil");
    }
    ptr<ir.IfStmt> ifGuard;

    slice<ir.Node> body = default;
    slice<ir.Node> init = default;

    if (t.Kind() == types.TARRAY || t.Kind() == types.TSLICE) 
        {
            var nn = arrayClear(_addr_nrange, v1, v2, a);

            if (nn != null) {
                @base.Pos = lno;
                return nn;
            } 

            // order.stmt arranged for a copy of the array/slice variable if needed.

        } 

        // order.stmt arranged for a copy of the array/slice variable if needed.
        var ha = a;

        var hv1 = typecheck.Temp(types.Types[types.TINT]);
        var hn = typecheck.Temp(types.Types[types.TINT]);

        init = append(init, ir.NewAssignStmt(@base.Pos, hv1, null));
        init = append(init, ir.NewAssignStmt(@base.Pos, hn, ir.NewUnaryExpr(@base.Pos, ir.OLEN, ha)));

        nfor.Cond = ir.NewBinaryExpr(@base.Pos, ir.OLT, hv1, hn);
        nfor.Post = ir.NewAssignStmt(@base.Pos, hv1, ir.NewBinaryExpr(@base.Pos, ir.OADD, hv1, ir.NewInt(1))); 

        // for range ha { body }
        if (v1 == null) {
            break;
        }
        if (v2 == null) {
            body = new slice<ir.Node>(new ir.Node[] { ir.NewAssignStmt(base.Pos,v1,hv1) });
            break;
        }
        if (cheapComputableIndex(t.Elem().Width)) { 
            // v1, v2 = hv1, ha[hv1]
            var tmp = ir.NewIndexExpr(@base.Pos, ha, hv1);
            tmp.SetBounded(true); 
            // Use OAS2 to correctly handle assignments
            // of the form "v1, a[v1] := range".
            a = ir.NewAssignListStmt(@base.Pos, ir.OAS2, new slice<ir.Node>(new ir.Node[] { v1, v2 }), new slice<ir.Node>(new ir.Node[] { hv1, tmp }));
            body = new slice<ir.Node>(new ir.Node[] { a });
            break;

        }
        ifGuard = ir.NewIfStmt(@base.Pos, null, null, null);
        ifGuard.Cond = ir.NewBinaryExpr(@base.Pos, ir.OLT, hv1, hn);
        nfor.SetOp(ir.OFORUNTIL);

        var hp = typecheck.Temp(types.NewPtr(t.Elem()));
        tmp = ir.NewIndexExpr(@base.Pos, ha, ir.NewInt(0));
        tmp.SetBounded(true);
        init = append(init, ir.NewAssignStmt(@base.Pos, hp, typecheck.NodAddr(tmp))); 

        // Use OAS2 to correctly handle assignments
        // of the form "v1, a[v1] := range".
        a = ir.NewAssignListStmt(@base.Pos, ir.OAS2, new slice<ir.Node>(new ir.Node[] { v1, v2 }), new slice<ir.Node>(new ir.Node[] { hv1, ir.NewStarExpr(base.Pos,hp) }));
        body = append(body, a); 

        // Advance pointer as part of the late increment.
        //
        // This runs *after* the condition check, so we know
        // advancing the pointer is safe and won't go past the
        // end of the allocation.
        var @as = ir.NewAssignStmt(@base.Pos, hp, addptr(hp, t.Elem().Width));
        nfor.Late = new slice<ir.Node>(new ir.Node[] { typecheck.Stmt(as) });
    else if (t.Kind() == types.TMAP) 
        // order.stmt allocated the iterator for us.
        // we only use a once, so no copy needed.
        ha = a;

        var hit = nrange.Prealloc;
        var th = hit.Type(); 
        // depends on layout of iterator struct.
        // See cmd/compile/internal/reflectdata/reflect.go:MapIterType
        var keysym = th.Field(0).Sym;
        var elemsym = th.Field(1).Sym; // ditto

        var fn = typecheck.LookupRuntime("mapiterinit");

        fn = typecheck.SubstArgTypes(fn, t.Key(), t.Elem(), th);
        init = append(init, mkcallstmt1(fn, reflectdata.TypePtr(t), ha, typecheck.NodAddr(hit)));
        nfor.Cond = ir.NewBinaryExpr(@base.Pos, ir.ONE, ir.NewSelectorExpr(@base.Pos, ir.ODOT, hit, keysym), typecheck.NodNil());

        fn = typecheck.LookupRuntime("mapiternext");
        fn = typecheck.SubstArgTypes(fn, th);
        nfor.Post = mkcallstmt1(fn, typecheck.NodAddr(hit));

        var key = ir.NewStarExpr(@base.Pos, ir.NewSelectorExpr(@base.Pos, ir.ODOT, hit, keysym));
        if (v1 == null) {
            body = null;
        }
        else if (v2 == null) {
            body = new slice<ir.Node>(new ir.Node[] { ir.NewAssignStmt(base.Pos,v1,key) });
        }
        else
 {
            var elem = ir.NewStarExpr(@base.Pos, ir.NewSelectorExpr(@base.Pos, ir.ODOT, hit, elemsym));
            a = ir.NewAssignListStmt(@base.Pos, ir.OAS2, new slice<ir.Node>(new ir.Node[] { v1, v2 }), new slice<ir.Node>(new ir.Node[] { key, elem }));
            body = new slice<ir.Node>(new ir.Node[] { a });
        }
    else if (t.Kind() == types.TCHAN) 
        // order.stmt arranged for a copy of the channel variable.
        ha = a;

        hv1 = typecheck.Temp(t.Elem());
        hv1.SetTypecheck(1);
        if (t.Elem().HasPointers()) {
            init = append(init, ir.NewAssignStmt(@base.Pos, hv1, null));
        }
        var hb = typecheck.Temp(types.Types[types.TBOOL]);

        nfor.Cond = ir.NewBinaryExpr(@base.Pos, ir.ONE, hb, ir.NewBool(false));
        ir.Node lhs = new slice<ir.Node>(new ir.Node[] { hv1, hb });
        ir.Node rhs = new slice<ir.Node>(new ir.Node[] { ir.NewUnaryExpr(base.Pos,ir.ORECV,ha) });
        a = ir.NewAssignListStmt(@base.Pos, ir.OAS2RECV, lhs, rhs);
        a.SetTypecheck(1);
        nfor.Cond = ir.InitExpr(new slice<ir.Node>(new ir.Node[] { a }), nfor.Cond);
        if (v1 == null) {
            body = null;
        }
        else
 {
            body = new slice<ir.Node>(new ir.Node[] { ir.NewAssignStmt(base.Pos,v1,hv1) });
        }
        body = append(body, ir.NewAssignStmt(@base.Pos, hv1, null));
    else if (t.Kind() == types.TSTRING) 
        // Transform string range statements like "for v1, v2 = range a" into
        //
        // ha := a
        // for hv1 := 0; hv1 < len(ha); {
        //   hv1t := hv1
        //   hv2 := rune(ha[hv1])
        //   if hv2 < utf8.RuneSelf {
        //      hv1++
        //   } else {
        //      hv2, hv1 = decoderune(ha, hv1)
        //   }
        //   v1, v2 = hv1t, hv2
        //   // original body
        // }

        // order.stmt arranged for a copy of the string variable.
        ha = a;

        hv1 = typecheck.Temp(types.Types[types.TINT]);
        var hv1t = typecheck.Temp(types.Types[types.TINT]);
        var hv2 = typecheck.Temp(types.RuneType); 

        // hv1 := 0
        init = append(init, ir.NewAssignStmt(@base.Pos, hv1, null)); 

        // hv1 < len(ha)
        nfor.Cond = ir.NewBinaryExpr(@base.Pos, ir.OLT, hv1, ir.NewUnaryExpr(@base.Pos, ir.OLEN, ha));

        if (v1 != null) { 
            // hv1t = hv1
            body = append(body, ir.NewAssignStmt(@base.Pos, hv1t, hv1));

        }
        var nind = ir.NewIndexExpr(@base.Pos, ha, hv1);
        nind.SetBounded(true);
        body = append(body, ir.NewAssignStmt(@base.Pos, hv2, typecheck.Conv(nind, types.RuneType))); 

        // if hv2 < utf8.RuneSelf
        var nif = ir.NewIfStmt(@base.Pos, null, null, null);
        nif.Cond = ir.NewBinaryExpr(@base.Pos, ir.OLT, hv2, ir.NewInt(utf8.RuneSelf)); 

        // hv1++
        nif.Body = new slice<ir.Node>(new ir.Node[] { ir.NewAssignStmt(base.Pos,hv1,ir.NewBinaryExpr(base.Pos,ir.OADD,hv1,ir.NewInt(1))) }); 

        // } else {
        // hv2, hv1 = decoderune(ha, hv1)
        fn = typecheck.LookupRuntime("decoderune");
        var call = mkcall1(fn, fn.Type().Results(), _addr_nif.Else, ha, hv1);
        a = ir.NewAssignListStmt(@base.Pos, ir.OAS2, new slice<ir.Node>(new ir.Node[] { hv2, hv1 }), new slice<ir.Node>(new ir.Node[] { call }));
        nif.Else.Append(a);

        body = append(body, nif);

        if (v1 != null) {
            if (v2 != null) { 
                // v1, v2 = hv1t, hv2
                a = ir.NewAssignListStmt(@base.Pos, ir.OAS2, new slice<ir.Node>(new ir.Node[] { v1, v2 }), new slice<ir.Node>(new ir.Node[] { hv1t, hv2 }));
                body = append(body, a);

            }
            else
 { 
                // v1 = hv1t
                body = append(body, ir.NewAssignStmt(@base.Pos, v1, hv1t));

            }

        }
    else 
        @base.Fatalf("walkRange");
        typecheck.Stmts(init);

    if (ifGuard != null) {
        ifGuard.PtrInit().Append(init);
        ifGuard = typecheck.Stmt(ifGuard)._<ptr<ir.IfStmt>>();
    }
    else
 {
        nfor.PtrInit().Append(init);
    }
    typecheck.Stmts(nfor.Cond.Init());

    nfor.Cond = typecheck.Expr(nfor.Cond);
    nfor.Cond = typecheck.DefaultLit(nfor.Cond, null);
    nfor.Post = typecheck.Stmt(nfor.Post);
    typecheck.Stmts(body);
    nfor.Body.Append(body);
    nfor.Body.Append(nrange.Body);

    n = nfor;
    if (ifGuard != null) {
        ifGuard.Body = new slice<ir.Node>(new ir.Node[] { n });
        n = ifGuard;
    }
    n = walkStmt(n);

    @base.Pos = lno;
    return n;

}

// isMapClear checks if n is of the form:
//
// for k := range m {
//   delete(m, k)
// }
//
// where == for keys of map m is reflexive.
private static bool isMapClear(ptr<ir.RangeStmt> _addr_n) {
    ref ir.RangeStmt n = ref _addr_n.val;

    if (@base.Flag.N != 0 || @base.Flag.Cfg.Instrumenting) {
        return false;
    }
    var t = n.X.Type();
    if (n.Op() != ir.ORANGE || t.Kind() != types.TMAP || n.Key == null || n.Value != null) {
        return false;
    }
    var k = n.Key; 
    // Require k to be a new variable name.
    if (!ir.DeclaredBy(k, n)) {
        return false;
    }
    if (len(n.Body) != 1) {
        return false;
    }
    var stmt = n.Body[0]; // only stmt in body
    if (stmt == null || stmt.Op() != ir.ODELETE) {
        return false;
    }
    var m = n.X;
    {
        ptr<ir.CallExpr> delete = stmt._<ptr<ir.CallExpr>>();

        if (!ir.SameSafeExpr(delete.Args[0], m) || !ir.SameSafeExpr(delete.Args[1], k)) {
            return false;
        }
    } 

    // Keys where equality is not reflexive can not be deleted from maps.
    if (!types.IsReflexive(t.Key())) {
        return false;
    }
    return true;

}

// mapClear constructs a call to runtime.mapclear for the map m.
private static ir.Node mapClear(ir.Node m) {
    var t = m.Type(); 

    // instantiate mapclear(typ *type, hmap map[any]any)
    var fn = typecheck.LookupRuntime("mapclear");
    fn = typecheck.SubstArgTypes(fn, t.Key(), t.Elem());
    var n = mkcallstmt1(fn, reflectdata.TypePtr(t), m);
    return walkStmt(typecheck.Stmt(n));

}

// Lower n into runtime·memclr if possible, for
// fast zeroing of slices and arrays (issue 5373).
// Look for instances of
//
// for i := range a {
//     a[i] = zero
// }
//
// in which the evaluation of a is side-effect-free.
//
// Parameters are as in walkRange: "for v1, v2 = range a".
private static ir.Node arrayClear(ptr<ir.RangeStmt> _addr_loop, ir.Node v1, ir.Node v2, ir.Node a) {
    ref ir.RangeStmt loop = ref _addr_loop.val;

    if (@base.Flag.N != 0 || @base.Flag.Cfg.Instrumenting) {
        return null;
    }
    if (v1 == null || v2 != null) {
        return null;
    }
    if (len(loop.Body) != 1 || loop.Body[0] == null) {
        return null;
    }
    var stmt1 = loop.Body[0]; // only stmt in body
    if (stmt1.Op() != ir.OAS) {
        return null;
    }
    ptr<ir.AssignStmt> stmt = stmt1._<ptr<ir.AssignStmt>>();
    if (stmt.X.Op() != ir.OINDEX) {
        return null;
    }
    ptr<ir.IndexExpr> lhs = stmt.X._<ptr<ir.IndexExpr>>();

    if (!ir.SameSafeExpr(lhs.X, a) || !ir.SameSafeExpr(lhs.Index, v1)) {
        return null;
    }
    var elemsize = typecheck.RangeExprType(loop.X.Type()).Elem().Width;
    if (elemsize <= 0 || !ir.IsZero(stmt.Y)) {
        return null;
    }
    var n = ir.NewIfStmt(@base.Pos, null, null, null);
    n.Cond = ir.NewBinaryExpr(@base.Pos, ir.ONE, ir.NewUnaryExpr(@base.Pos, ir.OLEN, a), ir.NewInt(0)); 

    // hp = &a[0]
    var hp = typecheck.Temp(types.Types[types.TUNSAFEPTR]);

    var ix = ir.NewIndexExpr(@base.Pos, a, ir.NewInt(0));
    ix.SetBounded(true);
    var addr = typecheck.ConvNop(typecheck.NodAddr(ix), types.Types[types.TUNSAFEPTR]);
    n.Body.Append(ir.NewAssignStmt(@base.Pos, hp, addr)); 

    // hn = len(a) * sizeof(elem(a))
    var hn = typecheck.Temp(types.Types[types.TUINTPTR]);
    var mul = typecheck.Conv(ir.NewBinaryExpr(@base.Pos, ir.OMUL, ir.NewUnaryExpr(@base.Pos, ir.OLEN, a), ir.NewInt(elemsize)), types.Types[types.TUINTPTR]);
    n.Body.Append(ir.NewAssignStmt(@base.Pos, hn, mul));

    ir.Node fn = default;
    if (a.Type().Elem().HasPointers()) { 
        // memclrHasPointers(hp, hn)
        ir.CurFunc.SetWBPos(stmt.Pos());
        fn = mkcallstmt("memclrHasPointers", hp, hn);

    }
    else
 { 
        // memclrNoHeapPointers(hp, hn)
        fn = mkcallstmt("memclrNoHeapPointers", hp, hn);

    }
    n.Body.Append(fn); 

    // i = len(a) - 1
    v1 = ir.NewAssignStmt(@base.Pos, v1, ir.NewBinaryExpr(@base.Pos, ir.OSUB, ir.NewUnaryExpr(@base.Pos, ir.OLEN, a), ir.NewInt(1)));

    n.Body.Append(v1);

    n.Cond = typecheck.Expr(n.Cond);
    n.Cond = typecheck.DefaultLit(n.Cond, null);
    typecheck.Stmts(n.Body);
    return walkStmt(n);

}

// addptr returns (*T)(uintptr(p) + n).
private static ir.Node addptr(ir.Node p, long n) {
    var t = p.Type();

    p = ir.NewConvExpr(@base.Pos, ir.OCONVNOP, null, p);
    p.SetType(types.Types[types.TUINTPTR]);

    p = ir.NewBinaryExpr(@base.Pos, ir.OADD, p, ir.NewInt(n));

    p = ir.NewConvExpr(@base.Pos, ir.OCONVNOP, null, p);
    p.SetType(t);

    return p;
}

} // end walk_package
