// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package unsafeptr -- go2cs converted at 2022 March 06 23:35:22 UTC
// import "cmd/vet/testdata/unsafeptr" ==> using unsafeptr = go.cmd.vet.testdata.unsafeptr_package
// Original source: C:\Program Files\Go\src\cmd\vet\testdata\unsafeptr\unsafeptr.go
using @unsafe = go.@unsafe_package;

namespace go.cmd.vet.testdata;

public static partial class unsafeptr_package {

private static void _() {
    unsafe.Pointer x = default;
    System.UIntPtr y = default;
    x = @unsafe.Pointer(y); // ERROR "possible misuse of unsafe.Pointer"
    _ = x;

}

} // end unsafeptr_package
