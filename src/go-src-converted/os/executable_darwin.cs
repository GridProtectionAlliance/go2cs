// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2022 March 06 22:13:25 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\executable_darwin.go
using errors = go.errors_package;

namespace go;

public static partial class os_package {

private static @string executablePath = default; // set by ../runtime/os_darwin.go



private static (@string, error) executable() {
    @string _p0 = default;
    error _p0 = default!;

    var ep = executablePath;
    if (len(ep) == 0) {
        return (ep, error.As(errors.New("cannot find executable path"))!);
    }
    if (ep[0] != '/') {
        if (initCwdErr != null) {
            return (ep, error.As(initCwdErr)!);
        }
        if (len(ep) > 2 && ep[(int)0..(int)2] == "./") { 
            // skip "./"
            ep = ep[(int)2..];

        }
        ep = initCwd + "/" + ep;

    }
    return (ep, error.As(null!)!);

}

} // end os_package
