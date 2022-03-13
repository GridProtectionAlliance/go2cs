// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || linux || solaris || zos
// +build aix linux solaris zos

// package term -- go2cs converted at 2022 March 13 06:41:33 UTC
// import "cmd/vendor/golang.org/x/term" ==> using term = go.cmd.vendor.golang.org.x.term_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\term\term_unix_other.go
namespace go.cmd.vendor.golang.org.x;

using unix = golang.org.x.sys.unix_package;

public static partial class term_package {

private static readonly var ioctlReadTermios = unix.TCGETS;

private static readonly var ioctlWriteTermios = unix.TCSETS;


} // end term_package
