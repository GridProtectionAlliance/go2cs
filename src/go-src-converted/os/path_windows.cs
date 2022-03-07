// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2022 March 06 22:13:43 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\path_windows.go


namespace go;

public static partial class os_package {

public static readonly char PathSeparator = '\\'; // OS-specific path separator
public static readonly char PathListSeparator = ';'; // OS-specific path list separator

// IsPathSeparator reports whether c is a directory separator character.
public static bool IsPathSeparator(byte c) { 
    // NOTE: Windows accept / as path separator.
    return c == '\\' || c == '/';

}

// basename removes trailing slashes and the leading
// directory name and drive letter from path name.
private static @string basename(@string name) { 
    // Remove drive letter
    if (len(name) == 2 && name[1] == ':') {
        name = ".";
    }
    else if (len(name) > 2 && name[1] == ':') {
        name = name[(int)2..];
    }
    var i = len(name) - 1; 
    // Remove trailing slashes
    while (i > 0 && (name[i] == '/' || name[i] == '\\')) {
        name = name[..(int)i];
        i--;
    } 
    // Remove leading directory name
    i--;

    while (i >= 0) {
        if (name[i] == '/' || name[i] == '\\') {
            name = name[(int)i + 1..];
            break;
        i--;
        }
    }
    return name;

}

private static bool isAbs(@string path) {
    bool b = default;

    var v = volumeName(path);
    if (v == "") {
        return false;
    }
    path = path[(int)len(v)..];
    if (path == "") {
        return false;
    }
    return IsPathSeparator(path[0]);

}

private static @string volumeName(@string path) {
    @string v = default;

    if (len(path) < 2) {
        return "";
    }
    var c = path[0];
    if (path[1] == ':' && ('0' <= c && c <= '9' || 'a' <= c && c <= 'z' || 'A' <= c && c <= 'Z')) {
        return path[..(int)2];
    }
    {
        var l = len(path);

        if (l >= 5 && IsPathSeparator(path[0]) && IsPathSeparator(path[1]) && !IsPathSeparator(path[2]) && path[2] != '.') { 
            // first, leading `\\` and next shouldn't be `\`. its server name.
            for (nint n = 3; n < l - 1; n++) { 
                // second, next '\' shouldn't be repeated.
                if (IsPathSeparator(path[n])) {
                    n++; 
                    // third, following something characters. its share name.
                    if (!IsPathSeparator(path[n])) {
                        if (path[n] == '.') {
                            break;
                        }
                        while (n < l) {
                            if (IsPathSeparator(path[n])) {
                                break;
                            n++;
                            }

                        }

                        return path[..(int)n];

                    }

                    break;

                }

            }


        }
    }

    return "";

}

private static @string fromSlash(@string path) { 
    // Replace each '/' with '\\' if present
    slice<byte> pathbuf = default;
    nint lastSlash = default;
    foreach (var (i, b) in path) {
        if (b == '/') {
            if (pathbuf == null) {
                pathbuf = make_slice<byte>(len(path));
            }
            copy(pathbuf[(int)lastSlash..], path[(int)lastSlash..(int)i]);
            pathbuf[i] = '\\';
            lastSlash = i + 1;
        }
    }    if (pathbuf == null) {
        return path;
    }
    copy(pathbuf[(int)lastSlash..], path[(int)lastSlash..]);
    return string(pathbuf);

}

private static @string dirname(@string path) {
    var vol = volumeName(path);
    var i = len(path) - 1;
    while (i >= len(vol) && !IsPathSeparator(path[i])) {
        i--;
    }
    var dir = path[(int)len(vol)..(int)i + 1];
    var last = len(dir) - 1;
    if (last > 0 && IsPathSeparator(dir[last])) {
        dir = dir[..(int)last];
    }
    if (dir == "") {
        dir = ".";
    }
    return vol + dir;

}

// This is set via go:linkname on runtime.canUseLongPaths, and is true when the OS
// supports opting into proper long path handling without the need for fixups.
private static bool canUseLongPaths = default;

// fixLongPath returns the extended-length (\\?\-prefixed) form of
// path when needed, in order to avoid the default 260 character file
// path limit imposed by Windows. If path is not easily converted to
// the extended-length form (for example, if path is a relative path
// or contains .. elements), or is short enough, fixLongPath returns
// path unmodified.
//
// See https://msdn.microsoft.com/en-us/library/windows/desktop/aa365247(v=vs.85).aspx#maxpath
private static @string fixLongPath(@string path) {
    if (canUseLongPaths) {
        return path;
    }
    if (len(path) < 248) { 
        // Don't fix. (This is how Go 1.7 and earlier worked,
        // not automatically generating the \\?\ form)
        return path;

    }
    if (len(path) >= 2 && path[..(int)2] == "\\\\") { 
        // Don't canonicalize UNC paths.
        return path;

    }
    if (!isAbs(path)) { 
        // Relative path
        return path;

    }
    const @string prefix = "\\\\?";



    var pathbuf = make_slice<byte>(len(prefix) + len(path) + len("\\"));
    copy(pathbuf, prefix);
    var n = len(path);
    nint r = 0;
    var w = len(prefix);
    while (r < n) {

        if (IsPathSeparator(path[r])) 
            // empty block
            r++;
        else if (path[r] == '.' && (r + 1 == n || IsPathSeparator(path[r + 1]))) 
            // /./
            r++;
        else if (r + 1 < n && path[r] == '.' && path[r + 1] == '.' && (r + 2 == n || IsPathSeparator(path[r + 2]))) 
            // /../ is currently unhandled
            return path;
        else 
            pathbuf[w] = '\\';
            w++;
            while (r < n && !IsPathSeparator(path[r])) {
                pathbuf[w] = path[r];
                w++;
                r++;
            }
        
    } 
    // A drive's root directory needs a trailing \
    if (w == len("\\\\?\\c:")) {
        pathbuf[w] = '\\';
        w++;
    }
    return string(pathbuf[..(int)w]);

}

// fixRootDirectory fixes a reference to a drive's root directory to
// have the required trailing slash.
private static @string fixRootDirectory(@string p) {
    if (len(p) == len("\\\\?\\c:")) {
        if (IsPathSeparator(p[0]) && IsPathSeparator(p[1]) && p[2] == '?' && IsPathSeparator(p[3]) && p[5] == ':') {
            return p + "\\";
        }
    }
    return p;

}

} // end os_package
