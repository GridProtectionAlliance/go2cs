// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package test -- go2cs converted at 2022 March 13 06:29:51 UTC
// import "cmd/go/internal/test" ==> using test = go.cmd.go.@internal.test_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\test\cover.go
namespace go.cmd.go.@internal;

using @base = cmd.go.@internal.@base_package;
using fmt = fmt_package;
using io = io_package;
using os = os_package;
using filepath = path.filepath_package;
using sync = sync_package;

public static partial class test_package {

private static var coverMerge = default;

// initCoverProfile initializes the test coverage profile.
// It must be run before any calls to mergeCoverProfile or closeCoverProfile.
// Using this function clears the profile in case it existed from a previous run,
// or in case it doesn't exist and the test is going to fail to create it (or not run).
private static void initCoverProfile() {
    if (testCoverProfile == "" || testC) {
        return ;
    }
    if (!filepath.IsAbs(testCoverProfile)) {
        testCoverProfile = filepath.Join(testOutputDir.getAbs(), testCoverProfile);
    }
    var (f, err) = os.Create(testCoverProfile);
    if (err != null) {
        @base.Fatalf("%v", err);
    }
    _, err = fmt.Fprintf(f, "mode: %s\n", testCoverMode);
    if (err != null) {
        @base.Fatalf("%v", err);
    }
    coverMerge.f = f;
}

// mergeCoverProfile merges file into the profile stored in testCoverProfile.
// It prints any errors it encounters to ew.
private static void mergeCoverProfile(io.Writer ew, @string file) => func((defer, _, _) => {
    if (coverMerge.f == null) {
        return ;
    }
    coverMerge.Lock();
    defer(coverMerge.Unlock());

    var expect = fmt.Sprintf("mode: %s\n", testCoverMode);
    var buf = make_slice<byte>(len(expect));
    var (r, err) = os.Open(file);
    if (err != null) { 
        // Test did not create profile, which is OK.
        return ;
    }
    defer(r.Close());

    var (n, err) = io.ReadFull(r, buf);
    if (n == 0) {
        return ;
    }
    if (err != null || string(buf) != expect) {
        fmt.Fprintf(ew, "error: test wrote malformed coverage profile.\n");
        return ;
    }
    _, err = io.Copy(coverMerge.f, r);
    if (err != null) {
        fmt.Fprintf(ew, "error: saving coverage profile: %v\n", err);
    }
});

private static void closeCoverProfile() {
    if (coverMerge.f == null) {
        return ;
    }
    {
        var err = coverMerge.f.Close();

        if (err != null) {
            @base.Errorf("closing coverage profile: %v", err);
        }
    }
}

} // end test_package
