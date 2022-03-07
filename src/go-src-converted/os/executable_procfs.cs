// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build linux || netbsd || (js && wasm)
// +build linux netbsd js,wasm

// package os -- go2cs converted at 2022 March 06 22:13:25 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\executable_procfs.go
using errors = go.errors_package;
using runtime = go.runtime_package;

namespace go;

public static partial class os_package {

private static (@string, error) executable() {
    @string _p0 = default;
    error _p0 = default!;

    @string procfn = default;
    switch (runtime.GOOS) {
        case "linux": 

        case "android": 
            procfn = "/proc/self/exe";
            break;
        case "netbsd": 
            procfn = "/proc/curproc/exe";
            break;
        default: 
            return ("", error.As(errors.New("Executable not implemented for " + runtime.GOOS))!);
            break;
    }
    var (path, err) = Readlink(procfn); 

    // When the executable has been deleted then Readlink returns a
    // path appended with " (deleted)".
    return (stringsTrimSuffix(path, " (deleted)"), error.As(err)!);

}

// stringsTrimSuffix is the same as strings.TrimSuffix.
private static @string stringsTrimSuffix(@string s, @string suffix) {
    if (len(s) >= len(suffix) && s[(int)len(s) - len(suffix)..] == suffix) {
        return s[..(int)len(s) - len(suffix)];
    }
    return s;

}

} // end os_package
