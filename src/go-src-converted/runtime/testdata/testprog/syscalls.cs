// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 09 05:00:50 UTC
// Original source: C:\Go\src\runtime\testdata\testprog\syscalls.go
using errors = go.errors_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static var errNotPermitted = errors.New("operation not permitted");
    }
}
