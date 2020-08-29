// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux,!android netbsd openbsd
// +build cgo

// Package pty is a simple pseudo-terminal package for Unix systems,
// implemented by calling C functions via cgo.
// This is only used for testing the os/signal package.
// package pty -- go2cs converted at 2020 August 29 08:24:53 UTC
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

        private static ref PtyError ptyError(@string name, error err)
        {
            return ref new PtyError(name,err.Error(),err.(syscall.Errno));
        }

        private static @string Error(this ref PtyError e)
        {
            return fmt.Sprintf("%s: %s", e.FuncName, e.ErrorString);
        }

        // Open returns a master pty and the name of the linked slave tty.
        public static (ref os.File, @string, error) Open()
        {
            var (m, err) = C.posix_openpt(C.O_RDWR);
            if (err != null)
            {
                return (null, "", ptyError("posix_openpt", err));
            }
            {
                var (_, err) = C.grantpt(m);

                if (err != null)
                {
                    C.close(m);
                    return (null, "", ptyError("grantpt", err));
                }

            }
            {
                (_, err) = C.unlockpt(m);

                if (err != null)
                {
                    C.close(m);
                    return (null, "", ptyError("unlockpt", err));
                }

            }
            slave = C.GoString(C.ptsname(m));
            return (os.NewFile(uintptr(m), "pty-master"), slave, null);
        }
    }
}}}}
