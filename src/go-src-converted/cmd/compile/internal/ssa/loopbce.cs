// package ssa -- go2cs converted at 2020 August 29 08:54:03 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\loopbce.go

using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        private partial struct indVar
        {
            public ptr<Value> ind; // induction variable
            public ptr<Value> inc; // increment, a constant
            public ptr<Value> nxt; // ind+inc variable
            public ptr<Value> min; // minimum value. inclusive,
            public ptr<Value> max; // maximum value. exclusive.
            public ptr<Block> entry; // entry block in the loop.
// Invariants: for all blocks dominated by entry:
//    min <= ind < max
//    min <= nxt <= max
        }

        // findIndVar finds induction variables in a function.
        //
        // Look for variables and blocks that satisfy the following
        //
        // loop:
        //   ind = (Phi min nxt),
        //   if ind < max
        //     then goto enter_loop
        //     else goto exit_loop
        //
        //   enter_loop:
        //    do something
        //      nxt = inc + ind
        //    goto loop
        //
        // exit_loop:
        //
        //
        // TODO: handle 32 bit operations
        private static slice<indVar> findIndVar(ref Func _f) => func(_f, (ref Func f, Defer _, Panic panic, Recover __) =>
        {
            slice<indVar> iv = default;
            var sdom = f.sdom();

nextb:

            foreach (var (_, b) in f.Blocks)
            {
                if (b.Kind != BlockIf || len(b.Preds) != 2L)
                {
                    continue;
                }
                ref Value ind = default;                ref Value max = default; // induction, and maximum
 // induction, and maximum
                long entry = -1L; // which successor of b enters the loop

                // Check thet the control if it either ind < max or max > ind.
                // TODO: Handle Leq64, Geq64.

                if (b.Control.Op == OpLess64) 
                    entry = 0L;
                    ind = b.Control.Args[0L];
                    max = b.Control.Args[1L];
                else if (b.Control.Op == OpGreater64) 
                    entry = 0L;
                    ind = b.Control.Args[1L];
                    max = b.Control.Args[0L];
                else 
                    _continuenextb = true;
                    break;
                // Check that the induction variable is a phi that depends on itself.
                if (ind.Op != OpPhi)
                {
                    continue;
                } 

                // Extract min and nxt knowing that nxt is an addition (e.g. Add64).
                ref Value min = default;                ref Value nxt = default; // minimum, and next value
 // minimum, and next value
                {
                    var n__prev1 = n;

                    var n = ind.Args[0L];

                    if (n.Op == OpAdd64 && (n.Args[0L] == ind || n.Args[1L] == ind))
                    {
                        min = ind.Args[1L];
                        nxt = n;
                    }                    {
                        var n__prev2 = n;

                        n = ind.Args[1L];


                        else if (n.Op == OpAdd64 && (n.Args[0L] == ind || n.Args[1L] == ind))
                        {
                            min = ind.Args[0L];
                            nxt = n;
                        }
                        else
                        { 
                            // Not a recognized induction variable.
                            continue;
                        }

                        n = n__prev2;

                    }


                    n = n__prev1;

                }

                ref Value inc = default;
                if (nxt.Args[0L] == ind)
                { // nxt = ind + inc
                    inc = nxt.Args[1L];
                }
                else if (nxt.Args[1L] == ind)
                { // nxt = inc + ind
                    inc = nxt.Args[0L];
                }
                else
                {
                    panic("unreachable"); // one of the cases must be true from the above.
                } 

                // Expect the increment to be a positive constant.
                // TODO: handle negative increment.
                if (inc.Op != OpConst64 || inc.AuxInt <= 0L)
                {
                    continue;
                } 

                // Up to now we extracted the induction variable (ind),
                // the increment delta (inc), the temporary sum (nxt),
                // the mininum value (min) and the maximum value (max).
                //
                // We also know that ind has the form (Phi min nxt) where
                // nxt is (Add inc nxt) which means: 1) inc dominates nxt
                // and 2) there is a loop starting at inc and containing nxt.
                //
                // We need to prove that the induction variable is incremented
                // only when it's smaller than the maximum value.
                // Two conditions must happen listed below to accept ind
                // as an induction variable.

                // First condition: loop entry has a single predecessor, which
                // is the header block.  This implies that b.Succs[entry] is
                // reached iff ind < max.
                if (len(b.Succs[entry].b.Preds) != 1L)
                { 
                    // b.Succs[1-entry] must exit the loop.
                    continue;
                } 

                // Second condition: b.Succs[entry] dominates nxt so that
                // nxt is computed when inc < max, meaning nxt <= max.
                if (!sdom.isAncestorEq(b.Succs[entry].b, nxt.Block))
                { 
                    // inc+ind can only be reached through the branch that enters the loop.
                    continue;
                } 

                // If max is c + SliceLen with c <= 0 then we drop c.
                // Makes sure c + SliceLen doesn't overflow when SliceLen == 0.
                // TODO: save c as an offset from max.
                {
                    var (w, c) = dropAdd64(max);

                    if ((w.Op == OpStringLen || w.Op == OpSliceLen) && 0L >= c && -c >= 0L)
                    {
                        max = w;
                    } 

                    // We can only guarantee that the loops runs within limits of induction variable
                    // if the increment is 1 or when the limits are constants.

                } 

                // We can only guarantee that the loops runs within limits of induction variable
                // if the increment is 1 or when the limits are constants.
                if (inc.AuxInt != 1L)
                {
                    var ok = false;
                    if (min.Op == OpConst64 && max.Op == OpConst64)
                    {
                        if (max.AuxInt > min.AuxInt && max.AuxInt % inc.AuxInt == min.AuxInt % inc.AuxInt)
                        { // handle overflow
                            ok = true;
                        }
                    }
                    if (!ok)
                    {
                        continue;
                    }
                }
                if (f.pass.debug > 1L)
                {
                    if (min.Op == OpConst64)
                    {
                        b.Func.Warnl(b.Pos, "Induction variable with minimum %d and increment %d", min.AuxInt, inc.AuxInt);
                    }
                    else
                    {
                        b.Func.Warnl(b.Pos, "Induction variable with non-const minimum and increment %d", inc.AuxInt);
                    }
                }
                iv = append(iv, new indVar(ind:ind,inc:inc,nxt:nxt,min:min,max:max,entry:b.Succs[entry].b,));
                b.Logf("found induction variable %v (inc = %v, min = %v, max = %v)\n", ind, inc, min, max);
            }
            return iv;
        });

        // loopbce performs loop based bounds check elimination.
        private static void loopbce(ref Func f)
        {
            var ivList = findIndVar(f);

            var m = make_map<ref Value, indVar>();
            foreach (var (_, iv) in ivList)
            {
                m[iv.ind] = iv;
            }
            removeBoundsChecks(f, m);
        }

        // removesBoundsChecks remove IsInBounds and IsSliceInBounds based on the induction variables.
        private static void removeBoundsChecks(ref Func f, map<ref Value, indVar> m)
        {
            var sdom = f.sdom();
            foreach (var (_, b) in f.Blocks)
            {
                if (b.Kind != BlockIf)
                {
                    continue;
                }
                var v = b.Control; 

                // Simplify:
                // (IsInBounds ind max) where 0 <= const == min <= ind < max.
                // (IsSliceInBounds ind max) where 0 <= const == min <= ind < max.
                // Found in:
                //    for i := range a {
                //        use a[i]
                //        use a[i:]
                //        use a[:i]
                //    }
                if (v.Op == OpIsInBounds || v.Op == OpIsSliceInBounds)
                {
                    var (ind, add) = dropAdd64(v.Args[0L]);
                    if (ind.Op != OpPhi)
                    {
                        goto skip1;
                    }
                    if (v.Op == OpIsInBounds && add != 0L)
                    {
                        goto skip1;
                    }
                    if (v.Op == OpIsSliceInBounds && (0L > add || add > 1L))
                    {
                        goto skip1;
                    }
                    {
                        var iv__prev2 = iv;

                        var (iv, has) = m[ind];

                        if (has && sdom.isAncestorEq(iv.entry, b) && isNonNegative(iv.min))
                        {
                            if (v.Args[1L] == iv.max)
                            {
                                if (f.pass.debug > 0L)
                                {
                                    f.Warnl(b.Pos, "Found redundant %s", v.Op);
                                }
                                goto simplify;
                            }
                        }

                        iv = iv__prev2;

                    }
                }
skip1:
                if (v.Op == OpIsSliceInBounds)
                {
                    (ind, add) = dropAdd64(v.Args[0L]);
                    if (ind.Op != OpPhi)
                    {
                        goto skip2;
                    }
                    if (0L > add || add > 1L)
                    {
                        goto skip2;
                    }
                    {
                        var iv__prev2 = iv;

                        (iv, has) = m[ind];

                        if (has && sdom.isAncestorEq(iv.entry, b) && isNonNegative(iv.min))
                        {
                            if (v.Args[1L].Op == OpSliceCap && iv.max.Op == OpSliceLen && v.Args[1L].Args[0L] == iv.max.Args[0L])
                            {
                                if (f.pass.debug > 0L)
                                {
                                    f.Warnl(b.Pos, "Found redundant %s (len promoted to cap)", v.Op);
                                }
                                goto simplify;
                            }
                        }

                        iv = iv__prev2;

                    }
                }
skip2:
                if (v.Op == OpIsInBounds || v.Op == OpIsSliceInBounds)
                {
                    (ind, add) = dropAdd64(v.Args[0L]);
                    if (ind.Op != OpPhi)
                    {
                        goto skip3;
                    } 

                    // ind + add >= 0 <-> min + add >= 0 <-> min >= -add
                    {
                        var iv__prev2 = iv;

                        (iv, has) = m[ind];

                        if (has && sdom.isAncestorEq(iv.entry, b) && isGreaterOrEqualThan(iv.min, -add))
                        {
                            if (!v.Args[1L].isGenericIntConst() || !iv.max.isGenericIntConst())
                            {
                                goto skip3;
                            }
                            var limit = v.Args[1L].AuxInt;
                            if (v.Op == OpIsSliceInBounds)
                            { 
                                // If limit++ overflows signed integer then 0 <= max && max <= limit will be false.
                                limit++;
                            }
                            {
                                var max = iv.max.AuxInt + add;

                                if (0L <= max && max <= limit)
                                { // handle overflow
                                    if (f.pass.debug > 0L)
                                    {
                                        f.Warnl(b.Pos, "Found redundant (%s ind %d), ind < %d", v.Op, v.Args[1L].AuxInt, iv.max.AuxInt + add);
                                    }
                                    goto simplify;
                                }

                            }
                        }

                        iv = iv__prev2;

                    }
                }
skip3:

                continue;
simplify:
                f.Logf("removing bounds check %v at %v in %s\n", b.Control, b, f.Name);
                b.Kind = BlockFirst;
                b.SetControl(null);
            }
        }

        private static (ref Value, long) dropAdd64(ref Value v)
        {
            if (v.Op == OpAdd64 && v.Args[0L].Op == OpConst64)
            {
                return (v.Args[1L], v.Args[0L].AuxInt);
            }
            if (v.Op == OpAdd64 && v.Args[1L].Op == OpConst64)
            {
                return (v.Args[0L], v.Args[1L].AuxInt);
            }
            return (v, 0L);
        }

        private static bool isGreaterOrEqualThan(ref Value v, long c)
        {
            if (c == 0L)
            {
                return isNonNegative(v);
            }
            if (v.isGenericIntConst() && v.AuxInt >= c)
            {
                return true;
            }
            return false;
        }
    }
}}}}
