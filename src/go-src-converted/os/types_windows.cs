// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 October 08 03:45:25 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\types_windows.go
using windows = go.@internal.syscall.windows_package;
using sync = go.sync_package;
using syscall = go.syscall_package;
using time = go.time_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        // A fileStat is the implementation of FileInfo returned by Stat and Lstat.
        private partial struct fileStat
        {
            public @string name; // from ByHandleFileInformation, Win32FileAttributeData and Win32finddata
            public uint FileAttributes;
            public syscall.Filetime CreationTime;
            public syscall.Filetime LastAccessTime;
            public syscall.Filetime LastWriteTime;
            public uint FileSizeHigh;
            public uint FileSizeLow; // from Win32finddata
            public uint Reserved0; // what syscall.GetFileType returns
            public uint filetype; // used to implement SameFile
            public ref sync.Mutex Mutex => ref Mutex_val;
            public @string path;
            public uint vol;
            public uint idxhi;
            public uint idxlo;
            public bool appendNameToPath;
        }

        // newFileStatFromGetFileInformationByHandle calls GetFileInformationByHandle
        // to gather all required information about the file handle h.
        private static (ptr<fileStat>, error) newFileStatFromGetFileInformationByHandle(@string path, syscall.Handle h)
        {
            ptr<fileStat> fs = default!;
            error err = default!;

            ref syscall.ByHandleFileInformation d = ref heap(out ptr<syscall.ByHandleFileInformation> _addr_d);
            err = syscall.GetFileInformationByHandle(h, _addr_d);
            if (err != null)
            {
                return (_addr_null!, error.As(addr(new PathError("GetFileInformationByHandle",path,err))!)!);
            }

            ref windows.FILE_ATTRIBUTE_TAG_INFO ti = ref heap(out ptr<windows.FILE_ATTRIBUTE_TAG_INFO> _addr_ti);
            err = windows.GetFileInformationByHandleEx(h, windows.FileAttributeTagInfo, (byte.val)(@unsafe.Pointer(_addr_ti)), uint32(@unsafe.Sizeof(ti)));
            if (err != null)
            {
                {
                    syscall.Errno (errno, ok) = err._<syscall.Errno>();

                    if (ok && errno == windows.ERROR_INVALID_PARAMETER)
                    { 
                        // It appears calling GetFileInformationByHandleEx with
                        // FILE_ATTRIBUTE_TAG_INFO fails on FAT file system with
                        // ERROR_INVALID_PARAMETER. Clear ti.ReparseTag in that
                        // instance to indicate no symlinks are possible.
                        ti.ReparseTag = 0L;

                    }
                    else
                    {
                        return (_addr_null!, error.As(addr(new PathError("GetFileInformationByHandleEx",path,err))!)!);
                    }

                }

            }

            return (addr(new fileStat(name:basename(path),FileAttributes:d.FileAttributes,CreationTime:d.CreationTime,LastAccessTime:d.LastAccessTime,LastWriteTime:d.LastWriteTime,FileSizeHigh:d.FileSizeHigh,FileSizeLow:d.FileSizeLow,vol:d.VolumeSerialNumber,idxhi:d.FileIndexHigh,idxlo:d.FileIndexLow,Reserved0:ti.ReparseTag,)), error.As(null!)!);

        }

        // newFileStatFromWin32finddata copies all required information
        // from syscall.Win32finddata d into the newly created fileStat.
        private static ptr<fileStat> newFileStatFromWin32finddata(ptr<syscall.Win32finddata> _addr_d)
        {
            ref syscall.Win32finddata d = ref _addr_d.val;

            return addr(new fileStat(FileAttributes:d.FileAttributes,CreationTime:d.CreationTime,LastAccessTime:d.LastAccessTime,LastWriteTime:d.LastWriteTime,FileSizeHigh:d.FileSizeHigh,FileSizeLow:d.FileSizeLow,Reserved0:d.Reserved0,));
        }

        private static bool isSymlink(this ptr<fileStat> _addr_fs)
        {
            ref fileStat fs = ref _addr_fs.val;
 
            // Use instructions described at
            // https://blogs.msdn.microsoft.com/oldnewthing/20100212-00/?p=14963/
            // to recognize whether it's a symlink.
            if (fs.FileAttributes & syscall.FILE_ATTRIBUTE_REPARSE_POINT == 0L)
            {
                return false;
            }

            return fs.Reserved0 == syscall.IO_REPARSE_TAG_SYMLINK || fs.Reserved0 == windows.IO_REPARSE_TAG_MOUNT_POINT;

        }

        private static long Size(this ptr<fileStat> _addr_fs)
        {
            ref fileStat fs = ref _addr_fs.val;

            return int64(fs.FileSizeHigh) << (int)(32L) + int64(fs.FileSizeLow);
        }

        private static FileMode Mode(this ptr<fileStat> _addr_fs)
        {
            FileMode m = default;
            ref fileStat fs = ref _addr_fs.val;

            if (fs == _addr_devNullStat)
            {
                return ModeDevice | ModeCharDevice | 0666L;
            }

            if (fs.FileAttributes & syscall.FILE_ATTRIBUTE_READONLY != 0L)
            {
                m |= 0444L;
            }
            else
            {
                m |= 0666L;
            }

            if (fs.isSymlink())
            {
                return m | ModeSymlink;
            }

            if (fs.FileAttributes & syscall.FILE_ATTRIBUTE_DIRECTORY != 0L)
            {
                m |= ModeDir | 0111L;
            }


            if (fs.filetype == syscall.FILE_TYPE_PIPE) 
                m |= ModeNamedPipe;
            else if (fs.filetype == syscall.FILE_TYPE_CHAR) 
                m |= ModeDevice | ModeCharDevice;
                        return m;

        }

        private static time.Time ModTime(this ptr<fileStat> _addr_fs)
        {
            ref fileStat fs = ref _addr_fs.val;

            return time.Unix(0L, fs.LastWriteTime.Nanoseconds());
        }

        // Sys returns syscall.Win32FileAttributeData for file fs.
        private static void Sys(this ptr<fileStat> _addr_fs)
        {
            ref fileStat fs = ref _addr_fs.val;

            return addr(new syscall.Win32FileAttributeData(FileAttributes:fs.FileAttributes,CreationTime:fs.CreationTime,LastAccessTime:fs.LastAccessTime,LastWriteTime:fs.LastWriteTime,FileSizeHigh:fs.FileSizeHigh,FileSizeLow:fs.FileSizeLow,));
        }

        private static error loadFileId(this ptr<fileStat> _addr_fs) => func((defer, _, __) =>
        {
            ref fileStat fs = ref _addr_fs.val;

            fs.Lock();
            defer(fs.Unlock());
            if (fs.path == "")
            { 
                // already done
                return error.As(null!)!;

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
                return error.As(err)!;
            }

            var attrs = uint32(syscall.FILE_FLAG_BACKUP_SEMANTICS);
            if (fs.isSymlink())
            { 
                // Use FILE_FLAG_OPEN_REPARSE_POINT, otherwise CreateFile will follow symlink.
                // See https://docs.microsoft.com/en-us/windows/desktop/FileIO/symbolic-link-effects-on-file-systems-functions#createfile-and-createfiletransacted
                attrs |= syscall.FILE_FLAG_OPEN_REPARSE_POINT;

            }

            var (h, err) = syscall.CreateFile(pathp, 0L, 0L, null, syscall.OPEN_EXISTING, attrs, 0L);
            if (err != null)
            {
                return error.As(err)!;
            }

            defer(syscall.CloseHandle(h));
            ref syscall.ByHandleFileInformation i = ref heap(out ptr<syscall.ByHandleFileInformation> _addr_i);
            err = syscall.GetFileInformationByHandle(h, _addr_i);
            if (err != null)
            {
                return error.As(err)!;
            }

            fs.path = "";
            fs.vol = i.VolumeSerialNumber;
            fs.idxhi = i.FileIndexHigh;
            fs.idxlo = i.FileIndexLow;
            return error.As(null!)!;

        });

        // saveInfoFromPath saves full path of the file to be used by os.SameFile later,
        // and set name from path.
        private static error saveInfoFromPath(this ptr<fileStat> _addr_fs, @string path)
        {
            ref fileStat fs = ref _addr_fs.val;

            fs.path = path;
            if (!isAbs(fs.path))
            {
                error err = default!;
                fs.path, err = syscall.FullPath(fs.path);
                if (err != null)
                {
                    return error.As(addr(new PathError("FullPath",path,err))!)!;
                }

            }

            fs.name = basename(path);
            return error.As(null!)!;

        }

        // devNullStat is fileStat structure describing DevNull file ("NUL").
        private static fileStat devNullStat = new fileStat(name:DevNull,vol:0,idxhi:0,idxlo:0,);

        private static bool sameFile(ptr<fileStat> _addr_fs1, ptr<fileStat> _addr_fs2)
        {
            ref fileStat fs1 = ref _addr_fs1.val;
            ref fileStat fs2 = ref _addr_fs2.val;

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
            return time.Unix(0L, fi.Sys()._<ptr<syscall.Win32FileAttributeData>>().LastAccessTime.Nanoseconds());
        }
    }
}
