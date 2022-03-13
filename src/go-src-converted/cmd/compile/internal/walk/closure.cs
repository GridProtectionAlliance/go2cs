// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package walk -- go2cs converted at 2022 March 13 06:24:54 UTC
// import "cmd/compile/internal/walk" ==> using walk = go.cmd.compile.@internal.walk_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\walk\closure.go
namespace go.cmd.compile.@internal;

using @base = cmd.compile.@internal.@base_package;
using ir = cmd.compile.@internal.ir_package;
using typecheck = cmd.compile.@internal.typecheck_package;
using types = cmd.compile.@internal.types_package;
using src = cmd.@internal.src_package;


// directClosureCall rewrites a direct call of a function literal into
// a normal function call with closure variables passed as arguments.
// This avoids allocation of a closure object.
//
// For illustration, the following call:
//
//    func(a int) {
//        println(byval)
//        byref++
//    }(42)
//
// becomes:
//
//    func(byval int, &byref *int, a int) {
//        println(byval)
//        (*&byref)++
//    }(byval, &byref, 42)

public static partial class walk_package {

private static void directClosureCall(ptr<ir.CallExpr> _addr_n) {
    ref ir.CallExpr n = ref _addr_n.val;

    ptr<ir.ClosureExpr> clo = n.X._<ptr<ir.ClosureExpr>>();
    var clofn = clo.Func;

    if (ir.IsTrivialClosure(clo)) {
        return ; // leave for walkClosure to handle
    }
    if (n.PreserveClosure) {
        return ;
    }
    slice<ptr<types.Field>> @params = default;
    slice<ptr<ir.Name>> decls = default;
    foreach (var (_, v) in clofn.ClosureVars) {
        if (!v.Byval()) { 
            // If v of type T is captured by reference,
            // we introduce function param &v *T
            // and v remains PAUTOHEAP with &v heapaddr
            // (accesses will implicitly deref &v).

            var addr = ir.NewNameAt(clofn.Pos(), typecheck.Lookup("&" + v.Sym().Name));
            addr.Curfn = clofn;
            addr.SetType(types.NewPtr(v.Type()));
            v.Heapaddr = addr;
            v = addr;
        }
        v.Class = ir.PPARAM;
        decls = append(decls, v);

        var fld = types.NewField(src.NoXPos, v.Sym(), v.Type());
        fld.Nname = v;
        params = append(params, fld);
    }    var f = clofn.Nname;
    var typ = f.Type(); 

    // Create new function type with parameters prepended, and
    // then update type and declarations.
    typ = types.NewSignature(typ.Pkg(), null, null, append(params, typ.Params().FieldSlice()), typ.Results().FieldSlice());
    f.SetType(typ);
    clofn.Dcl = append(decls, clofn.Dcl); 

    // Rewrite call.
    n.X = f;
    n.Args.Prepend(closureArgs(clo)); 

    // Update the call expression's type. We need to do this
    // because typecheck gave it the result type of the OCLOSURE
    // node, but we only rewrote the ONAME node's type. Logically,
    // they're the same, but the stack offsets probably changed.
    if (typ.NumResults() == 1) {
        n.SetType(typ.Results().Field(0).Type);
    }
    else
 {
        n.SetType(typ.Results());
    }
    ir.CurFunc.Closures = append(ir.CurFunc.Closures, clofn);
}

private static ir.Node walkClosure(ptr<ir.ClosureExpr> _addr_clo, ptr<ir.Nodes> _addr_init) => func((_, panic, _) => {
    ref ir.ClosureExpr clo = ref _addr_clo.val;
    ref ir.Nodes init = ref _addr_init.val;

    var clofn = clo.Func; 

    // If no closure vars, don't bother wrapping.
    if (ir.IsTrivialClosure(clo)) {
        if (@base.Debug.Closure > 0) {
            @base.WarnfAt(clo.Pos(), "closure converted to global");
        }
        return clofn.Nname;
    }
    ir.ClosureDebugRuntimeCheck(clo);
    clofn.SetNeedctxt(true);
    ir.CurFunc.Closures = append(ir.CurFunc.Closures, clofn);

    var typ = typecheck.ClosureType(clo);

    var clos = ir.NewCompLitExpr(@base.Pos, ir.OCOMPLIT, ir.TypeNode(typ), null);
    clos.SetEsc(clo.Esc());
    clos.List = append(new slice<ir.Node>(new ir.Node[] { ir.NewUnaryExpr(base.Pos,ir.OCFUNC,clofn.Nname) }), closureArgs(_addr_clo));

    var addr = typecheck.NodAddr(clos);
    addr.SetEsc(clo.Esc()); 

    // Force type conversion from *struct to the func type.
    var cfn = typecheck.ConvNop(addr, clo.Type()); 

    // non-escaping temp to use, if any.
    {
        var x = clo.Prealloc;

        if (x != null) {
            if (!types.Identical(typ, x.Type())) {
                panic("closure type does not match order's assigned type");
            }
            addr.Prealloc = x;
            clo.Prealloc = null;
        }
    }

    return walkExpr(cfn, init);
});

// closureArgs returns a slice of expressions that an be used to
// initialize the given closure's free variables. These correspond
// one-to-one with the variables in clo.Func.ClosureVars, and will be
// either an ONAME node (if the variable is captured by value) or an
// OADDR-of-ONAME node (if not).
private static slice<ir.Node> closureArgs(ptr<ir.ClosureExpr> _addr_clo) {
    ref ir.ClosureExpr clo = ref _addr_clo.val;

    var fn = clo.Func;

    var args = make_slice<ir.Node>(len(fn.ClosureVars));
    foreach (var (i, v) in fn.ClosureVars) {
        ir.Node outer = default;
        outer = v.Outer;
        if (!v.Byval()) {
            outer = typecheck.NodAddrAt(fn.Pos(), outer);
        }
        args[i] = typecheck.Expr(outer);
    }    return args;
}

private static ir.Node walkCallPart(ptr<ir.SelectorExpr> _addr_n, ptr<ir.Nodes> _addr_init) => func((_, panic, _) => {
    ref ir.SelectorExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;
 
    // Create closure in the form of a composite literal.
    // For x.M with receiver (x) type T, the generated code looks like:
    //
    //    clos = &struct{F uintptr; R T}{T.M·f, x}
    //
    // Like walkClosure above.

    if (n.X.Type().IsInterface()) { 
        // Trigger panic for method on nil interface now.
        // Otherwise it happens in the wrapper and is confusing.
        n.X = cheapExpr(n.X, init);
        n.X = walkExpr(n.X, null);

        var tab = typecheck.Expr(ir.NewUnaryExpr(@base.Pos, ir.OITAB, n.X));

        var c = ir.NewUnaryExpr(@base.Pos, ir.OCHECKNIL, tab);
        c.SetTypecheck(1);
        init.Append(c);
    }
    var typ = typecheck.PartialCallType(n);

    var clos = ir.NewCompLitExpr(@base.Pos, ir.OCOMPLIT, ir.TypeNode(typ), null);
    clos.SetEsc(n.Esc());
    clos.List = new slice<ir.Node>(new ir.Node[] { ir.NewUnaryExpr(base.Pos,ir.OCFUNC,typecheck.MethodValueWrapper(n).Nname), n.X });

    var addr = typecheck.NodAddr(clos);
    addr.SetEsc(n.Esc()); 

    // Force type conversion from *struct to the func type.
    var cfn = typecheck.ConvNop(addr, n.Type()); 

    // non-escaping temp to use, if any.
    {
        var x = n.Prealloc;

        if (x != null) {
            if (!types.Identical(typ, x.Type())) {
                panic("partial call type does not match order's assigned type");
            }
            addr.Prealloc = x;
            n.Prealloc = null;
        }
    }

    return walkExpr(cfn, init);
});

} // end walk_package
