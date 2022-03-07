// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2022 March 06 22:13:26 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\executable_solaris.go
using syscall = go.syscall_package;

namespace go;

public static partial class os_package {

private static @string executablePath = default; // set by sysauxv in ../runtime/os3_solaris.go



private static (@string, error) executable() {
    @string _p0 = default;
    error _p0 = default!;

    var path = executablePath;
    if (len(path) == 0) {
        var (path, err) = syscall.Getexecname();
        if (err != null) {
            return (path, error.As(err)!);
        }
    }
    if (len(path) > 0 && path[0] != '/') {
        if (initCwdErr != null) {
            return (path, error.As(initCwdErr)!);
        }
        if (len(path) > 2 && path[(int)0..(int)2] == "./") { 
            // skip "./"
            path = path[(int)2..];

        }
        return (initCwd + "/" + path, error.As(null!)!);

    }
    return (path, error.As(null!)!);

}

} // end os_package
