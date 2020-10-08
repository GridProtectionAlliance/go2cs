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
// package os -- go2cs converted at 2020 October 08 03:44:38 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\file.go
using errors = go.errors_package;
using poll = go.@internal.poll_package;
using testlog = go.@internal.testlog_package;
using io = go.io_package;
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using time = go.time_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        // Name returns the name of the file as presented to Open.
        private static @string Name(this ptr<File> _addr_f)
        {
            ref File f = ref _addr_f.val;

            return f.name;
        }

        // Stdin, Stdout, and Stderr are open Files pointing to the standard input,
        // standard output, and standard error file descriptors.
        //
        // Note that the Go runtime writes to standard error for panics and crashes;
        // closing Stderr may cause those messages to go elsewhere, perhaps
        // to a file opened later.
        public static var Stdin = NewFile(uintptr(syscall.Stdin), "/dev/stdin");        public static var Stdout = NewFile(uintptr(syscall.Stdout), "/dev/stdout");        public static var Stderr = NewFile(uintptr(syscall.Stderr), "/dev/stderr");

        // Flags to OpenFile wrapping those of the underlying system. Not all
        // flags may be implemented on a given system.
 
        // Exactly one of O_RDONLY, O_WRONLY, or O_RDWR must be specified.
        public static readonly long O_RDONLY = (long)syscall.O_RDONLY; // open the file read-only.
        public static readonly long O_WRONLY = (long)syscall.O_WRONLY; // open the file write-only.
        public static readonly long O_RDWR = (long)syscall.O_RDWR; // open the file read-write.
        // The remaining values may be or'ed in to control behavior.
        public static readonly long O_APPEND = (long)syscall.O_APPEND; // append data to the file when writing.
        public static readonly long O_CREATE = (long)syscall.O_CREAT; // create a new file if none exists.
        public static readonly long O_EXCL = (long)syscall.O_EXCL; // used with O_CREATE, file must not exist.
        public static readonly long O_SYNC = (long)syscall.O_SYNC; // open for synchronous I/O.
        public static readonly long O_TRUNC = (long)syscall.O_TRUNC; // truncate regular writable file when opened.

        // Seek whence values.
        //
        // Deprecated: Use io.SeekStart, io.SeekCurrent, and io.SeekEnd.
        public static readonly long SEEK_SET = 0L; // seek relative to the origin of the file
        public static readonly long SEEK_CUR = 1L; // seek relative to the current offset
        public static readonly long SEEK_END = 2L; // seek relative to the end

        // LinkError records an error during a link or symlink or rename
        // system call and the paths that caused it.
        public partial struct LinkError
        {
            public @string Op;
            public @string Old;
            public @string New;
            public error Err;
        }

        private static @string Error(this ptr<LinkError> _addr_e)
        {
            ref LinkError e = ref _addr_e.val;

            return e.Op + " " + e.Old + " " + e.New + ": " + e.Err.Error();
        }

        private static error Unwrap(this ptr<LinkError> _addr_e)
        {
            ref LinkError e = ref _addr_e.val;

            return error.As(e.Err)!;
        }

        // Read reads up to len(b) bytes from the File.
        // It returns the number of bytes read and any error encountered.
        // At end of file, Read returns 0, io.EOF.
        private static (long, error) Read(this ptr<File> _addr_f, slice<byte> b)
        {
            long n = default;
            error err = default!;
            ref File f = ref _addr_f.val;

            {
                var err = f.checkValid("read");

                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

            }

            var (n, e) = f.read(b);
            return (n, error.As(f.wrapErr("read", e))!);

        }

        // ReadAt reads len(b) bytes from the File starting at byte offset off.
        // It returns the number of bytes read and the error, if any.
        // ReadAt always returns a non-nil error when n < len(b).
        // At end of file, that error is io.EOF.
        private static (long, error) ReadAt(this ptr<File> _addr_f, slice<byte> b, long off)
        {
            long n = default;
            error err = default!;
            ref File f = ref _addr_f.val;

            {
                var err = f.checkValid("read");

                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

            }


            if (off < 0L)
            {
                return (0L, error.As(addr(new PathError("readat",f.name,errors.New("negative offset")))!)!);
            }

            while (len(b) > 0L)
            {
                var (m, e) = f.pread(b, off);
                if (e != null)
                {
                    err = f.wrapErr("read", e);
                    break;
                }

                n += m;
                b = b[m..];
                off += int64(m);

            }

            return ;

        }

        // ReadFrom implements io.ReaderFrom.
        private static (long, error) ReadFrom(this ptr<File> _addr_f, io.Reader r)
        {
            long n = default;
            error err = default!;
            ref File f = ref _addr_f.val;

            {
                var err = f.checkValid("write");

                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

            }

            var (n, handled, e) = f.readFrom(r);
            if (!handled)
            {
                return genericReadFrom(_addr_f, r); // without wrapping
            }

            return (n, error.As(f.wrapErr("write", e))!);

        }

        private static (long, error) genericReadFrom(ptr<File> _addr_f, io.Reader r)
        {
            long _p0 = default;
            error _p0 = default!;
            ref File f = ref _addr_f.val;

            return io.Copy(new onlyWriter(f), r);
        }

        private partial struct onlyWriter : io.Writer
        {
            public ref io.Writer Writer => ref Writer_val;
        }

        // Write writes len(b) bytes to the File.
        // It returns the number of bytes written and an error, if any.
        // Write returns a non-nil error when n != len(b).
        private static (long, error) Write(this ptr<File> _addr_f, slice<byte> b)
        {
            long n = default;
            error err = default!;
            ref File f = ref _addr_f.val;

            {
                var err = f.checkValid("write");

                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

            }

            var (n, e) = f.write(b);
            if (n < 0L)
            {
                n = 0L;
            }

            if (n != len(b))
            {
                err = io.ErrShortWrite;
            }

            epipecheck(f, e);

            if (e != null)
            {
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
        private static (long, error) WriteAt(this ptr<File> _addr_f, slice<byte> b, long off)
        {
            long n = default;
            error err = default!;
            ref File f = ref _addr_f.val;

            {
                var err = f.checkValid("write");

                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

            }

            if (f.appendMode)
            {
                return (0L, error.As(errWriteAtInAppendMode)!);
            }

            if (off < 0L)
            {
                return (0L, error.As(addr(new PathError("writeat",f.name,errors.New("negative offset")))!)!);
            }

            while (len(b) > 0L)
            {
                var (m, e) = f.pwrite(b, off);
                if (e != null)
                {
                    err = f.wrapErr("write", e);
                    break;
                }

                n += m;
                b = b[m..];
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
        private static (long, error) Seek(this ptr<File> _addr_f, long offset, long whence)
        {
            long ret = default;
            error err = default!;
            ref File f = ref _addr_f.val;

            {
                var err = f.checkValid("seek");

                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

            }

            var (r, e) = f.seek(offset, whence);
            if (e == null && f.dirinfo != null && r != 0L)
            {
                e = syscall.EISDIR;
            }

            if (e != null)
            {
                return (0L, error.As(f.wrapErr("seek", e))!);
            }

            return (r, error.As(null!)!);

        }

        // WriteString is like Write, but writes the contents of string s rather than
        // a slice of bytes.
        private static (long, error) WriteString(this ptr<File> _addr_f, @string s)
        {
            long n = default;
            error err = default!;
            ref File f = ref _addr_f.val;

            return f.Write((slice<byte>)s);
        }

        // Mkdir creates a new directory with the specified name and permission
        // bits (before umask).
        // If there is an error, it will be of type *PathError.
        public static error Mkdir(@string name, FileMode perm)
        {
            if (runtime.GOOS == "windows" && isWindowsNulName(name))
            {
                return error.As(addr(new PathError("mkdir",name,syscall.ENOTDIR))!)!;
            }

            var e = syscall.Mkdir(fixLongPath(name), syscallMode(perm));

            if (e != null)
            {
                return error.As(addr(new PathError("mkdir",name,e))!)!;
            } 

            // mkdir(2) itself won't handle the sticky bit on *BSD and Solaris
            if (!supportsCreateWithStickyBit && perm & ModeSticky != 0L)
            {
                e = setStickyBit(name);

                if (e != null)
                {
                    Remove(name);
                    return error.As(e)!;
                }

            }

            return error.As(null!)!;

        }

        // setStickyBit adds ModeSticky to the permission bits of path, non atomic.
        private static error setStickyBit(@string name)
        {
            var (fi, err) = Stat(name);
            if (err != null)
            {
                return error.As(err)!;
            }

            return error.As(Chmod(name, fi.Mode() | ModeSticky))!;

        }

        // Chdir changes the current working directory to the named directory.
        // If there is an error, it will be of type *PathError.
        public static error Chdir(@string dir)
        {
            {
                var e = syscall.Chdir(dir);

                if (e != null)
                {
                    testlog.Open(dir); // observe likely non-existent directory
                    return error.As(addr(new PathError("chdir",dir,e))!)!;

                }

            }

            {
                var log = testlog.Logger();

                if (log != null)
                {
                    var (wd, err) = Getwd();
                    if (err == null)
                    {
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
        public static (ptr<File>, error) Open(@string name)
        {
            ptr<File> _p0 = default!;
            error _p0 = default!;

            return _addr_OpenFile(name, O_RDONLY, 0L)!;
        }

        // Create creates or truncates the named file. If the file already exists,
        // it is truncated. If the file does not exist, it is created with mode 0666
        // (before umask). If successful, methods on the returned File can
        // be used for I/O; the associated file descriptor has mode O_RDWR.
        // If there is an error, it will be of type *PathError.
        public static (ptr<File>, error) Create(@string name)
        {
            ptr<File> _p0 = default!;
            error _p0 = default!;

            return _addr_OpenFile(name, O_RDWR | O_CREATE | O_TRUNC, 0666L)!;
        }

        // OpenFile is the generalized open call; most users will use Open
        // or Create instead. It opens the named file with specified flag
        // (O_RDONLY etc.). If the file does not exist, and the O_CREATE flag
        // is passed, it is created with mode perm (before umask). If successful,
        // methods on the returned File can be used for I/O.
        // If there is an error, it will be of type *PathError.
        public static (ptr<File>, error) OpenFile(@string name, long flag, FileMode perm)
        {
            ptr<File> _p0 = default!;
            error _p0 = default!;

            testlog.Open(name);
            var (f, err) = openFileNolog(name, flag, perm);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            f.appendMode = flag & O_APPEND != 0L;

            return (_addr_f!, error.As(null!)!);

        }

        // lstat is overridden in tests.
        private static var lstat = Lstat;

        // Rename renames (moves) oldpath to newpath.
        // If newpath already exists and is not a directory, Rename replaces it.
        // OS-specific restrictions may apply when oldpath and newpath are in different directories.
        // If there is an error, it will be of type *LinkError.
        public static error Rename(@string oldpath, @string newpath)
        {
            return error.As(rename(oldpath, newpath))!;
        }

        // Many functions in package syscall return a count of -1 instead of 0.
        // Using fixCount(call()) instead of call() corrects the count.
        private static (long, error) fixCount(long n, error err)
        {
            long _p0 = default;
            error _p0 = default!;

            if (n < 0L)
            {
                n = 0L;
            }

            return (n, error.As(err)!);

        }

        // wrapErr wraps an error that occurred during an operation on an open file.
        // It passes io.EOF through unchanged, otherwise converts
        // poll.ErrFileClosing to ErrClosed and wraps the error in a PathError.
        private static error wrapErr(this ptr<File> _addr_f, @string op, error err)
        {
            ref File f = ref _addr_f.val;

            if (err == null || err == io.EOF)
            {
                return error.As(err)!;
            }

            if (err == poll.ErrFileClosing)
            {
                err = ErrClosed;
            }

            return error.As(addr(new PathError(op,f.name,err))!)!;

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
        public static @string TempDir()
        {
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
        public static (@string, error) UserCacheDir()
        {
            @string _p0 = default;
            error _p0 = default!;

            @string dir = default;

            switch (runtime.GOOS)
            {
                case "windows": 
                    dir = Getenv("LocalAppData");
                    if (dir == "")
                    {
                        return ("", error.As(errors.New("%LocalAppData% is not defined"))!);
                    }

                    break;
                case "darwin": 
                    dir = Getenv("HOME");
                    if (dir == "")
                    {
                        return ("", error.As(errors.New("$HOME is not defined"))!);
                    }

                    dir += "/Library/Caches";
                    break;
                case "plan9": 
                    dir = Getenv("home");
                    if (dir == "")
                    {
                        return ("", error.As(errors.New("$home is not defined"))!);
                    }

                    dir += "/lib/cache";
                    break;
                default: // Unix
                    dir = Getenv("XDG_CACHE_HOME");
                    if (dir == "")
                    {
                        dir = Getenv("HOME");
                        if (dir == "")
                        {
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
        public static (@string, error) UserConfigDir()
        {
            @string _p0 = default;
            error _p0 = default!;

            @string dir = default;

            switch (runtime.GOOS)
            {
                case "windows": 
                    dir = Getenv("AppData");
                    if (dir == "")
                    {
                        return ("", error.As(errors.New("%AppData% is not defined"))!);
                    }

                    break;
                case "darwin": 
                    dir = Getenv("HOME");
                    if (dir == "")
                    {
                        return ("", error.As(errors.New("$HOME is not defined"))!);
                    }

                    dir += "/Library/Application Support";
                    break;
                case "plan9": 
                    dir = Getenv("home");
                    if (dir == "")
                    {
                        return ("", error.As(errors.New("$home is not defined"))!);
                    }

                    dir += "/lib";
                    break;
                default: // Unix
                    dir = Getenv("XDG_CONFIG_HOME");
                    if (dir == "")
                    {
                        dir = Getenv("HOME");
                        if (dir == "")
                        {
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
        public static (@string, error) UserHomeDir()
        {
            @string _p0 = default;
            error _p0 = default!;

            @string env = "HOME";
            @string enverr = "$HOME";
            switch (runtime.GOOS)
            {
                case "windows": 
                    env = "USERPROFILE";
                    enverr = "%userprofile%";
                    break;
                case "plan9": 
                    env = "home";
                    enverr = "$home";
                    break;
            }
            {
                var v = Getenv(env);

                if (v != "")
                {
                    return (v, error.As(null!)!);
                } 
                // On some geese the home directory is not always defined.

            } 
            // On some geese the home directory is not always defined.
            switch (runtime.GOOS)
            {
                case "android": 
                    return ("/sdcard", error.As(null!)!);
                    break;
                case "darwin": 
                    if (runtime.GOARCH == "arm64")
                    {
                        return ("/", error.As(null!)!);
                    }

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
        public static error Chmod(@string name, FileMode mode)
        {
            return error.As(chmod(name, mode))!;
        }

        // Chmod changes the mode of the file to mode.
        // If there is an error, it will be of type *PathError.
        private static error Chmod(this ptr<File> _addr_f, FileMode mode)
        {
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
        private static error SetDeadline(this ptr<File> _addr_f, time.Time t)
        {
            ref File f = ref _addr_f.val;

            return error.As(f.setDeadline(t))!;
        }

        // SetReadDeadline sets the deadline for future Read calls and any
        // currently-blocked Read call.
        // A zero value for t means Read will not time out.
        // Not all files support setting deadlines; see SetDeadline.
        private static error SetReadDeadline(this ptr<File> _addr_f, time.Time t)
        {
            ref File f = ref _addr_f.val;

            return error.As(f.setReadDeadline(t))!;
        }

        // SetWriteDeadline sets the deadline for any future Write calls and any
        // currently-blocked Write call.
        // Even if Write times out, it may return n > 0, indicating that
        // some of the data was successfully written.
        // A zero value for t means Write will not time out.
        // Not all files support setting deadlines; see SetDeadline.
        private static error SetWriteDeadline(this ptr<File> _addr_f, time.Time t)
        {
            ref File f = ref _addr_f.val;

            return error.As(f.setWriteDeadline(t))!;
        }

        // SyscallConn returns a raw file.
        // This implements the syscall.Conn interface.
        private static (syscall.RawConn, error) SyscallConn(this ptr<File> _addr_f)
        {
            syscall.RawConn _p0 = default;
            error _p0 = default!;
            ref File f = ref _addr_f.val;

            {
                var err = f.checkValid("SyscallConn");

                if (err != null)
                {
                    return (null, error.As(err)!);
                }

            }

            return newRawConn(f);

        }

        // isWindowsNulName reports whether name is os.DevNull ('NUL') on Windows.
        // True is returned if name is 'NUL' whatever the case.
        private static bool isWindowsNulName(@string name)
        {
            if (len(name) != 3L)
            {
                return false;
            }

            if (name[0L] != 'n' && name[0L] != 'N')
            {
                return false;
            }

            if (name[1L] != 'u' && name[1L] != 'U')
            {
                return false;
            }

            if (name[2L] != 'l' && name[2L] != 'L')
            {
                return false;
            }

            return true;

        }
    }
}
