// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux netbsd openbsd

// package poll -- go2cs converted at 2020 October 08 03:32:53 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Go\src\internal\poll\writev.go
using io = go.io_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class poll_package
    {
        // Writev wraps the writev system call.
        private static (long, error) Writev(this ptr<FD> _addr_fd, ptr<slice<slice<byte>>> _addr_v) => func((defer, _, __) =>
        {
            long _p0 = default;
            error _p0 = default!;
            ref FD fd = ref _addr_fd.val;
            ref slice<slice<byte>> v = ref _addr_v.val;

            {
                var err__prev1 = err;

                var err = fd.writeLock();

                if (err != null)
                {
                    return (0L, error.As(err)!);
                }
                err = err__prev1;

            }

            defer(fd.writeUnlock());
            {
                var err__prev1 = err;

                err = fd.pd.prepareWrite(fd.isFile);

                if (err != null)
                {
                    return (0L, error.As(err)!);
                }
                err = err__prev1;

            }


            slice<syscall.Iovec> iovecs = default;
            if (fd.iovecs != null)
            {
                iovecs = fd.iovecs.val;
            }
            long maxVec = 1024L;

            long n = default;
            err = default!;
            while (len(v) > 0L)
            {
                iovecs = iovecs[..0L];
                foreach (var (_, chunk) in v)
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
                if (fd.iovecs == null)
                {
                    fd.iovecs = @new<syscall.Iovec>();
                }
                fd.iovecs.val = iovecs; // cache

                System.UIntPtr wrote = default;
                wrote, err = writev(fd.Sysfd, iovecs);
                if (wrote == ~uintptr(0L))
                {
                    wrote = 0L;
                }
                TestHookDidWritev(int(wrote));
                n += int64(wrote);
                consume(v, int64(wrote));
                foreach (var (i) in iovecs)
                {
                    iovecs[i] = new syscall.Iovec();
                }                if (err != null)
                {
                    if (err == syscall.EINTR)
                    {
                        continue;
                    }
                    if (err == syscall.EAGAIN)
                    {
                        err = fd.pd.waitWrite(fd.isFile);

                        if (err == null)
                        {
                            continue;
                        }
                    }
                    break;

                }
                if (n == 0L)
                {
                    err = io.ErrUnexpectedEOF;
                    break;
                }
            }
            return (n, error.As(err)!);

        });
    }
}}
