// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 06 23:08:47 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\tighten.go


namespace go.cmd.compile.@internal;

public static partial class ssa_package {

    // tighten moves Values closer to the Blocks in which they are used.
    // This can reduce the amount of register spilling required,
    // if it doesn't also create more live values.
    // A Value can be moved to any block that
    // dominates all blocks in which it is used.
private static void tighten(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    var canMove = make_slice<bool>(f.NumValues());
    {
        var b__prev1 = b;

        foreach (var (_, __b) in f.Blocks) {
            b = __b;
            {
                var v__prev2 = v;

                foreach (var (_, __v) in b.Values) {
                    v = __v;
                    if (v.Op.isLoweredGetClosurePtr()) { 
                        // Must stay in the entry block.
                        continue;

                    }

                    if (v.Op == OpPhi || v.Op == OpArg || v.Op == OpArgIntReg || v.Op == OpArgFloatReg || v.Op == OpSelect0 || v.Op == OpSelect1 || v.Op == OpSelectN) 
                        // Phis need to stay in their block.
                        // Arg must stay in the entry block.
                        // Tuple selectors must stay with the tuple generator.
                        // SelectN is typically, ultimately, a register.
                        continue;
                                        if (v.MemoryArg() != null) { 
                        // We can't move values which have a memory arg - it might
                        // make two memory values live across a block boundary.
                        continue;

                    }
                    nint narg = 0;
                    {
                        var a__prev3 = a;

                        foreach (var (_, __a) in v.Args) {
                            a = __a;
                            if (!a.rematerializeable()) {
                                narg++;
                            }
                        }
                        a = a__prev3;
                    }

                    if (narg >= 2 && !v.Type.IsFlags()) { 
                        // Don't move values with more than one input, as that may
                        // increase register pressure.
                        // We make an exception for flags, as we want flag generators
                        // moved next to uses (because we only have 1 flag register).
                        continue;

                    }
                    canMove[v.ID] = true;

                }
                v = v__prev2;
            }
        }
        b = b__prev1;
    }

    var lca = makeLCArange(f); 

    // For each moveable value, record the block that dominates all uses found so far.
    var target = make_slice<ptr<Block>>(f.NumValues()); 

    // Grab loop information.
    // We use this to make sure we don't tighten a value into a (deeper) loop.
    var idom = f.Idom();
    var loops = f.loopnest();
    loops.calculateDepths();

    var changed = true;
    while (changed) {
        changed = false; 

        // Reset target
        {
            var i__prev2 = i;

            foreach (var (__i) in target) {
                i = __i;
                target[i] = null;
            }
            i = i__prev2;
        }

        {
            var b__prev2 = b;

            foreach (var (_, __b) in f.Blocks) {
                b = __b;
                {
                    var v__prev3 = v;

                    foreach (var (_, __v) in b.Values) {
                        v = __v;
                        {
                            var i__prev4 = i;
                            var a__prev4 = a;

                            foreach (var (__i, __a) in v.Args) {
                                i = __i;
                                a = __a;
                                if (!canMove[a.ID]) {
                                    continue;
                                }
                                var use = b;
                                if (v.Op == OpPhi) {
                                    use = b.Preds[i].b;
                                }
                                if (target[a.ID] == null) {
                                    target[a.ID] = use;
                                }
                                else
 {
                                    target[a.ID] = lca.find(target[a.ID], use);
                                }
                            }
                            i = i__prev4;
                            a = a__prev4;
                        }
                    }
                    v = v__prev3;
                }

                foreach (var (_, c) in b.ControlValues()) {
                    if (!canMove[c.ID]) {
                        continue;
                    }
                    if (target[c.ID] == null) {
                        target[c.ID] = b;
                    }
                    else
 {
                        target[c.ID] = lca.find(target[c.ID], b);
                    }
                }
            }
            b = b__prev2;
        }

        {
            var b__prev2 = b;

            foreach (var (_, __b) in f.Blocks) {
                b = __b;
                var origloop = loops.b2l[b.ID];
                {
                    var v__prev3 = v;

                    foreach (var (_, __v) in b.Values) {
                        v = __v;
                        var t = target[v.ID];
                        if (t == null) {
                            continue;
                        }
                        var targetloop = loops.b2l[t.ID];
                        while (targetloop != null && (origloop == null || targetloop.depth > origloop.depth)) {
                            t = idom[targetloop.header.ID];
                            target[v.ID] = t;
                            targetloop = loops.b2l[t.ID];
                        }

                    }
                    v = v__prev3;
                }
            }
            b = b__prev2;
        }

        {
            var b__prev2 = b;

            foreach (var (_, __b) in f.Blocks) {
                b = __b;
                {
                    var i__prev3 = i;

                    for (nint i = 0; i < len(b.Values); i++) {
                        var v = b.Values[i];
                        t = target[v.ID];
                        if (t == null || t == b) { 
                            // v is not moveable, or is already in correct place.
                            continue;

                        }
                        t.Values = append(t.Values, v);
                        v.Block = t;
                        var last = len(b.Values) - 1;
                        b.Values[i] = b.Values[last];
                        b.Values[last] = null;
                        b.Values = b.Values[..(int)last];
                        changed = true;
                        i--;

                    }

                    i = i__prev3;
                }

            }
            b = b__prev2;
        }
    }

}

// phiTighten moves constants closer to phi users.
// This pass avoids having lots of constants live for lots of the program.
// See issue 16407.
private static void phiTighten(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    foreach (var (_, b) in f.Blocks) {
        foreach (var (_, v) in b.Values) {
            if (v.Op != OpPhi) {
                continue;
            }
            foreach (var (i, a) in v.Args) {
                if (!a.rematerializeable()) {
                    continue; // not a constant we can move around
                }

                if (a.Block == b.Preds[i].b) {
                    continue; // already in the right place
                } 
                // Make a copy of a, put in predecessor block.
                v.SetArg(i, a.copyInto(b.Preds[i].b));

            }

        }
    }
}

} // end ssa_package
