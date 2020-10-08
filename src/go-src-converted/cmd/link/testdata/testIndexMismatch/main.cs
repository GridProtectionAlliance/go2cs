// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 08 04:39:54 UTC
// Original source: C:\Go\src\cmd\link\testdata\testIndexMismatch\main.go
using a = go.a_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {
            a.A();
        }
    }
}
