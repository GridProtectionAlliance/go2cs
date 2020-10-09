// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package poll -- go2cs converted at 2020 October 09 04:51:04 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Go\src\internal\poll\fd_opendir_darwin.go
using syscall = go.syscall_package;
using _@unsafe_ = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class poll_package
    {
        // OpenDir returns a pointer to a DIR structure suitable for
        // ReadDir. In case of an error, the name of the failed
        // syscall is returned along with a syscall.Errno.
        private static (System.UIntPtr, @string, error) OpenDir(this ptr<FD> _addr_fd)
        {
            System.UIntPtr _p0 = default;
            @string _p0 = default;
            error _p0 = default!;
            ref FD fd = ref _addr_fd.val;
 
            // fdopendir(3) takes control of the file descriptor,
            // so use a dup.
            var (fd2, call, err) = fd.Dup();
            if (err != null)
            {
                return (0L, call, error.As(err)!);
            }
            var (dir, err) = fdopendir(fd2);
            if (err != null)
            {
                syscall.Close(fd2);
                return (0L, "fdopendir", error.As(err)!);
            }
            return (dir, "", error.As(null!)!);

        }

        // Implemented in syscall/syscall_darwin.go.
        //go:linkname fdopendir syscall.fdopendir
        private static (System.UIntPtr, error) fdopendir(long fd)
;
    }
}}
