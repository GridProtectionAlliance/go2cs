// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build js,wasm

// package poll -- go2cs converted at 2020 October 09 04:51:05 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Go\src\internal\poll\fd_poll_js.go
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

        private static error init(this ptr<pollDesc> _addr_pd, ptr<FD> _addr_fd)
        {
            ref pollDesc pd = ref _addr_pd.val;
            ref FD fd = ref _addr_fd.val;

            pd.fd = fd;

            return error.As(null!)!;
        }

        private static void close(this ptr<pollDesc> _addr_pd)
        {
            ref pollDesc pd = ref _addr_pd.val;

        }

        private static void evict(this ptr<pollDesc> _addr_pd)
        {
            ref pollDesc pd = ref _addr_pd.val;

            pd.closing = true;
            if (pd.fd != null)
            {
                syscall.StopIO(pd.fd.Sysfd);
            }

        }

        private static error prepare(this ptr<pollDesc> _addr_pd, long mode, bool isFile)
        {
            ref pollDesc pd = ref _addr_pd.val;

            if (pd.closing)
            {
                return error.As(errClosing(isFile))!;
            }

            return error.As(null!)!;

        }

        private static error prepareRead(this ptr<pollDesc> _addr_pd, bool isFile)
        {
            ref pollDesc pd = ref _addr_pd.val;

            return error.As(pd.prepare('r', isFile))!;
        }

        private static error prepareWrite(this ptr<pollDesc> _addr_pd, bool isFile)
        {
            ref pollDesc pd = ref _addr_pd.val;

            return error.As(pd.prepare('w', isFile))!;
        }

        private static error wait(this ptr<pollDesc> _addr_pd, long mode, bool isFile)
        {
            ref pollDesc pd = ref _addr_pd.val;

            if (pd.closing)
            {
                return error.As(errClosing(isFile))!;
            }

            if (isFile)
            { // TODO(neelance): wasm: Use callbacks from JS to block until the read/write finished.
                return error.As(null!)!;

            }

            return error.As(ErrDeadlineExceeded)!;

        }

        private static error waitRead(this ptr<pollDesc> _addr_pd, bool isFile)
        {
            ref pollDesc pd = ref _addr_pd.val;

            return error.As(pd.wait('r', isFile))!;
        }

        private static error waitWrite(this ptr<pollDesc> _addr_pd, bool isFile)
        {
            ref pollDesc pd = ref _addr_pd.val;

            return error.As(pd.wait('w', isFile))!;
        }

        private static void waitCanceled(this ptr<pollDesc> _addr_pd, long mode)
        {
            ref pollDesc pd = ref _addr_pd.val;

        }

        private static bool pollable(this ptr<pollDesc> _addr_pd)
        {
            ref pollDesc pd = ref _addr_pd.val;

            return true;
        }

        // SetDeadline sets the read and write deadlines associated with fd.
        private static error SetDeadline(this ptr<FD> _addr_fd, time.Time t)
        {
            ref FD fd = ref _addr_fd.val;

            return error.As(setDeadlineImpl(_addr_fd, t, 'r' + 'w'))!;
        }

        // SetReadDeadline sets the read deadline associated with fd.
        private static error SetReadDeadline(this ptr<FD> _addr_fd, time.Time t)
        {
            ref FD fd = ref _addr_fd.val;

            return error.As(setDeadlineImpl(_addr_fd, t, 'r'))!;
        }

        // SetWriteDeadline sets the write deadline associated with fd.
        private static error SetWriteDeadline(this ptr<FD> _addr_fd, time.Time t)
        {
            ref FD fd = ref _addr_fd.val;

            return error.As(setDeadlineImpl(_addr_fd, t, 'w'))!;
        }

        private static error setDeadlineImpl(ptr<FD> _addr_fd, time.Time t, long mode)
        {
            ref FD fd = ref _addr_fd.val;

            var d = t.UnixNano();
            if (t.IsZero())
            {
                d = 0L;
            }

            {
                var err = fd.incref();

                if (err != null)
                {
                    return error.As(err)!;
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
            return error.As(null!)!;

        }

        // IsPollDescriptor reports whether fd is the descriptor being used by the poller.
        // This is only used for testing.
        public static bool IsPollDescriptor(System.UIntPtr fd)
        {
            return false;
        }
    }
}}
