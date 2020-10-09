// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 09 05:00:47 UTC
// Original source: C:\Go\src\runtime\testdata\testprog\map.go
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
            register("concurrentMapWrites", concurrentMapWrites);
            register("concurrentMapReadWrite", concurrentMapReadWrite);
            register("concurrentMapIterateWrite", concurrentMapIterateWrite);
        }

        private static void concurrentMapWrites()
        {
            map m = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<long, long>{};
            var c = make_channel<object>();
            go_(() => () =>
            {
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < 10000L; i++)
                    {
                        m[5L] = 0L;
                        runtime.Gosched();
                    }


                    i = i__prev1;
                }
                c.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});

            }());
            go_(() => () =>
            {
                {
                    long i__prev1 = i;

                    for (i = 0L; i < 10000L; i++)
                    {
                        m[6L] = 0L;
                        runtime.Gosched();
                    }


                    i = i__prev1;
                }
                c.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});

            }());
            c.Receive().Send(c);

        }

        private static void concurrentMapReadWrite()
        {
            map m = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<long, long>{};
            var c = make_channel<object>();
            go_(() => () =>
            {
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < 10000L; i++)
                    {
                        m[5L] = 0L;
                        runtime.Gosched();
                    }


                    i = i__prev1;
                }
                c.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});

            }());
            go_(() => () =>
            {
                {
                    long i__prev1 = i;

                    for (i = 0L; i < 10000L; i++)
                    {
                        _ = m[6L];
                        runtime.Gosched();
                    }


                    i = i__prev1;
                }
                c.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});

            }());
            c.Receive().Send(c);

        }

        private static void concurrentMapIterateWrite()
        {
            map m = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<long, long>{};
            var c = make_channel<object>();
            go_(() => () =>
            {
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < 10000L; i++)
                    {
                        m[5L] = 0L;
                        runtime.Gosched();
                    }


                    i = i__prev1;
                }
                c.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});

            }());
            go_(() => () =>
            {
                {
                    long i__prev1 = i;

                    for (i = 0L; i < 10000L; i++)
                    {
                        foreach (>>MARKER:FORRANGEEXPRESSIONS_LEVEL_2<< in m)
                        {>>MARKER:FORRANGEMUTABLEEXPRESSIONS_LEVEL_2<<
                        }
                        runtime.Gosched();

                    }


                    i = i__prev1;
                }
                c.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});

            }());
            c.Receive().Send(c);

        }
    }
}
