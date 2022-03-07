// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || (js && wasm) || (solaris && !illumos)
// +build aix darwin js,wasm solaris,!illumos

// package os -- go2cs converted at 2022 March 06 22:13:43 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\pipe_bsd.go
using syscall = go.syscall_package;

namespace go;

public static partial class os_package {

    // Pipe returns a connected pair of Files; reads from r return bytes written to w.
    // It returns the files and an error, if any.
public static (ptr<File>, ptr<File>, error) Pipe() {
    ptr<File> r = default!;
    ptr<File> w = default!;
    error err = default!;

    array<nint> p = new array<nint>(2); 

    // See ../syscall/exec.go for description of lock.
    syscall.ForkLock.RLock();
    var e = syscall.Pipe(p[(int)0..]);
    if (e != null) {
        syscall.ForkLock.RUnlock();
        return (_addr_null!, _addr_null!, error.As(NewSyscallError("pipe", e))!);
    }
    syscall.CloseOnExec(p[0]);
    syscall.CloseOnExec(p[1]);
    syscall.ForkLock.RUnlock();

    return (_addr_newFile(uintptr(p[0]), "|0", kindPipe)!, _addr_newFile(uintptr(p[1]), "|1", kindPipe)!, error.As(null!)!);

}

} // end os_package
