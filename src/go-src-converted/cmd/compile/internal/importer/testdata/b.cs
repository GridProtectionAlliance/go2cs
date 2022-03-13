// UNREVIEWED
// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Input for TestIssue13566

// package b -- go2cs converted at 2022 March 13 06:27:22 UTC
// import "cmd/compile/internal/importer.b" ==> using b = go.cmd.compile.@internal.importer.b_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\importer\testdata\b.go
namespace go.cmd.compile.@internal;

using a = ..a_package;

public static partial class b_package {

public partial struct A { // : a.A
}

} // end b_package
