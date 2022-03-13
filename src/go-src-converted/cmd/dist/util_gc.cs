// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build gc
// +build gc

// package main -- go2cs converted at 2022 March 13 06:29:04 UTC
// Original source: C:\Program Files\Go\src\cmd\dist\util_gc.go
namespace go;

public static partial class main_package {

// useVFPv1 tries to execute one VFPv1 instruction on ARM.
// It will crash the current process if VFPv1 is missing.
private static void useVFPv1();

// useVFPv3 tries to execute one VFPv3 instruction on ARM.
// It will crash the current process if VFPv3 is missing.
private static void useVFPv3();

// useARMv6K tries to run ARMv6K instructions on ARM.
// It will crash the current process if it doesn't implement
// ARMv6K or above.
private static void useARMv6K();

} // end main_package
