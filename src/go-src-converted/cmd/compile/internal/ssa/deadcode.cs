// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 06 22:49:35 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\deadcode.go
using src = go.cmd.@internal.src_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class ssa_package {

    // findlive returns the reachable blocks and live values in f.
    // The caller should call f.retDeadcodeLive(live) when it is done with it.
private static (slice<bool>, slice<bool>) findlive(ptr<Func> _addr_f) {
    slice<bool> reachable = default;
    slice<bool> live = default;
    ref Func f = ref _addr_f.val;

    reachable = ReachableBlocks(_addr_f);
    slice<ptr<Value>> order = default;
    live, order = liveValues(_addr_f, reachable);
    f.retDeadcodeLiveOrderStmts(order);
    return ;
}

// ReachableBlocks returns the reachable blocks in f.
public static slice<bool> ReachableBlocks(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    var reachable = make_slice<bool>(f.NumBlocks());
    reachable[f.Entry.ID] = true;
    var p = make_slice<ptr<Block>>(0, 64); // stack-like worklist
    p = append(p, f.Entry);
    while (len(p) > 0) { 
        // Pop a reachable block
        var b = p[len(p) - 1];
        p = p[..(int)len(p) - 1]; 
        // Mark successors as reachable
        var s = b.Succs;
        if (b.Kind == BlockFirst) {
            s = s[..(int)1];
        }
        foreach (var (_, e) in s) {
            var c = e.b;
            if (int(c.ID) >= len(reachable)) {
                f.Fatalf("block %s >= f.NumBlocks()=%d?", c, len(reachable));
            }
            if (!reachable[c.ID]) {
                reachable[c.ID] = true;
                p = append(p, c); // push
            }

        }
    }
    return reachable;

}

// liveValues returns the live values in f and a list of values that are eligible
// to be statements in reversed data flow order.
// The second result is used to help conserve statement boundaries for debugging.
// reachable is a map from block ID to whether the block is reachable.
// The caller should call f.retDeadcodeLive(live) and f.retDeadcodeLiveOrderStmts(liveOrderStmts)
// when they are done with the return values.
private static (slice<bool>, slice<ptr<Value>>) liveValues(ptr<Func> _addr_f, slice<bool> reachable) => func((defer, _, _) => {
    slice<bool> live = default;
    slice<ptr<Value>> liveOrderStmts = default;
    ref Func f = ref _addr_f.val;

    live = f.newDeadcodeLive();
    if (cap(live) < f.NumValues()) {
        live = make_slice<bool>(f.NumValues());
    }
    else
 {
        live = live[..(int)f.NumValues()];
        {
            var i__prev1 = i;

            foreach (var (__i) in live) {
                i = __i;
                live[i] = false;
            }

            i = i__prev1;
        }
    }
    liveOrderStmts = f.newDeadcodeLiveOrderStmts();
    liveOrderStmts = liveOrderStmts[..(int)0]; 

    // After regalloc, consider all values to be live.
    // See the comment at the top of regalloc.go and in deadcode for details.
    if (f.RegAlloc != null) {
        {
            var i__prev1 = i;

            foreach (var (__i) in live) {
                i = __i;
                live[i] = true;
            }

            i = i__prev1;
        }

        return ;

    }
    map<nint, bool> liveInlIdx = default;
    var pt = f.Config.ctxt.PosTable;
    {
        var b__prev1 = b;

        foreach (var (_, __b) in f.Blocks) {
            b = __b;
            {
                var v__prev2 = v;

                foreach (var (_, __v) in b.Values) {
                    v = __v;
                    var i = pt.Pos(v.Pos).Base().InliningIndex();
                    if (i < 0) {
                        continue;
                    }
                    if (liveInlIdx == null) {
                        liveInlIdx = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<nint, bool>{};
                    }
                    liveInlIdx[i] = true;
                }

                v = v__prev2;
            }

            i = pt.Pos(b.Pos).Base().InliningIndex();
            if (i < 0) {
                continue;
            }

            if (liveInlIdx == null) {
                liveInlIdx = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<nint, bool>{};
            }

            liveInlIdx[i] = true;

        }
        b = b__prev1;
    }

    var q = f.Cache.deadcode.q[..(int)0];
    defer(() => {
        f.Cache.deadcode.q = q;
    }()); 

    // Starting set: all control values of reachable blocks are live.
    // Calls are live (because callee can observe the memory state).
    {
        var b__prev1 = b;

        foreach (var (_, __b) in f.Blocks) {
            b = __b;
            if (!reachable[b.ID]) {
                continue;
            }
            {
                var v__prev2 = v;

                foreach (var (_, __v) in b.ControlValues()) {
                    v = __v;
                    if (!live[v.ID]) {
                        live[v.ID] = true;
                        q = append(q, v);
                        if (v.Pos.IsStmt() != src.PosNotStmt) {
                            liveOrderStmts = append(liveOrderStmts, v);
                        }
                    }
                }

                v = v__prev2;
            }

            {
                var v__prev2 = v;

                foreach (var (_, __v) in b.Values) {
                    v = __v;
                    if ((opcodeTable[v.Op].call || opcodeTable[v.Op].hasSideEffects) && !live[v.ID]) {
                        live[v.ID] = true;
                        q = append(q, v);
                        if (v.Pos.IsStmt() != src.PosNotStmt) {
                            liveOrderStmts = append(liveOrderStmts, v);
                        }
                    }
                    if (v.Type.IsVoid() && !live[v.ID]) { 
                        // The only Void ops are nil checks and inline marks.  We must keep these.
                        if (v.Op == OpInlMark && !liveInlIdx[int(v.AuxInt)]) { 
                            // We don't need marks for bodies that
                            // have been completely optimized away.
                            // TODO: save marks only for bodies which
                            // have a faulting instruction or a call?
                            continue;

                        }

                        live[v.ID] = true;
                        q = append(q, v);
                        if (v.Pos.IsStmt() != src.PosNotStmt) {
                            liveOrderStmts = append(liveOrderStmts, v);
                        }

                    }

                }

                v = v__prev2;
            }
        }
        b = b__prev1;
    }

    while (len(q) > 0) { 
        // pop a reachable value
        var v = q[len(q) - 1];
        q = q[..(int)len(q) - 1];
        {
            var i__prev2 = i;

            foreach (var (__i, __x) in v.Args) {
                i = __i;
                x = __x;
                if (v.Op == OpPhi && !reachable[v.Block.Preds[i].b.ID]) {
                    continue;
                }
                if (!live[x.ID]) {
                    live[x.ID] = true;
                    q = append(q, x); // push
                    if (x.Pos.IsStmt() != src.PosNotStmt) {
                        liveOrderStmts = append(liveOrderStmts, x);
                    }

                }

            }

            i = i__prev2;
        }
    }

    return ;

});

// deadcode removes dead code from f.
private static void deadcode(ptr<Func> _addr_f) => func((defer, _, _) => {
    ref Func f = ref _addr_f.val;
 
    // deadcode after regalloc is forbidden for now. Regalloc
    // doesn't quite generate legal SSA which will lead to some
    // required moves being eliminated. See the comment at the
    // top of regalloc.go for details.
    if (f.RegAlloc != null) {
        f.Fatalf("deadcode after regalloc");
    }
    var reachable = ReachableBlocks(_addr_f); 

    // Get rid of edges from dead to live code.
    {
        var b__prev1 = b;

        foreach (var (_, __b) in f.Blocks) {
            b = __b;
            if (reachable[b.ID]) {
                continue;
            }
            {
                nint i__prev2 = i;

                nint i = 0;

                while (i < len(b.Succs)) {
                    var e = b.Succs[i];
                    if (reachable[e.b.ID]) {
                        b.removeEdge(i);
                    }
                    else
 {
                        i++;
                    }

                }


                i = i__prev2;
            }

        }
        b = b__prev1;
    }

    {
        var b__prev1 = b;

        foreach (var (_, __b) in f.Blocks) {
            b = __b;
            if (!reachable[b.ID]) {
                continue;
            }
            if (b.Kind != BlockFirst) {
                continue;
            }
            b.removeEdge(1);
            b.Kind = BlockPlain;
            b.Likely = BranchUnknown;
        }
        b = b__prev1;
    }

    copyelim(f); 

    // Find live values.
    var (live, order) = liveValues(_addr_f, reachable);
    defer(f.retDeadcodeLive(live));
    defer(f.retDeadcodeLiveOrderStmts(order)); 

    // Remove dead & duplicate entries from namedValues map.
    var s = f.newSparseSet(f.NumValues());
    defer(f.retSparseSet(s));
    i = 0;
    foreach (var (_, name) in f.Names) {
        nint j = 0;
        s.clear();
        var values = f.NamedValues[name.val];
        {
            var v__prev2 = v;

            foreach (var (_, __v) in values) {
                v = __v;
                if (live[v.ID] && !s.contains(v.ID)) {
                    values[j] = v;
                    j++;
                    s.add(v.ID);
                }
            }

            v = v__prev2;
        }

        if (j == 0) {
            delete(f.NamedValues, name.val);
        }
        else
 {
            f.Names[i] = name;
            i++;
            for (var k = len(values) - 1; k >= j; k--) {
                values[k] = null;
            }

            f.NamedValues[name.val] = values[..(int)j];
        }
    }    var clearNames = f.Names[(int)i..];
    {
        nint j__prev1 = j;

        foreach (var (__j) in clearNames) {
            j = __j;
            clearNames[j] = null;
        }
        j = j__prev1;
    }

    f.Names = f.Names[..(int)i];

    var pendingLines = f.cachedLineStarts; // Holds statement boundaries that need to be moved to a new value/block
    pendingLines.clear(); 

    // Unlink values and conserve statement boundaries
    {
        nint i__prev1 = i;
        var b__prev1 = b;

        foreach (var (__i, __b) in f.Blocks) {
            i = __i;
            b = __b;
            if (!reachable[b.ID]) { 
                // TODO what if control is statement boundary? Too late here.
                b.ResetControls();

            }

            {
                var v__prev2 = v;

                foreach (var (_, __v) in b.Values) {
                    v = __v;
                    if (!live[v.ID]) {
                        v.resetArgs();
                        if (v.Pos.IsStmt() == src.PosIsStmt && reachable[b.ID]) {
                            pendingLines.set(v.Pos, int32(i)); // TODO could be more than one pos for a line
                        }

                    }

                }

                v = v__prev2;
            }
        }
        i = i__prev1;
        b = b__prev1;
    }

    {
        nint i__prev1 = i;

        for (i = len(order) - 1; i >= 0; i--) {
            var w = order[i];
            {
                nint j__prev1 = j;

                j = pendingLines.get(w.Pos);

                if (j > -1 && f.Blocks[j] == w.Block) {
                    w.Pos = w.Pos.WithIsStmt();
                    pendingLines.remove(w.Pos);
                }

                j = j__prev1;

            }

        }

        i = i__prev1;
    } 

    // Any boundary that failed to match a live value can move to a block end
    pendingLines.foreachEntry((j, l, bi) => {
        var b = f.Blocks[bi];
        if (b.Pos.Line() == l && b.Pos.FileIndex() == j) {
            b.Pos = b.Pos.WithIsStmt();
        }
    }); 

    // Remove dead values from blocks' value list. Return dead
    // values to the allocator.
    {
        var b__prev1 = b;

        foreach (var (_, __b) in f.Blocks) {
            b = __b;
            i = 0;
            {
                var v__prev2 = v;

                foreach (var (_, __v) in b.Values) {
                    v = __v;
                    if (live[v.ID]) {
                        b.Values[i] = v;
                        i++;
                    }
                    else
 {
                        f.freeValue(v);
                    }

                }

                v = v__prev2;
            }

            b.truncateValues(i);

        }
        b = b__prev1;
    }

    i = 0;
    {
        var b__prev1 = b;

        foreach (var (_, __b) in f.WBLoads) {
            b = __b;
            if (reachable[b.ID]) {
                f.WBLoads[i] = b;
                i++;
            }
        }
        b = b__prev1;
    }

    var clearWBLoads = f.WBLoads[(int)i..];
    {
        nint j__prev1 = j;

        foreach (var (__j) in clearWBLoads) {
            j = __j;
            clearWBLoads[j] = null;
        }
        j = j__prev1;
    }

    f.WBLoads = f.WBLoads[..(int)i]; 

    // Remove unreachable blocks. Return dead blocks to allocator.
    i = 0;
    {
        var b__prev1 = b;

        foreach (var (_, __b) in f.Blocks) {
            b = __b;
            if (reachable[b.ID]) {
                f.Blocks[i] = b;
                i++;
            }
            else
 {
                if (len(b.Values) > 0) {
                    b.Fatalf("live values in unreachable block %v: %v", b, b.Values);
                }
                f.freeBlock(b);
            }

        }
        b = b__prev1;
    }

    var tail = f.Blocks[(int)i..];
    {
        nint j__prev1 = j;

        foreach (var (__j) in tail) {
            j = __j;
            tail[j] = null;
        }
        j = j__prev1;
    }

    f.Blocks = f.Blocks[..(int)i];

});

// removeEdge removes the i'th outgoing edge from b (and
// the corresponding incoming edge from b.Succs[i].b).
private static void removeEdge(this ptr<Block> _addr_b, nint i) {
    ref Block b = ref _addr_b.val;

    var e = b.Succs[i];
    var c = e.b;
    var j = e.i; 

    // Adjust b.Succs
    b.removeSucc(i); 

    // Adjust c.Preds
    c.removePred(j); 

    // Remove phi args from c's phis.
    var n = len(c.Preds);
    foreach (var (_, v) in c.Values) {
        if (v.Op != OpPhi) {
            continue;
        }
        v.Args[j].Uses--;
        v.Args[j] = v.Args[n];
        v.Args[n] = null;
        v.Args = v.Args[..(int)n];
        phielimValue(v); 
        // Note: this is trickier than it looks. Replacing
        // a Phi with a Copy can in general cause problems because
        // Phi and Copy don't have exactly the same semantics.
        // Phi arguments always come from a predecessor block,
        // whereas copies don't. This matters in loops like:
        // 1: x = (Phi y)
        //    y = (Add x 1)
        //    goto 1
        // If we replace Phi->Copy, we get
        // 1: x = (Copy y)
        //    y = (Add x 1)
        //    goto 1
        // (Phi y) refers to the *previous* value of y, whereas
        // (Copy y) refers to the *current* value of y.
        // The modified code has a cycle and the scheduler
        // will barf on it.
        //
        // Fortunately, this situation can only happen for dead
        // code loops. We know the code we're working with is
        // not dead, so we're ok.
        // Proof: If we have a potential bad cycle, we have a
        // situation like this:
        //   x = (Phi z)
        //   y = (op1 x ...)
        //   z = (op2 y ...)
        // Where opX are not Phi ops. But such a situation
        // implies a cycle in the dominator graph. In the
        // example, x.Block dominates y.Block, y.Block dominates
        // z.Block, and z.Block dominates x.Block (treating
        // "dominates" as reflexive).  Cycles in the dominator
        // graph can only happen in an unreachable cycle.
    }
}

} // end ssa_package
