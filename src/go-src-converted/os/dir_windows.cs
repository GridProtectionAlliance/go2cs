// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 October 09 05:06:59 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\dir_windows.go
using io = go.io_package;
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        private static (slice<FileInfo>, error) readdir(this ptr<File> _addr_file, long n)
        {
            slice<FileInfo> fi = default;
            error err = default!;
            ref File file = ref _addr_file.val;

            if (file == null)
            {
                return (null, error.As(syscall.EINVAL)!);
            }
            if (!file.isdir())
            {
                return (null, error.As(addr(new PathError("Readdir",file.name,syscall.ENOTDIR))!)!);
            }
            var wantAll = n <= 0L;
            var size = n;
            if (wantAll)
            {
                n = -1L;
                size = 100L;
            }
            fi = make_slice<FileInfo>(0L, size); // Empty with room to grow.
            var d = _addr_file.dirinfo.data;
            while (n != 0L && !file.dirinfo.isempty)
            {
                if (file.dirinfo.needdata)
                {
                    var e = file.pfd.FindNextFile(d);
                    runtime.KeepAlive(file);
                    if (e != null)
                    {
                        if (e == syscall.ERROR_NO_MORE_FILES)
                        {
                            break;
                        }
                        else
                        {
                            err = addr(new PathError("FindNextFile",file.name,e));
                            if (!wantAll)
                            {
                                fi = null;
                            }
                            return ;

                        }
                    }
                }
                file.dirinfo.needdata = true;
                var name = syscall.UTF16ToString(d.FileName[0L..]);
                if (name == "." || name == "..")
                { // Useless names
                    continue;

                }
                var f = newFileStatFromWin32finddata(d);
                f.name = name;
                f.path = file.dirinfo.path;
                f.appendNameToPath = true;
                n--;
                fi = append(fi, f);

            }
            if (!wantAll && len(fi) == 0L)
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

            var (fis, err) = file.Readdir(n);
            names = make_slice<@string>(len(fis));
            foreach (var (i, fi) in fis)
            {
                names[i] = fi.Name();
            }
            return (names, error.As(err)!);

        }
    }
}
