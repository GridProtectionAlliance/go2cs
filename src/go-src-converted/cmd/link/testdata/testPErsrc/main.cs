// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Test that a PE rsrc section is handled correctly (issue 39658).
//
// rsrc.syso is created with:
//    windres -i a.rc -o rsrc.syso -O coff
// on windows-amd64-2016 builder, where a.rc is a text file with
// the following content:
//
// resname RCDATA {
//   "Hello Gophers!\0",
//   "This is a test.\0",
// }

// package main -- go2cs converted at 2020 October 09 05:50:38 UTC
// Original source: C:\Go\src\cmd\link\testdata\testPErsrc\main.go

using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {
        }
    }
}
