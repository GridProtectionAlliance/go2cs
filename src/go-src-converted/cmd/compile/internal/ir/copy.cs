// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ir -- go2cs converted at 2022 March 13 06:00:18 UTC
// import "cmd/compile/internal/ir" ==> using ir = go.cmd.compile.@internal.ir_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ir\copy.go
namespace go.cmd.compile.@internal;

using @base = cmd.compile.@internal.@base_package;
using src = cmd.@internal.src_package;


// A Node may implement the Orig and SetOrig method to
// maintain a pointer to the "unrewritten" form of a Node.
// If a Node does not implement OrigNode, it is its own Orig.
//
// Note that both SepCopy and Copy have definitions compatible
// with a Node that does not implement OrigNode: such a Node
// is its own Orig, and in that case, that's what both want to return
// anyway (SepCopy unconditionally, and Copy only when the input
// is its own Orig as well, but if the output does not implement
// OrigNode, then neither does the input, making the condition true).

using System.ComponentModel;
using System;
public static partial class ir_package {

public partial interface OrigNode {
    Node Orig();
    Node SetOrig(Node _p0);
}

// origNode may be embedded into a Node to make it implement OrigNode.
private partial struct origNode {
    [Description("mknode:\"-\"")]
    public Node orig;
}

private static Node Orig(this ptr<origNode> _addr_n) {
    ref origNode n = ref _addr_n.val;

    return n.orig;
}
private static void SetOrig(this ptr<origNode> _addr_n, Node o) {
    ref origNode n = ref _addr_n.val;

    n.orig = o;
}

// Orig returns the “original” node for n.
// If n implements OrigNode, Orig returns n.Orig().
// Otherwise Orig returns n itself.
public static Node Orig(Node n) {
    {
        OrigNode (n, ok) = OrigNode.As(n._<OrigNode>())!;

        if (ok) {
            var o = n.Orig();
            if (o == null) {
                Dump("Orig nil", n);
                @base.Fatalf("Orig returned nil");
            }
            return o;
        }
    }
    return n;
}

// SepCopy returns a separate shallow copy of n,
// breaking any Orig link to any other nodes.
public static Node SepCopy(Node n) {
    n = n.copy();
    {
        OrigNode (n, ok) = OrigNode.As(n._<OrigNode>())!;

        if (ok) {
            n.SetOrig(n);
        }
    }
    return n;
}

// Copy returns a shallow copy of n.
// If Orig(n) == n, then Orig(Copy(n)) == the copy.
// Otherwise the Orig link is preserved as well.
//
// The specific semantics surrounding Orig are subtle but right for most uses.
// See issues #26855 and #27765 for pitfalls.
public static Node Copy(Node n) {
    var c = n.copy();
    {
        OrigNode (n, ok) = OrigNode.As(n._<OrigNode>())!;

        if (ok && n.Orig() == n) {
            c._<OrigNode>().SetOrig(c);
        }
    }
    return c;
}

// DeepCopy returns a “deep” copy of n, with its entire structure copied
// (except for shared nodes like ONAME, ONONAME, OLITERAL, and OTYPE).
// If pos.IsKnown(), it sets the source position of newly allocated Nodes to pos.
public static Node DeepCopy(src.XPos pos, Node n) {
    Func<Node, Node> edit = default;
    edit = x => {

        if (x.Op() == OPACK || x.Op() == ONAME || x.Op() == ONONAME || x.Op() == OLITERAL || x.Op() == ONIL || x.Op() == OTYPE) 
            return x;
                x = Copy(x);
        if (pos.IsKnown()) {
            x.SetPos(pos);
        }
        EditChildren(x, edit);
        return x;
    };
    return edit(n);
}

// DeepCopyList returns a list of deep copies (using DeepCopy) of the nodes in list.
public static slice<Node> DeepCopyList(src.XPos pos, slice<Node> list) {
    slice<Node> @out = default;
    foreach (var (_, n) in list) {
        out = append(out, DeepCopy(pos, n));
    }    return out;
}

} // end ir_package
