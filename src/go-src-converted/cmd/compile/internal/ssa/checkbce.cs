// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 13 06:00:47 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\checkbce.go
namespace go.cmd.compile.@internal;

using logopt = cmd.compile.@internal.logopt_package;

public static partial class ssa_package {

// checkbce prints all bounds checks that are present in the function.
// Useful to find regressions. checkbce is only activated when with
// corresponding debug options, so it's off by default.
// See test/checkbce.go
private static void checkbce(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    if (f.pass.debug <= 0 && !logopt.Enabled()) {
        return ;
    }
    foreach (var (_, b) in f.Blocks) {
        foreach (var (_, v) in b.Values) {
            if (v.Op == OpIsInBounds || v.Op == OpIsSliceInBounds) {
                if (f.pass.debug > 0) {
                    f.Warnl(v.Pos, "Found %v", v.Op);
                }
                if (logopt.Enabled()) {
                    if (v.Op == OpIsInBounds) {
                        logopt.LogOpt(v.Pos, "isInBounds", "checkbce", f.Name);
                    }
                    if (v.Op == OpIsSliceInBounds) {
                        logopt.LogOpt(v.Pos, "isSliceInBounds", "checkbce", f.Name);
                    }
                }
            }
        }
    }
}

} // end ssa_package
