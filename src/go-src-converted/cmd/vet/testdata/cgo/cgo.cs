// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains tests for the cgo checker.

// package testdata -- go2cs converted at 2020 August 29 10:10:40 UTC
// import "cmd/vet/testdata.testdata" ==> using testdata = go.cmd.vet.testdata.testdata_package
// Original source: C:\Go\src\cmd\vet\testdata\cgo\cgo.go
// void f(void *);
using C = go.C_package;// void f(void *);


using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace vet
{
    public static unsafe partial class testdata_package
    {
        public static void CgoTests()
        {
            channel<bool> c = default;
            C.f(@unsafe.Pointer(ref c).Value); // ERROR "embedded pointer"
            C.f(@unsafe.Pointer(ref c)); // ERROR "embedded pointer"

            map<@string, @string> m = default;
            C.f(@unsafe.Pointer(ref m).Value); // ERROR "embedded pointer"
            C.f(@unsafe.Pointer(ref m)); // ERROR "embedded pointer"

            Action f = default;
            C.f(@unsafe.Pointer(ref f).Value); // ERROR "embedded pointer"
            C.f(@unsafe.Pointer(ref f)); // ERROR "embedded pointer"

            slice<long> s = default;
            C.f(@unsafe.Pointer(ref s).Value); // ERROR "embedded pointer"
            C.f(@unsafe.Pointer(ref s)); // ERROR "embedded pointer"

            array<slice<long>> a = new array<slice<long>>(1L);
            C.f(@unsafe.Pointer(ref a).Value); // ERROR "embedded pointer"
            C.f(@unsafe.Pointer(ref a)); // ERROR "embedded pointer"

            var st = default;
            C.f(@unsafe.Pointer(ref st).Value); // ERROR "embedded pointer"
            C.f(@unsafe.Pointer(ref st)); // ERROR "embedded pointer"

            // The following cases are OK.
            long i = default;
            C.f(@unsafe.Pointer(ref i).Value);
            C.f(@unsafe.Pointer(ref i));

            C.f(@unsafe.Pointer(ref s[0L]).Value);
            C.f(@unsafe.Pointer(ref s[0L]));

            array<long> a2 = new array<long>(1L);
            C.f(@unsafe.Pointer(ref a2).Value);
            C.f(@unsafe.Pointer(ref a2));

            var st2 = default;
            C.f(@unsafe.Pointer(ref st2).Value);
            C.f(@unsafe.Pointer(ref st2));

            private partial struct cgoStruct
            {
                public ptr<cgoStruct> p;
            }
            C.f(@unsafe.Pointer(ref new cgoStruct()));

            C.CBytes((slice<byte>)"hello");
        }
    }
}}}
