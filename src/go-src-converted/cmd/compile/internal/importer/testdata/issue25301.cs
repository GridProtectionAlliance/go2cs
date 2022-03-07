// UNREVIEWED
// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package issue25301 -- go2cs converted at 2022 March 06 23:13:55 UTC
// import "cmd/compile/internal/importer.issue25301" ==> using issue25301 = go.cmd.compile.@internal.importer.issue25301_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\importer\testdata\issue25301.go


namespace go.cmd.compile.@internal;

public static partial class issue25301_package {

public partial interface A {
    void M();
}
public partial interface T {
}
public partial struct S {
}
public static void M(this S _p0) {
    println("m");
}

} // end issue25301_package
