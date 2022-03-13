// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build (aix || darwin || dragonfly || freebsd || linux || netbsd || openbsd || solaris || zos) && go1.9
// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris zos
// +build go1.9

// package unix -- go2cs converted at 2022 March 13 06:41:16 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\aliases.go
namespace go.cmd.vendor.golang.org.x.sys;

using syscall = syscall_package;

public static partial class unix_package {

public partial struct Signal { // : syscall.Signal
}
public partial struct Errno { // : syscall.Errno
}
public partial struct SysProcAttr { // : syscall.SysProcAttr
}

} // end unix_package
