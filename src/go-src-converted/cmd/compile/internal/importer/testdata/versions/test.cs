// UNREVIEWED
// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// To create a test case for a new export format version,
// build this package with the latest compiler and store
// the resulting .a file appropriately named in the versions
// directory. The VersionHandling test will pick it up.
//
// In the testdata/versions:
//
// go build -o test_go1.$X_$Y.a test.go
//
// with $X = Go version and $Y = export format version
// (add 'b' or 'i' to distinguish between binary and
// indexed format starting with 1.11 as long as both
// formats are supported).
//
// Make sure this source is extended such that it exercises
// whatever export format change has taken place.

// package test -- go2cs converted at 2022 March 13 06:27:22 UTC
// import "cmd/compile/internal/importer/testdata.test" ==> using test = go.cmd.compile.@internal.importer.testdata.test_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\importer\testdata\versions\test.go
namespace go.cmd.compile.@internal.importer;

public static partial class test_package {

// Any release before and including Go 1.7 didn't encode
// the package for a blank struct field.
public partial struct BlankField {
    public nint _;
}

} // end test_package
