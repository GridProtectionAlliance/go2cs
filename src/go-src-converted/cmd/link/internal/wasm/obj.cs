// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package wasm -- go2cs converted at 2020 October 08 04:37:58 UTC
// import "cmd/link/internal/wasm" ==> using wasm = go.cmd.link.@internal.wasm_package
// Original source: C:\Go\src\cmd\link\internal\wasm\obj.go
using sys = go.cmd.@internal.sys_package;
using ld = go.cmd.link.@internal.ld_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class wasm_package
    {
        public static (ptr<sys.Arch>, ld.Arch) Init()
        {
            ptr<sys.Arch> _p0 = default!;
            ld.Arch _p0 = default;

            ld.Arch theArch = new ld.Arch(Funcalign:16,Maxalign:32,Minalign:1,Archinit:archinit,AssignAddress:assignAddress,Asmb:asmb,Asmb2:asmb2,Gentext2:gentext2,);

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
