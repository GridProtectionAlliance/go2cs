// UNREVIEWED
// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package p -- go2cs converted at 2022 March 13 06:27:22 UTC
// import "cmd/compile/internal/importer.p" ==> using p = go.cmd.compile.@internal.importer.p_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\importer\testdata\issue15920.go
namespace go.cmd.compile.@internal;

public static partial class p_package {

// The underlying type of Error is the underlying type of error.
// Make sure we can import this again without problems.
public partial struct Error { // : error
}

public static Error F() {
    return null;
}

} // end p_package
