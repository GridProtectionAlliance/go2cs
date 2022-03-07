// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package typecheck -- go2cs converted at 2022 March 06 22:48:13 UTC
// import "cmd/compile/internal/typecheck" ==> using typecheck = go.cmd.compile.@internal.typecheck_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\typecheck\expr.go
using fmt = go.fmt_package;
using constant = go.go.constant_package;
using token = go.go.token_package;
using strings = go.strings_package;

using @base = go.cmd.compile.@internal.@base_package;
using ir = go.cmd.compile.@internal.ir_package;
using types = go.cmd.compile.@internal.types_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class typecheck_package {

    // tcAddr typechecks an OADDR node.
private static ir.Node tcAddr(ptr<ir.AddrExpr> _addr_n) {
    ref ir.AddrExpr n = ref _addr_n.val;

    n.X = Expr(n.X);
    if (n.X.Type() == null) {
        n.SetType(null);
        return n;
    }

    if (n.X.Op() == ir.OARRAYLIT || n.X.Op() == ir.OMAPLIT || n.X.Op() == ir.OSLICELIT || n.X.Op() == ir.OSTRUCTLIT) 
        n.SetOp(ir.OPTRLIT);
    else 
        checklvalue(n.X, "take the address of");
        var r = ir.OuterValue(n.X);
        if (r.Op() == ir.ONAME) {
            r = r._<ptr<ir.Name>>();
            if (ir.Orig(r) != r) {
                @base.Fatalf("found non-orig name node %v", r); // TODO(mdempsky): What does this mean?
            }
        }
        n.X = DefaultLit(n.X, null);
        if (n.X.Type() == null) {
            n.SetType(null);
            return n;
        }
        n.SetType(types.NewPtr(n.X.Type()));
    return n;

}

private static (ir.Node, ir.Node, ptr<types.Type>) tcShift(ir.Node n, ir.Node l, ir.Node r) {
    ir.Node _p0 = default;
    ir.Node _p0 = default;
    ptr<types.Type> _p0 = default!;

    if (l.Type() == null || r.Type() == null) {
        return (l, r, _addr_null!);
    }
    r = DefaultLit(r, types.Types[types.TUINT]);
    var t = r.Type();
    if (!t.IsInteger()) {
        @base.Errorf("invalid operation: %v (shift count type %v, must be integer)", n, r.Type());
        return (l, r, _addr_null!);
    }
    if (t.IsSigned() && !types.AllowsGoVersion(curpkg(), 1, 13)) {
        @base.ErrorfVers("go1.13", "invalid operation: %v (signed shift count type %v)", n, r.Type());
        return (l, r, _addr_null!);
    }
    t = l.Type();
    if (t != null && t.Kind() != types.TIDEAL && !t.IsInteger()) {
        @base.Errorf("invalid operation: %v (shift of type %v)", n, t);
        return (l, r, _addr_null!);
    }
    t = l.Type();
    if ((l.Type() == types.UntypedFloat || l.Type() == types.UntypedComplex) && r.Op() == ir.OLITERAL) {
        t = types.UntypedInt;
    }
    return (l, r, _addr_t!);

}

public static bool IsCmp(ir.Op op) {
    return iscmp[op];
}

// tcArith typechecks operands of a binary arithmetic expression.
// The result of tcArith MUST be assigned back to original operands,
// t is the type of the expression, and should be set by the caller. e.g:
//     n.X, n.Y, t = tcArith(n, op, n.X, n.Y)
//     n.SetType(t)
private static (ir.Node, ir.Node, ptr<types.Type>) tcArith(ir.Node n, ir.Op op, ir.Node l, ir.Node r) {
    ir.Node _p0 = default;
    ir.Node _p0 = default;
    ptr<types.Type> _p0 = default!;

    l, r = defaultlit2(l, r, false);
    if (l.Type() == null || r.Type() == null) {
        return (l, r, _addr_null!);
    }
    var t = l.Type();
    if (t.Kind() == types.TIDEAL) {
        t = r.Type();
    }
    var aop = ir.OXXX;
    if (iscmp[n.Op()] && t.Kind() != types.TIDEAL && !types.Identical(l.Type(), r.Type())) { 
        // comparison is okay as long as one side is
        // assignable to the other.  convert so they have
        // the same type.
        //
        // the only conversion that isn't a no-op is concrete == interface.
        // in that case, check comparability of the concrete type.
        // The conversion allocates, so only do it if the concrete type is huge.
        var converted = false;
        if (r.Type().Kind() != types.TBLANK) {
            aop, _ = Assignop(l.Type(), r.Type());
            if (aop != ir.OXXX) {
                if (r.Type().IsInterface() && !l.Type().IsInterface() && !types.IsComparable(l.Type())) {
                    @base.Errorf("invalid operation: %v (operator %v not defined on %s)", n, op, typekind(l.Type()));
                    return (l, r, _addr_null!);
                }
                types.CalcSize(l.Type());
                if (r.Type().IsInterface() == l.Type().IsInterface() || l.Type().Width >= 1 << 16) {
                    l = ir.NewConvExpr(@base.Pos, aop, r.Type(), l);
                    l.SetTypecheck(1);
                }
                t = r.Type();
                converted = true;
            }
        }
        if (!converted && l.Type().Kind() != types.TBLANK) {
            aop, _ = Assignop(r.Type(), l.Type());
            if (aop != ir.OXXX) {
                if (l.Type().IsInterface() && !r.Type().IsInterface() && !types.IsComparable(r.Type())) {
                    @base.Errorf("invalid operation: %v (operator %v not defined on %s)", n, op, typekind(r.Type()));
                    return (l, r, _addr_null!);
                }
                types.CalcSize(r.Type());
                if (r.Type().IsInterface() == l.Type().IsInterface() || r.Type().Width >= 1 << 16) {
                    r = ir.NewConvExpr(@base.Pos, aop, l.Type(), r);
                    r.SetTypecheck(1);
                }
                t = l.Type();
            }
        }
    }
    if (t.Kind() != types.TIDEAL && !types.Identical(l.Type(), r.Type())) {
        l, r = defaultlit2(l, r, true);
        if (l.Type() == null || r.Type() == null) {
            return (l, r, _addr_null!);
        }
        if (l.Type().IsInterface() == r.Type().IsInterface() || aop == 0) {
            @base.Errorf("invalid operation: %v (mismatched types %v and %v)", n, l.Type(), r.Type());
            return (l, r, _addr_null!);
        }
    }
    if (t.Kind() == types.TIDEAL) {
        t = mixUntyped(l.Type(), r.Type());
    }
    {
        var dt = defaultType(t);

        if (!okfor[op][dt.Kind()]) {
            @base.Errorf("invalid operation: %v (operator %v not defined on %s)", n, op, typekind(t));
            return (l, r, _addr_null!);
        }
    } 

    // okfor allows any array == array, map == map, func == func.
    // restrict to slice/map/func == nil and nil == slice/map/func.
    if (l.Type().IsArray() && !types.IsComparable(l.Type())) {
        @base.Errorf("invalid operation: %v (%v cannot be compared)", n, l.Type());
        return (l, r, _addr_null!);
    }
    if (l.Type().IsSlice() && !ir.IsNil(l) && !ir.IsNil(r)) {
        @base.Errorf("invalid operation: %v (slice can only be compared to nil)", n);
        return (l, r, _addr_null!);
    }
    if (l.Type().IsMap() && !ir.IsNil(l) && !ir.IsNil(r)) {
        @base.Errorf("invalid operation: %v (map can only be compared to nil)", n);
        return (l, r, _addr_null!);
    }
    if (l.Type().Kind() == types.TFUNC && !ir.IsNil(l) && !ir.IsNil(r)) {
        @base.Errorf("invalid operation: %v (func can only be compared to nil)", n);
        return (l, r, _addr_null!);
    }
    if (l.Type().IsStruct()) {
        {
            var f = types.IncomparableField(l.Type());

            if (f != null) {
                @base.Errorf("invalid operation: %v (struct containing %v cannot be compared)", n, f.Type);
                return (l, r, _addr_null!);
            }

        }

    }
    if ((op == ir.ODIV || op == ir.OMOD) && ir.IsConst(r, constant.Int)) {
        if (constant.Sign(r.Val()) == 0) {
            @base.Errorf("division by zero");
            return (l, r, _addr_null!);
        }
    }
    return (l, r, _addr_t!);

}

// The result of tcCompLit MUST be assigned back to n, e.g.
//     n.Left = tcCompLit(n.Left)
private static ir.Node tcCompLit(ptr<ir.CompLitExpr> _addr_n) => func((defer, _, _) => {
    ir.Node res = default;
    ref ir.CompLitExpr n = ref _addr_n.val;

    if (@base.EnableTrace && @base.Flag.LowerT) {
        defer(tracePrint("tcCompLit", n)(_addr_res));
    }
    var lno = @base.Pos;
    defer(() => {
        @base.Pos = lno;
    }());

    if (n.Ntype == null) {
        @base.ErrorfAt(n.Pos(), "missing type in composite literal");
        n.SetType(null);
        return n;
    }
    n.SetOrig(ir.Copy(n));

    ir.SetPos(n.Ntype); 

    // Need to handle [...]T arrays specially.
    {
        ptr<ir.ArrayType> (array, ok) = n.Ntype._<ptr<ir.ArrayType>>();

        if (ok && array.Elem != null && array.Len == null) {
            array.Elem = typecheckNtype(array.Elem);
            var elemType = array.Elem.Type();
            if (elemType == null) {
                n.SetType(null);
                return n;
            }
            var length = typecheckarraylit(elemType, -1, n.List, "array literal");
            n.SetOp(ir.OARRAYLIT);
            n.SetType(types.NewArray(elemType, length));
            n.Ntype = null;
            return n;
        }
    }


    n.Ntype = typecheckNtype(n.Ntype);
    var t = n.Ntype.Type();
    if (t == null) {
        n.SetType(null);
        return n;
    }
    n.SetType(t);


    if (t.Kind() == types.TARRAY) 
        typecheckarraylit(t.Elem(), t.NumElem(), n.List, "array literal");
        n.SetOp(ir.OARRAYLIT);
        n.Ntype = null;
    else if (t.Kind() == types.TSLICE) 
        length = typecheckarraylit(t.Elem(), -1, n.List, "slice literal");
        n.SetOp(ir.OSLICELIT);
        n.Ntype = null;
        n.Len = length;
    else if (t.Kind() == types.TMAP) 
        constSet cs = default;
        {
            var l__prev1 = l;

            foreach (var (__i3, __l) in n.List) {
                i3 = __i3;
                l = __l;
                ir.SetPos(l);
                if (l.Op() != ir.OKEY) {
                    n.List[i3] = Expr(l);
                    @base.Errorf("missing key in map literal");
                    continue;
                }
                ptr<ir.KeyExpr> l = l._<ptr<ir.KeyExpr>>();

                var r = l.Key;
                r = pushtype(r, t.Key());
                r = Expr(r);
                l.Key = AssignConv(r, t.Key(), "map key");
                cs.add(@base.Pos, l.Key, "key", "map literal");

                r = l.Value;
                r = pushtype(r, t.Elem());
                r = Expr(r);
                l.Value = AssignConv(r, t.Elem(), "map value");
            }

            l = l__prev1;
        }

        n.SetOp(ir.OMAPLIT);
        n.Ntype = null;
    else if (t.Kind() == types.TSTRUCT) 
        // Need valid field offsets for Xoffset below.
        types.CalcSize(t);

        var errored = false;
        if (len(n.List) != 0 && nokeys(n.List)) { 
            // simple list of variables
            var ls = n.List;
            {
                var i__prev1 = i;

                foreach (var (__i, __n1) in ls) {
                    i = __i;
                    n1 = __n1;
                    ir.SetPos(n1);
                    n1 = Expr(n1);
                    ls[i] = n1;
                    if (i >= t.NumFields()) {
                        if (!errored) {
                            @base.Errorf("too many values in %v", n);
                            errored = true;
                        }
                        continue;
                    }
                    ref var f = ref heap(t.Field(i), out ptr<var> _addr_f);
                    var s = f.Sym;
                    if (s != null && !types.IsExported(s.Name) && s.Pkg != types.LocalPkg) {
                        @base.Errorf("implicit assignment of unexported field '%s' in %v literal", s.Name, t);
                    } 
                    // No pushtype allowed here. Must name fields for that.
                    n1 = AssignConv(n1, f.Type, "field value");
                    var sk = ir.NewStructKeyExpr(@base.Pos, f.Sym, n1);
                    sk.Offset = f.Offset;
                    ls[i] = sk;

                }
        else

                i = i__prev1;
            }

            if (len(ls) < t.NumFields()) {
                @base.Errorf("too few values in %v", n);
            }

        } {
            var hash = make_map<@string, bool>(); 

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
                        s = key.Sym();
                        {
                            ptr<ir.Ident> (id, ok) = key._<ptr<ir.Ident>>();

                            if (ok && DotImportRefs[id] != null) {
                                s = Lookup(s.Name);
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
                        if (s == null || s.Pkg != types.LocalPkg || key.Op() == ir.OXDOT || s.IsBlank()) {
                            @base.Errorf("invalid field name %v in struct initializer", key);
                            continue;
                        }

                        l = ir.NewStructKeyExpr(l.Pos(), s, kv.Value);
                        ls[i] = l;

                    }

                    if (l.Op() != ir.OSTRUCTKEY) {
                        if (!errored) {
                            @base.Errorf("mixture of field:value and value initializers");
                            errored = true;
                        }
                        ls[i] = Expr(ls[i]);
                        continue;
                    }

                    l = l._<ptr<ir.StructKeyExpr>>();

                    f = Lookdot1(null, l.Field, t, t.Fields(), 0);
                    if (f == null) {
                        {
                            var ci = Lookdot1(null, l.Field, t, t.Fields(), 2);

                            if (ci != null) { // Case-insensitive lookup.
                                if (visible(ci.Sym)) {
                                    @base.Errorf("unknown field '%v' in struct literal of type %v (but does have %v)", l.Field, t, ci.Sym);
                                }
                                else if (nonexported(l.Field) && l.Field.Name == ci.Sym.Name) { // Ensure exactness before the suggestion.
                                    @base.Errorf("cannot refer to unexported field '%v' in struct literal of type %v", l.Field, t);

                                }
                                else
 {
                                    @base.Errorf("unknown field '%v' in struct literal of type %v", l.Field, t);
                                }

                                continue;

                            }

                        }

                        f = ;
                        var (p, _) = dotpath(l.Field, t, _addr_f, true);
                        if (p == null || f.IsMethod()) {
                            @base.Errorf("unknown field '%v' in struct literal of type %v", l.Field, t);
                            continue;
                        } 
                        // dotpath returns the parent embedded types in reverse order.
                        slice<@string> ep = default;
                        for (var ei = len(p) - 1; ei >= 0; ei--) {
                            ep = append(ep, p[ei].field.Sym.Name);
                        }

                        ep = append(ep, l.Field.Name);
                        @base.Errorf("cannot use promoted field %v in struct literal of type %v", strings.Join(ep, "."), t);
                        continue;

                    }

                    fielddup(f.Sym.Name, hash);
                    l.Offset = f.Offset; 

                    // No pushtype allowed here. Tried and rejected.
                    l.Value = Expr(l.Value);
                    l.Value = AssignConv(l.Value, f.Type, "field value");

                }

                i = i__prev1;
                l = l__prev1;
            }
        }
        n.SetOp(ir.OSTRUCTLIT);
        n.Ntype = null;
    else 
        @base.Errorf("invalid composite literal type %v", t);
        n.SetType(null);
        return n;

});

// tcConv typechecks an OCONV node.
private static ir.Node tcConv(ptr<ir.ConvExpr> _addr_n) {
    ref ir.ConvExpr n = ref _addr_n.val;

    types.CheckSize(n.Type()); // ensure width is calculated for backend
    n.X = Expr(n.X);
    n.X = convlit1(n.X, n.Type(), true, null);
    var t = n.X.Type();
    if (t == null || n.Type() == null) {
        n.SetType(null);
        return n;
    }
    var (op, why) = Convertop(n.X.Op() == ir.OLITERAL, t, n.Type());
    if (op == ir.OXXX) {
        if (!n.Diag() && !n.Type().Broke() && !n.X.Diag()) {
            @base.Errorf("cannot convert %L to type %v%s", n.X, n.Type(), why);
            n.SetDiag(true);
        }
        n.SetOp(ir.OCONV);
        n.SetType(null);
        return n;

    }
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
            return stringtoruneslit(n);
        }
        return n;

}

// tcDot typechecks an OXDOT or ODOT node.
private static ir.Node tcDot(ptr<ir.SelectorExpr> _addr_n, nint top) {
    ref ir.SelectorExpr n = ref _addr_n.val;

    if (n.Op() == ir.OXDOT) {
        n = AddImplicitDots(n);
        n.SetOp(ir.ODOT);
        if (n.X == null) {
            n.SetType(null);
            return n;
        }
    }
    n.X = typecheck(n.X, ctxExpr | ctxType);
    n.X = DefaultLit(n.X, null);

    var t = n.X.Type();
    if (t == null) {
        @base.UpdateErrorDot(ir.Line(n), fmt.Sprint(n.X), fmt.Sprint(n));
        n.SetType(null);
        return n;
    }
    if (n.X.Op() == ir.OTYPE) {
        return typecheckMethodExpr(n);
    }
    if (t.IsPtr() && !t.Elem().IsInterface()) {
        t = t.Elem();
        if (t == null) {
            n.SetType(null);
            return n;
        }
        n.SetOp(ir.ODOTPTR);
        types.CheckSize(t);

    }
    if (n.Sel.IsBlank()) {
        @base.Errorf("cannot refer to blank field or method");
        n.SetType(null);
        return n;
    }
    if (Lookdot(n, t, 0) == null) { 
        // Legitimate field or method lookup failed, try to explain the error

        if (t.IsEmptyInterface()) 
            @base.Errorf("%v undefined (type %v is interface with no methods)", n, n.X.Type());
        else if (t.IsPtr() && t.Elem().IsInterface()) 
            // Pointer to interface is almost always a mistake.
            @base.Errorf("%v undefined (type %v is pointer to interface, not interface)", n, n.X.Type());
        else if (Lookdot(n, t, 1) != null) 
            // Field or method matches by name, but it is not exported.
            @base.Errorf("%v undefined (cannot refer to unexported field or method %v)", n, n.Sel);
        else 
            {
                var mt = Lookdot(n, t, 2);

                if (mt != null && visible(mt.Sym)) { // Case-insensitive lookup.
                    @base.Errorf("%v undefined (type %v has no field or method %v, but does have %v)", n, n.X.Type(), n.Sel, mt.Sym);

                }
                else
 {
                    @base.Errorf("%v undefined (type %v has no field or method %v)", n, n.X.Type(), n.Sel);
                }

            }

                n.SetType(null);
        return n;

    }
    if ((n.Op() == ir.ODOTINTER || n.Op() == ir.ODOTMETH) && top & ctxCallee == 0) {
        n.SetOp(ir.OCALLPART);
        n.SetType(MethodValueWrapper(n).Type());
    }
    return n;

}

// tcDotType typechecks an ODOTTYPE node.
private static ir.Node tcDotType(ptr<ir.TypeAssertExpr> _addr_n) {
    ref ir.TypeAssertExpr n = ref _addr_n.val;

    n.X = Expr(n.X);
    n.X = DefaultLit(n.X, null);
    var l = n.X;
    var t = l.Type();
    if (t == null) {
        n.SetType(null);
        return n;
    }
    if (!t.IsInterface()) {
        @base.Errorf("invalid type assertion: %v (non-interface type %v on left)", n, t);
        n.SetType(null);
        return n;
    }
    if (n.Ntype != null) {
        n.Ntype = typecheckNtype(n.Ntype);
        n.SetType(n.Ntype.Type());
        n.Ntype = null;
        if (n.Type() == null) {
            return n;
        }
    }
    if (n.Type() != null && !n.Type().IsInterface()) {
        ptr<types.Field> missing;        ptr<types.Field> have;

        ref nint ptr = ref heap(out ptr<nint> _addr_ptr);
        if (!implements(n.Type(), t, _addr_missing, _addr_have, _addr_ptr)) {
            if (have != null && have.Sym == missing.Sym) {
                @base.Errorf("impossible type assertion:\n\t%v does not implement %v (wrong type for %v method)\n" + "\t\thave %v%S\n\t\twant %v%S", n.Type(), t, missing.Sym, have.Sym, have.Type, missing.Sym, missing.Type);
            }
            else if (ptr != 0) {
                @base.Errorf("impossible type assertion:\n\t%v does not implement %v (%v method has pointer receiver)", n.Type(), t, missing.Sym);
            }
            else if (have != null) {
                @base.Errorf("impossible type assertion:\n\t%v does not implement %v (missing %v method)\n" + "\t\thave %v%S\n\t\twant %v%S", n.Type(), t, missing.Sym, have.Sym, have.Type, missing.Sym, missing.Type);
            }
            else
 {
                @base.Errorf("impossible type assertion:\n\t%v does not implement %v (missing %v method)", n.Type(), t, missing.Sym);
            }

            n.SetType(null);
            return n;

        }
    }
    return n;

}

// tcITab typechecks an OITAB node.
private static ir.Node tcITab(ptr<ir.UnaryExpr> _addr_n) {
    ref ir.UnaryExpr n = ref _addr_n.val;

    n.X = Expr(n.X);
    var t = n.X.Type();
    if (t == null) {
        n.SetType(null);
        return n;
    }
    if (!t.IsInterface()) {
        @base.Fatalf("OITAB of %v", t);
    }
    n.SetType(types.NewPtr(types.Types[types.TUINTPTR]));
    return n;

}

// tcIndex typechecks an OINDEX node.
private static ir.Node tcIndex(ptr<ir.IndexExpr> _addr_n) {
    ref ir.IndexExpr n = ref _addr_n.val;

    n.X = Expr(n.X);
    n.X = DefaultLit(n.X, null);
    n.X = implicitstar(n.X);
    var l = n.X;
    n.Index = Expr(n.Index);
    var r = n.Index;
    var t = l.Type();
    if (t == null || r.Type() == null) {
        n.SetType(null);
        return n;
    }

    if (t.Kind() == types.TSTRING || t.Kind() == types.TARRAY || t.Kind() == types.TSLICE) 
        n.Index = indexlit(n.Index);
        if (t.IsString()) {
            n.SetType(types.ByteType);
        }
        else
 {
            n.SetType(t.Elem());
        }
        @string why = "string";
        if (t.IsArray()) {
            why = "array";
        }
        else if (t.IsSlice()) {
            why = "slice";
        }
        if (n.Index.Type() != null && !n.Index.Type().IsInteger()) {
            @base.Errorf("non-integer %s index %v", why, n.Index);
            return n;
        }
        if (!n.Bounded() && ir.IsConst(n.Index, constant.Int)) {
            var x = n.Index.Val();
            if (constant.Sign(x) < 0) {
                @base.Errorf("invalid %s index %v (index must be non-negative)", why, n.Index);
            }
            else if (t.IsArray() && constant.Compare(x, token.GEQ, constant.MakeInt64(t.NumElem()))) {
                @base.Errorf("invalid array index %v (out of bounds for %d-element array)", n.Index, t.NumElem());
            }
            else if (ir.IsConst(n.X, constant.String) && constant.Compare(x, token.GEQ, constant.MakeInt64(int64(len(ir.StringVal(n.X)))))) {
                @base.Errorf("invalid string index %v (out of bounds for %d-byte string)", n.Index, len(ir.StringVal(n.X)));
            }
            else if (ir.ConstOverflow(x, types.Types[types.TINT])) {
                @base.Errorf("invalid %s index %v (index too large)", why, n.Index);
            }

        }
    else if (t.Kind() == types.TMAP) 
        n.Index = AssignConv(n.Index, t.Key(), "map index");
        n.SetType(t.Elem());
        n.SetOp(ir.OINDEXMAP);
        n.Assigned = false;
    else 
        @base.Errorf("invalid operation: %v (type %v does not support indexing)", n, t);
        n.SetType(null);
        return n;
        return n;

}

// tcLenCap typechecks an OLEN or OCAP node.
private static ir.Node tcLenCap(ptr<ir.UnaryExpr> _addr_n) {
    ref ir.UnaryExpr n = ref _addr_n.val;

    n.X = Expr(n.X);
    n.X = DefaultLit(n.X, null);
    n.X = implicitstar(n.X);
    var l = n.X;
    var t = l.Type();
    if (t == null) {
        n.SetType(null);
        return n;
    }
    bool ok = default;
    if (n.Op() == ir.OLEN) {
        ok = okforlen[t.Kind()];
    }
    else
 {
        ok = okforcap[t.Kind()];
    }
    if (!ok) {
        @base.Errorf("invalid argument %L for %v", l, n.Op());
        n.SetType(null);
        return n;
    }
    n.SetType(types.Types[types.TINT]);
    return n;

}

// tcRecv typechecks an ORECV node.
private static ir.Node tcRecv(ptr<ir.UnaryExpr> _addr_n) {
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
        @base.Errorf("invalid operation: %v (receive from non-chan type %v)", n, t);
        n.SetType(null);
        return n;
    }
    if (!t.ChanDir().CanRecv()) {
        @base.Errorf("invalid operation: %v (receive from send-only type %v)", n, t);
        n.SetType(null);
        return n;
    }
    n.SetType(t.Elem());
    return n;

}

// tcSPtr typechecks an OSPTR node.
private static ir.Node tcSPtr(ptr<ir.UnaryExpr> _addr_n) {
    ref ir.UnaryExpr n = ref _addr_n.val;

    n.X = Expr(n.X);
    var t = n.X.Type();
    if (t == null) {
        n.SetType(null);
        return n;
    }
    if (!t.IsSlice() && !t.IsString()) {
        @base.Fatalf("OSPTR of %v", t);
    }
    if (t.IsString()) {
        n.SetType(types.NewPtr(types.Types[types.TUINT8]));
    }
    else
 {
        n.SetType(types.NewPtr(t.Elem()));
    }
    return n;

}

// tcSlice typechecks an OSLICE or OSLICE3 node.
private static ir.Node tcSlice(ptr<ir.SliceExpr> _addr_n) {
    ref ir.SliceExpr n = ref _addr_n.val;

    n.X = DefaultLit(Expr(n.X), null);
    n.Low = indexlit(Expr(n.Low));
    n.High = indexlit(Expr(n.High));
    n.Max = indexlit(Expr(n.Max));
    var hasmax = n.Op().IsSlice3();
    var l = n.X;
    if (l.Type() == null) {
        n.SetType(null);
        return n;
    }
    if (l.Type().IsArray()) {
        if (!ir.IsAddressable(n.X)) {
            @base.Errorf("invalid operation %v (slice of unaddressable value)", n);
            n.SetType(null);
            return n;
        }
        var addr = NodAddr(n.X);
        addr.SetImplicit(true);
        n.X = Expr(addr);
        l = n.X;

    }
    var t = l.Type();
    ptr<types.Type> tp;
    if (t.IsString()) {
        if (hasmax) {
            @base.Errorf("invalid operation %v (3-index slice of string)", n);
            n.SetType(null);
            return n;
        }
        n.SetType(t);
        n.SetOp(ir.OSLICESTR);

    }
    else if (t.IsPtr() && t.Elem().IsArray()) {
        tp = t.Elem();
        n.SetType(types.NewSlice(tp.Elem()));
        types.CalcSize(n.Type());
        if (hasmax) {
            n.SetOp(ir.OSLICE3ARR);
        }
        else
 {
            n.SetOp(ir.OSLICEARR);
        }
    }
    else if (t.IsSlice()) {
        n.SetType(t);
    }
    else
 {
        @base.Errorf("cannot slice %v (type %v)", l, t);
        n.SetType(null);
        return n;
    }
    if (n.Low != null && !checksliceindex(l, n.Low, tp)) {
        n.SetType(null);
        return n;
    }
    if (n.High != null && !checksliceindex(l, n.High, tp)) {
        n.SetType(null);
        return n;
    }
    if (n.Max != null && !checksliceindex(l, n.Max, tp)) {
        n.SetType(null);
        return n;
    }
    if (!checksliceconst(n.Low, n.High) || !checksliceconst(n.Low, n.Max) || !checksliceconst(n.High, n.Max)) {
        n.SetType(null);
        return n;
    }
    return n;

}

// tcSliceHeader typechecks an OSLICEHEADER node.
private static ir.Node tcSliceHeader(ptr<ir.SliceHeaderExpr> _addr_n) {
    ref ir.SliceHeaderExpr n = ref _addr_n.val;
 
    // Errors here are Fatalf instead of Errorf because only the compiler
    // can construct an OSLICEHEADER node.
    // Components used in OSLICEHEADER that are supplied by parsed source code
    // have already been typechecked in e.g. OMAKESLICE earlier.
    var t = n.Type();
    if (t == null) {
        @base.Fatalf("no type specified for OSLICEHEADER");
    }
    if (!t.IsSlice()) {
        @base.Fatalf("invalid type %v for OSLICEHEADER", n.Type());
    }
    if (n.Ptr == null || n.Ptr.Type() == null || !n.Ptr.Type().IsUnsafePtr()) {
        @base.Fatalf("need unsafe.Pointer for OSLICEHEADER");
    }
    n.Ptr = Expr(n.Ptr);
    n.Len = DefaultLit(Expr(n.Len), types.Types[types.TINT]);
    n.Cap = DefaultLit(Expr(n.Cap), types.Types[types.TINT]);

    if (ir.IsConst(n.Len, constant.Int) && ir.Int64Val(n.Len) < 0) {
        @base.Fatalf("len for OSLICEHEADER must be non-negative");
    }
    if (ir.IsConst(n.Cap, constant.Int) && ir.Int64Val(n.Cap) < 0) {
        @base.Fatalf("cap for OSLICEHEADER must be non-negative");
    }
    if (ir.IsConst(n.Len, constant.Int) && ir.IsConst(n.Cap, constant.Int) && constant.Compare(n.Len.Val(), token.GTR, n.Cap.Val())) {
        @base.Fatalf("len larger than cap for OSLICEHEADER");
    }
    return n;

}

// tcStar typechecks an ODEREF node, which may be an expression or a type.
private static ir.Node tcStar(ptr<ir.StarExpr> _addr_n, nint top) {
    ref ir.StarExpr n = ref _addr_n.val;

    n.X = typecheck(n.X, ctxExpr | ctxType);
    var l = n.X;
    var t = l.Type();
    if (t == null) {
        n.SetType(null);
        return n;
    }
    if (l.Op() == ir.OTYPE) {
        n.SetOTYPE(types.NewPtr(l.Type())); 
        // Ensure l.Type gets CalcSize'd for the backend. Issue 20174.
        types.CheckSize(l.Type());
        return n;

    }
    if (!t.IsPtr()) {
        if (top & (ctxExpr | ctxStmt) != 0) {
            @base.Errorf("invalid indirect of %L", n.X);
            n.SetType(null);
            return n;
        }
        @base.Errorf("%v is not a type", l);
        return n;

    }
    n.SetType(t.Elem());
    return n;

}

// tcUnaryArith typechecks a unary arithmetic expression.
private static ir.Node tcUnaryArith(ptr<ir.UnaryExpr> _addr_n) {
    ref ir.UnaryExpr n = ref _addr_n.val;

    n.X = Expr(n.X);
    var l = n.X;
    var t = l.Type();
    if (t == null) {
        n.SetType(null);
        return n;
    }
    if (!okfor[n.Op()][defaultType(t).Kind()]) {
        @base.Errorf("invalid operation: %v (operator %v not defined on %s)", n, n.Op(), typekind(t));
        n.SetType(null);
        return n;
    }
    n.SetType(t);
    return n;

}

} // end typecheck_package
