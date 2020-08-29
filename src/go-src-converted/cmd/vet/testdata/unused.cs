// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains tests for the unusedresult checker.

// package testdata -- go2cs converted at 2020 August 29 10:10:40 UTC
// import "cmd/vet/testdata" ==> using testdata = go.cmd.vet.testdata_package
// Original source: C:\Go\src\cmd\vet\testdata\unused.go
using bytes = go.bytes_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vet
{
    public static partial class testdata_package
    {
        private static void _()
        {
            fmt.Errorf(""); // ERROR "result of fmt.Errorf call not used"
            _ = fmt.Errorf("");

            errors.New(""); // ERROR "result of errors.New call not used"

            var err = errors.New("");
            err.Error(); // ERROR "result of \(error\).Error call not used"

            bytes.Buffer buf = default;
            buf.String(); // ERROR "result of \(bytes.Buffer\).String call not used"

            fmt.Sprint(""); // ERROR "result of fmt.Sprint call not used"
            fmt.Sprintf(""); // ERROR "result of fmt.Sprintf call not used"
        }
    }
}}}
