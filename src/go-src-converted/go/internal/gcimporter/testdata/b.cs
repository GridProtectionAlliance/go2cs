// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Input for TestIssue13566

// package b -- go2cs converted at 2022 March 13 06:42:20 UTC
// import "go/internal/gcimporter.b" ==> using b = go.go.@internal.gcimporter.b_package
// Original source: C:\Program Files\Go\src\go\internal\gcimporter\testdata\b.go
namespace go.go.@internal;

using a = ..a_package;

public static partial class b_package {

public partial struct A { // : a.A
}

} // end b_package
