// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build go1.12
// +build go1.12

// package unitchecker -- go2cs converted at 2022 March 13 06:42:30 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis/unitchecker" ==> using unitchecker = go.cmd.vendor.golang.org.x.tools.go.analysis.unitchecker_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\unitchecker\unitchecker112.go
namespace go.cmd.vendor.golang.org.x.tools.go.analysis;

using importer = go.importer_package;

public static partial class unitchecker_package {

private static void init() {
    importerForCompiler = importer.ForCompiler;
}

} // end unitchecker_package
