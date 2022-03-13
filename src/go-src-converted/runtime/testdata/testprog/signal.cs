// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !windows,!plan9

// package main -- go2cs converted at 2022 March 13 05:29:26 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprog\signal.go
namespace go;

using syscall = syscall_package;
using time = time_package;

public static partial class main_package {

private static void init() {
    register("SignalExitStatus", SignalExitStatus);
}

public static void SignalExitStatus() {
    syscall.Kill(syscall.Getpid(), syscall.SIGTERM); 

    // Should die immediately, but we've seen flakiness on various
    // systems (see issue 14063). It's possible that the signal is
    // being delivered to a different thread and we are returning
    // and exiting before that thread runs again. Give the program
    // a little while to die to make sure we pick up the signal
    // before we return and exit the program. The time here
    // shouldn't matter--we'll never really sleep this long.
    time.Sleep(time.Second);
}

} // end main_package
