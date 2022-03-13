// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package wasm -- go2cs converted at 2022 March 13 06:33:32 UTC
// import "cmd/link/internal/wasm" ==> using wasm = go.cmd.link.@internal.wasm_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\wasm\obj.go
namespace go.cmd.link.@internal;

using sys = cmd.@internal.sys_package;
using ld = cmd.link.@internal.ld_package;

public static partial class wasm_package {

public static (ptr<sys.Arch>, ld.Arch) Init() {
    ptr<sys.Arch> _p0 = default!;
    ld.Arch _p0 = default;

    ld.Arch theArch = new ld.Arch(Funcalign:16,Maxalign:32,Minalign:1,Archinit:archinit,AssignAddress:assignAddress,Asmb:asmb,Asmb2:asmb2,Gentext:gentext,);

    return (_addr_sys.ArchWasm!, theArch);
}

private static void archinit(ptr<ld.Link> _addr_ctxt) {
    ref ld.Link ctxt = ref _addr_ctxt.val;

    if (ld.FlagRound == -1.val) {
        ld.FlagRound.val = 4096;
    }
    if (ld.FlagTextAddr == -1.val) {
        ld.FlagTextAddr.val = 0;
    }
}

} // end wasm_package
