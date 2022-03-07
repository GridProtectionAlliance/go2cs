// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2022 March 06 22:13:41 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\file_windows.go
using errors = go.errors_package;
using poll = go.@internal.poll_package;
using windows = go.@internal.syscall.windows_package;
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using utf16 = go.unicode.utf16_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class os_package {

    // file is the real representation of *File.
    // The extra level of indirection ensures that no clients of os
    // can overwrite this data, which could cause the finalizer
    // to close the wrong file descriptor.
private partial struct file {
    public poll.FD pfd;
    public @string name;
    public ptr<dirInfo> dirinfo; // nil unless directory being read
    public bool appendMode; // whether file is opened for appending
}

// Fd returns the Windows handle referencing the open file.
// If f is closed, the file descriptor becomes invalid.
// If f is garbage collected, a finalizer may close the file descriptor,
// making it invalid; see runtime.SetFinalizer for more information on when
// a finalizer might be run. On Unix systems this will cause the SetDeadline
// methods to stop working.
private static System.UIntPtr Fd(this ptr<File> _addr_file) {
    ref File file = ref _addr_file.val;

    if (file == null) {
        return uintptr(syscall.InvalidHandle);
    }
    return uintptr(file.pfd.Sysfd);

}

// newFile returns a new File with the given file handle and name.
// Unlike NewFile, it does not check that h is syscall.InvalidHandle.
private static ptr<File> newFile(syscall.Handle h, @string name, @string kind) {
    if (kind == "file") {
        ref uint m = ref heap(out ptr<uint> _addr_m);
        if (syscall.GetConsoleMode(h, _addr_m) == null) {
            kind = "console";
        }
        {
            var (t, err) = syscall.GetFileType(h);

            if (err == null && t == syscall.FILE_TYPE_PIPE) {
                kind = "pipe";
            }

        }

    }
    ptr<File> f = addr(new File(&file{pfd:poll.FD{Sysfd:h,IsStream:true,ZeroReadIsEOF:true,},name:name,}));
    runtime.SetFinalizer(f.file, (file.val).close); 

    // Ignore initialization errors.
    // Assume any problems will show up in later I/O.
    f.pfd.Init(kind, false);

    return _addr_f!;

}

// newConsoleFile creates new File that will be used as console.
private static ptr<File> newConsoleFile(syscall.Handle h, @string name) {
    return _addr_newFile(h, name, "console")!;
}

// NewFile returns a new File with the given file descriptor and
// name. The returned value will be nil if fd is not a valid file
// descriptor.
public static ptr<File> NewFile(System.UIntPtr fd, @string name) {
    var h = syscall.Handle(fd);
    if (h == syscall.InvalidHandle) {
        return _addr_null!;
    }
    return _addr_newFile(h, name, "file")!;

}

// Auxiliary information if the File describes a directory
private partial struct dirInfo {
    public syscall.Win32finddata data;
    public bool needdata;
    public @string path;
    public bool isempty; // set if FindFirstFile returns ERROR_FILE_NOT_FOUND
}

private static void epipecheck(ptr<File> _addr_file, error e) {
    ref File file = ref _addr_file.val;

}

// DevNull is the name of the operating system's ``null device.''
// On Unix-like systems, it is "/dev/null"; on Windows, "NUL".
public static readonly @string DevNull = "NUL";



private static bool isdir(this ptr<file> _addr_f) {
    ref file f = ref _addr_f.val;

    return f != null && f.dirinfo != null;
}

private static (ptr<File>, error) openFile(@string name, nint flag, FileMode perm) {
    ptr<File> file = default!;
    error err = default!;

    var (r, e) = syscall.Open(fixLongPath(name), flag | syscall.O_CLOEXEC, syscallMode(perm));
    if (e != null) {
        return (_addr_null!, error.As(e)!);
    }
    return (_addr_newFile(r, name, "file")!, error.As(null!)!);

}

private static (ptr<File>, error) openDir(@string name) {
    ptr<File> file = default!;
    error err = default!;

    @string mask = default;

    var path = fixLongPath(name);

    if (len(path) == 2 && path[1] == ':') { // it is a drive letter, like C:
        mask = path + "*";

    }
    else if (len(path) > 0) {
        var lc = path[len(path) - 1];
        if (lc == '/' || lc == '\\') {
            mask = path + "*";
        }
        else
 {
            mask = path + "\\*";
        }
    }
    else
 {
        mask = "\\*";
    }
    var (maskp, e) = syscall.UTF16PtrFromString(mask);
    if (e != null) {
        return (_addr_null!, error.As(e)!);
    }
    ptr<dirInfo> d = @new<dirInfo>();
    var (r, e) = syscall.FindFirstFile(maskp, _addr_d.data);
    if (e != null) { 
        // FindFirstFile returns ERROR_FILE_NOT_FOUND when
        // no matching files can be found. Then, if directory
        // exists, we should proceed.
        if (e != syscall.ERROR_FILE_NOT_FOUND) {
            return (_addr_null!, error.As(e)!);
        }
        ref syscall.Win32FileAttributeData fa = ref heap(out ptr<syscall.Win32FileAttributeData> _addr_fa);
        var (pathp, e) = syscall.UTF16PtrFromString(path);
        if (e != null) {
            return (_addr_null!, error.As(e)!);
        }
        e = syscall.GetFileAttributesEx(pathp, syscall.GetFileExInfoStandard, (byte.val)(@unsafe.Pointer(_addr_fa)));
        if (e != null) {
            return (_addr_null!, error.As(e)!);
        }
        if (fa.FileAttributes & syscall.FILE_ATTRIBUTE_DIRECTORY == 0) {
            return (_addr_null!, error.As(e)!);
        }
        d.isempty = true;

    }
    d.path = path;
    if (!isAbs(d.path)) {
        d.path, e = syscall.FullPath(d.path);
        if (e != null) {
            return (_addr_null!, error.As(e)!);
        }
    }
    var f = newFile(r, name, "dir");
    f.dirinfo = d;
    return (_addr_f!, error.As(null!)!);

}

// openFileNolog is the Windows implementation of OpenFile.
private static (ptr<File>, error) openFileNolog(@string name, nint flag, FileMode perm) {
    ptr<File> _p0 = default!;
    error _p0 = default!;

    if (name == "") {
        return (_addr_null!, error.As(addr(new PathError(Op:"open",Path:name,Err:syscall.ENOENT))!)!);
    }
    var (r, errf) = openFile(name, flag, perm);
    if (errf == null) {
        return (_addr_r!, error.As(null!)!);
    }
    var (r, errd) = openDir(name);
    if (errd == null) {
        if (flag & O_WRONLY != 0 || flag & O_RDWR != 0) {
            r.Close();
            return (_addr_null!, error.As(addr(new PathError(Op:"open",Path:name,Err:syscall.EISDIR))!)!);
        }
        return (_addr_r!, error.As(null!)!);

    }
    return (_addr_null!, error.As(addr(new PathError(Op:"open",Path:name,Err:errf))!)!);

}

private static error close(this ptr<file> _addr_file) {
    ref file file = ref _addr_file.val;

    if (file == null) {
        return error.As(syscall.EINVAL)!;
    }
    if (file.isdir() && file.dirinfo.isempty) { 
        // "special" empty directories
        return error.As(null!)!;

    }
    error err = default!;
    {
        var e = file.pfd.Close();

        if (e != null) {
            if (e == poll.ErrFileClosing) {
                e = ErrClosed;
            }
            err = error.As(addr(new PathError(Op:"close",Path:file.name,Err:e)))!;
        }
    } 

    // no need for a finalizer anymore
    runtime.SetFinalizer(file, null);
    return error.As(err)!;

}

// seek sets the offset for the next Read or Write on file to offset, interpreted
// according to whence: 0 means relative to the origin of the file, 1 means
// relative to the current offset, and 2 means relative to the end.
// It returns the new offset and an error, if any.
private static (long, error) seek(this ptr<File> _addr_f, long offset, nint whence) {
    long ret = default;
    error err = default!;
    ref File f = ref _addr_f.val;

    ret, err = f.pfd.Seek(offset, whence);
    runtime.KeepAlive(f);
    return (ret, error.As(err)!);
}

// Truncate changes the size of the named file.
// If the file is a symbolic link, it changes the size of the link's target.
public static error Truncate(@string name, long size) => func((defer, _, _) => {
    var (f, e) = OpenFile(name, O_WRONLY | O_CREATE, 0666);
    if (e != null) {
        return error.As(e)!;
    }
    defer(f.Close());
    var e1 = f.Truncate(size);
    if (e1 != null) {
        return error.As(e1)!;
    }
    return error.As(null!)!;

});

// Remove removes the named file or directory.
// If there is an error, it will be of type *PathError.
public static error Remove(@string name) {
    var (p, e) = syscall.UTF16PtrFromString(fixLongPath(name));
    if (e != null) {
        return error.As(addr(new PathError(Op:"remove",Path:name,Err:e))!)!;
    }
    e = syscall.DeleteFile(p);
    if (e == null) {
        return error.As(null!)!;
    }
    var e1 = syscall.RemoveDirectory(p);
    if (e1 == null) {
        return error.As(null!)!;
    }
    if (e1 != e) {
        var (a, e2) = syscall.GetFileAttributes(p);
        if (e2 != null) {
            e = e2;
        }
        else
 {
            if (a & syscall.FILE_ATTRIBUTE_DIRECTORY != 0) {
                e = e1;
            }
            else if (a & syscall.FILE_ATTRIBUTE_READONLY != 0) {
                e1 = syscall.SetFileAttributes(p, a & ~syscall.FILE_ATTRIBUTE_READONLY);

                if (e1 == null) {
                    e = syscall.DeleteFile(p);

                    if (e == null) {
                        return error.As(null!)!;
                    }

                }

            }

        }
    }
    return error.As(addr(new PathError(Op:"remove",Path:name,Err:e))!)!;

}

private static error rename(@string oldname, @string newname) {
    var e = windows.Rename(fixLongPath(oldname), fixLongPath(newname));
    if (e != null) {
        return error.As(addr(new LinkError("rename",oldname,newname,e))!)!;
    }
    return error.As(null!)!;

}

// Pipe returns a connected pair of Files; reads from r return bytes written to w.
// It returns the files and an error, if any. The Windows handles underlying
// the returned files are marked as inheritable by child processes.
public static (ptr<File>, ptr<File>, error) Pipe() {
    ptr<File> r = default!;
    ptr<File> w = default!;
    error err = default!;

    array<syscall.Handle> p = new array<syscall.Handle>(2);
    var e = syscall.Pipe(p[..]);
    if (e != null) {
        return (_addr_null!, _addr_null!, error.As(NewSyscallError("pipe", e))!);
    }
    return (_addr_newFile(p[0], "|0", "pipe")!, _addr_newFile(p[1], "|1", "pipe")!, error.As(null!)!);

}

private static @string tempDir() {
    var n = uint32(syscall.MAX_PATH);
    while (true) {
        var b = make_slice<ushort>(n);
        n, _ = syscall.GetTempPath(uint32(len(b)), _addr_b[0]);
        if (n > uint32(len(b))) {
            continue;
        }
        if (n == 3 && b[1] == ':' && b[2] == '\\') { 
            // Do nothing for path, like C:\.
        }
        else if (n > 0 && b[n - 1] == '\\') { 
            // Otherwise remove terminating \.
            n--;

        }
        return string(utf16.Decode(b[..(int)n]));

    }

}

// Link creates newname as a hard link to the oldname file.
// If there is an error, it will be of type *LinkError.
public static error Link(@string oldname, @string newname) {
    var (n, err) = syscall.UTF16PtrFromString(fixLongPath(newname));
    if (err != null) {
        return error.As(addr(new LinkError("link",oldname,newname,err))!)!;
    }
    var (o, err) = syscall.UTF16PtrFromString(fixLongPath(oldname));
    if (err != null) {
        return error.As(addr(new LinkError("link",oldname,newname,err))!)!;
    }
    err = syscall.CreateHardLink(n, o, 0);
    if (err != null) {
        return error.As(addr(new LinkError("link",oldname,newname,err))!)!;
    }
    return error.As(null!)!;

}

// Symlink creates newname as a symbolic link to oldname.
// On Windows, a symlink to a non-existent oldname creates a file symlink;
// if oldname is later created as a directory the symlink will not work.
// If there is an error, it will be of type *LinkError.
public static error Symlink(@string oldname, @string newname) { 
    // '/' does not work in link's content
    oldname = fromSlash(oldname); 

    // need the exact location of the oldname when it's relative to determine if it's a directory
    var destpath = oldname;
    {
        var v = volumeName(oldname);

        if (v == "") {
            if (len(oldname) > 0 && IsPathSeparator(oldname[0])) { 
                // oldname is relative to the volume containing newname.
                v = volumeName(newname);

                if (v != "") { 
                    // Prepend the volume explicitly, because it may be different from the
                    // volume of the current working directory.
                    destpath = v + oldname;

                }

            }
            else
 { 
                // oldname is relative to newname.
                destpath = dirname(newname) + "\\" + oldname;

            }

        }
    }


    var (fi, err) = Stat(destpath);
    var isdir = err == null && fi.IsDir();

    var (n, err) = syscall.UTF16PtrFromString(fixLongPath(newname));
    if (err != null) {
        return error.As(addr(new LinkError("symlink",oldname,newname,err))!)!;
    }
    var (o, err) = syscall.UTF16PtrFromString(fixLongPath(oldname));
    if (err != null) {
        return error.As(addr(new LinkError("symlink",oldname,newname,err))!)!;
    }
    uint flags = windows.SYMBOLIC_LINK_FLAG_ALLOW_UNPRIVILEGED_CREATE;
    if (isdir) {
        flags |= syscall.SYMBOLIC_LINK_FLAG_DIRECTORY;
    }
    err = syscall.CreateSymbolicLink(n, o, flags);
    if (err != null) { 
        // the unprivileged create flag is unsupported
        // below Windows 10 (1703, v10.0.14972). retry without it.
        flags &= windows.SYMBOLIC_LINK_FLAG_ALLOW_UNPRIVILEGED_CREATE;
        err = syscall.CreateSymbolicLink(n, o, flags);
        if (err != null) {
            return error.As(addr(new LinkError("symlink",oldname,newname,err))!)!;
        }
    }
    return error.As(null!)!;

}

// openSymlink calls CreateFile Windows API with FILE_FLAG_OPEN_REPARSE_POINT
// parameter, so that Windows does not follow symlink, if path is a symlink.
// openSymlink returns opened file handle.
private static (syscall.Handle, error) openSymlink(@string path) {
    syscall.Handle _p0 = default;
    error _p0 = default!;

    var (p, err) = syscall.UTF16PtrFromString(path);
    if (err != null) {
        return (0, error.As(err)!);
    }
    var attrs = uint32(syscall.FILE_FLAG_BACKUP_SEMANTICS); 
    // Use FILE_FLAG_OPEN_REPARSE_POINT, otherwise CreateFile will follow symlink.
    // See https://docs.microsoft.com/en-us/windows/desktop/FileIO/symbolic-link-effects-on-file-systems-functions#createfile-and-createfiletransacted
    attrs |= syscall.FILE_FLAG_OPEN_REPARSE_POINT;
    var (h, err) = syscall.CreateFile(p, 0, 0, null, syscall.OPEN_EXISTING, attrs, 0);
    if (err != null) {
        return (0, error.As(err)!);
    }
    return (h, error.As(null!)!);

}

// normaliseLinkPath converts absolute paths returned by
// DeviceIoControl(h, FSCTL_GET_REPARSE_POINT, ...)
// into paths acceptable by all Windows APIs.
// For example, it coverts
//  \??\C:\foo\bar into C:\foo\bar
//  \??\UNC\foo\bar into \\foo\bar
//  \??\Volume{abc}\ into C:\
private static (@string, error) normaliseLinkPath(@string path) => func((defer, _, _) => {
    @string _p0 = default;
    error _p0 = default!;

    if (len(path) < 4 || path[..(int)4] != "\\??\\") { 
        // unexpected path, return it as is
        return (path, error.As(null!)!);

    }
    var s = path[(int)4..];

    if (len(s) >= 2 && s[1] == ':') // \??\C:\foo\bar
        return (s, error.As(null!)!);
    else if (len(s) >= 4 && s[..(int)4] == "UNC\\") // \??\UNC\foo\bar
        return ("\\\\" + s[(int)4..], error.As(null!)!);
    // handle paths, like \??\Volume{abc}\...

    var err = windows.LoadGetFinalPathNameByHandle();
    if (err != null) { 
        // we must be using old version of Windows
        return ("", error.As(err)!);

    }
    var (h, err) = openSymlink(path);
    if (err != null) {
        return ("", error.As(err)!);
    }
    defer(syscall.CloseHandle(h));

    var buf = make_slice<ushort>(100);
    while (true) {
        var (n, err) = windows.GetFinalPathNameByHandle(h, _addr_buf[0], uint32(len(buf)), windows.VOLUME_NAME_DOS);
        if (err != null) {
            return ("", error.As(err)!);
        }
        if (n < uint32(len(buf))) {
            break;
        }
        buf = make_slice<ushort>(n);

    }
    s = syscall.UTF16ToString(buf);
    if (len(s) > 4 && s[..(int)4] == "\\\\?\\") {
        s = s[(int)4..];
        if (len(s) > 3 && s[..(int)3] == "UNC") { 
            // return path like \\server\share\...
            return ("\\" + s[(int)3..], error.As(null!)!);

        }
        return (s, error.As(null!)!);

    }
    return ("", error.As(errors.New("GetFinalPathNameByHandle returned unexpected path: " + s))!);

});

private static (@string, error) readlink(@string path) => func((defer, _, _) => {
    @string _p0 = default;
    error _p0 = default!;

    var (h, err) = openSymlink(path);
    if (err != null) {
        return ("", error.As(err)!);
    }
    defer(syscall.CloseHandle(h));

    var rdbbuf = make_slice<byte>(syscall.MAXIMUM_REPARSE_DATA_BUFFER_SIZE);
    ref uint bytesReturned = ref heap(out ptr<uint> _addr_bytesReturned);
    err = syscall.DeviceIoControl(h, syscall.FSCTL_GET_REPARSE_POINT, null, 0, _addr_rdbbuf[0], uint32(len(rdbbuf)), _addr_bytesReturned, null);
    if (err != null) {
        return ("", error.As(err)!);
    }
    var rdb = (windows.REPARSE_DATA_BUFFER.val)(@unsafe.Pointer(_addr_rdbbuf[0]));

    if (rdb.ReparseTag == syscall.IO_REPARSE_TAG_SYMLINK) 
        var rb = (windows.SymbolicLinkReparseBuffer.val)(@unsafe.Pointer(_addr_rdb.DUMMYUNIONNAME));
        var s = rb.Path();
        if (rb.Flags & windows.SYMLINK_FLAG_RELATIVE != 0) {
            return (s, error.As(null!)!);
        }
        return normaliseLinkPath(s);
    else if (rdb.ReparseTag == windows.IO_REPARSE_TAG_MOUNT_POINT) 
        return normaliseLinkPath((windows.MountPointReparseBuffer.val)(@unsafe.Pointer(_addr_rdb.DUMMYUNIONNAME)).Path());
    else 
        // the path is not a symlink or junction but another type of reparse
        // point
        return ("", error.As(syscall.ENOENT)!);
    
});

// Readlink returns the destination of the named symbolic link.
// If there is an error, it will be of type *PathError.
public static (@string, error) Readlink(@string name) {
    @string _p0 = default;
    error _p0 = default!;

    var (s, err) = readlink(fixLongPath(name));
    if (err != null) {
        return ("", error.As(addr(new PathError(Op:"readlink",Path:name,Err:err))!)!);
    }
    return (s, error.As(null!)!);

}

} // end os_package
