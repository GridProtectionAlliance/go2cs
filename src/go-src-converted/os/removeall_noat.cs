// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !aix,!darwin,!dragonfly,!freebsd,!linux,!netbsd,!openbsd,!solaris

// package os -- go2cs converted at 2020 October 08 03:45:00 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\removeall_noat.go
using io = go.io_package;
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        private static error removeAll(@string path)
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
            var (dir, serr) = Lstat(path);
            if (serr != null)
            {
                {
                    ptr<PathError> (serr, ok) = serr._<ptr<PathError>>();

                    if (ok && (IsNotExist(serr.Err) || serr.Err == syscall.ENOTDIR))
                    {
                        return error.As(null!)!;
                    }
                }

                return error.As(serr)!;

            }
            if (!dir.IsDir())
            { 
                // Not a directory; return the error from Remove.
                return error.As(err)!;

            }
            err = null;
            while (true)
            {
                var (fd, err) = Open(path);
                if (err != null)
                {
                    if (IsNotExist(err))
                    { 
                        // Already deleted by someone else.
                        return error.As(null!)!;

                    }
                    return error.As(err)!;

                }
                const long reqSize = (long)1024L;

                slice<@string> names = default;
                error readErr = default!;

                while (true)
                {
                    long numErr = 0L;
                    names, readErr = fd.Readdirnames(reqSize);

                    foreach (var (_, name) in names)
                    {
                        var err1 = RemoveAll(path + string(PathSeparator) + name);
                        if (err == null)
                        {
                            err = err1;
                        }
                        if (err1 != null)
                        {
                            numErr++;
                        }
                    }                    if (numErr != reqSize)
                    {
                        break;
                    }
                } 

                // Removing files from the directory may have caused
                // the OS to reshuffle it. Simply calling Readdirnames
                // again may skip some entries. The only reliable way
                // to avoid this is to close and re-open the
                // directory. See issue 20841.
                fd.Close();

                if (readErr == io.EOF)
                {
                    break;
                }
                if (err == null)
                {
                    err = readErr;
                }
                if (len(names) == 0L)
                {
                    break;
                }
                if (len(names) < reqSize)
                {
                    err1 = Remove(path);
                    if (err1 == null || IsNotExist(err1))
                    {
                        return error.As(null!)!;
                    }
                    if (err != null)
                    { 
                        // We got some error removing the
                        // directory contents, and since we
                        // read fewer names than we requested
                        // there probably aren't more files to
                        // remove. Don't loop around to read
                        // the directory again. We'll probably
                        // just get the same error.
                        return error.As(err)!;

                    }
                }
            } 

            // Remove directory.
            err1 = Remove(path);
            if (err1 == null || IsNotExist(err1))
            {
                return error.As(null!)!;
            }
            if (runtime.GOOS == "windows" && IsPermission(err1))
            {
                {
                    var (fs, err) = Stat(path);

                    if (err == null)
                    {
                        err = Chmod(path, FileMode(0200L | int(fs.Mode())));

                        if (err == null)
                        {
                            err1 = Remove(path);
                        }
                    }
                }

            }
            if (err == null)
            {
                err = err1;
            }
            return error.As(err)!;

        }
    }
}
