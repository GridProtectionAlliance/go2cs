// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !js

// TODO(@musiol, @odeke-em): re-unify this entire file back into
// example.go when js/wasm gets an os.Pipe implementation
// and no longer needs this separation.

// package testing -- go2cs converted at 2020 October 09 05:47:40 UTC
// import "testing" ==> using testing = go.testing_package
// Original source: C:\Go\src\testing\run_example.go
using fmt = go.fmt_package;
using io = go.io_package;
using os = go.os_package;
using strings = go.strings_package;
using time = go.time_package;
using static go.builtin;
using System;
using System.Threading;

namespace go
{
    public static partial class testing_package
    {
        private static bool runExample(InternalExample eg) => func((defer, _, __) =>
        {
            bool ok = default;

            if (chatty.val)
            {
                fmt.Printf("=== RUN   %s\n", eg.Name);
            }
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
                ref strings.Builder buf = ref heap(out ptr<strings.Builder> _addr_buf);
                var (_, err) = io.Copy(_addr_buf, r);
                r.Close();
                if (err != null)
                {
                    fmt.Fprintf(os.Stderr, "testing: copying pipe: %v\n", err);
                    os.Exit(1L);
                }
                outC.Send(buf.String());

            }());

            var start = time.Now(); 

            // Clean up in a deferred call so we can recover if the example panics.
            defer(() =>
            {
                var timeSpent = time.Since(start); 

                // Close pipe, restore stdout, get output.
                w.Close();
                os.Stdout = stdout;
                var @out = outC.Receive();

                var err = recover();
                ok = eg.processRunResult(out, timeSpent, err);

            }()); 

            // Run example.
            eg.F();
            return ;

        });
    }
}
