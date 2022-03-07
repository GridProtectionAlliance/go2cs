// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 06 22:49:31 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\copyelim.go


namespace go.cmd.compile.@internal;

public static partial class ssa_package {

    // copyelim removes all uses of OpCopy values from f.
    // A subsequent deadcode pass is needed to actually remove the copies.
private static void copyelim(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;
 
    // Modify all values so no arg (including args
    // of OpCopy) is a copy.
    {
        var b__prev1 = b;

        foreach (var (_, __b) in f.Blocks) {
            b = __b;
            {
                var v__prev2 = v;

                foreach (var (_, __v) in b.Values) {
                    v = __v;
                    copyelimValue(_addr_v);
                }
                v = v__prev2;
            }
        }
        b = b__prev1;
    }

    {
        var b__prev1 = b;

        foreach (var (_, __b) in f.Blocks) {
            b = __b;
            {
                var i__prev2 = i;
                var v__prev2 = v;

                foreach (var (__i, __v) in b.ControlValues()) {
                    i = __i;
                    v = __v;
                    if (v.Op == OpCopy) {
                        b.ReplaceControl(i, v.Args[0]);
                    }
                }
                i = i__prev2;
                v = v__prev2;
            }
        }
        b = b__prev1;
    }

    foreach (var (_, name) in f.Names) {
        var values = f.NamedValues[name.val];
        {
            var i__prev2 = i;
            var v__prev2 = v;

            foreach (var (__i, __v) in values) {
                i = __i;
                v = __v;
                if (v.Op == OpCopy) {
                    values[i] = v.Args[0];
                }
            }
            i = i__prev2;
            v = v__prev2;
        }
    }
}

// copySource returns the (non-copy) op which is the
// ultimate source of v.  v must be a copy op.
private static ptr<Value> copySource(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var w = v.Args[0]; 

    // This loop is just:
    // for w.Op == OpCopy {
    //     w = w.Args[0]
    // }
    // but we take some extra care to make sure we
    // don't get stuck in an infinite loop.
    // Infinite copy loops may happen in unreachable code.
    // (TODO: or can they? Needs a test.)
    var slow = w;
    bool advance = default;
    while (w.Op == OpCopy) {
        w = w.Args[0];
        if (w == slow) {
            w.reset(OpUnknown);
            break;
        }
        if (advance) {
            slow = slow.Args[0];
        }
        advance = !advance;

    } 

    // The answer is w.  Update all the copies we saw
    // to point directly to w.  Doing this update makes
    // sure that we don't end up doing O(n^2) work
    // for a chain of n copies.
    while (v != w) {
        var x = v.Args[0];
        v.SetArg(0, w);
        v = x;
    }
    return _addr_w!;

}

// copyelimValue ensures that no args of v are copies.
private static void copyelimValue(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    foreach (var (i, a) in v.Args) {
        if (a.Op == OpCopy) {
            v.SetArg(i, copySource(_addr_a));
        }
    }
}

} // end ssa_package
