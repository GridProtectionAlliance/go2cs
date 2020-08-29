// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux nacl netbsd openbsd solaris

// package os -- go2cs converted at 2020 August 29 08:44:26 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\stat_unix.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        // Stat returns the FileInfo structure describing file.
        // If there is an error, it will be of type *PathError.
        private static (FileInfo, error) Stat(this ref File f)
        {
            if (f == null)
            {
                return (null, ErrInvalid);
            }
            fileStat fs = default;
            var err = f.pfd.Fstat(ref fs.sys);
            if (err != null)
            {
                return (null, ref new PathError("stat",f.name,err));
            }
            fillFileStatFromSys(ref fs, f.name);
            return (ref fs, null);
        }

        // statNolog stats a file with no test logging.
        private static (FileInfo, error) statNolog(@string name)
        {
            fileStat fs = default;
            var err = syscall.Stat(name, ref fs.sys);
            if (err != null)
            {
                return (null, ref new PathError("stat",name,err));
            }
            fillFileStatFromSys(ref fs, name);
            return (ref fs, null);
        }

        // lstatNolog lstats a file with no test logging.
        private static (FileInfo, error) lstatNolog(@string name)
        {
            fileStat fs = default;
            var err = syscall.Lstat(name, ref fs.sys);
            if (err != null)
            {
                return (null, ref new PathError("lstat",name,err));
            }
            fillFileStatFromSys(ref fs, name);
            return (ref fs, null);
        }
    }
}
