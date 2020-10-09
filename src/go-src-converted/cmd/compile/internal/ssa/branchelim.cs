// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 09 05:24:20 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\branchelim.go
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
        // branchelim tries to eliminate branches by
        // generating CondSelect instructions.
        //
        // Search for basic blocks that look like
        //
        // bb0            bb0
        //  | \          /   \
        //  | bb1  or  bb1   bb2    <- trivial if/else blocks
        //  | /          \   /
        // bb2            bb3
        //
        // where the intermediate blocks are mostly empty (with no side-effects);
        // rewrite Phis in the postdominator as CondSelects.
        private static void branchelim(ptr<Func> _addr_f) => func((defer, _, __) =>
        {
            ref Func f = ref _addr_f.val;
 
            // FIXME: add support for lowering CondSelects on more architectures
            switch (f.Config.arch)
            {
                case "arm64": 

                case "amd64": 

                case "wasm": 
                    break;
                default: 
                    return ;
                    break;
            } 

            // Find all the values used in computing the address of any load.
            // Typically these values have operations like AddPtr, Lsh64x64, etc.
            var loadAddr = f.newSparseSet(f.NumValues());
            defer(f.retSparseSet(loadAddr));
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

                            if (v.Op == OpLoad || v.Op == OpAtomicLoad8 || v.Op == OpAtomicLoad32 || v.Op == OpAtomicLoad64 || v.Op == OpAtomicLoadPtr || v.Op == OpAtomicLoadAcq32) 
                                loadAddr.add(v.Args[0L].ID);
                            else if (v.Op == OpMove) 
                                loadAddr.add(v.Args[1L].ID);
                            
                        }
                        v = v__prev2;
                    }
                }
                b = b__prev1;
            }

            var po = f.postorder();
            while (true)
            {
                var n = loadAddr.size();
                {
                    var b__prev2 = b;

                    foreach (var (_, __b) in po)
                    {
                        b = __b;
                        for (var i = len(b.Values) - 1L; i >= 0L; i--)
                        {
                            var v = b.Values[i];
                            if (!loadAddr.contains(v.ID))
                            {
                                continue;
                            }
                            foreach (var (_, a) in v.Args)
                            {
                                if (a.Type.IsInteger() || a.Type.IsPtr() || a.Type.IsUnsafePtr())
                                {
                                    loadAddr.add(a.ID);
                                }
                            }
                        }

                    }
                    b = b__prev2;
                }

                if (loadAddr.size() == n)
                {
                    break;
                }
            }

            var change = true;
            while (change)
            {
                change = false;
                {
                    var b__prev2 = b;

                    foreach (var (_, __b) in f.Blocks)
                    {
                        b = __b;
                        change = elimIf(_addr_f, _addr_loadAddr, _addr_b) || elimIfElse(_addr_f, _addr_loadAddr, _addr_b) || change;
                    }
                    b = b__prev2;
                }
            }

        });

        private static bool canCondSelect(ptr<Value> _addr_v, @string arch, ptr<sparseSet> _addr_loadAddr)
        {
            ref Value v = ref _addr_v.val;
            ref sparseSet loadAddr = ref _addr_loadAddr.val;

            if (loadAddr.contains(v.ID))
            { 
                // The result of the soon-to-be conditional move is used to compute a load address.
                // We want to avoid generating a conditional move in this case
                // because the load address would now be data-dependent on the condition.
                // Previously it would only be control-dependent on the condition, which is faster
                // if the branch predicts well (or possibly even if it doesn't, if the load will
                // be an expensive cache miss).
                // See issue #26306.
                return false;

            } 
            // For now, stick to simple scalars that fit in registers

            if (v.Type.Size() > v.Block.Func.Config.RegSize) 
                return false;
            else if (v.Type.IsPtrShaped()) 
                return true;
            else if (v.Type.IsInteger()) 
                if (arch == "amd64" && v.Type.Size() < 2L)
                { 
                    // amd64 doesn't support CMOV with byte registers
                    return false;

                }

                return true;
            else 
                return false;
            
        }

        // elimIf converts the one-way branch starting at dom in f to a conditional move if possible.
        // loadAddr is a set of values which are used to compute the address of a load.
        // Those values are exempt from CMOV generation.
        private static bool elimIf(ptr<Func> _addr_f, ptr<sparseSet> _addr_loadAddr, ptr<Block> _addr_dom)
        {
            ref Func f = ref _addr_f.val;
            ref sparseSet loadAddr = ref _addr_loadAddr.val;
            ref Block dom = ref _addr_dom.val;
 
            // See if dom is an If with one arm that
            // is trivial and succeeded by the other
            // successor of dom.
            if (dom.Kind != BlockIf || dom.Likely != BranchUnknown)
            {
                return false;
            }

            ptr<Block> simple;            ptr<Block> post;

            {
                var i__prev1 = i;

                foreach (var (__i) in dom.Succs)
                {
                    i = __i;
                    var bb = dom.Succs[i].Block();
                    var other = dom.Succs[i ^ 1L].Block();
                    if (isLeafPlain(_addr_bb) && bb.Succs[0L].Block() == other)
                    {
                        simple = bb;
                        post = other;
                        break;
                    }

                }

                i = i__prev1;
            }

            if (simple == null || len(post.Preds) != 2L || post == dom)
            {
                return false;
            } 

            // We've found our diamond CFG of blocks.
            // Now decide if fusing 'simple' into dom+post
            // looks profitable.

            // Check that there are Phis, and that all of them
            // can be safely rewritten to CondSelect.
            var hasphis = false;
            {
                var v__prev1 = v;

                foreach (var (_, __v) in post.Values)
                {
                    v = __v;
                    if (v.Op == OpPhi)
                    {
                        hasphis = true;
                        if (!canCondSelect(_addr_v, f.Config.arch, _addr_loadAddr))
                        {
                            return false;
                        }

                    }

                }

                v = v__prev1;
            }

            if (!hasphis)
            {
                return false;
            } 

            // Pick some upper bound for the number of instructions
            // we'd be willing to execute just to generate a dead
            // argument to CondSelect. In the worst case, this is
            // the number of useless instructions executed.
            const long maxfuseinsts = (long)2L;



            if (len(simple.Values) > maxfuseinsts || !canSpeculativelyExecute(simple))
            {
                return false;
            } 

            // Replace Phi instructions in b with CondSelect instructions
            var swap = (post.Preds[0L].Block() == dom) != (dom.Succs[0L].Block() == post);
            {
                var v__prev1 = v;

                foreach (var (_, __v) in post.Values)
                {
                    v = __v;
                    if (v.Op != OpPhi)
                    {
                        continue;
                    }

                    v.Op = OpCondSelect;
                    if (swap)
                    {
                        v.Args[0L] = v.Args[1L];
                        v.Args[1L] = v.Args[0L];

                    }

                    v.AddArg(dom.Controls[0L]);

                } 

                // Put all of the instructions into 'dom'
                // and update the CFG appropriately.

                v = v__prev1;
            }

            dom.Kind = post.Kind;
            dom.CopyControls(post);
            dom.Aux = post.Aux;
            dom.Succs = append(dom.Succs[..0L], post.Succs);
            {
                var i__prev1 = i;

                foreach (var (__i) in dom.Succs)
                {
                    i = __i;
                    var e = dom.Succs[i];
                    e.b.Preds[e.i].b = dom;
                } 

                // Try really hard to preserve statement marks attached to blocks.

                i = i__prev1;
            }

            var simplePos = simple.Pos;
            var postPos = post.Pos;
            var simpleStmt = simplePos.IsStmt() == src.PosIsStmt;
            var postStmt = postPos.IsStmt() == src.PosIsStmt;

            {
                var v__prev1 = v;

                foreach (var (_, __v) in simple.Values)
                {
                    v = __v;
                    v.Block = dom;
                }

                v = v__prev1;
            }

            {
                var v__prev1 = v;

                foreach (var (_, __v) in post.Values)
                {
                    v = __v;
                    v.Block = dom;
                } 

                // findBlockPos determines if b contains a stmt-marked value
                // that has the same line number as the Pos for b itself.
                // (i.e. is the position on b actually redundant?)

                v = v__prev1;
            }

            Func<ptr<Block>, bool> findBlockPos = b =>
            {
                var pos = b.Pos;
                {
                    var v__prev1 = v;

                    foreach (var (_, __v) in b.Values)
                    {
                        v = __v; 
                        // See if there is a stmt-marked value already that matches simple.Pos (and perhaps post.Pos)
                        if (pos.SameFileAndLine(v.Pos) && v.Pos.IsStmt() == src.PosIsStmt)
                        {
                            return true;
                        }

                    }

                    v = v__prev1;
                }

                return false;

            }
;
            if (simpleStmt)
            {
                simpleStmt = !findBlockPos(simple);
                if (!simpleStmt && simplePos.SameFileAndLine(postPos))
                {
                    postStmt = false;
                }

            }

            if (postStmt)
            {
                postStmt = !findBlockPos(post);
            } 

            // If simpleStmt and/or postStmt are still true, then try harder
            // to find the corresponding statement marks new homes.

            // setBlockPos determines if b contains a can-be-statement value
            // that has the same line number as the Pos for b itself, and
            // puts a statement mark on it, and returns whether it succeeded
            // in this operation.
            Func<ptr<Block>, bool> setBlockPos = b =>
            {
                pos = b.Pos;
                {
                    var v__prev1 = v;

                    foreach (var (_, __v) in b.Values)
                    {
                        v = __v;
                        if (pos.SameFileAndLine(v.Pos) && !isPoorStatementOp(v.Op))
                        {
                            v.Pos = v.Pos.WithIsStmt();
                            return true;
                        }

                    }

                    v = v__prev1;
                }

                return false;

            } 
            // If necessary and possible, add a mark to a value in simple
; 
            // If necessary and possible, add a mark to a value in simple
            if (simpleStmt)
            {
                if (setBlockPos(simple) && simplePos.SameFileAndLine(postPos))
                {
                    postStmt = false;
                }

            } 
            // If necessary and possible, add a mark to a value in post
            if (postStmt)
            {
                postStmt = !setBlockPos(post);
            } 

            // Before giving up (this was added because it helps), try the end of "dom", and if that is not available,
            // try the values in the successor block if it is uncomplicated.
            if (postStmt)
            {
                if (dom.Pos.IsStmt() != src.PosIsStmt)
                {
                    dom.Pos = postPos;
                }
                else
                { 
                    // Try the successor block
                    if (len(dom.Succs) == 1L && len(dom.Succs[0L].Block().Preds) == 1L)
                    {
                        var succ = dom.Succs[0L].Block();
                        {
                            var v__prev1 = v;

                            foreach (var (_, __v) in succ.Values)
                            {
                                v = __v;
                                if (isPoorStatementOp(v.Op))
                                {
                                    continue;
                                }

                                if (postPos.SameFileAndLine(v.Pos))
                                {
                                    v.Pos = v.Pos.WithIsStmt();
                                }

                                postStmt = false;
                                break;

                            } 
                            // If postStmt still true, tag the block itself if possible

                            v = v__prev1;
                        }

                        if (postStmt && succ.Pos.IsStmt() != src.PosIsStmt)
                        {
                            succ.Pos = postPos;
                        }

                    }

                }

            }

            dom.Values = append(dom.Values, simple.Values);
            dom.Values = append(dom.Values, post.Values); 

            // Trash 'post' and 'simple'
            clobberBlock(post);
            clobberBlock(simple);

            f.invalidateCFG();
            return true;

        }

        // is this a BlockPlain with one predecessor?
        private static bool isLeafPlain(ptr<Block> _addr_b)
        {
            ref Block b = ref _addr_b.val;

            return b.Kind == BlockPlain && len(b.Preds) == 1L;
        }

        private static void clobberBlock(ptr<Block> _addr_b)
        {
            ref Block b = ref _addr_b.val;

            b.Values = null;
            b.Preds = null;
            b.Succs = null;
            b.Aux = null;
            b.ResetControls();
            b.Likely = BranchUnknown;
            b.Kind = BlockInvalid;
        }

        // elimIfElse converts the two-way branch starting at dom in f to a conditional move if possible.
        // loadAddr is a set of values which are used to compute the address of a load.
        // Those values are exempt from CMOV generation.
        private static bool elimIfElse(ptr<Func> _addr_f, ptr<sparseSet> _addr_loadAddr, ptr<Block> _addr_b)
        {
            ref Func f = ref _addr_f.val;
            ref sparseSet loadAddr = ref _addr_loadAddr.val;
            ref Block b = ref _addr_b.val;
 
            // See if 'b' ends in an if/else: it should
            // have two successors, both of which are BlockPlain
            // and succeeded by the same block.
            if (b.Kind != BlockIf || b.Likely != BranchUnknown)
            {
                return false;
            }

            var yes = b.Succs[0L].Block();
            var no = b.Succs[1L].Block();
            if (!isLeafPlain(_addr_yes) || len(yes.Values) > 1L || !canSpeculativelyExecute(_addr_yes))
            {
                return false;
            }

            if (!isLeafPlain(_addr_no) || len(no.Values) > 1L || !canSpeculativelyExecute(_addr_no))
            {
                return false;
            }

            if (b.Succs[0L].Block().Succs[0L].Block() != b.Succs[1L].Block().Succs[0L].Block())
            {
                return false;
            } 
            // block that postdominates the if/else
            var post = b.Succs[0L].Block().Succs[0L].Block();
            if (len(post.Preds) != 2L || post == b)
            {
                return false;
            }

            var hasphis = false;
            {
                var v__prev1 = v;

                foreach (var (_, __v) in post.Values)
                {
                    v = __v;
                    if (v.Op == OpPhi)
                    {
                        hasphis = true;
                        if (!canCondSelect(_addr_v, f.Config.arch, _addr_loadAddr))
                        {
                            return false;
                        }

                    }

                }

                v = v__prev1;
            }

            if (!hasphis)
            {
                return false;
            } 

            // Don't generate CondSelects if branch is cheaper.
            if (!shouldElimIfElse(_addr_no, _addr_yes, _addr_post, f.Config.arch))
            {
                return false;
            } 

            // now we're committed: rewrite each Phi as a CondSelect
            var swap = post.Preds[0L].Block() != b.Succs[0L].Block();
            {
                var v__prev1 = v;

                foreach (var (_, __v) in post.Values)
                {
                    v = __v;
                    if (v.Op != OpPhi)
                    {
                        continue;
                    }

                    v.Op = OpCondSelect;
                    if (swap)
                    {
                        v.Args[0L] = v.Args[1L];
                        v.Args[1L] = v.Args[0L];

                    }

                    v.AddArg(b.Controls[0L]);

                } 

                // Move the contents of all of these
                // blocks into 'b' and update CFG edges accordingly

                v = v__prev1;
            }

            b.Kind = post.Kind;
            b.CopyControls(post);
            b.Aux = post.Aux;
            b.Succs = append(b.Succs[..0L], post.Succs);
            {
                var i__prev1 = i;

                foreach (var (__i) in b.Succs)
                {
                    i = __i;
                    var e = b.Succs[i];
                    e.b.Preds[e.i].b = b;
                }

                i = i__prev1;
            }

            {
                var i__prev1 = i;

                foreach (var (__i) in post.Values)
                {
                    i = __i;
                    post.Values[i].Block = b;
                }

                i = i__prev1;
            }

            {
                var i__prev1 = i;

                foreach (var (__i) in yes.Values)
                {
                    i = __i;
                    yes.Values[i].Block = b;
                }

                i = i__prev1;
            }

            {
                var i__prev1 = i;

                foreach (var (__i) in no.Values)
                {
                    i = __i;
                    no.Values[i].Block = b;
                }

                i = i__prev1;
            }

            b.Values = append(b.Values, yes.Values);
            b.Values = append(b.Values, no.Values);
            b.Values = append(b.Values, post.Values); 

            // trash post, yes, and no
            clobberBlock(_addr_yes);
            clobberBlock(_addr_no);
            clobberBlock(_addr_post);

            f.invalidateCFG();
            return true;

        }

        // shouldElimIfElse reports whether estimated cost of eliminating branch
        // is lower than threshold.
        private static bool shouldElimIfElse(ptr<Block> _addr_no, ptr<Block> _addr_yes, ptr<Block> _addr_post, @string arch)
        {
            ref Block no = ref _addr_no.val;
            ref Block yes = ref _addr_yes.val;
            ref Block post = ref _addr_post.val;

            switch (arch)
            {
                case "amd64": 
                    const long maxcost = (long)2L;

                    long phi = 0L;
                    long other = 0L;
                    foreach (var (_, v) in post.Values)
                    {
                        if (v.Op == OpPhi)
                        { 
                            // Each phi results in CondSelect, which lowers into CMOV,
                            // CMOV has latency >1 on most CPUs.
                            phi++;

                        }

                        foreach (var (_, x) in v.Args)
                        {
                            if (x.Block == no || x.Block == yes)
                            {
                                other++;
                            }

                        }

                    }
                    var cost = phi * 1L;
                    if (phi > 1L)
                    { 
                        // If we have more than 1 phi and some values in post have args
                        // in yes or no blocks, we may have to recalculate condition, because
                        // those args may clobber flags. For now assume that all operations clobber flags.
                        cost += other * 1L;

                    }

                    return cost < maxcost;
                    break;
                default: 
                    return true;
                    break;
            }

        }

        // canSpeculativelyExecute reports whether every value in the block can
        // be evaluated without causing any observable side effects (memory
        // accesses, panics and so on) except for execution time changes. It
        // also ensures that the block does not contain any phis which we can't
        // speculatively execute.
        // Warning: this function cannot currently detect values that represent
        // instructions the execution of which need to be guarded with CPU
        // hardware feature checks. See issue #34950.
        private static bool canSpeculativelyExecute(ptr<Block> _addr_b)
        {
            ref Block b = ref _addr_b.val;
 
            // don't fuse memory ops, Phi ops, divides (can panic),
            // or anything else with side-effects
            foreach (var (_, v) in b.Values)
            {
                if (v.Op == OpPhi || isDivMod(v.Op) || v.Type.IsMemory() || v.MemoryArg() != null || opcodeTable[v.Op].hasSideEffects)
                {
                    return false;
                }

            }
            return true;

        }

        private static bool isDivMod(Op op)
        {

            if (op == OpDiv8 || op == OpDiv8u || op == OpDiv16 || op == OpDiv16u || op == OpDiv32 || op == OpDiv32u || op == OpDiv64 || op == OpDiv64u || op == OpDiv128u || op == OpDiv32F || op == OpDiv64F || op == OpMod8 || op == OpMod8u || op == OpMod16 || op == OpMod16u || op == OpMod32 || op == OpMod32u || op == OpMod64 || op == OpMod64u) 
                return true;
            else 
                return false;
            
        }
    }
}}}}
