// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 August 29 08:24:42 UTC
// Original source: C:\Go\src\runtime\testdata\testprog\stringconcat.go
using strings = go.strings_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("stringconcat", stringconcat);
        }

        private static void stringconcat() => func((_, panic, __) =>
        {
            var s0 = strings.Repeat("0", 1L << (int)(10L));
            var s1 = strings.Repeat("1", 1L << (int)(10L));
            var s2 = strings.Repeat("2", 1L << (int)(10L));
            var s3 = strings.Repeat("3", 1L << (int)(10L));
            var s = s0 + s1 + s2 + s3;
            panic(s);
        });
    }
}
