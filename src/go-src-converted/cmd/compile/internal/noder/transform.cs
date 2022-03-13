// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains transformation functions on nodes, which are the
// transformations that the typecheck package does that are distinct from the
// typechecking functionality. These transform functions are pared-down copies of
// the original typechecking functions, with all code removed that is related to:
//
//    - Detecting compile-time errors (already done by types2)
//    - Setting the actual type of existing nodes (already done based on
//      type info from types2)
//    - Dealing with untyped constants (which types2 has already resolved)
//
// Each of the transformation functions requires that node passed in has its type
// and typecheck flag set. If the transformation function replaces or adds new
// nodes, it will set the type and typecheck flag for those new nodes.

// package noder -- go2cs converted at 2022 March 13 06:27:47 UTC
// import "cmd/compile/internal/noder" ==> using noder = go.cmd.compile.@internal.noder_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\noder\transform.go
namespace go.cmd.compile.@internal;

using @base = cmd.compile.@internal.@base_package;
using ir = cmd.compile.@internal.ir_package;
using typecheck = cmd.compile.@internal.typecheck_package;
using types = cmd.compile.@internal.types_package;
using fmt = fmt_package;
using constant = go.constant_package;


// Transformation functions for expressions

// transformAdd transforms an addition operation (currently just addition of
// strings). Corresponds to the "binary operators" case in typecheck.typecheck1.

using System;
public static partial class noder_package {

private static ir.Node transformAdd(ptr<ir.BinaryExpr> _addr_n) {
    ref ir.BinaryExpr n = ref _addr_n.val;

    assert(n.Type() != null && n.Typecheck() == 1);
    var l = n.X;
    if (l.Type().IsString()) {
        ptr<ir.AddStringExpr> add;
        if (l.Op() == ir.OADDSTR) {
            add = l._<ptr<ir.AddStringExpr>>();
            add.SetPos(n.Pos());
        }
        else
 {
            add = ir.NewAddStringExpr(n.Pos(), new slice<ir.Node>(new ir.Node[] { l }));
        }
        var r = n.Y;
        if (r.Op() == ir.OADDSTR) {
            r = r._<ptr<ir.AddStringExpr>>();
            add.List.Append(r.List.Take());
        }
        else
 {
            add.List.Append(r);
        }
        typed(l.Type(), add);
        return add;
    }
    return n;
}

// Corresponds to typecheck.stringtoruneslit.
private static ir.Node stringtoruneslit(ptr<ir.ConvExpr> _addr_n) {
    ref ir.ConvExpr n = ref _addr_n.val;

    if (n.X.Op() != ir.OLITERAL || n.X.Val().Kind() != constant.String) {
        @base.Fatalf("stringtoarraylit %v", n);
    }
    slice<ir.Node> list = default;
    nint i = 0;
    var eltType = n.Type().Elem();
    foreach (var (_, r) in ir.StringVal(n.X)) {
        var elt = ir.NewKeyExpr(@base.Pos, ir.NewInt(int64(i)), ir.NewInt(int64(r))); 
        // Change from untyped int to the actual element type determined
        // by types2.  No need to change elt.Key, since the array indexes
        // are just used for setting up the element ordering.
        elt.Value.SetType(eltType);
        list = append(list, elt);
        i++;
    }    var nn = ir.NewCompLitExpr(@base.Pos, ir.OCOMPLIT, ir.TypeNode(n.Type()), null);
    nn.List = list;
    typed(n.Type(), nn); 
    // Need to transform the OCOMPLIT.
    return transformCompLit(_addr_nn);
}

// transformConv transforms an OCONV node as needed, based on the types involved,
// etc.  Corresponds to typecheck.tcConv.
private static ir.Node transformConv(ptr<ir.ConvExpr> _addr_n) {
    ref ir.ConvExpr n = ref _addr_n.val;

    var t = n.X.Type();
    var (op, _) = typecheck.Convertop(n.X.Op() == ir.OLITERAL, t, n.Type());
    n.SetOp(op);

    if (n.Op() == ir.OCONVNOP) 
        if (t.Kind() == n.Type().Kind()) {

            if (t.Kind() == types.TFLOAT32 || t.Kind() == types.TFLOAT64 || t.Kind() == types.TCOMPLEX64 || t.Kind() == types.TCOMPLEX128) 
                // Floating point casts imply rounding and
                // so the conversion must be kept.
                n.SetOp(ir.OCONV);
                    }
    else if (n.Op() == ir.OSTR2BYTES)     else if (n.Op() == ir.OSTR2RUNES) 
        if (n.X.Op() == ir.OLITERAL) {
            return stringtoruneslit(_addr_n);
        }
        return n;
}

// transformConvCall transforms a conversion call. Corresponds to the OTYPE part of
// typecheck.tcCall.
private static ir.Node transformConvCall(ptr<ir.CallExpr> _addr_n) {
    ref ir.CallExpr n = ref _addr_n.val;

    assert(n.Type() != null && n.Typecheck() == 1);
    var arg = n.Args[0];
    var n1 = ir.NewConvExpr(n.Pos(), ir.OCONV, null, arg);
    typed(n.X.Type(), n1);
    return transformConv(_addr_n1);
}

// transformCall transforms a normal function/method call. Corresponds to last half
// (non-conversion, non-builtin part) of typecheck.tcCall.
private static void transformCall(ptr<ir.CallExpr> _addr_n) {
    ref ir.CallExpr n = ref _addr_n.val;
 
    // n.Type() can be nil for calls with no return value
    assert(n.Typecheck() == 1);
    transformArgs(n);
    var l = n.X;
    var t = l.Type();


    if (l.Op() == ir.ODOTINTER) 
        n.SetOp(ir.OCALLINTER);
    else if (l.Op() == ir.ODOTMETH) 
        l = l._<ptr<ir.SelectorExpr>>();
        n.SetOp(ir.OCALLMETH);

        var tp = t.Recv().Type;

        if (l.X == null || !types.Identical(l.X.Type(), tp)) {
            @base.Fatalf("method receiver");
        }
    else 
        n.SetOp(ir.OCALLFUNC);
        typecheckaste(ir.OCALL, n.X, n.IsDDD, _addr_t.Params(), n.Args);
    if (t.NumResults() == 1) {
        n.SetType(l.Type().Results().Field(0).Type);

        if (n.Op() == ir.OCALLFUNC && n.X.Op() == ir.ONAME) {
            {
                ptr<ir.Name> sym = n.X._<ptr<ir.Name>>().Sym();

                if (types.IsRuntimePkg(sym.Pkg) && sym.Name == "getg") { 
                    // Emit code for runtime.getg() directly instead of calling function.
                    // Most such rewrites (for example the similar one for math.Sqrt) should be done in walk,
                    // so that the ordering pass can make sure to preserve the semantics of the original code
                    // (in particular, the exact time of the function call) by introducing temporaries.
                    // In this case, we know getg() always returns the same result within a given function
                    // and we want to avoid the temporaries, so we do the rewrite earlier than is typical.
                    n.SetOp(ir.OGETG);
                }

            }
        }
        return ;
    }
}

// transformCompare transforms a compare operation (currently just equals/not
// equals). Corresponds to the "comparison operators" case in
// typecheck.typecheck1, including tcArith.
private static void transformCompare(ptr<ir.BinaryExpr> _addr_n) {
    ref ir.BinaryExpr n = ref _addr_n.val;

    assert(n.Type() != null && n.Typecheck() == 1);
    if ((n.Op() == ir.OEQ || n.Op() == ir.ONE) && !types.Identical(n.X.Type(), n.Y.Type())) { 
        // Comparison is okay as long as one side is assignable to the
        // other. The only allowed case where the conversion is not CONVNOP is
        // "concrete == interface". In that case, check comparability of
        // the concrete type. The conversion allocates, so only do it if
        // the concrete type is huge.
        var l = n.X;
        var r = n.Y;
        var lt = l.Type();
        var rt = r.Type();
        var converted = false;
        if (rt.Kind() != types.TBLANK) {
            var (aop, _) = typecheck.Assignop(lt, rt);
            if (aop != ir.OXXX) {
                types.CalcSize(lt);
                if (rt.IsInterface() == lt.IsInterface() || lt.Width >= 1 << 16) {
                    l = ir.NewConvExpr(@base.Pos, aop, rt, l);
                    l.SetTypecheck(1);
                }
                converted = true;
            }
        }
        if (!converted && lt.Kind() != types.TBLANK) {
            (aop, _) = typecheck.Assignop(rt, lt);
            if (aop != ir.OXXX) {
                types.CalcSize(rt);
                if (rt.IsInterface() == lt.IsInterface() || rt.Width >= 1 << 16) {
                    r = ir.NewConvExpr(@base.Pos, aop, lt, r);
                    r.SetTypecheck(1);
                }
            }
        }
        (n.X, n.Y) = (l, r);
    }
}

// Corresponds to typecheck.implicitstar.
private static ir.Node implicitstar(ir.Node n) { 
    // insert implicit * if needed for fixed array
    var t = n.Type();
    if (!t.IsPtr()) {
        return n;
    }
    t = t.Elem();
    if (!t.IsArray()) {
        return n;
    }
    var star = ir.NewStarExpr(@base.Pos, n);
    star.SetImplicit(true);
    return typed(t, star);
}

// transformIndex transforms an index operation.  Corresponds to typecheck.tcIndex.
private static void transformIndex(ptr<ir.IndexExpr> _addr_n) {
    ref ir.IndexExpr n = ref _addr_n.val;

    assert(n.Type() != null && n.Typecheck() == 1);
    n.X = implicitstar(n.X);
    var l = n.X;
    var t = l.Type();
    if (t.Kind() == types.TMAP) {
        n.Index = assignconvfn(n.Index, _addr_t.Key());
        n.SetOp(ir.OINDEXMAP); 
        // Set type to just the map value, not (value, bool). This is
        // different from types2, but fits the later stages of the
        // compiler better.
        n.SetType(t.Elem());
        n.Assigned = false;
    }
}

// transformSlice transforms a slice operation.  Corresponds to typecheck.tcSlice.
private static void transformSlice(ptr<ir.SliceExpr> _addr_n) {
    ref ir.SliceExpr n = ref _addr_n.val;

    assert(n.Type() != null && n.Typecheck() == 1);
    var l = n.X;
    if (l.Type().IsArray()) {
        var addr = typecheck.NodAddr(n.X);
        addr.SetImplicit(true);
        typed(types.NewPtr(n.X.Type()), addr);
        n.X = addr;
        l = addr;
    }
    var t = l.Type();
    if (t.IsString()) {
        n.SetOp(ir.OSLICESTR);
    }
    else if (t.IsPtr() && t.Elem().IsArray()) {
        if (n.Op().IsSlice3()) {
            n.SetOp(ir.OSLICE3ARR);
        }
        else
 {
            n.SetOp(ir.OSLICEARR);
        }
    }
}

// Transformation functions for statements

// Corresponds to typecheck.checkassign.
private static void transformCheckAssign(ir.Node stmt, ir.Node n) {
    if (n.Op() == ir.OINDEXMAP) {
        ptr<ir.IndexExpr> n = n._<ptr<ir.IndexExpr>>();
        n.Assigned = true;
        return ;
    }
}

// Corresponds to typecheck.assign.
private static void transformAssign(ir.Node stmt, slice<ir.Node> lhs, slice<ir.Node> rhs) {
    Action<nint, ptr<types.Type>> checkLHS = (i, typ) => {
        transformCheckAssign(stmt, lhs[i]);
    };

    var cr = len(rhs);
    if (len(rhs) == 1) {
        {
            var rtyp__prev2 = rtyp;

            var rtyp = rhs[0].Type();

            if (rtyp != null && rtyp.IsFuncArgStruct()) {
                cr = rtyp.NumFields();
            }

            rtyp = rtyp__prev2;

        }
    }
assignOK:

    while (len(lhs) == 2 && cr == 1) {
        ptr<ir.AssignListStmt> stmt = stmt._<ptr<ir.AssignListStmt>>();
        var r = rhs[0];


        if (r.Op() == ir.OINDEXMAP) 
            stmt.SetOp(ir.OAS2MAPR);
        else if (r.Op() == ir.ORECV) 
            stmt.SetOp(ir.OAS2RECV);
        else if (r.Op() == ir.ODOTTYPE) 
            r = r._<ptr<ir.TypeAssertExpr>>();
            stmt.SetOp(ir.OAS2DOTTYPE);
            r.SetOp(ir.ODOTTYPE2);
        else 
            _breakassignOK = true;
            break;
                checkLHS(0, r.Type());
        checkLHS(1, types.UntypedBool);
        return ;
    }
    if (len(lhs) != cr) {
        {
            var i__prev1 = i;

            foreach (var (__i) in lhs) {
                i = __i;
                checkLHS(i, null);
            }

            i = i__prev1;
        }

        return ;
    }
    if (cr > len(rhs)) {
        stmt = stmt._<ptr<ir.AssignListStmt>>();
        stmt.SetOp(ir.OAS2FUNC);
        r = rhs[0]._<ptr<ir.CallExpr>>();
        r.Use = ir.CallUseList;
        rtyp = r.Type();

        {
            var i__prev1 = i;

            foreach (var (__i) in lhs) {
                i = __i;
                checkLHS(i, rtyp.Field(i).Type);
            }

            i = i__prev1;
        }

        return ;
    }
    {
        var i__prev1 = i;
        var r__prev1 = r;

        foreach (var (__i, __r) in rhs) {
            i = __i;
            r = __r;
            checkLHS(i, r.Type());
            if (lhs[i].Type() != null) {
                rhs[i] = assignconvfn(r, _addr_lhs[i].Type());
            }
        }
        i = i__prev1;
        r = r__prev1;
    }
}

// Corresponds to typecheck.typecheckargs.
private static void transformArgs(ir.InitNode n) {
    slice<ir.Node> list = default;
    switch (n.type()) {
        case ptr<ir.CallExpr> n:
            list = n.Args;
            if (n.IsDDD) {
                return ;
            }
            break;
        case ptr<ir.ReturnStmt> n:
            list = n.Results;
            break;
        default:
        {
            var n = n.type();
            @base.Fatalf("typecheckargs %+v", n.Op());
            break;
        }
    }
    if (len(list) != 1) {
        return ;
    }
    var t = list[0].Type();
    if (t == null || !t.IsFuncArgStruct()) {
        return ;
    }
    if (ir.Orig(n) == n) {
        n._<ir.OrigNode>().SetOrig(ir.SepCopy(n));
    }
    var @as = ir.NewAssignListStmt(@base.Pos, ir.OAS2, null, null);
    @as.Rhs.Append(list); 

    // If we're outside of function context, then this call will
    // be executed during the generated init function. However,
    // init.go hasn't yet created it. Instead, associate the
    // temporary variables with  InitTodoFunc for now, and init.go
    // will reassociate them later when it's appropriate.
    var @static = ir.CurFunc == null;
    if (static) {
        ir.CurFunc = typecheck.InitTodoFunc;
    }
    list = null;
    foreach (var (_, f) in t.FieldSlice()) {
        t = typecheck.Temp(f.Type);
        @as.PtrInit().Append(ir.NewDecl(@base.Pos, ir.ODCL, t));
        @as.Lhs.Append(t);
        list = append(list, t);
    }    if (static) {
        ir.CurFunc = null;
    }
    switch (n.type()) {
        case ptr<ir.CallExpr> n:
            n.Args = list;
            break;
        case ptr<ir.ReturnStmt> n:
            n.Results = list;
            break;

    }

    transformAssign(as, @as.Lhs, @as.Rhs);
    @as.SetTypecheck(1);
    n.PtrInit().Append(as);
}

// assignconvfn converts node n for assignment to type t. Corresponds to
// typecheck.assignconvfn.
private static ir.Node assignconvfn(ir.Node n, ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    if (t.Kind() == types.TBLANK) {
        return n;
    }
    if (types.Identical(n.Type(), t)) {
        return n;
    }
    var (op, _) = typecheck.Assignop(n.Type(), t);

    var r = ir.NewConvExpr(@base.Pos, op, t, n);
    r.SetTypecheck(1);
    r.SetImplicit(true);
    return r;
}

// Corresponds to typecheck.typecheckaste.
private static void typecheckaste(ir.Op op, ir.Node call, bool isddd, ptr<types.Type> _addr_tstruct, ir.Nodes nl) => func((defer, _, _) => {
    ref types.Type tstruct = ref _addr_tstruct.val;

    ptr<types.Type> t;
    nint i = default;

    var lno = @base.Pos;
    defer(() => {
        @base.Pos = lno;
    }());

    ir.Node n = default;
    if (len(nl) == 1) {
        n = nl[0];
    }
    i = 0;
    foreach (var (_, tl) in tstruct.Fields().Slice()) {
        t = tl.Type;
        if (tl.IsDDD()) {
            if (isddd) {
                n = nl[i];
                ir.SetPos(n);
                if (n.Type() != null) {
                    nl[i] = assignconvfn(n, t);
                }
                return ;
            } 

            // TODO(mdempsky): Make into ... call with implicit slice.
            while (i < len(nl)) {
                n = nl[i];
                ir.SetPos(n);
                if (n.Type() != null) {
                    nl[i] = assignconvfn(n, _addr_t.Elem());
                i++;
                }
            }

            return ;
        }
        n = nl[i];
        ir.SetPos(n);
        if (n.Type() != null) {
            nl[i] = assignconvfn(n, t);
        }
        i++;
    }
});

// transformSend transforms a send statement, converting the value to appropriate
// type for the channel, as needed. Corresponds of typecheck.tcSend.
private static void transformSend(ptr<ir.SendStmt> _addr_n) {
    ref ir.SendStmt n = ref _addr_n.val;

    n.Value = assignconvfn(n.Value, _addr_n.Chan.Type().Elem());
}

// transformReturn transforms a return node, by doing the needed assignments and
// any necessary conversions. Corresponds to typecheck.tcReturn()
private static void transformReturn(ptr<ir.ReturnStmt> _addr_rs) {
    ref ir.ReturnStmt rs = ref _addr_rs.val;

    transformArgs(rs);
    var nl = rs.Results;
    if (ir.HasNamedResults(ir.CurFunc) && len(nl) == 0) {
        return ;
    }
    typecheckaste(ir.ORETURN, null, false, _addr_ir.CurFunc.Type().Results(), nl);
}

// transformSelect transforms a select node, creating an assignment list as needed
// for each case. Corresponds to typecheck.tcSelect().
private static void transformSelect(ptr<ir.SelectStmt> _addr_sel) {
    ref ir.SelectStmt sel = ref _addr_sel.val;

    foreach (var (_, ncase) in sel.Cases) {
        if (ncase.Comm != null) {
            var n = ncase.Comm;
            Action<ir.Node, ir.Node, bool> oselrecv2 = (dst, recv, def) => {
                n = ir.NewAssignListStmt(n.Pos(), ir.OSELRECV2, new slice<ir.Node>(new ir.Node[] { dst, ir.BlankNode }), new slice<ir.Node>(new ir.Node[] { recv }));
                n.Def = def;
                n.SetTypecheck(1);
                ncase.Comm = n;
            }
;

            if (n.Op() == ir.OAS) 
                // convert x = <-c into x, _ = <-c
                // remove implicit conversions; the eventual assignment
                // will reintroduce them.
                n = n._<ptr<ir.AssignStmt>>();
                {
                    var r__prev2 = r;

                    var r = n.Y;

                    if (r.Op() == ir.OCONVNOP || r.Op() == ir.OCONVIFACE) {
                        r = r._<ptr<ir.ConvExpr>>();
                        if (r.Implicit()) {
                            n.Y = r.X;
                        }
                    }

                    r = r__prev2;

                }
                oselrecv2(n.X, n.Y, n.Def);
            else if (n.Op() == ir.OAS2RECV) 
                n = n._<ptr<ir.AssignListStmt>>();
                n.SetOp(ir.OSELRECV2);
            else if (n.Op() == ir.ORECV) 
                // convert <-c into _, _ = <-c
                n = n._<ptr<ir.UnaryExpr>>();
                oselrecv2(ir.BlankNode, n, false);
            else if (n.Op() == ir.OSEND) 
                break;
                    }
    }
}

// transformAsOp transforms an AssignOp statement. Corresponds to OASOP case in
// typecheck1.
private static void transformAsOp(ptr<ir.AssignOpStmt> _addr_n) {
    ref ir.AssignOpStmt n = ref _addr_n.val;

    transformCheckAssign(n, n.X);
}

// transformDot transforms an OXDOT (or ODOT) or ODOT, ODOTPTR, ODOTMETH,
// ODOTINTER, or OCALLPART, as appropriate. It adds in extra nodes as needed to
// access embedded fields. Corresponds to typecheck.tcDot.
private static ir.Node transformDot(ptr<ir.SelectorExpr> _addr_n, bool isCall) {
    ref ir.SelectorExpr n = ref _addr_n.val;

    assert(n.Type() != null && n.Typecheck() == 1);
    if (n.Op() == ir.OXDOT) {
        n = typecheck.AddImplicitDots(n);
        n.SetOp(ir.ODOT);
    }
    var t = n.X.Type();

    if (n.X.Op() == ir.OTYPE) {
        return transformMethodExpr(_addr_n);
    }
    if (t.IsPtr() && !t.Elem().IsInterface()) {
        t = t.Elem();
        n.SetOp(ir.ODOTPTR);
    }
    var f = typecheck.Lookdot(n, t, 0);
    assert(f != null);

    if ((n.Op() == ir.ODOTINTER || n.Op() == ir.ODOTMETH) && !isCall) {
        n.SetOp(ir.OCALLPART);
        n.SetType(typecheck.MethodValueWrapper(n).Type());
    }
    return n;
}

// Corresponds to typecheck.typecheckMethodExpr.
private static ir.Node transformMethodExpr(ptr<ir.SelectorExpr> _addr_n) {
    ir.Node res = default;
    ref ir.SelectorExpr n = ref _addr_n.val;

    var t = n.X.Type(); 

    // Compute the method set for t.
    ptr<types.Fields> ms;
    if (t.IsInterface()) {
        ms = t.AllMethods();
    }
    else
 {
        var mt = types.ReceiverBaseType(t);
        typecheck.CalcMethods(mt);
        ms = mt.AllMethods(); 

        // The method expression T.m requires a wrapper when T
        // is different from m's declared receiver type. We
        // normally generate these wrappers while writing out
        // runtime type descriptors, which is always done for
        // types declared at package scope. However, we need
        // to make sure to generate wrappers for anonymous
        // receiver types too.
        if (mt.Sym() == null) {
            typecheck.NeedRuntimeType(t);
        }
    }
    var s = n.Sel;
    var m = typecheck.Lookdot1(n, s, t, ms, 0);
    assert(m != null);

    n.SetOp(ir.OMETHEXPR);
    n.Selection = m;
    n.SetType(typecheck.NewMethodType(m.Type, n.X.Type()));
    return n;
}

// Corresponds to typecheck.tcAppend.
private static ir.Node transformAppend(ptr<ir.CallExpr> _addr_n) {
    ref ir.CallExpr n = ref _addr_n.val;

    transformArgs(n);
    var args = n.Args;
    var t = args[0].Type();
    assert(t.IsSlice());

    if (n.IsDDD) {
        if (t.Elem().IsKind(types.TUINT8) && args[1].Type().IsString()) {
            return n;
        }
        args[1] = assignconvfn(args[1], _addr_t.Underlying());
        return n;
    }
    var @as = args[(int)1..];
    foreach (var (i, n) in as) {
        assert(n.Type() != null);
        as[i] = assignconvfn(n, _addr_t.Elem());
    }    return n;
}

// Corresponds to typecheck.tcComplex.
private static ir.Node transformComplex(ptr<ir.BinaryExpr> _addr_n) => func((_, panic, _) => {
    ref ir.BinaryExpr n = ref _addr_n.val;

    var l = n.X;
    var r = n.Y;

    assert(types.Identical(l.Type(), r.Type()));

    ptr<types.Type> t;

    if (l.Type().Kind() == types.TFLOAT32) 
        t = types.Types[types.TCOMPLEX64];
    else if (l.Type().Kind() == types.TFLOAT64) 
        t = types.Types[types.TCOMPLEX128];
    else 
        panic(fmt.Sprintf("transformComplex: unexpected type %v", l.Type()));
    // Must set the type here for generics, because this can't be determined
    // by substitution of the generic types.
    typed(t, n);
    return n;
});

// Corresponds to typecheck.tcDelete.
private static ir.Node transformDelete(ptr<ir.CallExpr> _addr_n) {
    ref ir.CallExpr n = ref _addr_n.val;

    transformArgs(n);
    var args = n.Args;
    assert(len(args) == 2);

    var l = args[0];
    var r = args[1];

    args[1] = assignconvfn(r, _addr_l.Type().Key());
    return n;
}

// Corresponds to typecheck.tcMake.
private static ir.Node transformMake(ptr<ir.CallExpr> _addr_n) => func((_, panic, _) => {
    ref ir.CallExpr n = ref _addr_n.val;

    var args = n.Args;

    n.Args = null;
    var l = args[0];
    var t = l.Type();
    assert(t != null);

    nint i = 1;
    ir.Node nn = default;

    if (t.Kind() == types.TSLICE) 
        l = args[i];
        i++;
        ir.Node r = default;
        if (i < len(args)) {
            r = args[i];
            i++;
        }
        nn = ir.NewMakeExpr(n.Pos(), ir.OMAKESLICE, l, r);
    else if (t.Kind() == types.TMAP) 
        if (i < len(args)) {
            l = args[i];
            i++;
        }
        else
 {
            l = ir.NewInt(0);
        }
        nn = ir.NewMakeExpr(n.Pos(), ir.OMAKEMAP, l, null);
        nn.SetEsc(n.Esc());
    else if (t.Kind() == types.TCHAN) 
        l = null;
        if (i < len(args)) {
            l = args[i];
            i++;
        }
        else
 {
            l = ir.NewInt(0);
        }
        nn = ir.NewMakeExpr(n.Pos(), ir.OMAKECHAN, l, null);
    else 
        panic(fmt.Sprintf("transformMake: unexpected type %v", t));
        assert(i == len(args));
    typed(n.Type(), nn);
    return nn;
});

// Corresponds to typecheck.tcPanic.
private static ir.Node transformPanic(ptr<ir.UnaryExpr> _addr_n) {
    ref ir.UnaryExpr n = ref _addr_n.val;

    n.X = assignconvfn(n.X, _addr_types.Types[types.TINTER]);
    return n;
}

// Corresponds to typecheck.tcPrint.
private static ir.Node transformPrint(ptr<ir.CallExpr> _addr_n) {
    ref ir.CallExpr n = ref _addr_n.val;

    transformArgs(n);
    return n;
}

// Corresponds to typecheck.tcRealImag.
private static ir.Node transformRealImag(ptr<ir.UnaryExpr> _addr_n) => func((_, panic, _) => {
    ref ir.UnaryExpr n = ref _addr_n.val;

    var l = n.X;
    ptr<types.Type> t; 

    // Determine result type.

    if (l.Type().Kind() == types.TCOMPLEX64) 
        t = types.Types[types.TFLOAT32];
    else if (l.Type().Kind() == types.TCOMPLEX128) 
        t = types.Types[types.TFLOAT64];
    else 
        panic(fmt.Sprintf("transformRealImag: unexpected type %v", l.Type()));
    // Must set the type here for generics, because this can't be determined
    // by substitution of the generic types.
    typed(t, n);
    return n;
});

// Corresponds to typecheck.tcLenCap.
private static ir.Node transformLenCap(ptr<ir.UnaryExpr> _addr_n) {
    ref ir.UnaryExpr n = ref _addr_n.val;

    n.X = implicitstar(n.X);
    return n;
}

// Corresponds to Builtin part of tcCall.
private static ir.Node transformBuiltin(ptr<ir.CallExpr> _addr_n) => func((_, panic, _) => {
    ref ir.CallExpr n = ref _addr_n.val;
 
    // n.Type() can be nil for builtins with no return value
    assert(n.Typecheck() == 1);
    ptr<ir.Name> fun = n.X._<ptr<ir.Name>>();
    var op = fun.BuiltinOp;


    if (op == ir.OAPPEND || op == ir.ODELETE || op == ir.OMAKE || op == ir.OPRINT || op == ir.OPRINTN || op == ir.ORECOVER)
    {
        n.SetOp(op);
        n.X = null;

        if (op == ir.OAPPEND) 
            return transformAppend(_addr_n);
        else if (op == ir.ODELETE) 
            return transformDelete(_addr_n);
        else if (op == ir.OMAKE) 
            return transformMake(_addr_n);
        else if (op == ir.OPRINT || op == ir.OPRINTN) 
            return transformPrint(_addr_n);
        else if (op == ir.ORECOVER) 
            // nothing more to do
            return n;
                goto __switch_break0;
    }
    if (op == ir.OCAP || op == ir.OCLOSE || op == ir.OIMAG || op == ir.OLEN || op == ir.OPANIC || op == ir.OREAL)
    {
        transformArgs(n);
        fallthrough = true;

    }
    if (fallthrough || op == ir.ONEW || op == ir.OALIGNOF || op == ir.OOFFSETOF || op == ir.OSIZEOF)
    {
        var u = ir.NewUnaryExpr(n.Pos(), op, n.Args[0]);
        var u1 = typed(n.Type(), ir.InitExpr(n.Init(), u)); // typecheckargs can add to old.Init

        if (op == ir.OCAP || op == ir.OLEN) 
            return transformLenCap(u1._<ptr<ir.UnaryExpr>>());
        else if (op == ir.OREAL || op == ir.OIMAG) 
            return transformRealImag(u1._<ptr<ir.UnaryExpr>>());
        else if (op == ir.OPANIC) 
            return transformPanic(u1._<ptr<ir.UnaryExpr>>());
        else if (op == ir.OCLOSE || op == ir.ONEW || op == ir.OALIGNOF || op == ir.OOFFSETOF || op == ir.OSIZEOF) 
            // nothing more to do
            return u1;
                goto __switch_break0;
    }
    if (op == ir.OCOMPLEX || op == ir.OCOPY || op == ir.OUNSAFEADD || op == ir.OUNSAFESLICE)
    {
        transformArgs(n);
        var b = ir.NewBinaryExpr(n.Pos(), op, n.Args[0], n.Args[1]);
        var n1 = typed(n.Type(), ir.InitExpr(n.Init(), b));
        if (op != ir.OCOMPLEX) { 
            // nothing more to do
            return n1;
        }
        return transformComplex(n1._<ptr<ir.BinaryExpr>>());
        goto __switch_break0;
    }
    // default: 
        panic(fmt.Sprintf("transformBuiltin: unexpected op %v", op));

    __switch_break0:;

    return n;
});

private static bool hasKeys(ir.Nodes l) {
    foreach (var (_, n) in l) {
        if (n.Op() == ir.OKEY || n.Op() == ir.OSTRUCTKEY) {
            return true;
        }
    }    return false;
}

// transformArrayLit runs assignconvfn on each array element and returns the
// length of the slice/array that is needed to hold all the array keys/indexes
// (one more than the highest index). Corresponds to typecheck.typecheckarraylit.
private static long transformArrayLit(ptr<types.Type> _addr_elemType, long bound, slice<ir.Node> elts) {
    ref types.Type elemType = ref _addr_elemType.val;

    long key = default;    long length = default;

    {
        var elt__prev1 = elt;

        foreach (var (__i, __elt) in elts) {
            i = __i;
            elt = __elt;
            ir.SetPos(elt);
            var r = elts[i];
            ptr<ir.KeyExpr> kv;
            if (elt.Op() == ir.OKEY) {
                ptr<ir.KeyExpr> elt = elt._<ptr<ir.KeyExpr>>();
                key = typecheck.IndexConst(elt.Key);
                assert(key >= 0);
                kv = addr(elt);
                r = elt.Value;
            }
            r = assignconvfn(r, _addr_elemType);
            if (kv != null) {
                kv.Value = r;
            }
            else
 {
                elts[i] = r;
            }
            key++;
            if (key > length) {
                length = key;
            }
        }
        elt = elt__prev1;
    }

    return length;
}

// transformCompLit transforms n to an OARRAYLIT, OSLICELIT, OMAPLIT, or
// OSTRUCTLIT node, with any needed conversions. Corresponds to
// typecheck.tcCompLit.
private static ir.Node transformCompLit(ptr<ir.CompLitExpr> _addr_n) => func((defer, _, _) => {
    ir.Node res = default;
    ref ir.CompLitExpr n = ref _addr_n.val;

    assert(n.Type() != null && n.Typecheck() == 1);
    var lno = @base.Pos;
    defer(() => {
        @base.Pos = lno;
    }()); 

    // Save original node (including n.Right)
    n.SetOrig(ir.Copy(n));

    ir.SetPos(n);

    var t = n.Type();


    if (t.Kind() == types.TARRAY) 
        transformArrayLit(_addr_t.Elem(), t.NumElem(), n.List);
        n.SetOp(ir.OARRAYLIT);
    else if (t.Kind() == types.TSLICE) 
        var length = transformArrayLit(_addr_t.Elem(), -1, n.List);
        n.SetOp(ir.OSLICELIT);
        n.Len = length;
    else if (t.Kind() == types.TMAP) 
        {
            var l__prev1 = l;

            foreach (var (_, __l) in n.List) {
                l = __l;
                ir.SetPos(l);
                assert(l.Op() == ir.OKEY);
                ptr<ir.KeyExpr> l = l._<ptr<ir.KeyExpr>>();

                var r = l.Key;
                l.Key = assignconvfn(r, _addr_t.Key());

                r = l.Value;
                l.Value = assignconvfn(r, _addr_t.Elem());
            }

            l = l__prev1;
        }

        n.SetOp(ir.OMAPLIT);
    else if (t.Kind() == types.TSTRUCT) 
        // Need valid field offsets for Xoffset below.
        types.CalcSize(t);

        if (len(n.List) != 0 && !hasKeys(n.List)) { 
            // simple list of values
            var ls = n.List;
            {
                var i__prev1 = i;

                foreach (var (__i, __n1) in ls) {
                    i = __i;
                    n1 = __n1;
                    ir.SetPos(n1);

                    var f = t.Field(i);
                    n1 = assignconvfn(n1, _addr_f.Type);
                    var sk = ir.NewStructKeyExpr(@base.Pos, f.Sym, n1);
                    sk.Offset = f.Offset;
                    ls[i] = sk;
                }
        else

                i = i__prev1;
            }

            assert(len(ls) >= t.NumFields());
        } { 
            // keyed list
            ls = n.List;
            {
                var i__prev1 = i;
                var l__prev1 = l;

                foreach (var (__i, __l) in ls) {
                    i = __i;
                    l = __l;
                    ir.SetPos(l);

                    if (l.Op() == ir.OKEY) {
                        ptr<ir.KeyExpr> kv = l._<ptr<ir.KeyExpr>>();
                        var key = kv.Key; 

                        // Sym might have resolved to name in other top-level
                        // package, because of import dot. Redirect to correct sym
                        // before we do the lookup.
                        var s = key.Sym();
                        {
                            ptr<ir.Ident> (id, ok) = key._<ptr<ir.Ident>>();

                            if (ok && typecheck.DotImportRefs[id] != null) {
                                s = typecheck.Lookup(s.Name);
                            } 

                            // An OXDOT uses the Sym field to hold
                            // the field to the right of the dot,
                            // so s will be non-nil, but an OXDOT
                            // is never a valid struct literal key.

                        } 

                        // An OXDOT uses the Sym field to hold
                        // the field to the right of the dot,
                        // so s will be non-nil, but an OXDOT
                        // is never a valid struct literal key.
                        assert(!(s == null || s.Pkg != types.LocalPkg || key.Op() == ir.OXDOT || s.IsBlank()));

                        l = ir.NewStructKeyExpr(l.Pos(), s, kv.Value);
                        ls[i] = l;
                    }
                    assert(l.Op() == ir.OSTRUCTKEY);
                    l = l._<ptr<ir.StructKeyExpr>>();

                    f = typecheck.Lookdot1(null, l.Field, t, t.Fields(), 0);
                    l.Offset = f.Offset;

                    l.Value = assignconvfn(l.Value, _addr_f.Type);
                }

                i = i__prev1;
                l = l__prev1;
            }
        }
        n.SetOp(ir.OSTRUCTLIT);
    else 
        @base.Fatalf("transformCompLit %v", t.Kind());
        return n;
});

} // end noder_package
