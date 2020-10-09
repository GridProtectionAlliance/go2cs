// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 09 05:24:47 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\lca.go

using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        // Code to compute lowest common ancestors in the dominator tree.
        // https://en.wikipedia.org/wiki/Lowest_common_ancestor
        // https://en.wikipedia.org/wiki/Range_minimum_query#Solution_using_constant_time_and_linearithmic_space

        // lcaRange is a data structure that can compute lowest common ancestor queries
        // in O(n lg n) precomputed space and O(1) time per query.
        private partial struct lcaRange
        {
            public slice<lcaRangeBlock> blocks; // Data structure for range minimum queries.
// rangeMin[k][i] contains the ID of the minimum depth block
// in the Euler tour from positions i to i+1<<k-1, inclusive.
            public slice<slice<ID>> rangeMin;
        }

        private partial struct lcaRangeBlock
        {
            public ptr<Block> b;
            public ID parent; // parent in dominator tree.  0 = no parent (entry or unreachable)
            public ID firstChild; // first child in dominator tree
            public ID sibling; // next child of parent
            public int pos; // an index in the Euler tour where this block appears (any one of its occurrences)
            public int depth; // depth in dominator tree (root=0, its children=1, etc.)
        }

        private static ptr<lcaRange> makeLCArange(ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            var dom = f.Idom(); 

            // Build tree
            var blocks = make_slice<lcaRangeBlock>(f.NumBlocks());
            foreach (var (_, b) in f.Blocks)
            {
                blocks[b.ID].b = b;
                if (dom[b.ID] == null)
                {
                    continue; // entry or unreachable
                }

                var parent = dom[b.ID].ID;
                blocks[b.ID].parent = parent;
                blocks[b.ID].sibling = blocks[parent].firstChild;
                blocks[parent].firstChild = b.ID;

            } 

            // Compute euler tour ordering.
            // Each reachable block will appear #children+1 times in the tour.
            var tour = make_slice<ID>(0L, f.NumBlocks() * 2L - 1L);
            private partial struct queueEntry
            {
                public ID bid; // block to work on
                public ID cid; // child we're already working on (0 = haven't started yet)
            }
            queueEntry q = new slice<queueEntry>(new queueEntry[] { {f.Entry.ID,0} });
            while (len(q) > 0L)
            {
                var n = len(q) - 1L;
                var bid = q[n].bid;
                var cid = q[n].cid;
                q = q[..n]; 

                // Add block to tour.
                blocks[bid].pos = int32(len(tour));
                tour = append(tour, bid); 

                // Proceed down next child edge (if any).
                if (cid == 0L)
                { 
                    // This is our first visit to b. Set its depth.
                    blocks[bid].depth = blocks[blocks[bid].parent].depth + 1L; 
                    // Then explore its first child.
                    cid = blocks[bid].firstChild;

                }
                else
                { 
                    // We've seen b before. Explore the next child.
                    cid = blocks[cid].sibling;

                }

                if (cid != 0L)
                {
                    q = append(q, new queueEntry(bid,cid), new queueEntry(cid,0));
                }

            } 

            // Compute fast range-minimum query data structure
 

            // Compute fast range-minimum query data structure
            slice<slice<ID>> rangeMin = default;
            rangeMin = append(rangeMin, tour); // 1-size windows are just the tour itself.
            {
                long logS = 1L;
                long s = 2L;

                while (s < len(tour))
                {
                    var r = make_slice<ID>(len(tour) - s + 1L);
                    for (long i = 0L; i < len(tour) - s + 1L; i++)
                    {
                        bid = rangeMin[logS - 1L][i];
                        var bid2 = rangeMin[logS - 1L][i + s / 2L];
                        if (blocks[bid2].depth < blocks[bid].depth)
                        {
                            bid = bid2;
                        }

                        r[i] = bid;
                    logS = logS + 1L;
                s = s * 2L;
                    }

                    rangeMin = append(rangeMin, r);

                }

            }

            return addr(new lcaRange(blocks:blocks,rangeMin:rangeMin));

        }

        // find returns the lowest common ancestor of a and b.
        private static ptr<Block> find(this ptr<lcaRange> _addr_lca, ptr<Block> _addr_a, ptr<Block> _addr_b)
        {
            ref lcaRange lca = ref _addr_lca.val;
            ref Block a = ref _addr_a.val;
            ref Block b = ref _addr_b.val;

            if (a == b)
            {
                return _addr_a!;
            } 
            // Find the positions of a and bin the Euler tour.
            var p1 = lca.blocks[a.ID].pos;
            var p2 = lca.blocks[b.ID].pos;
            if (p1 > p2)
            {
                p1 = p2;
                p2 = p1;

            } 

            // The lowest common ancestor is the minimum depth block
            // on the tour from p1 to p2.  We've precomputed minimum
            // depth blocks for powers-of-two subsequences of the tour.
            // Combine the right two precomputed values to get the answer.
            var logS = uint(log2(int64(p2 - p1)));
            var bid1 = lca.rangeMin[logS][p1];
            var bid2 = lca.rangeMin[logS][p2 - 1L << (int)(logS) + 1L];
            if (lca.blocks[bid1].depth < lca.blocks[bid2].depth)
            {
                return _addr_lca.blocks[bid1].b!;
            }

            return _addr_lca.blocks[bid2].b!;

        }
    }
}}}}
