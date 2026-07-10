// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build gc
namespace go.go;

using filepath = global::go.path.filepath_package;
using runtime = runtime_package;
using global::go.path;

partial class build_package {

// getToolDir returns the default value of ToolDir.
internal static @string getToolDir() {
    return filepath.Join(runtime.GOROOT(), "pkg/tool/" + runtime.GOOS + "_" + runtime.GOARCH);
}

} // end build_package
