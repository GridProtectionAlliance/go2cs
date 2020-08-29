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
// package os -- go2cs converted at 2020 August 29 08:43:55 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\file.go
using errors = go.errors_package;
using poll = go.@internal.poll_package;
using testlog = go.@internal.testlog_package;
using io = go.io_package;
using syscall = go.syscall_package;
using time = go.time_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        // Name returns the name of the file as presented to Open.
        private static @string Name(this ref File f)
        {
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
        public static readonly long O_RDONLY = syscall.O_RDONLY; // open the file read-only.
        public static readonly long O_WRONLY = syscall.O_WRONLY; // open the file write-only.
        public static readonly long O_RDWR = syscall.O_RDWR; // open the file read-write.
        // The remaining values may be or'ed in to control behavior.
        public static readonly long O_APPEND = syscall.O_APPEND; // append data to the file when writing.
        public static readonly long O_CREATE = syscall.O_CREAT; // create a new file if none exists.
        public static readonly long O_EXCL = syscall.O_EXCL; // used with O_CREATE, file must not exist.
        public static readonly long O_SYNC = syscall.O_SYNC; // open for synchronous I/O.
        public static readonly long O_TRUNC = syscall.O_TRUNC; // if possible, truncate file when opened.

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

        private static @string Error(this ref LinkError e)
        {
            return e.Op + " " + e.Old + " " + e.New + ": " + e.Err.Error();
        }

        // Read reads up to len(b) bytes from the File.
        // It returns the number of bytes read and any error encountered.
        // At end of file, Read returns 0, io.EOF.
        private static (long, error) Read(this ref File f, slice<byte> b)
        {
            {
                var err = f.checkValid("read");

                if (err != null)
                {
                    return (0L, err);
                }

            }
            var (n, e) = f.read(b);
            return (n, f.wrapErr("read", e));
        }

        // ReadAt reads len(b) bytes from the File starting at byte offset off.
        // It returns the number of bytes read and the error, if any.
        // ReadAt always returns a non-nil error when n < len(b).
        // At end of file, that error is io.EOF.
        private static (long, error) ReadAt(this ref File f, slice<byte> b, long off)
        {
            {
                var err = f.checkValid("read");

                if (err != null)
                {
                    return (0L, err);
                }

            }

            if (off < 0L)
            {
                return (0L, ref new PathError("readat",f.name,errors.New("negative offset")));
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

            return;
        }

        // Write writes len(b) bytes to the File.
        // It returns the number of bytes written and an error, if any.
        // Write returns a non-nil error when n != len(b).
        private static (long, error) Write(this ref File f, slice<byte> b)
        {
            {
                var err = f.checkValid("write");

                if (err != null)
                {
                    return (0L, err);
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
            return (n, err);
        }

        // WriteAt writes len(b) bytes to the File starting at byte offset off.
        // It returns the number of bytes written and an error, if any.
        // WriteAt returns a non-nil error when n != len(b).
        private static (long, error) WriteAt(this ref File f, slice<byte> b, long off)
        {
            {
                var err = f.checkValid("write");

                if (err != null)
                {
                    return (0L, err);
                }

            }

            if (off < 0L)
            {
                return (0L, ref new PathError("writeat",f.name,errors.New("negative offset")));
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

            return;
        }

        // Seek sets the offset for the next Read or Write on file to offset, interpreted
        // according to whence: 0 means relative to the origin of the file, 1 means
        // relative to the current offset, and 2 means relative to the end.
        // It returns the new offset and an error, if any.
        // The behavior of Seek on a file opened with O_APPEND is not specified.
        private static (long, error) Seek(this ref File f, long offset, long whence)
        {
            {
                var err = f.checkValid("seek");

                if (err != null)
                {
                    return (0L, err);
                }

            }
            var (r, e) = f.seek(offset, whence);
            if (e == null && f.dirinfo != null && r != 0L)
            {
                e = syscall.EISDIR;
            }
            if (e != null)
            {
                return (0L, f.wrapErr("seek", e));
            }
            return (r, null);
        }

        // WriteString is like Write, but writes the contents of string s rather than
        // a slice of bytes.
        private static (long, error) WriteString(this ref File f, @string s)
        {
            return f.Write((slice<byte>)s);
        }

        // Mkdir creates a new directory with the specified name and permission
        // bits (before umask).
        // If there is an error, it will be of type *PathError.
        public static error Mkdir(@string name, FileMode perm)
        {
            var e = syscall.Mkdir(fixLongPath(name), syscallMode(perm));

            if (e != null)
            {
                return error.As(ref new PathError("mkdir",name,e));
            } 

            // mkdir(2) itself won't handle the sticky bit on *BSD and Solaris
            if (!supportsCreateWithStickyBit && perm & ModeSticky != 0L)
            {
                Chmod(name, perm);
            }
            return error.As(null);
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
                    return error.As(ref new PathError("chdir",dir,e));
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
            return error.As(null);
        }

        // Open opens the named file for reading. If successful, methods on
        // the returned file can be used for reading; the associated file
        // descriptor has mode O_RDONLY.
        // If there is an error, it will be of type *PathError.
        public static (ref File, error) Open(@string name)
        {
            return OpenFile(name, O_RDONLY, 0L);
        }

        // Create creates the named file with mode 0666 (before umask), truncating
        // it if it already exists. If successful, methods on the returned
        // File can be used for I/O; the associated file descriptor has mode
        // O_RDWR.
        // If there is an error, it will be of type *PathError.
        public static (ref File, error) Create(@string name)
        {
            return OpenFile(name, O_RDWR | O_CREATE | O_TRUNC, 0666L);
        }

        // OpenFile is the generalized open call; most users will use Open
        // or Create instead. It opens the named file with specified flag
        // (O_RDONLY etc.) and perm (before umask), if applicable. If successful,
        // methods on the returned File can be used for I/O.
        // If there is an error, it will be of type *PathError.
        public static (ref File, error) OpenFile(@string name, long flag, FileMode perm)
        {
            testlog.Open(name);
            return openFileNolog(name, flag, perm);
        }

        // lstat is overridden in tests.
        private static var lstat = Lstat;

        // Rename renames (moves) oldpath to newpath.
        // If newpath already exists and is not a directory, Rename replaces it.
        // OS-specific restrictions may apply when oldpath and newpath are in different directories.
        // If there is an error, it will be of type *LinkError.
        public static error Rename(@string oldpath, @string newpath)
        {
            return error.As(rename(oldpath, newpath));
        }

        // Many functions in package syscall return a count of -1 instead of 0.
        // Using fixCount(call()) instead of call() corrects the count.
        private static (long, error) fixCount(long n, error err)
        {
            if (n < 0L)
            {
                n = 0L;
            }
            return (n, err);
        }

        // wrapErr wraps an error that occurred during an operation on an open file.
        // It passes io.EOF through unchanged, otherwise converts
        // poll.ErrFileClosing to ErrClosed and wraps the error in a PathError.
        private static error wrapErr(this ref File f, @string op, error err)
        {
            if (err == null || err == io.EOF)
            {
                return error.As(err);
            }
            if (err == poll.ErrFileClosing)
            {
                err = ErrClosed;
            }
            return error.As(ref new PathError(op,f.name,err));
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
        // On Windows, the mode must be non-zero but otherwise only the 0200
        // bit (owner writable) of mode is used; it controls whether the
        // file's read-only attribute is set or cleared. attribute. The other
        // bits are currently unused. Use mode 0400 for a read-only file and
        // 0600 for a readable+writable file.
        //
        // On Plan 9, the mode's permission bits, ModeAppend, ModeExclusive,
        // and ModeTemporary are used.
        public static error Chmod(@string name, FileMode mode)
        {
            return error.As(chmod(name, mode));
        }

        // Chmod changes the mode of the file to mode.
        // If there is an error, it will be of type *PathError.
        private static error Chmod(this ref File f, FileMode mode)
        {
            return error.As(f.chmod(mode));
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
        // An error returned after a timeout fails will implement the
        // Timeout method, and calling the Timeout method will return true.
        // The PathError and SyscallError types implement the Timeout method.
        // In general, call IsTimeout to test whether an error indicates a timeout.
        //
        // An idle timeout can be implemented by repeatedly extending
        // the deadline after successful Read or Write calls.
        //
        // A zero value for t means I/O operations will not time out.
        private static error SetDeadline(this ref File f, time.Time t)
        {
            return error.As(f.setDeadline(t));
        }

        // SetReadDeadline sets the deadline for future Read calls and any
        // currently-blocked Read call.
        // A zero value for t means Read will not time out.
        // Not all files support setting deadlines; see SetDeadline.
        private static error SetReadDeadline(this ref File f, time.Time t)
        {
            return error.As(f.setReadDeadline(t));
        }

        // SetWriteDeadline sets the deadline for any future Write calls and any
        // currently-blocked Write call.
        // Even if Write times out, it may return n > 0, indicating that
        // some of the data was successfully written.
        // A zero value for t means Write will not time out.
        // Not all files support setting deadlines; see SetDeadline.
        private static error SetWriteDeadline(this ref File f, time.Time t)
        {
            return error.As(f.setWriteDeadline(t));
        }
    }
}
