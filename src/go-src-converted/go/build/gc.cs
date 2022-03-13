// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build gc
// +build gc

// package build -- go2cs converted at 2022 March 13 05:54:01 UTC
// import "go/build" ==> using build = go.go.build_package
// Original source: C:\Program Files\Go\src\go\build\gc.go
namespace go.go;

using filepath = path.filepath_package;
using runtime = runtime_package;


// getToolDir returns the default value of ToolDir.

public static partial class build_package {

private static @string getToolDir() {
    return filepath.Join(runtime.GOROOT(), "pkg/tool/" + runtime.GOOS + "_" + runtime.GOARCH);
}

} // end build_package
