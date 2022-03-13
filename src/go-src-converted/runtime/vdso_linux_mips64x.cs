// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build linux && (mips64 || mips64le)
// +build linux
// +build mips64 mips64le

// package runtime -- go2cs converted at 2022 March 13 05:27:32 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\vdso_linux_mips64x.go
namespace go;

public static partial class runtime_package {

 
// vdsoArrayMax is the byte-size of a maximally sized array on this architecture.
// See cmd/compile/internal/mips64/galign.go arch.MAXWIDTH initialization.
private static readonly nint vdsoArrayMax = 1 << 50 - 1;

// see man 7 vdso : mips
private static vdsoVersionKey vdsoLinuxVersion = new vdsoVersionKey("LINUX_2.6",0x3ae75f6);

// The symbol name is not __kernel_clock_gettime as suggested by the manpage;
// according to Linux source code it should be __vdso_clock_gettime instead.
private static vdsoSymbolKey vdsoSymbolKeys = new slice<vdsoSymbolKey>(new vdsoSymbolKey[] { {"__vdso_clock_gettime",0xd35ec75,0x6e43a318,&vdsoClockgettimeSym} });

// initialize to fall back to syscall
private static System.UIntPtr vdsoClockgettimeSym = 0;

} // end runtime_package
