// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build dragonfly || freebsd || netbsd || openbsd
// +build dragonfly freebsd netbsd openbsd

// package os -- go2cs converted at 2022 March 13 05:28:03 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\pipe2_bsd.go
namespace go;

using syscall = syscall_package;

public static partial class os_package {

// Pipe returns a connected pair of Files; reads from r return bytes written to w.
// It returns the files and an error, if any.
public static (ptr<File>, ptr<File>, error) Pipe() {
    ptr<File> r = default!;
    ptr<File> w = default!;
    error err = default!;

    array<nint> p = new array<nint>(2);

    var e = syscall.Pipe2(p[(int)0..], syscall.O_CLOEXEC);
    if (e != null) {
        return (_addr_null!, _addr_null!, error.As(NewSyscallError("pipe", e))!);
    }
    return (_addr_newFile(uintptr(p[0]), "|0", kindPipe)!, _addr_newFile(uintptr(p[1]), "|1", kindPipe)!, error.As(null!)!);
}

} // end os_package
