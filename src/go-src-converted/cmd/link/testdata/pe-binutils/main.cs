// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Test that a PE rsrc section is handled correctly (issue 39658).
//
// rsrc.syso is created using binutils with:
//    {x86_64,i686}-w64-mingw32-windres -i a.rc -o rsrc_$GOARCH.syso -O coff
// where a.rc is a text file with the following content:
//
// resname RCDATA {
//   "Hello Gophers!\0",
//   "This is a test.\0",
// }

// package main -- go2cs converted at 2022 March 13 06:35:39 UTC
// Original source: C:\Program Files\Go\src\cmd\link\testdata\pe-binutils\main.go
namespace go;

public static partial class main_package {

private static void Main() {
}

} // end main_package
