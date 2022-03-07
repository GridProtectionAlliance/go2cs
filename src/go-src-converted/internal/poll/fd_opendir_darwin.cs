// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package poll -- go2cs converted at 2022 March 06 22:13:00 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Program Files\Go\src\internal\poll\fd_opendir_darwin.go
using syscall = go.syscall_package;
using _@unsafe_ = go.@unsafe_package;

namespace go.@internal;

public static partial class poll_package {

    // OpenDir returns a pointer to a DIR structure suitable for
    // ReadDir. In case of an error, the name of the failed
    // syscall is returned along with a syscall.Errno.
private static (System.UIntPtr, @string, error) OpenDir(this ptr<FD> _addr_fd) {
    System.UIntPtr _p0 = default;
    @string _p0 = default;
    error _p0 = default!;
    ref FD fd = ref _addr_fd.val;
 
    // fdopendir(3) takes control of the file descriptor,
    // so use a dup.
    var (fd2, call, err) = fd.Dup();
    if (err != null) {
        return (0, call, error.As(err)!);
    }
    System.UIntPtr dir = default;
    while (true) {
        dir, err = fdopendir(fd2);
        if (err != syscall.EINTR) {
            break;
        }
    }
    if (err != null) {
        syscall.Close(fd2);
        return (0, "fdopendir", error.As(err)!);
    }
    return (dir, "", error.As(null!)!);

}

// Implemented in syscall/syscall_darwin.go.
//go:linkname fdopendir syscall.fdopendir
private static (System.UIntPtr, error) fdopendir(nint fd);

} // end poll_package
