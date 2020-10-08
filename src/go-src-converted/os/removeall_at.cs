// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris

// package os -- go2cs converted at 2020 October 08 03:44:59 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\removeall_at.go
using unix = go.@internal.syscall.unix_package;
using io = go.io_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        private static error removeAll(@string path) => func((defer, _, __) =>
        {
            if (path == "")
            { 
                // fail silently to retain compatibility with previous behavior
                // of RemoveAll. See issue 28830.
                return error.As(null!)!;

            }
            if (endsWithDot(path))
            {
                return error.As(addr(new PathError("RemoveAll",path,syscall.EINVAL))!)!;
            }
            var err = Remove(path);
            if (err == null || IsNotExist(err))
            {
                return error.As(null!)!;
            }
            var (parentDir, base) = splitPath(path);

            var (parent, err) = Open(parentDir);
            if (IsNotExist(err))
            { 
                // If parent does not exist, base cannot exist. Fail silently
                return error.As(null!)!;

            }
            if (err != null)
            {
                return error.As(err)!;
            }
            defer(parent.Close());

            {
                var err__prev1 = err;

                err = removeAllFrom(_addr_parent, base);

                if (err != null)
                {
                    {
                        ptr<PathError> (pathErr, ok) = err._<ptr<PathError>>();

                        if (ok)
                        {
                            pathErr.Path = parentDir + string(PathSeparator) + pathErr.Path;
                            err = pathErr;
                        }
                    }

                    return error.As(err)!;

                }
                err = err__prev1;

            }

            return error.As(null!)!;

        });

        private static error removeAllFrom(ptr<File> _addr_parent, @string @base)
        {
            ref File parent = ref _addr_parent.val;

            var parentFd = int(parent.Fd()); 
            // Simple case: if Unlink (aka remove) works, we're done.
            var err = unix.Unlinkat(parentFd, base, 0L);
            if (err == null || IsNotExist(err))
            {
                return error.As(null!)!;
            } 

            // EISDIR means that we have a directory, and we need to
            // remove its contents.
            // EPERM or EACCES means that we don't have write permission on
            // the parent directory, but this entry might still be a directory
            // whose contents need to be removed.
            // Otherwise just return the error.
            if (err != syscall.EISDIR && err != syscall.EPERM && err != syscall.EACCES)
            {
                return error.As(addr(new PathError("unlinkat",base,err))!)!;
            } 

            // Is this a directory we need to recurse into?
            ref syscall.Stat_t statInfo = ref heap(out ptr<syscall.Stat_t> _addr_statInfo);
            var statErr = unix.Fstatat(parentFd, base, _addr_statInfo, unix.AT_SYMLINK_NOFOLLOW);
            if (statErr != null)
            {
                if (IsNotExist(statErr))
                {
                    return error.As(null!)!;
                }

                return error.As(addr(new PathError("fstatat",base,statErr))!)!;

            }

            if (statInfo.Mode & syscall.S_IFMT != syscall.S_IFDIR)
            { 
                // Not a directory; return the error from the unix.Unlinkat.
                return error.As(addr(new PathError("unlinkat",base,err))!)!;

            } 

            // Remove the directory's entries.
            error recurseErr = default!;
            while (true)
            {
                const long reqSize = (long)1024L;

                long respSize = default; 

                // Open the directory to recurse into
                var (file, err) = openFdAt(parentFd, base);
                if (err != null)
                {
                    if (IsNotExist(err))
                    {
                        return error.As(null!)!;
                    }

                    recurseErr = error.As(addr(new PathError("openfdat",base,err)))!;
                    break;

                }

                while (true)
                {
                    long numErr = 0L;

                    var (names, readErr) = file.Readdirnames(reqSize); 
                    // Errors other than EOF should stop us from continuing.
                    if (readErr != null && readErr != io.EOF)
                    {
                        file.Close();
                        if (IsNotExist(readErr))
                        {
                            return error.As(null!)!;
                        }

                        return error.As(addr(new PathError("readdirnames",base,readErr))!)!;

                    }

                    respSize = len(names);
                    foreach (var (_, name) in names)
                    {
                        err = removeAllFrom(_addr_file, name);
                        if (err != null)
                        {
                            {
                                ptr<PathError> (pathErr, ok) = err._<ptr<PathError>>();

                                if (ok)
                                {
                                    pathErr.Path = base + string(PathSeparator) + pathErr.Path;
                                }

                            }

                            numErr++;
                            if (recurseErr == null)
                            {
                                recurseErr = error.As(err)!;
                            }

                        }

                    } 

                    // If we can delete any entry, break to start new iteration.
                    // Otherwise, we discard current names, get next entries and try deleting them.
                    if (numErr != reqSize)
                    {
                        break;
                    }

                } 

                // Removing files from the directory may have caused
                // the OS to reshuffle it. Simply calling Readdirnames
                // again may skip some entries. The only reliable way
                // to avoid this is to close and re-open the
                // directory. See issue 20841.
 

                // Removing files from the directory may have caused
                // the OS to reshuffle it. Simply calling Readdirnames
                // again may skip some entries. The only reliable way
                // to avoid this is to close and re-open the
                // directory. See issue 20841.
                file.Close(); 

                // Finish when the end of the directory is reached
                if (respSize < reqSize)
                {
                    break;
                }

            } 

            // Remove the directory itself.
 

            // Remove the directory itself.
            var unlinkError = unix.Unlinkat(parentFd, base, unix.AT_REMOVEDIR);
            if (unlinkError == null || IsNotExist(unlinkError))
            {
                return error.As(null!)!;
            }

            if (recurseErr != null)
            {
                return error.As(recurseErr)!;
            }

            return error.As(addr(new PathError("unlinkat",base,unlinkError))!)!;

        }

        // openFdAt opens path relative to the directory in fd.
        // Other than that this should act like openFileNolog.
        // This acts like openFileNolog rather than OpenFile because
        // we are going to (try to) remove the file.
        // The contents of this file are not relevant for test caching.
        private static (ptr<File>, error) openFdAt(long dirfd, @string name)
        {
            ptr<File> _p0 = default!;
            error _p0 = default!;

            long r = default;
            while (true)
            {
                error e = default!;
                r, e = unix.Openat(dirfd, name, O_RDONLY | syscall.O_CLOEXEC, 0L);
                if (e == null)
                {
                    break;
                } 

                // See comment in openFileNolog.
                if (e == syscall.EINTR)
                {
                    continue;
                }

                return (_addr_null!, error.As(e)!);

            }


            if (!supportsCloseOnExec)
            {
                syscall.CloseOnExec(r);
            }

            return (_addr_newFile(uintptr(r), name, kindOpenFile)!, error.As(null!)!);

        }
    }
}
