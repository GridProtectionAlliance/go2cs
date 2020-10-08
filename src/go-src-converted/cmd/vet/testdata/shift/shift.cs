// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains tests for the suspicious shift checker.

// package shift -- go2cs converted at 2020 October 08 04:58:39 UTC
// import "cmd/vet/testdata/shift" ==> using shift = go.cmd.vet.testdata.shift_package
// Original source: C:\Go\src\cmd\vet\testdata\shift\shift.go

using static go.builtin;

namespace go {
namespace cmd {
namespace vet {
namespace testdata
{
    public static partial class shift_package
    {
        public static void ShiftTest()
        {
            sbyte i8 = default;
            _ = i8 << (int)(7L);
            _ = (i8 + 1L) << (int)(8L); // ERROR ".i8 . 1. .8 bits. too small for shift of 8"
        }
    }
}}}}
