// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build linux && (ppc64 || ppc64le)
// +build linux
// +build ppc64 ppc64le

// package runtime -- go2cs converted at 2022 March 13 05:27:32 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\vdso_linux_ppc64x.go
namespace go;

public static partial class runtime_package {

 
// vdsoArrayMax is the byte-size of a maximally sized array on this architecture.
// See cmd/compile/internal/ppc64/galign.go arch.MAXWIDTH initialization.
private static readonly nint vdsoArrayMax = 1 << 50 - 1;

private static vdsoVersionKey vdsoLinuxVersion = new vdsoVersionKey("LINUX_2.6.15",0x75fcba5);

private static vdsoSymbolKey vdsoSymbolKeys = new slice<vdsoSymbolKey>(new vdsoSymbolKey[] { {"__kernel_clock_gettime",0xb0cd725,0xdfa941fd,&vdsoClockgettimeSym} });

// initialize with vsyscall fallbacks
private static System.UIntPtr vdsoClockgettimeSym = 0;

} // end runtime_package
