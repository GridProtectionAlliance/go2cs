// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package poll -- go2cs converted at 2020 August 29 08:25:45 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Go\src\internal\poll\sockopt_windows.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class poll_package
    {
        // Setsockopt wraps the setsockopt network call.
        private static error Setsockopt(this ref FD _fd, int level, int optname, ref byte _optval, int optlen) => func(_fd, _optval, (ref FD fd, ref byte optval, Defer defer, Panic _, Recover __) =>
        {
            {
                var err = fd.incref();

                if (err != null)
                {
                    return error.As(err);
                }
            }
            defer(fd.decref());
            return error.As(syscall.Setsockopt(fd.Sysfd, level, optname, optval, optlen));
        });

        // WSAIoctl wraps the WSAIoctl network call.
        private static error WSAIoctl(this ref FD _fd, uint iocc, ref byte _inbuf, uint cbif, ref byte _outbuf, uint cbob, ref uint _cbbr, ref syscall.Overlapped _overlapped, System.UIntPtr completionRoutine) => func(_fd, _inbuf, _outbuf, _cbbr, _overlapped, (ref FD fd, ref byte inbuf, ref byte outbuf, ref uint cbbr, ref syscall.Overlapped overlapped, Defer defer, Panic _, Recover __) =>
        {
            {
                var err = fd.incref();

                if (err != null)
                {
                    return error.As(err);
                }

            }
            defer(fd.decref());
            return error.As(syscall.WSAIoctl(fd.Sysfd, iocc, inbuf, cbif, outbuf, cbob, cbbr, overlapped, completionRoutine));
        });
    }
}}
