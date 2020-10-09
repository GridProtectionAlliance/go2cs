// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !testtag

// package main -- go2cs converted at 2020 October 09 06:05:12 UTC
// Original source: C:\Go\src\cmd\vet\testdata\tagtest\file2.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {
            fmt.Printf("%s", 0L);
        }
    }
}
