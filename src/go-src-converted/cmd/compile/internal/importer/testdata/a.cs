// UNREVIEWED
// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Input for TestIssue13566

// package a -- go2cs converted at 2022 March 13 06:27:22 UTC
// import "cmd/compile/internal/importer.a" ==> using a = go.cmd.compile.@internal.importer.a_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\importer\testdata\a.go
namespace go.cmd.compile.@internal;

using json = encoding.json_package;

public static partial class a_package {

public partial struct A {
    public ptr<A> a;
    public json.RawMessage json;
}

} // end a_package
