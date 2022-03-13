// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 13 06:01:27 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\fuse_comparisons.go
namespace go.cmd.compile.@internal;

using System;
public static partial class ssa_package {

// fuseIntegerComparisons optimizes inequalities such as '1 <= x && x < 5',
// which can be optimized to 'unsigned(x-1) < 4'.
//
// Look for branch structure like:
//
//   p
//   |\
//   | b
//   |/ \
//   s0 s1
//
// In our example, p has control '1 <= x', b has control 'x < 5',
// and s0 and s1 are the if and else results of the comparison.
//
// This will be optimized into:
//
//   p
//    \
//     b
//    / \
//   s0 s1
//
// where b has the combined control value 'unsigned(x-1) < 4'.
// Later passes will then fuse p and b.
private static bool fuseIntegerComparisons(ptr<Block> _addr_b) {
    ref Block b = ref _addr_b.val;

    if (len(b.Preds) != 1) {
        return false;
    }
    var p = b.Preds[0].Block();
    if (b.Kind != BlockIf || p.Kind != BlockIf) {
        return false;
    }
    if (p.Likely == BranchLikely && p.Succs[0].Block() != b) {
        return false;
    }
    if (p.Likely == BranchUnlikely && p.Succs[1].Block() != b) {
        return false;
    }
    var bc = b.Controls[0];
    var pc = p.Controls[0];
    if (!areMergeableInequalities(_addr_bc, _addr_pc)) {
        return false;
    }
    foreach (var (i, op) in new array<Op>(new Op[] { OpOrB, OpAndB })) {
        if (p.Succs[i].Block() != b.Succs[i].Block()) {
            continue;
        }
        if (!canSpeculativelyExecute(b)) {
            return false;
        }
        var v = b.NewValue0(bc.Pos, op, bc.Type);
        v.AddArg(pc);
        v.AddArg(bc); 

        // Set the combined control value as the control value for b.
        b.SetControl(v); 

        // Modify p so that it jumps directly to b.
        p.removeEdge(i);
        p.Kind = BlockPlain;
        p.Likely = BranchUnknown;
        p.ResetControls();

        return true;
    }    return false;
}

// getConstIntArgIndex returns the index of the first argument that is a
// constant integer or -1 if no such argument exists.
private static nint getConstIntArgIndex(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    foreach (var (i, a) in v.Args) {

        if (a.Op == OpConst8 || a.Op == OpConst16 || a.Op == OpConst32 || a.Op == OpConst64) 
            return i;
            }    return -1;
}

// isSignedInequality reports whether op represents the inequality < or ≤
// in the signed domain.
private static bool isSignedInequality(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;


    if (v.Op == OpLess64 || v.Op == OpLess32 || v.Op == OpLess16 || v.Op == OpLess8 || v.Op == OpLeq64 || v.Op == OpLeq32 || v.Op == OpLeq16 || v.Op == OpLeq8) 
        return true;
        return false;
}

// isUnsignedInequality reports whether op represents the inequality < or ≤
// in the unsigned domain.
private static bool isUnsignedInequality(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;


    if (v.Op == OpLess64U || v.Op == OpLess32U || v.Op == OpLess16U || v.Op == OpLess8U || v.Op == OpLeq64U || v.Op == OpLeq32U || v.Op == OpLeq16U || v.Op == OpLeq8U) 
        return true;
        return false;
}

private static bool areMergeableInequalities(ptr<Value> _addr_x, ptr<Value> _addr_y) {
    ref Value x = ref _addr_x.val;
    ref Value y = ref _addr_y.val;
 
    // We need both inequalities to be either in the signed or unsigned domain.
    // TODO(mundaym): it would also be good to merge when we have an Eq op that
    // could be transformed into a Less/Leq. For example in the unsigned
    // domain 'x == 0 || 3 < x' is equivalent to 'x <= 0 || 3 < x'
    array<Func<ptr<Value>, bool>> inequalityChecks = new array<Func<ptr<Value>, bool>>(new Func<ptr<Value>, bool>[] { isSignedInequality, isUnsignedInequality });
    foreach (var (_, f) in inequalityChecks) {
        if (!f(x) || !f(y)) {
            continue;
        }
        var xi = getConstIntArgIndex(_addr_x);
        if (xi < 0) {
            return false;
        }
        var yi = getConstIntArgIndex(_addr_y);
        if (yi < 0) {
            return false;
        }
        return x.Args[xi ^ 1] == y.Args[yi ^ 1];
    }    return false;
}

} // end ssa_package
