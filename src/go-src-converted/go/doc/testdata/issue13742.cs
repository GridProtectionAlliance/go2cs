// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package issue13742 -- go2cs converted at 2022 March 13 05:52:40 UTC
// import "go/doc.issue13742" ==> using issue13742 = go.go.doc.issue13742_package
// Original source: C:\Program Files\Go\src\go\doc\testdata\issue13742.go
namespace go.go;

using ast = go.ast_package;
using ast = go.ast_package;


// Both F0 and G0 should appear as functions.

public static partial class issue13742_package {

public static void F0(Node _p0) {
}
public static Node G0() {
    return null;
}

// Both F1 and G1 should appear as functions.
public static void F1(ast.Node _p0) {
}
public static ast.Node G1() {
    return null;
}

} // end issue13742_package
