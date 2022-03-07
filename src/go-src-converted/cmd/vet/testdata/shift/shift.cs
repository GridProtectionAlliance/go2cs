// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains tests for the suspicious shift checker.

// package shift -- go2cs converted at 2022 March 06 23:35:21 UTC
// import "cmd/vet/testdata/shift" ==> using shift = go.cmd.vet.testdata.shift_package
// Original source: C:\Program Files\Go\src\cmd\vet\testdata\shift\shift.go


namespace go.cmd.vet.testdata;

public static partial class shift_package {

public static void ShiftTest() {
    sbyte i8 = default;
    _ = i8 << 7;
    _ = (i8 + 1) << 8; // ERROR ".i8 . 1. .8 bits. too small for shift of 8"
}

} // end shift_package
