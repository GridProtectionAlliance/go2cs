// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package wasm -- go2cs converted at 2020 October 09 05:51:15 UTC
// import "cmd/oldlink/internal/wasm" ==> using wasm = go.cmd.oldlink.@internal.wasm_package
// Original source: C:\Go\src\cmd\oldlink\internal\wasm\obj.go
using sys = go.cmd.@internal.sys_package;
using ld = go.cmd.oldlink.@internal.ld_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace oldlink {
namespace @internal
{
    public static partial class wasm_package
    {
        public static (ptr<sys.Arch>, ld.Arch) Init()
        {
            ptr<sys.Arch> _p0 = default!;
            ld.Arch _p0 = default;

            ld.Arch theArch = new ld.Arch(Funcalign:16,Maxalign:32,Minalign:1,Archinit:archinit,AssignAddress:assignAddress,Asmb:asmb,Asmb2:asmb2,Gentext:gentext,);

            return (_addr_sys.ArchWasm!, theArch);
        }

        private static void archinit(ptr<ld.Link> _addr_ctxt)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;

            if (ld.FlagRound == -1L.val)
            {
                ld.FlagRound.val = 4096L;
            }

            if (ld.FlagTextAddr == -1L.val)
            {
                ld.FlagTextAddr.val = 0L;
            }

        }
    }
}}}}
