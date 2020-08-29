// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 August 29 08:53:37 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\deadcode.go

using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        // findlive returns the reachable blocks and live values in f.
        private static (slice<bool>, slice<bool>) findlive(ref Func f)
        {
            reachable = ReachableBlocks(f);
            live = liveValues(f, reachable);
            return;
        }

        // ReachableBlocks returns the reachable blocks in f.
        public static slice<bool> ReachableBlocks(ref Func f)
        {
            var reachable = make_slice<bool>(f.NumBlocks());
            reachable[f.Entry.ID] = true;
            ref Block p = new slice<ref Block>(new ref Block[] { f.Entry }); // stack-like worklist
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
                    if (!reachable[c.ID])
                    {
                        reachable[c.ID] = true;
                        p = append(p, c); // push
                    }
                }
            }

            return reachable;
        }

        // liveValues returns the live values in f.
        // reachable is a map from block ID to whether the block is reachable.
        private static slice<bool> liveValues(ref Func f, slice<bool> reachable)
        {
            var live = make_slice<bool>(f.NumValues()); 

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

                return live;
            } 

            // Find all live values
            slice<ref Value> q = default; // stack-like worklist of unscanned values

            // Starting set: all control values of reachable blocks are live.
            // Calls are live (because callee can observe the memory state).
            foreach (var (_, b) in f.Blocks)
            {
                if (!reachable[b.ID])
                {
                    continue;
                }
                {
                    var v__prev1 = v;

                    var v = b.Control;

                    if (v != null && !live[v.ID])
                    {
                        live[v.ID] = true;
                        q = append(q, v);
                    }

                    v = v__prev1;

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
                        }
                        if (v.Type.IsVoid() && !live[v.ID])
                        { 
                            // The only Void ops are nil checks.  We must keep these.
                            live[v.ID] = true;
                            q = append(q, v);
                        }
                    }

                    v = v__prev2;
                }

            } 

            // Compute transitive closure of live values.
            while (len(q) > 0L)
            { 
                // pop a reachable value
                v = q[len(q) - 1L];
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
                        }
                    }

                    i = i__prev2;
                }

            }


            return live;
        }

        // deadcode removes dead code from f.
        private static void deadcode(ref Func _f) => func(_f, (ref Func f, Defer defer, Panic _, Recover __) =>
        { 
            // deadcode after regalloc is forbidden for now. Regalloc
            // doesn't quite generate legal SSA which will lead to some
            // required moves being eliminated. See the comment at the
            // top of regalloc.go for details.
            if (f.RegAlloc != null)
            {
                f.Fatalf("deadcode after regalloc");
            } 

            // Find reachable blocks.
            var reachable = ReachableBlocks(f); 

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
            var live = liveValues(f, reachable); 

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
                    {
                        var k__prev2 = k;

                        for (var k = len(values) - 1L; k >= j; k--)
                        {
                            values[k] = null;
                        }


                        k = k__prev2;
                    }
                    f.NamedValues[name] = values[..j];
                }
            }
            {
                var k__prev1 = k;

                for (k = len(f.Names) - 1L; k >= i; k--)
                {
                    f.Names[k] = new LocalSlot();
                }


                k = k__prev1;
            }
            f.Names = f.Names[..i]; 

            // Unlink values.
            {
                var b__prev1 = b;

                foreach (var (_, __b) in f.Blocks)
                {
                    b = __b;
                    if (!reachable[b.ID])
                    {
                        b.SetControl(null);
                    }
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in b.Values)
                        {
                            v = __v;
                            if (!live[v.ID])
                            {
                                v.resetArgs();
                            }
                        }

                        v = v__prev2;
                    }

                } 

                // Remove dead values from blocks' value list. Return dead
                // values to the allocator.

                b = b__prev1;
            }

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
                        // aid GC

                        v = v__prev2;
                    }

                    var tail = b.Values[i..];
                    {
                        long j__prev2 = j;

                        foreach (var (__j) in tail)
                        {
                            j = __j;
                            tail[j] = null;
                        }

                        j = j__prev2;
                    }

                    b.Values = b.Values[..i];
                } 

                // Remove unreachable blocks. Return dead blocks to allocator.

                b = b__prev1;
            }

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

            tail = f.Blocks[i..];
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
        private static void removeEdge(this ref Block b, long i)
        {
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
