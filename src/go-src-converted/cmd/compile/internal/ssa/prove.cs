// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 06 22:50:54 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\prove.go
using src = go.cmd.@internal.src_package;
using fmt = go.fmt_package;
using math = go.math_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class ssa_package {

private partial struct branch { // : nint
}

private static readonly branch unknown = iota;
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
private partial struct relation { // : nuint
}

private static readonly relation lt = 1 << (int)(iota);
private static readonly var eq = 0;
private static readonly var gt = 1;


private static array<@string> relationStrings = new array<@string>(InitKeyedValues<@string>((0, "none"), (lt, "<"), (eq, "=="), (lt|eq, "<="), (gt, ">"), (gt|lt, "!="), (gt|eq, ">="), (gt|eq|lt, "any")));

private static @string String(this relation r) {
    if (r < relation(len(relationStrings))) {
        return relationStrings[r];
    }
    return fmt.Sprintf("relation(%d)", uint(r));

}

// domain represents the domain of a variable pair in which a set
// of relations is known. For example, relations learned for unsigned
// pairs cannot be transferred to signed pairs because the same bit
// representation can mean something else.
private partial struct domain { // : nuint
}

private static readonly domain signed = 1 << (int)(iota);
private static readonly var unsigned = 0;
private static readonly var pointer = 1;
private static readonly var boolean = 2;


private static array<@string> domainStrings = new array<@string>(new @string[] { "signed", "unsigned", "pointer", "boolean" });

private static @string String(this domain d) {
    @string s = "";
    foreach (var (i, ds) in domainStrings) {
        if (d & (1 << (int)(uint(i))) != 0) {
            if (len(s) != 0) {
                s += "|";
            }
            s += ds;
            d &= 1 << (int)(uint(i));
        }
    }    if (d != 0) {
        if (len(s) != 0) {
            s += "|";
        }
        s += fmt.Sprintf("0x%x", uint(d));

    }
    return s;

}

private partial struct pair {
    public ptr<Value> v; // a pair of values, ordered by ID.
// v can be nil, to mean the zero value.
// for booleans the zero value (v == nil) is false.
    public ptr<Value> w; // a pair of values, ordered by ID.
// v can be nil, to mean the zero value.
// for booleans the zero value (v == nil) is false.
    public domain d;
}

// fact is a pair plus a relation for that pair.
private partial struct fact {
    public pair p;
    public relation r;
}

// a limit records known upper and lower bounds for a value.
private partial struct limit {
    public long min; // min <= value <= max, signed
    public long max; // min <= value <= max, signed
    public ulong umin; // umin <= value <= umax, unsigned
    public ulong umax; // umin <= value <= umax, unsigned
}

private static @string String(this limit l) {
    return fmt.Sprintf("sm,SM,um,UM=%d,%d,%d,%d", l.min, l.max, l.umin, l.umax);
}

private static limit intersect(this limit l, limit l2) {
    if (l.min < l2.min) {
        l.min = l2.min;
    }
    if (l.umin < l2.umin) {
        l.umin = l2.umin;
    }
    if (l.max > l2.max) {
        l.max = l2.max;
    }
    if (l.umax > l2.umax) {
        l.umax = l2.umax;
    }
    return l;

}

private static limit noLimit = new limit(math.MinInt64,math.MaxInt64,0,math.MaxUint64);

// a limitFact is a limit known for a particular value.
private partial struct limitFact {
    public ID vid;
    public limit limit;
}

// factsTable keeps track of relations between pairs of values.
//
// The fact table logic is sound, but incomplete. Outside of a few
// special cases, it performs no deduction or arithmetic. While there
// are known decision procedures for this, the ad hoc approach taken
// by the facts table is effective for real code while remaining very
// efficient.
private partial struct factsTable {
    public bool unsat; // true if facts contains a contradiction
    public nint unsatDepth; // number of unsat checkpoints

    public map<pair, relation> facts; // current known set of relation
    public slice<fact> stack; // previous sets of relations

// order is a couple of partial order sets that record information
// about relations between SSA values in the signed and unsigned
// domain.
    public ptr<poset> orderS;
    public ptr<poset> orderU; // known lower and upper bounds on individual values.
    public map<ID, limit> limits;
    public slice<limitFact> limitStack; // previous entries

// For each slice s, a map from s to a len(s)/cap(s) value (if any)
// TODO: check if there are cases that matter where we have
// more than one len(s) for a slice. We could keep a list if necessary.
    public map<ID, ptr<Value>> lens;
    public map<ID, ptr<Value>> caps; // zero is a zero-valued constant
    public ptr<Value> zero;
}

// checkpointFact is an invalid value used for checkpointing
// and restoring factsTable.
private static fact checkpointFact = new fact();
private static limitFact checkpointBound = new limitFact();

private static ptr<factsTable> newFactsTable(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    ptr<factsTable> ft = addr(new factsTable());
    ft.orderS = f.newPoset();
    ft.orderU = f.newPoset();
    ft.orderS.SetUnsigned(false);
    ft.orderU.SetUnsigned(true);
    ft.facts = make_map<pair, relation>();
    ft.stack = make_slice<fact>(4);
    ft.limits = make_map<ID, limit>();
    ft.limitStack = make_slice<limitFact>(4);
    ft.zero = f.ConstInt64(f.Config.Types.Int64, 0);
    return _addr_ft!;
}

// update updates the set of relations between v and w in domain d
// restricting it to r.
private static void update(this ptr<factsTable> _addr_ft, ptr<Block> _addr_parent, ptr<Value> _addr_v, ptr<Value> _addr_w, domain d, relation r) => func((_, panic, _) => {
    ref factsTable ft = ref _addr_ft.val;
    ref Block parent = ref _addr_parent.val;
    ref Value v = ref _addr_v.val;
    ref Value w = ref _addr_w.val;

    if (parent.Func.pass.debug > 2) {
        parent.Func.Warnl(parent.Pos, "parent=%s, update %s %s %s", parent, v, w, r);
    }
    if (ft.unsat) {
        return ;
    }
    if (v == w) {
        if (r & eq == 0) {
            ft.unsat = true;
        }
        return ;

    }
    if (d == signed || d == unsigned) {
        bool ok = default;
        var order = ft.orderS;
        if (d == unsigned) {
            order = ft.orderU;
        }

        if (r == lt) 
            ok = order.SetOrder(v, w);
        else if (r == gt) 
            ok = order.SetOrder(w, v);
        else if (r == lt | eq) 
            ok = order.SetOrderOrEqual(v, w);
        else if (r == gt | eq) 
            ok = order.SetOrderOrEqual(w, v);
        else if (r == eq) 
            ok = order.SetEqual(v, w);
        else if (r == lt | gt) 
            ok = order.SetNonEqual(v, w);
        else 
            panic("unknown relation");
                if (!ok) {
            if (parent.Func.pass.debug > 2) {
                parent.Func.Warnl(parent.Pos, "unsat %s %s %s", v, w, r);
            }
            ft.unsat = true;
            return ;
        }
    }
    else
 {
        if (lessByID(_addr_w, _addr_v)) {
            (v, w) = (w, v);            r = reverseBits[r];
        }
        pair p = new pair(v,w,d);
        var (oldR, ok) = ft.facts[p];
        if (!ok) {
            if (v == w) {
                oldR = eq;
            }
            else
 {
                oldR = lt | eq | gt;
            }

        }
        if (oldR == r) {
            return ;
        }
        ft.stack = append(ft.stack, new fact(p,oldR));
        ft.facts[p] = oldR & r; 
        // If this relation is not satisfiable, mark it and exit right away
        if (oldR & r == 0) {
            if (parent.Func.pass.debug > 2) {
                parent.Func.Warnl(parent.Pos, "unsat %s %s %s", v, w, r);
            }
            ft.unsat = true;
            return ;
        }
    }
    if (v.isGenericIntConst()) {
        (v, w) = (w, v);        r = reverseBits[r];
    }
    if (v != null && w.isGenericIntConst()) { 
        // Note: all the +1/-1 below could overflow/underflow. Either will
        // still generate correct results, it will just lead to imprecision.
        // In fact if there is overflow/underflow, the corresponding
        // code is unreachable because the known range is outside the range
        // of the value's type.
        var (old, ok) = ft.limits[v.ID];
        if (!ok) {
            old = noLimit;
            if (v.isGenericIntConst()) {

                if (d == signed) 
                    (old.min, old.max) = (v.AuxInt, v.AuxInt);                    if (v.AuxInt >= 0) {
                        (old.umin, old.umax) = (uint64(v.AuxInt), uint64(v.AuxInt));
                    }

                else if (d == unsigned) 
                    old.umin = v.AuxUnsigned();
                    old.umax = old.umin;
                    if (int64(old.umin) >= 0) {
                        (old.min, old.max) = (int64(old.umin), int64(old.umin));
                    }

                            }

        }
        var lim = noLimit;

        if (d == signed) 
            var c = w.AuxInt;

            if (r == lt) 
                lim.max = c - 1;
            else if (r == lt | eq) 
                lim.max = c;
            else if (r == gt | eq) 
                lim.min = c;
            else if (r == gt) 
                lim.min = c + 1;
            else if (r == lt | gt) 
                lim = old;
                if (c == lim.min) {
                    lim.min++;
                }
                if (c == lim.max) {
                    lim.max--;
                }
            else if (r == eq) 
                lim.min = c;
                lim.max = c;
                        if (lim.min >= 0) { 
                // int(x) >= 0 && int(x) >= N  ⇒  uint(x) >= N
                lim.umin = uint64(lim.min);

            }

            if (lim.max != noLimit.max && old.min >= 0 && lim.max >= 0) { 
                // 0 <= int(x) <= N  ⇒  0 <= uint(x) <= N
                // This is for a max update, so the lower bound
                // comes from what we already know (old).
                lim.umax = uint64(lim.max);

            }

        else if (d == unsigned) 
            var uc = w.AuxUnsigned();

            if (r == lt) 
                lim.umax = uc - 1;
            else if (r == lt | eq) 
                lim.umax = uc;
            else if (r == gt | eq) 
                lim.umin = uc;
            else if (r == gt) 
                lim.umin = uc + 1;
            else if (r == lt | gt) 
                lim = old;
                if (uc == lim.umin) {
                    lim.umin++;
                }
                if (uc == lim.umax) {
                    lim.umax--;
                }
            else if (r == eq) 
                lim.umin = uc;
                lim.umax = uc;
            // We could use the contrapositives of the
            // signed implications to derive signed facts,
            // but it turns out not to matter.
                ft.limitStack = append(ft.limitStack, new limitFact(v.ID,old));
        lim = old.intersect(lim);
        ft.limits[v.ID] = lim;
        if (v.Block.Func.pass.debug > 2) {
            v.Block.Func.Warnl(parent.Pos, "parent=%s, new limits %s %s %s %s", parent, v, w, r, lim.String());
        }
        if (lim.min > lim.max || lim.umin > lim.umax) {
            ft.unsat = true;
            return ;
        }
    }
    if (d != signed && d != unsigned) {
        return ;
    }
    if (v.Op == OpSliceLen && r & lt == 0 && ft.caps[v.Args[0].ID] != null) { 
        // len(s) > w implies cap(s) > w
        // len(s) >= w implies cap(s) >= w
        // len(s) == w implies cap(s) >= w
        ft.update(parent, ft.caps[v.Args[0].ID], w, d, r | gt);

    }
    if (w.Op == OpSliceLen && r & gt == 0 && ft.caps[w.Args[0].ID] != null) { 
        // same, length on the RHS.
        ft.update(parent, v, ft.caps[w.Args[0].ID], d, r | lt);

    }
    if (v.Op == OpSliceCap && r & gt == 0 && ft.lens[v.Args[0].ID] != null) { 
        // cap(s) < w implies len(s) < w
        // cap(s) <= w implies len(s) <= w
        // cap(s) == w implies len(s) <= w
        ft.update(parent, ft.lens[v.Args[0].ID], w, d, r | lt);

    }
    if (w.Op == OpSliceCap && r & lt == 0 && ft.lens[w.Args[0].ID] != null) { 
        // same, capacity on the RHS.
        ft.update(parent, v, ft.lens[w.Args[0].ID], d, r | gt);

    }
    if (r == lt || r == lt | eq) {
        (v, w) = (w, v);        r = reverseBits[r];
    }

    if (r == gt) 
        {
            var x__prev1 = x;

            var (x, delta) = isConstDelta(_addr_v);

            if (x != null && delta == 1) { 
                // x+1 > w  ⇒  x >= w
                //
                // This is useful for eliminating the
                // growslice branch of append.
                ft.update(parent, x, w, d, gt | eq);

            }            {
                var x__prev2 = x;

                (x, delta) = isConstDelta(_addr_w);


                else if (x != null && delta == -1) { 
                    // v > x-1  ⇒  v >= x
                    ft.update(parent, v, x, d, gt | eq);

                }

                x = x__prev2;

            }


            x = x__prev1;

        }

    else if (r == gt | eq) 
        {
            var x__prev1 = x;

            (x, delta) = isConstDelta(_addr_v);

            if (x != null && delta == -1) { 
                // x-1 >= w && x > min  ⇒  x > w
                //
                // Useful for i > 0; s[i-1].
                var (lim, ok) = ft.limits[x.ID];
                if (ok && ((d == signed && lim.min > opMin[v.Op]) || (d == unsigned && lim.umin > 0))) {
                    ft.update(parent, x, w, d, gt);
                }

            }            {
                var x__prev2 = x;

                (x, delta) = isConstDelta(_addr_w);


                else if (x != null && delta == 1) { 
                    // v >= x+1 && x < max  ⇒  v > x
                    (lim, ok) = ft.limits[x.ID];
                    if (ok && ((d == signed && lim.max < opMax[w.Op]) || (d == unsigned && lim.umax < opUMax[w.Op]))) {
                        ft.update(parent, v, x, d, gt);
                    }

                }

                x = x__prev2;

            }


            x = x__prev1;

        }

    // Process: x+delta > w (with delta constant)
    // Only signed domain for now (useful for accesses to slices in loops).
    if (r == gt || r == gt | eq) {
        {
            var x__prev2 = x;

            (x, delta) = isConstDelta(_addr_v);

            if (x != null && d == signed) {
                if (parent.Func.pass.debug > 1) {
                    parent.Func.Warnl(parent.Pos, "x+d %s w; x:%v %v delta:%v w:%v d:%v", r, x, parent.String(), delta, w.AuxInt, d);
                }
                if (!w.isGenericIntConst()) { 
                    // If we know that x+delta > w but w is not constant, we can derive:
                    //    if delta < 0 and x > MinInt - delta, then x > w (because x+delta cannot underflow)
                    // This is useful for loops with bounds "len(slice)-K" (delta = -K)
                    {
                        var l__prev4 = l;

                        var (l, has) = ft.limits[x.ID];

                        if (has && delta < 0) {
                            if ((x.Type.Size() == 8 && l.min >= math.MinInt64 - delta) || (x.Type.Size() == 4 && l.min >= math.MinInt32 - delta)) {
                                ft.update(parent, x, w, signed, r);
                            }
                        }

                        l = l__prev4;

                    }

                }
                else
 { 
                    // With w,delta constants, we want to derive: x+delta > w  ⇒  x > w-delta
                    //
                    // We compute (using integers of the correct size):
                    //    min = w - delta
                    //    max = MaxInt - delta
                    //
                    // And we prove that:
                    //    if min<max: min < x AND x <= max
                    //    if min>max: min < x OR  x <= max
                    //
                    // This is always correct, even in case of overflow.
                    //
                    // If the initial fact is x+delta >= w instead, the derived conditions are:
                    //    if min<max: min <= x AND x <= max
                    //    if min>max: min <= x OR  x <= max
                    //
                    // Notice the conditions for max are still <=, as they handle overflows.
                    long min = default;                    long max = default;

                    ptr<Value> vmin;                    ptr<Value> vmax;

                    switch (x.Type.Size()) {
                        case 8: 
                            min = w.AuxInt - delta;
                            max = int64(~uint64(0) >> 1) - delta;

                            vmin = parent.NewValue0I(parent.Pos, OpConst64, parent.Func.Config.Types.Int64, min);
                            vmax = parent.NewValue0I(parent.Pos, OpConst64, parent.Func.Config.Types.Int64, max);
                            break;
                        case 4: 
                            min = int64(int32(w.AuxInt) - int32(delta));
                            max = int64(int32(~uint32(0) >> 1) - int32(delta));

                            vmin = parent.NewValue0I(parent.Pos, OpConst32, parent.Func.Config.Types.Int32, min);
                            vmax = parent.NewValue0I(parent.Pos, OpConst32, parent.Func.Config.Types.Int32, max);
                            break;
                        default: 
                            panic("unimplemented");
                            break;
                    }

                    if (min < max) { 
                        // Record that x > min and max >= x
                        ft.update(parent, x, vmin, d, r);
                        ft.update(parent, vmax, x, d, r | eq);

                    }
                    else
 { 
                        // We know that either x>min OR x<=max. factsTable cannot record OR conditions,
                        // so let's see if we can already prove that one of them is false, in which case
                        // the other must be true
                        {
                            var l__prev5 = l;

                            (l, has) = ft.limits[x.ID];

                            if (has) {
                                if (l.max <= min) {
                                    if (r & eq == 0 || l.max < min) { 
                                        // x>min (x>=min) is impossible, so it must be x<=max
                                        ft.update(parent, vmax, x, d, r | eq);

                                    }

                                }
                                else if (l.min > max) { 
                                    // x<=max is impossible, so it must be x>min
                                    ft.update(parent, x, vmin, d, r);

                                }

                            }

                            l = l__prev5;

                        }

                    }

                }

            }

            x = x__prev2;

        }

    }
    if (isCleanExt(_addr_v)) {

        if (d == signed && v.Args[0].Type.IsSigned())
        {
            fallthrough = true;
        }
        if (fallthrough || d == unsigned && !v.Args[0].Type.IsSigned())
        {
            ft.update(parent, v.Args[0], w, d, r);
            goto __switch_break0;
        }

        __switch_break0:;

    }
    if (isCleanExt(_addr_w)) {

        if (d == signed && w.Args[0].Type.IsSigned())
        {
            fallthrough = true;
        }
        if (fallthrough || d == unsigned && !w.Args[0].Type.IsSigned())
        {
            ft.update(parent, v, w.Args[0], d, r);
            goto __switch_break1;
        }

        __switch_break1:;

    }
});

private static map opMin = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<Op, long>{OpAdd64:math.MinInt64,OpSub64:math.MinInt64,OpAdd32:math.MinInt32,OpSub32:math.MinInt32,};

private static map opMax = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<Op, long>{OpAdd64:math.MaxInt64,OpSub64:math.MaxInt64,OpAdd32:math.MaxInt32,OpSub32:math.MaxInt32,};

private static map opUMax = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<Op, ulong>{OpAdd64:math.MaxUint64,OpSub64:math.MaxUint64,OpAdd32:math.MaxUint32,OpSub32:math.MaxUint32,};

// isNonNegative reports whether v is known to be non-negative.
private static bool isNonNegative(this ptr<factsTable> _addr_ft, ptr<Value> _addr_v) => func((_, panic, _) => {
    ref factsTable ft = ref _addr_ft.val;
    ref Value v = ref _addr_v.val;

    if (isNonNegative(_addr_v)) {
        return true;
    }
    long max = default;
    switch (v.Type.Size()) {
        case 1: 
            max = math.MaxInt8;
            break;
        case 2: 
            max = math.MaxInt16;
            break;
        case 4: 
            max = math.MaxInt32;
            break;
        case 8: 
            max = math.MaxInt64;
            break;
        default: 
            panic("unexpected integer size");
            break;
    } 

    // Check if the recorded limits can prove that the value is positive

    {
        var l__prev1 = l;

        var (l, has) = ft.limits[v.ID];

        if (has && (l.min >= 0 || l.umax <= uint64(max))) {
            return true;
        }
        l = l__prev1;

    } 

    // Check if v = x+delta, and we can use x's limits to prove that it's positive
    {
        var (x, delta) = isConstDelta(_addr_v);

        if (x != null) {
            {
                var l__prev2 = l;

                (l, has) = ft.limits[x.ID];

                if (has) {
                    if (delta > 0 && l.min >= -delta && l.max <= max - delta) {
                        return true;
                    }
                    if (delta < 0 && l.min >= -delta) {
                        return true;
                    }
                }

                l = l__prev2;

            }

        }
    } 

    // Check if v is a value-preserving extension of a non-negative value.
    if (isCleanExt(_addr_v) && ft.isNonNegative(v.Args[0])) {
        return true;
    }
    return ft.orderS.OrderedOrEqual(ft.zero, v);

});

// checkpoint saves the current state of known relations.
// Called when descending on a branch.
private static void checkpoint(this ptr<factsTable> _addr_ft) {
    ref factsTable ft = ref _addr_ft.val;

    if (ft.unsat) {
        ft.unsatDepth++;
    }
    ft.stack = append(ft.stack, checkpointFact);
    ft.limitStack = append(ft.limitStack, checkpointBound);
    ft.orderS.Checkpoint();
    ft.orderU.Checkpoint();

}

// restore restores known relation to the state just
// before the previous checkpoint.
// Called when backing up on a branch.
private static void restore(this ptr<factsTable> _addr_ft) {
    ref factsTable ft = ref _addr_ft.val;

    if (ft.unsatDepth > 0) {
        ft.unsatDepth--;
    }
    else
 {
        ft.unsat = false;
    }
    while (true) {
        var old = ft.stack[len(ft.stack) - 1];
        ft.stack = ft.stack[..(int)len(ft.stack) - 1];
        if (old == checkpointFact) {
            break;
        }
        if (old.r == lt | eq | gt) {
            delete(ft.facts, old.p);
        }
        else
 {
            ft.facts[old.p] = old.r;
        }
    }
    while (true) {
        old = ft.limitStack[len(ft.limitStack) - 1];
        ft.limitStack = ft.limitStack[..(int)len(ft.limitStack) - 1];
        if (old.vid == 0) { // checkpointBound
            break;

        }
        if (old.limit == noLimit) {
            delete(ft.limits, old.vid);
        }
        else
 {
            ft.limits[old.vid] = old.limit;
        }
    }
    ft.orderS.Undo();
    ft.orderU.Undo();

}

private static bool lessByID(ptr<Value> _addr_v, ptr<Value> _addr_w) {
    ref Value v = ref _addr_v.val;
    ref Value w = ref _addr_w.val;

    if (v == null && w == null) { 
        // Should not happen, but just in case.
        return false;

    }
    if (v == null) {
        return true;
    }
    return w != null && v.ID < w.ID;

}

private static array<relation> reverseBits = new array<relation>(new relation[] { 0, 4, 2, 6, 1, 5, 3, 7 });

// cleanup returns the posets to the free list
private static void cleanup(this ptr<factsTable> _addr_ft, ptr<Func> _addr_f) {
    ref factsTable ft = ref _addr_ft.val;
    ref Func f = ref _addr_f.val;

    foreach (var (_, po) in new slice<ptr<poset>>(new ptr<poset>[] { ft.orderS, ft.orderU })) { 
        // Make sure it's empty as it should be. A non-empty poset
        // might cause errors and miscompilations if reused.
        if (checkEnabled) {
            {
                var err = po.CheckEmpty();

                if (err != null) {
                    f.Fatalf("poset not empty after function %s: %v", f.Name, err);
                }

            }

        }
        f.retPoset(po);

    }
}

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
//
// prove works by finding contradictions and trimming branches whose
// conditions are unsatisfiable given the branches leading up to them.
// It tracks a "fact table" of branch conditions. For each branching
// block, it asserts the branch conditions that uniquely dominate that
// block, and then separately asserts the block's branch condition and
// its negation. If either leads to a contradiction, it can trim that
// successor.
private static void prove(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    var ft = newFactsTable(_addr_f);
    ft.checkpoint();

    map<ptr<Block>, slice<ptr<Value>>> lensVars = default; 

    // Find length and capacity ops.
    foreach (var (_, b) in f.Blocks) {
        {
            var v__prev2 = v;

            foreach (var (_, __v) in b.Values) {
                v = __v;
                if (v.Uses == 0) { 
                    // We don't care about dead values.
                    // (There can be some that are CSEd but not removed yet.)
                    continue;

                }


                if (v.Op == OpStringLen) 
                    ft.update(b, v, ft.zero, signed, gt | eq);
                else if (v.Op == OpSliceLen) 
                    if (ft.lens == null) {
                        ft.lens = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ID, ptr<Value>>{};
                    } 
                    // Set all len Values for the same slice as equal in the poset.
                    // The poset handles transitive relations, so Values related to
                    // any OpSliceLen for this slice will be correctly related to others.
                    {
                        var (l, ok) = ft.lens[v.Args[0].ID];

                        if (ok) {
                            ft.update(b, v, l, signed, eq);
                        }
                        else
 {
                            ft.lens[v.Args[0].ID] = v;
                        }

                    }

                    ft.update(b, v, ft.zero, signed, gt | eq);
                    if (v.Args[0].Op == OpSliceMake) {
                        if (lensVars == null) {
                            lensVars = make_map<ptr<Block>, slice<ptr<Value>>>();
                        }
                        lensVars[b] = append(lensVars[b], v);
                    }

                else if (v.Op == OpSliceCap) 
                    if (ft.caps == null) {
                        ft.caps = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ID, ptr<Value>>{};
                    } 
                    // Same as case OpSliceLen above, but for slice cap.
                    {
                        var (c, ok) = ft.caps[v.Args[0].ID];

                        if (ok) {
                            ft.update(b, v, c, signed, eq);
                        }
                        else
 {
                            ft.caps[v.Args[0].ID] = v;
                        }

                    }

                    ft.update(b, v, ft.zero, signed, gt | eq);
                    if (v.Args[0].Op == OpSliceMake) {
                        if (lensVars == null) {
                            lensVars = make_map<ptr<Block>, slice<ptr<Value>>>();
                        }
                        lensVars[b] = append(lensVars[b], v);
                    }

                            }

            v = v__prev2;
        }
    }    map<ptr<Block>, indVar> indVars = default;
    {
        var v__prev1 = v;

        foreach (var (_, __v) in findIndVar(f)) {
            v = __v;
            if (indVars == null) {
                indVars = make_map<ptr<Block>, indVar>();
            }
            indVars[v.entry] = v;
        }
        v = v__prev1;
    }

    private partial struct walkState { // : nint
    }
    const walkState descend = iota;
    const var simplify = 0;
 
    // work maintains the DFS stack.
    private partial struct bp {
        public ptr<Block> block; // current handled block
        public walkState state; // what's to do
    }
    var work = make_slice<bp>(0, 256);
    work = append(work, new bp(block:f.Entry,state:descend,));

    var idom = f.Idom();
    var sdom = f.Sdom(); 

    // DFS on the dominator tree.
    //
    // For efficiency, we consider only the dominator tree rather
    // than the entire flow graph. On the way down, we consider
    // incoming branches and accumulate conditions that uniquely
    // dominate the current block. If we discover a contradiction,
    // we can eliminate the entire block and all of its children.
    // On the way back up, we consider outgoing branches that
    // haven't already been considered. This way we consider each
    // branch condition only once.
    while (len(work) > 0) {
        var node = work[len(work) - 1];
        work = work[..(int)len(work) - 1];
        var parent = idom[node.block.ID];
        var branch = getBranch(sdom, _addr_parent, _addr_node.block);


        if (node.state == descend) 
            ft.checkpoint(); 

            // Entering the block, add the block-depending facts that we collected
            // at the beginning: induction variables and lens/caps of slices.
            {
                var (iv, ok) = indVars[node.block];

                if (ok) {
                    addIndVarRestrictions(_addr_ft, _addr_parent, iv);
                }

            }

            {
                var (lens, ok) = lensVars[node.block];

                if (ok) {
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in lens) {
                            v = __v;

                            if (v.Op == OpSliceLen) 
                                ft.update(node.block, v, v.Args[0].Args[1], signed, eq);
                            else if (v.Op == OpSliceCap) 
                                ft.update(node.block, v, v.Args[0].Args[2], signed, eq);
                            
                        }

                        v = v__prev2;
                    }
                }

            }


            if (branch != unknown) {
                addBranchRestrictions(_addr_ft, _addr_parent, branch);
                if (ft.unsat) { 
                    // node.block is unreachable.
                    // Remove it and don't visit
                    // its children.
                    removeBranch(_addr_parent, branch);
                    ft.restore();
                    break;

                } 
                // Otherwise, we can now commit to
                // taking this branch. We'll restore
                // ft when we unwind.
            } 

            // Add inductive facts for phis in this block.
            addLocalInductiveFacts(_addr_ft, _addr_node.block);

            work = append(work, new bp(block:node.block,state:simplify,));
            {
                var s = sdom.Child(node.block);

                while (s != null) {
                    work = append(work, new bp(block:s,state:descend,));
                    s = sdom.Sibling(s);
                }

            }
        else if (node.state == simplify) 
            simplifyBlock(sdom, _addr_ft, _addr_node.block);
            ft.restore();
        
    }

    ft.restore();

    ft.cleanup(f);

}

// getBranch returns the range restrictions added by p
// when reaching b. p is the immediate dominator of b.
private static branch getBranch(SparseTree sdom, ptr<Block> _addr_p, ptr<Block> _addr_b) {
    ref Block p = ref _addr_p.val;
    ref Block b = ref _addr_b.val;

    if (p == null || p.Kind != BlockIf) {
        return unknown;
    }
    if (sdom.IsAncestorEq(p.Succs[0].b, b) && len(p.Succs[0].b.Preds) == 1) {
        return positive;
    }
    if (sdom.IsAncestorEq(p.Succs[1].b, b) && len(p.Succs[1].b.Preds) == 1) {
        return negative;
    }
    return unknown;

}

// addIndVarRestrictions updates the factsTables ft with the facts
// learned from the induction variable indVar which drives the loop
// starting in Block b.
private static void addIndVarRestrictions(ptr<factsTable> _addr_ft, ptr<Block> _addr_b, indVar iv) {
    ref factsTable ft = ref _addr_ft.val;
    ref Block b = ref _addr_b.val;

    var d = signed;
    if (ft.isNonNegative(iv.min) && ft.isNonNegative(iv.max)) {
        d |= unsigned;
    }
    if (iv.flags & indVarMinExc == 0) {
        addRestrictions(_addr_b, _addr_ft, d, _addr_iv.min, _addr_iv.ind, lt | eq);
    }
    else
 {
        addRestrictions(_addr_b, _addr_ft, d, _addr_iv.min, _addr_iv.ind, lt);
    }
    if (iv.flags & indVarMaxInc == 0) {
        addRestrictions(_addr_b, _addr_ft, d, _addr_iv.ind, _addr_iv.max, lt);
    }
    else
 {
        addRestrictions(_addr_b, _addr_ft, d, _addr_iv.ind, _addr_iv.max, lt | eq);
    }
}

// addBranchRestrictions updates the factsTables ft with the facts learned when
// branching from Block b in direction br.
private static void addBranchRestrictions(ptr<factsTable> _addr_ft, ptr<Block> _addr_b, branch br) => func((_, panic, _) => {
    ref factsTable ft = ref _addr_ft.val;
    ref Block b = ref _addr_b.val;

    var c = b.Controls[0];

    if (br == negative) 
        addRestrictions(_addr_b, _addr_ft, boolean, _addr_null, _addr_c, eq);
    else if (br == positive) 
        addRestrictions(_addr_b, _addr_ft, boolean, _addr_null, _addr_c, lt | gt);
    else 
        panic("unknown branch");
        {
        var (tr, has) = domainRelationTable[c.Op];

        if (has) { 
            // When we branched from parent we learned a new set of
            // restrictions. Update the factsTable accordingly.
            var d = tr.d;
            if (d == signed && ft.isNonNegative(c.Args[0]) && ft.isNonNegative(c.Args[1])) {
                d |= unsigned;
            }


            if (c.Op == OpIsInBounds || c.Op == OpIsSliceInBounds) 
                // 0 <= a0 < a1 (or 0 <= a0 <= a1)
                //
                // On the positive branch, we learn:
                //   signed: 0 <= a0 < a1 (or 0 <= a0 <= a1)
                //   unsigned:    a0 < a1 (or a0 <= a1)
                //
                // On the negative branch, we learn (0 > a0 ||
                // a0 >= a1). In the unsigned domain, this is
                // simply a0 >= a1 (which is the reverse of the
                // positive branch, so nothing surprising).
                // But in the signed domain, we can't express the ||
                // condition, so check if a0 is non-negative instead,
                // to be able to learn something.

                if (br == negative) 
                    d = unsigned;
                    if (ft.isNonNegative(c.Args[0])) {
                        d |= signed;
                    }
                    addRestrictions(_addr_b, _addr_ft, d, _addr_c.Args[0], _addr_c.Args[1], tr.r ^ (lt | gt | eq));
                else if (br == positive) 
                    addRestrictions(_addr_b, _addr_ft, signed, _addr_ft.zero, _addr_c.Args[0], lt | eq);
                    addRestrictions(_addr_b, _addr_ft, d, _addr_c.Args[0], _addr_c.Args[1], tr.r);
                            else 

                if (br == negative) 
                    addRestrictions(_addr_b, _addr_ft, d, _addr_c.Args[0], _addr_c.Args[1], tr.r ^ (lt | gt | eq));
                else if (br == positive) 
                    addRestrictions(_addr_b, _addr_ft, d, _addr_c.Args[0], _addr_c.Args[1], tr.r);
                            
        }
    }

});

// addRestrictions updates restrictions from the immediate
// dominating block (p) using r.
private static void addRestrictions(ptr<Block> _addr_parent, ptr<factsTable> _addr_ft, domain t, ptr<Value> _addr_v, ptr<Value> _addr_w, relation r) {
    ref Block parent = ref _addr_parent.val;
    ref factsTable ft = ref _addr_ft.val;
    ref Value v = ref _addr_v.val;
    ref Value w = ref _addr_w.val;

    if (t == 0) { 
        // Trivial case: nothing to do.
        // Shoult not happen, but just in case.
        return ;

    }
    {
        var i = domain(1);

        while (i <= t) {
            if (t & i == 0) {
                continue;
            i<<=1;
            }

            ft.update(parent, v, w, i, r);

        }
    }

}

// addLocalInductiveFacts adds inductive facts when visiting b, where
// b is a join point in a loop. In contrast with findIndVar, this
// depends on facts established for b, which is why it happens when
// visiting b. addLocalInductiveFacts specifically targets the pattern
// created by OFORUNTIL, which isn't detected by findIndVar.
//
// TODO: It would be nice to combine this with findIndVar.
private static void addLocalInductiveFacts(ptr<factsTable> _addr_ft, ptr<Block> _addr_b) {
    ref factsTable ft = ref _addr_ft.val;
    ref Block b = ref _addr_b.val;
 
    // This looks for a specific pattern of induction:
    //
    // 1. i1 = OpPhi(min, i2) in b
    // 2. i2 = i1 + 1
    // 3. i2 < max at exit from b.Preds[1]
    // 4. min < max
    //
    // If all of these conditions are true, then i1 < max and i1 >= min.

    // To ensure this is a loop header node.
    if (len(b.Preds) != 2) {
        return ;
    }
    foreach (var (_, i1) in b.Values) {
        if (i1.Op != OpPhi) {
            continue;
        }
        var min = i1.Args[0];
        var i2 = i1.Args[1];
        {
            var (i1q, delta) = isConstDelta(_addr_i2);

            if (i1q != i1 || delta != 1) {
                continue;
            } 

            // Try to prove condition 3. We can't just query the
            // fact table for this because we don't know what the
            // facts of b.Preds[1] are (in general, b.Preds[1] is
            // a loop-back edge, so we haven't even been there
            // yet). As a conservative approximation, we look for
            // this condition in the predecessor chain until we
            // hit a join point.

        } 

        // Try to prove condition 3. We can't just query the
        // fact table for this because we don't know what the
        // facts of b.Preds[1] are (in general, b.Preds[1] is
        // a loop-back edge, so we haven't even been there
        // yet). As a conservative approximation, we look for
        // this condition in the predecessor chain until we
        // hit a join point.
        Func<ptr<Block>, ptr<Block>> uniquePred = b => {
            if (len(b.Preds) == 1) {
                return b.Preds[0].b;
            }
            return null;
        };
        var pred = b.Preds[1].b;
        var child = b;
        while (pred != null) {
            if (pred.Kind != BlockIf) {
                continue;
            (pred, child) = (uniquePred(pred), pred);
            }

            var control = pred.Controls[0];

            var br = unknown;
            if (pred.Succs[0].b == child) {
                br = positive;
            }

            if (pred.Succs[1].b == child) {
                if (br != unknown) {
                    continue;
                }
                br = negative;
            }

            if (br == unknown) {
                continue;
            }

            var (tr, has) = domainRelationTable[control.Op];
            if (!has) {
                continue;
            }

            var r = tr.r;
            if (br == negative) { 
                // Negative branch taken to reach b.
                // Complement the relations.
                r = (lt | eq | gt) ^ r;

            } 

            // Check for i2 < max or max > i2.
            ptr<Value> max;
            if (r == lt && control.Args[0] == i2) {
                max = control.Args[1];
            }
            else if (r == gt && control.Args[1] == i2) {
                max = control.Args[0];
            }
            else
 {
                continue;
            } 

            // Check condition 4 now that we have a
            // candidate max. For this we can query the
            // fact table. We "prove" min < max by showing
            // that min >= max is unsat. (This may simply
            // compare two constants; that's fine.)
            ft.checkpoint();
            ft.update(b, min, max, tr.d, gt | eq);
            var proved = ft.unsat;
            ft.restore();

            if (proved) { 
                // We know that min <= i1 < max.
                if (b.Func.pass.debug > 0) {
                    printIndVar(b, i1, min, max, 1, 0);
                }

                ft.update(b, min, i1, tr.d, lt | eq);
                ft.update(b, i1, max, tr.d, lt);

            }

        }

    }
}

private static map ctzNonZeroOp = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<Op, Op>{OpCtz8:OpCtz8NonZero,OpCtz16:OpCtz16NonZero,OpCtz32:OpCtz32NonZero,OpCtz64:OpCtz64NonZero};
private static map mostNegativeDividend = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<Op, long>{OpDiv16:-1<<15,OpMod16:-1<<15,OpDiv32:-1<<31,OpMod32:-1<<31,OpDiv64:-1<<63,OpMod64:-1<<63};

// simplifyBlock simplifies some constant values in b and evaluates
// branches to non-uniquely dominated successors of b.
private static void simplifyBlock(SparseTree sdom, ptr<factsTable> _addr_ft, ptr<Block> _addr_b) => func((_, panic, _) => {
    ref factsTable ft = ref _addr_ft.val;
    ref Block b = ref _addr_b.val;

    foreach (var (_, v) in b.Values) {

        if (v.Op == OpSlicemask) 
        {
            // Replace OpSlicemask operations in b with constants where possible.
            var (x, delta) = isConstDelta(_addr_v.Args[0]);
            if (x == null) {
                continue;
            } 
            // slicemask(x + y)
            // if x is larger than -y (y is negative), then slicemask is -1.
            var (lim, ok) = ft.limits[x.ID];
            if (!ok) {
                continue;
            }

            if (lim.umin > uint64(-delta)) {
                if (v.Args[0].Op == OpAdd64) {
                    v.reset(OpConst64);
                }
                else
 {
                    v.reset(OpConst32);
                }

                if (b.Func.pass.debug > 0) {
                    b.Func.Warnl(v.Pos, "Proved slicemask not needed");
                }

                v.AuxInt = -1;

            }

            goto __switch_break2;
        }
        if (v.Op == OpCtz8 || v.Op == OpCtz16 || v.Op == OpCtz32 || v.Op == OpCtz64) 
        {
            // On some architectures, notably amd64, we can generate much better
            // code for CtzNN if we know that the argument is non-zero.
            // Capture that information here for use in arch-specific optimizations.
            var x = v.Args[0];
            (lim, ok) = ft.limits[x.ID];
            if (!ok) {
                continue;
            }
            if (lim.umin > 0 || lim.min > 0 || lim.max < 0) {
                if (b.Func.pass.debug > 0) {
                    b.Func.Warnl(v.Pos, "Proved %v non-zero", v.Op);
                }
                v.Op = ctzNonZeroOp[v.Op];
            }
            goto __switch_break2;
        }
        if (v.Op == OpRsh8x8 || v.Op == OpRsh8x16 || v.Op == OpRsh8x32 || v.Op == OpRsh8x64 || v.Op == OpRsh16x8 || v.Op == OpRsh16x16 || v.Op == OpRsh16x32 || v.Op == OpRsh16x64 || v.Op == OpRsh32x8 || v.Op == OpRsh32x16 || v.Op == OpRsh32x32 || v.Op == OpRsh32x64 || v.Op == OpRsh64x8 || v.Op == OpRsh64x16 || v.Op == OpRsh64x32 || v.Op == OpRsh64x64) 
        {
            // Check whether, for a >> b, we know that a is non-negative
            // and b is all of a's bits except the MSB. If so, a is shifted to zero.
            nint bits = 8 * v.Type.Size();
            if (v.Args[1].isGenericIntConst() && v.Args[1].AuxInt >= bits - 1 && ft.isNonNegative(v.Args[0])) {
                if (b.Func.pass.debug > 0) {
                    b.Func.Warnl(v.Pos, "Proved %v shifts to zero", v.Op);
                }
                switch (bits) {
                    case 64: 
                        v.reset(OpConst64);
                        break;
                    case 32: 
                        v.reset(OpConst32);
                        break;
                    case 16: 
                        v.reset(OpConst16);
                        break;
                    case 8: 
                        v.reset(OpConst8);
                        break;
                    default: 
                        panic("unexpected integer size");
                        break;
                }
                v.AuxInt = 0;
                continue; // Be sure not to fallthrough - this is no longer OpRsh.
            } 
            // If the Rsh hasn't been replaced with 0, still check if it is bounded.
            fallthrough = true;
        }
        if (fallthrough || v.Op == OpLsh8x8 || v.Op == OpLsh8x16 || v.Op == OpLsh8x32 || v.Op == OpLsh8x64 || v.Op == OpLsh16x8 || v.Op == OpLsh16x16 || v.Op == OpLsh16x32 || v.Op == OpLsh16x64 || v.Op == OpLsh32x8 || v.Op == OpLsh32x16 || v.Op == OpLsh32x32 || v.Op == OpLsh32x64 || v.Op == OpLsh64x8 || v.Op == OpLsh64x16 || v.Op == OpLsh64x32 || v.Op == OpLsh64x64 || v.Op == OpRsh8Ux8 || v.Op == OpRsh8Ux16 || v.Op == OpRsh8Ux32 || v.Op == OpRsh8Ux64 || v.Op == OpRsh16Ux8 || v.Op == OpRsh16Ux16 || v.Op == OpRsh16Ux32 || v.Op == OpRsh16Ux64 || v.Op == OpRsh32Ux8 || v.Op == OpRsh32Ux16 || v.Op == OpRsh32Ux32 || v.Op == OpRsh32Ux64 || v.Op == OpRsh64Ux8 || v.Op == OpRsh64Ux16 || v.Op == OpRsh64Ux32 || v.Op == OpRsh64Ux64) 
        {
            // Check whether, for a << b, we know that b
            // is strictly less than the number of bits in a.
            var by = v.Args[1];
            (lim, ok) = ft.limits[by.ID];
            if (!ok) {
                continue;
            }
            bits = 8 * v.Args[0].Type.Size();
            if (lim.umax < uint64(bits) || (lim.max < bits && ft.isNonNegative(by))) {
                v.AuxInt = 1; // see shiftIsBounded
                if (b.Func.pass.debug > 0) {
                    b.Func.Warnl(v.Pos, "Proved %v bounded", v.Op);
                }

            }

            goto __switch_break2;
        }
        if (v.Op == OpDiv16 || v.Op == OpDiv32 || v.Op == OpDiv64 || v.Op == OpMod16 || v.Op == OpMod32 || v.Op == OpMod64) 
        {
            // On amd64 and 386 fix-up code can be avoided if we know
            //  the divisor is not -1 or the dividend > MinIntNN.
            // Don't modify AuxInt on other architectures,
            // as that can interfere with CSE.
            // TODO: add other architectures?
            if (b.Func.Config.arch != "386" && b.Func.Config.arch != "amd64") {
                break;
            }
            var divr = v.Args[1];
            var (divrLim, divrLimok) = ft.limits[divr.ID];
            var divd = v.Args[0];
            var (divdLim, divdLimok) = ft.limits[divd.ID];
            if ((divrLimok && (divrLim.max < -1 || divrLim.min > -1)) || (divdLimok && divdLim.min > mostNegativeDividend[v.Op])) { 
                // See DivisionNeedsFixUp in rewrite.go.
                // v.AuxInt = 1 means we have proved both that the divisor is not -1
                // and that the dividend is not the most negative integer,
                // so we do not need to add fix-up code.
                v.AuxInt = 1;
                if (b.Func.pass.debug > 0) {
                    b.Func.Warnl(v.Pos, "Proved %v does not need fix-up", v.Op);
                }

            }

            goto __switch_break2;
        }

        __switch_break2:;

    }    if (b.Kind != BlockIf) {
        return ;
    }
    var parent = b;
    foreach (var (i, branch) in new array<branch>(new branch[] { positive, negative })) {
        var child = parent.Succs[i].b;
        if (getBranch(sdom, _addr_parent, _addr_child) != unknown) { 
            // For edges to uniquely dominated blocks, we
            // already did this when we visited the child.
            continue;

        }
        ft.checkpoint();
        addBranchRestrictions(_addr_ft, _addr_parent, branch);
        var unsat = ft.unsat;
        ft.restore();
        if (unsat) { 
            // This branch is impossible, so remove it
            // from the block.
            removeBranch(_addr_parent, branch); 
            // No point in considering the other branch.
            // (It *is* possible for both to be
            // unsatisfiable since the fact table is
            // incomplete. We could turn this into a
            // BlockExit, but it doesn't seem worth it.)
            break;

        }
    }
});

private static void removeBranch(ptr<Block> _addr_b, branch branch) {
    ref Block b = ref _addr_b.val;

    var c = b.Controls[0];
    if (b.Func.pass.debug > 0) {
        @string verb = "Proved";
        if (branch == positive) {
            verb = "Disproved";
        }
        if (b.Func.pass.debug > 1) {
            b.Func.Warnl(b.Pos, "%s %s (%s)", verb, c.Op, c);
        }
        else
 {
            b.Func.Warnl(b.Pos, "%s %s", verb, c.Op);
        }
    }
    if (c != null && c.Pos.IsStmt() == src.PosIsStmt && c.Pos.SameFileAndLine(b.Pos)) { 
        // attempt to preserve statement marker.
        b.Pos = b.Pos.WithIsStmt();

    }
    b.Kind = BlockFirst;
    b.ResetControls();
    if (branch == positive) {
        b.swapSuccessors();
    }
}

// isNonNegative reports whether v is known to be greater or equal to zero.
private static bool isNonNegative(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    if (!v.Type.IsInteger()) {
        v.Fatalf("isNonNegative bad type: %v", v.Type);
    }

    if (v.Op == OpConst64) 
        return v.AuxInt >= 0;
    else if (v.Op == OpConst32) 
        return int32(v.AuxInt) >= 0;
    else if (v.Op == OpConst16) 
        return int16(v.AuxInt) >= 0;
    else if (v.Op == OpConst8) 
        return int8(v.AuxInt) >= 0;
    else if (v.Op == OpStringLen || v.Op == OpSliceLen || v.Op == OpSliceCap || v.Op == OpZeroExt8to64 || v.Op == OpZeroExt16to64 || v.Op == OpZeroExt32to64 || v.Op == OpZeroExt8to32 || v.Op == OpZeroExt16to32 || v.Op == OpZeroExt8to16 || v.Op == OpCtz64 || v.Op == OpCtz32 || v.Op == OpCtz16 || v.Op == OpCtz8) 
        return true;
    else if (v.Op == OpRsh64Ux64 || v.Op == OpRsh32Ux64) 
        var by = v.Args[1];
        return by.Op == OpConst64 && by.AuxInt > 0;
    else if (v.Op == OpRsh64x64 || v.Op == OpRsh32x64 || v.Op == OpRsh8x64 || v.Op == OpRsh16x64 || v.Op == OpRsh32x32 || v.Op == OpRsh64x32 || v.Op == OpSignExt32to64 || v.Op == OpSignExt16to64 || v.Op == OpSignExt8to64 || v.Op == OpSignExt16to32 || v.Op == OpSignExt8to32) 
        return isNonNegative(_addr_v.Args[0]);
    else if (v.Op == OpAnd64 || v.Op == OpAnd32 || v.Op == OpAnd16 || v.Op == OpAnd8) 
        return isNonNegative(_addr_v.Args[0]) || isNonNegative(_addr_v.Args[1]);
    else if (v.Op == OpMod64 || v.Op == OpMod32 || v.Op == OpMod16 || v.Op == OpMod8 || v.Op == OpDiv64 || v.Op == OpDiv32 || v.Op == OpDiv16 || v.Op == OpDiv8 || v.Op == OpOr64 || v.Op == OpOr32 || v.Op == OpOr16 || v.Op == OpOr8 || v.Op == OpXor64 || v.Op == OpXor32 || v.Op == OpXor16 || v.Op == OpXor8) 
        return isNonNegative(_addr_v.Args[0]) && isNonNegative(_addr_v.Args[1]); 

        // We could handle OpPhi here, but the improvements from doing
        // so are very minor, and it is neither simple nor cheap.
        return false;

}

// isConstDelta returns non-nil if v is equivalent to w+delta (signed).
private static (ptr<Value>, long) isConstDelta(ptr<Value> _addr_v) {
    ptr<Value> w = default!;
    long delta = default;
    ref Value v = ref _addr_v.val;

    var cop = OpConst64;

    if (v.Op == OpAdd32 || v.Op == OpSub32) 
        cop = OpConst32;
    
    if (v.Op == OpAdd64 || v.Op == OpAdd32) 
        if (v.Args[0].Op == cop) {
            return (_addr_v.Args[1]!, v.Args[0].AuxInt);
        }
        if (v.Args[1].Op == cop) {
            return (_addr_v.Args[0]!, v.Args[1].AuxInt);
        }
    else if (v.Op == OpSub64 || v.Op == OpSub32) 
        if (v.Args[1].Op == cop) {
            var aux = v.Args[1].AuxInt;
            if (aux != -aux) { // Overflow; too bad
                return (_addr_v.Args[0]!, -aux);

            }

        }
        return (_addr_null!, 0);

}

// isCleanExt reports whether v is the result of a value-preserving
// sign or zero extension
private static bool isCleanExt(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;


    if (v.Op == OpSignExt8to16 || v.Op == OpSignExt8to32 || v.Op == OpSignExt8to64 || v.Op == OpSignExt16to32 || v.Op == OpSignExt16to64 || v.Op == OpSignExt32to64) 
        // signed -> signed is the only value-preserving sign extension
        return v.Args[0].Type.IsSigned() && v.Type.IsSigned();
    else if (v.Op == OpZeroExt8to16 || v.Op == OpZeroExt8to32 || v.Op == OpZeroExt8to64 || v.Op == OpZeroExt16to32 || v.Op == OpZeroExt16to64 || v.Op == OpZeroExt32to64) 
        // unsigned -> signed/unsigned are value-preserving zero extensions
        return !v.Args[0].Type.IsSigned();
        return false;

}

} // end ssa_package
