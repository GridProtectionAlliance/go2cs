// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package os provides a platform-independent interface to operating system
// functionality. The design is Unix-like, although the error handling is
// Go-like; failing calls return values of type error rather than error numbers.
// Often, more information is available within the error. For example,
// if a call that takes a file name fails, such as [Open] or [Stat], the error
// will include the failing file name when printed and will be of type
// [*PathError], which may be unpacked for more information.
//
// The os interface is intended to be uniform across all operating systems.
// Features not generally available appear in the system-specific package syscall.
//
// Here is a simple example, opening a file and reading some of it.
//
//	file, err := os.Open("file.go") // For read access.
//	if err != nil {
//		log.Fatal(err)
//	}
//
// If the open fails, the error string will be self-explanatory, like
//
//	open file.go: no such file or directory
//
// The file's data can then be read into a slice of bytes. Read and
// Write take their byte counts from the length of the argument slice.
//
//	data := make([]byte, 100)
//	count, err := file.Read(data)
//	if err != nil {
//		log.Fatal(err)
//	}
//	fmt.Printf("read %d bytes: %q\n", count, data[:count])
//
// # Concurrency
//
// The methods of [File] correspond to file system operations. All are
// safe for concurrent use. The maximum number of concurrent
// operations on a File may be limited by the OS or the system. The
// number should be high, but exceeding it may degrade performance or
// cause other issues.
namespace go;

using errors = errors_package;
using filepathlite = @internal.filepathlite_package;
using poll = @internal.poll_package;
using testlog = @internal.testlog_package;
using io = io_package;
using fs = io.fs_package;
using runtime = runtime_package;
using syscall = syscall_package;
using time = time_package;
using @unsafe = unsafe_package;
using @internal;
using io;

partial class os_package {

// Name returns the name of the file as presented to Open.
//
// It is safe to call Name after [Close].
[GoRecv] public static @string Name(this ref File f) {
    return f.name;
}

// Stdin, Stdout, and Stderr are open Files pointing to the standard input,
// standard output, and standard error file descriptors.
//
// Note that the Go runtime writes to standard error for panics and crashes;
// closing Stderr may cause those messages to go elsewhere, perhaps
// to a file opened later.
public static ж<File> Stdin = NewFile(((uintptr)syscall.Stdin), "/dev/stdin"u8);

public static ж<File> Stdout = NewFile(((uintptr)syscall.Stdout), "/dev/stdout"u8);

public static ж<File> Stderr = NewFile(((uintptr)syscall.Stderr), "/dev/stderr"u8);

// Flags to OpenFile wrapping those of the underlying system. Not all
// flags may be implemented on a given system.
public const nint O_RDONLY = /* syscall.O_RDONLY */ 0;             // open the file read-only.

public const nint O_WRONLY = /* syscall.O_WRONLY */ 1;             // open the file write-only.

public const nint O_RDWR = /* syscall.O_RDWR */ 2;               // open the file read-write.

public const nint O_APPEND = /* syscall.O_APPEND */ 1024;             // append data to the file when writing.

public const nint O_CREATE = /* syscall.O_CREAT */ 64;             // create a new file if none exists.

public const nint O_EXCL = /* syscall.O_EXCL */ 128;               // used with O_CREATE, file must not exist.

public const nint O_SYNC = /* syscall.O_SYNC */ 4096;               // open for synchronous I/O.

public const nint O_TRUNC = /* syscall.O_TRUNC */ 512;              // truncate regular writable file when opened.

// Seek whence values.
//
// Deprecated: Use io.SeekStart, io.SeekCurrent, and io.SeekEnd.
public const nint SEEK_SET = 0; // seek relative to the origin of the file

public const nint SEEK_CUR = 1; // seek relative to the current offset

public const nint SEEK_END = 2; // seek relative to the end

// LinkError records an error during a link or symlink or rename
// system call and the paths that caused it.
[GoType] partial struct LinkError {
    public @string Op;
    public @string Old;
    public @string New;
    public error Err;
}

[GoRecv] public static @string Error(this ref LinkError e) {
    return e.Op + " "u8 + e.Old + " "u8 + e.New + ": "u8 + e.Err.Error();
}

[GoRecv] public static error Unwrap(this ref LinkError e) {
    return e.Err;
}

// Read reads up to len(b) bytes from the File and stores them in b.
// It returns the number of bytes read and any error encountered.
// At end of file, Read returns 0, io.EOF.
[GoRecv] public static (nint n, error err) Read(this ref File f, slice<byte> b) {
    nint n = default!;
    error err = default!;

    {
        var errΔ1 = f.checkValid("read"u8); if (errΔ1 != default!) {
            return (0, errΔ1);
        }
    }
    (n, e) = f.read(b);
    return (n, f.wrapErr("read"u8, e));
}

// ReadAt reads len(b) bytes from the File starting at byte offset off.
// It returns the number of bytes read and the error, if any.
// ReadAt always returns a non-nil error when n < len(b).
// At end of file, that error is io.EOF.
[GoRecv] public static (nint n, error err) ReadAt(this ref File f, slice<byte> b, int64 off) {
    nint n = default!;
    error err = default!;

    {
        var errΔ1 = f.checkValid("read"u8); if (errΔ1 != default!) {
            return (0, errΔ1);
        }
    }
    if (off < 0) {
        return (0, new PathError{Op: "readat"u8, Path: f.name, Err: errors.New("negative offset"u8)});
    }
    while (len(b) > 0) {
        var (m, e) = f.pread(b, off);
        if (e != default!) {
            err = f.wrapErr("read"u8, e);
            break;
        }
        n += m;
        b = b[(int)(m)..];
        off += ((int64)m);
    }
    return (n, err);
}

// ReadFrom implements io.ReaderFrom.
[GoRecv] public static (int64 n, error err) ReadFrom(this ref File f, io.Reader r) {
    int64 n = default!;
    error err = default!;

    {
        var errΔ1 = f.checkValid("write"u8); if (errΔ1 != default!) {
            return (0, errΔ1);
        }
    }
    var (n, handled, e) = f.readFrom(r);
    if (!handled) {
        return genericReadFrom(f, r);
    }
    // without wrapping
    return (n, f.wrapErr("write"u8, e));
}

// noReadFrom can be embedded alongside another type to
// hide the ReadFrom method of that other type.
[GoType] partial struct noReadFrom {
}

// ReadFrom hides another ReadFrom method.
// It should never be called.
internal static (int64, error) ReadFrom(this noReadFrom _, io.Reader _) {
    throw panic("can't happen");
}

// fileWithoutReadFrom implements all the methods of *File other
// than ReadFrom. This is used to permit ReadFrom to call io.Copy
// without leading to a recursive call to ReadFrom.
[GoType] partial struct fileWithoutReadFrom {
    internal partial ref noReadFrom noReadFrom { get; }
    public partial ref ж<File> File { get; }
}

internal static (int64, error) genericReadFrom(ж<File> Ꮡf, io.Reader r) {
    ref var f = ref Ꮡf.val;

    return io.Copy(new fileWithoutReadFrom(File: f), r);
}

// Write writes len(b) bytes from b to the File.
// It returns the number of bytes written and an error, if any.
// Write returns a non-nil error when n != len(b).
[GoRecv] public static (nint n, error err) Write(this ref File f, slice<byte> b) {
    nint n = default!;
    error err = default!;

    {
        var errΔ1 = f.checkValid("write"u8); if (errΔ1 != default!) {
            return (0, errΔ1);
        }
    }
    (n, e) = f.write(b);
    if (n < 0) {
        n = 0;
    }
    if (n != len(b)) {
        err = io.ErrShortWrite;
    }
    epipecheck(f, e);
    if (e != default!) {
        err = f.wrapErr("write"u8, e);
    }
    return (n, err);
}

internal static error errWriteAtInAppendMode = errors.New("os: invalid use of WriteAt on file opened with O_APPEND"u8);

// WriteAt writes len(b) bytes to the File starting at byte offset off.
// It returns the number of bytes written and an error, if any.
// WriteAt returns a non-nil error when n != len(b).
//
// If file was opened with the O_APPEND flag, WriteAt returns an error.
[GoRecv] public static (nint n, error err) WriteAt(this ref File f, slice<byte> b, int64 off) {
    nint n = default!;
    error err = default!;

    {
        var errΔ1 = f.checkValid("write"u8); if (errΔ1 != default!) {
            return (0, errΔ1);
        }
    }
    if (f.appendMode) {
        return (0, errWriteAtInAppendMode);
    }
    if (off < 0) {
        return (0, new PathError{Op: "writeat"u8, Path: f.name, Err: errors.New("negative offset"u8)});
    }
    while (len(b) > 0) {
        var (m, e) = f.pwrite(b, off);
        if (e != default!) {
            err = f.wrapErr("write"u8, e);
            break;
        }
        n += m;
        b = b[(int)(m)..];
        off += ((int64)m);
    }
    return (n, err);
}

// WriteTo implements io.WriterTo.
[GoRecv] public static (int64 n, error err) WriteTo(this ref File f, io.Writer w) {
    int64 n = default!;
    error err = default!;

    {
        var errΔ1 = f.checkValid("read"u8); if (errΔ1 != default!) {
            return (0, errΔ1);
        }
    }
    var (n, handled, e) = f.writeTo(w);
    if (handled) {
        return (n, f.wrapErr("read"u8, e));
    }
    return genericWriteTo(f, w);
}

// without wrapping

// noWriteTo can be embedded alongside another type to
// hide the WriteTo method of that other type.
[GoType] partial struct noWriteTo {
}

// WriteTo hides another WriteTo method.
// It should never be called.
internal static (int64, error) WriteTo(this noWriteTo _, io.Writer _) {
    throw panic("can't happen");
}

// fileWithoutWriteTo implements all the methods of *File other
// than WriteTo. This is used to permit WriteTo to call io.Copy
// without leading to a recursive call to WriteTo.
[GoType] partial struct fileWithoutWriteTo {
    internal partial ref noWriteTo noWriteTo { get; }
    public partial ref ж<File> File { get; }
}

internal static (int64, error) genericWriteTo(ж<File> Ꮡf, io.Writer w) {
    ref var f = ref Ꮡf.val;

    return io.Copy(w, new fileWithoutWriteTo(File: f));
}

// Seek sets the offset for the next Read or Write on file to offset, interpreted
// according to whence: 0 means relative to the origin of the file, 1 means
// relative to the current offset, and 2 means relative to the end.
// It returns the new offset and an error, if any.
// The behavior of Seek on a file opened with O_APPEND is not specified.
[GoRecv] public static (int64 ret, error err) Seek(this ref File f, int64 offset, nint whence) {
    int64 ret = default!;
    error err = default!;

    {
        var errΔ1 = f.checkValid("seek"u8); if (errΔ1 != default!) {
            return (0, errΔ1);
        }
    }
    var (r, e) = f.seek(offset, whence);
    if (e == default! && f.dirinfo.Load() != nil && r != 0) {
        e = syscall.EISDIR;
    }
    if (e != default!) {
        return (0, f.wrapErr("seek"u8, e));
    }
    return (r, default!);
}

// WriteString is like Write, but writes the contents of string s rather than
// a slice of bytes.
[GoRecv] public static (nint n, error err) WriteString(this ref File f, @string s) {
    nint n = default!;
    error err = default!;

    var b = @unsafe.Slice(@unsafe.StringData(s), len(s));
    return f.Write(b);
}

// Mkdir creates a new directory with the specified name and permission
// bits (before umask).
// If there is an error, it will be of type *PathError.
public static error Mkdir(@string name, FileMode perm) {
    @string longName = fixLongPath(name);
    var e = ignoringEINTR(() => syscall.Mkdir(longName, syscallMode(perm)));
    if (e != default!) {
        return new PathError{Op: "mkdir"u8, Path: name, Err: e};
    }
    // mkdir(2) itself won't handle the sticky bit on *BSD and Solaris
    if (!supportsCreateWithStickyBit && (FileMode)(perm & ModeSticky) != 0) {
        e = setStickyBit(name);
        if (e != default!) {
            Remove(name);
            return e;
        }
    }
    return default!;
}

// setStickyBit adds ModeSticky to the permission bits of path, non atomic.
internal static error setStickyBit(@string name) {
    (fi, err) = Stat(name);
    if (err != default!) {
        return err;
    }
    return Chmod(name, (fs.FileMode)(fi.Mode() | ModeSticky));
}

// Chdir changes the current working directory to the named directory.
// If there is an error, it will be of type *PathError.
public static error Chdir(@string dir) {
    {
        var e = syscall.Chdir(dir); if (e != default!) {
            testlog.Open(dir);
            // observe likely non-existent directory
            return new PathError{Op: "chdir"u8, Path: dir, Err: e};
        }
    }
    if (runtime.GOOS == "windows"u8) {
        getwdCache.Lock();
        getwdCache.dir = dir;
        getwdCache.Unlock();
    }
    {
        var log = testlog.Logger(); if (log != default!) {
            var (wd, err) = Getwd();
            if (err == default!) {
                log.Chdir(wd);
            }
        }
    }
    return default!;
}

// Open opens the named file for reading. If successful, methods on
// the returned file can be used for reading; the associated file
// descriptor has mode O_RDONLY.
// If there is an error, it will be of type *PathError.
public static (ж<File>, error) Open(@string name) {
    return OpenFile(name, O_RDONLY, 0);
}

// Create creates or truncates the named file. If the file already exists,
// it is truncated. If the file does not exist, it is created with mode 0o666
// (before umask). If successful, methods on the returned File can
// be used for I/O; the associated file descriptor has mode O_RDWR.
// If there is an error, it will be of type *PathError.
public static (ж<File>, error) Create(@string name) {
    return OpenFile(name, (nint)((nint)(O_RDWR | O_CREATE) | O_TRUNC), 438);
}

// OpenFile is the generalized open call; most users will use Open
// or Create instead. It opens the named file with specified flag
// (O_RDONLY etc.). If the file does not exist, and the O_CREATE flag
// is passed, it is created with mode perm (before umask). If successful,
// methods on the returned File can be used for I/O.
// If there is an error, it will be of type *PathError.
public static (ж<File>, error) OpenFile(@string name, nint flag, FileMode perm) {
    testlog.Open(name);
    (f, err) = openFileNolog(name, flag, perm);
    if (err != default!) {
        return (default!, err);
    }
    f.appendMode = (nint)(flag & O_APPEND) != 0;
    return (f, default!);
}

// openDir opens a file which is assumed to be a directory. As such, it skips
// the syscalls that make the file descriptor non-blocking as these take time
// and will fail on file descriptors for directories.
internal static (ж<File>, error) openDir(@string name) {
    testlog.Open(name);
    return openDirNolog(name);
}

// lstat is overridden in tests.
internal static Func<@string, (FileInfo, error)> lstat = Lstat;

// Rename renames (moves) oldpath to newpath.
// If newpath already exists and is not a directory, Rename replaces it.
// OS-specific restrictions may apply when oldpath and newpath are in different directories.
// Even within the same directory, on non-Unix platforms Rename is not an atomic operation.
// If there is an error, it will be of type *LinkError.
public static error Rename(@string oldpath, @string newpath) {
    return rename(oldpath, newpath);
}

// Readlink returns the destination of the named symbolic link.
// If there is an error, it will be of type *PathError.
//
// If the link destination is relative, Readlink returns the relative path
// without resolving it to an absolute one.
public static (@string, error) Readlink(@string name) {
    return readlink(name);
}

// Many functions in package syscall return a count of -1 instead of 0.
// Using fixCount(call()) instead of call() corrects the count.
internal static (nint, error) fixCount(nint n, error err) {
    if (n < 0) {
        n = 0;
    }
    return (n, err);
}

// checkWrapErr is the test hook to enable checking unexpected wrapped errors of poll.ErrFileClosing.
// It is set to true in the export_test.go for tests (including fuzz tests).
internal static bool checkWrapErr = false;

// wrapErr wraps an error that occurred during an operation on an open file.
// It passes io.EOF through unchanged, otherwise converts
// poll.ErrFileClosing to ErrClosed and wraps the error in a PathError.
[GoRecv] internal static error wrapErr(this ref File f, @string op, error err) {
    if (err == default! || AreEqual(err, io.EOF)) {
        return err;
    }
    if (AreEqual(err, poll.ErrFileClosing)){
        err = ErrClosed;
    } else 
    if (checkWrapErr && errors.Is(err, poll.ErrFileClosing)) {
        throw panic("unexpected error wrapping poll.ErrFileClosing: "u8 + err.Error());
    }
    return new PathError{Op: op, Path: f.name, Err: err};
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
    @string dir = default!;
    var exprᴛ1 = runtime.GOOS;
    if (exprᴛ1 == "windows"u8) {
        dir = Getenv("LocalAppData"u8);
        if (dir == ""u8) {
            return ("", errors.New("%LocalAppData% is not defined"u8));
        }
    }
    if (exprᴛ1 == "darwin"u8 || exprᴛ1 == "ios"u8) {
        dir = Getenv("HOME"u8);
        if (dir == ""u8) {
            return ("", errors.New("$HOME is not defined"u8));
        }
        dir += "/Library/Caches"u8;
    }
    else if (exprᴛ1 == "plan9"u8) {
        dir = Getenv("home"u8);
        if (dir == ""u8) {
            return ("", errors.New("$home is not defined"u8));
        }
        dir += "/lib/cache"u8;
    }
    else { /* default: */
        dir = Getenv("XDG_CACHE_HOME"u8);
        if (dir == ""u8) {
            // Unix
            dir = Getenv("HOME"u8);
            if (dir == ""u8) {
                return ("", errors.New("neither $XDG_CACHE_HOME nor $HOME are defined"u8));
            }
            dir += "/.cache"u8;
        }
    }

    return (dir, default!);
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
    @string dir = default!;
    var exprᴛ1 = runtime.GOOS;
    if (exprᴛ1 == "windows"u8) {
        dir = Getenv("AppData"u8);
        if (dir == ""u8) {
            return ("", errors.New("%AppData% is not defined"u8));
        }
    }
    if (exprᴛ1 == "darwin"u8 || exprᴛ1 == "ios"u8) {
        dir = Getenv("HOME"u8);
        if (dir == ""u8) {
            return ("", errors.New("$HOME is not defined"u8));
        }
        dir += "/Library/Application Support"u8;
    }
    else if (exprᴛ1 == "plan9"u8) {
        dir = Getenv("home"u8);
        if (dir == ""u8) {
            return ("", errors.New("$home is not defined"u8));
        }
        dir += "/lib"u8;
    }
    else { /* default: */
        dir = Getenv("XDG_CONFIG_HOME"u8);
        if (dir == ""u8) {
            // Unix
            dir = Getenv("HOME"u8);
            if (dir == ""u8) {
                return ("", errors.New("neither $XDG_CONFIG_HOME nor $HOME are defined"u8));
            }
            dir += "/.config"u8;
        }
    }

    return (dir, default!);
}

// UserHomeDir returns the current user's home directory.
//
// On Unix, including macOS, it returns the $HOME environment variable.
// On Windows, it returns %USERPROFILE%.
// On Plan 9, it returns the $home environment variable.
//
// If the expected variable is not set in the environment, UserHomeDir
// returns either a platform-specific default value or a non-nil error.
public static (@string, error) UserHomeDir() {
    @string env = "HOME"u8;
    @string enverr = "$HOME"u8;
    var exprᴛ1 = runtime.GOOS;
    if (exprᴛ1 == "windows"u8) {
        (env, enverr) = ("USERPROFILE"u8, "%userprofile%"u8);
    }
    else if (exprᴛ1 == "plan9"u8) {
        (env, enverr) = ("home"u8, "$home"u8);
    }

    {
        @string v = Getenv(env); if (v != ""u8) {
            return (v, default!);
        }
    }
    // On some geese the home directory is not always defined.
    var exprᴛ2 = runtime.GOOS;
    if (exprᴛ2 == "android"u8) {
        return ("/sdcard", default!);
    }
    if (exprᴛ2 == "ios"u8) {
        return ("/", default!);
    }

    return ("", errors.New(enverr + " is not defined"u8));
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
// On Windows, only the 0o200 bit (owner writable) of mode is used; it
// controls whether the file's read-only attribute is set or cleared.
// The other bits are currently unused. For compatibility with Go 1.12
// and earlier, use a non-zero mode. Use mode 0o400 for a read-only
// file and 0o600 for a readable+writable file.
//
// On Plan 9, the mode's permission bits, ModeAppend, ModeExclusive,
// and ModeTemporary are used.
public static error Chmod(@string name, FileMode mode) {
    return chmod(name, mode);
}

// Chmod changes the mode of the file to mode.
// If there is an error, it will be of type *PathError.
[GoRecv] public static error Chmod(this ref File f, FileMode mode) {
    return f.chmod(mode);
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
[GoRecv] public static error SetDeadline(this ref File f, time.Time t) {
    return f.setDeadline(t);
}

// SetReadDeadline sets the deadline for future Read calls and any
// currently-blocked Read call.
// A zero value for t means Read will not time out.
// Not all files support setting deadlines; see SetDeadline.
[GoRecv] public static error SetReadDeadline(this ref File f, time.Time t) {
    return f.setReadDeadline(t);
}

// SetWriteDeadline sets the deadline for any future Write calls and any
// currently-blocked Write call.
// Even if Write times out, it may return n > 0, indicating that
// some of the data was successfully written.
// A zero value for t means Write will not time out.
// Not all files support setting deadlines; see SetDeadline.
[GoRecv] public static error SetWriteDeadline(this ref File f, time.Time t) {
    return f.setWriteDeadline(t);
}

// SyscallConn returns a raw file.
// This implements the syscall.Conn interface.
[GoRecv] public static (syscall.RawConn, error) SyscallConn(this ref File f) {
    {
        var err = f.checkValid("SyscallConn"u8); if (err != default!) {
            return (default!, err);
        }
    }
    return newRawConn(f);
}

// DirFS returns a file system (an fs.FS) for the tree of files rooted at the directory dir.
//
// Note that DirFS("/prefix") only guarantees that the Open calls it makes to the
// operating system will begin with "/prefix": DirFS("/prefix").Open("file") is the
// same as os.Open("/prefix/file"). So if /prefix/file is a symbolic link pointing outside
// the /prefix tree, then using DirFS does not stop the access any more than using
// os.Open does. Additionally, the root of the fs.FS returned for a relative path,
// DirFS("prefix"), will be affected by later calls to Chdir. DirFS is therefore not
// a general substitute for a chroot-style security mechanism when the directory tree
// contains arbitrary content.
//
// The directory dir must not be "".
//
// The result implements [io/fs.StatFS], [io/fs.ReadFileFS] and
// [io/fs.ReadDirFS].
public static fs.FS DirFS(@string dir) {
    return ((dirFS)dir);
}

[GoType("@string")] partial struct dirFS;

internal static (fs.File, error) Open(this dirFS dir, @string name) {
    var (fullname, err) = dir.join(name);
    if (err != default!) {
        return (default!, new PathError{Op: "open"u8, Path: name, Err: err});
    }
    (f, err) = Open(fullname);
    if (err != default!) {
        // DirFS takes a string appropriate for GOOS,
        // while the name argument here is always slash separated.
        // dir.join will have mixed the two; undo that for
        // error reporting.
        err._<PathError.val>().Path = name;
        return (default!, err);
    }
    return (~f, default!);
}

// The ReadFile method calls the [ReadFile] function for the file
// with the given name in the directory. The function provides
// robust handling for small files and special file systems.
// Through this method, dirFS implements [io/fs.ReadFileFS].
internal static (slice<byte>, error) ReadFile(this dirFS dir, @string name) {
    var (fullname, err) = dir.join(name);
    if (err != default!) {
        return (default!, new PathError{Op: "readfile"u8, Path: name, Err: err});
    }
    (b, err) = ReadFile(fullname);
    if (err != default!) {
        {
            var (e, ok) = err._<PathError.val>(ᐧ); if (ok) {
                // See comment in dirFS.Open.
                e.val.Path = name;
            }
        }
        return (default!, err);
    }
    return (b, default!);
}

// ReadDir reads the named directory, returning all its directory entries sorted
// by filename. Through this method, dirFS implements [io/fs.ReadDirFS].
internal static (slice<DirEntry>, error) ReadDir(this dirFS dir, @string name) {
    var (fullname, err) = dir.join(name);
    if (err != default!) {
        return (default!, new PathError{Op: "readdir"u8, Path: name, Err: err});
    }
    (entries, err) = ReadDir(fullname);
    if (err != default!) {
        {
            var (e, ok) = err._<PathError.val>(ᐧ); if (ok) {
                // See comment in dirFS.Open.
                e.val.Path = name;
            }
        }
        return (default!, err);
    }
    return (entries, default!);
}

internal static (fs.FileInfo, error) Stat(this dirFS dir, @string name) {
    var (fullname, err) = dir.join(name);
    if (err != default!) {
        return (default!, new PathError{Op: "stat"u8, Path: name, Err: err});
    }
    (f, err) = Stat(fullname);
    if (err != default!) {
        // See comment in dirFS.Open.
        err._<PathError.val>().Path = name;
        return (default!, err);
    }
    return (f, default!);
}

// join returns the path for name in dir.
internal static (@string, error) join(this dirFS dir, @string name) {
    if (dir == ""u8) {
        return ("", errors.New("os: DirFS with empty root"u8));
    }
    (name, err) = filepathlite.Localize(name);
    if (err != default!) {
        return ("", ErrInvalid);
    }
    if (IsPathSeparator(dir[len(dir) - 1])) {
        return (((@string)dir) + name, default!);
    }
    return (((@string)dir) + ((@string)PathSeparator) + name, default!);
}

// ReadFile reads the named file and returns the contents.
// A successful call returns err == nil, not err == EOF.
// Because ReadFile reads the whole file, it does not treat an EOF from Read
// as an error to be reported.
public static (slice<byte>, error) ReadFile(@string name) => func((defer, _) => {
    (f, err) = Open(name);
    if (err != default!) {
        return (default!, err);
    }
    var fʗ1 = f;
    defer(fʗ1.Close);
    nint size = default!;
    {
        (info, errΔ1) = f.Stat(); if (errΔ1 == default!) {
            var size64 = info.Size();
            if (((int64)((nint)size64)) == size64) {
                size = ((nint)size64);
            }
        }
    }
    size++;
    // one byte for final read at EOF
    // If a file claims a small size, read at least 512 bytes.
    // In particular, files in Linux's /proc claim size 0 but
    // then do not work right if read in small pieces,
    // so an initial read of 1 byte would not work correctly.
    if (size < 512) {
        size = 512;
    }
    var data = new slice<byte>(0, size);
    while (ᐧ) {
        var (n, errΔ2) = f.Read(data[(int)(len(data))..(int)(cap(data))]);
        data = data[..(int)(len(data) + n)];
        if (errΔ2 != default!) {
            if (AreEqual(errΔ2, io.EOF)) {
                err = default!;
            }
            return (data, errΔ2);
        }
        if (len(data) >= cap(data)) {
            var d = append(data[..(int)(cap(data))], 0);
            data = d[..(int)(len(data))];
        }
    }
});

// WriteFile writes data to the named file, creating it if necessary.
// If the file does not exist, WriteFile creates it with permissions perm (before umask);
// otherwise WriteFile truncates it before writing, without changing permissions.
// Since WriteFile requires multiple system calls to complete, a failure mid-operation
// can leave the file in a partially written state.
public static error WriteFile(@string name, slice<byte> data, FileMode perm) {
    (f, err) = OpenFile(name, (nint)((nint)(O_WRONLY | O_CREATE) | O_TRUNC), perm);
    if (err != default!) {
        return err;
    }
    (_, err) = f.Write(data);
    {
        var err1 = f.Close(); if (err1 != default! && err == default!) {
            err = err1;
        }
    }
    return err;
}

} // end os_package
