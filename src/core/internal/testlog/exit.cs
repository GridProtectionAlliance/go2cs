// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using sync = sync_package;
using _ = unsafe_package; // for linkname

partial class testlog_package {

// PanicOnExit0 reports whether to panic on a call to os.Exit(0).
// This is in the testlog package because, like other definitions in
// package testlog, it is a hook between the testing package and the
// os package. This is used to ensure that an early call to os.Exit(0)
// does not cause a test to pass.
public static bool PanicOnExit0() => func((defer, _) => {
    panicOnExit0.mu.Lock();
    var panicOnExit0ʗ1 = panicOnExit0;
    defer(panicOnExit0ʗ1.mu.Unlock);
    return panicOnExit0.val;
});

// panicOnExit0 is the flag used for PanicOnExit0. This uses a lock
// because the value can be cleared via a timer call that may race
// with calls to os.Exit

[GoType("dyn")] partial struct panicOnExit0ᴛ1 {
    internal sync_package.Mutex mu;
    internal bool val;
}
internal static panicOnExit0ᴛ1 panicOnExit0;

// SetPanicOnExit0 sets panicOnExit0 to v.
//
// SetPanicOnExit0 should be an internal detail,
// but alternate implementations of go test in other
// build systems may need to access it using linkname.
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname SetPanicOnExit0
public static void SetPanicOnExit0(bool v) => func((defer, _) => {
    panicOnExit0.mu.Lock();
    var panicOnExit0ʗ1 = panicOnExit0;
    defer(panicOnExit0ʗ1.mu.Unlock);
    panicOnExit0.val = v;
});

} // end testlog_package
