// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using slices = slices_package;
using strings = strings_package;
using time = time_package;

partial class testing_package {

[GoType] partial struct InternalExample {
    public @string Name;
    public Action F;
    public @string Output;
    public bool Unordered;
}

// RunExamples is an internal function but exported because it is cross-package;
// it is part of the implementation of the "go test" command.
public static bool /*ok*/ RunExamples(Func<@string, @string, (bool, error)> matchString, slice<InternalExample> examples) {
    bool ok = default!;

    (_, ok) = runExamples(matchString, examples);
    return ok;
}

internal static (bool ran, bool ok) runExamples(Func<@string, @string, (bool, error)> matchString, slice<InternalExample> examples) {
    bool ran = default!;
    bool ok = default!;

    ok = true;
    var m = newMatcher(matchString, match.val, "-test.run"u8, skip.val);
    InternalExample eg = default!;
    foreach (var (_, vᴛ1) in examples) {
        eg = vᴛ1;

        var (_, matched, _) = m.fullName(nil, eg.Name);
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

internal static @string sortLines(@string output) {
    var lines = strings.Split(output, "\n"u8);
    slices.Sort(lines);
    return strings.Join(lines, "\n"u8);
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
[GoRecv] internal static bool /*passed*/ processRunResult(this ref InternalExample eg, @string stdout, time.Duration timeSpent, bool finished, any recovered) {
    bool passed = default!;

    passed = true;
    @string dstr = fmtDuration(timeSpent);
    @string fail = default!;
    @string got = strings.TrimSpace(stdout);
    @string want = strings.TrimSpace(eg.Output);
    if (eg.Unordered){
        if (sortLines(got) != sortLines(want) && recovered == default!) {
            fail = fmt.Sprintf("got:\n%s\nwant (unordered):\n%s\n"u8, stdout, eg.Output);
        }
    } else {
        if (got != want && recovered == default!) {
            fail = fmt.Sprintf("got:\n%s\nwant:\n%s\n"u8, got, want);
        }
    }
    if (fail != ""u8 || !finished || recovered != default!){
        fmt.Printf("%s--- FAIL: %s (%s)\n%s"u8, chatty.prefix(), eg.Name, dstr, fail);
        passed = false;
    } else 
    if (chatty.on) {
        fmt.Printf("%s--- PASS: %s (%s)\n"u8, chatty.prefix(), eg.Name, dstr);
    }
    if (chatty.on && chatty.json) {
        fmt.Printf("%s=== NAME   %s\n"u8, chatty.prefix(), "");
    }
    if (recovered != default!){
        // Propagate the previously recovered result, by panicking.
        throw panic(recovered);
    } else 
    if (!finished) {
        throw panic(errNilPanicOrGoexit);
    }
    return passed;
}

} // end testing_package
