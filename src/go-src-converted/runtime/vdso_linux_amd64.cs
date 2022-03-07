// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:12:29 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\vdso_linux_amd64.go


namespace go;

public static partial class runtime_package {

 
// vdsoArrayMax is the byte-size of a maximally sized array on this architecture.
// See cmd/compile/internal/amd64/galign.go arch.MAXWIDTH initialization.
private static readonly nint vdsoArrayMax = 1 << 50 - 1;


private static vdsoVersionKey vdsoLinuxVersion = new vdsoVersionKey("LINUX_2.6",0x3ae75f6);

private static vdsoSymbolKey vdsoSymbolKeys = new slice<vdsoSymbolKey>(new vdsoSymbolKey[] { {"__vdso_gettimeofday",0x315ca59,0xb01bca00,&vdsoGettimeofdaySym}, {"__vdso_clock_gettime",0xd35ec75,0x6e43a318,&vdsoClockgettimeSym} });

private static System.UIntPtr vdsoGettimeofdaySym = default;private static System.UIntPtr vdsoClockgettimeSym = default;

} // end runtime_package
