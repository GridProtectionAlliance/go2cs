// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix dragonfly freebsd js,wasm linux netbsd openbsd solaris

// package os -- go2cs converted at 2020 October 09 05:06:59 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\dir_unix.go
using io = go.io_package;
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        // Auxiliary information if the File describes a directory
        private partial struct dirInfo
        {
            public slice<byte> buf; // buffer for directory I/O
            public long nbuf; // length of buf; return value from Getdirentries
            public long bufp; // location of next record in buf.
        }

 
        // More than 5760 to work around https://golang.org/issue/24015.
        private static readonly long blockSize = (long)8192L;


        private static void close(this ptr<dirInfo> _addr_d)
        {
            ref dirInfo d = ref _addr_d.val;

        }

        private static (slice<@string>, error) readdirnames(this ptr<File> _addr_f, long n)
        {
            slice<@string> names = default;
            error err = default!;
            ref File f = ref _addr_f.val;
 
            // If this file has no dirinfo, create one.
            if (f.dirinfo == null)
            {
                f.dirinfo = @new<dirInfo>(); 
                // The buffer must be at least a block long.
                f.dirinfo.buf = make_slice<byte>(blockSize);

            }

            var d = f.dirinfo;

            var size = n;
            if (size <= 0L)
            {
                size = 100L;
                n = -1L;
            }

            names = make_slice<@string>(0L, size); // Empty with room to grow.
            while (n != 0L)
            { 
                // Refill the buffer if necessary
                if (d.bufp >= d.nbuf)
                {
                    d.bufp = 0L;
                    error errno = default!;
                    d.nbuf, errno = f.pfd.ReadDirent(d.buf);
                    runtime.KeepAlive(f);
                    if (errno != null)
                    {
                        return (names, error.As(wrapSyscallError("readdirent", errno))!);
                    }

                    if (d.nbuf <= 0L)
                    {
                        break; // EOF
                    }

                } 

                // Drain the buffer
                long nb = default;                long nc = default;

                nb, nc, names = syscall.ParseDirent(d.buf[d.bufp..d.nbuf], n, names);
                d.bufp += nb;
                n -= nc;

            }

            if (n >= 0L && len(names) == 0L)
            {
                return (names, error.As(io.EOF)!);
            }

            return (names, error.As(null!)!);

        }
    }
}
