// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package riscv64 -- go2cs converted at 2022 March 06 23:10:52 UTC
// import "cmd/compile/internal/riscv64" ==> using riscv64 = go.cmd.compile.@internal.riscv64_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\riscv64\gsubr.go
using objw = go.cmd.compile.@internal.objw_package;
using obj = go.cmd.@internal.obj_package;
using riscv = go.cmd.@internal.obj.riscv_package;

namespace go.cmd.compile.@internal;

public static partial class riscv64_package {

private static ptr<obj.Prog> ginsnop(ptr<objw.Progs> _addr_pp) {
    ref objw.Progs pp = ref _addr_pp.val;
 
    // Hardware nop is ADD $0, ZERO
    var p = pp.Prog(riscv.AADD);
    p.From.Type = obj.TYPE_CONST;
    p.Reg = riscv.REG_ZERO;
    p.To = new obj.Addr(Type:obj.TYPE_REG,Reg:riscv.REG_ZERO);
    return _addr_p!;

}

} // end riscv64_package
