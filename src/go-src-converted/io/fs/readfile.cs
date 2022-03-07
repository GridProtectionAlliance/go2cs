// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package fs -- go2cs converted at 2022 March 06 22:12:45 UTC
// import "io/fs" ==> using fs = go.io.fs_package
// Original source: C:\Program Files\Go\src\io\fs\readfile.go
using io = go.io_package;

namespace go.io;

public static partial class fs_package {

    // ReadFileFS is the interface implemented by a file system
    // that provides an optimized implementation of ReadFile.
public partial interface ReadFileFS {
    (slice<byte>, error) ReadFile(@string name);
}

// ReadFile reads the named file from the file system fs and returns its contents.
// A successful call returns a nil error, not io.EOF.
// (Because ReadFile reads the whole file, the expected EOF
// from the final Read is not treated as an error to be reported.)
//
// If fs implements ReadFileFS, ReadFile calls fs.ReadFile.
// Otherwise ReadFile calls fs.Open and uses Read and Close
// on the returned file.
public static (slice<byte>, error) ReadFile(FS fsys, @string name) => func((defer, _, _) => {
    slice<byte> _p0 = default;
    error _p0 = default!;

    {
        ReadFileFS (fsys, ok) = ReadFileFS.As(fsys._<ReadFileFS>())!;

        if (ok) {
            return fsys.ReadFile(name);
        }
    }


    var (file, err) = fsys.Open(name);
    if (err != null) {
        return (null, error.As(err)!);
    }
    defer(file.Close());

    nint size = default;
    {
        var (info, err) = file.Stat();

        if (err == null) {
            var size64 = info.Size();
            if (int64(int(size64)) == size64) {
                size = int(size64);
            }
        }
    }


    var data = make_slice<byte>(0, size + 1);
    while (true) {
        if (len(data) >= cap(data)) {
            var d = append(data[..(int)cap(data)], 0);
            data = d[..(int)len(data)];
        }
        var (n, err) = file.Read(data[(int)len(data)..(int)cap(data)]);
        data = data[..(int)len(data) + n];
        if (err != null) {
            if (err == io.EOF) {
                err = null;
            }
            return (data, error.As(err)!);
        }
    }

});

} // end fs_package
