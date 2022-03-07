// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package walk -- go2cs converted at 2022 March 06 23:11:49 UTC
// import "cmd/compile/internal/walk" ==> using walk = go.cmd.compile.@internal.walk_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\walk\expr.go
using fmt = go.fmt_package;
using constant = go.go.constant_package;
using buildcfg = go.@internal.buildcfg_package;
using strings = go.strings_package;

using @base = go.cmd.compile.@internal.@base_package;
using ir = go.cmd.compile.@internal.ir_package;
using reflectdata = go.cmd.compile.@internal.reflectdata_package;
using staticdata = go.cmd.compile.@internal.staticdata_package;
using typecheck = go.cmd.compile.@internal.typecheck_package;
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;

namespace go.cmd.compile.@internal;

public static partial class walk_package {

    // The result of walkExpr MUST be assigned back to n, e.g.
    //     n.Left = walkExpr(n.Left, init)
private static ir.Node walkExpr(ir.Node n, ptr<ir.Nodes> _addr_init) {
    ref ir.Nodes init = ref _addr_init.val;

    if (n == null) {
        return n;
    }
    {
        ir.InitNode n__prev1 = n;

        ir.InitNode (n, ok) = n._<ir.InitNode>();

        if (ok && init == n.PtrInit()) { 
            // not okay to use n->ninit when walking n,
            // because we might replace n with some other node
            // and would lose the init list.
            @base.Fatalf("walkExpr init == &n->ninit");

        }
        n = n__prev1;

    }


    if (len(n.Init()) != 0) {
        walkStmtList(n.Init());
        init.Append(ir.TakeInit(n));
    }
    var lno = ir.SetPos(n);

    if (@base.Flag.LowerW > 1) {
        ir.Dump("before walk expr", n);
    }
    if (n.Typecheck() != 1) {
        @base.Fatalf("missed typecheck: %+v", n);
    }
    if (n.Type().IsUntyped()) {
        @base.Fatalf("expression has untyped type: %+v", n);
    }
    n = walkExpr1(n, _addr_init); 

    // Eagerly compute sizes of all expressions for the back end.
    {
        var typ = n.Type();

        if (typ != null && typ.Kind() != types.TBLANK && !typ.IsFuncArgStruct()) {
            types.CheckSize(typ);
        }
    }

    {
        ir.InitNode n__prev1 = n;

        (n, ok) = n._<ptr<ir.Name>>();

        if (ok && n.Heapaddr != null) {
            types.CheckSize(n.Heapaddr.Type());
        }
        n = n__prev1;

    }

    if (ir.IsConst(n, constant.String)) { 
        // Emit string symbol now to avoid emitting
        // any concurrently during the backend.
        _ = staticdata.StringSym(n.Pos(), constant.StringVal(n.Val()));

    }
    if (@base.Flag.LowerW != 0 && n != null) {
        ir.Dump("after walk expr", n);
    }
    @base.Pos = lno;
    return n;

}

private static ir.Node walkExpr1(ir.Node n, ptr<ir.Nodes> _addr_init) => func((_, panic, _) => {
    ref ir.Nodes init = ref _addr_init.val;


    if (n.Op() == ir.ONONAME || n.Op() == ir.OGETG) 
        return n;
    else if (n.Op() == ir.OTYPE || n.Op() == ir.ONAME || n.Op() == ir.OLITERAL || n.Op() == ir.ONIL || n.Op() == ir.OLINKSYMOFFSET) 
        // TODO(mdempsky): Just return n; see discussion on CL 38655.
        // Perhaps refactor to use Node.mayBeShared for these instead.
        // If these return early, make sure to still call
        // StringSym for constant strings.
        return n;
    else if (n.Op() == ir.OMETHEXPR) 
        // TODO(mdempsky): Do this right after type checking.
        ptr<ir.SelectorExpr> n = n._<ptr<ir.SelectorExpr>>();
        return n.FuncName();
    else if (n.Op() == ir.ONOT || n.Op() == ir.ONEG || n.Op() == ir.OPLUS || n.Op() == ir.OBITNOT || n.Op() == ir.OREAL || n.Op() == ir.OIMAG || n.Op() == ir.OSPTR || n.Op() == ir.OITAB || n.Op() == ir.OIDATA) 
        n = n._<ptr<ir.UnaryExpr>>();
        n.X = walkExpr(n.X, _addr_init);
        return n;
    else if (n.Op() == ir.ODOTMETH || n.Op() == ir.ODOTINTER) 
        n = n._<ptr<ir.SelectorExpr>>();
        n.X = walkExpr(n.X, _addr_init);
        return n;
    else if (n.Op() == ir.OADDR) 
        n = n._<ptr<ir.AddrExpr>>();
        n.X = walkExpr(n.X, _addr_init);
        return n;
    else if (n.Op() == ir.ODEREF) 
        n = n._<ptr<ir.StarExpr>>();
        n.X = walkExpr(n.X, _addr_init);
        return n;
    else if (n.Op() == ir.OEFACE || n.Op() == ir.OAND || n.Op() == ir.OANDNOT || n.Op() == ir.OSUB || n.Op() == ir.OMUL || n.Op() == ir.OADD || n.Op() == ir.OOR || n.Op() == ir.OXOR || n.Op() == ir.OLSH || n.Op() == ir.ORSH || n.Op() == ir.OUNSAFEADD) 
        n = n._<ptr<ir.BinaryExpr>>();
        n.X = walkExpr(n.X, _addr_init);
        n.Y = walkExpr(n.Y, _addr_init);
        return n;
    else if (n.Op() == ir.OUNSAFESLICE) 
        n = n._<ptr<ir.BinaryExpr>>();
        return walkUnsafeSlice(n, init);
    else if (n.Op() == ir.ODOT || n.Op() == ir.ODOTPTR) 
        n = n._<ptr<ir.SelectorExpr>>();
        return walkDot(n, _addr_init);
    else if (n.Op() == ir.ODOTTYPE || n.Op() == ir.ODOTTYPE2) 
        n = n._<ptr<ir.TypeAssertExpr>>();
        return walkDotType(n, _addr_init);
    else if (n.Op() == ir.OLEN || n.Op() == ir.OCAP) 
        n = n._<ptr<ir.UnaryExpr>>();
        return walkLenCap(n, init);
    else if (n.Op() == ir.OCOMPLEX) 
        n = n._<ptr<ir.BinaryExpr>>();
        n.X = walkExpr(n.X, _addr_init);
        n.Y = walkExpr(n.Y, _addr_init);
        return n;
    else if (n.Op() == ir.OEQ || n.Op() == ir.ONE || n.Op() == ir.OLT || n.Op() == ir.OLE || n.Op() == ir.OGT || n.Op() == ir.OGE) 
        n = n._<ptr<ir.BinaryExpr>>();
        return walkCompare(n, init);
    else if (n.Op() == ir.OANDAND || n.Op() == ir.OOROR) 
        n = n._<ptr<ir.LogicalExpr>>();
        return walkLogical(n, _addr_init);
    else if (n.Op() == ir.OPRINT || n.Op() == ir.OPRINTN) 
        return walkPrint(n._<ptr<ir.CallExpr>>(), init);
    else if (n.Op() == ir.OPANIC) 
        n = n._<ptr<ir.UnaryExpr>>();
        return mkcall("gopanic", null, init, n.X);
    else if (n.Op() == ir.ORECOVER) 
        return walkRecover(n._<ptr<ir.CallExpr>>(), init);
    else if (n.Op() == ir.OCFUNC) 
        return n;
    else if (n.Op() == ir.OCALLINTER || n.Op() == ir.OCALLFUNC || n.Op() == ir.OCALLMETH) 
        n = n._<ptr<ir.CallExpr>>();
        return walkCall(n, _addr_init);
    else if (n.Op() == ir.OAS || n.Op() == ir.OASOP) 
        return walkAssign(init, n);
    else if (n.Op() == ir.OAS2) 
        n = n._<ptr<ir.AssignListStmt>>();
        return walkAssignList(init, n); 

        // a,b,... = fn()
    else if (n.Op() == ir.OAS2FUNC) 
        n = n._<ptr<ir.AssignListStmt>>();
        return walkAssignFunc(init, n); 

        // x, y = <-c
        // order.stmt made sure x is addressable or blank.
    else if (n.Op() == ir.OAS2RECV) 
        n = n._<ptr<ir.AssignListStmt>>();
        return walkAssignRecv(init, n); 

        // a,b = m[i]
    else if (n.Op() == ir.OAS2MAPR) 
        n = n._<ptr<ir.AssignListStmt>>();
        return walkAssignMapRead(init, n);
    else if (n.Op() == ir.ODELETE) 
        n = n._<ptr<ir.CallExpr>>();
        return walkDelete(init, n);
    else if (n.Op() == ir.OAS2DOTTYPE) 
        n = n._<ptr<ir.AssignListStmt>>();
        return walkAssignDotType(n, init);
    else if (n.Op() == ir.OCONVIFACE) 
        n = n._<ptr<ir.ConvExpr>>();
        return walkConvInterface(n, init);
    else if (n.Op() == ir.OCONV || n.Op() == ir.OCONVNOP) 
        n = n._<ptr<ir.ConvExpr>>();
        return walkConv(n, init);
    else if (n.Op() == ir.OSLICE2ARRPTR) 
        n = n._<ptr<ir.ConvExpr>>();
        n.X = walkExpr(n.X, _addr_init);
        return n;
    else if (n.Op() == ir.ODIV || n.Op() == ir.OMOD) 
        n = n._<ptr<ir.BinaryExpr>>();
        return walkDivMod(n, _addr_init);
    else if (n.Op() == ir.OINDEX) 
        n = n._<ptr<ir.IndexExpr>>();
        return walkIndex(n, _addr_init);
    else if (n.Op() == ir.OINDEXMAP) 
        n = n._<ptr<ir.IndexExpr>>();
        return walkIndexMap(n, _addr_init);
    else if (n.Op() == ir.ORECV) 
        @base.Fatalf("walkExpr ORECV"); // should see inside OAS only
        panic("unreachable");
    else if (n.Op() == ir.OSLICEHEADER) 
        n = n._<ptr<ir.SliceHeaderExpr>>();
        return walkSliceHeader(n, _addr_init);
    else if (n.Op() == ir.OSLICE || n.Op() == ir.OSLICEARR || n.Op() == ir.OSLICESTR || n.Op() == ir.OSLICE3 || n.Op() == ir.OSLICE3ARR) 
        n = n._<ptr<ir.SliceExpr>>();
        return walkSlice(n, _addr_init);
    else if (n.Op() == ir.ONEW) 
        n = n._<ptr<ir.UnaryExpr>>();
        return walkNew(n, init);
    else if (n.Op() == ir.OADDSTR) 
        return walkAddString(n._<ptr<ir.AddStringExpr>>(), _addr_init);
    else if (n.Op() == ir.OAPPEND) 
        // order should make sure we only see OAS(node, OAPPEND), which we handle above.
        @base.Fatalf("append outside assignment");
        panic("unreachable");
    else if (n.Op() == ir.OCOPY) 
        return walkCopy(n._<ptr<ir.BinaryExpr>>(), init, @base.Flag.Cfg.Instrumenting && !@base.Flag.CompilingRuntime);
    else if (n.Op() == ir.OCLOSE) 
        n = n._<ptr<ir.UnaryExpr>>();
        return walkClose(n, init);
    else if (n.Op() == ir.OMAKECHAN) 
        n = n._<ptr<ir.MakeExpr>>();
        return walkMakeChan(n, init);
    else if (n.Op() == ir.OMAKEMAP) 
        n = n._<ptr<ir.MakeExpr>>();
        return walkMakeMap(n, init);
    else if (n.Op() == ir.OMAKESLICE) 
        n = n._<ptr<ir.MakeExpr>>();
        return walkMakeSlice(n, init);
    else if (n.Op() == ir.OMAKESLICECOPY) 
        n = n._<ptr<ir.MakeExpr>>();
        return walkMakeSliceCopy(n, init);
    else if (n.Op() == ir.ORUNESTR) 
        n = n._<ptr<ir.ConvExpr>>();
        return walkRuneToString(n, init);
    else if (n.Op() == ir.OBYTES2STR || n.Op() == ir.ORUNES2STR) 
        n = n._<ptr<ir.ConvExpr>>();
        return walkBytesRunesToString(n, init);
    else if (n.Op() == ir.OBYTES2STRTMP) 
        n = n._<ptr<ir.ConvExpr>>();
        return walkBytesToStringTemp(n, init);
    else if (n.Op() == ir.OSTR2BYTES) 
        n = n._<ptr<ir.ConvExpr>>();
        return walkStringToBytes(n, init);
    else if (n.Op() == ir.OSTR2BYTESTMP) 
        n = n._<ptr<ir.ConvExpr>>();
        return walkStringToBytesTemp(n, init);
    else if (n.Op() == ir.OSTR2RUNES) 
        n = n._<ptr<ir.ConvExpr>>();
        return walkStringToRunes(n, init);
    else if (n.Op() == ir.OARRAYLIT || n.Op() == ir.OSLICELIT || n.Op() == ir.OMAPLIT || n.Op() == ir.OSTRUCTLIT || n.Op() == ir.OPTRLIT) 
        return walkCompLit(n, init);
    else if (n.Op() == ir.OSEND) 
        n = n._<ptr<ir.SendStmt>>();
        return walkSend(n, _addr_init);
    else if (n.Op() == ir.OCLOSURE) 
        return walkClosure(n._<ptr<ir.ClosureExpr>>(), init);
    else if (n.Op() == ir.OCALLPART) 
        return walkCallPart(n._<ptr<ir.SelectorExpr>>(), init);
    else 
        ir.Dump("walk", n);
        @base.Fatalf("walkExpr: switch 1 unknown op %+v", n.Op());
        panic("unreachable");
    // No return! Each case must return (or panic),
    // to avoid confusion about what gets returned
    // in the presence of type assertions.
});

// walk the whole tree of the body of an
// expression or simple statement.
// the types expressions are calculated.
// compile-time constants are evaluated.
// complex side effects like statements are appended to init
private static void walkExprList(slice<ir.Node> s, ptr<ir.Nodes> _addr_init) {
    ref ir.Nodes init = ref _addr_init.val;

    foreach (var (i) in s) {
        s[i] = walkExpr(s[i], _addr_init);
    }
}

private static void walkExprListCheap(slice<ir.Node> s, ptr<ir.Nodes> _addr_init) {
    ref ir.Nodes init = ref _addr_init.val;

    foreach (var (i, n) in s) {
        s[i] = cheapExpr(n, _addr_init);
        s[i] = walkExpr(s[i], _addr_init);
    }
}

private static void walkExprListSafe(slice<ir.Node> s, ptr<ir.Nodes> _addr_init) {
    ref ir.Nodes init = ref _addr_init.val;

    foreach (var (i, n) in s) {
        s[i] = safeExpr(n, _addr_init);
        s[i] = walkExpr(s[i], _addr_init);
    }
}

// return side-effect free and cheap n, appending side effects to init.
// result may not be assignable.
private static ir.Node cheapExpr(ir.Node n, ptr<ir.Nodes> _addr_init) {
    ref ir.Nodes init = ref _addr_init.val;


    if (n.Op() == ir.ONAME || n.Op() == ir.OLITERAL || n.Op() == ir.ONIL) 
        return n;
        return copyExpr(n, _addr_n.Type(), _addr_init);

}

// return side effect-free n, appending side effects to init.
// result is assignable if n is.
private static ir.Node safeExpr(ir.Node n, ptr<ir.Nodes> _addr_init) {
    ref ir.Nodes init = ref _addr_init.val;

    if (n == null) {
        return null;
    }
    if (len(n.Init()) != 0) {
        walkStmtList(n.Init());
        init.Append(ir.TakeInit(n));
    }

    if (n.Op() == ir.ONAME || n.Op() == ir.OLITERAL || n.Op() == ir.ONIL || n.Op() == ir.OLINKSYMOFFSET) 
        return n;
    else if (n.Op() == ir.OLEN || n.Op() == ir.OCAP) 
        ptr<ir.UnaryExpr> n = n._<ptr<ir.UnaryExpr>>();
        var l = safeExpr(n.X, _addr_init);
        if (l == n.X) {
            return n;
        }
        ptr<ir.UnaryExpr> a = ir.Copy(n)._<ptr<ir.UnaryExpr>>();
        a.X = l;
        return walkExpr(typecheck.Expr(a), _addr_init);
    else if (n.Op() == ir.ODOT || n.Op() == ir.ODOTPTR) 
        n = n._<ptr<ir.SelectorExpr>>();
        l = safeExpr(n.X, _addr_init);
        if (l == n.X) {
            return n;
        }
        a = ir.Copy(n)._<ptr<ir.SelectorExpr>>();
        a.X = l;
        return walkExpr(typecheck.Expr(a), _addr_init);
    else if (n.Op() == ir.ODEREF) 
        n = n._<ptr<ir.StarExpr>>();
        l = safeExpr(n.X, _addr_init);
        if (l == n.X) {
            return n;
        }
        a = ir.Copy(n)._<ptr<ir.StarExpr>>();
        a.X = l;
        return walkExpr(typecheck.Expr(a), _addr_init);
    else if (n.Op() == ir.OINDEX || n.Op() == ir.OINDEXMAP) 
        n = n._<ptr<ir.IndexExpr>>();
        l = safeExpr(n.X, _addr_init);
        var r = safeExpr(n.Index, _addr_init);
        if (l == n.X && r == n.Index) {
            return n;
        }
        a = ir.Copy(n)._<ptr<ir.IndexExpr>>();
        a.X = l;
        a.Index = r;
        return walkExpr(typecheck.Expr(a), _addr_init);
    else if (n.Op() == ir.OSTRUCTLIT || n.Op() == ir.OARRAYLIT || n.Op() == ir.OSLICELIT) 
        n = n._<ptr<ir.CompLitExpr>>();
        if (isStaticCompositeLiteral(n)) {
            return n;
        }
    // make a copy; must not be used as an lvalue
    if (ir.IsAddressable(n)) {
        @base.Fatalf("missing lvalue case in safeExpr: %v", n);
    }
    return cheapExpr(n, _addr_init);

}

private static ir.Node copyExpr(ir.Node n, ptr<types.Type> _addr_t, ptr<ir.Nodes> _addr_init) {
    ref types.Type t = ref _addr_t.val;
    ref ir.Nodes init = ref _addr_init.val;

    var l = typecheck.Temp(t);
    appendWalkStmt(init, ir.NewAssignStmt(@base.Pos, l, n));
    return l;
}

private static ir.Node walkAddString(ptr<ir.AddStringExpr> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.AddStringExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    var c = len(n.List);

    if (c < 2) {
        @base.Fatalf("walkAddString count %d too small", c);
    }
    var buf = typecheck.NodNil();
    if (n.Esc() == ir.EscNone) {
        var sz = int64(0);
        foreach (var (_, n1) in n.List) {
            if (n1.Op() == ir.OLITERAL) {
                sz += int64(len(ir.StringVal(n1)));
            }
        }        if (sz < tmpstringbufsize) { 
            // Create temporary buffer for result string on stack.
            buf = stackBufAddr(tmpstringbufsize, types.Types[types.TUINT8]);

        }
    }
    ir.Node args = new slice<ir.Node>(new ir.Node[] { buf });
    foreach (var (_, n2) in n.List) {
        args = append(args, typecheck.Conv(n2, types.Types[types.TSTRING]));
    }    @string fn = default;
    if (c <= 5) { 
        // small numbers of strings use direct runtime helpers.
        // note: order.expr knows this cutoff too.
        fn = fmt.Sprintf("concatstring%d", c);

    }
    else
 { 
        // large numbers of strings are passed to the runtime as a slice.
        fn = "concatstrings";

        var t = types.NewSlice(types.Types[types.TSTRING]); 
        // args[1:] to skip buf arg
        var slice = ir.NewCompLitExpr(@base.Pos, ir.OCOMPLIT, ir.TypeNode(t), args[(int)1..]);
        slice.Prealloc = n.Prealloc;
        args = new slice<ir.Node>(new ir.Node[] { buf, slice });
        slice.SetEsc(ir.EscNone);

    }
    var cat = typecheck.LookupRuntime(fn);
    var r = ir.NewCallExpr(@base.Pos, ir.OCALL, cat, null);
    r.Args = args;
    var r1 = typecheck.Expr(r);
    r1 = walkExpr(r1, _addr_init);
    r1.SetType(n.Type());

    return r1;

}

// walkCall walks an OCALLFUNC, OCALLINTER, or OCALLMETH node.
private static ir.Node walkCall(ptr<ir.CallExpr> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.CallExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    if (n.Op() == ir.OCALLINTER || n.Op() == ir.OCALLMETH) { 
        // We expect both interface call reflect.Type.Method and concrete
        // call reflect.(*rtype).Method.
        usemethod(_addr_n);

    }
    if (n.Op() == ir.OCALLINTER) {
        reflectdata.MarkUsedIfaceMethod(n);
    }
    if (n.Op() == ir.OCALLFUNC && n.X.Op() == ir.OCLOSURE) {
        directClosureCall(n);
    }
    if (isFuncPCIntrinsic(n)) { 
        // For internal/abi.FuncPCABIxxx(fn), if fn is a defined function, rewrite
        // it to the address of the function of the ABI fn is defined.
        ptr<ir.Name> name = n.X._<ptr<ir.Name>>().Sym().Name;
        var arg = n.Args[0];
        obj.ABI wantABI = default;
        switch (name) {
            case "FuncPCABI0": 
                wantABI = obj.ABI0;
                break;
            case "FuncPCABIInternal": 
                wantABI = obj.ABIInternal;
                break;
        }
        if (isIfaceOfFunc(arg)) {
            ptr<ir.Name> fn = arg._<ptr<ir.ConvExpr>>().X._<ptr<ir.Name>>();
            var abi = fn.Func.ABI;
            if (abi != wantABI) {
                @base.ErrorfAt(n.Pos(), "internal/abi.%s expects an %v function, %s is defined as %v", name, wantABI, fn.Sym().Name, abi);
            }
            ir.Node e = ir.NewLinksymExpr(n.Pos(), fn.Sym().LinksymABI(abi), types.Types[types.TUINTPTR]);
            e = ir.NewAddrExpr(n.Pos(), e);
            e.SetType(types.Types[types.TUINTPTR].PtrTo());
            e = ir.NewConvExpr(n.Pos(), ir.OCONVNOP, n.Type(), e);
            return e;
        }
        if (wantABI != obj.ABIInternal) {
            @base.ErrorfAt(n.Pos(), "internal/abi.%s does not accept func expression, which is ABIInternal", name);
        }
        arg = walkExpr(arg, _addr_init);
        e = ir.NewUnaryExpr(n.Pos(), ir.OIDATA, arg);
        e.SetType(n.Type().PtrTo());
        e = ir.NewStarExpr(n.Pos(), e);
        e.SetType(n.Type());
        return e;

    }
    walkCall1(_addr_n, _addr_init);
    return n;

}

private static void walkCall1(ptr<ir.CallExpr> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.CallExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    if (n.Walked()) {
        return ; // already walked
    }
    n.SetWalked(true); 

    // If this is a method call t.M(...),
    // rewrite into a function call T.M(t, ...).
    // TODO(mdempsky): Do this right after type checking.
    if (n.Op() == ir.OCALLMETH) {
        var withRecv = make_slice<ir.Node>(len(n.Args) + 1);
        ptr<ir.SelectorExpr> dot = n.X._<ptr<ir.SelectorExpr>>();
        withRecv[0] = dot.X;
        copy(withRecv[(int)1..], n.Args);
        n.Args = withRecv;

        dot = ir.NewSelectorExpr(dot.Pos(), ir.OXDOT, ir.TypeNode(dot.X.Type()), dot.Selection.Sym);

        n.SetOp(ir.OCALLFUNC);
        n.X = typecheck.Expr(dot);
    }
    var args = n.Args;
    var @params = n.X.Type().Params();

    n.X = walkExpr(n.X, _addr_init);
    walkExprList(args, _addr_init);

    foreach (var (i, arg) in args) { 
        // Validate argument and parameter types match.
        var param = @params.Field(i);
        if (!types.Identical(arg.Type(), param.Type)) {
            @base.FatalfAt(n.Pos(), "assigning %L to parameter %v (type %v)", arg, param.Sym, param.Type);
        }
        if (mayCall(arg)) { 
            // assignment of arg to Temp
            var tmp = typecheck.Temp(param.Type);
            init.Append(convas(typecheck.Stmt(ir.NewAssignStmt(@base.Pos, tmp, arg))._<ptr<ir.AssignStmt>>(), init)); 
            // replace arg with temp
            args[i] = tmp;

        }
    }    n.Args = args;

}

// walkDivMod walks an ODIV or OMOD node.
private static ir.Node walkDivMod(ptr<ir.BinaryExpr> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.BinaryExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    n.X = walkExpr(n.X, _addr_init);
    n.Y = walkExpr(n.Y, _addr_init); 

    // rewrite complex div into function call.
    var et = n.X.Type().Kind();

    if (types.IsComplex[et] && n.Op() == ir.ODIV) {
        var t = n.Type();
        var call = mkcall("complex128div", types.Types[types.TCOMPLEX128], init, typecheck.Conv(n.X, types.Types[types.TCOMPLEX128]), typecheck.Conv(n.Y, types.Types[types.TCOMPLEX128]));
        return typecheck.Conv(call, t);
    }
    if (types.IsFloat[et]) {
        return n;
    }
    if (types.RegSize < 8 && (et == types.TINT64 || et == types.TUINT64)) {
        if (n.Y.Op() == ir.OLITERAL) { 
            // Leave div/mod by constant powers of 2 or small 16-bit constants.
            // The SSA backend will handle those.

            if (et == types.TINT64) 
                var c = ir.Int64Val(n.Y);
                if (c < 0) {
                    c = -c;
                }
                if (c != 0 && c & (c - 1) == 0) {
                    return n;
                }
            else if (et == types.TUINT64) 
                c = ir.Uint64Val(n.Y);
                if (c < 1 << 16) {
                    return n;
                }
                if (c != 0 && c & (c - 1) == 0) {
                    return n;
                }
            
        }
        @string fn = default;
        if (et == types.TINT64) {
            fn = "int64";
        }
        else
 {
            fn = "uint64";
        }
        if (n.Op() == ir.ODIV) {
            fn += "div";
        }
        else
 {
            fn += "mod";
        }
        return mkcall(fn, n.Type(), init, typecheck.Conv(n.X, types.Types[et]), typecheck.Conv(n.Y, types.Types[et]));

    }
    return n;

}

// walkDot walks an ODOT or ODOTPTR node.
private static ir.Node walkDot(ptr<ir.SelectorExpr> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.SelectorExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    usefield(_addr_n);
    n.X = walkExpr(n.X, _addr_init);
    return n;
}

// walkDotType walks an ODOTTYPE or ODOTTYPE2 node.
private static ir.Node walkDotType(ptr<ir.TypeAssertExpr> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.TypeAssertExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    n.X = walkExpr(n.X, _addr_init); 
    // Set up interface type addresses for back end.
    if (!n.Type().IsInterface() && !n.X.Type().IsEmptyInterface()) {
        n.Itab = reflectdata.ITabAddr(n.Type(), n.X.Type());
    }
    return n;

}

// walkIndex walks an OINDEX node.
private static ir.Node walkIndex(ptr<ir.IndexExpr> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.IndexExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    n.X = walkExpr(n.X, _addr_init); 

    // save the original node for bounds checking elision.
    // If it was a ODIV/OMOD walk might rewrite it.
    var r = n.Index;

    n.Index = walkExpr(n.Index, _addr_init); 

    // if range of type cannot exceed static array bound,
    // disable bounds check.
    if (n.Bounded()) {
        return n;
    }
    var t = n.X.Type();
    if (t != null && t.IsPtr()) {
        t = t.Elem();
    }
    if (t.IsArray()) {
        n.SetBounded(bounded(r, t.NumElem()));
        if (@base.Flag.LowerM != 0 && n.Bounded() && !ir.IsConst(n.Index, constant.Int)) {
            @base.Warn("index bounds check elided");
        }
        if (ir.IsSmallIntConst(n.Index) && !n.Bounded()) {
            @base.Errorf("index out of bounds");
        }
    }
    else if (ir.IsConst(n.X, constant.String)) {
        n.SetBounded(bounded(r, int64(len(ir.StringVal(n.X)))));
        if (@base.Flag.LowerM != 0 && n.Bounded() && !ir.IsConst(n.Index, constant.Int)) {
            @base.Warn("index bounds check elided");
        }
        if (ir.IsSmallIntConst(n.Index) && !n.Bounded()) {
            @base.Errorf("index out of bounds");
        }
    }
    if (ir.IsConst(n.Index, constant.Int)) {
        {
            var v = n.Index.Val();

            if (constant.Sign(v) < 0 || ir.ConstOverflow(v, types.Types[types.TINT])) {
                @base.Errorf("index out of bounds");
            }

        }

    }
    return n;

}

// mapKeyArg returns an expression for key that is suitable to be passed
// as the key argument for mapaccess and mapdelete functions.
// n is is the map indexing or delete Node (to provide Pos).
// Note: this is not used for mapassign, which does distinguish pointer vs.
// integer key.
private static ir.Node mapKeyArg(nint fast, ir.Node n, ir.Node key) {

    if (fast == mapslow) 
        // standard version takes key by reference.
        // order.expr made sure key is addressable.
        return typecheck.NodAddr(key);
    else if (fast == mapfast32ptr) 
        // mapaccess and mapdelete don't distinguish pointer vs. integer key.
        return ir.NewConvExpr(n.Pos(), ir.OCONVNOP, types.Types[types.TUINT32], key);
    else if (fast == mapfast64ptr) 
        // mapaccess and mapdelete don't distinguish pointer vs. integer key.
        return ir.NewConvExpr(n.Pos(), ir.OCONVNOP, types.Types[types.TUINT64], key);
    else 
        // fast version takes key by value.
        return key;
    
}

// walkIndexMap walks an OINDEXMAP node.
private static ir.Node walkIndexMap(ptr<ir.IndexExpr> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.IndexExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;
 
    // Replace m[k] with *map{access1,assign}(maptype, m, &k)
    n.X = walkExpr(n.X, _addr_init);
    n.Index = walkExpr(n.Index, _addr_init);
    var map_ = n.X;
    var key = n.Index;
    var t = map_.Type();
    ptr<ir.CallExpr> call;
    if (n.Assigned) { 
        // This m[k] expression is on the left-hand side of an assignment.
        var fast = mapfast(t);
        if (fast == mapslow) { 
            // standard version takes key by reference.
            // order.expr made sure key is addressable.
            key = typecheck.NodAddr(key);

        }
        call = mkcall1(mapfn(mapassign[fast], t, false), null, init, reflectdata.TypePtr(t), map_, key);

    }
    else
 { 
        // m[k] is not the target of an assignment.
        fast = mapfast(t);
        key = mapKeyArg(fast, n, key);
        {
            var w = t.Elem().Width;

            if (w <= zeroValSize) {
                call = mkcall1(mapfn(mapaccess1[fast], t, false), types.NewPtr(t.Elem()), init, reflectdata.TypePtr(t), map_, key);
            }
            else
 {
                var z = reflectdata.ZeroAddr(w);
                call = mkcall1(mapfn("mapaccess1_fat", t, true), types.NewPtr(t.Elem()), init, reflectdata.TypePtr(t), map_, key, z);
            }

        }

    }
    call.SetType(types.NewPtr(t.Elem()));
    call.MarkNonNil(); // mapaccess1* and mapassign always return non-nil pointers.
    var star = ir.NewStarExpr(@base.Pos, call);
    star.SetType(t.Elem());
    star.SetTypecheck(1);
    return star;

}

// walkLogical walks an OANDAND or OOROR node.
private static ir.Node walkLogical(ptr<ir.LogicalExpr> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.LogicalExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    n.X = walkExpr(n.X, _addr_init); 

    // cannot put side effects from n.Right on init,
    // because they cannot run before n.Left is checked.
    // save elsewhere and store on the eventual n.Right.
    ref ir.Nodes ll = ref heap(out ptr<ir.Nodes> _addr_ll);

    n.Y = walkExpr(n.Y, _addr_ll);
    n.Y = ir.InitExpr(ll, n.Y);
    return n;

}

// walkSend walks an OSEND node.
private static ir.Node walkSend(ptr<ir.SendStmt> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.SendStmt n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    var n1 = n.Value;
    n1 = typecheck.AssignConv(n1, n.Chan.Type().Elem(), "chan send");
    n1 = walkExpr(n1, _addr_init);
    n1 = typecheck.NodAddr(n1);
    return mkcall1(chanfn("chansend1", 2, n.Chan.Type()), null, init, n.Chan, n1);
}

// walkSlice walks an OSLICE, OSLICEARR, OSLICESTR, OSLICE3, or OSLICE3ARR node.
private static ir.Node walkSlice(ptr<ir.SliceExpr> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.SliceExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    var checkSlice = ir.ShouldCheckPtr(ir.CurFunc, 1) && n.Op() == ir.OSLICE3ARR && n.X.Op() == ir.OCONVNOP && n.X._<ptr<ir.ConvExpr>>().X.Type().IsUnsafePtr();
    if (checkSlice) {
        ptr<ir.ConvExpr> conv = n.X._<ptr<ir.ConvExpr>>();
        conv.X = walkExpr(conv.X, _addr_init);
    }
    else
 {
        n.X = walkExpr(n.X, _addr_init);
    }
    n.Low = walkExpr(n.Low, _addr_init);
    if (n.Low != null && ir.IsZero(n.Low)) { 
        // Reduce x[0:j] to x[:j] and x[0:j:k] to x[:j:k].
        n.Low = null;

    }
    n.High = walkExpr(n.High, _addr_init);
    n.Max = walkExpr(n.Max, _addr_init);
    if (checkSlice) {
        n.X = walkCheckPtrAlignment(n.X._<ptr<ir.ConvExpr>>(), init, n.Max);
    }
    if (n.Op().IsSlice3()) {
        if (n.Max != null && n.Max.Op() == ir.OCAP && ir.SameSafeExpr(n.X, n.Max._<ptr<ir.UnaryExpr>>().X)) { 
            // Reduce x[i:j:cap(x)] to x[i:j].
            if (n.Op() == ir.OSLICE3) {
                n.SetOp(ir.OSLICE);
            }
            else
 {
                n.SetOp(ir.OSLICEARR);
            }

            return reduceSlice(_addr_n);

        }
        return n;

    }
    return reduceSlice(_addr_n);

}

// walkSliceHeader walks an OSLICEHEADER node.
private static ir.Node walkSliceHeader(ptr<ir.SliceHeaderExpr> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.SliceHeaderExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    n.Ptr = walkExpr(n.Ptr, _addr_init);
    n.Len = walkExpr(n.Len, _addr_init);
    n.Cap = walkExpr(n.Cap, _addr_init);
    return n;
}

// TODO(josharian): combine this with its caller and simplify
private static ir.Node reduceSlice(ptr<ir.SliceExpr> _addr_n) {
    ref ir.SliceExpr n = ref _addr_n.val;

    if (n.High != null && n.High.Op() == ir.OLEN && ir.SameSafeExpr(n.X, n.High._<ptr<ir.UnaryExpr>>().X)) { 
        // Reduce x[i:len(x)] to x[i:].
        n.High = null;

    }
    if ((n.Op() == ir.OSLICE || n.Op() == ir.OSLICESTR) && n.Low == null && n.High == null) { 
        // Reduce x[:] to x.
        if (@base.Debug.Slice > 0) {
            @base.Warn("slice: omit slice operation");
        }
        return n.X;

    }
    return n;

}

// return 1 if integer n must be in range [0, max), 0 otherwise
private static bool bounded(ir.Node n, long max) {
    if (n.Type() == null || !n.Type().IsInteger()) {
        return false;
    }
    var sign = n.Type().IsSigned();
    var bits = int32(8 * n.Type().Width);

    if (ir.IsSmallIntConst(n)) {
        var v = ir.Int64Val(n);
        return 0 <= v && v < max;
    }

    if (n.Op() == ir.OAND || n.Op() == ir.OANDNOT) 
        ptr<ir.BinaryExpr> n = n._<ptr<ir.BinaryExpr>>();
        v = int64(-1);

        if (ir.IsSmallIntConst(n.X)) 
            v = ir.Int64Val(n.X);
        else if (ir.IsSmallIntConst(n.Y)) 
            v = ir.Int64Val(n.Y);
            if (n.Op() == ir.OANDNOT) {
                v = ~v;
                if (!sign) {
                    v &= 1 << (int)(uint(bits)) - 1;
                }
            }
                if (0 <= v && v < max) {
            return true;
        }
    else if (n.Op() == ir.OMOD) 
        n = n._<ptr<ir.BinaryExpr>>();
        if (!sign && ir.IsSmallIntConst(n.Y)) {
            v = ir.Int64Val(n.Y);
            if (0 <= v && v <= max) {
                return true;
            }
        }
    else if (n.Op() == ir.ODIV) 
        n = n._<ptr<ir.BinaryExpr>>();
        if (!sign && ir.IsSmallIntConst(n.Y)) {
            v = ir.Int64Val(n.Y);
            while (bits > 0 && v >= 2) {
                bits--;
                v>>=1;
            }
        }
    else if (n.Op() == ir.ORSH) 
        n = n._<ptr<ir.BinaryExpr>>();
        if (!sign && ir.IsSmallIntConst(n.Y)) {
            v = ir.Int64Val(n.Y);
            if (v > int64(bits)) {
                return true;
            }
            bits -= int32(v);
        }
        if (!sign && bits <= 62 && 1 << (int)(uint(bits)) <= max) {
        return true;
    }
    return false;

}

// usemethod checks interface method calls for uses of reflect.Type.Method.
private static void usemethod(ptr<ir.CallExpr> _addr_n) {
    ref ir.CallExpr n = ref _addr_n.val;

    var t = n.X.Type(); 

    // Looking for either of:
    //    Method(int) reflect.Method
    //    MethodByName(string) (reflect.Method, bool)
    //
    // TODO(crawshaw): improve precision of match by working out
    //                 how to check the method name.
    {
        var n__prev1 = n;

        var n = t.NumParams();

        if (n != 1) {
            return ;
        }
        n = n__prev1;

    }

    {
        var n__prev1 = n;

        n = t.NumResults();

        if (n != 1 && n != 2) {
            return ;
        }
        n = n__prev1;

    }

    var p0 = t.Params().Field(0);
    var res0 = t.Results().Field(0);
    ptr<types.Field> res1;
    if (t.NumResults() == 2) {
        res1 = t.Results().Field(1);
    }
    if (res1 == null) {
        if (p0.Type.Kind() != types.TINT) {
            return ;
        }
    }
    else
 {
        if (!p0.Type.IsString()) {
            return ;
        }
        if (!res1.Type.IsBoolean()) {
            return ;
        }
    }
    if (@base.Ctxt.Pkgpath == "reflect") {
        switch (ir.CurFunc.Nname.Sym().Name) { // TODO: is there a better way than hardcoding the names?
            case "(*rtype).Method": 

            case "(*rtype).MethodByName": 

            case "(*interfaceType).Method": 

            case "(*interfaceType).MethodByName": 
                return ;
                break;
        }

    }
    {
        var s = res0.Type.Sym();

        if (s != null && s.Name == "Method" && types.IsReflectPkg(s.Pkg)) {
            ir.CurFunc.SetReflectMethod(true); 
            // The LSym is initialized at this point. We need to set the attribute on the LSym.
            ir.CurFunc.LSym.Set(obj.AttrReflectMethod, true);

        }
    }

}

private static void usefield(ptr<ir.SelectorExpr> _addr_n) {
    ref ir.SelectorExpr n = ref _addr_n.val;

    if (!buildcfg.Experiment.FieldTrack) {
        return ;
    }

    if (n.Op() == ir.ODOT || n.Op() == ir.ODOTPTR) 
        break;
    else 
        @base.Fatalf("usefield %v", n.Op());
        var field = n.Selection;
    if (field == null) {
        @base.Fatalf("usefield %v %v without paramfld", n.X.Type(), n.Sel);
    }
    if (field.Sym != n.Sel) {
        @base.Fatalf("field inconsistency: %v != %v", field.Sym, n.Sel);
    }
    if (!strings.Contains(field.Note, "go:\"track\"")) {
        return ;
    }
    var outer = n.X.Type();
    if (outer.IsPtr()) {
        outer = outer.Elem();
    }
    if (outer.Sym() == null) {
        @base.Errorf("tracked field must be in named struct type");
    }
    if (!types.IsExported(field.Sym.Name)) {
        @base.Errorf("tracked field must be exported (upper case)");
    }
    var sym = reflectdata.TrackSym(outer, field);
    if (ir.CurFunc.FieldTrack == null) {
        ir.CurFunc.FieldTrack = make();
    }
    ir.CurFunc.FieldTrack[sym] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};

}

} // end walk_package
