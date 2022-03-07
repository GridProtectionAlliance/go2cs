// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package poll -- go2cs converted at 2022 March 06 22:13:23 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Program Files\Go\src\internal\poll\splice_linux.go
using unix = go.@internal.syscall.unix_package;
using runtime = go.runtime_package;
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;

namespace go.@internal;

public static partial class poll_package {

 
// spliceNonblock makes calls to splice(2) non-blocking.
private static readonly nuint spliceNonblock = 0x2; 

// maxSpliceSize is the maximum amount of data Splice asks
// the kernel to move in a single call to splice(2).
private static readonly nint maxSpliceSize = 4 << 20;


// Splice transfers at most remain bytes of data from src to dst, using the
// splice system call to minimize copies of data from and to userspace.
//
// Splice gets a pipe buffer from the pool or creates a new one if needed, to serve as a buffer for the data transfer.
// src and dst must both be stream-oriented sockets.
//
// If err != nil, sc is the system call which caused the error.
public static (long, bool, @string, error) Splice(ptr<FD> _addr_dst, ptr<FD> _addr_src, long remain) => func((defer, _, _) => {
    long written = default;
    bool handled = default;
    @string sc = default;
    error err = default!;
    ref FD dst = ref _addr_dst.val;
    ref FD src = ref _addr_src.val;

    var (p, sc, err) = getPipe();
    if (err != null) {
        return (0, false, sc, error.As(err)!);
    }
    defer(putPipe(_addr_p));
    nint inPipe = default;    nint n = default;

    while (err == null && remain > 0) {
        var max = maxSpliceSize;
        if (int64(max) > remain) {
            max = int(remain);
        }
        inPipe, err = spliceDrain(p.wfd, _addr_src, max); 
        // The operation is considered handled if splice returns no
        // error, or an error other than EINVAL. An EINVAL means the
        // kernel does not support splice for the socket type of src.
        // The failed syscall does not consume any data so it is safe
        // to fall back to a generic copy.
        //
        // spliceDrain should never return EAGAIN, so if err != nil,
        // Splice cannot continue.
        //
        // If inPipe == 0 && err == nil, src is at EOF, and the
        // transfer is complete.
        handled = handled || (err != syscall.EINVAL);
        if (err != null || inPipe == 0) {
            break;
        }
        p.data += inPipe;

        n, err = splicePump(_addr_dst, p.rfd, inPipe);
        if (n > 0) {
            written += int64(n);
            remain -= int64(n);
            p.data -= n;
        }
    }
    if (err != null) {
        return (written, handled, "splice", error.As(err)!);
    }
    return (written, true, "", error.As(null!)!);

});

// spliceDrain moves data from a socket to a pipe.
//
// Invariant: when entering spliceDrain, the pipe is empty. It is either in its
// initial state, or splicePump has emptied it previously.
//
// Given this, spliceDrain can reasonably assume that the pipe is ready for
// writing, so if splice returns EAGAIN, it must be because the socket is not
// ready for reading.
//
// If spliceDrain returns (0, nil), src is at EOF.
private static (nint, error) spliceDrain(nint pipefd, ptr<FD> _addr_sock, nint max) => func((defer, _, _) => {
    nint _p0 = default;
    error _p0 = default!;
    ref FD sock = ref _addr_sock.val;

    {
        var err__prev1 = err;

        var err = sock.readLock();

        if (err != null) {
            return (0, error.As(err)!);
        }
        err = err__prev1;

    }

    defer(sock.readUnlock());
    {
        var err__prev1 = err;

        err = sock.pd.prepareRead(sock.isFile);

        if (err != null) {
            return (0, error.As(err)!);
        }
        err = err__prev1;

    }

    while (true) {
        var (n, err) = splice(pipefd, sock.Sysfd, max, spliceNonblock);
        if (err == syscall.EINTR) {
            continue;
        }
        if (err != syscall.EAGAIN) {
            return (n, error.As(err)!);
        }
        {
            var err__prev1 = err;

            err = sock.pd.waitRead(sock.isFile);

            if (err != null) {
                return (n, error.As(err)!);
            }

            err = err__prev1;

        }

    }

});

// splicePump moves all the buffered data from a pipe to a socket.
//
// Invariant: when entering splicePump, there are exactly inPipe
// bytes of data in the pipe, from a previous call to spliceDrain.
//
// By analogy to the condition from spliceDrain, splicePump
// only needs to poll the socket for readiness, if splice returns
// EAGAIN.
//
// If splicePump cannot move all the data in a single call to
// splice(2), it loops over the buffered data until it has written
// all of it to the socket. This behavior is similar to the Write
// step of an io.Copy in userspace.
private static (nint, error) splicePump(ptr<FD> _addr_sock, nint pipefd, nint inPipe) => func((defer, _, _) => {
    nint _p0 = default;
    error _p0 = default!;
    ref FD sock = ref _addr_sock.val;

    {
        var err__prev1 = err;

        var err = sock.writeLock();

        if (err != null) {
            return (0, error.As(err)!);
        }
        err = err__prev1;

    }

    defer(sock.writeUnlock());
    {
        var err__prev1 = err;

        err = sock.pd.prepareWrite(sock.isFile);

        if (err != null) {
            return (0, error.As(err)!);
        }
        err = err__prev1;

    }

    nint written = 0;
    while (inPipe > 0) {
        var (n, err) = splice(sock.Sysfd, pipefd, inPipe, spliceNonblock); 
        // Here, the condition n == 0 && err == nil should never be
        // observed, since Splice controls the write side of the pipe.
        if (n > 0) {
            inPipe -= n;
            written += n;
            continue;
        }
        if (err != syscall.EAGAIN) {
            return (written, error.As(err)!);
        }
        {
            var err__prev1 = err;

            err = sock.pd.waitWrite(sock.isFile);

            if (err != null) {
                return (written, error.As(err)!);
            }

            err = err__prev1;

        }

    }
    return (written, error.As(null!)!);

});

// splice wraps the splice system call. Since the current implementation
// only uses splice on sockets and pipes, the offset arguments are unused.
// splice returns int instead of int64, because callers never ask it to
// move more data in a single call than can fit in an int32.
private static (nint, error) splice(nint @out, nint @in, nint max, nint flags) {
    nint _p0 = default;
    error _p0 = default!;

    var (n, err) = syscall.Splice(in, null, out, null, max, flags);
    return (int(n), error.As(err)!);
}

private partial struct splicePipe {
    public nint rfd;
    public nint wfd;
    public nint data;
}

// splicePipePool caches pipes to avoid high-frequency construction and destruction of pipe buffers.
// The garbage collector will free all pipes in the sync.Pool periodically, thus we need to set up
// a finalizer for each pipe to close its file descriptors before the actual GC.
private static sync.Pool splicePipePool = new sync.Pool(New:newPoolPipe);

private static void newPoolPipe() { 
    // Discard the error which occurred during the creation of pipe buffer,
    // redirecting the data transmission to the conventional way utilizing read() + write() as a fallback.
    var p = newPipe();
    if (p == null) {
        return null;
    }
    runtime.SetFinalizer(p, destroyPipe);
    return p;

}

// getPipe tries to acquire a pipe buffer from the pool or create a new one with newPipe() if it gets nil from the cache.
//
// Note that it may fail to create a new pipe buffer by newPipe(), in which case getPipe() will return a generic error
// and system call name splice in a string as the indication.
private static (ptr<splicePipe>, @string, error) getPipe() {
    ptr<splicePipe> _p0 = default!;
    @string _p0 = default;
    error _p0 = default!;

    var v = splicePipePool.Get();
    if (v == null) {
        return (_addr_null!, "splice", error.As(syscall.EINVAL)!);
    }
    return (v._<ptr<splicePipe>>(), "", error.As(null!)!);

}

private static void putPipe(ptr<splicePipe> _addr_p) {
    ref splicePipe p = ref _addr_p.val;
 
    // If there is still data left in the pipe,
    // then close and discard it instead of putting it back into the pool.
    if (p.data != 0) {
        runtime.SetFinalizer(p, null);
        destroyPipe(_addr_p);
        return ;
    }
    splicePipePool.Put(p);

}

private static unsafe.Pointer disableSplice = default;

// newPipe sets up a pipe for a splice operation.
private static ptr<splicePipe> newPipe() => func((defer, _, _) => {
    ptr<splicePipe> sp = default!;

    var p = (bool.val)(atomic.LoadPointer(_addr_disableSplice));
    if (p != null && p.val) {
        return _addr_null!;
    }
    array<nint> fds = new array<nint>(2); 
    // pipe2 was added in 2.6.27 and our minimum requirement is 2.6.23, so it
    // might not be implemented. Falling back to pipe is possible, but prior to
    // 2.6.29 splice returns -EAGAIN instead of 0 when the connection is
    // closed.
    const var flags = syscall.O_CLOEXEC | syscall.O_NONBLOCK;

    {
        var err = syscall.Pipe2(fds[..], flags);

        if (err != null) {
            return _addr_null!;
        }
    }


    sp = addr(new splicePipe(rfd:fds[0],wfd:fds[1]));

    if (p == null) {
        p = @new<bool>();
        defer(atomic.StorePointer(_addr_disableSplice, @unsafe.Pointer(p))); 

        // F_GETPIPE_SZ was added in 2.6.35, which does not have the -EAGAIN bug.
        {
            var (_, _, errno) = syscall.Syscall(unix.FcntlSyscall, uintptr(fds[0]), syscall.F_GETPIPE_SZ, 0);

            if (errno != 0) {
                p.val = true;
                destroyPipe(_addr_sp);
                return _addr_null!;
            }

        }

    }
    return ;

});

// destroyPipe destroys a pipe.
private static void destroyPipe(ptr<splicePipe> _addr_p) {
    ref splicePipe p = ref _addr_p.val;

    CloseFunc(p.rfd);
    CloseFunc(p.wfd);
}

} // end poll_package
