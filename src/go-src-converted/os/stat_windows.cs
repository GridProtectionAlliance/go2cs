// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 October 09 05:07:27 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\stat_windows.go
using windows = go.@internal.syscall.windows_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        // Stat returns the FileInfo structure describing file.
        // If there is an error, it will be of type *PathError.
        private static (FileInfo, error) Stat(this ptr<File> _addr_file)
        {
            FileInfo _p0 = default;
            error _p0 = default!;
            ref File file = ref _addr_file.val;

            if (file == null)
            {
                return (null, error.As(ErrInvalid)!);
            }
            if (file.isdir())
            { 
                // I don't know any better way to do that for directory
                return Stat(file.dirinfo.path);

            }
            if (isWindowsNulName(file.name))
            {
                return (_addr_devNullStat, error.As(null!)!);
            }
            var (ft, err) = file.pfd.GetFileType();
            if (err != null)
            {
                return (null, error.As(addr(new PathError("GetFileType",file.name,err))!)!);
            }

            if (ft == syscall.FILE_TYPE_PIPE || ft == syscall.FILE_TYPE_CHAR) 
                return (addr(new fileStat(name:basename(file.name),filetype:ft)), error.As(null!)!);
                        var (fs, err) = newFileStatFromGetFileInformationByHandle(file.name, file.pfd.Sysfd);
            if (err != null)
            {
                return (null, error.As(err)!);
            }
            fs.filetype = ft;
            return (fs, error.As(err)!);

        }

        // stat implements both Stat and Lstat of a file.
        private static (FileInfo, error) stat(@string funcname, @string name, uint createFileAttrs) => func((defer, _, __) =>
        {
            FileInfo _p0 = default;
            error _p0 = default!;

            if (len(name) == 0L)
            {
                return (null, error.As(addr(new PathError(funcname,name,syscall.Errno(syscall.ERROR_PATH_NOT_FOUND)))!)!);
            }

            if (isWindowsNulName(name))
            {
                return (_addr_devNullStat, error.As(null!)!);
            }

            var (namep, err) = syscall.UTF16PtrFromString(fixLongPath(name));
            if (err != null)
            {
                return (null, error.As(addr(new PathError(funcname,name,err))!)!);
            } 

            // Try GetFileAttributesEx first, because it is faster than CreateFile.
            // See https://golang.org/issues/19922#issuecomment-300031421 for details.
            ref syscall.Win32FileAttributeData fa = ref heap(out ptr<syscall.Win32FileAttributeData> _addr_fa);
            err = syscall.GetFileAttributesEx(namep, syscall.GetFileExInfoStandard, (byte.val)(@unsafe.Pointer(_addr_fa)));
            if (err == null && fa.FileAttributes & syscall.FILE_ATTRIBUTE_REPARSE_POINT == 0L)
            { 
                // Not a symlink.
                ptr<fileStat> fs = addr(new fileStat(FileAttributes:fa.FileAttributes,CreationTime:fa.CreationTime,LastAccessTime:fa.LastAccessTime,LastWriteTime:fa.LastWriteTime,FileSizeHigh:fa.FileSizeHigh,FileSizeLow:fa.FileSizeLow,));
                {
                    var err__prev2 = err;

                    var err = fs.saveInfoFromPath(name);

                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    }

                    err = err__prev2;

                }

                return (fs, error.As(null!)!);

            } 
            // GetFileAttributesEx fails with ERROR_SHARING_VIOLATION error for
            // files, like c:\pagefile.sys. Use FindFirstFile for such files.
            if (err == windows.ERROR_SHARING_VIOLATION)
            {
                ref syscall.Win32finddata fd = ref heap(out ptr<syscall.Win32finddata> _addr_fd);
                var (sh, err) = syscall.FindFirstFile(namep, _addr_fd);
                if (err != null)
                {
                    return (null, error.As(addr(new PathError("FindFirstFile",name,err))!)!);
                }

                syscall.FindClose(sh);
                fs = newFileStatFromWin32finddata(_addr_fd);
                {
                    var err__prev2 = err;

                    err = fs.saveInfoFromPath(name);

                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    }

                    err = err__prev2;

                }

                return (fs, error.As(null!)!);

            } 

            // Finally use CreateFile.
            var (h, err) = syscall.CreateFile(namep, 0L, 0L, null, syscall.OPEN_EXISTING, createFileAttrs, 0L);
            if (err != null)
            {
                return (null, error.As(addr(new PathError("CreateFile",name,err))!)!);
            }

            defer(syscall.CloseHandle(h));

            return newFileStatFromGetFileInformationByHandle(name, h);

        });

        // statNolog implements Stat for Windows.
        private static (FileInfo, error) statNolog(@string name)
        {
            FileInfo _p0 = default;
            error _p0 = default!;

            return stat("Stat", name, syscall.FILE_FLAG_BACKUP_SEMANTICS);
        }

        // lstatNolog implements Lstat for Windows.
        private static (FileInfo, error) lstatNolog(@string name)
        {
            FileInfo _p0 = default;
            error _p0 = default!;

            var attrs = uint32(syscall.FILE_FLAG_BACKUP_SEMANTICS); 
            // Use FILE_FLAG_OPEN_REPARSE_POINT, otherwise CreateFile will follow symlink.
            // See https://docs.microsoft.com/en-us/windows/desktop/FileIO/symbolic-link-effects-on-file-systems-functions#createfile-and-createfiletransacted
            attrs |= syscall.FILE_FLAG_OPEN_REPARSE_POINT;
            return stat("Lstat", name, attrs);

        }
    }
}
