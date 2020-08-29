// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build linux netbsd dragonfly nacl

// package os -- go2cs converted at 2020 August 29 08:43:44 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\executable_procfs.go
using errors = go.errors_package;
using runtime = go.runtime_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class os_package
    {
        // We query the executable path at init time to avoid the problem of
        // readlink returns a path appended with " (deleted)" when the original
        // binary gets deleted.


        private static (@string, error) executable()
        {
            return (executablePath, executablePathErr);
        }
    }
}
