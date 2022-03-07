// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package typecheck -- go2cs converted at 2022 March 06 22:48:41 UTC
// import "cmd/compile/internal/typecheck" ==> using typecheck = go.cmd.compile.@internal.typecheck_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\typecheck\stmt.go
using @base = go.cmd.compile.@internal.@base_package;
using ir = go.cmd.compile.@internal.ir_package;
using types = go.cmd.compile.@internal.types_package;
using src = go.cmd.@internal.src_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class typecheck_package {

public static ptr<types.Type> RangeExprType(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    if (t.IsPtr() && t.Elem().IsArray()) {
        return _addr_t.Elem()!;
    }
    return _addr_t!;

}

private static void typecheckrangeExpr(ptr<ir.RangeStmt> _addr_n) {
    ref ir.RangeStmt n = ref _addr_n.val;

    n.X = Expr(n.X);
    if (n.X.Type() == null) {
        return ;
    }
    var t = RangeExprType(_addr_n.X.Type()); 
    // delicate little dance.  see tcAssignList
    if (n.Key != null && !ir.DeclaredBy(n.Key, n)) {
        n.Key = AssignExpr(n.Key);
    }
    if (n.Value != null && !ir.DeclaredBy(n.Value, n)) {
        n.Value = AssignExpr(n.Value);
    }
    ptr<types.Type> tk;    ptr<types.Type> tv;

    var toomany = false;

    if (t.Kind() == types.TARRAY || t.Kind() == types.TSLICE) 
        tk = types.Types[types.TINT];
        tv = t.Elem();
    else if (t.Kind() == types.TMAP) 
        tk = t.Key();
        tv = t.Elem();
    else if (t.Kind() == types.TCHAN) 
        if (!t.ChanDir().CanRecv()) {
            @base.ErrorfAt(n.Pos(), "invalid operation: range %v (receive from send-only type %v)", n.X, n.X.Type());
            return ;
        }
        tk = t.Elem();
        tv = null;
        if (n.Value != null) {
            toomany = true;
        }
    else if (t.Kind() == types.TSTRING) 
        tk = types.Types[types.TINT];
        tv = types.RuneType;
    else 
        @base.ErrorfAt(n.Pos(), "cannot range over %L", n.X);
        return ;
        if (toomany) {
        @base.ErrorfAt(n.Pos(), "too many variables in range");
    }
    Action<ir.Node, ptr<types.Type>> @do = (nn, t) => {
        if (nn != null) {
            if (ir.DeclaredBy(nn, n)) {
                nn.SetType(t);
            }
            else if (nn.Type() != null) {
                {
                    var (op, why) = Assignop(t, nn.Type());

                    if (op == ir.OXXX) {
                        @base.ErrorfAt(n.Pos(), "cannot assign type %v to %L in range%s", t, nn, why);
                    }

                }

            }

            checkassign(n, nn);

        }
    };
    do(n.Key, tk);
    do(n.Value, tv);

}

// type check assignment.
// if this assignment is the definition of a var on the left side,
// fill in the var's type.
private static void tcAssign(ptr<ir.AssignStmt> _addr_n) => func((defer, _, _) => {
    ref ir.AssignStmt n = ref _addr_n.val;

    if (@base.EnableTrace && @base.Flag.LowerT) {
        defer(tracePrint("tcAssign", n)(null));
    }
    if (n.Y == null) {
        n.X = AssignExpr(n.X);
        return ;
    }
    ir.Node lhs = new slice<ir.Node>(new ir.Node[] { n.X });
    ir.Node rhs = new slice<ir.Node>(new ir.Node[] { n.Y });
    assign(n, lhs, rhs);
    (n.X, n.Y) = (lhs[0], rhs[0]);    if (!ir.IsBlank(n.X)) {
        types.CheckSize(n.X.Type()); // ensure width is calculated for backend
    }
});

private static void tcAssignList(ptr<ir.AssignListStmt> _addr_n) => func((defer, _, _) => {
    ref ir.AssignListStmt n = ref _addr_n.val;

    if (@base.EnableTrace && @base.Flag.LowerT) {
        defer(tracePrint("tcAssignList", n)(null));
    }
    assign(n, n.Lhs, n.Rhs);

});

private static void assign(ir.Node stmt, slice<ir.Node> lhs, slice<ir.Node> rhs) { 
    // delicate little dance.
    // the definition of lhs may refer to this assignment
    // as its definition, in which case it will call tcAssign.
    // in that case, do not call typecheck back, or it will cycle.
    // if the variable has a type (ntype) then typechecking
    // will not look at defn, so it is okay (and desirable,
    // so that the conversion below happens).

    Action<nint, ptr<types.Type>> checkLHS = (i, typ) => {
        lhs[i] = Resolve(lhs[i]);
        {
            var n = lhs[i];

            if (typ != null && ir.DeclaredBy(n, stmt) && n.Name().Ntype == null) {
                if (typ.Kind() != types.TNIL) {
                    n.SetType(defaultType(typ));
                }
                else
 {
                    @base.Errorf("use of untyped nil");
                }

            }

        }

        if (lhs[i].Typecheck() == 0) {
            lhs[i] = AssignExpr(lhs[i]);
        }
        checkassign(stmt, lhs[i]);

    };

    Action<nint, ptr<types.Type>> assignType = (i, typ) => {
        checkLHS(i, typ);
        if (typ != null) {
            checkassignto(typ, lhs[i]);
        }
    };

    var cr = len(rhs);
    if (len(rhs) == 1) {
        rhs[0] = typecheck(rhs[0], ctxExpr | ctxMultiOK);
        {
            var rtyp__prev2 = rtyp;

            var rtyp = rhs[0].Type();

            if (rtyp != null && rtyp.IsFuncArgStruct()) {
                cr = rtyp.NumFields();
            }

            rtyp = rtyp__prev2;

        }

    }
    else
 {
        Exprs(rhs);
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
                assignType(0, r.Type());
        assignType(1, types.UntypedBool);
        return ;

    }
    if (len(lhs) != cr) {
        {
            var r__prev2 = r;

            ptr<ir.CallExpr> (r, ok) = rhs[0]._<ptr<ir.CallExpr>>();

            if (ok && len(rhs) == 1) {
                if (r.Type() != null) {
                    @base.ErrorfAt(stmt.Pos(), "assignment mismatch: %d variable%s but %v returns %d value%s", len(lhs), plural(len(lhs)), r.X, cr, plural(cr));
                }
            }
            else
 {
                @base.ErrorfAt(stmt.Pos(), "assignment mismatch: %d variable%s but %v value%s", len(lhs), plural(len(lhs)), len(rhs), plural(len(rhs)));
            }

            r = r__prev2;

        }


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

        var mismatched = false;
        var failed = false;
        {
            var i__prev1 = i;

            foreach (var (__i) in lhs) {
                i = __i;
                var result = rtyp.Field(i).Type;
                assignType(i, result);

                if (lhs[i].Type() == null || result == null) {
                    failed = true;
                }
                else if (lhs[i] != ir.BlankNode && !types.Identical(lhs[i].Type(), result)) {
                    mismatched = true;
                }

            }

            i = i__prev1;
        }

        if (mismatched && !failed) {
            rewriteMultiValueCall(stmt, r);
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
                rhs[i] = AssignConv(r, lhs[i].Type(), "assignment");
            }
        }
        i = i__prev1;
        r = r__prev1;
    }
}

private static @string plural(nint n) {
    if (n == 1) {
        return "";
    }
    return "s";

}

// tcFor typechecks an OFOR node.
private static ir.Node tcFor(ptr<ir.ForStmt> _addr_n) {
    ref ir.ForStmt n = ref _addr_n.val;

    Stmts(n.Init());
    n.Cond = Expr(n.Cond);
    n.Cond = DefaultLit(n.Cond, null);
    if (n.Cond != null) {
        var t = n.Cond.Type();
        if (t != null && !t.IsBoolean()) {
            @base.Errorf("non-bool %L used as for condition", n.Cond);
        }
    }
    n.Post = Stmt(n.Post);
    if (n.Op() == ir.OFORUNTIL) {
        Stmts(n.Late);
    }
    Stmts(n.Body);
    return n;

}

private static void tcGoDefer(ptr<ir.GoDeferStmt> _addr_n) {
    ref ir.GoDeferStmt n = ref _addr_n.val;

    @string what = "defer";
    if (n.Op() == ir.OGO) {
        what = "go";
    }

    // ok
    if (n.Call.Op() == ir.OCALLINTER || n.Call.Op() == ir.OCALLMETH || n.Call.Op() == ir.OCALLFUNC || n.Call.Op() == ir.OCLOSE || n.Call.Op() == ir.OCOPY || n.Call.Op() == ir.ODELETE || n.Call.Op() == ir.OPANIC || n.Call.Op() == ir.OPRINT || n.Call.Op() == ir.OPRINTN || n.Call.Op() == ir.ORECOVER) 
        return ;
    else if (n.Call.Op() == ir.OAPPEND || n.Call.Op() == ir.OCAP || n.Call.Op() == ir.OCOMPLEX || n.Call.Op() == ir.OIMAG || n.Call.Op() == ir.OLEN || n.Call.Op() == ir.OMAKE || n.Call.Op() == ir.OMAKESLICE || n.Call.Op() == ir.OMAKECHAN || n.Call.Op() == ir.OMAKEMAP || n.Call.Op() == ir.ONEW || n.Call.Op() == ir.OREAL || n.Call.Op() == ir.OLITERAL) // conversion or unsafe.Alignof, Offsetof, Sizeof
        {
            var orig = ir.Orig(n.Call);

            if (orig.Op() == ir.OCONV) {
                break;
            }

        }

        @base.ErrorfAt(n.Pos(), "%s discards result of %v", what, n.Call);
        return ;
    // type is broken or missing, most likely a method call on a broken type
    // we will warn about the broken type elsewhere. no need to emit a potentially confusing error
    if (n.Call.Type() == null || n.Call.Type().Broke()) {
        return ;
    }
    if (!n.Diag()) { 
        // The syntax made sure it was a call, so this must be
        // a conversion.
        n.SetDiag(true);
        @base.ErrorfAt(n.Pos(), "%s requires function call, not conversion", what);

    }
}

// tcIf typechecks an OIF node.
private static ir.Node tcIf(ptr<ir.IfStmt> _addr_n) {
    ref ir.IfStmt n = ref _addr_n.val;

    Stmts(n.Init());
    n.Cond = Expr(n.Cond);
    n.Cond = DefaultLit(n.Cond, null);
    if (n.Cond != null) {
        var t = n.Cond.Type();
        if (t != null && !t.IsBoolean()) {
            @base.Errorf("non-bool %L used as if condition", n.Cond);
        }
    }
    Stmts(n.Body);
    Stmts(n.Else);
    return n;

}

// range
private static void tcRange(ptr<ir.RangeStmt> _addr_n) {
    ref ir.RangeStmt n = ref _addr_n.val;
 
    // Typechecking order is important here:
    // 0. first typecheck range expression (slice/map/chan),
    //    it is evaluated only once and so logically it is not part of the loop.
    // 1. typecheck produced values,
    //    this part can declare new vars and so it must be typechecked before body,
    //    because body can contain a closure that captures the vars.
    // 2. decldepth++ to denote loop body.
    // 3. typecheck body.
    // 4. decldepth--.
    typecheckrangeExpr(_addr_n); 

    // second half of dance, the first half being typecheckrangeExpr
    n.SetTypecheck(1);
    if (n.Key != null && n.Key.Typecheck() == 0) {
        n.Key = AssignExpr(n.Key);
    }
    if (n.Value != null && n.Value.Typecheck() == 0) {
        n.Value = AssignExpr(n.Value);
    }
    Stmts(n.Body);

}

// tcReturn typechecks an ORETURN node.
private static ir.Node tcReturn(ptr<ir.ReturnStmt> _addr_n) {
    ref ir.ReturnStmt n = ref _addr_n.val;

    typecheckargs(n);
    if (ir.CurFunc == null) {
        @base.Errorf("return outside function");
        n.SetType(null);
        return n;
    }
    if (ir.HasNamedResults(ir.CurFunc) && len(n.Results) == 0) {
        return n;
    }
    typecheckaste(ir.ORETURN, null, false, ir.CurFunc.Type().Results(), n.Results, () => "return argument");
    return n;

}

// select
private static void tcSelect(ptr<ir.SelectStmt> _addr_sel) {
    ref ir.SelectStmt sel = ref _addr_sel.val;

    ptr<ir.CommClause> def;
    var lno = ir.SetPos(sel);
    Stmts(sel.Init());
    foreach (var (_, ncase) in sel.Cases) {
        if (ncase.Comm == null) { 
            // default
            if (def != null) {
                @base.ErrorfAt(ncase.Pos(), "multiple defaults in select (first at %v)", ir.Line(def));
            }
            else
 {
                def = ncase;
            }

        }
        else
 {
            var n = Stmt(ncase.Comm);
            ncase.Comm = n;
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

                if (n.Y.Op() != ir.ORECV) {
                    @base.ErrorfAt(n.Pos(), "select assignment must have receive on right hand side");
                    break;
                }

                oselrecv2(n.X, n.Y, n.Def);
            else if (n.Op() == ir.OAS2RECV) 
                n = n._<ptr<ir.AssignListStmt>>();
                if (n.Rhs[0].Op() != ir.ORECV) {
                    @base.ErrorfAt(n.Pos(), "select assignment must have receive on right hand side");
                    break;
                }
                n.SetOp(ir.OSELRECV2);
            else if (n.Op() == ir.ORECV) 
                // convert <-c into _, _ = <-c
                n = n._<ptr<ir.UnaryExpr>>();
                oselrecv2(ir.BlankNode, n, false);
            else if (n.Op() == ir.OSEND) 
                break;
            else 
                var pos = n.Pos();
                if (n.Op() == ir.ONAME) { 
                    // We don't have the right position for ONAME nodes (see #15459 and
                    // others). Using ncase.Pos for now as it will provide the correct
                    // line number (assuming the expression follows the "case" keyword
                    // on the same line). This matches the approach before 1.10.
                    pos = ncase.Pos();

                }

                @base.ErrorfAt(pos, "select case must be receive, send or assign recv");
            
        }
        Stmts(ncase.Body);

    }    @base.Pos = lno;

}

// tcSend typechecks an OSEND node.
private static ir.Node tcSend(ptr<ir.SendStmt> _addr_n) {
    ref ir.SendStmt n = ref _addr_n.val;

    n.Chan = Expr(n.Chan);
    n.Value = Expr(n.Value);
    n.Chan = DefaultLit(n.Chan, null);
    var t = n.Chan.Type();
    if (t == null) {
        return n;
    }
    if (!t.IsChan()) {
        @base.Errorf("invalid operation: %v (send to non-chan type %v)", n, t);
        return n;
    }
    if (!t.ChanDir().CanSend()) {
        @base.Errorf("invalid operation: %v (send to receive-only type %v)", n, t);
        return n;
    }
    n.Value = AssignConv(n.Value, t.Elem(), "send");
    if (n.Value.Type() == null) {
        return n;
    }
    return n;

}

// tcSwitch typechecks a switch statement.
private static void tcSwitch(ptr<ir.SwitchStmt> _addr_n) {
    ref ir.SwitchStmt n = ref _addr_n.val;

    Stmts(n.Init());
    if (n.Tag != null && n.Tag.Op() == ir.OTYPESW) {
        tcSwitchType(_addr_n);
    }
    else
 {
        tcSwitchExpr(_addr_n);
    }
}

private static void tcSwitchExpr(ptr<ir.SwitchStmt> _addr_n) {
    ref ir.SwitchStmt n = ref _addr_n.val;

    var t = types.Types[types.TBOOL];
    if (n.Tag != null) {
        n.Tag = Expr(n.Tag);
        n.Tag = DefaultLit(n.Tag, null);
        t = n.Tag.Type();
    }
    @string nilonly = default;
    if (t != null) {

        if (t.IsMap()) 
            nilonly = "map";
        else if (t.Kind() == types.TFUNC) 
            nilonly = "func";
        else if (t.IsSlice()) 
            nilonly = "slice";
        else if (!types.IsComparable(t)) 
            if (t.IsStruct()) {
                @base.ErrorfAt(n.Pos(), "cannot switch on %L (struct containing %v cannot be compared)", n.Tag, types.IncomparableField(t).Type);
            }
            else
 {
                @base.ErrorfAt(n.Pos(), "cannot switch on %L", n.Tag);
            }

            t = null;
        
    }
    ir.Node defCase = default;
    constSet cs = default;
    foreach (var (_, ncase) in n.Cases) {
        var ls = ncase.List;
        if (len(ls) == 0) { // default:
            if (defCase != null) {
                @base.ErrorfAt(ncase.Pos(), "multiple defaults in switch (first at %v)", ir.Line(defCase));
            }
            else
 {
                defCase = ncase;
            }

        }
        foreach (var (i) in ls) {
            ir.SetPos(ncase);
            ls[i] = Expr(ls[i]);
            ls[i] = DefaultLit(ls[i], t);
            var n1 = ls[i];
            if (t == null || n1.Type() == null) {
                continue;
            }
            if (nilonly != "" && !ir.IsNil(n1)) {
                @base.ErrorfAt(ncase.Pos(), "invalid case %v in switch (can only compare %s %v to nil)", n1, nilonly, n.Tag);
            }
            else if (t.IsInterface() && !n1.Type().IsInterface() && !types.IsComparable(n1.Type())) {
                @base.ErrorfAt(ncase.Pos(), "invalid case %L in switch (incomparable type)", n1);
            }
            else
 {
                var (op1, _) = Assignop(n1.Type(), t);
                var (op2, _) = Assignop(t, n1.Type());
                if (op1 == ir.OXXX && op2 == ir.OXXX) {
                    if (n.Tag != null) {
                        @base.ErrorfAt(ncase.Pos(), "invalid case %v in switch on %v (mismatched types %v and %v)", n1, n.Tag, n1.Type(), t);
                    }
                    else
 {
                        @base.ErrorfAt(ncase.Pos(), "invalid case %v in switch (mismatched types %v and bool)", n1, n1.Type());
                    }

                }

            } 

            // Don't check for duplicate bools. Although the spec allows it,
            // (1) the compiler hasn't checked it in the past, so compatibility mandates it, and
            // (2) it would disallow useful things like
            //       case GOARCH == "arm" && GOARM == "5":
            //       case GOARCH == "arm":
            //     which would both evaluate to false for non-ARM compiles.
            if (!n1.Type().IsBoolean()) {
                cs.add(ncase.Pos(), n1, "case", "switch");
            }

        }        Stmts(ncase.Body);

    }
}

private static void tcSwitchType(ptr<ir.SwitchStmt> _addr_n) {
    ref ir.SwitchStmt n = ref _addr_n.val;

    ptr<ir.TypeSwitchGuard> guard = n.Tag._<ptr<ir.TypeSwitchGuard>>();
    guard.X = Expr(guard.X);
    var t = guard.X.Type();
    if (t != null && !t.IsInterface()) {
        @base.ErrorfAt(n.Pos(), "cannot type switch on non-interface value %L", guard.X);
        t = null;
    }
    {
        var v = guard.Tag;

        if (v != null && !ir.IsBlank(v) && len(n.Cases) == 0) {
            @base.ErrorfAt(v.Pos(), "%v declared but not used", v.Sym());
        }
    }


    ir.Node defCase = default;    ir.Node nilCase = default;

    typeSet ts = default;
    foreach (var (_, ncase) in n.Cases) {
        var ls = ncase.List;
        if (len(ls) == 0) { // default:
            if (defCase != null) {
                @base.ErrorfAt(ncase.Pos(), "multiple defaults in switch (first at %v)", ir.Line(defCase));
            }
            else
 {
                defCase = ncase;
            }

        }
        foreach (var (i) in ls) {
            ls[i] = typecheck(ls[i], ctxExpr | ctxType);
            var n1 = ls[i];
            if (t == null || n1.Type() == null) {
                continue;
            }
            ptr<types.Field> missing;            ptr<types.Field> have;

            ref nint ptr = ref heap(out ptr<nint> _addr_ptr);
            if (ir.IsNil(n1)) { // case nil:
                if (nilCase != null) {
                    @base.ErrorfAt(ncase.Pos(), "multiple nil cases in type switch (first at %v)", ir.Line(nilCase));
                }
                else
 {
                    nilCase = ncase;
                }

                continue;

            }

            if (n1.Op() != ir.OTYPE) {
                @base.ErrorfAt(ncase.Pos(), "%L is not a type", n1);
                continue;
            }

            if (!n1.Type().IsInterface() && !implements(n1.Type(), t, _addr_missing, _addr_have, _addr_ptr) && !missing.Broke()) {
                if (have != null && !have.Broke()) {
                    @base.ErrorfAt(ncase.Pos(), "impossible type switch case: %L cannot have dynamic type %v" + " (wrong type for %v method)\n\thave %v%S\n\twant %v%S", guard.X, n1.Type(), missing.Sym, have.Sym, have.Type, missing.Sym, missing.Type);
                }
                else if (ptr != 0) {
                    @base.ErrorfAt(ncase.Pos(), "impossible type switch case: %L cannot have dynamic type %v" + " (%v method has pointer receiver)", guard.X, n1.Type(), missing.Sym);
                }
                else
 {
                    @base.ErrorfAt(ncase.Pos(), "impossible type switch case: %L cannot have dynamic type %v" + " (missing %v method)", guard.X, n1.Type(), missing.Sym);
                }

                continue;

            }

            ts.add(ncase.Pos(), n1.Type());

        }        if (ncase.Var != null) { 
            // Assign the clause variable's type.
            var vt = t;
            if (len(ls) == 1) {
                if (ls[0].Op() == ir.OTYPE) {
                    vt = ls[0].Type();
                }
                else if (!ir.IsNil(ls[0])) { 
                    // Invalid single-type case;
                    // mark variable as broken.
                    vt = null;

                }

            }

            var nvar = ncase.Var;
            nvar.SetType(vt);
            if (vt != null) {
                nvar = AssignExpr(nvar)._<ptr<ir.Name>>();
            }
            else
 { 
                // Clause variable is broken; prevent typechecking.
                nvar.SetTypecheck(1);
                nvar.SetWalkdef(1);

            }

            ncase.Var = nvar;

        }
        Stmts(ncase.Body);

    }
}

private partial struct typeSet {
    public map<@string, slice<typeSetEntry>> m;
}

private partial struct typeSetEntry {
    public src.XPos pos;
    public ptr<types.Type> typ;
}

private static void add(this ptr<typeSet> _addr_s, src.XPos pos, ptr<types.Type> _addr_typ) {
    ref typeSet s = ref _addr_s.val;
    ref types.Type typ = ref _addr_typ.val;

    if (s.m == null) {
        s.m = make_map<@string, slice<typeSetEntry>>();
    }
    var ls = typ.LongString();
    var prevs = s.m[ls];
    foreach (var (_, prev) in prevs) {
        if (types.Identical(typ, prev.typ)) {
            @base.ErrorfAt(pos, "duplicate case %v in type switch\n\tprevious case at %s", typ, @base.FmtPos(prev.pos));
            return ;
        }
    }    s.m[ls] = append(prevs, new typeSetEntry(pos,typ));

}

} // end typecheck_package
