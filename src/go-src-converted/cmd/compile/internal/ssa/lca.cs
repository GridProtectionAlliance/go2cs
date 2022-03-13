// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 13 06:01:31 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\lca.go
namespace go.cmd.compile.@internal;

using bits = math.bits_package;


// Code to compute lowest common ancestors in the dominator tree.
// https://en.wikipedia.org/wiki/Lowest_common_ancestor
// https://en.wikipedia.org/wiki/Range_minimum_query#Solution_using_constant_time_and_linearithmic_space

// lcaRange is a data structure that can compute lowest common ancestor queries
// in O(n lg n) precomputed space and O(1) time per query.

public static partial class ssa_package {

private partial struct lcaRange {
    public slice<lcaRangeBlock> blocks; // Data structure for range minimum queries.
// rangeMin[k][i] contains the ID of the minimum depth block
// in the Euler tour from positions i to i+1<<k-1, inclusive.
    public slice<slice<ID>> rangeMin;
}

private partial struct lcaRangeBlock {
    public ptr<Block> b;
    public ID parent; // parent in dominator tree.  0 = no parent (entry or unreachable)
    public ID firstChild; // first child in dominator tree
    public ID sibling; // next child of parent
    public int pos; // an index in the Euler tour where this block appears (any one of its occurrences)
    public int depth; // depth in dominator tree (root=0, its children=1, etc.)
}

private static ptr<lcaRange> makeLCArange(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    var dom = f.Idom(); 

    // Build tree
    var blocks = make_slice<lcaRangeBlock>(f.NumBlocks());
    foreach (var (_, b) in f.Blocks) {
        blocks[b.ID].b = b;
        if (dom[b.ID] == null) {
            continue; // entry or unreachable
        }
        var parent = dom[b.ID].ID;
        blocks[b.ID].parent = parent;
        blocks[b.ID].sibling = blocks[parent].firstChild;
        blocks[parent].firstChild = b.ID;
    }    var tour = make_slice<ID>(0, f.NumBlocks() * 2 - 1);
    private partial struct queueEntry {
        public ID bid; // block to work on
        public ID cid; // child we're already working on (0 = haven't started yet)
    }
    queueEntry q = new slice<queueEntry>(new queueEntry[] { {f.Entry.ID,0} });
    while (len(q) > 0) {
        var n = len(q) - 1;
        var bid = q[n].bid;
        var cid = q[n].cid;
        q = q[..(int)n]; 

        // Add block to tour.
        blocks[bid].pos = int32(len(tour));
        tour = append(tour, bid); 

        // Proceed down next child edge (if any).
        if (cid == 0) { 
            // This is our first visit to b. Set its depth.
            blocks[bid].depth = blocks[blocks[bid].parent].depth + 1; 
            // Then explore its first child.
            cid = blocks[bid].firstChild;
        }
        else
 { 
            // We've seen b before. Explore the next child.
            cid = blocks[cid].sibling;
        }
        if (cid != 0) {
            q = append(q, new queueEntry(bid,cid), new queueEntry(cid,0));
        }
    } 

    // Compute fast range-minimum query data structure
    var rangeMin = make_slice<slice<ID>>(0, bits.Len64(uint64(len(tour))));
    rangeMin = append(rangeMin, tour); // 1-size windows are just the tour itself.
    {
        nint logS = 1;
        nint s = 2;

        while (s < len(tour)) {
            var r = make_slice<ID>(len(tour) - s + 1);
            for (nint i = 0; i < len(tour) - s + 1; i++) {
                bid = rangeMin[logS - 1][i];
                var bid2 = rangeMin[logS - 1][i + s / 2];
                if (blocks[bid2].depth < blocks[bid].depth) {
                    bid = bid2;
                }
                r[i] = bid;
            (logS, s) = (logS + 1, s * 2);
            }

            rangeMin = append(rangeMin, r);
        }
    }

    return addr(new lcaRange(blocks:blocks,rangeMin:rangeMin));
}

// find returns the lowest common ancestor of a and b.
private static ptr<Block> find(this ptr<lcaRange> _addr_lca, ptr<Block> _addr_a, ptr<Block> _addr_b) {
    ref lcaRange lca = ref _addr_lca.val;
    ref Block a = ref _addr_a.val;
    ref Block b = ref _addr_b.val;

    if (a == b) {
        return _addr_a!;
    }
    var p1 = lca.blocks[a.ID].pos;
    var p2 = lca.blocks[b.ID].pos;
    if (p1 > p2) {
        (p1, p2) = (p2, p1);
    }
    var logS = uint(log64(int64(p2 - p1)));
    var bid1 = lca.rangeMin[logS][p1];
    var bid2 = lca.rangeMin[logS][p2 - 1 << (int)(logS) + 1];
    if (lca.blocks[bid1].depth < lca.blocks[bid2].depth) {
        return _addr_lca.blocks[bid1].b!;
    }
    return _addr_lca.blocks[bid2].b!;
}

} // end ssa_package
