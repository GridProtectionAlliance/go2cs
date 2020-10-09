// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris

// package unix -- go2cs converted at 2020 October 09 05:56:17 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\ioctl.go
using runtime = go.runtime_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class unix_package
    {
        // ioctl itself should not be exposed directly, but additional get/set
        // functions for specific types are permissible.

        // IoctlSetInt performs an ioctl operation which sets an integer value
        // on fd, using the specified request number.
        public static error IoctlSetInt(long fd, ulong req, long value)
        {
            return error.As(ioctl(fd, req, uintptr(value)))!;
        }

        // IoctlSetWinsize performs an ioctl on fd with a *Winsize argument.
        //
        // To change fd's window size, the req argument should be TIOCSWINSZ.
        public static error IoctlSetWinsize(long fd, ulong req, ptr<Winsize> _addr_value)
        {
            ref Winsize value = ref _addr_value.val;
 
            // TODO: if we get the chance, remove the req parameter and
            // hardcode TIOCSWINSZ.
            var err = ioctl(fd, req, uintptr(@unsafe.Pointer(value)));
            runtime.KeepAlive(value);
            return error.As(err)!;

        }

        // IoctlSetTermios performs an ioctl on fd with a *Termios.
        //
        // The req value will usually be TCSETA or TIOCSETA.
        public static error IoctlSetTermios(long fd, ulong req, ptr<Termios> _addr_value)
        {
            ref Termios value = ref _addr_value.val;
 
            // TODO: if we get the chance, remove the req parameter.
            var err = ioctl(fd, req, uintptr(@unsafe.Pointer(value)));
            runtime.KeepAlive(value);
            return error.As(err)!;

        }

        // IoctlGetInt performs an ioctl operation which gets an integer value
        // from fd, using the specified request number.
        //
        // A few ioctl requests use the return value as an output parameter;
        // for those, IoctlRetInt should be used instead of this function.
        public static (long, error) IoctlGetInt(long fd, ulong req)
        {
            long _p0 = default;
            error _p0 = default!;

            ref long value = ref heap(out ptr<long> _addr_value);
            var err = ioctl(fd, req, uintptr(@unsafe.Pointer(_addr_value)));
            return (value, error.As(err)!);
        }

        public static (ptr<Winsize>, error) IoctlGetWinsize(long fd, ulong req)
        {
            ptr<Winsize> _p0 = default!;
            error _p0 = default!;

            ref Winsize value = ref heap(out ptr<Winsize> _addr_value);
            var err = ioctl(fd, req, uintptr(@unsafe.Pointer(_addr_value)));
            return (_addr__addr_value!, error.As(err)!);
        }

        public static (ptr<Termios>, error) IoctlGetTermios(long fd, ulong req)
        {
            ptr<Termios> _p0 = default!;
            error _p0 = default!;

            ref Termios value = ref heap(out ptr<Termios> _addr_value);
            var err = ioctl(fd, req, uintptr(@unsafe.Pointer(_addr_value)));
            return (_addr__addr_value!, error.As(err)!);
        }
    }
}}}}}}
