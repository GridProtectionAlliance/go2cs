// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package path implements utility routines for manipulating slash-separated
// paths.
//
// The path package should only be used for paths separated by forward
// slashes, such as the paths in URLs. This package does not deal with
// Windows paths with drive letters or backslashes; to manipulate
// operating system paths, use the path/filepath package.

// package path -- go2cs converted at 2022 March 13 05:41:54 UTC
// import "path" ==> using path = go.path_package
// Original source: C:\Program Files\Go\src\path\path.go
namespace go;

public static partial class path_package {

// A lazybuf is a lazily constructed path buffer.
// It supports append, reading previously appended bytes,
// and retrieving the final string. It does not allocate a buffer
// to hold the output until that output diverges from s.
private partial struct lazybuf {
    public @string s;
    public slice<byte> buf;
    public nint w;
}

private static byte index(this ptr<lazybuf> _addr_b, nint i) {
    ref lazybuf b = ref _addr_b.val;

    if (b.buf != null) {
        return b.buf[i];
    }
    return b.s[i];
}

private static void append(this ptr<lazybuf> _addr_b, byte c) {
    ref lazybuf b = ref _addr_b.val;

    if (b.buf == null) {
        if (b.w < len(b.s) && b.s[b.w] == c) {
            b.w++;
            return ;
        }
        b.buf = make_slice<byte>(len(b.s));
        copy(b.buf, b.s[..(int)b.w]);
    }
    b.buf[b.w] = c;
    b.w++;
}

private static @string @string(this ptr<lazybuf> _addr_b) {
    ref lazybuf b = ref _addr_b.val;

    if (b.buf == null) {
        return b.s[..(int)b.w];
    }
    return string(b.buf[..(int)b.w]);
}

// Clean returns the shortest path name equivalent to path
// by purely lexical processing. It applies the following rules
// iteratively until no further processing can be done:
//
//    1. Replace multiple slashes with a single slash.
//    2. Eliminate each . path name element (the current directory).
//    3. Eliminate each inner .. path name element (the parent directory)
//       along with the non-.. element that precedes it.
//    4. Eliminate .. elements that begin a rooted path:
//       that is, replace "/.." by "/" at the beginning of a path.
//
// The returned path ends in a slash only if it is the root "/".
//
// If the result of this process is an empty string, Clean
// returns the string ".".
//
// See also Rob Pike, ``Lexical File Names in Plan 9 or
// Getting Dot-Dot Right,''
// https://9p.io/sys/doc/lexnames.html
public static @string Clean(@string path) {
    if (path == "") {
        return ".";
    }
    var rooted = path[0] == '/';
    var n = len(path); 

    // Invariants:
    //    reading from path; r is index of next byte to process.
    //    writing to buf; w is index of next byte to write.
    //    dotdot is index in buf where .. must stop, either because
    //        it is the leading slash or it is a leading ../../.. prefix.
    lazybuf @out = new lazybuf(s:path);
    nint r = 0;
    nint dotdot = 0;
    if (rooted) {
        @out.append('/');
        (r, dotdot) = (1, 1);
    }
    while (r < n) {

        if (path[r] == '/') 
            // empty path element
            r++;
        else if (path[r] == '.' && (r + 1 == n || path[r + 1] == '/')) 
            // . element
            r++;
        else if (path[r] == '.' && path[r + 1] == '.' && (r + 2 == n || path[r + 2] == '/')) 
            // .. element: remove to last /
            r += 2;

            if (@out.w > dotdot) 
                // can backtrack
                @out.w--;
                while (@out.w > dotdot && @out.index(@out.w) != '/') {
                    @out.w--;
                }
            else if (!rooted) 
                // cannot backtrack, but not rooted, so append .. element.
                if (@out.w > 0) {
                    @out.append('/');
                }
                @out.append('.');
                @out.append('.');
                dotdot = @out.w;
                    else 
            // real path element.
            // add slash if needed
            if (rooted && @out.w != 1 || !rooted && @out.w != 0) {
                @out.append('/');
            } 
            // copy element
            while (r < n && path[r] != '/') {
                @out.append(path[r]);
                r++;
            }
            } 

    // Turn empty string into "."
    if (@out.w == 0) {
        return ".";
    }
    return @out.@string();
}

// lastSlash(s) is strings.LastIndex(s, "/") but we can't import strings.
private static nint lastSlash(@string s) {
    var i = len(s) - 1;
    while (i >= 0 && s[i] != '/') {
        i--;
    }
    return i;
}

// Split splits path immediately following the final slash,
// separating it into a directory and file name component.
// If there is no slash in path, Split returns an empty dir and
// file set to path.
// The returned values have the property that path = dir+file.
public static (@string, @string) Split(@string path) {
    @string dir = default;
    @string file = default;

    var i = lastSlash(path);
    return (path[..(int)i + 1], path[(int)i + 1..]);
}

// Join joins any number of path elements into a single path,
// separating them with slashes. Empty elements are ignored.
// The result is Cleaned. However, if the argument list is
// empty or all its elements are empty, Join returns
// an empty string.
public static @string Join(params @string[] elem) {
    elem = elem.Clone();

    nint size = 0;
    {
        var e__prev1 = e;

        foreach (var (_, __e) in elem) {
            e = __e;
            size += len(e);
        }
        e = e__prev1;
    }

    if (size == 0) {
        return "";
    }
    var buf = make_slice<byte>(0, size + len(elem) - 1);
    {
        var e__prev1 = e;

        foreach (var (_, __e) in elem) {
            e = __e;
            if (len(buf) > 0 || e != "") {
                if (len(buf) > 0) {
                    buf = append(buf, '/');
                }
                buf = append(buf, e);
            }
        }
        e = e__prev1;
    }

    return Clean(string(buf));
}

// Ext returns the file name extension used by path.
// The extension is the suffix beginning at the final dot
// in the final slash-separated element of path;
// it is empty if there is no dot.
public static @string Ext(@string path) {
    for (var i = len(path) - 1; i >= 0 && path[i] != '/'; i--) {
        if (path[i] == '.') {
            return path[(int)i..];
        }
    }
    return "";
}

// Base returns the last element of path.
// Trailing slashes are removed before extracting the last element.
// If the path is empty, Base returns ".".
// If the path consists entirely of slashes, Base returns "/".
public static @string Base(@string path) {
    if (path == "") {
        return ".";
    }
    while (len(path) > 0 && path[len(path) - 1] == '/') {
        path = path[(int)0..(int)len(path) - 1];
    } 
    // Find the last element
    {
        var i = lastSlash(path);

        if (i >= 0) {
            path = path[(int)i + 1..];
        }
    } 
    // If empty now, it had only slashes.
    if (path == "") {
        return "/";
    }
    return path;
}

// IsAbs reports whether the path is absolute.
public static bool IsAbs(@string path) {
    return len(path) > 0 && path[0] == '/';
}

// Dir returns all but the last element of path, typically the path's directory.
// After dropping the final element using Split, the path is Cleaned and trailing
// slashes are removed.
// If the path is empty, Dir returns ".".
// If the path consists entirely of slashes followed by non-slash bytes, Dir
// returns a single slash. In any other case, the returned path does not end in a
// slash.
public static @string Dir(@string path) {
    var (dir, _) = Split(path);
    return Clean(dir);
}

} // end path_package
