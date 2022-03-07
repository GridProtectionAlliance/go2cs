// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package unix -- go2cs converted at 2022 March 06 22:12:53 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Program Files\Go\src\internal\syscall\unix\copy_file_range_linux.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;

namespace go.@internal.syscall;

public static partial class unix_package {

public static (nint, error) CopyFileRange(nint rfd, ptr<long> _addr_roff, nint wfd, ptr<long> _addr_woff, nint len, nint flags) {
    nint n = default;
    error err = default!;
    ref long roff = ref _addr_roff.val;
    ref long woff = ref _addr_woff.val;

    var (r1, _, errno) = syscall.Syscall6(copyFileRangeTrap, uintptr(rfd), uintptr(@unsafe.Pointer(roff)), uintptr(wfd), uintptr(@unsafe.Pointer(woff)), uintptr(len), uintptr(flags));
    n = int(r1);
    if (errno != 0) {
        err = errno;
    }
    return ;

}

} // end unix_package
