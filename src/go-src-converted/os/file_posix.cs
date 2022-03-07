// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || (js && wasm) || linux || netbsd || openbsd || solaris || windows
// +build aix darwin dragonfly freebsd js,wasm linux netbsd openbsd solaris windows

// package os -- go2cs converted at 2022 March 06 22:13:36 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\file_posix.go
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using time = go.time_package;
using System;


namespace go;

public static partial class os_package {

private static void sigpipe(); // implemented in package runtime

// Close closes the File, rendering it unusable for I/O.
// On files that support SetDeadline, any pending I/O operations will
// be canceled and return immediately with an error.
// Close will return an error if it has already been called.
private static error Close(this ptr<File> _addr_f) {
    ref File f = ref _addr_f.val;

    if (f == null) {>>MARKER:FUNCTION_sigpipe_BLOCK_PREFIX<<
        return error.As(ErrInvalid)!;
    }
    return error.As(f.file.close())!;

}

// read reads up to len(b) bytes from the File.
// It returns the number of bytes read and an error, if any.
private static (nint, error) read(this ptr<File> _addr_f, slice<byte> b) {
    nint n = default;
    error err = default!;
    ref File f = ref _addr_f.val;

    n, err = f.pfd.Read(b);
    runtime.KeepAlive(f);
    return (n, error.As(err)!);
}

// pread reads len(b) bytes from the File starting at byte offset off.
// It returns the number of bytes read and the error, if any.
// EOF is signaled by a zero count with err set to nil.
private static (nint, error) pread(this ptr<File> _addr_f, slice<byte> b, long off) {
    nint n = default;
    error err = default!;
    ref File f = ref _addr_f.val;

    n, err = f.pfd.Pread(b, off);
    runtime.KeepAlive(f);
    return (n, error.As(err)!);
}

// write writes len(b) bytes to the File.
// It returns the number of bytes written and an error, if any.
private static (nint, error) write(this ptr<File> _addr_f, slice<byte> b) {
    nint n = default;
    error err = default!;
    ref File f = ref _addr_f.val;

    n, err = f.pfd.Write(b);
    runtime.KeepAlive(f);
    return (n, error.As(err)!);
}

// pwrite writes len(b) bytes to the File starting at byte offset off.
// It returns the number of bytes written and an error, if any.
private static (nint, error) pwrite(this ptr<File> _addr_f, slice<byte> b, long off) {
    nint n = default;
    error err = default!;
    ref File f = ref _addr_f.val;

    n, err = f.pfd.Pwrite(b, off);
    runtime.KeepAlive(f);
    return (n, error.As(err)!);
}

// syscallMode returns the syscall-specific mode bits from Go's portable mode bits.
private static uint syscallMode(FileMode i) {
    uint o = default;

    o |= uint32(i.Perm());
    if (i & ModeSetuid != 0) {
        o |= syscall.S_ISUID;
    }
    if (i & ModeSetgid != 0) {
        o |= syscall.S_ISGID;
    }
    if (i & ModeSticky != 0) {
        o |= syscall.S_ISVTX;
    }
    return ;

}

// See docs in file.go:Chmod.
private static error chmod(@string name, FileMode mode) {
    var longName = fixLongPath(name);
    var e = ignoringEINTR(() => {
        return error.As(syscall.Chmod(longName, syscallMode(mode)))!;
    });
    if (e != null) {
        return error.As(addr(new PathError(Op:"chmod",Path:name,Err:e))!)!;
    }
    return error.As(null!)!;

}

// See docs in file.go:(*File).Chmod.
private static error chmod(this ptr<File> _addr_f, FileMode mode) {
    ref File f = ref _addr_f.val;

    {
        var err = f.checkValid("chmod");

        if (err != null) {
            return error.As(err)!;
        }
    }

    {
        var e = f.pfd.Fchmod(syscallMode(mode));

        if (e != null) {
            return error.As(f.wrapErr("chmod", e))!;
        }
    }

    return error.As(null!)!;

}

// Chown changes the numeric uid and gid of the named file.
// If the file is a symbolic link, it changes the uid and gid of the link's target.
// A uid or gid of -1 means to not change that value.
// If there is an error, it will be of type *PathError.
//
// On Windows or Plan 9, Chown always returns the syscall.EWINDOWS or
// EPLAN9 error, wrapped in *PathError.
public static error Chown(@string name, nint uid, nint gid) {
    var e = ignoringEINTR(() => {
        return error.As(syscall.Chown(name, uid, gid))!;
    });
    if (e != null) {
        return error.As(addr(new PathError(Op:"chown",Path:name,Err:e))!)!;
    }
    return error.As(null!)!;

}

// Lchown changes the numeric uid and gid of the named file.
// If the file is a symbolic link, it changes the uid and gid of the link itself.
// If there is an error, it will be of type *PathError.
//
// On Windows, it always returns the syscall.EWINDOWS error, wrapped
// in *PathError.
public static error Lchown(@string name, nint uid, nint gid) {
    var e = ignoringEINTR(() => {
        return error.As(syscall.Lchown(name, uid, gid))!;
    });
    if (e != null) {
        return error.As(addr(new PathError(Op:"lchown",Path:name,Err:e))!)!;
    }
    return error.As(null!)!;

}

// Chown changes the numeric uid and gid of the named file.
// If there is an error, it will be of type *PathError.
//
// On Windows, it always returns the syscall.EWINDOWS error, wrapped
// in *PathError.
private static error Chown(this ptr<File> _addr_f, nint uid, nint gid) {
    ref File f = ref _addr_f.val;

    {
        var err = f.checkValid("chown");

        if (err != null) {
            return error.As(err)!;
        }
    }

    {
        var e = f.pfd.Fchown(uid, gid);

        if (e != null) {
            return error.As(f.wrapErr("chown", e))!;
        }
    }

    return error.As(null!)!;

}

// Truncate changes the size of the file.
// It does not change the I/O offset.
// If there is an error, it will be of type *PathError.
private static error Truncate(this ptr<File> _addr_f, long size) {
    ref File f = ref _addr_f.val;

    {
        var err = f.checkValid("truncate");

        if (err != null) {
            return error.As(err)!;
        }
    }

    {
        var e = f.pfd.Ftruncate(size);

        if (e != null) {
            return error.As(f.wrapErr("truncate", e))!;
        }
    }

    return error.As(null!)!;

}

// Sync commits the current contents of the file to stable storage.
// Typically, this means flushing the file system's in-memory copy
// of recently written data to disk.
private static error Sync(this ptr<File> _addr_f) {
    ref File f = ref _addr_f.val;

    {
        var err = f.checkValid("sync");

        if (err != null) {
            return error.As(err)!;
        }
    }

    {
        var e = f.pfd.Fsync();

        if (e != null) {
            return error.As(f.wrapErr("sync", e))!;
        }
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
    array<syscall.Timespec> utimes = new array<syscall.Timespec>(2);
    utimes[0] = syscall.NsecToTimespec(atime.UnixNano());
    utimes[1] = syscall.NsecToTimespec(mtime.UnixNano());
    {
        var e = syscall.UtimesNano(fixLongPath(name), utimes[(int)0..]);

        if (e != null) {
            return error.As(addr(new PathError(Op:"chtimes",Path:name,Err:e))!)!;
        }
    }

    return error.As(null!)!;

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
        var e = f.pfd.Fchdir();

        if (e != null) {
            return error.As(f.wrapErr("chdir", e))!;
        }
    }

    return error.As(null!)!;

}

// setDeadline sets the read and write deadline.
private static error setDeadline(this ptr<File> _addr_f, time.Time t) {
    ref File f = ref _addr_f.val;

    {
        var err = f.checkValid("SetDeadline");

        if (err != null) {
            return error.As(err)!;
        }
    }

    return error.As(f.pfd.SetDeadline(t))!;

}

// setReadDeadline sets the read deadline.
private static error setReadDeadline(this ptr<File> _addr_f, time.Time t) {
    ref File f = ref _addr_f.val;

    {
        var err = f.checkValid("SetReadDeadline");

        if (err != null) {
            return error.As(err)!;
        }
    }

    return error.As(f.pfd.SetReadDeadline(t))!;

}

// setWriteDeadline sets the write deadline.
private static error setWriteDeadline(this ptr<File> _addr_f, time.Time t) {
    ref File f = ref _addr_f.val;

    {
        var err = f.checkValid("SetWriteDeadline");

        if (err != null) {
            return error.As(err)!;
        }
    }

    return error.As(f.pfd.SetWriteDeadline(t))!;

}

// checkValid checks whether f is valid for use.
// If not, it returns an appropriate error, perhaps incorporating the operation name op.
private static error checkValid(this ptr<File> _addr_f, @string op) {
    ref File f = ref _addr_f.val;

    if (f == null) {
        return error.As(ErrInvalid)!;
    }
    return error.As(null!)!;

}

// ignoringEINTR makes a function call and repeats it if it returns an
// EINTR error. This appears to be required even though we install all
// signal handlers with SA_RESTART: see #22838, #38033, #38836, #40846.
// Also #20400 and #36644 are issues in which a signal handler is
// installed without setting SA_RESTART. None of these are the common case,
// but there are enough of them that it seems that we can't avoid
// an EINTR loop.
private static error ignoringEINTR(Func<error> fn) {
    while (true) {
        var err = fn();
        if (err != syscall.EINTR) {
            return error.As(err)!;
        }
    }

}

} // end os_package
