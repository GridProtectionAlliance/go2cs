// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || (js && wasm) || linux || netbsd || openbsd || solaris
// +build aix darwin dragonfly freebsd js,wasm linux netbsd openbsd solaris

// package poll -- go2cs converted at 2022 March 13 05:27:52 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Program Files\Go\src\internal\poll\fd_unix.go
namespace go.@internal;

using io = io_package;
using atomic = sync.atomic_package;
using syscall = syscall_package;


// FD is a file descriptor. The net and os packages use this type as a
// field of a larger type representing a network connection or OS file.

using System;
public static partial class poll_package {

public partial struct FD {
    public fdMutex fdmu; // System file descriptor. Immutable until Close.
    public nint Sysfd; // I/O poller.
    public pollDesc pd; // Writev cache.
    public ptr<slice<syscall.Iovec>> iovecs; // Semaphore signaled when file is closed.
    public uint csema; // Non-zero if this file has been set to blocking mode.
    public uint isBlocking; // Whether this is a streaming descriptor, as opposed to a
// packet-based descriptor like a UDP socket. Immutable.
    public bool IsStream; // Whether a zero byte read indicates EOF. This is false for a
// message based socket connection.
    public bool ZeroReadIsEOF; // Whether this is a file rather than a network socket.
    public bool isFile;
}

// Init initializes the FD. The Sysfd field should already be set.
// This can be called multiple times on a single FD.
// The net argument is a network name from the net package (e.g., "tcp"),
// or "file".
// Set pollable to true if fd should be managed by runtime netpoll.
private static error Init(this ptr<FD> _addr_fd, @string net, bool pollable) {
    ref FD fd = ref _addr_fd.val;
 
    // We don't actually care about the various network types.
    if (net == "file") {
        fd.isFile = true;
    }
    if (!pollable) {
        fd.isBlocking = 1;
        return error.As(null!)!;
    }
    var err = fd.pd.init(fd);
    if (err != null) { 
        // If we could not initialize the runtime poller,
        // assume we are using blocking mode.
        fd.isBlocking = 1;
    }
    return error.As(err)!;
}

// Destroy closes the file descriptor. This is called when there are
// no remaining references.
private static error destroy(this ptr<FD> _addr_fd) {
    ref FD fd = ref _addr_fd.val;
 
    // Poller may want to unregister fd in readiness notification mechanism,
    // so this must be executed before CloseFunc.
    fd.pd.close(); 

    // We don't use ignoringEINTR here because POSIX does not define
    // whether the descriptor is closed if close returns EINTR.
    // If the descriptor is indeed closed, using a loop would race
    // with some other goroutine opening a new descriptor.
    // (The Linux kernel guarantees that it is closed on an EINTR error.)
    var err = CloseFunc(fd.Sysfd);

    fd.Sysfd = -1;
    runtime_Semrelease(_addr_fd.csema);
    return error.As(err)!;
}

// Close closes the FD. The underlying file descriptor is closed by the
// destroy method when there are no remaining references.
private static error Close(this ptr<FD> _addr_fd) {
    ref FD fd = ref _addr_fd.val;

    if (!fd.fdmu.increfAndClose()) {
        return error.As(errClosing(fd.isFile))!;
    }
    fd.pd.evict(); 

    // The call to decref will call destroy if there are no other
    // references.
    var err = fd.decref(); 

    // Wait until the descriptor is closed. If this was the only
    // reference, it is already closed. Only wait if the file has
    // not been set to blocking mode, as otherwise any current I/O
    // may be blocking, and that would block the Close.
    // No need for an atomic read of isBlocking, increfAndClose means
    // we have exclusive access to fd.
    if (fd.isBlocking == 0) {
        runtime_Semacquire(_addr_fd.csema);
    }
    return error.As(err)!;
}

// SetBlocking puts the file into blocking mode.
private static error SetBlocking(this ptr<FD> _addr_fd) => func((defer, _, _) => {
    ref FD fd = ref _addr_fd.val;

    {
        var err = fd.incref();

        if (err != null) {
            return error.As(err)!;
        }
    }
    defer(fd.decref()); 
    // Atomic store so that concurrent calls to SetBlocking
    // do not cause a race condition. isBlocking only ever goes
    // from 0 to 1 so there is no real race here.
    atomic.StoreUint32(_addr_fd.isBlocking, 1);
    return error.As(syscall.SetNonblock(fd.Sysfd, false))!;
});

// Darwin and FreeBSD can't read or write 2GB+ files at a time,
// even on 64-bit systems.
// The same is true of socket implementations on many systems.
// See golang.org/issue/7812 and golang.org/issue/16266.
// Use 1GB instead of, say, 2GB-1, to keep subsequent reads aligned.
private static readonly nint maxRW = 1 << 30;

// Read implements io.Reader.


// Read implements io.Reader.
private static (nint, error) Read(this ptr<FD> _addr_fd, slice<byte> p) => func((defer, _, _) => {
    nint _p0 = default;
    error _p0 = default!;
    ref FD fd = ref _addr_fd.val;

    {
        var err__prev1 = err;

        var err = fd.readLock();

        if (err != null) {
            return (0, error.As(err)!);
        }
        err = err__prev1;

    }
    defer(fd.readUnlock());
    if (len(p) == 0) { 
        // If the caller wanted a zero byte read, return immediately
        // without trying (but after acquiring the readLock).
        // Otherwise syscall.Read returns 0, nil which looks like
        // io.EOF.
        // TODO(bradfitz): make it wait for readability? (Issue 15735)
        return (0, error.As(null!)!);
    }
    {
        var err__prev1 = err;

        err = fd.pd.prepareRead(fd.isFile);

        if (err != null) {
            return (0, error.As(err)!);
        }
        err = err__prev1;

    }
    if (fd.IsStream && len(p) > maxRW) {
        p = p[..(int)maxRW];
    }
    while (true) {
        var (n, err) = ignoringEINTRIO(syscall.Read, fd.Sysfd, p);
        if (err != null) {
            n = 0;
            if (err == syscall.EAGAIN && fd.pd.pollable()) {
                err = fd.pd.waitRead(fd.isFile);

                if (err == null) {
                    continue;
                }
            }
        }
        err = fd.eofError(n, err);
        return (n, error.As(err)!);
    }
});

// Pread wraps the pread system call.
private static (nint, error) Pread(this ptr<FD> _addr_fd, slice<byte> p, long off) {
    nint _p0 = default;
    error _p0 = default!;
    ref FD fd = ref _addr_fd.val;
 
    // Call incref, not readLock, because since pread specifies the
    // offset it is independent from other reads.
    // Similarly, using the poller doesn't make sense for pread.
    {
        var err__prev1 = err;

        var err = fd.incref();

        if (err != null) {
            return (0, error.As(err)!);
        }
        err = err__prev1;

    }
    if (fd.IsStream && len(p) > maxRW) {
        p = p[..(int)maxRW];
    }
    nint n = default;    err = default!;
    while (true) {
        n, err = syscall.Pread(fd.Sysfd, p, off);
        if (err != syscall.EINTR) {
            break;
        }
    }
    if (err != null) {
        n = 0;
    }
    fd.decref();
    err = fd.eofError(n, err);
    return (n, error.As(err)!);
}

// ReadFrom wraps the recvfrom network call.
private static (nint, syscall.Sockaddr, error) ReadFrom(this ptr<FD> _addr_fd, slice<byte> p) => func((defer, _, _) => {
    nint _p0 = default;
    syscall.Sockaddr _p0 = default;
    error _p0 = default!;
    ref FD fd = ref _addr_fd.val;

    {
        var err__prev1 = err;

        var err = fd.readLock();

        if (err != null) {
            return (0, null, error.As(err)!);
        }
        err = err__prev1;

    }
    defer(fd.readUnlock());
    {
        var err__prev1 = err;

        err = fd.pd.prepareRead(fd.isFile);

        if (err != null) {
            return (0, null, error.As(err)!);
        }
        err = err__prev1;

    }
    while (true) {
        var (n, sa, err) = syscall.Recvfrom(fd.Sysfd, p, 0);
        if (err != null) {
            if (err == syscall.EINTR) {
                continue;
            }
            n = 0;
            if (err == syscall.EAGAIN && fd.pd.pollable()) {
                err = fd.pd.waitRead(fd.isFile);

                if (err == null) {
                    continue;
                }
            }
        }
        err = fd.eofError(n, err);
        return (n, sa, error.As(err)!);
    }
});

// ReadMsg wraps the recvmsg network call.
private static (nint, nint, nint, syscall.Sockaddr, error) ReadMsg(this ptr<FD> _addr_fd, slice<byte> p, slice<byte> oob, nint flags) => func((defer, _, _) => {
    nint _p0 = default;
    nint _p0 = default;
    nint _p0 = default;
    syscall.Sockaddr _p0 = default;
    error _p0 = default!;
    ref FD fd = ref _addr_fd.val;

    {
        var err__prev1 = err;

        var err = fd.readLock();

        if (err != null) {
            return (0, 0, 0, null, error.As(err)!);
        }
        err = err__prev1;

    }
    defer(fd.readUnlock());
    {
        var err__prev1 = err;

        err = fd.pd.prepareRead(fd.isFile);

        if (err != null) {
            return (0, 0, 0, null, error.As(err)!);
        }
        err = err__prev1;

    }
    while (true) {
        var (n, oobn, sysflags, sa, err) = syscall.Recvmsg(fd.Sysfd, p, oob, flags);
        if (err != null) {
            if (err == syscall.EINTR) {
                continue;
            } 
            // TODO(dfc) should n and oobn be set to 0
            if (err == syscall.EAGAIN && fd.pd.pollable()) {
                err = fd.pd.waitRead(fd.isFile);

                if (err == null) {
                    continue;
                }
            }
        }
        err = fd.eofError(n, err);
        return (n, oobn, sysflags, sa, error.As(err)!);
    }
});

// Write implements io.Writer.
private static (nint, error) Write(this ptr<FD> _addr_fd, slice<byte> p) => func((defer, _, _) => {
    nint _p0 = default;
    error _p0 = default!;
    ref FD fd = ref _addr_fd.val;

    {
        var err__prev1 = err;

        var err = fd.writeLock();

        if (err != null) {
            return (0, error.As(err)!);
        }
        err = err__prev1;

    }
    defer(fd.writeUnlock());
    {
        var err__prev1 = err;

        err = fd.pd.prepareWrite(fd.isFile);

        if (err != null) {
            return (0, error.As(err)!);
        }
        err = err__prev1;

    }
    nint nn = default;
    while (true) {
        var max = len(p);
        if (fd.IsStream && max - nn > maxRW) {
            max = nn + maxRW;
        }
        var (n, err) = ignoringEINTRIO(syscall.Write, fd.Sysfd, p[(int)nn..(int)max]);
        if (n > 0) {
            nn += n;
        }
        if (nn == len(p)) {
            return (nn, error.As(err)!);
        }
        if (err == syscall.EAGAIN && fd.pd.pollable()) {
            err = fd.pd.waitWrite(fd.isFile);

            if (err == null) {
                continue;
            }
        }
        if (err != null) {
            return (nn, error.As(err)!);
        }
        if (n == 0) {
            return (nn, error.As(io.ErrUnexpectedEOF)!);
        }
    }
});

// Pwrite wraps the pwrite system call.
private static (nint, error) Pwrite(this ptr<FD> _addr_fd, slice<byte> p, long off) => func((defer, _, _) => {
    nint _p0 = default;
    error _p0 = default!;
    ref FD fd = ref _addr_fd.val;
 
    // Call incref, not writeLock, because since pwrite specifies the
    // offset it is independent from other writes.
    // Similarly, using the poller doesn't make sense for pwrite.
    {
        var err = fd.incref();

        if (err != null) {
            return (0, error.As(err)!);
        }
    }
    defer(fd.decref());
    nint nn = default;
    while (true) {
        var max = len(p);
        if (fd.IsStream && max - nn > maxRW) {
            max = nn + maxRW;
        }
        var (n, err) = syscall.Pwrite(fd.Sysfd, p[(int)nn..(int)max], off + int64(nn));
        if (err == syscall.EINTR) {
            continue;
        }
        if (n > 0) {
            nn += n;
        }
        if (nn == len(p)) {
            return (nn, error.As(err)!);
        }
        if (err != null) {
            return (nn, error.As(err)!);
        }
        if (n == 0) {
            return (nn, error.As(io.ErrUnexpectedEOF)!);
        }
    }
});

// WriteTo wraps the sendto network call.
private static (nint, error) WriteTo(this ptr<FD> _addr_fd, slice<byte> p, syscall.Sockaddr sa) => func((defer, _, _) => {
    nint _p0 = default;
    error _p0 = default!;
    ref FD fd = ref _addr_fd.val;

    {
        var err__prev1 = err;

        var err = fd.writeLock();

        if (err != null) {
            return (0, error.As(err)!);
        }
        err = err__prev1;

    }
    defer(fd.writeUnlock());
    {
        var err__prev1 = err;

        err = fd.pd.prepareWrite(fd.isFile);

        if (err != null) {
            return (0, error.As(err)!);
        }
        err = err__prev1;

    }
    while (true) {
        err = syscall.Sendto(fd.Sysfd, p, 0, sa);
        if (err == syscall.EINTR) {
            continue;
        }
        if (err == syscall.EAGAIN && fd.pd.pollable()) {
            err = fd.pd.waitWrite(fd.isFile);

            if (err == null) {
                continue;
            }
        }
        if (err != null) {
            return (0, error.As(err)!);
        }
        return (len(p), error.As(null!)!);
    }
});

// WriteMsg wraps the sendmsg network call.
private static (nint, nint, error) WriteMsg(this ptr<FD> _addr_fd, slice<byte> p, slice<byte> oob, syscall.Sockaddr sa) => func((defer, _, _) => {
    nint _p0 = default;
    nint _p0 = default;
    error _p0 = default!;
    ref FD fd = ref _addr_fd.val;

    {
        var err__prev1 = err;

        var err = fd.writeLock();

        if (err != null) {
            return (0, 0, error.As(err)!);
        }
        err = err__prev1;

    }
    defer(fd.writeUnlock());
    {
        var err__prev1 = err;

        err = fd.pd.prepareWrite(fd.isFile);

        if (err != null) {
            return (0, 0, error.As(err)!);
        }
        err = err__prev1;

    }
    while (true) {
        var (n, err) = syscall.SendmsgN(fd.Sysfd, p, oob, sa, 0);
        if (err == syscall.EINTR) {
            continue;
        }
        if (err == syscall.EAGAIN && fd.pd.pollable()) {
            err = fd.pd.waitWrite(fd.isFile);

            if (err == null) {
                continue;
            }
        }
        if (err != null) {
            return (n, 0, error.As(err)!);
        }
        return (n, len(oob), error.As(err)!);
    }
});

// Accept wraps the accept network call.
private static (nint, syscall.Sockaddr, @string, error) Accept(this ptr<FD> _addr_fd) => func((defer, _, _) => {
    nint _p0 = default;
    syscall.Sockaddr _p0 = default;
    @string _p0 = default;
    error _p0 = default!;
    ref FD fd = ref _addr_fd.val;

    {
        var err__prev1 = err;

        var err = fd.readLock();

        if (err != null) {
            return (-1, null, "", error.As(err)!);
        }
        err = err__prev1;

    }
    defer(fd.readUnlock());

    {
        var err__prev1 = err;

        err = fd.pd.prepareRead(fd.isFile);

        if (err != null) {
            return (-1, null, "", error.As(err)!);
        }
        err = err__prev1;

    }
    while (true) {
        var (s, rsa, errcall, err) = accept(fd.Sysfd);
        if (err == null) {
            return (s, rsa, "", error.As(err)!);
        }

        if (err == syscall.EINTR) 
            continue;
        else if (err == syscall.EAGAIN) 
            if (fd.pd.pollable()) {
                err = fd.pd.waitRead(fd.isFile);

                if (err == null) {
                    continue;
                }
            }
        else if (err == syscall.ECONNABORTED) 
            // This means that a socket on the listen
            // queue was closed before we Accept()ed it;
            // it's a silly error, so try again.
            continue;
                return (-1, null, errcall, error.As(err)!);
    }
});

// Seek wraps syscall.Seek.
private static (long, error) Seek(this ptr<FD> _addr_fd, long offset, nint whence) => func((defer, _, _) => {
    long _p0 = default;
    error _p0 = default!;
    ref FD fd = ref _addr_fd.val;

    {
        var err = fd.incref();

        if (err != null) {
            return (0, error.As(err)!);
        }
    }
    defer(fd.decref());
    return syscall.Seek(fd.Sysfd, offset, whence);
});

// ReadDirent wraps syscall.ReadDirent.
// We treat this like an ordinary system call rather than a call
// that tries to fill the buffer.
private static (nint, error) ReadDirent(this ptr<FD> _addr_fd, slice<byte> buf) => func((defer, _, _) => {
    nint _p0 = default;
    error _p0 = default!;
    ref FD fd = ref _addr_fd.val;

    {
        var err = fd.incref();

        if (err != null) {
            return (0, error.As(err)!);
        }
    }
    defer(fd.decref());
    while (true) {
        var (n, err) = ignoringEINTRIO(syscall.ReadDirent, fd.Sysfd, buf);
        if (err != null) {
            n = 0;
            if (err == syscall.EAGAIN && fd.pd.pollable()) {
                err = fd.pd.waitRead(fd.isFile);

                if (err == null) {
                    continue;
                }
            }
        }
        return (n, error.As(err)!);
    }
});

// Fchmod wraps syscall.Fchmod.
private static error Fchmod(this ptr<FD> _addr_fd, uint mode) => func((defer, _, _) => {
    ref FD fd = ref _addr_fd.val;

    {
        var err = fd.incref();

        if (err != null) {
            return error.As(err)!;
        }
    }
    defer(fd.decref());
    return error.As(ignoringEINTR(() => error.As(syscall.Fchmod(fd.Sysfd, mode))!))!;
});

// Fchdir wraps syscall.Fchdir.
private static error Fchdir(this ptr<FD> _addr_fd) => func((defer, _, _) => {
    ref FD fd = ref _addr_fd.val;

    {
        var err = fd.incref();

        if (err != null) {
            return error.As(err)!;
        }
    }
    defer(fd.decref());
    return error.As(syscall.Fchdir(fd.Sysfd))!;
});

// Fstat wraps syscall.Fstat
private static error Fstat(this ptr<FD> _addr_fd, ptr<syscall.Stat_t> _addr_s) => func((defer, _, _) => {
    ref FD fd = ref _addr_fd.val;
    ref syscall.Stat_t s = ref _addr_s.val;

    {
        var err = fd.incref();

        if (err != null) {
            return error.As(err)!;
        }
    }
    defer(fd.decref());
    return error.As(ignoringEINTR(() => error.As(syscall.Fstat(fd.Sysfd, s))!))!;
});

// tryDupCloexec indicates whether F_DUPFD_CLOEXEC should be used.
// If the kernel doesn't support it, this is set to 0.
private static var tryDupCloexec = int32(1);

// DupCloseOnExec dups fd and marks it close-on-exec.
public static (nint, @string, error) DupCloseOnExec(nint fd) {
    nint _p0 = default;
    @string _p0 = default;
    error _p0 = default!;

    if (syscall.F_DUPFD_CLOEXEC != 0 && atomic.LoadInt32(_addr_tryDupCloexec) == 1) {
        var (r0, e1) = fcntl(fd, syscall.F_DUPFD_CLOEXEC, 0);
        if (e1 == null) {
            return (r0, "", error.As(null!)!);
        }

        if (e1._<syscall.Errno>() == syscall.EINVAL || e1._<syscall.Errno>() == syscall.ENOSYS) 
            // Old kernel, or js/wasm (which returns
            // ENOSYS). Fall back to the portable way from
            // now on.
            atomic.StoreInt32(_addr_tryDupCloexec, 0);
        else 
            return (-1, "fcntl", error.As(e1)!);
            }
    return dupCloseOnExecOld(fd);
}

// dupCloseOnExecOld is the traditional way to dup an fd and
// set its O_CLOEXEC bit, using two system calls.
private static (nint, @string, error) dupCloseOnExecOld(nint fd) => func((defer, _, _) => {
    nint _p0 = default;
    @string _p0 = default;
    error _p0 = default!;

    syscall.ForkLock.RLock();
    defer(syscall.ForkLock.RUnlock());
    var (newfd, err) = syscall.Dup(fd);
    if (err != null) {
        return (-1, "dup", error.As(err)!);
    }
    syscall.CloseOnExec(newfd);
    return (newfd, "", error.As(null!)!);
});

// Dup duplicates the file descriptor.
private static (nint, @string, error) Dup(this ptr<FD> _addr_fd) => func((defer, _, _) => {
    nint _p0 = default;
    @string _p0 = default;
    error _p0 = default!;
    ref FD fd = ref _addr_fd.val;

    {
        var err = fd.incref();

        if (err != null) {
            return (-1, "", error.As(err)!);
        }
    }
    defer(fd.decref());
    return DupCloseOnExec(fd.Sysfd);
});

// On Unix variants only, expose the IO event for the net code.

// WaitWrite waits until data can be read from fd.
private static error WaitWrite(this ptr<FD> _addr_fd) {
    ref FD fd = ref _addr_fd.val;

    return error.As(fd.pd.waitWrite(fd.isFile))!;
}

// WriteOnce is for testing only. It makes a single write call.
private static (nint, error) WriteOnce(this ptr<FD> _addr_fd, slice<byte> p) => func((defer, _, _) => {
    nint _p0 = default;
    error _p0 = default!;
    ref FD fd = ref _addr_fd.val;

    {
        var err = fd.writeLock();

        if (err != null) {
            return (0, error.As(err)!);
        }
    }
    defer(fd.writeUnlock());
    return ignoringEINTRIO(syscall.Write, fd.Sysfd, p);
});

// RawRead invokes the user-defined function f for a read operation.
private static error RawRead(this ptr<FD> _addr_fd, Func<System.UIntPtr, bool> f) => func((defer, _, _) => {
    ref FD fd = ref _addr_fd.val;

    {
        var err__prev1 = err;

        var err = fd.readLock();

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }
    defer(fd.readUnlock());
    {
        var err__prev1 = err;

        err = fd.pd.prepareRead(fd.isFile);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }
    while (true) {
        if (f(uintptr(fd.Sysfd))) {
            return error.As(null!)!;
        }
        {
            var err__prev1 = err;

            err = fd.pd.waitRead(fd.isFile);

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev1;

        }
    }
});

// RawWrite invokes the user-defined function f for a write operation.
private static error RawWrite(this ptr<FD> _addr_fd, Func<System.UIntPtr, bool> f) => func((defer, _, _) => {
    ref FD fd = ref _addr_fd.val;

    {
        var err__prev1 = err;

        var err = fd.writeLock();

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }
    defer(fd.writeUnlock());
    {
        var err__prev1 = err;

        err = fd.pd.prepareWrite(fd.isFile);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }
    while (true) {
        if (f(uintptr(fd.Sysfd))) {
            return error.As(null!)!;
        }
        {
            var err__prev1 = err;

            err = fd.pd.waitWrite(fd.isFile);

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev1;

        }
    }
});

// ignoringEINTRIO is like ignoringEINTR, but just for IO calls.
private static (nint, error) ignoringEINTRIO(Func<nint, slice<byte>, (nint, error)> fn, nint fd, slice<byte> p) {
    nint _p0 = default;
    error _p0 = default!;

    while (true) {
        var (n, err) = fn(fd, p);
        if (err != syscall.EINTR) {
            return (n, error.As(err)!);
        }
    }
}

} // end poll_package
