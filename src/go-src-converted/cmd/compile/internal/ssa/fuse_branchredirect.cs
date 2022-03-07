// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 06 22:50:03 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\fuse_branchredirect.go


namespace go.cmd.compile.@internal;

public static partial class ssa_package {

    // fuseBranchRedirect checks for a CFG in which the outbound branch
    // of an If block can be derived from its predecessor If block, in
    // some such cases, we can redirect the predecessor If block to the
    // corresponding successor block directly. For example:
    // p:
    //   v11 = Less64 <bool> v10 v8
    //   If v11 goto b else u
    // b: <- p ...
    //   v17 = Leq64 <bool> v10 v8
    //   If v17 goto s else o
    // We can redirect p to s directly.
    //
    // The implementation here borrows the framework of the prove pass.
    // 1, Traverse all blocks of function f to find If blocks.
    // 2,   For any If block b, traverse all its predecessors to find If blocks.
    // 3,     For any If block predecessor p, update relationship p->b.
    // 4,     Traverse all successors of b.
    // 5,       For any successor s of b, try to update relationship b->s, if a
    //          contradiction is found then redirect p to another successor of b.
private static bool fuseBranchRedirect(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    var ft = newFactsTable(f);
    ft.checkpoint();

    var changed = false;
    for (var i = len(f.Blocks) - 1; i >= 0; i--) {
        var b = f.Blocks[i];
        if (b.Kind != BlockIf) {
            continue;
        }
        var bCtl = b.Controls[0];
        if (bCtl.Block != b && len(b.Values) != 0 || (len(b.Values) != 1 || bCtl.Uses != 1) && bCtl.Block == b) {
            continue;
        }
        for (nint k = 0; k < len(b.Preds); k++) {
            var pk = b.Preds[k];
            var p = pk.b;
            if (p.Kind != BlockIf || p == b) {
                continue;
            }
            var pbranch = positive;
            if (pk.i == 1) {
                pbranch = negative;
            }
            ft.checkpoint(); 
            // Assume branch p->b is taken.
            addBranchRestrictions(ft, p, pbranch); 
            // Check if any outgoing branch is unreachable based on the above condition.
            var parent = b;
            foreach (var (j, bbranch) in new array<branch>(new branch[] { positive, negative })) {
                ft.checkpoint(); 
                // Try to update relationship b->child, and check if the contradiction occurs.
                addBranchRestrictions(ft, parent, bbranch);
                var unsat = ft.unsat;
                ft.restore();
                if (!unsat) {
                    continue;
                }
                nint @out = 1 ^ j;
                var child = parent.Succs[out].b;
                if (child == b) {
                    continue;
                }
                b.removePred(k);
                p.Succs[pk.i] = new Edge(child,len(child.Preds)); 
                // Fix up Phi value in b to have one less argument.
                {
                    var v__prev4 = v;

                    foreach (var (_, __v) in b.Values) {
                        v = __v;
                        if (v.Op != OpPhi) {
                            continue;
                        }
                        v.RemoveArg(k);
                        phielimValue(v);

                    }
                    v = v__prev4;
                }

                child.Preds = append(child.Preds, new Edge(p,pk.i));
                var ai = b.Succs[out].i;
                {
                    var v__prev4 = v;

                    foreach (var (_, __v) in child.Values) {
                        v = __v;
                        if (v.Op != OpPhi) {
                            continue;
                        }
                        v.AddArg(v.Args[ai]);

                    }
                    v = v__prev4;
                }

                if (b.Func.pass.debug > 0) {
                    b.Func.Warnl(b.Controls[0].Pos, "Redirect %s based on %s", b.Controls[0].Op, p.Controls[0].Op);
                }
                changed = true;
                k--;
                break;

            }            ft.restore();

        }
        if (len(b.Preds) == 0 && b != f.Entry) { 
            // Block is now dead.
            b.Kind = BlockInvalid;

        }
    }
    ft.restore();
    ft.cleanup(f);
    return changed;

}

} // end ssa_package
