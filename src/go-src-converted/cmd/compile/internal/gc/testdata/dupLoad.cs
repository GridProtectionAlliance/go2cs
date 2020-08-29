// run

// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This test makes sure that we don't split a single
// load up into two separate loads.

// package main -- go2cs converted at 2020 August 29 09:58:13 UTC
// Original source: C:\Go\src\cmd\compile\internal\gc\testdata\dupLoad.go
using fmt = go.fmt_package;
using static go.builtin;
using System;
using System.Threading;

namespace go
{
    public static partial class main_package
    {
        //go:noinline
        private static (ushort, ushort) read1(slice<byte> b)
        { 
            // There is only a single read of b[0].  The two
            // returned values must have the same low byte.
            var v = b[0L];
            return (uint16(v), uint16(v) | uint16(b[1L]) << (int)(8L));
        }

        public static readonly long N = 100000L;



        private static void main1() => func((_, panic, __) =>
        {
            var done = make_channel<object>();
            var b = make_slice<byte>(2L);
            go_(() => () =>
            {
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < N; i++)
                    {
                        b[0L] = byte(i);
                        b[1L] = byte(i);
                    }


                    i = i__prev1;
                }
                done.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});
            }());
            go_(() => () =>
            {
                {
                    long i__prev1 = i;

                    for (i = 0L; i < N; i++)
                    {
                        var (x, y) = read1(b);
                        if (byte(x) != byte(y))
                        {
                            fmt.Printf("x=%x y=%x\n", x, y);
                            panic("bad");
                        }
                    }


                    i = i__prev1;
                }
                done.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});
            }());
            done.Receive().Send(done);
        });

        //go:noinline
        private static (ushort, ushort) read2(slice<byte> b)
        { 
            // There is only a single read of b[1].  The two
            // returned values must have the same high byte.
            var v = uint16(b[1L]) << (int)(8L);
            return (v, uint16(b[0L]) | v);
        }

        private static void main2() => func((_, panic, __) =>
        {
            var done = make_channel<object>();
            var b = make_slice<byte>(2L);
            go_(() => () =>
            {
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < N; i++)
                    {
                        b[0L] = byte(i);
                        b[1L] = byte(i);
                    }


                    i = i__prev1;
                }
                done.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});
            }());
            go_(() => () =>
            {
                {
                    long i__prev1 = i;

                    for (i = 0L; i < N; i++)
                    {
                        var (x, y) = read2(b);
                        if (x & 0xff00UL != y & 0xff00UL)
                        {
                            fmt.Printf("x=%x y=%x\n", x, y);
                            panic("bad");
                        }
                    }


                    i = i__prev1;
                }
                done.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});
            }());
            done.Receive().Send(done);
        });

        private static void Main()
        {
            main1();
            main2();
        }
    }
}
