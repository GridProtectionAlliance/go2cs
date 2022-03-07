// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package pkginit -- go2cs converted at 2022 March 06 23:13:45 UTC
// import "cmd/compile/internal/pkginit" ==> using pkginit = go.cmd.compile.@internal.pkginit_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\pkginit\initorder.go
using bytes = go.bytes_package;
using heap = go.container.heap_package;
using fmt = go.fmt_package;

using @base = go.cmd.compile.@internal.@base_package;
using ir = go.cmd.compile.@internal.ir_package;
using staticinit = go.cmd.compile.@internal.staticinit_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class pkginit_package {

    // Package initialization
    //
    // Here we implement the algorithm for ordering package-level variable
    // initialization. The spec is written in terms of variable
    // initialization, but multiple variables initialized by a single
    // assignment are handled together, so here we instead focus on
    // ordering initialization assignments. Conveniently, this maps well
    // to how we represent package-level initializations using the Node
    // AST.
    //
    // Assignments are in one of three phases: NotStarted, Pending, or
    // Done. For assignments in the Pending phase, we use Xoffset to
    // record the number of unique variable dependencies whose
    // initialization assignment is not yet Done. We also maintain a
    // "blocking" map that maps assignments back to all of the assignments
    // that depend on it.
    //
    // For example, for an initialization like:
    //
    //     var x = f(a, b, b)
    //     var a, b = g()
    //
    // the "x = f(a, b, b)" assignment depends on two variables (a and b),
    // so its Xoffset will be 2. Correspondingly, the "a, b = g()"
    // assignment's "blocking" entry will have two entries back to x's
    // assignment.
    //
    // Logically, initialization works by (1) taking all NotStarted
    // assignments, calculating their dependencies, and marking them
    // Pending; (2) adding all Pending assignments with Xoffset==0 to a
    // "ready" priority queue (ordered by variable declaration position);
    // and (3) iteratively processing the next Pending assignment from the
    // queue, decreasing the Xoffset of assignments it's blocking, and
    // adding them to the queue if decremented to 0.
    //
    // As an optimization, we actually apply each of these three steps for
    // each assignment. This yields the same order, but keeps queue size
    // down and thus also heap operation costs.

    // Static initialization phase.
    // These values are stored in two bits in Node.flags.
public static readonly var InitNotStarted = iota;
public static readonly var InitDone = 0;
public static readonly var InitPending = 1;


public partial struct InitOrder {
    public map<ir.Node, slice<ir.Node>> blocking; // ready is the queue of Pending initialization assignments
// that are ready for initialization.
    public declOrder ready;
    public map<ir.Node, nint> order;
}

// initOrder computes initialization order for a list l of
// package-level declarations (in declaration order) and outputs the
// corresponding list of statements to include in the init() function
// body.
private static slice<ir.Node> initOrder(slice<ir.Node> l) {
    staticinit.Schedule s = new staticinit.Schedule(Plans:make(map[ir.Node]*staticinit.Plan),Temps:make(map[ir.Node]*ir.Name),);
    InitOrder o = new InitOrder(blocking:make(map[ir.Node][]ir.Node),order:make(map[ir.Node]int),); 

    // Process all package-level assignment in declaration order.
    {
        var n__prev1 = n;

        foreach (var (_, __n) in l) {
            n = __n;

            if (n.Op() == ir.OAS || n.Op() == ir.OAS2DOTTYPE || n.Op() == ir.OAS2FUNC || n.Op() == ir.OAS2MAPR || n.Op() == ir.OAS2RECV) 
                o.processAssign(n);
                o.flushReady(s.StaticInit);
            else if (n.Op() == ir.ODCLCONST || n.Op() == ir.ODCLFUNC || n.Op() == ir.ODCLTYPE)             else 
                @base.Fatalf("unexpected package-level statement: %v", n);
            
        }
        n = n__prev1;
    }

    {
        var n__prev1 = n;

        foreach (var (_, __n) in l) {
            n = __n;

            if (n.Op() == ir.OAS || n.Op() == ir.OAS2DOTTYPE || n.Op() == ir.OAS2FUNC || n.Op() == ir.OAS2MAPR || n.Op() == ir.OAS2RECV) 
                if (o.order[n] != orderDone) { 
                    // If there have already been errors
                    // printed, those errors may have
                    // confused us and there might not be
                    // a loop. Let the user fix those
                    // first.
                    @base.ExitIfErrors();

                    o.findInitLoopAndExit(firstLHS(n), @new<*ir.Name>(), @new<ir.NameSet>());
                    @base.Fatalf("initialization unfinished, but failed to identify loop");

                }

                    }
        n = n__prev1;
    }

    if (len(o.blocking) != 0) {
        @base.Fatalf("expected empty map: %v", o.blocking);
    }
    return s.Out;

}

private static void processAssign(this ptr<InitOrder> _addr_o, ir.Node n) {
    ref InitOrder o = ref _addr_o.val;

    {
        var (_, ok) = o.order[n];

        if (ok) {
            @base.Fatalf("unexpected state: %v, %v", n, o.order[n]);
        }
    }

    o.order[n] = 0; 

    // Compute number of variable dependencies and build the
    // inverse dependency ("blocking") graph.
    foreach (var (dep) in collectDeps(n, true)) {
        var defn = dep.Defn; 
        // Skip dependencies on functions (PFUNC) and
        // variables already initialized (InitDone).
        if (dep.Class != ir.PEXTERN || o.order[defn] == orderDone) {
            continue;
        }
        o.order[n]++;
        o.blocking[defn] = append(o.blocking[defn], n);

    }    if (o.order[n] == 0) {
        heap.Push(_addr_o.ready, n);
    }
}

private static readonly nint orderDone = -1000;

// flushReady repeatedly applies initialize to the earliest (in
// declaration order) assignment ready for initialization and updates
// the inverse dependency ("blocking") graph.


// flushReady repeatedly applies initialize to the earliest (in
// declaration order) assignment ready for initialization and updates
// the inverse dependency ("blocking") graph.
private static void flushReady(this ptr<InitOrder> _addr_o, Action<ir.Node> initialize) {
    ref InitOrder o = ref _addr_o.val;

    while (o.ready.Len() != 0) {
        ir.Node n = heap.Pop(_addr_o.ready)._<ir.Node>();
        {
            var (order, ok) = o.order[n];

            if (!ok || order != 0) {
                @base.Fatalf("unexpected state: %v, %v, %v", n, ok, order);
            }

        }


        initialize(n);
        o.order[n] = orderDone;

        var blocked = o.blocking[n];
        delete(o.blocking, n);

        foreach (var (_, m) in blocked) {
            o.order[m]--;

            if (o.order[m] == 0) {
                heap.Push(_addr_o.ready, m);
            }

        }
    }

}

// findInitLoopAndExit searches for an initialization loop involving variable
// or function n. If one is found, it reports the loop as an error and exits.
//
// path points to a slice used for tracking the sequence of
// variables/functions visited. Using a pointer to a slice allows the
// slice capacity to grow and limit reallocations.
private static void findInitLoopAndExit(this ptr<InitOrder> _addr_o, ptr<ir.Name> _addr_n, ptr<slice<ptr<ir.Name>>> _addr_path, ptr<ir.NameSet> _addr_ok) {
    ref InitOrder o = ref _addr_o.val;
    ref ir.Name n = ref _addr_n.val;
    ref slice<ptr<ir.Name>> path = ref _addr_path.val;
    ref ir.NameSet ok = ref _addr_ok.val;

    foreach (var (i, x) in path.val) {
        if (x == n) {
            reportInitLoopAndExit((path.val)[(int)i..]);
            return ;
        }
    }    var refers = collectDeps(n.Defn, false).Sorted((ni, nj) => {
        return ni.Pos().Before(nj.Pos());
    });

    path.val = append(path.val, n);
    foreach (var (_, ref) in refers) { 
        // Short-circuit variables that were initialized.
        if (@ref.Class == ir.PEXTERN && o.order[@ref.Defn] == orderDone || ok.Has(ref)) {
            continue;
        }
        o.findInitLoopAndExit(ref, path, ok);

    }    ok.Add(n);

    path.val = (path.val)[..(int)len(path.val) - 1];

}

// reportInitLoopAndExit reports and initialization loop as an error
// and exits. However, if l is not actually an initialization loop, it
// simply returns instead.
private static void reportInitLoopAndExit(slice<ptr<ir.Name>> l) { 
    // Rotate loop so that the earliest variable declaration is at
    // the start.
    nint i = -1;
    {
        var n__prev1 = n;

        foreach (var (__j, __n) in l) {
            j = __j;
            n = __n;
            if (n.Class == ir.PEXTERN && (i == -1 || n.Pos().Before(l[i].Pos()))) {
                i = j;
            }
        }
        n = n__prev1;
    }

    if (i == -1) { 
        // False positive: loop only involves recursive
        // functions. Return so that findInitLoop can continue
        // searching.
        return ;

    }
    l = append(l[(int)i..], l[..(int)i]); 

    // TODO(mdempsky): Method values are printed as "T.m-fm"
    // rather than "T.m". Figure out how to avoid that.

    ref bytes.Buffer msg = ref heap(out ptr<bytes.Buffer> _addr_msg);
    fmt.Fprintf(_addr_msg, "initialization loop:\n");
    {
        var n__prev1 = n;

        foreach (var (_, __n) in l) {
            n = __n;
            fmt.Fprintf(_addr_msg, "\t%v: %v refers to\n", ir.Line(n), n);
        }
        n = n__prev1;
    }

    fmt.Fprintf(_addr_msg, "\t%v: %v", ir.Line(l[0]), l[0]);

    @base.ErrorfAt(l[0].Pos(), msg.String());
    @base.ErrorExit();

}

// collectDeps returns all of the package-level functions and
// variables that declaration n depends on. If transitive is true,
// then it also includes the transitive dependencies of any depended
// upon functions (but not variables).
private static ir.NameSet collectDeps(ir.Node n, bool transitive) {
    initDeps d = new initDeps(transitive:transitive);

    if (n.Op() == ir.OAS) 
        ptr<ir.AssignStmt> n = n._<ptr<ir.AssignStmt>>();
        d.inspect(n.Y);
    else if (n.Op() == ir.OAS2DOTTYPE || n.Op() == ir.OAS2FUNC || n.Op() == ir.OAS2MAPR || n.Op() == ir.OAS2RECV) 
        n = n._<ptr<ir.AssignListStmt>>();
        d.inspect(n.Rhs[0]);
    else if (n.Op() == ir.ODCLFUNC) 
        n = n._<ptr<ir.Func>>();
        d.inspectList(n.Body);
    else 
        @base.Fatalf("unexpected Op: %v", n.Op());
        return d.seen;

}

private partial struct initDeps {
    public bool transitive;
    public ir.NameSet seen;
    public Action<ir.Node> cvisit;
}

private static Action<ir.Node> cachedVisit(this ptr<initDeps> _addr_d) {
    ref initDeps d = ref _addr_d.val;

    if (d.cvisit == null) {
        d.cvisit = d.visit; // cache closure
    }
    return d.cvisit;

}

private static void inspect(this ptr<initDeps> _addr_d, ir.Node n) {
    ref initDeps d = ref _addr_d.val;

    ir.Visit(n, d.cachedVisit());
}
private static void inspectList(this ptr<initDeps> _addr_d, ir.Nodes l) {
    ref initDeps d = ref _addr_d.val;

    ir.VisitList(l, d.cachedVisit());
}

// visit calls foundDep on any package-level functions or variables
// referenced by n, if any.
private static void visit(this ptr<initDeps> _addr_d, ir.Node n) {
    ref initDeps d = ref _addr_d.val;


    if (n.Op() == ir.ONAME) 
        ptr<ir.Name> n = n._<ptr<ir.Name>>();

        if (n.Class == ir.PEXTERN || n.Class == ir.PFUNC) 
            d.foundDep(n);
            else if (n.Op() == ir.OCLOSURE) 
        n = n._<ptr<ir.ClosureExpr>>();
        d.inspectList(n.Func.Body);
    else if (n.Op() == ir.ODOTMETH || n.Op() == ir.OCALLPART || n.Op() == ir.OMETHEXPR) 
        d.foundDep(ir.MethodExprName(n));
    
}

// foundDep records that we've found a dependency on n by adding it to
// seen.
private static void foundDep(this ptr<initDeps> _addr_d, ptr<ir.Name> _addr_n) {
    ref initDeps d = ref _addr_d.val;
    ref ir.Name n = ref _addr_n.val;
 
    // Can happen with method expressions involving interface
    // types; e.g., fixedbugs/issue4495.go.
    if (n == null) {
        return ;
    }
    if (n.Defn == null) {
        return ;
    }
    if (d.seen.Has(n)) {
        return ;
    }
    d.seen.Add(n);
    if (d.transitive && n.Class == ir.PFUNC) {
        d.inspectList(n.Defn._<ptr<ir.Func>>().Body);
    }
}

// declOrder implements heap.Interface, ordering assignment statements
// by the position of their first LHS expression.
//
// N.B., the Pos of the first LHS expression is used because because
// an OAS node's Pos may not be unique. For example, given the
// declaration "var a, b = f(), g()", "a" must be ordered before "b",
// but both OAS nodes use the "=" token's position as their Pos.
private partial struct declOrder { // : slice<ir.Node>
}

private static nint Len(this declOrder s) {
    return len(s);
}
private static bool Less(this declOrder s, nint i, nint j) {
    return firstLHS(s[i]).Pos().Before(firstLHS(s[j]).Pos());
}
private static void Swap(this declOrder s, nint i, nint j) {
    (s[i], s[j]) = (s[j], s[i]);
}

private static void Push(this ptr<declOrder> _addr_s, object x) {
    ref declOrder s = ref _addr_s.val;

    s.val = append(s.val, x._<ir.Node>());
}
private static void Pop(this ptr<declOrder> _addr_s) {
    ref declOrder s = ref _addr_s.val;

    var n = (s.val)[len(s.val) - 1];
    s.val = (s.val)[..(int)len(s.val) - 1];
    return n;
}

// firstLHS returns the first expression on the left-hand side of
// assignment n.
private static ptr<ir.Name> firstLHS(ir.Node n) {

    if (n.Op() == ir.OAS) 
        ptr<ir.AssignStmt> n = n._<ptr<ir.AssignStmt>>();
        return _addr_n.X.Name()!;
    else if (n.Op() == ir.OAS2DOTTYPE || n.Op() == ir.OAS2FUNC || n.Op() == ir.OAS2RECV || n.Op() == ir.OAS2MAPR) 
        n = n._<ptr<ir.AssignListStmt>>();
        return _addr_n.Lhs[0].Name()!;
        @base.Fatalf("unexpected Op: %v", n.Op());
    return _addr_null!;

}

} // end pkginit_package
