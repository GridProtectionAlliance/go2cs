// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux nacl netbsd openbsd solaris windows

// package os -- go2cs converted at 2020 August 29 08:44:00 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\file_posix.go
using syscall = go.syscall_package;
using time = go.time_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        private static void sigpipe()
; // implemented in package runtime

        // Readlink returns the destination of the named symbolic link.
        // If there is an error, it will be of type *PathError.
        public static (@string, error) Readlink(@string name)
        {
            {
                long len = 128L;

                while (>>MARKER:FOREXPRESSION_LEVEL_1<<)
                {>>MARKER:FUNCTION_sigpipe_BLOCK_PREFIX<<
                    var b = make_slice<byte>(len);
                    var (n, e) = fixCount(syscall.Readlink(fixLongPath(name), b));
                    if (e != null)
                    {
                        return ("", ref new PathError("readlink",name,e));
                    len *= 2L;
                    }
                    if (n < len)
                    {
                        return (string(b[0L..n]), null);
                    }
                }

            }
        }

        // syscallMode returns the syscall-specific mode bits from Go's portable mode bits.
        private static uint syscallMode(FileMode i)
        {
            o |= uint32(i.Perm());
            if (i & ModeSetuid != 0L)
            {
                o |= syscall.S_ISUID;
            }
            if (i & ModeSetgid != 0L)
            {
                o |= syscall.S_ISGID;
            }
            if (i & ModeSticky != 0L)
            {
                o |= syscall.S_ISVTX;
            } 
            // No mapping for Go's ModeTemporary (plan9 only).
            return;
        }

        // See docs in file.go:Chmod.
        private static error chmod(@string name, FileMode mode)
        {
            {
                var e = syscall.Chmod(fixLongPath(name), syscallMode(mode));

                if (e != null)
                {
                    return error.As(ref new PathError("chmod",name,e));
                }

            }
            return error.As(null);
        }

        // See docs in file.go:(*File).Chmod.
        private static error chmod(this ref File f, FileMode mode)
        {
            {
                var err = f.checkValid("chmod");

                if (err != null)
                {
                    return error.As(err);
                }

            }
            {
                var e = f.pfd.Fchmod(syscallMode(mode));

                if (e != null)
                {
                    return error.As(f.wrapErr("chmod", e));
                }

            }
            return error.As(null);
        }

        // Chown changes the numeric uid and gid of the named file.
        // If the file is a symbolic link, it changes the uid and gid of the link's target.
        // If there is an error, it will be of type *PathError.
        //
        // On Windows, it always returns the syscall.EWINDOWS error, wrapped
        // in *PathError.
        public static error Chown(@string name, long uid, long gid)
        {
            {
                var e = syscall.Chown(name, uid, gid);

                if (e != null)
                {
                    return error.As(ref new PathError("chown",name,e));
                }

            }
            return error.As(null);
        }

        // Lchown changes the numeric uid and gid of the named file.
        // If the file is a symbolic link, it changes the uid and gid of the link itself.
        // If there is an error, it will be of type *PathError.
        //
        // On Windows, it always returns the syscall.EWINDOWS error, wrapped
        // in *PathError.
        public static error Lchown(@string name, long uid, long gid)
        {
            {
                var e = syscall.Lchown(name, uid, gid);

                if (e != null)
                {
                    return error.As(ref new PathError("lchown",name,e));
                }

            }
            return error.As(null);
        }

        // Chown changes the numeric uid and gid of the named file.
        // If there is an error, it will be of type *PathError.
        //
        // On Windows, it always returns the syscall.EWINDOWS error, wrapped
        // in *PathError.
        private static error Chown(this ref File f, long uid, long gid)
        {
            {
                var err = f.checkValid("chown");

                if (err != null)
                {
                    return error.As(err);
                }

            }
            {
                var e = f.pfd.Fchown(uid, gid);

                if (e != null)
                {
                    return error.As(f.wrapErr("chown", e));
                }

            }
            return error.As(null);
        }

        // Truncate changes the size of the file.
        // It does not change the I/O offset.
        // If there is an error, it will be of type *PathError.
        private static error Truncate(this ref File f, long size)
        {
            {
                var err = f.checkValid("truncate");

                if (err != null)
                {
                    return error.As(err);
                }

            }
            {
                var e = f.pfd.Ftruncate(size);

                if (e != null)
                {
                    return error.As(f.wrapErr("truncate", e));
                }

            }
            return error.As(null);
        }

        // Sync commits the current contents of the file to stable storage.
        // Typically, this means flushing the file system's in-memory copy
        // of recently written data to disk.
        private static error Sync(this ref File f)
        {
            {
                var err = f.checkValid("sync");

                if (err != null)
                {
                    return error.As(err);
                }

            }
            {
                var e = f.pfd.Fsync();

                if (e != null)
                {
                    return error.As(f.wrapErr("sync", e));
                }

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
            array<syscall.Timespec> utimes = new array<syscall.Timespec>(2L);
            utimes[0L] = syscall.NsecToTimespec(atime.UnixNano());
            utimes[1L] = syscall.NsecToTimespec(mtime.UnixNano());
            {
                var e = syscall.UtimesNano(fixLongPath(name), utimes[0L..]);

                if (e != null)
                {
                    return error.As(ref new PathError("chtimes",name,e));
                }

            }
            return error.As(null);
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
                var e = f.pfd.Fchdir();

                if (e != null)
                {
                    return error.As(f.wrapErr("chdir", e));
                }

            }
            return error.As(null);
        }

        // setDeadline sets the read and write deadline.
        private static error setDeadline(this ref File f, time.Time t)
        {
            {
                var err = f.checkValid("SetDeadline");

                if (err != null)
                {
                    return error.As(err);
                }

            }
            return error.As(f.pfd.SetDeadline(t));
        }

        // setReadDeadline sets the read deadline.
        private static error setReadDeadline(this ref File f, time.Time t)
        {
            {
                var err = f.checkValid("SetReadDeadline");

                if (err != null)
                {
                    return error.As(err);
                }

            }
            return error.As(f.pfd.SetReadDeadline(t));
        }

        // setWriteDeadline sets the write deadline.
        private static error setWriteDeadline(this ref File f, time.Time t)
        {
            {
                var err = f.checkValid("SetWriteDeadline");

                if (err != null)
                {
                    return error.As(err);
                }

            }
            return error.As(f.pfd.SetWriteDeadline(t));
        }

        // checkValid checks whether f is valid for use.
        // If not, it returns an appropriate error, perhaps incorporating the operation name op.
        private static error checkValid(this ref File f, @string op)
        {
            if (f == null)
            {
                return error.As(ErrInvalid);
            }
            return error.As(null);
        }
    }
}
