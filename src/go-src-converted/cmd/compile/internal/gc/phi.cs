// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 08 04:29:55 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\phi.go
using ssa = go.cmd.compile.@internal.ssa_package;
using types = go.cmd.compile.@internal.types_package;
using src = go.cmd.@internal.src_package;
using heap = go.container.heap_package;
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        // This file contains the algorithm to place phi nodes in a function.
        // For small functions, we use Braun, Buchwald, Hack, Leißa, Mallon, and Zwinkau.
        // https://pp.info.uni-karlsruhe.de/uploads/publikationen/braun13cc.pdf
        // For large functions, we use Sreedhar & Gao: A Linear Time Algorithm for Placing Φ-Nodes.
        // http://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.8.1979&rep=rep1&type=pdf
        private static readonly long smallBlocks = (long)500L;



        private static readonly var debugPhi = (var)false;

        // insertPhis finds all the places in the function where a phi is
        // necessary and inserts them.
        // Uses FwdRef ops to find all uses of variables, and s.defvars to find
        // all definitions.
        // Phi values are inserted, and all FwdRefs are changed to a Copy
        // of the appropriate phi or definition.
        // TODO: make this part of cmd/compile/internal/ssa somehow?


        // insertPhis finds all the places in the function where a phi is
        // necessary and inserts them.
        // Uses FwdRef ops to find all uses of variables, and s.defvars to find
        // all definitions.
        // Phi values are inserted, and all FwdRefs are changed to a Copy
        // of the appropriate phi or definition.
        // TODO: make this part of cmd/compile/internal/ssa somehow?
        private static void insertPhis(this ptr<state> _addr_s)
        {
            ref state s = ref _addr_s.val;

            if (len(s.f.Blocks) <= smallBlocks)
            {
                simplePhiState sps = new simplePhiState(s:s,f:s.f,defvars:s.defvars);
                sps.insertPhis();
                return ;
            }

            phiState ps = new phiState(s:s,f:s.f,defvars:s.defvars);
            ps.insertPhis();

        }

        private partial struct phiState
        {
            public ptr<state> s; // SSA state
            public ptr<ssa.Func> f; // function to work on
            public slice<map<ptr<Node>, ptr<ssa.Value>>> defvars; // defined variables at end of each block

            public map<ptr<Node>, int> varnum; // variable numbering

// properties of the dominator tree
            public slice<ptr<ssa.Block>> idom; // dominator parents
            public slice<domBlock> tree; // dominator child+sibling
            public slice<int> level; // level in dominator tree (0 = root or unreachable, 1 = children of root, ...)

// scratch locations
            public blockHeap priq; // priority queue of blocks, higher level (toward leaves) = higher priority
            public slice<ptr<ssa.Block>> q; // inner loop queue
            public ptr<sparseSet> queued; // has been put in q
            public ptr<sparseSet> hasPhi; // has a phi
            public ptr<sparseSet> hasDef; // has a write of the variable we're processing

// miscellaneous
            public ptr<ssa.Value> placeholder; // dummy value to use as a "not set yet" placeholder.
        }

        private static void insertPhis(this ptr<phiState> _addr_s)
        {
            ref phiState s = ref _addr_s.val;

            if (debugPhi)
            {
                fmt.Println(s.f.String());
            } 

            // Find all the variables for which we need to match up reads & writes.
            // This step prunes any basic-block-only variables from consideration.
            // Generate a numbering for these variables.
            s.varnum = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<Node>, int>{};
            slice<ptr<Node>> vars = default;
            slice<ptr<types.Type>> vartypes = default;
            {
                var b__prev1 = b;

                foreach (var (_, __b) in s.f.Blocks)
                {
                    b = __b;
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in b.Values)
                        {
                            v = __v;
                            if (v.Op != ssa.OpFwdRef)
                            {
                                continue;
                            }

                            ptr<Node> var_ = v.Aux._<ptr<Node>>(); 

                            // Optimization: look back 1 block for the definition.
                            if (len(b.Preds) == 1L)
                            {
                                var c = b.Preds[0L].Block();
                                {
                                    var w = s.defvars[c.ID][var_];

                                    if (w != null)
                                    {
                                        v.Op = ssa.OpCopy;
                                        v.Aux = null;
                                        v.AddArg(w);
                                        continue;
                                    }

                                }

                            }

                            {
                                var (_, ok) = s.varnum[var_];

                                if (ok)
                                {
                                    continue;
                                }

                            }

                            s.varnum[var_] = int32(len(vartypes));
                            if (debugPhi)
                            {
                                fmt.Printf("var%d = %v\n", len(vartypes), var_);
                            }

                            vars = append(vars, var_);
                            vartypes = append(vartypes, v.Type);

                        }

                        v = v__prev2;
                    }
                }

                b = b__prev1;
            }

            if (len(vartypes) == 0L)
            {
                return ;
            } 

            // Find all definitions of the variables we need to process.
            // defs[n] contains all the blocks in which variable number n is assigned.
            var defs = make_slice<slice<ptr<ssa.Block>>>(len(vartypes));
            {
                var b__prev1 = b;

                foreach (var (_, __b) in s.f.Blocks)
                {
                    b = __b;
                    {
                        ptr<Node> var___prev2 = var_;

                        foreach (var (__var_) in s.defvars[b.ID])
                        {
                            var_ = __var_; // TODO: encode defvars some other way (explicit ops)? make defvars[n] a slice instead of a map.
                            {
                                var n__prev1 = n;

                                var (n, ok) = s.varnum[var_];

                                if (ok)
                                {
                                    defs[n] = append(defs[n], b);
                                }

                                n = n__prev1;

                            }

                        }

                        var_ = var___prev2;
                    }
                } 

                // Make dominator tree.

                b = b__prev1;
            }

            s.idom = s.f.Idom();
            s.tree = make_slice<domBlock>(s.f.NumBlocks());
            {
                var b__prev1 = b;

                foreach (var (_, __b) in s.f.Blocks)
                {
                    b = __b;
                    var p = s.idom[b.ID];
                    if (p != null)
                    {
                        s.tree[b.ID].sibling = s.tree[p.ID].firstChild;
                        s.tree[p.ID].firstChild = b;
                    }

                } 
                // Compute levels in dominator tree.
                // With parent pointers we can do a depth-first walk without
                // any auxiliary storage.

                b = b__prev1;
            }

            s.level = make_slice<int>(s.f.NumBlocks());
            var b = s.f.Entry;
levels: 

            // Allocate scratch locations.
            while (true)
            {
                {
                    var p__prev1 = p;

                    p = s.idom[b.ID];

                    if (p != null)
                    {
                        s.level[b.ID] = s.level[p.ID] + 1L;
                        if (debugPhi)
                        {
                            fmt.Printf("level %s = %d\n", b, s.level[b.ID]);
                        }

                    }

                    p = p__prev1;

                }

                {
                    var c__prev1 = c;

                    c = s.tree[b.ID].firstChild;

                    if (c != null)
                    {
                        b = c;
                        continue;
                    }

                    c = c__prev1;

                }

                while (true)
                {
                    {
                        var c__prev1 = c;

                        c = s.tree[b.ID].sibling;

                        if (c != null)
                        {
                            b = c;
                            _continuelevels = true;
                            break;
                        }

                        c = c__prev1;

                    }

                    b = s.idom[b.ID];
                    if (b == null)
                    {
                        _breaklevels = true;
                        break;
                    }

                }


            } 

            // Allocate scratch locations.
 

            // Allocate scratch locations.
            s.priq.level = s.level;
            s.q = make_slice<ptr<ssa.Block>>(0L, s.f.NumBlocks());
            s.queued = newSparseSet(s.f.NumBlocks());
            s.hasPhi = newSparseSet(s.f.NumBlocks());
            s.hasDef = newSparseSet(s.f.NumBlocks());
            s.placeholder = s.s.entryNewValue0(ssa.OpUnknown, types.TypeInvalid); 

            // Generate phi ops for each variable.
            {
                var n__prev1 = n;

                foreach (var (__n) in vartypes)
                {
                    n = __n;
                    s.insertVarPhis(n, vars[n], defs[n], vartypes[n]);
                } 

                // Resolve FwdRefs to the correct write or phi.

                n = n__prev1;
            }

            s.resolveFwdRefs(); 

            // Erase variable numbers stored in AuxInt fields of phi ops. They are no longer needed.
            {
                var b__prev1 = b;

                foreach (var (_, __b) in s.f.Blocks)
                {
                    b = __b;
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in b.Values)
                        {
                            v = __v;
                            if (v.Op == ssa.OpPhi)
                            {
                                v.AuxInt = 0L;
                            }

                        }

                        v = v__prev2;
                    }
                }

                b = b__prev1;
            }
        }

        private static void insertVarPhis(this ptr<phiState> _addr_s, long n, ptr<Node> _addr_var_, slice<ptr<ssa.Block>> defs, ptr<types.Type> _addr_typ)
        {
            ref phiState s = ref _addr_s.val;
            ref Node var_ = ref _addr_var_.val;
            ref types.Type typ = ref _addr_typ.val;

            var priq = _addr_s.priq;
            var q = s.q;
            var queued = s.queued;
            queued.clear();
            var hasPhi = s.hasPhi;
            hasPhi.clear();
            var hasDef = s.hasDef;
            hasDef.clear(); 

            // Add defining blocks to priority queue.
            {
                var b__prev1 = b;

                foreach (var (_, __b) in defs)
                {
                    b = __b;
                    priq.a = append(priq.a, b);
                    hasDef.add(b.ID);
                    if (debugPhi)
                    {
                        fmt.Printf("def of var%d in %s\n", n, b);
                    }

                }

                b = b__prev1;
            }

            heap.Init(priq); 

            // Visit blocks defining variable n, from deepest to shallowest.
            while (len(priq.a) > 0L)
            {
                ptr<ssa.Block> currentRoot = heap.Pop(priq)._<ptr<ssa.Block>>();
                if (debugPhi)
                {
                    fmt.Printf("currentRoot %s\n", currentRoot);
                } 
                // Walk subtree below definition.
                // Skip subtrees we've done in previous iterations.
                // Find edges exiting tree dominated by definition (the dominance frontier).
                // Insert phis at target blocks.
                if (queued.contains(currentRoot.ID))
                {
                    s.s.Fatalf("root already in queue");
                }

                q = append(q, currentRoot);
                queued.add(currentRoot.ID);
                while (len(q) > 0L)
                {
                    var b = q[len(q) - 1L];
                    q = q[..len(q) - 1L];
                    if (debugPhi)
                    {
                        fmt.Printf("  processing %s\n", b);
                    }

                    var currentRootLevel = s.level[currentRoot.ID];
                    foreach (var (_, e) in b.Succs)
                    {
                        var c = e.Block(); 
                        // TODO: if the variable is dead at c, skip it.
                        if (s.level[c.ID] > currentRootLevel)
                        { 
                            // a D-edge, or an edge whose target is in currentRoot's subtree.
                            continue;

                        }

                        if (hasPhi.contains(c.ID))
                        {
                            continue;
                        } 
                        // Add a phi to block c for variable n.
                        hasPhi.add(c.ID);
                        var v = c.NewValue0I(currentRoot.Pos, ssa.OpPhi, typ, int64(n)); // TODO: line number right?
                        // Note: we store the variable number in the phi's AuxInt field. Used temporarily by phi building.
                        s.s.addNamedValue(var_, v);
                        foreach (>>MARKER:FORRANGEEXPRESSIONS_LEVEL_4<< in c.Preds)
                        {>>MARKER:FORRANGEMUTABLEEXPRESSIONS_LEVEL_4<<
                            v.AddArg(s.placeholder); // Actual args will be filled in by resolveFwdRefs.
                        }
                        if (debugPhi)
                        {
                            fmt.Printf("new phi for var%d in %s: %s\n", n, c, v);
                        }

                        if (!hasDef.contains(c.ID))
                        { 
                            // There's now a new definition of this variable in block c.
                            // Add it to the priority queue to explore.
                            heap.Push(priq, c);
                            hasDef.add(c.ID);

                        }

                    } 

                    // Visit children if they have not been visited yet.
                    {
                        var c__prev3 = c;

                        c = s.tree[b.ID].firstChild;

                        while (c != null)
                        {
                            if (!queued.contains(c.ID))
                            {
                                q = append(q, c);
                                queued.add(c.ID);
                            c = s.tree[c.ID].sibling;
                            }

                        }


                        c = c__prev3;
                    }

                }


            }


        }

        // resolveFwdRefs links all FwdRef uses up to their nearest dominating definition.
        private static void resolveFwdRefs(this ptr<phiState> _addr_s)
        {
            ref phiState s = ref _addr_s.val;
 
            // Do a depth-first walk of the dominator tree, keeping track
            // of the most-recently-seen value for each variable.

            // Map from variable ID to SSA value at the current point of the walk.
            var values = make_slice<ptr<ssa.Value>>(len(s.varnum));
            {
                var i__prev1 = i;

                foreach (var (__i) in values)
                {
                    i = __i;
                    values[i] = s.placeholder;
                } 

                // Stack of work to do.

                i = i__prev1;
            }

            private partial struct stackEntry
            {
                public ptr<ssa.Block> b; // block to explore

// variable/value pair to reinstate on exit
                public int n; // variable ID
                public ptr<ssa.Value> v; // Note: only one of b or n,v will be set.
            }
            slice<stackEntry> stk = default;

            stk = append(stk, new stackEntry(b:s.f.Entry));
            while (len(stk) > 0L)
            {
                var work = stk[len(stk) - 1L];
                stk = stk[..len(stk) - 1L];

                var b = work.b;
                if (b == null)
                { 
                    // On exit from a block, this case will undo any assignments done below.
                    values[work.n] = work.v;
                    continue;

                } 

                // Process phis as new defs. They come before FwdRefs in this block.
                {
                    var v__prev2 = v;

                    foreach (var (_, __v) in b.Values)
                    {
                        v = __v;
                        if (v.Op != ssa.OpPhi)
                        {
                            continue;
                        }

                        var n = int32(v.AuxInt); 
                        // Remember the old assignment so we can undo it when we exit b.
                        stk = append(stk, new stackEntry(n:n,v:values[n])); 
                        // Record the new assignment.
                        values[n] = v;

                    } 

                    // Replace a FwdRef op with the current incoming value for its variable.

                    v = v__prev2;
                }

                {
                    var v__prev2 = v;

                    foreach (var (_, __v) in b.Values)
                    {
                        v = __v;
                        if (v.Op != ssa.OpFwdRef)
                        {
                            continue;
                        }

                        n = s.varnum[v.Aux._<ptr<Node>>()];
                        v.Op = ssa.OpCopy;
                        v.Aux = null;
                        v.AddArg(values[n]);

                    } 

                    // Establish values for variables defined in b.

                    v = v__prev2;
                }

                {
                    var v__prev2 = v;

                    foreach (var (__var_, __v) in s.defvars[b.ID])
                    {
                        var_ = __var_;
                        v = __v;
                        var (n, ok) = s.varnum[var_];
                        if (!ok)
                        { 
                            // some variable not live across a basic block boundary.
                            continue;

                        } 
                        // Remember the old assignment so we can undo it when we exit b.
                        stk = append(stk, new stackEntry(n:n,v:values[n])); 
                        // Record the new assignment.
                        values[n] = v;

                    } 

                    // Replace phi args in successors with the current incoming value.

                    v = v__prev2;
                }

                foreach (var (_, e) in b.Succs)
                {
                    var c = e.Block();
                    var i = e.Index();
                    for (var j = len(c.Values) - 1L; j >= 0L; j--)
                    {
                        var v = c.Values[j];
                        if (v.Op != ssa.OpPhi)
                        {
                            break; // All phis will be at the end of the block during phi building.
                        } 
                        // Only set arguments that have been resolved.
                        // For very wide CFGs, this significantly speeds up phi resolution.
                        // See golang.org/issue/8225.
                        {
                            var w = values[v.AuxInt];

                            if (w.Op != ssa.OpUnknown)
                            {
                                v.SetArg(i, w);
                            }

                        }

                    }


                } 

                // Walk children in dominator tree.
                {
                    var c__prev2 = c;

                    c = s.tree[b.ID].firstChild;

                    while (c != null)
                    {
                        stk = append(stk, new stackEntry(b:c));
                        c = s.tree[c.ID].sibling;
                    }


                    c = c__prev2;
                }

            }


        }

        // domBlock contains extra per-block information to record the dominator tree.
        private partial struct domBlock
        {
            public ptr<ssa.Block> firstChild; // first child of block in dominator tree
            public ptr<ssa.Block> sibling; // next child of parent in dominator tree
        }

        // A block heap is used as a priority queue to implement the PiggyBank
        // from Sreedhar and Gao.  That paper uses an array which is better
        // asymptotically but worse in the common case when the PiggyBank
        // holds a sparse set of blocks.
        private partial struct blockHeap
        {
            public slice<ptr<ssa.Block>> a; // block IDs in heap
            public slice<int> level; // depth in dominator tree (static, used for determining priority)
        }

        private static long Len(this ptr<blockHeap> _addr_h)
        {
            ref blockHeap h = ref _addr_h.val;

            return len(h.a);
        }
        private static void Swap(this ptr<blockHeap> _addr_h, long i, long j)
        {
            ref blockHeap h = ref _addr_h.val;

            var a = h.a;

            a[i] = a[j];
            a[j] = a[i];
        }

        private static void Push(this ptr<blockHeap> _addr_h, object x)
        {
            ref blockHeap h = ref _addr_h.val;

            ptr<ssa.Block> v = x._<ptr<ssa.Block>>();
            h.a = append(h.a, v);
        }
        private static void Pop(this ptr<blockHeap> _addr_h)
        {
            ref blockHeap h = ref _addr_h.val;

            var old = h.a;
            var n = len(old);
            var x = old[n - 1L];
            h.a = old[..n - 1L];
            return x;
        }
        private static bool Less(this ptr<blockHeap> _addr_h, long i, long j)
        {
            ref blockHeap h = ref _addr_h.val;

            return h.level[h.a[i].ID] > h.level[h.a[j].ID];
        }

        // TODO: stop walking the iterated domininance frontier when
        // the variable is dead. Maybe detect that by checking if the
        // node we're on is reverse dominated by all the reads?
        // Reverse dominated by the highest common successor of all the reads?

        // copy of ../ssa/sparseset.go
        // TODO: move this file to ../ssa, then use sparseSet there.
        private partial struct sparseSet
        {
            public slice<ssa.ID> dense;
            public slice<int> sparse;
        }

        // newSparseSet returns a sparseSet that can represent
        // integers between 0 and n-1
        private static ptr<sparseSet> newSparseSet(long n)
        {
            return addr(new sparseSet(dense:nil,sparse:make([]int32,n)));
        }

        private static bool contains(this ptr<sparseSet> _addr_s, ssa.ID x)
        {
            ref sparseSet s = ref _addr_s.val;

            var i = s.sparse[x];
            return i < int32(len(s.dense)) && s.dense[i] == x;
        }

        private static void add(this ptr<sparseSet> _addr_s, ssa.ID x)
        {
            ref sparseSet s = ref _addr_s.val;

            var i = s.sparse[x];
            if (i < int32(len(s.dense)) && s.dense[i] == x)
            {
                return ;
            }

            s.dense = append(s.dense, x);
            s.sparse[x] = int32(len(s.dense)) - 1L;

        }

        private static void clear(this ptr<sparseSet> _addr_s)
        {
            ref sparseSet s = ref _addr_s.val;

            s.dense = s.dense[..0L];
        }

        // Variant to use for small functions.
        private partial struct simplePhiState
        {
            public ptr<state> s; // SSA state
            public ptr<ssa.Func> f; // function to work on
            public slice<ptr<ssa.Value>> fwdrefs; // list of FwdRefs to be processed
            public slice<map<ptr<Node>, ptr<ssa.Value>>> defvars; // defined variables at end of each block
            public slice<bool> reachable; // which blocks are reachable
        }

        private static void insertPhis(this ptr<simplePhiState> _addr_s)
        {
            ref simplePhiState s = ref _addr_s.val;

            s.reachable = ssa.ReachableBlocks(s.f); 

            // Find FwdRef ops.
            {
                var b__prev1 = b;

                foreach (var (_, __b) in s.f.Blocks)
                {
                    b = __b;
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in b.Values)
                        {
                            v = __v;
                            if (v.Op != ssa.OpFwdRef)
                            {
                                continue;
                            }

                            s.fwdrefs = append(s.fwdrefs, v);
                            ptr<Node> var_ = v.Aux._<ptr<Node>>();
                            {
                                var (_, ok) = s.defvars[b.ID][var_];

                                if (!ok)
                                {
                                    s.defvars[b.ID][var_] = v; // treat FwdDefs as definitions.
                                }

                            }

                        }

                        v = v__prev2;
                    }
                }

                b = b__prev1;
            }

            slice<ptr<ssa.Value>> args = default;

loop:
            while (len(s.fwdrefs) > 0L)
            {
                var v = s.fwdrefs[len(s.fwdrefs) - 1L];
                s.fwdrefs = s.fwdrefs[..len(s.fwdrefs) - 1L];
                var b = v.Block;
                var_ = v.Aux._<ptr<Node>>();
                if (b == s.f.Entry)
                { 
                    // No variable should be live at entry.
                    s.s.Fatalf("Value live at entry. It shouldn't be. func %s, node %v, value %v", s.f.Name, var_, v);

                }

                if (!s.reachable[b.ID])
                { 
                    // This block is dead.
                    // It doesn't matter what we use here as long as it is well-formed.
                    v.Op = ssa.OpUnknown;
                    v.Aux = null;
                    continue;

                } 
                // Find variable value on each predecessor.
                args = args[..0L];
                foreach (var (_, e) in b.Preds)
                {
                    args = append(args, s.lookupVarOutgoing(e.Block(), v.Type, var_, v.Pos));
                } 

                // Decide if we need a phi or not. We need a phi if there
                // are two different args (which are both not v).
                ptr<ssa.Value> w;
                foreach (var (_, a) in args)
                {
                    if (a == v)
                    {
                        continue; // self-reference
                    }

                    if (a == w)
                    {
                        continue; // already have this witness
                    }

                    if (w != null)
                    { 
                        // two witnesses, need a phi value
                        v.Op = ssa.OpPhi;
                        v.AddArgs(args);
                        v.Aux = null;
                        _continueloop = true;
                        break;
                    }

                    w = a; // save witness
                }
                if (w == null)
                {
                    s.s.Fatalf("no witness for reachable phi %s", v);
                } 
                // One witness. Make v a copy of w.
                v.Op = ssa.OpCopy;
                v.Aux = null;
                v.AddArg(w);

            }

        }

        // lookupVarOutgoing finds the variable's value at the end of block b.
        private static ptr<ssa.Value> lookupVarOutgoing(this ptr<simplePhiState> _addr_s, ptr<ssa.Block> _addr_b, ptr<types.Type> _addr_t, ptr<Node> _addr_var_, src.XPos line)
        {
            ref simplePhiState s = ref _addr_s.val;
            ref ssa.Block b = ref _addr_b.val;
            ref types.Type t = ref _addr_t.val;
            ref Node var_ = ref _addr_var_.val;

            while (true)
            {
                {
                    var v__prev1 = v;

                    var v = s.defvars[b.ID][var_];

                    if (v != null)
                    {
                        return _addr_v!;
                    } 
                    // The variable is not defined by b and we haven't looked it up yet.
                    // If b has exactly one predecessor, loop to look it up there.
                    // Otherwise, give up and insert a new FwdRef and resolve it later.

                    v = v__prev1;

                } 
                // The variable is not defined by b and we haven't looked it up yet.
                // If b has exactly one predecessor, loop to look it up there.
                // Otherwise, give up and insert a new FwdRef and resolve it later.
                if (len(b.Preds) != 1L)
                {
                    break;
                }

                b = b.Preds[0L].Block();
                if (!s.reachable[b.ID])
                { 
                    // This is rare; it happens with oddly interleaved infinite loops in dead code.
                    // See issue 19783.
                    break;

                }

            } 
            // Generate a FwdRef for the variable and return that.
 
            // Generate a FwdRef for the variable and return that.
            v = b.NewValue0A(line, ssa.OpFwdRef, t, var_);
            s.defvars[b.ID][var_] = v;
            s.s.addNamedValue(var_, v);
            s.fwdrefs = append(s.fwdrefs, v);
            return _addr_v!;

        }
    }
}}}}
