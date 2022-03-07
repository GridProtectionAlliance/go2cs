// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains tests for the buildtag checker.

// ERRORNEXT "possible malformed [+]build comment"
// +builder
// +build !ignore

// package testdata -- go2cs converted at 2022 March 06 23:35:18 UTC
// import "cmd/vet/testdata.testdata" ==> using testdata = go.cmd.vet.testdata.testdata_package
// Original source: C:\Program Files\Go\src\cmd\vet\testdata\buildtag\buildtag2.go


namespace go.cmd.vet;

public static partial class testdata_package {

    // ERRORNEXT "misplaced \+build comment"
    // +build toolate
    // ERRORNEXT "misplaced //go:build comment"
    //go:build toolate
private static nint _ = 3;

private static @string _ = "\n// +build notacomment\n";

} // end testdata_package
