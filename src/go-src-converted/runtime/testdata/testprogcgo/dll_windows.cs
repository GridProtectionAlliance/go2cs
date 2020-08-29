// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 August 29 08:24:47 UTC
// Original source: C:\Go\src\runtime\testdata\testprogcgo\dll_windows.go
/*
#include <windows.h>

DWORD getthread() {
    return GetCurrentThreadId();
}
*/
using C = go.C_package;/*
#include <windows.h>

DWORD getthread() {
    return GetCurrentThreadId();
}
*/

using windows = go...windows_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("CgoDLLImportsMain", CgoDLLImportsMain);
        }

        public static void CgoDLLImportsMain()
        {
            C.getthread();
            windows.GetThread();
            println("OK");
        }
    }
}
