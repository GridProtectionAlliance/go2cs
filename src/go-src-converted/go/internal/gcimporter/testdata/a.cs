// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Input for TestIssue13566

// package a -- go2cs converted at 2022 March 06 23:32:40 UTC
// import "go/internal/gcimporter.a" ==> using a = go.go.@internal.gcimporter.a_package
// Original source: C:\Program Files\Go\src\go\internal\gcimporter\testdata\a.go
using json = go.encoding.json_package;

namespace go.go.@internal;

public static partial class a_package {

public partial struct A {
    public ptr<A> a;
    public json.RawMessage json;
}

} // end a_package
