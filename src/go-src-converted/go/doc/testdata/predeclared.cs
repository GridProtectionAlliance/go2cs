// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package predeclared is a go/doc test for handling of
// exported methods on locally-defined predeclared types.
// See issue 9860.

// package predeclared -- go2cs converted at 2022 March 13 05:52:40 UTC
// import "go/doc.predeclared" ==> using predeclared = go.go.doc.predeclared_package
// Original source: C:\Program Files\Go\src\go\doc\testdata\predeclared.go
namespace go.go;

public static partial class predeclared_package {

private partial struct error {
}

// Must not be visible.
private static @string Error(this error e) {
    return "";
}

private partial struct @bool { // : nint
}

// Must not be visible.
private static @string String(this bool b) {
    return "";
}

} // end predeclared_package
