// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2020 October 09 05:01:33 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\sock_cloexec_linux.go

using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        // This is a stripped down version of sysSocket from net/sock_cloexec.go.
        private static (long, error) cloexecSocket(long family, long sotype, long proto)
        {
            long _p0 = default;
            error _p0 = default!;

            var (s, err) = Socket(family, sotype | SOCK_CLOEXEC, proto);

            if (err == null) 
                return (s, error.As(null!)!);
            else if (err == EINVAL)             else 
                return (-1L, error.As(err)!);
                        ForkLock.RLock();
            s, err = Socket(family, sotype, proto);
            if (err == null)
            {
                CloseOnExec(s);
            }
            ForkLock.RUnlock();
            if (err != null)
            {
                Close(s);
                return (-1L, error.As(err)!);
            }
            return (s, error.As(null!)!);

        }
    }
}
