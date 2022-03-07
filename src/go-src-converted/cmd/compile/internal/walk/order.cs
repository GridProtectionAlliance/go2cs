// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package walk -- go2cs converted at 2022 March 06 23:11:57 UTC
// import "cmd/compile/internal/walk" ==> using walk = go.cmd.compile.@internal.walk_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\walk\order.go
using fmt = go.fmt_package;
using constant = go.go.constant_package;
using buildcfg = go.@internal.buildcfg_package;

using @base = go.cmd.compile.@internal.@base_package;
using escape = go.cmd.compile.@internal.escape_package;
using ir = go.cmd.compile.@internal.ir_package;
using reflectdata = go.cmd.compile.@internal.reflectdata_package;
using staticinit = go.cmd.compile.@internal.staticinit_package;
using typecheck = go.cmd.compile.@internal.typecheck_package;
using types = go.cmd.compile.@internal.types_package;
using src = go.cmd.@internal.src_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class walk_package {

    // Rewrite tree to use separate statements to enforce
    // order of evaluation. Makes walk easier, because it
    // can (after this runs) reorder at will within an expression.
    //
    // Rewrite m[k] op= r into m[k] = m[k] op r if op is / or %.
    //
    // Introduce temporaries as needed by runtime routines.
    // For example, the map runtime routines take the map key
    // by reference, so make sure all map keys are addressable
    // by copying them to temporaries as needed.
    // The same is true for channel operations.
    //
    // Arrange that map index expressions only appear in direct
    // assignments x = m[k] or m[k] = x, never in larger expressions.
    //
    // Arrange that receive expressions only appear in direct assignments
    // x = <-c or as standalone statements <-c, never in larger expressions.

    // TODO(rsc): The temporary introduction during multiple assignments
    // should be moved into this file, so that the temporaries can be cleaned
    // and so that conversions implicit in the OAS2FUNC and OAS2RECV
    // nodes can be made explicit and then have their temporaries cleaned.

    // TODO(rsc): Goto and multilevel break/continue can jump over
    // inserted VARKILL annotations. Work out a way to handle these.
    // The current implementation is safe, in that it will execute correctly.
    // But it won't reuse temporaries as aggressively as it might, and
    // it can result in unnecessary zeroing of those variables in the function
    // prologue.

    // orderState holds state during the ordering process.
private partial struct orderState {
    public slice<ir.Node> @out; // list of generated statements
    public slice<ptr<ir.Name>> temp; // stack of temporary variables
    public map<@string, slice<ptr<ir.Name>>> free; // free list of unused temporaries, by type.LongString().
    public Func<ir.Node, ir.Node> edit; // cached closure of o.exprNoLHS
}

// Order rewrites fn.Nbody to apply the ordering constraints
// described in the comment at the top of the file.
private static void order(ptr<ir.Func> _addr_fn) {
    ref ir.Func fn = ref _addr_fn.val;

    if (@base.Flag.W > 1) {
        var s = fmt.Sprintf("\nbefore order %v", fn.Sym());
        ir.DumpList(s, fn.Body);
    }
    orderBlock(_addr_fn.Body, /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, slice<ptr<ir.Name>>>{});

}

// append typechecks stmt and appends it to out.
private static void append(this ptr<orderState> _addr_o, ir.Node stmt) {
    ref orderState o = ref _addr_o.val;

    o.@out = append(o.@out, typecheck.Stmt(stmt));
}

// newTemp allocates a new temporary with the given type,
// pushes it onto the temp stack, and returns it.
// If clear is true, newTemp emits code to zero the temporary.
private static ptr<ir.Name> newTemp(this ptr<orderState> _addr_o, ptr<types.Type> _addr_t, bool clear) {
    ref orderState o = ref _addr_o.val;
    ref types.Type t = ref _addr_t.val;

    ptr<ir.Name> v; 
    // Note: LongString is close to the type equality we want,
    // but not exactly. We still need to double-check with types.Identical.
    var key = t.LongString();
    var a = o.free[key];
    foreach (var (i, n) in a) {
        if (types.Identical(t, n.Type())) {
            v = a[i];
            a[i] = a[len(a) - 1];
            a = a[..(int)len(a) - 1];
            o.free[key] = a;
            break;
        }
    }    if (v == null) {
        v = typecheck.Temp(t);
    }
    if (clear) {
        o.append(ir.NewAssignStmt(@base.Pos, v, null));
    }
    o.temp = append(o.temp, v);
    return _addr_v!;

}

// copyExpr behaves like newTemp but also emits
// code to initialize the temporary to the value n.
private static ptr<ir.Name> copyExpr(this ptr<orderState> _addr_o, ir.Node n) {
    ref orderState o = ref _addr_o.val;

    return _addr_o.copyExpr1(n, false)!;
}

// copyExprClear is like copyExpr but clears the temp before assignment.
// It is provided for use when the evaluation of tmp = n turns into
// a function call that is passed a pointer to the temporary as the output space.
// If the call blocks before tmp has been written,
// the garbage collector will still treat the temporary as live,
// so we must zero it before entering that call.
// Today, this only happens for channel receive operations.
// (The other candidate would be map access, but map access
// returns a pointer to the result data instead of taking a pointer
// to be filled in.)
private static ptr<ir.Name> copyExprClear(this ptr<orderState> _addr_o, ir.Node n) {
    ref orderState o = ref _addr_o.val;

    return _addr_o.copyExpr1(n, true)!;
}

private static ptr<ir.Name> copyExpr1(this ptr<orderState> _addr_o, ir.Node n, bool clear) {
    ref orderState o = ref _addr_o.val;

    var t = n.Type();
    var v = o.newTemp(t, clear);
    o.append(ir.NewAssignStmt(@base.Pos, v, n));
    return _addr_v!;
}

// cheapExpr returns a cheap version of n.
// The definition of cheap is that n is a variable or constant.
// If not, cheapExpr allocates a new tmp, emits tmp = n,
// and then returns tmp.
private static ir.Node cheapExpr(this ptr<orderState> _addr_o, ir.Node n) {
    ref orderState o = ref _addr_o.val;

    if (n == null) {
        return null;
    }

    if (n.Op() == ir.ONAME || n.Op() == ir.OLITERAL || n.Op() == ir.ONIL) 
        return n;
    else if (n.Op() == ir.OLEN || n.Op() == ir.OCAP) 
        ptr<ir.UnaryExpr> n = n._<ptr<ir.UnaryExpr>>();
        var l = o.cheapExpr(n.X);
        if (l == n.X) {
            return n;
        }
        ptr<ir.UnaryExpr> a = ir.SepCopy(n)._<ptr<ir.UnaryExpr>>();
        a.X = l;
        return typecheck.Expr(a);
        return o.copyExpr(n);

}

// safeExpr returns a safe version of n.
// The definition of safe is that n can appear multiple times
// without violating the semantics of the original program,
// and that assigning to the safe version has the same effect
// as assigning to the original n.
//
// The intended use is to apply to x when rewriting x += y into x = x + y.
private static ir.Node safeExpr(this ptr<orderState> _addr_o, ir.Node n) {
    ref orderState o = ref _addr_o.val;


    if (n.Op() == ir.ONAME || n.Op() == ir.OLITERAL || n.Op() == ir.ONIL) 
        return n;
    else if (n.Op() == ir.OLEN || n.Op() == ir.OCAP) 
        ptr<ir.UnaryExpr> n = n._<ptr<ir.UnaryExpr>>();
        var l = o.safeExpr(n.X);
        if (l == n.X) {
            return n;
        }
        ptr<ir.UnaryExpr> a = ir.SepCopy(n)._<ptr<ir.UnaryExpr>>();
        a.X = l;
        return typecheck.Expr(a);
    else if (n.Op() == ir.ODOT) 
        n = n._<ptr<ir.SelectorExpr>>();
        l = o.safeExpr(n.X);
        if (l == n.X) {
            return n;
        }
        a = ir.SepCopy(n)._<ptr<ir.SelectorExpr>>();
        a.X = l;
        return typecheck.Expr(a);
    else if (n.Op() == ir.ODOTPTR) 
        n = n._<ptr<ir.SelectorExpr>>();
        l = o.cheapExpr(n.X);
        if (l == n.X) {
            return n;
        }
        a = ir.SepCopy(n)._<ptr<ir.SelectorExpr>>();
        a.X = l;
        return typecheck.Expr(a);
    else if (n.Op() == ir.ODEREF) 
        n = n._<ptr<ir.StarExpr>>();
        l = o.cheapExpr(n.X);
        if (l == n.X) {
            return n;
        }
        a = ir.SepCopy(n)._<ptr<ir.StarExpr>>();
        a.X = l;
        return typecheck.Expr(a);
    else if (n.Op() == ir.OINDEX || n.Op() == ir.OINDEXMAP) 
        n = n._<ptr<ir.IndexExpr>>();
        l = default;
        if (n.X.Type().IsArray()) {
            l = o.safeExpr(n.X);
        }
        else
 {
            l = o.cheapExpr(n.X);
        }
        var r = o.cheapExpr(n.Index);
        if (l == n.X && r == n.Index) {
            return n;
        }
        a = ir.SepCopy(n)._<ptr<ir.IndexExpr>>();
        a.X = l;
        a.Index = r;
        return typecheck.Expr(a);
    else 
        @base.Fatalf("order.safeExpr %v", n.Op());
        return null; // not reached
    }

// isaddrokay reports whether it is okay to pass n's address to runtime routines.
// Taking the address of a variable makes the liveness and optimization analyses
// lose track of where the variable's lifetime ends. To avoid hurting the analyses
// of ordinary stack variables, those are not 'isaddrokay'. Temporaries are okay,
// because we emit explicit VARKILL instructions marking the end of those
// temporaries' lifetimes.
private static bool isaddrokay(ir.Node n) {
    return ir.IsAddressable(n) && (n.Op() != ir.ONAME || n._<ptr<ir.Name>>().Class == ir.PEXTERN || ir.IsAutoTmp(n));
}

// addrTemp ensures that n is okay to pass by address to runtime routines.
// If the original argument n is not okay, addrTemp creates a tmp, emits
// tmp = n, and then returns tmp.
// The result of addrTemp MUST be assigned back to n, e.g.
//     n.Left = o.addrTemp(n.Left)
private static ir.Node addrTemp(this ptr<orderState> _addr_o, ir.Node n) {
    ref orderState o = ref _addr_o.val;

    if (n.Op() == ir.OLITERAL || n.Op() == ir.ONIL) { 
        // TODO: expand this to all static composite literal nodes?
        n = typecheck.DefaultLit(n, null);
        types.CalcSize(n.Type());
        var vstat = readonlystaticname(n.Type());
        staticinit.Schedule s = default;
        s.StaticAssign(vstat, 0, n, n.Type());
        if (s.Out != null) {
            @base.Fatalf("staticassign of const generated code: %+v", n);
        }
        vstat = typecheck.Expr(vstat)._<ptr<ir.Name>>();
        return vstat;

    }
    if (isaddrokay(n)) {
        return n;
    }
    return o.copyExpr(n);

}

// mapKeyTemp prepares n to be a key in a map runtime call and returns n.
// It should only be used for map runtime calls which have *_fast* versions.
private static ir.Node mapKeyTemp(this ptr<orderState> _addr_o, ptr<types.Type> _addr_t, ir.Node n) {
    ref orderState o = ref _addr_o.val;
    ref types.Type t = ref _addr_t.val;
 
    // Most map calls need to take the address of the key.
    // Exception: map*_fast* calls. See golang.org/issue/19015.
    var alg = mapfast(t);
    if (alg == mapslow) {
        return o.addrTemp(n);
    }
    ptr<types.Type> kt;

    if (alg == mapfast32) 
        kt = types.Types[types.TUINT32];
    else if (alg == mapfast64) 
        kt = types.Types[types.TUINT64];
    else if (alg == mapfast32ptr || alg == mapfast64ptr) 
        kt = types.Types[types.TUNSAFEPTR];
    else if (alg == mapfaststr) 
        kt = types.Types[types.TSTRING];
        var nt = n.Type();

    if (nt == kt) 
        return n;
    else if (nt.Kind() == kt.Kind() || nt.IsPtrShaped() && kt.IsPtrShaped()) 
        // can directly convert (e.g. named type to underlying type, or one pointer to another)
        return typecheck.Expr(ir.NewConvExpr(n.Pos(), ir.OCONVNOP, kt, n));
    else if (nt.IsInteger() && kt.IsInteger()) 
        // can directly convert (e.g. int32 to uint32)
        if (n.Op() == ir.OLITERAL && nt.IsSigned()) { 
            // avoid constant overflow error
            n = ir.NewConstExpr(constant.MakeUint64(uint64(ir.Int64Val(n))), n);
            n.SetType(kt);
            return n;

        }
        return typecheck.Expr(ir.NewConvExpr(n.Pos(), ir.OCONV, kt, n));
    else 
        // Unsafe cast through memory.
        // We'll need to do a load with type kt. Create a temporary of type kt to
        // ensure sufficient alignment. nt may be under-aligned.
        if (kt.Align < nt.Align) {
            @base.Fatalf("mapKeyTemp: key type is not sufficiently aligned, kt=%v nt=%v", kt, nt);
        }
        var tmp = o.newTemp(kt, true); 
        // *(*nt)(&tmp) = n
        ir.Node e = typecheck.NodAddr(tmp);
        e = ir.NewConvExpr(n.Pos(), ir.OCONVNOP, nt.PtrTo(), e);
        e = ir.NewStarExpr(n.Pos(), e);
        o.append(ir.NewAssignStmt(@base.Pos, e, n));
        return tmp;
    
}

// mapKeyReplaceStrConv replaces OBYTES2STR by OBYTES2STRTMP
// in n to avoid string allocations for keys in map lookups.
// Returns a bool that signals if a modification was made.
//
// For:
//  x = m[string(k)]
//  x = m[T1{... Tn{..., string(k), ...}]
// where k is []byte, T1 to Tn is a nesting of struct and array literals,
// the allocation of backing bytes for the string can be avoided
// by reusing the []byte backing array. These are special cases
// for avoiding allocations when converting byte slices to strings.
// It would be nice to handle these generally, but because
// []byte keys are not allowed in maps, the use of string(k)
// comes up in important cases in practice. See issue 3512.
private static bool mapKeyReplaceStrConv(ir.Node n) {
    bool replaced = default;

    if (n.Op() == ir.OBYTES2STR) 
        ptr<ir.ConvExpr> n = n._<ptr<ir.ConvExpr>>();
        n.SetOp(ir.OBYTES2STRTMP);
        replaced = true;
    else if (n.Op() == ir.OSTRUCTLIT) 
        n = n._<ptr<ir.CompLitExpr>>();
        {
            var elem__prev1 = elem;

            foreach (var (_, __elem) in n.List) {
                elem = __elem;
                ptr<ir.StructKeyExpr> elem = elem._<ptr<ir.StructKeyExpr>>();
                if (mapKeyReplaceStrConv(elem.Value)) {
                    replaced = true;
                }
            }

            elem = elem__prev1;
        }
    else if (n.Op() == ir.OARRAYLIT) 
        n = n._<ptr<ir.CompLitExpr>>();
        {
            var elem__prev1 = elem;

            foreach (var (_, __elem) in n.List) {
                elem = __elem;
                if (elem.Op() == ir.OKEY) {
                    elem = elem._<ptr<ir.KeyExpr>>().Value;
                }
                if (mapKeyReplaceStrConv(elem)) {
                    replaced = true;
                }
            }

            elem = elem__prev1;
        }
        return replaced;

}

private partial struct ordermarker { // : nint
}

// markTemp returns the top of the temporary variable stack.
private static ordermarker markTemp(this ptr<orderState> _addr_o) {
    ref orderState o = ref _addr_o.val;

    return ordermarker(len(o.temp));
}

// popTemp pops temporaries off the stack until reaching the mark,
// which must have been returned by markTemp.
private static void popTemp(this ptr<orderState> _addr_o, ordermarker mark) {
    ref orderState o = ref _addr_o.val;

    foreach (var (_, n) in o.temp[(int)mark..]) {
        var key = n.Type().LongString();
        o.free[key] = append(o.free[key], n);
    }    o.temp = o.temp[..(int)mark];
}

// cleanTempNoPop emits VARKILL instructions to *out
// for each temporary above the mark on the temporary stack.
// It does not pop the temporaries from the stack.
private static slice<ir.Node> cleanTempNoPop(this ptr<orderState> _addr_o, ordermarker mark) {
    ref orderState o = ref _addr_o.val;

    slice<ir.Node> @out = default;
    for (var i = len(o.temp) - 1; i >= int(mark); i--) {
        var n = o.temp[i];
        out = append(out, typecheck.Stmt(ir.NewUnaryExpr(@base.Pos, ir.OVARKILL, n)));
    }
    return out;
}

// cleanTemp emits VARKILL instructions for each temporary above the
// mark on the temporary stack and removes them from the stack.
private static void cleanTemp(this ptr<orderState> _addr_o, ordermarker top) {
    ref orderState o = ref _addr_o.val;

    o.@out = append(o.@out, o.cleanTempNoPop(top));
    o.popTemp(top);
}

// stmtList orders each of the statements in the list.
private static void stmtList(this ptr<orderState> _addr_o, ir.Nodes l) {
    ref orderState o = ref _addr_o.val;

    var s = l;
    foreach (var (i) in s) {
        orderMakeSliceCopy(s[(int)i..]);
        o.stmt(s[i]);
    }
}

// orderMakeSliceCopy matches the pattern:
//  m = OMAKESLICE([]T, x); OCOPY(m, s)
// and rewrites it to:
//  m = OMAKESLICECOPY([]T, x, s); nil
private static void orderMakeSliceCopy(slice<ir.Node> s) {
    if (@base.Flag.N != 0 || @base.Flag.Cfg.Instrumenting) {
        return ;
    }
    if (len(s) < 2 || s[0] == null || s[0].Op() != ir.OAS || s[1] == null || s[1].Op() != ir.OCOPY) {
        return ;
    }
    ptr<ir.AssignStmt> @as = s[0]._<ptr<ir.AssignStmt>>();
    ptr<ir.BinaryExpr> cp = s[1]._<ptr<ir.BinaryExpr>>();
    if (@as.Y == null || @as.Y.Op() != ir.OMAKESLICE || ir.IsBlank(@as.X) || @as.X.Op() != ir.ONAME || cp.X.Op() != ir.ONAME || cp.Y.Op() != ir.ONAME || @as.X.Name() != cp.X.Name() || cp.X.Name() == cp.Y.Name()) { 
        // The line above this one is correct with the differing equality operators:
        // we want as.X and cp.X to be the same name,
        // but we want the initial data to be coming from a different name.
        return ;

    }
    ptr<ir.MakeExpr> mk = @as.Y._<ptr<ir.MakeExpr>>();
    if (mk.Esc() == ir.EscNone || mk.Len == null || mk.Cap != null) {
        return ;
    }
    mk.SetOp(ir.OMAKESLICECOPY);
    mk.Cap = cp.Y; 
    // Set bounded when m = OMAKESLICE([]T, len(s)); OCOPY(m, s)
    mk.SetBounded(mk.Len.Op() == ir.OLEN && ir.SameSafeExpr(mk.Len._<ptr<ir.UnaryExpr>>().X, cp.Y));
    @as.Y = typecheck.Expr(mk);
    s[1] = null; // remove separate copy call
}

// edge inserts coverage instrumentation for libfuzzer.
private static void edge(this ptr<orderState> _addr_o) {
    ref orderState o = ref _addr_o.val;

    if (@base.Debug.Libfuzzer == 0) {
        return ;
    }
    var counter = staticinit.StaticName(types.Types[types.TUINT8]);
    counter.SetLibfuzzerExtraCounter(true); 

    // counter += 1
    var incr = ir.NewAssignOpStmt(@base.Pos, ir.OADD, counter, ir.NewInt(1));
    o.append(incr);

}

// orderBlock orders the block of statements in n into a new slice,
// and then replaces the old slice in n with the new slice.
// free is a map that can be used to obtain temporary variables by type.
private static void orderBlock(ptr<ir.Nodes> _addr_n, map<@string, slice<ptr<ir.Name>>> free) {
    ref ir.Nodes n = ref _addr_n.val;

    orderState order = default;
    order.free = free;
    var mark = order.markTemp();
    order.edge();
    order.stmtList(n);
    order.cleanTemp(mark);
    n = order.@out;
}

// exprInPlace orders the side effects in *np and
// leaves them as the init list of the final *np.
// The result of exprInPlace MUST be assigned back to n, e.g.
//     n.Left = o.exprInPlace(n.Left)
private static ir.Node exprInPlace(this ptr<orderState> _addr_o, ir.Node n) {
    ref orderState o = ref _addr_o.val;

    orderState order = default;
    order.free = o.free;
    n = order.expr(n, null);
    n = ir.InitExpr(order.@out, n); 

    // insert new temporaries from order
    // at head of outer list.
    o.temp = append(o.temp, order.temp);
    return n;

}

// orderStmtInPlace orders the side effects of the single statement *np
// and replaces it with the resulting statement list.
// The result of orderStmtInPlace MUST be assigned back to n, e.g.
//     n.Left = orderStmtInPlace(n.Left)
// free is a map that can be used to obtain temporary variables by type.
private static ir.Node orderStmtInPlace(ir.Node n, map<@string, slice<ptr<ir.Name>>> free) {
    orderState order = default;
    order.free = free;
    var mark = order.markTemp();
    order.stmt(n);
    order.cleanTemp(mark);
    return ir.NewBlockStmt(src.NoXPos, order.@out);
}

// init moves n's init list to o.out.
private static void init(this ptr<orderState> _addr_o, ir.Node n) {
    ref orderState o = ref _addr_o.val;

    if (ir.MayBeShared(n)) { 
        // For concurrency safety, don't mutate potentially shared nodes.
        // First, ensure that no work is required here.
        if (len(n.Init()) > 0) {
            @base.Fatalf("order.init shared node with ninit");
        }
        return ;

    }
    o.stmtList(ir.TakeInit(n));

}

// call orders the call expression n.
// n.Op is OCALLMETH/OCALLFUNC/OCALLINTER or a builtin like OCOPY.
private static void call(this ptr<orderState> _addr_o, ir.Node nn) {
    ref orderState o = ref _addr_o.val;

    if (len(nn.Init()) > 0) { 
        // Caller should have already called o.init(nn).
        @base.Fatalf("%v with unexpected ninit", nn.Op());

    }
    if (nn.Op() != ir.OCALLFUNC && nn.Op() != ir.OCALLMETH && nn.Op() != ir.OCALLINTER) {
        switch (nn.type()) {
            case ptr<ir.UnaryExpr> n:
                n.X = o.expr(n.X, null);
                break;
            case ptr<ir.ConvExpr> n:
                n.X = o.expr(n.X, null);
                break;
            case ptr<ir.BinaryExpr> n:
                n.X = o.expr(n.X, null);
                n.Y = o.expr(n.Y, null);
                break;
            case ptr<ir.MakeExpr> n:
                n.Len = o.expr(n.Len, null);
                n.Cap = o.expr(n.Cap, null);
                break;
            case ptr<ir.CallExpr> n:
                o.exprList(n.Args);
                break;
            default:
            {
                var n = nn.type();
                @base.Fatalf("unexpected call: %+v", n);
                break;
            }
        }
        return ;

    }
    ptr<ir.CallExpr> n = nn._<ptr<ir.CallExpr>>();
    typecheck.FixVariadicCall(n);

    if (isFuncPCIntrinsic(_addr_n) && isIfaceOfFunc(n.Args[0])) { 
        // For internal/abi.FuncPCABIxxx(fn), if fn is a defined function,
        // do not introduce temporaries here, so it is easier to rewrite it
        // to symbol address reference later in walk.
        return ;

    }
    n.X = o.expr(n.X, null);
    o.exprList(n.Args);

    if (n.Op() == ir.OCALLINTER) {
        return ;
    }
    Action<ir.Node> keepAlive = arg => { 
        // If the argument is really a pointer being converted to uintptr,
        // arrange for the pointer to be kept alive until the call returns,
        // by copying it into a temp and marking that temp
        // still alive when we pop the temp stack.
        if (arg.Op() == ir.OCONVNOP) {
            ptr<ir.ConvExpr> arg = arg._<ptr<ir.ConvExpr>>();
            if (arg.X.Type().IsUnsafePtr()) {
                var x = o.copyExpr(arg.X);
                arg.X = x;
                x.SetAddrtaken(true); // ensure SSA keeps the x variable
                n.KeepAlive = append(n.KeepAlive, x);

            }

        }
    }; 

    // Check for "unsafe-uintptr" tag provided by escape analysis.
    foreach (var (i, param) in n.X.Type().Params().FieldSlice()) {
        if (param.Note == escape.UnsafeUintptrNote || param.Note == escape.UintptrEscapesNote) {
            {
                ptr<ir.ConvExpr> arg__prev2 = arg;

                arg = n.Args[i];

                if (arg.Op() == ir.OSLICELIT) {
                    arg = arg._<ptr<ir.CompLitExpr>>();
                    foreach (var (_, elt) in arg.List) {
                        keepAlive(elt);
                    }
                else
                } {
                    keepAlive(arg);
                }

                arg = arg__prev2;

            }

        }
    }
}

// mapAssign appends n to o.out.
private static void mapAssign(this ptr<orderState> _addr_o, ir.Node n) {
    ref orderState o = ref _addr_o.val;


    if (n.Op() == ir.OAS) 
        ptr<ir.AssignStmt> n = n._<ptr<ir.AssignStmt>>();
        if (n.X.Op() == ir.OINDEXMAP) {
            n.Y = o.safeMapRHS(n.Y);
        }
        o.@out = append(o.@out, n);
    else if (n.Op() == ir.OASOP) 
        n = n._<ptr<ir.AssignOpStmt>>();
        if (n.X.Op() == ir.OINDEXMAP) {
            n.Y = o.safeMapRHS(n.Y);
        }
        o.@out = append(o.@out, n);
    else 
        @base.Fatalf("order.mapAssign %v", n.Op());
    
}

private static ir.Node safeMapRHS(this ptr<orderState> _addr_o, ir.Node r) {
    ref orderState o = ref _addr_o.val;
 
    // Make sure we evaluate the RHS before starting the map insert.
    // We need to make sure the RHS won't panic.  See issue 22881.
    if (r.Op() == ir.OAPPEND) {
        ptr<ir.CallExpr> r = r._<ptr<ir.CallExpr>>();
        var s = r.Args[(int)1..];
        foreach (var (i, n) in s) {
            s[i] = o.cheapExpr(n);
        }        return r;
    }
    return o.cheapExpr(r);

}

// stmt orders the statement n, appending to o.out.
// Temporaries created during the statement are cleaned
// up using VARKILL instructions as possible.
private static void stmt(this ptr<orderState> _addr_o, ir.Node n) {
    ref orderState o = ref _addr_o.val;

    if (n == null) {
        return ;
    }
    var lno = ir.SetPos(n);
    o.init(n);


    if (n.Op() == ir.OVARKILL || n.Op() == ir.OVARLIVE || n.Op() == ir.OINLMARK) 
        o.@out = append(o.@out, n);
    else if (n.Op() == ir.OAS) 
        ptr<ir.AssignStmt> n = n._<ptr<ir.AssignStmt>>();
        var t = o.markTemp();
        n.X = o.expr(n.X, null);
        n.Y = o.expr(n.Y, n.X);
        o.mapAssign(n);
        o.cleanTemp(t);
    else if (n.Op() == ir.OASOP) 
        n = n._<ptr<ir.AssignOpStmt>>();
        t = o.markTemp();
        n.X = o.expr(n.X, null);
        n.Y = o.expr(n.Y, null);

        if (@base.Flag.Cfg.Instrumenting || n.X.Op() == ir.OINDEXMAP && (n.AsOp == ir.ODIV || n.AsOp == ir.OMOD)) { 
            // Rewrite m[k] op= r into m[k] = m[k] op r so
            // that we can ensure that if op panics
            // because r is zero, the panic happens before
            // the map assignment.
            // DeepCopy is a big hammer here, but safeExpr
            // makes sure there is nothing too deep being copied.
            var l1 = o.safeExpr(n.X);
            var l2 = ir.DeepCopy(src.NoXPos, l1);
            if (l2.Op() == ir.OINDEXMAP) {
                l2 = l2._<ptr<ir.IndexExpr>>();
                l2.Assigned = false;
            }

            l2 = o.copyExpr(l2);
            var r = o.expr(typecheck.Expr(ir.NewBinaryExpr(n.Pos(), n.AsOp, l2, n.Y)), null);
            var @as = typecheck.Stmt(ir.NewAssignStmt(n.Pos(), l1, r));
            o.mapAssign(as);
            o.cleanTemp(t);
            return ;

        }
        o.mapAssign(n);
        o.cleanTemp(t);
    else if (n.Op() == ir.OAS2) 
        n = n._<ptr<ir.AssignListStmt>>();
        t = o.markTemp();
        o.exprList(n.Lhs);
        o.exprList(n.Rhs);
        o.@out = append(o.@out, n);
        o.cleanTemp(t); 

        // Special: avoid copy of func call n.Right
    else if (n.Op() == ir.OAS2FUNC) 
        n = n._<ptr<ir.AssignListStmt>>();
        t = o.markTemp();
        o.exprList(n.Lhs);
        o.init(n.Rhs[0]);
        o.call(n.Rhs[0]);
        o.as2func(n);
        o.cleanTemp(t); 

        // Special: use temporary variables to hold result,
        // so that runtime can take address of temporary.
        // No temporary for blank assignment.
        //
        // OAS2MAPR: make sure key is addressable if needed,
        //           and make sure OINDEXMAP is not copied out.
    else if (n.Op() == ir.OAS2DOTTYPE || n.Op() == ir.OAS2RECV || n.Op() == ir.OAS2MAPR) 
        n = n._<ptr<ir.AssignListStmt>>();
        t = o.markTemp();
        o.exprList(n.Lhs);

        {
            var r__prev2 = r;

            r = n.Rhs[0];


            if (r.Op() == ir.ODOTTYPE2) 
                r = r._<ptr<ir.TypeAssertExpr>>();
                r.X = o.expr(r.X, null);
            else if (r.Op() == ir.ORECV) 
                r = r._<ptr<ir.UnaryExpr>>();
                r.X = o.expr(r.X, null);
            else if (r.Op() == ir.OINDEXMAP) 
                r = r._<ptr<ir.IndexExpr>>();
                r.X = o.expr(r.X, null);
                r.Index = o.expr(r.Index, null); 
                // See similar conversion for OINDEXMAP below.
                _ = mapKeyReplaceStrConv(r.Index);
                r.Index = o.mapKeyTemp(r.X.Type(), r.Index);
            else 
                @base.Fatalf("order.stmt: %v", r.Op());


            r = r__prev2;
        }

        o.as2ok(n);
        o.cleanTemp(t); 

        // Special: does not save n onto out.
    else if (n.Op() == ir.OBLOCK) 
        n = n._<ptr<ir.BlockStmt>>();
        o.stmtList(n.List); 

        // Special: n->left is not an expression; save as is.
    else if (n.Op() == ir.OBREAK || n.Op() == ir.OCONTINUE || n.Op() == ir.ODCL || n.Op() == ir.ODCLCONST || n.Op() == ir.ODCLTYPE || n.Op() == ir.OFALL || n.Op() == ir.OGOTO || n.Op() == ir.OLABEL || n.Op() == ir.OTAILCALL) 
        o.@out = append(o.@out, n); 

        // Special: handle call arguments.
    else if (n.Op() == ir.OCALLFUNC || n.Op() == ir.OCALLINTER || n.Op() == ir.OCALLMETH) 
        n = n._<ptr<ir.CallExpr>>();
        t = o.markTemp();
        o.call(n);
        o.@out = append(o.@out, n);
        o.cleanTemp(t);
    else if (n.Op() == ir.OCLOSE || n.Op() == ir.ORECV) 
        n = n._<ptr<ir.UnaryExpr>>();
        t = o.markTemp();
        n.X = o.expr(n.X, null);
        o.@out = append(o.@out, n);
        o.cleanTemp(t);
    else if (n.Op() == ir.OCOPY) 
        n = n._<ptr<ir.BinaryExpr>>();
        t = o.markTemp();
        n.X = o.expr(n.X, null);
        n.Y = o.expr(n.Y, null);
        o.@out = append(o.@out, n);
        o.cleanTemp(t);
    else if (n.Op() == ir.OPRINT || n.Op() == ir.OPRINTN || n.Op() == ir.ORECOVER) 
        n = n._<ptr<ir.CallExpr>>();
        t = o.markTemp();
        o.exprList(n.Args);
        o.@out = append(o.@out, n);
        o.cleanTemp(t); 

        // Special: order arguments to inner call but not call itself.
    else if (n.Op() == ir.ODEFER || n.Op() == ir.OGO) 
        n = n._<ptr<ir.GoDeferStmt>>();
        t = o.markTemp();
        o.init(n.Call);
        o.call(n.Call);
        if (n.Call.Op() == ir.ORECOVER) { 
            // Special handling of "defer recover()". We need to evaluate the FP
            // argument before wrapping.
            ref ir.Nodes init = ref heap(out ptr<ir.Nodes> _addr_init);
            n.Call = walkRecover(n.Call._<ptr<ir.CallExpr>>(), _addr_init);
            o.stmtList(init);

        }
        if (buildcfg.Experiment.RegabiDefer) {
            o.wrapGoDefer(n);
        }
        o.@out = append(o.@out, n);
        o.cleanTemp(t);
    else if (n.Op() == ir.ODELETE) 
        n = n._<ptr<ir.CallExpr>>();
        t = o.markTemp();
        n.Args[0] = o.expr(n.Args[0], null);
        n.Args[1] = o.expr(n.Args[1], null);
        n.Args[1] = o.mapKeyTemp(n.Args[0].Type(), n.Args[1]);
        o.@out = append(o.@out, n);
        o.cleanTemp(t); 

        // Clean temporaries from condition evaluation at
        // beginning of loop body and after for statement.
    else if (n.Op() == ir.OFOR) 
        n = n._<ptr<ir.ForStmt>>();
        t = o.markTemp();
        n.Cond = o.exprInPlace(n.Cond);
        n.Body.Prepend(o.cleanTempNoPop(t));
        orderBlock(_addr_n.Body, o.free);
        n.Post = orderStmtInPlace(n.Post, o.free);
        o.@out = append(o.@out, n);
        o.cleanTemp(t); 

        // Clean temporaries from condition at
        // beginning of both branches.
    else if (n.Op() == ir.OIF) 
        n = n._<ptr<ir.IfStmt>>();
        t = o.markTemp();
        n.Cond = o.exprInPlace(n.Cond);
        n.Body.Prepend(o.cleanTempNoPop(t));
        n.Else.Prepend(o.cleanTempNoPop(t));
        o.popTemp(t);
        orderBlock(_addr_n.Body, o.free);
        orderBlock(_addr_n.Else, o.free);
        o.@out = append(o.@out, n);
    else if (n.Op() == ir.OPANIC) 
        n = n._<ptr<ir.UnaryExpr>>();
        t = o.markTemp();
        n.X = o.expr(n.X, null);
        if (!n.X.Type().IsEmptyInterface()) {
            @base.FatalfAt(n.Pos(), "bad argument to panic: %L", n.X);
        }
        o.@out = append(o.@out, n);
        o.cleanTemp(t);
    else if (n.Op() == ir.ORANGE) 
        // n.Right is the expression being ranged over.
        // order it, and then make a copy if we need one.
        // We almost always do, to ensure that we don't
        // see any value changes made during the loop.
        // Usually the copy is cheap (e.g., array pointer,
        // chan, slice, string are all tiny).
        // The exception is ranging over an array value
        // (not a slice, not a pointer to array),
        // which must make a copy to avoid seeing updates made during
        // the range body. Ranging over an array value is uncommon though.

        // Mark []byte(str) range expression to reuse string backing storage.
        // It is safe because the storage cannot be mutated.
        n = n._<ptr<ir.RangeStmt>>();
        if (n.X.Op() == ir.OSTR2BYTES) {
            n.X._<ptr<ir.ConvExpr>>().SetOp(ir.OSTR2BYTESTMP);
        }
        t = o.markTemp();
        n.X = o.expr(n.X, null);

        var orderBody = true;
        var xt = typecheck.RangeExprType(n.X.Type());

        if (xt.Kind() == types.TARRAY || xt.Kind() == types.TSLICE)
        {
            if (n.Value == null || ir.IsBlank(n.Value)) { 
                // for i := range x will only use x once, to compute len(x).
                // No need to copy it.
                break;

            }

            fallthrough = true;

        }
        if (fallthrough || xt.Kind() == types.TCHAN || xt.Kind() == types.TSTRING) 
        {
            // chan, string, slice, array ranges use value multiple times.
            // make copy.
            r = n.X;

            if (r.Type().IsString() && r.Type() != types.Types[types.TSTRING]) {
                r = ir.NewConvExpr(@base.Pos, ir.OCONV, null, r);
                r.SetType(types.Types[types.TSTRING]);
                r = typecheck.Expr(r);
            }
            n.X = o.copyExpr(r);
            goto __switch_break0;
        }
        if (xt.Kind() == types.TMAP)
        {
            if (isMapClear(n)) { 
                // Preserve the body of the map clear pattern so it can
                // be detected during walk. The loop body will not be used
                // when optimizing away the range loop to a runtime call.
                orderBody = false;
                break;

            } 

            // copy the map value in case it is a map literal.
            // TODO(rsc): Make tmp = literal expressions reuse tmp.
            // For maps tmp is just one word so it hardly matters.
            r = n.X;
            n.X = o.copyExpr(r); 

            // n.Prealloc is the temp for the iterator.
            // MapIterType contains pointers and needs to be zeroed.
            n.Prealloc = o.newTemp(reflectdata.MapIterType(xt), true);
            goto __switch_break0;
        }
        // default: 
            @base.Fatalf("order.stmt range %v", n.Type());

        __switch_break0:;
        n.Key = o.exprInPlace(n.Key);
        n.Value = o.exprInPlace(n.Value);
        if (orderBody) {
            orderBlock(_addr_n.Body, o.free);
        }
        o.@out = append(o.@out, n);
        o.cleanTemp(t);
    else if (n.Op() == ir.ORETURN) 
        n = n._<ptr<ir.ReturnStmt>>();
        o.exprList(n.Results);
        o.@out = append(o.@out, n); 

        // Special: clean case temporaries in each block entry.
        // Select must enter one of its blocks, so there is no
        // need for a cleaning at the end.
        // Doubly special: evaluation order for select is stricter
        // than ordinary expressions. Even something like p.c
        // has to be hoisted into a temporary, so that it cannot be
        // reordered after the channel evaluation for a different
        // case (if p were nil, then the timing of the fault would
        // give this away).
    else if (n.Op() == ir.OSELECT) 
        n = n._<ptr<ir.SelectStmt>>();
        t = o.markTemp();
        {
            var ncas__prev1 = ncas;

            foreach (var (_, __ncas) in n.Cases) {
                ncas = __ncas;
                r = ncas.Comm;
                ir.SetPos(ncas); 

                // Append any new body prologue to ninit.
                // The next loop will insert ninit into nbody.
                if (len(ncas.Init()) != 0) {
                    @base.Fatalf("order select ninit");
                }

                if (r == null) {
                    continue;
                }


                if (r.Op() == ir.OSELRECV2) 
                    // case x, ok = <-c
                    r = r._<ptr<ir.AssignListStmt>>();
                    ptr<ir.UnaryExpr> recv = r.Rhs[0]._<ptr<ir.UnaryExpr>>();
                    recv.X = o.expr(recv.X, null);
                    if (!ir.IsAutoTmp(recv.X)) {
                        recv.X = o.copyExpr(recv.X);
                    }
                    init = ir.TakeInit(r);

                    var colas = r.Def;
                    Action<nint, ptr<types.Type>> @do = (i, t) => {
                        n = r.Lhs[i];
                        if (ir.IsBlank(n)) {
                            return ;
                        } 
                        // If this is case x := <-ch or case x, y := <-ch, the case has
                        // the ODCL nodes to declare x and y. We want to delay that
                        // declaration (and possible allocation) until inside the case body.
                        // Delete the ODCL nodes here and recreate them inside the body below.
                        if (colas) {
                            if (len(init) > 0 && init[0].Op() == ir.ODCL && init[0]._<ptr<ir.Decl>>().X == n) {
                                init = init[(int)1..];
                            }
                            var dcl = typecheck.Stmt(ir.NewDecl(@base.Pos, ir.ODCL, n._<ptr<ir.Name>>()));
                            ncas.PtrInit().Append(dcl);
                        }

                        var tmp = o.newTemp(t, t.HasPointers());
                        @as = typecheck.Stmt(ir.NewAssignStmt(@base.Pos, n, typecheck.Conv(tmp, n.Type())));
                        ncas.PtrInit().Append(as);
                        r.Lhs[i] = tmp;

                    }
;
                    do(0, recv.X.Type().Elem());
                    do(1, types.Types[types.TBOOL]);
                    if (len(init) != 0) {
                        ir.DumpList("ninit", r.Init());
                        @base.Fatalf("ninit on select recv");
                    }

                    orderBlock(_addr_ncas.PtrInit(), o.free);
                else if (r.Op() == ir.OSEND) 
                    r = r._<ptr<ir.SendStmt>>();
                    if (len(r.Init()) != 0) {
                        ir.DumpList("ninit", r.Init());
                        @base.Fatalf("ninit on select send");
                    } 

                    // case c <- x
                    // r->left is c, r->right is x, both are always evaluated.
                    r.Chan = o.expr(r.Chan, null);

                    if (!ir.IsAutoTmp(r.Chan)) {
                        r.Chan = o.copyExpr(r.Chan);
                    }

                    r.Value = o.expr(r.Value, null);
                    if (!ir.IsAutoTmp(r.Value)) {
                        r.Value = o.copyExpr(r.Value);
                    }

                else 
                    ir.Dump("select case", r);
                    @base.Fatalf("unknown op in select %v", r.Op());
                
            } 
            // Now that we have accumulated all the temporaries, clean them.
            // Also insert any ninit queued during the previous loop.
            // (The temporary cleaning must follow that ninit work.)

            ncas = ncas__prev1;
        }

        foreach (var (_, cas) in n.Cases) {
            orderBlock(_addr_cas.Body, o.free);
            cas.Body.Prepend(o.cleanTempNoPop(t)); 

            // TODO(mdempsky): Is this actually necessary?
            // walkSelect appears to walk Ninit.
            cas.Body.Prepend(ir.TakeInit(cas));

        }        o.@out = append(o.@out, n);
        o.popTemp(t); 

        // Special: value being sent is passed as a pointer; make it addressable.
    else if (n.Op() == ir.OSEND) 
        n = n._<ptr<ir.SendStmt>>();
        t = o.markTemp();
        n.Chan = o.expr(n.Chan, null);
        n.Value = o.expr(n.Value, null);
        if (@base.Flag.Cfg.Instrumenting) { 
            // Force copying to the stack so that (chan T)(nil) <- x
            // is still instrumented as a read of x.
            n.Value = o.copyExpr(n.Value);

        }
        else
 {
            n.Value = o.addrTemp(n.Value);
        }
        o.@out = append(o.@out, n);
        o.cleanTemp(t); 

        // TODO(rsc): Clean temporaries more aggressively.
        // Note that because walkSwitch will rewrite some of the
        // switch into a binary search, this is not as easy as it looks.
        // (If we ran that code here we could invoke order.stmt on
        // the if-else chain instead.)
        // For now just clean all the temporaries at the end.
        // In practice that's fine.
    else if (n.Op() == ir.OSWITCH) 
        n = n._<ptr<ir.SwitchStmt>>();
        if (@base.Debug.Libfuzzer != 0 && !hasDefaultCase(n)) { 
            // Add empty "default:" case for instrumentation.
            n.Cases = append(n.Cases, ir.NewCaseStmt(@base.Pos, null, null));

        }
        t = o.markTemp();
        n.Tag = o.expr(n.Tag, null);
        {
            var ncas__prev1 = ncas;

            foreach (var (_, __ncas) in n.Cases) {
                ncas = __ncas;
                o.exprListInPlace(ncas.List);
                orderBlock(_addr_ncas.Body, o.free);
            }

            ncas = ncas__prev1;
        }

        o.@out = append(o.@out, n);
        o.cleanTemp(t);
    else 
        @base.Fatalf("order.stmt %v", n.Op());
        @base.Pos = lno;

}

private static bool hasDefaultCase(ptr<ir.SwitchStmt> _addr_n) {
    ref ir.SwitchStmt n = ref _addr_n.val;

    foreach (var (_, ncas) in n.Cases) {
        if (len(ncas.List) == 0) {
            return true;
        }
    }    return false;

}

// exprList orders the expression list l into o.
private static void exprList(this ptr<orderState> _addr_o, ir.Nodes l) {
    ref orderState o = ref _addr_o.val;

    var s = l;
    foreach (var (i) in s) {
        s[i] = o.expr(s[i], null);
    }
}

// exprListInPlace orders the expression list l but saves
// the side effects on the individual expression ninit lists.
private static void exprListInPlace(this ptr<orderState> _addr_o, ir.Nodes l) {
    ref orderState o = ref _addr_o.val;

    var s = l;
    foreach (var (i) in s) {
        s[i] = o.exprInPlace(s[i]);
    }
}

private static ir.Node exprNoLHS(this ptr<orderState> _addr_o, ir.Node n) {
    ref orderState o = ref _addr_o.val;

    return o.expr(n, null);
}

// expr orders a single expression, appending side
// effects to o.out as needed.
// If this is part of an assignment lhs = *np, lhs is given.
// Otherwise lhs == nil. (When lhs != nil it may be possible
// to avoid copying the result of the expression to a temporary.)
// The result of expr MUST be assigned back to n, e.g.
//     n.Left = o.expr(n.Left, lhs)
private static ir.Node expr(this ptr<orderState> _addr_o, ir.Node n, ir.Node lhs) {
    ref orderState o = ref _addr_o.val;

    if (n == null) {
        return n;
    }
    var lno = ir.SetPos(n);
    n = o.expr1(n, lhs);
    @base.Pos = lno;
    return n;

}

private static ir.Node expr1(this ptr<orderState> _addr_o, ir.Node n, ir.Node lhs) {
    ref orderState o = ref _addr_o.val;

    o.init(n);


    if (n.Op() == ir.OADDSTR) 
        ptr<ir.AddStringExpr> n = n._<ptr<ir.AddStringExpr>>();
        o.exprList(n.List);

        if (len(n.List) > 5) {
            var t = types.NewArray(types.Types[types.TSTRING], int64(len(n.List)));
            n.Prealloc = o.newTemp(t, false);
        }
        var hasbyte = false;

        var haslit = false;
        foreach (var (_, n1) in n.List) {
            hasbyte = hasbyte || n1.Op() == ir.OBYTES2STR;
            haslit = haslit || n1.Op() == ir.OLITERAL && len(ir.StringVal(n1)) != 0;
        }        if (haslit && hasbyte) {
            {
                var n2__prev1 = n2;

                foreach (var (_, __n2) in n.List) {
                    n2 = __n2;
                    if (n2.Op() == ir.OBYTES2STR) {
                        ptr<ir.ConvExpr> n2 = n2._<ptr<ir.ConvExpr>>();
                        n2.SetOp(ir.OBYTES2STRTMP);
                    }
                }

                n2 = n2__prev1;
            }
        }
        return n;
    else if (n.Op() == ir.OINDEXMAP) 
        n = n._<ptr<ir.IndexExpr>>();
        n.X = o.expr(n.X, null);
        n.Index = o.expr(n.Index, null);
        var needCopy = false;

        if (!n.Assigned) { 
            // Enforce that any []byte slices we are not copying
            // can not be changed before the map index by forcing
            // the map index to happen immediately following the
            // conversions. See copyExpr a few lines below.
            needCopy = mapKeyReplaceStrConv(n.Index);

            if (@base.Flag.Cfg.Instrumenting) { 
                // Race detector needs the copy.
                needCopy = true;

            }

        }
        n.Index = o.mapKeyTemp(n.X.Type(), n.Index);
        if (needCopy) {
            return o.copyExpr(n);
        }
        return n; 

        // concrete type (not interface) argument might need an addressable
        // temporary to pass to the runtime conversion routine.
    else if (n.Op() == ir.OCONVIFACE) 
        n = n._<ptr<ir.ConvExpr>>();
        n.X = o.expr(n.X, null);
        if (n.X.Type().IsInterface()) {
            return n;
        }
        {
            var (_, _, needsaddr) = convFuncName(n.X.Type(), n.Type());

            if (needsaddr || isStaticCompositeLiteral(n.X)) { 
                // Need a temp if we need to pass the address to the conversion function.
                // We also process static composite literal node here, making a named static global
                // whose address we can put directly in an interface (see OCONVIFACE case in walk).
                n.X = o.addrTemp(n.X);

            }

        }

        return n;
    else if (n.Op() == ir.OCONVNOP) 
        n = n._<ptr<ir.ConvExpr>>();
        if (n.Type().IsKind(types.TUNSAFEPTR) && n.X.Type().IsKind(types.TUINTPTR) && (n.X.Op() == ir.OCALLFUNC || n.X.Op() == ir.OCALLINTER || n.X.Op() == ir.OCALLMETH)) {
            ptr<ir.CallExpr> call = n.X._<ptr<ir.CallExpr>>(); 
            // When reordering unsafe.Pointer(f()) into a separate
            // statement, the conversion and function call must stay
            // together. See golang.org/issue/15329.
            o.init(call);
            o.call(call);
            if (lhs == null || lhs.Op() != ir.ONAME || @base.Flag.Cfg.Instrumenting) {
                return o.copyExpr(n);
            }

        }
        else
 {
            n.X = o.expr(n.X, null);
        }
        return n;
    else if (n.Op() == ir.OANDAND || n.Op() == ir.OOROR) 
        // ... = LHS && RHS
        //
        // var r bool
        // r = LHS
        // if r {       // or !r, for OROR
        //     r = RHS
        // }
        // ... = r

        n = n._<ptr<ir.LogicalExpr>>();
        var r = o.newTemp(n.Type(), false); 

        // Evaluate left-hand side.
        var lhs = o.expr(n.X, null);
        o.@out = append(o.@out, typecheck.Stmt(ir.NewAssignStmt(@base.Pos, r, lhs))); 

        // Evaluate right-hand side, save generated code.
        var saveout = o.@out;
        o.@out = null;
        t = o.markTemp();
        o.edge();
        var rhs = o.expr(n.Y, null);
        o.@out = append(o.@out, typecheck.Stmt(ir.NewAssignStmt(@base.Pos, r, rhs)));
        o.cleanTemp(t);
        var gen = o.@out;
        o.@out = saveout; 

        // If left-hand side doesn't cause a short-circuit, issue right-hand side.
        var nif = ir.NewIfStmt(@base.Pos, r, null, null);
        if (n.Op() == ir.OANDAND) {
            nif.Body = gen;
        }
        else
 {
            nif.Else = gen;
        }
        o.@out = append(o.@out, nif);
        return r;
    else if (n.Op() == ir.OCALLFUNC || n.Op() == ir.OCALLINTER || n.Op() == ir.OCALLMETH || n.Op() == ir.OCAP || n.Op() == ir.OCOMPLEX || n.Op() == ir.OCOPY || n.Op() == ir.OIMAG || n.Op() == ir.OLEN || n.Op() == ir.OMAKECHAN || n.Op() == ir.OMAKEMAP || n.Op() == ir.OMAKESLICE || n.Op() == ir.OMAKESLICECOPY || n.Op() == ir.ONEW || n.Op() == ir.OREAL || n.Op() == ir.ORECOVER || n.Op() == ir.OSTR2BYTES || n.Op() == ir.OSTR2BYTESTMP || n.Op() == ir.OSTR2RUNES) 

        if (isRuneCount(n)) { 
            // len([]rune(s)) is rewritten to runtime.countrunes(s) later.
            ptr<ir.ConvExpr> conv = n._<ptr<ir.UnaryExpr>>().X._<ptr<ir.ConvExpr>>();
            conv.X = o.expr(conv.X, null);

        }
        else
 {
            o.call(n);
        }
        if (lhs == null || lhs.Op() != ir.ONAME || @base.Flag.Cfg.Instrumenting) {
            return o.copyExpr(n);
        }
        return n;
    else if (n.Op() == ir.OAPPEND) 
        // Check for append(x, make([]T, y)...) .
        n = n._<ptr<ir.CallExpr>>();
        if (isAppendOfMake(n)) {
            n.Args[0] = o.expr(n.Args[0], null); // order x
            ptr<ir.MakeExpr> mk = n.Args[1]._<ptr<ir.MakeExpr>>();
            mk.Len = o.expr(mk.Len, null); // order y
        }
        else
 {
            o.exprList(n.Args);
        }
        if (lhs == null || lhs.Op() != ir.ONAME && !ir.SameSafeExpr(lhs, n.Args[0])) {
            return o.copyExpr(n);
        }
        return n;
    else if (n.Op() == ir.OSLICE || n.Op() == ir.OSLICEARR || n.Op() == ir.OSLICESTR || n.Op() == ir.OSLICE3 || n.Op() == ir.OSLICE3ARR) 
        n = n._<ptr<ir.SliceExpr>>();
        n.X = o.expr(n.X, null);
        n.Low = o.cheapExpr(o.expr(n.Low, null));
        n.High = o.cheapExpr(o.expr(n.High, null));
        n.Max = o.cheapExpr(o.expr(n.Max, null));
        if (lhs == null || lhs.Op() != ir.ONAME && !ir.SameSafeExpr(lhs, n.X)) {
            return o.copyExpr(n);
        }
        return n;
    else if (n.Op() == ir.OCLOSURE) 
        n = n._<ptr<ir.ClosureExpr>>();
        if (n.Transient() && len(n.Func.ClosureVars) > 0) {
            n.Prealloc = o.newTemp(typecheck.ClosureType(n), false);
        }
        return n;
    else if (n.Op() == ir.OCALLPART) 
        n = n._<ptr<ir.SelectorExpr>>();
        n.X = o.expr(n.X, null);
        if (n.Transient()) {
            t = typecheck.PartialCallType(n);
            n.Prealloc = o.newTemp(t, false);
        }
        return n;
    else if (n.Op() == ir.OSLICELIT) 
        n = n._<ptr<ir.CompLitExpr>>();
        o.exprList(n.List);
        if (n.Transient()) {
            t = types.NewArray(n.Type().Elem(), n.Len);
            n.Prealloc = o.newTemp(t, false);
        }
        return n;
    else if (n.Op() == ir.ODOTTYPE || n.Op() == ir.ODOTTYPE2) 
        n = n._<ptr<ir.TypeAssertExpr>>();
        n.X = o.expr(n.X, null);
        if (!types.IsDirectIface(n.Type()) || @base.Flag.Cfg.Instrumenting) {
            return o.copyExprClear(n);
        }
        return n;
    else if (n.Op() == ir.ORECV) 
        n = n._<ptr<ir.UnaryExpr>>();
        n.X = o.expr(n.X, null);
        return o.copyExprClear(n);
    else if (n.Op() == ir.OEQ || n.Op() == ir.ONE || n.Op() == ir.OLT || n.Op() == ir.OLE || n.Op() == ir.OGT || n.Op() == ir.OGE) 
        n = n._<ptr<ir.BinaryExpr>>();
        n.X = o.expr(n.X, null);
        n.Y = o.expr(n.Y, null);

        t = n.X.Type();

        if (t.IsString()) 
            // Mark string(byteSlice) arguments to reuse byteSlice backing
            // buffer during conversion. String comparison does not
            // memorize the strings for later use, so it is safe.
            if (n.X.Op() == ir.OBYTES2STR) {
                n.X._<ptr<ir.ConvExpr>>().SetOp(ir.OBYTES2STRTMP);
            }
            if (n.Y.Op() == ir.OBYTES2STR) {
                n.Y._<ptr<ir.ConvExpr>>().SetOp(ir.OBYTES2STRTMP);
            }
        else if (t.IsStruct() || t.IsArray()) 
            // for complex comparisons, we need both args to be
            // addressable so we can pass them to the runtime.
            n.X = o.addrTemp(n.X);
            n.Y = o.addrTemp(n.Y);
                return n;
    else if (n.Op() == ir.OMAPLIT) 
        // Order map by converting:
        //   map[int]int{
        //     a(): b(),
        //     c(): d(),
        //     e(): f(),
        //   }
        // to
        //   m := map[int]int{}
        //   m[a()] = b()
        //   m[c()] = d()
        //   m[e()] = f()
        // Then order the result.
        // Without this special case, order would otherwise compute all
        // the keys and values before storing any of them to the map.
        // See issue 26552.
        n = n._<ptr<ir.CompLitExpr>>();
        var entries = n.List;
        var statics = entries[..(int)0];
        slice<ptr<ir.KeyExpr>> dynamics = default;
        {
            var r__prev1 = r;

            foreach (var (_, __r) in entries) {
                r = __r;
                r = r._<ptr<ir.KeyExpr>>();

                if (!isStaticCompositeLiteral(r.Key) || !isStaticCompositeLiteral(r.Value)) {
                    dynamics = append(dynamics, r);
                    continue;
                } 

                // Recursively ordering some static entries can change them to dynamic;
                // e.g., OCONVIFACE nodes. See #31777.
                r = o.expr(r, null)._<ptr<ir.KeyExpr>>();
                if (!isStaticCompositeLiteral(r.Key) || !isStaticCompositeLiteral(r.Value)) {
                    dynamics = append(dynamics, r);
                    continue;
                }

                statics = append(statics, r);

            }

            r = r__prev1;
        }

        n.List = statics;

        if (len(dynamics) == 0) {
            return n;
        }
        var m = o.newTemp(n.Type(), false);
        var @as = ir.NewAssignStmt(@base.Pos, m, n);
        typecheck.Stmt(as);
        o.stmt(as); 

        // Emit eval+insert of dynamic entries, one at a time.
        {
            var r__prev1 = r;

            foreach (var (_, __r) in dynamics) {
                r = __r;
                @as = ir.NewAssignStmt(@base.Pos, ir.NewIndexExpr(@base.Pos, m, r.Key), r.Value);
                typecheck.Stmt(as); // Note: this converts the OINDEX to an OINDEXMAP
                o.stmt(as);

            }

            r = r__prev1;
        }

        return m;
    else 
        if (o.edit == null) {
            o.edit = o.exprNoLHS; // create closure once
        }
        ir.EditChildren(n, o.edit);
        return n; 

        // Addition of strings turns into a function call.
        // Allocate a temporary to hold the strings.
        // Fewer than 5 strings use direct runtime helpers.
    // No return - type-assertions above. Each case must return for itself.
}

// as2func orders OAS2FUNC nodes. It creates temporaries to ensure left-to-right assignment.
// The caller should order the right-hand side of the assignment before calling order.as2func.
// It rewrites,
//    a, b, a = ...
// as
//    tmp1, tmp2, tmp3 = ...
//    a, b, a = tmp1, tmp2, tmp3
// This is necessary to ensure left to right assignment order.
private static void as2func(this ptr<orderState> _addr_o, ptr<ir.AssignListStmt> _addr_n) {
    ref orderState o = ref _addr_o.val;
    ref ir.AssignListStmt n = ref _addr_n.val;

    var results = n.Rhs[0].Type();
    var @as = ir.NewAssignListStmt(n.Pos(), ir.OAS2, null, null);
    foreach (var (i, nl) in n.Lhs) {
        if (!ir.IsBlank(nl)) {
            var typ = results.Field(i).Type;
            var tmp = o.newTemp(typ, typ.HasPointers());
            n.Lhs[i] = tmp;
            @as.Lhs = append(@as.Lhs, nl);
            @as.Rhs = append(@as.Rhs, tmp);
        }
    }    o.@out = append(o.@out, n);
    o.stmt(typecheck.Stmt(as));

}

// as2ok orders OAS2XXX with ok.
// Just like as2func, this also adds temporaries to ensure left-to-right assignment.
private static void as2ok(this ptr<orderState> _addr_o, ptr<ir.AssignListStmt> _addr_n) {
    ref orderState o = ref _addr_o.val;
    ref ir.AssignListStmt n = ref _addr_n.val;

    var @as = ir.NewAssignListStmt(n.Pos(), ir.OAS2, null, null);

    Action<nint, ptr<types.Type>> @do = (i, typ) => {
        {
            var nl = n.Lhs[i];

            if (!ir.IsBlank(nl)) {
                ir.Node tmp = o.newTemp(typ, typ.HasPointers());
                n.Lhs[i] = tmp;
                @as.Lhs = append(@as.Lhs, nl);
                if (i == 1) { 
                    // The "ok" result is an untyped boolean according to the Go
                    // spec. We need to explicitly convert it to the LHS type in
                    // case the latter is a defined boolean type (#8475).
                    tmp = typecheck.Conv(tmp, nl.Type());

                }

                @as.Rhs = append(@as.Rhs, tmp);

            }

        }

    };

    do(0, n.Rhs[0].Type());
    do(1, types.Types[types.TBOOL]);

    o.@out = append(o.@out, n);
    o.stmt(typecheck.Stmt(as));

}

private static nint wrapGoDefer_prgen = default;

// wrapGoDefer wraps the target of a "go" or "defer" statement with a
// new "function with no arguments" closure. Specifically, it converts
//
//   defer f(x, y)
//
// to
//
//   x1, y1 := x, y
//   defer func() { f(x1, y1) }()
//
// This is primarily to enable a quicker bringup of defers under the
// new register ABI; by doing this conversion, we can simplify the
// code in the runtime that invokes defers on the panic path.
private static void wrapGoDefer(this ptr<orderState> _addr_o, ptr<ir.GoDeferStmt> _addr_n) => func((_, panic, _) => {
    ref orderState o = ref _addr_o.val;
    ref ir.GoDeferStmt n = ref _addr_n.val;

    var call = n.Call;

    ir.Node callX = default; // thing being called
    slice<ir.Node> callArgs = default; // call arguments
    slice<ptr<ir.Name>> keepAlive = default; // KeepAlive list from call, if present

    // A helper to recreate the call within the closure.
    Func<src.XPos, ir.Op, ir.Node, slice<ir.Node>, ir.Node> mkNewCall = default; 

    // Defer calls come in many shapes and sizes; not all of them
    // are ir.CallExpr's. Examine the type to see what we're dealing with.
    switch (call.type()) {
        case ptr<ir.CallExpr> x:
            callX = x.X;
            callArgs = x.Args;
            keepAlive = x.KeepAlive;
            mkNewCall = (pos, op, fun, args) => {
                var newcall = ir.NewCallExpr(pos, op, fun, args);
                newcall.IsDDD = x.IsDDD;
                return ir.Node(newcall);
            }
;
            break;
        case ptr<ir.UnaryExpr> x:
            callArgs = new slice<ir.Node>(new ir.Node[] { x.X });
            mkNewCall = (pos, op, fun, args) => {
                if (len(args) != 1) {
                    panic("internal error, expecting single arg");
                }
                return ir.Node(ir.NewUnaryExpr(pos, op, args[0]));
            }
;
            break;
        case ptr<ir.BinaryExpr> x:
            callArgs = new slice<ir.Node>(new ir.Node[] { x.X, x.Y });
            mkNewCall = (pos, op, fun, args) => {
                if (len(args) != 2) {
                    panic("internal error, expecting two args");
                }
                return ir.Node(ir.NewBinaryExpr(pos, op, args[0], args[1]));
            }
;
            break;
        default:
        {
            var x = call.type();
            panic("unhandled op");
            break;
        } 

        // No need to wrap if called func has no args, no receiver, and no results.
        // However in the case of "defer func() { ... }()" we need to
        // protect against the possibility of directClosureCall rewriting
        // things so that the call does have arguments.
        //
        // Do wrap method calls (OCALLMETH, OCALLINTER), because it has
        // a receiver.
        //
        // Also do wrap builtin functions, because they may be expanded to
        // calls with arguments (e.g. ORECOVER).
        //
        // TODO: maybe not wrap if the called function has no arguments and
        // only in-register results?
    } 

    // No need to wrap if called func has no args, no receiver, and no results.
    // However in the case of "defer func() { ... }()" we need to
    // protect against the possibility of directClosureCall rewriting
    // things so that the call does have arguments.
    //
    // Do wrap method calls (OCALLMETH, OCALLINTER), because it has
    // a receiver.
    //
    // Also do wrap builtin functions, because they may be expanded to
    // calls with arguments (e.g. ORECOVER).
    //
    // TODO: maybe not wrap if the called function has no arguments and
    // only in-register results?
    if (len(callArgs) == 0 && call.Op() == ir.OCALLFUNC && callX.Type().NumResults() == 0) {
        {
            ptr<ir.CallExpr> c__prev2 = c;

            ptr<ir.CallExpr> (c, ok) = call._<ptr<ir.CallExpr>>();

            if (ok && callX != null && callX.Op() == ir.OCLOSURE) {
                ptr<ir.ClosureExpr> cloFunc = callX._<ptr<ir.ClosureExpr>>().Func;
                cloFunc.SetClosureCalled(false);
                c.PreserveClosure = true;
            }

            c = c__prev2;

        }

        return ;

    }
    {
        ptr<ir.CallExpr> c__prev1 = c;

        (c, ok) = call._<ptr<ir.CallExpr>>();

        if (ok) { 
            // To simplify things, turn f(a, b, []T{c, d, e}...) back
            // into f(a, b, c, d, e) -- when the final call is run through the
            // type checker below, it will rebuild the proper slice literal.
            undoVariadic(c);
            callX = c.X;
            callArgs = c.Args;

        }
        c = c__prev1;

    } 

    // This is set to true if the closure we're generating escapes
    // (needs heap allocation).
    Func<bool> cloEscapes = () => {
        if (n.Op() == ir.OGO) { 
            // For "go", assume that all closures escape.
            return true;

        }
        return n.Esc() != ir.EscNever;

    }(); 

    // A helper for making a copy of an argument. Note that it is
    // not safe to use o.copyExpr(arg) if we're putting a
    // reference to the temp into the closure (as opposed to
    // copying it in by value), since in the by-reference case we
    // need a temporary whose lifetime extends to the end of the
    // function (as opposed to being local to the current block or
    // statement being ordered).
    Func<ir.Node, ptr<ir.Name>> mkArgCopy = arg => {
        var t = arg.Type();
        var byval = t.Size() <= 128 || cloEscapes;
        ptr<ir.Name> argCopy;
        if (byval) {
            argCopy = o.copyExpr(arg);
        }
        else
 {
            argCopy = typecheck.Temp(t);
            o.append(ir.NewAssignStmt(@base.Pos, argCopy, arg));
        }
        argCopy.SetByval(byval);
        return argCopy;

    }; 

    // getUnsafeArg looks for an unsafe.Pointer arg that has been
    // previously captured into the call's keepalive list, returning
    // the name node for it if found.
    Func<ir.Node, ptr<ir.Name>> getUnsafeArg = arg => { 
        // Look for uintptr(unsafe.Pointer(name))
        if (arg.Op() != ir.OCONVNOP) {
            return null;
        }
        if (!arg.Type().IsUintptr()) {
            return null;
        }
        if (!arg._<ptr<ir.ConvExpr>>().X.Type().IsUnsafePtr()) {
            return null;
        }
        arg = arg._<ptr<ir.ConvExpr>>().X;
        ptr<ir.Name> (argname, ok) = arg._<ptr<ir.Name>>();
        if (!ok) {
            return null;
        }
        {
            var i__prev1 = i;

            foreach (var (__i) in keepAlive) {
                i = __i;
                if (argname == keepAlive[i]) {
                    return argname;
                }
            }

            i = i__prev1;
        }

        return null;

    }; 

    // Copy the arguments to the function into temps.
    //
    // For calls with uintptr(unsafe.Pointer(...)) args that are being
    // kept alive (see code in (*orderState).call that does this), use
    // the existing arg copy instead of creating a new copy.
    var unsafeArgs = make_slice<ptr<ir.Name>>(len(callArgs));
    var origArgs = callArgs;
    slice<ptr<ir.Name>> newNames = default;
    {
        var i__prev1 = i;

        foreach (var (__i) in callArgs) {
            i = __i;
            var arg = callArgs[i];
            ptr<ir.Name> argname;
            var unsafeArgName = getUnsafeArg(arg);
            if (unsafeArgName != null) { 
                // arg has been copied already, use keepalive copy
                argname = unsafeArgName;
                unsafeArgs[i] = unsafeArgName;

            }
            else
 {
                argname = mkArgCopy(arg);
            }

            newNames = append(newNames, argname);

        }
        i = i__prev1;
    }

    ptr<ir.Name> fnExpr;
    ptr<ir.SelectorExpr> methSelectorExpr;
    if (callX != null) {

        if (callX.Op() == ir.ODOTMETH || callX.Op() == ir.ODOTINTER) 
            // Handle defer of a method call, e.g. "defer v.MyMethod(x, y)"
            ptr<ir.SelectorExpr> n = callX._<ptr<ir.SelectorExpr>>();
            n.X = mkArgCopy(n.X);
            methSelectorExpr = addr(n);
            if (callX.Op() == ir.ODOTINTER) { 
                // Currently for "defer i.M()" if i is nil it panics at the
                // point of defer statement, not when deferred function is called.
                // (I think there is an issue discussing what is the intended
                // behavior but I cannot find it.)
                // We need to do the nil check outside of the wrapper.
                var tab = typecheck.Expr(ir.NewUnaryExpr(@base.Pos, ir.OITAB, n.X));
                var c = ir.NewUnaryExpr(n.Pos(), ir.OCHECKNIL, tab);
                c.SetTypecheck(1);
                o.append(c);

            }

        else if (!(callX.Op() == ir.ONAME && callX._<ptr<ir.Name>>().Class == ir.PFUNC)) 
            // Deal with "defer returnsafunc()(x, y)" (for
            // example) by copying the callee expression.
            fnExpr = mkArgCopy(callX);
            if (callX.Op() == ir.OCLOSURE) { 
                // For "defer func(...)", in addition to copying the
                // closure into a temp, mark it as no longer directly
                // called.
                callX._<ptr<ir.ClosureExpr>>().Func.SetClosureCalled(false);

            }

            }
    slice<ptr<ir.Field>> noFuncArgs = default;
    var noargst = ir.NewFuncType(@base.Pos, null, noFuncArgs, null);
    wrapGoDefer_prgen++;
    var outerfn = ir.CurFunc;
    var wrapname = fmt.Sprintf("%v·dwrap·%d", outerfn, wrapGoDefer_prgen);
    var sym = types.LocalPkg.Lookup(wrapname);
    var fn = typecheck.DeclFunc(sym, noargst);
    fn.SetIsHiddenClosure(true);
    fn.SetWrapper(true); 

    // helper for capturing reference to a var declared in an outer scope.
    Func<src.XPos, ptr<ir.Func>, ptr<ir.Name>, ptr<ir.Name>> capName = (pos, fn, n) => {
        t = n.Type();
        var cv = ir.CaptureName(pos, fn, n);
        cv.SetType(t);
        return typecheck.Expr(cv)._<ptr<ir.Name>>();
    }; 

    // Call args (x1, y1) need to be captured as part of the newly
    // created closure.
    ir.Node newCallArgs = new slice<ir.Node>(new ir.Node[] {  });
    {
        var i__prev1 = i;

        foreach (var (__i) in newNames) {
            i = __i;
            arg = default;
            arg = capName(callArgs[i].Pos(), fn, newNames[i]);
            if (unsafeArgs[i] != null) {
                arg = ir.NewConvExpr(arg.Pos(), origArgs[i].Op(), origArgs[i].Type(), arg);
            }
            newCallArgs = append(newCallArgs, arg);
        }
        i = i__prev1;
    }

    if (fnExpr != null) {
        callX = capName(callX.Pos(), fn, fnExpr);
    }
    if (methSelectorExpr != null) {
        methSelectorExpr.X = capName(callX.Pos(), fn, methSelectorExpr.X._<ptr<ir.Name>>());
    }
    ir.FinishCaptureNames(n.Pos(), outerfn, fn); 

    // This flags a builtin as opposed to a regular call.
    var irregular = (call.Op() != ir.OCALLFUNC && call.Op() != ir.OCALLMETH && call.Op() != ir.OCALLINTER); 

    // Construct new function body:  f(x1, y1)
    var op = ir.OCALL;
    if (irregular) {
        op = call.Op();
    }
    newcall = mkNewCall(call.Pos(), op, callX, newCallArgs); 

    // Type-check the result.
    if (!irregular) {
        typecheck.Call(newcall._<ptr<ir.CallExpr>>());
    }
    else
 {
        typecheck.Stmt(newcall);
    }
    fn.Body = new slice<ir.Node>(new ir.Node[] { newcall });
    typecheck.FinishFuncBody();
    typecheck.Func(fn);
    typecheck.Target.Decls = append(typecheck.Target.Decls, fn); 

    // Create closure expr
    var clo = ir.NewClosureExpr(n.Pos(), fn);
    fn.OClosure = clo;
    clo.SetType(fn.Type()); 

    // Set escape properties for closure.
    if (n.Op() == ir.OGO) { 
        // For "go", assume that the closure is going to escape
        // (with an exception for the runtime, which doesn't
        // permit heap-allocated closures).
        if (@base.Ctxt.Pkgpath != "runtime") {
            clo.SetEsc(ir.EscHeap);
        }
    }
    else
 { 
        // For defer, just use whatever result escape analysis
        // has determined for the defer.
        if (n.Esc() == ir.EscNever) {
            clo.SetTransient(true);
            clo.SetEsc(ir.EscNone);
        }
    }
    var topcall = ir.NewCallExpr(n.Pos(), ir.OCALL, clo, new slice<ir.Node>(new ir.Node[] {  }));
    typecheck.Call(topcall); 

    // Tag the call to insure that directClosureCall doesn't undo our work.
    topcall.PreserveClosure = true;

    fn.SetClosureCalled(false); 

    // Finally, point the defer statement at the newly generated call.
    n.Call = topcall;

});

// isFuncPCIntrinsic returns whether n is a direct call of internal/abi.FuncPCABIxxx functions.
private static bool isFuncPCIntrinsic(ptr<ir.CallExpr> _addr_n) {
    ref ir.CallExpr n = ref _addr_n.val;

    if (n.Op() != ir.OCALLFUNC || n.X.Op() != ir.ONAME) {
        return false;
    }
    ptr<ir.Name> fn = n.X._<ptr<ir.Name>>().Sym();
    return (fn.Name == "FuncPCABI0" || fn.Name == "FuncPCABIInternal") && (fn.Pkg.Path == "internal/abi" || fn.Pkg == types.LocalPkg && @base.Ctxt.Pkgpath == "internal/abi");

}

// isIfaceOfFunc returns whether n is an interface conversion from a direct reference of a func.
private static bool isIfaceOfFunc(ir.Node n) {
    return n.Op() == ir.OCONVIFACE && n._<ptr<ir.ConvExpr>>().X.Op() == ir.ONAME && n._<ptr<ir.ConvExpr>>().X._<ptr<ir.Name>>().Class == ir.PFUNC;
}

} // end walk_package
