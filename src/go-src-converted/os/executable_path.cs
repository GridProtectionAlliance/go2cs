// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || openbsd
// +build aix openbsd

// package os -- go2cs converted at 2022 March 13 05:27:55 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\executable_path.go
namespace go;

public static partial class os_package {

// We query the working directory at init, to use it later to search for the
// executable file
// errWd will be checked later, if we need to use initWd


private static (@string, error) executable() {
    @string _p0 = default;
    error _p0 = default!;

    @string exePath = default;
    if (len(Args) == 0 || Args[0] == "") {
        return ("", error.As(ErrNotExist)!);
    }
    if (IsPathSeparator(Args[0][0])) { 
        // Args[0] is an absolute path, so it is the executable.
        // Note that we only need to worry about Unix paths here.
        exePath = Args[0];
    }
    else
 {
        for (nint i = 1; i < len(Args[0]); i++) {
            if (IsPathSeparator(Args[0][i])) { 
                // Args[0] is a relative path: prepend the
                // initial working directory.
                if (errWd != null) {
                    return ("", error.As(errWd)!);
                }
                exePath = initWd + string(PathSeparator) + Args[0];
                break;
            }
        }
    }
    if (exePath != "") {
        {
            var err = isExecutable(exePath);

            if (err != null) {
                return ("", error.As(err)!);
            }

        }
        return (exePath, error.As(null!)!);
    }
    foreach (var (_, dir) in splitPathList(Getenv("PATH"))) {
        if (len(dir) == 0) {
            dir = ".";
        }
        if (!IsPathSeparator(dir[0])) {
            if (errWd != null) {
                return ("", error.As(errWd)!);
            }
            dir = initWd + string(PathSeparator) + dir;
        }
        exePath = dir + string(PathSeparator) + Args[0];

        if (isExecutable(exePath) == null) 
            return (exePath, error.As(null!)!);
        else if (isExecutable(exePath) == ErrPermission) 
            return ("", error.As(ErrPermission)!);
            }    return ("", error.As(ErrNotExist)!);
}

// isExecutable returns an error if a given file is not an executable.
private static error isExecutable(@string path) {
    var (stat, err) = Stat(path);
    if (err != null) {
        return error.As(err)!;
    }
    var mode = stat.Mode();
    if (!mode.IsRegular()) {
        return error.As(ErrPermission)!;
    }
    if ((mode & 0111) == 0) {
        return error.As(ErrPermission)!;
    }
    return error.As(null!)!;
}

// splitPathList splits a path list.
// This is based on genSplit from strings/strings.go
private static slice<@string> splitPathList(@string pathList) {
    if (pathList == "") {
        return null;
    }
    nint n = 1;
    {
        nint i__prev1 = i;

        for (nint i = 0; i < len(pathList); i++) {
            if (pathList[i] == PathListSeparator) {
                n++;
            }
        }

        i = i__prev1;
    }
    nint start = 0;
    var a = make_slice<@string>(n);
    nint na = 0;
    {
        nint i__prev1 = i;

        for (i = 0; i + 1 <= len(pathList) && na + 1 < n; i++) {
            if (pathList[i] == PathListSeparator) {
                a[na] = pathList[(int)start..(int)i];
                na++;
                start = i + 1;
            }
        }

        i = i__prev1;
    }
    a[na] = pathList[(int)start..];
    return a[..(int)na + 1];
}

} // end os_package
