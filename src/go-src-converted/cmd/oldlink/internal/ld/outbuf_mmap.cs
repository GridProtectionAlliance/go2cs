// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux openbsd

// package ld -- go2cs converted at 2020 October 08 04:41:44 UTC
// import "cmd/oldlink/internal/ld" ==> using ld = go.cmd.oldlink.@internal.ld_package
// Original source: C:\Go\src\cmd\oldlink\internal\ld\outbuf_mmap.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace oldlink {
namespace @internal
{
    public static partial class ld_package
    {
        private static error Mmap(this ptr<OutBuf> _addr_@out, ulong filesize)
        {
            ref OutBuf @out = ref _addr_@out.val;

            var err = @out.f.Truncate(int64(filesize));
            if (err != null)
            {
                Exitf("resize output file failed: %v", err);
            }
            @out.buf, err = syscall.Mmap(int(@out.f.Fd()), 0L, int(filesize), syscall.PROT_READ | syscall.PROT_WRITE, syscall.MAP_SHARED | syscall.MAP_FILE);
            return error.As(err)!;

        }

        private static void Munmap(this ptr<OutBuf> _addr_@out)
        {
            ref OutBuf @out = ref _addr_@out.val;

            var err = @out.Msync();
            if (err != null)
            {
                Exitf("msync output file failed: %v", err);
            }

            syscall.Munmap(@out.buf);
            @out.buf = null;
            _, err = @out.f.Seek(@out.off, 0L);
            if (err != null)
            {
                Exitf("seek output file failed: %v", err);
            }

        }

        private static error Msync(this ptr<OutBuf> _addr_@out)
        {
            ref OutBuf @out = ref _addr_@out.val;
 
            // TODO: netbsd supports mmap and msync, but the syscall package doesn't define MSYNC.
            // It is excluded from the build tag for now.
            var (_, _, errno) = syscall.Syscall(syscall.SYS_MSYNC, uintptr(@unsafe.Pointer(_addr_@out.buf[0L])), uintptr(len(@out.buf)), syscall.MS_SYNC);
            if (errno != 0L)
            {
                return error.As(errno)!;
            }

            return error.As(null!)!;

        }
    }
}}}}
