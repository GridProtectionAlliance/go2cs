// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 August 29 08:53:57 UTC
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
        private static void layout(ref Func _f) => func(_f, (ref Func f, Defer defer, Panic _, Recover __) =>
        {
            var order = make_slice<ref Block>(0L, f.NumBlocks());
            var scheduled = make_slice<bool>(f.NumBlocks());
            var idToBlock = make_slice<ref Block>(f.NumBlocks());
            var indegree = make_slice<long>(f.NumBlocks());
            var posdegree = f.newSparseSet(f.NumBlocks()); // blocks with positive remaining degree
            defer(f.retSparseSet(posdegree));
            var zerodegree = f.newSparseSet(f.NumBlocks()); // blocks with zero remaining degree
            defer(f.retSparseSet(zerodegree));
            var exit = f.newSparseSet(f.NumBlocks()); // exit blocks
            defer(f.retSparseSet(exit)); 

            // Initialize indegree of each block
            {
                var b__prev1 = b;

                foreach (var (_, __b) in f.Blocks)
                {
                    b = __b;
                    idToBlock[b.ID] = b;
                    if (b.Kind == BlockExit)
                    { 
                        // exit blocks are always scheduled last
                        // TODO: also add blocks post-dominated by exit blocks
                        exit.add(b.ID);
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
                var b = idToBlock[bid];
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
                    e = e__prev2;
                }

                ref Block likely = default;

                if (b.Likely == BranchLikely) 
                    likely = b.Succs[0L].b;
                else if (b.Likely == BranchUnlikely) 
                    likely = b.Succs[1L].b;
                                if (likely != null && !scheduled[likely.ID])
                {
                    bid = likely.ID;
                    continue;
                }
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
            f.Blocks = order;
        });
    }
}}}}
