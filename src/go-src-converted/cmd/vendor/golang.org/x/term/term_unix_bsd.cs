// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build darwin || dragonfly || freebsd || netbsd || openbsd
// +build darwin dragonfly freebsd netbsd openbsd

// package term -- go2cs converted at 2022 March 13 06:41:33 UTC
// import "cmd/vendor/golang.org/x/term" ==> using term = go.cmd.vendor.golang.org.x.term_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\term\term_unix_bsd.go
namespace go.cmd.vendor.golang.org.x;

using unix = golang.org.x.sys.unix_package;

public static partial class term_package {

private static readonly var ioctlReadTermios = unix.TIOCGETA;

private static readonly var ioctlWriteTermios = unix.TIOCSETA;


} // end term_package
