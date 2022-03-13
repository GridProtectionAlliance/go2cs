// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package walk -- go2cs converted at 2022 March 13 06:25:24 UTC
// import "cmd/compile/internal/walk" ==> using walk = go.cmd.compile.@internal.walk_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\walk\stmt.go
namespace go.cmd.compile.@internal;

using @base = cmd.compile.@internal.@base_package;
using ir = cmd.compile.@internal.ir_package;
using typecheck = cmd.compile.@internal.typecheck_package;


// The result of walkStmt MUST be assigned back to n, e.g.
//     n.Left = walkStmt(n.Left)

public static partial class walk_package {

private static ir.Node walkStmt(ir.Node n) => func((_, panic, _) => {
    if (n == null) {
        return n;
    }
    ir.SetPos(n);

    walkStmtList(n.Init());


    if (n.Op() == ir.OAS || n.Op() == ir.OASOP || n.Op() == ir.OAS2 || n.Op() == ir.OAS2DOTTYPE || n.Op() == ir.OAS2RECV || n.Op() == ir.OAS2FUNC || n.Op() == ir.OAS2MAPR || n.Op() == ir.OCLOSE || n.Op() == ir.OCOPY || n.Op() == ir.OCALLMETH || n.Op() == ir.OCALLINTER || n.Op() == ir.OCALL || n.Op() == ir.OCALLFUNC || n.Op() == ir.ODELETE || n.Op() == ir.OSEND || n.Op() == ir.OPRINT || n.Op() == ir.OPRINTN || n.Op() == ir.OPANIC || n.Op() == ir.ORECOVER || n.Op() == ir.OGETG)
    {
        if (n.Typecheck() == 0) {
            @base.Fatalf("missing typecheck: %+v", n);
        }
        ref var init = ref heap(ir.TakeInit(n), out ptr<var> _addr_init);
        n = walkExpr(n, _addr_init);
        if (n.Op() == ir.ONAME) { 
            // copy rewrote to a statement list and a temp for the length.
            // Throw away the temp to avoid plain values as statements.
            n = ir.NewBlockStmt(n.Pos(), init);
            init = null;
        }
        if (len(init) > 0) {

            if (n.Op() == ir.OAS || n.Op() == ir.OAS2 || n.Op() == ir.OBLOCK) 
                n._<ir.InitNode>().PtrInit().Prepend(init);
            else 
                init.Append(n);
                n = ir.NewBlockStmt(n.Pos(), init);
                    }
        return n; 

        // special case for a receive where we throw away
        // the value received.
        goto __switch_break0;
    }
    if (n.Op() == ir.ORECV)
    {
        n = n._<ptr<ir.UnaryExpr>>();
        return walkRecv(n);
        goto __switch_break0;
    }
    if (n.Op() == ir.OBREAK || n.Op() == ir.OCONTINUE || n.Op() == ir.OFALL || n.Op() == ir.OGOTO || n.Op() == ir.OLABEL || n.Op() == ir.ODCL || n.Op() == ir.ODCLCONST || n.Op() == ir.ODCLTYPE || n.Op() == ir.OCHECKNIL || n.Op() == ir.OVARDEF || n.Op() == ir.OVARKILL || n.Op() == ir.OVARLIVE)
    {
        return n;
        goto __switch_break0;
    }
    if (n.Op() == ir.OBLOCK)
    {
        n = n._<ptr<ir.BlockStmt>>();
        walkStmtList(n.List);
        return n;
        goto __switch_break0;
    }
    if (n.Op() == ir.OCASE)
    {
        @base.Errorf("case statement out of place");
        panic("unreachable");
        goto __switch_break0;
    }
    if (n.Op() == ir.ODEFER)
    {
        n = n._<ptr<ir.GoDeferStmt>>();
        ir.CurFunc.SetHasDefer(true);
        ir.CurFunc.NumDefers++;
        if (ir.CurFunc.NumDefers > maxOpenDefers) { 
            // Don't allow open-coded defers if there are more than
            // 8 defers in the function, since we use a single
            // byte to record active defers.
            ir.CurFunc.SetOpenCodedDeferDisallowed(true);
        }
        if (n.Esc() != ir.EscNever) { 
            // If n.Esc is not EscNever, then this defer occurs in a loop,
            // so open-coded defers cannot be used in this function.
            ir.CurFunc.SetOpenCodedDeferDisallowed(true);
        }
        fallthrough = true;
    }
    if (fallthrough || n.Op() == ir.OGO)
    {
        n = n._<ptr<ir.GoDeferStmt>>();
        return walkGoDefer(n);
        goto __switch_break0;
    }
    if (n.Op() == ir.OFOR || n.Op() == ir.OFORUNTIL)
    {
        n = n._<ptr<ir.ForStmt>>();
        return walkFor(n);
        goto __switch_break0;
    }
    if (n.Op() == ir.OIF)
    {
        n = n._<ptr<ir.IfStmt>>();
        return walkIf(n);
        goto __switch_break0;
    }
    if (n.Op() == ir.ORETURN)
    {
        n = n._<ptr<ir.ReturnStmt>>();
        return walkReturn(n);
        goto __switch_break0;
    }
    if (n.Op() == ir.OTAILCALL)
    {
        n = n._<ptr<ir.TailCallStmt>>();
        return n;
        goto __switch_break0;
    }
    if (n.Op() == ir.OINLMARK)
    {
        n = n._<ptr<ir.InlineMarkStmt>>();
        return n;
        goto __switch_break0;
    }
    if (n.Op() == ir.OSELECT)
    {
        n = n._<ptr<ir.SelectStmt>>();
        walkSelect(n);
        return n;
        goto __switch_break0;
    }
    if (n.Op() == ir.OSWITCH)
    {
        n = n._<ptr<ir.SwitchStmt>>();
        walkSwitch(n);
        return n;
        goto __switch_break0;
    }
    if (n.Op() == ir.ORANGE)
    {
        n = n._<ptr<ir.RangeStmt>>();
        return walkRange(n);
        goto __switch_break0;
    }
    // default: 
        if (n.Op() == ir.ONAME) {
            ptr<ir.Name> n = n._<ptr<ir.Name>>();
            @base.Errorf("%v is not a top level statement", n.Sym());
        }
        else
 {
            @base.Errorf("%v is not a top level statement", n.Op());
        }
        ir.Dump("nottop", n);
        return n;

    __switch_break0:; 

    // No return! Each case must return (or panic),
    // to avoid confusion about what gets returned
    // in the presence of type assertions.
});

private static void walkStmtList(slice<ir.Node> s) {
    foreach (var (i) in s) {
        s[i] = walkStmt(s[i]);
    }
}

// walkFor walks an OFOR or OFORUNTIL node.
private static ir.Node walkFor(ptr<ir.ForStmt> _addr_n) {
    ref ir.ForStmt n = ref _addr_n.val;

    if (n.Cond != null) {
        ref var init = ref heap(ir.TakeInit(n.Cond), out ptr<var> _addr_init);
        walkStmtList(init);
        n.Cond = walkExpr(n.Cond, _addr_init);
        n.Cond = ir.InitExpr(init, n.Cond);
    }
    n.Post = walkStmt(n.Post);
    if (n.Op() == ir.OFORUNTIL) {
        walkStmtList(n.Late);
    }
    walkStmtList(n.Body);
    return n;
}

// walkGoDefer walks an OGO or ODEFER node.
private static ir.Node walkGoDefer(ptr<ir.GoDeferStmt> _addr_n) {
    ref ir.GoDeferStmt n = ref _addr_n.val;

    ref ir.Nodes init = ref heap(out ptr<ir.Nodes> _addr_init);
    {
        var call__prev1 = call;

        var call = n.Call;


        if (call.Op() == ir.OPRINT || call.Op() == ir.OPRINTN) 
            call = call._<ptr<ir.CallExpr>>();
            n.Call = wrapCall(_addr_call, _addr_init);
        else if (call.Op() == ir.ODELETE) 
            call = call._<ptr<ir.CallExpr>>();
            n.Call = wrapCall(_addr_call, _addr_init);
        else if (call.Op() == ir.OCOPY) 
            call = call._<ptr<ir.BinaryExpr>>();
            n.Call = walkCopy(call, _addr_init, true);
        else if (call.Op() == ir.OCALLFUNC || call.Op() == ir.OCALLMETH || call.Op() == ir.OCALLINTER) 
            call = call._<ptr<ir.CallExpr>>();
            if (len(call.KeepAlive) > 0) {
                n.Call = wrapCall(_addr_call, _addr_init);
            }
            else
 {
                n.Call = walkExpr(call, _addr_init);
            }
        else 
            n.Call = walkExpr(call, _addr_init);


        call = call__prev1;
    }
    if (len(init) > 0) {
        init.Append(n);
        return ir.NewBlockStmt(n.Pos(), init);
    }
    return n;
}

// walkIf walks an OIF node.
private static ir.Node walkIf(ptr<ir.IfStmt> _addr_n) {
    ref ir.IfStmt n = ref _addr_n.val;

    n.Cond = walkExpr(n.Cond, n.PtrInit());
    walkStmtList(n.Body);
    walkStmtList(n.Else);
    return n;
}

// Rewrite
//    go builtin(x, y, z)
// into
//    go func(a1, a2, a3) {
//        builtin(a1, a2, a3)
//    }(x, y, z)
// for print, println, and delete.
//
// Rewrite
//    go f(x, y, uintptr(unsafe.Pointer(z)))
// into
//    go func(a1, a2, a3) {
//        f(a1, a2, uintptr(a3))
//    }(x, y, unsafe.Pointer(z))
// for function contains unsafe-uintptr arguments.

private static nint wrapCall_prgen = default;

// The result of wrapCall MUST be assigned back to n, e.g.
//     n.Left = wrapCall(n.Left, init)
private static ir.Node wrapCall(ptr<ir.CallExpr> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.CallExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    if (len(n.Init()) != 0) {
        walkStmtList(n.Init());
        init.Append(ir.TakeInit(n));
    }
    var isBuiltinCall = n.Op() != ir.OCALLFUNC && n.Op() != ir.OCALLMETH && n.Op() != ir.OCALLINTER; 

    // Turn f(a, b, []T{c, d, e}...) back into f(a, b, c, d, e).
    if (!isBuiltinCall && n.IsDDD) {
        undoVariadic(_addr_n);
    }
    var wrapArgs = n.Args; 
    // If there's a receiver argument, it needs to be passed through the wrapper too.
    if (n.Op() == ir.OCALLMETH || n.Op() == ir.OCALLINTER) {
        ptr<ir.SelectorExpr> recv = n.X._<ptr<ir.SelectorExpr>>().X;
        wrapArgs = append(new slice<ir.Node>(new ir.Node[] { recv }), wrapArgs);
    }
    var origArgs = make_slice<ir.Node>(len(wrapArgs));
    slice<ptr<ir.Field>> funcArgs = default;
    {
        var i__prev1 = i;

        foreach (var (__i, __arg) in wrapArgs) {
            i = __i;
            arg = __arg;
            var s = typecheck.LookupNum("a", i);
            if (!isBuiltinCall && arg.Op() == ir.OCONVNOP && arg.Type().IsUintptr() && arg._<ptr<ir.ConvExpr>>().X.Type().IsUnsafePtr()) {
                origArgs[i] = arg;
                arg = arg._<ptr<ir.ConvExpr>>().X;
                wrapArgs[i] = arg;
            }
            funcArgs = append(funcArgs, ir.NewField(@base.Pos, s, null, arg.Type()));
        }
        i = i__prev1;
    }

    var t = ir.NewFuncType(@base.Pos, null, funcArgs, null);

    wrapCall_prgen++;
    var sym = typecheck.LookupNum("wrap·", wrapCall_prgen);
    var fn = typecheck.DeclFunc(sym, t);

    var args = ir.ParamNames(t.Type());
    {
        var i__prev1 = i;

        foreach (var (__i, __origArg) in origArgs) {
            i = __i;
            origArg = __origArg;
            if (origArg == null) {
                continue;
            }
            args[i] = ir.NewConvExpr(@base.Pos, origArg.Op(), origArg.Type(), args[i]);
        }
        i = i__prev1;
    }

    if (n.Op() == ir.OCALLMETH || n.Op() == ir.OCALLINTER) { 
        // Move wrapped receiver argument back to its appropriate place.
        recv = typecheck.Expr(args[0]);
        n.X._<ptr<ir.SelectorExpr>>().X = addr(recv);
        args = args[(int)1..];
    }
    var call = ir.NewCallExpr(@base.Pos, n.Op(), n.X, args);
    if (!isBuiltinCall) {
        call.SetOp(ir.OCALL);
        call.IsDDD = n.IsDDD;
    }
    fn.Body = new slice<ir.Node>(new ir.Node[] { call });

    typecheck.FinishFuncBody();

    typecheck.Func(fn);
    typecheck.Stmts(fn.Body);
    typecheck.Target.Decls = append(typecheck.Target.Decls, fn);

    call = ir.NewCallExpr(@base.Pos, ir.OCALL, fn.Nname, wrapArgs);
    return walkExpr(typecheck.Stmt(call), init);
}

// undoVariadic turns a call to a variadic function of the form
//
//      f(a, b, []T{c, d, e}...)
//
// back into
//
//      f(a, b, c, d, e)
//
private static void undoVariadic(ptr<ir.CallExpr> _addr_call) {
    ref ir.CallExpr call = ref _addr_call.val;

    if (call.IsDDD) {
        var last = len(call.Args) - 1;
        {
            var va__prev2 = va;

            var va = call.Args[last];

            if (va.Op() == ir.OSLICELIT) {
                va = va._<ptr<ir.CompLitExpr>>();
                call.Args = append(call.Args[..(int)last], va.List);
                call.IsDDD = false;
            }

            va = va__prev2;

        }
    }
}

} // end walk_package
