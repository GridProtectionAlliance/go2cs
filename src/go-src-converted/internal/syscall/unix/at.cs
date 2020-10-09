// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build linux openbsd netbsd dragonfly

// package unix -- go2cs converted at 2020 October 09 04:50:54 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Go\src\internal\syscall\unix\at.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace @internal {
namespace syscall
{
    public static partial class unix_package
    {
        public static error Unlinkat(long dirfd, @string path, long flags)
        {
            ptr<byte> p;
            var (p, err) = syscall.BytePtrFromString(path);
            if (err != null)
            {
                return error.As(err)!;
            }
            var (_, _, errno) = syscall.Syscall(unlinkatTrap, uintptr(dirfd), uintptr(@unsafe.Pointer(p)), uintptr(flags));
            if (errno != 0L)
            {
                return error.As(errno)!;
            }
            return error.As(null!)!;

        }

        public static (long, error) Openat(long dirfd, @string path, long flags, uint perm)
        {
            long _p0 = default;
            error _p0 = default!;

            ptr<byte> p;
            var (p, err) = syscall.BytePtrFromString(path);
            if (err != null)
            {
                return (0L, error.As(err)!);
            }

            var (fd, _, errno) = syscall.Syscall6(openatTrap, uintptr(dirfd), uintptr(@unsafe.Pointer(p)), uintptr(flags), uintptr(perm), 0L, 0L);
            if (errno != 0L)
            {
                return (0L, error.As(errno)!);
            }

            return (int(fd), error.As(null!)!);

        }

        public static error Fstatat(long dirfd, @string path, ptr<syscall.Stat_t> _addr_stat, long flags)
        {
            ref syscall.Stat_t stat = ref _addr_stat.val;

            ptr<byte> p;
            var (p, err) = syscall.BytePtrFromString(path);
            if (err != null)
            {
                return error.As(err)!;
            }

            var (_, _, errno) = syscall.Syscall6(fstatatTrap, uintptr(dirfd), uintptr(@unsafe.Pointer(p)), uintptr(@unsafe.Pointer(stat)), uintptr(flags), 0L, 0L);
            if (errno != 0L)
            {
                return error.As(errno)!;
            }

            return error.As(null!)!;


        }
    }
}}}
