// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 August 29 08:44:39 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\types_windows.go
using sync = go.sync_package;
using syscall = go.syscall_package;
using time = go.time_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        // A fileStat is the implementation of FileInfo returned by Stat and Lstat.
        private partial struct fileStat
        {
            public @string name;
            public syscall.Win32FileAttributeData sys;
            public uint filetype; // what syscall.GetFileType returns

// used to implement SameFile
            public ref sync.Mutex Mutex => ref Mutex_val;
            public @string path;
            public uint vol;
            public uint idxhi;
            public uint idxlo;
            public bool appendNameToPath;
        }

        private static long Size(this ref fileStat fs)
        {
            return int64(fs.sys.FileSizeHigh) << (int)(32L) + int64(fs.sys.FileSizeLow);
        }

        private static FileMode Mode(this ref fileStat fs)
        {
            if (fs == ref devNullStat)
            {
                return ModeDevice | ModeCharDevice | 0666L;
            }
            if (fs.sys.FileAttributes & syscall.FILE_ATTRIBUTE_READONLY != 0L)
            {
                m |= 0444L;
            }
            else
            {
                m |= 0666L;
            }
            if (fs.sys.FileAttributes & syscall.FILE_ATTRIBUTE_REPARSE_POINT != 0L)
            {
                return m | ModeSymlink;
            }
            if (fs.sys.FileAttributes & syscall.FILE_ATTRIBUTE_DIRECTORY != 0L)
            {
                m |= ModeDir | 0111L;
            }

            if (fs.filetype == syscall.FILE_TYPE_PIPE) 
                m |= ModeNamedPipe;
            else if (fs.filetype == syscall.FILE_TYPE_CHAR) 
                m |= ModeCharDevice;
                        return m;
        }

        private static time.Time ModTime(this ref fileStat fs)
        {
            return time.Unix(0L, fs.sys.LastWriteTime.Nanoseconds());
        }

        // Sys returns syscall.Win32FileAttributeData for file fs.
        private static void Sys(this ref fileStat fs)
        {
            return ref fs.sys;
        }

        private static error loadFileId(this ref fileStat _fs) => func(_fs, (ref fileStat fs, Defer defer, Panic _, Recover __) =>
        {
            fs.Lock();
            defer(fs.Unlock());
            if (fs.path == "")
            { 
                // already done
                return error.As(null);
            }
            @string path = default;
            if (fs.appendNameToPath)
            {
                path = fs.path + "\\" + fs.name;
            }
            else
            {
                path = fs.path;
            }
            var (pathp, err) = syscall.UTF16PtrFromString(path);
            if (err != null)
            {
                return error.As(err);
            }
            var (h, err) = syscall.CreateFile(pathp, 0L, 0L, null, syscall.OPEN_EXISTING, syscall.FILE_FLAG_BACKUP_SEMANTICS, 0L);
            if (err != null)
            {
                return error.As(err);
            }
            defer(syscall.CloseHandle(h));
            syscall.ByHandleFileInformation i = default;
            err = syscall.GetFileInformationByHandle(h, ref i);
            if (err != null)
            {
                return error.As(err);
            }
            fs.path = "";
            fs.vol = i.VolumeSerialNumber;
            fs.idxhi = i.FileIndexHigh;
            fs.idxlo = i.FileIndexLow;
            return error.As(null);
        });

        // devNullStat is fileStat structure describing DevNull file ("NUL").
        private static fileStat devNullStat = new fileStat(name:DevNull,vol:0,idxhi:0,idxlo:0,);

        private static bool sameFile(ref fileStat fs1, ref fileStat fs2)
        {
            var e = fs1.loadFileId();
            if (e != null)
            {
                return false;
            }
            e = fs2.loadFileId();
            if (e != null)
            {
                return false;
            }
            return fs1.vol == fs2.vol && fs1.idxhi == fs2.idxhi && fs1.idxlo == fs2.idxlo;
        }

        // For testing.
        private static time.Time atime(FileInfo fi)
        {
            return time.Unix(0L, fi.Sys()._<ref syscall.Win32FileAttributeData>().LastAccessTime.Nanoseconds());
        }
    }
}
