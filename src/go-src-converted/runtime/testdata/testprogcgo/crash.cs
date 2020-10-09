// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 09 05:00:55 UTC
// Original source: C:\Go\src\runtime\testdata\testprogcgo\crash.go
using fmt = go.fmt_package;
using runtime = go.runtime_package;
using static go.builtin;
using System;
using System.Threading;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("Crash", Crash);
        }

        private static void test(@string name) => func((defer, _, __) =>
        {
            defer(() =>
            {
                {
                    var x = recover();

                    if (x != null)
                    {
                        fmt.Printf(" recovered");
                    }

                }

                fmt.Printf(" done\n");

            }());
            fmt.Printf("%s:", name);
            ptr<@string> s;
            _ = s.val;
            fmt.Print("SHOULD NOT BE HERE");

        });

        private static void testInNewThread(@string name)
        {
            var c = make_channel<bool>();
            go_(() => () =>
            {
                runtime.LockOSThread();
                test(name);
                c.Send(true);
            }());
            c.Receive();

        }

        public static void Crash()
        {
            runtime.LockOSThread();
            test("main");
            testInNewThread("new-thread");
            testInNewThread("second-new-thread");
            test("main-again");
        }
    }
}
