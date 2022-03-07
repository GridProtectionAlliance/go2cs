// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build cmd_go_bootstrap
// +build cmd_go_bootstrap

// package modfetch -- go2cs converted at 2022 March 06 23:16:08 UTC
// import "cmd/go/internal/modfetch" ==> using modfetch = go.cmd.go.@internal.modfetch_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modfetch\bootstrap.go
using module = go.golang.org.x.mod.module_package;

namespace go.cmd.go.@internal;

public static partial class modfetch_package {

private static bool useSumDB(module.Version mod) {
    return false;
}

private static (@string, slice<@string>, error) lookupSumDB(module.Version mod) => func((_, panic, _) => {
    @string _p0 = default;
    slice<@string> _p0 = default;
    error _p0 = default!;

    panic("bootstrap");
});

} // end modfetch_package
