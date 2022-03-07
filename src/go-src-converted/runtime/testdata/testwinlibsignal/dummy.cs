//go:build windows
// +build windows

// package main -- go2cs converted at 2022 March 06 22:26:22 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testwinlibsignal\dummy.go
using C = go.C_package;

namespace go;

public static partial class main_package {

    //export Dummy
public static nint Dummy() {
    return 42;
}

private static void Main() {
}

} // end main_package
