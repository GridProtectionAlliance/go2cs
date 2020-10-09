// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package nilfunc -- go2cs converted at 2020 October 09 06:05:09 UTC
// import "cmd/vet/testdata/nilfunc" ==> using nilfunc = go.cmd.vet.testdata.nilfunc_package
// Original source: C:\Go\src\cmd\vet\testdata\nilfunc\nilfunc.go

using static go.builtin;

namespace go {
namespace cmd {
namespace vet {
namespace testdata
{
    public static partial class nilfunc_package
    {
        public static void F()
        {
        }

        public static void Comparison() => func((_, panic, __) =>
        {
            if (F == null)
            { // ERROR "comparison of function F == nil is always false"
                panic("can't happen");

            }

        });
    }
}}}}
