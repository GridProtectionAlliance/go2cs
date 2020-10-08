// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package testing -- go2cs converted at 2020 October 08 04:36:29 UTC
// import "testing" ==> using testing = go.testing_package
// Original source: C:\Go\src\testing\example.go
using fmt = go.fmt_package;
using os = go.os_package;
using sort = go.sort_package;
using strings = go.strings_package;
using time = go.time_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class testing_package
    {
        public partial struct InternalExample
        {
            public @string Name;
            public Action F;
            public @string Output;
            public bool Unordered;
        }

        // RunExamples is an internal function but exported because it is cross-package;
        // it is part of the implementation of the "go test" command.
        public static bool RunExamples(Func<@string, @string, (bool, error)> matchString, slice<InternalExample> examples)
        {
            bool ok = default;

            _, ok = runExamples(matchString, examples);
            return ok;
        }

        private static (bool, bool) runExamples(Func<@string, @string, (bool, error)> matchString, slice<InternalExample> examples)
        {
            bool ran = default;
            bool ok = default;

            ok = true;

            InternalExample eg = default;

            foreach (var (_, __eg) in examples)
            {
                eg = __eg;
                var (matched, err) = matchString(match.val, eg.Name);
                if (err != null)
                {
                    fmt.Fprintf(os.Stderr, "testing: invalid regexp for -test.run: %s\n", err);
                    os.Exit(1L);
                }

                if (!matched)
                {
                    continue;
                }

                ran = true;
                if (!runExample(eg))
                {
                    ok = false;
                }

            }

            return (ran, ok);

        }

        private static @string sortLines(@string output)
        {
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
        private static bool processRunResult(this ptr<InternalExample> _addr_eg, @string stdout, time.Duration timeSpent, object recovered) => func((_, panic, __) =>
        {
            bool passed = default;
            ref InternalExample eg = ref _addr_eg.val;

            passed = true;

            var dstr = fmtDuration(timeSpent);
            @string fail = default;
            var got = strings.TrimSpace(stdout);
            var want = strings.TrimSpace(eg.Output);
            if (eg.Unordered)
            {
                if (sortLines(got) != sortLines(want) && recovered == null)
                {
                    fail = fmt.Sprintf("got:\n%s\nwant (unordered):\n%s\n", stdout, eg.Output);
                }

            }
            else
            {
                if (got != want && recovered == null)
                {
                    fail = fmt.Sprintf("got:\n%s\nwant:\n%s\n", got, want);
                }

            }

            if (fail != "" || recovered != null)
            {
                fmt.Printf("--- FAIL: %s (%s)\n%s", eg.Name, dstr, fail);
                passed = false;
            }
            else if (chatty.val)
            {
                fmt.Printf("--- PASS: %s (%s)\n", eg.Name, dstr);
            }

            if (recovered != null)
            { 
                // Propagate the previously recovered result, by panicking.
                panic(recovered);

            }

            return ;

        });
    }
}
