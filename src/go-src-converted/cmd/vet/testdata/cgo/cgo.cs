// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains tests for the cgo checker.

// package testdata -- go2cs converted at 2020 October 09 06:05:08 UTC
// import "cmd/vet/testdata.testdata" ==> using testdata = go.cmd.vet.testdata.testdata_package
// Original source: C:\Go\src\cmd\vet\testdata\cgo\cgo.go
// void f(void *p) {}
using C = go.C_package;// void f(void *p) {}


using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vet
{
    public static partial class testdata_package
    {
        public static void CgoTests()
        {
            ref channel<bool> c = ref heap(out ptr<channel<bool>> _addr_c);
            C.f(new ptr<ptr<ptr<unsafe.Pointer>>>(@unsafe.Pointer(_addr_c))); // ERROR "embedded pointer"
            C.f(@unsafe.Pointer(_addr_c)); // ERROR "embedded pointer"
        }
    }
}}}
