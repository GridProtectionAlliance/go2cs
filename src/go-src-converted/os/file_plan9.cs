// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2022 March 06 22:13:35 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\file_plan9.go
using poll = go.@internal.poll_package;
using io = go.io_package;
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using time = go.time_package;
using System;


namespace go;

public static partial class os_package {

    // fixLongPath is a noop on non-Windows platforms.
private static @string fixLongPath(@string path) {
    return path;
}

// file is the real representation of *File.
// The extra level of indirection ensures that no clients of os
// can overwrite this data, which could cause the finalizer
// to close the wrong file descriptor.
private partial struct file {
    public nint fd;
    public @string name;
    public ptr<dirInfo> dirinfo; // nil unless directory being read
    public bool appendMode; // whether file is opened for appending
}

// Fd returns the integer Plan 9 file descriptor referencing the open file.
// If f is closed, the file descriptor becomes invalid.
// If f is garbage collected, a finalizer may close the file descriptor,
// making it invalid; see runtime.SetFinalizer for more information on when
// a finalizer might be run. On Unix systems this will cause the SetDeadline
// methods to stop working.
//
// As an alternative, see the f.SyscallConn method.
private static System.UIntPtr Fd(this ptr<File> _addr_f) {
    ref File f = ref _addr_f.val;

    if (f == null) {
        return ~(uintptr(0));
    }
    return uintptr(f.fd);

}

// NewFile returns a new File with the given file descriptor and
// name. The returned value will be nil if fd is not a valid file
// descriptor.
public static ptr<File> NewFile(System.UIntPtr fd, @string name) {
    var fdi = int(fd);
    if (fdi < 0) {
        return _addr_null!;
    }
    ptr<File> f = addr(new File(&file{fd:fdi,name:name}));
    runtime.SetFinalizer(f.file, (file.val).close);
    return _addr_f!;

}

// Auxiliary information if the File describes a directory
private partial struct dirInfo {
    public array<byte> buf; // buffer for directory I/O
    public nint nbuf; // length of buf; return value from Read
    public nint bufp; // location of next record in buf.
}

private static void epipecheck(ptr<File> _addr_file, error e) {
    ref File file = ref _addr_file.val;

}

// DevNull is the name of the operating system's ``null device.''
// On Unix-like systems, it is "/dev/null"; on Windows, "NUL".
public static readonly @string DevNull = "/dev/null";

// syscallMode returns the syscall-specific mode bits from Go's portable mode bits.


// syscallMode returns the syscall-specific mode bits from Go's portable mode bits.
private static uint syscallMode(FileMode i) {
    uint o = default;

    o |= uint32(i.Perm());
    if (i & ModeAppend != 0) {
        o |= syscall.DMAPPEND;
    }
    if (i & ModeExclusive != 0) {
        o |= syscall.DMEXCL;
    }
    if (i & ModeTemporary != 0) {
        o |= syscall.DMTMP;
    }
    return ;

}

// openFileNolog is the Plan 9 implementation of OpenFile.
private static (ptr<File>, error) openFileNolog(@string name, nint flag, FileMode perm) {
    ptr<File> _p0 = default!;
    error _p0 = default!;

    nint fd = default;    error e = default!;    bool create = default;    bool excl = default;    bool trunc = default;    bool append = default;

    if (flag & O_CREATE == O_CREATE) {
        flag = flag & ~O_CREATE;
        create = true;
    }
    if (flag & O_EXCL == O_EXCL) {
        excl = true;
    }
    if (flag & O_TRUNC == O_TRUNC) {
        trunc = true;
    }
    if (flag & O_APPEND == O_APPEND) {
        flag = flag & ~O_APPEND;
        append = true;
    }
    if ((create && trunc) || excl) {
        fd, e = syscall.Create(name, flag, syscallMode(perm));
    }
    else
 {
        fd, e = syscall.Open(name, flag);
        if (IsNotExist(e) && create) {
            fd, e = syscall.Create(name, flag, syscallMode(perm));
            if (e != null) {
                return (_addr_null!, error.As(addr(new PathError(Op:"create",Path:name,Err:e))!)!);
            }
        }
    }
    if (e != null) {
        return (_addr_null!, error.As(addr(new PathError(Op:"open",Path:name,Err:e))!)!);
    }
    if (append) {
        _, e = syscall.Seek(fd, 0, io.SeekEnd);

        if (e != null) {
            return (_addr_null!, error.As(addr(new PathError(Op:"seek",Path:name,Err:e))!)!);
        }
    }
    return (_addr_NewFile(uintptr(fd), name)!, error.As(null!)!);

}

// Close closes the File, rendering it unusable for I/O.
// On files that support SetDeadline, any pending I/O operations will
// be canceled and return immediately with an error.
// Close will return an error if it has already been called.
private static error Close(this ptr<File> _addr_f) {
    ref File f = ref _addr_f.val;

    {
        var err = f.checkValid("close");

        if (err != null) {
            return error.As(err)!;
        }
    }

    return error.As(f.file.close())!;

}

private static error close(this ptr<file> _addr_file) {
    ref file file = ref _addr_file.val;

    if (file == null || file.fd == badFd) {
        return error.As(ErrInvalid)!;
    }
    error err = default!;
    {
        var e = syscall.Close(file.fd);

        if (e != null) {
            err = error.As(addr(new PathError(Op:"close",Path:file.name,Err:e)))!;
        }
    }

    file.fd = badFd; // so it can't be closed again

    // no need for a finalizer anymore
    runtime.SetFinalizer(file, null);
    return error.As(err)!;

}

// Stat returns the FileInfo structure describing file.
// If there is an error, it will be of type *PathError.
private static (FileInfo, error) Stat(this ptr<File> _addr_f) {
    FileInfo _p0 = default;
    error _p0 = default!;
    ref File f = ref _addr_f.val;

    if (f == null) {
        return (null, error.As(ErrInvalid)!);
    }
    var (d, err) = dirstat(f);
    if (err != null) {
        return (null, error.As(err)!);
    }
    return (fileInfoFromStat(d), error.As(null!)!);

}

// Truncate changes the size of the file.
// It does not change the I/O offset.
// If there is an error, it will be of type *PathError.
private static error Truncate(this ptr<File> _addr_f, long size) {
    ref File f = ref _addr_f.val;

    if (f == null) {
        return error.As(ErrInvalid)!;
    }
    syscall.Dir d = default;
    d.Null();
    d.Length = size;

    array<byte> buf = new array<byte>(syscall.STATFIXLEN);
    var (n, err) = d.Marshal(buf[..]);
    if (err != null) {
        return error.As(addr(new PathError(Op:"truncate",Path:f.name,Err:err))!)!;
    }
    err = syscall.Fwstat(f.fd, buf[..(int)n]);

    if (err != null) {
        return error.As(addr(new PathError(Op:"truncate",Path:f.name,Err:err))!)!;
    }
    return error.As(null!)!;

}

private static readonly var chmodMask = uint32(syscall.DMAPPEND | syscall.DMEXCL | syscall.DMTMP | ModePerm);



private static error chmod(this ptr<File> _addr_f, FileMode mode) {
    ref File f = ref _addr_f.val;

    if (f == null) {
        return error.As(ErrInvalid)!;
    }
    syscall.Dir d = default;

    var (odir, e) = dirstat(f);
    if (e != null) {
        return error.As(addr(new PathError(Op:"chmod",Path:f.name,Err:e))!)!;
    }
    d.Null();
    d.Mode = odir.Mode & ~chmodMask | syscallMode(mode) & chmodMask;

    array<byte> buf = new array<byte>(syscall.STATFIXLEN);
    var (n, err) = d.Marshal(buf[..]);
    if (err != null) {
        return error.As(addr(new PathError(Op:"chmod",Path:f.name,Err:err))!)!;
    }
    err = syscall.Fwstat(f.fd, buf[..(int)n]);

    if (err != null) {
        return error.As(addr(new PathError(Op:"chmod",Path:f.name,Err:err))!)!;
    }
    return error.As(null!)!;

}

// Sync commits the current contents of the file to stable storage.
// Typically, this means flushing the file system's in-memory copy
// of recently written data to disk.
private static error Sync(this ptr<File> _addr_f) {
    ref File f = ref _addr_f.val;

    if (f == null) {
        return error.As(ErrInvalid)!;
    }
    syscall.Dir d = default;
    d.Null();

    array<byte> buf = new array<byte>(syscall.STATFIXLEN);
    var (n, err) = d.Marshal(buf[..]);
    if (err != null) {
        return error.As(addr(new PathError(Op:"sync",Path:f.name,Err:err))!)!;
    }
    err = syscall.Fwstat(f.fd, buf[..(int)n]);

    if (err != null) {
        return error.As(addr(new PathError(Op:"sync",Path:f.name,Err:err))!)!;
    }
    return error.As(null!)!;

}

// read reads up to len(b) bytes from the File.
// It returns the number of bytes read and an error, if any.
private static (nint, error) read(this ptr<File> _addr_f, slice<byte> b) {
    nint n = default;
    error err = default!;
    ref File f = ref _addr_f.val;

    var (n, e) = fixCount(syscall.Read(f.fd, b));
    if (n == 0 && len(b) > 0 && e == null) {
        return (0, error.As(io.EOF)!);
    }
    return (n, error.As(e)!);

}

// pread reads len(b) bytes from the File starting at byte offset off.
// It returns the number of bytes read and the error, if any.
// EOF is signaled by a zero count with err set to nil.
private static (nint, error) pread(this ptr<File> _addr_f, slice<byte> b, long off) {
    nint n = default;
    error err = default!;
    ref File f = ref _addr_f.val;

    var (n, e) = fixCount(syscall.Pread(f.fd, b, off));
    if (n == 0 && len(b) > 0 && e == null) {
        return (0, error.As(io.EOF)!);
    }
    return (n, error.As(e)!);

}

// write writes len(b) bytes to the File.
// It returns the number of bytes written and an error, if any.
// Since Plan 9 preserves message boundaries, never allow
// a zero-byte write.
private static (nint, error) write(this ptr<File> _addr_f, slice<byte> b) {
    nint n = default;
    error err = default!;
    ref File f = ref _addr_f.val;

    if (len(b) == 0) {
        return (0, error.As(null!)!);
    }
    return fixCount(syscall.Write(f.fd, b));

}

// pwrite writes len(b) bytes to the File starting at byte offset off.
// It returns the number of bytes written and an error, if any.
// Since Plan 9 preserves message boundaries, never allow
// a zero-byte write.
private static (nint, error) pwrite(this ptr<File> _addr_f, slice<byte> b, long off) {
    nint n = default;
    error err = default!;
    ref File f = ref _addr_f.val;

    if (len(b) == 0) {
        return (0, error.As(null!)!);
    }
    return fixCount(syscall.Pwrite(f.fd, b, off));

}

// seek sets the offset for the next Read or Write on file to offset, interpreted
// according to whence: 0 means relative to the origin of the file, 1 means
// relative to the current offset, and 2 means relative to the end.
// It returns the new offset and an error, if any.
private static (long, error) seek(this ptr<File> _addr_f, long offset, nint whence) {
    long ret = default;
    error err = default!;
    ref File f = ref _addr_f.val;

    if (f.dirinfo != null) { 
        // Free cached dirinfo, so we allocate a new one if we
        // access this file as a directory again. See #35767 and #37161.
        f.dirinfo = null;

    }
    return syscall.Seek(f.fd, offset, whence);

}

// Truncate changes the size of the named file.
// If the file is a symbolic link, it changes the size of the link's target.
// If there is an error, it will be of type *PathError.
public static error Truncate(@string name, long size) {
    syscall.Dir d = default;

    d.Null();
    d.Length = size;

    array<byte> buf = new array<byte>(syscall.STATFIXLEN);
    var (n, err) = d.Marshal(buf[..]);
    if (err != null) {
        return error.As(addr(new PathError(Op:"truncate",Path:name,Err:err))!)!;
    }
    err = syscall.Wstat(name, buf[..(int)n]);

    if (err != null) {
        return error.As(addr(new PathError(Op:"truncate",Path:name,Err:err))!)!;
    }
    return error.As(null!)!;

}

// Remove removes the named file or directory.
// If there is an error, it will be of type *PathError.
public static error Remove(@string name) {
    {
        var e = syscall.Remove(name);

        if (e != null) {
            return error.As(addr(new PathError(Op:"remove",Path:name,Err:e))!)!;
        }
    }

    return error.As(null!)!;

}

// HasPrefix from the strings package.
private static bool hasPrefix(@string s, @string prefix) {
    return len(s) >= len(prefix) && s[(int)0..(int)len(prefix)] == prefix;
}

private static error rename(@string oldname, @string newname) {
    var dirname = oldname[..(int)lastIndex(oldname, '/') + 1];
    if (hasPrefix(newname, dirname)) {
        newname = newname[(int)len(dirname)..];
    }
    else
 {
        return error.As(addr(new LinkError("rename",oldname,newname,ErrInvalid))!)!;
    }
    if (lastIndex(newname, '/') >= 0) {
        return error.As(addr(new LinkError("rename",oldname,newname,ErrInvalid))!)!;
    }
    syscall.Dir d = default;

    d.Null();
    d.Name = newname;

    var buf = make_slice<byte>(syscall.STATFIXLEN + len(d.Name));
    var (n, err) = d.Marshal(buf[..]);
    if (err != null) {
        return error.As(addr(new LinkError("rename",oldname,newname,err))!)!;
    }
    var (f, err) = Stat(dirname + newname);
    if (err == null && !f.IsDir()) {
        Remove(dirname + newname);
    }
    err = syscall.Wstat(oldname, buf[..(int)n]);

    if (err != null) {
        return error.As(addr(new LinkError("rename",oldname,newname,err))!)!;
    }
    return error.As(null!)!;

}

// See docs in file.go:Chmod.
private static error chmod(@string name, FileMode mode) {
    syscall.Dir d = default;

    var (odir, e) = dirstat(name);
    if (e != null) {
        return error.As(addr(new PathError(Op:"chmod",Path:name,Err:e))!)!;
    }
    d.Null();
    d.Mode = odir.Mode & ~chmodMask | syscallMode(mode) & chmodMask;

    array<byte> buf = new array<byte>(syscall.STATFIXLEN);
    var (n, err) = d.Marshal(buf[..]);
    if (err != null) {
        return error.As(addr(new PathError(Op:"chmod",Path:name,Err:err))!)!;
    }
    err = syscall.Wstat(name, buf[..(int)n]);

    if (err != null) {
        return error.As(addr(new PathError(Op:"chmod",Path:name,Err:err))!)!;
    }
    return error.As(null!)!;

}

// Chtimes changes the access and modification times of the named
// file, similar to the Unix utime() or utimes() functions.
//
// The underlying filesystem may truncate or round the values to a
// less precise time unit.
// If there is an error, it will be of type *PathError.
public static error Chtimes(@string name, time.Time atime, time.Time mtime) {
    syscall.Dir d = default;

    d.Null();
    d.Atime = uint32(atime.Unix());
    d.Mtime = uint32(mtime.Unix());

    array<byte> buf = new array<byte>(syscall.STATFIXLEN);
    var (n, err) = d.Marshal(buf[..]);
    if (err != null) {
        return error.As(addr(new PathError(Op:"chtimes",Path:name,Err:err))!)!;
    }
    err = syscall.Wstat(name, buf[..(int)n]);

    if (err != null) {
        return error.As(addr(new PathError(Op:"chtimes",Path:name,Err:err))!)!;
    }
    return error.As(null!)!;

}

// Pipe returns a connected pair of Files; reads from r return bytes
// written to w. It returns the files and an error, if any.
public static (ptr<File>, ptr<File>, error) Pipe() {
    ptr<File> r = default!;
    ptr<File> w = default!;
    error err = default!;

    array<nint> p = new array<nint>(2);

    {
        var e = syscall.Pipe(p[(int)0..]);

        if (e != null) {
            return (_addr_null!, _addr_null!, error.As(NewSyscallError("pipe", e))!);
        }
    }


    return (_addr_NewFile(uintptr(p[0]), "|0")!, _addr_NewFile(uintptr(p[1]), "|1")!, error.As(null!)!);

}

// not supported on Plan 9

// Link creates newname as a hard link to the oldname file.
// If there is an error, it will be of type *LinkError.
public static error Link(@string oldname, @string newname) {
    return error.As(addr(new LinkError("link",oldname,newname,syscall.EPLAN9))!)!;
}

// Symlink creates newname as a symbolic link to oldname.
// On Windows, a symlink to a non-existent oldname creates a file symlink;
// if oldname is later created as a directory the symlink will not work.
// If there is an error, it will be of type *LinkError.
public static error Symlink(@string oldname, @string newname) {
    return error.As(addr(new LinkError("symlink",oldname,newname,syscall.EPLAN9))!)!;
}

// Readlink returns the destination of the named symbolic link.
// If there is an error, it will be of type *PathError.
public static (@string, error) Readlink(@string name) {
    @string _p0 = default;
    error _p0 = default!;

    return ("", error.As(addr(new PathError(Op:"readlink",Path:name,Err:syscall.EPLAN9))!)!);
}

// Chown changes the numeric uid and gid of the named file.
// If the file is a symbolic link, it changes the uid and gid of the link's target.
// A uid or gid of -1 means to not change that value.
// If there is an error, it will be of type *PathError.
//
// On Windows or Plan 9, Chown always returns the syscall.EWINDOWS or
// EPLAN9 error, wrapped in *PathError.
public static error Chown(@string name, nint uid, nint gid) {
    return error.As(addr(new PathError(Op:"chown",Path:name,Err:syscall.EPLAN9))!)!;
}

// Lchown changes the numeric uid and gid of the named file.
// If the file is a symbolic link, it changes the uid and gid of the link itself.
// If there is an error, it will be of type *PathError.
public static error Lchown(@string name, nint uid, nint gid) {
    return error.As(addr(new PathError(Op:"lchown",Path:name,Err:syscall.EPLAN9))!)!;
}

// Chown changes the numeric uid and gid of the named file.
// If there is an error, it will be of type *PathError.
private static error Chown(this ptr<File> _addr_f, nint uid, nint gid) {
    ref File f = ref _addr_f.val;

    if (f == null) {
        return error.As(ErrInvalid)!;
    }
    return error.As(addr(new PathError(Op:"chown",Path:f.name,Err:syscall.EPLAN9))!)!;

}

private static @string tempDir() {
    var dir = Getenv("TMPDIR");
    if (dir == "") {
        dir = "/tmp";
    }
    return dir;


}

// Chdir changes the current working directory to the file,
// which must be a directory.
// If there is an error, it will be of type *PathError.
private static error Chdir(this ptr<File> _addr_f) {
    ref File f = ref _addr_f.val;

    {
        var err = f.checkValid("chdir");

        if (err != null) {
            return error.As(err)!;
        }
    }

    {
        var e = syscall.Fchdir(f.fd);

        if (e != null) {
            return error.As(addr(new PathError(Op:"chdir",Path:f.name,Err:e))!)!;
        }
    }

    return error.As(null!)!;

}

// setDeadline sets the read and write deadline.
private static error setDeadline(this ptr<File> _addr_f, time.Time _p0) {
    ref File f = ref _addr_f.val;

    {
        var err = f.checkValid("SetDeadline");

        if (err != null) {
            return error.As(err)!;
        }
    }

    return error.As(poll.ErrNoDeadline)!;

}

// setReadDeadline sets the read deadline.
private static error setReadDeadline(this ptr<File> _addr_f, time.Time _p0) {
    ref File f = ref _addr_f.val;

    {
        var err = f.checkValid("SetReadDeadline");

        if (err != null) {
            return error.As(err)!;
        }
    }

    return error.As(poll.ErrNoDeadline)!;

}

// setWriteDeadline sets the write deadline.
private static error setWriteDeadline(this ptr<File> _addr_f, time.Time _p0) {
    ref File f = ref _addr_f.val;

    {
        var err = f.checkValid("SetWriteDeadline");

        if (err != null) {
            return error.As(err)!;
        }
    }

    return error.As(poll.ErrNoDeadline)!;

}

// checkValid checks whether f is valid for use.
// If not, it returns an appropriate error, perhaps incorporating the operation name op.
private static error checkValid(this ptr<File> _addr_f, @string op) {
    ref File f = ref _addr_f.val;

    if (f == null) {
        return error.As(ErrInvalid)!;
    }
    if (f.fd == badFd) {
        return error.As(addr(new PathError(Op:op,Path:f.name,Err:ErrClosed))!)!;
    }
    return error.As(null!)!;

}

private partial struct rawConn {
}

private static error Control(this ptr<rawConn> _addr_c, Action<System.UIntPtr> f) {
    ref rawConn c = ref _addr_c.val;

    return error.As(syscall.EPLAN9)!;
}

private static error Read(this ptr<rawConn> _addr_c, Func<System.UIntPtr, bool> f) {
    ref rawConn c = ref _addr_c.val;

    return error.As(syscall.EPLAN9)!;
}

private static error Write(this ptr<rawConn> _addr_c, Func<System.UIntPtr, bool> f) {
    ref rawConn c = ref _addr_c.val;

    return error.As(syscall.EPLAN9)!;
}

private static (ptr<rawConn>, error) newRawConn(ptr<File> _addr_file) {
    ptr<rawConn> _p0 = default!;
    error _p0 = default!;
    ref File file = ref _addr_file.val;

    return (_addr_null!, error.As(syscall.EPLAN9)!);
}

private static error ignoringEINTR(Func<error> fn) {
    return error.As(fn())!;
}

} // end os_package
