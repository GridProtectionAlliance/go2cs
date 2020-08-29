// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package testing -- go2cs converted at 2020 August 29 08:47:13 UTC
// import "go/doc.testing" ==> using testing = go.go.doc.testing_package
// Original source: C:\Go\src\go\doc\testdata\example.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using io = go.io_package;
using os = go.os_package;
using strings = go.strings_package;
using time = go.time_package;
using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace go
{
    public static partial class testing_package
    {
        public partial struct InternalExample
        {
            public @string Name;
            public Action F;
            public @string Output;
        }

        public static bool RunExamples(slice<InternalExample> examples) => func((defer, _, __) =>
        {
            ok = true;

            InternalExample eg = default;

            var stdout = os.Stdout;
            var stderr = os.Stderr;
            defer(() =>
            {
                os.Stdout = stdout;
                os.Stderr = stderr;
                {
                    var e__prev1 = e;

                    var e = recover();

                    if (e != null)
                    {
                        fmt.Printf("--- FAIL: %s\npanic: %v\n", eg.Name, e);
                        os.Exit(1L);
                    }

                    e = e__prev1;

                }
            }());

            foreach (var (_, __eg) in examples)
            {
                eg = __eg;
                if (chatty.Value)
                {
                    fmt.Printf("=== RUN: %s\n", eg.Name);
                } 

                // capture stdout and stderr
                var (r, w, err) = os.Pipe();
                if (err != null)
                {
                    fmt.Fprintln(os.Stderr, err);
                    os.Exit(1L);
                }
                os.Stdout = w;
                os.Stderr = w;
                var outC = make_channel<@string>();
                go_(() => () =>
                {
                    ptr<object> buf = @new<bytes.Buffer>();
                    var (_, err) = io.Copy(buf, r);
                    if (err != null)
                    {
                        fmt.Fprintf(stderr, "testing: copying pipe: %v\n", err);
                        os.Exit(1L);
                    }
                    outC.Send(buf.String());
                }()); 

                // run example
                var t0 = time.Now();
                eg.F();
                var dt = time.Now().Sub(t0); 

                // close pipe, restore stdout/stderr, get output
                w.Close();
                os.Stdout = stdout;
                os.Stderr = stderr;
                var @out = outC.Receive(); 

                // report any errors
                var tstr = fmt.Sprintf("(%.2f seconds)", dt.Seconds());
                {
                    var e__prev1 = e;

                    var g = strings.TrimSpace(out);
                    e = strings.TrimSpace(eg.Output);

                    if (g != e)
                    {
                        fmt.Printf("--- FAIL: %s %s\ngot:\n%s\nwant:\n%s\n", eg.Name, tstr, g, e);
                        ok = false;
                    }
                    else if (chatty.Value)
                    {
                        fmt.Printf("--- PASS: %s %s\n", eg.Name, tstr);
                    }

                    e = e__prev1;

                }
            }

            return;
        });
    }
}}
