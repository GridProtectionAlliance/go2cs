// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This program is processed by the cover command, and then testAll is called.
// The test driver in main.go can then compare the coverage statistics with expectation.

// The word LINE is replaced by the line number in this file. When the file is executed,
// the coverage processing has changed the line numbers, so we can't use runtime.Caller.

// package main -- go2cs converted at 2020 October 08 04:32:37 UTC
// Original source: C:\Go\src\cmd\cover\testdata\test.go
using _@unsafe_ = go.@unsafe_package;
using static go.builtin;
using System;
using System.Threading;

namespace go
{
    public static partial class main_package
    { // for go:linkname

        //go:linkname some_name some_name
        private static readonly float anything = (float)1e9F; // Just some unlikely value that means "we got here, don't care how often"

 // Just some unlikely value that means "we got here, don't care how often"

        private static void testAll()
        {
            testSimple();
            testBlockRun();
            testIf();
            testFor();
            testRange();
            testSwitch();
            testTypeSwitch();
            testSelect1();
            testSelect2();
            testPanic();
            testEmptySwitches();
            testFunctionLiteral();
            testGoto();
        }

        // The indexes of the counters in testPanic are known to main.go
        private static readonly long panicIndex = (long)3L;

        // This test appears first because the index of its counters is known to main.go


        // This test appears first because the index of its counters is known to main.go
        private static void testPanic() => func((defer, panic, recover) =>
        {
            defer(() =>
            {
                recover();
            }());
            check(LINE, 1L);
            panic("should not get next line");
            check(LINE, 0L); // this is GoCover.Count[panicIndex]
            // The next counter is in testSimple and it will be non-zero.
            // If the panic above does not trigger a counter, the test will fail
            // because GoCover.Count[panicIndex] will be the one in testSimple.
        });

        private static void testSimple()
        {
            check(LINE, 1L);
        }

        private static void testIf()
        {
            if (true)
            {
                check(LINE, 1L);
            }
            else
            {
                check(LINE, 0L);
            }

            if (false)
            {
                check(LINE, 0L);
            }
            else
            {
                check(LINE, 1L);
            }

            {
                long i__prev1 = i;

                for (long i = 0L; i < 3L; i++)
                {
                    if (checkVal(LINE, 3L, i) <= 2L)
                    {
                        check(LINE, 3L);
                    }

                    if (checkVal(LINE, 3L, i) <= 1L)
                    {
                        check(LINE, 2L);
                    }

                    if (checkVal(LINE, 3L, i) <= 0L)
                    {
                        check(LINE, 1L);
                    }

                }


                i = i__prev1;
            }
            {
                long i__prev1 = i;

                for (i = 0L; i < 3L; i++)
                {
                    if (checkVal(LINE, 3L, i) <= 1L)
                    {
                        check(LINE, 2L);
                    }
                    else
                    {
                        check(LINE, 1L);
                    }

                }


                i = i__prev1;
            }
            {
                long i__prev1 = i;

                for (i = 0L; i < 3L; i++)
                {
                    if (checkVal(LINE, 3L, i) <= 0L)
                    {
                        check(LINE, 1L);
                    }
                    else if (checkVal(LINE, 2L, i) <= 1L)
                    {
                        check(LINE, 1L);
                    }
                    else if (checkVal(LINE, 1L, i) <= 2L)
                    {
                        check(LINE, 1L);
                    }
                    else if (checkVal(LINE, 0L, i) <= 3L)
                    {
                        check(LINE, 0L);
                    }

                }


                i = i__prev1;
            }
            if ((a, b) => a < b(3L, 4L))
            {
                check(LINE, 1L);
            }

        }

        private static void testFor()
        {
            for (long i = 0L; i < 10L; () =>
            {
                i++;

                check(LINE, 10L);
            }())
            {
                check(LINE, 10L);
            }


        }

        private static void testRange()
        {
            foreach (var (_, f) in new slice<Action>(new Action[] { func(){check(LINE,1)} }))
            {
                f();
                check(LINE, 1L);
            }

        }

        private static void testBlockRun()
        {
            check(LINE, 1L);
            {
                check(LINE, 1L);
            }
            {
                check(LINE, 1L);
            }
            check(LINE, 1L);
            {
                check(LINE, 1L);
            }
            {
                check(LINE, 1L);
            }
            check(LINE, 1L);
        }

        private static void testSwitch()
        {
            for (long i = 0L; i < 5L; () =>
            {
                i++;

                check(LINE, 5L);
            }())
            {
                goto label2;
label1:
                goto label1;
label2:
                switch (i)
                {
                    case 0L: 
                        check(LINE, 1L);
                        break;
                    case 1L: 
                        check(LINE, 1L);
                        break;
                    case 2L: 
                        check(LINE, 1L);
                        break;
                    default: 
                        check(LINE, 2L);
                        break;
                }

            }


        }

        private static void testTypeSwitch()
        {

            foreach (var (_, v) in x)
            {
                    () =>
                    {
                        check(LINE, 3L);
                    }();

                switch (v.type())
                {
                    case long _:
                        check(LINE, 1L);
                        break;
                    case double _:
                        check(LINE, 1L);
                        break;
                    case @string _:
                        check(LINE, 1L);
                        break;
                    case System.Numerics.Complex128 _:
                        check(LINE, 0L);
                        break;
                    default:
                    {
                        check(LINE, 0L);
                        break;
                    }
                }

            }

        }

        private static void testSelect1()
        {
            var c = make_channel<long>();
            go_(() => () =>
            {
                for (long i = 0L; i < 1000L; i++)
                {
                    c.Send(i);
                }


            }());
            while (true)
            {
                check(LINE, anything);
                check(LINE, anything);
                check(LINE, 1L);
                return ;
            }


        }

        private static void testSelect2()
        {
            var c1 = make_channel<long>(1000L);
            var c2 = make_channel<long>(1000L);
            for (long i = 0L; i < 1000L; i++)
            {
                c1.Send(i);
                c2.Send(i);
            }

            while (true)
            {
                check(LINE, 1000L);
                check(LINE, 1000L);
                check(LINE, 1L);
                return ;
            }


        }

        // Empty control statements created syntax errors. This function
        // is here just to be sure that those are handled correctly now.
        private static void testEmptySwitches()
        {
            check(LINE, 1L);
            switch (3L)
            {
            }
            check(LINE, 1L);
            {
                long i = ._<long>();

                switch (i)
                {
                }
            }
            check(LINE, 1L);
            var c = make_channel<long>();
            go_(() => () =>
            {
                check(LINE, 1L);
                c.Send(1L);
            }());
            c.Receive();
            check(LINE, 1L);

        }

        private static void testFunctionLiteral() => func((_, panic, __) =>
        {
            Func<Action, error> a = f =>
            {
                f();
                f();
                return null;
            }
;

            Func<Action, bool> b = f =>
            {
                f();
                f();
                return true;
            }
;

            check(LINE, 1L);
            a(() =>
            {
                check(LINE, 2L);
            });

            {
                var err = a(() =>
                {
                    check(LINE, 2L);
                });

                if (err != null)
                {
                }

            }


            switch (b(() =>
            {
                check(LINE, 2L);
            }))
            {
            }

            long x = 2L;

            if (x == () =>
                {
                    check(LINE, 1L);

                    return 1L;
                }()) 
                check(LINE, 0L);
                panic("2=1");
            else if (x == () =>
                {
                    check(LINE, 1L);

                    return 2L;
                }()) 
                check(LINE, 1L);
            else if (x == () =>
                {
                    check(LINE, 0L);

                    return 3L;
                }()) 
                check(LINE, 0L);
                panic("2=3");
            
        });

        private static void testGoto()
        {
            for (long i = 0L; i < 2L; i++)
            {
                if (i == 0L)
                {
                    goto Label;
                }

                check(LINE, 1L);
Label:
                check(LINE, 2L);

            } 
            // Now test that we don't inject empty statements
            // between a label and a loop.
 
            // Now test that we don't inject empty statements
            // between a label and a loop.
loop:
            while (true)
            {
                check(LINE, 1L);
                _breakloop = true;
                break;
            }

        }

        // This comment didn't appear in generated go code.
        private static void haha()
        { 
            // Needed for cover to add counter increment here.
            _ = 42L;

        }

        // Some someFunction.
        //
        //go:nosplit
        private static void someFunction()
        {
        }
    }
}
