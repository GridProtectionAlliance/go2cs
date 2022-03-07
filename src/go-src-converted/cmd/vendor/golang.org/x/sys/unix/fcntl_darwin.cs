// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package unix -- go2cs converted at 2022 March 06 23:26:34 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\fcntl_darwin.go
using @unsafe = go.@unsafe_package;

namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

    // FcntlInt performs a fcntl syscall on fd with the provided command and argument.
public static (nint, error) FcntlInt(System.UIntPtr fd, nint cmd, nint arg) {
    nint _p0 = default;
    error _p0 = default!;

    return fcntl(int(fd), cmd, arg);
}

// FcntlFlock performs a fcntl syscall for the F_GETLK, F_SETLK or F_SETLKW command.
public static error FcntlFlock(System.UIntPtr fd, nint cmd, ptr<Flock_t> _addr_lk) {
    ref Flock_t lk = ref _addr_lk.val;

    var (_, err) = fcntl(int(fd), cmd, int(uintptr(@unsafe.Pointer(lk))));
    return error.As(err)!;
}

// FcntlFstore performs a fcntl syscall for the F_PREALLOCATE command.
public static error FcntlFstore(System.UIntPtr fd, nint cmd, ptr<Fstore_t> _addr_fstore) {
    ref Fstore_t fstore = ref _addr_fstore.val;

    var (_, err) = fcntl(int(fd), cmd, int(uintptr(@unsafe.Pointer(fstore))));
    return error.As(err)!;
}

} // end unix_package
