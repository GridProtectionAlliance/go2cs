// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:12:29 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\vdso_linux_arm64.go


namespace go;

public static partial class runtime_package {

 
// vdsoArrayMax is the byte-size of a maximally sized array on this architecture.
// See cmd/compile/internal/arm64/galign.go arch.MAXWIDTH initialization.
private static readonly nint vdsoArrayMax = 1 << 50 - 1;


// key and version at man 7 vdso : aarch64
private static vdsoVersionKey vdsoLinuxVersion = new vdsoVersionKey("LINUX_2.6.39",0x75fcb89);

private static vdsoSymbolKey vdsoSymbolKeys = new slice<vdsoSymbolKey>(new vdsoSymbolKey[] { {"__kernel_clock_gettime",0xd35ec75,0x6e43a318,&vdsoClockgettimeSym} });

// initialize to fall back to syscall
private static System.UIntPtr vdsoClockgettimeSym = 0;

} // end runtime_package
