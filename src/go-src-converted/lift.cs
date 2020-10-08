// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 08 04:57:01 UTC
// import "golang.org/x/tools/go/ssa" ==> using ssa = go.golang.org.x.tools.go.ssa_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\lift.go
// This file defines the lifting pass which tries to "lift" Alloc
// cells (new/local variables) into SSA registers, replacing loads
// with the dominating stored value, eliminating loads and stores, and
// inserting φ-nodes as needed.

// Cited papers and resources:
//
// Ron Cytron et al. 1991. Efficiently computing SSA form...
// http://doi.acm.org/10.1145/115372.115320
//
// Cooper, Harvey, Kennedy.  2001.  A Simple, Fast Dominance Algorithm.
// Software Practice and Experience 2001, 4:1-10.
// http://www.hipersoft.rice.edu/grads/publications/dom14.pdf
//
// Daniel Berlin, llvmdev mailing list, 2012.
// http://lists.cs.uiuc.edu/pipermail/llvmdev/2012-January/046638.html
// (Be sure to expand the whole thread.)

// TODO(adonovan): opt: there are many optimizations worth evaluating, and
// the conventional wisdom for SSA construction is that a simple
// algorithm well engineered often beats those of better asymptotic
// complexity on all but the most egregious inputs.
//
// Danny Berlin suggests that the Cooper et al. algorithm for
// computing the dominance frontier is superior to Cytron et al.
// Furthermore he recommends that rather than computing the DF for the
// whole function then renaming all alloc cells, it may be cheaper to
// compute the DF for each alloc cell separately and throw it away.
//
// Consider exploiting liveness information to avoid creating dead
// φ-nodes which we then immediately remove.
//
// Also see many other "TODO: opt" suggestions in the code.

using fmt = go.fmt_package;
using token = go.go.token_package;
using types = go.go.types_package;
using big = go.math.big_package;
using os = go.os_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go
{
    public static partial class ssa_package
    {
        // If true, show diagnostic information at each step of lifting.
        // Very verbose.
        private static readonly var debugLifting = (var)false;

        // domFrontier maps each block to the set of blocks in its dominance
        // frontier.  The outer slice is conceptually a map keyed by
        // Block.Index.  The inner slice is conceptually a set, possibly
        // containing duplicates.
        //
        // TODO(adonovan): opt: measure impact of dups; consider a packed bit
        // representation, e.g. big.Int, and bitwise parallel operations for
        // the union step in the Children loop.
        //
        // domFrontier's methods mutate the slice's elements but not its
        // length, so their receivers needn't be pointers.
        //


        // domFrontier maps each block to the set of blocks in its dominance
        // frontier.  The outer slice is conceptually a map keyed by
        // Block.Index.  The inner slice is conceptually a set, possibly
        // containing duplicates.
        //
        // TODO(adonovan): opt: measure impact of dups; consider a packed bit
        // representation, e.g. big.Int, and bitwise parallel operations for
        // the union step in the Children loop.
        //
        // domFrontier's methods mutate the slice's elements but not its
        // length, so their receivers needn't be pointers.
        //
        private partial struct domFrontier // : slice<slice<ptr<BasicBlock>>>
        {
        }

        private static void add(this domFrontier df, ptr<BasicBlock> _addr_u, ptr<BasicBlock> _addr_v)
        {
            ref BasicBlock u = ref _addr_u.val;
            ref BasicBlock v = ref _addr_v.val;

            var p = _addr_df[u.Index];
            p.val = append(p.val, v);
        }

        // build builds the dominance frontier df for the dominator (sub)tree
        // rooted at u, using the Cytron et al. algorithm.
        //
        // TODO(adonovan): opt: consider Berlin approach, computing pruned SSA
        // by pruning the entire IDF computation, rather than merely pruning
        // the DF -> IDF step.
        private static void build(this domFrontier df, ptr<BasicBlock> _addr_u)
        {
            ref BasicBlock u = ref _addr_u.val;
 
            // Encounter each node u in postorder of dom tree.
            foreach (var (_, child) in u.dom.children)
            {
                df.build(child);
            }
            {
                var vb__prev1 = vb;

                foreach (var (_, __vb) in u.Succs)
                {
                    vb = __vb;
                    {
                        var v__prev1 = v;

                        var v = vb.dom;

                        if (v.idom != u)
                        {
                            df.add(u, vb);
                        }

                        v = v__prev1;

                    }

                }

                vb = vb__prev1;
            }

            foreach (var (_, w) in u.dom.children)
            {
                {
                    var vb__prev2 = vb;

                    foreach (var (_, __vb) in df[w.Index])
                    {
                        vb = __vb; 
                        // TODO(adonovan): opt: use word-parallel bitwise union.
                        {
                            var v__prev1 = v;

                            v = vb.dom;

                            if (v.idom != u)
                            {
                                df.add(u, vb);
                            }

                            v = v__prev1;

                        }

                    }

                    vb = vb__prev2;
                }
            }

        }

        private static domFrontier buildDomFrontier(ptr<Function> _addr_fn)
        {
            ref Function fn = ref _addr_fn.val;

            var df = make(domFrontier, len(fn.Blocks));
            df.build(fn.Blocks[0L]);
            if (fn.Recover != null)
            {
                df.build(fn.Recover);
            }

            return df;

        }

        private static slice<Instruction> removeInstr(slice<Instruction> refs, Instruction instr)
        {
            long i = 0L;
            foreach (var (_, ref) in refs)
            {
                if (ref == instr)
                {
                    continue;
                }

                refs[i] = ref;
                i++;

            }
            for (var j = i; j != len(refs); j++)
            {
                refs[j] = null; // aid GC
            }

            return refs[..i];

        }

        // lift replaces local and new Allocs accessed only with
        // load/store by SSA registers, inserting φ-nodes where necessary.
        // The result is a program in classical pruned SSA form.
        //
        // Preconditions:
        // - fn has no dead blocks (blockopt has run).
        // - Def/use info (Operands and Referrers) is up-to-date.
        // - The dominator tree is up-to-date.
        //
        private static void lift(ptr<Function> _addr_fn)
        {
            ref Function fn = ref _addr_fn.val;
 
            // TODO(adonovan): opt: lots of little optimizations may be
            // worthwhile here, especially if they cause us to avoid
            // buildDomFrontier.  For example:
            //
            // - Alloc never loaded?  Eliminate.
            // - Alloc never stored?  Replace all loads with a zero constant.
            // - Alloc stored once?  Replace loads with dominating store;
            //   don't forget that an Alloc is itself an effective store
            //   of zero.
            // - Alloc used only within a single block?
            //   Use degenerate algorithm avoiding φ-nodes.
            // - Consider synergy with scalar replacement of aggregates (SRA).
            //   e.g. *(&x.f) where x is an Alloc.
            //   Perhaps we'd get better results if we generated this as x.f
            //   i.e. Field(x, .f) instead of Load(FieldIndex(x, .f)).
            //   Unclear.
            //
            // But we will start with the simplest correct code.
            var df = buildDomFrontier(_addr_fn);

            if (debugLifting)
            {
                var title = false;
                {
                    var i__prev1 = i;

                    foreach (var (__i, __blocks) in df)
                    {
                        i = __i;
                        blocks = __blocks;
                        if (blocks != null)
                        {
                            if (!title)
                            {
                                fmt.Fprintf(os.Stderr, "Dominance frontier of %s:\n", fn);
                                title = true;
                            }

                            fmt.Fprintf(os.Stderr, "\t%s: %s\n", fn.Blocks[i], blocks);

                        }

                    }

                    i = i__prev1;
                }
            }

            var newPhis = make(newPhiMap); 

            // During this pass we will replace some BasicBlock.Instrs
            // (allocs, loads and stores) with nil, keeping a count in
            // BasicBlock.gaps.  At the end we will reset Instrs to the
            // concatenation of all non-dead newPhis and non-nil Instrs
            // for the block, reusing the original array if space permits.

            // While we're here, we also eliminate 'rundefers'
            // instructions in functions that contain no 'defer'
            // instructions.
            var usesDefer = false; 

            // A counter used to generate ~unique ids for Phi nodes, as an
            // aid to debugging.  We use large numbers to make them highly
            // visible.  All nodes are renumbered later.
            ref long fresh = ref heap(1000L, out ptr<long> _addr_fresh); 

            // Determine which allocs we can lift and number them densely.
            // The renaming phase uses this numbering for compact maps.
            long numAllocs = 0L;
            {
                var b__prev1 = b;

                foreach (var (_, __b) in fn.Blocks)
                {
                    b = __b;
                    b.gaps = 0L;
                    b.rundefers = 0L;
                    {
                        var instr__prev2 = instr;

                        foreach (var (_, __instr) in b.Instrs)
                        {
                            instr = __instr;
                            switch (instr.type())
                            {
                                case ptr<Alloc> instr:
                                    long index = -1L;
                                    if (liftAlloc(df, _addr_instr, newPhis, _addr_fresh))
                                    {
                                        index = numAllocs;
                                        numAllocs++;
                                    }

                                    instr.index = index;
                                    break;
                                case ptr<Defer> instr:
                                    usesDefer = true;
                                    break;
                                case ptr<RunDefers> instr:
                                    b.rundefers++;
                                    break;
                            }

                        }

                        instr = instr__prev2;
                    }
                } 

                // renaming maps an alloc (keyed by index) to its replacement
                // value.  Initially the renaming contains nil, signifying the
                // zero constant of the appropriate type; we construct the
                // Const lazily at most once on each path through the domtree.
                // TODO(adonovan): opt: cache per-function not per subtree.

                b = b__prev1;
            }

            var renaming = make_slice<Value>(numAllocs); 

            // Renaming.
            rename(_addr_fn.Blocks[0L], renaming, newPhis); 

            // Eliminate dead φ-nodes.
            removeDeadPhis(fn.Blocks, newPhis); 

            // Prepend remaining live φ-nodes to each block.
            {
                var b__prev1 = b;

                foreach (var (_, __b) in fn.Blocks)
                {
                    b = __b;
                    var nps = newPhis[b];
                    var j = len(nps);

                    var rundefersToKill = b.rundefers;
                    if (usesDefer)
                    {
                        rundefersToKill = 0L;
                    }

                    if (j + b.gaps + rundefersToKill == 0L)
                    {
                        continue; // fast path: no new phis or gaps
                    } 

                    // Compact nps + non-nil Instrs into a new slice.
                    // TODO(adonovan): opt: compact in situ (rightwards)
                    // if Instrs has sufficient space or slack.
                    var dst = make_slice<Instruction>(len(b.Instrs) + j - b.gaps - rundefersToKill);
                    {
                        var i__prev2 = i;

                        foreach (var (__i, __np) in nps)
                        {
                            i = __i;
                            np = __np;
                            dst[i] = np.phi;
                        }

                        i = i__prev2;
                    }

                    {
                        var instr__prev2 = instr;

                        foreach (var (_, __instr) in b.Instrs)
                        {
                            instr = __instr;
                            if (instr == null)
                            {
                                continue;
                            }

                            if (!usesDefer)
                            {
                                {
                                    ptr<RunDefers> (_, ok) = instr._<ptr<RunDefers>>();

                                    if (ok)
                                    {
                                        continue;
                                    }

                                }

                            }

                            dst[j] = instr;
                            j++;

                        }

                        instr = instr__prev2;
                    }

                    b.Instrs = dst;

                } 

                // Remove any fn.Locals that were lifted.

                b = b__prev1;
            }

            j = 0L;
            foreach (var (_, l) in fn.Locals)
            {
                if (l.index < 0L)
                {
                    fn.Locals[j] = l;
                    j++;
                }

            } 
            // Nil out fn.Locals[j:] to aid GC.
            {
                var i__prev1 = i;

                for (var i = j; i < len(fn.Locals); i++)
                {
                    fn.Locals[i] = null;
                }


                i = i__prev1;
            }
            fn.Locals = fn.Locals[..j];

        }

        // removeDeadPhis removes φ-nodes not transitively needed by a
        // non-Phi, non-DebugRef instruction.
        private static void removeDeadPhis(slice<ptr<BasicBlock>> blocks, newPhiMap newPhis)
        { 
            // First pass: find the set of "live" φ-nodes: those reachable
            // from some non-Phi instruction.
            //
            // We compute reachability in reverse, starting from each φ,
            // rather than forwards, starting from each live non-Phi
            // instruction, because this way visits much less of the
            // Value graph.
            var livePhis = make_map<ptr<Phi>, bool>();
            {
                var npList__prev1 = npList;

                foreach (var (_, __npList) in newPhis)
                {
                    npList = __npList;
                    {
                        var np__prev2 = np;

                        foreach (var (_, __np) in npList)
                        {
                            np = __np;
                            var phi = np.phi;
                            if (!livePhis[phi] && phiHasDirectReferrer(_addr_phi))
                            {
                                markLivePhi(livePhis, _addr_phi);
                            }

                        }

                        np = np__prev2;
                    }
                } 

                // Existing φ-nodes due to && and || operators
                // are all considered live (see Go issue 19622).

                npList = npList__prev1;
            }

            foreach (var (_, b) in blocks)
            {
                {
                    var phi__prev2 = phi;

                    foreach (var (_, __phi) in b.phis())
                    {
                        phi = __phi;
                        markLivePhi(livePhis, phi._<ptr<Phi>>());
                    }

                    phi = phi__prev2;
                }
            } 

            // Second pass: eliminate unused phis from newPhis.
            {
                var npList__prev1 = npList;

                foreach (var (__block, __npList) in newPhis)
                {
                    block = __block;
                    npList = __npList;
                    long j = 0L;
                    {
                        var np__prev2 = np;

                        foreach (var (_, __np) in npList)
                        {
                            np = __np;
                            if (livePhis[np.phi])
                            {
                                npList[j] = np;
                                j++;
                            }
                            else
                            { 
                                // discard it, first removing it from referrers
                                foreach (var (_, val) in np.phi.Edges)
                                {
                                    {
                                        var refs = val.Referrers();

                                        if (refs != null)
                                        {
                                            refs.val = removeInstr(refs.val, np.phi);
                                        }

                                    }

                                }
                                np.phi.block = null;

                            }

                        }

                        np = np__prev2;
                    }

                    newPhis[block] = npList[..j];

                }

                npList = npList__prev1;
            }
        }

        // markLivePhi marks phi, and all φ-nodes transitively reachable via
        // its Operands, live.
        private static void markLivePhi(map<ptr<Phi>, bool> livePhis, ptr<Phi> _addr_phi)
        {
            ref Phi phi = ref _addr_phi.val;

            livePhis[phi] = true;
            foreach (var (_, rand) in phi.Operands(null))
            {
                {
                    ptr<Phi> (q, ok) = (rand.val)._<ptr<Phi>>();

                    if (ok)
                    {
                        if (!livePhis[q])
                        {
                            markLivePhi(livePhis, q);
                        }

                    }

                }

            }

        }

        // phiHasDirectReferrer reports whether phi is directly referred to by
        // a non-Phi instruction.  Such instructions are the
        // roots of the liveness traversal.
        private static bool phiHasDirectReferrer(ptr<Phi> _addr_phi)
        {
            ref Phi phi = ref _addr_phi.val;

            foreach (var (_, instr) in phi.Referrers().val)
            {
                {
                    ptr<Phi> (_, ok) = instr._<ptr<Phi>>();

                    if (!ok)
                    {
                        return true;
                    }

                }

            }
            return false;

        }

        private partial struct blockSet
        {
            public ref big.Int Int => ref Int_val;
        } // (inherit methods from Int)

        // add adds b to the set and returns true if the set changed.
        private static bool add(this ptr<blockSet> _addr_s, ptr<BasicBlock> _addr_b)
        {
            ref blockSet s = ref _addr_s.val;
            ref BasicBlock b = ref _addr_b.val;

            var i = b.Index;
            if (s.Bit(i) != 0L)
            {
                return false;
            }

            s.SetBit(_addr_s.Int, i, 1L);
            return true;

        }

        // take removes an arbitrary element from a set s and
        // returns its index, or returns -1 if empty.
        private static long take(this ptr<blockSet> _addr_s)
        {
            ref blockSet s = ref _addr_s.val;

            var l = s.BitLen();
            for (long i = 0L; i < l; i++)
            {
                if (s.Bit(i) == 1L)
                {
                    s.SetBit(_addr_s.Int, i, 0L);
                    return i;
                }

            }

            return -1L;

        }

        // newPhi is a pair of a newly introduced φ-node and the lifted Alloc
        // it replaces.
        private partial struct newPhi
        {
            public ptr<Phi> phi;
            public ptr<Alloc> alloc;
        }

        // newPhiMap records for each basic block, the set of newPhis that
        // must be prepended to the block.
        private partial struct newPhiMap // : map<ptr<BasicBlock>, slice<newPhi>>
        {
        }

        // liftAlloc determines whether alloc can be lifted into registers,
        // and if so, it populates newPhis with all the φ-nodes it may require
        // and returns true.
        //
        // fresh is a source of fresh ids for phi nodes.
        //
        private static bool liftAlloc(domFrontier df, ptr<Alloc> _addr_alloc, newPhiMap newPhis, ptr<long> _addr_fresh) => func((_, panic, __) =>
        {
            ref Alloc alloc = ref _addr_alloc.val;
            ref long fresh = ref _addr_fresh.val;
 
            // Don't lift aggregates into registers, because we don't have
            // a way to express their zero-constants.
            switch (deref(alloc.Type()).Underlying().type())
            {
                case ptr<types.Array> _:
                    return false;
                    break;
                case ptr<types.Struct> _:
                    return false;
                    break; 

                // Don't lift named return values in functions that defer
                // calls that may recover from panic.
            } 

            // Don't lift named return values in functions that defer
            // calls that may recover from panic.
            {
                var fn__prev1 = fn;

                var fn = alloc.Parent();

                if (fn.Recover != null)
                {
                    foreach (var (_, nr) in fn.namedResults)
                    {
                        if (nr == alloc)
                        {
                            return false;
                        }

                    }

                } 

                // Compute defblocks, the set of blocks containing a
                // definition of the alloc cell.

                fn = fn__prev1;

            } 

            // Compute defblocks, the set of blocks containing a
            // definition of the alloc cell.
            blockSet defblocks = default;
            {
                var instr__prev1 = instr;

                foreach (var (_, __instr) in alloc.Referrers().val)
                {
                    instr = __instr; 
                    // Bail out if we discover the alloc is not liftable;
                    // the only operations permitted to use the alloc are
                    // loads/stores into the cell, and DebugRef.
                    switch (instr.type())
                    {
                        case ptr<Store> instr:
                            if (instr.Val == alloc)
                            {
                                return false; // address used as value
                            }

                            if (instr.Addr != alloc)
                            {
                                panic("Alloc.Referrers is inconsistent");
                            }

                            defblocks.add(instr.Block());
                            break;
                        case ptr<UnOp> instr:
                            if (instr.Op != token.MUL)
                            {
                                return false; // not a load
                            }

                            if (instr.X != alloc)
                            {
                                panic("Alloc.Referrers is inconsistent");
                            }

                            break;
                        case ptr<DebugRef> instr:
                            break;
                        default:
                        {
                            var instr = instr.type();
                            return false; // some other instruction
                            break;
                        }
                    }

                } 
                // The Alloc itself counts as a (zero) definition of the cell.

                instr = instr__prev1;
            }

            defblocks.add(alloc.Block());

            if (debugLifting)
            {
                fmt.Fprintln(os.Stderr, "\tlifting ", alloc, alloc.Name());
            }

            fn = alloc.Parent(); 

            // Φ-insertion.
            //
            // What follows is the body of the main loop of the insert-φ
            // function described by Cytron et al, but instead of using
            // counter tricks, we just reset the 'hasAlready' and 'work'
            // sets each iteration.  These are bitmaps so it's pretty cheap.
            //
            // TODO(adonovan): opt: recycle slice storage for W,
            // hasAlready, defBlocks across liftAlloc calls.
            blockSet hasAlready = default; 

            // Initialize W and work to defblocks.
            blockSet work = defblocks; // blocks seen
            blockSet W = default; // blocks to do
            W.Set(_addr_defblocks.Int); 

            // Traverse iterated dominance frontier, inserting φ-nodes.
            {
                var i = W.take();

                while (i != -1L)
                {
                    var u = fn.Blocks[i];
                    foreach (var (_, v) in df[u.Index])
                    {
                        if (hasAlready.add(v))
                        { 
                            // Create φ-node.
                            // It will be prepended to v.Instrs later, if needed.
                            ptr<Phi> phi = addr(new Phi(Edges:make([]Value,len(v.Preds)),Comment:alloc.Comment,)); 
                            // This is merely a debugging aid:
                            phi.setNum(fresh);
                            fresh++;

                            phi.pos = alloc.Pos();
                            phi.setType(deref(alloc.Type()));
                            phi.block = v;
                            if (debugLifting)
                            {
                                fmt.Fprintf(os.Stderr, "\tplace %s = %s at block %s\n", phi.Name(), phi, v);
                            }

                            newPhis[v] = append(newPhis[v], new newPhi(phi,alloc));

                            if (work.add(v))
                            {
                                W.add(v);
                    i = W.take();
                            }

                        }

                    }

                }

            }

            return true;

        });

        // replaceAll replaces all intraprocedural uses of x with y,
        // updating x.Referrers and y.Referrers.
        // Precondition: x.Referrers() != nil, i.e. x must be local to some function.
        //
        private static void replaceAll(Value x, Value y)
        {
            slice<ptr<Value>> rands = default;
            var pxrefs = x.Referrers();
            var pyrefs = y.Referrers();
            foreach (var (_, instr) in pxrefs.val)
            {
                rands = instr.Operands(rands[..0L]); // recycle storage
                foreach (var (_, rand) in rands)
                {
                    if (rand != null.val)
                    {
                        if (rand == x.val)
                        {
                            rand.val = y;
                        }

                    }

                }
                if (pyrefs != null)
                {
                    pyrefs.val = append(pyrefs.val, instr); // dups ok
                }

            }
            pxrefs.val = null; // x is now unreferenced
        }

        // renamed returns the value to which alloc is being renamed,
        // constructing it lazily if it's the implicit zero initialization.
        //
        private static Value renamed(slice<Value> renaming, ptr<Alloc> _addr_alloc)
        {
            ref Alloc alloc = ref _addr_alloc.val;

            var v = renaming[alloc.index];
            if (v == null)
            {
                v = zeroConst(deref(alloc.Type()));
                renaming[alloc.index] = v;
            }

            return v;

        }

        // rename implements the (Cytron et al) SSA renaming algorithm, a
        // preorder traversal of the dominator tree replacing all loads of
        // Alloc cells with the value stored to that cell by the dominating
        // store instruction.  For lifting, we need only consider loads,
        // stores and φ-nodes.
        //
        // renaming is a map from *Alloc (keyed by index number) to its
        // dominating stored value; newPhis[x] is the set of new φ-nodes to be
        // prepended to block x.
        //
        private static void rename(ptr<BasicBlock> _addr_u, slice<Value> renaming, newPhiMap newPhis)
        {
            ref BasicBlock u = ref _addr_u.val;
 
            // Each φ-node becomes the new name for its associated Alloc.
            {
                var np__prev1 = np;

                foreach (var (_, __np) in newPhis[u])
                {
                    np = __np;
                    var phi = np.phi;
                    var alloc = np.alloc;
                    renaming[alloc.index] = phi;
                } 

                // Rename loads and stores of allocs.

                np = np__prev1;
            }

            {
                var i__prev1 = i;
                var instr__prev1 = instr;

                foreach (var (__i, __instr) in u.Instrs)
                {
                    i = __i;
                    instr = __instr;
                    switch (instr.type())
                    {
                        case ptr<Alloc> instr:
                            if (instr.index >= 0L)
                            { // store of zero to Alloc cell
                                // Replace dominated loads by the zero value.
                                renaming[instr.index] = null;
                                if (debugLifting)
                                {
                                    fmt.Fprintf(os.Stderr, "\tkill alloc %s\n", instr);
                                } 
                                // Delete the Alloc.
                                u.Instrs[i] = null;
                                u.gaps++;

                            }

                            break;
                        case ptr<Store> instr:
                            {
                                var alloc__prev1 = alloc;

                                ptr<Alloc> (alloc, ok) = instr.Addr._<ptr<Alloc>>();

                                if (ok && alloc.index >= 0L)
                                { // store to Alloc cell
                                    // Replace dominated loads by the stored value.
                                    renaming[alloc.index] = instr.Val;
                                    if (debugLifting)
                                    {
                                        fmt.Fprintf(os.Stderr, "\tkill store %s; new value: %s\n", instr, instr.Val.Name());
                                    } 
                                    // Remove the store from the referrer list of the stored value.
                                    {
                                        var refs__prev2 = refs;

                                        var refs = instr.Val.Referrers();

                                        if (refs != null)
                                        {
                                            refs.val = removeInstr(refs.val, instr);
                                        } 
                                        // Delete the Store.

                                        refs = refs__prev2;

                                    } 
                                    // Delete the Store.
                                    u.Instrs[i] = null;
                                    u.gaps++;

                                }

                                alloc = alloc__prev1;

                            }


                            break;
                        case ptr<UnOp> instr:
                            if (instr.Op == token.MUL)
                            {
                                {
                                    var alloc__prev2 = alloc;

                                    (alloc, ok) = instr.X._<ptr<Alloc>>();

                                    if (ok && alloc.index >= 0L)
                                    { // load of Alloc cell
                                        var newval = renamed(renaming, _addr_alloc);
                                        if (debugLifting)
                                        {
                                            fmt.Fprintf(os.Stderr, "\tupdate load %s = %s with %s\n", instr.Name(), instr, newval.Name());
                                        } 
                                        // Replace all references to
                                        // the loaded value by the
                                        // dominating stored value.
                                        replaceAll(instr, newval); 
                                        // Delete the Load.
                                        u.Instrs[i] = null;
                                        u.gaps++;

                                    }

                                    alloc = alloc__prev2;

                                }

                            }

                            break;
                        case ptr<DebugRef> instr:
                            {
                                var alloc__prev1 = alloc;

                                (alloc, ok) = instr.X._<ptr<Alloc>>();

                                if (ok && alloc.index >= 0L)
                                { // ref of Alloc cell
                                    if (instr.IsAddr)
                                    {
                                        instr.X = renamed(renaming, _addr_alloc);
                                        instr.IsAddr = false; 

                                        // Add DebugRef to instr.X's referrers.
                                        {
                                            var refs__prev3 = refs;

                                            refs = instr.X.Referrers();

                                            if (refs != null)
                                            {
                                                refs.val = append(refs.val, instr);
                                            }

                                            refs = refs__prev3;

                                        }

                                    }
                                    else
                                    { 
                                        // A source expression denotes the address
                                        // of an Alloc that was optimized away.
                                        instr.X = null; 

                                        // Delete the DebugRef.
                                        u.Instrs[i] = null;
                                        u.gaps++;

                                    }

                                }

                                alloc = alloc__prev1;

                            }

                            break;
                    }

                } 

                // For each φ-node in a CFG successor, rename the edge.

                i = i__prev1;
                instr = instr__prev1;
            }

            {
                var v__prev1 = v;

                foreach (var (_, __v) in u.Succs)
                {
                    v = __v;
                    var phis = newPhis[v];
                    if (len(phis) == 0L)
                    {
                        continue;
                    }

                    var i = v.predIndex(u);
                    {
                        var np__prev2 = np;

                        foreach (var (_, __np) in phis)
                        {
                            np = __np;
                            phi = np.phi;
                            alloc = np.alloc;
                            newval = renamed(renaming, _addr_alloc);
                            if (debugLifting)
                            {
                                fmt.Fprintf(os.Stderr, "\tsetphi %s edge %s -> %s (#%d) (alloc=%s) := %s\n", phi.Name(), u, v, i, alloc.Name(), newval.Name());
                            }

                            phi.Edges[i] = newval;
                            {
                                var prefs = newval.Referrers();

                                if (prefs != null)
                                {
                                    prefs.val = append(prefs.val, phi);
                                }

                            }

                        }

                        np = np__prev2;
                    }
                } 

                // Continue depth-first recursion over domtree, pushing a
                // fresh copy of the renaming map for each subtree.

                v = v__prev1;
            }

            {
                var i__prev1 = i;
                var v__prev1 = v;

                foreach (var (__i, __v) in u.dom.children)
                {
                    i = __i;
                    v = __v;
                    var r = renaming;
                    if (i < len(u.dom.children) - 1L)
                    { 
                        // On all but the final iteration, we must make
                        // a copy to avoid destructive update.
                        r = make_slice<Value>(len(renaming));
                        copy(r, renaming);

                    }

                    rename(_addr_v, r, newPhis);

                }

                i = i__prev1;
                v = v__prev1;
            }
        }
    }
}}}}}
