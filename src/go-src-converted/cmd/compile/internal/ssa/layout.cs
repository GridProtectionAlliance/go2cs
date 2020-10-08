// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 08 04:10:35 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\layout.go

using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        // layout orders basic blocks in f with the goal of minimizing control flow instructions.
        // After this phase returns, the order of f.Blocks matters and is the order
        // in which those blocks will appear in the assembly output.
        private static void layout(ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            f.Blocks = layoutOrder(_addr_f);
        }

        // Register allocation may use a different order which has constraints
        // imposed by the linear-scan algorithm. Note that f.pass here is
        // regalloc, so the switch is conditional on -d=ssa/regalloc/test=N
        private static slice<ptr<Block>> layoutRegallocOrder(ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            switch (f.pass.test)
            {
                case 0L: // layout order
                    return layoutOrder(_addr_f);
                    break;
                case 1L: // existing block order
                    return f.Blocks;
                    break;
                case 2L: // reverse of postorder; legal, but usually not good.
                    var po = f.postorder();
                    var visitOrder = make_slice<ptr<Block>>(len(po));
                    foreach (var (i, b) in po)
                    {
                        var j = len(po) - i - 1L;
                        visitOrder[j] = b;
                    }
                    return visitOrder;
                    break;
            }

            return null;

        }

        private static slice<ptr<Block>> layoutOrder(ptr<Func> _addr_f) => func((defer, _, __) =>
        {
            ref Func f = ref _addr_f.val;

            var order = make_slice<ptr<Block>>(0L, f.NumBlocks());
            var scheduled = make_slice<bool>(f.NumBlocks());
            var idToBlock = make_slice<ptr<Block>>(f.NumBlocks());
            var indegree = make_slice<long>(f.NumBlocks());
            var posdegree = f.newSparseSet(f.NumBlocks()); // blocks with positive remaining degree
            defer(f.retSparseSet(posdegree));
            var zerodegree = f.newSparseSet(f.NumBlocks()); // blocks with zero remaining degree
            defer(f.retSparseSet(zerodegree));
            var exit = f.newSparseSet(f.NumBlocks()); // exit blocks
            defer(f.retSparseSet(exit)); 

            // Populate idToBlock and find exit blocks.
            {
                var b__prev1 = b;

                foreach (var (_, __b) in f.Blocks)
                {
                    b = __b;
                    idToBlock[b.ID] = b;
                    if (b.Kind == BlockExit)
                    {
                        exit.add(b.ID);
                    }

                } 

                // Expand exit to include blocks post-dominated by exit blocks.

                b = b__prev1;
            }

            while (true)
            {
                var changed = false;
                foreach (var (_, id) in exit.contents())
                {
                    var b = idToBlock[id];
NextPred:
                    foreach (var (_, pe) in b.Preds)
                    {
                        var p = pe.b;
                        if (exit.contains(p.ID))
                        {
                            continue;
                        }

                        foreach (var (_, s) in p.Succs)
                        {
                            if (!exit.contains(s.b.ID))
                            {
                                _continueNextPred = true;
                                break;
                            }

                        } 
                        // All Succs are in exit; add p.
                        exit.add(p.ID);
                        changed = true;

                    }

                }
                if (!changed)
                {
                    break;
                }

            } 

            // Initialize indegree of each block
 

            // Initialize indegree of each block
            {
                var b__prev1 = b;

                foreach (var (_, __b) in f.Blocks)
                {
                    b = __b;
                    if (exit.contains(b.ID))
                    { 
                        // exit blocks are always scheduled last
                        continue;

                    }

                    indegree[b.ID] = len(b.Preds);
                    if (len(b.Preds) == 0L)
                    {
                        zerodegree.add(b.ID);
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
            while (true)
            { 
                // add block to schedule
                b = idToBlock[bid];
                order = append(order, b);
                scheduled[bid] = true;
                if (len(order) == len(f.Blocks))
                {
                    break;
                }

                {
                    var e__prev2 = e;

                    foreach (var (_, __e) in b.Succs)
                    {
                        e = __e;
                        var c = e.b;
                        indegree[c.ID]--;
                        if (indegree[c.ID] == 0L)
                        {
                            posdegree.remove(c.ID);
                            zerodegree.add(c.ID);
                        }

                    } 

                    // Pick the next block to schedule
                    // Pick among the successor blocks that have not been scheduled yet.

                    // Use likely direction if we have it.

                    e = e__prev2;
                }

                ptr<Block> likely;

                if (b.Likely == BranchLikely) 
                    likely = b.Succs[0L].b;
                else if (b.Likely == BranchUnlikely) 
                    likely = b.Succs[1L].b;
                                if (likely != null && !scheduled[likely.ID])
                {
                    bid = likely.ID;
                    continue;
                } 

                // Use degree for now.
                bid = 0L;
                var mindegree = f.NumBlocks();
                {
                    var e__prev2 = e;

                    foreach (var (_, __e) in order[len(order) - 1L].Succs)
                    {
                        e = __e;
                        c = e.b;
                        if (scheduled[c.ID] || c.Kind == BlockExit)
                        {
                            continue;
                        }

                        if (indegree[c.ID] < mindegree)
                        {
                            mindegree = indegree[c.ID];
                            bid = c.ID;
                        }

                    }

                    e = e__prev2;
                }

                if (bid != 0L)
                {
                    continue;
                } 
                // TODO: improve this part
                // No successor of the previously scheduled block works.
                // Pick a zero-degree block if we can.
                while (zerodegree.size() > 0L)
                {
                    var cid = zerodegree.pop();
                    if (!scheduled[cid])
                    {
                        bid = cid;
                        _continueblockloop = true;
                        break;
                    }

                } 
                // Still nothing, pick any non-exit block.
 
                // Still nothing, pick any non-exit block.
                while (posdegree.size() > 0L)
                {
                    cid = posdegree.pop();
                    if (!scheduled[cid])
                    {
                        bid = cid;
                        _continueblockloop = true;
                        break;
                    }

                } 
                // Pick any exit block.
                // TODO: Order these to minimize jump distances?
 
                // Pick any exit block.
                // TODO: Order these to minimize jump distances?
                while (true)
                {
                    cid = exit.pop();
                    if (!scheduled[cid])
                    {
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
    }
}}}}
