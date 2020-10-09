// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build wasm windows

// package ld -- go2cs converted at 2020 October 09 05:49:39 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\execarchive_noexec.go

using static go.builtin;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class ld_package
    {
        private static readonly var syscallExecSupported = false;



        private static void execArchive(this ptr<Link> _addr_ctxt, slice<@string> argv) => func((_, panic, __) =>
        {
            ref Link ctxt = ref _addr_ctxt.val;

            panic("should never arrive here");
        });
    }
}}}}
