// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 08 04:10:09 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\cse.go
using types = go.cmd.compile.@internal.types_package;
using src = go.cmd.@internal.src_package;
using fmt = go.fmt_package;
using sort = go.sort_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        // cse does common-subexpression elimination on the Function.
        // Values are just relinked, nothing is deleted. A subsequent deadcode
        // pass is required to actually remove duplicate expressions.
        private static void cse(ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;
 
            // Two values are equivalent if they satisfy the following definition:
            // equivalent(v, w):
            //   v.op == w.op
            //   v.type == w.type
            //   v.aux == w.aux
            //   v.auxint == w.auxint
            //   len(v.args) == len(w.args)
            //   v.block == w.block if v.op == OpPhi
            //   equivalent(v.args[i], w.args[i]) for i in 0..len(v.args)-1

            // The algorithm searches for a partition of f's values into
            // equivalence classes using the above definition.
            // It starts with a coarse partition and iteratively refines it
            // until it reaches a fixed point.

            // Make initial coarse partitions by using a subset of the conditions above.
            var a = make_slice<ptr<Value>>(0L, f.NumValues());
            if (f.auxmap == null)
            {
                f.auxmap = new auxmap();
            }
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
                            if (v.Type.IsMemory())
                            {
                                continue; // memory values can never cse
                            }
                            if (f.auxmap[v.Aux] == 0L)
                            {
                                f.auxmap[v.Aux] = int32(len(f.auxmap)) + 1L;
                            }
                            a = append(a, v);

                        }
                        v = v__prev2;
                    }
                }
                b = b__prev1;
            }

            var partition = partitionValues(a, f.auxmap); 

            // map from value id back to eqclass id
            var valueEqClass = make_slice<ID>(f.NumValues());
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
                            // Use negative equivalence class #s for unique values.
                            valueEqClass[v.ID] = -v.ID;

                        }
                        v = v__prev2;
                    }
                }
                b = b__prev1;
            }

            ID pNum = 1L;
            {
                var e__prev1 = e;

                foreach (var (_, __e) in partition)
                {
                    e = __e;
                    if (f.pass.debug > 1L && len(e) > 500L)
                    {
                        fmt.Printf("CSE.large partition (%d): ", len(e));
                        {
                            long j__prev2 = j;

                            for (long j = 0L; j < 3L; j++)
                            {
                                fmt.Printf("%s ", e[j].LongString());
                            }

                            j = j__prev2;
                        }
                        fmt.Println();

                    }
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in e)
                        {
                            v = __v;
                            valueEqClass[v.ID] = pNum;
                        }
                        v = v__prev2;
                    }

                    if (f.pass.debug > 2L && len(e) > 1L)
                    {
                        fmt.Printf("CSE.partition #%d:", pNum);
                        {
                            var v__prev2 = v;

                            foreach (var (_, __v) in e)
                            {
                                v = __v;
                                fmt.Printf(" %s", v.String());
                            }
                            v = v__prev2;
                        }

                        fmt.Printf("\n");

                    }
                    pNum++;

                }
                e = e__prev1;
            }

            slice<long> splitPoints = default;
            ptr<object> byArgClass = @new<partitionByArgClass>(); // reuseable partitionByArgClass to reduce allocations
            while (true)
            {
                var changed = false; 

                // partition can grow in the loop. By not using a range loop here,
                // we process new additions as they arrive, avoiding O(n^2) behavior.
                {
                    long i__prev2 = i;

                    for (long i = 0L; i < len(partition); i++)
                    {
                        var e = partition[i];

                        if (opcodeTable[e[0L].Op].commutative)
                        { 
                            // Order the first two args before comparison.
                            {
                                var v__prev3 = v;

                                foreach (var (_, __v) in e)
                                {
                                    v = __v;
                                    if (valueEqClass[v.Args[0L].ID] > valueEqClass[v.Args[1L].ID])
                                    {
                                        v.Args[0L] = v.Args[1L];
                                        v.Args[1L] = v.Args[0L];

                                    }
                                }
                                v = v__prev3;
                            }
                        }
                        byArgClass.a = e;
                        byArgClass.eqClass = valueEqClass;
                        sort.Sort(byArgClass); 

                        // Find split points.
                        splitPoints = append(splitPoints[..0L], 0L);
                        {
                            long j__prev3 = j;

                            for (j = 1L; j < len(e); j++)
                            {
                                var v = e[j - 1L];
                                var w = e[j]; 
                                // Note: commutative args already correctly ordered by byArgClass.
                                var eqArgs = true;
                                {
                                    var a__prev4 = a;

                                    foreach (var (__k, __a) in v.Args)
                                    {
                                        k = __k;
                                        a = __a;
                                        var b = w.Args[k];
                                        if (valueEqClass[a.ID] != valueEqClass[b.ID])
                                        {
                                            eqArgs = false;
                                            break;
                                        }
                                    }
                                    a = a__prev4;
                                }

                                if (!eqArgs)
                                {
                                    splitPoints = append(splitPoints, j);
                                }
                            }

                            j = j__prev3;
                        }
                        if (len(splitPoints) == 1L)
                        {
                            continue; // no splits, leave equivalence class alone.
                        }
                        partition[i] = partition[len(partition) - 1L];
                        partition = partition[..len(partition) - 1L];
                        i--; 

                        // Add new equivalence classes for the parts of e we found.
                        splitPoints = append(splitPoints, len(e));
                        {
                            long j__prev3 = j;

                            for (j = 0L; j < len(splitPoints) - 1L; j++)
                            {
                                var f = e[splitPoints[j]..splitPoints[j + 1L]];
                                if (len(f) == 1L)
                                { 
                                    // Don't add singletons.
                                    valueEqClass[f[0L].ID] = -f[0L].ID;
                                    continue;

                                }
                                {
                                    var v__prev4 = v;

                                    foreach (var (_, __v) in f)
                                    {
                                        v = __v;
                                        valueEqClass[v.ID] = pNum;
                                    }
                                    v = v__prev4;
                                }

                                pNum++;
                                partition = append(partition, f);

                            }

                            j = j__prev3;
                        }
                        changed = true;

                    }

                    i = i__prev2;
                }

                if (!changed)
                {
                    break;
                }
            }

            var sdom = f.Sdom(); 

            // Compute substitutions we would like to do. We substitute v for w
            // if v and w are in the same equivalence class and v dominates w.
            var rewrite = make_slice<ptr<Value>>(f.NumValues());
            ptr<object> byDom = @new<partitionByDom>(); // reusable partitionByDom to reduce allocs
            {
                var e__prev1 = e;

                foreach (var (_, __e) in partition)
                {
                    e = __e;
                    byDom.a = e;
                    byDom.sdom = sdom;
                    sort.Sort(byDom);
                    {
                        long i__prev2 = i;

                        for (i = 0L; i < len(e) - 1L; i++)
                        { 
                            // e is sorted by domorder, so a maximal dominant element is first in the slice
                            v = e[i];
                            if (v == null)
                            {
                                continue;
                            }
                            e[i] = null; 
                            // Replace all elements of e which v dominates
                            {
                                long j__prev3 = j;

                                for (j = i + 1L; j < len(e); j++)
                                {
                                    w = e[j];
                                    if (w == null)
                                    {
                                        continue;
                                    }
                                    if (sdom.IsAncestorEq(v.Block, w.Block))
                                    {
                                        rewrite[w.ID] = v;
                                        e[j] = null;
                                    }
                                    else
                                    { 
                                        // e is sorted by domorder, so v.Block doesn't dominate any subsequent blocks in e
                                        break;

                                    }
                                }

                                j = j__prev3;
                            }

                        }

                        i = i__prev2;
                    }

                }
                e = e__prev1;
            }

            var rewrites = int64(0L); 

            // Apply substitutions
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
                            {
                                long i__prev3 = i;
                                var w__prev3 = w;

                                foreach (var (__i, __w) in v.Args)
                                {
                                    i = __i;
                                    w = __w;
                                    {
                                        var x__prev1 = x;

                                        var x = rewrite[w.ID];

                                        if (x != null)
                                        {
                                            if (w.Pos.IsStmt() == src.PosIsStmt)
                                            { 
                                                // about to lose a statement marker, w
                                                // w is an input to v; if they're in the same block
                                                // and the same line, v is a good-enough new statement boundary.
                                                if (w.Block == v.Block && w.Pos.Line() == v.Pos.Line())
                                                {
                                                    v.Pos = v.Pos.WithIsStmt();
                                                    w.Pos = w.Pos.WithNotStmt();
                                                }
                                            }
                                            v.SetArg(i, x);
                                            rewrites++;

                                        }
                                        x = x__prev1;

                                    }

                                }
                                i = i__prev3;
                                w = w__prev3;
                            }
                        }
                        v = v__prev2;
                    }

                    {
                        long i__prev2 = i;
                        var v__prev2 = v;

                        foreach (var (__i, __v) in b.ControlValues())
                        {
                            i = __i;
                            v = __v;
                            {
                                var x__prev1 = x;

                                x = rewrite[v.ID];

                                if (x != null)
                                {
                                    if (v.Op == OpNilCheck)
                                    { 
                                        // nilcheck pass will remove the nil checks and log
                                        // them appropriately, so don't mess with them here.
                                        continue;

                                    }
                                    b.ReplaceControl(i, x);

                                }
                                x = x__prev1;

                            }

                        }
                        i = i__prev2;
                        v = v__prev2;
                    }
                }
                b = b__prev1;
            }

            if (f.pass.stats > 0L)
            {
                f.LogStat("CSE REWRITES", rewrites);
            }
        }

        // An eqclass approximates an equivalence class. During the
        // algorithm it may represent the union of several of the
        // final equivalence classes.
        private partial struct eqclass // : slice<ptr<Value>>
        {
        }

        // partitionValues partitions the values into equivalence classes
        // based on having all the following features match:
        //  - opcode
        //  - type
        //  - auxint
        //  - aux
        //  - nargs
        //  - block # if a phi op
        //  - first two arg's opcodes and auxint
        //  - NOT first two arg's aux; that can break CSE.
        // partitionValues returns a list of equivalence classes, each
        // being a sorted by ID list of *Values. The eqclass slices are
        // backed by the same storage as the input slice.
        // Equivalence classes of size 1 are ignored.
        private static slice<eqclass> partitionValues(slice<ptr<Value>> a, auxmap auxIDs)
        {
            sort.Sort(new sortvalues(a,auxIDs));

            slice<eqclass> partition = default;
            while (len(a) > 0L)
            {
                var v = a[0L];
                long j = 1L;
                while (j < len(a))
                {
                    var w = a[j];
                    if (cmpVal(_addr_v, _addr_w, auxIDs) != types.CMPeq)
                    {
                        break;
                    j++;
                    }

                }

                if (j > 1L)
                {
                    partition = append(partition, a[..j]);
                }

                a = a[j..];

            }


            return partition;

        }
        private static types.Cmp lt2Cmp(bool isLt)
        {
            if (isLt)
            {
                return types.CMPlt;
            }

            return types.CMPgt;

        }

        private static types.Cmp cmpVal(ptr<Value> _addr_v, ptr<Value> _addr_w, auxmap auxIDs)
        {
            ref Value v = ref _addr_v.val;
            ref Value w = ref _addr_w.val;
 
            // Try to order these comparison by cost (cheaper first)
            if (v.Op != w.Op)
            {
                return lt2Cmp(v.Op < w.Op);
            }

            if (v.AuxInt != w.AuxInt)
            {
                return lt2Cmp(v.AuxInt < w.AuxInt);
            }

            if (len(v.Args) != len(w.Args))
            {
                return lt2Cmp(len(v.Args) < len(w.Args));
            }

            if (v.Op == OpPhi && v.Block != w.Block)
            {
                return lt2Cmp(v.Block.ID < w.Block.ID);
            }

            if (v.Type.IsMemory())
            { 
                // We will never be able to CSE two values
                // that generate memory.
                return lt2Cmp(v.ID < w.ID);

            } 
            // OpSelect is a pseudo-op. We need to be more aggressive
            // regarding CSE to keep multiple OpSelect's of the same
            // argument from existing.
            if (v.Op != OpSelect0 && v.Op != OpSelect1)
            {
                {
                    var tc = v.Type.Compare(w.Type);

                    if (tc != types.CMPeq)
                    {
                        return tc;
                    }

                }

            }

            if (v.Aux != w.Aux)
            {
                if (v.Aux == null)
                {
                    return types.CMPlt;
                }

                if (w.Aux == null)
                {
                    return types.CMPgt;
                }

                return lt2Cmp(auxIDs[v.Aux] < auxIDs[w.Aux]);

            }

            return types.CMPeq;

        }

        // Sort values to make the initial partition.
        private partial struct sortvalues
        {
            public slice<ptr<Value>> a; // array of values
            public auxmap auxIDs; // aux -> aux ID map
        }

        private static long Len(this sortvalues sv)
        {
            return len(sv.a);
        }
        private static void Swap(this sortvalues sv, long i, long j)
        {
            sv.a[i] = sv.a[j];
            sv.a[j] = sv.a[i];
        }
        private static bool Less(this sortvalues sv, long i, long j)
        {
            var v = sv.a[i];
            var w = sv.a[j];
            {
                var cmp = cmpVal(_addr_v, _addr_w, sv.auxIDs);

                if (cmp != types.CMPeq)
                {
                    return cmp == types.CMPlt;
                } 

                // Sort by value ID last to keep the sort result deterministic.

            } 

            // Sort by value ID last to keep the sort result deterministic.
            return v.ID < w.ID;

        }

        private partial struct partitionByDom
        {
            public slice<ptr<Value>> a; // array of values
            public SparseTree sdom;
        }

        private static long Len(this partitionByDom sv)
        {
            return len(sv.a);
        }
        private static void Swap(this partitionByDom sv, long i, long j)
        {
            sv.a[i] = sv.a[j];
            sv.a[j] = sv.a[i];
        }
        private static bool Less(this partitionByDom sv, long i, long j)
        {
            var v = sv.a[i];
            var w = sv.a[j];
            return sv.sdom.domorder(v.Block) < sv.sdom.domorder(w.Block);
        }

        private partial struct partitionByArgClass
        {
            public slice<ptr<Value>> a; // array of values
            public slice<ID> eqClass; // equivalence class IDs of values
        }

        private static long Len(this partitionByArgClass sv)
        {
            return len(sv.a);
        }
        private static void Swap(this partitionByArgClass sv, long i, long j)
        {
            sv.a[i] = sv.a[j];
            sv.a[j] = sv.a[i];
        }
        private static bool Less(this partitionByArgClass sv, long i, long j)
        {
            var v = sv.a[i];
            var w = sv.a[j];
            foreach (var (i, a) in v.Args)
            {
                var b = w.Args[i];
                if (sv.eqClass[a.ID] < sv.eqClass[b.ID])
                {
                    return true;
                }

                if (sv.eqClass[a.ID] > sv.eqClass[b.ID])
                {
                    return false;
                }

            }
            return false;

        }
    }
}}}}
