// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains tests for the bool checker.

// package @bool -- go2cs converted at 2020 October 08 04:58:36 UTC
// import "cmd/vet/testdata/bool" ==> using @bool = go.cmd.vet.testdata.@bool_package
// Original source: C:\Go\src\cmd\vet\testdata\bool\bool.go

using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace vet {
namespace testdata
{
    public static partial class @bool_package
    {
        private static void _()
        {
            Func<long> f = default;            Func<long> g = default;



            {
                var v = f();
                var w = g();

                if (v == w || v == w)
                { // ERROR "redundant or: v == w || v == w"
                }
            }

        }
    }
}}}}
