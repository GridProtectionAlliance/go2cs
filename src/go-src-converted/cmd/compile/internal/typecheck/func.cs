// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package typecheck -- go2cs converted at 2022 March 13 05:59:36 UTC
// import "cmd/compile/internal/typecheck" ==> using typecheck = go.cmd.compile.@internal.typecheck_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\typecheck\func.go
namespace go.cmd.compile.@internal;

using @base = cmd.compile.@internal.@base_package;
using ir = cmd.compile.@internal.ir_package;
using types = cmd.compile.@internal.types_package;

using fmt = fmt_package;
using constant = go.constant_package;
using token = go.token_package;


// package all the arguments that match a ... T parameter into a []T.

using System;
public static partial class typecheck_package {

public static ir.Node MakeDotArgs(ptr<types.Type> _addr_typ, slice<ir.Node> args) {
    ref types.Type typ = ref _addr_typ.val;

    ir.Node n = default;
    if (len(args) == 0) {
        n = NodNil();
        n.SetType(typ);
    }
    else
 {
        var lit = ir.NewCompLitExpr(@base.Pos, ir.OCOMPLIT, ir.TypeNode(typ), null);
        lit.List.Append(args);
        lit.SetImplicit(true);
        n = lit;
    }
    n = Expr(n);
    if (n.Type() == null) {
        @base.Fatalf("mkdotargslice: typecheck failed");
    }
    return n;
}

// FixVariadicCall rewrites calls to variadic functions to use an
// explicit ... argument if one is not already present.
public static void FixVariadicCall(ptr<ir.CallExpr> _addr_call) {
    ref ir.CallExpr call = ref _addr_call.val;

    var fntype = call.X.Type();
    if (!fntype.IsVariadic() || call.IsDDD) {
        return ;
    }
    var vi = fntype.NumParams() - 1;
    var vt = fntype.Params().Field(vi).Type;

    var args = call.Args;
    var extra = args[(int)vi..];
    var slice = MakeDotArgs(_addr_vt, extra);
    foreach (var (i) in extra) {
        extra[i] = null; // allow GC
    }    call.Args = append(args[..(int)vi], slice);
    call.IsDDD = true;
}

// ClosureType returns the struct type used to hold all the information
// needed in the closure for clo (clo must be a OCLOSURE node).
// The address of a variable of the returned type can be cast to a func.
public static ptr<types.Type> ClosureType(ptr<ir.ClosureExpr> _addr_clo) {
    ref ir.ClosureExpr clo = ref _addr_clo.val;
 
    // Create closure in the form of a composite literal.
    // supposing the closure captures an int i and a string s
    // and has one float64 argument and no results,
    // the generated code looks like:
    //
    //    clos = &struct{.F uintptr; i *int; s *string}{func.1, &i, &s}
    //
    // The use of the struct provides type information to the garbage
    // collector so that it can walk the closure. We could use (in this case)
    // [3]unsafe.Pointer instead, but that would leave the gc in the dark.
    // The information appears in the binary in the form of type descriptors;
    // the struct is unnamed so that closures in multiple packages with the
    // same struct type can share the descriptor.
    ptr<types.Field> fields = new slice<ptr<types.Field>>(new ptr<types.Field>[] { types.NewField(base.Pos,Lookup(".F"),types.Types[types.TUINTPTR]) });
    foreach (var (_, v) in clo.Func.ClosureVars) {
        var typ = v.Type();
        if (!v.Byval()) {
            typ = types.NewPtr(typ);
        }
        fields = append(fields, types.NewField(@base.Pos, v.Sym(), typ));
    }    typ = types.NewStruct(types.NoPkg, fields);
    typ.SetNoalg(true);
    return _addr_typ!;
}

// PartialCallType returns the struct type used to hold all the information
// needed in the closure for n (n must be a OCALLPART node).
// The address of a variable of the returned type can be cast to a func.
public static ptr<types.Type> PartialCallType(ptr<ir.SelectorExpr> _addr_n) {
    ref ir.SelectorExpr n = ref _addr_n.val;

    var t = types.NewStruct(types.NoPkg, new slice<ptr<types.Field>>(new ptr<types.Field>[] { types.NewField(base.Pos,Lookup("F"),types.Types[types.TUINTPTR]), types.NewField(base.Pos,Lookup("R"),n.X.Type()) }));
    t.SetNoalg(true);
    return _addr_t!;
}

// True if we are typechecking an inline body in ImportedBody below. We use this
// flag to not create a new closure function in tcClosure when we are just
// typechecking an inline body, as opposed to the body of a real function.
private static bool inTypeCheckInl = default;

// ImportedBody returns immediately if the inlining information for fn is
// populated. Otherwise, fn must be an imported function. If so, ImportedBody
// loads in the dcls and body for fn, and typechecks as needed.
public static void ImportedBody(ptr<ir.Func> _addr_fn) => func((defer, _, _) => {
    ref ir.Func fn = ref _addr_fn.val;

    if (fn.Inl.Body != null) {
        return ;
    }
    var lno = ir.SetPos(fn.Nname); 

    // When we load an inlined body, we need to allow OADDR
    // operations on untyped expressions. We will fix the
    // addrtaken flags on all the arguments of the OADDR with the
    // computeAddrtaken call below (after we typecheck the body).
    // TODO: export/import types and addrtaken marks along with inlined bodies,
    // so this will be unnecessary.
    IncrementalAddrtaken = false;
    defer(() => {
        if (DirtyAddrtaken) {
            ComputeAddrtaken(fn.Inl.Body); // compute addrtaken marks once types are available
            DirtyAddrtaken = false;
        }
        IncrementalAddrtaken = true;
    }());

    ImportBody(fn); 

    // Stmts(fn.Inl.Body) below is only for imported functions;
    // their bodies may refer to unsafe as long as the package
    // was marked safe during import (which was checked then).
    // the ->inl of a local function has been typechecked before CanInline copied it.
    var pkg = fnpkg(_addr_fn.Nname);

    if (pkg == types.LocalPkg || pkg == null) {
        return ; // ImportedBody on local function
    }
    if (@base.Flag.LowerM > 2 || @base.Debug.Export != 0) {
        fmt.Printf("typecheck import [%v] %L { %v }\n", fn.Sym(), fn, ir.Nodes(fn.Inl.Body));
    }
    if (!go117ExportTypes) { 
        // If we didn't export & import types, typecheck the code here.
        var savefn = ir.CurFunc;
        ir.CurFunc = fn;
        if (inTypeCheckInl) {
            @base.Fatalf("inTypeCheckInl should not be set recursively");
        }
        inTypeCheckInl = true;
        Stmts(fn.Inl.Body);
        inTypeCheckInl = false;
        ir.CurFunc = savefn;
    }
    @base.Pos = lno;
});

// Get the function's package. For ordinary functions it's on the ->sym, but for imported methods
// the ->sym can be re-used in the local package, so peel it off the receiver's type.
private static ptr<types.Pkg> fnpkg(ptr<ir.Name> _addr_fn) {
    ref ir.Name fn = ref _addr_fn.val;

    if (ir.IsMethod(fn)) { 
        // method
        var rcvr = fn.Type().Recv().Type;

        if (rcvr.IsPtr()) {
            rcvr = rcvr.Elem();
        }
        if (rcvr.Sym() == null) {
            @base.Fatalf("receiver with no sym: [%v] %L  (%v)", fn.Sym(), fn, rcvr);
        }
        return _addr_rcvr.Sym().Pkg!;
    }
    return _addr_fn.Sym().Pkg!;
}

// ClosureName generates a new unique name for a closure within
// outerfunc.
public static ptr<types.Sym> ClosureName(ptr<ir.Func> _addr_outerfunc) {
    ref ir.Func outerfunc = ref _addr_outerfunc.val;

    @string outer = "glob.";
    @string prefix = "func";
    var gen = _addr_globClosgen;

    if (outerfunc != null) {
        if (outerfunc.OClosure != null) {
            prefix = "";
        }
        outer = ir.FuncName(outerfunc); 

        // There may be multiple functions named "_". In those
        // cases, we can't use their individual Closgens as it
        // would lead to name clashes.
        if (!ir.IsBlank(outerfunc.Nname)) {
            gen = _addr_outerfunc.Closgen;
        }
    }
    gen.val++;
    return _addr_Lookup(fmt.Sprintf("%s.%s%d", outer, prefix, gen.val))!;
}

// globClosgen is like Func.Closgen, but for the global scope.
private static int globClosgen = default;

// MethodValueWrapper returns the DCLFUNC node representing the
// wrapper function (*-fm) needed for the given method value. If the
// wrapper function hasn't already been created yet, it's created and
// added to Target.Decls.
//
// TODO(mdempsky): Move into walk. This isn't part of type checking.
public static ptr<ir.Func> MethodValueWrapper(ptr<ir.SelectorExpr> _addr_dot) {
    ref ir.SelectorExpr dot = ref _addr_dot.val;

    if (dot.Op() != ir.OCALLPART) {
        @base.Fatalf("MethodValueWrapper: unexpected %v (%v)", dot, dot.Op());
    }
    var t0 = dot.Type();
    var meth = dot.Sel;
    var rcvrtype = dot.X.Type();
    var sym = ir.MethodSymSuffix(rcvrtype, meth, "-fm");

    if (sym.Uniq()) {
        return sym.Def._<ptr<ir.Func>>();
    }
    sym.SetUniq(true);

    var savecurfn = ir.CurFunc;
    var saveLineNo = @base.Pos;
    ir.CurFunc = null; 

    // Set line number equal to the line number where the method is declared.
    {
        var pos = dot.Selection.Pos;

        if (pos.IsKnown()) {
            @base.Pos = pos;
        }
    } 
    // Note: !dot.Selection.Pos.IsKnown() happens for method expressions where
    // the method is implicitly declared. The Error method of the
    // built-in error type is one such method.  We leave the line
    // number at the use of the method expression in this
    // case. See issue 29389.

    var tfn = ir.NewFuncType(@base.Pos, null, NewFuncParams(t0.Params(), true), NewFuncParams(t0.Results(), false));

    var fn = DeclFunc(sym, tfn);
    fn.SetDupok(true);
    fn.SetNeedctxt(true);
    fn.SetWrapper(true); 

    // Declare and initialize variable holding receiver.
    var ptr = ir.NewNameAt(@base.Pos, Lookup(".this"));
    ptr.Class = ir.PAUTOHEAP;
    ptr.SetType(rcvrtype);
    ptr.Curfn = fn;
    ptr.SetIsClosureVar(true);
    ptr.SetByval(true);
    fn.ClosureVars = append(fn.ClosureVars, ptr);

    var call = ir.NewCallExpr(@base.Pos, ir.OCALL, ir.NewSelectorExpr(@base.Pos, ir.OXDOT, ptr, meth), null);
    call.Args = ir.ParamNames(tfn.Type());
    call.IsDDD = tfn.Type().IsVariadic();

    ir.Node body = call;
    if (t0.NumResults() != 0) {
        var ret = ir.NewReturnStmt(@base.Pos, null);
        ret.Results = new slice<ir.Node>(new ir.Node[] { call });
        body = ret;
    }
    fn.Body = new slice<ir.Node>(new ir.Node[] { body });
    FinishFuncBody();

    Func(fn); 
    // Need to typecheck the body of the just-generated wrapper.
    // typecheckslice() requires that Curfn is set when processing an ORETURN.
    ir.CurFunc = fn;
    Stmts(fn.Body);
    sym.Def = fn;
    Target.Decls = append(Target.Decls, fn);
    ir.CurFunc = savecurfn;
    @base.Pos = saveLineNo;

    return _addr_fn!;
}

// tcClosure typechecks an OCLOSURE node. It also creates the named
// function associated with the closure.
// TODO: This creation of the named function should probably really be done in a
// separate pass from type-checking.
private static void tcClosure(ptr<ir.ClosureExpr> _addr_clo, nint top) {
    ref ir.ClosureExpr clo = ref _addr_clo.val;

    var fn = clo.Func; 
    // Set current associated iota value, so iota can be used inside
    // function in ConstSpec, see issue #22344
    {
        var x = getIotaValue();

        if (x >= 0) {
            fn.Iota = x;
        }
    }

    fn.SetClosureCalled(top & ctxCallee != 0); 

    // Do not typecheck fn twice, otherwise, we will end up pushing
    // fn to Target.Decls multiple times, causing InitLSym called twice.
    // See #30709
    if (fn.Typecheck() == 1) {
        clo.SetType(fn.Type());
        return ;
    }
    if (!inTypeCheckInl) {
        fn.Nname.SetSym(ClosureName(_addr_ir.CurFunc));
        ir.MarkFunc(fn.Nname);
    }
    Func(fn);
    clo.SetType(fn.Type()); 

    // Type check the body now, but only if we're inside a function.
    // At top level (in a variable initialization: curfn==nil) we're not
    // ready to type check code yet; we'll check it later, because the
    // underlying closure function we create is added to Target.Decls.
    if (ir.CurFunc != null && clo.Type() != null) {
        var oldfn = ir.CurFunc;
        ir.CurFunc = fn;
        Stmts(fn.Body);
        ir.CurFunc = oldfn;
    }
    nint @out = 0;
    foreach (var (_, v) in fn.ClosureVars) {
        if (v.Type() == null) { 
            // If v.Type is nil, it means v looked like it was going to be
            // used in the closure, but isn't. This happens in struct
            // literals like s{f: x} where we can't distinguish whether f is
            // a field identifier or expression until resolving s.
            continue;
        }
        Expr(v.Outer);

        fn.ClosureVars[out] = v;
        out++;
    }    fn.ClosureVars = fn.ClosureVars[..(int)out];

    if (@base.Flag.W > 1) {
        var s = fmt.Sprintf("New closure func: %s", ir.FuncName(fn));
        ir.Dump(s, fn);
    }
    if (!inTypeCheckInl) { 
        // Add function to Target.Decls once only when we give it a name
        Target.Decls = append(Target.Decls, fn);
    }
}

// type check function definition
// To be called by typecheck, not directly.
// (Call typecheck.Func instead.)
private static void tcFunc(ptr<ir.Func> _addr_n) => func((defer, _, _) => {
    ref ir.Func n = ref _addr_n.val;

    if (@base.EnableTrace && @base.Flag.LowerT) {
        defer(tracePrint("tcFunc", n)(null));
    }
    n.Nname = AssignExpr(n.Nname)._<ptr<ir.Name>>();
    var t = n.Nname.Type();
    if (t == null) {
        return ;
    }
    var rcvr = t.Recv();
    if (rcvr != null && n.Shortname != null) {
        var m = addmethod(n, n.Shortname, t, true, n.Pragma & ir.Nointerface != 0);
        if (m == null) {
            return ;
        }
        n.Nname.SetSym(ir.MethodSym(rcvr.Type, n.Shortname));
        Declare(n.Nname, ir.PFUNC);
    }
});

// tcCall typechecks an OCALL node.
private static ir.Node tcCall(ptr<ir.CallExpr> _addr_n, nint top) => func((_, panic, _) => {
    ref ir.CallExpr n = ref _addr_n.val;

    n.Use = ir.CallUseExpr;
    if (top == ctxStmt) {
        n.Use = ir.CallUseStmt;
    }
    Stmts(n.Init()); // imported rewritten f(g()) calls (#30907)
    n.X = typecheck(n.X, ctxExpr | ctxType | ctxCallee);
    if (n.X.Diag()) {
        n.SetDiag(true);
    }
    var l = n.X;

    if (l.Op() == ir.ONAME && l._<ptr<ir.Name>>().BuiltinOp != 0) {
        l = l._<ptr<ir.Name>>();
        if (n.IsDDD && l.BuiltinOp != ir.OAPPEND) {
            @base.Errorf("invalid use of ... with builtin %v", l);
        }

        if (l.BuiltinOp == ir.OAPPEND || l.BuiltinOp == ir.ODELETE || l.BuiltinOp == ir.OMAKE || l.BuiltinOp == ir.OPRINT || l.BuiltinOp == ir.OPRINTN || l.BuiltinOp == ir.ORECOVER)
        {
            n.SetOp(l.BuiltinOp);
            n.X = null;
            n.SetTypecheck(0); // re-typechecking new op is OK, not a loop
            return typecheck(n, top);
            goto __switch_break0;
        }
        if (l.BuiltinOp == ir.OCAP || l.BuiltinOp == ir.OCLOSE || l.BuiltinOp == ir.OIMAG || l.BuiltinOp == ir.OLEN || l.BuiltinOp == ir.OPANIC || l.BuiltinOp == ir.OREAL)
        {
            typecheckargs(n);
            fallthrough = true;
        }
        if (fallthrough || l.BuiltinOp == ir.ONEW || l.BuiltinOp == ir.OALIGNOF || l.BuiltinOp == ir.OOFFSETOF || l.BuiltinOp == ir.OSIZEOF)
        {
            var (arg, ok) = needOneArg(n, "%v", n.Op());
            if (!ok) {
                n.SetType(null);
                return n;
            }
            var u = ir.NewUnaryExpr(n.Pos(), l.BuiltinOp, arg);
            return typecheck(ir.InitExpr(n.Init(), u), top); // typecheckargs can add to old.Init
            goto __switch_break0;
        }
        if (l.BuiltinOp == ir.OCOMPLEX || l.BuiltinOp == ir.OCOPY || l.BuiltinOp == ir.OUNSAFEADD || l.BuiltinOp == ir.OUNSAFESLICE)
        {
            typecheckargs(n);
            var (arg1, arg2, ok) = needTwoArgs(n);
            if (!ok) {
                n.SetType(null);
                return n;
            }
            var b = ir.NewBinaryExpr(n.Pos(), l.BuiltinOp, arg1, arg2);
            return typecheck(ir.InitExpr(n.Init(), b), top); // typecheckargs can add to old.Init
            goto __switch_break0;
        }
        // default: 
            @base.Fatalf("unknown builtin %v", l);

        __switch_break0:;
        panic("unreachable");
    }
    n.X = DefaultLit(n.X, null);
    l = n.X;
    if (l.Op() == ir.OTYPE) {
        if (n.IsDDD) {
            if (!l.Type().Broke()) {
                @base.Errorf("invalid use of ... in type conversion to %v", l.Type());
            }
            n.SetDiag(true);
        }
        (arg, ok) = needOneArg(n, "conversion to %v", l.Type());
        if (!ok) {
            n.SetType(null);
            return n;
        }
        var n = ir.NewConvExpr(n.Pos(), ir.OCONV, null, arg);
        n.SetType(l.Type());
        return tcConv(n);
    }
    typecheckargs(n);
    var t = l.Type();
    if (t == null) {
        n.SetType(null);
        return n;
    }
    types.CheckSize(t);


    if (l.Op() == ir.ODOTINTER) 
        n.SetOp(ir.OCALLINTER);
    else if (l.Op() == ir.ODOTMETH) 
        l = l._<ptr<ir.SelectorExpr>>();
        n.SetOp(ir.OCALLMETH); 

        // typecheckaste was used here but there wasn't enough
        // information further down the call chain to know if we
        // were testing a method receiver for unexported fields.
        // It isn't necessary, so just do a sanity check.
        var tp = t.Recv().Type;

        if (l.X == null || !types.Identical(l.X.Type(), tp)) {
            @base.Fatalf("method receiver");
        }
    else 
        n.SetOp(ir.OCALLFUNC);
        if (t.Kind() != types.TFUNC) {
            {
                var o = ir.Orig(l);

                if (o.Name() != null && types.BuiltinPkg.Lookup(o.Sym().Name).Def != null) { 
                    // be more specific when the non-function
                    // name matches a predeclared function
                    @base.Errorf("cannot call non-function %L, declared at %s", l, @base.FmtPos(o.Name().Pos()));
                }
                else
 {
                    @base.Errorf("cannot call non-function %L", l);
                }

            }
            n.SetType(null);
            return n;
        }
        typecheckaste(ir.OCALL, n.X, n.IsDDD, t.Params(), n.Args, () => fmt.Sprintf("argument to %v", n.X));
    if (t.NumResults() == 0) {
        return n;
    }
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
        return n;
    }
    if (top & (ctxMultiOK | ctxStmt) == 0) {
        @base.Errorf("multiple-value %v() in single-value context", l);
        return n;
    }
    n.SetType(l.Type().Results());
    return n;
});

// tcAppend typechecks an OAPPEND node.
private static ir.Node tcAppend(ptr<ir.CallExpr> _addr_n) {
    ref ir.CallExpr n = ref _addr_n.val;

    typecheckargs(n);
    var args = n.Args;
    if (len(args) == 0) {
        @base.Errorf("missing arguments to append");
        n.SetType(null);
        return n;
    }
    var t = args[0].Type();
    if (t == null) {
        n.SetType(null);
        return n;
    }
    n.SetType(t);
    if (!t.IsSlice()) {
        if (ir.IsNil(args[0])) {
            @base.Errorf("first argument to append must be typed slice; have untyped nil");
            n.SetType(null);
            return n;
        }
        @base.Errorf("first argument to append must be slice; have %L", t);
        n.SetType(null);
        return n;
    }
    if (n.IsDDD) {
        if (len(args) == 1) {
            @base.Errorf("cannot use ... on first argument to append");
            n.SetType(null);
            return n;
        }
        if (len(args) != 2) {
            @base.Errorf("too many arguments to append");
            n.SetType(null);
            return n;
        }
        if (t.Elem().IsKind(types.TUINT8) && args[1].Type().IsString()) {
            args[1] = DefaultLit(args[1], types.Types[types.TSTRING]);
            return n;
        }
        args[1] = AssignConv(args[1], t.Underlying(), "append");
        return n;
    }
    var @as = args[(int)1..];
    foreach (var (i, n) in as) {
        if (n.Type() == null) {
            continue;
        }
        as[i] = AssignConv(n, t.Elem(), "append");
        types.CheckSize(as[i].Type()); // ensure width is calculated for backend
    }    return n;
}

// tcClose typechecks an OCLOSE node.
private static ir.Node tcClose(ptr<ir.UnaryExpr> _addr_n) {
    ref ir.UnaryExpr n = ref _addr_n.val;

    n.X = Expr(n.X);
    n.X = DefaultLit(n.X, null);
    var l = n.X;
    var t = l.Type();
    if (t == null) {
        n.SetType(null);
        return n;
    }
    if (!t.IsChan()) {
        @base.Errorf("invalid operation: %v (non-chan type %v)", n, t);
        n.SetType(null);
        return n;
    }
    if (!t.ChanDir().CanSend()) {
        @base.Errorf("invalid operation: %v (cannot close receive-only channel)", n);
        n.SetType(null);
        return n;
    }
    return n;
}

// tcComplex typechecks an OCOMPLEX node.
private static ir.Node tcComplex(ptr<ir.BinaryExpr> _addr_n) {
    ref ir.BinaryExpr n = ref _addr_n.val;

    var l = Expr(n.X);
    var r = Expr(n.Y);
    if (l.Type() == null || r.Type() == null) {
        n.SetType(null);
        return n;
    }
    l, r = defaultlit2(l, r, false);
    if (l.Type() == null || r.Type() == null) {
        n.SetType(null);
        return n;
    }
    n.X = l;
    n.Y = r;

    if (!types.Identical(l.Type(), r.Type())) {
        @base.Errorf("invalid operation: %v (mismatched types %v and %v)", n, l.Type(), r.Type());
        n.SetType(null);
        return n;
    }
    ptr<types.Type> t;

    if (l.Type().Kind() == types.TIDEAL) 
        t = types.UntypedComplex;
    else if (l.Type().Kind() == types.TFLOAT32) 
        t = types.Types[types.TCOMPLEX64];
    else if (l.Type().Kind() == types.TFLOAT64) 
        t = types.Types[types.TCOMPLEX128];
    else 
        @base.Errorf("invalid operation: %v (arguments have type %v, expected floating-point)", n, l.Type());
        n.SetType(null);
        return n;
        n.SetType(t);
    return n;
}

// tcCopy typechecks an OCOPY node.
private static ir.Node tcCopy(ptr<ir.BinaryExpr> _addr_n) {
    ref ir.BinaryExpr n = ref _addr_n.val;

    n.SetType(types.Types[types.TINT]);
    n.X = Expr(n.X);
    n.X = DefaultLit(n.X, null);
    n.Y = Expr(n.Y);
    n.Y = DefaultLit(n.Y, null);
    if (n.X.Type() == null || n.Y.Type() == null) {
        n.SetType(null);
        return n;
    }
    if (n.X.Type().IsSlice() && n.Y.Type().IsString()) {
        if (types.Identical(n.X.Type().Elem(), types.ByteType)) {
            return n;
        }
        @base.Errorf("arguments to copy have different element types: %L and string", n.X.Type());
        n.SetType(null);
        return n;
    }
    if (!n.X.Type().IsSlice() || !n.Y.Type().IsSlice()) {
        if (!n.X.Type().IsSlice() && !n.Y.Type().IsSlice()) {
            @base.Errorf("arguments to copy must be slices; have %L, %L", n.X.Type(), n.Y.Type());
        }
        else if (!n.X.Type().IsSlice()) {
            @base.Errorf("first argument to copy should be slice; have %L", n.X.Type());
        }
        else
 {
            @base.Errorf("second argument to copy should be slice or string; have %L", n.Y.Type());
        }
        n.SetType(null);
        return n;
    }
    if (!types.Identical(n.X.Type().Elem(), n.Y.Type().Elem())) {
        @base.Errorf("arguments to copy have different element types: %L and %L", n.X.Type(), n.Y.Type());
        n.SetType(null);
        return n;
    }
    return n;
}

// tcDelete typechecks an ODELETE node.
private static ir.Node tcDelete(ptr<ir.CallExpr> _addr_n) {
    ref ir.CallExpr n = ref _addr_n.val;

    typecheckargs(n);
    var args = n.Args;
    if (len(args) == 0) {
        @base.Errorf("missing arguments to delete");
        n.SetType(null);
        return n;
    }
    if (len(args) == 1) {
        @base.Errorf("missing second (key) argument to delete");
        n.SetType(null);
        return n;
    }
    if (len(args) != 2) {
        @base.Errorf("too many arguments to delete");
        n.SetType(null);
        return n;
    }
    var l = args[0];
    var r = args[1];
    if (l.Type() != null && !l.Type().IsMap()) {
        @base.Errorf("first argument to delete must be map; have %L", l.Type());
        n.SetType(null);
        return n;
    }
    args[1] = AssignConv(r, l.Type().Key(), "delete");
    return n;
}

// tcMake typechecks an OMAKE node.
private static ir.Node tcMake(ptr<ir.CallExpr> _addr_n) {
    ref ir.CallExpr n = ref _addr_n.val;

    var args = n.Args;
    if (len(args) == 0) {
        @base.Errorf("missing argument to make");
        n.SetType(null);
        return n;
    }
    n.Args = null;
    ref var l = ref heap(args[0], out ptr<var> _addr_l);
    l = typecheck(l, ctxType);
    var t = l.Type();
    if (t == null) {
        n.SetType(null);
        return n;
    }
    nint i = 1;
    ir.Node nn = default;

    if (t.Kind() == types.TSLICE) 
        if (i >= len(args)) {
            @base.Errorf("missing len argument to make(%v)", t);
            n.SetType(null);
            return n;
        }
        l = args[i];
        i++;
        l = Expr(l);
        ref ir.Node r = ref heap(out ptr<ir.Node> _addr_r);
        if (i < len(args)) {
            r = args[i];
            i++;
            r = Expr(r);
        }
        if (l.Type() == null || (r != null && r.Type() == null)) {
            n.SetType(null);
            return n;
        }
        if (!checkmake(t, "len", _addr_l) || r != null && !checkmake(t, "cap", _addr_r)) {
            n.SetType(null);
            return n;
        }
        if (ir.IsConst(l, constant.Int) && r != null && ir.IsConst(r, constant.Int) && constant.Compare(l.Val(), token.GTR, r.Val())) {
            @base.Errorf("len larger than cap in make(%v)", t);
            n.SetType(null);
            return n;
        }
        nn = ir.NewMakeExpr(n.Pos(), ir.OMAKESLICE, l, r);
    else if (t.Kind() == types.TMAP) 
        if (i < len(args)) {
            l = args[i];
            i++;
            l = Expr(l);
            l = DefaultLit(l, types.Types[types.TINT]);
            if (l.Type() == null) {
                n.SetType(null);
                return n;
            }
            if (!checkmake(t, "size", _addr_l)) {
                n.SetType(null);
                return n;
            }
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
            l = Expr(l);
            l = DefaultLit(l, types.Types[types.TINT]);
            if (l.Type() == null) {
                n.SetType(null);
                return n;
            }
            if (!checkmake(t, "buffer", _addr_l)) {
                n.SetType(null);
                return n;
            }
        }
        else
 {
            l = ir.NewInt(0);
        }
        nn = ir.NewMakeExpr(n.Pos(), ir.OMAKECHAN, l, null);
    else 
        @base.Errorf("cannot make type %v", t);
        n.SetType(null);
        return n;
        if (i < len(args)) {
        @base.Errorf("too many arguments to make(%v)", t);
        n.SetType(null);
        return n;
    }
    nn.SetType(t);
    return nn;
}

// tcMakeSliceCopy typechecks an OMAKESLICECOPY node.
private static ir.Node tcMakeSliceCopy(ptr<ir.MakeExpr> _addr_n) {
    ref ir.MakeExpr n = ref _addr_n.val;
 
    // Errors here are Fatalf instead of Errorf because only the compiler
    // can construct an OMAKESLICECOPY node.
    // Components used in OMAKESCLICECOPY that are supplied by parsed source code
    // have already been typechecked in OMAKE and OCOPY earlier.
    var t = n.Type();

    if (t == null) {
        @base.Fatalf("no type specified for OMAKESLICECOPY");
    }
    if (!t.IsSlice()) {
        @base.Fatalf("invalid type %v for OMAKESLICECOPY", n.Type());
    }
    if (n.Len == null) {
        @base.Fatalf("missing len argument for OMAKESLICECOPY");
    }
    if (n.Cap == null) {
        @base.Fatalf("missing slice argument to copy for OMAKESLICECOPY");
    }
    n.Len = Expr(n.Len);
    n.Cap = Expr(n.Cap);

    n.Len = DefaultLit(n.Len, types.Types[types.TINT]);

    if (!n.Len.Type().IsInteger() && n.Type().Kind() != types.TIDEAL) {
        @base.Errorf("non-integer len argument in OMAKESLICECOPY");
    }
    if (ir.IsConst(n.Len, constant.Int)) {
        if (ir.ConstOverflow(n.Len.Val(), types.Types[types.TINT])) {
            @base.Fatalf("len for OMAKESLICECOPY too large");
        }
        if (constant.Sign(n.Len.Val()) < 0) {
            @base.Fatalf("len for OMAKESLICECOPY must be non-negative");
        }
    }
    return n;
}

// tcNew typechecks an ONEW node.
private static ir.Node tcNew(ptr<ir.UnaryExpr> _addr_n) {
    ref ir.UnaryExpr n = ref _addr_n.val;

    if (n.X == null) { 
        // Fatalf because the OCALL above checked for us,
        // so this must be an internally-generated mistake.
        @base.Fatalf("missing argument to new");
    }
    var l = n.X;
    l = typecheck(l, ctxType);
    var t = l.Type();
    if (t == null) {
        n.SetType(null);
        return n;
    }
    n.X = l;
    n.SetType(types.NewPtr(t));
    return n;
}

// tcPanic typechecks an OPANIC node.
private static ir.Node tcPanic(ptr<ir.UnaryExpr> _addr_n) {
    ref ir.UnaryExpr n = ref _addr_n.val;

    n.X = Expr(n.X);
    n.X = AssignConv(n.X, types.Types[types.TINTER], "argument to panic");
    if (n.X.Type() == null) {
        n.SetType(null);
        return n;
    }
    return n;
}

// tcPrint typechecks an OPRINT or OPRINTN node.
private static ir.Node tcPrint(ptr<ir.CallExpr> _addr_n) {
    ref ir.CallExpr n = ref _addr_n.val;

    typecheckargs(n);
    var ls = n.Args;
    foreach (var (i1, n1) in ls) { 
        // Special case for print: int constant is int64, not int.
        if (ir.IsConst(n1, constant.Int)) {
            ls[i1] = DefaultLit(ls[i1], types.Types[types.TINT64]);
        }
        else
 {
            ls[i1] = DefaultLit(ls[i1], null);
        }
    }    return n;
}

// tcRealImag typechecks an OREAL or OIMAG node.
private static ir.Node tcRealImag(ptr<ir.UnaryExpr> _addr_n) {
    ref ir.UnaryExpr n = ref _addr_n.val;

    n.X = Expr(n.X);
    var l = n.X;
    var t = l.Type();
    if (t == null) {
        n.SetType(null);
        return n;
    }

    if (t.Kind() == types.TIDEAL) 
        n.SetType(types.UntypedFloat);
    else if (t.Kind() == types.TCOMPLEX64) 
        n.SetType(types.Types[types.TFLOAT32]);
    else if (t.Kind() == types.TCOMPLEX128) 
        n.SetType(types.Types[types.TFLOAT64]);
    else 
        @base.Errorf("invalid argument %L for %v", l, n.Op());
        n.SetType(null);
        return n;
        return n;
}

// tcRecover typechecks an ORECOVER node.
private static ir.Node tcRecover(ptr<ir.CallExpr> _addr_n) {
    ref ir.CallExpr n = ref _addr_n.val;

    if (len(n.Args) != 0) {
        @base.Errorf("too many arguments to recover");
        n.SetType(null);
        return n;
    }
    n.SetType(types.Types[types.TINTER]);
    return n;
}

// tcUnsafeAdd typechecks an OUNSAFEADD node.
private static ptr<ir.BinaryExpr> tcUnsafeAdd(ptr<ir.BinaryExpr> _addr_n) {
    ref ir.BinaryExpr n = ref _addr_n.val;

    if (!types.AllowsGoVersion(curpkg(), 1, 17)) {
        @base.ErrorfVers("go1.17", "unsafe.Add");
        n.SetType(null);
        return _addr_n!;
    }
    n.X = AssignConv(Expr(n.X), types.Types[types.TUNSAFEPTR], "argument to unsafe.Add");
    n.Y = DefaultLit(Expr(n.Y), types.Types[types.TINT]);
    if (n.X.Type() == null || n.Y.Type() == null) {
        n.SetType(null);
        return _addr_n!;
    }
    if (!n.Y.Type().IsInteger()) {
        n.SetType(null);
        return _addr_n!;
    }
    n.SetType(n.X.Type());
    return _addr_n!;
}

// tcUnsafeSlice typechecks an OUNSAFESLICE node.
private static ptr<ir.BinaryExpr> tcUnsafeSlice(ptr<ir.BinaryExpr> _addr_n) {
    ref ir.BinaryExpr n = ref _addr_n.val;

    if (!types.AllowsGoVersion(curpkg(), 1, 17)) {
        @base.ErrorfVers("go1.17", "unsafe.Slice");
        n.SetType(null);
        return _addr_n!;
    }
    n.X = Expr(n.X);
    n.Y = Expr(n.Y);
    if (n.X.Type() == null || n.Y.Type() == null) {
        n.SetType(null);
        return _addr_n!;
    }
    var t = n.X.Type();
    if (!t.IsPtr()) {
        @base.Errorf("first argument to unsafe.Slice must be pointer; have %L", t);
    }
    else if (t.Elem().NotInHeap()) { 
        // TODO(mdempsky): This can be relaxed, but should only affect the
        // Go runtime itself. End users should only see //go:notinheap
        // types due to incomplete C structs in cgo, and those types don't
        // have a meaningful size anyway.
        @base.Errorf("unsafe.Slice of incomplete (or unallocatable) type not allowed");
    }
    if (!checkunsafeslice(_addr_n.Y)) {
        n.SetType(null);
        return _addr_n!;
    }
    n.SetType(types.NewSlice(t.Elem()));
    return _addr_n!;
}

} // end typecheck_package
