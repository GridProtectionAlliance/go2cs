// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build freebsd netbsd openbsd

// package os -- go2cs converted at 2020 October 08 03:44:53 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\pipe2_bsd.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        // Pipe returns a connected pair of Files; reads from r return bytes written to w.
        // It returns the files and an error, if any.
        public static (ptr<File>, ptr<File>, error) Pipe()
        {
            ptr<File> r = default!;
            ptr<File> w = default!;
            error err = default!;

            array<long> p = new array<long>(2L);

            var e = syscall.Pipe2(p[0L..], syscall.O_CLOEXEC);
            if (e != null)
            {
                return (_addr_null!, _addr_null!, error.As(NewSyscallError("pipe", e))!);
            }
            return (_addr_newFile(uintptr(p[0L]), "|0", kindPipe)!, _addr_newFile(uintptr(p[1L]), "|1", kindPipe)!, error.As(null!)!);

        }
    }
}
