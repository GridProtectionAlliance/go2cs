// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file is processed by the cover command, then a test verifies that
// all compiler directives are preserved and positioned appropriately.

//go:a

//go:b

// package main -- go2cs converted at 2022 March 13 06:28:40 UTC
// Original source: C:\Program Files\Go\src\cmd\cover\testdata\directives.go
namespace go;

public static partial class main_package {

//go:c1

//go:c2
//doc
private static void c() {
}

//go:d1

//doc
//go:d2
private partial struct d { // : nint
}

//go:e1

//doc
//go:e2
private partial struct e { // : nint
}
private partial struct f { // : nint
}
} // end main_package
