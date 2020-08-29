// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 August 29 08:43:58 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\file_plan9.go
using poll = go.@internal.poll_package;
using io = go.io_package;
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using time = go.time_package;
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

        // file is the real representation of *File.
        // The extra level of indirection ensures that no clients of os
        // can overwrite this data, which could cause the finalizer
        // to close the wrong file descriptor.
        private partial struct file
        {
            public long fd;
            public @string name;
            public ptr<dirInfo> dirinfo; // nil unless directory being read
        }

        // Fd returns the integer Plan 9 file descriptor referencing the open file.
        // The file descriptor is valid only until f.Close is called or f is garbage collected.
        // On Unix systems this will cause the SetDeadline methods to stop working.
        private static System.UIntPtr Fd(this ref File f)
        {
            if (f == null)
            {
                return ~(uintptr(0L));
            }
            return uintptr(f.fd);
        }

        // NewFile returns a new File with the given file descriptor and
        // name. The returned value will be nil if fd is not a valid file
        // descriptor.
        public static ref File NewFile(System.UIntPtr fd, @string name)
        {
            var fdi = int(fd);
            if (fdi < 0L)
            {
                return null;
            }
            File f = ref new File(&file{fd:fdi,name:name});
            runtime.SetFinalizer(f.file, ref file);
            return f;
        }

        // Auxiliary information if the File describes a directory
        private partial struct dirInfo
        {
            public array<byte> buf; // buffer for directory I/O
            public long nbuf; // length of buf; return value from Read
            public long bufp; // location of next record in buf.
        }

        private static void epipecheck(ref File file, error e)
        {
        }

        // DevNull is the name of the operating system's ``null device.''
        // On Unix-like systems, it is "/dev/null"; on Windows, "NUL".
        public static readonly @string DevNull = "/dev/null";

        // syscallMode returns the syscall-specific mode bits from Go's portable mode bits.


        // syscallMode returns the syscall-specific mode bits from Go's portable mode bits.
        private static uint syscallMode(FileMode i)
        {
            o |= uint32(i.Perm());
            if (i & ModeAppend != 0L)
            {
                o |= syscall.DMAPPEND;
            }
            if (i & ModeExclusive != 0L)
            {
                o |= syscall.DMEXCL;
            }
            if (i & ModeTemporary != 0L)
            {
                o |= syscall.DMTMP;
            }
            return;
        }

        // openFileNolog is the Plan 9 implementation of OpenFile.
        private static (ref File, error) openFileNolog(@string name, long flag, FileMode perm)
        {
            long fd = default;            error e = default;            bool create = default;            bool excl = default;            bool trunc = default;            bool append = default;

            if (flag & O_CREATE == O_CREATE)
            {
                flag = flag & ~O_CREATE;
                create = true;
            }
            if (flag & O_EXCL == O_EXCL)
            {
                excl = true;
            }
            if (flag & O_TRUNC == O_TRUNC)
            {
                trunc = true;
            } 
            // O_APPEND is emulated on Plan 9
            if (flag & O_APPEND == O_APPEND)
            {
                flag = flag & ~O_APPEND;
                append = true;
            }
            if ((create && trunc) || excl)
            {
                fd, e = syscall.Create(name, flag, syscallMode(perm));
            }
            else
            {
                fd, e = syscall.Open(name, flag);
                if (e != null && create)
                {
                    error e1 = default;
                    fd, e1 = syscall.Create(name, flag, syscallMode(perm));
                    if (e1 == null)
                    {
                        e = error.As(null);
                    }
                }
            }
            if (e != null)
            {
                return (null, ref new PathError("open",name,e));
            }
            if (append)
            {
                _, e = syscall.Seek(fd, 0L, io.SeekEnd);

                if (e != null)
                {
                    return (null, ref new PathError("seek",name,e));
                }
            }
            return (NewFile(uintptr(fd), name), null);
        }

        // Close closes the File, rendering it unusable for I/O.
        // It returns an error, if any.
        private static error Close(this ref File f)
        {
            {
                var err = f.checkValid("close");

                if (err != null)
                {
                    return error.As(err);
                }

            }
            return error.As(f.file.close());
        }

        private static error close(this ref file file)
        {
            if (file == null || file.fd == badFd)
            {
                return error.As(ErrInvalid);
            }
            error err = default;
            {
                var e = syscall.Close(file.fd);

                if (e != null)
                {
                    err = error.As(ref new PathError("close",file.name,e));
                }

            }
            file.fd = badFd; // so it can't be closed again

            // no need for a finalizer anymore
            runtime.SetFinalizer(file, null);
            return error.As(err);
        }

        // Stat returns the FileInfo structure describing file.
        // If there is an error, it will be of type *PathError.
        private static (FileInfo, error) Stat(this ref File f)
        {
            if (f == null)
            {
                return (null, ErrInvalid);
            }
            var (d, err) = dirstat(f);
            if (err != null)
            {
                return (null, err);
            }
            return (fileInfoFromStat(d), null);
        }

        // Truncate changes the size of the file.
        // It does not change the I/O offset.
        // If there is an error, it will be of type *PathError.
        private static error Truncate(this ref File f, long size)
        {
            if (f == null)
            {
                return error.As(ErrInvalid);
            }
            syscall.Dir d = default;
            d.Null();
            d.Length = size;

            array<byte> buf = new array<byte>(syscall.STATFIXLEN);
            var (n, err) = d.Marshal(buf[..]);
            if (err != null)
            {
                return error.As(ref new PathError("truncate",f.name,err));
            }
            err = syscall.Fwstat(f.fd, buf[..n]);

            if (err != null)
            {
                return error.As(ref new PathError("truncate",f.name,err));
            }
            return error.As(null);
        }

        private static readonly var chmodMask = uint32(syscall.DMAPPEND | syscall.DMEXCL | syscall.DMTMP | ModePerm);



        private static error chmod(this ref File f, FileMode mode)
        {
            if (f == null)
            {
                return error.As(ErrInvalid);
            }
            syscall.Dir d = default;

            var (odir, e) = dirstat(f);
            if (e != null)
            {
                return error.As(ref new PathError("chmod",f.name,e));
            }
            d.Null();
            d.Mode = odir.Mode & ~chmodMask | syscallMode(mode) & chmodMask;

            array<byte> buf = new array<byte>(syscall.STATFIXLEN);
            var (n, err) = d.Marshal(buf[..]);
            if (err != null)
            {
                return error.As(ref new PathError("chmod",f.name,err));
            }
            err = syscall.Fwstat(f.fd, buf[..n]);

            if (err != null)
            {
                return error.As(ref new PathError("chmod",f.name,err));
            }
            return error.As(null);
        }

        // Sync commits the current contents of the file to stable storage.
        // Typically, this means flushing the file system's in-memory copy
        // of recently written data to disk.
        private static error Sync(this ref File f)
        {
            if (f == null)
            {
                return error.As(ErrInvalid);
            }
            syscall.Dir d = default;
            d.Null();

            array<byte> buf = new array<byte>(syscall.STATFIXLEN);
            var (n, err) = d.Marshal(buf[..]);
            if (err != null)
            {
                return error.As(NewSyscallError("fsync", err));
            }
            err = syscall.Fwstat(f.fd, buf[..n]);

            if (err != null)
            {
                return error.As(NewSyscallError("fsync", err));
            }
            return error.As(null);
        }

        // read reads up to len(b) bytes from the File.
        // It returns the number of bytes read and an error, if any.
        private static (long, error) read(this ref File f, slice<byte> b)
        {
            var (n, e) = fixCount(syscall.Read(f.fd, b));
            if (n == 0L && len(b) > 0L && e == null)
            {
                return (0L, io.EOF);
            }
            return (n, e);
        }

        // pread reads len(b) bytes from the File starting at byte offset off.
        // It returns the number of bytes read and the error, if any.
        // EOF is signaled by a zero count with err set to nil.
        private static (long, error) pread(this ref File f, slice<byte> b, long off)
        {
            var (n, e) = fixCount(syscall.Pread(f.fd, b, off));
            if (n == 0L && len(b) > 0L && e == null)
            {
                return (0L, io.EOF);
            }
            return (n, e);
        }

        // write writes len(b) bytes to the File.
        // It returns the number of bytes written and an error, if any.
        // Since Plan 9 preserves message boundaries, never allow
        // a zero-byte write.
        private static (long, error) write(this ref File f, slice<byte> b)
        {
            if (len(b) == 0L)
            {
                return (0L, null);
            }
            return fixCount(syscall.Write(f.fd, b));
        }

        // pwrite writes len(b) bytes to the File starting at byte offset off.
        // It returns the number of bytes written and an error, if any.
        // Since Plan 9 preserves message boundaries, never allow
        // a zero-byte write.
        private static (long, error) pwrite(this ref File f, slice<byte> b, long off)
        {
            if (len(b) == 0L)
            {
                return (0L, null);
            }
            return fixCount(syscall.Pwrite(f.fd, b, off));
        }

        // seek sets the offset for the next Read or Write on file to offset, interpreted
        // according to whence: 0 means relative to the origin of the file, 1 means
        // relative to the current offset, and 2 means relative to the end.
        // It returns the new offset and an error, if any.
        private static (long, error) seek(this ref File f, long offset, long whence)
        {
            return syscall.Seek(f.fd, offset, whence);
        }

        // Truncate changes the size of the named file.
        // If the file is a symbolic link, it changes the size of the link's target.
        // If there is an error, it will be of type *PathError.
        public static error Truncate(@string name, long size)
        {
            syscall.Dir d = default;

            d.Null();
            d.Length = size;

            array<byte> buf = new array<byte>(syscall.STATFIXLEN);
            var (n, err) = d.Marshal(buf[..]);
            if (err != null)
            {
                return error.As(ref new PathError("truncate",name,err));
            }
            err = syscall.Wstat(name, buf[..n]);

            if (err != null)
            {
                return error.As(ref new PathError("truncate",name,err));
            }
            return error.As(null);
        }

        // Remove removes the named file or directory.
        // If there is an error, it will be of type *PathError.
        public static error Remove(@string name)
        {
            {
                var e = syscall.Remove(name);

                if (e != null)
                {
                    return error.As(ref new PathError("remove",name,e));
                }

            }
            return error.As(null);
        }

        // HasPrefix from the strings package.
        private static bool hasPrefix(@string s, @string prefix)
        {
            return len(s) >= len(prefix) && s[0L..len(prefix)] == prefix;
        }

        // LastIndexByte from the strings package.
        private static long lastIndex(@string s, byte sep)
        {
            for (var i = len(s) - 1L; i >= 0L; i--)
            {
                if (s[i] == sep)
                {
                    return i;
                }
            }

            return -1L;
        }

        private static error rename(@string oldname, @string newname)
        {
            var dirname = oldname[..lastIndex(oldname, '/') + 1L];
            if (hasPrefix(newname, dirname))
            {
                newname = newname[len(dirname)..];
            }
            else
            {
                return error.As(ref new LinkError("rename",oldname,newname,ErrInvalid));
            } 

            // If newname still contains slashes after removing the oldname
            // prefix, the rename is cross-directory and must be rejected.
            if (lastIndex(newname, '/') >= 0L)
            {
                return error.As(ref new LinkError("rename",oldname,newname,ErrInvalid));
            }
            syscall.Dir d = default;

            d.Null();
            d.Name = newname;

            var buf = make_slice<byte>(syscall.STATFIXLEN + len(d.Name));
            var (n, err) = d.Marshal(buf[..]);
            if (err != null)
            {
                return error.As(ref new LinkError("rename",oldname,newname,err));
            } 

            // If newname already exists and is not a directory, rename replaces it.
            var (f, err) = Stat(dirname + newname);
            if (err == null && !f.IsDir())
            {
                Remove(dirname + newname);
            }
            err = syscall.Wstat(oldname, buf[..n]);

            if (err != null)
            {
                return error.As(ref new LinkError("rename",oldname,newname,err));
            }
            return error.As(null);
        }

        // See docs in file.go:Chmod.
        private static error chmod(@string name, FileMode mode)
        {
            syscall.Dir d = default;

            var (odir, e) = dirstat(name);
            if (e != null)
            {
                return error.As(ref new PathError("chmod",name,e));
            }
            d.Null();
            d.Mode = odir.Mode & ~chmodMask | syscallMode(mode) & chmodMask;

            array<byte> buf = new array<byte>(syscall.STATFIXLEN);
            var (n, err) = d.Marshal(buf[..]);
            if (err != null)
            {
                return error.As(ref new PathError("chmod",name,err));
            }
            err = syscall.Wstat(name, buf[..n]);

            if (err != null)
            {
                return error.As(ref new PathError("chmod",name,err));
            }
            return error.As(null);
        }

        // Chtimes changes the access and modification times of the named
        // file, similar to the Unix utime() or utimes() functions.
        //
        // The underlying filesystem may truncate or round the values to a
        // less precise time unit.
        // If there is an error, it will be of type *PathError.
        public static error Chtimes(@string name, time.Time atime, time.Time mtime)
        {
            syscall.Dir d = default;

            d.Null();
            d.Atime = uint32(atime.Unix());
            d.Mtime = uint32(mtime.Unix());

            array<byte> buf = new array<byte>(syscall.STATFIXLEN);
            var (n, err) = d.Marshal(buf[..]);
            if (err != null)
            {
                return error.As(ref new PathError("chtimes",name,err));
            }
            err = syscall.Wstat(name, buf[..n]);

            if (err != null)
            {
                return error.As(ref new PathError("chtimes",name,err));
            }
            return error.As(null);
        }

        // Pipe returns a connected pair of Files; reads from r return bytes
        // written to w. It returns the files and an error, if any.
        public static (ref File, ref File, error) Pipe()
        {
            array<long> p = new array<long>(2L);

            {
                var e = syscall.Pipe(p[0L..]);

                if (e != null)
                {
                    return (null, null, NewSyscallError("pipe", e));
                }

            }

            return (NewFile(uintptr(p[0L]), "|0"), NewFile(uintptr(p[1L]), "|1"), null);
        }

        // not supported on Plan 9

        // Link creates newname as a hard link to the oldname file.
        // If there is an error, it will be of type *LinkError.
        public static error Link(@string oldname, @string newname)
        {
            return error.As(ref new LinkError("link",oldname,newname,syscall.EPLAN9));
        }

        // Symlink creates newname as a symbolic link to oldname.
        // If there is an error, it will be of type *LinkError.
        public static error Symlink(@string oldname, @string newname)
        {
            return error.As(ref new LinkError("symlink",oldname,newname,syscall.EPLAN9));
        }

        // Readlink returns the destination of the named symbolic link.
        // If there is an error, it will be of type *PathError.
        public static (@string, error) Readlink(@string name)
        {
            return ("", ref new PathError("readlink",name,syscall.EPLAN9));
        }

        // Chown changes the numeric uid and gid of the named file.
        // If the file is a symbolic link, it changes the uid and gid of the link's target.
        // If there is an error, it will be of type *PathError.
        public static error Chown(@string name, long uid, long gid)
        {
            return error.As(ref new PathError("chown",name,syscall.EPLAN9));
        }

        // Lchown changes the numeric uid and gid of the named file.
        // If the file is a symbolic link, it changes the uid and gid of the link itself.
        // If there is an error, it will be of type *PathError.
        public static error Lchown(@string name, long uid, long gid)
        {
            return error.As(ref new PathError("lchown",name,syscall.EPLAN9));
        }

        // Chown changes the numeric uid and gid of the named file.
        // If there is an error, it will be of type *PathError.
        private static error Chown(this ref File f, long uid, long gid)
        {
            if (f == null)
            {
                return error.As(ErrInvalid);
            }
            return error.As(ref new PathError("chown",f.name,syscall.EPLAN9));
        }

        private static @string tempDir()
        {
            return "/tmp";
        }

        // Chdir changes the current working directory to the file,
        // which must be a directory.
        // If there is an error, it will be of type *PathError.
        private static error Chdir(this ref File f)
        {
            {
                var err = f.checkValid("chdir");

                if (err != null)
                {
                    return error.As(err);
                }

            }
            {
                var e = syscall.Fchdir(f.fd);

                if (e != null)
                {
                    return error.As(ref new PathError("chdir",f.name,e));
                }

            }
            return error.As(null);
        }

        // setDeadline sets the read and write deadline.
        private static error setDeadline(this ref File f, time.Time _p0)
        {
            {
                var err = f.checkValid("SetDeadline");

                if (err != null)
                {
                    return error.As(err);
                }

            }
            return error.As(poll.ErrNoDeadline);
        }

        // setReadDeadline sets the read deadline.
        private static error setReadDeadline(this ref File f, time.Time _p0)
        {
            {
                var err = f.checkValid("SetReadDeadline");

                if (err != null)
                {
                    return error.As(err);
                }

            }
            return error.As(poll.ErrNoDeadline);
        }

        // setWriteDeadline sets the write deadline.
        private static error setWriteDeadline(this ref File f, time.Time _p0)
        {
            {
                var err = f.checkValid("SetWriteDeadline");

                if (err != null)
                {
                    return error.As(err);
                }

            }
            return error.As(poll.ErrNoDeadline);
        }

        // checkValid checks whether f is valid for use.
        // If not, it returns an appropriate error, perhaps incorporating the operation name op.
        private static error checkValid(this ref File f, @string op)
        {
            if (f == null)
            {
                return error.As(ErrInvalid);
            }
            if (f.fd == badFd)
            {
                return error.As(ref new PathError(op,f.name,ErrClosed));
            }
            return error.As(null);
        }
    }
}
