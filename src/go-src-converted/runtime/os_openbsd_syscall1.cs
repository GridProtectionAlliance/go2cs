// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build openbsd && mips64
// +build openbsd,mips64

// package runtime -- go2cs converted at 2022 March 06 22:10:33 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\os_openbsd_syscall1.go


namespace go;

public static partial class runtime_package {

    //go:noescape
private static int thrsleep(System.UIntPtr ident, int clock_id, ptr<timespec> tsp, System.UIntPtr @lock, ptr<uint> abort);

//go:noescape
private static int thrwakeup(System.UIntPtr ident, int n);

private static void osyield();

//go:nosplit
private static void osyield_no_g() {
    osyield();
}

} // end runtime_package
