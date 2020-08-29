// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux nacl netbsd openbsd solaris

// package os -- go2cs converted at 2020 August 29 08:44:02 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\file_unix.go
using poll = go.@internal.poll_package;
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        // fixLongPath is a noop on non-Windows platforms.
        private static @string fixLongPath(@string path)
        {
            return path;
        }

        private static error rename(@string oldname, @string newname)
        {
            var (fi, err) = Lstat(newname);
            if (err == null && fi.IsDir())
            { 
                // There are two independent errors this function can return:
                // one for a bad oldname, and one for a bad newname.
                // At this point we've determined the newname is bad.
                // But just in case oldname is also bad, prioritize returning
                // the oldname error because that's what we did historically.
                {
                    var (_, err) = Lstat(oldname);

                    if (err != null)
                    {
                        {
                            ref PathError (pe, ok) = err._<ref PathError>();

                            if (ok)
                            {
                                err = pe.Err;
                            }

                        }
                        return error.As(ref new LinkError("rename",oldname,newname,err));
                    }

                }
                return error.As(ref new LinkError("rename",oldname,newname,syscall.EEXIST));
            }
            err = syscall.Rename(oldname, newname);
            if (err != null)
            {
                return error.As(ref new LinkError("rename",oldname,newname,err));
            }
            return error.As(null);
        }

        // file is the real representation of *File.
        // The extra level of indirection ensures that no clients of os
        // can overwrite this data, which could cause the finalizer
        // to close the wrong file descriptor.
        private partial struct file
        {
            public poll.FD pfd;
            public @string name;
            public ptr<dirInfo> dirinfo; // nil unless directory being read
            public bool nonblock; // whether we set nonblocking mode
            public bool stdoutOrErr; // whether this is stdout or stderr
        }

        // Fd returns the integer Unix file descriptor referencing the open file.
        // The file descriptor is valid only until f.Close is called or f is garbage collected.
        // On Unix systems this will cause the SetDeadline methods to stop working.
        private static System.UIntPtr Fd(this ref File f)
        {
            if (f == null)
            {
                return ~(uintptr(0L));
            } 

            // If we put the file descriptor into nonblocking mode,
            // then set it to blocking mode before we return it,
            // because historically we have always returned a descriptor
            // opened in blocking mode. The File will continue to work,
            // but any blocking operation will tie up a thread.
            if (f.nonblock)
            {
                f.pfd.SetBlocking();
            }
            return uintptr(f.pfd.Sysfd);
        }

        // NewFile returns a new File with the given file descriptor and
        // name. The returned value will be nil if fd is not a valid file
        // descriptor.
        public static ref File NewFile(System.UIntPtr fd, @string name)
        {
            return newFile(fd, name, kindNewFile);
        }

        // newFileKind describes the kind of file to newFile.
        private partial struct newFileKind // : long
        {
        }

        private static readonly newFileKind kindNewFile = iota;
        private static readonly var kindOpenFile = 0;
        private static readonly var kindPipe = 1;

        // newFile is like NewFile, but if called from OpenFile or Pipe
        // (as passed in the kind parameter) it tries to add the file to
        // the runtime poller.
        private static ref File newFile(System.UIntPtr fd, @string name, newFileKind kind)
        {
            var fdi = int(fd);
            if (fdi < 0L)
            {
                return null;
            }
            File f = ref new File(&file{pfd:poll.FD{Sysfd:fdi,IsStream:true,ZeroReadIsEOF:true,},name:name,stdoutOrErr:fdi==1||fdi==2,}); 

            // Don't try to use kqueue with regular files on FreeBSD.
            // It crashes the system unpredictably while running all.bash.
            // Issue 19093.
            if (runtime.GOOS == "freebsd" && kind == kindOpenFile)
            {
                kind = kindNewFile;
            }
            var pollable = kind == kindOpenFile || kind == kindPipe;
            {
                var err__prev1 = err;

                var err = f.pfd.Init("file", pollable);

                if (err != null)
                { 
                    // An error here indicates a failure to register
                    // with the netpoll system. That can happen for
                    // a file descriptor that is not supported by
                    // epoll/kqueue; for example, disk files on
                    // GNU/Linux systems. We assume that any real error
                    // will show up in later I/O.
                }
                else if (pollable)
                { 
                    // We successfully registered with netpoll, so put
                    // the file into nonblocking mode.
                    {
                        var err__prev3 = err;

                        err = syscall.SetNonblock(fdi, true);

                        if (err == null)
                        {
                            f.nonblock = true;
                        }

                        err = err__prev3;

                    }
                }

                err = err__prev1;

            }

            runtime.SetFinalizer(f.file, ref file);
            return f;
        }

        // Auxiliary information if the File describes a directory
        private partial struct dirInfo
        {
            public slice<byte> buf; // buffer for directory I/O
            public long nbuf; // length of buf; return value from Getdirentries
            public long bufp; // location of next record in buf.
        }

        // epipecheck raises SIGPIPE if we get an EPIPE error on standard
        // output or standard error. See the SIGPIPE docs in os/signal, and
        // issue 11845.
        private static void epipecheck(ref File file, error e)
        {
            if (e == syscall.EPIPE && file.stdoutOrErr)
            {
                sigpipe();
            }
        }

        // DevNull is the name of the operating system's ``null device.''
        // On Unix-like systems, it is "/dev/null"; on Windows, "NUL".
        public static readonly @string DevNull = "/dev/null";

        // openFileNolog is the Unix implementation of OpenFile.


        // openFileNolog is the Unix implementation of OpenFile.
        private static (ref File, error) openFileNolog(@string name, long flag, FileMode perm)
        {
            var chmod = false;
            if (!supportsCreateWithStickyBit && flag & O_CREATE != 0L && perm & ModeSticky != 0L)
            {
                {
                    var (_, err) = Stat(name);

                    if (IsNotExist(err))
                    {
                        chmod = true;
                    }

                }
            }
            long r = default;
            while (true)
            {
                error e = default;
                r, e = syscall.Open(name, flag | syscall.O_CLOEXEC, syscallMode(perm));
                if (e == null)
                {
                    break;
                } 

                // On OS X, sigaction(2) doesn't guarantee that SA_RESTART will cause
                // open(2) to be restarted for regular files. This is easy to reproduce on
                // fuse file systems (see http://golang.org/issue/11180).
                if (runtime.GOOS == "darwin" && e == syscall.EINTR)
                {
                    continue;
                }
                return (null, ref new PathError("open",name,e));
            } 

            // open(2) itself won't handle the sticky bit on *BSD and Solaris
 

            // open(2) itself won't handle the sticky bit on *BSD and Solaris
            if (chmod)
            {
                Chmod(name, perm);
            } 

            // There's a race here with fork/exec, which we are
            // content to live with. See ../syscall/exec_unix.go.
            if (!supportsCloseOnExec)
            {
                syscall.CloseOnExec(r);
            }
            return (newFile(uintptr(r), name, kindOpenFile), null);
        }

        // Close closes the File, rendering it unusable for I/O.
        // It returns an error, if any.
        private static error Close(this ref File f)
        {
            if (f == null)
            {
                return error.As(ErrInvalid);
            }
            return error.As(f.file.close());
        }

        private static error close(this ref file file)
        {
            if (file == null)
            {
                return error.As(syscall.EINVAL);
            }
            error err = default;
            {
                var e = file.pfd.Close();

                if (e != null)
                {
                    if (e == poll.ErrFileClosing)
                    {
                        e = ErrClosed;
                    }
                    err = error.As(ref new PathError("close",file.name,e));
                } 

                // no need for a finalizer anymore

            } 

            // no need for a finalizer anymore
            runtime.SetFinalizer(file, null);
            return error.As(err);
        }

        // read reads up to len(b) bytes from the File.
        // It returns the number of bytes read and an error, if any.
        private static (long, error) read(this ref File f, slice<byte> b)
        {
            n, err = f.pfd.Read(b);
            runtime.KeepAlive(f);
            return (n, err);
        }

        // pread reads len(b) bytes from the File starting at byte offset off.
        // It returns the number of bytes read and the error, if any.
        // EOF is signaled by a zero count with err set to nil.
        private static (long, error) pread(this ref File f, slice<byte> b, long off)
        {
            n, err = f.pfd.Pread(b, off);
            runtime.KeepAlive(f);
            return (n, err);
        }

        // write writes len(b) bytes to the File.
        // It returns the number of bytes written and an error, if any.
        private static (long, error) write(this ref File f, slice<byte> b)
        {
            n, err = f.pfd.Write(b);
            runtime.KeepAlive(f);
            return (n, err);
        }

        // pwrite writes len(b) bytes to the File starting at byte offset off.
        // It returns the number of bytes written and an error, if any.
        private static (long, error) pwrite(this ref File f, slice<byte> b, long off)
        {
            n, err = f.pfd.Pwrite(b, off);
            runtime.KeepAlive(f);
            return (n, err);
        }

        // seek sets the offset for the next Read or Write on file to offset, interpreted
        // according to whence: 0 means relative to the origin of the file, 1 means
        // relative to the current offset, and 2 means relative to the end.
        // It returns the new offset and an error, if any.
        private static (long, error) seek(this ref File f, long offset, long whence)
        {
            ret, err = f.pfd.Seek(offset, whence);
            runtime.KeepAlive(f);
            return (ret, err);
        }

        // Truncate changes the size of the named file.
        // If the file is a symbolic link, it changes the size of the link's target.
        // If there is an error, it will be of type *PathError.
        public static error Truncate(@string name, long size)
        {
            {
                var e = syscall.Truncate(name, size);

                if (e != null)
                {
                    return error.As(ref new PathError("truncate",name,e));
                }

            }
            return error.As(null);
        }

        // Remove removes the named file or directory.
        // If there is an error, it will be of type *PathError.
        public static error Remove(@string name)
        { 
            // System call interface forces us to know
            // whether name is a file or directory.
            // Try both: it is cheaper on average than
            // doing a Stat plus the right one.
            var e = syscall.Unlink(name);
            if (e == null)
            {
                return error.As(null);
            }
            var e1 = syscall.Rmdir(name);
            if (e1 == null)
            {
                return error.As(null);
            } 

            // Both failed: figure out which error to return.
            // OS X and Linux differ on whether unlink(dir)
            // returns EISDIR, so can't use that. However,
            // both agree that rmdir(file) returns ENOTDIR,
            // so we can use that to decide which error is real.
            // Rmdir might also return ENOTDIR if given a bad
            // file path, like /etc/passwd/foo, but in that case,
            // both errors will be ENOTDIR, so it's okay to
            // use the error from unlink.
            if (e1 != syscall.ENOTDIR)
            {
                e = e1;
            }
            return error.As(ref new PathError("remove",name,e));
        }

        private static @string tempDir()
        {
            var dir = Getenv("TMPDIR");
            if (dir == "")
            {
                if (runtime.GOOS == "android")
                {
                    dir = "/data/local/tmp";
                }
                else
                {
                    dir = "/tmp";
                }
            }
            return dir;
        }

        // Link creates newname as a hard link to the oldname file.
        // If there is an error, it will be of type *LinkError.
        public static error Link(@string oldname, @string newname)
        {
            var e = syscall.Link(oldname, newname);
            if (e != null)
            {
                return error.As(ref new LinkError("link",oldname,newname,e));
            }
            return error.As(null);
        }

        // Symlink creates newname as a symbolic link to oldname.
        // If there is an error, it will be of type *LinkError.
        public static error Symlink(@string oldname, @string newname)
        {
            var e = syscall.Symlink(oldname, newname);
            if (e != null)
            {
                return error.As(ref new LinkError("symlink",oldname,newname,e));
            }
            return error.As(null);
        }
    }
}
