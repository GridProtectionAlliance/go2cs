// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package testing -- go2cs converted at 2020 August 29 10:05:50 UTC
// import "testing" ==> using testing = go.testing_package
// Original source: C:\Go\src\testing\example.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using io = go.io_package;
using os = go.os_package;
using sort = go.sort_package;
using strings = go.strings_package;
using time = go.time_package;
using static go.builtin;
using System;
using System.Threading;

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

        // An internal function but exported because it is cross-package; part of the implementation
        // of the "go test" command.
        public static bool RunExamples(Func<@string, @string, (bool, error)> matchString, slice<InternalExample> examples)
        {
            _, ok = runExamples(matchString, examples);
            return ok;
        }

        private static (bool, bool) runExamples(Func<@string, @string, (bool, error)> matchString, slice<InternalExample> examples)
        {
            ok = true;

            InternalExample eg = default;

            foreach (var (_, __eg) in examples)
            {
                eg = __eg;
                var (matched, err) = matchString(match.Value, eg.Name);
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

        private static bool runExample(InternalExample eg) => func((defer, panic, _) =>
        {
            if (chatty.Value)
            {
                fmt.Printf("=== RUN   %s\n", eg.Name);
            } 

            // Capture stdout.
            var stdout = os.Stdout;
            var (r, w, err) = os.Pipe();
            if (err != null)
            {
                fmt.Fprintln(os.Stderr, err);
                os.Exit(1L);
            }
            os.Stdout = w;
            var outC = make_channel<@string>();
            go_(() => () =>
            {
                bytes.Buffer buf = default;
                var (_, err) = io.Copy(ref buf, r);
                r.Close();
                if (err != null)
                {
                    fmt.Fprintf(os.Stderr, "testing: copying pipe: %v\n", err);
                    os.Exit(1L);
                }
                outC.Send(buf.String());
            }());

            var start = time.Now();
            ok = true; 

            // Clean up in a deferred call so we can recover if the example panics.
            defer(() =>
            {
                var dstr = fmtDuration(time.Since(start)); 

                // Close pipe, restore stdout, get output.
                w.Close();
                os.Stdout = stdout;
                var @out = outC.Receive();

                @string fail = default;
                var err = recover();
                var got = strings.TrimSpace(out);
                var want = strings.TrimSpace(eg.Output);
                if (eg.Unordered)
                {
                    if (sortLines(got) != sortLines(want) && err == null)
                    {
                        fail = fmt.Sprintf("got:\n%s\nwant (unordered):\n%s\n", out, eg.Output);
                    }
                }
                else
                {
                    if (got != want && err == null)
                    {
                        fail = fmt.Sprintf("got:\n%s\nwant:\n%s\n", got, want);
                    }
                }
                if (fail != "" || err != null)
                {
                    fmt.Printf("--- FAIL: %s (%s)\n%s", eg.Name, dstr, fail);
                    ok = false;
                }
                else if (chatty.Value)
                {
                    fmt.Printf("--- PASS: %s (%s)\n", eg.Name, dstr);
                }
                if (err != null)
                {
                    panic(err);
                }
            }()); 

            // Run example.
            eg.F();
            return;
        });
    }
}
