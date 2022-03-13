// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package walk -- go2cs converted at 2022 March 13 06:25:23 UTC
// import "cmd/compile/internal/walk" ==> using walk = go.cmd.compile.@internal.walk_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\walk\select.go
namespace go.cmd.compile.@internal;

using @base = cmd.compile.@internal.@base_package;
using ir = cmd.compile.@internal.ir_package;
using typecheck = cmd.compile.@internal.typecheck_package;
using types = cmd.compile.@internal.types_package;
using System;

public static partial class walk_package {

private static void walkSelect(ptr<ir.SelectStmt> _addr_sel) {
    ref ir.SelectStmt sel = ref _addr_sel.val;

    var lno = ir.SetPos(sel);
    if (sel.Walked()) {
        @base.Fatalf("double walkSelect");
    }
    sel.SetWalked(true);

    var init = ir.TakeInit(sel);

    init = append(init, walkSelectCases(sel.Cases));
    sel.Cases = null;

    sel.Compiled = init;
    walkStmtList(sel.Compiled);

    @base.Pos = lno;
}

private static slice<ir.Node> walkSelectCases(slice<ptr<ir.CommClause>> cases) {
    var ncas = len(cases);
    var sellineno = @base.Pos; 

    // optimization: zero-case select
    if (ncas == 0) {
        return new slice<ir.Node>(new ir.Node[] { mkcallstmt("block") });
    }
    if (ncas == 1) {
        var cas = cases[0];
        ir.SetPos(cas);
        var l = cas.Init();
        if (cas.Comm != null) { // not default:
            var n = cas.Comm;
            l = append(l, ir.TakeInit(n));

            if (n.Op() == ir.OSEND)             else if (n.Op() == ir.OSELRECV2) 
                ptr<ir.AssignListStmt> r = n._<ptr<ir.AssignListStmt>>();
                if (ir.IsBlank(r.Lhs[0]) && ir.IsBlank(r.Lhs[1])) {
                    n = r.Rhs[0];
                    break;
                }
                r.SetOp(ir.OAS2RECV);
            else 
                @base.Fatalf("select %v", n.Op());
                        l = append(l, n);
        }
        l = append(l, cas.Body);
        l = append(l, ir.NewBranchStmt(@base.Pos, ir.OBREAK, null));
        return l;
    }
    ptr<ir.CommClause> dflt;
    {
        var cas__prev1 = cas;

        foreach (var (_, __cas) in cases) {
            cas = __cas;
            ir.SetPos(cas);
            n = cas.Comm;
            if (n == null) {
                dflt = cas;
                continue;
            }

            if (n.Op() == ir.OSEND) 
                n = n._<ptr<ir.SendStmt>>();
                n.Value = typecheck.NodAddr(n.Value);
                n.Value = typecheck.Expr(n.Value);
            else if (n.Op() == ir.OSELRECV2) 
                n = n._<ptr<ir.AssignListStmt>>();
                if (!ir.IsBlank(n.Lhs[0])) {
                    n.Lhs[0] = typecheck.NodAddr(n.Lhs[0]);
                    n.Lhs[0] = typecheck.Expr(n.Lhs[0]);
                }
                    }
        cas = cas__prev1;
    }

    if (ncas == 2 && dflt != null) {
        cas = cases[0];
        if (cas == dflt) {
            cas = cases[1];
        }
        n = cas.Comm;
        ir.SetPos(n);
        r = ir.NewIfStmt(@base.Pos, null, null, null);
        r.PtrInit().val = cas.Init();
        ir.Node cond = default;

        if (n.Op() == ir.OSEND) 
            // if selectnbsend(c, v) { body } else { default body }
            n = n._<ptr<ir.SendStmt>>();
            var ch = n.Chan;
            cond = mkcall1(chanfn("selectnbsend", 2, ch.Type()), types.Types[types.TBOOL], r.PtrInit(), ch, n.Value);
        else if (n.Op() == ir.OSELRECV2) 
            n = n._<ptr<ir.AssignListStmt>>();
            ptr<ir.UnaryExpr> recv = n.Rhs[0]._<ptr<ir.UnaryExpr>>();
            ch = recv.X;
            var elem = n.Lhs[0];
            if (ir.IsBlank(elem)) {
                elem = typecheck.NodNil();
            }
            cond = typecheck.Temp(types.Types[types.TBOOL]);
            var fn = chanfn("selectnbrecv", 2, ch.Type());
            var call = mkcall1(fn, fn.Type().Results(), r.PtrInit(), elem, ch);
            var @as = ir.NewAssignListStmt(r.Pos(), ir.OAS2, new slice<ir.Node>(new ir.Node[] { cond, n.Lhs[1] }), new slice<ir.Node>(new ir.Node[] { call }));
            r.PtrInit().Append(typecheck.Stmt(as));
        else 
            @base.Fatalf("select %v", n.Op());
                r.Cond = typecheck.Expr(cond);
        r.Body = cas.Body;
        r.Else = append(dflt.Init(), dflt.Body);
        return new slice<ir.Node>(new ir.Node[] { r, ir.NewBranchStmt(base.Pos,ir.OBREAK,nil) });
    }
    if (dflt != null) {
        ncas--;
    }
    var casorder = make_slice<ptr<ir.CommClause>>(ncas);
    nint nsends = 0;
    nint nrecvs = 0;

    slice<ir.Node> init = default; 

    // generate sel-struct
    @base.Pos = sellineno;
    var selv = typecheck.Temp(types.NewArray(scasetype(), int64(ncas)));
    init = append(init, typecheck.Stmt(ir.NewAssignStmt(@base.Pos, selv, null))); 

    // No initialization for order; runtime.selectgo is responsible for that.
    var order = typecheck.Temp(types.NewArray(types.Types[types.TUINT16], 2 * int64(ncas)));

    ir.Node pc0 = default;    ir.Node pcs = default;

    if (@base.Flag.Race) {
        pcs = typecheck.Temp(types.NewArray(types.Types[types.TUINTPTR], int64(ncas)));
        pc0 = typecheck.Expr(typecheck.NodAddr(ir.NewIndexExpr(@base.Pos, pcs, ir.NewInt(0))));
    }
    else
 {
        pc0 = typecheck.NodNil();
    }
    {
        var cas__prev1 = cas;

        foreach (var (_, __cas) in cases) {
            cas = __cas;
            ir.SetPos(cas);

            init = append(init, ir.TakeInit(cas));

            n = cas.Comm;
            if (n == null) { // default:
                continue;
            }
            nint i = default;
            ir.Node c = default;            elem = default;


            if (n.Op() == ir.OSEND) 
                n = n._<ptr<ir.SendStmt>>();
                i = nsends;
                nsends++;
                c = n.Chan;
                elem = n.Value;
            else if (n.Op() == ir.OSELRECV2) 
                n = n._<ptr<ir.AssignListStmt>>();
                nrecvs++;
                i = ncas - nrecvs;
                recv = n.Rhs[0]._<ptr<ir.UnaryExpr>>();
                c = recv.X;
                elem = n.Lhs[0];
            else 
                @base.Fatalf("select %v", n.Op());
                        casorder[i] = cas;

            Action<@string, ir.Node> setField = (f, val) => {
                r = ir.NewAssignStmt(@base.Pos, ir.NewSelectorExpr(@base.Pos, ir.ODOT, ir.NewIndexExpr(@base.Pos, selv, ir.NewInt(int64(i))), typecheck.Lookup(f)), val);
                init = append(init, typecheck.Stmt(r));
            }
;

            c = typecheck.ConvNop(c, types.Types[types.TUNSAFEPTR]);
            setField("c", c);
            if (!ir.IsBlank(elem)) {
                elem = typecheck.ConvNop(elem, types.Types[types.TUNSAFEPTR]);
                setField("elem", elem);
            } 

            // TODO(mdempsky): There should be a cleaner way to
            // handle this.
            if (@base.Flag.Race) {
                r = mkcallstmt("selectsetpc", typecheck.NodAddr(ir.NewIndexExpr(@base.Pos, pcs, ir.NewInt(int64(i)))));
                init = append(init, r);
            }
        }
        cas = cas__prev1;
    }

    if (nsends + nrecvs != ncas) {
        @base.Fatalf("walkSelectCases: miscount: %v + %v != %v", nsends, nrecvs, ncas);
    }
    @base.Pos = sellineno;
    var chosen = typecheck.Temp(types.Types[types.TINT]);
    var recvOK = typecheck.Temp(types.Types[types.TBOOL]);
    r = ir.NewAssignListStmt(@base.Pos, ir.OAS2, null, null);
    r.Lhs = new slice<ir.Node>(new ir.Node[] { chosen, recvOK });
    fn = typecheck.LookupRuntime("selectgo");
    ref ir.Nodes fnInit = ref heap(out ptr<ir.Nodes> _addr_fnInit);
    r.Rhs = new slice<ir.Node>(new ir.Node[] { mkcall1(fn,fn.Type().Results(),&fnInit,bytePtrToIndex(selv,0),bytePtrToIndex(order,0),pc0,ir.NewInt(int64(nsends)),ir.NewInt(int64(nrecvs)),ir.NewBool(dflt==nil)) });
    init = append(init, fnInit);
    init = append(init, typecheck.Stmt(r)); 

    // selv and order are no longer alive after selectgo.
    init = append(init, ir.NewUnaryExpr(@base.Pos, ir.OVARKILL, selv));
    init = append(init, ir.NewUnaryExpr(@base.Pos, ir.OVARKILL, order));
    if (@base.Flag.Race) {
        init = append(init, ir.NewUnaryExpr(@base.Pos, ir.OVARKILL, pcs));
    }
    Action<ir.Node, ptr<ir.CommClause>> dispatch = (cond, cas) => {
        cond = typecheck.Expr(cond);
        cond = typecheck.DefaultLit(cond, null);

        r = ir.NewIfStmt(@base.Pos, cond, null, null);

        {
            var n__prev1 = n;

            n = cas.Comm;

            if (n != null && n.Op() == ir.OSELRECV2) {
                n = n._<ptr<ir.AssignListStmt>>();
                if (!ir.IsBlank(n.Lhs[1])) {
                    var x = ir.NewAssignStmt(@base.Pos, n.Lhs[1], recvOK);
                    r.Body.Append(typecheck.Stmt(x));
                }
            }

            n = n__prev1;

        }

        r.Body.Append(cas.Body.Take());
        r.Body.Append(ir.NewBranchStmt(@base.Pos, ir.OBREAK, null));
        init = append(init, r);
    };

    if (dflt != null) {
        ir.SetPos(dflt);
        dispatch(ir.NewBinaryExpr(@base.Pos, ir.OLT, chosen, ir.NewInt(0)), dflt);
    }
    {
        nint i__prev1 = i;
        var cas__prev1 = cas;

        foreach (var (__i, __cas) in casorder) {
            i = __i;
            cas = __cas;
            ir.SetPos(cas);
            dispatch(ir.NewBinaryExpr(@base.Pos, ir.OEQ, chosen, ir.NewInt(int64(i))), cas);
        }
        i = i__prev1;
        cas = cas__prev1;
    }

    return init;
}

// bytePtrToIndex returns a Node representing "(*byte)(&n[i])".
private static ir.Node bytePtrToIndex(ir.Node n, long i) {
    var s = typecheck.NodAddr(ir.NewIndexExpr(@base.Pos, n, ir.NewInt(i)));
    var t = types.NewPtr(types.Types[types.TUINT8]);
    return typecheck.ConvNop(s, t);
}

private static ptr<types.Type> scase;

// Keep in sync with src/runtime/select.go.
private static ptr<types.Type> scasetype() {
    if (scase == null) {
        scase = types.NewStruct(types.NoPkg, new slice<ptr<types.Field>>(new ptr<types.Field>[] { types.NewField(base.Pos,typecheck.Lookup("c"),types.Types[types.TUNSAFEPTR]), types.NewField(base.Pos,typecheck.Lookup("elem"),types.Types[types.TUNSAFEPTR]) }));
        scase.SetNoalg(true);
    }
    return _addr_scase!;
}

} // end walk_package
