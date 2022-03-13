// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || (js && wasm) || linux || netbsd || openbsd || solaris
// +build aix darwin dragonfly freebsd js,wasm linux netbsd openbsd solaris

// package filepath -- go2cs converted at 2022 March 13 05:28:16 UTC
// import "path/filepath" ==> using filepath = go.path.filepath_package
// Original source: C:\Program Files\Go\src\path\filepath\path_unix.go
namespace go.path;

using strings = strings_package;

public static partial class filepath_package {

// IsAbs reports whether the path is absolute.
public static bool IsAbs(@string path) {
    return strings.HasPrefix(path, "/");
}

// volumeNameLen returns length of the leading volume name on Windows.
// It returns 0 elsewhere.
private static nint volumeNameLen(@string path) {
    return 0;
}

// HasPrefix exists for historical compatibility and should not be used.
//
// Deprecated: HasPrefix does not respect path boundaries and
// does not ignore case when required.
public static bool HasPrefix(@string p, @string prefix) {
    return strings.HasPrefix(p, prefix);
}

private static slice<@string> splitList(@string path) {
    if (path == "") {
        return new slice<@string>(new @string[] {  });
    }
    return strings.Split(path, string(ListSeparator));
}

private static (@string, error) abs(@string path) {
    @string _p0 = default;
    error _p0 = default!;

    return unixAbs(path);
}

private static @string join(slice<@string> elem) { 
    // If there's a bug here, fix the logic in ./path_plan9.go too.
    foreach (var (i, e) in elem) {
        if (e != "") {
            return Clean(strings.Join(elem[(int)i..], string(Separator)));
        }
    }    return "";
}

private static bool sameWord(@string a, @string b) {
    return a == b;
}

} // end filepath_package
