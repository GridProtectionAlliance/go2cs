// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:11:59 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\stubs_386.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

private static uint float64touint32(double a);
private static double uint32tofloat64(uint a);

// stackcheck checks that SP is in range [g->stack.lo, g->stack.hi).
private static void stackcheck();

// Called from assembly only; declared for go vet.
private static void setldt(System.UIntPtr slot, unsafe.Pointer @base, System.UIntPtr size);
private static void emptyfunc();

//go:noescape
private static void asmcgocall_no_g(unsafe.Pointer fn, unsafe.Pointer arg);

} // end runtime_package
