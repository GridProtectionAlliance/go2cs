// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 06 22:50:02 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\fuse.go
using src = go.cmd.@internal.src_package;

namespace go.cmd.compile.@internal;

public static partial class ssa_package {

    // fuseEarly runs fuse(f, fuseTypePlain|fuseTypeIntInRange).
private static void fuseEarly(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    fuse(_addr_f, fuseTypePlain | fuseTypeIntInRange);
}

// fuseLate runs fuse(f, fuseTypePlain|fuseTypeIf|fuseTypeBranchRedirect).
private static void fuseLate(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    fuse(_addr_f, fuseTypePlain | fuseTypeIf | fuseTypeBranchRedirect);
}

private partial struct fuseType { // : byte
}

private static readonly fuseType fuseTypePlain = 1 << (int)(iota);
private static readonly var fuseTypeIf = 0;
private static readonly var fuseTypeIntInRange = 1;
private static readonly var fuseTypeBranchRedirect = 2;
private static readonly var fuseTypeShortCircuit = 3;


// fuse simplifies control flow by joining basic blocks.
private static void fuse(ptr<Func> _addr_f, fuseType typ) {
    ref Func f = ref _addr_f.val;

    {
        var changed = true;

        while (changed) {
            changed = false; 
            // Fuse from end to beginning, to avoid quadratic behavior in fuseBlockPlain. See issue 13554.
            for (var i = len(f.Blocks) - 1; i >= 0; i--) {
                var b = f.Blocks[i];
                if (typ & fuseTypeIf != 0) {
                    changed = fuseBlockIf(_addr_b) || changed;
                }
                if (typ & fuseTypeIntInRange != 0) {
                    changed = fuseIntegerComparisons(b) || changed;
                }
                if (typ & fuseTypePlain != 0) {
                    changed = fuseBlockPlain(_addr_b) || changed;
                }
                if (typ & fuseTypeShortCircuit != 0) {
                    changed = shortcircuitBlock(b) || changed;
                }
            }

            if (typ & fuseTypeBranchRedirect != 0) {
                changed = fuseBranchRedirect(f) || changed;
            }

            if (changed) {
                f.invalidateCFG();
            }

        }
    }

}

// fuseBlockIf handles the following cases where s0 and s1 are empty blocks.
//
//       b        b           b       b
//    \ / \ /    | \  /    \ / |     | |
//     s0  s1    |  s1      s0 |     | |
//      \ /      | /         \ |     | |
//       ss      ss           ss      ss
//
// If all Phi ops in ss have identical variables for slots corresponding to
// s0, s1 and b then the branch can be dropped.
// This optimization often comes up in switch statements with multiple
// expressions in a case clause:
//   switch n {
//     case 1,2,3: return 4
//   }
// TODO: If ss doesn't contain any OpPhis, are s0 and s1 dead code anyway.
private static bool fuseBlockIf(ptr<Block> _addr_b) {
    ref Block b = ref _addr_b.val;

    if (b.Kind != BlockIf) {
        return false;
    }
    ptr<Block> ss0;    ptr<Block> ss1;

    var s0 = b.Succs[0].b;
    var i0 = b.Succs[0].i;
    if (s0.Kind != BlockPlain || !isEmpty(_addr_s0)) {
        (s0, ss0) = (b, s0);
    }
    else
 {
        ss0 = s0.Succs[0].b;
        i0 = s0.Succs[0].i;
    }
    var s1 = b.Succs[1].b;
    var i1 = b.Succs[1].i;
    if (s1.Kind != BlockPlain || !isEmpty(_addr_s1)) {
        (s1, ss1) = (b, s1);
    }
    else
 {
        ss1 = s1.Succs[0].b;
        i1 = s1.Succs[0].i;
    }
    if (ss0 != ss1) {
        if (s0.Kind == BlockPlain && isEmpty(_addr_s0) && s1.Kind == BlockPlain && isEmpty(_addr_s1)) { 
            // Two special cases where both s0, s1 and ss are empty blocks.
            if (s0 == ss1) {
                (s0, ss0) = (b, ss1);
            }
            else if (ss0 == s1) {
                (s1, ss1) = (b, ss0);
            }
            else
 {
                return false;
            }

        }
        else
 {
            return false;
        }
    }
    var ss = ss0; 

    // s0 and s1 are equal with b if the corresponding block is missing
    // (2nd, 3rd and 4th case in the figure).

    {
        var v__prev1 = v;

        foreach (var (_, __v) in ss.Values) {
            v = __v;
            if (v.Op == OpPhi && v.Uses > 0 && v.Args[i0] != v.Args[i1]) {
                return false;
            }
        }
        v = v__prev1;
    }

    b.removeEdge(0);
    if (s0 != b && len(s0.Preds) == 0) {
        s0.removeEdge(0); 
        // Move any (dead) values in s0 to b,
        // where they will be eliminated by the next deadcode pass.
        {
            var v__prev1 = v;

            foreach (var (_, __v) in s0.Values) {
                v = __v;
                v.Block = b;
            }

            v = v__prev1;
        }

        b.Values = append(b.Values, s0.Values); 
        // Clear s0.
        s0.Kind = BlockInvalid;
        s0.Values = null;
        s0.Succs = null;
        s0.Preds = null;

    }
    b.Kind = BlockPlain;
    b.Likely = BranchUnknown;
    b.ResetControls(); 
    // The values in b may be dead codes, and clearing them in time may
    // obtain new optimization opportunities.
    // First put dead values that can be deleted into a slice walkValues.
    // Then put their arguments in walkValues before resetting the dead values
    // in walkValues, because the arguments may also become dead values.
    ptr<Value> walkValues = new slice<ptr<Value>>(new ptr<Value>[] {  });
    {
        var v__prev1 = v;

        foreach (var (_, __v) in b.Values) {
            v = __v;
            if (v.Uses == 0 && v.removeable()) {
                walkValues = append(walkValues, v);
            }
        }
        v = v__prev1;
    }

    while (len(walkValues) != 0) {
        var v = walkValues[len(walkValues) - 1];
        walkValues = walkValues[..(int)len(walkValues) - 1];
        if (v.Uses == 0 && v.removeable()) {
            walkValues = append(walkValues, v.Args);
            v.reset(OpInvalid);
        }
    }
    return true;

}

// isEmpty reports whether b contains any live values.
// There may be false positives.
private static bool isEmpty(ptr<Block> _addr_b) {
    ref Block b = ref _addr_b.val;

    foreach (var (_, v) in b.Values) {
        if (v.Uses > 0 || v.Op.IsCall() || v.Op.HasSideEffects() || v.Type.IsVoid()) {
            return false;
        }
    }    return true;

}

private static bool fuseBlockPlain(ptr<Block> _addr_b) {
    ref Block b = ref _addr_b.val;

    if (b.Kind != BlockPlain) {
        return false;
    }
    var c = b.Succs[0].b;
    if (len(c.Preds) != 1) {
        return false;
    }
    if (b.Pos.IsStmt() == src.PosIsStmt) {
        var l = b.Pos.Line();
        {
            var v__prev1 = v;

            foreach (var (_, __v) in c.Values) {
                v = __v;
                if (v.Pos.IsStmt() == src.PosNotStmt) {
                    continue;
                }
                if (l == v.Pos.Line()) {
                    v.Pos = v.Pos.WithIsStmt();
                    l = 0;
                    break;
                }
            }

            v = v__prev1;
        }

        if (l != 0 && c.Pos.Line() == l) {
            c.Pos = c.Pos.WithIsStmt();
        }
    }
    {
        var v__prev1 = v;

        foreach (var (_, __v) in b.Values) {
            v = __v;
            v.Block = c;
        }
        v = v__prev1;
    }

    if (cap(c.Values) >= cap(b.Values) || len(b.Values) <= len(b.valstorage)) {
        var bl = len(b.Values);
        var cl = len(c.Values);
        slice<ptr<Value>> t = default; // construct t = b.Values followed-by c.Values, but with attention to allocation.
        if (cap(c.Values) < bl + cl) { 
            // reallocate
            t = make_slice<ptr<Value>>(bl + cl);

        }
        else
 { 
            // in place.
            t = c.Values[(int)0..(int)bl + cl];

        }
        copy(t[(int)bl..], c.Values); // possibly in-place
        c.Values = t;
        copy(c.Values, b.Values);

    }
    else
 {
        c.Values = append(b.Values, c.Values);
    }
    c.predstorage[0] = new Edge();
    if (len(b.Preds) > len(b.predstorage)) {
        c.Preds = b.Preds;
    }
    else
 {
        c.Preds = append(c.predstorage[..(int)0], b.Preds);
    }
    foreach (var (i, e) in c.Preds) {
        var p = e.b;
        p.Succs[e.i] = new Edge(c,i);
    }    var f = b.Func;
    if (f.Entry == b) {
        f.Entry = c;
    }
    b.Kind = BlockInvalid;
    b.Values = null;
    b.Preds = null;
    b.Succs = null;
    return true;

}

} // end ssa_package
