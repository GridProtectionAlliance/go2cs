// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package testdata -- go2cs converted at 2020 August 29 10:10:33 UTC
// import "cmd/vet/testdata" ==> using testdata = go.cmd.vet.testdata_package
// Original source: C:\Go\src\cmd\vet\testdata\nilfunc.go

using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace vet
{
    public static partial class testdata_package
    {
        public static void F()
        {
        }

        public partial struct T
        {
            public Action F;
        }

        public static void M(this T _p0)
        {
        }

        public static var Fv = F;

        public static void Comparison() => func((_, panic, __) =>
        {
            T t = default;
            Action fn = default;
            if (fn == null || Fv == null || t.F == null)
            { 
                // no error; these func vars or fields may be nil
            }
            if (F == null)
            { // ERROR "comparison of function F == nil is always false"
                panic("can't happen");
            }
            if (t.M == null)
            { // ERROR "comparison of function M == nil is always false"
                panic("can't happen");
            }
            if (F != null)
            { // ERROR "comparison of function F != nil is always true"
                if (t.M != null)
                { // ERROR "comparison of function M != nil is always true"
                    return;
                }
            }
            panic("can't happen");
        });
    }
}}}
