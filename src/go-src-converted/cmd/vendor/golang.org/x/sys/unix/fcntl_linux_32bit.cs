// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build (linux && 386) || (linux && arm) || (linux && mips) || (linux && mipsle) || (linux && ppc)
// +build linux,386 linux,arm linux,mips linux,mipsle linux,ppc

// package unix -- go2cs converted at 2022 March 13 06:41:18 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\fcntl_linux_32bit.go
namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

private static void init() { 
    // On 32-bit Linux systems, the fcntl syscall that matches Go's
    // Flock_t type is SYS_FCNTL64, not SYS_FCNTL.
    fcntl64Syscall = SYS_FCNTL64;
}

} // end unix_package
