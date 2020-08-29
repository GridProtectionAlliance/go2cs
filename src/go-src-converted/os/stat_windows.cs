// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 August 29 08:44:28 UTC
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
        private static (FileInfo, error) Stat(this ref File file)
        {
            if (file == null)
            {
                return (null, ErrInvalid);
            }
            if (file.isdir())
            { 
                // I don't know any better way to do that for directory
                return Stat(file.dirinfo.path);
            }
            if (file.name == DevNull)
            {
                return (ref devNullStat, null);
            }
            var (ft, err) = file.pfd.GetFileType();
            if (err != null)
            {
                return (null, ref new PathError("GetFileType",file.name,err));
            }

            if (ft == syscall.FILE_TYPE_PIPE || ft == syscall.FILE_TYPE_CHAR) 
                return (ref new fileStat(name:basename(file.name),filetype:ft), null);
                        syscall.ByHandleFileInformation d = default;
            err = file.pfd.GetFileInformationByHandle(ref d);
            if (err != null)
            {
                return (null, ref new PathError("GetFileInformationByHandle",file.name,err));
            }
            return (ref new fileStat(name:basename(file.name),sys:syscall.Win32FileAttributeData{FileAttributes:d.FileAttributes,CreationTime:d.CreationTime,LastAccessTime:d.LastAccessTime,LastWriteTime:d.LastWriteTime,FileSizeHigh:d.FileSizeHigh,FileSizeLow:d.FileSizeLow,},filetype:ft,vol:d.VolumeSerialNumber,idxhi:d.FileIndexHigh,idxlo:d.FileIndexLow,), null);
        }

        // statNolog implements Stat for Windows.
        private static (FileInfo, error) statNolog(@string name) => func((defer, _, __) =>
        {
            if (len(name) == 0L)
            {
                return (null, ref new PathError("Stat",name,syscall.Errno(syscall.ERROR_PATH_NOT_FOUND)));
            }
            if (name == DevNull)
            {
                return (ref devNullStat, null);
            }
            var (namep, err) = syscall.UTF16PtrFromString(fixLongPath(name));
            if (err != null)
            {
                return (null, ref new PathError("Stat",name,err));
            } 
            // Apparently (see https://golang.org/issues/19922#issuecomment-300031421)
            // GetFileAttributesEx is fastest approach to get file info.
            // It does not work for symlinks. But symlinks are rare,
            // so try GetFileAttributesEx first.
            fileStat fs = default;
            err = syscall.GetFileAttributesEx(namep, syscall.GetFileExInfoStandard, (byte.Value)(@unsafe.Pointer(ref fs.sys)));
            if (err == null && fs.sys.FileAttributes & syscall.FILE_ATTRIBUTE_REPARSE_POINT == 0L)
            {
                fs.path = name;
                if (!isAbs(fs.path))
                {
                    fs.path, err = syscall.FullPath(fs.path);
                    if (err != null)
                    {
                        return (null, ref new PathError("FullPath",name,err));
                    }
                }
                fs.name = basename(name);
                return (ref fs, null);
            } 
            // Use Windows I/O manager to dereference the symbolic link, as per
            // https://blogs.msdn.microsoft.com/oldnewthing/20100212-00/?p=14963/
            var (h, err) = syscall.CreateFile(namep, 0L, 0L, null, syscall.OPEN_EXISTING, syscall.FILE_FLAG_BACKUP_SEMANTICS, 0L);
            if (err != null)
            {
                if (err == windows.ERROR_SHARING_VIOLATION)
                { 
                    // try FindFirstFile now that CreateFile failed
                    return statWithFindFirstFile(name, namep);
                }
                return (null, ref new PathError("CreateFile",name,err));
            }
            defer(syscall.CloseHandle(h));

            syscall.ByHandleFileInformation d = default;
            err = syscall.GetFileInformationByHandle(h, ref d);
            if (err != null)
            {
                return (null, ref new PathError("GetFileInformationByHandle",name,err));
            }
            return (ref new fileStat(name:basename(name),sys:syscall.Win32FileAttributeData{FileAttributes:d.FileAttributes,CreationTime:d.CreationTime,LastAccessTime:d.LastAccessTime,LastWriteTime:d.LastWriteTime,FileSizeHigh:d.FileSizeHigh,FileSizeLow:d.FileSizeLow,},vol:d.VolumeSerialNumber,idxhi:d.FileIndexHigh,idxlo:d.FileIndexLow,), null);
        });

        // statWithFindFirstFile is used by Stat to handle special case of statting
        // c:\pagefile.sys. We might discover that other files need similar treatment.
        private static (FileInfo, error) statWithFindFirstFile(@string name, ref ushort namep)
        {
            syscall.Win32finddata fd = default;
            var (h, err) = syscall.FindFirstFile(namep, ref fd);
            if (err != null)
            {
                return (null, ref new PathError("FindFirstFile",name,err));
            }
            syscall.FindClose(h);

            var fullpath = name;
            if (!isAbs(fullpath))
            {
                fullpath, err = syscall.FullPath(fullpath);
                if (err != null)
                {
                    return (null, ref new PathError("FullPath",name,err));
                }
            }
            return (ref new fileStat(name:basename(name),path:fullpath,sys:syscall.Win32FileAttributeData{FileAttributes:fd.FileAttributes,CreationTime:fd.CreationTime,LastAccessTime:fd.LastAccessTime,LastWriteTime:fd.LastWriteTime,FileSizeHigh:fd.FileSizeHigh,FileSizeLow:fd.FileSizeLow,},), null);
        }

        // lstatNolog implements Lstat for Windows.
        private static (FileInfo, error) lstatNolog(@string name)
        {
            if (len(name) == 0L)
            {
                return (null, ref new PathError("Lstat",name,syscall.Errno(syscall.ERROR_PATH_NOT_FOUND)));
            }
            if (name == DevNull)
            {
                return (ref devNullStat, null);
            }
            fileStat fs = ref new fileStat(name:basename(name));
            var (namep, e) = syscall.UTF16PtrFromString(fixLongPath(name));
            if (e != null)
            {
                return (null, ref new PathError("Lstat",name,e));
            }
            e = syscall.GetFileAttributesEx(namep, syscall.GetFileExInfoStandard, (byte.Value)(@unsafe.Pointer(ref fs.sys)));
            if (e != null)
            {
                if (e != windows.ERROR_SHARING_VIOLATION)
                {
                    return (null, ref new PathError("GetFileAttributesEx",name,e));
                } 
                // try FindFirstFile now that GetFileAttributesEx failed
                return statWithFindFirstFile(name, namep);
            }
            fs.path = name;
            if (!isAbs(fs.path))
            {
                fs.path, e = syscall.FullPath(fs.path);
                if (e != null)
                {
                    return (null, ref new PathError("FullPath",name,e));
                }
            }
            return (fs, null);
        }
    }
}
