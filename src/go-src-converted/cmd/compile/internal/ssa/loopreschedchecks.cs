// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 08 04:10:42 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\loopreschedchecks.go
using types = go.cmd.compile.@internal.types_package;
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        // an edgeMem records a backedge, together with the memory
        // phi functions at the target of the backedge that must
        // be updated when a rescheduling check replaces the backedge.
        private partial struct edgeMem
        {
            public Edge e;
            public ptr<Value> m; // phi for memory at dest of e
        }

        // a rewriteTarget is a value-argindex pair indicating
        // where a rewrite is applied.  Note that this is for values,
        // not for block controls, because block controls are not targets
        // for the rewrites performed in inserting rescheduling checks.
        private partial struct rewriteTarget
        {
            public ptr<Value> v;
            public long i;
        }

        private partial struct rewrite
        {
            public ptr<Value> before; // before is the expected value before rewrite, after is the new value installed.
            public ptr<Value> after; // before is the expected value before rewrite, after is the new value installed.
            public slice<rewriteTarget> rewrites; // all the targets for this rewrite.
        }

        private static @string String(this ptr<rewrite> _addr_r)
        {
            ref rewrite r = ref _addr_r.val;

            @string s = "\n\tbefore=" + r.before.String() + ", after=" + r.after.String();
            foreach (var (_, rw) in r.rewrites)
            {
                s += ", (i=" + fmt.Sprint(rw.i) + ", v=" + rw.v.LongString() + ")";
            }
            s += "\n";
            return s;

        }

        // insertLoopReschedChecks inserts rescheduling checks on loop backedges.
        private static void insertLoopReschedChecks(ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;
 
            // TODO: when split information is recorded in export data, insert checks only on backedges that can be reached on a split-call-free path.

            // Loop reschedule checks compare the stack pointer with
            // the per-g stack bound.  If the pointer appears invalid,
            // that means a reschedule check is needed.
            //
            // Steps:
            // 1. locate backedges.
            // 2. Record memory definitions at block end so that
            //    the SSA graph for mem can be properly modified.
            // 3. Ensure that phi functions that will-be-needed for mem
            //    are present in the graph, initially with trivial inputs.
            // 4. Record all to-be-modified uses of mem;
            //    apply modifications (split into two steps to simplify and
            //    avoided nagging order-dependencies).
            // 5. Rewrite backedges to include reschedule check,
            //    and modify destination phi function appropriately with new
            //    definitions for mem.

            if (f.NoSplit)
            { // nosplit functions don't reschedule.
                return ;

            }

            var backedges = backedges(_addr_f);
            if (len(backedges) == 0L)
            { // no backedges means no rescheduling checks.
                return ;

            }

            var lastMems = findLastMems(_addr_f);

            var idom = f.Idom();
            var po = f.postorder(); 
            // The ordering in the dominator tree matters; it's important that
            // the walk of the dominator tree also be a preorder (i.e., a node is
            // visited only after all its non-backedge predecessors have been visited).
            var sdom = newSparseOrderedTree(f, idom, po);

            if (f.pass.debug > 1L)
            {
                fmt.Printf("before %s = %s\n", f.Name, sdom.treestructure(f.Entry));
            }

            edgeMem tofixBackedges = new slice<edgeMem>(new edgeMem[] {  });

            {
                var e__prev1 = e;

                foreach (var (_, __e) in backedges)
                {
                    e = __e; // TODO: could filter here by calls in loops, if declared and inferred nosplit are recorded in export data.
                    tofixBackedges = append(tofixBackedges, new edgeMem(e,nil));

                } 

                // It's possible that there is no memory state (no global/pointer loads/stores or calls)

                e = e__prev1;
            }

            if (lastMems[f.Entry.ID] == null)
            {
                lastMems[f.Entry.ID] = f.Entry.NewValue0(f.Entry.Pos, OpInitMem, types.TypeMem);
            }

            var memDefsAtBlockEnds = make_slice<ptr<Value>>(f.NumBlocks()); // For each block, the mem def seen at its bottom. Could be from earlier block.

            // Propagate last mem definitions forward through successor blocks.
            {
                var i__prev1 = i;

                for (var i = len(po) - 1L; i >= 0L; i--)
                {
                    var b = po[i];
                    var mem = lastMems[b.ID];
                    for (long j = 0L; mem == null; j++)
                    { // if there's no def, then there's no phi, so the visible mem is identical in all predecessors.
                        // loop because there might be backedges that haven't been visited yet.
                        mem = memDefsAtBlockEnds[b.Preds[j].b.ID];

                    }

                    memDefsAtBlockEnds[b.ID] = mem;
                    if (f.pass.debug > 2L)
                    {
                        fmt.Printf("memDefsAtBlockEnds[%s] = %s\n", b, mem);
                    }

                } 

                // Maps from block to newly-inserted phi function in block.


                i = i__prev1;
            } 

            // Maps from block to newly-inserted phi function in block.
            var newmemphis = make_map<ptr<Block>, rewrite>(); 

            // Insert phi functions as necessary for future changes to flow graph.
            {
                var i__prev1 = i;
                var emc__prev1 = emc;

                foreach (var (__i, __emc) in tofixBackedges)
                {
                    i = __i;
                    emc = __emc;
                    var e = emc.e;
                    var h = e.b; 

                    // find the phi function for the memory input at "h", if there is one.
                    ptr<Value> headerMemPhi; // look for header mem phi

                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in h.Values)
                        {
                            v = __v;
                            if (v.Op == OpPhi && v.Type.IsMemory())
                            {
                                headerMemPhi = v;
                            }

                        }

                        v = v__prev2;
                    }

                    if (headerMemPhi == null)
                    { 
                        // if the header is nil, make a trivial phi from the dominator
                        var mem0 = memDefsAtBlockEnds[idom[h.ID].ID];
                        headerMemPhi = newPhiFor(_addr_h, _addr_mem0);
                        newmemphis[h] = new rewrite(before:mem0,after:headerMemPhi);
                        addDFphis(_addr_mem0, _addr_h, _addr_h, _addr_f, memDefsAtBlockEnds, newmemphis, sdom);


                    }

                    tofixBackedges[i].m = headerMemPhi;


                }

                i = i__prev1;
                emc = emc__prev1;
            }

            if (f.pass.debug > 0L)
            {
                {
                    var b__prev1 = b;
                    var r__prev1 = r;

                    foreach (var (__b, __r) in newmemphis)
                    {
                        b = __b;
                        r = __r;
                        fmt.Printf("before b=%s, rewrite=%s\n", b, r.String());
                    }

                    b = b__prev1;
                    r = r__prev1;
                }
            } 

            // dfPhiTargets notes inputs to phis in dominance frontiers that should not
            // be rewritten as part of the dominated children of some outer rewrite.
            var dfPhiTargets = make_map<rewriteTarget, bool>();

            rewriteNewPhis(_addr_f.Entry, _addr_f.Entry, _addr_f, memDefsAtBlockEnds, newmemphis, dfPhiTargets, sdom);

            if (f.pass.debug > 0L)
            {
                {
                    var b__prev1 = b;
                    var r__prev1 = r;

                    foreach (var (__b, __r) in newmemphis)
                    {
                        b = __b;
                        r = __r;
                        fmt.Printf("after b=%s, rewrite=%s\n", b, r.String());
                    }

                    b = b__prev1;
                    r = r__prev1;
                }
            } 

            // Apply collected rewrites.
            {
                var r__prev1 = r;

                foreach (var (_, __r) in newmemphis)
                {
                    r = __r;
                    foreach (var (_, rw) in r.rewrites)
                    {
                        rw.v.SetArg(rw.i, r.after);
                    }

                } 

                // Rewrite backedges to include reschedule checks.

                r = r__prev1;
            }

            {
                var emc__prev1 = emc;

                foreach (var (_, __emc) in tofixBackedges)
                {
                    emc = __emc;
                    e = emc.e;
                    headerMemPhi = emc.m;
                    h = e.b;
                    i = e.i;
                    var p = h.Preds[i];
                    var bb = p.b;
                    mem0 = headerMemPhi.Args[i]; 
                    // bb e->p h,
                    // Because we're going to insert a rare-call, make sure the
                    // looping edge still looks likely.
                    var likely = BranchLikely;
                    if (p.i != 0L)
                    {
                        likely = BranchUnlikely;
                    }

                    if (bb.Kind != BlockPlain)
                    { // backedges can be unconditional. e.g., if x { something; continue }
                        bb.Likely = likely;

                    } 

                    // rewrite edge to include reschedule check
                    // existing edges:
                    //
                    // bb.Succs[p.i] == Edge{h, i}
                    // h.Preds[i] == p == Edge{bb,p.i}
                    //
                    // new block(s):
                    // test:
                    //    if sp < g.limit { goto sched }
                    //    goto join
                    // sched:
                    //    mem1 := call resched (mem0)
                    //    goto join
                    // join:
                    //    mem2 := phi(mem0, mem1)
                    //    goto h
                    //
                    // and correct arg i of headerMemPhi and headerCtrPhi
                    //
                    // EXCEPT: join block containing only phi functions is bad
                    // for the register allocator.  Therefore, there is no
                    // join, and branches targeting join must instead target
                    // the header, and the other phi functions within header are
                    // adjusted for the additional input.
                    var test = f.NewBlock(BlockIf);
                    var sched = f.NewBlock(BlockPlain);

                    test.Pos = bb.Pos;
                    sched.Pos = bb.Pos; 

                    // if sp < g.limit { goto sched }
                    // goto header

                    var cfgtypes = _addr_f.Config.Types;
                    var pt = cfgtypes.Uintptr;
                    var g = test.NewValue1(bb.Pos, OpGetG, pt, mem0);
                    var sp = test.NewValue0(bb.Pos, OpSP, pt);
                    var cmpOp = OpLess64U;
                    if (pt.Size() == 4L)
                    {
                        cmpOp = OpLess32U;
                    }

                    var limaddr = test.NewValue1I(bb.Pos, OpOffPtr, pt, 2L * pt.Size(), g);
                    var lim = test.NewValue2(bb.Pos, OpLoad, pt, limaddr, mem0);
                    var cmp = test.NewValue2(bb.Pos, cmpOp, cfgtypes.Bool, sp, lim);
                    test.SetControl(cmp); 

                    // if true, goto sched
                    test.AddEdgeTo(sched); 

                    // if false, rewrite edge to header.
                    // do NOT remove+add, because that will perturb all the other phi functions
                    // as well as messing up other edges to the header.
                    test.Succs = append(test.Succs, new Edge(h,i));
                    h.Preds[i] = new Edge(test,1);
                    headerMemPhi.SetArg(i, mem0);

                    test.Likely = BranchUnlikely; 

                    // sched:
                    //    mem1 := call resched (mem0)
                    //    goto header
                    var resched = f.fe.Syslook("goschedguarded");
                    var mem1 = sched.NewValue1A(bb.Pos, OpStaticCall, types.TypeMem, resched, mem0);
                    sched.AddEdgeTo(h);
                    headerMemPhi.AddArg(mem1);

                    bb.Succs[p.i] = new Edge(test,0);
                    test.Preds = append(test.Preds, new Edge(bb,p.i)); 

                    // Must correct all the other phi functions in the header for new incoming edge.
                    // Except for mem phis, it will be the same value seen on the original
                    // backedge at index i.
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in h.Values)
                        {
                            v = __v;
                            if (v.Op == OpPhi && v != headerMemPhi)
                            {
                                v.AddArg(v.Args[i]);
                            }

                        }

                        v = v__prev2;
                    }
                }

                emc = emc__prev1;
            }

            f.invalidateCFG();

            if (f.pass.debug > 1L)
            {
                sdom = newSparseTree(f, f.Idom());
                fmt.Printf("after %s = %s\n", f.Name, sdom.treestructure(f.Entry));
            }

        }

        // newPhiFor inserts a new Phi function into b,
        // with all inputs set to v.
        private static ptr<Value> newPhiFor(ptr<Block> _addr_b, ptr<Value> _addr_v)
        {
            ref Block b = ref _addr_b.val;
            ref Value v = ref _addr_v.val;

            var phiV = b.NewValue0(b.Pos, OpPhi, v.Type);

            foreach (>>MARKER:FORRANGEEXPRESSIONS_LEVEL_1<< in b.Preds)
            {>>MARKER:FORRANGEMUTABLEEXPRESSIONS_LEVEL_1<<
                phiV.AddArg(v);
            }
            return _addr_phiV!;

        }

        // rewriteNewPhis updates newphis[h] to record all places where the new phi function inserted
        // in block h will replace a previous definition.  Block b is the block currently being processed;
        // if b has its own phi definition then it takes the place of h.
        // defsForUses provides information about other definitions of the variable that are present
        // (and if nil, indicates that the variable is no longer live)
        // sdom must yield a preorder of the flow graph if recursively walked, root-to-children.
        // The result of newSparseOrderedTree with order supplied by a dfs-postorder satisfies this
        // requirement.
        private static void rewriteNewPhis(ptr<Block> _addr_h, ptr<Block> _addr_b, ptr<Func> _addr_f, slice<ptr<Value>> defsForUses, map<ptr<Block>, rewrite> newphis, map<rewriteTarget, bool> dfPhiTargets, SparseTree sdom)
        {
            ref Block h = ref _addr_h.val;
            ref Block b = ref _addr_b.val;
            ref Func f = ref _addr_f.val;
 
            // If b is a block with a new phi, then a new rewrite applies below it in the dominator tree.
            {
                var (_, ok) = newphis[b];

                if (ok)
                {
                    h = b;
                }

            }

            var change = newphis[h];
            var x = change.before;
            var y = change.after; 

            // Apply rewrites to this block
            if (x != null)
            { // don't waste time on the common case of no definition.
                var p = _addr_change.rewrites;
                {
                    var v__prev1 = v;

                    foreach (var (_, __v) in b.Values)
                    {
                        v = __v;
                        if (v == y)
                        { // don't rewrite self -- phi inputs are handled below.
                            continue;

                        }

                        foreach (var (i, w) in v.Args)
                        {
                            if (w != x)
                            {
                                continue;
                            }

                            rewriteTarget tgt = new rewriteTarget(v,i); 

                            // It's possible dominated control flow will rewrite this instead.
                            // Visiting in preorder (a property of how sdom was constructed)
                            // ensures that these are seen in the proper order.
                            if (dfPhiTargets[tgt])
                            {
                                continue;
                            }

                            p.val = append(p.val, tgt);
                            if (f.pass.debug > 1L)
                            {
                                fmt.Printf("added block target for h=%v, b=%v, x=%v, y=%v, tgt.v=%s, tgt.i=%d\n", h, b, x, y, v, i);
                            }

                        }

                    } 

                    // Rewrite appropriate inputs of phis reached in successors
                    // in dominance frontier, self, and dominated.
                    // If the variable def reaching uses in b is itself defined in b, then the new phi function
                    // does not reach the successors of b.  (This assumes a bit about the structure of the
                    // phi use-def graph, but it's true for memory.)

                    v = v__prev1;
                }

                {
                    var dfu = defsForUses[b.ID];

                    if (dfu != null && dfu.Block != b)
                    {
                        foreach (var (_, e) in b.Succs)
                        {
                            var s = e.b;

                            {
                                var v__prev2 = v;

                                foreach (var (_, __v) in s.Values)
                                {
                                    v = __v;
                                    if (v.Op == OpPhi && v.Args[e.i] == x)
                                    {
                                        tgt = new rewriteTarget(v,e.i);
                                        p.val = append(p.val, tgt);
                                        dfPhiTargets[tgt] = true;
                                        if (f.pass.debug > 1L)
                                        {
                                            fmt.Printf("added phi target for h=%v, b=%v, s=%v, x=%v, y=%v, tgt.v=%s, tgt.i=%d\n", h, b, s, x, y, v.LongString(), e.i);
                                        }

                                        break;

                                    }

                                }

                                v = v__prev2;
                            }
                        }

                    }

                }

                newphis[h] = change;

            }

            {
                var c = sdom[b.ID].child;

                while (c != null)
                {
                    rewriteNewPhis(_addr_h, _addr_c, _addr_f, defsForUses, newphis, dfPhiTargets, sdom); // TODO: convert to explicit stack from recursion.
                    c = sdom[c.ID].sibling;
                }

            }

        }

        // addDFphis creates new trivial phis that are necessary to correctly reflect (within SSA)
        // a new definition for variable "x" inserted at h (usually but not necessarily a phi).
        // These new phis can only occur at the dominance frontier of h; block s is in the dominance
        // frontier of h if h does not strictly dominate s and if s is a successor of a block b where
        // either b = h or h strictly dominates b.
        // These newly created phis are themselves new definitions that may require addition of their
        // own trivial phi functions in their own dominance frontier, and this is handled recursively.
        private static void addDFphis(ptr<Value> _addr_x, ptr<Block> _addr_h, ptr<Block> _addr_b, ptr<Func> _addr_f, slice<ptr<Value>> defForUses, map<ptr<Block>, rewrite> newphis, SparseTree sdom)
        {
            ref Value x = ref _addr_x.val;
            ref Block h = ref _addr_h.val;
            ref Block b = ref _addr_b.val;
            ref Func f = ref _addr_f.val;

            var oldv = defForUses[b.ID];
            if (oldv != x)
            { // either a new definition replacing x, or nil if it is proven that there are no uses reachable from b
                return ;

            }

            var idom = f.Idom();
outer:
            foreach (var (_, e) in b.Succs)
            {
                var s = e.b; 
                // check phi functions in the dominance frontier
                if (sdom.isAncestor(h, s))
                {
                    continue; // h dominates s, successor of b, therefore s is not in the frontier.
                }

                {
                    var (_, ok) = newphis[s];

                    if (ok)
                    {
                        continue; // successor s of b already has a new phi function, so there is no need to add another.
                    }

                }

                if (x != null)
                {
                    foreach (var (_, v) in s.Values)
                    {
                        if (v.Op == OpPhi && v.Args[e.i] == x)
                        {
                            _continueouter = true; // successor s of b has an old phi function, so there is no need to add another.
                            break;
                        }

                    }

                }

                var old = defForUses[idom[s.ID].ID]; // new phi function is correct-but-redundant, combining value "old" on all inputs.
                var headerPhi = newPhiFor(_addr_s, _addr_old); 
                // the new phi will replace "old" in block s and all blocks dominated by s.
                newphis[s] = new rewrite(before:old,after:headerPhi); // record new phi, to have inputs labeled "old" rewritten to "headerPhi"
                addDFphis(_addr_old, _addr_s, _addr_s, _addr_f, defForUses, newphis, sdom); // the new definition may also create new phi functions.
            }
            {
                var c = sdom[b.ID].child;

                while (c != null)
                {
                    addDFphis(_addr_x, _addr_h, _addr_c, _addr_f, defForUses, newphis, sdom); // TODO: convert to explicit stack from recursion.
                    c = sdom[c.ID].sibling;
                }

            }

        }

        // findLastMems maps block ids to last memory-output op in a block, if any
        private static slice<ptr<Value>> findLastMems(ptr<Func> _addr_f) => func((defer, _, __) =>
        {
            ref Func f = ref _addr_f.val;

            slice<ptr<Value>> stores = default;
            var lastMems = make_slice<ptr<Value>>(f.NumBlocks());
            var storeUse = f.newSparseSet(f.NumValues());
            defer(f.retSparseSet(storeUse));
            foreach (var (_, b) in f.Blocks)
            { 
                // Find all the stores in this block. Categorize their uses:
                //  storeUse contains stores which are used by a subsequent store.
                storeUse.clear();
                stores = stores[..0L];
                ptr<Value> memPhi;
                {
                    var v__prev2 = v;

                    foreach (var (_, __v) in b.Values)
                    {
                        v = __v;
                        if (v.Op == OpPhi)
                        {
                            if (v.Type.IsMemory())
                            {
                                memPhi = v;
                            }

                            continue;

                        }

                        if (v.Type.IsMemory())
                        {
                            stores = append(stores, v);
                            foreach (var (_, a) in v.Args)
                            {
                                if (a.Block == b && a.Type.IsMemory())
                                {
                                    storeUse.add(a.ID);
                                }

                            }

                        }

                    }

                    v = v__prev2;
                }

                if (len(stores) == 0L)
                {
                    lastMems[b.ID] = memPhi;
                    continue;
                } 

                // find last store in the block
                ptr<Value> last;
                {
                    var v__prev2 = v;

                    foreach (var (_, __v) in stores)
                    {
                        v = __v;
                        if (storeUse.contains(v.ID))
                        {
                            continue;
                        }

                        if (last != null)
                        {
                            b.Fatalf("two final stores - simultaneous live stores %s %s", last, v);
                        }

                        last = v;

                    }

                    v = v__prev2;
                }

                if (last == null)
                {
                    b.Fatalf("no last store found - cycle?");
                }

                lastMems[b.ID] = last;

            }
            return lastMems;

        });

        // mark values
        private partial struct markKind // : byte
        {
        }

        private static readonly markKind notFound = (markKind)iota; // block has not been discovered yet
        private static readonly var notExplored = (var)0; // discovered and in queue, outedges not processed yet
        private static readonly var explored = (var)1; // discovered and in queue, outedges processed
        private static readonly var done = (var)2; // all done, in output ordering

        private partial struct backedgesState
        {
            public ptr<Block> b;
            public long i;
        }

        // backedges returns a slice of successor edges that are back
        // edges.  For reducible loops, edge.b is the header.
        private static slice<Edge> backedges(ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            Edge edges = new slice<Edge>(new Edge[] {  });
            var mark = make_slice<markKind>(f.NumBlocks());
            backedgesState stack = new slice<backedgesState>(new backedgesState[] {  });

            mark[f.Entry.ID] = notExplored;
            stack = append(stack, new backedgesState(f.Entry,0));

            while (len(stack) > 0L)
            {
                var l = len(stack);
                var x = stack[l - 1L];
                if (x.i < len(x.b.Succs))
                {
                    var e = x.b.Succs[x.i];
                    stack[l - 1L].i++;
                    var s = e.b;
                    if (mark[s.ID] == notFound)
                    {
                        mark[s.ID] = notExplored;
                        stack = append(stack, new backedgesState(s,0));
                    }
                    else if (mark[s.ID] == notExplored)
                    {
                        edges = append(edges, e);
                    }

                }
                else
                {
                    mark[x.b.ID] = done;
                    stack = stack[0L..l - 1L];
                }

            }

            return edges;

        }
    }
}}}}
