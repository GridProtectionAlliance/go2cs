// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using filepathlite = @internal.filepathlite_package;
using godebug = @internal.godebug_package;
using poll = @internal.poll_package;
using windows = @internal.syscall.windows_package;
using runtime = runtime_package;
using sync = sync_package;
using atomic = sync.atomic_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.syscall;
using sync;

partial class os_package {

// This matches the value in syscall/syscall_windows.go.
internal static readonly UntypedInt _UTIME_OMIT = -1;

// file is the real representation of *File.
// The extra level of indirection ensures that no clients of os
// can overwrite this data, which could cause the finalizer
// to close the wrong file descriptor.
[GoType] partial struct file {
    internal @internal.poll_package.FD pfd;
    internal @string name;
    internal sync.atomic_package.Pointer dirinfo; // nil unless directory being read
    internal bool appendMode;                    // whether file is opened for appending
}

// Fd returns the Windows handle referencing the open file.
// If f is closed, the file descriptor becomes invalid.
// If f is garbage collected, a finalizer may close the file descriptor,
// making it invalid; see [runtime.SetFinalizer] for more information on when
// a finalizer might be run. On Unix systems this will cause the [File.SetDeadline]
// methods to stop working.
[GoRecv] public static uintptr Fd(this ref File file) {
    if (file == nil) {
        return ((uintptr)syscall.InvalidHandle);
    }
    return ((uintptr)file.pfd.Sysfd);
}

// newFile returns a new File with the given file handle and name.
// Unlike NewFile, it does not check that h is syscall.InvalidHandle.
internal static ж<File> newFile(syscallꓸHandle h, @string name, @string kind) {
    if (kind == "file"u8) {
        ref var m = ref heap(new uint32(), out var Ꮡm);
        if (syscall.GetConsoleMode(h, Ꮡm) == default!) {
            kind = "console"u8;
        }
        {
            var (t, err) = syscall.GetFileType(h); if (err == default! && t == syscall.FILE_TYPE_PIPE) {
                kind = "pipe"u8;
            }
        }
    }
    var f = Ꮡ(new File(Ꮡ(new file(
        pfd: new poll.FD(
            Sysfd: h,
            IsStream: true,
            ZeroReadIsEOF: true
        ),
        name: name
    ))
    ));
    runtime.SetFinalizer((~f).file, (ж<file>).close);
    // Ignore initialization errors.
    // Assume any problems will show up in later I/O.
    f.pfd.Init(kind, false);
    return f;
}

// newConsoleFile creates new File that will be used as console.
internal static ж<File> newConsoleFile(syscallꓸHandle h, @string name) {
    return newFile(h, name, "console"u8);
}

// NewFile returns a new File with the given file descriptor and
// name. The returned value will be nil if fd is not a valid file
// descriptor.
public static ж<File> NewFile(uintptr fd, @string name) {
    var h = ((syscallꓸHandle)fd);
    if (h == syscall.InvalidHandle) {
        return default!;
    }
    return newFile(h, name, "file"u8);
}

internal static void epipecheck(ж<File> Ꮡfile, error e) {
    ref var file = ref Ꮡfile.val;

}

// DevNull is the name of the operating system's “null device.”
// On Unix-like systems, it is "/dev/null"; on Windows, "NUL".
public static readonly @string DevNull = "NUL"u8;

// openFileNolog is the Windows implementation of OpenFile.
internal static (ж<File>, error) openFileNolog(@string name, nint flag, FileMode perm) {
    if (name == ""u8) {
        return (default!, new PathError{Op: "open"u8, Path: name, Err: syscall.ENOENT});
    }
    @string path = fixLongPath(name);
    var (r, e) = syscall.Open(path, (nint)(flag | syscall.O_CLOEXEC), syscallMode(perm));
    if (e != default!) {
        // We should return EISDIR when we are trying to open a directory with write access.
        if (e == syscall.ERROR_ACCESS_DENIED && ((nint)(flag & O_WRONLY) != 0 || (nint)(flag & O_RDWR) != 0)) {
            (pathp, e1) = syscall.UTF16PtrFromString(path);
            if (e1 == default!) {
                ref var fa = ref heap(new syscall_package.Win32FileAttributeData(), out var Ꮡfa);
                e1 = syscall.GetFileAttributesEx(pathp, syscall.GetFileExInfoStandard, (ж<byte>)(uintptr)(new @unsafe.Pointer(Ꮡfa)));
                if (e1 == default! && (uint32)(fa.FileAttributes & syscall.FILE_ATTRIBUTE_DIRECTORY) != 0) {
                    e = syscall.EISDIR;
                }
            }
        }
        return (default!, new PathError{Op: "open"u8, Path: name, Err: e});
    }
    return (newFile(r, name, "file"u8), default!);
}

internal static (ж<File>, error) openDirNolog(@string name) {
    return openFileNolog(name, O_RDONLY, 0);
}

[GoRecv] internal static error close(this ref file file) {
    if (file == nil) {
        return syscall.EINVAL;
    }
    {
        var info = file.dirinfo.Swap(nil); if (info != nil) {
            info.close();
        }
    }
    error err = default!;
    {
        var e = file.pfd.Close(); if (e != default!) {
            if (AreEqual(e, poll.ErrFileClosing)) {
                e = ErrClosed;
            }
            Ꮡerr = new PathError{Op: "close"u8, Path: file.name, Err: e}; err = ref Ꮡerr.val;
        }
    }
    // no need for a finalizer anymore
    runtime.SetFinalizer(file, default!);
    return err;
}

// seek sets the offset for the next Read or Write on file to offset, interpreted
// according to whence: 0 means relative to the origin of the file, 1 means
// relative to the current offset, and 2 means relative to the end.
// It returns the new offset and an error, if any.
[GoRecv] internal static (int64 ret, error err) seek(this ref File f, int64 offset, nint whence) {
    int64 ret = default!;
    error err = default!;

    {
        var info = f.dirinfo.Swap(nil); if (info != nil) {
            // Free cached dirinfo, so we allocate a new one if we
            // access this file as a directory again. See #35767 and #37161.
            info.close();
        }
    }
    (ret, err) = f.pfd.Seek(offset, whence);
    runtime.KeepAlive(f);
    return (ret, err);
}

// Truncate changes the size of the named file.
// If the file is a symbolic link, it changes the size of the link's target.
public static error Truncate(@string name, int64 size) => func((defer, _) => {
    (f, e) = OpenFile(name, O_WRONLY, 438);
    if (e != default!) {
        return e;
    }
    var fʗ1 = f;
    defer(fʗ1.Close);
    var e1 = f.Truncate(size);
    if (e1 != default!) {
        return e1;
    }
    return default!;
});

// Remove removes the named file or directory.
// If there is an error, it will be of type *PathError.
public static error Remove(@string name) {
    (p, e) = syscall.UTF16PtrFromString(fixLongPath(name));
    if (e != default!) {
        return new PathError{Op: "remove"u8, Path: name, Err: e};
    }
    // Go file interface forces us to know whether
    // name is a file or directory. Try both.
    e = syscall.DeleteFile(p);
    if (e == default!) {
        return default!;
    }
    var e1 = syscall.RemoveDirectory(p);
    if (e1 == default!) {
        return default!;
    }
    // Both failed: figure out which error to return.
    if (!AreEqual(e1, e)) {
        var (a, e2) = syscall.GetFileAttributes(p);
        if (e2 != default!){
            e = e2;
        } else {
            if ((uint32)(a & syscall.FILE_ATTRIBUTE_DIRECTORY) != 0){
                e = e1;
            } else 
            if ((uint32)(a & syscall.FILE_ATTRIBUTE_READONLY) != 0) {
                {
                    e1 = syscall.SetFileAttributes(p, (uint32)(a & ~syscall.FILE_ATTRIBUTE_READONLY)); if (e1 == default!) {
                        {
                            e = syscall.DeleteFile(p); if (e == default!) {
                                return default!;
                            }
                        }
                    }
                }
            }
        }
    }
    return new PathError{Op: "remove"u8, Path: name, Err: e};
}

internal static error rename(@string oldname, @string newname) {
    var e = windows.Rename(fixLongPath(oldname), fixLongPath(newname));
    if (e != default!) {
        return new LinkError("rename", oldname, newname, e);
    }
    return default!;
}

// Pipe returns a connected pair of Files; reads from r return bytes written to w.
// It returns the files and an error, if any. The Windows handles underlying
// the returned files are marked as inheritable by child processes.
public static (ж<File> r, ж<File> w, error err) Pipe() {
    ж<File> r = default!;
    ж<File> w = default!;
    error err = default!;

    array<syscallꓸHandle> p = new(2);
    var e = syscall.Pipe(p[..]);
    if (e != default!) {
        return (default!, default!, NewSyscallError("pipe"u8, e));
    }
    return (newFile(p[0], "|0"u8, "pipe"u8), newFile(p[1], "|1"u8, "pipe"u8), default!);
}

internal static sync.Once useGetTempPath2Once;
internal static bool useGetTempPath2;

internal static @string tempDir() {
    useGetTempPath2Once.Do(() => {
        useGetTempPath2 = (windows.ErrorLoadingGetTempPath2() == default!);
    });
    var getTempPath = syscall.GetTempPath;
    if (useGetTempPath2) {
        getTempPath = windows.GetTempPath2;
    }
    var n = ((uint32)syscall.MAX_PATH);
    while (ᐧ) {
        var b = new slice<uint16>(n);
        (n, _) = getTempPath(((uint32)len(b)), Ꮡ(b, 0));
        if (n > ((uint32)len(b))) {
            continue;
        }
        if (n == 3 && b[1] == (rune)':' && b[2] == (rune)'\\'){
        } else 
        if (n > 0 && b[n - 1] == (rune)'\\') {
            // Do nothing for path, like C:\.
            // Otherwise remove terminating \.
            n--;
        }
        return syscall.UTF16ToString(b[..(int)(n)]);
    }
}

// Link creates newname as a hard link to the oldname file.
// If there is an error, it will be of type *LinkError.
public static error Link(@string oldname, @string newname) {
    (n, err) = syscall.UTF16PtrFromString(fixLongPath(newname));
    if (err != default!) {
        return new LinkError("link", oldname, newname, err);
    }
    (o, err) = syscall.UTF16PtrFromString(fixLongPath(oldname));
    if (err != default!) {
        return new LinkError("link", oldname, newname, err);
    }
    err = syscall.CreateHardLink(n, o, 0);
    if (err != default!) {
        return new LinkError("link", oldname, newname, err);
    }
    return default!;
}

// Symlink creates newname as a symbolic link to oldname.
// On Windows, a symlink to a non-existent oldname creates a file symlink;
// if oldname is later created as a directory the symlink will not work.
// If there is an error, it will be of type *LinkError.
public static error Symlink(@string oldname, @string newname) {
    // '/' does not work in link's content
    oldname = filepathlite.FromSlash(oldname);
    // need the exact location of the oldname when it's relative to determine if it's a directory
    @string destpath = oldname;
    {
        @string v = filepathlite.VolumeName(oldname); if (v == ""u8) {
            if (len(oldname) > 0 && IsPathSeparator(oldname[0])){
                // oldname is relative to the volume containing newname.
                {
                    v = filepathlite.VolumeName(newname); if (v != ""u8) {
                        // Prepend the volume explicitly, because it may be different from the
                        // volume of the current working directory.
                        destpath = v + oldname;
                    }
                }
            } else {
                // oldname is relative to newname.
                destpath = dirname(newname) + @"\"u8 + oldname;
            }
        }
    }
    (fi, err) = Stat(destpath);
    var isdir = err == default! && fi.IsDir();
    (n, err) = syscall.UTF16PtrFromString(fixLongPath(newname));
    if (err != default!) {
        return new LinkError("symlink", oldname, newname, err);
    }
    ж<uint16> o = default!;
    if (filepathlite.IsAbs(oldname)){
        (o, err) = syscall.UTF16PtrFromString(fixLongPath(oldname));
    } else {
        // Do not use fixLongPath on oldname for relative symlinks,
        // as it would turn the name into an absolute path thus making
        // an absolute symlink instead.
        // Notice that CreateSymbolicLinkW does not fail for relative
        // symlinks beyond MAX_PATH, so this does not prevent the
        // creation of an arbitrary long path name.
        (o, err) = syscall.UTF16PtrFromString(oldname);
    }
    if (err != default!) {
        return new LinkError("symlink", oldname, newname, err);
    }
    uint32 flags = windows.SYMBOLIC_LINK_FLAG_ALLOW_UNPRIVILEGED_CREATE;
    if (isdir) {
        flags |= (uint32)(syscall.SYMBOLIC_LINK_FLAG_DIRECTORY);
    }
    err = syscall.CreateSymbolicLink(n, o, flags);
    if (err != default!) {
        // the unprivileged create flag is unsupported
        // below Windows 10 (1703, v10.0.14972). retry without it.
        flags &= ~(uint32)(windows.SYMBOLIC_LINK_FLAG_ALLOW_UNPRIVILEGED_CREATE);
        err = syscall.CreateSymbolicLink(n, o, flags);
        if (err != default!) {
            return new LinkError("symlink", oldname, newname, err);
        }
    }
    return default!;
}

// openSymlink calls CreateFile Windows API with FILE_FLAG_OPEN_REPARSE_POINT
// parameter, so that Windows does not follow symlink, if path is a symlink.
// openSymlink returns opened file handle.
internal static (syscallꓸHandle, error) openSymlink(@string path) {
    (p, err) = syscall.UTF16PtrFromString(path);
    if (err != default!) {
        return (0, err);
    }
    var attrs = ((uint32)syscall.FILE_FLAG_BACKUP_SEMANTICS);
    // Use FILE_FLAG_OPEN_REPARSE_POINT, otherwise CreateFile will follow symlink.
    // See https://docs.microsoft.com/en-us/windows/desktop/FileIO/symbolic-link-effects-on-file-systems-functions#createfile-and-createfiletransacted
    attrs |= (uint32)(syscall.FILE_FLAG_OPEN_REPARSE_POINT);
    var (h, err) = syscall.CreateFile(p, 0, 0, nil, syscall.OPEN_EXISTING, attrs, 0);
    if (err != default!) {
        return (0, err);
    }
    return (h, default!);
}

internal static ж<godebug.Setting> winreadlinkvolume = godebug.New("winreadlinkvolume"u8);

// normaliseLinkPath converts absolute paths returned by
// DeviceIoControl(h, FSCTL_GET_REPARSE_POINT, ...)
// into paths acceptable by all Windows APIs.
// For example, it converts
//
//	\??\C:\foo\bar into C:\foo\bar
//	\??\UNC\foo\bar into \\foo\bar
//	\??\Volume{abc}\ into \\?\Volume{abc}\
internal static (@string, error) normaliseLinkPath(@string path) => func((defer, _) => {
    if (len(path) < 4 || path[..4] != @"\??\") {
        // unexpected path, return it as is
        return (path, default!);
    }
    // we have path that start with \??\
    @string s = path[4..];
    switch (ᐧ) {
    case {} when len(s) >= 2 && s[1] == (rune)':': {
        return (s, default!);
    }
    case {} when len(s) >= 4 && s[..4] == @"UNC\": {
        return (@"\\" + s[4..], default!);
    }}

    // \??\C:\foo\bar
    // \??\UNC\foo\bar
    // \??\Volume{abc}\
    if (winreadlinkvolume.Value() != "0"u8) {
        return (@"\\?\" + path[4..], default!);
    }
    winreadlinkvolume.IncNonDefault();
    var (h, err) = openSymlink(path);
    if (err != default!) {
        return ("", err);
    }
    deferǃ(syscall.CloseHandle, h, defer);
    var buf = new slice<uint16>(100);
    while (ᐧ) {
        var (n, errΔ1) = windows.GetFinalPathNameByHandle(h, Ꮡ(buf, 0), ((uint32)len(buf)), windows.VOLUME_NAME_DOS);
        if (errΔ1 != default!) {
            return ("", errΔ1);
        }
        if (n < ((uint32)len(buf))) {
            break;
        }
        buf = new slice<uint16>(n);
    }
    s = syscall.UTF16ToString(buf);
    if (len(s) > 4 && s[..4] == @"\\?\") {
        s = s[4..];
        if (len(s) > 3 && s[..3] == @"UNC") {
            // return path like \\server\share\...
            return (@"\" + s[3..], default!);
        }
        return (s, default!);
    }
    return ("", errors.New("GetFinalPathNameByHandle returned unexpected path: "u8 + s));
});

internal static (@string, error) readReparseLink(@string path) => func((defer, _) => {
    var (h, err) = openSymlink(path);
    if (err != default!) {
        return ("", err);
    }
    deferǃ(syscall.CloseHandle, h, defer);
    var rdbbuf = new slice<byte>(syscall.MAXIMUM_REPARSE_DATA_BUFFER_SIZE);
    ref var bytesReturned = ref heap(new uint32(), out var ᏑbytesReturned);
    err = syscall.DeviceIoControl(h, syscall.FSCTL_GET_REPARSE_POINT, nil, 0, Ꮡ(rdbbuf, 0), ((uint32)len(rdbbuf)), ᏑbytesReturned, nil);
    if (err != default!) {
        return ("", err);
    }
    var rdb = (ж<windows.REPARSE_DATA_BUFFER>)(uintptr)(new @unsafe.Pointer(Ꮡ(rdbbuf, 0)));
    switch ((~rdb).ReparseTag) {
    case syscall.IO_REPARSE_TAG_SYMLINK: {
        var rb = (ж<windows.SymbolicLinkReparseBuffer>)(uintptr)(new @unsafe.Pointer(Ꮡ((~rdb).DUMMYUNIONNAME)));
        @string s = rb.Path();
        if ((uint32)((~rb).Flags & windows.SYMLINK_FLAG_RELATIVE) != 0) {
            return (s, default!);
        }
        return normaliseLinkPath(s);
    }
    case windows.IO_REPARSE_TAG_MOUNT_POINT: {
        return normaliseLinkPath(((ж<windows.MountPointReparseBuffer>)(uintptr)(new @unsafe.Pointer(Ꮡ((~rdb).DUMMYUNIONNAME)))).val.Path());
    }
    default: {
        return ("", syscall.ENOENT);
    }}

});

// the path is not a symlink or junction but another type of reparse
// point
internal static (@string, error) readlink(@string name) {
    var (s, err) = readReparseLink(fixLongPath(name));
    if (err != default!) {
        return ("", new PathError{Op: "readlink"u8, Path: name, Err: err});
    }
    return (s, default!);
}

} // end os_package
