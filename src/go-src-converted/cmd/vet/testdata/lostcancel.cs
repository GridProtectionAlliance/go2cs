// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package testdata -- go2cs converted at 2020 August 29 10:10:33 UTC
// import "cmd/vet/testdata" ==> using testdata = go.cmd.vet.testdata_package
// Original source: C:\Go\src\cmd\vet\testdata\lostcancel.go
using context = go.context_package;
using log = go.log_package;
using os = go.os_package;
using testing = go.testing_package;
using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace cmd {
namespace vet
{
    public static partial class testdata_package
    {
        // Check the three functions and assignment forms (var, :=, =) we look for.
        // (Do these early: line numbers are fragile.)
        private static void _()
        {
 // ERROR "the cancel function is not used on all paths \(possible context leak\)"
        } // ERROR "this return statement may be reached without using the cancel var defined on line 17"

        private static void _()
        {
            var (ctx, cancel2) = context.WithDeadline(); // ERROR "the cancel2 function is not used..."
        } // ERROR "may be reached without using the cancel2 var defined on line 21"

        private static void _()
        {
            context.Context ctx = default;
            Action cancel3 = default;
            ctx, cancel3 = context.WithTimeout(); // ERROR "function is not used..."
        } // ERROR "this return statement may be reached without using the cancel3 var defined on line 27"

        private static void _()
        {
            var (ctx, _) = context.WithCancel(); // ERROR "the cancel function returned by context.WithCancel should be called, not discarded, to avoid a context leak"
            ctx, _ = context.WithTimeout(); // ERROR "the cancel function returned by context.WithTimeout should be called, not discarded, to avoid a context leak"
            ctx, _ = context.WithDeadline(); // ERROR "the cancel function returned by context.WithDeadline should be called, not discarded, to avoid a context leak"
        }

        private static void _()
        {
            var (ctx, cancel) = context.WithCancel();
            defer(cancel()); // ok
        }

        private static void _()
        {
            var (ctx, cancel) = context.WithCancel(); // ERROR "not used on all paths"
            if (condition)
            {
                cancel();
            }
            return; // ERROR "this return statement may be reached without using the cancel var"
        }

        private static void _()
        {
            var (ctx, cancel) = context.WithCancel();
            if (condition)
            {
                cancel();
            }
            else
            { 
                // ok: infinite loop
                while (true)
                {
                    print(0L);
                }

            }
        }

        private static void _()
        {
            var (ctx, cancel) = context.WithCancel(); // ERROR "not used on all paths"
            if (condition)
            {
                cancel();
            }
            else
            {
                for (long i = 0L; i < 10L; i++)
                {
                    print(0L);
                }

            }
        } // ERROR "this return statement may be reached without using the cancel var"

        private static void _()
        {
            var (ctx, cancel) = context.WithCancel(); 
            // ok: used on all paths

            if (someInt == 0L)
            {
                @new<testing.T>().FailNow();
                goto __switch_break0;
            }
            if (someInt == 1L)
            {
                log.Fatal();
                goto __switch_break0;
            }
            if (someInt == 2L)
            {
                cancel();
                goto __switch_break0;
            }
            if (someInt == 3L)
            {
                print("hi");
            }
            // default: 
                os.Exit(1L);

            __switch_break0:;
        }

        private static void _()
        {
            var (ctx, cancel) = context.WithCancel(); // ERROR "not used on all paths"
            switch (someInt)
            {
                case 0L: 
                    @new<testing.T>().FailNow();
                    break;
                case 1L: 
                    log.Fatal();
                    break;
                case 2L: 
                    cancel();
                    break;
                case 3L: 
                    print("hi"); // falls through to implicit return
                    break;
                default: 
                    os.Exit(1L);
                    break;
            }
        } // ERROR "this return statement may be reached without using the cancel var"

        private static void _()
        {
            var (ctx, cancel) = context.WithCancel(); // ERROR "not used on all paths"
            @new<testing.T>().FailNow();
            print("hi"); // falls through to implicit return
            cancel();
            os.Exit(1L);
        } // ERROR "this return statement may be reached without using the cancel var"

        private static void _()
        {
            var (ctx, cancel) = context.WithCancel(); 
            // A blocking select must execute one of its cases.
            panic();
        }

        private static void _()
        {
            go_(() => () =>
            {
                var (ctx, cancel) = context.WithCancel(); // ERROR "not used on all paths"
                print(ctx);
            }()); // ERROR "may be reached without using the cancel var"
        }

        private static bool condition = default;
        private static long someInt = default;

        // Regression test for Go issue 16143.
        private static void _()
        {
            var x = default;
            x.f();
        }

        // Regression test for Go issue 16230.
        private static void _()
        {
            ctx, cancel = context.WithCancel();
            return; // a naked return counts as a load of the named result values
        }

        // Same as above, but for literal function.
        private static Func<(context.Context, Action)> _ = () =>
        {
            ctx, cancel = context.WithCancel();
            return;
        };
    }
}}}
