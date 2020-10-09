// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file is a copy of $GOROOT/src/go/internal/gcimporter/testdata/versions.test.go.

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

// package test -- go2cs converted at 2020 October 09 06:02:21 UTC
// import "golang.org/x/tools/go/internal/gcimporter/testdata.test" ==> using test = go.golang.org.x.tools.go.@internal.gcimporter.testdata.test_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\internal\gcimporter\testdata\versions\test.go

using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace @internal {
namespace gcimporter
{
    public static partial class test_package
    {
        // Any release before and including Go 1.7 didn't encode
        // the package for a blank struct field.
        public partial struct BlankField
        {
            public long _;
        }
    }
}}}}}}}
