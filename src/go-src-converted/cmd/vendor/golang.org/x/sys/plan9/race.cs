// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build plan9,race

// package plan9 -- go2cs converted at 2022 March 13 06:41:15 UTC
// import "cmd/vendor/golang.org/x/sys/plan9" ==> using plan9 = go.cmd.vendor.golang.org.x.sys.plan9_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\plan9\race.go
namespace go.cmd.vendor.golang.org.x.sys;

using runtime = runtime_package;
using @unsafe = @unsafe_package;

public static partial class plan9_package {

private static readonly var raceenabled = true;



private static void raceAcquire(unsafe.Pointer addr) {
    runtime.RaceAcquire(addr);
}

private static void raceReleaseMerge(unsafe.Pointer addr) {
    runtime.RaceReleaseMerge(addr);
}

private static void raceReadRange(unsafe.Pointer addr, nint len) {
    runtime.RaceReadRange(addr, len);
}

private static void raceWriteRange(unsafe.Pointer addr, nint len) {
    runtime.RaceWriteRange(addr, len);
}

} // end plan9_package
