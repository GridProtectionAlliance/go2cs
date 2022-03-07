// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 06 22:50:13 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\looprotate.go


namespace go.cmd.compile.@internal;

public static partial class ssa_package {

    // loopRotate converts loops with a check-loop-condition-at-beginning
    // to loops with a check-loop-condition-at-end.
    // This helps loops avoid extra unnecessary jumps.
    //
    //   loop:
    //     CMPQ ...
    //     JGE exit
    //     ...
    //     JMP loop
    //   exit:
    //
    //    JMP entry
    //  loop:
    //    ...
    //  entry:
    //    CMPQ ...
    //    JLT loop
private static void loopRotate(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    var loopnest = f.loopnest();
    if (loopnest.hasIrreducible) {
        return ;
    }
    if (len(loopnest.loops) == 0) {
        return ;
    }
    var idToIdx = make_slice<nint>(f.NumBlocks());
    {
        var b__prev1 = b;

        foreach (var (__i, __b) in f.Blocks) {
            i = __i;
            b = __b;
            idToIdx[b.ID] = i;
        }
        b = b__prev1;
    }

    map after = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ID, slice<ptr<Block>>>{}; 

    // Check each loop header and decide if we want to move it.
    foreach (var (_, loop) in loopnest.loops) {
        var b = loop.header;
        ptr<Block> p; // b's in-loop predecessor
        foreach (var (_, e) in b.Preds) {
            if (e.b.Kind != BlockPlain) {
                continue;
            }
            if (loopnest.b2l[e.b.ID] != loop) {
                continue;
            }
            p = e.b;

        }        if (p == null || p == b) {
            continue;
        }
        after[p.ID] = new slice<ptr<Block>>(new ptr<Block>[] { b });
        while (true) {
            var nextIdx = idToIdx[b.ID] + 1;
            if (nextIdx >= len(f.Blocks)) { // reached end of function (maybe impossible?)
                break;

            }
            var nextb = f.Blocks[nextIdx];
            if (nextb == p) { // original loop predecessor is next
                break;

            }
            if (loopnest.b2l[nextb.ID] == loop) {
                after[p.ID] = append(after[p.ID], nextb);
            }
            b = nextb;

        } 
        // Swap b and p so that we'll handle p before b when moving blocks.
        f.Blocks[idToIdx[loop.header.ID]] = p;
        f.Blocks[idToIdx[p.ID]] = loop.header;
        (idToIdx[loop.header.ID], idToIdx[p.ID]) = (idToIdx[p.ID], idToIdx[loop.header.ID]);        {
            var b__prev2 = b;

            foreach (var (_, __b) in after[p.ID]) {
                b = __b;
                move[b.ID] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
            }
            b = b__prev2;
        }
    }    nint j = 0; 
    // Some blocks that are not part of a loop may be placed
    // between loop blocks. In order to avoid these blocks from
    // being overwritten, use a temporary slice.
    var newOrder = make_slice<ptr<Block>>(0, f.NumBlocks());
    {
        var b__prev1 = b;

        foreach (var (_, __b) in f.Blocks) {
            b = __b;
            {
                var (_, ok) = move[b.ID];

                if (ok) {
                    continue;
                }
            }

            newOrder = append(newOrder, b);
            j++;
            foreach (var (_, a) in after[b.ID]) {
                newOrder = append(newOrder, a);
                j++;
            }
        }
        b = b__prev1;
    }

    if (j != len(f.Blocks)) {
        f.Fatalf("bad reordering in looprotate");
    }
    f.Blocks = newOrder;

}

} // end ssa_package
