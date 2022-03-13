// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || (js && wasm) || linux || netbsd || openbsd || solaris
// +build aix darwin dragonfly freebsd js,wasm linux netbsd openbsd solaris

// package os -- go2cs converted at 2022 March 13 05:28:03 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\path_unix.go
namespace go;

public static partial class os_package {

public static readonly char PathSeparator = '/'; // OS-specific path separator
public static readonly char PathListSeparator = ':'; // OS-specific path list separator

// IsPathSeparator reports whether c is a directory separator character.
public static bool IsPathSeparator(byte c) {
    return PathSeparator == c;
}

// basename removes trailing slashes and the leading directory name from path name.
private static @string basename(@string name) {
    var i = len(name) - 1; 
    // Remove trailing slashes
    while (i > 0 && name[i] == '/') {
        name = name[..(int)i];
        i--;
    } 
    // Remove leading directory name
    i--;

    while (i >= 0) {
        if (name[i] == '/') {
            name = name[(int)i + 1..];
            break;
        i--;
        }
    }

    return name;
}

// splitPath returns the base name and parent directory.
private static (@string, @string) splitPath(@string path) {
    @string _p0 = default;
    @string _p0 = default;
 
    // if no better parent is found, the path is relative from "here"
    @string dirname = "."; 

    // Remove all but one leading slash.
    while (len(path) > 1 && path[0] == '/' && path[1] == '/') {
        path = path[(int)1..];
    }

    var i = len(path) - 1; 

    // Remove trailing slashes.
    while (i > 0 && path[i] == '/') {
        path = path[..(int)i];
        i--;
    } 

    // if no slashes in path, base is path
    var basename = path; 

    // Remove leading directory path
    i--;

    while (i >= 0) {
        if (path[i] == '/') {
            if (i == 0) {
                dirname = path[..(int)1];
        i--;
            }
            else
 {
                dirname = path[..(int)i];
            }
            basename = path[(int)i + 1..];
            break;
        }
    }

    return (dirname, basename);
}

private static @string fixRootDirectory(@string p) {
    return p;
}

} // end os_package
