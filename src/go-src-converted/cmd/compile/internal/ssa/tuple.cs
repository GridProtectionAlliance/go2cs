// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 13 06:22:06 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\tuple.go
namespace go.cmd.compile.@internal;

public static partial class ssa_package {

// tightenTupleSelectors ensures that tuple selectors (Select0, Select1,
// and SelectN ops) are in the same block as their tuple generator. The
// function also ensures that there are no duplicate tuple selectors.
// These properties are expected by the scheduler but may not have
// been maintained by the optimization pipeline up to this point.
//
// See issues 16741 and 39472.
private static void tightenTupleSelectors(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    var selectors = make();
    foreach (var (_, b) in f.Blocks) {
        foreach (var (_, selector) in b.Values) { 
            // Key fields for de-duplication
            ptr<Value> tuple;
            nint idx = 0;

            if (selector.Op == OpSelect1)
            {
                idx = 1;
                fallthrough = true;
            }
            if (fallthrough || selector.Op == OpSelect0)
            {
                tuple = selector.Args[0];
                if (!tuple.Type.IsTuple()) {
                    f.Fatalf("arg of tuple selector %s is not a tuple: %s", selector.String(), tuple.LongString());
                }
                goto __switch_break0;
            }
            if (selector.Op == OpSelectN)
            {
                tuple = selector.Args[0];
                idx = int(selector.AuxInt);
                if (!tuple.Type.IsResults()) {
                    f.Fatalf("arg of result selector %s is not a results: %s", selector.String(), tuple.LongString());
                }
                goto __switch_break0;
            }
            // default: 
                continue;

            __switch_break0:; 

            // If there is a pre-existing selector in the target block then
            // use that. Do this even if the selector is already in the
            // target block to avoid duplicate tuple selectors.
            struct{idIDwhichint} key = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{idIDwhichint}{tuple.ID,idx};
            {
                var t__prev1 = t;

                var t = selectors[key];

                if (t != null) {
                    if (selector != t) {
                        selector.copyOf(t);
                    }
                    continue;
                }
                t = t__prev1;

            } 

            // If the selector is in the wrong block copy it into the target
            // block.
            if (selector.Block != tuple.Block) {
                t = selector.copyInto(tuple.Block);
                selector.copyOf(t);
                selectors[key] = t;
                continue;
            }
            selectors[key] = selector;
        }
    }
}

} // end ssa_package
