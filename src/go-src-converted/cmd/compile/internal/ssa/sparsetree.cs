// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 08 04:26:39 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\sparsetree.go
using fmt = go.fmt_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        public partial struct SparseTreeNode
        {
            public ptr<Block> child;
            public ptr<Block> sibling;
            public ptr<Block> parent; // Every block has 6 numbers associated with it:
// entry-1, entry, entry+1, exit-1, and exit, exit+1.
// entry and exit are conceptually the top of the block (phi functions)
// entry+1 and exit-1 are conceptually the bottom of the block (ordinary defs)
// entry-1 and exit+1 are conceptually "just before" the block (conditions flowing in)
//
// This simplifies life if we wish to query information about x
// when x is both an input to and output of a block.
            public int entry;
            public int exit;
        }

        private static @string String(this ptr<SparseTreeNode> _addr_s)
        {
            ref SparseTreeNode s = ref _addr_s.val;

            return fmt.Sprintf("[%d,%d]", s.entry, s.exit);
        }

        private static int Entry(this ptr<SparseTreeNode> _addr_s)
        {
            ref SparseTreeNode s = ref _addr_s.val;

            return s.entry;
        }

        private static int Exit(this ptr<SparseTreeNode> _addr_s)
        {
            ref SparseTreeNode s = ref _addr_s.val;

            return s.exit;
        }

 
        // When used to lookup up definitions in a sparse tree,
        // these adjustments to a block's entry (+adjust) and
        // exit (-adjust) numbers allow a distinction to be made
        // between assignments (typically branch-dependent
        // conditionals) occurring "before" the block (e.g., as inputs
        // to the block and its phi functions), "within" the block,
        // and "after" the block.
        public static readonly long AdjustBefore = (long)-1L; // defined before phi
        public static readonly long AdjustWithin = (long)0L; // defined by phi
        public static readonly long AdjustAfter = (long)1L; // defined within block

        // A SparseTree is a tree of Blocks.
        // It allows rapid ancestor queries,
        // such as whether one block dominates another.
        public partial struct SparseTree // : slice<SparseTreeNode>
        {
        }

        // newSparseTree creates a SparseTree from a block-to-parent map (array indexed by Block.ID)
        private static SparseTree newSparseTree(ptr<Func> _addr_f, slice<ptr<Block>> parentOf)
        {
            ref Func f = ref _addr_f.val;

            var t = make(SparseTree, f.NumBlocks());
            foreach (var (_, b) in f.Blocks)
            {
                var n = _addr_t[b.ID];
                {
                    var p = parentOf[b.ID];

                    if (p != null)
                    {
                        n.parent = p;
                        n.sibling = t[p.ID].child;
                        t[p.ID].child = b;
                    }

                }

            }
            t.numberBlock(f.Entry, 1L);
            return t;

        }

        // newSparseOrderedTree creates a SparseTree from a block-to-parent map (array indexed by Block.ID)
        // children will appear in the reverse of their order in reverseOrder
        // in particular, if reverseOrder is a dfs-reversePostOrder, then the root-to-children
        // walk of the tree will yield a pre-order.
        private static SparseTree newSparseOrderedTree(ptr<Func> _addr_f, slice<ptr<Block>> parentOf, slice<ptr<Block>> reverseOrder)
        {
            ref Func f = ref _addr_f.val;

            var t = make(SparseTree, f.NumBlocks());
            foreach (var (_, b) in reverseOrder)
            {
                var n = _addr_t[b.ID];
                {
                    var p = parentOf[b.ID];

                    if (p != null)
                    {
                        n.parent = p;
                        n.sibling = t[p.ID].child;
                        t[p.ID].child = b;
                    }

                }

            }
            t.numberBlock(f.Entry, 1L);
            return t;

        }

        // treestructure provides a string description of the dominator
        // tree and flow structure of block b and all blocks that it
        // dominates.
        public static @string treestructure(this SparseTree t, ptr<Block> _addr_b)
        {
            ref Block b = ref _addr_b.val;

            return t.treestructure1(b, 0L);
        }
        public static @string treestructure1(this SparseTree t, ptr<Block> _addr_b, long i)
        {
            ref Block b = ref _addr_b.val;

            @string s = "\n" + strings.Repeat("\t", i) + b.String() + "->[";
            foreach (var (i, e) in b.Succs)
            {
                if (i > 0L)
                {
                    s += ",";
                }

                s += e.b.String();

            }
            s += "]";
            {
                var c0 = t[b.ID].child;

                if (c0 != null)
                {
                    s += "(";
                    {
                        var c = c0;

                        while (c != null)
                        {
                            if (c != c0)
                            {
                                s += " ";
                            c = t[c.ID].sibling;
                            }

                            s += t.treestructure1(c, i + 1L);

                        }

                    }
                    s += ")";

                }

            }

            return s;

        }

        // numberBlock assigns entry and exit numbers for b and b's
        // children in an in-order walk from a gappy sequence, where n
        // is the first number not yet assigned or reserved. N should
        // be larger than zero. For each entry and exit number, the
        // values one larger and smaller are reserved to indicate
        // "strictly above" and "strictly below". numberBlock returns
        // the smallest number not yet assigned or reserved (i.e., the
        // exit number of the last block visited, plus two, because
        // last.exit+1 is a reserved value.)
        //
        // examples:
        //
        // single node tree Root, call with n=1
        //         entry=2 Root exit=5; returns 7
        //
        // two node tree, Root->Child, call with n=1
        //         entry=2 Root exit=11; returns 13
        //         entry=5 Child exit=8
        //
        // three node tree, Root->(Left, Right), call with n=1
        //         entry=2 Root exit=17; returns 19
        // entry=5 Left exit=8;  entry=11 Right exit=14
        //
        // This is the in-order sequence of assigned and reserved numbers
        // for the last example:
        //   root     left     left      right       right       root
        //  1 2e 3 | 4 5e 6 | 7 8x 9 | 10 11e 12 | 13 14x 15 | 16 17x 18

        public static int numberBlock(this SparseTree t, ptr<Block> _addr_b, int n)
        {
            ref Block b = ref _addr_b.val;
 
            // reserve n for entry-1, assign n+1 to entry
            n++;
            t[b.ID].entry = n; 
            // reserve n+1 for entry+1, n+2 is next free number
            n += 2L;
            {
                var c = t[b.ID].child;

                while (c != null)
                {
                    n = t.numberBlock(c, n); // preserves n = next free number
                    c = t[c.ID].sibling;
                } 
                // reserve n for exit-1, assign n+1 to exit

            } 
            // reserve n for exit-1, assign n+1 to exit
            n++;
            t[b.ID].exit = n; 
            // reserve n+1 for exit+1, n+2 is next free number, returned.
            return n + 2L;

        }

        // Sibling returns a sibling of x in the dominator tree (i.e.,
        // a node with the same immediate dominator) or nil if there
        // are no remaining siblings in the arbitrary but repeatable
        // order chosen. Because the Child-Sibling order is used
        // to assign entry and exit numbers in the treewalk, those
        // numbers are also consistent with this order (i.e.,
        // Sibling(x) has entry number larger than x's exit number).
        public static ptr<Block> Sibling(this SparseTree t, ptr<Block> _addr_x)
        {
            ref Block x = ref _addr_x.val;

            return _addr_t[x.ID].sibling!;
        }

        // Child returns a child of x in the dominator tree, or
        // nil if there are none. The choice of first child is
        // arbitrary but repeatable.
        public static ptr<Block> Child(this SparseTree t, ptr<Block> _addr_x)
        {
            ref Block x = ref _addr_x.val;

            return _addr_t[x.ID].child!;
        }

        // isAncestorEq reports whether x is an ancestor of or equal to y.
        public static bool IsAncestorEq(this SparseTree t, ptr<Block> _addr_x, ptr<Block> _addr_y)
        {
            ref Block x = ref _addr_x.val;
            ref Block y = ref _addr_y.val;

            if (x == y)
            {
                return true;
            }

            var xx = _addr_t[x.ID];
            var yy = _addr_t[y.ID];
            return xx.entry <= yy.entry && yy.exit <= xx.exit;

        }

        // isAncestor reports whether x is a strict ancestor of y.
        public static bool isAncestor(this SparseTree t, ptr<Block> _addr_x, ptr<Block> _addr_y)
        {
            ref Block x = ref _addr_x.val;
            ref Block y = ref _addr_y.val;

            if (x == y)
            {
                return false;
            }

            var xx = _addr_t[x.ID];
            var yy = _addr_t[y.ID];
            return xx.entry < yy.entry && yy.exit < xx.exit;

        }

        // domorder returns a value for dominator-oriented sorting.
        // Block domination does not provide a total ordering,
        // but domorder two has useful properties.
        // (1) If domorder(x) > domorder(y) then x does not dominate y.
        // (2) If domorder(x) < domorder(y) and domorder(y) < domorder(z) and x does not dominate y,
        //     then x does not dominate z.
        // Property (1) means that blocks sorted by domorder always have a maximal dominant block first.
        // Property (2) allows searches for dominated blocks to exit early.
        public static int domorder(this SparseTree t, ptr<Block> _addr_x)
        {
            ref Block x = ref _addr_x.val;
 
            // Here is an argument that entry(x) provides the properties documented above.
            //
            // Entry and exit values are assigned in a depth-first dominator tree walk.
            // For all blocks x and y, one of the following holds:
            //
            // (x-dom-y) x dominates y => entry(x) < entry(y) < exit(y) < exit(x)
            // (y-dom-x) y dominates x => entry(y) < entry(x) < exit(x) < exit(y)
            // (x-then-y) neither x nor y dominates the other and x walked before y => entry(x) < exit(x) < entry(y) < exit(y)
            // (y-then-x) neither x nor y dominates the other and y walked before y => entry(y) < exit(y) < entry(x) < exit(x)
            //
            // entry(x) > entry(y) eliminates case x-dom-y. This provides property (1) above.
            //
            // For property (2), assume entry(x) < entry(y) and entry(y) < entry(z) and x does not dominate y.
            // entry(x) < entry(y) allows cases x-dom-y and x-then-y.
            // But by supposition, x does not dominate y. So we have x-then-y.
            //
            // For contradiction, assume x dominates z.
            // Then entry(x) < entry(z) < exit(z) < exit(x).
            // But we know x-then-y, so entry(x) < exit(x) < entry(y) < exit(y).
            // Combining those, entry(x) < entry(z) < exit(z) < exit(x) < entry(y) < exit(y).
            // By supposition, entry(y) < entry(z), which allows cases y-dom-z and y-then-z.
            // y-dom-z requires entry(y) < entry(z), but we have entry(z) < entry(y).
            // y-then-z requires exit(y) < entry(z), but we have entry(z) < exit(y).
            // We have a contradiction, so x does not dominate z, as required.
            return t[x.ID].entry;

        }
    }
}}}}
