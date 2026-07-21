// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using maps = go.maps_package;
using strings = strings_package;
using go;

partial class maps_test_package {

public static void ExampleClone() {
    var m1 = new map<@string, nint>{
        ["key"u8] = 1
    };
    var m2 = maps.Clone<map<@string, nint>, @string, nint>(m1);
    m2["key"u8] = 100;
    fmt.Println(m1["key"u8]);
    fmt.Println(m2["key"u8]);
    var m3 = new map<@string, slice<nint>>{
        ["key"u8] = new nint[]{1, 2, 3}.slice()
    };
    var m4 = maps.Clone<map<@string, slice<nint>>, @string, slice<nint>>(m3);
    fmt.Println(m4["key"u8][0]);
    m4["key"u8][0] = 100;
    fmt.Println(m3["key"u8][0]);
    fmt.Println(m4["key"u8][0]);
}

// Output:
// 1
// 100
// 1
// 100
// 100
public static void ExampleCopy() {
    var m1 = new map<@string, nint>{
        ["one"u8] = 1,
        ["two"u8] = 2
    };
    var m2 = new map<@string, nint>{
        ["one"u8] = 10
    };
    maps.Copy<map<@string, nint>, map<@string, nint>, @string, nint>(m2, m1);
    fmt.Println("m2 is:", m2);
    m2["one"u8] = 100;
    fmt.Println("m1 is:", m1);
    fmt.Println("m2 is:", m2);
    var m3 = new map<@string, slice<nint>>{
        ["one"u8] = new nint[]{1, 2, 3}.slice(),
        ["two"u8] = new nint[]{4, 5, 6}.slice()
    };
    var m4 = new map<@string, slice<nint>>{
        ["one"u8] = new nint[]{7, 8, 9}.slice()
    };
    maps.Copy<map<@string, slice<nint>>, map<@string, slice<nint>>, @string, slice<nint>>(m4, m3);
    fmt.Println("m4 is:", m4);
    m4["one"u8][0] = 100;
    fmt.Println("m3 is:", m3);
    fmt.Println("m4 is:", m4);
}

// Output:
// m2 is: map[one:1 two:2]
// m1 is: map[one:1 two:2]
// m2 is: map[one:100 two:2]
// m4 is: map[one:[1 2 3] two:[4 5 6]]
// m3 is: map[one:[100 2 3] two:[4 5 6]]
// m4 is: map[one:[100 2 3] two:[4 5 6]]
public static void ExampleDeleteFunc() {
    var m = new map<@string, nint>{
        ["one"u8] = 1,
        ["two"u8] = 2,
        ["three"u8] = 3,
        ["four"u8] = 4
    };
    maps.DeleteFunc(m, (@string k, nint v) => v % 2 != 0);
    // delete odd values
    fmt.Println(m);
}

// Output:
// map[four:4 two:2]
public static void ExampleEqual() {
    var m1 = new map<nint, @string>{
        [1] = "one"u8,
        [10] = "Ten"u8,
        [1000] = "THOUSAND"u8
    };
    var m2 = new map<nint, @string>{
        [1] = "one"u8,
        [10] = "Ten"u8,
        [1000] = "THOUSAND"u8
    };
    var m3 = new map<nint, @string>{
        [1] = "one"u8,
        [10] = "ten"u8,
        [1000] = "thousand"u8
    };
    fmt.Println(maps.Equal<map<nint, @string>, map<nint, @string>, nint, @string>(m1, m2));
    fmt.Println(maps.Equal<map<nint, @string>, map<nint, @string>, nint, @string>(m1, m3));
}

// Output:
// true
// false
public static void ExampleEqualFunc() {
    var m1 = new map<nint, @string>{
        [1] = "one"u8,
        [10] = "Ten"u8,
        [1000] = "THOUSAND"u8
    };
    var m2 = new map<nint, slice<byte>>{
        [1] = slice<byte>("One"u8),
        [10] = slice<byte>("Ten"u8),
        [1000] = slice<byte>("Thousand"u8)
    };
    var eq = maps.EqualFunc<map<nint, @string>, map<nint, slice<byte>>, nint, @string, slice<byte>>(m1, m2, (@string v1, slice<byte> v2) => strings.ToLower(v1) == strings.ToLower(((@string)v2)));
    fmt.Println(eq);
}

// Output:
// true

} // end maps_test_package
