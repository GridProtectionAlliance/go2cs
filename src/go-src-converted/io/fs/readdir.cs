// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package fs -- go2cs converted at 2022 March 13 05:27:46 UTC
// import "io/fs" ==> using fs = go.io.fs_package
// Original source: C:\Program Files\Go\src\io\fs\readdir.go
namespace go.io;

using errors = errors_package;
using sort = sort_package;


// ReadDirFS is the interface implemented by a file system
// that provides an optimized implementation of ReadDir.

using System;
public static partial class fs_package {

public partial interface ReadDirFS {
    (slice<DirEntry>, error) ReadDir(@string name);
}

// ReadDir reads the named directory
// and returns a list of directory entries sorted by filename.
//
// If fs implements ReadDirFS, ReadDir calls fs.ReadDir.
// Otherwise ReadDir calls fs.Open and uses ReadDir and Close
// on the returned file.
public static (slice<DirEntry>, error) ReadDir(FS fsys, @string name) => func((defer, _, _) => {
    slice<DirEntry> _p0 = default;
    error _p0 = default!;

    {
        ReadDirFS (fsys, ok) = ReadDirFS.As(fsys._<ReadDirFS>())!;

        if (ok) {
            return fsys.ReadDir(name);
        }
    }

    var (file, err) = fsys.Open(name);
    if (err != null) {
        return (null, error.As(err)!);
    }
    defer(file.Close());

    ReadDirFile (dir, ok) = file._<ReadDirFile>();
    if (!ok) {
        return (null, error.As(addr(new PathError(Op:"readdir",Path:name,Err:errors.New("not implemented")))!)!);
    }
    var (list, err) = dir.ReadDir(-1);
    sort.Slice(list, (i, j) => list[i].Name() < list[j].Name());
    return (list, error.As(err)!);
});

// dirInfo is a DirEntry based on a FileInfo.
private partial struct dirInfo {
    public FileInfo fileInfo;
}

private static bool IsDir(this dirInfo di) {
    return di.fileInfo.IsDir();
}

private static FileMode Type(this dirInfo di) {
    return di.fileInfo.Mode().Type();
}

private static (FileInfo, error) Info(this dirInfo di) {
    FileInfo _p0 = default;
    error _p0 = default!;

    return (di.fileInfo, error.As(null!)!);
}

private static @string Name(this dirInfo di) {
    return di.fileInfo.Name();
}

// FileInfoToDirEntry returns a DirEntry that returns information from info.
// If info is nil, FileInfoToDirEntry returns nil.
public static DirEntry FileInfoToDirEntry(FileInfo info) {
    if (info == null) {
        return null;
    }
    return new dirInfo(fileInfo:info);
}

} // end fs_package
