// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package unix -- go2cs converted at 2020 October 08 03:31:48 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Go\src\internal\syscall\unix\at_darwin.go
using syscall = go.syscall_package;
using _@unsafe_ = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace @internal {
namespace syscall
{
    public static partial class unix_package
    {
        public static error Unlinkat(long dirfd, @string path, long flags)
        {
            return error.As(unlinkat(dirfd, path, flags))!;
        }

        public static (long, error) Openat(long dirfd, @string path, long flags, uint perm)
        {
            long _p0 = default;
            error _p0 = default!;

            return openat(dirfd, path, flags, perm);
        }

        public static error Fstatat(long dirfd, @string path, ptr<syscall.Stat_t> _addr_stat, long flags)
        {
            ref syscall.Stat_t stat = ref _addr_stat.val;

            return error.As(fstatat(dirfd, path, _addr_stat, flags))!;
        }

        //go:linkname unlinkat syscall.unlinkat
        private static error unlinkat(long dirfd, @string path, long flags)
;

        //go:linkname openat syscall.openat
        private static (long, error) openat(long dirfd, @string path, long flags, uint perm)
;

        //go:linkname fstatat syscall.fstatat
        private static error fstatat(long dirfd, @string path, ptr<syscall.Stat_t> stat, long flags)
;
    }
}}}
