// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package unsafeptr -- go2cs converted at 2020 October 09 06:05:12 UTC
// import "cmd/vet/testdata/unsafeptr" ==> using unsafeptr = go.cmd.vet.testdata.unsafeptr_package
// Original source: C:\Go\src\cmd\vet\testdata\unsafeptr\unsafeptr.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vet {
namespace testdata
{
    public static partial class unsafeptr_package
    {
        private static void _()
        {
            unsafe.Pointer x = default;
            System.UIntPtr y = default;
            x = @unsafe.Pointer(y); // ERROR "possible misuse of unsafe.Pointer"
            _ = x;

        }
    }
}}}}
