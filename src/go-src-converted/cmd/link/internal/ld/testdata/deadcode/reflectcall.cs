// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This example uses reflect.Value.Call, but not
// reflect.{Value,Type}.Method. This should not
// need to bring all methods live.

// package main -- go2cs converted at 2020 October 09 05:50:36 UTC
// Original source: C:\Go\src\cmd\link\internal\ld\testdata\deadcode\reflectcall.go
using reflect = go.reflect_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void f()
        {
            println("call");
        }

        public partial struct T // : long
        {
        }

        public static void M(this T _p0)
        {
        }

        private static void Main()
        {
            var v = reflect.ValueOf(f);
            v.Call(null);
            println(i);
        }
    }
}
