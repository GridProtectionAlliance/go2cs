// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !plan9

// package main -- go2cs converted at 2020 August 29 08:25:00 UTC
// Original source: C:\Go\src\runtime\testdata\testprogcgo\threadpanic.go
// void start(void);
using C = go.C_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("CgoExternalThreadPanic", CgoExternalThreadPanic);
        }

        public static void CgoExternalThreadPanic()
        {
            C.start();
        }

        //export gopanic
        private static void gopanic() => func((_, panic, __) =>
        {
            panic("BOOM");
        });
    }
}
