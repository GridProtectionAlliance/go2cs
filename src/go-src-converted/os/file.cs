// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package os provides a platform-independent interface to operating system
// functionality. The design is Unix-like, although the error handling is
// Go-like; failing calls return values of type error rather than error numbers.
// Often, more information is available within the error. For example,
// if a call that takes a file name fails, such as Open or Stat, the error
// will include the failing file name when printed and will be of type
// *PathError, which may be unpacked for more information.
//
// The os interface is intended to be uniform across all operating systems.
// Features not generally available appear in the system-specific package syscall.
//
// Here is a simple example, opening a file and reading some of it.
//
//    file, err := os.Open("file.go") // For read access.
//    if err != nil {
//        log.Fatal(err)
//    }
//
// If the open fails, the error string will be self-explanatory, like
//
//    open file.go: no such file or directory
//
// The file's data can then be read into a slice of bytes. Read and
// Write take their byte counts from the length of the argument slice.
//
//    data := make([]byte, 100)
//    count, err := file.Read(data)
//    if err != nil {
//        log.Fatal(err)
//    }
//    fmt.Printf("read %d bytes: %q\n", count, data[:count])
//
// Note: The maximum number of concurrent operations on a File may be limited by
// the OS or the system. The number should be high, but exceeding it may degrade
// performance or cause other issues.
//

// package os -- go2cs converted at 2022 March 13 05:27:58 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\file.go
namespace go;

using errors = errors_package;
using poll = @internal.poll_package;
using testlog = @internal.testlog_package;
using unsafeheader = @internal.unsafeheader_package;
using io = io_package;
using fs = io.fs_package;
using runtime = runtime_package;
using syscall = syscall_package;
using time = time_package;
using @unsafe = @unsafe_package;


// Name returns the name of the file as presented to Open.

using System;
public static partial class os_package {

private static @string Name(this ptr<File> _addr_f) {
    ref File f = ref _addr_f.val;

    return f.name;
}

// Stdin, Stdout, and Stderr are open Files pointing to the standard input,
// standard output, and standard error file descriptors.
//
// Note that the Go runtime writes to standard error for panics and crashes;
// closing Stderr may cause those messages to go elsewhere, perhaps
// to a file opened later.
public static var Stdin = NewFile(uintptr(syscall.Stdin), "/dev/stdin");public static var Stdout = NewFile(uintptr(syscall.Stdout), "/dev/stdout");public static var Stderr = NewFile(uintptr(syscall.Stderr), "/dev/stderr");

// Flags to OpenFile wrapping those of the underlying system. Not all
// flags may be implemented on a given system.
 
// Exactly one of O_RDONLY, O_WRONLY, or O_RDWR must be specified.
public static readonly nint O_RDONLY = syscall.O_RDONLY; // open the file read-only.
public static readonly nint O_WRONLY = syscall.O_WRONLY; // open the file write-only.
public static readonly nint O_RDWR = syscall.O_RDWR; // open the file read-write.
// The remaining values may be or'ed in to control behavior.
public static readonly nint O_APPEND = syscall.O_APPEND; // append data to the file when writing.
public static readonly nint O_CREATE = syscall.O_CREAT; // create a new file if none exists.
public static readonly nint O_EXCL = syscall.O_EXCL; // used with O_CREATE, file must not exist.
public static readonly nint O_SYNC = syscall.O_SYNC; // open for synchronous I/O.
public static readonly nint O_TRUNC = syscall.O_TRUNC; // truncate regular writable file when opened.

// Seek whence values.
//
// Deprecated: Use io.SeekStart, io.SeekCurrent, and io.SeekEnd.
public static readonly nint SEEK_SET = 0; // seek relative to the origin of the file
public static readonly nint SEEK_CUR = 1; // seek relative to the current offset
public static readonly nint SEEK_END = 2; // seek relative to the end

// LinkError records an error during a link or symlink or rename
// system call and the paths that caused it.
public partial struct LinkError {
    public @string Op;
    public @string Old;
    public @string New;
    public error Err;
}

private static @string Error(this ptr<LinkError> _addr_e) {
    ref LinkError e = ref _addr_e.val;

    return e.Op + " " + e.Old + " " + e.New + ": " + e.Err.Error();
}

private static error Unwrap(this ptr<LinkError> _addr_e) {
    ref LinkError e = ref _addr_e.val;

    return error.As(e.Err)!;
}

// Read reads up to len(b) bytes from the File.
// It returns the number of bytes read and any error encountered.
// At end of file, Read returns 0, io.EOF.
private static (nint, error) Read(this ptr<File> _addr_f, slice<byte> b) {
    nint n = default;
    error err = default!;
    ref File f = ref _addr_f.val;

    {
        var err = f.checkValid("read");

        if (err != null) {
            return (0, error.As(err)!);
        }
    }
    var (n, e) = f.read(b);
    return (n, error.As(f.wrapErr("read", e))!);
}

// ReadAt reads len(b) bytes from the File starting at byte offset off.
// It returns the number of bytes read and the error, if any.
// ReadAt always returns a non-nil error when n < len(b).
// At end of file, that error is io.EOF.
private static (nint, error) ReadAt(this ptr<File> _addr_f, slice<byte> b, long off) {
    nint n = default;
    error err = default!;
    ref File f = ref _addr_f.val;

    {
        var err = f.checkValid("read");

        if (err != null) {
            return (0, error.As(err)!);
        }
    }

    if (off < 0) {
        return (0, error.As(addr(new PathError(Op:"readat",Path:f.name,Err:errors.New("negative offset")))!)!);
    }
    while (len(b) > 0) {
        var (m, e) = f.pread(b, off);
        if (e != null) {
            err = f.wrapErr("read", e);
            break;
        }
        n += m;
        b = b[(int)m..];
        off += int64(m);
    }
    return ;
}

// ReadFrom implements io.ReaderFrom.
private static (long, error) ReadFrom(this ptr<File> _addr_f, io.Reader r) {
    long n = default;
    error err = default!;
    ref File f = ref _addr_f.val;

    {
        var err = f.checkValid("write");

        if (err != null) {
            return (0, error.As(err)!);
        }
    }
    var (n, handled, e) = f.readFrom(r);
    if (!handled) {
        return genericReadFrom(_addr_f, r); // without wrapping
    }
    return (n, error.As(f.wrapErr("write", e))!);
}

private static (long, error) genericReadFrom(ptr<File> _addr_f, io.Reader r) {
    long _p0 = default;
    error _p0 = default!;
    ref File f = ref _addr_f.val;

    return io.Copy(new onlyWriter(f), r);
}

private partial struct onlyWriter : io.Writer {
    public ref io.Writer Writer => ref Writer_val;
}

// Write writes len(b) bytes to the File.
// It returns the number of bytes written and an error, if any.
// Write returns a non-nil error when n != len(b).
private static (nint, error) Write(this ptr<File> _addr_f, slice<byte> b) {
    nint n = default;
    error err = default!;
    ref File f = ref _addr_f.val;

    {
        var err = f.checkValid("write");

        if (err != null) {
            return (0, error.As(err)!);
        }
    }
    var (n, e) = f.write(b);
    if (n < 0) {
        n = 0;
    }
    if (n != len(b)) {
        err = io.ErrShortWrite;
    }
    epipecheck(f, e);

    if (e != null) {
        err = f.wrapErr("write", e);
    }
    return (n, error.As(err)!);
}

private static var errWriteAtInAppendMode = errors.New("os: invalid use of WriteAt on file opened with O_APPEND");

// WriteAt writes len(b) bytes to the File starting at byte offset off.
// It returns the number of bytes written and an error, if any.
// WriteAt returns a non-nil error when n != len(b).
//
// If file was opened with the O_APPEND flag, WriteAt returns an error.
private static (nint, error) WriteAt(this ptr<File> _addr_f, slice<byte> b, long off) {
    nint n = default;
    error err = default!;
    ref File f = ref _addr_f.val;

    {
        var err = f.checkValid("write");

        if (err != null) {
            return (0, error.As(err)!);
        }
    }
    if (f.appendMode) {
        return (0, error.As(errWriteAtInAppendMode)!);
    }
    if (off < 0) {
        return (0, error.As(addr(new PathError(Op:"writeat",Path:f.name,Err:errors.New("negative offset")))!)!);
    }
    while (len(b) > 0) {
        var (m, e) = f.pwrite(b, off);
        if (e != null) {
            err = f.wrapErr("write", e);
            break;
        }
        n += m;
        b = b[(int)m..];
        off += int64(m);
    }
    return ;
}

// Seek sets the offset for the next Read or Write on file to offset, interpreted
// according to whence: 0 means relative to the origin of the file, 1 means
// relative to the current offset, and 2 means relative to the end.
// It returns the new offset and an error, if any.
// The behavior of Seek on a file opened with O_APPEND is not specified.
//
// If f is a directory, the behavior of Seek varies by operating
// system; you can seek to the beginning of the directory on Unix-like
// operating systems, but not on Windows.
private static (long, error) Seek(this ptr<File> _addr_f, long offset, nint whence) {
    long ret = default;
    error err = default!;
    ref File f = ref _addr_f.val;

    {
        var err = f.checkValid("seek");

        if (err != null) {
            return (0, error.As(err)!);
        }
    }
    var (r, e) = f.seek(offset, whence);
    if (e == null && f.dirinfo != null && r != 0) {
        e = syscall.EISDIR;
    }
    if (e != null) {
        return (0, error.As(f.wrapErr("seek", e))!);
    }
    return (r, error.As(null!)!);
}

// WriteString is like Write, but writes the contents of string s rather than
// a slice of bytes.
private static (nint, error) WriteString(this ptr<File> _addr_f, @string s) {
    nint n = default;
    error err = default!;
    ref File f = ref _addr_f.val;

    ref slice<byte> b = ref heap(out ptr<slice<byte>> _addr_b);
    var hdr = (unsafeheader.Slice.val)(@unsafe.Pointer(_addr_b));
    hdr.Data = (unsafeheader.String.val)(@unsafe.Pointer(_addr_s)).Data;
    hdr.Cap = len(s);
    hdr.Len = len(s);
    return f.Write(b);
}

// Mkdir creates a new directory with the specified name and permission
// bits (before umask).
// If there is an error, it will be of type *PathError.
public static error Mkdir(@string name, FileMode perm) {
    if (runtime.GOOS == "windows" && isWindowsNulName(name)) {
        return error.As(addr(new PathError(Op:"mkdir",Path:name,Err:syscall.ENOTDIR))!)!;
    }
    var longName = fixLongPath(name);
    var e = ignoringEINTR(() => error.As(syscall.Mkdir(longName, syscallMode(perm)))!);

    if (e != null) {
        return error.As(addr(new PathError(Op:"mkdir",Path:name,Err:e))!)!;
    }
    if (!supportsCreateWithStickyBit && perm & ModeSticky != 0) {
        e = setStickyBit(name);

        if (e != null) {
            Remove(name);
            return error.As(e)!;
        }
    }
    return error.As(null!)!;
}

// setStickyBit adds ModeSticky to the permission bits of path, non atomic.
private static error setStickyBit(@string name) {
    var (fi, err) = Stat(name);
    if (err != null) {
        return error.As(err)!;
    }
    return error.As(Chmod(name, fi.Mode() | ModeSticky))!;
}

// Chdir changes the current working directory to the named directory.
// If there is an error, it will be of type *PathError.
public static error Chdir(@string dir) {
    {
        var e = syscall.Chdir(dir);

        if (e != null) {
            testlog.Open(dir); // observe likely non-existent directory
            return error.As(addr(new PathError(Op:"chdir",Path:dir,Err:e))!)!;
        }
    }
    {
        var log = testlog.Logger();

        if (log != null) {
            var (wd, err) = Getwd();
            if (err == null) {
                log.Chdir(wd);
            }
        }
    }
    return error.As(null!)!;
}

// Open opens the named file for reading. If successful, methods on
// the returned file can be used for reading; the associated file
// descriptor has mode O_RDONLY.
// If there is an error, it will be of type *PathError.
public static (ptr<File>, error) Open(@string name) {
    ptr<File> _p0 = default!;
    error _p0 = default!;

    return _addr_OpenFile(name, O_RDONLY, 0)!;
}

// Create creates or truncates the named file. If the file already exists,
// it is truncated. If the file does not exist, it is created with mode 0666
// (before umask). If successful, methods on the returned File can
// be used for I/O; the associated file descriptor has mode O_RDWR.
// If there is an error, it will be of type *PathError.
public static (ptr<File>, error) Create(@string name) {
    ptr<File> _p0 = default!;
    error _p0 = default!;

    return _addr_OpenFile(name, O_RDWR | O_CREATE | O_TRUNC, 0666)!;
}

// OpenFile is the generalized open call; most users will use Open
// or Create instead. It opens the named file with specified flag
// (O_RDONLY etc.). If the file does not exist, and the O_CREATE flag
// is passed, it is created with mode perm (before umask). If successful,
// methods on the returned File can be used for I/O.
// If there is an error, it will be of type *PathError.
public static (ptr<File>, error) OpenFile(@string name, nint flag, FileMode perm) {
    ptr<File> _p0 = default!;
    error _p0 = default!;

    testlog.Open(name);
    var (f, err) = openFileNolog(name, flag, perm);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    f.appendMode = flag & O_APPEND != 0;

    return (_addr_f!, error.As(null!)!);
}

// lstat is overridden in tests.
private static var lstat = Lstat;

// Rename renames (moves) oldpath to newpath.
// If newpath already exists and is not a directory, Rename replaces it.
// OS-specific restrictions may apply when oldpath and newpath are in different directories.
// If there is an error, it will be of type *LinkError.
public static error Rename(@string oldpath, @string newpath) {
    return error.As(rename(oldpath, newpath))!;
}

// Many functions in package syscall return a count of -1 instead of 0.
// Using fixCount(call()) instead of call() corrects the count.
private static (nint, error) fixCount(nint n, error err) {
    nint _p0 = default;
    error _p0 = default!;

    if (n < 0) {
        n = 0;
    }
    return (n, error.As(err)!);
}

// wrapErr wraps an error that occurred during an operation on an open file.
// It passes io.EOF through unchanged, otherwise converts
// poll.ErrFileClosing to ErrClosed and wraps the error in a PathError.
private static error wrapErr(this ptr<File> _addr_f, @string op, error err) {
    ref File f = ref _addr_f.val;

    if (err == null || err == io.EOF) {
        return error.As(err)!;
    }
    if (err == poll.ErrFileClosing) {
        err = ErrClosed;
    }
    return error.As(addr(new PathError(Op:op,Path:f.name,Err:err))!)!;
}

// TempDir returns the default directory to use for temporary files.
//
// On Unix systems, it returns $TMPDIR if non-empty, else /tmp.
// On Windows, it uses GetTempPath, returning the first non-empty
// value from %TMP%, %TEMP%, %USERPROFILE%, or the Windows directory.
// On Plan 9, it returns /tmp.
//
// The directory is neither guaranteed to exist nor have accessible
// permissions.
public static @string TempDir() {
    return tempDir();
}

// UserCacheDir returns the default root directory to use for user-specific
// cached data. Users should create their own application-specific subdirectory
// within this one and use that.
//
// On Unix systems, it returns $XDG_CACHE_HOME as specified by
// https://specifications.freedesktop.org/basedir-spec/basedir-spec-latest.html if
// non-empty, else $HOME/.cache.
// On Darwin, it returns $HOME/Library/Caches.
// On Windows, it returns %LocalAppData%.
// On Plan 9, it returns $home/lib/cache.
//
// If the location cannot be determined (for example, $HOME is not defined),
// then it will return an error.
public static (@string, error) UserCacheDir() {
    @string _p0 = default;
    error _p0 = default!;

    @string dir = default;

    switch (runtime.GOOS) {
        case "windows": 
            dir = Getenv("LocalAppData");
            if (dir == "") {
                return ("", error.As(errors.New("%LocalAppData% is not defined"))!);
            }
            break;
        case "darwin": 

        case "ios": 
            dir = Getenv("HOME");
            if (dir == "") {
                return ("", error.As(errors.New("$HOME is not defined"))!);
            }
            dir += "/Library/Caches";
            break;
        case "plan9": 
            dir = Getenv("home");
            if (dir == "") {
                return ("", error.As(errors.New("$home is not defined"))!);
            }
            dir += "/lib/cache";
            break;
        default: // Unix
            dir = Getenv("XDG_CACHE_HOME");
            if (dir == "") {
                dir = Getenv("HOME");
                if (dir == "") {
                    return ("", error.As(errors.New("neither $XDG_CACHE_HOME nor $HOME are defined"))!);
                }
                dir += "/.cache";
            }
            break;
    }

    return (dir, error.As(null!)!);
}

// UserConfigDir returns the default root directory to use for user-specific
// configuration data. Users should create their own application-specific
// subdirectory within this one and use that.
//
// On Unix systems, it returns $XDG_CONFIG_HOME as specified by
// https://specifications.freedesktop.org/basedir-spec/basedir-spec-latest.html if
// non-empty, else $HOME/.config.
// On Darwin, it returns $HOME/Library/Application Support.
// On Windows, it returns %AppData%.
// On Plan 9, it returns $home/lib.
//
// If the location cannot be determined (for example, $HOME is not defined),
// then it will return an error.
public static (@string, error) UserConfigDir() {
    @string _p0 = default;
    error _p0 = default!;

    @string dir = default;

    switch (runtime.GOOS) {
        case "windows": 
            dir = Getenv("AppData");
            if (dir == "") {
                return ("", error.As(errors.New("%AppData% is not defined"))!);
            }
            break;
        case "darwin": 

        case "ios": 
            dir = Getenv("HOME");
            if (dir == "") {
                return ("", error.As(errors.New("$HOME is not defined"))!);
            }
            dir += "/Library/Application Support";
            break;
        case "plan9": 
            dir = Getenv("home");
            if (dir == "") {
                return ("", error.As(errors.New("$home is not defined"))!);
            }
            dir += "/lib";
            break;
        default: // Unix
            dir = Getenv("XDG_CONFIG_HOME");
            if (dir == "") {
                dir = Getenv("HOME");
                if (dir == "") {
                    return ("", error.As(errors.New("neither $XDG_CONFIG_HOME nor $HOME are defined"))!);
                }
                dir += "/.config";
            }
            break;
    }

    return (dir, error.As(null!)!);
}

// UserHomeDir returns the current user's home directory.
//
// On Unix, including macOS, it returns the $HOME environment variable.
// On Windows, it returns %USERPROFILE%.
// On Plan 9, it returns the $home environment variable.
public static (@string, error) UserHomeDir() {
    @string _p0 = default;
    error _p0 = default!;

    @string env = "HOME";
    @string enverr = "$HOME";
    switch (runtime.GOOS) {
        case "windows": 
            (env, enverr) = ("USERPROFILE", "%userprofile%");
            break;
        case "plan9": 
            (env, enverr) = ("home", "$home");
            break;
    }
    {
        var v = Getenv(env);

        if (v != "") {
            return (v, error.As(null!)!);
        }
    } 
    // On some geese the home directory is not always defined.
    switch (runtime.GOOS) {
        case "android": 
            return ("/sdcard", error.As(null!)!);
            break;
        case "ios": 
            return ("/", error.As(null!)!);
            break;
    }
    return ("", error.As(errors.New(enverr + " is not defined"))!);
}

// Chmod changes the mode of the named file to mode.
// If the file is a symbolic link, it changes the mode of the link's target.
// If there is an error, it will be of type *PathError.
//
// A different subset of the mode bits are used, depending on the
// operating system.
//
// On Unix, the mode's permission bits, ModeSetuid, ModeSetgid, and
// ModeSticky are used.
//
// On Windows, only the 0200 bit (owner writable) of mode is used; it
// controls whether the file's read-only attribute is set or cleared.
// The other bits are currently unused. For compatibility with Go 1.12
// and earlier, use a non-zero mode. Use mode 0400 for a read-only
// file and 0600 for a readable+writable file.
//
// On Plan 9, the mode's permission bits, ModeAppend, ModeExclusive,
// and ModeTemporary are used.
public static error Chmod(@string name, FileMode mode) {
    return error.As(chmod(name, mode))!;
}

// Chmod changes the mode of the file to mode.
// If there is an error, it will be of type *PathError.
private static error Chmod(this ptr<File> _addr_f, FileMode mode) {
    ref File f = ref _addr_f.val;

    return error.As(f.chmod(mode))!;
}

// SetDeadline sets the read and write deadlines for a File.
// It is equivalent to calling both SetReadDeadline and SetWriteDeadline.
//
// Only some kinds of files support setting a deadline. Calls to SetDeadline
// for files that do not support deadlines will return ErrNoDeadline.
// On most systems ordinary files do not support deadlines, but pipes do.
//
// A deadline is an absolute time after which I/O operations fail with an
// error instead of blocking. The deadline applies to all future and pending
// I/O, not just the immediately following call to Read or Write.
// After a deadline has been exceeded, the connection can be refreshed
// by setting a deadline in the future.
//
// If the deadline is exceeded a call to Read or Write or to other I/O
// methods will return an error that wraps ErrDeadlineExceeded.
// This can be tested using errors.Is(err, os.ErrDeadlineExceeded).
// That error implements the Timeout method, and calling the Timeout
// method will return true, but there are other possible errors for which
// the Timeout will return true even if the deadline has not been exceeded.
//
// An idle timeout can be implemented by repeatedly extending
// the deadline after successful Read or Write calls.
//
// A zero value for t means I/O operations will not time out.
private static error SetDeadline(this ptr<File> _addr_f, time.Time t) {
    ref File f = ref _addr_f.val;

    return error.As(f.setDeadline(t))!;
}

// SetReadDeadline sets the deadline for future Read calls and any
// currently-blocked Read call.
// A zero value for t means Read will not time out.
// Not all files support setting deadlines; see SetDeadline.
private static error SetReadDeadline(this ptr<File> _addr_f, time.Time t) {
    ref File f = ref _addr_f.val;

    return error.As(f.setReadDeadline(t))!;
}

// SetWriteDeadline sets the deadline for any future Write calls and any
// currently-blocked Write call.
// Even if Write times out, it may return n > 0, indicating that
// some of the data was successfully written.
// A zero value for t means Write will not time out.
// Not all files support setting deadlines; see SetDeadline.
private static error SetWriteDeadline(this ptr<File> _addr_f, time.Time t) {
    ref File f = ref _addr_f.val;

    return error.As(f.setWriteDeadline(t))!;
}

// SyscallConn returns a raw file.
// This implements the syscall.Conn interface.
private static (syscall.RawConn, error) SyscallConn(this ptr<File> _addr_f) {
    syscall.RawConn _p0 = default;
    error _p0 = default!;
    ref File f = ref _addr_f.val;

    {
        var err = f.checkValid("SyscallConn");

        if (err != null) {
            return (null, error.As(err)!);
        }
    }
    return newRawConn(f);
}

// isWindowsNulName reports whether name is os.DevNull ('NUL') on Windows.
// True is returned if name is 'NUL' whatever the case.
private static bool isWindowsNulName(@string name) {
    if (len(name) != 3) {
        return false;
    }
    if (name[0] != 'n' && name[0] != 'N') {
        return false;
    }
    if (name[1] != 'u' && name[1] != 'U') {
        return false;
    }
    if (name[2] != 'l' && name[2] != 'L') {
        return false;
    }
    return true;
}

// DirFS returns a file system (an fs.FS) for the tree of files rooted at the directory dir.
//
// Note that DirFS("/prefix") only guarantees that the Open calls it makes to the
// operating system will begin with "/prefix": DirFS("/prefix").Open("file") is the
// same as os.Open("/prefix/file"). So if /prefix/file is a symbolic link pointing outside
// the /prefix tree, then using DirFS does not stop the access any more than using
// os.Open does. DirFS is therefore not a general substitute for a chroot-style security
// mechanism when the directory tree contains arbitrary content.
public static fs.FS DirFS(@string dir) {
    return dirFS(dir);
}

private static bool containsAny(@string s, @string chars) {
    for (nint i = 0; i < len(s); i++) {
        for (nint j = 0; j < len(chars); j++) {
            if (s[i] == chars[j]) {
                return true;
            }
        }
    }
    return false;
}

private partial struct dirFS { // : @string
}

private static (fs.File, error) Open(this dirFS dir, @string name) {
    fs.File _p0 = default;
    error _p0 = default!;

    if (!fs.ValidPath(name) || runtime.GOOS == "windows" && containsAny(name, "\\:")) {
        return (null, error.As(addr(new PathError(Op:"open",Path:name,Err:ErrInvalid))!)!);
    }
    var (f, err) = Open(string(dir) + "/" + name);
    if (err != null) {
        return (null, error.As(err)!); // nil fs.File
    }
    return (f, error.As(null!)!);
}

private static (fs.FileInfo, error) Stat(this dirFS dir, @string name) {
    fs.FileInfo _p0 = default;
    error _p0 = default!;

    if (!fs.ValidPath(name) || runtime.GOOS == "windows" && containsAny(name, "\\:")) {
        return (null, error.As(addr(new PathError(Op:"stat",Path:name,Err:ErrInvalid))!)!);
    }
    var (f, err) = Stat(string(dir) + "/" + name);
    if (err != null) {
        return (null, error.As(err)!);
    }
    return (f, error.As(null!)!);
}

// ReadFile reads the named file and returns the contents.
// A successful call returns err == nil, not err == EOF.
// Because ReadFile reads the whole file, it does not treat an EOF from Read
// as an error to be reported.
public static (slice<byte>, error) ReadFile(@string name) => func((defer, _, _) => {
    slice<byte> _p0 = default;
    error _p0 = default!;

    var (f, err) = Open(name);
    if (err != null) {
        return (null, error.As(err)!);
    }
    defer(f.Close());

    nint size = default;
    {
        var (info, err) = f.Stat();

        if (err == null) {
            var size64 = info.Size();
            if (int64(int(size64)) == size64) {
                size = int(size64);
            }
        }
    }
    size++; // one byte for final read at EOF

    // If a file claims a small size, read at least 512 bytes.
    // In particular, files in Linux's /proc claim size 0 but
    // then do not work right if read in small pieces,
    // so an initial read of 1 byte would not work correctly.
    if (size < 512) {
        size = 512;
    }
    var data = make_slice<byte>(0, size);
    while (true) {
        if (len(data) >= cap(data)) {
            var d = append(data[..(int)cap(data)], 0);
            data = d[..(int)len(data)];
        }
        var (n, err) = f.Read(data[(int)len(data)..(int)cap(data)]);
        data = data[..(int)len(data) + n];
        if (err != null) {
            if (err == io.EOF) {
                err = null;
            }
            return (data, error.As(err)!);
        }
    }
});

// WriteFile writes data to the named file, creating it if necessary.
// If the file does not exist, WriteFile creates it with permissions perm (before umask);
// otherwise WriteFile truncates it before writing, without changing permissions.
public static error WriteFile(@string name, slice<byte> data, FileMode perm) {
    var (f, err) = OpenFile(name, O_WRONLY | O_CREATE | O_TRUNC, perm);
    if (err != null) {
        return error.As(err)!;
    }
    _, err = f.Write(data);
    {
        var err1 = f.Close();

        if (err1 != null && err == null) {
            err = err1;
        }
    }
    return error.As(err)!;
}

} // end os_package
