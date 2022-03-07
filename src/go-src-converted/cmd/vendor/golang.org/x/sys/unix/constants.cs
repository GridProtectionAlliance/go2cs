// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || linux || netbsd || openbsd || solaris || zos
// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris zos

// package unix -- go2cs converted at 2022 March 06 23:26:30 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\constants.go


namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

public static readonly nuint R_OK = 0x4;
public static readonly nuint W_OK = 0x2;
public static readonly nuint X_OK = 0x1;


} // end unix_package
