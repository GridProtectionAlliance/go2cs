// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux openbsd

// package ld -- go2cs converted at 2020 October 09 05:50:13 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\outbuf_mmap.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class ld_package
    {
        private static error Mmap(this ptr<OutBuf> _addr_@out, ulong filesize)
        {
            error err = default!;
            ref OutBuf @out = ref _addr_@out.val;

            while (true)
            {
                err = @out.fallocate(filesize);

                if (err != syscall.EINTR)
                {
                    break;
                }
            }
            if (err != null)
            { 
                // Some file systems do not support fallocate. We ignore that error as linking
                // can still take place, but you might SIGBUS when you write to the mmapped
                // area.
                if (err.Error() != fallocateNotSupportedErr)
                {
                    return error.As(err)!;
                }
            }
            err = @out.f.Truncate(int64(filesize));
            if (err != null)
            {
                Exitf("resize output file failed: %v", err);
            }
            @out.buf, err = syscall.Mmap(int(@out.f.Fd()), 0L, int(filesize), syscall.PROT_READ | syscall.PROT_WRITE, syscall.MAP_SHARED | syscall.MAP_FILE);
            return error.As(err)!;

        }

        private static void munmap(this ptr<OutBuf> _addr_@out)
        {
            ref OutBuf @out = ref _addr_@out.val;

            if (@out.buf == null)
            {
                return ;
            }

            syscall.Munmap(@out.buf);
            @out.buf = null;
            var (_, err) = @out.f.Seek(@out.off, 0L);
            if (err != null)
            {
                Exitf("seek output file failed: %v", err);
            }

        }
    }
}}}}
