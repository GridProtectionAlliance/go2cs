// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package inspector provides helper functions for traversal over the
// syntax trees of a package, including node filtering by type, and
// materialization of the traversal stack.
//
// During construction, the inspector does a complete traversal and
// builds a list of push/pop events and their node type. Subsequent
// method calls that request a traversal scan this list, rather than walk
// the AST, and perform type filtering using efficient bit sets.
//
// Experiments suggest the inspector's traversals are about 2.5x faster
// than ast.Inspect, but it may take around 5 traversals for this
// benefit to amortize the inspector's construction cost.
// If efficiency is the primary concern, do not use Inspector for
// one-off traversals.
// package inspector -- go2cs converted at 2022 March 06 23:32:55 UTC
// import "golang.org/x/tools/go/ast/inspector" ==> using inspector = go.golang.org.x.tools.go.ast.inspector_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ast\inspector\inspector.go
// There are four orthogonal features in a traversal:
//  1 type filtering
//  2 pruning
//  3 postorder calls to f
//  4 stack
// Rather than offer all of them in the API,
// only a few combinations are exposed:
// - Preorder is the fastest and has fewest features,
//   but is the most commonly needed traversal.
// - Nodes and WithStack both provide pruning and postorder calls,
//   even though few clients need it, because supporting two versions
//   is not justified.
// More combinations could be supported by expressing them as
// wrappers around a more generic traversal, but this was measured
// and found to degrade performance significantly (30%).

using ast = go.go.ast_package;
using System;


namespace go.golang.org.x.tools.go.ast;

public static partial class inspector_package {

    // An Inspector provides methods for inspecting
    // (traversing) the syntax trees of a package.
public partial struct Inspector {
    public slice<event> events;
}

// New returns an Inspector for the specified syntax trees.
public static ptr<Inspector> New(slice<ptr<ast.File>> files) {
    return addr(new Inspector(traverse(files)));
}

// An event represents a push or a pop
// of an ast.Node during a traversal.
private partial struct @event {
    public ast.Node node;
    public ulong typ; // typeOf(node)
    public nint index; // 1 + index of corresponding pop event, or 0 if this is a pop
}

// Preorder visits all the nodes of the files supplied to New in
// depth-first order. It calls f(n) for each node n before it visits
// n's children.
//
// The types argument, if non-empty, enables type-based filtering of
// events. The function f if is called only for nodes whose type
// matches an element of the types slice.
private static void Preorder(this ptr<Inspector> _addr_@in, slice<ast.Node> types, Action<ast.Node> f) {
    ref Inspector @in = ref _addr_@in.val;
 
    // Because it avoids postorder calls to f, and the pruning
    // check, Preorder is almost twice as fast as Nodes. The two
    // features seem to contribute similar slowdowns (~1.4x each).

    var mask = maskOf(types);
    {
        nint i = 0;

        while (i < len(@in.events)) {
            var ev = @in.events[i];
            if (ev.typ & mask != 0) {
                if (ev.index > 0) {
                    f(ev.node);
                }
            }
            i++;
        }
    }

}

// Nodes visits the nodes of the files supplied to New in depth-first
// order. It calls f(n, true) for each node n before it visits n's
// children. If f returns true, Nodes invokes f recursively for each
// of the non-nil children of the node, followed by a call of
// f(n, false).
//
// The types argument, if non-empty, enables type-based filtering of
// events. The function f if is called only for nodes whose type
// matches an element of the types slice.
private static bool Nodes(this ptr<Inspector> _addr_@in, slice<ast.Node> types, Func<ast.Node, bool, bool> f) {
    bool proceed = default;
    ref Inspector @in = ref _addr_@in.val;

    var mask = maskOf(types);
    {
        nint i = 0;

        while (i < len(@in.events)) {
            var ev = @in.events[i];
            if (ev.typ & mask != 0) {
                if (ev.index > 0) { 
                    // push
                    if (!f(ev.node, true)) {
                        i = ev.index; // jump to corresponding pop + 1
                        continue;

                    }

                }
                else
 { 
                    // pop
                    f(ev.node, false);

                }

            }

            i++;

        }
    }

}

// WithStack visits nodes in a similar manner to Nodes, but it
// supplies each call to f an additional argument, the current
// traversal stack. The stack's first element is the outermost node,
// an *ast.File; its last is the innermost, n.
private static bool WithStack(this ptr<Inspector> _addr_@in, slice<ast.Node> types, Func<ast.Node, bool, slice<ast.Node>, bool> f) {
    bool proceed = default;
    ref Inspector @in = ref _addr_@in.val;

    var mask = maskOf(types);
    slice<ast.Node> stack = default;
    {
        nint i = 0;

        while (i < len(@in.events)) {
            var ev = @in.events[i];
            if (ev.index > 0) { 
                // push
                stack = append(stack, ev.node);
                if (ev.typ & mask != 0) {
                    if (!f(ev.node, true, stack)) {
                        i = ev.index;
                        stack = stack[..(int)len(stack) - 1];
                        continue;
                    }
                }

            }
            else
 { 
                // pop
                if (ev.typ & mask != 0) {
                    f(ev.node, false, stack);
                }

                stack = stack[..(int)len(stack) - 1];

            }

            i++;

        }
    }

}

// traverse builds the table of events representing a traversal.
private static slice<event> traverse(slice<ptr<ast.File>> files) { 
    // Preallocate approximate number of events
    // based on source file extent.
    // This makes traverse faster by 4x (!).
    nint extent = default;
    {
        var f__prev1 = f;

        foreach (var (_, __f) in files) {
            f = __f;
            extent += int(f.End() - f.Pos());
        }
        f = f__prev1;
    }

    var capacity = extent * 33 / 100;
    if (capacity > 1e6F) {
        capacity = 1e6F; // impose some reasonable maximum
    }
    var events = make_slice<event>(0, capacity);

    slice<event> stack = default;
    {
        var f__prev1 = f;

        foreach (var (_, __f) in files) {
            f = __f;
            ast.Inspect(f, n => {
                if (n != null) { 
                    // push
                    event ev = new event(node:n,typ:typeOf(n),index:len(events),);
                    stack = append(stack, ev);
                    events = append(events, ev);

                }
                else
 { 
                    // pop
                    ev = stack[len(stack) - 1];
                    stack = stack[..(int)len(stack) - 1];

                    events[ev.index].index = len(events) + 1; // make push refer to pop

                    ev.index = 0; // turn ev into a pop event
                    events = append(events, ev);

                }

                return true;

            });

        }
        f = f__prev1;
    }

    return events;

}

} // end inspector_package
