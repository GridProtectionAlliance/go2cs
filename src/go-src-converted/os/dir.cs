// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 October 09 04:45:16 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\dir.go

using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        // Readdir reads the contents of the directory associated with file and
        // returns a slice of up to n FileInfo values, as would be returned
        // by Lstat, in directory order. Subsequent calls on the same file will yield
        // further FileInfos.
        //
        // If n > 0, Readdir returns at most n FileInfo structures. In this case, if
        // Readdir returns an empty slice, it will return a non-nil error
        // explaining why. At the end of a directory, the error is io.EOF.
        //
        // If n <= 0, Readdir returns all the FileInfo from the directory in
        // a single slice. In this case, if Readdir succeeds (reads all
        // the way to the end of the directory), it returns the slice and a
        // nil error. If it encounters an error before the end of the
        // directory, Readdir returns the FileInfo read until that point
        // and a non-nil error.
        private static (slice<FileInfo>, error) Readdir(this ptr<File> _addr_f, long n)
        {
            slice<FileInfo> _p0 = default;
            error _p0 = default!;
            ref File f = ref _addr_f.val;

            if (f == null)
            {
                return (null, error.As(ErrInvalid)!);
            }
            return f.readdir(n);

        }

        // Readdirnames reads the contents of the directory associated with file
        // and returns a slice of up to n names of files in the directory,
        // in directory order. Subsequent calls on the same file will yield
        // further names.
        //
        // If n > 0, Readdirnames returns at most n names. In this case, if
        // Readdirnames returns an empty slice, it will return a non-nil error
        // explaining why. At the end of a directory, the error is io.EOF.
        //
        // If n <= 0, Readdirnames returns all the names from the directory in
        // a single slice. In this case, if Readdirnames succeeds (reads all
        // the way to the end of the directory), it returns the slice and a
        // nil error. If it encounters an error before the end of the
        // directory, Readdirnames returns the names read until that point and
        // a non-nil error.
        private static (slice<@string>, error) Readdirnames(this ptr<File> _addr_f, long n)
        {
            slice<@string> names = default;
            error err = default!;
            ref File f = ref _addr_f.val;

            if (f == null)
            {
                return (null, error.As(ErrInvalid)!);
            }

            return f.readdirnames(n);

        }
    }
}
