// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || linux || netbsd || openbsd || windows || solaris
// +build aix darwin dragonfly freebsd linux netbsd openbsd windows solaris

// package poll -- go2cs converted at 2022 March 13 05:27:51 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Program Files\Go\src\internal\poll\fd_poll_runtime.go
namespace go.@internal;

using errors = errors_package;
using sync = sync_package;
using syscall = syscall_package;
using time = time_package;
using _@unsafe_ = @unsafe_package; // for go:linkname


// runtimeNano returns the current value of the runtime clock in nanoseconds.
//go:linkname runtimeNano runtime.nanotime

public static partial class poll_package {

private static long runtimeNano();

private static void runtime_pollServerInit();
private static (System.UIntPtr, nint) runtime_pollOpen(System.UIntPtr fd);
private static void runtime_pollClose(System.UIntPtr ctx);
private static nint runtime_pollWait(System.UIntPtr ctx, nint mode);
private static nint runtime_pollWaitCanceled(System.UIntPtr ctx, nint mode);
private static nint runtime_pollReset(System.UIntPtr ctx, nint mode);
private static void runtime_pollSetDeadline(System.UIntPtr ctx, long d, nint mode);
private static void runtime_pollUnblock(System.UIntPtr ctx);
private static bool runtime_isPollServerDescriptor(System.UIntPtr fd);

private partial struct pollDesc {
    public System.UIntPtr runtimeCtx;
}

private static sync.Once serverInit = default;

private static error init(this ptr<pollDesc> _addr_pd, ptr<FD> _addr_fd) {
    ref pollDesc pd = ref _addr_pd.val;
    ref FD fd = ref _addr_fd.val;

    serverInit.Do(runtime_pollServerInit);
    var (ctx, errno) = runtime_pollOpen(uintptr(fd.Sysfd));
    if (errno != 0) {>>MARKER:FUNCTION_runtime_isPollServerDescriptor_BLOCK_PREFIX<<
        return error.As(errnoErr(syscall.Errno(errno)))!;
    }
    pd.runtimeCtx = ctx;
    return error.As(null!)!;
}

private static void close(this ptr<pollDesc> _addr_pd) {
    ref pollDesc pd = ref _addr_pd.val;

    if (pd.runtimeCtx == 0) {>>MARKER:FUNCTION_runtime_pollUnblock_BLOCK_PREFIX<<
        return ;
    }
    runtime_pollClose(pd.runtimeCtx);
    pd.runtimeCtx = 0;
}

// Evict evicts fd from the pending list, unblocking any I/O running on fd.
private static void evict(this ptr<pollDesc> _addr_pd) {
    ref pollDesc pd = ref _addr_pd.val;

    if (pd.runtimeCtx == 0) {>>MARKER:FUNCTION_runtime_pollSetDeadline_BLOCK_PREFIX<<
        return ;
    }
    runtime_pollUnblock(pd.runtimeCtx);
}

private static error prepare(this ptr<pollDesc> _addr_pd, nint mode, bool isFile) {
    ref pollDesc pd = ref _addr_pd.val;

    if (pd.runtimeCtx == 0) {>>MARKER:FUNCTION_runtime_pollReset_BLOCK_PREFIX<<
        return error.As(null!)!;
    }
    var res = runtime_pollReset(pd.runtimeCtx, mode);
    return error.As(convertErr(res, isFile))!;
}

private static error prepareRead(this ptr<pollDesc> _addr_pd, bool isFile) {
    ref pollDesc pd = ref _addr_pd.val;

    return error.As(pd.prepare('r', isFile))!;
}

private static error prepareWrite(this ptr<pollDesc> _addr_pd, bool isFile) {
    ref pollDesc pd = ref _addr_pd.val;

    return error.As(pd.prepare('w', isFile))!;
}

private static error wait(this ptr<pollDesc> _addr_pd, nint mode, bool isFile) {
    ref pollDesc pd = ref _addr_pd.val;

    if (pd.runtimeCtx == 0) {>>MARKER:FUNCTION_runtime_pollWaitCanceled_BLOCK_PREFIX<<
        return error.As(errors.New("waiting for unsupported file type"))!;
    }
    var res = runtime_pollWait(pd.runtimeCtx, mode);
    return error.As(convertErr(res, isFile))!;
}

private static error waitRead(this ptr<pollDesc> _addr_pd, bool isFile) {
    ref pollDesc pd = ref _addr_pd.val;

    return error.As(pd.wait('r', isFile))!;
}

private static error waitWrite(this ptr<pollDesc> _addr_pd, bool isFile) {
    ref pollDesc pd = ref _addr_pd.val;

    return error.As(pd.wait('w', isFile))!;
}

private static void waitCanceled(this ptr<pollDesc> _addr_pd, nint mode) {
    ref pollDesc pd = ref _addr_pd.val;

    if (pd.runtimeCtx == 0) {>>MARKER:FUNCTION_runtime_pollWait_BLOCK_PREFIX<<
        return ;
    }
    runtime_pollWaitCanceled(pd.runtimeCtx, mode);
}

private static bool pollable(this ptr<pollDesc> _addr_pd) {
    ref pollDesc pd = ref _addr_pd.val;

    return pd.runtimeCtx != 0;
}

// Error values returned by runtime_pollReset and runtime_pollWait.
// These must match the values in runtime/netpoll.go.
private static readonly nint pollNoError = 0;
private static readonly nint pollErrClosing = 1;
private static readonly nint pollErrTimeout = 2;
private static readonly nint pollErrNotPollable = 3;

private static error convertErr(nint res, bool isFile) => func((_, panic, _) => {

    if (res == pollNoError) 
        return error.As(null!)!;
    else if (res == pollErrClosing) 
        return error.As(errClosing(isFile))!;
    else if (res == pollErrTimeout) 
        return error.As(ErrDeadlineExceeded)!;
    else if (res == pollErrNotPollable) 
        return error.As(ErrNotPollable)!;
        println("unreachable: ", res);
    panic("unreachable");
});

// SetDeadline sets the read and write deadlines associated with fd.
private static error SetDeadline(this ptr<FD> _addr_fd, time.Time t) {
    ref FD fd = ref _addr_fd.val;

    return error.As(setDeadlineImpl(_addr_fd, t, 'r' + 'w'))!;
}

// SetReadDeadline sets the read deadline associated with fd.
private static error SetReadDeadline(this ptr<FD> _addr_fd, time.Time t) {
    ref FD fd = ref _addr_fd.val;

    return error.As(setDeadlineImpl(_addr_fd, t, 'r'))!;
}

// SetWriteDeadline sets the write deadline associated with fd.
private static error SetWriteDeadline(this ptr<FD> _addr_fd, time.Time t) {
    ref FD fd = ref _addr_fd.val;

    return error.As(setDeadlineImpl(_addr_fd, t, 'w'))!;
}

private static error setDeadlineImpl(ptr<FD> _addr_fd, time.Time t, nint mode) => func((defer, _, _) => {
    ref FD fd = ref _addr_fd.val;

    long d = default;
    if (!t.IsZero()) {>>MARKER:FUNCTION_runtime_pollClose_BLOCK_PREFIX<<
        d = int64(time.Until(t));
        if (d == 0) {>>MARKER:FUNCTION_runtime_pollOpen_BLOCK_PREFIX<<
            d = -1; // don't confuse deadline right now with no deadline
        }
    }
    {
        var err = fd.incref();

        if (err != null) {>>MARKER:FUNCTION_runtime_pollServerInit_BLOCK_PREFIX<<
            return error.As(err)!;
        }
    }
    defer(fd.decref());
    if (fd.pd.runtimeCtx == 0) {>>MARKER:FUNCTION_runtimeNano_BLOCK_PREFIX<<
        return error.As(ErrNoDeadline)!;
    }
    runtime_pollSetDeadline(fd.pd.runtimeCtx, d, mode);
    return error.As(null!)!;
});

// IsPollDescriptor reports whether fd is the descriptor being used by the poller.
// This is only used for testing.
public static bool IsPollDescriptor(System.UIntPtr fd) {
    return runtime_isPollServerDescriptor(fd);
}

} // end poll_package
