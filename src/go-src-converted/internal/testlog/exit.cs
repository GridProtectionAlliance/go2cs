// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package testlog -- go2cs converted at 2022 March 06 22:12:49 UTC
// import "internal/testlog" ==> using testlog = go.@internal.testlog_package
// Original source: C:\Program Files\Go\src\internal\testlog\exit.go
using sync = go.sync_package;

namespace go.@internal;

public static partial class testlog_package {

    // PanicOnExit0 reports whether to panic on a call to os.Exit(0).
    // This is in the testlog package because, like other definitions in
    // package testlog, it is a hook between the testing package and the
    // os package. This is used to ensure that an early call to os.Exit(0)
    // does not cause a test to pass.
public static bool PanicOnExit0() => func((defer, _, _) => {
    panicOnExit0.mu.Lock();
    defer(panicOnExit0.mu.Unlock());
    return panicOnExit0.val;
});

// panicOnExit0 is the flag used for PanicOnExit0. This uses a lock
// because the value can be cleared via a timer call that may race
// with calls to os.Exit
private static var panicOnExit0 = default;

// SetPanicOnExit0 sets panicOnExit0 to v.
public static void SetPanicOnExit0(bool v) => func((defer, _, _) => {
    panicOnExit0.mu.Lock();
    defer(panicOnExit0.mu.Unlock());
    panicOnExit0.val = v;
});

} // end testlog_package
