// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package poll supports non-blocking I/O on file descriptors with polling.
// This supports I/O operations that block only a goroutine, not a thread.
// This is used by the net and os packages.
// It uses a poller built into the runtime, with support from the
// runtime scheduler.
// package poll -- go2cs converted at 2020 August 29 08:25:16 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Go\src\internal\poll\fd.go
using errors = go.errors_package;
using static go.builtin;
using System;

namespace go {
namespace @internal
{
    public static partial class poll_package
    {
        // ErrNetClosing is returned when a network descriptor is used after
        // it has been closed. Keep this string consistent because of issue
        // #4373: since historically programs have not been able to detect
        // this error, they look for the string.
        public static var ErrNetClosing = errors.New("use of closed network connection");

        // ErrFileClosing is returned when a file descriptor is used after it
        // has been closed.
        public static var ErrFileClosing = errors.New("use of closed file");

        // ErrNoDeadline is returned when a request is made to set a deadline
        // on a file type that does not use the poller.
        public static var ErrNoDeadline = errors.New("file type does not support deadline");

        // Return the appropriate closing error based on isFile.
        private static error errClosing(bool isFile)
        {
            if (isFile)
            {
                return error.As(ErrFileClosing);
            }
            return error.As(ErrNetClosing);
        }

        // ErrTimeout is returned for an expired deadline.
        public static error ErrTimeout = error.As(ref new TimeoutError());

        // TimeoutError is returned for an expired deadline.
        public partial struct TimeoutError
        {
        }

        // Implement the net.Error interface.
        private static @string Error(this ref TimeoutError e)
        {
            return "i/o timeout";
        }
        private static bool Timeout(this ref TimeoutError e)
        {
            return true;
        }
        private static bool Temporary(this ref TimeoutError e)
        {
            return true;
        }

        // consume removes data from a slice of byte slices, for writev.
        private static void consume(ref slice<slice<byte>> v, long n)
        {
            while (len(v.Value) > 0L)
            {
                var ln0 = int64(len((v.Value)[0L]));
                if (ln0 > n)
                {
                    (v.Value)[0L] = (v.Value)[0L][n..];
                    return;
                }
                n -= ln0;
                v.Value = (v.Value)[1L..];
            }

        }

        // TestHookDidWritev is a hook for testing writev.
        public static Action<long> TestHookDidWritev = wrote =>
        {
        };
    }
}}
