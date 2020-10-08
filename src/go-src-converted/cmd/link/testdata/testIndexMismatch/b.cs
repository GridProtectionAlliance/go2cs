// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package a -- go2cs converted at 2020 October 08 04:39:54 UTC
// import "cmd/link/testdata.a" ==> using a = go.cmd.link.testdata.a_package
// Original source: C:\Go\src\cmd\link\testdata\testIndexMismatch\b.go

using static go.builtin;

namespace go {
namespace cmd {
namespace link
{
    public static partial class a_package
    {
        //go:noinline
        public static void B()
        {
            println("B");
        }
    }
}}}
