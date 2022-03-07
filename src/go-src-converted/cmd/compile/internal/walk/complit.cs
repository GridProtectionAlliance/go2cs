// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package walk -- go2cs converted at 2022 March 06 23:11:40 UTC
// import "cmd/compile/internal/walk" ==> using walk = go.cmd.compile.@internal.walk_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\walk\complit.go
using @base = go.cmd.compile.@internal.@base_package;
using ir = go.cmd.compile.@internal.ir_package;
using ssagen = go.cmd.compile.@internal.ssagen_package;
using staticdata = go.cmd.compile.@internal.staticdata_package;
using staticinit = go.cmd.compile.@internal.staticinit_package;
using typecheck = go.cmd.compile.@internal.typecheck_package;
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class walk_package {

    // walkCompLit walks a composite literal node:
    // OARRAYLIT, OSLICELIT, OMAPLIT, OSTRUCTLIT (all CompLitExpr), or OPTRLIT (AddrExpr).
private static ir.Node walkCompLit(ir.Node n, ptr<ir.Nodes> _addr_init) {
    ref ir.Nodes init = ref _addr_init.val;

    if (isStaticCompositeLiteral(n) && !ssagen.TypeOK(n.Type())) {
        ptr<ir.CompLitExpr> n = n._<ptr<ir.CompLitExpr>>(); // not OPTRLIT
        // n can be directly represented in the read-only data section.
        // Make direct reference to the static data. See issue 12841.
        var vstat = readonlystaticname(_addr_n.Type());
        fixedlit(inInitFunction, initKindStatic, n, vstat, _addr_init);
        return typecheck.Expr(vstat);

    }
    var var_ = typecheck.Temp(n.Type());
    anylit(n, var_, _addr_init);
    return var_;

}

// initContext is the context in which static data is populated.
// It is either in an init function or in any other function.
// Static data populated in an init function will be written either
// zero times (as a readonly, static data symbol) or
// one time (during init function execution).
// Either way, there is no opportunity for races or further modification,
// so the data can be written to a (possibly readonly) data symbol.
// Static data populated in any other function needs to be local to
// that function to allow multiple instances of that function
// to execute concurrently without clobbering each others' data.
private partial struct initContext { // : byte
}

private static readonly initContext inInitFunction = iota;
private static readonly var inNonInitFunction = 0;


private static @string String(this initContext c) {
    if (c == inInitFunction) {
        return "inInitFunction";
    }
    return "inNonInitFunction";

}

// readonlystaticname returns a name backed by a read-only static data symbol.
private static ptr<ir.Name> readonlystaticname(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    var n = staticinit.StaticName(t);
    n.MarkReadonly();
    n.Linksym().Set(obj.AttrContentAddressable, true);
    n.Linksym().Set(obj.AttrLocal, true);
    return _addr_n!;
}

private static bool isSimpleName(ir.Node nn) {
    if (nn.Op() != ir.ONAME || ir.IsBlank(nn)) {
        return false;
    }
    ptr<ir.Name> n = nn._<ptr<ir.Name>>();
    return n.OnStack();

}

private static void litas(ir.Node l, ir.Node r, ptr<ir.Nodes> _addr_init) {
    ref ir.Nodes init = ref _addr_init.val;

    appendWalkStmt(init, ir.NewAssignStmt(@base.Pos, l, r));
}

// initGenType is a bitmap indicating the types of generation that will occur for a static value.
private partial struct initGenType { // : byte
}

private static readonly initGenType initDynamic = 1 << (int)(iota); // contains some dynamic values, for which init code will be generated
private static readonly var initConst = 0; // contains some constant values, which may be written into data symbols

// getdyn calculates the initGenType for n.
// If top is false, getdyn is recursing.
private static initGenType getdyn(ir.Node n, bool top) {

    if (n.Op() == ir.OSLICELIT) 
        ptr<ir.CompLitExpr> n = n._<ptr<ir.CompLitExpr>>();
        if (!top) {
            return initDynamic;
        }
        if (n.Len / 4 > int64(len(n.List))) { 
            // <25% of entries have explicit values.
            // Very rough estimation, it takes 4 bytes of instructions
            // to initialize 1 byte of result. So don't use a static
            // initializer if the dynamic initialization code would be
            // smaller than the static value.
            // See issue 23780.
            return initDynamic;

        }
    else if (n.Op() == ir.OARRAYLIT || n.Op() == ir.OSTRUCTLIT)     else 
        if (ir.IsConstNode(n)) {
            return initConst;
        }
        return initDynamic;
        ptr<ir.CompLitExpr> lit = n._<ptr<ir.CompLitExpr>>();

    initGenType mode = default;
    foreach (var (_, n1) in lit.List) {

        if (n1.Op() == ir.OKEY) 
            n1 = n1._<ptr<ir.KeyExpr>>().Value;
        else if (n1.Op() == ir.OSTRUCTKEY) 
            n1 = n1._<ptr<ir.StructKeyExpr>>().Value;
                mode |= getdyn(n1, false);
        if (mode == initDynamic | initConst) {
            break;
        }
    }    return mode;

}

// isStaticCompositeLiteral reports whether n is a compile-time constant.
private static bool isStaticCompositeLiteral(ir.Node n) {

    if (n.Op() == ir.OSLICELIT) 
        return false;
    else if (n.Op() == ir.OARRAYLIT) 
        ptr<ir.CompLitExpr> n = n._<ptr<ir.CompLitExpr>>();
        {
            var r__prev1 = r;

            foreach (var (_, __r) in n.List) {
                r = __r;
                if (r.Op() == ir.OKEY) {
                    r = r._<ptr<ir.KeyExpr>>().Value;
                }
                if (!isStaticCompositeLiteral(r)) {
                    return false;
                }
            }

            r = r__prev1;
        }

        return true;
    else if (n.Op() == ir.OSTRUCTLIT) 
        n = n._<ptr<ir.CompLitExpr>>();
        {
            var r__prev1 = r;

            foreach (var (_, __r) in n.List) {
                r = __r;
                ptr<ir.StructKeyExpr> r = r._<ptr<ir.StructKeyExpr>>();
                if (!isStaticCompositeLiteral(r.Value)) {
                    return false;
                }
            }

            r = r__prev1;
        }

        return true;
    else if (n.Op() == ir.OLITERAL || n.Op() == ir.ONIL) 
        return true;
    else if (n.Op() == ir.OCONVIFACE) 
        // See staticassign's OCONVIFACE case for comments.
        n = n._<ptr<ir.ConvExpr>>();
        var val = ir.Node(n);
        while (val.Op() == ir.OCONVIFACE) {
            val = val._<ptr<ir.ConvExpr>>().X;
        }
        if (val.Type().IsInterface()) {
            return val.Op() == ir.ONIL;
        }
        if (types.IsDirectIface(val.Type()) && val.Op() == ir.ONIL) {
            return true;
        }
        return isStaticCompositeLiteral(val);
        return false;

}

// initKind is a kind of static initialization: static, dynamic, or local.
// Static initialization represents literals and
// literal components of composite literals.
// Dynamic initialization represents non-literals and
// non-literal components of composite literals.
// LocalCode initialization represents initialization
// that occurs purely in generated code local to the function of use.
// Initialization code is sometimes generated in passes,
// first static then dynamic.
private partial struct initKind { // : byte
}

private static readonly initKind initKindStatic = iota + 1;
private static readonly var initKindDynamic = 0;
private static readonly var initKindLocalCode = 1;


// fixedlit handles struct, array, and slice literals.
// TODO: expand documentation.
private static void fixedlit(initContext ctxt, initKind kind, ptr<ir.CompLitExpr> _addr_n, ir.Node var_, ptr<ir.Nodes> _addr_init) {
    ref ir.CompLitExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    var isBlank = var_ == ir.BlankNode;
    Func<ir.Node, (ir.Node, ir.Node)> splitnode = default;

    if (n.Op() == ir.OARRAYLIT || n.Op() == ir.OSLICELIT) 
        long k = default;
        splitnode = r => {
            if (r.Op() == ir.OKEY) {
                ptr<ir.KeyExpr> kv = r._<ptr<ir.KeyExpr>>();
                k = typecheck.IndexConst(kv.Key);
                if (k < 0) {
                    @base.Fatalf("fixedlit: invalid index %v", kv.Key);
                }
                r = kv.Value;
            }
            var a = ir.NewIndexExpr(@base.Pos, var_, ir.NewInt(k));
            k++;
            if (isBlank) {
                return (ir.BlankNode, r);
            }
            return (a, r);
        };
    else if (n.Op() == ir.OSTRUCTLIT) 
        splitnode = rn => {
            ptr<ir.StructKeyExpr> r = rn._<ptr<ir.StructKeyExpr>>();
            if (r.Field.IsBlank() || isBlank) {
                return (ir.BlankNode, r.Value);
            }
            ir.SetPos(r);
            return (ir.NewSelectorExpr(@base.Pos, ir.ODOT, var_, r.Field), r.Value);
        };
    else 
        @base.Fatalf("fixedlit bad op: %v", n.Op());
        {
        ptr<ir.StructKeyExpr> r__prev1 = r;

        foreach (var (_, __r) in n.List) {
            r = __r;
            var (a, value) = splitnode(r);
            if (a == ir.BlankNode && !staticinit.AnySideEffects(value)) { 
                // Discard.
                continue;

            }


            if (value.Op() == ir.OSLICELIT) 
                ptr<ir.CompLitExpr> value = value._<ptr<ir.CompLitExpr>>();
                if ((kind == initKindStatic && ctxt == inNonInitFunction) || (kind == initKindDynamic && ctxt == inInitFunction)) {
                    slicelit(ctxt, value, a, _addr_init);
                    continue;
                }
            else if (value.Op() == ir.OARRAYLIT || value.Op() == ir.OSTRUCTLIT) 
                value = value._<ptr<ir.CompLitExpr>>();
                fixedlit(ctxt, kind, value, a, _addr_init);
                continue;
                        var islit = ir.IsConstNode(value);
            if ((kind == initKindStatic && !islit) || (kind == initKindDynamic && islit)) {
                continue;
            } 

            // build list of assignments: var[index] = expr
            ir.SetPos(a);
            var @as = ir.NewAssignStmt(@base.Pos, a, value);
            as = typecheck.Stmt(as)._<ptr<ir.AssignStmt>>();

            if (kind == initKindStatic) 
                genAsStatic(_addr_as);
            else if (kind == initKindDynamic || kind == initKindLocalCode) 
                a = orderStmtInPlace(as, /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, slice<ptr<ir.Name>>>{});
                a = walkStmt(a);
                init.Append(a);
            else 
                @base.Fatalf("fixedlit: bad kind %d", kind);
            
        }
        r = r__prev1;
    }
}

private static bool isSmallSliceLit(ptr<ir.CompLitExpr> _addr_n) {
    ref ir.CompLitExpr n = ref _addr_n.val;

    if (n.Op() != ir.OSLICELIT) {
        return false;
    }
    return n.Type().Elem().Width == 0 || n.Len <= ir.MaxSmallArraySize / n.Type().Elem().Width;

}

private static void slicelit(initContext ctxt, ptr<ir.CompLitExpr> _addr_n, ir.Node var_, ptr<ir.Nodes> _addr_init) => func((_, panic, _) => {
    ref ir.CompLitExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;
 
    // make an array type corresponding the number of elements we have
    var t = types.NewArray(n.Type().Elem(), n.Len);
    types.CalcSize(t);

    if (ctxt == inNonInitFunction) { 
        // put everything into static array
        var vstat = staticinit.StaticName(t);

        fixedlit(ctxt, initKindStatic, _addr_n, vstat, _addr_init);
        fixedlit(ctxt, initKindDynamic, _addr_n, vstat, _addr_init); 

        // copy static to slice
        var_ = typecheck.AssignExpr(var_);
        var (name, offset, ok) = staticinit.StaticLoc(var_);
        if (!ok || name.Class != ir.PEXTERN) {
            @base.Fatalf("slicelit: %v", var_);
        }
        staticdata.InitSlice(name, offset, vstat.Linksym(), t.NumElem());
        return ;

    }
    vstat = default;

    var mode = getdyn(n, true);
    if (mode & initConst != 0 && !isSmallSliceLit(_addr_n)) {
        if (ctxt == inInitFunction) {
            vstat = readonlystaticname(_addr_t);
        }
        else
 {
            vstat = staticinit.StaticName(t);
        }
        fixedlit(ctxt, initKindStatic, _addr_n, vstat, _addr_init);

    }
    var vauto = typecheck.Temp(types.NewPtr(t)); 

    // set auto to point at new temp or heap (3 assign)
    ir.Node a = default;
    {
        var x = n.Prealloc;

        if (x != null) { 
            // temp allocated during order.go for dddarg
            if (!types.Identical(t, x.Type())) {
                panic("dotdotdot base type does not match order's assigned type");
            }

            a = initStackTemp(init, x, vstat);

        }
        else if (n.Esc() == ir.EscNone) {
            a = initStackTemp(init, typecheck.Temp(t), vstat);
        }
        else
 {
            a = ir.NewUnaryExpr(@base.Pos, ir.ONEW, ir.TypeNode(t));
        }

    }

    appendWalkStmt(init, ir.NewAssignStmt(@base.Pos, vauto, a));

    if (vstat != null && n.Prealloc == null && n.Esc() != ir.EscNone) { 
        // If we allocated on the heap with ONEW, copy the static to the
        // heap (4). We skip this for stack temporaries, because
        // initStackTemp already handled the copy.
        a = ir.NewStarExpr(@base.Pos, vauto);
        appendWalkStmt(init, ir.NewAssignStmt(@base.Pos, a, vstat));

    }
    long index = default;
    {
        var value__prev1 = value;

        foreach (var (_, __value) in n.List) {
            value = __value;
            if (value.Op() == ir.OKEY) {
                ptr<ir.KeyExpr> kv = value._<ptr<ir.KeyExpr>>();
                index = typecheck.IndexConst(kv.Key);
                if (index < 0) {
                    @base.Fatalf("slicelit: invalid index %v", kv.Key);
                }
                value = kv.Value;
            }
            a = ir.NewIndexExpr(@base.Pos, vauto, ir.NewInt(index));
            a.SetBounded(true);
            index++; 

            // TODO need to check bounds?


            if (value.Op() == ir.OSLICELIT) 
                break;
            else if (value.Op() == ir.OARRAYLIT || value.Op() == ir.OSTRUCTLIT) 
                ptr<ir.CompLitExpr> value = value._<ptr<ir.CompLitExpr>>();
                var k = initKindDynamic;
                if (vstat == null) { 
                    // Generate both static and dynamic initializations.
                    // See issue #31987.
                    k = initKindLocalCode;

                }

                fixedlit(ctxt, k, _addr_value, a, _addr_init);
                continue;
                        if (vstat != null && ir.IsConstNode(value)) { // already set by copy from static value
                continue;

            } 

            // build list of vauto[c] = expr
            ir.SetPos(value);
            var @as = typecheck.Stmt(ir.NewAssignStmt(@base.Pos, a, value));
            as = orderStmtInPlace(as, /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, slice<ptr<ir.Name>>>{});
            as = walkStmt(as);
            init.Append(as);

        }
        value = value__prev1;
    }

    a = ir.NewAssignStmt(@base.Pos, var_, ir.NewSliceExpr(@base.Pos, ir.OSLICE, vauto, null, null, null));

    a = typecheck.Stmt(a);
    a = orderStmtInPlace(a, /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, slice<ptr<ir.Name>>>{});
    a = walkStmt(a);
    init.Append(a);

});

private static void maplit(ptr<ir.CompLitExpr> _addr_n, ir.Node m, ptr<ir.Nodes> _addr_init) {
    ref ir.CompLitExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;
 
    // make the map var
    var a = ir.NewCallExpr(@base.Pos, ir.OMAKE, null, null);
    a.SetEsc(n.Esc());
    a.Args = new slice<ir.Node>(new ir.Node[] { ir.TypeNode(n.Type()), ir.NewInt(int64(len(n.List))) });
    litas(m, a, _addr_init);

    var entries = n.List; 

    // The order pass already removed any dynamic (runtime-computed) entries.
    // All remaining entries are static. Double-check that.
    {
        var r__prev1 = r;

        foreach (var (_, __r) in entries) {
            r = __r;
            ptr<ir.KeyExpr> r = r._<ptr<ir.KeyExpr>>();
            if (!isStaticCompositeLiteral(r.Key) || !isStaticCompositeLiteral(r.Value)) {
                @base.Fatalf("maplit: entry is not a literal: %v", r);
            }
        }
        r = r__prev1;
    }

    if (len(entries) > 25) { 
        // For a large number of entries, put them in an array and loop.

        // build types [count]Tindex and [count]Tvalue
        var tk = types.NewArray(n.Type().Key(), int64(len(entries)));
        var te = types.NewArray(n.Type().Elem(), int64(len(entries)));

        tk.SetNoalg(true);
        te.SetNoalg(true);

        types.CalcSize(tk);
        types.CalcSize(te); 

        // make and initialize static arrays
        var vstatk = readonlystaticname(_addr_tk);
        var vstate = readonlystaticname(_addr_te);

        var datak = ir.NewCompLitExpr(@base.Pos, ir.OARRAYLIT, null, null);
        var datae = ir.NewCompLitExpr(@base.Pos, ir.OARRAYLIT, null, null);
        {
            var r__prev1 = r;

            foreach (var (_, __r) in entries) {
                r = __r;
                r = r._<ptr<ir.KeyExpr>>();
                datak.List.Append(r.Key);
                datae.List.Append(r.Value);
            }

            r = r__prev1;
        }

        fixedlit(inInitFunction, initKindStatic, _addr_datak, vstatk, _addr_init);
        fixedlit(inInitFunction, initKindStatic, _addr_datae, vstate, _addr_init); 

        // loop adding structure elements to map
        // for i = 0; i < len(vstatk); i++ {
        //    map[vstatk[i]] = vstate[i]
        // }
        var i = typecheck.Temp(types.Types[types.TINT]);
        var rhs = ir.NewIndexExpr(@base.Pos, vstate, i);
        rhs.SetBounded(true);

        var kidx = ir.NewIndexExpr(@base.Pos, vstatk, i);
        kidx.SetBounded(true);
        var lhs = ir.NewIndexExpr(@base.Pos, m, kidx);

        var zero = ir.NewAssignStmt(@base.Pos, i, ir.NewInt(0));
        var cond = ir.NewBinaryExpr(@base.Pos, ir.OLT, i, ir.NewInt(tk.NumElem()));
        var incr = ir.NewAssignStmt(@base.Pos, i, ir.NewBinaryExpr(@base.Pos, ir.OADD, i, ir.NewInt(1)));

        ir.Node body = ir.NewAssignStmt(@base.Pos, lhs, rhs);
        body = typecheck.Stmt(body); // typechecker rewrites OINDEX to OINDEXMAP
        body = orderStmtInPlace(body, /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, slice<ptr<ir.Name>>>{});

        var loop = ir.NewForStmt(@base.Pos, null, cond, incr, null);
        loop.Body = new slice<ir.Node>(new ir.Node[] { body });
        loop.PtrInit().val = new slice<ir.Node>(new ir.Node[] { zero });

        appendWalkStmt(init, loop);
        return ;

    }
    var tmpkey = typecheck.Temp(m.Type().Key());
    var tmpelem = typecheck.Temp(m.Type().Elem());

    {
        var r__prev1 = r;

        foreach (var (_, __r) in entries) {
            r = __r;
            r = r._<ptr<ir.KeyExpr>>();
            var index = r.Key;
            var elem = r.Value;

            ir.SetPos(index);
            appendWalkStmt(init, ir.NewAssignStmt(@base.Pos, tmpkey, index));

            ir.SetPos(elem);
            appendWalkStmt(init, ir.NewAssignStmt(@base.Pos, tmpelem, elem));

            ir.SetPos(tmpelem);
            a = ir.NewAssignStmt(@base.Pos, ir.NewIndexExpr(@base.Pos, m, tmpkey), tmpelem);
            a = typecheck.Stmt(a); // typechecker rewrites OINDEX to OINDEXMAP
            a = orderStmtInPlace(a, /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, slice<ptr<ir.Name>>>{});
            appendWalkStmt(init, a);

        }
        r = r__prev1;
    }

    appendWalkStmt(init, ir.NewUnaryExpr(@base.Pos, ir.OVARKILL, tmpkey));
    appendWalkStmt(init, ir.NewUnaryExpr(@base.Pos, ir.OVARKILL, tmpelem));

}

private static void anylit(ir.Node n, ir.Node var_, ptr<ir.Nodes> _addr_init) {
    ref ir.Nodes init = ref _addr_init.val;

    var t = n.Type();

    if (n.Op() == ir.ONAME) 
        ptr<ir.Name> n = n._<ptr<ir.Name>>();
        appendWalkStmt(init, ir.NewAssignStmt(@base.Pos, var_, n));
    else if (n.Op() == ir.OMETHEXPR) 
        n = n._<ptr<ir.SelectorExpr>>();
        anylit(n.FuncName(), var_, _addr_init);
    else if (n.Op() == ir.OPTRLIT) 
        n = n._<ptr<ir.AddrExpr>>();
        if (!t.IsPtr()) {
            @base.Fatalf("anylit: not ptr");
        }
        ir.Node r = default;
        if (n.Prealloc != null) { 
            // n.Prealloc is stack temporary used as backing store.
            r = initStackTemp(init, n.Prealloc, null);

        }
        else
 {
            r = ir.NewUnaryExpr(@base.Pos, ir.ONEW, ir.TypeNode(n.X.Type()));
            r.SetEsc(n.Esc());
        }
        appendWalkStmt(init, ir.NewAssignStmt(@base.Pos, var_, r));

        var_ = ir.NewStarExpr(@base.Pos, var_);
        var_ = typecheck.AssignExpr(var_);
        anylit(n.X, var_, _addr_init);
    else if (n.Op() == ir.OSTRUCTLIT || n.Op() == ir.OARRAYLIT) 
        n = n._<ptr<ir.CompLitExpr>>();
        if (!t.IsStruct() && !t.IsArray()) {
            @base.Fatalf("anylit: not struct/array");
        }
        if (isSimpleName(var_) && len(n.List) > 4) { 
            // lay out static data
            var vstat = readonlystaticname(_addr_t);

            var ctxt = inInitFunction;
            if (n.Op() == ir.OARRAYLIT) {
                ctxt = inNonInitFunction;
            }

            fixedlit(ctxt, initKindStatic, n, vstat, _addr_init); 

            // copy static to var
            appendWalkStmt(init, ir.NewAssignStmt(@base.Pos, var_, vstat)); 

            // add expressions to automatic
            fixedlit(inInitFunction, initKindDynamic, n, var_, _addr_init);
            break;

        }
        long components = default;
        if (n.Op() == ir.OARRAYLIT) {
            components = t.NumElem();
        }
        else
 {
            components = int64(t.NumFields());
        }
        if (isSimpleName(var_) || int64(len(n.List)) < components) {
            appendWalkStmt(init, ir.NewAssignStmt(@base.Pos, var_, null));
        }
        fixedlit(inInitFunction, initKindLocalCode, n, var_, _addr_init);
    else if (n.Op() == ir.OSLICELIT) 
        n = n._<ptr<ir.CompLitExpr>>();
        slicelit(inInitFunction, n, var_, _addr_init);
    else if (n.Op() == ir.OMAPLIT) 
        n = n._<ptr<ir.CompLitExpr>>();
        if (!t.IsMap()) {
            @base.Fatalf("anylit: not map");
        }
        maplit(n, var_, _addr_init);
    else 
        @base.Fatalf("anylit: not lit, op=%v node=%v", n.Op(), n);
    
}

// oaslit handles special composite literal assignments.
// It returns true if n's effects have been added to init,
// in which case n should be dropped from the program by the caller.
private static bool oaslit(ptr<ir.AssignStmt> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.AssignStmt n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    if (n.X == null || n.Y == null) { 
        // not a special composite literal assignment
        return false;

    }
    if (n.X.Type() == null || n.Y.Type() == null) { 
        // not a special composite literal assignment
        return false;

    }
    if (!isSimpleName(n.X)) { 
        // not a special composite literal assignment
        return false;

    }
    ptr<ir.Name> x = n.X._<ptr<ir.Name>>();
    if (!types.Identical(n.X.Type(), n.Y.Type())) { 
        // not a special composite literal assignment
        return false;

    }

    if (n.Y.Op() == ir.OSTRUCTLIT || n.Y.Op() == ir.OARRAYLIT || n.Y.Op() == ir.OSLICELIT || n.Y.Op() == ir.OMAPLIT) 
        if (ir.Any(n.Y, y => ir.Uses(y, x))) { 
            // not a special composite literal assignment
            return false;

        }
        anylit(n.Y, n.X, _addr_init);
    else 
        // not a special composite literal assignment
        return false;
        return true;

}

private static void genAsStatic(ptr<ir.AssignStmt> _addr_@as) {
    ref ir.AssignStmt @as = ref _addr_@as.val;

    if (@as.X.Type() == null) {
        @base.Fatalf("genAsStatic as.Left not typechecked");
    }
    var (name, offset, ok) = staticinit.StaticLoc(@as.X);
    if (!ok || (name.Class != ir.PEXTERN && @as.X != ir.BlankNode)) {
        @base.Fatalf("genAsStatic: lhs %v", @as.X);
    }
    {
        var r__prev1 = r;

        var r = @as.Y;


        if (r.Op() == ir.OLITERAL) 
            staticdata.InitConst(name, offset, r, int(r.Type().Width));
            return ;
        else if (r.Op() == ir.OMETHEXPR) 
            r = r._<ptr<ir.SelectorExpr>>();
            staticdata.InitAddr(name, offset, staticdata.FuncLinksym(r.FuncName()));
            return ;
        else if (r.Op() == ir.ONAME) 
            r = r._<ptr<ir.Name>>();
            if (r.Offset_ != 0) {
                @base.Fatalf("genAsStatic %+v", as);
            }
            if (r.Class == ir.PFUNC) {
                staticdata.InitAddr(name, offset, staticdata.FuncLinksym(r));
                return ;
            }


        r = r__prev1;
    }
    @base.Fatalf("genAsStatic: rhs %v", @as.Y);

}

} // end walk_package
