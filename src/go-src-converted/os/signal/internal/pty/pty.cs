// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd linux,!android netbsd openbsd
// +build cgo

// Package pty is a simple pseudo-terminal package for Unix systems,
// implemented by calling C functions via cgo.
// This is only used for testing the os/signal package.
// package pty -- go2cs converted at 2020 October 09 05:01:00 UTC
// import "os/signal/internal/pty" ==> using pty = go.os.signal.@internal.pty_package
// Original source: C:\Go\src\os\signal\internal\pty\pty.go
/*
#define _XOPEN_SOURCE 600
#include <fcntl.h>
#include <stdlib.h>
#include <unistd.h>
*/
using C = go.C_package;/*
#define _XOPEN_SOURCE 600
#include <fcntl.h>
#include <stdlib.h>
#include <unistd.h>
*/


using fmt = go.fmt_package;
using os = go.os_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace os {
namespace signal {
namespace @internal
{
    public static partial class pty_package
    {
        public partial struct PtyError
        {
            public @string FuncName;
            public @string ErrorString;
            public syscall.Errno Errno;
        }

        private static ptr<PtyError> ptyError(@string name, error err)
        {
            return addr(new PtyError(name,err.Error(),err.(syscall.Errno)));
        }

        private static @string Error(this ptr<PtyError> _addr_e)
        {
            ref PtyError e = ref _addr_e.val;

            return fmt.Sprintf("%s: %s", e.FuncName, e.ErrorString);
        }

        private static error Unwrap(this ptr<PtyError> _addr_e)
        {
            ref PtyError e = ref _addr_e.val;

            return error.As(e.Errno)!;
        }

        // Open returns a control pty and the name of the linked process tty.
        public static (ptr<os.File>, @string, error) Open()
        {
            ptr<os.File> pty = default!;
            @string processTTY = default;
            error err = default!;

            var (m, err) = C.posix_openpt(C.O_RDWR);
            if (err != null)
            {
                return (_addr_null!, "", error.As(ptyError("posix_openpt", err))!);
            }

            {
                var (_, err) = C.grantpt(m);

                if (err != null)
                {
                    C.close(m);
                    return (_addr_null!, "", error.As(ptyError("grantpt", err))!);
                }

            }

            {
                (_, err) = C.unlockpt(m);

                if (err != null)
                {
                    C.close(m);
                    return (_addr_null!, "", error.As(ptyError("unlockpt", err))!);
                }

            }

            processTTY = C.GoString(C.ptsname(m));
            return (_addr_os.NewFile(uintptr(m), "pty")!, processTTY, error.As(null!)!);

        }
    }
}}}}
