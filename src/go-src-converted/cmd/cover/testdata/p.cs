// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// A package such that there are 3 functions with zero total and covered lines.
// And one with 1 total and covered lines. Reproduces issue #20515.
// package p -- go2cs converted at 2020 October 09 05:44:25 UTC
// import "cmd/cover.p" ==> using p = go.cmd.cover.p_package
// Original source: C:\Go\src\cmd\cover\testdata\p.go

using static go.builtin;

namespace go {
namespace cmd
{
    public static partial class p_package
    {
        //go:noinline
        public static void A()
        {
        }

        //go:noinline
        public static void B()
        {
        }

        //go:noinline
        public static void C()
        {
        }

        //go:noinline
        public static long D()
        {
            return 42L;
        }
    }
}}
