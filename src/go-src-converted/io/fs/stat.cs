// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package fs -- go2cs converted at 2022 March 06 22:12:45 UTC
// import "io/fs" ==> using fs = go.io.fs_package
// Original source: C:\Program Files\Go\src\io\fs\stat.go


namespace go.io;

public static partial class fs_package {

    // A StatFS is a file system with a Stat method.
public partial interface StatFS {
    (FileInfo, error) Stat(@string name);
}

// Stat returns a FileInfo describing the named file from the file system.
//
// If fs implements StatFS, Stat calls fs.Stat.
// Otherwise, Stat opens the file to stat it.
public static (FileInfo, error) Stat(FS fsys, @string name) => func((defer, _, _) => {
    FileInfo _p0 = default;
    error _p0 = default!;

    {
        StatFS (fsys, ok) = StatFS.As(fsys._<StatFS>())!;

        if (ok) {
            return fsys.Stat(name);
        }
    }


    var (file, err) = fsys.Open(name);
    if (err != null) {
        return (null, error.As(err)!);
    }
    defer(file.Close());
    return file.Stat();

});

} // end fs_package
