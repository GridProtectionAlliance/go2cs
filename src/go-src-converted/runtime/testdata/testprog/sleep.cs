// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 08 03:43:44 UTC
// Original source: C:\Go\src\runtime\testdata\testprog\sleep.go
using time = go.time_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        // for golang.org/issue/27250
        private static void init()
        {
            register("After1", After1);
        }

        public static void After1()
        {
            time.After(1L * time.Second).Receive();
        }
    }
}
