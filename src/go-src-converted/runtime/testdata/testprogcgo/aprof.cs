// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 09 05:00:54 UTC
// Original source: C:\Go\src\runtime\testdata\testprogcgo\aprof.go
// Test that SIGPROF received in C code does not crash the process
// looking for the C code's func pointer.

// The test fails when the function is the first C function.
// The exported functions are the first C functions, so we use that.

// extern void GoNop();
using C = go.C_package;// Test that SIGPROF received in C code does not crash the process
// looking for the C code's func pointer.

// The test fails when the function is the first C function.
// The exported functions are the first C functions, so we use that.

// extern void GoNop();


using bytes = go.bytes_package;
using fmt = go.fmt_package;
using pprof = go.runtime.pprof_package;
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
            register("CgoCCodeSIGPROF", CgoCCodeSIGPROF);
        }

        //export GoNop
        public static void GoNop()
        {
        }

        public static void CgoCCodeSIGPROF()
        {
            var c = make_channel<bool>();
            go_(() => () =>
            {
                c.Receive();
                var start = time.Now();
                for (long i = 0L; i < 1e7F; i++)
                {
                    if (i % 1000L == 0L)
                    {
                        if (time.Since(start) > time.Second)
                        {
                            break;
                        }

                    }

                    C.GoNop();

                }

                c.Send(true);

            }());

            ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
            pprof.StartCPUProfile(_addr_buf);
            c.Send(true);
            c.Receive();
            pprof.StopCPUProfile();

            fmt.Println("OK");

        }
    }
}
