// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file is processed by the cover command, then a test verifies that
// all compiler directives are preserved and positioned appropriately.

//go:a

//go:b
// package main -- go2cs converted at 2020 August 29 09:59:30 UTC
// Original source: C:\Go\src\cmd\cover\testdata\directives.go

using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        //go:c1

        //go:c2
        //doc
        private static void c()
        {
        }

        //go:d1

        //doc
        //go:d2
        private partial struct d // : long
        {
        }

        //go:e1

        //doc
        //go:e2
        private partial struct e // : long
        {
        }
        private partial struct f // : long
        {
        }    }
}
