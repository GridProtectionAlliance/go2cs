// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package noder -- go2cs converted at 2022 March 06 23:13:49 UTC
// import "cmd/compile/internal/noder" ==> using noder = go.cmd.compile.@internal.noder_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\noder\helpers.go
using constant = go.go.constant_package;

using @base = go.cmd.compile.@internal.@base_package;
using ir = go.cmd.compile.@internal.ir_package;
using typecheck = go.cmd.compile.@internal.typecheck_package;
using types = go.cmd.compile.@internal.types_package;
using src = go.cmd.@internal.src_package;

namespace go.cmd.compile.@internal;

public static partial class noder_package {

    // Helpers for constructing typed IR nodes.
    //
    // TODO(mdempsky): Move into their own package so they can be easily
    // reused by iimport and frontend optimizations.
public partial interface ImplicitNode {
    void SetImplicit(bool x);
}

// Implicit returns n after marking it as Implicit.
public static ImplicitNode Implicit(ImplicitNode n) {
    n.SetImplicit(true);
    return n;
}

// typed returns n after setting its type to typ.
private static ir.Node typed(ptr<types.Type> _addr_typ, ir.Node n) {
    ref types.Type typ = ref _addr_typ.val;

    n.SetType(typ);
    n.SetTypecheck(1);
    return n;
}

// Values

public static ir.Node Const(src.XPos pos, ptr<types.Type> _addr_typ, constant.Value val) {
    ref types.Type typ = ref _addr_typ.val;

    return typed(_addr_typ, ir.NewBasicLit(pos, val));
}

public static ir.Node Nil(src.XPos pos, ptr<types.Type> _addr_typ) {
    ref types.Type typ = ref _addr_typ.val;

    return typed(_addr_typ, ir.NewNilExpr(pos));
}

// Expressions

public static ptr<ir.AddrExpr> Addr(src.XPos pos, ir.Node x) {
    var n = typecheck.NodAddrAt(pos, x);

    if (x.Op() == ir.OARRAYLIT || x.Op() == ir.OMAPLIT || x.Op() == ir.OSLICELIT || x.Op() == ir.OSTRUCTLIT) 
        n.SetOp(ir.OPTRLIT);
        typed(_addr_types.NewPtr(x.Type()), n);
    return _addr_n!;

}

public static ir.Node Assert(src.XPos pos, ir.Node x, ptr<types.Type> _addr_typ) {
    ref types.Type typ = ref _addr_typ.val;

    return typed(_addr_typ, ir.NewTypeAssertExpr(pos, x, null));
}

public static ir.Node Binary(src.XPos pos, ir.Op op, ptr<types.Type> _addr_typ, ir.Node x, ir.Node y) {
    ref types.Type typ = ref _addr_typ.val;


    if (op == ir.OANDAND || op == ir.OOROR) 
        return typed(_addr_x.Type(), ir.NewLogicalExpr(pos, op, x, y));
    else if (op == ir.OADD) 
        var n = ir.NewBinaryExpr(pos, op, x, y);
        if (x.Type().HasTParam() || y.Type().HasTParam()) { 
            // Delay transformAdd() if either arg has a type param,
            // since it needs to know the exact types to decide whether
            // to transform OADD to OADDSTR.
            n.SetType(typ);
            n.SetTypecheck(3);
            return n;

        }
        typed(_addr_typ, n);
        return transformAdd(n);
    else 
        return typed(_addr_x.Type(), ir.NewBinaryExpr(pos, op, x, y));
    
}

public static ir.Node Call(src.XPos pos, ptr<types.Type> _addr_typ, ir.Node fun, slice<ir.Node> args, bool dots) {
    ref types.Type typ = ref _addr_typ.val;

    var n = ir.NewCallExpr(pos, ir.OCALL, fun, args);
    n.IsDDD = dots; 
    // n.Use will be changed to ir.CallUseStmt in g.stmt() if this call is
    // just a statement (any return values are ignored).
    n.Use = ir.CallUseExpr;

    if (fun.Op() == ir.OTYPE) { 
        // Actually a type conversion, not a function call.
        if (fun.Type().HasTParam() || args[0].Type().HasTParam()) { 
            // For type params, don't typecheck until we actually know
            // the type.
            return typed(_addr_typ, n);

        }
        typed(_addr_typ, n);
        return transformConvCall(n);

    }
    {
        ptr<ir.Name> fun__prev1 = fun;

        ptr<ir.Name> (fun, ok) = fun._<ptr<ir.Name>>();

        if (ok && fun.BuiltinOp != 0) { 
            // For Builtin ops, we currently stay with using the old
            // typechecker to transform the call to a more specific expression
            // and possibly use more specific ops. However, for a bunch of the
            // ops, we delay doing the old typechecker if any of the args have
            // type params, for a variety of reasons:
            //
            // OMAKE: hard to choose specific ops OMAKESLICE, etc. until arg type is known
            // OREAL/OIMAG: can't determine type float32/float64 until arg type know
            // OLEN/OCAP: old typechecker will complain if arg is not obviously a slice/array.
            // OAPPEND: old typechecker will complain if arg is not obviously slice, etc.
            //
            // We will eventually break out the transforming functionality
            // needed for builtin's, and call it here or during stenciling, as
            // appropriate.

            if (fun.BuiltinOp == ir.OMAKE || fun.BuiltinOp == ir.OREAL || fun.BuiltinOp == ir.OIMAG || fun.BuiltinOp == ir.OLEN || fun.BuiltinOp == ir.OCAP || fun.BuiltinOp == ir.OAPPEND) 
                var hasTParam = false;
                foreach (var (_, arg) in args) {
                    if (arg.Type().HasTParam()) {
                        hasTParam = true;
                        break;
                    }
                }
                if (hasTParam) {
                    return typed(_addr_typ, n);
                }
                        typed(_addr_typ, n);
            return transformBuiltin(n);

        }
        fun = fun__prev1;

    } 

    // Add information, now that we know that fun is actually being called.
    switch (fun.type()) {
        case ptr<ir.ClosureExpr> fun:
            fun.Func.SetClosureCalled(true);
            break;
        case ptr<ir.SelectorExpr> fun:
            if (fun.Op() == ir.OCALLPART) {
                var op = ir.ODOTMETH;
                if (fun.X.Type().IsInterface()) {
                    op = ir.ODOTINTER;
                }
                fun.SetOp(op); 
                // Set the type to include the receiver, since that's what
                // later parts of the compiler expect
                fun.SetType(fun.Selection.Type);

            }

            break;

    }

    if (fun.Type().HasTParam()) { 
        // If the fun arg is or has a type param, don't do any extra
        // transformations, since we may not have needed properties yet
        // (e.g. number of return values, etc). The type param is probably
        // described by a structural constraint that requires it to be a
        // certain function type, etc., but we don't want to analyze that.
        return typed(_addr_typ, n);

    }
    if (fun.Op() == ir.OXDOT) {
        if (!fun._<ptr<ir.SelectorExpr>>().X.Type().HasTParam()) {
            @base.FatalfAt(pos, "Expecting type param receiver in %v", fun);
        }
        typed(_addr_typ, n);
        return n;

    }
    if (fun.Op() != ir.OFUNCINST) { 
        // If no type params, do the normal call transformations. This
        // will convert OCALL to OCALLFUNC.
        typed(_addr_typ, n);
        transformCall(n);
        return n;

    }
    typed(_addr_typ, n);
    return n;

}

public static ir.Node Compare(src.XPos pos, ptr<types.Type> _addr_typ, ir.Op op, ir.Node x, ir.Node y) {
    ref types.Type typ = ref _addr_typ.val;

    var n = ir.NewBinaryExpr(pos, op, x, y);
    if (x.Type().HasTParam() || y.Type().HasTParam()) { 
        // Delay transformCompare() if either arg has a type param, since
        // it needs to know the exact types to decide on any needed conversions.
        n.SetType(typ);
        n.SetTypecheck(3);
        return n;

    }
    typed(_addr_typ, n);
    transformCompare(n);
    return n;

}

public static ptr<ir.StarExpr> Deref(src.XPos pos, ptr<types.Type> _addr_typ, ir.Node x) {
    ref types.Type typ = ref _addr_typ.val;

    var n = ir.NewStarExpr(pos, x);
    typed(_addr_typ, n);
    return _addr_n!;
}

public static ptr<ir.SelectorExpr> DotField(src.XPos pos, ir.Node x, nint index) {
    var op = ir.ODOT;
    var typ = x.Type();
    if (typ.IsPtr()) {
        (op, typ) = (ir.ODOTPTR, typ.Elem());
    }
    if (!typ.IsStruct()) {
        @base.FatalfAt(pos, "DotField of non-struct: %L", x);
    }
    types.CalcSize(typ);

    var field = typ.Field(index);
    return _addr_dot(pos, _addr_field.Type, op, x, _addr_field)!;

}

public static ptr<ir.SelectorExpr> DotMethod(src.XPos pos, ir.Node x, nint index) {
    var method = method(_addr_x.Type(), index); 

    // Method value.
    var typ = typecheck.NewMethodType(method.Type, null);
    return _addr_dot(pos, _addr_typ, ir.OCALLPART, x, _addr_method)!;

}

// MethodExpr returns a OMETHEXPR node with the indicated index into the methods
// of typ. The receiver type is set from recv, which is different from typ if the
// method was accessed via embedded fields. Similarly, the X value of the
// ir.SelectorExpr is recv, the original OTYPE node before passing through the
// embedded fields.
public static ptr<ir.SelectorExpr> MethodExpr(src.XPos pos, ir.Node recv, ptr<types.Type> _addr_embed, nint index) {
    ref types.Type embed = ref _addr_embed.val;

    var method = method(_addr_embed, index);
    var typ = typecheck.NewMethodType(method.Type, recv.Type()); 
    // The method expression T.m requires a wrapper when T
    // is different from m's declared receiver type. We
    // normally generate these wrappers while writing out
    // runtime type descriptors, which is always done for
    // types declared at package scope. However, we need
    // to make sure to generate wrappers for anonymous
    // receiver types too.
    if (recv.Sym() == null) {
        typecheck.NeedRuntimeType(recv.Type());
    }
    return _addr_dot(pos, _addr_typ, ir.OMETHEXPR, recv, _addr_method)!;

}

private static ptr<ir.SelectorExpr> dot(src.XPos pos, ptr<types.Type> _addr_typ, ir.Op op, ir.Node x, ptr<types.Field> _addr_selection) {
    ref types.Type typ = ref _addr_typ.val;
    ref types.Field selection = ref _addr_selection.val;

    var n = ir.NewSelectorExpr(pos, op, x, selection.Sym);
    n.Selection = selection;
    typed(_addr_typ, n);
    return _addr_n!;
}

// TODO(mdempsky): Move to package types.
private static ptr<types.Field> method(ptr<types.Type> _addr_typ, nint index) {
    ref types.Type typ = ref _addr_typ.val;

    if (typ.IsInterface()) {
        return _addr_typ.AllMethods().Index(index)!;
    }
    return _addr_types.ReceiverBaseType(typ).Methods().Index(index)!;

}

public static ir.Node Index(src.XPos pos, ptr<types.Type> _addr_typ, ir.Node x, ir.Node index) {
    ref types.Type typ = ref _addr_typ.val;

    var n = ir.NewIndexExpr(pos, x, index);
    if (x.Type().HasTParam()) { 
        // transformIndex needs to know exact type
        n.SetType(typ);
        n.SetTypecheck(3);
        return n;

    }
    typed(_addr_typ, n); 
    // transformIndex will modify n.Type() for OINDEXMAP.
    transformIndex(n);
    return n;

}

public static ir.Node Slice(src.XPos pos, ptr<types.Type> _addr_typ, ir.Node x, ir.Node low, ir.Node high, ir.Node max) {
    ref types.Type typ = ref _addr_typ.val;

    var op = ir.OSLICE;
    if (max != null) {
        op = ir.OSLICE3;
    }
    var n = ir.NewSliceExpr(pos, op, x, low, high, max);
    if (x.Type().HasTParam()) { 
        // transformSlice needs to know if x.Type() is a string or an array or a slice.
        n.SetType(typ);
        n.SetTypecheck(3);
        return n;

    }
    typed(_addr_typ, n);
    transformSlice(n);
    return n;

}

public static ir.Node Unary(src.XPos pos, ptr<types.Type> _addr_typ, ir.Op op, ir.Node x) {
    ref types.Type typ = ref _addr_typ.val;


    if (op == ir.OADDR) 
        return Addr(pos, x);
    else if (op == ir.ODEREF) 
        return Deref(pos, _addr_typ, x);
        if (op == ir.ORECV) {
        if (typ.IsFuncArgStruct() && typ.NumFields() == 2) { 
            // Remove the second boolean type (if provided by type2),
            // since that works better with the rest of the compiler
            // (which will add it back in later).
            assert(typ.Field(1).Type.Kind() == types.TBOOL);
            typ = typ.Field(0).Type;

        }
    }
    return typed(_addr_typ, ir.NewUnaryExpr(pos, op, x));

}

// Statements

private static var one = constant.MakeInt64(1);

public static ptr<ir.AssignOpStmt> IncDec(src.XPos pos, ir.Op op, ir.Node x) {
    assert(x.Type() != null);
    return _addr_ir.NewAssignOpStmt(pos, op, x, typecheck.DefaultLit(ir.NewBasicLit(pos, one), x.Type()))!;
}

} // end noder_package
