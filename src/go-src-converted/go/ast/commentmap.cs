// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ast -- go2cs converted at 2022 March 06 22:42:56 UTC
// import "go/ast" ==> using ast = go.go.ast_package
// Original source: C:\Program Files\Go\src\go\ast\commentmap.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using token = go.go.token_package;
using sort = go.sort_package;
using System;


namespace go.go;

public static partial class ast_package {

private partial struct byPos { // : slice<ptr<CommentGroup>>
}

private static nint Len(this byPos a) {
    return len(a);
}
private static bool Less(this byPos a, nint i, nint j) {
    return a[i].Pos() < a[j].Pos();
}
private static void Swap(this byPos a, nint i, nint j) {
    (a[i], a[j]) = (a[j], a[i]);
}

// sortComments sorts the list of comment groups in source order.
//
private static void sortComments(slice<ptr<CommentGroup>> list) { 
    // TODO(gri): Does it make sense to check for sorted-ness
    //            first (because we know that sorted-ness is
    //            very likely)?
    {
        var orderedList = byPos(list);

        if (!sort.IsSorted(orderedList)) {
            sort.Sort(orderedList);
        }
    }

}

// A CommentMap maps an AST node to a list of comment groups
// associated with it. See NewCommentMap for a description of
// the association.
//
public partial struct CommentMap { // : map<Node, slice<ptr<CommentGroup>>>
}

public static void addComment(this CommentMap cmap, Node n, ptr<CommentGroup> _addr_c) {
    ref CommentGroup c = ref _addr_c.val;

    var list = cmap[n];
    if (len(list) == 0) {
        list = new slice<ptr<CommentGroup>>(new ptr<CommentGroup>[] { c });
    }
    else
 {
        list = append(list, c);
    }
    cmap[n] = list;

}

private partial struct byInterval { // : slice<Node>
}

private static nint Len(this byInterval a) {
    return len(a);
}
private static bool Less(this byInterval a, nint i, nint j) {
    var pi = a[i].Pos();
    var pj = a[j].Pos();
    return pi < pj || pi == pj && a[i].End() > a[j].End();

}
private static void Swap(this byInterval a, nint i, nint j) {
    (a[i], a[j]) = (a[j], a[i]);
}

// nodeList returns the list of nodes of the AST n in source order.
//
private static slice<Node> nodeList(Node n) {
    slice<Node> list = default;
    Inspect(n, n => { 
        // don't collect comments
        switch (n.type()) {
            case ptr<CommentGroup> _:
                return false;
                break;
            case ptr<Comment> _:
                return false;
                break;
        }
        list = append(list, n);
        return true;

    }); 
    // Note: The current implementation assumes that Inspect traverses the
    //       AST in depth-first and thus _source_ order. If AST traversal
    //       does not follow source order, the sorting call below will be
    //       required.
    // sort.Sort(byInterval(list))
    return list;

}

// A commentListReader helps iterating through a list of comment groups.
//
private partial struct commentListReader {
    public ptr<token.FileSet> fset;
    public slice<ptr<CommentGroup>> list;
    public nint index;
    public ptr<CommentGroup> comment; // comment group at current index
    public token.Position pos; // source interval of comment group at current index
    public token.Position end; // source interval of comment group at current index
}

private static bool eol(this ptr<commentListReader> _addr_r) {
    ref commentListReader r = ref _addr_r.val;

    return r.index >= len(r.list);
}

private static void next(this ptr<commentListReader> _addr_r) {
    ref commentListReader r = ref _addr_r.val;

    if (!r.eol()) {
        r.comment = r.list[r.index];
        r.pos = r.fset.Position(r.comment.Pos());
        r.end = r.fset.Position(r.comment.End());
        r.index++;
    }
}

// A nodeStack keeps track of nested nodes.
// A node lower on the stack lexically contains the nodes higher on the stack.
//
private partial struct nodeStack { // : slice<Node>
}

// push pops all nodes that appear lexically before n
// and then pushes n on the stack.
//
private static void push(this ptr<nodeStack> _addr_s, Node n) {
    ref nodeStack s = ref _addr_s.val;

    s.pop(n.Pos());
    s.val = append((s.val), n);
}

// pop pops all nodes that appear lexically before pos
// (i.e., whose lexical extent has ended before or at pos).
// It returns the last node popped.
//
private static Node pop(this ptr<nodeStack> _addr_s, token.Pos pos) {
    Node top = default;
    ref nodeStack s = ref _addr_s.val;

    var i = len(s.val);
    while (i > 0 && (s.val)[i - 1].End() <= pos) {
        top = (s.val)[i - 1];
        i--;
    }
    s.val = (s.val)[(int)0..(int)i];
    return top;
}

// NewCommentMap creates a new comment map by associating comment groups
// of the comments list with the nodes of the AST specified by node.
//
// A comment group g is associated with a node n if:
//
//   - g starts on the same line as n ends
//   - g starts on the line immediately following n, and there is
//     at least one empty line after g and before the next node
//   - g starts before n and is not associated to the node before n
//     via the previous rules
//
// NewCommentMap tries to associate a comment group to the "largest"
// node possible: For instance, if the comment is a line comment
// trailing an assignment, the comment is associated with the entire
// assignment rather than just the last operand in the assignment.
//
public static CommentMap NewCommentMap(ptr<token.FileSet> _addr_fset, Node node, slice<ptr<CommentGroup>> comments) => func((_, panic, _) => {
    ref token.FileSet fset = ref _addr_fset.val;

    if (len(comments) == 0) {
        return null; // no comments to map
    }
    var cmap = make(CommentMap); 

    // set up comment reader r
    var tmp = make_slice<ptr<CommentGroup>>(len(comments));
    copy(tmp, comments); // don't change incoming comments
    sortComments(tmp);
    commentListReader r = new commentListReader(fset:fset,list:tmp); // !r.eol() because len(comments) > 0
    r.next(); 

    // create node list in lexical order
    var nodes = nodeList(node);
    nodes = append(nodes, null); // append sentinel

    // set up iteration variables
    Node p = default;    token.Position pend = default;    Node pg = default;    token.Position pgend = default;    nodeStack stack = default;

    foreach (var (_, q) in nodes) {
        token.Position qpos = default;
        if (q != null) {
            qpos = fset.Position(q.Pos()); // current node position
        }
        else
 { 
            // set fake sentinel position to infinity so that
            // all comments get processed before the sentinel
            const nint infinity = 1 << 30;

            qpos.Offset = infinity;
            qpos.Line = infinity;

        }
        while (r.end.Offset <= qpos.Offset) { 
            // determine recent node group
            {
                var top = stack.pop(r.comment.Pos());

                if (top != null) {
                    pg = top;
                    pgend = fset.Position(pg.End());
                } 
                // Try to associate a comment first with a node group
                // (i.e., a node of "importance" such as a declaration);
                // if that fails, try to associate it with the most recent
                // node.
                // TODO(gri) try to simplify the logic below

            } 
            // Try to associate a comment first with a node group
            // (i.e., a node of "importance" such as a declaration);
            // if that fails, try to associate it with the most recent
            // node.
            // TODO(gri) try to simplify the logic below
            Node assoc = default;

            if (pg != null && (pgend.Line == r.pos.Line || pgend.Line + 1 == r.pos.Line && r.end.Line + 1 < qpos.Line)) 
                // 1) comment starts on same line as previous node group ends, or
                // 2) comment starts on the line immediately after the
                //    previous node group and there is an empty line before
                //    the current node
                // => associate comment with previous node group
                assoc = pg;
            else if (p != null && (pend.Line == r.pos.Line || pend.Line + 1 == r.pos.Line && r.end.Line + 1 < qpos.Line || q == null)) 
                // same rules apply as above for p rather than pg,
                // but also associate with p if we are at the end (q == nil)
                assoc = p;
            else 
                // otherwise, associate comment with current node
                if (q == null) { 
                    // we can only reach here if there was no p
                    // which would imply that there were no nodes
                    panic("internal error: no comments should be associated with sentinel");

                }

                assoc = q;
                        cmap.addComment(assoc, r.comment);
            if (r.eol()) {
                return cmap;
            }

            r.next();

        } 

        // update previous node
        p = q;
        pend = fset.Position(p.End()); 

        // update previous node group if we see an "important" node
        switch (q.type()) {
            case ptr<File> _:
                stack.push(q);
                break;
            case ptr<Field> _:
                stack.push(q);
                break;
            case Decl _:
                stack.push(q);
                break;
            case Spec _:
                stack.push(q);
                break;
            case Stmt _:
                stack.push(q);
                break;
        }

    }    return cmap;

});

// Update replaces an old node in the comment map with the new node
// and returns the new node. Comments that were associated with the
// old node are associated with the new node.
//
public static Node Update(this CommentMap cmap, Node old, Node @new) {
    {
        var list = cmap[old];

        if (len(list) > 0) {
            delete(cmap, old);
            cmap[new] = append(cmap[new], list);
        }
    }

    return new;

}

// Filter returns a new comment map consisting of only those
// entries of cmap for which a corresponding node exists in
// the AST specified by node.
//
public static CommentMap Filter(this CommentMap cmap, Node node) {
    var umap = make(CommentMap);
    Inspect(node, n => {
        {
            var g = cmap[n];

            if (len(g) > 0) {
                umap[n] = g;
            }

        }

        return true;

    });
    return umap;

}

// Comments returns the list of comment groups in the comment map.
// The result is sorted in source order.
//
public static slice<ptr<CommentGroup>> Comments(this CommentMap cmap) {
    var list = make_slice<ptr<CommentGroup>>(0, len(cmap));
    foreach (var (_, e) in cmap) {
        list = append(list, e);
    }    sortComments(list);
    return list;
}

private static @string summary(slice<ptr<CommentGroup>> list) {
    const nint maxLen = 40;

    bytes.Buffer buf = default; 

    // collect comments text
loop: 

    // truncate if too long
    foreach (var (_, group) in list) { 
        // Note: CommentGroup.Text() does too much work for what we
        //       need and would only replace this innermost loop.
        //       Just do it explicitly.
        foreach (var (_, comment) in group.List) {
            if (buf.Len() >= maxLen) {
                _breakloop = true;
                break;
            }

            buf.WriteString(comment.Text);

        }
    }    if (buf.Len() > maxLen) {
        buf.Truncate(maxLen - 3);
        buf.WriteString("...");
    }
    var bytes = buf.Bytes();
    foreach (var (i, b) in bytes) {
        switch (b) {
            case '\t': 

            case '\n': 

            case '\r': 
                bytes[i] = ' ';
                break;
        }

    }    return string(bytes);

}

public static @string String(this CommentMap cmap) { 
    // print map entries in sorted order
    slice<Node> nodes = default;
    {
        var node__prev1 = node;

        foreach (var (__node) in cmap) {
            node = __node;
            nodes = append(nodes, node);
        }
        node = node__prev1;
    }

    sort.Sort(byInterval(nodes));

    ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
    fmt.Fprintln(_addr_buf, "CommentMap {");
    {
        var node__prev1 = node;

        foreach (var (_, __node) in nodes) {
            node = __node;
            var comment = cmap[node]; 
            // print name of identifiers; print node type for other nodes
            @string s = default;
            {
                ptr<Ident> (ident, ok) = node._<ptr<Ident>>();

                if (ok) {
                    s = ident.Name;
                }
                else
 {
                    s = fmt.Sprintf("%T", node);
                }

            }

            fmt.Fprintf(_addr_buf, "\t%p  %20s:  %s\n", node, s, summary(comment));

        }
        node = node__prev1;
    }

    fmt.Fprintln(_addr_buf, "}");
    return buf.String();

}

} // end ast_package
