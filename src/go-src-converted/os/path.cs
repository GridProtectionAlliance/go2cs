// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 October 08 03:44:51 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\path.go
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
                    return error.As(null!)!;
                }
                return error.As(addr(new PathError("mkdir",path,syscall.ENOTDIR))!)!;

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
                // Create parent.
                err = MkdirAll(fixRootDirectory(path[..j - 1L]), perm);
                if (err != null)
                {
                    return error.As(err)!;
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
                    return error.As(null!)!;
                }
                return error.As(err)!;

            }
            return error.As(null!)!;

        }

        // RemoveAll removes path and any children it contains.
        // It removes everything it can but returns the first error
        // it encounters. If the path does not exist, RemoveAll
        // returns nil (no error).
        // If there is an error, it will be of type *PathError.
        public static error RemoveAll(@string path)
        {
            return error.As(removeAll(path))!;
        }

        // endsWithDot reports whether the final component of path is ".".
        private static bool endsWithDot(@string path)
        {
            if (path == ".")
            {
                return true;
            }

            if (len(path) >= 2L && path[len(path) - 1L] == '.' && IsPathSeparator(path[len(path) - 2L]))
            {
                return true;
            }

            return false;

        }
    }
}
