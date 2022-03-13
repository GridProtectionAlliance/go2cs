// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package walk -- go2cs converted at 2022 March 13 06:24:49 UTC
// import "cmd/compile/internal/walk" ==> using walk = go.cmd.compile.@internal.walk_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\walk\assign.go
namespace go.cmd.compile.@internal;

using constant = go.constant_package;

using @base = cmd.compile.@internal.@base_package;
using ir = cmd.compile.@internal.ir_package;
using reflectdata = cmd.compile.@internal.reflectdata_package;
using typecheck = cmd.compile.@internal.typecheck_package;
using types = cmd.compile.@internal.types_package;
using src = cmd.@internal.src_package;


// walkAssign walks an OAS (AssignExpr) or OASOP (AssignOpExpr) node.

using System;
public static partial class walk_package {

private static ir.Node walkAssign(ptr<ir.Nodes> _addr_init, ir.Node n) {
    ref ir.Nodes init = ref _addr_init.val;

    init.Append(ir.TakeInit(n));

    ir.Node left = default;    ir.Node right = default;


    if (n.Op() == ir.OAS) 
        ptr<ir.AssignStmt> n = n._<ptr<ir.AssignStmt>>();
        (left, right) = (n.X, n.Y);    else if (n.Op() == ir.OASOP) 
        n = n._<ptr<ir.AssignOpStmt>>();
        (left, right) = (n.X, n.Y);    // Recognize m[k] = append(m[k], ...) so we can reuse
    // the mapassign call.
    ptr<ir.CallExpr> mapAppend;
    if (left.Op() == ir.OINDEXMAP && right.Op() == ir.OAPPEND) {
        left = left._<ptr<ir.IndexExpr>>();
        mapAppend = right._<ptr<ir.CallExpr>>();
        if (!ir.SameSafeExpr(left, mapAppend.Args[0])) {
            @base.Fatalf("not same expressions: %v != %v", left, mapAppend.Args[0]);
        }
    }
    left = walkExpr(left, init);
    left = safeExpr(left, init);
    if (mapAppend != null) {
        mapAppend.Args[0] = left;
    }
    if (n.Op() == ir.OASOP) { 
        // Rewrite x op= y into x = x op y.
        n = ir.NewAssignStmt(@base.Pos, left, typecheck.Expr(ir.NewBinaryExpr(@base.Pos, n._<ptr<ir.AssignOpStmt>>().AsOp, left, right)));
    }
    else
 {
        n._<ptr<ir.AssignStmt>>().X = left;
    }
    ptr<ir.AssignStmt> @as = n._<ptr<ir.AssignStmt>>();

    if (oaslit(as, init)) {
        return ir.NewBlockStmt(@as.Pos(), null);
    }
    if (@as.Y == null) { 
        // TODO(austin): Check all "implicit zeroing"
        return as;
    }
    if (!@base.Flag.Cfg.Instrumenting && ir.IsZero(@as.Y)) {
        return as;
    }

    if (@as.Y.Op() == ir.ORECV) 
        // x = <-c; as.Left is x, as.Right.Left is c.
        // order.stmt made sure x is addressable.
        ptr<ir.UnaryExpr> recv = @as.Y._<ptr<ir.UnaryExpr>>();
        recv.X = walkExpr(recv.X, init);

        var n1 = typecheck.NodAddr(@as.X);
        var r = recv.X; // the channel
        return mkcall1(chanfn("chanrecv1", 2, r.Type()), null, init, r, n1);
    else if (@as.Y.Op() == ir.OAPPEND) 
        // x = append(...)
        ptr<ir.CallExpr> call = @as.Y._<ptr<ir.CallExpr>>();
        if (call.Type().Elem().NotInHeap()) {
            @base.Errorf("%v can't be allocated in Go; it is incomplete (or unallocatable)", call.Type().Elem());
        }
        r = default;

        if (isAppendOfMake(call)) 
            // x = append(y, make([]T, y)...)
            r = extendSlice(call, _addr_init);
        else if (call.IsDDD) 
            r = appendSlice(call, _addr_init); // also works for append(slice, string).
        else 
            r = walkAppend(call, init, as);
                @as.Y = r;
        if (r.Op() == ir.OAPPEND) { 
            // Left in place for back end.
            // Do not add a new write barrier.
            // Set up address of type for back end.
            r._<ptr<ir.CallExpr>>().X = reflectdata.TypePtr(r.Type().Elem());
            return as;
        }
    else 
        @as.Y = walkExpr(@as.Y, init);
        if (@as.X != null && @as.Y != null) {
        return convas(as, init);
    }
    return as;
}

// walkAssignDotType walks an OAS2DOTTYPE node.
private static ir.Node walkAssignDotType(ptr<ir.AssignListStmt> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.AssignListStmt n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    walkExprListSafe(n.Lhs, init);
    n.Rhs[0] = walkExpr(n.Rhs[0], init);
    return n;
}

// walkAssignFunc walks an OAS2FUNC node.
private static ir.Node walkAssignFunc(ptr<ir.Nodes> _addr_init, ptr<ir.AssignListStmt> _addr_n) {
    ref ir.Nodes init = ref _addr_init.val;
    ref ir.AssignListStmt n = ref _addr_n.val;

    init.Append(ir.TakeInit(n));

    var r = n.Rhs[0];
    walkExprListSafe(n.Lhs, init);
    r = walkExpr(r, init);

    if (ir.IsIntrinsicCall(r._<ptr<ir.CallExpr>>())) {
        n.Rhs = new slice<ir.Node>(new ir.Node[] { r });
        return n;
    }
    init.Append(r);

    var ll = ascompatet(n.Lhs, _addr_r.Type());
    return ir.NewBlockStmt(src.NoXPos, ll);
}

// walkAssignList walks an OAS2 node.
private static ir.Node walkAssignList(ptr<ir.Nodes> _addr_init, ptr<ir.AssignListStmt> _addr_n) {
    ref ir.Nodes init = ref _addr_init.val;
    ref ir.AssignListStmt n = ref _addr_n.val;

    init.Append(ir.TakeInit(n));
    return ir.NewBlockStmt(src.NoXPos, ascompatee(ir.OAS, n.Lhs, n.Rhs));
}

// walkAssignMapRead walks an OAS2MAPR node.
private static ir.Node walkAssignMapRead(ptr<ir.Nodes> _addr_init, ptr<ir.AssignListStmt> _addr_n) {
    ref ir.Nodes init = ref _addr_init.val;
    ref ir.AssignListStmt n = ref _addr_n.val;

    init.Append(ir.TakeInit(n));

    ptr<ir.IndexExpr> r = n.Rhs[0]._<ptr<ir.IndexExpr>>();
    walkExprListSafe(n.Lhs, init);
    r.X = walkExpr(r.X, init);
    r.Index = walkExpr(r.Index, init);
    var t = r.X.Type();

    var fast = mapfast(t);
    var key = mapKeyArg(fast, r, r.Index); 

    // from:
    //   a,b = m[i]
    // to:
    //   var,b = mapaccess2*(t, m, i)
    //   a = *var
    var a = n.Lhs[0];

    ptr<ir.CallExpr> call;
    {
        var w = t.Elem().Width;

        if (w <= zeroValSize) {
            var fn = mapfn(mapaccess2[fast], t, false);
            call = mkcall1(fn, fn.Type().Results(), init, reflectdata.TypePtr(t), r.X, key);
        }
        else
 {
            fn = mapfn("mapaccess2_fat", t, true);
            var z = reflectdata.ZeroAddr(w);
            call = mkcall1(fn, fn.Type().Results(), init, reflectdata.TypePtr(t), r.X, key, z);
        }
    } 

    // mapaccess2* returns a typed bool, but due to spec changes,
    // the boolean result of i.(T) is now untyped so we make it the
    // same type as the variable on the lhs.
    {
        var ok = n.Lhs[1];

        if (!ir.IsBlank(ok) && ok.Type().IsBoolean()) {
            call.Type().Field;

            (1).Type = ok.Type();
        }
    }
    n.Rhs = new slice<ir.Node>(new ir.Node[] { call });
    n.SetOp(ir.OAS2FUNC); 

    // don't generate a = *var if a is _
    if (ir.IsBlank(a)) {
        return walkExpr(typecheck.Stmt(n), init);
    }
    var var_ = typecheck.Temp(types.NewPtr(t.Elem()));
    var_.SetTypecheck(1);
    var_.MarkNonNil(); // mapaccess always returns a non-nil pointer

    n.Lhs[0] = var_;
    init.Append(walkExpr(n, init));

    var @as = ir.NewAssignStmt(@base.Pos, a, ir.NewStarExpr(@base.Pos, var_));
    return walkExpr(typecheck.Stmt(as), init);
}

// walkAssignRecv walks an OAS2RECV node.
private static ir.Node walkAssignRecv(ptr<ir.Nodes> _addr_init, ptr<ir.AssignListStmt> _addr_n) {
    ref ir.Nodes init = ref _addr_init.val;
    ref ir.AssignListStmt n = ref _addr_n.val;

    init.Append(ir.TakeInit(n));

    ptr<ir.UnaryExpr> r = n.Rhs[0]._<ptr<ir.UnaryExpr>>(); // recv
    walkExprListSafe(n.Lhs, init);
    r.X = walkExpr(r.X, init);
    ir.Node n1 = default;
    if (ir.IsBlank(n.Lhs[0])) {
        n1 = typecheck.NodNil();
    }
    else
 {
        n1 = typecheck.NodAddr(n.Lhs[0]);
    }
    var fn = chanfn("chanrecv2", 2, r.X.Type());
    var ok = n.Lhs[1];
    var call = mkcall1(fn, types.Types[types.TBOOL], init, r.X, n1);
    return typecheck.Stmt(ir.NewAssignStmt(@base.Pos, ok, call));
}

// walkReturn walks an ORETURN node.
private static ir.Node walkReturn(ptr<ir.ReturnStmt> _addr_n) {
    ref ir.ReturnStmt n = ref _addr_n.val;

    var fn = ir.CurFunc;

    fn.NumReturns++;
    if (len(n.Results) == 0) {
        return n;
    }
    var results = fn.Type().Results().FieldSlice();
    var dsts = make_slice<ir.Node>(len(results));
    foreach (var (i, v) in results) { 
        // TODO(mdempsky): typecheck should have already checked the result variables.
        dsts[i] = typecheck.AssignExpr(v.Nname._<ptr<ir.Name>>());
    }    n.Results = ascompatee(n.Op(), dsts, n.Results);
    return n;
}

// check assign type list to
// an expression list. called in
//    expr-list = func()
private static slice<ir.Node> ascompatet(ir.Nodes nl, ptr<types.Type> _addr_nr) {
    ref types.Type nr = ref _addr_nr.val;

    if (len(nl) != nr.NumFields()) {
        @base.Fatalf("ascompatet: assignment count mismatch: %d = %d", len(nl), nr.NumFields());
    }
    ir.Nodes nn = default;
    foreach (var (i, l) in nl) {
        if (ir.IsBlank(l)) {
            continue;
        }
        var r = nr.Field(i); 

        // Order should have created autotemps of the appropriate type for
        // us to store results into.
        {
            ptr<ir.Name> (tmp, ok) = l._<ptr<ir.Name>>();

            if (!ok || !tmp.AutoTemp() || !types.Identical(tmp.Type(), r.Type)) {
                @base.FatalfAt(l.Pos(), "assigning %v to %+v", r.Type, l);
            }

        }

        var res = ir.NewResultExpr(@base.Pos, null, types.BADWIDTH);
        res.Index = int64(i);
        res.SetType(r.Type);
        res.SetTypecheck(1);

        nn.Append(ir.NewAssignStmt(@base.Pos, l, res));
    }    return nn;
}

// check assign expression list to
// an expression list. called in
//    expr-list = expr-list
private static slice<ir.Node> ascompatee(ir.Op op, slice<ir.Node> nl, slice<ir.Node> nr) { 
    // cannot happen: should have been rejected during type checking
    if (len(nl) != len(nr)) {
        @base.Fatalf("assignment operands mismatch: %+v / %+v", ir.Nodes(nl), ir.Nodes(nr));
    }
    ir.NameSet assigned = default;
    bool memWrite = default;    bool deferResultWrite = default; 

    // affected reports whether expression n could be affected by
    // the assignments applied so far.
 

    // affected reports whether expression n could be affected by
    // the assignments applied so far.
    Func<ir.Node, bool> affected = n => {
        if (deferResultWrite) {
            return true;
        }
        return ir.Any(n, n => {
            if (n.Op() == ir.ONAME && assigned.Has(n._<ptr<ir.Name>>())) {
                return true;
            }
            if (memWrite && readsMemory(n)) {
                return true;
            }
            return false;
        });
    }; 

    // If a needed expression may be affected by an
    // earlier assignment, make an early copy of that
    // expression and use the copy instead.
    ref ir.Nodes early = ref heap(out ptr<ir.Nodes> _addr_early);
    Action<ptr<ir.Node>> save = np => {
        {
            var n = np.val;

            if (affected(n)) {
                np.val = copyExpr(n, n.Type(), _addr_early);
            }

        }
    };

    ref ir.Nodes late = ref heap(out ptr<ir.Nodes> _addr_late);
    foreach (var (i, lorig) in nl) {
        var l = lorig;
        ref var r = ref heap(nr[i], out ptr<var> _addr_r); 

        // Do not generate 'x = x' during return. See issue 4014.
        if (op == ir.ORETURN && ir.SameSafeExpr(l, r)) {
            continue;
        }
        while (true) { 
            // If an expression has init statements, they must be evaluated
            // before any of its saved sub-operands (#45706).
            // TODO(mdempsky): Disallow init statements on lvalues.
            var init = ir.TakeInit(l);
            walkStmtList(init);
            early.Append(init);

            switch (l.type()) {
                case ptr<ir.IndexExpr> ll:
                    if (ll.X.Type().IsArray()) {
                        save(_addr_ll.Index);
                        l = ll.X;
                        continue;
                    }
                    break;
                case ptr<ir.ParenExpr> ll:
                    l = ll.X;
                    continue;
                    break;
                case ptr<ir.SelectorExpr> ll:
                    if (ll.Op() == ir.ODOT) {
                        l = ll.X;
                        continue;
                    }
                    break;
            }
            break;
        }

        ptr<ir.Name> name;

        if (l.Op() == ir.ONAME) 
            name = l._<ptr<ir.Name>>();
        else if (l.Op() == ir.OINDEX || l.Op() == ir.OINDEXMAP) 
            l = l._<ptr<ir.IndexExpr>>();
            save(_addr_l.X);
            save(_addr_l.Index);
        else if (l.Op() == ir.ODEREF) 
            l = l._<ptr<ir.StarExpr>>();
            save(_addr_l.X);
        else if (l.Op() == ir.ODOTPTR) 
            l = l._<ptr<ir.SelectorExpr>>();
            save(_addr_l.X);
        else 
            @base.Fatalf("unexpected lvalue %v", l.Op());
        // Save expression on right side.
        save(_addr_r);

        appendWalkStmt(_addr_late, convas(ir.NewAssignStmt(@base.Pos, lorig, r), _addr_late)); 

        // Check for reasons why we may need to compute later expressions
        // before this assignment happens.

        if (name == null) { 
            // Not a direct assignment to a declared variable.
            // Conservatively assume any memory access might alias.
            memWrite = true;
            continue;
        }
        if (name.Class == ir.PPARAMOUT && ir.CurFunc.HasDefer()) { 
            // Assignments to a result parameter in a function with defers
            // becomes visible early if evaluation of any later expression
            // panics (#43835).
            deferResultWrite = true;
            continue;
        }
        {
            var sym = types.OrigSym(name.Sym());

            if (sym == null || sym.IsBlank()) { 
                // We can ignore assignments to blank or anonymous result parameters.
                // These can't appear in expressions anyway.
                continue;
            }

        }

        if (name.Addrtaken() || !name.OnStack()) { 
            // Global variable, heap escaped, or just addrtaken.
            // Conservatively assume any memory access might alias.
            memWrite = true;
            continue;
        }
        assigned.Add(name);
    }    early.Append(late.Take());
    return early;
}

// readsMemory reports whether the evaluation n directly reads from
// memory that might be written to indirectly.
private static bool readsMemory(ir.Node n) {

    if (n.Op() == ir.ONAME) 
        ptr<ir.Name> n = n._<ptr<ir.Name>>();
        if (n.Class == ir.PFUNC) {
            return false;
        }
        return n.Addrtaken() || !n.OnStack();
    else if (n.Op() == ir.OADD || n.Op() == ir.OAND || n.Op() == ir.OANDAND || n.Op() == ir.OANDNOT || n.Op() == ir.OBITNOT || n.Op() == ir.OCONV || n.Op() == ir.OCONVIFACE || n.Op() == ir.OCONVNOP || n.Op() == ir.ODIV || n.Op() == ir.ODOT || n.Op() == ir.ODOTTYPE || n.Op() == ir.OLITERAL || n.Op() == ir.OLSH || n.Op() == ir.OMOD || n.Op() == ir.OMUL || n.Op() == ir.ONEG || n.Op() == ir.ONIL || n.Op() == ir.OOR || n.Op() == ir.OOROR || n.Op() == ir.OPAREN || n.Op() == ir.OPLUS || n.Op() == ir.ORSH || n.Op() == ir.OSUB || n.Op() == ir.OXOR) 
        return false;
    // Be conservative.
    return true;
}

// expand append(l1, l2...) to
//   init {
//     s := l1
//     n := len(s) + len(l2)
//     // Compare as uint so growslice can panic on overflow.
//     if uint(n) > uint(cap(s)) {
//       s = growslice(s, n)
//     }
//     s = s[:n]
//     memmove(&s[len(l1)], &l2[0], len(l2)*sizeof(T))
//   }
//   s
//
// l2 is allowed to be a string.
private static ir.Node appendSlice(ptr<ir.CallExpr> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.CallExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    walkAppendArgs(n, init);

    var l1 = n.Args[0];
    var l2 = n.Args[1];
    l2 = cheapExpr(l2, init);
    n.Args[1] = l2;

    ref ir.Nodes nodes = ref heap(out ptr<ir.Nodes> _addr_nodes); 

    // var s []T
    var s = typecheck.Temp(l1.Type());
    nodes.Append(ir.NewAssignStmt(@base.Pos, s, l1)); // s = l1

    var elemtype = s.Type().Elem(); 

    // n := len(s) + len(l2)
    var nn = typecheck.Temp(types.Types[types.TINT]);
    nodes.Append(ir.NewAssignStmt(@base.Pos, nn, ir.NewBinaryExpr(@base.Pos, ir.OADD, ir.NewUnaryExpr(@base.Pos, ir.OLEN, s), ir.NewUnaryExpr(@base.Pos, ir.OLEN, l2)))); 

    // if uint(n) > uint(cap(s))
    var nif = ir.NewIfStmt(@base.Pos, null, null, null);
    var nuint = typecheck.Conv(nn, types.Types[types.TUINT]);
    var scapuint = typecheck.Conv(ir.NewUnaryExpr(@base.Pos, ir.OCAP, s), types.Types[types.TUINT]);
    nif.Cond = ir.NewBinaryExpr(@base.Pos, ir.OGT, nuint, scapuint); 

    // instantiate growslice(typ *type, []any, int) []any
    var fn = typecheck.LookupRuntime("growslice");
    fn = typecheck.SubstArgTypes(fn, elemtype, elemtype); 

    // s = growslice(T, s, n)
    nif.Body = new slice<ir.Node>(new ir.Node[] { ir.NewAssignStmt(base.Pos,s,mkcall1(fn,s.Type(),nif.PtrInit(),reflectdata.TypePtr(elemtype),s,nn)) });
    nodes.Append(nif); 

    // s = s[:n]
    var nt = ir.NewSliceExpr(@base.Pos, ir.OSLICE, s, null, nn, null);
    nt.SetBounded(true);
    nodes.Append(ir.NewAssignStmt(@base.Pos, s, nt));

    ir.Node ncopy = default;
    if (elemtype.HasPointers()) { 
        // copy(s[len(l1):], l2)
        var slice = ir.NewSliceExpr(@base.Pos, ir.OSLICE, s, ir.NewUnaryExpr(@base.Pos, ir.OLEN, l1), null, null);
        slice.SetType(s.Type());

        ir.CurFunc.SetWBPos(n.Pos()); 

        // instantiate typedslicecopy(typ *type, dstPtr *any, dstLen int, srcPtr *any, srcLen int) int
        fn = typecheck.LookupRuntime("typedslicecopy");
        fn = typecheck.SubstArgTypes(fn, l1.Type().Elem(), l2.Type().Elem());
        var (ptr1, len1) = backingArrayPtrLen(cheapExpr(slice, _addr_nodes));
        var (ptr2, len2) = backingArrayPtrLen(l2);
        ncopy = mkcall1(fn, types.Types[types.TINT], _addr_nodes, reflectdata.TypePtr(elemtype), ptr1, len1, ptr2, len2);
    }
    else if (@base.Flag.Cfg.Instrumenting && !@base.Flag.CompilingRuntime) { 
        // rely on runtime to instrument:
        //  copy(s[len(l1):], l2)
        // l2 can be a slice or string.
        slice = ir.NewSliceExpr(@base.Pos, ir.OSLICE, s, ir.NewUnaryExpr(@base.Pos, ir.OLEN, l1), null, null);
        slice.SetType(s.Type());

        (ptr1, len1) = backingArrayPtrLen(cheapExpr(slice, _addr_nodes));
        (ptr2, len2) = backingArrayPtrLen(l2);

        fn = typecheck.LookupRuntime("slicecopy");
        fn = typecheck.SubstArgTypes(fn, ptr1.Type().Elem(), ptr2.Type().Elem());
        ncopy = mkcall1(fn, types.Types[types.TINT], _addr_nodes, ptr1, len1, ptr2, len2, ir.NewInt(elemtype.Width));
    }
    else
 { 
        // memmove(&s[len(l1)], &l2[0], len(l2)*sizeof(T))
        var ix = ir.NewIndexExpr(@base.Pos, s, ir.NewUnaryExpr(@base.Pos, ir.OLEN, l1));
        ix.SetBounded(true);
        var addr = typecheck.NodAddr(ix);

        var sptr = ir.NewUnaryExpr(@base.Pos, ir.OSPTR, l2);

        var nwid = cheapExpr(typecheck.Conv(ir.NewUnaryExpr(@base.Pos, ir.OLEN, l2), types.Types[types.TUINTPTR]), _addr_nodes);
        nwid = ir.NewBinaryExpr(@base.Pos, ir.OMUL, nwid, ir.NewInt(elemtype.Width)); 

        // instantiate func memmove(to *any, frm *any, length uintptr)
        fn = typecheck.LookupRuntime("memmove");
        fn = typecheck.SubstArgTypes(fn, elemtype, elemtype);
        ncopy = mkcall1(fn, null, _addr_nodes, addr, sptr, nwid);
    }
    var ln = append(nodes, ncopy);

    typecheck.Stmts(ln);
    walkStmtList(ln);
    init.Append(ln);
    return s;
}

// isAppendOfMake reports whether n is of the form append(x, make([]T, y)...).
// isAppendOfMake assumes n has already been typechecked.
private static bool isAppendOfMake(ir.Node n) {
    if (@base.Flag.N != 0 || @base.Flag.Cfg.Instrumenting) {
        return false;
    }
    if (n.Typecheck() == 0) {
        @base.Fatalf("missing typecheck: %+v", n);
    }
    if (n.Op() != ir.OAPPEND) {
        return false;
    }
    ptr<ir.CallExpr> call = n._<ptr<ir.CallExpr>>();
    if (!call.IsDDD || len(call.Args) != 2 || call.Args[1].Op() != ir.OMAKESLICE) {
        return false;
    }
    ptr<ir.MakeExpr> mk = call.Args[1]._<ptr<ir.MakeExpr>>();
    if (mk.Cap != null) {
        return false;
    }
    var y = mk.Len;
    if (!ir.IsConst(y, constant.Int) && y.Type().Size() > types.Types[types.TUINT].Size()) {
        return false;
    }
    return true;
}

// extendSlice rewrites append(l1, make([]T, l2)...) to
//   init {
//     if l2 >= 0 { // Empty if block here for more meaningful node.SetLikely(true)
//     } else {
//       panicmakeslicelen()
//     }
//     s := l1
//     n := len(s) + l2
//     // Compare n and s as uint so growslice can panic on overflow of len(s) + l2.
//     // cap is a positive int and n can become negative when len(s) + l2
//     // overflows int. Interpreting n when negative as uint makes it larger
//     // than cap(s). growslice will check the int n arg and panic if n is
//     // negative. This prevents the overflow from being undetected.
//     if uint(n) > uint(cap(s)) {
//       s = growslice(T, s, n)
//     }
//     s = s[:n]
//     lptr := &l1[0]
//     sptr := &s[0]
//     if lptr == sptr || !T.HasPointers() {
//       // growslice did not clear the whole underlying array (or did not get called)
//       hp := &s[len(l1)]
//       hn := l2 * sizeof(T)
//       memclr(hp, hn)
//     }
//   }
//   s
private static ir.Node extendSlice(ptr<ir.CallExpr> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.CallExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;
 
    // isAppendOfMake made sure all possible positive values of l2 fit into an uint.
    // The case of l2 overflow when converting from e.g. uint to int is handled by an explicit
    // check of l2 < 0 at runtime which is generated below.
    var l2 = typecheck.Conv(n.Args[1]._<ptr<ir.MakeExpr>>().Len, types.Types[types.TINT]);
    l2 = typecheck.Expr(l2);
    n.Args[1] = l2; // walkAppendArgs expects l2 in n.List.Second().

    walkAppendArgs(n, init);

    var l1 = n.Args[0];
    l2 = n.Args[1]; // re-read l2, as it may have been updated by walkAppendArgs

    slice<ir.Node> nodes = default; 

    // if l2 >= 0 (likely happens), do nothing
    var nifneg = ir.NewIfStmt(@base.Pos, ir.NewBinaryExpr(@base.Pos, ir.OGE, l2, ir.NewInt(0)), null, null);
    nifneg.Likely = true; 

    // else panicmakeslicelen()
    nifneg.Else = new slice<ir.Node>(new ir.Node[] { mkcall("panicmakeslicelen",nil,init) });
    nodes = append(nodes, nifneg); 

    // s := l1
    var s = typecheck.Temp(l1.Type());
    nodes = append(nodes, ir.NewAssignStmt(@base.Pos, s, l1));

    var elemtype = s.Type().Elem(); 

    // n := len(s) + l2
    var nn = typecheck.Temp(types.Types[types.TINT]);
    nodes = append(nodes, ir.NewAssignStmt(@base.Pos, nn, ir.NewBinaryExpr(@base.Pos, ir.OADD, ir.NewUnaryExpr(@base.Pos, ir.OLEN, s), l2))); 

    // if uint(n) > uint(cap(s))
    var nuint = typecheck.Conv(nn, types.Types[types.TUINT]);
    var capuint = typecheck.Conv(ir.NewUnaryExpr(@base.Pos, ir.OCAP, s), types.Types[types.TUINT]);
    var nif = ir.NewIfStmt(@base.Pos, ir.NewBinaryExpr(@base.Pos, ir.OGT, nuint, capuint), null, null); 

    // instantiate growslice(typ *type, old []any, newcap int) []any
    var fn = typecheck.LookupRuntime("growslice");
    fn = typecheck.SubstArgTypes(fn, elemtype, elemtype); 

    // s = growslice(T, s, n)
    nif.Body = new slice<ir.Node>(new ir.Node[] { ir.NewAssignStmt(base.Pos,s,mkcall1(fn,s.Type(),nif.PtrInit(),reflectdata.TypePtr(elemtype),s,nn)) });
    nodes = append(nodes, nif); 

    // s = s[:n]
    var nt = ir.NewSliceExpr(@base.Pos, ir.OSLICE, s, null, nn, null);
    nt.SetBounded(true);
    nodes = append(nodes, ir.NewAssignStmt(@base.Pos, s, nt)); 

    // lptr := &l1[0]
    var l1ptr = typecheck.Temp(l1.Type().Elem().PtrTo());
    var tmp = ir.NewUnaryExpr(@base.Pos, ir.OSPTR, l1);
    nodes = append(nodes, ir.NewAssignStmt(@base.Pos, l1ptr, tmp)); 

    // sptr := &s[0]
    var sptr = typecheck.Temp(elemtype.PtrTo());
    tmp = ir.NewUnaryExpr(@base.Pos, ir.OSPTR, s);
    nodes = append(nodes, ir.NewAssignStmt(@base.Pos, sptr, tmp)); 

    // hp := &s[len(l1)]
    var ix = ir.NewIndexExpr(@base.Pos, s, ir.NewUnaryExpr(@base.Pos, ir.OLEN, l1));
    ix.SetBounded(true);
    var hp = typecheck.ConvNop(typecheck.NodAddr(ix), types.Types[types.TUNSAFEPTR]); 

    // hn := l2 * sizeof(elem(s))
    var hn = typecheck.Conv(ir.NewBinaryExpr(@base.Pos, ir.OMUL, l2, ir.NewInt(elemtype.Width)), types.Types[types.TUINTPTR]);

    @string clrname = "memclrNoHeapPointers";
    var hasPointers = elemtype.HasPointers();
    if (hasPointers) {
        clrname = "memclrHasPointers";
        ir.CurFunc.SetWBPos(n.Pos());
    }
    ref ir.Nodes clr = ref heap(out ptr<ir.Nodes> _addr_clr);
    var clrfn = mkcall(clrname, null, _addr_clr, hp, hn);
    clr.Append(clrfn);

    if (hasPointers) { 
        // if l1ptr == sptr
        var nifclr = ir.NewIfStmt(@base.Pos, ir.NewBinaryExpr(@base.Pos, ir.OEQ, l1ptr, sptr), null, null);
        nifclr.Body = clr;
        nodes = append(nodes, nifclr);
    }
    else
 {
        nodes = append(nodes, clr);
    }
    typecheck.Stmts(nodes);
    walkStmtList(nodes);
    init.Append(nodes);
    return s;
}

} // end walk_package
