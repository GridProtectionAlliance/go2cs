// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 August 29 08:44:09 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\path.go
using io = go.io_package;
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        // MkdirAll creates a directory named path,
        // along with any necessary parents, and returns nil,
        // or else returns an error.
        // The permission bits perm (before umask) are used for all
        // directories that MkdirAll creates.
        // If path is already a directory, MkdirAll does nothing
        // and returns nil.
        public static error MkdirAll(@string path, FileMode perm)
        { 
            // Fast path: if we can tell whether path is a directory or file, stop with success or error.
            var (dir, err) = Stat(path);
            if (err == null)
            {
                if (dir.IsDir())
                {
                    return error.As(null);
                }
                return error.As(ref new PathError("mkdir",path,syscall.ENOTDIR));
            }
            var i = len(path);
            while (i > 0L && IsPathSeparator(path[i - 1L]))
            { // Skip trailing path separator.
                i--;
            }

            var j = i;
            while (j > 0L && !IsPathSeparator(path[j - 1L]))
            { // Scan backward over element.
                j--;
            }

            if (j > 1L)
            { 
                // Create parent
                err = MkdirAll(path[0L..j - 1L], perm);
                if (err != null)
                {
                    return error.As(err);
                }
            }
            err = Mkdir(path, perm);
            if (err != null)
            { 
                // Handle arguments like "foo/." by
                // double-checking that directory doesn't exist.
                var (dir, err1) = Lstat(path);
                if (err1 == null && dir.IsDir())
                {
                    return error.As(null);
                }
                return error.As(err);
            }
            return error.As(null);
        }

        // RemoveAll removes path and any children it contains.
        // It removes everything it can but returns the first error
        // it encounters. If the path does not exist, RemoveAll
        // returns nil (no error).
        public static error RemoveAll(@string path)
        { 
            // Simple case: if Remove works, we're done.
            var err = Remove(path);
            if (err == null || IsNotExist(err))
            {
                return error.As(null);
            } 

            // Otherwise, is this a directory we need to recurse into?
            var (dir, serr) = Lstat(path);
            if (serr != null)
            {
                {
                    ref PathError (serr, ok) = serr._<ref PathError>();

                    if (ok && (IsNotExist(serr.Err) || serr.Err == syscall.ENOTDIR))
                    {
                        return error.As(null);
                    }

                }
                return error.As(serr);
            }
            if (!dir.IsDir())
            { 
                // Not a directory; return the error from Remove.
                return error.As(err);
            } 

            // Directory.
            var (fd, err) = Open(path);
            if (err != null)
            {
                if (IsNotExist(err))
                { 
                    // Race. It was deleted between the Lstat and Open.
                    // Return nil per RemoveAll's docs.
                    return error.As(null);
                }
                return error.As(err);
            } 

            // Remove contents & return first error.
            err = null;
            while (true)
            {
                if (err == null && (runtime.GOOS == "plan9" || runtime.GOOS == "nacl"))
                { 
                    // Reset read offset after removing directory entries.
                    // See golang.org/issue/22572.
                    fd.Seek(0L, 0L);
                }
                var (names, err1) = fd.Readdirnames(100L);
                foreach (var (_, name) in names)
                {
                    var err1 = RemoveAll(path + string(PathSeparator) + name);
                    if (err == null)
                    {
                        err = err1;
                    }
                }
                if (err1 == io.EOF)
                {
                    break;
                } 
                // If Readdirnames returned an error, use it.
                if (err == null)
                {
                    err = err1;
                }
                if (len(names) == 0L)
                {
                    break;
                }
            } 

            // Close directory, because windows won't remove opened directory.
 

            // Close directory, because windows won't remove opened directory.
            fd.Close(); 

            // Remove directory.
            err1 = Remove(path);
            if (err1 == null || IsNotExist(err1))
            {
                return error.As(null);
            }
            if (err == null)
            {
                err = err1;
            }
            return error.As(err);
        }
    }
}
