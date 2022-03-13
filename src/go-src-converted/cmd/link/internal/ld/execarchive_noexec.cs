// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build wasm || windows
// +build wasm windows

// package ld -- go2cs converted at 2022 March 13 06:34:22 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\execarchive_noexec.go
namespace go.cmd.link.@internal;

public static partial class ld_package {

private static readonly var syscallExecSupported = false;



private static void execArchive(this ptr<Link> _addr_ctxt, slice<@string> argv) => func((_, panic, _) => {
    ref Link ctxt = ref _addr_ctxt.val;

    panic("should never arrive here");
});

} // end ld_package
