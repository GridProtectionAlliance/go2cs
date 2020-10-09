// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 09 05:24:30 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\deadcode.go
using src = go.cmd.@internal.src_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        // findlive returns the reachable blocks and live values in f.
        // The caller should call f.retDeadcodeLive(live) when it is done with it.
        private static (slice<bool>, slice<bool>) findlive(ptr<Func> _addr_f)
        {
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
        public static slice<bool> ReachableBlocks(ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            var reachable = make_slice<bool>(f.NumBlocks());
            reachable[f.Entry.ID] = true;
            var p = make_slice<ptr<Block>>(0L, 64L); // stack-like worklist
            p = append(p, f.Entry);
            while (len(p) > 0L)
            { 
                // Pop a reachable block
                var b = p[len(p) - 1L];
                p = p[..len(p) - 1L]; 
                // Mark successors as reachable
                var s = b.Succs;
                if (b.Kind == BlockFirst)
                {
                    s = s[..1L];
                }

                foreach (var (_, e) in s)
                {
                    var c = e.b;
                    if (int(c.ID) >= len(reachable))
                    {
                        f.Fatalf("block %s >= f.NumBlocks()=%d?", c, len(reachable));
                    }

                    if (!reachable[c.ID])
                    {
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
        private static (slice<bool>, slice<ptr<Value>>) liveValues(ptr<Func> _addr_f, slice<bool> reachable) => func((defer, _, __) =>
        {
            slice<bool> live = default;
            slice<ptr<Value>> liveOrderStmts = default;
            ref Func f = ref _addr_f.val;

            live = f.newDeadcodeLive();
            if (cap(live) < f.NumValues())
            {
                live = make_slice<bool>(f.NumValues());
            }
            else
            {
                live = live[..f.NumValues()];
                {
                    var i__prev1 = i;

                    foreach (var (__i) in live)
                    {
                        i = __i;
                        live[i] = false;
                    }

                    i = i__prev1;
                }
            }

            liveOrderStmts = f.newDeadcodeLiveOrderStmts();
            liveOrderStmts = liveOrderStmts[..0L]; 

            // After regalloc, consider all values to be live.
            // See the comment at the top of regalloc.go and in deadcode for details.
            if (f.RegAlloc != null)
            {
                {
                    var i__prev1 = i;

                    foreach (var (__i) in live)
                    {
                        i = __i;
                        live[i] = true;
                    }

                    i = i__prev1;
                }

                return ;

            } 

            // Record all the inline indexes we need
            map<long, bool> liveInlIdx = default;
            var pt = f.Config.ctxt.PosTable;
            {
                var b__prev1 = b;

                foreach (var (_, __b) in f.Blocks)
                {
                    b = __b;
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in b.Values)
                        {
                            v = __v;
                            var i = pt.Pos(v.Pos).Base().InliningIndex();
                            if (i < 0L)
                            {
                                continue;
                            }

                            if (liveInlIdx == null)
                            {
                                liveInlIdx = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<long, bool>{};
                            }

                            liveInlIdx[i] = true;

                        }

                        v = v__prev2;
                    }

                    i = pt.Pos(b.Pos).Base().InliningIndex();
                    if (i < 0L)
                    {
                        continue;
                    }

                    if (liveInlIdx == null)
                    {
                        liveInlIdx = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<long, bool>{};
                    }

                    liveInlIdx[i] = true;

                } 

                // Find all live values

                b = b__prev1;
            }

            var q = f.Cache.deadcode.q[..0L];
            defer(() =>
            {
                f.Cache.deadcode.q = q;
            }()); 

            // Starting set: all control values of reachable blocks are live.
            // Calls are live (because callee can observe the memory state).
            {
                var b__prev1 = b;

                foreach (var (_, __b) in f.Blocks)
                {
                    b = __b;
                    if (!reachable[b.ID])
                    {
                        continue;
                    }

                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in b.ControlValues())
                        {
                            v = __v;
                            if (!live[v.ID])
                            {
                                live[v.ID] = true;
                                q = append(q, v);
                                if (v.Pos.IsStmt() != src.PosNotStmt)
                                {
                                    liveOrderStmts = append(liveOrderStmts, v);
                                }

                            }

                        }

                        v = v__prev2;
                    }

                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in b.Values)
                        {
                            v = __v;
                            if ((opcodeTable[v.Op].call || opcodeTable[v.Op].hasSideEffects) && !live[v.ID])
                            {
                                live[v.ID] = true;
                                q = append(q, v);
                                if (v.Pos.IsStmt() != src.PosNotStmt)
                                {
                                    liveOrderStmts = append(liveOrderStmts, v);
                                }

                            }

                            if (v.Type.IsVoid() && !live[v.ID])
                            { 
                                // The only Void ops are nil checks and inline marks.  We must keep these.
                                if (v.Op == OpInlMark && !liveInlIdx[int(v.AuxInt)])
                                { 
                                    // We don't need marks for bodies that
                                    // have been completely optimized away.
                                    // TODO: save marks only for bodies which
                                    // have a faulting instruction or a call?
                                    continue;

                                }

                                live[v.ID] = true;
                                q = append(q, v);
                                if (v.Pos.IsStmt() != src.PosNotStmt)
                                {
                                    liveOrderStmts = append(liveOrderStmts, v);
                                }

                            }

                        }

                        v = v__prev2;
                    }
                } 

                // Compute transitive closure of live values.

                b = b__prev1;
            }

            while (len(q) > 0L)
            { 
                // pop a reachable value
                var v = q[len(q) - 1L];
                q = q[..len(q) - 1L];
                {
                    var i__prev2 = i;

                    foreach (var (__i, __x) in v.Args)
                    {
                        i = __i;
                        x = __x;
                        if (v.Op == OpPhi && !reachable[v.Block.Preds[i].b.ID])
                        {
                            continue;
                        }

                        if (!live[x.ID])
                        {
                            live[x.ID] = true;
                            q = append(q, x); // push
                            if (x.Pos.IsStmt() != src.PosNotStmt)
                            {
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
        private static void deadcode(ptr<Func> _addr_f) => func((defer, _, __) =>
        {
            ref Func f = ref _addr_f.val;
 
            // deadcode after regalloc is forbidden for now. Regalloc
            // doesn't quite generate legal SSA which will lead to some
            // required moves being eliminated. See the comment at the
            // top of regalloc.go for details.
            if (f.RegAlloc != null)
            {
                f.Fatalf("deadcode after regalloc");
            } 

            // Find reachable blocks.
            var reachable = ReachableBlocks(_addr_f); 

            // Get rid of edges from dead to live code.
            {
                var b__prev1 = b;

                foreach (var (_, __b) in f.Blocks)
                {
                    b = __b;
                    if (reachable[b.ID])
                    {
                        continue;
                    }

                    {
                        long i__prev2 = i;

                        long i = 0L;

                        while (i < len(b.Succs))
                        {
                            var e = b.Succs[i];
                            if (reachable[e.b.ID])
                            {
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

                // Get rid of dead edges from live code.

                b = b__prev1;
            }

            {
                var b__prev1 = b;

                foreach (var (_, __b) in f.Blocks)
                {
                    b = __b;
                    if (!reachable[b.ID])
                    {
                        continue;
                    }

                    if (b.Kind != BlockFirst)
                    {
                        continue;
                    }

                    b.removeEdge(1L);
                    b.Kind = BlockPlain;
                    b.Likely = BranchUnknown;

                } 

                // Splice out any copies introduced during dead block removal.

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
            i = 0L;
            foreach (var (_, name) in f.Names)
            {
                long j = 0L;
                s.clear();
                var values = f.NamedValues[name];
                {
                    var v__prev2 = v;

                    foreach (var (_, __v) in values)
                    {
                        v = __v;
                        if (live[v.ID] && !s.contains(v.ID))
                        {
                            values[j] = v;
                            j++;
                            s.add(v.ID);
                        }

                    }

                    v = v__prev2;
                }

                if (j == 0L)
                {
                    delete(f.NamedValues, name);
                }
                else
                {
                    f.Names[i] = name;
                    i++;
                    for (var k = len(values) - 1L; k >= j; k--)
                    {
                        values[k] = null;
                    }

                    f.NamedValues[name] = values[..j];

                }

            }
            var clearNames = f.Names[i..];
            {
                long j__prev1 = j;

                foreach (var (__j) in clearNames)
                {
                    j = __j;
                    clearNames[j] = new LocalSlot();
                }

                j = j__prev1;
            }

            f.Names = f.Names[..i];

            var pendingLines = f.cachedLineStarts; // Holds statement boundaries that need to be moved to a new value/block
            pendingLines.clear(); 

            // Unlink values and conserve statement boundaries
            {
                long i__prev1 = i;
                var b__prev1 = b;

                foreach (var (__i, __b) in f.Blocks)
                {
                    i = __i;
                    b = __b;
                    if (!reachable[b.ID])
                    { 
                        // TODO what if control is statement boundary? Too late here.
                        b.ResetControls();

                    }

                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in b.Values)
                        {
                            v = __v;
                            if (!live[v.ID])
                            {
                                v.resetArgs();
                                if (v.Pos.IsStmt() == src.PosIsStmt && reachable[b.ID])
                                {
                                    pendingLines.set(v.Pos, int32(i)); // TODO could be more than one pos for a line
                                }

                            }

                        }

                        v = v__prev2;
                    }
                } 

                // Find new homes for lost lines -- require earliest in data flow with same line that is also in same block

                i = i__prev1;
                b = b__prev1;
            }

            {
                long i__prev1 = i;

                for (i = len(order) - 1L; i >= 0L; i--)
                {
                    var w = order[i];
                    {
                        long j__prev1 = j;

                        j = pendingLines.get(w.Pos);

                        if (j > -1L && f.Blocks[j] == w.Block)
                        {
                            w.Pos = w.Pos.WithIsStmt();
                            pendingLines.remove(w.Pos);
                        }

                        j = j__prev1;

                    }

                } 

                // Any boundary that failed to match a live value can move to a block end


                i = i__prev1;
            } 

            // Any boundary that failed to match a live value can move to a block end
            pendingLines.foreachEntry((j, l, bi) =>
            {
                var b = f.Blocks[bi];
                if (b.Pos.Line() == l && b.Pos.FileIndex() == j)
                {
                    b.Pos = b.Pos.WithIsStmt();
                }

            }); 

            // Remove dead values from blocks' value list. Return dead
            // values to the allocator.
            {
                var b__prev1 = b;

                foreach (var (_, __b) in f.Blocks)
                {
                    b = __b;
                    i = 0L;
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in b.Values)
                        {
                            v = __v;
                            if (live[v.ID])
                            {
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

                // Remove dead blocks from WBLoads list.

                b = b__prev1;
            }

            i = 0L;
            {
                var b__prev1 = b;

                foreach (var (_, __b) in f.WBLoads)
                {
                    b = __b;
                    if (reachable[b.ID])
                    {
                        f.WBLoads[i] = b;
                        i++;
                    }

                }

                b = b__prev1;
            }

            var clearWBLoads = f.WBLoads[i..];
            {
                long j__prev1 = j;

                foreach (var (__j) in clearWBLoads)
                {
                    j = __j;
                    clearWBLoads[j] = null;
                }

                j = j__prev1;
            }

            f.WBLoads = f.WBLoads[..i]; 

            // Remove unreachable blocks. Return dead blocks to allocator.
            i = 0L;
            {
                var b__prev1 = b;

                foreach (var (_, __b) in f.Blocks)
                {
                    b = __b;
                    if (reachable[b.ID])
                    {
                        f.Blocks[i] = b;
                        i++;
                    }
                    else
                    {
                        if (len(b.Values) > 0L)
                        {
                            b.Fatalf("live values in unreachable block %v: %v", b, b.Values);
                        }

                        f.freeBlock(b);

                    }

                } 
                // zero remainder to help GC

                b = b__prev1;
            }

            var tail = f.Blocks[i..];
            {
                long j__prev1 = j;

                foreach (var (__j) in tail)
                {
                    j = __j;
                    tail[j] = null;
                }

                j = j__prev1;
            }

            f.Blocks = f.Blocks[..i];

        });

        // removeEdge removes the i'th outgoing edge from b (and
        // the corresponding incoming edge from b.Succs[i].b).
        private static void removeEdge(this ptr<Block> _addr_b, long i)
        {
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
            foreach (var (_, v) in c.Values)
            {
                if (v.Op != OpPhi)
                {
                    continue;
                }

                v.Args[j].Uses--;
                v.Args[j] = v.Args[n];
                v.Args[n] = null;
                v.Args = v.Args[..n];
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
    }
}}}}
