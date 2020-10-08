// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build gc

// package build -- go2cs converted at 2020 October 08 04:04:15 UTC
// import "go/build" ==> using build = go.go.build_package
// Original source: C:\Go\src\go\build\gc.go
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using static go.builtin;

namespace go {
namespace go
{
    public static partial class build_package
    {
        // getToolDir returns the default value of ToolDir.
        private static @string getToolDir()
        {
            return filepath.Join(runtime.GOROOT(), "pkg/tool/" + runtime.GOOS + "_" + runtime.GOARCH);
        }
    }
}}
