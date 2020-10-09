// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build js

// package testing -- go2cs converted at 2020 October 09 05:47:41 UTC
// import "testing" ==> using testing = go.testing_package
// Original source: C:\Go\src\testing\run_example_js.go
using fmt = go.fmt_package;
using io = go.io_package;
using os = go.os_package;
using strings = go.strings_package;
using time = go.time_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class testing_package
    {
        // TODO(@musiol, @odeke-em): unify this code back into
        // example.go when js/wasm gets an os.Pipe implementation.
        private static bool runExample(InternalExample eg) => func((defer, _, __) =>
        {
            bool ok = default;

            if (chatty.val)
            {
                fmt.Printf("=== RUN   %s\n", eg.Name);
            }
            var stdout = os.Stdout;
            var f = createTempFile(eg.Name);
            os.Stdout = f;
            var start = time.Now(); 

            // Clean up in a deferred call so we can recover if the example panics.
            defer(() =>
            {
                var timeSpent = time.Since(start); 

                // Restore stdout, get output and remove temporary file.
                os.Stdout = stdout;
                ref strings.Builder buf = ref heap(out ptr<strings.Builder> _addr_buf);
                var (_, seekErr) = f.Seek(0L, os.SEEK_SET);
                var (_, readErr) = io.Copy(_addr_buf, f);
                var @out = buf.String();
                f.Close();
                os.Remove(f.Name());
                if (seekErr != null)
                {
                    fmt.Fprintf(os.Stderr, "testing: seek temp file: %v\n", seekErr);
                    os.Exit(1L);
                }
                if (readErr != null)
                {
                    fmt.Fprintf(os.Stderr, "testing: read temp file: %v\n", readErr);
                    os.Exit(1L);
                }
                var err = recover();
                ok = eg.processRunResult(out, timeSpent, err);

            }()); 

            // Run example.
            eg.F();
            return ;

        });

        private static ptr<os.File> createTempFile(@string exampleName)
        {
            for (long i = 0L; >>MARKER:FOREXPRESSION_LEVEL_1<<; i++)
            {
                var name = fmt.Sprintf("%s/go-example-stdout-%s-%d.txt", os.TempDir(), exampleName, i);
                var (f, err) = os.OpenFile(name, os.O_RDWR | os.O_CREATE | os.O_EXCL, 0600L);
                if (err != null)
                {
                    if (os.IsExist(err))
                    {
                        continue;
                    }

                    fmt.Fprintf(os.Stderr, "testing: open temp file: %v\n", err);
                    os.Exit(1L);

                }

                return _addr_f!;

            }


        }
    }
}
