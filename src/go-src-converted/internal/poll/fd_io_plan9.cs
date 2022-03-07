// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package poll -- go2cs converted at 2022 March 06 22:12:59 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Program Files\Go\src\internal\poll\fd_io_plan9.go
using itoa = go.@internal.itoa_package;
using runtime = go.runtime_package;
using sync = go.sync_package;
using syscall = go.syscall_package;
using System;
using System.Threading;


namespace go.@internal;

public static partial class poll_package {

    // asyncIO implements asynchronous cancelable I/O.
    // An asyncIO represents a single asynchronous Read or Write
    // operation. The result is returned on the result channel.
    // The undergoing I/O system call can either complete or be
    // interrupted by a note.
private partial struct asyncIO {
    public channel<result> res; // mu guards the pid field.
    public sync.Mutex mu; // pid holds the process id of
// the process running the IO operation.
    public nint pid;
}

// result is the return value of a Read or Write operation.
private partial struct result {
    public nint n;
    public error err;
}

// newAsyncIO returns a new asyncIO that performs an I/O
// operation by calling fn, which must do one and only one
// interruptible system call.
private static ptr<asyncIO> newAsyncIO(Func<slice<byte>, (nint, error)> fn, slice<byte> b) {
    ptr<asyncIO> aio = addr(new asyncIO(res:make(chanresult,0),));
    aio.mu.Lock();
    go_(() => () => { 
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
        aio.pid = -1;
        runtime_unignoreHangup();
        aio.mu.Unlock();

        aio.res.Send(new result(n,err));

    }());
    return _addr_aio!;

}

// Cancel interrupts the I/O operation, causing
// the Wait function to return.
private static void Cancel(this ptr<asyncIO> _addr_aio) => func((defer, _, _) => {
    ref asyncIO aio = ref _addr_aio.val;

    aio.mu.Lock();
    defer(aio.mu.Unlock());
    if (aio.pid == -1) {
        return ;
    }
    var (f, e) = syscall.Open("/proc/" + itoa.Itoa(aio.pid) + "/note", syscall.O_WRONLY);
    if (e != null) {
        return ;
    }
    syscall.Write(f, (slice<byte>)"hangup");
    syscall.Close(f);

});

// Wait for the I/O operation to complete.
private static (nint, error) Wait(this ptr<asyncIO> _addr_aio) {
    nint _p0 = default;
    error _p0 = default!;
    ref asyncIO aio = ref _addr_aio.val;

    var res = aio.res.Receive();
    return (res.n, error.As(res.err)!);
}

// The following functions, provided by the runtime, are used to
// ignore and unignore the "hangup" signal received by the process.
private static void runtime_ignoreHangup();
private static void runtime_unignoreHangup();

} // end poll_package
