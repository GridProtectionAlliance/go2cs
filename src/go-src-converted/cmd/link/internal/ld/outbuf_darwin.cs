// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2020 October 09 05:50:13 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\outbuf_darwin.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class ld_package
    {
        private static error fallocate(this ptr<OutBuf> _addr_@out, ulong size)
        {
            ref OutBuf @out = ref _addr_@out.val;

            var (stat, err) = @out.f.Stat();
            if (err != null)
            {
                return error.As(err)!;
            }
            var cursize = uint64(stat.Sys()._<ptr<syscall.Stat_t>>().Blocks * 512L); // allocated size
            if (size <= cursize)
            {
                return error.As(null!)!;
            }
            ptr<syscall.Fstore_t> store = addr(new syscall.Fstore_t(Flags:syscall.F_ALLOCATEALL,Posmode:syscall.F_PEOFPOSMODE,Offset:0,Length:int64(size-cursize),));

            var (_, _, errno) = syscall.Syscall(syscall.SYS_FCNTL, uintptr(@out.f.Fd()), syscall.F_PREALLOCATE, uintptr(@unsafe.Pointer(store)));
            if (errno != 0L)
            {
                return error.As(errno)!;
            }
            return error.As(null!)!;

        }
    }
}}}}
