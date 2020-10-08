// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 08 04:26:46 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\tuple.go

using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        // tightenTupleSelectors ensures that tuple selectors (Select0 and
        // Select1 ops) are in the same block as their tuple generator. The
        // function also ensures that there are no duplicate tuple selectors.
        // These properties are expected by the scheduler but may not have
        // been maintained by the optimization pipeline up to this point.
        //
        // See issues 16741 and 39472.
        private static void tightenTupleSelectors(ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            var selectors = make();
            foreach (var (_, b) in f.Blocks)
            {
                foreach (var (_, selector) in b.Values)
                {
                    if (selector.Op != OpSelect0 && selector.Op != OpSelect1)
                    {
                        continue;
                    }
                    var tuple = selector.Args[0L];
                    if (!tuple.Type.IsTuple())
                    {
                        f.Fatalf("arg of tuple selector %s is not a tuple: %s", selector.String(), tuple.LongString());
                    }
                    struct{idIDopOp} key = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{idIDopOp}{tuple.ID,selector.Op};
                    {
                        var t__prev1 = t;

                        var t = selectors[key];

                        if (t != null)
                        {
                            if (selector != t)
                            {
                                selector.copyOf(t);
                            }
                            continue;

                        }
                        t = t__prev1;

                    } 

                    // If the selector is in the wrong block copy it into the target
                    // block.
                    if (selector.Block != tuple.Block)
                    {
                        t = selector.copyInto(tuple.Block);
                        selector.copyOf(t);
                        selectors[key] = t;
                        continue;
                    }
                    selectors[key] = selector;

                }
            }
        }
    }
}}}}
