// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build darwin || dragonfly || freebsd || (js && wasm) || netbsd || openbsd
// +build darwin dragonfly freebsd js,wasm netbsd openbsd

// package os -- go2cs converted at 2022 March 13 05:28:05 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\sys_bsd.go
namespace go;

using syscall = syscall_package;

public static partial class os_package {

private static (@string, error) hostname() {
    @string name = default;
    error err = default!;

    name, err = syscall.Sysctl("kern.hostname");
    if (err != null) {
        return ("", error.As(NewSyscallError("sysctl kern.hostname", err))!);
    }
    return (name, error.As(null!)!);
}

} // end os_package
