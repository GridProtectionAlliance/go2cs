// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Support for test coverage with redesigned coverage implementation.
namespace go;

using fmt = fmt_package;
using goexperiment = @internal.goexperiment_package;
using os = os_package;
using _ = unsafe_package; // for linkname
using @internal;

partial class testing_package {

// cover2 variable stores the current coverage mode and a
// tear-down function to be called at the end of the testing run.

[GoType("dyn")] partial struct cover2ᴛ1 {
    internal @string mode;
    internal Func<@string, @string, (string, error)> tearDown;
    internal Func<float64> snapshotcov;
}
internal static cover2ᴛ1 cover2;

// registerCover2 is invoked during "go test -cover" runs.
// It is used to record a 'tear down' function
// (to be called when the test is complete) and the coverage mode.
internal static void registerCover2(@string mode, Func<@string, @string, (string, error)> tearDown, Func<float64> snapcov) {
    if (mode == ""u8) {
        return;
    }
    cover2.mode = mode;
    cover2.tearDown = tearDown;
    cover2.snapshotcov = snapcov;
}

// coverReport2 invokes a callback in _testmain.go that will
// emit coverage data at the point where test execution is complete,
// for "go test -cover" runs.
internal static void coverReport2() {
    if (!goexperiment.CoverageRedesign) {
        throw panic("unexpected");
    }
    {
        var (errmsg, err) = cover2.tearDown(coverProfile.val, gocoverdir.val); if (err != default!) {
            fmt.Fprintf(~os.Stderr, "%s: %v\n"u8, errmsg, err);
            os.Exit(2);
        }
    }
}

// coverage2 returns a rough "coverage percentage so far"
// number to support the testing.Coverage() function.
internal static float64 coverage2() {
    if (cover2.mode == ""u8) {
        return 0.0F;
    }
    return cover2.snapshotcov();
}

} // end testing_package
