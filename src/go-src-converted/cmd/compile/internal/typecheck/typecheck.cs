// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package typecheck -- go2cs converted at 2022 March 13 06:00:15 UTC
// import "cmd/compile/internal/typecheck" ==> using typecheck = go.cmd.compile.@internal.typecheck_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\typecheck\typecheck.go
namespace go.cmd.compile.@internal;

using fmt = fmt_package;
using constant = go.constant_package;
using token = go.token_package;
using strings = strings_package;

using @base = cmd.compile.@internal.@base_package;
using ir = cmd.compile.@internal.ir_package;
using types = cmd.compile.@internal.types_package;


// Function collecting autotmps generated during typechecking,
// to be included in the package-level init function.

using System;
public static partial class typecheck_package {

public static var InitTodoFunc = ir.NewFunc(@base.Pos);

private static bool inimport = default; // set during import

public static bool TypecheckAllowed = default;

public static Action<ptr<types.Type>, ptr<types.Type>> NeedITab = (t, itype) => {
};public static Action<ptr<types.Type>> NeedRuntimeType = _p0 => {
};

public static ir.Node AssignExpr(ir.Node n) {
    return typecheck(n, ctxExpr | ctxAssign);
}
public static ir.Node Expr(ir.Node n) {
    return typecheck(n, ctxExpr);
}
public static ir.Node Stmt(ir.Node n) {
    return typecheck(n, ctxStmt);
}

public static void Exprs(slice<ir.Node> exprs) {
    typecheckslice(exprs, ctxExpr);
}
public static void Stmts(slice<ir.Node> stmts) {
    typecheckslice(stmts, ctxStmt);
}

public static void Call(ptr<ir.CallExpr> _addr_call) => func((_, panic, _) => {
    ref ir.CallExpr call = ref _addr_call.val;

    var t = call.X.Type();
    if (t == null) {
        panic("misuse of Call");
    }
    var ctx = ctxStmt;
    if (t.NumResults() > 0) {
        ctx = ctxExpr | ctxMultiOK;
    }
    if (typecheck(call, ctx) != call) {
        panic("bad typecheck");
    }
});

public static ir.Node Callee(ir.Node n) {
    return typecheck(n, ctxExpr | ctxCallee);
}

public static void FuncBody(ptr<ir.Func> _addr_n) {
    ref ir.Func n = ref _addr_n.val;

    ir.CurFunc = n;
    var errorsBefore = @base.Errors();
    Stmts(n.Body);
    CheckUnused(_addr_n);
    CheckReturn(_addr_n);
    if (@base.Errors() > errorsBefore) {
        n.Body = null; // type errors; do not compile
    }
}

private static slice<ptr<ir.Func>> importlist = default;

// AllImportedBodies reads in the bodies of all imported functions and typechecks
// them, if needed.
public static void AllImportedBodies() {
    foreach (var (_, n) in importlist) {
        if (n.Inl != null) {
            ImportedBody(n);
        }
    }
}

private static slice<byte> traceIndent = default;

private static Action<ptr<ir.Node>> tracePrint(@string title, ir.Node n) => func((defer, _, _) => {
    var indent = traceIndent; 

    // guard against nil
    @string pos = default;    @string op = default;

    byte tc = default;
    if (n != null) {
        pos = @base.FmtPos(n.Pos());
        op = n.Op().String();
        tc = n.Typecheck();
    }
    types.SkipSizeForTracing = true;
    defer(() => {
        types.SkipSizeForTracing = false;
    }());
    fmt.Printf("%s: %s%s %p %s %v tc=%d\n", pos, indent, title, n, op, n, tc);
    traceIndent = append(traceIndent, ". ");

    return np => {
        traceIndent = traceIndent[..(int)len(traceIndent) - 2]; 

        // if we have a result, use that
        if (np != null) {
            n = np.val;
        }
        tc = default;
        ptr<types.Type> typ;
        if (n != null) {
            pos = @base.FmtPos(n.Pos());
            op = n.Op().String();
            tc = n.Typecheck();
            typ = n.Type();
        }
        types.SkipSizeForTracing = true;
        defer(() => {
            types.SkipSizeForTracing = false;
        }());
        fmt.Printf("%s: %s=> %p %s %v tc=%d type=%L\n", pos, indent, n, op, n, tc, typ);
    };
});

private static readonly nint ctxStmt = 1 << (int)(iota); // evaluated at statement level
private static readonly var ctxExpr = 0; // evaluated in value context
private static readonly var ctxType = 1; // evaluated in type context
private static readonly var ctxCallee = 2; // call-only expressions are ok
private static readonly var ctxMultiOK = 3; // multivalue function returns are ok
private static readonly var ctxAssign = 4; // assigning to expression

// type checks the whole tree of an expression.
// calculates expression types.
// evaluates compile time constants.
// marks variables that escape the local frame.
// rewrites n.Op to be more specific in some cases.

private static slice<ptr<ir.Name>> typecheckdefstack = default;

// Resolve ONONAME to definition, if any.
public static ir.Node Resolve(ir.Node n) => func((defer, _, _) => {
    ir.Node res = default;

    if (n == null || n.Op() != ir.ONONAME) {
        return n;
    }
    if (@base.EnableTrace && @base.Flag.LowerT) {
        defer(tracePrint("resolve", n)(_addr_res));
    }
    {
        var sym = n.Sym();

        if (sym.Pkg != types.LocalPkg) { 
            // We might have an ir.Ident from oldname or importDot.
            {
                ptr<ir.Ident> (id, ok) = n._<ptr<ir.Ident>>();

                if (ok) {
                    {
                        var pkgName = DotImportRefs[id];

                        if (pkgName != null) {
                            pkgName.Used = true;
                        }

                    }
                }

            }

            return expandDecl(n);
        }
    }

    var r = ir.AsNode(n.Sym().Def);
    if (r == null) {
        return n;
    }
    if (r.Op() == ir.OIOTA) {
        {
            var x = getIotaValue();

            if (x >= 0) {
                return ir.NewInt(x);
            }

        }
        return n;
    }
    return r;
});

private static void typecheckslice(slice<ir.Node> l, nint top) {
    foreach (var (i) in l) {
        l[i] = typecheck(l[i], top);
    }
}

private static @string _typekind = new slice<@string>(InitKeyedValues<@string>((types.TINT, "int"), (types.TUINT, "uint"), (types.TINT8, "int8"), (types.TUINT8, "uint8"), (types.TINT16, "int16"), (types.TUINT16, "uint16"), (types.TINT32, "int32"), (types.TUINT32, "uint32"), (types.TINT64, "int64"), (types.TUINT64, "uint64"), (types.TUINTPTR, "uintptr"), (types.TCOMPLEX64, "complex64"), (types.TCOMPLEX128, "complex128"), (types.TFLOAT32, "float32"), (types.TFLOAT64, "float64"), (types.TBOOL, "bool"), (types.TSTRING, "string"), (types.TPTR, "pointer"), (types.TUNSAFEPTR, "unsafe.Pointer"), (types.TSTRUCT, "struct"), (types.TINTER, "interface"), (types.TCHAN, "chan"), (types.TMAP, "map"), (types.TARRAY, "array"), (types.TSLICE, "slice"), (types.TFUNC, "func"), (types.TNIL, "nil"), (types.TIDEAL, "untyped number")));

private static @string typekind(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    if (t.IsUntyped()) {
        return fmt.Sprintf("%v", t);
    }
    var et = t.Kind();
    if (int(et) < len(_typekind)) {
        var s = _typekind[et];
        if (s != "") {
            return s;
        }
    }
    return fmt.Sprintf("etype=%d", et);
}

private static slice<ir.Node> cycleFor(ir.Node start) { 
    // Find the start node in typecheck_tcstack.
    // We know that it must exist because each time we mark
    // a node with n.SetTypecheck(2) we push it on the stack,
    // and each time we mark a node with n.SetTypecheck(2) we
    // pop it from the stack. We hit a cycle when we encounter
    // a node marked 2 in which case is must be on the stack.
    var i = len(typecheck_tcstack) - 1;
    while (i > 0 && typecheck_tcstack[i] != start) {
        i--;
    } 

    // collect all nodes with same Op
    slice<ir.Node> cycle = default;
    foreach (var (_, n) in typecheck_tcstack[(int)i..]) {
        if (n.Op() == start.Op()) {
            cycle = append(cycle, n);
        }
    }    return cycle;
}

private static @string cycleTrace(slice<ir.Node> cycle) {
    @string s = default;
    foreach (var (i, n) in cycle) {
        s += fmt.Sprintf("\n\t%v: %v uses %v", ir.Line(n), n, cycle[(i + 1) % len(cycle)]);
    }    return s;
}

private static slice<ir.Node> typecheck_tcstack = default;

public static void Func(ptr<ir.Func> _addr_fn) {
    ref ir.Func fn = ref _addr_fn.val;

    var @new = Stmt(fn);
    if (new != fn) {
        @base.Fatalf("typecheck changed func");
    }
}

private static ir.Ntype typecheckNtype(ir.Ntype n) {
    return typecheck(n, ctxType)._<ir.Ntype>();
}

// typecheck type checks node n.
// The result of typecheck MUST be assigned back to n, e.g.
//     n.Left = typecheck(n.Left, top)
private static ir.Node typecheck(ir.Node n, nint top) => func((defer, _, _) => {
    ir.Node res = default;
 
    // cannot type check until all the source has been parsed
    if (!TypecheckAllowed) {
        @base.Fatalf("early typecheck");
    }
    if (n == null) {
        return null;
    }
    if (@base.EnableTrace && @base.Flag.LowerT) {
        defer(tracePrint("typecheck", n)(_addr_res));
    }
    var lno = ir.SetPos(n); 

    // Skip over parens.
    while (n.Op() == ir.OPAREN) {
        n = n._<ptr<ir.ParenExpr>>().X;
    } 

    // Resolve definition of name and value of iota lazily.
    n = Resolve(n); 

    // Skip typecheck if already done.
    // But re-typecheck ONAME/OTYPE/OLITERAL/OPACK node in case context has changed.
    if (n.Typecheck() == 1 || n.Typecheck() == 3) {

        if (n.Op() == ir.ONAME || n.Op() == ir.OTYPE || n.Op() == ir.OLITERAL || n.Op() == ir.OPACK) 
            break;
        else 
            @base.Pos = lno;
            return n;
            }
    if (n.Typecheck() == 2) { 
        // Typechecking loop. Trying printing a meaningful message,
        // otherwise a stack trace of typechecking.

        // We can already diagnose variables used as types.
        if (n.Op() == ir.ONAME) 
            ptr<ir.Name> n = n._<ptr<ir.Name>>();
            if (top & (ctxExpr | ctxType) == ctxType) {
                @base.Errorf("%v is not a type", n);
            }
        else if (n.Op() == ir.OTYPE) 
            // Only report a type cycle if we are expecting a type.
            // Otherwise let other code report an error.
            if (top & ctxType == ctxType) { 
                // A cycle containing only alias types is an error
                // since it would expand indefinitely when aliases
                // are substituted.
                var cycle = cycleFor(n);
                foreach (var (_, n1) in cycle) {
                    if (n1.Name() != null && !n1.Name().Alias()) { 
                        // Cycle is ok. But if n is an alias type and doesn't
                        // have a type yet, we have a recursive type declaration
                        // with aliases that we can't handle properly yet.
                        // Report an error rather than crashing later.
                        if (n.Name() != null && n.Name().Alias() && n.Type() == null) {
                            @base.Pos = n.Pos();
                            @base.Fatalf("cannot handle alias type declaration (issue #25838): %v", n);
                        }
                        @base.Pos = lno;
                        return n;
                    }
                }
                @base.ErrorfAt(n.Pos(), "invalid recursive type alias %v%s", n, cycleTrace(cycle));
            }
        else if (n.Op() == ir.OLITERAL) 
            if (top & (ctxExpr | ctxType) == ctxType) {
                @base.Errorf("%v is not a type", n);
                break;
            }
            @base.ErrorfAt(n.Pos(), "constant definition loop%s", cycleTrace(cycleFor(n)));
                if (@base.Errors() == 0) {
            @string trace = default;
            for (var i = len(typecheck_tcstack) - 1; i >= 0; i--) {
                var x = typecheck_tcstack[i];
                trace += fmt.Sprintf("\n\t%v %v", ir.Line(x), x);
            }

            @base.Errorf("typechecking loop involving %v%s", n, trace);
        }
        @base.Pos = lno;
        return n;
    }
    typecheck_tcstack = append(typecheck_tcstack, n);

    n.SetTypecheck(2);
    n = typecheck1(n, top);
    n.SetTypecheck(1);

    var last = len(typecheck_tcstack) - 1;
    typecheck_tcstack[last] = null;
    typecheck_tcstack = typecheck_tcstack[..(int)last];

    ir.Expr (_, isExpr) = n._<ir.Expr>();
    ir.Stmt (_, isStmt) = n._<ir.Stmt>();
    var isMulti = false;

    if (n.Op() == ir.OCALLFUNC || n.Op() == ir.OCALLINTER || n.Op() == ir.OCALLMETH) 
        n = n._<ptr<ir.CallExpr>>();
        {
            var t__prev1 = t;

            var t = n.X.Type();

            if (t != null && t.Kind() == types.TFUNC) {
                var nr = t.NumResults();
                isMulti = nr > 1;
                if (nr == 0) {
                    isExpr = false;
                }
            }

            t = t__prev1;

        }
    else if (n.Op() == ir.OAPPEND) 
        // Must be used (and not BinaryExpr/UnaryExpr).
        isStmt = false;
    else if (n.Op() == ir.OCLOSE || n.Op() == ir.ODELETE || n.Op() == ir.OPANIC || n.Op() == ir.OPRINT || n.Op() == ir.OPRINTN || n.Op() == ir.OVARKILL || n.Op() == ir.OVARLIVE) 
        // Must not be used.
        isExpr = false;
        isStmt = true;
    else if (n.Op() == ir.OCOPY || n.Op() == ir.ORECOVER || n.Op() == ir.ORECV) 
        // Can be used or not.
        isStmt = true;
        t = n.Type();
    if (t != null && !t.IsFuncArgStruct() && n.Op() != ir.OTYPE) {

        if (t.Kind() == types.TFUNC || t.Kind() == types.TANY || t.Kind() == types.TFORW || t.Kind() == types.TIDEAL || t.Kind() == types.TNIL || t.Kind() == types.TBLANK) 
            break;
        else 
            types.CheckSize(t);
            }
    if (t != null) {
        n = EvalConst(n);
        t = n.Type();
    }

    if (top & (ctxStmt | ctxExpr) == ctxExpr && !isExpr && n.Op() != ir.OTYPE && !isMulti) 
        if (!n.Diag()) {
            @base.Errorf("%v used as value", n);
            n.SetDiag(true);
        }
        if (t != null) {
            n.SetType(null);
        }
    else if (top & ctxType == 0 && n.Op() == ir.OTYPE && t != null) 
        if (!n.Type().Broke()) {
            @base.Errorf("type %v is not an expression", n.Type());
            n.SetDiag(true);
        }
    else if (top & (ctxStmt | ctxExpr) == ctxStmt && !isStmt && t != null) 
        if (!n.Diag()) {
            @base.Errorf("%v evaluated but not used", n);
            n.SetDiag(true);
        }
        n.SetType(null);
    else if (top & (ctxType | ctxExpr) == ctxType && n.Op() != ir.OTYPE && n.Op() != ir.ONONAME && (t != null || n.Op() == ir.ONAME)) 
        @base.Errorf("%v is not a type", n);
        if (t != null) {
            if (n.Op() == ir.ONAME) {
                t.SetBroke(true);
            }
            else
 {
                n.SetType(null);
            }
        }
        @base.Pos = lno;
    return n;
});

// indexlit implements typechecking of untyped values as
// array/slice indexes. It is almost equivalent to DefaultLit
// but also accepts untyped numeric values representable as
// value of type int (see also checkmake for comparison).
// The result of indexlit MUST be assigned back to n, e.g.
//     n.Left = indexlit(n.Left)
private static ir.Node indexlit(ir.Node n) {
    if (n != null && n.Type() != null && n.Type().Kind() == types.TIDEAL) {
        return DefaultLit(n, types.Types[types.TINT]);
    }
    return n;
}

// typecheck1 should ONLY be called from typecheck.
private static ir.Node typecheck1(ir.Node n, nint top) => func((_, panic, _) => {
    {
        ptr<ir.Name> n__prev1 = n;

        ptr<ir.Name> (n, ok) = n._<ptr<ir.Name>>();

        if (ok) {
            typecheckdef(n);
        }
        n = n__prev1;

    }


    if (n.Op() == ir.OLITERAL) 
        if (n.Sym() == null && n.Type() == null) {
            if (!n.Diag()) {
                @base.Fatalf("literal missing type: %v", n);
            }
        }
        return n;
    else if (n.Op() == ir.ONIL) 
        return n; 

        // names
    else if (n.Op() == ir.ONONAME) 
        if (!n.Diag()) { 
            // Note: adderrorname looks for this string and
            // adds context about the outer expression
            @base.ErrorfAt(n.Pos(), "undefined: %v", n.Sym());
            n.SetDiag(true);
        }
        n.SetType(null);
        return n;
    else if (n.Op() == ir.ONAME) 
        ptr<ir.Name> n = n._<ptr<ir.Name>>();
        if (n.BuiltinOp != 0) {
            if (top & ctxCallee == 0) {
                @base.Errorf("use of builtin %v not in function call", n.Sym());
                n.SetType(null);
                return n;
            }
            return n;
        }
        if (top & ctxAssign == 0) { 
            // not a write to the variable
            if (ir.IsBlank(n)) {
                @base.Errorf("cannot use _ as value");
                n.SetType(null);
                return n;
            }
            n.SetUsed(true);
        }
        return n;
    else if (n.Op() == ir.OLINKSYMOFFSET) 
        // type already set
        return n;
    else if (n.Op() == ir.OPACK) 
        n = n._<ptr<ir.PkgName>>();
        @base.Errorf("use of package %v without selector", n.Sym());
        n.SetDiag(true);
        return n; 

        // types (ODEREF is with exprs)
    else if (n.Op() == ir.OTYPE) 
        return n;
    else if (n.Op() == ir.OTSLICE) 
        n = n._<ptr<ir.SliceType>>();
        return tcSliceType(n);
    else if (n.Op() == ir.OTARRAY) 
        n = n._<ptr<ir.ArrayType>>();
        return tcArrayType(n);
    else if (n.Op() == ir.OTMAP) 
        n = n._<ptr<ir.MapType>>();
        return tcMapType(n);
    else if (n.Op() == ir.OTCHAN) 
        n = n._<ptr<ir.ChanType>>();
        return tcChanType(n);
    else if (n.Op() == ir.OTSTRUCT) 
        n = n._<ptr<ir.StructType>>();
        return tcStructType(n);
    else if (n.Op() == ir.OTINTER) 
        n = n._<ptr<ir.InterfaceType>>();
        return tcInterfaceType(n);
    else if (n.Op() == ir.OTFUNC) 
        n = n._<ptr<ir.FuncType>>();
        return tcFuncType(n); 
        // type or expr
    else if (n.Op() == ir.ODEREF) 
        n = n._<ptr<ir.StarExpr>>();
        return tcStar(n, top); 

        // x op= y
    else if (n.Op() == ir.OASOP) 
        n = n._<ptr<ir.AssignOpStmt>>();
        (n.X, n.Y) = (Expr(n.X), Expr(n.Y));        checkassign(n, n.X);
        if (n.IncDec && !okforarith[n.X.Type().Kind()]) {
            @base.Errorf("invalid operation: %v (non-numeric type %v)", n, n.X.Type());
            return n;
        }

        if (n.AsOp == ir.OLSH || n.AsOp == ir.ORSH) 
            n.X, n.Y, _ = tcShift(n, n.X, n.Y);
        else if (n.AsOp == ir.OADD || n.AsOp == ir.OAND || n.AsOp == ir.OANDNOT || n.AsOp == ir.ODIV || n.AsOp == ir.OMOD || n.AsOp == ir.OMUL || n.AsOp == ir.OOR || n.AsOp == ir.OSUB || n.AsOp == ir.OXOR) 
            n.X, n.Y, _ = tcArith(n, n.AsOp, n.X, n.Y);
        else 
            @base.Fatalf("invalid assign op: %v", n.AsOp);
                return n; 

        // logical operators
    else if (n.Op() == ir.OANDAND || n.Op() == ir.OOROR) 
        n = n._<ptr<ir.LogicalExpr>>();
        (n.X, n.Y) = (Expr(n.X), Expr(n.Y));        if (n.X.Type() == null || n.Y.Type() == null) {
            n.SetType(null);
            return n;
        }
        if (!n.X.Type().IsBoolean()) {
            @base.Errorf("invalid operation: %v (operator %v not defined on %s)", n, n.Op(), typekind(_addr_n.X.Type()));
            n.SetType(null);
            return n;
        }
        if (!n.Y.Type().IsBoolean()) {
            @base.Errorf("invalid operation: %v (operator %v not defined on %s)", n, n.Op(), typekind(_addr_n.Y.Type()));
            n.SetType(null);
            return n;
        }
        var (l, r, t) = tcArith(n, n.Op(), n.X, n.Y);
        (n.X, n.Y) = (l, r);        n.SetType(t);
        return n; 

        // shift operators
    else if (n.Op() == ir.OLSH || n.Op() == ir.ORSH) 
        n = n._<ptr<ir.BinaryExpr>>();
        (n.X, n.Y) = (Expr(n.X), Expr(n.Y));        (l, r, t) = tcShift(n, n.X, n.Y);
        (n.X, n.Y) = (l, r);        n.SetType(t);
        return n; 

        // comparison operators
    else if (n.Op() == ir.OEQ || n.Op() == ir.OGE || n.Op() == ir.OGT || n.Op() == ir.OLE || n.Op() == ir.OLT || n.Op() == ir.ONE) 
        n = n._<ptr<ir.BinaryExpr>>();
        (n.X, n.Y) = (Expr(n.X), Expr(n.Y));        (l, r, t) = tcArith(n, n.Op(), n.X, n.Y);
        if (t != null) {
            (n.X, n.Y) = (l, r);            n.SetType(types.UntypedBool);
            {
                var con = EvalConst(n);

                if (con.Op() == ir.OLITERAL) {
                    return con;
                }

            }
            n.X, n.Y = defaultlit2(l, r, true);
        }
        return n; 

        // binary operators
    else if (n.Op() == ir.OADD || n.Op() == ir.OAND || n.Op() == ir.OANDNOT || n.Op() == ir.ODIV || n.Op() == ir.OMOD || n.Op() == ir.OMUL || n.Op() == ir.OOR || n.Op() == ir.OSUB || n.Op() == ir.OXOR) 
        n = n._<ptr<ir.BinaryExpr>>();
        (n.X, n.Y) = (Expr(n.X), Expr(n.Y));        (l, r, t) = tcArith(n, n.Op(), n.X, n.Y);
        if (t != null && t.Kind() == types.TSTRING && n.Op() == ir.OADD) { 
            // create or update OADDSTR node with list of strings in x + y + z + (w + v) + ...
            ptr<ir.AddStringExpr> add;
            if (l.Op() == ir.OADDSTR) {
                add = l._<ptr<ir.AddStringExpr>>();
                add.SetPos(n.Pos());
            }
            else
 {
                add = ir.NewAddStringExpr(n.Pos(), new slice<ir.Node>(new ir.Node[] { l }));
            }
            if (r.Op() == ir.OADDSTR) {
                ptr<ir.AddStringExpr> r = r._<ptr<ir.AddStringExpr>>();
                add.List.Append(r.List.Take());
            }
            else
 {
                add.List.Append(r);
            }
            add.SetType(t);
            return add;
        }
        (n.X, n.Y) = (l, r);        n.SetType(t);
        return n;
    else if (n.Op() == ir.OBITNOT || n.Op() == ir.ONEG || n.Op() == ir.ONOT || n.Op() == ir.OPLUS) 
        n = n._<ptr<ir.UnaryExpr>>();
        return tcUnaryArith(n); 

        // exprs
    else if (n.Op() == ir.OADDR) 
        n = n._<ptr<ir.AddrExpr>>();
        return tcAddr(n);
    else if (n.Op() == ir.OCOMPLIT) 
        return tcCompLit(n._<ptr<ir.CompLitExpr>>());
    else if (n.Op() == ir.OXDOT || n.Op() == ir.ODOT) 
        n = n._<ptr<ir.SelectorExpr>>();
        return tcDot(n, top);
    else if (n.Op() == ir.ODOTTYPE) 
        n = n._<ptr<ir.TypeAssertExpr>>();
        return tcDotType(n);
    else if (n.Op() == ir.OINDEX) 
        n = n._<ptr<ir.IndexExpr>>();
        return tcIndex(n);
    else if (n.Op() == ir.ORECV) 
        n = n._<ptr<ir.UnaryExpr>>();
        return tcRecv(n);
    else if (n.Op() == ir.OSEND) 
        n = n._<ptr<ir.SendStmt>>();
        return tcSend(n);
    else if (n.Op() == ir.OSLICEHEADER) 
        n = n._<ptr<ir.SliceHeaderExpr>>();
        return tcSliceHeader(n);
    else if (n.Op() == ir.OMAKESLICECOPY) 
        n = n._<ptr<ir.MakeExpr>>();
        return tcMakeSliceCopy(n);
    else if (n.Op() == ir.OSLICE || n.Op() == ir.OSLICE3) 
        n = n._<ptr<ir.SliceExpr>>();
        return tcSlice(n); 

        // call and call like
    else if (n.Op() == ir.OCALL) 
        n = n._<ptr<ir.CallExpr>>();
        return tcCall(n, top);
    else if (n.Op() == ir.OALIGNOF || n.Op() == ir.OOFFSETOF || n.Op() == ir.OSIZEOF) 
        n = n._<ptr<ir.UnaryExpr>>();
        n.SetType(types.Types[types.TUINTPTR]);
        return n;
    else if (n.Op() == ir.OCAP || n.Op() == ir.OLEN) 
        n = n._<ptr<ir.UnaryExpr>>();
        return tcLenCap(n);
    else if (n.Op() == ir.OREAL || n.Op() == ir.OIMAG) 
        n = n._<ptr<ir.UnaryExpr>>();
        return tcRealImag(n);
    else if (n.Op() == ir.OCOMPLEX) 
        n = n._<ptr<ir.BinaryExpr>>();
        return tcComplex(n);
    else if (n.Op() == ir.OCLOSE) 
        n = n._<ptr<ir.UnaryExpr>>();
        return tcClose(n);
    else if (n.Op() == ir.ODELETE) 
        n = n._<ptr<ir.CallExpr>>();
        return tcDelete(n);
    else if (n.Op() == ir.OAPPEND) 
        n = n._<ptr<ir.CallExpr>>();
        return tcAppend(n);
    else if (n.Op() == ir.OCOPY) 
        n = n._<ptr<ir.BinaryExpr>>();
        return tcCopy(n);
    else if (n.Op() == ir.OCONV) 
        n = n._<ptr<ir.ConvExpr>>();
        return tcConv(n);
    else if (n.Op() == ir.OMAKE) 
        n = n._<ptr<ir.CallExpr>>();
        return tcMake(n);
    else if (n.Op() == ir.ONEW) 
        n = n._<ptr<ir.UnaryExpr>>();
        return tcNew(n);
    else if (n.Op() == ir.OPRINT || n.Op() == ir.OPRINTN) 
        n = n._<ptr<ir.CallExpr>>();
        return tcPrint(n);
    else if (n.Op() == ir.OPANIC) 
        n = n._<ptr<ir.UnaryExpr>>();
        return tcPanic(n);
    else if (n.Op() == ir.ORECOVER) 
        n = n._<ptr<ir.CallExpr>>();
        return tcRecover(n);
    else if (n.Op() == ir.OUNSAFEADD) 
        n = n._<ptr<ir.BinaryExpr>>();
        return tcUnsafeAdd(n);
    else if (n.Op() == ir.OUNSAFESLICE) 
        n = n._<ptr<ir.BinaryExpr>>();
        return tcUnsafeSlice(n);
    else if (n.Op() == ir.OCLOSURE) 
        n = n._<ptr<ir.ClosureExpr>>();
        tcClosure(n, top);
        if (n.Type() == null) {
            return n;
        }
        return n;
    else if (n.Op() == ir.OITAB) 
        n = n._<ptr<ir.UnaryExpr>>();
        return tcITab(n);
    else if (n.Op() == ir.OIDATA) 
        // Whoever creates the OIDATA node must know a priori the concrete type at that moment,
        // usually by just having checked the OITAB.
        n = n._<ptr<ir.UnaryExpr>>();
        @base.Fatalf("cannot typecheck interface data %v", n);
        panic("unreachable");
    else if (n.Op() == ir.OSPTR) 
        n = n._<ptr<ir.UnaryExpr>>();
        return tcSPtr(n);
    else if (n.Op() == ir.OCFUNC) 
        n = n._<ptr<ir.UnaryExpr>>();
        n.X = Expr(n.X);
        n.SetType(types.Types[types.TUINTPTR]);
        return n;
    else if (n.Op() == ir.OCONVNOP) 
        n = n._<ptr<ir.ConvExpr>>();
        n.X = Expr(n.X);
        return n; 

        // statements
    else if (n.Op() == ir.OAS) 
        n = n._<ptr<ir.AssignStmt>>();
        tcAssign(n); 

        // Code that creates temps does not bother to set defn, so do it here.
        if (n.X.Op() == ir.ONAME && ir.IsAutoTmp(n.X)) {
            n.X.Name().Defn = n;
        }
        return n;
    else if (n.Op() == ir.OAS2) 
        tcAssignList(n._<ptr<ir.AssignListStmt>>());
        return n;
    else if (n.Op() == ir.OBREAK || n.Op() == ir.OCONTINUE || n.Op() == ir.ODCL || n.Op() == ir.OGOTO || n.Op() == ir.OFALL || n.Op() == ir.OVARKILL || n.Op() == ir.OVARLIVE) 
        return n;
    else if (n.Op() == ir.OBLOCK) 
        n = n._<ptr<ir.BlockStmt>>();
        Stmts(n.List);
        return n;
    else if (n.Op() == ir.OLABEL) 
        if (n.Sym().IsBlank()) { 
            // Empty identifier is valid but useless.
            // Eliminate now to simplify life later.
            // See issues 7538, 11589, 11593.
            n = ir.NewBlockStmt(n.Pos(), null);
        }
        return n;
    else if (n.Op() == ir.ODEFER || n.Op() == ir.OGO) 
        n = n._<ptr<ir.GoDeferStmt>>();
        n.Call = typecheck(n.Call, ctxStmt | ctxExpr);
        if (!n.Call.Diag()) {
            tcGoDefer(n);
        }
        return n;
    else if (n.Op() == ir.OFOR || n.Op() == ir.OFORUNTIL) 
        n = n._<ptr<ir.ForStmt>>();
        return tcFor(n);
    else if (n.Op() == ir.OIF) 
        n = n._<ptr<ir.IfStmt>>();
        return tcIf(n);
    else if (n.Op() == ir.ORETURN) 
        n = n._<ptr<ir.ReturnStmt>>();
        return tcReturn(n);
    else if (n.Op() == ir.OTAILCALL) 
        n = n._<ptr<ir.TailCallStmt>>();
        return n;
    else if (n.Op() == ir.OSELECT) 
        tcSelect(n._<ptr<ir.SelectStmt>>());
        return n;
    else if (n.Op() == ir.OSWITCH) 
        tcSwitch(n._<ptr<ir.SwitchStmt>>());
        return n;
    else if (n.Op() == ir.ORANGE) 
        tcRange(n._<ptr<ir.RangeStmt>>());
        return n;
    else if (n.Op() == ir.OTYPESW) 
        n = n._<ptr<ir.TypeSwitchGuard>>();
        @base.Errorf("use of .(type) outside type switch");
        n.SetDiag(true);
        return n;
    else if (n.Op() == ir.ODCLFUNC) 
        tcFunc(n._<ptr<ir.Func>>());
        return n;
    else if (n.Op() == ir.ODCLCONST) 
        n = n._<ptr<ir.Decl>>();
        n.X = Expr(n.X)._<ptr<ir.Name>>();
        return n;
    else if (n.Op() == ir.ODCLTYPE) 
        n = n._<ptr<ir.Decl>>();
        n.X = typecheck(n.X, ctxType)._<ptr<ir.Name>>();
        types.CheckSize(n.X.Type());
        return n;
    else 
        ir.Dump("typecheck", n);
        @base.Fatalf("typecheck %v", n.Op());
        panic("unreachable");
    // No return n here!
    // Individual cases can type-assert n, introducing a new one.
    // Each must execute its own return n.
});

private static void typecheckargs(ir.InitNode n) {
    slice<ir.Node> list = default;
    switch (n.type()) {
        case ptr<ir.CallExpr> n:
            list = n.Args;
            if (n.IsDDD) {
                Exprs(list);
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
        Exprs(list);
        return ;
    }
    typecheckslice(list, ctxExpr | ctxMultiOK);
    var t = list[0].Type();
    if (t == null || !t.IsFuncArgStruct()) {
        return ;
    }
    if (ir.Orig(n) == n) {
        n._<ir.OrigNode>().SetOrig(ir.SepCopy(n));
    }
    rewriteMultiValueCall(n, list[0]);
}

// rewriteMultiValueCall rewrites multi-valued f() to use temporaries,
// so the backend wouldn't need to worry about tuple-valued expressions.
private static void rewriteMultiValueCall(ir.InitNode n, ir.Node call) { 
    // If we're outside of function context, then this call will
    // be executed during the generated init function. However,
    // init.go hasn't yet created it. Instead, associate the
    // temporary variables with  InitTodoFunc for now, and init.go
    // will reassociate them later when it's appropriate.
    var @static = ir.CurFunc == null;
    if (static) {
        ir.CurFunc = InitTodoFunc;
    }
    var @as = ir.NewAssignListStmt(@base.Pos, ir.OAS2, null, new slice<ir.Node>(new ir.Node[] { call }));
    var results = call.Type().FieldSlice();
    var list = make_slice<ir.Node>(len(results));
    {
        var i__prev1 = i;

        foreach (var (__i, __result) in results) {
            i = __i;
            result = __result;
            var tmp = Temp(result.Type);
            @as.PtrInit().Append(ir.NewDecl(@base.Pos, ir.ODCL, tmp));
            @as.Lhs.Append(tmp);
            list[i] = tmp;
        }
        i = i__prev1;
    }

    if (static) {
        ir.CurFunc = null;
    }
    n.PtrInit().Append(Stmt(as));

    switch (n.type()) {
        case ptr<ir.CallExpr> n:
            n.Args = list;
            break;
        case ptr<ir.ReturnStmt> n:
            n.Results = list;
            break;
        case ptr<ir.AssignListStmt> n:
            if (n.Op() != ir.OAS2FUNC) {
                @base.Fatalf("rewriteMultiValueCall: invalid op %v", n.Op());
            }
            @as.SetOp(ir.OAS2FUNC);
            n.SetOp(ir.OAS2);
            n.Rhs = make_slice<ir.Node>(len(list));
            {
                var i__prev1 = i;
                var tmp__prev1 = tmp;

                foreach (var (__i, __tmp) in list) {
                    i = __i;
                    tmp = __tmp;
                    n.Rhs[i] = AssignConv(tmp, n.Lhs[i].Type(), "assignment");
                }

                i = i__prev1;
                tmp = tmp__prev1;
            }
            break;
        default:
        {
            var n = n.type();
            @base.Fatalf("rewriteMultiValueCall %+v", n.Op());
            break;
        }
    }
}

private static bool checksliceindex(ir.Node l, ir.Node r, ptr<types.Type> _addr_tp) {
    ref types.Type tp = ref _addr_tp.val;

    var t = r.Type();
    if (t == null) {
        return false;
    }
    if (!t.IsInteger()) {
        @base.Errorf("invalid slice index %v (type %v)", r, t);
        return false;
    }
    if (r.Op() == ir.OLITERAL) {
        var x = r.Val();
        if (constant.Sign(x) < 0) {
            @base.Errorf("invalid slice index %v (index must be non-negative)", r);
            return false;
        }
        else if (tp != null && tp.NumElem() >= 0 && constant.Compare(x, token.GTR, constant.MakeInt64(tp.NumElem()))) {
            @base.Errorf("invalid slice index %v (out of bounds for %d-element array)", r, tp.NumElem());
            return false;
        }
        else if (ir.IsConst(l, constant.String) && constant.Compare(x, token.GTR, constant.MakeInt64(int64(len(ir.StringVal(l)))))) {
            @base.Errorf("invalid slice index %v (out of bounds for %d-byte string)", r, len(ir.StringVal(l)));
            return false;
        }
        else if (ir.ConstOverflow(x, types.Types[types.TINT])) {
            @base.Errorf("invalid slice index %v (index too large)", r);
            return false;
        }
    }
    return true;
}

private static bool checksliceconst(ir.Node lo, ir.Node hi) {
    if (lo != null && hi != null && lo.Op() == ir.OLITERAL && hi.Op() == ir.OLITERAL && constant.Compare(lo.Val(), token.GTR, hi.Val())) {
        @base.Errorf("invalid slice index: %v > %v", lo, hi);
        return false;
    }
    return true;
}

// The result of implicitstar MUST be assigned back to n, e.g.
//     n.Left = implicitstar(n.Left)
private static ir.Node implicitstar(ir.Node n) { 
    // insert implicit * if needed for fixed array
    var t = n.Type();
    if (t == null || !t.IsPtr()) {
        return n;
    }
    t = t.Elem();
    if (t == null) {
        return n;
    }
    if (!t.IsArray()) {
        return n;
    }
    var star = ir.NewStarExpr(@base.Pos, n);
    star.SetImplicit(true);
    return Expr(star);
}

private static (ir.Node, bool) needOneArg(ptr<ir.CallExpr> _addr_n, @string f, params object[] args) {
    ir.Node _p0 = default;
    bool _p0 = default;
    args = args.Clone();
    ref ir.CallExpr n = ref _addr_n.val;

    if (len(n.Args) == 0) {
        var p = fmt.Sprintf(f, args);
        @base.Errorf("missing argument to %s: %v", p, n);
        return (null, false);
    }
    if (len(n.Args) > 1) {
        p = fmt.Sprintf(f, args);
        @base.Errorf("too many arguments to %s: %v", p, n);
        return (n.Args[0], false);
    }
    return (n.Args[0], true);
}

private static (ir.Node, ir.Node, bool) needTwoArgs(ptr<ir.CallExpr> _addr_n) {
    ir.Node _p0 = default;
    ir.Node _p0 = default;
    bool _p0 = default;
    ref ir.CallExpr n = ref _addr_n.val;

    if (len(n.Args) != 2) {
        if (len(n.Args) < 2) {
            @base.Errorf("not enough arguments in call to %v", n);
        }
        else
 {
            @base.Errorf("too many arguments in call to %v", n);
        }
        return (null, null, false);
    }
    return (n.Args[0], n.Args[1], true);
}

// Lookdot1 looks up the specified method s in the list fs of methods, returning
// the matching field or nil. If dostrcmp is 0, it matches the symbols. If
// dostrcmp is 1, it matches by name exactly. If dostrcmp is 2, it matches names
// with case folding.
public static ptr<types.Field> Lookdot1(ir.Node errnode, ptr<types.Sym> _addr_s, ptr<types.Type> _addr_t, ptr<types.Fields> _addr_fs, nint dostrcmp) {
    ref types.Sym s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref types.Fields fs = ref _addr_fs.val;

    ptr<types.Field> r;
    foreach (var (_, f) in fs.Slice()) {
        if (dostrcmp != 0 && f.Sym.Name == s.Name) {
            return _addr_f!;
        }
        if (dostrcmp == 2 && strings.EqualFold(f.Sym.Name, s.Name)) {
            return _addr_f!;
        }
        if (f.Sym != s) {
            continue;
        }
        if (r != null) {
            if (errnode != null) {
                @base.Errorf("ambiguous selector %v", errnode);
            }
            else if (t.IsPtr()) {
                @base.Errorf("ambiguous selector (%v).%v", t, s);
            }
            else
 {
                @base.Errorf("ambiguous selector %v.%v", t, s);
            }
            break;
        }
        r = f;
    }    return _addr_r!;
}

// typecheckMethodExpr checks selector expressions (ODOT) where the
// base expression is a type expression (OTYPE).
private static ir.Node typecheckMethodExpr(ptr<ir.SelectorExpr> _addr_n) => func((defer, _, _) => {
    ir.Node res = default;
    ref ir.SelectorExpr n = ref _addr_n.val;

    if (@base.EnableTrace && @base.Flag.LowerT) {
        defer(tracePrint("typecheckMethodExpr", n)(_addr_res));
    }
    var t = n.X.Type(); 

    // Compute the method set for t.
    ptr<types.Fields> ms;
    if (t.IsInterface()) {
        ms = t.AllMethods();
    }
    else
 {
        var mt = types.ReceiverBaseType(t);
        if (mt == null) {
            @base.Errorf("%v undefined (type %v has no method %v)", n, t, n.Sel);
            n.SetType(null);
            return n;
        }
        CalcMethods(mt);
        ms = mt.AllMethods(); 

        // The method expression T.m requires a wrapper when T
        // is different from m's declared receiver type. We
        // normally generate these wrappers while writing out
        // runtime type descriptors, which is always done for
        // types declared at package scope. However, we need
        // to make sure to generate wrappers for anonymous
        // receiver types too.
        if (mt.Sym() == null) {
            NeedRuntimeType(t);
        }
    }
    var s = n.Sel;
    var m = Lookdot1(n, _addr_s, _addr_t, ms, 0);
    if (m == null) {
        if (Lookdot1(n, _addr_s, _addr_t, ms, 1) != null) {
            @base.Errorf("%v undefined (cannot refer to unexported method %v)", n, s);
        }        {
            var (_, ambig) = dotpath(s, t, null, false);


            else if (ambig) {
                @base.Errorf("%v undefined (ambiguous selector)", n); // method or field
            }
            else
 {
                @base.Errorf("%v undefined (type %v has no method %v)", n, t, s);
            }

        }
        n.SetType(null);
        return n;
    }
    if (!types.IsMethodApplicable(t, m)) {
        @base.Errorf("invalid method expression %v (needs pointer receiver: (*%v).%S)", n, t, s);
        n.SetType(null);
        return n;
    }
    n.SetOp(ir.OMETHEXPR);
    n.Selection = m;
    n.SetType(NewMethodType(m.Type, n.X.Type()));
    return n;
});

private static ptr<types.Type> derefall(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    while (t != null && t.IsPtr()) {
        t = t.Elem();
    }
    return _addr_t!;
}

// Lookdot looks up field or method n.Sel in the type t and returns the matching
// field. It transforms the op of node n to ODOTINTER or ODOTMETH, if appropriate.
// It also may add a StarExpr node to n.X as needed for access to non-pointer
// methods. If dostrcmp is 0, it matches the field/method with the exact symbol
// as n.Sel (appropriate for exported fields). If dostrcmp is 1, it matches by name
// exactly. If dostrcmp is 2, it matches names with case folding.
public static ptr<types.Field> Lookdot(ptr<ir.SelectorExpr> _addr_n, ptr<types.Type> _addr_t, nint dostrcmp) {
    ref ir.SelectorExpr n = ref _addr_n.val;
    ref types.Type t = ref _addr_t.val;

    var s = n.Sel;

    types.CalcSize(t);
    ptr<types.Field> f1;
    if (t.IsStruct()) {
        f1 = Lookdot1(n, _addr_s, _addr_t, _addr_t.Fields(), dostrcmp);
    }
    else if (t.IsInterface()) {
        f1 = Lookdot1(n, _addr_s, _addr_t, _addr_t.AllMethods(), dostrcmp);
    }
    ptr<types.Field> f2;
    if (n.X.Type() == t || n.X.Type().Sym() == null) {
        var mt = types.ReceiverBaseType(t);
        if (mt != null) {
            f2 = Lookdot1(n, _addr_s, _addr_mt, _addr_mt.Methods(), dostrcmp);
        }
    }
    if (f1 != null) {
        if (dostrcmp > 1 || f1.Broke()) { 
            // Already in the process of diagnosing an error.
            return _addr_f1!;
        }
        if (f2 != null) {
            @base.Errorf("%v is both field and method", n.Sel);
        }
        if (f1.Offset == types.BADWIDTH) {
            @base.Fatalf("Lookdot badwidth t=%v, f1=%v@%p", t, f1, f1);
        }
        n.Selection = f1;
        n.SetType(f1.Type);
        if (t.IsInterface()) {
            if (n.X.Type().IsPtr()) {
                var star = ir.NewStarExpr(@base.Pos, n.X);
                star.SetImplicit(true);
                n.X = Expr(star);
            }
            n.SetOp(ir.ODOTINTER);
        }
        return _addr_f1!;
    }
    if (f2 != null) {
        if (dostrcmp > 1) { 
            // Already in the process of diagnosing an error.
            return _addr_f2!;
        }
        var orig = n.X;
        var tt = n.X.Type();
        types.CalcSize(tt);
        var rcvr = f2.Type.Recv().Type;
        if (!types.Identical(rcvr, tt)) {
            if (rcvr.IsPtr() && types.Identical(rcvr.Elem(), tt)) {
                checklvalue(n.X, "call pointer method on");
                var addr = NodAddr(n.X);
                addr.SetImplicit(true);
                n.X = typecheck(addr, ctxType | ctxExpr);
            }
            else if (tt.IsPtr() && (!rcvr.IsPtr() || rcvr.IsPtr() && rcvr.Elem().NotInHeap()) && types.Identical(tt.Elem(), rcvr)) {
                star = ir.NewStarExpr(@base.Pos, n.X);
                star.SetImplicit(true);
                n.X = typecheck(star, ctxType | ctxExpr);
            }
            else if (tt.IsPtr() && tt.Elem().IsPtr() && types.Identical(derefall(_addr_tt), derefall(_addr_rcvr))) {
                @base.Errorf("calling method %v with receiver %L requires explicit dereference", n.Sel, n.X);
                while (tt.IsPtr()) { 
                    // Stop one level early for method with pointer receiver.
                    if (rcvr.IsPtr() && !tt.Elem().IsPtr()) {
                        break;
                    }
                    star = ir.NewStarExpr(@base.Pos, n.X);
                    star.SetImplicit(true);
                    n.X = typecheck(star, ctxType | ctxExpr);
                    tt = tt.Elem();
                }
            else
            } {
                @base.Fatalf("method mismatch: %v for %v", rcvr, tt);
            }
        }
        {
            var x__prev1 = x;

            var x = n.X;

            while () {
                ir.Node inner = default;
                var @implicit = false;
                switch (x.type()) {
                    case ptr<ir.AddrExpr> x:
                        (inner, implicit) = (x.X, x.Implicit());                        break;
                    case ptr<ir.SelectorExpr> x:
                        (inner, implicit) = (x.X, x.Implicit());                        break;
                    case ptr<ir.StarExpr> x:
                        (inner, implicit) = (x.X, x.Implicit());                        break;
                }
                if (!implicit) {
                    break;
                }
                if (inner.Type().Sym() != null && (x.Op() == ir.ODEREF || x.Op() == ir.ODOTPTR)) { 
                    // Found an implicit dereference of a defined pointer type.
                    // Restore n.X for better error message.
                    n.X = orig;
                    return _addr_null!;
                }
                x = inner;
            }


            x = x__prev1;
        }

        n.Selection = f2;
        n.SetType(f2.Type);
        n.SetOp(ir.ODOTMETH);

        return _addr_f2!;
    }
    return _addr_null!;
}

private static bool nokeys(ir.Nodes l) {
    foreach (var (_, n) in l) {
        if (n.Op() == ir.OKEY || n.Op() == ir.OSTRUCTKEY) {
            return false;
        }
    }    return true;
}

private static bool hasddd(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    foreach (var (_, tl) in t.Fields().Slice()) {
        if (tl.IsDDD()) {
            return true;
        }
    }    return false;
}

// typecheck assignment: type list = expression list
private static @string typecheckaste(ir.Op op, ir.Node call, bool isddd, ptr<types.Type> _addr_tstruct, ir.Nodes nl, Func<@string> desc) => func((defer, _, _) => {
    ref types.Type tstruct = ref _addr_tstruct.val;

    ptr<types.Type> t;
    nint i = default;

    var lno = @base.Pos;
    defer(() => {
        @base.Pos = lno;
    }());

    if (tstruct.Broke()) {
        return ;
    }
    ir.Node n = default;
    if (len(nl) == 1) {
        n = nl[0];
    }
    var n1 = tstruct.NumFields();
    var n2 = len(nl);
    if (!hasddd(_addr_tstruct)) {
        if (isddd) {
            goto invalidddd;
        }
        if (n2 > n1) {
            goto toomany;
        }
        if (n2 < n1) {
            goto notenough;
        }
    }
    else
 {
        if (!isddd) {
            if (n2 < n1 - 1) {
                goto notenough;
            }
        }
        else
 {
            if (n2 > n1) {
                goto toomany;
            }
            if (n2 < n1) {
                goto notenough;
            }
        }
    }
    i = 0;
    foreach (var (_, tl) in tstruct.Fields().Slice()) {
        t = tl.Type;
        if (tl.IsDDD()) {
            if (isddd) {
                if (i >= len(nl)) {
                    goto notenough;
                }
                if (len(nl) - i > 1) {
                    goto toomany;
                }
                n = nl[i];
                ir.SetPos(n);
                if (n.Type() != null) {
                    nl[i] = assignconvfn(n, t, desc);
                }
                return ;
            } 

            // TODO(mdempsky): Make into ... call with implicit slice.
            while (i < len(nl)) {
                n = nl[i];
                ir.SetPos(n);
                if (n.Type() != null) {
                    nl[i] = assignconvfn(n, t.Elem(), desc);
                i++;
                }
            }

            return ;
        }
        if (i >= len(nl)) {
            goto notenough;
        }
        n = nl[i];
        ir.SetPos(n);
        if (n.Type() != null) {
            nl[i] = assignconvfn(n, t, desc);
        }
        i++;
    }    if (i < len(nl)) {
        goto toomany;
    }
invalidddd:
    if (isddd) {
        if (call != null) {
            @base.Errorf("invalid use of ... in call to %v", call);
        }
        else
 {
            @base.Errorf("invalid use of ... in %v", op);
        }
    }
    return ;

notenough:
    if (n == null || (!n.Diag() && n.Type() != null)) {
        var details = errorDetails(nl, _addr_tstruct, isddd);
        if (call != null) { 
            // call is the expression being called, not the overall call.
            // Method expressions have the form T.M, and the compiler has
            // rewritten those to ONAME nodes but left T in Left.
            if (call.Op() == ir.OMETHEXPR) {
                ptr<ir.SelectorExpr> call = call._<ptr<ir.SelectorExpr>>();
                @base.Errorf("not enough arguments in call to method expression %v%s", call, details);
            }
            else
 {
                @base.Errorf("not enough arguments in call to %v%s", call, details);
            }
        }
        else
 {
            @base.Errorf("not enough arguments to %v%s", op, details);
        }
        if (n != null) {
            n.SetDiag(true);
        }
    }
    return ;

toomany:
    details = errorDetails(nl, _addr_tstruct, isddd);
    if (call != null) {
        @base.Errorf("too many arguments in call to %v%s", call, details);
    }
    else
 {
        @base.Errorf("too many arguments to %v%s", op, details);
    }
});

private static @string errorDetails(ir.Nodes nl, ptr<types.Type> _addr_tstruct, bool isddd) {
    ref types.Type tstruct = ref _addr_tstruct.val;
 
    // Suppress any return message signatures if:
    //
    // (1) We don't know any type at a call site (see #19012).
    // (2) Any node has an unknown type.
    // (3) Invalid type for variadic parameter (see #46957).
    if (tstruct == null) {
        return ""; // case 1
    }
    if (isddd && !nl[len(nl) - 1].Type().IsSlice()) {
        return ""; // case 3
    }
    foreach (var (_, n) in nl) {
        if (n.Type() == null) {
            return ""; // case 2
        }
    }    return fmt.Sprintf("\n\thave %s\n\twant %v", fmtSignature(nl, isddd), tstruct);
}

// sigrepr is a type's representation to the outside world,
// in string representations of return signatures
// e.g in error messages about wrong arguments to return.
private static @string sigrepr(ptr<types.Type> _addr_t, bool isddd) {
    ref types.Type t = ref _addr_t.val;


    if (t == types.UntypedString) 
        return "string";
    else if (t == types.UntypedBool) 
        return "bool";
        if (t.Kind() == types.TIDEAL) { 
        // "untyped number" is not commonly used
        // outside of the compiler, so let's use "number".
        // TODO(mdempsky): Revisit this.
        return "number";
    }
    if (isddd) {
        if (!t.IsSlice()) {
            @base.Fatalf("bad type for ... argument: %v", t);
        }
        return "..." + t.Elem().String();
    }
    return t.String();
}

// sigerr returns the signature of the types at the call or return.
private static @string fmtSignature(ir.Nodes nl, bool isddd) {
    if (len(nl) < 1) {
        return "()";
    }
    slice<@string> typeStrings = default;
    foreach (var (i, n) in nl) {
        var isdddArg = isddd && i == len(nl) - 1;
        typeStrings = append(typeStrings, sigrepr(_addr_n.Type(), isdddArg));
    }    return fmt.Sprintf("(%s)", strings.Join(typeStrings, ", "));
}

// type check composite
private static void fielddup(@string name, map<@string, bool> hash) {
    if (hash[name]) {
        @base.Errorf("duplicate field name in struct literal: %s", name);
        return ;
    }
    hash[name] = true;
}

// iscomptype reports whether type t is a composite literal type.
private static bool iscomptype(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;


    if (t.Kind() == types.TARRAY || t.Kind() == types.TSLICE || t.Kind() == types.TSTRUCT || t.Kind() == types.TMAP) 
        return true;
    else 
        return false;
    }

// pushtype adds elided type information for composite literals if
// appropriate, and returns the resulting expression.
private static ir.Node pushtype(ir.Node nn, ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    if (nn == null || nn.Op() != ir.OCOMPLIT) {
        return nn;
    }
    ptr<ir.CompLitExpr> n = nn._<ptr<ir.CompLitExpr>>();
    if (n.Ntype != null) {
        return n;
    }

    if (iscomptype(_addr_t)) 
        // For T, return T{...}.
        n.Ntype = ir.TypeNode(t);
    else if (t.IsPtr() && iscomptype(_addr_t.Elem())) 
        // For *T, return &T{...}.
        n.Ntype = ir.TypeNode(t.Elem());

        var addr = NodAddrAt(n.Pos(), n);
        addr.SetImplicit(true);
        return addr;
        return n;
}

// typecheckarraylit type-checks a sequence of slice/array literal elements.
private static long typecheckarraylit(ptr<types.Type> _addr_elemType, long bound, slice<ir.Node> elts, @string ctx) {
    ref types.Type elemType = ref _addr_elemType.val;
 
    // If there are key/value pairs, create a map to keep seen
    // keys so we can check for duplicate indices.
    map<long, bool> indices = default;
    {
        var elt__prev1 = elt;

        foreach (var (_, __elt) in elts) {
            elt = __elt;
            if (elt.Op() == ir.OKEY) {
                indices = make_map<long, bool>();
                break;
            }
        }
        elt = elt__prev1;
    }

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
                elt.Key = Expr(elt.Key);
                key = IndexConst(elt.Key);
                if (key < 0) {
                    if (!elt.Key.Diag()) {
                        if (key == -2) {
                            @base.Errorf("index too large");
                        }
                        else
 {
                            @base.Errorf("index must be non-negative integer constant");
                        }
                        elt.Key.SetDiag(true);
                    }
                    key = -(1 << 30); // stay negative for a while
                }
                kv = addr(elt);
                r = elt.Value;
            }
            r = pushtype(r, _addr_elemType);
            r = Expr(r);
            r = AssignConv(r, elemType, ctx);
            if (kv != null) {
                kv.Value = r;
            }
            else
 {
                elts[i] = r;
            }
            if (key >= 0) {
                if (indices != null) {
                    if (indices[key]) {
                        @base.Errorf("duplicate index in %s: %d", ctx, key);
                    }
                    else
 {
                        indices[key] = true;
                    }
                }
                if (bound >= 0 && key >= bound) {
                    @base.Errorf("array index %d out of bounds [0:%d]", key, bound);
                    bound = -1;
                }
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

// visible reports whether sym is exported or locally defined.
private static bool visible(ptr<types.Sym> _addr_sym) {
    ref types.Sym sym = ref _addr_sym.val;

    return sym != null && (types.IsExported(sym.Name) || sym.Pkg == types.LocalPkg);
}

// nonexported reports whether sym is an unexported field.
private static bool nonexported(ptr<types.Sym> _addr_sym) {
    ref types.Sym sym = ref _addr_sym.val;

    return sym != null && !types.IsExported(sym.Name);
}

private static void checklvalue(ir.Node n, @string verb) {
    if (!ir.IsAddressable(n)) {
        @base.Errorf("cannot %s %v", verb, n);
    }
}

private static void checkassign(ir.Node stmt, ir.Node n) => func((defer, _, _) => { 
    // have already complained about n being invalid
    if (n.Type() == null) {
        if (@base.Errors() == 0) {
            @base.Fatalf("expected an error about %v", n);
        }
        return ;
    }
    if (ir.IsAddressable(n)) {
        return ;
    }
    if (n.Op() == ir.OINDEXMAP) {
        ptr<ir.IndexExpr> n = n._<ptr<ir.IndexExpr>>();
        n.Assigned = true;
        return ;
    }
    defer(n.SetType(null));
    if (n.Diag()) {
        return ;
    }

    if (n.Op() == ir.ODOT && n._<ptr<ir.SelectorExpr>>().X.Op() == ir.OINDEXMAP) 
        @base.Errorf("cannot assign to struct field %v in map", n);
    else if ((n.Op() == ir.OINDEX && n._<ptr<ir.IndexExpr>>().X.Type().IsString()) || n.Op() == ir.OSLICESTR) 
        @base.Errorf("cannot assign to %v (strings are immutable)", n);
    else if (n.Op() == ir.OLITERAL && n.Sym() != null && ir.IsConstNode(n)) 
        @base.Errorf("cannot assign to %v (declared const)", n);
    else 
        @base.Errorf("cannot assign to %v", n);
    });

private static void checkassignto(ptr<types.Type> _addr_src, ir.Node dst) {
    ref types.Type src = ref _addr_src.val;
 
    // TODO(mdempsky): Handle all untyped types correctly.
    if (src == types.UntypedBool && dst.Type().IsBoolean()) {
        return ;
    }
    {
        var (op, why) = Assignop(src, dst.Type());

        if (op == ir.OXXX) {
            @base.Errorf("cannot assign %v to %L in multiple assignment%s", src, dst, why);
            return ;
        }
    }
}

// The result of stringtoruneslit MUST be assigned back to n, e.g.
//     n.Left = stringtoruneslit(n.Left)
private static ir.Node stringtoruneslit(ptr<ir.ConvExpr> _addr_n) {
    ref ir.ConvExpr n = ref _addr_n.val;

    if (n.X.Op() != ir.OLITERAL || n.X.Val().Kind() != constant.String) {
        @base.Fatalf("stringtoarraylit %v", n);
    }
    slice<ir.Node> l = default;
    nint i = 0;
    foreach (var (_, r) in ir.StringVal(n.X)) {
        l = append(l, ir.NewKeyExpr(@base.Pos, ir.NewInt(int64(i)), ir.NewInt(int64(r))));
        i++;
    }    var nn = ir.NewCompLitExpr(@base.Pos, ir.OCOMPLIT, ir.TypeNode(n.Type()), null);
    nn.List = l;
    return Expr(nn);
}

private static slice<ptr<ir.MapType>> mapqueue = default;

public static void CheckMapKeys() {
    foreach (var (_, n) in mapqueue) {
        var k = n.Type().MapType().Key;
        if (!k.Broke() && !types.IsComparable(k)) {
            @base.ErrorfAt(n.Pos(), "invalid map key type %v", k);
        }
    }    mapqueue = null;
}

// TypeGen tracks the number of function-scoped defined types that
// have been declared. It's used to generate unique linker symbols for
// their runtime type descriptors.
public static int TypeGen = default;

private static void typecheckdeftype(ptr<ir.Name> _addr_n) => func((defer, _, _) => {
    ref ir.Name n = ref _addr_n.val;

    if (@base.EnableTrace && @base.Flag.LowerT) {
        defer(tracePrint("typecheckdeftype", n)(null));
    }
    var t = types.NewNamed(n);
    if (n.Curfn != null) {
        TypeGen++;
        t.Vargen = TypeGen;
    }
    if (n.Pragma() & ir.NotInHeap != 0) {
        t.SetNotInHeap(true);
    }
    n.SetType(t);
    n.SetTypecheck(1);
    n.SetWalkdef(1);

    types.DeferCheckSize();
    var errorsBefore = @base.Errors();
    n.Ntype = typecheckNtype(n.Ntype);
    {
        var underlying = n.Ntype.Type();

        if (underlying != null) {
            t.SetUnderlying(underlying);
        }
        else
 {
            n.SetDiag(true);
            n.SetType(null);
        }
    }
    if (t.Kind() == types.TFORW && @base.Errors() > errorsBefore) { 
        // Something went wrong during type-checking,
        // but it was reported. Silence future errors.
        t.SetBroke(true);
    }
    types.ResumeCheckSize();
});

private static void typecheckdef(ptr<ir.Name> _addr_n) => func((defer, _, _) => {
    ref ir.Name n = ref _addr_n.val;

    if (@base.EnableTrace && @base.Flag.LowerT) {
        defer(tracePrint("typecheckdef", n)(null));
    }
    if (n.Walkdef() == 1) {
        return ;
    }
    if (n.Type() != null) { // builtin
        // Mark as Walkdef so that if n.SetType(nil) is called later, we
        // won't try walking again.
        {
            var got = n.Walkdef();

            if (got != 0) {
                @base.Fatalf("unexpected walkdef: %v", got);
            }

        }
        n.SetWalkdef(1);
        return ;
    }
    var lno = ir.SetPos(n);
    typecheckdefstack = append(typecheckdefstack, n);
    if (n.Walkdef() == 2) {
        @base.FlushErrors();
        fmt.Printf("typecheckdef loop:");
        for (var i = len(typecheckdefstack) - 1; i >= 0; i--) {
            var n = typecheckdefstack[i];
            fmt.Printf(" %v", n.Sym());
        }
        fmt.Printf("\n");
        @base.Fatalf("typecheckdef loop");
    }
    n.SetWalkdef(2);


    if (n.Op() == ir.OLITERAL) 
        if (n.Ntype != null) {
            n.Ntype = typecheckNtype(n.Ntype);
            n.SetType(n.Ntype.Type());
            n.Ntype = null;
            if (n.Type() == null) {
                n.SetDiag(true);
                goto ret;
            }
        }
        var e = n.Defn;
        n.Defn = null;
        if (e == null) {
            ir.Dump("typecheckdef nil defn", n);
            @base.ErrorfAt(n.Pos(), "xxx");
        }
        e = Expr(e);
        if (e.Type() == null) {
            goto ret;
        }
        if (!ir.IsConstNode(e)) {
            if (!e.Diag()) {
                if (e.Op() == ir.ONIL) {
                    @base.ErrorfAt(n.Pos(), "const initializer cannot be nil");
                }
                else
 {
                    @base.ErrorfAt(n.Pos(), "const initializer %v is not a constant", e);
                }
                e.SetDiag(true);
            }
            goto ret;
        }
        var t = n.Type();
        if (t != null) {
            if (!ir.OKForConst[t.Kind()]) {
                @base.ErrorfAt(n.Pos(), "invalid constant type %v", t);
                goto ret;
            }
            if (!e.Type().IsUntyped() && !types.Identical(t, e.Type())) {
                @base.ErrorfAt(n.Pos(), "cannot use %L as type %v in const initializer", e, t);
                goto ret;
            }
            e = convlit(e, t);
        }
        n.SetType(e.Type());
        if (n.Type() != null) {
            n.SetVal(e.Val());
        }
    else if (n.Op() == ir.ONAME) 
        if (n.Ntype != null) {
            n.Ntype = typecheckNtype(n.Ntype);
            n.SetType(n.Ntype.Type());
            if (n.Type() == null) {
                n.SetDiag(true);
                goto ret;
            }
        }
        if (n.Type() != null) {
            break;
        }
        if (n.Defn == null) {
            if (n.BuiltinOp != 0) { // like OPRINTN
                break;
            }
            if (@base.Errors() > 0) { 
                // Can have undefined variables in x := foo
                // that make x have an n.name.Defn == nil.
                // If there are other errors anyway, don't
                // bother adding to the noise.
                break;
            }
            @base.Fatalf("var without type, init: %v", n.Sym());
        }
        if (n.Defn.Op() == ir.ONAME) {
            n.Defn = Expr(n.Defn);
            n.SetType(n.Defn.Type());
            break;
        }
        n.Defn = Stmt(n.Defn); // fills in n.Type
    else if (n.Op() == ir.OTYPE) 
        if (n.Alias()) { 
            // Type alias declaration: Simply use the rhs type - no need
            // to create a new type.
            // If we have a syntax error, name.Ntype may be nil.
            if (n.Ntype != null) {
                n.Ntype = typecheckNtype(n.Ntype);
                n.SetType(n.Ntype.Type());
                if (n.Type() == null) {
                    n.SetDiag(true);
                    goto ret;
                } 
                // For package-level type aliases, set n.Sym.Def so we can identify
                // it as a type alias during export. See also #31959.
                if (n.Curfn == null) {
                    n.Sym().Def = n.Ntype;
                }
            }
            break;
        }
        typecheckdeftype(_addr_n);
    else 
        @base.Fatalf("typecheckdef %v", n.Op());
    ret:
    if (n.Op() != ir.OLITERAL && n.Type() != null && n.Type().IsUntyped()) {
        @base.Fatalf("got %v for %v", n.Type(), n);
    }
    var last = len(typecheckdefstack) - 1;
    if (typecheckdefstack[last] != n) {
        @base.Fatalf("typecheckdefstack mismatch");
    }
    typecheckdefstack[last] = null;
    typecheckdefstack = typecheckdefstack[..(int)last];

    @base.Pos = lno;
    n.SetWalkdef(1);
});

private static bool checkmake(ptr<types.Type> _addr_t, @string arg, ptr<ir.Node> _addr_np) {
    ref types.Type t = ref _addr_t.val;
    ref ir.Node np = ref _addr_np.val;

    ir.Node n = np;
    if (!n.Type().IsInteger() && n.Type().Kind() != types.TIDEAL) {
        @base.Errorf("non-integer %s argument in make(%v) - %v", arg, t, n.Type());
        return false;
    }
    if (n.Op() == ir.OLITERAL) {
        var v = toint(n.Val());
        if (constant.Sign(v) < 0) {
            @base.Errorf("negative %s argument in make(%v)", arg, t);
            return false;
        }
        if (ir.ConstOverflow(v, types.Types[types.TINT])) {
            @base.Errorf("%s argument too large in make(%v)", arg, t);
            return false;
        }
    }
    n = DefaultLit(n, types.Types[types.TINT]);
    np = n;

    return true;
}

// checkunsafeslice is like checkmake but for unsafe.Slice.
private static bool checkunsafeslice(ptr<ir.Node> _addr_np) {
    ref ir.Node np = ref _addr_np.val;

    ir.Node n = np;
    if (!n.Type().IsInteger() && n.Type().Kind() != types.TIDEAL) {
        @base.Errorf("non-integer len argument in unsafe.Slice - %v", n.Type());
        return false;
    }
    if (n.Op() == ir.OLITERAL) {
        var v = toint(n.Val());
        if (constant.Sign(v) < 0) {
            @base.Errorf("negative len argument in unsafe.Slice");
            return false;
        }
        if (ir.ConstOverflow(v, types.Types[types.TINT])) {
            @base.Errorf("len argument too large in unsafe.Slice");
            return false;
        }
    }
    n = DefaultLit(n, types.Types[types.TINT]);
    np = n;

    return true;
}

// markBreak marks control statements containing break statements with SetHasBreak(true).
private static void markBreak(ptr<ir.Func> _addr_fn) {
    ref ir.Func fn = ref _addr_fn.val;

    map<ptr<types.Sym>, ir.Node> labels = default;
    ir.Node @implicit = default;

    Func<ir.Node, bool> mark = default;
    mark = n => {

        if (n.Op() == ir.OBREAK) 
            ptr<ir.BranchStmt> n = n._<ptr<ir.BranchStmt>>();
            if (n.Label == null) {
                setHasBreak(implicit);
            }
            else
 {
                setHasBreak(labels[n.Label]);
            }
        else if (n.Op() == ir.OFOR || n.Op() == ir.OFORUNTIL || n.Op() == ir.OSWITCH || n.Op() == ir.OSELECT || n.Op() == ir.ORANGE) 
            var old = implicit;
            implicit = n;
            ptr<types.Sym> sym;
            switch (n.type()) {
                case ptr<ir.ForStmt> n:
                    sym = n.Label;
                    break;
                case ptr<ir.RangeStmt> n:
                    sym = n.Label;
                    break;
                case ptr<ir.SelectStmt> n:
                    sym = n.Label;
                    break;
                case ptr<ir.SwitchStmt> n:
                    sym = n.Label;
                    break;
            }
            if (sym != null) {
                if (labels == null) { 
                    // Map creation delayed until we need it - most functions don't.
                    labels = make_map<ptr<types.Sym>, ir.Node>();
                }
                labels[sym] = n;
            }
            ir.DoChildren(n, mark);
            if (sym != null) {
                delete(labels, sym);
            }
            implicit = old;
        else 
            ir.DoChildren(n, mark);
                return false;
    };

    mark(fn);
}

private static ptr<types.Sym> controlLabel(ir.Node n) {
    switch (n.type()) {
        case ptr<ir.ForStmt> n:
            return _addr_n.Label!;
            break;
        case ptr<ir.RangeStmt> n:
            return _addr_n.Label!;
            break;
        case ptr<ir.SelectStmt> n:
            return _addr_n.Label!;
            break;
        case ptr<ir.SwitchStmt> n:
            return _addr_n.Label!;
            break;
        default:
        {
            var n = n.type();
            @base.Fatalf("controlLabel %+v", n.Op());
            return _addr_null!;
            break;
        }
    }
}

private static void setHasBreak(ir.Node n) {
    switch (n.type()) {
        case 
            break;
        case ptr<ir.ForStmt> n:
            n.HasBreak = true;
            break;
        case ptr<ir.RangeStmt> n:
            n.HasBreak = true;
            break;
        case ptr<ir.SelectStmt> n:
            n.HasBreak = true;
            break;
        case ptr<ir.SwitchStmt> n:
            n.HasBreak = true;
            break;
        default:
        {
            var n = n.type();
            @base.Fatalf("setHasBreak %+v", n.Op());
            break;
        }
    }
}

// isTermNodes reports whether the Nodes list ends with a terminating statement.
private static bool isTermNodes(ir.Nodes l) {
    var s = l;
    var c = len(s);
    if (c == 0) {
        return false;
    }
    return isTermNode(s[c - 1]);
}

// isTermNode reports whether the node n, the last one in a
// statement list, is a terminating statement.
private static bool isTermNode(ir.Node n) {

    // NOTE: OLABEL is treated as a separate statement,
    // not a separate prefix, so skipping to the last statement
    // in the block handles the labeled statement case by
    // skipping over the label. No case OLABEL here.

    if (n.Op() == ir.OBLOCK) 
        ptr<ir.BlockStmt> n = n._<ptr<ir.BlockStmt>>();
        return isTermNodes(n.List);
    else if (n.Op() == ir.OGOTO || n.Op() == ir.ORETURN || n.Op() == ir.OTAILCALL || n.Op() == ir.OPANIC || n.Op() == ir.OFALL) 
        return true;
    else if (n.Op() == ir.OFOR || n.Op() == ir.OFORUNTIL) 
        n = n._<ptr<ir.ForStmt>>();
        if (n.Cond != null) {
            return false;
        }
        if (n.HasBreak) {
            return false;
        }
        return true;
    else if (n.Op() == ir.OIF) 
        n = n._<ptr<ir.IfStmt>>();
        return isTermNodes(n.Body) && isTermNodes(n.Else);
    else if (n.Op() == ir.OSWITCH) 
        n = n._<ptr<ir.SwitchStmt>>();
        if (n.HasBreak) {
            return false;
        }
        var def = false;
        {
            var cas__prev1 = cas;

            foreach (var (_, __cas) in n.Cases) {
                cas = __cas;
                if (!isTermNodes(cas.Body)) {
                    return false;
                }
                if (len(cas.List) == 0) { // default
                    def = true;
                }
            }

            cas = cas__prev1;
        }

        return def;
    else if (n.Op() == ir.OSELECT) 
        n = n._<ptr<ir.SelectStmt>>();
        if (n.HasBreak) {
            return false;
        }
        {
            var cas__prev1 = cas;

            foreach (var (_, __cas) in n.Cases) {
                cas = __cas;
                if (!isTermNodes(cas.Body)) {
                    return false;
                }
            }

            cas = cas__prev1;
        }

        return true;
        return false;
}

// CheckUnused checks for any declared variables that weren't used.
public static void CheckUnused(ptr<ir.Func> _addr_fn) {
    ref ir.Func fn = ref _addr_fn.val;
 
    // Only report unused variables if we haven't seen any type-checking
    // errors yet.
    if (@base.Errors() != 0) {
        return ;
    }
    {
        var ln__prev1 = ln;

        foreach (var (_, __ln) in fn.Dcl) {
            ln = __ln;
            if (ln.Op() == ir.ONAME && ln.Class == ir.PAUTO && ln.Used()) {
                {
                    ptr<ir.TypeSwitchGuard> (guard, ok) = ln.Defn._<ptr<ir.TypeSwitchGuard>>();

                    if (ok) {
                        guard.Used = true;
                    }

                }
            }
        }
        ln = ln__prev1;
    }

    {
        var ln__prev1 = ln;

        foreach (var (_, __ln) in fn.Dcl) {
            ln = __ln;
            if (ln.Op() != ir.ONAME || ln.Class != ir.PAUTO || ln.Used()) {
                continue;
            }
            {
                ptr<ir.TypeSwitchGuard> (defn, ok) = ln.Defn._<ptr<ir.TypeSwitchGuard>>();

                if (ok) {
                    if (defn.Used) {
                        continue;
                    }
                    @base.ErrorfAt(defn.Tag.Pos(), "%v declared but not used", ln.Sym());
                    defn.Used = true; // suppress repeats
                }
                else
 {
                    @base.ErrorfAt(ln.Pos(), "%v declared but not used", ln.Sym());
                }

            }
        }
        ln = ln__prev1;
    }
}

// CheckReturn makes sure that fn terminates appropriately.
public static void CheckReturn(ptr<ir.Func> _addr_fn) {
    ref ir.Func fn = ref _addr_fn.val;

    if (fn.Type() != null && fn.Type().NumResults() != 0 && len(fn.Body) != 0) {
        markBreak(_addr_fn);
        if (!isTermNodes(fn.Body)) {
            @base.ErrorfAt(fn.Endlineno, "missing return at end of function");
        }
    }
}

// getIotaValue returns the current value for "iota",
// or -1 if not within a ConstSpec.
private static long getIotaValue() {
    {
        var i = len(typecheckdefstack);

        if (i > 0) {
            {
                var x = typecheckdefstack[i - 1];

                if (x.Op() == ir.OLITERAL) {
                    return x.Iota();
                }

            }
        }
    }

    if (ir.CurFunc != null && ir.CurFunc.Iota >= 0) {
        return ir.CurFunc.Iota;
    }
    return -1;
}

// curpkg returns the current package, based on Curfn.
private static ptr<types.Pkg> curpkg() {
    var fn = ir.CurFunc;
    if (fn == null) { 
        // Initialization expressions for package-scope variables.
        return _addr_types.LocalPkg!;
    }
    return _addr_fnpkg(fn.Nname)!;
}

public static ir.Node Conv(ir.Node n, ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    if (types.Identical(n.Type(), t)) {
        return n;
    }
    n = ir.NewConvExpr(@base.Pos, ir.OCONV, null, n);
    n.SetType(t);
    n = Expr(n);
    return n;
}

// ConvNop converts node n to type t using the OCONVNOP op
// and typechecks the result with ctxExpr.
public static ir.Node ConvNop(ir.Node n, ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    if (types.Identical(n.Type(), t)) {
        return n;
    }
    n = ir.NewConvExpr(@base.Pos, ir.OCONVNOP, null, n);
    n.SetType(t);
    n = Expr(n);
    return n;
}

} // end typecheck_package
