// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package poll -- go2cs converted at 2020 October 08 03:32:11 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Go\src\internal\poll\fd_io_plan9.go
using runtime = go.runtime_package;
using sync = go.sync_package;
using syscall = go.syscall_package;
using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace @internal
{
    public static partial class poll_package
    {
        // asyncIO implements asynchronous cancelable I/O.
        // An asyncIO represents a single asynchronous Read or Write
        // operation. The result is returned on the result channel.
        // The undergoing I/O system call can either complete or be
        // interrupted by a note.
        private partial struct asyncIO
        {
            public channel<result> res; // mu guards the pid field.
            public sync.Mutex mu; // pid holds the process id of
// the process running the IO operation.
            public long pid;
        }

        // result is the return value of a Read or Write operation.
        private partial struct result
        {
            public long n;
            public error err;
        }

        // newAsyncIO returns a new asyncIO that performs an I/O
        // operation by calling fn, which must do one and only one
        // interruptible system call.
        private static ptr<asyncIO> newAsyncIO(Func<slice<byte>, (long, error)> fn, slice<byte> b)
        {
            ptr<asyncIO> aio = addr(new asyncIO(res:make(chanresult,0),));
            aio.mu.Lock();
            go_(() => () =>
            { 
                // Lock the current goroutine to its process
                // and store the pid in io so that Cancel can
                // interrupt it. We ignore the "hangup" signal,
                // so the signal does not take down the entire
                // Go runtime.
                runtime.LockOSThread();
                runtime_ignoreHangup();
                aio.pid = syscall.Getpid();
                aio.mu.Unlock();

                var (n, err) = fn(b);

                aio.mu.Lock();
                aio.pid = -1L;
                runtime_unignoreHangup();
                aio.mu.Unlock();

                aio.res.Send(new result(n,err));

            }());
            return _addr_aio!;

        }

        // Cancel interrupts the I/O operation, causing
        // the Wait function to return.
        private static void Cancel(this ptr<asyncIO> _addr_aio) => func((defer, _, __) =>
        {
            ref asyncIO aio = ref _addr_aio.val;

            aio.mu.Lock();
            defer(aio.mu.Unlock());
            if (aio.pid == -1L)
            {
                return ;
            }

            var (f, e) = syscall.Open("/proc/" + itoa(aio.pid) + "/note", syscall.O_WRONLY);
            if (e != null)
            {
                return ;
            }

            syscall.Write(f, (slice<byte>)"hangup");
            syscall.Close(f);

        });

        // Wait for the I/O operation to complete.
        private static (long, error) Wait(this ptr<asyncIO> _addr_aio)
        {
            long _p0 = default;
            error _p0 = default!;
            ref asyncIO aio = ref _addr_aio.val;

            var res = aio.res.Receive();
            return (res.n, error.As(res.err)!);
        }

        // The following functions, provided by the runtime, are used to
        // ignore and unignore the "hangup" signal received by the process.
        private static void runtime_ignoreHangup()
;
        private static void runtime_unignoreHangup()
;
    }
}}
