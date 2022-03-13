// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || (darwin && !race) || (linux && !race) || (freebsd && !race) || netbsd || openbsd || solaris || dragonfly || zos
// +build aix darwin,!race linux,!race freebsd,!race netbsd openbsd solaris dragonfly zos

// package unix -- go2cs converted at 2022 March 13 06:41:19 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\race0.go
namespace go.cmd.vendor.golang.org.x.sys;

using @unsafe = @unsafe_package;

public static partial class unix_package {

private static readonly var raceenabled = false;



private static void raceAcquire(unsafe.Pointer addr) {
}

private static void raceReleaseMerge(unsafe.Pointer addr) {
}

private static void raceReadRange(unsafe.Pointer addr, nint len) {
}

private static void raceWriteRange(unsafe.Pointer addr, nint len) {
}

} // end unix_package
