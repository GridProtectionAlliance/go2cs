// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux netbsd openbsd

// This is in testprognet instead of testprog because testprog
// must not import anything (like net, but also like os/signal)
// that kicks off background goroutines during init.

// package main -- go2cs converted at 2020 August 29 08:36:41 UTC
// Original source: C:\Go\src\runtime\testdata\testprognet\signalexec.go
using fmt = go.fmt_package;
using os = go.os_package;
using exec = go.os.exec_package;
using signal = go.os.signal_package;
using sync = go.sync_package;
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
            register("SignalDuringExec", SignalDuringExec);
            register("Nop", Nop);
        }

        public static void SignalDuringExec() => func((defer, _, __) =>
        {
            var pgrp = syscall.Getpgrp();

            const long tries = 10L;



            sync.WaitGroup wg = default;
            var c = make_channel<os.Signal>(tries);
            signal.Notify(c, syscall.SIGWINCH);
            wg.Add(1L);
            go_(() => () =>
            {
                defer(wg.Done());
                foreach (>>MARKER:FORRANGEEXPRESSIONS_LEVEL_1<< in c)
                {>>MARKER:FORRANGEMUTABLEEXPRESSIONS_LEVEL_1<<
                }
            }());

            for (long i = 0L; i < tries; i++)
            {
                time.Sleep(time.Microsecond);
                wg.Add(2L);
                go_(() => () =>
                {
                    defer(wg.Done());
                    var cmd = exec.Command(os.Args[0L], "Nop");
                    cmd.Stdout = os.Stdout;
                    cmd.Stderr = os.Stderr;
                    {
                        var err = cmd.Run();

                        if (err != null)
                        {
                            fmt.Printf("Start failed: %v", err);
                        }

                    }
                }());
                go_(() => () =>
                {
                    defer(wg.Done());
                    syscall.Kill(-pgrp, syscall.SIGWINCH);
                }());
            }


            signal.Stop(c);
            close(c);
            wg.Wait();

            fmt.Println("OK");
        });

        public static void Nop()
        { 
            // This is just for SignalDuringExec.
        }
    }
}
