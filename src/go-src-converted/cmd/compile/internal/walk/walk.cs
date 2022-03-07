// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package walk -- go2cs converted at 2022 March 06 23:12:09 UTC
// import "cmd/compile/internal/walk" ==> using walk = go.cmd.compile.@internal.walk_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\walk\walk.go
using errors = go.errors_package;
using fmt = go.fmt_package;

using @base = go.cmd.compile.@internal.@base_package;
using ir = go.cmd.compile.@internal.ir_package;
using reflectdata = go.cmd.compile.@internal.reflectdata_package;
using ssagen = go.cmd.compile.@internal.ssagen_package;
using typecheck = go.cmd.compile.@internal.typecheck_package;
using types = go.cmd.compile.@internal.types_package;
using src = go.cmd.@internal.src_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class walk_package {

    // The constant is known to runtime.
private static readonly nint tmpstringbufsize = 32;

private static readonly nint zeroValSize = 1024; // must match value of runtime/map.go:maxZero

 // must match value of runtime/map.go:maxZero

public static void Walk(ptr<ir.Func> _addr_fn) {
    ref ir.Func fn = ref _addr_fn.val;

    ir.CurFunc = fn;
    var errorsBefore = @base.Errors();
    order(fn);
    if (@base.Errors() > errorsBefore) {
        return ;
    }
    if (@base.Flag.W != 0) {
        var s = fmt.Sprintf("\nbefore walk %v", ir.CurFunc.Sym());
        ir.DumpList(s, ir.CurFunc.Body);
    }
    var lno = @base.Pos;

    @base.Pos = lno;
    if (@base.Errors() > errorsBefore) {
        return ;
    }
    walkStmtList(ir.CurFunc.Body);
    if (@base.Flag.W != 0) {
        s = fmt.Sprintf("after walk %v", ir.CurFunc.Sym());
        ir.DumpList(s, ir.CurFunc.Body);
    }
    if (@base.Flag.Cfg.Instrumenting) {
        instrument(fn);
    }
    foreach (var (_, n) in fn.Dcl) {
        types.CalcSize(n.Type());
    }
}

// walkRecv walks an ORECV node.
private static ir.Node walkRecv(ptr<ir.UnaryExpr> _addr_n) {
    ref ir.UnaryExpr n = ref _addr_n.val;

    if (n.Typecheck() == 0) {
        @base.Fatalf("missing typecheck: %+v", n);
    }
    ref var init = ref heap(ir.TakeInit(n), out ptr<var> _addr_init);

    n.X = walkExpr(n.X, _addr_init);
    var call = walkExpr(mkcall1(chanfn("chanrecv1", 2, _addr_n.X.Type()), _addr_null, _addr_init, n.X, typecheck.NodNil()), _addr_init);
    return ir.InitExpr(init, call);

}

private static ptr<ir.AssignStmt> convas(ptr<ir.AssignStmt> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.AssignStmt n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    if (n.Op() != ir.OAS) {
        @base.Fatalf("convas: not OAS %v", n.Op());
    }
    n.SetTypecheck(1);

    if (n.X == null || n.Y == null) {
        return _addr_n!;
    }
    var lt = n.X.Type();
    var rt = n.Y.Type();
    if (lt == null || rt == null) {
        return _addr_n!;
    }
    if (ir.IsBlank(n.X)) {
        n.Y = typecheck.DefaultLit(n.Y, null);
        return _addr_n!;
    }
    if (!types.Identical(lt, rt)) {
        n.Y = typecheck.AssignConv(n.Y, lt, "assignment");
        n.Y = walkExpr(n.Y, init);
    }
    types.CalcSize(n.Y.Type());

    return _addr_n!;

}

private static var stop = errors.New("stop");

private static ptr<ir.CallExpr> vmkcall(ir.Node fn, ptr<types.Type> _addr_t, ptr<ir.Nodes> _addr_init, slice<ir.Node> va) {
    ref types.Type t = ref _addr_t.val;
    ref ir.Nodes init = ref _addr_init.val;

    if (init == null) {
        @base.Fatalf("mkcall with nil init: %v", fn);
    }
    if (fn.Type() == null || fn.Type().Kind() != types.TFUNC) {
        @base.Fatalf("mkcall %v %v", fn, fn.Type());
    }
    var n = fn.Type().NumParams();
    if (n != len(va)) {
        @base.Fatalf("vmkcall %v needs %v args got %v", fn, n, len(va));
    }
    var call = ir.NewCallExpr(@base.Pos, ir.OCALL, fn, va);
    typecheck.Call(call);
    call.SetType(t);
    return walkExpr(call, init)._<ptr<ir.CallExpr>>();

}

private static ptr<ir.CallExpr> mkcall(@string name, ptr<types.Type> _addr_t, ptr<ir.Nodes> _addr_init, params ir.Node[] args) {
    args = args.Clone();
    ref types.Type t = ref _addr_t.val;
    ref ir.Nodes init = ref _addr_init.val;

    return _addr_vmkcall(typecheck.LookupRuntime(name), _addr_t, _addr_init, args)!;
}

private static ir.Node mkcallstmt(@string name, params ir.Node[] args) {
    args = args.Clone();

    return mkcallstmt1(typecheck.LookupRuntime(name), args);
}

private static ptr<ir.CallExpr> mkcall1(ir.Node fn, ptr<types.Type> _addr_t, ptr<ir.Nodes> _addr_init, params ir.Node[] args) {
    args = args.Clone();
    ref types.Type t = ref _addr_t.val;
    ref ir.Nodes init = ref _addr_init.val;

    return _addr_vmkcall(fn, _addr_t, _addr_init, args)!;
}

private static ir.Node mkcallstmt1(ir.Node fn, params ir.Node[] args) {
    args = args.Clone();

    ref ir.Nodes init = ref heap(out ptr<ir.Nodes> _addr_init);
    var n = vmkcall(fn, _addr_null, _addr_init, args);
    if (len(init) == 0) {
        return n;
    }
    init.Append(n);
    return ir.NewBlockStmt(n.Pos(), init);

}

private static ir.Node chanfn(@string name, nint n, ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    if (!t.IsChan()) {
        @base.Fatalf("chanfn %v", t);
    }
    var fn = typecheck.LookupRuntime(name);
    switch (n) {
        case 1: 
            fn = typecheck.SubstArgTypes(fn, t.Elem());
            break;
        case 2: 
            fn = typecheck.SubstArgTypes(fn, t.Elem(), t.Elem());
            break;
        default: 
            @base.Fatalf("chanfn %d", n);
            break;
    }
    return fn;

}

private static ir.Node mapfn(@string name, ptr<types.Type> _addr_t, bool isfat) {
    ref types.Type t = ref _addr_t.val;

    if (!t.IsMap()) {
        @base.Fatalf("mapfn %v", t);
    }
    var fn = typecheck.LookupRuntime(name);
    if (mapfast(_addr_t) == mapslow || isfat) {
        fn = typecheck.SubstArgTypes(fn, t.Key(), t.Elem(), t.Key(), t.Elem());
    }
    else
 {
        fn = typecheck.SubstArgTypes(fn, t.Key(), t.Elem(), t.Elem());
    }
    return fn;

}

private static ir.Node mapfndel(@string name, ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    if (!t.IsMap()) {
        @base.Fatalf("mapfn %v", t);
    }
    var fn = typecheck.LookupRuntime(name);
    if (mapfast(_addr_t) == mapslow) {
        fn = typecheck.SubstArgTypes(fn, t.Key(), t.Elem(), t.Key());
    }
    else
 {
        fn = typecheck.SubstArgTypes(fn, t.Key(), t.Elem());
    }
    return fn;

}

private static readonly var mapslow = iota;
private static readonly var mapfast32 = 0;
private static readonly var mapfast32ptr = 1;
private static readonly var mapfast64 = 2;
private static readonly var mapfast64ptr = 3;
private static readonly var mapfaststr = 4;
private static readonly var nmapfast = 5;


private partial struct mapnames { // : array<@string>
}

private static mapnames mkmapnames(@string @base, @string ptr) {
    return new mapnames(base,base+"_fast32",base+"_fast32"+ptr,base+"_fast64",base+"_fast64"+ptr,base+"_faststr");
}

private static var mapaccess1 = mkmapnames("mapaccess1", "");
private static var mapaccess2 = mkmapnames("mapaccess2", "");
private static var mapassign = mkmapnames("mapassign", "ptr");
private static var mapdelete = mkmapnames("mapdelete", "");

private static nint mapfast(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;
 
    // Check runtime/map.go:maxElemSize before changing.
    if (t.Elem().Width > 128) {
        return mapslow;
    }

    if (reflectdata.AlgType(t.Key()) == types.AMEM32) 
        if (!t.Key().HasPointers()) {
            return mapfast32;
        }
        if (types.PtrSize == 4) {
            return mapfast32ptr;
        }
        @base.Fatalf("small pointer %v", t.Key());
    else if (reflectdata.AlgType(t.Key()) == types.AMEM64) 
        if (!t.Key().HasPointers()) {
            return mapfast64;
        }
        if (types.PtrSize == 8) {
            return mapfast64ptr;
        }
    else if (reflectdata.AlgType(t.Key()) == types.ASTRING) 
        return mapfaststr;
        return mapslow;

}

private static void walkAppendArgs(ptr<ir.CallExpr> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.CallExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    walkExprListSafe(n.Args, init); 

    // walkExprListSafe will leave OINDEX (s[n]) alone if both s
    // and n are name or literal, but those may index the slice we're
    // modifying here. Fix explicitly.
    var ls = n.Args;
    foreach (var (i1, n1) in ls) {
        ls[i1] = cheapExpr(n1, init);
    }
}

// appendWalkStmt typechecks and walks stmt and then appends it to init.
private static void appendWalkStmt(ptr<ir.Nodes> _addr_init, ir.Node stmt) {
    ref ir.Nodes init = ref _addr_init.val;

    var op = stmt.Op();
    var n = typecheck.Stmt(stmt);
    if (op == ir.OAS || op == ir.OAS2) { 
        // If the assignment has side effects, walkExpr will append them
        // directly to init for us, while walkStmt will wrap it in an OBLOCK.
        // We need to append them directly.
        // TODO(rsc): Clean this up.
        n = walkExpr(n, init);

    }
    else
 {
        n = walkStmt(n);
    }
    init.Append(n);

}

// The max number of defers in a function using open-coded defers. We enforce this
// limit because the deferBits bitmask is currently a single byte (to minimize code size)
private static readonly nint maxOpenDefers = 8;

// backingArrayPtrLen extracts the pointer and length from a slice or string.
// This constructs two nodes referring to n, so n must be a cheapExpr.


// backingArrayPtrLen extracts the pointer and length from a slice or string.
// This constructs two nodes referring to n, so n must be a cheapExpr.
private static (ir.Node, ir.Node) backingArrayPtrLen(ir.Node n) {
    ir.Node ptr = default;
    ir.Node length = default;

    ref ir.Nodes init = ref heap(out ptr<ir.Nodes> _addr_init);
    var c = cheapExpr(n, _addr_init);
    if (c != n || len(init) != 0) {
        @base.Fatalf("backingArrayPtrLen not cheap: %v", n);
    }
    ptr = ir.NewUnaryExpr(@base.Pos, ir.OSPTR, n);
    if (n.Type().IsString()) {
        ptr.SetType(types.Types[types.TUINT8].PtrTo());
    }
    else
 {
        ptr.SetType(n.Type().Elem().PtrTo());
    }
    length = ir.NewUnaryExpr(@base.Pos, ir.OLEN, n);
    length.SetType(types.Types[types.TINT]);
    return (ptr, length);

}

// mayCall reports whether evaluating expression n may require
// function calls, which could clobber function call arguments/results
// currently on the stack.
private static bool mayCall(ir.Node n) { 
    // When instrumenting, any expression might require function calls.
    if (@base.Flag.Cfg.Instrumenting) {
        return true;
    }
    Func<ptr<types.Type>, bool> isSoftFloat = typ => {
        return types.IsFloat[typ.Kind()] || types.IsComplex[typ.Kind()];
    };

    return ir.Any(n, n => { 
        // walk should have already moved any Init blocks off of
        // expressions.
        if (len(n.Init()) != 0) {
            @base.FatalfAt(n.Pos(), "mayCall %+v", n);
        }

        if (n.Op() == ir.OCALLFUNC || n.Op() == ir.OCALLMETH || n.Op() == ir.OCALLINTER || n.Op() == ir.OUNSAFEADD || n.Op() == ir.OUNSAFESLICE) 
            return true;
        else if (n.Op() == ir.OINDEX || n.Op() == ir.OSLICE || n.Op() == ir.OSLICEARR || n.Op() == ir.OSLICE3 || n.Op() == ir.OSLICE3ARR || n.Op() == ir.OSLICESTR || n.Op() == ir.ODEREF || n.Op() == ir.ODOTPTR || n.Op() == ir.ODOTTYPE || n.Op() == ir.ODIV || n.Op() == ir.OMOD || n.Op() == ir.OSLICE2ARRPTR) 
            // These ops might panic, make sure they are done
            // before we start marshaling args for a call. See issue 16760.
            return true;
        else if (n.Op() == ir.OANDAND || n.Op() == ir.OOROR) 
            ptr<ir.LogicalExpr> n = n._<ptr<ir.LogicalExpr>>(); 
            // The RHS expression may have init statements that
            // should only execute conditionally, and so cannot be
            // pulled out to the top-level init list. We could try
            // to be more precise here.
            return len(n.Y.Init()) != 0; 

            // When using soft-float, these ops might be rewritten to function calls
            // so we ensure they are evaluated first.
        else if (n.Op() == ir.OADD || n.Op() == ir.OSUB || n.Op() == ir.OMUL || n.Op() == ir.ONEG) 
            return ssagen.Arch.SoftFloat && isSoftFloat(n.Type());
        else if (n.Op() == ir.OLT || n.Op() == ir.OEQ || n.Op() == ir.ONE || n.Op() == ir.OLE || n.Op() == ir.OGE || n.Op() == ir.OGT) 
            n = n._<ptr<ir.BinaryExpr>>();
            return ssagen.Arch.SoftFloat && isSoftFloat(n.X.Type());
        else if (n.Op() == ir.OCONV) 
            n = n._<ptr<ir.ConvExpr>>();
            return ssagen.Arch.SoftFloat && (isSoftFloat(n.Type()) || isSoftFloat(n.X.Type()));
        else if (n.Op() == ir.OLITERAL || n.Op() == ir.ONIL || n.Op() == ir.ONAME || n.Op() == ir.OLINKSYMOFFSET || n.Op() == ir.OMETHEXPR || n.Op() == ir.OAND || n.Op() == ir.OANDNOT || n.Op() == ir.OLSH || n.Op() == ir.OOR || n.Op() == ir.ORSH || n.Op() == ir.OXOR || n.Op() == ir.OCOMPLEX || n.Op() == ir.OEFACE || n.Op() == ir.OADDR || n.Op() == ir.OBITNOT || n.Op() == ir.ONOT || n.Op() == ir.OPLUS || n.Op() == ir.OCAP || n.Op() == ir.OIMAG || n.Op() == ir.OLEN || n.Op() == ir.OREAL || n.Op() == ir.OCONVNOP || n.Op() == ir.ODOT || n.Op() == ir.OCFUNC || n.Op() == ir.OIDATA || n.Op() == ir.OITAB || n.Op() == ir.OSPTR || n.Op() == ir.OBYTES2STRTMP || n.Op() == ir.OGETG || n.Op() == ir.OSLICEHEADER)         else 
            @base.FatalfAt(n.Pos(), "mayCall %+v", n);
                return false;

    });

}

// itabType loads the _type field from a runtime.itab struct.
private static ir.Node itabType(ir.Node itab) {
    if (itabTypeField == null) { 
        // runtime.itab's _type field
        itabTypeField = runtimeField("_type", int64(types.PtrSize), _addr_types.NewPtr(types.Types[types.TUINT8]));

    }
    return boundedDotPtr(@base.Pos, itab, _addr_itabTypeField);

}

private static ptr<types.Field> itabTypeField;

// boundedDotPtr returns a selector expression representing ptr.field
// and omits nil-pointer checks for ptr.
private static ptr<ir.SelectorExpr> boundedDotPtr(src.XPos pos, ir.Node ptr, ptr<types.Field> _addr_field) {
    ref types.Field field = ref _addr_field.val;

    var sel = ir.NewSelectorExpr(pos, ir.ODOTPTR, ptr, field.Sym);
    sel.Selection = field;
    sel.SetType(field.Type);
    sel.SetTypecheck(1);
    sel.SetBounded(true); // guaranteed not to fault
    return _addr_sel!;

}

private static ptr<types.Field> runtimeField(@string name, long offset, ptr<types.Type> _addr_typ) {
    ref types.Type typ = ref _addr_typ.val;

    var f = types.NewField(src.NoXPos, ir.Pkgs.Runtime.Lookup(name), typ);
    f.Offset = offset;
    return _addr_f!;
}

// ifaceData loads the data field from an interface.
// The concrete type must be known to have type t.
// It follows the pointer if !IsDirectIface(t).
private static ir.Node ifaceData(src.XPos pos, ir.Node n, ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    if (t.IsInterface()) {
        @base.Fatalf("ifaceData interface: %v", t);
    }
    var ptr = ir.NewUnaryExpr(pos, ir.OIDATA, n);
    if (types.IsDirectIface(t)) {
        ptr.SetType(t);
        ptr.SetTypecheck(1);
        return ptr;
    }
    ptr.SetType(types.NewPtr(t));
    ptr.SetTypecheck(1);
    var ind = ir.NewStarExpr(pos, ptr);
    ind.SetType(t);
    ind.SetTypecheck(1);
    ind.SetBounded(true);
    return ind;

}

} // end walk_package
