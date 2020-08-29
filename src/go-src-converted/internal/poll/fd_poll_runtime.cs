// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux netbsd openbsd windows solaris

// package poll -- go2cs converted at 2020 August 29 08:25:22 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Go\src\internal\poll\fd_poll_runtime.go
using errors = go.errors_package;
using sync = go.sync_package;
using syscall = go.syscall_package;
using time = go.time_package;
using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class poll_package
    {
        // runtimeNano returns the current value of the runtime clock in nanoseconds.
        private static long runtimeNano()
;

        private static void runtime_pollServerInit()
;
        private static System.UIntPtr runtime_pollServerDescriptor()
;
        private static (System.UIntPtr, long) runtime_pollOpen(System.UIntPtr fd)
;
        private static void runtime_pollClose(System.UIntPtr ctx)
;
        private static long runtime_pollWait(System.UIntPtr ctx, long mode)
;
        private static long runtime_pollWaitCanceled(System.UIntPtr ctx, long mode)
;
        private static long runtime_pollReset(System.UIntPtr ctx, long mode)
;
        private static void runtime_pollSetDeadline(System.UIntPtr ctx, long d, long mode)
;
        private static void runtime_pollUnblock(System.UIntPtr ctx)
;

        private partial struct pollDesc
        {
            public System.UIntPtr runtimeCtx;
        }

        private static sync.Once serverInit = default;

        private static error init(this ref pollDesc pd, ref FD fd)
        {>>MARKER:FUNCTION_runtime_pollUnblock_BLOCK_PREFIX<<
            serverInit.Do(runtime_pollServerInit);
            var (ctx, errno) = runtime_pollOpen(uintptr(fd.Sysfd));
            if (errno != 0L)
            {>>MARKER:FUNCTION_runtime_pollSetDeadline_BLOCK_PREFIX<<
                if (ctx != 0L)
                {>>MARKER:FUNCTION_runtime_pollReset_BLOCK_PREFIX<<
                    runtime_pollUnblock(ctx);
                    runtime_pollClose(ctx);
                }
                return error.As(syscall.Errno(errno));
            }
            pd.runtimeCtx = ctx;
            return error.As(null);
        }

        private static void close(this ref pollDesc pd)
        {>>MARKER:FUNCTION_runtime_pollWaitCanceled_BLOCK_PREFIX<<
            if (pd.runtimeCtx == 0L)
            {>>MARKER:FUNCTION_runtime_pollWait_BLOCK_PREFIX<<
                return;
            }
            runtime_pollClose(pd.runtimeCtx);
            pd.runtimeCtx = 0L;
        }

        // Evict evicts fd from the pending list, unblocking any I/O running on fd.
        private static void evict(this ref pollDesc pd)
        {>>MARKER:FUNCTION_runtime_pollClose_BLOCK_PREFIX<<
            if (pd.runtimeCtx == 0L)
            {>>MARKER:FUNCTION_runtime_pollOpen_BLOCK_PREFIX<<
                return;
            }
            runtime_pollUnblock(pd.runtimeCtx);
        }

        private static error prepare(this ref pollDesc pd, long mode, bool isFile)
        {>>MARKER:FUNCTION_runtime_pollServerDescriptor_BLOCK_PREFIX<<
            if (pd.runtimeCtx == 0L)
            {>>MARKER:FUNCTION_runtime_pollServerInit_BLOCK_PREFIX<<
                return error.As(null);
            }
            var res = runtime_pollReset(pd.runtimeCtx, mode);
            return error.As(convertErr(res, isFile));
        }

        private static error prepareRead(this ref pollDesc pd, bool isFile)
        {>>MARKER:FUNCTION_runtimeNano_BLOCK_PREFIX<<
            return error.As(pd.prepare('r', isFile));
        }

        private static error prepareWrite(this ref pollDesc pd, bool isFile)
        {
            return error.As(pd.prepare('w', isFile));
        }

        private static error wait(this ref pollDesc pd, long mode, bool isFile)
        {
            if (pd.runtimeCtx == 0L)
            {
                return error.As(errors.New("waiting for unsupported file type"));
            }
            var res = runtime_pollWait(pd.runtimeCtx, mode);
            return error.As(convertErr(res, isFile));
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
            if (pd.runtimeCtx == 0L)
            {
                return;
            }
            runtime_pollWaitCanceled(pd.runtimeCtx, mode);
        }

        private static bool pollable(this ref pollDesc pd)
        {
            return pd.runtimeCtx != 0L;
        }

        private static error convertErr(long res, bool isFile) => func((_, panic, __) =>
        {
            switch (res)
            {
                case 0L: 
                    return error.As(null);
                    break;
                case 1L: 
                    return error.As(errClosing(isFile));
                    break;
                case 2L: 
                    return error.As(ErrTimeout);
                    break;
            }
            println("unreachable: ", res);
            panic("unreachable");
        });

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

        private static error setDeadlineImpl(ref FD _fd, time.Time t, long mode) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            var diff = int64(time.Until(t));
            var d = runtimeNano() + diff;
            if (d <= 0L && diff > 0L)
            { 
                // If the user has a deadline in the future, but the delay calculation
                // overflows, then set the deadline to the maximum possible value.
                d = 1L << (int)(63L) - 1L;
            }
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
            defer(fd.decref());
            if (fd.pd.runtimeCtx == 0L)
            {
                return error.As(ErrNoDeadline);
            }
            runtime_pollSetDeadline(fd.pd.runtimeCtx, d, mode);
            return error.As(null);
        });

        // PollDescriptor returns the descriptor being used by the poller,
        // or ^uintptr(0) if there isn't one. This is only used for testing.
        public static System.UIntPtr PollDescriptor()
        {
            return runtime_pollServerDescriptor();
        }
    }
}}
