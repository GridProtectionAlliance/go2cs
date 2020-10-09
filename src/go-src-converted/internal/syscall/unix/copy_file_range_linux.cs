// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package unix -- go2cs converted at 2020 October 09 04:50:57 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Go\src\internal\syscall\unix\copy_file_range_linux.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace @internal {
namespace syscall
{
    public static partial class unix_package
    {
        public static (long, error) CopyFileRange(long rfd, ptr<long> _addr_roff, long wfd, ptr<long> _addr_woff, long len, long flags)
        {
            long n = default;
            error err = default!;
            ref long roff = ref _addr_roff.val;
            ref long woff = ref _addr_woff.val;

            var (r1, _, errno) = syscall.Syscall6(copyFileRangeTrap, uintptr(rfd), uintptr(@unsafe.Pointer(roff)), uintptr(wfd), uintptr(@unsafe.Pointer(woff)), uintptr(len), uintptr(flags));
            n = int(r1);
            if (errno != 0L)
            {
                err = errno;
            }
            return ;

        }
    }
}}}
