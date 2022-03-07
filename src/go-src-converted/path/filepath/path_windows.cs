// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package filepath -- go2cs converted at 2022 March 06 22:14:07 UTC
// import "path/filepath" ==> using filepath = go.path.filepath_package
// Original source: C:\Program Files\Go\src\path\filepath\path_windows.go
using strings = go.strings_package;
using syscall = go.syscall_package;

namespace go.path;

public static partial class filepath_package {

private static bool isSlash(byte c) {
    return c == '\\' || c == '/';
}

// reservedNames lists reserved Windows names. Search for PRN in
// https://docs.microsoft.com/en-us/windows/desktop/fileio/naming-a-file
// for details.
private static @string reservedNames = new slice<@string>(new @string[] { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" });

// isReservedName returns true, if path is Windows reserved name.
// See reservedNames for the full list.
private static bool isReservedName(@string path) {
    if (len(path) == 0) {
        return false;
    }
    foreach (var (_, reserved) in reservedNames) {
        if (strings.EqualFold(path, reserved)) {
            return true;
        }
    }    return false;

}

// IsAbs reports whether the path is absolute.
public static bool IsAbs(@string path) {
    bool b = default;

    if (isReservedName(path)) {
        return true;
    }
    var l = volumeNameLen(path);
    if (l == 0) {
        return false;
    }
    path = path[(int)l..];
    if (path == "") {
        return false;
    }
    return isSlash(path[0]);

}

// volumeNameLen returns length of the leading volume name on Windows.
// It returns 0 elsewhere.
private static nint volumeNameLen(@string path) {
    if (len(path) < 2) {
        return 0;
    }
    var c = path[0];
    if (path[1] == ':' && ('a' <= c && c <= 'z' || 'A' <= c && c <= 'Z')) {
        return 2;
    }
    {
        var l = len(path);

        if (l >= 5 && isSlash(path[0]) && isSlash(path[1]) && !isSlash(path[2]) && path[2] != '.') { 
            // first, leading `\\` and next shouldn't be `\`. its server name.
            for (nint n = 3; n < l - 1; n++) { 
                // second, next '\' shouldn't be repeated.
                if (isSlash(path[n])) {
                    n++; 
                    // third, following something characters. its share name.
                    if (!isSlash(path[n])) {
                        if (path[n] == '.') {
                            break;
                        }
                        while (n < l) {
                            if (isSlash(path[n])) {
                                break;
                            n++;
                            }

                        }

                        return n;

                    }

                    break;

                }

            }


        }
    }

    return 0;

}

// HasPrefix exists for historical compatibility and should not be used.
//
// Deprecated: HasPrefix does not respect path boundaries and
// does not ignore case when required.
public static bool HasPrefix(@string p, @string prefix) {
    if (strings.HasPrefix(p, prefix)) {
        return true;
    }
    return strings.HasPrefix(strings.ToLower(p), strings.ToLower(prefix));

}

private static slice<@string> splitList(@string path) { 
    // The same implementation is used in LookPath in os/exec;
    // consider changing os/exec when changing this.

    if (path == "") {
        return new slice<@string>(new @string[] {  });
    }
    @string list = new slice<@string>(new @string[] {  });
    nint start = 0;
    var quo = false;
    {
        nint i__prev1 = i;

        for (nint i = 0; i < len(path); i++) {
            {
                var c = path[i];


                if (c == '"') 
                    quo = !quo;
                else if (c == ListSeparator && !quo) 
                    list = append(list, path[(int)start..(int)i]);
                    start = i + 1;

            }

        }

        i = i__prev1;
    }
    list = append(list, path[(int)start..]); 

    // Remove quotes.
    {
        nint i__prev1 = i;

        foreach (var (__i, __s) in list) {
            i = __i;
            s = __s;
            list[i] = strings.ReplaceAll(s, "\"", "");
        }
        i = i__prev1;
    }

    return list;

}

private static (@string, error) abs(@string path) {
    @string _p0 = default;
    error _p0 = default!;

    if (path == "") { 
        // syscall.FullPath returns an error on empty path, because it's not a valid path.
        // To implement Abs behavior of returning working directory on empty string input,
        // special-case empty path by changing it to "." path. See golang.org/issue/24441.
        path = ".";

    }
    var (fullPath, err) = syscall.FullPath(path);
    if (err != null) {
        return ("", error.As(err)!);
    }
    return (Clean(fullPath), error.As(null!)!);

}

private static @string join(slice<@string> elem) {
    foreach (var (i, e) in elem) {
        if (e != "") {
            return joinNonEmpty(elem[(int)i..]);
        }
    }    return "";

}

// joinNonEmpty is like join, but it assumes that the first element is non-empty.
private static @string joinNonEmpty(slice<@string> elem) {
    if (len(elem[0]) == 2 && elem[0][1] == ':') { 
        // First element is drive letter without terminating slash.
        // Keep path relative to current directory on that drive.
        // Skip empty elements.
        nint i = 1;
        while (i < len(elem)) {
            if (elem[i] != "") {
                break;
            i++;
            }

        }
        return Clean(elem[0] + strings.Join(elem[(int)i..], string(Separator)));

    }
    var p = Clean(strings.Join(elem, string(Separator)));
    if (!isUNC(p)) {
        return p;
    }
    var head = Clean(elem[0]);
    if (isUNC(head)) {
        return p;
    }
    var tail = Clean(strings.Join(elem[(int)1..], string(Separator)));
    if (head[len(head) - 1] == Separator) {
        return head + tail;
    }
    return head + string(Separator) + tail;

}

// isUNC reports whether path is a UNC path.
private static bool isUNC(@string path) {
    return volumeNameLen(path) > 2;
}

private static bool sameWord(@string a, @string b) {
    return strings.EqualFold(a, b);
}

} // end filepath_package
