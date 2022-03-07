// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//
// The inlining facility makes 2 passes: first caninl determines which
// functions are suitable for inlining, and for those that are it
// saves a copy of the body. Then InlineCalls walks each function body to
// expand calls to inlinable functions.
//
// The Debug.l flag controls the aggressiveness. Note that main() swaps level 0 and 1,
// making 1 the default and -l disable. Additional levels (beyond -l) may be buggy and
// are not supported.
//      0: disabled
//      1: 80-nodes leaf functions, oneliners, panic, lazy typechecking (default)
//      2: (unassigned)
//      3: (unassigned)
//      4: allow non-leaf functions
//
// At some point this may get another default and become switch-offable with -N.
//
// The -d typcheckinl flag enables early typechecking of all imported bodies,
// which is useful to flush out bugs.
//
// The Debug.m flag enables diagnostic output.  a single -m is useful for verifying
// which calls get inlined or not, more is for debugging, and may go away at any point.

// package inline -- go2cs converted at 2022 March 06 23:09:20 UTC
// import "cmd/compile/internal/inline" ==> using inline = go.cmd.compile.@internal.inline_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\inline\inl.go
using fmt = go.fmt_package;
using constant = go.go.constant_package;
using strings = go.strings_package;

using @base = go.cmd.compile.@internal.@base_package;
using ir = go.cmd.compile.@internal.ir_package;
using logopt = go.cmd.compile.@internal.logopt_package;
using typecheck = go.cmd.compile.@internal.typecheck_package;
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using src = go.cmd.@internal.src_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class inline_package {

    // Inlining budget parameters, gathered in one place
private static readonly nint inlineMaxBudget = 80;
private static readonly nint inlineExtraAppendCost = 0; 
// default is to inline if there's at most one call. -l=4 overrides this by using 1 instead.
private static readonly nint inlineExtraCallCost = 57; // 57 was benchmarked to provided most benefit with no bad surprises; see https://github.com/golang/go/issues/19348#issuecomment-439370742
private static readonly nint inlineExtraPanicCost = 1; // do not penalize inlining panics.
private static readonly var inlineExtraThrowCost = inlineMaxBudget; // with current (2018-05/1.11) code, inlining runtime.throw does not help.

private static readonly nint inlineBigFunctionNodes = 5000; // Functions with this many nodes are considered "big".
private static readonly nint inlineBigFunctionMaxCost = 20; // Max cost of inlinee when inlining into a "big" function.

// InlinePackage finds functions that can be inlined and clones them before walk expands them.
public static void InlinePackage() {
    ir.VisitFuncsBottomUp(typecheck.Target.Decls, (list, recursive) => {
        var numfns = numNonClosures(list);
        foreach (var (_, n) in list) {
            if (!recursive || numfns > 1) { 
                // We allow inlining if there is no
                // recursion, or the recursion cycle is
                // across more than one function.
                CanInline(_addr_n);

            }
            else
 {
                if (@base.Flag.LowerM > 1) {
                    fmt.Printf("%v: cannot inline %v: recursive\n", ir.Line(n), n.Nname);
                }
            }

            InlineCalls(_addr_n);

        }
    });

}

// CanInline determines whether fn is inlineable.
// If so, CanInline saves copies of fn.Body and fn.Dcl in fn.Inl.
// fn and fn.Body will already have been typechecked.
public static void CanInline(ptr<ir.Func> _addr_fn) => func((defer, _, _) => {
    ref ir.Func fn = ref _addr_fn.val;

    if (fn.Nname == null) {
        @base.Fatalf("CanInline no nname %+v", fn);
    }
    @string reason = default; // reason, if any, that the function was not inlined
    if (@base.Flag.LowerM > 1 || logopt.Enabled()) {
        defer(() => {
            if (reason != "") {
                if (@base.Flag.LowerM > 1) {
                    fmt.Printf("%v: cannot inline %v: %s\n", ir.Line(fn), fn.Nname, reason);
                }
                if (logopt.Enabled()) {
                    logopt.LogOpt(fn.Pos(), "cannotInlineFunction", "inline", ir.FuncName(fn), reason);
                }
            }
        }());
    }
    if (fn.Pragma & ir.Noinline != 0) {
        reason = "marked go:noinline";
        return ;
    }
    if (@base.Flag.Race && fn.Pragma & ir.Norace != 0) {
        reason = "marked go:norace with -race compilation";
        return ;
    }
    if (@base.Debug.Checkptr != 0 && fn.Pragma & ir.NoCheckPtr != 0) {
        reason = "marked go:nocheckptr";
        return ;
    }
    if (fn.Pragma & ir.CgoUnsafeArgs != 0) {
        reason = "marked go:cgo_unsafe_args";
        return ;
    }
    if (fn.Pragma & ir.UintptrEscapes != 0) {
        reason = "marked as having an escaping uintptr argument";
        return ;
    }
    if (fn.Pragma & ir.Yeswritebarrierrec != 0) {
        reason = "marked go:yeswritebarrierrec";
        return ;
    }
    if (len(fn.Body) == 0) {
        reason = "no function body";
        return ;
    }
    if (fn.Typecheck() == 0) {
        @base.Fatalf("CanInline on non-typechecked function %v", fn);
    }
    var n = fn.Nname;
    if (n.Func.InlinabilityChecked()) {
        return ;
    }
    defer(n.Func.SetInlinabilityChecked(true));

    var cc = int32(inlineExtraCallCost);
    if (@base.Flag.LowerL == 4) {
        cc = 1; // this appears to yield better performance than 0.
    }
    ref hairyVisitor visitor = ref heap(new hairyVisitor(budget:inlineMaxBudget,extraCallCost:cc,), out ptr<hairyVisitor> _addr_visitor);
    if (visitor.tooHairy(fn)) {
        reason = visitor.reason;
        return ;
    }
    n.Func.Inl = addr(new ir.Inline(Cost:inlineMaxBudget-visitor.budget,Dcl:pruneUnusedAutos(n.Defn.(*ir.Func).Dcl,&visitor),Body:inlcopylist(fn.Body),));

    if (@base.Flag.LowerM > 1) {
        fmt.Printf("%v: can inline %v with cost %d as: %v { %v }\n", ir.Line(fn), n, inlineMaxBudget - visitor.budget, fn.Type(), ir.Nodes(n.Func.Inl.Body));
    }
    else if (@base.Flag.LowerM != 0) {
        fmt.Printf("%v: can inline %v\n", ir.Line(fn), n);
    }
    if (logopt.Enabled()) {
        logopt.LogOpt(fn.Pos(), "canInlineFunction", "inline", ir.FuncName(fn), fmt.Sprintf("cost: %d", inlineMaxBudget - visitor.budget));
    }
});

// Inline_Flood marks n's inline body for export and recursively ensures
// all called functions are marked too.
public static void Inline_Flood(ptr<ir.Name> _addr_n, Action<ptr<ir.Name>> exportsym) {
    ref ir.Name n = ref _addr_n.val;

    if (n == null) {
        return ;
    }
    if (n.Op() != ir.ONAME || n.Class != ir.PFUNC) {
        @base.Fatalf("Inline_Flood: unexpected %v, %v, %v", n, n.Op(), n.Class);
    }
    var fn = n.Func;
    if (fn == null) {
        @base.Fatalf("Inline_Flood: missing Func on %v", n);
    }
    if (fn.Inl == null) {
        return ;
    }
    if (fn.ExportInline()) {
        return ;
    }
    fn.SetExportInline(true);

    typecheck.ImportedBody(fn);

    Action<ir.Node> doFlood = default;
    doFlood = n => {

        if (n.Op() == ir.OMETHEXPR || n.Op() == ir.ODOTMETH) 
            Inline_Flood(_addr_ir.MethodExprName(n), exportsym);
        else if (n.Op() == ir.ONAME) 
            ptr<ir.Name> n = n._<ptr<ir.Name>>();

            if (n.Class == ir.PFUNC) 
                Inline_Flood(n, exportsym);
                exportsym(n);
            else if (n.Class == ir.PEXTERN) 
                exportsym(n);
                    else if (n.Op() == ir.OCALLPART)         else if (n.Op() == ir.OCLOSURE) 
            // VisitList doesn't visit closure bodies, so force a
            // recursive call to VisitList on the body of the closure.
            ir.VisitList(n._<ptr<ir.ClosureExpr>>().Func.Body, doFlood);
        
    }; 

    // Recursively identify all referenced functions for
    // reexport. We want to include even non-called functions,
    // because after inlining they might be callable.
    ir.VisitList(ir.Nodes(fn.Inl.Body), doFlood);

}

// hairyVisitor visits a function body to determine its inlining
// hairiness and whether or not it can be inlined.
private partial struct hairyVisitor {
    public int budget;
    public @string reason;
    public int extraCallCost;
    public ir.NameSet usedLocals;
    public Func<ir.Node, bool> @do;
}

private static bool tooHairy(this ptr<hairyVisitor> _addr_v, ptr<ir.Func> _addr_fn) {
    ref hairyVisitor v = ref _addr_v.val;
    ref ir.Func fn = ref _addr_fn.val;

    v.@do = v.doNode; // cache closure
    if (ir.DoChildren(fn, v.@do)) {
        return true;
    }
    if (v.budget < 0) {
        v.reason = fmt.Sprintf("function too complex: cost %d exceeds budget %d", inlineMaxBudget - v.budget, inlineMaxBudget);
        return true;
    }
    return false;

}

private static bool doNode(this ptr<hairyVisitor> _addr_v, ir.Node n) {
    ref hairyVisitor v = ref _addr_v.val;

    if (n == null) {
        return false;
    }

    // Call is okay if inlinable and we have the budget for the body.
    if (n.Op() == ir.OCALLFUNC) 
        ptr<ir.CallExpr> n = n._<ptr<ir.CallExpr>>(); 
        // Functions that call runtime.getcaller{pc,sp} can not be inlined
        // because getcaller{pc,sp} expect a pointer to the caller's first argument.
        //
        // runtime.throw is a "cheap call" like panic in normal code.
        if (n.X.Op() == ir.ONAME) {
            ptr<ir.Name> name = n.X._<ptr<ir.Name>>();
            if (name.Class == ir.PFUNC && types.IsRuntimePkg(name.Sym().Pkg)) {
                var fn = name.Sym().Name;
                if (fn == "getcallerpc" || fn == "getcallersp") {
                    v.reason = "call to " + fn;
                    return true;
                }
                if (fn == "throw") {
                    v.budget -= inlineExtraThrowCost;
                    break;
                }
            }
        }
        if (ir.IsIntrinsicCall(n)) { 
            // Treat like any other node.
            break;

        }
        {
            var fn__prev1 = fn;

            fn = inlCallee(n.X);

            if (fn != null && fn.Inl != null) {
                v.budget -= fn.Inl.Cost;
                break;
            } 

            // Call cost for non-leaf inlining.

            fn = fn__prev1;

        } 

        // Call cost for non-leaf inlining.
        v.budget -= v.extraCallCost; 

        // Call is okay if inlinable and we have the budget for the body.
    else if (n.Op() == ir.OCALLMETH) 
        n = n._<ptr<ir.CallExpr>>();
        var t = n.X.Type();
        if (t == null) {
            @base.Fatalf("no function type for [%p] %+v\n", n.X, n.X);
        }
        fn = ir.MethodExprName(n.X).Func;
        if (types.IsRuntimePkg(fn.Sym().Pkg) && fn.Sym().Name == "heapBits.nextArena") { 
            // Special case: explicitly allow
            // mid-stack inlining of
            // runtime.heapBits.next even though
            // it calls slow-path
            // runtime.heapBits.nextArena.
            break;

        }
        if (fn.Inl != null) {
            v.budget -= fn.Inl.Cost;
            break;
        }
        v.budget -= v.extraCallCost; 

        // Things that are too hairy, irrespective of the budget
    else if (n.Op() == ir.OCALL || n.Op() == ir.OCALLINTER) 
        // Call cost for non-leaf inlining.
        v.budget -= v.extraCallCost;
    else if (n.Op() == ir.OPANIC) 
        n = n._<ptr<ir.UnaryExpr>>();
        if (n.X.Op() == ir.OCONVIFACE && n.X._<ptr<ir.ConvExpr>>().Implicit()) { 
            // Hack to keep reflect.flag.mustBe inlinable for TestIntendedInlining.
            // Before CL 284412, these conversions were introduced later in the
            // compiler, so they didn't count against inlining budget.
            v.budget++;

        }
        v.budget -= inlineExtraPanicCost;
    else if (n.Op() == ir.ORECOVER) 
        // recover matches the argument frame pointer to find
        // the right panic value, so it needs an argument frame.
        v.reason = "call to recover";
        return true;
    else if (n.Op() == ir.OCLOSURE) 
        if (@base.Debug.InlFuncsWithClosures == 0) {
            v.reason = "not inlining functions with closures";
            return true;
        }
        v.budget -= 15; 
        // Scan body of closure (which DoChildren doesn't automatically
        // do) to check for disallowed ops in the body and include the
        // body in the budget.
        if (doList(n._<ptr<ir.ClosureExpr>>().Func.Body, v.@do)) {
            return true;
        }
    else if (n.Op() == ir.ORANGE || n.Op() == ir.OSELECT || n.Op() == ir.OGO || n.Op() == ir.ODEFER || n.Op() == ir.ODCLTYPE || n.Op() == ir.OTAILCALL) 
        v.reason = "unhandled op " + n.Op().String();
        return true;
    else if (n.Op() == ir.OAPPEND) 
        v.budget -= inlineExtraAppendCost;
    else if (n.Op() == ir.ODEREF) 
        // *(*X)(unsafe.Pointer(&x)) is low-cost
        n = n._<ptr<ir.StarExpr>>();

        var ptr = n.X;
        while (ptr.Op() == ir.OCONVNOP) {
            ptr = ptr._<ptr<ir.ConvExpr>>().X;
        }
        if (ptr.Op() == ir.OADDR) {
            v.budget += 1; // undo half of default cost of ir.ODEREF+ir.OADDR
        }
    else if (n.Op() == ir.OCONVNOP) 
        // This doesn't produce code, but the children might.
        v.budget++; // undo default cost
    else if (n.Op() == ir.ODCLCONST || n.Op() == ir.OFALL) 
        // These nodes don't produce code; omit from inlining budget.
        return false;
    else if (n.Op() == ir.OFOR || n.Op() == ir.OFORUNTIL) 
        n = n._<ptr<ir.ForStmt>>();
        if (n.Label != null) {
            v.reason = "labeled control";
            return true;
        }
    else if (n.Op() == ir.OSWITCH) 
        n = n._<ptr<ir.SwitchStmt>>();
        if (n.Label != null) {
            v.reason = "labeled control";
            return true;
        }
    else if (n.Op() == ir.OBREAK || n.Op() == ir.OCONTINUE) 
        n = n._<ptr<ir.BranchStmt>>();
        if (n.Label != null) { 
            // Should have short-circuited due to labeled control error above.
            @base.Fatalf("unexpected labeled break/continue: %v", n);

        }
    else if (n.Op() == ir.OIF) 
        n = n._<ptr<ir.IfStmt>>();
        if (ir.IsConst(n.Cond, constant.Bool)) { 
            // This if and the condition cost nothing.
            // TODO(rsc): It seems strange that we visit the dead branch.
            return doList(n.Init(), v.@do) || doList(n.Body, v.@do) || doList(n.Else, v.@do);

        }
    else if (n.Op() == ir.ONAME) 
        n = n._<ptr<ir.Name>>();
        if (n.Class == ir.PAUTO) {
            v.usedLocals.Add(n);
        }
    else if (n.Op() == ir.OBLOCK) 
        // The only OBLOCK we should see at this point is an empty one.
        // In any event, let the visitList(n.List()) below take care of the statements,
        // and don't charge for the OBLOCK itself. The ++ undoes the -- below.
        v.budget++;
    else if (n.Op() == ir.OCALLPART || n.Op() == ir.OSLICELIT) 
        v.budget--; // Hack for toolstash -cmp.
    else if (n.Op() == ir.OMETHEXPR) 
        v.budget++; // Hack for toolstash -cmp.
        v.budget--; 

    // When debugging, don't stop early, to get full cost of inlining this function
    if (v.budget < 0 && @base.Flag.LowerM < 2 && !logopt.Enabled()) {
        v.reason = "too expensive";
        return true;
    }
    return ir.DoChildren(n, v.@do);

}

private static bool isBigFunc(ptr<ir.Func> _addr_fn) {
    ref ir.Func fn = ref _addr_fn.val;

    var budget = inlineBigFunctionNodes;
    return ir.Any(fn, n => {
        budget--;
        return budget <= 0;
    });
}

// inlcopylist (together with inlcopy) recursively copies a list of nodes, except
// that it keeps the same ONAME, OTYPE, and OLITERAL nodes. It is used for copying
// the body and dcls of an inlineable function.
private static slice<ir.Node> inlcopylist(slice<ir.Node> ll) {
    var s = make_slice<ir.Node>(len(ll));
    foreach (var (i, n) in ll) {
        s[i] = inlcopy(n);
    }    return s;
}

// inlcopy is like DeepCopy(), but does extra work to copy closures.
private static ir.Node inlcopy(ir.Node n) {
    Func<ir.Node, ir.Node> edit = default;
    edit = x => {

        if (x.Op() == ir.ONAME || x.Op() == ir.OTYPE || x.Op() == ir.OLITERAL || x.Op() == ir.ONIL) 
            return x;
                var m = ir.Copy(x);
        ir.EditChildren(m, edit);
        if (x.Op() == ir.OCLOSURE) {
            ptr<ir.ClosureExpr> x = x._<ptr<ir.ClosureExpr>>(); 
            // Need to save/duplicate x.Func.Nname,
            // x.Func.Nname.Ntype, x.Func.Dcl, x.Func.ClosureVars, and
            // x.Func.Body for iexport and local inlining.
            var oldfn = x.Func;
            var newfn = ir.NewFunc(oldfn.Pos());
            if (oldfn.ClosureCalled()) {
                newfn.SetClosureCalled(true);
            }

            m._<ptr<ir.ClosureExpr>>().Func = newfn;
            newfn.Nname = ir.NewNameAt(oldfn.Nname.Pos(), oldfn.Nname.Sym()); 
            // XXX OK to share fn.Type() ??
            newfn.Nname.SetType(oldfn.Nname.Type()); 
            // Ntype can be nil for -G=3 mode.
            if (oldfn.Nname.Ntype != null) {
                newfn.Nname.Ntype = inlcopy(oldfn.Nname.Ntype)._<ir.Ntype>();
            }

            newfn.Body = inlcopylist(oldfn.Body); 
            // Make shallow copy of the Dcl and ClosureVar slices
            newfn.Dcl = append((slice<ptr<ir.Name>>)null, oldfn.Dcl);
            newfn.ClosureVars = append((slice<ptr<ir.Name>>)null, oldfn.ClosureVars);

        }
        return m;

    };
    return edit(n);

}

// InlineCalls/inlnode walks fn's statements and expressions and substitutes any
// calls made to inlineable functions. This is the external entry point.
public static void InlineCalls(ptr<ir.Func> _addr_fn) {
    ref ir.Func fn = ref _addr_fn.val;

    var savefn = ir.CurFunc;
    ir.CurFunc = fn;
    var maxCost = int32(inlineMaxBudget);
    if (isBigFunc(_addr_fn)) {
        maxCost = inlineBigFunctionMaxCost;
    }
    var inlMap = make_map<ptr<ir.Func>, bool>();
    Func<ir.Node, ir.Node> edit = default;
    edit = n => {
        return inlnode(n, maxCost, inlMap, edit);
    };
    ir.EditChildren(fn, edit);
    ir.CurFunc = savefn;

}

// Turn an OINLCALL into a statement.
private static ir.Node inlconv2stmt(ptr<ir.InlinedCallExpr> _addr_inlcall) {
    ref ir.InlinedCallExpr inlcall = ref _addr_inlcall.val;

    var n = ir.NewBlockStmt(inlcall.Pos(), null);
    n.List = inlcall.Init();
    n.List.Append(inlcall.Body.Take());
    return n;
}

// Turn an OINLCALL into a single valued expression.
// The result of inlconv2expr MUST be assigned back to n, e.g.
//     n.Left = inlconv2expr(n.Left)
private static ir.Node inlconv2expr(ptr<ir.InlinedCallExpr> _addr_n) {
    ref ir.InlinedCallExpr n = ref _addr_n.val;

    var r = n.ReturnVars[0];
    return ir.InitExpr(append(n.Init(), n.Body), r);
}

// Turn the rlist (with the return values) of the OINLCALL in
// n into an expression list lumping the ninit and body
// containing the inlined statements on the first list element so
// order will be preserved. Used in return, oas2func and call
// statements.
private static slice<ir.Node> inlconv2list(ptr<ir.InlinedCallExpr> _addr_n) {
    ref ir.InlinedCallExpr n = ref _addr_n.val;

    if (n.Op() != ir.OINLCALL || len(n.ReturnVars) == 0) {
        @base.Fatalf("inlconv2list %+v\n", n);
    }
    var s = n.ReturnVars;
    s[0] = ir.InitExpr(append(n.Init(), n.Body), s[0]);
    return s;

}

// inlnode recurses over the tree to find inlineable calls, which will
// be turned into OINLCALLs by mkinlcall. When the recursion comes
// back up will examine left, right, list, rlist, ninit, ntest, nincr,
// nbody and nelse and use one of the 4 inlconv/glue functions above
// to turn the OINLCALL into an expression, a statement, or patch it
// in to this nodes list or rlist as appropriate.
// NOTE it makes no sense to pass the glue functions down the
// recursion to the level where the OINLCALL gets created because they
// have to edit /this/ n, so you'd have to push that one down as well,
// but then you may as well do it here.  so this is cleaner and
// shorter and less complicated.
// The result of inlnode MUST be assigned back to n, e.g.
//     n.Left = inlnode(n.Left)
private static ir.Node inlnode(ir.Node n, int maxCost, map<ptr<ir.Func>, bool> inlMap, Func<ir.Node, ir.Node> edit) {
    if (n == null) {
        return n;
    }

    if (n.Op() == ir.ODEFER || n.Op() == ir.OGO) 
        ptr<ir.GoDeferStmt> n = n._<ptr<ir.GoDeferStmt>>();
        {
            var call__prev2 = call;

            var call = n.Call;


            if (call.Op() == ir.OCALLFUNC || call.Op() == ir.OCALLMETH) 
                call = call._<ptr<ir.CallExpr>>();
                call.NoInline = true;


            call = call__prev2;
        } 

        // TODO do them here (or earlier),
        // so escape analysis can avoid more heapmoves.
    else if (n.Op() == ir.OCLOSURE) 
        return n;
    else if (n.Op() == ir.OCALLMETH) 
        // Prevent inlining some reflect.Value methods when using checkptr,
        // even when package reflect was compiled without it (#35073).
        n = n._<ptr<ir.CallExpr>>();
        {
            var s = ir.MethodExprName(n.X).Sym();

            if (@base.Debug.Checkptr != 0 && types.IsReflectPkg(s.Pkg) && (s.Name == "Value.UnsafeAddr" || s.Name == "Value.Pointer")) {
                return n;
            }

        }

        var lno = ir.SetPos(n);

    ir.EditChildren(n, edit);

    {
        var as__prev1 = as;

        var @as = n;

        if (@as.Op() == ir.OAS2FUNC) {
            @as = as._<ptr<ir.AssignListStmt>>();
            if (@as.Rhs[0].Op() == ir.OINLCALL) {
                @as.Rhs = inlconv2list(@as.Rhs[0]._<ptr<ir.InlinedCallExpr>>());
                @as.SetOp(ir.OAS2);
                @as.SetTypecheck(0);
                n = typecheck.Stmt(as);
            }
        }
        as = as__prev1;

    } 

    // with all the branches out of the way, it is now time to
    // transmogrify this node itself unless inhibited by the
    // switch at the top of this function.

    if (n.Op() == ir.OCALLFUNC || n.Op() == ir.OCALLMETH) 
        n = n._<ptr<ir.CallExpr>>();
        if (n.NoInline) {
            return n;
        }
        call = ;

    if (n.Op() == ir.OCALLFUNC) 
        call = n._<ptr<ir.CallExpr>>();
        if (@base.Flag.LowerM > 3) {
            fmt.Printf("%v:call to func %+v\n", ir.Line(n), call.X);
        }
        if (ir.IsIntrinsicCall(call)) {
            break;
        }
        {
            var fn = inlCallee(call.X);

            if (fn != null && fn.Inl != null) {
                n = mkinlcall(_addr_call, _addr_fn, maxCost, inlMap, edit);
            }

        }


    else if (n.Op() == ir.OCALLMETH) 
        call = n._<ptr<ir.CallExpr>>();
        if (@base.Flag.LowerM > 3) {
            fmt.Printf("%v:call to meth %v\n", ir.Line(n), call.X._<ptr<ir.SelectorExpr>>().Sel);
        }
        if (call.X.Type() == null) {
            @base.Fatalf("no function type for [%p] %+v\n", call.X, call.X);
        }
        n = mkinlcall(_addr_call, _addr_ir.MethodExprName(call.X).Func, maxCost, inlMap, edit);
        @base.Pos = lno;

    if (n.Op() == ir.OINLCALL) {
        ptr<ir.InlinedCallExpr> ic = n._<ptr<ir.InlinedCallExpr>>();

        if (call.Use == ir.CallUseExpr) 
            n = inlconv2expr(ic);
        else if (call.Use == ir.CallUseStmt) 
            n = inlconv2stmt(ic);
        else if (call.Use == ir.CallUseList)         else 
            ir.Dump("call", call);
            @base.Fatalf("call missing use");
        
    }
    return n;

}

// inlCallee takes a function-typed expression and returns the underlying function ONAME
// that it refers to if statically known. Otherwise, it returns nil.
private static ptr<ir.Func> inlCallee(ir.Node fn) {
    fn = ir.StaticValue(fn);

    if (fn.Op() == ir.OMETHEXPR) 
        ptr<ir.SelectorExpr> fn = fn._<ptr<ir.SelectorExpr>>();
        var n = ir.MethodExprName(fn); 
        // Check that receiver type matches fn.X.
        // TODO(mdempsky): Handle implicit dereference
        // of pointer receiver argument?
        if (n == null || !types.Identical(n.Type().Recv().Type, fn.X.Type())) {
            return _addr_null!;
        }
        return _addr_n.Func!;
    else if (fn.Op() == ir.ONAME) 
        fn = fn._<ptr<ir.Name>>();
        if (fn.Class == ir.PFUNC) {
            return _addr_fn.Func!;
        }
    else if (fn.Op() == ir.OCLOSURE) 
        fn = fn._<ptr<ir.ClosureExpr>>();
        var c = fn.Func;
        CanInline(_addr_c);
        return _addr_c!;
        return _addr_null!;

}

private static ir.Node inlParam(ptr<types.Field> _addr_t, ir.InitNode @as, map<ptr<ir.Name>, ptr<ir.Name>> inlvars) {
    ref types.Field t = ref _addr_t.val;

    if (t.Nname == null) {
        return ir.BlankNode;
    }
    ptr<ir.Name> n = t.Nname._<ptr<ir.Name>>();
    if (ir.IsBlank(n)) {
        return ir.BlankNode;
    }
    var inlvar = inlvars[n];
    if (inlvar == null) {
        @base.Fatalf("missing inlvar for %v", n);
    }
    @as.PtrInit().Append(ir.NewDecl(@base.Pos, ir.ODCL, inlvar));
    inlvar.Name().Defn = as;
    return inlvar;

}

private static nint inlgen = default;

// SSADumpInline gives the SSA back end a chance to dump the function
// when producing output for debugging the compiler itself.
public static Action<ptr<ir.Func>> SSADumpInline = _p0 => {
};

// If n is a call node (OCALLFUNC or OCALLMETH), and fn is an ONAME node for a
// function with an inlinable body, return an OINLCALL node that can replace n.
// The returned node's Ninit has the parameter assignments, the Nbody is the
// inlined function body, and (List, Rlist) contain the (input, output)
// parameters.
// The result of mkinlcall MUST be assigned back to n, e.g.
//     n.Left = mkinlcall(n.Left, fn, isddd)
private static ir.Node mkinlcall(ptr<ir.CallExpr> _addr_n, ptr<ir.Func> _addr_fn, int maxCost, map<ptr<ir.Func>, bool> inlMap, Func<ir.Node, ir.Node> edit) => func((defer, _, _) => {
    ref ir.CallExpr n = ref _addr_n.val;
    ref ir.Func fn = ref _addr_fn.val;

    if (fn.Inl == null) {
        if (logopt.Enabled()) {
            logopt.LogOpt(n.Pos(), "cannotInlineCall", "inline", ir.FuncName(ir.CurFunc), fmt.Sprintf("%s cannot be inlined", ir.PkgFuncName(fn)));
        }
        return n;

    }
    if (fn.Inl.Cost > maxCost) { 
        // The inlined function body is too big. Typically we use this check to restrict
        // inlining into very big functions.  See issue 26546 and 17566.
        if (logopt.Enabled()) {
            logopt.LogOpt(n.Pos(), "cannotInlineCall", "inline", ir.FuncName(ir.CurFunc), fmt.Sprintf("cost %d of %s exceeds max large caller cost %d", fn.Inl.Cost, ir.PkgFuncName(fn), maxCost));
        }
        return n;

    }
    if (fn == ir.CurFunc) { 
        // Can't recursively inline a function into itself.
        if (logopt.Enabled()) {
            logopt.LogOpt(n.Pos(), "cannotInlineCall", "inline", fmt.Sprintf("recursive call to %s", ir.FuncName(ir.CurFunc)));
        }
        return n;

    }
    if (@base.Flag.Cfg.Instrumenting && types.IsRuntimePkg(fn.Sym().Pkg)) { 
        // Runtime package must not be instrumented.
        // Instrument skips runtime package. However, some runtime code can be
        // inlined into other packages and instrumented there. To avoid this,
        // we disable inlining of runtime functions when instrumenting.
        // The example that we observed is inlining of LockOSThread,
        // which lead to false race reports on m contents.
        return n;

    }
    if (inlMap[fn]) {
        if (@base.Flag.LowerM > 1) {
            fmt.Printf("%v: cannot inline %v into %v: repeated recursive cycle\n", ir.Line(n), fn, ir.FuncName(ir.CurFunc));
        }
        return n;

    }
    inlMap[fn] = true;
    defer(() => {
        inlMap[fn] = false;
    }());
    if (@base.Debug.TypecheckInl == 0) {
        typecheck.ImportedBody(fn);
    }
    if (@base.Flag.LowerM > 1) {
        fmt.Printf("%v: inlining call to %v %v { %v }\n", ir.Line(n), fn.Sym(), fn.Type(), ir.Nodes(fn.Inl.Body));
    }
    else if (@base.Flag.LowerM != 0) {
        fmt.Printf("%v: inlining call to %v\n", ir.Line(n), fn);
    }
    if (@base.Flag.LowerM > 2) {
        fmt.Printf("%v: Before inlining: %+v\n", ir.Line(n), n);
    }
    SSADumpInline(fn);

    var ninit = n.Init(); 

    // For normal function calls, the function callee expression
    // may contain side effects (e.g., added by addinit during
    // inlconv2expr or inlconv2list). Make sure to preserve these,
    // if necessary (#42703).
    if (n.Op() == ir.OCALLFUNC) {
        var callee = n.X;
        while (callee.Op() == ir.OCONVNOP) {
            ptr<ir.ConvExpr> conv = callee._<ptr<ir.ConvExpr>>();
            ninit.Append(ir.TakeInit(conv));
            callee = conv.X;
        }
        if (callee.Op() != ir.ONAME && callee.Op() != ir.OCLOSURE && callee.Op() != ir.OMETHEXPR) {
            @base.Fatalf("unexpected callee expression: %v", callee);
        }
    }
    var inlvars = make_map<ptr<ir.Name>, ptr<ir.Name>>(); 

    // record formals/locals for later post-processing
    slice<ptr<ir.Name>> inlfvars = default;

    foreach (var (_, ln) in fn.Inl.Dcl) {
        if (ln.Op() != ir.ONAME) {
            continue;
        }
        if (ln.Class == ir.PPARAMOUT) { // return values handled below.
            continue;

        }
        ptr<ir.Name> inlf = typecheck.Expr(inlvar(_addr_ln))._<ptr<ir.Name>>();
        inlvars[ln] = inlf;
        if (@base.Flag.GenDwarfInl > 0) {
            if (ln.Class == ir.PPARAM) {
                inlf.Name().SetInlFormal(true);
            }
            else
 {
                inlf.Name().SetInlLocal(true);
            }

            inlf.SetPos(ln.Pos());
            inlfvars = append(inlfvars, inlf);

        }
    }    var delayretvars = true;

    nint nreturns = 0;
    ir.VisitList(ir.Nodes(fn.Inl.Body), n => {
        {
            ptr<ir.ReturnStmt> n__prev1 = n;

            ptr<ir.ReturnStmt> (n, ok) = n._<ptr<ir.ReturnStmt>>();

            if (ok) {
                nreturns++;
                if (len(n.Results) == 0) {
                    delayretvars = false; // empty return statement (case 2)
                }

            }

            n = n__prev1;

        }

    });

    if (nreturns != 1) {
        delayretvars = false; // not exactly one return statement (case 1)
    }
    slice<ir.Node> retvars = default;
    foreach (var (i, t) in fn.Type().Results().Fields().Slice()) {
        ptr<ir.Name> m;
        {
            var nn = t.Nname;

            if (nn != null && !ir.IsBlank(nn._<ptr<ir.Name>>()) && !strings.HasPrefix(nn.Sym().Name, "~r")) {
                ptr<ir.Name> n = nn._<ptr<ir.Name>>();
                m = inlvar(n);
                m = typecheck.Expr(m)._<ptr<ir.Name>>();
                inlvars[n] = m;
                delayretvars = false; // found a named result parameter (case 3)
            }
            else
 { 
                // anonymous return values, synthesize names for use in assignment that replaces return
                m = retvar(_addr_t, i);

            }

        }


        if (@base.Flag.GenDwarfInl > 0) { 
            // Don't update the src.Pos on a return variable if it
            // was manufactured by the inliner (e.g. "~R2"); such vars
            // were not part of the original callee.
            if (!strings.HasPrefix(m.Sym().Name, "~R")) {
                m.Name().SetInlFormal(true);
                m.SetPos(t.Pos);
                inlfvars = append(inlfvars, m);
            }

        }
        retvars = append(retvars, m);

    }    var @as = ir.NewAssignListStmt(@base.Pos, ir.OAS2, null, null);
    @as.Def = true;
    if (n.Op() == ir.OCALLMETH) {
        ptr<ir.SelectorExpr> sel = n.X._<ptr<ir.SelectorExpr>>();
        if (sel.X == null) {
            @base.Fatalf("method call without receiver: %+v", n);
        }
        @as.Rhs.Append(sel.X);

    }
    @as.Rhs.Append(n.Args); 

    // For non-dotted calls to variadic functions, we assign the
    // variadic parameter's temp name separately.
    ptr<ir.AssignStmt> vas;

    {
        var recv = fn.Type().Recv();

        if (recv != null) {
            @as.Lhs.Append(inlParam(_addr_recv, as, inlvars));
        }
    }

    foreach (var (_, param) in fn.Type().Params().Fields().Slice()) { 
        // For ordinary parameters or variadic parameters in
        // dotted calls, just add the variable to the
        // assignment list, and we're done.
        if (!param.IsDDD() || n.IsDDD) {
            @as.Lhs.Append(inlParam(_addr_param, as, inlvars));
            continue;
        }
        var x = len(@as.Lhs);
        while (len(@as.Lhs) < len(@as.Rhs)) {
            @as.Lhs.Append(argvar(_addr_param.Type, len(@as.Lhs)));
        }
        var varargs = @as.Lhs[(int)x..];

        vas = ir.NewAssignStmt(@base.Pos, null, null);
        vas.X = inlParam(_addr_param, vas, inlvars);
        if (len(varargs) == 0) {
            vas.Y = typecheck.NodNil();
            vas.Y.SetType(param.Type);
        }
        else
 {
            var lit = ir.NewCompLitExpr(@base.Pos, ir.OCOMPLIT, ir.TypeNode(param.Type), null);
            lit.List = varargs;
            vas.Y = lit;
        }
    }    if (len(@as.Rhs) != 0) {
        ninit.Append(typecheck.Stmt(as));
    }
    if (vas != null) {
        ninit.Append(typecheck.Stmt(vas));
    }
    if (!delayretvars) { 
        // Zero the return parameters.
        {
            ptr<ir.ReturnStmt> n__prev1 = n;

            foreach (var (_, __n) in retvars) {
                n = __n;
                ninit.Append(ir.NewDecl(@base.Pos, ir.ODCL, n._<ptr<ir.Name>>()));
                var ras = ir.NewAssignStmt(@base.Pos, n, null);
                ninit.Append(typecheck.Stmt(ras));
            }

            n = n__prev1;
        }
    }
    var retlabel = typecheck.AutoLabel(".i");

    inlgen++;

    nint parent = -1;
    {
        var b = @base.Ctxt.PosTable.Pos(n.Pos()).Base();

        if (b != null) {
            parent = b.InliningIndex();
        }
    }


    var sym = fn.Linksym();
    var newIndex = @base.Ctxt.InlTree.Add(parent, n.Pos(), sym); 

    // Add an inline mark just before the inlined body.
    // This mark is inline in the code so that it's a reasonable spot
    // to put a breakpoint. Not sure if that's really necessary or not
    // (in which case it could go at the end of the function instead).
    // Note issue 28603.
    var inlMark = ir.NewInlineMarkStmt(@base.Pos, types.BADWIDTH);
    inlMark.SetPos(n.Pos().WithIsStmt());
    inlMark.Index = int64(newIndex);
    ninit.Append(inlMark);

    if (@base.Flag.GenDwarfInl > 0) {
        if (!sym.WasInlined()) {
            @base.Ctxt.DwFixups.SetPrecursorFunc(sym, fn);
            sym.Set(obj.AttrWasInlined, true);
        }
    }
    inlsubst subst = new inlsubst(retlabel:retlabel,retvars:retvars,delayretvars:delayretvars,inlvars:inlvars,defnMarker:ir.NilExpr{},bases:make(map[*src.PosBase]*src.PosBase),newInlIndex:newIndex,fn:fn,);
    subst.edit = subst.node;

    var body = subst.list(ir.Nodes(fn.Inl.Body));

    var lab = ir.NewLabelStmt(@base.Pos, retlabel);
    body = append(body, lab);

    if (!typecheck.Go117ExportTypes) {
        typecheck.Stmts(body);
    }
    if (@base.Flag.GenDwarfInl > 0) {
        foreach (var (_, v) in inlfvars) {
            v.SetPos(subst.updatedPos(v.Pos()));
        }
    }
    var call = ir.NewInlinedCallExpr(@base.Pos, null, null);
    call.PtrInit().val = ninit;
    call.Body = body;
    call.ReturnVars = retvars;
    call.SetType(n.Type());
    call.SetTypecheck(1); 

    // transitive inlining
    // might be nice to do this before exporting the body,
    // but can't emit the body with inlining expanded.
    // instead we emit the things that the body needs
    // and each use must redo the inlining.
    // luckily these are small.
    ir.EditChildren(call, edit);

    if (@base.Flag.LowerM > 2) {
        fmt.Printf("%v: After inlining %+v\n\n", ir.Line(call), call);
    }
    return call;

});

// Every time we expand a function we generate a new set of tmpnames,
// PAUTO's in the calling functions, and link them off of the
// PPARAM's, PAUTOS and PPARAMOUTs of the called function.
private static ptr<ir.Name> inlvar(ptr<ir.Name> _addr_var_) {
    ref ir.Name var_ = ref _addr_var_.val;

    if (@base.Flag.LowerM > 3) {
        fmt.Printf("inlvar %+v\n", var_);
    }
    var n = typecheck.NewName(var_.Sym());
    n.SetType(var_.Type());
    n.Class = ir.PAUTO;
    n.SetUsed(true);
    n.Curfn = ir.CurFunc; // the calling function, not the called one
    n.SetAddrtaken(var_.Addrtaken());

    ir.CurFunc.Dcl = append(ir.CurFunc.Dcl, n);
    return _addr_n!;

}

// Synthesize a variable to store the inlined function's results in.
private static ptr<ir.Name> retvar(ptr<types.Field> _addr_t, nint i) {
    ref types.Field t = ref _addr_t.val;

    var n = typecheck.NewName(typecheck.LookupNum("~R", i));
    n.SetType(t.Type);
    n.Class = ir.PAUTO;
    n.SetUsed(true);
    n.Curfn = ir.CurFunc; // the calling function, not the called one
    ir.CurFunc.Dcl = append(ir.CurFunc.Dcl, n);
    return _addr_n!;

}

// Synthesize a variable to store the inlined function's arguments
// when they come from a multiple return call.
private static ir.Node argvar(ptr<types.Type> _addr_t, nint i) {
    ref types.Type t = ref _addr_t.val;

    var n = typecheck.NewName(typecheck.LookupNum("~arg", i));
    n.SetType(t.Elem());
    n.Class = ir.PAUTO;
    n.SetUsed(true);
    n.Curfn = ir.CurFunc; // the calling function, not the called one
    ir.CurFunc.Dcl = append(ir.CurFunc.Dcl, n);
    return n;

}

// The inlsubst type implements the actual inlining of a single
// function call.
private partial struct inlsubst {
    public ptr<types.Sym> retlabel; // Temporary result variables.
    public slice<ir.Node> retvars; // Whether result variables should be initialized at the
// "return" statement.
    public bool delayretvars;
    public map<ptr<ir.Name>, ptr<ir.Name>> inlvars; // defnMarker is used to mark a Node for reassignment.
// inlsubst.clovar set this during creating new ONAME.
// inlsubst.node will set the correct Defn for inlvar.
    public ir.NilExpr defnMarker; // bases maps from original PosBase to PosBase with an extra
// inlined call frame.
    public map<ptr<src.PosBase>, ptr<src.PosBase>> bases; // newInlIndex is the index of the inlined call frame to
// insert for inlined nodes.
    public nint newInlIndex;
    public Func<ir.Node, ir.Node> edit; // cached copy of subst.node method value closure

// If non-nil, we are inside a closure inside the inlined function, and
// newclofn is the Func of the new inlined closure.
    public ptr<ir.Func> newclofn;
    public ptr<ir.Func> fn; // For debug -- the func that is being inlined

// If true, then don't update source positions during substitution
// (retain old source positions).
    public bool noPosUpdate;
}

// list inlines a list of nodes.
private static slice<ir.Node> list(this ptr<inlsubst> _addr_subst, ir.Nodes ll) {
    ref inlsubst subst = ref _addr_subst.val;

    var s = make_slice<ir.Node>(0, len(ll));
    foreach (var (_, n) in ll) {
        s = append(s, subst.node(n));
    }    return s;
}

// fields returns a list of the fields of a struct type representing receiver,
// params, or results, after duplicating the field nodes and substituting the
// Nname nodes inside the field nodes.
private static slice<ptr<types.Field>> fields(this ptr<inlsubst> _addr_subst, ptr<types.Type> _addr_oldt) {
    ref inlsubst subst = ref _addr_subst.val;
    ref types.Type oldt = ref _addr_oldt.val;

    var oldfields = oldt.FieldSlice();
    var newfields = make_slice<ptr<types.Field>>(len(oldfields));
    foreach (var (i) in oldfields) {
        newfields[i] = oldfields[i].Copy();
        if (oldfields[i].Nname != null) {
            newfields[i].Nname = subst.node(oldfields[i].Nname._<ptr<ir.Name>>());
        }
    }    return newfields;

}

// clovar creates a new ONAME node for a local variable or param of a closure
// inside a function being inlined.
private static ptr<ir.Name> clovar(this ptr<inlsubst> _addr_subst, ptr<ir.Name> _addr_n) {
    ref inlsubst subst = ref _addr_subst.val;
    ref ir.Name n = ref _addr_n.val;
 
    // TODO(danscales): want to get rid of this shallow copy, with code like the
    // following, but it is hard to copy all the necessary flags in a maintainable way.
    // m := ir.NewNameAt(n.Pos(), n.Sym())
    // m.Class = n.Class
    // m.SetType(n.Type())
    // m.SetTypecheck(1)
    //if n.IsClosureVar() {
    //    m.SetIsClosureVar(true)
    //}
    ptr<ir.Name> m = addr(new ir.Name());
    m.val = n;
    m.Curfn = subst.newclofn;

    switch (n.Defn.type()) {
        case 
            break;
        case ptr<ir.Name> defn:
            if (!n.IsClosureVar()) {
                @base.FatalfAt(n.Pos(), "want closure variable, got: %+v", n);
            }
            if (n.Sym().Pkg != types.LocalPkg) { 
                // If the closure came from inlining a function from
                // another package, must change package of captured
                // variable to localpkg, so that the fields of the closure
                // struct are local package and can be accessed even if
                // name is not exported. If you disable this code, you can
                // reproduce the problem by running 'go test
                // go/internal/srcimporter'. TODO(mdempsky) - maybe change
                // how we create closure structs?
                m.SetSym(types.LocalPkg.Lookup(n.Sym().Name));

            } 
            // Make sure any inlvar which is the Defn
            // of an ONAME closure var is rewritten
            // during inlining. Don't substitute
            // if Defn node is outside inlined function.
            if (subst.inlvars[n.Defn._<ptr<ir.Name>>()] != null) {
                m.Defn = subst.node(n.Defn);
            }

            break;
        case ptr<ir.AssignStmt> defn:
            m.Defn = _addr_subst.defnMarker;
            break;
        case ptr<ir.AssignListStmt> defn:
            m.Defn = _addr_subst.defnMarker;
            break;
        case ptr<ir.TypeSwitchGuard> defn:
            break;
        default:
        {
            var defn = n.Defn.type();
            @base.FatalfAt(n.Pos(), "unexpected Defn: %+v", defn);
            break;
        }

    }

    if (n.Outer != null) { 
        // Either the outer variable is defined in function being inlined,
        // and we will replace it with the substituted variable, or it is
        // defined outside the function being inlined, and we should just
        // skip the outer variable (the closure variable of the function
        // being inlined).
        ptr<ir.Name> s = subst.node(n.Outer)._<ptr<ir.Name>>();
        if (s == n.Outer) {
            s = n.Outer.Outer;
        }
        m.Outer = s;

    }
    return _addr_m!;

}

// closure does the necessary substitions for a ClosureExpr n and returns the new
// closure node.
private static ir.Node closure(this ptr<inlsubst> _addr_subst, ptr<ir.ClosureExpr> _addr_n) => func((defer, _, _) => {
    ref inlsubst subst = ref _addr_subst.val;
    ref ir.ClosureExpr n = ref _addr_n.val;

    var m = ir.Copy(n); 

    // Prior to the subst edit, set a flag in the inlsubst to
    // indicated that we don't want to update the source positions in
    // the new closure. If we do this, it will appear that the closure
    // itself has things inlined into it, which is not the case. See
    // issue #46234 for more details.
    defer(prev => {
        subst.noPosUpdate = prev;
    }(subst.noPosUpdate));
    subst.noPosUpdate = true;
    ir.EditChildren(m, subst.edit); 

    //fmt.Printf("Inlining func %v with closure into %v\n", subst.fn, ir.FuncName(ir.CurFunc))

    // The following is similar to funcLit
    var oldfn = n.Func;
    var newfn = ir.NewFunc(oldfn.Pos()); 
    // These three lines are not strictly necessary, but just to be clear
    // that new function needs to redo typechecking and inlinability.
    newfn.SetTypecheck(0);
    newfn.SetInlinabilityChecked(false);
    newfn.Inl = null;
    newfn.SetIsHiddenClosure(true);
    newfn.Nname = ir.NewNameAt(n.Pos(), ir.BlankNode.Sym());
    newfn.Nname.Func = newfn; 
    // Ntype can be nil for -G=3 mode.
    if (oldfn.Nname.Ntype != null) {
        newfn.Nname.Ntype = subst.node(oldfn.Nname.Ntype)._<ir.Ntype>();
    }
    newfn.Nname.Defn = newfn;

    m._<ptr<ir.ClosureExpr>>().Func = newfn;
    newfn.OClosure = m._<ptr<ir.ClosureExpr>>();

    if (subst.newclofn != null) { 
        //fmt.Printf("Inlining a closure with a nested closure\n")
    }
    var prevxfunc = subst.newclofn; 

    // Mark that we are now substituting within a closure (within the
    // inlined function), and create new nodes for all the local
    // vars/params inside this closure.
    subst.newclofn = newfn;
    newfn.Dcl = null;
    newfn.ClosureVars = null;
    {
        var oldv__prev1 = oldv;

        foreach (var (_, __oldv) in oldfn.Dcl) {
            oldv = __oldv;
            var newv = subst.clovar(oldv);
            subst.inlvars[oldv] = newv;
            newfn.Dcl = append(newfn.Dcl, newv);
        }
        oldv = oldv__prev1;
    }

    {
        var oldv__prev1 = oldv;

        foreach (var (_, __oldv) in oldfn.ClosureVars) {
            oldv = __oldv;
            newv = subst.clovar(oldv);
            subst.inlvars[oldv] = newv;
            newfn.ClosureVars = append(newfn.ClosureVars, newv);
        }
        oldv = oldv__prev1;
    }

    var oldt = oldfn.Type();
    var newrecvs = subst.fields(oldt.Recvs());
    ptr<types.Field> newrecv;
    if (len(newrecvs) > 0) {
        newrecv = newrecvs[0];
    }
    var newt = types.NewSignature(oldt.Pkg(), newrecv, null, subst.fields(oldt.Params()), subst.fields(oldt.Results()));

    newfn.Nname.SetType(newt);
    newfn.Body = subst.list(oldfn.Body); 

    // Remove the nodes for the current closure from subst.inlvars
    {
        var oldv__prev1 = oldv;

        foreach (var (_, __oldv) in oldfn.Dcl) {
            oldv = __oldv;
            delete(subst.inlvars, oldv);
        }
        oldv = oldv__prev1;
    }

    {
        var oldv__prev1 = oldv;

        foreach (var (_, __oldv) in oldfn.ClosureVars) {
            oldv = __oldv;
            delete(subst.inlvars, oldv);
        }
        oldv = oldv__prev1;
    }

    subst.newclofn = prevxfunc; 

    // Actually create the named function for the closure, now that
    // the closure is inlined in a specific function.
    m.SetTypecheck(0);
    if (oldfn.ClosureCalled()) {
        typecheck.Callee(m);
    }
    else
 {
        typecheck.Expr(m);
    }
    return m;

});

// node recursively copies a node from the saved pristine body of the
// inlined function, substituting references to input/output
// parameters with ones to the tmpnames, and substituting returns with
// assignments to the output.
private static ir.Node node(this ptr<inlsubst> _addr_subst, ir.Node n) {
    ref inlsubst subst = ref _addr_subst.val;

    if (n == null) {
        return null;
    }

    if (n.Op() == ir.ONAME) 
        ptr<ir.Name> n = n._<ptr<ir.Name>>(); 

        // Handle captured variables when inlining closures.
        if (n.IsClosureVar() && subst.newclofn == null) {
            var o = n.Outer; 

            // Deal with case where sequence of closures are inlined.
            // TODO(danscales) - write test case to see if we need to
            // go up multiple levels.
            if (o.Curfn != ir.CurFunc) {
                o = o.Outer;
            } 

            // make sure the outer param matches the inlining location
            if (o == null || o.Curfn != ir.CurFunc) {
                @base.Fatalf("%v: unresolvable capture %v\n", ir.Line(n), n);
            }

            if (@base.Flag.LowerM > 2) {
                fmt.Printf("substituting captured name %+v  ->  %+v\n", n, o);
            }

            return o;

        }
        {
            var inlvar = subst.inlvars[n];

            if (inlvar != null) { // These will be set during inlnode
                if (@base.Flag.LowerM > 2) {
                    fmt.Printf("substituting name %+v  ->  %+v\n", n, inlvar);
                }

                return inlvar;

            }

        }


        if (@base.Flag.LowerM > 2) {
            fmt.Printf("not substituting name %+v\n", n);
        }
        return n;
    else if (n.Op() == ir.OMETHEXPR) 
        n = n._<ptr<ir.SelectorExpr>>();
        return n;
    else if (n.Op() == ir.OLITERAL || n.Op() == ir.ONIL || n.Op() == ir.OTYPE) 
        // If n is a named constant or type, we can continue
        // using it in the inline copy. Otherwise, make a copy
        // so we can update the line number.
        if (n.Sym() != null) {
            return n;
        }
    else if (n.Op() == ir.ORETURN) 
        if (subst.newclofn != null) { 
            // Don't do special substitutions if inside a closure
            break;

        }
        n = n._<ptr<ir.ReturnStmt>>();
        var init = subst.list(n.Init());
        if (len(subst.retvars) != 0 && len(n.Results) != 0) {
            var @as = ir.NewAssignListStmt(@base.Pos, ir.OAS2, null, null); 

            // Make a shallow copy of retvars.
            // Otherwise OINLCALL.Rlist will be the same list,
            // and later walk and typecheck may clobber it.
            {
                ptr<ir.Name> n__prev1 = n;

                foreach (var (_, __n) in subst.retvars) {
                    n = __n;
                    @as.Lhs.Append(n);
                }

                n = n__prev1;
            }

            @as.Rhs = subst.list(n.Results);

            if (subst.delayretvars) {
                {
                    ptr<ir.Name> n__prev1 = n;

                    foreach (var (_, __n) in @as.Lhs) {
                        n = __n;
                        @as.PtrInit().Append(ir.NewDecl(@base.Pos, ir.ODCL, n._<ptr<ir.Name>>()));
                        n.Name().Defn = as;
                    }

                    n = n__prev1;
                }
            }

            init = append(init, typecheck.Stmt(as));

        }
        init = append(init, ir.NewBranchStmt(@base.Pos, ir.OGOTO, subst.retlabel));
        typecheck.Stmts(init);
        return ir.NewBlockStmt(@base.Pos, init);
    else if (n.Op() == ir.OGOTO) 
        if (subst.newclofn != null) { 
            // Don't do special substitutions if inside a closure
            break;

        }
        n = n._<ptr<ir.BranchStmt>>();
        ptr<ir.BranchStmt> m = ir.Copy(n)._<ptr<ir.BranchStmt>>();
        m.SetPos(subst.updatedPos(m.Pos()));
        m.PtrInit().val = null;
        var p = fmt.Sprintf("%s·%d", n.Label.Name, inlgen);
        m.Label = typecheck.Lookup(p);
        return m;
    else if (n.Op() == ir.OLABEL) 
        if (subst.newclofn != null) { 
            // Don't do special substitutions if inside a closure
            break;

        }
        n = n._<ptr<ir.LabelStmt>>();
        m = ir.Copy(n)._<ptr<ir.LabelStmt>>();
        m.SetPos(subst.updatedPos(m.Pos()));
        m.PtrInit().val = null;
        p = fmt.Sprintf("%s·%d", n.Label.Name, inlgen);
        m.Label = typecheck.Lookup(p);
        return m;
    else if (n.Op() == ir.OCLOSURE) 
        return subst.closure(n._<ptr<ir.ClosureExpr>>());
        m = ir.Copy(n);
    m.SetPos(subst.updatedPos(m.Pos()));
    ir.EditChildren(m, subst.edit);

    switch (m.type()) {
        case ptr<ir.AssignStmt> m:
            {
                ptr<ir.Name> lhs__prev1 = lhs;

                ptr<ir.Name> (lhs, ok) = m.X._<ptr<ir.Name>>();

                if (ok && lhs.Defn == _addr_subst.defnMarker) {
                    lhs.Defn = m;
                }

                lhs = lhs__prev1;

            }

            break;
        case ptr<ir.AssignListStmt> m:
            {
                ptr<ir.Name> lhs__prev1 = lhs;

                foreach (var (_, __lhs) in m.Lhs) {
                    lhs = __lhs;
                    {
                        ptr<ir.Name> lhs__prev1 = lhs;

                        (lhs, ok) = lhs._<ptr<ir.Name>>();

                        if (ok && lhs.Defn == _addr_subst.defnMarker) {
                            lhs.Defn = m;
                        }

                        lhs = lhs__prev1;

                    }

                }

                lhs = lhs__prev1;
            }
            break;

    }

    return m;

}

private static src.XPos updatedPos(this ptr<inlsubst> _addr_subst, src.XPos xpos) {
    ref inlsubst subst = ref _addr_subst.val;

    if (subst.noPosUpdate) {
        return xpos;
    }
    var pos = @base.Ctxt.PosTable.Pos(xpos);
    var oldbase = pos.Base(); // can be nil
    var newbase = subst.bases[oldbase];
    if (newbase == null) {
        newbase = src.NewInliningBase(oldbase, subst.newInlIndex);
        subst.bases[oldbase] = newbase;
    }
    pos.SetBase(newbase);
    return @base.Ctxt.PosTable.XPos(pos);

}

private static slice<ptr<ir.Name>> pruneUnusedAutos(slice<ptr<ir.Name>> ll, ptr<hairyVisitor> _addr_vis) {
    ref hairyVisitor vis = ref _addr_vis.val;

    var s = make_slice<ptr<ir.Name>>(0, len(ll));
    foreach (var (_, n) in ll) {
        if (n.Class == ir.PAUTO) {
            if (!vis.usedLocals.Has(n)) {
                continue;
            }
        }
        s = append(s, n);

    }    return s;

}

// numNonClosures returns the number of functions in list which are not closures.
private static nint numNonClosures(slice<ptr<ir.Func>> list) {
    nint count = 0;
    foreach (var (_, fn) in list) {
        if (fn.OClosure == null) {
            count++;
        }
    }    return count;

}

private static bool doList(slice<ir.Node> list, Func<ir.Node, bool> @do) {
    foreach (var (_, x) in list) {
        if (x != null) {
            if (do(x)) {
                return true;
            }
        }
    }    return false;

}

} // end inline_package
