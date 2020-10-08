// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 08 04:26:35 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\shortcircuit.go

using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        // Shortcircuit finds situations where branch directions
        // are always correlated and rewrites the CFG to take
        // advantage of that fact.
        // This optimization is useful for compiling && and || expressions.
        private static void shortcircuit(ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;
 
            // Step 1: Replace a phi arg with a constant if that arg
            // is the control value of a preceding If block.
            // b1:
            //    If a goto b2 else b3
            // b2: <- b1 ...
            //    x = phi(a, ...)
            //
            // We can replace the "a" in the phi with the constant true.
            ptr<Value> ct;            ptr<Value> cf;

            foreach (var (_, b) in f.Blocks)
            {
                foreach (var (_, v) in b.Values)
                {
                    if (v.Op != OpPhi)
                    {
                        continue;
                    }
                    if (!v.Type.IsBoolean())
                    {
                        continue;
                    }
                    foreach (var (i, a) in v.Args)
                    {
                        var e = b.Preds[i];
                        var p = e.b;
                        if (p.Kind != BlockIf)
                        {
                            continue;
                        }
                        if (p.Controls[0L] != a)
                        {
                            continue;
                        }
                        if (e.i == 0L)
                        {
                            if (ct == null)
                            {
                                ct = f.ConstBool(f.Config.Types.Bool, true);
                            }
                            v.SetArg(i, ct);

                        }
                        else
                        {
                            if (cf == null)
                            {
                                cf = f.ConstBool(f.Config.Types.Bool, false);
                            }
                            v.SetArg(i, cf);

                        }
                    }
                }
            }            fuse(f, fuseTypePlain | fuseTypeShortCircuit);

        }

        // shortcircuitBlock checks for a CFG in which an If block
        // has as its control value a Phi that has a ConstBool arg.
        // In some such cases, we can rewrite the CFG into a flatter form.
        //
        // (1) Look for a CFG of the form
        //
        //   p   other pred(s)
        //    \ /
        //     b
        //    / \
        //   t   other succ
        //
        // in which b is an If block containing a single phi value with a single use (b's Control),
        // which has a ConstBool arg.
        // p is the predecessor corresponding to the argument slot in which the ConstBool is found.
        // t is the successor corresponding to the value of the ConstBool arg.
        //
        // Rewrite this into
        //
        //   p   other pred(s)
        //   |  /
        //   | b
        //   |/ \
        //   t   u
        //
        // and remove the appropriate phi arg(s).
        //
        // (2) Look for a CFG of the form
        //
        //   p   q
        //    \ /
        //     b
        //    / \
        //   t   u
        //
        // in which b is as described in (1).
        // However, b may also contain other phi values.
        // The CFG will be modified as described in (1).
        // However, in order to handle those other phi values,
        // for each other phi value w, we must be able to eliminate w from b.
        // We can do that though a combination of moving w to a different block
        // and rewriting uses of w to use a different value instead.
        // See shortcircuitPhiPlan for details.
        private static bool shortcircuitBlock(ptr<Block> _addr_b)
        {
            ref Block b = ref _addr_b.val;

            if (b.Kind != BlockIf)
            {
                return false;
            } 
            // Look for control values of the form Copy(Not(Copy(Phi(const, ...)))).
            // Those must be the only values in the b, and they each must be used only by b.
            // Track the negations so that we can swap successors as needed later.
            var ctl = b.Controls[0L];
            long nval = 1L; // the control value
            long swap = default;
            while (ctl.Uses == 1L && ctl.Block == b && (ctl.Op == OpCopy || ctl.Op == OpNot))
            {
                if (ctl.Op == OpNot)
                {
                    swap = 1L ^ swap;
                }

                ctl = ctl.Args[0L];
                nval++; // wrapper around control value
            }

            if (ctl.Op != OpPhi || ctl.Block != b || ctl.Uses != 1L)
            {
                return false;
            }

            long nOtherPhi = 0L;
            foreach (var (_, w) in b.Values)
            {
                if (w.Op == OpPhi && w != ctl)
                {
                    nOtherPhi++;
                }

            }
            if (nOtherPhi > 0L && len(b.Preds) != 2L)
            { 
                // We rely on b having exactly two preds in shortcircuitPhiPlan
                // to reason about the values of phis.
                return false;

            }

            if (len(b.Values) != nval + nOtherPhi)
            {
                return false;
            } 

            // Locate index of first const phi arg.
            long cidx = -1L;
            {
                var i__prev1 = i;
                var a__prev1 = a;

                foreach (var (__i, __a) in ctl.Args)
                {
                    i = __i;
                    a = __a;
                    if (a.Op == OpConstBool)
                    {
                        cidx = i;
                        break;
                    }

                }

                i = i__prev1;
                a = a__prev1;
            }

            if (cidx == -1L)
            {
                return false;
            } 

            // p is the predecessor corresponding to cidx.
            var pe = b.Preds[cidx];
            var p = pe.b;
            var pi = pe.i; 

            // t is the "taken" branch: the successor we always go to when coming in from p.
            long ti = 1L ^ ctl.Args[cidx].AuxInt ^ swap;
            var te = b.Succs[ti];
            var t = te.b;
            if (p == b || t == b)
            { 
                // This is an infinite loop; we can't remove it. See issue 33903.
                return false;

            }

            Action<ptr<Value>, long> fixPhi = default;
            if (nOtherPhi > 0L)
            {
                fixPhi = shortcircuitPhiPlan(_addr_b, _addr_ctl, cidx, ti);
                if (fixPhi == null)
                {
                    return false;
                }

            } 

            // We're committed. Update CFG and Phis.
            // If you modify this section, update shortcircuitPhiPlan corresponding.

            // Remove b's incoming edge from p.
            b.removePred(cidx);
            var n = len(b.Preds);
            ctl.Args[cidx].Uses--;
            ctl.Args[cidx] = ctl.Args[n];
            ctl.Args[n] = null;
            ctl.Args = ctl.Args[..n]; 

            // Redirect p's outgoing edge to t.
            p.Succs[pi] = new Edge(t,len(t.Preds)); 

            // Fix up t to have one more predecessor.
            t.Preds = append(t.Preds, new Edge(p,pi));
            {
                var v__prev1 = v;

                foreach (var (_, __v) in t.Values)
                {
                    v = __v;
                    if (v.Op != OpPhi)
                    {
                        continue;
                    }

                    v.AddArg(v.Args[te.i]);

                }

                v = v__prev1;
            }

            if (nOtherPhi != 0L)
            { 
                // Adjust all other phis as necessary.
                // Use a plain for loop instead of range because fixPhi may move phis,
                // thus modifying b.Values.
                {
                    var i__prev1 = i;

                    for (long i = 0L; i < len(b.Values); i++)
                    {
                        var phi = b.Values[i];
                        if (phi.Uses == 0L || phi == ctl || phi.Op != OpPhi)
                        {
                            continue;
                        }

                        fixPhi(phi, i);
                        if (phi.Block == b)
                        {
                            continue;
                        } 
                        // phi got moved to a different block with v.moveTo.
                        // Adjust phi values in this new block that refer
                        // to phi to refer to the corresponding phi arg instead.
                        // phi used to be evaluated prior to this block,
                        // and now it is evaluated in this block.
                        {
                            var v__prev2 = v;

                            foreach (var (_, __v) in phi.Block.Values)
                            {
                                v = __v;
                                if (v.Op != OpPhi || v == phi)
                                {
                                    continue;
                                }

                                {
                                    var a__prev3 = a;

                                    foreach (var (__j, __a) in v.Args)
                                    {
                                        j = __j;
                                        a = __a;
                                        if (a == phi)
                                        {
                                            v.SetArg(j, phi.Args[j]);
                                        }

                                    }

                                    a = a__prev3;
                                }
                            }

                            v = v__prev2;
                        }

                        if (phi.Uses != 0L)
                        {
                            phielimValue(phi);
                        }
                        else
                        {
                            phi.reset(OpInvalid);
                        }

                        i--; // v.moveTo put a new value at index i; reprocess
                    } 

                    // We may have left behind some phi values with no uses
                    // but the wrong number of arguments. Eliminate those.


                    i = i__prev1;
                } 

                // We may have left behind some phi values with no uses
                // but the wrong number of arguments. Eliminate those.
                {
                    var v__prev1 = v;

                    foreach (var (_, __v) in b.Values)
                    {
                        v = __v;
                        if (v.Uses == 0L)
                        {
                            v.reset(OpInvalid);
                        }

                    }

                    v = v__prev1;
                }
            }

            if (len(b.Preds) == 0L)
            { 
                // Block is now dead.
                b.Kind = BlockInvalid;

            }

            phielimValue(ctl);
            return true;

        }

        // shortcircuitPhiPlan returns a function to handle non-ctl phi values in b,
        // where b is as described in shortcircuitBlock.
        // The returned function accepts a value v
        // and the index i of v in v.Block: v.Block.Values[i] == v.
        // If the returned function moves v to a different block, it will use v.moveTo.
        // cidx is the index in ctl of the ConstBool arg.
        // ti is the index in b.Succs of the always taken branch when arriving from p.
        // If shortcircuitPhiPlan returns nil, there is no plan available,
        // and the CFG modifications must not proceed.
        // The returned function assumes that shortcircuitBlock has completed its CFG modifications.
        private static Action<ptr<Value>, long> shortcircuitPhiPlan(ptr<Block> _addr_b, ptr<Value> _addr_ctl, long cidx, long ti)
        {
            ref Block b = ref _addr_b.val;
            ref Value ctl = ref _addr_ctl.val;

            const var go115shortcircuitPhis = (var)true;

            if (!go115shortcircuitPhis)
            {
                return null;
            } 

            // t is the "taken" branch: the successor we always go to when coming in from p.
            var t = b.Succs[ti].b; 
            // u is the "untaken" branch: the successor we never go to when coming in from p.
            var u = b.Succs[1L ^ ti].b; 

            // Look for some common CFG structures
            // in which the outbound paths from b merge,
            // with no other preds joining them.
            // In these cases, we can reconstruct what the value
            // of any phi in b must be in the successor blocks.

            if (len(t.Preds) == 1L && len(t.Succs) == 1L && len(u.Preds) == 1L && len(u.Succs) == 1L && t.Succs[0L].b == u.Succs[0L].b && len(t.Succs[0L].b.Preds) == 2L)
            { 
                // p   q
                //  \ /
                //   b
                //  / \
                // t   u
                //  \ /
                //   m
                //
                // After the CFG modifications, this will look like
                //
                // p   q
                // |  /
                // | b
                // |/ \
                // t   u
                //  \ /
                //   m
                //
                // NB: t.Preds is (b, p), not (p, b).
                var m = t.Succs[0L].b;
                return (v, i) =>
                { 
                    // Replace any uses of v in t and u with the value v must have,
                    // given that we have arrived at that block.
                    // Then move v to m and adjust its value accordingly;
                    // this handles all other uses of v.
                    var argP = v.Args[cidx];
                    var argQ = v.Args[1L ^ cidx];
                    u.replaceUses(v, argQ);
                    var phi = t.Func.newValue(OpPhi, v.Type, t, v.Pos);
                    phi.AddArg2(argQ, argP);
                    t.replaceUses(v, phi);
                    if (v.Uses == 0L)
                    {
                        return ;
                    }

                    v.moveTo(m, i); 
                    // The phi in m belongs to whichever pred idx corresponds to t.
                    if (m.Preds[0L].b == t)
                    {
                        v.SetArgs2(phi, argQ);
                    }
                    else
                    {
                        v.SetArgs2(argQ, phi);
                    }

                };

            }

            if (len(t.Preds) == 2L && len(u.Preds) == 1L && len(u.Succs) == 1L && u.Succs[0L].b == t)
            { 
                // p   q
                //  \ /
                //   b
                //   |\
                //   | u
                //   |/
                //   t
                //
                // After the CFG modifications, this will look like
                //
                //     q
                //    /
                //   b
                //   |\
                // p | u
                //  \|/
                //   t
                //
                // NB: t.Preds is (b or u, b or u, p).
                return (v, i) =>
                { 
                    // Replace any uses of v in u. Then move v to t.
                    argP = v.Args[cidx];
                    argQ = v.Args[1L ^ cidx];
                    u.replaceUses(v, argQ);
                    v.moveTo(t, i);
                    v.SetArgs3(argQ, argQ, argP);

                };

            }

            if (len(u.Preds) == 2L && len(t.Preds) == 1L && len(t.Succs) == 1L && t.Succs[0L].b == u)
            { 
                // p   q
                //  \ /
                //   b
                //  /|
                // t |
                //  \|
                //   u
                //
                // After the CFG modifications, this will look like
                //
                // p   q
                // |  /
                // | b
                // |/|
                // t |
                //  \|
                //   u
                //
                // NB: t.Preds is (b, p), not (p, b).
                return (v, i) =>
                { 
                    // Replace any uses of v in t. Then move v to u.
                    argP = v.Args[cidx];
                    argQ = v.Args[1L ^ cidx];
                    phi = t.Func.newValue(OpPhi, v.Type, t, v.Pos);
                    phi.AddArg2(argQ, argP);
                    t.replaceUses(v, phi);
                    if (v.Uses == 0L)
                    {
                        return ;
                    }

                    v.moveTo(u, i);
                    v.SetArgs2(argQ, phi);

                };

            } 

            // Look for some common CFG structures
            // in which one outbound path from b exits,
            // with no other preds joining.
            // In these cases, we can reconstruct what the value
            // of any phi in b must be in the path leading to exit,
            // and move the phi to the non-exit path.
            if (len(t.Preds) == 1L && len(u.Preds) == 1L && len(t.Succs) == 0L)
            { 
                // p   q
                //  \ /
                //   b
                //  / \
                // t   u
                //
                // where t is an Exit/Ret block.
                //
                // After the CFG modifications, this will look like
                //
                // p   q
                // |  /
                // | b
                // |/ \
                // t   u
                //
                // NB: t.Preds is (b, p), not (p, b).
                return (v, i) =>
                { 
                    // Replace any uses of v in t and x. Then move v to u.
                    argP = v.Args[cidx];
                    argQ = v.Args[1L ^ cidx]; 
                    // If there are no uses of v in t or x, this phi will be unused.
                    // That's OK; it's not worth the cost to prevent that.
                    phi = t.Func.newValue(OpPhi, v.Type, t, v.Pos);
                    phi.AddArg2(argQ, argP);
                    t.replaceUses(v, phi);
                    if (v.Uses == 0L)
                    {
                        return ;
                    }

                    v.moveTo(u, i);
                    v.SetArgs1(argQ);

                };

            }

            if (len(u.Preds) == 1L && len(t.Preds) == 1L && len(u.Succs) == 0L)
            { 
                // p   q
                //  \ /
                //   b
                //  / \
                // t   u
                //
                // where u is an Exit/Ret block.
                //
                // After the CFG modifications, this will look like
                //
                // p   q
                // |  /
                // | b
                // |/ \
                // t   u
                //
                // NB: t.Preds is (b, p), not (p, b).
                return (v, i) =>
                { 
                    // Replace any uses of v in u (and x). Then move v to t.
                    argP = v.Args[cidx];
                    argQ = v.Args[1L ^ cidx];
                    u.replaceUses(v, argQ);
                    v.moveTo(t, i);
                    v.SetArgs2(argQ, argP);

                };

            } 

            // TODO: handle more cases; shortcircuit optimizations turn out to be reasonably high impact
            return null;

        }

        // replaceUses replaces all uses of old in b with new.
        private static void replaceUses(this ptr<Block> _addr_b, ptr<Value> _addr_old, ptr<Value> _addr_@new)
        {
            ref Block b = ref _addr_b.val;
            ref Value old = ref _addr_old.val;
            ref Value @new = ref _addr_@new.val;

            {
                var v__prev1 = v;

                foreach (var (_, __v) in b.Values)
                {
                    v = __v;
                    {
                        var i__prev2 = i;

                        foreach (var (__i, __a) in v.Args)
                        {
                            i = __i;
                            a = __a;
                            if (a == old)
                            {
                                v.SetArg(i, new);
                            }

                        }

                        i = i__prev2;
                    }
                }

                v = v__prev1;
            }

            {
                var i__prev1 = i;
                var v__prev1 = v;

                foreach (var (__i, __v) in b.ControlValues())
                {
                    i = __i;
                    v = __v;
                    if (v == old)
                    {
                        b.ReplaceControl(i, new);
                    }

                }

                i = i__prev1;
                v = v__prev1;
            }
        }

        // moveTo moves v to dst, adjusting the appropriate Block.Values slices.
        // The caller is responsible for ensuring that this is safe.
        // i is the index of v in v.Block.Values.
        private static void moveTo(this ptr<Value> _addr_v, ptr<Block> _addr_dst, long i)
        {
            ref Value v = ref _addr_v.val;
            ref Block dst = ref _addr_dst.val;

            if (dst.Func.scheduled)
            {
                v.Fatalf("moveTo after scheduling");
            }

            var src = v.Block;
            if (src.Values[i] != v)
            {
                v.Fatalf("moveTo bad index %d", v, i);
            }

            if (src == dst)
            {
                return ;
            }

            v.Block = dst;
            dst.Values = append(dst.Values, v);
            var last = len(src.Values) - 1L;
            src.Values[i] = src.Values[last];
            src.Values[last] = null;
            src.Values = src.Values[..last];

        }
    }
}}}}
