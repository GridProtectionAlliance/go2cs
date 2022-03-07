// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types -- go2cs converted at 2022 March 06 22:42:02 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Program Files\Go\src\go\types\initorder.go
using heap = go.container.heap_package;
using fmt = go.fmt_package;

namespace go.go;

public static partial class types_package {

    // initOrder computes the Info.InitOrder for package variables.
private static void initOrder(this ptr<Checker> _addr_check) {
    ref Checker check = ref _addr_check.val;
 
    // An InitOrder may already have been computed if a package is
    // built from several calls to (*Checker).Files. Clear it.
    check.Info.InitOrder = check.Info.InitOrder[..(int)0]; 

    // Compute the object dependency graph and initialize
    // a priority queue with the list of graph nodes.
    ref var pq = ref heap(nodeQueue(dependencyGraph(check.objMap)), out ptr<var> _addr_pq);
    heap.Init(_addr_pq);

    const var debug = false;

    if (debug) {
        fmt.Printf("Computing initialization order for %s\n\n", check.pkg);
        fmt.Println("Object dependency graph:");
        {
            var obj__prev1 = obj;

            foreach (var (__obj, __d) in check.objMap) {
                obj = __obj;
                d = __d; 
                // only print objects that may appear in the dependency graph
                {
                    var obj__prev2 = obj;

                    dependency (obj, _) = dependency.As(obj._<dependency>())!;

                    if (obj != null) {
                        if (len(d.deps) > 0) {
                            fmt.Printf("\t%s depends on\n", obj.Name());
                            foreach (var (dep) in d.deps) {
                                fmt.Printf("\t\t%s\n", dep.Name());
                            }
                        else
                        } {
                            fmt.Printf("\t%s has no dependencies\n", obj.Name());
                        }
                    }
                    obj = obj__prev2;

                }

            }
            obj = obj__prev1;
        }

        fmt.Println();

        fmt.Println("Transposed object dependency graph (functions eliminated):");
        {
            var n__prev1 = n;

            foreach (var (_, __n) in pq) {
                n = __n;
                fmt.Printf("\t%s depends on %d nodes\n", n.obj.Name(), n.ndeps);
                {
                    var p__prev2 = p;

                    foreach (var (__p) in n.pred) {
                        p = __p;
                        fmt.Printf("\t\t%s is dependent\n", p.obj.Name());
                    }
                    p = p__prev2;
                }
            }
            n = n__prev1;
        }

        fmt.Println();

        fmt.Println("Processing nodes:");

    }
    var emitted = make_map<ptr<declInfo>, bool>();
    while (len(pq) > 0) { 
        // get the next node
        ptr<graphNode> n = heap.Pop(_addr_pq)._<ptr<graphNode>>();

        if (debug) {
            fmt.Printf("\t%s (src pos %d) depends on %d nodes now\n", n.obj.Name(), n.obj.order(), n.ndeps);
        }
        if (n.ndeps > 0) {
            var cycle = findPath(check.objMap, n.obj, n.obj, make_map<Object, bool>()); 
            // If n.obj is not part of the cycle (e.g., n.obj->b->c->d->c),
            // cycle will be nil. Don't report anything in that case since
            // the cycle is reported when the algorithm gets to an object
            // in the cycle.
            // Furthermore, once an object in the cycle is encountered,
            // the cycle will be broken (dependency count will be reduced
            // below), and so the remaining nodes in the cycle don't trigger
            // another error (unless they are part of multiple cycles).
            if (cycle != null) {
                check.reportCycle(cycle);
            }
        }
        {
            var p__prev2 = p;

            foreach (var (__p) in n.pred) {
                p = __p;
                p.ndeps--;
                heap.Fix(_addr_pq, p.index);
            }
            p = p__prev2;
        }

        ptr<Var> (v, _) = n.obj._<ptr<Var>>();
        var info = check.objMap[v];
        if (v == null || !info.hasInitializer()) {
            continue;
        }
        if (emitted[info]) {
            continue; // initializer already emitted, if any
        }
        emitted[info] = true;

        var infoLhs = info.lhs; // possibly nil (see declInfo.lhs field comment)
        if (infoLhs == null) {
            infoLhs = new slice<ptr<Var>>(new ptr<Var>[] { v });
        }
        ptr<Initializer> init = addr(new Initializer(infoLhs,info.init));
        check.Info.InitOrder = append(check.Info.InitOrder, init);

    }

    if (debug) {
        fmt.Println();
        fmt.Println("Initialization order:");
        {
            ptr<Initializer> init__prev1 = init;

            foreach (var (_, __init) in check.Info.InitOrder) {
                init = __init;
                fmt.Printf("\t%s\n", init);
            }
            init = init__prev1;
        }

        fmt.Println();

    }
}

// findPath returns the (reversed) list of objects []Object{to, ... from}
// such that there is a path of object dependencies from 'from' to 'to'.
// If there is no such path, the result is nil.
private static slice<Object> findPath(map<Object, ptr<declInfo>> objMap, Object from, Object to, map<Object, bool> seen) {
    if (seen[from]) {
        return null;
    }
    seen[from] = true;

    foreach (var (d) in objMap[from].deps) {
        if (d == to) {
            return new slice<Object>(new Object[] { d });
        }
        {
            var P = findPath(objMap, d, to, seen);

            if (P != null) {
                return append(P, d);
            }

        }

    }    return null;

}

// reportCycle reports an error for the given cycle.
private static void reportCycle(this ptr<Checker> _addr_check, slice<Object> cycle) {
    ref Checker check = ref _addr_check.val;

    var obj = cycle[0];
    check.errorf(obj, _InvalidInitCycle, "initialization cycle for %s", obj.Name()); 
    // subtle loop: print cycle[i] for i = 0, n-1, n-2, ... 1 for len(cycle) = n
    for (var i = len(cycle) - 1; i >= 0; i--) {
        check.errorf(obj, _InvalidInitCycle, "\t%s refers to", obj.Name()); // secondary error, \t indented
        obj = cycle[i];

    } 
    // print cycle[0] again to close the cycle
    check.errorf(obj, _InvalidInitCycle, "\t%s", obj.Name());

}

// ----------------------------------------------------------------------------
// Object dependency graph

// A dependency is an object that may be a dependency in an initialization
// expression. Only constants, variables, and functions can be dependencies.
// Constants are here because constant expression cycles are reported during
// initialization order computation.
private partial interface dependency {
    void isDependency();
}

// A graphNode represents a node in the object dependency graph.
// Each node p in n.pred represents an edge p->n, and each node
// s in n.succ represents an edge n->s; with a->b indicating that
// a depends on b.
private partial struct graphNode {
    public dependency obj; // object represented by this node
    public nodeSet pred; // consumers and dependencies of this node (lazily initialized)
    public nodeSet succ; // consumers and dependencies of this node (lazily initialized)
    public nint index; // node index in graph slice/priority queue
    public nint ndeps; // number of outstanding dependencies before this object can be initialized
}

private partial struct nodeSet { // : map<ptr<graphNode>, bool>
}

private static void add(this ptr<nodeSet> _addr_s, ptr<graphNode> _addr_p) {
    ref nodeSet s = ref _addr_s.val;
    ref graphNode p = ref _addr_p.val;

    if (s == null.val) {
        s.val = make(nodeSet);
    }
    (s.val)[p] = true;

}

// dependencyGraph computes the object dependency graph from the given objMap,
// with any function nodes removed. The resulting graph contains only constants
// and variables.
private static slice<ptr<graphNode>> dependencyGraph(map<Object, ptr<declInfo>> objMap) { 
    // M is the dependency (Object) -> graphNode mapping
    var M = make_map<dependency, ptr<graphNode>>();
    {
        var obj__prev1 = obj;

        foreach (var (__obj) in objMap) {
            obj = __obj; 
            // only consider nodes that may be an initialization dependency
            {
                var obj__prev1 = obj;

                dependency (obj, _) = dependency.As(obj._<dependency>())!;

                if (obj != null) {
                    M[obj] = addr(new graphNode(obj:obj));
                }

                obj = obj__prev1;

            }

        }
        obj = obj__prev1;
    }

    {
        var obj__prev1 = obj;
        var n__prev1 = n;

        foreach (var (__obj, __n) in M) {
            obj = __obj;
            n = __n; 
            // for each dependency obj -> d (= deps[i]), create graph edges n->s and s->n
            {
                var d__prev2 = d;

                foreach (var (__d) in objMap[obj].deps) {
                    d = __d; 
                    // only consider nodes that may be an initialization dependency
                    {
                        var d__prev1 = d;

                        dependency (d, _) = dependency.As(d._<dependency>())!;

                        if (d != null) {
                            var d = M[d];
                            n.succ.add(d);
                            d.pred.add(n);
                        }

                        d = d__prev1;

                    }

                }

                d = d__prev2;
            }
        }
        obj = obj__prev1;
        n = n__prev1;
    }

    slice<ptr<graphNode>> G = default;
    {
        var obj__prev1 = obj;
        var n__prev1 = n;

        foreach (var (__obj, __n) in M) {
            obj = __obj;
            n = __n;
            {
                ptr<Func> (_, ok) = obj._<ptr<Func>>();

                if (ok) { 
                    // connect each predecessor p of n with each successor s
                    // and drop the function node (don't collect it in G)
                    foreach (var (p) in n.pred) { 
                        // ignore self-cycles
                        if (p != n) { 
                            // Each successor s of n becomes a successor of p, and
                            // each predecessor p of n becomes a predecessor of s.
                            foreach (var (s) in n.succ) { 
                                // ignore self-cycles
                                if (s != n) {
                                    p.succ.add(s);
                                    s.pred.add(p);
                                    delete(s.pred, n); // remove edge to n
                                }

                            }
                            delete(p.succ, n); // remove edge to n
                        }
                else
                    }

                } { 
                    // collect non-function nodes
                    G = append(G, n);

                }

            }

        }
        obj = obj__prev1;
        n = n__prev1;
    }

    {
        var n__prev1 = n;

        foreach (var (__i, __n) in G) {
            i = __i;
            n = __n;
            n.index = i;
            n.ndeps = len(n.succ);
        }
        n = n__prev1;
    }

    return G;

}

// ----------------------------------------------------------------------------
// Priority queue

// nodeQueue implements the container/heap interface;
// a nodeQueue may be used as a priority queue.
private partial struct nodeQueue { // : slice<ptr<graphNode>>
}

private static nint Len(this nodeQueue a) {
    return len(a);
}

private static void Swap(this nodeQueue a, nint i, nint j) {
    var x = a[i];
    var y = a[j];
    (a[i], a[j]) = (y, x);    (x.index, y.index) = (j, i);
}

private static bool Less(this nodeQueue a, nint i, nint j) {
    var x = a[i];
    var y = a[j]; 
    // nodes are prioritized by number of incoming dependencies (1st key)
    // and source order (2nd key)
    return x.ndeps < y.ndeps || x.ndeps == y.ndeps && x.obj.order() < y.obj.order();

}

private static void Push(this ptr<nodeQueue> _addr_a, object x) => func((_, panic, _) => {
    ref nodeQueue a = ref _addr_a.val;

    panic("unreachable");
});

private static void Pop(this ptr<nodeQueue> _addr_a) {
    ref nodeQueue a = ref _addr_a.val;

    var n = len(a.val);
    var x = (a.val)[n - 1];
    x.index = -1; // for safety
    a.val = (a.val)[..(int)n - 1];
    return x;

}

} // end types_package
