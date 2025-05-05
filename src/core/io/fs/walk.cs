// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.io;

using errors = errors_package;
using path = path_package;

partial class fs_package {

// SkipDir is used as a return value from [WalkDirFunc] to indicate that
// the directory named in the call is to be skipped. It is not returned
// as an error by any function.
public static error SkipDir = errors.New("skip this directory"u8);

// SkipAll is used as a return value from [WalkDirFunc] to indicate that
// all remaining files and directories are to be skipped. It is not returned
// as an error by any function.
public static error SkipAll = errors.New("skip everything and stop the walk"u8);

public delegate error WalkDirFunc(@string path, DirEntry d, error err);

// walkDir recursively descends path, calling walkDirFn.
internal static error walkDir(FS fsys, @string name, DirEntry d, WalkDirFunc walkDirFn) {
    {
        var errΔ1 = walkDirFn(name, d, default!); if (errΔ1 != default! || !d.IsDir()) {
            if (AreEqual(errΔ1, SkipDir) && d.IsDir()) {
                // Successfully skipped directory.
                 = default!;
            }
            return errΔ1;
        }
    }
    (dirs, err) = ReadDir(fsys, name);
    if (err != default!) {
        // Second call, to report ReadDir error.
        err = walkDirFn(name, d, err);
        if (err != default!) {
            if (AreEqual(err, SkipDir) && d.IsDir()) {
                err = default!;
            }
            return err;
        }
    }
    foreach (var (_, d1) in dirs) {
        @string name1 = path.Join(name, d1.Name());
        {
            var errΔ2 = walkDir(fsys, name1, d1, walkDirFn); if (errΔ2 != default!) {
                if (AreEqual(errΔ2, SkipDir)) {
                    break;
                }
                return errΔ2;
            }
        }
    }
    return default!;
}

// WalkDir walks the file tree rooted at root, calling fn for each file or
// directory in the tree, including root.
//
// All errors that arise visiting files and directories are filtered by fn:
// see the [fs.WalkDirFunc] documentation for details.
//
// The files are walked in lexical order, which makes the output deterministic
// but requires WalkDir to read an entire directory into memory before proceeding
// to walk that directory.
//
// WalkDir does not follow symbolic links found in directories,
// but if root itself is a symbolic link, its target will be walked.
public static error WalkDir(FS fsys, @string root, WalkDirFunc fn) {
    (info, err) = Stat(fsys, root);
    if (err != default!){
        err = fn(root, default!, err);
    } else {
        err = walkDir(fsys, root, FileInfoToDirEntry(info), fn);
    }
    if (AreEqual(err, SkipDir) || AreEqual(err, SkipAll)) {
        return default!;
    }
    return err;
}

} // end fs_package
