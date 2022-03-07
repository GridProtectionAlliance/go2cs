// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build go1.12
// +build go1.12

// package unitchecker -- go2cs converted at 2022 March 06 23:34:57 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis/unitchecker" ==> using unitchecker = go.cmd.vendor.golang.org.x.tools.go.analysis.unitchecker_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\unitchecker\unitchecker112.go
using importer = go.go.importer_package;

namespace go.cmd.vendor.golang.org.x.tools.go.analysis;

public static partial class unitchecker_package {

private static void init() {
    importerForCompiler = importer.ForCompiler;
}

} // end unitchecker_package
