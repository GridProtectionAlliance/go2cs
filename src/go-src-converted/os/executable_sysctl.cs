// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build freebsd || dragonfly
// +build freebsd dragonfly

// package os -- go2cs converted at 2022 March 13 05:27:55 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\executable_sysctl.go
namespace go;

using syscall = syscall_package;
using @unsafe = @unsafe_package;

public static partial class os_package {

private static (@string, error) executable() {
    @string _p0 = default;
    error _p0 = default!;

    array<int> mib = new array<int>(new int[] { _CTL_KERN, _KERN_PROC, _KERN_PROC_PATHNAME, -1 });

    ref var n = ref heap(uintptr(0), out ptr<var> _addr_n); 
    // get length
    var (_, _, err) = syscall.Syscall6(syscall.SYS___SYSCTL, uintptr(@unsafe.Pointer(_addr_mib[0])), 4, 0, uintptr(@unsafe.Pointer(_addr_n)), 0, 0);
    if (err != 0) {
        return ("", error.As(err)!);
    }
    if (n == 0) { // shouldn't happen
        return ("", error.As(null!)!);
    }
    var buf = make_slice<byte>(n);
    _, _, err = syscall.Syscall6(syscall.SYS___SYSCTL, uintptr(@unsafe.Pointer(_addr_mib[0])), 4, uintptr(@unsafe.Pointer(_addr_buf[0])), uintptr(@unsafe.Pointer(_addr_n)), 0, 0);
    if (err != 0) {
        return ("", error.As(err)!);
    }
    if (n == 0) { // shouldn't happen
        return ("", error.As(null!)!);
    }
    return (string(buf[..(int)n - 1]), error.As(null!)!);
}

} // end os_package
