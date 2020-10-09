// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !windows,!plan9

// This is in testprognet instead of testprog because testprog
// must not import anything (like net, but also like os/signal)
// that kicks off background goroutines during init.

// package main -- go2cs converted at 2020 October 09 05:01:06 UTC
// Original source: C:\Go\src\runtime\testdata\testprognet\signal.go
using signal = go.os.signal_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("SignalIgnoreSIGTRAP", SignalIgnoreSIGTRAP);
        }

        public static void SignalIgnoreSIGTRAP()
        {
            signal.Ignore(syscall.SIGTRAP);
            syscall.Kill(syscall.Getpid(), syscall.SIGTRAP);
            println("OK");
        }
    }
}
