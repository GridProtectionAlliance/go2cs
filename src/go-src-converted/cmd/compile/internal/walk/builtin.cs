// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package walk -- go2cs converted at 2022 March 13 06:24:53 UTC
// import "cmd/compile/internal/walk" ==> using walk = go.cmd.compile.@internal.walk_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\walk\builtin.go
namespace go.cmd.compile.@internal;

using fmt = fmt_package;
using constant = go.constant_package;
using token = go.token_package;
using strings = strings_package;

using @base = cmd.compile.@internal.@base_package;
using escape = cmd.compile.@internal.escape_package;
using ir = cmd.compile.@internal.ir_package;
using reflectdata = cmd.compile.@internal.reflectdata_package;
using typecheck = cmd.compile.@internal.typecheck_package;
using types = cmd.compile.@internal.types_package;


// Rewrite append(src, x, y, z) so that any side effects in
// x, y, z (including runtime panics) are evaluated in
// initialization statements before the append.
// For normal code generation, stop there and leave the
// rest to cgen_append.
//
// For race detector, expand append(src, a [, b]* ) to
//
//   init {
//     s := src
//     const argc = len(args) - 1
//     if cap(s) - len(s) < argc {
//        s = growslice(s, len(s)+argc)
//     }
//     n := len(s)
//     s = s[:n+argc]
//     s[n] = a
//     s[n+1] = b
//     ...
//   }
//   s

public static partial class walk_package {

private static ir.Node walkAppend(ptr<ir.CallExpr> _addr_n, ptr<ir.Nodes> _addr_init, ir.Node dst) {
    ref ir.CallExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    if (!ir.SameSafeExpr(dst, n.Args[0])) {
        n.Args[0] = safeExpr(n.Args[0], init);
        n.Args[0] = walkExpr(n.Args[0], init);
    }
    walkExprListSafe(n.Args[(int)1..], init);

    var nsrc = n.Args[0]; 

    // walkExprListSafe will leave OINDEX (s[n]) alone if both s
    // and n are name or literal, but those may index the slice we're
    // modifying here. Fix explicitly.
    // Using cheapExpr also makes sure that the evaluation
    // of all arguments (and especially any panics) happen
    // before we begin to modify the slice in a visible way.
    var ls = n.Args[(int)1..];
    {
        var i__prev1 = i;
        var n__prev1 = n;

        foreach (var (__i, __n) in ls) {
            i = __i;
            n = __n;
            n = cheapExpr(n, init);
            if (!types.Identical(n.Type(), nsrc.Type().Elem())) {
                n = typecheck.AssignConv(n, nsrc.Type().Elem(), "append");
                n = walkExpr(n, init);
            }
            ls[i] = n;
        }
        i = i__prev1;
        n = n__prev1;
    }

    var argc = len(n.Args) - 1;
    if (argc < 1) {
        return nsrc;
    }
    if (!@base.Flag.Cfg.Instrumenting || @base.Flag.CompilingRuntime) {
        return n;
    }
    slice<ir.Node> l = default;

    var ns = typecheck.Temp(nsrc.Type());
    l = append(l, ir.NewAssignStmt(@base.Pos, ns, nsrc)); // s = src

    var na = ir.NewInt(int64(argc)); // const argc
    var nif = ir.NewIfStmt(@base.Pos, null, null, null); // if cap(s) - len(s) < argc
    nif.Cond = ir.NewBinaryExpr(@base.Pos, ir.OLT, ir.NewBinaryExpr(@base.Pos, ir.OSUB, ir.NewUnaryExpr(@base.Pos, ir.OCAP, ns), ir.NewUnaryExpr(@base.Pos, ir.OLEN, ns)), na);

    var fn = typecheck.LookupRuntime("growslice"); //   growslice(<type>, old []T, mincap int) (ret []T)
    fn = typecheck.SubstArgTypes(fn, ns.Type().Elem(), ns.Type().Elem());

    nif.Body = new slice<ir.Node>(new ir.Node[] { ir.NewAssignStmt(base.Pos,ns,mkcall1(fn,ns.Type(),nif.PtrInit(),reflectdata.TypePtr(ns.Type().Elem()),ns,ir.NewBinaryExpr(base.Pos,ir.OADD,ir.NewUnaryExpr(base.Pos,ir.OLEN,ns),na))) });

    l = append(l, nif);

    var nn = typecheck.Temp(types.Types[types.TINT]);
    l = append(l, ir.NewAssignStmt(@base.Pos, nn, ir.NewUnaryExpr(@base.Pos, ir.OLEN, ns))); // n = len(s)

    var slice = ir.NewSliceExpr(@base.Pos, ir.OSLICE, ns, null, ir.NewBinaryExpr(@base.Pos, ir.OADD, nn, na), null); // ...s[:n+argc]
    slice.SetBounded(true);
    l = append(l, ir.NewAssignStmt(@base.Pos, ns, slice)); // s = s[:n+argc]

    ls = n.Args[(int)1..];
    {
        var i__prev1 = i;
        var n__prev1 = n;

        foreach (var (__i, __n) in ls) {
            i = __i;
            n = __n;
            var ix = ir.NewIndexExpr(@base.Pos, ns, nn); // s[n] ...
            ix.SetBounded(true);
            l = append(l, ir.NewAssignStmt(@base.Pos, ix, n)); // s[n] = arg
            if (i + 1 < len(ls)) {
                l = append(l, ir.NewAssignStmt(@base.Pos, nn, ir.NewBinaryExpr(@base.Pos, ir.OADD, nn, ir.NewInt(1)))); // n = n + 1
            }
        }
        i = i__prev1;
        n = n__prev1;
    }

    typecheck.Stmts(l);
    walkStmtList(l);
    init.Append(l);
    return ns;
}

// walkClose walks an OCLOSE node.
private static ir.Node walkClose(ptr<ir.UnaryExpr> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.UnaryExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;
 
    // cannot use chanfn - closechan takes any, not chan any
    var fn = typecheck.LookupRuntime("closechan");
    fn = typecheck.SubstArgTypes(fn, n.X.Type());
    return mkcall1(fn, null, init, n.X);
}

// Lower copy(a, b) to a memmove call or a runtime call.
//
// init {
//   n := len(a)
//   if n > len(b) { n = len(b) }
//   if a.ptr != b.ptr { memmove(a.ptr, b.ptr, n*sizeof(elem(a))) }
// }
// n;
//
// Also works if b is a string.
//
private static ir.Node walkCopy(ptr<ir.BinaryExpr> _addr_n, ptr<ir.Nodes> _addr_init, bool runtimecall) {
    ref ir.BinaryExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    if (n.X.Type().Elem().HasPointers()) {
        ir.CurFunc.SetWBPos(n.Pos());
        var fn = writebarrierfn("typedslicecopy", _addr_n.X.Type().Elem(), _addr_n.Y.Type().Elem());
        n.X = cheapExpr(n.X, init);
        var (ptrL, lenL) = backingArrayPtrLen(n.X);
        n.Y = cheapExpr(n.Y, init);
        var (ptrR, lenR) = backingArrayPtrLen(n.Y);
        return mkcall1(fn, n.Type(), init, reflectdata.TypePtr(n.X.Type().Elem()), ptrL, lenL, ptrR, lenR);
    }
    if (runtimecall) { 
        // rely on runtime to instrument:
        //  copy(n.Left, n.Right)
        // n.Right can be a slice or string.

        n.X = cheapExpr(n.X, init);
        (ptrL, lenL) = backingArrayPtrLen(n.X);
        n.Y = cheapExpr(n.Y, init);
        (ptrR, lenR) = backingArrayPtrLen(n.Y);

        fn = typecheck.LookupRuntime("slicecopy");
        fn = typecheck.SubstArgTypes(fn, ptrL.Type().Elem(), ptrR.Type().Elem());

        return mkcall1(fn, n.Type(), init, ptrL, lenL, ptrR, lenR, ir.NewInt(n.X.Type().Elem().Width));
    }
    n.X = walkExpr(n.X, init);
    n.Y = walkExpr(n.Y, init);
    var nl = typecheck.Temp(n.X.Type());
    var nr = typecheck.Temp(n.Y.Type());
    slice<ir.Node> l = default;
    l = append(l, ir.NewAssignStmt(@base.Pos, nl, n.X));
    l = append(l, ir.NewAssignStmt(@base.Pos, nr, n.Y));

    var nfrm = ir.NewUnaryExpr(@base.Pos, ir.OSPTR, nr);
    var nto = ir.NewUnaryExpr(@base.Pos, ir.OSPTR, nl);

    var nlen = typecheck.Temp(types.Types[types.TINT]); 

    // n = len(to)
    l = append(l, ir.NewAssignStmt(@base.Pos, nlen, ir.NewUnaryExpr(@base.Pos, ir.OLEN, nl))); 

    // if n > len(frm) { n = len(frm) }
    var nif = ir.NewIfStmt(@base.Pos, null, null, null);

    nif.Cond = ir.NewBinaryExpr(@base.Pos, ir.OGT, nlen, ir.NewUnaryExpr(@base.Pos, ir.OLEN, nr));
    nif.Body.Append(ir.NewAssignStmt(@base.Pos, nlen, ir.NewUnaryExpr(@base.Pos, ir.OLEN, nr)));
    l = append(l, nif); 

    // if to.ptr != frm.ptr { memmove( ... ) }
    var ne = ir.NewIfStmt(@base.Pos, ir.NewBinaryExpr(@base.Pos, ir.ONE, nto, nfrm), null, null);
    ne.Likely = true;
    l = append(l, ne);

    fn = typecheck.LookupRuntime("memmove");
    fn = typecheck.SubstArgTypes(fn, nl.Type().Elem(), nl.Type().Elem());
    var nwid = ir.Node(typecheck.Temp(types.Types[types.TUINTPTR]));
    var setwid = ir.NewAssignStmt(@base.Pos, nwid, typecheck.Conv(nlen, types.Types[types.TUINTPTR]));
    ne.Body.Append(setwid);
    nwid = ir.NewBinaryExpr(@base.Pos, ir.OMUL, nwid, ir.NewInt(nl.Type().Elem().Width));
    var call = mkcall1(fn, null, init, nto, nfrm, nwid);
    ne.Body.Append(call);

    typecheck.Stmts(l);
    walkStmtList(l);
    init.Append(l);
    return nlen;
}

// walkDelete walks an ODELETE node.
private static ir.Node walkDelete(ptr<ir.Nodes> _addr_init, ptr<ir.CallExpr> _addr_n) {
    ref ir.Nodes init = ref _addr_init.val;
    ref ir.CallExpr n = ref _addr_n.val;

    init.Append(ir.TakeInit(n));
    var map_ = n.Args[0];
    var key = n.Args[1];
    map_ = walkExpr(map_, init);
    key = walkExpr(key, init);

    var t = map_.Type();
    var fast = mapfast(t);
    key = mapKeyArg(fast, n, key);
    return mkcall1(mapfndel(mapdelete[fast], t), null, init, reflectdata.TypePtr(t), map_, key);
}

// walkLenCap walks an OLEN or OCAP node.
private static ir.Node walkLenCap(ptr<ir.UnaryExpr> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.UnaryExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    if (isRuneCount(n)) { 
        // Replace len([]rune(string)) with runtime.countrunes(string).
        return mkcall("countrunes", n.Type(), init, typecheck.Conv(n.X._<ptr<ir.ConvExpr>>().X, types.Types[types.TSTRING]));
    }
    n.X = walkExpr(n.X, init); 

    // replace len(*[10]int) with 10.
    // delayed until now to preserve side effects.
    var t = n.X.Type();

    if (t.IsPtr()) {
        t = t.Elem();
    }
    if (t.IsArray()) {
        safeExpr(n.X, init);
        var con = typecheck.OrigInt(n, t.NumElem());
        con.SetTypecheck(1);
        return con;
    }
    return n;
}

// walkMakeChan walks an OMAKECHAN node.
private static ir.Node walkMakeChan(ptr<ir.MakeExpr> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.MakeExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;
 
    // When size fits into int, use makechan instead of
    // makechan64, which is faster and shorter on 32 bit platforms.
    var size = n.Len;
    @string fnname = "makechan64";
    var argtype = types.Types[types.TINT64]; 

    // Type checking guarantees that TIDEAL size is positive and fits in an int.
    // The case of size overflow when converting TUINT or TUINTPTR to TINT
    // will be handled by the negative range checks in makechan during runtime.
    if (size.Type().IsKind(types.TIDEAL) || size.Type().Size() <= types.Types[types.TUINT].Size()) {
        fnname = "makechan";
        argtype = types.Types[types.TINT];
    }
    return mkcall1(chanfn(fnname, 1, n.Type()), n.Type(), init, reflectdata.TypePtr(n.Type()), typecheck.Conv(size, argtype));
}

// walkMakeMap walks an OMAKEMAP node.
private static ir.Node walkMakeMap(ptr<ir.MakeExpr> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.MakeExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    var t = n.Type();
    var hmapType = reflectdata.MapType(t);
    var hint = n.Len; 

    // var h *hmap
    ir.Node h = default;
    if (n.Esc() == ir.EscNone) { 
        // Allocate hmap on stack.

        // var hv hmap
        // h = &hv
        h = stackTempAddr(init, hmapType); 

        // Allocate one bucket pointed to by hmap.buckets on stack if hint
        // is not larger than BUCKETSIZE. In case hint is larger than
        // BUCKETSIZE runtime.makemap will allocate the buckets on the heap.
        // Maximum key and elem size is 128 bytes, larger objects
        // are stored with an indirection. So max bucket size is 2048+eps.
        if (!ir.IsConst(hint, constant.Int) || constant.Compare(hint.Val(), token.LEQ, constant.MakeInt64(reflectdata.BUCKETSIZE))) {
            // In case hint is larger than BUCKETSIZE runtime.makemap
            // will allocate the buckets on the heap, see #20184
            //
            // if hint <= BUCKETSIZE {
            //     var bv bmap
            //     b = &bv
            //     h.buckets = b
            // }

            var nif = ir.NewIfStmt(@base.Pos, ir.NewBinaryExpr(@base.Pos, ir.OLE, hint, ir.NewInt(reflectdata.BUCKETSIZE)), null, null);
            nif.Likely = true; 

            // var bv bmap
            // b = &bv
            var b = stackTempAddr(_addr_nif.Body, reflectdata.MapBucketType(t)); 

            // h.buckets = b
            var bsym = hmapType.Field(5).Sym; // hmap.buckets see reflect.go:hmap
            var na = ir.NewAssignStmt(@base.Pos, ir.NewSelectorExpr(@base.Pos, ir.ODOT, h, bsym), b);
            nif.Body.Append(na);
            appendWalkStmt(init, nif);
        }
    }
    if (ir.IsConst(hint, constant.Int) && constant.Compare(hint.Val(), token.LEQ, constant.MakeInt64(reflectdata.BUCKETSIZE))) { 
        // Handling make(map[any]any) and
        // make(map[any]any, hint) where hint <= BUCKETSIZE
        // special allows for faster map initialization and
        // improves binary size by using calls with fewer arguments.
        // For hint <= BUCKETSIZE overLoadFactor(hint, 0) is false
        // and no buckets will be allocated by makemap. Therefore,
        // no buckets need to be allocated in this code path.
        if (n.Esc() == ir.EscNone) { 
            // Only need to initialize h.hash0 since
            // hmap h has been allocated on the stack already.
            // h.hash0 = fastrand()
            var rand = mkcall("fastrand", types.Types[types.TUINT32], init);
            var hashsym = hmapType.Field(4).Sym; // hmap.hash0 see reflect.go:hmap
            appendWalkStmt(init, ir.NewAssignStmt(@base.Pos, ir.NewSelectorExpr(@base.Pos, ir.ODOT, h, hashsym), rand));
            return typecheck.ConvNop(h, t);
        }
        var fn = typecheck.LookupRuntime("makemap_small");
        fn = typecheck.SubstArgTypes(fn, t.Key(), t.Elem());
        return mkcall1(fn, n.Type(), init);
    }
    if (n.Esc() != ir.EscNone) {
        h = typecheck.NodNil();
    }
    @string fnname = "makemap64";
    var argtype = types.Types[types.TINT64]; 

    // Type checking guarantees that TIDEAL hint is positive and fits in an int.
    // See checkmake call in TMAP case of OMAKE case in OpSwitch in typecheck1 function.
    // The case of hint overflow when converting TUINT or TUINTPTR to TINT
    // will be handled by the negative range checks in makemap during runtime.
    if (hint.Type().IsKind(types.TIDEAL) || hint.Type().Size() <= types.Types[types.TUINT].Size()) {
        fnname = "makemap";
        argtype = types.Types[types.TINT];
    }
    fn = typecheck.LookupRuntime(fnname);
    fn = typecheck.SubstArgTypes(fn, hmapType, t.Key(), t.Elem());
    return mkcall1(fn, n.Type(), init, reflectdata.TypePtr(n.Type()), typecheck.Conv(hint, argtype), h);
}

// walkMakeSlice walks an OMAKESLICE node.
private static ir.Node walkMakeSlice(ptr<ir.MakeExpr> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.MakeExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    var l = n.Len;
    var r = n.Cap;
    if (r == null) {
        r = safeExpr(l, init);
        l = r;
    }
    var t = n.Type();
    if (t.Elem().NotInHeap()) {
        @base.Errorf("%v can't be allocated in Go; it is incomplete (or unallocatable)", t.Elem());
    }
    if (n.Esc() == ir.EscNone) {
        {
            var why = escape.HeapAllocReason(n);

            if (why != "") {
                @base.Fatalf("%v has EscNone, but %v", n, why);
            } 
            // var arr [r]T
            // n = arr[:l]

        } 
        // var arr [r]T
        // n = arr[:l]
        var i = typecheck.IndexConst(r);
        if (i < 0) {
            @base.Fatalf("walkExpr: invalid index %v", r);
        }
        var nif = ir.NewIfStmt(@base.Pos, ir.NewBinaryExpr(@base.Pos, ir.OGT, typecheck.Conv(l, types.Types[types.TUINT64]), ir.NewInt(i)), null, null);
        var niflen = ir.NewIfStmt(@base.Pos, ir.NewBinaryExpr(@base.Pos, ir.OLT, l, ir.NewInt(0)), null, null);
        niflen.Body = new slice<ir.Node>(new ir.Node[] { mkcall("panicmakeslicelen",nil,init) });
        nif.Body.Append(niflen, mkcall("panicmakeslicecap", null, init));
        init.Append(typecheck.Stmt(nif));

        t = types.NewArray(t.Elem(), i); // [r]T
        var var_ = typecheck.Temp(t);
        appendWalkStmt(init, ir.NewAssignStmt(@base.Pos, var_, null)); // zero temp
        r = ir.NewSliceExpr(@base.Pos, ir.OSLICE, var_, null, l, null); // arr[:l]
        // The conv is necessary in case n.Type is named.
        return walkExpr(typecheck.Expr(typecheck.Conv(r, n.Type())), init);
    }
    var len = l;
    var cap = r;

    @string fnname = "makeslice64";
    var argtype = types.Types[types.TINT64]; 

    // Type checking guarantees that TIDEAL len/cap are positive and fit in an int.
    // The case of len or cap overflow when converting TUINT or TUINTPTR to TINT
    // will be handled by the negative range checks in makeslice during runtime.
    if ((len.Type().IsKind(types.TIDEAL) || len.Type().Size() <= types.Types[types.TUINT].Size()) && (cap.Type().IsKind(types.TIDEAL) || cap.Type().Size() <= types.Types[types.TUINT].Size())) {
        fnname = "makeslice";
        argtype = types.Types[types.TINT];
    }
    var fn = typecheck.LookupRuntime(fnname);
    var ptr = mkcall1(fn, types.Types[types.TUNSAFEPTR], init, reflectdata.TypePtr(t.Elem()), typecheck.Conv(len, argtype), typecheck.Conv(cap, argtype));
    ptr.MarkNonNil();
    len = typecheck.Conv(len, types.Types[types.TINT]);
    cap = typecheck.Conv(cap, types.Types[types.TINT]);
    var sh = ir.NewSliceHeaderExpr(@base.Pos, t, ptr, len, cap);
    return walkExpr(typecheck.Expr(sh), init);
}

// walkMakeSliceCopy walks an OMAKESLICECOPY node.
private static ir.Node walkMakeSliceCopy(ptr<ir.MakeExpr> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.MakeExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    if (n.Esc() == ir.EscNone) {
        @base.Fatalf("OMAKESLICECOPY with EscNone: %v", n);
    }
    var t = n.Type();
    if (t.Elem().NotInHeap()) {
        @base.Errorf("%v can't be allocated in Go; it is incomplete (or unallocatable)", t.Elem());
    }
    var length = typecheck.Conv(n.Len, types.Types[types.TINT]);
    var copylen = ir.NewUnaryExpr(@base.Pos, ir.OLEN, n.Cap);
    var copyptr = ir.NewUnaryExpr(@base.Pos, ir.OSPTR, n.Cap);

    if (!t.Elem().HasPointers() && n.Bounded()) { 
        // When len(to)==len(from) and elements have no pointers:
        // replace make+copy with runtime.mallocgc+runtime.memmove.

        // We do not check for overflow of len(to)*elem.Width here
        // since len(from) is an existing checked slice capacity
        // with same elem.Width for the from slice.
        var size = ir.NewBinaryExpr(@base.Pos, ir.OMUL, typecheck.Conv(length, types.Types[types.TUINTPTR]), typecheck.Conv(ir.NewInt(t.Elem().Width), types.Types[types.TUINTPTR])); 

        // instantiate mallocgc(size uintptr, typ *byte, needszero bool) unsafe.Pointer
        var fn = typecheck.LookupRuntime("mallocgc");
        var ptr = mkcall1(fn, types.Types[types.TUNSAFEPTR], init, size, typecheck.NodNil(), ir.NewBool(false));
        ptr.MarkNonNil();
        var sh = ir.NewSliceHeaderExpr(@base.Pos, t, ptr, length, length);

        var s = typecheck.Temp(t);
        var r = typecheck.Stmt(ir.NewAssignStmt(@base.Pos, s, sh));
        r = walkExpr(r, init);
        init.Append(r); 

        // instantiate memmove(to *any, frm *any, size uintptr)
        fn = typecheck.LookupRuntime("memmove");
        fn = typecheck.SubstArgTypes(fn, t.Elem(), t.Elem());
        var ncopy = mkcall1(fn, null, init, ir.NewUnaryExpr(@base.Pos, ir.OSPTR, s), copyptr, size);
        init.Append(walkExpr(typecheck.Stmt(ncopy), init));

        return s;
    }
    fn = typecheck.LookupRuntime("makeslicecopy");
    ptr = mkcall1(fn, types.Types[types.TUNSAFEPTR], init, reflectdata.TypePtr(t.Elem()), length, copylen, typecheck.Conv(copyptr, types.Types[types.TUNSAFEPTR]));
    ptr.MarkNonNil();
    sh = ir.NewSliceHeaderExpr(@base.Pos, t, ptr, length, length);
    return walkExpr(typecheck.Expr(sh), init);
}

// walkNew walks an ONEW node.
private static ir.Node walkNew(ptr<ir.UnaryExpr> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.UnaryExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    var t = n.Type().Elem();
    if (t.NotInHeap()) {
        @base.Errorf("%v can't be allocated in Go; it is incomplete (or unallocatable)", n.Type().Elem());
    }
    if (n.Esc() == ir.EscNone) {
        if (t.Size() > ir.MaxImplicitStackVarSize) {
            @base.Fatalf("large ONEW with EscNone: %v", n);
        }
        return stackTempAddr(init, t);
    }
    types.CalcSize(t);
    n.MarkNonNil();
    return n;
}

// generate code for print
private static ir.Node walkPrint(ptr<ir.CallExpr> _addr_nn, ptr<ir.Nodes> _addr_init) {
    ref ir.CallExpr nn = ref _addr_nn.val;
    ref ir.Nodes init = ref _addr_init.val;
 
    // Hoist all the argument evaluation up before the lock.
    walkExprListCheap(nn.Args, init); 

    // For println, add " " between elements and "\n" at the end.
    if (nn.Op() == ir.OPRINTN) {
        var s = nn.Args;
        var t = make_slice<ir.Node>(0, len(s) * 2);
        {
            var i__prev1 = i;
            var n__prev1 = n;

            foreach (var (__i, __n) in s) {
                i = __i;
                n = __n;
                if (i != 0) {
                    t = append(t, ir.NewString(" "));
                }
                t = append(t, n);
            }

            i = i__prev1;
            n = n__prev1;
        }

        t = append(t, ir.NewString("\n"));
        nn.Args = t;
    }
    s = nn.Args;
    t = make_slice<ir.Node>(0, len(s));
    {
        var i__prev1 = i;

        nint i = 0;

        while (i < len(s)) {
            slice<@string> strs = default;
            while (i < len(s) && ir.IsConst(s[i], constant.String)) {
                strs = append(strs, ir.StringVal(s[i]));
                i++;
            }

            if (len(strs) > 0) {
                t = append(t, ir.NewString(strings.Join(strs, "")));
            }
            if (i < len(s)) {
                t = append(t, s[i]);
                i++;
            }
        }

        i = i__prev1;
    }
    nn.Args = t;

    ir.Node calls = new slice<ir.Node>(new ir.Node[] { mkcall("printlock",nil,init) });
    {
        var i__prev1 = i;
        var n__prev1 = n;

        foreach (var (__i, __n) in nn.Args) {
            i = __i;
            n = __n;
            if (n.Op() == ir.OLITERAL) {
                if (n.Type() == types.UntypedRune) {
                    n = typecheck.DefaultLit(n, types.RuneType);
                }

                if (n.Val().Kind() == constant.Int) 
                    n = typecheck.DefaultLit(n, types.Types[types.TINT64]);
                else if (n.Val().Kind() == constant.Float) 
                    n = typecheck.DefaultLit(n, types.Types[types.TFLOAT64]);
                            }
            if (n.Op() != ir.OLITERAL && n.Type() != null && n.Type().Kind() == types.TIDEAL) {
                n = typecheck.DefaultLit(n, types.Types[types.TINT64]);
            }
            n = typecheck.DefaultLit(n, null);
            nn.Args[i] = n;
            if (n.Type() == null || n.Type().Kind() == types.TFORW) {
                continue;
            }
            ptr<ir.Name> on;

            if (n.Type().Kind() == types.TINTER)
            {
                if (n.Type().IsEmptyInterface()) {
                    on = typecheck.LookupRuntime("printeface");
                }
                else
 {
                    on = typecheck.LookupRuntime("printiface");
                }
                on = typecheck.SubstArgTypes(on, n.Type()); // any-1
                goto __switch_break0;
            }
            if (n.Type().Kind() == types.TPTR)
            {
                if (n.Type().Elem().NotInHeap()) {
                    on = typecheck.LookupRuntime("printuintptr");
                    n = ir.NewConvExpr(@base.Pos, ir.OCONV, null, n);
                    n.SetType(types.Types[types.TUNSAFEPTR]);
                    n = ir.NewConvExpr(@base.Pos, ir.OCONV, null, n);
                    n.SetType(types.Types[types.TUINTPTR]);
                    break;
                }
                fallthrough = true;
            }
            if (fallthrough || n.Type().Kind() == types.TCHAN || n.Type().Kind() == types.TMAP || n.Type().Kind() == types.TFUNC || n.Type().Kind() == types.TUNSAFEPTR)
            {
                on = typecheck.LookupRuntime("printpointer");
                on = typecheck.SubstArgTypes(on, n.Type()); // any-1
                goto __switch_break0;
            }
            if (n.Type().Kind() == types.TSLICE)
            {
                on = typecheck.LookupRuntime("printslice");
                on = typecheck.SubstArgTypes(on, n.Type()); // any-1
                goto __switch_break0;
            }
            if (n.Type().Kind() == types.TUINT || n.Type().Kind() == types.TUINT8 || n.Type().Kind() == types.TUINT16 || n.Type().Kind() == types.TUINT32 || n.Type().Kind() == types.TUINT64 || n.Type().Kind() == types.TUINTPTR)
            {
                if (types.IsRuntimePkg(n.Type().Sym().Pkg) && n.Type().Sym().Name == "hex") {
                    on = typecheck.LookupRuntime("printhex");
                }
                else
 {
                    on = typecheck.LookupRuntime("printuint");
                }
                goto __switch_break0;
            }
            if (n.Type().Kind() == types.TINT || n.Type().Kind() == types.TINT8 || n.Type().Kind() == types.TINT16 || n.Type().Kind() == types.TINT32 || n.Type().Kind() == types.TINT64)
            {
                on = typecheck.LookupRuntime("printint");
                goto __switch_break0;
            }
            if (n.Type().Kind() == types.TFLOAT32 || n.Type().Kind() == types.TFLOAT64)
            {
                on = typecheck.LookupRuntime("printfloat");
                goto __switch_break0;
            }
            if (n.Type().Kind() == types.TCOMPLEX64 || n.Type().Kind() == types.TCOMPLEX128)
            {
                on = typecheck.LookupRuntime("printcomplex");
                goto __switch_break0;
            }
            if (n.Type().Kind() == types.TBOOL)
            {
                on = typecheck.LookupRuntime("printbool");
                goto __switch_break0;
            }
            if (n.Type().Kind() == types.TSTRING)
            {
                @string cs = "";
                if (ir.IsConst(n, constant.String)) {
                    cs = ir.StringVal(n);
                }
                switch (cs) {
                    case " ": 
                        on = typecheck.LookupRuntime("printsp");
                        break;
                    case "\n": 
                        on = typecheck.LookupRuntime("printnl");
                        break;
                    default: 
                        on = typecheck.LookupRuntime("printstring");
                        break;
                }
                goto __switch_break0;
            }
            // default: 
                badtype(ir.OPRINT, _addr_n.Type(), _addr_null);
                continue;

            __switch_break0:;

            var r = ir.NewCallExpr(@base.Pos, ir.OCALL, on, null);
            {
                var @params = on.Type().Params().FieldSlice();

                if (len(params) > 0) {
                    t = params[0].Type;
                    if (!types.Identical(t, n.Type())) {
                        n = ir.NewConvExpr(@base.Pos, ir.OCONV, null, n);
                        n.SetType(t);
                    }
                    r.Args.Append(n);
                }

            }
            calls = append(calls, r);
        }
        i = i__prev1;
        n = n__prev1;
    }

    calls = append(calls, mkcall("printunlock", null, init));

    typecheck.Stmts(calls);
    walkExprList(calls, init);

    r = ir.NewBlockStmt(@base.Pos, null);
    r.List = calls;
    return walkStmt(typecheck.Stmt(r));
}

// walkRecover walks an ORECOVER node.
private static ir.Node walkRecover(ptr<ir.CallExpr> _addr_nn, ptr<ir.Nodes> _addr_init) {
    ref ir.CallExpr nn = ref _addr_nn.val;
    ref ir.Nodes init = ref _addr_init.val;
 
    // Call gorecover with the FP of this frame.
    // FP is equal to caller's SP plus FixedFrameSize().
    ir.Node fp = mkcall("getcallersp", types.Types[types.TUINTPTR], init);
    {
        var off = @base.Ctxt.FixedFrameSize();

        if (off != 0) {
            fp = ir.NewBinaryExpr(fp.Pos(), ir.OADD, fp, ir.NewInt(off));
        }
    }
    fp = ir.NewConvExpr(fp.Pos(), ir.OCONVNOP, types.NewPtr(types.Types[types.TINT32]), fp);
    return mkcall("gorecover", nn.Type(), init, fp);
}

private static ir.Node walkUnsafeSlice(ptr<ir.BinaryExpr> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.BinaryExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    var ptr = safeExpr(n.X, init);
    var len = safeExpr(n.Y, init);

    @string fnname = "unsafeslice64";
    var lenType = types.Types[types.TINT64]; 

    // Type checking guarantees that TIDEAL len/cap are positive and fit in an int.
    // The case of len or cap overflow when converting TUINT or TUINTPTR to TINT
    // will be handled by the negative range checks in unsafeslice during runtime.
    if (ir.ShouldCheckPtr(ir.CurFunc, 1)) {
        fnname = "unsafeslicecheckptr"; 
        // for simplicity, unsafeslicecheckptr always uses int64
    }
    else if (len.Type().IsKind(types.TIDEAL) || len.Type().Size() <= types.Types[types.TUINT].Size()) {
        fnname = "unsafeslice";
        lenType = types.Types[types.TINT];
    }
    var t = n.Type(); 

    // Call runtime.unsafeslice{,64,checkptr} to check ptr and len.
    var fn = typecheck.LookupRuntime(fnname);
    init.Append(mkcall1(fn, null, init, reflectdata.TypePtr(t.Elem()), typecheck.Conv(ptr, types.Types[types.TUNSAFEPTR]), typecheck.Conv(len, lenType)));

    var h = ir.NewSliceHeaderExpr(n.Pos(), t, typecheck.Conv(ptr, types.Types[types.TUNSAFEPTR]), typecheck.Conv(len, types.Types[types.TINT]), typecheck.Conv(len, types.Types[types.TINT]));
    return walkExpr(typecheck.Expr(h), init);
}

private static void badtype(ir.Op op, ptr<types.Type> _addr_tl, ptr<types.Type> _addr_tr) {
    ref types.Type tl = ref _addr_tl.val;
    ref types.Type tr = ref _addr_tr.val;

    @string s = default;
    if (tl != null) {
        s += fmt.Sprintf("\n\t%v", tl);
    }
    if (tr != null) {
        s += fmt.Sprintf("\n\t%v", tr);
    }
    if (tl != null && tr != null && tl.IsPtr() && tr.IsPtr()) {
        if (tl.Elem().IsStruct() && tr.Elem().IsInterface()) {
            s += "\n\t(*struct vs *interface)";
        }
        else if (tl.Elem().IsInterface() && tr.Elem().IsStruct()) {
            s += "\n\t(*interface vs *struct)";
        }
    }
    @base.Errorf("illegal types for operand: %v%s", op, s);
}

private static ir.Node writebarrierfn(@string name, ptr<types.Type> _addr_l, ptr<types.Type> _addr_r) {
    ref types.Type l = ref _addr_l.val;
    ref types.Type r = ref _addr_r.val;

    var fn = typecheck.LookupRuntime(name);
    fn = typecheck.SubstArgTypes(fn, l, r);
    return fn;
}

// isRuneCount reports whether n is of the form len([]rune(string)).
// These are optimized into a call to runtime.countrunes.
private static bool isRuneCount(ir.Node n) {
    return @base.Flag.N == 0 && !@base.Flag.Cfg.Instrumenting && n.Op() == ir.OLEN && n._<ptr<ir.UnaryExpr>>().X.Op() == ir.OSTR2RUNES;
}

} // end walk_package
