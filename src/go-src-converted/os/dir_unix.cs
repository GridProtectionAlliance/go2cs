// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux nacl netbsd openbsd solaris

// package os -- go2cs converted at 2020 August 29 08:43:30 UTC
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
        private static readonly long blockSize = 4096L;

        private static (slice<FileInfo>, error) readdir(this ref File f, long n)
        {
            var dirname = f.name;
            if (dirname == "")
            {
                dirname = ".";
            }
            var (names, err) = f.Readdirnames(n);
            fi = make_slice<FileInfo>(0L, len(names));
            foreach (var (_, filename) in names)
            {
                var (fip, lerr) = lstat(dirname + "/" + filename);
                if (IsNotExist(lerr))
                { 
                    // File disappeared between readdir + stat.
                    // Just treat it as if it didn't exist.
                    continue;
                }
                if (lerr != null)
                {
                    return (fi, lerr);
                }
                fi = append(fi, fip);
            }
            if (len(fi) == 0L && err == null && n > 0L)
            { 
                // Per File.Readdir, the slice must be non-empty or err
                // must be non-nil if n > 0.
                err = io.EOF;
            }
            return (fi, err);
        }

        private static (slice<@string>, error) readdirnames(this ref File f, long n)
        { 
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
                    error errno = default;
                    d.nbuf, errno = f.pfd.ReadDirent(d.buf);
                    runtime.KeepAlive(f);
                    if (errno != null)
                    {
                        return (names, wrapSyscallError("readdirent", errno));
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
                return (names, io.EOF);
            }
            return (names, null);
        }
    }
}
