// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package walk -- go2cs converted at 2022 March 13 06:25:19 UTC
// import "cmd/compile/internal/walk" ==> using walk = go.cmd.compile.@internal.walk_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\walk\race.go
namespace go.cmd.compile.@internal;

using @base = cmd.compile.@internal.@base_package;
using ir = cmd.compile.@internal.ir_package;
using types = cmd.compile.@internal.types_package;
using src = cmd.@internal.src_package;

public static partial class walk_package {

private static void instrument(ptr<ir.Func> _addr_fn) {
    ref ir.Func fn = ref _addr_fn.val;

    if (fn.Pragma & ir.Norace != 0 || (fn.Linksym() != null && fn.Linksym().ABIWrapper())) {
        return ;
    }
    if (!@base.Flag.Race || !@base.Compiling(@base.NoRacePkgs)) {
        fn.SetInstrumentBody(true);
    }
    if (@base.Flag.Race) {
        var lno = @base.Pos;
        @base.Pos = src.NoXPos;
        ref ir.Nodes init = ref heap(out ptr<ir.Nodes> _addr_init);
        fn.Enter.Prepend(mkcallstmt("racefuncenter", mkcall("getcallerpc", types.Types[types.TUINTPTR], _addr_init)));
        if (len(init) != 0) {
            @base.Fatalf("race walk: unexpected init for getcallerpc");
        }
        fn.Exit.Append(mkcallstmt("racefuncexit"));
        @base.Pos = lno;
    }
}

} // end walk_package
