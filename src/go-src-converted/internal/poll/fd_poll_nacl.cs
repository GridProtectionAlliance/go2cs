// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package poll -- go2cs converted at 2020 August 29 08:25:20 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Go\src\internal\poll\fd_poll_nacl.go
using syscall = go.syscall_package;
using time = go.time_package;
using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class poll_package
    {
        private partial struct pollDesc
        {
            public ptr<FD> fd;
            public bool closing;
        }

        private static error init(this ref pollDesc pd, ref FD fd)
        {
            pd.fd = fd;

            return error.As(null);
        }

        private static void close(this ref pollDesc pd)
        {
        }

        private static void evict(this ref pollDesc pd)
        {
            pd.closing = true;
            if (pd.fd != null)
            {
                syscall.StopIO(pd.fd.Sysfd);
            }
        }

        private static error prepare(this ref pollDesc pd, long mode, bool isFile)
        {
            if (pd.closing)
            {
                return error.As(errClosing(isFile));
            }
            return error.As(null);
        }

        private static error prepareRead(this ref pollDesc pd, bool isFile)
        {
            return error.As(pd.prepare('r', isFile));
        }

        private static error prepareWrite(this ref pollDesc pd, bool isFile)
        {
            return error.As(pd.prepare('w', isFile));
        }

        private static error wait(this ref pollDesc pd, long mode, bool isFile)
        {
            if (pd.closing)
            {
                return error.As(errClosing(isFile));
            }
            return error.As(ErrTimeout);
        }

        private static error waitRead(this ref pollDesc pd, bool isFile)
        {
            return error.As(pd.wait('r', isFile));
        }

        private static error waitWrite(this ref pollDesc pd, bool isFile)
        {
            return error.As(pd.wait('w', isFile));
        }

        private static void waitCanceled(this ref pollDesc pd, long mode)
        {
        }

        private static bool pollable(this ref pollDesc pd)
        {
            return true;
        }

        // SetDeadline sets the read and write deadlines associated with fd.
        private static error SetDeadline(this ref FD fd, time.Time t)
        {
            return error.As(setDeadlineImpl(fd, t, 'r' + 'w'));
        }

        // SetReadDeadline sets the read deadline associated with fd.
        private static error SetReadDeadline(this ref FD fd, time.Time t)
        {
            return error.As(setDeadlineImpl(fd, t, 'r'));
        }

        // SetWriteDeadline sets the write deadline associated with fd.
        private static error SetWriteDeadline(this ref FD fd, time.Time t)
        {
            return error.As(setDeadlineImpl(fd, t, 'w'));
        }

        private static error setDeadlineImpl(ref FD fd, time.Time t, long mode)
        {
            var d = t.UnixNano();
            if (t.IsZero())
            {
                d = 0L;
            }
            {
                var err = fd.incref();

                if (err != null)
                {
                    return error.As(err);
                }

            }
            switch (mode)
            {
                case 'r': 
                    syscall.SetReadDeadline(fd.Sysfd, d);
                    break;
                case 'w': 
                    syscall.SetWriteDeadline(fd.Sysfd, d);
                    break;
                case 'r' + 'w': 
                    syscall.SetReadDeadline(fd.Sysfd, d);
                    syscall.SetWriteDeadline(fd.Sysfd, d);
                    break;
            }
            fd.decref();
            return error.As(null);
        }

        // PollDescriptor returns the descriptor being used by the poller,
        // or ^uintptr(0) if there isn't one. This is only used for testing.
        public static System.UIntPtr PollDescriptor()
        {
            return ~uintptr(0L);
        }
    }
}}
