// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 06 22:50:43 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\phiopt.go


namespace go.cmd.compile.@internal;

public static partial class ssa_package {

    // phiopt eliminates boolean Phis based on the previous if.
    //
    // Main use case is to transform:
    //   x := false
    //   if b {
    //     x = true
    //   }
    // into x = b.
    //
    // In SSA code this appears as
    //
    // b0
    //   If b -> b1 b2
    // b1
    //   Plain -> b2
    // b2
    //   x = (OpPhi (ConstBool [true]) (ConstBool [false]))
    //
    // In this case we can replace x with a copy of b.
private static void phiopt(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    var sdom = f.Sdom();
    {
        var b__prev1 = b;

        foreach (var (_, __b) in f.Blocks) {
            b = __b;
            if (len(b.Preds) != 2 || len(b.Values) == 0) { 
                // TODO: handle more than 2 predecessors, e.g. a || b || c.
                continue;

            }
            var pb0 = b;
            var b0 = b.Preds[0].b;
            while (len(b0.Succs) == 1 && len(b0.Preds) == 1) {
                (pb0, b0) = (b0, b0.Preds[0].b);
            }
            if (b0.Kind != BlockIf) {
                continue;
            }
            var pb1 = b;
            var b1 = b.Preds[1].b;
            while (len(b1.Succs) == 1 && len(b1.Preds) == 1) {
                (pb1, b1) = (b1, b1.Preds[0].b);
            }
            if (b1 != b0) {
                continue;
            }
            nint reverse = default;
            if (b0.Succs[0].b == pb0 && b0.Succs[1].b == pb1) {
                reverse = 0;
            }
            else if (b0.Succs[0].b == pb1 && b0.Succs[1].b == pb0) {
                reverse = 1;
            }
            else
 {
                b.Fatalf("invalid predecessors\n");
            }
            {
                var v__prev2 = v;

                foreach (var (_, __v) in b.Values) {
                    v = __v;
                    if (v.Op != OpPhi) {
                        continue;
                    }
                    if (v.Type.IsInteger()) {
                        phioptint(_addr_v, _addr_b0, reverse);
                    }
                    if (!v.Type.IsBoolean()) {
                        continue;
                    }
                    if (v.Args[0].Op == OpConstBool && v.Args[1].Op == OpConstBool) {
                        if (v.Args[reverse].AuxInt != v.Args[1 - reverse].AuxInt) {
                            array<Op> ops = new array<Op>(new Op[] { OpNot, OpCopy });
                            v.reset(ops[v.Args[reverse].AuxInt]);
                            v.AddArg(b0.Controls[0]);
                            if (f.pass.debug > 0) {
                                f.Warnl(b.Pos, "converted OpPhi to %v", v.Op);
                            }
                            continue;

                        }
                    }
                    if (v.Args[reverse].Op == OpConstBool && v.Args[reverse].AuxInt == 1) {
                        {
                            var tmp__prev2 = tmp;

                            var tmp = v.Args[1 - reverse];

                            if (sdom.IsAncestorEq(tmp.Block, b)) {
                                v.reset(OpOrB);
                                v.SetArgs2(b0.Controls[0], tmp);
                                if (f.pass.debug > 0) {
                                    f.Warnl(b.Pos, "converted OpPhi to %v", v.Op);
                                }
                                continue;

                            }
                            tmp = tmp__prev2;

                        }

                    }
                    if (v.Args[1 - reverse].Op == OpConstBool && v.Args[1 - reverse].AuxInt == 0) {
                        {
                            var tmp__prev2 = tmp;

                            tmp = v.Args[reverse];

                            if (sdom.IsAncestorEq(tmp.Block, b)) {
                                v.reset(OpAndB);
                                v.SetArgs2(b0.Controls[0], tmp);
                                if (f.pass.debug > 0) {
                                    f.Warnl(b.Pos, "converted OpPhi to %v", v.Op);
                                }
                                continue;

                            }
                            tmp = tmp__prev2;

                        }

                    }
                }
                v = v__prev2;
            }
        }
        b = b__prev1;
    }

    ptr<lcaRange> lca;
    {
        var b__prev1 = b;

        foreach (var (_, __b) in f.Blocks) {
            b = __b;
            if (len(b.Preds) != 2 || len(b.Values) == 0) { 
                // TODO: handle more than 2 predecessors, e.g. a || b || c.
                continue;

            }
            {
                var v__prev2 = v;

                foreach (var (_, __v) in b.Values) {
                    v = __v; 
                    // find a phi value v = OpPhi (ConstBool [true]) (ConstBool [false]).
                    // TODO: v = OpPhi (ConstBool [true]) (Arg <bool> {value})
                    if (v.Op != OpPhi) {
                        continue;
                    }
                    if (v.Args[0].Op != OpConstBool || v.Args[1].Op != OpConstBool) {
                        continue;
                    }
                    if (v.Args[0].AuxInt == v.Args[1].AuxInt) {
                        continue;
                    }
                    pb0 = b.Preds[0].b;
                    pb1 = b.Preds[1].b;
                    if (pb0.Kind == BlockIf && pb0 == sdom.Parent(b)) { 
                        // special case: pb0 is the dominator block b0.
                        //     b0(pb0)
                        //    |  \
                        //    |  sb1
                        //    |  ...
                        //    |  pb1
                        //    |  /
                        //     b
                        // if another successor sb1 of b0(pb0) dominates pb1, do replace.
                        var ei = b.Preds[0].i;
                        var sb1 = pb0.Succs[1 - ei].b;
                        if (sdom.IsAncestorEq(sb1, pb1)) {
                            convertPhi(_addr_pb0, _addr_v, ei);
                            break;
                        }
                    }
                    else if (pb1.Kind == BlockIf && pb1 == sdom.Parent(b)) { 
                        // special case: pb1 is the dominator block b0.
                        //       b0(pb1)
                        //     /   |
                        //    sb0  |
                        //    ...  |
                        //    pb0  |
                        //      \  |
                        //        b
                        // if another successor sb0 of b0(pb0) dominates pb0, do replace.
                        ei = b.Preds[1].i;
                        var sb0 = pb1.Succs[1 - ei].b;
                        if (sdom.IsAncestorEq(sb0, pb0)) {
                            convertPhi(_addr_pb1, _addr_v, 1 - ei);
                            break;
                        }
                    }
                    else
 { 
                        //      b0
                        //     /   \
                        //    sb0  sb1
                        //    ...  ...
                        //    pb0  pb1
                        //      \   /
                        //        b
                        //
                        // Build data structure for fast least-common-ancestor queries.
                        if (lca == null) {
                            lca = makeLCArange(f);
                        }
                        b0 = lca.find(pb0, pb1);
                        if (b0.Kind != BlockIf) {
                            break;
                        }
                        sb0 = b0.Succs[0].b;
                        sb1 = b0.Succs[1].b;
                        reverse = default;
                        if (sdom.IsAncestorEq(sb0, pb0) && sdom.IsAncestorEq(sb1, pb1)) {
                            reverse = 0;
                        }
                        else if (sdom.IsAncestorEq(sb1, pb0) && sdom.IsAncestorEq(sb0, pb1)) {
                            reverse = 1;
                        }
                        else
 {
                            break;
                        }
                        if (len(sb0.Preds) != 1 || len(sb1.Preds) != 1) { 
                            // we can not replace phi value x in the following case.
                            //   if gp == nil || sp < lo { x = true}
                            //   if a || b { x = true }
                            // so the if statement can only have one condition.
                            break;

                        }
                        convertPhi(_addr_b0, _addr_v, reverse);

                    }
                }
                v = v__prev2;
            }
        }
        b = b__prev1;
    }
}

private static void phioptint(ptr<Value> _addr_v, ptr<Block> _addr_b0, nint reverse) {
    ref Value v = ref _addr_v.val;
    ref Block b0 = ref _addr_b0.val;

    var a0 = v.Args[0];
    var a1 = v.Args[1];
    if (a0.Op != a1.Op) {
        return ;
    }

    if (a0.Op == OpConst8 || a0.Op == OpConst16 || a0.Op == OpConst32 || a0.Op == OpConst64)     else 
        return ;
        var negate = false;

    if (a0.AuxInt == 0 && a1.AuxInt == 1) 
        negate = true;
    else if (a0.AuxInt == 1 && a1.AuxInt == 0)     else 
        return ;
        if (reverse == 1) {
        negate = !negate;
    }
    var a = b0.Controls[0];
    if (negate) {
        a = v.Block.NewValue1(v.Pos, OpNot, a.Type, a);
    }
    v.AddArg(a);

    var cvt = v.Block.NewValue1(v.Pos, OpCvtBoolToUint8, v.Block.Func.Config.Types.UInt8, a);
    switch (v.Type.Size()) {
        case 1: 
            v.reset(OpCopy);
            break;
        case 2: 
            v.reset(OpZeroExt8to16);
            break;
        case 4: 
            v.reset(OpZeroExt8to32);
            break;
        case 8: 
            v.reset(OpZeroExt8to64);
            break;
        default: 
            v.Fatalf("bad int size %d", v.Type.Size());
            break;
    }
    v.AddArg(cvt);

    var f = b0.Func;
    if (f.pass.debug > 0) {
        f.Warnl(v.Block.Pos, "converted OpPhi bool -> int%d", v.Type.Size() * 8);
    }
}

// b is the If block giving the boolean value.
// v is the phi value v = (OpPhi (ConstBool [true]) (ConstBool [false])).
// reverse is the predecessor from which the truth value comes.
private static void convertPhi(ptr<Block> _addr_b, ptr<Value> _addr_v, nint reverse) {
    ref Block b = ref _addr_b.val;
    ref Value v = ref _addr_v.val;

    var f = b.Func;
    array<Op> ops = new array<Op>(new Op[] { OpNot, OpCopy });
    v.reset(ops[v.Args[reverse].AuxInt]);
    v.AddArg(b.Controls[0]);
    if (f.pass.debug > 0) {
        f.Warnl(b.Pos, "converted OpPhi to %v", v.Op);
    }
}

} // end ssa_package
