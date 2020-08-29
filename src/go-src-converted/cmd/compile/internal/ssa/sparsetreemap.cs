// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 August 29 09:24:19 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\sparsetreemap.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        // A SparseTreeMap encodes a subset of nodes within a tree
        // used for sparse-ancestor queries.
        //
        // Combined with a SparseTreeHelper, this supports an Insert
        // to add a tree node to the set and a Find operation to locate
        // the nearest tree ancestor of a given node such that the
        // ancestor is also in the set.
        //
        // Given a set of blocks {B1, B2, B3} within the dominator tree, established
        // by stm.Insert()ing B1, B2, B3, etc, a query at block B
        // (performed with stm.Find(stm, B, adjust, helper))
        // will return the member of the set that is the nearest strict
        // ancestor of B within the dominator tree, or nil if none exists.
        // The expected complexity of this operation is the log of the size
        // the set, given certain assumptions about sparsity (the log complexity
        // could be guaranteed with additional data structures whose constant-
        // factor overhead has not yet been justified.)
        //
        // The adjust parameter allows positioning of the insertion
        // and lookup points within a block -- one of
        // AdjustBefore, AdjustWithin, AdjustAfter,
        // where lookups at AdjustWithin can find insertions at
        // AdjustBefore in the same block, and lookups at AdjustAfter
        // can find insertions at either AdjustBefore or AdjustWithin
        // in the same block.  (Note that this assumes a gappy numbering
        // such that exit number or exit number is separated from its
        // nearest neighbor by at least 3).
        //
        // The Sparse Tree lookup algorithm is described by
        // Paul F. Dietz. Maintaining order in a linked list. In
        // Proceedings of the Fourteenth Annual ACM Symposium on
        // Theory of Computing, pages 122–127, May 1982.
        // and by
        // Ben Wegbreit. Faster retrieval from context trees.
        // Communications of the ACM, 19(9):526–529, September 1976.
        public partial struct SparseTreeMap // : RBTint32
        {
        }

        // A SparseTreeHelper contains indexing and allocation data
        // structures common to a collection of SparseTreeMaps, as well
        // as exposing some useful control-flow-related data to other
        // packages, such as gc.
        public partial struct SparseTreeHelper
        {
            public slice<SparseTreeNode> Sdom; // indexed by block.ID
            public slice<ref Block> Po; // exported data; the blocks, in a post-order
            public slice<ref Block> Dom; // exported data; the dominator of this block.
            public slice<int> Ponums; // exported data; Po[Ponums[b.ID]] == b; the index of b in Po
        }

        // NewSparseTreeHelper returns a SparseTreeHelper for use
        // in the gc package, for example in phi-function placement.
        public static ref SparseTreeHelper NewSparseTreeHelper(ref Func f)
        {
            var dom = f.Idom();
            var ponums = make_slice<int>(f.NumBlocks());
            var po = postorderWithNumbering(f, ponums);
            return makeSparseTreeHelper(newSparseTree(f, dom), dom, po, ponums);
        }

        private static ref SparseTreeMap NewTree(this ref SparseTreeHelper h)
        {
            return ref new SparseTreeMap();
        }

        private static ref SparseTreeHelper makeSparseTreeHelper(SparseTree sdom, slice<ref Block> dom, slice<ref Block> po, slice<int> ponums)
        {
            SparseTreeHelper helper = ref new SparseTreeHelper(Sdom:[]SparseTreeNode(sdom),Dom:dom,Po:po,Ponums:ponums,);
            return helper;
        }

        // A sparseTreeMapEntry contains the data stored in a binary search
        // data structure indexed by (dominator tree walk) entry and exit numbers.
        // Each entry is added twice, once keyed by entry-1/entry/entry+1 and
        // once keyed by exit+1/exit/exit-1.
        //
        // Within a sparse tree, the two entries added bracket all their descendant
        // entries within the tree; the first insertion is keyed by entry number,
        // which comes before all the entry and exit numbers of descendants, and
        // the second insertion is keyed by exit number, which comes after all the
        // entry and exit numbers of the descendants.
        private partial struct sparseTreeMapEntry
        {
            public ptr<SparseTreeNode> index; // references the entry and exit numbers for a block in the sparse tree
            public ptr<Block> block; // TODO: store this in a separate index.
            public ptr<sparseTreeMapEntry> sparseParent; // references the nearest ancestor of this block in the sparse tree.
            public int adjust; // at what adjustment was this node entered into the sparse tree? The same block may be entered more than once, but at different adjustments.
        }

        // Insert creates a definition within b with data x.
        // adjust indicates where in the block should be inserted:
        // AdjustBefore means defined at a phi function (visible Within or After in the same block)
        // AdjustWithin means defined within the block (visible After in the same block)
        // AdjustAfter means after the block (visible within child blocks)
        private static void Insert(this ref SparseTreeMap m, ref Block b, int adjust, object x, ref SparseTreeHelper helper)
        {
            var rbtree = (RBTint32.Value)(m);
            var blockIndex = ref helper.Sdom[b.ID];
            if (blockIndex.entry == 0L)
            { 
                // assert unreachable
                return;
            } 
            // sp will be the sparse parent in this sparse tree (nearest ancestor in the larger tree that is also in this sparse tree)
            var sp = m.findEntry(b, adjust, helper);
            sparseTreeMapEntry entry = ref new sparseTreeMapEntry(index:blockIndex,block:b,data:x,sparseParent:sp,adjust:adjust);

            var right = blockIndex.exit - adjust;
            _ = rbtree.Insert(right, entry);

            var left = blockIndex.entry + adjust;
            _ = rbtree.Insert(left, entry); 

            // This newly inserted block may now be the sparse parent of some existing nodes (the new sparse children of this block)
            // Iterate over nodes bracketed by this new node to correct their parent, but not over the proper sparse descendants of those nodes.
            var (_, d) = rbtree.Lub(left); // Lub (not EQ) of left is either right or a sparse child
            {
                ref sparseTreeMapEntry tme = d._<ref sparseTreeMapEntry>();

                while (tme != entry)
                {
                    tme.sparseParent = entry; 
                    // all descendants of tme are unchanged;
                    // next sparse sibling (or right-bracketing sparse parent == entry) is first node after tme.index.exit - tme.adjust
                    _, d = rbtree.Lub(tme.index.exit - tme.adjust);
                    tme = d._<ref sparseTreeMapEntry>();
                }

            }
        }

        // Find returns the definition visible from block b, or nil if none can be found.
        // Adjust indicates where the block should be searched.
        // AdjustBefore searches before the phi functions of b.
        // AdjustWithin searches starting at the phi functions of b.
        // AdjustAfter searches starting at the exit from the block, including normal within-block definitions.
        //
        // Note that Finds are properly nested with Inserts:
        // m.Insert(b, a) followed by m.Find(b, a) will not return the result of the insert,
        // but m.Insert(b, AdjustBefore) followed by m.Find(b, AdjustWithin) will.
        //
        // Another way to think of this is that Find searches for inputs, Insert defines outputs.
        private static void Find(this ref SparseTreeMap m, ref Block b, int adjust, ref SparseTreeHelper helper)
        {
            var v = m.findEntry(b, adjust, helper);
            if (v == null)
            {
                return null;
            }
            return v.data;
        }

        private static ref sparseTreeMapEntry findEntry(this ref SparseTreeMap m, ref Block b, int adjust, ref SparseTreeHelper helper)
        {
            var rbtree = (RBTint32.Value)(m);
            if (rbtree == null)
            {
                return null;
            }
            var blockIndex = ref helper.Sdom[b.ID]; 

            // The Glb (not EQ) of this probe is either the entry-indexed end of a sparse parent
            // or the exit-indexed end of a sparse sibling
            var (_, v) = rbtree.Glb(blockIndex.entry + adjust);

            if (v == null)
            {
                return null;
            }
            ref sparseTreeMapEntry otherEntry = v._<ref sparseTreeMapEntry>();
            if (otherEntry.index.exit >= blockIndex.exit)
            { // otherEntry exit after blockIndex exit; therefore, brackets
                return otherEntry;
            } 
            // otherEntry is a sparse Sibling, and shares the same sparse parent (nearest ancestor within larger tree)
            var sp = otherEntry.sparseParent;
            if (sp != null)
            {
                if (sp.index.exit < blockIndex.exit)
                { // no ancestor found
                    return null;
                }
                return sp;
            }
            return null;
        }

        private static @string String(this ref SparseTreeMap m)
        {
            var tree = (RBTint32.Value)(m);
            return tree.String();
        }

        private static @string String(this ref sparseTreeMapEntry e)
        {
            if (e == null)
            {
                return "nil";
            }
            return fmt.Sprintf("(index=%v, block=%v, data=%v)->%v", e.index, e.block, e.data, e.sparseParent);
        }
    }
}}}}
