// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package testing -- go2cs converted at 2022 March 13 06:43:01 UTC
// import "testing" ==> using testing = go.testing_package
// Original source: C:\Program Files\Go\src\testing\example.go
namespace go;

using fmt = fmt_package;
using os = os_package;
using sort = sort_package;
using strings = strings_package;
using time = time_package;
using System;

public static partial class testing_package {

public partial struct InternalExample {
    public @string Name;
    public Action F;
    public @string Output;
    public bool Unordered;
}

// RunExamples is an internal function but exported because it is cross-package;
// it is part of the implementation of the "go test" command.
public static bool RunExamples(Func<@string, @string, (bool, error)> matchString, slice<InternalExample> examples) {
    bool ok = default;

    _, ok = runExamples(matchString, examples);
    return ok;
}

private static (bool, bool) runExamples(Func<@string, @string, (bool, error)> matchString, slice<InternalExample> examples) {
    bool ran = default;
    bool ok = default;

    ok = true;

    InternalExample eg = default;

    foreach (var (_, __eg) in examples) {
        eg = __eg;
        var (matched, err) = matchString(match.val, eg.Name);
        if (err != null) {
            fmt.Fprintf(os.Stderr, "testing: invalid regexp for -test.run: %s\n", err);
            os.Exit(1);
        }
        if (!matched) {
            continue;
        }
        ran = true;
        if (!runExample(eg)) {
            ok = false;
        }
    }
    return (ran, ok);
}

private static @string sortLines(@string output) {
    var lines = strings.Split(output, "\n");
    sort.Strings(lines);
    return strings.Join(lines, "\n");
}

// processRunResult computes a summary and status of the result of running an example test.
// stdout is the captured output from stdout of the test.
// recovered is the result of invoking recover after running the test, in case it panicked.
//
// If stdout doesn't match the expected output or if recovered is non-nil, it'll print the cause of failure to stdout.
// If the test is chatty/verbose, it'll print a success message to stdout.
// If recovered is non-nil, it'll panic with that value.
// If the test panicked with nil, or invoked runtime.Goexit, it'll be
// made to fail and panic with errNilPanicOrGoexit
private static bool processRunResult(this ptr<InternalExample> _addr_eg, @string stdout, time.Duration timeSpent, bool finished, object recovered) => func((_, panic, _) => {
    bool passed = default;
    ref InternalExample eg = ref _addr_eg.val;

    passed = true;
    var dstr = fmtDuration(timeSpent);
    @string fail = default;
    var got = strings.TrimSpace(stdout);
    var want = strings.TrimSpace(eg.Output);
    if (eg.Unordered) {
        if (sortLines(got) != sortLines(want) && recovered == null) {
            fail = fmt.Sprintf("got:\n%s\nwant (unordered):\n%s\n", stdout, eg.Output);
        }
    }
    else
 {
        if (got != want && recovered == null) {
            fail = fmt.Sprintf("got:\n%s\nwant:\n%s\n", got, want);
        }
    }
    if (fail != "" || !finished || recovered != null) {
        fmt.Printf("--- FAIL: %s (%s)\n%s", eg.Name, dstr, fail);
        passed = false;
    }
    else if (chatty.val) {
        fmt.Printf("--- PASS: %s (%s)\n", eg.Name, dstr);
    }
    if (recovered != null) { 
        // Propagate the previously recovered result, by panicking.
        panic(recovered);
    }
    if (!finished && recovered == null) {
        panic(errNilPanicOrGoexit);
    }
    return ;
});

} // end testing_package
