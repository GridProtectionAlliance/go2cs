// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build amd64 && linux && gc
// +build amd64,linux,gc

// package unix -- go2cs converted at 2022 March 06 23:27:05 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_linux_amd64_gc.go
using syscall = go.syscall_package;

namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

    //go:noescape
private static syscall.Errno gettimeofday(ptr<Timeval> tv);

} // end unix_package
