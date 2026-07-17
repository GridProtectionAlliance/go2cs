// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.unicode;

using fmt = fmt_package;
using utf8 = go.unicode.utf8_package;
using go.unicode;

partial class utf8_test_package {

public static void ExampleDecodeLastRune() {
    var b = slice<byte>("Hello, 世界"u8);
    while (len(b) > 0) {
        var (r, size) = utf8.DecodeLastRune(b);
        fmt.Printf("%c %v\n"u8, r, size);
        b = b[..(int)(len(b) - size)];
    }
}

// Output:
// 界 3
// 世 3
//   1
// , 1
// o 1
// l 1
// l 1
// e 1
// H 1
public static void ExampleDecodeLastRuneInString() {
    @string str = "Hello, 世界"u8;
    while (len(str) > 0) {
        var (r, size) = utf8.DecodeLastRuneInString(str);
        fmt.Printf("%c %v\n"u8, r, size);
        str = str[..(int)(len(str) - size)];
    }
}

// Output:
// 界 3
// 世 3
//   1
// , 1
// o 1
// l 1
// l 1
// e 1
// H 1
public static void ExampleDecodeRune() {
    var b = slice<byte>("Hello, 世界"u8);
    while (len(b) > 0) {
        var (r, size) = utf8.DecodeRune(b);
        fmt.Printf("%c %v\n"u8, r, size);
        b = b[(int)(size)..];
    }
}

// Output:
// H 1
// e 1
// l 1
// l 1
// o 1
// , 1
//   1
// 世 3
// 界 3
public static void ExampleDecodeRuneInString() {
    @string str = "Hello, 世界"u8;
    while (len(str) > 0) {
        var (r, size) = utf8.DecodeRuneInString(str);
        fmt.Printf("%c %v\n"u8, r, size);
        str = str[(int)(size)..];
    }
}

// Output:
// H 1
// e 1
// l 1
// l 1
// o 1
// , 1
//   1
// 世 3
// 界 3
public static void ExampleEncodeRune() {
    var r = (rune)'世';
    var buf = new slice<byte>(3);
    nint n = utf8.EncodeRune(buf, r);
    fmt.Println(buf);
    fmt.Println(n);
}

// Output:
// [228 184 150]
// 3
public static void ExampleEncodeRune_outOfRange() {
    var runes = new rune[]{ // Less than 0, out of range.

        -1, // Greater than 0x10FFFF, out of range.

        0x110000, // The Unicode replacement character.

        utf8.RuneError
    }.slice();
    foreach (var (i, c) in runes) {
        var buf = new slice<byte>(3);
        nint size = utf8.EncodeRune(buf, c);
        fmt.Printf("%d: %d %[2]s %d\n"u8, i, buf, size);
    }
}

// Output:
// 0: [239 191 189] � 3
// 1: [239 191 189] � 3
// 2: [239 191 189] � 3
public static void ExampleFullRune() {
    var buf = new byte[]{228, 184, 150}.slice();
    // 世
    fmt.Println(utf8.FullRune(buf));
    fmt.Println(utf8.FullRune(buf[..2]));
}

// Output:
// true
// false
public static void ExampleFullRuneInString() {
    @string str = "世"u8;
    fmt.Println(utf8.FullRuneInString(str));
    fmt.Println(utf8.FullRuneInString(str[..2]));
}

// Output:
// true
// false
public static void ExampleRuneCount() {
    var buf = slice<byte>("Hello, 世界"u8);
    fmt.Println("bytes =", len(buf));
    fmt.Println("runes =", utf8.RuneCount(buf));
}

// Output:
// bytes = 13
// runes = 9
public static void ExampleRuneCountInString() {
    @string str = "Hello, 世界"u8;
    fmt.Println("bytes =", len(str));
    fmt.Println("runes =", utf8.RuneCountInString(str));
}

// Output:
// bytes = 13
// runes = 9
public static void ExampleRuneLen() {
    fmt.Println(utf8.RuneLen((rune)'a'));
    fmt.Println(utf8.RuneLen((rune)'界'));
}

// Output:
// 1
// 3
public static void ExampleRuneStart() {
    var buf = slice<byte>("a界"u8);
    fmt.Println(utf8.RuneStart(buf[0]));
    fmt.Println(utf8.RuneStart(buf[1]));
    fmt.Println(utf8.RuneStart(buf[2]));
}

// Output:
// true
// true
// false
public static void ExampleValid() {
    var valid = slice<byte>("Hello, 世界"u8);
    var invalid = new byte[]{0xff, 0xfe, 0xfd}.slice();
    fmt.Println(utf8.Valid(valid));
    fmt.Println(utf8.Valid(invalid));
}

// Output:
// true
// false
public static void ExampleValidRune() {
    var valid = (rune)'a';
    var invalid = (rune)0xfffffff;
    fmt.Println(utf8.ValidRune(valid));
    fmt.Println(utf8.ValidRune(invalid));
}

// Output:
// true
// false
public static void ExampleValidString() {
    @string valid = "Hello, 世界"u8;
    @string invalid = ((@string)new byte[]{0xff, 0xfe, 0xfd}.slice());
    fmt.Println(utf8.ValidString(valid));
    fmt.Println(utf8.ValidString(invalid));
}

// Output:
// true
// false
public static void ExampleAppendRune() {
    var buf1 = utf8.AppendRune(default!, 0x10000);
    var buf2 = utf8.AppendRune(slice<byte>("init"u8), 0x10000);
    fmt.Println(((@string)buf1));
    fmt.Println(((@string)buf2));
}

// Output:
// 𐀀
// init𐀀

} // end utf8_test_package
