// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !plan9

// package os -- go2cs converted at 2020 October 09 05:07:20 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\rawconn.go
using runtime = go.runtime_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class os_package
    {
        // rawConn implements syscall.RawConn.
        private partial struct rawConn
        {
            public ptr<File> file;
        }

        private static error Control(this ptr<rawConn> _addr_c, Action<System.UIntPtr> f)
        {
            ref rawConn c = ref _addr_c.val;

            {
                var err__prev1 = err;

                var err = c.file.checkValid("SyscallConn.Control");

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            err = c.file.pfd.RawControl(f);
            runtime.KeepAlive(c.file);
            return error.As(err)!;

        }

        private static error Read(this ptr<rawConn> _addr_c, Func<System.UIntPtr, bool> f)
        {
            ref rawConn c = ref _addr_c.val;

            {
                var err__prev1 = err;

                var err = c.file.checkValid("SyscallConn.Read");

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            err = c.file.pfd.RawRead(f);
            runtime.KeepAlive(c.file);
            return error.As(err)!;

        }

        private static error Write(this ptr<rawConn> _addr_c, Func<System.UIntPtr, bool> f)
        {
            ref rawConn c = ref _addr_c.val;

            {
                var err__prev1 = err;

                var err = c.file.checkValid("SyscallConn.Write");

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            err = c.file.pfd.RawWrite(f);
            runtime.KeepAlive(c.file);
            return error.As(err)!;

        }

        private static (ptr<rawConn>, error) newRawConn(ptr<File> _addr_file)
        {
            ptr<rawConn> _p0 = default!;
            error _p0 = default!;
            ref File file = ref _addr_file.val;

            return (addr(new rawConn(file:file)), error.As(null!)!);
        }
    }
}
