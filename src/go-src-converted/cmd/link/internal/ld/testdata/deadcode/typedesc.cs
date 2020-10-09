// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Test that a live variable doesn't bring its type
// descriptor live.

// package main -- go2cs converted at 2020 October 09 05:50:36 UTC
// Original source: C:\Go\src\cmd\link\internal\ld\testdata\deadcode\typedesc.go

using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        public partial struct T // : array<@string>
        {
        }

        private static T t = default;

        private static void Main()
        {
            println(t[8L]);
        }
    }
}
