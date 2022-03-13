// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package mypkg -- go2cs converted at 2022 March 13 05:43:28 UTC
// import "cmd/internal/archive.mypkg" ==> using mypkg = go.cmd.@internal.archive.mypkg_package
// Original source: C:\Program Files\Go\src\cmd\internal\archive\testdata\go1.go
namespace go.cmd.@internal;

using fmt = fmt_package;

public static partial class mypkg_package {

private static void go1() {
    fmt.Println("go1");
}

} // end mypkg_package
