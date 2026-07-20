// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using hex = go.encoding.hex_package;
using fmt = fmt_package;
using log = log_package;
using os = os_package;
using go.encoding;
using io = io_package;

partial class hex_test_package {

public static void ExampleEncode() {
    var src = slice<byte>("Hello Gopher!"u8);
    var dst = new slice<byte>(hex.EncodedLen(len(src)));
    hex.Encode(dst, src);
    fmt.Printf("%s\n"u8, dst);
}

// Output:
// 48656c6c6f20476f7068657221
public static void ExampleDecode() {
    var src = slice<byte>("48656c6c6f20476f7068657221"u8);
    var dst = new slice<byte>(hex.DecodedLen(len(src)));
    var (n, err) = hex.Decode(dst, src);
    if (err != default!) {
        log.Fatal(err);
    }
    fmt.Printf("%s\n"u8, dst[..(int)(n)]);
}

// Output:
// Hello Gopher!
public static void ExampleDecodeString() {
    @string s = "48656c6c6f20476f7068657221"u8;
    var (decoded, err) = hex.DecodeString(s);
    if (err != default!) {
        log.Fatal(err);
    }
    fmt.Printf("%s\n"u8, decoded);
}

// Output:
// Hello Gopher!
public static void ExampleDump() {
    var content = slice<byte>("Go is an open source programming language."u8);
    fmt.Printf("%s"u8, hex.Dump(content));
}

// Output:
// 00000000  47 6f 20 69 73 20 61 6e  20 6f 70 65 6e 20 73 6f  |Go is an open so|
// 00000010  75 72 63 65 20 70 72 6f  67 72 61 6d 6d 69 6e 67  |urce programming|
// 00000020  20 6c 61 6e 67 75 61 67  65 2e                    | language.|
public static void ExampleDumper() => func((defer, recover) => {
    var lines = new @string[]{
        "Go is an open source programming language.",
        "\n",
        "We encourage all Go users to subscribe to golang-announce."
    }.slice();
    var stdoutDumper = hex.Dumper(new os.FileжWriter(os.Stdout));
    var stdoutDumperʗ1 = stdoutDumper;
    defer(() => stdoutDumperʗ1.Close());
    foreach (var (_, line) in lines) {
        stdoutDumper.Write(slice<byte>(line));
    }
});

// Output:
// 00000000  47 6f 20 69 73 20 61 6e  20 6f 70 65 6e 20 73 6f  |Go is an open so|
// 00000010  75 72 63 65 20 70 72 6f  67 72 61 6d 6d 69 6e 67  |urce programming|
// 00000020  20 6c 61 6e 67 75 61 67  65 2e 0a 57 65 20 65 6e  | language..We en|
// 00000030  63 6f 75 72 61 67 65 20  61 6c 6c 20 47 6f 20 75  |courage all Go u|
// 00000040  73 65 72 73 20 74 6f 20  73 75 62 73 63 72 69 62  |sers to subscrib|
// 00000050  65 20 74 6f 20 67 6f 6c  61 6e 67 2d 61 6e 6e 6f  |e to golang-anno|
// 00000060  75 6e 63 65 2e                                    |unce.|
public static void ExampleEncodeToString() {
    var src = slice<byte>("Hello"u8);
    @string encodedStr = hex.EncodeToString(src);
    fmt.Printf("%s\n"u8, encodedStr);
}

// Output:
// 48656c6c6f

} // end hex_test_package
