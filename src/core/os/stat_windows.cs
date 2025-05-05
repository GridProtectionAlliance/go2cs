// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using filepathlite = @internal.filepathlite_package;
using windows = @internal.syscall.windows_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.syscall;

partial class os_package {

// Stat returns the [FileInfo] structure describing file.
// If there is an error, it will be of type [*PathError].
[GoRecv] public static (FileInfo, error) Stat(this ref File file) {
    if (file == nil) {
        return (default!, ErrInvalid);
    }
    return statHandle(file.name, file.pfd.Sysfd);
}

// stat implements both Stat and Lstat of a file.
internal static (FileInfo, error) stat(@string funcname, @string name, bool followSurrogates) => func((defer, _) => {
    if (len(name) == 0) {
        return (default!, new PathError{Op: funcname, Path: name, Err: ((syscall.Errno)syscall.ERROR_PATH_NOT_FOUND)});
    }
    (namep, err) = syscall.UTF16PtrFromString(fixLongPath(name));
    if (err != default!) {
        return (default!, new PathError{Op: funcname, Path: name, Err: err});
    }
    // Try GetFileAttributesEx first, because it is faster than CreateFile.
    // See https://golang.org/issues/19922#issuecomment-300031421 for details.
    ref var fa = ref heap(new syscall_package.Win32FileAttributeData(), out var Ꮡfa);
    err = syscall.GetFileAttributesEx(namep, syscall.GetFileExInfoStandard, (ж<byte>)(uintptr)(new @unsafe.Pointer(Ꮡfa)));
    if (err == default! && (uint32)(fa.FileAttributes & syscall.FILE_ATTRIBUTE_REPARSE_POINT) == 0) {
        // Not a surrogate for another named entity, because it isn't any kind of reparse point.
        // The information we got from GetFileAttributesEx is good enough for now.
        var fs = newFileStatFromWin32FileAttributeData(Ꮡfa);
        {
            var errΔ1 = fs.saveInfoFromPath(name); if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
        }
        return (~fs, default!);
    }
    // GetFileAttributesEx fails with ERROR_SHARING_VIOLATION error for
    // files like c:\pagefile.sys. Use FindFirstFile for such files.
    if (err == windows.ERROR_SHARING_VIOLATION) {
        ref var fd = ref heap(new syscall_package.Win32finddata(), out var Ꮡfd);
        var (sh, errΔ2) = syscall.FindFirstFile(namep, Ꮡfd);
        if (errΔ2 != default!) {
            return (default!, new PathError{Op: "FindFirstFile"u8, Path: name, Err: errΔ2});
        }
        syscall.FindClose(sh);
        if ((uint32)(fd.FileAttributes & syscall.FILE_ATTRIBUTE_REPARSE_POINT) == 0) {
            // Not a surrogate for another named entity. FindFirstFile is good enough.
            var fs = newFileStatFromWin32finddata(Ꮡfd);
            {
                var errΔ3 = fs.saveInfoFromPath(name); if (errΔ3 != default!) {
                    return (default!, errΔ3);
                }
            }
            return (~fs, default!);
        }
    }
    // Use CreateFile to determine whether the file is a name surrogate and, if so,
    // save information about the link target.
    // Set FILE_FLAG_BACKUP_SEMANTICS so that CreateFile will create the handle
    // even if name refers to a directory.
    uint32 flags = (uint32)(syscall.FILE_FLAG_BACKUP_SEMANTICS | syscall.FILE_FLAG_OPEN_REPARSE_POINT);
    var (h, err) = syscall.CreateFile(namep, 0, 0, nil, syscall.OPEN_EXISTING, flags, 0);
    if (err == windows.ERROR_INVALID_PARAMETER) {
        // Console handles, like "\\.\con", require generic read access. See
        // https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-createfilew#consoles.
        // We haven't set it previously because it is normally not required
        // to read attributes and some files may not allow it.
        (h, err) = syscall.CreateFile(namep, syscall.GENERIC_READ, 0, nil, syscall.OPEN_EXISTING, flags, 0);
    }
    if (err != default!) {
        // Since CreateFile failed, we can't determine whether name refers to a
        // name surrogate, or some other kind of reparse point. Since we can't return a
        // FileInfo with a known-accurate Mode, we must return an error.
        return (default!, new PathError{Op: "CreateFile"u8, Path: name, Err: err});
    }
    (fi, err) = statHandle(name, h);
    syscall.CloseHandle(h);
    if (err == default! && followSurrogates && fi._<fileStat.val>().isReparseTagNameSurrogate()) {
        // To obtain information about the link target, we reopen the file without
        // FILE_FLAG_OPEN_REPARSE_POINT and examine the resulting handle.
        // (See https://devblogs.microsoft.com/oldnewthing/20100212-00/?p=14963.)
        (h, err) = syscall.CreateFile(namep, 0, 0, nil, syscall.OPEN_EXISTING, syscall.FILE_FLAG_BACKUP_SEMANTICS, 0);
        if (err != default!) {
            // name refers to a symlink, but we couldn't resolve the symlink target.
            return (default!, new PathError{Op: "CreateFile"u8, Path: name, Err: err});
        }
        deferǃ(syscall.CloseHandle, h, defer);
        return statHandle(name, h);
    }
    return (fi, err);
});

internal static (FileInfo, error) statHandle(@string name, syscallꓸHandle h) {
    (ft, err) = syscall.GetFileType(h);
    if (err != default!) {
        return (default!, new PathError{Op: "GetFileType"u8, Path: name, Err: err});
    }
    switch (ft) {
    case syscall.FILE_TYPE_PIPE or syscall.FILE_TYPE_CHAR: {
        return (new fileStat(name: filepathlite.Base(name), filetype: ft), default!);
    }}

    (fs, err) = newFileStatFromGetFileInformationByHandle(name, h);
    if (err != default!) {
        return (default!, err);
    }
    fs.val.filetype = ft;
    return (~fs, err);
}

// statNolog implements Stat for Windows.
internal static (FileInfo, error) statNolog(@string name) {
    return stat("Stat"u8, name, true);
}

// lstatNolog implements Lstat for Windows.
internal static (FileInfo, error) lstatNolog(@string name) {
    var followSurrogates = false;
    if (name != ""u8 && IsPathSeparator(name[len(name) - 1])) {
        // We try to implement POSIX semantics for Lstat path resolution
        // (per https://pubs.opengroup.org/onlinepubs/9699919799.2013edition/basedefs/V1_chap04.html#tag_04_12):
        // symlinks before the last separator in the path must be resolved. Since
        // the last separator in this case follows the last path element, we should
        // follow symlinks in the last path element.
        followSurrogates = true;
    }
    return stat("Lstat"u8, name, followSurrogates);
}

} // end os_package
