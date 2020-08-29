// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 August 29 08:24:47 UTC
// Original source: C:\Go\src\runtime\testdata\testprogcgo\deadlock.go
/*
char *geterror() {
    return "cgo error";
}
*/
using C = go.C_package;/*
char *geterror() {
    return "cgo error";
}
*/

using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("CgoPanicDeadlock", CgoPanicDeadlock);
        }

        private partial struct cgoError
        {
        }

        private static @string Error(this cgoError _p0)
        {
            fmt.Print(""); // necessary to trigger the deadlock
            return C.GoString(C.geterror());
        }

        public static void CgoPanicDeadlock() => func((_, panic, __) =>
        {
            panic(new cgoError());
        });
    }
}
