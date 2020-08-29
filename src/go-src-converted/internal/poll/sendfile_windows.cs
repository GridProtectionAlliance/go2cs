// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package poll -- go2cs converted at 2020 August 29 08:25:40 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Go\src\internal\poll\sendfile_windows.go
using syscall = go.syscall_package;
using static go.builtin;
using System;

namespace go {
namespace @internal
{
    public static partial class poll_package
    {
        // SendFile wraps the TransmitFile call.
        public static (long, error) SendFile(ref FD _fd, syscall.Handle src, long n) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            var (ft, err) = syscall.GetFileType(src);
            if (err != null)
            {
                return (0L, err);
            }
            if (ft == syscall.FILE_TYPE_PIPE)
            {
                return (0L, syscall.ESPIPE);
            }
            {
                var err = fd.writeLock();

                if (err != null)
                {
                    return (0L, err);
                }
            }
            defer(fd.writeUnlock());

            var o = ref fd.wop;
            o.qty = uint32(n);
            o.handle = src;
            var (done, err) = wsrv.ExecIO(o, o =>
            {
                return syscall.TransmitFile(o.fd.Sysfd, o.handle, o.qty, 0L, ref o.o, null, syscall.TF_WRITE_BEHIND);
            });
            return (int64(done), err);
        });
    }
}}
