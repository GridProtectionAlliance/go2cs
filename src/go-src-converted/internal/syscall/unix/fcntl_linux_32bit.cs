// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// On 32-bit Linux systems, use SYS_FCNTL64.
// If you change the build tags here, see syscall/flock_linux_32bit.go.

//go:build (linux && 386) || (linux && arm) || (linux && mips) || (linux && mipsle)
// +build linux,386 linux,arm linux,mips linux,mipsle

// package unix -- go2cs converted at 2022 March 13 05:27:49 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Program Files\Go\src\internal\syscall\unix\fcntl_linux_32bit.go
namespace go.@internal.syscall;

using syscall = syscall_package;

public static partial class unix_package {

private static void init() {
    FcntlSyscall = syscall.SYS_FCNTL64;
}

} // end unix_package
