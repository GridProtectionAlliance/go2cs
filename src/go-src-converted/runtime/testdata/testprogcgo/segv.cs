// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !plan9,!windows

// package main -- go2cs converted at 2020 October 09 05:01:03 UTC
// Original source: C:\Go\src\runtime\testdata\testprogcgo\segv.go
// static void nop() {}
using C = go.C_package;// static void nop() {}


using syscall = go.syscall_package;
using time = go.time_package;
using static go.builtin;
using System;
using System.Threading;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("Segv", Segv);
            register("SegvInCgo", SegvInCgo);
        }

        public static long Sum = default;

        public static void Segv()
        {
            var c = make_channel<bool>();
            go_(() => () =>
            {
                close(c);
                for (long i = 0L; >>MARKER:FOREXPRESSION_LEVEL_1<<; i++)
                {
                    Sum += i;
                }


            }());

            c.Receive();

            syscall.Kill(syscall.Getpid(), syscall.SIGSEGV); 

            // Give the OS time to deliver the signal.
            time.Sleep(time.Second);

        }

        public static void SegvInCgo()
        {
            var c = make_channel<bool>();
            go_(() => () =>
            {
                close(c);
                while (true)
                {
                    C.nop();
                }


            }());

            c.Receive();

            syscall.Kill(syscall.Getpid(), syscall.SIGSEGV); 

            // Give the OS time to deliver the signal.
            time.Sleep(time.Second);

        }
    }
}
