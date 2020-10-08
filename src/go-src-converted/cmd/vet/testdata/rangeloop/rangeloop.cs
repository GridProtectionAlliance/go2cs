// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains tests for the rangeloop checker.

// package rangeloop -- go2cs converted at 2020 October 08 04:58:39 UTC
// import "cmd/vet/testdata/rangeloop" ==> using rangeloop = go.cmd.vet.testdata.rangeloop_package
// Original source: C:\Go\src\cmd\vet\testdata\rangeloop\rangeloop.go

using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace cmd {
namespace vet {
namespace testdata
{
    public static partial class rangeloop_package
    {
        public static void RangeLoopTests()
        {
            slice<long> s = default;
            foreach (var (i, v) in s)
            {
                go_(() => () =>
                {
                    println(i); // ERROR "loop variable i captured by func literal"
                    println(v); // ERROR "loop variable v captured by func literal"
                }());

            }
        }
    }
}}}}
