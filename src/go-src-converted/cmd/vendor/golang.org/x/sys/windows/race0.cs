// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build windows,!race

// package windows -- go2cs converted at 2022 March 13 06:41:28 UTC
// import "cmd/vendor/golang.org/x/sys/windows" ==> using windows = go.cmd.vendor.golang.org.x.sys.windows_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\windows\race0.go
namespace go.cmd.vendor.golang.org.x.sys;

using @unsafe = @unsafe_package;

public static partial class windows_package {

private static readonly var raceenabled = false;



private static void raceAcquire(unsafe.Pointer addr) {
}

private static void raceReleaseMerge(unsafe.Pointer addr) {
}

private static void raceReadRange(unsafe.Pointer addr, nint len) {
}

private static void raceWriteRange(unsafe.Pointer addr, nint len) {
}

} // end windows_package
