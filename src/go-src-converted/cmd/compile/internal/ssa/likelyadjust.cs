// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 09 05:24:49 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\likelyadjust.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        private partial struct loop
        {
            public ptr<Block> header; // The header node of this (reducible) loop
            public ptr<loop> outer; // loop containing this loop

// By default, children, exits, and depth are not initialized.
            public slice<ptr<loop>> children; // loops nested directly within this loop. Initialized by assembleChildren().
            public slice<ptr<Block>> exits; // exits records blocks reached by exits from this loop. Initialized by findExits().

// Next three fields used by regalloc and/or
// aid in computation of inner-ness and list of blocks.
            public int nBlocks; // Number of blocks in this loop but not within inner loops
            public short depth; // Nesting depth of the loop; 1 is outermost. Initialized by calculateDepths().
            public bool isInner; // True if never discovered to contain a loop

// register allocation uses this.
            public bool containsUnavoidableCall; // True if all paths through the loop have a call
        }

        // outerinner records that outer contains inner
        public static void outerinner(this SparseTree sdom, ptr<loop> _addr_outer, ptr<loop> _addr_inner)
        {
            ref loop outer = ref _addr_outer.val;
            ref loop inner = ref _addr_inner.val;
 
            // There could be other outer loops found in some random order,
            // locate the new outer loop appropriately among them.

            // Outer loop headers dominate inner loop headers.
            // Use this to put the "new" "outer" loop in the right place.
            var oldouter = inner.outer;
            while (oldouter != null && sdom.isAncestor(outer.header, oldouter.header))
            {
                inner = oldouter;
                oldouter = inner.outer;
            }

            if (outer == oldouter)
            {
                return ;
            }

            if (oldouter != null)
            {
                sdom.outerinner(oldouter, outer);
            }

            inner.outer = outer;
            outer.isInner = false;

        }

        private static bool checkContainsCall(ptr<Block> _addr_bb)
        {
            ref Block bb = ref _addr_bb.val;

            if (bb.Kind == BlockDefer)
            {
                return true;
            }

            foreach (var (_, v) in bb.Values)
            {
                if (opcodeTable[v.Op].call)
                {
                    return true;
                }

            }
            return false;

        }

        private partial struct loopnest
        {
            public ptr<Func> f;
            public slice<ptr<loop>> b2l;
            public slice<ptr<Block>> po;
            public SparseTree sdom;
            public slice<ptr<loop>> loops;
            public bool hasIrreducible; // TODO current treatment of irreducible loops is very flaky, if accurate loops are needed, must punt at function level.

// Record which of the lazily initialized fields have actually been initialized.
            public bool initializedChildren;
            public bool initializedDepth;
            public bool initializedExits;
        }

        private static sbyte min8(sbyte a, sbyte b)
        {
            if (a < b)
            {
                return a;
            }

            return b;

        }

        private static sbyte max8(sbyte a, sbyte b)
        {
            if (a > b)
            {
                return a;
            }

            return b;

        }

        private static readonly long blDEFAULT = (long)0L;
        private static readonly var blMin = blDEFAULT;
        private static readonly long blCALL = (long)1L;
        private static readonly long blRET = (long)2L;
        private static readonly long blEXIT = (long)3L;


        private static array<@string> bllikelies = new array<@string>(new @string[] { "default", "call", "ret", "exit" });

        private static @string describePredictionAgrees(ptr<Block> _addr_b, BranchPrediction prediction)
        {
            ref Block b = ref _addr_b.val;

            @string s = "";
            if (prediction == b.Likely)
            {
                s = " (agrees with previous)";
            }
            else if (b.Likely != BranchUnknown)
            {
                s = " (disagrees with previous, ignored)";
            }

            return s;

        }

        private static void describeBranchPrediction(ptr<Func> _addr_f, ptr<Block> _addr_b, sbyte likely, sbyte not, BranchPrediction prediction)
        {
            ref Func f = ref _addr_f.val;
            ref Block b = ref _addr_b.val;

            f.Warnl(b.Pos, "Branch prediction rule %s < %s%s", bllikelies[likely - blMin], bllikelies[not - blMin], describePredictionAgrees(_addr_b, prediction));
        }

        private static void likelyadjust(ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;
 
            // The values assigned to certain and local only matter
            // in their rank order.  0 is default, more positive
            // is less likely. It's possible to assign a negative
            // unlikeliness (though not currently the case).
            var certain = make_slice<sbyte>(f.NumBlocks()); // In the long run, all outcomes are at least this bad. Mainly for Exit
            var local = make_slice<sbyte>(f.NumBlocks()); // for our immediate predecessors.

            var po = f.postorder();
            var nest = f.loopnest();
            var b2l = nest.b2l;

            foreach (var (_, b) in po)
            {

                if (b.Kind == BlockExit) 
                    // Very unlikely.
                    local[b.ID] = blEXIT;
                    certain[b.ID] = blEXIT; 

                    // Ret, it depends.
                else if (b.Kind == BlockRet || b.Kind == BlockRetJmp) 
                    local[b.ID] = blRET;
                    certain[b.ID] = blRET; 

                    // Calls. TODO not all calls are equal, names give useful clues.
                    // Any name-based heuristics are only relative to other calls,
                    // and less influential than inferences from loop structure.
                else if (b.Kind == BlockDefer) 
                    local[b.ID] = blCALL;
                    certain[b.ID] = max8(blCALL, certain[b.Succs[0L].b.ID]);
                else 
                    if (len(b.Succs) == 1L)
                    {
                        certain[b.ID] = certain[b.Succs[0L].b.ID];
                    }
                    else if (len(b.Succs) == 2L)
                    { 
                        // If successor is an unvisited backedge, it's in loop and we don't care.
                        // Its default unlikely is also zero which is consistent with favoring loop edges.
                        // Notice that this can act like a "reset" on unlikeliness at loops; the
                        // default "everything returns" unlikeliness is erased by min with the
                        // backedge likeliness; however a loop with calls on every path will be
                        // tagged with call cost. Net effect is that loop entry is favored.
                        var b0 = b.Succs[0L].b.ID;
                        var b1 = b.Succs[1L].b.ID;
                        certain[b.ID] = min8(certain[b0], certain[b1]);

                        var l = b2l[b.ID];
                        var l0 = b2l[b0];
                        var l1 = b2l[b1];

                        var prediction = b.Likely; 
                        // Weak loop heuristic -- both source and at least one dest are in loops,
                        // and there is a difference in the destinations.
                        // TODO what is best arrangement for nested loops?
                        if (l != null && l0 != l1)
                        {
                            var noprediction = false;

                            // prefer not to exit loops
                            if (l1 == null) 
                                prediction = BranchLikely;
                            else if (l0 == null) 
                                prediction = BranchUnlikely; 

                                // prefer to stay in loop, not exit to outer.
                            else if (l == l0) 
                                prediction = BranchLikely;
                            else if (l == l1) 
                                prediction = BranchUnlikely;
                            else 
                                noprediction = true;
                                                        if (f.pass.debug > 0L && !noprediction)
                            {
                                f.Warnl(b.Pos, "Branch prediction rule stay in loop%s", describePredictionAgrees(_addr_b, prediction));
                            }

                        }
                        else
                        { 
                            // Lacking loop structure, fall back on heuristics.
                            if (certain[b1] > certain[b0])
                            {
                                prediction = BranchLikely;
                                if (f.pass.debug > 0L)
                                {
                                    describeBranchPrediction(_addr_f, _addr_b, certain[b0], certain[b1], prediction);
                                }

                            }
                            else if (certain[b0] > certain[b1])
                            {
                                prediction = BranchUnlikely;
                                if (f.pass.debug > 0L)
                                {
                                    describeBranchPrediction(_addr_f, _addr_b, certain[b1], certain[b0], prediction);
                                }

                            }
                            else if (local[b1] > local[b0])
                            {
                                prediction = BranchLikely;
                                if (f.pass.debug > 0L)
                                {
                                    describeBranchPrediction(_addr_f, _addr_b, local[b0], local[b1], prediction);
                                }

                            }
                            else if (local[b0] > local[b1])
                            {
                                prediction = BranchUnlikely;
                                if (f.pass.debug > 0L)
                                {
                                    describeBranchPrediction(_addr_f, _addr_b, local[b1], local[b0], prediction);
                                }

                            }

                        }

                        if (b.Likely != prediction)
                        {
                            if (b.Likely == BranchUnknown)
                            {
                                b.Likely = prediction;
                            }

                        }

                    } 
                    // Look for calls in the block.  If there is one, make this block unlikely.
                    foreach (var (_, v) in b.Values)
                    {
                        if (opcodeTable[v.Op].call)
                        {
                            local[b.ID] = blCALL;
                            certain[b.ID] = max8(blCALL, certain[b.Succs[0L].b.ID]);
                        }

                    }
                                if (f.pass.debug > 2L)
                {
                    f.Warnl(b.Pos, "BP: Block %s, local=%s, certain=%s", b, bllikelies[local[b.ID] - blMin], bllikelies[certain[b.ID] - blMin]);
                }

            }

        }

        private static @string String(this ptr<loop> _addr_l)
        {
            ref loop l = ref _addr_l.val;

            return fmt.Sprintf("hdr:%s", l.header);
        }

        private static @string LongString(this ptr<loop> _addr_l)
        {
            ref loop l = ref _addr_l.val;

            @string i = "";
            @string o = "";
            if (l.isInner)
            {
                i = ", INNER";
            }

            if (l.outer != null)
            {
                o = ", o=" + l.outer.header.String();
            }

            return fmt.Sprintf("hdr:%s%s%s", l.header, i, o);

        }

        private static bool isWithinOrEq(this ptr<loop> _addr_l, ptr<loop> _addr_ll)
        {
            ref loop l = ref _addr_l.val;
            ref loop ll = ref _addr_ll.val;

            if (ll == null)
            { // nil means whole program
                return true;

            }

            while (l != null)
            {
                if (l == ll)
                {
                    return true;
                l = l.outer;
                }

            }

            return false;

        }

        // nearestOuterLoop returns the outer loop of loop most nearly
        // containing block b; the header must dominate b.  loop itself
        // is assumed to not be that loop. For acceptable performance,
        // we're relying on loop nests to not be terribly deep.
        private static ptr<loop> nearestOuterLoop(this ptr<loop> _addr_l, SparseTree sdom, ptr<Block> _addr_b)
        {
            ref loop l = ref _addr_l.val;
            ref Block b = ref _addr_b.val;

            ptr<loop> o;
            o = l.outer;

            while (o != null && !sdom.IsAncestorEq(o.header, b))
            {
                o = o.outer;
            }

            return _addr_o!;

        }

        private static ptr<loopnest> loopnestfor(ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            var po = f.postorder();
            var sdom = f.Sdom();
            var b2l = make_slice<ptr<loop>>(f.NumBlocks());
            var loops = make_slice<ptr<loop>>(0L);
            var visited = make_slice<bool>(f.NumBlocks());
            var sawIrred = false;

            if (f.pass.debug > 2L)
            {
                fmt.Printf("loop finding in %s\n", f.Name);
            } 

            // Reducible-loop-nest-finding.
            {
                var b__prev1 = b;

                foreach (var (_, __b) in po)
                {
                    b = __b;
                    if (f.pass != null && f.pass.debug > 3L)
                    {
                        fmt.Printf("loop finding at %s\n", b);
                    }

                    ptr<loop> innermost; // innermost header reachable from this block

                    // IF any successor s of b is in a loop headed by h
                    // AND h dominates b
                    // THEN b is in the loop headed by h.
                    //
                    // Choose the first/innermost such h.
                    //
                    // IF s itself dominates b, then s is a loop header;
                    // and there may be more than one such s.
                    // Since there's at most 2 successors, the inner/outer ordering
                    // between them can be established with simple comparisons.
                    foreach (var (_, e) in b.Succs)
                    {
                        var bb = e.b;
                        var l = b2l[bb.ID];

                        if (sdom.IsAncestorEq(bb, b))
                        { // Found a loop header
                            if (f.pass != null && f.pass.debug > 4L)
                            {
                                fmt.Printf("loop finding    succ %s of %s is header\n", bb.String(), b.String());
                            }

                            if (l == null)
                            {
                                l = addr(new loop(header:bb,isInner:true));
                                loops = append(loops, l);
                                b2l[bb.ID] = l;
                            }

                        }
                        else if (!visited[bb.ID])
                        { // Found an irreducible loop
                            sawIrred = true;
                            if (f.pass != null && f.pass.debug > 4L)
                            {
                                fmt.Printf("loop finding    succ %s of %s is IRRED, in %s\n", bb.String(), b.String(), f.Name);
                            }

                        }
                        else if (l != null)
                        { 
                            // TODO handle case where l is irreducible.
                            // Perhaps a loop header is inherited.
                            // is there any loop containing our successor whose
                            // header dominates b?
                            if (!sdom.IsAncestorEq(l.header, b))
                            {
                                l = l.nearestOuterLoop(sdom, b);
                            }

                            if (f.pass != null && f.pass.debug > 4L)
                            {
                                if (l == null)
                                {
                                    fmt.Printf("loop finding    succ %s of %s has no loop\n", bb.String(), b.String());
                                }
                                else
                                {
                                    fmt.Printf("loop finding    succ %s of %s provides loop with header %s\n", bb.String(), b.String(), l.header.String());
                                }

                            }

                        }
                        else
                        { // No loop
                            if (f.pass != null && f.pass.debug > 4L)
                            {
                                fmt.Printf("loop finding    succ %s of %s has no loop\n", bb.String(), b.String());
                            }

                        }

                        if (l == null || innermost == l)
                        {
                            continue;
                        }

                        if (innermost == null)
                        {
                            innermost = l;
                            continue;
                        }

                        if (sdom.isAncestor(innermost.header, l.header))
                        {
                            sdom.outerinner(innermost, l);
                            innermost = l;
                        }
                        else if (sdom.isAncestor(l.header, innermost.header))
                        {
                            sdom.outerinner(l, innermost);
                        }

                    }
                    if (innermost != null)
                    {
                        b2l[b.ID] = innermost;
                        innermost.nBlocks++;
                    }

                    visited[b.ID] = true;

                }

                b = b__prev1;
            }

            ptr<loopnest> ln = addr(new loopnest(f:f,b2l:b2l,po:po,sdom:sdom,loops:loops,hasIrreducible:sawIrred)); 

            // Calculate containsUnavoidableCall for regalloc
            var dominatedByCall = make_slice<bool>(f.NumBlocks());
            {
                var b__prev1 = b;

                foreach (var (_, __b) in po)
                {
                    b = __b;
                    if (checkContainsCall(_addr_b))
                    {
                        dominatedByCall[b.ID] = true;
                    }

                } 
                // Run dfs to find path through the loop that avoids all calls.
                // Such path either escapes loop or return back to header.
                // It isn't enough to have exit not dominated by any call, for example:
                // ... some loop
                // call1   call2
                //   \      /
                //     exit
                // ...
                // exit is not dominated by any call, but we don't have call-free path to it.

                b = b__prev1;
            }

            {
                var l__prev1 = l;

                foreach (var (_, __l) in loops)
                {
                    l = __l; 
                    // Header contains call.
                    if (dominatedByCall[l.header.ID])
                    {
                        l.containsUnavoidableCall = true;
                        continue;
                    }

                    var callfreepath = false;
                    var tovisit = make_slice<ptr<Block>>(0L, len(l.header.Succs)); 
                    // Push all non-loop non-exit successors of header onto toVisit.
                    {
                        var s__prev2 = s;

                        foreach (var (_, __s) in l.header.Succs)
                        {
                            s = __s;
                            var nb = s.Block(); 
                            // This corresponds to loop with zero iterations.
                            if (!l.iterationEnd(nb, b2l))
                            {
                                tovisit = append(tovisit, nb);
                            }

                        }

                        s = s__prev2;
                    }

                    while (len(tovisit) > 0L)
                    {
                        var cur = tovisit[len(tovisit) - 1L];
                        tovisit = tovisit[..len(tovisit) - 1L];
                        if (dominatedByCall[cur.ID])
                        {
                            continue;
                        } 
                        // Record visited in dominatedByCall.
                        dominatedByCall[cur.ID] = true;
                        {
                            var s__prev3 = s;

                            foreach (var (_, __s) in cur.Succs)
                            {
                                s = __s;
                                nb = s.Block();
                                if (l.iterationEnd(nb, b2l))
                                {
                                    callfreepath = true;
                                }

                                if (!dominatedByCall[nb.ID])
                                {
                                    tovisit = append(tovisit, nb);
                                }

                            }

                            s = s__prev3;
                        }

                        if (callfreepath)
                        {
                            break;
                        }

                    }

                    if (!callfreepath)
                    {
                        l.containsUnavoidableCall = true;
                    }

                } 

                // Curious about the loopiness? "-d=ssa/likelyadjust/stats"

                l = l__prev1;
            }

            if (f.pass != null && f.pass.stats > 0L && len(loops) > 0L)
            {
                ln.assembleChildren();
                ln.calculateDepths();
                ln.findExits(); 

                // Note stats for non-innermost loops are slightly flawed because
                // they don't account for inner loop exits that span multiple levels.

                {
                    var l__prev1 = l;

                    foreach (var (_, __l) in loops)
                    {
                        l = __l;
                        var x = len(l.exits);
                        long cf = 0L;
                        if (!l.containsUnavoidableCall)
                        {
                            cf = 1L;
                        }

                        long inner = 0L;
                        if (l.isInner)
                        {
                            inner++;
                        }

                        f.LogStat("loopstats:", l.depth, "depth", x, "exits", inner, "is_inner", cf, "always_calls", l.nBlocks, "n_blocks");

                    }

                    l = l__prev1;
                }
            }

            if (f.pass != null && f.pass.debug > 1L && len(loops) > 0L)
            {
                fmt.Printf("Loops in %s:\n", f.Name);
                {
                    var l__prev1 = l;

                    foreach (var (_, __l) in loops)
                    {
                        l = __l;
                        fmt.Printf("%s, b=", l.LongString());
                        {
                            var b__prev2 = b;

                            foreach (var (_, __b) in f.Blocks)
                            {
                                b = __b;
                                if (b2l[b.ID] == l)
                                {
                                    fmt.Printf(" %s", b);
                                }

                            }

                            b = b__prev2;
                        }

                        fmt.Print("\n");

                    }

                    l = l__prev1;
                }

                fmt.Printf("Nonloop blocks in %s:", f.Name);
                {
                    var b__prev1 = b;

                    foreach (var (_, __b) in f.Blocks)
                    {
                        b = __b;
                        if (b2l[b.ID] == null)
                        {
                            fmt.Printf(" %s", b);
                        }

                    }

                    b = b__prev1;
                }

                fmt.Print("\n");

            }

            return _addr_ln!;

        }

        // assembleChildren initializes the children field of each
        // loop in the nest.  Loop A is a child of loop B if A is
        // directly nested within B (based on the reducible-loops
        // detection above)
        private static void assembleChildren(this ptr<loopnest> _addr_ln)
        {
            ref loopnest ln = ref _addr_ln.val;

            if (ln.initializedChildren)
            {
                return ;
            }

            foreach (var (_, l) in ln.loops)
            {
                if (l.outer != null)
                {
                    l.outer.children = append(l.outer.children, l);
                }

            }
            ln.initializedChildren = true;

        }

        // calculateDepths uses the children field of loops
        // to determine the nesting depth (outer=1) of each
        // loop.  This is helpful for finding exit edges.
        private static void calculateDepths(this ptr<loopnest> _addr_ln)
        {
            ref loopnest ln = ref _addr_ln.val;

            if (ln.initializedDepth)
            {
                return ;
            }

            ln.assembleChildren();
            foreach (var (_, l) in ln.loops)
            {
                if (l.outer == null)
                {
                    l.setDepth(1L);
                }

            }
            ln.initializedDepth = true;

        }

        // findExits uses loop depth information to find the
        // exits from a loop.
        private static void findExits(this ptr<loopnest> _addr_ln)
        {
            ref loopnest ln = ref _addr_ln.val;

            if (ln.initializedExits)
            {
                return ;
            }

            ln.calculateDepths();
            var b2l = ln.b2l;
            foreach (var (_, b) in ln.po)
            {
                var l = b2l[b.ID];
                if (l != null && len(b.Succs) == 2L)
                {
                    var sl = b2l[b.Succs[0L].b.ID];
                    if (recordIfExit(_addr_l, _addr_sl, _addr_b.Succs[0L].b))
                    {
                        continue;
                    }

                    sl = b2l[b.Succs[1L].b.ID];
                    if (recordIfExit(_addr_l, _addr_sl, _addr_b.Succs[1L].b))
                    {
                        continue;
                    }

                }

            }
            ln.initializedExits = true;

        }

        // depth returns the loop nesting level of block b.
        private static short depth(this ptr<loopnest> _addr_ln, ID b)
        {
            ref loopnest ln = ref _addr_ln.val;

            {
                var l = ln.b2l[b];

                if (l != null)
                {
                    return l.depth;
                }

            }

            return 0L;

        }

        // recordIfExit checks sl (the loop containing b) to see if it
        // is outside of loop l, and if so, records b as an exit block
        // from l and returns true.
        private static bool recordIfExit(ptr<loop> _addr_l, ptr<loop> _addr_sl, ptr<Block> _addr_b)
        {
            ref loop l = ref _addr_l.val;
            ref loop sl = ref _addr_sl.val;
            ref Block b = ref _addr_b.val;

            if (sl != l)
            {
                if (sl == null || sl.depth <= l.depth)
                {
                    l.exits = append(l.exits, b);
                    return true;
                } 
                // sl is not nil, and is deeper than l
                // it's possible for this to be a goto into an irreducible loop made from gotos.
                while (sl.depth > l.depth)
                {
                    sl = sl.outer;
                }

                if (sl != l)
                {
                    l.exits = append(l.exits, b);
                    return true;
                }

            }

            return false;

        }

        private static void setDepth(this ptr<loop> _addr_l, short d)
        {
            ref loop l = ref _addr_l.val;

            l.depth = d;
            foreach (var (_, c) in l.children)
            {
                c.setDepth(d + 1L);
            }

        }

        // iterationEnd checks if block b ends iteration of loop l.
        // Ending iteration means either escaping to outer loop/code or
        // going back to header
        private static bool iterationEnd(this ptr<loop> _addr_l, ptr<Block> _addr_b, slice<ptr<loop>> b2l)
        {
            ref loop l = ref _addr_l.val;
            ref Block b = ref _addr_b.val;

            return b == l.header || b2l[b.ID] == null || (b2l[b.ID] != l && b2l[b.ID].depth <= l.depth);
        }
    }
}}}}
