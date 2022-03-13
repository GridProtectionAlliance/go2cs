// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package walk -- go2cs converted at 2022 March 13 06:25:28 UTC
// import "cmd/compile/internal/walk" ==> using walk = go.cmd.compile.@internal.walk_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\walk\switch.go
namespace go.cmd.compile.@internal;

using constant = go.constant_package;
using token = go.token_package;
using sort = sort_package;

using @base = cmd.compile.@internal.@base_package;
using ir = cmd.compile.@internal.ir_package;
using typecheck = cmd.compile.@internal.typecheck_package;
using types = cmd.compile.@internal.types_package;
using src = cmd.@internal.src_package;


// walkSwitch walks a switch statement.

using System;
public static partial class walk_package {

private static void walkSwitch(ptr<ir.SwitchStmt> _addr_sw) {
    ref ir.SwitchStmt sw = ref _addr_sw.val;
 
    // Guard against double walk, see #25776.
    if (sw.Walked()) {
        return ; // Was fatal, but eliminating every possible source of double-walking is hard
    }
    sw.SetWalked(true);

    if (sw.Tag != null && sw.Tag.Op() == ir.OTYPESW) {
        walkSwitchType(_addr_sw);
    }
    else
 {
        walkSwitchExpr(_addr_sw);
    }
}

// walkSwitchExpr generates an AST implementing sw.  sw is an
// expression switch.
private static void walkSwitchExpr(ptr<ir.SwitchStmt> _addr_sw) {
    ref ir.SwitchStmt sw = ref _addr_sw.val;

    var lno = ir.SetPos(sw);

    var cond = sw.Tag;
    sw.Tag = null; 

    // convert switch {...} to switch true {...}
    if (cond == null) {
        cond = ir.NewBool(true);
        cond = typecheck.Expr(cond);
        cond = typecheck.DefaultLit(cond, null);
    }
    if (cond.Op() == ir.OBYTES2STR && allCaseExprsAreSideEffectFree(_addr_sw)) {
        cond = cond._<ptr<ir.ConvExpr>>();
        cond.SetOp(ir.OBYTES2STRTMP);
    }
    cond = walkExpr(cond, sw.PtrInit());
    if (cond.Op() != ir.OLITERAL && cond.Op() != ir.ONIL) {
        cond = copyExpr(cond, cond.Type(), _addr_sw.Compiled);
    }
    @base.Pos = lno;

    exprSwitch s = new exprSwitch(exprname:cond,);

    ir.Node defaultGoto = default;
    ir.Nodes body = default;
    foreach (var (_, ncase) in sw.Cases) {
        var label = typecheck.AutoLabel(".s");
        var jmp = ir.NewBranchStmt(ncase.Pos(), ir.OGOTO, label); 

        // Process case dispatch.
        if (len(ncase.List) == 0) {
            if (defaultGoto != null) {
                @base.Fatalf("duplicate default case not detected during typechecking");
            }
            defaultGoto = jmp;
        }
        foreach (var (_, n1) in ncase.List) {
            s.Add(ncase.Pos(), n1, jmp);
        }        body.Append(ir.NewLabelStmt(ncase.Pos(), label));
        body.Append(ncase.Body);
        {
            var (fall, pos) = endsInFallthrough(ncase.Body);

            if (!fall) {
                var br = ir.NewBranchStmt(@base.Pos, ir.OBREAK, null);
                br.SetPos(pos);
                body.Append(br);
            }

        }
    }    sw.Cases = null;

    if (defaultGoto == null) {
        br = ir.NewBranchStmt(@base.Pos, ir.OBREAK, null);
        br.SetPos(br.Pos().WithNotStmt());
        defaultGoto = br;
    }
    s.Emit(_addr_sw.Compiled);
    sw.Compiled.Append(defaultGoto);
    sw.Compiled.Append(body.Take());
    walkStmtList(sw.Compiled);
}

// An exprSwitch walks an expression switch.
private partial struct exprSwitch {
    public ir.Node exprname; // value being switched on

    public ir.Nodes done;
    public slice<exprClause> clauses;
}

private partial struct exprClause {
    public src.XPos pos;
    public ir.Node lo;
    public ir.Node hi;
    public ir.Node jmp;
}

private static void Add(this ptr<exprSwitch> _addr_s, src.XPos pos, ir.Node expr, ir.Node jmp) {
    ref exprSwitch s = ref _addr_s.val;

    exprClause c = new exprClause(pos:pos,lo:expr,hi:expr,jmp:jmp);
    if (types.IsOrdered[s.exprname.Type().Kind()] && expr.Op() == ir.OLITERAL) {
        s.clauses = append(s.clauses, c);
        return ;
    }
    s.flush();
    s.clauses = append(s.clauses, c);
    s.flush();
}

private static void Emit(this ptr<exprSwitch> _addr_s, ptr<ir.Nodes> _addr_@out) {
    ref exprSwitch s = ref _addr_s.val;
    ref ir.Nodes @out = ref _addr_@out.val;

    s.flush();
    @out.Append(s.done.Take());
}

private static void flush(this ptr<exprSwitch> _addr_s) {
    ref exprSwitch s = ref _addr_s.val;

    var cc = s.clauses;
    s.clauses = null;
    if (len(cc) == 0) {
        return ;
    }
    if (s.exprname.Type().IsString() && len(cc) >= 2) { 
        // Sort strings by length and then by value. It is
        // much cheaper to compare lengths than values, and
        // all we need here is consistency. We respect this
        // sorting below.
        sort.Slice(cc, (i, j) => {
            var si = ir.StringVal(cc[i].lo);
            var sj = ir.StringVal(cc[j].lo);
            if (len(si) != len(sj)) {
                return len(si) < len(sj);
            }
            return si < sj;
        }); 

        // runLen returns the string length associated with a
        // particular run of exprClauses.
        Func<slice<exprClause>, long> runLen = run => int64(len(ir.StringVal(run[0].lo))); 

        // Collapse runs of consecutive strings with the same length.
        slice<slice<exprClause>> runs = default;
        nint start = 0;
        for (nint i = 1; i < len(cc); i++) {
            if (runLen(cc[(int)start..]) != runLen(cc[(int)i..])) {
                runs = append(runs, cc[(int)start..(int)i]);
                start = i;
            }
        }
        runs = append(runs, cc[(int)start..]); 

        // Perform two-level binary search.
        binarySearch(len(runs), _addr_s.done, i => ir.NewBinaryExpr(@base.Pos, ir.OLE, ir.NewUnaryExpr(@base.Pos, ir.OLEN, s.exprname), ir.NewInt(runLen(runs[i - 1]))), (i, nif) => {
            var run = runs[i];
            nif.Cond = ir.NewBinaryExpr(@base.Pos, ir.OEQ, ir.NewUnaryExpr(@base.Pos, ir.OLEN, s.exprname), ir.NewInt(runLen(run)));
            s.search(run, _addr_nif.Body);
        });
        return ;
    }
    sort.Slice(cc, (i, j) => constant.Compare(cc[i].lo.Val(), token.LSS, cc[j].lo.Val())); 

    // Merge consecutive integer cases.
    if (s.exprname.Type().IsInteger()) {
        Func<constant.Value, constant.Value, bool> consecutive = (last, next) => {
            var delta = constant.BinaryOp(next, token.SUB, last);
            return constant.Compare(delta, token.EQL, constant.MakeInt64(1));
        };

        var merged = cc[..(int)1];
        foreach (var (_, c) in cc[(int)1..]) {
            var last = _addr_merged[len(merged) - 1];
            if (last.jmp == c.jmp && consecutive(last.hi.Val(), c.lo.Val())) {
                last.hi = c.lo;
            }
            else
 {
                merged = append(merged, c);
            }
        }        cc = merged;
    }
    s.search(cc, _addr_s.done);
}

private static void search(this ptr<exprSwitch> _addr_s, slice<exprClause> cc, ptr<ir.Nodes> _addr_@out) {
    ref exprSwitch s = ref _addr_s.val;
    ref ir.Nodes @out = ref _addr_@out.val;

    binarySearch(len(cc), _addr_out, i => ir.NewBinaryExpr(@base.Pos, ir.OLE, s.exprname, cc[i - 1].hi), (i, nif) => {
        var c = _addr_cc[i];
        nif.Cond = c.test(s.exprname);
        nif.Body = new slice<ir.Node>(new ir.Node[] { c.jmp });
    });
}

private static ir.Node test(this ptr<exprClause> _addr_c, ir.Node exprname) {
    ref exprClause c = ref _addr_c.val;
 
    // Integer range.
    if (c.hi != c.lo) {
        var low = ir.NewBinaryExpr(c.pos, ir.OGE, exprname, c.lo);
        var high = ir.NewBinaryExpr(c.pos, ir.OLE, exprname, c.hi);
        return ir.NewLogicalExpr(c.pos, ir.OANDAND, low, high);
    }
    if (ir.IsConst(exprname, constant.Bool) && !c.lo.Type().IsInterface()) {
        if (ir.BoolVal(exprname)) {
            return c.lo;
        }
        else
 {
            return ir.NewUnaryExpr(c.pos, ir.ONOT, c.lo);
        }
    }
    return ir.NewBinaryExpr(c.pos, ir.OEQ, exprname, c.lo);
}

private static bool allCaseExprsAreSideEffectFree(ptr<ir.SwitchStmt> _addr_sw) {
    ref ir.SwitchStmt sw = ref _addr_sw.val;
 
    // In theory, we could be more aggressive, allowing any
    // side-effect-free expressions in cases, but it's a bit
    // tricky because some of that information is unavailable due
    // to the introduction of temporaries during order.
    // Restricting to constants is simple and probably powerful
    // enough.

    foreach (var (_, ncase) in sw.Cases) {
        foreach (var (_, v) in ncase.List) {
            if (v.Op() != ir.OLITERAL) {
                return false;
            }
        }
    }    return true;
}

// endsInFallthrough reports whether stmts ends with a "fallthrough" statement.
private static (bool, src.XPos) endsInFallthrough(slice<ir.Node> stmts) {
    bool _p0 = default;
    src.XPos _p0 = default;
 
    // Search backwards for the index of the fallthrough
    // statement. Do not assume it'll be in the last
    // position, since in some cases (e.g. when the statement
    // list contains autotmp_ variables), one or more OVARKILL
    // nodes will be at the end of the list.

    var i = len(stmts) - 1;
    while (i >= 0 && stmts[i].Op() == ir.OVARKILL) {
        i--;
    }
    if (i < 0) {
        return (false, src.NoXPos);
    }
    return (stmts[i].Op() == ir.OFALL, stmts[i].Pos());
}

// walkSwitchType generates an AST that implements sw, where sw is a
// type switch.
private static void walkSwitchType(ptr<ir.SwitchStmt> _addr_sw) {
    ref ir.SwitchStmt sw = ref _addr_sw.val;

    typeSwitch s = default;
    s.facename = sw.Tag._<ptr<ir.TypeSwitchGuard>>().X;
    sw.Tag = null;

    s.facename = walkExpr(s.facename, sw.PtrInit());
    s.facename = copyExpr(s.facename, s.facename.Type(), _addr_sw.Compiled);
    s.okname = typecheck.Temp(types.Types[types.TBOOL]); 

    // Get interface descriptor word.
    // For empty interfaces this will be the type.
    // For non-empty interfaces this will be the itab.
    var itab = ir.NewUnaryExpr(@base.Pos, ir.OITAB, s.facename); 

    // For empty interfaces, do:
    //     if e._type == nil {
    //         do nil case if it exists, otherwise default
    //     }
    //     h := e._type.hash
    // Use a similar strategy for non-empty interfaces.
    var ifNil = ir.NewIfStmt(@base.Pos, null, null, null);
    ifNil.Cond = ir.NewBinaryExpr(@base.Pos, ir.OEQ, itab, typecheck.NodNil());
    @base.Pos = @base.Pos.WithNotStmt(); // disable statement marks after the first check.
    ifNil.Cond = typecheck.Expr(ifNil.Cond);
    ifNil.Cond = typecheck.DefaultLit(ifNil.Cond, null); 
    // ifNil.Nbody assigned at end.
    sw.Compiled.Append(ifNil); 

    // Load hash from type or itab.
    var dotHash = typeHashFieldOf(@base.Pos, _addr_itab);
    s.hashname = copyExpr(dotHash, dotHash.Type(), _addr_sw.Compiled);

    var br = ir.NewBranchStmt(@base.Pos, ir.OBREAK, null);
    ir.Node defaultGoto = default;    ir.Node nilGoto = default;

    ir.Nodes body = default;
    foreach (var (_, ncase) in sw.Cases) {
        var caseVar = ncase.Var; 

        // For single-type cases with an interface type,
        // we initialize the case variable as part of the type assertion.
        // In other cases, we initialize it in the body.
        ptr<types.Type> singleType;
        if (len(ncase.List) == 1 && ncase.List[0].Op() == ir.OTYPE) {
            singleType = ncase.List[0].Type();
        }
        var caseVarInitialized = false;

        var label = typecheck.AutoLabel(".s");
        var jmp = ir.NewBranchStmt(ncase.Pos(), ir.OGOTO, label);

        if (len(ncase.List) == 0) { // default:
            if (defaultGoto != null) {
                @base.Fatalf("duplicate default case not detected during typechecking");
            }
            defaultGoto = jmp;
        }
        foreach (var (_, n1) in ncase.List) {
            if (ir.IsNil(n1)) { // case nil:
                if (nilGoto != null) {
                    @base.Fatalf("duplicate nil case not detected during typechecking");
                }
                nilGoto = jmp;
                continue;
            }
            if (singleType != null && singleType.IsInterface()) {
                s.Add(ncase.Pos(), n1.Type(), caseVar, jmp);
                caseVarInitialized = true;
            }
            else
 {
                s.Add(ncase.Pos(), n1.Type(), null, jmp);
            }
        }        body.Append(ir.NewLabelStmt(ncase.Pos(), label));
        if (caseVar != null && !caseVarInitialized) {
            var val = s.facename;
            if (singleType != null) { 
                // We have a single concrete type. Extract the data.
                if (singleType.IsInterface()) {
                    @base.Fatalf("singleType interface should have been handled in Add");
                }
                val = ifaceData(ncase.Pos(), s.facename, singleType);
            }
            ir.Node l = new slice<ir.Node>(new ir.Node[] { ir.NewDecl(ncase.Pos(),ir.ODCL,caseVar), ir.NewAssignStmt(ncase.Pos(),caseVar,val) });
            typecheck.Stmts(l);
            body.Append(l);
        }
        body.Append(ncase.Body);
        body.Append(br);
    }    sw.Cases = null;

    if (defaultGoto == null) {
        defaultGoto = br;
    }
    if (nilGoto == null) {
        nilGoto = defaultGoto;
    }
    ifNil.Body = new slice<ir.Node>(new ir.Node[] { nilGoto });

    s.Emit(_addr_sw.Compiled);
    sw.Compiled.Append(defaultGoto);
    sw.Compiled.Append(body.Take());

    walkStmtList(sw.Compiled);
}

// typeHashFieldOf returns an expression to select the type hash field
// from an interface's descriptor word (whether a *runtime._type or
// *runtime.itab pointer).
private static ptr<ir.SelectorExpr> typeHashFieldOf(src.XPos pos, ptr<ir.UnaryExpr> _addr_itab) {
    ref ir.UnaryExpr itab = ref _addr_itab.val;

    if (itab.Op() != ir.OITAB) {
        @base.Fatalf("expected OITAB, got %v", itab.Op());
    }
    ptr<types.Field> hashField;
    if (itab.X.Type().IsEmptyInterface()) { 
        // runtime._type's hash field
        if (rtypeHashField == null) {
            rtypeHashField = runtimeField("hash", int64(2 * types.PtrSize), types.Types[types.TUINT32]);
        }
        hashField = rtypeHashField;
    }
    else
 { 
        // runtime.itab's hash field
        if (itabHashField == null) {
            itabHashField = runtimeField("hash", int64(2 * types.PtrSize), types.Types[types.TUINT32]);
        }
        hashField = itabHashField;
    }
    return _addr_boundedDotPtr(pos, itab, hashField)!;
}

private static ptr<types.Field> rtypeHashField;private static ptr<types.Field> itabHashField;

// A typeSwitch walks a type switch.


// A typeSwitch walks a type switch.
private partial struct typeSwitch {
    public ir.Node facename; // value being type-switched on
    public ir.Node hashname; // type hash of the value being type-switched on
    public ir.Node okname; // boolean used for comma-ok type assertions

    public ir.Nodes done;
    public slice<typeClause> clauses;
}

private partial struct typeClause {
    public uint hash;
    public ir.Nodes body;
}

private static void Add(this ptr<typeSwitch> _addr_s, src.XPos pos, ptr<types.Type> _addr_typ, ptr<ir.Name> _addr_caseVar, ir.Node jmp) {
    ref typeSwitch s = ref _addr_s.val;
    ref types.Type typ = ref _addr_typ.val;
    ref ir.Name caseVar = ref _addr_caseVar.val;

    ref ir.Nodes body = ref heap(out ptr<ir.Nodes> _addr_body);
    if (caseVar != null) {
        ir.Node l = new slice<ir.Node>(new ir.Node[] { ir.NewDecl(pos,ir.ODCL,caseVar), ir.NewAssignStmt(pos,caseVar,nil) });
        typecheck.Stmts(l);
        body.Append(l);
    }
    else
 {
        caseVar = ir.BlankNode._<ptr<ir.Name>>();
    }
    var @as = ir.NewAssignListStmt(pos, ir.OAS2, null, null);
    @as.Lhs = new slice<ir.Node>(new ir.Node[] { caseVar, s.okname }); // cv, ok =
    var dot = ir.NewTypeAssertExpr(pos, s.facename, null);
    dot.SetType(typ); // iface.(type)
    @as.Rhs = new slice<ir.Node>(new ir.Node[] { dot });
    appendWalkStmt(_addr_body, as); 

    // if ok { goto label }
    var nif = ir.NewIfStmt(pos, null, null, null);
    nif.Cond = s.okname;
    nif.Body = new slice<ir.Node>(new ir.Node[] { jmp });
    body.Append(nif);

    if (!typ.IsInterface()) {
        s.clauses = append(s.clauses, new typeClause(hash:types.TypeHash(typ),body:body,));
        return ;
    }
    s.flush();
    s.done.Append(body.Take());
}

private static void Emit(this ptr<typeSwitch> _addr_s, ptr<ir.Nodes> _addr_@out) {
    ref typeSwitch s = ref _addr_s.val;
    ref ir.Nodes @out = ref _addr_@out.val;

    s.flush();
    @out.Append(s.done.Take());
}

private static void flush(this ptr<typeSwitch> _addr_s) {
    ref typeSwitch s = ref _addr_s.val;

    var cc = s.clauses;
    s.clauses = null;
    if (len(cc) == 0) {
        return ;
    }
    sort.Slice(cc, (i, j) => cc[i].hash < cc[j].hash); 

    // Combine adjacent cases with the same hash.
    var merged = cc[..(int)1];
    {
        var c__prev1 = c;

        foreach (var (_, __c) in cc[(int)1..]) {
            c = __c;
            var last = _addr_merged[len(merged) - 1];
            if (last.hash == c.hash) {
                last.body.Append(c.body.Take());
            }
            else
 {
                merged = append(merged, c);
            }
        }
        c = c__prev1;
    }

    cc = merged;

    binarySearch(len(cc), _addr_s.done, i => ir.NewBinaryExpr(@base.Pos, ir.OLE, s.hashname, ir.NewInt(int64(cc[i - 1].hash))), (i, nif) => { 
        // TODO(mdempsky): Omit hash equality check if
        // there's only one type.
        var c = cc[i];
        nif.Cond = ir.NewBinaryExpr(@base.Pos, ir.OEQ, s.hashname, ir.NewInt(int64(c.hash)));
        nif.Body.Append(c.body.Take());
    });
}

// binarySearch constructs a binary search tree for handling n cases,
// and appends it to out. It's used for efficiently implementing
// switch statements.
//
// less(i) should return a boolean expression. If it evaluates true,
// then cases before i will be tested; otherwise, cases i and later.
//
// leaf(i, nif) should setup nif (an OIF node) to test case i. In
// particular, it should set nif.Left and nif.Nbody.
private static void binarySearch(nint n, ptr<ir.Nodes> _addr_@out, Func<nint, ir.Node> less, Action<nint, ptr<ir.IfStmt>> leaf) {
    ref ir.Nodes @out = ref _addr_@out.val;

    const nint binarySearchMin = 4; // minimum number of cases for binary search

 // minimum number of cases for binary search

    Action<nint, nint, ptr<ir.Nodes>> @do = default;
    do = (lo, hi, @out) => {
        var n = hi - lo;
        if (n < binarySearchMin) {
            for (var i = lo; i < hi; i++) {
                var nif = ir.NewIfStmt(@base.Pos, null, null, null);
                leaf(i, nif);
                @base.Pos = @base.Pos.WithNotStmt();
                nif.Cond = typecheck.Expr(nif.Cond);
                nif.Cond = typecheck.DefaultLit(nif.Cond, null);
                @out.Append(nif);
                out = _addr_nif.Else;
            }

            return ;
        }
        var half = lo + n / 2;
        nif = ir.NewIfStmt(@base.Pos, null, null, null);
        nif.Cond = less(half);
        @base.Pos = @base.Pos.WithNotStmt();
        nif.Cond = typecheck.Expr(nif.Cond);
        nif.Cond = typecheck.DefaultLit(nif.Cond, null);
        do(lo, half, _addr_nif.Body);
        do(half, hi, _addr_nif.Else);
        @out.Append(nif);
    };

    do(0, n, out);
}

} // end walk_package
