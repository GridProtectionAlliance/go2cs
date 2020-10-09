// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 09 05:00:48 UTC
// Original source: C:\Go\src\runtime\testdata\testprog\misc.go
using runtime = go.runtime_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("NumGoroutine", NumGoroutine);
        }

        public static void NumGoroutine()
        {
            println(runtime.NumGoroutine());
        }
    }
}
