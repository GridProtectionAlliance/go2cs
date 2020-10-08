// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 October 08 03:44:15 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\dir_plan9.go
using io = go.io_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        private static (slice<FileInfo>, error) readdir(this ptr<File> _addr_file, long n)
        {
            slice<FileInfo> _p0 = default;
            error _p0 = default!;
            ref File file = ref _addr_file.val;
 
            // If this file has no dirinfo, create one.
            if (file.dirinfo == null)
            {
                file.dirinfo = @new<dirInfo>();
            }
            var d = file.dirinfo;
            var size = n;
            if (size <= 0L)
            {
                size = 100L;
                n = -1L;
            }
            var fi = make_slice<FileInfo>(0L, size); // Empty with room to grow.
            while (n != 0L)
            { 
                // Refill the buffer if necessary.
                if (d.bufp >= d.nbuf)
                {
                    var (nb, err) = file.Read(d.buf[..]); 

                    // Update the buffer state before checking for errors.
                    d.bufp = 0L;
                    d.nbuf = nb;

                    if (err != null)
                    {
                        if (err == io.EOF)
                        {
                            break;
                        }
                        return (fi, error.As(addr(new PathError("readdir",file.name,err))!)!);

                    }
                    if (nb < syscall.STATFIXLEN)
                    {
                        return (fi, error.As(addr(new PathError("readdir",file.name,syscall.ErrShortStat))!)!);
                    }
                }
                var b = d.buf[d.bufp..];
                var m = int(uint16(b[0L]) | uint16(b[1L]) << (int)(8L)) + 2L;
                if (m < syscall.STATFIXLEN)
                {
                    return (fi, error.As(addr(new PathError("readdir",file.name,syscall.ErrShortStat))!)!);
                }
                var (dir, err) = syscall.UnmarshalDir(b[..m]);
                if (err != null)
                {
                    return (fi, error.As(addr(new PathError("readdir",file.name,err))!)!);
                }
                fi = append(fi, fileInfoFromStat(dir));

                d.bufp += m;
                n--;

            }

            if (n >= 0L && len(fi) == 0L)
            {
                return (fi, error.As(io.EOF)!);
            }
            return (fi, error.As(null!)!);

        }

        private static (slice<@string>, error) readdirnames(this ptr<File> _addr_file, long n)
        {
            slice<@string> names = default;
            error err = default!;
            ref File file = ref _addr_file.val;

            var (fi, err) = file.Readdir(n);
            names = make_slice<@string>(len(fi));
            foreach (var (i) in fi)
            {
                names[i] = fi[i].Name();
            }
            return ;

        }
    }
}
