// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 06 22:50:07 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\layout.go


namespace go.cmd.compile.@internal;

public static partial class ssa_package {

    // layout orders basic blocks in f with the goal of minimizing control flow instructions.
    // After this phase returns, the order of f.Blocks matters and is the order
    // in which those blocks will appear in the assembly output.
private static void layout(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    f.Blocks = layoutOrder(_addr_f);
}

// Register allocation may use a different order which has constraints
// imposed by the linear-scan algorithm.
private static slice<ptr<Block>> layoutRegallocOrder(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;
 
    // remnant of an experiment; perhaps there will be another.
    return layoutOrder(_addr_f);

}

private static slice<ptr<Block>> layoutOrder(ptr<Func> _addr_f) => func((defer, _, _) => {
    ref Func f = ref _addr_f.val;

    var order = make_slice<ptr<Block>>(0, f.NumBlocks());
    var scheduled = make_slice<bool>(f.NumBlocks());
    var idToBlock = make_slice<ptr<Block>>(f.NumBlocks());
    var indegree = make_slice<nint>(f.NumBlocks());
    var posdegree = f.newSparseSet(f.NumBlocks()); // blocks with positive remaining degree
    defer(f.retSparseSet(posdegree)); 
    // blocks with zero remaining degree. Use slice to simulate a LIFO queue to implement
    // the depth-first topology sorting algorithm.
    slice<ID> zerodegree = default; 
    // LIFO queue. Track the successor blocks of the scheduled block so that when we
    // encounter loops, we choose to schedule the successor block of the most recently
    // scheduled block.
    slice<ID> succs = default;
    var exit = f.newSparseSet(f.NumBlocks()); // exit blocks
    defer(f.retSparseSet(exit)); 

    // Populate idToBlock and find exit blocks.
    {
        var b__prev1 = b;

        foreach (var (_, __b) in f.Blocks) {
            b = __b;
            idToBlock[b.ID] = b;
            if (b.Kind == BlockExit) {
                exit.add(b.ID);
            }
        }
        b = b__prev1;
    }

    while (true) {
        var changed = false;
        foreach (var (_, id) in exit.contents()) {
            var b = idToBlock[id];
NextPred:
            foreach (var (_, pe) in b.Preds) {
                var p = pe.b;
                if (exit.contains(p.ID)) {
                    continue;
                }
                foreach (var (_, s) in p.Succs) {
                    if (!exit.contains(s.b.ID)) {
                        _continueNextPred = true;
                        break;
                    }

                } 
                // All Succs are in exit; add p.
                exit.add(p.ID);
                changed = true;

            }

        }        if (!changed) {
            break;
        }
    } 

    // Initialize indegree of each block
    {
        var b__prev1 = b;

        foreach (var (_, __b) in f.Blocks) {
            b = __b;
            if (exit.contains(b.ID)) { 
                // exit blocks are always scheduled last
                continue;

            }

            indegree[b.ID] = len(b.Preds);
            if (len(b.Preds) == 0) { 
                // Push an element to the tail of the queue.
                zerodegree = append(zerodegree, b.ID);

            }
            else
 {
                posdegree.add(b.ID);
            }

        }
        b = b__prev1;
    }

    var bid = f.Entry.ID;
blockloop:
    while (true) { 
        // add block to schedule
        b = idToBlock[bid];
        order = append(order, b);
        scheduled[bid] = true;
        if (len(order) == len(f.Blocks)) {
            break;
        }
        for (var i = len(b.Succs) - 1; i >= 0; i--) {
            var c = b.Succs[i].b;
            indegree[c.ID]--;
            if (indegree[c.ID] == 0) {
                posdegree.remove(c.ID);
                zerodegree = append(zerodegree, c.ID);
            }
            else
 {
                succs = append(succs, c.ID);
            }

        } 

        // Pick the next block to schedule
        // Pick among the successor blocks that have not been scheduled yet.

        // Use likely direction if we have it.
        ptr<Block> likely;

        if (b.Likely == BranchLikely) 
            likely = b.Succs[0].b;
        else if (b.Likely == BranchUnlikely) 
            likely = b.Succs[1].b;
                if (likely != null && !scheduled[likely.ID]) {
            bid = likely.ID;
            continue;
        }
        bid = 0; 
        // TODO: improve this part
        // No successor of the previously scheduled block works.
        // Pick a zero-degree block if we can.
        while (len(zerodegree) > 0) { 
            // Pop an element from the tail of the queue.
            var cid = zerodegree[len(zerodegree) - 1];
            zerodegree = zerodegree[..(int)len(zerodegree) - 1];
            if (!scheduled[cid]) {
                bid = cid;
                _continueblockloop = true;
                break;
            }

        } 

        // Still nothing, pick the unscheduled successor block encountered most recently.
        while (len(succs) > 0) { 
            // Pop an element from the tail of the queue.
            cid = succs[len(succs) - 1];
            succs = succs[..(int)len(succs) - 1];
            if (!scheduled[cid]) {
                bid = cid;
                _continueblockloop = true;
                break;
            }

        } 

        // Still nothing, pick any non-exit block.
        while (posdegree.size() > 0) {
            cid = posdegree.pop();
            if (!scheduled[cid]) {
                bid = cid;
                _continueblockloop = true;
                break;
            }

        } 
        // Pick any exit block.
        // TODO: Order these to minimize jump distances?
        while (true) {
            cid = exit.pop();
            if (!scheduled[cid]) {
                bid = cid;
                _continueblockloop = true;
                break;
            }

        }

    }
    f.laidout = true;
    return order; 
    //f.Blocks = order
});

} // end ssa_package
