// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 August 29 08:54:46 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\prove.go
using fmt = go.fmt_package;
using math = go.math_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        private partial struct branch // : long
        {
        }

        private static readonly var unknown = iota;
        private static readonly var positive = 0;
        private static readonly var negative = 1;

        // relation represents the set of possible relations between
        // pairs of variables (v, w). Without a priori knowledge the
        // mask is lt | eq | gt meaning v can be less than, equal to or
        // greater than w. When the execution path branches on the condition
        // `v op w` the set of relations is updated to exclude any
        // relation not possible due to `v op w` being true (or false).
        //
        // E.g.
        //
        // r := relation(...)
        //
        // if v < w {
        //   newR := r & lt
        // }
        // if v >= w {
        //   newR := r & (eq|gt)
        // }
        // if v != w {
        //   newR := r & (lt|gt)
        // }
        private partial struct relation // : ulong
        {
        }

        private static readonly relation lt = 1L << (int)(iota);
        private static readonly var eq = 0;
        private static readonly var gt = 1;

        // domain represents the domain of a variable pair in which a set
        // of relations is known.  For example, relations learned for unsigned
        // pairs cannot be transferred to signed pairs because the same bit
        // representation can mean something else.
        private partial struct domain // : ulong
        {
        }

        private static readonly domain signed = 1L << (int)(iota);
        private static readonly var unsigned = 0;
        private static readonly var pointer = 1;
        private static readonly var boolean = 2;

        private partial struct pair
        {
            public ptr<Value> v; // a pair of values, ordered by ID.
// v can be nil, to mean the zero value.
// for booleans the zero value (v == nil) is false.
            public ptr<Value> w; // a pair of values, ordered by ID.
// v can be nil, to mean the zero value.
// for booleans the zero value (v == nil) is false.
            public domain d;
        }

        // fact is a pair plus a relation for that pair.
        private partial struct fact
        {
            public pair p;
            public relation r;
        }

        // a limit records known upper and lower bounds for a value.
        private partial struct limit
        {
            public long min; // min <= value <= max, signed
            public long max; // min <= value <= max, signed
            public ulong umin; // umin <= value <= umax, unsigned
            public ulong umax; // umin <= value <= umax, unsigned
        }

        private static @string String(this limit l)
        {
            return fmt.Sprintf("sm,SM,um,UM=%d,%d,%d,%d", l.min, l.max, l.umin, l.umax);
        }

        private static limit noLimit = new limit(math.MinInt64,math.MaxInt64,0,math.MaxUint64);

        // a limitFact is a limit known for a particular value.
        private partial struct limitFact
        {
            public ID vid;
            public limit limit;
        }

        // factsTable keeps track of relations between pairs of values.
        private partial struct factsTable
        {
            public map<pair, relation> facts; // current known set of relation
            public slice<fact> stack; // previous sets of relations

// known lower and upper bounds on individual values.
            public map<ID, limit> limits;
            public slice<limitFact> limitStack; // previous entries

// For each slice s, a map from s to a len(s)/cap(s) value (if any)
// TODO: check if there are cases that matter where we have
// more than one len(s) for a slice. We could keep a list if necessary.
            public map<ID, ref Value> lens;
            public map<ID, ref Value> caps;
        }

        // checkpointFact is an invalid value used for checkpointing
        // and restoring factsTable.
        private static fact checkpointFact = new fact();
        private static limitFact checkpointBound = new limitFact();

        private static ref factsTable newFactsTable()
        {
            factsTable ft = ref new factsTable();
            ft.facts = make_map<pair, relation>();
            ft.stack = make_slice<fact>(4L);
            ft.limits = make_map<ID, limit>();
            ft.limitStack = make_slice<limitFact>(4L);
            return ft;
        }

        // get returns the known possible relations between v and w.
        // If v and w are not in the map it returns lt|eq|gt, i.e. any order.
        private static relation get(this ref factsTable ft, ref Value v, ref Value w, domain d)
        {
            if (v.isGenericIntConst() || w.isGenericIntConst())
            {
                var reversed = false;
                if (v.isGenericIntConst())
                {
                    v = w;
                    w = v;
                    reversed = true;
                }
                var r = lt | eq | gt;
                var (lim, ok) = ft.limits[v.ID];
                if (!ok)
                {
                    return r;
                }
                var c = w.AuxInt;

                if (d == signed) 

                    if (c < lim.min) 
                        r = gt;
                    else if (c > lim.max) 
                        r = lt;
                    else if (c == lim.min && c == lim.max) 
                        r = eq;
                    else if (c == lim.min) 
                        r = gt | eq;
                    else if (c == lim.max) 
                        r = lt | eq;
                                    else if (d == unsigned) 
                    // TODO: also use signed data if lim.min >= 0?
                    ulong uc = default;

                    if (w.Op == OpConst64) 
                        uc = uint64(c);
                    else if (w.Op == OpConst32) 
                        uc = uint64(uint32(c));
                    else if (w.Op == OpConst16) 
                        uc = uint64(uint16(c));
                    else if (w.Op == OpConst8) 
                        uc = uint64(uint8(c));
                    
                    if (uc < lim.umin) 
                        r = gt;
                    else if (uc > lim.umax) 
                        r = lt;
                    else if (uc == lim.umin && uc == lim.umax) 
                        r = eq;
                    else if (uc == lim.umin) 
                        r = gt | eq;
                    else if (uc == lim.umax) 
                        r = lt | eq;
                                                    if (reversed)
                {
                    return reverseBits[r];
                }
                return r;
            }
            reversed = false;
            if (lessByID(w, v))
            {
                v = w;
                w = v;
                reversed = !reversed;
            }
            pair p = new pair(v,w,d);
            var (r, ok) = ft.facts[p];
            if (!ok)
            {
                if (p.v == p.w)
                {
                    r = eq;
                }
                else
                {
                    r = lt | eq | gt;
                }
            }
            if (reversed)
            {
                return reverseBits[r];
            }
            return r;
        }

        // update updates the set of relations between v and w in domain d
        // restricting it to r.
        private static void update(this ref factsTable ft, ref Block parent, ref Value v, ref Value w, domain d, relation r)
        {
            if (lessByID(w, v))
            {
                v = w;
                w = v;
                r = reverseBits[r];
            }
            pair p = new pair(v,w,d);
            var oldR = ft.get(v, w, d);
            ft.stack = append(ft.stack, new fact(p,oldR));
            ft.facts[p] = oldR & r; 

            // Extract bounds when comparing against constants
            if (v.isGenericIntConst())
            {
                v = w;
                w = v;
                r = reverseBits[r];
            }
            if (v != null && w.isGenericIntConst())
            {
                var c = w.AuxInt; 
                // Note: all the +1/-1 below could overflow/underflow. Either will
                // still generate correct results, it will just lead to imprecision.
                // In fact if there is overflow/underflow, the corresponding
                // code is unreachable because the known range is outside the range
                // of the value's type.
                var (old, ok) = ft.limits[v.ID];
                if (!ok)
                {
                    old = noLimit;
                }
                var lim = old; 
                // Update lim with the new information we know.

                if (d == signed) 

                    if (r == lt) 
                        if (c - 1L < lim.max)
                        {
                            lim.max = c - 1L;
                        }
                    else if (r == lt | eq) 
                        if (c < lim.max)
                        {
                            lim.max = c;
                        }
                    else if (r == gt | eq) 
                        if (c > lim.min)
                        {
                            lim.min = c;
                        }
                    else if (r == gt) 
                        if (c + 1L > lim.min)
                        {
                            lim.min = c + 1L;
                        }
                    else if (r == lt | gt) 
                        if (c == lim.min)
                        {
                            lim.min++;
                        }
                        if (c == lim.max)
                        {
                            lim.max--;
                        }
                    else if (r == eq) 
                        lim.min = c;
                        lim.max = c;
                                    else if (d == unsigned) 
                    ulong uc = default;

                    if (w.Op == OpConst64) 
                        uc = uint64(c);
                    else if (w.Op == OpConst32) 
                        uc = uint64(uint32(c));
                    else if (w.Op == OpConst16) 
                        uc = uint64(uint16(c));
                    else if (w.Op == OpConst8) 
                        uc = uint64(uint8(c));
                    
                    if (r == lt) 
                        if (uc - 1L < lim.umax)
                        {
                            lim.umax = uc - 1L;
                        }
                    else if (r == lt | eq) 
                        if (uc < lim.umax)
                        {
                            lim.umax = uc;
                        }
                    else if (r == gt | eq) 
                        if (uc > lim.umin)
                        {
                            lim.umin = uc;
                        }
                    else if (r == gt) 
                        if (uc + 1L > lim.umin)
                        {
                            lim.umin = uc + 1L;
                        }
                    else if (r == lt | gt) 
                        if (uc == lim.umin)
                        {
                            lim.umin++;
                        }
                        if (uc == lim.umax)
                        {
                            lim.umax--;
                        }
                    else if (r == eq) 
                        lim.umin = uc;
                        lim.umax = uc;
                                                    ft.limitStack = append(ft.limitStack, new limitFact(v.ID,old));
                ft.limits[v.ID] = lim;
                if (v.Block.Func.pass.debug > 2L)
                {
                    v.Block.Func.Warnl(parent.Pos, "parent=%s, new limits %s %s %s", parent, v, w, lim.String());
                }
            }
        }

        // isNonNegative returns true if v is known to be non-negative.
        private static bool isNonNegative(this ref factsTable ft, ref Value v)
        {
            if (isNonNegative(v))
            {
                return true;
            }
            var (l, has) = ft.limits[v.ID];
            return has && (l.min >= 0L || l.umax <= math.MaxInt64);
        }

        // checkpoint saves the current state of known relations.
        // Called when descending on a branch.
        private static void checkpoint(this ref factsTable ft)
        {
            ft.stack = append(ft.stack, checkpointFact);
            ft.limitStack = append(ft.limitStack, checkpointBound);
        }

        // restore restores known relation to the state just
        // before the previous checkpoint.
        // Called when backing up on a branch.
        private static void restore(this ref factsTable ft)
        {
            while (true)
            {
                var old = ft.stack[len(ft.stack) - 1L];
                ft.stack = ft.stack[..len(ft.stack) - 1L];
                if (old == checkpointFact)
                {
                    break;
                }
                if (old.r == lt | eq | gt)
                {
                    delete(ft.facts, old.p);
                }
                else
                {
                    ft.facts[old.p] = old.r;
                }
            }

            while (true)
            {
                old = ft.limitStack[len(ft.limitStack) - 1L];
                ft.limitStack = ft.limitStack[..len(ft.limitStack) - 1L];
                if (old.vid == 0L)
                { // checkpointBound
                    break;
                }
                if (old.limit == noLimit)
                {
                    delete(ft.limits, old.vid);
                }
                else
                {
                    ft.limits[old.vid] = old.limit;
                }
            }

        }

        private static bool lessByID(ref Value v, ref Value w)
        {
            if (v == null && w == null)
            { 
                // Should not happen, but just in case.
                return false;
            }
            if (v == null)
            {
                return true;
            }
            return w != null && v.ID < w.ID;
        }

        private static array<relation> reverseBits = new array<relation>(new relation[] { 0, 4, 2, 6, 1, 5, 3, 7 });

        // prove removes redundant BlockIf branches that can be inferred
        // from previous dominating comparisons.
        //
        // By far, the most common redundant pair are generated by bounds checking.
        // For example for the code:
        //
        //    a[i] = 4
        //    foo(a[i])
        //
        // The compiler will generate the following code:
        //
        //    if i >= len(a) {
        //        panic("not in bounds")
        //    }
        //    a[i] = 4
        //    if i >= len(a) {
        //        panic("not in bounds")
        //    }
        //    foo(a[i])
        //
        // The second comparison i >= len(a) is clearly redundant because if the
        // else branch of the first comparison is executed, we already know that i < len(a).
        // The code for the second panic can be removed.
        private static void prove(ref Func f)
        {
            var ft = newFactsTable(); 

            // Find length and capacity ops.
            {
                var b__prev1 = b;

                foreach (var (_, __b) in f.Blocks)
                {
                    b = __b;
                    foreach (var (_, v) in b.Values)
                    {
                        if (v.Uses == 0L)
                        { 
                            // We don't care about dead values.
                            // (There can be some that are CSEd but not removed yet.)
                            continue;
                        }

                        if (v.Op == OpSliceLen) 
                            if (ft.lens == null)
                            {
                                ft.lens = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ID, ref Value>{};
                            }
                            ft.lens[v.Args[0L].ID] = v;
                        else if (v.Op == OpSliceCap) 
                            if (ft.caps == null)
                            {
                                ft.caps = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ID, ref Value>{};
                            }
                            ft.caps[v.Args[0L].ID] = v;
                                            }
                } 

                // current node state

                b = b__prev1;
            }

            private partial struct walkState // : long
            {
            }
            const walkState descend = iota;
            const var simplify = 0; 
            // work maintains the DFS stack.
            private partial struct bp
            {
                public ptr<Block> block; // current handled block
                public walkState state; // what's to do
            }
            var work = make_slice<bp>(0L, 256L);
            work = append(work, new bp(block:f.Entry,state:descend,));

            var idom = f.Idom();
            var sdom = f.sdom(); 

            // DFS on the dominator tree.
            while (len(work) > 0L)
            {
                var node = work[len(work) - 1L];
                work = work[..len(work) - 1L];
                var parent = idom[node.block.ID];
                var branch = getBranch(sdom, parent, node.block);


                if (node.state == descend) 
                    if (branch != unknown)
                    {
                        ft.checkpoint();
                        var c = parent.Control;
                        updateRestrictions(parent, ft, boolean, null, c, lt | gt, branch);
                        {
                            var (tr, has) = domainRelationTable[parent.Control.Op];

                            if (has)
                            { 
                                // When we branched from parent we learned a new set of
                                // restrictions. Update the factsTable accordingly.
                                updateRestrictions(parent, ft, tr.d, c.Args[0L], c.Args[1L], tr.r, branch);
                            }

                        }
                    }
                    work = append(work, new bp(block:node.block,state:simplify,));
                    {
                        var s = sdom.Child(node.block);

                        while (s != null)
                        {
                            work = append(work, new bp(block:s,state:descend,));
                            s = sdom.Sibling(s);
                        }

                    }
                else if (node.state == simplify) 
                    var succ = simplifyBlock(ft, node.block);
                    if (succ != unknown)
                    {
                        var b = node.block;
                        b.Kind = BlockFirst;
                        b.SetControl(null);
                        if (succ == negative)
                        {
                            b.swapSuccessors();
                        }
                    }
                    if (branch != unknown)
                    {
                        ft.restore();
                    }
                            }

        }

        // getBranch returns the range restrictions added by p
        // when reaching b. p is the immediate dominator of b.
        private static branch getBranch(SparseTree sdom, ref Block p, ref Block b)
        {
            if (p == null || p.Kind != BlockIf)
            {
                return unknown;
            } 
            // If p and p.Succs[0] are dominators it means that every path
            // from entry to b passes through p and p.Succs[0]. We care that
            // no path from entry to b passes through p.Succs[1]. If p.Succs[0]
            // has one predecessor then (apart from the degenerate case),
            // there is no path from entry that can reach b through p.Succs[1].
            // TODO: how about p->yes->b->yes, i.e. a loop in yes.
            if (sdom.isAncestorEq(p.Succs[0L].b, b) && len(p.Succs[0L].b.Preds) == 1L)
            {
                return positive;
            }
            if (sdom.isAncestorEq(p.Succs[1L].b, b) && len(p.Succs[1L].b.Preds) == 1L)
            {
                return negative;
            }
            return unknown;
        }

        // updateRestrictions updates restrictions from the immediate
        // dominating block (p) using r. r is adjusted according to the branch taken.
        private static void updateRestrictions(ref Block parent, ref factsTable ft, domain t, ref Value v, ref Value w, relation r, branch branch)
        {
            if (t == 0L || branch == unknown)
            { 
                // Trivial case: nothing to do, or branch unknown.
                // Shoult not happen, but just in case.
                return;
            }
            if (branch == negative)
            { 
                // Negative branch taken, complement the relations.
                r = (lt | eq | gt) ^ r;
            }
            {
                var i = domain(1L);

                while (i <= t)
                {
                    if (t & i == 0L)
                    {
                        continue;
                    i <<= 1L;
                    }
                    ft.update(parent, v, w, i, r); 

                    // Additional facts we know given the relationship between len and cap.
                    if (i != signed && i != unsigned)
                    {
                        continue;
                    }
                    if (v.Op == OpSliceLen && r & lt == 0L && ft.caps[v.Args[0L].ID] != null)
                    { 
                        // len(s) > w implies cap(s) > w
                        // len(s) >= w implies cap(s) >= w
                        // len(s) == w implies cap(s) >= w
                        ft.update(parent, ft.caps[v.Args[0L].ID], w, i, r | gt);
                    }
                    if (w.Op == OpSliceLen && r & gt == 0L && ft.caps[w.Args[0L].ID] != null)
                    { 
                        // same, length on the RHS.
                        ft.update(parent, v, ft.caps[w.Args[0L].ID], i, r | lt);
                    }
                    if (v.Op == OpSliceCap && r & gt == 0L && ft.lens[v.Args[0L].ID] != null)
                    { 
                        // cap(s) < w implies len(s) < w
                        // cap(s) <= w implies len(s) <= w
                        // cap(s) == w implies len(s) <= w
                        ft.update(parent, ft.lens[v.Args[0L].ID], w, i, r | lt);
                    }
                    if (w.Op == OpSliceCap && r & lt == 0L && ft.lens[w.Args[0L].ID] != null)
                    { 
                        // same, capacity on the RHS.
                        ft.update(parent, v, ft.lens[w.Args[0L].ID], i, r | gt);
                    }
                }

            }
        }

        // simplifyBlock simplifies block known the restrictions in ft.
        // Returns which branch must always be taken.
        private static branch simplifyBlock(ref factsTable ft, ref Block b)
        {
            foreach (var (_, v) in b.Values)
            {
                if (v.Op != OpSlicemask)
                {
                    continue;
                }
                var add = v.Args[0L];
                if (add.Op != OpAdd64 && add.Op != OpAdd32)
                {
                    continue;
                } 
                // Note that the arg of slicemask was originally a sub, but
                // was rewritten to an add by generic.rules (if the thing
                // being subtracted was a constant).
                var x = add.Args[0L];
                var y = add.Args[1L];
                if (x.Op == OpConst64 || x.Op == OpConst32)
                {
                    x = y;
                    y = x;
                }
                if (y.Op != OpConst64 && y.Op != OpConst32)
                {
                    continue;
                } 
                // slicemask(x + y)
                // if x is larger than -y (y is negative), then slicemask is -1.
                var (lim, ok) = ft.limits[x.ID];
                if (!ok)
                {
                    continue;
                }
                if (lim.umin > uint64(-y.AuxInt))
                {
                    if (v.Args[0L].Op == OpAdd64)
                    {
                        v.reset(OpConst64);
                    }
                    else
                    {
                        v.reset(OpConst32);
                    }
                    if (b.Func.pass.debug > 0L)
                    {
                        b.Func.Warnl(v.Pos, "Proved slicemask not needed");
                    }
                    v.AuxInt = -1L;
                }
            }
            if (b.Kind != BlockIf)
            {
                return unknown;
            } 

            // First, checks if the condition itself is redundant.
            var m = ft.get(null, b.Control, boolean);
            if (m == lt | gt)
            {
                if (b.Func.pass.debug > 0L)
                {
                    if (b.Func.pass.debug > 1L)
                    {
                        b.Func.Warnl(b.Pos, "Proved boolean %s (%s)", b.Control.Op, b.Control);
                    }
                    else
                    {
                        b.Func.Warnl(b.Pos, "Proved boolean %s", b.Control.Op);
                    }
                }
                return positive;
            }
            if (m == eq)
            {
                if (b.Func.pass.debug > 0L)
                {
                    if (b.Func.pass.debug > 1L)
                    {
                        b.Func.Warnl(b.Pos, "Disproved boolean %s (%s)", b.Control.Op, b.Control);
                    }
                    else
                    {
                        b.Func.Warnl(b.Pos, "Disproved boolean %s", b.Control.Op);
                    }
                }
                return negative;
            } 

            // Next look check equalities.
            var c = b.Control;
            var (tr, has) = domainRelationTable[c.Op];
            if (!has)
            {
                return unknown;
            }
            var a0 = c.Args[0L];
            var a1 = c.Args[1L];
            {
                var d = domain(1L);

                while (d <= tr.d)
                {
                    if (d & tr.d == 0L)
                    {
                        continue;
                    d <<= 1L;
                    } 

                    // tr.r represents in which case the positive branch is taken.
                    // m represents which cases are possible because of previous relations.
                    // If the set of possible relations m is included in the set of relations
                    // need to take the positive branch (or negative) then that branch will
                    // always be taken.
                    // For shortcut, if m == 0 then this block is dead code.
                    m = ft.get(a0, a1, d);
                    if (m != 0L && tr.r & m == m)
                    {
                        if (b.Func.pass.debug > 0L)
                        {
                            if (b.Func.pass.debug > 1L)
                            {
                                b.Func.Warnl(b.Pos, "Proved %s (%s)", c.Op, c);
                            }
                            else
                            {
                                b.Func.Warnl(b.Pos, "Proved %s", c.Op);
                            }
                        }
                        return positive;
                    }
                    if (m != 0L && ((lt | eq | gt) ^ tr.r) & m == m)
                    {
                        if (b.Func.pass.debug > 0L)
                        {
                            if (b.Func.pass.debug > 1L)
                            {
                                b.Func.Warnl(b.Pos, "Disproved %s (%s)", c.Op, c);
                            }
                            else
                            {
                                b.Func.Warnl(b.Pos, "Disproved %s", c.Op);
                            }
                        }
                        return negative;
                    }
                } 

                // HACK: If the first argument of IsInBounds or IsSliceInBounds
                // is a constant and we already know that constant is smaller (or equal)
                // to the upper bound than this is proven. Most useful in cases such as:
                // if len(a) <= 1 { return }
                // do something with a[1]

            } 

            // HACK: If the first argument of IsInBounds or IsSliceInBounds
            // is a constant and we already know that constant is smaller (or equal)
            // to the upper bound than this is proven. Most useful in cases such as:
            // if len(a) <= 1 { return }
            // do something with a[1]
            if ((c.Op == OpIsInBounds || c.Op == OpIsSliceInBounds) && ft.isNonNegative(c.Args[0L]))
            {
                m = ft.get(a0, a1, signed);
                if (m != 0L && tr.r & m == m)
                {
                    if (b.Func.pass.debug > 0L)
                    {
                        if (b.Func.pass.debug > 1L)
                        {
                            b.Func.Warnl(b.Pos, "Proved non-negative bounds %s (%s)", c.Op, c);
                        }
                        else
                        {
                            b.Func.Warnl(b.Pos, "Proved non-negative bounds %s", c.Op);
                        }
                    }
                    return positive;
                }
            }
            return unknown;
        }

        // isNonNegative returns true is v is known to be greater or equal to zero.
        private static bool isNonNegative(ref Value v)
        {

            if (v.Op == OpConst64) 
                return v.AuxInt >= 0L;
            else if (v.Op == OpConst32) 
                return int32(v.AuxInt) >= 0L;
            else if (v.Op == OpStringLen || v.Op == OpSliceLen || v.Op == OpSliceCap || v.Op == OpZeroExt8to64 || v.Op == OpZeroExt16to64 || v.Op == OpZeroExt32to64) 
                return true;
            else if (v.Op == OpRsh64x64) 
                return isNonNegative(v.Args[0L]);
                        return false;
        }
    }
}}}}
