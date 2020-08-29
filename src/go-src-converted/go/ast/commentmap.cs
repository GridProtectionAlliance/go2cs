// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ast -- go2cs converted at 2020 August 29 08:48:30 UTC
// import "go/ast" ==> using ast = go.go.ast_package
// Original source: C:\Go\src\go\ast\commentmap.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using token = go.go.token_package;
using sort = go.sort_package;
using static go.builtin;
using System;

namespace go {
namespace go
{
    public static partial class ast_package
    {
        private partial struct byPos // : slice<ref CommentGroup>
        {
        }

        private static long Len(this byPos a)
        {
            return len(a);
        }
        private static bool Less(this byPos a, long i, long j)
        {
            return a[i].Pos() < a[j].Pos();
        }
        private static void Swap(this byPos a, long i, long j)
        {
            a[i] = a[j];
            a[j] = a[i];

        }

        // sortComments sorts the list of comment groups in source order.
        //
        private static void sortComments(slice<ref CommentGroup> list)
        { 
            // TODO(gri): Does it make sense to check for sorted-ness
            //            first (because we know that sorted-ness is
            //            very likely)?
            {
                var orderedList = byPos(list);

                if (!sort.IsSorted(orderedList))
                {
                    sort.Sort(orderedList);
                }

            }
        }

        // A CommentMap maps an AST node to a list of comment groups
        // associated with it. See NewCommentMap for a description of
        // the association.
        //
        public partial struct CommentMap // : map<Node, slice<ref CommentGroup>>
        {
        }

        public static void addComment(this CommentMap cmap, Node n, ref CommentGroup c)
        {
            var list = cmap[n];
            if (len(list) == 0L)
            {
                list = new slice<ref CommentGroup>(new ref CommentGroup[] { c });
            }
            else
            {
                list = append(list, c);
            }
            cmap[n] = list;
        }

        private partial struct byInterval // : slice<Node>
        {
        }

        private static long Len(this byInterval a)
        {
            return len(a);
        }
        private static bool Less(this byInterval a, long i, long j)
        {
            var pi = a[i].Pos();
            var pj = a[j].Pos();
            return pi < pj || pi == pj && a[i].End() > a[j].End();
        }
        private static void Swap(this byInterval a, long i, long j)
        {
            a[i] = a[j];
            a[j] = a[i];

        }

        // nodeList returns the list of nodes of the AST n in source order.
        //
        private static slice<Node> nodeList(Node n)
        {
            slice<Node> list = default;
            Inspect(n, n =>
            { 
                // don't collect comments
                switch (n.type())
                {
                    case ref CommentGroup _:
                        return false;
                        break;
                    case ref Comment _:
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
        private partial struct commentListReader
        {
            public ptr<token.FileSet> fset;
            public slice<ref CommentGroup> list;
            public long index;
            public ptr<CommentGroup> comment; // comment group at current index
            public token.Position pos; // source interval of comment group at current index
            public token.Position end; // source interval of comment group at current index
        }

        private static bool eol(this ref commentListReader r)
        {
            return r.index >= len(r.list);
        }

        private static void next(this ref commentListReader r)
        {
            if (!r.eol())
            {
                r.comment = r.list[r.index];
                r.pos = r.fset.Position(r.comment.Pos());
                r.end = r.fset.Position(r.comment.End());
                r.index++;
            }
        }

        // A nodeStack keeps track of nested nodes.
        // A node lower on the stack lexically contains the nodes higher on the stack.
        //
        private partial struct nodeStack // : slice<Node>
        {
        }

        // push pops all nodes that appear lexically before n
        // and then pushes n on the stack.
        //
        private static void push(this ref nodeStack s, Node n)
        {
            s.pop(n.Pos());
            s.Value = append((s.Value), n);
        }

        // pop pops all nodes that appear lexically before pos
        // (i.e., whose lexical extent has ended before or at pos).
        // It returns the last node popped.
        //
        private static Node pop(this ref nodeStack s, token.Pos pos)
        {
            var i = len(s.Value);
            while (i > 0L && (s.Value)[i - 1L].End() <= pos)
            {
                top = (s.Value)[i - 1L];
                i--;
            }

            s.Value = (s.Value)[0L..i];
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
        public static CommentMap NewCommentMap(ref token.FileSet _fset, Node node, slice<ref CommentGroup> comments) => func(_fset, (ref token.FileSet fset, Defer _, Panic panic, Recover __) =>
        {
            if (len(comments) == 0L)
            {
                return null; // no comments to map
            }
            var cmap = make(CommentMap); 

            // set up comment reader r
            var tmp = make_slice<ref CommentGroup>(len(comments));
            copy(tmp, comments); // don't change incoming comments
            sortComments(tmp);
            commentListReader r = new commentListReader(fset:fset,list:tmp); // !r.eol() because len(comments) > 0
            r.next(); 

            // create node list in lexical order
            var nodes = nodeList(node);
            nodes = append(nodes, null); // append sentinel

            // set up iteration variables
            Node p = default;            token.Position pend = default;            Node pg = default;            token.Position pgend = default;            nodeStack stack = default;

            foreach (var (_, q) in nodes)
            {
                token.Position qpos = default;
                if (q != null)
                {
                    qpos = fset.Position(q.Pos()); // current node position
                }
                else
                { 
                    // set fake sentinel position to infinity so that
                    // all comments get processed before the sentinel
                    const long infinity = 1L << (int)(30L);

                    qpos.Offset = infinity;
                    qpos.Line = infinity;
                } 

                // process comments before current node
                while (r.end.Offset <= qpos.Offset)
                { 
                    // determine recent node group
                    {
                        var top = stack.pop(r.comment.Pos());

                        if (top != null)
                        {
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

                    if (pg != null && (pgend.Line == r.pos.Line || pgend.Line + 1L == r.pos.Line && r.end.Line + 1L < qpos.Line)) 
                        // 1) comment starts on same line as previous node group ends, or
                        // 2) comment starts on the line immediately after the
                        //    previous node group and there is an empty line before
                        //    the current node
                        // => associate comment with previous node group
                        assoc = pg;
                    else if (p != null && (pend.Line == r.pos.Line || pend.Line + 1L == r.pos.Line && r.end.Line + 1L < qpos.Line || q == null)) 
                        // same rules apply as above for p rather than pg,
                        // but also associate with p if we are at the end (q == nil)
                        assoc = p;
                    else 
                        // otherwise, associate comment with current node
                        if (q == null)
                        { 
                            // we can only reach here if there was no p
                            // which would imply that there were no nodes
                            panic("internal error: no comments should be associated with sentinel");
                        }
                        assoc = q;
                                        cmap.addComment(assoc, r.comment);
                    if (r.eol())
                    {
                        return cmap;
                    }
                    r.next();
                } 

                // update previous node
 

                // update previous node
                p = q;
                pend = fset.Position(p.End()); 

                // update previous node group if we see an "important" node
                switch (q.type())
                {
                    case ref File _:
                        stack.push(q);
                        break;
                    case ref Field _:
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
            }
            return cmap;
        });

        // Update replaces an old node in the comment map with the new node
        // and returns the new node. Comments that were associated with the
        // old node are associated with the new node.
        //
        public static Node Update(this CommentMap cmap, Node old, Node @new)
        {
            {
                var list = cmap[old];

                if (len(list) > 0L)
                {
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
        public static CommentMap Filter(this CommentMap cmap, Node node)
        {
            var umap = make(CommentMap);
            Inspect(node, n =>
            {
                {
                    var g = cmap[n];

                    if (len(g) > 0L)
                    {
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
        public static slice<ref CommentGroup> Comments(this CommentMap cmap)
        {
            var list = make_slice<ref CommentGroup>(0L, len(cmap));
            foreach (var (_, e) in cmap)
            {
                list = append(list, e);
            }
            sortComments(list);
            return list;
        }

        private static @string summary(slice<ref CommentGroup> list)
        {
            const long maxLen = 40L;

            bytes.Buffer buf = default; 

            // collect comments text
loop: 

            // truncate if too long
            foreach (var (_, group) in list)
            { 
                // Note: CommentGroup.Text() does too much work for what we
                //       need and would only replace this innermost loop.
                //       Just do it explicitly.
                foreach (var (_, comment) in group.List)
                {
                    if (buf.Len() >= maxLen)
                    {
                        _breakloop = true;
                        break;
                    }
                    buf.WriteString(comment.Text);
                }
            } 

            // truncate if too long
            if (buf.Len() > maxLen)
            {
                buf.Truncate(maxLen - 3L);
                buf.WriteString("...");
            } 

            // replace any invisibles with blanks
            var bytes = buf.Bytes();
            foreach (var (i, b) in bytes)
            {
                switch (b)
                {
                    case '\t': 

                    case '\n': 

                    case '\r': 
                        bytes[i] = ' ';
                        break;
                }
            }
            return string(bytes);
        }

        public static @string String(this CommentMap cmap)
        {
            bytes.Buffer buf = default;
            fmt.Fprintln(ref buf, "CommentMap {");
            foreach (var (node, comment) in cmap)
            { 
                // print name of identifiers; print node type for other nodes
                @string s = default;
                {
                    ref Ident (ident, ok) = node._<ref Ident>();

                    if (ok)
                    {
                        s = ident.Name;
                    }
                    else
                    {
                        s = fmt.Sprintf("%T", node);
                    }

                }
                fmt.Fprintf(ref buf, "\t%p  %20s:  %s\n", node, s, summary(comment));
            }
            fmt.Fprintln(ref buf, "}");
            return buf.String();
        }
    }
}}
