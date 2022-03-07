// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package fs -- go2cs converted at 2022 March 06 22:12:45 UTC
// import "io/fs" ==> using fs = go.io.fs_package
// Original source: C:\Program Files\Go\src\io\fs\walk.go
using errors = go.errors_package;
using path = go.path_package;

namespace go.io;

public static partial class fs_package {

    // SkipDir is used as a return value from WalkDirFuncs to indicate that
    // the directory named in the call is to be skipped. It is not returned
    // as an error by any function.
public static var SkipDir = errors.New("skip this directory");

// WalkDirFunc is the type of the function called by WalkDir to visit
// each file or directory.
//
// The path argument contains the argument to WalkDir as a prefix.
// That is, if WalkDir is called with root argument "dir" and finds a file
// named "a" in that directory, the walk function will be called with
// argument "dir/a".
//
// The d argument is the fs.DirEntry for the named path.
//
// The error result returned by the function controls how WalkDir
// continues. If the function returns the special value SkipDir, WalkDir
// skips the current directory (path if d.IsDir() is true, otherwise
// path's parent directory). Otherwise, if the function returns a non-nil
// error, WalkDir stops entirely and returns that error.
//
// The err argument reports an error related to path, signaling that
// WalkDir will not walk into that directory. The function can decide how
// to handle that error; as described earlier, returning the error will
// cause WalkDir to stop walking the entire tree.
//
// WalkDir calls the function with a non-nil err argument in two cases.
//
// First, if the initial fs.Stat on the root directory fails, WalkDir
// calls the function with path set to root, d set to nil, and err set to
// the error from fs.Stat.
//
// Second, if a directory's ReadDir method fails, WalkDir calls the
// function with path set to the directory's path, d set to an
// fs.DirEntry describing the directory, and err set to the error from
// ReadDir. In this second case, the function is called twice with the
// path of the directory: the first call is before the directory read is
// attempted and has err set to nil, giving the function a chance to
// return SkipDir and avoid the ReadDir entirely. The second call is
// after a failed ReadDir and reports the error from ReadDir.
// (If ReadDir succeeds, there is no second call.)
//
// The differences between WalkDirFunc compared to filepath.WalkFunc are:
//
//   - The second argument has type fs.DirEntry instead of fs.FileInfo.
//   - The function is called before reading a directory, to allow SkipDir
//     to bypass the directory read entirely.
//   - If a directory read fails, the function is called a second time
//     for that directory to report the error.
//
public delegate  error WalkDirFunc(@string,  DirEntry,  error);

// walkDir recursively descends path, calling walkDirFn.
private static error walkDir(FS fsys, @string name, DirEntry d, WalkDirFunc walkDirFn) {
    {
        var err__prev1 = err;

        var err = walkDirFn(name, d, null);

        if (err != null || !d.IsDir()) {
            if (err == SkipDir && d.IsDir()) { 
                // Successfully skipped directory.
                err = null;

            }

            return error.As(err)!;

        }
        err = err__prev1;

    }


    var (dirs, err) = ReadDir(fsys, name);
    if (err != null) { 
        // Second call, to report ReadDir error.
        err = walkDirFn(name, d, err);
        if (err != null) {
            return error.As(err)!;
        }
    }
    foreach (var (_, d1) in dirs) {
        var name1 = path.Join(name, d1.Name());
        {
            var err__prev1 = err;

            err = walkDir(fsys, name1, d1, walkDirFn);

            if (err != null) {
                if (err == SkipDir) {
                    break;
                }
                return error.As(err)!;
            }

            err = err__prev1;

        }

    }    return error.As(null!)!;

}

// WalkDir walks the file tree rooted at root, calling fn for each file or
// directory in the tree, including root.
//
// All errors that arise visiting files and directories are filtered by fn:
// see the fs.WalkDirFunc documentation for details.
//
// The files are walked in lexical order, which makes the output deterministic
// but requires WalkDir to read an entire directory into memory before proceeding
// to walk that directory.
//
// WalkDir does not follow symbolic links found in directories,
// but if root itself is a symbolic link, its target will be walked.
public static error WalkDir(FS fsys, @string root, WalkDirFunc fn) {
    var (info, err) = Stat(fsys, root);
    if (err != null) {
        err = fn(root, null, err);
    }
    else
 {
        err = walkDir(fsys, root, addr(new statDirEntry(info)), fn);
    }
    if (err == SkipDir) {
        return error.As(null!)!;
    }
    return error.As(err)!;

}

private partial struct statDirEntry {
    public FileInfo info;
}

private static @string Name(this ptr<statDirEntry> _addr_d) {
    ref statDirEntry d = ref _addr_d.val;

    return d.info.Name();
}
private static bool IsDir(this ptr<statDirEntry> _addr_d) {
    ref statDirEntry d = ref _addr_d.val;

    return d.info.IsDir();
}
private static FileMode Type(this ptr<statDirEntry> _addr_d) {
    ref statDirEntry d = ref _addr_d.val;

    return d.info.Mode().Type();
}
private static (FileInfo, error) Info(this ptr<statDirEntry> _addr_d) {
    FileInfo _p0 = default;
    error _p0 = default!;
    ref statDirEntry d = ref _addr_d.val;

    return (d.info, error.As(null!)!);
}

} // end fs_package
