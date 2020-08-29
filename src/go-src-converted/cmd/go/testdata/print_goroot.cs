// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 August 29 10:02:01 UTC
// Original source: C:\Go\src\cmd\go\testdata\print_goroot.go
using runtime = go.runtime_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {
            println(runtime.GOROOT());
        }
    }
}
