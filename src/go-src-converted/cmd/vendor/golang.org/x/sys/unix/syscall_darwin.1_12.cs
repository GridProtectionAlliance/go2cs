// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build darwin && go1.12 && !go1.13
// +build darwin,go1.12,!go1.13

// package unix -- go2cs converted at 2022 March 13 06:41:22 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_darwin.1_12.go
namespace go.cmd.vendor.golang.org.x.sys;

using @unsafe = @unsafe_package;

public static partial class unix_package {

private static readonly nint _SYS_GETDIRENTRIES64 = 344;



public static (nint, error) Getdirentries(nint fd, slice<byte> buf, ptr<System.UIntPtr> _addr_basep) {
    nint n = default;
    error err = default!;
    ref System.UIntPtr basep = ref _addr_basep.val;
 
    // To implement this using libSystem we'd need syscall_syscallPtr for
    // fdopendir. However, syscallPtr was only added in Go 1.13, so we fall
    // back to raw syscalls for this func on Go 1.12.
    unsafe.Pointer p = default;
    if (len(buf) > 0) {
        p = @unsafe.Pointer(_addr_buf[0]);
    }
    else
 {
        p = @unsafe.Pointer(_addr__zero);
    }
    var (r0, _, e1) = Syscall6(_SYS_GETDIRENTRIES64, uintptr(fd), uintptr(p), uintptr(len(buf)), uintptr(@unsafe.Pointer(basep)), 0, 0);
    n = int(r0);
    if (e1 != 0) {
        return (n, error.As(errnoErr(e1))!);
    }
    return (n, error.As(null!)!);
}

} // end unix_package
