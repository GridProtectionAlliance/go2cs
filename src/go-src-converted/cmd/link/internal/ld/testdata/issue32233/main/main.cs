// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 08 04:39:53 UTC
// Original source: C:\Go\src\cmd\link\internal\ld\testdata\issue32233\main\main.go
using lib = go.cmd.link.@internal.ld.testdata.issue32233.lib_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {
            lib.DoC();
        }
    }
}
