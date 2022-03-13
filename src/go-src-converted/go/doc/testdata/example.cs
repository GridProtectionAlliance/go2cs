// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package testing -- go2cs converted at 2022 March 13 05:52:40 UTC
// import "go/doc.testing" ==> using testing = go.go.doc.testing_package
// Original source: C:\Program Files\Go\src\go\doc\testdata\example.go
namespace go.go;

using bytes = bytes_package;
using fmt = fmt_package;
using io = io_package;
using os = os_package;
using strings = strings_package;
using time = time_package;
using System;
using System.Threading;

public static partial class testing_package {

public partial struct InternalExample {
    public @string Name;
    public Action F;
    public @string Output;
}

public static bool RunExamples(slice<InternalExample> examples) => func((defer, _, _) => {
    bool ok = default;

    ok = true;

    InternalExample eg = default;

    var stdout = os.Stdout;
    var stderr = os.Stderr;
    defer(() => {
        (os.Stdout, os.Stderr) = (stdout, stderr);        {
            var e__prev1 = e;

            var e = recover();

            if (e != null) {
                fmt.Printf("--- FAIL: %s\npanic: %v\n", eg.Name, e);
                os.Exit(1);
            }

            e = e__prev1;

        }
    }());

    foreach (var (_, __eg) in examples) {
        eg = __eg;
        if (chatty.val) {
            fmt.Printf("=== RUN: %s\n", eg.Name);
        }
        var (r, w, err) = os.Pipe();
        if (err != null) {
            fmt.Fprintln(os.Stderr, err);
            os.Exit(1);
        }
        (os.Stdout, os.Stderr) = (w, w);        var outC = make_channel<@string>();
        go_(() => () => {
            ptr<object> buf = @new<bytes.Buffer>();
            var (_, err) = io.Copy(buf, r);
            if (err != null) {
                fmt.Fprintf(stderr, "testing: copying pipe: %v\n", err);
                os.Exit(1);
            }
            outC.Send(buf.String());
        }()); 

        // run example
        var t0 = time.Now();
        eg.F();
        var dt = time.Now().Sub(t0); 

        // close pipe, restore stdout/stderr, get output
        w.Close();
        (os.Stdout, os.Stderr) = (stdout, stderr);        var @out = outC.Receive(); 

        // report any errors
        var tstr = fmt.Sprintf("(%.2f seconds)", dt.Seconds());
        {
            var e__prev1 = e;

            var g = strings.TrimSpace(out);
            e = strings.TrimSpace(eg.Output);

            if (g != e) {
                fmt.Printf("--- FAIL: %s %s\ngot:\n%s\nwant:\n%s\n", eg.Name, tstr, g, e);
                ok = false;
            }
            else if (chatty.val) {
                fmt.Printf("--- PASS: %s %s\n", eg.Name, tstr);
            }

            e = e__prev1;

        }
    }
    return ;
});

} // end testing_package
