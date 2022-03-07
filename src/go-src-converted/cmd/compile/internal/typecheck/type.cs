// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package typecheck -- go2cs converted at 2022 March 06 22:48:46 UTC
// import "cmd/compile/internal/typecheck" ==> using typecheck = go.cmd.compile.@internal.typecheck_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\typecheck\type.go
using constant = go.go.constant_package;

using @base = go.cmd.compile.@internal.@base_package;
using ir = go.cmd.compile.@internal.ir_package;
using types = go.cmd.compile.@internal.types_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class typecheck_package {

    // tcArrayType typechecks an OTARRAY node.
private static ir.Node tcArrayType(ptr<ir.ArrayType> _addr_n) {
    ref ir.ArrayType n = ref _addr_n.val;

    n.Elem = typecheckNtype(n.Elem);
    if (n.Elem.Type() == null) {
        return n;
    }
    if (n.Len == null) { // [...]T
        if (!n.Diag()) {
            n.SetDiag(true);
            @base.Errorf("use of [...] array outside of array literal");
        }
        return n;

    }
    n.Len = indexlit(Expr(n.Len));
    var size = n.Len;
    if (ir.ConstType(size) != constant.Int) {

        if (size.Type() == null)         else if (size.Type().IsInteger() && size.Op() != ir.OLITERAL) 
            @base.Errorf("non-constant array bound %v", size);
        else 
            @base.Errorf("invalid array bound %v", size);
                return n;

    }
    var v = size.Val();
    if (ir.ConstOverflow(v, types.Types[types.TINT])) {
        @base.Errorf("array bound is too large");
        return n;
    }
    if (constant.Sign(v) < 0) {
        @base.Errorf("array bound must be non-negative");
        return n;
    }
    var (bound, _) = constant.Int64Val(v);
    var t = types.NewArray(n.Elem.Type(), bound);
    n.SetOTYPE(t);
    types.CheckSize(t);
    return n;

}

// tcChanType typechecks an OTCHAN node.
private static ir.Node tcChanType(ptr<ir.ChanType> _addr_n) {
    ref ir.ChanType n = ref _addr_n.val;

    n.Elem = typecheckNtype(n.Elem);
    var l = n.Elem;
    if (l.Type() == null) {
        return n;
    }
    if (l.Type().NotInHeap()) {
        @base.Errorf("chan of incomplete (or unallocatable) type not allowed");
    }
    n.SetOTYPE(types.NewChan(l.Type(), n.Dir));
    return n;

}

// tcFuncType typechecks an OTFUNC node.
private static ir.Node tcFuncType(ptr<ir.FuncType> _addr_n) {
    ref ir.FuncType n = ref _addr_n.val;

    Action<ptr<types.Field>, ptr<ir.Field>> misc = (f, nf) => {
        f.SetIsDDD(nf.IsDDD);
        if (nf.Decl != null) {
            nf.Decl.SetType(f.Type);
            f.Nname = nf.Decl;
        }
    };

    var lno = @base.Pos;

    ptr<types.Field> recv;
    if (n.Recv != null) {
        recv = tcField(_addr_n.Recv, misc);
    }
    var t = types.NewSignature(types.LocalPkg, recv, null, tcFields(n.Params, misc), tcFields(n.Results, misc));
    checkdupfields("argument", t.Recvs().FieldSlice(), t.Params().FieldSlice(), t.Results().FieldSlice());

    @base.Pos = lno;

    n.SetOTYPE(t);
    return n;

}

// tcInterfaceType typechecks an OTINTER node.
private static ir.Node tcInterfaceType(ptr<ir.InterfaceType> _addr_n) {
    ref ir.InterfaceType n = ref _addr_n.val;

    if (len(n.Methods) == 0) {
        n.SetOTYPE(types.Types[types.TINTER]);
        return n;
    }
    var lno = @base.Pos;
    var methods = tcFields(n.Methods, null);
    @base.Pos = lno;

    n.SetOTYPE(types.NewInterface(types.LocalPkg, methods));
    return n;

}

// tcMapType typechecks an OTMAP node.
private static ir.Node tcMapType(ptr<ir.MapType> _addr_n) {
    ref ir.MapType n = ref _addr_n.val;

    n.Key = typecheckNtype(n.Key);
    n.Elem = typecheckNtype(n.Elem);
    var l = n.Key;
    var r = n.Elem;
    if (l.Type() == null || r.Type() == null) {
        return n;
    }
    if (l.Type().NotInHeap()) {
        @base.Errorf("incomplete (or unallocatable) map key not allowed");
    }
    if (r.Type().NotInHeap()) {
        @base.Errorf("incomplete (or unallocatable) map value not allowed");
    }
    n.SetOTYPE(types.NewMap(l.Type(), r.Type()));
    mapqueue = append(mapqueue, n); // check map keys when all types are settled
    return n;

}

// tcSliceType typechecks an OTSLICE node.
private static ir.Node tcSliceType(ptr<ir.SliceType> _addr_n) {
    ref ir.SliceType n = ref _addr_n.val;

    n.Elem = typecheckNtype(n.Elem);
    if (n.Elem.Type() == null) {
        return n;
    }
    var t = types.NewSlice(n.Elem.Type());
    n.SetOTYPE(t);
    types.CheckSize(t);
    return n;

}

// tcStructType typechecks an OTSTRUCT node.
private static ir.Node tcStructType(ptr<ir.StructType> _addr_n) {
    ref ir.StructType n = ref _addr_n.val;

    var lno = @base.Pos;

    var fields = tcFields(n.Fields, (f, nf) => {
        if (nf.Embedded) {
            checkembeddedtype(f.Type);
            f.Embedded = 1;
        }
        f.Note = nf.Note;

    });
    checkdupfields("field", fields);

    @base.Pos = lno;
    n.SetOTYPE(types.NewStruct(types.LocalPkg, fields));
    return n;

}

// tcField typechecks a generic Field.
// misc can be provided to handle specialized typechecking.
private static ptr<types.Field> tcField(ptr<ir.Field> _addr_n, Action<ptr<types.Field>, ptr<ir.Field>> misc) {
    ref ir.Field n = ref _addr_n.val;

    @base.Pos = n.Pos;
    if (n.Ntype != null) {
        n.Type = typecheckNtype(n.Ntype).Type();
        n.Ntype = null;
    }
    var f = types.NewField(n.Pos, n.Sym, n.Type);
    if (misc != null) {
        misc(f, n);
    }
    return _addr_f!;

}

// tcFields typechecks a slice of generic Fields.
// misc can be provided to handle specialized typechecking.
private static slice<ptr<types.Field>> tcFields(slice<ptr<ir.Field>> l, Action<ptr<types.Field>, ptr<ir.Field>> misc) {
    var fields = make_slice<ptr<types.Field>>(len(l));
    foreach (var (i, n) in l) {
        fields[i] = tcField(_addr_n, misc);
    }    return fields;
}

} // end typecheck_package
