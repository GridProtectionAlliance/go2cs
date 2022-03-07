// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Recreate a getsystemcfg syscall handler instead of
// using the one provided by x/sys/unix to avoid having
// the dependency between them. (See golang.org/issue/32102)
// Morever, this file will be used during the building of
// gccgo's libgo and thus must not used a CGo method.

//go:build aix && gccgo
// +build aix,gccgo

// package cpu -- go2cs converted at 2022 March 06 23:38:21 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\sys\cpu\syscall_aix_gccgo.go
using syscall = go.syscall_package;

namespace go.vendor.golang.org.x.sys;

public static partial class cpu_package {

    //extern getsystemcfg
private static ulong gccgoGetsystemcfg(uint label);

private static (System.UIntPtr, syscall.Errno) callgetsystemcfg(nint label) {
    System.UIntPtr r1 = default;
    syscall.Errno e1 = default;

    r1 = uintptr(gccgoGetsystemcfg(uint32(label)));
    e1 = syscall.GetErrno();
    return ;
}

} // end cpu_package
