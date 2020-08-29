// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 August 29 08:44:12 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\pipe_freebsd.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        // Pipe returns a connected pair of Files; reads from r return bytes written to w.
        // It returns the files and an error, if any.
        public static (ref File, ref File, error) Pipe()
        {
            array<long> p = new array<long>(2L);

            var e = syscall.Pipe2(p[0L..], syscall.O_CLOEXEC);
            if (e != null)
            {
                return (null, null, NewSyscallError("pipe", e));
            }
            return (newFile(uintptr(p[0L]), "|0", kindPipe), newFile(uintptr(p[1L]), "|1", kindPipe), null);
        }
    }
}
