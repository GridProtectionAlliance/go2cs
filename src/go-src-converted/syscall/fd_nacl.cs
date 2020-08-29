// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// File descriptor support for Native Client.
// We want to provide access to a broader range of (simulated) files than
// Native Client allows, so we maintain our own file descriptor table exposed
// to higher-level packages.

// package syscall -- go2cs converted at 2020 August 29 08:37:07 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\fd_nacl.go
using io = go.io_package;
using sync = go.sync_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class syscall_package
    {
        // files is the table indexed by a file descriptor.
        private static var files = default;

        // A file is an open file, something with a file descriptor.
        // A particular *file may appear in files multiple times, due to use of Dup or Dup2.
        private partial struct file
        {
            public long fdref; // uses in files.tab
            public fileImpl impl; // underlying implementation
        }

        // A fileImpl is the implementation of something that can be a file.
        private partial interface fileImpl
        {
            error stat(ref Stat_t _p0);
            error read(slice<byte> _p0);
            error write(slice<byte> _p0);
            error seek(long _p0, long _p0);
            error pread(slice<byte> _p0, long _p0);
            error pwrite(slice<byte> _p0, long _p0); // Close is called when the last reference to a *file is removed
// from the file descriptor table. It may be called concurrently
// with active operations such as blocked read or write calls.
            error close();
        }

        // newFD adds impl to the file descriptor table,
        // returning the new file descriptor.
        // Like Unix, it uses the lowest available descriptor.
        private static long newFD(fileImpl impl) => func((defer, _, __) =>
        {
            files.Lock();
            defer(files.Unlock());
            file f = ref new file(impl:impl,fdref:1);
            {
                var fd__prev1 = fd;

                foreach (var (__fd, __oldf) in files.tab)
                {
                    fd = __fd;
                    oldf = __oldf;
                    if (oldf == null)
                    {
                        files.tab[fd] = f;
                        return fd;
                    }
                }

                fd = fd__prev1;
            }

            var fd = len(files.tab);
            files.tab = append(files.tab, f);
            return fd;
        });

        // Install Native Client stdin, stdout, stderr.
        private static void init()
        {
            newFD(ref new naclFile(naclFD:0));
            newFD(ref new naclFile(naclFD:1));
            newFD(ref new naclFile(naclFD:2));
        }

        // fdToFile retrieves the *file corresponding to a file descriptor.
        private static (ref file, error) fdToFile(long fd) => func((defer, _, __) =>
        {
            files.Lock();
            defer(files.Unlock());
            if (fd < 0L || fd >= len(files.tab) || files.tab[fd] == null)
            {
                return (null, EBADF);
            }
            return (files.tab[fd], null);
        });

        public static error Close(long fd)
        {
            files.Lock();
            if (fd < 0L || fd >= len(files.tab) || files.tab[fd] == null)
            {
                files.Unlock();
                return error.As(EBADF);
            }
            var f = files.tab[fd];
            files.tab[fd] = null;
            f.fdref--;
            var fdref = f.fdref;
            files.Unlock();
            if (fdref > 0L)
            {
                return error.As(null);
            }
            return error.As(f.impl.close());
        }

        public static void CloseOnExec(long fd)
        { 
            // nothing to do - no exec
        }

        public static (long, error) Dup(long fd) => func((defer, _, __) =>
        {
            files.Lock();
            defer(files.Unlock());
            if (fd < 0L || fd >= len(files.tab) || files.tab[fd] == null)
            {
                return (-1L, EBADF);
            }
            var f = files.tab[fd];
            f.fdref++;
            {
                var newfd__prev1 = newfd;

                foreach (var (__newfd, __oldf) in files.tab)
                {
                    newfd = __newfd;
                    oldf = __oldf;
                    if (oldf == null)
                    {
                        files.tab[newfd] = f;
                        return (newfd, null);
                    }
                }

                newfd = newfd__prev1;
            }

            var newfd = len(files.tab);
            files.tab = append(files.tab, f);
            return (newfd, null);
        });

        public static error Dup2(long fd, long newfd) => func((defer, _, __) =>
        {
            files.Lock();
            defer(files.Unlock());
            if (fd < 0L || fd >= len(files.tab) || files.tab[fd] == null || newfd < 0L || newfd >= len(files.tab) + 100L)
            {
                files.Unlock();
                return error.As(EBADF);
            }
            var f = files.tab[fd];
            f.fdref++;
            while (cap(files.tab) <= newfd)
            {
                files.tab = append(files.tab[..cap(files.tab)], null);
            }

            var oldf = files.tab[newfd];
            long oldfdref = default;
            if (oldf != null)
            {
                oldf.fdref--;
                oldfdref = oldf.fdref;
            }
            files.tab[newfd] = f;
            files.Unlock();
            if (oldf != null)
            {
                if (oldfdref == 0L)
                {
                    oldf.impl.close();
                }
            }
            return error.As(null);
        });

        public static error Fstat(long fd, ref Stat_t st)
        {
            var (f, err) = fdToFile(fd);
            if (err != null)
            {
                return error.As(err);
            }
            return error.As(f.impl.stat(st));
        }

        public static (long, error) Read(long fd, slice<byte> b)
        {
            var (f, err) = fdToFile(fd);
            if (err != null)
            {
                return (0L, err);
            }
            return f.impl.read(b);
        }

        private static array<byte> zerobuf = new array<byte>(0L);

        public static (long, error) Write(long fd, slice<byte> b)
        {
            if (b == null)
            { 
                // avoid nil in syscalls; nacl doesn't like that.
                b = zerobuf[..];
            }
            var (f, err) = fdToFile(fd);
            if (err != null)
            {
                return (0L, err);
            }
            return f.impl.write(b);
        }

        public static (long, error) Pread(long fd, slice<byte> b, long offset)
        {
            var (f, err) = fdToFile(fd);
            if (err != null)
            {
                return (0L, err);
            }
            return f.impl.pread(b, offset);
        }

        public static (long, error) Pwrite(long fd, slice<byte> b, long offset)
        {
            var (f, err) = fdToFile(fd);
            if (err != null)
            {
                return (0L, err);
            }
            return f.impl.pwrite(b, offset);
        }

        public static (long, error) Seek(long fd, long offset, long whence)
        {
            var (f, err) = fdToFile(fd);
            if (err != null)
            {
                return (0L, err);
            }
            return f.impl.seek(offset, whence);
        }

        // defaulFileImpl implements fileImpl.
        // It can be embedded to complete a partial fileImpl implementation.
        private partial struct defaultFileImpl
        {
        }

        private static error close(this ref defaultFileImpl _p0)
        {
            return error.As(null);
        }
        private static error stat(this ref defaultFileImpl _p0, ref Stat_t _p0)
        {
            return error.As(ENOSYS);
        }
        private static (long, error) read(this ref defaultFileImpl _p0, slice<byte> _p0)
        {
            return (0L, ENOSYS);
        }
        private static (long, error) write(this ref defaultFileImpl _p0, slice<byte> _p0)
        {
            return (0L, ENOSYS);
        }
        private static (long, error) seek(this ref defaultFileImpl _p0, long _p0, long _p0)
        {
            return (0L, ENOSYS);
        }
        private static (long, error) pread(this ref defaultFileImpl _p0, slice<byte> _p0, long _p0)
        {
            return (0L, ENOSYS);
        }
        private static (long, error) pwrite(this ref defaultFileImpl _p0, slice<byte> _p0, long _p0)
        {
            return (0L, ENOSYS);
        }

        // naclFile is the fileImpl implementation for a Native Client file descriptor.
        private partial struct naclFile
        {
            public ref defaultFileImpl defaultFileImpl => ref defaultFileImpl_val;
            public long naclFD;
        }

        private static error stat(this ref naclFile f, ref Stat_t st)
        {
            return error.As(naclFstat(f.naclFD, st));
        }

        private static (long, error) read(this ref naclFile f, slice<byte> b)
        {
            var (n, err) = naclRead(f.naclFD, b);
            if (err != null)
            {
                n = 0L;
            }
            return (n, err);
        }

        // implemented in package runtime, to add time header on playground
        private static long naclWrite(long fd, slice<byte> b)
;

        private static (long, error) write(this ref naclFile f, slice<byte> b)
        {>>MARKER:FUNCTION_naclWrite_BLOCK_PREFIX<<
            var n = naclWrite(f.naclFD, b);
            if (n < 0L)
            {
                return (0L, Errno(-n));
            }
            return (n, null);
        }

        private static (long, error) seek(this ref naclFile f, long off, long whence)
        {
            var old = off;
            var err = naclSeek(f.naclFD, ref off, whence);
            if (err != null)
            {
                return (old, err);
            }
            return (off, null);
        }

        private static (long, error) prw(this ref naclFile f, slice<byte> b, long offset, Func<slice<byte>, (long, error)> rw)
        { 
            // NaCl has no pread; simulate with seek and hope for no races.
            var (old, err) = f.seek(0L, io.SeekCurrent);
            if (err != null)
            {
                return (0L, err);
            }
            {
                var (_, err) = f.seek(offset, io.SeekStart);

                if (err != null)
                {
                    return (0L, err);
                }

            }
            var (n, err) = rw(b);
            f.seek(old, io.SeekStart);
            return (n, err);
        }

        private static (long, error) pread(this ref naclFile f, slice<byte> b, long offset)
        {
            return f.prw(b, offset, f.read);
        }

        private static (long, error) pwrite(this ref naclFile f, slice<byte> b, long offset)
        {
            return f.prw(b, offset, f.write);
        }

        private static error close(this ref naclFile f)
        {
            var err = naclClose(f.naclFD);
            f.naclFD = -1L;
            return error.As(err);
        }

        // A pipeFile is an in-memory implementation of a pipe.
        // The byteq implementation is in net_nacl.go.
        private partial struct pipeFile
        {
            public ref defaultFileImpl defaultFileImpl => ref defaultFileImpl_val;
            public ptr<byteq> rd;
            public ptr<byteq> wr;
        }

        private static error close(this ref pipeFile f)
        {
            if (f.rd != null)
            {
                f.rd.close();
            }
            if (f.wr != null)
            {
                f.wr.close();
            }
            return error.As(null);
        }

        private static (long, error) read(this ref pipeFile f, slice<byte> b)
        {
            if (f.rd == null)
            {
                return (0L, EINVAL);
            }
            var (n, err) = f.rd.read(b, 0L);
            if (err == EAGAIN)
            {
                err = null;
            }
            return (n, err);
        }

        private static (long, error) write(this ref pipeFile f, slice<byte> b)
        {
            if (f.wr == null)
            {
                return (0L, EINVAL);
            }
            var (n, err) = f.wr.write(b, 0L);
            if (err == EAGAIN)
            {
                err = EPIPE;
            }
            return (n, err);
        }

        public static error Pipe(slice<long> fd)
        {
            var q = newByteq();
            fd[0L] = newFD(ref new pipeFile(rd:q));
            fd[1L] = newFD(ref new pipeFile(wr:q));
            return error.As(null);
        }
    }
}
