// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file encapsulates some of the odd characteristics of the RISCV64
// instruction set, to minimize its interaction with the core of the
// assembler.

// package arch -- go2cs converted at 2022 March 13 05:57:52 UTC
// import "cmd/asm/internal/arch" ==> using arch = go.cmd.asm.@internal.arch_package
// Original source: C:\Program Files\Go\src\cmd\asm\internal\arch\riscv64.go
namespace go.cmd.asm.@internal;

using obj = cmd.@internal.obj_package;
using riscv = cmd.@internal.obj.riscv_package;


// IsRISCV64AMO reports whether the op (as defined by a riscv.A*
// constant) is one of the AMO instructions that requires special
// handling.

public static partial class arch_package {

public static bool IsRISCV64AMO(obj.As op) {

    if (op == riscv.ASCW || op == riscv.ASCD || op == riscv.AAMOSWAPW || op == riscv.AAMOSWAPD || op == riscv.AAMOADDW || op == riscv.AAMOADDD || op == riscv.AAMOANDW || op == riscv.AAMOANDD || op == riscv.AAMOORW || op == riscv.AAMOORD || op == riscv.AAMOXORW || op == riscv.AAMOXORD || op == riscv.AAMOMINW || op == riscv.AAMOMIND || op == riscv.AAMOMINUW || op == riscv.AAMOMINUD || op == riscv.AAMOMAXW || op == riscv.AAMOMAXD || op == riscv.AAMOMAXUW || op == riscv.AAMOMAXUD) 
        return true;
        return false;
}

} // end arch_package
