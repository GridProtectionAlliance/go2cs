// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package fs -- go2cs converted at 2022 March 13 05:27:46 UTC
// import "io/fs" ==> using fs = go.io.fs_package
// Original source: C:\Program Files\Go\src\io\fs\glob.go
namespace go.io;

using path = path_package;


// A GlobFS is a file system with a Glob method.

public static partial class fs_package {

public partial interface GlobFS {
    (slice<@string>, error) Glob(@string pattern);
}

// Glob returns the names of all files matching pattern or nil
// if there is no matching file. The syntax of patterns is the same
// as in path.Match. The pattern may describe hierarchical names such as
// usr/*/bin/ed.
//
// Glob ignores file system errors such as I/O errors reading directories.
// The only possible returned error is path.ErrBadPattern, reporting that
// the pattern is malformed.
//
// If fs implements GlobFS, Glob calls fs.Glob.
// Otherwise, Glob uses ReadDir to traverse the directory tree
// and look for matches for the pattern.
public static (slice<@string>, error) Glob(FS fsys, @string pattern) {
    slice<@string> matches = default;
    error err = default!;

    {
        GlobFS (fsys, ok) = GlobFS.As(fsys._<GlobFS>())!;

        if (ok) {
            return fsys.Glob(pattern);
        }
    } 

    // Check pattern is well-formed.
    {
        var (_, err) = path.Match(pattern, "");

        if (err != null) {
            return (null, error.As(err)!);
        }
    }
    if (!hasMeta(pattern)) {
        _, err = Stat(fsys, pattern);

        if (err != null) {
            return (null, error.As(null!)!);
        }
        return (new slice<@string>(new @string[] { pattern }), error.As(null!)!);
    }
    var (dir, file) = path.Split(pattern);
    dir = cleanGlobPath(dir);

    if (!hasMeta(dir)) {
        return glob(fsys, dir, file, null);
    }
    if (dir == pattern) {
        return (null, error.As(path.ErrBadPattern)!);
    }
    slice<@string> m = default;
    m, err = Glob(fsys, dir);
    if (err != null) {
        return ;
    }
    foreach (var (_, d) in m) {
        matches, err = glob(fsys, d, file, matches);
        if (err != null) {
            return ;
        }
    }    return ;
}

// cleanGlobPath prepares path for glob matching.
private static @string cleanGlobPath(@string path) {
    switch (path) {
        case "": 
            return ".";
            break;
        default: 
            return path[(int)0..(int)len(path) - 1]; // chop off trailing separator
            break;
    }
}

// glob searches for files matching pattern in the directory dir
// and appends them to matches, returning the updated slice.
// If the directory cannot be opened, glob returns the existing matches.
// New matches are added in lexicographical order.
private static (slice<@string>, error) glob(FS fs, @string dir, @string pattern, slice<@string> matches) {
    slice<@string> m = default;
    error e = default!;

    m = matches;
    var (infos, err) = ReadDir(fs, dir);
    if (err != null) {
        return ; // ignore I/O error
    }
    foreach (var (_, info) in infos) {
        var n = info.Name();
        var (matched, err) = path.Match(pattern, n);
        if (err != null) {
            return (m, error.As(err)!);
        }
        if (matched) {
            m = append(m, path.Join(dir, n));
        }
    }    return ;
}

// hasMeta reports whether path contains any of the magic characters
// recognized by path.Match.
private static bool hasMeta(@string path) {
    for (nint i = 0; i < len(path); i++) {
        switch (path[i]) {
            case '*': 

            case '?': 

            case '[': 

            case '\\': 
                return true;
                break;
        }
    }
    return false;
}

} // end fs_package
