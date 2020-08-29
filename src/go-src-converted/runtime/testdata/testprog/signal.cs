// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !windows,!plan9,!nacl

// package main -- go2cs converted at 2020 August 29 08:24:41 UTC
// Original source: C:\Go\src\runtime\testdata\testprog\signal.go
using syscall = go.syscall_package;
using time = go.time_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("SignalExitStatus", SignalExitStatus);
        }

        public static void SignalExitStatus()
        {
            syscall.Kill(syscall.Getpid(), syscall.SIGTERM); 

            // Should die immediately, but we've seen flakiness on various
            // systems (see issue 14063). It's possible that the signal is
            // being delivered to a different thread and we are returning
            // and exiting before that thread runs again. Give the program
            // a little while to die to make sure we pick up the signal
            // before we return and exit the program. The time here
            // shouldn't matter--we'll never really sleep this long.
            time.Sleep(time.Second);
        }
    }
}
