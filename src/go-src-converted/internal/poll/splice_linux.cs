// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package poll -- go2cs converted at 2020 October 09 04:51:24 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Go\src\internal\poll\splice_linux.go
using unix = go.@internal.syscall.unix_package;
using atomic = go.sync.atomic_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class poll_package
    {
 
        // spliceNonblock makes calls to splice(2) non-blocking.
        private static readonly ulong spliceNonblock = (ulong)0x2UL; 

        // maxSpliceSize is the maximum amount of data Splice asks
        // the kernel to move in a single call to splice(2).
        private static readonly long maxSpliceSize = (long)4L << (int)(20L);


        // Splice transfers at most remain bytes of data from src to dst, using the
        // splice system call to minimize copies of data from and to userspace.
        //
        // Splice creates a temporary pipe, to serve as a buffer for the data transfer.
        // src and dst must both be stream-oriented sockets.
        //
        // If err != nil, sc is the system call which caused the error.
        public static (long, bool, @string, error) Splice(ptr<FD> _addr_dst, ptr<FD> _addr_src, long remain) => func((defer, _, __) =>
        {
            long written = default;
            bool handled = default;
            @string sc = default;
            error err = default!;
            ref FD dst = ref _addr_dst.val;
            ref FD src = ref _addr_src.val;

            var (prfd, pwfd, sc, err) = newTempPipe();
            if (err != null)
            {
                return (0L, false, sc, error.As(err)!);
            }

            defer(destroyTempPipe(prfd, pwfd));
            long inPipe = default;            long n = default;

            while (err == null && remain > 0L)
            {
                var max = maxSpliceSize;
                if (int64(max) > remain)
                {
                    max = int(remain);
                }

                inPipe, err = spliceDrain(pwfd, _addr_src, max); 
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
                if (err != null || (inPipe == 0L && err == null))
                {
                    break;
                }

                n, err = splicePump(_addr_dst, prfd, inPipe);
                if (n > 0L)
                {
                    written += int64(n);
                    remain -= int64(n);
                }

            }

            if (err != null)
            {
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
        private static (long, error) spliceDrain(long pipefd, ptr<FD> _addr_sock, long max) => func((defer, _, __) =>
        {
            long _p0 = default;
            error _p0 = default!;
            ref FD sock = ref _addr_sock.val;

            {
                var err__prev1 = err;

                var err = sock.readLock();

                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

                err = err__prev1;

            }

            defer(sock.readUnlock());
            {
                var err__prev1 = err;

                err = sock.pd.prepareRead(sock.isFile);

                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

                err = err__prev1;

            }

            while (true)
            {
                var (n, err) = splice(pipefd, sock.Sysfd, max, spliceNonblock);
                if (err == syscall.EINTR)
                {
                    continue;
                }

                if (err != syscall.EAGAIN)
                {
                    return (n, error.As(err)!);
                }

                {
                    var err__prev1 = err;

                    err = sock.pd.waitRead(sock.isFile);

                    if (err != null)
                    {
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
        private static (long, error) splicePump(ptr<FD> _addr_sock, long pipefd, long inPipe) => func((defer, _, __) =>
        {
            long _p0 = default;
            error _p0 = default!;
            ref FD sock = ref _addr_sock.val;

            {
                var err__prev1 = err;

                var err = sock.writeLock();

                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

                err = err__prev1;

            }

            defer(sock.writeUnlock());
            {
                var err__prev1 = err;

                err = sock.pd.prepareWrite(sock.isFile);

                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

                err = err__prev1;

            }

            long written = 0L;
            while (inPipe > 0L)
            {
                var (n, err) = splice(sock.Sysfd, pipefd, inPipe, spliceNonblock); 
                // Here, the condition n == 0 && err == nil should never be
                // observed, since Splice controls the write side of the pipe.
                if (n > 0L)
                {
                    inPipe -= n;
                    written += n;
                    continue;
                }

                if (err != syscall.EAGAIN)
                {
                    return (written, error.As(err)!);
                }

                {
                    var err__prev1 = err;

                    err = sock.pd.waitWrite(sock.isFile);

                    if (err != null)
                    {
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
        private static (long, error) splice(long @out, long @in, long max, long flags)
        {
            long _p0 = default;
            error _p0 = default!;

            var (n, err) = syscall.Splice(in, null, out, null, max, flags);
            return (int(n), error.As(err)!);
        }

        private static unsafe.Pointer disableSplice = default;

        // newTempPipe sets up a temporary pipe for a splice operation.
        private static (long, long, @string, error) newTempPipe() => func((defer, _, __) =>
        {
            long prfd = default;
            long pwfd = default;
            @string sc = default;
            error err = default!;

            var p = (bool.val)(atomic.LoadPointer(_addr_disableSplice));
            if (p != null && p.val)
            {
                return (-1L, -1L, "splice", error.As(syscall.EINVAL)!);
            }

            array<long> fds = new array<long>(2L); 
            // pipe2 was added in 2.6.27 and our minimum requirement is 2.6.23, so it
            // might not be implemented. Falling back to pipe is possible, but prior to
            // 2.6.29 splice returns -EAGAIN instead of 0 when the connection is
            // closed.
            const var flags = syscall.O_CLOEXEC | syscall.O_NONBLOCK;

            {
                var err = syscall.Pipe2(fds[..], flags);

                if (err != null)
                {
                    return (-1L, -1L, "pipe2", error.As(err)!);
                }

            }


            if (p == null)
            {
                p = @new<bool>();
                defer(atomic.StorePointer(_addr_disableSplice, @unsafe.Pointer(p))); 

                // F_GETPIPE_SZ was added in 2.6.35, which does not have the -EAGAIN bug.
                {
                    var (_, _, errno) = syscall.Syscall(unix.FcntlSyscall, uintptr(fds[0L]), syscall.F_GETPIPE_SZ, 0L);

                    if (errno != 0L)
                    {
                        p.val = true;
                        destroyTempPipe(fds[0L], fds[1L]);
                        return (-1L, -1L, "fcntl", error.As(errno)!);
                    }

                }

            }

            return (fds[0L], fds[1L], "", error.As(null!)!);

        });

        // destroyTempPipe destroys a temporary pipe.
        private static error destroyTempPipe(long prfd, long pwfd)
        {
            var err = CloseFunc(prfd);
            var err1 = CloseFunc(pwfd);
            if (err == null)
            {
                return error.As(err1)!;
            }

            return error.As(err)!;

        }
    }
}}
