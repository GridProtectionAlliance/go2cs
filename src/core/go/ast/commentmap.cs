// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using bytes = bytes_package;
using cmp = cmp_package;
using fmt = fmt_package;
using token = go.token_package;
using slices = slices_package;
using strings = strings_package;

partial class ast_package {

// sortComments sorts the list of comment groups in source order.
internal static void sortComments(slice<ж<CommentGroup>> list) {
    slices.SortFunc(list, (ж<CommentGroup> a, ж<CommentGroup> b) => cmp.Compare(a.Pos(), b.Pos()));
}
/* visitMapType: map[Node][]*CommentGroup */

public static void addComment(this CommentMap cmap, Node n, ж<CommentGroup> Ꮡc) {
    ref var c = ref Ꮡc.val;

    var list = cmap[n];
    if (len(list) == 0){
        list = new ж<CommentGroup>[]{c}.slice();
    } else {
        list = append(list, Ꮡc);
    }
    cmap[n] = list;
}

[GoType("[]Node")] partial struct byInterval;

internal static nint Len(this byInterval a) {
    return len(a);
}

internal static bool Less(this byInterval a, nint i, nint j) {
    tokenꓸPos pi = a[i].Pos();
    tokenꓸPos pj = a[j].Pos();
    return pi < pj || pi == pj && a[i].End() > a[j].End();
}

internal static void Swap(this byInterval a, nint i, nint j) {
    (a[i], a[j]) = (a[j], a[i]);
}

// nodeList returns the list of nodes of the AST n in source order.
internal static slice<Node> nodeList(Node n) {
    slice<Node> list = default!;
    Inspect(n, 
    var listʗ1 = list;
    (Node n) => {
        // don't collect comments
        switch (nΔ1.type()) {
        case default! : {
            return false;
        }
        case CommentGroup.val : {
            return false;
        }
        case Comment.val : {
            return false;
        }}

        listʗ1 = append(listʗ1, nΔ1);
        return true;
    });
    // Note: The current implementation assumes that Inspect traverses the
    //       AST in depth-first and thus _source_ order. If AST traversal
    //       does not follow source order, the sorting call below will be
    //       required.
    // slices.Sort(list, func(a, b Node) int {
    //       r := cmp.Compare(a.Pos(), b.Pos())
    //       if r != 0 {
    //               return r
    //       }
    //       return cmp.Compare(a.End(), b.End())
    // })
    return list;
}

// A commentListReader helps iterating through a list of comment groups.
[GoType] partial struct commentListReader {
    internal ж<go.token_package.FileSet> fset;
    internal slice<ж<CommentGroup>> list;
    internal nint index;
    internal ж<CommentGroup> comment; // comment group at current index
    internal go.token_package.ΔPosition pos; // source interval of comment group at current index
    internal go.token_package.ΔPosition end;
}

[GoRecv] internal static bool eol(this ref commentListReader r) {
    return r.index >= len(r.list);
}

[GoRecv] internal static void next(this ref commentListReader r) {
    if (!r.eol()) {
        r.comment = r.list[r.index];
        r.pos = r.fset.Position(r.comment.Pos());
        r.end = r.fset.Position(r.comment.End());
        r.index++;
    }
}

[GoType("[]Node")] partial struct nodeStack;

// push pops all nodes that appear lexically before n
// and then pushes n on the stack.
[GoRecv] internal static void push(this ref nodeStack s, Node n) {
    s.pop(n.Pos());
    s = append((ж<ж<nodeStack>>), n);
}

// pop pops all nodes that appear lexically before pos
// (i.e., whose lexical extent has ended before or at pos).
// It returns the last node popped.
[GoRecv] internal static unsafe Node /*top*/ pop(this ref nodeStack s, tokenꓸPos pos) {
    Node top = default!;

    nint i = len(s);
    while (i > 0 && (ж<ж<nodeStack>>)[i - 1].End() <= pos) {
        top = (ж<ж<nodeStack>>)[i - 1];
        i--;
    }
    s = new Span<ж<nodeStack>>((nodeStack**), i);
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
public static CommentMap NewCommentMap(ж<token.FileSet> Ꮡfset, Node node, slice<ж<CommentGroup>> comments) {
    ref var fset = ref Ꮡfset.val;

    if (len(comments) == 0) {
        return default!;
    }
    // no comments to map
    var cmap = new CommentMap();
    // set up comment reader r
    var tmp = new slice<ж<CommentGroup>>(len(comments));
    copy(tmp, comments);
    // don't change incoming comments
    sortComments(tmp);
    ref var r = ref heap<commentListReader>(out var Ꮡr);
    r = new commentListReader(fset: fset, list: tmp);
    // !r.eol() because len(comments) > 0
    r.next();
    // create node list in lexical order
    var nodes = nodeList(node);
    nodes = append(nodes, default!);
    // append sentinel
    // set up iteration variables
    Node p = default!;                       // previous node
    
    tokenꓸPosition pend = default!;                           // end of p
    
    Node pg = default!;                     // previous node group (enclosing nodes of "importance")
    
    tokenꓸPosition pgend = default!;                         // end of pg
    
    nodeStack stack = default!;                    // stack of node groups
    foreach (var (_, q) in nodes) {
        tokenꓸPosition qpos = default!;
        if (q != default!){
            qpos = fset.Position(q.Pos());
        } else {
            // current node position
            // set fake sentinel position to infinity so that
            // all comments get processed before the sentinel
            static readonly UntypedInt infinity = /* 1 << 30 */ 1073741824;
            qpos.Offset = infinity;
            qpos.Line = infinity;
        }
        // process comments before current node
        while (r.end.Offset <= qpos.Offset) {
            // determine recent node group
            {
                var top = stack.pop(r.comment.Pos()); if (top != default!) {
                    pg = top;
                    pgend = fset.Position(pg.End());
                }
            }
            // Try to associate a comment first with a node group
            // (i.e., a node of "importance" such as a declaration);
            // if that fails, try to associate it with the most recent
            // node.
            // TODO(gri) try to simplify the logic below
            Node assoc = default!;
            switch (ᐧ) {
            case {} when pg != default! && (pgend.Line == r.pos.Line || pgend.Line + 1 == r.pos.Line && r.end.Line + 1 < qpos.Line): {
                assoc = pg;
                break;
            }
            case {} when p != default! && (pend.Line == r.pos.Line || pend.Line + 1 == r.pos.Line && r.end.Line + 1 < qpos.Line || q == default!): {
                assoc = p;
                break;
            }
            default: {
                if (q == default!) {
                    // 1) comment starts on same line as previous node group ends, or
                    // 2) comment starts on the line immediately after the
                    //    previous node group and there is an empty line before
                    //    the current node
                    // => associate comment with previous node group
                    // same rules apply as above for p rather than pg,
                    // but also associate with p if we are at the end (q == nil)
                    // otherwise, associate comment with current node
                    // we can only reach here if there was no p
                    // which would imply that there were no nodes
                    throw panic("internal error: no comments should be associated with sentinel");
                }
                assoc = q;
                break;
            }}

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
        case File.val : {
            stack.push(q);
            break;
        }
        case Field.val : {
            stack.push(q);
            break;
        }
        case Decl : {
            stack.push(q);
            break;
        }
        case Spec : {
            stack.push(q);
            break;
        }
        case Stmt : {
            stack.push(q);
            break;
        }}

    }
    return cmap;
}

// Update replaces an old node in the comment map with the new node
// and returns the new node. Comments that were associated with the
// old node are associated with the new node.
public static Node Update(this CommentMap cmap, Node old, Node @new) {
    {
        var list = cmap[old]; if (len(list) > 0) {
            delete(cmap, old);
            cmap[@new] = append(cmap[@new], Ꮡlist.ꓸꓸꓸ);
        }
    }
    return @new;
}

// Filter returns a new comment map consisting of only those
// entries of cmap for which a corresponding node exists in
// the AST specified by node.
public static CommentMap Filter(this CommentMap cmap, Node node) {
    var umap = new CommentMap();
    Inspect(node, 
    var cmapʗ1 = cmap;
    var umapʗ1 = umap;
    (Node n) => {
        {
            var g = cmapʗ1[n]; if (len(g) > 0) {
                umapʗ1[n] = g;
            }
        }
        return true;
    });
    return umap;
}

// Comments returns the list of comment groups in the comment map.
// The result is sorted in source order.
public static slice<ж<CommentGroup>> Comments(this CommentMap cmap) {
    var list = new slice<ж<CommentGroup>>(0, len(cmap));
    foreach (var (_, e) in cmap) {
        list = append(list, Ꮡe.ꓸꓸꓸ);
    }
    sortComments(list);
    return list;
}

internal static @string summary(slice<ж<CommentGroup>> list) {
    static readonly UntypedInt maxLen = 40;
    bytes.Buffer buf = default!;
    // collect comments text
loop:
    foreach (var (_, group) in list) {
        // Note: CommentGroup.Text() does too much work for what we
        //       need and would only replace this innermost loop.
        //       Just do it explicitly.
        foreach (var (_, comment) in (~group).List) {
            if (buf.Len() >= maxLen) {
                goto break_loop;
            }
            buf.WriteString((~comment).Text);
        }
    }
    // truncate if too long
    if (buf.Len() > maxLen) {
        buf.Truncate(maxLen - 3);
        buf.WriteString("..."u8);
    }
    // replace any invisibles with blanks
    var bytes = buf.Bytes();
    foreach (var (i, b) in bytes) {
        switch (b) {
        case (rune)'\t' or (rune)'\n' or (rune)'\r': {
            bytes[i] = (rune)' ';
            break;
        }}

    }
    return ((@string)bytes);
}

public static @string String(this CommentMap cmap) {
    // print map entries in sorted order
    slice<Node> nodes = default!;
    foreach (var (node, _) in cmap) {
        nodes = append(nodes, node);
    }
    slices.SortFunc(nodes, (Node a, Node b) => {
        nint r = cmp.Compare(a.Pos(), b.Pos());
        if (r != 0) {
            return r;
        }
        return cmp.Compare(a.End(), b.End());
    });
    ref var buf = ref heap(new strings_package.Builder(), out var Ꮡbuf);
    fmt.Fprintln(~Ꮡbuf, "CommentMap {");
    foreach (var (_, node) in nodes) {
        var comment = cmap[node];
        // print name of identifiers; print node type for other nodes
        @string s = default!;
        {
            var (ident, ok) = node._<Ident.val>(ᐧ); if (ok){
                s = ident.val.Name;
            } else {
                s = fmt.Sprintf("%T"u8, node);
            }
        }
        fmt.Fprintf(~Ꮡbuf, "\t%p  %20s:  %s\n"u8, node, s, summary(comment));
    }
    fmt.Fprintln(~Ꮡbuf, "}");
    return buf.String();
}

} // end ast_package
