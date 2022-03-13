// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2022 March 13 05:28:05 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\stat.go
namespace go;

using testlog = @internal.testlog_package;

public static partial class os_package {

// Stat returns a FileInfo describing the named file.
// If there is an error, it will be of type *PathError.
public static (FileInfo, error) Stat(@string name) {
    FileInfo _p0 = default;
    error _p0 = default!;

    testlog.Stat(name);
    return statNolog(name);
}

// Lstat returns a FileInfo describing the named file.
// If the file is a symbolic link, the returned FileInfo
// describes the symbolic link. Lstat makes no attempt to follow the link.
// If there is an error, it will be of type *PathError.
public static (FileInfo, error) Lstat(@string name) {
    FileInfo _p0 = default;
    error _p0 = default!;

    testlog.Stat(name);
    return lstatNolog(name);
}

} // end os_package
