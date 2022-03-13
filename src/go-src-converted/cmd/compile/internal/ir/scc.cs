// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ir -- go2cs converted at 2022 March 13 06:00:37 UTC
// import "cmd/compile/internal/ir" ==> using ir = go.cmd.compile.@internal.ir_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ir\scc.go
namespace go.cmd.compile.@internal;

using System;
public static partial class ir_package {

// Strongly connected components.
//
// Run analysis on minimal sets of mutually recursive functions
// or single non-recursive functions, bottom up.
//
// Finding these sets is finding strongly connected components
// by reverse topological order in the static call graph.
// The algorithm (known as Tarjan's algorithm) for doing that is taken from
// Sedgewick, Algorithms, Second Edition, p. 482, with two adaptations.
//
// First, a hidden closure function (n.Func.IsHiddenClosure()) cannot be the
// root of a connected component. Refusing to use it as a root
// forces it into the component of the function in which it appears.
// This is more convenient for escape analysis.
//
// Second, each function becomes two virtual nodes in the graph,
// with numbers n and n+1. We record the function's node number as n
// but search from node n+1. If the search tells us that the component
// number (min) is n+1, we know that this is a trivial component: one function
// plus its closures. If the search tells us that the component number is
// n, then there was a path from node n+1 back to node n, meaning that
// the function set is mutually recursive. The escape analysis can be
// more precise when analyzing a single non-recursive function than
// when analyzing a set of mutually recursive functions.

private partial struct bottomUpVisitor {
    public Action<slice<ptr<Func>>, bool> analyze;
    public uint visitgen;
    public map<ptr<Func>, uint> nodeID;
    public slice<ptr<Func>> stack;
}

// VisitFuncsBottomUp invokes analyze on the ODCLFUNC nodes listed in list.
// It calls analyze with successive groups of functions, working from
// the bottom of the call graph upward. Each time analyze is called with
// a list of functions, every function on that list only calls other functions
// on the list or functions that have been passed in previous invocations of
// analyze. Closures appear in the same list as their outer functions.
// The lists are as short as possible while preserving those requirements.
// (In a typical program, many invocations of analyze will be passed just
// a single function.) The boolean argument 'recursive' passed to analyze
// specifies whether the functions on the list are mutually recursive.
// If recursive is false, the list consists of only a single function and its closures.
// If recursive is true, the list may still contain only a single function,
// if that function is itself recursive.
public static void VisitFuncsBottomUp(slice<Node> list, Action<slice<ptr<Func>>, bool> analyze) {
    bottomUpVisitor v = default;
    v.analyze = analyze;
    v.nodeID = make_map<ptr<Func>, uint>();
    {
        var n__prev1 = n;

        foreach (var (_, __n) in list) {
            n = __n;
            if (n.Op() == ODCLFUNC) {
                ptr<Func> n = n._<ptr<Func>>();
                if (!n.IsHiddenClosure()) {
                    v.visit(n);
                }
            }
        }
        n = n__prev1;
    }
}

private static uint visit(this ptr<bottomUpVisitor> _addr_v, ptr<Func> _addr_n) {
    ref bottomUpVisitor v = ref _addr_v.val;
    ref Func n = ref _addr_n.val;

    {
        var id__prev1 = id;

        var id = v.nodeID[n];

        if (id > 0) { 
            // already visited
            return id;
        }
        id = id__prev1;

    }

    v.visitgen++;
    id = v.visitgen;
    v.nodeID[n] = id;
    v.visitgen++;
    var min = v.visitgen;
    v.stack = append(v.stack, n);

    Action<Node> @do = defn => {
        if (defn != null) {
            {
                var m = v.visit(defn._<ptr<Func>>());

                if (m < min) {
                    min = m;
                }

            }
        }
    };

    Visit(n, n => {

        if (n.Op() == ONAME) 
            {
                ptr<Name> n__prev1 = n;

                ptr<Name> n = n._<ptr<Name>>();

                if (n.Class == PFUNC) {
                    do(n.Defn);
                }

                n = n__prev1;

            }
        else if (n.Op() == ODOTMETH || n.Op() == OCALLPART || n.Op() == OMETHEXPR) 
            {
                var fn = MethodExprName(n);

                if (fn != null) {
                    do(fn.Defn);
                }

            }
        else if (n.Op() == OCLOSURE) 
            n = n._<ptr<ClosureExpr>>();
            do(n.Func);
            });

    if ((min == id || min == id + 1) && !n.IsHiddenClosure()) { 
        // This node is the root of a strongly connected component.

        // The original min passed to visitcodelist was v.nodeID[n]+1.
        // If visitcodelist found its way back to v.nodeID[n], then this
        // block is a set of mutually recursive functions.
        // Otherwise it's just a lone function that does not recurse.
        var recursive = min == id; 

        // Remove connected component from stack.
        // Mark walkgen so that future visits return a large number
        // so as not to affect the caller's min.

        nint i = default;
        for (i = len(v.stack) - 1; i >= 0; i--) {
            var x = v.stack[i];
            if (x == n) {
                break;
            }
            v.nodeID[x] = ~uint32(0);
        }
        v.nodeID[n] = ~uint32(0);
        var block = v.stack[(int)i..]; 
        // Run escape analysis on this set of functions.
        v.stack = v.stack[..(int)i];
        v.analyze(block, recursive);
    }
    return min;
}

} // end ir_package
