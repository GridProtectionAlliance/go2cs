// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 October 09 05:07:22 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\stat.go
using testlog = go.@internal.testlog_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        // Stat returns a FileInfo describing the named file.
        // If there is an error, it will be of type *PathError.
        public static (FileInfo, error) Stat(@string name)
        {
            FileInfo _p0 = default;
            error _p0 = default!;

            testlog.Stat(name);
            return statNolog(name);
        }

        // Lstat returns a FileInfo describing the named file.
        // If the file is a symbolic link, the returned FileInfo
        // describes the symbolic link. Lstat makes no attempt to follow the link.
        // If there is an error, it will be of type *PathError.
        public static (FileInfo, error) Lstat(@string name)
        {
            FileInfo _p0 = default;
            error _p0 = default!;

            testlog.Stat(name);
            return lstatNolog(name);
        }
    }
}
