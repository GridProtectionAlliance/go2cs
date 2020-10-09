// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build linux

// package main -- go2cs converted at 2020 October 09 05:44:44 UTC
// Original source: C:\Go\src\cmd\dist\test_linux.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static readonly var ioctlReadTermios = syscall.TCGETS;

        // isTerminal reports whether fd is a terminal.


        // isTerminal reports whether fd is a terminal.
        private static bool isTerminal(System.UIntPtr fd)
        {
            ref syscall.Termios termios = ref heap(out ptr<syscall.Termios> _addr_termios);
            var (_, _, err) = syscall.Syscall6(syscall.SYS_IOCTL, fd, ioctlReadTermios, uintptr(@unsafe.Pointer(_addr_termios)), 0L, 0L, 0L);
            return err == 0L;
        }

        private static void init()
        {
            stdOutErrAreTerminals = () =>
            {
                return isTerminal(1L) && isTerminal(2L);
            }
;

        }
    }
}
