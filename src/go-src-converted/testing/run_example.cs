// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !js && !wasip1
// TODO(@musiol, @odeke-em): re-unify this entire file back into
// example.go when js/wasm gets an os.Pipe implementation
// and no longer needs this separation.
namespace go;

using fmt = fmt_package;
using Δio = io_package;
using os = os_package;
using strings = strings_package;
using time = time_package;

partial class testing_package {

internal static bool /*ok*/ runExample(InternalExample eg) {
    bool ok = default!;
    func((defer, recover) => {
        if (chatty.on) {
            fmt.Printf("%s=== RUN   %s\n"u8, chatty.prefix(), eg.Name);
        }
        // Capture stdout.
        var stdout = os.Stdout;
        var (r, w, err) = os.Pipe();
        if (err != default!) {
            fmt.Fprintln(new os.FileжWriter(os.Stderr), err);
            os.Exit(1);
        }
        os.Stdout = w;
        var outC = new channel<@string>(1);
        var outCʗ1 = outC;
        var rʗ1 = r;
        goǃ(() => {
            ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
            var (_, errΔ1) = Δio.Copy(new strings_BuilderжWriter(Ꮡbuf), new os_FileжReader(rʗ1));
            rʗ1.Close();
            if (errΔ1 != default!) {
                fmt.Fprintf(new os.FileжWriter(os.Stderr), "testing: copying pipe: %v\n"u8, errΔ1);
                os.Exit(1);
            }
            outCʗ1.ᐸꟷ(buf.String());
        });
        var finished = false;
        ref var start = ref heap<time.Time>(out var Ꮡstart);
        start = time.Now();
        // Clean up in a deferred call so we can recover if the example panics.
        var outCʗ2 = outC;
        var startʗ1 = start;
        var stdoutʗ1 = stdout;
        var wʗ1 = w;
        defer(() => {
            var timeSpent = time.Since(startʗ1);
            // Close pipe, restore stdout, get output.
            wʗ1.Close();
            os.Stdout = stdoutʗ1;
            @string @out = ᐸꟷ(outCʗ2);
            var errΔ2 = recover();
            ok = eg.processRunResult(@out, timeSpent, finished, errΔ2);
        });
        // Run example.
        eg.F();
        finished = true;
    });
    return ok;
}

} // end testing_package
