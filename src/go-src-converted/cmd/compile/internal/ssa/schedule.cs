// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 13 06:21:57 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\schedule.go
namespace go.cmd.compile.@internal;

using types = cmd.compile.@internal.types_package;
using heap = container.heap_package;
using sort = sort_package;

public static partial class ssa_package {

public static readonly var ScorePhi = iota; // towards top of block
public static readonly var ScoreArg = 0;
public static readonly var ScoreNilCheck = 1;
public static readonly var ScoreReadTuple = 2;
public static readonly var ScoreVarDef = 3;
public static readonly var ScoreMemory = 4;
public static readonly var ScoreReadFlags = 5;
public static readonly var ScoreDefault = 6;
public static readonly var ScoreFlags = 7;
public static readonly var ScoreControl = 8; // towards bottom of block

public partial struct ValHeap {
    public slice<ptr<Value>> a;
    public slice<sbyte> score;
}

public static nint Len(this ValHeap h) {
    return len(h.a);
}
public static void Swap(this ValHeap h, nint i, nint j) {
    var a = h.a;

    (a[i], a[j]) = (a[j], a[i]);
}

private static void Push(this ptr<ValHeap> _addr_h, object x) {
    ref ValHeap h = ref _addr_h.val;
 
    // Push and Pop use pointer receivers because they modify the slice's length,
    // not just its contents.
    ptr<Value> v = x._<ptr<Value>>();
    h.a = append(h.a, v);
}
private static void Pop(this ptr<ValHeap> _addr_h) {
    ref ValHeap h = ref _addr_h.val;

    var old = h.a;
    var n = len(old);
    var x = old[n - 1];
    h.a = old[(int)0..(int)n - 1];
    return x;
}
public static bool Less(this ValHeap h, nint i, nint j) {
    var x = h.a[i];
    var y = h.a[j];
    var sx = h.score[x.ID];
    var sy = h.score[y.ID];
    {
        var c__prev1 = c;

        var c = sx - sy;

        if (c != 0) {
            return c > 0; // higher score comes later.
        }
        c = c__prev1;

    }
    if (x.Pos != y.Pos) { // Favor in-order line stepping
        return x.Pos.After(y.Pos);
    }
    if (x.Op != OpPhi) {
        {
            var c__prev2 = c;

            c = len(x.Args) - len(y.Args);

            if (c != 0) {
                return c < 0; // smaller args comes later
            }

            c = c__prev2;

        }
    }
    {
        var c__prev1 = c;

        c = x.Uses - y.Uses;

        if (c != 0) {
            return c < 0; // smaller uses come later
        }
        c = c__prev1;

    } 
    // These comparisons are fairly arbitrary.
    // The goal here is stability in the face
    // of unrelated changes elsewhere in the compiler.
    {
        var c__prev1 = c;

        c = x.AuxInt - y.AuxInt;

        if (c != 0) {
            return c > 0;
        }
        c = c__prev1;

    }
    {
        var cmp = x.Type.Compare(y.Type);

        if (cmp != types.CMPeq) {
            return cmp == types.CMPgt;
        }
    }
    return x.ID > y.ID;
}

public static bool isLoweredGetClosurePtr(this Op op) {

    if (op == OpAMD64LoweredGetClosurePtr || op == OpPPC64LoweredGetClosurePtr || op == OpARMLoweredGetClosurePtr || op == OpARM64LoweredGetClosurePtr || op == Op386LoweredGetClosurePtr || op == OpMIPS64LoweredGetClosurePtr || op == OpS390XLoweredGetClosurePtr || op == OpMIPSLoweredGetClosurePtr || op == OpRISCV64LoweredGetClosurePtr || op == OpWasmLoweredGetClosurePtr) 
        return true;
        return false;
}

// Schedule the Values in each Block. After this phase returns, the
// order of b.Values matters and is the order in which those values
// will appear in the assembly output. For now it generates a
// reasonable valid schedule using a priority queue. TODO(khr):
// schedule smarter.
private static void schedule(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;
 
    // For each value, the number of times it is used in the block
    // by values that have not been scheduled yet.
    var uses = make_slice<int>(f.NumValues()); 

    // reusable priority queue
    ptr<ValHeap> priq = @new<ValHeap>(); 

    // "priority" for a value
    var score = make_slice<sbyte>(f.NumValues()); 

    // scheduling order. We queue values in this list in reverse order.
    // A constant bound allows this to be stack-allocated. 64 is
    // enough to cover almost every schedule call.
    var order = make_slice<ptr<Value>>(0, 64); 

    // maps mem values to the next live memory value
    var nextMem = make_slice<ptr<Value>>(f.NumValues()); 
    // additional pretend arguments for each Value. Used to enforce load/store ordering.
    var additionalArgs = make_slice<slice<ptr<Value>>>(f.NumValues());

    {
        var b__prev1 = b;

        foreach (var (_, __b) in f.Blocks) {
            b = __b; 
            // Compute score. Larger numbers are scheduled closer to the end of the block.
            {
                var v__prev2 = v;

                foreach (var (_, __v) in b.Values) {
                    v = __v;

                    if (v.Op.isLoweredGetClosurePtr()) 
                        // We also score GetLoweredClosurePtr as early as possible to ensure that the
                        // context register is not stomped. GetLoweredClosurePtr should only appear
                        // in the entry block where there are no phi functions, so there is no
                        // conflict or ambiguity here.
                        if (b != f.Entry) {
                            f.Fatalf("LoweredGetClosurePtr appeared outside of entry block, b=%s", b.String());
                        }
                        score[v.ID] = ScorePhi;
                    else if (v.Op == OpAMD64LoweredNilCheck || v.Op == OpPPC64LoweredNilCheck || v.Op == OpARMLoweredNilCheck || v.Op == OpARM64LoweredNilCheck || v.Op == Op386LoweredNilCheck || v.Op == OpMIPS64LoweredNilCheck || v.Op == OpS390XLoweredNilCheck || v.Op == OpMIPSLoweredNilCheck || v.Op == OpRISCV64LoweredNilCheck || v.Op == OpWasmLoweredNilCheck) 
                        // Nil checks must come before loads from the same address.
                        score[v.ID] = ScoreNilCheck;
                    else if (v.Op == OpPhi) 
                        // We want all the phis first.
                        score[v.ID] = ScorePhi;
                    else if (v.Op == OpVarDef) 
                        // We want all the vardefs next.
                        score[v.ID] = ScoreVarDef;
                    else if (v.Op == OpArgIntReg || v.Op == OpArgFloatReg) 
                        // In-register args must be scheduled as early as possible to ensure that the
                        // context register is not stomped. They should only appear in the entry block.
                        if (b != f.Entry) {
                            f.Fatalf("%s appeared outside of entry block, b=%s", v.Op, b.String());
                        }
                        score[v.ID] = ScorePhi;
                    else if (v.Op == OpArg) 
                        // We want all the args as early as possible, for better debugging.
                        score[v.ID] = ScoreArg;
                    else if (v.Type.IsMemory()) 
                        // Schedule stores as early as possible. This tends to
                        // reduce register pressure. It also helps make sure
                        // VARDEF ops are scheduled before the corresponding LEA.
                        score[v.ID] = ScoreMemory;
                    else if (v.Op == OpSelect0 || v.Op == OpSelect1 || v.Op == OpSelectN) 
                        // Schedule the pseudo-op of reading part of a tuple
                        // immediately after the tuple-generating op, since
                        // this value is already live. This also removes its
                        // false dependency on the other part of the tuple.
                        // Also ensures tuple is never spilled.
                        score[v.ID] = ScoreReadTuple;
                    else if (v.Type.IsFlags() || v.Type.IsTuple() && v.Type.FieldType(1).IsFlags()) 
                        // Schedule flag register generation as late as possible.
                        // This makes sure that we only have one live flags
                        // value at a time.
                        score[v.ID] = ScoreFlags;
                    else 
                        score[v.ID] = ScoreDefault; 
                        // If we're reading flags, schedule earlier to keep flag lifetime short.
                        {
                            var a__prev3 = a;

                            foreach (var (_, __a) in v.Args) {
                                a = __a;
                                if (a.Type.IsFlags()) {
                                    score[v.ID] = ScoreReadFlags;
                                }
                            }

                            a = a__prev3;
                        }
                                    }

                v = v__prev2;
            }
        }
        b = b__prev1;
    }

    {
        var b__prev1 = b;

        foreach (var (_, __b) in f.Blocks) {
            b = __b; 
            // Find store chain for block.
            // Store chains for different blocks overwrite each other, so
            // the calculated store chain is good only for this block.
            {
                var v__prev2 = v;

                foreach (var (_, __v) in b.Values) {
                    v = __v;
                    if (v.Op != OpPhi && v.Type.IsMemory()) {
                        {
                            var w__prev3 = w;

                            foreach (var (_, __w) in v.Args) {
                                w = __w;
                                if (w.Type.IsMemory()) {
                                    nextMem[w.ID] = v;
                                }
                            }

                            w = w__prev3;
                        }
                    }
                } 

                // Compute uses.

                v = v__prev2;
            }

            {
                var v__prev2 = v;

                foreach (var (_, __v) in b.Values) {
                    v = __v;
                    if (v.Op == OpPhi) { 
                        // If a value is used by a phi, it does not induce
                        // a scheduling edge because that use is from the
                        // previous iteration.
                        continue;
                    }
                    {
                        var w__prev3 = w;

                        foreach (var (_, __w) in v.Args) {
                            w = __w;
                            if (w.Block == b) {
                                uses[w.ID]++;
                            } 
                            // Any load must come before the following store.
                            if (!v.Type.IsMemory() && w.Type.IsMemory()) { 
                                // v is a load.
                                var s = nextMem[w.ID];
                                if (s == null || s.Block != b) {
                                    continue;
                                }
                                additionalArgs[s.ID] = append(additionalArgs[s.ID], v);
                                uses[v.ID]++;
                            }
                        }

                        w = w__prev3;
                    }
                }

                v = v__prev2;
            }

            foreach (var (_, c) in b.ControlValues()) { 
                // Force the control values to be scheduled at the end,
                // unless they are phi values (which must be first).
                // OpArg also goes first -- if it is stack it register allocates
                // to a LoadReg, if it is register it is from the beginning anyway.
                if (c.Op == OpPhi || c.Op == OpArg) {
                    continue;
                }
                score[c.ID] = ScoreControl; 

                // Schedule values dependent on the control values at the end.
                // This reduces the number of register spills. We don't find
                // all values that depend on the controls, just values with a
                // direct dependency. This is cheaper and in testing there
                // was no difference in the number of spills.
                {
                    var v__prev3 = v;

                    foreach (var (_, __v) in b.Values) {
                        v = __v;
                        if (v.Op != OpPhi) {
                            {
                                var a__prev4 = a;

                                foreach (var (_, __a) in v.Args) {
                                    a = __a;
                                    if (a == c) {
                                        score[v.ID] = ScoreControl;
                                    }
                                }

                                a = a__prev4;
                            }
                        }
                    }

                    v = v__prev3;
                }
            } 

            // To put things into a priority queue
            // The values that should come last are least.
            priq.score = score;
            priq.a = priq.a[..(int)0]; 

            // Initialize priority queue with schedulable values.
            {
                var v__prev2 = v;

                foreach (var (_, __v) in b.Values) {
                    v = __v;
                    if (uses[v.ID] == 0) {
                        heap.Push(priq, v);
                    }
                } 

                // Schedule highest priority value, update use counts, repeat.

                v = v__prev2;
            }

            order = order[..(int)0];
            var tuples = make_map<ID, slice<ptr<Value>>>();
            while (priq.Len() > 0) { 
                // Find highest priority schedulable value.
                // Note that schedule is assembled backwards.

                ptr<Value> v = heap.Pop(priq)._<ptr<Value>>(); 

                // Add it to the schedule.
                // Do not emit tuple-reading ops until we're ready to emit the tuple-generating op.
                //TODO: maybe remove ReadTuple score above, if it does not help on performance

                if (v.Op == OpSelect0)
                {
                    if (tuples[v.Args[0].ID] == null) {
                        tuples[v.Args[0].ID] = make_slice<ptr<Value>>(2);
                    }
                    tuples[v.Args[0].ID][0] = v;
                    goto __switch_break0;
                }
                if (v.Op == OpSelect1)
                {
                    if (tuples[v.Args[0].ID] == null) {
                        tuples[v.Args[0].ID] = make_slice<ptr<Value>>(2);
                    }
                    tuples[v.Args[0].ID][1] = v;
                    goto __switch_break0;
                }
                if (v.Op == OpSelectN)
                {
                    if (tuples[v.Args[0].ID] == null) {
                        tuples[v.Args[0].ID] = make_slice<ptr<Value>>(v.Args[0].Type.NumFields());
                    }
                    tuples[v.Args[0].ID][v.AuxInt] = v;
                    goto __switch_break0;
                }
                if (v.Type.IsResults() && tuples[v.ID] != null)
                {
                    var tup = tuples[v.ID];
                    {
                        var i__prev3 = i;

                        for (var i = len(tup) - 1; i >= 0; i--) {
                            if (tup[i] != null) {
                                order = append(order, tup[i]);
                            }
                        }


                        i = i__prev3;
                    }
                    delete(tuples, v.ID);
                    order = append(order, v);
                    goto __switch_break0;
                }
                if (v.Type.IsTuple() && tuples[v.ID] != null)
                {
                    if (tuples[v.ID][1] != null) {
                        order = append(order, tuples[v.ID][1]);
                    }
                    if (tuples[v.ID][0] != null) {
                        order = append(order, tuples[v.ID][0]);
                    }
                    delete(tuples, v.ID);
                }
                // default: 
                    order = append(order, v);

                __switch_break0:; 

                // Update use counts of arguments.
                {
                    var w__prev3 = w;

                    foreach (var (_, __w) in v.Args) {
                        w = __w;
                        if (w.Block != b) {
                            continue;
                        }
                        uses[w.ID]--;
                        if (uses[w.ID] == 0) { 
                            // All uses scheduled, w is now schedulable.
                            heap.Push(priq, w);
                        }
                    }

                    w = w__prev3;
                }

                {
                    var w__prev3 = w;

                    foreach (var (_, __w) in additionalArgs[v.ID]) {
                        w = __w;
                        uses[w.ID]--;
                        if (uses[w.ID] == 0) { 
                            // All uses scheduled, w is now schedulable.
                            heap.Push(priq, w);
                        }
                    }

                    w = w__prev3;
                }
            }

            if (len(order) != len(b.Values)) {
                f.Fatalf("schedule does not include all values in block %s", b);
            }
            {
                var i__prev2 = i;

                for (i = 0; i < len(b.Values); i++) {
                    b.Values[i] = order[len(b.Values) - 1 - i];
                }


                i = i__prev2;
            }
        }
        b = b__prev1;
    }

    f.scheduled = true;
}

// storeOrder orders values with respect to stores. That is,
// if v transitively depends on store s, v is ordered after s,
// otherwise v is ordered before s.
// Specifically, values are ordered like
//   store1
//   NilCheck that depends on store1
//   other values that depends on store1
//   store2
//   NilCheck that depends on store2
//   other values that depends on store2
//   ...
// The order of non-store and non-NilCheck values are undefined
// (not necessarily dependency order). This should be cheaper
// than a full scheduling as done above.
// Note that simple dependency order won't work: there is no
// dependency between NilChecks and values like IsNonNil.
// Auxiliary data structures are passed in as arguments, so
// that they can be allocated in the caller and be reused.
// This function takes care of reset them.
private static slice<ptr<Value>> storeOrder(slice<ptr<Value>> values, ptr<sparseSet> _addr_sset, slice<int> storeNumber) {
    ref sparseSet sset = ref _addr_sset.val;

    if (len(values) == 0) {
        return values;
    }
    var f = values[0].Block.Func; 

    // find all stores

    // Members of values that are store values.
    // A constant bound allows this to be stack-allocated. 64 is
    // enough to cover almost every storeOrder call.
    var stores = make_slice<ptr<Value>>(0, 64);
    var hasNilCheck = false;
    sset.clear(); // sset is the set of stores that are used in other values
    {
        var v__prev1 = v;

        foreach (var (_, __v) in values) {
            v = __v;
            if (v.Type.IsMemory()) {
                stores = append(stores, v);
                if (v.Op == OpInitMem || v.Op == OpPhi) {
                    continue;
                }
                sset.add(v.MemoryArg().ID); // record that v's memory arg is used
            }
            if (v.Op == OpNilCheck) {
                hasNilCheck = true;
            }
        }
        v = v__prev1;
    }

    if (len(stores) == 0 || !hasNilCheck && f.pass.name == "nilcheckelim") { 
        // there is no store, the order does not matter
        return values;
    }
    ptr<Value> last;
    {
        var v__prev1 = v;

        foreach (var (_, __v) in stores) {
            v = __v;
            if (!sset.contains(v.ID)) {
                if (last != null) {
                    f.Fatalf("two stores live simultaneously: %v and %v", v, last);
                }
                last = v;
            }
        }
        v = v__prev1;
    }

    var count = make_slice<int>(3 * (len(stores) + 1));
    sset.clear(); // reuse sparse set to ensure that a value is pushed to stack only once
    {
        var n__prev1 = n;
        var w__prev1 = w;

        for (var n = len(stores);
        var w = last; n > 0; n--) {
            storeNumber[w.ID] = int32(3 * n);
            count[3 * n]++;
            sset.add(w.ID);
            if (w.Op == OpInitMem || w.Op == OpPhi) {
                if (n != 1) {
                    f.Fatalf("store order is wrong: there are stores before %v", w);
                }
                break;
            }
            w = w.MemoryArg();
        }

        n = n__prev1;
        w = w__prev1;
    }
    slice<ptr<Value>> stack = default;
    {
        var v__prev1 = v;

        foreach (var (_, __v) in values) {
            v = __v;
            if (sset.contains(v.ID)) { 
                // in sset means v is a store, or already pushed to stack, or already assigned a store number
                continue;
            }
            stack = append(stack, v);
            sset.add(v.ID);

            while (len(stack) > 0) {
                w = stack[len(stack) - 1];
                if (storeNumber[w.ID] != 0) {
                    stack = stack[..(int)len(stack) - 1];
                    continue;
                }
                if (w.Op == OpPhi) { 
                    // Phi value doesn't depend on store in the current block.
                    // Do this early to avoid dependency cycle.
                    storeNumber[w.ID] = 2;
                    count[2]++;
                    stack = stack[..(int)len(stack) - 1];
                    continue;
                }
                var max = int32(0); // latest store dependency
                var argsdone = true;
                foreach (var (_, a) in w.Args) {
                    if (a.Block != w.Block) {
                        continue;
                    }
                    if (!sset.contains(a.ID)) {
                        stack = append(stack, a);
                        sset.add(a.ID);
                        argsdone = false;
                        break;
                    }
                    if (storeNumber[a.ID] / 3 > max) {
                        max = storeNumber[a.ID] / 3;
                    }
                }
                if (!argsdone) {
                    continue;
                }
                n = 3 * max + 2;
                if (w.Op == OpNilCheck) {
                    n = 3 * max + 1;
                }
                storeNumber[w.ID] = n;
                count[n]++;
                stack = stack[..(int)len(stack) - 1];
            }
        }
        v = v__prev1;
    }

    {
        var i__prev1 = i;

        foreach (var (__i) in count) {
            i = __i;
            if (i == 0) {
                continue;
            }
            count[i] += count[i - 1];
        }
        i = i__prev1;
    }

    if (count[len(count) - 1] != int32(len(values))) {
        f.Fatalf("storeOrder: value is missing, total count = %d, values = %v", count[len(count) - 1], values);
    }
    var order = make_slice<ptr<Value>>(len(values));
    {
        var v__prev1 = v;

        foreach (var (_, __v) in values) {
            v = __v;
            var s = storeNumber[v.ID];
            order[count[s - 1]] = v;
            count[s - 1]++;
        }
        v = v__prev1;
    }

    if (hasNilCheck) {
        nint start = -1;
        {
            var i__prev1 = i;
            var v__prev1 = v;

            foreach (var (__i, __v) in order) {
                i = __i;
                v = __v;
                if (v.Op == OpNilCheck) {
                    if (start == -1) {
                        start = i;
                    }
                }
                else
 {
                    if (start != -1) {
                        sort.Sort(bySourcePos(order[(int)start..(int)i]));
                        start = -1;
                    }
                }
            }

            i = i__prev1;
            v = v__prev1;
        }

        if (start != -1) {
            sort.Sort(bySourcePos(order[(int)start..]));
        }
    }
    return order;
}

private partial struct bySourcePos { // : slice<ptr<Value>>
}

private static nint Len(this bySourcePos s) {
    return len(s);
}
private static void Swap(this bySourcePos s, nint i, nint j) {
    (s[i], s[j]) = (s[j], s[i]);
}
private static bool Less(this bySourcePos s, nint i, nint j) {
    return s[i].Pos.Before(s[j].Pos);
}

} // end ssa_package
