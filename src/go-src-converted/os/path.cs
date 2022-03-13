// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2022 March 13 05:28:03 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\path.go
namespace go;

using syscall = syscall_package;


// MkdirAll creates a directory named path,
// along with any necessary parents, and returns nil,
// or else returns an error.
// The permission bits perm (before umask) are used for all
// directories that MkdirAll creates.
// If path is already a directory, MkdirAll does nothing
// and returns nil.

public static partial class os_package {

public static error MkdirAll(@string path, FileMode perm) { 
    // Fast path: if we can tell whether path is a directory or file, stop with success or error.
    var (dir, err) = Stat(path);
    if (err == null) {
        if (dir.IsDir()) {
            return error.As(null!)!;
        }
        return error.As(addr(new PathError(Op:"mkdir",Path:path,Err:syscall.ENOTDIR))!)!;
    }
    var i = len(path);
    while (i > 0 && IsPathSeparator(path[i - 1])) { // Skip trailing path separator.
        i--;
    }

    var j = i;
    while (j > 0 && !IsPathSeparator(path[j - 1])) { // Scan backward over element.
        j--;
    }

    if (j > 1) { 
        // Create parent.
        err = MkdirAll(fixRootDirectory(path[..(int)j - 1]), perm);
        if (err != null) {
            return error.As(err)!;
        }
    }
    err = Mkdir(path, perm);
    if (err != null) { 
        // Handle arguments like "foo/." by
        // double-checking that directory doesn't exist.
        var (dir, err1) = Lstat(path);
        if (err1 == null && dir.IsDir()) {
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
public static error RemoveAll(@string path) {
    return error.As(removeAll(path))!;
}

// endsWithDot reports whether the final component of path is ".".
private static bool endsWithDot(@string path) {
    if (path == ".") {
        return true;
    }
    if (len(path) >= 2 && path[len(path) - 1] == '.' && IsPathSeparator(path[len(path) - 2])) {
        return true;
    }
    return false;
}

} // end os_package
