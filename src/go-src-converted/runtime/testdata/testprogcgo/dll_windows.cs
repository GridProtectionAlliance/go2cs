// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 06 22:26:11 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprogcgo\dll_windows.go
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

using windows = go.runtime.testdata.testprogcgo.windows_package;

namespace go;

public static partial class main_package {

private static void init() {
    register("CgoDLLImportsMain", CgoDLLImportsMain);
}

public static void CgoDLLImportsMain() {
    C.getthread();
    windows.GetThread();
    println("OK");
}

} // end main_package
