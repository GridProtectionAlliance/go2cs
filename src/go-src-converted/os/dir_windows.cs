// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 August 29 08:43:32 UTC
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
        private static (slice<FileInfo>, error) readdir(this ref File file, long n)
        {
            if (file == null)
            {
                return (null, syscall.EINVAL);
            }
            if (!file.isdir())
            {
                return (null, ref new PathError("Readdir",file.name,syscall.ENOTDIR));
            }
            var wantAll = n <= 0L;
            var size = n;
            if (wantAll)
            {
                n = -1L;
                size = 100L;
            }
            fi = make_slice<FileInfo>(0L, size); // Empty with room to grow.
            var d = ref file.dirinfo.data;
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
                            err = ref new PathError("FindNextFile",file.name,e);
                            if (!wantAll)
                            {
                                fi = null;
                            }
                            return;
                        }
                    }
                }
                file.dirinfo.needdata = true;
                var name = syscall.UTF16ToString(d.FileName[0L..]);
                if (name == "." || name == "..")
                { // Useless names
                    continue;
                }
                fileStat f = ref new fileStat(name:name,sys:syscall.Win32FileAttributeData{FileAttributes:d.FileAttributes,CreationTime:d.CreationTime,LastAccessTime:d.LastAccessTime,LastWriteTime:d.LastWriteTime,FileSizeHigh:d.FileSizeHigh,FileSizeLow:d.FileSizeLow,},path:file.dirinfo.path,appendNameToPath:true,);
                n--;
                fi = append(fi, f);
            }
            if (!wantAll && len(fi) == 0L)
            {
                return (fi, io.EOF);
            }
            return (fi, null);
        }

        private static (slice<@string>, error) readdirnames(this ref File file, long n)
        {
            var (fis, err) = file.Readdir(n);
            names = make_slice<@string>(len(fis));
            foreach (var (i, fi) in fis)
            {
                names[i] = fi.Name();
            }
            return (names, err);
        }
    }
}
