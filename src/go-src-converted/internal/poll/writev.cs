// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux netbsd openbsd

// package poll -- go2cs converted at 2020 August 29 08:25:48 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Go\src\internal\poll\writev.go
using io = go.io_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class poll_package
    {
        // Writev wraps the writev system call.
        private static (long, error) Writev(this ref FD _fd, ref slice<slice<byte>> _v) => func(_fd, _v, (ref FD fd, ref slice<slice<byte>> v, Defer defer, Panic _, Recover __) =>
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

            slice<syscall.Iovec> iovecs = default;
            if (fd.iovecs != null)
            {
                iovecs = fd.iovecs.Value;
            }
            long maxVec = 1024L;

            long n = default;
            err = default;
            while (len(v.Value) > 0L)
            {
                iovecs = iovecs[..0L];
                foreach (var (_, chunk) in v.Value)
                {
                    if (len(chunk) == 0L)
                    {
                        continue;
                    }
                    iovecs = append(iovecs, new syscall.Iovec(Base:&chunk[0]));
                    if (fd.IsStream && len(chunk) > 1L << (int)(30L))
                    {
                        iovecs[len(iovecs) - 1L].SetLen(1L << (int)(30L));
                        break; // continue chunk on next writev
                    }
                    iovecs[len(iovecs) - 1L].SetLen(len(chunk));
                    if (len(iovecs) == maxVec)
                    {
                        break;
                    }
                }                if (len(iovecs) == 0L)
                {
                    break;
                }
                fd.iovecs = ref iovecs; // cache

                var (wrote, _, e0) = syscall.Syscall(syscall.SYS_WRITEV, uintptr(fd.Sysfd), uintptr(@unsafe.Pointer(ref iovecs[0L])), uintptr(len(iovecs)));
                if (wrote == ~uintptr(0L))
                {
                    wrote = 0L;
                }
                TestHookDidWritev(int(wrote));
                n += int64(wrote);
                consume(v, int64(wrote));
                if (e0 == syscall.EAGAIN)
                {
                    err = fd.pd.waitWrite(fd.isFile);

                    if (err == null)
                    {
                        continue;
                    }
                }
                else if (e0 != 0L)
                {
                    err = syscall.Errno(e0);
                }
                if (err != null)
                {
                    break;
                }
                if (n == 0L)
                {
                    err = io.ErrUnexpectedEOF;
                    break;
                }
            }
            return (n, err);
        });
    }
}}
