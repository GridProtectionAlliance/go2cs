// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux nacl netbsd openbsd solaris

// package poll -- go2cs converted at 2020 August 29 08:25:26 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Go\src\internal\poll\fd_unix.go
using io = go.io_package;
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using static go.builtin;
using System;

namespace go {
namespace @internal
{
    public static partial class poll_package
    {
        // FD is a file descriptor. The net and os packages use this type as a
        // field of a larger type representing a network connection or OS file.
        public partial struct FD
        {
            public fdMutex fdmu; // System file descriptor. Immutable until Close.
            public long Sysfd; // I/O poller.
            public pollDesc pd; // Writev cache.
            public ptr<slice<syscall.Iovec>> iovecs; // Semaphore signaled when file is closed.
            public uint csema; // Whether this is a streaming descriptor, as opposed to a
// packet-based descriptor like a UDP socket. Immutable.
            public bool IsStream; // Whether a zero byte read indicates EOF. This is false for a
// message based socket connection.
            public bool ZeroReadIsEOF; // Whether this is a file rather than a network socket.
            public bool isFile; // Whether this file has been set to blocking mode.
            public bool isBlocking;
        }

        // Init initializes the FD. The Sysfd field should already be set.
        // This can be called multiple times on a single FD.
        // The net argument is a network name from the net package (e.g., "tcp"),
        // or "file".
        // Set pollable to true if fd should be managed by runtime netpoll.
        private static error Init(this ref FD fd, @string net, bool pollable)
        { 
            // We don't actually care about the various network types.
            if (net == "file")
            {
                fd.isFile = true;
            }
            if (!pollable)
            {
                fd.isBlocking = true;
                return error.As(null);
            }
            return error.As(fd.pd.init(fd));
        }

        // Destroy closes the file descriptor. This is called when there are
        // no remaining references.
        private static error destroy(this ref FD fd)
        { 
            // Poller may want to unregister fd in readiness notification mechanism,
            // so this must be executed before CloseFunc.
            fd.pd.close();
            var err = CloseFunc(fd.Sysfd);
            fd.Sysfd = -1L;
            runtime_Semrelease(ref fd.csema);
            return error.As(err);
        }

        // Close closes the FD. The underlying file descriptor is closed by the
        // destroy method when there are no remaining references.
        private static error Close(this ref FD fd)
        {
            if (!fd.fdmu.increfAndClose())
            {
                return error.As(errClosing(fd.isFile));
            } 

            // Unblock any I/O.  Once it all unblocks and returns,
            // so that it cannot be referring to fd.sysfd anymore,
            // the final decref will close fd.sysfd. This should happen
            // fairly quickly, since all the I/O is non-blocking, and any
            // attempts to block in the pollDesc will return errClosing(fd.isFile).
            fd.pd.evict(); 

            // The call to decref will call destroy if there are no other
            // references.
            var err = fd.decref(); 

            // Wait until the descriptor is closed. If this was the only
            // reference, it is already closed. Only wait if the file has
            // not been set to blocking mode, as otherwise any current I/O
            // may be blocking, and that would block the Close.
            if (!fd.isBlocking)
            {
                runtime_Semacquire(ref fd.csema);
            }
            return error.As(err);
        }

        // Shutdown wraps the shutdown network call.
        private static error Shutdown(this ref FD _fd, long how) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            {
                var err = fd.incref();

                if (err != null)
                {
                    return error.As(err);
                }

            }
            defer(fd.decref());
            return error.As(syscall.Shutdown(fd.Sysfd, how));
        });

        // SetBlocking puts the file into blocking mode.
        private static error SetBlocking(this ref FD _fd) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            {
                var err = fd.incref();

                if (err != null)
                {
                    return error.As(err);
                }

            }
            defer(fd.decref());
            fd.isBlocking = true;
            return error.As(syscall.SetNonblock(fd.Sysfd, false));
        });

        // Darwin and FreeBSD can't read or write 2GB+ files at a time,
        // even on 64-bit systems.
        // The same is true of socket implementations on many systems.
        // See golang.org/issue/7812 and golang.org/issue/16266.
        // Use 1GB instead of, say, 2GB-1, to keep subsequent reads aligned.
        private static readonly long maxRW = 1L << (int)(30L);

        // Read implements io.Reader.


        // Read implements io.Reader.
        private static (long, error) Read(this ref FD _fd, slice<byte> p) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            {
                var err__prev1 = err;

                var err = fd.readLock();

                if (err != null)
                {
                    return (0L, err);
                }

                err = err__prev1;

            }
            defer(fd.readUnlock());
            if (len(p) == 0L)
            { 
                // If the caller wanted a zero byte read, return immediately
                // without trying (but after acquiring the readLock).
                // Otherwise syscall.Read returns 0, nil which looks like
                // io.EOF.
                // TODO(bradfitz): make it wait for readability? (Issue 15735)
                return (0L, null);
            }
            {
                var err__prev1 = err;

                err = fd.pd.prepareRead(fd.isFile);

                if (err != null)
                {
                    return (0L, err);
                }

                err = err__prev1;

            }
            if (fd.IsStream && len(p) > maxRW)
            {
                p = p[..maxRW];
            }
            while (true)
            {
                var (n, err) = syscall.Read(fd.Sysfd, p);
                if (err != null)
                {
                    n = 0L;
                    if (err == syscall.EAGAIN && fd.pd.pollable())
                    {
                        err = fd.pd.waitRead(fd.isFile);

                        if (err == null)
                        {
                            continue;
                        }
                    } 

                    // On MacOS we can see EINTR here if the user
                    // pressed ^Z.  See issue #22838.
                    if (runtime.GOOS == "darwin" && err == syscall.EINTR)
                    {
                        continue;
                    }
                }
                err = fd.eofError(n, err);
                return (n, err);
            }

        });

        // Pread wraps the pread system call.
        private static (long, error) Pread(this ref FD fd, slice<byte> p, long off)
        { 
            // Call incref, not readLock, because since pread specifies the
            // offset it is independent from other reads.
            // Similarly, using the poller doesn't make sense for pread.
            {
                var err = fd.incref();

                if (err != null)
                {
                    return (0L, err);
                }

            }
            if (fd.IsStream && len(p) > maxRW)
            {
                p = p[..maxRW];
            }
            var (n, err) = syscall.Pread(fd.Sysfd, p, off);
            if (err != null)
            {
                n = 0L;
            }
            fd.decref();
            err = fd.eofError(n, err);
            return (n, err);
        }

        // ReadFrom wraps the recvfrom network call.
        private static (long, syscall.Sockaddr, error) ReadFrom(this ref FD _fd, slice<byte> p) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            {
                var err__prev1 = err;

                var err = fd.readLock();

                if (err != null)
                {
                    return (0L, null, err);
                }

                err = err__prev1;

            }
            defer(fd.readUnlock());
            {
                var err__prev1 = err;

                err = fd.pd.prepareRead(fd.isFile);

                if (err != null)
                {
                    return (0L, null, err);
                }

                err = err__prev1;

            }
            while (true)
            {
                var (n, sa, err) = syscall.Recvfrom(fd.Sysfd, p, 0L);
                if (err != null)
                {
                    n = 0L;
                    if (err == syscall.EAGAIN && fd.pd.pollable())
                    {
                        err = fd.pd.waitRead(fd.isFile);

                        if (err == null)
                        {
                            continue;
                        }
                    }
                }
                err = fd.eofError(n, err);
                return (n, sa, err);
            }

        });

        // ReadMsg wraps the recvmsg network call.
        private static (long, long, long, syscall.Sockaddr, error) ReadMsg(this ref FD _fd, slice<byte> p, slice<byte> oob) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            {
                var err__prev1 = err;

                var err = fd.readLock();

                if (err != null)
                {
                    return (0L, 0L, 0L, null, err);
                }

                err = err__prev1;

            }
            defer(fd.readUnlock());
            {
                var err__prev1 = err;

                err = fd.pd.prepareRead(fd.isFile);

                if (err != null)
                {
                    return (0L, 0L, 0L, null, err);
                }

                err = err__prev1;

            }
            while (true)
            {
                var (n, oobn, flags, sa, err) = syscall.Recvmsg(fd.Sysfd, p, oob, 0L);
                if (err != null)
                { 
                    // TODO(dfc) should n and oobn be set to 0
                    if (err == syscall.EAGAIN && fd.pd.pollable())
                    {
                        err = fd.pd.waitRead(fd.isFile);

                        if (err == null)
                        {
                            continue;
                        }
                    }
                }
                err = fd.eofError(n, err);
                return (n, oobn, flags, sa, err);
            }

        });

        // Write implements io.Writer.
        private static (long, error) Write(this ref FD _fd, slice<byte> p) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            {
                var err__prev1 = err;

                var err = fd.writeLock();

                if (err != null)
                {
                    return (0L, err);
                }

                err = err__prev1;

            }
            defer(fd.writeUnlock());
            {
                var err__prev1 = err;

                err = fd.pd.prepareWrite(fd.isFile);

                if (err != null)
                {
                    return (0L, err);
                }

                err = err__prev1;

            }
            long nn = default;
            while (true)
            {
                var max = len(p);
                if (fd.IsStream && max - nn > maxRW)
                {
                    max = nn + maxRW;
                }
                var (n, err) = syscall.Write(fd.Sysfd, p[nn..max]);
                if (n > 0L)
                {
                    nn += n;
                }
                if (nn == len(p))
                {
                    return (nn, err);
                }
                if (err == syscall.EAGAIN && fd.pd.pollable())
                {
                    err = fd.pd.waitWrite(fd.isFile);

                    if (err == null)
                    {
                        continue;
                    }
                }
                if (err != null)
                {
                    return (nn, err);
                }
                if (n == 0L)
                {
                    return (nn, io.ErrUnexpectedEOF);
                }
            }

        });

        // Pwrite wraps the pwrite system call.
        private static (long, error) Pwrite(this ref FD _fd, slice<byte> p, long off) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        { 
            // Call incref, not writeLock, because since pwrite specifies the
            // offset it is independent from other writes.
            // Similarly, using the poller doesn't make sense for pwrite.
            {
                var err = fd.incref();

                if (err != null)
                {
                    return (0L, err);
                }

            }
            defer(fd.decref());
            long nn = default;
            while (true)
            {
                var max = len(p);
                if (fd.IsStream && max - nn > maxRW)
                {
                    max = nn + maxRW;
                }
                var (n, err) = syscall.Pwrite(fd.Sysfd, p[nn..max], off + int64(nn));
                if (n > 0L)
                {
                    nn += n;
                }
                if (nn == len(p))
                {
                    return (nn, err);
                }
                if (err != null)
                {
                    return (nn, err);
                }
                if (n == 0L)
                {
                    return (nn, io.ErrUnexpectedEOF);
                }
            }

        });

        // WriteTo wraps the sendto network call.
        private static (long, error) WriteTo(this ref FD _fd, slice<byte> p, syscall.Sockaddr sa) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            {
                var err__prev1 = err;

                var err = fd.writeLock();

                if (err != null)
                {
                    return (0L, err);
                }

                err = err__prev1;

            }
            defer(fd.writeUnlock());
            {
                var err__prev1 = err;

                err = fd.pd.prepareWrite(fd.isFile);

                if (err != null)
                {
                    return (0L, err);
                }

                err = err__prev1;

            }
            while (true)
            {
                err = syscall.Sendto(fd.Sysfd, p, 0L, sa);
                if (err == syscall.EAGAIN && fd.pd.pollable())
                {
                    err = fd.pd.waitWrite(fd.isFile);

                    if (err == null)
                    {
                        continue;
                    }
                }
                if (err != null)
                {
                    return (0L, err);
                }
                return (len(p), null);
            }

        });

        // WriteMsg wraps the sendmsg network call.
        private static (long, long, error) WriteMsg(this ref FD _fd, slice<byte> p, slice<byte> oob, syscall.Sockaddr sa) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            {
                var err__prev1 = err;

                var err = fd.writeLock();

                if (err != null)
                {
                    return (0L, 0L, err);
                }

                err = err__prev1;

            }
            defer(fd.writeUnlock());
            {
                var err__prev1 = err;

                err = fd.pd.prepareWrite(fd.isFile);

                if (err != null)
                {
                    return (0L, 0L, err);
                }

                err = err__prev1;

            }
            while (true)
            {
                var (n, err) = syscall.SendmsgN(fd.Sysfd, p, oob, sa, 0L);
                if (err == syscall.EAGAIN && fd.pd.pollable())
                {
                    err = fd.pd.waitWrite(fd.isFile);

                    if (err == null)
                    {
                        continue;
                    }
                }
                if (err != null)
                {
                    return (n, 0L, err);
                }
                return (n, len(oob), err);
            }

        });

        // Accept wraps the accept network call.
        private static (long, syscall.Sockaddr, @string, error) Accept(this ref FD _fd) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            {
                var err__prev1 = err;

                var err = fd.readLock();

                if (err != null)
                {
                    return (-1L, null, "", err);
                }

                err = err__prev1;

            }
            defer(fd.readUnlock());

            {
                var err__prev1 = err;

                err = fd.pd.prepareRead(fd.isFile);

                if (err != null)
                {
                    return (-1L, null, "", err);
                }

                err = err__prev1;

            }
            while (true)
            {
                var (s, rsa, errcall, err) = accept(fd.Sysfd);
                if (err == null)
                {
                    return (s, rsa, "", err);
                }

                if (err == syscall.EAGAIN) 
                    if (fd.pd.pollable())
                    {
                        err = fd.pd.waitRead(fd.isFile);

                        if (err == null)
                        {
                            continue;
                        }
                    }
                else if (err == syscall.ECONNABORTED) 
                    // This means that a socket on the listen
                    // queue was closed before we Accept()ed it;
                    // it's a silly error, so try again.
                    continue;
                                return (-1L, null, errcall, err);
            }

        });

        // Seek wraps syscall.Seek.
        private static (long, error) Seek(this ref FD _fd, long offset, long whence) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            {
                var err = fd.incref();

                if (err != null)
                {
                    return (0L, err);
                }

            }
            defer(fd.decref());
            return syscall.Seek(fd.Sysfd, offset, whence);
        });

        // ReadDirent wraps syscall.ReadDirent.
        // We treat this like an ordinary system call rather than a call
        // that tries to fill the buffer.
        private static (long, error) ReadDirent(this ref FD _fd, slice<byte> buf) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            {
                var err = fd.incref();

                if (err != null)
                {
                    return (0L, err);
                }

            }
            defer(fd.decref());
            while (true)
            {
                var (n, err) = syscall.ReadDirent(fd.Sysfd, buf);
                if (err != null)
                {
                    n = 0L;
                    if (err == syscall.EAGAIN && fd.pd.pollable())
                    {
                        err = fd.pd.waitRead(fd.isFile);

                        if (err == null)
                        {
                            continue;
                        }
                    }
                } 
                // Do not call eofError; caller does not expect to see io.EOF.
                return (n, err);
            }

        });

        // Fchdir wraps syscall.Fchdir.
        private static error Fchdir(this ref FD _fd) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            {
                var err = fd.incref();

                if (err != null)
                {
                    return error.As(err);
                }

            }
            defer(fd.decref());
            return error.As(syscall.Fchdir(fd.Sysfd));
        });

        // Fstat wraps syscall.Fstat
        private static error Fstat(this ref FD _fd, ref syscall.Stat_t _s) => func(_fd, _s, (ref FD fd, ref syscall.Stat_t s, Defer defer, Panic _, Recover __) =>
        {
            {
                var err = fd.incref();

                if (err != null)
                {
                    return error.As(err);
                }

            }
            defer(fd.decref());
            return error.As(syscall.Fstat(fd.Sysfd, s));
        });

        // On Unix variants only, expose the IO event for the net code.

        // WaitWrite waits until data can be read from fd.
        private static error WaitWrite(this ref FD fd)
        {
            return error.As(fd.pd.waitWrite(fd.isFile));
        }

        // WriteOnce is for testing only. It makes a single write call.
        private static (long, error) WriteOnce(this ref FD _fd, slice<byte> p) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            {
                var err = fd.writeLock();

                if (err != null)
                {
                    return (0L, err);
                }

            }
            defer(fd.writeUnlock());
            return syscall.Write(fd.Sysfd, p);
        });

        // RawControl invokes the user-defined function f for a non-IO
        // operation.
        private static error RawControl(this ref FD _fd, Action<System.UIntPtr> f) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            {
                var err = fd.incref();

                if (err != null)
                {
                    return error.As(err);
                }

            }
            defer(fd.decref());
            f(uintptr(fd.Sysfd));
            return error.As(null);
        });

        // RawRead invokes the user-defined function f for a read operation.
        private static error RawRead(this ref FD _fd, Func<System.UIntPtr, bool> f) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            {
                var err__prev1 = err;

                var err = fd.readLock();

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }
            defer(fd.readUnlock());
            {
                var err__prev1 = err;

                err = fd.pd.prepareRead(fd.isFile);

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }
            while (true)
            {
                if (f(uintptr(fd.Sysfd)))
                {
                    return error.As(null);
                }
                {
                    var err__prev1 = err;

                    err = fd.pd.waitRead(fd.isFile);

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev1;

                }
            }

        });

        // RawWrite invokes the user-defined function f for a write operation.
        private static error RawWrite(this ref FD _fd, Func<System.UIntPtr, bool> f) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            {
                var err__prev1 = err;

                var err = fd.writeLock();

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }
            defer(fd.writeUnlock());
            {
                var err__prev1 = err;

                err = fd.pd.prepareWrite(fd.isFile);

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }
            while (true)
            {
                if (f(uintptr(fd.Sysfd)))
                {
                    return error.As(null);
                }
                {
                    var err__prev1 = err;

                    err = fd.pd.waitWrite(fd.isFile);

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev1;

                }
            }

        });
    }
}}
