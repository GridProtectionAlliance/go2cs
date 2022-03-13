// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 13 06:02:06 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\opt.go
namespace go.cmd.compile.@internal;

public static partial class ssa_package {

// machine-independent optimization
private static void opt(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    applyRewrite(f, rewriteBlockgeneric, rewriteValuegeneric, removeDeadValues);
}

} // end ssa_package
