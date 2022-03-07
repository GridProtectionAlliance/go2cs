// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package noder -- go2cs converted at 2022 March 06 23:13:47 UTC
// import "cmd/compile/internal/noder" ==> using noder = go.cmd.compile.@internal.noder_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\noder\expr.go
using @base = go.cmd.compile.@internal.@base_package;
using ir = go.cmd.compile.@internal.ir_package;
using syntax = go.cmd.compile.@internal.syntax_package;
using typecheck = go.cmd.compile.@internal.typecheck_package;
using types = go.cmd.compile.@internal.types_package;
using types2 = go.cmd.compile.@internal.types2_package;
using src = go.cmd.@internal.src_package;

namespace go.cmd.compile.@internal;

public static partial class noder_package {

private static ir.Node expr(this ptr<irgen> _addr_g, syntax.Expr expr) {
    ref irgen g = ref _addr_g.val;

    if (expr == null) {
        return null;
    }
    {
        ptr<syntax.Name> expr__prev1 = expr;

        ptr<syntax.Name> (expr, ok) = expr._<ptr<syntax.Name>>();

        if (ok && expr.Value == "_") {
            return ir.BlankNode;
        }
        expr = expr__prev1;

    }


    var (tv, ok) = g.info.Types[expr];
    if (!ok) {
        @base.FatalfAt(g.pos(expr), "missing type for %v (%T)", expr, expr);
    }

    if (tv.IsBuiltin()) 
        // Qualified builtins, such as unsafe.Add and unsafe.Slice.
        {
            ptr<syntax.Name> expr__prev1 = expr;

            (expr, ok) = expr._<ptr<syntax.SelectorExpr>>();

            if (ok) {
                {
                    ptr<syntax.Name> (name, ok) = expr.X._<ptr<syntax.Name>>();

                    if (ok) {
                        {
                            ptr<types2.PkgName> (_, ok) = g.info.Uses[name]._<ptr<types2.PkgName>>();

                            if (ok) {
                                return g.use(expr.Sel);
                            }
                        }

                    }
                }

            }
            expr = expr__prev1;

        }

        return g.use(expr._<ptr<syntax.Name>>());
    else if (tv.IsType()) 
        return ir.TypeNode(g.typ(tv.Type));
    else if (tv.IsValue() || tv.IsVoid())     else 
        @base.FatalfAt(g.pos(expr), "unrecognized type-checker result");
    // The gc backend expects all expressions to have a concrete type, and
    // types2 mostly satisfies this expectation already. But there are a few
    // cases where the Go spec doesn't require converting to concrete type,
    // and so types2 leaves them untyped. So we need to fix those up here.
    var typ = tv.Type;
    {
        ptr<types2.Basic> (basic, ok) = typ._<ptr<types2.Basic>>();

        if (ok && basic.Info() & types2.IsUntyped != 0) {

            if (basic.Kind() == types2.UntypedNil)             else if (basic.Kind() == types2.UntypedBool) 
                typ = types2.Typ[types2.Bool]; // expression in "if" or "for" condition
            else if (basic.Kind() == types2.UntypedString) 
                typ = types2.Typ[types2.String]; // argument to "append" or "copy" calls
            else 
                @base.FatalfAt(g.pos(expr), "unexpected untyped type: %v", basic);
            
        }
    } 

    // Constant expression.
    if (tv.Value != null) {
        return Const(g.pos(expr), g.typ(typ), tv.Value);
    }
    var n = g.expr0(typ, expr);
    if (n.Typecheck() != 1 && n.Typecheck() != 3) {
        @base.FatalfAt(g.pos(expr), "missed typecheck: %+v", n);
    }
    if (!g.match(n.Type(), typ, tv.HasOk())) {
        @base.FatalfAt(g.pos(expr), "expected %L to have type %v", n, typ);
    }
    return n;

}

private static ir.Node expr0(this ptr<irgen> _addr_g, types2.Type typ, syntax.Expr expr) => func((_, panic, _) => {
    ref irgen g = ref _addr_g.val;

    var pos = g.pos(expr);

    switch (expr.type()) {
        case ptr<syntax.Name> expr:
            {
                ptr<types2.Nil> (_, isNil) = g.info.Uses[expr]._<ptr<types2.Nil>>();

                if (isNil) {
                    return Nil(pos, g.typ(typ));
                }

            }

            return g.use(expr);
            break;
        case ptr<syntax.CompositeLit> expr:
            return g.compLit(typ, expr);
            break;
        case ptr<syntax.FuncLit> expr:
            return g.funcLit(typ, expr);
            break;
        case ptr<syntax.AssertExpr> expr:
            return Assert(pos, g.expr(expr.X), g.typeExpr(expr.Type));
            break;
        case ptr<syntax.CallExpr> expr:
            var fun = g.expr(expr.Fun); 

            // The key for the Inferred map is the CallExpr (if inferring
            // types required the function arguments) or the IndexExpr below
            // (if types could be inferred without the function arguments).
            {
                var inferred__prev1 = inferred;

                var (inferred, ok) = g.info.Inferred[expr];

                if (ok && len(inferred.Targs) > 0) { 
                    // This is the case where inferring types required the
                    // types of the function arguments.
                    var targs = make_slice<ir.Node>(len(inferred.Targs));
                    {
                        var i__prev1 = i;
                        var targ__prev1 = targ;

                        foreach (var (__i, __targ) in inferred.Targs) {
                            i = __i;
                            targ = __targ;
                            targs[i] = ir.TypeNode(g.typ(targ));
                        }

                        i = i__prev1;
                        targ = targ__prev1;
                    }

                    if (fun.Op() == ir.OFUNCINST) { 
                        // Replace explicit type args with the full list that
                        // includes the additional inferred type args
                        fun._<ptr<ir.InstExpr>>().Targs = targs;

                    }
                    else
 { 
                        // Create a function instantiation here, given
                        // there are only inferred type args (e.g.
                        // min(5,6), where min is a generic function)
                        var inst = ir.NewInstExpr(pos, ir.OFUNCINST, fun, targs);
                        typed(fun.Type(), inst);
                        fun = inst;

                    }

                }

                inferred = inferred__prev1;

            }

            return Call(pos, g.typ(typ), fun, g.exprs(expr.ArgList), expr.HasDots);
            break;
        case ptr<syntax.IndexExpr> expr:
            targs = default;

            {
                var inferred__prev1 = inferred;

                (inferred, ok) = g.info.Inferred[expr];

                if (ok && len(inferred.Targs) > 0) { 
                    // This is the partial type inference case where the types
                    // can be inferred from other type arguments without using
                    // the types of the function arguments.
                    targs = make_slice<ir.Node>(len(inferred.Targs));
                    {
                        var i__prev1 = i;
                        var targ__prev1 = targ;

                        foreach (var (__i, __targ) in inferred.Targs) {
                            i = __i;
                            targ = __targ;
                            targs[i] = ir.TypeNode(g.typ(targ));
                        }

                        i = i__prev1;
                        targ = targ__prev1;
                    }
                }                {
                    ptr<syntax.ListExpr> (_, ok) = expr.Index._<ptr<syntax.ListExpr>>();


                    else if (ok) {
                        targs = g.exprList(expr.Index);
                    }
                    else
 {
                        var index = g.expr(expr.Index);
                        if (index.Op() != ir.OTYPE) { 
                            // This is just a normal index expression
                            return Index(pos, g.typ(typ), g.expr(expr.X), index);

                        } 
                        // This is generic function instantiation with a single type
                        targs = new slice<ir.Node>(new ir.Node[] { index });

                    } 
                    // This is a generic function instantiation (e.g. min[int]).
                    // Generic type instantiation is handled in the type
                    // section of expr() above (using g.typ).

                } 
                // This is a generic function instantiation (e.g. min[int]).
                // Generic type instantiation is handled in the type
                // section of expr() above (using g.typ).

                inferred = inferred__prev1;

            } 
            // This is a generic function instantiation (e.g. min[int]).
            // Generic type instantiation is handled in the type
            // section of expr() above (using g.typ).
            var x = g.expr(expr.X);
            if (x.Op() != ir.ONAME || x.Type().Kind() != types.TFUNC) {
                panic("Incorrect argument for generic func instantiation");
            }

            var n = ir.NewInstExpr(pos, ir.OFUNCINST, x, targs);
            typed(g.typ(typ), n);
            return n;
            break;
        case ptr<syntax.ParenExpr> expr:
            return g.expr(expr.X); // skip parens; unneeded after parse+typecheck
            break;
        case ptr<syntax.SelectorExpr> expr:
            {
                ptr<syntax.Name> (name, ok) = expr.X._<ptr<syntax.Name>>();

                if (ok) {
                    {
                        (_, ok) = g.info.Uses[name]._<ptr<types2.PkgName>>();

                        if (ok) {
                            return g.use(expr.Sel);
                        }

                    }

                }

            }

            return g.selectorExpr(pos, typ, expr);
            break;
        case ptr<syntax.SliceExpr> expr:
            return Slice(pos, g.typ(typ), g.expr(expr.X), g.expr(expr.Index[0]), g.expr(expr.Index[1]), g.expr(expr.Index[2]));
            break;
        case ptr<syntax.Operation> expr:
            if (expr.Y == null) {
                return Unary(pos, g.typ(typ), g.op(expr.Op, unOps[..]), g.expr(expr.X));
            }
            {
                var op = g.op(expr.Op, binOps[..]);


                if (op == ir.OEQ || op == ir.ONE || op == ir.OLT || op == ir.OLE || op == ir.OGT || op == ir.OGE) 
                    return Compare(pos, g.typ(typ), op, g.expr(expr.X), g.expr(expr.Y));
                else 
                    return Binary(pos, op, g.typ(typ), g.expr(expr.X), g.expr(expr.Y));

            }
            break;
        default:
        {
            var expr = expr.type();
            g.unhandled("expression", expr);
            panic("unreachable");
            break;
        }
    }

});

// selectorExpr resolves the choice of ODOT, ODOTPTR, OCALLPART (eventually
// ODOTMETH & ODOTINTER), and OMETHEXPR and deals with embedded fields here rather
// than in typecheck.go.
private static ir.Node selectorExpr(this ptr<irgen> _addr_g, src.XPos pos, types2.Type typ, ptr<syntax.SelectorExpr> _addr_expr) {
    ref irgen g = ref _addr_g.val;
    ref syntax.SelectorExpr expr = ref _addr_expr.val;

    var x = g.expr(expr.X);
    if (x.Type().HasTParam()) { 
        // Leave a method call on a type param as an OXDOT, since it can
        // only be fully transformed once it has an instantiated type.
        var n = ir.NewSelectorExpr(pos, ir.OXDOT, x, typecheck.Lookup(expr.Sel.Value));
        typed(g.typ(typ), n);
        return n;

    }
    var selinfo = g.info.Selections[expr]; 
    // Everything up to the last selection is an implicit embedded field access,
    // and the last selection is determined by selinfo.Kind().
    var index = selinfo.Index();
    var embeds = index[..(int)len(index) - 1];
    var last = index[len(index) - 1];

    var origx = x;
    foreach (var (_, ix) in embeds) {
        x = Implicit(DotField(pos, x, ix));
    }    var kind = selinfo.Kind();
    if (kind == types2.FieldVal) {
        return DotField(pos, x, last);
    }
    n = default;
    ptr<types2.Func> method2 = selinfo.Obj()._<ptr<types2.Func>>();

    if (kind == types2.MethodExpr) { 
        // OMETHEXPR is unusual in using directly the node and type of the
        // original OTYPE node (origx) before passing through embedded
        // fields, even though the method is selected from the type
        // (x.Type()) reached after following the embedded fields. We will
        // actually drop any ODOT nodes we created due to the embedded
        // fields.
        n = MethodExpr(pos, origx, x.Type(), last);

    }
    else
 { 
        // Add implicit addr/deref for method values, if needed.
        if (x.Type().IsInterface()) {
            n = DotMethod(pos, x, last);
        }
        else
 {
            ptr<types2.Signature> recvType2 = method2.Type()._<ptr<types2.Signature>>().Recv().Type();
            ptr<types2.Pointer> (_, wantPtr) = recvType2._<ptr<types2.Pointer>>();
            var havePtr = x.Type().IsPtr();

            if (havePtr != wantPtr) {
                if (havePtr) {
                    x = Implicit(Deref(pos, x.Type().Elem(), x));
                }
                else
 {
                    x = Implicit(Addr(pos, x));
                }

            }

            var recvType2Base = recvType2;
            if (wantPtr) {
                recvType2Base = types2.AsPointer(recvType2).Elem();
            }

            if (len(types2.AsNamed(recvType2Base).TParams()) > 0) { 
                // recvType2 is the original generic type that is
                // instantiated for this method call.
                // selinfo.Recv() is the instantiated type
                recvType2 = recvType2Base; 
                // method is the generic method associated with the gen type
                var method = g.obj(types2.AsNamed(recvType2).Method(last));
                n = ir.NewSelectorExpr(pos, ir.OCALLPART, x, method.Sym());
                n._<ptr<ir.SelectorExpr>>().Selection = types.NewField(pos, method.Sym(), method.Type());
                n._<ptr<ir.SelectorExpr>>().Selection.Nname = method;
                typed(method.Type(), n); 

                // selinfo.Targs() are the types used to
                // instantiate the type of receiver
                var targs2 = getTargs(_addr_selinfo);
                var targs = make_slice<ir.Node>(len(targs2));
                foreach (var (i, targ2) in targs2) {
                    targs[i] = ir.TypeNode(g.typ(targ2));
                } 

                // Create function instantiation with the type
                // args for the receiver type for the method call.
                n = ir.NewInstExpr(pos, ir.OFUNCINST, n, targs);
                typed(g.typ(typ), n);
                return n;

            }

            if (!g.match(x.Type(), recvType2, false)) {
                @base.FatalfAt(pos, "expected %L to have type %v", x, recvType2);
            }
            else
 {
                n = DotMethod(pos, x, last);
            }

        }
    }
    {
        var have = n.Sym();
        var want = g.selector(method2);

        if (have != want) {
            @base.FatalfAt(pos, "bad Sym: have %v, want %v", have, want);
        }
    }

    return n;

}

// getTargs gets the targs associated with the receiver of a selected method
private static slice<types2.Type> getTargs(ptr<types2.Selection> _addr_selinfo) {
    ref types2.Selection selinfo = ref _addr_selinfo.val;

    var r = selinfo.Recv();
    {
        var p = types2.AsPointer(r);

        if (p != null) {
            r = p.Elem();
        }
    }

    var n = types2.AsNamed(r);
    if (n == null) {
        @base.Fatalf("Incorrect type for selinfo %v", selinfo);
    }
    return n.TArgs();

}

private static slice<ir.Node> exprList(this ptr<irgen> _addr_g, syntax.Expr expr) {
    ref irgen g = ref _addr_g.val;

    switch (expr.type()) {
        case 
            return null;
            break;
        case ptr<syntax.ListExpr> expr:
            return g.exprs(expr.ElemList);
            break;
        default:
        {
            var expr = expr.type();
            return new slice<ir.Node>(new ir.Node[] { g.expr(expr) });
            break;
        }
    }

}

private static slice<ir.Node> exprs(this ptr<irgen> _addr_g, slice<syntax.Expr> exprs) {
    ref irgen g = ref _addr_g.val;

    var nodes = make_slice<ir.Node>(len(exprs));
    foreach (var (i, expr) in exprs) {
        nodes[i] = g.expr(expr);
    }    return nodes;
}

private static ir.Node compLit(this ptr<irgen> _addr_g, types2.Type typ, ptr<syntax.CompositeLit> _addr_lit) {
    ref irgen g = ref _addr_g.val;
    ref syntax.CompositeLit lit = ref _addr_lit.val;

    {
        ptr<types2.Pointer> (ptr, ok) = typ.Underlying()._<ptr<types2.Pointer>>();

        if (ok) {
            var n = ir.NewAddrExpr(g.pos(lit), g.compLit(ptr.Elem(), lit));
            n.SetOp(ir.OPTRLIT);
            return typed(g.typ(typ), n);
        }
    }


    ptr<types2.Struct> (_, isStruct) = typ.Underlying()._<ptr<types2.Struct>>();

    var exprs = make_slice<ir.Node>(len(lit.ElemList));
    {
        var elem__prev1 = elem;

        foreach (var (__i, __elem) in lit.ElemList) {
            i = __i;
            elem = __elem;
            switch (elem.type()) {
                case ptr<syntax.KeyValueExpr> elem:
                    if (isStruct) {
                        exprs[i] = ir.NewStructKeyExpr(g.pos(elem), g.name(elem.Key._<ptr<syntax.Name>>()), g.expr(elem.Value));
                    }
                    else
 {
                        exprs[i] = ir.NewKeyExpr(g.pos(elem), g.expr(elem.Key), g.expr(elem.Value));
                    }

                    break;
                default:
                {
                    var elem = elem.type();
                    exprs[i] = g.expr(elem);
                    break;
                }
            }

        }
        elem = elem__prev1;
    }

    n = ir.NewCompLitExpr(g.pos(lit), ir.OCOMPLIT, null, exprs);
    typed(g.typ(typ), n);
    return transformCompLit(n);

}

private static ir.Node funcLit(this ptr<irgen> _addr_g, types2.Type typ2, ptr<syntax.FuncLit> _addr_expr) {
    ref irgen g = ref _addr_g.val;
    ref syntax.FuncLit expr = ref _addr_expr.val;

    var fn = ir.NewFunc(g.pos(expr));
    fn.SetIsHiddenClosure(ir.CurFunc != null);

    fn.Nname = ir.NewNameAt(g.pos(expr), typecheck.ClosureName(ir.CurFunc));
    ir.MarkFunc(fn.Nname);
    var typ = g.typ(typ2);
    fn.Nname.Func = fn;
    fn.Nname.Defn = fn;
    typed(typ, fn.Nname);
    fn.SetTypecheck(1);

    fn.OClosure = ir.NewClosureExpr(g.pos(expr), fn);
    typed(typ, fn.OClosure);

    g.funcBody(fn, null, expr.Type, expr.Body);

    ir.FinishCaptureNames(fn.Pos(), ir.CurFunc, fn); 

    // TODO(mdempsky): ir.CaptureName should probably handle
    // copying these fields from the canonical variable.
    foreach (var (_, cv) in fn.ClosureVars) {
        cv.SetType(cv.Canonical().Type());
        cv.SetTypecheck(1);
        cv.SetWalkdef(1);
    }    g.target.Decls = append(g.target.Decls, fn);

    return fn.OClosure;

}

private static ptr<types.Type> typeExpr(this ptr<irgen> _addr_g, syntax.Expr typ) {
    ref irgen g = ref _addr_g.val;

    var n = g.expr(typ);
    if (n.Op() != ir.OTYPE) {
        @base.FatalfAt(g.pos(typ), "expected type: %L", n);
    }
    return _addr_n.Type()!;

}

} // end noder_package
