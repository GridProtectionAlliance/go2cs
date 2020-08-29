// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 August 29 08:44:24 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\stat_plan9.go
using syscall = go.syscall_package;
using time = go.time_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        private static readonly long _BIT16SZ = 2L;



        private static FileInfo fileInfoFromStat(ref syscall.Dir d)
        {
            fileStat fs = ref new fileStat(name:d.Name,size:d.Length,modTime:time.Unix(int64(d.Mtime),0),sys:d,);
            fs.mode = FileMode(d.Mode & 0777L);
            if (d.Mode & syscall.DMDIR != 0L)
            {
                fs.mode |= ModeDir;
            }
            if (d.Mode & syscall.DMAPPEND != 0L)
            {
                fs.mode |= ModeAppend;
            }
            if (d.Mode & syscall.DMEXCL != 0L)
            {
                fs.mode |= ModeExclusive;
            }
            if (d.Mode & syscall.DMTMP != 0L)
            {
                fs.mode |= ModeTemporary;
            } 
            // Consider all files not served by #M as device files.
            if (d.Type != 'M')
            {
                fs.mode |= ModeDevice;
            }
            return fs;
        }

        // arg is an open *File or a path string.
        private static (ref syscall.Dir, error) dirstat(object arg) => func((_, panic, __) =>
        {
            @string name = default;
            error err = default;

            var size = syscall.STATFIXLEN + 16L * 4L;

            for (long i = 0L; i < 2L; i++)
            {
                var buf = make_slice<byte>(_BIT16SZ + size);

                long n = default;
                switch (arg.type())
                {
                    case ref File a:
                        name = a.name;
                        n, err = syscall.Fstat(a.fd, buf);
                        break;
                    case @string a:
                        name = a;
                        n, err = syscall.Stat(a, buf);
                        break;
                    default:
                    {
                        var a = arg.type();
                        panic("phase error in dirstat");
                        break;
                    }

                }

                if (n < _BIT16SZ)
                {
                    return (null, ref new PathError("stat",name,err));
                } 

                // Pull the real size out of the stat message.
                size = int(uint16(buf[0L]) | uint16(buf[1L]) << (int)(8L)); 

                // If the stat message is larger than our buffer we will
                // go around the loop and allocate one that is big enough.
                if (size <= n)
                {
                    var (d, err) = syscall.UnmarshalDir(buf[..n]);
                    if (err != null)
                    {
                        return (null, ref new PathError("stat",name,err));
                    }
                    return (d, null);
                }
            }


            if (err == null)
            {
                err = error.As(syscall.ErrBadStat);
            }
            return (null, ref new PathError("stat",name,err));
        });

        // statNolog implements Stat for Plan 9.
        private static (FileInfo, error) statNolog(@string name)
        {
            var (d, err) = dirstat(name);
            if (err != null)
            {
                return (null, err);
            }
            return (fileInfoFromStat(d), null);
        }

        // lstatNolog implements Lstat for Plan 9.
        private static (FileInfo, error) lstatNolog(@string name)
        {
            return statNolog(name);
        }

        // For testing.
        private static time.Time atime(FileInfo fi)
        {
            return time.Unix(int64(fi.Sys()._<ref syscall.Dir>().Atime), 0L);
        }
    }
}
