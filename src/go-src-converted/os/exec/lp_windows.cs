// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package exec -- go2cs converted at 2022 March 06 22:14:24 UTC
// import "os/exec" ==> using exec = go.os.exec_package
// Original source: C:\Program Files\Go\src\os\exec\lp_windows.go
using errors = go.errors_package;
using fs = go.io.fs_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;

namespace go.os;

public static partial class exec_package {

    // ErrNotFound is the error resulting if a path search failed to find an executable file.
public static var ErrNotFound = errors.New("executable file not found in %PATH%");

private static error chkStat(@string file) {
    var (d, err) = os.Stat(file);
    if (err != null) {
        return error.As(err)!;
    }
    if (d.IsDir()) {
        return error.As(fs.ErrPermission)!;
    }
    return error.As(null!)!;

}

private static bool hasExt(@string file) {
    var i = strings.LastIndex(file, ".");
    if (i < 0) {
        return false;
    }
    return strings.LastIndexAny(file, ":\\/") < i;

}

private static (@string, error) findExecutable(@string file, slice<@string> exts) {
    @string _p0 = default;
    error _p0 = default!;

    if (len(exts) == 0) {
        return (file, error.As(chkStat(file))!);
    }
    if (hasExt(file)) {
        if (chkStat(file) == null) {
            return (file, error.As(null!)!);
        }
    }
    foreach (var (_, e) in exts) {
        {
            var f = file + e;

            if (chkStat(f) == null) {
                return (f, error.As(null!)!);
            }

        }

    }    return ("", error.As(fs.ErrNotExist)!);

}

// LookPath searches for an executable named file in the
// directories named by the PATH environment variable.
// If file contains a slash, it is tried directly and the PATH is not consulted.
// LookPath also uses PATHEXT environment variable to match
// a suitable candidate.
// The result may be an absolute path or a path relative to the current directory.
public static (@string, error) LookPath(@string file) {
    @string _p0 = default;
    error _p0 = default!;

    slice<@string> exts = default;
    var x = os.Getenv("PATHEXT");
    if (x != "") {
        foreach (var (_, e) in strings.Split(strings.ToLower(x), ";")) {
            if (e == "") {
                continue;
            }
            if (e[0] != '.') {
                e = "." + e;
            }
            exts = append(exts, e);
        }
    else
    } {
        exts = new slice<@string>(new @string[] { ".com", ".exe", ".bat", ".cmd" });
    }
    if (strings.ContainsAny(file, ":\\/")) {
        {
            var f__prev2 = f;

            var (f, err) = findExecutable(file, exts);

            if (err == null) {
                return (f, error.As(null!)!);
            }
            else
 {
                return ("", error.As(addr(new Error(file,err))!)!);
            }

            f = f__prev2;

        }

    }
    {
        var f__prev1 = f;

        (f, err) = findExecutable(filepath.Join(".", file), exts);

        if (err == null) {
            return (f, error.As(null!)!);
        }
        f = f__prev1;

    }

    var path = os.Getenv("path");
    foreach (var (_, dir) in filepath.SplitList(path)) {
        {
            var f__prev1 = f;

            (f, err) = findExecutable(filepath.Join(dir, file), exts);

            if (err == null) {
                return (f, error.As(null!)!);
            }

            f = f__prev1;

        }

    }    return ("", error.As(addr(new Error(file,ErrNotFound))!)!);

}

} // end exec_package
