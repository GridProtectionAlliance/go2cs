// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssagen -- go2cs converted at 2022 March 13 06:23:01 UTC
// import "cmd/compile/internal/ssagen" ==> using ssagen = go.cmd.compile.@internal.ssagen_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssagen\nowb.go
namespace go.cmd.compile.@internal;

using bytes = bytes_package;
using fmt = fmt_package;

using @base = cmd.compile.@internal.@base_package;
using ir = cmd.compile.@internal.ir_package;
using typecheck = cmd.compile.@internal.typecheck_package;
using types = cmd.compile.@internal.types_package;
using obj = cmd.@internal.obj_package;
using src = cmd.@internal.src_package;
using System;

public static partial class ssagen_package {

public static void EnableNoWriteBarrierRecCheck() {
    nowritebarrierrecCheck = newNowritebarrierrecChecker();
}

public static void NoWriteBarrierRecCheck() { 
    // Write barriers are now known. Check the
    // call graph.
    nowritebarrierrecCheck.check();
    nowritebarrierrecCheck = null;
}

private static ptr<nowritebarrierrecChecker> nowritebarrierrecCheck;

private partial struct nowritebarrierrecChecker {
    public map<ptr<ir.Func>, slice<nowritebarrierrecCall>> extraCalls; // curfn is the current function during AST walks.
    public ptr<ir.Func> curfn;
}

private partial struct nowritebarrierrecCall {
    public ptr<ir.Func> target; // caller or callee
    public src.XPos lineno; // line of call
}

// newNowritebarrierrecChecker creates a nowritebarrierrecChecker. It
// must be called before walk
private static ptr<nowritebarrierrecChecker> newNowritebarrierrecChecker() {
    ptr<nowritebarrierrecChecker> c = addr(new nowritebarrierrecChecker(extraCalls:make(map[*ir.Func][]nowritebarrierrecCall),)); 

    // Find all systemstack calls and record their targets. In
    // general, flow analysis can't see into systemstack, but it's
    // important to handle it for this check, so we model it
    // directly. This has to happen before transforming closures in walk since
    // it's a lot harder to work out the argument after.
    foreach (var (_, n) in typecheck.Target.Decls) {
        if (n.Op() != ir.ODCLFUNC) {
            continue;
        }
        c.curfn = n._<ptr<ir.Func>>();
        if (c.curfn.ABIWrapper()) { 
            // We only want "real" calls to these
            // functions, not the generated ones within
            // their own ABI wrappers.
            continue;
        }
        ir.Visit(n, c.findExtraCalls);
    }    c.curfn = null;
    return _addr_c!;
}

private static void findExtraCalls(this ptr<nowritebarrierrecChecker> _addr_c, ir.Node nn) {
    ref nowritebarrierrecChecker c = ref _addr_c.val;

    if (nn.Op() != ir.OCALLFUNC) {
        return ;
    }
    ptr<ir.CallExpr> n = nn._<ptr<ir.CallExpr>>();
    if (n.X == null || n.X.Op() != ir.ONAME) {
        return ;
    }
    ptr<ir.Name> fn = n.X._<ptr<ir.Name>>();
    if (fn.Class != ir.PFUNC || fn.Defn == null) {
        return ;
    }
    if (!types.IsRuntimePkg(fn.Sym().Pkg) || fn.Sym().Name != "systemstack") {
        return ;
    }
    ptr<ir.Func> callee;
    var arg = n.Args[0];

    if (arg.Op() == ir.ONAME) 
        arg = arg._<ptr<ir.Name>>();
        callee = arg.Defn._<ptr<ir.Func>>();
    else if (arg.Op() == ir.OCLOSURE) 
        arg = arg._<ptr<ir.ClosureExpr>>();
        callee = arg.Func;
    else 
        @base.Fatalf("expected ONAME or OCLOSURE node, got %+v", arg);
        if (callee.Op() != ir.ODCLFUNC) {
        @base.Fatalf("expected ODCLFUNC node, got %+v", callee);
    }
    c.extraCalls[c.curfn] = append(c.extraCalls[c.curfn], new nowritebarrierrecCall(callee,n.Pos()));
}

// recordCall records a call from ODCLFUNC node "from", to function
// symbol "to" at position pos.
//
// This should be done as late as possible during compilation to
// capture precise call graphs. The target of the call is an LSym
// because that's all we know after we start SSA.
//
// This can be called concurrently for different from Nodes.
private static void recordCall(this ptr<nowritebarrierrecChecker> _addr_c, ptr<ir.Func> _addr_fn, ptr<obj.LSym> _addr_to, src.XPos pos) {
    ref nowritebarrierrecChecker c = ref _addr_c.val;
    ref ir.Func fn = ref _addr_fn.val;
    ref obj.LSym to = ref _addr_to.val;
 
    // We record this information on the *Func so this is concurrent-safe.
    if (fn.NWBRCalls == null) {
        fn.NWBRCalls = @new<ir.SymAndPos>();
    }
    fn.NWBRCalls.val = append(fn.NWBRCalls.val, new ir.SymAndPos(Sym:to,Pos:pos));
}

private static void check(this ptr<nowritebarrierrecChecker> _addr_c) {
    ref nowritebarrierrecChecker c = ref _addr_c.val;
 
    // We walk the call graph as late as possible so we can
    // capture all calls created by lowering, but this means we
    // only get to see the obj.LSyms of calls. symToFunc lets us
    // get back to the ODCLFUNCs.
    var symToFunc = make_map<ptr<obj.LSym>, ptr<ir.Func>>(); 
    // funcs records the back-edges of the BFS call graph walk. It
    // maps from the ODCLFUNC of each function that must not have
    // write barriers to the call that inhibits them. Functions
    // that are directly marked go:nowritebarrierrec are in this
    // map with a zero-valued nowritebarrierrecCall. This also
    // acts as the set of marks for the BFS of the call graph.
    var funcs = make_map<ptr<ir.Func>, nowritebarrierrecCall>(); 
    // q is the queue of ODCLFUNC Nodes to visit in BFS order.
    ir.NameQueue q = default;

    foreach (var (_, n) in typecheck.Target.Decls) {
        if (n.Op() != ir.ODCLFUNC) {
            continue;
        }
        ptr<ir.Func> fn = n._<ptr<ir.Func>>();

        symToFunc[fn.LSym] = fn; 

        // Make nowritebarrierrec functions BFS roots.
        if (fn.Pragma & ir.Nowritebarrierrec != 0) {
            funcs[fn] = new nowritebarrierrecCall();
            q.PushRight(fn.Nname);
        }
        if (fn.Pragma & ir.Nowritebarrier != 0 && fn.WBPos.IsKnown()) {
            @base.ErrorfAt(fn.WBPos, "write barrier prohibited");
        }
    }    Action<ptr<ir.Func>, ptr<ir.Func>, src.XPos> enqueue = (src, target, pos) => {
        if (target.Pragma & ir.Yeswritebarrierrec != 0) { 
            // Don't flow into this function.
            return ;
        }
        {
            var (_, ok) = funcs[target];

            if (ok) { 
                // Already found a path to target.
                return ;
            } 

            // Record the path.

        } 

        // Record the path.
        funcs[target] = new nowritebarrierrecCall(target:src,lineno:pos);
        q.PushRight(target.Nname);
    };
    while (!q.Empty()) {
        fn = q.PopLeft().Func; 

        // Check fn.
        if (fn.WBPos.IsKnown()) {
            ref bytes.Buffer err = ref heap(out ptr<bytes.Buffer> _addr_err);
            var call = funcs[fn];
            while (call.target != null) {
                fmt.Fprintf(_addr_err, "\n\t%v: called by %v", @base.FmtPos(call.lineno), call.target.Nname);
                call = funcs[call.target];
            }

            @base.ErrorfAt(fn.WBPos, "write barrier prohibited by caller; %v%s", fn.Nname, err.String());
            continue;
        }
        {
            var callee__prev2 = callee;

            foreach (var (_, __callee) in c.extraCalls[fn]) {
                callee = __callee;
                enqueue(fn, callee.target, callee.lineno);
            }

            callee = callee__prev2;
        }

        if (fn.NWBRCalls == null) {
            continue;
        }
        {
            var callee__prev2 = callee;

            foreach (var (_, __callee) in fn.NWBRCalls.val) {
                callee = __callee;
                var target = symToFunc[callee.Sym];
                if (target != null) {
                    enqueue(fn, target, callee.Pos);
                }
            }

            callee = callee__prev2;
        }
    }
}

} // end ssagen_package
