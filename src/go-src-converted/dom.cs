// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 08 04:56:54 UTC
// import "golang.org/x/tools/go/ssa" ==> using ssa = go.golang.org.x.tools.go.ssa_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\dom.go
// This file defines algorithms related to dominance.

// Dominator tree construction ----------------------------------------
//
// We use the algorithm described in Lengauer & Tarjan. 1979.  A fast
// algorithm for finding dominators in a flowgraph.
// http://doi.acm.org/10.1145/357062.357071
//
// We also apply the optimizations to SLT described in Georgiadis et
// al, Finding Dominators in Practice, JGAA 2006,
// http://jgaa.info/accepted/2006/GeorgiadisTarjanWerneck2006.10.1.pdf
// to avoid the need for buckets of size > 1.

using bytes = go.bytes_package;
using fmt = go.fmt_package;
using big = go.math.big_package;
using os = go.os_package;
using sort = go.sort_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go
{
    public static partial class ssa_package
    {
        // Idom returns the block that immediately dominates b:
        // its parent in the dominator tree, if any.
        // Neither the entry node (b.Index==0) nor recover node
        // (b==b.Parent().Recover()) have a parent.
        //
        private static ptr<BasicBlock> Idom(this ptr<BasicBlock> _addr_b)
        {
            ref BasicBlock b = ref _addr_b.val;

            return _addr_b.dom.idom!;
        }

        // Dominees returns the list of blocks that b immediately dominates:
        // its children in the dominator tree.
        //
        private static slice<ptr<BasicBlock>> Dominees(this ptr<BasicBlock> _addr_b)
        {
            ref BasicBlock b = ref _addr_b.val;

            return b.dom.children;
        }

        // Dominates reports whether b dominates c.
        private static bool Dominates(this ptr<BasicBlock> _addr_b, ptr<BasicBlock> _addr_c)
        {
            ref BasicBlock b = ref _addr_b.val;
            ref BasicBlock c = ref _addr_c.val;

            return b.dom.pre <= c.dom.pre && c.dom.post <= b.dom.post;
        }

        private partial struct byDomPreorder // : slice<ptr<BasicBlock>>
        {
        }

        private static long Len(this byDomPreorder a)
        {
            return len(a);
        }
        private static void Swap(this byDomPreorder a, long i, long j)
        {
            a[i] = a[j];
            a[j] = a[i];
        }
        private static bool Less(this byDomPreorder a, long i, long j)
        {
            return a[i].dom.pre < a[j].dom.pre;
        }

        // DomPreorder returns a new slice containing the blocks of f in
        // dominator tree preorder.
        //
        private static slice<ptr<BasicBlock>> DomPreorder(this ptr<Function> _addr_f)
        {
            ref Function f = ref _addr_f.val;

            var n = len(f.Blocks);
            var order = make(byDomPreorder, n);
            copy(order, f.Blocks);
            sort.Sort(order);
            return order;
        }

        // domInfo contains a BasicBlock's dominance information.
        private partial struct domInfo
        {
            public ptr<BasicBlock> idom; // immediate dominator (parent in domtree)
            public slice<ptr<BasicBlock>> children; // nodes immediately dominated by this one
            public int pre; // pre- and post-order numbering within domtree
            public int post; // pre- and post-order numbering within domtree
        }

        // ltState holds the working state for Lengauer-Tarjan algorithm
        // (during which domInfo.pre is repurposed for CFG DFS preorder number).
        private partial struct ltState
        {
            public slice<ptr<BasicBlock>> sdom; // b's semidominator
            public slice<ptr<BasicBlock>> parent; // b's parent in DFS traversal of CFG
            public slice<ptr<BasicBlock>> ancestor; // b's ancestor with least sdom
        }

        // dfs implements the depth-first search part of the LT algorithm.
        private static int dfs(this ptr<ltState> _addr_lt, ptr<BasicBlock> _addr_v, int i, slice<ptr<BasicBlock>> preorder)
        {
            ref ltState lt = ref _addr_lt.val;
            ref BasicBlock v = ref _addr_v.val;

            preorder[i] = v;
            v.dom.pre = i; // For now: DFS preorder of spanning tree of CFG
            i++;
            lt.sdom[v.Index] = v;
            lt.link(null, v);
            foreach (var (_, w) in v.Succs)
            {
                if (lt.sdom[w.Index] == null)
                {
                    lt.parent[w.Index] = v;
                    i = lt.dfs(w, i, preorder);
                }

            }
            return i;

        }

        // eval implements the EVAL part of the LT algorithm.
        private static ptr<BasicBlock> eval(this ptr<ltState> _addr_lt, ptr<BasicBlock> _addr_v)
        {
            ref ltState lt = ref _addr_lt.val;
            ref BasicBlock v = ref _addr_v.val;
 
            // TODO(adonovan): opt: do path compression per simple LT.
            var u = v;
            while (lt.ancestor[v.Index] != null)
            {
                if (lt.sdom[v.Index].dom.pre < lt.sdom[u.Index].dom.pre)
                {
                    u = v;
                v = lt.ancestor[v.Index];
                }

            }

            return _addr_u!;

        }

        // link implements the LINK part of the LT algorithm.
        private static void link(this ptr<ltState> _addr_lt, ptr<BasicBlock> _addr_v, ptr<BasicBlock> _addr_w)
        {
            ref ltState lt = ref _addr_lt.val;
            ref BasicBlock v = ref _addr_v.val;
            ref BasicBlock w = ref _addr_w.val;

            lt.ancestor[w.Index] = v;
        }

        // buildDomTree computes the dominator tree of f using the LT algorithm.
        // Precondition: all blocks are reachable (e.g. optimizeBlocks has been run).
        //
        private static void buildDomTree(ptr<Function> _addr_f)
        {
            ref Function f = ref _addr_f.val;
 
            // The step numbers refer to the original LT paper; the
            // reordering is due to Georgiadis.

            // Clear any previous domInfo.
            foreach (var (_, b) in f.Blocks)
            {
                b.dom = new domInfo();
            }
            var n = len(f.Blocks); 
            // Allocate space for 5 contiguous [n]*BasicBlock arrays:
            // sdom, parent, ancestor, preorder, buckets.
            var space = make_slice<ptr<BasicBlock>>(5L * n);
            ltState lt = new ltState(sdom:space[0:n],parent:space[n:2*n],ancestor:space[2*n:3*n],); 

            // Step 1.  Number vertices by depth-first preorder.
            var preorder = space[3L * n..4L * n];
            var root = f.Blocks[0L];
            var prenum = lt.dfs(root, 0L, preorder);
            var recover = f.Recover;
            if (recover != null)
            {
                lt.dfs(recover, prenum, preorder);
            }

            var buckets = space[4L * n..5L * n];
            copy(buckets, preorder); 

            // In reverse preorder...
            for (var i = int32(n) - 1L; i > 0L; i--)
            {
                var w = preorder[i]; 

                // Step 3. Implicitly define the immediate dominator of each node.
                {
                    var v__prev2 = v;

                    var v = buckets[i];

                    while (v != w)
                    {
                        var u = lt.eval(v);
                        if (lt.sdom[u.Index].dom.pre < i)
                        {
                            v.dom.idom = u;
                        v = buckets[v.dom.pre];
                        }
                        else
                        {
                            v.dom.idom = w;
                        }

                    } 

                    // Step 2. Compute the semidominators of all nodes.


                    v = v__prev2;
                } 

                // Step 2. Compute the semidominators of all nodes.
                lt.sdom[w.Index] = lt.parent[w.Index];
                {
                    var v__prev2 = v;

                    foreach (var (_, __v) in w.Preds)
                    {
                        v = __v;
                        u = lt.eval(v);
                        if (lt.sdom[u.Index].dom.pre < lt.sdom[w.Index].dom.pre)
                        {
                            lt.sdom[w.Index] = lt.sdom[u.Index];
                        }

                    }

                    v = v__prev2;
                }

                lt.link(lt.parent[w.Index], w);

                if (lt.parent[w.Index] == lt.sdom[w.Index])
                {
                    w.dom.idom = lt.parent[w.Index];
                }
                else
                {
                    buckets[i] = buckets[lt.sdom[w.Index].dom.pre];
                    buckets[lt.sdom[w.Index].dom.pre] = w;
                }

            } 

            // The final 'Step 3' is now outside the loop.
 

            // The final 'Step 3' is now outside the loop.
            {
                var v__prev1 = v;

                v = buckets[0L];

                while (v != root)
                {
                    v.dom.idom = root;
                    v = buckets[v.dom.pre];
                } 

                // Step 4. Explicitly define the immediate dominator of each
                // node, in preorder.


                v = v__prev1;
            } 

            // Step 4. Explicitly define the immediate dominator of each
            // node, in preorder.
            {
                var w__prev1 = w;

                foreach (var (_, __w) in preorder[1L..])
                {
                    w = __w;
                    if (w == root || w == recover)
                    {
                        w.dom.idom = null;
                    }
                    else
                    {
                        if (w.dom.idom != lt.sdom[w.Index])
                        {
                            w.dom.idom = w.dom.idom.dom.idom;
                        } 
                        // Calculate Children relation as inverse of Idom.
                        w.dom.idom.dom.children = append(w.dom.idom.dom.children, w);

                    }

                }

                w = w__prev1;
            }

            var (pre, post) = numberDomTree(_addr_root, 0L, 0L);
            if (recover != null)
            {
                numberDomTree(_addr_recover, pre, post);
            } 

            // printDomTreeDot(os.Stderr, f)        // debugging
            // printDomTreeText(os.Stderr, root, 0) // debugging
            if (f.Prog.mode & SanityCheckFunctions != 0L)
            {
                sanityCheckDomTree(_addr_f);
            }

        }

        // numberDomTree sets the pre- and post-order numbers of a depth-first
        // traversal of the dominator tree rooted at v.  These are used to
        // answer dominance queries in constant time.
        //
        private static (int, int) numberDomTree(ptr<BasicBlock> _addr_v, int pre, int post)
        {
            int _p0 = default;
            int _p0 = default;
            ref BasicBlock v = ref _addr_v.val;

            v.dom.pre = pre;
            pre++;
            foreach (var (_, child) in v.dom.children)
            {
                pre, post = numberDomTree(_addr_child, pre, post);
            }
            v.dom.post = post;
            post++;
            return (pre, post);

        }

        // Testing utilities ----------------------------------------

        // sanityCheckDomTree checks the correctness of the dominator tree
        // computed by the LT algorithm by comparing against the dominance
        // relation computed by a naive Kildall-style forward dataflow
        // analysis (Algorithm 10.16 from the "Dragon" book).
        //
        private static void sanityCheckDomTree(ptr<Function> _addr_f) => func((_, panic, __) =>
        {
            ref Function f = ref _addr_f.val;

            var n = len(f.Blocks); 

            // D[i] is the set of blocks that dominate f.Blocks[i],
            // represented as a bit-set of block indices.
            var D = make_slice<big.Int>(n);

            var one = big.NewInt(1L); 

            // all is the set of all blocks; constant.
            ref big.Int all = ref heap(out ptr<big.Int> _addr_all);
            all.Set(one).Lsh(_addr_all, uint(n)).Sub(_addr_all, one); 

            // Initialization.
            {
                var i__prev1 = i;
                var b__prev1 = b;

                foreach (var (__i, __b) in f.Blocks)
                {
                    i = __i;
                    b = __b;
                    if (i == 0L || b == f.Recover)
                    { 
                        // A root is dominated only by itself.
                        D[i].SetBit(_addr_D[0L], 0L, 1L);

                    }
                    else
                    { 
                        // All other blocks are (initially) dominated
                        // by every block.
                        D[i].Set(_addr_all);

                    }

                } 

                // Iteration until fixed point.

                i = i__prev1;
                b = b__prev1;
            }

            {
                var changed = true;

                while (changed)
                {
                    changed = false;
                    {
                        var i__prev2 = i;
                        var b__prev2 = b;

                        foreach (var (__i, __b) in f.Blocks)
                        {
                            i = __i;
                            b = __b;
                            if (i == 0L || b == f.Recover)
                            {
                                continue;
                            } 
                            // Compute intersection across predecessors.
                            ref big.Int x = ref heap(out ptr<big.Int> _addr_x);
                            x.Set(_addr_all);
                            foreach (var (_, pred) in b.Preds)
                            {
                                x.And(_addr_x, _addr_D[pred.Index]);
                            }
                            x.SetBit(_addr_x, i, 1L); // a block always dominates itself.
                            if (D[i].Cmp(_addr_x) != 0L)
                            {
                                D[i].Set(_addr_x);
                                changed = true;
                            }

                        }

                        i = i__prev2;
                        b = b__prev2;
                    }
                } 

                // Check the entire relation.  O(n^2).
                // The Recover block (if any) must be treated specially so we skip it.

            } 

            // Check the entire relation.  O(n^2).
            // The Recover block (if any) must be treated specially so we skip it.
            var ok = true;
            {
                var i__prev1 = i;

                for (long i = 0L; i < n; i++)
                {
                    for (long j = 0L; j < n; j++)
                    {
                        var b = f.Blocks[i];
                        var c = f.Blocks[j];
                        if (c == f.Recover)
                        {
                            continue;
                        }

                        var actual = b.Dominates(c);
                        var expected = D[j].Bit(i) == 1L;
                        if (actual != expected)
                        {
                            fmt.Fprintf(os.Stderr, "dominates(%s, %s)==%t, want %t\n", b, c, actual, expected);
                            ok = false;
                        }

                    }


                }


                i = i__prev1;
            }

            var preorder = f.DomPreorder();
            {
                var b__prev1 = b;

                foreach (var (_, __b) in f.Blocks)
                {
                    b = __b;
                    {
                        var got = preorder[b.dom.pre];

                        if (got != b)
                        {
                            fmt.Fprintf(os.Stderr, "preorder[%d]==%s, want %s\n", b.dom.pre, got, b);
                            ok = false;
                        }

                    }

                }

                b = b__prev1;
            }

            if (!ok)
            {
                panic("sanityCheckDomTree failed for " + f.String());
            }

        });

        // Printing functions ----------------------------------------

        // printDomTree prints the dominator tree as text, using indentation.
        private static void printDomTreeText(ptr<bytes.Buffer> _addr_buf, ptr<BasicBlock> _addr_v, long indent)
        {
            ref bytes.Buffer buf = ref _addr_buf.val;
            ref BasicBlock v = ref _addr_v.val;

            fmt.Fprintf(buf, "%*s%s\n", 4L * indent, "", v);
            foreach (var (_, child) in v.dom.children)
            {
                printDomTreeText(_addr_buf, _addr_child, indent + 1L);
            }

        }

        // printDomTreeDot prints the dominator tree of f in AT&T GraphViz
        // (.dot) format.
        private static void printDomTreeDot(ptr<bytes.Buffer> _addr_buf, ptr<Function> _addr_f)
        {
            ref bytes.Buffer buf = ref _addr_buf.val;
            ref Function f = ref _addr_f.val;

            fmt.Fprintln(buf, "//", f);
            fmt.Fprintln(buf, "digraph domtree {");
            foreach (var (i, b) in f.Blocks)
            {
                var v = b.dom;
                fmt.Fprintf(buf, "\tn%d [label=\"%s (%d, %d)\",shape=\"rectangle\"];\n", v.pre, b, v.pre, v.post); 
                // TODO(adonovan): improve appearance of edges
                // belonging to both dominator tree and CFG.

                // Dominator tree edge.
                if (i != 0L)
                {
                    fmt.Fprintf(buf, "\tn%d -> n%d [style=\"solid\",weight=100];\n", v.idom.dom.pre, v.pre);
                } 
                // CFG edges.
                foreach (var (_, pred) in b.Preds)
                {
                    fmt.Fprintf(buf, "\tn%d -> n%d [style=\"dotted\",weight=0];\n", pred.dom.pre, v.pre);
                }

            }
            fmt.Fprintln(buf, "}");

        }
    }
}}}}}
