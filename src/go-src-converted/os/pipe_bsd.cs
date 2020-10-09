// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly js,wasm solaris

// package os -- go2cs converted at 2020 October 09 05:07:18 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\pipe_bsd.go
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

            // See ../syscall/exec.go for description of lock.
            syscall.ForkLock.RLock();
            var e = syscall.Pipe(p[0L..]);
            if (e != null)
            {
                syscall.ForkLock.RUnlock();
                return (_addr_null!, _addr_null!, error.As(NewSyscallError("pipe", e))!);
            }
            syscall.CloseOnExec(p[0L]);
            syscall.CloseOnExec(p[1L]);
            syscall.ForkLock.RUnlock();

            return (_addr_newFile(uintptr(p[0L]), "|0", kindPipe)!, _addr_newFile(uintptr(p[1L]), "|1", kindPipe)!, error.As(null!)!);

        }
    }
}
