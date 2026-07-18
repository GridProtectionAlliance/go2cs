// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using Δpath = go.path_package;
using go;

partial class path_test_package {

public static void ExampleBase() {
    fmt.Println(Δpath.Base("/a/b"u8));
    fmt.Println(Δpath.Base("/"u8));
    fmt.Println(Δpath.Base(""u8));
}

// Output:
// b
// /
// .
public static void ExampleClean() {
    var paths = new @string[]{
        "a/c",
        "a//c",
        "a/c/.",
        "a/c/b/..",
        "/../a/c",
        "/../a/b/../././/c",
        ""
    }.slice();
    foreach (var (_, p) in paths) {
        fmt.Printf("Clean(%q) = %q\n"u8, p, Δpath.Clean(p));
    }
}

// Output:
// Clean("a/c") = "a/c"
// Clean("a//c") = "a/c"
// Clean("a/c/.") = "a/c"
// Clean("a/c/b/..") = "a/c"
// Clean("/../a/c") = "/a/c"
// Clean("/../a/b/../././/c") = "/a/c"
// Clean("") = "."
public static void ExampleDir() {
    fmt.Println(Δpath.Dir("/a/b/c"u8));
    fmt.Println(Δpath.Dir("a/b/c"u8));
    fmt.Println(Δpath.Dir("/a/"u8));
    fmt.Println(Δpath.Dir("a/"u8));
    fmt.Println(Δpath.Dir("/"u8));
    fmt.Println(Δpath.Dir(""u8));
}

// Output:
// /a/b
// a/b
// /a
// a
// /
// .
public static void ExampleExt() {
    fmt.Println(Δpath.Ext("/a/b/c/bar.css"u8));
    fmt.Println(Δpath.Ext("/"u8));
    fmt.Println(Δpath.Ext(""u8));
}

// Output:
// .css
//
//
public static void ExampleIsAbs() {
    fmt.Println(Δpath.IsAbs("/dev/null"u8));
}

// Output: true
public static void ExampleJoin() {
    fmt.Println(Δpath.Join("a"u8, "b", "c"));
    fmt.Println(Δpath.Join("a"u8, "b/c"));
    fmt.Println(Δpath.Join("a/b"u8, "c"));
    fmt.Println(Δpath.Join("a/b"u8, "../../../xyz"));
    fmt.Println(Δpath.Join(""u8, ""));
    fmt.Println(Δpath.Join("a"u8, ""));
    fmt.Println(Δpath.Join(""u8, "a"));
}

// Output:
// a/b/c
// a/b/c
// a/b/c
// ../xyz
//
// a
// a
public static void ExampleMatch() {
    var (ᴛ1, ᴛ2) = Δpath.Match("abc"u8, "abc"u8);
    fmt.Println(ᴛ1, ᴛ2);
    var (ᴛ3, ᴛ4) = Δpath.Match("a*"u8, "abc"u8);
    fmt.Println(ᴛ3, ᴛ4);
    var (ᴛ5, ᴛ6) = Δpath.Match("a*/b"u8, "a/c/b"u8);
    fmt.Println(ᴛ5, ᴛ6);
}

// Output:
// true <nil>
// true <nil>
// false <nil>
public static void ExampleSplit() {
    var split = (@string s) => {
        var (dir, @file) = Δpath.Split(s);
        fmt.Printf("path.Split(%q) = dir: %q, file: %q\n"u8, s, dir, @file);
    };
    split("static/myfile.css"u8);
    split("myfile.css"u8);
    split(""u8);
}

// Output:
// path.Split("static/myfile.css") = dir: "static/", file: "myfile.css"
// path.Split("myfile.css") = dir: "", file: "myfile.css"
// path.Split("") = dir: "", file: ""

} // end path_test_package
