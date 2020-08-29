// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 August 29 08:44:13 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\pipe_linux.go
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
            // pipe2 was added in 2.6.27 and our minimum requirement is 2.6.23, so it
            // might not be implemented.
            if (e == syscall.ENOSYS)
            { 
                // See ../syscall/exec.go for description of lock.
                syscall.ForkLock.RLock();
                e = syscall.Pipe(p[0L..]);
                if (e != null)
                {
                    syscall.ForkLock.RUnlock();
                    return (null, null, NewSyscallError("pipe", e));
                }
                syscall.CloseOnExec(p[0L]);
                syscall.CloseOnExec(p[1L]);
                syscall.ForkLock.RUnlock();
            }
            else if (e != null)
            {
                return (null, null, NewSyscallError("pipe2", e));
            }
            return (newFile(uintptr(p[0L]), "|0", kindPipe), newFile(uintptr(p[1L]), "|1", kindPipe), null);
        }
    }
}
