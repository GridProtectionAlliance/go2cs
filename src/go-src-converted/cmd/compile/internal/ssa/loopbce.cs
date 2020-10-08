// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 08 04:10:40 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\loopbce.go
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
        private partial struct indVarFlags // : byte
        {
        }

        private static readonly indVarFlags indVarMinExc = (indVarFlags)1L << (int)(iota); // minimum value is exclusive (default: inclusive)
        private static readonly var indVarMaxInc = (var)0; // maximum value is inclusive (default: exclusive)

        private partial struct indVar
        {
            public ptr<Value> ind; // induction variable
            public ptr<Value> min; // minimum value, inclusive/exclusive depends on flags
            public ptr<Value> max; // maximum value, inclusive/exclusive depends on flags
            public ptr<Block> entry; // entry block in the loop.
            public indVarFlags flags; // Invariant: for all blocks strictly dominated by entry:
//    min <= ind <  max    [if flags == 0]
//    min <  ind <  max    [if flags == indVarMinExc]
//    min <= ind <= max    [if flags == indVarMaxInc]
//    min <  ind <= max    [if flags == indVarMinExc|indVarMaxInc]
        }

        // parseIndVar checks whether the SSA value passed as argument is a valid induction
        // variable, and, if so, extracts:
        //   * the minimum bound
        //   * the increment value
        //   * the "next" value (SSA value that is Phi'd into the induction variable every loop)
        // Currently, we detect induction variables that match (Phi min nxt),
        // with nxt being (Add inc ind).
        // If it can't parse the induction variable correctly, it returns (nil, nil, nil).
        private static (ptr<Value>, ptr<Value>, ptr<Value>) parseIndVar(ptr<Value> _addr_ind) => func((_, panic, __) =>
        {
            ptr<Value> min = default!;
            ptr<Value> inc = default!;
            ptr<Value> nxt = default!;
            ref Value ind = ref _addr_ind.val;

            if (ind.Op != OpPhi)
            {
                return ;
            }

            {
                var n__prev1 = n;

                var n = ind.Args[0L];

                if (n.Op == OpAdd64 && (n.Args[0L] == ind || n.Args[1L] == ind))
                {
                    min = ind.Args[1L];
                    nxt = n;

                }                {
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
                        return ;

                    }

                    n = n__prev2;

                }



                n = n__prev1;

            }


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

            return ;

        });

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
        private static slice<indVar> findIndVar(ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            slice<indVar> iv = default;
            var sdom = f.Sdom();

            foreach (var (_, b) in f.Blocks)
            {
                if (b.Kind != BlockIf || len(b.Preds) != 2L)
                {
                    continue;
                }

                indVarFlags flags = default;
                ptr<Value> ind;                ptr<Value> max; // induction, and maximum

                // Check thet the control if it either ind </<= max or max >/>= ind.
                // TODO: Handle 32-bit comparisons.
                // TODO: Handle unsigned comparisons?
 // induction, and maximum

                // Check thet the control if it either ind </<= max or max >/>= ind.
                // TODO: Handle 32-bit comparisons.
                // TODO: Handle unsigned comparisons?
                var c = b.Controls[0L];

                if (c.Op == OpLeq64)
                {
                    flags |= indVarMaxInc;
                    fallthrough = true;
                }
                if (fallthrough || c.Op == OpLess64)
                {
                    ind = c.Args[0L];
                    max = c.Args[1L];
                    goto __switch_break0;
                }
                // default: 
                    continue;

                __switch_break0:; 

                // See if this is really an induction variable
                var less = true;
                var (min, inc, nxt) = parseIndVar(ind);
                if (min == null)
                { 
                    // We failed to parse the induction variable. Before punting, we want to check
                    // whether the control op was written with arguments in non-idiomatic order,
                    // so that we believe being "max" (the upper bound) is actually the induction
                    // variable itself. This would happen for code like:
                    //     for i := 0; len(n) > i; i++
                    min, inc, nxt = parseIndVar(max);
                    if (min == null)
                    { 
                        // No recognied induction variable on either operand
                        continue;

                    } 

                    // Ok, the arguments were reversed. Swap them, and remember that we're
                    // looking at a ind >/>= loop (so the induction must be decrementing).
                    ind = addr(max);
                    max = addr(ind);
                    less = false;

                } 

                // Expect the increment to be a nonzero constant.
                if (inc.Op != OpConst64)
                {
                    continue;
                }

                var step = inc.AuxInt;
                if (step == 0L)
                {
                    continue;
                } 

                // Increment sign must match comparison direction.
                // When incrementing, the termination comparison must be ind </<= max.
                // When decrementing, the termination comparison must be ind >/>= max.
                // See issue 26116.
                if (step > 0L && !less)
                {
                    continue;
                }

                if (step < 0L && less)
                {
                    continue;
                } 

                // If the increment is negative, swap min/max and their flags
                if (step < 0L)
                {
                    min = max;
                    max = min;
                    var oldf = flags;
                    flags = indVarMaxInc;
                    if (oldf & indVarMaxInc == 0L)
                    {
                        flags |= indVarMinExc;
                    }

                    step = -step;

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
                // is the header block.  This implies that b.Succs[0] is
                // reached iff ind < max.
                if (len(b.Succs[0L].b.Preds) != 1L)
                { 
                    // b.Succs[1] must exit the loop.
                    continue;

                } 

                // Second condition: b.Succs[0] dominates nxt so that
                // nxt is computed when inc < max, meaning nxt <= max.
                if (!sdom.IsAncestorEq(b.Succs[0L].b, nxt.Block))
                { 
                    // inc+ind can only be reached through the branch that enters the loop.
                    continue;

                } 

                // We can only guarantee that the loop runs within limits of induction variable
                // if (one of)
                // (1) the increment is ±1
                // (2) the limits are constants
                // (3) loop is of the form k0 upto Known_not_negative-k inclusive, step <= k
                // (4) loop is of the form k0 upto Known_not_negative-k exclusive, step <= k+1
                // (5) loop is of the form Known_not_negative downto k0, minint+step < k0
                if (step > 1L)
                {
                    var ok = false;
                    if (min.Op == OpConst64 && max.Op == OpConst64)
                    {
                        if (max.AuxInt > min.AuxInt && max.AuxInt % step == min.AuxInt % step)
                        { // handle overflow
                            ok = true;

                        }

                    } 
                    // Handle induction variables of these forms.
                    // KNN is known-not-negative.
                    // SIGNED ARITHMETIC ONLY. (see switch on c above)
                    // Possibilities for KNN are len and cap; perhaps we can infer others.
                    // for i := 0; i <= KNN-k    ; i += k
                    // for i := 0; i <  KNN-(k-1); i += k
                    // Also handle decreasing.

                    // "Proof" copied from https://go-review.googlesource.com/c/go/+/104041/10/src/cmd/compile/internal/ssa/loopbce.go#164
                    //
                    //    In the case of
                    //    // PC is Positive Constant
                    //    L := len(A)-PC
                    //    for i := 0; i < L; i = i+PC
                    //
                    //    we know:
                    //
                    //    0 + PC does not over/underflow.
                    //    len(A)-PC does not over/underflow
                    //    maximum value for L is MaxInt-PC
                    //    i < L <= MaxInt-PC means i + PC < MaxInt hence no overflow.

                    // To match in SSA:
                    // if  (a) min.Op == OpConst64(k0)
                    // and (b) k0 >= MININT + step
                    // and (c) max.Op == OpSubtract(Op{StringLen,SliceLen,SliceCap}, k)
                    // or  (c) max.Op == OpAdd(Op{StringLen,SliceLen,SliceCap}, -k)
                    // or  (c) max.Op == Op{StringLen,SliceLen,SliceCap}
                    // and (d) if upto loop, require indVarMaxInc && step <= k or !indVarMaxInc && step-1 <= k
                    if (min.Op == OpConst64 && min.AuxInt >= step + math.MinInt64)
                    {
                        var knn = max;
                        var k = int64(0L);
                        ptr<Value> kArg;


                        if (max.Op == OpSub64) 
                            knn = max.Args[0L];
                            kArg = max.Args[1L];
                        else if (max.Op == OpAdd64) 
                            knn = max.Args[0L];
                            kArg = max.Args[1L];
                            if (knn.Op == OpConst64)
                            {
                                knn = kArg;
                                kArg = knn;

                            }

                        
                        if (knn.Op == OpSliceLen || knn.Op == OpStringLen || knn.Op == OpSliceCap)                         else 
                            knn = null;
                                                if (kArg != null && kArg.Op == OpConst64)
                        {
                            k = kArg.AuxInt;
                            if (max.Op == OpAdd64)
                            {
                                k = -k;
                            }

                        }

                        if (k >= 0L && knn != null)
                        {
                            if (inc.AuxInt > 0L)
                            { // increasing iteration
                                // The concern for the relation between step and k is to ensure that iv never exceeds knn
                                // i.e., iv < knn-(K-1) ==> iv + K <= knn; iv <= knn-K ==> iv +K < knn
                                if (step <= k || flags & indVarMaxInc == 0L && step - 1L == k)
                                {
                                    ok = true;
                                }

                            }
                            else
                            { // decreasing iteration
                                // Will be decrementing from max towards min; max is knn-k; will only attempt decrement if
                                // knn-k >[=] min; underflow is only a concern if min-step is not smaller than min.
                                // This all assumes signed integer arithmetic
                                // This is already assured by the test above: min.AuxInt >= step+math.MinInt64
                                ok = true;

                            }

                        }

                    } 

                    // TODO: other unrolling idioms
                    // for i := 0; i < KNN - KNN % k ; i += k
                    // for i := 0; i < KNN&^(k-1) ; i += k // k a power of 2
                    // for i := 0; i < KNN&(-k) ; i += k // k a power of 2
                    if (!ok)
                    {
                        continue;
                    }

                }

                if (f.pass.debug >= 1L)
                {
                    printIndVar(_addr_b, ind, _addr_min, max, step, flags);
                }

                iv = append(iv, new indVar(ind:ind,min:min,max:max,entry:b.Succs[0].b,flags:flags,));
                b.Logf("found induction variable %v (inc = %v, min = %v, max = %v)\n", ind, inc, min, max);

            }
            return iv;

        }

        private static (ptr<Value>, long) dropAdd64(ptr<Value> _addr_v)
        {
            ptr<Value> _p0 = default!;
            long _p0 = default;
            ref Value v = ref _addr_v.val;

            if (v.Op == OpAdd64 && v.Args[0L].Op == OpConst64)
            {
                return (_addr_v.Args[1L]!, v.Args[0L].AuxInt);
            }

            if (v.Op == OpAdd64 && v.Args[1L].Op == OpConst64)
            {
                return (_addr_v.Args[0L]!, v.Args[1L].AuxInt);
            }

            return (_addr_v!, 0L);

        }

        private static void printIndVar(ptr<Block> _addr_b, ptr<Value> _addr_i, ptr<Value> _addr_min, ptr<Value> _addr_max, long inc, indVarFlags flags)
        {
            ref Block b = ref _addr_b.val;
            ref Value i = ref _addr_i.val;
            ref Value min = ref _addr_min.val;
            ref Value max = ref _addr_max.val;

            @string mb1 = "[";
            @string mb2 = "]";
            if (flags & indVarMinExc != 0L)
            {
                mb1 = "(";
            }

            if (flags & indVarMaxInc == 0L)
            {
                mb2 = ")";
            }

            var mlim1 = fmt.Sprint(min.AuxInt);
            var mlim2 = fmt.Sprint(max.AuxInt);
            if (!min.isGenericIntConst())
            {
                if (b.Func.pass.debug >= 2L)
                {
                    mlim1 = fmt.Sprint(min);
                }
                else
                {
                    mlim1 = "?";
                }

            }

            if (!max.isGenericIntConst())
            {
                if (b.Func.pass.debug >= 2L)
                {
                    mlim2 = fmt.Sprint(max);
                }
                else
                {
                    mlim2 = "?";
                }

            }

            @string extra = "";
            if (b.Func.pass.debug >= 2L)
            {
                extra = fmt.Sprintf(" (%s)", i);
            }

            b.Func.Warnl(b.Pos, "Induction variable: limits %v%v,%v%v, increment %d%s", mb1, mlim1, mlim2, mb2, inc, extra);

        }
    }
}}}}
