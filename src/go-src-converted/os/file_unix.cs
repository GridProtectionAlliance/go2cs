// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd js,wasm linux netbsd openbsd solaris

// package os -- go2cs converted at 2020 October 08 03:44:45 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\file_unix.go
using poll = go.@internal.poll_package;
using unix = go.@internal.syscall.unix_package;
using io = go.io_package;
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
                // However, if the old name and new name are not the same, yet
                // they refer to the same file, it implies a case-only
                // rename on a case-insensitive filesystem, which is ok.
                {
                    var (ofi, err) = Lstat(oldname);

                    if (err != null)
                    {
                        {
                            ptr<PathError> (pe, ok) = err._<ptr<PathError>>();

                            if (ok)
                            {
                                err = pe.Err;
                            }

                        }

                        return error.As(addr(new LinkError("rename",oldname,newname,err))!)!;

                    }
                    else if (newname == oldname || !SameFile(fi, ofi))
                    {
                        return error.As(addr(new LinkError("rename",oldname,newname,syscall.EEXIST))!)!;
                    }


                }

            }

            err = syscall.Rename(oldname, newname);
            if (err != null)
            {
                return error.As(addr(new LinkError("rename",oldname,newname,err))!)!;
            }

            return error.As(null!)!;

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
            public bool appendMode; // whether file is opened for appending
        }

        // Fd returns the integer Unix file descriptor referencing the open file.
        // The file descriptor is valid only until f.Close is called or f is garbage collected.
        // On Unix systems this will cause the SetDeadline methods to stop working.
        private static System.UIntPtr Fd(this ptr<File> _addr_f)
        {
            ref File f = ref _addr_f.val;

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
        // descriptor. On Unix systems, if the file descriptor is in
        // non-blocking mode, NewFile will attempt to return a pollable File
        // (one for which the SetDeadline methods work).
        public static ptr<File> NewFile(System.UIntPtr fd, @string name)
        {
            var kind = kindNewFile;
            {
                var (nb, err) = unix.IsNonblock(int(fd));

                if (err == null && nb)
                {
                    kind = kindNonBlock;
                }

            }

            return _addr_newFile(fd, name, kind)!;

        }

        // newFileKind describes the kind of file to newFile.
        private partial struct newFileKind // : long
        {
        }

        private static readonly newFileKind kindNewFile = (newFileKind)iota;
        private static readonly var kindOpenFile = (var)0;
        private static readonly var kindPipe = (var)1;
        private static readonly var kindNonBlock = (var)2;


        // newFile is like NewFile, but if called from OpenFile or Pipe
        // (as passed in the kind parameter) it tries to add the file to
        // the runtime poller.
        private static ptr<File> newFile(System.UIntPtr fd, @string name, newFileKind kind)
        {
            var fdi = int(fd);
            if (fdi < 0L)
            {
                return _addr_null!;
            }

            ptr<File> f = addr(new File(&file{pfd:poll.FD{Sysfd:fdi,IsStream:true,ZeroReadIsEOF:true,},name:name,stdoutOrErr:fdi==1||fdi==2,}));

            var pollable = kind == kindOpenFile || kind == kindPipe || kind == kindNonBlock; 

            // If the caller passed a non-blocking filedes (kindNonBlock),
            // we assume they know what they are doing so we allow it to be
            // used with kqueue.
            if (kind == kindOpenFile)
            {
                switch (runtime.GOOS)
                {
                    case "darwin": 

                    case "dragonfly": 

                    case "freebsd": 

                    case "netbsd": 

                    case "openbsd": 
                        ref syscall.Stat_t st = ref heap(out ptr<syscall.Stat_t> _addr_st);
                        var err = syscall.Fstat(fdi, _addr_st);
                        var typ = st.Mode & syscall.S_IFMT; 
                        // Don't try to use kqueue with regular files on *BSDs.
                        // On FreeBSD a regular file is always
                        // reported as ready for writing.
                        // On Dragonfly, NetBSD and OpenBSD the fd is signaled
                        // only once as ready (both read and write).
                        // Issue 19093.
                        // Also don't add directories to the netpoller.
                        if (err == null && (typ == syscall.S_IFREG || typ == syscall.S_IFDIR))
                        {
                            pollable = false;
                        } 

                        // In addition to the behavior described above for regular files,
                        // on Darwin, kqueue does not work properly with fifos:
                        // closing the last writer does not cause a kqueue event
                        // for any readers. See issue #24164.
                        if (runtime.GOOS == "darwin" && typ == syscall.S_IFIFO)
                        {
                            pollable = false;
                        }

                        break;
                }

            }

            {
                var err__prev1 = err;

                err = f.pfd.Init("file", pollable);

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


            runtime.SetFinalizer(f.file, ptr<file>);
            return _addr_f!;

        }

        // epipecheck raises SIGPIPE if we get an EPIPE error on standard
        // output or standard error. See the SIGPIPE docs in os/signal, and
        // issue 11845.
        private static void epipecheck(ptr<File> _addr_file, error e)
        {
            ref File file = ref _addr_file.val;

            if (e == syscall.EPIPE && file.stdoutOrErr)
            {
                sigpipe();
            }

        }

        // DevNull is the name of the operating system's ``null device.''
        // On Unix-like systems, it is "/dev/null"; on Windows, "NUL".
        public static readonly @string DevNull = (@string)"/dev/null";

        // openFileNolog is the Unix implementation of OpenFile.
        // Changes here should be reflected in openFdAt, if relevant.


        // openFileNolog is the Unix implementation of OpenFile.
        // Changes here should be reflected in openFdAt, if relevant.
        private static (ptr<File>, error) openFileNolog(@string name, long flag, FileMode perm)
        {
            ptr<File> _p0 = default!;
            error _p0 = default!;

            var setSticky = false;
            if (!supportsCreateWithStickyBit && flag & O_CREATE != 0L && perm & ModeSticky != 0L)
            {
                {
                    var (_, err) = Stat(name);

                    if (IsNotExist(err))
                    {
                        setSticky = true;
                    }

                }

            }

            long r = default;
            while (true)
            {
                error e = default!;
                r, e = syscall.Open(name, flag | syscall.O_CLOEXEC, syscallMode(perm));
                if (e == null)
                {
                    break;
                } 

                // We have to check EINTR here, per issues 11180 and 39237.
                if (e == syscall.EINTR)
                {
                    continue;
                }

                return (_addr_null!, error.As(addr(new PathError("open",name,e))!)!);

            } 

            // open(2) itself won't handle the sticky bit on *BSD and Solaris
 

            // open(2) itself won't handle the sticky bit on *BSD and Solaris
            if (setSticky)
            {
                setStickyBit(name);
            } 

            // There's a race here with fork/exec, which we are
            // content to live with. See ../syscall/exec_unix.go.
            if (!supportsCloseOnExec)
            {
                syscall.CloseOnExec(r);
            }

            return (_addr_newFile(uintptr(r), name, kindOpenFile)!, error.As(null!)!);

        }

        private static error close(this ptr<file> _addr_file)
        {
            ref file file = ref _addr_file.val;

            if (file == null)
            {
                return error.As(syscall.EINVAL)!;
            }

            if (file.dirinfo != null)
            {
                file.dirinfo.close();
            }

            error err = default!;
            {
                var e = file.pfd.Close();

                if (e != null)
                {
                    if (e == poll.ErrFileClosing)
                    {
                        e = ErrClosed;
                    }

                    err = error.As(addr(new PathError("close",file.name,e)))!;

                } 

                // no need for a finalizer anymore

            } 

            // no need for a finalizer anymore
            runtime.SetFinalizer(file, null);
            return error.As(err)!;

        }

        // seek sets the offset for the next Read or Write on file to offset, interpreted
        // according to whence: 0 means relative to the origin of the file, 1 means
        // relative to the current offset, and 2 means relative to the end.
        // It returns the new offset and an error, if any.
        private static (long, error) seek(this ptr<File> _addr_f, long offset, long whence)
        {
            long ret = default;
            error err = default!;
            ref File f = ref _addr_f.val;

            if (f.dirinfo != null)
            { 
                // Free cached dirinfo, so we allocate a new one if we
                // access this file as a directory again. See #35767 and #37161.
                f.dirinfo.close();
                f.dirinfo = null;

            }

            ret, err = f.pfd.Seek(offset, whence);
            runtime.KeepAlive(f);
            return (ret, error.As(err)!);

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
                    return error.As(addr(new PathError("truncate",name,e))!)!;
                }

            }

            return error.As(null!)!;

        }

        // Remove removes the named file or (empty) directory.
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
                return error.As(null!)!;
            }

            var e1 = syscall.Rmdir(name);
            if (e1 == null)
            {
                return error.As(null!)!;
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

            return error.As(addr(new PathError("remove",name,e))!)!;

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
                return error.As(addr(new LinkError("link",oldname,newname,e))!)!;
            }

            return error.As(null!)!;

        }

        // Symlink creates newname as a symbolic link to oldname.
        // If there is an error, it will be of type *LinkError.
        public static error Symlink(@string oldname, @string newname)
        {
            var e = syscall.Symlink(oldname, newname);
            if (e != null)
            {
                return error.As(addr(new LinkError("symlink",oldname,newname,e))!)!;
            }

            return error.As(null!)!;

        }

        private static (slice<FileInfo>, error) readdir(this ptr<File> _addr_f, long n)
        {
            slice<FileInfo> fi = default;
            error err = default!;
            ref File f = ref _addr_f.val;

            var dirname = f.name;
            if (dirname == "")
            {
                dirname = ".";
            }

            var (names, err) = f.Readdirnames(n);
            fi = make_slice<FileInfo>(0L, len(names));
            foreach (var (_, filename) in names)
            {
                var (fip, lerr) = lstat(dirname + "/" + filename);
                if (IsNotExist(lerr))
                { 
                    // File disappeared between readdir + stat.
                    // Just treat it as if it didn't exist.
                    continue;

                }

                if (lerr != null)
                {
                    return (fi, error.As(lerr)!);
                }

                fi = append(fi, fip);

            }
            if (len(fi) == 0L && err == null && n > 0L)
            { 
                // Per File.Readdir, the slice must be non-empty or err
                // must be non-nil if n > 0.
                err = io.EOF;

            }

            return (fi, error.As(err)!);

        }

        // Readlink returns the destination of the named symbolic link.
        // If there is an error, it will be of type *PathError.
        public static (@string, error) Readlink(@string name)
        {
            @string _p0 = default;
            error _p0 = default!;

            {
                long len = 128L;

                while (>>MARKER:FOREXPRESSION_LEVEL_1<<)
                {
                    var b = make_slice<byte>(len);
                    var (n, e) = fixCount(syscall.Readlink(name, b)); 
                    // buffer too small
                    if (runtime.GOOS == "aix" && e == syscall.ERANGE)
                    {
                        continue;
                    len *= 2L;
                    }

                    if (e != null)
                    {
                        return ("", error.As(addr(new PathError("readlink",name,e))!)!);
                    }

                    if (n < len)
                    {
                        return (string(b[0L..n]), error.As(null!)!);
                    }

                }

            }

        }
    }
}
