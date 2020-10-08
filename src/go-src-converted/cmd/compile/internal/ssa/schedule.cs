// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 08 04:26:33 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\schedule.go
using types = go.cmd.compile.@internal.types_package;
using heap = go.container.heap_package;
using sort = go.sort_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        public static readonly var ScorePhi = (var)iota; // towards top of block
        public static readonly var ScoreArg = (var)0;
        public static readonly var ScoreNilCheck = (var)1;
        public static readonly var ScoreReadTuple = (var)2;
        public static readonly var ScoreVarDef = (var)3;
        public static readonly var ScoreMemory = (var)4;
        public static readonly var ScoreReadFlags = (var)5;
        public static readonly var ScoreDefault = (var)6;
        public static readonly var ScoreFlags = (var)7;
        public static readonly var ScoreControl = (var)8; // towards bottom of block

        public partial struct ValHeap
        {
            public slice<ptr<Value>> a;
            public slice<sbyte> score;
        }

        public static long Len(this ValHeap h)
        {
            return len(h.a);
        }
        public static void Swap(this ValHeap h, long i, long j)
        {
            var a = h.a;

            a[i] = a[j];
            a[j] = a[i];
        }

        private static void Push(this ptr<ValHeap> _addr_h, object x)
        {
            ref ValHeap h = ref _addr_h.val;
 
            // Push and Pop use pointer receivers because they modify the slice's length,
            // not just its contents.
            ptr<Value> v = x._<ptr<Value>>();
            h.a = append(h.a, v);

        }
        private static void Pop(this ptr<ValHeap> _addr_h)
        {
            ref ValHeap h = ref _addr_h.val;

            var old = h.a;
            var n = len(old);
            var x = old[n - 1L];
            h.a = old[0L..n - 1L];
            return x;
        }
        public static bool Less(this ValHeap h, long i, long j)
        {
            var x = h.a[i];
            var y = h.a[j];
            var sx = h.score[x.ID];
            var sy = h.score[y.ID];
            {
                var c__prev1 = c;

                var c = sx - sy;

                if (c != 0L)
                {
                    return c > 0L; // higher score comes later.
                }

                c = c__prev1;

            }

            if (x.Pos != y.Pos)
            { // Favor in-order line stepping
                return x.Pos.After(y.Pos);

            }

            if (x.Op != OpPhi)
            {
                {
                    var c__prev2 = c;

                    c = len(x.Args) - len(y.Args);

                    if (c != 0L)
                    {
                        return c < 0L; // smaller args comes later
                    }

                    c = c__prev2;

                }

            }

            {
                var c__prev1 = c;

                c = x.Uses - y.Uses;

                if (c != 0L)
                {
                    return c < 0L; // smaller uses come later
                } 
                // These comparisons are fairly arbitrary.
                // The goal here is stability in the face
                // of unrelated changes elsewhere in the compiler.

                c = c__prev1;

            } 
            // These comparisons are fairly arbitrary.
            // The goal here is stability in the face
            // of unrelated changes elsewhere in the compiler.
            {
                var c__prev1 = c;

                c = x.AuxInt - y.AuxInt;

                if (c != 0L)
                {
                    return c > 0L;
                }

                c = c__prev1;

            }

            {
                var cmp = x.Type.Compare(y.Type);

                if (cmp != types.CMPeq)
                {
                    return cmp == types.CMPgt;
                }

            }

            return x.ID > y.ID;

        }

        public static bool isLoweredGetClosurePtr(this Op op)
        {

            if (op == OpAMD64LoweredGetClosurePtr || op == OpPPC64LoweredGetClosurePtr || op == OpARMLoweredGetClosurePtr || op == OpARM64LoweredGetClosurePtr || op == Op386LoweredGetClosurePtr || op == OpMIPS64LoweredGetClosurePtr || op == OpS390XLoweredGetClosurePtr || op == OpMIPSLoweredGetClosurePtr || op == OpRISCV64LoweredGetClosurePtr || op == OpWasmLoweredGetClosurePtr) 
                return true;
                        return false;

        }

        // Schedule the Values in each Block. After this phase returns, the
        // order of b.Values matters and is the order in which those values
        // will appear in the assembly output. For now it generates a
        // reasonable valid schedule using a priority queue. TODO(khr):
        // schedule smarter.
        private static void schedule(ptr<Func> _addr_f)
        {
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
            var order = make_slice<ptr<Value>>(0L, 64L); 

            // maps mem values to the next live memory value
            var nextMem = make_slice<ptr<Value>>(f.NumValues()); 
            // additional pretend arguments for each Value. Used to enforce load/store ordering.
            var additionalArgs = make_slice<slice<ptr<Value>>>(f.NumValues());

            {
                var b__prev1 = b;

                foreach (var (_, __b) in f.Blocks)
                {
                    b = __b; 
                    // Compute score. Larger numbers are scheduled closer to the end of the block.
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in b.Values)
                        {
                            v = __v;

                            if (v.Op.isLoweredGetClosurePtr()) 
                                // We also score GetLoweredClosurePtr as early as possible to ensure that the
                                // context register is not stomped. GetLoweredClosurePtr should only appear
                                // in the entry block where there are no phi functions, so there is no
                                // conflict or ambiguity here.
                                if (b != f.Entry)
                                {
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
                            else if (v.Op == OpArg) 
                                // We want all the args as early as possible, for better debugging.
                                score[v.ID] = ScoreArg;
                            else if (v.Type.IsMemory()) 
                                // Schedule stores as early as possible. This tends to
                                // reduce register pressure. It also helps make sure
                                // VARDEF ops are scheduled before the corresponding LEA.
                                score[v.ID] = ScoreMemory;
                            else if (v.Op == OpSelect0 || v.Op == OpSelect1) 
                                // Schedule the pseudo-op of reading part of a tuple
                                // immediately after the tuple-generating op, since
                                // this value is already live. This also removes its
                                // false dependency on the other part of the tuple.
                                // Also ensures tuple is never spilled.
                                score[v.ID] = ScoreReadTuple;
                            else if (v.Type.IsFlags() || v.Type.IsTuple() && v.Type.FieldType(1L).IsFlags()) 
                                // Schedule flag register generation as late as possible.
                                // This makes sure that we only have one live flags
                                // value at a time.
                                score[v.ID] = ScoreFlags;
                            else 
                                score[v.ID] = ScoreDefault; 
                                // If we're reading flags, schedule earlier to keep flag lifetime short.
                                {
                                    var a__prev3 = a;

                                    foreach (var (_, __a) in v.Args)
                                    {
                                        a = __a;
                                        if (a.Type.IsFlags())
                                        {
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

                foreach (var (_, __b) in f.Blocks)
                {
                    b = __b; 
                    // Find store chain for block.
                    // Store chains for different blocks overwrite each other, so
                    // the calculated store chain is good only for this block.
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in b.Values)
                        {
                            v = __v;
                            if (v.Op != OpPhi && v.Type.IsMemory())
                            {
                                {
                                    var w__prev3 = w;

                                    foreach (var (_, __w) in v.Args)
                                    {
                                        w = __w;
                                        if (w.Type.IsMemory())
                                        {
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

                        foreach (var (_, __v) in b.Values)
                        {
                            v = __v;
                            if (v.Op == OpPhi)
                            { 
                                // If a value is used by a phi, it does not induce
                                // a scheduling edge because that use is from the
                                // previous iteration.
                                continue;

                            }

                            {
                                var w__prev3 = w;

                                foreach (var (_, __w) in v.Args)
                                {
                                    w = __w;
                                    if (w.Block == b)
                                    {
                                        uses[w.ID]++;
                                    } 
                                    // Any load must come before the following store.
                                    if (!v.Type.IsMemory() && w.Type.IsMemory())
                                    { 
                                        // v is a load.
                                        var s = nextMem[w.ID];
                                        if (s == null || s.Block != b)
                                        {
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

                    foreach (var (_, c) in b.ControlValues())
                    { 
                        // Force the control values to be scheduled at the end,
                        // unless they are phi values (which must be first).
                        // OpArg also goes first -- if it is stack it register allocates
                        // to a LoadReg, if it is register it is from the beginning anyway.
                        if (c.Op == OpPhi || c.Op == OpArg)
                        {
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

                            foreach (var (_, __v) in b.Values)
                            {
                                v = __v;
                                if (v.Op != OpPhi)
                                {
                                    {
                                        var a__prev4 = a;

                                        foreach (var (_, __a) in v.Args)
                                        {
                                            a = __a;
                                            if (a == c)
                                            {
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
                    priq.a = priq.a[..0L]; 

                    // Initialize priority queue with schedulable values.
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in b.Values)
                        {
                            v = __v;
                            if (uses[v.ID] == 0L)
                            {
                                heap.Push(priq, v);
                            }

                        } 

                        // Schedule highest priority value, update use counts, repeat.

                        v = v__prev2;
                    }

                    order = order[..0L];
                    var tuples = make_map<ID, slice<ptr<Value>>>();
                    while (priq.Len() > 0L)
                    { 
                        // Find highest priority schedulable value.
                        // Note that schedule is assembled backwards.

                        ptr<Value> v = heap.Pop(priq)._<ptr<Value>>(); 

                        // Add it to the schedule.
                        // Do not emit tuple-reading ops until we're ready to emit the tuple-generating op.
                        //TODO: maybe remove ReadTuple score above, if it does not help on performance

                        if (v.Op == OpSelect0)
                        {
                            if (tuples[v.Args[0L].ID] == null)
                            {
                                tuples[v.Args[0L].ID] = make_slice<ptr<Value>>(2L);
                            }

                            tuples[v.Args[0L].ID][0L] = v;
                            goto __switch_break0;
                        }
                        if (v.Op == OpSelect1)
                        {
                            if (tuples[v.Args[0L].ID] == null)
                            {
                                tuples[v.Args[0L].ID] = make_slice<ptr<Value>>(2L);
                            }

                            tuples[v.Args[0L].ID][1L] = v;
                            goto __switch_break0;
                        }
                        if (v.Type.IsTuple() && tuples[v.ID] != null)
                        {
                            if (tuples[v.ID][1L] != null)
                            {
                                order = append(order, tuples[v.ID][1L]);
                            }

                            if (tuples[v.ID][0L] != null)
                            {
                                order = append(order, tuples[v.ID][0L]);
                            }

                            delete(tuples, v.ID);
                        }
                        // default: 
                            order = append(order, v);

                        __switch_break0:; 

                        // Update use counts of arguments.
                        {
                            var w__prev3 = w;

                            foreach (var (_, __w) in v.Args)
                            {
                                w = __w;
                                if (w.Block != b)
                                {
                                    continue;
                                }

                                uses[w.ID]--;
                                if (uses[w.ID] == 0L)
                                { 
                                    // All uses scheduled, w is now schedulable.
                                    heap.Push(priq, w);

                                }

                            }

                            w = w__prev3;
                        }

                        {
                            var w__prev3 = w;

                            foreach (var (_, __w) in additionalArgs[v.ID])
                            {
                                w = __w;
                                uses[w.ID]--;
                                if (uses[w.ID] == 0L)
                                { 
                                    // All uses scheduled, w is now schedulable.
                                    heap.Push(priq, w);

                                }

                            }

                            w = w__prev3;
                        }
                    }

                    if (len(order) != len(b.Values))
                    {
                        f.Fatalf("schedule does not include all values in block %s", b);
                    }

                    for (long i = 0L; i < len(b.Values); i++)
                    {
                        b.Values[i] = order[len(b.Values) - 1L - i];
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
        private static slice<ptr<Value>> storeOrder(slice<ptr<Value>> values, ptr<sparseSet> _addr_sset, slice<int> storeNumber)
        {
            ref sparseSet sset = ref _addr_sset.val;

            if (len(values) == 0L)
            {
                return values;
            }

            var f = values[0L].Block.Func; 

            // find all stores

            // Members of values that are store values.
            // A constant bound allows this to be stack-allocated. 64 is
            // enough to cover almost every storeOrder call.
            var stores = make_slice<ptr<Value>>(0L, 64L);
            var hasNilCheck = false;
            sset.clear(); // sset is the set of stores that are used in other values
            {
                var v__prev1 = v;

                foreach (var (_, __v) in values)
                {
                    v = __v;
                    if (v.Type.IsMemory())
                    {
                        stores = append(stores, v);
                        if (v.Op == OpInitMem || v.Op == OpPhi)
                        {
                            continue;
                        }

                        sset.add(v.MemoryArg().ID); // record that v's memory arg is used
                    }

                    if (v.Op == OpNilCheck)
                    {
                        hasNilCheck = true;
                    }

                }

                v = v__prev1;
            }

            if (len(stores) == 0L || !hasNilCheck && f.pass.name == "nilcheckelim")
            { 
                // there is no store, the order does not matter
                return values;

            } 

            // find last store, which is the one that is not used by other stores
            ptr<Value> last;
            {
                var v__prev1 = v;

                foreach (var (_, __v) in stores)
                {
                    v = __v;
                    if (!sset.contains(v.ID))
                    {
                        if (last != null)
                        {
                            f.Fatalf("two stores live simultaneously: %v and %v", v, last);
                        }

                        last = v;

                    }

                } 

                // We assign a store number to each value. Store number is the
                // index of the latest store that this value transitively depends.
                // The i-th store in the current block gets store number 3*i. A nil
                // check that depends on the i-th store gets store number 3*i+1.
                // Other values that depends on the i-th store gets store number 3*i+2.
                // Special case: 0 -- unassigned, 1 or 2 -- the latest store it depends
                // is in the previous block (or no store at all, e.g. value is Const).
                // First we assign the number to all stores by walking back the store chain,
                // then assign the number to other values in DFS order.

                v = v__prev1;
            }

            var count = make_slice<int>(3L * (len(stores) + 1L));
            sset.clear(); // reuse sparse set to ensure that a value is pushed to stack only once
            {
                var n__prev1 = n;
                var w__prev1 = w;

                for (var n = len(stores);
                var w = last; n > 0L; n--)
                {
                    storeNumber[w.ID] = int32(3L * n);
                    count[3L * n]++;
                    sset.add(w.ID);
                    if (w.Op == OpInitMem || w.Op == OpPhi)
                    {
                        if (n != 1L)
                        {
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

                foreach (var (_, __v) in values)
                {
                    v = __v;
                    if (sset.contains(v.ID))
                    { 
                        // in sset means v is a store, or already pushed to stack, or already assigned a store number
                        continue;

                    }

                    stack = append(stack, v);
                    sset.add(v.ID);

                    while (len(stack) > 0L)
                    {
                        w = stack[len(stack) - 1L];
                        if (storeNumber[w.ID] != 0L)
                        {
                            stack = stack[..len(stack) - 1L];
                            continue;
                        }

                        if (w.Op == OpPhi)
                        { 
                            // Phi value doesn't depend on store in the current block.
                            // Do this early to avoid dependency cycle.
                            storeNumber[w.ID] = 2L;
                            count[2L]++;
                            stack = stack[..len(stack) - 1L];
                            continue;

                        }

                        var max = int32(0L); // latest store dependency
                        var argsdone = true;
                        foreach (var (_, a) in w.Args)
                        {
                            if (a.Block != w.Block)
                            {
                                continue;
                            }

                            if (!sset.contains(a.ID))
                            {
                                stack = append(stack, a);
                                sset.add(a.ID);
                                argsdone = false;
                                break;
                            }

                            if (storeNumber[a.ID] / 3L > max)
                            {
                                max = storeNumber[a.ID] / 3L;
                            }

                        }
                        if (!argsdone)
                        {
                            continue;
                        }

                        n = 3L * max + 2L;
                        if (w.Op == OpNilCheck)
                        {
                            n = 3L * max + 1L;
                        }

                        storeNumber[w.ID] = n;
                        count[n]++;
                        stack = stack[..len(stack) - 1L];

                    }


                } 

                // convert count to prefix sum of counts: count'[i] = sum_{j<=i} count[i]

                v = v__prev1;
            }

            {
                var i__prev1 = i;

                foreach (var (__i) in count)
                {
                    i = __i;
                    if (i == 0L)
                    {
                        continue;
                    }

                    count[i] += count[i - 1L];

                }

                i = i__prev1;
            }

            if (count[len(count) - 1L] != int32(len(values)))
            {
                f.Fatalf("storeOrder: value is missing, total count = %d, values = %v", count[len(count) - 1L], values);
            } 

            // place values in count-indexed bins, which are in the desired store order
            var order = make_slice<ptr<Value>>(len(values));
            {
                var v__prev1 = v;

                foreach (var (_, __v) in values)
                {
                    v = __v;
                    var s = storeNumber[v.ID];
                    order[count[s - 1L]] = v;
                    count[s - 1L]++;
                } 

                // Order nil checks in source order. We want the first in source order to trigger.
                // If two are on the same line, we don't really care which happens first.
                // See issue 18169.

                v = v__prev1;
            }

            if (hasNilCheck)
            {
                long start = -1L;
                {
                    var i__prev1 = i;
                    var v__prev1 = v;

                    foreach (var (__i, __v) in order)
                    {
                        i = __i;
                        v = __v;
                        if (v.Op == OpNilCheck)
                        {
                            if (start == -1L)
                            {
                                start = i;
                            }

                        }
                        else
                        {
                            if (start != -1L)
                            {
                                sort.Sort(bySourcePos(order[start..i]));
                                start = -1L;
                            }

                        }

                    }

                    i = i__prev1;
                    v = v__prev1;
                }

                if (start != -1L)
                {
                    sort.Sort(bySourcePos(order[start..]));
                }

            }

            return order;

        }

        private partial struct bySourcePos // : slice<ptr<Value>>
        {
        }

        private static long Len(this bySourcePos s)
        {
            return len(s);
        }
        private static void Swap(this bySourcePos s, long i, long j)
        {
            s[i] = s[j];
            s[j] = s[i];
        }
        private static bool Less(this bySourcePos s, long i, long j)
        {
            return s[i].Pos.Before(s[j].Pos);
        }
    }
}}}}
