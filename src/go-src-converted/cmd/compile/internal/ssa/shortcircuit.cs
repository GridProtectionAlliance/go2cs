// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 06 23:08:42 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\shortcircuit.go

using System;


namespace go.cmd.compile.@internal;

public static partial class ssa_package {

    // Shortcircuit finds situations where branch directions
    // are always correlated and rewrites the CFG to take
    // advantage of that fact.
    // This optimization is useful for compiling && and || expressions.
private static void shortcircuit(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;
 
    // Step 1: Replace a phi arg with a constant if that arg
    // is the control value of a preceding If block.
    // b1:
    //    If a goto b2 else b3
    // b2: <- b1 ...
    //    x = phi(a, ...)
    //
    // We can replace the "a" in the phi with the constant true.
    ptr<Value> ct;    ptr<Value> cf;

    foreach (var (_, b) in f.Blocks) {
        foreach (var (_, v) in b.Values) {
            if (v.Op != OpPhi) {
                continue;
            }
            if (!v.Type.IsBoolean()) {
                continue;
            }
            foreach (var (i, a) in v.Args) {
                var e = b.Preds[i];
                var p = e.b;
                if (p.Kind != BlockIf) {
                    continue;
                }
                if (p.Controls[0] != a) {
                    continue;
                }
                if (e.i == 0) {
                    if (ct == null) {
                        ct = f.ConstBool(f.Config.Types.Bool, true);
                    }
                    v.SetArg(i, ct);

                }
                else
 {
                    if (cf == null) {
                        cf = f.ConstBool(f.Config.Types.Bool, false);
                    }
                    v.SetArg(i, cf);

                }
            }
        }
    }    fuse(f, fuseTypePlain | fuseTypeShortCircuit);

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
private static bool shortcircuitBlock(ptr<Block> _addr_b) {
    ref Block b = ref _addr_b.val;

    if (b.Kind != BlockIf) {
        return false;
    }
    var ctl = b.Controls[0];
    nint nval = 1; // the control value
    long swap = default;
    while (ctl.Uses == 1 && ctl.Block == b && (ctl.Op == OpCopy || ctl.Op == OpNot)) {
        if (ctl.Op == OpNot) {
            swap = 1 ^ swap;
        }
        ctl = ctl.Args[0];
        nval++; // wrapper around control value
    }
    if (ctl.Op != OpPhi || ctl.Block != b || ctl.Uses != 1) {
        return false;
    }
    nint nOtherPhi = 0;
    foreach (var (_, w) in b.Values) {
        if (w.Op == OpPhi && w != ctl) {
            nOtherPhi++;
        }
    }    if (nOtherPhi > 0 && len(b.Preds) != 2) { 
        // We rely on b having exactly two preds in shortcircuitPhiPlan
        // to reason about the values of phis.
        return false;

    }
    if (len(b.Values) != nval + nOtherPhi) {
        return false;
    }
    if (nOtherPhi > 0) { 
        // Check for any phi which is the argument of another phi.
        // These cases are tricky, as substitutions done by replaceUses
        // are no longer trivial to do in any ordering. See issue 45175.
        var m = make_map<ptr<Value>, bool>(1 + nOtherPhi);
        {
            var v__prev1 = v;

            foreach (var (_, __v) in b.Values) {
                v = __v;
                if (v.Op == OpPhi) {
                    m[v] = true;
                }
            }

            v = v__prev1;
        }

        {
            var v__prev1 = v;

            foreach (var (__v) in m) {
                v = __v;
                {
                    var a__prev2 = a;

                    foreach (var (_, __a) in v.Args) {
                        a = __a;
                        if (a != v && m[a]) {
                            return false;
                        }
                    }

                    a = a__prev2;
                }
            }

            v = v__prev1;
        }
    }
    nint cidx = -1;
    {
        var i__prev1 = i;
        var a__prev1 = a;

        foreach (var (__i, __a) in ctl.Args) {
            i = __i;
            a = __a;
            if (a.Op == OpConstBool) {
                cidx = i;
                break;
            }
        }
        i = i__prev1;
        a = a__prev1;
    }

    if (cidx == -1) {
        return false;
    }
    var pe = b.Preds[cidx];
    var p = pe.b;
    var pi = pe.i; 

    // t is the "taken" branch: the successor we always go to when coming in from p.
    nint ti = 1 ^ ctl.Args[cidx].AuxInt ^ swap;
    var te = b.Succs[ti];
    var t = te.b;
    if (p == b || t == b) { 
        // This is an infinite loop; we can't remove it. See issue 33903.
        return false;

    }
    Action<ptr<Value>, nint> fixPhi = default;
    if (nOtherPhi > 0) {
        fixPhi = shortcircuitPhiPlan(_addr_b, _addr_ctl, cidx, ti);
        if (fixPhi == null) {
            return false;
        }
    }
    b.removePred(cidx);
    var n = len(b.Preds);
    ctl.Args[cidx].Uses--;
    ctl.Args[cidx] = ctl.Args[n];
    ctl.Args[n] = null;
    ctl.Args = ctl.Args[..(int)n]; 

    // Redirect p's outgoing edge to t.
    p.Succs[pi] = new Edge(t,len(t.Preds)); 

    // Fix up t to have one more predecessor.
    t.Preds = append(t.Preds, new Edge(p,pi));
    {
        var v__prev1 = v;

        foreach (var (_, __v) in t.Values) {
            v = __v;
            if (v.Op != OpPhi) {
                continue;
            }
            v.AddArg(v.Args[te.i]);
        }
        v = v__prev1;
    }

    if (nOtherPhi != 0) { 
        // Adjust all other phis as necessary.
        // Use a plain for loop instead of range because fixPhi may move phis,
        // thus modifying b.Values.
        {
            var i__prev1 = i;

            for (nint i = 0; i < len(b.Values); i++) {
                var phi = b.Values[i];
                if (phi.Uses == 0 || phi == ctl || phi.Op != OpPhi) {
                    continue;
                }
                fixPhi(phi, i);
                if (phi.Block == b) {
                    continue;
                } 
                // phi got moved to a different block with v.moveTo.
                // Adjust phi values in this new block that refer
                // to phi to refer to the corresponding phi arg instead.
                // phi used to be evaluated prior to this block,
                // and now it is evaluated in this block.
                {
                    var v__prev2 = v;

                    foreach (var (_, __v) in phi.Block.Values) {
                        v = __v;
                        if (v.Op != OpPhi || v == phi) {
                            continue;
                        }
                        {
                            var a__prev3 = a;

                            foreach (var (__j, __a) in v.Args) {
                                j = __j;
                                a = __a;
                                if (a == phi) {
                                    v.SetArg(j, phi.Args[j]);
                                }
                            }

                            a = a__prev3;
                        }
                    }

                    v = v__prev2;
                }

                if (phi.Uses != 0) {
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

            foreach (var (_, __v) in b.Values) {
                v = __v;
                if (v.Uses == 0) {
                    v.reset(OpInvalid);
                }
            }

            v = v__prev1;
        }
    }
    if (len(b.Preds) == 0) { 
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
private static Action<ptr<Value>, nint> shortcircuitPhiPlan(ptr<Block> _addr_b, ptr<Value> _addr_ctl, nint cidx, long ti) {
    ref Block b = ref _addr_b.val;
    ref Value ctl = ref _addr_ctl.val;
 
    // t is the "taken" branch: the successor we always go to when coming in from p.
    var t = b.Succs[ti].b; 
    // u is the "untaken" branch: the successor we never go to when coming in from p.
    var u = b.Succs[1 ^ ti].b; 

    // In the following CFG matching, ensure that b's preds are entirely distinct from b's succs.
    // This is probably a stronger condition than required, but this happens extremely rarely,
    // and it makes it easier to avoid getting deceived by pretty ASCII charts. See #44465.
    {
        var p0 = b.Preds[0].b;
        var p1 = b.Preds[1].b;

        if (p0 == t || p1 == t || p0 == u || p1 == u) {
            return null;
        }
    } 

    // Look for some common CFG structures
    // in which the outbound paths from b merge,
    // with no other preds joining them.
    // In these cases, we can reconstruct what the value
    // of any phi in b must be in the successor blocks.

    if (len(t.Preds) == 1 && len(t.Succs) == 1 && len(u.Preds) == 1 && len(u.Succs) == 1 && t.Succs[0].b == u.Succs[0].b && len(t.Succs[0].b.Preds) == 2) { 
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
        var m = t.Succs[0].b;
        return (v, i) => { 
            // Replace any uses of v in t and u with the value v must have,
            // given that we have arrived at that block.
            // Then move v to m and adjust its value accordingly;
            // this handles all other uses of v.
            var argP = v.Args[cidx];
            var argQ = v.Args[1 ^ cidx];
            u.replaceUses(v, argQ);
            var phi = t.Func.newValue(OpPhi, v.Type, t, v.Pos);
            phi.AddArg2(argQ, argP);
            t.replaceUses(v, phi);
            if (v.Uses == 0) {
                return ;
            }

            v.moveTo(m, i); 
            // The phi in m belongs to whichever pred idx corresponds to t.
            if (m.Preds[0].b == t) {
                v.SetArgs2(phi, argQ);
            }
            else
 {
                v.SetArgs2(argQ, phi);
            }

        };

    }
    if (len(t.Preds) == 2 && len(u.Preds) == 1 && len(u.Succs) == 1 && u.Succs[0].b == t) { 
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
        return (v, i) => { 
            // Replace any uses of v in u. Then move v to t.
            argP = v.Args[cidx];
            argQ = v.Args[1 ^ cidx];
            u.replaceUses(v, argQ);
            v.moveTo(t, i);
            v.SetArgs3(argQ, argQ, argP);

        };

    }
    if (len(u.Preds) == 2 && len(t.Preds) == 1 && len(t.Succs) == 1 && t.Succs[0].b == u) { 
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
        return (v, i) => { 
            // Replace any uses of v in t. Then move v to u.
            argP = v.Args[cidx];
            argQ = v.Args[1 ^ cidx];
            phi = t.Func.newValue(OpPhi, v.Type, t, v.Pos);
            phi.AddArg2(argQ, argP);
            t.replaceUses(v, phi);
            if (v.Uses == 0) {
                return ;
            }

            v.moveTo(u, i);
            v.SetArgs2(argQ, phi);

        };

    }
    if (len(t.Preds) == 1 && len(u.Preds) == 1 && len(t.Succs) == 0) { 
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
        return (v, i) => { 
            // Replace any uses of v in t and x. Then move v to u.
            argP = v.Args[cidx];
            argQ = v.Args[1 ^ cidx]; 
            // If there are no uses of v in t or x, this phi will be unused.
            // That's OK; it's not worth the cost to prevent that.
            phi = t.Func.newValue(OpPhi, v.Type, t, v.Pos);
            phi.AddArg2(argQ, argP);
            t.replaceUses(v, phi);
            if (v.Uses == 0) {
                return ;
            }

            v.moveTo(u, i);
            v.SetArgs1(argQ);

        };

    }
    if (len(u.Preds) == 1 && len(t.Preds) == 1 && len(u.Succs) == 0) { 
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
        return (v, i) => { 
            // Replace any uses of v in u (and x). Then move v to t.
            argP = v.Args[cidx];
            argQ = v.Args[1 ^ cidx];
            u.replaceUses(v, argQ);
            v.moveTo(t, i);
            v.SetArgs2(argQ, argP);

        };

    }
    return null;

}

// replaceUses replaces all uses of old in b with new.
private static void replaceUses(this ptr<Block> _addr_b, ptr<Value> _addr_old, ptr<Value> _addr_@new) {
    ref Block b = ref _addr_b.val;
    ref Value old = ref _addr_old.val;
    ref Value @new = ref _addr_@new.val;

    {
        var v__prev1 = v;

        foreach (var (_, __v) in b.Values) {
            v = __v;
            {
                var i__prev2 = i;

                foreach (var (__i, __a) in v.Args) {
                    i = __i;
                    a = __a;
                    if (a == old) {
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

        foreach (var (__i, __v) in b.ControlValues()) {
            i = __i;
            v = __v;
            if (v == old) {
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
private static void moveTo(this ptr<Value> _addr_v, ptr<Block> _addr_dst, nint i) {
    ref Value v = ref _addr_v.val;
    ref Block dst = ref _addr_dst.val;

    if (dst.Func.scheduled) {
        v.Fatalf("moveTo after scheduling");
    }
    var src = v.Block;
    if (src.Values[i] != v) {
        v.Fatalf("moveTo bad index %d", v, i);
    }
    if (src == dst) {
        return ;
    }
    v.Block = dst;
    dst.Values = append(dst.Values, v);
    var last = len(src.Values) - 1;
    src.Values[i] = src.Values[last];
    src.Values[last] = null;
    src.Values = src.Values[..(int)last];

}

} // end ssa_package
