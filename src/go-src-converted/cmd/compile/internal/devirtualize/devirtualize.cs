// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package devirtualize implements a simple "devirtualization"
// optimization pass, which replaces interface method calls with
// direct concrete-type method calls where possible.
// package devirtualize -- go2cs converted at 2022 March 06 23:12:13 UTC
// import "cmd/compile/internal/devirtualize" ==> using devirtualize = go.cmd.compile.@internal.devirtualize_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\devirtualize\devirtualize.go
using @base = go.cmd.compile.@internal.@base_package;
using ir = go.cmd.compile.@internal.ir_package;
using typecheck = go.cmd.compile.@internal.typecheck_package;
using types = go.cmd.compile.@internal.types_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class devirtualize_package {

    // Func devirtualizes calls within fn where possible.
public static void Func(ptr<ir.Func> _addr_fn) {
    ref ir.Func fn = ref _addr_fn.val;

    ir.CurFunc = fn;
    ir.VisitList(fn.Body, n => {
        {
            ptr<ir.CallExpr> (call, ok) = n._<ptr<ir.CallExpr>>();

            if (ok) {
                Call(call);
            }
        }

    });

}

// Call devirtualizes the given call if possible.
public static void Call(ptr<ir.CallExpr> _addr_call) {
    ref ir.CallExpr call = ref _addr_call.val;

    if (call.Op() != ir.OCALLINTER) {
        return ;
    }
    ptr<ir.SelectorExpr> sel = call.X._<ptr<ir.SelectorExpr>>();
    var r = ir.StaticValue(sel.X);
    if (r.Op() != ir.OCONVIFACE) {
        return ;
    }
    ptr<ir.ConvExpr> recv = r._<ptr<ir.ConvExpr>>();

    var typ = recv.X.Type();
    if (typ.IsInterface()) {
        return ;
    }
    var dt = ir.NewTypeAssertExpr(sel.Pos(), sel.X, null);
    dt.SetType(typ);
    var x = typecheck.Callee(ir.NewSelectorExpr(sel.Pos(), ir.OXDOT, dt, sel.Sel));

    if (x.Op() == ir.ODOTMETH) 
        x = x._<ptr<ir.SelectorExpr>>();
        if (@base.Flag.LowerM != 0) {
            @base.WarnfAt(call.Pos(), "devirtualizing %v to %v", sel, typ);
        }
        call.SetOp(ir.OCALLMETH);
        call.X = x;
    else if (x.Op() == ir.ODOTINTER) 
        // Promoted method from embedded interface-typed field (#42279).
        x = x._<ptr<ir.SelectorExpr>>();
        if (@base.Flag.LowerM != 0) {
            @base.WarnfAt(call.Pos(), "partially devirtualizing %v to %v", sel, typ);
        }
        call.SetOp(ir.OCALLINTER);
        call.X = x;
    else 
        // TODO(mdempsky): Turn back into Fatalf after more testing.
        if (@base.Flag.LowerM != 0) {
            @base.WarnfAt(call.Pos(), "failed to devirtualize %v (%v)", x, x.Op());
        }
        return ;
    // Duplicated logic from typecheck for function call return
    // value types.
    //
    // Receiver parameter size may have changed; need to update
    // call.Type to get correct stack offsets for result
    // parameters.
    types.CheckSize(x.Type());
    {
        var ft = x.Type();

        switch (ft.NumResults()) {
            case 0: 

                break;
            case 1: 
                call.SetType(ft.Results().Field(0).Type);
                break;
            default: 
                call.SetType(ft.Results());
                break;
        }
    }

}

} // end devirtualize_package
